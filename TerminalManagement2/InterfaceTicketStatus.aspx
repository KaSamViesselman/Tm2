<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="InterfaceTicketStatus.aspx.vb"
	Inherits="KahlerAutomation.TerminalManagement2.InterfaceTicketStatus" EnableViewState="true" %>

<!DOCTYPE html>
<html lang="en">
<head id="Head1" runat="server">
	<title>Interfaces : Ticket Export Status</title>
	<link rel="shortcut icon" href="FavIcon.ico" />
	<meta name="viewport" content="width=device-width" />
	<meta http-equiv="X-UA-Compatible" content="IE=edge" />
	<link rel="stylesheet" href="styles/site.css" />
	<link rel="stylesheet" href="styles/site-tablet.css" media="(max-width:768px)" />
	<link rel="stylesheet" href="styles/site-phone.css" media="(max-width:480px)" />
	<script type="text/javascript" src="scripts/jquery-1.11.1.min.js"></script>
	<script type="text/javascript" src="scripts/soap-1.0.1.js"></script>
	<script type="text/javascript" src="scripts/page-controller.js"></script>
	<script type="text/javascript" language="javascript">

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
			document.getElementById('pnlMain').style.visibility = "hidden";
		}

		function onRequestDone(sender, args) {
			$find('PleaseWaitPopup').hide();
			document.getElementById('pnlMain').style.visibility = "visible";
			if (args.get_error() != undefined) {
				var errorMessage = args.get_error().message
				args.set_errorHandled(true);
				alert(errorMessage);
			}
		}

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
	<form id="main" runat="server" method="post">
		<asp:ScriptManager ID="ToolkitScriptManager1" runat="server" AsyncPostBackTimeout="3600" OnAsyncPostBackError="ScriptManager1_AsyncPostBackError"
			AsyncPostBackErrorMessage="Timeout while retrieving exported ticket data">
		</asp:ScriptManager>
		<asp:Panel ID="PleaseWaitMessagePanel" runat="server" CssClass="modalPopup" Style="height: 50px; width: 125px; text-align: center;">
			Please wait<br />
			<img src="images/ajax-loader.gif" alt="Please wait" />
		</asp:Panel>
		<asp:Button ID="HiddenButton" runat="server" Text="Hidden Button" Style="visibility: hidden;"
			ToolTip="Necessary for Modal Popup Extender" />
		<ajaxToolkit:ModalPopupExtender ID="PleaseWaitPopup" BehaviorID="PleaseWaitPopup"
			runat="server" TargetControlID="HiddenButton" PopupControlID="PleaseWaitMessagePanel"
			BackgroundCssClass="modalBackground">
		</ajaxToolkit:ModalPopupExtender>
		<asp:UpdatePanel ID="PleaseWaitPanel" runat="server" RenderMode="Inline">
			<ContentTemplate>
				<div class="recordSelection">
					<label>
						Interface
					</label>
					<asp:DropDownList ID="ddlInterfaces" runat="server" AutoPostBack="True">
					</asp:DropDownList>
				</div>
				<div class="recordControl" id="recordControl" runat="server">
					<asp:Panel runat="server" ID="pnlRecordControl">
						<asp:Button ID="btnPrinterFriendly" runat="server" Text="Printer Friendly" />
						<asp:Button ID="btnDownload" runat="server" Text="Download Report" />
					</asp:Panel>
				</div>
				<div class="section" id="pnlMain">
					<div class="sectionEven" id="ticketDisplayOptions" runat="server" style="text-align: left; vertical-align: top;">
						<asp:RadioButton ID="rbShowTicketsExported" runat="server" AutoPostBack="true" Text="Show tickets exported" /><br />
						<asp:CheckBox ID="chkIncludeTicketsMarkedManually" runat="server" AutoPostBack="true"
							Text="Include tickets that were marked manually" Style="margin-left: 20px;" /><br />
						<asp:RadioButton ID="rbShowTicketsNotExported" runat="server" AutoPostBack="true"
							Text="Show tickets not exported" Checked="True" /><br />
						<asp:RadioButton ID="rbIncludeTicketsWithoutErrors" runat="server" AutoPostBack="true"
							Text="Show tickets without errors" Style="margin-left: 20px;" Checked="True"></asp:RadioButton><br />
						<asp:CheckBox ID="chkOnlyIncludeOrdersForThisInterface" runat="server" AutoPostBack="true"
							Text="Only include orders for this interface" Checked="true" Style="margin-left: 40px;" /><br />
						<asp:RadioButton ID="rbIncludeTicketsWithError" runat="server" AutoPostBack="true"
							Text="Show tickets with errors" Style="margin-left: 20px;"></asp:RadioButton><br />
						<asp:RadioButton ID="rbIncludeTicketsWithIgnoredError" runat="server" AutoPostBack="true"
							Text="Show tickets with ignored errors" Style="margin-left: 20px;"></asp:RadioButton><br />
					</div>
					<div class="sectionOdd" id="ticketSortOptions" runat="server">
						<label>
							Sort</label>
						<asp:DropDownList ID="ddlSortBy" runat="server" AutoPostBack="true">
							<asp:ListItem Text="Exported date (Asc)" Value="ExportedAtDateAsc"></asp:ListItem>
							<asp:ListItem Text="Exported date (Desc)" Value="ExportedAtDateDesc" Selected="True"></asp:ListItem>
							<asp:ListItem Text="Loaded date (Asc)" Value="LoadedAtDateAsc"></asp:ListItem>
							<asp:ListItem Text="Loaded date (Desc)" Value="LoadedAtDateDesc"></asp:ListItem>
							<asp:ListItem Text="Ticket number (Asc)" Value="TicketAsc"></asp:ListItem>
							<asp:ListItem Text="Ticket number (Desc)" Value="TicketDesc"></asp:ListItem>
						</asp:DropDownList>
					</div>
					<div class="section" id="divTickets" runat="server">
						<table id="tblTickets" runat="server">
							<tr>
								<th style="color: White;">Tickets Table
								</th>
							</tr>
						</table>
					</div>
					<div class="sectionEven" id="pnlPageNumbers" runat="server">
						<asp:Button ID="btnPreviousPage" runat="server" Text="Previous" />
						<asp:DropDownList ID="ddlPageNumber" runat="server" AutoPostBack="true" EnableViewState="true">
						</asp:DropDownList>
						<asp:Button ID="btnNextPage" runat="server" Text="Next" />
						<label>
							Items Per Page</label>
						<asp:DropDownList ID="ddlTicketsPerPage" runat="server" AutoPostBack="true">
							<asp:ListItem Text="25" Value="25" Selected="True"></asp:ListItem>
							<asp:ListItem Text="50" Value="50"></asp:ListItem>
							<asp:ListItem Text="100" Value="100"></asp:ListItem>
							<asp:ListItem Text="250" Value="250"></asp:ListItem>
							<asp:ListItem Text="All" Value="-1"></asp:ListItem>
						</asp:DropDownList>
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
				</div>
			</ContentTemplate>
		</asp:UpdatePanel>
	</form>
</body>
</html>
