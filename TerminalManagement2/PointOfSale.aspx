<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="PointOfSale.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.PointOfSale"
	MaintainScrollPositionOnPostback="true" EnableViewState="true" %>

<!DOCTYPE html>
<html lang="en">
<head id="Head1" runat="server">
	<title>Orders : </title>
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
		}

		function onBeginRequest(sender, args) {
			var send = args.get_postBackElement().value;
			$find('PleaseWaitPopup').show();
			document.getElementById('recordControl').style.visibility = "hidden";
			document.getElementById('pnlGeneralOrderInformation').style.visibility = "hidden";
			document.getElementById('pnlStagedOrderInformation').style.visibility = "hidden";
		}

		function onRequestDone(sender, args) {
			$find('PleaseWaitPopup').hide();
			document.getElementById('recordControl').style.visibility = "visible";
			document.getElementById('pnlGeneralOrderInformation').style.visibility = "visible";
			document.getElementById('pnlStagedOrderInformation').style.visibility = "visible";
			if (args.get_error() != undefined) {
				var errorMessage = args.get_error().message
				args.set_errorHandled(true);
				alert(errorMessage);
			}
		}

	</script>
	<style type="text/css">
		.transportRow {
			background-color: #003399 !important;
			color: White;
		}
	</style>
</head>
<body>
	<form id="main" method="post" runat="server">
		<div class="recordSelection">
			<h1 style="font-size: large; font-weight: bold;">Create point of sale ticket
			</h1>
		</div>
		<div class="recordControl" id="recordControl" runat="server">
			<asp:Button ID="btnCreateTicket" TabIndex="9" runat="server" Text="Create Ticket" />
			<asp:Button ID="btnCancel" runat="server" Text="Cancel" />
			<asp:Label ID="lblStatus" runat="server" ForeColor="Red" />
			<span class="sectionRequiredField"><span class="required"></span>&nbsp;indicates required
			field </span>
		</div>
		<asp:ScriptManager ID="ToolkitScriptManager1" runat="server" AsyncPostBackTimeout="3600" AsyncPostBackErrorMessage="Timeout while retrieving exported ticket data." EnablePartialRendering="true" OnAsyncPostBackError="ScriptManager1_AsyncPostBackError">
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
				<div class="section" id="pnlGeneralOrderInformation" runat="server">
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
							<li>
								<label>
									Bay
								</label>
								<asp:DropDownList ID="ddlBayAssigned" runat="server" AutoPostBack="true">
								</asp:DropDownList>
							</li>
							<li>
								<label>
									Panel
								</label>
								<asp:DropDownList ID="ddlPanel" runat="server" AutoPostBack="true">
								</asp:DropDownList>
							</li>
							<li>
								<label>
									Carrier
								</label>
								<asp:DropDownList ID="ddlCarrier" runat="server" AutoPostBack="true">
								</asp:DropDownList>
							</li>
							<li>
								<label>
									Driver
								</label>
								<asp:DropDownList ID="ddlDriver" runat="server">
								</asp:DropDownList>
							</li>
						</ul>
					</div>
					<div class="sectionOdd">
						<ul>
							<li>
								<label>
									Order number
								</label>
								<span class="input">
									<asp:ListBox ID="lstUsedOrders" runat="server" Width="100%" AutoPostBack="true" Style="text-align: right;"></asp:ListBox>
								</span></li>
							<li>
								<label>
									Customer
								</label>
								<asp:Literal ID="litCustomerName" runat="server"></asp:Literal>
							</li>
							<li id="pnlShipTo" runat="server">
								<label>
									Ship to
								</label>
								<span class="input">
									<asp:DropDownList ID="ddlCustomerSite" runat="server" Width="100%" AutoPostBack="true">
									</asp:DropDownList>
									<br />
									<asp:Literal ID="litShipTo" runat="server"></asp:Literal>
								</span></li>
							<li id="pnlAcres" runat="server">
								<label>
									Applicator
								</label>
								<asp:DropDownList ID="ddlOrderApplicator" runat="server" AutoPostBack="true">
								</asp:DropDownList>
							</li>
							<li id="pnlApplicator" runat="server">
								<label>
									Acres
								</label>
								<asp:TextBox ID="tbxOrderAcres" runat="server" AutoPostBack="true" CssClass="inputNumeric"
									Text="0"></asp:TextBox>
							</li>
						</ul>
					</div>
				</div>
				<div class="section" id="pnlStagedOrderInformation" runat="server">
					<ul id="pnlStagedTransports" runat="server">
					</ul>
					<ul id="pnlReceiveIntoStorageLocation" runat="server" cssclass="sectionEven">
						<li>
							<label>
								Discharge into storage location(s)
							</label>
							<asp:CheckBoxList ID="cblDischargeStorageLocations" runat="server" RepeatLayout="UnorderedList" CssClass="input"></asp:CheckBoxList>
						</li>
					</ul>
					<asp:Panel CssClass="section" ID="pnlCustomLoadQuestions" runat="server">
						<hr />
						<asp:Panel CssClass="sectionEven" ID="pnlCustomPreLoadQuestions" runat="server">
							<h2>Pre-load questions</h2>
							<ul id="ulCustomPreLoadQuestions" runat="server" enableviewstate="true">
							</ul>
						</asp:Panel>
						<asp:Panel CssClass="sectionOdd" ID="pnlCustomPostLoadQuestions" runat="server">
							<h2>Post-load questions</h2>
							<ul id="ulCustomPostLoadQuestions" runat="server" enableviewstate="true">
							</ul>
						</asp:Panel>
					</asp:Panel>
					<span id="pnlInternalNotes" runat="server">
						<hr />
						Internal notes&nbsp;
			<asp:Label ID="lblInternalNotes" runat="server" TextMode="MultiLine" Style="width: 100%;"></asp:Label>
					</span>
				</div>
			</ContentTemplate>
		</asp:UpdatePanel>
	</form>
</body>
</html>
