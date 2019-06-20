<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="CustomAnalysisTemplate.aspx.vb" EnableViewState="true" Inherits="KahlerAutomation.TerminalManagement2.CustomAnalysisTemplate" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Fertilizer Analysis</title>
	<link rel="shortcut icon" href="FavIcon.ico" />
	<meta name="viewport" content="width=device-width" />
	<meta http-equiv="X-UA-Compatible" content="IE=edge" />
	<link rel="stylesheet" href="styles/site.css" />
	<link rel="stylesheet" href="styles/site-tablet.css" media="(max-width:768px)" />
	<link rel="stylesheet" href="styles/site-phone.css" media="(max-width:480px)" />
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
			document.getElementById('pnlAnalysis').style.visibility = "hidden";
		}

		function onRequestDone(sender, args) {
			$find('PleaseWaitPopup').hide();
			document.getElementById('pnlAnalysis').style.visibility = "visible";
			if (args.get_error() != undefined) {
				var errorMessage = args.get_error().message
				args.set_errorHandled(true);
				alert(errorMessage);
			}
		}
	</script>
</head>
<body>
	<form id="form1" runat="server">
		<div class="recordControl">
			<asp:Button ID="btnSave" runat="server" Text="Save" />
			<asp:Button ID="btnAddAnalysis" runat="server" Text="Add analysis" />
			<asp:Label ID="lblStatus" runat="server" ForeColor="Red"></asp:Label>
		</div>
		<asp:ScriptManager ID="ToolkitScriptManager1" runat="server" AsyncPostBackTimeout="3600" AsyncPostBackErrorMessage="Timeout while retrieving analysis data." EnablePartialRendering="true" OnAsyncPostBackError="ScriptManager1_AsyncPostBackError">
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
				<div id="pnlAnalysis" runat="server" class="section">
					<h1>
						<asp:Label ID="lblLastAnalysisAt" runat="server" Text="Last Analysis At:"></asp:Label>
					</h1>
					<asp:Table ID="tblAnalysisEntries" runat="server" Style="width: auto; min-width: 200px;"></asp:Table>
				</div>
			</ContentTemplate>
		</asp:UpdatePanel>
	</form>
</body>
</html>
