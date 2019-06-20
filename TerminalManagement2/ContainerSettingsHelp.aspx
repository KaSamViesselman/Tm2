<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="ContainerSettingsHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.ContainerSettingsHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head id="Head1" runat="server"><title>Help : General Options</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="ContainerSettingsHelp" runat="server">
		<div><span style="font-size: large; font-weight: bold;"><a href="help.aspx#ContainerSettings">Help</a> : Container Settings</span><hr />
			<div id="divHelp">
				<p>Container settings control the behavior of Terminal Management and the applications that work with Terminal Management.</p>
				<p><span class="helpItem">Default packaged inventory location:</span> the virtual facility that filled containers are assigned to.</p>
				<p><span class="helpItem">Container inspection interval:</span> how often containers (mini-bulk, shuttles, etc.) should be inspected. Used to warn user if container is due to be inspected.</p>
				<p><span class="helpItem">Container inspection warning:</span> how many days before an inspection is due that a container should be marked as requiring inspection on the all containers report.</p>
				<p><span class="helpItem">Equipment inspection interval:</span> how often container equipment (pumps, valves, etc.) should be inspected. Used to warn user if the equipment is due to be inspected.</p>
				<p><span class="helpItem">Equipment inspection warning:</span> how many days before an inspection is due that a piece of equipment should be marked as requiring inspection on the all container equipment report.</p>
			</div>
			<p><span class="helpItem">Save:</span> saves the settings entered.</p>
		</div>
	</form>
</body>
</html>
