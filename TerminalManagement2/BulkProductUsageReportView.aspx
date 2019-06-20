<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="BulkProductUsageReportView.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.BulkProductUsageReportView" %>

<!DOCTYPE html>
<html lang="en">
<head id="Head1" runat="server">
	<title>Bulk Product Usage Report</title>
	<link rel="shortcut icon" href="FavIcon.ico" />
	<link rel="stylesheet" href="styles/site.css" />
	<link rel="stylesheet" href="styles/site-tablet.css" media="(max-width:768px)" />
	<link rel="stylesheet" href="styles/site-phone.css" media="(max-width:480px)" />
</head>
<body>
	<form id="Reports" runat="server">
	<div>
		<asp:Literal ID="litReport" runat="server"></asp:Literal>
	</div>
	</form>
</body>
</html>
