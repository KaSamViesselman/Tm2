<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="OrdersHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.OrdersHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Help : Orders</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="OrdersHelp" runat="server">
		<div>
			<span style="font-size: large; font-weight: bold;"><a href="help.aspx#Orders">Help</a> : Orders</span>
			<hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>Order records represents a pre-entered request for one or more products by one or more customer accounts. Orders may be entered and modified with Terminal Management, but may also be created by a 3rd party software via an interface.</p>
				<p><span class="helpItem">Facility drop-down list:</span> contains all the facilities set up in the system. If a particular facility is selected, then only the orders that have bulk products assigned to all of the products on the order at that facility will be displayed.</p>
				<p><span class="helpItem">Order drop-down list:</span> contains all the orders that are still loadable. If the user logged in has been assigned to an owner, they will only see that owner's orders in the drop-down list. To modify an existing order, select the order number in the drop-down list. To create a new order, select "Enter a new order" in the drop-down list.</p>
				<table>
					<tr>
						<td><span class="helpItem">Copy:</span> used to duplicate the selected order one or more times. After clicking the "Copy this order" button there are two values that must be specified. The "Preceding order number" specifies the order number to start with. If the preceding order number ends with a numeric value (e.g. ABC0001), then the copy orders will start with a number +1 from the "Preceding order number" (e.g. ABC0002, ABC0003, etc.), otherwise a number is appended to the end of the order number (e.g. 123ABC-001, 123ABC-002, 123ABC-003, etc.). The "Number of copies" indicates the number of copies to create. Once those values have been entered, click the "Copy" button. To cancel the copy, click the "Cancel" button.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Print:</span> opens the selected order in a new window without the Terminal Management title bar or side bar, suitable for printing.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Mark complete:</span> marks the selected order as complete (no longer loadable). The order will be removed from the order drop-down list and moved to "Past orders".</td>
					</tr>
					<tr>
						<td><span class="helpItem">Create point of sale:</span> creates a ticket against the order for product sold but not requiring automation to load.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Set/Clear Locked Status:</span> changes the locked status of the order.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Find:</span> searches through current orders, and finds the next order that has the search keyword in its fields.</td>
					</tr>
				</table>
				<hr />
				<span class="helpItem">General order information</span>
				<table>
					<tr>
						<td style="width: 50%; border-right: 1px solid #646464;">
							<table>
								<tr>
									<td><span class="helpItem">Order number:</span> the order number is required and may be up to 50 characters in length.</td>
								</tr>
								<tr>
									<td><span class="helpItem">PO number:</span> the PO (purchase order) number is optional and may be up to 50 characters in length.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Interface:</span> the interface to a 3rd party software package that the selected order belongs to. This is the interface that the order, in most cases, came from and will return to.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Batches:</span> the number of batches that have been delivered, and the number of batches that have been requested.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Acres:</span> the number of acres that the order is to be applied on. The acres information may be used by the load-out software to adjust the load quantity by total number of acres that the load is for. Acres information is optional.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Applicator:</span> the name of the applicator assigned to the order.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Ticket notes:</span> notes that can be shown on the ticket. The ticket notes are optional.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Internal notes:</span> notes that can be shown in some dispensing applications for the operator to see. These notes will not be displayed on the delivery ticket. The internal notes are optional.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Ship to:</span> the name and address where the order is to be delivered to. The ship to information is optional.</td>
								</tr>
							</table>
						</td>
						<td style="width: 50%;">
							<table>
								<tr>
									<td><span class="helpItem">Release number:</span> the release number is optional and may be up to 50 characters in length.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Over scaling (%):</span> the percentage above the requested quantities that a driver may scale an order in a self-serve system. Over scaling must be a numeric value greater than or equal to 0.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Branch:</span> the branch that generated this order. The branch is optional.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Owner:</span> the owner for this order. The owner is required.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Do not blend:</span> indicates whether the products in this order should be restricted from being blended when the order is loaded.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Transport type:</span> indicates the default transport type requested for this order.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Expiration date:</span> indicates the date that this order is scheduled to expire. This is used by some agronomy packages to automatically remove orders form the system after a period of time. There is a calendar option that will open a dialog box to enter the expiration date using a visual calendar.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Order locked:</span> used by the dispensing/loading software to indicate that an order is currently being loaded (to prevent the order from being loaded twice). If the dispensing/loading software is interrupted while loading an order the order locked status may need to be cleared manually on the order page before it is available to load again.</td>
								</tr>
							</table>
						</td>
					</tr>
				</table>
				<br />
				<hr />
				<br />
				<table style="width: 100%;">
					<tr>
						<td style="width: 50%; border-right: 1px solid #646464;"><span class="helpItem">Product(s) to be dispensed</span>
							<table>
								<tr>
									<td><span class="helpItem">Product drop-down list:</span> the product to be sold on this order or a function to be performed on the load. At least one product is required on an order. If this order was imported using an interface, then this list may not be enabled due to the settings for the interface type.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Remove:</span> will remove the current product from the order.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Requested:</span> the quantity that was originally requested for the selected product/function. The requested quantity is required.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Delivered:</span> the quantity that has already been loaded for the selected product.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Unit of measure drop-down list:</span> the unit of measure that the request and delivered amounts are measured in.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Grouping:</span> if the order has blend grouping assigned to it, then this column will be visible. Blend grouping is used for <span style="font-style: italic;">Do not blend</span> orders, to force certain products to be treated as a single product and blended together. If this order was imported using an interface, then this list may not be enabled due to the settings for the interface type.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Arrow up and arrow down:</span> will change the order of the products on the order.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Notes:</span> the notes are optional.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Add another product:</span> adds another product row.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Product priority sort:</span> will sort the products on the order based upon the interface selected for the order. If there is not an interface setting set up for the product, it will assume a priority value of 100.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Add grouping:</span> will add a value to be used for blend grouping for <span style="font-style: italic;">Do not blend</span> orders. If this order was imported using an interface, then this section may not be visible due to the settings for the interface type.</td>
								</tr>
							</table>
						</td>
						<td style="width: 50%;"><span class="helpItem">Account(s) to be billed</span>
							<table>
								<tr>
									<td><span class="helpItem">Account drop-down list:</span> the account to be billed for this order. At least one account is required on an order. Split billing is possible by adding more accounts. If this order was imported using an interface, then this list may not be enabled due to the settings for the interface type.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Remove:</span> will remove the current account from the order.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Percent:</span> the percent of the bill that this account is responsible for. The total percent of all the selected accounts must add to 100%.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Add another account:</span> adds another account row.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Update total:</span> updates the total percent display based on the percentage values entered for each account.</td>
								</tr>
							</table>
						</td>
					</tr>
				</table>
			</div>
			<hr />
			<p><span class="helpItem">Save button:</span> if "Enter a new order" is selected on the order drop-down list a new order will be created with the information entered, otherwise the information for the selected order is updated.</p>
			<p><span class="helpItem">Save & New button:</span> if "Enter a new order" is selected on the order drop-down list a new order will be created with the information entered, otherwise the information for the selected order is updated. The form will then clear all data, and be ready to enter a new orders information.</p>
			<p><span class="helpItem">Delete button:</span> will delete the selected order.</p>
		</div>
	</form>
</body>
</html>
