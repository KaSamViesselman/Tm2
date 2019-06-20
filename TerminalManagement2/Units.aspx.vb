Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class Units : Inherits System.Web.UI.Page
    Private _currentUser As KaUser
    Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
    Private _customFields As New List(Of KaCustomField)
    Private _customFieldData As New List(Of KaCustomFieldData)

#Region "Events"
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Response.Cache.SetCacheability(HttpCacheability.NoCache)
        _currentUser = Utilities.GetUser(Me)
        _currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaUnit.TABLE_NAME}), "Units")
        If Not _currentUserPermission(KaUnit.TABLE_NAME).Read Then Response.Redirect("Welcome.aspx")
        If Not Page.IsPostBack Then
            SetTextboxMaxLengths()

            _customFields.Clear()
            For Each customField As KaCustomField In KaCustomField.GetAll(GetUserConnection(_currentUser.Id), String.Format("deleted=0 AND {0} = {1}", KaCustomField.FN_TABLE_NAME, Q(KaUnit.TABLE_NAME)), KaCustomField.FN_FIELD_NAME)
                _customFields.Add(customField)
            Next

            PopulateUnitList()
            PopulateBaseUnitList()
            If Page.Request("UnitId") IsNot Nothing Then
                Try
                    ddlUnits.SelectedValue = Page.Request("UnitId")
                Catch ex As ArgumentOutOfRangeException
                    ddlUnits.SelectedIndex = 0
                End Try
            End If
            ddlUnits_SelectedIndexChanged(ddlUnits, New EventArgs())
            Utilities.ConfirmBox(Me.btnDelete, "Are you sure you want to delete this unit?")
            Utilities.SetFocus(tbxName, Me)
        End If
    End Sub

    Protected Sub btnSave_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnSave.Click
        lblStatus.Text = ""
        If ValidateFields() Then
            With New KaUnit()
                .Id = Guid.Parse(ddlUnits.SelectedValue)
                If .Id <> Guid.Empty Then .SqlSelect(GetUserConnection(_currentUser.Id))
                .Abbreviation = tbxAbbreviation.Text
                .BaseUnit = ddlBaseUnit.SelectedValue
                .Factor = tbxFactor.Text
                .Name = tbxName.Text
                .UnitPrecision = lblUnitPrecisionPreview.Text

                Dim status As String = ""
                If .Id = Guid.Empty Then
                    .SqlInsert(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                    status = "Unit successfully added."
                Else
                    .SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                    status = "Unit successfully updated."
                End If

                Utilities.ConvertCustomFieldPanelToLists(_customFields, _customFieldData, lstCustomFields)
                For Each customData As KaCustomFieldData In _customFieldData
                    customData.RecordId = .Id
                    customData.SqlUpdateInsertIfNotFound(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                Next

                PopulateUnitList()
                ddlUnits.SelectedValue = .Id.ToString()
                ddlUnits_SelectedIndexChanged(ddlUnits, New EventArgs())
                lblStatus.Text = status
            End With
        End If
    End Sub

    Protected Sub btnDelete_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnDelete.Click
        lblStatus.Text = ""
        Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
        Dim unitId As Guid = Guid.Parse(ddlUnits.SelectedValue)
        If unitId.Equals(Guid.Empty) OrElse CanDeleteUnit(connection, unitId) Then
            With New KaUnit(connection, unitId)
                .Deleted = True
                .SqlUpdate(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                PopulateUnitList()
                ddlUnits.SelectedValue = Guid.Empty.ToString
                ddlUnits_SelectedIndexChanged(ddlUnits, New EventArgs())
                lblStatus.Text = "Unit successfully deleted."
            End With
        End If
    End Sub

    Protected Sub ddlUnits_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles ddlUnits.SelectedIndexChanged
        PopulateUnitInformation()
        PopulateUnitInterfaceList(Guid.Parse(ddlUnits.SelectedValue))
        PopulateInterfaceInformation(Guid.Empty)
        SetControlUsabilityFromPermissions()
    End Sub

    Protected Sub btnUnitPrecisionMoreWhole_Click(sender As Object, e As EventArgs) Handles btnUnitPrecisionMoreWhole.Click
        Dim whole As Integer
        Dim fractional As Integer
        Tm2Database.GetPrecisionWholeAndFractionalDigitCount(lblUnitPrecisionPreview.Text, whole, fractional)
        lblUnitPrecisionPreview.Text = Tm2Database.GeneratePrecisionFormat(whole + 1, fractional)
    End Sub

    Protected Sub btnUnitPrecisionLessWhole_Click(sender As Object, e As EventArgs) Handles btnUnitPrecisionLessWhole.Click
        Dim whole As Integer
        Dim fractional As Integer
        Tm2Database.GetPrecisionWholeAndFractionalDigitCount(lblUnitPrecisionPreview.Text, whole, fractional)
        lblUnitPrecisionPreview.Text = Tm2Database.GeneratePrecisionFormat(Math.Max(whole - 1, 0), fractional)
    End Sub

    Protected Sub btnUnitPrecisionMoreFractional_Click(sender As Object, e As EventArgs) Handles btnUnitPrecisionMoreFractional.Click
        Dim whole As Integer
        Dim fractional As Integer
        Tm2Database.GetPrecisionWholeAndFractionalDigitCount(lblUnitPrecisionPreview.Text, whole, fractional)
        lblUnitPrecisionPreview.Text = Tm2Database.GeneratePrecisionFormat(whole, fractional + 1)
    End Sub

    Protected Sub btnUnitPrecisionLessFractional_Click(sender As Object, e As EventArgs) Handles btnUnitPrecisionLessFractional.Click
        Dim whole As Integer
        Dim fractional As Integer
        Tm2Database.GetPrecisionWholeAndFractionalDigitCount(lblUnitPrecisionPreview.Text, whole, fractional)
        lblUnitPrecisionPreview.Text = Tm2Database.GeneratePrecisionFormat(whole, Math.Max(fractional - 1, 0))
    End Sub
#End Region

    Private Sub PopulateUnitList()
        ddlUnits.Items.Clear()
        If _currentUserPermission(KaUnit.TABLE_NAME).Create Then
            ddlUnits.Items.Add(New ListItem("Enter new unit", Guid.Empty.ToString()))
        Else
            ddlUnits.Items.Add(New ListItem("Select unit", Guid.Empty.ToString()))
        End If
        For Each r As KaUnit In KaUnit.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
            ddlUnits.Items.Add(New ListItem(r.Name, r.Id.ToString()))
        Next
    End Sub

    Private Sub PopulateBaseUnitList()
        ddlBaseUnit.Items.Clear()
        Dim i As Integer = 0
        Do While i <= 12 ' use 12 because of 12 base units 
            If i <> KaUnit.Unit.Seconds OrElse i <> KaUnit.Unit.Pulses Then ddlBaseUnit.Items.Add(New ListItem(KaUnit.GetBaseUnitAbbreviation(i), i))
            i += 1
        Loop
    End Sub

    Private Sub PopulateUnitInformation()
        _customFieldData.Clear()
        lblStatus.Text = ""
        With New KaUnit()
            .Id = Guid.Parse(ddlUnits.SelectedValue)
            If .Id = Guid.Empty Then
                btnDelete.Enabled = False
            Else
                With _currentUserPermission(KaUnit.TABLE_NAME)
                    btnDelete.Enabled = .Edit AndAlso .Delete
                End With
                .SqlSelect(GetUserConnection(_currentUser.Id))
                For Each customFieldValue As KaCustomFieldData In KaCustomFieldData.GetAll(GetUserConnection(_currentUser.Id), String.Format("deleted = 0 AND {0} = {1}", KaCustomFieldData.FN_RECORD_ID, Q(.Id)), KaCustomFieldData.FN_LAST_UPDATED)
                    _customFieldData.Add(customFieldValue)
                Next
            End If
            tbxName.Text = .Name
            tbxAbbreviation.Text = .Abbreviation
            tbxFactor.Text = .Factor
            Try
                ddlBaseUnit.SelectedValue = .BaseUnit
            Catch ex As ArgumentOutOfRangeException
                ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidUnit", Utilities.JsAlert("Base unit not found where ID = " & .BaseUnit & "."))
                ddlBaseUnit.SelectedIndex = 0
            End Try
            lblUnitPrecisionPreview.Text = .UnitPrecision
        End With
		Utilities.CreateDynamicCustomFieldPanelControls(_customFields, _customFieldData, lstCustomFields, Page)
	End Sub

	Private Function ValidateFields() As Boolean
		If tbxName.Text.Trim.Length = 0 Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidName", Utilities.JsAlert("Name must be specified.")) : Return False
		If KaUnit.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND id<>" & Q(Guid.Parse(ddlUnits.SelectedValue)) & " AND name=" & Q(tbxName.Text), "").Count > 0 Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidDuplicateName", Utilities.JsAlert("Unit already exists with name = """ & tbxName.Text & """")) : Return False
		If tbxAbbreviation.Text.Trim.Length = 0 Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidAbbreviation", Utilities.JsAlert("Abbreviation must be specified.")) : Return False
		If KaUnit.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND id<>" & Q(Guid.Parse(ddlUnits.SelectedValue)) & " AND abbreviation=" & Q(tbxAbbreviation.Text), "").Count > 0 Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidAbbrevExists", Utilities.JsAlert("Unit already exists with abbreviation = """ & tbxAbbreviation.Text & """")) : Return False
		If Not IsNumeric(tbxFactor.Text) OrElse Double.Parse(tbxFactor.Text) <= 0 Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidFactor", Utilities.JsAlert("Factor must be a numeric value greater than zero.")) : Return False
		Return True
	End Function

	Private Sub SetTextboxMaxLengths()
		tbxAbbreviation.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaUnit.TABLE_NAME, "abbreviation"))
		tbxFactor.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaUnit.TABLE_NAME, "factor"))
		tbxName.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaUnit.TABLE_NAME, "name"))
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
					"AND ((" & KaInterfaceTypes.FN_SHOW_UNITS_INTERFACE & " = 1) " &
					"OR (interfaces.id IN (SELECT " & KaUnitInterfaceSettings.TABLE_NAME & ".interface_id " &
											"FROM " & KaUnitInterfaceSettings.TABLE_NAME & " " &
											"WHERE (deleted=0) " &
												"AND (" & KaUnitInterfaceSettings.TABLE_NAME & "." & KaUnitInterfaceSettings.FN_UNIT_ID & " = " & Q(ddlUnits.SelectedValue) & ") " &
												"AND (" & KaUnitInterfaceSettings.TABLE_NAME & "." & KaUnitInterfaceSettings.FN_UNIT_ID & " <> " & Q(Guid.Empty) & ")))) " &
				"ORDER BY interfaces.name")
		Do While getInterfaceRdr.Read
			ddlInterface.Items.Add(New ListItem(getInterfaceRdr.Item("name"), getInterfaceRdr.Item("id").ToString()))
		Loop
		getInterfaceRdr.Close()
		pnlInterfaceSetup.Visible = (ddlInterface.Items.Count > 1) AndAlso ddlUnits.SelectedValue <> Guid.Empty.ToString
	End Sub

	Private Sub SaveInterface()
		lblStatus.Text = ""
		If Guid.Parse(ddlUnits.SelectedValue) = Guid.Empty Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidNotSaved", Utilities.JsAlert("You must save the Unit before you can set up interface cross references.")) : Exit Sub
		If Guid.Parse(ddlInterface.SelectedValue) = Guid.Empty Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidInterface", Utilities.JsAlert("An interface must be selected. Interface settings not saved.")) : Exit Sub
		If tbxInterfaceCrossReference.Text.Length = 0 Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidCrossReference", Utilities.JsAlert("A cross reference must be specified. Interface settings not saved.")) : Exit Sub

		' If this is not export only, check if there are any other settings with the same cross reference ID
		If Not chkExportOnly.Checked Then
			Dim allInterfaceSettings As ArrayList = KaUnitInterfaceSettings.GetAll(GetUserConnection(_currentUser.Id), KaUnitInterfaceSettings.FN_CROSS_REFERENCE & " = " & Q(tbxInterfaceCrossReference.Text.Trim) &
																							" AND " & KaUnitInterfaceSettings.FN_INTERFACE_ID & " = " & Q(ddlInterface.SelectedValue) &
																							" AND " & KaUnitInterfaceSettings.FN_DELETED & " = 0 " &
																							" AND " & KaUnitInterfaceSettings.FN_EXPORT_ONLY & " = 0 " &
																							" AND " & KaUnitInterfaceSettings.FN_ID & " <> " & Q(ddlUnitInterface.SelectedValue), "")
			If allInterfaceSettings.Count > 0 Then
				ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidCrossReferenceExists", Utilities.JsAlert("A cross reference of " & tbxInterfaceCrossReference.Text.Trim & " already exists for this interface."))
				Exit Sub
			End If
		End If

		Dim unitInterfaceId As Guid = Guid.Parse(ddlUnitInterface.SelectedValue)
		If unitInterfaceId = Guid.Empty Then
			Dim unitInterface As KaUnitInterfaceSettings = New KaUnitInterfaceSettings
			unitInterface.UnitId = Guid.Parse(ddlUnits.SelectedValue)
			unitInterface.InterfaceId = Guid.Parse(ddlInterface.SelectedValue)
			unitInterface.CrossReference = tbxInterfaceCrossReference.Text.Trim
			unitInterface.DefaultSetting = chkDefaultSetting.Checked
			unitInterface.ExportOnly = chkExportOnly.Checked
			unitInterface.SqlInsert(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
			unitInterfaceId = unitInterface.Id
		Else
			Dim unitInterface As KaUnitInterfaceSettings = New KaUnitInterfaceSettings(GetUserConnection(_currentUser.Id), unitInterfaceId)
			unitInterface.UnitId = Guid.Parse(ddlUnits.SelectedValue)
			unitInterface.InterfaceId = Guid.Parse(ddlInterface.SelectedValue)
			unitInterface.CrossReference = tbxInterfaceCrossReference.Text.Trim
			unitInterface.DefaultSetting = chkDefaultSetting.Checked
			unitInterface.ExportOnly = chkExportOnly.Checked
			unitInterface.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
		End If

		PopulateUnitInterfaceList(Guid.Parse(ddlUnits.SelectedValue))
		ddlUnitInterface.SelectedValue = unitInterfaceId.ToString
		ddlUnitInterface_SelectedIndexChanged(ddlUnitInterface, New EventArgs)
		btnRemoveInterface.Enabled = True
	End Sub

	Private Sub RemoveInterface()
		Dim selectedId As Guid = Guid.Parse(ddlUnitInterface.SelectedValue)
		If selectedId <> Guid.Empty Then
			Dim unitInterfaceSetting As KaUnitInterfaceSettings = New KaUnitInterfaceSettings(GetUserConnection(_currentUser.Id), selectedId)
			unitInterfaceSetting.Deleted = True
			unitInterfaceSetting.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
		End If
		PopulateUnitInterfaceList(Guid.Parse(ddlUnits.SelectedValue))
		btnRemoveInterface.Enabled = PopulateInterfaceInformation(Guid.Empty)
	End Sub

	Private Sub DeleteInterfaceInformation(ByVal unitId As Guid)
		For Each r As KaUnitInterfaceSettings In KaUnitInterfaceSettings.GetAll(GetUserConnection(_currentUser.Id), KaUnitInterfaceSettings.FN_DELETED & " = 0 and " & KaUnitInterfaceSettings.FN_UNIT_ID & " = " & Q(unitId), "")
			r.Deleted = True
			r.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
		Next
	End Sub

	Private Sub PopulateUnitInterfaceList(ByVal unitId As Guid)
		PopulateInterfaceList()
		ddlUnitInterface.Items.Clear()
		ddlUnitInterface.Items.Add(New ListItem(IIf(ddlInterface.Items.Count > 1, "Add an interface", ""), Guid.Empty.ToString))
		Dim getInterfaceRdr As OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), "SELECT " & KaUnitInterfaceSettings.TABLE_NAME & ".id, interfaces.name, " & KaUnitInterfaceSettings.TABLE_NAME & ".cross_reference " &
											   "FROM " & KaUnitInterfaceSettings.TABLE_NAME & " " &
											   "INNER JOIN interfaces ON " & KaUnitInterfaceSettings.TABLE_NAME & ".interface_id = interfaces.id " &
											   "INNER JOIN interface_types ON interfaces.interface_type_id = interface_types.id " &
											   "WHERE (" & KaUnitInterfaceSettings.TABLE_NAME & ".deleted = 0) " &
												   "AND (interfaces.deleted = 0) " &
												   "AND (interface_types.deleted = 0) " &
												   "AND (" & KaUnitInterfaceSettings.TABLE_NAME & "." & KaUnitInterfaceSettings.FN_UNIT_ID & "=" & Q(unitId) & ") " &
											   "ORDER BY interfaces.name, " & KaUnitInterfaceSettings.TABLE_NAME & ".cross_reference")
		Do While getInterfaceRdr.Read
			ddlUnitInterface.Items.Add(New ListItem(getInterfaceRdr.Item("name") & " (" & getInterfaceRdr.Item("cross_reference") & ")", getInterfaceRdr.Item("id").ToString()))
		Loop
		getInterfaceRdr.Close()
	End Sub

	Private Function PopulateInterfaceInformation(ByVal unitInterfaceId As Guid) As Boolean
		Dim retval As Boolean = False
		If unitInterfaceId <> Guid.Empty Then
			Dim unitInterfaceSetting As KaUnitInterfaceSettings = New KaUnitInterfaceSettings(GetUserConnection(_currentUser.Id), unitInterfaceId)
			ddlInterface.SelectedValue = unitInterfaceSetting.InterfaceId.ToString
			tbxInterfaceCrossReference.Text = unitInterfaceSetting.CrossReference
			chkDefaultSetting.Checked = unitInterfaceSetting.DefaultSetting
			chkExportOnly.Checked = unitInterfaceSetting.ExportOnly
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

	Protected Sub ddlUnitInterface_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles ddlUnitInterface.SelectedIndexChanged
		btnRemoveInterface.Enabled = PopulateInterfaceInformation(Guid.Parse(ddlUnitInterface.SelectedValue))
	End Sub
#End Region

	Private Sub ddlInterface_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles ddlInterface.SelectedIndexChanged
		If Guid.Parse(ddlUnitInterface.SelectedValue) = Guid.Empty Then
			'Only do this check if we are a new interface setting
			Dim count As Integer = 0
			Try
				Dim rdr As OleDb.OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), "SELECT count(*) " &
																			 "FROM " & KaUnitInterfaceSettings.TABLE_NAME & " " &
																			 "WHERE " & KaUnitInterfaceSettings.FN_DELETED & " = 0 " &
																			 "AND " & KaUnitInterfaceSettings.FN_INTERFACE_ID & " = " & Q(Guid.Parse(ddlInterface.SelectedValue)) & " " &
																			 "AND " & KaUnitInterfaceSettings.FN_UNIT_ID & " = " & Q(Guid.Parse(ddlUnits.SelectedValue)))
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

    Private Function CanDeleteUnit(connection As OleDbConnection, unitId As Guid) As Boolean
        Dim tablesWithDeletedColumn As New DataTable
        Dim tableDA As New OleDbDataAdapter("SELECT LOWER([TABLE_NAME]) FROM [INFORMATION_SCHEMA].[COLUMNS] WHERE ([COLUMN_NAME] = 'deleted')", connection)
        If Tm2Database.CommandTimeout > 0 Then tableDA.SelectCommand.CommandTimeout = Tm2Database.CommandTimeout
        tableDA.Fill(tablesWithDeletedColumn)
        Dim tablesWithDeletedColumnList As New List(Of String)
        For Each row As DataRow In tablesWithDeletedColumn.Rows
            If Not tablesWithDeletedColumnList.Contains(row.Item(0)) Then tablesWithDeletedColumnList.Add(row.Item(0))
        Next

        Dim tablesWithUnitsColumn As New DataTable
        Dim tablesWithUnitsDA As New OleDbDataAdapter("SELECT LOWER([TABLE_NAME]), LOWER([COLUMN_NAME]) FROM [INFORMATION_SCHEMA].[COLUMNS] WHERE ([TABLE_NAME] <> 'unit_interface_settings') AND (NOT ([TABLE_NAME] like 'ticket%')) AND ([COLUMN_NAME] like '%unit_id%')", connection)
        If Tm2Database.CommandTimeout > 0 Then tablesWithUnitsDA.SelectCommand.CommandTimeout = Tm2Database.CommandTimeout
        tablesWithUnitsDA.Fill(tablesWithUnitsColumn)
        For Each row As DataRow In tablesWithUnitsColumn.Rows
            Dim sql As String = String.Format("SELECT COUNT(*) FROM {0} WHERE {1} = {2}", row.Item(0), row.Item(1), Q(unitId))
            If tablesWithDeletedColumnList.Contains(row.Item(0)) Then sql &= " AND deleted = 0"
            Dim tableReader As OleDbDataReader = Tm2Database.ExecuteReader(connection, sql)
            If tableReader.Read() AndAlso tableReader.Item(0) > 0 Then
                ClientScript.RegisterClientScriptBlock(Me.GetType(), "UnitUsedError", Utilities.JsAlert(String.Format("Unit cannot be deleted because it is in the {0} field in the {1} database table.", row.Item(1), row.Item(0))))
                Return False
            End If
        Next
        Return True
    End Function
    Private Sub SetControlUsabilityFromPermissions()
        With _currentUserPermission(KaUnit.TABLE_NAME)
            Dim shouldEnable = (.Edit AndAlso ddlUnits.SelectedIndex > 0) OrElse (.Create AndAlso ddlUnits.SelectedIndex = 0)
            pnlEven.Enabled = shouldEnable
            pnlGeneral.Enabled = shouldEnable
            pnlInterfaceSettings.Enabled = shouldEnable
            btnSave.Enabled = shouldEnable
        End With
    End Sub

End Class
