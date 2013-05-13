using System;
using System.Configuration;
using System.Web;
using System.Collections.Generic;
using Microsoft.IdentityModel.Protocols.WSIdentity;

namespace WebSTS
{
    public partial class _Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            Dictionary<string, string> claims = new Dictionary<string, string>();
            claims.Add(WSIdentityConstants.ClaimTypes.Name, txtName.Text);
            claims.Add(WSIdentityConstants.ClaimTypes.PrivatePersonalIdentifier, txtUserId.Text);
            claims.Add(WSIdentityConstants.ClaimTypes.Locality, txtLanguageId.Text);

            Saml11SecureTokenProvider provider = new Saml11SecureTokenProvider(claims);
            string token = provider.Issue();
            if (token != null)
            {
                HttpContext.Current.Items.Add("EncSamlToken", token);
                HttpContext.Current.Items.Add("TargetRp", ConfigurationManager.AppSettings["AppliesToAddress"]);
                Server.Transfer("~/StsProcessing.aspx");
            }
        }
    }
}
