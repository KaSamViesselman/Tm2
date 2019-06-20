<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="Interfaces.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.Interfaces" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Interfaces : Interfaces</title>
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
	<form id="main" runat="server" method="post" defaultfocus="ddlInterfaces">
	<div class="recordSelection">
		<label>
			Interfaces
		</label>
		<asp:DropDownList ID="ddlInterfaces" runat="server" AutoPostBack="True">
		</asp:DropDownList>
	</div>
	<div class="recordControl">
		<asp:Button ID="btnSave" runat="server" Text="Save" />
		<asp:Button ID="btnDelete" runat="server" Text="Delete" />
		<asp:Label ID="lblStatus" runat="server" ForeColor="#ff0000"></asp:Label>
		<span class="sectionRequiredField"><span class="required"></span>&nbsp;indicates required
			field </span>
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
				<label>
					Type
				</label>
				<span class="required">
					<asp:DropDownList ID="ddlInterfaceTypes" runat="server" AutoPostBack="True">
					</asp:DropDownList>
				</span></li>
			<li>
				<label>
					Interface ID
				</label>
				<asp:Literal ID="litInterfaceId" runat="server"></asp:Literal>
			</li>
		</ul>
	</div>
	<div class="sectionOdd">
		<ul id="lstCustomFields" runat="server">
		</ul>
	</div>
	</form>
</body>
</html>
