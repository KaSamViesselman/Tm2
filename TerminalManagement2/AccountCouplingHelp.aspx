<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="AccountCouplingHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.AccountCouplingHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server"><title>Help : Account Coupling</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="AccountCouplingHelp" runat="server">
		<div><span style="font-size: large; font-weight: bold;"><a href="help.aspx#AccountCoupling">Help</a> : Account Coupling</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>The account coupling may be used to setup common customer account splits or associations. Associations are used on the orders page where selecting a customer account that has been coupled will result in the associated customer accounts and corresponding percentages will automatically be selected.</p>
				<p><span class="helpItem">Account drop-down list:</span> select a customer account to create or modify associations with other customer accounts.</p>
				<p><span class="helpItem">Create association:</span> click this to create an association for an account that has not been coupled yet.</p>
				<table>
					<tr>
						<td><span class="helpItem">Account association table:</span> shows the accounts that have been coupled and the percentage of the billing split.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Add account drop-down list:</span> select an account to association list.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Add:</span> click to add the account selected in the add account drop-down list.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Remove:</span> click to remove the account listed in that row.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Percentage:</span> the percentage of the bill that this customer account is responsible for.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Acres:</span> if the destination is a field, its land area may be recorded in acres. Acres is optional.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Update percentage:</span> updates the account association table with the percentage entered in the percentage field to the left.</td>
					</tr>
				</table>
			</div>
		</div>
	</form>
</body>
</html>
