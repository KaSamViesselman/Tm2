<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="Branches.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.Branches" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Branches : Branches</title>
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
	<form id="main" method="post" runat="server">
	<div class="recordSelection">
		<label>
			Branch
		</label>
		<asp:DropDownList ID="ddlBranches" runat="server" AutoPostBack="True" />
	</div>
	<div class="recordControl">
		<asp:Button ID="btnSave" runat="server" Text="Save" />
		<asp:Button ID="btnDelete" runat="server" Text="Delete" />
		<asp:Label ID="lblStatus" runat="server" ForeColor="#ff0000"></asp:Label>
		<div class="sectionRequiredField">
			<label>
				<span class="required"></span>&nbsp;indicates required field
			</label>
		</div>
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
					<asp:TextBox ID="tbxName" runat="server" MaxLength="50"></asp:TextBox>
				</span></li>
			<li>
				<label>
					Branch number</label>
				<asp:TextBox ID="tbxBranchNumber" runat="server" MaxLength="50"></asp:TextBox>
			</li>
			<li>
				<label>
					Address</label>
				<asp:TextBox ID="tbxAddress" runat="server" MaxLength="50"></asp:TextBox>
			</li>
			<li>
				<label>
					City</label>
				<asp:TextBox ID="tbxCity" runat="server" MaxLength="50"></asp:TextBox>
			</li>
			<li>
				<label>
					State</label>
				<asp:TextBox ID="tbxState" runat="server" MaxLength="50"></asp:TextBox>
			</li>
			<li>
				<label>
					Zip code</label>
				<asp:TextBox ID="tbxZip" runat="server" MaxLength="15"></asp:TextBox>
			</li>
			<li>
				<label>
					Country</label>
				<asp:TextBox ID="tbxCountry" runat="server" MaxLength="100"></asp:TextBox>
			</li>
			<li>
				<label>
					Phone</label>
				<asp:TextBox ID="tbxPhone" runat="server" MaxLength="50"></asp:TextBox>
			</li>
			<li>
				<label>
					E-mail</label>
				<asp:TextBox ID="tbxEmail" runat="server"></asp:TextBox>
			</li>
		</ul>
		<ul id="lstCustomFields" runat="server">
		</ul>
	</asp:Panel>
	<div class="sectionOdd" id="pnlInterfaceSetup" runat="server">
		<h2>
			Interface</h2>
		<div class="recordSelectionEvenOdd">
			<label>
				Interface setting</label>
			<asp:DropDownList ID="ddlBranchInterface" runat="server" AutoPostBack="True">
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
