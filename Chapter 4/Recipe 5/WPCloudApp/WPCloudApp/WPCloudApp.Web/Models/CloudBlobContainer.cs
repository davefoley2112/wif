namespace WPCloudApp.Web.Models
{
    using System;
    using System.Runtime.Serialization;

    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/Microsoft.Samples.WindowsPhoneCloud.StorageClient", Name = "Container")]
    public class CloudBlobContainer
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember(Name = "Url")]
        public Uri Uri { get; set; }
    }
}