<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="BranchesHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.BranchesHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Help : Branches</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="BranchesHelp" runat="server">
		<div>
			<span style="font-size: large; font-weight: bold;"><a href="help.aspx#Branches">Help</a>: Branches</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>Branches are used to indicate where an order was created.</p>
				<p><span class="helpItem">Branch drop-down list:</span> to modify an existing branch, select the branch from the drop-down list. To enter a new branch, select "Enter new branch" from the drop-down list.</p>
				<table>
					<tr>
						<td style="width: 50%; border-right: 1px solid #404040">
							<table>
								<tr>
									<td><span class="helpItem">Name:</span> the name of the branch is required and may be up to 50 characters in length.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Address:</span> the street address of the branch is optional and may be up to 50 characters in length.</td>
								</tr>
								<tr>
									<td><span class="helpItem">City:</span> the city where the branch is located is optional and may be up to 50 characters in length.</td>
								</tr>
								<tr>
									<td><span class="helpItem">State:</span> the state or province that the branch is located in is optional and may be up to 50 characters in length.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Zip code:</span> the zip code for the branch is optional and may be up to 15 characters in length.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Phone:</span> the phone number for the branch is optional and may be up to 50 characters in length.</td>
								</tr>
								<tr>
									<td><span class="helpItem">E-mail:</span> if Terminal Management 2 is configured to e-mail tickets a ticket will be sent to this (or these) e-mail address(es) when the ticket is created by a system for an order referencing this branch. E-mail is optional and multiple e-mail addresses may be entered separated by a comma.</td>
								</tr>
							</table>
						</td>
						<td style="width: 50%;"><span class="helpItem">Interfaces</span><p>Interfaces to 3rd party software packages may require that interface settings for each branch that may be specified by the 3rd party software package be setup.</p>
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
			<p><span class="helpItem">Save:</span> saves the changes made to an existing branch record or create a new branch when "enter new branch" is selected in the user drop-down list.</p>
			<p><span class="helpItem">Delete:</span> deletes the selected branch.</p>
		</div>
	</form>
</body>
</html>
