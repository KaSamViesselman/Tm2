<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="PointOfSaleHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.PointOfSaleHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Help : Point of Sale</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="PointOfSaleHelp" runat="server">
		<div>
			<span style="font-size: large; font-weight: bold;"><a href="help.aspx">Help</a> : Point of Sale</span>
			<hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>The point of sale allows the user to create a dispense ticket for an order.</p>
				<p><span class="helpItem">Order drop-down list:</span> contains all the orders that are still loadable. If the user logged in has been assigned to an owner, they will only see that owner's orders in the drop-down list.</p>
				<span class="helpItem">General order information</span>
				<table>
					<tr>
						<td style="width: 50%; border-right: 1px solid #404040;">
							<table>
								<tr>
									<td><span class="helpItem">Facility:</span> the facility that is assigned to the ticket.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Bay:</span> the bay that is assigned to the ticket.  this can also be used to determine what pre-load and post-load questions should be asked.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Panel:</span> the panel that is assigned to the ticket.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Carrier:</span> the carrier that is assigned to the ticket.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Driver:</span> the driver that is assigned to the ticket.</td>
								</tr>

							</table>
						</td>
						<td style="width: 50%;">
							<table>
								<tr>
									<td><span class="helpItem">Order number:</span> the order number(s) of the order(s).</td>
								</tr>
								<tr>
									<td><span class="helpItem">Customer:</span> the name and applied percentage of the accounts that are on the order that is selected on the order number list.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Ship To:</span> the account destination that will be assigned to the ticket for the order that is selected on the order number list.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Applicator:</span> the name of the applicator to be referenced on the ticket for the order that is selected on the order number list.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Acres:</span> the number of acres that is to be applied to the order that is selected on the order number list.</td>
								</tr>

							</table>
						</td>
					</tr>
					<tr>
						<td style="border-right: 1px solid #404040;">
							<table>
								<tr>
									<td><span class="helpItem">Transport:</span> the transport that is assigned to the ticket.</td>
								</tr>
								<tr>
									<td>
										<p><span class="helpItem">Transport tare info</span></p>
										<table>
											<tr>
												<td><span class="helpItem">Weight:</span> the tare weight of the transport that will be loaded with the selected order.</td>
											</tr>
											<tr>
												<td><span class="helpItem">Date:</span> the date and time that the tare weight was read.</td>
											</tr>
											<tr>
												<td><span class="helpItem">Assign Tare:</span> will transfer the tare information stored for the transport to the staged order record.</td>
											</tr>
										</table>
									</td>
								</tr>
								<tr>
									<td>
										<span class="helpItem">Products</span>
										<table>
											<tr>
												<td><span class="helpItem">Product Name:</span> the name of the product that is on the order.</td>
											</tr>
											<tr>
												<td><span class="helpItem">Product Requested Amount:</span> the amount requested of the product that is on the order.</td>
											</tr>
											<tr>
												<td><span class="helpItem">Delivered:</span> the amount of the product that is delivered on the ticket.</td>
											</tr>
										</table>
									</td>
								</tr>
							</table>
						</td>
						<td>
							<table>
								<tr>
									<td>
										<span class="helpItem">Discharge into storage location(s):</span> allows the user to specify storage location(s) that the ticket was dispensed into.
										<p class="indentedNote">This option will only be displayed if Item Traceability is enabled.</p>
									</td>
								</tr>
								<tr>
									<td>
										<span class="helpItem">Pre-load questions:</span> are questions that are typically answered prior to dispensing product. The questions displayed are enabled on the "Order Settings" section of "General Settings". 
									</td>
								</tr>
								<tr>
									<td>
										<span class="helpItem">Post-load questions:</span> are questions that are typically answered after dispensing product. The questions displayed are enabled on the "Order Settings" section of "General Settings". 
									</td>
								</tr>
								<tr>
									<td>
										<span class="helpItem">Internal notes:</span> the internal notes from the order or staged order.
									</td>
								</tr>
							</table>
						</td>
					</tr>
				</table>
			</div>
			<p><span class="helpItem">Create ticket button:</span> creates a new ticket for the selected order.</p>
			<p><span class="helpItem">Cancel button:</span> cancels the creation of a ticket for the selected order.</p>
		</div>
	</form>
</body>
</html>
