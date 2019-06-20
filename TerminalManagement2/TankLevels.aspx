<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="TankLevels.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.TankLevels" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Tanks : Tank Levels</title>
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
		<div class="sectionEven">
			<ul>
				<li>
					<label>
						Facility
					</label>
					<asp:DropDownList ID="ddlLocation" runat="server" />
				</li>
				<li>
					<label>
						Owner
					</label>
					<asp:DropDownList ID="ddlOwner" runat="server" />
				</li>
			</ul>
		</div>
		<div class="sectionOdd">
			<ul>
				<li>
					<label>
						Bulk product
					</label>
					<asp:DropDownList ID="ddlBulkProduct" runat="server" />
				</li>
				<li>
					<label>
						Display Unit
					</label>
					<asp:DropDownList ID="ddlDisplayUnit" runat="server" />
				</li>
			</ul>
		</div>
	</div>
	<div class="recordControl">
		<asp:Button ID="btnShowReport" runat="server" Text="Show report" />
	</div>
	<div class="section">
		<asp:Literal ID="litTankList" runat="server"></asp:Literal>
		<asp:Literal ID="litTankGroupList" runat="server"></asp:Literal>
	</div>
	</form>
</body>
</html>
