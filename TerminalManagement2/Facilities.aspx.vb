Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class Facilities : Inherits System.Web.UI.Page
    Private _currentUser As KaUser
Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
    Private _currentTableName As String = KaLocation.TABLE_NAME
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
    Private Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles MyBase.Load, Me.Load
        Response.Cache.SetCacheability(HttpCacheability.NoCache)
        _currentUser = Utilities.GetUser(Me)
        _currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Facilities")
        If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")
        pnlAccounts.Visible = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaCustomerAccount.TABLE_NAME}), "Accounts")(KaCustomerAccount.TABLE_NAME).Read
        pnlDrivers.Visible = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaDriver.TABLE_NAME}), "Drivers")(KaDriver.TABLE_NAME).Read
        If Not Page.IsPostBack Then
            PopulateFacilitiesList()
            PopulateInterfaceList()
            _customFields.Clear()
            For Each customField As KaCustomField In KaCustomField.GetAll(GetUserConnection(_currentUser.Id), String.Format("deleted=0 AND {0} = {1}", KaCustomField.FN_TABLE_NAME, Q(KaLocation.TABLE_NAME)), KaCustomField.FN_FIELD_NAME)
                _customFields.Add(customField)
            Next
            If Page.Request("LocationId") IsNot Nothing Then
                Try
                    ddlFacilities.SelectedValue = Page.Request("LocationId")
                Catch ex As ArgumentOutOfRangeException
                    ddlFacilities.SelectedIndex = 0
                End Try
            End If
            SetControlUsabilityFromPermissions()
            ddlFacilities_SelectedIndexChanged(ddlFacilities, New EventArgs())
            Utilities.ConfirmBox(Me.btnDelete, "Are you sure you want to delete this facility?")
            Utilities.SetFocus(tbxName, Me)
        End If
    End Sub

    Private Sub ddlFacilities_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ddlFacilities.SelectedIndexChanged
        lblStatus.Text = ""
        tbxCompletionPercentage.Text = "100"
        Dim facilityId As Guid = Guid.Parse(ddlFacilities.SelectedValue)
        PopulateFacilityData(facilityId)
        If facilityId <> Guid.Empty Then
            SetControlUsability(_currentUserPermission(_currentTableName).Edit)
            btnDelete.Enabled = _currentUserPermission(_currentTableName).Delete
        Else
            btnDelete.Enabled = False
            Utilities.SetFocus(tbxName, Me)
            lstDrivers.Items.Clear()
            lstAccounts.Items.Clear()
        End If
        PopulateDriverData(facilityId)
        PopulateAccountData(facilityId)
        PopulateLocationInterfaceList(facilityId)
        PopulateInterfaceInformation(Guid.Empty)
		Utilities.CreateDynamicCustomFieldPanelControls(_customFields, _customFieldData, lstCustomFields, Page)
		SetControlUsabilityFromPermissions()
	End Sub

	Private Sub btnSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSave.Click
		If ValidateFields() Then
			SaveFacility()
		End If
	End Sub

	Private Function ValidateFields() As Boolean
		If tbxName.Text.Trim = "" Then
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidName", Utilities.JsAlert("A name must be specified.")) : Return False
		End If

		Dim allLocations As ArrayList = KaLocation.GetAll(GetUserConnection(_currentUser.Id), "name = " & Q(tbxName.Text.Trim) & " and deleted = 0", "")
		If allLocations.Count > 0 Then
			Dim tempLocation As KaLocation = allLocations.Item(0)
			If tempLocation.Id <> Guid.Parse(ddlFacilities.SelectedValue) Then
				ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidDuplicateName", Utilities.JsAlert("A location with name " & tbxName.Text.Trim & " already exists.  Name must be unique")) : Return False
			End If
		End If

		Dim completionPercentage As Double = 0.0
		If cbxUsePercentageToDetermineOrderCompletion.Checked AndAlso Not Double.TryParse(tbxCompletionPercentage.Text.Trim, completionPercentage) OrElse completionPercentage > 100 OrElse completionPercentage < 0 Then
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidCompletionPercentage", Utilities.JsAlert("An order completion percent must be a specified between 0 and 100.")) : Return False
		End If
		If cbxUsePercentageToDetermineReceivingOrderCompletion.Checked AndAlso Not Double.TryParse(tbxReceivingOrderCompletionPercentage.Text.Trim, completionPercentage) OrElse completionPercentage > 100 OrElse completionPercentage < 0 Then
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidReceivingCompletionPercentage", Utilities.JsAlert("An receiving order completion percent must be a specified between 0 and 100.")) : Return False
		End If

		Dim message As String = ""
		If Not Utilities.IsEmailFieldValid(tbxEmail.Text, message) Then
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidEmail", Utilities.JsAlert(message))
			Return False
		End If
		Return True
	End Function

	Protected Sub btnDelete_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnDelete.Click
		lblStatus.Text = ""
		If Guid.Parse(ddlFacilities.SelectedValue) <> Guid.Empty Then
			With New KaLocation(GetUserConnection(_currentUser.Id), Guid.Parse(ddlFacilities.SelectedValue))
				.Deleted = True
				.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
				PopulateFacilitiesList()
				ddlFacilities.SelectedIndex = 0
				ddlFacilities_SelectedIndexChanged(ddlFacilities, New EventArgs())
				lblStatus.Text = "Selected Facility deleted successfully"
				SetControlUsabilityFromPermissions()
				btnDelete.Enabled = False
			End With
		End If
	End Sub

	Protected Sub cbxUseOwnerOrderCompletionSettings_CheckedChanged(sender As Object, e As EventArgs) Handles cbxUseOwnerOrderCompletionSettings.CheckedChanged
		UpdateControlsEnabled()
	End Sub

	Protected Sub cbxUsePercentageToDetermineOrderCompletion_CheckedChanged(sender As Object, e As EventArgs) Handles cbxUsePercentageToDetermineOrderCompletion.CheckedChanged
		UpdateControlsEnabled()
	End Sub

	Protected Sub cbxUseOwnerReceivingOrderCompletionSettings_CheckedChanged(sender As Object, e As EventArgs) Handles cbxUseOwnerReceivingOrderCompletionSettings.CheckedChanged
		UpdateControlsEnabled()
	End Sub

	Protected Sub cbxUsePercentageToDetermineReceivingOrderCompletion_CheckedChanged(sender As Object, e As EventArgs) Handles cbxUsePercentageToDetermineReceivingOrderCompletion.CheckedChanged
		UpdateControlsEnabled()
	End Sub
#End Region

	Private Sub PopulateFacilitiesList()
		ddlFacilities.Items.Clear() ' populate the Facilities list
		If _currentUserPermission(_currentTableName).Create Then
			ddlFacilities.Items.Add(New ListItem("Enter a new facility", Guid.Empty.ToString()))
		Else
			ddlFacilities.Items.Add(New ListItem("Select a facility", Guid.Empty.ToString()))
		End If
		For Each location As KaLocation In KaLocation.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
			ddlFacilities.Items.Add(New ListItem(location.Name, location.Id.ToString()))
		Next
	End Sub

	Private Sub SetDropListToCurrent(ByVal currentValue As Guid)
		ddlFacilities.SelectedValue = currentValue.ToString()
		ddlFacilities_SelectedIndexChanged(ddlFacilities, New EventArgs())
	End Sub

	Private Sub PopulateFacilityData(ByVal facilityId As Guid)
		Dim facilityInfo As KaLocation
		Try
			facilityInfo = New KaLocation(GetUserConnection(_currentUser.Id), facilityId)
		Catch ex As RecordNotFoundException
			facilityInfo = New KaLocation
		End Try

		_customFieldData.Clear()
		With facilityInfo
			tbxName.Text = .Name
			tbxAddress.Text = .Street
			tbxCity.Text = .City
			tbxState.Text = .State
			tbxZip.Text = .ZipCode
			tbxCountry.Text = .Country
			tbxEpaNumber.Text = .EpaNumber
			tbxPhone.Text = .Phone
			tbxEmail.Text = .Email
			cbxUseOwnerOrderCompletionSettings.Checked = .UseOwnerOrderCompletionSettings
			cbxUsePercentageToDetermineOrderCompletion.Checked = .UsePercentToDetermineOrderCompletion
			tbxCompletionPercentage.Text = .CompletionPercentage
			cbxUseBatchCountToDetermineOrderCompletion.Checked = .UseBatchCountToDetermineOrderCompletion

			cbxUseOwnerReceivingOrderCompletionSettings.Checked = .UseOwnerReceivingPoCompletionSettings
			cbxUsePercentageToDetermineReceivingOrderCompletion.Checked = .UsePercentToDetermineReceivingPoCompletion
			tbxReceivingOrderCompletionPercentage.Text = .ReceivingPoCompletionPercentage

			For Each customFieldValue As KaCustomFieldData In KaCustomFieldData.GetAll(GetUserConnection(_currentUser.Id), String.Format("deleted = 0 AND {0} = {1}", KaCustomFieldData.FN_RECORD_ID, Q(.Id)), KaCustomFieldData.FN_LAST_UPDATED)
				_customFieldData.Add(customFieldValue)
			Next
		End With
		UpdateControlsEnabled()
	End Sub

	Private Function FacilityAlreadyExists(ByVal facilityName As String) As Boolean
		Return KaUser.GetAll(GetUserConnection(_currentUser.Id), "deleted = 0 AND name=" & Q(facilityName), "").Count > 0
	End Function

	Private Sub SaveFacility()
		With New KaLocation()
			.Id = Guid.Parse(ddlFacilities.SelectedValue)
			.Name = tbxName.Text.Trim
			.Street = tbxAddress.Text.Trim
			.City = tbxCity.Text.Trim
			.State = tbxState.Text.Trim
			.ZipCode = tbxZip.Text.Trim
			.Country = tbxCountry.Text.Trim
			.EpaNumber = tbxEpaNumber.Text.Trim
			.Phone = tbxPhone.Text.Trim
			.Email = tbxEmail.Text.Trim
			.UseOwnerOrderCompletionSettings = cbxUseOwnerOrderCompletionSettings.Checked
			.UsePercentToDetermineOrderCompletion = cbxUsePercentageToDetermineOrderCompletion.Checked
			Double.TryParse(tbxCompletionPercentage.Text.Trim, .CompletionPercentage)
			.UseBatchCountToDetermineOrderCompletion = cbxUseBatchCountToDetermineOrderCompletion.Checked
			.UseOwnerReceivingPoCompletionSettings = cbxUseOwnerReceivingOrderCompletionSettings.Checked
			.UsePercentToDetermineReceivingPoCompletion = cbxUsePercentageToDetermineReceivingOrderCompletion.Checked
			Double.TryParse(tbxReceivingOrderCompletionPercentage.Text, .ReceivingPoCompletionPercentage)

			Dim status As String = ""
			If .Id = Guid.Empty Then
				.SqlInsert(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
				status = "New facility added successfully"
			Else
				.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
				status = "Selected facility updated successfully"
			End If

			Utilities.ConvertCustomFieldPanelToLists(_customFields, _customFieldData, lstCustomFields)
			For Each customData As KaCustomFieldData In _customFieldData
				customData.RecordId = .Id
				customData.SqlUpdateInsertIfNotFound(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
			Next

			PopulateFacilitiesList()
			SaveAccounts(.Id)
			SaveDrivers(.Id)
			SetDropListToCurrent(.Id)
			lblStatus.Text = status
		End With
		btnDelete.Enabled = _currentUserPermission(_currentTableName).Delete
	End Sub

	Private Sub UpdateControlsEnabled()
		cbxUsePercentageToDetermineOrderCompletion.Enabled = Not cbxUseOwnerOrderCompletionSettings.Checked
		tbxCompletionPercentage.Enabled = Not cbxUseOwnerOrderCompletionSettings.Checked AndAlso cbxUsePercentageToDetermineOrderCompletion.Checked
		cbxUseBatchCountToDetermineOrderCompletion.Enabled = Not cbxUseOwnerOrderCompletionSettings.Checked

		cbxUsePercentageToDetermineReceivingOrderCompletion.Enabled = Not cbxUseOwnerReceivingOrderCompletionSettings.Checked
		tbxReceivingOrderCompletionPercentage.Enabled = Not cbxUseOwnerReceivingOrderCompletionSettings.Checked AndAlso cbxUsePercentageToDetermineReceivingOrderCompletion.Checked

		btnDelete.Enabled = Not Guid.Parse(ddlFacilities.SelectedValue).Equals(Guid.Empty) AndAlso _currentUserPermission(_currentTableName).Delete
	End Sub

	Private Sub SetTextboxMaxLengths()
		tbxAddress.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaLocation.TABLE_NAME, "street"))
		tbxCity.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaLocation.TABLE_NAME, "city"))
		tbxCompletionPercentage.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaLocation.TABLE_NAME, "completion_percentage"))
		tbxEmail.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaLocation.TABLE_NAME, "email"))
		tbxEpaNumber.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaLocation.TABLE_NAME, "epa_number"))
		tbxName.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaLocation.TABLE_NAME, "name"))
		tbxPhone.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaLocation.TABLE_NAME, "phone"))
		tbxState.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaLocation.TABLE_NAME, "state"))
		tbxZip.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaLocation.TABLE_NAME, "zip_code"))
		tbxCountry.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaLocation.TABLE_NAME, "country"))
		tbxInterfaceCrossReference.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaLocationInterfaceSettings.TABLE_NAME, KaLocationInterfaceSettings.FN_CROSS_REFERENCE))
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
					"AND ((" & KaInterfaceTypes.FN_SHOW_LOCATION_INTERFACE & " = 1) " &
					"OR (interfaces.id IN (SELECT " & KaLocationInterfaceSettings.TABLE_NAME & ".interface_id " &
								"FROM " & KaLocationInterfaceSettings.TABLE_NAME & " " &
								"WHERE (deleted=0) " &
									"AND (" & KaLocationInterfaceSettings.TABLE_NAME & "." & KaLocationInterfaceSettings.FN_LOCATION_ID & " = " & Q(ddlFacilities.SelectedValue) & ") " &
									"AND (" & KaLocationInterfaceSettings.TABLE_NAME & "." & KaLocationInterfaceSettings.FN_LOCATION_ID & " <> " & Q(Guid.Empty) & ")))) " &
				"ORDER BY interfaces.name")
		Do While getInterfaceRdr.Read
			ddlInterface.Items.Add(New ListItem(getInterfaceRdr.Item("name"), getInterfaceRdr.Item("id").ToString()))
		Loop
		getInterfaceRdr.Close()
		pnlInterfaceSetup.Visible = (ddlInterface.Items.Count > 1) AndAlso ddlFacilities.SelectedValue <> Guid.Empty.ToString
	End Sub

	Private Sub SaveInterface()
		lblStatus.Text = ""
		If Guid.Parse(ddlFacilities.SelectedValue) = Guid.Empty Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidLocationNotSaved", Utilities.JsAlert("You must save the Location before you can set up interface cross references.")) : Exit Sub
		If Guid.Parse(ddlInterface.SelectedValue) = Guid.Empty Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidInterface", Utilities.JsAlert("An interface must be selected. Interface settings not saved.")) : Exit Sub
		If tbxInterfaceCrossReference.Text.Length = 0 Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidCrossReference", Utilities.JsAlert("A cross reference must be specified. Interface settings not saved.")) : Exit Sub

		' If this is not export only, check if there are any other settings with the same cross reference ID
		If Not chkExportOnly.Checked Then
			Dim allInterfaceSettings As ArrayList = KaLocationInterfaceSettings.GetAll(GetUserConnection(_currentUser.Id), KaLocationInterfaceSettings.FN_CROSS_REFERENCE & " = " & Q(tbxInterfaceCrossReference.Text.Trim) &
																							" AND " & KaLocationInterfaceSettings.FN_INTERFACE_ID & " = " & Q(ddlInterface.SelectedValue) &
																							" AND " & KaLocationInterfaceSettings.FN_DELETED & " = 0 " &
																							" AND " & KaLocationInterfaceSettings.FN_EXPORT_ONLY & " = 0 " &
																							" AND " & KaLocationInterfaceSettings.FN_ID & " <> " & Q(ddlFacilitiesInterface.SelectedValue), "")
			If allInterfaceSettings.Count > 0 Then
				ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidCrossReferenceExists", Utilities.JsAlert("A cross reference of " & tbxInterfaceCrossReference.Text.Trim & " already exists for this interface."))
				Exit Sub
			End If
		End If

		Dim locationInterfaceId As Guid = Guid.Parse(ddlFacilitiesInterface.SelectedValue)
		If locationInterfaceId = Guid.Empty Then
			Dim locationInterface As KaLocationInterfaceSettings = New KaLocationInterfaceSettings
			locationInterface.LocationId = Guid.Parse(ddlFacilities.SelectedValue)
			locationInterface.InterfaceId = Guid.Parse(ddlInterface.SelectedValue)
			locationInterface.CrossReference = tbxInterfaceCrossReference.Text.Trim
			locationInterface.DefaultSetting = chkDefaultSetting.Checked
			locationInterface.ExportOnly = chkExportOnly.Checked
			locationInterface.SqlInsert(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
			locationInterfaceId = locationInterface.Id
		Else
			Dim locationInterface As KaLocationInterfaceSettings = New KaLocationInterfaceSettings(GetUserConnection(_currentUser.Id), locationInterfaceId)
			locationInterface.LocationId = Guid.Parse(ddlFacilities.SelectedValue)
			locationInterface.InterfaceId = Guid.Parse(ddlInterface.SelectedValue)
			locationInterface.CrossReference = tbxInterfaceCrossReference.Text.Trim
			locationInterface.DefaultSetting = chkDefaultSetting.Checked
			locationInterface.ExportOnly = chkExportOnly.Checked
			locationInterface.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
		End If

		PopulateLocationInterfaceList(Guid.Parse(ddlFacilities.SelectedValue))
		ddlFacilitiesInterface.SelectedValue = locationInterfaceId.ToString
		ddlFacilitiesInterface_SelectedIndexChanged(ddlFacilitiesInterface, New EventArgs)
		btnRemoveInterface.Enabled = True
	End Sub

	Private Sub RemoveInterface()
		Dim selectedId As Guid = Guid.Parse(ddlFacilitiesInterface.SelectedValue)
		If selectedId <> Guid.Empty Then
			Dim prodInterfaceSetting As KaLocationInterfaceSettings = New KaLocationInterfaceSettings(GetUserConnection(_currentUser.Id), selectedId)
			prodInterfaceSetting.Deleted = True
			prodInterfaceSetting.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
		End If
		PopulateLocationInterfaceList(Guid.Parse(ddlFacilities.SelectedValue))
		btnRemoveInterface.Enabled = PopulateInterfaceInformation(Guid.Empty)
	End Sub

	Private Sub DeleteInterfaceInformation(ByVal locationId As Guid)
		For Each r As KaLocationInterfaceSettings In KaLocationInterfaceSettings.GetAll(GetUserConnection(_currentUser.Id), KaLocationInterfaceSettings.FN_DELETED & " = 0 and " & KaLocationInterfaceSettings.FN_LOCATION_ID & " = " & Q(locationId), "")
			r.Deleted = True
			r.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
		Next
	End Sub

	Private Sub PopulateLocationInterfaceList(ByVal locationId As Guid)
		PopulateInterfaceList()
		ddlFacilitiesInterface.Items.Clear()
		ddlFacilitiesInterface.Items.Add(New ListItem(IIf(ddlInterface.Items.Count > 1, "Add an interface", ""), Guid.Empty.ToString))
		Dim getInterfaceRdr As OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), "SELECT " & KaLocationInterfaceSettings.TABLE_NAME & ".id, interfaces.name, " & KaLocationInterfaceSettings.TABLE_NAME & ".cross_reference " &
					"FROM " & KaLocationInterfaceSettings.TABLE_NAME & " " &
					"INNER JOIN interfaces ON " & KaLocationInterfaceSettings.TABLE_NAME & ".interface_id = interfaces.id " &
					"INNER JOIN interface_types ON interfaces.interface_type_id = interface_types.id " &
					"WHERE (" & KaLocationInterfaceSettings.TABLE_NAME & ".deleted = 0) " &
						"AND (interfaces.deleted = 0) " &
						"AND (interface_types.deleted = 0) " &
						"AND (" & KaLocationInterfaceSettings.TABLE_NAME & "." & KaLocationInterfaceSettings.FN_LOCATION_ID & "=" & Q(locationId) & ") " &
					"ORDER BY interfaces.name, " & KaLocationInterfaceSettings.TABLE_NAME & ".cross_reference")
		Do While getInterfaceRdr.Read
			ddlFacilitiesInterface.Items.Add(New ListItem(getInterfaceRdr.Item("name") & " (" & getInterfaceRdr.Item("cross_reference") & ")", getInterfaceRdr.Item("id").ToString()))
		Loop
		getInterfaceRdr.Close()
	End Sub

	Private Function PopulateInterfaceInformation(ByVal locationInterfaceId As Guid) As Boolean
		Dim retval As Boolean = False
		If locationInterfaceId <> Guid.Empty Then
			Dim locationInterfaceSetting As KaLocationInterfaceSettings = New KaLocationInterfaceSettings(GetUserConnection(_currentUser.Id), locationInterfaceId)
			ddlInterface.SelectedValue = locationInterfaceSetting.InterfaceId.ToString
			tbxInterfaceCrossReference.Text = locationInterfaceSetting.CrossReference
			chkDefaultSetting.Checked = locationInterfaceSetting.DefaultSetting
			chkExportOnly.Checked = locationInterfaceSetting.ExportOnly
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

	Protected Sub ddlFacilitiesInterface_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles ddlFacilitiesInterface.SelectedIndexChanged
		btnRemoveInterface.Enabled = PopulateInterfaceInformation(Guid.Parse(ddlFacilitiesInterface.SelectedValue))
	End Sub
#End Region

	Private Sub ddlInterface_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles ddlInterface.SelectedIndexChanged
		If Guid.Parse(ddlFacilitiesInterface.SelectedValue) = Guid.Empty Then
			'Only do this check if we are a new interface setting
			Dim count As Integer = 0
			Try
				Dim rdr As OleDb.OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), "SELECT count(*) " &
																			 "FROM " & KaLocationInterfaceSettings.TABLE_NAME & " " &
																			 "WHERE " & KaLocationInterfaceSettings.FN_DELETED & " = 0 " &
																			 "AND " & KaLocationInterfaceSettings.FN_INTERFACE_ID & " = " & Q(Guid.Parse(ddlInterface.SelectedValue)) & " " &
																			 "AND " & KaLocationInterfaceSettings.FN_LOCATION_ID & " = " & Q(Guid.Parse(ddlFacilities.SelectedValue)))
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
            Dim shouldEnable = (.Edit AndAlso ddlFacilities.SelectedIndex > 0) OrElse (.Create AndAlso ddlFacilities.SelectedIndex = 0)
            pnlEven.Enabled = shouldEnable
            pnlInterfaceSettings.Enabled = shouldEnable
            pnlFacilityValidIn.Enabled = shouldEnable
            btnSave.Enabled = shouldEnable
            btnDelete.Enabled = Not Guid.Parse(ddlFacilities.SelectedValue).Equals(Guid.Empty) AndAlso .Edit AndAlso .Delete
        End With
    End Sub

#Region "Drivers and Accounts sections"
    Protected Sub ddlDrivers_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlDrivers.SelectedIndexChanged
        Dim driverId As Guid = Guid.Parse(ddlDrivers.SelectedValue)
        If driverId <> Guid.Empty Then
            Dim notFound As Boolean = True
            For Each item As ListItem In lstDrivers.Items
                If item.Value.ToString = ddlDrivers.SelectedValue Then
                    notFound = False
                    Exit For
                End If
            Next
            btnAddDriver.Enabled = notFound AndAlso _currentUserPermission(_currentTableName).Edit
        Else
            btnAddDriver.Enabled = False
        End If
    End Sub

    Protected Sub ddlAccounts_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlAccounts.SelectedIndexChanged
        Dim accountId As Guid = Guid.Parse(ddlAccounts.SelectedValue)
        If accountId <> Guid.Empty Then
            Dim notFound As Boolean = True
            For Each item As ListItem In lstAccounts.Items
                If item.Value.ToString = ddlAccounts.SelectedValue Then
                    notFound = False
                    Exit For
                End If
            Next
            btnAddAccount.Enabled = notFound AndAlso _currentUserPermission(_currentTableName).Edit
        Else
            btnAddAccount.Enabled = False
        End If
    End Sub

    Protected Sub lstDrivers_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lstDrivers.SelectedIndexChanged
        If lstDrivers.SelectedIndex >= 0 Then
            btnRemoveDriver.Enabled = _currentUserPermission(_currentTableName).Edit
        Else
            btnRemoveDriver.Enabled = False
        End If
    End Sub

    Protected Sub lstAccounts_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lstAccounts.SelectedIndexChanged
        If lstAccounts.SelectedIndex >= 0 Then
            btnRemoveAccount.Enabled = _currentUserPermission(_currentTableName).Edit
        Else
            btnRemoveAccount.Enabled = False
        End If
    End Sub

    Private Function ListContains(ByVal driverId As String, list As ListBox) As Boolean
        For Each li As ListItem In list.Items
            If (li.Value = driverId) Then
                Return True
            End If
        Next
        Return False
    End Function

    Protected Sub btnAddDriver_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnAddDriver.Click
        If ddlDrivers.SelectedIndex > 0 Then
            If ListContains(ddlDrivers.SelectedItem.Value, lstDrivers) Then
                Exit Sub
            End If
            lstDrivers.Items.Add(New ListItem(ddlDrivers.SelectedItem.Text, ddlDrivers.SelectedItem.Value))
            lstDrivers.SelectedIndex = lstDrivers.Items.Count - 1
            btnRemoveDriver.Enabled = True
            btnRemoveAllDrivers.Enabled = True
        End If
        ddlDrivers_SelectedIndexChanged(Nothing, Nothing)
    End Sub

    Protected Sub btnAddAccount_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnAddAccount.Click
        If ddlAccounts.SelectedIndex > 0 Then
            If ListContains(ddlAccounts.SelectedItem.Value, lstAccounts) Then
                Exit Sub
            End If
            lstAccounts.Items.Add(New ListItem(ddlAccounts.SelectedItem.Text, ddlAccounts.SelectedItem.Value))
            lstAccounts.SelectedIndex = lstAccounts.Items.Count - 1
            btnRemoveAccount.Enabled = True
            btnRemoveAllAccounts.Enabled = True
        End If
        ddlAccounts_SelectedIndexChanged(Nothing, Nothing)
    End Sub

    Protected Sub btnAddAllDrivers_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnAddAllDrivers.Click
        For Each li As ListItem In ddlDrivers.Items
            If li.Value = Guid.Empty.ToString Then
                Continue For
            Else
                If ListContains(li.Value, lstDrivers) Then
                    Continue For
                Else
                    lstDrivers.Items.Add(New ListItem(li.Text, li.Value))
                    lstDrivers.SelectedIndex = lstDrivers.Items.Count - 1
                End If
                btnRemoveDriver.Enabled = True
                btnRemoveAllDrivers.Enabled = True
            End If
        Next
        ddlDrivers_SelectedIndexChanged(Nothing, Nothing)
    End Sub

    Protected Sub btnAddAllAccounts_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnAddAllAccounts.Click
        For Each li As ListItem In ddlAccounts.Items
            If li.Value = Guid.Empty.ToString Then
                Continue For
            Else
                If ListContains(li.Value, lstAccounts) Then
                    Continue For
                Else
                    lstAccounts.Items.Add(New ListItem(li.Text, li.Value))
                    lstAccounts.SelectedIndex = lstAccounts.Items.Count - 1
                End If
                btnRemoveAccount.Enabled = True
                btnRemoveAllAccounts.Enabled = True
            End If
        Next
        ddlAccounts_SelectedIndexChanged(Nothing, Nothing)
    End Sub

    Protected Sub btnRemoveDriver_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnRemoveDriver.Click
        lblStatus.Text = ""
        If lstDrivers.SelectedIndex <> -1 Then
            lstDrivers.Items.RemoveAt(lstDrivers.SelectedIndex)
            lstDrivers.SelectedIndex = -1
            btnRemoveDriver.Enabled = False
            If lstDrivers.Items.Count = 0 Then
                btnRemoveAllDrivers.Enabled = False
            End If
        End If
        ddlDrivers_SelectedIndexChanged(Nothing, Nothing)
    End Sub

    Protected Sub btnRemoveAccount_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnRemoveAccount.Click
        lblStatus.Text = ""
        If lstAccounts.SelectedIndex <> -1 Then
            lstAccounts.Items.RemoveAt(lstAccounts.SelectedIndex)
            lstAccounts.SelectedIndex = -1
            btnRemoveAccount.Enabled = False
            If lstAccounts.Items.Count = 0 Then
                btnRemoveAllAccounts.Enabled = False
            End If
        End If
        ddlAccounts_SelectedIndexChanged(Nothing, Nothing)
    End Sub

    Protected Sub btnRemoveAllDrivers_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnRemoveAllDrivers.Click
        lblStatus.Text = ""
        lstDrivers.Items.Clear()
        btnRemoveDriver.Enabled = False
        btnRemoveAllDrivers.Enabled = False
        ddlDrivers_SelectedIndexChanged(Nothing, Nothing)
    End Sub

    Protected Sub btnRemoveAllAccounts_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnRemoveAllAccounts.Click
        lblStatus.Text = ""
        lstAccounts.Items.Clear()
        btnRemoveAccount.Enabled = False
        btnRemoveAllAccounts.Enabled = False
        ddlAccounts_SelectedIndexChanged(Nothing, Nothing)
    End Sub

    Private Sub PopulateDriverData(ByVal locationId As Guid)
        lstDrivers.Items.Clear()
        Dim getAssignedDriversRdr As OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), "SELECT        location_driver_access.driver_id, drivers.name + CASE WHEN drivers.valid_for_all_locations = 1 THEN ' (Valid for all locations)' ELSE '' END AS name " &
                                    "FROM location_driver_access " &
                                    "INNER JOIN drivers ON drivers.id = location_driver_access.driver_id " &
                                    "WHERE (location_driver_access.deleted = 0) AND (location_driver_access.location_id = " & Q(locationId) & ") " &
                                    "ORDER BY name")
        Do While getAssignedDriversRdr.Read()
            lstDrivers.Items.Add(New ListItem(getAssignedDriversRdr.Item("name"), getAssignedDriversRdr.Item("driver_id").ToString))
        Loop
        getAssignedDriversRdr.Close()
        ddlDrivers.Items.Clear()
        ddlDrivers.Items.Add(New ListItem("Select a driver", Guid.Empty.ToString()))
        Dim getDriversAvailRdr As OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), "SELECT id, name, valid_for_all_locations FROM " & KaDriver.TABLE_NAME & " WHERE deleted=0  order by name")

        Do While getDriversAvailRdr.Read()
            ddlDrivers.Items.Add(New ListItem(getDriversAvailRdr.Item("Name") & IIf(getDriversAvailRdr.Item("valid_for_all_locations"), " (Valid for all locations)", ""), getDriversAvailRdr.Item("Id").ToString()))
        Loop
        getDriversAvailRdr.Close()
        ddlDrivers_SelectedIndexChanged(Nothing, Nothing)
        lstDrivers_SelectedIndexChanged(Nothing, Nothing)
    End Sub

    Private Sub PopulateAccountData(ByVal locationId As Guid)
        lstAccounts.Items.Clear()

        Dim getAssignedAccountsRdr As OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), "SELECT location_customer_access.customer_account_id, customer_accounts.name + CASE WHEN customer_accounts.valid_for_all_locations = 1 THEN ' (Valid for all locations)' ELSE '' END AS name " &
                                    "FROM location_customer_access " &
                                    "INNER JOIN customer_accounts ON customer_accounts.id = location_customer_access.customer_account_id " &
                                    "WHERE (customer_accounts.deleted = 0) AND (location_customer_access.deleted = 0) AND (location_customer_access.location_id = " & Q(locationId) & ") " &
                                    "ORDER BY name")
        Do While getAssignedAccountsRdr.Read()
            lstAccounts.Items.Add(New ListItem(getAssignedAccountsRdr.Item("name"), getAssignedAccountsRdr.Item("customer_account_id").ToString))
        Loop
        getAssignedAccountsRdr.Close()
        ddlAccounts.Items.Clear()
        ddlAccounts.Items.Add(New ListItem("Select an account", Guid.Empty.ToString()))
        Dim getAccountsAvailRdr As OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), "SELECT id, name, valid_for_all_locations FROM " & KaCustomerAccount.TABLE_NAME & " WHERE deleted=0 AND is_supplier = 0 order by name")

        Do While getAccountsAvailRdr.Read()
            ddlAccounts.Items.Add(New ListItem(getAccountsAvailRdr.Item("Name") & IIf(getAccountsAvailRdr.Item("valid_for_all_locations"), " (Valid for all locations)", ""), getAccountsAvailRdr.Item("Id").ToString()))
        Loop
        getAccountsAvailRdr.Close()
        ddlAccounts_SelectedIndexChanged(Nothing, Nothing)
        lstAccounts_SelectedIndexChanged(Nothing, Nothing)
    End Sub

    Private Sub SaveDrivers(ByVal locationId As Guid)
        If locationId <> Guid.Empty Then
            Dim c As OleDbConnection = GetUserConnection(_currentUser.Id)
            Dim driverLocations As ArrayList = KaLocationDriverAccess.GetAll(GetUserConnection(_currentUser.Id), "location_id = " & Q(locationId), "last_updated desc")
            Dim updatedDriverGuids As List(Of Guid) = New List(Of Guid)
            Dim existingDriverGuids As List(Of Guid) = New List(Of Guid)
            'Compile list of  records to be inserted/updated from the ListBox
            For Each li As ListItem In lstDrivers.Items
                updatedDriverGuids.Add(Guid.Parse(li.Value))
            Next
            'Iterate through existing records from database, to update previously created records (either set deleted to true or false)
            For Each driver As KaLocationDriverAccess In driverLocations
                If (Not existingDriverGuids.Contains(driver.DriverId)) Then
                    If (updatedDriverGuids.Contains(driver.DriverId)) Then
                        driver.Deleted = False
                    Else
                        driver.Deleted = True
                    End If
                    driver.LastUpdated = Now
                    driver.SqlUpdate(c, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                    existingDriverGuids.Add(driver.DriverId)
                End If
            Next
            'Iterate through each Guid in the list box to determine if a new record should be inserted
            For Each driverId As Guid In updatedDriverGuids
                If (Not existingDriverGuids.Contains(driverId)) Then
                    With New KaLocationDriverAccess()
                        .Id = Guid.Empty
                        .LastUpdated = Now
                        .LocationId = locationId
                        .DriverId = driverId
                        .Deleted = False
                        .SqlInsert(c, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                    End With
                End If
            Next
        End If
    End Sub

    Private Sub SaveAccounts(ByVal locationId As Guid)
        If locationId <> Guid.Empty Then
            Dim c As OleDbConnection = GetUserConnection(_currentUser.Id)
            Dim customerAccountLocations As ArrayList = KaLocationCustomerAccountAccess.GetAll(GetUserConnection(_currentUser.Id), "location_id = " & Q(locationId), "last_updated desc")
            Dim updatedCustomerAccountGuids As List(Of Guid) = New List(Of Guid)
            Dim existingCustomerAccountGuids As List(Of Guid) = New List(Of Guid)

            For Each li As ListItem In lstAccounts.Items
                updatedCustomerAccountGuids.Add(Guid.Parse(li.Value))
            Next

            For Each account As KaLocationCustomerAccountAccess In customerAccountLocations
                If (Not existingCustomerAccountGuids.Contains(account.CustomerAccountId)) Then
                    If (updatedCustomerAccountGuids.Contains(account.CustomerAccountId)) Then
                        account.Deleted = False
                    Else
                        account.Deleted = True
                    End If
                    account.LastUpdated = Now
                    account.SqlUpdate(c, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                    existingCustomerAccountGuids.Add(account.CustomerAccountId)
                End If
            Next

            For Each accountId As Guid In updatedCustomerAccountGuids
                If (Not existingCustomerAccountGuids.Contains(accountId)) Then
                    With New KaLocationCustomerAccountAccess()
                        .Id = Guid.Empty
                        .LastUpdated = Now
                        .LocationId = locationId
                        .CustomerAccountId = accountId
                        .Deleted = False
                        .SqlInsert(c, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                    End With
                End If
            Next
        End If
    End Sub

    Private Sub SetControlUsability(enabled As Boolean)
        lstDrivers.Enabled = enabled
        btnAddAllDrivers.Enabled = enabled
        btnRemoveAllDrivers.Enabled = enabled
        btnAddDriver.Enabled = enabled
        btnRemoveDriver.Enabled = enabled
        ddlDrivers.Enabled = enabled
        lstAccounts.Enabled = enabled
        btnAddAllAccounts.Enabled = enabled
        btnRemoveAllAccounts.Enabled = enabled
        btnAddAccount.Enabled = enabled
        btnRemoveAccount.Enabled = enabled
        ddlAccounts.Enabled = enabled
    End Sub
#End Region
End Class