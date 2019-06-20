<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="PastReceiving.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.PastReceiving" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Purchase Orders : Past Receiving Purchase Orders</title>
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
		function pageLoad(sender, args) {
			var sm = Sys.WebForms.PageRequestManager.getInstance();
			if (!sm.get_isInAsyncPostBack()) {
				sm.add_beginRequest(onBeginRequest);
				sm.add_endRequest(onRequestDone);
			}
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
<body onload="resizeIframe('frmTicket');">
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
								<asp:DropDownList ID="ddlPastReceivingPurchaseOrders" runat="server" AutoPostBack="True">
								</asp:DropDownList>
								<span class="findRecord">
									<asp:TextBox ID="tbxFind" runat="server" MaxLength="50"></asp:TextBox>
									<asp:Button ID="btnFind" runat="server" Text="Find" />
									<asp:CheckBox ID="cbxIncludeArchived" runat="server" AutoPostBack="true" Text="Include archived"
										Visible="false" />
									&nbsp; <a href="AdvancedSearch.aspx?SearchType=PastReceiving">Advanced Search</a>
								</span>
							</li>
						</ul>
					</div>
					<div class="section" id="pnlMain" runat="server">
						<div class="recordControl">
							<asp:Button ID="btnMarkIncomplete" runat="server" Text="Mark incomplete" />
							<asp:Button ID="btnArchive" runat="server" Text="Archive" Enabled="False" />
							<asp:Button ID="btnUnarchive" runat="server" Text="Unarchive" Enabled="False" />
							<asp:Button ID="btnPrintPo" runat="server" Text="Print PO" />
							<asp:Button ID="btnShowPoUsages" runat="server" OnClientClick="document.getElementById('pnlReceivingPoTicketUsage').style.display = 'block'; document.getElementById('pnlReceivingTicketUsage').style.display = 'none'; return false;" Text="Show usages" UseSubmitBehavior="false" Style="width: 150px;"></asp:Button>
						</div>
						<asp:Panel ID="pnlEven" runat="server" CssClass="sectionEven">
							<ul>
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
										Delivered
									</label>
									<asp:Label ID="lblDelivered" runat="server"></asp:Label>
								</li>
							</ul>
						</asp:Panel>
						<asp:Panel ID="pnlTickets" runat="server" CssClass="recordSelectionEvenOdd">
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
									<li style="text-align: center;">
										<asp:Button ID="btnPrintTicket" runat="server" Width="30%" Text="Printer friendly version" Enabled="False" />
										<asp:Button ID="btnVoidTicket" runat="server" Text="Void ticket" Style="width: 30%;" />
										<asp:Button ID="btnShowTicketUsages" runat="server" OnClientClick="document.getElementById('pnlReceivingPoTicketUsage').style.display = 'none'; document.getElementById('pnlReceivingTicketUsage').style.display = 'block'; return false;" Text="Show usages" UseSubmitBehavior="false" Style="width: 30%;"></asp:Button>
									</li>
								</ul>
							</div>
						</asp:Panel>
						<div class="section">
							<iframe id="frmTicket" runat="server" name="frmTicket" class="iframe" style="width: 100%; height: 400px; border: thin solid #000000;" scrolling="auto" onclick="$('#pnlReceivingTicketUsage').fadeOut();"></iframe>
						</div>
						<div class="section" id="pnlTextTicket" runat="server" visible="False" style="border: 2px solid navy;">
							<asp:Literal ID="litTextTicketOutput" runat="server"></asp:Literal>
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
				</div>
			</ContentTemplate>
		</asp:UpdatePanel>
	</form>
</body>
</html>
