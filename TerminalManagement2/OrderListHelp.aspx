<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="OrderListHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.OrderListHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Help : Order List</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="OrderListHelp" runat="server">
		<div>
			<span style="font-size: large; font-weight: bold;"><a href="help.aspx#OrderList">Help</a> : Order List</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>The order list page shows a list of all the loadable orders. If the user has been assigned to an owner, only the orders assigned to that owner will be listed.</p>
				<table>
					<tr>
						<td><span class="helpItem">Sort by:</span> shows how the report with be ordered by.<br />
							<table style="font-style: italic; margin-left: 20px;">
								<tr>
									<td><span class="helpItem">Order number:</span> the number for each order.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Account name:</span> the list of the customer accounts for each order.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Product name:</span> the list of products to be loaded for each order.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Owner:</span> the owner for each order.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Created:</span> the date and time that each order was created.</td>
								</tr>
							</table>
						</td>
					</tr>
					<tr>
						<td><span class="helpItem">Customer account:</span> will limit the report based upon the selected customer account.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Owner:</span> will limit the report based upon the selected owner.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Facility:</span> will limit the report based upon the selected facility.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Report type:</span> will choose what format the report will be presented in.<br />
							<table style="font-style: italic; margin-left: 20px;">
								<tr>
									<td><span class="helpItem">Each product has individual column:</span> the report will present each order on a single line, with the products used each having 2 columns, one for the requested quantity, and the second for the remaining quantity.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Each order item has individual row:</span> the report will list each product on an order on an individual line, with a column for the requested quantity, and a second column for the remaining quantity.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Each order has single row:</span> the report will present each order on a single line, with the products for the order being listed on the one row in a single column.</td>
								</tr>
							</table>
						</td>
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
