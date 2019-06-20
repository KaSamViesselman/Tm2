<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="AllCarriersHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.AllCarriersHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Help : All Carriers Report</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="AllCarriers" runat="server">
		<div>
			<span style="font-size: large; font-weight: bold;"><a href="help.aspx#AllCarriersReport">Help</a> : All Carriers Report</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>The all carriers report shows all the carrier records in a tabular form.</p>
				<table>
					<tr>
						<td><span class="helpItem">Printer friendly version:</span> opens the report in a new window without the Terminal Management header or side bar that is suitable for printing.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Download:</span> downloads the carrier list as a comma separated value (CSV) file that may be opened by a spreadsheet application.</td>
					</tr>
				</table>
				<p><span class="helpItem">E-mail to:</span> the all carriers report may be e-mailed directly from the page. Enter the e-mail addresses for the recipients (multiple addresses may be separated by commas) and click the "Send" button.</p>
				<p><span class="helpItem">Add address:</span> will look in the e-mail history, and items to find possible e-mail addresses that the report may be sent to.</p>
			</div>
		</div>
	</form>
</body>
</html>
