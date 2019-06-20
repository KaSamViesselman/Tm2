<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="PastOrdersHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.PastOrdersHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Help : Past Orders</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="PastOrdersHelp" runat="server">
		<div>
			<span style="font-size: large; font-weight: bold;"><a href="help.aspx#PastOrders">Help</a>: Past Orders</span>
			<hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>The past orders page may be used to review orders that have been marked as complete and are no longer loadable.</p>
				<p><span class="helpItem">Facility drop-down list:</span> contains all the facilities set up in the system. If a particular facility is selected, then only the orders that tickets fulfilled at the facility will be displayed. Orders that do not have any tickets for them will be included if they have bulk products assigned to all of the products on the order.</p>
				<p><span class="helpItem">Past order drop-down list:</span> contains all the orders that have been marked as complete. If the user has been assigned to an owner only the orders for that owner will be available in this list.</p>
				<table>
					<tr>
						<td><span class="helpItem">Mark incomplete:</span> used to clear the complete status of the order, making it loadable again. If an order is marked incomplete it will be removed from the past order list and moved to "Orders".</td>
					</tr>
					<tr>
						<td><span class="helpItem">Archive:</span> used to mark the order as archived. If an order that is archived has tickets, then the user will be asked if they wish to archive the tickets associated with the order.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Unarchive:</span> used to remove the archived status for the order. Tickets that have been marked as archived will not have their archived status changed.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Printer Friendly:</span> used to open the selected order in a new window suitable for printing, without the Terminal Management header or side bar.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Find:</span> used to search for a past order.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Include archived:</span> used to include archived orders in the list of available orders.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Products:</span> shows a list of products in the order detailing the original request quantity and the quantity that was delivered.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Accounts:</span> shows a list of the customer accounts to be billed for the order.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Tickets:</span> shows a list of the tickets for the loads. The ticket number is a link to view the ticket.</td>
					</tr>
				</table>
			</div>
		</div>
	</form>
</body>
</html>
