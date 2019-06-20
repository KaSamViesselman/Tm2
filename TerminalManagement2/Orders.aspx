<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="Orders.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.Orders"
	MaintainScrollPositionOnPostback="true" %>

<!DOCTYPE html>
<html lang="en">
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
	<style type="text/css">
		tr:nth-child(odd) {
			background-color: white;
		}

		td, th {
			border: none;
		}

		.ProductNotes {
			width: 100% !important;
		}
	</style>
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
			document.getElementById('divContent').style.visibility = "hidden";
		}

		function onRequestDone(sender, args) {
			$find('PleaseWaitPopup').hide();
			document.getElementById('divContent').style.visibility = "visible";
			if (args.get_error() != undefined) {
				var errorMessage = args.get_error().message
				args.set_errorHandled(true);
				alert(errorMessage);
			}
		}
		function SetCalendarPickers() {
			$('#tbxExpirationDate').datetimepicker({
				timeFormat: 'h:mm:ss TT',
				showSecond: true,
				showOn: "both",
				buttonImage: 'Images/Calendar_scheduleHS.png',
				buttonImageOnly: true,
				buttonText: "Show calendar"
			});
		}
	</script>
</head>
<body>
	<form id="main" method="post" runat="server" defaultfocus="tbxOrderNum">
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
				<div id="divContent">
					<asp:Panel ID="pnlMain" runat="server" Visible="True">
						<div class="recordSelection">
							<ul>
								<li>
									<label>
										Facility</label>
									<asp:DropDownList ID="ddlFacilityFilter" runat="server" AutoPostBack="true">
									</asp:DropDownList>
								</li>
								<li>
									<label>
										Order</label>
									<asp:DropDownList ID="ddlOrders" runat="server" AutoPostBack="True" />
									<span class="findRecord">
										<asp:TextBox ID="tbxFind" runat="server" MaxLength="50"></asp:TextBox>
										<asp:Button ID="btnFind" runat="server" Text="Find" />
										&nbsp;<a href="AdvancedSearch.aspx?SearchType=Orders">Advanced Search</a>
									</span>
								</li>
							</ul>
						</div>
						<div class="recordControl">
							<asp:Button ID="btnSave" runat="server" Text="Save" />
							<asp:Button ID="btnSaveNew" runat="server" Text="Save & New" />
							<asp:Button ID="btnDelete" runat="server" Enabled="False" Text="Delete" />
							<asp:Label ID="lblStatus" runat="server" ForeColor="Red"></asp:Label>
							<span class="sectionRequiredField"><span class="required"></span>&nbsp;indicates required
				field </span>
						</div>
						<div class="section">
							<asp:Button ID="btnCopy" runat="server" Enabled="False" Text="Copy" Style="width: 18%; min-width: 120px;" />
							<asp:Button ID="btnPrintOrder" runat="server" Enabled="False" Text="Print" Style="width: 18%; min-width: 120px;" />
							<asp:Button ID="btnMarkComplete" runat="server" Enabled="False" Text="Mark Complete"
								Style="width: 18%; min-width: 120px;" />
							<asp:Button ID="btnPointOfSale" runat="server" Enabled="False" Text="Create Point of Sale"
								Style="width: 18%; min-width: 140px;" />
							<asp:Button ID="btnClearLockedStatus" runat="server" Enabled="False" Text="Clear Locked Status"
								Style="width: 18%; min-width: 140px;" />
							<asp:Button ID="btnSetLockedStatus" runat="server" Enabled="False" Text="Set Locked Status"
								Style="width: 18%; min-width: 140px;" />
						</div>
						<div class="section" id="pnlOrderDetailStatus" runat="server" visible="false">
							<asp:Label ID="lblOrderDetailStatus" runat="server" Text="" ForeColor="Red">   
							</asp:Label>
						</div>
						<asp:Panel ID="pnlAll" runat="server" CssClass="section">
							<asp:Panel ID="pnlEven" runat="server" CssClass="sectionEven">
								<h1>General</h1>
								<input type="hidden" id="tbxLastUpdated" runat="server" />
								<ul>
									<li>
										<label>
											Order number
										</label>
										<span class="required">
											<asp:TextBox ID="tbxOrderNum" runat="server" Enabled="False"></asp:TextBox>
										</span></li>
									<li>
										<label>
											PO number
										</label>
										<asp:TextBox ID="tbxPurchaseOrderNum" runat="server"></asp:TextBox>
									</li>
									<li id="pnlInterface" runat="server">
										<label>
											Interface
										</label>
										<asp:Label ID="lblInterfaceName" runat="server" CssClass="input"></asp:Label>
									</li>
									<li>
										<label>
											Batches
										</label>
										<span class="input">
											<asp:TextBox ID="tbxDeliveredBatches" runat="server" Width="30%" Style="text-align: right;">0</asp:TextBox>
											&nbsp;of&nbsp;
						<asp:TextBox ID="tbxRequestedBatches" runat="server" Width="30%" Style="text-align: right;">1</asp:TextBox>
										</span></li>
									<li>
										<label>
											Acres
										</label>
										<asp:TextBox ID="tbxAcres" runat="server" Style="text-align: right;">0</asp:TextBox>
									</li>
									<li>
										<label>
											Applicator
										</label>
										<asp:DropDownList ID="ddlApplicator" runat="server">
										</asp:DropDownList>
									</li>
								</ul>
							</asp:Panel>
							<asp:Panel ID="pnlOdd" runat="server" CssClass="sectionOdd">
								<ul>
									<li>
										<label>
											&nbsp;
										</label>
									</li>
									<li>
										<label>
											Release number
										</label>
										<asp:TextBox ID="tbxReleaseNumber" runat="server"></asp:TextBox>
									</li>
									<li>
										<label>
											Over scaling (%)
										</label>
										<asp:TextBox ID="tbxOverun" runat="server" Style="text-align: right;">0</asp:TextBox>
									</li>
									<li>
										<label>
											Branch
										</label>
										<asp:DropDownList ID="ddlBranches" runat="server">
										</asp:DropDownList>
									</li>
									<li>
										<label>
											Owner
										</label>
										<span class="required">
											<asp:DropDownList ID="ddlOwners" runat="server" AutoPostBack="True">
											</asp:DropDownList>
										</span></li>
									<li>
										<label>
										</label>
										<asp:CheckBox ID="cbxDoNotBlend" runat="server" Text="Do not blend" AutoPostBack="true" />
									</li>
									<li>
										<label>
											Transport type
										</label>
										<asp:DropDownList ID="ddlTransportType" runat="server">
										</asp:DropDownList>
									</li>
									<li>
										<label>
											Expiration date
										</label>
										<span class="input">
											<input type="text" name="tbxExpirationDate" id="tbxExpirationDate" value="" runat="server" />
										</span>
									</li>
								</ul>
							</asp:Panel>
							<div class="section">
								<ul>
									<li>
										<label style="width: 15%;">
											Ticket notes
										</label>
										<asp:TextBox ID="tbxNotes" runat="server" TextMode="MultiLine" Style="width: 75%;"></asp:TextBox>
									</li>
									<li>
										<label style="width: 15%;">
											Internal notes
										</label>
										<asp:TextBox ID="tbxInternalNotes" runat="server" TextMode="MultiLine" Style="width: 75%;"></asp:TextBox>
									</li>
								</ul>
							</div>
							<div class="sectionEven">
								<ul>
									<li>
										<label>
											Ship to
										</label>
										<asp:Panel ID="pnlCustomerSite" runat="server" CssClass="input">
											<asp:DropDownList ID="ddlCustomerSite" runat="server" AutoPostBack="true" Style="width: 100%;">
											</asp:DropDownList>
										</asp:Panel>
										<span class="input">
											<asp:Panel ID="pnlShipToInfo" runat="server" CssClass="addRemoveSection">
												<asp:TextBox ID="tbxShipTo" runat="server" Enabled="False" CssClass="addRemoveList"></asp:TextBox>
												<asp:Button ID="btnShipTo" runat="server" Text="Edit" CssClass="addRemoveButton"></asp:Button>
											</asp:Panel>
										</span></li>
									<li>
										<asp:Panel ID="pnlShipTo" runat="server" Visible="False">
											<hr style="color: #000080" />
											<ul>
												<li>
													<label>
														Ship to name
													</label>
													<span class="required">
														<asp:TextBox ID="tbxShipToName" runat="server"></asp:TextBox>
													</span></li>
												<li>
													<label>
														Ship to street
													</label>
													<asp:TextBox ID="tbxShipToStreet" runat="server"></asp:TextBox>
												</li>
												<li>
													<label>
														Ship to city
													</label>
													<asp:TextBox ID="tbxShipToCity" runat="server"></asp:TextBox>
												</li>
												<li>
													<label>
														Ship to state
													</label>
													<asp:TextBox ID="tbxShipToState" runat="server" Width="15%"></asp:TextBox>
												</li>
												<li>
													<label>
														Ship to zip
													</label>
													<asp:TextBox ID="tbxShipToZip" runat="server"></asp:TextBox>
												</li>
												<li>
													<label>
														Ship to country
													</label>
													<asp:TextBox ID="tbxShipToCountry" runat="server"></asp:TextBox>
												</li>
												<li>
													<label>
													</label>
													<span class="input">
														<asp:Button ID="btnShipToOk" runat="server" Text="OK" Style="width: 45%;"></asp:Button>
														<asp:Button ID="btnShipToCancel" runat="server" Text="Cancel" Style="width: 45%;"></asp:Button>
													</span></li>
											</ul>
										</asp:Panel>
									</li>
								</ul>
							</div>
							<div class="sectionOdd">
								<ul id="lstCustomFields" runat="server">
								</ul>
							</div>
							<div class="section">
								<hr style="color: #000080" />
							</div>
							<div class="section">
								<h2>Product(s) To Be Dispensed</h2>
								<table style="width: 100%;">
									<thead>
										<tr>
											<th></th>
											<th><span class="required">Product
											</span></th>
											<th></th>
											<th style="text-align: right;"><span class="required">Requested
											</span></th>
											<th style="text-align: right;">Delivered
											</th>
											<th>Unit
											</th>
											<th id="pnlGroupingHeader" runat="server">Grouping
											</th>
											<th></th>
											<th></th>
											<th>Notes
											</th>
										</tr>
									</thead>
									<tr id="pnlProduct1" runat="server" visible="True">
										<td style="width: 0px">
											<asp:Label ID="lblOrderItemId1" runat="server" Text="" Visible="false"></asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlProduct1" runat="server" Width="100%" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:Button ID="btnRemoveProduct1" runat="server" Width="100%" Text="Remove" Visible="True"></asp:Button>
										</td>
										<td>
											<asp:TextBox ID="tbxProductAmount1" runat="server" Width="100%" Style="text-align: right;">0</asp:TextBox>
										</td>
										<td style="text-align: right;">
											<asp:Label ID="lblDeliv1" runat="server" Style="min-width: 70px; text-align: right;">0</asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlUnits1" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:DropDownList ID="ddlOrderItemGrouping1" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True">
											</asp:DropDownList>
										</td>
										<td>&nbsp;
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductDown1" runat="server" ImageUrl="~/images/arrow-down-2.png"
												AlternateText="Move Down" CssClass="arrowDownImageButton" />
											&nbsp;
										</td>
										<td style="vertical-align: middle;">
											<asp:TextBox ID="tbxProductNotes1" runat="server" TextMode="MultiLine" class="ProductNotes"></asp:TextBox>
										</td>
									</tr>
									<tr id="pnlProduct2" runat="server" visible="False">
										<td style="width: 0px">
											<asp:Label ID="lblOrderItemId2" runat="server" Text="" Visible="false"></asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlProduct2" runat="server" Width="100%" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:Button ID="btnRemoveProduct2" runat="server" Width="100%" Text="Remove" Visible="True"></asp:Button>
										</td>
										<td>
											<asp:TextBox ID="tbxProductAmount2" runat="server" Width="100%" Style="text-align: right;">0</asp:TextBox>
										</td>
										<td style="text-align: right;">
											<asp:Label ID="lblDeliv2" runat="server" Style="min-width: 70px; text-align: right;">0</asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlUnits2" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:DropDownList ID="ddlOrderItemGrouping2" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductUp2" runat="server" ImageUrl="~/images/arrow-up-2.png"
												AlternateText="Move Up" CssClass="arrowUpImageButton" />
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductDown2" runat="server" ImageUrl="~/images/arrow-down-2.png"
												AlternateText="Move Down" CssClass="arrowDownImageButton" />
										</td>
										<td style="vertical-align: middle;">
											<asp:TextBox ID="tbxProductNotes2" runat="server" TextMode="MultiLine" class="ProductNotes"></asp:TextBox>
										</td>
									</tr>
									<tr id="pnlProduct3" runat="server" visible="False">
										<td style="width: 0px">
											<asp:Label ID="lblOrderItemId3" runat="server" Text="" Visible="false"></asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlProduct3" runat="server" Width="100%" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:Button ID="btnRemoveProduct3" runat="server" Width="100%" Text="Remove" Visible="True"></asp:Button>
										</td>
										<td>
											<asp:TextBox ID="tbxProductAmount3" runat="server" Width="100%" Style="text-align: right;">0</asp:TextBox>
										</td>
										<td style="text-align: right;">
											<asp:Label ID="lblDeliv3" runat="server" Style="min-width: 70px; text-align: right;">0</asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlUnits3" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:DropDownList ID="ddlOrderItemGrouping3" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductUp3" runat="server" ImageUrl="~/images/arrow-up-2.png"
												AlternateText="Move Up" CssClass="arrowUpImageButton" />
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductDown3" runat="server" ImageUrl="~/images/arrow-down-2.png"
												AlternateText="Move Down" CssClass="arrowDownImageButton" />
										</td>
										<td style="vertical-align: middle;">
											<asp:TextBox ID="tbxProductNotes3" runat="server" TextMode="MultiLine" class="ProductNotes"></asp:TextBox>
										</td>
									</tr>
									<tr id="pnlProduct4" runat="server" visible="False">
										<td style="width: 0px">
											<asp:Label ID="lblOrderItemId4" runat="server" Text="" Visible="false"></asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlProduct4" runat="server" Width="100%" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:Button ID="btnRemoveProduct4" runat="server" Width="100%" Text="Remove" Visible="True"></asp:Button>
										</td>
										<td>
											<asp:TextBox ID="tbxProductAmount4" runat="server" Width="100%" Style="text-align: right;">0</asp:TextBox>
										</td>
										<td style="text-align: right;">
											<asp:Label ID="lblDeliv4" runat="server" Style="min-width: 70px; text-align: right;">0</asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlUnits4" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:DropDownList ID="ddlOrderItemGrouping4" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductUp4" runat="server" ImageUrl="~/images/arrow-up-2.png"
												AlternateText="Move Up" CssClass="arrowUpImageButton" />
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductDown4" runat="server" ImageUrl="~/images/arrow-down-2.png"
												AlternateText="Move Down" CssClass="arrowDownImageButton" />
										</td>
										<td style="vertical-align: middle;">
											<asp:TextBox ID="tbxProductNotes4" runat="server" TextMode="MultiLine" class="ProductNotes"></asp:TextBox>
										</td>
									</tr>
									<tr id="pnlProduct5" runat="server" visible="False">
										<td style="width: 0px">
											<asp:Label ID="lblOrderItemId5" runat="server" Text="" Visible="false"></asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlProduct5" runat="server" Width="100%" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:Button ID="btnRemoveProduct5" runat="server" Width="100%" Text="Remove" Visible="True"></asp:Button>
										</td>
										<td>
											<asp:TextBox ID="tbxProductAmount5" runat="server" Width="100%" Style="text-align: right;">0</asp:TextBox>
										</td>
										<td style="text-align: right;">
											<asp:Label ID="lblDeliv5" runat="server" Style="min-width: 70px; text-align: right;">0</asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlUnits5" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:DropDownList ID="ddlOrderItemGrouping5" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductUp5" runat="server" ImageUrl="~/images/arrow-up-2.png"
												AlternateText="Move Up" CssClass="arrowUpImageButton" />
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductDown5" runat="server" ImageUrl="~/images/arrow-down-2.png"
												AlternateText="Move Down" CssClass="arrowDownImageButton" />
										</td>
										<td style="vertical-align: middle;">
											<asp:TextBox ID="tbxProductNotes5" runat="server" TextMode="MultiLine" class="ProductNotes"></asp:TextBox>
										</td>
									</tr>
									<tr id="pnlProduct6" runat="server" visible="False">
										<td style="width: 0px">
											<asp:Label ID="lblOrderItemId6" runat="server" Text="" Visible="false"></asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlProduct6" runat="server" Width="100%" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:Button ID="btnRemoveProduct6" runat="server" Width="100%" Text="Remove" Visible="True"></asp:Button>
										</td>
										<td>
											<asp:TextBox ID="tbxProductAmount6" runat="server" Width="100%" Style="text-align: right;">0</asp:TextBox>
										</td>
										<td style="text-align: right;">
											<asp:Label ID="lblDeliv6" runat="server" Style="min-width: 70px; text-align: right;">0</asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlUnits6" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:DropDownList ID="ddlOrderItemGrouping6" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductUp6" runat="server" ImageUrl="~/images/arrow-up-2.png"
												AlternateText="Move Up" CssClass="arrowUpImageButton" />
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductDown6" runat="server" ImageUrl="~/images/arrow-down-2.png"
												AlternateText="Move Down" CssClass="arrowDownImageButton" />
										</td>
										<td style="vertical-align: middle;">
											<asp:TextBox ID="tbxProductNotes6" runat="server" TextMode="MultiLine" class="ProductNotes"></asp:TextBox>
										</td>
									</tr>
									<tr id="pnlProduct7" runat="server" visible="False">
										<td style="width: 0px">
											<asp:Label ID="lblOrderItemId7" runat="server" Text="" Visible="false"></asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlProduct7" runat="server" Width="100%" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:Button ID="btnRemoveProduct7" runat="server" Width="100%" Text="Remove" Visible="True"></asp:Button>
										</td>
										<td>
											<asp:TextBox ID="tbxProductAmount7" runat="server" Width="100%" Style="text-align: right;">0</asp:TextBox>
										</td>
										<td style="text-align: right;">
											<asp:Label ID="lblDeliv7" runat="server" Style="min-width: 70px; text-align: right;">0</asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlUnits7" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:DropDownList ID="ddlOrderItemGrouping7" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductUp7" runat="server" ImageUrl="~/images/arrow-up-2.png"
												AlternateText="Move Up" CssClass="arrowUpImageButton" />
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductDown7" runat="server" ImageUrl="~/images/arrow-down-2.png"
												AlternateText="Move Down" CssClass="arrowDownImageButton" />
										</td>
										<td style="vertical-align: middle;">
											<asp:TextBox ID="tbxProductNotes7" runat="server" TextMode="MultiLine" class="ProductNotes"></asp:TextBox>
										</td>
									</tr>
									<tr id="pnlProduct8" runat="server" visible="False">
										<td style="width: 0px">
											<asp:Label ID="lblOrderItemId8" runat="server" Text="" Visible="false"></asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlProduct8" runat="server" Width="100%" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:Button ID="btnRemoveProduct8" runat="server" Width="100%" Text="Remove" Visible="True"></asp:Button>
										</td>
										<td>
											<asp:TextBox ID="tbxProductAmount8" runat="server" Width="100%" Style="text-align: right;">0</asp:TextBox>
										</td>
										<td style="text-align: right;">
											<asp:Label ID="lblDeliv8" runat="server" Style="min-width: 70px; text-align: right;">0</asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlUnits8" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:DropDownList ID="ddlOrderItemGrouping8" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductUp8" runat="server" ImageUrl="~/images/arrow-up-2.png"
												AlternateText="Move Up" CssClass="arrowUpImageButton" />
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductDown8" runat="server" ImageUrl="~/images/arrow-down-2.png"
												AlternateText="Move Down" CssClass="arrowDownImageButton" />
										</td>
										<td style="vertical-align: middle;">
											<asp:TextBox ID="tbxProductNotes8" runat="server" TextMode="MultiLine" class="ProductNotes"></asp:TextBox>
										</td>
									</tr>
									<tr id="pnlProduct9" runat="server" visible="False">
										<td style="width: 0px">
											<asp:Label ID="lblOrderItemId9" runat="server" Text="" Visible="false"></asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlProduct9" runat="server" Width="100%" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:Button ID="btnRemoveProduct9" runat="server" Width="100%" Text="Remove" Visible="True"></asp:Button>
										</td>
										<td>
											<asp:TextBox ID="tbxProductAmount9" runat="server" Width="100%" Style="text-align: right;">0</asp:TextBox>
										</td>
										<td style="text-align: right;">
											<asp:Label ID="lblDeliv9" runat="server" Style="min-width: 70px; text-align: right;">0</asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlUnits9" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:DropDownList ID="ddlOrderItemGrouping9" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductUp9" runat="server" ImageUrl="~/images/arrow-up-2.png"
												AlternateText="Move Up" CssClass="arrowUpImageButton" />
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductDown9" runat="server" ImageUrl="~/images/arrow-down-2.png"
												AlternateText="Move Down" CssClass="arrowDownImageButton" />
										</td>
										<td style="vertical-align: middle;">
											<asp:TextBox ID="tbxProductNotes9" runat="server" TextMode="MultiLine" class="ProductNotes"></asp:TextBox>
										</td>
									</tr>
									<tr id="pnlProduct10" runat="server" visible="False">
										<td style="width: 0px">
											<asp:Label ID="lblOrderItemId10" runat="server" Text="" Visible="false"></asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlProduct10" runat="server" Width="100%" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:Button ID="btnRemoveProduct10" runat="server" Width="100%" Text="Remove" Visible="True"></asp:Button>
										</td>
										<td>
											<asp:TextBox ID="tbxProductAmount10" runat="server" Width="100%" Style="text-align: right;">0</asp:TextBox>
										</td>
										<td style="text-align: right;">
											<asp:Label ID="lblDeliv10" runat="server" Style="min-width: 70px; text-align: right;">0</asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlUnits10" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:DropDownList ID="ddlOrderItemGrouping10" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductUp10" runat="server" ImageUrl="~/images/arrow-up-2.png"
												AlternateText="Move Up" CssClass="arrowUpImageButton" />
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductDown10" runat="server" ImageUrl="~/images/arrow-down-2.png"
												AlternateText="Move Down" CssClass="arrowDownImageButton" />
										</td>
										<td style="vertical-align: middle;">
											<asp:TextBox ID="tbxProductNotes10" runat="server" TextMode="MultiLine" class="ProductNotes"></asp:TextBox>
										</td>
									</tr>
									<tr id="pnlProduct11" runat="server" visible="False">
										<td style="width: 0px">
											<asp:Label ID="lblOrderItemId11" runat="server" Text="" Visible="false"></asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlProduct11" runat="server" Width="100%" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:Button ID="btnRemoveProduct11" runat="server" Width="100%" Text="Remove" Visible="True"></asp:Button>
										</td>
										<td>
											<asp:TextBox ID="tbxProductAmount11" runat="server" Width="100%" Style="text-align: right;">0</asp:TextBox>
										</td>
										<td style="text-align: right;">
											<asp:Label ID="lblDeliv11" runat="server" Style="min-width: 70px; text-align: right;">0</asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlUnits11" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:DropDownList ID="ddlOrderItemGrouping11" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductUp11" runat="server" ImageUrl="~/images/arrow-up-2.png"
												AlternateText="Move Up" CssClass="arrowUpImageButton" />
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductDown11" runat="server" ImageUrl="~/images/arrow-down-2.png"
												AlternateText="Move Down" CssClass="arrowDownImageButton" />
										</td>
										<td style="vertical-align: middle;">
											<asp:TextBox ID="tbxProductNotes11" runat="server" TextMode="MultiLine" class="ProductNotes"></asp:TextBox>
										</td>
									</tr>
									<tr id="pnlProduct12" runat="server" visible="False">
										<td style="width: 0px">
											<asp:Label ID="lblOrderItemId12" runat="server" Text="" Visible="false"></asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlProduct12" runat="server" Width="100%" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:Button ID="btnRemoveProduct12" runat="server" Width="100%" Text="Remove" Visible="True"></asp:Button>
										</td>
										<td>
											<asp:TextBox ID="tbxProductAmount12" runat="server" Width="100%" Style="text-align: right;">0</asp:TextBox>
										</td>
										<td style="text-align: right;">
											<asp:Label ID="lblDeliv12" runat="server" Style="min-width: 70px; text-align: right;">0</asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlUnits12" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:DropDownList ID="ddlOrderItemGrouping12" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductUp12" runat="server" ImageUrl="~/images/arrow-up-2.png"
												AlternateText="Move Up" CssClass="arrowUpImageButton" />
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductDown12" runat="server" ImageUrl="~/images/arrow-down-2.png"
												AlternateText="Move Down" CssClass="arrowDownImageButton" />
										</td>
										<td style="vertical-align: middle;">
											<asp:TextBox ID="tbxProductNotes12" runat="server" TextMode="MultiLine" class="ProductNotes"></asp:TextBox>
										</td>
									</tr>
									<tr id="pnlProduct13" runat="server" visible="False">
										<td style="width: 0px">
											<asp:Label ID="lblOrderItemId13" runat="server" Text="" Visible="false"></asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlProduct13" runat="server" Width="100%" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:Button ID="btnRemoveProduct13" runat="server" Width="100%" Text="Remove" Visible="True"></asp:Button>
										</td>
										<td>
											<asp:TextBox ID="tbxProductAmount13" runat="server" Width="100%" Style="text-align: right;">0</asp:TextBox>
										</td>
										<td style="text-align: right;">
											<asp:Label ID="lblDeliv13" runat="server" Style="min-width: 70px; text-align: right;">0</asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlUnits13" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:DropDownList ID="ddlOrderItemGrouping13" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductUp13" runat="server" ImageUrl="~/images/arrow-up-2.png"
												AlternateText="Move Up" CssClass="arrowUpImageButton" />
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductDown13" runat="server" ImageUrl="~/images/arrow-down-2.png"
												AlternateText="Move Down" CssClass="arrowDownImageButton" />
										</td>
										<td style="vertical-align: middle;">
											<asp:TextBox ID="tbxProductNotes13" runat="server" TextMode="MultiLine" class="ProductNotes"></asp:TextBox>
										</td>
									</tr>
									<tr id="pnlProduct14" runat="server" visible="False">
										<td style="width: 0px">
											<asp:Label ID="lblOrderItemId14" runat="server" Text="" Visible="false"></asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlProduct14" runat="server" Width="100%" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:Button ID="btnRemoveProduct14" runat="server" Width="100%" Text="Remove" Visible="True"></asp:Button>
										</td>
										<td>
											<asp:TextBox ID="tbxProductAmount14" runat="server" Width="100%" Style="text-align: right;">0</asp:TextBox>
										</td>
										<td style="text-align: right;">
											<asp:Label ID="lblDeliv14" runat="server" Style="min-width: 70px; text-align: right;">0</asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlUnits14" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:DropDownList ID="ddlOrderItemGrouping14" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductUp14" runat="server" ImageUrl="~/images/arrow-up-2.png"
												AlternateText="Move Up" CssClass="arrowUpImageButton" />
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductDown14" runat="server" ImageUrl="~/images/arrow-down-2.png"
												AlternateText="Move Down" CssClass="arrowDownImageButton" />
										</td>
										<td style="vertical-align: middle;">
											<asp:TextBox ID="tbxProductNotes14" runat="server" TextMode="MultiLine" class="ProductNotes"></asp:TextBox>
										</td>
									</tr>
									<tr id="pnlProduct15" runat="server" visible="False">
										<td style="width: 0px">
											<asp:Label ID="lblOrderItemId15" runat="server" Text="" Visible="false"></asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlProduct15" runat="server" Width="100%" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:Button ID="btnRemoveProduct15" runat="server" Width="100%" Text="Remove" Visible="True"></asp:Button>
										</td>
										<td>
											<asp:TextBox ID="tbxProductAmount15" runat="server" Width="100%" Style="text-align: right;">0</asp:TextBox>
										</td>
										<td style="text-align: right;">
											<asp:Label ID="lblDeliv15" runat="server" Style="min-width: 70px; text-align: right;">0</asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlUnits15" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:DropDownList ID="ddlOrderItemGrouping15" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductUp15" runat="server" ImageUrl="~/images/arrow-up-2.png"
												AlternateText="Move Up" CssClass="arrowUpImageButton" />
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductDown15" runat="server" ImageUrl="~/images/arrow-down-2.png"
												AlternateText="Move Down" CssClass="arrowDownImageButton" />
										</td>
										<td style="vertical-align: middle;">
											<asp:TextBox ID="tbxProductNotes15" runat="server" TextMode="MultiLine" class="ProductNotes"></asp:TextBox>
										</td>
									</tr>
									<tr id="pnlProduct16" runat="server" visible="False">
										<td style="width: 0px">
											<asp:Label ID="lblOrderItemId16" runat="server" Text="" Visible="false"></asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlProduct16" runat="server" Width="100%" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:Button ID="btnRemoveProduct16" runat="server" Width="100%" Text="Remove" Visible="True"></asp:Button>
										</td>
										<td>
											<asp:TextBox ID="tbxProductAmount16" runat="server" Width="100%" Style="text-align: right;">0</asp:TextBox>
										</td>
										<td style="text-align: right;">
											<asp:Label ID="lblDeliv16" runat="server" Style="min-width: 70px; text-align: right;">0</asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlUnits16" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:DropDownList ID="ddlOrderItemGrouping16" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductUp16" runat="server" ImageUrl="~/images/arrow-up-2.png"
												AlternateText="Move Up" CssClass="arrowUpImageButton" />
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductDown16" runat="server" ImageUrl="~/images/arrow-down-2.png"
												AlternateText="Move Down" CssClass="arrowDownImageButton" />
										</td>
										<td style="vertical-align: middle;">
											<asp:TextBox ID="tbxProductNotes16" runat="server" TextMode="MultiLine" class="ProductNotes"></asp:TextBox>
										</td>
									</tr>
									<tr id="pnlProduct17" runat="server" visible="False">
										<td style="width: 0px">
											<asp:Label ID="lblOrderItemId17" runat="server" Text="" Visible="false"></asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlProduct17" runat="server" Width="100%" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:Button ID="btnRemoveProduct17" runat="server" Width="100%" Text="Remove" Visible="True"></asp:Button>
										</td>
										<td>
											<asp:TextBox ID="tbxProductAmount17" runat="server" Width="100%" Style="text-align: right;">0</asp:TextBox>
										</td>
										<td style="text-align: right;">
											<asp:Label ID="lblDeliv17" runat="server" Style="min-width: 70px; text-align: right;">0</asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlUnits17" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:DropDownList ID="ddlOrderItemGrouping17" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductUp17" runat="server" ImageUrl="~/images/arrow-up-2.png"
												AlternateText="Move Up" CssClass="arrowUpImageButton" />
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductDown17" runat="server" ImageUrl="~/images/arrow-down-2.png"
												AlternateText="Move Down" CssClass="arrowDownImageButton" />
										</td>
										<td style="vertical-align: middle;">
											<asp:TextBox ID="tbxProductNotes17" runat="server" TextMode="MultiLine" class="ProductNotes"></asp:TextBox>
										</td>
									</tr>
									<tr id="pnlProduct18" runat="server" visible="False">
										<td style="width: 0px">
											<asp:Label ID="lblOrderItemId18" runat="server" Text="" Visible="false"></asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlProduct18" runat="server" Width="100%" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:Button ID="btnRemoveProduct18" runat="server" Width="100%" Text="Remove" Visible="True"></asp:Button>
										</td>
										<td>
											<asp:TextBox ID="tbxProductAmount18" runat="server" Width="100%" Style="text-align: right;">0</asp:TextBox>
										</td>
										<td style="text-align: right;">
											<asp:Label ID="lblDeliv18" runat="server" Style="min-width: 70px; text-align: right;">0</asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlUnits18" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:DropDownList ID="ddlOrderItemGrouping18" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductUp18" runat="server" ImageUrl="~/images/arrow-up-2.png"
												AlternateText="Move Up" CssClass="arrowUpImageButton" />
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductDown18" runat="server" ImageUrl="~/images/arrow-down-2.png"
												AlternateText="Move Down" CssClass="arrowDownImageButton" />
										</td>
										<td style="vertical-align: middle;">
											<asp:TextBox ID="tbxProductNotes18" runat="server" TextMode="MultiLine" class="ProductNotes"></asp:TextBox>
										</td>
									</tr>
									<tr id="pnlProduct19" runat="server" visible="False">
										<td style="width: 0px">
											<asp:Label ID="lblOrderItemId19" runat="server" Text="" Visible="false"></asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlProduct19" runat="server" Width="100%" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:Button ID="btnRemoveProduct19" runat="server" Width="100%" Text="Remove" Visible="True"></asp:Button>
										</td>
										<td>
											<asp:TextBox ID="tbxProductAmount19" runat="server" Width="100%" Style="text-align: right;">0</asp:TextBox>
										</td>
										<td style="text-align: right;">
											<asp:Label ID="lblDeliv19" runat="server" Style="min-width: 70px; text-align: right;">0</asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlUnits19" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:DropDownList ID="ddlOrderItemGrouping19" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductUp19" runat="server" ImageUrl="~/images/arrow-up-2.png"
												AlternateText="Move Up" CssClass="arrowUpImageButton" />
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductDown19" runat="server" ImageUrl="~/images/arrow-down-2.png"
												AlternateText="Move Down" CssClass="arrowDownImageButton" />
										</td>
										<td style="vertical-align: middle;">
											<asp:TextBox ID="tbxProductNotes19" runat="server" TextMode="MultiLine" class="ProductNotes"></asp:TextBox>
										</td>
									</tr>
									<tr id="pnlProduct20" runat="server" visible="False">
										<td style="width: 0px">
											<asp:Label ID="lblOrderItemId20" runat="server" Text="" Visible="false"></asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlProduct20" runat="server" Width="100%" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:Button ID="btnRemoveProduct20" runat="server" Width="100%" Text="Remove" Visible="True"></asp:Button>
										</td>
										<td>
											<asp:TextBox ID="tbxProductAmount20" runat="server" Width="100%" Style="text-align: right;">0</asp:TextBox>
										</td>
										<td style="text-align: right;">
											<asp:Label ID="lblDeliv20" runat="server" Style="min-width: 70px; text-align: right;">0</asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlUnits20" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:DropDownList ID="ddlOrderItemGrouping20" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductUp20" runat="server" ImageUrl="~/images/arrow-up-2.png"
												AlternateText="Move Up" CssClass="arrowUpImageButton" />
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductDown20" runat="server" ImageUrl="~/images/arrow-down-2.png"
												AlternateText="Move Down" CssClass="arrowDownImageButton" />
										</td>
										<td style="vertical-align: middle;">
											<asp:TextBox ID="tbxProductNotes20" runat="server" TextMode="MultiLine" class="ProductNotes"></asp:TextBox>
										</td>
									</tr>
									<tr id="pnlProduct21" runat="server" visible="False">
										<td style="width: 0px">
											<asp:Label ID="lblOrderItemId21" runat="server" Text="" Visible="false"></asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlProduct21" runat="server" Width="100%" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:Button ID="btnRemoveProduct21" runat="server" Width="100%" Text="Remove" Visible="True"></asp:Button>
										</td>
										<td>
											<asp:TextBox ID="tbxProductAmount21" runat="server" Width="100%" Style="text-align: right;">0</asp:TextBox>
										</td>
										<td style="text-align: right;">
											<asp:Label ID="lblDeliv21" runat="server" Style="min-width: 70px; text-align: right;">0</asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlUnits21" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:DropDownList ID="ddlOrderItemGrouping21" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductUp21" runat="server" ImageUrl="~/images/arrow-up-2.png"
												AlternateText="Move Up" CssClass="arrowUpImageButton" />
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductDown21" runat="server" ImageUrl="~/images/arrow-down-2.png"
												AlternateText="Move Down" CssClass="arrowDownImageButton" />
										</td>
										<td style="vertical-align: middle;">
											<asp:TextBox ID="tbxProductNotes21" runat="server" TextMode="MultiLine" class="ProductNotes"></asp:TextBox>
										</td>
									</tr>
									<tr id="pnlProduct22" runat="server" visible="False">
										<td style="width: 0px">
											<asp:Label ID="lblOrderItemId22" runat="server" Text="" Visible="false"></asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlProduct22" runat="server" Width="100%" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:Button ID="btnRemoveProduct22" runat="server" Width="100%" Text="Remove" Visible="True"></asp:Button>
										</td>
										<td>
											<asp:TextBox ID="tbxProductAmount22" runat="server" Width="100%" Style="text-align: right;">0</asp:TextBox>
										</td>
										<td style="text-align: right;">
											<asp:Label ID="lblDeliv22" runat="server" Style="min-width: 70px; text-align: right;">0</asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlUnits22" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:DropDownList ID="ddlOrderItemGrouping22" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductUp22" runat="server" ImageUrl="~/images/arrow-up-2.png"
												AlternateText="Move Up" CssClass="arrowUpImageButton" />
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductDown22" runat="server" ImageUrl="~/images/arrow-down-2.png"
												AlternateText="Move Down" CssClass="arrowDownImageButton" />
										</td>
										<td style="vertical-align: middle;">
											<asp:TextBox ID="tbxProductNotes22" runat="server" TextMode="MultiLine" class="ProductNotes"></asp:TextBox>
										</td>
									</tr>
									<tr id="pnlProduct23" runat="server" visible="False">
										<td style="width: 0px">
											<asp:Label ID="lblOrderItemId23" runat="server" Text="" Visible="false"></asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlProduct23" runat="server" Width="100%" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:Button ID="btnRemoveProduct23" runat="server" Width="100%" Text="Remove" Visible="True"></asp:Button>
										</td>
										<td>
											<asp:TextBox ID="tbxProductAmount23" runat="server" Width="100%" Style="text-align: right;">0</asp:TextBox>
										</td>
										<td style="text-align: right;">
											<asp:Label ID="lblDeliv23" runat="server" Style="min-width: 70px; text-align: right;">0</asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlUnits23" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:DropDownList ID="ddlOrderItemGrouping23" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductUp23" runat="server" ImageUrl="~/images/arrow-up-2.png"
												AlternateText="Move Up" CssClass="arrowUpImageButton" />
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductDown23" runat="server" ImageUrl="~/images/arrow-down-2.png"
												AlternateText="Move Down" CssClass="arrowDownImageButton" />
										</td>
										<td style="vertical-align: middle;">
											<asp:TextBox ID="tbxProductNotes23" runat="server" TextMode="MultiLine" class="ProductNotes"></asp:TextBox>
										</td>
									</tr>
									<tr id="pnlProduct24" runat="server" visible="False">
										<td style="width: 0px">
											<asp:Label ID="lblOrderItemId24" runat="server" Text="" Visible="false"></asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlProduct24" runat="server" Width="100%" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:Button ID="btnRemoveProduct24" runat="server" Width="100%" Text="Remove" Visible="True"></asp:Button>
										</td>
										<td>
											<asp:TextBox ID="tbxProductAmount24" runat="server" Width="100%" Style="text-align: right;">0</asp:TextBox>
										</td>
										<td style="text-align: right;">
											<asp:Label ID="lblDeliv24" runat="server" Style="min-width: 70px; text-align: right;">0</asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlUnits24" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:DropDownList ID="ddlOrderItemGrouping24" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductUp24" runat="server" ImageUrl="~/images/arrow-up-2.png"
												AlternateText="Move Up" CssClass="arrowUpImageButton" />
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductDown24" runat="server" ImageUrl="~/images/arrow-down-2.png"
												AlternateText="Move Down" CssClass="arrowDownImageButton" />
										</td>
										<td style="vertical-align: middle;">
											<asp:TextBox ID="tbxProductNotes24" runat="server" TextMode="MultiLine" class="ProductNotes"></asp:TextBox>
										</td>
									</tr>
									<tr id="pnlProduct25" runat="server" visible="False">
										<td style="width: 0px">
											<asp:Label ID="lblOrderItemId25" runat="server" Text="" Visible="false"></asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlProduct25" runat="server" Width="100%" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:Button ID="btnRemoveProduct25" runat="server" Width="100%" Text="Remove" Visible="True"></asp:Button>
										</td>
										<td>
											<asp:TextBox ID="tbxProductAmount25" runat="server" Width="100%" Style="text-align: right;">0</asp:TextBox>
										</td>
										<td style="text-align: right;">
											<asp:Label ID="lblDeliv25" runat="server" Style="min-width: 70px; text-align: right;">0</asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlUnits25" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:DropDownList ID="ddlOrderItemGrouping25" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductUp25" runat="server" ImageUrl="~/images/arrow-up-2.png"
												AlternateText="Move Up" CssClass="arrowUpImageButton" />
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductDown25" runat="server" ImageUrl="~/images/arrow-down-2.png"
												AlternateText="Move Down" CssClass="arrowDownImageButton" />
										</td>
										<td style="vertical-align: middle;">
											<asp:TextBox ID="tbxProductNotes25" runat="server" TextMode="MultiLine" class="ProductNotes"></asp:TextBox>
										</td>
									</tr>
									<tr id="pnlProduct26" runat="server" visible="False">
										<td style="width: 0px">
											<asp:Label ID="lblOrderItemId26" runat="server" Text="" Visible="false"></asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlProduct26" runat="server" Width="100%" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:Button ID="btnRemoveProduct26" runat="server" Width="100%" Text="Remove" Visible="True"></asp:Button>
										</td>
										<td>
											<asp:TextBox ID="tbxProductAmount26" runat="server" Width="100%" Style="text-align: right;">0</asp:TextBox>
										</td>
										<td style="text-align: right;">
											<asp:Label ID="lblDeliv26" runat="server" Style="min-width: 70px; text-align: right;">0</asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlUnits26" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:DropDownList ID="ddlOrderItemGrouping26" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductUp26" runat="server" ImageUrl="~/images/arrow-up-2.png"
												AlternateText="Move Up" CssClass="arrowUpImageButton" />
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductDown26" runat="server" ImageUrl="~/images/arrow-down-2.png"
												AlternateText="Move Down" CssClass="arrowDownImageButton" />
										</td>
										<td style="vertical-align: middle;">
											<asp:TextBox ID="tbxProductNotes26" runat="server" TextMode="MultiLine" class="ProductNotes"></asp:TextBox>
										</td>
									</tr>
									<tr id="pnlProduct27" runat="server" visible="False">
										<td style="width: 0px">
											<asp:Label ID="lblOrderItemId27" runat="server" Text="" Visible="false"></asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlProduct27" runat="server" Width="100%" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:Button ID="btnRemoveProduct27" runat="server" Width="100%" Text="Remove" Visible="True"></asp:Button>
										</td>
										<td>
											<asp:TextBox ID="tbxProductAmount27" runat="server" Width="100%" Style="text-align: right;">0</asp:TextBox>
										</td>
										<td style="text-align: right;">
											<asp:Label ID="lblDeliv27" runat="server" Style="min-width: 70px; text-align: right;">0</asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlUnits27" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:DropDownList ID="ddlOrderItemGrouping27" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductUp27" runat="server" ImageUrl="~/images/arrow-up-2.png"
												AlternateText="Move Up" CssClass="arrowUpImageButton" />
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductDown27" runat="server" ImageUrl="~/images/arrow-down-2.png"
												AlternateText="Move Down" CssClass="arrowDownImageButton" />
										</td>
										<td style="vertical-align: middle;">
											<asp:TextBox ID="tbxProductNotes27" runat="server" TextMode="MultiLine" class="ProductNotes"></asp:TextBox>
										</td>
									</tr>
									<tr id="pnlProduct28" runat="server" visible="False">
										<td style="width: 0px">
											<asp:Label ID="lblOrderItemId28" runat="server" Text="" Visible="false"></asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlProduct28" runat="server" Width="100%" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:Button ID="btnRemoveProduct28" runat="server" Width="100%" Text="Remove" Visible="True"></asp:Button>
										</td>
										<td>
											<asp:TextBox ID="tbxProductAmount28" runat="server" Width="100%" Style="text-align: right;">0</asp:TextBox>
										</td>
										<td style="text-align: right;">
											<asp:Label ID="lblDeliv28" runat="server" Style="min-width: 70px; text-align: right;">0</asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlUnits28" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:DropDownList ID="ddlOrderItemGrouping28" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductUp28" runat="server" ImageUrl="~/images/arrow-up-2.png"
												AlternateText="Move Up" CssClass="arrowUpImageButton" />
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductDown28" runat="server" ImageUrl="~/images/arrow-down-2.png"
												AlternateText="Move Down" CssClass="arrowDownImageButton" />
										</td>
										<td style="vertical-align: middle;">
											<asp:TextBox ID="tbxProductNotes28" runat="server" TextMode="MultiLine" class="ProductNotes"></asp:TextBox>
										</td>
									</tr>
									<tr id="pnlProduct29" runat="server" visible="False">
										<td style="width: 0px">
											<asp:Label ID="lblOrderItemId29" runat="server" Text="" Visible="false"></asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlProduct29" runat="server" Width="100%" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:Button ID="btnRemoveProduct29" runat="server" Width="100%" Text="Remove" Visible="True"></asp:Button>
										</td>
										<td>
											<asp:TextBox ID="tbxProductAmount29" runat="server" Width="100%" Style="text-align: right;">0</asp:TextBox>
										</td>
										<td style="text-align: right;">
											<asp:Label ID="lblDeliv29" runat="server" Style="min-width: 70px; text-align: right;">0</asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlUnits29" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:DropDownList ID="ddlOrderItemGrouping29" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductUp29" runat="server" ImageUrl="~/images/arrow-up-2.png"
												AlternateText="Move Up" CssClass="arrowUpImageButton" />
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductDown29" runat="server" ImageUrl="~/images/arrow-down-2.png"
												AlternateText="Move Down" CssClass="arrowDownImageButton" />
										</td>
										<td style="vertical-align: middle;">
											<asp:TextBox ID="tbxProductNotes29" runat="server" TextMode="MultiLine" class="ProductNotes"></asp:TextBox>
										</td>
									</tr>
									<tr id="pnlProduct30" runat="server" visible="False">
										<td style="width: 0px">
											<asp:Label ID="lblOrderItemId30" runat="server" Text="" Visible="false"></asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlProduct30" runat="server" Width="100%" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:Button ID="btnRemoveProduct30" runat="server" Width="100%" Text="Remove" Visible="True"></asp:Button>
										</td>
										<td>
											<asp:TextBox ID="tbxProductAmount30" runat="server" Width="100%" Style="text-align: right;">0</asp:TextBox>
										</td>
										<td style="text-align: right;">
											<asp:Label ID="lblDeliv30" runat="server" Style="min-width: 70px; text-align: right;">0</asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlUnits30" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:DropDownList ID="ddlOrderItemGrouping30" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductUp30" runat="server" ImageUrl="~/images/arrow-up-2.png"
												AlternateText="Move Up" CssClass="arrowUpImageButton" />
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductDown30" runat="server" ImageUrl="~/images/arrow-down-2.png"
												AlternateText="Move Down" CssClass="arrowDownImageButton" />
										</td>
										<td style="vertical-align: middle;">
											<asp:TextBox ID="tbxProductNotes30" runat="server" TextMode="MultiLine" class="ProductNotes"></asp:TextBox>
										</td>
									</tr>
									<tr id="pnlProduct31" runat="server" visible="False">
										<td style="width: 0px">
											<asp:Label ID="lblOrderItemId31" runat="server" Text="" Visible="false"></asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlProduct31" runat="server" Width="100%" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:Button ID="btnRemoveProduct31" runat="server" Width="100%" Text="Remove" Visible="True"></asp:Button>
										</td>
										<td>
											<asp:TextBox ID="tbxProductAmount31" runat="server" Width="100%" Style="text-align: right;">0</asp:TextBox>
										</td>
										<td style="text-align: right;">
											<asp:Label ID="lblDeliv31" runat="server" Style="min-width: 70px; text-align: right;">0</asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlUnits31" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:DropDownList ID="ddlOrderItemGrouping31" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductUp31" runat="server" ImageUrl="~/images/arrow-up-2.png"
												AlternateText="Move Up" CssClass="arrowUpImageButton" />
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductDown31" runat="server" ImageUrl="~/images/arrow-down-2.png"
												AlternateText="Move Down" CssClass="arrowDownImageButton" />
										</td>
										<td style="vertical-align: middle;">
											<asp:TextBox ID="tbxProductNotes31" runat="server" TextMode="MultiLine" class="ProductNotes"></asp:TextBox>
										</td>
									</tr>
									<tr id="pnlProduct32" runat="server" visible="False">
										<td style="width: 0px">
											<asp:Label ID="lblOrderItemId32" runat="server" Text="" Visible="false"></asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlProduct32" runat="server" Width="100%" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:Button ID="btnRemoveProduct32" runat="server" Width="100%" Text="Remove" Visible="True"></asp:Button>
										</td>
										<td>
											<asp:TextBox ID="tbxProductAmount32" runat="server" Width="100%" Style="text-align: right;">0</asp:TextBox>
										</td>
										<td style="text-align: right;">
											<asp:Label ID="lblDeliv32" runat="server" Style="min-width: 70px; text-align: right;">0</asp:Label>
										</td>
										<td>
											<asp:DropDownList ID="ddlUnits32" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True" AutoPostBack="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:DropDownList ID="ddlOrderItemGrouping32" runat="server" Style="width: auto; min-width: 75px;"
												Visible="True">
											</asp:DropDownList>
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductUp32" runat="server" ImageUrl="~/images/arrow-up-2.png"
												AlternateText="Move Up" CssClass="arrowUpImageButton" />
										</td>
										<td>
											<asp:ImageButton ID="btnMoveProductDown32" runat="server" ImageUrl="~/images/arrow-down-2.png"
												AlternateText="Move Down" CssClass="arrowDownImageButton" />
										</td>
										<td style="vertical-align: middle;">
											<asp:TextBox ID="tbxProductNotes32" runat="server" TextMode="MultiLine" class="ProductNotes"></asp:TextBox>
										</td>
									</tr>
								</table>
							</div>
							<div class="section">
								<div class="sectionEven">
									<asp:Button ID="btnAddProduct" runat="server" Width="150px" Text="Add Another Product"></asp:Button>
									<asp:Button ID="btnSortUsingProductPriority" runat="server" Text="Product Priority Sort"
										Visible="false" Width="150px" />
								</div>
								<div id="pnlAddOrderItemGrouping" runat="server" class="sectionOdd" style="border: 1px solid gray;">
									<ul class="addRemoveSection">
										<li>
											<h3>Grouping name
											</h3>
										</li>
										<li>
											<asp:TextBox ID="tbxOrderItemGroupingName" runat="server" CssClass="addRemoveList"></asp:TextBox>
											<asp:Button ID="btnAddOrderItemGrouping" runat="server" Text="Add grouping" class="addRemoveButton" />
										</li>
									</ul>
								</div>
							</div>
							<%-- Begin of new account section --%>
							<br />
							<hr style="color: #000080" />
							<div class="section">
								<h2>Account(s) To Be Billed</h2>
								<asp:CheckBox ID="cbxAccountCoupling" runat="server" AutoPostBack="True" Text="Use account coupling"
									Style="width: auto;" />
								<ul>
									<li id="pnlAccount1" runat="server">
										<%-- ===== 1st account row ===== --%>
										<span class="required">
											<asp:DropDownList ID="ddlAccount1" runat="server" AutoPostBack="true" Style="max-width: 300px;">
											</asp:DropDownList></span>
										<span style="white-space: nowrap; display: inline-block;">
											<label style="width: auto;">
												Percent</label>
											<asp:TextBox ID="tbxPercent1" runat="server" CssClass="inputNumeric" Style="width: auto; min-width: 15%;"
												Text="100"></asp:TextBox>
										</span>
										<asp:Button ID="btnRemoveAccount1" runat="server" Style="width: auto;" Text="Remove" />
									</li>
									<li id="pnlAccount2" runat="server">
										<%-- ===== 2nd account row ===== --%>
										<asp:DropDownList ID="ddlAccount2" runat="server" AutoPostBack="true" Style="max-width: 300px;">
										</asp:DropDownList>
										<span style="white-space: nowrap; display: inline-block;">
											<label style="width: auto;">
												Percent</label>
											<asp:TextBox ID="tbxPercent2" runat="server" CssClass="inputNumeric" Style="width: auto; min-width: 15%;"
												Text="100"></asp:TextBox>
										</span>
										<asp:Button ID="btnRemoveAccount2" runat="server" Style="width: auto;" Text="Remove" />
									</li>
									<li id="pnlAccount3" runat="server">
										<%-- ===== 3rd account row ===== --%>
										<asp:DropDownList ID="ddlAccount3" runat="server" AutoPostBack="true" Style="max-width: 300px;">
										</asp:DropDownList>
										<span style="white-space: nowrap; display: inline-block;">
											<label style="width: auto;">
												Percent</label>
											<asp:TextBox ID="tbxPercent3" runat="server" CssClass="inputNumeric" Style="width: auto; min-width: 15%;"
												Text="100"></asp:TextBox>
										</span>
										<asp:Button ID="btnRemoveAccount3" runat="server" Style="width: auto;" Text="Remove" />
									</li>
									<li id="pnlAccount4" runat="server" visible="False">
										<%-- ===== 4th account row ===== --%>
										<asp:DropDownList ID="ddlAccount4" runat="server" AutoPostBack="true" Style="max-width: 300px;">
										</asp:DropDownList>
										<span style="white-space: nowrap; display: inline-block;">
											<label style="width: auto;">
												Percent</label>
											<asp:TextBox ID="tbxPercent4" runat="server" CssClass="inputNumeric" Style="width: auto; min-width: 15%;"
												Text="100"></asp:TextBox>
										</span>
										<asp:Button ID="btnRemoveAccount4" runat="server" Style="width: auto;" Text="Remove" />
									</li>
									<li id="pnlAccount5" runat="server" visible="False">
										<%-- ===== 5th account row ===== --%>
										<asp:DropDownList ID="ddlAccount5" runat="server" AutoPostBack="true" Style="max-width: 300px;">
										</asp:DropDownList>
										<span style="white-space: nowrap; display: inline-block;">
											<label style="width: auto;">
												Percent</label>
											<asp:TextBox ID="tbxPercent5" runat="server" CssClass="inputNumeric" Style="width: auto; min-width: 15%;"
												Text="100"></asp:TextBox>
										</span>
										<asp:Button ID="btnRemoveAccount5" runat="server" Style="width: auto;" Text="Remove" />
									</li>
									<li id="pnlAccount6" runat="server" visible="False">
										<%-- ===== 6th account row ===== --%>
										<asp:DropDownList ID="ddlAccount6" runat="server" AutoPostBack="true" Style="max-width: 300px;">
										</asp:DropDownList>
										<span style="white-space: nowrap; display: inline-block;">
											<label style="width: auto;">
												Percent</label>
											<asp:TextBox ID="tbxPercent6" runat="server" CssClass="inputNumeric" Style="width: auto; min-width: 15%;"
												Text="100"></asp:TextBox>
										</span>
										<asp:Button ID="btnRemoveAccount6" runat="server" Style="width: auto;" Text="Remove" />
									</li>
									<li id="pnlAccount7" runat="server" visible="False">
										<%-- ===== 7th account row ===== --%>
										<asp:DropDownList ID="ddlAccount7" runat="server" AutoPostBack="true" Style="max-width: 300px;">
										</asp:DropDownList>
										<span style="white-space: nowrap; display: inline-block;">
											<label style="width: auto;">
												Percent</label>
											<asp:TextBox ID="tbxPercent7" runat="server" CssClass="inputNumeric" Style="width: auto; min-width: 15%;"
												Text="100"></asp:TextBox>
										</span>
										<asp:Button ID="btnRemoveAccount7" runat="server" Style="width: auto;" Text="Remove" />
									</li>
									<li id="pnlAccount8" runat="server" visible="False">
										<%-- ===== 8th account row ===== --%>
										<asp:DropDownList ID="ddlAccount8" runat="server" AutoPostBack="true" Style="max-width: 300px;">
										</asp:DropDownList>
										<span style="white-space: nowrap; display: inline-block;">
											<label style="width: auto;">
												Percent</label>
											<asp:TextBox ID="tbxPercent8" runat="server" CssClass="inputNumeric" Style="width: auto; min-width: 15%;"
												Text="100"></asp:TextBox>
										</span>
										<asp:Button ID="btnRemoveAccount8" runat="server" Style="width: auto;" Text="Remove" />
									</li>
									<li id="pnlAccount9" runat="server" visible="False">
										<%-- ===== 9th account row ===== --%>
										<asp:DropDownList ID="ddlAccount9" runat="server" AutoPostBack="true" Style="max-width: 300px;">
										</asp:DropDownList>
										<span style="white-space: nowrap; display: inline-block;">
											<label style="width: auto;">
												Percent</label>
											<asp:TextBox ID="tbxPercent9" runat="server" CssClass="inputNumeric" Style="width: auto; min-width: 15%;"
												Text="100"></asp:TextBox>
										</span>
										<asp:Button ID="btnRemoveAccount9" runat="server" Style="width: auto;" Text="Remove" />
									</li>
									<li id="pnlAccount10" runat="server" visible="False">
										<%-- ===== 10th account row =====--%>
										<asp:DropDownList ID="ddlAccount10" runat="server" AutoPostBack="true" Style="max-width: 300px;">
										</asp:DropDownList>
										<span style="white-space: nowrap; display: inline-block;">
											<label style="width: auto;">
												Percent</label>
											<asp:TextBox ID="tbxPercent10" runat="server" CssClass="inputNumeric" Style="width: auto; min-width: 15%;"
												Text="100"></asp:TextBox>
										</span>
										<asp:Button ID="btnRemoveAccount10" runat="server" Style="width: auto;" Text="Remove" />
									</li>
									<li id="pnlAccount11" runat="server" visible="False">
										<%-- ===== 11th account row ===== --%>
										<asp:DropDownList ID="ddlAccount11" runat="server" AutoPostBack="true" Style="max-width: 300px;">
										</asp:DropDownList>
										<span style="white-space: nowrap; display: inline-block;">
											<label style="width: auto;">
												Percent</label>
											<asp:TextBox ID="tbxPercent11" runat="server" CssClass="inputNumeric" Style="width: auto; min-width: 15%;"
												Text="100"></asp:TextBox>
										</span>
										<asp:Button ID="btnRemoveAccount11" runat="server" Style="width: auto;" Text="Remove" />
									</li>
									<li id="pnlAccount12" runat="server" visible="False">
										<%-- ===== 12th account row ===== --%>
										<asp:DropDownList ID="ddlAccount12" runat="server" AutoPostBack="true" Style="max-width: 300px;">
										</asp:DropDownList>
										<span style="white-space: nowrap; display: inline-block;">
											<label style="width: auto;">
												Percent</label>
											<asp:TextBox ID="tbxPercent12" runat="server" CssClass="inputNumeric" Style="width: auto; min-width: 15%;"
												Text="100"></asp:TextBox>
										</span>
										<asp:Button ID="btnRemoveAccount12" runat="server" Style="width: auto;" Text="Remove" />
									</li>
									<li>
										<asp:Button ID="btnAddAccount" runat="server" Style="width: auto;" Text="Add Another Account" />
										<span style="white-space: nowrap; display: inline-block;">
											<label style="width: auto;">
												Total Percent</label>
											<asp:TextBox ID="tbxTotalPercent" runat="server" CssClass="inputNumeric" Enabled="false"
												Style="max-width: 75px;">0</asp:TextBox>
										</span>
										<asp:Button ID="btnUpdatePercent" runat="server" Style="width: auto;" Text="Update Total" />
									</li>
								</ul>
							</div>
							<div id="pnlTickets" runat="server" class="section">
								<h2>Tickets</h2>
								<asp:Literal ID="litTickets" runat="server"></asp:Literal>
							</div>
						</asp:Panel>
					</asp:Panel>
					<div id="pnlWarning" runat="server" class="sectionEven" visible="False">
						<asp:Label ID="lblWarning" runat="server" Style="width: auto; background-color: Red; color: White;"></asp:Label>
						&nbsp;								
						<asp:Button ID="btnWarningOk" runat="server" Text="Ok" Width="45%" />
					</div>
					<div id="pnlWarningConfirm" runat="server" class="section" visible="false">
						<asp:Label ID="lblWarningConfirm" runat="server" Style="width: auto; background-color: Red; color: White;"></asp:Label>
						&nbsp;		
						<asp:Button ID="btnWarningConfirmYes" runat="server" Text="Yes" Width="50px" />
						<asp:Button ID="btnWarningConfirmNo" runat="server" Text="No" Width="50px" />
					</div>
					<div class="sectionEven" id="pnlCopyOrder" runat="server" visible="false">
						<ul>
							<li>
								<label>
									Preceding order number
								</label>
								<asp:TextBox ID="tbxStartNumber" runat="server"></asp:TextBox>
							</li>
							<li>
								<label>
									Number of copies
								</label>
								<asp:TextBox ID="tbxNumOfCopys" runat="server" CssClass="inputNumeric">1</asp:TextBox>
							</li>
							<li>
								<asp:Button ID="btnCreateCopies" runat="server" Style="width: 45%;" Text="Copy" />
								<asp:Button ID="btnCancelCopies" runat="server" Style="width: 45%;" Text="Cancel" />
							</li>
						</ul>
					</div>
				</div>
			</ContentTemplate>
		</asp:UpdatePanel>
	</form>
</body>
</html>
