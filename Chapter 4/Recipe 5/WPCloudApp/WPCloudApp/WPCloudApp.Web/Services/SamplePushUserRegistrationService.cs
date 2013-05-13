﻿namespace WPCloudApp.Web.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Web;
    using System.Web;
    using System.Web.Security;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.StorageClient;
    using WindowsPhone.Recipes.Push.Messages;
    using WPCloudApp.Web.Infrastructure;
    using WPCloudApp.Web.Models;
    using WPCloudApp.Web.UserAccountWrappers;

    [ServiceBehavior(IncludeExceptionDetailInFaults = false)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class SamplePushUserRegistrationService : ISamplePushUserRegistrationService
    {
        private readonly HttpContextBase context;
        private readonly IPushUserEndpointsRepository pushUserEndpointsRepository;
        private readonly CloudQueueClient cloudQueueClient;
        private readonly WebOperationContext webOperationContext;

        public SamplePushUserRegistrationService()
            : this(new HttpContextWrapper(HttpContext.Current), WebOperationContext.Current, new UserTablesServiceContext(), null)
        {
        }

        [CLSCompliant(false)]
        public SamplePushUserRegistrationService(HttpContextBase context, WebOperationContext webOperationContext, IPushUserEndpointsRepository pushUserEndpointsRepository, CloudQueueClient cloudQueueClient)
        {
            if ((context == null) && (HttpContext.Current == null))
            {
                throw new ArgumentNullException("context", "Context cannot be null if not running on a Web context.");
            }

            if (pushUserEndpointsRepository == null)
            {
                throw new ArgumentNullException("pushUserEndpointsRepository", "PushUserEndpoints repository cannot be null.");
            }

            if ((cloudQueueClient == null) && (GetStorageAccountFromConfigurationSetting() == null))
            {
                throw new ArgumentNullException("cloudQueueClient", "Cloud Queue Client cannot be null if no configuration is loaded.");
            }

            this.cloudQueueClient = cloudQueueClient ?? GetStorageAccountFromConfigurationSetting().CreateCloudQueueClient();
            this.webOperationContext = webOperationContext;
            this.context = context;
            this.pushUserEndpointsRepository = pushUserEndpointsRepository;
        }

        private string UserId
        {
            get
            {
                var identity = HttpContext.Current.User.Identity as Microsoft.IdentityModel.Claims.IClaimsIdentity;
                return identity.Claims.Single(c => c.ClaimType == Microsoft.IdentityModel.Claims.ClaimTypes.NameIdentifier).Value;
            }
        }

        public string Register(PushUserServiceRequest pushUserRegister)
        {
            // Authenticate.
            var userId = this.UserId;

            try
            {
                var pushUserEndpoint = this.pushUserEndpointsRepository.GetPushUserByApplicationAndDevice(pushUserRegister.ApplicationId, pushUserRegister.DeviceId);
                if (pushUserEndpoint == null)
                {
                    var newPushUserEndPoint = new PushUserEndpoint(pushUserRegister.ApplicationId, pushUserRegister.DeviceId) { ChannelUri = pushUserRegister.ChannelUri.ToString(), UserId = userId };
                    this.pushUserEndpointsRepository.AddPushUserEndpoint(newPushUserEndPoint);
                }
                else
                {
                    // If the user did not change the channel URI, then, there is nothing to update, otherwise, set the new connection status
                    if (!pushUserEndpoint.ChannelUri.Equals(pushUserRegister.ChannelUri.ToString()) ||
                        !pushUserEndpoint.UserId.Equals(userId))
                    {
                        // Update che channelUri for the UserEndpoint and reset status fields.
                        pushUserEndpoint.ChannelUri = pushUserRegister.ChannelUri.ToString();
                        pushUserEndpoint.UserId = userId;
                    }

                    this.pushUserEndpointsRepository.UpdatePushUserEndpoint(pushUserEndpoint);
                }
            }
            catch (Exception exception)
            {
                throw new WebFaultException<string>(
                    string.Format(CultureInfo.InvariantCulture, "There was an error registering the Push Notification Endpoint: {0}", exception.Message),
                    HttpStatusCode.InternalServerError);
            }

            return "Success";
        }

        public string Unregister(PushUserServiceRequest pushUserUnregister)
        {
            // Authenticate.
            var userId = this.UserId;

            try
            {
                this.pushUserEndpointsRepository.RemovePushUserEndpoint(new PushUserEndpoint(pushUserUnregister.ApplicationId, pushUserUnregister.DeviceId) { UserId = userId });
            }
            catch (Exception exception)
            {
                throw new WebFaultException<string>(
                    string.Format(CultureInfo.InvariantCulture, "There was an error unregistering the Push Notification Endpoint: {0}", exception.Message),
                    HttpStatusCode.InternalServerError);
            }

            return "Success";
        }

        public string[] GetUpdates(string applicationId, string deviceId)
        {
            // Authenticate.
            var userId = this.UserId;

            this.webOperationContext.OutgoingResponse.Headers.Add("Cache-Control", "no-cache");
            var userEndpoint = this.pushUserEndpointsRepository.GetPushUserByApplicationAndDevice(applicationId, deviceId);
            try
            {
                var queueName = GetQueueName(applicationId, deviceId, userEndpoint.UserId);
                var queue = this.cloudQueueClient.GetQueueReference(queueName);
                var messages = new List<string>();
                if (queue.Exists())
                {
                    var message = queue.GetMessage();
                    while (message != null)
                    {
                        messages.Add(message.AsString);
                        queue.DeleteMessage(message);
                        message = queue.GetMessage();
                    }
                }

                this.ResetTileNotificationCount(applicationId, deviceId);

                return messages.ToArray();
            }
            catch (Exception exception)
            {
                throw new WebFaultException<string>(string.Format(CultureInfo.InvariantCulture, "There was an error getting the push notification updates: {0}", exception.Message), HttpStatusCode.InternalServerError);
            }
        }

        private static string GetQueueName(string applicationId, string deviceId, string userId)
        {
            var uniqueName = string.Concat(applicationId, deviceId, userId);

            return string.Concat("notification", uniqueName.GetHashCode());
        }

        private static CloudStorageAccount GetStorageAccountFromConfigurationSetting()
        {
            CloudStorageAccount account = null;

            try
            {
                account = CloudStorageAccount.FromConfigurationSetting("DataConnectionString");
            }
            catch (InvalidOperationException)
            {
                account = null;
            }

            return account;
        }

        private void ResetTileNotificationCount(string applicationId, string deviceId)
        {
            var pushUserEndpoint = this.pushUserEndpointsRepository.GetPushUserByApplicationAndDevice(applicationId, deviceId);

            pushUserEndpoint.TileCount = 0;

            var tile = new TilePushNotificationMessage
            {
                SendPriority = MessageSendPriority.High,
                Count = pushUserEndpoint.TileCount
            };

            // Send a new tile notification message to reset the count in the phone application.
            tile.SendAndHandleErrors(new Uri(pushUserEndpoint.ChannelUri));

            // Save the updated count.
            this.pushUserEndpointsRepository.UpdatePushUserEndpoint(pushUserEndpoint);
        }
    }
}