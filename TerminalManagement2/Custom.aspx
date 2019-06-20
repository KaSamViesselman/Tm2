<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="Custom.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.Custom" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Custom Page</title>
	<link rel="shortcut icon" href="FavIcon.ico" />
	<meta name="viewport" content="width=device-width" />
	<meta http-equiv="X-UA-Compatible" content="IE=edge" />
	<link rel="stylesheet" href="styles/site.css" />
	<link rel="stylesheet" href="styles/site-tablet.css" media="(max-width:768px)" />
	<link rel="stylesheet" href="styles/site-phone.css" media="(max-width:480px)" />
	<script type="text/javascript" src="scripts/jquery-1.11.1.min.js"></script>
	<script type="text/javascript" src="scripts/soap-1.0.1.js"></script>
	<script type="text/javascript" src="scripts/page-controller.js"></script>
</head>
<body onload="resizeIframe('customPageFrame');">
	<form id="main" method="post" runat="server" defaultfocus="">
		<div class="section">
			<iframe id="customPageFrame" src="" runat="server" style="width: 100%; height: 800px;"></iframe>
			<label id="errorDetails" style="width: 100%; text-align: left;">
			</label>
		</div>
	</form>
</body>
</html>
