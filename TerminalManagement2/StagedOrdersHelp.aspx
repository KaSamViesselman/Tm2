<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="StagedOrdersHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.StagedOrdersHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Help : Staged Orders</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="StagedOrdersHelp" runat="server">
		<div>
			<span style="font-size: large; font-weight: bold;"><a href="help.aspx#StagedOrders">Help</a> : Staged Orders</span>
			<hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>Staged orders may be used to setup load information such as transport, carrier and driver prior to the vehicle being at the load-out point. This is typically done to minimize the amount of communication that needs to take place between the load-out operator and the driver.</p>
				<p><span class="helpItem">Staged order drop-down list:</span> select an existing staged order from the list to modify or select "New staged order" to create a new staged order record.</p>
				<table style="width: 100%;">
					<tr>
						<td style="width: 50%; border-right: 1px solid #404040;">
							<table>
								<tr>
									<td><span class="helpItem">Facility:</span> select the facility that this staged order should be loaded in. This is a required field.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Bay:</span> select the bay that this staged order should be loaded in.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Order number:</span> select the order(s) that this staged order is for.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Tickets are created from amount filled in facility:</span> sets whether the ticket should be created by using the dispensed quantity from the loading process.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Tickets are created from amount used off-site:</span> sets whether the ticket should be created by using the amount of product unloaded off-site.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Use Order Percents:</span> sets whether the quantities should be divided between the selected orders by percentage.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Ship to:</span> the ship to information for the order.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Carrier:</span> select the carrier/shipping company that will be transporting this load.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Driver:</span> select the driver that will be picking this load up.</td>
								</tr>
							</table>
							<p><span class="helpItem">Order details:</span> shows the details for the selected order.</p>
						</td>
						<td style="width: 50%;">
							<p><span class="helpItem">Transports</span> </p>
							<table>
								<tr>
									<td><span class="helpItem">Transport:</span> select the transport(s) that will be loaded with this load.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Remove transport:</span> removes the transport from the staged order.</td>
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
									<td><span class="helpItem">Product:</span> selects the product(s) to be loaded into the compartment on the transport.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Quantity:</span> indicates the quantity that should be loaded into the compartment.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Remove product:</span> removes the product from the transport compartment.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Add product:</span> adds another product line to the transport compartment.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Remove compartment:</span> removes the compartment from the transport.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Move compartment up:</span> moves the compartment 1 level up in the transport.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Move compartment down:</span> moves the compartment 1 level down in the transport.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Add compartment:</span> adds another compartment to the transport.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Add transport:</span> adds another transport to the staged order.</td>
								</tr>
							</table>
						</td>
					</tr>
				</table>
				<table>
					<tr>
						<td colspan="2"><span class="helpItem">Shortcuts:</span> used to fill compartments with products and quantities.</td>
					</tr>
					<tr>
						<td style="width: 50%;">
							<table>
								<tr>
									<td><span class="helpItem">Use remaining order quantity:</span> when clicked, the total quantity to be dispensed for the load is calculated as the difference between the order total requested and total delivered. The total quantity is then distributed proportionally over all the specified compartments utilizing the transport and compartment capacities.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Use original order quantity:</span> when clicked, the total quantity to be dispensed for the load is set as the order total requested. The total quantity is then distributed proportionally over all the specified compartments utilizing the transport and compartment capacities.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Use application rate:</span> when clicked, the total quantity to be dispensed for the load is calculated using the order application rate and the total number of acres specified for the load. The total quantity is then distributed proportionally over all the specified compartments utilizing the transport and compartment capacities.</td>
								</tr>
							</table>
						</td>
						<td style="width: 50%;">
							<table>
								<tr>
									<td><span class="helpItem">Use transport capacity:</span> when clicked, the total quantity to be dispensed for the load is calculated as the total capacity of the selected transports. The total quantity is then distributed proportionally over all the specified compartments. The amount assigned does not take into consideration the order quantity. For example, if a transport has a tare weight of 28680 lb., and a maximum weight of 80000 lb.; and the order is for 45000 lb. with a 10% over scaling allowed percentage, the load will be created for 51320 lb., which is greater than the amount defined by the order with the over scaling allowed percentage.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Use batch quantity:</span> when clicked, the total quantity to be dispensed for the load is calculated using the order total requested divided by the number of requested batches for the order. The total quantity is then distributed proportionally over all the specified compartments utilizing the transport and compartment capacities.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Specify total quantity:</span> when clicked, a numeric keypad appears prompting for the total quantity to be loaded. After clicking the “OK” button on the numeric keypad, the entered quantity will be distributed over the load compartments.</td>
								</tr>
							</table>
						</td>
					</tr>
				</table>
				<p><span class="helpItem">Internal Notes:</span> notes that can be shown in some dispensing applications for the operator to see. These notes will not be displayed on the delivery ticket. The internal notes are optional.</p>
				<p><span class="helpItem">Save:</span> saves changes made to an existing staged order record when selected or creates a new staged order record when "New staged order" is selected in the staged order drop-down list.</p>
				<p><span class="helpItem">Delete:</span> deletes the selected staged order record.</p>
				<p><span class="helpItem">Create Point of Sale:</span> will allow the user to create a ticket using the currently selected staged order as a framework.</p>
				<p><span class="helpItem">Print:</span> will print a pick ticket for the information entered for this staged order.</p>
				<p><span class="helpItem">Set/Clear Locked Status:</span> this may be used to change the locked status of the staged order.</p>
			</div>
	</form>
</body>
</html>
