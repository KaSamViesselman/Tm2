<%@ Page Language="vb" AutoEventWireup="false" Async="True" CodeBehind="Login.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.Login" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Login</title>
	<link rel="shortcut icon" href="FavIcon.ico" />
	<meta name="viewport" content="width=device-width" />
	<meta http-equiv="X-UA-Compatible" content="IE=edge" />
	<link rel="stylesheet" href="styles/site.css" />
	<link rel="stylesheet" href="styles/site-tablet.css" media="(max-width:768px)" />
	<link rel="stylesheet" href="styles/site-phone.css" media="(max-width:480px)" />
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
	<form id="Login" method="post" runat="server" defaultfocus="tbxUsername">
		<div class="header">
			<img src="images/Kahler-logo-standard.png" alt="Kahler Automation" class="logo" />
			<span class="applicationTitle">Terminal Management 2</span>
		</div>
		<div class="titleBar">
			<h1>Login</h1>
		</div>
		<div id="pnlLogin" runat="server" class="sectionEven">
			<ul>
				<li>
					<label style="width: 89px; text-align: right">
						Username</label>
					<asp:TextBox ID="tbxUsername" runat="server" Width="150px"></asp:TextBox>
				</li>
				<li>
					<label style="width: 89px; text-align: right">
						Password
					</label>
					<span class="input" style="margin-left: 0;">
						<asp:TextBox ID="tbxPassword" runat="server" TextMode="Password" Width="134px"></asp:TextBox>
						<img id="imgShowPassword" onclick="TogglePasswordVisibility()" src="images/icons8-eye-512.png" style="height: 16px; vertical-align: middle;" />
					</span>
				</li>
				<li>
					<label style="width: 89px; text-align: right">
					</label>
					<asp:Button ID="btnSubmit" runat="server" Text="Enter" Width="150px" />
				</li>
			</ul>
			<asp:Label ID="lblWarning" runat="server" Height="24px" Text="Incorrect username or password"
				Width="200px" ForeColor="Red" Visible="False"></asp:Label>
		</div>
		<div class="sectionOdd">
			<asp:Label ID="lblVersion" runat="server" Text="Version:" Style="float: right;"></asp:Label>
			<br />
			<asp:Label ID="lblDatabaseVersion" runat="server" Text="Database Version:" Style="float: right;"></asp:Label>
			<br />
			<asp:Label ID="lblCopyright" runat="server" Text="" Style="float: right;"></asp:Label>
		</div>
		<div id="pnlActivationWarning" runat="server" class="section">
			<br />
			<hr />
			<br />
			<asp:Label ID="lblActivationWarning" runat="server" Text="Activation error" Style="width: 100%; color: red; font-size: large;"></asp:Label>
			<div id="pnlActivation" runat="server" class="section">
				<asp:Button ID="btnAuthorization" runat="server" Text="Authorization" Width="150px" />
			</div>
		</div>
	</form>
</body>
</html>
