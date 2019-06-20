<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="AllContainerEquipment.aspx.vb"
    Inherits="KahlerAutomation.TerminalManagement2.AllContainerEquipment" MaintainScrollPositionOnPostback="true" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <title>Containers : All Container Equipment Report</title>
    <link rel="shortcut icon" href="FavIcon.ico" />
    <meta name="viewport" content="width=device-width" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <link rel="stylesheet" href="styles/site.css" />
    <link rel="stylesheet" href="styles/site-tablet.css" media="(max-width:768px)" />
    <link rel="stylesheet" href="styles/site-phone.css" media="(max-width:480px)" />
    <script type="text/javascript" src="scripts/jquery-1.11.1.min.js"></script>
    <script type="text/javascript" src="scripts/soap-1.0.1.js"></script>
    <script type="text/javascript" src="scripts/page-controller.js"></script>
    <script type="text/javascript">
        function DisplayAddEmailButton(value) {
            if (value != '') document.getElementById('btnAddEmailAddress').style.visibility = 'visible';
            else document.getElementById('btnAddEmailAddress').style.visibility = 'hidden';
        }
    </script>
</head>
<body>
    <form id="main" runat="server">
        <div class="recordControl">
            <asp:Button ID="btnShowReport" runat="server" Text="Show report" />
            <asp:Button ID="btnPrinterFriendly" runat="server" Text="Printer friendly" />
            <asp:Button ID="btnDownload" runat="server" Text="Download report" />
        </div>
        <div class="section">
            <div class="section">
                <span>Filter by:</span><br />
                <asp:DropDownList ID="ddlLocation" runat="server" Width="20%">
                    <asp:ListItem Text="All facilities" Value="00000000-0000-0000-0000-000000000000" />
                </asp:DropDownList>
                <asp:DropDownList ID="ddlStatus" runat="server" Width="20%">
                    <asp:ListItem Text="Any status" Value="-1" />
                    <asp:ListItem Text="In facility" Value="0" />
                    <asp:ListItem Text="In customer custody" Value="1" />
                </asp:DropDownList>
                <asp:DropDownList ID="ddlOwner" runat="server" Width="20%">
                    <asp:ListItem Text="All owners" Value="00000000-0000-0000-0000-000000000000" />
                </asp:DropDownList>
                <asp:DropDownList ID="ddlCustomerAccount" runat="server" Width="20%">
                    <asp:ListItem Text="All customer accounts" Value="00000000-0000-0000-0000-000000000000" />
                </asp:DropDownList>
                <br />
                <asp:CheckBox ID="cbxShowDeleted" runat="server" Checked="false" Text="Show deleted"  Width="16%" />
                <span>Number:</span><asp:TextBox ID="tbxNumber" runat="server" Width="20%" />
            </div>
            <div class="sectionEven">
                <label>Sort by:</label>
                <asp:DropDownList ID="ddlOrderBy" runat="server" Width="20%">
                    <asp:ListItem Text="Number" Value="barcode_id" />
                    <asp:ListItem Text="Last inspected" Value="last_inspected" />
                    <asp:ListItem Text="Created" Value="created" />
                </asp:DropDownList>
                <asp:DropDownList ID="ddlAscDesc" runat="server" Width="20%">
                    <asp:ListItem Text="Ascending" Value="ASC" />
                    <asp:ListItem Text="Descending" Value="DESC" />
                </asp:DropDownList>
            </div>
        </div>
        <div class="section" id="pnlSendEmail" runat="server">
            <hr style="width: 100%; color: #003399;" />
            <div class="sectionOdd">
                <ul>
                    <li>
                        <label>
                            E-mail to</label>
                        <asp:TextBox ID="tbxEmailTo" Style="width: 45%;" runat="server" AutoPostBack="true" />
                        <asp:Button ID="btnSendEmail" Style="width: 15%;" runat="server" Text="Send" />
                    </li>
                    <li id="rowAddAddress" runat="server">
                        <label>
                            Add address</label>
                        <asp:DropDownList ID="ddlAddEmailAddress" runat="server" Style="width: 45%;" onchange="DisplayAddEmailButton(this.value);" />
                        <asp:Button ID="btnAddEmailAddress" runat="server" Style="width: 15%;" Text="Add"
                            Visibility="false" />
                    </li>
                    <li style="color: Red;">
                        <asp:Literal ID="litEmailConfirmation" runat="server" />
                    </li>
                </ul>
            </div>
        </div>
    </form>
</body>
</html>
