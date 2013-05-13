using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IdentityService.Common;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens;
using System.IdentityModel.Selectors;
using System.Security.Cryptography.X509Certificates;
using System.Configuration;
using Microsoft.IdentityModel.Configuration;
using System.IO;
using System.Text;
using System.Xml;
using Microsoft.IdentityModel.Tokens.Saml11;

namespace WebRP
{
    public class Saml11SecureTokenConsumer : SecureTokenConsumerBase
    {
        private string _token;
        private string _serviceConfig;
        private string resolvedToken;

        public Saml11SecureTokenConsumer(string token, string serviceConfig)
        {
            _token = token;
            _serviceConfig = serviceConfig;
        }

        protected override SecurityTokenHandlerCollection GetTokenHandlerCollection()
        {
            SecurityTokenHandlerCollection collection = SecurityTokenHandlerCollection.CreateDefaultSecurityTokenHandlerCollection();
            List<SecurityToken> serviceTokens = new List<SecurityToken>();
            X509Certificate2 encryptCert = CertificateUtil.GetCertificate(
                StoreName.My, 
                StoreLocation.LocalMachine,
                ConfigurationManager.AppSettings["RpCertificate"]);
            serviceTokens.Add(new X509SecurityToken(encryptCert));
            SecurityTokenResolver serviceResolver = SecurityTokenResolver.CreateDefaultSecurityTokenResolver(serviceTokens.AsReadOnly(), false);
            collection.Configuration.ServiceTokenResolver = serviceResolver;
            X509CertificateStoreTokenResolver certificateStoreIssuerResolver = new X509CertificateStoreTokenResolver(StoreName.My, StoreLocation.LocalMachine);
            collection.Configuration.IssuerTokenResolver = certificateStoreIssuerResolver;
            return collection;
        }

        protected override SecurityToken DeserializeToken(SecurityTokenHandlerCollection handlers)
        {
            ServiceConfiguration config = new ServiceConfiguration(_serviceConfig);
            handlers.Configuration.AudienceRestriction = config.AudienceRestriction;
            handlers.Configuration.IssuerNameRegistry = config.IssuerNameRegistry;
            var txtReader = new StringReader(_token);
            StringBuilder sb = new StringBuilder();
            XmlReader reader = XmlReader.Create(txtReader);

            var token = handlers.ReadToken(reader);
            resolvedToken = SerializeToken(token as SamlSecurityToken);
            return token;
        }

        public string ResolvedToken
        {
            get
            {
                return resolvedToken;
            }
        }

        private string SerializeToken(SamlSecurityToken token)
        {
            var handler = new Saml11SecurityTokenHandler();
            XmlWriterSettings settings = new XmlWriterSettings()
                {
                    Encoding = Encoding.UTF8,
                    Indent = true
                };

            StringBuilder sb = new StringBuilder();
            var writer = XmlTextWriter.Create(sb, settings);
            writer.WriteProcessingInstruction("xml", "version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"");
            handler.WriteToken(writer, token);
            writer.Close();
            return sb.ToString();
        }
    }
}