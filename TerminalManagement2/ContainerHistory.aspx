<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="ContainerHistory.aspx.vb"
	Inherits="KahlerAutomation.TerminalManagement2.ContainerHistory" MaintainScrollPositionOnPostback="true" %>

<!DOCTYPE html>
<html lang="en">
<head id="Head1" runat="server">
	<title>Containers : Container History</title>
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
	<form id="main" runat="server">
		<div class="recordControl">
			<asp:Button ID="btnPrinterFriendly" runat="server" Text="Printer friendly" Width="120px" />
			&nbsp;
		<asp:Button ID="btnDownload" runat="server" Text="Download report" Width="120px" />
		</div>
		<div class="section">
			<h2>Configure Displayed Columns</h2>
			<div class="collapsingSection not">
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
							<asp:CheckBox ID="cbxLastTransactionColumnShown" runat="server" Text="Last Transaction" />
						</li>
						<li>
							<asp:CheckBox ID="cbxEmptyWeightColumnShown" runat="server" Text="Empty Weight" />
						</li>
						<li>
							<asp:CheckBox ID="cbxVolumeColumnShown" runat="server" Text="Volume" />
						</li>
						<li>
							<asp:CheckBox ID="cbxProductWeightColumnShown" runat="server" Text="Product Weight" />
						</li>
						<li>
							<asp:CheckBox ID="cbxInServiceColumnShown" runat="server" Text="In Service" />
						</li>
						<li>
							<asp:CheckBox ID="cbxLastFilledColumnShown" runat="server" Text="Last Filled" />
						</li>
						<li>
							<asp:CheckBox ID="cbxBulkProdEpaColumnShown" runat="server" Text="Bulk Product Epa" />
						</li>
						<li>
							<asp:CheckBox ID="cbxSealNumberColumnShown" runat="server" Text="Seal Number" />
						</li>
						<li>
							<asp:CheckBox ID="cbxCreatedColumnShow" runat="server" Text="Created" />
						</li>
						<li>
							<asp:CheckBox ID="cbxLastUserIdColumnShown" runat="server" Text="Last user" />
						</li>
					</ul>
				</div>
				<div class="sectionOdd">
					<ul>
						<li>
							<asp:CheckBox ID="cbxTypeColumnShown" runat="server" Text="Type" />
						</li>
						<li>
							<asp:CheckBox ID="cbxConditionColumnShown" runat="server" Text="Condition" />
						</li>
						<li>
							<asp:CheckBox ID="cbxLastChangedColumnShown" runat="server" Text="Last Changed" />
						</li>
						<li>
							<asp:CheckBox ID="cbxManufacturedColumnShown" runat="server" Text="Manufactured" />
						</li>
						<li>
							<asp:CheckBox ID="cbxLastInspectedColumnShown" runat="server" Text="Last Inspected" />
						</li>
						<li>
							<asp:CheckBox ID="cbxPassedInspectionColumnShown" runat="server" Text="Passed Inspection" />
						</li>
						<li>
							<asp:CheckBox ID="cbxRefillableColumnShown" runat="server" Text="Refillable" />
						</li>
						<li>
							<asp:CheckBox ID="cbxLastCleanedColumnShown" runat="server" Text="Last Cleaned" />
						</li>
						<li>
							<asp:CheckBox ID="cbxNotesColumnShown" runat="server" Text="Notes" />
						</li>
						<li>
							<asp:CheckBox ID="cbxSealBrokenColumnShown" runat="server" Text="Seal Broken" />
						</li>
						<li>
							<asp:CheckBox ID="cbxPassedPressureTestColumnShown" runat="server" Text="Passed Pressure Test" />
						</li>
						<li>
							<asp:CheckBox ID="cbxOneWayValvePresentColumnShown" runat="server" Text="One Way Valve Present" />
						</li>
						<li>
							<asp:CheckBox ID="cbxForOrderIdColumnShown" runat="server" Text="For Order" />
						</li>
						<li>
							<asp:CheckBox ID="cbxEquipmentColumnShown" runat="server" Text="Equipment" />
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
		<div class="section">
			<asp:Literal ID="litContainers" runat="server"></asp:Literal>
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
