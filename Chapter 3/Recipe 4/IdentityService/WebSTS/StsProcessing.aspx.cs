﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Web.UI.HtmlControls;

namespace WebSTS
{
    public partial class StsProcessing : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string encToken = HttpContext.Current.Items["SAMLResponse"] as string;
            string targetRp = HttpContext.Current.Items["RelayState"] as string;
            if (string.IsNullOrEmpty(encToken))
                Response.Write("error");
            else
            {
                txtToken.InnerText = encToken;
                Page.Form.Action = targetRp;
                Page.Form.Method = "POST";
                RelayState.Value = "http://localhost:8002";
                SAMLResponse.Value = encToken;
            }
        }
    }
}
