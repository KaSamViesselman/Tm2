<%@ Page Language="vb" MaintainScrollPositionOnPostback="true" AutoEventWireup="false"
	CodeBehind="TransportTrackingReport.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.TransportTrackingReport" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Transports : Transport Tracking Report</title>
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
		function pleaseWait() {
			document.getElementById('pnlMain').style.visibility = "hidden";
			document.getElementById('tblTransports').style.visibility = "hidden";
			document.getElementById('lblPleaseWait').innerHTML = "<br><br>&nbsp;&nbsp;&nbsp;Please wait while the selected data is being fetched...";
		}
	</script>
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
	<form id="main" runat="server" method="post">
	<asp:Label ID="lblPleaseWait" runat="server" Font-Size="Large" ForeColor="#003399"></asp:Label>
	<div class="recordSelection" id="pnlMain" runat="server">
		<div class="sectionEven">
			<ul>
				<li>
					<label>
						Order By</label>
					<span class="input">
						<asp:DropDownList ID="ddlOrderBy" runat="server" AutoPostBack="true" Width="60%">
						</asp:DropDownList>
						<asp:DropDownList ID="ddlAscDesc" runat="server" AutoPostBack="true" Width="30%">
						</asp:DropDownList>
					</span></li>
				<li>
					<label>
						Display Unit</label>
					<asp:DropDownList ID="ddlDisplayUnit" runat="server" AutoPostBack="true">
					</asp:DropDownList>
				</li>
			</ul>
		</div>
		<div class="sectionOdd" id="pnlSendEmail" runat="server">
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
	<div class="recordControl">
		<asp:Button ID="btnDisplayReport" runat="server" Text="Display report" />
		<asp:Button ID="btnPrinterFriendlyVersion" runat="server" Text="Printer Friendly Version" />
		<asp:Button ID="btnDownload" runat="server" Text="Download" />
	</div>
	<div class="section">
		<table id="tblTransports" border="1" cellspacing="0" runat="server" width="100%"
			enableviewstate="false">
		</table>
	</div>
	</form>
</body>
</html>
