<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="BulkProducts.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.BulkProducts" %>

<!DOCTYPE html>
<html lang="en">
<head id="Head1" runat="server">
	<title>Products : Bulk Products</title>
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
			document.getElementById('pnlGeneralInformation').style.visibility = "hidden";
			document.getElementById('pnlInterfaces').style.visibility = "hidden";
		}

		function onRequestDone(sender, args) {
			$find('PleaseWaitPopup').hide();
			document.getElementById('pnlRecordControl').style.visibility = "visible";
			document.getElementById('pnlGeneralInformation').style.visibility = "visible";
			document.getElementById('pnlInterfaces').style.visibility = "visible";

			if (args.get_error() != undefined) {
				var errorMessage = args.get_error().message
				args.set_errorHandled(true);
				alert(errorMessage);
			}
		}
	</script>
</head>
<body>
	<form id="main" method="post" runat="server" defaultfocus="tbxName">
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
						Bulk Product
					</label>
					<asp:DropDownList ID="ddlBulkProducts" runat="server" AutoPostBack="True">
					</asp:DropDownList>
				</div>
				<div id="pnlRecordControl" class="recordControl">
					<asp:Button ID="btnSave" runat="server" Text="Save" />
					<asp:Button ID="btnDelete" runat="server" Text="Delete" Enabled="False" />
					<asp:Label ID="lblStatus" runat="server" ForeColor="Red"></asp:Label>
					<span class="sectionRequiredField"><span class="required"></span>&nbsp;indicates required field </span>
				</div>
				<asp:Panel ID="pnlGeneralInformation" runat="server" CssClass="sectionEven">
					<h1>General</h1>
					<ul>
						<li>
							<label>
								Name
							</label>
							<span class="required">
								<asp:TextBox ID="tbxName" runat="server" MaxLength="50"></asp:TextBox>
							</span></li>
						<li>
							<label>
								Owner
							</label>
							<asp:DropDownList ID="ddlOwner" runat="server">
							</asp:DropDownList>
						</li>
						<li>
							<label>
								Default unit
							</label>
							<span class="required">
								<asp:DropDownList ID="ddlUnit" runat="server">
								</asp:DropDownList>
							</span></li>
						<li>
							<label>
								Density
							</label>
							<span class="input" style="display: inline-block;">
								<asp:TextBox ID="tbxDensity" runat="server" Style="width: 30%;"></asp:TextBox>
								<asp:DropDownList ID="ddlUnitOfWeight" runat="server" Style="width: 25%;">
								</asp:DropDownList>
								/
					<asp:DropDownList ID="ddlUnitOfVolume" runat="server" Style="width: 25%;">
					</asp:DropDownList>
							</span></li>
						<li>
							<label>
								EPA number
							</label>
							<asp:TextBox ID="tbxEPA" runat="server" MaxLength="50"></asp:TextBox>
						</li>
						<li>
							<label>
								Barcode number
							</label>
							<asp:TextBox ID="tbxBarcode" runat="server" MaxLength="50"></asp:TextBox>
						</li>
						<li>
							<label>
								<asp:Label ID="lblCropTypes" runat="server" Text="Crop types" Visible="false"></asp:Label>
							</label>
							<asp:CheckBoxList ID="cblCropTypes" runat="server" CssClass="input" RepeatLayout="UnorderedList">
							</asp:CheckBoxList>
						</li>
						<li>
							<label>
								Notes
							</label>
							<asp:TextBox ID="tbxNotes" runat="server" TextMode="MultiLine"></asp:TextBox>
						</li>
						<li class="addRemoveSection">
							<label>Derived from</label>
							<div class="input">
								<asp:ListBox ID="lstDerivedFrom" runat="server" CssClass="addRemoveList" AutoPostBack="true"></asp:ListBox>
								<asp:Button ID="btnAddNewDerivedFrom" runat="server" CssClass="addRemoveButton" Text="Add" />
								<asp:Button ID="btnRemoveNewDerivedFrom" runat="server" CssClass="addRemoveButton" Text="Remove" />
							</div>
						</li>
						<li class="addRemoveSection">
							<label>
								&nbsp;
							</label>
							<div class="input">
								<asp:TextBox ID="tbxDerivedFrom" runat="server" CssClass="addRemoveList"></asp:TextBox>
								<asp:Button ID="btnUpdateDerivedFrom" runat="server" CssClass="addRemoveButton" Text="Update" />
							</div>
						</li>
						<li id="pnlLotUsageTrackingType" runat="server">
							<label>
								Track specific lot usage during dispensing
							</label>
							<asp:RadioButtonList ID="rblLotUsageTrackingType" runat="server" RepeatLayout="UnorderedList" CssClass="input">
								<asp:ListItem Text="Do not track" Selected="True"></asp:ListItem>
								<asp:ListItem Text="FIFO" Selected="True"></asp:ListItem>
								<asp:ListItem Text="LIFO" Selected="True"></asp:ListItem>
							</asp:RadioButtonList>
						</li>
					</ul>
					<ul id="lstCustomFields" runat="server">
					</ul>
				</asp:Panel>
				<div id="pnlInterfaces" runat="server" class="sectionOdd">
					<div id="pnlInterfaceSetup" runat="server" class="section">
						<h2>Interface</h2>
						<div class="recordSelectionEvenOdd">
							<label>
								Interface setting</label>
							<asp:DropDownList ID="ddlBulkProductInterface" runat="server" AutoPostBack="True">
							</asp:DropDownList>
						</div>
						<asp:Panel ID="pnlInterfaceSettings" runat="server" CssClass="section">
							<ul>
								<li>
									<label>
										Interface
									</label>
									<span class="required">
										<asp:DropDownList ID="ddlInterface" runat="server" AutoPostBack="true">
										</asp:DropDownList>
									</span></li>
								<li>
									<label>
										Cross reference
									</label>
									<span class="required">
										<asp:TextBox ID="tbxInterfaceCrossReference" runat="server" MaxLength="50"></asp:TextBox>
									</span></li>
								<li id="pnlInterfaceExchangeUnit" runat="server">
									<label>
										Interface exchange unit
									</label>
									<asp:DropDownList ID="ddlInterfaceUnit" runat="server">
									</asp:DropDownList>
								</li>
								<li>
									<label>
									</label>
									<asp:CheckBox ID="chkDefaultSetting" runat="server" Text="Default setting" />
								</li>
								<li>
									<label>
									</label>
									<asp:CheckBox ID="chkExportOnly" runat="server" Text="Export only" />
								</li>
							</ul>
							<asp:Button ID="btnSaveInterface" runat="server" Text="Save" CssClass="recordInterfaceButton" />
							<asp:Button ID="btnRemoveInterface" runat="server" Text="Remove" CssClass="recordInterfaceButton" />
						</asp:Panel>
					</div>
				</div>
			</ContentTemplate>
		</asp:UpdatePanel>
	</form>
</body>
</html>
