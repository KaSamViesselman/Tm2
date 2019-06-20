Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class Branches
	Inherits System.Web.UI.Page

	Private _currentUser As KaUser
	Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
	Private _currentTableName As String = KaBranch.TABLE_NAME
	Private _customFields As New List(Of KaCustomField)
	Private _customFieldData As New List(Of KaCustomFieldData)

#Region " Web Form Designer Generated Code "

	'This call is required by the Web Form Designer.
	<System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()

	End Sub


	Private Sub Page_Init(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Init
		'CODEGEN: This method call is required by the Web Form Designer
		'Do not modify it using the code editor.
		InitializeComponent()
	End Sub

#End Region

#Region "Events"
	Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
		Response.Cache.SetCacheability(HttpCacheability.NoCache)
		_currentUser = Utilities.GetUser(Me)
		_currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Branches")
		If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")
        lblStatus.Text = ""

        If Not Page.IsPostBack Then
			SetTextboxMaxLengths()
			_customFields.Clear()
			For Each customField As KaCustomField In KaCustomField.GetAll(GetUserConnection(_currentUser.Id), String.Format("deleted=0 AND {0} = {1}", KaCustomField.FN_TABLE_NAME, Q(KaBranch.TABLE_NAME)), KaCustomField.FN_FIELD_NAME)
				_customFields.Add(customField)
			Next
			Utilities.ConfirmBox(Me.btnDelete, "Are you sure you want to delete this branch?")
			Utilities.SetFocus(tbxName, Me)
			PopulateBranchesList()
			If Page.Request("BranchId") IsNot Nothing Then
				Try
					ddlBranches.SelectedValue = Page.Request("BranchId")
				Catch ex As ArgumentOutOfRangeException

				End Try
			End If
			ddlBranches_SelectedIndexChanged(ddlBranches, New EventArgs())
		End If
	End Sub

	Private Sub ddlBranches_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ddlBranches.SelectedIndexChanged
        Dim branchId As Guid = Guid.Parse(ddlBranches.SelectedValue)
        PopulateBranchData(branchId)
		PopulateBranchInterfaceList(branchId)
		PopulateInterfaceInformation(Guid.Empty)
		btnDelete.Enabled = branchId <> Guid.Empty
		SetControlUsabilityFromPermissions()
		Utilities.SetFocus(tbxName, Me)
	End Sub

	Private Sub btnSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSave.Click
		SaveBranch()
	End Sub

    Protected Sub btnDelete_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnDelete.Click
        Dim branchId As Guid = Guid.Empty
        If Guid.TryParse(ddlBranches.SelectedValue, branchId) Then
            Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
        Dim orders As ArrayList = KaOrder.GetAll(GetUserConnection(_currentUser.Id), $"{KaOrder.FN_DELETED} = 0 AND {KaOrder.FN_COMPLETED} = 0 AND {KaOrder.FN_BRANCH_ID} = {Q(branchId)}", "number ASC")
            If orders.Count > 0 Then
                Dim warning As String = "Branch is associated with other records (see below for details). Branch information not deleted.\n\nDetails:\n\nOrders:\n"
                Dim lastOrderId As Guid = Guid.Empty
                For Each order As KaOrder In orders
                    warning &= New KaOrder(GetUserConnection(_currentUser.Id), order.Id).Number & " "
                    lastOrderId = order.Id
                Next
                ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidProductUsed", Utilities.JsAlert(warning))
            Else
                With New KaBranch(GetUserConnection(_currentUser.Id), branchId)
                    .Deleted = True
                    .SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                End With
                PopulateBranchData(Guid.Empty)
                PopulateBranchesList()
                lblStatus.Text = "Selected branch deleted successfully"
                btnDelete.Enabled = False
            End If
        End If
    End Sub
#End Region

    Private Sub PopulateBranchesList()
		ddlBranches.Items.Clear() ' populate the branches list
		If _currentUserPermission(_currentTableName).Create Then ddlBranches.Items.Add(New ListItem("Enter a new branch", Guid.Empty.ToString())) Else ddlBranches.Items.Add(New ListItem("Select a branch", Guid.Empty.ToString()))
		For Each r As KaBranch In KaBranch.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
			ddlBranches.Items.Add(New ListItem(r.Name, r.Id.ToString()))
		Next
	End Sub

	Private Sub PopulateBranchData(ByVal branchId As Guid)
		_customFieldData.Clear()
		Dim r As New KaBranch()
		r.Id = branchId
		If r.Id <> Guid.Empty Then
			r.SqlSelect(GetUserConnection(_currentUser.Id))
			For Each customFieldValue As KaCustomFieldData In KaCustomFieldData.GetAll(GetUserConnection(_currentUser.Id), String.Format("deleted = 0 AND {0} = {1}", KaCustomFieldData.FN_RECORD_ID, Q(r.Id)), KaCustomFieldData.FN_LAST_UPDATED)
				_customFieldData.Add(customFieldValue)
			Next
		End If
		tbxName.Text = r.Name
		tbxBranchNumber.Text = r.Number
		tbxAddress.Text = r.Street
		tbxCity.Text = r.City
		tbxState.Text = r.State
		tbxZip.Text = r.ZipCode
		tbxCountry.Text = r.Country
		tbxPhone.Text = r.Phone
		tbxEmail.Text = r.Email
		Utilities.CreateDynamicCustomFieldPanelControls(_customFields, _customFieldData, lstCustomFields, Page)
	End Sub

	Private Sub SaveBranch()
		If ValidateFields() Then
			With New KaBranch()
				.Id = Guid.Parse(ddlBranches.SelectedValue)
				.Name = tbxName.Text.Trim
				.Number = tbxBranchNumber.Text.Trim
				.Street = tbxAddress.Text.Trim
				.City = tbxCity.Text.Trim
				.State = tbxState.Text.Trim
				.ZipCode = tbxZip.Text.Trim
				.Country = tbxCountry.Text.Trim
				.Phone = tbxPhone.Text.Trim
				.Email = tbxEmail.Text.Trim
				Dim statusText As String
				If .Id = Guid.Empty Then
					.SqlInsert(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
					statusText = "New branch added successfully"
				Else
					.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
					statusText = "Selected branch updated successfully"
				End If

				Utilities.ConvertCustomFieldPanelToLists(_customFields, _customFieldData, lstCustomFields)
				For Each customData As KaCustomFieldData In _customFieldData
					customData.RecordId = .Id
					customData.SqlUpdateInsertIfNotFound(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
				Next
				PopulateBranchesList()
				ddlBranches.SelectedValue = .Id.ToString()
				ddlBranches_SelectedIndexChanged(ddlBranches, New EventArgs())
				lblStatus.Text = statusText
			End With
			btnDelete.Enabled = _currentUserPermission(_currentTableName).Delete
		End If
	End Sub

	Private Function ValidateFields() As Boolean
		Dim retval As Boolean = True
		If tbxName.Text.Trim().Length = 0 Then
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidName", Utilities.JsAlert("Please specify a name."))
			Return False
		End If
		If KaBranch.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND id<>" & Q(Guid.Parse(ddlBranches.SelectedValue)) & " AND name=" & Q(tbxName.Text), "").Count > 0 Then
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidNameExists", Utilities.JsAlert("A branch with the name """ & tbxName.Text & """ already exists. Please specify a unique name for this branch."))
			Return False
		End If
		Dim message As String = ""
		If Not Utilities.IsEmailFieldValid(tbxEmail.Text, message) Then
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidEmail", Utilities.JsAlert(message))
			Return False
		End If
		Return True
	End Function

#Region "Interfaces"
	Private Sub PopulateInterfaceList()
		ddlInterface.Items.Clear()
		ddlInterface.Items.Add(New ListItem("", Guid.Empty.ToString()))
		Dim getInterfaceRdr As OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), "SELECT interfaces.id, interfaces.name " &
				"FROM interfaces " &
				"INNER JOIN interface_types ON interfaces.interface_type_id = interface_types.id " &
				"WHERE (interfaces.deleted = 0) " &
					"AND (interface_types.deleted = 0) " &
					"AND ((" & KaInterfaceTypes.FN_SHOW_BRANCH_INTERFACE & " = 1) " &
					"OR (interfaces.id IN (SELECT " & KaBranchInterfaceSettings.TABLE_NAME & ".interface_id " &
							"FROM " & KaBranchInterfaceSettings.TABLE_NAME & " " &
							"WHERE (deleted=0) " &
								"AND (" & KaBranchInterfaceSettings.TABLE_NAME & "." & KaBranchInterfaceSettings.FN_BRANCH_ID & " = " & Q(ddlBranches.SelectedValue) & ") " &
								"AND (" & KaBranchInterfaceSettings.TABLE_NAME & "." & KaBranchInterfaceSettings.FN_BRANCH_ID & " <> " & Q(Guid.Empty) & ")))) " &
				"ORDER BY interfaces.name")
		Do While getInterfaceRdr.Read
			ddlInterface.Items.Add(New ListItem(getInterfaceRdr.Item("name"), getInterfaceRdr.Item("id").ToString()))
		Loop
		getInterfaceRdr.Close()
		pnlInterfaceSetup.Visible = (ddlInterface.Items.Count > 1) AndAlso ddlBranches.SelectedValue <> Guid.Empty.ToString
	End Sub

	Private Sub SaveInterface()
        If Guid.Parse(ddlBranches.SelectedValue) = Guid.Empty Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidBranchNotSaved", Utilities.JsAlert("You must save the Branch before you can set up interface cross references.")) : Exit Sub
        If Guid.Parse(ddlInterface.SelectedValue) = Guid.Empty Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidInterface", Utilities.JsAlert("An interface must be selected. Interface settings not saved.")) : Exit Sub
        If tbxInterfaceCrossReference.Text.Length = 0 Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidCrossReference", Utilities.JsAlert("A cross reference must be specified. Interface settings not saved.")) : Exit Sub

        ' If this is not export only, check if there are any other settings with the same cross reference ID
        If Not chkExportOnly.Checked Then
            Dim allInterfaceSettings As ArrayList = KaBranchInterfaceSettings.GetAll(GetUserConnection(_currentUser.Id), KaBranchInterfaceSettings.FN_CROSS_REFERENCE & " = " & Q(tbxInterfaceCrossReference.Text.Trim) &
                                                                                            " AND " & KaBranchInterfaceSettings.FN_INTERFACE_ID & " = " & Q(ddlInterface.SelectedValue) &
                                                                                            " AND " & KaBranchInterfaceSettings.FN_DELETED & " = 0 " &
                                                                                            " AND " & KaBranchInterfaceSettings.FN_EXPORT_ONLY & " = 0 " &
                                                                                            " AND " & KaBranchInterfaceSettings.FN_ID & " <> " & Q(ddlBranchInterface.SelectedValue), "")
            If allInterfaceSettings.Count > 0 Then
                ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidDuplicateCrossReference", Utilities.JsAlert("A cross reference of " & tbxInterfaceCrossReference.Text.Trim & " already exists for this interface."))
                Exit Sub
            End If
        End If

        Dim branchInterfaceId As Guid = Guid.Parse(ddlBranchInterface.SelectedValue)
        If branchInterfaceId = Guid.Empty Then
            Dim branchInterface As KaBranchInterfaceSettings = New KaBranchInterfaceSettings
            branchInterface.BranchId = Guid.Parse(ddlBranches.SelectedValue)
            branchInterface.InterfaceId = Guid.Parse(ddlInterface.SelectedValue)
            branchInterface.CrossReference = tbxInterfaceCrossReference.Text.Trim
            branchInterface.DefaultSetting = chkDefaultSetting.Checked
            branchInterface.ExportOnly = chkExportOnly.Checked
            branchInterface.SqlInsert(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
            branchInterfaceId = branchInterface.Id
        Else
            Dim branchInterface As KaBranchInterfaceSettings = New KaBranchInterfaceSettings(GetUserConnection(_currentUser.Id), branchInterfaceId)
            branchInterface.BranchId = Guid.Parse(ddlBranches.SelectedValue)
            branchInterface.InterfaceId = Guid.Parse(ddlInterface.SelectedValue)
            branchInterface.CrossReference = tbxInterfaceCrossReference.Text.Trim
            branchInterface.DefaultSetting = chkDefaultSetting.Checked
            branchInterface.ExportOnly = chkExportOnly.Checked
            branchInterface.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
        End If

        PopulateBranchInterfaceList(Guid.Parse(ddlBranches.SelectedValue))
        ddlBranchInterface.SelectedValue = branchInterfaceId.ToString
        ddlBranchInterface_SelectedIndexChanged(ddlBranchInterface, New EventArgs)
        btnRemoveInterface.Enabled = True
        End Sub

    Private Sub RemoveInterface()
		Dim selectedId As Guid = Guid.Parse(ddlBranchInterface.SelectedValue)
		If selectedId <> Guid.Empty Then
			Dim prodInterfaceSetting As KaBranchInterfaceSettings = New KaBranchInterfaceSettings(GetUserConnection(_currentUser.Id), selectedId)
			prodInterfaceSetting.Deleted = True
			prodInterfaceSetting.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
		End If
		PopulateBranchInterfaceList(Guid.Parse(ddlBranches.SelectedValue))
		btnRemoveInterface.Enabled = PopulateInterfaceInformation(Guid.Empty)
	End Sub

	Private Sub DeleteInterfaceInformation(ByVal branchId As Guid)
		For Each r As KaBranchInterfaceSettings In KaBranchInterfaceSettings.GetAll(GetUserConnection(_currentUser.Id), KaBranchInterfaceSettings.FN_DELETED & " = 0 and " & KaBranchInterfaceSettings.FN_BRANCH_ID & " = " & Q(branchId), "")
			r.Deleted = True
			r.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
		Next
	End Sub

	Private Sub PopulateBranchInterfaceList(ByVal branchId As Guid)
		PopulateInterfaceList()
		ddlBranchInterface.Items.Clear()
		ddlBranchInterface.Items.Add(New ListItem(IIf(ddlInterface.Items.Count > 1, "Add an interface", ""), Guid.Empty.ToString))
		Dim getInterfaceRdr As OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), "SELECT " & KaBranchInterfaceSettings.TABLE_NAME & ".id, interfaces.name, " & KaBranchInterfaceSettings.TABLE_NAME & ".cross_reference " &
				"FROM " & KaBranchInterfaceSettings.TABLE_NAME & " " &
				"INNER JOIN interfaces ON " & KaBranchInterfaceSettings.TABLE_NAME & ".interface_id = interfaces.id " &
				"INNER JOIN interface_types ON interfaces.interface_type_id = interface_types.id " &
				"WHERE (" & KaBranchInterfaceSettings.TABLE_NAME & ".deleted = 0) " &
					"AND (interfaces.deleted = 0) " &
					"AND (interface_types.deleted = 0) " &
					"AND (" & KaBranchInterfaceSettings.TABLE_NAME & "." & KaBranchInterfaceSettings.FN_BRANCH_ID & "=" & Q(branchId) & ") " &
				"ORDER BY interfaces.name, " & KaBranchInterfaceSettings.TABLE_NAME & ".cross_reference")
		Do While getInterfaceRdr.Read
			ddlBranchInterface.Items.Add(New ListItem(getInterfaceRdr.Item("name") & " (" & getInterfaceRdr.Item("cross_reference") & ")", getInterfaceRdr.Item("id").ToString()))
		Loop
		getInterfaceRdr.Close()
	End Sub

	Private Function PopulateInterfaceInformation(ByVal branchInterfaceId As Guid) As Boolean
		Dim retval As Boolean = False
		If branchInterfaceId <> Guid.Empty Then
			Dim branchInterfaceSetting As KaBranchInterfaceSettings = New KaBranchInterfaceSettings(GetUserConnection(_currentUser.Id), branchInterfaceId)
			ddlInterface.SelectedValue = branchInterfaceSetting.InterfaceId.ToString
			tbxInterfaceCrossReference.Text = branchInterfaceSetting.CrossReference
			chkDefaultSetting.Checked = branchInterfaceSetting.DefaultSetting
			chkExportOnly.Checked = branchInterfaceSetting.ExportOnly
			retval = True
		Else
			ddlInterface.SelectedIndex = 0
			tbxInterfaceCrossReference.Text = ""
			ddlInterface_SelectedIndexChanged(ddlInterface, New EventArgs)
			chkExportOnly.Checked = False
			retval = False
		End If

		Return retval
	End Function

	Protected Sub btnRemoveInterface_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnRemoveInterface.Click
		RemoveInterface()
	End Sub

	Protected Sub btnSaveInterface_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnSaveInterface.Click
		SaveInterface()
	End Sub

	Protected Sub ddlBranchInterface_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles ddlBranchInterface.SelectedIndexChanged
		btnRemoveInterface.Enabled = PopulateInterfaceInformation(Guid.Parse(ddlBranchInterface.SelectedValue))
	End Sub
#End Region

	Private Sub SetTextboxMaxLengths()
		tbxAddress.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaBranch.TABLE_NAME, "street"))
		tbxBranchNumber.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaBranch.TABLE_NAME, "branch_number"))
		tbxCity.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaBranch.TABLE_NAME, "city"))
		tbxInterfaceCrossReference.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaBranchInterfaceSettings.TABLE_NAME, KaBranchInterfaceSettings.FN_CROSS_REFERENCE))
		tbxEmail.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaBranch.TABLE_NAME, "email"))
		tbxName.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaBranch.TABLE_NAME, "name"))
		tbxPhone.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaBranch.TABLE_NAME, "phone"))
		tbxState.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaBranch.TABLE_NAME, "state"))
		tbxZip.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaBranch.TABLE_NAME, "zip_code"))
		tbxCountry.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaBranch.TABLE_NAME, "country"))
	End Sub

	Private Sub ddlInterface_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles ddlInterface.SelectedIndexChanged
		If Guid.Parse(ddlBranchInterface.SelectedValue) = Guid.Empty Then
			'Only do this check if we are a new interface setting
			Dim count As Integer = 0
			Try
				Dim rdr As OleDb.OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), "SELECT count(*) " &
																			 "FROM " & KaBranchInterfaceSettings.TABLE_NAME & " " &
																			 "WHERE " & KaBranchInterfaceSettings.FN_DELETED & " = 0 " &
																			 "AND " & KaBranchInterfaceSettings.FN_INTERFACE_ID & " = " & Q(Guid.Parse(ddlInterface.SelectedValue)) & " " &
																			 "AND " & KaBranchInterfaceSettings.FN_BRANCH_ID & " = " & Q(Guid.Parse(ddlBranches.SelectedValue)))
				If rdr.Read Then count = rdr.Item(0)
			Catch ex As Exception

			End Try
			chkDefaultSetting.Checked = (count = 0)
		End If
	End Sub

	Protected Overrides Function SaveViewState() As Object
		Dim viewState(2) As Object
		'Saving the grid values to the View State
		Utilities.ConvertCustomFieldPanelToLists(_customFields, _customFieldData, lstCustomFields)
		viewState(0) = MyBase.SaveViewState()
		viewState(1) = _customFields
		viewState(2) = _customFieldData
		Return viewState
	End Function

	Protected Overrides Sub LoadViewState(savedState As Object)
		'Getting the dropdown list value from view state.
		If savedState IsNot Nothing AndAlso CType(savedState, Object).Length = 3 Then
			Dim viewState As Object() = savedState
			MyBase.LoadViewState(viewState(0))
			_customFields = viewState(1)
			_customFieldData = viewState(2)
			Utilities.CreateDynamicCustomFieldPanelControls(_customFields, _customFieldData, lstCustomFields, Page)
		Else
			MyBase.LoadViewState(savedState)
		End If
	End Sub
	Private Sub SetControlUsabilityFromPermissions()
		With _currentUserPermission(_currentTableName)
			Dim shouldEnable = (.Edit AndAlso ddlBranches.SelectedIndex > 0) OrElse (.Create AndAlso ddlBranches.SelectedIndex = 0)
			pnlEven.Enabled = shouldEnable
			pnlInterfaceSettings.Enabled = shouldEnable
			btnSave.Enabled = shouldEnable
			btnDelete.Enabled = Not Guid.Parse(ddlBranches.SelectedValue).Equals(Guid.Empty) AndAlso .Edit AndAlso .Delete
		End With
	End Sub
End Class