using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.IdentityModel.Tokens;

namespace X509CertificateHelper
{
    public class X509CertificationHelper
    {
        public static X509Certificate2 GetValidCertificateFromStore(StoreLocation location, DateTime timeValid, string subjectDistinguishedName)
        {
            X509Store store = new X509Store(location);
            try
            {
                store.Open(OpenFlags.ReadOnly);
                X509Certificate2Collection validCertificates = store.Certificates.Find(X509FindType.FindByTimeValid, timeValid, false);
                X509Certificate2Collection signingCertificate = validCertificates.Find(X509FindType.FindBySubjectDistinguishedName, subjectDistinguishedName, false);
                if (signingCertificate.Count == 0)
                    return null;
                return signingCertificate[0];
            }
            finally
            {
                store.Close();
            }
        }

        public static SigningCredentials GetSigningCredentials(string subjectDistinguishedName)
        {
            X509Certificate2 certificate2 = GetValidCertificateFromStore(StoreLocation.CurrentUser, DateTime.Now, subjectDistinguishedName);
            X509AsymmetricSecurityKey securityKey = new X509AsymmetricSecurityKey(certificate2);
            return new SigningCredentials(
                securityKey,
                SecurityAlgorithms.RsaSha1Signature,
                SecurityAlgorithms.Sha1Digest,
                new SecurityKeyIdentifier(new X509ThumbprintKeyIdentifierClause(certificate2)));
        }
    }
}
