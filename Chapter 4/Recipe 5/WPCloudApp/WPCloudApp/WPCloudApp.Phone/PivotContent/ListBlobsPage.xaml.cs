namespace WPCloudApp.Phone.PivotContent
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using Microsoft.Samples.WindowsPhoneCloud.StorageClient;
    using WPCloudApp.Phone.Helpers;
    using WPCloudApp.Phone.ViewModel;

    public partial class ListBlobsPage : UserControl
    {
        public ListBlobsPage()
        {
            this.InitializeComponent();
            PhoneHelpers.SetApplicationState("LoadContainers", true);

            this.ViewModel = new ListBlobsPageViewModel();
            this.Loaded += this.OnListBlobsPageLoaded;
        }

        public event EventHandler<EventArgs> TakePhoto
        {
            add { this.ViewModel.TakePhoto += value; }
            remove { this.ViewModel.TakePhoto -= value; }
        }

        public ListBlobsPageViewModel ViewModel
        {
            get { return this.DataContext as ListBlobsPageViewModel; }
            set { this.DataContext = value; }
        }

        private void OnListBlobs(object sender, RoutedEventArgs e)
        {
            this.ViewModel.ListBlobs();
        }

        private void OnDeleteBlob(object sender, RoutedEventArgs e)
        {
            var blob = ((Button)sender).Tag as ICloudBlob;
            if (blob != null)
            {
                this.ViewModel.DeleteBlob(blob);
            }
        }

        private void OnListBlobsPageLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (PhoneHelpers.GetApplicationState<bool>("LoadContainers"))
            {
                this.ViewModel.ListContainers();
                PhoneHelpers.SetApplicationState("LoadContainers", false);
            }
        }
    }
}