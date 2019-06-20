<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="ReceivingHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.ReceivingHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Help : Receiving Purchase Orders</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="ReceivingHelp" runat="server">
		<div>
			<span style="font-size: large; font-weight: bold;"><a href="help.aspx#ReceivingPurchaseOrders">Help</a> : Receiving Purchase Orders</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>Receiving purchase order records represent orders to receive bulk material at a facility. When a purchase order is received at a facility, the inventory of the bulk product for the selected owner at the selected facility is automatically increased by the quantity received.</p>
				<p><span class="helpItem">Facility drop-down list:</span> contains all the facilities set up in the system. If a particular facility is selected, then only the receiving purchase orders that have a product/bulk product formulation at the facility will be displayed.</p>
				<p><span class="helpItem">Receiving purchase order drop-down list:</span> to modify an existing receiving purchase order select it from the drop-down list, otherwise to create a new receiving purchase order select "Enter new receiving purchase order" from the drop-down list.</p>
				<table>
					<tr>
						<td style="width: 50%; border-right: 1px solid #646464;">
							<table>
								<tr>
									<td><span class="helpItem">Copy:</span> make one or more copies of this receiving purchase order.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Print:</span> open this purchase order in a new window in a format that is suitable for printing.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Mark complete:</span> mark the purchase order as complete, so that it is no longer available to receive against.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Receive:</span> manually enter a quantity that has been received.</td>
								</tr>
								<tr>
									<td>
										<span class="helpItem">Show usages:</span> will display the dispensing tickets that included bulk ingredients received with this receiving purchase order. 
										<p class="indentedNote">This option will only be displayed if Item Traceability is enabled, and if there are dispensing tickets that used product from a storage location where this purchase order was received into.</p>
									</td>
								</tr>
							</table>
							<table>
								<tr>
									<td><span class="helpItem">Number:</span> the number is required and may be up to 50 characters in length.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Owner:</span> the owner that is receiving the bulk product. The owner is required.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Supplier:</span> the supplier that is selling the bulk product to the owner. The supplier is required.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Bulk product:</span> the bulk product being received. The bulk product is required.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Notes:</span> will show up on all tickets created for this particular Receiving Purchase Order.  Notes are optional.  Ticket specific notes can be set during ticket creation (receiving against a Receiving Purchase Order).</td>
								</tr>
								<tr>
									<td><span class="helpItem">Purchased:</span> the quantity of bulk product that the owner has purchased from the supplier.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Delivered:</span> a reference to the quantity of bulk product that has already been delivered under this purchase order.</td>
								</tr>
							</table>
							<p><span class="helpItem">Save:</span> saves changes to an existing purchase order if selected in the receiving purchase order drop-down list, otherwise creates a new purchase order record if "Enter new receiving purchase order" is selected in the receiving purchase order drop-down list.</p>
							<p><span class="helpItem">Delete:</span> deletes the selected receiving purchase order.</p>
						</td>
						<td style="width: 50%;"><span class="helpItem">Tickets</span>
							<table>
								<tr>
									<td><span class="helpItem">Ticket:</span> select a ticket to review for the selected purchase order.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Show voided tickets:</span> check to display previously voided tickets in tickets drop down list.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Print ticket:</span> opens the selected ticket in a new window with formatting suitable for printing.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Void ticket:</span> click to void the selected ticket. This will undo any inventory transactions that occurred when the ticket was created. The date of the inventory records that will undo the original records will be set to the local time of the Terminal Management 2 Web Server.</td>
								</tr>
								<tr>
									<td>
										<span class="helpItem">Show usages:</span> will display the dispensing tickets that included bulk ingredients received with this ticket. 
										<p class="indentedNote">This option will only be displayed if Item Traceability is enabled, and if there are dispensing tickets that used product from a storage location where this ticket was received into.</p>
									</td>
								</tr>
							</table>
							<span class="helpItem">Receiving</span>
							<table>
								<tr>
									<td><span class="helpItem">Delivered:</span> the quantity of bulk product that was delivered. The delivered quantity is required and must be greater than 0.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Carrier:</span> the carrier/shipping company used to deliver the bulk product. The carrier is optional.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Transport:</span> the transport used to deliver the bulk product. The transport is optional.</td>
								</tr>
								<tr>
									<td>
										<p><span class="helpItem">Transport tare info</span> </p>
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
									<td><span class="helpItem">Driver:</span> the driver that delivered the bulk product. The driver is optional.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Facility:</span> the facility that is receiving the bulk product. The facility is required.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Ticket Notes:</span> are additional notes that will be available to print on the receiving ticket that is being created. The notes are optional.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Lot number:</span> allows the user to specify a specific lot number to be used.<br />
										<span class="indented">If the user selects the option to create a new lot, then an area will be displayed for the user to enter the new lot's number.</span>
										<p class="indentedNote">This option will only be displayed if Item Traceability is enabled.</p>
									</td>
								</tr>
							</table>
							<table>
								<tr>
									<td><span class="helpItem">Receive:</span> create a receiving ticket using the information entered.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Cancel:</span> cancel the receiving and return to the purchase order form.</td>
								</tr>
							</table>
						</td>
					</tr>
				</table>
			</div>
		</div>
	</form>
</body>
</html>
