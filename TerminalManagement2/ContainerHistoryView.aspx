<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="ContainerHistoryView.aspx.vb"
    Inherits="KahlerAutomation.TerminalManagement2.ContainerHistoryView" %>

<!DOCTYPE html>
<html lang="en">
<head id="Head1" runat="server">
    <title>Containers : Container History Report</title>
    <link rel="shortcut icon" href="FavIcon.ico" />
    <link rel="shortcut icon" href="FavIcon.ico" />
    <meta name="viewport" content="width=device-width" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <link rel="stylesheet" href="styles/site.css" />
    <link rel="stylesheet" href="styles/site-tablet.css" media="(max-width:768px)" />
    <link rel="stylesheet" href="styles/site-phone.css" media="(max-width:480px)" />
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:Literal ID="litContainers" runat="server"></asp:Literal>
    </div>
    </form>
</body>
</html>
