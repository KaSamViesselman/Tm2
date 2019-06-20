<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="TankLevelsHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.TankLevelsHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server"><title>Help : Tank Levels</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="TankLevelsHelp" runat="server">
		<div><span style="font-size: large; font-weight: bold;"><a href="help.aspx#TankLevels">Help</a>: Tank Levels</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>The tank levels page shows the last physical tank level readings and tank group totals. The last updated column shows when the physical tank level readings were taken. If the reading is more than 15 minutes old the date and time will be displayed in red to indicate that there may be a problem with the tank level monitor (TLM) panel or the network connectivity between the Terminal Management server and the TLM panel. The alarms column shows any active alarms for the tank. The level column shows the current level of the product in the tank in feet and inches. The quantity column shows the current physical quantity that is in the tank according to the level reading and the tank dimensions. The capacity column shows the user defined capacity of the tank.</p>
				<table>
					<tr>
						<td><span class="helpItem">Facility:</span> to filter the tank list to only show tanks at a specific facility, select the facility in the facility drop-down list.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Owner:</span> to filter the tank list to only show tanks for a specific owner, select the owner in the owner drop-down list. If the user has been assigned to an owner they will only see the tanks assigned to either the same owner or tanks not assigned to any owner.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Bulk product:</span> to filter the tank list to only show tanks for a specific bulk product, select the bulk product in the bulk product drop-down list.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Display unit:</span> to display the tank quantity and capacity in a specific unit of measure, select the unit of measure in the display unit drop-down list.</td>
					</tr>
				</table>
			</div>
		</div>
	</form>
</body>
</html>
