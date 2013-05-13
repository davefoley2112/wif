namespace WPCloudApp.Phone.Models
{
    using System;
    using System.Globalization;
    using Microsoft.Samples.Data.Services.Common;
    using Microsoft.Samples.WindowsPhoneCloud.StorageClient;

    [DataServiceEntity]
    [EntitySet("SampleData")]
    public class SampleData : TableServiceEntity
    {
        private string name;
        private string description;
        private int number;
        private DateTime date = DateTime.Now;

        public SampleData()
            : base("a", string.Format(CultureInfo.InvariantCulture, "{0:10}_{1}", DateTime.MaxValue.Ticks - DateTime.Now.Ticks, Guid.NewGuid()))
        {
        }

        public SampleData(string partitionKey, string rowKey)
            : base(partitionKey, rowKey)
        {
        }

        public string Name
        {
            get
            {
                return this.name;
            }

            set
            {
                this.name = value;
                this.OnPropertyChanged("Name");
            }
        }

        public string Description
        {
            get
            {
                return this.description;
            }

            set
            {
                this.description = value;
                this.OnPropertyChanged("Description");
            }
        }

        public int Number
        {
            get
            {
                return this.number;
            }

            set
            {
                this.number = value;
                this.OnPropertyChanged("Number");
            }
        }

        public DateTime Date
        {
            get
            {
                return this.date;
            }

            set
            {
                this.date = value;
                this.OnPropertyChanged("Date");
            }
        }
    }
}
