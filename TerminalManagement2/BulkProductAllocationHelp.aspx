<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="BulkProductAllocationHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.BulkProductAllocationHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head id="Head1" runat="server">
	<title>Help : Bulk Product Allocation</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="BulkProductAllocationHelp" runat="server">
		<div>
			<span style="font-size: large; font-weight: bold;"><a href="help.aspx#BulkProductAllocation">Help</a> : Bulk Product Allocation</span>
			<hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>The bulk product allocation report shows how much product is being called for in all the orders for a selected facility.</p>
				<table>
					<tr>
						<td><span class="helpItem">Facility:</span> selects the facility that the report will run for. The facility is required.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Account:</span> when selected, filters to show product allocation for orders with the selected account only.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Bulk product:</span> when selected, filters to show allocation for only the bulk product selected.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Display unit:</span> determines the unit of measure that the allocation quantities are displayed in.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Show report:</span> click to generate the report using the selected facility, account, product and display unit.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Printer friendly version:</span> opens a copy of the report in a new window without the Terminal Management header and side bar.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Download:</span> downloads the report data in a comma separated value (CSV) file that may be opened in any spreadsheet application.</td>
					</tr>
				</table>
				<p><span class="helpItem">E-mail to:</span> the bulk product allocation report may be e-mailed directly from the page. Enter the e-mail addresses for the recipients (multiple addresses may be separated by commas) and click the "Send" button.</p>
				<p><span class="helpItem">Add address:</span> will look in the e-mail history, and items to find possible e-mail addresses that the report may be sent to.</p>
			</div>
		</div>
	</form>
</body>
</html>
