<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="InterfaceAssignOrderHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.InterfaceAssignOrderHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head id="Head1" runat="server"><title>Help : Assign Interface to Orders</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="InterfaceAssignOrderHelp" runat="server">
		<div><span style="font-size: large; font-weight: bold;"><a href="help.aspx#InterfaceAssignOrder">Help</a> : Assign Interface to Orders</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>The assign interface to order page will assign the interface to be used to an order. An interface should only be assigned to an order if the interface selected is able to properly handle the creation of tickets.</p>
				<p><span class="helpItem">Facility drop-down list:</span> is a list containing all the facilities set up in the system. If a particular facility is selected, then only the orders that have bulk products assigned to all of the products on the order at that facility will be displayed.</p>
				<p><span class="helpItem">Order drop-down list:</span> the order drop-down list contains all the orders that are still loadable. If the user logged in has been assigned to an owner, they will only see that owner's orders in the drop-down list. To modify an existing order, select the order number in the drop-down list. To create a new order, select "Enter a new order" in the drop-down list.</p>
				<p><span class="helpItem">Find:</span> this may be used to search through current orders, and find orders that have the search keyword in their fields.</p>
				<p><span class="helpItem">Interface:</span> the interface to a 3rd party software package that the selected order belongs to. This is the interface that the order, in most cases, came from and will return to.</p>
			</div>
		</div>
	</form>
</body>
</html>
