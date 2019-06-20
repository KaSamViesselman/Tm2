<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="Accounts.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.Accounts" %>

<!DOCTYPE html>
<html lang="en">
<head id="Head1" runat="server">
    <title>Customer Accounts : Accounts</title>
    <link rel="shortcut icon" href="FavIcon.ico" />
    <meta name="viewport" content="width=device-width" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <link rel="stylesheet" href="styles/site.css" />
    <link rel="stylesheet" href="styles/site-tablet.css" media="(max-width:768px)" />
    <link rel="stylesheet" href="styles/site-phone.css" media="(max-width:480px)" />
    <script type="text/javascript" src="scripts/jquery-1.11.1.min.js"></script>
    <script type="text/javascript" src="scripts/soap-1.0.1.js"></script>
    <script type="text/javascript" src="scripts/page-controller.js"></script>
    <script type="text/javascript" src="jquery-ui/jquery-ui.min.js"></script>
    <script type="text/javascript" src="Scripts/TimePicker/jquery-ui-timepicker-addon.min.js"></script>
    <link rel="stylesheet" href="jquery-ui/jquery-ui.min.css" />
    <link rel="stylesheet" href="jquery-ui/jquery-ui.structure.min.css" />
    <link rel="stylesheet" href="Styles/TimePicker/jquery-ui-timepicker-addon.min.css" />
</head>
<body>
    <form id="main" method="post" runat="server" defaultfocus="ddlCustomers">
        <div class="recordSelection">
            <label>
                Account
            </label>
            <asp:DropDownList ID="ddlCustomers" runat="server" AutoPostBack="True">
            </asp:DropDownList>
        </div>
        <div class="recordControl">
            <asp:Button ID="btnSave" runat="server" Text="Save" />
            <asp:Button ID="btnDelete" runat="server" Text="Delete" />
            <asp:Label ID="lblStatus" runat="server" ForeColor="#ff0000"></asp:Label>
            <span class="sectionRequiredField"><span class="required"></span>&nbsp;indicates required
			field </span>
        </div>
        <asp:Panel CssClass="section" ID="pnlMain" runat="server">
            <div class="sectionEven">
                <h1>General</h1>
                <ul>
                    <li>
                        <label>
                            Name
                        </label>
                        <span class="required">
                            <asp:TextBox ID="tbxName" runat="server" MaxLength="50"></asp:TextBox>
                        </span></li>
                    <li>
                        <label>
                            Default cross reference
                        </label>
                        <asp:TextBox ID="tbxAcctNum" runat="server" MaxLength="50"></asp:TextBox>
                    </li>
                    <li>
                        <label>
                            Owner
                        </label>
                        <asp:DropDownList ID="ddlOwners" runat="server">
                        </asp:DropDownList>
                    </li>
                    <li>
                        <label>
                            Address
                        </label>
                        <asp:TextBox ID="tbxAddress" runat="server" MaxLength="50"></asp:TextBox>
                    </li>
                    <li>
                        <label>
                            City
                        </label>
                        <asp:TextBox ID="tbxCity" runat="server" MaxLength="50"></asp:TextBox>
                    </li>
                    <li>
                        <label>
                            State
                        </label>
                        <asp:TextBox ID="tbxState" runat="server" MaxLength="50"></asp:TextBox>
                    </li>
                    <li>
                        <label>
                            Zip code
                        </label>
                        <asp:TextBox ID="tbxZip" runat="server" MaxLength="15"></asp:TextBox>
                    </li>
                    <li>
                        <label>
                            County
                        </label>
                        <asp:TextBox ID="tbxCounty" runat="server" MaxLength="50"></asp:TextBox>
                    </li>
                    <li>
                        <label>
                            Country
                        </label>
                        <asp:TextBox ID="tbxCountry" runat="server" MaxLength="100"></asp:TextBox>
                    </li>
                    <li>
                        <label>
                            Phone
                        </label>
                        <asp:TextBox ID="tbxPhone" runat="server" MaxLength="50"></asp:TextBox>
                    </li>
                    <li>
                        <label>
                            E-mail
                        </label>
                        <asp:TextBox ID="tbxEmail" runat="server"></asp:TextBox>
                    </li>
                    <li>
                        <label>
                            Billing account number
                        </label>
                        <asp:TextBox ID="tbxBillingNum" runat="server" MaxLength="50"></asp:TextBox>
                    </li>
                    <li>
                        <label>
                            Coop
                        </label>
                        <asp:TextBox ID="tbxCoop" runat="server" MaxLength="50"></asp:TextBox>
                    </li>
                    <li>
                        <label>
                            Notes
                        </label>
                        <asp:TextBox ID="tbxNotes" runat="server" TextMode="MultiLine"></asp:TextBox>
                    </li>
                    <li>
                        <label>
                            Disabled</label>
                        <asp:CheckBox ID="cbxDisabled" runat="server" Text=""></asp:CheckBox>
                    </li>
                </ul>
                <ul id="lstCustomFields" runat="server">
                </ul>
            </div>
            <asp:Panel ID="pnlOdd" runat="server" CssClass="sectionOdd">
                <asp:Panel ID="pnlFacilities" runat="server" CssClass="section">
                    <h1>Facilities</h1>
                    <ul>
                        <li>
                            <label>
                            </label>
                            <asp:CheckBox ID="cbxValidForAllFacilities" runat="server" AutoPostBack="true" Checked="true"
                                Text="Valid for all facilities" />
                        </li>
                    </ul>
                    <ul class="addRemoveSection">
                        <li id="rowAddtoFacility" runat="server">
                            <asp:DropDownList ID="ddlFacilities" runat="server" Enabled="True" CssClass="addRemoveList" />
                            <asp:Button ID="btnAddFacility" runat="server" Text="Add facility" CssClass="addRemoveButton" />
                        </li>
                        <li>
                            <asp:ListBox runat="server" ID="lstFacility" AutoPostBack="True" CssClass="addRemoveList" />
                            <asp:Button ID="btnDeleteFromFacility" runat="server" Enabled="False" Text="Remove facility"
                                CssClass="addRemoveButton" />
                        </li>
                    </ul>
                    <asp:Button ID="btnAddAllFacilities" runat="server" Text="Add to all facilities"
                        Width="45%" />
                    <asp:Button ID="btnDeleteAllFacilities" runat="server" Text="Remove from all facilities"
                        Enabled="False" Width="45%" />
                </asp:Panel>
                <asp:Panel ID="pnlDrivers" runat="server" CssClass="section">
                    <h1>Drivers</h1>
                    <ul class="addRemoveSection">
                        <li>
                            <asp:DropDownList ID="ddlDrivers" runat="server" AutoPostBack="True" Enabled="False"
                                CssClass="addRemoveList" />
                            <asp:Button ID="btnAddDriver" runat="server" Text="Add driver" Enabled="False" CssClass="addRemoveButton" />
                        </li>
                        <li>
                            <asp:ListBox ID="lstDrivers" runat="server" Rows="6" AutoPostBack="True" Enabled="False"
                                CssClass="addRemoveList" />
                            <asp:Button ID="btnDeleteFromDriver" runat="server" Text="Remove driver" Enabled="False"
                                CssClass="addRemoveButton" />
                        </li>
                    </ul>
                    <asp:Button ID="btnAddAllDrivers" runat="server" Width="45%" Text="Add all drivers" Enabled="False" CssClass="addRemoveButton" />
                    <asp:Button ID="btnDeleteAllDrivers" runat="server" Width="45%" Text="Remove all drivers" Enabled="False" CssClass="addRemoveButton" />
                </asp:Panel>
            </asp:Panel>
        </asp:Panel>
        <div class="sectionOdd" id="pnlInterfaceSetup" runat="server">
            <h2>Interface</h2>
            <div class="recordSelectionEvenOdd">
                <label>
                    Interface setting</label>
                <asp:DropDownList ID="ddlAccountInterface" runat="server" AutoPostBack="True">
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
                        <asp:CheckBox ID="cbxDefaultSetting" runat="server" Text="Default setting" />
                    </li>
                    <li>
                        <label>
                        </label>
                        <asp:CheckBox ID="cbxExportOnly" runat="server" Text="Export only" />
                    </li>
                </ul>
                <asp:Button ID="btnSaveInterface" runat="server" Text="Save" CssClass="recordInterfaceButton" />
                <asp:Button ID="btnRemoveInterface" runat="server" Text="Remove" CssClass="recordInterfaceButton" />
            </asp:Panel>
        </div>
    </form>
</body>
</html>
