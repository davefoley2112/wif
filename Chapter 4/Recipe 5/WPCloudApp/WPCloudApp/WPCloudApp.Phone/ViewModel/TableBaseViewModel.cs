namespace WPCloudApp.Phone.ViewModel
{
    using System;
    using System.Globalization;
    using System.Windows.Threading;
    using Microsoft.Samples.Data.Services.Client;
    using Microsoft.Samples.WindowsPhoneCloud.StorageClient;

    public abstract class TableBaseViewModel<T> : PivotItemViewModel
    {
        private string message;
        private bool isLoading = false;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "The TableName property specifies the 'context name' from where to resolve the class dependencies.")]
        protected TableBaseViewModel(ICloudClientFactory cloudClientFactory, Dispatcher dispatcher)
        {
            cloudClientFactory.CleanTablesClientsKey(this.TableName);

            this.Context = cloudClientFactory.ResolveTablesServiceContext(this.TableName);
            this.Table = cloudClientFactory.ResolveDataServiceCollection<T>(this.TableName);

            this.Table.LoadCompleted += this.OnTableLoaded;
            this.Dispatcher = dispatcher;
        }

        public DataServiceCollection<T> Table { get; set; }

        public string Message
        {
            get
            {
                return this.message;
            }

            set
            {
                if (this.message != value)
                {
                    this.message = value;
                    this.NotifyPropertyChanged("Message");
                }
            }
        }

        public bool IsLoading
        {
            get
            {
                return this.isLoading;
            }

            set
            {
                if (this.isLoading != value)
                {
                    this.isLoading = value;
                    this.NotifyPropertyChanged("IsLoading");
                }
            }
        }

        public abstract string TableName { get; }

        public ITableServiceContext Context { get; private set; }

        protected Dispatcher Dispatcher { get; private set; }

        public virtual void LoadTable()
        {
            if (!this.IsLoading)
            {
                // Adding an incremental seed to avoid a cached response.
                var tableUri = new Uri(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "{0}/{1}?incrementalSeed={2}",
                        this.Context.BaseUri.AbsoluteUri.TrimEnd('/'),
                        this.TableName,
                        DateTime.UtcNow.Ticks),
                    UriKind.Absolute);

                this.IsLoading = true;
                this.Message = "Loading...";

                this.Table.Clear();
                try
                {
                    this.Table.LoadAsync(tableUri);
                }
                catch (Exception exception)
                {
                    var errorMessage = StorageClientExceptionParser.ParseDataServiceException(exception).Message;

                    this.IsLoading = false;
                    this.Message = string.Format(CultureInfo.InvariantCulture, "Error: {0}", errorMessage);
                }
            }
        }

        protected virtual void OnTableLoaded(object sender, LoadCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                this.DispatchResult("The operation has been cancelled.");
            }
            else if (e.Error != null)
            {
                var errorMessage = StorageClientExceptionParser.ParseDataServiceException(e.Error).Message;
                this.DispatchResult(string.Format(CultureInfo.InvariantCulture, "Error: {0}", errorMessage));
            }
            else if (this.Table.Continuation != null)
            {
                this.Table.LoadNextPartialSetAsync();
            }
            else
            {
                this.DispatchResult();
            }
        }

        protected virtual void DispatchResult(string message = "")
        {
            this.Dispatcher.BeginInvoke(
                () =>
                {
                    this.Message = message;
                    this.IsLoading = false;
                });
        }
    }
}
