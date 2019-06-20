﻿<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="DefaultReceivingWebTicketSettings.aspx.vb"
	MaintainScrollPositionOnPostback="true" Inherits="KahlerAutomation.TerminalManagement2.DefaultReceivingWebTicketSettings" %>

<!DOCTYPE html>
<html lang="en">
<head id="Head1" runat="server">
	<title>General Settings : Default Receiving Web Ticket Settings</title>
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
			document.getElementById('pnlRecordControl').style.visibility = "hidden";
		}

		function onRequestDone(sender, args) {
			$find('PleaseWaitPopup').hide();
			document.getElementById('pnlRecordControl').style.visibility = "visible";

			if (args.get_error() != undefined) {
				var errorMessage = args.get_error().message
				args.set_errorHandled(true);
				alert(errorMessage);
			}
		}
	</script>
</head>
<body>
	<form id="main" method="post" runat="server" defaultfocus="cbxSeparateTicketNumberPerOwner">
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
				<div class="recordSelection">
					<ul>
						<li>
							<label>
								Ticket owner
							</label>
							<asp:DropDownList ID="ddlWebTicketOwner" runat="server" AutoPostBack="true">
							</asp:DropDownList>
						</li>
						<li>
							<label>
							</label>
							<asp:Label ID="lblWebTicketSettingsExist" runat="server" Text="Settings exist" ForeColor="#FF3300"
								Visible="False"></asp:Label>
						</li>
					</ul>
				</div>
				<div class="recordControl" id="pnlRecordControl">
					<asp:Button ID="btnSaveOwnerWebTicketSettings" runat="server" Text="Save Owner Web Ticket Settings" />
					<asp:Button ID="btnDeleteOwnerWebTicketSettings" runat="server" Text="Delete Owner Web Ticket Settings" />
				</div>
				<div class="section">
					<div class="sectionEven">
						<ul>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="cbxShowSupplier" runat="server" Text="Show supplier" />
							</li>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="cbxShowCarrierId" runat="server" Text="Show carrier" />
							</li>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="cbxShowDate" runat="server" Text="Show date and " AutoPostBack="true" />
								<asp:CheckBox ID="cbxShowTime" runat="server" Text="show time" />
							</li>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="cbxShowDensityOnTicket" runat="server" Text="Show density on ticket" />
							</li>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="cbxShowDriverName" runat="server" AutoPostBack="true" Text="Show driver" />
							</li>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="cbxShowDriverNumber" runat="server" Text="Show driver number" Style="margin-left: 2em;" />
							</li>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="cbxShowEmailAddress" runat="server" Text="Show e-mail address" />
							</li>
							<li id="pnlShowLotNumber" runat="server">
								<label>
								</label>
								<asp:CheckBox ID="cbxShowLotNumber" runat="server" Text="Show lot number" />
							</li>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="cbxShowOwner" runat="server" Text="Show owner" />
							</li>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="cbxShowTransport" runat="server" Text="Show transport" AutoPostBack="true"></asp:CheckBox>
							</li>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="chkShowTicketTransportTareInfo" runat="server" Text="Show transport tare information"
									AutoPostBack="true" Style="margin-left: 2em;" />
							</li>
							<li id="tblTransportTareOptions" runat="server">
								<label>
								</label>
								<div class="input" style="margin-left: 2em;">
									<ul>
										<li id="rowTransportTareFirstOption" runat="server">
											<label>
												First position
											</label>
											<asp:DropDownList ID="ddlTransportTareFirstOption" runat="server" AutoPostBack="true">
											</asp:DropDownList>
										</li>
										<li id="rowTransportTareSecondOption" runat="server">
											<label>
												Second position
											</label>
											<asp:DropDownList ID="ddlTransportTareSecondOption" runat="server" AutoPostBack="true">
											</asp:DropDownList>
										</li>
										<li id="rowTransportTareThirdOption" runat="server">
											<label>
												Third position
											</label>
											<asp:DropDownList ID="ddlTransportTareThirdOption" runat="server">
											</asp:DropDownList>
										</li>
									</ul>
								</div>
							</li>
						</ul>
					</div>
					<div class="sectionOdd">
						<ul>
							<li>
								<label>
									Show additional units
								</label>
								<asp:CheckBoxList ID="cblAdditionalUnitsForTicket" runat="server" RepeatLayout="UnorderedList"
									CssClass="input">
								</asp:CheckBoxList>
							</li>
							<li>
								<label>
									Density unit precision
								</label>
								<ul class="input">
									<li class="addRemoveSection"><span class="addRemoveList">
										<asp:DropDownList ID="ddlWebTicketDensityMass" runat="server" AutoPostBack="True"
											Style="width: auto;">
										</asp:DropDownList>
										&nbsp;/
							<asp:DropDownList ID="ddlWebTicketDensityVolume" runat="server" AutoPostBack="True"
								Style="width: auto;">
							</asp:DropDownList>
									</span>
										<asp:Button ID="btnWebTicketDensityAdd" runat="server" Text="Add" CssClass="addRemoveButton" />
									</li>
									<li class="addRemoveSection">
										<asp:ListBox ID="lstWebTicketDensityList" runat="server" AutoPostBack="True" CssClass="addRemoveList"></asp:ListBox>
										<asp:Button ID="btnWebTicketDensityRemove" runat="server" Text="Remove" CssClass="addRemoveButton" />
										<ul class="addRemoveButton" id="trWebTicketDensityPrecisionControls" runat="server"
											visible="false">
											<li>
												<ul>
													<li>
														<label>
															Whole
														</label>
														<span class="input">
															<asp:Button ID="btnWebTicketDensityAddWhole" runat="server" Text="+" CssClass="button"
																Style="width: auto;" />
															<asp:Button ID="btnWebTicketDensityRemoveWhole" runat="server" Text="-" CssClass="button"
																Style="width: auto;" /></span> </li>
													<li>
														<label>
															Fractional
														</label>
														<span class="input">
															<asp:Button ID="btnWebTicketDensityAddFractional" runat="server" Text="+" CssClass="button"
																Style="width: auto;" />
															<asp:Button ID="btnWebTicketDensityRemoveFractional" runat="server" Text="-" CssClass="button"
																Style="width: auto;" /></span> </li>
												</ul>
											</li>
										</ul>
									</li>
								</ul>
							</li>
							<li>
								<label>
									Ticket logo
								</label>
								<asp:TextBox ID="tbxTicketLogo" runat="server"></asp:TextBox>
							</li>
							<li>
								<label>
									Owner message
								</label>
								<asp:TextBox ID="tbxOwnerMessage" runat="server" Text="" TextMode="MultiLine"></asp:TextBox>
							</li>
							<li>
								<label>
									Disclaimer
								</label>
								<asp:TextBox ID="tbxOwnerDisclaimer" runat="server" Text="" TextMode="MultiLine"></asp:TextBox>
							</li>
							<li id="pnlReceivingTicketCustomFieldsAssigned" runat="server" visible="false">
								<label>
									Show custom fields on receiving ticket
								</label>
								<span class="input">
									<asp:CheckBox ID="cbxShowAllCustomFieldsOnReceivingTicket" runat="server" Text="Show all custom fields on receiving ticket"
										AutoPostBack="true" />
									<asp:CheckBoxList ID="cblShowCustomFieldsOnReceivingTicket" runat="server" Enabled="false"
										RepeatLayout="UnorderedList" Style="border: 1px solid black !important;">
									</asp:CheckBoxList>
								</span></li>
						</ul>
					</div>
				</div>
			</ContentTemplate>
		</asp:UpdatePanel>
	</form>
</body>
</html>
