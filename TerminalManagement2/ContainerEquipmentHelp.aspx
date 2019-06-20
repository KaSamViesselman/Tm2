<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="ContainerEquipmentHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.ContainerEquipmentHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server"><title>Help : Container Equipment</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="ContainerEquipmentHelp" runat="server">
		<div><span style="font-size: large; font-weight: bold;"><a href="help.aspx#ContainerEquipment">Help</a> : Container Equipment</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>Container equipment may be used to track equipment (example: pumps) that are attached to containers. Multiple container equipment records may be attached to a container.</p>
				<p><span class="helpItem">Container equipment drop-down list:</span> located at the top of the page, this list contains all the container equipment records. To edit an existing container equipment record, select its name in the drop-down list. To create a new container equipment record, select "Enter new container equipment".</p>
				<table>
					<tr>
						<td><span class="helpItem">Name:</span> the name of the container equipment. The name is required and may include up to 50 characters.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Equipment type:</span> a reference field used to identify what type of equipment this record represents. The container equipment type is optional. Container equipment types are user defined under the "Container Equipment Types" tab.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Owner:</span> the owner of the container equipment. If an owner is selected, then only users setup for that owner (or for all owners) will be able to access the container equipment record. The owner is optional.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Facility:</span> the facility that the container equipment is currently at (when not with customer) or was last at (when with customer).</td>
					</tr>
					<tr>
						<td><span class="helpItem">With customer:</span> indicates if a customer currently has possession of the equipment.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Container:</span> the container that the equipment is currently associated/connected to.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Barcode:</span> a number that is used to identify the container equipment. This can be scanned by container filling/tracking software. The barcode is optional and may contain up to 50 characters (alpha-numeric)</td>
					</tr>
					<tr>
						<td><span class="helpItem">Last inspected:</span> the date that the container equipment was last inspected.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Passed inspection:</span> indicated if the container equipment passed inspection.</td>
					</tr>
				</table>
			</div>
			<p><span class="helpItem">Save button:</span> located at the top of the page, click this button to save any changes made to a container equipment record or create a new container equipment record.</p>
			<p><span class="helpItem">Delete button:</span> located at the top of the page, click this button to delete the current container equipment record.</p>
		</div>
	</form>
</body>
</html>
