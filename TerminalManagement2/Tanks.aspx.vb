Imports KahlerAutomation.KaTm2Database
Imports System.Environment
Imports System.Data.OleDb

Public Class Tanks : Inherits System.Web.UI.Page
	Private _currentUser As KaUser
	Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing

#Region "Events"
	Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
		Response.Cache.SetCacheability(HttpCacheability.NoCache)
		_currentUser = Utilities.GetUser(Me)
		_currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaTank.TABLE_NAME}), "Tanks")
		If Not _currentUserPermission(KaTank.TABLE_NAME).Read Then Response.Redirect("Welcome.aspx")
		If Not Page.IsPostBack Then
			SetTextboxMaxLengths()
			PopulateTankList()
			PopulateOwnerList()
			PopulateLocationList()
			PopulateBulkProductList()
			PopulatePanelList()
			PopulateSensorList()
			PopulateUnitList()
			If Page.Request("TankId") IsNot Nothing Then
				Try
					ddlTanks.SelectedValue = Page.Request("TankId")
				Catch ex As ArgumentOutOfRangeException
					ddlTanks.SelectedIndex = 0
				End Try
			End If
			ddlTanks_SelectedIndexChanged(ddlTanks, New EventArgs())
			SetControlUsabilityFromPermissions()
			Utilities.ConfirmBox(Me.btnDelete, "Are you sure you want to delete this tank?")
			Utilities.SetFocus(tbxName, Me)
		End If
	End Sub

	Protected Sub btnDelete_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnDelete.Click
		lblStatus.Text = ""
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		With New KaTank(connection, Guid.Parse(ddlTanks.SelectedValue))
			.Deleted = True
			.SqlUpdate(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
			PopulateTankList() ' update the list (which will remove this tank from the list)
			ddlTanks.SelectedValue = Guid.Empty.ToString() ' set the list selection to "enter a new tank"
			PopulateTankInformation(Guid.Empty) ' clear the tank fields
			lblStatus.Text = "Tank successfully deleted." ' let the user know that the tank was deleted
		End With
	End Sub

	Protected Sub btnSave_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnSave.Click
		lblStatus.Text = ""
		If ValidateFields() Then
			Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
			With New KaTank()
				.Id = Guid.Parse(ddlTanks.SelectedValue)
				If .Id <> Guid.Empty Then .SqlSelect(connection) ' if we're updating the tank, get the existing record
				.Name = tbxName.Text
				.OwnerId = Guid.Parse(ddlOwner.SelectedValue)
				.LocationId = Guid.Parse(ddlLocation.SelectedValue)
				.BulkProductId = Guid.Parse(ddlBulkProduct.SelectedValue)
				.PanelId = Guid.Parse(ddlPanel.SelectedValue)
				.Sensor = ddlSensor.SelectedValue
				.EmailRecipients = tbxEmail.Text
				.UnitId = Guid.Parse(ddlUnit.SelectedValue)
				Dim status As String = ""
				If .Id = Guid.Empty Then ' we're creating a new tank
					.SqlInsert(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
					btnDelete.Enabled = True
					status = "Tank successfully added."
				Else ' we're updating an existing tank
					.SqlUpdate(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
					status = "Tank successfully updated."
				End If
				PopulateTankList() ' make sure the addition of the tank or changes to the tank's name are shown in the tank list
				ddlTanks.SelectedValue = .Id.ToString() ' make sure the tank updated or created is selected in the tank list
				ddlTanks_SelectedIndexChanged(ddlTanks, New EventArgs())
				lblStatus.Text = status
			End With
		End If
	End Sub

	Private Function ValidateFields() As Boolean
		If tbxName.Text.Trim().Length = 0 Then ' name is blank
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidName", Utilities.JsAlert("Name must be specified."))
			Return False
		End If
		Dim conditions As String = String.Format("deleted=0 AND name={0} AND id<>{1}", Q(tbxName.Text), Q(ddlTanks.SelectedValue))
		If KaTank.GetAll(GetUserConnection(_currentUser.Id), conditions, "").Count > 0 Then ' the name has already been used
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidDuplicateName", Utilities.JsAlert("A tank with name """ & tbxName.Text & """ already exists. Please enter a unique name for this tank."))
			Return False
		End If
		Dim message As String = ""
		If Not Utilities.IsEmailFieldValid(tbxEmail.Text, message) Then ' the e-mail field is not formatted correctly
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidEmail", Utilities.JsAlert(message))
			Return False
		End If
		Return True
	End Function

	Protected Sub ddlTanks_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles ddlTanks.SelectedIndexChanged
		Dim tankId As Guid = Guid.Parse(ddlTanks.SelectedValue)
		PopulateTankInformation(tankId)
		PopulateTankInterfaceList(tankId)
		SetControlUsabilityFromPermissions()
	End Sub

	Protected Sub lbtConfigure_Click(ByVal sender As Object, ByVal e As EventArgs) Handles lbtConfigure.Click
		With New KaPanel(GetUserConnection(_currentUser.Id), Guid.Parse(ddlPanel.SelectedValue))
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "ConfigureTank", Utilities.JsWindowOpen("http://" & .IpAddress))
		End With
	End Sub

	Protected Sub ddlPanel_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles ddlPanel.SelectedIndexChanged
		Dim panelId As Guid = Guid.Parse(ddlPanel.SelectedValue)
		lbtConfigure.Visible = panelId <> Guid.Empty AndAlso New KaPanel(GetUserConnection(_currentUser.Id), panelId).ConnectionType = KaPanel.PanelConnectionType.Ethernet
	End Sub
#End Region

	Private Sub PopulateTankList()
		ddlTanks.Items.Clear()
		If _currentUserPermission(KaTank.TABLE_NAME).Create Then
			ddlTanks.Items.Add(New ListItem("Enter a new tank", Guid.Empty.ToString()))
		Else
			ddlTanks.Items.Add(New ListItem("Select a tank", Guid.Empty.ToString()))
		End If
		For Each r As KaTank In KaTank.GetAll(GetUserConnection(_currentUser.Id), "deleted=0" & IIf(_currentUser.OwnerId = Guid.Empty, "", " AND owner_id=" & Q(_currentUser.OwnerId)), "name ASC")
			ddlTanks.Items.Add(New ListItem(r.Name, r.Id.ToString()))
		Next
	End Sub

	Private Sub PopulateOwnerList()
		ddlOwner.Items.Clear()
		If _currentUser.OwnerId = Guid.Empty Then ddlOwner.Items.Add(New ListItem("All owners", Guid.Empty.ToString()))
		For Each r As KaOwner In KaOwner.GetAll(GetUserConnection(_currentUser.Id), "deleted=0" & IIf(_currentUser.OwnerId = Guid.Empty, "", " AND id=" & Q(_currentUser.OwnerId)), "name ASC")
			ddlOwner.Items.Add(New ListItem(r.Name, r.Id.ToString()))
		Next
	End Sub

	Private Sub PopulateLocationList()
		ddlLocation.Items.Clear()
		ddlLocation.Items.Add(New ListItem("", Guid.Empty.ToString()))
		For Each r As KaLocation In KaLocation.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
			ddlLocation.Items.Add(New ListItem(r.Name, r.Id.ToString()))
		Next
	End Sub

	Private Sub PopulateBulkProductList()
		ddlBulkProduct.Items.Clear()
		ddlBulkProduct.Items.Add(New ListItem("", Guid.Empty.ToString()))
		Dim bulkProdWhere As String = ""
		Dim selectedOwnerId As Guid = Guid.Parse(ddlOwner.SelectedValue)
		If selectedOwnerId <> Guid.Empty Then
			bulkProdWhere = " and (owner_id = " & Q(selectedOwnerId) & " or owner_id = " & Q(Guid.Empty) & ")"
		End If
		For Each r As KaBulkProduct In KaBulkProduct.GetAll(GetUserConnection(_currentUser.Id), "deleted = " & Q(False) & bulkProdWhere, "name ASC")
			If Not r.IsFunction(GetUserConnection(_currentUser.Id)) Then
				ddlBulkProduct.Items.Add(New ListItem(r.Name, r.Id.ToString()))
			End If
		Next
	End Sub

	Private Sub PopulatePanelList()
		ddlPanel.Items.Clear()
		ddlPanel.Items.Add(New ListItem("", Guid.Empty.ToString()))
		For Each r As KaPanel In KaPanel.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
			ddlPanel.Items.Add(New ListItem(r.Name, r.Id.ToString()))
		Next
	End Sub

	Private Sub PopulateSensorList()
		ddlSensor.Items.Clear()
		For i As Integer = 0 To 31
			ddlSensor.Items.Add(New ListItem(i + 1, i))
		Next
	End Sub

	Private Sub PopulateUnitList()
		ddlUnit.Items.Clear()
		ddlUnit.Items.Add(New ListItem("", Guid.Empty.ToString()))
		For Each r As KaUnit In KaUnit.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND base_unit<>9", "name ASC")
			ddlUnit.Items.Add(New ListItem(r.Name, r.Id.ToString()))
		Next
	End Sub

	Private Sub PopulateTankInformation(ByVal tankId As Guid)
		lblStatus.Text = ""
		With New KaTank()
			.Id = tankId
			If .Id <> Guid.Empty Then
				.SqlSelect(GetUserConnection(_currentUser.Id))
			ElseIf _currentUser.OwnerId <> Guid.Empty Then
				.OwnerId = _currentUser.OwnerId
			End If
			tbxName.Text = .Name
			Try
				ddlOwner.SelectedValue = .OwnerId.ToString()
			Catch ex As ArgumentOutOfRangeException
				ddlOwner.SelectedValue = Utilities.GetUser(Me).OwnerId.ToString()
				ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidOwnerId", Utilities.JsAlert("Record not found in owners where ID = " & .OwnerId.ToString() & ". Owner not set."))
			End Try
			Try
				ddlLocation.SelectedValue = .LocationId.ToString()
			Catch ex As ArgumentOutOfRangeException
				ddlLocation.SelectedValue = Guid.Empty.ToString()
				ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidFacilityId", Utilities.JsAlert("Record not found in facilities where ID = " & .LocationId.ToString() & ". Facility not set."))
			End Try
			Try
				ddlBulkProduct.SelectedValue = .BulkProductId.ToString()
			Catch ex As ArgumentOutOfRangeException
				ddlBulkProduct.SelectedValue = Guid.Empty.ToString()
				ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidBulkProductId", Utilities.JsAlert("Record not found in bulk products where ID = " & .BulkProductId.ToString() & ". Bulk product not set."))
			End Try
			Try
				ddlPanel.SelectedValue = .PanelId.ToString()
				lbtConfigure.Visible = .PanelId <> Guid.Empty AndAlso New KaPanel(GetUserConnection(_currentUser.Id), .PanelId).ConnectionType = KaPanel.PanelConnectionType.Ethernet
			Catch ex As ArgumentOutOfRangeException
				ddlPanel.SelectedValue = Guid.Empty.ToString()
				ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidPanelId", Utilities.JsAlert("Record not found in panels where ID = " & .PanelId.ToString() & ". Panel not set."))
			End Try
			Try
				ddlSensor.SelectedValue = .Sensor
			Catch ex As ArgumentOutOfRangeException
				ddlSensor.SelectedValue = "1"
				ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidSensorIndex", Utilities.JsAlert("Invalid sensor index (" & .Sensor & "). Sensor set to ""1""."))
			End Try
			Try
				ddlUnit.SelectedValue = .UnitId.ToString()
			Catch ex As ArgumentOutOfRangeException
				ddlUnit.SelectedValue = Guid.Empty.ToString()
				ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidUnitId", Utilities.JsAlert("Record not found in units where ID = " & .UnitId.ToString() & ". Unit not set."))
			End Try
			tbxEmail.Text = .EmailRecipients
			If .LastRead > New DateTime(1900, 1, 1) Then
				lblLastUpdated.Text = String.Format("{0:G}", .LastRead)
				lblAlarms.Text = .StatusAlarmString()
				If lblAlarms.Text.Length = 0 Then lblAlarms.Text = "None"
				lblLevel.Text = KaTank.InchesToFeetAndInches(.Level)
				Dim displayUnit As KaUnit
				Try
					displayUnit = New KaUnit(GetUserConnection(_currentUser.Id), .UnitId)
				Catch ex As RecordNotFoundException
					displayUnit = New KaUnit(GetUserConnection(_currentUser.Id), KaUnit.GetSystemDefaultVolumeUnitOfMeasure(GetUserConnection(_currentUser.Id), Nothing))
				End Try
				lblQuantity.Text = String.Format("{0:" & displayUnit.UnitPrecision & "} {1}", .Quantity, displayUnit.Abbreviation)
				lblCapacity.Text = String.Format("{0:" & displayUnit.UnitPrecision & "} {1}", .Capacity, displayUnit.Abbreviation)
				lblMaxHeight.Text = KaTank.InchesToFeetAndInches(.Height)
				lblTemperature.Text = String.Format("{0:0.0}°", .Temperature)
				pnlLastReportedValues.Visible = True
			Else
				pnlLastReportedValues.Visible = False
			End If
		End With
	End Sub

	Private Sub ddlOwner_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlOwner.SelectedIndexChanged
		PopulateBulkProductList()
	End Sub

	Private Sub SetTextboxMaxLengths()
		tbxEmail.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaTank.TABLE_NAME, "email_recipients"))
		tbxName.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaTank.TABLE_NAME, "name"))
	End Sub

	Private Sub SetControlUsabilityFromPermissions()
		With _currentUserPermission(KaTank.TABLE_NAME)
			Dim shouldEnable = (.Edit AndAlso ddlTanks.SelectedIndex > 0) OrElse (.Create AndAlso ddlTanks.SelectedIndex = 0)
			pnlEven.Enabled = shouldEnable
			pnlInterfaceSettings.Enabled = shouldEnable
			btnSave.Enabled = shouldEnable
			Dim value = Guid.Parse(ddlTanks.SelectedValue)
			btnDelete.Enabled = .Edit AndAlso .Delete AndAlso value <> Guid.Empty
		End With
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
					"AND ((" & KaInterfaceTypes.FN_SHOW_TANKS_INTERFACE & " = 1) " &
					"OR (interfaces.id IN (SELECT " & KaTankInterfaceSettings.TABLE_NAME & ".interface_id " &
											"FROM " & KaTankInterfaceSettings.TABLE_NAME & " " &
											"WHERE (deleted=0) " &
												"AND (" & KaTankInterfaceSettings.TABLE_NAME & "." & KaTankInterfaceSettings.FN_TANK_ID & " = " & Q(ddlTanks.SelectedValue) & ") " &
												"AND (" & KaTankInterfaceSettings.TABLE_NAME & "." & KaTankInterfaceSettings.FN_TANK_ID & " <> " & Q(Guid.Empty) & ")))) " &
				"ORDER BY interfaces.name")
		Do While getInterfaceRdr.Read
			ddlInterface.Items.Add(New ListItem(getInterfaceRdr.Item("name"), getInterfaceRdr.Item("id").ToString()))
		Loop
		getInterfaceRdr.Close()
		pnlInterfaceSetup.Visible = (ddlInterface.Items.Count > 1) AndAlso ddlTanks.SelectedValue <> Guid.Empty.ToString
	End Sub

	Private Sub SaveInterface()
		lblStatus.Text = ""
		If Guid.Parse(ddlTanks.SelectedValue) = Guid.Empty Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidTankNotSaved", Utilities.JsAlert("You must save the tank before you can set up interface cross references.")) : Exit Sub
		If Guid.Parse(ddlInterface.SelectedValue) = Guid.Empty Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidInterface", Utilities.JsAlert("An interface must be selected. Interface settings not saved.")) : Exit Sub
		If tbxInterfaceCrossReference.Text.Length = 0 Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidCrossReference", Utilities.JsAlert("A cross reference must be specified. Interface settings not saved.")) : Exit Sub

		' If this is not export only, check if there are any other settings with the same cross reference ID
		If Not chkExportOnly.Checked Then
			Dim allInterfaceSettings As ArrayList = KaTankInterfaceSettings.GetAll(GetUserConnection(_currentUser.Id), KaTankInterfaceSettings.FN_CROSS_REFERENCE & " = " & Q(tbxInterfaceCrossReference.Text.Trim) &
																				   " AND " & KaTankInterfaceSettings.FN_INTERFACE_ID & " = " & Q(ddlInterface.SelectedValue) &
																				   " AND " & KaTankInterfaceSettings.FN_DELETED & " = 0 " &
																				   " AND " & KaTankInterfaceSettings.FN_EXPORT_ONLY & " = 0 " &
																				   " AND " & KaTankInterfaceSettings.FN_ID & " <> " & Q(ddlTankInterface.SelectedValue), "")
			If allInterfaceSettings.Count > 0 Then
				ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidCrossReferenceExists", Utilities.JsAlert("A cross reference of " & tbxInterfaceCrossReference.Text.Trim & " already exists for this interface."))
				Exit Sub
			End If
		End If

		Dim tankInterfaceId As Guid = Guid.Parse(ddlTankInterface.SelectedValue)
		If tankInterfaceId = Guid.Empty Then
			Dim tankInterface As KaTankInterfaceSettings = New KaTankInterfaceSettings
			tankInterface.TankId = Guid.Parse(ddlTanks.SelectedValue)
			tankInterface.InterfaceId = Guid.Parse(ddlInterface.SelectedValue)
			tankInterface.CrossReference = tbxInterfaceCrossReference.Text.Trim
			tankInterface.DefaultSetting = chkDefaultSetting.Checked
			tankInterface.ExportOnly = chkExportOnly.Checked
			tankInterface.SqlInsert(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
			tankInterfaceId = tankInterface.Id
		Else
			Dim tankInterface As KaTankInterfaceSettings = New KaTankInterfaceSettings(GetUserConnection(_currentUser.Id), tankInterfaceId)
			tankInterface.TankId = Guid.Parse(ddlTanks.SelectedValue)
			tankInterface.InterfaceId = Guid.Parse(ddlInterface.SelectedValue)
			tankInterface.CrossReference = tbxInterfaceCrossReference.Text.Trim
			tankInterface.DefaultSetting = chkDefaultSetting.Checked
			tankInterface.ExportOnly = chkExportOnly.Checked
			tankInterface.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
		End If

		PopulateTankInterfaceList(Guid.Parse(ddlTanks.SelectedValue))
		ddlTankInterface.SelectedValue = tankInterfaceId.ToString
		ddlTankInterface_SelectedIndexChanged(ddlTankInterface, New EventArgs)
		btnRemoveInterface.Enabled = True
	End Sub

	Private Sub RemoveInterface()
		Dim selectedId As Guid = Guid.Parse(ddlTankInterface.SelectedValue)
		If selectedId <> Guid.Empty Then
			Dim prodInterfaceSetting As KaTankInterfaceSettings = New KaTankInterfaceSettings(GetUserConnection(_currentUser.Id), selectedId)
			prodInterfaceSetting.Deleted = True
			prodInterfaceSetting.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
		End If
		PopulateTankInterfaceList(Guid.Parse(ddlTanks.SelectedValue))
		btnRemoveInterface.Enabled = PopulateInterfaceInformation(Guid.Empty)
	End Sub

	Private Sub DeleteInterfaceInformation(ByVal tankId As Guid)
		For Each r As KaTankInterfaceSettings In KaTankInterfaceSettings.GetAll(GetUserConnection(_currentUser.Id), KaTankInterfaceSettings.FN_DELETED & " = 0 and " & KaTankInterfaceSettings.FN_TANK_ID & " = " & Q(tankId), "")
			r.Deleted = True
			r.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
		Next
	End Sub

	Private Sub PopulateTankInterfaceList(ByVal tankId As Guid)
		PopulateInterfaceList()
		ddlTankInterface.Items.Clear()
		ddlTankInterface.Items.Add(New ListItem(IIf(ddlInterface.Items.Count > 1, "Add an interface", ""), Guid.Empty.ToString))
		Dim getInterfaceRdr As OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), "SELECT " & KaTankInterfaceSettings.TABLE_NAME & ".id, interfaces.name, " & KaTankInterfaceSettings.TABLE_NAME & ".cross_reference " &
				"FROM " & KaTankInterfaceSettings.TABLE_NAME & " " &
				"INNER JOIN interfaces ON " & KaTankInterfaceSettings.TABLE_NAME & ".interface_id = interfaces.id " &
				"INNER JOIN interface_types ON interfaces.interface_type_id = interface_types.id " &
				"WHERE (" & KaTankInterfaceSettings.TABLE_NAME & ".deleted = 0) " &
					"AND (interfaces.deleted = 0) " &
					"AND (interface_types.deleted = 0) " &
					"AND (" & KaTankInterfaceSettings.TABLE_NAME & "." & KaTankInterfaceSettings.FN_TANK_ID & "=" & Q(tankId) & ") " &
				"ORDER BY interfaces.name, " & KaTankInterfaceSettings.TABLE_NAME & ".cross_reference")
		Do While getInterfaceRdr.Read
			ddlTankInterface.Items.Add(New ListItem(getInterfaceRdr.Item("name") & " (" & getInterfaceRdr.Item("cross_reference") & ")", getInterfaceRdr.Item("id").ToString()))
		Loop
		getInterfaceRdr.Close()
	End Sub

	Private Function PopulateInterfaceInformation(ByVal tankInterfaceId As Guid) As Boolean
		Dim retval As Boolean = False
		If tankInterfaceId <> Guid.Empty Then
			Dim tankInterfaceSetting As KaTankInterfaceSettings = New KaTankInterfaceSettings(GetUserConnection(_currentUser.Id), tankInterfaceId)
			ddlInterface.SelectedValue = tankInterfaceSetting.InterfaceId.ToString
			tbxInterfaceCrossReference.Text = tankInterfaceSetting.CrossReference
			chkDefaultSetting.Checked = tankInterfaceSetting.DefaultSetting
			chkExportOnly.Checked = tankInterfaceSetting.ExportOnly
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

	Protected Sub ddlTankInterface_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles ddlTankInterface.SelectedIndexChanged
		btnRemoveInterface.Enabled = PopulateInterfaceInformation(Guid.Parse(ddlTankInterface.SelectedValue))
	End Sub

	Private Sub ddlInterface_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles ddlInterface.SelectedIndexChanged
		If Guid.Parse(ddlTankInterface.SelectedValue) = Guid.Empty Then
			'Only do this check if we are a new interface setting
			Dim count As Integer = 0
			Try
				Dim rdr As OleDb.OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), "SELECT count(*) " &
																			 "FROM " & KaTankInterfaceSettings.TABLE_NAME & " " &
																			 "WHERE " & KaTankInterfaceSettings.FN_DELETED & " = 0 " &
																			 "AND " & KaTankInterfaceSettings.FN_INTERFACE_ID & " = " & Q(Guid.Parse(ddlInterface.SelectedValue)) & " " &
																			 "AND " & KaTankInterfaceSettings.FN_TANK_ID & " = " & Q(Guid.Parse(ddlTanks.SelectedValue)))
				If rdr.Read Then count = rdr.Item(0)
			Catch ex As Exception

			End Try
			chkDefaultSetting.Checked = (count = 0)
		End If
	End Sub
#End Region
End Class