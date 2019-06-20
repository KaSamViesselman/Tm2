<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="CropTypesHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.CropTypesHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server"><title>Help : Crop Types</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="CropTypesHelp" runat="server">
		<div><span style="font-size: large; font-weight: bold;"><a href="help.aspx#CropTypes">Help</a>: Crop Types</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>Crop types are used to identify the type of a crop.</p>
				<p><span class="helpItem">Crop types drop-down list:</span> located at the top of the page, this list contains all the crop type records. To create a new crop type select "Enter new crop type".</p>
				<table>
					<tr>
						<td><span class="helpItem">Name:</span> this is the name of the crop type. The name is required and may be up to 50 characters long.</td>
					</tr>
				</table>
			</div>
			<p><span class="helpItem">Save button:</span> located at the top of the page, click this button to save any changes made to a crop type record or create a new record.</p>
			<p><span class="helpItem">Delete button:</span> located at the top of the page, click this button to delete the current crop type record.</p>
		</div>
	</form>
</body>
</html>
