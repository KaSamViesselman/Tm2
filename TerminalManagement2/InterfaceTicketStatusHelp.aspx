<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="InterfaceTicketStatusHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.InterfaceTicketStatusHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head id="Head1" runat="server"><title>Help : Interface Ticket Export Status</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="InterfaceTicketStatusHelp" runat="server">
		<div><span style="font-size: large; font-weight: bold;"><a href="help.aspx#Interfaces">Help</a>: Interface Ticket Export Status</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>Interfaces are used to communicate with 3rd party software packages, usually to sync order, account, and ticket information.</p>
				<p><span class="helpItem">Interface drop-down list:</span> located at the top of the page, this list contains all the interfaces.</p>
				<table>
					<tr>
						<td><span class="helpItem">Show tickets exported:</span> will show a list of tickets that have been marked as exported to the interface.</td>
					</tr>
					<tr>
						<td><span class="helpItem" style="margin-left: 30px;">Include tickets that were marked manually:</span> will include tickets that were marked as exported manually.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Show tickets not exported:</span> will show a list of tickets that have not been marked as exported to the interface.</td>
					</tr>
					<tr>
						<td><span class="helpItem" style="margin-left: 30px;">Show tickets without errors:</span> will show tickets that have not been exported, and that have not been marked as having an error in the database. Some interfaces use this method to try and export a ticket, and if there is an issue, it will "Black List" the ticket by marking it with an error.</td>
					</tr>
					<tr>
						<td><span class="helpItem" style="margin-left: 60px;">Only include orders for this interface:</span> will only show tickets that were completed when the order was marked as being created by the interface. If unchecked, it will include all tickets for any order, regardless of the source of the order.</td>
					</tr>
					<tr>
						<td><span class="helpItem" style="margin-left: 30px;">Show tickets with errors:</span> will show tickets that have not been exported, but have been marked as having an error in the database. Some interfaces use this method to try and export a ticket, and if there is an issue, it will "Black List" the ticket by marking it with an error.<br />
							<span style="font-style: italic; margin-left: 60px;">For best practices, the list of tickets with errors should be cleared, either by resetting the error status, or by ignoring the error if the ticket has been handled manually in the agronomy package (such as manually invoiced).</span></td>
					</tr>
					<tr>
						<td><span class="helpItem" style="margin-left: 30px;">Show tickets with ignored errors:</span> will show tickets that have not been exported, but have been marked as having an error in the database, and have had the error marked as ignored.</td>
					</tr>
				</table>
				<hr />
				<table>
					<tr>
						<td><span class="helpItem">Clear Exported Status:</span> will clear the exported status of the ticket, allowing the interface to try and re-export the ticket information.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Set Exported Status:</span> will mark the ticket as manually exported.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Clear Error Status:</span> will clear the export error status of the ticket, allowing the interface to try and re-export the ticket information.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Ignore Error Status:</span> will set the status of the error to ignore. The primary function of this is to remove it from the list of tickets with errors.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Clear Ignore Status:</span> will clear the export error status of the ticket, allowing the interface to try and re-export the ticket information.</td>
					</tr>
				</table>
			</div>
		</div>
	</form>
</body>
</html>
