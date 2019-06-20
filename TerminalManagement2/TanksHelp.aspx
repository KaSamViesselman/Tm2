<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="TanksHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.TanksHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server"><title>Help : Tanks</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="form1" runat="server">
		<div><span style="font-size: large; font-weight: bold;"><a href="help.aspx#Tanks">Help</a>: Tanks</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>Tank records represent tanks containing bulk product, measured by a tank level monitor (TLM) panel.</p>
				<p><span class="helpItem">Tank drop-down list:</span> located at the top of the page, this list contains all the container equipment records. To edit an existing tank record, select its name in the drop-down list. To create a new tank record, select "Enter new facility".</p>
				<table>
					<tr>
						<td style="width: 50%; border-right: 1px solid #404040;">
							<table>
								<tr>
									<td><span class="helpItem">Name:</span> the name of the tank. The name is required and may include up to 50 characters.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Owner:</span> the owner of the tank. Owner is optional. If selected only users assigned to that owner or with access to all owners will be able to access the tank.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Facility:</span> the facility where the tank is located. The facility is optional.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Bulk product:</span> the bulk product that is stored in the tank. The bulk product is optional.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Unit:</span> the unit of measure that the bulk product in the tank is measured with.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Panel:</span> the panel that is used to measure the tank level. The panel is optional, but the tank level will only update when a tank is specified.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Sensor:</span> the sensor input on the panel that the level sensor is connected to.</td>
								</tr>
								<tr>
									<td><span class="helpItem">E-mail alerts:</span> e-mail address(es) that should be notified on tank alert conditions (e.g. high-high, high, low, low-low, flow rate, sensor). E-mail alerts is optional and multiple e-mail addresses may be entered separated by a comma.</td>
								</tr>
							</table>
						</td>
						<td style="width: 50%; vertical-align: top;"><span class="helpItem">Interfaces</span><p>Interfaces to 3rd party software packages may require that interface settings for each tank that may be specified by the 3rd party software package be setup.</p>
							<table>
								<tr>
									<td><span class="helpItem">Interface settings:</span> contains a list of all the available interface setting records. To modify an existing interface setting record, select it from the drop down list. To add a new interface setting record, select "Add an interface" from the drop-down list.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Interface:</span> selects the interface that this tank interface settings correspond to.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Cross reference:</span> a cross reference used to identify the tank when received or sent through an interface with a 3rd party software package. Cross reference may be up to 50 characters in length.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Default setting:</span> used when there are multiple values set for an item for an interface, and the interface setting was not set for the item to be exported for this interface (exporting manually created orders, exporting to an interface from which it wasn&#39;t imported from, etc.).</td>
								</tr>
								<tr>
									<td><span class="helpItem">Export only:</span> is used only for exporting, to allow for multiple cross references available for different tanks. If checked, then this cross reference will not be used for inbound interface lookups.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Save interface:</span> click to save the tank interface settings record.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Remove interface:</span> click to remove the selected tank interface settings record.</td>
								</tr>
							</table>
						</td>
					</tr>
				</table>
			</div>
			<p><span class="helpItem">Save button:</span> located at the top of the page, click this button to save any changes made to a tank record or create a new tank record.</p>
			<p><span class="helpItem">Delete button:</span> located at the top of the page, click this button to delete the current tank record.</p>
		</div>
	</form>
</body>
</html>
