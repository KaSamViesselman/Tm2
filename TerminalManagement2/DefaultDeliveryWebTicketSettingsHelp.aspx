<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="DefaultDeliveryWebTicketSettingsHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.DefaultDeliveryWebTicketSettingsHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head id="Head1" runat="server">
	<title>Help : Default Delivery Web Ticket Settings</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="DefaultDeliveryWebTicketSettingsHelp" runat="server">
		<div>
			<span style="font-size: large; font-weight: bold;"><a href="help.aspx#DefaultDeliveryWebTicketSettings">Help</a> : Default Delivery Web Ticket Settings</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>Default delivery web ticket settings control the behavior of Terminal Management and the applications that work with Terminal Management.</p>
				<p><span class="helpItem">Ticket owner:</span> select whether to change the settings for all owners or for a specific owner. If settings exist for an owner, those settings are used; otherwise the settings specified for "all owners" are used.</p>
				<p><span class="helpItem">Save owner web ticket settings:</span> click to save the web ticket settings for the selected owner or all owners.</p>
				<p><span class="helpItem">Delete owner web ticket settings:</span> click to delete the web ticket settings for the selected owner or all owners.</p>
				<table>
					<tr>
						<td style="width: 50%; border-right: 1px solid #404040;">
							<table>
								<tr>
									<td><span class="helpItem">NTEP compliant:</span> when enabled, the ticket displays extra information required for NTEP compliance, such as if the ticket was from a point-of-sale transaction or if products were hand-added or emulated.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Show acres:</span> when enabled, the acres field is shown on tickets.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Show application rate:</span> when enabled, the application rate is shown on tickets.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Use order's application rate:</span> when enabled, the application rate from the order is displayed for the compartment and for the products summary. Individual application rates for bulk products will not be displayed.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Show applicator:</span> when enabled, the applicator is shown on tickets.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Display blend group name as product name:</span> when enabled, will display the blend group name assigned to the order on the ticket instead of the product name.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Show bulk product summary totals:</span> when enabled, the summary of each bulk product that was loaded is displayed.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Show bulk product notes in summary totals:</span> when enabled, will display any notes assigned to the bulk product in the bulk product summary section.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Show bulk product EPA number in summary totals:</span>
									when enabled, will display the EPA number associated with the bulk product.
								</tr>
								<tr>
									<td><span class="helpItem">Show branch location:</span> when enabled, the branch location is shown on tickets.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Show carrier:</span> when enabled, the carrier is shown on tickets.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Show compartments:</span> when enabled, the compartment that each product was loaded to is displayed.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Show product notes:</span> when enabled, will display any notes assigned to the product in the compartment summary section.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Show bulk ingredients in compartment:</span> when enabled, the bulk ingredients loaded in the compartment are shown on the ticket.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Show bulk ingredient notes:</span> when enabled, will display any notes assigned to the bulk ingredient in the compartment summary section.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Show bulk ingredient EPA number:</span>
									when enabled, will display the EPA number associated with the bulk product.
								</tr>
								<tr>
									<td>
										<span class="helpItem">Show bulk ingredient lot numbers assigned:</span> when enabled, will display the specific lot number used during the dispensing process.
										<p class="indentedNote">This option will only be displayed if Item Traceability is enabled.</p>
									</td>
								</tr>
								<tr>
									<td><span class="helpItem">Show compartment totals:</span> when enabled, the total amount loaded in each compartment is shown on the ticket.&nbsp; This total will be displayed in the unit(s) of measure defined in the <b>Show additional units</b>and the <b>Show additional units for product groups</b> sections.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Show compartment loaded index:</span> when enabled, the order that the load was loaded into the compartment will be displayed as the initial item in the compartment row. If this option is not enabled, then only the compartment information supplied from a transport will be displayed.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Show customer:</span> when enabled, the customer is shown on tickets.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Show customer destination notes:</span> when enabled, the notes associated with the customer destination will be displayed.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Show customer notes:</span> when enabled, the notes associated with each customer will be displayed.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Show date and time:</span> when enabled, the date and time the load was finished loading is shown on tickets.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Show density on ticket:</span> when enabled, the density of product is shown on tickets.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Show derived from on ticket:</span> when enabled, will display the derived from source(s) for the bulk product that were assigned at the time of the ticket creation.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Show discharge location(s):</span> when enabled, the discharge location is shown on tickets.</td>
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
									<td><span class="helpItem">Show facility:</span> when enabled, the facility where the ticket was created will be shown on tickets.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Show fertilizer guaranteed analysis:</span> when enabled, the fertilizer guaranteed analysis is shown on tickets.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Show analysis by compartment for Do Not Blend orders:</span> when enabled, the fertilizer guaranteed analysis by compartment is shown on tickets.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Show fertilizer grade:</span> when enabled, the fertilizer grade is shown on tickets.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Show fertilizer grade by compartment for Do Not Blend orders:</span> when enabled, each compartment will display the fertilizer grade by compartment for Do Not Blend loads only.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Number of decimals to display if analysis is greater than or equal to 1%:</span> will define the number of decimal positions to display on the ticket for analysis percentages greater than or equal to 1%.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Number of decimals to display if analysis is less than 1%:</span> will define the number of decimal positions to display on the ticket for analysis percentages between 0% and 1%.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Analysis entries should round down:</span> will define how analysis entries are handled.  If this value is checked, then the last number displayed will not be rounded. If this value is not checked, then the last unit displayed will be rounded.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Show all decimal places on analysis:</span> will define how the decimals places should be displayed if the last numbers are 0. For example, if the number of decimal is defined to be 4 decimal, but the percentage is 0.5%, should the display be 0.5 or 0.5000?</td>
								</tr>
								<tr>
									<td><span class="helpItem">Hide N-P-K nutrients with 0% inclusion in guaranteed analysis:</span> will define if N-P-K values should be displayed in the guaranteed analysis if they have a 0 percent analysis.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Show loaded by (user):</span> when enabled, the operator that loaded the load is shown on tickets.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Show order summary:</span> when enabled, the delivered amounts for the order are shown on tickets.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Show summary at time of ticket creation:</span> when enabled, the delivered amounts for the order at the time the ticket was created are shown on tickets.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Show owner:</span> when enabled, the order owner is shown on tickets.</td>
								</tr>

								<tr>
									<td><span class="helpItem">Show product hazardous material:</span> when enabled, the hazardous material property for the product is shown on tickets. This does not necessarily enable the hazardous material analysis portion of the ticket, look to "Analysis Settings" for those options. </td>
								</tr>
								<tr>
									<td><span class="helpItem">Show product summary totals:</span> when enabled, the product summary totals are shown on tickets.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Show product notes:</span> when enabled, will display any notes assigned to the product in the product summary section.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Show purchase order number:</span> when enabled, the purchase order number for the order is shown on tickets.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Show release number:</span> when enabled, the release number for the order is shown on tickets.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Show requested quantities:</span> when enabled, the requested quantities for the order are shown on tickets.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Show rinse entries:</span> when enabled, a product(recipe) using the rinse entry will a "Rinse" line shown on the ticket.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Show ship to:</span> when enabled, the ship to location for the customer is shown on tickets.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Show total:</span> when enabled, the total amount for the product is shown on tickets.</td>
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
							</table>
						</td>
						<td style="width: 50%;">
							<table>
								<tr>
									<td><span class="helpItem">Show additional units:</span> select a unit of measure that quantities should always be displayed in (in addition to the request unit of measure). This unit is also used for displaying <b>Compartment totals</b>.<br />
										<br />
										<span style="font-style: italic; text-indent: 5em;">Note that if you are converting units from mass to volume, accurate densities must be entered in the bulk products. If any are missing the unit will not be shown.</span><br />
										<br />
										Ticket decimal precision is configured on the Units tab for the totals and the panel settings for the compartments.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Show additional units for product groups:</span> select a unit of measure that quantities should always be displayed in (in addition to the request unit of measure) if a product is assigned to a product group. This unit is also used for displaying <b>Compartment totals</b>.<br />
										<br />
										<span style="font-style: italic; text-indent: 5em;">Note that if you are converting units from mass to volume, accurate densities must be entered in the bulk products. If any are missing the unit will not be shown.</span><br />
										<br />
										Ticket decimal precision is configured on the Units tab for the totals and the panel settings for the compartments.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Density unit precision:</span> used to define what decimal precision should be shown on the ticket when densities are displayed.</td>
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
									<td><span class="helpItem">Blank 1, Blank 2, Blank 3:</span> printed at the end of every ticket with a blank line with it. Leave blank to not show the line.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Ticket add-on URL:</span> used to display an external web page on the ticket.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Show all custom fields on delivery ticket:</span> will show all available custom fields for the ticket. If unchecked, then only the custom fields selected below this will be displayed.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Show all custom pre-load questions:</span> will show all available custom pre load questions for the ticket. If unchecked, then only the custom pre-load questions selected below this will be displayed.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Show all custom post-load questions:</span> will show all available custom post-load questions for the ticket. If unchecked, then only the custom post-load questions selected below this will be displayed.</td>
								</tr>
							</table>
						</td>
					</tr>
				</table>
			</div>
			<p><span class="helpItem">Save:</span> saves the settings entered.</p>
		</div>
	</form>
</body>
</html>
