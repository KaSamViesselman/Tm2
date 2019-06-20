<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="Tracks.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.Tracks" %>

<!DOCTYPE html>
<html lang="en">
<head id="Head1" runat="server">
	<title>Facilities : Tracks</title>
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
	<form id="main" method="post" runat="server" defaultfocus="tbxname">
	<div class="recordSelection">
		<label>
			Track
		</label>
		<asp:DropDownList ID="ddlTracks" runat="server" AutoPostBack="True">
		</asp:DropDownList>
	</div>
	<div class="recordControl">
		<asp:Button ID="btnSave" runat="server" Text="Save" Width="120px" />
		<asp:Button ID="btnDelete" runat="server" Text="Delete" Width="120px" />
		<asp:Label ID="lblStatus" runat="server" ForeColor="#ff0000"></asp:Label>
		<span class="sectionRequiredField"><span class="required"></span>&nbsp;indicates required
			field </span>
	</div>
	<asp:Panel ID="pnlEven" runat="server" CssClass="sectionEven">
		<ul>
			<li>
				<label>
					Name
				</label>
				<span class="required">
					<asp:TextBox ID="tbxName" TabIndex="1" runat="server"></asp:TextBox>
				</span></li>
			<li>
				<label>
					Facility
				</label>
				<span class="required">
					<asp:DropDownList ID="ddlFacility" runat="server">
					</asp:DropDownList>
				</span></li>
			<li>
				<label>
					Length
				</label>
				<span class="input">
					<asp:TextBox ID="tbxLength" TabIndex="5" runat="server" Width="60%" />
					<asp:DropDownList ID="ddlLengthUnit" runat="server" Width="30%" />
				</span></li>
			<li>
				<label>
					Notes
				</label>
				<asp:TextBox ID="tbxNotes" TabIndex="6" runat="server" TextMode="MultiLine"></asp:TextBox>
			</li>
		</ul>
	</asp:Panel>
	</form>
</body>
</html>
