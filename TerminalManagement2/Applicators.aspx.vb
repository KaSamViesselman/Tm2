Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class Applicators : Inherits System.Web.UI.Page
#Region "Events"
    Private _currentUser As KaUser
    Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
    Private _currentTableName As String = KaApplicator.TABLE_NAME
    Private _customFields As New List(Of KaCustomField)
    Private _customFieldData As New List(Of KaCustomFieldData)

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        Response.Cache.SetCacheability(HttpCacheability.NoCache)
        _currentUser = Utilities.GetUser(Me)
        _currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Applicators")
        If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")

        If Not Page.IsPostBack Then
            SetTextboxMaxLengths()
            PopulateApplicatorList()
            PopulateInterfaceList()
            SetControlUsabilityFromPermissions()
            Utilities.ConfirmBox(Me.btnDelete, "Are you sure you want to delete this applicator?") ' Delete confirmation box setup
            Utilities.SetFocus(tbxName, Me) ' set focus to the first textbox on the page
            _customFields.Clear()
            For Each customField As KaCustomField In KaCustomField.GetAll(GetUserConnection(_currentUser.Id), String.Format("deleted=0 AND {0} = {1}", KaCustomField.FN_TABLE_NAME, Q(KaApplicator.TABLE_NAME)), KaCustomField.FN_FIELD_NAME)
                _customFields.Add(customField)
            Next
            Dim applicatorId As Guid = Guid.Empty
            Guid.TryParse(Page.Request("ApplicatorId"), applicatorId)
            PopulateApplicatorInformation(applicatorId)
        End If
    End Sub

    Protected Sub ddlApplicators_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles ddlApplicators.SelectedIndexChanged
        Dim applicatorId As Guid = Guid.Parse(ddlApplicators.SelectedValue)
        PopulateApplicatorInformation(applicatorId)
        PopulateApplicatorInterfaceList(applicatorId)
        PopulateInterfaceInformation(Guid.Empty)
        SetControlUsabilityFromPermissions()
    End Sub

    Protected Sub btnSave_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnSave.Click
        With New KaApplicator()
            .Id = Guid.Parse(ddlApplicators.SelectedValue)
            If tbxName.Text.Length = 0 Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidName", Utilities.JsAlert("Please specify a name.")) : Exit Sub
            If KaApplicator.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND id<>" & Q(.Id) & " AND name=" & Q(tbxName.Text), "").Count > 0 Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidDuplicateName", Utilities.JsAlert("A applicator with the name """ & tbxName.Text & """ already exists. Please specify a unique name for this applicator.")) : Exit Sub
            If Not IsNumeric(tbxAcres.Text) Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidAcres", Utilities.JsAlert("Acres must be a numeric value.")) : Exit Sub
            .Name = tbxName.Text
            .License = tbxLicense.Text
            .EpaNumber = tbxEpaNumber.Text
            .Acres = tbxAcres.Text
            .Email = tbxEmail.Text
            Dim status As String = ""
            If .Id <> Guid.Empty Then
                .SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                status = "Applicator updated successfully."
            Else
                .SqlInsert(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                status = "Applicator added successfully."
            End If

            Utilities.ConvertCustomFieldPanelToLists(_customFields, _customFieldData, lstCustomFields)
            For Each customData As KaCustomFieldData In _customFieldData
                customData.RecordId = .Id
                customData.SqlUpdateInsertIfNotFound(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
            Next

            PopulateApplicatorList()

            ddlApplicators.SelectedValue = .Id.ToString()
            ddlApplicators_SelectedIndexChanged(ddlApplicators, New EventArgs())
            lblStatus.Text = status
            btnDelete.Enabled = _currentUserPermission(_currentTableName).Delete
        End With
    End Sub

    Protected Sub btnDelete_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnDelete.Click
        If DeleteApplicatorInformation(Guid.Parse(ddlApplicators.SelectedValue)) Then
            PopulateApplicatorList()
            PopulateApplicatorInformation(Guid.Empty)
            btnDelete.Enabled = False
        End If
    End Sub
#End Region

    Private Sub PopulateApplicatorList()
        lblStatus.Text = ""
        ddlApplicators.Items.Clear()
        If _currentUserPermission(_currentTableName).Create Then ddlApplicators.Items.Add(New ListItem("Enter a new applicator", Guid.Empty.ToString())) Else ddlApplicators.Items.Add(New ListItem("Select an applicator", Guid.Empty.ToString()))
        For Each r As KaApplicator In KaApplicator.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
            ddlApplicators.Items.Add(New ListItem(r.Name, r.Id.ToString()))
        Next
    End Sub

    Private Sub PopulateApplicatorInformation(ByVal id As Guid)
        _customFieldData.Clear()
        With New KaApplicator()
            .Id = id
            Try
                .SqlSelect(GetUserConnection(_currentUser.Id))
                btnDelete.Enabled = _currentUserPermission(_currentTableName).Delete
            Catch ex As RecordNotFoundException
                btnDelete.Enabled = False
            End Try
            tbxName.Text = .Name
            tbxLicense.Text = .License
            tbxEpaNumber.Text = .EpaNumber
            tbxAcres.Text = .Acres
            tbxEmail.Text = .Email
            For Each customFieldValue As KaCustomFieldData In KaCustomFieldData.GetAll(GetUserConnection(_currentUser.Id), String.Format("deleted = 0 AND {0} = {1}", KaCustomFieldData.FN_RECORD_ID, Q(.Id)), KaCustomFieldData.FN_LAST_UPDATED)
                _customFieldData.Add(customFieldValue)
            Next
        End With

		Utilities.CreateDynamicCustomFieldPanelControls(_customFields, _customFieldData, lstCustomFields, Page)
	End Sub

	Private Sub SetTextboxMaxLengths()
		tbxAcres.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaApplicator.TABLE_NAME, KaApplicator.FN_ACRES))
		tbxEpaNumber.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaApplicator.TABLE_NAME, KaApplicator.FN_EPANUMBER))
		tbxLicense.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaApplicator.TABLE_NAME, KaApplicator.FN_LICENSE))
		tbxName.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaApplicator.TABLE_NAME, KaApplicator.FN_NAME))
		tbxInterfaceCrossReference.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaApplicatorInterfaceSettings.TABLE_NAME, KaApplicatorInterfaceSettings.FN_CROSS_REFERENCE))
		tbxEmail.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaApplicator.TABLE_NAME, KaApplicator.FN_EMAIL))
	End Sub

	Private Function DeleteApplicatorInformation(ByVal applicatorId As Guid) As Boolean
		lblStatus.Text = ""
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim applicatorIdVal As String = applicatorId.ToString()
		Dim reader As OleDbDataReader = Tm2Database.ExecuteReader(connection, String.Format("SELECT DISTINCT orders.id AS order_id, orders.number FROM orders WHERE orders.applicator_id={0} AND orders.deleted=0 AND orders.completed=0", Q(applicatorId)))
		Dim warning As String = ""
		Dim conditions As String = "id=" & Q(Guid.Empty)
		Do While reader.Read()
			warning &= reader("number") & " "
		Loop
		reader.Close()

		If warning.Trim.Length > 0 Then
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidApplicatorUsed", Utilities.JsAlert("Applicator is associated with other records (see below for details). Applicator information not deleted.\n\nDetails:\n\nOrders:\n" & warning.Trim))
			Return False
		Else
			With New KaApplicator(connection, applicatorId)
				.Deleted = True
				.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
				Tm2Database.ExecuteNonQuery(connection, String.Format("UPDATE {0} SET {1} = 1 WHERE {2} = {3}", KaApplicatorInterfaceSettings.TABLE_NAME, KaApplicatorInterfaceSettings.FN_DELETED, KaApplicatorInterfaceSettings.FN_APPLICATOR_ID, Q(.Id)))
				lblStatus.Text = "Applicator deleted successfully."
			End With
			Return True
		End If
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
					"AND ((" & KaInterfaceTypes.FN_SHOW_APPLICATOR_INTERFACE & " = 1) " &
					"OR (interfaces.id IN (SELECT " & KaApplicatorInterfaceSettings.TABLE_NAME & ".interface_id " &
							"FROM " & KaApplicatorInterfaceSettings.TABLE_NAME & " " &
							"WHERE (deleted=0) " &
								"AND (" & KaApplicatorInterfaceSettings.TABLE_NAME & "." & KaApplicatorInterfaceSettings.FN_APPLICATOR_ID & " = " & Q(ddlApplicators.SelectedValue) & ") " &
								"AND (" & KaApplicatorInterfaceSettings.TABLE_NAME & "." & KaApplicatorInterfaceSettings.FN_APPLICATOR_ID & " <> " & Q(Guid.Empty) & ")))) " &
				"ORDER BY interfaces.name")
		Do While getInterfaceRdr.Read
			ddlInterface.Items.Add(New ListItem(getInterfaceRdr.Item("name"), getInterfaceRdr.Item("id").ToString()))
		Loop
		getInterfaceRdr.Close()
		pnlInterfaceSetup.Visible = (ddlInterface.Items.Count > 1) AndAlso ddlApplicators.SelectedValue <> Guid.Empty.ToString
	End Sub

	Private Sub SaveInterface()
		lblStatus.Text = ""
		If Guid.Parse(ddlApplicators.SelectedValue) = Guid.Empty Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidNotSaved", Utilities.JsAlert("You must save the Applicator before you can set up interface cross references.")) : Exit Sub
		If Guid.Parse(ddlInterface.SelectedValue) = Guid.Empty Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidInterface", Utilities.JsAlert("An interface must be selected. Interface settings not saved.")) : Exit Sub
		If tbxInterfaceCrossReference.Text.Length = 0 Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidCrossReference", Utilities.JsAlert("A cross reference must be specified. Interface settings not saved.")) : Exit Sub

		' If this is not export only, check if there are any other settings with the same cross reference ID
		If Not chkExportOnly.Checked Then
			Dim allInterfaceSettings As ArrayList = KaApplicatorInterfaceSettings.GetAll(GetUserConnection(_currentUser.Id), KaApplicatorInterfaceSettings.FN_CROSS_REFERENCE & " = " & Q(tbxInterfaceCrossReference.Text.Trim) &
																							" AND " & KaApplicatorInterfaceSettings.FN_INTERFACE_ID & " = " & Q(ddlInterface.SelectedValue) &
																							" AND " & KaApplicatorInterfaceSettings.FN_DELETED & " = 0 " &
																							" AND " & KaApplicatorInterfaceSettings.FN_EXPORT_ONLY & " = 0 " &
																							" AND " & KaApplicatorInterfaceSettings.FN_ID & " <> " & Q(ddlApplicatorInterface.SelectedValue), "")
			If allInterfaceSettings.Count > 0 Then
				ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidCrossReferenceExists", Utilities.JsAlert("A cross reference of " & tbxInterfaceCrossReference.Text.Trim & " already exists for this interface."))
				Exit Sub
			End If
		End If

		Dim applicatorInterfaceId As Guid = Guid.Parse(ddlApplicatorInterface.SelectedValue)
		If applicatorInterfaceId = Guid.Empty Then
			Dim applicatorInterface As KaApplicatorInterfaceSettings = New KaApplicatorInterfaceSettings
			applicatorInterface.ApplicatorId = Guid.Parse(ddlApplicators.SelectedValue)
			applicatorInterface.InterfaceId = Guid.Parse(ddlInterface.SelectedValue)
			applicatorInterface.CrossReference = tbxInterfaceCrossReference.Text.Trim
			applicatorInterface.DefaultSetting = chkDefaultSetting.Checked
			applicatorInterface.ExportOnly = chkExportOnly.Checked
			applicatorInterface.SqlInsert(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
			applicatorInterfaceId = applicatorInterface.Id
		Else
			Dim applicatorInterface As KaApplicatorInterfaceSettings = New KaApplicatorInterfaceSettings(GetUserConnection(_currentUser.Id), applicatorInterfaceId)
			applicatorInterface.ApplicatorId = Guid.Parse(ddlApplicators.SelectedValue)
			applicatorInterface.InterfaceId = Guid.Parse(ddlInterface.SelectedValue)
			applicatorInterface.CrossReference = tbxInterfaceCrossReference.Text.Trim
			applicatorInterface.DefaultSetting = chkDefaultSetting.Checked
			applicatorInterface.ExportOnly = chkExportOnly.Checked
			applicatorInterface.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
		End If

		PopulateApplicatorInterfaceList(Guid.Parse(ddlApplicators.SelectedValue))
		ddlApplicatorInterface.SelectedValue = applicatorInterfaceId.ToString
		ddlApplicatorInterface_SelectedIndexChanged(ddlApplicatorInterface, New EventArgs)
		btnRemoveInterface.Enabled = True
	End Sub

	Private Sub RemoveInterface()
		Dim selectedId As Guid = Guid.Parse(ddlApplicatorInterface.SelectedValue)
		If selectedId <> Guid.Empty Then
			Dim prodInterfaceSetting As KaApplicatorInterfaceSettings = New KaApplicatorInterfaceSettings(GetUserConnection(_currentUser.Id), selectedId)
			prodInterfaceSetting.Deleted = True
			prodInterfaceSetting.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
		End If
		PopulateApplicatorInterfaceList(Guid.Parse(ddlApplicators.SelectedValue))
		btnRemoveInterface.Enabled = PopulateInterfaceInformation(Guid.Empty)
	End Sub

	Private Sub DeleteInterfaceInformation(ByVal applicatorId As Guid)
		For Each r As KaApplicatorInterfaceSettings In KaApplicatorInterfaceSettings.GetAll(GetUserConnection(_currentUser.Id), KaApplicatorInterfaceSettings.FN_DELETED & " = 0 and " & KaApplicatorInterfaceSettings.FN_APPLICATOR_ID & " = " & Q(applicatorId), "")
			r.Deleted = True
			r.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
		Next
	End Sub

	Private Sub PopulateApplicatorInterfaceList(ByVal applicatorId As Guid)
		PopulateInterfaceList()
		ddlApplicatorInterface.Items.Clear()
		ddlApplicatorInterface.Items.Add(New ListItem(IIf(ddlInterface.Items.Count > 1, "Add an interface", ""), Guid.Empty.ToString))
		Dim getInterfaceRdr As OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), "SELECT " & KaApplicatorInterfaceSettings.TABLE_NAME & ".id, interfaces.name, " & KaApplicatorInterfaceSettings.TABLE_NAME & ".cross_reference " &
				"FROM " & KaApplicatorInterfaceSettings.TABLE_NAME & " " &
				"INNER JOIN interfaces ON " & KaApplicatorInterfaceSettings.TABLE_NAME & ".interface_id = interfaces.id " &
				"INNER JOIN interface_types ON interfaces.interface_type_id = interface_types.id " &
				"WHERE (" & KaApplicatorInterfaceSettings.TABLE_NAME & ".deleted = 0) " &
					"AND (interfaces.deleted = 0) " &
					"AND (interface_types.deleted = 0) " &
					"AND (" & KaApplicatorInterfaceSettings.TABLE_NAME & "." & KaApplicatorInterfaceSettings.FN_APPLICATOR_ID & "=" & Q(applicatorId) & ") " &
				"ORDER BY interfaces.name, " & KaApplicatorInterfaceSettings.TABLE_NAME & ".cross_reference")
		Do While getInterfaceRdr.Read
			ddlApplicatorInterface.Items.Add(New ListItem(getInterfaceRdr.Item("name") & " (" & getInterfaceRdr.Item("cross_reference") & ")", getInterfaceRdr.Item("id").ToString()))
		Loop
		getInterfaceRdr.Close()
	End Sub

	Private Function PopulateInterfaceInformation(ByVal applicatorInterfaceId As Guid) As Boolean
		Dim retval As Boolean = False
		If applicatorInterfaceId <> Guid.Empty Then
			Dim applicatorInterfaceSetting As KaApplicatorInterfaceSettings = New KaApplicatorInterfaceSettings(GetUserConnection(_currentUser.Id), applicatorInterfaceId)
			ddlInterface.SelectedValue = applicatorInterfaceSetting.InterfaceId.ToString
			tbxInterfaceCrossReference.Text = applicatorInterfaceSetting.CrossReference
			chkDefaultSetting.Checked = applicatorInterfaceSetting.DefaultSetting
			chkExportOnly.Checked = applicatorInterfaceSetting.ExportOnly
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

	Protected Sub ddlApplicatorInterface_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles ddlApplicatorInterface.SelectedIndexChanged
		btnRemoveInterface.Enabled = PopulateInterfaceInformation(Guid.Parse(ddlApplicatorInterface.SelectedValue))
	End Sub

	Private Sub ddlInterface_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles ddlInterface.SelectedIndexChanged
		If Guid.Parse(ddlApplicatorInterface.SelectedValue) = Guid.Empty Then
			'Only do this check if we are a new interface setting
			Dim count As Integer = 0
			Try
				Dim rdr As OleDb.OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), "SELECT count(*) " &
																			 "FROM " & KaApplicatorInterfaceSettings.TABLE_NAME & " " &
																			 "WHERE " & KaApplicatorInterfaceSettings.FN_DELETED & " = 0 " &
																			 "AND " & KaApplicatorInterfaceSettings.FN_INTERFACE_ID & " = " & Q(Guid.Parse(ddlInterface.SelectedValue)) & " " &
																			 "AND " & KaApplicatorInterfaceSettings.FN_APPLICATOR_ID & " = " & Q(Guid.Parse(ddlApplicators.SelectedValue)))
				If rdr.Read Then count = rdr.Item(0)
			Catch ex As Exception

			End Try
			chkDefaultSetting.Checked = (count = 0)
		End If
	End Sub
#End Region

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
            Dim shouldEnable = (.Edit AndAlso ddlApplicators.SelectedIndex > 0) OrElse (.Create AndAlso ddlApplicators.SelectedIndex = 0)
            pnlEven.Enabled = shouldEnable
            pnlInterfaceSettings.Enabled = shouldEnable
            btnSave.Enabled = shouldEnable
            btnDelete.Enabled = Not Guid.Parse(ddlApplicators.SelectedValue).Equals(Guid.Empty) AndAlso .Edit AndAlso .Delete
        End With
    End Sub
End Class