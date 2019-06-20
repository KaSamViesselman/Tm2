<%@ Page Language="vb" AutoEventWireup="true" CodeBehind="InterfaceItems.aspx.vb"
	EnableViewState="true" MaintainScrollPositionOnPostback="true" Inherits="KahlerAutomation.TerminalManagement2.InterfaceUpdateCopy" %>

<!DOCTYPE html>
<html lang="en">
<head id="Head1" runat="server">
	<title>Interfaces : Interface Items</title>
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
	<form id="main" runat="server" method="post">
	<div class="recordSelection">
		<label>
			Cross reference type</label>
		<asp:DropDownList ID="ddlItemType" runat="server" AutoPostBack="True">
		</asp:DropDownList>
	</div>
	<div class="recordControl">
		<asp:Button ID="btnSave" runat="server" Text="Save" />
		<asp:Label ID="lblStatus" runat="server" ForeColor="#ff0000"></asp:Label>
	</div>
	<div id="divInterfaceSettings" runat="server" class="section">
		<table id="tblInterfaceSetting" runat="server" border="1" cellspacing="0">
			<tr>
				<th>
				</th>
				<th style="text-align: center;">
					Source Interface
				</th>
				<th>
					&nbsp;
				</th>
				<th style="text-align: center;">
					Target Interface
				</th>
			</tr>
			<tr>
				<th>
					<asp:Label ID="lblItemType" runat="server" Text="Item"></asp:Label>
				</th>
				<th style="text-align: center;">
					<asp:DropDownList ID="ddlInterface1" runat="server" AutoPostBack="true">
					</asp:DropDownList>
				</th>
				<th>
					&nbsp;
				</th>
				<th style="text-align: center;">
					<asp:DropDownList ID="ddlInterface2" runat="server" AutoPostBack="true">
					</asp:DropDownList>
				</th>
			</tr>
			<tr>
				<td>
					&nbsp;
				</td>
				<td style="text-align: center;">
					<asp:Button ID="btnCopy" runat="server" Text="Copy All" />
				</td>
				<td>
					&nbsp;
				</td>
				<td style="text-align: center;">
				</td>
			</tr>
		</table>
	</div>
	<div>
		<span id="pnlPageNumbers" runat="server" style="text-align: right; vertical-align: middle;">
			<asp:Button ID="btnPreviousPage" runat="server" Text="Previous" />
			&nbsp;
			<asp:DropDownList ID="ddlPageNumber" runat="server" AutoPostBack="true" EnableViewState="true">
			</asp:DropDownList>
			&nbsp;
			<asp:Button ID="btnNextPage" runat="server" Text="Next" />
		</span>&nbsp;&nbsp; <span id="pnlItemsToDisplay" runat="server" style="text-align: right;
			vertical-align: middle;">Items Per Page&nbsp;
			<asp:DropDownList ID="ddlItemsPerPage" runat="server" AutoPostBack="true">
				<asp:ListItem Text="25" Value="25" Selected="True"></asp:ListItem>
				<asp:ListItem Text="50" Value="50"></asp:ListItem>
				<asp:ListItem Text="100" Value="100"></asp:ListItem>
			</asp:DropDownList>
		</span>
	</div>
	</form>
</body>
</html>
