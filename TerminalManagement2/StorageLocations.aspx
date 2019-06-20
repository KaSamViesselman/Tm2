<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="StorageLocations.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.StorageLocations" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>Facilities : Storage Locations</title>
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
			$('#tbxCleanoutDate').datetimepicker({
				timeFormat: 'h:mm:ss TT',
				showSecond: true,
				showOn: "button",
				buttonImage: 'Images/Calendar_scheduleHS.png',
				buttonImageOnly: true,
				buttonText: "Show calendar"
			});
			$('#tbxTransferStart').datetimepicker({
				timeFormat: 'h:mm:ss TT',
				showSecond: true,
				showOn: "button",
				buttonImage: 'Images/Calendar_scheduleHS.png',
				buttonImageOnly: true,
				buttonText: "Show calendar"
			});
			$('#tbxTransferStop').datetimepicker({
				timeFormat: 'h:mm:ss TT',
				showSecond: true,
				showOn: "button",
				buttonImage: 'Images/Calendar_scheduleHS.png',
				buttonImageOnly: true,
				buttonText: "Show calendar"
			});
			$('#tbxStorageLocationContentsAsOfDate').datetimepicker({
				timeFormat: 'h:mm:ss TT',
				showSecond: true,
				showOn: "button",
				buttonImage: 'Images/Calendar_scheduleHS.png',
				buttonImageOnly: true,
				buttonText: "Show calendar"
			});
			if (args.get_error() != undefined) {
				var errorMessage = args.get_error().message
				args.set_errorHandled(true);
				alert(errorMessage);
			}
		}
	</script>
</head>
<body>
	<form id="main" method="post" runat="server" defaultfocus="tbxname">
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
						<label style="vertical-align: top;">
							Show storage locations of type
						</label>
						<asp:RadioButtonList ID="rblStorageLocationType" runat="server" AutoPostBack="true" CssClass="input" RepeatLayout="UnorderedList" Style="display: inline-block; width: 30%;">
							<asp:ListItem Text="Bulk product storage" Value="BulkProductStorage" Selected="True"></asp:ListItem>
							<asp:ListItem Text="Tank storage" Value="TankStorage"></asp:ListItem>
							<asp:ListItem Text="Container storage" Value="ContainerStorage"></asp:ListItem>
						</asp:RadioButtonList>
					</li>
					<li>
						<hr />
					</li>
					<li>
						<label>
							Storage Location</label>
						<asp:DropDownList ID="ddlStorageLocations" runat="server" AutoPostBack="True" />
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
										Name</label>
									<span class="required">
										<asp:TextBox ID="tbxName" TabIndex="1" runat="server" /></span>
								</li>
								<li id="pnlLocation" runat="server">
									<label>
										Facility
									</label>
									<asp:DropDownList ID="ddlLocation" runat="server" AutoPostBack="true"></asp:DropDownList>
								</li>
								<li id="pnlTank" runat="server">
									<label>
										Tank
									</label>
									<asp:DropDownList ID="ddlTank" runat="server" AutoPostBack="true"></asp:DropDownList>
								</li>
								<li id="pnlContainer" runat="server">
									<label>
										Container
									</label>
									<asp:DropDownList ID="ddlContainer" runat="server" AutoPostBack="true"></asp:DropDownList>
								</li>
							</ul>
							<ul id="lstCustomFields" runat="server">
							</ul>
						</div>
						<div class="sectionOdd" id="plnStorageLocationFunctions" runat="server">
							<div class="section">
								<h1>Clean-out/empty</h1>
								<ul>
									<li id="pnlDateOfLastCleanout" runat="server">
										<label>
											Date of last clean-out entry
										</label>
										<asp:Label ID="lblDateOfLastCleanout" runat="server" CssClass="input"></asp:Label>
									</li>
									<li>
										<label>
											New clean-out date</label>
										<input type="text" name="tbxCleanoutDate" id="tbxCleanoutDate" value="" runat="server" />
									</li>
									<li>
										<label>
											&nbsp;
										</label>
										<asp:Button ID="btnMarkedAsCleanedOut" runat="server" Text="Mark as cleaned out" CssClass="input" Style="width: auto; min-width: 100px;" />
									</li>
								</ul>
							</div>
							<div class="section">
								<h1>Storage location transfer</h1>
								<ul>
									<li>
										<label>
											Transfer to
										</label>
										<asp:DropDownList ID="ddlTransferToStorageLocation" runat="server"></asp:DropDownList>
									</li>
									<li>
										<label>
											Transfer start</label>
										<input type="text" name="tbxTransferStart" id="tbxTransferStart" value="" runat="server" />
									</li>
									<li>
										<label>
											Transfer end</label>
										<input type="text" name="tbxTransferStop" id="tbxTransferStop" value="" runat="server" />
									</li>
									<li>
										<label>
											&nbsp;
										</label>
										<asp:Button ID="btnTransferToStorageLocation" runat="server" Text="Transfer" CssClass="input" Style="width: auto; min-width: 100px;" />
									</li>
								</ul>
							</div>
						</div>
					</div>
					<div class="section" id="pnlStorageLocationContentsAsOfDate" runat="server">
						<hr />
						<div class="sectionEven">
							<h2>Current sources</h2>
							<ul>
								<li>
									<label>
										Date to check</label>
									<input type="text" name="tbxStorageLocationContentsAsOfDate" id="tbxStorageLocationContentsAsOfDate" value="" runat="server" />
								</li>
								<li>
									<label>
									</label>
									<asp:Button ID="btnStorageLocationContentsAsOfDate" runat="server" Text="Display" Style="width: auto; min-width: 75px;" />
								</li>
							</ul>
						</div>
						<asp:Literal ID="litStorageLocationContentsAsOfDate" runat="server"></asp:Literal>
					</div>
				</asp:Panel>
			</ContentTemplate>
		</asp:UpdatePanel>
	</form>
</body>
</html>
