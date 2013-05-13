using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Security;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;

namespace ClaimsProvider
{
    public class ClaimsTokenManager : ServiceCredentialsSecurityTokenManager
    {
        ClaimsProviderServiceCredentials claimsProviderServiceCredentials;

        public ClaimsTokenManager(ClaimsProviderServiceCredentials claimsProviderServiceCredentials)
            : base(claimsProviderServiceCredentials)
        {
            this.claimsProviderServiceCredentials = claimsProviderServiceCredentials;
        }

        public override SecurityTokenAuthenticator CreateSecurityTokenAuthenticator(SecurityTokenRequirement tokenRequirement, out SecurityTokenResolver outOfBandTokenResolver)
        {
            if (tokenRequirement.TokenType ==  SecurityTokenTypes.UserName)
            {
                outOfBandTokenResolver = null;
                return new ClaimsTokenAuthenticator();
            }
            else
            {
                return base.CreateSecurityTokenAuthenticator(tokenRequirement, out outOfBandTokenResolver);
            }
        }
    }
}
