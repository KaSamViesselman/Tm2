<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="ContainerInventoryPFV.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.ContainerInventoryPFV" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Containers : Container Inventory</title>
	<link rel="shortcut icon" href="FavIcon.ico" />
	<meta name="viewport" content="width=device-width" />
	<meta http-equiv="X-UA-Compatible" content="IE=edge" />
	<link rel="stylesheet" href="styles/site.css" />
	<link rel="stylesheet" href="styles/site-tablet.css" media="(max-width:768px)" />
	<link rel="stylesheet" href="styles/site-phone.css" media="(max-width:480px)" />
</head>
<body>
	<form id="Report" runat="server">
		<asp:Literal ID="litReport" runat="server"></asp:Literal>
	</form>
</body>
</html>
