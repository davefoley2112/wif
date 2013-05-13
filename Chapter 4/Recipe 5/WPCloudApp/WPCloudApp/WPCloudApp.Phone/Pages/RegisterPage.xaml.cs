namespace WPCloudApp.Phone.Pages
{
    using System.Globalization;
    using System.Windows;
    using System.Windows.Input;
    using Microsoft.Phone.Controls;
    using WPCloudApp.Phone.Helpers;
    using WPCloudApp.Phone.ViewModel;

    public partial class RegisterPage : PhoneApplicationPage
    {
        private const string NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";
        private const string EmailClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress";

        public RegisterPage()
        {
            this.InitializeComponent();

            this.ViewModel = new RegisterPageViewModel();
            var items = PhoneHelpers.ParseQueryString(App.CloudClientFactory.TokenStore.SecurityToken);
            var claimsUserName = items[System.Net.HttpUtility.UrlEncode(NameClaimType)];
            var claimsEmail = items[System.Net.HttpUtility.UrlEncode(EmailClaimType)];
            this.ViewModel.UserName = string.IsNullOrWhiteSpace(claimsUserName) ? string.Empty : claimsUserName;
            this.ViewModel.EMail = string.IsNullOrWhiteSpace(claimsEmail) ? string.Empty : claimsEmail;
        }

        public RegisterPageViewModel ViewModel
        {
            get { return this.DataContext as RegisterPageViewModel; }
            set { this.DataContext = value; }
        }

        private void OnRegisterButtonClick(object sender, RoutedEventArgs e)
        {
            if (this.ViewModel.IsRegisterEnabled)
            {
                this.ViewModel.Register(
                    () =>
                    {
                        MessageBox.Show(
                            string.Format(CultureInfo.CurrentCulture, "User {0} successfully registered.", this.ViewModel.UserName),
                            "Registration Successful",
                            MessageBoxButton.OK);

                        PhoneHelpers.SetIsolatedStorageSetting("UserIsRegistered", true);
                        this.NavigationService.GoBack();
                    },
                    msg =>
                    {
                        MessageBox.Show(msg, "Registration Error", MessageBoxButton.OK);
                    });
            }
            else
            {
                MessageBox.Show("Please fill out all the information in the registration page.", "Registration Incomplete", MessageBoxButton.OK);
            }
        }

        private void OnTextBoxKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (sender == this.UserNameTextBox)
                {
                    this.EMailTextBox.Focus();
                }
                else
                {
                    this.RegisterButton.Focus();
                }
            }
        }
    }
}