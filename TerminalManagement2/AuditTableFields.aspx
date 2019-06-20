<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="AuditTableFields.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.AuditTableFields" EnableViewState="true" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>Audit Table Fields : Audit Table Fields</title>
	<meta name="viewport" content="width=device-width" />
	<meta http-equiv="X-UA-Compatible" content="IE=edge" />
	<link rel="stylesheet" href="styles/site.css" />
	<link rel="stylesheet" href="styles/site-tablet.css" media="(max-width:768px)" />
	<link rel="stylesheet" href="styles/site-phone.css" media="(max-width:480px)" />
	<script type="text/javascript" src="scripts/jquery-1.11.1.min.js"></script>
	<script type="text/javascript" src="scripts/soap-1.0.1.js"></script>
	<script type="text/javascript" src="jquery-ui/jquery-ui.min.js"></script>
	<script type="text/javascript" src="Scripts/TimePicker/jquery-ui-timepicker-addon.min.js"></script>
	<script type="text/javascript" src="scripts/page-controller.js"></script>
	<link rel="stylesheet" href="jquery-ui/jquery-ui.min.css" />
	<link rel="stylesheet" href="jquery-ui/jquery-ui.structure.min.css" />
	<link rel="stylesheet" href="Styles/TimePicker/jquery-ui-timepicker-addon.min.css" />
</head>
<body>
	<form id="main" method="post" runat="server" defaultfocus="ddlTable">
	<div class="recordSelection">
		<label>
			Table</label>
		<asp:DropDownList ID="ddlTable" runat="server" AutoPostBack="True" />
	</div>
	<div class="recordControl">
		<asp:Button ID="btnSave" runat="server" Text="Save" />
		<asp:Label ID="lblStatus" runat="server" ForeColor="Red" />
	</div>
	<div class="sectionEven">
		<table id="tblTableFields" runat="server" style="width: 100%;">
			<tr>
				<th>
					Field
				</th>
				<th>
					Audit on insert
				</th>
				<th>
					Audit on update
				</th>
				<th>
					Audit on delete
				</th>
			</tr>
		</table>
	</div>
	</form>
</body>
</html>
