<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="CustomPages.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.CustomPages" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Custom Pages : Custom Pages</title>
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
<body onload="resizeIframe('iframeAnalysisConfiguration');">
	<form id="main" method="post" runat="server" defaultfocus="tbxName">
		<div class="recordSelection">
			<label>Custom Page</label>
			<asp:DropDownList ID="ddlCustomPages" runat="server" AutoPostBack="True" />
		</div>
		<div class="recordControl">
			<asp:Button ID="btnSave" runat="server" Text="Save" />
			<asp:Button ID="btnDelete" runat="server" Text="Delete" Enabled="false" />
			<asp:Label ID="lblStatus" runat="server" ForeColor="Red" />
			<span class="sectionRequiredField">
				<span class="required"></span>&nbsp;indicates required field
			</span>
		</div>
		<div class="sectionEven" id="pnlMain" runat="server">
			<h1>General</h1>
			<ul>
				<li>
					<label>Page label</label>
					<span class="required">
						<asp:TextBox ID="tbxPageLabel" runat="server" /></span>
				</li>
				<li>
					<label>URL</label>
					<asp:TextBox ID="tbxPageURL" runat="server" />
				</li>
				<li>
					<label>Page type</label>
					<span class="input">
						<asp:RadioButtonList ID="rdoPageType" runat="server" AutoPostBack="True" RepeatLayout="Flow">
							<asp:ListItem Value="mainMenuLink" Text="Main menu link" Selected="True" />
							<asp:ListItem Value="report" Text="Report" />
							<asp:ListItem Value="analysis" Text="Analysis" />
							<asp:ListItem Value="custom_shortcut" Text="Custom shortcut" />
						</asp:RadioButtonList>
					</span>
				</li>
			</ul>
			<div class="section" id="pnlAnalysisOptions" runat="server">
				<ul>
					<li>
						<label></label>
						<asp:CheckBox ID="cbxBulkProductAnalysis" runat="server" Text="Bulk product analysis" />
					</li>
					<li>
						<label></label>
						<asp:CheckBox ID="cbxTankAnalysis" runat="server" Text="Tank analysis" />
					</li>
					<li>
						<label></label>
						<asp:CheckBox ID="cbxAnalysisUrlHasConfigOption" runat="server" Text="URL has configuration option" />
					</li>
				</ul>
			</div>
			<div class="section" id="pnlReportOptions" runat="server">
				<ul>
					<li>
						<label></label>
						<asp:CheckBox ID="chkEmailReport" runat="server" Text="E-mail report" AutoPostBack="true" />
					</li>
					<li>
						<label></label>
						<asp:CheckBox ID="chkViewReport" runat="server" Text="View report" /></li>
					<li></li>
				</ul>
			</div>
			<div class="section" id="pnlEmailWebServiceInfo" runat="server">
				<ul>
					<li>
						<label>E-mail web service URL:</label>
						<asp:TextBox ID="tbxEmailWebServiceUrl" runat="server" />
					</li>
					<li>
						<label>E-mail web service name (class name)</label>
						<asp:TextBox ID="tbxEmailWebServiceServiceName" runat="server" />
					</li>
					<li>
						<label>E-mail web service method name</label>
						<asp:TextBox ID="tbxEmailWebServiceMethodName" runat="server" />
					</li>
				</ul>
			</div>
			<asp:Panel ID="pnlCustomShortcut" runat="server" Visible="false">
				<ul>
					<li>
						<label>Prompt type</label>
						<asp:DropDownList ID="ddlCustomShortcutPromptType" runat="server">
							<asp:ListItem Value="0" Text="None" />
							<asp:ListItem Value="1" Text="Quantity" />
						</asp:DropDownList>
					</li>
				</ul>
			</asp:Panel>
		</div>
		<div id="pnlAnalysisConfiguration" runat="server" class="section">
			<iframe id="iframeAnalysisConfiguration" runat="server" style="width: 100%;"></iframe>
		</div>
	</form>
</body>
</html>
