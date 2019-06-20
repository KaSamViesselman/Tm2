<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="AccountCouplingSettings.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.AccountCouplingSettings" %>

<!DOCTYPE html>
<html lang="en">
<head id="Head1" runat="server">
	<title>General Settings : Account Coupling Settings</title>
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
    <form id="main" method="post" runat="server" defaultfocus="cbxCoupledAccounts">
    <div class="recordControl">
		<asp:Button ID="btnSave" runat="server" Text="Save" />
		<asp:Label ID="lblSave" runat="server" ForeColor="Red" Text="Account coupling settings were saved successfully"
			Visible="False" />
	</div>
	<div class="sectionEven" id="pnlAccountCouplingSettings" runat="server">
		<ul>
			<li>
				<label>
				</label>
				<asp:CheckBox ID="cbxCoupledAccounts" runat="server" AutoPostBack="True" Text="Use coupled / associated  accounts when entering orders">
				</asp:CheckBox>
			</li>
			<li>
				<label>
				</label>
				<asp:CheckBox ID="cbxShowCoupledBoxOnOrders" runat="server" AutoPostBack="True" Text="Display a check box on the orders page to turn on / off coupled accounts">
				</asp:CheckBox>
			</li>
		</ul>
	</div>
    </form>
</body>
</html>
