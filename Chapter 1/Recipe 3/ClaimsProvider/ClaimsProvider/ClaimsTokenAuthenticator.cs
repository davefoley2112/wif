using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IdentityModel.Selectors;
using System.Collections.ObjectModel;
using System.IdentityModel.Policy;
using System.IdentityModel.Claims;
using System.Security.Principal;

namespace ClaimsProvider
{
    public class ClaimsTokenAuthenticator : WindowsUserNameSecurityTokenAuthenticator
    {
        protected override ReadOnlyCollection<IAuthorizationPolicy> ValidateUserNamePasswordCore(string userName, string password)
        {
            if (IsAuthenticated(userName, password))
            {
                string name = userName.Split('\\')[1];
                List<IAuthorizationPolicy> policies = new List<IAuthorizationPolicy>(1);
                WindowsClaimSet claimSet = new WindowsClaimSet(new WindowsIdentity(name), true);
                policies.Add(new SecurityTokenAuthorizationPolicy(claimSet));
                return policies.AsReadOnly();
            }
            return null;
        }

        private bool IsAuthenticated(string userName, string password)
        {
            //TODO: Call the provider to validate the credentials
            return true;
        }
    }
}
