<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="AccountCouplingSettingsHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.AccountCouplingSettingsHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head id="Head1" runat="server"><title>Help : Account Coupling Settings</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="AccountCouplingSettingsHelp" runat="server">
		<div><span style="font-size: large; font-weight: bold;"><a href="help.aspx#AccountCouplingSettings">Help</a> : Account Coupling Settings</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>Account coupling settings control the behavior of Terminal Management and the applications</p>
				<p><span class="helpItem">Use coupled/associated accounts when entering orders:</span> when enabled, customer account couples are used to automatically select associated accounts when selecting customer accounts in the order page.</p>
				<p><span class="helpItem">Display a check box on orders page to turn on/off coupled accounts:</span> when enabled, this adds an option to the orders page that can be used to temporarily enable/disable customer account coupling while selecting customer accounts in the order page.</p>
			</div>
			<p><span class="helpItem">Save:</span> saves the settings entered.</p>
		</div>
	</form>
</body>
</html>
