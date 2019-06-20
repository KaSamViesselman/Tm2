<%@ Page Language="vb" AutoEventWireup="true" CodeBehind="EmailReports.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.EmailReports" MaintainScrollPositionOnPostback="true" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Reports : E-mail Reports</title>
	<link rel="shortcut icon" href="FavIcon.ico" />
	<meta name="viewport" content="width=device-width" />
	<meta http-equiv="X-UA-Compatible" content="IE=edge" />
	<link rel="stylesheet" href="jquery-ui/jquery-ui.min.css" />
	<link rel="stylesheet" href="jquery-ui/jquery-ui.structure.min.css" />
	<link rel="stylesheet" href="Styles/TimePicker/jquery-ui-timepicker-addon.min.css" />
	<link rel="stylesheet" href="styles/site.css" />
	<link rel="stylesheet" href="styles/site-tablet.css" media="(max-width:768px)" />
	<link rel="stylesheet" href="styles/site-phone.css" media="(max-width:480px)" />
	<script type="text/javascript" src="scripts/jquery-1.11.1.min.js"></script>
	<script type="text/javascript" src="scripts/soap-1.0.1.js"></script>
	<script type="text/javascript" src="scripts/page-controller.js"></script>
	<script type="text/javascript" src="jquery-ui/jquery-ui.min.js"></script>
	<script type="text/javascript" src="Scripts/TimePicker/jquery-ui-timepicker-addon.min.js"></script>
	<script type="text/javascript">
		function DisplayAddEmailButton(value) {
			if (value != '') {
				document.getElementById('btnAddEmailAddress').style.visibility = 'visible';
			}
			else {
				document.getElementById('btnAddEmailAddress').style.visibility = 'hidden';
			}
		}
		function SaveIframeData() {
			try {
				document.getElementById("frmConfig").contentWindow.SaveReportParameters();
				//document.title = document.getElementById("frmConfig").contentWindow.document.title;
			}
			catch (exception) {
			}
		}
	</script>
	<script type="text/javascript">
		function pageLoad(sender, args) {
			var sm = Sys.WebForms.PageRequestManager.getInstance();
			if (!sm.get_isInAsyncPostBack()) {
				sm.add_beginRequest(onBeginRequest);
				sm.add_endRequest(onRequestDone);
			}
			SetCalendarPickers();
		}

		function onBeginRequest(sender, args) {
			var send = args.get_postBackElement().value;
			$find('PleaseWaitPopup').show();
			document.getElementById('pnlRecordSelection').style.visibility = "hidden";
			document.getElementById('pnlRecordControl').style.visibility = "hidden";
			document.getElementById('pnlMain').style.visibility = "hidden";
		}

		function onRequestDone(sender, args) {
			$find('PleaseWaitPopup').hide();
			document.getElementById('pnlRecordSelection').style.visibility = "visible";
			document.getElementById('pnlRecordControl').style.visibility = "visible";
			document.getElementById('pnlMain').style.visibility = "visible";

			if (args.get_error() != undefined) {
				var errorMessage = args.get_error().message
				args.set_errorHandled(true);
				alert(errorMessage);
			}
		}

		function SetCalendarPickers() {
			$('#tbxLastSent').datetimepicker({
				timeFormat: 'h:mm:ss TT',
				showSecond: true,
				showOn: "button",
				buttonImage: 'Images/Calendar_scheduleHS.png',
				buttonImageOnly: true,
				buttonText: "Show calendar"
			});

			$('#tbxTriggerSendTime').datetimepicker({
				timeFormat: 'h:mm:ss TT',
				showSecond: true,
				showOn: "button",
				timeOnly: true,
				buttonImage: 'Images/Calendar_scheduleHS.png',
				buttonImageOnly: true,
				buttonText: "Show calendar"
			});
		}
	</script>
</head>
<body onload="resizeIframe('frmConfig');">
	<form id="main" method="post" runat="server">
		<asp:ScriptManager ID="ToolkitScriptManager1" runat="server" AsyncPostBackTimeout="3600" AsyncPostBackErrorMessage="Timeout while retrieving staged order data." EnablePartialRendering="true" OnAsyncPostBackError="ToolkitScriptManager1_AsyncPostBackError">
		</asp:ScriptManager>
		<asp:Panel ID="PleaseWaitMessagePanel" runat="server" CssClass="modalPopup" Style="height: 50px; width: 125px; text-align: center;">
			Please wait<br />
			<img src="images/ajax-loader.gif" alt="Please wait" />
		</asp:Panel>
		<asp:Button ID="HiddenButton" runat="server" Text="Hidden Button" Style="visibility: hidden;" ToolTip="Necessary for Modal Popup Extender" />
		<ajaxToolkit:ModalPopupExtender ID="PleaseWaitPopup" BehaviorID="PleaseWaitPopup" runat="server" TargetControlID="HiddenButton" PopupControlID="PleaseWaitMessagePanel" BackgroundCssClass="modalBackground">
		</ajaxToolkit:ModalPopupExtender>
		<asp:UpdatePanel ID="PleaseWaitPanel" runat="server" RenderMode="Inline">
			<ContentTemplate>
				<div id="pnlRecordSelection" class="recordSelection">
					<label>
						E-mail Report</label>
					<asp:DropDownList ID="ddlEmailReport" runat="server" AutoPostBack="True">
					</asp:DropDownList>
				</div>
				<div id="pnlRecordControl" class="recordControl">
					<asp:Button ID="btnSave" runat="server" Text="Save" Width="120px" />
					<asp:Button ID="btnDelete" runat="server" Text="Delete" Width="120px" Enabled="False" /><asp:Label ID="lblStatus" runat="server" Width="392px" ForeColor="Red"></asp:Label>
					<div class="sectionRequiredField">
						<label>
							<span class="required"></span>&nbsp;indicates required field
						</label>
					</div>
				</div>
				<asp:Panel ID="pnlMain" runat="server" CssClass="section">
					<div class="sectionEven">
						<h1>E-mail Report</h1>
						<ul>
							<li>
								<label>
									Name</label>
								<span class="required">
									<asp:TextBox ID="tbxName" runat="server" CssClass="required" /></span> </li>
							<li>
								<label>
									ID</label>
								<asp:Literal ID="litId" runat="server"></asp:Literal>
							</li>
							<li>
								<label>
									Subject</label>
								<span class="required">
									<asp:TextBox ID="tbxSubject" runat="server" CssClass="required"></asp:TextBox>
								</span></li>
							<li>
								<label>
									Report type</label>
								<asp:DropDownList ID="ddlReportType" runat="server" AutoPostBack="True">
									<asp:ListItem Text="Bulk product usage" Value="BulkProductUsageReport" />
									<asp:ListItem Text="Carrier list" Value="CarrierList" />
									<asp:ListItem Text="Container list" Value="ContainerList" />
									<asp:ListItem Text="Container history" Value="ContainerHistory" />
									<asp:ListItem Text="Customer activity report" Value="CustomerActivityReport" />
									<asp:ListItem Text="Driver in facility history" Value="DriverInFacilityHistoryReport" />
									<asp:ListItem Text="Driver list" Value="DriverList" />
									<asp:ListItem Text="Interface ticket export status report" Value="InterfaceTicketExportStatusReport" />
									<asp:ListItem Text="Interface ticket receiving export status report" Value="InterfaceTicketReceivingExportStatusReport" />
									<asp:ListItem Text="Inventory" Value="Inventory" />
									<asp:ListItem Text="Inventory change report" Value="InventoryChangeReport" />
									<asp:ListItem Text="Order list" Value="OrderList" />
									<asp:ListItem Text="Product allocation" Value="ProductAllocation" />
									<asp:ListItem Text="Product list" Value="ProductList" />
									<asp:ListItem Text="Receiving activity report" Value="ReceivingActivityReport" />
									<asp:ListItem Text="Receiving purchase order list report" Value="ReceivingPurchaseOrderList" />
									<asp:ListItem Text="Tank alarm history" Value="TankAlarmHistory" />
									<asp:ListItem Text="Tank levels" Value="TankLevels" />
									<asp:ListItem Text="Tank level trend" Value="TankLevelTrend" />
									<asp:ListItem Text="Track report" Value="TrackReport" />
									<asp:ListItem Text="Transport in facility history" Value="TransportInFacilityHistoryReport" />
									<asp:ListItem Text="Transport list" Value="TransportList" />
									<asp:ListItem Text="Transport usage report" Value="TransportUsageReport" />
									<asp:ListItem Text="Transport tracking report" Value="TransportTrackingReport" />
								</asp:DropDownList>
							</li>
							<li>
								<label>
									Report output</label>
								<asp:DropDownList ID="ddlReportRuntype" runat="server" AutoPostBack="True">
									<asp:ListItem Text="E-mail" Value="Email" />
									<asp:ListItem Text="Save file" Value="SaveFile" />
								</asp:DropDownList>
							</li>
							<li id="pnlRecipients" runat="server">
								<label>
									Recipients</label>
								<span class="required">
									<asp:TextBox ID="tbxRecipients" runat="server" AutoPostBack="true" class="required"></asp:TextBox>
								</span></li>
							<li id="rowAddAddress" runat="server">
								<label>
									Add address</label>
								<asp:DropDownList ID="ddlAddEmailAddress" runat="server" AutoPostBack="false" onchange="DisplayAddEmailButton(this.value);" Width="45%">
								</asp:DropDownList>
								<asp:Button ID="btnAddEmailAddress" runat="server" Text="Add" visibility="false" Width="15%" />
							</li>
							<li id="pnlReportDomainURL" runat="server">
								<label>
									Report domain URL (ex. http://64.121.65.7)
								</label>
								<asp:TextBox ID="tbxReportDomainURL" runat="server" AutoPostBack="true"></asp:TextBox>
							</li>
							<li id="pnlReportFileSaveLocation" runat="server">
								<label>
									Report file location and name
								</label>
								<span class="input">
									<asp:TextBox ID="tbxReportFileSaveLocation" runat="server" Style="width: 100%;"></asp:TextBox>
									<br />
									(e.g. C:\Kaco\Reports\ActivityReport{now}.csv) </span></li>
							<li>
								<label>
									Owner</label>
								<asp:DropDownList ID="ddlOwner" runat="server">
								</asp:DropDownList>
							</li>
							<li id="pnlFormat" runat="server" visible="false">
								<label>
									Format</label>
								<asp:DropDownList ID="ddlFormat" runat="server">
									<asp:ListItem Text="HTML" Value="html" />
									<asp:ListItem Text="HTML (printer friendly)" Value="pfv" />
									<asp:ListItem Text="CSV" Value="comma" />
								</asp:DropDownList>
							</li>
							<li>
								<label>
									Report is month to date</label>
								<asp:CheckBox ID="cbxMonthToDate" runat="server" Text="" Checked="false" />
							</li>
							<li>
								<label>
									Disabled</label>
								<asp:CheckBox ID="cbxDisabled" runat="server" Text="" Checked="false" />
							</li>
							<li id="pnlDateLastSent" runat="server">
								<label>
									Last sent</label>
								<input type="text" name="tbxLastSent" id="tbxLastSent" value="" runat="server" style="width: 200px;" />
							</li>
							<li>
								<hr />
							</li>
							<li id="pnlApplicator" runat="server" visible="false">
								<label>
									Applicator</label>
								<asp:DropDownList ID="ddlApplicator" runat="server">
								</asp:DropDownList>
							</li>
							<li id="pnlBay" runat="server" visible="false">
								<label>
									Bay</label>
								<asp:DropDownList ID="ddlBay" runat="server">
								</asp:DropDownList>
							</li>
							<li id="pnlBranch" runat="server" visible="false">
								<label>
									Branch</label>
								<asp:DropDownList ID="ddlBranch" runat="server">
								</asp:DropDownList>
							</li>
							<li id="pnlBulkProduct" runat="server" visible="false">
								<label>
									Bulk product</label>
								<asp:DropDownList ID="ddlBulkProduct" runat="server">
								</asp:DropDownList>
							</li>
							<li id="pnlIncludeAllBulkProducts" runat="server" visible="false">
								<label>
									Include all bulk products
								</label>
								<asp:CheckBox ID="cbxIncludeAllBulkProducts" runat="server" AutoPostBack="true" Checked="true" Text="" />
							</li>
							<li id="pnlBulkProductList" runat="server" visible="false">
								<label>
									Bulk product(s)</label>
								<asp:CheckBoxList ID="cblBulkProductList" runat="server" RepeatColumns="2" Style="display: inline-block;" class="input">
								</asp:CheckBoxList>
							</li>
							<li id="pnlCarrier" runat="server" visible="false">
								<label>
									Carrier</label>
								<asp:DropDownList ID="ddlCarrier" runat="server">
								</asp:DropDownList>
							</li>
							<li id="pnlContainer" runat="server" visible="false">
								<label>
									Container</label>
								<asp:DropDownList ID="ddlContainer" runat="server">
								</asp:DropDownList>
							</li>
							<li id="pnlCustomerAccount" runat="server" visible="false">
								<label>
									Customer account</label>
								<asp:DropDownList ID="ddlCustomerAccount" runat="server" AutoPostBack="true">
								</asp:DropDownList>
							</li>
							<li id="pnlCustomerAccountDestination" runat="server" visible="false">
								<label>
									Customer destination</label>
								<asp:DropDownList ID="ddlCustomerAccountDestination" runat="server">
								</asp:DropDownList>
							</li>
							<li id="pnlSupplierAccount" runat="server" visible="false">
								<label>
									Supplier account</label>
								<asp:DropDownList ID="ddlSupplierAccount" runat="server">
								</asp:DropDownList>
							</li>
							<li id="pnlDriver" runat="server" visible="false">
								<label>
									Driver</label>
								<asp:DropDownList ID="ddlDriver" runat="server">
								</asp:DropDownList>
							</li>
							<li id="pnlLocation" runat="server" visible="false">
								<label>
									Facility</label>
								<asp:DropDownList ID="ddlLocation" runat="server">
								</asp:DropDownList>
							</li>
							<li id="pnlPanels" runat="server" visible="false">
								<label>
									Panel</label>
								<asp:DropDownList ID="ddlPanel" runat="server">
								</asp:DropDownList>
							</li>
							<li id="pnlProduct" runat="server" visible="false">
								<label>
									Product</label>
								<asp:DropDownList ID="ddlProduct" runat="server">
								</asp:DropDownList>
							</li>
							<li id="pnlTank" runat="server" visible="false">
								<label>
									Tank</label>
								<asp:DropDownList ID="ddlTank" runat="server">
								</asp:DropDownList>
							</li>
							<li id="pnlTankLevelTrend" runat="server" visible="false">
								<label>
									Tank level trend</label>
								<asp:DropDownList ID="ddlTankLevelTrend" runat="server">
								</asp:DropDownList>
							</li>
							<li id="pnlShowTemperature" runat="server">
								<label>
									&nbsp;
								</label>
								<asp:CheckBox ID="cbxShowTemperature" runat="server" AutoPostBack="False" Text="Show temperature" />
							</li>
							<li id="pnlTrack" runat="server" visible="false">
								<label>
									Track</label>
								<asp:DropDownList ID="ddlTrack" runat="server">
								</asp:DropDownList>
							</li>
							<li id="pnlTransport" runat="server" visible="false">
								<label>
									Transport</label>
								<asp:DropDownList ID="ddlTransport" runat="server">
								</asp:DropDownList>
							</li>
							<li id="pnlUser" runat="server" visible="false">
								<label>
									User</label>
								<asp:DropDownList ID="ddlUser" runat="server">
								</asp:DropDownList>
							</li>
							<li id="pnlInterface" runat="server" visible="false">
								<label>
									Interface</label>
								<asp:DropDownList ID="ddlInterface" runat="server">
								</asp:DropDownList>
							</li>
							<li id="pnlUnit" runat="server" visible="false">
								<label>
									Unit</label>
								<asp:DropDownList ID="ddlUnit" runat="server" Width="244px">
								</asp:DropDownList>
								<div id="lblUnitDigitsAfterDecimalPoint" runat="server">
									<label>
										Number of digits after the decimal point</label>
									<asp:DropDownList ID="ddlUnitDigitsAfterDecimalPoint" runat="server" Width="100px">
										<asp:ListItem Value="0">0</asp:ListItem>
										<asp:ListItem Value="1">1</asp:ListItem>
										<asp:ListItem Value="2">2</asp:ListItem>
										<asp:ListItem Value="3">3</asp:ListItem>
										<asp:ListItem Value="4">4</asp:ListItem>
										<asp:ListItem Value="5">5</asp:ListItem>
										<asp:ListItem Value="6">6</asp:ListItem>
									</asp:DropDownList>
								</div>
							</li>
							<li id="pnlTotalUnitColumn" runat="server" visible="false">
								<label>
									Total unit of measure</label>
								<asp:CheckBoxList ID="cblTotalUnits" runat="server" RepeatLayout="UnorderedList" CssClass="input">
								</asp:CheckBoxList>
								<br />
								<label>
									Adjust number of digits after the decimal point
								</label>
								<span style="display: inline-block; white-space: nowrap; width: 60%;">
									<asp:DropDownList ID="ddlTotalUnitsDecimals" runat="server">
									</asp:DropDownList>
									<asp:LinkButton ID="btnIncreaseTotalDecimalDigits" runat="server" CssClass="button" Text="+"></asp:LinkButton>
									<asp:LinkButton ID="btnDecreaseTotalDecimalDigits" runat="server" CssClass="button" Text="-"></asp:LinkButton></span> </li>
							<li id="pnlAdditionalUnits" runat="server" visible="false">
								<label>
									Show additional units</label>
								<asp:CheckBoxList ID="cblShowAdditionalUnits" runat="server" RepeatLayout="UnorderedList" CssClass="input">
								</asp:CheckBoxList>
							</li>
							<li id="pnlDisplayUnit" runat="server" visible="false">
								<label>
									Display Unit</label>
								<asp:DropDownList ID="ddlDisplayUnit" runat="server">
								</asp:DropDownList>
							</li>
							<li id="pnlTransportTrackingOrderBy" runat="server" visible="false">
								<label>
									Order By</label>
								<asp:DropDownList ID="ddlTransportTrackingOrderBy" runat="server" Width="45%">
								</asp:DropDownList>
								<asp:DropDownList ID="ddlTransportTrackingAscDesc" runat="server" Width="15%">
								</asp:DropDownList>
							</li>
							<li id="pnlOrderListOrderBy" runat="server" visible="false">
								<label>
									Order By</label>
								<asp:DropDownList ID="ddlOrderListOrderBy" runat="server" Width="45%">
								</asp:DropDownList>
								<asp:DropDownList ID="ddlOrderListAscDesc" runat="server" Width="15%">
								</asp:DropDownList>
							</li>
							<li id="pnlReceivingPurchageOrderListOrderBy" runat="server" visible="false">
								<label>
									Order By</label>
								<asp:DropDownList ID="ddlReceivingPurchaseOrderListOrderBy" runat="server" Width="45%">
								</asp:DropDownList>
								<asp:DropDownList ID="ddlReceivingPurchaseOrderListAscDesc" runat="server" Width="15%">
								</asp:DropDownList>
							</li>
							<li id="pnlOrderListReportType" runat="server" visible="false">
								<label>
									Order list report type</label>
								<asp:DropDownList ID="ddlOrderListReportType" runat="server">
								</asp:DropDownList>
							</li>
							<li id="pnlCustomerActivityReport" runat="server" visible="false">
								<ul>
									<li>
										<label>
											Product display
										</label>
										<asp:DropDownList ID="ddlProductDisplayOptions" runat="server">
											<asp:ListItem Text="Products" Value="0" />
											<asp:ListItem Text="Bulk products" Value="1" />
											<asp:ListItem Text="Products on separate rows" Value="2" />
											<asp:ListItem Text="Products and bulk products" Value="3" />
										</asp:DropDownList>
									</li>
									<li>
										<div class="sectionEven">
											<label style="font-weight: bold;">
												Columns</label>
											<ul>
												<li>
													<asp:CheckBox ID="cbxDateTime" runat="server" Text="Date/time" />
												</li>
												<li>
													<asp:CheckBox ID="cbxOrderNumber" runat="server" Text="Order number" />
												</li>
												<li>
													<asp:CheckBox ID="cbxTicketNumber" runat="server" Text="Ticket number" AutoPostBack="true" />
												</li>
												<li>
													<asp:CheckBox ID="cbxCustomer" runat="server" Text="Customer" />
													<asp:CheckBox ID="cbxSupplier" runat="server" Text="Supplier" />
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
												<li id="pnlLotNumber" runat="server">
													<asp:CheckBox ID="cbxLotNumber" runat="server" Text="Lot number" />
												</li>
											</ul>
										</div>
										<div class="sectionOdd">
											<br />
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
													<asp:CheckBox ID="cbxDischargeLocations" runat="server" Text="Discharge Locations" />
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
									</li>
									<li>
										<div class="section">
										</div>
										<label>
											Sort by</label>
										<asp:DropDownList ID="ddlSortBy" runat="server">
										</asp:DropDownList>
									</li>
								</ul>
							</li>
							<li id="pnlIncludeVoidedTickets" runat="server" visible="false">
								<label>
									Include voided tickets</label>
								<asp:CheckBox ID="cbxIncludeVoidedTickets" runat="server" Checked="false" />
							</li>
							<li id="pnlInventory" runat="server" visible="false">
								<ul>
									<li>
										<label>
											Only show non-zero inventories</label>
										<asp:CheckBox ID="cbxOnlyShowBulkProductsWithNonZeroInventory" runat="server" Text="" Checked="True" />
									</li>
									<li>
										<label>
											Assign physical inventory to owner assigned to tank</label>
										<asp:CheckBox ID="cbxAssignPhysicalInventoryToOwner" runat="server" Text="" Checked="True" AutoPostBack="true" />
									</li>
								</ul>
							</li>
							<li id="pnlTrackReportColumns" runat="server" visible="false">
								<ul>
									<li>
										<label>
										</label>
										<asp:CheckBox ID="cbxShowOperator" runat="server" Checked="true" Text="Show operator name" />
									</li>
									<li>
										<label>
										</label>
										<asp:CheckBox ID="cbxShowRfid" runat="server" AutoPostBack="False" Checked="True" Text="Show RFID number(s)" />
									</li>
									<li>
										<label>
										</label>
										<asp:CheckBox ID="cbxShowCarNumber" runat="server" AutoPostBack="False" Checked="True" Text="Show rail car number" />
									</li>
									<li>
										<label>
										</label>
										<asp:CheckBox ID="cbxShowTrack" runat="server" AutoPostBack="False" Checked="True" Text="Show track name" />
									</li>
									<li>
										<label>
										</label>
										<asp:CheckBox ID="cbxShowScannedTime" runat="server" AutoPostBack="False" Checked="True" Text="Show scanned date/time" />
									</li>
									<li>
										<label>
										</label>
										<asp:CheckBox ID="cbxShowReverseOrder" runat="server" AutoPostBack="False" Text="Show in reverse order" />
									</li>
								</ul>
							</li>
							<li id="pnlOnlyShowProductsWithBulkProductsAtLocation" runat="server" visible="false">
								<ul>
									<li>
										<label>
											Only show products with bulk products at location
										</label>
										<asp:CheckBox ID="cbxOnlyShowProductsWithBulkProductsAtLocation" runat="server" Text="" Checked="true" />
									</li>
								</ul>
							</li>
							<li id="pnlTicketExportStatus" runat="server" visible="false">
								<ul>
									<li>

										<label>Sort by</label>
										<asp:DropDownList ID="ddlTicketSort" runat="server" AutoPostBack="true">
											<asp:ListItem Text="Exported date (Asc)" Value="exported_at ASC"></asp:ListItem>
											<asp:ListItem Text="Exported date (Desc)" Value="exported_at DESC" Selected="True"></asp:ListItem>
											<asp:ListItem Text="Loaded date (Asc)" Value="loaded_at ASC"></asp:ListItem>
											<asp:ListItem Text="Loaded date (Desc)" Value="loaded_at DESC"></asp:ListItem>
											<asp:ListItem Text="Ticket number (Asc)" Value="number ASC"></asp:ListItem>
											<asp:ListItem Text="Ticket number (Desc)" Value="number DESC"></asp:ListItem>
										</asp:DropDownList><br />
										<label></label>
										<asp:RadioButton ID="rbShowTicketsExported" GroupName="ShowExported" runat="server" AutoPostBack="true" Text="Show tickets exported" /><br />
										<label></label>
										<asp:CheckBox ID="cbxIncludeTicketsMarkedManually" runat="server" AutoPostBack="true"
											Text="Include tickets that were marked manually" Style="margin-left: 20px;" /><br />
										<label></label>
										<asp:RadioButton ID="rbShowTicketsNotExported" GroupName="ShowExported" runat="server" AutoPostBack="true"
											Text="Show tickets not exported" Checked="true" /><br />
										<label></label>
										<asp:RadioButton ID="rbIncludeTicketsWithoutErrors" GroupName="IncludeTickets" runat="server" AutoPostBack="true"
											Text="Show tickets without errors" Checked="true" Style="margin-left: 20px;"></asp:RadioButton><br />
										<label></label>
										<asp:CheckBox ID="cbxOnlyIncludeOrdersForThisInterface" runat="server" AutoPostBack="true"
											Text="Only include orders for this interface" Checked="true" Style="margin-left: 40px;" /><br />
										<label></label>
										<asp:RadioButton ID="rbIncludeTicketsWithError" GroupName="IncludeTickets" runat="server" AutoPostBack="true"
											Text="Show tickets with errors" Style="margin-left: 20px;"></asp:RadioButton><br />
										<label></label>
										<asp:RadioButton ID="rbIncludeTicketsWithIgnoredError" GroupName="IncludeTickets" runat="server" AutoPostBack="true"
											Text="Show tickets with ignored errors" Style="margin-left: 20px;"></asp:RadioButton><br />
									</li>
								</ul>
							</li>
							<li id="pnlContainerDisplayedColumns" runat="server">
								<div class="sectionEven">
									<ul>
										<li>
											<asp:CheckBox ID="cbxContainerNumberColumnShown" runat="server" Text="Number" />
										</li>
										<li>
											<asp:CheckBox ID="cbxContainerLocationColumnShown" runat="server" Text="Location" />
										</li>
										<li>
											<asp:CheckBox ID="cbxContainerStatusColumnShown" runat="server" Text="Status" />
										</li>
										<li>
											<asp:CheckBox ID="cbxContainerProductColumnShown" runat="server" Text="Product" />
										</li>
										<li>
											<asp:CheckBox ID="cbxContainerOwnerColumnShown" runat="server" Text="Owner" />
										</li>
										<li>
											<asp:CheckBox ID="cbxContainerAccountColumnShown" runat="server" Text="Account" />
										</li>
										<li>
											<asp:CheckBox ID="cbxContainerLastTransactionColumnShown" runat="server" Text="Last transaction" />
										</li>
										<li>
											<asp:CheckBox ID="cbxContainerEmptyWeightColumnShown" runat="server" Text="Empty weight" />
										</li>
										<li>
											<asp:CheckBox ID="cbxContainerVolumeColumnShown" runat="server" Text="Volume" />
										</li>
										<li>
											<asp:CheckBox ID="cbxContainerProductWeightColumnShown" runat="server" Text="Product weight" />
										</li>
										<li>
											<asp:CheckBox ID="cbxContainerInServiceColumnShown" runat="server" Text="In service" />
										</li>
										<li>
											<asp:CheckBox ID="cbxContainerLastFilledColumnShown" runat="server" Text="Last filled" />
										</li>
										<li>
											<asp:CheckBox ID="cbxContainerBulkProdEpaColumnShown" runat="server" Text="Bulk product EPA" />
										</li>
										<li>
											<asp:CheckBox ID="cbxContainerSealNumberColumnShown" runat="server" Text="Seal number" />
										</li>
										<li>
											<asp:CheckBox ID="cbxContainerTypeColumnShown" runat="server" Text="Type" />
										</li>
									</ul>
								</div>
								<div class="sectionOdd">
									<ul>
										<li>
											<asp:CheckBox ID="cbxContainerConditionColumnShown" runat="server" Text="Condition" />
										</li>
										<li>
											<asp:CheckBox ID="cbxContainerLastChangedColumnShown" runat="server" Text="Last changed" />
										</li>
										<li>
											<asp:CheckBox ID="cbxContainerManufacturedColumnShown" runat="server" Text="Manufactured" />
										</li>
										<li>
											<asp:CheckBox ID="cbxContainerLastInspectedColumnShown" runat="server" Text="Last inspected" />
										</li>
										<li>
											<asp:CheckBox ID="cbxContainerPassedInspectionColumnShown" runat="server" Text="Passed inspection" />
										</li>
										<li>
											<asp:CheckBox ID="cbxContainerRefillableColumnShown" runat="server" Text="Refillable" />
										</li>
										<li>
											<asp:CheckBox ID="cbxContainerLastCleanedColumnShown" runat="server" Text="Last cleaned" />
										</li>
										<li>
											<asp:CheckBox ID="cbxContainerNotesColumnShown" runat="server" Text="Notes" />
										</li>
										<li>
											<asp:CheckBox ID="cbxContainerSealBrokenColumnShown" runat="server" Text="Seal broken" />
										</li>
										<li>
											<asp:CheckBox ID="cbxContainerPassedPressureTestColumnShown" runat="server" Text="Passed pressure test" />
										</li>
										<li>
											<asp:CheckBox ID="cbxContainerOneWayValvePresentColumnShown" runat="server" Text="One-way valve present" />
										</li>
										<li>
											<asp:CheckBox ID="cbxContainerForOrderIdColumnShown" runat="server" Text="For order" />
										</li>
										<li>
											<asp:CheckBox ID="cbxContainerEquipmentColumnShown" runat="server" Text="Equipment" />
										</li>
										<li>
											<asp:CheckBox ID="cbxContainerLastUserIdColumnShown" runat="server" Text="Last user" />
										</li>
										<li>
											<asp:CheckBox ID="cbxContainerCreatedColumnShow" runat="server" Text="Created" />
										</li>
										<li>
											<asp:CheckBox ID="cbxContainerAssignedLotColumnShow" runat="server" Text="Lot" />
										</li>
									</ul>
								</div>
							</li>
						</ul>
					</div>
					<asp:Panel ID="pnlReportTriggers" runat="server" CssClass="sectionOdd" Enabled="false">
						<h1>Report scheduled run times</h1>
						<ul>
							<li class="addRemoveSection">
								<asp:ListBox ID="lstEmailSchedule" runat="server" CssClass="addRemoveList" AutoPostBack="true"></asp:ListBox>
								<asp:Button ID="btnAddSchedule" runat="server" Text="Add" CssClass="addRemoveButton" />
							</li>
							<li id="pnlEmailScheduleType" runat="server">
								<label>
								</label>
								<asp:RadioButtonList ID="rblEmailScheduleType" runat="server" RepeatLayout="UnorderedList" CssClass="input" AutoPostBack="true">
									<asp:ListItem Text="Send at specific time" Value="SendAtSpecificTime" Selected="True" />
									<asp:ListItem Text="Send on scheduled period" Value="SendOnScheduledPeriod" />
									<asp:ListItem Text="Send on specific date of month" Value="SendOnSpecificDateOfMonth" />
									<asp:ListItem Text="Send on specific day of month" Value="SendOnSpecificDayOfMonth" />
								</asp:RadioButtonList>
							</li>
							<li id="pnlTriggerSendTime" runat="server">
								<label>
									Send at
								</label>
								<span class="input">
									<input type="text" name="tbxTriggerSendTime" id="tbxTriggerSendTime" value="" runat="server" />
								</span></li>
							<li id="pnlReportRunScheduledInterval" runat="server">
								<label>
									Send every
								</label>
								<span class="input">
									<asp:TextBox ID="tbxReportRunScheduledInterval" runat="server" CssClass="inputNumeric"></asp:TextBox>
									<label style="text-align: left;">
										minutes
									</label>
								</span></li>
							<li id="pnlReportRunOnSpecificDateOfMonth" runat="server">
								<label>
									Send on the
								</label>
								<span class="input">
									<asp:DropDownList ID="ddlReportRunOnSpecificDateOfMonth" runat="server" Style="text-align: right;">
									</asp:DropDownList>
									<label style="text-align: left;">
										of the month
									</label>
								</span></li>
							<li id="pnlReportRunOnSpecificDayOfMonth" runat="server">
								<label>
									Send on the
								</label>
								<span class="input">
									<asp:DropDownList ID="ddlReportRunOnSpecificDayOfMonth" runat="server">
										<asp:ListItem Text="1st" Value="1" Selected="True" />
										<asp:ListItem Text="2nd" Value="2" />
										<asp:ListItem Text="3rd" Value="3" />
										<asp:ListItem Text="4th" Value="4" />
										<asp:ListItem Text="Last" Value="0" />
									</asp:DropDownList>
									<label style="text-align: left;">
										of the month
									</label>
								</span></li>
							<li id="pnlReportRunDays" runat="server">
								<label>
									Days of week
								</label>
								<asp:CheckBoxList ID="cblReportRunDays" runat="server" CssClass="input" RepeatLayout="UnorderedList">
								</asp:CheckBoxList>
							</li>
							<li>&nbsp;</li>
							<li id="pnlReportRunScheduledIntervalCatchesUpAfterOneRun" runat="server">
								<label>
									Run once to catch up
								</label>
								<asp:CheckBox ID="cbxReportRunScheduledIntervalCatchesUpAfterOneRun" runat="server" Text="" />
							</li>
							<li>&nbsp;</li>
							<li id="pnlReportTriggerDisabled" runat="server">
								<label>
									Disabled
								</label>
								<asp:CheckBox ID="cbxReportTriggerDisabled" runat="server" Text="" />
							</li>
						</ul>
						<asp:Button ID="btnUpdateSchedule" runat="server" Text="Update" Width="45%" />
						<asp:Button ID="btnRemoveSchedule" runat="server" Text="Remove" Width="45%" />
					</asp:Panel>
					<div id="tblCustomReportConfig" class="section" runat="server">
						<%--<hr style="width: 100%; color: #003399;" />--%>
						<iframe id="frmConfig" src="" runat="server" style="width: 100%; height: 800px;"></iframe>
						<label id="errorDetails" style="width: 100%; text-align: left;">
						</label>
					</div>
				</asp:Panel>
			</ContentTemplate>
		</asp:UpdatePanel>
	</form>
</body>
</html>
