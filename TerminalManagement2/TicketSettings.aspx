<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="TicketSettings.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.TicketSettings" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<!DOCTYPE html>
<html lang="en">
<head id="Head1" runat="server">
    <title>General Settings : Ticket Settings</title>
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
    <form id="main" method="post" runat="server" defaultfocus="cbxSeparateTicketNumberPerOwner">
        <div class="recordControl">
            <asp:Button ID="btnSave" runat="server" Text="Save" />
            <asp:Label ID="lblSave" runat="server" ForeColor="Red" Text="Ticket settings were saved successfully"
                Visible="False" />
        </div>
        <div class="sectionEven" id="pnlTicketSettings" runat="server">
            <ul>
                <li>
                    <label>
                    </label>
                    <asp:CheckBox ID="cbxSeparateTicketNumberPerOwner" runat="server" Text="Use separate ticket numbers per owner"
                        AutoPostBack="True" />
                </li>
                <li>
                    <label>
                        Next ticket number
                    </label>
                    <asp:TextBox ID="tbxNextTicketNumber" runat="server"></asp:TextBox>
                </li>
                <li>
                    <label>
                        <asp:Label ID="lblSaveNextTicketNumber" runat="server" Text="Net ticket number saved successfully"
                            ForeColor="Red" Visible="false" /></label>
                    <asp:Button ID="btnSaveNextTicketNumber" runat="server" Text="Save next ticket number" />
                </li>
                <li>
                    <label>
                    </label>
                    <asp:CheckBox ID="cbxSeparateTicketPrefixPerOwner" runat="server" Text="Use separate ticket prefix per owner"
                        AutoPostBack="True" />
                </li>
                <li>
                    <label>
                        Ticket prefix
                    </label>
                    <asp:TextBox ID="tbxTicketPrefix" runat="server"></asp:TextBox>
                </li>
                <li>
                    <label>
                    </label>
                    <asp:CheckBox ID="cbxSeparateTicketSuffixPerOwner" runat="server" Text="Use separate ticket suffix per owner"
                        AutoPostBack="True" />
                </li>
                <li>
                    <label>
                        Ticket suffix
                    </label>
                    <asp:TextBox ID="tbxTicketSuffix" runat="server"></asp:TextBox>
                </li>
                <li>
                    <label>
                        Send tickets to:
                    </label>
                    <asp:CheckBox ID="chkEmailTicketAccount" runat="server" Text="Account" />
                </li>
                <li>
                    <label>
                    </label>
                    <asp:CheckBox ID="chkEmailTicketAccountLocation" runat="server" Text="Account location" />
                </li>
                <li>
                    <label>
                    </label>
                    <asp:CheckBox ID="chkEmailTicketApplicator" runat="server" Text="Applicator" />
                </li>
                <li>
                    <label>
                    </label>
                    <asp:CheckBox ID="chkEmailTicketBranch" runat="server" Text="Branch" />
                </li>
                <li>
                    <label>
                    </label>
                    <asp:CheckBox ID="chkEmailTicketCarrier" runat="server" Text="Carrier" />
                </li>
                <li>
                    <label>
                    </label>
                    <asp:CheckBox ID="chkEmailTicketDriver" runat="server" Text="Driver" />
                </li>
                <li>
                    <label>
                    </label>
                    <asp:CheckBox ID="chkEmailTicketLocation" runat="server" Text="Facility" />
                </li>
                <li>
                    <label>
                    </label>
                    <asp:CheckBox ID="chkEmailTicketOwner" runat="server" Text="Owner" />
                </li>
            </ul>
        </div>
    </form>
</body>
</html>
