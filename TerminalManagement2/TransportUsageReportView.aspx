<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="TransportUsageReportView.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.TransportUsageReportView" %>

<!DOCTYPE html>

<html lang="en">
<head runat="server">
    <title>Transport Usage Report</title>
    <link rel="stylesheet" href="styles/site.css" />
    <link rel="stylesheet" href="styles/site-tablet.css" media="(max-width:768px)" />
    <link rel="stylesheet" href="styles/site-phone.css" media="(max-width:480px)" />
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:Literal ID="litReport" runat="server"></asp:Literal>
        </div>
    </form>
</body>
</html>
