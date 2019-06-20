<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="DefaultReceivingWebTicketSettingsHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.DefaultReceivingWebTicketSettingsHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head id="Head1" runat="server">
	<title>Help : Default Receiving Web Ticket Settings</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="DefaultReceivingWebTicketSettingsHelp" runat="server">
		<div>
			<span style="font-size: large; font-weight: bold;"><a href="help.aspx#DefaultReceivingWebTicketSettings">Help</a> : Default Receiving Web Ticket Settings</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>Default receiving web ticket settings control the behavior of Terminal Management and the applications that work with Terminal Management.</p>
				<p><span class="helpItem">Ticket owner:</span> select whether to change the settings for all owners or for a specific owner. If settings exist for an owner those settings are used, otherwise the settings specified for "all owners" are used.</p>
				<p><span class="helpItem">Save owner web ticket settings:</span> click to save the web ticket settings for the selected owner or all owners.</p>
				<p><span class="helpItem">Delete owner web ticket settings:</span> click to delete the web ticket settings for the selected owner or all owners.</p>
				<table>
					<tr>
						<td style="width: 50%;">
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
									<td>
										<span class="helpItem">Show lot number:</span> when enabled, will display the lot number assigned to the receiving ticket. 
										<p class="indentedNote">This option will only be displayed if Item Traceability is enabled.</p>
									</td>
								</tr>
								<tr>
									<td><span class="helpItem">Show owner:</span> when enabled, the order owner is shown on tickets.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Show transport:</span> when enabled, the transport information is shown on tickets.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Show transport tare information:</span> will show tare, net, and gross weight information about the transport.</td>
								</tr>
								<tr>
									<td><span class="helpItem">First position, second position, third position:</span> designates what weight information should be shown in which order for the transport.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Show additional units:</span> select a unit of measure that quantities should always be displayed in (in addition to the request unit of measure). 
										<p class="indentedNote">Note that if you are converting units from mass to volume, accurate densities must be entered in the bulk products. If any are missing the unit will not be shown.</p>

										Ticket decimal precision is configured on the panel settings.</td>
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
								<tr>
									<td><span class="helpItem">Show all custom fields on receiving ticket:</span> will show all available custom fields for the ticket. If unchecked, then only the custom fields selected below this will be displayed.</td>
								</tr>
							</table>
						</td>
						<td style="width: 50%;">&nbsp;
						</td>
					</tr>
				</table>
			</div>
			<p><span class="helpItem">Save:</span> saves the settings entered.</p>
		</div>
	</form>
</body>
</html>
