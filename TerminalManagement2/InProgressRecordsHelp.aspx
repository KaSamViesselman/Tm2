<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="InProgressRecordsHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.InProgressRecordsHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head id="Head1" runat="server">
	<title>Help : In Progress Records</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="InProgressRecordsHelp" runat="server">
		<div>
			<span style="font-size: large; font-weight: bold;"><a href="help.aspx#InProgressRecords">Help</a> : In Progress Records</span>
			<hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>The in progress records page shows a list of all the current in progress records that applications are currently working on for dispensing, receiving, or manufacturing.</p>
				<p><span class="helpItem">Show Individual Weighments:</span> when checked, will show the individual entries for the in progress record. If not checked, will summarize the products and bulk products.</p>
			</div>
		</div>
	</form>
</body>
</html>
