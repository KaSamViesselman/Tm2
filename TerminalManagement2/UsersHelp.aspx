<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="UsersHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.UsersHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Help : Users</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="UsersHelp" runat="server">
		<div>
			<span style="font-size: large; font-weight: bold;"><a href="help.aspx#Users">Help</a>: Users</span>
			<hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>The users page is used to manage the users that have access to Terminal Management and what level of access they have.</p>
				<p><span class="helpItem">User drop-down list:</span> to modify an existing user, select the user from the drop-down list. To enter a new user, select "Enter new user" from the drop-down list.</p>
				<table>
					<tr>
						<td style="width: 50%; border-right: 1px solid #646464;">
							<table>
								<tr>
									<td><span class="helpItem">Name:</span> the full name of the user. The name is required and may be up to 50 characters in length.</td>
								</tr>
								<tr>
									<td><span class="helpItem">User profile:</span> the user profile that should be used for the user. The user profile is not required. If a user profile is selected, then the access rights for Terminal Management 2 will be determined by that profile.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Username:</span> used to login to Terminal Management and other applications that work with Terminal Management. The username is required and may be up to 50 characters in length.<p class="indentedNote">If windows authentication is the preferred method for authenticating users, then the username should be in the format of DOMAINNAME\Username if the PC is connected to a domain, or COMPUTERNAME\Username if the PC is in a workgroup.</p>
										<p class="indentedNote">This option can be enabled by setting the authentication mode to Windows; or disabled by setting the authentication mode to Forms in the <u>authentication.config</u>file located on the web server in the Terminal Management 2 installation directory.</p>
									</td>
								</tr>
								<tr>
									<td><span class="helpItem">Password:</span> used to login to Terminal Management and other applications that work with Terminal Management The password is required and may be up to 50 characters in length.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Owner:</span> the owner that the user is assigned to. If assigned to a specific owner the user will only be able to access that owner's information.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Signature:</span> the signature that will be applied to completed load tickets for the user.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Disabled:</span> used to temporarily disable access to the systems.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Application configuration:</span> used to determine if the user has access to modify the configuration in the individual dispensing applications.</td>
								</tr>
							</table>
						</td>
						<td style="width: 50%;">
							<p><span class="helpItem">Permissions:</span> the areas of Terminal Management 2 that the user has access to. This will only be displayed if the user has not been assigned to a user profile.</p>
						</td>
					</tr>
				</table>
			</div>
			<p><span class="helpItem">Save:</span> saves the changes made to an existing user record or create a new user when "enter new user" is selected in the user drop-down list.</p>
			<p><span class="helpItem">Delete:</span> deletes the selected user.</p>
		</div>
	</form>
</body>
</html>
