<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="LotsHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.LotsHelp" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>Help : Lots</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="LotsHelp" runat="server">
		<div>
			<span style="font-size: large; font-weight: bold;"><a href="help.aspx#Lots">Help</a> : Lots</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>Lot records represent a means of tracking bulk product usage in a facility.</p>
				<p><span class="helpItem">Show lots for bulk product:</span> contains all the bulk products set up in the system. If a particular bulk product is selected, then only the lots that have that bulk product assigned to it will be displayed.</p>
				<p><span class="helpItem">Lot drop-down list:</span> to modify an existing lot, select it from the drop-down list; otherwise to create a new lot select "Enter new lot" from the drop-down list.</p>
				<table>
					<tr>
						<td style="width: 50%; border-right: 1px solid #646464;">
							<span class="helpItem">Tickets</span>
							<table>
								<tr>
									<td><span class="helpItem">Lot number:</span> the lot number is required and may be up to 50 characters in length.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Bulk product:</span> is the bulk product associated with the specified lot number. The bulk product is required.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Dispensing usage tracking completed:</span> determines if this lot should be included during the assigning of lots after dispensing a load. If the lot's usage tracking is marked as completed, it will not be included in the list of available lots to assign to the ticket's bulk item record.</td>
								</tr>
							</table>
							<p><span class="helpItem">Save:</span> saves changes to an existing lot if selected in the lot drop-down list; otherwise creates a new lot if "Enter new lot" is selected in the lot drop-down list.</p>
							<p><span class="helpItem">Delete:</span> deletes the selected lot.</p>
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
