Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class Carriers : Inherits System.Web.UI.Page
    Private _currentUser As KaUser
Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
    Private _currentTableName As String = KaCarrier.TABLE_NAME
    Private _customFields As New List(Of KaCustomField)
    Private _customFieldData As New List(Of KaCustomFieldData)

#Region "Events"
	Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
		Response.Cache.SetCacheability(HttpCacheability.NoCache)
		_currentUser = Utilities.GetUser(Me)
		_currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Carriers")
		If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")

		If Not Page.IsPostBack Then
			SetTextboxMaxLengths()
			_customFields.Clear()
			For Each customField As KaCustomField In KaCustomField.GetAll(GetUserConnection(_currentUser.Id), String.Format("deleted=0 AND {0} = {1}", KaCustomField.FN_TABLE_NAME, Q(KaCarrier.TABLE_NAME)), KaCustomField.FN_FIELD_NAME)
				_customFields.Add(customField)
			Next
			Dim carrierId As Guid = Guid.Empty
			Guid.TryParse(Page.Request("CarrierId"), carrierId)
			PopulateCarriersList(carrierId)
			SetControlUsabilityFromPermissions()
			Utilities.SetFocus(tbxName, Me)
			Utilities.ConfirmBox(Me.btnDelete, "Are you sure you want to delete this carrier?")
		End If
	End Sub

	Private Sub ddlCarriers_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ddlCarriers.SelectedIndexChanged
		lblStatus.Text = ""
		Dim carrierId As Guid = Guid.Parse(ddlCarriers.SelectedValue)
		_customFieldData.Clear()
		PopulateCarrierData(carrierId)
		btnDelete.Enabled = carrierId <> Guid.Empty AndAlso _currentUserPermission(_currentTableName).Delete ' can't delete a carrier that doesn't exist yet
		PopulateCarrierInterfaceList(Guid.Parse(ddlCarriers.SelectedValue))
		SetControlUsabilityFromPermissions()
		Utilities.CreateDynamicCustomFieldPanelControls(_customFields, _customFieldData, lstCustomFields, Page)
		Utilities.SetFocus(tbxName, Me)
	End Sub

	Private Sub btnSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSave.Click
		If ValidateFields() Then
			Dim carrierId As Guid = Guid.Parse(ddlCarriers.SelectedValue)
			If carrierId <> Guid.Empty Then ' existing carrier
				SaveCarrier()
			Else ' new carrier
				If Not CarrierNumberAlreadyExists(carrierId, tbxCarrierNumber.Text) Then
					SaveCarrier()
				Else
					pnlEven.Visible = False
					pnlWarning.Visible = True
					lblWarning.Text = "Warning !! This carrier number is already listed in the database." & vbCrLf & "Do you want to continue?"
				End If
			End If
		End If
	End Sub

	Private Sub btnDelete_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDelete.Click
		lblStatus.Text = ""
		If Guid.Parse(ddlCarriers.SelectedValue) <> Guid.Empty Then
			With New KaCarrier(GetUserConnection(_currentUser.Id), Guid.Parse(ddlCarriers.SelectedValue))
				.Deleted = True
				.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
				PopulateCarriersList(Guid.Empty)
				lblStatus.Text = "Selected carrier deleted successfully"
			End With
		End If
	End Sub

	Private Sub btnYes_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnYes.Click
		SaveCarrier()
		pnlWarning.Visible = False
		pnlEven.Visible = True
	End Sub

	Private Sub btnNo_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnNo.Click
		Response.Redirect("Carriers.aspx")
	End Sub
#End Region

	Private Sub PopulateCarriersList(carrierId As Guid)
		ddlCarriers.Items.Clear()
		If _currentUserPermission(_currentTableName).Create Then ddlCarriers.Items.Add(New ListItem("Enter a new carrier", Guid.Empty.ToString())) Else ddlCarriers.Items.Add(New ListItem("Select a carrier", Guid.Empty.ToString()))
		For Each c As KaCarrier In KaCarrier.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
			ddlCarriers.Items.Add(New ListItem(c.Name, c.Id.ToString()))
			If c.Id = carrierId Then ddlCarriers.SelectedIndex = ddlCarriers.Items.Count - 1
		Next
		If ddlCarriers.SelectedIndex = -1 Then ddlCarriers.SelectedIndex = 0
		ddlCarriers_SelectedIndexChanged(ddlCarriers, New EventArgs())
		pnlWarning.Visible = False
	End Sub

	Private Sub PopulateCarrierData(carrierId As Guid)
		Dim carrier As KaCarrier
		If carrierId <> Guid.Empty Then
			carrier = New KaCarrier(GetUserConnection(_currentUser.Id), carrierId)
			For Each customFieldValue As KaCustomFieldData In KaCustomFieldData.GetAll(GetUserConnection(_currentUser.Id), String.Format("deleted = 0 AND {0} = {1}", KaCustomFieldData.FN_RECORD_ID, Q(carrierId)), KaCustomFieldData.FN_LAST_UPDATED)
				_customFieldData.Add(customFieldValue)
			Next
		Else ' user is entering a new carrier
			carrier = New KaCarrier()
			_customFieldData.Clear()
		End If
		With carrier
			tbxName.Text = .Name
			tbxCarrierNumber.Text = .Number
			tbxAddress.Text = .Street
			tbxCity.Text = .City
			tbxState.Text = .State
			tbxZip.Text = .ZipCode
			tbxCountry.Text = .Country
			tbxPhone.Text = .Phone
			tbxEmail.Text = .Email
			tbxNotes.Text = .Notes
		End With
	End Sub

	Private Function ValidateFields() As Boolean
		If tbxName.Text.Trim = "" Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidName", Utilities.JsAlert("Please specify a name for this carrier.")) : Return False
		Dim conditions As String = String.Format("deleted=0 AND name={0} AND id<>{1}", Q(tbxName.Text), Q(ddlCarriers.SelectedValue))
		If KaCarrier.GetAll(GetUserConnection(_currentUser.Id), conditions, "").Count > 0 Then
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidDuplicateName", Utilities.JsAlert("A carrier with the name """ & tbxName.Text & """ already exists. Please enter a unique name for this carrier.")) : Return False
		End If
		Dim message As String = ""
		If Not Utilities.IsEmailFieldValid(tbxEmail.Text, message) Then
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidEmail", Utilities.JsAlert(message))
			Return False
		End If
		Return True
	End Function

	Private Function CarrierNumberAlreadyExists(ByVal carrierId As Guid, ByVal carrierNumber As String) As Boolean
		If carrierNumber.Trim.Length > 0 Then
			Return KaCarrier.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND number=" & Q(carrierNumber.Trim) & " AND id <> " & Q(carrierId), "").Count > 0
		Else
			Return False
		End If
	End Function

	Private Sub SaveCarrier()
		With New KaCarrier()
			.Id = Guid.Parse(ddlCarriers.SelectedValue)
			.Name = tbxName.Text.Trim
			.Number = tbxCarrierNumber.Text.Trim
			.Street = tbxAddress.Text.Trim
			.City = tbxCity.Text.Trim
			.State = tbxState.Text.Trim
			.Phone = tbxPhone.Text.Trim
			.ZipCode = tbxZip.Text.Trim
			.Country = tbxCountry.Text.Trim
			.Email = tbxEmail.Text.Trim
			.Notes = tbxNotes.Text.Trim

			Dim status As String = ""
			If .Id = Guid.Empty Then
				.SqlInsert(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
				status = "New carrier added successfully"
			Else
				.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
				status = "Selected carrier updated successfully"
			End If

			Utilities.ConvertCustomFieldPanelToLists(_customFields, _customFieldData, lstCustomFields)
			For Each customData As KaCustomFieldData In _customFieldData
				customData.RecordId = .Id
				customData.SqlUpdateInsertIfNotFound(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
			Next
			PopulateCarriersList(.Id)
			lblStatus.Text = status
		End With
		btnDelete.Enabled = _currentUserPermission(_currentTableName).Delete
	End Sub

	Private Sub SetTextboxMaxLengths()
		tbxAddress.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaCarrier.TABLE_NAME, "street"))
		tbxCarrierNumber.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaCarrier.TABLE_NAME, ""))
		tbxCity.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaCarrier.TABLE_NAME, "city"))
		tbxEmail.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaCarrier.TABLE_NAME, "email"))
		tbxName.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaCarrier.TABLE_NAME, "name"))
		tbxNotes.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaCarrier.TABLE_NAME, "notes"))
		tbxPhone.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaCarrier.TABLE_NAME, "phone"))
		tbxState.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaCarrier.TABLE_NAME, "state"))
		tbxZip.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaCarrier.TABLE_NAME, "zip_code"))
		tbxCountry.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaBranch.TABLE_NAME, "country"))
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
						"AND ((" & KaInterfaceTypes.FN_SHOW_CARRIER_INTERFACE & " = 1) " &
						"OR (interfaces.id IN (SELECT " & KaCarrierInterfaceSettings.TABLE_NAME & ".interface_id " &
								"FROM " & KaCarrierInterfaceSettings.TABLE_NAME & " " &
								"WHERE (deleted=0) " &
									"AND (" & KaCarrierInterfaceSettings.TABLE_NAME & "." & KaCarrierInterfaceSettings.FN_CARRIER_ID & " = " & Q(ddlCarriers.SelectedValue) & ") " &
								"AND (" & KaCarrierInterfaceSettings.TABLE_NAME & "." & KaCarrierInterfaceSettings.FN_CARRIER_ID & " <> " & Q(Guid.Empty) & ")))) " &
					"ORDER BY interfaces.name")
		Do While getInterfaceRdr.Read
			ddlInterface.Items.Add(New ListItem(getInterfaceRdr.Item("name"), getInterfaceRdr.Item("id").ToString()))
		Loop
		getInterfaceRdr.Close()
		pnlInterfaceSetup.Visible = (ddlInterface.Items.Count > 1) AndAlso ddlCarriers.SelectedValue <> Guid.Empty.ToString
	End Sub

	Private Sub SaveInterface()
		lblStatus.Text = ""
		If Guid.Parse(ddlCarriers.SelectedValue) = Guid.Empty Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidCarrierNotSaved", Utilities.JsAlert("You must save the carrier before you can set up interface cross references.")) : Exit Sub
		If Guid.Parse(ddlInterface.SelectedValue) = Guid.Empty Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidInterface", Utilities.JsAlert("An interface must be selected. Interface settings not saved.")) : Exit Sub
		If tbxInterfaceCrossReference.Text.Length = 0 Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidCrossReference", Utilities.JsAlert("A cross reference must be specified. Interface settings not saved.")) : Exit Sub

		' If this is not export only, check if there are any other settings with the same cross reference ID
		If Not chkExportOnly.Checked Then
			Dim allInterfaceSettings As ArrayList = KaCarrierInterfaceSettings.GetAll(GetUserConnection(_currentUser.Id), KaCarrierInterfaceSettings.FN_CROSS_REFERENCE & " = " & Q(tbxInterfaceCrossReference.Text.Trim) &
																							" AND " & KaCarrierInterfaceSettings.FN_INTERFACE_ID & " = " & Q(ddlInterface.SelectedValue) &
																							" AND " & KaCarrierInterfaceSettings.FN_DELETED & " = 0 " &
																							" AND " & KaCarrierInterfaceSettings.FN_EXPORT_ONLY & " = 0 " &
																							" AND " & KaCarrierInterfaceSettings.FN_ID & " <> " & Q(ddlCarrierInterface.SelectedValue), "")
			If allInterfaceSettings.Count > 0 Then
				ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidCrossReferenceExists", Utilities.JsAlert("A cross reference of " & tbxInterfaceCrossReference.Text.Trim & " already exists for this interface."))
				Exit Sub
			End If
		End If

		Dim carrierInterfaceId As Guid = Guid.Parse(ddlCarrierInterface.SelectedValue)
		If carrierInterfaceId = Guid.Empty Then
			Dim carrierInterface As KaCarrierInterfaceSettings = New KaCarrierInterfaceSettings
			carrierInterface.CarrierId = Guid.Parse(ddlCarriers.SelectedValue)
			carrierInterface.InterfaceId = Guid.Parse(ddlInterface.SelectedValue)
			carrierInterface.CrossReference = tbxInterfaceCrossReference.Text.Trim
			carrierInterface.DefaultSetting = chkDefaultSetting.Checked
			carrierInterface.ExportOnly = chkExportOnly.Checked
			carrierInterface.SqlInsert(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
			carrierInterfaceId = carrierInterface.Id
		Else
			Dim carrierInterface As KaCarrierInterfaceSettings = New KaCarrierInterfaceSettings(GetUserConnection(_currentUser.Id), carrierInterfaceId)
			carrierInterface.CarrierId = Guid.Parse(ddlCarriers.SelectedValue)
			carrierInterface.InterfaceId = Guid.Parse(ddlInterface.SelectedValue)
			carrierInterface.CrossReference = tbxInterfaceCrossReference.Text.Trim
			carrierInterface.DefaultSetting = chkDefaultSetting.Checked
			carrierInterface.ExportOnly = chkExportOnly.Checked
			carrierInterface.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
		End If

		PopulateCarrierInterfaceList(Guid.Parse(ddlCarriers.SelectedValue))
		ddlCarrierInterface.SelectedValue = carrierInterfaceId.ToString
		ddlcarrierInterface_SelectedIndexChanged(ddlCarrierInterface, New EventArgs)
		btnRemoveInterface.Enabled = True
	End Sub

	Private Sub RemoveInterface()
		Dim selectedId As Guid = Guid.Parse(ddlCarrierInterface.SelectedValue)
		If selectedId <> Guid.Empty Then
			Dim prodInterfaceSetting As KaCarrierInterfaceSettings = New KaCarrierInterfaceSettings(GetUserConnection(_currentUser.Id), selectedId)
			prodInterfaceSetting.Deleted = True
			prodInterfaceSetting.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
		End If
		PopulateCarrierInterfaceList(Guid.Parse(ddlCarriers.SelectedValue))
		btnRemoveInterface.Enabled = PopulateInterfaceInformation(Guid.Empty)
	End Sub

	Private Sub DeleteInterfaceInformation(ByVal carrierId As Guid)
		For Each r As KaCarrierInterfaceSettings In KaCarrierInterfaceSettings.GetAll(GetUserConnection(_currentUser.Id), KaCarrierInterfaceSettings.FN_DELETED & " = 0 and " & KaCarrierInterfaceSettings.FN_CARRIER_ID & " = " & Q(carrierId), "")
			r.Deleted = True
			r.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
		Next
	End Sub

	Private Sub PopulateCarrierInterfaceList(ByVal carrierId As Guid)
		PopulateInterfaceList()
		ddlCarrierInterface.Items.Clear()
		ddlCarrierInterface.Items.Add(New ListItem(IIf(ddlInterface.Items.Count > 1, "Add an interface", ""), Guid.Empty.ToString))
		Dim getInterfaceRdr As OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), "SELECT " & KaCarrierInterfaceSettings.TABLE_NAME & ".id, interfaces.name, " & KaCarrierInterfaceSettings.TABLE_NAME & ".cross_reference " &
					"FROM " & KaCarrierInterfaceSettings.TABLE_NAME & " " &
					"INNER JOIN interfaces ON " & KaCarrierInterfaceSettings.TABLE_NAME & ".interface_id = interfaces.id " &
					"INNER JOIN interface_types ON interfaces.interface_type_id = interface_types.id " &
					"WHERE (" & KaCarrierInterfaceSettings.TABLE_NAME & ".deleted = 0) " &
						"AND (interfaces.deleted = 0) " &
						"AND (interface_types.deleted = 0) " &
						"AND (" & KaCarrierInterfaceSettings.TABLE_NAME & "." & KaCarrierInterfaceSettings.FN_CARRIER_ID & "=" & Q(carrierId) & ") " &
					"ORDER BY interfaces.name, " & KaCarrierInterfaceSettings.TABLE_NAME & ".cross_reference")
		Do While getInterfaceRdr.Read
			ddlCarrierInterface.Items.Add(New ListItem(getInterfaceRdr.Item("name") & " (" & getInterfaceRdr.Item("cross_reference") & ")", getInterfaceRdr.Item("id").ToString()))
		Loop
		getInterfaceRdr.Close()
		ddlCarrierInterface.SelectedIndex = 0
		ddlcarrierInterface_SelectedIndexChanged(ddlCarrierInterface, New EventArgs())
	End Sub

	Private Function PopulateInterfaceInformation(ByVal carrierInterfaceId As Guid) As Boolean
		Dim retval As Boolean = False
		If carrierInterfaceId <> Guid.Empty Then
			Dim carrierInterfaceSetting As KaCarrierInterfaceSettings = New KaCarrierInterfaceSettings(GetUserConnection(_currentUser.Id), carrierInterfaceId)
			ddlInterface.SelectedValue = carrierInterfaceSetting.InterfaceId.ToString
			tbxInterfaceCrossReference.Text = carrierInterfaceSetting.CrossReference
			chkDefaultSetting.Checked = carrierInterfaceSetting.DefaultSetting
			chkExportOnly.Checked = carrierInterfaceSetting.ExportOnly
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

	Protected Sub ddlcarrierInterface_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles ddlCarrierInterface.SelectedIndexChanged
		btnRemoveInterface.Enabled = PopulateInterfaceInformation(Guid.Parse(ddlCarrierInterface.SelectedValue))
	End Sub

	Private Sub ddlInterface_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles ddlInterface.SelectedIndexChanged
		If Guid.Parse(ddlCarrierInterface.SelectedValue) = Guid.Empty Then
			'Only do this check if we are a new interface setting
			Dim count As Integer = 0
			Try
				Dim rdr As OleDb.OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), "SELECT count(*) " &
																			 "FROM " & KaCarrierInterfaceSettings.TABLE_NAME & " " &
																			 "WHERE " & KaCarrierInterfaceSettings.FN_DELETED & " = 0 " &
																			 "AND " & KaCarrierInterfaceSettings.FN_INTERFACE_ID & " = " & Q(Guid.Parse(ddlInterface.SelectedValue)) & " " &
																			 "AND " & KaCarrierInterfaceSettings.FN_CARRIER_ID & " = " & Q(Guid.Parse(ddlCarriers.SelectedValue)))
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
			Dim shouldEnable = (.Edit AndAlso ddlCarriers.SelectedIndex > 0) OrElse (.Create AndAlso ddlCarriers.SelectedIndex = 0)
			pnlEven.Enabled = shouldEnable
			pnlInterfaceSettings.Enabled = shouldEnable
			btnSave.Enabled = shouldEnable
			btnDelete.Enabled = Not Guid.Parse(ddlCarriers.SelectedValue).Equals(Guid.Empty) AndAlso .Edit AndAlso .Delete
		End With
	End Sub
End Class