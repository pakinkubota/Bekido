<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="web1.aspx.cs" Inherits="WebApplication2.WebForm1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<link href="StyleSheet1.css" rel="stylesheet" />
<body>
    <form id="form1" runat="server">
        <div style="text-align: center">
            Hello World<br />
            <br />
            Hello FGL<br />
            <br />
            Name :
            <asp:TextBox ID="txbName" runat="server" Width="81px">Beam</asp:TextBox>
&nbsp;&nbsp;&nbsp; Age :
            <asp:TextBox ID="txbAge" runat="server" Width="74px">25</asp:TextBox>
&nbsp;&nbsp;&nbsp; Position :
            <asp:TextBox ID="txbPosition" runat="server">Engineer</asp:TextBox>
            <br />
            <br />
            <asp:Label ID="lblWarning" runat="server" ForeColor="Red"></asp:Label>
            <br />
            <br />
            <asp:Button ID="btnInsert" runat="server" OnClick="btnInsert_Click" Text="Insert" />
            <br />
            <br />
            <asp:GridView ID="gvw1" runat="server"  HorizontalAlign="Center" HeaderStyle-width="10%">
                <HeaderStyle Width="10%" />
                <RowStyle Wrap="False" />
            </asp:GridView>
            <br />
            <asp:HyperLink ID="hpl1" runat="server" NavigateUrl="http://172.20.162.105:80/fgltest/Test.aspx">test</asp:HyperLink>
        &nbsp;
            <asp:HyperLink ID="hpl2" runat="server" NavigateUrl="http://172.20.162.105:80/fgltest/Bekido_Main.aspx">Bekido</asp:HyperLink>
        </div>
    </form>
</body>
</html>
