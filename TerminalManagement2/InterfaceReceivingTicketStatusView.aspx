﻿<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="InterfaceReceivingTicketStatusView.aspx.vb"
    Inherits="KahlerAutomation.TerminalManagement2.InterfaceReceivingTicketStatusView" %>

<!DOCTYPE html>
<html lang="en">
<head id="Head1" runat="server">
    <title>Interfaces : Receiving Ticket Export Status View</title>
    <link rel="shortcut icon" href="FavIcon.ico" />
    <meta name="viewport" content="width=device-width" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <link rel="stylesheet" href="styles/site.css" />
    <link rel="stylesheet" href="styles/site-tablet.css" media="(max-width:768px)" />
    <link rel="stylesheet" href="styles/site-phone.css" media="(max-width:480px)" />
</head>
<body>
    <form id="Reports" runat="server">
    <div class="section">
        <asp:Literal ID="litReport" runat="server" EnableViewState="false"></asp:Literal>
    </div>
    </form>
</body>
</html>
