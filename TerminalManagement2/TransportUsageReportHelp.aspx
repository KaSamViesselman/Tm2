<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="TransportUsageReportHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.TransportUsageReportHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server"><title>Help : Transport Usage Report</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="TransportUsageReportHelp" runat="server">
		<div><span style="font-size: large; font-weight: bold;"><a href="help.aspx#TransportsUsageReport">Help</a> : Transport Usage Report</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>The transport usage report may be used to review what loads a transport was used for or what transports have been used to haul a customer account's loads.</p>
				<p><span class="helpItem">Start date:</span> the oldest load to include in the report.</p>
				<p><span class="helpItem">End date:</span> the newest load to include in the report.</p>
				<p><span class="helpItem">Customer:</span> filter the results in the report to only include loads for the selected customer.</p>
				<p><span class="helpItem">Transport:</span> filter the results in the report to only include loads using the selected transport.</p>
				<p><span class="helpItem">Run report:</span> click to create the report with the selected date range, customer and transport.</p>
				<p><span class="helpItem">Printer friendly version:</span> opens the report in a new window without the Terminal Management header or side bar, which is suitable for printing.</p>
				<p><span class="helpItem">E-mail to:</span> the transport usage report may be e-mailed directly from the page. Enter the e-mail addresses for the recipients (multiple addresses may be separated by commas) and click the "Send" button.</p>
				<p><span class="helpItem">Add address:</span> will look in the e-mail history, and items to find possible e-mail addresses that the report may be sent to.</p>
			</div>
		</div>
	</form>
</body>
</html>
