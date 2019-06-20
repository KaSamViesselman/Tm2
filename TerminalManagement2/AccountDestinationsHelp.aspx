<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="AccountDestinationsHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.AccountDestinationsHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Help : Account Destinations</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="AccountDestinationsHelp" runat="server">
		<div>
			<span style="font-size: large; font-weight: bold;"><a href="help.aspx#AccountDestinations">Help</a> : Account Destinations</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>Destination records represent a location for a customer account where a load will be shipped to. The destination can be selected in place of the ship to information specified on the order.</p>
				<p><span class="helpItem">Account:</span> destinations are associated with accounts. Select an account to add, remove or modify a destination.</p>
				<p><span class="helpItem">Destination:</span> to modify an existing destination, select the destination in the drop-down list. To create a new destination, select "Enter a new destination" from the drop-down list.</p>
				<table>
					<tr>
						<td style="width: 50%; border-right: 1px solid #404040;"><span class="helpItem">General</span><table>
							<tr>
								<td><span class="helpItem">Name:</span> the name of the destination is required and may be up to 50 characters in length. The name is typically how the destination is identified when listed in load-out software.</td>
							</tr>
							<tr>
								<td><span class="helpItem">Account:</span> the account that is associated with inputed destination information.</td>
							</tr>
							<tr>
								<td><span class="helpItem">Street:</span> the street address of the destination is optional and may be up to 50 characters in length.</td>
							</tr>
							<tr>
								<td><span class="helpItem">City:</span> the city that the destination is located in. The city is optional and may be up to 50 characters in length.</td>
							</tr>
							<tr>
								<td><span class="helpItem">State:</span> the state or province that the destination is located in. The state is optional and may be up to 50 characters in length.</td>
							</tr>
							<tr>
								<td><span class="helpItem">Zip code:</span> the zip code for the destination. The zip code is optional and may be up to 50 characters in length.</td>
							</tr>
							<tr>
								<td><span class="helpItem">Acres:</span> if the destination is a field, its land area may be recorded in acres. Acres is optional.</td>
							</tr>
							<tr>
								<td><span class="helpItem">Default cross reference:</span> will act as a default cross reference number to identify a destination when working with some interfaces to 3rd party software packages if a specified cross reference number is not defined for that interface.</td>
							</tr>
							<tr>
								<td><span class="helpItem">E-mail:</span> e-mail address(es) for the account destination. If Terminal Management 2 is configured to e-mail tickets a ticket will be sent to this (or these) e-mail address(es) when the ticket is created by a system for this account destination. E-mail is optional and multiple e-mail addresses may be entered separated by a comma.</td>
							</tr>
							<tr>
								<td><span class="helpItem">Notes:</span> notes for account destination. Notes are optional.</td>
							</tr>
						</table>
						</td>
						<td style="width: 50%;"><span class="helpItem">Interfaces</span><p>Interfaces to 3rd party software packages may require that interface settings for each account destination that may be specified by the 3rd party software package be setup.</p>
							<table>
								<tr>
									<td><span class="helpItem">Interface settings:</span> contains a list of all the available interface setting records. To modify an existing interface setting record, select it from the drop down list. To add a new interface setting record, select "Add an interface" from the drop-down list.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Interface:</span> selects the interface that this product interface settings correspond to.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Cross reference:</span> a cross reference used to identify the account destination when received in an order or sent as a ticket through an interface with a 3rd party software package. Cross reference may be up to 50 characters in length.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Default setting:</span> used when there are multiple values set for an item for an interface, and the interface setting was not set for the item to be exported for this interface (exporting manually created orders, exporting to an interface from which it wasn&#39;t imported from, etc.).</td>
								</tr>
								<tr>
									<td><span class="helpItem">Export only:</span> used only for outbound tickets, to allow for multiple cross references available for different account destinations. If checked, then this cross reference will not be used for inbound interface lookups.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Save interface:</span> click to save the account destination interface settings record.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Remove interface:</span> click to remove the selected account destination interface settings record.</td>
								</tr>
							</table>
						</td>
					</tr>
				</table>
			</div>
			<p><span class="helpItem">Save button:</span> when an existing account destination is selected, clicking the "Save" button will update the account destination record information. If "Enter a new destination" is selected, clicking the "Save" button will create a new account destination record.</p>
			<p><span class="helpItem">Delete button:</span> deletes the selected account destination record.</p>
		</div>
	</form>
</body>
</html>
