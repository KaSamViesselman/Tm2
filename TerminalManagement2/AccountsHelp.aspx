<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="AccountsHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.AccountsHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server"><title>Help : Accounts</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="AccountsHelp" runat="server">
		<div><span style="font-size: large; font-weight: bold;"><a href="help.aspx#Accounts">Help</a>: Accounts</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>Account records represent customer accounts that product may be sold to. Accounts are typically used with orders.</p>
				<p><span class="helpItem">Account drop-down list:</span> the account drop-down list contains all the customer accounts. If logged in as a user that has been assigned to a specific owner then only the customer accounts that have been assigned to that owner will be available. To modify an existing account, select the account from the drop-down list. To create a new account, select "Enter a new account" from the drop-down list.</p>
				<table>
					<tr>
						<td style="width: 50%; border-right: 1px solid #404040;"><span class="helpItem">General</span><table>
							<tr>
								<td><span class="helpItem">Name:</span> the name is required and may be up to 50 characters in length.</td>
							</tr>
							<tr>
								<td><span class="helpItem">Default cross reference:</span> will act as a default cross reference number to identify an account when working with some interfaces to 3rd party software packages if a specified cross reference number is not defined for that interface. The number is optional and may be up to 50 characters in length.</td>
							</tr>
							<tr>
								<td><span class="helpItem">Owner:</span> the owner that this customer account is associated with.</td>
							</tr>
							<tr>
								<td><span class="helpItem">Address:</span> the street address of the customer. The street address is optional and may be up to 50 characters in length.</td>
							</tr>
							<tr>
								<td><span class="helpItem">City:</span> the city that the customer is located in. The city is optional and may be up to 50 characters in length.</td>
							</tr>
							<tr>
								<td><span class="helpItem">State:</span> the state or province that the customer is located in. The state is optional and may be up to 50 characters in length.</td>
							</tr>
							<tr>
								<td><span class="helpItem">Zip code:</span> the zip code for the customer. The zip code is optional and may be up to 15 characters in length.</td>
							</tr>
							<tr>
								<td><span class="helpItem">County:</span> the county that the customer is located in. The county is optional and may be up to 50 characters in length.</td>
							</tr>
							<tr>
								<td><span class="helpItem">Phone:</span> the phone number for the customer. The phone number is optional and may be up to 50 characters in length.</td>
							</tr>
							<tr>
								<td><span class="helpItem">E-mail:</span> e-mail address(es) for the customer. If Terminal Management 2 is configured to e-mail tickets a ticket will be sent to this (or these) e-mail address(es) when the ticket is created by a system for this account. E-mail is optional and multiple e-mail addresses may be entered separated by a comma.</td>
							</tr>
							<tr>
								<td><span class="helpItem">Billing account number:</span> the number is typically used to reference to a billing number. The number is optional and may be up to 50 characters in length.</td>
							</tr>
							<tr>
								<td><span class="helpItem">Coop:</span> the co-op that the customer account belongs to. The co-op field is optional and may be up to 50 characters in length.</td>
							</tr>
							<tr>
								<td><span class="helpItem">Notes:</span> notes for customer. Notes are optional.</td>
							</tr>
							<tr>
								<td><span class="helpItem">Disabled:</span> determines if the customer account is active. If the customer account is disabled, orders for the account will be unloadable on self-serve systems.</td>
							</tr>
						</table>
						</td>
						<td style="width: 50%;">
							<p><span class="helpItem">Facilities</span></p>
							<table>
								<tr>
									<td><span class="helpItem">Valid for all facilities:</span> allows the customer to pick from all facilities, including new facilities added at a later time.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Add:</span> add the facility from the drop-down list to the list of facilities.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Remove:</span> remove the selected facility from the list of facilities.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Add to all facilities:</span> allows the customer to use all facilities.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Remove from all facilities:</span> disallow the customer from using any facilities.</td>
								</tr>
							</table>
							<span class="helpItem">Drivers</span><p>Shows which drivers are assigned to the selected customer account. Drivers may be assigned to multiple accounts.</p>
							<table>
								<tr>
									<td><span class="helpItem">Driver drop-down list:</span> the list shows which drivers are currently available for the selected customer account.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Driver assignment list:</span> the list shows which drivers are currently assigned to the selected account.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Driver drop-down list:</span> select the driver to be assigned to the customer account.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Add driver:</span> add the driver currently selected in the driver drop-down list to the customer account.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Remove driver:</span> remove the driver currently selected in the driver assignment list from the customer account.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Add all drivers:</span> add all the available drivers to the customer account.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Remove all drivers:</span> remove all drivers listed in the driver assignment list from the customer account.</td>
								</tr>
							</table>
							<span class="helpItem">Interfaces</span><p>Interfaces to 3rd party software packages may require that interface settings for each account that may be specified by the 3rd party software package be setup.</p>
							<table>
								<tr>
									<td><span class="helpItem">Interface settings:</span> contains a list of all the available interface setting records. To modify an existing interface setting record, select it from the drop down list. To add a new interface setting record, select "Add an interface" from the drop-down list.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Interface:</span> selects the interface that this product interface settings correspond to.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Cross reference:</span> a cross reference used to identify the account when received in an order or sent as a ticket through an interface with a 3rd party software package. Cross reference may be up to 50 characters in length.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Default setting:</span> used when there are multiple values set for an item for an interface, and the interface setting was not set for the item to be exported for this interface (exporting manually created orders, exporting to an interface from which it wasn&#39;t imported from, etc.).</td>
								</tr>
								<tr>
									<td><span class="helpItem">Export only:</span> is used only for outbound tickets, to allow for multiple cross references available for different accounts. If checked, then this cross reference will not be used for inbound interface lookups.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Save interface:</span> click to save the account interface settings record.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Remove interface:</span> click to remove the selected account interface settings record.</td>
								</tr>
							</table>
						</td>
					</tr>
				</table>
			</div>
			<p><span class="helpItem">Save button:</span> located at the top of the page, click this button to save any changes made to an account record or create a new account record.</p>
			<p><span class="helpItem">Delete button:</span> located at the top of the page, click this button to delete the current account record.</p>
		</div>
	</form>
</body>
</html>
