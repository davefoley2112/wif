using System;
using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.ServiceModel.Security;
using System.Xml;

namespace WindowsIdentityToClaimsConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            using (WindowsClaimSet claims = new WindowsClaimSet(WindowsIdentity.GetCurrent()))
            {
                var accessControlClaims = claims.FindClaims(ClaimTypes.Sid, Rights.PossessProperty);
                SamlAssertion assertion = CreateSamlAssertionFromWindowsIdentityClaims(accessControlClaims);
                SamlSecurityToken token = new SamlSecurityToken(assertion);
                SerializeSamlTokenToFile(token);
            }
            
            Console.ReadLine();
            
        }

        private static SamlAssertion CreateSamlAssertionFromWindowsIdentityClaims(IEnumerable<Claim> claims)
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

        //
        // The SAML token needs to be digitally signed using a certificate to validate the authenticity 
        // of the issuer. In our solution, we are signing the assertion using a self-issued X.509 certificate. 
        // The SamlAssertion class exposes a SigningCredentials property that is used to digitally sign the 
        // SAML assertion:
        //
        private static void SignSamlAssertion(SamlAssertion assertion)
        {
            X509Certificate2 certificate2 = GetCertificateFromStore(StoreLocation.CurrentUser, DateTime.Now, "CN=SamlTokenSigningCertificate");

            if (certificate2 != null)
            {
                X509AsymmetricSecurityKey securityKey = new X509AsymmetricSecurityKey(certificate2);

                assertion.SigningCredentials = new SigningCredentials(
                    securityKey,
                    SecurityAlgorithms.RsaSha1Signature,
                    SecurityAlgorithms.Sha1Digest,
                    new SecurityKeyIdentifier(new X509ThumbprintKeyIdentifierClause(certificate2)));
            }
        }

        private static X509Certificate2 GetCertificateFromStore(StoreLocation location, DateTime timeValid, string subjectDistinguishedName)
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

        //
        // The System.ServiceModel.Security namespace has a WSSecurityTokenSerializer class that can be 
        // used to serialize and deserialize the security tokens conforming to the WS-Security and the 
        // WS-Trust specifications. In our solution, we are using a XmlWriter class to persist the token 
        // into the filesystem:
        //
        private static void SerializeSamlTokenToFile(SamlSecurityToken token)
        {
            using (XmlWriter writer = XmlWriter.Create(@"c:\github\wif\saml.xml"))
            {
                try
                {
                    WSSecurityTokenSerializer keyInfoSerializer = new WSSecurityTokenSerializer();
                    keyInfoSerializer.WriteToken(writer, token);
                    Console.WriteLine("Saml Token Successfully Created");
                }
                catch (Exception)
                {
                    Console.WriteLine("Failed to save Saml Token to Disk");
                }
            }
        }
    }
}
