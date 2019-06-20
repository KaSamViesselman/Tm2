<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="ApplicationUsageView.aspx.vb"
    Inherits="KahlerAutomation.TerminalManagement2.ApplicationUsageView" %>
<!DOCTYPE html>
<html lang="en">
<head id="Head1" runat="server">
    <title>Application Usage</title>
    <link rel="shortcut icon" href="FavIcon.ico" />
    <link href="style.css" type="text/css" rel="stylesheet" />
    <link href="borders.css" type="text/css" rel="Stylesheet" />
</head>
<body>
    <form id="Reports" runat="server">
    <div>
        <asp:Literal ID="litReport" runat="server"></asp:Literal>
    </div>
    </form>
</body>
</html>
