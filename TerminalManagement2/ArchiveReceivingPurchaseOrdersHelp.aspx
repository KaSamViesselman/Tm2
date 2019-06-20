<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="ArchiveReceivingPurchaseOrdersHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.ArchiveReceivingPurchaseOrdersHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Help : Archive Orders</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="ArchiveReceivingPurchaseOrdersHelp" runat="server">
		<div>
			<span style="font-size: large; font-weight: bold;"><a href="help.aspx#ArchiveReceivingPurchaseOrders">Help</a>: Archive Receiving Purchase Orders</span>
			<hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>The archive receiving purchase orders page may be used to archive/unarchive multiple past receiving purchase orders at the same time, as well as archive/unarchive a receiving purchase order's tickets.</p>
				<p><span class="helpItem">Created from date:</span> textbox for entering a date that corresponds to the oldest created date in the receiving purchase orders table to show.</p>
				<p><span class="helpItem">Created to date:</span> textbox for entering a date that corresponds to the newest created date in the receiving purchase orders table to show.</p>
				<p><span class="helpItem">Supplier drop-down list:</span> list containing all the suppliers set up in the system. If a particular supplier is selected, then only the receiving purchase orders that are associated with that supplier will be displayed.</p>
				<p><span class="helpItem">Facility drop-down list:</span> contains all the facilities set up in the system. If a particular facility is selected, then only the receiving purchase orders that have a product/bulk product formulation at the facility will be displayed.</p>
				<p><span class="helpItem">Owner drop-down list:</span> contains all the owners set up in the system. If a particular owner is selected, then only the receiving purchase orders that are associated with that owner will be displayed.</p>
				<p><span class="helpItem">Order number contains:</span> will limit the list to only the orders that contain the specified text.</p>
				<p><span class="helpItem">Show archived only checkbox:</span> used to filter the receiving purchase orders table to either show all unarchived receiving purchase orders or all archived receiving purchase orders. The Archive Order and Archive Tickets columns change depending on if this checkbox is checked or not.</p>
				<table>
					<tr>
						<td><span class="helpItem">Filter:</span> used to filter the receiving purchase orders table on the page. If there is an error in one of your filter selections, a dialog box will appear stating the error(s) and the receiving purchase orders table will be emptied.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Archive:</span> used to update the archived status of the checked receiving purchase orders, as well as update archived status for an receiving purchase order's tickets if Archive tickets is checked. This button will change to Unarchive when Show Archived Only is checked.</td>
					</tr>
				</table>
				<p><span class="helpItem">Receiving purchase orders table:</span> displays the past receiving purchase orders from the database, using filters as specified.</p>
				<table>
					<tr>
						<td><span class="helpItem">Purchase Order Number:</span> displays the purchase order number field from the receiving purchase order record.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Supplier:</span> displays the supplier associated with the receiving purchase order record.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Owner:</span> displays the owner of the receiving purchase order record.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Product:</span> displays the bulk product associated with the receiving purchase order record.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Created:</span> displays the created date of the receiving purchase order record.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Archive Order checkbox:</span> the receiving purchase orders that should be checked to either archive or unarchive, column header will be updated to indicate which action will be performed. Checking the first checkbox in the table will mark all of the checkboxes as checked.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Archive Tickets checkbox:</span> the tickets of the receiving purchase order checked to either archive or unarchive, column header will be updated to indicate which action will be performed. Checking the first checkbox in the table will mark all of the checkboxes as checked. A receiving purchase order's tickets can be archived without having to archive its receiving purchase order.</td>
					</tr>
				</table>
			</div>
		</div>
	</form>
</body>
</html>
