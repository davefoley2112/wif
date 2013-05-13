using System.Configuration;
using Microsoft.IdentityModel.Configuration;
using System;
using Microsoft.IdentityModel.Claims;
using System.Threading;
using System.Security.Principal;

namespace IdentityModelConfiguration
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Configuration.Configuration appConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var identityModelConfiguration = (MicrosoftIdentityModelSection)appConfig.GetSection("microsoft.identityModel");
            string identityConfig = identityModelConfiguration.SectionInformation.GetRawXml();

            IClaimsPrincipal claimsPrincipal = WindowsClaimsPrincipal.CreateFromWindowsIdentity(WindowsIdentity.GetCurrent());
            IClaimsIdentity claimsIdentity = (IClaimsIdentity)claimsPrincipal.Identity;
            var nameTypeClaim = claimsIdentity.NameClaimType;
            if (identityConfig.Contains(nameTypeClaim))
                Console.WriteLine("Claim Type {0} supported for identity {1}", claimsIdentity.NameClaimType, claimsIdentity.Name);
            Console.ReadLine();
        }
    }
}
