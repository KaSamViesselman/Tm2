<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="Tanks.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.Tanks" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Tanks : Tanks</title>
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
	<form id="main" runat="server">
		<div class="recordSelection">
			<label>
				Tank</label>
			<asp:DropDownList ID="ddlTanks" runat="server" AutoPostBack="True" />
		</div>
		<div class="recordControl">
			<asp:Button ID="btnSave" runat="server" Width="120px" Text="Save" />
			<asp:Button ID="btnDelete" runat="server" Width="120px" Text="Delete" />
			<asp:Label ID="lblStatus" runat="server" ForeColor="Red" />
			<div class="sectionRequiredField">
				<label>
					<span class="required"></span>&nbsp;indicates required field
				</label>
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
				<li>
					<label>
						Owner</label>
					<asp:DropDownList ID="ddlOwner" runat="server" AutoPostBack="true" />
				</li>
				<li>
					<label>
						Facility</label>
					<asp:DropDownList ID="ddlLocation" runat="server" />
				</li>
				<li>
					<label>
						Bulk product</label>
					<asp:DropDownList ID="ddlBulkProduct" runat="server" />
				</li>
				<li>
					<label>
						Unit</label>
					<asp:DropDownList ID="ddlUnit" runat="server" />
				</li>
				<li>
					<label>
						Panel</label>
					<asp:DropDownList ID="ddlPanel" runat="server" AutoPostBack="True" />
					<asp:LinkButton ID="lbtConfigure" runat="server" Text="Configure" />
				</li>
				<li>
					<label>
						Sensor</label>
					<asp:DropDownList ID="ddlSensor" runat="server" />
				</li>
				<li>
					<label>
						E-mail alerts (separate by comma)</label>
					<asp:TextBox ID="tbxEmail" runat="server" />
				</li>
			</ul>
		</asp:Panel>
		<div id="pnlLastReportedValues" runat="server" class="sectionEven">
			<h2>Last updated values</h2>
			<ul>
				<li>
					<label>
						Last updated
					</label>
					<asp:Label ID="lblLastUpdated" runat="server" CssClass="input"></asp:Label>
				</li>
				<li>
					<label>
						Alarms
					</label>
					<asp:Label ID="lblAlarms" runat="server" CssClass="input"></asp:Label>
				</li>
				<li>
					<label>
						Level
					</label>
					<asp:Label ID="lblLevel" runat="server" CssClass="input"></asp:Label>
				</li>
				<li>
					<label>
						Max. Height
					</label>
					<asp:Label ID="lblMaxHeight" runat="server" CssClass="input"></asp:Label>
				</li>
				<li>
					<label>
						Quantity
					</label>
					<asp:Label ID="lblQuantity" runat="server" CssClass="input"></asp:Label>
				</li>
				<li>
					<label>
						Capacity
					</label>
					<asp:Label ID="lblCapacity" runat="server" CssClass="input"></asp:Label>
				</li>
				<li>
					<label>
						Temperature
					</label>
					<asp:Label ID="lblTemperature" runat="server" CssClass="input"></asp:Label>
				</li>
			</ul>
		</div>
		<div id="pnlInterfaceSetup" runat="server" class="sectionOdd">
			<h2>Interface</h2>
			<div class="recordSelectionEvenOdd">
				<label>
					Interface setting</label>
				<asp:DropDownList ID="ddlTankInterface" runat="server" AutoPostBack="True">
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
	</form>
</body>
</html>
