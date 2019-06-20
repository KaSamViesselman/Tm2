<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="CustomPagesHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.CustomPagesHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server"><title>Help : Custom Pages</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="CustomPagesHelp" runat="server">
		<div><span style="font-size: large; font-weight: bold;"><a href="help.aspx#CustomPages">Help</a> : Custom Pages</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>Custom pages may be setup to provide access to add-in pages, analysis pages and external web pages from within Terminal Management.</p>
				<p><span class="helpItem">Custom page drop-down list:</span> to modify an existing custom page, select the page from the drop-down list. To create a new custom page, select "Enter a new custom page" from the drop-down list.</p>
				<table>
					<tr>
						<td><span class="helpItem">Page label:</span> the label is used on the Terminal Management side bar if custom page is a main menu link, the reports tab bar if the custom page is a report, or in the analysis drop-down list if the custom page is an analysis. The page label is required and may include up to 50 characters.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Page URL:</span> the web address for the custom page (e.g. http://localhost/TerminalManagement2/Welcome.aspx).</td>
					</tr>
					<tr>
						<td><span class="helpItem">Page type:</span> the custom page may be a main menu link, which appears in the Terminal Management side bar; or a report, which appears in the tab bar of the reports section; or an analysis, which is available in bulk product and tank analysis drop-down lists.</td>
					</tr>
				</table>
				<p><span class="helpItem">Save:</span> saves changes to an existing custom page if one is selected in the custom page drop-down list. Creates a new custom page record if "Enter a new custom page" is selected in the custom page drop-down list.</p>
				<p><span class="helpItem">Delete:</span> deletes the selected custom page record.</p>
			</div>
		</div>
	</form>
</body>
</html>
