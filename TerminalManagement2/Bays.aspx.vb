Imports KahlerAutomation.KaTm2Database

Public Class Bays : Inherits System.Web.UI.Page

    Private _currentUser As KaUser
Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
    Private _currentTableName As String = KaLocation.TABLE_NAME
    Private _customFields As New List(Of KaCustomField)
    Private _customFieldData As New List(Of KaCustomFieldData)

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Response.Cache.SetCacheability(HttpCacheability.NoCache)
        _currentUser = Utilities.GetUser(Me)
        _currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Facilities")
        If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")

        If Not Page.IsPostBack Then
            SetTextboxMaxLengths()
            PopulateBays()
            PopulateFacilities()
            _customFields.Clear()
            For Each customField As KaCustomField In KaCustomField.GetAll(GetUserConnection(_currentUser.Id), String.Format("deleted=0 AND {0} = {1}", KaCustomField.FN_TABLE_NAME, Q(KaBay.TABLE_NAME)), KaCustomField.FN_FIELD_NAME)
                _customFields.Add(customField)
            Next
            Utilities.ConfirmBox(Me.btnDelete, "Are you sure you want to delete this bay?")
            If Page.Request("BayId") IsNot Nothing Then
                Try
                    ddlBays.SelectedValue = Page.Request("BayId")
                Catch ex As ArgumentOutOfRangeException

                End Try
            End If
            SetControlUsabilityFromPermissions()
            ddlBays_SelectedIndexChanged(ddlBays, New EventArgs)
            Utilities.SetFocus(tbxName, Me)
        End If
    End Sub

    Private Sub PopulateBays()
        ddlBays.Items.Clear()
        Dim li As ListItem = New ListItem
        If _currentUserPermission(_currentTableName).Create Then li.Text = "Enter a new bay" Else li.Text = "Select a bay"
        li.Value = Guid.Empty.ToString
        ddlBays.Items.Add(li)
        Dim allbays As ArrayList = KaBay.GetAll(GetUserConnection(_currentUser.Id), "deleted = " & Q(False), "name asc")
        For Each bay As KaBay In allbays
            li = New ListItem
            li.Text = bay.Name
            li.Value = bay.Id.ToString
            ddlBays.Items.Add(li)
        Next
        ddlBays_SelectedIndexChanged(ddlBays, New EventArgs)
    End Sub

    Private Sub PopulateFacilities()
        ddlFacilities.Items.Clear()
        Dim li As ListItem = New ListItem
        li.Text = ""
        li.Value = Guid.Empty.ToString
        ddlFacilities.Items.Add(li)
        Dim allFacilities As ArrayList = KaLocation.GetAll(GetUserConnection(_currentUser.Id), "deleted = " & Q(False), "name asc")
        For Each facility As KaLocation In allFacilities
            li = New ListItem
            li.Text = facility.Name
            li.Value = facility.Id.ToString
            ddlFacilities.Items.Add(li)
        Next
    End Sub

    Private Sub ddlBays_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlBays.SelectedIndexChanged
        lblStatus.Text = ""
        SetControlUsabilityFromPermissions()
        Dim facilityId As Guid = Guid.Parse(ddlBays.SelectedValue)
        If facilityId <> Guid.Empty Then
            btnDelete.Enabled = _currentUserPermission(_currentTableName).Delete
        Else
            btnDelete.Enabled = False
            Utilities.SetFocus(tbxName, Me)
        End If
        PopulateBayData()
    End Sub

    Private Sub PopulateBayData()
        ClearBayData()
        If Guid.Parse(ddlBays.SelectedValue) <> Guid.Empty Then
            Dim bay As KaBay = New KaBay(GetUserConnection(_currentUser.Id), Guid.Parse(ddlBays.SelectedValue))
            tbxName.Text = bay.Name
            ddlFacilities.SelectedValue = bay.LocationId.ToString
            cbxStagedOrdersReturnToScale.Checked = bay.StagedOrdersReturnToScale

            For Each customFieldValue As KaCustomFieldData In KaCustomFieldData.GetAll(GetUserConnection(_currentUser.Id), String.Format("deleted = 0 AND {0} = {1}", KaCustomFieldData.FN_RECORD_ID, Q(bay.Id)), KaCustomFieldData.FN_LAST_UPDATED)
                _customFieldData.Add(customFieldValue)
            Next
        End If
		Utilities.CreateDynamicCustomFieldPanelControls(_customFields, _customFieldData, lstCustomFields, Page)
	End Sub

	Private Sub ClearBayData()
		tbxName.Text = ""
		ddlFacilities.SelectedIndex = 0
		_customFieldData.Clear()
	End Sub

	Private Sub btnSave_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnSave.Click
		If (ValidateFields()) Then
			Dim bay As KaBay = New KaBay
			bay.Name = tbxName.Text.Trim
			bay.LocationId = Guid.Parse(ddlFacilities.SelectedValue)
			bay.StagedOrdersReturnToScale = cbxStagedOrdersReturnToScale.Checked
			Dim status As String = ""
			If ddlBays.SelectedIndex > 0 Then
				bay.Id = Guid.Parse(ddlBays.SelectedValue)
				bay.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
				status = "Selected Bay updated successfully"
			Else
				bay.SqlInsert(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
				status = "New Bay added successfully"
			End If

			Utilities.ConvertCustomFieldPanelToLists(_customFields, _customFieldData, lstCustomFields)
			For Each customData As KaCustomFieldData In _customFieldData
				customData.RecordId = bay.Id
				customData.SqlUpdateInsertIfNotFound(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
			Next

			PopulateBays()
			ddlBays.SelectedValue = bay.Id.ToString
			ddlBays_SelectedIndexChanged(ddlBays, New EventArgs)
			lblStatus.Text = status
		End If
	End Sub

	Private Function ValidateFields() As Boolean
		If tbxName.Text.Trim = "" Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidName", Utilities.JsAlert("Please specify a name for this bay.")) : Return False
		If ddlFacilities.SelectedIndex = 0 Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidFacility", Utilities.JsAlert("Please select a facility for this bay.")) : Return False
		Dim conditions As String = String.Format("deleted=0 AND name={0} AND id<>{1}", Q(tbxName.Text), Q(ddlBays.SelectedValue))
		If KaBay.GetAll(GetUserConnection(_currentUser.Id), conditions, "").Count > 0 Then
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidDuplicateName", Utilities.JsAlert("A bay with the name """ & tbxName.Text & """ already exists. Please specify a unique name for this bay.")) : Return False
		End If
		Return True
	End Function

	Private Sub btnDelete_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnDelete.Click
		lblStatus.Text = ""
		If Guid.Parse(ddlBays.SelectedValue) <> Guid.Empty Then
			With New KaBay(GetUserConnection(_currentUser.Id), Guid.Parse(ddlBays.SelectedValue))
				.Deleted = True
				.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
				ddlBays.SelectedIndex = 0
				ClearBayData()
				lblStatus.Text = "Selected Bay deleted successfully"
				SetControlUsabilityFromPermissions()
				PopulateBays()
				ddlBays.SelectedIndex = 0
			End With
		End If
	End Sub

	Private Sub SetTextboxMaxLengths()
		tbxName.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaBay.TABLE_NAME, "name"))
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
            Dim shouldEnable = (.Edit AndAlso ddlBays.SelectedIndex > 0) OrElse (.Create AndAlso ddlBays.SelectedIndex = 0)
            tbxName.Enabled = shouldEnable
            ddlFacilities.Enabled = shouldEnable
            cbxStagedOrdersReturnToScale.Enabled = shouldEnable
            btnSave.Enabled = shouldEnable
            btnDelete.Enabled = Not Guid.Parse(ddlBays.SelectedValue).Equals(Guid.Empty) AndAlso .Edit AndAlso .Delete
        End With
    End Sub
End Class