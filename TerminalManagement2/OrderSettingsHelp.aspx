<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="OrderSettingsHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.OrderSettingsHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head id="Head1" runat="server">
	<title>Help : Order Settings</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="OrderSettingsHelp" runat="server">
		<div>
			<span style="font-size: large; font-weight: bold;"><a href="help.aspx#OrderSettings">Help</a> : Order Settings</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>Order settings control the behavior of Terminal Management and the applications that work with Terminal Management.</p>
				<table>
					<tr>
						<td style="width: 50%; border-right: 1px solid #646464;">
							<p><span class="helpItem">Order settings</span></p>
							<table>
								<tr>
									<td><span class="helpItem">Automatically generate sequential order numbers:</span> when enabled, an order number will automatically be generated sequentially when creating a new order record.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Allow order numbers to be modified:</span> when enabled, the user may modify the order number of an order record in the orders page.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Use separate order numbers per owner:</span> when enabled, each owner has their own sequence of order numbers. If not enabled, all owners pull from the same sequence of numbers, the next number specified in the "starting order number" field.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Create new destination from order ship to information:</span> when enabled, user can input a destination in Orders page, and when user selects "Save order" the inputed destination will be associated with the customer account assigned to the largest percentage billed.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Next order number:</span> the next number to use for an order when the automatically generate sequential order numbers option is selected.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Mark orders as complete instead of deleting when an order is deleted:</span> when enabled, orders are marked as complete (no longer loadable) instead of deleting the record.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Lock owner drop down list:</span> when enabled, the owner drop-down list on the orders page is locked to the preset owner selection (set under preset order settings).</td>
								</tr>
								<tr>
									<td><span class="helpItem">Lock branches drop down list:</span> when enabled, the branch drop-down list on the orders page is locked to the preset branch selection (set under preset order settings).</td>
								</tr>
								<tr>
									<td><span class="helpItem">Lock over scaling field:</span> when enabled, the over scaling field on the orders page is locked to the preset over scaling selection (set under preset order settings).</td>
								</tr>
								<tr>
									<td><span class="helpItem">Order comparison tolerance:</span> how close (comparing request quantities) two or more orders must be to be considered compatible to dispense together in a single load.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Show release number in order list:</span> should the release number field be shown in parenthesis in the order list.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Internal transfer customer account:</span> loads or point of sales for orders that have only this account specified will not generate tickets or update inventory.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Send order summaries to:</span> will designate which type of entity e-mail addresses should be looked at to send order summaries to after an order is completed.</td>
								</tr>
							</table>
						</td>
						<td style="width: 50%;">
							<p><span class="helpItem">Preset order settings</span></p>
							<table>
								<tr>
									<td><span class="helpItem">Owner:</span> the owner that should automatically be selected when entering a new order.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Branch:</span> the branch that should automatically be selected when entering a new order.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Over scaling:</span> the over scaling percent that should automatically be entered for a new order.</td>
								</tr>
							</table>
							<p><span class="helpItem">Point of Sale and Staged Order Settings</span></p>
							<table>
								<tr>
									<td><span class="helpItem">Use order percentage for new staged orders:</span> when enabled, the default for "Use order percentage" on the staged orders page will be set.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Allow order to be assigned to multiple staged orders:</span> when not selected, an order may only be staged once, preventing another transport from trying to load against the same order. </td>
								</tr>
								<tr>
									<td><span class="helpItem">Limit Drivers To Drivers Assigned To Account:</span> when enabled, only those drivers that are marked as available for all customer accounts or have been designated as being allowed to haul for an account will be listed. If not enabled, then all drivers will be listed.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Limit Transports To Carrier Selected:</span> when enabled, the list of available transports will be limited to those transports that are assigned to the selected carrier. If the carrier is not selected, it will list all transports. If this option is not enabled, then all transports will be available.</td>
								</tr>
								<tr>
									<td><span class="helpItem">E-mail created Point of Sale tickets:</span> will determine if tickets created by a Point of Sale will be e-mailed to any entities (e.g. owner, customer, etc.) that are configured with an e-mail address.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Staged order shortcuts:</span> determines which standard load shortcuts will be displayed on the staged order page.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Staged order custom shortcuts:</span> determines which custom load shortcuts will be displayed on the staged order page.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Point of sale custom load questions</span><ul>
										<li><span class="helpItem">Pre load questions:</span> determines which pre-load questions should be displayed on the Point of Sale screen.</li>
										<li><span class="helpItem">Post load questions:</span> determines which post-load questions should be displayed on the Point of Sale screen.</li>
									</ul>
									</td>
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
