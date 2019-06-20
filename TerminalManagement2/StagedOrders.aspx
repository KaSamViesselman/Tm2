<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="StagedOrders.aspx.vb" MaintainScrollPositionOnPostback="true" Inherits="KahlerAutomation.TerminalManagement2.StagedOrders" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Orders : Staged Orders</title>
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
			padding: 0px;
		}

		.shortcutPanel {
			width: 30%;
			min-width: 210px;
			vertical-align: top;
			display: inline-block;
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
			document.getElementById('pnlGeneralOrderInformation').style.visibility = "hidden";
			document.getElementById('pnlStagedOrderInformation').style.visibility = "hidden";
		}

		function onRequestDone(sender, args) {
			$find('PleaseWaitPopup').hide();
			document.getElementById('pnlGeneralOrderInformation').style.visibility = "visible";
			document.getElementById('pnlStagedOrderInformation').style.visibility = "visible";
			if (args.get_error() != undefined) {
				var errorMessage = args.get_error().message
				args.set_errorHandled(true);
				alert(errorMessage);
			}
		}
		function SetCalendarPickers() {
		}
	</script>
</head>
<body>
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
				<input id="SetTareWeightOnTransports" runat="server" type="hidden" value="false" />
				<div class="recordSelection">
					<label>
						Staged order
					</label>
					<asp:DropDownList ID="ddlStagedOrders" runat="server" AutoPostBack="True">
					</asp:DropDownList>
				</div>
				<div class="recordControl">
					<asp:Button ID="btnSave" runat="server" Text="Save" />
					<asp:Button ID="btnDelete" runat="server" Text="Delete" Enabled="False" />
					<asp:Button ID="btnPointOfSale" runat="server" Text="Create Point of Sale" />
					<asp:Button ID="btnPrintPickticket" runat="server" Text="Print" />
					<asp:Button ID="btnClearLockedStatus" runat="server" Text="Clear Locked Status" />
					<asp:Button ID="btnSetLockedStatus" runat="server" Text="Set Locked Status" />
					<asp:Label ID="lblStatus" runat="server" ForeColor="Red" />
					<span class="sectionRequiredField"><span class="required"></span>&nbsp;indicates required field </span>
				</div>
				<asp:Panel ID="pnlGeneralOrderInformation" runat="server" CssClass="section">
					<div class="section" id="pnlOrderDetailStatus" runat="server" visible="false">
						<asp:Label ID="lblOrderDetailStatus" runat="server" Text="" ForeColor="Red">   
						</asp:Label>
					</div>
					<div class="sectionEven">
						<div class="section">
							<h1>General Order Information
							</h1>
							<div class="sectionEven">
								<ul>
									<li>
										<label>
											Facility
										</label>
										<span class="required">
											<asp:DropDownList ID="ddlFacility" runat="server" AutoPostBack="true">
											</asp:DropDownList>
										</span></li>
								</ul>
							</div>
							<div class="sectionOdd">
								<ul>
									<li>
										<label>
											Bay
										</label>
										<asp:DropDownList ID="ddlBayAssigned" runat="server" AutoPostBack="true">
										</asp:DropDownList>
									</li>
								</ul>
							</div>
						</div>
						<div class="section">
							<h1>Order Number
							</h1>
							<div class="sectionEven">
								<ul>
									<li style="text-align: left;">
										<asp:ListBox ID="lstUnusedOrders" runat="server" Style="text-align: right; width: 75%;"></asp:ListBox>
										<span style="float: right; width: 15%;">
											<asp:LinkButton ID="btnAddOrderToStagedOrder" runat="server" CssClass="button" Text="r"></asp:LinkButton><br />
											<asp:LinkButton ID="btnRemoveOrderFromStagedOrder" runat="server" CssClass="button" Text="l"></asp:LinkButton>
										</span></li>
									<li style="font-size: x-small;">An asterisk next to the order number denotes that the order is assigned to another staged order. </li>
									<li>
										<asp:TextBox ID="tbxFind" runat="server"></asp:TextBox>
										<asp:Button ID="btnFind" runat="server" Text="Find" Style="width: auto;" />
									</li>
									<li>
										<label>
											Customer
										</label>
										<asp:Literal ID="litCustomerName" runat="server"></asp:Literal>
									</li>
									<li>
										<asp:RadioButtonList ID="rblTicketCreationSource" runat="server" Style="width: 100%; display: inline;" AutoPostBack="true">
											<asp:ListItem Text="Tickets are created from amount filled in facility" Value="TicketsAreCreatedForAmountFilledInFacility"></asp:ListItem>
											<asp:ListItem Text="Tickets are created from amount used off-site" Value="TicketsAreCreatedForAmountUsedOffsite"></asp:ListItem>
										</asp:RadioButtonList>
									</li>
								</ul>
							</div>
							<div class="sectionOdd">
								<ul>
									<li><span class="required">
										<asp:ListBox ID="lstUsedOrders" runat="server" AutoPostBack="true" Style="text-align: right; width: 75%;"></asp:ListBox>
									</span></li>
									<li>
										<label>
											Use Order Percents
										</label>
										<asp:CheckBox ID="chkUseOrderPercents" runat="server" AutoPostBack="true" Text="" />
									</li>
									<li id="lblOrderPercentage" runat="server">
										<label>
											Percentage
										</label>
										<span class="input" style="text-align:right;">
											<asp:TextBox ID="tbxOrderPercentage" runat="server" AutoPostBack="true" Text="0" Style="text-align: right; width: 80%;"></asp:TextBox>%
										</span>
									</li>
									<li>
										<asp:Literal ID="litPercentageAssigned" runat="server"></asp:Literal>
									</li>
									<li id="rowStagedOrderOrderApplicatorInfo" runat="server">
										<label>
											Applicator
										</label>
										<asp:DropDownList ID="ddlOrderApplicator" runat="server" AutoPostBack="true">
										</asp:DropDownList>
									</li>
									<li id="rowStagedOrderOrderAcresInfo" runat="server">
										<label>
											Acres
										</label>
										<asp:TextBox ID="tbxOrderAcres" runat="server" AutoPostBack="true" Style="text-align: right" Text="0"></asp:TextBox>
									</li>
									<li id="lblShipTo" runat="server">
										<label>
											Ship to
										</label>
										<asp:DropDownList ID="ddlCustomerSite" runat="server" AutoPostBack="true">
										</asp:DropDownList>
										<span class="input">
											<asp:Literal ID="litShipTo" runat="server"></asp:Literal>
										</span></li>
								</ul>
							</div>
						</div>
						<div class="section">
							<div class="sectionEven">
								<ul>
									<li>
										<label>
											Carrier
										</label>
										<asp:DropDownList ID="ddlCarrier" runat="server" AutoPostBack="true">
										</asp:DropDownList>
									</li>
								</ul>
							</div>
							<div class="sectionOdd">
								<ul>
									<li>
										<label>
											Driver
										</label>
										<asp:DropDownList ID="ddlDriver" runat="server">
										</asp:DropDownList>
									</li>
								</ul>
							</div>
						</div>
					</div>
					<div class="sectionOdd" id="orderDetailsCell" runat="server">
						<h2>Order Details
						</h2>
						<asp:Literal ID="litOrderDetails" runat="server"></asp:Literal>
					</div>
					<div class="section" id="pnlStagedOrderInformation">
						<hr />
						<ul id="pnlStagedTransports" runat="server">
						</ul>
						<center>
					<asp:Label ID="lblStagedOrderTotals" runat="server" Style="font-weight: bold;"></asp:Label>
					</center>
						<div class="section" id="rowAutofill" runat="server">
							<hr />
							<h1>Shortcut &nbsp;<img src="images/ShortcutSmall.png" alt="" />
							</h1>
							<ul id="pnlShortcuts" runat="server">
								<li>
									<div class="shortcutPanel" id="pnlUseRemainingOrderQuantityShortcut" runat="server">
										<asp:Button ID="btnUseRemainingOrderQuantityShortcut" runat="server" align="center" Width="95%" Text="Use Remaining Order Quantity" />
									</div>
									<div class="shortcutPanel" id="pnlUseOriginalOrderQuantityShortcut" runat="server">
										<asp:Button ID="btnUseOriginalOrderQuantityShortcut" runat="server" align="center" Width="95%" Text="Use Original Order Quantity" />
									</div>
									<div class="shortcutPanel" id="pnlUseApplicationRateShortcut" runat="server">
										<asp:Button ID="btnUseApplicationRateShortcut" runat="server" align="center" Text="Use Application Rate" Width="95%" />
									</div>
								</li>
								<li>
									<div class="shortcutPanel" id="pnlUseTransportCapacityShortcut" runat="server">
										<asp:Button ID="btnUseTransportCapacityShortcut" runat="server" align="center" Text="Use Transport Capacity" Width="95%" />
									</div>
									<div class="shortcutPanel" id="pnlUseBatchQuantityShortcut" runat="server">
										<asp:Button ID="btnUseBatchQuantityShortcut" runat="server" align="center" Text="Use Batch Quantity" Width="95%" />
									</div>
									<div class="shortcutPanel" id="pnlSpecifyTotalQuantityShortcut" runat="server">
										<asp:Button ID="btnSpecifyTotalQuantityShortcut" runat="server" align="center" Text="Specify Total Quantity" Width="95%" />
										<asp:TextBox ID="tbxSpecifyTotalQuantity" runat="server" Style="text-align: right; width: 40%;" Text="0"></asp:TextBox>
										<asp:DropDownList ID="ddlSpecifyTotalQuantity" runat="server" Style="width: auto; max-width: 50%; min-width: 10%;">
										</asp:DropDownList>
										&nbsp;
									</div>
								</li>
							</ul>
						</div>
						<hr />
						<div class="section">
							<ul>
								<li>
									<label style="width: 15%;">
										Internal notes
									</label>
									<asp:TextBox ID="tbxNotes" runat="server" TextMode="MultiLine" Style="width: 80%;"></asp:TextBox></li>
							</ul>
						</div>
					</div>
				</asp:Panel>
			</ContentTemplate>
		</asp:UpdatePanel>
	</form>
</body>
</html>
