<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="InterfaceAssignOrder.aspx.vb"
	Inherits="KahlerAutomation.TerminalManagement2.InterfaceAssignOrder" %>

<!DOCTYPE html>
<html lang="en">
<head id="Head1" runat="server">
	<title>Interfaces : Assign Interface to Orders</title>
	<link rel="shortcut icon" href="FavIcon.ico" />
	<meta name="viewport" content="width=device-width" />
	<meta http-equiv="X-UA-Compatible" content="IE=edge" />
	<link rel="stylesheet" href="styles/site.css" />
	<link rel="stylesheet" href="styles/site-tablet.css" media="(max-width:768px)" />
	<link rel="stylesheet" href="styles/site-phone.css" media="(max-width:480px)" />
	<script type="text/javascript" src="scripts/jquery-1.11.1.min.js"></script>
	<script type="text/javascript" src="scripts/soap-1.0.1.js"></script>
	<script type="text/javascript" src="scripts/page-controller.js"></script>
	<script type="text/javascript" src="jquery-ui/jquery-ui.min.js"></script>
	<link rel="stylesheet" href="jquery-ui/jquery-ui.min.css" />
	<link rel="stylesheet" href="jquery-ui/jquery-ui.structure.min.css" />
</head>
<body>
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
				<asp:DropDownList ID="ddlOrders" runat="server" AutoPostBack="True" />
				<span class="findRecord">
					<asp:TextBox ID="tbxFind" runat="server" MaxLength="50"></asp:TextBox>
					<asp:Button ID="btnFind" runat="server" Text="Find" />
				</span>&nbsp;<a href="AdvancedSearch.aspx?SearchType=InterfaceOrders">Advanced Search</a></li>
		</ul>
	</div>
	<div class="recordControl">
		<asp:Button ID="btnAssignInterfaceToOrder" runat="server" Text="Assign interface to order" />
		<asp:Label ID="lblStatus" runat="server" ForeColor="Red"></asp:Label>
	</div>
	<div class="sectionEven">
		<h1>
			General</h1>
		<ul>
			<li>
				<label>
					Interface
				</label>
				<asp:DropDownList ID="ddlInterfaces" runat="server" AutoPostBack="true">
				</asp:DropDownList>
			</li>
		</ul>
	</div>
	</form>
</body>
</html>
