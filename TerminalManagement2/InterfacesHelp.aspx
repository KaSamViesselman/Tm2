<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="InterfacesHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.InterfacesHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head id="Head1" runat="server"><title>Help : Interface</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="InterfacesHelp" runat="server">
		<div><span style="font-size: large; font-weight: bold;"><a href="help.aspx#Interfaces">Help</a>: Interface</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>Interfaces are used to communicate with 3rd party software packages, usually to sync order, account, and ticket information.</p>
				<p><span class="helpItem">Interface drop-down list:</span> located at the top of the page, this list contains all the interface records. To create a new interface select "Enter a new interface".</p>
				<table>
					<tr>
						<td><span class="helpItem">Name:</span> this is the name of the interface. The name is required and may be up to 50 characters long.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Type:</span> this is the interface type this interface will reference. This is a required value.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Interface ID:</span> this is the database ID used when referencing this interface. This is used as a parameter to distinguish which interface to use. This field is not editable.</td>
					</tr>
				</table>
			</div>
			<p><span class="helpItem">Save button:</span> located at the top of the page, click this button to save any changes made to an Interface record or create a new record.</p>
			<p><span class="helpItem">Delete button:</span> located at the top of the page, click this button to delete the current Interface record.</p>
		</div>
	</form>
</body>
</html>
