<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="ProductListHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.ProductListHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server"><title>Help : Product List</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="ProductListHelp" runat="server">
		<div><span style="font-size: large; font-weight: bold;"><a href="help.aspx#ProductList">Help</a>: Product List</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>The product list shows all the product records and the bulk products used to make those products at each facility.</p>
				<table>
					<tr>
						<td><span class="helpItem">Facility:</span> by default all facilities are shown. To filter the report to only show a specific facility, select the facility in the drop-down list.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Printer friendly version:</span> opens the report in a new window without the Terminal Management header and side bar.</td>
					</tr>
				</table>
				<p><span class="helpItem">E-mail to:</span> the product list report may be e-mailed directly from the page. Enter the e-mail addresses for the recipients (multiple addresses may be separated by commas) and click the "Send" button.</p>
				<p><span class="helpItem">Add address:</span> will look in the e-mail history, and items to find possible e-mail addresses that the report may be sent to.</p>
			</div>
		</div>
	</form>
</body>
</html>
