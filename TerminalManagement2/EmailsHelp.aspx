<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="EmailsHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.EmailsHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server"><title>Help : E-mails</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="EmailsHelp" runat="server">
		<div><span style="font-size: large; font-weight: bold;"><a href="help.aspx#Emails">Help</a>: E-mails</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>The e-mails page provides access to ticket/report e-mail messages waiting to send e-mail messages that have been sent by Terminal Management server.</p>
				<p><span class="helpItem">Show messages as old as:</span> selects how old the e-mail messages may be that are displayed.</p>
				<p><span class="helpItem">Show deleted messages:</span> determines if deleted/sent e-mail messages should be displayed.</p>
				<p><span class="helpItem">Message list:</span> the list of e-mail messages. Select a message in this list to review or modify the message below.</p>
				<table>
					<tr>
						<td><span class="helpItem">Deleted:</span> determines if the selected message is marked as deleted. A message may be re-sent by clearing the deleted status.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Subject:</span> the subject of the selected message.</td>
					</tr>
					<tr>
						<td><span class="helpItem">To:</span> the recipients for the selected message. Multiple e-mail addresses should be separated by commas.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Attachments:</span> the file attachments included in the message. Click "Open Attachment" to view/download the attachment. Click "Delete Attachment" to remove the attachment from the e-mail message.</td>
					</tr>
				</table>
			</div>
			<p><span class="helpItem">Save:</span> click to save any changes made to the selected e-mail message.</p>
		</div>
	</form>
</body>
</html>
