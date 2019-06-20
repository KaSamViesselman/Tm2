<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="AnalysisSettings.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.AnalysisSettings" %>

<!DOCTYPE html>
<html lang="en">
<head id="Head1" runat="server">
	<title>General Settings : Analysis Settings</title>
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
    <form id="main" method="post" runat="server" defaultfocus="ddlDefaultBulkProductAnalysis">
    <div class="recordControl">
		<asp:Button ID="btnSave" runat="server" Text="Save" />
		<asp:Label ID="lblSave" runat="server" ForeColor="Red" Text="Analysis settings were saved successfully"
			Visible="False" />
	</div>
	<div class="sectionEven" id="pnlAnalysisSettings" runat="server">
		<ul>
			<li>
				<label>
					Default bulk product analysis
				</label>
				<asp:DropDownList ID="ddlDefaultBulkProductAnalysis" runat="server">
				</asp:DropDownList>
			</li>
			<li>
				<label>
					Default tank analysis
				</label>
				<asp:DropDownList ID="ddlDefaultTankAnalysis" runat="server">
				</asp:DropDownList>
			</li>
			<li>
				<label>
					Default hazardous material analysis
				</label>
				<asp:DropDownList ID="ddlDefaultHazmatAnalysis" runat="server">
				</asp:DropDownList>
			</li>
		</ul>
	</div>
    </form>
</body>
</html>
