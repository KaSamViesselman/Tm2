<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="SuppliersHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.SuppliersHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Help : Suppliers</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="SuppliersHelp" runat="server">
		<div>
			<span style="font-size: large; font-weight: bold;"><a href="help.aspx#Suppliers">Help</a> : Suppliers</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>Supplier records represent bulk material suppliers and are typically used with receiving purchase orders.</p>
				<p><span class="helpItem">Supplier drop-down list:</span> to edit an existing supplier, select it from the drop-down list. To create a new supplier, select "Enter a new supplier" from the drop-down list.</p>
				<table>
					<tr>
						<td style="width: 50%; border-right: 1px solid #404040;"><span class="helpItem">General</span><table>
							<tr>
								<td><span class="helpItem">Name:</span> the name of the supplier is required and may be up to 50 characters in length.</td>
							</tr>
							<tr>
								<td><span class="helpItem">Owner:</span> the owner that the supplier record is for. When selected, only users assigned to the owner or no owner are able to see the supplier record.</td>
							</tr>
							<tr>
								<td><span class="helpItem">Default cross reference:</span> will act as a default cross reference number to identify a supplier when working with some interfaces to 3rd party software packages if a specified cross reference number is not defined for that interface. The number is optional and may be up to 50 characters in length.</td>
							</tr>
							<tr>
								<td><span class="helpItem">Street:</span> the street address of the supplier account. The address is optional and may include up to 100 characters.</td>
							</tr>
							<tr>
								<td><span class="helpItem">City:</span> the city that the supplier is in. The city is optional and may include up to 50 characters.</td>
							</tr>
							<tr>
								<td><span class="helpItem">State:</span> the state or province that the supplier is in. The state is optional and may include up to 50 characters.</td>
							</tr>
							<tr>
								<td><span class="helpItem">Zip code:</span> the zip code for the supplier. The zip code is optional and may include up to 15 characters.</td>
							</tr>
							<tr>
								<td><span class="helpItem">Phone:</span> the phone number for the supplier. The phone number is optional and may include up to 50 characters.</td>
							</tr>
							<tr>
								<td><span class="helpItem">E-mail:</span> e-mail address(es) for the supplier. If Terminal Management 2 is configured to e-mail tickets a ticket will be sent to this (or these) e-mail address(es) when the ticket is created by a system for a purchase order referencing this supplier account. E-mail is optional and multiple e-mail addresses may be entered separated by a comma.</td>
							</tr>
						</table>
						</td>
						<td style="width: 50%;"><span class="helpItem">Interfaces</span><p>Interfaces to 3rd party software packages may require that interface settings for each supplier that may be specified by the 3rd party software package be setup.</p>
							<table>
								<tr>
									<td><span class="helpItem">Interface settings:</span> contains a list of all the available interface setting records. To modify an existing interface setting record, select it from the drop down list. To add a new interface setting record, select "Add an interface" from the drop-down list.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Interface:</span> selects the interface that this product interface settings correspond to.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Cross reference:</span> a cross reference used to identify the supplier when received or sent back through an interface with a 3rd party software package. Cross reference may be up to 50 characters in length.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Default setting:</span> used when there are multiple values set for an item for an interface, and the interface setting was not set for the item to be exported for this interface (exporting manually created orders, exporting to an interface from which it wasn&#39;t imported from, etc.).</td>
								</tr>
								<tr>
									<td><span class="helpItem">Export only:</span> used only for outbound tickets, to allow for multiple cross references available for different suppliers. If checked, then this cross reference will not be used for inbound interface lookups.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Save interface:</span> click to save the supplier interface settings record.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Remove interface:</span> click to remove the selected supplier interface settings record.</td>
								</tr>
							</table>
						</td>
					</tr>
				</table>
			</div>
			<p><span class="helpItem">Save:</span> saves the changes to a supplier record when an existing supplier is selected. Creates a new supplier record when "Enter a new supplier" is selected in the supplier drop-down list.</p>
			<p><span class="helpItem">Delete:</span> deletes the selected supplier record.</p>
		</div>
	</form>
</body>
</html>
