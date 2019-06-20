<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="Bays.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.Bays" %>

<!DOCTYPE html>
<html lang="en">
<head>
    <title>Facilities : Bays</title>
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
    <form id="main" method="post" runat="server" defaultfocus="tbxname">
    <div class="recordSelection">
        <label>
            Bay</label>
        <asp:DropDownList ID="ddlBays" runat="server" AutoPostBack="True" />
    </div>
    <div class="recordControl">
        <asp:Button ID="btnSave" runat="server" Text="Save" />
        <asp:Button ID="btnDelete" runat="server" Text="Delete" Enabled="false" />
        <asp:Label ID="lblStatus" runat="server" ForeColor="Red" />
        <span class="sectionRequiredField"><span class="required"></span>&nbsp;indicates required
            field </span>
    </div>
    <div class="sectionEven">
        <ul>
            <li>
                <label>
                    Name</label>
                <span class="required">
                    <asp:TextBox ID="tbxName" TabIndex="1" runat="server" /></span> </li>
            <li>
                <label>
                    Facility</label>
                <span class="required">
                    <asp:DropDownList ID="ddlFacilities" runat="server" />
                </span></li>
            <li>
                <label>
                </label>
                <asp:CheckBox ID="cbxStagedOrdersReturnToScale" runat="server" Text="Staged orders return to scale" />
            </li>
        </ul>
        <ul id="lstCustomFields" runat="server">
        </ul>
    </div>
    </form>
</body>
</html>
