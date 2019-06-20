<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="Lots.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.Lots" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>Products : Bulk Product Lots</title>
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
			document.getElementById('pnlRecordControl').style.visibility = "hidden";
			document.getElementById('pnlGeneralInformation').style.visibility = "hidden";
		}

		function onRequestDone(sender, args) {
			$find('PleaseWaitPopup').hide();
			document.getElementById('pnlRecordControl').style.visibility = "visible";
			document.getElementById('pnlGeneralInformation').style.visibility = "visible";

			if (args.get_error() != undefined) {
				var errorMessage = args.get_error().message
				args.set_errorHandled(true);
				alert(errorMessage);
			}
		}
	</script>
</head>
<body>
	<form id="main" method="post" runat="server" defaultfocus="tbxLotNumber">
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
				<ul class="recordSelection">
					<li>
						<label>
							Show lots for bulk product</label>
						<asp:DropDownList ID="ddlBulkProductFilter" runat="server" AutoPostBack="True" />
					</li>
					<li>
						<label>
							Lot</label>
						<asp:DropDownList ID="ddlLots" runat="server" AutoPostBack="True" />
					</li>
				</ul>
				<div id="pnlRecordControl" class="recordControl">
					<asp:Button ID="btnSave" runat="server" Text="Save" />
					<asp:Button ID="btnDelete" runat="server" Text="Delete" Enabled="false" />
					<asp:Label ID="lblStatus" runat="server" ForeColor="Red" />
					<span class="sectionRequiredField"><span class="required"></span>&nbsp;indicates required field </span>
				</div>
				<asp:Panel ID="pnlGeneralInformation" runat="server">
					<div class="section">
						<div class="sectionEven">
							<h1>General</h1>
							<ul>
								<li>
									<label>
										Lot number</label>
									<span class="required">
										<asp:TextBox ID="tbxLotNumber" TabIndex="1" runat="server" /></span>
								</li>
								<li>
									<label>
										Bulk product
									</label>
									<asp:DropDownList ID="ddlBulkProduct" runat="server" AutoPostBack="true"></asp:DropDownList>
								</li>
								<li>
									<label>
									</label>
									<asp:CheckBox ID="cbxUsageTrackingComplete" runat="server" Text="Dispensing usage tracking completed" />
								</li>
							</ul>
						</div>
				</asp:Panel>
			</ContentTemplate>
		</asp:UpdatePanel>
	</form>
</body>
</html>
