<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="PanelBulkProducts.aspx.vb"
    Inherits="KahlerAutomation.TerminalManagement2.PanelBulkProducts" MaintainScrollPositionOnPostback="true" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <title>Panels : Panel Bulk Products Report</title>
    <link rel="shortcut icon" href="FavIcon.ico" />
    <meta name="viewport" content="width=device-width" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <link rel="stylesheet" href="styles/site.css" />
    <link rel="stylesheet" href="styles/site-tablet.css" media="(max-width:768px)" />
    <link rel="stylesheet" href="styles/site-phone.css" media="(max-width:480px)" />
    <script type="text/javascript">
        function DisplayAddEmailButton(value) {
            if (value != '') {
                document.getElementById('btnAddEmailAddress').style.visibility = 'visible';
            }
            else {
                document.getElementById('btnAddEmailAddress').style.visibility = 'hidden';
            }
        }
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
            document.getElementById('lblBulkProductStatus').style.visibility = "hidden";
        }

        function onRequestDone() {
            $find('PleaseWaitPopup').hide();
            document.getElementById('pnlMain').style.visibility = "visible";
            document.getElementById('lblBulkProductStatus').style.visibility = "visible";
        }
    </script>
</head>
<body>
    <form id="frmMain" runat="server">
        <asp:ScriptManager ID="ToolkitScriptManager1" runat="server" AsyncPostBackTimeout="3600" AsyncPostBackErrorMessage="Timeout while retrieving data." EnablePartialRendering="true">
        </asp:ScriptManager>
        <asp:Panel ID="PleaseWaitMessagePanel" runat="server" CssClass="modalPopup" Style="height: 50px; width: 125px; text-align: center;">
            Please wait<br />
            <img src="images/ajax-loader.gif" alt="Please wait" />
        </asp:Panel>
        <asp:Button ID="HiddenButton" runat="server" Text="Hidden Button" Style="visibility: hidden;" ToolTip="Necessary for Modal Popup Extender" />
        <ajaxToolkit:ModalPopupExtender ID="PleaseWaitPopup" BehaviorID="PleaseWaitPopup" runat="server" TargetControlID="HiddenButton" PopupControlID="PleaseWaitMessagePanel" BackgroundCssClass="modalBackground" Y="200">
        </ajaxToolkit:ModalPopupExtender>
        <asp:UpdatePanel ID="PleaseWaitPanel" runat="server" RenderMode="Inline">
            <ContentTemplate>
                <div class="sectionEven" id="pnlFilters" runat="server">
                    <ul>
                        <li>
                            <label style="font-weight: bold">
                                Filters</label>
                        </li>
                        <li>
                            <label>
                                Bulk product</label>
                            <asp:DropDownList ID="ddlBulkProduct" runat="server" AutoPostBack="true">
                            </asp:DropDownList>
                        </li>
                        <li>
                            <label>
                                Facility</label>
                            <asp:DropDownList ID="ddlFacility" runat="server" AutoPostBack="true">
                            </asp:DropDownList>
                        </li>
                        <li>
                            <label>
                                Panel</label>
                            <asp:DropDownList ID="ddlPanel" runat="server" AutoPostBack="true">
                            </asp:DropDownList>
                        </li>
                    </ul>
                </div>
                <div class="recordControl" id="pnlRecordControl" runat="server">
                    <asp:Button ID="btnApplyFilter" runat="server" Text="Apply filter" />
                    <asp:Button ID="btnPrinterFriendly" runat="server" Text="Printer friendly" />
                    <asp:Label ID="lblBulkProductStatus" runat="server" ForeColor="Red" />
                </div>
                <div id="pnlMain">
                    <div class="section" id="pnlBulkProductPanelSettings" runat="server">
                        <table id="tblPanelBulkProducts" runat="server">
                            <tr>
                                <th>Bulk product
                                </th>
                                <th>Panel function
                                </th>
                                <th>Panel
                                </th>
                                <th style="text-align: center;">Disabled
                                </th>
                                <th>Start parameter
                                </th>
                                <th>Finish parameter
                                </th>
                                <th style="text-align: center;">Always use finish parameter
                                </th>
                                <th style="text-align: right;">Anticipation
                                </th>
                                <th>Anticipation update factor
                                </th>
                                <th>Conversion factor
                                </th>
                                <th style="text-align: center;">Update density using meter
                                </th>
                                <th style="text-align: center;">Use average density for ticket
                                </th>
                                <th>Dump time
                                </th>
                            </tr>
                        </table>
                    </div>
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
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </form>
</body>
</html>
