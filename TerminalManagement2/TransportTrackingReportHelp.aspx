<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="TransportTrackingReportHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.TransportTrackingReportHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head id="Head1" runat="server"><title>Help : Transport Tracking Report</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="TransportTrackingReportHelp" runat="server">
		<div><span style="font-size: large; font-weight: bold;"><a href="help.aspx#TransportTrackingReport">Help</a> : Transport Tracking Report</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>The transport tracking report is used to view the last load a transport carried.</p>
				<p><span class="helpItem">Order By:</span> allows the ability to order the transports by selecting an option.</p>
				<p><span class="helpItem">Asc/Desc:</span> allows the ability to order the transports by ascending or descending values of the selected Order By field.</p>
				<p><span class="helpItem">Display Unit of Measure:</span> all numbers will be formatted to this display unit (and converted if needed).</p>
				<p><span class="helpItem">E-mail to:</span> the transport tracking report may be e-mailed directly from the page. Enter the e-mail addresses for the recipients (multiple addresses may be separated by commas) and click the "Send" button.</p>
				<p><span class="helpItem">Add address:</span> will look in the e-mail history, and items to find possible e-mail addresses that the report may be sent to.</p>
				<p><span class="helpItem">Display report:</span> click to show the report with the selected unit.</p>
				<p><span class="helpItem">Printer friendly:</span> click to show the report with the selected unit in printable form.</p>
				<p><span class="helpItem">Download:</span> click to download a comma separated value (CSV) file containing the report data in a file format that may be opened by a spreadsheet application.</p>
			</div>
		</div>
	</form>
</body>
</html>
