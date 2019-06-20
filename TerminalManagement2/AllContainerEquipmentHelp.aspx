<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="AllContainerEquipmentHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.AllContainerEquipmentHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Help : All Container Equipment Report</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="AllContainerEquipmentHelp" runat="server">
		<div>
			<span style="font-size: large; font-weight: bold;"><a href="help.aspx#AllContainerEquipmentReport">Help</a> : All Container Equipment Report</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>The all container equipment report shows all the container equipment records in a table format. Container equipment displayed on the report may be sorted and filtered by various fields and a search function is also available.</p>
				<table>
					<tr>
						<td><span class="helpItem">Filter by:</span> container equipment records may be filtered by facility, status (with customer), owner, customer account, or barcode number. Only the records that match the filter conditions will be displayed</td>
					</tr>
					<tr>
						<td><span class="helpItem">Show deleted:</span> when enabled, deleted container equipment records are included in the report.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Sort by:</span> container records may be sorted by barcode number, last inspected date, or created date in either ascending or descending order.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Show report:</span> show the report in a new tab.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Printer friendly version:</span> show the report in a new tab, without any hyper-links.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Download:</span> download the report in a format readable by Excel.</td>
					</tr>
				</table>
				<p><span class="helpItem">E-mail to:</span> the all container equipment report may be e-mailed directly from the page. Enter the e-mail addresses for the recipients (multiple addresses may be separated by commas) and click the "Send" button.</p>
				<p><span class="helpItem">Add address:</span> will look in the e-mail history, and items to find possible e-mail addresses that the report may be sent to.</p>
			</div>
		</div>
	</form>
</body>
</html>
