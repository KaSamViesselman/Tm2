<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="ApplicationUsage.aspx.vb"
	Inherits="KahlerAutomation.TerminalManagement2.ApplicationUsage" MaintainScrollPositionOnPostback="true" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Reports : Applications</title>
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
		<div class="recordControl">
			<asp:Button ID="btnPrinterFriendlyVersion" runat="server" Text="Printer Friendly Version" />
			<asp:Button ID="btnDownload" runat="server" Text="Download" />
		</div>
		<div id="pnlMain" runat="server" class="section">
			<asp:Literal ID="litReport" runat="server"></asp:Literal>
			<div class="sectionEven">
				<ul id="pnlRemoveApplicationPcUsage" runat="server" class="addRemoveSection">
					<li>Remove PC/Application</li>
					<li>
						<asp:ListBox ID="lstApplicationPcUsage" runat="server" CssClass="addRemoveList"></asp:ListBox>
						<asp:Button ID="btnRemoveApplicationPcUsage" runat="server" CssClass="addRemoveButton"
							Text="Remove" /></li>
				</ul>
			</div>
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
