<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="Receiving.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.Receiving" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Purchase Orders : Receiving Purchase Orders</title>
	<link rel="shortcut icon" href="FavIcon.ico" />
	<meta name="viewport" content="width=device-width" />
	<meta http-equiv="X-UA-Compatible" content="IE=edge" />
	<meta name="description" content="Receiving purchase order records represent orders to receive bulk material at a facility. When a purchase order is received at a facility, the inventory of the bulk product for the selected owner at the selected facility is automatically increased by the quantity received." />
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
			document.getElementById('pnlUpdatePanel').style.visibility = "hidden";
		}

		function onRequestDone(sender, args) {
			$find('PleaseWaitPopup').hide();
			document.getElementById('pnlUpdatePanel').style.visibility = "visible";

			if (args.get_error() != undefined) {
				var errorMessage = args.get_error().message
				args.set_errorHandled(true);
				alert(errorMessage);
			}
		}

		function SetCalendarPickers() {
			$('#tbxTransportTareDate').datetimepicker({
				timeFormat: 'h:mm:ss TT',
				showSecond: true,
				showOn: "button",
				buttonImage: 'Images/Calendar_scheduleHS.png',
				buttonImageOnly: true,
				buttonText: "Show calendar"
			});
		}

		// This should set the capability to fade out the window
		$(document).on('click', function (event) {
			if (event.target.id === 'btnShowPoUsages') {
				$('#pnlReceivingTicketUsage').fadeOut();
				event.stopPropagation();
			} else if (event.target.id === 'btnShowTicketUsages') {
				$('#pnlReceivingPoTicketUsage').fadeOut();
				event.stopPropagation();
			} else {
				var $currEl = $(event.target);
				if ($currEl.closest($('#pnlReceivingPoTicketUsage')).length > 0) {
					$('#pnlReceivingTicketUsage').fadeOut();
					event.stopPropagation();
				} else if ($currEl.closest($('#pnlReceivingTicketUsage')).length > 0) {
					$('#pnlReceivingPoTicketUsage').fadeOut();
					event.stopPropagation();
				} else {
					$('#pnlReceivingPoTicketUsage').fadeOut();
					$('#pnlReceivingTicketUsage').fadeOut();
				}
			}
		});
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
				<div id="pnlUpdatePanel">
					<asp:Panel ID="pnlMain" runat="server" Visible="True">
						<div class="recordSelection" id="pnlRecordControl">
							<ul>
								<li>
									<label>
										Facility</label>
									<asp:DropDownList ID="ddlFacilityFilter" runat="server" AutoPostBack="true">
									</asp:DropDownList>
								</li>
								<li>
									<label>
										Receiving Purchase Order
									</label>
									<asp:DropDownList ID="ddlPurchaseOrders" runat="server" AutoPostBack="True">
									</asp:DropDownList>
									<span class="findRecord">
										<asp:TextBox ID="tbxFind" runat="server" MaxLength="50"></asp:TextBox>
										<asp:Button ID="btnFind" runat="server" Text="Find" />
										&nbsp;<a href="AdvancedSearch.aspx?SearchType=Receiving">Advanced Search</a>
									</span>
								</li>
							</ul>
						</div>
						<div class="recordControl">
							<asp:Button ID="btnSave" runat="server" Width="120px" Text="Save" />
							<asp:Button ID="btnDelete" runat="server" Width="120px" Text="Delete" />
							<asp:Label ID="lblStatus" runat="server" ForeColor="Red" />
							<div class="sectionRequiredField">
								<label>
									<span class="required"></span>&nbsp;indicates required field
								</label>
							</div>
						</div>
						<asp:Panel ID="pnlEven" runat="server" CssClass="section">
							<div class="section">
								<asp:Button ID="btnCopy" runat="server" Width="150px" Text="Copy" Enabled="False" />
								<asp:Button ID="btnPrintPo" runat="server" Width="150px" Text="Print" Enabled="false" />
								<asp:Button ID="btnMarkComplete" runat="server" Width="150px" Text="Mark Complete" Enabled="False" />
								<asp:Button ID="btnReceive" runat="server" Width="150px" Text="Receive" Enabled="false" />
								<asp:Button ID="btnShowPoUsages" runat="server" OnClientClick="document.getElementById('pnlReceivingPoTicketUsage').style.display = 'block'; document.getElementById('pnlReceivingTicketUsage').style.display = 'none'; return false;" Text="Show usages" UseSubmitBehavior="false" Style="width: 150px;"></asp:Button>
							</div>
							<asp:Panel ID="pnlGeneral" runat="server" CssClass="sectionEven">
								<h1>General</h1>
								<ul>
									<li>
										<label>
											Number
										</label>
										<span class="required">
											<asp:TextBox ID="tbxNumber" runat="server" MaxLength="50"></asp:TextBox>
										</span></li>
									<li>
										<label>
											Owner
										</label>
										<span class="required">
											<asp:DropDownList ID="ddlOwner" runat="server" AutoPostBack="true">
											</asp:DropDownList>
										</span></li>
									<li>
										<label>
											Supplier
										</label>
										<span class="required">
											<asp:DropDownList ID="ddlSupplier" runat="server">
											</asp:DropDownList>
										</span></li>
									<li>
										<label>
											Bulk product
										</label>
										<span class="required">
											<asp:DropDownList ID="ddlBulkProduct" runat="server" AutoPostBack="true">
											</asp:DropDownList>
										</span></li>
									<li>
										<label>
											Notes
										</label>
										<asp:TextBox ID="tbxNotes" runat="server" TextMode="MultiLine"></asp:TextBox></li>
									<li>
										<label>
											Purchased
										</label>
										<span class="required">
											<asp:TextBox ID="tbxPurchased" runat="server" Width="45%" Style="text-align: right;"></asp:TextBox>
											<asp:DropDownList ID="ddlUnit" runat="server" Width="15%">
											</asp:DropDownList>
										</span></li>
									<li>
										<label>
											Received
										</label>
										<asp:Label ID="lblDelivered" runat="server" Text=""></asp:Label>
									</li>
								</ul>
								<ul id="lstCustomFields" runat="server">
								</ul>
							</asp:Panel>
						</asp:Panel>
					</asp:Panel>
					<div class="sectionEven" id="pnlCopy" runat="server" visible="false">
						<ul>
							<li>
								<label>
									Start number
								</label>
								<asp:TextBox ID="tbxStartNumber" runat="server"></asp:TextBox>
							</li>
							<li>
								<label>
									Copies
								</label>
								<asp:TextBox ID="tbxCopies" runat="server" Width="15%" Style="text-align: right;"></asp:TextBox>
							</li>
						</ul>
						<asp:Button ID="btnCreateCopies" runat="server" Width="45%" Text="Create copies" />
						<asp:Button ID="btnCancelCopy" runat="server" Width="45%" Text="Cancel" />
					</div>
					<div class="sectionEven" id="pnlReceive" runat="server" visible="false">
						<ul>
							<li>
								<label>
									Number
								</label>
								<asp:Label ID="lblNumber" runat="server"></asp:Label>
							</li>
							<li>
								<label>
									Owner
								</label>
								<asp:Label ID="lblOwner" runat="server"></asp:Label>
							</li>
							<li>
								<label>
									Supplier account
								</label>
								<asp:Label ID="lblSupplierAccount" runat="server"></asp:Label>
							</li>
							<li>
								<label>
									Bulk product
								</label>
								<asp:Label ID="lblBulkProduct" runat="server"></asp:Label>
							</li>
							<li>
								<label>
									Purchased
								</label>
								<asp:Label ID="lblPurchased" runat="server"></asp:Label>
							</li>
							<li>
								<label>
									Total Delivered
								</label>
								<asp:Label ID="lblTotalDelivered" runat="server"></asp:Label>
							</li>
							<li>
								<label>
									Delivered
								</label>
								<asp:TextBox ID="tbxDelivered" runat="server" Width="45%" Style="text-align: right;"></asp:TextBox><asp:DropDownList ID="ddlDeliveredUnit" runat="server" Width="15%">
								</asp:DropDownList>
							</li>
							<li>
								<div class="section" id="pnlExistingCarrier" runat="server">
									<label>
										Carrier
									</label>
									<span class="input">
										<asp:DropDownList ID="ddlCarrier" runat="server">
										</asp:DropDownList>
										<asp:Button ID="btnNewCarrier" runat="server" Style="width: 15%; min-width: 60px;" Text="New" Enabled="false" />
									</span>
								</div>
								<div class="section" id="pnlNewCarrier" runat="server" visible="false">
									<ul>
										<li>
											<label>
												Carrier name
											</label>
											<span class="input">
												<asp:TextBox ID="tbxCarrierName" runat="server"></asp:TextBox>
												<asp:Button ID="btnListCarriers" runat="server" Style="width: 15%; min-width: 60px;" Text="List" />
											</span></li>
										<li>
											<label>
												Carrier number
											</label>
											<asp:TextBox ID="tbxCarrierNumber" runat="server"></asp:TextBox>
										</li>
									</ul>
								</div>
							</li>
							<li>
								<div class="section" id="pnlExistingTransport" runat="server">
									<label>
										Transport
									</label>
									<span class="input">
										<asp:DropDownList ID="ddlTransport" runat="server" AutoPostBack="true">
										</asp:DropDownList>
										<asp:Button ID="btnNewTransport" runat="server" Style="width: 15%; min-width: 60px;" Text="New" Enabled="false" />
									</span>
								</div>
								<div class="section" id="pnlNewTransport" runat="server" visible="false">
									<ul>
										<li>
											<label>
												Transport name
											</label>
											<span class="input">
												<asp:TextBox ID="tbxTransportName" runat="server"></asp:TextBox>
												<asp:Button ID="btnListTransports" runat="server" Style="width: 15%; min-width: 60px;" Text="List" />
											</span></li>
										<li>
											<label>
												Transport number
											</label>
											<asp:TextBox ID="tbxTransportNumber" runat="server"></asp:TextBox>
										</li>
									</ul>
								</div>
								<div class="section">
									<ul>
										<li></li>
										<li>
											<label>
												Transport tare weight<br />
												<span id="pnlCurrentTransportTareInfo" runat="server">
													<asp:Literal ID="lblCurrentTransportTareInfo" runat="server"></asp:Literal>
													<asp:LinkButton ID="btnAssignTareFromTransport" runat="server" Text="r" CssClass="button"></asp:LinkButton>
												</span>
											</label>
											<span class="input">
												<asp:TextBox ID="tbxTransportTareWeight" runat="server" Width="45%" Style="text-align: right;"></asp:TextBox>
												<asp:DropDownList ID="ddlTransportUnit" runat="server" Width="15%">
												</asp:DropDownList>
												<br />
												<asp:TextBox ID="tbxTransportTareDate" runat="server" Width="45%" Style="text-align: right;"></asp:TextBox>
											</span></li>
									</ul>
								</div>
							</li>
							<li>
								<div class="section" id="pnlExistingDriver" runat="server">
									<label>
										Driver
									</label>
									<span class="input">
										<asp:DropDownList ID="ddlDriver" runat="server">
										</asp:DropDownList>
										<asp:Button ID="btnNewDriver" runat="server" Style="width: 15%; min-width: 60px;" Text="New" Enabled="false" />
									</span>
								</div>
								<div class="section" id="pnlNewDriver" runat="server" visible="false">
									<ul>
										<li>
											<label>
												Driver name
											</label>
											<span class="input">
												<asp:TextBox ID="tbxDriverName" runat="server"></asp:TextBox>
												<asp:Button ID="btnListDrivers" runat="server" Style="width: 15%; min-width: 60px;" Text="List" />
											</span></li>
										<li>
											<label>
												Driver number
											</label>
											<asp:TextBox ID="tbxDriverNumber" runat="server"></asp:TextBox>
										</li>
									</ul>
								</div>
							</li>
							<li>
								<label>
									Facility
								</label>
								<asp:DropDownList ID="ddlLocation" runat="server">
								</asp:DropDownList>
							</li>
							<li>
								<label>
									Ticket Notes
								</label>
								<asp:TextBox ID="tbxReceivingNotes" runat="server" TextMode="MultiLine"></asp:TextBox>
							</li>
							<li id="pnlReceivingLot" runat="server">
								<label>
									Lot number
								</label>
								<asp:DropDownList ID="ddlReceivingLot" runat="server" AutoPostBack="true"></asp:DropDownList>
							</li>
							<li id="pnlNewReceivingLot" runat="server">
								<label>
									New lot name
								</label>
								<asp:TextBox ID="tbxNewReceivingLotNumber" runat="server"></asp:TextBox>
							</li>
							<li id="pnlReceiveIntoStorageLocation" runat="server">
								<label>
									Storage location
								</label>
								<asp:CheckBoxList ID="cblStorageLocations" runat="server" RepeatLayout="UnorderedList" CssClass="input"></asp:CheckBoxList>
							</li>
						</ul>
						<asp:Button ID="btnReceiveOk" runat="server" Text="Receive" Width="45%" />
						<asp:Button ID="btnReceiveCancel" runat="server" Text="Cancel" Width="45%" />
					</div>
					<div class="section" id="pnlTickets" runat="server">
						<div class="recordSelectionEvenOdd">
							<h2>Tickets
							</h2>
							<div class="sectionEven">
								<ul>
									<li>
										<label>
											Ticket
										</label>
										<asp:DropDownList ID="ddlReceivingTickets" runat="server" AutoPostBack="True">
										</asp:DropDownList>
									</li>
									<li>
										<label>
										</label>
										<asp:CheckBox ID="chkShowVoidedTickets" runat="server" AutoPostBack="true" Text="Show voided tickets" Checked="false" />
									</li>
								</ul>
								<div style="text-align: center;">
									<asp:Button ID="btnPrintTicket" runat="server" Width="30%" Text="Printer friendly version" Enabled="False" />
									<asp:Button ID="btnVoidTicket" runat="server" Width="30%" Text="Void ticket" Enabled="False" />
									<asp:Button ID="btnShowTicketUsages" runat="server" OnClientClick="document.getElementById('pnlReceivingPoTicketUsage').style.display = 'none'; document.getElementById('pnlReceivingTicketUsage').style.display = 'block'; return false;" Text="Show usages" UseSubmitBehavior="false" Style="width: 30%;"></asp:Button>
								</div>
							</div>
						</div>
						<div class="section" id="pnlTextTicket" runat="server" visible="False" bordercolor="Navy" borderstyle="Solid" borderwidth="2px">
							<asp:Literal ID="litTextTicketOutput" runat="server"></asp:Literal>
						</div>
						<div class="section" id="pnlHtmlticket" runat="server" visible="False" bordercolor="Navy" borderstyle="Solid" borderwidth="2px">
							<div id="divHtmlTicket" runat="server">
							</div>
						</div>
					</div>
				</div>
				<div class="overlay" id="pnlReceivingPoTicketUsage" runat="server">
					<button onclick="document.getElementById('pnlReceivingPoTicketUsage').fadeOut(); return false;" style="float: right; top: 0px;">Close</button>
					<asp:Literal ID="litReceivingPoTicketUsage" runat="server"></asp:Literal>
				</div>
				<div class="overlay" id="pnlReceivingTicketUsage" runat="server">
					<button onclick="document.getElementById('pnlReceivingTicketUsage').fadeOut(); return false;" style="float: right; top: 0px;">Close</button>
					<asp:Literal ID="litReceivingTicketUsage" runat="server"></asp:Literal>
				</div>
			</ContentTemplate>
		</asp:UpdatePanel>
	</form>
</body>
</html>
