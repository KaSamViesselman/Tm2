<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="Panels.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.Panels"
    MaintainScrollPositionOnPostback="true" %>

<!DOCTYPE html>
<html lang="en">
<head id="Head1" runat="server">
    <title>Panels : Panels</title>
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
            document.getElementById('pnlPanelInfo').style.visibility = "hidden";
        }

        function onRequestDone(sender, args) {
        	$find('PleaseWaitPopup').hide();
        	document.getElementById('pnlPanelInfo').style.visibility = "visible";
        	if (args.get_error() != undefined) {
        		var errorMessage = args.get_error().message
        		args.set_errorHandled(true);
        		alert(errorMessage);
        	}
        }
    </script>
</head>
<body>
    <form id="main" runat="server" defaultfocus="tbxName">
        <asp:ScriptManager ID="ToolkitScriptManager1" runat="server" AsyncPostBackTimeout="3600" AsyncPostBackErrorMessage="Timeout while retrieving data." EnablePartialRendering="true" OnAsyncPostBackError="ScriptManager1_AsyncPostBackError">
        </asp:ScriptManager>
        <asp:Panel ID="PleaseWaitMessagePanel" runat="server" CssClass="modalPopup" Style="height: 50px; width: 125px; text-align: center;">
            Please wait<br />
            <img src="images/ajax-loader.gif" alt="Please wait" />
        </asp:Panel>
        <asp:Button ID="HiddenButton" runat="server" Text="Hidden Button" Style="visibility: hidden;" ToolTip="Necessary for Modal Popup Extender" />
        <ajaxToolkit:ModalPopupExtender ID="PleaseWaitPopup" BehaviorID="PleaseWaitPopup" runat="server" TargetControlID="HiddenButton" PopupControlID="PleaseWaitMessagePanel" BackgroundCssClass="modalBackground" Y="-1">
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
                    <asp:Button ID="btnSave" runat="server" Text="Save" />
                    <asp:Button ID="btnDelete" runat="server" Text="Delete" />
                    <asp:Label ID="lblStatus" runat="server" ForeColor="Red" />
                    <span class="sectionRequiredField"><span class="required"></span>&nbsp;indicates required
			field </span>
                </div>
                <div id="pnlPanelInfo" class="section">
                    <asp:Panel ID="pnlEven" runat="server" CssClass="sectionEven">
                        <h1>General</h1>
                        <ul>
                            <li>
                                <label>
                                    Name
                                </label>
                                <span class="required">
                                    <asp:TextBox ID="tbxName" runat="server" /></span> </li>
                            <li>
                                <label>
                                    Facility
                                </label>
                                <span class="required">
                                    <asp:DropDownList ID="ddlLocation" runat="server" />
                                </span></li>
                            <li>
                                <label>
                                    Role
                                </label>
                                <asp:DropDownList ID="ddlRole" runat="server" AutoPostBack="True" />
                            </li>
                        </ul>
                        <asp:Panel ID="pnlPsSettings" runat="server">
                            <ul>
                                <li>
                                    <label>
                                        Rank
                                    </label>
                                    <span class="required">
                                        <asp:TextBox ID="tbxRank" runat="server" /></span> </li>
                                <li>
                                    <label>
                                        Follow by (%)
                                    </label>
                                    <span class="required">
                                        <asp:TextBox ID="tbxFollowBy" runat="server" /></span> </li>
                                <li>
                                    <label>
                                        Rinse threshold
                                    </label>
                                    <span class="required">
                                        <asp:TextBox ID="tbxRinseThreshold" runat="server" /></span> </li>
                                <li>
                                    <label>
                                        Hold threshold
                                    </label>
                                    <span class="required, input">
                                        <asp:TextBox ID="tbxHoldThreshold" runat="server" Width="60%" />
                                        <asp:DropDownList ID="ddlHoldThresholdUnit" runat="server" Width="30%" />
                                    </span></li>
                                <li>
                                    <label>
                                        Mass unit
                                    </label>
                                    <asp:DropDownList ID="ddlWeightUnit" runat="server" />
                                </li>
                                <li>
                                    <label>
                                        Volume unit
                                    </label>
                                    <asp:DropDownList ID="ddlVolumeUnit" runat="server" />
                                </li>
                                <li>
                                    <label>
                                        &nbsp;
                                    </label>
                                    <asp:CheckBox ID="chkUseReservations" runat="server" Text="Use reservations" />
                                </li>
                                <li>
                                    <label>
                                        &nbsp;
                                    </label>
                                    <span class="input">
                                        <asp:Literal ID="litCurrentReservation" runat="server" />
                                        <asp:Button ID="btnClearReservation" runat="server" Text="Clear" Visible="False" />
                                    </span></li>
                            </ul>
                        </asp:Panel>
                        <ul>
                            <li>
                                <label>
                                    Connection type
                                </label>
                                <asp:DropDownList ID="ddlConnectionType" runat="server" AutoPostBack="True" />
                            </li>
                            <li>
                                <label>
                                    Controller number
                                </label>
                                <span class="required">
                                    <asp:TextBox ID="tbxSlaveNumber" runat="server" MaxLength="3" /></span>
                            </li>
                            <li>
                                <label id="lblSystemAddress" runat="server">
                                    System address
                                </label>
                                <span class="required">
                                    <asp:TextBox ID="tbxSystemAddress" runat="server" /></span> </li>
                        </ul>
                        <asp:Panel ID="pnlTcpConnection" runat="server" Visible="False">
                            <ul>
                                <li>
                                    <label>
                                        IP address
                                    </label>
                                    <span class="input"><span class="required">
                                        <asp:TextBox ID="tbxIpAddress" runat="server" /></span>
                                        <asp:LinkButton ID="lbtConfigure" runat="server" Text="Configure"></asp:LinkButton></span>
                                </li>
                                <li>
                                    <label>
                                        TCP port
                                    </label>
                                    <span class="required">
                                        <asp:TextBox ID="tbxTcpPort" runat="server" /></span> </li>
                            </ul>
                        </asp:Panel>
                        <asp:Panel ID="pnlSerialConnection" runat="server" Visible="False">
                            <ul>
                                <li>
                                    <label>
                                        Serial port
                                    </label>
                                    <asp:DropDownList ID="ddlSerialPort" runat="server" />
                                </li>
                                <li>
                                    <label>
                                        Baud rate
                                    </label>
                                    <asp:DropDownList ID="ddlBaudRate" runat="server" />
                                </li>
                                <li>
                                    <label>
                                        Parity
                                    </label>
                                    <asp:DropDownList ID="ddlParity" runat="server" />
                                </li>
                                <li>
                                    <label>
                                        Data bits
                                    </label>
                                    <asp:DropDownList ID="ddlDataBits" runat="server" />
                                </li>
                                <li>
                                    <label>
                                        Stop bits
                                    </label>
                                    <asp:DropDownList ID="ddlStopBits" runat="server" />
                                </li>
                            </ul>
                        </asp:Panel>
                        <asp:Panel ID="pnlEmulation" runat="server" Visible="False">
                            <ul>
                                <li>
                                    <label>
                                        Emulation rate
                                    </label>
                                    <span class="required, input">
                                        <asp:TextBox ID="tbxEmulateUnitsPerSecond" runat="server" />
                                        <asp:DropDownList ID="ddlEmulateBaseUnit" runat="server" Style="width: auto;" />
                                        &nbsp;/sec 
                                    </span>
                                </li>
                            </ul>
                        </asp:Panel>
                        <ul id="pnlCurrrentPanelSettings" runat="server">
                            <li>
                                <label>
                                    KA-2000 application
                                </label>
                                <asp:Label CssClass="input" ID="lblKa2000Application" runat="server"></asp:Label>
                                <br />
                                <label>
                                    KA-2000 system
                                </label>
                                <asp:Label CssClass="input" ID="lblKa2000System" runat="server"></asp:Label>
                                <br />
                                <label>
                                    KA-2000 version
                                </label>
                                <asp:Label CssClass="input" ID="lblKa2000Version" runat="server"></asp:Label>
                                <br />
                            </li>
                        </ul>
                        <ul id="lstCustomFields" runat="server">
                        </ul>
                    </asp:Panel>
                    <asp:Panel ID="pnlOdd" runat="server" CssClass="sectionOdd">
                        <h1>Bulk products</h1>
                        <ul>
                            <li>
                                <label>
                                    &nbsp;
                                </label>
                                <a href="PanelBulkProductSettings.aspx">Click here for Bulk Product settings</a>
                            </li>
                        </ul>
                    </asp:Panel>
                    <hr />
                    <asp:Panel ID="pnlPanelUnitPrecisions" runat="server" CssClass="sectionEven">
                        <h1>Unit precision</h1>
                        <table id="tblPanelUnitPrecisions" runat="server" style="width: auto;">
                            <tr>
                                <th>Unit
                                </th>
                                <th style="width: auto; text-align: center;">Precision
                                </th>
                                <th style="width: auto; text-align: center;" colspan="2">Whole
                                </th>
                                <th style="width: auto; text-align: center;" colspan="2">Fractional
                                </th>
                                <th style="width: auto; text-align: center;">Current Unit Precision
                                </th>
                                <th>
                                    <asp:Button ID="btnResetAllPrecisiontoUnits" runat="server" Text="Use default for all"
                                        Style="width: auto; text-align: center;" />
                                </th>
                            </tr>
                            <tr>
                                <td>&nbsp;</td>
                                <td>&nbsp;</td>
                                <td>&nbsp;</td>
                                <td>&nbsp;</td>
                                <td>&nbsp;</td>
                                <td>&nbsp;</td>
                                <td>&nbsp;</td>
                                <td>&nbsp;</td>
                            </tr>
                        </table>
                        <h2>Unit precision calculations based on minimum scale/meter division 
                        </h2>
                        <ul>
                            <li id="pnlKa2000SuppliedPrecision" runat="server">
                                <label>
                                    Meter precision calculated from precision provided by KA-2000
                                </label>
                                <asp:Label ID="lblKa2000SuppliedPrecision" runat="server" CssClass="input"></asp:Label>
                            </li>
                            <li>
                                <label>
                                    Minimum scale/meter division 
                                </label>
                                <span class="input">
                                    <asp:DropDownList ID="ddlPanelMinScaleDivision" runat="server" Style="width: 20%; text-align: right;">
                                        <asp:ListItem Text="1" Value="1"></asp:ListItem>
                                        <asp:ListItem Text="2" Value="2"></asp:ListItem>
                                        <asp:ListItem Text="5" Value="5"></asp:ListItem>
                                    </asp:DropDownList>
                                    &nbsp;X&nbsp;
							<asp:DropDownList ID="ddlPanelMinScaleDivisionMultiplier" runat="server" Style="width: 20%; text-align: right;">
                                <asp:ListItem Text="100" Value="100"></asp:ListItem>
                                <asp:ListItem Text="10" Value="10"></asp:ListItem>
                                <asp:ListItem Text="1" Value="1"></asp:ListItem>
                                <asp:ListItem Text="0.1" Value="0.1" Selected="True"></asp:ListItem>
                                <asp:ListItem Text="0.01" Value="0.01"></asp:ListItem>
                                <asp:ListItem Text="0.001" Value="0.001"></asp:ListItem>
                                <asp:ListItem Text="0.0001" Value="0.0001"></asp:ListItem>
                                <asp:ListItem Text="0.00001" Value="0.00001"></asp:ListItem>
                                <asp:ListItem Text="0.000001" Value="0.000001"></asp:ListItem>
                            </asp:DropDownList>
                                    &nbsp;
							<asp:DropDownList ID="ddlPanelMinScaleDivisionUnit" runat="server" Style="width: 20%; text-align: right;"></asp:DropDownList>
                                </span>
                            </li>
                            <li>
                                <label>
                                    Typical product density
                                </label>
                                <span class="input">
                                    <asp:TextBox ID="tbxPanelMinScaleDivisionDensity" runat="server" Style="width: 20%; text-align: right;">10.0</asp:TextBox>
                                    &nbsp;
				<asp:DropDownList ID="ddlPanelMinScaleDivisionDensityWeightUnit" runat="server" Style="width: 20%; text-align: right;"></asp:DropDownList>
                                    &nbsp;/&nbsp;
				<asp:DropDownList ID="ddlPanelMinScaleDivisionDensityVolumeUnit" runat="server" Style="width: 20%; text-align: right;"></asp:DropDownList>
                                </span>
                            </li>
                            <li style="text-align: center;">
                                <asp:Button ID="btnPanelMinScaleDivision" runat="server" Text="Calculate recommendations" Style="width: auto;" />
                                <asp:Button ID="btnAssignScaleDivisionPrecision" runat="server" Text="Set panel precision" Style="width: auto;" Visible="false" />
                            </li>
                            <li>
                                <table id="tblPanelMinScaleDivisionRecommendations" runat="server" enableviewstate="true">
                                    <tr>
                                        <th>Unit
                                        </th>
                                        <th>Converted value</th>
                                        <th>Precision
                                        </th>
                                    </tr>
                                </table>
                            </li>
                        </ul>
                    </asp:Panel>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </form>
</body>
</html>
