//-----------------------------------------------------------------------------
//
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//
//
//-----------------------------------------------------------------------------


using System;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Web.Configuration;

using Microsoft.IdentityModel.Claims;
using Microsoft.IdentityModel.Configuration;
using Microsoft.IdentityModel.Protocols.WSTrust;
using Microsoft.IdentityModel.SecurityTokenService;
using System.IdentityModel.Claims;
using System.Security.Principal;

/// <summary>
/// A custom SecurityTokenService implementation.
/// </summary>
public class CustomSecurityTokenService : SecurityTokenService
{
    // TODO: Set enableAppliesToValidation to true to enable only the RP Url's specified in the PassiveRedirectBasedClaimsAwareWebApps array to get a token from this STS
    static bool enableAppliesToValidation = false;

    // TODO: Add relying party Url's that will be allowed to get token from this STS
    static readonly string[] PassiveRedirectBasedClaimsAwareWebApps = { /*"https://localhost/PassiveRedirectBasedClaimsAwareWebApp"*/ };

    /// <summary>
    /// Creates an instance of CustomSecurityTokenService.
    /// </summary>
    /// <param name="configuration">The SecurityTokenServiceConfiguration.</param>
    public CustomSecurityTokenService( SecurityTokenServiceConfiguration configuration )
        : base( configuration )
    {
    }

    /// <summary>
    /// Validates appliesTo and throws an exception if the appliesTo is null or contains an unexpected address.
    /// </summary>
    /// <param name="appliesTo">The AppliesTo value that came in the RST.</param>
    /// <exception cref="ArgumentNullException">If 'appliesTo' parameter is null.</exception>
    /// <exception cref="InvalidRequestException">If 'appliesTo' is not valid.</exception>
    void ValidateAppliesTo( EndpointAddress appliesTo )
    {
        if ( appliesTo == null )
        {
            throw new ArgumentNullException( "appliesTo" );
        }

        // TODO: Enable AppliesTo validation for allowed relying party Urls by setting enableAppliesToValidation to true. By default it is false.
        if ( enableAppliesToValidation )
        {
            bool validAppliesTo = false;
            foreach ( string rpUrl in PassiveRedirectBasedClaimsAwareWebApps )
            {
                if ( appliesTo.Uri.Equals( new Uri( rpUrl ) ) )
                {
                    validAppliesTo = true;
                    break;
                }
            }

            if ( !validAppliesTo )
            {
                throw new InvalidRequestException( String.Format( "The 'appliesTo' address '{0}' is not valid.", appliesTo.Uri.OriginalString ) );
            }
        }
    }

    /// <summary>
    /// This method returns the configuration for the token issuance request. The configuration
    /// is represented by the Scope class. In our case, we are only capable of issuing a token for a
    /// single RP identity represented by the EncryptingCertificateName.
    /// </summary>
    /// <param name="principal">The caller's principal.</param>
    /// <param name="request">The incoming RST.</param>
    /// <returns>The scope information to be used for the token issuance.</returns>
    protected override Scope GetScope( IClaimsPrincipal principal, RequestSecurityToken request )
    {
        ValidateAppliesTo( request.AppliesTo );

        //
        // Note: The signing certificate used by default has a Distinguished name of "CN=STSTestCert",
        // and is located in the Personal certificate store of the Local Computer. Before going into production,
        // ensure that you change this certificate to a valid CA-issued certificate as appropriate.
        //
        Scope scope = new Scope( request.AppliesTo.Uri.OriginalString, SecurityTokenServiceConfiguration.SigningCredentials );

        string encryptingCertificateName = WebConfigurationManager.AppSettings[ "EncryptingCertificateName" ];
        if ( !string.IsNullOrEmpty( encryptingCertificateName ) )
        {
            // Important note on setting the encrypting credentials.
            // In a production deployment, you would need to select a certificate that is specific to the RP that is requesting the token.
            // You can examine the 'request' to obtain information to determine the certificate to use.
            scope.EncryptingCredentials = new X509EncryptingCredentials( CertificateUtil.GetCertificate( StoreName.My, StoreLocation.LocalMachine, encryptingCertificateName ) );
        }
        else
        {
            // If there is no encryption certificate specified, the STS will not perform encryption.
            // This will succeed for tokens that are created without keys (BearerTokens) or asymmetric keys.  
            scope.TokenEncryptionRequired = false;            
        }

        // Set the ReplyTo address for the WS-Federation passive protocol (wreply). This is the address to which responses will be directed. 
        // In this template, we have chosen to set this to the AppliesToAddress.
        scope.ReplyToAddress = scope.AppliesToAddress;

        return scope;
    }


    /// <summary>
    /// This method returns the claims to be issued in the token.
    /// </summary>
    /// <param name="principal">The caller's principal.</param>
    /// <param name="request">The incoming RST, can be used to obtain addtional information.</param>
    /// <param name="scope">The scope information corresponding to this request.</param> 
    /// <exception cref="ArgumentNullException">If 'principal' parameter is null.</exception>
    /// <returns>The outgoing claimsIdentity to be included in the issued token.</returns>
    protected override IClaimsIdentity GetOutputClaimsIdentity( IClaimsPrincipal principal, RequestSecurityToken request, Scope scope )
    {
        if ( null == principal )
        {
            throw new ArgumentNullException( "principal" );
        }

        ClaimsIdentity outputIdentity = new ClaimsIdentity();
        outputIdentity.Claims.Add(new Microsoft.IdentityModel.Claims.Claim(System.IdentityModel.Claims.ClaimTypes.Name, principal.Identity.Name));
        //Fetch the groups associated with the AD user and create roles
        WindowsClaimSet claimSet = new WindowsClaimSet(new WindowsIdentity(principal.Identity.Name), true);
        var accessControlClaims = claimSet.FindClaims(System.IdentityModel.Claims.ClaimTypes.Sid, Rights.PossessProperty);
        foreach (var claim in accessControlClaims)
        {
            outputIdentity.Claims.Add(new
                Microsoft.IdentityModel.Claims.Claim(
                Microsoft.IdentityModel.Claims.ClaimTypes.Role,
                GetResourceFromSid(claim.Resource as SecurityIdentifier)));
        }

        return outputIdentity;
        //The code block above is for illustration purpose only to display the named identifiers. In a real time scenario the following method can be used:
        //return WindowsClaimsIdentity.CreateFromUpn(principal.Identity.Name, AuthenticationTypes.Windows, false);
    }

    private static string GetResourceFromSid(SecurityIdentifier sid)
    {
        try
        {
            return sid.Translate(typeof(NTAccount)).ToString();
        }
        catch (Exception)
        {
            return sid.Value;
        }
    }
}
