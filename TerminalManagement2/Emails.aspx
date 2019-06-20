<%@ Page Language="vb" AutoEventWireup="true" CodeBehind="Emails.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.Emails" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Reports : E-mails</title>
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
			document.getElementById('pnlUpdatePanel').style.visibility = "hidden";
		}

		function onRequestDone(sender, args) {
			$find('PleaseWaitPopup').hide();
			document.getElementById('pnlUpdatePanel').style.visibility = "visible";

			if (args.get_error() != undefined) {
				var errorMessage = args.get_error().message
				args.set_errorHandled(true);
				alert(errorMessage);
			}
		}
	</script>
</head>
<body onload="resizeIframe('frmEmailBody');">
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
				<div id="pnlUpdatePanel">
					<div class="recordControl">
						<asp:Button ID="btnSave" runat="server" Text="Save" Width="110px" />
					</div>
					<div class="sectionEven">
						<ul>
							<li>
								<label>
									Show messages as old as a</label>
								<asp:DropDownList ID="ddlMaxMessageAge" runat="server" Width="70px" AutoPostBack="True">
									<asp:ListItem Text="day" Value="1"></asp:ListItem>
									<asp:ListItem Text="week" Value="7"></asp:ListItem>
									<asp:ListItem Text="month" Value="31"></asp:ListItem>
									<asp:ListItem Text="year" Value="365"></asp:ListItem>
								</asp:DropDownList>
							</li>
							<li>
								<label>
									Show sent messages</label>
								<asp:DropDownList ID="ddlShowSentMessages" runat="server" AutoPostBack="true">
									<asp:ListItem Text="No" Value="false"></asp:ListItem>
									<asp:ListItem Text="Yes" Value="true"></asp:ListItem>
								</asp:DropDownList>
							</li>
						</ul>
					</div>
					<div class="section">
						<ul>
							<li>
								<label>
								</label>
								<asp:ListBox ID="lstMessages" runat="server" Width="100%" AutoPostBack="True" Height="209px"></asp:ListBox>
							</li>
						</ul>
					</div>
					<div class="sectionEven">
						<ul>
							<li>
								<label>
									Sent</label>
								<input type="checkbox" id="cbxDeleted" runat="server" />
							</li>
							<li>
								<label>
									Subject</label>
								<asp:TextBox ID="tbxSubject" runat="server"></asp:TextBox>
							</li>
							<li>
								<label>
									To</label>
								<asp:TextBox ID="tbxRecipients" runat="server"></asp:TextBox>
							</li>
							<li>
								<label>
									Attachments</label>
								<span class="input">
									<span class="addRemoveSection">
										<asp:ListBox ID="lstAttachments" runat="server" AutoPostBack="True" CssClass="addRemoveList"></asp:ListBox>
										<asp:Button ID="btnOpenAttachment" runat="server" Text="Open" CssClass="addRemoveButton" />
										<asp:Button ID="btnDeleteAttachment" runat="server" Text="Delete" CssClass="addRemoveButton" />
									</span>
								</span>
							</li>
						</ul>
					</div>
					<div class="section">
						<iframe id="frmEmailBody" runat="server" style="width: 100%;"></iframe>
						<label id="errorDetails" style="width: 100%; text-align: left;">
						</label>
					</div>
				</div>
			</ContentTemplate>
		</asp:UpdatePanel>
	</form>
</body>
</html>
