<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="TransportTypesHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.TransportTypesHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Help : Transport Types</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="TransportTypesHelp" runat="server">
		<div>
			<span style="font-size: large; font-weight: bold;"><a href="help.aspx#TransportTypes">Help</a> : Transport Types</span>
			<hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>Transport types may be used to categorize transport records.</p>
				<p><span class="helpItem">Transport type drop-down list:</span> to modify an existing transport type, select the transport type from the drop-down list. To create a new transport type, select "Enter a new transport type" from the drop-down list.</p>
				<table>
					<tr>
						<td style="width: 50%; border-right: 1px solid #404040;">
							<table>
								<tr>
									<td><span class="helpItem">Name:</span> the transport type name is required and may be up to 50 characters in length.</td>
								</tr>
							</table>
						</td>
						<td style="width: 50%;"><span class="helpItem">Interfaces</span>
							<p>Interfaces to 3rd party software packages may require that interface settings for each transport type that may be specified by the 3rd party software package be setup.</p>
							<table>
								<tr>
									<td><span class="helpItem">Interface settings:</span> contains a list of all the available interface setting records. To modify an existing interface setting record, select it from the drop down list. To add a new interface setting record, select "Add an interface" from the drop-down list.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Interface:</span> selects the interface that this transport type interface settings correspond to.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Cross reference:</span> a cross reference used to identify the transport type when received in an order or sent as a ticket through an interface with a 3rd party software package. Cross reference may be up to 50 characters in length.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Default setting:</span> used when there are multiple values set for an item for an interface, and the interface setting was not set for the item to be exported for this interface (exporting manually created orders, exporting to an interface from which it wasn&#39;t imported from, etc.).</td>
								</tr>
								<tr>
									<td><span class="helpItem">Export only:</span> used only for outbound tickets, to allow for multiple cross references available for different transport types. If checked, then this cross reference will not be used for inbound interface lookups.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Save interface:</span> click to save the transport type interface settings record.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Remove interface:</span> click to remove the selected transport type interface settings record.</td>
								</tr>
							</table>
						</td>
					</tr>
				</table>
				<p><span class="helpItem">Save:</span> saves the changes made to an existing transport type is selected. Creates a new transport type if "Enter a new transport type" is selected in the transport type drop-down list.</p>
				<p><span class="helpItem">Delete:</span> deletes the selected transport type.</p>
			</div>
		</div>
	</form>
</body>
</html>
