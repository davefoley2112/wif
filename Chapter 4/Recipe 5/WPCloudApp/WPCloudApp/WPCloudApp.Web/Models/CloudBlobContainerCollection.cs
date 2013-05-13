namespace WPCloudApp.Web.Models
{
    using System.Runtime.Serialization;

    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/Microsoft.Samples.WindowsPhoneCloud.StorageClient")]
    public class CloudBlobContainerCollection
    {
        [DataMember]
        public CloudBlobContainer[] Containers { get; set; }
    }
}