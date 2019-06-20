<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="AuditTableFieldsHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.AuditTableFieldsHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head id="Head1" runat="server"><title>Help : Audit Table Fields</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="AuditTableFieldsHelp" runat="server">
		<div><span style="font-size: large; font-weight: bold;"><a href="help.aspx#AuditTableFields">Help</a> : Audit Table Fields</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>The audit fields is used to set up database triggers for the purpose of data auditing (changes).</p>
				<table>
					<tr>
						<td style="width: 50%; border-right: 1px solid #646464;">
							<table>
								<tr>
									<td><span class="helpItem">table:</span> the table that should be audited.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Field:</span> is the name of the field in the database to be audited.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Audit on insert:</span> will create an audit entry whenever that field is inserted into the database.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Audit on update:</span> will create an audit entry whenever that field is updated in the database.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Audit on delete:</span> will create an audit entry whenever that field is deleted from the database.</td>
								</tr>
							</table>
						</td>
						<td style="width: 50%;"></td>
					</tr>
				</table>
				<p><span class="helpItem">Save button:</span> located at the top of the page, click this button to save any changes made to the audited fields. If no fields are selected for auditing, then the database trigger will be removed.</p>
				<p>A report for the audited data may be set up by following the directions located<a id="help" href="AuditTableChangesHelp.aspx" class="help">here</a>.</p>
			</div>
		</div>
	</form>
</body>
</html>
