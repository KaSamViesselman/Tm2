<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="AboutHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.AboutHelp" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Help : About</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="AboutHelp" runat="server">
		<div>
			<span style="font-size: large; font-weight: bold;"><a href="help.aspx#About">Help</a>: About</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>The information found on the about page includes version information, and copyright notices.</p>
				<p><span class="helpItem">Version information:</span> is the information regarding the main web application, as well as any other information useful for troubleshooting purposes.</p>
			</div>
		</div>
	</form>
</body>
</html>
