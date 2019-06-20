<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="OrderSettings.aspx.vb"
    Inherits="KahlerAutomation.TerminalManagement2.OrderSettings" MaintainScrollPositionOnPostback="true" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <title>General Settings : Order Settings</title>
    <link rel="shortcut icon" href="FavIcon.ico" />
    <meta name="viewport" content="width=device-width" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <link rel="stylesheet" href="styles/site.css" />
    <link rel="stylesheet" href="styles/site-tablet.css" media="(max-width:768px)" />
    <link rel="stylesheet" href="styles/site-phone.css" media="(max-width:480px)" />
    <script type="text/javascript" src="scripts/jquery-1.11.1.min.js"></script>
    <script type="text/javascript" src="scripts/soap-1.0.1.js"></script>
    <script type="text/javascript" src="scripts/page-controller.js"></script>
</head>
<body>
    <form id="main" method="post" runat="server" defaultfocus="cbxAutoGenerate">
        <div class="recordControl">
            <asp:Button ID="btnSave" runat="server" Text="Save" />
            <asp:Label ID="lblSave" runat="server" ForeColor="Red" Text="Order settings were saved successfully"
                Visible="False" />
        </div>
        <div class="sectionEven" id="pnlOrderSettings" runat="server">
            <ul>
                <li>
                    <label>
                    </label>
                    <asp:CheckBox ID="cbxAutoGenerate" runat="server" Text="Automatically generate sequential order numbers"
                        AutoPostBack="True"></asp:CheckBox>
                </li>
                <li>
                    <label>
                    </label>
                    <asp:CheckBox ID="cbxAllowModification" runat="server" Text="Allow order numbers to be modified"
                        AutoPostBack="True" />
                </li>
                <li>
                    <label>
                    </label>
                    <asp:CheckBox ID="cbxSeparateOrderNumberPerOwner" runat="server" Text="Use separate order numbers per owner"
                        AutoPostBack="true" />
                </li>
                <li>
                    <label>
                    </label>
                    <asp:CheckBox ID="cbxCreateNewDestinationFromOrderShipToInformation" runat="server"
                        Text="Create new destination from order ship to information" />
                </li>
                <li>
                    <label>
                        Next order number
                    </label>
                    <asp:TextBox ID="tbxStartingOrderNumber" runat="server"></asp:TextBox>
                </li>
                <li>
                    <label>
                        <asp:Label ID="lblSaveNextOrderNumber" runat="server" Text="Next order number saved successfully"
                            ForeColor="Red" Visible="false" />
                    </label>
                    <asp:Button ID="btnSaveNextOrderNumber" runat="server" Text="Save next order number" />
                </li>
                <li>
                    <label>
                    </label>
                    <asp:CheckBox ID="cbxPartialDelete" runat="server" Text="Mark orders as complete instead of deleting when an order is deleted"
                        AutoPostBack="True"></asp:CheckBox>
                </li>
                <li>
                    <label>
                    </label>
                    <asp:CheckBox ID="cbxLockOwner" runat="server" Text="Lock owner drop down list"></asp:CheckBox>
                </li>
                <li>
                    <label>
                    </label>
                    <asp:CheckBox ID="cbxLockBranch" runat="server" Text="Lock branches drop down list"></asp:CheckBox>
                </li>
                <li>
                    <label>
                    </label>
                    <asp:CheckBox ID="cbxLockRunOverPercent" runat="server" Text="Lock over scaling field"></asp:CheckBox>
                </li>
                <li>
                    <label>
                        Order comparison tolerance (%)
                    </label>
                    <asp:TextBox ID="tbxOrderComparisonPercentTolerance" runat="server" CssClass="inputNumeric"
                        Style="text-align: right; width: 15%; min-width: 2em;" Text="1.0"></asp:TextBox>
                </li>
                <li>
                    <label>
                    </label>
                    <asp:CheckBox ID="cbxShowReleaseNumberInOrderList" runat="server" Text="Show release number in order list" />
                </li>
                <li>
                    <label>
                        Internal transfer customer account
                    </label>
                    <asp:DropDownList ID="ddlInternalTransferCustomerAccount" runat="server" />
                </li>
                <li>
                    <label>
                        Send order summaries to</label>
                    <asp:CheckBox ID="cbxSendOrderSummaryToApplicator" runat="server" Text="Applicator" />
                </li>
                <li>
                    <label>
                    </label>
                    <asp:CheckBox ID="cbxSendOrderSummaryToBranch" runat="server" Text="Branch" />
                </li>
                <li>
                    <label>
                    </label>
                    <asp:CheckBox ID="cbxSendOrderSummaryToCustomerAccount" runat="server" Text="Customer account" />
                </li>
                <li>
                    <label>
                    </label>
                    <asp:CheckBox ID="cbxSendOrderSummaryToCustomerAccountLocation" runat="server" Text="Customer account location" />
                </li>
                <li>
                    <label>
                    </label>
                    <asp:CheckBox ID="cbxSendOrderSummaryToOwner" runat="server" Text="Owner" />
                </li>
            </ul>
        </div>
        <div class="sectionOdd" id="pnlPresetOrderSettings" runat="server">
            <h2>Preset order settings</h2>
            <ul>
                <li>
                    <label>
                        Owner
                    </label>
                    <asp:DropDownList ID="ddlOwners" runat="server">
                    </asp:DropDownList>
                </li>
                <li>
                    <label>
                        Branch
                    </label>
                    <asp:DropDownList ID="ddlBranches" runat="server">
                    </asp:DropDownList>
                </li>
                <li>
                    <label>
                        Over scaling (%)
                    </label>
                    <asp:TextBox ID="tbxRunOverPercent" runat="server" CssClass="inputNumeric" Style="text-align: right; width: 15%; min-width: 2em;"
                        Text="1.0"></asp:TextBox>
                </li>
            </ul>
        </div>
        <div class="sectionEven" id="pnlPointOfSaleSettings" runat="server">
            <h2>Point of sale and staged orders settings</h2>
            <ul>
                <li>
                    <label>
                    </label>
                    <asp:CheckBox ID="chkUseOrderPercentage" runat="server" Text="Use order percentage for new staged orders"
                        AutoPostBack="True" />
                </li>
                <li>
                    <label>
                    </label>
                    <asp:CheckBox ID="chkAllowOrdersToBeAssignedToMultipleStagedOrders" runat="server" Text="Allow orders to be assigned to multiple staged orders" ToolTip="If not checked, the system will check the Staged Orders table to see if a staged order exists for this order" Checked="true" />
                </li>
                <li>
                    <label>
                    </label>
                    <asp:CheckBox ID="chkLimitDriversToDriversAssignedToAccount" runat="server" Text="Limit drivers to drivers assigned to account" AutoPostBack="True"></asp:CheckBox>
                </li>
                <li>
                    <label>
                    </label>
                    <asp:CheckBox ID="chkLimitTransportsToCarrierSelected" runat="server" Text="Limit transports to carrier selected" AutoPostBack="True"></asp:CheckBox>
                </li>
                <li>
                    <label>
                    </label>
                    <asp:CheckBox ID="cbxEmailCreatedPointOfSaleTickets" runat="server" Text="E-mail created Point of Sale tickets" />
                </li>
            </ul>
            <ul>
                <li>
                    <hr />
                </li>
                <li>
                    <h2>Staged order shortcuts
                    </h2>
                </li>
                <li>
                    <label>
                        Staged order shortcuts</label>
                    <asp:CheckBoxList ID="cblStagedOrderShortcuts" runat="server" RepeatLayout="UnorderedList"
                        CssClass="input" BorderStyle="Solid" BorderWidth="1px">
                        <asp:ListItem Text="Show &quot;Use remaining order quantity&quot; shortcut" Value="UseRemainingOrderQuantityShortcut"></asp:ListItem>
                        <asp:ListItem Text="Show &quot;Use Original Order Quantity&quot; shortcut" Value="UseOriginalOrderQuantityShortcut"></asp:ListItem>
                        <asp:ListItem Text="Show &quot;Use Application Rate&quot; shortcut" Value="UseApplicationRateShortcut"></asp:ListItem>
                        <asp:ListItem Text="Show &quot;Use Transport Capacity&quot; shortcut" Value="UseTransportCapacityShortcut"></asp:ListItem>
                        <asp:ListItem Text="Show &quot;Use Batch Quantity&quot; shortcut" Value="UseBatchQuantityShortcut"></asp:ListItem>
                        <asp:ListItem Text="Show &quot;Specify Total Quantity&quot; shortcut" Value="SpecifyTotalQuantityShortcut"></asp:ListItem>
                    </asp:CheckBoxList>
                </li>
                <li id="pnlStagedOrderCustomShortcuts" runat="server">
                    <label>
                        Staged order custom shortcuts</label>
                    <asp:CheckBoxList ID="cblStagedOrderCustomShortcuts" runat="server" RepeatLayout="UnorderedList"
                        CssClass="input" BorderStyle="Solid" BorderWidth="1px">
                    </asp:CheckBoxList>
                </li>
            </ul>
            <ul id="pnlPoSCustomLoadQuestions" runat="server">
                <li>
                    <hr />
                </li>
                <li>
                    <h2>Point of sale custom load questions
                    </h2>
                </li>
                <li id="pnlPoSCustomPreLoadQuestions" runat="server">
                    <label>
                        Pre load questions
                    </label>
                    <asp:CheckBoxList ID="cblPoSCustomPreLoadQuestion" runat="server" RepeatLayout="UnorderedList"
                        CssClass="input" BorderStyle="Solid" BorderWidth="1px">
                    </asp:CheckBoxList>
                </li>
                <li id="pnlPoSCustomPostLoadQuestion" runat="server">
                    <label>
                        Post load questions
                    </label>
                    <asp:CheckBoxList ID="cblPoSCustomPostLoadQuestion" runat="server" RepeatLayout="UnorderedList"
                        CssClass="input" BorderStyle="Solid" BorderWidth="1px">
                    </asp:CheckBoxList>
                </li>
            </ul>
        </div>
    </form>
</body>
</html>
