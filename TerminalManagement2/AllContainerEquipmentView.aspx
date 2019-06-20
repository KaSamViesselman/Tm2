<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="AllContainerEquipmentView.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.AllContainerEquipmentView" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>All container equipment</title>
	<link rel="shortcut icon" href="FavIcon.ico" />
	<meta name="viewport" content="width=device-width" />
	<meta http-equiv="X-UA-Compatible" content="IE=edge" />
	<link rel="stylesheet" href="styles/site.css" />
	<link rel="stylesheet" href="styles/site-tablet.css" media="(max-width:768px)" />
	<link rel="stylesheet" href="styles/site-phone.css" media="(max-width:480px)" />
</head>
<body>
	<form id="Reports" runat="server">
	<div>
		<asp:Literal ID="litReport" runat="server" />
	</div>
	</form>
</body>
</html>
