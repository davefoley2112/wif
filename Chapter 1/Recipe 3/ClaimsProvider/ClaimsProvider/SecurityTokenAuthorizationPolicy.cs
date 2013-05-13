using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IdentityModel.Policy;
using System.IdentityModel.Claims;
using System.Security.Principal;

namespace ClaimsProvider
{
    public class SecurityTokenAuthorizationPolicy : IAuthorizationPolicy
    {
        WindowsClaimSet claims;

        public ClaimSet Issuer
        {
            get { return ClaimSet.System; }
        }

        public string Id
        {
            get { return Guid.NewGuid().ToString(); }
        }

        public SecurityTokenAuthorizationPolicy(WindowsClaimSet claims)
        {
            this.claims = claims;
        }

        public bool Evaluate(EvaluationContext evaluationContext, ref object state)
        {
            evaluationContext.AddClaimSet(this, claims);
            evaluationContext.RecordExpirationTime(DateTime.Now.AddHours(10));
            return true;
        }
       
    }
}
