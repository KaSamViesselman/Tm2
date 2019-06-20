<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="AdvancedSearchHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.AdvancedSearchHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head id="Head1" runat="server">
	<title>Help : Advanced Search</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="AdvancedSearchHelp" runat="server">
		<div>
			<span style="font-size: large; font-weight: bold;"><a href="help.aspx#AdvancedSearch">Help</a> : Advanced Search</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>The advanced search is used to search for orders or tickets assigned to a particular entity.</p>
				<p>The list of search options depends on if orders or tickets are being searched.</p>
				<table>
					<tr>
						<td style="width: 50%; border-right: 1px solid #646464;">
							<p><span class="helpItem">Filters</span></p>
							<table>
								<tr>
									<td><span class="helpItem">Customer:</span> select an account to only include ticket records or orders that reference the selected customer account.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Supplier:</span> select a supplier to only include receiving ticket records or orders that reference the selected supplier account.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Owner:</span> select an owner to only include ticket records or orders that reference the selected owner.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Branch:</span> select an branch to only include ticket records or orders that reference the selected branch.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Product:</span> select a product to only include ticket records or orders that reference the selected product.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Bulk product:</span> select a bulk product to only include receiving ticket records or orders that reference the selected bulk product.</td>
								</tr>
								<tr>
									<td><span class="helpItem">From:</span> the oldest ticket records to display in the report.</td>
								</tr>
								<tr>
									<td><span class="helpItem">To:</span> the latest ticket records to include in the report.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Include voided tickets:</span> determines if the activity report should include voided tickets in the list.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Keyword:</span> will look at the data to match the keyword wit fields available in the ticket or order.</td>
								</tr>
							</table>
							<p><span class="helpItem">Sort:</span> determines how the data is sorted.</p>
							<p>
						</td>
						<td style="width: 50%;">
							<p><span class="helpItem">Columns</span> will be shown when tickets are being searched.</p>
							<table>
								<tr>
									<td><span class="helpItem">Date/time:</span> show the date and time that the load was loaded.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Order number:</span> show the order number that the load was for.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Ticket number:</span> select an owner to only include ticket records that reference the selected owner.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Customer:</span> show the customer(s) that the load was for.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Customer destination:</span> show the destination that the load was shipped to.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Owner:</span> show the owner that the load was for.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Branch:</span> show the branch that the load was for.</td>
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
									<td><span class="helpItem">Discharge locations:</span> show the discharge locations that were used to dispense the load.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Include interface cross references:</span> will determine if interface cross references should be included in the search parameters.</td>
								</tr>
							</table>
						</td>
					</tr>
				</table>
				<p><span class="helpItem">Filter:</span> click to show the results with the selected filter, column and sort selections. This option will have the hyper-links to the individual tickets and orders.</p>
			</div>
		</div>
	</form>
</body>
</html>
