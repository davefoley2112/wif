using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IdentityModel.Claims;
using System.Security.Principal;
using System.Security.Cryptography.X509Certificates;
using System.IdentityModel.Policy;

namespace WindowsIdentityToClaimsConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            //
            // A claim is used to identify a user or provide access to a particular resource requested by the user. 
            // There are three properties exposed by the Claim class:
            //
            // ClaimType: It identifies the type of claim. In our example, Sid (security identifier) and Name are the 
            //            two claim types displayed in the console window. 
            //
            // Resource: It identifies the resource associated with the claim.
            //
            // Right: It is a URI representing the Identity or PossessProperty right associated with the claim. 
            //        PossessProperty determines whether the user has the access to Resource.
            //
            // Both the Claim and the ClaimSet classes are serialization-friendly, 
            // which allows them to be transmitted over service boundaries.
            //
            using (WindowsClaimSet claims = new WindowsClaimSet(WindowsIdentity.GetCurrent()))
            {
                foreach (var claim in claims)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(string.Format("Claim Type: {0}", claim.ClaimType));
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(string.Format("Resource: {0}", claim.Resource.ToString()));
                    Console.WriteLine(string.Format("Right: {0}", claim.Right));
                    Console.WriteLine("\n");
                }
            }
            
            Console.ReadLine();
        }

        static void DummyMethodForClassDiagram()
        {
            //
            // There's more:
            //
            // In addition to WindowsClaimSet, the System.IdentityModel.Claims namespace provides a DefaultClaimSet 
            // class that allows you to create your implementation of claims, and a X509CertificateClaimSet class 
            // to abstract claims from an X.509 certificate.
            //
            var a1 = new DefaultClaimSet();
            var a2 = new X509CertificateClaimSet(new X509Certificate2());

            //
            // Authorization context:
            //
            // The System.IdentityModel.Policy namespace exposes a AuthorizationContext class that can be used to 
            // evaluate the authorization policies in a sent message. The AuthorizationContext class has a ClaimSet 
            // property that allows a service to retrieve all the claims associated with the security token in the 
            // sent message. 
            //
            // You can learn more with an example in the MSDN documentation at http://msdn.microsoft.com/en-us/library/system.identitymodel.policy.authorizationcontext.claimsets.aspx.
            //
            AuthorizationContext a3;
        }
    }
}
