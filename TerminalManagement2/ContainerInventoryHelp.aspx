<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="ContainerInventoryHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.ContainerInventoryHelp" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Help : Container Inventory</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="ContainerInventoryHelp" runat="server">
		<div>
			<span style="font-size: large; font-weight: bold;"><a href="help.aspx#ContainerInventory">Help</a> : Container Inventory</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>
					The container inventory page shows the inventory levels for bulk products per owner per facility per bulk product that are being stored in a container.
				</p>
				<p>
					<span class="helpItem">Owner:</span> select the owner to filter the inventory table to only show inventory levels for that owner.
				</p>
				<p>
					<span class="helpItem">Facility:</span> select the facility to filter the inventory table to only show inventory levels at that facility.
				</p>
				<p>
					<span class="helpItem">Bulk product:</span> select the bulk product to filter the inventory table to only show inventory levels for that bulk product.
				<br />
					<span style="margin-left: 2em;">Inventory groups will have a designation of (Grouped inventory) after their name.</span>
				</p>
				<p>
					<span class="helpItem">Current status:</span> select the current status to filter the inventory table to only show inventory levels for containers that are assigned to a particular status. 
				</p>
				<p>
					<span class="helpItem">Only show non-zero inventories:</span> will only show the bulk products that currently have a zero inventory, and have no orders assigned to them, and have not had any history run against them.
				</p>
				<p>
					<span class="helpItem">Show additional units:</span> select a unit of measure that quantities should always be displayed in (in addition to the request unit of measure).
				<br />
					<br />
					<span style="font-style: italic; text-indent: 5em;">Note that if you are converting units from mass to volume, accurate densities must be entered in the bulk products. If any are missing the unit will not be shown.</span>
					<br />
					<br />
					Decimal precision is configured on the Units tab for the totals and the panel settings for the compartments.
				</p>
				<p>
					<span class="helpItem">Container inventory table</span>
				</p>
				<table>
					<tr>
						<td>
							<span class="helpItem">Bulk product:</span> the name of the bulk product.
						<br />
							<span style="margin-left: 2em;">Inventory groups will have a link to drill down into the bulk products that make up this inventory. There will also be a tool tip available that will list the bulk products assigned to this group.</span>
						</td>
					</tr>
					<tr>
						<td>
							<span class="helpItem">Owner:</span> the owner of the inventory.
						</td>
					</tr>
					<tr>
						<td>
							<span class="helpItem">Facility:</span> the facility where the inventory is located.
						</td>
					</tr>
					<tr>
						<td>
							<span class="helpItem">Status:</span> the status of the containers where the inventory is located.
						</td>
					</tr>
					<tr>
						<td>
							<span class="helpItem">Inventory:</span> is the sum of the amounts of the bulk products assigned to containers.
						</td>
					</tr>
				</table>
				<p>
					<span class="helpItem">E-mail to:</span> the current container inventory report may be e-mailed directly from the page. Enter the e-mail addresses for the recipients (multiple addresses may be separated by commas) and click the "Send" button.
				</p>
				<p>
					<span class="helpItem">Add address:</span> will look in the e-mail history, and items to find possible e-mail addresses that the report may be sent to.
				</p>
			</div>
		</div>
	</form>
</body>
</html>
