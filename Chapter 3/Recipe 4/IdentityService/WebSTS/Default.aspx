<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="WebSTS._Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title></title>
    <style type="text/css">
        .style1
        {
            border-left-style: solid;
            border-left-width: 1px;
            border-right: 1px solid #C0C0C0;
            border-top-style: solid;
            border-top-width: 1px;
            border-bottom: 1px solid #C0C0C0;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <table class="style1">
            <tr>
                <td>
                    Name</td>
                <td>
                    <asp:TextBox ID="txtName" runat="server" Width="200px" Text="John Doe"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td>
                    UserID</td>
                <td>
                    <asp:TextBox ID="txtUserId" runat="server" Width="200px" Text="johndoe"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td>
                    Language ID</td>
                <td>
                    <asp:TextBox ID="txtLanguageId" runat="server" Width="200px" Text="en-US"></asp:TextBox>
                </td>
            </tr>
        </table>
        <br />
        <asp:Button ID="Button1" runat="server" onclick="Button1_Click" Text="Create Token" />
    </div>
    </form>
</body>
</html>
