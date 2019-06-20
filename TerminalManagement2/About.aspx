<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="About.aspx.vb" Async="true" Inherits="KahlerAutomation.TerminalManagement2.About" %>

<!DOCTYPE html>
<html lang="en">
<head id="Head1" runat="server">
	<title>About</title>
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
<body>
	<form id="main" method="post" runat="server">
		<div class="sectionEven">
			<h1>Version information
			</h1>
			<hr />
			<ul id="pnlVersionInformation" runat="server">
			</ul>
		</div>
		<div class="sectionEven">
			<h1>Copyright notifications
			</h1>
			<hr />
			<ul>
				<li>
					<label style="text-align: left;">Terminal Management 2</label>
					<asp:Label ID="lblCopyright" runat="server" CssClass="input"></asp:Label>
				</li>
				<li style="padding-top: 1em;">
					<label style="text-align: left;">jQuery Timepicker Addon</label>
					<span style="width: 65%; text-align: left;" class="input">Copyright (c) 2016 Trent Richardson
						<br />
						<a href="http://trentrichardson.com/Impromptu/MIT-LICENSE.txt" style="margin-left: 2em;">Licensed MIT</a>
					</span>
				</li>
				<li style="padding-top: 1em;">
					<label style="text-align: left;">AJAX Control Toolkit</label>
					<span style="width: 65%; text-align: left;" class="input">Copyright (c) 2012-2019, CodePlex Foundation
						<br />
						<span style="margin-left: 2em;">All rights reserved.</span>
						<br />
						<a href="https://github.com/DevExpress/AjaxControlToolkit/blob/master/LICENSE.txt" style="margin-left: 2em;">New BSD License (BSD)</a>
					</span>
				</li>
			</ul>
		</div>
		<asp:ScriptManager runat="server" />
		<div id="pnlAuthorization" runat="server" class="sectionOdd">
			<auth:AuthorizationControl ID="authControl" runat="server" />
		</div>
        <div id="pnlUpdate" runat="server" class="sectionOdd">
			<update:UpdateControl ID="updateControl" runat="server" />
		</div>
	</form>
</body>
</html>
