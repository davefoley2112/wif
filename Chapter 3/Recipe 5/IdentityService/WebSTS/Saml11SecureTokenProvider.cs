using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using IdentityService.Common;
using Microsoft.IdentityModel.Claims;
using Microsoft.IdentityModel.Protocols.WSTrust;
using Microsoft.IdentityModel.SecurityTokenService;
using Microsoft.IdentityModel.Tokens;

namespace WebSTS
{
    public class Saml11SecureTokenProvider : SecureTokenProviderBase
    {
        Dictionary<string, string> _claims;
        public Saml11SecureTokenProvider(Dictionary<string, string> claims)
        {
            _claims = claims;
        }

        protected override SigningCredentials GetSigningCredentials()
        {
            X509Certificate2 signingCert = CertificateUtil.GetCertificate(
                StoreName.My,
                StoreLocation.LocalMachine,
                ConfigurationManager.AppSettings["SigningCertificate"]);
            return new X509SigningCredentials(signingCert);
        }

        protected override IClaimsIdentity GetOutputClaimsIdentity()
        {
            List<Claim> claims = new List<Claim>();
            foreach (var claim in _claims)
            {
                claims.Add(new Claim(claim.Key, claim.Value));
            }
            return new ClaimsIdentity(claims);
        }

        protected override EncryptingCredentials GetEncryptingCredentials()
        {
            X509Certificate2 encryptCert = CertificateUtil.GetCertificate(
                StoreName.My,
                StoreLocation.LocalMachine,
                ConfigurationManager.AppSettings["EncryptingCertificate"]);
            return new EncryptedKeyEncryptingCredentials(encryptCert);
        }

        protected override Lifetime GetTokenLifeTime()
        {
            return new Lifetime(DateTime.UtcNow, 
                DateTime.UtcNow.AddHours(
                double.Parse(ConfigurationManager.AppSettings["HoursToExpiration"])));
        }

        protected override string SerializeToken(SecurityToken token)
        {
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                Indent = true
            };
            StringBuilder sb = new StringBuilder();
            XmlWriter innerWriter = XmlWriter.Create(sb, settings);
            innerWriter.WriteProcessingInstruction("xml", "version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"");
            SecurityTokenHandlerCollectionManager mgr = SecurityTokenHandlerCollectionManager.CreateDefaultSecurityTokenHandlerCollectionManager();
            SecurityTokenHandlerCollection sthc = mgr.SecurityTokenHandlerCollections.First();
            SecurityTokenSerializer ser = new SecurityTokenSerializerAdapter(sthc);
            ser.WriteToken(innerWriter, token);
            innerWriter.Close();

            return sb.ToString();
        }

        protected override string GetAppliesToAddress()
        {
            return ConfigurationManager.AppSettings["AppliesToAddress"];
        }

        protected override string GetIssuerName()
        {
            return ConfigurationManager.AppSettings["IssuerName"];
        }

        protected override SecurityTokenHandler GetTokenHandler()
        {
            SecurityTokenHandlerCollection handlers = SecurityTokenHandlerCollection.CreateDefaultSecurityTokenHandlerCollection();
            return handlers[typeof(SamlSecurityToken)];
        }

        protected override bool IsEncrypted()
        {
            return !string.IsNullOrEmpty(ConfigurationManager.AppSettings["EncryptingCertificate"]);
        }
    }
}