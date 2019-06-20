<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="ContainerEquipmentTypes.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.ContainerEquipmentTypes" EnableSessionState="True" %>

<!DOCTYPE html>
<html lang="en">
<head>
	<title>Containers : Container Equipment Types</title>
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
			Container equipment type</label>
		<asp:DropDownList ID="ddlContainerEquipmentTypes" runat="server" AutoPostBack="True" />
	</div>
	<div class="recordControl">
		<asp:Button ID="btnSave" runat="server" Text="Save" />
		<asp:Button ID="btnDelete" runat="server" Text="Delete" Enabled="false" />
		<asp:Label ID="lblStatus" runat="server" ForeColor="Red" />
		<span class="sectionRequiredField"><span class="required"></span>&nbsp;indicates required field </span>
	</div>
	<div class="sectionEven">
		<h1>
			General</h1>
		<ul>
			<li>
				<label>
					Name
				</label>
				<span class="required">
					<asp:TextBox ID="tbxName" runat="server" />
				</span></li>
			<li>
				<label>
					Use general container equipment settings for report inspection intervals
				</label>
				<asp:CheckBox ID="cbxUseGeneralSettingsForInterval" runat="server" Text="" />
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
