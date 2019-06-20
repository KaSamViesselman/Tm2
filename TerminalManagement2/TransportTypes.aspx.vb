Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class TransportTypes : Inherits System.Web.UI.Page
    Private _currentUser As KaUser
    Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
    Private _customFields As New List(Of KaCustomField)
    Private _customFieldData As New List(Of KaCustomFieldData)

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Response.Cache.SetCacheability(HttpCacheability.NoCache)
        _currentUser = Utilities.GetUser(Me)
        _currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaTransport.TABLE_NAME}), "Transports")
        If Not _currentUserPermission(KaTransport.TABLE_NAME).Read Then Response.Redirect("Welcome.aspx")

        If Not Page.IsPostBack Then
            SetTextboxMaxLengths()

            _customFields.Clear()
            For Each customField As KaCustomField In KaCustomField.GetAll(GetUserConnection(_currentUser.Id), String.Format("deleted=0 AND {0} = {1}", KaCustomField.FN_TABLE_NAME, Q(KaTransportTypes.TABLE_NAME)), KaCustomField.FN_FIELD_NAME)
                _customFields.Add(customField)
            Next

            PopulateTransportTypes()
            PopulateInterfaceList()
            If Page.Request("TransportTypeId") IsNot Nothing Then
                Try
                    ddlTransportTypes.SelectedValue = Page.Request("TransportTypeId")
                Catch ex As ArgumentOutOfRangeException
                    ddlTransportTypes.SelectedIndex = 0
                End Try
            End If
            ddlTransportTypes_SelectedIndexChanged(ddlTransportTypes, New EventArgs())
            Utilities.ConfirmBox(Me.btnDelete, "Are you sure you want to delete this transport type?")
            Utilities.SetFocus(tbxName, Me)
        End If
    End Sub

    Private Sub PopulateTransportTypes()
        ddlTransportTypes.Items.Clear()
        Dim li As ListItem = New ListItem
        If _currentUserPermission(KaTransport.TABLE_NAME).Create Then
            li.Text = "Enter a new transport type"
        Else
            li.Text = "Select a transport type"
        End If
        li.Value = Guid.Empty.ToString
        ddlTransportTypes.Items.Add(li)
        For Each transportType As KaTransportTypes In KaTransportTypes.GetAll(GetUserConnection(_currentUser.Id), "deleted = " & Q(False), "name asc")
            li = New ListItem
            li.Text = transportType.Name
            li.Value = transportType.Id.ToString
            ddlTransportTypes.Items.Add(li)
        Next
    End Sub

    Private Sub ddlTransportTypes_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlTransportTypes.SelectedIndexChanged
        ClearFields()
        Dim transportTypeId As Guid = Guid.Parse(ddlTransportTypes.SelectedValue)
        If transportTypeId <> Guid.Empty Then
            Dim transportType As KaTransportTypes = New KaTransportTypes(GetUserConnection(_currentUser.Id), Guid.Parse(ddlTransportTypes.SelectedValue))
            tbxName.Text = transportType.Name
            With _currentUserPermission(KaTransport.TABLE_NAME)
                btnDelete.Enabled = .Edit AndAlso .Delete
            End With
            _customFieldData.Clear()
            For Each customFieldValue As KaCustomFieldData In KaCustomFieldData.GetAll(GetUserConnection(_currentUser.Id), String.Format("deleted = 0 AND {0} = {1}", KaCustomFieldData.FN_RECORD_ID, Q(transportTypeId)), KaCustomFieldData.FN_LAST_UPDATED)
                _customFieldData.Add(customFieldValue)
            Next
        End If
        PopulateTransportTypeInterfaceList(transportTypeId)
        PopulateInterfaceInformation(Guid.Empty)
        SetControlUsabilityFromPermissions()
		Utilities.CreateDynamicCustomFieldPanelControls(_customFields, _customFieldData, lstCustomFields, Page)
	End Sub

	Private Sub ClearFields()
		tbxName.Text = ""
		lblStatus.Text = ""
		btnDelete.Enabled = False
		_customFieldData.Clear()
	End Sub

	Private Sub btnSave_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnSave.Click
		lblStatus.Text = ""
		If ValidateFields() Then
			Dim transportType As New KaTransportTypes
			With transportType
				.Id = Guid.Parse(ddlTransportTypes.SelectedValue)
				.Deleted = False
				.Name = tbxName.Text.Trim
				Dim status As String = ""
				If .Id = Guid.Empty Then
					.SqlInsert(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
					status = "New transport type added successfully"
				Else
					.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
					status = "Selected transport type updated successfully"
				End If

				Utilities.ConvertCustomFieldPanelToLists(_customFields, _customFieldData, lstCustomFields)
				For Each customData As KaCustomFieldData In _customFieldData
					customData.RecordId = .Id
					customData.SqlUpdateInsertIfNotFound(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
				Next

				PopulateTransportTypes()
				ddlTransportTypes.SelectedValue = .Id.ToString
				ddlTransportTypes_SelectedIndexChanged(ddlTransportTypes, New EventArgs())
				lblStatus.Text = status
			End With
		End If
	End Sub

	Private Function ValidateFields() As Boolean
		If tbxName.Text.Trim = "" Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidName", Utilities.JsAlert("You must enter a name.")) : Return False

		Dim allTransportTypes As ArrayList = KaTransportTypes.GetAll(GetUserConnection(_currentUser.Id), "name = " & Q(tbxName.Text.Trim), "")
		If allTransportTypes.Count > 0 Then
			Dim tempTransportTypes As KaTransportTypes = allTransportTypes.Item(0)
			If tempTransportTypes.Id <> Guid.Parse(ddlTransportTypes.SelectedValue) Then
				ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidDuplicateName", Utilities.JsAlert("A Transport Type with name " & tbxName.Text.Trim & " already exists.  Transport Type name must be unique.")) : Return False
			End If
		End If
		Return True
	End Function

	Protected Sub btnDelete_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnDelete.Click
		lblStatus.Text = ""
		If Guid.Parse(ddlTransportTypes.SelectedValue) <> Guid.Empty Then
			With New KaTransportTypes(GetUserConnection(_currentUser.Id), Guid.Parse(ddlTransportTypes.SelectedValue))
				.Deleted = True
				.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
				lblStatus.Text = "Selected Transport Type deleted successfully"
				PopulateTransportTypes()
				ClearFields()
				ddlTransportTypes.SelectedIndex = 0
				btnSave.Enabled = True
				btnDelete.Enabled = False
			End With
		End If
	End Sub

	Private Sub SetTextboxMaxLengths()
		tbxName.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaTransportTypes.TABLE_NAME, "name"))
		tbxInterfaceCrossReference.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaTransportTypeInterfaceSettings.TABLE_NAME, KaTransportTypeInterfaceSettings.FN_CROSS_REFERENCE))
	End Sub

#Region "Interfaces"
	Private Sub PopulateInterfaceList()
		ddlInterface.Items.Clear()
		ddlInterface.Items.Add(New ListItem("", Guid.Empty.ToString()))
		Dim getInterfaceRdr As OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), "SELECT interfaces.id, interfaces.name " &
			   "FROM interfaces " &
			   "INNER JOIN interface_types ON interfaces.interface_type_id = interface_types.id " &
			   "WHERE (interfaces.deleted = 0) " &
				   "AND (interface_types.deleted = 0) " &
				   "AND ((" & KaInterfaceTypes.FN_SHOW_TRANSPORT_TYPE_INTERFACE & " = 1) " &
				   "OR (interfaces.id IN (SELECT " & KaTransportTypeInterfaceSettings.TABLE_NAME & ".interface_id " &
										   "FROM " & KaTransportTypeInterfaceSettings.TABLE_NAME & " " &
										   "WHERE (deleted=0) " &
											   "AND (" & KaTransportTypeInterfaceSettings.TABLE_NAME & "." & KaTransportTypeInterfaceSettings.FN_TRANSPORT_TYPE_ID & " = " & Q(ddlTransportTypes.SelectedValue) & ") " &
											   "AND (" & KaTransportTypeInterfaceSettings.TABLE_NAME & "." & KaTransportTypeInterfaceSettings.FN_TRANSPORT_TYPE_ID & " <> " & Q(Guid.Empty) & ")))) " &
			   "ORDER BY interfaces.name")
		Do While getInterfaceRdr.Read
			ddlInterface.Items.Add(New ListItem(getInterfaceRdr.Item("name"), getInterfaceRdr.Item("id").ToString()))
		Loop
		getInterfaceRdr.Close()
		pnlInterfaceSetup.Visible = (ddlInterface.Items.Count > 1) AndAlso ddlTransportTypes.SelectedValue <> Guid.Empty.ToString
	End Sub

	Private Sub SaveInterface()
		lblStatus.Text = ""
		If Guid.Parse(ddlTransportTypes.SelectedValue) = Guid.Empty Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidNotSaved", Utilities.JsAlert("You must save the Transport Type before you can set up interface cross references.")) : Exit Sub
		If Guid.Parse(ddlInterface.SelectedValue) = Guid.Empty Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidInterface", Utilities.JsAlert("An interface must be selected. Interface settings not saved.")) : Exit Sub
		If tbxInterfaceCrossReference.Text.Length = 0 Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidCrossReference", Utilities.JsAlert("A cross reference must be specified. Interface settings not saved.")) : Exit Sub

		' If this is not export only, check if there are any other settings with the same cross reference ID
		If Not chkExportOnly.Checked Then
			Dim allInterfaceSettings As ArrayList = KaTransportTypeInterfaceSettings.GetAll(GetUserConnection(_currentUser.Id), KaTransportTypeInterfaceSettings.FN_CROSS_REFERENCE & " = " & Q(tbxInterfaceCrossReference.Text.Trim) &
																							" AND " & KaTransportTypeInterfaceSettings.FN_INTERFACE_ID & " = " & Q(ddlInterface.SelectedValue) &
																							" AND " & KaTransportTypeInterfaceSettings.FN_DELETED & " = 0 " &
																							" AND " & KaTransportTypeInterfaceSettings.FN_EXPORT_ONLY & " = 0 " &
																							" AND " & KaTransportTypeInterfaceSettings.FN_ID & " <> " & Q(ddlTransportTypeInterface.SelectedValue), "")
			If allInterfaceSettings.Count > 0 Then
				ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidCrossReferenceExists", Utilities.JsAlert("A cross reference of " & tbxInterfaceCrossReference.Text.Trim & " already exists for this interface."))
				Exit Sub
			End If
		End If

		Dim transportTypeInterfaceId As Guid = Guid.Parse(ddlTransportTypeInterface.SelectedValue)
		If transportTypeInterfaceId = Guid.Empty Then
			Dim transportTypeInterface As KaTransportTypeInterfaceSettings = New KaTransportTypeInterfaceSettings
			transportTypeInterface.TransportTypeId = Guid.Parse(ddlTransportTypes.SelectedValue)
			transportTypeInterface.InterfaceId = Guid.Parse(ddlInterface.SelectedValue)
			transportTypeInterface.CrossReference = tbxInterfaceCrossReference.Text.Trim
			transportTypeInterface.DefaultSetting = chkDefaultSetting.Checked
			transportTypeInterface.ExportOnly = chkExportOnly.Checked
			transportTypeInterface.SqlInsert(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
			transportTypeInterfaceId = transportTypeInterface.Id
		Else
			Dim transportTypeInterface As KaTransportTypeInterfaceSettings = New KaTransportTypeInterfaceSettings(GetUserConnection(_currentUser.Id), transportTypeInterfaceId)
			transportTypeInterface.TransportTypeId = Guid.Parse(ddlTransportTypes.SelectedValue)
			transportTypeInterface.InterfaceId = Guid.Parse(ddlInterface.SelectedValue)
			transportTypeInterface.CrossReference = tbxInterfaceCrossReference.Text.Trim
			transportTypeInterface.DefaultSetting = chkDefaultSetting.Checked
			transportTypeInterface.ExportOnly = chkExportOnly.Checked
			transportTypeInterface.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
		End If

		PopulateTransportTypeInterfaceList(Guid.Parse(ddlTransportTypes.SelectedValue))
		ddlTransportTypeInterface.SelectedValue = transportTypeInterfaceId.ToString
		ddlTransportTypeInterface_SelectedIndexChanged(ddlTransportTypeInterface, New EventArgs)
		btnRemoveInterface.Enabled = True
	End Sub

	Private Sub RemoveInterface()
		Dim selectedId As Guid = Guid.Parse(ddlTransportTypeInterface.SelectedValue)
		If selectedId <> Guid.Empty Then
			Dim prodInterfaceSetting As KaTransportTypeInterfaceSettings = New KaTransportTypeInterfaceSettings(GetUserConnection(_currentUser.Id), selectedId)
			prodInterfaceSetting.Deleted = True
			prodInterfaceSetting.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
		End If
		PopulateTransportTypeInterfaceList(Guid.Parse(ddlTransportTypes.SelectedValue))
		btnRemoveInterface.Enabled = PopulateInterfaceInformation(Guid.Empty)
	End Sub

	Private Sub DeleteInterfaceInformation(ByVal TransportTypeId As Guid)
		For Each r As KaTransportTypeInterfaceSettings In KaTransportTypeInterfaceSettings.GetAll(GetUserConnection(_currentUser.Id), KaTransportTypeInterfaceSettings.FN_DELETED & " = 0 and " & KaTransportTypeInterfaceSettings.FN_TRANSPORT_TYPE_ID & " = " & Q(TransportTypeId), "")
			r.Deleted = True
			r.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
		Next
	End Sub

	Private Sub PopulateTransportTypeInterfaceList(ByVal transportTypeId As Guid)
		PopulateInterfaceList()
		ddlTransportTypeInterface.Items.Clear()
		ddlTransportTypeInterface.Items.Add(New ListItem(IIf(ddlInterface.Items.Count > 1, "Add an interface", ""), Guid.Empty.ToString))
		Dim getInterfaceRdr As OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), "SELECT " & KaTransportTypeInterfaceSettings.TABLE_NAME & ".id, interfaces.name, " & KaTransportTypeInterfaceSettings.TABLE_NAME & ".cross_reference " &
		   "FROM " & KaTransportTypeInterfaceSettings.TABLE_NAME & " " &
		   "INNER JOIN interfaces ON " & KaTransportTypeInterfaceSettings.TABLE_NAME & ".interface_id = interfaces.id " &
		   "INNER JOIN interface_types ON interfaces.interface_type_id = interface_types.id " &
		   "WHERE (" & KaTransportTypeInterfaceSettings.TABLE_NAME & ".deleted = 0) " &
			   "AND (interfaces.deleted = 0) " &
			   "AND (interface_types.deleted = 0) " &
			   "AND (" & KaTransportTypeInterfaceSettings.TABLE_NAME & "." & KaTransportTypeInterfaceSettings.FN_TRANSPORT_TYPE_ID & "=" & Q(transportTypeId) & ") " &
		   "ORDER BY interfaces.name, " & KaTransportTypeInterfaceSettings.TABLE_NAME & ".cross_reference")
		Do While getInterfaceRdr.Read
			ddlTransportTypeInterface.Items.Add(New ListItem(getInterfaceRdr.Item("name") & " (" & getInterfaceRdr.Item("cross_reference") & ")", getInterfaceRdr.Item("id").ToString()))
		Loop
		getInterfaceRdr.Close()
	End Sub

	Private Function PopulateInterfaceInformation(ByVal transportTypeInterfaceId As Guid) As Boolean
		Dim retval As Boolean = False
		If transportTypeInterfaceId <> Guid.Empty Then
			Dim transportTypeInterfaceSetting As KaTransportTypeInterfaceSettings = New KaTransportTypeInterfaceSettings(GetUserConnection(_currentUser.Id), transportTypeInterfaceId)
			ddlInterface.SelectedValue = transportTypeInterfaceSetting.InterfaceId.ToString
			tbxInterfaceCrossReference.Text = transportTypeInterfaceSetting.CrossReference
			chkDefaultSetting.Checked = transportTypeInterfaceSetting.DefaultSetting
			chkExportOnly.Checked = transportTypeInterfaceSetting.ExportOnly
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

	Protected Sub ddlTransportTypeInterface_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles ddlTransportTypeInterface.SelectedIndexChanged
		btnRemoveInterface.Enabled = PopulateInterfaceInformation(Guid.Parse(ddlTransportTypeInterface.SelectedValue))
	End Sub
#End Region

	Private Sub ddlInterface_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles ddlInterface.SelectedIndexChanged
		If Guid.Parse(ddlTransportTypeInterface.SelectedValue) = Guid.Empty Then
			'Only do this check if we are a new interface setting
			Dim count As Integer = 0
			Try
				Dim rdr As OleDb.OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), "SELECT count(*) " &
																			 "FROM " & KaTransportTypeInterfaceSettings.TABLE_NAME & " " &
																			 "WHERE " & KaTransportTypeInterfaceSettings.FN_DELETED & " = 0 " &
																			 "AND " & KaTransportTypeInterfaceSettings.FN_INTERFACE_ID & " = " & Q(Guid.Parse(ddlInterface.SelectedValue)) & " " &
																			 "AND " & KaTransportTypeInterfaceSettings.FN_TRANSPORT_TYPE_ID & " = " & Q(Guid.Parse(ddlTransportTypes.SelectedValue)))
				If rdr.Read Then count = rdr.Item(0)
			Catch ex As Exception

			End Try
			chkDefaultSetting.Checked = (count = 0)
		End If
	End Sub

	Protected Overrides Function SaveViewState() As Object
		Dim viewState(2) As Object

		Utilities.ConvertCustomFieldPanelToLists(_customFields, _customFieldData, lstCustomFields)
		viewState(0) = MyBase.SaveViewState()
		viewState(1) = _customFields
		viewState(2) = _customFieldData
		Return viewState
	End Function

	Protected Overrides Sub LoadViewState(savedState As Object)
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
        With _currentUserPermission(KaTransport.TABLE_NAME)
            Dim shouldEnable = (.Edit AndAlso ddlTransportTypes.SelectedIndex > 0) OrElse (.Create AndAlso ddlTransportTypes.SelectedIndex = 0)
            pnlEven.Enabled = shouldEnable
            pnlInterfaceSettings.Enabled = shouldEnable
            btnSave.Enabled = shouldEnable
        End With
    End Sub
End Class