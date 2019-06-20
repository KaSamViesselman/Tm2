<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="ReceivingPickTicket.aspx.vb"
	Inherits="KahlerAutomation.TerminalManagement2.ReceivingPickTicket" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>Receiving Pick Ticket</title>
	<asp:Literal ID="StyleSheet" runat="Server" />
</head>
<body>
	<form id="ticketForm" method="post" runat="server">
		<table style="width: 100%;">
			<tr>
				<td style="vertical-align: top;">
					<img id="imgLogo" runat="server" alt=" " src="images/Kahler-logo-standard.png" align="left" />
					<strong>Purchase Order:</strong>
					<asp:Literal ID="litOrderNumber" runat="server"></asp:Literal>
				</td>
				<td style="vertical-align: top;"></td>
			</tr>
			<tr id="pnlDateTime" runat="server">
				<td>
					<strong>Date/time:</strong>
					<asp:Literal ID="litDateTime" runat="server"></asp:Literal>
				</td>
				<td style="vertical-align: top;"></td>
			</tr>
			<tr>
				<td colspan="2">
					<asp:Literal ID="litComments" runat="server"></asp:Literal>
				</td>
			</tr>
			<tr id="pnlSoldBy" runat="server">
				<td style="vertical-align: top;">
					<strong>Sold by:</strong><br />
					<asp:Literal ID="litSoldBy" runat="server"></asp:Literal>
				</td>
				<td style="vertical-align: top;"></td>
			</tr>
			<tr>
				<td id="pnlOwnerMessage" runat="server" colspan="2">
					<br />
					<asp:Literal ID="litOwnerMessage" runat="server"></asp:Literal>
					<br />
				</td>
			</tr>
			<tr>
				<td style="vertical-align: top;">
					<asp:Panel ID="pnlSoldTo" runat="server">
						<strong>Sold to:</strong><br />
						<asp:Literal ID="litSoldTo" runat="server"></asp:Literal>
					</asp:Panel>
				</td>
				<td style="vertical-align: top;">
					<asp:Panel ID="pnlFacility" runat="server">
						<strong>Facility:</strong><br />
						<asp:Literal ID="litFacility" runat="server"></asp:Literal>
					</asp:Panel>
				</td>
			</tr>
		</table>
		<asp:Literal ID="litProducts" runat="server"></asp:Literal>
		<br />
		<table cellspacing="0" cellpadding="3" style="width: 100%;">
			<tr id="rowCarrier" runat="server">
				<td style="border-bottom: 1px solid black; vertical-align: bottom;">Carrier:
				</td>
				<td style="border-bottom: 1px solid black;">
					<asp:Literal ID="litCarrier" runat="server"></asp:Literal>&nbsp;
				</td>
			</tr>
			<tr id="rowTransport" runat="server">
				<td style="border-bottom: 1px solid black; vertical-align: text-top;">Transport:
				</td>
				<td style="border-bottom: 1px solid black; vertical-align: text-top;">
					<asp:Literal ID="litTransports" runat="server"></asp:Literal>&nbsp;
				</td>
			</tr>
			<tr id="rowDriver" runat="server">
				<td style="border-bottom: 1px solid black; vertical-align: bottom;">Driver:
				</td>
				<td style="border-bottom: 1px solid black;">
					<asp:Literal ID="litDriver" runat="server"></asp:Literal>&nbsp;
				</td>
			</tr>
			<tr id="rowGrossWeight" runat="server">
				<td style="border-bottom: 1px solid black; vertical-align: bottom;">Gross weight:
				</td>
				<td style="border-bottom: 1px solid black;">
					<asp:Literal ID="litGrossWeight" runat="server"></asp:Literal>&nbsp;
				</td>
			</tr>
			<tr id="rowDisclaimer" runat="server">
				<td style="border-bottom: 1px solid black; vertical-align: bottom;" colspan="2">
					<asp:Literal ID="litDisclaimer" runat="server"></asp:Literal>&nbsp;
				</td>
			</tr>
		</table>
	</form>
</body>
</html>
