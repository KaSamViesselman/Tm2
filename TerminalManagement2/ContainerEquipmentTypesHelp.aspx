<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="ContainerEquipmentTypesHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.ContainerEquipmentTypesHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server"><title>Help : Container Equipment Types</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="ContainerEquipmentTypesHelp" runat="server">
		<div><span style="font-size: large; font-weight: bold;"><a href="help.aspx#ContainerEquipmentTypes">Help</a> : Container Equipment Types</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>Container equipment types are used to identify the type of a container equipment on a container equipment record. Container equipment types are for reference only.</p>
				<p><span class="helpItem">Container equipment types drop-down list:</span> located at the top of the page, this list contains all the container equipment type records. To create a new container equipment type select "Enter new container equipment type".</p>
				<table style="width: 50%;">
					<tr>
						<td><span class="helpItem">Name:</span> this is the name of the container equipment type. The name is required and may be up to 50 characters long.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Use general container settings for report inspection intervals:</span> determines if the equipment should use the interval settings assigned to the container equipment type, or if it should use the setting assigned under General Container Settings for the container equipment report.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Equipment inspection interval:</span> how often container equipment (pumps, valves, etc.) should be inspected. Used to warn user if the equipment is due to be inspected.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Equipment inspection warning:</span> how many days before an inspection is due that a piece of equipment should be marked as requiring inspection on the all container equipment report.</td>
					</tr>
				</table>
			</div>
			<p><span class="helpItem">Save button:</span> located at the top of the page, click this button to save any changes made to a container equipment type record or create a new record.</p>
			<p><span class="helpItem">Delete button:</span> located at the top of the page, click this button to delete the current container equipment type record.</p>
		</div>
	</form>
</body>
</html>
