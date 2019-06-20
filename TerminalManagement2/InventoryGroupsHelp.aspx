<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="InventoryGroupsHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.InventoryGroupsHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head id="Head1" runat="server">
	<title>Help : Inventory Groups</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="InventoryHelp" runat="server">
		<div>
			<span style="font-size: large; font-weight: bold;"><a href="help.aspx#InventoryGroups">Help</a> : Inventory Groups</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>Inventory groups are used to group bulk product inventory records together for the inventory report.</p>
				<p><span class="helpItem">Inventory group drop-down list:</span> to modify an existing inventory group, select the inventory group from the drop-down list. To enter a new inventory group, select "Enter new inventory group" from the drop-down list.</p>
				<table>
					<tr>
						<td style="width: 50%; border-right: 1px solid #404040;">
							<table>
								<tr>
									<td><span class="helpItem">Name:</span> the name of the inventory group is required and may be up to 50 characters in length.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Bulk products:</span> the bulk products that this inventory group will include in it's inventory. A bulk product may be assigned to more than one inventory group at a time.<br />
										<span style="margin-left: 2em;">To add a bulk product select it from the drop-down list and click the "Add bulk product" button.</span><br />
										<span style="margin-left: 2em;">To remove a bulk product select the product from the list (below the drop-down list) and click the "Remove bulk product" button.</span></td>
								</tr>
							</table>
						</td>
						<td style="width: 50%;">
							<table>
								<tr>
									<td><span class="helpItem">Is totalized group:</span> defines if this inventory group shows up as a totalized group on the inventory report. If not selected, then this group is used to allow the inventory and inventory change report to display the multiple bulk products assigned to the group.</td>
								</tr>
							</table>
						</td>
					</tr>
				</table>
			</div>
			<p><span class="helpItem">Save:</span> saves the changes made to an existing inventory group record or create a new inventory group when "Enter new inventory group" is selected in the user drop-down list.</p>
			<p><span class="helpItem">Delete:</span> deletes the selected inventory group.</p>
		</div>
	</form>
</body>
</html>
