using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IdentityModel.Claims;
using System.Security.Principal;

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
                    Console.WriteLine(string.Format("Claim Type: {0}", claim.ClaimType));
                    Console.WriteLine(string.Format("Resource: {0}", claim.Resource.ToString()));
                    Console.WriteLine(string.Format("Right: {0}", claim.Right));
                    Console.WriteLine("\n");
                }
            }
            
            Console.ReadLine();
        }
    }
}
