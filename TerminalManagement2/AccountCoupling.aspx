<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="AccountCoupling.aspx.vb"
	Inherits="KahlerAutomation.TerminalManagement2.AccountCoupling" %>

<!DOCTYPE html>
<html lang="en">
<head id="Head1" runat="server">
	<title>Customer Accounts : Account Coupling</title>
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
	<form id="main" method="post" runat="server" defaultfocus="ddlAccounts">
	<div class="recordSelection">
		<label>
			Account
		</label>
		<asp:DropDownList ID="ddlAccounts" runat="server" AutoPostBack="True">
		</asp:DropDownList>
	</div>
	<div class="recordControl">
		<asp:Button ID="btnCreateAssociation" runat="server" Text="Create Association" Visible="False">
		</asp:Button>
		<asp:Label ID="lblStatus" runat="server" ForeColor="#ff0000"></asp:Label>
	</div>
	<asp:Panel ID="pnlMain" runat="server" CssClass="section">
		<h1>
			Account Coupling</h1>
		<ul>
			<li>
				<asp:Literal ID="litOutput" runat="server"></asp:Literal>
			</li>
			<li>
				<div class="section">
					<div class="sectionEven">
						<asp:Label ID="lblAccount1" runat="server" Visible="False" Width="30%" />
						<asp:DropDownList ID="ddlAccounts1" runat="server" Visible="False" Width="30%" />
						<asp:Button ID="btnAdd1" runat="server" Text="Add" Visible="False" Width="75px">
						</asp:Button>
						<asp:Button ID="btnRemove1" runat="server" Text="Remove" Visible="False" Width="75px">
						</asp:Button>
					</div>
					<div class="sectionOdd">
						<asp:Label ID="lblPercent1" runat="server" Visible="False">Percentage</asp:Label>
						<asp:TextBox ID="tbxPercent1" runat="server" Visible="False" Width="34px">00</asp:TextBox>
						<asp:Button ID="btnPercent1" runat="server" Text="Update Percent" Visible="False"
							Width="120px"></asp:Button>
					</div>
				</div>
			</li>
			<li>
				<div class="section">
					<div class="sectionEven">
						<asp:Label ID="lblAccount2" runat="server" Visible="False" Width="30%" />
						<asp:DropDownList ID="ddlAccounts2" runat="server" Visible="False" Width="30%">
						</asp:DropDownList>
						<asp:Button ID="btnAdd2" runat="server" Text="Add" Visible="False" Width="75px">
						</asp:Button>
						<asp:Button ID="btnRemove2" runat="server" Text="Remove" Visible="False" Width="75px">
						</asp:Button>
					</div>
					<div class="sectionOdd">
						<asp:Label ID="lblPercent2" runat="server" Visible="False">Percentage</asp:Label>
						<asp:TextBox ID="tbxPercent2" runat="server" Visible="False" Width="34px">00</asp:TextBox>
						<asp:Button ID="btnPercent2" runat="server" Text="Update Percent" Visible="False"
							Width="120px"></asp:Button>
					</div>
				</div>
			</li>
			<li>
				<div class="section">
					<div class="sectionEven">
						<asp:Label ID="lblAccount3" runat="server" Visible="False" Width="30%" />
						<asp:DropDownList ID="ddlAccounts3" runat="server" Visible="False" Width="30%">
						</asp:DropDownList>
						<asp:Button ID="btnAdd3" runat="server" Text="Add" Visible="False" Width="75px">
						</asp:Button>
						<asp:Button ID="btnRemove3" runat="server" Text="Remove" Visible="False" Width="75px">
						</asp:Button>
					</div>
					<div class="sectionOdd">
						<asp:Label ID="lblPercent3" runat="server" Visible="False">Percentage</asp:Label>
						<asp:TextBox ID="tbxPercent3" runat="server" Visible="False" Width="34px">00</asp:TextBox>
						<asp:Button ID="btnPercent3" runat="server" Text="Update Percent" Visible="False"
							Width="120px"></asp:Button>
					</div>
				</div>
			</li>
			<li>
				<div class="section">
					<div class="sectionEven">
						<asp:Label ID="lblAccount4" runat="server" Visible="False" Width="30%" />
						<asp:DropDownList ID="ddlAccounts4" runat="server" Visible="False" Width="30%">
						</asp:DropDownList>
						<asp:Button ID="btnAdd4" runat="server" Text="Add" Visible="False" Width="75px">
						</asp:Button>
						<asp:Button ID="btnRemove4" runat="server" Text="Remove" Visible="False" Width="75px">
						</asp:Button>
					</div>
					<div class="sectionOdd">
						<asp:Label ID="lblPercent4" runat="server" Visible="False">Percentage</asp:Label>
						<asp:TextBox ID="tbxPercent4" runat="server" Visible="False" Width="34px">00</asp:TextBox>
						<asp:Button ID="btnPercent4" runat="server" Text="Update Percent" Visible="False"
							Width="120px"></asp:Button>
					</div>
				</div>
			</li>
			<li>
				<div class="section">
					<div class="sectionEven">
						<asp:Label ID="lblAccount5" runat="server" Visible="False" Width="30%" />
						<asp:DropDownList ID="ddlAccounts5" runat="server" Visible="False" Width="30%">
						</asp:DropDownList>
						<asp:Button ID="btnAdd5" runat="server" Text="Add" Visible="False" Width="75px">
						</asp:Button>
						<asp:Button ID="btnRemove5" runat="server" Text="Remove" Visible="False" Width="75px">
						</asp:Button>
					</div>
					<div class="sectionOdd">
						<asp:Label ID="lblPercent5" runat="server" Visible="False">Percentage</asp:Label>
						<asp:TextBox ID="tbxPercent5" runat="server" Visible="False" Width="34px">00</asp:TextBox>
						<asp:Button ID="btnPercent5" runat="server" Text="Update Percent" Visible="False"
							Width="120px"></asp:Button>
					</div>
				</div>
			</li>
			<li>
				<div class="section">
					<div class="sectionEven">
						<asp:Label ID="lblAccount6" runat="server" Visible="False" Width="30%" />
						<asp:DropDownList ID="ddlAccounts6" runat="server" Visible="False" Width="30%">
						</asp:DropDownList>
						<asp:Button ID="btnAdd6" runat="server" Text="Add" Visible="False" Width="75px">
						</asp:Button>
						<asp:Button ID="btnRemove6" runat="server" Text="Remove" Visible="False" Width="75px">
						</asp:Button>
					</div>
					<div class="sectionOdd">
						<asp:Label ID="lblPercent6" runat="server" Visible="False">Percentage</asp:Label>
						<asp:TextBox ID="tbxPercent6" runat="server" Visible="False" Width="34px">00</asp:TextBox>
						<asp:Button ID="btnPercent6" runat="server" Text="Update Percent" Visible="False"
							Width="120px"></asp:Button>
					</div>
				</div>
			</li>
			<li>
				<div class="section">
					<div class="sectionEven">
						<asp:Label ID="lblAccount7" runat="server" Visible="False" Width="30%" />
						<asp:DropDownList ID="ddlAccounts7" runat="server" Visible="False" Width="30%">
						</asp:DropDownList>
						<asp:Button ID="btnAdd7" runat="server" Text="Add" Visible="False" Width="75px">
						</asp:Button>
						<asp:Button ID="btnRemove7" runat="server" Text="Remove" Visible="False" Width="75px">
						</asp:Button>
					</div>
					<div class="sectionOdd">
						<asp:Label ID="lblPercent7" runat="server" Visible="False">Percentage</asp:Label>
						<asp:TextBox ID="tbxPercent7" runat="server" Visible="False" Width="34px">00</asp:TextBox>
						<asp:Button ID="btnPercent7" runat="server" Text="Update Percent" Visible="False"
							Width="120px"></asp:Button>
					</div>
				</div>
			</li>
			<li>
				<div class="section">
					<div class="sectionEven">
						<asp:Label ID="lblAccount8" runat="server" Visible="False" Width="30%" />
						<asp:DropDownList ID="ddlAccounts8" runat="server" Visible="False" Width="30%">
						</asp:DropDownList>
						<asp:Button ID="btnAdd8" runat="server" Text="Add" Visible="False" Width="75px">
						</asp:Button>
						<asp:Button ID="btnRemove8" runat="server" Text="Remove" Visible="False" Width="75px">
						</asp:Button>
					</div>
					<div class="sectionOdd">
						<asp:Label ID="lblPercent8" runat="server" Visible="False">Percentage</asp:Label>
						<asp:TextBox ID="tbxPercent8" runat="server" Visible="False" Width="34px">00</asp:TextBox>
						<asp:Button ID="btnPercent8" runat="server" Text="Update Percent" Visible="False"
							Width="120px"></asp:Button>
					</div>
				</div>
			</li>
			<li>
				<div class="section">
					<div class="sectionEven">
						<asp:Label ID="lblAccount9" runat="server" Visible="False" Width="30%" />
						<asp:DropDownList ID="ddlAccounts9" runat="server" Visible="False" Width="30%">
						</asp:DropDownList>
						<asp:Button ID="btnAdd9" runat="server" Text="Add" Visible="False" Width="75px">
						</asp:Button>
						<asp:Button ID="btnRemove9" runat="server" Text="Remove" Visible="False" Width="75px">
						</asp:Button>
					</div>
					<div class="sectionOdd">
						<asp:Label ID="lblPercent9" runat="server" Visible="False">Percentage</asp:Label>
						<asp:TextBox ID="tbxPercent9" runat="server" Visible="False" Width="34px">00</asp:TextBox>
						<asp:Button ID="btnPercent9" runat="server" Text="Update Percent" Visible="False"
							Width="120px"></asp:Button>
					</div>
				</div>
			</li>
			<li>
				<div class="section">
					<div class="sectionEven">
						<asp:Label ID="lblAccount10" runat="server" Visible="False" Width="30%" />
						<asp:DropDownList ID="ddlAccounts10" runat="server" Visible="False" Width="30%">
						</asp:DropDownList>
						<asp:Button ID="btnAdd10" runat="server" Text="Add" Visible="False" Width="75px">
						</asp:Button>
						<asp:Button ID="btnRemove10" runat="server" Text="Remove" Visible="False" Width="75px">
						</asp:Button>
					</div>
					<div class="sectionOdd">
						<asp:Label ID="lblPercent10" runat="server" Visible="False">Percentage</asp:Label>
						<asp:TextBox ID="tbxPercent10" runat="server" Visible="False" Width="34px">00</asp:TextBox>
						<asp:Button ID="btnPercent10" runat="server" Text="Update Percent" Visible="False"
							Width="120px"></asp:Button>
					</div>
				</div>
			</li>
		</ul>
	</asp:Panel>
	</form>
</body>
</html>
