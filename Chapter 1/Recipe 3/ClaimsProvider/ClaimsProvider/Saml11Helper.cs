using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IdentityModel.Tokens;
using System.IdentityModel.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using System.ServiceModel.Security;
using System.Security.Principal;

namespace ClaimsProvider
{
    public class Saml11Helper
    {
        public static SamlAssertion CreateSamlAssertionFromUserNameClaims(IEnumerable<Claim> claims)
        {
            SamlSubject subject = new SamlSubject()
            {
                Name = "Windows Group Claim"
            };
            SamlAttributeStatement statement = new SamlAttributeStatement()
            {
                SamlSubject = subject
            };
            SamlAssertion assertion = new SamlAssertion()
            {
                AssertionId = string.Format("_{0}", Guid.NewGuid().ToString()),
                Issuer = "System"
            };

            foreach (var claim in claims)
            {
                Claim samlClaim = new Claim(claim.ClaimType, GetResourceFromSid(claim.Resource as SecurityIdentifier), claim.Right);
                SamlAttribute attribute = new SamlAttribute(samlClaim);
                statement.Attributes.Add(attribute);
            }

            assertion.Statements.Add(statement);
            SignSamlAssertion(assertion);
            return assertion;
        }

        private static string GetResourceFromSid(SecurityIdentifier sid)
        {
            try
            {
                return sid.Translate(typeof(NTAccount)).ToString();
            }
            catch (Exception)
            {
                return sid.Value;
            }
        }

        private static void SignSamlAssertion(SamlAssertion assertion)
        {
            X509Certificate2 certificate2 = GetCertificateFromStore(System.Configuration.ConfigurationManager.AppSettings["TokenSigningCeritificate"]);
            X509AsymmetricSecurityKey securityKey = new X509AsymmetricSecurityKey(certificate2);
            assertion.SigningCredentials = new SigningCredentials(
                securityKey,
                SecurityAlgorithms.RsaSha1Signature,
                SecurityAlgorithms.Sha1Digest,
                new SecurityKeyIdentifier(new X509ThumbprintKeyIdentifierClause(certificate2)));
        }

        private static X509Certificate2 GetCertificateFromStore(string certName)
        {
            X509Store store = new X509Store(StoreLocation.CurrentUser);
            try
            {
                store.Open(OpenFlags.ReadOnly);
                X509Certificate2Collection certCollection = store.Certificates;
                X509Certificate2Collection currentCerts = certCollection.Find(X509FindType.FindByTimeValid, DateTime.Now, false);
                X509Certificate2Collection signingCert = currentCerts.Find(X509FindType.FindBySubjectDistinguishedName, certName, false);
                if (signingCert.Count == 0)
                    return null;
                return signingCert[0];
            }
            finally
            {
                store.Close();
            }
        }

        public static string SerializeSamlToken(SamlSecurityToken token)
        {
            StringBuilder samlBuilder = new StringBuilder();
            using (XmlWriter writer = XmlWriter.Create(samlBuilder))
            {
                try
                {
                    WSSecurityTokenSerializer keyInfoSerializer = new WSSecurityTokenSerializer();
                    keyInfoSerializer.WriteToken(writer, token);
                    Console.WriteLine("Saml Token Successfully Created");
                }
                catch (Exception)
                {
                    Console.WriteLine("Failed to seralize token");
                }
            }
            return samlBuilder.ToString();
        }
    }
}
