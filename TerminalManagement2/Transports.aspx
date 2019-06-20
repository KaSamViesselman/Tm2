<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="Transports.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.Transports" MaintainScrollPositionOnPostback="true" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <title>Transports : Transports</title>
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
</head>
<body>
    <form id="main" method="post" runat="server" defaultfocus="tbxname">
        <div class="recordSelection">
            <label>
                Transport
            </label>
            <asp:DropDownList ID="ddlTransports" runat="server" AutoPostBack="True">
            </asp:DropDownList>
        </div>
        <div class="recordControl">
            <asp:Button ID="btnSave" runat="server" Text="Save" />
            <asp:Button ID="btnDelete" runat="server" Text="Delete" Enabled="false" />
            <asp:Label ID="lblStatus" runat="server" ForeColor="#ff0000"></asp:Label>
            <div class="sectionRequiredField">
                <label>
                    <span class="required"></span>&nbsp;indicates required field
                </label>
            </div>
        </div>
        <asp:Panel ID="pnlEven" runat="server" CssClass="sectionEven">
            <h1>General</h1>
            <ul>
                <li>
                    <label>
                        Name
                    </label>
                    <span class="required">
                        <asp:TextBox ID="tbxName" runat="server"></asp:TextBox>
                    </span></li>
                <li>
                    <label>
                        Number
                    </label>
                    <asp:TextBox ID="tbxNumber" runat="server"></asp:TextBox>
                </li>
                <li>
                    <label>
                        Type
                    </label>
                    <asp:DropDownList ID="ddlTransportType" runat="server">
                    </asp:DropDownList>
                </li>
                <li>
                    <label>
                        RFID number
                    </label>
                    <asp:ListBox ID="lstRfidNumber" runat="server" SelectionMode="Single" AutoPostBack="true"></asp:ListBox>
                </li>
                <li>
                    <label>
                    </label>
                    <asp:TextBox ID="tbxRfidNumber" runat="server"></asp:TextBox><br />
                </li>
                <li>
                    <label>
                    </label>
                    <span class="input">
                        <asp:Button ID="btnAddRfidTag" runat="server" Text="Add" Width="30%" />
                        <asp:Button ID="btnSetRfidTag" runat="server" Text="Set" Width="30%" />
                        <asp:Button ID="btnRemoveRfidTag" runat="server" Text="Remove" Width="30%" />
                    </span></li>
                <li>
                    <label>
                        Unit
                    </label>
                    <span class="required">
                        <asp:DropDownList ID="ddlTransportUnitId" runat="server" AutoPostBack="True">
                        </asp:DropDownList>
                    </span></li>
                <li>
                    <label>
                        Empty weight
                    </label>
                    <span class="input">
                        <asp:TextBox ID="tbxEmptyWeight" runat="server">0</asp:TextBox>&nbsp;
                        <asp:Label ID="lblEmptyWeightUnit" runat="server" Style="text-align: left;"></asp:Label>
                    </span>
                </li>
                <li>
                    <label>
                        Max weight
                    </label>
                    <span class="input">
                        <asp:TextBox ID="tbxMaxWeight" runat="server">0</asp:TextBox>&nbsp;
                        <asp:Label ID="lblMaxWeightUnit" runat="server" Style="text-align: left;"></asp:Label></span>
                </li>
                <li>
                    <label>
                        Temporary maximum gross weight
                    </label>
                    <span class="input">
                        <asp:TextBox ID="tbxTempOverweightAmount" runat="server">0</asp:TextBox>&nbsp;
                        <asp:Label ID="lblTempOverweightAmountUnit" runat="server" Style="text-align: left;"></asp:Label>
                    </span>
                </li>
                <li>
                    <label>
                        Temporary maximum gross weight expiration date
                    </label>
                    <input type="text" name="tbxTempOverweightExpirationDate" id="tbxTempOverweightExpirationDate" value="" runat="server" />
                    <script type="text/javascript">
                        $('#tbxTempOverweightExpirationDate').datetimepicker({
                            timeFormat: 'h:mm:ss TT',
                            showSecond: true,
                            showOn: "button",
                            buttonImage: 'Images/Calendar_scheduleHS.png',
                            buttonImageOnly: true,
                            buttonText: "Show calendar"
                        });
                    </script>
                </li>
                <li>
                    <label>
                        Length
                    </label>
                    <span class="input">
                        <asp:TextBox ID="tbxLength" runat="server" Width="70%">0</asp:TextBox>
                        <asp:DropDownList ID="ddlLengthUnit" runat="server" Width="20%">
                        </asp:DropDownList>
                    </span></li>
                <li>
                    <label>
                        Carrier
                    </label>
                    <asp:DropDownList ID="ddlCarriers" runat="server" AutoPostBack="True">
                    </asp:DropDownList>
                </li>
                <li>
                    <label>
                        Current Order
                    </label>
                    <asp:DropDownList ID="ddlCurrentOrder" runat="server">
                    </asp:DropDownList>
                </li>
            </ul>
            <ul id="lstCustomFields" runat="server">
            </ul>
            <hr style="width: 100%; color: #99ccff" />
            <ul>
                <li>
                    <h1>Compartments
                    </h1>
                </li>
                <li>
                    <label>
                        &nbsp;
                    </label>
                    <span class="input">
                        <asp:DropDownList ID="ddlCompartments" runat="server" AutoPostBack="True" Width="20%">
                        </asp:DropDownList>
                        <asp:Button ID="btnAddCompartment" runat="server" Text="Add" Enabled="False" Width="70%"></asp:Button>
                    </span></li>
                <li>
                    <label>
                        Capacity
                    </label>
                    <span class="input">
                        <asp:TextBox ID="tbxCompartmentCapacity" runat="server" Enabled="False" Width="45%">0</asp:TextBox>
                        <asp:DropDownList ID="ddlCompartmnetUnitId" runat="server" AutoPostBack="True" Enabled="False" Width="45%">
                        </asp:DropDownList>
                    </span></li>
                <li>
                    <label>
                    </label>
                    <span class="input">
                        <asp:Button ID="btnSaveCompartment" runat="server" Text="Save" Width="45%" />
                        <asp:Button ID="btnDeleteCompartment" runat="server" Text="Delete" Width="45%" />
                    </span></li>
            </ul>
            <hr style="width: 100%; color: #99ccff" />
            <ul id="pnlLastInFacilityInfo" runat="server">
                <li>
                    <h1>In facility
                    </h1>
                </li>
                <li id="liLastInFacilityLocation" runat="server">
                    <label>
                        Facility
                    </label>
                    <asp:Label CssClass="input" ID="lblLastInFacilityLocation" runat="server"></asp:Label>
                </li>
                <li id="liLastEnteredFacility" runat="server">
                    <label>Last entered</label>
                    <asp:Label ID="lblLastEnteredFacility" runat="server"></asp:Label>
                </li>
                <li id="liLastExitedFacility" runat="server">
                    <label>
                        Last exited
                    </label>
                    <asp:Label ID="lblLastExitedFacility" runat="server"></asp:Label>
                </li>
                <li id="liLastInFacilityStatus" runat="server">
                    <label>
                        Current status
                    </label>
                    <span class="input">
                        <asp:Label ID="lblLastInFacilityStatus" runat="server" CssClass="label"></asp:Label><br />
                        <asp:Button ID="btnClearLastInFacilityStatus" runat="server" Text="Set as exited facility" Style="width: auto;" />
                    </span>
                </li>
            </ul>
        </asp:Panel>
        <asp:Panel ID="pnlInterfaceSetup" runat="server" CssClass="sectionOdd">
            <h2>Interface</h2>
            <div class="recordSelectionEvenOdd">
                <label>
                    Interface setting
                </label>
                <asp:DropDownList ID="ddlTransportInterface" runat="server" AutoPostBack="True">
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
        </asp:Panel>
    </form>
</body>
</html>
