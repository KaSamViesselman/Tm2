<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="ArchiveTicketsHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.ArchiveTicketsHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Help : Archive Tickets</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="ArchiveTicketsHelp" runat="server">
		<div>
			<span style="font-size: large; font-weight: bold;"><a href="help.aspx#ArchiveTickets">Help</a>: Archive Tickets</span>
			<hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>The archive tickets page may be used to archive/un-archive as well as void multiple tickets at the same time.</p>
				<p><span class="helpItem">Loaded at from date:</span> a date that corresponds to the oldest loaded at date in the tickets table to show.</p>
				<p><span class="helpItem">Loaded at to date:</span> a date that corresponds to the newest loaded at date in the tickets table to show.</p>
				<p><span class="helpItem">Owner drop-down list:</span> contains all the owners set up in the system. If a particular owner is selected, then only the tickets that are associated with that owner will be displayed.</p>
				<p><span class="helpItem">Order number contains:</span> will limit the list to only the orders that contain the specified text.</p>
				<p><span class="helpItem">Ticket number contains:</span> will limit the list to only the tickets that contain the specified text.</p>
				<p><span class="helpItem">Show archived:</span> used to filter the tickets table to show both archived and unarchived tickets.</p>
				<p><span class="helpItem">Show voided:</span> used to filter the tickets table to include voided tickets.</p>
				<table>
					<tr>
						<td><span class="helpItem">Filter:</span> used to filter the tickets table on the page. If there is an error in one of your filter selections, a dialog box will appear stating the error(s) and the tickets table will be emptied.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Archive:</span> used to update the archived status of the checked tickets, toggling between archived and unarchived.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Void:</span> used to update the voided status of the checked tickets to true, once voided, a ticket can no longer be archived.</td>
					</tr>
				</table>
				<p><span class="helpItem">Tickets table:</span> displays the tickets from the database, using filters as specified.</p>
				<table>
					<tr>
						<td><span class="helpItem">Ticket number:</span> displays the ticket number field from the ticket record.</td>
					</tr>
					<tr>S
						<td><span class="helpItem">Order number:</span> displays the order the ticket is a part of.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Customer:</span> displays the customer account that the ticket is a part of.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Owner:</span> displays the owner the ticket belongs to.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Loaded date:</span> displays the loaded at date of the ticket record.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Checkbox:</span> the tickets that should be archived or voided. Checking the first checkbox in the table will mark all of the checkboxes as checked.</td>
					</tr>
				</table>
			</div>
		</div>
	</form>
</body>
</html>
