<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="DischargeLocationsHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.DischargeLocationsHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Help : Discharge Locations</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="DischargeLocationsHelp" runat="server">
		<div>
			<span style="font-size: large; font-weight: bold;"><a href="help.aspx#DischargeLocations">Help</a> : Discharge Locations</span>
			<hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>Discharge location records represent locations where product can be dispensed to. Panel names may appear in the list of discharge locations since there may be panels that can act as a vessel that receives product. Discharge locations may also cascade (i.e. one discharge location/panel leads to another discharge location).</p>
				<p><span class="helpItem">Facility drop-down list:</span> contains all the facilities set up in the system. If a particular facility is selected, then only the discharge locations that are panels assigned to that facility, or if not a panel, are assigned to a bay that is assigned to that facility will be displayed.</p>
				<p><span class="helpItem">Discharge location drop-down list:</span> contains a list of all the discharge location records. To modify an existing discharge location record, select it in the drop-down list. To enter a new discharge location record, select "Enter new discharge location".</p>
				<table>
					<tr>
						<td style="width: 50%; border-right: 1px solid #404040;">
							<h2>General</h2>
							<table>
								<tr>
									<td><span class="helpItem">Name:</span> of the discharge location. The name is required and may be up to 50 characters in length.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Fill limit, Secondary fill limit:</span> is the maximum quantity of product(s) that may be delivered or routed through this discharge location. If the quantity to be dispensed to or through this discharge location is larger than the fill limit, the quantity is split into batches. If the fill limit is 0, it is ignored.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Bay:</span> the name of the bay that this discharge location is in. Used by dispensing software, which may be assigned bays, to determine if the discharge location is an option for load-out. The bay is optional.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Accepts blends:</span> determines if the product dispensed to or through this discharge location may be blended. If not selected only single product is allowed.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Confirm empty:</span> determines if the user must confirm that the discharge location is empty before dispensing another load to the discharge location. This is typically used with overhead holding tanks or blend storage tanks.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Last ticket:</span> the number of the last ticket that was loaded to the discharge location. If the discharge location is configured to "confirm empty" the last ticket is used to signal that the discharge location is still full. If the last ticket is cleared by clicking the "Clear" button, that signals the system that the location is empty and ready for a new load.</td>
								</tr>
							</table>
						</td>
						<td>
							<h2>Panels</h2>
							The panel settings determine which panels are able to dispense to the discharge location. To add a panel select it in the panel drop-down list and click the "Add panel" button. To remove an existing panel, select the panel in the list below the panel drop-down list and click the "Remove panel" button. 
                             <table>
								 <tr>
									 <td><span class="helpItem">Diverters:</span> control which outputs (valves, gates, etc.) must be on to route product from the panel to the discharge location. Diverters are associated with the panel currently selected in the panel list below the panel drop-down list.</td>
								 </tr>
								 <tr>
									 <td><span class="helpItem">Purge time multiplier:</span> used to adjust the purge time to match the length to the discharge locations. For example, if a one discharge location is further away than another, a multiplier of 2 might be entered to double the purge time for that discharge location.</td>
								 </tr>
								 <tr>
									 <td><span class="helpItem">Anticipation multiplier:</span> used to adjust the anticipation to match the length to the discharge locations. For example, if a one discharge location is further away than another, a multiplier of 2 might be entered to double the anticipation for that discharge location. This setting is different that the anticipation multiplier used on KA-2000 dry systems (FDS-2850, FDS-2851, FDS-2866, for example).</td>
								 </tr>
								 <tr>
									 <td><span class="helpItem">Automatically discharge panel:</span> when enabled on a system that requires a discharge to complete the dispensing cycle, will allow the system to begin discharging for the compartment once the "Start compartment" button is pressed in the dispensing application, and will continue to discharge automatically for every batch within that compartment. The operator will have the ability to pause the discharge process at any time during the discharging of the system. If not enabled on a system that requires a discharge to complete the dispensing cycle, the operator will be required to initiate each discharge cycle by pressing the "Dump" button for each batch in the compartment.</td>
								 </tr>
								 <tr>
									 <td><span class="helpItem">Save panel:</span> click the "Save panel" button to save the diverters and purge time multiplier for the panel selected in the list below the panel drop-down list. This should be done before saving the discharge location.</td>
								 </tr>
							 </table>
							<h2>Storage locations</h2>
							The storage location settings determine if a item traceability movement should be created into a storage location when a bulk product is dispensed. To add a storage location, select it in the storage location drop-down list and click the "Add" button. To remove an existing storage location, select the storage location in the list below the storage location drop-down list and click the "Remove" button. 
							<p class="indentedNote">This option will only be displayed if Item Traceability is enabled.</p>
						</td>
					</tr>
				</table>
			</div>
			<p><span class="helpItem">Save button:</span> located at the top of the page, click this button to save any changes made to a discharge location record or create a new discharge location record.</p>
			<p><span class="helpItem">Delete button:</span> located at the top of the page, click this button to delete the current discharge location record.</p>
		</div>
	</form>
</body>
</html>
