<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="PastOrders.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.PastOrders" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Orders : Past Orders</title>
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
<body onload="resizeIframe('orderSummaryFrame');">
	<form id="main" method="post" runat="server">
		<div class="recordSelection">
			<ul>
				<li>
					<label>
						Facility</label>
					<asp:DropDownList ID="ddlFacilityFilter" runat="server" AutoPostBack="true">
					</asp:DropDownList>
				</li>
				<li>
					<label>
						Order</label>
					<asp:DropDownList ID="ddlPastOrders" runat="server" AutoPostBack="True" />
					<span class="findRecord">
						<asp:TextBox ID="tbxFind" runat="server" />
						<asp:Button ID="btnFind" runat="server" Text="Find" />
						<asp:CheckBox ID="cbxIncludeArchived" runat="server" AutoPostBack="true" Text="Include archived"
							Visible="false" />
						&nbsp;<a href="AdvancedSearch.aspx?SearchType=PastOrders">Advanced Search</a>
					</span>
				</li>
			</ul>
		</div>
		<div class="recordControl">
			<asp:Button ID="btnMarkIncomplete" runat="server" Text="Mark incomplete" Enabled="False" />
			<asp:Button ID="btnArchive" runat="server" Text="Archive" Enabled="False" />
			<asp:Button ID="btnUnarchive" runat="server" Text="Unarchive" Enabled="False" />
			<asp:Button ID="btnPrinterFriendly" runat="server" Text="Printer Friendly" Enabled="false" />
		</div>
		<div class="section">
			<iframe id="orderSummaryFrame" runat="server" width="100%" height="600px" style="border: 1 solid #000000;"></iframe>
			<label id="errorDetails" style="width: 100%; text-align: left;">
			</label>
		</div>
	</form>
</body>
</html>
