<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="InventoryGroups.aspx.vb"
	Inherits="KahlerAutomation.TerminalManagement2.InventoryGroups" %>

<!DOCTYPE html>
<html lang="en">
<head id="Head1" runat="server">
	<title>Inventory : Inventory Groups</title>
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
			Inventory Groups
		</label>
		<asp:DropDownList ID="ddlInventoryGroups" runat="server" AutoPostBack="True" />
	</div>
	<div class="recordControl">
		<asp:Button ID="btnSave" runat="server" Text="Save" />
		<asp:Button ID="btnDelete" runat="server" Text="Delete" />
		<asp:Label ID="lblStatus" runat="server" ForeColor="#ff0000"></asp:Label>
		<div class="sectionRequiredField">
			<label>
				<span class="required"></span>&nbsp;indicates required field
			</label>
		</div>
	</div>
	<div class="sectionEven">
		<h1>
			General</h1>
		<ul>
			<li>
				<label>
					Name
				</label>
				<span class="required">
					<asp:TextBox ID="tbxName" runat="server" MaxLength="50"></asp:TextBox>
				</span></li>
			<li>
			<label>Is totalized group
			</label>
			<asp:CheckBox ID="cbxIsTotalizedGroup" runat="server" />
			</li>
		</ul>
	</div>
	<div class="sectionOdd">
		<h1>
			Bulk products</h1>
		<ul class="addRemoveSection">
			<li>
				<asp:DropDownList ID="ddlBulkProduct" runat="server" AutoPostBack="True" class="addRemoveList" />
				<asp:Button ID="btnAddBulkProduct" runat="server" Text="Add" Enabled="False" class="addRemoveButton" />
			</li>
			<li>
				<asp:ListBox ID="lstBulkProducts" runat="server" AutoPostBack="True" class="addRemoveList" />
				<asp:Button ID="btnRemoveBulkProduct" runat="server" Text="Remove" Enabled="False"
					class="addRemoveButton" />
			</li>
		</ul>
	</div>
	</form>
</body>
</html>
