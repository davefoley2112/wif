namespace WPCloudApp.Phone.ViewModel
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Navigation;
    using System.Windows.Threading;
    using Microsoft.Phone.Shell;
    using Microsoft.Samples.WindowsPhoneCloud.StorageClient;
    using WPCloudApp.Phone.Models;

    public class SampleDataTablePageViewModel : TableBaseViewModel<SampleData>
    {
        private const string IconsRootUri = "/Toolkit.Content/";

        private readonly ICloudTableClient cloudTableClient;

        private static bool sampleDataTableCreated = false;

        public SampleDataTablePageViewModel()
            : this(App.CloudClientFactory, Deployment.Current.Dispatcher)
        {
        }

        public SampleDataTablePageViewModel(ICloudClientFactory cloudClientFactory, Dispatcher dispatcher)
            : base(cloudClientFactory, dispatcher)
        {
            this.cloudTableClient = cloudClientFactory.ResolveCloudTableClient(this.TableName);
        }

        public event EventHandler<NavigationEventArgs> Navigate;

        public ICloudTableClient CloudTableClient
        {
            get
            {
                return this.cloudTableClient;
            }
        }

        public override string TableName
        {
            get { return "SampleData"; }
        }

        public override void LoadTable()
        {
            if (!sampleDataTableCreated)
            {
                try
                {
                    this.cloudTableClient.CreateTableIfNotExist(
                        this.TableName,
                        r =>
                        {
                            if (this.Dispatcher != null)
                            {
                                this.Dispatcher.BeginInvoke(() => this.HandleTableCreationResponse(r));
                            }
                            else
                            {
                                this.HandleTableCreationResponse(r);
                            }
                        });
                }
                catch (Exception exception)
                {
                    var errorMessage = StorageClientExceptionParser.ParseDataServiceException(exception).Message;

                    this.IsLoading = false;
                    this.Message = string.Format(CultureInfo.InvariantCulture, "Error: {0}", errorMessage);
                }
            }
            else
            {
                base.LoadTable();
            }
        }

        public void NavigateTo(Uri navigationUri)
        {
            this.RaiseNavigate(navigationUri);
        }

        protected override void PopulateApplicationBarButtons(IApplicationBar applicationBar)
        {
            var refreshButton = new ApplicationBarIconButton(new Uri(string.Format(CultureInfo.InvariantCulture, "{0}{1}", IconsRootUri, "appbar.refresh.rest.png"), UriKind.Relative)) { Text = "refresh" };
            refreshButton.Click += (s, e) => this.LoadTable();

            var addButton = new ApplicationBarIconButton(new Uri(string.Format("{0}{1}", IconsRootUri, "appbar.add.rest.png"), UriKind.Relative)) { Text = "add row" };
            addButton.Click += (s, e) => this.RaiseNavigate(new Uri("/Pages/SampleDataDetailsPage.xaml?editSampleData=false", UriKind.Relative));

            applicationBar.Buttons.Add(refreshButton);
            applicationBar.Buttons.Add(addButton);
        }

        private void HandleTableCreationResponse(CloudOperationResponse<bool> response)
        {
            if (response.Exception == null)
            {
                base.LoadTable();
                sampleDataTableCreated = true;
            }
            else
            {
                this.Message = string.Format(CultureInfo.InvariantCulture, "Error: {0}", response.Exception.Message);
            }
        }

        private void RaiseNavigate(Uri uri)
        {
            var navigate = this.Navigate;
            if (navigate != null)
            {
                navigate(this, new NavigationEventArgs(null, uri));
            }
        }
    }
}
