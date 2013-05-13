﻿namespace WPCloudApp.Phone.Pages
{
    using System;
    using System.Windows;
    using System.Windows.Navigation;
    using Microsoft.Phone.Controls;
    using SL.Phone.Federation.Utilities;
    using WPCloudApp.Phone.Helpers;
    using WPCloudApp.Phone.ViewModel;

    public partial class LoginPage : PhoneApplicationPage
    {
        private readonly RequestSecurityTokenResponseStore rstrStore = App.Current.Resources["rstrStore"] as RequestSecurityTokenResponseStore;

        public LoginPage()
        {
            this.InitializeComponent();

            this.PageTransitionReset.Begin();

            this.SignInControl.RequestSecurityTokenResponseCompleted +=
                (s, e) => this.ViewModel.HandleRequestSecurityTokenResponseCompleted(
                    s,
                    e,
                    () =>
                    {
                        PhoneHelpers.SetIsolatedStorageSetting("UserIsRegistered", true);
                        this.NavigationService.Navigate(new Uri("/Pages/MainPage.xaml", UriKind.Relative));
                    },
                    () =>
                    {
                        PhoneHelpers.SetIsolatedStorageSetting("UserIsRegistered", false);
                        this.NavigationService.Navigate(new Uri("/Pages/RegisterPage.xaml", UriKind.Relative));
                    },
                    errorMessage =>
                    {
                        this.SignInControl.GetSecurityToken();

                        errorMessage = string.IsNullOrWhiteSpace(errorMessage)
                            ? "An error occurred determining if the user was already registered. Make sure that the appropriate SSL certificate is installed."
                            : errorMessage;
                        MessageBox.Show(errorMessage, "Registration Error", MessageBoxButton.OK);
                    });
            this.SignInControl.NavigatingToIdentityProvider +=
                (s, e) =>
                {
                    // When the user navigates to an identity provider, the previous login information is cleaned up.
                    PhoneHelpers.SetIsolatedStorageSetting("UserIsRegistered", false);
                    App.CloudClientFactory.CleanAuthenticationToken();
                };
        }

        public LoginPageViewModel ViewModel
        {
            get { return this.DataContext as LoginPageViewModel; }
            set { this.DataContext = value; }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (this.rstrStore.IsValid() && PhoneHelpers.GetIsolatedStorageSetting<bool>("UserIsRegistered") && !PhoneHelpers.GetApplicationState<bool>("UserBackPress"))
            {
                this.NavigationService.Navigate(new Uri("/Pages/MainPage.xaml", UriKind.Relative));
            }
            else
            {
                this.StartTransition();

                PhoneHelpers.SetApplicationState("UserBackPress", false);

                this.SignInControl.GetSecurityToken();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.PageTransitionReset.Begin();
        }

        private void StartTransition()
        {
            this.PageTransitionIn.Begin();
            if (this.ViewModel == null)
            {
                this.ViewModel = new LoginPageViewModel();
            }
        }
    }
}