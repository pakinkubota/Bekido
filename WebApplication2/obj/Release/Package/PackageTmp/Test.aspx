<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Test.aspx.cs" Inherits="WebApplication2.Test" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div style="text-align: center">
            Hello FGL<br />
            Beam<br />
            Pakin.P<br />
            <asp:HyperLink ID="HyperLink1" runat="server" NavigateUrl="http://172.20.162.105:80/fgltest/web1.aspx">web1</asp:HyperLink>
            <br />
            <asp:Button ID="btnDog" runat="server" Text="Dog" OnClick="btnDog_Click" />
            <asp:Button ID="btnCat" runat="server" Text="Cat" OnClick="btnCat_Click" />
            <asp:Button ID="btnBird" runat="server" Text="Bird" OnClick="btnBird_Click" />
            <br />
            <embed src="http://172.20.162.105/fgltest/Bekido_M_Dozer" style="width:1280px; height: 720px;" />
            <asp:Image ID="imgShow" runat="server" ImageUrl="~/Pic/Cat.png" />
        </div>
    </form>
</body>
</html>
