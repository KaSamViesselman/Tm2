<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="PanelBulkProductSettingsHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.PanelBulkProductSettingsHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head id="Head1" runat="server">
	<title>Help : Panel Bulk Product Settings</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="PanelsHelp" runat="server">
		<div>
			<span style="font-size: large; font-weight: bold;"><a href="help.aspx#PanelBulkProductSettings">Help</a> : Panel Bulk Product Settings</span>
			<hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p><span class="helpItem">Facility drop-down list:</span> contains all the facilities set up in the system. If a particular facility is selected, then only the panels that are assigned to that facility will be displayed.</p>
				<p><span class="helpItem">Panel drop-down list:</span> contains a list of all the panel records. To modify the bulk product settings assigned to an existing panel record, select it in the drop-down list.</p>
				<table>
					<tr>
						<td style="width: 50%; border-right: 1px solid #404040;">
							<table>
								<tr>
									<td><span class="helpItem">Bulk products:</span> the bulk products that this panel is able to dispense. To add a bulk product select it from the drop-down list and click the "Add bulk product" button. Multiple of the same bulk product can be added to a panel, but only one similarly named bulk product can be enabled at a time. To remove a bulk product select the product from the list (below the drop-down list) and click the "Remove bulk product" button.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Panel function:</span> the panel function that should be used for the selected bulk product:<ul>
										<li>Product: dispense a product where the number corresponds to an output, typically on an RB panel.</li>
										<li>Measured discharge: discharge to a specific set point. Pause the system before discharging.</li>
										<li>Timed mix: mix for a specified number of seconds.</li>
										<li>Discharge: discharge all product in a hopper, tank or mixer. Pause the system before discharging.</li>
										<li>Timed purge: purge for a specified number of seconds.</li>
										<li>Automatic discharge: discharge all product in a hopper, tank or mixer. Start discharging immediately.</li>
										<li>Pause: pause the system. Wait for user to restart.</li>
										<li>Start recirculation: start recirculating product. Typically the system continues through the following functions while recirculating.</li>
										<li>Stop recirculation: stop recirculating product. Typically used after a start recirculation function has been used in a load.</li>
										<li>Start agitator: start the agitator in a liquid blender system. Typically the system continues through the following functions while agitating.</li>
										<li>Stop agitator: stop the agitator in a liquid blender system. Typically used after a start agitator function has been used in a load.</li>
										<li>Hand-add: pause the system and allow the user to manually add product. If used on a scale system the weight of the product added to the scale will be captured.</li>
										<li>Timed agitate: agitate product in a liquid blender for a specified number of seconds.</li>
										<li>Rinse: this is a rinse function within the KA-2000 controller that is typically use as a pre-rinse or wetting operation. The dispensing application will be used to specify when this function is used. This function is different than the rinse that occurs during a discharge cycle on a fill/discharge system.</li>
									</ul>
									</td>
								</tr>
							</table>
						</td>
						<td style="width: 50%;">
							<h2>Panel settings for bulk product</h2>
							<table>
								<tr>
									<td><span class="helpItem">Start parameter:</span> on dry/granular systems this is typically the number of times to "bump" a gate to loosen product to improve flow. On liquid systems this is typically a flood time (in seconds) to allow product to flood into the pump before starting the pump.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Finishing parameter:</span> typically this is the number of seconds to purge the line after a product dispense completes.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Anticipation:</span> specifies when the panel should stop dispensing so that it does not dispense more than was requested. If the panel overshoots the requested quantity the anticipation should be increased. If the panel undershoots the requested quantity the anticipation should be decreased.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Anticipation update factor:</span> used to automatically correct anticipation when the panel over or undershoots the request quantity. 0 disables to automatic update, 1 uses the full difference between the requested and delivered amount to update the anticipation. A value of 0.5 (half the difference) is typically recommended to prevent the system from over-correcting.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Conversion factor:</span> used for panels with pulse based meters to convert the pulses received by the meter into a real world quantity.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Update density using meter:</span> on mass-flow meters the system may use the density read by the meter to update the bulk product's density.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Use average density for ticket:</span> used during ticket creation. If this is enabled, and the meter can report the average density of the product being dispensed, then this amount will be stored with the ticket. If it is not enabled, or it=f the meter doesn't report the average density of the delivered product, then the amount assigned to the ticket will be the density assigned to the bulk product at the time of ticket creation.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Dump time:</span> used on some dry tower systems (FDS-2866, for example) to specify how long it takes to discharge product from a vessel that has no measuring system.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Disabled:</span> specifies if the product settings should be used. If a setting is disabled, then it will not be used to dispense a bulk product. Only 1 bulk product setting per panel can be enabled at a time. This option is useful for products that are constantly changing locations, where the anticipation numbers may be different based upon their different source locations.</td>
								</tr>
							</table>
							<h2>Storage locations</h2>
							The storage location settings determine records should be created for a storage location when a bulk product is dispensed. To add a storage location, select it in the storage location drop-down list and click the "Add" button. To remove an existing storage location, select the storage location in the list below the storage location drop-down list and click the "Remove" button.
							<p class="indentedNote">This option will only be displayed if Item Traceability is enabled.</p> 
						</td>
					</tr>
				</table>
			</div>
			<p><span class="helpItem">Save button:</span> located at the top of the page, click this to save bulk product/panel settings. This should be done before saving the panel record. A dialog will appear if bulk product is saved as enabled and there is an existing bulk product with the same name on the panel that is enabled, stating previously enabled bulk product will be disabled.</p>
		</div>
	</form>
</body>
</html>
