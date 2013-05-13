namespace WPCloudApp.Phone.PivotContent
{
    using System;
    using System.Windows.Controls;
    using Microsoft.Samples.WindowsPhoneCloud.StorageClient;
    using WPCloudApp.Phone.ViewModel;

    public partial class TablesPage : UserControl
    {
        public TablesPage()
        {
            this.InitializeComponent();

            this.Loaded += this.OnTablesPageLoaded;
        }

        public TablesPageViewModel ViewModel
        {
            get { return this.DataContext as TablesPageViewModel; }
            set { this.DataContext = value; }
        }

        private void OnTablesPageLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.ViewModel != null)
            {
                this.ViewModel.LoadTable();
            }
        }

        private void OnDeleteTable(object sender, EventArgs e)
        {
            var table = ((Button)sender).Tag as TableServiceSchema;
            if ((this.ViewModel != null) && (table != null))
            {
                this.ViewModel.DeleteTable(table);
            }
        }
    }
}