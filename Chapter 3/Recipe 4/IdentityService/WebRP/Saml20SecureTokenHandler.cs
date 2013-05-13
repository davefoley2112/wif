using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens;
using System.Xml;
using System.Xml.XPath;
using Microsoft.IdentityModel.Tokens.Saml2;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Configuration;

namespace WebRP
{
    public class Saml20SecureTokenHandler : SecurityTokenHandler
    {
        static string[] _tokenTypeIdentifiers = null;

        static Saml20SecureTokenHandler()
        {
            _tokenTypeIdentifiers = new string[]
            { 
                "urn:oasis:names:tc:SAML:2.0:protocol", "http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-protocol-1.1#SAMLV2.0"
            };
        }

        public override string[] GetTokenTypeIdentifiers()
        {
            return _tokenTypeIdentifiers;
        }

        public override Type TokenType
        {
            get { return typeof(MySaml20SecurityToken); }
        }

        public override bool CanReadToken(XmlReader reader)
        {
            return (reader != null && reader.IsStartElement("Response", "urn:oasis:names:tc:SAML:2.0:protocol")) ? true : false;
        }

        public override SecurityToken ReadToken(XmlReader reader)
        {
            string assertionXML = null;

            try
            {
                Saml2SecurityTokenHandler saml2Handler = new Saml2SecurityTokenHandler();
                XmlDictionaryReader reader2 = XmlDictionaryReader.CreateDictionaryReader(reader);
                reader2.ReadToDescendant("Assertion", "urn:oasis:names:tc:SAML:2.0:assertion");
                assertionXML = reader2.ReadOuterXml();
                XmlReader reader3 = XmlReader.Create(new StringReader(assertionXML));
                XmlDocument signedXml = new XmlDocument();
                signedXml.Load(reader3);
                XmlReader reader4 = XmlReader.Create(new StringReader(signedXml.OuterXml));

                return base.ContainingCollection.ReadToken(reader4);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Can't validate token", ex);
            }
        }
    }

    public class MySaml20SecurityToken : SecurityToken
    {
        Saml2SecurityToken _token;
        public MySaml20SecurityToken(Saml2SecurityToken samlToken)
        {
            _token = samlToken;
        }

        public MySaml20SecurityToken()
        {
        }

        public Saml2SecurityToken Token
        {
            get { return _token; }
        }

        public override string Id
        {
            get { throw new NotImplementedException(); }
        }

        public override System.Collections.ObjectModel.ReadOnlyCollection<SecurityKey> SecurityKeys
        {
            get { throw new NotImplementedException(); }
        }

        public override DateTime ValidFrom
        {
            get { throw new NotImplementedException(); }
        }

        public override DateTime ValidTo
        {
            get { throw new NotImplementedException(); }
        }
    }
}