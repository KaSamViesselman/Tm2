<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="UserProfiles.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.UserProfiles" MaintainScrollPositionOnPostback="true" %>

<!DOCTYPE html>
<html lang="en">
<head id="Head1" runat="server">
    <title>Users : User Profiles</title>
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
        }

        function onRequestDone(sender, args) {
        	$find('PleaseWaitPopup').hide();
        	if (args.get_error() != undefined) {
        		var errorMessage = args.get_error().message
        		args.set_errorHandled(true);
        		alert(errorMessage);
        	}
        }

        $(window).scroll(function () {
            document.getElementById('__SCROLLPOSITIONY').value = $(window).scrollTop();
        });
    </script>
    <style type="text/css">
        table tr td label {
            max-width: 100% !important;
            vertical-align: top !important;
        }

        .colorlist li:nth-child(even) {
            background-color: #e2e2e2;
        }

        table, th, td {
            padding: 2px 3px 2px 3px;
        }

        #PleaseWaitMessagePanel {
            width: 100%;
            height: 100%;
            top: 0;
            left: 0;
            background-color: rgba(0, 0, 0, 0.24); /*Sets transparency without using the inheritable opacity property*/
            text-align: center;
        }
            /*Trick the browser into thinking this div has content. If the browser doesn't support psudo-elements then the logo will be in the upper middle instead of the center*/
            #PleaseWaitMessagePanel:before {
                content: '';
                display: inline-block;
                vertical-align: middle;
                height: 100%;
            }

        #divInnerPopup {
            display: inline-block;
            padding: 1em;
            font-size: 1rem; /*required for text to render properly*/
            background-color: rgba(255, 255, 255, 1);
            border-radius: 2em;
            text-align: center;
            vertical-align: middle;
        }
    </style>
</head>
<body>
    <form id="main" runat="server" defaultfocus="tbxName">
        <asp:ScriptManager ID="ToolkitScriptManager1" runat="server" AsyncPostBackTimeout="3600" AsyncPostBackErrorMessage="Update timed out." OnAsyncPostBackError="ScriptManager1_AsyncPostBackError" EnablePartialRendering="true">
        </asp:ScriptManager>
        <asp:Panel ID="PleaseWaitMessagePanel" runat="server" CssClass="modalPopup">
            <div id="divInnerPopup">
                Please wait<br />
                <img src="images/ajax-loader.gif" alt="Please wait" />
            </div>
        </asp:Panel>
        <asp:Button ID="HiddenButton" runat="server" Text="Hidden Button" Style="display: none;" ToolTip="Necessary for Modal Popup Extender" />
        <ajaxToolkit:ModalPopupExtender ID="PleaseWaitPopup" BehaviorID="PleaseWaitPopup" runat="server" TargetControlID="HiddenButton" PopupControlID="PleaseWaitMessagePanel" BackgroundCssClass="modalBackground">
        </ajaxToolkit:ModalPopupExtender>
        <asp:HiddenField ID="hfScrollPosition" runat="server" />
        <asp:UpdatePanel ID="PleaseWaitPanel" runat="server" RenderMode="Inline">
            <ContentTemplate>
                <div class="recordSelection">
                    <label>
                        User profile</label>
                    <asp:DropDownList ID="ddlUserProfiles" runat="server" AutoPostBack="True" />
                </div>
                <div class="recordControl">
                    <asp:Button ID="btnSave" runat="server" Text="Save" />
                    <asp:Button ID="btnDelete" runat="server" Text="Delete" Enabled="False" />
                    <asp:Label ID="lblStatus" runat="server" ForeColor="Red" />
                    <div class="sectionRequiredField">
                        <label>
                            <span class="required"></span>&nbsp;indicates required field</label>
                    </div>
                </div>
                <asp:Panel ID="pnlEven" runat="server" CssClass="sectionEven">
                    <h1>General</h1>
                    <ul>
                        <li>
                            <label>
                                Name</label>
                            <span class="required">
                                <asp:TextBox ID="tbxName" runat="server" MaxLength="50" /></span> </li>
                    </ul>
                </asp:Panel>
                <asp:Panel ID="pnlOdd" runat="server" CssClass="sectionOdd">
                    <h1>Permissions</h1>
                    <div id="divPerms" runat="server">
                        <table>
                            <tr>
                                <td>Applicators
                                </td>
                                <td>
                                    <asp:RadioButtonList ID="rblApplicatorsPermissions" Style="display: inline-flex; vertical-align: top;" runat="server" RepeatLayout="Flow" AutoPostBack="true">
                                        <asp:ListItem Text="None" Value="N" Selected="True" />
                                        <asp:ListItem Text="View" Value="V" />
                                        <asp:ListItem Text="Modify" Value="M" />
                                    </asp:RadioButtonList>
                                    <asp:CheckBoxList ID="cblApplicatorsPermissions" Style="display: inline-flex; vertical-align: top;" runat="server" RepeatLayout="Flow" Enabled="false" AutoPostBack="true">
                                        <asp:ListItem Text="Create" Value="C" />
                                        <asp:ListItem Text="Edit" Value="E" />
                                        <asp:ListItem Text="Delete" Value="D" />
                                    </asp:CheckBoxList>
                                </td>
                            </tr>
                            <tr>
                                <td>Branches
                                </td>
                                <td>
                                    <asp:RadioButtonList ID="rblBranchesPermissions" runat="server" Style="display: inline-flex; vertical-align: top;" RepeatLayout="Flow" AutoPostBack="true">
                                        <asp:ListItem Text="None" Value="N" Selected="True" />
                                        <asp:ListItem Text="View" Value="V" />
                                        <asp:ListItem Text="Modify" Value="M" />
                                    </asp:RadioButtonList>
                                    <asp:CheckBoxList ID="cblBranchesPermissions" runat="server" Style="display: inline-flex; vertical-align: top;" RepeatLayout="Flow" Enabled="false" AutoPostBack="true">
                                        <asp:ListItem Text="Create" Value="C" />
                                        <asp:ListItem Text="Edit" Value="E" />
                                        <asp:ListItem Text="Delete" Value="D" />
                                    </asp:CheckBoxList>
                                </td>
                            </tr>
                            <tr>
                                <td>Carriers
                                </td>
                                <td>
                                    <asp:RadioButtonList ID="rblCarriersPermissions" runat="server" Style="display: inline-flex; vertical-align: top;" RepeatLayout="Flow" AutoPostBack="true">
                                        <asp:ListItem Text="None" Value="N" Selected="True" />
                                        <asp:ListItem Text="View" Value="V" />
                                        <asp:ListItem Text="Modify" Value="M" />
                                    </asp:RadioButtonList>
                                    <asp:CheckBoxList ID="cblCarriersPermissions" runat="server" Style="display: inline-flex; vertical-align: top;" RepeatLayout="Flow" Enabled="false" AutoPostBack="true">
                                        <asp:ListItem Text="Create" Value="C" />
                                        <asp:ListItem Text="Edit" Value="E" />
                                        <asp:ListItem Text="Delete" Value="D" />
                                    </asp:CheckBoxList>
                                </td>
                            </tr>
                            <tr>
                                <td>Containers
                                </td>
                                <td>
                                    <asp:RadioButtonList ID="rblContainersPermissions" runat="server" Style="display: inline-flex; vertical-align: top;" RepeatLayout="Flow" AutoPostBack="true">
                                        <asp:ListItem Text="None" Value="N" Selected="True" />
                                        <asp:ListItem Text="View" Value="V" />
                                        <asp:ListItem Text="Modify" Value="M" />
                                    </asp:RadioButtonList>
                                    <asp:CheckBoxList ID="cblContainersPermissions" runat="server" Style="display: inline-flex; vertical-align: top;" RepeatLayout="Flow" Enabled="false" AutoPostBack="true">
                                        <asp:ListItem Text="Create" Value="C" />
                                        <asp:ListItem Text="Edit" Value="E" />
                                        <asp:ListItem Text="Delete" Value="D" />
                                    </asp:CheckBoxList>
                                </td>
                            </tr>
                            <tr>
                                <td>Crops
                                </td>
                                <td>
                                    <asp:RadioButtonList ID="rblCropsPermissions" runat="server" Style="display: inline-flex; vertical-align: top;" RepeatLayout="Flow" AutoPostBack="true">
                                        <asp:ListItem Text="None" Value="N" Selected="True" />
                                        <asp:ListItem Text="View" Value="V" />
                                        <asp:ListItem Text="Modify" Value="M" />
                                    </asp:RadioButtonList>
                                    <asp:CheckBoxList ID="cblCropsPermissions" runat="server" Style="display: inline-flex; vertical-align: top;" RepeatLayout="Flow" Enabled="false" AutoPostBack="true">
                                        <asp:ListItem Text="Create" Value="C" />
                                        <asp:ListItem Text="Edit" Value="E" />
                                        <asp:ListItem Text="Delete" Value="D" />
                                    </asp:CheckBoxList>
                                </td>
                            </tr>
                            <tr>
                                <td>Custom Pages
                                </td>
                                <td>
                                    <asp:RadioButtonList ID="rblCustomPagesPermissions" runat="server" Style="display: inline-flex; vertical-align: top;" RepeatLayout="Flow" AutoPostBack="true">
                                        <asp:ListItem Text="None" Value="N" Selected="True" />
                                        <asp:ListItem Text="View" Value="V" />
                                        <asp:ListItem Text="Modify" Value="M" />
                                    </asp:RadioButtonList>
                                    <asp:CheckBoxList ID="cblCustomPagesPermissions" runat="server" Style="display: inline-flex; vertical-align: top;" RepeatLayout="Flow" Enabled="false" AutoPostBack="true">
                                        <asp:ListItem Text="Create" Value="C" />
                                        <asp:ListItem Text="Edit" Value="E" />
                                        <asp:ListItem Text="Delete" Value="D" />
                                    </asp:CheckBoxList>
                                </td>
                            </tr>
                            <tr>
                                <td>Customer Accounts
                                </td>
                                <td>
                                    <asp:RadioButtonList ID="rblCustomerAccountsPermissions" runat="server" Style="display: inline-flex; vertical-align: top;" RepeatLayout="Flow" AutoPostBack="true">
                                        <asp:ListItem Text="None" Value="N" Selected="True" />
                                        <asp:ListItem Text="View" Value="V" />
                                        <asp:ListItem Text="Modify" Value="M" />
                                    </asp:RadioButtonList>
                                    <asp:CheckBoxList ID="cblCustomerAccountsPermissions" runat="server" Style="display: inline-flex; vertical-align: top;" RepeatLayout="Flow" Enabled="false" AutoPostBack="true">
                                        <asp:ListItem Text="Create" Value="C" />
                                        <asp:ListItem Text="Edit" Value="E" />
                                        <asp:ListItem Text="Delete" Value="D" />
                                    </asp:CheckBoxList>
                                </td>
                            </tr>
                            <tr>
                                <td>Drivers
                                </td>
                                <td>
                                    <asp:RadioButtonList ID="rblDriversPermissions" runat="server" Style="display: inline-flex; vertical-align: top;" RepeatLayout="Flow" AutoPostBack="true">
                                        <asp:ListItem Text="None" Value="N" Selected="True" />
                                        <asp:ListItem Text="View" Value="V" />
                                        <asp:ListItem Text="Modify" Value="M" />
                                    </asp:RadioButtonList>
                                    <asp:CheckBoxList ID="cblDriversPermissions" runat="server" Style="display: inline-flex; vertical-align: top;" RepeatLayout="Flow" Enabled="false" AutoPostBack="true">
                                        <asp:ListItem Text="Create" Value="C" />
                                        <asp:ListItem Text="Edit" Value="E" />
                                        <asp:ListItem Text="Delete" Value="D" />
                                    </asp:CheckBoxList>
                                </td>
                            </tr>
                            <tr>
                                <td>E-mails
                                </td>
                                <td>
                                    <asp:RadioButtonList ID="rblEmailsPermissions" runat="server" Style="display: inline-flex; vertical-align: top;" RepeatLayout="Flow" AutoPostBack="true">
                                        <asp:ListItem Text="None" Value="N" Selected="True" />
                                        <asp:ListItem Text="View" Value="V" />
                                        <asp:ListItem Text="Modify" Value="M" />
                                    </asp:RadioButtonList>
                                    <asp:CheckBoxList ID="cblEmailsPermissions" runat="server" Style="display: inline-flex; vertical-align: top;" RepeatLayout="Flow" Enabled="false" AutoPostBack="true">
                                        <asp:ListItem Text="Create" Value="C" />
                                        <asp:ListItem Text="Edit" Value="E" />
                                        <asp:ListItem Text="Delete" Value="D" />
                                    </asp:CheckBoxList>
                                </td>
                            </tr>
                            <tr>
                                <td>Facilities
                                </td>
                                <td>
                                    <asp:RadioButtonList ID="rblFacilitiesPermissions" runat="server" Style="display: inline-flex; vertical-align: top;" RepeatLayout="Flow" AutoPostBack="true">
                                        <asp:ListItem Text="None" Value="N" Selected="True" />
                                        <asp:ListItem Text="View" Value="V" />
                                        <asp:ListItem Text="Modify" Value="M" />
                                    </asp:RadioButtonList>
                                    <asp:CheckBoxList ID="cblFacilitiesPermissions" runat="server" Style="display: inline-flex; vertical-align: top;" RepeatLayout="Flow" Enabled="false" AutoPostBack="true">
                                        <asp:ListItem Text="Create" Value="C" />
                                        <asp:ListItem Text="Edit" Value="E" />
                                        <asp:ListItem Text="Delete" Value="D" />
                                    </asp:CheckBoxList>
                                </td>
                            </tr>
                            <tr>
                                <td>General Settings
                                </td>
                                <td>
                                    <asp:RadioButtonList ID="rblGeneralSettingsPermissions" runat="server" Style="display: inline-flex; vertical-align: top;" RepeatLayout="Flow" AutoPostBack="true">
                                        <asp:ListItem Text="None" Value="N" Selected="True" />
                                        <asp:ListItem Text="View" Value="V" />
                                        <asp:ListItem Text="Modify" Value="M" />
                                    </asp:RadioButtonList>
                                    <asp:CheckBoxList ID="cblGeneralSettingsPermissions" runat="server" Style="display: inline-flex; vertical-align: top;" RepeatLayout="Flow" Enabled="false" AutoPostBack="true">
                                        <asp:ListItem Text="Create" Value="C" />
                                        <asp:ListItem Text="Edit" Value="E" />
                                        <asp:ListItem Text="Delete" Value="D" />
                                    </asp:CheckBoxList>
                                </td>
                            </tr>
                            <tr>
                                <td>Interfaces
                                </td>
                                <td>
                                    <asp:RadioButtonList ID="rblInterfacesPermissions" runat="server" Style="display: inline-flex; vertical-align: top;" RepeatLayout="Flow" AutoPostBack="true">
                                        <asp:ListItem Text="None" Value="N" Selected="True" />
                                        <asp:ListItem Text="View" Value="V" />
                                        <asp:ListItem Text="Modify" Value="M" />
                                    </asp:RadioButtonList>
                                    <asp:CheckBoxList ID="cblInterfacesPermissions" runat="server" Style="display: inline-flex; vertical-align: top;" RepeatLayout="Flow" Enabled="false" AutoPostBack="true">
                                        <asp:ListItem Text="Create" Value="C" />
                                        <asp:ListItem Text="Edit" Value="E" />
                                        <asp:ListItem Text="Delete" Value="D" />
                                    </asp:CheckBoxList>
                                </td>
                            </tr>
                            <tr>
                                <td>Inventory
                                </td>
                                <td>
                                    <asp:RadioButtonList ID="rblInventoryPermissions" runat="server" Style="display: inline-flex; vertical-align: top;" RepeatLayout="Flow" AutoPostBack="true">
                                        <asp:ListItem Text="None" Value="N" Selected="True" />
                                        <asp:ListItem Text="View" Value="V" />
                                        <asp:ListItem Text="Modify" Value="M" />
                                    </asp:RadioButtonList>
                                    <asp:CheckBoxList ID="cblInventoryPermissions" runat="server" Style="display: inline-flex; vertical-align: top;" RepeatLayout="Flow" Enabled="false" AutoPostBack="true">
                                        <asp:ListItem Text="Create" Value="C" />
                                        <asp:ListItem Text="Edit" Value="E" />
                                        <asp:ListItem Text="Delete" Value="D" />
                                    </asp:CheckBoxList>
                                </td>
                            </tr>
                            <tr>
                                <td>Order Page Interfaces
                                </td>
                                <td>
                                    <asp:RadioButtonList ID="rblOrderPageInterfacesPermissions" runat="server" Style="display: inline-flex; vertical-align: top;" RepeatLayout="Flow" AutoPostBack="true">
                                        <asp:ListItem Text="None" Value="N" Selected="True" />
                                        <asp:ListItem Text="View" Value="V" />
                                        <asp:ListItem Text="Modify" Value="M" />
                                    </asp:RadioButtonList>
                                    <asp:CheckBoxList ID="cblOrderPageInterfacesPermissions" runat="server" Style="display: inline-flex; vertical-align: top;" RepeatLayout="Flow" Enabled="false" AutoPostBack="true">
                                        <asp:ListItem Text="Create" Value="C" />
                                        <asp:ListItem Text="Edit" Value="E" />
                                        <asp:ListItem Text="Delete" Value="D" />
                                    </asp:CheckBoxList>
                                </td>
                            </tr>
                            <tr>
                                <td>Orders
                                </td>
                                <td>
                                    <asp:RadioButtonList ID="rblOrderPermissions" runat="server" Style="display: inline-flex; vertical-align: top;" RepeatLayout="Flow" AutoPostBack="true">
                                        <asp:ListItem Text="None" Value="N" Selected="True" />
                                        <asp:ListItem Text="View" Value="V" />
                                        <asp:ListItem Text="Modify" Value="M" />
                                    </asp:RadioButtonList>
                                    <asp:CheckBoxList ID="cblOrderPermissions" runat="server" Style="display: inline-flex; vertical-align: top;" RepeatLayout="Flow" Enabled="false" AutoPostBack="true">
                                        <asp:ListItem Text="Create" Value="C" />
                                        <asp:ListItem Text="Edit" Value="E" />
                                        <asp:ListItem Text="Delete" Value="D" />
                                    </asp:CheckBoxList>
                                </td>
                            </tr>
                            <tr>
                                <td>Owners
                                </td>
                                <td>
                                    <asp:RadioButtonList ID="rblOwnerPermissions" runat="server" Style="display: inline-flex; vertical-align: top;" RepeatLayout="Flow" AutoPostBack="true">
                                        <asp:ListItem Text="None" Value="N" Selected="True" />
                                        <asp:ListItem Text="View" Value="V" />
                                        <asp:ListItem Text="Modify" Value="M" />
                                    </asp:RadioButtonList>
                                    <asp:CheckBoxList ID="cblOwnerPermissions" runat="server" Style="display: inline-flex; vertical-align: top;" RepeatLayout="Flow" Enabled="false" AutoPostBack="true">
                                        <asp:ListItem Text="Create" Value="C" />
                                        <asp:ListItem Text="Edit" Value="E" />
                                        <asp:ListItem Text="Delete" Value="D" />
                                    </asp:CheckBoxList>
                                </td>
                            </tr>
                            <tr>
                                <td>Panel Bulk Product
                                </td>
                                <td>
                                    <asp:RadioButtonList ID="rblPanelBulkPermissions" runat="server" Style="display: inline-flex; vertical-align: top;" RepeatLayout="Flow" AutoPostBack="true">
                                        <asp:ListItem Text="None" Value="N" Selected="True" />
                                        <asp:ListItem Text="View" Value="V" />
                                        <asp:ListItem Text="Modify" Value="M" />
                                    </asp:RadioButtonList>
                                    <asp:CheckBoxList ID="cblPanelBulkPermissions" runat="server" Style="display: inline-flex; vertical-align: top;" RepeatLayout="Flow" Enabled="false" AutoPostBack="true">
                                        <asp:ListItem Text="Create" Value="C" />
                                        <asp:ListItem Text="Edit" Value="E" />
                                        <asp:ListItem Text="Delete" Value="D" />
                                    </asp:CheckBoxList>
                                </td>
                            </tr>
                            <tr>
                                <td>Panels
                                </td>
                                <td>
                                    <asp:RadioButtonList ID="rblPanelsPermissions" runat="server" Style="display: inline-flex; vertical-align: top;" RepeatLayout="Flow" AutoPostBack="true">
                                        <asp:ListItem Text="None" Value="N" Selected="True" />
                                        <asp:ListItem Text="View" Value="V" />
                                        <asp:ListItem Text="Modify" Value="M" />
                                    </asp:RadioButtonList>
                                    <asp:CheckBoxList ID="cblPanelsPermissions" runat="server" Style="display: inline-flex; vertical-align: top;" RepeatLayout="Flow" Enabled="false" AutoPostBack="true">
                                        <asp:ListItem Text="Create" Value="C" />
                                        <asp:ListItem Text="Edit" Value="E" />
                                        <asp:ListItem Text="Delete" Value="D" />
                                    </asp:CheckBoxList>
                                </td>
                            </tr>
                            <tr>
                                <td>Products
                                </td>
                                <td>
                                    <asp:RadioButtonList ID="rblProductsPermissions" runat="server" Style="display: inline-flex; vertical-align: top;" RepeatLayout="Flow" AutoPostBack="true">
                                        <asp:ListItem Text="None" Value="N" Selected="True" />
                                        <asp:ListItem Text="View" Value="V" />
                                        <asp:ListItem Text="Modify" Value="M" />
                                    </asp:RadioButtonList>
                                    <asp:CheckBoxList ID="cblProductsPermissions" runat="server" Style="display: inline-flex; vertical-align: top;" RepeatLayout="Flow" Enabled="false" AutoPostBack="true">
                                        <asp:ListItem Text="Create" Value="C" />
                                        <asp:ListItem Text="Edit" Value="E" />
                                        <asp:ListItem Text="Delete" Value="D" />
                                    </asp:CheckBoxList>
                                </td>
                            </tr>
                            <tr>
                                <td>Purchase Orders
                                </td>
                                <td>
                                    <asp:RadioButtonList ID="rblPurchaseOrdersPermissions" runat="server" Style="display: inline-flex; vertical-align: top;" RepeatLayout="Flow">
                                        <asp:ListItem Text="None" Value="N" Selected="True" />
                                        <asp:ListItem Text="View" Value="V" />
                                        <asp:ListItem Text="Modify" Value="M" />
                                    </asp:RadioButtonList>
                                    <asp:CheckBoxList ID="cblPurchaseOrdersPermissions" runat="server" Style="display: inline-flex; vertical-align: top;" RepeatLayout="Flow" Enabled="false" AutoPostBack="true">
                                        <asp:ListItem Text="Create" Value="C" />
                                        <asp:ListItem Text="Edit" Value="E" />
                                        <asp:ListItem Text="Delete" Value="D" />
                                    </asp:CheckBoxList>
                                </td>
                            </tr>
                            <tr>
                                <td>Receipt Page Interfaces
                                </td>
                                <td>
                                    <asp:RadioButtonList ID="rblReceiptPageInterfacesPermissions" runat="server" Style="display: inline-flex; vertical-align: top;" RepeatLayout="Flow" AutoPostBack="true">
                                        <asp:ListItem Text="None" Value="N" Selected="True" />
                                        <asp:ListItem Text="View" Value="V" />
                                        <asp:ListItem Text="Modify" Value="M" />
                                    </asp:RadioButtonList>
                                    <asp:CheckBoxList ID="cblReceiptPageInterfacesPermissions" runat="server" Style="display: inline-flex; vertical-align: top;" RepeatLayout="Flow" Enabled="false" AutoPostBack="true">
                                        <asp:ListItem Text="Create" Value="C" />
                                        <asp:ListItem Text="Edit" Value="E" />
                                        <asp:ListItem Text="Delete" Value="D" />
                                    </asp:CheckBoxList>
                                </td>
                            </tr>
                            <tr>
                                <td>Reports
                                </td>
                                <td>
                                    <asp:RadioButtonList ID="rblReportsPermissions" runat="server" Style="display: inline-flex; vertical-align: top;" RepeatLayout="Flow" AutoPostBack="true">
                                        <asp:ListItem Text="None" Value="N" Selected="True" />
                                        <asp:ListItem Text="View" Value="V" />
                                        <asp:ListItem Text="Modify" Value="M" />
                                    </asp:RadioButtonList>
                                    <asp:CheckBoxList ID="cblReportsPermissions" runat="server" Style="display: inline-flex; vertical-align: top;" RepeatLayout="Flow" Enabled="false" AutoPostBack="true">
                                        <asp:ListItem Text="Create" Value="C" />
                                        <asp:ListItem Text="Edit" Value="E" />
                                        <asp:ListItem Text="Delete" Value="D" />
                                    </asp:CheckBoxList>
                                </td>
                            </tr>
                            <tr>
                                <td>Staged Orders
                                </td>
                                <td>
                                    <asp:RadioButtonList ID="rblStagedOrdersPermissions" runat="server" Style="display: inline-flex; vertical-align: top;" RepeatLayout="Flow" AutoPostBack="true">
                                        <asp:ListItem Text="None" Value="N" Selected="True" />
                                        <asp:ListItem Text="View" Value="V" />
                                        <asp:ListItem Text="Modify" Value="M" />
                                    </asp:RadioButtonList>
                                    <asp:CheckBoxList ID="cblStagedOrdersPermissions" runat="server" Style="display: inline-flex; vertical-align: top;" RepeatLayout="Flow" Enabled="false" AutoPostBack="true">
                                        <asp:ListItem Text="Create" Value="C" />
                                        <asp:ListItem Text="Edit" Value="E" />
                                        <asp:ListItem Text="Delete" Value="D" />
                                    </asp:CheckBoxList>
                                </td>
                            </tr>
                            <tr>
                                <td>Tanks
                                </td>
                                <td>
                                    <asp:RadioButtonList ID="rblTanksPermissions" runat="server" Style="display: inline-flex; vertical-align: top;" RepeatLayout="Flow" AutoPostBack="true">
                                        <asp:ListItem Text="None" Value="N" Selected="True" />
                                        <asp:ListItem Text="View" Value="V" />
                                        <asp:ListItem Text="Modify" Value="M" />
                                    </asp:RadioButtonList>
                                    <asp:CheckBoxList ID="cblTanksPermissions" runat="server" Style="display: inline-flex; vertical-align: top;" RepeatLayout="Flow" Enabled="false" AutoPostBack="true">
                                        <asp:ListItem Text="Create" Value="C" />
                                        <asp:ListItem Text="Edit" Value="E" />
                                        <asp:ListItem Text="Delete" Value="D" />
                                    </asp:CheckBoxList>
                                </td>
                            </tr>
                            <tr>
                                <td>Transports
                                </td>
                                <td>
                                    <asp:RadioButtonList ID="rblTransportsPermissions" runat="server" Style="display: inline-flex; vertical-align: top;" RepeatLayout="Flow" AutoPostBack="true">
                                        <asp:ListItem Text="None" Value="N" Selected="True" />
                                        <asp:ListItem Text="View" Value="V" />
                                        <asp:ListItem Text="Modify" Value="M" />
                                    </asp:RadioButtonList>
                                    <asp:CheckBoxList ID="cblTransportsPermissions" runat="server" Style="display: inline-flex; vertical-align: top;" RepeatLayout="Flow" Enabled="false" AutoPostBack="true">
                                        <asp:ListItem Text="Create" Value="C" />
                                        <asp:ListItem Text="Edit" Value="E" />
                                        <asp:ListItem Text="Delete" Value="D" />
                                    </asp:CheckBoxList>
                                </td>
                            </tr>
                            <tr>
                                <td>Units
                                </td>
                                <td>
                                    <asp:RadioButtonList ID="rblUnitsPermissions" runat="server" Style="display: inline-flex; vertical-align: top;" RepeatLayout="Flow" AutoPostBack="true">
                                        <asp:ListItem Text="None" Value="N" Selected="True" />
                                        <asp:ListItem Text="View" Value="V" />
                                        <asp:ListItem Text="Modify" Value="M" />
                                    </asp:RadioButtonList>
                                    <asp:CheckBoxList ID="cblUnitsPermissions" runat="server" Style="display: inline-flex; vertical-align: top;" RepeatLayout="Flow" Enabled="false" AutoPostBack="true">
                                        <asp:ListItem Text="Create" Value="C" />
                                        <asp:ListItem Text="Edit" Value="E" />
                                        <asp:ListItem Text="Delete" Value="D" />
                                    </asp:CheckBoxList>
                                </td>
                            </tr>
                            <tr>
                                <td>Users
                                </td>
                                <td>
                                    <asp:RadioButtonList ID="rblUsersPermissions" runat="server" Style="display: inline-flex; vertical-align: top;" RepeatLayout="Flow" AutoPostBack="true">
                                        <asp:ListItem Text="None" Value="N" Selected="True" />
                                        <asp:ListItem Text="View" Value="V" />
                                        <asp:ListItem Text="Modify" Value="M" />
                                    </asp:RadioButtonList>
                                    <asp:CheckBoxList ID="cblUsersPermissions" runat="server" Style="display: inline-flex; vertical-align: top;" RepeatLayout="Flow" Enabled="false" AutoPostBack="true">
                                        <asp:ListItem Text="Create" Value="C" />
                                        <asp:ListItem Text="Edit" Value="E" />
                                        <asp:ListItem Text="Delete" Value="D" />
                                    </asp:CheckBoxList>
                                </td>
                            </tr>
                            <hr />
                            <tr style="border: 1px  black;">
                                <td>Select All
                                </td>
                                <td>
                                    <asp:RadioButtonList ID="rblSelectAll" runat="server" Style="display: inline-flex; vertical-align: top;" RepeatLayout="Flow" AutoPostBack="true">
                                        <asp:ListItem Text="None" Value="N" />
                                        <asp:ListItem Text="View" Value="V" />
                                        <asp:ListItem Text="Modify" Value="M" />
                                    </asp:RadioButtonList>
                                    <asp:CheckBoxList ID="cblSelectAll" runat="server" Style="display: inline-flex; vertical-align: top;" RepeatLayout="Flow" AutoPostBack="true">
                                        <asp:ListItem Text="Create" Value="C" />
                                        <asp:ListItem Text="Edit" Value="E" />
                                        <asp:ListItem Text="Delete" Value="D" />
                                    </asp:CheckBoxList>
                                </td>
                            </tr>
                        </table>
                    </div>
                    <div id="pnlCustomPages" runat="server">
                        <hr />
                        <h1>Custom permissions</h1>
                        <asp:CheckBoxList ID="cbxCustomPages" runat="server" Width="100%" RepeatLayout="UnorderedList" />
                        <asp:CheckBox ID="chkSelectAll" runat="server" Text="Select All" AutoPostBack="True" />
                    </div>
                </asp:Panel>
            </ContentTemplate>
        </asp:UpdatePanel>
    </form>
</body>
</html>
