<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Manual_Add.aspx.cs" Inherits="WebApplication2.Manual_Add" %>

<!DOCTYPE html>
<link href="Bekido.css" rel="stylesheet" />
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body >
    <form id="form1" runat="server">
        <div class="center">
            <asp:Button ID="btnAddTarget4" runat="server" Text="Add Target 4" />
            <asp:Button ID="btnAddActual4" runat="server" Text="Add Actual 4" />
        </div>
        <div class="center">
            <asp:Button ID="Button3" runat="server" Text="Add Target 5" />
            <asp:Button ID="Button4" runat="server" Text="Add Actual 5" />
        </div>
        <div class="center">
            <asp:HyperLink ID="hplDisplay" runat="server" NavigateUrl="http://172.20.162.105/fgltest/Bekido.aspx">Display</asp:HyperLink>
        </div>
    </form>
</body>
</html>
