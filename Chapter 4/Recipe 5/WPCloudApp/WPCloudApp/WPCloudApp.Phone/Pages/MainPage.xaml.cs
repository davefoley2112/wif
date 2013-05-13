namespace WPCloudApp.Phone.Pages
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Navigation;
    using Microsoft.Phone.Controls;
    using Microsoft.Phone.Shell;
    using Microsoft.Phone.Tasks;
    using WPCloudApp.Phone.Helpers;
    using WPCloudApp.Phone.PivotContent;
    using WPCloudApp.Phone.ViewModel;

    public partial class MainPage : PhoneApplicationPage
    {
        public MainPage()
        {
            this.InitializeComponent();

            this.ViewModel = new MainPageViewModel();
            this.LoadPivotItems();
            this.OnMainPivotSelectionChanged();

            this.MainPivot.SelectionChanged += (s, e) => this.OnMainPivotSelectionChanged();
        }

        public MainPageViewModel ViewModel
        {
            get { return this.DataContext as MainPageViewModel; }
            set { this.DataContext = value; }
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            PhoneHelpers.SetApplicationState("UserBackPress", true);
            base.OnBackKeyPress(e);
        }

        private void LoadPivotItems()
        {
            this.MainPivot.Items.Clear();

            var notificationsPage = new NotificationsPage();
            var notificationsPivot = new PivotItem();
            notificationsPivot.Header = "notifications";
            notificationsPivot.Name = "PushNotifications";
            notificationsPivot.Content = notificationsPage;
            notificationsPage.ViewModel = this.ViewModel.NotificationsViewModel;
            notificationsPage.BeginPushConnection += this.OnBeginPushConnection;
            notificationsPage.EndPushConnection += this.OnEndPushConnection;
            this.MainPivot.Items.Add(notificationsPivot);

            var tablesPage = new TablesPage();
            var tablesPivot = new PivotItem();
            tablesPivot.Header = "tables list";
            tablesPivot.Name = "TablesList";
            tablesPivot.Content = tablesPage;
            tablesPage.ViewModel = this.ViewModel.TablesPageViewModel;
            this.MainPivot.Items.Add(tablesPivot);

            var sampleDataTablesPage = new SampleDataTablePage();
            var sampleDataTablesPivot = new PivotItem();
            sampleDataTablesPivot.Header = "sample data";
            sampleDataTablesPivot.Name = "SampleDataRows";
            sampleDataTablesPivot.Content = sampleDataTablesPage;
            sampleDataTablesPage.ViewModel = this.ViewModel.SampleDataTablePageViewModel;
            sampleDataTablesPage.Navigate += this.OnNavigatePage;
            this.MainPivot.Items.Add(sampleDataTablesPivot);

            var listBlobsPage = new ListBlobsPage();
            var listBlobsPivot = new PivotItem();
            listBlobsPivot.Header = "list blobs";
            listBlobsPivot.Name = "ListBlobs";
            listBlobsPivot.Content = listBlobsPage;
            listBlobsPage.ViewModel = this.ViewModel.ListBlobsPageViewModel;
            listBlobsPage.TakePhoto += this.OnLaunchCamera;
            this.MainPivot.Items.Add(listBlobsPivot);

            var listQueuesPage = new ListQueuesPage();
            var listQueuesPivot = new PivotItem();
            listQueuesPivot.Header = "queues";
            listQueuesPivot.Name = "ListQueuesPage";
            listQueuesPivot.Content = listQueuesPage;
            listQueuesPage.ViewModel = this.ViewModel.ListQueuesPageViewModel;
            listQueuesPage.Navigate += this.OnNavigatePage;
            this.MainPivot.Items.Add(listQueuesPivot);

            this.MainPivot.SelectedItem = notificationsPivot;
        }

        private IEnumerable<IApplicationBarMenuItem> GetApplicationBarItemsByText(string text)
        {
            return this.ApplicationBar.MenuItems.Cast<IApplicationBarMenuItem>()
                        .Where(m => m.Text.Equals(text, StringComparison.OrdinalIgnoreCase))
                        .Concat(
                            this.ApplicationBar.Buttons.Cast<IApplicationBarMenuItem>()
                                .Where(b => b.Text.Equals(text, StringComparison.OrdinalIgnoreCase)));
        }

        private void OnMainPivotSelectionChanged()
        {
            var currentItem = this.MainPivot.SelectedItem as PivotItem;
            if (currentItem != null)
            {
                var currentView = currentItem.Content as UserControl;
                if (currentView != null)
                {
                    var viewModel = currentView.DataContext as PivotItemViewModel;
                    if (viewModel != null)
                    {
                        viewModel.UpdateApplicationBarButtons(this.ApplicationBar, new[] { "log out" });
                    }
                }
            }
        }

        private void OnLaunchCamera(object sender, EventArgs e)
        {
            var cameraCaptureTask = new CameraCaptureTask();
            cameraCaptureTask.Completed += this.OnCameraCaptureTaskCompleted;
            cameraCaptureTask.Show();
        }

        private void OnCameraCaptureTaskCompleted(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                PhoneHelpers.SetApplicationState("PhotoResult", e);

                // Delay navigation until the first navigated event.
                this.NavigationService.Navigated += this.OnNavigatedCompleted;
            }
        }

        private void OnNavigatedCompleted(object sender, EventArgs e)
        {
            PhoneHelpers.SetApplicationState("UserBackPress", false);

            // Do the delayed navigation from the main page.
            this.NavigationService.Navigate(new Uri("/Pages/UploadPhotoPage.xaml", UriKind.Relative));
            this.NavigationService.Navigated -= this.OnNavigatedCompleted;
        }

        private void OnNavigatePage(object sender, NavigationEventArgs e)
        {
            this.NavigationService.Navigate(e.Uri);
        }

        private void OnLogout(object sender, EventArgs e)
        {
            this.MainPivot.IsEnabled = false;
            this.MainPivot.Opacity = 0.3d;
            foreach (var button in this.GetApplicationBarItemsByText("log out"))
            {
                button.IsEnabled = false;
            }

            var pushNotificationClient = App.CloudClientFactory.ResolvePushNotificationClient();
            pushNotificationClient.Disconnect(r => this.Dispatcher.BeginInvoke(() => this.CleanUp(r.Exception)));
        }

        private void CleanUp(Exception exception)
        {
            if (exception != null)
            {
                MessageBox.Show(exception.Message, "Push Unregistration Error", MessageBoxButton.OK);
            }

            this.MainPivot.IsEnabled = true;
            this.MainPivot.Opacity = 1;

            foreach (var button in this.GetApplicationBarItemsByText("log out"))
            {
                button.IsEnabled = true;
            }

            // Clean the current authentication token, flags and view models.
            App.CloudClientFactory.CleanAuthenticationToken();
            this.DataContext = null;
            this.MainPivot.Items.Clear();
            PhoneHelpers.RemoveIsolatedStorageSetting("UserIsRegistered");
            PhoneHelpers.SetApplicationState("UserBackPress", false);

            // Navigate to the log in page.
            this.NavigationService.GoBack();
        }

        private void OnBeginPushConnection(object sender, EventArgs e)
        {
            var items = this.GetApplicationBarItemsByText("log out");
            foreach (var item in items)
            {
                item.IsEnabled = false;
            }
        }

        private void OnEndPushConnection(object sender, EventArgs e)
        {
            var items = this.GetApplicationBarItemsByText("log out");
            foreach (var item in items)
            {
                item.IsEnabled = true;
            }
        }
    }
}