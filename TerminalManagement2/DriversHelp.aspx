<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="DriversHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.DriversHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Help : Drivers</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="DriversHelp" runat="server">
		<div>
			<span style="font-size: large; font-weight: bold;"><a href="help.aspx#Drivers">Help</a> : Drivers</span>
			<hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>Driver records represent drivers who are authorized to pick up loads.</p>
				<p><span class="helpItem">Driver drop-down list:</span> to modify an existing driver, select the driver from the drop-down list. To enter a new driver, select "Enter a new driver" from the drop-down list.</p>
				<table>
					<tr>
						<td style="width: 50%; border-right: 1px solid #404040;">
							<table>
								<tr>
									<td><span class="helpItem">Name:</span> the name of the driver is required and may be up to 50 characters in length.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Number:</span> a number that represents the driver, which may correspond to an iButton or card used by the driver to identify themselves at a keypad or self-serve terminal. The number is optional and may be up to 50 characters in length.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Street:</span> the street address of the driver is optional and may be up to 50 characters in length.</td>
								</tr>
								<tr>
									<td><span class="helpItem">City:</span> the city where the driver is located is optional and may be up to 50 characters in length.</td>
								</tr>
								<tr>
									<td><span class="helpItem">State:</span> the state or province that the driver is located in is optional and may be up to 50 characters in length.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Zip code:</span> the zip code for the driver is optional and may be up to 15 characters in length.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Phone:</span> the phone number for the driver is optional and may be up to 50 characters in length.</td>
								</tr>
								<tr>
									<td><span class="helpItem">E-mail:</span> if Terminal Management 2 is configured to e-mail tickets a ticket will be sent to this (or these) e-mail address(es) when the ticket is created by a system referencing this driver. E-mail is optional and multiple e-mail addresses may be entered separated by a comma.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Notes:</span> notes for driver are optional.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Password:</span> a password that the driver is required to enter at a keypad or self-serve terminal when identifying themselves.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Disabled:</span> used to temporarily disable access to systems.</td>
								</tr>
							</table>
						</td>
						<td style="width: 50%;">
							<p><span class="helpItem">Accounts</span> </p>
							<table>
								<tr>
									<td><span class="helpItem">Valid for all accounts:</span> allows the driver to pick up loads for all customer accounts, including new accounts added at a later time.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Add:</span> allows the driver to pick up loads for the customer account selected in the drop-down list to the left.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Remove:</span> removes the driver's ability to pick up loads for the customer account selected in the list to the left.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Add to all accounts:</span> allows the driver to pick up loads for all customer accounts.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Remove from all accounts:</span> removes the driver's ability to pick up loads for any customer accounts.</td>
								</tr>
							</table>
							<p><span class="helpItem">Facilities</span> </p>
							<table>
								<tr>
									<td><span class="helpItem">Valid for all facilities:</span> allows the driver to pick up loads for all facilities, including new facilities added at a later time.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Add:</span> allows the driver to pick up loads for the facility selected in the drop-down list to the left.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Remove:</span> removes the driver's ability to pick up loads for the facility selected in the list to the left.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Add to all facilities:</span> allows the driver to pick up loads for all facilities.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Remove from all facilities:</span> removes the driver's ability to pick up loads for all facilities.</td>
								</tr>
							</table>
							<p><span class="helpItem">Self serve permissions</span> </p>
							<table>
								<tr>
									<td><span class="helpItem">Can dispense partial orders:</span> allows the specified driver to select which individual products and quantities they can dispense at the time of loading if this functionality is enabled in the Self Serve application.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Can dispense hand add products:</span> allows the specified driver to dispense orders that have a hand add assigned to them for the panel associated with the Self Serve application if this functionality is enabled in the Self Serve application.</td>
								</tr>
							</table>
							<p><span class="helpItem">In facility</span> </p>
							<table>
								<tr>
									<td><span class="helpItem">Facility:</span> is the facility that the in facility record was recorded against.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Last entered:</span> is the time that was recorded as entering a facility for the most recent in facility record. If the date was not recorded, then this section will not be displayed.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Last exited:</span> is the time that was recorded as exiting a facility for the most recent in facility record. If the date was not recorded, then this section will not be displayed.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Current status:</span> will display the current status of the most recent in facility record.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Set as exited facility:</span> will mark the current in facility entry for the driver as being out of the facility.</td>
								</tr>
							</table>
							<p><span class="helpItem">Interfaces</span> </p>
							<p>Interfaces to 3rd party software packages may require that interface settings for each branch that may be specified by the 3rd party software package be setup.</p>
							<table>
								<tr>
									<td><span class="helpItem">Interface settings:</span> contains a list of all the available interface setting records. To modify an existing interface setting record, select it from the drop down list. To add a new interface setting record, select "Add an interface" from the drop-down list.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Interface:</span> selects the interface that this branch interface settings correspond to.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Cross reference:</span> a cross reference used to identify the branch when received in an order or sent as a ticket through an interface with a 3rd party software package. Cross reference may be up to 50 characters in length.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Default setting:</span> used when there are multiple values set for an item for an interface, and the interface setting was not set for the item to be exported for this interface (exporting manually created orders, exporting to an interface from which it wasn&#39;t imported from, etc.).</td>
								</tr>
								<tr>
									<td><span class="helpItem">Export only:</span> used only for outbound tickets, to allow for multiple cross references available for different branches. If checked, then this cross reference will not be used for inbound interface lookups.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Save interface:</span> click to save the branch interface settings record.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Remove interface:</span> click to remove the selected branch interface settings record.</td>
								</tr>
							</table>
						</td>
					</tr>
				</table>
			</div>
			<p><span class="helpItem">Save:</span> saves the changes made to an existing driver record or create a new driver when "enter a new driver" is selected in the user drop-down list.</p>
			<p><span class="helpItem">Delete:</span> deletes the selected driver.</p>
		</div>
	</form>
</body>
</html>
