<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="EmailSettings.aspx.vb"
	Inherits="KahlerAutomation.TerminalManagement2.EmailSettings" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>General Settings : E-mail Settings</title>
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
	<form id="main" method="post" runat="server" defaultfocus="tbxServerEmail">
		<div class="recordControl">
			<asp:Button ID="btnSave" runat="server" Text="Save" />
			<asp:Label ID="lblSave" runat="server" ForeColor="Red" Text="E-mail settings were saved successfully"
				Visible="False" />
		</div>
		<div class="sectionEven" id="pnlEmailSettings" runat="server">
			<ul>
				<li>
					<label>
						Server e-mail address (from)
					</label>
					<asp:TextBox ID="tbxServerEmail" runat="server"></asp:TextBox>
				</li>
				<li>
					<label>
						Outgoing mail server (SMTP)
					</label>
					<asp:TextBox ID="tbxServerSmtp" runat="server"></asp:TextBox>
				</li>
				<li>
					<label>
						E-mail username
					</label>
					<asp:TextBox ID="tbxUsername" runat="server"></asp:TextBox>
				</li>
				<li>
					<label>
						E-mail password
					</label>
					<asp:TextBox ID="tbxPassword" runat="server" TextMode="Password"></asp:TextBox>
				</li>
				<li>
					<label>
						Outgoing mail server port
					</label>
					<asp:TextBox ID="tbxEmailServerPort" runat="server"></asp:TextBox>
				</li>
				<li>
					<label>
					</label>
					<asp:CheckBox ID="cbxEmailServerUseSsl" runat="server" Text="Use SSL/TLS" />
				</li>
				<li>
					<label>
						Days to keep e-mail records
					</label>
					<asp:TextBox ID="tbxDaysToKeepEmailRecords" runat="server"></asp:TextBox>
				</li>
				<li>
					<label>
					</label>
					<asp:CheckBox ID="cbxMarkEmailsWithNoRecipientAsSent" runat="server" Text="Mark e-mails with no recipient as sent" />
				</li>
				<li>&nbsp;</li>
				<li>&nbsp;</li>
				<li>
					<label>
						Test e-mail address
					</label>
					<asp:TextBox ID="tbxTestEmailAddress" runat="server"></asp:TextBox>
				</li>
				<li>
					<label>
						&nbsp;
					</label>
					<asp:Button ID="btnTestEmail" runat="server" Text="Test e-mail settings" />
				</li>
				<li>
					<label>
						&nbsp;
					</label>
					<asp:Label ID="lblTestEmail" runat="server" Text="" ForeColor="Red" CssClass="input"></asp:Label>
				</li>
			</ul>
		</div>
	</form>
</body>
</html>
