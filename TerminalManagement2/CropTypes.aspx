<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="CropTypes.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.CropTypes" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <title>Crops : Crop types</title>
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
    <form id="main" method="post" runat="server" defaultfocus="tbxName">
    <div class="recordSelection">
        <label>
            Crop type
        </label>
        <asp:DropDownList ID="ddlCropTypes" runat="server" AutoPostBack="True">
        </asp:DropDownList>
    </div>
    <div class="recordControl">
        <asp:Button ID="btnSave" runat="server" Text="Save" />
        <asp:Button ID="btnDelete" runat="server" Text="Delete" />
        <asp:Label ID="lblStatus" runat="server" ForeColor="#ff0000"></asp:Label>
        <span class="sectionRequiredField"><span class="required"></span>&nbsp;indicates required
            field </span>
    </div>
    <div class="sectionEven">
        <h1>
            General</h1>
        <ul>
            <li>
                <label>
                    Name
                </label>
                <span class="required">
                    <asp:TextBox ID="tbxName" runat="server" MaxLength="50"></asp:TextBox>
                </span></li>
        </ul>
        <ul id="lstCustomFields" runat="server">
        </ul>
    </div>
    </form>
</body>
</html>
