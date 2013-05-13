using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IdentityModel.Claims;
using System.ServiceModel;
using System.IdentityModel.Tokens;
using System.Security.Principal;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using System.ServiceModel.Security;

namespace ClaimsProvider
{
    [ServiceContract(Namespace = "http://ClaimsProviderService")]
    public interface IClaimsProviderService
    {
        [OperationContract]
        string GetSaml11Token();
    }

    public class ClaimsProviderService : IClaimsProviderService
    {
        public string GetSaml11Token()
        {
            var claimSets = new List<ClaimSet>(ServiceSecurityContext.Current.AuthorizationContext.ClaimSets);
            ClaimSet claimSet = claimSets.Find((Predicate<ClaimSet>)delegate(ClaimSet target)
            {
                WindowsClaimSet defaultClaimSet = target.Issuer as WindowsClaimSet;
                return defaultClaimSet != null;
            });

            var accessControlClaims = claimSet.FindClaims(ClaimTypes.Sid, Rights.PossessProperty);
            SamlAssertion assertion = Saml11Helper.CreateSamlAssertionFromUserNameClaims(accessControlClaims);
            SamlSecurityToken token = new SamlSecurityToken(assertion);
            return Saml11Helper.SerializeSamlToken(token);
        }
    }
}
