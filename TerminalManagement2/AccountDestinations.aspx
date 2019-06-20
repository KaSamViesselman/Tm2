<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="AccountDestinations.aspx.vb"
	Inherits="KahlerAutomation.TerminalManagement2.Destinations" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Customer Accounts : Destinations</title>
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
	<form id="main" runat="server" method="post" defaultfocus="ddlAccounts">
	<div class="sectionEven">
		<ul>
			<li>
				<label>
					Account
				</label>
				<asp:DropDownList ID="ddlAccounts" runat="server" AutoPostBack="True">
				</asp:DropDownList>
			</li>
			<li>
				<label>
					Destination</label>
				<asp:DropDownList ID="ddlDestinations" runat="server" AutoPostBack="True" Enabled="False">
				</asp:DropDownList>
			</li>
		</ul>
	</div>
	<div class="recordControl">
		<asp:Button ID="btnSave" runat="server" Text="Save" />
		<asp:Button ID="btnDelete" runat="server" Text="Delete" />
		<asp:Label ID="lblStatus" runat="server" ForeColor="#ff0000"></asp:Label>
		<span class="sectionRequiredField"><span class="required"></span>&nbsp;indicates required
			field </span>
	</div>
	<asp:Panel ID="pnlEven" runat="server" CssClass="sectionEven">
		<h1>
			General</h1>
		<ul>
			<li>
				<label>
					Name
				</label>
				<span class="required">
					<asp:TextBox ID="tbxName" runat="server" MaxLength="50" Enabled="False"></asp:TextBox>
				</span></li>
			<li>
				<label>
					Account
				</label>
				<asp:DropDownList ID="ddlAccounts2" runat="server" AutoPostBack="True" Enabled="False">
				</asp:DropDownList>
			</li>
			<li>
				<label>
					Street
				</label>
				<asp:TextBox ID="tbxStreet" runat="server" MaxLength="100" Enabled="False"></asp:TextBox>
			</li>
			<li>
				<label>
					City
				</label>
				<asp:TextBox ID="tbxCity" runat="server" MaxLength="50" Enabled="False"></asp:TextBox>
			</li>
			<li>
				<label>
					State
				</label>
				<asp:TextBox ID="tbxState" runat="server" MaxLength="50" Enabled="False"></asp:TextBox>
			</li>
			<li>
				<label>
					Zip code
				</label>
				<asp:TextBox ID="tbxZip" runat="server" MaxLength="15" Enabled="False"></asp:TextBox>
			</li>
			<li>
				<label>
					Country
				</label>
				<asp:TextBox ID="tbxCountry" runat="server" MaxLength="100" Enabled="False"></asp:TextBox>
			</li>
			<li>
				<label>
					Acres
				</label>
				<asp:TextBox ID="tbxAcres" runat="server" MaxLength="8" Enabled="False"></asp:TextBox>
			</li>
			<li>
				<label>
					Default cross reference
				</label>
				<asp:TextBox ID="tbxCrossRef" runat="server" MaxLength="50" Enabled="False"></asp:TextBox>
			</li>
			<li>
				<label>
					E-mail
				</label>
				<asp:TextBox ID="tbxEmail" runat="server" Enabled="false"></asp:TextBox>
			</li>
			<li>
				<label>
					Notes
				</label>
				<asp:TextBox ID="tbxNotes" runat="server" Enabled="false" TextMode="MultiLine"></asp:TextBox>
			</li>
		</ul>
		<ul id="lstCustomFields" runat="server">
		</ul>
	</asp:Panel>
	<div class="sectionOdd" id="pnlAccountLocationInterfaceSetup" runat="server">
		<h2>
			Interface</h2>
		<div class="recordSelectionEvenOdd">
			<label>
				Interface setting</label>
			<asp:DropDownList ID="ddlAccountLocationInterface" runat="server" AutoPostBack="True">
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
