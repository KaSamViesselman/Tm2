<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="ProductList.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.ProductList"
	MaintainScrollPositionOnPostback="true" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Products : Product List</title>
	<link rel="shortcut icon" href="FavIcon.ico" />
	<meta name="viewport" content="width=device-width" />
	<meta http-equiv="X-UA-Compatible" content="IE=edge" />
	<link rel="stylesheet" href="styles/site.css" />
	<link rel="stylesheet" href="styles/site-tablet.css" media="(max-width:768px)" />
	<link rel="stylesheet" href="styles/site-phone.css" media="(max-width:480px)" />
	<script type="text/javascript" src="scripts/jquery-1.11.1.min.js"></script>
	<script type="text/javascript" src="scripts/soap-1.0.1.js"></script>
	<script type="text/javascript" src="scripts/page-controller.js"></script>
	<script type="text/javascript">
		function DisplayAddEmailButton(value) {
			if (value != '') {
				document.getElementById('btnAddEmailAddress').style.visibility = 'visible';
			}
			else {
				document.getElementById('btnAddEmailAddress').style.visibility = 'hidden';
			}
		}
	</script>
</head>
<body>
	<form id="main" method="post" runat="server">
	<div class="recordSelection">
		<span class="sectionRequiredField"><span class="required"></span>&nbsp;indicates required
			field </span>
		<ul>
			<li>
				<label>
					Facility
				</label>
				<span class="required">
					<asp:DropDownList ID="ddlLocation" runat="server" AutoPostBack="True">
					</asp:DropDownList>
				</span></li>
		</ul>
	</div>
	<div class="recordControl">
		<asp:Button ID="btnShow" runat="server" Text="Show Report" AutoPostBack="True" />
		<asp:Button ID="btnPrinterFriendly" runat="server" Text="Printer friendly" />
	</div>
	<div class="section">
		<asp:Literal ID="litReport" runat="server"></asp:Literal>
	</div>
	<div class="section" id="pnlSendEmail" runat="server">
		<hr style="width: 100%; color: #003399;" />
		<div class="sectionOdd">
			<ul>
				<li>
					<label>
						E-mail to</label>
					<asp:TextBox ID="tbxEmailTo" Style="width: 45%;" runat="server" AutoPostBack="true"></asp:TextBox>
					<asp:Button ID="btnSendEmail" Style="width: 15%;" runat="server" Text="Send" />
				</li>
				<li id="rowAddAddress" runat="server">
					<label>
						Add address</label>
					<asp:DropDownList ID="ddlAddEmailAddress" runat="server" Style="width: 45%;" onchange="DisplayAddEmailButton(this.value);">
					</asp:DropDownList>
					<asp:Button ID="btnAddEmailAddress" runat="server" Style="width: 15%;" Text="Add"
						visibility="false" />
				</li>
				<li style="color: Red;">
					<asp:Literal ID="litEmailConfirmation" runat="server"></asp:Literal>
				</li>
			</ul>
		</div>
	</div>
	</form>
</body>
</html>
