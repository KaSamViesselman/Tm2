<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="BulkProductUsageReportHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.BulkProductUsageReportHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head id="Head1" runat="server">
	<title>Help : Bulk Product Usage Report</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="BulkProductUsageReportHelp" runat="server">
		<div>
			<span style="font-size: large; font-weight: bold;"><a href="help.aspx#BulkProductUsageReport">Help</a> : Bulk Product Usage Report</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>The bulk product usage report page shows all of the bulk products used over a selectable date range.</p>
				<table>
					<tr>
						<td style="width: 50%; border-right: 1px solid #404040;">
							<label style="font-weight: bold; text-align: left;">Filters</label><table>
								<tr>
									<td><span class="helpItem">Start date:</span> the oldest loaded date for a ticket to include in the quantities.</td>
								</tr>
								<tr>
									<td><span class="helpItem">End date:</span> the newest loaded date for a ticket to include in the quantities.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Owner:</span> select an owner to only include ticket records that reference the selected owner in the report.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Bay:</span> select an bay to only include ticket records that reference the selected bay in the report.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Panel:</span> select an panel to only include ticket records that were loaded using the selected panel in the report.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Unit of measure:</span> determines what unit of the bulk product quantities are displayed in. Multiple units may be selected. A sample of the number of decimals is displayed in parenthesis after the name of the unit.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Adjust number of digits after the decimal point:</span> will change the number of decimals to be displayed after the unit of measure.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Include all bulk products:</span> when selected, will include all bulk products used in the report. If not selected, then the list of bulk products to include in the report will be displayed.</td>
								</tr>
							</table>
						</td>
						<td style="width: 50%;">
							<label style="font-weight: bold; text-align: left;">E-mail</label><table>
								<tr>
									<td><span class="helpItem">E-mail to:</span> the bulk product usage report may be e-mailed directly from the page. Enter the e-mail addresses for the recipients (multiple addresses may be separated by commas) and click the "Send" button.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Add address:</span> will look in the e-mail history, and items to find possible e-mail addresses that the report may be sent to.</td>
								</tr>
							</table>
						</td>
					</tr>
				</table>
				<p><span class="helpItem">Show report:</span> click to show the report with the selected filter, column and sort selections.</p>
				<p><span class="helpItem">Download report:</span> click to download a comma separated value (CSV) file containing the report data in a file format that may be opened by a spreadsheet application.</p>
				<%-- <p><span class="helpItem">Printer friendly:</span> click to show the report with the selected filter, column and sort selections. This option will not have the hyper-links to the individual tickets.</p>--%>
			</div>
		</div>
	</form>
</body>
</html>
