<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="CustomerActivityReport.aspx.vb"
	Inherits="KahlerAutomation.TerminalManagement2.CustomerActivityReport" MaintainScrollPositionOnPostback="true" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Reports : Customer Activity Report</title>
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
			<asp:Button ID="btnPrinterFriendly" runat="server" Text="Printer friendly" />
			<asp:Button ID="btnDownload" runat="server" Text="Download report" />
			<asp:Button ID="btnPrintTickets" runat="server" Text="Print Tickets" />
		</div>
		<div class="sectionEven">
			<ul>
				<li>
					<label>
						From</label>
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
			</ul>
		</div>
		<div class="sectionOdd">
			<ul>
				<li>
					<label>
						To</label>
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
			</ul>
		</div>
		<div class="section">
			<hr style="width: 100%; color: #003399;" />
		</div>
		<div class="sectionEven">
			<ul>
				<li>
					<label style="font-weight: bold">
						Filters</label>
				</li>
				<li>
					<label>
						Customer account</label>
					<asp:DropDownList ID="ddlCustomerAccount" runat="server" AutoPostBack="true">
					</asp:DropDownList>
				</li>
				<li>
					<label>
						Customer destination</label>
					<asp:DropDownList ID="ddlCustomerDestination" runat="server">
					</asp:DropDownList>
				</li>
				<li>
					<label>
						Product</label>
					<asp:DropDownList ID="ddlProduct" runat="server">
					</asp:DropDownList>
				</li>
				<li>
					<label>
						Owner</label>
					<asp:DropDownList ID="ddlOwner" runat="server">
					</asp:DropDownList>
				</li>
				<li>
					<label>
						Branch</label>
					<asp:DropDownList ID="ddlBranch" runat="server">
					</asp:DropDownList>
				</li>
				<li>
					<label>
						Facility</label>
					<asp:DropDownList ID="ddlFacility" runat="server">
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
						Driver</label>
					<asp:DropDownList ID="ddlDriver" runat="server">
					</asp:DropDownList>
				</li>
				<li>
					<label>
						Transport</label>
					<asp:DropDownList ID="ddlTransport" runat="server">
					</asp:DropDownList>
				</li>
				<li>
					<label>
						Carrier</label>
					<asp:DropDownList ID="ddlCarrier" runat="server">
					</asp:DropDownList>
				</li>
				<li>
					<label>
						Applicator</label>
					<asp:DropDownList ID="ddlApplicator" runat="server">
					</asp:DropDownList>
				</li>
				<li>
					<label>
						User</label>
					<asp:DropDownList ID="ddlUser" runat="server">
					</asp:DropDownList>
				</li>
				<li>
					<label>
						Interface</label>
					<asp:DropDownList ID="ddlInterface" runat="server">
					</asp:DropDownList>
				</li>
			</ul>
		</div>
		<div class="sectionOdd">
			<ul>
				<li>
					<label style="font-weight: bold">
						Columns</label>
				</li>
			</ul>
			<div class="sectionEven">
				<ul>
					<li>
						<asp:CheckBox ID="cbxDateTime" runat="server" Text="Date/time" />
					</li>
					<li>
						<asp:CheckBox ID="cbxOrderNumber" runat="server" Text="Order number" />
					</li>
					<li>
						<asp:CheckBox ID="cbxTicketNumber" runat="server" AutoPostBack="true" Text="Ticket number" />
					</li>
					<li>
						<asp:CheckBox ID="cbxCustomer" runat="server" Text="Customer" />
					</li>
					<li>
						<asp:CheckBox ID="cbxCustomerDestination" runat="server" Text="Customer destination" />
					</li>
					<li>
						<asp:CheckBox ID="cbxOwner" runat="server" Text="Owner" />
					</li>
					<li>
						<asp:CheckBox ID="cbxBranch" runat="server" Text="Branch" />
					</li>
					<li>
						<asp:CheckBox ID="cbxFacility" runat="server" Text="Facility" />
					</li>
					<li>
						<asp:CheckBox ID="cbxNotes" runat="server" Text="Notes" />
					</li>
				</ul>
			</div>
			<div class="sectionOdd">
				<ul>
					<li>
						<asp:CheckBox ID="cbxDriver" runat="server" Text="Driver" />
					</li>
					<li>
						<asp:CheckBox ID="cbxTransports" runat="server" Text="Transports" />
					</li>
					<li>
						<asp:CheckBox ID="cbxCarrier" runat="server" Text="Carrier" />
					</li>
					<li>
						<asp:CheckBox ID="cbxPanel" runat="server" Text="Panel" />
					</li>
					<li>
						<asp:CheckBox ID="cbxDischargeLocations" runat="server" Text="Discharge locations" />
					</li>
					<li>
						<asp:CheckBox ID="cbxApplicator" runat="server" Text="Applicator" />
					</li>
					<li>
						<asp:CheckBox ID="cbxUser" runat="server" Text="User" />
					</li>
					<li>
						<asp:CheckBox ID="cbxInterface" runat="server" Text="Interface" />
					</li>
				</ul>
			</div>
			<div class="section">
				<ul>
					<li>
						<label>
							Products</label>
						<asp:DropDownList ID="ddlProductDisplayOptions" runat="server" AutoPostBack="true">
							<asp:ListItem Text="Products" Value="0"></asp:ListItem>
							<asp:ListItem Text="Bulk products" Value="1"></asp:ListItem>
							<asp:ListItem Text="Products on separate rows" Value="2"></asp:ListItem>
							<asp:ListItem Text="Products and bulk products" Value="3"></asp:ListItem>
						</asp:DropDownList>
					</li>
					<li id="pnlUnitDisplay" runat="server">
						<label>
							Unit of measure</label>
						<asp:DropDownList ID="ddlUnit" runat="server" AutoPostBack="true">
						</asp:DropDownList>
					</li>
					<li id="pnlUnitDecimalDisplay" runat="server">
						<label>
							Number of digits after the decimal point</label>
						<asp:DropDownList ID="ddlDigitsAfterDecimalPoint" runat="server">
							<asp:ListItem Value="0">0</asp:ListItem>
							<asp:ListItem Value="1">1</asp:ListItem>
							<asp:ListItem Value="2">2</asp:ListItem>
							<asp:ListItem Value="3">3</asp:ListItem>
							<asp:ListItem Value="4">4</asp:ListItem>
							<asp:ListItem Value="5">5</asp:ListItem>
							<asp:ListItem Value="6">6</asp:ListItem>
						</asp:DropDownList>
					</li>
					<li>
						<label>
							Total unit of measure</label>
						<asp:CheckBoxList ID="cblTotalUnits" runat="server" RepeatLayout="UnorderedList"
							CssClass="input">
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
				</ul>
			</div>
		</div>
		<div class="sectionEven">
			<ul>
				<li>
					<br />
					<label>
						Sort</label>
					<asp:DropDownList ID="ddlSort" runat="server">
					</asp:DropDownList>
				</li>
				<li>
					<label>
					</label>
					<asp:CheckBox ID="cbxIncludeVoidedTickets" runat="server" Text="Include voided tickets" />
				</li>
			</ul>
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
