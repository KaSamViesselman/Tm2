<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="DefaultOrderSummarySettingsHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.DefaultOrderSummarySettingsHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head id="Head1" runat="server"><title>Help : Default Order Summary Settings</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="DefaultOrderSummarySettingsHelp" runat="server">
		<div><span style="font-size: large; font-weight: bold;"><a href="help.aspx#DefaultOrderSummarySettings">Help</a> : Default Order Summary Settings</span> <hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>Default order summary settings control the behavior of Terminal Management and the applications that work with Terminal Management.</p>
				<p><span class="helpItem">Order owner:</span> select whether to change the settings for all owners or for a specific owner. If settings exist for an owner those settings are used, otherwise the settings specified for "all owners" are used.</p>
				<p><span class="helpItem">Save owner order summary settings:</span> click to save the order summary settings for the selected owner or all owners.</p>
				<p><span class="helpItem">Delete owner order summary settings:</span> click to delete the order summary settings for the selected owner or all owners.</p>
				<table>
					<tr>
						<td><span class="helpItem">Show acres:</span> when enabled, the acres field is shown from the order.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Show branch location:</span> when enabled, the branch location is shown from the order.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Show customer account number:</span> when enabled, the customer&#39;s account number will be displayed when the order is shown.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Show e-mail address:</span> when enabled, the e-mail addresses from the order owner, customer, etc., are shown.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Show interface:</span> when enabled, the interface assigned to the order is shown.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Show notes:</span> when enabled, the ticket notes assigned to the order are shown.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Show ship to:</span> when enabled, the ship to information assigned to the order is shown.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Show PO number:</span> when enabled, the PO Number assigned to the order is shown.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Show release number:</span> when enabled, the Release Number assigned to the order is shown.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Use ticket delivered amounts for delivered quantity:</span> will calculate the delivered quantity by looking at every ticket created from the order, instead of using the delivered value stored at the order item level.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Show additional units:</span> select a unit of measure that quantities should always be displayed in (in addition to the request unit of measure).<br />
							<br />
							<span style="font-style: italic; text-indent: 5em;">Note that if you are converting units from mass to volume, accurate densities must be entered in the bulk products. If any are missing the unit will not be shown.</span> <br />
							<br />
							Ticket decimal precision is configured on the Units tab for the totals and the panel settings for the compartments.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Show all custom fields on order summary:</span> will show all available custom fields for the order. If unchecked, then only the custom fields selected below this will be displayed.</td>
					</tr>
				</table>
			</div>
			<p><span class="helpItem">Save:</span> saves the settings entered.</p>
		</div>
	</form>
</body>
</html>
