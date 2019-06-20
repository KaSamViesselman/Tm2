<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="InventoryChangeReportHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.InventoryChangeReportHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server"><title>Help : Inventory Change Report</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="InventoryChangeReportHelp" runat="server">
		<div><span style="font-size: large; font-weight: bold;"><a href="help.aspx#InventoryChangeReport">Help</a> : Inventory Change Report</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>The inventory change report shows all changes/adjustments made to the inventory records.</p>
				<p><span class="helpItem">Owner:</span> select an owner to show the inventory change records to those for the selected owner only.</p>
				<p><span class="helpItem">Facility:</span> select a facility to show the inventory change records to those at the selected facility only.</p>
				<p><span class="helpItem">Bulk product:</span> select a bulk product to show the inventory change records to those for the selected bulk product only.</p>
				<p><span class="helpItem">Show additional units:</span> select a unit of measure that quantities should always be displayed in (in addition to the request unit of measure).<br />
					<br />
					<span style="font-style: italic; text-indent: 5em;">Note that if you are converting units from mass to volume, accurate densities must be entered in the bulk products. If any are missing the unit will not be shown.</span><br />
					<br />
					Decimal precision is configured on the Units tab for the totals and the panel settings for the compartments.</p>
				<p><span class="helpItem">Inventory change report table</span></p>
				<table>
					<tr>
						<td><span class="helpItem">Date/time:</span> the date and time that the inventory change took place.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Bulk product:</span> the bulk product affected by the inventory change.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Owner:</span> the owner of the bulk product affected by the inventory change.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Facility:</span> the facility where the inventory change took place.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Change:</span> the quantity that the inventory changed.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Inventory:</span> the resulting inventory level after the change.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Dispensed:</span> the resulting inventory level after the change.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Notes:</span> notes associated with the inventory change.</td>
					</tr>
				</table>
				<p><span class="helpItem">E-mail to:</span> the inventory change report may be e-mailed directly from the page. Enter the e-mail addresses for the recipients (multiple addresses may be separated by commas) and click the "Send" button.</p>
				<p><span class="helpItem">Add address:</span> will look in the e-mail history, and items to find possible e-mail addresses that the report may be sent to.</p>
			</div>
		</div>
	</form>
</body>
</html>
