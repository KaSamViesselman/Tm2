<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="Inventory.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.Inventory"
    MaintainScrollPositionOnPostback="true" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <title>Inventory : Inventory</title>
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
        function pleaseWait() {
            document.getElementById('pnlMain').style.visibility = "hidden";
            document.getElementById('lblPleaseWait').innerHTML = "<br><br>&nbsp;&nbsp;&nbsp;Please wait while the selected inventory data is being fetched...";
        }
    </script>
    <script type="text/javascript">
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
    <form id="main" method="post" runat="server">
        <asp:Label ID="lblPleaseWait" runat="server" Font-Size="Large" ForeColor="#003399"></asp:Label>
        <div class="recordSelection" id="pnlMain" runat="server">
            <div class="sectionEven">
                <ul>
                    <li>
                        <label>
                            Owner</label>
                        <asp:DropDownList ID="ddlOwner" runat="server" AutoPostBack="true">
                        </asp:DropDownList>
                    </li>
                    <li>
                        <label>
                            Facility</label>
                        <asp:DropDownList ID="ddlLocation" runat="server" AutoPostBack="true">
                        </asp:DropDownList>
                    </li>
                </ul>
            </div>
            <div class="sectionOdd">
                <ul>
                    <li>
                        <label>
                            Bulk product</label>
                        <asp:DropDownList ID="ddlBulkProduct" runat="server" AutoPostBack="true">
                        </asp:DropDownList>
                    </li>
                    <li>
                        <label>
                        </label>
                        <asp:CheckBox ID="cbxOnlyShowBulkProductsWithNonZeroInventory" runat="server" Text="Only show non-zero inventories"
                            Checked="True" AutoPostBack="true" />
                    </li>
                    <li>
                        <label>
                        </label>
                        <asp:CheckBox ID="cbxAssignPhysicalInventoryToOwner" runat="server" Text="Assign physical inventory to owner assigned to tank"
                            Checked="True" AutoPostBack="true" />
                    </li>
                </ul>
            </div>
            <div class="sectionEven">
                <h3>Additional Units</h3>
                <div class="collapsingSection not" id="pnlAdditionalUnits" runat="server">
                    <ul>
                        <li>
                            <asp:CheckBoxList ID="cblAdditionalUnits" runat="server" RepeatLayout="UnorderedList" CssClass="input">
                            </asp:CheckBoxList>
                        </li>
                    </ul>
                </div>
            </div>
        </div>
        <div class="recordControl">
            <asp:Button ID="btnShowInventory" runat="server" Text="Show inventory" />
            <asp:Button ID="btnPrinterFriendly" runat="server" Text="Printer friendly" />
            <asp:Button ID="btnDownload" runat="server" Text="Download report" />
            <asp:Label ID="litFiltersChanged" runat="server" />
        </div>
        <asp:Literal ID="litInventory" runat="server"></asp:Literal>
        <div class="section">
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
        <div class="section" >
            <hr style="width: 100%; color: #003399;" />
            <h2>Adjustments</h2>
            <div class="sectionEven">
                    <ul>
                        <li>
                            <label>
                                Owner</label>
                            <asp:DropDownList ID="ddlAdjustOwner" runat="server" AutoPostBack="true">
                            </asp:DropDownList>
                        </li> 
                        <li>
                            <label>
                                Facility</label>
                            <asp:DropDownList ID="ddlAdjustLocation" runat="server" AutoPostBack="true">
                            </asp:DropDownList>
                        </li>
                    </ul>
                </div> 
            <div class="sectionOdd"> 
                    <ul>
                        <li>
                            <label>
                                Bulk product</label>
                            <asp:DropDownList ID="ddlAdjustBulkProduct" runat="server" AutoPostBack="true">
                            </asp:DropDownList>
                        </li>
                    </ul> 
            </div>
            </div>
            <div class="section" id="pnlInventoryButtons" runat="server">
                <div class="sectionEven">
                    <div class="sectionEven">
                        <h2>Inventory quantity</h2>
                        <div class="collapsingSection not">
                            <ul>
                                <li>
                                    <label>
                                        Adjustment
                                    </label>
                                    <span class="input">
                                        <asp:TextBox ID="txtAdjustInventory" runat="server" Style="text-align: right; width: 60%;"></asp:TextBox><asp:DropDownList ID="ddlAdjustInventoryUnit" runat="server" Style="width: 30%;"></asp:DropDownList></span>
                                </li>
                                <li>
                                    <label>
                                        Notes</label>
                                    <asp:TextBox ID="txtNotes" runat="server"></asp:TextBox>
                                </li>
                            </ul>
                            <div class="recordControl">
                                <asp:Button ID="btnAdjustInventoryApply" runat="server" Text="Apply" />
                                <asp:Button ID="btnAdjustInventoryCancel" runat="server" Text="Cancel" />
                            </div>
                        </div>
                    </div>
                    <div class="sectionOdd">
                        <h2>Dispensed quantity</h2>
                        <div class="collapsingSection not">
                            <asp:Button ID="btnResetDispensed" runat="server" Text="Reset dispensed quantity" />
                        </div>
                    </div>
                </div>
                <div class="sectionOdd">
                    <div class="sectionEven">
                        <h2>Unit of measure</h2>
                        <div class="collapsingSection not">
                            <ul>
                                <li>
                                    <asp:DropDownList ID="ddlUnitOfMeasure" runat="server">
                                    </asp:DropDownList>
                                </li>
                                <li>
                                    <asp:CheckBox ID="cbxConvert" runat="server" Text="Convert current quantities" />
                                </li>
                            </ul>
                            <div class="recordControl">
                                <asp:Button ID="btnChangeUnitOfMeasureApply" runat="server" Text="Apply" />
                                <asp:Button ID="btnChangeUnitOfMeasureCancel" runat="server" Text="Cancel" />
                            </div>
                        </div>
                    </div>
                    <div class="sectionOdd">
                        <h2>Transfer</h2>
                        <div class="collapsingSection not">
                            <ul>
                                <li>
                                    <label>
                                        Quantity</label>
                                    <span class="input">
                                        <asp:TextBox ID="tbxTransferQuantity" runat="server" Style="text-align: right; width: 60%;"></asp:TextBox>
                                        <asp:DropDownList ID="ddlTransferUnit" runat="server" Style="width: 30%;"></asp:DropDownList></span>
                                </li>
                                <li>
                                    <label>
                                        To owner
                                    </label>
                                    <asp:DropDownList ID="ddlTransferToOwner" runat="server" />
                                </li>
                                <li>
                                    <label>
                                        At facility
                                    </label>
                                    <asp:DropDownList ID="ddlTransferToLocation" runat="server" />
                                </li>
                                <li>
                                    <label>
                                        Notes
                                    </label>
                                    <asp:TextBox ID="tbxTransferNotes" runat="server" />
                                </li>
                            </ul>
                            <div class="recordControl">
                                <asp:Button ID="btnApplyTransfer" runat="server" Text="Apply" />
                                <asp:Button ID="btnCancelTransfer" runat="server" Text="Cancel" />
                            </div>
                        </div>
                    </div>
                </div>
            </div>
         </form>
</body>
</html>
