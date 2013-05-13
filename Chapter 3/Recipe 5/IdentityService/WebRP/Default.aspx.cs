using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using Microsoft.IdentityModel.Claims;
using System.Threading;
using Microsoft.IdentityModel.Protocols.WSIdentity;
using Microsoft.IdentityModel.WindowsTokenService;
using System.ServiceModel.Security;
using System.Security.Principal;

namespace WebRP
{
    public partial class _Default : System.Web.UI.Page
    {
        Dictionary<string, string> _claimList;

        protected void Page_Load(object sender, EventArgs e)
        {
            
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            var rv = Request.Params["wresult"];
            Saml11SecureTokenConsumer tokenConsumer = new Saml11SecureTokenConsumer(Server.HtmlDecode(rv), "IdentityServiceConfig");
            _claimList = tokenConsumer.ParseAttributesFromSecureToken();
            HtmlTable table = new HtmlTable();
            table.Border = 1;

            foreach (var item in _claimList)
            {
                HtmlTableRow row = new HtmlTableRow();
                HtmlTableCell cell1 = new HtmlTableCell();
                cell1.InnerText = item.Key;

                HtmlTableCell cell2 = new HtmlTableCell();
                cell2.InnerText = item.Value;

                row.Controls.Add(cell1);
                row.Controls.Add(cell2);

                table.Controls.Add(row);

                if (item.Key == WSIdentityConstants.ClaimTypes.Upn)
                {
                    try
                    {
                        var windowsIdentity = S4UClient.UpnLogon(item.Value);
                        using (WindowsImpersonationContext context = windowsIdentity.Impersonate())
                        {
                            lblImpersonationMessage.Text = WindowsIdentity.GetCurrent().Name;
                            context.Undo();
                        }
                        lblLoginMessage.Text = WindowsIdentity.GetCurrent().Name;
                    }
                    catch (SecurityAccessDeniedException)
                    {
                        lblLoginMessage.Text = "Access Denied";
                        lblImpersonationMessage.Text = "Impersonation Failed";
                    }

                }
            }

            this.form1.Controls.Add(table);
            HtmlTextArea saml = new HtmlTextArea();
            saml.Value = tokenConsumer.ResolvedToken;
            saml.Rows = 40;
            saml.Cols = 120;
            this.form1.Controls.Add(saml);
        }
    }
}
