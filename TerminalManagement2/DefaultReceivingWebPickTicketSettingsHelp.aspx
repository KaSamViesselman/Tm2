<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="DefaultReceivingWebPickTicketSettingsHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.DefaultReceivingWebPickTicketSettingsHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server"><title>Help : Default Receiving Web Pick Ticket Settings</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="DefaultReceivingWebTicketSettingsHelp" runat="server">
		<div><span style="font-size: large; font-weight: bold;"><a href="help.aspx#DefaultReceivingWebPickTicketSettings">Help</a> : Default Receiving Web Ticket Settings</span> <hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>Default receiving web ticket settings control the behavior of Terminal Management and the applications that work with Terminal Management.</p>
				<p><span class="helpItem">Ticket owner:</span> select whether to change the settings for all owners or for a specific owner. If settings exist for an owner those settings are used, otherwise the settings specified for "all owners" are used.</p>
				<p><span class="helpItem">Save owner web ticket settings:</span> click to save the web ticket settings for the selected owner or all owners.</p>
				<p><span class="helpItem">Delete owner web ticket settings:</span> click to delete the web ticket settings for the selected owner or all owners.</p>
				<table>
					<tr>
						<td><span class="helpItem">Show supplier:</span> when enabled, the supplier is shown on tickets.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Show carrier:</span> when enabled, the carrier is shown on tickets.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Show date and time:</span> when enabled, the date and time the load was finished loading is shown on tickets.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Show density on ticket:</span> when enabled, the density of product is shown on tickets.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Show driver:</span> when enabled, the driver is shown on tickets.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Show driver number:</span> when enabled, the driver number is shown on tickets.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Show e-mail address:</span> when enabled, the e-mail addresses are shown on tickets.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Show facility:</span> will show the facility where the product will be received at.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Show gross weight:</span> will show the inbound gross weight from the in progress record.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Show owner:</span> when enabled, the order owner is shown on tickets.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Show transport:</span> when enabled, the transport information is shown on tickets.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Show additional units:</span> select a unit of measure that quantities should always be displayed in (in addition to the request unit of measure).<br />
							<br />
							<span style="font-style: italic; text-indent: 5em;">Note that if you are converting units from mass to volume, accurate densities must be entered in the bulk products. If any are missing the unit will not be shown.</span> <br />
							<br />
							Ticket decimal precision is configured on the Units tab for the totals and the panel settings for the compartments.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Density unit precision:</span> is used to define what decimal precision should be shown on the ticket when densities are displayed.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Ticket logo:</span> relative path to the image to use for the logo.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Owner message:</span> will be printed under the Sold By information on the ticket.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Disclaimer:</span> printed at the end of every ticket.</td>
					</tr>
				</table>
			</div>
			<p><span class="helpItem">Save:</span> saves the settings entered.</p>
		</div>
	</form>
</body>
</html>
