<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="Products.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.Products" MaintainScrollPositionOnPostback="true" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Products : Products</title>
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
	<form id="main" method="post" runat="server">
		<div class="recordSelection">
			<label>
				Product
			</label>
			<asp:DropDownList ID="ddlProducts" runat="server" AutoPostBack="True">
			</asp:DropDownList>
		</div>
		<div class="recordControl">
			<asp:Button ID="btnSave" runat="server" Text="Save" />
			<asp:Button ID="btnDelete" runat="server" Text="Delete" Enabled="False" />
			<asp:Label ID="lblStatus" runat="server" ForeColor="Red"></asp:Label>
			<span class="sectionRequiredField"><span class="required"></span>&nbsp;indicates required field </span>
		</div>
		<asp:Panel ID="pnlEven" runat="server" CssClass="sectionEven">
			<h1>General</h1>
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
						Owner
					</label>
					<asp:DropDownList ID="ddlOwner" runat="server" AutoPostBack="true">
					</asp:DropDownList>
				</li>
				<li>
					<label>
						Default unit
					</label>
					<span class="required">
						<asp:DropDownList ID="ddlUnit" runat="server">
						</asp:DropDownList>
					</span></li>
				<li>
					<label>
						Notes
					</label>
					<asp:TextBox ID="tbxNotes" runat="server" TextMode="MultiLine"></asp:TextBox>
				</li>
				<li>
					<label>
						EPA number
					</label>
					<asp:TextBox ID="tbxEpaNumber" runat="server"></asp:TextBox>
				</li>
				<li>
					<label>
						MSDS number
					</label>
					<asp:TextBox ID="tbxMsdsNumber" runat="server"></asp:TextBox>
				</li>
				<li>
					<label>
						Manufacturer
					</label>
					<asp:TextBox ID="tbxManufacturer" runat="server"></asp:TextBox>
				</li>
				<li>
					<label>
						Active ingredients
					</label>
					<asp:TextBox ID="tbxActiveIngredients" runat="server"></asp:TextBox>
				</li>
				<li>
					<label>
						Restrictions
					</label>
					<asp:TextBox ID="tbxRestrictions" runat="server"></asp:TextBox>
				</li>
				<li>
					<label>
						Max app. rate
					</label>
					<span class="input">
						<asp:TextBox ID="tbxMaxAppRate" runat="server" Style="text-align: right; width: 30%; min-width: 30px;"></asp:TextBox>
					</span></li>
				<li>
					<label>
						Min app. rate
					</label>
					<span class="input">
						<asp:TextBox ID="tbxMinAppRate" runat="server" Style="text-align: right; width: 30%; min-width: 30px;"></asp:TextBox>
					</span></li>
				<li>
					<label>
					</label>
					<asp:CheckBox ID="chkDoNotStack" runat="server" Text="Do not stack (for weigh-tanks)" />
				</li>
				<li>
					<label>
						Product group
					</label>
					<asp:DropDownList ID="ddlProductGroup" runat="server">
					</asp:DropDownList>
				</li>
				<li>
					<label>
					</label>
					<asp:CheckBox ID="chkHazardousMaterial" runat="server" Text="Hazardous material" />
				</li>
			</ul>
			<ul id="lstCustomFields" runat="server">
			</ul>
		</asp:Panel>
		<div class="sectionOdd" id="pnlProductInterfaceSetup" runat="server">
			<h2>Interface</h2>
			<div class="recordSelectionEvenOdd">
				<label>
					Interface setting</label>
				<asp:DropDownList ID="ddlProductInterface" runat="server" AutoPostBack="True">
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
					<li id="pnlInterfaceExchangeUnit" runat="server">
						<label>
							Interface exchange unit
						</label>
						<asp:DropDownList ID="ddlInterfaceUnit" runat="server">
						</asp:DropDownList>
					</li>
					<li id="pnlInterfaceOrderItemUnit" runat="server">
						<label>
							Order item unit
						</label>
						<asp:DropDownList ID="ddlInterfaceOrderItemUnit" runat="server">
						</asp:DropDownList>
					</li>
					<li id="pnlSplitProductFormulationFacility" runat="server">
						<label>
							Split product formulation facility
						</label>
						<asp:DropDownList ID="ddlSplitProductFormulationFacility" runat="server">
						</asp:DropDownList>
					</li>
					<li>
						<label>
							Order item sort priority
						</label>
						<asp:TextBox ID="tbxPriority" runat="server" MaxLength="50" Text="100" Style="text-align: right;"></asp:TextBox>
					</li>
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
		</div>
		<div class="section">
			<h1>Bulk products
			</h1>
		</div>
		<div class="recordSelection">
			<label>
				Facility
			</label>
			<asp:DropDownList ID="ddlLocation" runat="server" AutoPostBack="true">
			</asp:DropDownList>
		</div>
		<asp:Panel ID="pnlBulkProducts" runat="server" CssClass="section">
			<div class="sectionOdd">
				<table border="1" id="tblBulkProducts" runat="server">
					<tr>
						<th>Bulk product
						</th>
						<th>Portion
						</th>
						<th>Order
						</th>
					</tr>
				</table>
			</div>
			<div class="sectionEven">
				<ul class="addRemoveSection">
					<li><span class="addRemoveList">
						<label>
							Bulk product
						</label>
						<asp:DropDownList ID="ddlBulkProduct" runat="server" AutoPostBack="True">
						</asp:DropDownList>
					</span>
						<asp:Button ID="btnAddBulkProduct" runat="server" Text="Add" CssClass="addRemoveButton" />
					</li>
					<li>
						<label class="addRemoveList">
						</label>
						<asp:Button ID="btnRemoveBulkProduct" runat="server" Text="Remove" CssClass="addRemoveButton" />
					</li>
					<li><span class="addRemoveList">
						<label>
							<asp:Label ID="lblPortionUnit" runat="server" Text="Percent of total"></asp:Label></label>
						<asp:TextBox ID="tbxPercent" runat="server" Style="text-align: right;"></asp:TextBox>
					</span>
						<asp:Button ID="btnUpdateBulkProductPercent" runat="server" Text="Update %" CssClass="addRemoveButton" />
					</li>
				</ul>
			</div>
		</asp:Panel>
	</form>
</body>
</html>
