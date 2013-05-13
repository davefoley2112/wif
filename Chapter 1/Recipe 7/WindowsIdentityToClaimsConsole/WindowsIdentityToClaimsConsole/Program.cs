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
            using (WindowsClaimSet claims = new WindowsClaimSet(WindowsIdentity.GetCurrent()))
            {
                foreach (var claim in claims)
                {
                    Console.WriteLine(string.Format("Claim Type: {0}", claim.ClaimType));
                    Console.WriteLine(string.Format("Resource: {0}", claim.Resource.ToString()));
                    Console.WriteLine(string.Format("Right: {0}", claim.Right));
                    Console.WriteLine("**********************************************");
                }
            }
            
            Console.ReadLine();
        }
    }
}
