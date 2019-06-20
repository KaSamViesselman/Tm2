<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="ProductGroups.aspx.vb"
    Inherits="KahlerAutomation.TerminalManagement2.ProductGroups" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <title>Products : Product Groups</title>
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
        <label>
            Product Group
        </label>
        <asp:DropDownList ID="ddlProductGroups" runat="server" Width="244px" AutoPostBack="True">
        </asp:DropDownList>
    </div>
    <div class="recordControl">
        <asp:Button ID="btnSave" runat="server" Text="Save" />
        <asp:Button ID="btnDelete" runat="server" Text="Delete" Enabled="False" />
        <asp:Label ID="lblStatus" runat="server" ForeColor="Red"></asp:Label>
        <span class="sectionRequiredField"><span class="required"></span>&nbsp;indicates required
            field </span>
    </div>
    <div class="sectionEven">
        <h1>
            General</h1>
        <ul>
            <li>
                <label>
                    Name</label>
                <span class="required">
                    <asp:TextBox ID="tbxName" TabIndex="1" runat="server" Width="244px"></asp:TextBox>
                </span></li>
        </ul>
    </div>
    </form>
</body>
</html>
