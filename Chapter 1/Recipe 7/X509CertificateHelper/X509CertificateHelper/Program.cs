using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IdentityModel.Tokens;

namespace X509CertificateHelper
{
    class Program
    {
        static void Main(string[] args)
        {
            SigningCredentials cred = X509CertificationHelper.GetSigningCredentials("CN=SamlTokenSigningCertificate");
            Console.WriteLine(string.Format("Signing Key Size {0}",cred.SigningKey.KeySize));
            Console.ReadLine();
        }
    }
}
