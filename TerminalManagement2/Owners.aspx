<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="Owners.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.Owners" %>

<!DOCTYPE html>
<html lang="en">
<head id="Head1" runat="server">
	<title>Owners : Owners</title>
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
	<form id="main" runat="server" defaultfocus="tbxName">
	<asp:Panel ID="pnlMain" runat="server">
		<div class="recordSelection">
			<label>
				Owner</label>
			<asp:DropDownList ID="ddlOwners" runat="server" AutoPostBack="True" />
		</div>
		<div class="recordControl">
			<asp:Button ID="btnSave" TabIndex="9" runat="server" Text="Save" />
			<asp:Button ID="btnDelete" runat="server" Text="Delete" Enabled="False" />
			<asp:Label ID="lblStatus" runat="server" ForeColor="Red" />
			<span class="sectionRequiredField"><span class="required"></span>&nbsp;indicates required
				field </span>
		</div>
		<asp:Panel ID="pnlEven" runat="server" CssClass="sectionEven">
			<h1>
				General</h1>
			<ul>
				<li>
					<label>
						Name</label>
					<span class="required">
						<asp:TextBox ID="tbxName" TabIndex="1" runat="server" /></span> </li>
				<li>
					<label>
						Number</label>
					<asp:TextBox ID="tbxOwnerNumber" TabIndex="1" runat="server" />
				</li>
				<li>
					<label>
						Address</label>
					<asp:TextBox ID="tbxAddress" TabIndex="2" runat="server" />
				</li>
				<li>
					<label>
						City</label>
					<asp:TextBox ID="tbxCity" TabIndex="3" runat="server" />
				</li>
				<li>
					<label>
						State</label>
					<asp:TextBox ID="tbxState" TabIndex="4" runat="server" />
				</li>
				<li>
					<label>
						Zip code</label>
					<asp:TextBox ID="tbxZip" TabIndex="5" runat="server" />
				</li>
				<li>
					<label>
						Country</label>
					<asp:TextBox ID="tbxCountry" TabIndex="5" runat="server" />
				</li>
				<li>
					<label>
						Phone</label>
					<asp:TextBox ID="tbxPhone" TabIndex="7" runat="server" />
				</li>
				<li>
					<label>
						E-mail</label>
					<asp:TextBox ID="tbxEmail" TabIndex="8" runat="server" />
				</li>
				<li>
					<label>
						Notes</label>
					<asp:TextBox ID="tbxNotes" TabIndex="6" runat="server" TextMode="MultiLine" />
				</li>
			</ul>
			<h1>
				Order completion</h1>
			Owner order completion settings are only used if the facility is configured to use
			the owner's order completion settings
			<ul>
				<li>
					<label>
					</label>
					<asp:CheckBox ID="cbxUsePercentageToDetermineOrderCompletion" runat="server" Text="Mark orders complete at"
						AutoPostBack="true" />&nbsp;
					<asp:TextBox ID="tbxCompletionPercentage" runat="server" Width="50px" />% </li>
				<li>
					<label>
					</label>
					<asp:CheckBox ID="cbxUseBatchCountToDetermineOrderCompletion" runat="server" Text="Mark orders complete when the requested batch count is reached" />
				</li>
			</ul>
			<h1>
				Orders</h1>
			<ul>
				<li>
					<label>
						Summary URL</label>
					<asp:TextBox ID="tbxOrderSummaryUrl" runat="server" />
				</li>
				<li>
					<label>
						Next order number</label>
					<asp:TextBox ID="tbxNextOrderNumber" runat="server" Enabled="False" />
				</li>
				<li>
					<label>
					</label>
					<asp:Button ID="btnSaveNextOrderNumber" runat="server" Text="Save next order number" />
				</li>
				<li>
					<label>
					</label>
					<asp:Label ID="lblSaveNextOrderNumber" runat="server" Text="Next order number saved successfully"
						ForeColor="Red" Visible="False" CssClass="input" />
				</li>
			</ul>
			<h1>
				Tickets</h1>
			<ul>
				<li>
					<label>
						Ticket URL</label>
					<asp:TextBox ID="tbxTicketURL" runat="server" />
				</li>
				<li>
					<label>
						Next ticket number</label>
					<asp:TextBox ID="tbxNextTicketNumber" runat="server" Enabled="False" />
				</li>
				<li>
					<label>
					</label>
					<asp:Button ID="btnSaveNextTicketNumber" runat="server" Text="Save next ticket number" />
				</li>
				<li>
					<label>
					</label>
					<asp:Label ID="lblSaveNextTicketNumber" runat="server" Text="Next ticket number saved successfully"
						Visible="False" ForeColor="Red" CssClass="input" />
				</li>
				<li>
					<label>
						Ticket prefix</label>
					<asp:TextBox ID="tbxTicketPrefix" runat="server" Enabled="False" />
				</li>
				<li>
					<label>
						Ticket suffix</label>
					<asp:TextBox ID="tbxTicketSuffix" runat="server" Enabled="False" />
				</li>
			</ul>
			<h1>
				Receiving PO completion settings</h1>
			<ul>
				<li>
					<label>
					</label>
					<span class="input">
						<asp:CheckBox ID="cbxUsePercentageToDetermineReceivingOrderCompletion" runat="server"
							Text="Mark receiving orders complete at" AutoPostBack="true" Checked="true" Width="60%" />
						<asp:TextBox ID="tbxReceivingOrderCompletionPercentage" runat="server" Width="30%" />%
					</span></li>
			</ul>
			<h1>
				Receiving PO Tickets</h1>
			<ul>
				<li>
					<label>
						Receiving PO ticket URL
					</label>
					<asp:TextBox ID="tbxReceivingTicketURL" runat="server"></asp:TextBox>
				</li>
			</ul>
			<ul id="lstCustomFields" runat="server">
			</ul>
		</asp:Panel>
		<div class="sectionOdd" id="pnlInterfaceSetup" runat="server">
			<h2>
				Interface</h2>
			<div class="recordSelectionEvenOdd">
				<label>
					Interface setting</label>
				<asp:DropDownList ID="ddlOwnerInterface" runat="server" AutoPostBack="True">
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
	</asp:Panel>
	<asp:Panel runat="server" ID="pnlOwnerDeleteWarning" Visible="false">
		<div class="section">
			<asp:Label ID="lblDeleteWarning" runat="server" ForeColor="Red" />
			<asp:Button ID="btnOwnerDeleteYes" runat="server" Width="50px" Text="Yes" />
			<asp:Button ID="btnOwnerDeleteNo" runat="server" Width="50px" Text="No" />
		</div>
	</asp:Panel>
	</form>
</body>
</html>
