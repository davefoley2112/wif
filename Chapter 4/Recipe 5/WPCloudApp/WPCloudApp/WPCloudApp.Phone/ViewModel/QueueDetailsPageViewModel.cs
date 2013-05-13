namespace WPCloudApp.Phone.ViewModel
{
    using System;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Text;
    using System.Windows;
    using System.Windows.Threading;
    using Microsoft.Samples.WindowsPhoneCloud.StorageClient;

    public class QueueDetailsPageViewModel : BaseViewModel
    {
        private readonly Dispatcher dispatcher;

        private ICloudQueue queue;
        private bool isBusy;
        private string queueMessageContent;
        private string message;

        public QueueDetailsPageViewModel() :
            this(Deployment.Current.Dispatcher)
        {
        }

        public QueueDetailsPageViewModel(Dispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
            this.CloudQueueMessages = new ObservableCollection<CloudQueueMessage>();
        }

        public ICloudQueue Queue
        {
            get
            {
                return this.queue;
            }

            set
            {
                this.queue = value;
                this.NotifyPropertyChanged("Queue");
            }
        }

        public bool IsBusy
        {
            get
            {
                return this.isBusy;
            }

            set
            {
                if (this.isBusy != value)
                {
                    this.isBusy = value;
                    this.NotifyPropertyChanged("IsBusy");
                }
            }
        }

        public string QueueMessageContent
        {
            get
            {
                return this.queueMessageContent;
            }

            set
            {
                if (this.queueMessageContent != value)
                {
                    this.queueMessageContent = value;
                    this.NotifyPropertyChanged("QueueMessageContent");
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

        public ObservableCollection<CloudQueueMessage> CloudQueueMessages { get; set; }

        public void QueueMessage()
        {
            this.Message = "Queing message...";
            this.IsBusy = true;
            try
            {
                this.Queue.AddMessage(
                    new CloudQueueMessage { AsBytes = Encoding.UTF8.GetBytes(this.QueueMessageContent) },
                    r => this.dispatcher.BeginInvoke(
                        () =>
                        {
                            this.Message = r.Exception == null
                                ? "Message successfully queued!"
                                : string.Format(CultureInfo.InvariantCulture, "Error: {0}", r.Exception.Message);

                            this.IsBusy = false;
                        }));
            }
            catch (Exception exception)
            {
                var errorMessage = StorageClientExceptionParser.ParseDataServiceException(exception).Message;

                this.IsBusy = false;
                this.Message = string.Format(CultureInfo.InvariantCulture, "Error: {0}", errorMessage);
            }
        }

        public void DequeueMessage()
        {
            this.Message = "Dequeing message...";
            this.IsBusy = true;
            try
            {
                this.Queue.GetMessage(
                    s =>
                    {
                        if (s.Exception == null)
                        {
                            if (s.Response == null)
                            {
                                this.dispatcher.BeginInvoke(
                                    () =>
                                    {
                                        this.Message = "Queue is empty";
                                        this.IsBusy = false;
                                    });
                            }
                            else
                            {
                                this.Queue.DeleteMessage(
                                    s.Response,
                                    r => this.dispatcher.BeginInvoke(
                                        () =>
                                        {
                                            if (r.Exception == null)
                                            {
                                                this.CloudQueueMessages.Add(s.Response);
                                                this.Message = "Message successfully dequeued!";
                                            }
                                            else
                                            {
                                                this.Message = string.Format(CultureInfo.InvariantCulture, "Error: {0}", r.Exception.Message);
                                            }

                                            this.IsBusy = false;
                                        }));
                            }
                        }
                        else
                        {
                            this.dispatcher.BeginInvoke(
                                () =>
                                {
                                    this.Message = string.Format(CultureInfo.InvariantCulture, "Error: {0}", s.Exception.Message);
                                    this.IsBusy = false;
                                });
                        }
                    });
            }
            catch (Exception exception)
            {
                var errorMessage = StorageClientExceptionParser.ParseDataServiceException(exception).Message;

                this.IsBusy = false;
                this.Message = string.Format(CultureInfo.InvariantCulture, "Error: {0}", errorMessage);
            }
        }
    }
}
