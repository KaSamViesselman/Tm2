Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class Users
    Inherits System.Web.UI.Page

    Private _currentUser As KaUser
    Public Const IMAGE_PREFIX_STRING As String = "<img style=""border-style: solid; border-width: 1; width: 60%; max-width: 244px;"" src=""data:image/jpg;base64,"
    Public Const IMAGE_SUFFIX_STRING As String = """ />"
    Public Const NO_IMAGE_SPECIFIED_STRING As String = "No Signature specified"
    Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
#Region "Events"

    Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load, Me.Load
        Response.Cache.SetCacheability(HttpCacheability.NoCache)
        _currentUser = Utilities.GetUser(Me)
        _currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaUser.TABLE_NAME}), "Users")
        If Not _currentUserPermission(KaUser.TABLE_NAME).Read Then Response.Redirect("Welcome.aspx")
        If Page.IsPostBack Then
            tbxPassword.Attributes.Add("value", tbxPassword.Text)
        End If
        If Not Page.IsPostBack Then
            SetTextboxMaxLengths()
            PopulateUsersList(_currentUser)
            PopulateOwnerList(_currentUser)
            PopulateUserProfileList(_currentUser)
            If Page.Request("UserId") IsNot Nothing Then
                Try
                    ddlUsers.SelectedValue = Page.Request("UserId")
                Catch ex As ArgumentOutOfRangeException
                    ddlUsers.SelectedIndex = 0
                End Try
            End If
            ddlUsers_SelectedIndexChanged(ddlUsers, New EventArgs)
            pnlPassword.Visible = Not Utilities.UseWindowsAuthentication()
            Utilities.ConfirmBox(Me.btnDelete, "Are you sure you want to delete this user?")
            Utilities.SetFocus(tbxName, Me)
        End If
    End Sub

    Private Sub ddlUsers_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ddlUsers.SelectedIndexChanged
        lblStatus.Text = ""
        Dim userId As Guid = Guid.Parse(ddlUsers.SelectedValue)
        PopulateUserData(userId)
        UploadStatusLabel.Text = ""
        SetControlUsabilityFromPermissions()
    End Sub

    Private Sub btnSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSave.Click
        SaveUser()
    End Sub

    Private Sub btnDelete_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDelete.Click
        lblStatus.Text = ""
        If Guid.Parse(ddlUsers.SelectedValue) <> Guid.Empty Then
            If Not DoesNotLockOutUsersPage(True) Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidLastAccess", Utilities.JsAlert("User """ & tbxUserName.Text & """ may not be deleted since they are the last user that has access to view/modify users.")) : Exit Sub
            With New KaUser(GetUserConnection(_currentUser.Id), Guid.Parse(ddlUsers.SelectedValue))
                .Deleted = True
                .SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                PopulateUsersList(Utilities.GetUser(Me))
                ddlUsers.SelectedIndex = 0
                ddlUsers_SelectedIndexChanged(ddlUsers, New EventArgs())
                lblStatus.Text = "Selected user deleted successfully"
            End With
        End If
    End Sub

    Private Sub chkSelectAll_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles chkSelectAll.CheckedChanged
        For i As Integer = 0 To cbxRights.Items.Count - 1
            cbxRights.Items.Item(i).Selected = chkSelectAll.Checked
        Next
        For i As Integer = 0 To cbxCustomPages.Items.Count - 1
            cbxCustomPages.Items.Item(i).Selected = chkSelectAll.Checked
        Next
    End Sub
#End Region

    Private Sub PopulateUsersList(ByVal currentUser As KaUser)
        ddlUsers.Items.Clear() ' populate the users list
        If _currentUserPermission(KaUser.TABLE_NAME).Create Then
            ddlUsers.Items.Add(New ListItem("Enter a new user", Guid.Empty.ToString()))
        Else
            ddlUsers.Items.Add(New ListItem("Select a user", Guid.Empty.ToString()))
        End If
        For Each u As KaUser In KaUser.GetAll(GetUserConnection(currentUser.Id), "deleted=0" & IIf(currentUser.OwnerId = Guid.Empty, "", " AND owner_id=" & Q(currentUser.OwnerId)), "name ASC")
            ddlUsers.Items.Add(New ListItem(u.Name, u.Id.ToString()))
        Next
    End Sub

    Private Sub PopulateOwnerList(ByVal currentUser As KaUser)
        ddlOwners.Items.Clear() ' populate the owners list
        ' Per TFS ID 931, we should always show all owners available.
        ddlOwners.Items.Add(New ListItem("All owners", Guid.Empty.ToString()))
        For Each o As KaOwner In KaOwner.GetAll(GetUserConnection(currentUser.Id), "deleted=0", "name ASC")
            ddlOwners.Items.Add(New ListItem(o.Name, o.Id.ToString()))
        Next
    End Sub

    Private Sub PopulateUserProfileList(ByVal currentUser As KaUser)
        ddlUserProfile.Items.Clear() ' populate the user profile list
        ddlUserProfile.Items.Add(New ListItem("No user profile", Guid.Empty.ToString()))
        For Each up As KaUserProfile In KaUserProfile.GetAll(GetUserConnection(currentUser.Id), "deleted=0", "name ASC")
            ddlUserProfile.Items.Add(New ListItem(up.Name, up.Id.ToString()))
        Next
    End Sub

    Private Sub PopulateUserData(ByVal userId As Guid)
        Dim user As KaUser
        Try
            user = New KaUser(GetUserConnection(_currentUser.Id), userId)
        Catch ex As RecordNotFoundException
            user = New KaUser()
        End Try

        With user
            tbxName.Text = .Name
            tbxUserName.Text = .Username
            tbxPassword.Attributes("value") = .Password
            Try
                ddlOwners.SelectedValue = .OwnerId.ToString()
            Catch ex As ArgumentOutOfRangeException
                ddlOwners.SelectedValue = Guid.Empty.ToString()
                lblStatus.Text = "Record not found in owners where ID = " & .OwnerId.ToString() & ". User owner was set to ""all owners""."
            End Try
            Try
                ddlUserProfile.SelectedValue = .UserProfileId.ToString()
            Catch ex As Exception
                ddlUserProfile.SelectedValue = Guid.Empty.ToString()
                lblStatus.Text = "Record not found in user profiles where ID = " & .UserProfileId.ToString() & ". User profile was set to ""none""."
            End Try
            cbxDisabled.Checked = .Disabled
            cbxAppConfig.Checked = .GetPermissionValueByName("AppConfig") = "M"

            cbxRights.Items(0).Selected = .GetPermissionValueByName("Orders") = "M" OrElse .GetPermissionValueByName("Orders") = "V"
            cbxRights.Items(1).Selected = .GetPermissionValueByName("Orders") = "M"
            cbxRights.Items(2).Selected = .GetPermissionValueByName("Owners") = "M"
            cbxRights.Items(3).Selected = .GetPermissionValueByName("Accounts") = "M"
            cbxRights.Items(4).Selected = .GetPermissionValueByName("Drivers") = "M"
            cbxRights.Items(5).Selected = .GetPermissionValueByName("Carriers") = "M"
            cbxRights.Items(6).Selected = .GetPermissionValueByName("Transports") = "M"
            cbxRights.Items(7).Selected = .GetPermissionValueByName("Users") = "M"
            cbxRights.Items(8).Selected = .GetPermissionValueByName("Products") = "M"
            cbxRights.Items(9).Selected = .GetPermissionValueByName("Inventory") = "M" OrElse .GetPermissionValueByName("Inventory") = "V"
            cbxRights.Items(10).Selected = .GetPermissionValueByName("Inventory") = "M"
            cbxRights.Items(11).Selected = .GetPermissionValueByName("Panels") = "M"
            cbxRights.Items(12).Selected = .GetPermissionValueByName("PanelBulkProductSettings") = "M"
            cbxRights.Items(13).Selected = .GetPermissionValueByName("Facilities") = "M"
            cbxRights.Items(14).Selected = .GetPermissionValueByName("Branches") = "M"
            cbxRights.Items(15).Selected = .GetPermissionValueByName("Reports") = "M" OrElse .GetPermissionValueByName("Reports") = "V"
            cbxRights.Items(16).Selected = .GetPermissionValueByName("Reports") = "M"
            cbxRights.Items(17).Selected = .GetPermissionValueByName("Containers") = "M"
            cbxRights.Items(18).Selected = .GetPermissionValueByName("PurchaseOrders") = "M" OrElse .GetPermissionValueByName("PurchaseOrders") = "V"
            cbxRights.Items(19).Selected = .GetPermissionValueByName("PurchaseOrders") = "M"
            cbxRights.Items(20).Selected = .GetPermissionValueByName("GeneralSettings") = "M"
            cbxRights.Items(21).Selected = .GetPermissionValueByName("Tanks") = "V" OrElse .GetPermissionValueByName("Tanks") = "M"
            cbxRights.Items(22).Selected = .GetPermissionValueByName("Tanks") = "M"
            cbxRights.Items(23).Selected = .GetPermissionValueByName("Units") = "M"
            cbxRights.Items(24).Selected = .GetPermissionValueByName("Crops") = "M"
            cbxRights.Items(25).Selected = .GetPermissionValueByName("Applicators") = "M"
            cbxRights.Items(26).Selected = .GetPermissionValueByName("CustomPages") = "M"
            cbxRights.Items(27).Selected = .GetPermissionValueByName("OrderPageInterfaces") = "M"
            cbxRights.Items(28).Selected = .GetPermissionValueByName("ReceiptPageInterfaces") = "M"
            cbxRights.Items(29).Selected = .GetPermissionValueByName("Emails") = "M"
            cbxRights.Items(30).Selected = .GetPermissionValueByName("StagedOrders") = "M"
            cbxRights.Items(31).Selected = .GetPermissionValueByName("Interfaces") = "M"


            Dim allCustomPages As ArrayList = KaCustomPages.GetAll(GetUserConnection(user.Id), "(main_menu_link = 1 or report = 1) and deleted <> 1", "page_label asc")
            If allCustomPages.Count > 0 Then
                cbxCustomPages.Items.Clear()

                Dim index As Integer = 0
                For Each customPage As KaCustomPages In allCustomPages
                    Dim li As ListItem = New ListItem
                    li.Value = customPage.Id.ToString
                    li.Text = customPage.PageLabel
                    cbxCustomPages.Items.Add(li)

                    If .GetPermissionValueByName(customPage.Id.ToString) = "M" Then
                        cbxCustomPages.Items(index).Selected = True
                    End If
                    index += 1
                Next
            End If
            If .Signature.Length = 0 Then
                litUserSignature.Text = NO_IMAGE_SPECIFIED_STRING
                btnClearSignature.Enabled = False
            Else
                litUserSignature.Text = IMAGE_PREFIX_STRING & .Signature & IMAGE_SUFFIX_STRING
                btnClearSignature.Enabled = True
            End If
        End With

        ddlUserProfile_SelectedIndexChanged(ddlUserProfile, Nothing)
        If userId <> Guid.Empty Then ' existing user
            pnlSignature.Visible = True
            With _currentUserPermission(KaUser.TABLE_NAME)
                btnDelete.Enabled = .Edit AndAlso .Delete
            End With
        Else ' new user
            btnDelete.Enabled = False
            pnlSignature.Visible = False
        End If
        Utilities.SetFocus(tbxName, Me)
    End Sub

    Private Sub SaveUser()
        Dim useWindowsAuthentication As Boolean = Utilities.UseWindowsAuthentication()
        If ValidateFields(useWindowsAuthentication) Then
            Dim userInfo As KaUser
            Try
                userInfo = New KaUser(Tm2Database.Connection, Guid.Parse(ddlUsers.SelectedValue))
            Catch ex As RecordNotFoundException
                userInfo = New KaUser()
            End Try
            With userInfo
                .Name = tbxName.Text
                .OwnerId = Guid.Parse(ddlOwners.SelectedValue)
                .UserProfileId = Guid.Parse(ddlUserProfile.SelectedValue)
                If Not useWindowsAuthentication Then .Password = tbxPassword.Text
                .Username = tbxUserName.Text
                .Disabled = cbxDisabled.Checked
                If litUserSignature.Text.StartsWith(IMAGE_PREFIX_STRING) Then
                    .Signature = litUserSignature.Text.Substring(IMAGE_PREFIX_STRING.Length, litUserSignature.Text.Length - IMAGE_PREFIX_STRING.Length - IMAGE_SUFFIX_STRING.Length)
                Else
                    .Signature = ""
                End If
                If ddlUserProfile.SelectedIndex = 0 Then
                    SetUserPermission(.Permissions, "Orders", IIf(cbxRights.Items(1).Selected, "M", IIf(cbxRights.Items(0).Selected, "V", "N")))
                    SetUserPermission(.Permissions, "Owners", IIf(cbxRights.Items(2).Selected, "M", "N"))
                    SetUserPermission(.Permissions, "Accounts", IIf(cbxRights.Items(3).Selected, "M", "N"))
                    SetUserPermission(.Permissions, "Drivers", IIf(cbxRights.Items(4).Selected, "M", "N"))
                    SetUserPermission(.Permissions, "Carriers", IIf(cbxRights.Items(5).Selected, "M", "N"))
                    SetUserPermission(.Permissions, "Transports", IIf(cbxRights.Items(6).Selected, "M", "N"))
                    SetUserPermission(.Permissions, "Users", IIf(cbxRights.Items(7).Selected, "M", "N"))
                    SetUserPermission(.Permissions, "Products", IIf(cbxRights.Items(8).Selected, "M", "N"))
                    SetUserPermission(.Permissions, "Inventory", IIf(cbxRights.Items(10).Selected, "M", IIf(cbxRights.Items(9).Selected, "V", "N")))
                    SetUserPermission(.Permissions, "Panels", IIf(cbxRights.Items(11).Selected, "M", "N"))
                    SetUserPermission(.Permissions, "PanelBulkProductSettings", IIf(cbxRights.Items(12).Selected, "M", "N"))
                    SetUserPermission(.Permissions, "Facilities", IIf(cbxRights.Items(13).Selected, "M", "N"))
                    SetUserPermission(.Permissions, "Branches", IIf(cbxRights.Items(14).Selected, "M", "N"))
                    SetUserPermission(.Permissions, "Reports", IIf(cbxRights.Items(16).Selected, "M", IIf(cbxRights.Items(15).Selected, "V", "N")))
                    SetUserPermission(.Permissions, "Containers", IIf(cbxRights.Items(17).Selected, "M", "N"))
                    SetUserPermission(.Permissions, "PurchaseOrders", IIf(cbxRights.Items(19).Selected, "M", IIf(cbxRights.Items(18).Selected, "V", "N")))
                    SetUserPermission(.Permissions, "GeneralSettings", IIf(cbxRights.Items(20).Selected, "M", "N"))
                    SetUserPermission(.Permissions, "Tanks", IIf(cbxRights.Items(22).Selected, "M", IIf(cbxRights.Items(21).Selected, "V", "N")))
                    SetUserPermission(.Permissions, "Units", IIf(cbxRights.Items(23).Selected, "M", "N"))
                    SetUserPermission(.Permissions, "Crops", IIf(cbxRights.Items(24).Selected, "M", "N"))
                    SetUserPermission(.Permissions, "Applicators", IIf(cbxRights.Items(25).Selected, "M", "N"))
                    SetUserPermission(.Permissions, "CustomPages", IIf(cbxRights.Items(26).Selected, "M", "N"))
                    SetUserPermission(.Permissions, "OrderPageInterfaces", IIf(cbxRights.Items(27).Selected, "M", "N"))
                    SetUserPermission(.Permissions, "ReceiptPageInterfaces", IIf(cbxRights.Items(28).Selected, "M", "N"))
                    SetUserPermission(.Permissions, "Emails", IIf(cbxRights.Items(29).Selected, "M", "N"))
                    SetUserPermission(.Permissions, "StagedOrders", IIf(cbxRights.Items(30).Selected, "M", "N"))
                    SetUserPermission(.Permissions, "Interfaces", IIf(cbxRights.Items(31).Selected, "M", "N"))
                End If
                SetUserPermission(.Permissions, "AppConfig", IIf(cbxAppConfig.Checked, "M", "N"))

                For i As Integer = 0 To cbxCustomPages.Items.Count - 1
                    Dim li As ListItem = cbxCustomPages.Items(i)
                    SetUserPermission(.Permissions, li.Value, IIf(cbxCustomPages.Items(i).Selected, "M", "N"))
                Next
                Dim status As String = ""
                If .Id = Guid.Empty Then
                    .SqlInsert(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                    status = "User added successfully"
                Else
                    .SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                    status = "User updated successfully"
                End If
                PopulateUsersList(Utilities.GetUser(Me))
                ddlUsers.SelectedValue = .Id.ToString
                ddlUsers_SelectedIndexChanged(ddlUsers, New EventArgs())
                lblStatus.Text = status
            End With
            btnDelete.Enabled = True
        End If
    End Sub

    Private Function ValidateFields(useWindowsAuthentication As Boolean) As Boolean
        If tbxName.Text.Trim().Length = 0 Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidName", Utilities.JsAlert("Please specify a name.")) : Return False
        If tbxUserName.Text.Trim().Length = 0 Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidUsername", Utilities.JsAlert("Please specify a username.")) : Return False
        If tbxPassword.Text.Trim().Length = 0 AndAlso Not useWindowsAuthentication Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidPassword", Utilities.JsAlert("Please specify a password.")) : Return False
        ' verify that the username is unique
        If KaUser.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND username=" & Q(tbxUserName.Text.Trim) & " AND id<>" & Q(ddlUsers.SelectedValue), "").Count > 0 Then
            ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidNameExists", Utilities.JsAlert("A user with the username """ & tbxUserName.Text.Trim & """ already exists. Please enter a unique username.")) : Return False
        End If
        If Not DoesNotLockOutUsersPage(False) Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidUserAccess", Utilities.JsAlert("User """ & tbxUserName.Text & """ may not be saved as disabled or without the ability to view/modify users rights since there are no other users with that ability.")) : Return False

        Return True
    End Function

    Private Function DoesNotLockOutUsersPage(deleting As Boolean) As Boolean ' confirm that at least one user is able to modify users so they can't lock themselves out
        If ddlUsers.SelectedIndex = 0 Then
            'New User
            Return True
        Else
            'Existing User
            If Not deleting AndAlso Not cbxDisabled.Checked Then
                If (cbxRights.Items(7).Selected AndAlso ddlUserProfile.SelectedIndex = 0) Then
                    'No profile, look at 'old' way we did permissions
                    Return True
                ElseIf ddlUserProfile.SelectedIndex > 0 Then
                    'Using profile, look at 'new' way we do permissions
                    Dim userProfile As KaUserProfile = New KaUserProfile(GetUserConnection(_currentUser.Id), Guid.Parse(ddlUserProfile.SelectedValue))
                    If userProfile.GetPermissionsForTable("Users").Edit Then
                        Return True
                    End If
                End If
            End If
        End If

        If ddlUsers.SelectedIndex > 0 Then
            'Check to see if any other users can modify users
            Dim allUsers As ArrayList = KaUser.GetAll(GetUserConnection(_currentUser.Id), "deleted = 0 AND disabled = 0 AND id <> " & Q(ddlUsers.SelectedValue), "")
            For Each user As KaUser In allUsers
                If user.UserProfileId = Guid.Empty AndAlso user.GetPermissionValueByName("Users") = "M" Then
                    'Permission the 'old' way
                    Return True
                ElseIf user.UserProfileId <> Guid.Empty Then
                    'Permission the 'new' way
                    Dim userProfile As KaUserProfile = New KaUserProfile(GetUserConnection(_currentUser.Id), user.UserProfileId)
                    If userProfile.GetPermissionsForTable("Users").Edit Then
                        Return True
                    End If
                End If
            Next
        End If

        Return False
    End Function

    Private Sub SetTextboxMaxLengths()
        tbxName.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaUser.TABLE_NAME, "name"))
		tbxPassword.MaxLength = KaUser.GetPasswordMaximumLength()
		tbxUserName.MaxLength = KaUser.GetUsernameMaximumLength()
	End Sub

    Private Sub btnUploadSignature_Click(sender As Object, e As System.EventArgs) Handles btnUploadSignature.Click
        ' Before attempting to save the file, verify
        ' that the FileUpload control contains a file.
        Dim user As KaUser = New KaUser(GetUserConnection(_currentUser.Id), Guid.Parse(ddlUsers.SelectedValue))
        With user
            If (objFileUpload.HasFile) Then
                ' Call a helper method routine to save the file.
                If objFileUpload.FileBytes.LongLength = 0 Then
                    litUserSignature.Text = NO_IMAGE_SPECIFIED_STRING
                Else
                    litUserSignature.Text = IMAGE_PREFIX_STRING & Convert.ToBase64String(objFileUpload.FileBytes) & IMAGE_SUFFIX_STRING
                    btnClearSignature.Enabled = True
                End If

                UploadStatusLabel.Text = ""
            Else
                ' Notify the user that a file was not uploaded.
                UploadStatusLabel.Text = "You did not specify a file to upload."
            End If
        End With
    End Sub

    Private Sub SetUserPermission(ByRef permissions As List(Of KaUserPermission), permissionName As String, value As String)
        For Each permission As KaUserPermission In permissions
            If permission.Name.ToLower = permissionName.ToLower Then
                permission.Value = value
                Exit Sub
            End If
        Next
        'if we get here permission wasn't found so create new one
        permissions.Add(New KaUserPermission(permissionName, value))
    End Sub

    Protected Sub ddlUserProfile_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlUserProfile.SelectedIndexChanged
        pnlOdd.Visible = ddlUserProfile.SelectedIndex = 0
    End Sub

    Private Sub SetControlUsabilityFromPermissions()
        With _currentUserPermission(KaUser.TABLE_NAME)
            pnlEven.Enabled = (.Edit AndAlso ddlUsers.SelectedIndex > 0) OrElse (.Create AndAlso ddlUsers.SelectedIndex = 0)
            pnlOdd.Enabled = pnlEven.Enabled
            btnSave.Enabled = pnlEven.Enabled
        End With
    End Sub

    Protected Sub btnClearSignature_Click(sender As Object, e As EventArgs) Handles btnClearSignature.Click
        litUserSignature.Text = NO_IMAGE_SPECIFIED_STRING
        btnClearSignature.Enabled = False
    End Sub
End Class