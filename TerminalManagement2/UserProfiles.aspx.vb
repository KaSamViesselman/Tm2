Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class UserProfiles
	Inherits System.Web.UI.Page
	Private _currentUser As KaUser
	Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing

	Private Const CREATE_OPTION_INDEX As Integer = 0
	Private Const EDIT_OPTION_INDEX As Integer = 1
	Private Const DELETE_OPTION_INDEX As Integer = 2

#Region "Events"

	Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load, Me.Load
		Response.Cache.SetCacheability(HttpCacheability.NoCache)
		_currentUser = Utilities.GetUser(Me)
		_currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaUser.TABLE_NAME}), "Users")
		If Not _currentUserPermission(KaUser.TABLE_NAME).Read Then Response.Redirect("Welcome.aspx")
        ScriptManager.RegisterStartupScript(PleaseWaitPanel, PleaseWaitPanel.GetType(), "RefreshScroll", "<script language='javascript'>$(window).scrollTop(document.getElementById('__SCROLLPOSITIONY').value); console.log(document.getElementById('__SCROLLPOSITIONY').value);</script>", False)

        If Not Page.IsPostBack Then
			SetTextboxMaxLengths()
			PopulateUserProfilesList(_currentUser)
			If Page.Request("UserId") IsNot Nothing Then
				Try
					ddlUserProfiles.SelectedValue = Page.Request("UserId")
				Catch ex As ArgumentOutOfRangeException
					ddlUserProfiles.SelectedIndex = 0
				End Try
			End If
			ddlUserProfiles_SelectedIndexChanged(ddlUserProfiles, New EventArgs)
			Utilities.ConfirmBox(Me.btnDelete, "Are you sure you want to delete this user profile?")
			Utilities.SetFocus(tbxName, Me)
		End If
	End Sub

	Private Sub ddlUserProfiles_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ddlUserProfiles.SelectedIndexChanged
		lblStatus.Text = ""
		Dim userId As Guid = Guid.Parse(ddlUserProfiles.SelectedValue)
		PopulateUserData(userId)
		SetControlUsabilityFromPermissions()
		ScriptManager.RegisterStartupScript(PleaseWaitPanel, PleaseWaitPanel.GetType(), "RefreshScroll", "<script language='javascript'>document.documentElement.scrollTop = document.getElementById('hfScrollPosition').value;</script>", False)

	End Sub

	Private Sub btnSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSave.Click
		SaveUser()
	End Sub

	Private Sub btnDelete_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDelete.Click
		lblStatus.Text = ""
		Dim id As Guid = Guid.Parse(ddlUserProfiles.SelectedValue)
		If id <> Guid.Empty Then
			If KaUser.GetAll(GetUserConnection(_currentUser.Id), String.Format("deleted = 0 AND {0}={1}", KaUser.FN_USER_PROFILE_ID, Q(id)), "").Count > 0 Then ScriptManager.RegisterStartupScript(PleaseWaitPanel, PleaseWaitPanel.GetType(), "InvalidUserStillAssigned", Utilities.JsAlert("Unable to delete because there are still users assigned to this profile."), False) : Exit Sub

			If Not DoesNotLockOutUsersPage(True) Then ScriptManager.RegisterStartupScript(PleaseWaitPanel, PleaseWaitPanel.GetType(), "InvalidLastAccess", Utilities.JsAlert("User """ & tbxName.Text & """ may not be deleted since they are the last user that has access to view/modify users."), False) : Exit Sub

			With New KaUserProfile(GetUserConnection(_currentUser.Id), Guid.Parse(ddlUserProfiles.SelectedValue))
				.Deleted = True
				.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
				PopulateUserProfilesList(Utilities.GetUser(Me))
				ddlUserProfiles.SelectedIndex = 0
				ddlUserProfiles_SelectedIndexChanged(ddlUserProfiles, New EventArgs())
				lblStatus.Text = "Selected user deleted successfully"
			End With
		End If
	End Sub

	Private Sub chkSelectAll_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles chkSelectAll.CheckedChanged
		For i As Integer = 0 To cbxCustomPages.Items.Count - 1
			cbxCustomPages.Items.Item(i).Selected = chkSelectAll.Checked
		Next
	End Sub

	Private Sub GetControlList(Of T As Control)(controlCollection As ControlCollection, resultCollection As List(Of T))
		For Each control As Control In controlCollection
			If TypeOf control Is T Then
				resultCollection.Add(DirectCast(control, T))
			End If
			If control.HasControls() Then
				GetControlList(control.Controls, resultCollection)
			End If
		Next
	End Sub

	Private Sub rblSelectAll_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles rblSelectAll.SelectedIndexChanged
		Dim allRBL As New List(Of RadioButtonList)
		GetControlList(Of RadioButtonList)(Page.Controls, allRBL)
		If rblSelectAll.SelectedValue = "N" OrElse rblSelectAll.SelectedValue = "V" OrElse rblSelectAll.SelectedValue = "M" Then
			For Each childControl In allRBL
				SetPermission(childControl, rblSelectAll.SelectedValue)
			Next
		End If
		rblSelectAll.SelectedIndex = -1
		If rblSelectAll.SelectedValue <> "M" Then cblSelectAll.SelectedIndex = -1
	End Sub

	Protected Sub cblSelectAll_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cblSelectAll.SelectedIndexChanged
		Dim allCBL As New List(Of CheckBoxList)
		GetControlList(Of CheckBoxList)(Page.Controls, allCBL)
		For Each childControl In allCBL
			If childControl.Enabled And (childControl.Items.Contains(cblSelectAll.Items.Item(CREATE_OPTION_INDEX)) Or childControl.Items.Contains(cblSelectAll.Items.Item(EDIT_OPTION_INDEX)) Or childControl.Items.Contains(cblSelectAll.Items.Item(DELETE_OPTION_INDEX))) Then
				If _lastSelectAllState(CREATE_OPTION_INDEX) <> cblSelectAll.Items(CREATE_OPTION_INDEX).Selected Then childControl.Items(CREATE_OPTION_INDEX).Selected = cblSelectAll.Items(CREATE_OPTION_INDEX).Selected
				If _lastSelectAllState(EDIT_OPTION_INDEX) <> cblSelectAll.Items(EDIT_OPTION_INDEX).Selected Then childControl.Items(EDIT_OPTION_INDEX).Selected = cblSelectAll.Items(EDIT_OPTION_INDEX).Selected
				If _lastSelectAllState(DELETE_OPTION_INDEX) <> cblSelectAll.Items(DELETE_OPTION_INDEX).Selected Then childControl.Items(DELETE_OPTION_INDEX).Selected = cblSelectAll.Items(DELETE_OPTION_INDEX).Selected

				cblPermissions_SelectedIndexChanged(childControl, Nothing)
			End If
		Next
	End Sub

	Private _lastSelectAllState(2) As Boolean
	Protected Overrides Function SaveViewState() As Object
		Dim viewState(1) As Object
		'Saving the grid values to the View State
		_lastSelectAllState(CREATE_OPTION_INDEX) = cblSelectAll.Items(CREATE_OPTION_INDEX).Selected
		_lastSelectAllState(EDIT_OPTION_INDEX) = cblSelectAll.Items(EDIT_OPTION_INDEX).Selected
		_lastSelectAllState(DELETE_OPTION_INDEX) = cblSelectAll.Items(DELETE_OPTION_INDEX).Selected

		viewState(0) = _lastSelectAllState
		viewState(1) = MyBase.SaveViewState()
		Return viewState
	End Function

	Protected Overrides Sub LoadViewState(savedState As Object)
		'Getting the dropdown list value from view state.
		If savedState IsNot Nothing AndAlso CType(savedState, Object).Length = 2 Then
			_currentUser = Utilities.GetUser(Me)
			Dim viewState As Object() = savedState
			_lastSelectAllState = viewState(0)
			MyBase.LoadViewState(viewState(1))
		Else
			MyBase.LoadViewState(savedState)
		End If
	End Sub
#End Region

	Private Sub PopulateUserProfilesList(ByVal currentUser As KaUser)
		ddlUserProfiles.Items.Clear() ' populate the users list
		If _currentUserPermission(KaUser.TABLE_NAME).Create Then
			ddlUserProfiles.Items.Add(New ListItem("Enter a new user profile", Guid.Empty.ToString()))
		Else
			ddlUserProfiles.Items.Add(New ListItem("Select a user profile", Guid.Empty.ToString()))
		End If
        For Each up As KaUserProfile In KaUserProfile.GetAll(GetUserConnection(currentUser.Id), "deleted=0", "name ASC")
            ddlUserProfiles.Items.Add(New ListItem(up.Name, up.Id.ToString()))
        Next
        ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), "ResetScrollPosition;", "resetDotNetScrollPosition();", True)
    End Sub

	Private Sub PopulateUserData(ByVal userId As Guid)
		Dim userprofile As KaUserProfile
		Try
			userprofile = New KaUserProfile(GetUserConnection(_currentUser.Id), userId)
		Catch ex As RecordNotFoundException
			userprofile = New KaUserProfile()
		End Try

		With userprofile
			tbxName.Text = .Name

			'cbxAppConfig.Checked = .GetPermissionValueByName("AppConfig")
			Dim tablePermissions As List(Of KaTablePermission)
			Try
				tablePermissions = Tm2Database.FromXml(.TablePermissions, GetType(List(Of KaTablePermission)))
			Catch ex As Exception
				tablePermissions = New List(Of KaTablePermission)
			End Try
			AssignPermissionValues(rblApplicatorsPermissions, cblApplicatorsPermissions, GetTablePermission(tablePermissions, KaApplicator.TABLE_NAME))
			AssignPermissionValues(rblBranchesPermissions, cblBranchesPermissions, GetTablePermission(tablePermissions, KaBranch.TABLE_NAME))
			AssignPermissionValues(rblCarriersPermissions, cblCarriersPermissions, GetTablePermission(tablePermissions, KaCarrier.TABLE_NAME))
			AssignPermissionValues(rblContainersPermissions, cblContainersPermissions, GetTablePermission(tablePermissions, KaContainer.TABLE_NAME))
			AssignPermissionValues(rblCropsPermissions, cblCropsPermissions, GetTablePermission(tablePermissions, KaCropType.TABLE_NAME))
			AssignPermissionValues(rblCustomerAccountsPermissions, cblCustomerAccountsPermissions, GetTablePermission(tablePermissions, KaCustomerAccount.TABLE_NAME))
			AssignPermissionValues(rblCustomPagesPermissions, cblCustomPagesPermissions, GetTablePermission(tablePermissions, KaCustomPages.TABLE_NAME))
			AssignPermissionValues(rblDriversPermissions, cblDriversPermissions, GetTablePermission(tablePermissions, KaDriver.TABLE_NAME))
			AssignPermissionValues(rblEmailsPermissions, cblEmailsPermissions, GetTablePermission(tablePermissions, KaEmail.TABLE_NAME))
			AssignPermissionValues(rblFacilitiesPermissions, cblFacilitiesPermissions, GetTablePermission(tablePermissions, KaLocation.TABLE_NAME))
			AssignPermissionValues(rblGeneralSettingsPermissions, cblGeneralSettingsPermissions, GetTablePermission(tablePermissions, KaSetting.TABLE_NAME))
			AssignPermissionValues(rblInterfacesPermissions, cblInterfacesPermissions, GetTablePermission(tablePermissions, KaInterface.TABLE_NAME))
			AssignPermissionValues(rblInventoryPermissions, cblInventoryPermissions, GetTablePermission(tablePermissions, KaBulkProductInventory.TABLE_NAME))
			AssignPermissionValues(rblOrderPageInterfacesPermissions, cblOrderPageInterfacesPermissions, GetTablePermission(tablePermissions, "OrderPageInterfaces"))
			AssignPermissionValues(rblOrderPermissions, cblOrderPermissions, GetTablePermission(tablePermissions, KaOrder.TABLE_NAME))
			AssignPermissionValues(rblOwnerPermissions, cblOwnerPermissions, GetTablePermission(tablePermissions, KaOwner.TABLE_NAME))
			AssignPermissionValues(rblPanelsPermissions, cblPanelsPermissions, GetTablePermission(tablePermissions, KaPanel.TABLE_NAME))
			AssignPermissionValues(rblPanelBulkPermissions, cblPanelBulkPermissions, GetTablePermission(tablePermissions, KaBulkProductPanelSettings.TABLE_NAME))
			AssignPermissionValues(rblProductsPermissions, cblProductsPermissions, GetTablePermission(tablePermissions, KaProduct.TABLE_NAME))
			AssignPermissionValues(rblPurchaseOrdersPermissions, cblPurchaseOrdersPermissions, GetTablePermission(tablePermissions, KaReceivingPurchaseOrder.TABLE_NAME))
			AssignPermissionValues(rblReceiptPageInterfacesPermissions, cblReceiptPageInterfacesPermissions, GetTablePermission(tablePermissions, "ReceiptPageInterfaces"))
			AssignPermissionValues(rblReportsPermissions, cblReportsPermissions, GetTablePermission(tablePermissions, "reports"))
			AssignPermissionValues(rblStagedOrdersPermissions, cblStagedOrdersPermissions, GetTablePermission(tablePermissions, KaStagedOrder.TABLE_NAME))
			AssignPermissionValues(rblTanksPermissions, cblTanksPermissions, GetTablePermission(tablePermissions, KaTank.TABLE_NAME))
			AssignPermissionValues(rblTransportsPermissions, cblTransportsPermissions, GetTablePermission(tablePermissions, KaTransport.TABLE_NAME))
			AssignPermissionValues(rblUnitsPermissions, cblUnitsPermissions, GetTablePermission(tablePermissions, KaUnit.TABLE_NAME))
			AssignPermissionValues(rblUsersPermissions, cblUsersPermissions, GetTablePermission(tablePermissions, KaUser.TABLE_NAME))

			Dim allCustomPages As ArrayList = KaCustomPages.GetAll(GetUserConnection(userprofile.Id), "(main_menu_link = 1 or report = 1) and deleted <> 1", "page_label asc")
			If allCustomPages.Count > 0 Then
				cbxCustomPages.Items.Clear()

				Dim index As Integer = 0
				For Each customPage As KaCustomPages In allCustomPages
					Dim li As ListItem = New ListItem
					li.Value = customPage.Id.ToString
					li.Text = customPage.PageLabel
					cbxCustomPages.Items.Add(li)
					Dim tp As KaTablePermission = GetTablePermission(tablePermissions, customPage.Id.ToString)

					cbxCustomPages.Items(index).Selected = tp.Read

					index += 1
				Next
				pnlCustomPages.Visible = True
			Else
				pnlCustomPages.Visible = False
			End If
		End With

		If userId <> Guid.Empty Then ' existing user profile
			With _currentUserPermission(KaUser.TABLE_NAME)
				btnDelete.Enabled = .Edit AndAlso .Delete
			End With
		Else ' new user
			btnDelete.Enabled = False
		End If
		Utilities.SetFocus(tbxName, Me)
	End Sub

	Private Sub SaveUser()
		Dim useWindowsAuthentication As Boolean = Utilities.UseWindowsAuthentication()
		If ValidateFields(useWindowsAuthentication) Then
			Dim userInfo As KaUserProfile
			Try
				userInfo = New KaUserProfile(Tm2Database.Connection, Guid.Parse(ddlUserProfiles.SelectedValue))
			Catch ex As RecordNotFoundException
				userInfo = New KaUserProfile()
			End Try
			With userInfo
				.Name = tbxName.Text
				Dim tablePermissions As List(Of KaTablePermission)
				Try
					tablePermissions = Tm2Database.FromXml(.TablePermissions, GetType(List(Of KaTablePermission)))
				Catch ex As Exception
					tablePermissions = New List(Of KaTablePermission)
				End Try
				SetTablePermission(tablePermissions, KaCustomerAccount.TABLE_NAME, rblCustomerAccountsPermissions.SelectedValue <> "N", cblCustomerAccountsPermissions.Items(CREATE_OPTION_INDEX).Selected, cblCustomerAccountsPermissions.Items(EDIT_OPTION_INDEX).Selected, cblCustomerAccountsPermissions.Items(DELETE_OPTION_INDEX).Selected)
				SetTablePermission(tablePermissions, KaDriver.TABLE_NAME, rblDriversPermissions.SelectedValue <> "N", cblDriversPermissions.Items(CREATE_OPTION_INDEX).Selected, cblDriversPermissions.Items(EDIT_OPTION_INDEX).Selected, cblDriversPermissions.Items(DELETE_OPTION_INDEX).Selected)
				SetTablePermission(tablePermissions, KaCarrier.TABLE_NAME, rblCarriersPermissions.SelectedValue <> "N", cblCarriersPermissions.Items(CREATE_OPTION_INDEX).Selected, cblCarriersPermissions.Items(EDIT_OPTION_INDEX).Selected, cblCarriersPermissions.Items(DELETE_OPTION_INDEX).Selected)
				SetTablePermission(tablePermissions, KaTransport.TABLE_NAME, rblTransportsPermissions.SelectedValue <> "N", cblTransportsPermissions.Items(CREATE_OPTION_INDEX).Selected, cblTransportsPermissions.Items(EDIT_OPTION_INDEX).Selected, cblTransportsPermissions.Items(DELETE_OPTION_INDEX).Selected)
				SetTablePermission(tablePermissions, KaUser.TABLE_NAME, rblUsersPermissions.SelectedValue <> "N", cblUsersPermissions.Items(CREATE_OPTION_INDEX).Selected, cblUsersPermissions.Items(EDIT_OPTION_INDEX).Selected, cblUsersPermissions.Items(DELETE_OPTION_INDEX).Selected)
				SetTablePermission(tablePermissions, KaProduct.TABLE_NAME, rblProductsPermissions.SelectedValue <> "N", cblProductsPermissions.Items(CREATE_OPTION_INDEX).Selected, cblProductsPermissions.Items(EDIT_OPTION_INDEX).Selected, cblProductsPermissions.Items(DELETE_OPTION_INDEX).Selected)
				SetTablePermission(tablePermissions, KaBulkProductInventory.TABLE_NAME, rblInventoryPermissions.SelectedValue <> "N", cblInventoryPermissions.Items(CREATE_OPTION_INDEX).Selected, cblInventoryPermissions.Items(EDIT_OPTION_INDEX).Selected, cblInventoryPermissions.Items(DELETE_OPTION_INDEX).Selected)
				SetTablePermission(tablePermissions, KaPanel.TABLE_NAME, rblPanelsPermissions.SelectedValue <> "N", cblPanelsPermissions.Items(CREATE_OPTION_INDEX).Selected, cblPanelsPermissions.Items(EDIT_OPTION_INDEX).Selected, cblPanelsPermissions.Items(DELETE_OPTION_INDEX).Selected)
				SetTablePermission(tablePermissions, KaBulkProductPanelSettings.TABLE_NAME, rblPanelBulkPermissions.SelectedValue <> "N", cblPanelBulkPermissions.Items(CREATE_OPTION_INDEX).Selected, cblPanelBulkPermissions.Items(EDIT_OPTION_INDEX).Selected, cblPanelBulkPermissions.Items(DELETE_OPTION_INDEX).Selected)
				SetTablePermission(tablePermissions, KaLocation.TABLE_NAME, rblFacilitiesPermissions.SelectedValue <> "N", cblFacilitiesPermissions.Items(CREATE_OPTION_INDEX).Selected, cblFacilitiesPermissions.Items(EDIT_OPTION_INDEX).Selected, cblFacilitiesPermissions.Items(DELETE_OPTION_INDEX).Selected)
				SetTablePermission(tablePermissions, KaBranch.TABLE_NAME, rblBranchesPermissions.SelectedValue <> "N", cblBranchesPermissions.Items(CREATE_OPTION_INDEX).Selected, cblBranchesPermissions.Items(EDIT_OPTION_INDEX).Selected, cblBranchesPermissions.Items(DELETE_OPTION_INDEX).Selected)
				SetTablePermission(tablePermissions, "reports", rblReportsPermissions.SelectedValue <> "N", cblReportsPermissions.Items(CREATE_OPTION_INDEX).Selected, cblReportsPermissions.Items(EDIT_OPTION_INDEX).Selected, cblReportsPermissions.Items(DELETE_OPTION_INDEX).Selected)
				SetTablePermission(tablePermissions, KaContainer.TABLE_NAME, rblContainersPermissions.SelectedValue <> "N", cblContainersPermissions.Items(CREATE_OPTION_INDEX).Selected, cblContainersPermissions.Items(EDIT_OPTION_INDEX).Selected, cblContainersPermissions.Items(DELETE_OPTION_INDEX).Selected)
				SetTablePermission(tablePermissions, KaReceivingPurchaseOrder.TABLE_NAME, rblPurchaseOrdersPermissions.SelectedValue <> "N", cblPurchaseOrdersPermissions.Items(CREATE_OPTION_INDEX).Selected, cblPurchaseOrdersPermissions.Items(EDIT_OPTION_INDEX).Selected, cblPurchaseOrdersPermissions.Items(DELETE_OPTION_INDEX).Selected)
				SetTablePermission(tablePermissions, KaSetting.TABLE_NAME, rblGeneralSettingsPermissions.SelectedValue <> "N", cblGeneralSettingsPermissions.Items(CREATE_OPTION_INDEX).Selected, cblGeneralSettingsPermissions.Items(EDIT_OPTION_INDEX).Selected, cblGeneralSettingsPermissions.Items(DELETE_OPTION_INDEX).Selected)
				SetTablePermission(tablePermissions, KaTank.TABLE_NAME, rblTanksPermissions.SelectedValue <> "N", cblTanksPermissions.Items(CREATE_OPTION_INDEX).Selected, cblTanksPermissions.Items(EDIT_OPTION_INDEX).Selected, cblTanksPermissions.Items(DELETE_OPTION_INDEX).Selected)
				SetTablePermission(tablePermissions, KaUnit.TABLE_NAME, rblUnitsPermissions.SelectedValue <> "N", cblUnitsPermissions.Items(CREATE_OPTION_INDEX).Selected, cblUnitsPermissions.Items(EDIT_OPTION_INDEX).Selected, cblUnitsPermissions.Items(DELETE_OPTION_INDEX).Selected)
				SetTablePermission(tablePermissions, KaCropType.TABLE_NAME, rblCropsPermissions.SelectedValue <> "N", cblCropsPermissions.Items(CREATE_OPTION_INDEX).Selected, cblCropsPermissions.Items(EDIT_OPTION_INDEX).Selected, cblCropsPermissions.Items(DELETE_OPTION_INDEX).Selected)
				SetTablePermission(tablePermissions, KaApplicator.TABLE_NAME, rblApplicatorsPermissions.SelectedValue <> "N", cblApplicatorsPermissions.Items(CREATE_OPTION_INDEX).Selected, cblApplicatorsPermissions.Items(EDIT_OPTION_INDEX).Selected, cblApplicatorsPermissions.Items(DELETE_OPTION_INDEX).Selected)
				SetTablePermission(tablePermissions, KaCustomPages.TABLE_NAME, rblCustomPagesPermissions.SelectedValue <> "N", cblCustomPagesPermissions.Items(CREATE_OPTION_INDEX).Selected, cblCustomPagesPermissions.Items(EDIT_OPTION_INDEX).Selected, cblCustomPagesPermissions.Items(DELETE_OPTION_INDEX).Selected)
				SetTablePermission(tablePermissions, "OrderPageInterfaces", rblOrderPageInterfacesPermissions.SelectedValue <> "N", cblOrderPageInterfacesPermissions.Items(CREATE_OPTION_INDEX).Selected, cblOrderPageInterfacesPermissions.Items(EDIT_OPTION_INDEX).Selected, cblOrderPageInterfacesPermissions.Items(DELETE_OPTION_INDEX).Selected)
				SetTablePermission(tablePermissions, KaOrder.TABLE_NAME, rblOrderPermissions.SelectedValue <> "N", cblOrderPermissions.Items(CREATE_OPTION_INDEX).Selected, cblOrderPermissions.Items(EDIT_OPTION_INDEX).Selected, cblOrderPermissions.Items(DELETE_OPTION_INDEX).Selected)
				SetTablePermission(tablePermissions, KaOwner.TABLE_NAME, rblOwnerPermissions.SelectedValue <> "N", cblOwnerPermissions.Items(CREATE_OPTION_INDEX).Selected, cblOwnerPermissions.Items(EDIT_OPTION_INDEX).Selected, cblOwnerPermissions.Items(DELETE_OPTION_INDEX).Selected)
				SetTablePermission(tablePermissions, "ReceiptPageInterfaces", rblReceiptPageInterfacesPermissions.SelectedValue <> "N", cblReceiptPageInterfacesPermissions.Items(CREATE_OPTION_INDEX).Selected, cblReceiptPageInterfacesPermissions.Items(EDIT_OPTION_INDEX).Selected, cblReceiptPageInterfacesPermissions.Items(DELETE_OPTION_INDEX).Selected)
				SetTablePermission(tablePermissions, KaEmail.TABLE_NAME, rblEmailsPermissions.SelectedValue <> "N", cblEmailsPermissions.Items(CREATE_OPTION_INDEX).Selected, cblEmailsPermissions.Items(EDIT_OPTION_INDEX).Selected, cblEmailsPermissions.Items(DELETE_OPTION_INDEX).Selected)
				SetTablePermission(tablePermissions, KaStagedOrder.TABLE_NAME, rblStagedOrdersPermissions.SelectedValue <> "N", cblStagedOrdersPermissions.Items(CREATE_OPTION_INDEX).Selected, cblStagedOrdersPermissions.Items(EDIT_OPTION_INDEX).Selected, cblStagedOrdersPermissions.Items(DELETE_OPTION_INDEX).Selected)
				SetTablePermission(tablePermissions, KaInterface.TABLE_NAME, rblInterfacesPermissions.SelectedValue <> "N", cblInterfacesPermissions.Items(CREATE_OPTION_INDEX).Selected, cblInterfacesPermissions.Items(EDIT_OPTION_INDEX).Selected, cblInterfacesPermissions.Items(DELETE_OPTION_INDEX).Selected)

				For i As Integer = 0 To cbxCustomPages.Items.Count - 1
					Dim li As ListItem = cbxCustomPages.Items(i)
					Dim enabled As Boolean = cbxCustomPages.Items(i).Selected
					SetTablePermission(tablePermissions, li.Value, enabled, enabled, enabled, enabled)
				Next

				.TablePermissions = Tm2Database.ToXml(tablePermissions, GetType(List(Of KaTablePermission)))

				Dim status As String = ""
				If .Id = Guid.Empty Then
					.SqlInsert(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
					status = "User profile added successfully"
				Else
					.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
					status = "User profile updated successfully"
				End If
				PopulateUserProfilesList(Utilities.GetUser(Me))
				ddlUserProfiles.SelectedValue = .Id.ToString
				ddlUserProfiles_SelectedIndexChanged(ddlUserProfiles, New EventArgs())
				lblStatus.Text = status
			End With
			btnDelete.Enabled = True
		End If
	End Sub

	Private Sub SetTablePermission(ByRef permissions As List(Of KaTablePermission), permissionName As String, read As Boolean, create As Boolean, edit As Boolean, delete As Boolean)
		If delete AndAlso Not edit Then ScriptManager.RegisterStartupScript(PleaseWaitPanel, PleaseWaitPanel.GetType(), "InvalidDelete", Utilities.JsAlert("The delete permission requires that edit be selected as well. Permissions in violation of this have had delete deselected."), False)
		For Each permission As KaTablePermission In permissions
			If permission.TableName.ToLower = permissionName.ToLower Then
				permission.Read = read
				permission.Create = create
				permission.Edit = edit
				permission.Delete = delete AndAlso edit
				Exit Sub
			End If
		Next
		'if we get here permission wasn't found so create new one
		Dim p As KaTablePermission = New KaTablePermission()
		p.TableName = permissionName.ToLower
		p.Read = read
		p.Create = create
		p.Edit = edit
		p.Delete = delete AndAlso edit
		permissions.Add(p)
	End Sub

	Private Function GetTablePermission(ByVal permissions As List(Of KaTablePermission), permissionName As String) As KaTablePermission
		For Each permission As KaTablePermission In permissions
			If permission.TableName.ToLower = permissionName.ToLower Then
				Return permission
			End If
		Next
		'if we get here permission wasn't found so create new one
		Dim p As KaTablePermission = New KaTablePermission()
		p.TableName = permissionName.ToLower
		Return p
	End Function

	Private Sub AssignPermissionValues(ByRef rbl As RadioButtonList, ByRef cbl As CheckBoxList, ByVal permission As KaTablePermission)
		If Not permission.Read Then
			rbl.SelectedValue = "N"
			cbl.Items(CREATE_OPTION_INDEX).Selected = False
			cbl.Items(EDIT_OPTION_INDEX).Selected = False
			cbl.Items(DELETE_OPTION_INDEX).Selected = False
		ElseIf permission.Create Or permission.Delete Or permission.Edit Then
			rbl.SelectedValue = "M"
			cbl.Items(CREATE_OPTION_INDEX).Selected = permission.Create
			cbl.Items(EDIT_OPTION_INDEX).Selected = permission.Edit
			cbl.Items(DELETE_OPTION_INDEX).Selected = permission.Delete
		Else
			rbl.SelectedValue = "V"
			cbl.Items(CREATE_OPTION_INDEX).Selected = False
			cbl.Items(EDIT_OPTION_INDEX).Selected = False
			cbl.Items(DELETE_OPTION_INDEX).Selected = False
		End If
		rblPermissionsSelectedIndexChanged(rbl, Nothing)
	End Sub

	Private Function ValidateFields(useWindowsAuthentication As Boolean) As Boolean
		If tbxName.Text.Trim().Length = 0 Then ScriptManager.RegisterStartupScript(PleaseWaitPanel, PleaseWaitPanel.GetType(), "InvalidName", Utilities.JsAlert("Please specify a name."), False) : Return False
		' verify that the profile is not assigned to a user
		If KaUserProfile.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND name=" & Q(tbxName.Text.Trim) & " AND id<>" & Q(ddlUserProfiles.SelectedValue), "").Count > 0 Then
			ScriptManager.RegisterStartupScript(PleaseWaitPanel, Me.GetType(), "InvalidNameExists", Utilities.JsAlert("A user profile with the name """ & tbxName.Text & """ already exists. Please enter a unique username."), False) : Return False
		End If
		If Not DoesNotLockOutUsersPage(False) Then ScriptManager.RegisterStartupScript(PleaseWaitPanel, Me.GetType(), "InvalidUserAccess", Utilities.JsAlert("User profile """ & tbxName.Text & """ may not be saved as disabled or without the ability to view/modify users rights since there are no other users with that ability."), False) : Return False

		Return True
	End Function

	Private Function DoesNotLockOutUsersPage(deleting As Boolean) As Boolean
		If ddlUserProfiles.SelectedIndex = 0 Then
			'New Profile
			Return True
		Else
			'Existing Profile
			If Not deleting Then
				If rblUsersPermissions.SelectedValue = "M" AndAlso cblUsersPermissions.Items(EDIT_OPTION_INDEX).Selected Then
					'This profile has user privileges for editing a user.
					Return True
				End If
			End If
		End If

		'Check to see if any other users can modify users
		Dim allUsers As ArrayList = KaUser.GetAll(GetUserConnection(_currentUser.Id), "deleted = 0 AND disabled = 0 AND user_profile_id <> " & Q(ddlUserProfiles.SelectedValue), "")
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

		Return False
	End Function

	Private Sub SetTextboxMaxLengths()
		tbxName.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaUserProfile.TABLE_NAME, "name"))
	End Sub

	Private Sub SetPermission(ByRef radioButtonList As RadioButtonList, permission As String)
		Try
			radioButtonList.SelectedValue = permission
		Catch ex As ArgumentOutOfRangeException
			radioButtonList.SelectedValue = "N"
		End Try
		rblPermissionsSelectedIndexChanged(radioButtonList, Nothing)
	End Sub

	Private Sub ModifyChecker(rbl As RadioButtonList, cbl As CheckBoxList)
		cbl.Enabled = rbl.SelectedValue = "M"
		If rbl.SelectedValue <> "M" Then
			cbl.ClearSelection()
		ElseIf cbl.SelectedIndex = -1 Then
			cbl.Items(CREATE_OPTION_INDEX).Selected = True
			cbl.Items(EDIT_OPTION_INDEX).Selected = True
			cbl.Items(DELETE_OPTION_INDEX).Selected = True
		End If
	End Sub

	Protected Sub rblPermissionsSelectedIndexChanged(sender As Object, e As EventArgs) Handles rblApplicatorsPermissions.SelectedIndexChanged, rblBranchesPermissions.SelectedIndexChanged, rblCarriersPermissions.SelectedIndexChanged, rblContainersPermissions.SelectedIndexChanged, rblCropsPermissions.SelectedIndexChanged, rblCustomerAccountsPermissions.SelectedIndexChanged, rblCustomPagesPermissions.SelectedIndexChanged, rblDriversPermissions.SelectedIndexChanged, rblEmailsPermissions.SelectedIndexChanged, rblFacilitiesPermissions.SelectedIndexChanged, rblGeneralSettingsPermissions.SelectedIndexChanged, rblInterfacesPermissions.SelectedIndexChanged, rblInventoryPermissions.SelectedIndexChanged, rblOrderPageInterfacesPermissions.SelectedIndexChanged, rblOrderPermissions.SelectedIndexChanged, rblOwnerPermissions.SelectedIndexChanged, rblPanelBulkPermissions.SelectedIndexChanged, rblPanelsPermissions.SelectedIndexChanged, rblProductsPermissions.SelectedIndexChanged, rblPurchaseOrdersPermissions.SelectedIndexChanged, rblReceiptPageInterfacesPermissions.SelectedIndexChanged, rblReportsPermissions.SelectedIndexChanged, rblStagedOrdersPermissions.SelectedIndexChanged, rblTanksPermissions.SelectedIndexChanged, rblTransportsPermissions.SelectedIndexChanged, rblUnitsPermissions.SelectedIndexChanged, rblUsersPermissions.SelectedIndexChanged
		Select Case CType(sender, RadioButtonList).ID
			Case "rblApplicatorsPermissions"
				ModifyChecker(sender, cblApplicatorsPermissions)
			Case "rblBranchesPermissions"
				ModifyChecker(sender, cblBranchesPermissions)
			Case "rblCarriersPermissions"
				ModifyChecker(sender, cblCarriersPermissions)
			Case "rblContainersPermissions"
				ModifyChecker(sender, cblContainersPermissions)
			Case "rblCropsPermissions"
				ModifyChecker(sender, cblCropsPermissions)
			Case "rblCustomerAccountsPermissions"
				ModifyChecker(sender, cblCustomerAccountsPermissions)
			Case "rblCustomPagesPermissions"
				ModifyChecker(sender, cblCustomPagesPermissions)
			Case "rblDriversPermissions"
				ModifyChecker(sender, cblDriversPermissions)
			Case "rblEmailsPermissions"
				ModifyChecker(sender, cblEmailsPermissions)
			Case "rblFacilitiesPermissions"
				ModifyChecker(sender, cblFacilitiesPermissions)
			Case "rblGeneralSettingsPermissions"
				ModifyChecker(sender, cblGeneralSettingsPermissions)
			Case "rblInterfacesPermissions"
				ModifyChecker(sender, cblInterfacesPermissions)
			Case "rblInventoryPermissions"
				ModifyChecker(sender, cblInventoryPermissions)
			Case "rblOrderPageInterfacesPermissions"
				ModifyChecker(sender, cblOrderPageInterfacesPermissions)
			Case "rblOrderPermissions"
				ModifyChecker(sender, cblOrderPermissions)
			Case "rblOwnerPermissions"
				ModifyChecker(sender, cblOwnerPermissions)
			Case "rblPanelBulkPermissions"
				ModifyChecker(sender, cblPanelBulkPermissions)
			Case "rblPanelsPermissions"
				ModifyChecker(sender, cblPanelsPermissions)
			Case "rblProductsPermissions"
				ModifyChecker(sender, cblProductsPermissions)
			Case "rblPurchaseOrdersPermissions"
				ModifyChecker(sender, cblPurchaseOrdersPermissions)
			Case "rblReceiptPageInterfacesPermissions"
				ModifyChecker(sender, cblReceiptPageInterfacesPermissions)
			Case "rblReportsPermissions"
				ModifyChecker(sender, cblReportsPermissions)
			Case "rblStagedOrdersPermissions"
				ModifyChecker(sender, cblStagedOrdersPermissions)
			Case "rblTanksPermissions"
				ModifyChecker(sender, cblTanksPermissions)
			Case "rblTransportsPermissions"
				ModifyChecker(sender, cblTransportsPermissions)
			Case "rblUnitsPermissions"
				ModifyChecker(sender, cblUnitsPermissions)
			Case "rblUsersPermissions"
				ModifyChecker(sender, cblUsersPermissions)
		End Select
	End Sub
	Private Sub SetControlUsabilityFromPermissions()
		With _currentUserPermission(KaUser.TABLE_NAME)
			pnlEven.Enabled = (.Edit AndAlso ddlUserProfiles.SelectedIndex > 0) OrElse (.Create AndAlso ddlUserProfiles.SelectedIndex = 0)
			pnlOdd.Enabled = pnlEven.Enabled
			btnSave.Enabled = pnlEven.Enabled
		End With
	End Sub

	Private Sub cblPermissions_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles cblApplicatorsPermissions.SelectedIndexChanged, cblBranchesPermissions.SelectedIndexChanged, cblCarriersPermissions.SelectedIndexChanged, cblContainersPermissions.SelectedIndexChanged, cblCropsPermissions.SelectedIndexChanged, cblCustomerAccountsPermissions.SelectedIndexChanged, cblCustomPagesPermissions.SelectedIndexChanged, cblDriversPermissions.SelectedIndexChanged, cblEmailsPermissions.SelectedIndexChanged, cblFacilitiesPermissions.SelectedIndexChanged, cblGeneralSettingsPermissions.SelectedIndexChanged, cblInterfacesPermissions.SelectedIndexChanged, cblInventoryPermissions.SelectedIndexChanged, cblOrderPageInterfacesPermissions.SelectedIndexChanged, cblOrderPermissions.SelectedIndexChanged, cblOwnerPermissions.SelectedIndexChanged, cblPanelBulkPermissions.SelectedIndexChanged, cblPanelsPermissions.SelectedIndexChanged, cblProductsPermissions.SelectedIndexChanged, cblPurchaseOrdersPermissions.SelectedIndexChanged, cblReceiptPageInterfacesPermissions.SelectedIndexChanged, cblReportsPermissions.SelectedIndexChanged, cblStagedOrdersPermissions.SelectedIndexChanged, cblTanksPermissions.SelectedIndexChanged, cblTransportsPermissions.SelectedIndexChanged, cblUnitsPermissions.SelectedIndexChanged, cblUsersPermissions.SelectedIndexChanged
		With CType(sender, CheckBoxList)
			If .Items(EDIT_OPTION_INDEX).Selected Then
				.Items(DELETE_OPTION_INDEX).Enabled = True
			Else
				.Items(DELETE_OPTION_INDEX).Enabled = False
				.Items(DELETE_OPTION_INDEX).Selected = False
			End If
		End With
		cblSelectAll.Items(DELETE_OPTION_INDEX).Enabled = cblSelectAll.Items(EDIT_OPTION_INDEX).Enabled
	End Sub
	Protected Sub ScriptManager1_AsyncPostBackError(ByVal sender As Object, ByVal e As System.Web.UI.AsyncPostBackErrorEventArgs)
		ToolkitScriptManager1.AsyncPostBackErrorMessage = e.Exception.Message
	End Sub
End Class