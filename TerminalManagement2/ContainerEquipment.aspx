<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="ContainerEquipment.aspx.vb"
    Inherits="KahlerAutomation.TerminalManagement2.ContainerEquipment" EnableSessionState="True" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <title>Containers : Container Equipment</title>
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
    <form id="main" method="post" runat="server">
        <div class="recordSelection">
            <ul>
                <li>
                    <label>
                        Facility</label>
                    <asp:DropDownList ID="ddlFacilityFilter" runat="server" AutoPostBack="true">
                    </asp:DropDownList>
                </li>
                <li>
                    <label>
                        Container equipment</label>
                    <asp:DropDownList ID="ddlContainerEquipment" runat="server" AutoPostBack="True">
                    </asp:DropDownList>
                </li>
            </ul>
        </div>
        <div class="recordControl">
            <asp:Button ID="btnSave" runat="server" Text="Save" />
            <asp:Button ID="btnDelete" runat="server" Text="Delete" />
            <asp:Label ID="lblStatus" runat="server" ForeColor="#ff0000"></asp:Label>
            <span class="sectionRequiredField"><span class="required"></span>&nbsp;indicates required
			field </span>
        </div>
        <asp:Panel ID="pnlEven" runat="server" CssClass="sectionEven">
            <h1>General</h1>
            <ul>
                <li>
                    <label>
                        Name</label>
                    <span class="required">
                        <asp:TextBox ID="tbxName" runat="server" MaxLength="50"></asp:TextBox>
                    </span></li>
                <li>
                    <label>
                        Equipment type</label>
                    <asp:DropDownList ID="ddlType" runat="server">
                    </asp:DropDownList>
                </li>
                <li>
                    <label>
                        Owner</label>
                    <asp:DropDownList ID="ddlOwner" runat="server">
                    </asp:DropDownList>
                </li>
                <li>
                    <label>
                        Facility</label>
                    <asp:DropDownList ID="ddlFacility" runat="server">
                    </asp:DropDownList>
                </li>
                <li>
                    <label>
                    </label>
                    <asp:CheckBox ID="chkWithCustomer" runat="server" Text="With customer" />
                </li>
                <li>
                    <label>
                        Container</label>
                    <asp:DropDownList ID="ddlContainer" runat="server">
                    </asp:DropDownList>
                </li>
                <li>
                    <label>
                        Barcode</label>
                    <asp:TextBox ID="tbxBarcode" runat="server"></asp:TextBox>
                </li>
                <li>
                    <label>
                        Last inspected</label>
                    <span class="required">
                        <input type="text" name="tbxLastInspectedDate" id="tbxLastInspectedDate" value=""
                            runat="server" style="width: 45%;" />
                    </span>
                    <script type="text/javascript">
                        $('#tbxLastInspectedDate').datetimepicker({
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
                    </label>
                    <asp:CheckBox ID="chkPassedInspection" Text="Passed inspection" runat="server" />
                </li>
            </ul>
        </asp:Panel>
    </form>
</body>
</html>
