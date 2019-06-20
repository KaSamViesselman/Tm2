<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="BulkProductUsageReport.aspx.vb"
	Inherits="KahlerAutomation.TerminalManagement2.BulkProductUsageReport" MaintainScrollPositionOnPostback="true" %>

<!DOCTYPE html>
<html lang="en">
<head id="Head1" runat="server">
	<title>Reports : Bulk Product Usage Report</title>
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
		<div class="recordControl">
			<asp:Button ID="btnShowReport" runat="server" Text="Show report" />
			<asp:Button ID="btnPrinterFriendly" runat="server" Text="Printer friendly" Visible="false" />
			<asp:Button ID="btnDownload" runat="server" Text="Download report" />
		</div>
		<div class="sectionEven">
			<ul>
				<li>
					<label style="font-weight: bold; text-align: left;">
						Filters</label>
				</li>
				<li>
					<label>
						Start date</label>
					<input type="text" name="tbxFromDate" id="tbxFromDate" value="" runat="server" />
					<script type="text/javascript">
						$('#tbxFromDate').datetimepicker({
							timeFormat: 'h:mm:ss TT',
							showSecond: true,
							showOn: "button",
							buttonImage: 'Images/Calendar_scheduleHS.png',
							buttonImageOnly: true,
							buttonText: "Show calendar"
						});
					</script>
				</li>
				<li>
					<label>
						End date</label>
					<input type="text" name="tbxToDate" id="tbxToDate" value="" runat="server" />
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
				<li>
					<label>
						Owner</label>
					<asp:DropDownList ID="ddlOwner" runat="server">
					</asp:DropDownList>
				</li>
				<li>
					<label>
						Bay</label>
					<asp:DropDownList ID="ddlBay" runat="server">
					</asp:DropDownList>
				</li>
				<li>
					<label>
						Panel</label>
					<asp:DropDownList ID="ddlPanel" runat="server">
					</asp:DropDownList>
				</li>
				<li>
					<label>
						Unit of measure</label>
					<asp:CheckBoxList ID="cblUnits" runat="server" RepeatLayout="UnorderedList" CssClass="input">
					</asp:CheckBoxList>
				</li>
				<li>
					<label>
						Adjust number of digits after the decimal point
					</label>
					<span style="display: inline-block; white-space: nowrap; width: 60%;">
						<asp:DropDownList ID="ddlTotalUnitsDecimals" runat="server">
						</asp:DropDownList>
						<asp:LinkButton ID="btnIncreaseTotalDecimalDigits" runat="server" CssClass="button"
							Text="+"></asp:LinkButton>
						<asp:LinkButton ID="btnDecreaseTotalDecimalDigits" runat="server" CssClass="button"
							Text="-"></asp:LinkButton></span> </li>
				<li>
					<label>
						Include all bulk products
					</label>
					<asp:CheckBox ID="cbxIncludeAllBulkProducts" runat="server" AutoPostBack="true" Text="" />
				</li>
				<li id="pnlIncludedBulkProducts" runat="server">
					<asp:CheckBoxList ID="cblIncludedBulkProducts" runat="server" RepeatColumns="2" Style="display: inline-block; width: 100%;">
					</asp:CheckBoxList>
				</li>
				<li>
					<label>
						Include voided tickets
					</label>
					<asp:CheckBox ID="cbxIncludeVoidedTickets" runat="server" />
				</li>
			</ul>
		</div>
		<div class="sectionOdd" id="pnlSendEmail" runat="server">
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
	</form>
</body>
</html>
