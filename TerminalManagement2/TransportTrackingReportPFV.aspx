<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="TransportTrackingReportPFV.aspx.vb"
    Inherits="KahlerAutomation.TerminalManagement2.TransportTrackingReportPFV" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <title></title>
    <link rel="shortcut icon" href="FavIcon.ico" />
    <meta name="viewport" content="width=device-width" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <link rel="stylesheet" href="styles/site.css" />
    <link rel="stylesheet" href="styles/site-tablet.css" media="(max-width:768px)" />
    <link rel="stylesheet" href="styles/site-phone.css" media="(max-width:480px)" />
</head>
<body onload="this.focus();">
    <form id="main" method="post" runat="server">
    <div class="section">
        <table id="tblTransports" border="1" cellspacing="0" runat="server" width="100%"
            enableviewstate="false">
        </table>
    </div>
    </form>
</body>
</html>
