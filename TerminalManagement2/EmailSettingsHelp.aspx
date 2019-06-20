<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="EmailSettingsHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.EmailSettingsHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head id="Head1" runat="server"><title>Help : E-mail Settings</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="EmailSettingsHelp" runat="server">
		<div><span style="font-size: large; font-weight: bold;"><a href="help.aspx#EmailSettings">Help</a> : E-mail Settings</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>E-mail settings control the behavior of Terminal Management and the applications that work with Terminal Management.</p>
				<p><span class="helpItem">Server e-mail address:</span> the return address that the server uses when sending ticket/report e-mails.</p>
				<p><span class="helpItem">Outgoing mail server:</span> the address of the outgoing (SMTP) mail server (e.g. smtp.kahlerautomation.com).</p>
				<p><span class="helpItem">E-mail username:</span> username used to send e-mail through the outgoing (SMTP) mail server.</p>
				<p><span class="helpItem">E-mail password:</span> password used to send e-mail through the outgoing (SMTP) mail server.</p>
				<p><span class="helpItem">Outgoing mail server port:</span> the TCP/IP port used to communicate with the outgoing (SMTP) mail server. Typically this is 25 for unsecured connections and 587 for secured (SSL) connections.</p>
				<p><span class="helpItem">Use SSL/TLS:</span> when enabled, the server will attempt to use encryption to secure the communication with the outgoing (SMTP) mail server.</p>
				<p><span class="helpItem">Mark e-mails with no recipient as sent:</span> when enabled, will mark any e-mails as being sent if there are no recipients assigned to the e-mail.</p>
				<p><span class="helpItem">Days to keep e-mail records:</span> the number of days to keep sent e-mails in the database.</p>
			</div>
			<p><span class="helpItem">Save:</span> saves the settings entered.</p>
		</div>
	</form>
</body>
</html>
