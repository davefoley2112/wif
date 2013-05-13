namespace WPCloudApp.Web.Infrastructure
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.ServiceModel.Web;
    using System.Web;
    using WPCloudApp.Web.UserAccountWrappers;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.StorageClient;

    public class BlobContainerRequestValidator
    {
        private readonly IUserPrivilegesRepository userPrivilegesRepository;
        private readonly CloudBlobClient cloudBlobClient;


        public BlobContainerRequestValidator()
            : this(new UserTablesServiceContext(), null)
        {
        }

        [CLSCompliant(false)]
        public BlobContainerRequestValidator(IUserPrivilegesRepository userPrivilegesRepository, CloudBlobClient cloudBlobClient)
        {
            if ((cloudBlobClient == null) && (GetStorageAccountFromConfigurationSetting() == null))
            {
                throw new ArgumentNullException("cloudBlobClient", "The Cloud Blob Client cannot be null if no configuration is loaded.");
            }

            this.userPrivilegesRepository = userPrivilegesRepository;
            this.cloudBlobClient = cloudBlobClient ?? GetStorageAccountFromConfigurationSetting().CreateCloudBlobClient();
        }

        public string GetUserId(HttpContextBase context)
        {
            var identity = HttpContext.Current.User.Identity as Microsoft.IdentityModel.Claims.IClaimsIdentity;
            return identity.Claims.Single(c => c.ClaimType == Microsoft.IdentityModel.Claims.ClaimTypes.NameIdentifier).Value;
        }


        public bool OnValidateRequest(string userId)
        {
            return this.OnValidateRequest(userId, string.Empty);
        }

        public bool OnValidateRequest(string userId, string blobContainerName, bool isCreating = false)
        {
            if (!this.CanUseBlobContainers(userId))
            {
                throw new WebFaultException<string>("You have no permission to use blob containers.", HttpStatusCode.Unauthorized);
            }

            if (!this.CanUseBlobContainer(userId, blobContainerName, isCreating))
            {
                throw new WebFaultException<string>("You have no permission to use this blob container.", HttpStatusCode.Unauthorized);
            }

            return true;
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

        private bool CanUseBlobContainer(string userId, string blobContainerName, bool isCreating)
        {
            if (string.IsNullOrWhiteSpace(blobContainerName))
            {
                return true;
            }

            var publicBlobContainerPrivilege = string.Format(CultureInfo.InvariantCulture, "{0}{1}", blobContainerName, PrivilegeConstants.PublicBlobContainerPrivilegeSuffix);
            if (!this.userPrivilegesRepository.PublicPrivilegeExists(publicBlobContainerPrivilege))
            {
                var accessBlobContainerPrivilege = string.Format(CultureInfo.InvariantCulture, "{0}{1}", blobContainerName, PrivilegeConstants.BlobContainerPrivilegeSuffix);
                if (!this.userPrivilegesRepository.HasUserPrivilege(userId, accessBlobContainerPrivilege))
                {
                    // Check if the user is creating a new blob container.
                    if (isCreating)
                    {
                        var container = this.cloudBlobClient.GetContainerReference(blobContainerName);
                        return !container.Exists();
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private bool CanUseBlobContainers(string userId)
        {
            return this.userPrivilegesRepository.HasUserPrivilege(userId, PrivilegeConstants.BlobContainersUsagePrivilege);
        }
    }
}