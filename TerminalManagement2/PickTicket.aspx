<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="PickTicket.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.PickTicket"
	EnableSessionState="False" EnableViewState="false" ViewStateMode="Disabled" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Pick Ticket</title>
	<asp:Literal ID="StyleSheet" runat="Server" />
</head>
<body>
	<form id="form1" runat="server">
		<div>
			<asp:Literal ID="litTicket" runat="server"></asp:Literal>
		</div>
	</form>
</body>
</html>
