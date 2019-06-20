<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="BaysHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.BaysHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server"><title>Help : Bays</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="BaysHelp" runat="server">
		<div><span style="font-size: large; font-weight: bold;"><a href="help.aspx#Bays">Help</a> : Bays</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>Bay records represent load-out bays within a facility.</p>
				<p><span class="helpItem">Bay drop-down list:</span> to edit an existing bay, select it from the drop-down list. To create a new bay, select "Enter a new bay" from the drop-down list.</p>
				<table>
					<tr>
						<td><span class="helpItem">Name:</span> the name of the bay is required and may be up to 50 characters in length.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Facility:</span> the facility where the bay is located. The facility is required.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Staged orders return to scale:</span> when enabled, orders staged on a scale that are loaded in this bay must return to a truck scale to be weighed out.</td>
					</tr>
				</table>
			</div>
			<p><span class="helpItem">Save:</span> saves the changes to a bay record when an existing bay is selected. Creates a new bay record when "Enter a new bay" is selected in the bay drop-down list.</p>
			<p><span class="helpItem">Delete:</span> deletes the selected bay record.</p>
		</div>
	</form>
</body>
</html>
