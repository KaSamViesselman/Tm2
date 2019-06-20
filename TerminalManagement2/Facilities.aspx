<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="Facilities.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.Facilities" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <title>Facilities : Facilities</title>
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
                Facility</label>
            <asp:DropDownList ID="ddlFacilities" runat="server" AutoPostBack="True" />
        </div>
        <div class="recordControl">
            <asp:Button ID="btnSave" runat="server" Text="Save" />
            <asp:Button ID="btnDelete" runat="server" Text="Delete" Enabled="false" />
            <asp:Label ID="lblStatus" runat="server" ForeColor="Red" />
            <span class="sectionRequiredField"><span class="required"></span>&nbsp;indicates required
			field </span>
        </div>
        <asp:Panel ID="pnlEven" runat="server" CssClass="sectionEven">
            <h1>General</h1>
            <ul>
                <li>
                    <label>
                        Name</label>
                    <span class="required">
                        <asp:TextBox ID="tbxName" TabIndex="1" runat="server" /></span> </li>
                <li>
                    <label>
                        Address</label>
                    <asp:TextBox ID="tbxAddress" TabIndex="2" runat="server" />
                </li>
                <li>
                    <label>
                        City</label>
                    <asp:TextBox ID="tbxCity" runat="server" />
                </li>
                <li>
                    <label>
                        State</label>
                    <asp:TextBox ID="tbxState" runat="server" />
                </li>
                <li>
                    <label>
                        Zip code</label>
                    <asp:TextBox ID="tbxZip" runat="server" />
                </li>
                <li>
                    <label>
                        Country</label>
                    <asp:TextBox ID="tbxCountry" runat="server" />
                </li>
                <li>
                    <label>
                        EPA number</label>
                    <asp:TextBox ID="tbxEpaNumber" runat="server" />
                </li>
                <li>
                    <label>
                        Phone</label>
                    <asp:TextBox ID="tbxPhone" runat="server" />
                </li>
                <li>
                    <label>
                        E-mail</label>
                    <asp:TextBox ID="tbxEmail" runat="server" />
                </li>
            </ul>
            <h1>Order completion settings</h1>
            <ul>
                <li>
                    <label>
                    </label>
                    <asp:CheckBox ID="cbxUseOwnerOrderCompletionSettings" runat="server" Text="Use owner order completion settings"
                        AutoPostBack="true" />
                </li>
                <li>
                    <label>
                    </label>
                    <span class="input">
                        <asp:CheckBox ID="cbxUsePercentageToDetermineOrderCompletion" runat="server" Text="Mark orders complete at"
                            AutoPostBack="true" Checked="true" Width="60%" />
                        <asp:TextBox ID="tbxCompletionPercentage" runat="server" Width="30%" />% </span>
                </li>
                <li>
                    <label>
                    </label>
                    <asp:CheckBox ID="cbxUseBatchCountToDetermineOrderCompletion" runat="server" Text="Mark orders complete when the requested batch count is reached" />
                </li>
            </ul>
            <h1>Receiving PO completion settings</h1>
            <ul>
                <li>
                    <label>
                    </label>
                    <asp:CheckBox ID="cbxUseOwnerReceivingOrderCompletionSettings" runat="server" Text="Use owner receiving order completion settings"
                        AutoPostBack="true" />
                </li>
                <li>
                    <label>
                    </label>
                    <span class="input">
                        <asp:CheckBox ID="cbxUsePercentageToDetermineReceivingOrderCompletion" runat="server"
                            Text="Mark receiving orders complete at" AutoPostBack="true" Checked="true" Width="60%" />
                        <asp:TextBox ID="tbxReceivingOrderCompletionPercentage" runat="server" Width="30%" />%
                    </span></li>
            </ul>
            <ul id="lstCustomFields" runat="server">
            </ul>
        </asp:Panel>
        <asp:Panel cssclass="sectionOdd" ID="pnlFacilityValidIn" runat="server">
            <asp:Panel ID="pnlDrivers" runat="server" CssClass="section">
                <h1>Assigned drivers</h1>
                <ul class="addRemoveSection">
                    <li>
                        <asp:DropDownList ID="ddlDrivers" runat="server" AutoPostBack="True" Enabled="False" CssClass="addRemoveList" />
                        <asp:Button ID="btnAddDriver" runat="server" Text="Add driver" Enabled="False" CssClass="addRemoveButton" />
                    </li>
                    <li>
                        <asp:ListBox ID="lstDrivers" runat="server" Rows="6" AutoPostBack="True" Enabled="False" CssClass="addRemoveList" />
                        <asp:Button ID="btnRemoveDriver" runat="server" Text="Remove driver" Enabled="False" CssClass="addRemoveButton" />
                    </li>
                </ul>
                        <asp:Button ID="btnAddAllDrivers" runat="server" Width="45%" Text="Add all drivers" Enabled="False" CssClass="addRemoveButton" />
                        <asp:Button ID="btnRemoveAllDrivers" runat="server" Width="45%" Text="Remove all drivers" Enabled="False" CssClass="addRemoveButton" />
                            </asp:Panel>
            <asp:Panel ID="pnlAccounts" runat="server" CssClass="section">
                <h1>Assigned customer accounts</h1>
                <ul class="addRemoveSection">
                    <li>
                        <asp:DropDownList ID="ddlAccounts" runat="server" AutoPostBack="True" Enabled="False" CssClass="addRemoveList" />
                        <asp:Button ID="btnAddAccount" runat="server" Text="Add account" Enabled="False" CssClass="addRemoveButton" />
                    </li>
                    <li>
                        <asp:ListBox ID="lstAccounts" runat="server" Rows="6" AutoPostBack="True" Enabled="False" CssClass="addRemoveList" />
                        <asp:Button ID="btnRemoveAccount" runat="server" Text="Remove account" Enabled="False" CssClass="addRemoveButton" />
                    </li>
                </ul>
                        <asp:Button ID="btnAddAllAccounts" runat="server" Width="45%" Text="Add all accounts" Enabled="False" CssClass="addRemoveButton" />
                        <asp:Button ID="btnRemoveAllAccounts" runat="server" Width="45%" Text="Remove all accounts" Enabled="False" CssClass="addRemoveButton" />
                            </asp:Panel>
        </asp:Panel>
        <div class="sectionOdd" id="pnlInterfaceSetup" runat="server">
            <h2>Interface</h2>
            <div class="recordSelectionEvenOdd">
                <label>
                    Interface setting</label>
                <asp:DropDownList ID="ddlFacilitiesInterface" runat="server" AutoPostBack="True">
                </asp:DropDownList>
            </div>
            <asp:Panel ID="pnlInterfaceSettings" runat="server" CssClass="section">
                <ul>
                    <li>
                        <label>
                            Interface
                        </label>
                        <span class="required">
                            <asp:DropDownList ID="ddlInterface" runat="server" AutoPostBack="true">
                            </asp:DropDownList>
                        </span></li>
                    <li>
                        <label>
                            Cross reference
                        </label>
                        <span class="required">
                            <asp:TextBox ID="tbxInterfaceCrossReference" runat="server" MaxLength="50"></asp:TextBox>
                        </span></li>
                    <li>
                        <label>
                        </label>
                        <asp:CheckBox ID="chkDefaultSetting" runat="server" Text="Default setting" />
                    </li>
                    <li>
                        <label>
                        </label>
                        <asp:CheckBox ID="chkExportOnly" runat="server" Text="Export only" />
                    </li>
                </ul>
                <asp:Button ID="btnSaveInterface" runat="server" Text="Save" CssClass="recordInterfaceButton" />
                <asp:Button ID="btnRemoveInterface" runat="server" Text="Remove" CssClass="recordInterfaceButton" />
            </asp:Panel>
        </div>
    </form>
</body>
</html>
