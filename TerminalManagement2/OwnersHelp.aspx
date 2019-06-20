<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="OwnersHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.OwnersHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Help : Owners</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="OwnersHelp" runat="server">
		<div>
			<span style="font-size: large; font-weight: bold;"><a href="help.aspx#Owners">Help</a>: Owners</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>Owner records represent product owners. Owners can be used to partition information in the Terminal Management application. For example, a user assigned to a specific owner may only see orders, products, bulk products, tanks, containers, inventory, etc. that are marked for the owner that the user has been assigned to.</p>
				<p><span class="helpItem">Owner drop-down list:</span> contains a list of all the owners. To modify an existing owner, select the owner from the drop-down list. To enter a new owner record, select "Enter a new owner" from the drop-down list.</p>
				<table>
					<tr>
						<td style="width: 50%; border-right: 1px solid #404040;">
							<table>
								<tr>
									<td><span class="helpItem">Name:</span> the owner's name is required and may be up to 50 characters in length.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Address:</span> the street address for the owner. The address is optional and may include up to 100 characters.</td>
								</tr>
								<tr>
									<td><span class="helpItem">City:</span> the city that the owner is in. The city is optional and may include up to 50 characters.</td>
								</tr>
								<tr>
									<td><span class="helpItem">State:</span> the state or province that the owner is in. The state is optional and may include up to 50 characters.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Zip code:</span> the zip code for the owner. The zip code is optional and may include up to 15 characters.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Phone:</span> the phone number for the owner. The phone number is optional and may include up to 50 characters.</td>
								</tr>
								<tr>
									<td><span class="helpItem">E-mail:</span> e-mail address(es) for the owner. If Terminal Management 2 is configured to e-mail tickets a ticket will be sent to this (or these) e-mail address(es) when the ticket is created by a system for this owner. E-mail is optional and multiple e-mail addresses may be entered separated by a comma.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Notes:</span> a reference field for any notes on the owner. The notes field is optional.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Mark orders complete at percent:</span> the threshold at which an order should be marked as complete and no longer be available for loading. A 0% order completion would mark the order as complete after one load, no matter how much was loaded. A 100% order completion would require that all product was loaded to the set point or more before being marked as complete. This functionality may be enabled by checking the box to the left of the completion threshold.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Mark orders complete when the requested batch count is reached:</span> when checked the orders will be marked complete when the delivered batch count is equal to or greater than the requested batch count for the order.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Next order number:</span> if the system is configured to use separate order numbers per owner (see General Settings / Order Settings) then the next order number may be modified here.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Ticket URL:</span> the web address of the owner's ticket. Typically this is configured by a Kahler Automation technician.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Next ticket number:</span> if the system is configured to use separate ticket numbers per owner (see General Settings / Ticket Settings) then the next ticket number may be modified here.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Ticket prefix:</span> if the system is configured to use separate ticket number prefixes per owner (see General Settings / Ticket Settings) then the ticket number prefix may be modified here.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Ticket suffix:</span> if the system is configured to use separate ticket number suffixes per owner (see General Settings / Ticket Settings) then the ticket number suffix may be modified here.</td>
								</tr>
							</table>
							<span class="helpItem">Receiving PO Tickets</span><table>
								<tr>
									<td><span class="helpItem">Receiving PO ticket URL:</span> the web address to use for printing completed transactions for receiving purchase orders.</td>
								</tr>
							</table>
						</td>
						<td style="width: 50%;"><span class="helpItem">Interfaces</span><p>Interfaces to 3rd party software packages may require that interface settings for each owner that may be specified by the 3rd party software package be setup.</p>
							<table>
								<tr>
									<td><span class="helpItem">Interface settings:</span> contains a list of all the available interface setting records. To modify an existing interface setting record, select it from the drop down list. To add a new interface setting record, select "Add an interface" from the drop-down list.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Interface:</span> selects the interface that this owner interface settings correspond to.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Cross reference:</span> a cross reference used to identify the owner when received in an order or sent as a ticket through an interface with a 3rd party software package. Cross reference may be up to 50 characters in length.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Default setting:</span> used when there are multiple values set for an item for an interface, and the interface setting was not set for the item to be exported for this interface (exporting manually created orders, exporting to an interface from which it wasn&#39;t imported from, etc.).</td>
								</tr>
								<tr>
									<td><span class="helpItem">Export only:</span> used only for outbound tickets, to allow for multiple cross references available for different owners. If checked, then this cross reference will not be used for inbound interface lookups.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Save interface:</span> click to save the owner interface settings record.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Remove interface:</span> click to remove the selected owner interface settings record.</td>
								</tr>
							</table>
						</td>
					</tr>
				</table>
			</div>
			<p><span class="helpItem">Save button:</span> located at the top of the page, click this button to save any changes made to a owner record or create a new owner record.</p>
			<p><span class="helpItem">Delete button:</span> located at the top of the page, click this button to delete the current owner record.</p>
		</div>
	</form>
</body>
</html>
