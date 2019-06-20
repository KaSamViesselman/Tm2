<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="TankLevelTrends.aspx.vb"
	Inherits="KahlerAutomation.TerminalManagement2.TankLevelTrends" MaintainScrollPositionOnPostback="true" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Tanks : Tank Level Trends</title>
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
			document.getElementById('pnlEven').style.visibility = "hidden";
			document.getElementById('pnlData').style.visibility = "hidden";
		}

		function onRequestDone(sender, args) {
			$find('PleaseWaitPopup').hide();
			document.getElementById('pnlEven').style.visibility = "visible";
			document.getElementById('pnlData').style.visibility = "visible";

			if (args.get_error() != undefined) {
				var errorMessage = args.get_error().message
				args.set_errorHandled(true);
				alert(errorMessage);
			}
		}

		function SetCalendarPickers() {
			$('#tbxBeginningAtDate').datetimepicker({
				timeFormat: 'h:mm:ss TT',
				showSecond: true,
				showOn: "button",
				buttonImage: 'Images/Calendar_scheduleHS.png',
				buttonImageOnly: true,
				buttonText: "Show calendar"
			});

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
	<form id="main" runat="server">
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
					<label>
						Tank level trend</label>
					<asp:DropDownList ID="ddlTankLevelTrends" runat="server" AutoPostBack="True" />
				</div>
				<div class="recordControl">
					<asp:Button ID="btnSave" runat="server" Text="Save" />
					<asp:Button ID="btnDelete" runat="server" Text="Delete" />
					<asp:Label ID="lblStatus" runat="server" ForeColor="Red" />
					<div class="sectionRequiredField">
						<label>
							<span class="required"></span>&nbsp;indicates required field
						</label>
					</div>
				</div>
				<asp:Panel ID="pnlEven" runat="server" CssClass="sectionEven">
					<ul>
						<li>
							<label>
								Name</label>
							<span class="required">
								<asp:TextBox ID="tbxName" runat="server" MaxLength="50" /></span> </li>
						<li>
							<label>
								Tank</label>
							<span class="required">
								<asp:DropDownList ID="ddlTank" runat="server" />
							</span></li>
						<li>
							<label>
								Interval</label>
							<span class="input">
								<asp:TextBox ID="tbxInterval" runat="server" Style="width: 40%;" />
								<asp:DropDownList ID="ddlPeriod" runat="server" Style="width: 40%;" />
							</span></li>
						<li>
							<label>
								Beginning at</label>
							<div class="input">
								<span class="required">
									<input type="text" name="tbxBeginningAtDate" id="tbxBeginningAtDate" value="" runat="server" />
								</span>
							</div>
						</li>
					</ul>
				</asp:Panel>
				<div id="pnlData">
					<asp:Panel ID="pnlTankLevelTrendData" runat="server" CssClass="section">
						<hr style="width: 100%; color: #003399;" />
						<h2>Tank level trend data filters</h2>
						<div class="sectionEven">
							<ul>
								<li>
									<label>
										From
									</label>
									<input type="text" name="tbxFromDate" id="tbxFromDate" value="" runat="server" />
								</li>
							</ul>
						</div>
						<div class="sectionOdd">
							<ul>
								<li>
									<label>
										To
									</label>
									<input type="text" name="tbxToDate" id="tbxToDate" value="" runat="server" />
								</li>
							</ul>
						</div>
						<div class="sectionEven">
							<ul>
								<li>
									<label>
										Display Unit
									</label>
									<asp:DropDownList ID="ddlDisplayUnit" runat="server" AutoPostBack="true">
									</asp:DropDownList>
								</li>
							</ul>
						</div>
						<div class="sectionOdd">
							<ul>
								<li>
									<label>
										&nbsp;
									</label>
									<asp:CheckBox ID="cbxDisplayTemperature" runat="server" Text="Display temperature" />
								</li>
							</ul>
						</div>
						<div class="section" style="background-color: #eeeeee;">
							<asp:Button ID="btnShowData" runat="server" Text="Show data" Style="width: auto;" />
							<asp:Button ID="btnDownload" runat="server" Text="Download" Style="width: auto;" />
							<asp:Button ID="btnPrinterFriendly" runat="server" Text="Printer friendly" Style="width: auto;" />
						</div>
						<asp:Panel ID="pnlTankLevelTrendDataDisplay" runat="server" CssClass="section">
							<hr style="width: 100%; color: #003399;" />
							<img id="imgGraph" runat="server" src="data:image/png;base64,0" />
							<br />
							<asp:Literal ID="litData" runat="server"></asp:Literal>
						</asp:Panel>
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

					</asp:Panel>
				</div>
			</ContentTemplate>
		</asp:UpdatePanel>
	</form>
</body>
</html>
