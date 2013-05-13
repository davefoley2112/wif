namespace WPCloudApp.Phone.PivotContent
{
    using System;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Navigation;
    using WPCloudApp.Phone.Helpers;
    using WPCloudApp.Phone.ViewModel;

    public partial class SampleDataTablePage : UserControl
    {
        public SampleDataTablePage()
        {
            this.InitializeComponent();

            this.Loaded += this.OnSampleDataTablePageLoaded;
        }

        public event EventHandler<NavigationEventArgs> Navigate
        {
            add { this.ViewModel.Navigate += value; }
            remove { this.ViewModel.Navigate -= value; }
        }

        public SampleDataTablePageViewModel ViewModel
        {
            get { return this.DataContext as SampleDataTablePageViewModel; }
            set { this.DataContext = value; }
        }

        private void OnSampleDataTablePageLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.ViewModel != null)
            {
                this.ViewModel.LoadTable();
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

                PhoneHelpers.SetApplicationState("CurrentSampleDataRow", this.ViewModel.Table[selector.SelectedIndex]);
                this.ViewModel.NavigateTo(new Uri("/Pages/SampleDataDetailsPage.xaml?editSampleData=true", UriKind.Relative));

                selector.SelectedIndex = -1;
            }
        }
    }
}