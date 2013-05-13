<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="StsProcessing.aspx.cs"
    Inherits="WebSTS.StsProcessing" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>

    <script type="text/javascript">
        setTimeout('document.forms[0].submit()', 0);
    </script>

</head>
<body>
    <form id="form1" runat="server">

    <div>
    <input type="hidden" ID="SAMLResponse" runat="server"  />
    <input type="hidden" ID="RelayState" runat="server"  />
    <textarea id="txtToken" cols="120" rows="40" runat="server"></textarea>
    <button type="submit">POST</button> 
    </div>

    </form>
</body>
</html>
