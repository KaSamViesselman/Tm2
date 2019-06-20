<%@ Page Language="vb" MaintainScrollPositionOnPostback="true" AutoEventWireup="false"
	CodeBehind="Suppliers.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.Suppliers" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Purchase Orders : Suppliers</title>
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
	<form id="main" method="post" runat="server" defaultfocus="ddlOwners">
		<div class="recordSelection">
			<label>
				Supplier
			</label>
			<asp:DropDownList ID="ddlSupplierAccounts" runat="server" AutoPostBack="True">
			</asp:DropDownList>
		</div>
		<div class="recordControl">
			<asp:Button ID="btnSave" runat="server" Text="Save" />
			<asp:Button ID="btnDelete" runat="server" Text="Delete" Enabled="False" />
			<asp:Label ID="lblStatus" runat="server" ForeColor="Red"></asp:Label>
			<span class="sectionRequiredField"><span class="required"></span>&nbsp;indicates required
			field </span>
		</div>
		<asp:Panel ID="pnlEven" runat="server" CssClass="sectionEven">
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
						Owner
					</label>
					<span class="required">
						<asp:DropDownList ID="ddlOwner" runat="server">
						</asp:DropDownList>
					</span></li>
				<li>
					<label>
						Default cross reference
					</label>
					<asp:TextBox ID="tbxAccountNumber" runat="server" MaxLength="50"></asp:TextBox>
				</li>
				<li>
					<label>
						Street
					</label>
					<asp:TextBox ID="tbxStreet" runat="server" MaxLength="100"></asp:TextBox>
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
					<asp:TextBox ID="tbxZipcode" runat="server" MaxLength="15"></asp:TextBox>
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
			</ul>
		</asp:Panel>
		<div class="sectionOdd" id="pnlInterfaceSetup" runat="server">
			<h2>Interface</h2>
			<div class="recordSelectionEvenOdd">
				<label>
					Interface setting</label>
				<asp:DropDownList ID="ddlSupplierInterface" runat="server" AutoPostBack="True">
				</asp:DropDownList>
			</div>
			<asp:Panel ID="pnlInterfaceSettings" runat="server">
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
