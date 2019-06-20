<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="ReceivingPoSettings.aspx.vb"
	Inherits="KahlerAutomation.TerminalManagement2.ReceivingPoSettings" %>

<!DOCTYPE html>
<html lang="en">
<head id="Head1" runat="server">
	<title>General Settings : Receiving PO Settings</title>
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
	<form id="main" method="post" runat="server" defaultfocus="cbxAutoGenerateReceivingOrderNumbers">
	<div class="recordControl">
		<asp:Button ID="btnSave" runat="server" Text="Save" />
		<asp:Label ID="lblSave" runat="server" ForeColor="Red" Text="Receiving PO settings were saved successfully"
			Visible="False" />
	</div>
	<div class="sectionEven" id="pnlReceivingPoSettings" runat="server">
		<ul>
			<li>
				<h1>
					Purchase order numbers
				</h1>
			</li>
			<li>
				<label>
				</label>
				<asp:CheckBox ID="cbxAutoGenerateReceivingOrderNumbers" runat="server" Text="Automatically generate sequential order numbers"
					AutoPostBack="True"></asp:CheckBox>
			</li>
			<li>
				<label>
				</label>
				<asp:CheckBox ID="cbxAllowModificationReceivingOrderNumber" runat="server" Text="Allow order numbers to be modified"
					AutoPostBack="True" />
			</li>
			<li>
				<label>
				</label>
				<asp:CheckBox ID="cbxSeparateReceivingOrderNumberPerOwner" runat="server" Text="Use separate order numbers per owner"
					AutoPostBack="true" />
			</li>
			<li>
				<label>
					Starting order number
				</label>
				<asp:TextBox ID="tbxStartingReceivingOrderNumber" runat="server"></asp:TextBox>
			</li>
		</ul>
	</div>
	<div class="sectionOdd">
		<ul>
			<li>
				<h1>
					E-mail tickets
				</h1>
			</li>
			<li>
				<label>
				</label>
				<asp:CheckBox ID="cbxEmailCreatedPointOfSaleTickets" runat="server" Text="E-mail created Point of Sale tickets" />
			</li>
            <li><label></label></li>
			<li>
				<label>
					Send tickets to:
				</label>
				<asp:CheckBox ID="cbxEmailReceivingTicketOwner" runat="server" Text="Owner" />
			</li>
			<li>
				<label>
				</label>
				<asp:CheckBox ID="cbxEmailReceivingTicketSupplierAccount" runat="server" Text="Supplier" />
			</li>
			<li>
				<label>
				</label>
				<asp:CheckBox ID="cbxEmailReceivingTicketCarrier" runat="server" Text="Carrier" />
			</li>
			<li>
				<label>
				</label>
				<asp:CheckBox ID="cbxEmailReceivingTicketDriver" runat="server" Text="Driver" />
			</li>
			<li>
				<label>
				</label>
				<asp:CheckBox ID="cbxEmailReceivingTicketLocation" runat="server" Text="Facility" />
			</li>
		</ul>
	</div>
	</form>
</body>
</html>
