<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="DeleteOrdersHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.DeleteOrdersHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Help : Delete Orders</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="DeleteOrdersHelp" runat="server">
		<div>
			<span style="font-size: large; font-weight: bold;"><a href="help.aspx#DeleteOrders">Help</a>: Delete Orders</span>
			<hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>The delete orders page may be used to delete or mark as complete multiple orders at the same time.</p>
				<p><span class="helpItem">Created from date:</span> textbox for entering a date that corresponds to the oldest created date in the orders table to show.</p>
				<p><span class="helpItem">Created to date:</span> textbox for entering a date that corresponds to the newest created date in the orders table to show.</p>
				<p><span class="helpItem">Customer account drop-down list:</span> list containing all the customer accounts set up in the system. If a particular customer account is selected, then only the orders that are associated with that customer will be displayed.</p>
				<p><span class="helpItem">Facility drop-down list:</span> contains all the facilities set up in the system. If a particular facility is selected, then only the orders that are associated with that facility will be displayed.</p>
				<p><span class="helpItem">Owner drop-down list:</span> contains all the owners set up in the system. If a particular owner is selected, then only the orders that are associated with that owner will be displayed.</p>
				<p><span class="helpItem">Order number contains:</span> will limit the list to only the orders that contain the specified text.</p> 
				<table>
					<tr>
						<td><span class="helpItem">Filter:</span> used to filter the orders table on the page. If there is an error in one of your filter selections, a dialog box will appear stating the error(s) and the orders table will be emptied.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Mark complete:</span> used to update the completed status of each checked order in the table to true, and move each order to the "Past Orders" page. If an order checked to be completed is a staged order, the order will not be marked as complete, and a message will appear on the page with the Order Number unable to be completed.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Delete:</span> used to update the deleted status of each checked order in the table to true, and remove each order from the list of orders. If an order has tickets associated with it, the order will not be deleted, instead it will go through the mark as complete process. On page load, a message will appear with each order that was unable to be removed.</td>
					</tr>
				</table>
				<p><span class="helpItem">Orders table:</span> displays the open orders from the database, using filters as specified.</p>
				<table>
					<tr>
						<td><span class="helpItem">Order Number:</span> displays the number field from the order record.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Accounts:</span> displays the customer accounts associated with the order record. Column will show account name and percentage of order belonging to each account.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Owner:</span> displays the owner of the order record.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Product:</span> displays a comma separated list of products associated to the order record.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Created:</span> displays the created date of the order record.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Notes:</span> displays a message if the order record is staged and cannot be marked as complete or if the order record has tickets in history and cannot be deleted.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Checkbox:</span> displays the orders that should be checked to either mark as complete or delete. Checking the first checkbox in the table will mark all of the checkboxes as checked.</td>
					</tr>
				</table>
			</div>
		</div>
	</form>
</body>
</html>
