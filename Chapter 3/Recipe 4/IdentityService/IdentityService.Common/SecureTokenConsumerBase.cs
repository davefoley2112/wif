using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using Microsoft.IdentityModel.Claims;
using Microsoft.IdentityModel.Tokens;

namespace IdentityService.Common
{
    public abstract class SecureTokenConsumerBase
    {
        protected abstract SecurityTokenHandlerCollection GetTokenHandlerCollection();

        protected abstract SecurityToken DeserializeToken(SecurityTokenHandlerCollection handlers);

        public virtual Dictionary<string, string> ParseAttributesFromSecureToken()
        {
            Dictionary<string, string> attributes = new Dictionary<string, string>();
            var handlers = GetTokenHandlerCollection();
            var token = DeserializeToken(handlers);
            ClaimsIdentityCollection idc = handlers.ValidateToken(token);
            foreach (var claimsIdentity in idc)
            {
                foreach (var claim in claimsIdentity.Claims)
                {
                    attributes.Add(claim.ClaimType, claim.Value);
                }
            }

            return attributes;
        }
    }
}
