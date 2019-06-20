<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="ReceivingPurchaseOrderListHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.ReceivingPurchaseOrderListHelp" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Help : Receiving Purchase Order List</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="ReceivingPurchaseOrderListHelp" runat="server">
		<div>
			<span style="font-size: large; font-weight: bold;"><a href="help.aspx#ReceivingPurchaseOrdersList">Help</a> : Receiving Purchase Order List</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>The receiving purchase order list page shows a list of all the open purchase orders. If the user has been assigned to an owner, only the purchase orders assigned to that owner will be listed.</p>
				<table>
					<tr>
						<td><span class="helpItem">Sort By:</span> shows how the report with be ordered by.<br />
							<table style="font-style: italic; margin-left: 20px;">
								<tr>
									<td><span class="helpItem">Purchase order number:</span> the number for each purchase order.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Facility:</span> contains all the facilities set up in the system. If a particular facility is selected, then only the receiving purchase orders that have a product/bulk product formulation at the facility will be displayed.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Supplier name:</span> the list of the suppliers for each purchase order.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Bulk product name:</span> the list of bulk products to be received for each purchase order.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Owner:</span> the owner for each purchase order.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Created:</span> the date and time that each purchase order was created.</td>
								</tr>
							</table>
						</td>
					</tr>
					<tr>
						<td><span class="helpItem">Facility:</span> will limit the report based upon the selected facility. If a particular facility is selected, then only the receiving purchase orders that have a product/bulk product formulation at the facility will be displayed.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Supplier:</span> will limit the report based upon the selected supplier.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Bulk product:</span> will limit the report based upon the selected bulk product.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Owner:</span> will limit the report based upon the selected owner.</td>
					</tr>
				</table>
				<p><span class="helpItem">Show report:</span> click to show the report with the selected filter, column and sort selections.</p>
				<p><span class="helpItem">Download report:</span> click to download a comma separated value (CSV) file containing the report data in a file format that may be opened by a spreadsheet application.</p>
				<p><span class="helpItem">E-mail to:</span> the current order list may be e-mailed directly from the page. Enter the e-mail addresses for the recipients (multiple addresses may be separated by commas) and click the "Send" button.</p>
				<p><span class="helpItem">Add address:</span> will look in the e-mail history, and items to find possible e-mail addresses that the report may be sent to.</p>
			</div>
		</div>
	</form>
</body>
</html>
