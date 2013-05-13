using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Net;
using System.Data.Services.Client;
using System.Collections.Specialized;
using System.Web.Script.Serialization;
using System.Globalization;
using ACSManagementServiceClient.AccessControlService;
using System.Configuration;

namespace ACSManagementServiceClient
{
    class Program
    {
        public const string serviceIdentityUsernameForManagement = "ManagementClient";
        public const string serviceIdentityPasswordForManagement = "--your password here-";
        public const string serviceNamespace = "--your namespace--";
        public const string acsHostName = "accesscontrol.windows.net";
        public const string acsManagementServicesRelativeUrl = "v2/mgmt/service/";
        static string cachedSwtToken;

        static void Main(string[] args)
        {
            CreateRelyingPartyApplication(CreateManagementServiceClient());
        }

        public static void CreateRelyingPartyApplication(ManagementService svc)
        {
            var relyingParty = new RelyingParty()
            {
                Name = "MyProgrammaticRelyingPartyApplication",
                AsymmetricTokenEncryptionRequired = false,
                TokenType = "SAML_2_0",
                TokenLifetime = 3600
            };
            svc.AddToRelyingParties(relyingParty);

            //Create the Realm Address
            var realmAddress = new RelyingPartyAddress()
            {
                Address = "http://ProgrammaticRelyingPartyApplication.com/Realm",
                EndpointType = "Realm"
            };
            svc.AddRelatedObject(relyingParty, "RelyingPartyAddresses", realmAddress);

            //Create the Return URL Address

            var replyAddress = new RelyingPartyAddress()
            {
                Address = "http://ProgrammaticRelyingPartyApplication.com/Reply",
                EndpointType = "Reply"
            };
            svc.AddRelatedObject(relyingParty, "RelyingPartyAddresses", replyAddress);

            // Create a Rule Group for This Relying Party Application
            var rg = new RuleGroup();
            rg.Name = "SampleRuleGroup For " + relyingParty.Name;
            svc.AddToRuleGroups(rg);

            // Assign This New Rule Group to Your New Relying Party Application
            var relyingPartyRuleGroup = new RelyingPartyRuleGroup();
            svc.AddToRelyingPartyRuleGroups(relyingPartyRuleGroup);
            svc.AddLink(relyingParty, "RelyingPartyRuleGroups", relyingPartyRuleGroup);
            svc.AddLink(rg, "RelyingPartyRuleGroups", relyingPartyRuleGroup);
            
            //Save Your New Relying Party Application
            svc.SaveChanges(SaveChangesOptions.Batch);
        }

        public static ManagementService CreateManagementServiceClient()
        {
            string managementServiceEndpoint = String.Format(CultureInfo.InvariantCulture, "https://{0}.{1}/{2}",
                serviceNamespace,
                acsHostName,
                acsManagementServicesRelativeUrl);
            ManagementService managementService = new ManagementService(new Uri(managementServiceEndpoint));

            managementService.SendingRequest += GetTokenWithWritePermission;

            return managementService;
        }

        public static void GetTokenWithWritePermission(object sender, SendingRequestEventArgs args)
        {
            GetTokenWithWritePermission((HttpWebRequest)args.Request);
        }

        public static void GetTokenWithWritePermission(HttpWebRequest args)
        {
            if (cachedSwtToken == null)
            {
                cachedSwtToken = GetTokenFromACS();
            }
            args.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + cachedSwtToken);
        }

        private static string GetTokenFromACS()
        {
            //
            // Request a token from ACS
            //
            var client = new WebClient();
            client.BaseAddress = string.Format(CultureInfo.CurrentCulture,
                                               "https://{0}.{1}",
                                               serviceNamespace,
                                               acsHostName);

            var values = new NameValueCollection();
            values.Add("grant_type", "client_credentials");
            values.Add("client_id", serviceIdentityUsernameForManagement);
            values.Add("client_secret", serviceIdentityPasswordForManagement);
            values.Add("scope", client.BaseAddress + acsManagementServicesRelativeUrl);

            byte[] responseBytes = client.UploadValues("/v2/OAuth2-13", "POST", values);
            string response = Encoding.UTF8.GetString(responseBytes);

            // Parse the JSON response and return the access token 
            var serializer = new JavaScriptSerializer();
            Dictionary<string, object> decodedDictionary = serializer.DeserializeObject(response) as Dictionary<string, object>;
            return decodedDictionary["access_token"] as string;
        }

    }
}
