<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="DefaultContainerLabel.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.DefaultContainerLabel" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>Container label</title>
	<asp:Literal ID="StyleSheet" runat="Server" />
</head>
<body>
	<form id="form1" runat="server">
		<table style="width: 100%;">
			<tr id="pnlTicketNumber" runat="server">
				<td>
					<asp:Label ID="lblTicketNumber" runat="server" Style="font-weight: bold;" Text="Ticket:"></asp:Label>
				</td>
				<td>
					<asp:Literal ID="litTicketNumber" runat="server"></asp:Literal>
				</td>
			</tr>
			<tr id="pnlOrderNumber" runat="server">
				<td>
					<asp:Label ID="lblOrderNumber" runat="server" Style="font-weight: bold;" Text="Order:"></asp:Label>
				</td>
				<td>
					<asp:Literal ID="litOrderNumber" runat="server"></asp:Literal>
				</td>
			</tr>
			<tr id="pnlDateTime" runat="server">
				<td>
					<asp:Label ID="lblDateTime" runat="server" Style="font-weight: bold;" Text="Date/time:"></asp:Label>
				</td>
				<td>
					<asp:Literal ID="litDateTime" runat="server"></asp:Literal>
				</td>
			</tr>
			<tr id="pnlSoldBy" runat="server">
				<td>
					<asp:Label ID="lblSoldBy" runat="server" Style="font-weight: bold;" Text="Sold by:"></asp:Label>
				</td>
				<td>
					<asp:Literal ID="litSoldBy" runat="server"></asp:Literal>
				</td>
			</tr>
			<tr id="pnlBranch" runat="server">
				<td>
					<asp:Label ID="lblBranch" runat="server" Style="font-weight: bold;" Text="Branch:"></asp:Label>
				</td>
				<td>
					<asp:Literal ID="litBranchLocation" runat="server"></asp:Literal>
				</td>
			</tr>
			<tr id="pnlSoldTo" runat="server">
				<td>
					<asp:Label ID="lblSoldTo" runat="server" Style="font-weight: bold;" Text="Sold to:"></asp:Label>
				</td>
				<td>
					<asp:Literal ID="litSoldTo" runat="server"></asp:Literal>
				</td>
			</tr>
			<tr id="pnlShipTo" runat="server">
				<td>
					<asp:Label ID="lblShipTo" runat="server" Style="font-weight: bold;" Text="Ship to:"></asp:Label>
				</td>
				<td>
					<asp:Literal ID="litShipTo" runat="server"></asp:Literal>
				</td>
			</tr>
			<tr id="pnlComments" runat="server">
				<td colspan="2">
					<asp:Literal ID="litComments" runat="server"></asp:Literal>
				</td>
			</tr>
			<tr>
				<td colspan="2">
					<asp:Literal ID="litProducts" runat="server"></asp:Literal>
				</td>
			</tr>
			<tr id="rowCarrier" runat="server">
				<td>
					<asp:Label ID="lblCarrier" runat="server" Text="Carrier:"></asp:Label>
				</td>
				<td>
					<asp:Literal ID="litCarrier" runat="server"></asp:Literal>&nbsp;
				</td>
			</tr>
			<tr id="rowTransport" runat="server">
				<td>
					<asp:Label ID="lblTransports" runat="server" Text="Transport(s):"></asp:Label>
				</td>
				<td>
					<asp:Literal ID="litTransports" runat="server"></asp:Literal>&nbsp;
				</td>
			</tr>
			<tr id="rowDriver" runat="server">
				<td>
					<asp:Label ID="lblDriver" runat="server" Text="Driver:"></asp:Label>
				</td>
				<td>
					<asp:Literal ID="litDriver" runat="server"></asp:Literal>&nbsp;
				</td>
			</tr>
			<tr id="rowUser" runat="server">
				<td>
					<asp:Label ID="lblLoadedBy" runat="server" Text="Loaded by:"></asp:Label>
				</td>
				<td>
					<asp:Literal ID="litUser" runat="server"></asp:Literal>&nbsp;
				</td>
			</tr>
			<tr id="rowCustomPreLoadQuestions" runat="server">
				<td>
					<asp:Label ID="lblCustomPreLoadQuestions" runat="server" Text="Pre-Load Questions:"></asp:Label>
				</td>
				<td>
					<asp:Literal ID="litCustomPreLoadQuestions" runat="server"></asp:Literal>&nbsp;
				</td>
			</tr>
			<tr id="rowCustomPostLoadQuestions" runat="server">
				<td>
					<asp:Label ID="lblCustomPostLoadQuestions" runat="server" Text="Post-Load Questions:"></asp:Label>
				</td>
				<td>
					<asp:Literal ID="litCustomPostLoadQuestions" runat="server"></asp:Literal>&nbsp;
				</td>
			</tr>
			<tr id="rowDisclaimer" runat="server">
				<td colspan="2"><span id="disclaimerText">
					<asp:Literal ID="litDisclaimer" runat="server"></asp:Literal>&nbsp;</span>
				</td>
			</tr>
		</table>
		<table id="BlankSpaces" runat="server" style="width: 100%">
			<tr>
				<td id="lineCell1" style="width: 100%; border-bottom: 1px solid black;" visible="false"
					runat="server">&nbsp;
				</td>
			</tr>
			<tr>
				<td id="tdBlank1" style="text-align: center;" visible="false" runat="server" class="BlankSpaceText">
					<asp:Label ID="lblBlank1" runat="server" Text="Blank1"></asp:Label>
				</td>
			</tr>
			<tr>
				<td id="lineCell2" style="border-bottom: 1px solid black;" visible="false"
					runat="server">&nbsp;
				</td>
			</tr>
			<tr>
				<td id="tdBlank2" style="text-align: center;" visible="false" runat="server" class="BlankSpaceText">
					<asp:Label ID="lblBlank2" runat="server" Text="Blank2"></asp:Label>
				</td>
			</tr>
			<tr>
				<td id="lineCell3" style="border-bottom: 1px solid black;" visible="false"
					runat="server">&nbsp;
				</td>
			</tr>
			<tr>
				<td id="tdBlank3" style="text-align: center;" visible="false" runat="server" class="BlankSpaceText">
					<asp:Label ID="lblBlank3" runat="server" Text="Blank3"></asp:Label>
				</td>
			</tr>
		</table>
		<asp:Label ID="lblLabelCount" runat="server" Style="float: right; position: absolute; top: 8px; right: 8px;"></asp:Label>
	</form>
</body>
</html>
