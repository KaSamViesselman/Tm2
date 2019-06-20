<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="EmailReportsHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.EmailReportsHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Help : E-mail Reports</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="EmailReports" runat="server">
		<div>
			<span style="font-size: large; font-weight: bold;"><a href="help.aspx#EmailReports">Help</a> : E-mail Reports</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>The e-mail reports page is used to set up automatic report e-mails. Once the e-mail report record is set up the report must be scheduled on the Terminal Management server, which is typically done by a Kahler Automation technician. Reports may be available in either HTML (printable) or CSV (opens in spreadsheet application) formats.</p>
				<p><span class="helpItem">E-mail report drop-down list:</span> to modify an existing e-mail report, select the report in the drop-down list. To create a new e-mail report, select "Enter new e-mail report" in the drop-down list.</p>
				<table>
					<tr>
						<td style="width: 50%; border-right: 1px solid #404040">
							<h2>E-mail Report</h2>
							<table>
								<tr>
									<td><span class="helpItem">Name:</span> the name of the report is required and may be up to 50 characters in length.</td>
								</tr>
								<tr>
									<td><span class="helpItem">ID:</span> unique identifier that represents the e-mail report. This is used by a Kahler Automation technician to schedule the report of the Terminal Management server.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Recipients:</span> the e-mail addresses that the report should be sent to. Multiple e-mail addresses may be separated by commas.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Report domain URL:</span> the links within the report will be created using this Domain. If nothing is specified here, links may not work.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Subject:</span> the subject that should be used on the e-mail when sending the report. The subject may reference the date that the report was sent by including the text <i>{now}</i> and may reference the date and time that the report was last sent by including the text <i>{last_sent}</i>.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Report type:</span> the type of report that should be sent. Options will vary based on the report type selection.<ul>
										<li><span style="font-style: italic">Bulk product usage</span>: will list all of the bulk products used.</li>
										<li><span style="font-style: italic">Carrier list</span>: a list of all the carriers.</li>
										<li><span style="font-style: italic">Container list</span>: a list of all the containers.</li>
										<li><span style="font-style: italic">Container history</span>: the history of all or one container.</li>
										<li><span style="font-style: italic">Customer activity report</span>: see customer activity report for more information on the report. </li>
										<li><span style="font-style: italic">Driver in facility history</span>: will list all of the drivers that were in the facility in between the days of the report.</li>
										<li><span style="font-style: italic">Driver list</span>: a list of all the drivers.</li>
										<li><span style="font-style: italic">Inventory</span>: the current inventory levels for all or one bulk product at all or one facility.</li>
										<li><span style="font-style: italic">Inventory change report</span>: lists the inventory changes that have occurred since the last time the report was run. </li>
										<li><span style="font-style: italic">Order list</span>: a list of all the orders.</li>
										<li><span style="font-style: italic">Product allocation</span>: shows how much of all or one product has been requested at a facility in a selectable unit of measure.</li>
										<li><span style="font-style: italic">Product list</span>: a list of all the products and the bulk products used to make those products.</li>
										<li><span style="font-style: italic">Receiving activity report</span>: see the receiving activity report for more information on the report. </li>
										<li><span style="font-style: italic">Receiving purchase order list report</span>: a list of all the current receiving purchase orders.</li>
										<li><span style="font-style: italic">Tank alarm history</span>: a list of tank alarms for all or one tanks at all or one facility.</li>
										<li><span style="font-style: italic">Tank levels</span>: the current tank levels as read by a tank level monitor (TLM) panel for all or one bulk product at all or one facility.</li>
										<li><span style="font-style: italic">Tank level trend</span>: the tank level trend data for all or one tank level trend.</li>
										<li><span style="font-style: italic">Track report</span>: will list all of the rail cars that are on the tracks.</li>
										<li><span style="font-style: italic">Transports in facility history</span>: will list all of the transports that were in the facility in between the days of the report.</li>
										<li><span style="font-style: italic">Transport list</span>: a list of all the transports.</li>
										<li><span style="font-style: italic">Transport usage report</span>: a list of tickets that all or one transport have been used for, for all or one customer account.</li>
										<li><span style="font-style: italic">Transport tracking report</span>: a list of transports with their last order(s) that were applied to that transport.</li>
									</ul>
									</td>
								</tr>
								<tr>
									<td><span class="helpItem">Owner:</span> the owner for the report. If the user logged in has been assigned to a specific owner, only the reports for that owner will be available to the user.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Report is month to date:</span> specifies if the report should include data from the beginning of the month to the run date.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Disabled:</span> specifies if the report to be emailed out should be sent. If the report is disabled, then the TM2 E-mail program will write an entry to the E-mail log file that it was disabled, but will not send it.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Last sent:</span> is the date that the report was last sent. This value is used on reports that include data from the last time they were sent.</td>
								</tr>
							</table>
						</td>
						<td style="width: 50%;">
							<h2>Report scheduled run times</h2>
							<table>
								<tr>
									<td><span class="helpItem">Run times list:</span> will display the current list of times that the report should be automatically run. If there are no entries, then the only way the report will run is to assign a scheduled task to run the report.<p><span class="helpItem">Add:</span> will add a new, blank trigger to the E-mail Report.</p>
									</td>
								</tr>
								<tr>
									<td><span class="helpItem">Send at Specific Time:</span> will set the report to run at a specific time of the day assigned in the <b>Send at</b> section on the days of the week defined in the <b>Days of week</b> section.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Send On Scheduled Period:</span> will set the report to run every X minutes, which are defined in the <b>Send every minutes</b> section on the days of the week defined in the <b>Days of week</b> section.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Send On Specific Date of Month:</span> will set the report to run at a specific time assigned in the <b>Send at</b> section on the date assigned in the <b>Send on the date of the month</b> section.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Send On Specific Day of Month:</span> will set the report to run at a specific time assigned in the <b>Send at</b> section on the date assigned in the <b>Send on the day of the month</b> and the <b>Days of week</b> section. The options listed are which instance of the date should the report be run on. For example, setting this to the 2nd Monday will generate the report on the 2nd Monday of the month (which occurs with a date between 8 and 14).</td>
								</tr>
								<tr>
									<td><span class="helpItem">Days of week:</span> define which days of the week the trigger is valid for.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Run once to catch up:</span> will set last sent time to the report run time when it is run. If this is not selected, and there are multiple trigger times between the time the report was run and the last time it was run, then the E-mail Service will send out a report for each missing report, if the report is date and time dependent.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Disabled:</span> specifies if the trigger should be used to determine if the report should be emailed.</td>
								</tr>
							</table>
							<p><span class="helpItem">Update:</span> will update the selected report trigger with the applied settings.</p>
							<p><span class="helpItem">Remove:</span> removes the currently selected report trigger from the E-mail Report.</p>
						</td>
					</tr>
				</table>
			</div>
			<p><span class="helpItem">Save:</span> if an existing e-mail report is selected, clicking updates the record. If "Enter new e-mail report" is selected, clicking creates a new record.</p>
			<p><span class="helpItem">Delete:</span> deletes the selected e-mail report record.</p>
		</div>
	</form>
</body>
</html>
