<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="AllContainersHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.AllContainersHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Help : All Containers Report</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="AllContainersHelp" runat="server">
		<div>
			<span style="font-size: large; font-weight: bold;"><a href="help.aspx#AllContainersReport">Help</a> : All Containers Report</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>The all containers report shows all the container records in a table format. Containers displayed on the report may be sorted and filtered by various fields and a search function is also available. Container history is available as a link for each container on the right side of the table. Containers that are near or past due on inspection are highlighted.</p>
				<table>
					<tr>
						<td><span class="helpItem">Filter by:</span> container records may be filtered by facility, status (in facility, in transit or with customer), bulk product, owner, customer, or container number. Only the records that match the filter conditions will be displayed</td>
					</tr>
					<tr>
						<td><span class="helpItem">Show deleted:</span> when enabled, deleted container records are included in the report.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Refresh:</span> click this button to refresh the all container report or enact the sort and filter options.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Configure displayed columns:</span> click this button to change which container fields are shown in the report table.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Sort by:</span> container records may be sorted by number, last inspected date, or created date in either ascending or descending order.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Display count:</span> the report shows a limited number of container entries on the page at a time. To change the number of records per page, modify the display count.</td>
					</tr>
					<tr>
						<td><span class="helpItem">First:</span> navigate to the first page of the all container report.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Previous (Prev):</span> navigate to page previous to the page currently being viewed of the all container report.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Next:</span> navigate to the page after the page currently being viewed of the all container report.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Last:</span> navigate to the last page of the all containers report.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Printer friendly version:</span> link to a version of the report where all containers are listed on one page, without the controls and tabs.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Download:</span> link to download a comma-separated value (CSV) file containing all the containers listed in the report. The CSV file may be opened in a spreadsheet application such as Microsoft Excel.</td>
					</tr>
				</table>
				<p><span class="helpItem">E-mail to:</span> the all containers report may be e-mailed directly from the page. Enter the e-mail addresses for the recipients (multiple addresses may be separated by commas) and click the "Send" button.</p>
				<p><span class="helpItem">Add address:</span> will look in the e-mail history, and items to find possible e-mail addresses that the report may be sent to.</p>
			</div>
		</div>
	</form>
</body>
</html>
