<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="Receipts.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.Receipts"
	MaintainScrollPositionOnPostback="true" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Reports : Receipts</title>
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
			if (event.target.id === 'btnShowTicketSources') {
				event.stopPropagation();
			} else {
				var $currEl = $(event.target);
				if ($currEl.closest($('#pnlTicketReceivingSources')).length > 0)
					event.stopPropagation();
				else
					$('#pnlTicketReceivingSources').fadeOut();
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
					<div class="recordSelection" style="margin: 0; padding: 0;">
						<div class="sectionEven">
							<ul>
								<li>
									<label>
										Facility</label>
									<asp:DropDownList ID="ddlLocation" runat="server" AutoPostBack="True">
									</asp:DropDownList>
								</li>
								<li>
									<label>
										Ticket</label>
									<asp:DropDownList ID="ddlTicket" runat="server" AutoPostBack="True">
									</asp:DropDownList>
								</li>
								<li>
									<label>
										Sort</label>
									<asp:DropDownList ID="ddlSortBy" runat="server" AutoPostBack="true">
										<asp:ListItem Text="Oldest at top" Value="DateAsc"></asp:ListItem>
										<asp:ListItem Text="Newest at top" Value="DateDesc" Selected="True"></asp:ListItem>
										<asp:ListItem Text="Ticket Number (Asc)" Value="TicketAsc"></asp:ListItem>
										<asp:ListItem Text="Ticket Number (Desc)" Value="TicketDesc"></asp:ListItem>
									</asp:DropDownList>
								</li>
								<li>
									<label>
										&nbsp;</label>
									<asp:CheckBox ID="cbxShowArchived" Text="Show archived" runat="server" AutoPostBack="True"
										Style="width: 33%;" />
									<asp:CheckBox ID="cbxShowVoided" Text="Show voided" runat="server" AutoPostBack="True"
										Style="width: 33%;" />
									<asp:CheckBox ID="cbxShowInternalTransfer" Text="Show internal transfer" runat="server"
										AutoPostBack="True" Style="width: 33%;" />
								</li>
							</ul>
						</div>
						<div class="sectionOdd">
							<ul>
								<li><span class="findRecord">
									<asp:TextBox ID="tbxFind" runat="server"></asp:TextBox>
									<asp:Button ID="btnFind" runat="server" Text="Find" Style="width: auto;" />
								</span>&nbsp;<a href="AdvancedSearch.aspx?SearchType=Tickets">Advanced Search</a>
								</li>
							</ul>
						</div>
					</div>
					<div class="section" id="tblTicketOptions" runat="server" style="margin: 0; padding: 0;">
						<div class="recordControl">
							<asp:Button ID="btnArchive" runat="server" Text="Archive" Visible="False" />
							<asp:Button ID="btnVoid" runat="server" Text="Void" Visible="False" />
							<asp:Button ID="btnPrinterFriendlyVersion" runat="server" Text="Printer Friendly Version"></asp:Button>
							<asp:Button ID="btnShowTicketSources" runat="server" OnClientClick="document.getElementById('pnlTicketReceivingSources').style.display = 'block'; return false;" Text="Show sources" UseSubmitBehavior="false"></asp:Button>
						</div>
						<div class="sectionEven" id="pnlLinkedTickets" runat="server">
							<ul>
								<li>
									<label>
										Linked Tickets</label>
									<span class="input">
										<asp:Literal ID="litLinkedTickets" runat="server"></asp:Literal>
									</span>
								</li>
							</ul>
						</div>
						<div class="overlay" id="pnlTicketReceivingSources" runat="server">
							<button onclick="document.getElementById('pnlTicketReceivingSources').fadeOut(); return false;" style="float: right; top: 0px;">Close</button>
							<asp:Literal ID="litReceivingTicketUsage" runat="server"></asp:Literal>
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
									<asp:DropDownList ID="ddlAddEmailAddress" Style="width: 45%;" runat="server" onchange="DisplayAddEmailButton(this.value);">
									</asp:DropDownList>
									<asp:Button ID="btnAddEmailAddress" Style="width: 15%;" runat="server" Text="Add" visibility="false" />
								</li>
								<li style="color: Red;">
									<asp:Literal ID="litEmailConfirmation" runat="server"></asp:Literal>
								</li>
							</ul>
						</div>
						<div class="section">
							<iframe id="frmTicket" runat="server" style="width: 100%; height: 600px; border: 1px solid #000000;" onclick="$('#pnlTicketReceivingSources').fadeOut();"></iframe>
						</div>
					</div>
				</div>
			</ContentTemplate>
		</asp:UpdatePanel>
	</form>
</body>
</html>
