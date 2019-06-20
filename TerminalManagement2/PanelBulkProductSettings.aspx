<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="PanelBulkProductSettings.aspx.vb"
	Inherits="KahlerAutomation.TerminalManagement2.PanelBulkProductSettings" MaintainScrollPositionOnPostback="true" %>

<!DOCTYPE html>
<html lang="en">
<head id="Head1" runat="server">
	<title>Panels : Panel Bulk Product Settings</title>
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
			document.getElementById('pnlBulkProductSettings').style.visibility = "hidden";
		}

		function onRequestDone(sender, args) {
			$find('PleaseWaitPopup').hide();
			document.getElementById('pnlBulkProductSettings').style.visibility = "visible";
			if (args.get_error() != undefined) {
				var errorMessage = args.get_error().message
				args.set_errorHandled(true);
				alert(errorMessage);
			}
		}
	</script>
</head>
<body>
	<form id="main" runat="server" defaultfocus="ddlPanels">
		<asp:ScriptManager ID="ToolkitScriptManager1" runat="server" AsyncPostBackTimeout="3600" AsyncPostBackErrorMessage="Timeout while retrieving data." EnablePartialRendering="true" OnAsyncPostBackError="ScriptManager1_AsyncPostBackError">
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
				<div class="recordSelection">
					<ul>
						<li>
							<label>
								Facility
							</label>
							<asp:DropDownList ID="ddlFacilityFilter" runat="server" AutoPostBack="true">
							</asp:DropDownList>
						</li>
						<li>
							<label>
								Panel
							</label>
							<asp:DropDownList ID="ddlPanels" runat="server" AutoPostBack="True" />
						</li>
					</ul>
				</div>
				<div class="recordControl">
					<asp:Button ID="btnSave" runat="server" Style="width: auto;" Text="Save"
						Enabled="False" />
					<asp:Label ID="lblBulkProductStatus" runat="server" ForeColor="Red" />
					<span class="sectionRequiredField"><span class="required"></span>&nbsp;indicates required
			field </span>
				</div>
				<asp:Panel ID="pnlBulkProductSettings" runat="server" CssClass="section">
					<h1>Bulk products</h1>
					<asp:Panel ID="pnlEven" runat="server" CssClass="sectionEven">
						<ul class="addRemoveSection">
							<li>
								<asp:CheckBox ID="chkHideDisabledBulkProducts" runat="server" Text="Hide disabled bulk products" AutoPostBack="true" />
							</li>
							<li>
								<asp:DropDownList ID="ddlBulkProduct" runat="server" AutoPostBack="True" CssClass="addRemoveList" />
								<asp:Button ID="btnAddBulkProduct" runat="server" Text="Add" Enabled="False" CssClass="addRemoveButton" />
							</li>
							<li>
								<asp:ListBox ID="lstBulkProducts" runat="server" AutoPostBack="True" CssClass="addRemoveList" Style="max-height: 400px;" />
								<asp:Button ID="btnRemoveBulkProduct" runat="server" Text="Remove" Enabled="False"
									CssClass="addRemoveButton" />
							</li>
						</ul>
					</asp:Panel>
					<asp:Panel ID="pnlSettings" runat="server" CssClass="sectionOdd">
						<h1>Panel settings for bulk product</h1>
						<ul>
							<li>&nbsp;</li>
							<li>
								<label>
									Bulk product
								</label>
								<asp:Label ID="lblBulkProductName" runat="server" CssClass="input"></asp:Label>
							</li>
							<li>
								<label>
									Panel function</label>
								<asp:DropDownList ID="ddlNumber" runat="server" />
							</li>
							<li>
								<label>
									Start parameter (flood time, start bumps)</label>
								<asp:TextBox ID="tbxStartParameter" runat="server" />
							</li>
							<li>
								<label>
									Finishing parameter (purge time)</label>
								<asp:TextBox ID="tbxFinishingParameter" runat="server" />
							</li>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="chkAlwaysUseFinishParameter" runat="server" Text="Always use purge time"
									Enabled="False" />
							</li>
							<li>
								<label>
									Anticipation</label>
								<span class="input">
									<asp:TextBox ID="tbxAnticipation" runat="server" Width="60%" />&nbsp;
					<asp:DropDownList ID="ddlAnticipationUnit" runat="server" Width="30%" />
								</span></li>
							<li>
								<label>
									Anticipation update factor (0 to 1)</label>
								<asp:TextBox ID="tbxAnticipationUpdateFactor" runat="server" />
							</li>
							<li>
								<label>
									Conversion factor</label>
								<span class="input">
									<asp:TextBox ID="tbxConversionFactor" runat="server" Width="30%" />
									pulses per
					<asp:DropDownList ID="ddlConversionFactorUnit" runat="server" Width="30%" />
								</span></li>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="chkUpdateDensityUsingMeter" runat="server" Text="Update density using meter" />
							</li>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="chkUseAverageDensityForTicket" runat="server" Text="Use average density for ticket" />
							</li>
							<li>
								<label>
									Dump time (sec)</label>
								<asp:TextBox ID="tbxDumpTime" runat="server" />
							</li>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="chkDisabled" runat="server" Text="Disabled" />
							</li>
						</ul>
					</asp:Panel>
					<asp:Panel ID="pnlStorageLocations" runat="server" CssClass="sectionOdd">
						<h1>Storage locations</h1>
						<div class="addRemoveSection">
							<asp:DropDownList ID="ddlStorageLocations" runat="server" CssClass="addRemoveList" AutoPostBack="true"></asp:DropDownList>
							<asp:Button ID="btnAddStorageLocations" runat="server" CssClass="addRemoveButton" Text="Add" />
							<asp:ListBox ID="lstStorageLocations" runat="server" CssClass="addRemoveList" AutoPostBack="true"></asp:ListBox>
							<asp:Button ID="btnRemoveStorageLocations" runat="server" CssClass="addRemoveButton" Text="Remove" />
						</div>
					</asp:Panel>
				</asp:Panel>
			</ContentTemplate>
		</asp:UpdatePanel>
	</form>
</body>
</html>
