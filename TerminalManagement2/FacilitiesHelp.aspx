<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="FacilitiesHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.FacilitiesHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server"><title>Help : Facilities</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="FacilitiesHelp" runat="server">
		<div><span style="font-size: large; font-weight: bold;"><a href="help.aspx#Facilities">Help</a>: Facilities</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>Facilities records are used to specify the location of panels, tanks, containers, load-out systems, etc. Facilities are also used to differentiate the bulk products that make up a product and the inventory of bulk products at each facility.</p>
				<p><span class="helpItem">Facility drop-down list:</span> located at the top of the page, this list contains all the facility records. To edit an existing facility record, select its name in the drop-down list. To create a new facility record, select "Enter new facility".</p>
				<table>
					<tr>
						<td style="width: 50%; border-right: 1px solid #404040;">
							<table>
								<tr>
									<td><span class="helpItem">Name:</span> the name of the facility. The name is required and may include up to 50 characters.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Address:</span> the street address of the facility. The address is optional and may include up to 100 characters.</td>
								</tr>
								<tr>
									<td><span class="helpItem">City:</span> the city that the facility is in. The city is optional and may include up to 50 characters.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Facility:</span> he facility that the container equipment is currently at (when not with customer) or was last at (when with customer).</td>
								</tr>
								<tr>
									<td><span class="helpItem">State:</span> the state or province that the facility is in. The state is optional and may include up to 50 characters.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Zip code:</span> the zip code for the facility. The zip code is optional and may include up to 15 characters.</td>
								</tr>
								<tr>
									<td><span class="helpItem">EPA number:</span> the EPA number for the facility. The EPA number is optional.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Phone:</span> the phone number for the facility. The phone number is optional and may include up to 50 characters.</td>
								</tr>
								<tr>
									<td><span class="helpItem">E-mail:</span> e-mail address(es) for the facility. If Terminal Management 2 is configured to e-mail tickets a ticket will be sent to this (or these) e-mail address(es) when the ticket is created by a system at this facility. E-mail is optional and multiple e-mail addresses may be entered separated by a comma.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Use owner order completion settings:</span> when checked the owner's order completion settings will be used in place of this facilities order completion settings.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Mark orders complete at percent:</span> the threshold at which an order should be marked as complete and no longer be available for loading. A 0% order completion would mark the order as complete after one load, no matter how much was loaded. A 100% order completion would require that all product was loaded to the set point or more before being marked as complete. This functionality may be enabled by checking the box to the left of the completion threshold.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Mark orders complete when the requested batch count is reached:</span> when checked the orders will be marked complete when the delivered batch count is equal to or greater than the requested batch count for the order.</td>
								</tr>
							</table>
						</td>
						<td style="width: 50%;"><span class="helpItem">Driver assignments</span><p>Shows which drivers are allowed to pick up loads from facilities on self-serve load-out systems. Drivers may be assigned to multiple facilities.</p>
							<table>
								<tr>
									<td><span class="helpItem">Driver drop-down list:</span> the list shows which drivers are currently available to the selected facility.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Driver assignment list:</span> the list shows which drivers are currently assigned to the selected facility.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Driver drop-down list:</span> select the driver to be assigned to the facility.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Add driver:</span> add the driver currently selected in the driver drop-down list to the facility.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Remove driver:</span> remove the driver currently selected in the driver assignment list from the facility.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Add all drivers:</span> add all the available drivers to the facility.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Remove all drivers:</span> remove all drivers listed in the driver assignment list from the facility.</td>
								</tr>
							</table>
							<span class="helpItem">Account assignments</span><p>Shows which customer accounts are allowed to pick up loads from facilities on self-serve load-out systems. Customer accounts may be assigned to multiple facilities.</p>
							<table>
								<tr></tr>
								<tr>
									<td><span class="helpItem">Facility drop-down list:</span> the list shows which customer accounts are currently available to the selected facility.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Accounts assignment list:</span> the list shows which customer accounts are currently assigned to the selected facility.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Account drop-down list:</span> select the customer account to be assigned to the facility.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Add account:</span> add the customer account currently selected in the account drop-down list to the facility.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Remove account:</span> remove the customer account currently selected in the account assignment list from the facility.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Add all customer accounts:</span> add all the available accounts to the facility.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Remove all customer accounts:</span> remove all customer accounts listed in the account assignment list from the facility.</td>
								</tr>
							</table>
							<span class="helpItem">Interfaces</span><p>Interfaces to 3rd party software packages may require that interface settings for each branch that may be specified by the 3rd party software package be setup.</p>
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
									<td><span class="helpItem">Export only:</span> is used only for outbound tickets, to allow for multiple cross references available for different branches. If checked, then this cross reference will not be used for inbound interface lookups.</td>
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
			<p><span class="helpItem">Save button:</span> located at the top of the page, click this button to save any changes made to a facility record or create a new facility record.</p>
			<p><span class="helpItem">Delete button:</span> located at the top of the page, click this button to delete the current facility record.</p>
		</div>
	</form>
</body>
</html>
