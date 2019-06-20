<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="AuditTableChangesHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.AuditTableChangesHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head id="Head1" runat="server">
	<title>Help : Audit Table Changes</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="AuditTableChangesHelp" runat="server">
		<div>
			<span style="font-size: large; font-weight: bold;"><a href="help.aspx#AuditTableChanges">Help</a> : Audit Table Changes</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>The audit changes report may be used to show changes made to fields in the database that are set up for data auditing (changes).</p>
				<p>
					The report may be set up as a custom report by using the following settings:<br />
					The <span class="helpItem">E-mail web service URL</span> should be set to <span style="color: Blue; text-decoration: underline;">AuditTableChangesReport.asmx</span><br />
					The <span class="helpItem">E-mail web service name (class name)</span> should be set to <span style="color: Blue;">AuditTableChangesReport</span><br />
					The <span class="helpItem">E-mail web service method name</span> should be set to <span style="color: Blue;">GetAuditedDataReport</span>
				</p>
				<table>
					<tr>
						<td style="width: 50%; border-right: 1px solid #646464;">
							<table>
								<tr>
									<td><span class="helpItem">From:</span> the oldest records to display in the report.</td>
								</tr>
								<tr>
									<td><span class="helpItem">To:</span> the latest records to include in the report.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Table name:</span> select a table to only include records that reference the selected table in the report.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Type:</span> select a type of audit to only include records that were created during the specified database function in the report.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Application:</span> select a PC/Application to only include records that reference the selected entity in the report.</td>
								</tr>
								<tr>
									<td><span class="helpItem">User:</span> select a user to only include records that reference the selected user in the report.</td>
								</tr>
							</table>
						</td>
						<td style="width: 50%;"></td>
					</tr>
				</table>
				<p><span class="helpItem">E-mail to:</span> the audit changes report may be e-mailed directly from the page. Enter the e-mail addresses for the recipients (multiple addresses may be separated by commas) and click the "Send" button.</p>
				<p><span class="helpItem">Add address:</span> will look in the e-mail history, and items to find possible e-mail addresses that the report may be sent to.</p>
				<p><span class="helpItem">Show report:</span> click to show the report with the selected filter, column and sort selections.</p>
				<p><span class="helpItem">Download report:</span> click to download a comma separated value (CSV) file containing the report data in a file format that may be opened by a spreadsheet application.</p>
			</div>
		</div>
	</form>
</body>
</html>
