<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Bekido_Main.aspx.cs" Inherits="WebApplication2.Bekido_Main" %>

<!DOCTYPE html>
<link href="BekidoMain.css" rel="stylesheet" />
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Bekido FGL</title>
    <link rel="icon" type="image/x-icon" href="https://cdn-icons-png.flaticon.com/512/11068/11068821.png">
</head>
<body runat="server">
    <div class = "row">
        <asp:HyperLink ID="hplCell1" runat="server" ForeColor="White" NavigateUrl="http://172.20.162.105/fgltest/Bekido_Cell_1.aspx"> Cell1 </asp:HyperLink>
        <asp:HyperLink ID="hplCell2" runat="server" ForeColor="White" NavigateUrl="http://172.20.162.105/fgltest/Bekido_Cell_2.aspx"> Cell2 </asp:HyperLink>
    </div>
    <div class = "row">
        <asp:HyperLink ID="hplCell3" runat="server" ForeColor="White" NavigateUrl="http://172.20.162.105/fgltest/Bekido_Cell_3.aspx"> Cell3 </asp:HyperLink>
        <asp:HyperLink ID="hplCell4" runat="server" ForeColor="White" NavigateUrl="http://172.20.162.105/fgltest/Bekido_Cell_4.aspx"> Cell4 </asp:HyperLink>
    </div>
    <div class = "row">
        <asp:HyperLink ID="hplMD" runat="server" ForeColor="White" NavigateUrl="http://172.20.162.105/fgltest/Bekido_M_Dozer.aspx"> M-Dozer </asp:HyperLink>
        <asp:HyperLink ID="hplLD" runat="server" ForeColor="White" NavigateUrl="http://172.20.162.105/fgltest/Bekido_L_Dozer.aspx"> L-Dozer </asp:HyperLink>
    </div>
    <div class = "row">
        <asp:HyperLink ID="hplAll" runat="server" ForeColor="White" NavigateUrl="http://172.20.162.105/fgltest/Bekido_All.aspx"> All </asp:HyperLink>
    </div>
</body>
</html>
