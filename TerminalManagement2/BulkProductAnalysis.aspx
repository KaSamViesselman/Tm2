<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="BulkProductAnalysis.aspx.vb"
    Inherits="KahlerAutomation.TerminalManagement2.BulkProductAnalysis" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <title>Products : Bulk Product Analysis</title>
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
<body onload="resizeIframe('analysisFrame');">
    <form id="main" method="post" runat="server" defaultfocus="tbxName">
        <div class="recordSelection">
            <span style="white-space: nowrap">
                <label for="ddlBulkProducts">
                    Bulk product&nbsp;
                </label>
                <asp:DropDownList ID="ddlBulkProducts" runat="server" AutoPostBack="True">
                </asp:DropDownList>
            </span>&nbsp;<span style="white-space: nowrap">
                <label for="ddlBulkProductAnalysis">
                    Analysis type&nbsp;
                </label>
                <asp:DropDownList ID="ddlBulkProductAnalysis" runat="server" AutoPostBack="True"
                    Enabled="False">
                </asp:DropDownList>
            </span>
        </div>
        <div class="recordControlNoFloat" id="pnlRecordControl" runat="server">
            <asp:Button ID="btnRemoveAnalysisRecord" runat="server" Text="Remove analysis" />
        </div>
        <iframe id="analysisFrame" src="" runat="server" width="100%" height="600px"></iframe>
    </form>
</body>
</html>
