<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="ContainerSettings.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.ContainerSettings" %>

<!DOCTYPE html>
<html lang="en">
<head id="Head1" runat="server">
	<title>General Settings : Container Settings</title>
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
	<form id="main" method="post" runat="server" defaultfocus="tbxInspectionInterval">
		<div class="recordControl">
			<asp:Button ID="btnSave" runat="server" Text="Save" />
			<asp:Label ID="lblSave" runat="server" ForeColor="Red" Text="Container settings were saved successfully" Visible="False" />
		</div>
		<div class="sectionEven" id="pnlContainerSettings" runat="server">
			<ul>
				<li>
					<label>
						Default packaged inventory location
					</label>
					<asp:DropDownList ID="ddlDefaultPackagedInventoryLocation" runat="server"></asp:DropDownList>
				</li>
				<li>
					<label>
						Container inspection interval (Days)
					</label>
					<asp:TextBox ID="tbxContainerInspectionInterval" runat="server"></asp:TextBox>
				</li>
				<li>
					<label>
						Container inspection warning (Days)
					</label>
					<asp:TextBox ID="tbxContainerInspectionWarning" runat="server"></asp:TextBox>
				</li>
				<li>
					<label>
						Equipment inspection interval (Days)
					</label>
					<asp:TextBox ID="tbxEquipmentInspectionInterval" runat="server"></asp:TextBox>
				</li>
				<li>
					<label>
						Equipment inspection warning (Days)
					</label>
					<asp:TextBox ID="tbxEquipmentInspectionWarning" runat="server"></asp:TextBox>
				</li>
			</ul>
		</div>
	</form>
</body>
</html>
