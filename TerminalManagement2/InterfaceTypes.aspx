<%@ Page Language="vb" MaintainScrollPositionOnPostback="true" AutoEventWireup="false"
	CodeBehind="InterfaceTypes.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.InterfaceTypes" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Interfaces : Interface Types</title>
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
	<form id="main" runat="server" method="post" defaultfocus="ddlInterfaceTypes">
	<div class="recordSelection">
		<label>
			Interface type
		</label>
		<asp:DropDownList ID="ddlInterfaceTypes" runat="server" AutoPostBack="True">
		</asp:DropDownList>
	</div>
	<div class="recordControl">
		<asp:Button ID="btnSave" runat="server" Text="Save"></asp:Button>
		<asp:Button ID="btnDelete" runat="server" Text="Delete"></asp:Button><br />
		<asp:Label ID="lblStatus" runat="server" ForeColor="Red"></asp:Label>
		<span class="sectionRequiredField"><span class="required"></span>&nbsp;indicates required
			field </span>
	</div>
	<asp:Panel ID="pnlMain" runat="server" CssClass="section">
		<h1>
			General</h1>
		<div class="sectionEven">
			<ul>
				<li>
					<label>
						Name
					</label>
					<span class="required">
						<asp:TextBox ID="tbxName" runat="server" MaxLength="50"></asp:TextBox>
					</span></li>
				<li>
					<label>
						Configuration URL
					</label>
					<span class="required">
						<asp:TextBox ID="tbxConfigUrl" runat="server"></asp:TextBox>
					</span></li>
				<li>
					<label>
					</label>
					<asp:CheckBox ID="chkShowApplicatorSetup" runat="server" Text="Show interface setup for applicators" />
				</li>
				<li>
					<label>
					</label>
					<asp:CheckBox ID="chkShowBranchSetup" runat="server" Text="Show interface setup for branches" />
				</li>
				<li>
					<label>
					</label>
					<asp:CheckBox ID="chkShowBulkProductSetup" runat="server" Text="Show interface setup for bulk products" />
				</li>
				<li>
					<label>
					</label>
					<asp:CheckBox ID="chkShowCarrierSetup" runat="server" Text="Show interface setup for carriers" />
				</li>
				<li>
					<label>
					</label>
					<asp:CheckBox ID="chkShowDriverSetup" runat="server" Text="Show interface setup for drivers" />
				</li>
				<li>
					<label>
					</label>
					<asp:CheckBox ID="chkShowFacilitySetup" runat="server" Text="Show interface setup for facilities" />
				</li>
				<li>
					<label>
					</label>
					<asp:CheckBox ID="chkShowOwnerSetup" runat="server" Text="Show interface setup for owners" />
				</li>
				<li>
					<label>
					</label>
					<asp:CheckBox ID="chkShowTankSetup" runat="server" Text="Show interface setup for tanks" />
				</li>
				<li>
					<label>
					</label>
					<asp:CheckBox ID="chkShowTransportSetup" runat="server" Text="Show interface setup for transports" />
				</li>
				<li>
					<label>
					</label>
					<asp:CheckBox ID="chkShowTransportTypeSetup" runat="server" Text="Show interface setup for transport types" />
				</li>
				<li>
					<label>
					</label>
					<asp:CheckBox ID="chkShowUnitSetup" runat="server" Text="Show interface setup for units" />
				</li>
			</ul>
		</div>
		<div class="sectionOdd">
			<ul>
				<li>
					<label>
					</label>
					<asp:CheckBox ID="chkShowInterfaceExchangeUnit" runat="server" Text="Show interface exchange unit of measure" />
				</li>
				<li>
					<label>
					</label>
					<asp:CheckBox ID="chkUseInterfaceUnitAsOrderItemUnit" runat="server" Text="Use the interface request unit of measure for the order item's requested unit of measure" />
				</li>
				<li>
					<label>
					</label>
					<asp:CheckBox ID="chkSplitImportedProductsIntoBulkProductOrderItems" runat="server"
						Text="Split imported products into separate bulk product order items" />
				</li>
				<li>
					<label>
					</label>
					<asp:CheckBox ID="chkAllowOrderProductChange" runat="server" Text="Allow user to change the product on an imported order" />
				</li>
				<li>
					<label>
					</label>
					<asp:CheckBox ID="chkAllowOrderCustomerChange" runat="server" Text="Allow user to change the customer on an imported order" />
				</li>
				<li>
					<label>
					</label>
					<asp:CheckBox ID="chkAllowOrderProductGroupingChange" runat="server" Text="Allow user to change the grouping for products on an imported order" />
				</li>
				<li>
					<label>
					</label>
					<asp:CheckBox ID="chkAllowOrderStatusChangeTicketsExist" runat="server" Text="Allow order to be marked as incomplete if tickets exist" />
				</li>
			</ul>
		</div>
	</asp:Panel>
	</form>
</body>
</html>
