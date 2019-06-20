<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="ReportDesigner.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.ReportDesigner" MaintainScrollPositionOnPostback="true" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title></title>
	<link rel="shortcut icon" href="FavIcon.ico" />
	<meta name="viewport" content="width=device-width" />
	<meta http-equiv="X-UA-Compatible" content="IE=edge" />
	<link rel="stylesheet" href="~/styles/site.css" />
	<link rel="stylesheet" href="~/styles/site-tablet.css" media="(max-width:768px)" />
	<link rel="stylesheet" href="~/styles/site-phone.css" media="(max-width:480px)" />
	<script type="text/javascript" src="~/scripts/jquery-1.11.1.min.js"></script>
	<script type="text/javascript" src="~/scripts/soap-1.0.1.js"></script>
	<script type="text/javascript" src="~/scripts/page-controller.js"></script>
	<script type="text/javascript">
		function postBackByObject() {
			var o = window.event.srcElement;
			if (o.tagName == "INPUT" && o.type == "checkbox") {
				//__doPostBack("", "");
				__doPostBack();
			}
		}
	</script>
</head>
<body>
	<form id="main" runat="server" style="width: 100%;">
		<div class="recordSelection ">
			<label>Report type</label>
			<asp:DropDownList ID="ddlReportBasis" runat="server" AutoPostBack="true"></asp:DropDownList>
		</div>
		<div id="pnlShowReport" runat="server" class="recordControl">
			<asp:Button ID="btnShowReport" runat="server" Text="Show report" />
		</div>
		<div id="pnlDisplayedFields" runat="server" class="sectionEven">
			<h2>Report fields</h2>
			<asp:TreeView ID="tvDisplayedFields" runat="server" CssClass="Treeview" onclick="javascript:postBackByObject()" Style="border: solid 1px black;" NodeIndent="0">
			</asp:TreeView>
		</div>
		<div id="pnlSelectedDetails" runat="server" class="sectionOdd">
			<h2>Selected report fields</h2>
			<div class="addRemoveSection">
				<asp:ListBox ID="lstSelectedDetails" runat="server" class="addRemoveList"></asp:ListBox>
				<asp:ImageButton ID="btnMoveFieldUp" runat="server" ImageUrl="~/images/arrow-up-2.png" AlternateText="Move Up" CssClass="arrowUpImageButton" />
				<br />
				<asp:ImageButton ID="btnMoveFieldDown" runat="server" ImageUrl="~/images/arrow-down-2.png" AlternateText="Move Down" CssClass="arrowDownImageButton" />
			</div>
		</div>
	</form>
</body>
</html>
