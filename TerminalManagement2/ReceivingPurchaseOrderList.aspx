<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="ReceivingPurchaseOrderList.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.ReceivingPurhcaseOrderList"
	MaintainScrollPositionOnPostback="true" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Purchase Orders : Receiving Purchase Orders List</title>
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
			<ul>
				<li>
					<label>
						Sort by
					</label>
					<span class="input">
						<asp:DropDownList ID="ddlSortBy" runat="server" Width="22%">
						</asp:DropDownList>
						<asp:DropDownList ID="ddlAscDesc" runat="server" Width="8%">
						</asp:DropDownList>
					</span></li>
				<li>
					<label>
						Facility
					</label>
					<asp:DropDownList ID="ddlFacilityFilter" runat="server">
					</asp:DropDownList>
				</li>
				<li>
					<label>
						Supplier
					</label>
					<asp:DropDownList ID="ddlSuppliers" runat="server">
					</asp:DropDownList>
				</li>
				<li>
					<label>
						Bulk product
					</label>
					<asp:DropDownList ID="ddlBulkProducts" runat="server">
					</asp:DropDownList>
				</li>
				<li>
					<label>
						Owner
					</label>
					<asp:DropDownList ID="ddlOwner" runat="server">
					</asp:DropDownList>
				</li>
			</ul>
		</div>
		<div class="recordControl">
			<asp:Button ID="btnShowReport" runat="server" Text="Show report" />
			<asp:Button ID="btnDownload" runat="server" Text="Download report" />
		</div>
		<div class="section">
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
