<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="InterfaceItemsHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.InterfaceItemsHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head id="Head1" runat="server">
	<title>Help : Interface Update and Copy</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="InterfaceUsageReportHelp" runat="server">
		<div>
			<span style="font-size: large; font-weight: bold;"><a href="help.aspx#InterfaceItems">Help</a> : Interface Items </span>
			<hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>The interface update and copy page allows a user to enter the cross reference values for an interface directly, or to copy the values from another interface.</p>
				<p><span class="helpItem">Cross Reference Type:</span> select the type of item to assign the cross reference values to.</p>
				<p><span class="helpItem">Name:</span> the name of the item.</p>
				<p><span class="helpItem">Source Interface:</span> the name of the interface that the values can be copied from.</p>
				<p><span class="helpItem">Target Interface:</span> the name of the interface that will have the cross reference values assigned to.</p>
				<table>
					<tr>
						<td><span class="helpItem">Cross reference:</span> a cross reference used to identify the item when received in an order or sent as a ticket through an interface with a 3rd party software package. Cross reference may be up to 50 characters in length.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Interface exchange unit:</span> the unit of measure that the 3rd party software package will use when sending requests or reading delivered quantities. This field is only shown for bulk products or products, and if the interface type has the setting <i>Show interface exchange unit of measure</i> is checked.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Order item unit:</span> the unit of measure that will be used when creating an order. This field is only shown for products, and if the interface type has the setting <i>Use the interface request unit of measure for the order item's requested unit of measure</i> unchecked.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Split product according to formulation at facility:</span> designates the facility to use to determine the formulation of the product when splitting the product into components. This will only be shown if the interface type ha the option selected to split the product into components.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Default setting:</span> used when there are multiple values set for an item for an interface, and the interface setting was not set for the item to be exported for this interface (exporting manually created orders, exporting to an interface from which it wasn&#39;t imported from, etc.).</td>
					</tr>
					<tr>
						<td><span class="helpItem">Export only:</span> used only for outbound tickets, to allow for multiple cross references available for different items. If checked, then this cross reference will not be used for inbound interface lookups.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Order item sort priority:</span> is the number used for automatically sorting order items. the default value is 100. A number that is lower (closer to 1) will result in the order item being moved towards the top of the list. A number greater than 100 will move the order item towards the bottom of the list. If 2 items have the same priority, then their order relative to one another will remain the same. This field is only shown for products.</td>
					</tr>
				</table>
				<table>
					<tr>
						<td><span class="helpItem">Copy</span> copies the cross reference value(s) from the source interface to the target interface.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Copy All</span> copies all of the cross reference value(s) from the source interface to the target interface.</td>
					</tr>
				</table>
			</div>
			<p><span class="helpItem">Save</span> saves the cross reference value(s) to the target interface.</p>
		</div>
	</form>
</body>
</html>
