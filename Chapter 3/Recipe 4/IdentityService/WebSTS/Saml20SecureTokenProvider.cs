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
using Microsoft.IdentityModel.Tokens.Saml2;

namespace WebSTS
{
    public class Saml20SecureTokenProvider : SecureTokenProviderBase
    {
        Dictionary<string, string> _claims;
        public Saml20SecureTokenProvider(Dictionary<string, string> claims)
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
            throw new NotImplementedException();
        }

        protected override Lifetime GetTokenLifeTime()
        {
            return new Lifetime(DateTime.UtcNow,
                DateTime.UtcNow.AddHours(
                double.Parse(ConfigurationManager.AppSettings["HoursToExpiration"])));
        }

        protected override string SerializeToken(SecurityToken token)
        {
            XmlWriterSettings settings = new XmlWriterSettings()
            {
                Encoding = Encoding.UTF8,
                Indent = true
            };
            StringBuilder sb = new StringBuilder();
            XmlWriter innerWriter = XmlWriter.Create(sb, settings);
            innerWriter.WriteStartElement("Response", "urn:oasis:names:tc:SAML:2.0:protocol");
            innerWriter.WriteAttributeString("IssueInstant", DateTime.UtcNow.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.ffffZ"));
            innerWriter.WriteAttributeString("ID", "_" + Guid.NewGuid());
            innerWriter.WriteAttributeString("Version", "2.0");

            innerWriter.WriteStartElement("Status");
            innerWriter.WriteStartElement("StatusCode");
            innerWriter.WriteAttributeString("Value", "urn:oasis:names:tc:SAML:2.0:status:Success");
            innerWriter.WriteEndElement();
            innerWriter.WriteEndElement();
            SecurityTokenHandlerCollectionManager mgr = SecurityTokenHandlerCollectionManager.CreateDefaultSecurityTokenHandlerCollectionManager();
            SecurityTokenHandlerCollection sthc = mgr.SecurityTokenHandlerCollections.First();
            SecurityTokenSerializer ser = new SecurityTokenSerializerAdapter(sthc);

            ser.WriteToken(innerWriter, token);
            innerWriter.WriteEndElement(); 
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
            return handlers[typeof(Saml2SecurityToken)] as Saml2SecurityTokenHandler;
        }

        protected override bool IsEncrypted()
        {
            return false;
        }

        public override string Issue()
        {
            var handler = GetTokenHandler();
            var descriptor = GetSecurityTokenDescriptor();
            var saml2Token = handler.CreateToken(descriptor) as Saml2SecurityToken;
            
            Saml2SubjectConfirmationData subConfirmData = new Saml2SubjectConfirmationData();
            subConfirmData.Recipient = new Uri(GetAppliesToAddress());
            subConfirmData.NotOnOrAfter = descriptor.Lifetime.Expires;
            Saml2SubjectConfirmation subjConfirm = new Saml2SubjectConfirmation(
                Saml2Constants.ConfirmationMethods.Bearer,
                subConfirmData);
            saml2Token.Assertion.Subject = new Saml2Subject(subjConfirm);
            Saml2AuthenticationContext authCtx = new Saml2AuthenticationContext(new Uri("urn:none"));
            saml2Token.Assertion.Statements.Add(new Saml2AuthenticationStatement(authCtx));

            return SerializeToken(saml2Token);
        }

        public override SecurityTokenDescriptor GetSecurityTokenDescriptor()
        {
            return new SecurityTokenDescriptor
            {
                TokenType = Microsoft.IdentityModel.Tokens.SecurityTokenTypes.OasisWssSaml2TokenProfile11,
                AppliesToAddress = GetAppliesToAddress(),
                Lifetime = GetTokenLifeTime(),
                TokenIssuerName = GetIssuerName(),
                SigningCredentials = GetSigningCredentials(),
                Subject = GetOutputClaimsIdentity()
            };
        }
    }
}