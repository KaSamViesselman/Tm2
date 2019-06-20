<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="CustomFieldsHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.CustomFieldsHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head id="Head1" runat="server"><title>Help : Custom Fields</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="CustomFieldsHelp" runat="server">
		<div><%-- <span style="font-size: large; font-weight: bold;"><a href="help.aspx#CustomFields">Help</a> : Custom Fields</span>--%><span style="font-size: large; font-weight: bold;">Help : Custom Fields</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>Custom fields are used to define custom information the end user wants stored with a particular item.</p>
				<p>These fields may only be displayed on the page for the type they are assigned to. If a custom field is desired to be shown on a report, or used as a filter for a report, a custom report may be quoted that would add this functionality. These fields will also not be available in dispensing applications, and standard interfaces will not populate them by default.</p>
				<p><span class="helpItem">Custom field drop-down list:</span> located at the top of the page, this list contains all the custom field records. To create a new custom field select "Enter new custom field".</p>
				<table>
					<tr>
						<td style="width: 50%;">
							<table>
								<tr>
									<td><span class="helpItem">Name:</span> this is the name of the custom field. The name is required and may be up to 50 characters long.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Table:</span> this is the table that the custom field is associated with. The table is a required field.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Type:</span> this is the type of data that will be stored with the custom field. The type is only shown for a non-ticket custom field. The type is a required field.<ul>
										<li><i>Text</i> will prompt the user to enter a value that may be alpha-numeric.</li>
										<li><i>List (Single select)</i> will present the user with a list of options. The user will only be allowed to select an individual record.</li>
										<li><i>List (Multiple select)</i> will present the user with a list of options. The user will be allowed to select multiple records by holding down the control key on the keyboard while clicking, or by holding down the shift key while clicking to select a range of options.</li>
										<li><i>Drop down list</i> will present the user with a list of options.</li>
										<li><i>Date & Time</i> will allow the user to enter a date/time field. </li>
										<li><i>Checkbox</i> will prompt the user with a question that can be answered with either a yes or a no.</li>
										<li><i>Table lookup (Single select)</i> will look up the distinct values of a field from the table specified. If there is a deleted flag in the table, those records will not be included. The user will only be allowed to select an individual record.</li>
										<li><i>Table lookup (Multiple select)</i> will look up the distinct values of a field from the table specified. If there is a deleted flag in the table, those records will not be included. The user will be allowed to select multiple records by holding down the control key on the keyboard while clicking, or by holding down the shift key while clicking to select a range of options.</li>
									</ul>
									</td>
								</tr>
								<tr>
									<td><span class="helpItem">Ticket source:</span> this is the source of the custom data for a ticket. The custom data will be automatically copied from the source custom field to the ticket at the time the ticket was generated for historical purposes.</td>
								</tr>
							</table>
						</td>
						<td style="width: 50%">
							<table>
								<tr>
									<td><span class="helpItem">Options:</span> are used for lists and drop down lists to provide a list of available selections for the user to choose from.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Sort asc:</span> will sort the list alphabetically.</td>
								</tr>
							</table>
						</td>
					</tr>
				</table>
			</div>
			<p><span class="helpItem">Save button:</span> located at the top of the page, click this button to save any changes made to a custom field record or create a new record.</p>
			<p><span class="helpItem">Delete button:</span> located at the top of the page, click this button to delete the current custom field record.</p>
		</div>
	</form>
</body>
</html>
