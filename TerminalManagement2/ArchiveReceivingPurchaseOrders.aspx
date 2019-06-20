﻿<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="ArchiveReceivingPurchaseOrders.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.ArchiveReceivingPurchaseOrders" MaintainScrollPositionOnPostback="true" %>

<!DOCTYPE html>
<html lang="en">
<head id="Head1" runat="server">
	<title>Purchase Orders : Archive Orders</title>
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
			document.getElementById('pnlFilter').style.visibility = "hidden";
			document.getElementById('pnlRecordControl').style.visibility = "hidden";
			document.getElementById('pnlOrdersArea').style.visibility = "hidden";
		}

		function onRequestDone(sender, args) {
			$find('PleaseWaitPopup').hide();
			document.getElementById('pnlFilter').style.visibility = "visible";
			document.getElementById('pnlRecordControl').style.visibility = "visible";
			document.getElementById('pnlOrdersArea').style.visibility = "visible";

			if (args.get_error() != undefined) {
				var errorMessage = args.get_error().message
				args.set_errorHandled(true);
				alert(errorMessage);
			}
		}

		function SetCalendarPickers() {
			$('#tbxFromDate').datetimepicker({
				timeFormat: 'h:mm:ss TT',
				showSecond: true,
				showOn: "button",
				buttonImage: 'Images/Calendar_scheduleHS.png',
				buttonImageOnly: true,
				buttonText: "Show calendar"
			});

			$('#tbxToDate').datetimepicker({
				timeFormat: 'h:mm:ss TT',
				showSecond: true,
				showOn: "button",
				buttonImage: 'Images/Calendar_scheduleHS.png',
				buttonImageOnly: true,
				buttonText: "Show calendar"
			});
		}
	</script>
</head>
<body>
	<form id="main" runat="server" defaultfocus="tbxName">
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
				<div id="pnlFilter" class="recordSelection">
					<h1>Filters</h1>
					<div class="sectionEven">
						<ul>
							<li>
								<label>
									Created from</label>
								<input type="text" name="tbxFromDate" id="tbxFromDate" value="" runat="server" />
							</li>
						</ul>
					</div>
					<div class="sectionOdd">
						<ul>
							<li>
								<label style="text-align: left;">
									Created to</label>
								<input type="text" name="tbxToDate" id="tbxToDate" value="" runat="server" />
							</li>
						</ul>
					</div>
					<div class="sectionEven">
						<ul>
							<li>
								<label>
									Supplier</label>
								<asp:DropDownList ID="ddlAccounts" runat="server">
								</asp:DropDownList>
							</li>
						</ul>
					</div>
					<div class="sectionOdd">
						<ul>
							<li>
								<label style="text-align: left;">
									Facility</label>
								<asp:DropDownList ID="ddlFacility" runat="server">
								</asp:DropDownList>
							</li>
						</ul>
					</div>
					<div class="sectionEven">
						<ul>
							<li>
								<label>
									Owner</label>
								<asp:DropDownList ID="ddlOwner" runat="server">
								</asp:DropDownList>
							</li>
						</ul>
					</div>
					<div class="sectionOdd">
						<ul>
							<li>
								<label>
									Order number contains
								</label>
								<asp:TextBox ID="tbxOrderNumberContains" runat="server"></asp:TextBox>
							</li>
						</ul>
					</div>
					<div class="sectionEven">
						<ul>
							<li>&nbsp;</li>
						</ul>
					</div>
					<div class="sectionOdd">
						<ul>
							<li>
								<label>
									&nbsp;
								</label>
								<asp:CheckBox ID="cbxShowArchived" Text="Show archived only" runat="server" />
							</li>
						</ul>
					</div>
				</div>
				<div id="pnlRecordControl" class="recordControl">
					<asp:Button ID="btnFilter" runat="server" Text="Filter" />
					<asp:Button ID="btnArchive" runat="server" Text="Archive" />
					<asp:Label ID="lblStatus" runat="server" ForeColor="Red" />
				</div>
				<div id="pnlOrdersArea" runat="server" class="section">
					<asp:Panel ID="pnlOrders" runat="server">
						<h1>Orders</h1>
						<table id="tblOrders" runat="server">
							<tr>
								<th>Purchase Order Number
								</th>
								<th>Supplier
								</th>
								<th>Owner
								</th>
								<th>Product
								</th>
								<th>Created
								</th>
								<th>
									<asp:CheckBox ID="cbxCheckAllOrders" runat="server" AutoPostBack="true" Text="Archive Order" />
								</th>
								<th>
									<asp:CheckBox ID="cbxCheckAllTickets" runat="server" AutoPostBack="true" Text="Archive Tickets" />
								</th>
							</tr>
						</table>
					</asp:Panel>
				</div>
			</ContentTemplate>
		</asp:UpdatePanel>
	</form>
</body>
</html>
