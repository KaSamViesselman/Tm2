<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="ReceivingPoSettingsHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.ReceivingPoSettingsHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head id="Head1" runat="server">
	<title>Help : Receiving PO Settings</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="ReceivingPoSettingsHelp" runat="server">
		<div>
			<span style="font-size: large; font-weight: bold;"><a href="help.aspx#ReceivingPoSettings">Help</a> : Receiving PO Settings</span>
			<hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>Receiving PO settings control the behavior of Terminal Management and the applications that work with Terminal Management.</p>
				<p><span class="helpItem">Automatically generate sequential order numbers:</span> when enabled, a purchase order number will automatically be generated sequentially when creating a new receiving purchase order record.</p>
				<p><span class="helpItem">Allow order numbers to be modified:</span> when enabled, the user may modify the receiving purchase order number.</p>
				<p><span class="helpItem">Use separate order numbers per owner:</span> when enabled, each owner has their own sequence of receiving purchase order numbers. If not enabled, all owners pull from the same sequence of numbers, the next number specified in the "starting order number" field.</p>
				<p><span class="helpItem">Starting order number:</span> the next number to use for an order when the automatically generate sequential order numbers option is selected.</p>
				<p><span class="helpItem">E-mail created Point of Sale tickets:</span> will determine if tickets created by manually receiving through the web site will be e-mailed to any entities (e.g. owner, supplier, etc.) that are configured with an e-mail address.</p>
			</div>
			<p><span class="helpItem">Save:</span> saves the settings entered.</p>
		</div>
	</form>
</body>
</html>
