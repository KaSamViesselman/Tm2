<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="PanelGroups.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.PanelGroups" %>

<!DOCTYPE html>
<html lang="en">
<head>
    <title>Panels : Panel Groups</title>
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
    <form id="main" runat="server">
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
                    <label>
                        Panel Group</label>
                    <asp:DropDownList ID="ddlRecords" runat="server" AutoPostBack="true" />
                </div>
                <div class="recordControl">
                    <asp:Button ID="btnSave" runat="server" Text="Save" />
                    <asp:Button ID="btnDelete" runat="server" Text="Delete" /><asp:Label ID="lblMessage" runat="server" Text="" ForeColor="Red"></asp:Label>
                </div>
                <asp:Panel ID="pnlMain" runat="server" CssClass="section">
                    <div class="sectionEven">
                        <h1>Panel Group</h1>
                        <ul>
                            <li>
                                <label>
                                    Name</label>
                                <asp:TextBox ID="tbxName" runat="server" />
                            </li>
                            <li>
                                <label>
                                </label>
                                <asp:CheckBox ID="cbxCannotFillSimultaneously" Text="Cannot fill simultaneously" runat="server" />
                            </li>
                        </ul>
                    </div>
                    <div class="sectionOdd">
                        <h1>Members</h1>
                        <ul class="addRemoveSection">
                            <li>
                                <asp:DropDownList ID="ddlPanel" runat="server" AutoPostBack="true" class="addRemoveList" />
                                <asp:Button ID="btnAddPanel" runat="server" Text="Add" class="addRemoveButton" />
                            </li>
                            <li>
                                <asp:ListBox ID="lstPanels" runat="server" AutoPostBack="true" class="addRemoveList" />
                                <asp:Button ID="btnRemovePanel" runat="server" Text="Remove" class="addRemoveButton" />
                            </li>
                        </ul>
                    </div>
                </asp:Panel>
            </ContentTemplate>
        </asp:UpdatePanel>
    </form>
</body>
</html>
