<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="Drivers.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.Drivers" %>

<!DOCTYPE html>
<html lang="en">
<head id="Head1" runat="server">
	<title>Drivers : Drivers</title>
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
	<form id="main" method="post" runat="server" defaultfocus="ddlDrivers">
		<div class="recordSelection">
			<label>
				Driver
			</label>
			<asp:DropDownList ID="ddlDrivers" runat="server" AutoPostBack="True">
			</asp:DropDownList>
		</div>
		<div class="recordControl">
			<asp:Button ID="btnSave" runat="server" Text="Save" />
			<asp:Button ID="btnDelete" runat="server" Text="Delete" />
			<asp:Label ID="lblStatus" runat="server" ForeColor="#ff0000"></asp:Label>
			<span class="sectionRequiredField"><span class="required"></span>&nbsp;indicates required field </span>
		</div>
		<asp:Panel ID="pnlGeneral" runat="server" CssClass="section">
			<div class="sectionEven" id="pnlMain" runat="server">
				<h1>General</h1>
				<ul>
					<li>
						<label>
							Name
						</label>
						<span class="required">
							<asp:TextBox ID="tbxName" runat="server"></asp:TextBox>
						</span></li>
					<li>
						<label>
							Number
						</label>
						<asp:TextBox ID="tbxNumber" runat="server"></asp:TextBox>
					</li>
					<li>
						<label>
							Street
						</label>
						<asp:TextBox ID="tbxStreet" runat="server"></asp:TextBox>
					</li>
					<li>
						<label>
							City
						</label>
						<asp:TextBox ID="tbxCity" runat="server"></asp:TextBox>
					</li>
					<li>
						<label>
							State
						</label>
						<asp:TextBox ID="tbxState" runat="server"></asp:TextBox>
					</li>
					<li>
						<label>
							Zip code
						</label>
						<asp:TextBox ID="tbxZip" runat="server"></asp:TextBox>
					</li>
					<li>
						<label>
							Country
						</label>
						<asp:TextBox ID="tbxCountry" runat="server"></asp:TextBox>
					</li>
					<li>
						<label>
							Phone
						</label>
						<asp:TextBox ID="tbxPhone" runat="server"></asp:TextBox>
					</li>
					<li>
						<label>
							E-mail
						</label>
						<asp:TextBox ID="tbxEmail" runat="server"></asp:TextBox>
					</li>
					<li>
						<label>
							Notes
						</label>
						<asp:TextBox ID="tbxNotes" runat="server"></asp:TextBox>
					</li>
					<li>
						<label>
							Password
						</label>
						<asp:TextBox ID="tbxPassword" runat="server"></asp:TextBox>
					</li>
					<li>
						<label>
						</label>
						<asp:CheckBox ID="cbxDisabled" runat="server" Text="Disabled" />
					</li>
				</ul>
				<ul id="lstCustomFields" runat="server">
				</ul>
			</div>
			<div class="sectionEven">
				<h1>Accounts</h1>
				<ul>
					<li>
						<label>
						</label>
						<asp:CheckBox ID="chkValidForAllAccounts" runat="server" AutoPostBack="true" Checked="true" Text="Valid for all accounts" />
					</li>
				</ul>
				<ul class="addRemoveSection">
					<li id="rowAddToAccount" runat="server">
						<asp:DropDownList ID="ddlAccounts" runat="server" Enabled="True" CssClass="addRemoveList" />
						<asp:Button ID="btnAddAccount" runat="server" Text="Add" CssClass="addRemoveButton" />
					</li>
					<li>
						<asp:ListBox runat="server" ID="lstAccount" AutoPostBack="True" CssClass="addRemoveList" />
						<asp:Button ID="btnDeleteFromAccount" runat="server" Enabled="False" Text="Remove" CssClass="addRemoveButton" />
					</li>
				</ul>
				<asp:Button ID="btnAddAllAccounts" runat="server" Text="Add to all accounts" Width="45%" />
				<asp:Button ID="btnDeleteAllAccounts" runat="server" Text="Remove from all accounts" Enabled="False" Width="45%" />
			</div>
			<div class="sectionEven">
				<h1>Facilities</h1>
				<ul>
					<li>
						<label>
						</label>
						<asp:CheckBox ID="chkValidForAllFacilities" runat="server" AutoPostBack="true" Checked="true" Text="Valid for all facilities" />
					</li>
				</ul>
				<ul class="addRemoveSection">
					<li id="rowAddtoFacility" runat="server">
						<asp:DropDownList ID="ddlFacilities" runat="server" Enabled="True" CssClass="addRemoveList" />
						<asp:Button ID="btnAddFacility" runat="server" Text="Add" CssClass="addRemoveButton" />
					</li>
					<li>
						<asp:ListBox runat="server" ID="lstFacility" AutoPostBack="True" CssClass="addRemoveList" />
						<asp:Button ID="btnDeleteFromFacility" runat="server" Enabled="False" Text="Remove" CssClass="addRemoveButton" />
					</li>
				</ul>
				<asp:Button ID="btnAddAllFacilities" runat="server" Text="Add to all facilities" Width="45%" />
				<asp:Button ID="btnDeleteAllFacilities" runat="server" Text="Remove from all facilities" Enabled="False" Width="45%" />
			</div>
			<div id="pnlSelfServePermissions" runat="server" class="sectionEven">
				<h1>Self serve permissions
				</h1>
				<ul>
					<li>
						<label>
							&nbsp;
						</label>
						<asp:CheckBox ID="cbxSelfServePartialOrdersAllowed" runat="server" Text="Can dispense partial orders" />
					</li>
					<li>
						<label>
							&nbsp;							
						</label>
						<asp:CheckBox ID="cbxSelfServeHandAddsAllowed" runat="server" Text="Can dispense hand add products" />
					</li>
				</ul>
			</div>
			<div id="pnlLastInFacilityInfo" runat="server" class="sectionEven">
				<h1>In facility
				</h1>
				<ul>
					<li id="liLastInFacilityLocation" runat="server">
						<label>
							Facility
						</label>
						<asp:Label CssClass="input" ID="lblLastInFacilityLocation" runat="server"></asp:Label>
					</li>
					<li id="liLastEnteredFacility" runat="server">
						<label>Last entered</label>
						<asp:Label ID="lblLastEnteredFacility" runat="server"></asp:Label>
					</li>
					<li id="liLastExitedFacility" runat="server">
						<label>
							Last exited
						</label>
						<asp:Label ID="lblLastExitedFacility" runat="server"></asp:Label>
					</li>
					<li id="liLastInFacilityStatus" runat="server">
						<label>
							Current status
						</label>
						<span class="input">
							<asp:Label ID="lblLastInFacilityStatus" runat="server" CssClass="label"></asp:Label><br />
							<asp:Button ID="btnClearLastInFacilityStatus" runat="server" Text="Set as exited facility" Style="width: auto;" />
						</span>
					</li>
				</ul>
			</div>
		</asp:Panel>
		<div class="sectionOdd" id="pnlInterfaceSetup" runat="server">
			<h2>Interface</h2>
			<div class="recordSelectionEvenOdd">
				<label>
					Interface setting</label>
				<asp:DropDownList ID="ddlDriverInterface" runat="server" AutoPostBack="True">
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
		<div class="section" id="pnlWarning" runat="server">
			<asp:Label ID="lblWarning" runat="server" BackColor="Red"></asp:Label>
			<asp:Button ID="btnYes" runat="server" Width="50px" Text="Yes"></asp:Button>
			<asp:Button ID="btnNo" runat="server" Width="50px" Text="No"></asp:Button>
		</div>
	</form>
</body>
</html>
