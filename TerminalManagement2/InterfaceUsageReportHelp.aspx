<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="InterfaceUsageReportHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.InterfaceUsageReportHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head id="Head1" runat="server"><title>Help : Interface Usage Report</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="InterfaceUsageReportHelp" runat="server">
		<div><span style="font-size: large; font-weight: bold;"><a href="help.aspx#InterfaceUsageReport">Help</a> : Interface Usage Report</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>The interface usage report shows all cross reference listings for each item.</p>
				<p><span class="helpItem">Report type:</span> select the type of item to run the report on.</p>
				<table>
					<tr>
						<td><span class="helpItem">Name:</span> the name of the item.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Interface Names:</span> the name of each interface. The interface type is included in parentheses.</td>
					</tr>
				</table>
				<p><span class="helpItem">E-mail to:</span> the interface usage report may be e-mailed directly from the page. Enter the e-mail addresses for the recipients (multiple addresses may be separated by commas) and click the "Send" button.</p>
				<p><span class="helpItem">Add address:</span> will look in the e-mail history, and items to find possible e-mail addresses that the report may be sent to.</p>
			</div>
		</div>
	</form>
</body>
</html>
