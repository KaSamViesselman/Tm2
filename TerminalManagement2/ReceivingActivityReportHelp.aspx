<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="ReceivingActivityReportHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.ReceivingActivityReportHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Help : Receiving Activity Report</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="ReceivingActivityReportHelp" runat="server">
		<div>
			<span style="font-size: large; font-weight: bold;"><a href="help.aspx">Help</a> : Receiving Activity Report</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>The receiving activity report may be used to show load details in a tabular format.</p>
				<p><span class="helpItem">From:</span> the oldest ticket records to display in the report.</p>
				<p><span class="helpItem">To:</span> the latest ticket records to include in the report.</p>
				<table>
					<tr>
						<td style="width: 50%; border-right: 1px solid #646464;">
							<p><span class="helpItem">Filters</span></p>
							<table>
								<tr>
									<td><span class="helpItem">Supplier account:</span> select an account to only include ticket records that reference the selected supplier account in the report.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Bulk product:</span> select a bulk product to only include ticket records that reference the selected bulk product in the report.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Owner:</span> select an owner to only include ticket records that reference the selected owner in the report.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Facility:</span> select a facility to only include ticket records created at the selected facility in the report.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Driver:</span> select a driver to only include ticket records that reference the selected driver in the report.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Transport:</span> select a transport to only include ticket records that reference the selected transport in the report.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Carrier:</span> select a carrier to only include ticket records that reference the selected carrier in the report.</td>
								</tr>
							</table>
							<p><span class="helpItem">Sort:</span> determines how the data in the report is sorted.</p>
							<p><span class="helpItem">Include voided tickets:</span> check to display previously voided tickets in the report.</p>
						</td>
						<td style="width: 50%;">
							<p><span class="helpItem">Columns</span></p>
							<table>
								<tr>
									<td><span class="helpItem">Date/time:</span> show the date and time that the load was loaded.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Order number:</span> show the order number that the load was for.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Ticket number:</span> select an owner to only include ticket records that reference the selected owner in the report.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Supplier:</span> show the supplier that the load was for.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Owner:</span> show the owner that the load was for.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Facility:</span> show the facility that the load was loaded at.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Driver:</span> show the driver that picked up the load.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Transports:</span> show the transport(s) used to haul the load.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Carrier:</span> show the carrier used to transport the load.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Panel:</span> show the panel(s) that the load was loaded with.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Notes:</span> show the notes from the receiving ticket.</td>
								</tr>

								<tr>
									<td>
										<span class="helpItem">Lot number:</span> show the lot number associated with each receiving ticket.
										<p class="indentedNote">This option will only be displayed if Item Traceability is enabled.</p>
									</td>
								</tr>
							</table>
							<p><span class="helpItem">Unit of measure:</span> determines what unit of measure product/bulk product quantities are displayed in.</p>
							<p><span class="helpItem">Number of digits after the decimal point:</span> determines how many decimal places should be shown for the quantities shown on the report.</p>
							<p><span class="helpItem">Total unit of measure:</span> determines what unit of the total product/bulk product quantities are displayed in. Multiple units may be selected. A sample of the number of decimals is displayed in parenthesis after the name of the unit.</p>
							<p><span class="helpItem">Adjust number of digits after the decimal point:</span> will change the number of decimals to be displayed after the unit of measure.</p>
						</td>
					</tr>
				</table>
				<p><span class="helpItem">E-mail to:</span> the receiving activity report may be e-mailed directly from the page. Enter the e-mail addresses for the recipients (multiple addresses may be separated by commas) and click the "Send" button.</p>
				<p><span class="helpItem">Add address:</span> will look in the e-mail history, and items to find possible e-mail addresses that the report may be sent to.</p>
				<p><span class="helpItem">Show report:</span> click to show the report with the selected filter, column and sort selections. This option will have the hyper-links to the individual tickets.</p>
				<p><span class="helpItem">Download report:</span> click to download a comma separated value (CSV) file containing the report data in a file format that may be opened by a spreadsheet application.</p>
				<p><span class="helpItem">Printer friendly:</span> click to show the report with the selected filter, column and sort selections. This option will not have the hyper-links to the individual tickets.</p>
			</div>
		</div>
	</form>
</body>
</html>
