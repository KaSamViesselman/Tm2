<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="InventoryHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.InventoryHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Help : Inventory</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="InventoryHelp" runat="server">
		<div>
			<span style="font-size: large; font-weight: bold;"><a href="help.aspx#Inventory">Help</a> : Inventory</span>
			<hr />
			<div id="divHelp">
				<p>
					The inventory page shows the inventory levels for bulk products per owner per facility per bulk product.
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
					<span class="helpItem">Only show non-zero inventories:</span> will only show the bulk products that currently have a zero inventory, and have no orders assigned to them, and have not had any history run against them.
				</p>
				<p>
					<span class="helpItem">Assign physical inventory to owner assigned to tank:</span> will assign the physical inventory of the bulk product to the owner assigned to the Tank. If a tank is assigned to all owners, then a new row will be created called "All owners" that will display this information.
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
					<span class="helpItem">Inventory table</span>
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
							<br />
							<span style="margin-left: 2em;">A facility will have a link to drill down into the containers report if the facility is the facility that is designated as the container packaged virtual facility.  Clicking on this link will open up a <span style="font-style: italic;">Container inventory</span> report</span>

						</td>
					</tr>
					<tr>
						<td>
							<span class="helpItem">Inventory:</span> the accumulated inventory. This number is based on meter/scale readings. Inventory is added by a receiving system with a meter or scale. Inventory is subtracted if this bulk product is dispensed at this facility for this owner by a load-out system with a meter or scale. Inventory may also be manually adjusted at the bottom of the inventory page.
						</td>
					</tr>
					<tr>
						<td>
							<span class="helpItem">Physical:</span> the physical inventory reading. This is the product level read by a tank level monitoring (TLM) system (if available).
						</td>
					</tr>
					<tr>
						<td>
							<span class="helpItem">Dispensed:</span> the accumulated dispensed quantity. This number is based on meter/scale readings from a load-out system. Any time this bulk product is dispensed at this facility for this owner, the amount that is dispensed will be added to this total.
						</td>
					</tr>
					<tr>
						<td>
							<span class="helpItem">Allocated:</span> the total quantity of this bulk product for this owner at this facility that has been requested in orders that has not yet been delivered.
						</td>
					</tr>
					<tr>
						<td>
							<span class="helpItem">Difference:</span> the difference between the inventory value and the allocated quantity.
						</td>
					</tr>
				</table>
				<p>
					<span class="helpItem">Adjust inventory</span>
				</p>
				<p>
					<span style="font-style: italic;">An owner, facility and bulk product must be selected to adjust inventory.</span>
				</p>
				<table>
					<tr>
						<td>
							<span class="helpItem">Adjustment:</span> the amount to adjust the inventory by. Positive numbers will add to inventory, negative numbers will subtract from inventory.
						</td>
					</tr>
					<tr>
						<td>
							<span class="helpItem">Notes:</span> notes to record with the inventory adjustment (i.e. PO number when receiving product, or a description of why the inventory is being adjusted).
						</td>
					</tr>
					<tr>
						<td>
							<span class="helpItem">Apply:</span> apply the adjustment. Once clicked the inventory will be modified and an inventory change record is created recording the adjustment details.
						</td>
					</tr>
					<tr>
						<td>
							<span class="helpItem">Cancel:</span> cancel without applying the adjustment.
						</td>
					</tr>
				</table>
				<p>
					<span class="helpItem">Reset dispensed quantity</span>
				</p>
				<p>
					<span style="font-style: italic;">An owner, facility and bulk product must be selected to reset the dispensed quantity.</span>
					<br />
					When clicked the dispensed quantity is reset to 0 and an inventory change record is created recording the reset.
				</p>
				<p>
					<span class="helpItem">Change unit of measure</span>
				</p>
				<p>
					<span style="font-style: italic;">An owner, facility and bulk product must be selected to change the unit of measure of an inventory record.</span>
				</p>
				<table>
					<tr>
						<td>
							<span class="helpItem">Unit:</span> select the unit of measure to change to.
						</td>
					</tr>
					<tr>
						<td>
							<span class="helpItem">Convert current quantities:</span> if checked, Terminal Management will try to convert the existing quantity from the current unit of measure to the new unit of measure. If not checked the unit is changed, but no conversion is performed (i.e. the quantity stays the same).
						</td>
					</tr>
					<tr>
						<td>
							<span class="helpItem">Apply:</span> apply the unit of measure change.
						</td>
					</tr>
					<tr>
						<td>
							<span class="helpItem">Cancel:</span> cancel the unit of measure change.
						</td>
					</tr>
				</table>
				<p>
					<span class="helpItem">E-mail to:</span> the current inventory report may be e-mailed directly from the page. Enter the e-mail addresses for the recipients (multiple addresses may be separated by commas) and click the "Send" button.
				</p>
				<p>
					<span class="helpItem">Add address:</span> will look in the e-mail history, and items to find possible e-mail addresses that the report may be sent to.
				</p>
			</div>
		</div>
	</form>
</body>
</html>
