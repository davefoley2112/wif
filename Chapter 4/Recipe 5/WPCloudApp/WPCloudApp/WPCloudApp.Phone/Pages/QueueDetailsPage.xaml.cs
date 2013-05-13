namespace WPCloudApp.Phone.Pages
{
    using System;
    using System.Windows;
    using System.Windows.Navigation;
    using Microsoft.Phone.Controls;
    using Microsoft.Samples.WindowsPhoneCloud.StorageClient;
    using WPCloudApp.Phone.ViewModel;
    using WPCloudApp.Phone.Helpers;

    public partial class QueueDetailsPage : PhoneApplicationPage
    {
        public QueueDetailsPage()
        {
            this.InitializeComponent();

            this.ViewModel = new QueueDetailsPageViewModel();
        }

        public QueueDetailsPageViewModel ViewModel
        {
            get { return this.DataContext as QueueDetailsPageViewModel; }
            set { this.DataContext = value; }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.ViewModel.Queue = PhoneHelpers.GetApplicationState<ICloudQueue>("CurrentQueue");
            if (this.ViewModel.Queue == null)
            {
                // If there is not any queue in the application state, then go back.
                this.NavigationService.GoBack();
            }
            else
            {
                PhoneHelpers.RemoveApplicationState("CurrentQueue");
            }
        }

        private void OnQueueMessage(object sender, RoutedEventArgs e)
        {
            if (this.ViewModel != null)
            {
                if (string.IsNullOrWhiteSpace(this.ViewModel.QueueMessageContent))
                {
                    MessageBox.Show("The queue message cannot be null nor empty.", "Queue Message Result", MessageBoxButton.OK);
                }
                else
                {
                    this.ViewModel.QueueMessage();
                }
            }
        }

        private void OnDequeueMessage(object sender, RoutedEventArgs e)
        {
            if (this.ViewModel != null)
            {
                this.ViewModel.DequeueMessage();
            }
        }

        private void OnDeleteQueueMessages(object sender, EventArgs e)
        {
            if (this.ViewModel != null)
            {
                this.ViewModel.CloudQueueMessages.Clear();
            }
        }
    }
}