using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Security.Cryptography.X509Certificates;

namespace ClaimsProvider
{
    class Program
    {
        static void Main(string[] args)
        {
            using (ServiceHost serviceHost = new ServiceHost(typeof(ClaimsProviderService)))
            {
                ServiceCredentials credentials = serviceHost.Credentials;
                X509Certificate2 certificate2 = credentials.ServiceCertificate.Certificate;
                ClaimsProviderServiceCredentials serviceCredentials = new ClaimsProviderServiceCredentials();
                serviceCredentials.ServiceCertificate.Certificate = certificate2;
                serviceHost.Description.Behaviors.Remove((typeof(ServiceCredentials)));
                serviceHost.Description.Behaviors.Add(serviceCredentials);
                serviceHost.Open();
                Console.WriteLine("Service Provider is accepting requests..");
                Console.ReadLine();
            }
        }
    }
}
