using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

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

            var rv = Request.Params["SAMLResponse"];
            var tokenConsumer = new Saml20SecureTokenConsumer(Server.HtmlDecode(rv), "IdentityServiceConfig");
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
            }

            this.form1.Controls.Add(table);
        }
    }
}
