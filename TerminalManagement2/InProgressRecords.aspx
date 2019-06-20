<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="InProgressRecords.aspx.vb"
    Inherits="KahlerAutomation.TerminalManagement2.InProgressRecords" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <title>Orders : In Progress</title>
    <link rel="shortcut icon" href="FavIcon.ico" />
    <meta name="viewport" content="width=device-width" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <link rel="stylesheet" href="styles/site.css" />
    <link rel="stylesheet" href="styles/site-tablet.css" media="(max-width:768px)" />
    <link rel="stylesheet" href="styles/site-phone.css" media="(max-width:480px)" />
    <script type="text/javascript" src="scripts/jquery-1.11.1.min.js"></script>
    <script type="text/javascript" src="scripts/soap-1.0.1.js"></script>
    <script type="text/javascript" src="scripts/page-controller.js"></script>
</head>
<body>
    <form id="main" runat="server">
    <div class="recordSelection">
        <asp:CheckBox ID="cbxShowIndividualWeighments" runat="server" Text="Show Individual Weighments"
            Checked="false" AutoPostBack="True" />
    </div>
    <div class="section">
        <asp:Literal ID="litInProgressData" runat="server"></asp:Literal>
    </div>
    </form>
</body>
</html>
