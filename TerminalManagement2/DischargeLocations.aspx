<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="DischargeLocations.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.DischargeLocations" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Panels : Discharge Locations</title>
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
	</script>
</head>
<body>
	<form id="main" method="post" runat="server">
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
								Discharge location
							</label>
							<asp:DropDownList ID="ddlDischargeLocations" runat="server" AutoPostBack="True" />
						</li>
					</ul>
				</div>
				<div class="recordControl">
					<asp:Button ID="btnSave" runat="server" Text="Save" />
					<asp:Button ID="btnDelete" runat="server" Text="Delete" />
					<asp:Label ID="lblStatus" runat="server" ForeColor="#FF0000" />
					<span class="sectionRequiredField"><span class="required"></span>&nbsp;indicates required field </span>
				</div>
				<asp:Panel ID="pnlMain" runat="server" CssClass="section">
					<div class="sectionEven">
						<h1>General</h1>
						<ul>
							<li>
								<label>
									Name</label>
								<span class="required">
									<asp:TextBox ID="tbxName" runat="server" /></span> </li>
							<li>
								<label>
									Fill limit</label>
								<span class="input">
									<asp:TextBox ID="tbxFillLimit" runat="server" Width="60%" />
									<asp:DropDownList ID="ddlFillLimitUnit" runat="server" Width="30%" />
								</span></li>
							<li>
								<label>
									Secondary fill limit</label>
								<span class="input">
									<asp:TextBox ID="tbxSecondaryFillLimit" runat="server" Width="60%" />
									<asp:DropDownList ID="ddlSecondaryFillLimitUnit" runat="server" Width="30%" />
								</span></li>
							<li>
								<label>
									Bay</label>
								<asp:DropDownList ID="ddlBay" runat="server" />
							</li>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="cbxAcceptsBlends" runat="server" Text="Accepts blends" />
							</li>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="cbxConfirmEmpty" runat="server" Text="Confirm empty" />
							</li>
							<li>
								<label>
									Last ticket</label>
								<span class="input">
									<asp:Label ID="lblLastTicket" runat="server" />
									<asp:Button ID="btnClear" runat="server" Text="Clear" />
								</span></li>
						</ul>
					</div>
					<asp:Panel ID="pnlPanels" runat="server" CssClass="sectionOdd">
						<h1>Panels</h1>
						<ul class="addRemoveSection">
							<li>
								<asp:DropDownList ID="ddlPanel" runat="server" AutoPostBack="True" CssClass="addRemoveList" />
								<asp:Button ID="btnAddPanel" runat="server" Text="Add" CssClass="addRemoveButton" />
							</li>
							<li>
								<asp:ListBox ID="lstPanels" runat="server" AutoPostBack="True" CssClass="addRemoveList" />
								<asp:Button ID="btnRemovePanel" runat="server" Text="Remove" CssClass="addRemoveButton" />
							</li>
						</ul>
						<ul>
							<li>
								<label>
									Diverters</label>
								<span class="input">
									<asp:CheckBoxList ID="cblDiverters" runat="server" RepeatColumns="2" Enabled="False">
										<asp:ListItem>Diverter 1</asp:ListItem>
										<asp:ListItem>Diverter 2</asp:ListItem>
										<asp:ListItem>Diverter 3</asp:ListItem>
										<asp:ListItem>Diverter 4</asp:ListItem>
										<asp:ListItem>Diverter 5</asp:ListItem>
										<asp:ListItem>Diverter 6</asp:ListItem>
										<asp:ListItem>Diverter 7</asp:ListItem>
										<asp:ListItem>Diverter 8</asp:ListItem>
									</asp:CheckBoxList>
								</span></li>
							<li>
								<label>
									Purge time multiplier</label>
								<asp:TextBox ID="tbxPurgeTimeMultiplier" runat="server" Enabled="False" Text="1" />
							</li>
							<li>
								<label>
									Anticipation multiplier</label>
								<asp:TextBox ID="tbxAnticipationMultiplier" runat="server" Enabled="False" Text="1" />
							</li>
							<li>
								<label>
									&nbsp;
								</label>
								<asp:CheckBox ID="cbxFinalPanelAutoDischarge" runat="server" Text="Automatically discharge panel" />
							</li>
						</ul>
						<asp:Button ID="btnSavePanel" runat="server" Text="Save panel settings" Enabled="False" />
						<asp:Label ID="lblPanelStatus" runat="server" ForeColor="Red" />
					</asp:Panel>
					<asp:Panel ID="pnlStorageLocations" runat="server" CssClass="sectionOdd">
						<h1>Storage locations</h1>
						<div class="addRemoveSection">
							<asp:DropDownList ID="ddlStorageLocations" runat="server" CssClass="addRemoveList" AutoPostBack="True"></asp:DropDownList>
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
