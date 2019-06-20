<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="TicketSettingsHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.TicketSettingsHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head id="Head1" runat="server"><title>Help : Ticket Settings</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="TicketSettingsHelp" runat="server">
		<div><span style="font-size: large; font-weight: bold;"><a href="help.aspx#TicketSettings">Help</a> : Ticket Settings</span> <hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>Ticket settings control the behavior of Terminal Management and the applications that work with Terminal Management.</p>
				<p><span class="helpItem">Use separate ticket numbers per owner:</span> when enabled, each owner uses their own sequence of ticket numbers. If not enabled, all owners use one sequence of ticket numbers.</p>
				<p><span class="helpItem">Next ticket number:</span> the next ticket number (if separate ticket numbers are not being used per owner).</p>
				<p><span class="helpItem">Use separate ticket prefix per owner:</span> when enabled, each owner may specify their own ticket number prefix.</p>
				<p><span class="helpItem">Ticket prefix:</span> used if not configured to use ticket prefix per owner, characters to add to the left-side of the ticket number.</p>
				<p><span class="helpItem">Use separate ticket suffix per owner:</span> when enabled, each owner may specify their own ticket number suffix.</p>
				<p><span class="helpItem">Ticket suffix:</span> used if not configured to use ticket suffix per owner, characters to add to the right-side of the ticket number.</p>
				<p><span class="helpItem">Send tickets to:</span> will designate which type of entity e-mail addresses should be looked at to send tickets to after the ticket</p>
			</div>
			<p><span class="helpItem">Save:</span> saves the settings entered.</p>
		</div>
	</form>
</body>
</html>
