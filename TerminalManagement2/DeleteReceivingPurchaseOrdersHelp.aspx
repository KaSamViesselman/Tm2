<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="DeleteReceivingPurchaseOrdersHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.DeleteReceivingPurchaseOrdersHelp" %>


<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Help : Delete Receiving Purchase Orders</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="DeleteReceivingPurchaseOrdersHelp" runat="server">
		<div>
			<span style="font-size: large; font-weight: bold;"><a href="help.aspx#DeleteReceivingPurchaseOrders">Help</a>: Delete Receiving Purchase Orders</span>
			<hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>The delete receiving purchase orders page may be used to delete or mark as complete multiple receiving purchase orders at the same time.</p>
				<p><span class="helpItem">Created from date:</span> textbox for entering a date that corresponds to the oldest created date in the receiving purchase orders table to show.</p>
				<p><span class="helpItem">Created to date:</span> textbox for entering a date that corresponds to the newest created date in the receiving purchase orders table to show.</p>
				<p><span class="helpItem">Supplier drop-down list:</span> list containing all the suppliers set up in the system. If a particular supplier is selected, then only the receiving purchase orders that are associated with that supplier will be displayed.</p>
				<p><span class="helpItem">Facility drop-down list:</span> contains all the facilities set up in the system. If a particular facility is selected, then only the receiving purchase orders that have a product/bulk product formulation at the facility will be displayed.</p>
				<p><span class="helpItem">Owner drop-down list:</span> contains all the owners set up in the system. If a particular owner is selected, then only the receiving purchase orders that are associated with that owner will be displayed.</p>
				<p><span class="helpItem">Order number contains:</span> will limit the list to only the orders that contain the specified text.</p>
				<p><span class="helpItem">Show completed only checkbox:</span> used to filter the receiving purchase orders table to either show all completed receiving purchase orders or all open receiving purchase orders.</p>
				<p><span class="helpItem">Show archived only checkbox:</span> used to filter the receiving purchase orders table to either show all unarchived receiving purchase orders or all archived receiving purchase orders. The Archive Order and Archive Tickets columns change depending on if this checkbox is checked or not.</p>
				<table>
					<tr>
						<td><span class="helpItem">Filter:</span> used to filter the receiving purchase orders table on the page. If there is an error in one of your filter selections, a dialog box will appear stating the error(s) and the receiving purchase orders table will be emptied.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Mark Complete:</span> used to update the completed status of each checked receiving purchase order in the table to true, and move each receiving purchase order to the "Past Receiving Purchase Orders" page.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Delete:</span> used to update the deleted status of each checked receiving purchase order in the table to true, and remove each receiving purchase order from the list of receiving purchase orders. If a receiving purchase order has tickets associated with it, the receiving purchase order will not be deleted, instead it will go through the mark as complete process. On page load, a message will appear with each receiving purchase order that was unable to be removed.</td>
					</tr>
				</table>
				<p><span class="helpItem">Receiving purchase orders table:</span> displays the open receiving purchase orders from the database, using filters as specified.</p>
				<table>
					<tr>
						<td><span class="helpItem">Purchase Order Number:</span> displays the number field from the receiving purchase order record.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Supplier:</span> displays the supplier associated with the receiving purchase order record.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Owner:</span> displays the owner of the receiving purchase order record.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Product:</span> displays the product associated with the receiving purchase order record.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Created:</span> displays the created date of the receiving purchase order record.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Notes:</span> displays a message if the receiving purchase order record has tickets in history and cannot be deleted.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Checkbox:</span> displays the receiving purchase orders that should be checked to either mark as complete or delete. Checking the first checkbox in the table will mark all of the checkboxes as checked.</td>
					</tr>
				</table>
			</div>
		</div>
	</form>
</body>
</html>
