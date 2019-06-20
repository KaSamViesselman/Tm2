<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="TankGroupsHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.TankGroupsHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server"><title>Help : Tank Groups</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="form1" runat="server">
		<div><span style="font-size: large; font-weight: bold;"><a href="help.aspx#TankGroups">Help</a>: Tank Groups</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>Tank group records may be used to group tanks, measured by a tank level monitor (TLM) panel, together. One use of tank groups might be to show the total physical inventory of a bulk product that is stored in multiple tanks.</p>
				<p><span class="helpItem">Tank group drop-down list:</span> located at the top of the page, this list contains all the tank group records. To edit an existing tank group record, select its name in the drop-down list. To create a new tank group record, select "Enter new tank group".</p>
				<table>
					<tr>
						<td><span class="helpItem">Name:</span> the name of the tank group. The name is required and may include up to 50 characters.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Tank drop-down list:</span> located below the tank group name, use this to select a tank to be added to the current tank group.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Add tank:</span> click this to add the tank selected in the tank drop-down list to the tank group.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Tank list:</span> a list of tanks associated with the selected tank group.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Remove tank:</span> select a tank in the tank list and click this to remove that tank from the selected tank group.</td>
					</tr>
				</table>
			</div>
			<p><span class="helpItem">Save button:</span> located at the top of the page, click this button to save any changes made to a tank group record or create a new tank group record.</p>
			<p><span class="helpItem">Delete button:</span> located at the top of the page, click this button to delete the current tank group record.</p>
		</div>
	</form>
</body>
</html>
