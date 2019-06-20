<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="DriverReportHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.DriverReportHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server"><title>Help : Drivers In Facility</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="DriverReportHelp" runat="server">
		<div><span style="font-size: large; font-weight: bold;"><a href="help.aspx#DriversInFacility">Help</a> : Drivers In Facility</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>The drivers in facility page shows all the drivers currently in facilities.</p>
				<p><span class="helpItem">E-mail to:</span> the current driver in facility report may be e-mailed directly from the page. Enter the e-mail addresses for the recipients (multiple addresses may be separated by commas) and click the "Send" button.</p>
				<p><span class="helpItem">Add address:</span> will look in the e-mail history, and items to find possible e-mail addresses that the report may be sent to.</p>
			</div>
		</div>
	</form>
</body>
</html>
