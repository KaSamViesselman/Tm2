<%@ Page Language="vb" CodeBehind="EventLog.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.EventLog"
	MaintainScrollPositionOnPostback="true" %>

<!DOCTYPE html>
<html lang="en">
<head>
	<title>Reports : Event Log</title>
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
	<form id="main" runat="server">
	<div class="recordControl">
		<asp:Button ID="btnShowLogEntries" runat="server" Text="Show log entries" />
	</div>
	<div class="sectionEven">
		<h1>
			Range</h1>
		<ul>
			<li>
				<label>
					From</label>
				<input type="text" name="tbxFrom" id="tbxFrom" value="" runat="server" />
				<script type="text/javascript">
					$('#tbxFrom').datetimepicker({
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
					To</label>
				<input type="text" name="tbxTo" id="tbxTo" value="" runat="server" />
				<script type="text/javascript">
					$('#tbxTo').datetimepicker({
						timeFormat: 'h:mm:ss TT',
						showSecond: true,
						showOn: "button",
						buttonImage: 'Images/Calendar_scheduleHS.png',
						buttonImageOnly: true,
						buttonText: "Show calendar"
					});
				</script>
			</li>
		</ul>
	</div>
	<div class="sectionOdd">
		<h1>
			Filters</h1>
		<ul>
			<li>
				<label>
					Computer</label>
				<asp:DropDownList ID="ddlComputer" runat="server" />
			</li>
			<li>
				<label>
					Application</label>
				<asp:DropDownList ID="ddlApplication" runat="server" />
			</li>
			<li>
				<label>
					Category</label>
				<asp:DropDownList ID="ddlCategory" runat="server" />
			</li>
		</ul>
	</div>
	<div class="section">
		<h1>
			Log entries</h1>
		<asp:Table ID="logEntries" runat="server">
			<asp:TableHeaderRow ID="logEntriesHeader" runat="server">
				<asp:TableHeaderCell ID="logEntriesDateTime" runat="server">Date/time</asp:TableHeaderCell>
				<asp:TableHeaderCell ID="logEntriesComputer" runat="server">Computer</asp:TableHeaderCell>
				<asp:TableHeaderCell ID="logEntriesApplication" runat="server">Application</asp:TableHeaderCell>
				<asp:TableHeaderCell ID="logEntriesVersion" runat="server">Version</asp:TableHeaderCell>
				<asp:TableHeaderCell ID="logEntriesCategory" runat="server">Category</asp:TableHeaderCell>
				<asp:TableHeaderCell ID="logEntriesDescription" runat="server">Description</asp:TableHeaderCell>
			</asp:TableHeaderRow>
		</asp:Table>
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
