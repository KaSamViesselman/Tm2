<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="AdvancedSearch.aspx.vb"
	Inherits="KahlerAutomation.TerminalManagement2.AdvancedSearch" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>Orders : Orders</title>
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
	<form id="main" method="post" runat="server">
	<div class="recordSelection">
		<div class="sectionEven">
			<h1>
				Filters
			</h1>
			<ul>
				<li id="pnlCustomerFilter" runat="server">
					<label>
						Customer
					</label>
					<asp:DropDownList ID="ddlCustomerFilter" runat="server" AutoPostBack="true">
					</asp:DropDownList>
				</li>
				<li id="pnlSupplierFilter" runat="server">
					<label>
						Supplier
					</label>
					<asp:DropDownList ID="ddlSupplierFilter" runat="server" AutoPostBack="true">
					</asp:DropDownList>
				</li>
				<li id="pnlOwnerFilter" runat="server">
					<label>
						Owner
					</label>
					<asp:DropDownList ID="ddlOwnerFilter" runat="server" AutoPostBack="True" />
				</li>
				<li id="pnlBranchFilter" runat="server">
					<label>
						Branch
					</label>
					<asp:DropDownList ID="ddlBranchFilter" runat="server" AutoPostBack="True" />
				</li>
				<li id="pnlProductFilter" runat="server">
					<label>
						Product
					</label>
					<asp:DropDownList ID="ddlProductFilter" runat="server" AutoPostBack="True" />
				</li>
				<li id="pnlBulkProductFilter" runat="server">
					<label>
						Bulk product
					</label>
					<asp:DropDownList ID="ddlBulkProductFilter" runat="server" AutoPostBack="True" />
				</li>
				<li id="pnlFromDate" runat="server">
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
				<li id="pnlToDate" runat="server">
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
				<li>
					<label>
						Keyword
					</label>
					<asp:TextBox ID="tbxFind" runat="server"></asp:TextBox>
				</li>
				<li>&nbsp;</li>
				<li id="pnlSortBy" runat="server">
					<label>
						Sort</label>
					<span class="input">
						<asp:DropDownList ID="ddlSortBy" runat="server" Style="width: auto;">
						</asp:DropDownList>
						<asp:DropDownList ID="ddlAscDesc" runat="server" Style="width: auto;">
						</asp:DropDownList>
					</span></li>
				<li id="pnlIncludeInterfaceCrossReferences" runat="server">
					<label>
						Include interface cross references
					</label>
					<asp:CheckBox ID="cbxIncludeInterfaceCrossReferences" runat="server" Text="" Checked="true" />
				</li>
			</ul>
		</div>
		<div class="sectionOdd" id="pnlActivityReportColumns" runat="server">
			<h1>
				Columns
			</h1>
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
				</ul>
			</div>
			<div class="sectionOdd">
				<ul>
					<li>
						<asp:CheckBox ID="cbxFacility" runat="server" Text="Facility" />
					</li>
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
						<asp:CheckBox ID="cbxDischargeLocations" runat="server" Text="Discharge Locations" />
					</li>
					<li>
						<asp:CheckBox ID="cbxApplicator" runat="server" Text="Applicator" />
					</li>
				</ul>
			</div>
		</div>
	</div>
	<div class="recordControl">
		<asp:Button ID="btnFilter" runat="server" Text="Filter" />
	</div>
	<div class="section">
		<asp:Literal ID="litReport" runat="server"></asp:Literal>
	</div>
	</form>
</body>
</html>
