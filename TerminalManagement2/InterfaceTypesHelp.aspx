<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="InterfaceTypesHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.InterfaceTypesHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head id="Head1" runat="server"><title>Help : Interface Types</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="InterfaceTypesHelp" runat="server">
		<div><span style="font-size: large; font-weight: bold;"><a href="help.aspx#InterfaceTypes">Help</a> : Interface Types</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>Interface types are used to identify the type of an interface, typically on an interface record. Interface types are for reference only.</p>
				<p><span class="helpItem">Interface types drop-down list:</span> located at the top of the page, this list contains all the interface type records. To create a new interface type select "Enter a new interface type".</p>
				<table>
					<tr>
						<td style="width: 50%; border-right: 1px solid #404040">
							<table>
								<tr>
									<td><span class="helpItem">Name:</span> this is the name of the Interface type. The name is required and may be up to 50 characters long.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Show interface setup for applicators:</span> defines whether this interface is to be used with applicators.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Show interface setup for branches:</span> defines whether this interface is to be used with branches.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Show interface setup for bulk products:</span> defines whether this interface is to be used with bulk products.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Show interface setup for carriers:</span> defines whether this interface is to be used with carriers.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Show interface setup for drivers:</span> defines whether this interface is to be used with drivers.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Show interface setup for facilities:</span> defines whether this interface is to be used with facilities.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Show interface setup for owners:</span> defines whether this interface is to be used with owners.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Show interface setup for tanks:</span> defines whether this interface is to be used with tanks.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Show interface setup for transports:</span> defines whether this interface is to be used with transports.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Show interface setup for transport types:</span> defines whether this interface is to be used with transport types.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Show interface setup for units:</span> defines whether this interface is to be used with units.</td>
								</tr>
							</table>
						</td>
						<td style="width: 50%;">
							<table>
								<tr>
									<td><span class="helpItem">Show interface exchange unit of measure:</span> defines whether this interface should show the interface exchange unit selection for products and bulk products. Interfaces that do not have the requested unit of measure supplied in the interface should have this option selected.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Use the interface request unit of measure for the order item's requested unit of measure:</span> defines if the user will be allowed to set what unit of measure each product should be on an order when the order is imported. If this option is selected, then the order item will use the interface exchange unit of measure. If this option is not selected, then the user will be able to define what the unit of measure should be when imported.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Split imported products into separate bulk product order items:</span> defines if the interface should try to split the incoming product into its bulk product components, based upon the formulation facility assigned to the product in the product interface setup.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Allow user to change the product on an imported order:</span> defines if the interface allows the end user to change the product that was imported with the order. If a user needs to change the product to dispense on any order, then the proper step would be to un-assign the interface from the order, make the change, then reassign the interface to the order.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Allow user to change the customer on an imported order:</span> defines if the interface allows the end user to change the customer account that was imported with the order. If a user needs to change the customer account on any order, then the proper step would be to un-assign the interface from the order, make the change, then reassign the interface to the order.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Allow user to change the grouping for products on an imported order:</span> defines if the interface allows the end user to change the product grouping that was imported with the order. If a user needs to change the product grouping to dispense on any order, then the proper step would be to un-assign the interface from the order, make the change, then reassign the interface to the order.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Allow order to be marked as incomplete if tickets exist:</span> defines if the interface allows the end user to change the status of a past order from being completed to incomplete if there a tickets for the order.</td>
								</tr>
							</table>
						</td>
					</tr>
				</table>
			</div>
			<p><span class="helpItem">Save button:</span> located at the top of the page, click this button to save any changes made to an Interface type record or create a new record.</p>
			<p><span class="helpItem">Delete button:</span> located at the top of the page, click this button to delete the current Interface type record.</p>
		</div>
	</form>
</body>
</html>
