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
using Microsoft.IdentityModel.Tokens.Saml2;

namespace WebRP
{
    public class Saml20SecureTokenConsumer : SecureTokenConsumerBase
    {
        private string _token;
        private string _serviceConfig;

        public Saml20SecureTokenConsumer(string token, string serviceConfig)
        {
            _token = token;
            _serviceConfig = serviceConfig;
        }

        protected override SecurityTokenHandlerCollection GetTokenHandlerCollection()
        {
            SecurityTokenHandlerCollectionManager manager = SecurityTokenHandlerCollectionManager.CreateDefaultSecurityTokenHandlerCollectionManager();
            SecurityTokenHandlerCollection handlers = manager.SecurityTokenHandlerCollections.First();
            handlers.Add(new Saml20SecureTokenHandler());

            return handlers;
        }

        protected override SecurityToken DeserializeToken(SecurityTokenHandlerCollection handlers)
        {
            ServiceConfiguration config = new ServiceConfiguration(_serviceConfig);
            handlers.Configuration.AudienceRestriction = config.AudienceRestriction;
            var txtReader = new StringReader(_token);
            StringBuilder sb = new StringBuilder();
            XmlReader reader = XmlReader.Create(txtReader);

            var token = handlers.ReadToken(reader);
            return token;
        }
       
        public override Dictionary<string, string> ParseAttributesFromSecureToken()
        {
            Dictionary<string, string> attributes = new Dictionary<string, string>();
            var handlers = GetTokenHandlerCollection();
            var token = DeserializeToken(handlers) as Saml2SecurityToken;
            foreach (var item in token.Assertion.Statements)
            {
                Saml2AttributeStatement attStmt = item as Saml2AttributeStatement;
                if (attStmt != null)
                {
                    foreach (var item2 in attStmt.Attributes)
                    {
                        attributes.Add(item2.Name, item2.Values[0]);
                    }
                }

            }

            return attributes;
        }
    }
}