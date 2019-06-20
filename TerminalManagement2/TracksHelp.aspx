<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="TracksHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.TracksHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server"><title>Help : Tracks</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="TracksHelp" runat="server">
		<div><span style="font-size: large; font-weight: bold;"><a href="help.aspx#Tracks">Help</a>: Tracks</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>Track records represent rail car tracks located at a facility.</p>
				<p><span class="helpItem">Track drop-down list:</span> to edit an existing track, select it from the drop-down list. To create a new track, select "Enter a new track" from the drop-down list.</p>
				<table>
					<tr>
						<td><span class="helpItem">Name:</span> the name of the track is required and may be up to 50 characters in length.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Facility:</span> the facility where the bay is located. The facility is required.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Length:</span> the length of the track. Typically used to calculate the average number of rail cars that may be parked on the track.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Notes:</span> notes for the track are optional.</td>
					</tr>
				</table>
				<p><span class="helpItem">Save:</span> saves the changes to a track record when an existing track is selected. Creates a new track record when "Enter a new track" is selected in the track drop-down list.</p>
				<p><span class="helpItem">Delete:</span> deletes the selected track record.</p>
			</div>
		</div>
	</form>
</body>
</html>
