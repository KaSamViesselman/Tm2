<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="TankLevelTrendsHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.TankLevelTrendsHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Help : Tank Level Trends</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="form1" runat="server">
		<div>
			<span style="font-size: large; font-weight: bold;"><a href="help.aspx#TankLevelTrends">Help</a> : Tank Level Trends</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>Tank level trends may be setup to record the physical level of a tank at a regular interval of time. The tank level trend data may be reviewed or downloaded as a comma separated value (CSV) file which may be opened in a spreadsheet application for analysis and graphing.</p>
				<p><span class="helpItem">Tank level trend drop-down list:</span> located at the top of the page, this list contains all the tank level trend records. To edit an existing tank level trend record, select its name in the drop-down list. To create a new tank level trend record, select "Enter new tank level trend".</p>
				<table>
					<tr>
						<td style="width: 50%;">
							<table>
								<tr>
									<td><span class="helpItem">Name:</span> the name of the tank level trend. The name is required and may include up to 50 characters.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Tank:</span> the tank that the trend should collect data from. The tank is optional, but data is only collected when a tank is selected.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Interval:</span> the period of time between tank level readings (in minutes, hours, days, months).</td>
								</tr>
								<tr>
									<td><span class="helpItem">Beginning at:</span> the date and time that the interval should align with. If the interval is 1 day and the date and time are set to 3/14/2013 1:00 PM, the system will record a tank level trend data point at 3/14/2013 1:00 PM, 3/15/2013 1:00 PM, 3/16/2013 1:00 PM and so on.</td>
								</tr>
							</table>
						</td>
						<td style="width: 50%;">
							<table>
								<tr>
									<td><span class="helpItem">Show data:</span> click this to view the tank level trend data.
									</td>
								</tr>
								<tr>
									<td><span class="helpItem">Download:</span> click this to download a file that may be opened in a spreadsheet application.
									</td>
								</tr>
								<tr>
									<td><span class="helpItem">Printer friendly:</span> click this to view the tank level trend data in a new browser window suitable for printing.
									</td>
								</tr>
							</table>
						</td>
					</tr>
				</table>
			</div>
			<p><span class="helpItem">Save button:</span> located at the top of the page, click this button to save any changes made to a tank level trend record or create a new tank level trend record.</p>
			<p><span class="helpItem">Delete button:</span> located at the top of the page, click this button to delete the current tank level trend record.</p>
		</div>
	</form>
</body>
</html>
