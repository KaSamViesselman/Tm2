<%@ Page Title="Ticket" Language="VB" AutoEventWireup="false" CodeBehind="ticket.aspx.vb"
    Inherits="KahlerAutomation.TerminalManagement2.ticket" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <title>Ticket</title>
    <asp:Literal ID="StyleSheet" runat="Server" />
    <style type="text/css">
        .totals {
            border-top: 1px;
        }
    </style>
</head>
<body>
    <form id="ticketForm" method="post" runat="server">
        <table style="width: 100%">
            <tr>
                <td style="vertical-align: top;">
                    <img id="imgLogo" runat="server" src="" alt=" " align="left" /><%--src="images/Kahler-logo-standard.png"--%>
                </td>
            </tr>
        </table>
        <table style="width: 100%;">
            <tr>
                <td style="width: 50%; vertical-align: top;">
                    <asp:Label ID="lblTicketNumber" runat="server" Style="font-weight: bold;" Text="Ticket:"></asp:Label>
                    <asp:Literal ID="litTicketNumber" runat="server"></asp:Literal>
                </td>
                <td style="width: 50%; vertical-align: top;">
                    <asp:Label ID="lblOrderNumber" runat="server" Style="font-weight: bold;" Text="Order:"></asp:Label>
                    <asp:Literal ID="litOrderNumber" runat="server"></asp:Literal>
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Panel ID="pnlDateTime" runat="server">
                        <asp:Label ID="lblDateTime" runat="server" Style="font-weight: bold;" Text="Date/time:"></asp:Label>
                        <asp:Literal ID="litDateTime" runat="server"></asp:Literal>
                    </asp:Panel>
                </td>
                <td id="cellPurchaseOrderNumber" runat="server" style="vertical-align: top;">
                    <asp:Label ID="lblPurchaseOrder" runat="server" Style="font-weight: bold;" Text="Purchase Order:"></asp:Label>
                    <asp:Literal ID="litPurchaseOrderNumber" runat="server"></asp:Literal>
                </td>
            </tr>
            <tr>
                <td id="cellDischargeLocation" runat="server">
                    <asp:Panel ID="pnlFacility" runat="server">
                        <asp:Label ID="lblFacility" runat="server" Style="font-weight: bold;" Text="Facility:"></asp:Label>
                        <asp:Literal ID="litFacility" runat="server" />
                    </asp:Panel>
                    <asp:Panel ID="pnlDischargeLocation" runat="server">
                        <asp:Label ID="lblDischargeLocation" runat="server" Style="font-weight: bold;" Text="Discharge location:"></asp:Label>
                        <asp:Literal ID="litDischargeLocation" runat="server" />
                    </asp:Panel>
                </td>
                <td id="cellReleaseNumber" runat="server" style="vertical-align: top;">
                    <asp:Label ID="lblReleaseNumber" runat="server" Style="font-weight: bold;" Text="Release Number:"></asp:Label>
                    <asp:Literal ID="litReleaseNumber" runat="server"></asp:Literal>
                </td>
            </tr>
            <tr>
                <td colspan="2">
                    <asp:Literal ID="litComments" runat="server"></asp:Literal>
                </td>
            </tr>
            <tr>
                <td style="vertical-align: top;">
                    <asp:Panel ID="pnlSoldBy" runat="server">
                        <asp:Label ID="lblSoldBy" runat="server" Style="font-weight: bold;" Text="Sold by:"></asp:Label>
                        <br />
                        <asp:Literal ID="litSoldBy" runat="server"></asp:Literal>
                    </asp:Panel>
                </td>
                <td style="vertical-align: top;">
                    <asp:Panel ID="pnlBranch" runat="server">
                        <asp:Label ID="lblBranch" runat="server" Style="font-weight: bold;" Text="Branch:"></asp:Label>
                        <br />
                        <asp:Literal ID="litBranchLocation" runat="server"></asp:Literal>
                    </asp:Panel>
                </td>
            </tr>
            <tr id="trOwnerMessage" visible="false" runat="server">
                <td style="vertical-align: top;" colspan="2">
                    <asp:Panel ID="pnlOwnerMessage" runat="server">
                        <br />
                        <asp:Literal ID="litOwnerMessage" runat="server"></asp:Literal>
                        <br />
                    </asp:Panel>
                </td>
            </tr>
            <tr>
                <td style="vertical-align: top;">
                    <asp:Panel ID="pnlSoldTo" runat="server">
                        <asp:Label ID="lblSoldTo" runat="server" Style="font-weight: bold;" Text="Sold to:"></asp:Label>
                        <br />
                        <asp:Literal ID="litSoldTo" runat="server"></asp:Literal>
                    </asp:Panel>
                </td>
                <td style="vertical-align: top;">
                    <asp:Panel ID="pnlShipTo" runat="server">
                        <asp:Label ID="lblShipTo" runat="server" Style="font-weight: bold;" Text="Ship to:"></asp:Label>
                        <br />
                        <asp:Literal ID="litShipTo" runat="server"></asp:Literal>
                    </asp:Panel>
                </td>
            </tr>
            <tr>
                <td style="vertical-align: top;">
                    <asp:Panel ID="pnlAcres" runat="server">
                        <asp:Label ID="lblAcres" runat="server" Style="font-weight: bold;" Text="Acres:"></asp:Label>
                        <asp:Literal ID="litAcres" runat="server"></asp:Literal>
                    </asp:Panel>
                </td>
                <td></td>
            </tr>
            <tr id="trTicketCustomFields" runat="server">
                <td id="tdTicketCustomFields" runat="server"></td>
                <td></td>
            </tr>
        </table>
        <asp:Literal ID="litProducts" runat="server"></asp:Literal>
        <br />
        <table cellspacing="0" cellpadding="3" style="width: 100%;">
            <tr id="rowCarrier" runat="server">
                <td style="border-bottom: 1px solid black; vertical-align: bottom;">
                    <asp:Label ID="lblCarrier" runat="server" Text="Carrier:"></asp:Label>
                </td>
                <td style="border-bottom: 1px solid black;">
                    <asp:Literal ID="litCarrier" runat="server"></asp:Literal>&nbsp;
                </td>
            </tr>
            <tr id="rowTransport" runat="server">
                <td style="border-bottom: 1px solid black; vertical-align: text-top;">
                    <asp:Label ID="lblTransports" runat="server" Text="Transport(s):"></asp:Label>
                </td>
                <td style="border-bottom: 1px solid black; vertical-align: text-top;">
                    <asp:Literal ID="litTransports" runat="server"></asp:Literal>&nbsp;
                </td>
            </tr>
            <tr id="rowDriver" runat="server">
                <td style="border-bottom: 1px solid black; vertical-align: bottom;">
                    <asp:Label ID="lblDriver" runat="server" Text="Driver:"></asp:Label>
                </td>
                <td style="border-bottom: 1px solid black;">
                    <asp:Literal ID="litDriver" runat="server"></asp:Literal>&nbsp;
                </td>
            </tr>
            <tr id="rowUser" runat="server">
                <td style="border-bottom: 1px solid black; vertical-align: bottom;">
                    <asp:Label ID="lblLoadedBy" runat="server" Text="Loaded by:"></asp:Label>
                </td>
                <td style="border-bottom: 1px solid black;">
                    <asp:Literal ID="litUser" runat="server"></asp:Literal>&nbsp;
                </td>
            </tr>
            <tr id="rowApplicator" runat="server">
                <td style="border-bottom: 1px solid black; vertical-align: bottom;">
                    <asp:Label ID="lblApplicator" runat="server" Text="Applicator:"></asp:Label>
                </td>
                <td style="border-bottom: 1px solid black;">
                    <asp:Literal ID="litApplicator" runat="server"></asp:Literal>&nbsp;
                </td>
            </tr>
            <tr id="rowCustomPreLoadQuestions" runat="server">
                <td style="border-bottom: 1px solid black; vertical-align: top;">
                    <asp:Label ID="lblCustomPreLoadQuestions" runat="server" Text="Pre-Load Questions:"></asp:Label>
                </td>
                <td style="border-bottom: 1px solid black;">
                    <asp:Literal ID="litCustomPreLoadQuestions" runat="server"></asp:Literal>&nbsp;
                </td>
            </tr>
            <tr id="rowCustomPostLoadQuestions" runat="server">
                <td style="border-bottom: 1px solid black; vertical-align: top;">
                    <asp:Label ID="lblCustomPostLoadQuestions" runat="server" Text="Post-Load Questions:"></asp:Label>
                </td>
                <td style="border-bottom: 1px solid black;">
                    <asp:Literal ID="litCustomPostLoadQuestions" runat="server"></asp:Literal>&nbsp;
                </td>
            </tr>
            <tr id="rowDisclaimer" runat="server">
                <td style="border-bottom: 1px solid black; vertical-align: bottom; font-size: small;" colspan="2">
                    <span id="disclaimerText">
                        <asp:Literal ID="litDisclaimer" runat="server"></asp:Literal>&nbsp;</span>
                </td>
            </tr>
        </table>
        <br />
        <table id="BlankSpaces" runat="server" style="width: 100%">
            <tr>
                <td id="tdBlank1" style="width: 30%" visible="false" runat="server" class="BlankSpaceText">
                    <asp:Label ID="lblBlank1" runat="server" Text="Blank1"></asp:Label>
                </td>
                <td style="width: 3%"></td>
                <td id="tdBlank2" style="width: 30%" visible="false" runat="server" class="BlankSpaceText">
                    <asp:Label ID="lblBlank2" runat="server" Text="Blank2"></asp:Label>
                </td>
                <td style="width: 3%"></td>
                <td id="tdBlank3" style="width: 30%" visible="false" runat="server" class="BlankSpaceText">
                    <asp:Label ID="lblBlank3" runat="server" Text="Blank3"></asp:Label>
                </td>
            </tr>
            <tr>
                <td id="lineCell1" style="width: 30%; border-bottom: 1px solid black;" visible="false"
                    runat="server">&nbsp;
                </td>
                <td style="width: 3%"></td>
                <td id="lineCell2" style="width: 30%; border-bottom: 1px solid black;" visible="false"
                    runat="server">&nbsp;
                </td>
                <td style="width: 3%"></td>
                <td id="lineCell3" style="width: 30%; border-bottom: 1px solid black;" visible="false"
                    runat="server">&nbsp;
                </td>
            </tr>
        </table>
        <table style="width: 100%;">
            <tr>
                <td>
                    <asp:Literal ID="litTicketAddon" runat="server" Visible="False"></asp:Literal>
                </td>
            </tr>
        </table>
    </form>
</body>
</html>
