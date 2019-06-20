<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="Assignments.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.Assignments" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html lang="en">
<head id="Head1" runat="server">
	<title>Facilities : Assignments</title>
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
	<script type="text/javascript" src="Scripts/TimePicker/jquery-ui-timepicker-addon.min.js"></script>
	<link rel="stylesheet" href="jquery-ui/jquery-ui.min.css" />
	<link rel="stylesheet" href="jquery-ui/jquery-ui.structure.min.css" />
	<link rel="stylesheet" href="Styles/TimePicker/jquery-ui-timepicker-addon.min.css" />
</head>
<body>
	<form id="main" runat="server" method="post" defaultfocus="ddlLocations">
	<div class="recordSelection">
		<label>
			Facility
		</label>
		<asp:DropDownList ID="ddlLocations" runat="server" AutoPostBack="True">
		</asp:DropDownList>
	</div>
	<div class="recordControl">
		<asp:Label ID="lblStatus" runat="server" ForeColor="Red"></asp:Label>
	</div>
	<div class="section" id="pnlMain" runat="server">
		<asp:Panel ID="pnlDrivers" runat="server" CssClass="section">
			<h1>
				Assigned drivers</h1>
			<ul class="addRemoveSection">
				<li>
					<asp:DropDownList ID="ddlDrivers" runat="server" AutoPostBack="True" Enabled="False" CssClass="addRemoveList" />
					<asp:Button ID="btnAddDriver" runat="server" Text="Add driver" Enabled="False" CssClass="addRemoveButton" />
				</li>
				<li>
					<asp:ListBox ID="lstDrivers" runat="server" Rows="10" AutoPostBack="True" Enabled="False" CssClass="addRemoveList" />
					<asp:Button ID="btnRemoveDriver" runat="server" Text="Remove driver" Enabled="False" CssClass="addRemoveButton" />
					<asp:Button ID="btnAddAllDrivers" runat="server" Text="Add all drivers" Enabled="False" CssClass="addRemoveButton" />
					<asp:Button ID="btnRemoveAllDrivers" runat="server" Text="Remove all drivers" Enabled="False" CssClass="addRemoveButton" />
				</li>
			</ul>
		</asp:Panel>
		<asp:Panel ID="pnlAccounts" runat="server" CssClass="section">
			<h1>Assigned customer accounts</h1> <asp:CheckBox ID="cbxValid" runat="server" Text="Customer accounts valid for all facilities" />
			<ul class="addRemoveSection">
				<li>
					<asp:DropDownList ID="ddlAccounts" runat="server" AutoPostBack="True" Enabled="False" CssClass="addRemoveList" />
					<asp:Button ID="btnAddAccount" runat="server" Text="Add account" Enabled="False" CssClass="addRemoveButton" />
				</li>
				<li>
					<asp:ListBox ID="lstAccounts" runat="server" Rows="10" AutoPostBack="True" Enabled="False" CssClass="addRemoveList" />
					<asp:Button ID="btnRemoveAccount" runat="server" Text="Remove account" Enabled="False" CssClass="addRemoveButton" />
					<asp:Button ID="btnAddAllAccounts" runat="server" Text="Add all accounts" Enabled="False" CssClass="addRemoveButton" />
					<asp:Button ID="btnRemoveAllAccounts" runat="server" Text="Remove all accounts" Enabled="False" CssClass="addRemoveButton" />
				</li>
			</ul>
		</asp:Panel>
	</div>
	</form>
</body>
</html>
