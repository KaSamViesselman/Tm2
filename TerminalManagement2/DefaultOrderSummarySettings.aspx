<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="DefaultOrderSummarySettings.aspx.vb"
	MaintainScrollPositionOnPostback="true" Inherits="KahlerAutomation.TerminalManagement2.DefaultOrderSummarySettings" %>

<!DOCTYPE html>
<html lang="en">
<head id="Head1" runat="server">
	<title>General Settings : Default Order Summary Settings</title>
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
	<form id="main" method="post" runat="server" defaultfocus="cbxOrderSummaryShowAcres">
	<div class="recordSelection">
		<ul>
			<li>
				<label>
					Order owner
				</label>
				<asp:DropDownList ID="ddlOrderSummaryOwner" runat="server" AutoPostBack="true">
				</asp:DropDownList>
			</li>
			<li>
				<label>
				</label>
				<asp:Label ID="lblOrderSummarySettingsExist" runat="server" Text="Settings exist"
					ForeColor="#FF3300" Visible="False"></asp:Label>
			</li>
		</ul>
	</div>
	<div class="recordControl">
		<asp:Button ID="btnSaveOwnerOrderSummarySettings" runat="server" Text="Save Owner Order Summary Settings" />
		<asp:Button ID="btnDeleteOwnerOrderSummarySettings" runat="server" Text="Delete Owner Order Summary Settings" />
	</div>
	<div class="section">
		<div class="sectionEven">
			<ul>
				<li>
					<label>
					</label>
					<asp:CheckBox ID="cbxOrderSummaryShowAcres" runat="server" Text="Show acres" />
				</li>
				<li>
					<label>
					</label>
					<asp:CheckBox ID="cbxOrderSummaryShowBranch" runat="server" Text="Show branch location" />
				</li>
				<li>
					<label>
					</label>
					<asp:CheckBox ID="cbxShowCustomerAccountNumber" runat="server" Text="Show customer account number" />
				</li>
				<li>
					<label>
					</label>
					<asp:CheckBox ID="cbxOrderSummaryShowEmailAddress" runat="server" Text="Show e-mail address" />
				</li>
				<li>
					<label>
					</label>
					<asp:CheckBox ID="cbxOrderSummaryShowInterface" runat="server" Text="Show interface" />
				</li>
				<li>
					<label>
					</label>
					<asp:CheckBox ID="cbxOrderSummaryShowNotes" runat="server" Text="Show notes" />
				</li>
				<li>
					<label>
					</label>
					<asp:CheckBox ID="cbxOrderSummaryShowShipTo" runat="server" Text="Show ship to" />
				</li>
				<li>
					<label>
					</label>
					<asp:CheckBox ID="cbxOrderSummaryShowPoNumber" runat="server" Text="Show PO number" />
				</li>
				<li>
					<label>
					</label>
					<asp:CheckBox ID="cbxOrderSummaryShowReleaseNumber" runat="server" Text="Show release number" />
				</li>
				<li>
					<label>
					</label>
					<asp:CheckBox ID="cbxOrderSummaryUseTicketDeliveredAmounts" runat="server" Text="Use ticket delivered amounts for delivered quantity" />
				</li>
			</ul>
		</div>
		<div class="sectionOdd">
			<ul>
				<li>
					<label>
						Show additional units
					</label>
					<asp:CheckBoxList ID="cblAdditionalUnitsForOrderSummary" runat="server" RepeatLayout="UnorderedList"
						CssClass="input">
					</asp:CheckBoxList>
				</li>
				<li>
					<label>
						Show additional units for product groups</label>
					<span class="input"><span class="addRemoveSection">
						<asp:DropDownList ID="ddlOrderSummaryProductGroup" runat="server" AutoPostBack="true"
							CssClass="addRemoveList">
						</asp:DropDownList>
						<asp:Button ID="btnSaveOrderSummaryProductGroupAdditionalUnits" runat="server" Text="Save"
							AutoPostBack="true" CssClass="addRemoveButton" /></span></span> </li>
				<li>
					<label>
					</label>
					<asp:CheckBoxList ID="cblOrderSummaryAdditionalUnitsForProductGroup" runat="server"
						RepeatLayout="UnorderedList" CssClass="input">
					</asp:CheckBoxList>
				</li>
				<li id="pnlOrderSummaryCustomFieldsAssigned" runat="server" visible="false">
					<label>
						Show custom fields on order summary
					</label>
					<span class="input">
						<asp:CheckBox ID="cbxShowAllCustomFieldsOnOrderSummary" runat="server" Text="Show all custom fields on order summary"
							AutoPostBack="true" />
						<asp:CheckBoxList ID="cblShowCustomFieldsOnOrderSummary" runat="server" Enabled="false"
							RepeatLayout="UnorderedList" Style="border: 1 solid black !important;">
						</asp:CheckBoxList>
					</span></li>
			</ul>
		</div>
	</div>
	</form>
</body>
</html>
