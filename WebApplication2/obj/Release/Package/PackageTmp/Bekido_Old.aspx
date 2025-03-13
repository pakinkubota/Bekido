<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Bekido_Old.aspx.cs" Inherits="WebDataHour.Bekido_Old" %>
<style>
.firstrow span {
    align-items: center;
    justify-content: center;
}

.secondrow span {
    align-items: center;
    justify-content: center;
}

</style>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">

<link href="Bekido.css" rel="stylesheet" />

<head runat="server">
    <title></title>
</head>

<body>
    
    <form id="form1" runat="server">
    <div class = "top">
        <span>
            <asp:Label ID="lblLineName" runat="server" Text=""></asp:Label>
            <asp:Label ID="lblStatus" runat="server" Text=" Status"></asp:Label>
            <asp:HyperLink ID="hplMain" runat="server" ForeColor="White" NavigateUrl="http://172.20.162.105/fgltest/Bekido_Main.aspx"> Main </asp:HyperLink>
        </span>
        <span id="timeNow">Time&nbsp;</span>
        <script type="text/javascript" src="Clock.js"></script>
    </div>
    <div class ="plan firstrow">
        <div class="firstrow_text">
            Plan
        </div>
        <asp:Label ID="lblPlan" runat="server" Text="0" CssClass="firstrow_label"></asp:Label>
    </div>
    <div class ="target firstrow">
        <div class="firstrow_text">
            Target
        </div>
        <asp:Label ID="lblTarget" runat="server" Text="0" CssClass="firstrow_label"></asp:Label>
    </div>
    <div class ="actual firstrow">
        <div class="firstrow_text">
            Actual
        </div>
        <asp:Label ID="lblActual" runat="server" Text="0" CssClass="firstrow_label"></asp:Label>
    </div>
    <div class ="bekido secondrow">
        <div class="bekido_text">
            Bekido
        </div>
        <asp:Label ID="lblBekido" runat="server" Text="lblBekido" CssClass="bekido_label"></asp:Label>
    </div>
    <div class ="detail secondrow">
            <asp:GridView ID="gvwDetail" runat="server" CssClass="detail_table" AutoGenerateColumns="False" CellPadding="0" Height="100%" Width="100%" GridLines="None">
                <AlternatingRowStyle CssClass="row_even" />
                <Columns>
                    <asp:BoundField DataField="Time" HeaderText="Time" SortExpression="Time" ItemStyle-Width ="30%" HeaderStyle-Width="30%">
<HeaderStyle Width="30%"></HeaderStyle>

<ItemStyle Width="30%"></ItemStyle>
                    </asp:BoundField>
                    <asp:BoundField DataField="Target" HeaderText="Target" SortExpression="Target" ItemStyle-Width ="15%" HeaderStyle-Width="15%">
<HeaderStyle Width="15%"></HeaderStyle>

<ItemStyle Width="15%"></ItemStyle>
                    </asp:BoundField>
                    <asp:BoundField DataField="Actual" HeaderText="Actual" SortExpression="Actual" ItemStyle-Width ="15%" HeaderStyle-Width="15%">
<HeaderStyle Width="15%"></HeaderStyle>

<ItemStyle Width="15%"></ItemStyle>
                    </asp:BoundField>
                    <asp:BoundField DataField="Bekido" HeaderText="Bekido" SortExpression="Bekido" ItemStyle-Width ="20%" HeaderStyle-Width="20%">
<HeaderStyle Width="20%"></HeaderStyle>
<ItemStyle Width="20%" HorizontalAlign="Right"></ItemStyle>
                    </asp:BoundField>
                    <asp:BoundField DataField="ST(Stop Max)" HeaderText="ST(Stop Max)" SortExpression="ST(Stop Max)" ItemStyle-Width ="20%" HeaderStyle-Width="20%">
<HeaderStyle Width="20%"></HeaderStyle>
<ItemStyle Width="20%" ></ItemStyle>
                    </asp:BoundField>
                </Columns>
                <HeaderStyle CssClass="detail_header" Wrap="False" Height="6.4%" />
                <RowStyle CssClass="row_odd"  HorizontalAlign="Center" Height="7.8%"/>
            </asp:GridView>
            <asp:ScriptManager ID="smg1" runat="server">
            </asp:ScriptManager>
                    <asp:Timer ID="tim1" runat="server" OnTick="Timer1_Tick" Interval="10000">
            </asp:Timer>
    </div>
    </form>
</body>
</html>
