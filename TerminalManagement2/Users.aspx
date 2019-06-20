<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="Users.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.Users" %>

<!DOCTYPE html>
<html lang="en">
<head id="Head1" runat="server">
	<title>Users : Users</title>
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
		// Change the type of input to password or text 
		function TogglePasswordVisibility() {
			var password = document.getElementById("tbxPassword");
			var showPassword = document.getElementById("imgShowPassword");
			if (password.type === "password") {
				password.type = "text";
				showPassword.src = "images/icons8-password-16.png";
			} else {
				password.type = "password";
				showPassword.src = "images/icons8-eye-512.png"
			}
		}
	</script>
</head>
<body>
	<form id="main" runat="server" defaultfocus="tbxName">
		<div class="recordSelection">
			<label>
				User</label>
			<asp:DropDownList ID="ddlUsers" runat="server" AutoPostBack="True" />
		</div>
		<div class="recordControl">
			<asp:Button ID="btnSave" runat="server" Text="Save" />
			<asp:Button ID="btnDelete" runat="server" Text="Delete" Enabled="False" />
			<asp:Label ID="lblStatus" runat="server" ForeColor="Red" />
			<div class="sectionRequiredField">
				<label>
					<span class="required"></span>&nbsp;indicates required field</label>
			</div>
		</div>
		<asp:Panel ID="pnlEven" runat="server" CssClass="sectionEven">
			<h1>General</h1>
			<ul>
				<li>
					<label>
						Name</label>
					<span class="required">
						<asp:TextBox ID="tbxName" runat="server" MaxLength="50" /></span>
				</li>
				<li>
					<label>
						User profile</label>
					<asp:DropDownList ID="ddlUserProfile" runat="server" AutoPostBack="true" />
				</li>
				<li>
					<label>
						Username</label>
					<span class="required">
						<asp:TextBox ID="tbxUserName" runat="server" MaxLength="50" /></span>
				</li>
				<li id="pnlPassword" runat="server">
					<label>
						Password</label>
					<span class="required">
						<asp:TextBox ID="tbxPassword" runat="server" MaxLength="50" TextMode="Password" /></span>
				</li>
				<%--<li id="pnlPassword" runat="server">
					<label>
						Password</label>
					<span class="required">
						<span class="input" style="margin-left: 0;">
							<asp:TextBox ID="TextBox1" runat="server" MaxLength="50" TextMode="Password" Style="width: 90%;" />
							<img id="imgShowPassword" onclick="TogglePasswordVisibility()" src="images/icons8-eye-512.png" style="height: 16px; vertical-align: middle;" />
						</span>
					</span>
				</li>--%>
				<li>
					<label>
						Owner</label>
					<asp:DropDownList ID="ddlOwners" runat="server" />
				</li>
				<li>
					<label>
						Disabled</label>
					<asp:CheckBox ID="cbxDisabled" runat="server" Text=""></asp:CheckBox>
				</li>
				<li>
					<label>
						Application configuration</label>
					<asp:CheckBox ID="cbxAppConfig" runat="server" />
				</li>
			</ul>
		</asp:Panel>
		<asp:Panel ID="pnlOdd" runat="server" CssClass="sectionOdd">
			<h1>Permissions</h1>
			<asp:CheckBoxList ID="cbxRights" runat="server" RepeatColumns="2">
				<asp:ListItem>View orders</asp:ListItem>
				<asp:ListItem>Modify orders</asp:ListItem>
				<asp:ListItem>View / modify owners</asp:ListItem>
				<asp:ListItem>View / modify customer accounts</asp:ListItem>
				<asp:ListItem>View / modify drivers</asp:ListItem>
				<asp:ListItem>View / modify carriers</asp:ListItem>
				<asp:ListItem>View / modify transports</asp:ListItem>
				<asp:ListItem>View / modify users</asp:ListItem>
				<asp:ListItem>View / modify products</asp:ListItem>
				<asp:ListItem>View inventory</asp:ListItem>
				<asp:ListItem>Modify inventory</asp:ListItem>
				<asp:ListItem>View / modify panels</asp:ListItem>
				<asp:ListItem>View / modify panel bulk product settings</asp:ListItem>
				<asp:ListItem>View / modify facilities</asp:ListItem>
				<asp:ListItem>View / modify branches</asp:ListItem>
				<asp:ListItem>View reports</asp:ListItem>
				<asp:ListItem>Modify reports</asp:ListItem>
				<asp:ListItem>View / modify containers</asp:ListItem>
				<asp:ListItem>View purchase orders</asp:ListItem>
				<asp:ListItem>Modify purchase orders</asp:ListItem>
				<asp:ListItem>View / modify general settings</asp:ListItem>
				<asp:ListItem>View tanks</asp:ListItem>
				<asp:ListItem>Modify tanks</asp:ListItem>
				<asp:ListItem>View / modify units</asp:ListItem>
				<asp:ListItem>View / modify crops</asp:ListItem>
				<asp:ListItem>View / modify applicators</asp:ListItem>
				<asp:ListItem>View / modify custom pages</asp:ListItem>
				<asp:ListItem>View / modify order page interfaces</asp:ListItem>
				<asp:ListItem>View / modify receipt page interfaces</asp:ListItem>
				<asp:ListItem>View / modify e-mails</asp:ListItem>
				<asp:ListItem>View / modify staged orders</asp:ListItem>
				<asp:ListItem>View / modify interfaces</asp:ListItem>
			</asp:CheckBoxList>
			<asp:CheckBoxList ID="cbxCustomPages" runat="server" RepeatColumns="2" Width="100%" />
			<asp:CheckBox ID="chkSelectAll" runat="server" Text="Select All" AutoPostBack="True" />
		</asp:Panel>
		<asp:Panel ID="pnlSignature" runat="server" Visible="false">
			<div class="sectionEven" id="signatureSection" runat="server">
				<h1>Signature</h1>
				<ul>
					<li>
						<asp:Literal ID="litUserSignature" runat="server" />
						<asp:Button ID="btnClearSignature" runat="server" Text="Clear Signature" Style="width: auto;" />
					</li>
					<li>
						<asp:FileUpload ID="objFileUpload" runat="server" CssClass="input" Style="width: 95%;" />
					</li>
					<li>
						<label>
						</label>
						<asp:Button ID="btnUploadSignature" runat="server" Text="Upload Signature" Style="width: auto;" />
					</li>
					<li>
						<asp:Label ID="UploadStatusLabel" runat="server"></asp:Label>
					</li>
				</ul>
			</div>
		</asp:Panel>
	</form>
</body>
</html>
