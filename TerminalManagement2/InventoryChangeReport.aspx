<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="InventoryChangeReport.aspx.vb"
	Inherits="KahlerAutomation.TerminalManagement2.InventoryChangeReport" MaintainScrollPositionOnPostback="true" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Inventory : Inventory Change Report</title>
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
	<div class="section" id="pnlMain" runat="server">
		<div class="sectionEven">
			<ul>
				<li>
					<label>
						Owner
					</label>
					<asp:DropDownList ID="ddlOwner" runat="server" AutoPostBack="true">
					</asp:DropDownList>
				</li>
				<li>
					<label>
						Facility
					</label>
					<asp:DropDownList ID="ddlLocation" runat="server" AutoPostBack="true">
					</asp:DropDownList>
				</li>
				<li>
					<label>
						Bulk product
					</label>
					<asp:DropDownList ID="ddlBulkProduct" runat="server" AutoPostBack="true">
					</asp:DropDownList>
				</li>
			</ul>
		</div>
		<div class="sectionOdd">
			<ul>
				<li>
					<label>
						From
					</label>
					<div>
						<input type="text" name="tbxFromDate" id="tbxFromDate" value="" runat="server" />
					</div>
					<script type="text/javascript">
						$('#tbxFromDate').datetimepicker({
							timeFormat: 'h:mm:ss TT',
							showSecond: true,
							showOn: "button",
							buttonImage: 'Images/Calendar_scheduleHS.png',
							buttonImageOnly: true,
							buttonText: "Show calendar"
						});</script>
				</li>
				<li>
					<label>
						To
					</label>
					<div>
						<input type="text" name="tbxToDate" id="tbxToDate" value="" runat="server" />
					</div>
					<script type="text/javascript">
						$('#tbxToDate').datetimepicker({
							timeFormat: 'h:mm:ss TT',
							showSecond: true,
							showOn: "button",
							buttonImage: 'Images/Calendar_scheduleHS.png',
							buttonImageOnly: true,
							buttonText: "Show calendar"
						});
					</script>
				</li>
			</ul>
		</div>
		<div class="sectionEven">
			<h3>
				Additional Units</h3>
			<div class="collapsingSection not" id="pnlAdditionalUnits" runat="server">
				<ul>
					<li>
						<asp:CheckBoxList ID="cblAdditionalUnits" runat="server" RepeatLayout="UnorderedList" CssClass="input">
						</asp:CheckBoxList>
					</li>
				</ul>
			</div>
		</div>
	</div>
	<div class="recordControl">
		<ul>
			<li>
				<asp:Button ID="btnShowReport" runat="server" Text="Show report" />
				<asp:Button ID="btnDownload" runat="server" Text="Download report" />
			</li>
			<li>
				<asp:Label ID="litFiltersChanged" runat="server" />
			</li>
		</ul>
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
