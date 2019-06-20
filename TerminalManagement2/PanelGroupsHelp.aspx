<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="PanelGroupsHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.PanelGroupsHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server"><title>Panel Group Help</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="PanelGroupHelp" runat="server">
		<div><span style="font-size: large; font-weight: bold;"><a href="help.aspx#PanelGroup">Help</a> : Panel Group</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>A panel group may be used to group two or more panels that may have a shared resource that must be taken into account when running a load using both panels.</p>
				<p><strong>Group drop-down list:</strong> select a panel group to create or modify.</p>
				<p><strong>Name:</strong> the name of the panel group, which must be unique.</p>
				<p><strong>Cannot fill simultaneously:</strong> panels that are a member of this group cannot fill at the same time.</p>
				<p><strong>Members:</strong> the members of the panel group. Use the drop down list to select a panel to add, then click add. To remove a panel from the list, select the panel from the list box, then click remove.</p>
			</div>
		</div>
	</form>
</body>
</html>
