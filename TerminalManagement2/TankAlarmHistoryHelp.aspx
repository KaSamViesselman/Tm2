<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="TankAlarmHistoryHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.TankAlarmHistoryHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server"><title>Help : Tank Alarm History</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="TankAlarmHistoryHelp" runat="server">
		<div><span style="font-size: large; font-weight: bold;"><a href="help.aspx#TankAlarmHistory">Help</a> : Tank Alarm History</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>Tank alarm history shows all the tank alarms (high-high, high, low, low-low, flow rate, sensor) for tanks over a selectable period of time.</p>
				<table>
					<tr>
						<td><span class="helpItem">Tank:</span> By default alarm history will be shown for all tanks. To narrow the alarm history to a single tank, select the tank name in the tank drop-down list.</td>
					</tr>
					<tr>
						<td><span class="helpItem">From:</span> the date and time of the earliest tank alarm to show.</td>
					</tr>
					<tr>
						<td><span class="helpItem">To:</span> the date and time of the latest tank alarm to show.</td>
					</tr>
				</table>
				<p><span class="helpItem">E-mail to:</span> the tank alarm history report may be e-mailed directly from the page. Enter the e-mail addresses for the recipients (multiple addresses may be separated by commas) and click the "Send" button.</p>
				<p><span class="helpItem">Add address:</span> will look in the e-mail history, and items to find possible e-mail addresses that the report may be sent to.</p>
			</div>
		</div>
	</form>
</body>
</html>
