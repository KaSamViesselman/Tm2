<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="EventLogHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.EventLogHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server"><title>Help : Event Log</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="EventLog" runat="server">
		<div><span style="font-size: large; font-weight: bold;"><a href="help.aspx#EventLog">Help</a> : Event Log</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>The event log shows information, warnings and failures that the applications log.</p>
				<strong>Range</strong><table>
					<tr>
						<td><strong>From:</strong> specifies the oldest log entries that are displayed.</td>
					</tr>
					<tr>
						<td><strong>To:</strong> specifies the newest log entries that are displayed.</td>
					</tr>
				</table>
				<strong>Filters</strong><table>
					<tr>
						<td><strong>Computer:</strong> by default, log entries for all computers are displayed, however the list may be limited to a specific computer by selecting that computer in this drop-down list.</td>
					</tr>
					<tr>
						<td><strong>Application:</strong> by default, log entries for all applications are displayed, however the list may be limited to a specific application by selecting that application in this drop-down list.</td>
					</tr>
					<tr>
						<td><strong>Category:</strong> by default, log entries for all categories (or type) are displayed, however the list may be limited to a specific category by selecting that category in this drop-down list.</td>
					</tr>
				</table>
				<p><span class="helpItem">Show log entries:</span> display the applicable log entries.</p>
				<p><span class="helpItem">E-mail to:</span> the event log may be e-mailed directly from the page. Enter the e-mail addresses for the recipients (multiple addresses may be separated by commas) and click the "Send" button.</p>
				<p><span class="helpItem">Add address:</span> will look in the e-mail history, and items to find possible e-mail addresses that the report may be sent to.</p>
			</div>
		</div>
	</form>
</body>
</html>
