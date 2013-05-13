using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Description;
using System.IdentityModel.Selectors;

namespace ClaimsProvider
{
    public class ClaimsProviderServiceCredentials : ServiceCredentials
    {
        public ClaimsProviderServiceCredentials()
            : base()
        {
        }

        protected override ServiceCredentials CloneCore()
        {
            return new ClaimsProviderServiceCredentials();
        }

        public override SecurityTokenManager CreateSecurityTokenManager()
        {
            return new ClaimsTokenManager(this);
        }
    }
}
