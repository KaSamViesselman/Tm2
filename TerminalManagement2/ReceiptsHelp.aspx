<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="ReceiptsHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.ReceiptsHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Help : Receipts</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="ReceiptsHelp" runat="server">
		<div>
			<span style="font-size: large; font-weight: bold;"><a href="help.aspx#Receipts">Help</a>: Receipts</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>The receipts page provides access to tickets/receipts for past loads.</p>
				<table>
					<tr>
						<td style="width: 50%;">
							<table>
								<tr>
									<td><span class="helpItem">Facility:</span> select a facility to filter the available tickets to a single facility.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Ticket:</span> select to display the ticket. Tickets are listed by ticket number.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Sort:</span> select to sort ticket order in Tickets drop down list.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Show archived:</span> check to display previously archived tickets in tickets drop down list.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Find:</span> search the tickets for the keyword entered in the find text field.</td>
								</tr>

								<tr>
									<td><span class="helpItem">Archive:</span> click to archive the selected ticket. The archived ticket will still be available to view when "Show archived" checkbox is checked.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Void:</span> click to void the selected ticket. This will undo any inventory transactions that occurred when the ticket was created. The date of the inventory records that will undo the original records will be set to the local time of the Terminal Management 2 Web Server.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Printer friendly version:</span> click to open the selected ticket/receipt in a new window suitable for printing, without the Terminal Management header or side bar.</td>
								</tr>
								<tr>
									<td>
										<span class="helpItem">Show sources:</span> will display the receiving tickets and manufacturing tickets that were possible sources, or dispensing tickets that included bulk ingredients received with this ticket. 
										<p class="indentedNote">This option will only be displayed if Item Traceability is enabled.</p>
									</td>
								</tr>
								<tr>
									<td><span class="helpItem">E-mail to:</span> the ticket may be e-mailed directly from the page. Enter the e-mail addresses for the recipients (multiple addresses may be separated by commas) and click the "Send" button.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Add address:</span> will look in the e-mail history, and items to find possible e-mail addresses that the report may be sent to.</td>
								</tr>
							</table>
						</td>
						<td style="width: 50%;">&nbsp;
						</td>
					</tr>
				</table>
			</div>
		</div>
	</form>
</body>
</html>
