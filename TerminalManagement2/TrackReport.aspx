<%@ Page AutoEventWireup="false" CodeBehind="TrackReport.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.TrackReport"
	Language="vb" MaintainScrollPositionOnPostback="true" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Reports : Track Report</title>
	<meta name="viewport" content="width=device-width" />
	<meta http-equiv="X-UA-Compatible" content="IE=edge" />
	<link rel="stylesheet" href="styles/site.css" />
	<link rel="stylesheet" href="styles/site-tablet.css" media="(max-width:768px)" />
	<link rel="stylesheet" href="styles/site-phone.css" media="(max-width:480px)" />
	<script type="text/javascript" src="scripts/jquery-1.11.1.min.js"></script>
	<script type="text/javascript" src="scripts/soap-1.0.1.js"></script>
	<script type="text/javascript" src="scripts/page-controller.js"></script>
	<script type="text/javascript" src="jquery-ui/jquery-ui.min.js"></script>
	<script type="text/javascript" src="Scripts/TimePicker/jquery-ui-timepicker-addon.min.js"></script>
	<link rel="stylesheet" href="jquery-ui/jquery-ui.min.css" />
	<link rel="stylesheet" href="jquery-ui/jquery-ui.structure.min.css" />
	<link rel="stylesheet" href="Styles/TimePicker/jquery-ui-timepicker-addon.min.css" />
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
		<asp:Button ID="btnShowReport" runat="server" Text="Show Report" Width="125px" />
		<asp:Button ID="btnDownload" runat="server" Text="Download" Width="125px" />
	</div>
	<div class="sectionEven">
		<ul>
			<li>
				<label>
					Track</label>
				<asp:DropDownList ID="ddlTracks" runat="server">
				</asp:DropDownList>
			</li>
			<li>
				<label>
					Start date</label>
				<input type="text" name="tbxFromDate" id="tbxFromDate" value="" runat="server" />
				<script type="text/javascript">
					$('#tbxFromDate').datepicker({
						showTime: false,
						timeFormat: 'h:mm:ss TT',
						showSecond: true,
						showOn: "button",
						buttonImage: 'Images/Calendar_scheduleHS.png',
						buttonImageOnly: true,
						buttonText: "Show calendar"
					});
				</script>
			</li>
			<li>
				<label>
					End date</label>
				<input type="text" name="tbxToDate" id="tbxToDate" value="" runat="server" />
				<script type="text/javascript">
					$('#tbxToDate').datepicker({
						showTime: false,
						timeFormat: 'h:mm:ss TT',
						showSecond: true,
						showOn: "button",
						buttonImage: 'Images/Calendar_scheduleHS.png',
						buttonImageOnly: true,
						buttonText: "Show calendar"
					});
				</script>
			</li>
			<li>
				<label>
				</label>
				<asp:CheckBox ID="cbxShowOperator" runat="server" Checked="true" Text="Show operator name" />
			</li>
			<li>
				<label>
				</label>
				<asp:CheckBox ID="cbxShowRfid" runat="server" AutoPostBack="False" Checked="True"
					Text="Show RFID number(s)" />
			</li>
			<li>
				<label>
				</label>
				<asp:CheckBox ID="cbxShowCarNumber" runat="server" AutoPostBack="False" Checked="True"
					Text="Show rail car number" />
			</li>
			<li>
				<label>
				</label>
				<asp:CheckBox ID="cbxShowTrack" runat="server" AutoPostBack="False" Checked="True"
					Text="Show track name" />
			</li>
			<li>
				<label>
				</label>
				<asp:CheckBox ID="cbxShowScannedTime" runat="server" AutoPostBack="False" Checked="True"
					Text="Show scanned date/time" />
			</li>
			<li>
				<label>
				</label>
				<asp:CheckBox ID="cbxShowReverseOrder" runat="server" AutoPostBack="False" Text="Show in reverse order" />
			</li>
		</ul>
	</div>
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
	</form>
</body>
</html>
