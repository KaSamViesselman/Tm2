<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="ProductGroupsHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.ProductGroupsHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server"><title>Help : Product Groups</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="ProductGroupsHelp" runat="server">
		<div><span style="font-size: large; font-weight: bold;"><a href="help.aspx#ProductGroups">Help</a> : Product Groups</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>Product Group records represent filtering/display options available to each product.</p>
				<p><span class="helpItem">Product Group drop-down list:</span> to edit an existing product group, select it from the drop-down list. To create a new product group, select "Enter a new product group" from the drop-down list.</p>
				<table>
					<tr>
						<td><span class="helpItem">Name:</span> the name of the product group is required and may be up to 100 characters in length.</td>
					</tr>
				</table>
			</div>
			<p><span class="helpItem">Save:</span> saves the changes to a product group record when an existing product group is selected. Creates a new product group record when "Enter a new product group" is selected in the product group drop-down list.</p>
			<p><span class="helpItem">Delete:</span> deletes the selected product group record.</p>
		</div>
	</form>
</body>
</html>
