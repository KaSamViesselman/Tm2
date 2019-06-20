<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="TankGroups.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.TankGroups" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Tanks : Tank Groups</title>
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
	<div class="recordSelection">
		<label>
			Tank group</label>
		<asp:DropDownList ID="ddlTankGroups" runat="server" AutoPostBack="True" />
	</div>
	<div class="recordControl">
		<asp:Button ID="btnSave" runat="server" Text="Save" />
		<asp:Button ID="btnDelete" runat="server" Text="Delete" />
		<asp:Label ID="lblStatus" runat="server" ForeColor="Red" />
		<div class="sectionRequiredField">
			<label>
				<span class="required"></span>indicates required field
			</label>
		</div>
	</div>
		<asp:Panel ID="pnlEven" runat="server" CssClass="sectionEven">

		<h1>
			General</h1>
		<ul>
			<li>
				<label>
					Name</label>
				<span class="required">
					<asp:TextBox ID="tbxName" runat="server" /></span> </li>
		</ul>
	</asp:Panel>
		<asp:Panel ID="pnlOdd" runat="server" CssClass="sectionOdd">

		<h1>
			Members</h1>
		<ul class="addRemoveSection">
			<li>
				<asp:DropDownList ID="ddlTank" runat="server" AutoPostBack="True" CssClass="addRemoveList" />
				<asp:Button ID="btnAddTank" runat="server" Text="Add" CssClass="addRemoveButton" />
			</li>
			<li>
				<asp:ListBox ID="lstTanks" runat="server" AutoPostBack="True" CssClass="addRemoveList" />
				<asp:Button ID="btnRemoveTank" runat="server" Text="Remove" CssClass="addRemoveButton" />
			</li>
		</ul>
	</asp:Panel>
	</form>
</body>
</html>
