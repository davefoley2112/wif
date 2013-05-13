namespace WPCloudApp.Phone.PivotContent
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Navigation;
    using Microsoft.Samples.WindowsPhoneCloud.StorageClient;
    using WPCloudApp.Phone.Helpers;
    using WPCloudApp.Phone.ViewModel;

    public partial class ListQueuesPage : UserControl
    {
        public ListQueuesPage()
        {
            this.InitializeComponent();

            this.Loaded += this.OnListQueues;
        }

        public event EventHandler<NavigationEventArgs> Navigate
        {
            add { this.ViewModel.Navigate += value; }
            remove { this.ViewModel.Navigate -= value; }
        }

        public ListQueuesPageViewModel ViewModel
        {
            get { return this.DataContext as ListQueuesPageViewModel; }
            set { this.DataContext = value; }
        }

        private void OnListQueues(object sender, RoutedEventArgs e)
        {
            if (this.ViewModel != null)
            {
                this.ViewModel.ListQueues();
            }
        }

        private void OnDeleteQueue(object sender, EventArgs e)
        {
            var queue = ((Button)sender).Tag as ICloudQueue;
            if ((this.ViewModel != null) && (queue != null))
            {
                this.ViewModel.DeleteQueue(queue);
            }
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.ViewModel != null)
            {
                var selector = sender as Selector;
                if ((selector == null) || (selector.SelectedIndex == -1))
                {
                    return;
                }

                PhoneHelpers.SetApplicationState("CurrentQueue", this.ViewModel.CloudQueues[selector.SelectedIndex]);
                this.ViewModel.NavigateTo(new Uri("/Pages/QueueDetailsPage.xaml", UriKind.Relative));
                selector.SelectedIndex = -1;
            }
        }
    }
}