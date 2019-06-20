<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="DefaultReceivingWebPickTicketSettings.aspx.vb"
	Inherits="KahlerAutomation.TerminalManagement2.DefaultReceivingWebPickTicketSettings" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>General Settings : Default Receiving Web Pick Ticket Settings</title>
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
	<form id="main" method="post" runat="server" defaultfocus="cbxSeparateTicketNumberPerOwner">
	<div class="recordSelection">
		<ul>
			<li>
				<label>
					Ticket owner
				</label>
				<asp:DropDownList ID="ddlWebTicketOwner" runat="server" AutoPostBack="true">
				</asp:DropDownList>
			</li>
			<li>
				<label>
				</label>
				<asp:Label ID="lblWebTicketSettingsExist" runat="server" Text="Settings exist" ForeColor="#FF3300"
					Visible="False"></asp:Label>
			</li>
		</ul>
	</div>
	<div class="recordControl">
		<asp:Button ID="btnSaveOwnerWebTicketSettings" runat="server" Text="Save Owner Web Ticket Settings" />
		<asp:Button ID="btnDeleteOwnerWebTicketSettings" runat="server" Text="Delete Owner Web Ticket Settings" />
	</div>
	<div class="section">
		<div class="sectionEven">
			<ul>
				<li>
					<label>
					</label>
					<asp:CheckBox ID="cbxShowSupplier" runat="server" Text="Show supplier" />
				</li>
				<li>
					<label>
					</label>
					<asp:CheckBox ID="cbxShowCarrierId" runat="server" Text="Show carrier" />
				</li>
				<li>
					<label>
					</label>
					<asp:CheckBox ID="cbxShowDate" runat="server" Text="Show date and " AutoPostBack="true" />
					<asp:CheckBox ID="cbxShowTime" runat="server" Text="show time" />
				</li>
				<li>
					<label>
					</label>
					<asp:CheckBox ID="cbxShowDensityOnTicket" runat="server" Text="Show density on ticket" />
				</li>
				<li>
					<label>
					</label>
					<asp:CheckBox ID="cbxShowDriverName" runat="server" AutoPostBack="true" Text="Show driver" />
				</li>
				<li>
					<label>
					</label>
					<asp:CheckBox ID="cbxShowDriverNumber" runat="server" Text="Show driver number" Style="margin-left: 2em;" />
				</li>
				<li>
					<label>
					</label>
					<asp:CheckBox ID="cbxShowEmailAddress" runat="server" Text="Show e-mail address" />
				</li>
				<li>
					<label>
					</label>
					<asp:CheckBox ID="cbxShowFacility" runat="server" Text="Show facility" />
				</li>
				<li>
					<label>
					</label>
					<asp:CheckBox ID="cbxShowGrossWeight" runat="server" Text="Show gross weight"></asp:CheckBox>
				</li>
				<li>
					<label>
					</label>
					<asp:CheckBox ID="cbxShowOwner" runat="server" Text="Show owner" />
				</li>
				<li>
					<label>
					</label>
					<asp:CheckBox ID="cbxShowTransport" runat="server" Text="Show transport"></asp:CheckBox>
				</li>
			</ul>
		</div>
		<div class="sectionOdd">
			<ul>
				<li>
					<label>
						Show additional units
					</label>
					<asp:CheckBoxList ID="cblAdditionalUnitsForTicket" runat="server" RepeatLayout="UnorderedList"
						CssClass="input">
					</asp:CheckBoxList>
				</li>
				<li>
					<label>
						Density unit precision
					</label>
					<ul class="input">
						<li class="addRemoveSection"><span class="addRemoveList">
							<asp:DropDownList ID="ddlWebTicketDensityMass" runat="server" AutoPostBack="True"
								Style="width: auto;">
							</asp:DropDownList>
							&nbsp;/
							<asp:DropDownList ID="ddlWebTicketDensityVolume" runat="server" AutoPostBack="True"
								Style="width: auto;">
							</asp:DropDownList>
						</span>
							<asp:Button ID="btnWebTicketDensityAdd" runat="server" Text="Add" CssClass="addRemoveButton" />
						</li>
						<li class="addRemoveSection">
							<asp:ListBox ID="lstWebTicketDensityList" runat="server" AutoPostBack="True" CssClass="addRemoveList">
							</asp:ListBox>
							<asp:Button ID="btnWebTicketDensityRemove" runat="server" Text="Remove" CssClass="addRemoveButton" />
							<ul class="addRemoveButton" id="trWebTicketDensityPrecisionControls" runat="server"
								visible="false">
								<li>
									<ul>
										<li>
											<label>
												Whole
											</label>
											<span class="input">
												<asp:Button ID="btnWebTicketDensityAddWhole" runat="server" Text="+" CssClass="button"
													Style="width: auto;" />
												<asp:Button ID="btnWebTicketDensityRemoveWhole" runat="server" Text="-" CssClass="button"
													Style="width: auto;" /></span> </li>
										<li>
											<label>
												Fractional
											</label>
											<span class="input">
												<asp:Button ID="btnWebTicketDensityAddFractional" runat="server" Text="+" CssClass="button"
													Style="width: auto;" />
												<asp:Button ID="btnWebTicketDensityRemoveFractional" runat="server" Text="-" CssClass="button"
													Style="width: auto;" /></span> </li>
									</ul>
								</li>
							</ul>
						</li>
					</ul>
				</li>
				<li>
					<label>
						Ticket logo
					</label>
					<asp:TextBox ID="tbxTicketLogo" runat="server"></asp:TextBox>
				</li>
				<li>
					<label>
						Owner message
					</label>
					<asp:TextBox ID="tbxOwnerMessage" runat="server" Text="" TextMode="MultiLine"></asp:TextBox>
				</li>
				<li>
					<label>
						Disclaimer
					</label>
					<asp:TextBox ID="tbxOwnerDisclaimer" runat="server" Text="" TextMode="MultiLine"></asp:TextBox>
				</li>
			</ul>
		</div>
	</div>
	</form>
</body>
</html>
