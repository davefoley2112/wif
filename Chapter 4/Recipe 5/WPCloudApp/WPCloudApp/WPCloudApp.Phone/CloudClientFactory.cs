namespace WPCloudApp.Phone
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Threading;
    using Microsoft.Samples.Data.Services.Client;
    using Microsoft.Samples.WindowsPhoneCloud.StorageClient;
    using Microsoft.Samples.WindowsPhoneCloud.StorageClient.Credentials;
    using WPCloudApp.Phone.Push;
    using WPCloudApp.Phone.Helpers;

    public class CloudClientFactory : ICloudClientFactory
    {
        private const string NameIdentifierClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";

        private readonly IDictionary<object, object> appResources;
        private readonly Dispatcher dispatcher;

        private readonly IDictionary<string, ITableServiceContext> tableServiceContextDictionary = new Dictionary<string, ITableServiceContext>();
        private readonly IDictionary<string, ICloudTableClient> cloudTableClientDictionary = new Dictionary<string, ICloudTableClient>();
        private readonly IDictionary<string, ICloudBlobClient> cloudBlobClientDictionary = new Dictionary<string, ICloudBlobClient>();
        private readonly IDictionary<string, ICloudQueueClient> cloudQueueClientDictionary = new Dictionary<string, ICloudQueueClient>();

        public CloudClientFactory()
            : this(Application.Current.Resources, Deployment.Current.Dispatcher)
        {
        }

        public CloudClientFactory(IDictionary<object, object> appResources, Dispatcher dispatcher)
        {
            this.appResources = appResources;
            this.dispatcher = dispatcher;

            // Initialize the PushContext.Current instance.
            if (PushContext.Current == null)
            {
                new PushContext(
                    this.appResources["PushChannelName"].ToString(),
                    this.appResources["PushServiceName"].ToString(),
                    new[] { new Uri("https://127.0.0.1") },
                    this.dispatcher);
            }
        }

        public SL.Phone.Federation.Utilities.RequestSecurityTokenResponseStore TokenStore
        {
            get
            {
                return this.appResources["rstrStore"] as SL.Phone.Federation.Utilities.RequestSecurityTokenResponseStore;
            }
        }

        public string UserIdentifier
        {
            get
            {
                var items = PhoneHelpers.ParseQueryString(this.TokenStore.SecurityToken);
                return items[System.Net.HttpUtility.UrlEncode(NameIdentifierClaimType)];
            }
        }

        public ITableServiceContext ResolveTablesServiceContext(string key = "")
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return new TableServiceContext(this.appResources["AzureStorageTableProxyEndpoint"].ToString(), this.ResolveStorageCredentials());
            }

            if (!this.tableServiceContextDictionary.ContainsKey(key))
            {
                this.tableServiceContextDictionary.Add(key, new TableServiceContext(this.appResources["AzureStorageTableProxyEndpoint"].ToString(), this.ResolveStorageCredentials()));
            }

            return this.tableServiceContextDictionary[key];
        }

        public DataServiceCollection<T> ResolveDataServiceCollection<T>(string key = "")
        {
            return new DataServiceCollection<T>(this.ResolveTablesServiceContext(key) as DataServiceContext);
        }

        public ICloudTableClient ResolveCloudTableClient(string key = "")
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return new CloudTableClient(this.ResolveTablesServiceContext(key));
            }

            if (!this.cloudTableClientDictionary.ContainsKey(key))
            {
                this.cloudTableClientDictionary.Add(key, new CloudTableClient(this.ResolveTablesServiceContext(key)));
            }

            return this.cloudTableClientDictionary[key];
        }

        public ICloudBlobClient ResolveCloudBlobClient(string key = "")
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return this.CreateCloudBlobClient();
            }

            if (!this.cloudBlobClientDictionary.ContainsKey(key))
            {
                this.cloudBlobClientDictionary.Add(key, this.CreateCloudBlobClient());
            }

            return this.cloudBlobClientDictionary[key];
        }

        public ICloudQueueClient ResolveCloudQueueClient(string key = "")
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return this.CreateCloudQueueClient();
            }

            if (!this.cloudQueueClientDictionary.ContainsKey(key))
            {
                this.cloudQueueClientDictionary.Add(key, this.CreateCloudQueueClient());
            }

            return this.cloudQueueClientDictionary[key];
        }

        public ISamplePushUserRegistrationClient ResolvePushNotificationClient()
        {
            return new SamplePushUserRegistrationClient(new Uri(this.appResources["PushNotificationServiceEndpoint"].ToString()), this.ResolveStorageCredentials(), this.appResources["ApplicationId"].ToString());
        }


        public IStorageCredentials ResolveStorageCredentials()
        {
            return new StorageCredentialsSwtToken(
                this.TokenStore.SecurityToken,
                this.TokenStore.RequestSecurityTokenResponse != null ? this.TokenStore.RequestSecurityTokenResponse.expires : 0);
        }

        public IRegistrationClient ResolveRegistrationClient()
        {
            return new RegistrationClient(this.appResources["RegistrationServiceEndpoint"].ToString(), this.ResolveStorageCredentials());
        }

        public void CleanTablesClientsKey(string key)
        {
            if (!string.IsNullOrWhiteSpace(key))
            {
                if (this.tableServiceContextDictionary.ContainsKey(key))
                {
                    this.tableServiceContextDictionary.Remove(key);
                }

                if (this.cloudTableClientDictionary.ContainsKey(key))
                {
                    this.cloudTableClientDictionary.Remove(key);
                }
            }
        }

        public void CleanAuthenticationToken()
        {
            // Remove the authentication token from the phone state dictionary.
            if (this.TokenStore.ContainsValidRequestSecurityTokenResponse())
            {
                this.TokenStore.RequestSecurityTokenResponse = null;
            }

            // Clear the current state of the factory.
            this.tableServiceContextDictionary.Clear();
            this.cloudTableClientDictionary.Clear();
            this.cloudBlobClientDictionary.Clear();
            this.cloudQueueClientDictionary.Clear();
        }

        private ICloudBlobClient CreateCloudBlobClient()
        {
            // To use the Shared Access Signature to access blobs and containers.
            return new CloudBlobClient(
                new SharedAccessSignatureServiceClient(
                    this.appResources["SharedAccessSignatureServiceEndpoint"].ToString(),
                    this.ResolveStorageCredentials()));
        }

        private ICloudQueueClient CreateCloudQueueClient()
        {
            return new CloudQueueClient(this.appResources["AzureStorageQueueProxyEndpoint"].ToString(), this.ResolveStorageCredentials());
        }
    }
}
