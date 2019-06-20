<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="UserProfilesHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.UserProfilesHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head id="Head1" runat="server">
	<title>Help : User Profiles</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="UsersHelp" runat="server">
		<div>
			<span style="font-size: large; font-weight: bold;"><a href="help.aspx#UserProfiles">Help</a> : User Profiles</span>
			<hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>The user profiles page is used to manage the user profiles in Terminal Management and what level of access they have.</p>
				<p><span class="helpItem">User Profile drop-down list:</span> to modify an existing user profile, select the user profile from the drop-down list. To enter a new user profile, select "Enter a new user profile" from the drop-down list.</p>
				<table>
					<tr>
						<td style="width: 50%; border-right: 1px solid #646464;">
							<table>
								<tr>
									<td><span class="helpItem">Profile Name:</span> the name of the user profile. The name is required and may be up to 50 characters in length.</td>
								</tr>
							</table>
						</td>
						<td style="width: 50%;">
							<p><span class="helpItem">Permissions:</span> the areas of Terminal Management 2 that the user profile has access to. To enable the selection of the checkboxes, the modify radio button for that permission must be enabled. Once modify is selected, each checkbox in the row to the right is also selected, permissions can be revoked by de-selecting the checkbox.</p>
							<p>The following access options may be combined to create specific permissions:</p>
							<ul class="indentedNote">
								<li>View: user may view the record. </li>
								<li>Modify: user has the ability to create, edit, or delete records.</li>
								<li>Create: user may create new records.</li>
								<li>Edit: user may edit existing records. </li>
								<li>Delete: user may delete records.</li>
							</ul>
						</td>
					</tr>
				</table>
			</div>
			<p><span class="helpItem">Save:</span> saves the changes made to an existing user profile or create a new user profile when "enter new user profile" is selected in the user profile drop-down list.</p>
			<p><span class="helpItem">Delete:</span> deletes the selected user profile.</p>
		</div>
	</form>
</body>
</html>
