<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="CustomerActivityReportView.aspx.vb"
	Inherits="KahlerAutomation.TerminalManagement2.CustomerActivityReportView" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Customer Activity Report</title>
	<link rel="shortcut icon" href="FavIcon.ico" />
	<link href="style.css" type="text/css" rel="stylesheet" />
	<link href="borders.css" type="text/css" rel="Stylesheet" />
</head>
<body>
	<form id="Reports" runat="server">
	<div>
		<asp:Literal ID="litReport" runat="server"></asp:Literal>
	</div>
	</form>
</body>
</html>
