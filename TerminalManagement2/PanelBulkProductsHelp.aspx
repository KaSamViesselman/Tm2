<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="PanelBulkProductsHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.PanelBulkProductsHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server"><title>Help : Panel Bulk Products</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="PanelBulkProductsHelp" runat="server">
		<div><span style="font-size: large; font-weight: bold;"><a href="help.aspx#PanelBulkProducts">Help</a> : Panel Bulk Products</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>The Panel Bulk Products page shows a list of all of the bulk products for each panel in the system, what the bulk product's panel function is, and if the bulk product is enabled or disabled. This table also allows for enabling/disabling of bulk products, if enabling a bulk product, all other similarly named bulk products on that panel will be disabled.</p>
				<p><span class="helpItem">E-mail to:</span> the panel bulk products report may be e-mailed directly from the page. Enter the e-mail addresses for the recipients (multiple addresses may be separated by commas) and click the "Send" button.</p>
				<p><span class="helpItem">Add address:</span> will look in the e-mail history, and items to find possible e-mail addresses that the report may be sent to.</p>
			</div>
		</div>
	</form>
</body>
</html>
