using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Security.Authentication.Web;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace MetroAcsApplication
{
    partial class MainPage
    {
        private const string ServiceNamespace = "Your Namespace";
        private const string Realm = "Your RP";
        private string loginUrl = "https://open.login.yahooapis.com/openid/op/auth"; //to be updated
        public MainPage()
        {
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            Task getLoginUrl = GetIdentityProviderLoginUrlAsync("Yahoo");
            txtStatus.Text = "Loading Provider...";
            await getLoginUrl;
            Task authenticate = GetAuthenticationResultAsync(loginUrl);
            txtStatus.Text = "Authenticating..";
            await authenticate;
        }

        private async Task GetAuthenticationResultAsync(string ipURL)
        {
            try
            {
                var result = await WebAuthenticationBroker.AuthenticateAsync(
                    WebAuthenticationOptions.Default,
                    new Uri(ipURL));
                txtStatus.Text = (result.ResponseStatus == 0) ? "Success" : "Failure";
            }
            catch (Exception e)
            {
                txtStatus.Text = e.Message;
            }
        }

        private async Task GetIdentityProviderLoginUrlAsync(string provider)
        {
            string metadataUrl = string.Format("https://{0}.accesscontrol.windows.net/v2/metadata/IdentityProviders.js?protocol=wsfederation&realm={1}version=1.0",
                ServiceNamespace,
                Realm);
            HttpClient client = new HttpClient();
            var jsonContent = await client.GetAsync(metadataUrl);
            //TODO: Parse jsonContent and assign to loginUrl
        }
    }
}
