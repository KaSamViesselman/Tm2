<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="TankLevelTrendData.aspx.vb"
	Inherits="KahlerAutomation.TerminalManagement2.TankLevelTrendData" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Tank Level Trend Data</title>
	<link rel="shortcut icon" href="FavIcon.ico" />
	<meta name="viewport" content="width=device-width" />
	<meta http-equiv="X-UA-Compatible" content="IE=edge" />
	<link rel="stylesheet" href="styles/site.css" />
	<link rel="stylesheet" href="styles/site-tablet.css" media="(max-width:768px)" />
	<link rel="stylesheet" href="styles/site-phone.css" media="(max-width:480px)" />
</head>
<body>
	<form id="TankLevelTrendData" runat="server">
		<img id="imgGraph" runat="server" src="data:image/png;base64,0" />
		<br />
		<asp:Literal ID="litData" runat="server"></asp:Literal>
	</form>
</body>
</html>
