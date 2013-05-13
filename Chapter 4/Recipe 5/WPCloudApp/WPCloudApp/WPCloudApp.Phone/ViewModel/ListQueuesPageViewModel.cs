namespace WPCloudApp.Phone.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using System.Windows;
    using System.Windows.Navigation;
    using System.Windows.Threading;
    using Microsoft.Phone.Shell;
    using Microsoft.Samples.WindowsPhoneCloud.StorageClient;

    public class ListQueuesPageViewModel : PivotItemViewModel
    {
        private const string IconsRootUri = "/Toolkit.Content/";

        private readonly ICloudQueueClient queueClient;
        private readonly Dispatcher dispatcher;

        private bool isLoading = false;
        private bool hasResults = true;
        private string prefix = string.Empty;
        private string message;

        public ListQueuesPageViewModel()
            : this(App.CloudClientFactory.ResolveCloudQueueClient("QueueAndList"), Deployment.Current.Dispatcher)
        {
        }

        public ListQueuesPageViewModel(ICloudQueueClient queueClient, Dispatcher dispatcher)
        {
            this.queueClient = queueClient;
            this.dispatcher = dispatcher;

            this.CloudQueues = new ObservableCollection<ICloudQueue>();
        }

        public event EventHandler<NavigationEventArgs> Navigate;

        public ObservableCollection<ICloudQueue> CloudQueues { get; set; }

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

        public bool HasResults
        {
            get
            {
                return this.hasResults;
            }

            set
            {
                if (this.hasResults != value)
                {
                    this.hasResults = value;
                    this.NotifyPropertyChanged("HasResults");
                }
            }
        }

        public string Prefix
        {
            get
            {
                return this.prefix;
            }

            set
            {
                if (this.prefix != value)
                {
                    this.prefix = value;
                    this.NotifyPropertyChanged("Prefix");
                }
            }
        }

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

        public void ListQueues()
        {
            this.IsLoading = true;
            this.Message = "Listing queues...";
            try
            {
                this.queueClient.ListQueues(
                this.Prefix,
                r =>
                {
                    if (this.dispatcher != null)
                    {
                        this.dispatcher.BeginInvoke(() => this.HandleQueueListResult(r));
                    }
                    else
                    {
                        this.HandleQueueListResult(r);
                    }
                });
            }
            catch (Exception exception)
            {
                this.IsLoading = false;
                this.Message = string.Format(CultureInfo.InvariantCulture, "Error: {0}", exception.Message);
            }
        }

        public void NavigateTo(Uri navigationUri)
        {
            this.RaiseNavigate(navigationUri);
        }

        public void NewQueue()
        {
            var newQueueName = string.Format(CultureInfo.InvariantCulture, "Queue{0}", DateTime.Now.ToString("yyyyMMddTHHmmss", CultureInfo.InvariantCulture)).ToLowerInvariant();

            this.Message = "Creating new queue...";
            this.IsLoading = true;

            try
            {
                var currentQueue = this.queueClient.GetQueueReference(newQueueName);
                currentQueue.Create(
                    creationResult =>
                    {
                        if (this.dispatcher != null)
                        {
                            this.dispatcher.BeginInvoke(() => this.HandleQueueCreationResult(currentQueue, creationResult));
                        }
                        else
                        {
                            this.HandleQueueCreationResult(currentQueue, creationResult);
                        }
                    });
            }
            catch (Exception exception)
            {
                this.IsLoading = false;
                this.Message = string.Format(CultureInfo.InvariantCulture, "Error: {0}", exception.Message);
            }
        }

        public void DeleteQueue(ICloudQueue queue)
        {
            this.Message = "Deleting queue...";
            this.IsLoading = true;

            try
            {
                var currentQueue = this.queueClient.GetQueueReference(queue.Name);
                currentQueue.Delete(
                    deletionResult =>
                    {
                        if (this.dispatcher != null)
                        {
                            this.dispatcher.BeginInvoke(() => this.HandleQueueDeletionResult(currentQueue, deletionResult));
                        }
                        else
                        {
                            this.HandleQueueDeletionResult(currentQueue, deletionResult);
                        }
                    });
            }
            catch (Exception exception)
            {
                this.IsLoading = false;
                this.Message = string.Format(CultureInfo.InvariantCulture, "Error: {0}", exception.Message);
            }
        }

        protected override void PopulateApplicationBarButtons(IApplicationBar applicationBar)
        {
            if (applicationBar != null && applicationBar.Buttons != null)
            {
                var addButton = new ApplicationBarIconButton(new Uri(string.Format(CultureInfo.InvariantCulture, "{0}{1}", IconsRootUri, "appbar.add.rest.png"), UriKind.Relative)) { Text = "add queue" };
                addButton.Click += (s, e) => this.NewQueue();

                applicationBar.Buttons.Add(addButton);
            }
        }

        private void HandleQueueListResult(CloudOperationResponse<IEnumerable<ICloudQueue>> result)
        {
            if (result.Exception == null)
            {
                this.CloudQueues.Clear();
                foreach (var queue in result.Response)
                {
                    this.CloudQueues.Add(queue);
                }

                this.HasResults = this.CloudQueues.Count > 0;
                this.Message = "Listing finished successfully.";
            }
            else
            {
                this.Message = string.Format(CultureInfo.InvariantCulture, "Error: {0}", result.Exception.Message);
            }

            this.IsLoading = false;
        }

        private void HandleQueueCreationResult(ICloudQueue currentQueue, CloudOperationResponse<bool> creationResult)
        {
            if (creationResult.Response)
            {
                this.CloudQueues.Add(currentQueue);
                this.HasResults = true;
                this.Message = "Changes saved successfully.";
            }
            else
            {
                this.Message = string.Format(CultureInfo.InvariantCulture, "Error: {0}", creationResult.Exception.Message);
            }

            this.IsLoading = false;
        }

        private void HandleQueueDeletionResult(ICloudQueue currentQueue, CloudOperationResponse<bool> deletionResult)
        {
            if (deletionResult.Response)
            {
                var listedQueue = this.CloudQueues.Where(q => q.Name.Equals(currentQueue.Name, StringComparison.InvariantCulture)).SingleOrDefault();
                if (listedQueue != null)
                {
                    this.CloudQueues.Remove(listedQueue);
                    this.Message = "Changes saved successfully.";
                }
            }
            else
            {
                this.Message = string.Format(CultureInfo.InvariantCulture, "Error: {0}", deletionResult.Exception.Message);
            }

            this.IsLoading = false;
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
