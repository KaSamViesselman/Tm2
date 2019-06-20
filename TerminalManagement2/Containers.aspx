<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="Containers.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.Containers"
	EnableSessionState="True" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Containers : Containers</title>
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
			document.getElementById('containerPanel').style.visibility = "hidden";
		}

		function onRequestDone(sender, args) {
			$find('PleaseWaitPopup').hide();
			document.getElementById('containerPanel').style.visibility = "visible";

			if (args.get_error() != undefined) {
				var errorMessage = args.get_error().message
				args.set_errorHandled(true);
				alert(errorMessage);
			}
		}

		function SetCalendarPickers() {
			$('#tbxLastCleanedDate').datetimepicker({
				timeFormat: 'h:mm:ss TT',
				showSecond: true,
				showOn: "button",
				buttonImage: 'Images/Calendar_scheduleHS.png',
				buttonImageOnly: true,
				buttonText: "Show calendar"
			});

			$('#tbxInServiceDate').datetimepicker({
				timeFormat: 'h:mm:ss TT',
				showSecond: true,
				showOn: "button",
				buttonImage: 'Images/Calendar_scheduleHS.png',
				buttonImageOnly: true,
				buttonText: "Show calendar"
			});

			$('#tbxLastInspectedDate').datetimepicker({
				timeFormat: 'h:mm:ss TT',
				showSecond: true,
				showOn: "button",
				buttonImage: 'Images/Calendar_scheduleHS.png',
				buttonImageOnly: true,
				buttonText: "Show calendar"
			});

			$('#tbxManufacturedDate').datetimepicker({
				timeFormat: 'h:mm:ss TT',
				showSecond: true,
				showOn: "button",
				buttonImage: 'Images/Calendar_scheduleHS.png',
				buttonImageOnly: true,
				buttonText: "Show calendar"
			});

			$('#tbxLastFilledDate').datetimepicker({
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
				<div id="containerPanel">
					<div class="recordSelection" id="recordSelection" runat="server">
						<ul>
							<li>
								<label>
									Facility</label>
								<asp:DropDownList ID="ddlFacilityFilter" runat="server" AutoPostBack="true">
								</asp:DropDownList>
							</li>
							<li>
								<label>
									Container</label>
								<asp:DropDownList ID="ddlContainers" runat="server" AutoPostBack="True">
								</asp:DropDownList>
							</li>
						</ul>
					</div>
					<div class="recordControl" id="recordControl" runat="server">
						<asp:Button ID="btnSave" runat="server" Text="Save" />
						<asp:Button ID="btnDelete" runat="server" Text="Delete" />
						<asp:Label ID="lblStatus" runat="server" ForeColor="#ff0000"></asp:Label>
						<span class="sectionRequiredField"><span class="required"></span>&nbsp;indicates required
			field </span>
					</div>
					<asp:Panel ID="pnlMain" runat="server" CssClass="section">
						<div class="sectionEven" id="general1" runat="server">
							<asp:Label ID="lblContainerDeletedStatus" runat="server" ForeColor="#ff0000" Visible="false">Container is marked as deleted</asp:Label>
							<h1>General</h1>
							<ul>
								<li>
									<label>
										Number</label>
									<span class="required">
										<asp:TextBox ID="tbxNumber" runat="server" MaxLength="50"></asp:TextBox>
									</span></li>
								<li>
									<label>
										Container type</label>
									<asp:DropDownList ID="ddlContainerType" runat="server">
									</asp:DropDownList>
								</li>
								<li>
									<label>
										Volume</label>
									<asp:TextBox ID="tbxVolume" runat="server" Width="45%"></asp:TextBox>
									<asp:DropDownList ID="ddlVolume" runat="server" Width="15%">
									</asp:DropDownList>
								</li>
								<li>
									<label>
										Empty weight</label>
									<asp:TextBox ID="tbxEmptyWeight" runat="server" Width="45%"></asp:TextBox>
									<asp:DropDownList ID="ddlWeight" runat="server" AutoPostBack="True" Width="15%">
									</asp:DropDownList>
								</li>
								<li>
									<label>
										Condition
									</label>
									<asp:DropDownList ID="ddlCondition" runat="server">
									</asp:DropDownList>
								</li>
								<li>
									<label>
										Seal number
									</label>
									<asp:TextBox ID="tbxSealNumber" runat="server"></asp:TextBox>
								</li>
								<li>
									<label>
										Notes
									</label>
									<asp:TextBox ID="tbxNotes" runat="server" TextMode="MultiLine"></asp:TextBox>
								</li>
								<li>
									<label>
										In service
									</label>
									<span class="required">
										<input type="text" name="tbxInServiceDate" id="tbxInServiceDate" value="" runat="server"
											style="width: 45%;" />
									</span>
								</li>
								<li>
									<label>
										Last inspected
									</label>
									<span class="required">
										<input type="text" name="tbxLastInspectedDate" id="tbxLastInspectedDate" value=""
											runat="server" style="width: 45%;" />
									</span>
								</li>
								<li>
									<label>
										Passed inspection
									</label>
									<asp:RadioButtonList ID="rblPassedInspection" runat="server" RepeatColumns="2" RepeatLayout="Flow">
										<asp:ListItem Text="Yes"></asp:ListItem>
										<asp:ListItem Text="No"></asp:ListItem>
									</asp:RadioButtonList>
								</li>
								<li>
									<label>
										Manufactured
									</label>
									<span class="required">
										<input type="text" name="tbxManufacturedDate" id="tbxManufacturedDate" value="" runat="server"
											style="width: 45%;" />
									</span>
								</li>
								<li>
									<label></label>
									<span class="input">
										<asp:LinkButton ID="lbtHistory" runat="server" Text="History"></asp:LinkButton>
									</span>
								</li>
							</ul>
						</div>
						<div class="sectionOdd" id="general2" runat="server">
							<ul>
								<li>
									<label>
										Owner
									</label>
									<asp:DropDownList ID="ddlOwner" runat="server" AutoPostBack="True">
									</asp:DropDownList>
								</li>
								<li>
									<label>
										Facility
									</label>
									<asp:DropDownList ID="ddlLocation" runat="server" AutoPostBack="true">
									</asp:DropDownList>
								</li>
								<li>
									<label>
										Status
									</label>
									<asp:DropDownList ID="ddlStatus" runat="server">
									</asp:DropDownList>
								</li>
								<li>
									<label>
										Last filled
									</label>
									<span class="required">
										<input type="text" name="tbxLastFilledDate" id="tbxLastFilledDate" value="" runat="server"
											style="width: 45%" />
									</span>
								</li>
								<li>
									<label>
										Last cleaned
									</label>
									<input type="text" name="tbxLastCleanedDate" id="tbxLastCleanedDate" value="" runat="server"
										style="width: 45%;" />
								</li>
								<li>
									<label>
										Bulk product
									</label>
									<asp:DropDownList ID="ddlBulkProduct" runat="server" AutoPostBack="true">
									</asp:DropDownList>
								</li>
								<li>
									<label>
										Product weight
									</label>
									<asp:TextBox ID="tbxProductWeight" runat="server" Width="45%"></asp:TextBox>
									<asp:Label ID="lblProductWeight" runat="server" Width="15%"></asp:Label>
								</li>
								<li>
									<label>
										For order
									</label>
									<asp:DropDownList ID="ddlOrders" runat="server">
									</asp:DropDownList>
								</li>
								<li>
									<label>
										Last ticket
									</label>
									<asp:Label ID="lblLastTicket" runat="server"></asp:Label>&nbsp;
				<asp:Button ID="btnClearLastTicket" runat="server" Text="Clear" Width="20%" />
								</li>
								<li>
									<label>
										Refillable
									</label>
									<asp:RadioButtonList ID="rblRefillable" runat="server" RepeatColumns="2" RepeatLayout="Flow">
										<asp:ListItem Text="Yes"></asp:ListItem>
										<asp:ListItem Text="No"></asp:ListItem>
									</asp:RadioButtonList>
								</li>
								<li>
									<label>
										Passed pressure test
									</label>
									<asp:RadioButtonList ID="rblPassedPressureTest" runat="server" RepeatColumns="2"
										RepeatLayout="Flow">
										<asp:ListItem Text="Yes"></asp:ListItem>
										<asp:ListItem Text="No"></asp:ListItem>
									</asp:RadioButtonList>
								</li>
								<li>
									<label>
										Seal broken
									</label>
									<asp:RadioButtonList ID="rblSealBroken" runat="server" RepeatColumns="2" RepeatLayout="Flow">
										<asp:ListItem Text="Yes"></asp:ListItem>
										<asp:ListItem Text="No"></asp:ListItem>
									</asp:RadioButtonList>
								</li>
								<li>
									<label>
										One-way valve present
									</label>
									<asp:RadioButtonList ID="rblOneWayValvePresent" runat="server" RepeatColumns="2"
										RepeatLayout="Flow">
										<asp:ListItem Text="Yes"></asp:ListItem>
										<asp:ListItem Text="No"></asp:ListItem>
									</asp:RadioButtonList>
								</li>
								<li id="pnlLot" runat="server">
									<label>
										Lot number
									</label>
									<asp:DropDownList ID="ddlLot" runat="server" AutoPostBack="true"></asp:DropDownList>
								</li>
								<li id="pnlNewLot" runat="server">
									<label>
										New lot name
									</label>
									<asp:TextBox ID="tbxNewLotNumber" runat="server"></asp:TextBox>
								</li>

							</ul>
						</div>
						<div class="recordControl" id="productChangedRecordControl" runat="server" visible="false">
							<asp:Button ID="btnProductChangedSave" runat="server" Text="Save" />
							<asp:Button ID="btnProductChangedSaveWithoutInventoryAdjustment" runat="server" Text="Save Without Inventory Adjustment" />
							<asp:Button ID="btnProductChangedCancel" runat="server" Text="Cancel" />
							<asp:Label ID="lblProductChangedStatus" runat="server" Text="" ForeColor="Red" />
						</div>
						<div class="section" id="productChanged" runat="server" visible="false">
							<h1>Product Change</h1>
							<asp:TextBox ID="tbxProductChangedType" runat="server" Visible="false" />
							<asp:Label ID="lblProductChangedDetails" runat="server" />
							<br />
							<br />
							<br />
							<ul>
								<li>
									<label>Default owner</label>
									<asp:DropDownList ID="ddlProductChangedDefaultOwner" runat="server" />
								</li>
								<li>
									<label>Facility Inventory</label>
									<asp:DropDownList ID="ddlProductChangedFacility" runat="server" />
								</li>
								<li>
									<label>Packaged Inventory</label>
									<asp:DropDownList ID="ddlProductChangedPackaged" runat="server" />
								</li>
							</ul>
						</div>
					</asp:Panel>
				</div>
			</ContentTemplate>
		</asp:UpdatePanel>
	</form>
</body>
</html>
