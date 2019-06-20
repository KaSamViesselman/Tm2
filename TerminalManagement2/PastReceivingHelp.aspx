<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="PastReceivingHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.PastReceivingHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Help : Past Receiving Purchase Orders Help</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="PastReceivingHelp" runat="server">
		<div>
			<span style="font-size: large; font-weight: bold;"><a href="help.aspx#PastReceivingPurchaseOrders">Help</a> : Past Receiving Purchase Order Help</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>The past receiving purchase orders page may be used to review purchase orders that have been marked as complete and are no longer receivable.</p>
				<p><span class="helpItem">Facility drop-down list:</span> contains all the facilities set up in the system. If a particular facility is selected, then only the receiving purchase orders that have a product/bulk product formulation at the facility will be displayed.</p>
				<p><span class="helpItem">Receiving purchase order drop-down list:</span> select the receiving purchase order to review from the drop-down list.</p>
				<table>
					<tr>
						<td><span class="helpItem">Mark incomplete:</span> clears the complete status of the purchase order, making it receivable again. If a purchase order is marked incomplete it will be removed from the past receiving purchase order list and moved to "Receiving Purchase Orders".</td>
					</tr>
					<tr>
						<td><span class="helpItem">Archive:</span> used to mark the purchase order as archived. If a purchase order that is archived has tickets, then the user will be asked if they wish to archive the tickets associated with the purchase order.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Unarchive:</span> used to remove the archived status for the purchase order. Tickets that have been marked as archived will not have their archived status changed.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Print PO:</span> opens the receiving purchase order in a new window with formatting suitable for printing.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Show usages:</span> will display any usages of the received lot(s) associated with this purchase order. A lot must be assigned when the receipt is created.</td>
						<p class="indentedNote">This option will only be displayed if Item Traceability is enabled, and there are usages associated with the received lot(s).</p>
					</tr>
					<tr>
						<td><span class="helpItem">Ticket:</span> select a receiving purchase order ticket to review.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Show voided tickets:</span> check to display previously voided tickets in tickets drop down list.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Print ticket:</span> opens the selected ticket in a new window with formatting suitable for printing.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Void ticket:</span> voids the selected ticket.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Show usages (for ticket):</span> will display any usages of the received lot associated with this ticket. A lot must be assigned when the receipt is created.</td>
						<p class="indentedNote">This option will only be displayed if Item Traceability is enabled, and there are usages associated with the received lot.</p>
					</tr>
				</table>
			</div>
		</div>
	</form>
</body>
</html>
