namespace WPCloudApp.Phone
{
    using Microsoft.Samples.Data.Services.Client;
    using Microsoft.Samples.WindowsPhoneCloud.StorageClient;
    using Microsoft.Samples.WindowsPhoneCloud.StorageClient.Credentials;
    using WPCloudApp.Phone.Push;

    public interface ICloudClientFactory
    {
        string UserIdentifier { get; }

        SL.Phone.Federation.Utilities.RequestSecurityTokenResponseStore TokenStore { get; }

        IRegistrationClient ResolveRegistrationClient();


        ITableServiceContext ResolveTablesServiceContext(string key = "");

        DataServiceCollection<T> ResolveDataServiceCollection<T>(string key = "");

        ICloudTableClient ResolveCloudTableClient(string key = "");

        ICloudBlobClient ResolveCloudBlobClient(string key = "");

        ICloudQueueClient ResolveCloudQueueClient(string key = "");

        ISamplePushUserRegistrationClient ResolvePushNotificationClient();

        void CleanTablesClientsKey(string key);

        void CleanAuthenticationToken();
    }
}
