<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="AllContainers.aspx.vb"
	Inherits="KahlerAutomation.TerminalManagement2.AllContainers" MaintainScrollPositionOnPostback="true" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Containers : All Containers Report</title>
	<link rel="shortcut icon" href="FavIcon.ico" />
	<meta name="viewport" content="width=device-width" />
	<meta http-equiv="X-UA-Compatible" content="IE=edge" />
	<link rel="stylesheet" href="styles/site.css" />
	<link rel="stylesheet" href="styles/site-tablet.css" media="(max-width:768px)" />
	<link rel="stylesheet" href="styles/site-phone.css" media="(max-width:480px)" />
	<script type="text/javascript" src="scripts/jquery-1.11.1.min.js"></script>
	<script type="text/javascript" src="scripts/soap-1.0.1.js"></script>
	<script type="text/javascript" src="scripts/page-controller.js"></script>
	<script type="text/javascript">
		function DisplayAddEmailButton(value) {
			if (value != '') {
				document.getElementById('btnAddEmailAddress').style.visibility = 'visible';
			}
			else {
				document.getElementById('btnAddEmailAddress').style.visibility = 'hidden';
			}
		}
	</script>
</head>
<body>
	<form id="main" method="post" runat="server">
		<div class="recordControl">
			<asp:Button ID="btnShowReport" runat="server" Text="Show report" Width="120px" />
			&nbsp;
		<asp:Button ID="btnPrinterFriendly" runat="server" Text="Printer friendly" Width="120px" />
			&nbsp;
		<asp:Button ID="btnDownload" runat="server" Text="Download report" Width="120px" />
		</div>
		<div class="section">
			<div class="section">
				<asp:Label ID="lblFilterBy" runat="server" Text="Filter by:" /><br />
				<asp:DropDownList ID="ddlLocation" runat="server" Width="20%">
					<asp:ListItem>Facility</asp:ListItem>
				</asp:DropDownList>
				<asp:DropDownList ID="ddlStatus" runat="server" Width="20%">
					<asp:ListItem Value="-1">Status</asp:ListItem>
					<asp:ListItem Value="0">In facility</asp:ListItem>
					<asp:ListItem Value="1">In transit</asp:ListItem>
					<asp:ListItem Value="2">With customer</asp:ListItem>
				</asp:DropDownList>
				<asp:DropDownList ID="ddlProduct" runat="server" Width="20%">
					<asp:ListItem>Product</asp:ListItem>
				</asp:DropDownList>
				<asp:DropDownList ID="ddlOwner" runat="server" Width="20%">
					<asp:ListItem>Owner</asp:ListItem>
				</asp:DropDownList>
				<asp:DropDownList ID="ddlCustomerAccount" runat="server" Width="20%">
					<asp:ListItem>Customer account</asp:ListItem>
				</asp:DropDownList>
				<asp:CheckBox ID="cbxShowDeleted" runat="server" Text="Show deleted" Width="20%" />
				<asp:Label ID="lblSearch" runat="server" Text="Container number: "></asp:Label>
				<asp:TextBox ID="txbSearch" runat="server" Width="20%"></asp:TextBox>
			</div>
			<div class="sectionEven">
				<div>
					<label>
						Sort by:</label>
					<asp:DropDownList ID="ddlOrderBy" runat="server" Width="45%">
						<asp:ListItem Value="number">Number</asp:ListItem>
						<asp:ListItem Value="last_Inspected">Last inspected</asp:ListItem>
						<asp:ListItem Value="created">Created date</asp:ListItem>
					</asp:DropDownList>
					<asp:DropDownList ID="ddlAscDesc" runat="server" Width="15%">
						<asp:ListItem Value="asc">Ascending</asp:ListItem>
						<asp:ListItem Value="desc">Descending</asp:ListItem>
					</asp:DropDownList>
				</div>
			</div>
			<div class="sectionOdd">
				<h2>Configure Displayed Columns</h2>
				<div class="collapsingSection not" id="pnlDisplayedColumns" runat="server">
					<div class="sectionEven">
						<ul>
							<li>
								<asp:CheckBox ID="cbxNumberColumnShown" runat="server" Text="Number" />
							</li>
							<li>
								<asp:CheckBox ID="cbxLocationColumnShown" runat="server" Text="Location" />
							</li>
							<li>
								<asp:CheckBox ID="cbxStatusColumnShown" runat="server" Text="Status" />
							</li>
							<li>
								<asp:CheckBox ID="cbxProductColumnShown" runat="server" Text="Product" />
							</li>
							<li>
								<asp:CheckBox ID="cbxOwnerColumnShown" runat="server" Text="Owner" />
							</li>
							<li>
								<asp:CheckBox ID="cbxAccountColumnShown" runat="server" Text="Account" />
							</li>
							<li>
								<asp:CheckBox ID="cbxLastTransactionColumnShown" runat="server" Text="Last transaction" />
							</li>
							<li>
								<asp:CheckBox ID="cbxEmptyWeightColumnShown" runat="server" Text="Empty weight" />
							</li>
							<li>
								<asp:CheckBox ID="cbxVolumeColumnShown" runat="server" Text="Volume" />
							</li>
							<li>
								<asp:CheckBox ID="cbxProductWeightColumnShown" runat="server" Text="Product weight" />
							</li>
							<li>
								<asp:CheckBox ID="cbxInServiceColumnShown" runat="server" Text="In service" />
							</li>
							<li>
								<asp:CheckBox ID="cbxLastFilledColumnShown" runat="server" Text="Last filled" />
							</li>
							<li>
								<asp:CheckBox ID="cbxBulkProdEpaColumnShown" runat="server" Text="Bulk product EPA" />
							</li>
							<li>
								<asp:CheckBox ID="cbxSealNumberColumnShown" runat="server" Text="Seal number" />
							</li>
							<li>
								<asp:CheckBox ID="cbxTypeColumnShown" runat="server" Text="Type" />
							</li> 
						</ul>
					</div>
					<div class="sectionOdd">
						<ul>
							<li>
								<asp:CheckBox ID="cbxConditionColumnShown" runat="server" Text="Condition" />
							</li>
							<li>
								<asp:CheckBox ID="cbxLastChangedColumnShown" runat="server" Text="Last changed" />
							</li>
							<li>
								<asp:CheckBox ID="cbxManufacturedColumnShown" runat="server" Text="Manufactured" />
							</li>
							<li>
								<asp:CheckBox ID="cbxLastInspectedColumnShown" runat="server" Text="Last inspected" />
							</li>
							<li>
								<asp:CheckBox ID="cbxPassedInspectionColumnShown" runat="server" Text="Passed inspection" />
							</li>
							<li>
								<asp:CheckBox ID="cbxRefillableColumnShown" runat="server" Text="Refillable" />
							</li>
							<li>
								<asp:CheckBox ID="cbxLastCleanedColumnShown" runat="server" Text="Last cleaned" />
							</li>
							<li>
								<asp:CheckBox ID="cbxNotesColumnShown" runat="server" Text="Notes" />
							</li>
							<li>
								<asp:CheckBox ID="cbxSealBrokenColumnShown" runat="server" Text="Seal broken" />
							</li>
							<li>
								<asp:CheckBox ID="cbxPassedPressureTestColumnShown" runat="server" Text="Passed pressure test" />
							</li>
							<li>
								<asp:CheckBox ID="cbxOneWayValvePresentColumnShown" runat="server" Text="One-way valve present" />
							</li>
							<li>
								<asp:CheckBox ID="cbxForOrderIdColumnShown" runat="server" Text="For order" />
							</li>
							<li>
								<asp:CheckBox ID="cbxEquipmentColumnShown" runat="server" Text="Equipment" />
							</li>
							<li>
								<asp:CheckBox ID="cbxLastUserIdColumnShown" runat="server" Text="Last user" />
							</li>
							<li>
								<asp:CheckBox ID="cbxCreatedColumnShow" runat="server" Text="Created" />
							</li>
							<li>
								<asp:CheckBox ID="cbxAssignedLotColumnShow" runat="server" Text="Lot" />
							</li>
						</ul>
					</div>
					<div class="section">
						<asp:Button ID="btnConfigure" runat="server" Text="Save" ToolTip="Allows configuration of the 
                                    displayed columns. These options are then saved for next time the report is run"
							Width="180px" />
					</div>
				</div>
			</div>
		</div>
		<div class="section" id="pnlSendEmail" runat="server">
			<hr style="width: 100%; color: #003399;" />
			<div class="sectionOdd">
				<ul>
					<li>
						<label>
							E-mail to</label>
						<asp:TextBox ID="tbxEmailTo" Style="width: 45%;" runat="server" AutoPostBack="true"></asp:TextBox>
						<asp:Button ID="btnSendEmail" Style="width: 15%;" runat="server" Text="Send" />
					</li>
					<li id="rowAddAddress" runat="server">
						<label>
							Add address</label>
						<asp:DropDownList ID="ddlAddEmailAddress" runat="server" Style="width: 45%;" onchange="DisplayAddEmailButton(this.value);">
						</asp:DropDownList>
						<asp:Button ID="btnAddEmailAddress" runat="server" Style="width: 15%;" Text="Add"
							visibility="false" />
					</li>
					<li style="color: Red;">
						<asp:Literal ID="litEmailConfirmation" runat="server"></asp:Literal>
					</li>
				</ul>
			</div>
		</div>
	</form>
</body>
</html>
