<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="DefaultHazmatAnalysis.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.DefaultHazmatAnalysis" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Hazardous Material Analysis</title>
    <link rel="shortcut icon" href="FavIcon.ico" />
    <meta name="viewport" content="width=device-width" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <link rel="stylesheet" href="styles/site.css" />
    <link rel="stylesheet" href="styles/site-tablet.css" media="(max-width:768px)" />
    <link rel="stylesheet" href="styles/site-phone.css" media="(max-width:480px)" />
</head>
<body>
    <form id="form1" runat="server" height="100%">
        <div class="section">
            <h1>
                <asp:Label ID="lblLastAnalysisAt" runat="server" Text="Last Analysis At:"></asp:Label>
            </h1>
        </div>
        <div class="recordControl">
            <asp:Button ID="btnSave" runat="server" Text="Save" />
            <asp:Label ID="lblStatus" runat="server" ForeColor="Red"></asp:Label>
            <span class="sectionRequiredField"><span class="required"></span>&nbsp;indicates required
            field </span>
        </div>
        <div class="section">
            <ul>
                <li>
                    <label style="width: 15%;">
                        Basic Description 
                    </label>
                    <span class="input" style="width: 80%;"><span class="required">
                        <asp:TextBox ID="tbxDescription" runat="server" Style="width: 97%; vertical-align: text-top;" CssClass="input" TextMode="MultiLine">UN0000, Bulk Product, 6.1, PGIII, Marine Pollutant</asp:TextBox>
                    </span></span></li>
                <li style="width: 50%;">
                    <label>
                        Reportable Quantity 
                    </label>
                    <span class="input"><span class="required">
                        <asp:TextBox ID="tbxReportableQuantity" runat="server" CssClass="inputNumeric" Width="65%">0</asp:TextBox>
                        <asp:DropDownList ID="ddlUnits" runat="server" Width="30%" />
                    </span></span></li>
            </ul>
        </div>
    </form>
</body>
</html>
