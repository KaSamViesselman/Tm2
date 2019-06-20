<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="ArchiveOrdersHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.ArchiveOrdersHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Help : Archive Orders</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="ArchiveOrdersHelp" runat="server">
		<div>
			<span style="font-size: large; font-weight: bold;"><a href="help.aspx#ArchiveOrders">Help</a>: Archive Orders</span>
			<hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>The archive orders page may be used to archive/unarchive multiple past orders at the same time, as well as archive/unarchive an order's tickets.</p>
				<p><span class="helpItem">Created from date:</span> a date that corresponds to the oldest created date in the orders table to show.</p>
				<p><span class="helpItem">Created to date:</span> a date that corresponds to the newest created date in the orders table to show.</p>
				<p><span class="helpItem">Customer account drop-down list:</span> contains all the customer accounts set up in the system. If a particular customer account is selected, then only the orders that are associated with that customer will be displayed.</p>
				<p><span class="helpItem">Facility drop-down list:</span> contains all the facilities set up in the system. If a particular facility is selected, then only the orders that are associated with that facility will be displayed.</p>
				<p><span class="helpItem">Owner drop-down list:</span> contains all the owners set up in the system. If a particular owner is selected, then only the orders that are associated with that owner will be displayed.</p>
				<p><span class="helpItem">Order number contains:</span> will limit the list to only the orders that contain the specified text.</p>
				<p><span class="helpItem">Show archived only checkbox:</span> used to filter the orders table to either show all unarchived orders or all archived orders. The Archive Order and Archive Tickets columns change depending on if this checkbox is checked or not.</p>
				<table>
					<tr>
						<td><span class="helpItem">Filter:</span> used to filter the orders table on the page. If there is an error in one of your filter selections, a dialog box will appear stating the error(s) and the orders table will be emptied.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Archive:</span> used to update the archived status of the checked orders, as well as update archived status for an order's tickets if Archive tickets is checked. This button will change to Unarchive when Show Archived Only is checked.</td>
					</tr>
				</table>
				<p><span class="helpItem">Orders table:</span> displays the past orders from the database, using filters as specified.</p>
				<table>
					<tr>
						<td><span class="helpItem">Order number:</span> displays the order number field from the order record.</td>
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
						<td><span class="helpItem">Archive order checkbox:</span> the orders that should be checked to either archive or unarchive, column header will be updated to indicate which action will be performed. Checking the first checkbox in the table will mark all of the checkboxes as checked.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Archive tickets checkbox:</span> the tickets of the order checked to either archive or unarchive, column header will be updated to indicate which action will be performed. Checking the first checkbox in the table will mark all of the checkboxes as checked. An order's tickets can be archived without having to archive its order.</td>
					</tr>
				</table>
			</div>
		</div>
	</form>
</body>
</html>
