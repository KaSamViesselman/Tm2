<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="PanelBulkProductFillLimits.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.PanelBulkProductFillLimits" %>

<!DOCTYPE html>
<html lang="en" xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Panels : Panel Bulk Product Fill Limits</title>
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
            document.getElementById('pnlBulkProductFillLimits').style.visibility = "hidden";
        }

        function onRequestDone(sender, args) {
        	$find('PleaseWaitPopup').hide();
        	document.getElementById('pnlBulkProductFillLimits').style.visibility = "visible";
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
                    <asp:Button ID="btnSaveFillLimit" runat="server" Style="width: auto;" Text="Save settings"
                        Enabled="False" />
                    <asp:Label ID="lblFillLimitStatus" runat="server" ForeColor="Red" />
                    <span class="sectionRequiredField"><span class="required"></span>&nbsp;indicates required
			field </span>
                </div>
                <asp:Panel ID="pnlBulkProductFillLimits" runat="server" CssClass="sectionEven" Style="width: 75%;">
                    <h1>Bulk product fill limits</h1>
                    <ul class="addRemoveSection">
                        <li>
                            <asp:ListBox ID="lstBulkProductFillLimit" runat="server" AutoPostBack="True" CssClass="addRemoveList" />
                            <asp:Button ID="btnAddBulkProductFillLimit" runat="server" Text="Add fill limit" CssClass="addRemoveButton" />
                            <asp:Button ID="btnRemoveBulkProductFillLimit" runat="server" Text="Remove fill limit" Enabled="False"
                                CssClass="addRemoveButton" />
                        </li>
                    </ul>
                    <asp:Panel ID="pnlFillLimitSettings" runat="server" CssClass="section">
                        <ul>
                            <li>
                                <hr />
                            </li>
                            <li class="addRemoveSection">
                                <span class="addRemoveList">
                                    <label>
                                        Fill limit</label>
                                    <span class="input">
                                        <span class="required">
                                            <asp:TextBox ID="tbxFillLimit" runat="server" Style="width: 50%; text-align: right;">0</asp:TextBox>
                                            <asp:DropDownList ID="ddlFillLimitUnit" runat="server" Style="width: 40%;" />
                                        </span></span>
                                </span>
                                <span class="addRemoveButton">&nbsp;</span>
                            </li>
                            <li class="addRemoveSection">
                                <span class="addRemoveList">
                                    <label>
                                        Bulk product source from
                                    </label>
                                    <span class="required">
                                        <asp:DropDownList ID="ddlProductNumber" runat="server" AutoPostBack="true"></asp:DropDownList>
                                    </span>
                                </span>
                            </li>
                            <li id="pnlBulkProductAssigned" runat="server">
                                <ul class="addRemoveSection">
                                    <li>
                                        <label style="width: 100%; text-align: left;">
                                            Bulk Products 
                                        </label>
                                    </li>
                                    <li>
                                        <asp:DropDownList ID="ddlBulkProducts" runat="server" AutoPostBack="true" CssClass="addRemoveList" />
                                        <asp:Button ID="btnAddBulkProductToFillLimit" runat="server" Text="Add bulk product" Enabled="False" CssClass="addRemoveButton" />
                                    </li>
                                    <li>
                                        <span class="required">
                                            <asp:ListBox ID="lstBulkProductsAssignedToFillLimit" runat="server" AutoPostBack="true" CssClass="addRemoveList" />
                                        </span>
                                        <asp:Button ID="btnRemoveBulkProductFromFillLimit" runat="server" Text="Remove bulk product" Enabled="False"
                                            CssClass="addRemoveButton" />
                                    </li>
                                </ul>
                            </li>
                        </ul>
                    </asp:Panel>
                </asp:Panel>
            </ContentTemplate>
        </asp:UpdatePanel>
    </form>
</body>
</html>
