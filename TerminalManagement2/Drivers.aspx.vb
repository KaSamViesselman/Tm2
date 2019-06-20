Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class Drivers : Inherits System.Web.UI.Page
	Private _currentUser As KaUser
	Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
	Private _currentTableName As String = KaDriver.TABLE_NAME
	Private _customFields As New List(Of KaCustomField)
	Private _customFieldData As New List(Of KaCustomFieldData)

#Region "Events"
	Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load, Me.Load
		Response.Cache.SetCacheability(HttpCacheability.NoCache)
		_currentUser = Utilities.GetUser(Me)
		_currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Drivers")
		If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")

		If Not Page.IsPostBack Then
			SetTextboxMaxLengths()
			_customFields.Clear()
			For Each customField As KaCustomField In KaCustomField.GetAll(GetUserConnection(_currentUser.Id), String.Format("deleted=0 AND {0} = {1}", KaCustomField.FN_TABLE_NAME, Q(KaDriver.TABLE_NAME)), KaCustomField.FN_FIELD_NAME)
				_customFields.Add(customField)
			Next
			Dim driverId As Guid = Guid.Empty
			Guid.TryParse(Page.Request("DriverId"), driverId)
			PopulateDriversList(driverId)
			PopulateAccountsCombo()
			PopulateFacilitiesCombo()
			pnlWarning.Visible = False
			SetControlUsabilityFromPermissions()
			ddlDrivers_SelectedIndexChanged(ddlDrivers, New EventArgs())
			Utilities.ConfirmBox(Me.btnDelete, "Are you sure you want to delete this driver?")
			Utilities.SetFocus(tbxNumber, Me)
		End If
	End Sub

	Protected Sub btnAddAccount_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnAddAccount.Click
		If ddlAccounts.SelectedIndex <> -1 And ddlAccounts.SelectedIndex <> 0 Then
			If AccountListContains(ddlAccounts.SelectedItem.Text) Then
				Exit Sub
			End If
			lstAccount.Items.Add(New ListItem(ddlAccounts.SelectedItem.Text, ddlAccounts.SelectedItem.Value))
			lstAccount.SelectedIndex = lstAccount.Items.Count - 1
			btnDeleteFromAccount.Enabled = True
			btnDeleteAllAccounts.Enabled = True
		End If
	End Sub

	Protected Sub btnAddAllAccounts_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnAddAllAccounts.Click
		For Each li As ListItem In ddlAccounts.Items
			If li.Text = "Accounts" Then
				Continue For
			Else
				If AccountListContains(li.Text) Then
					Continue For
				Else
					lstAccount.Items.Add(New ListItem(li.Text, li.Value))
					lstAccount.SelectedIndex = lstAccount.Items.Count - 1
				End If
				btnDeleteFromAccount.Enabled = True
				btnDeleteAllAccounts.Enabled = True
			End If
		Next
	End Sub

	Protected Sub btnAddFacility_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnAddFacility.Click
		If ddlFacilities.SelectedIndex <> -1 And ddlFacilities.SelectedIndex <> 0 Then
			If FacilityListContains(ddlFacilities.SelectedItem.Text) Then
				Exit Sub
			End If
			lstFacility.Items.Add(New ListItem(ddlFacilities.SelectedItem.Text, ddlFacilities.SelectedItem.Value))
			lstFacility.SelectedIndex = lstFacility.Items.Count - 1
			btnDeleteFromFacility.Enabled = True
			btnDeleteAllFacilities.Enabled = True
		End If
	End Sub

	Protected Sub btnAddAllFacilities_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnAddAllFacilities.Click
		For Each li As ListItem In ddlFacilities.Items
			If li.Text = "Facilities" Then
				Continue For
			Else
				If FacilityListContains(li.Text) Then
					Continue For
				Else
					lstFacility.Items.Add(New ListItem(li.Text, li.Value))
					lstFacility.SelectedIndex = lstFacility.Items.Count - 1
				End If
				btnDeleteFromFacility.Enabled = True
				btnDeleteAllFacilities.Enabled = True
			End If
		Next
	End Sub

	Private Sub btnDelete_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDelete.Click
		lblStatus.Text = ""
		If Guid.Parse(ddlDrivers.SelectedValue) <> Guid.Empty Then
			With New KaDriver(GetUserConnection(_currentUser.Id), Guid.Parse(ddlDrivers.SelectedValue))
				.Deleted = True
				.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
				PopulateDriversList(Guid.Empty)
				lblStatus.Text = "Selected driver deleted successfully"
				btnDelete.Enabled = False
			End With
		End If
	End Sub

	Protected Sub btnDeleteAllAccounts_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnDeleteAllAccounts.Click
		lstAccount.Items.Clear()
		btnDeleteFromAccount.Enabled = False
		btnDeleteAllAccounts.Enabled = False
	End Sub

	Protected Sub btnDeleteFromAccount_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnDeleteFromAccount.Click
		If lstAccount.SelectedIndex <> -1 Then
			lstAccount.Items.RemoveAt(lstAccount.SelectedIndex)
			lstAccount.SelectedIndex = -1
			btnDeleteFromAccount.Enabled = False
			If lstAccount.Items.Count = 0 Then
				btnDeleteAllAccounts.Enabled = False
			End If
		End If
	End Sub

	Protected Sub btnDeleteAllFacilities_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnDeleteAllFacilities.Click
		lstFacility.Items.Clear()
		btnDeleteFromFacility.Enabled = False
		btnDeleteAllFacilities.Enabled = False
	End Sub

	Protected Sub btnDeleteFromFacility_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnDeleteFromFacility.Click
		If lstFacility.SelectedIndex <> -1 Then
			lstFacility.Items.RemoveAt(lstFacility.SelectedIndex)
			lstFacility.SelectedIndex = -1
			btnDeleteFromFacility.Enabled = False
			If lstFacility.Items.Count = 0 Then
				btnDeleteAllFacilities.Enabled = False
			End If
		End If
	End Sub

	Protected Sub btnNo_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnNo.Click
		Response.Redirect("Drivers.aspx")
	End Sub

	Protected Sub btnSave_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnSave.Click
		If ValidateFields() Then
			SaveDriver()
		End If
	End Sub

	Protected Sub btnYes_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnYes.Click
		SaveDriver()
		pnlWarning.Visible = False
		pnlMain.Visible = True
	End Sub

	Protected Sub chkValidForAllAccounts_CheckedChanged(sender As Object, e As EventArgs) Handles chkValidForAllAccounts.CheckedChanged
		Dim notAllAccounts As Boolean = Not chkValidForAllAccounts.Checked
		rowAddToAccount.Visible = notAllAccounts
		btnAddAllAccounts.Visible = notAllAccounts
		btnDeleteFromAccount.Visible = notAllAccounts
		btnDeleteAllAccounts.Visible = notAllAccounts
		lstAccount.Visible = notAllAccounts
	End Sub

	Protected Sub chkValidForAllFacilities_CheckedChanged(sender As Object, e As EventArgs) Handles chkValidForAllFacilities.CheckedChanged
		Dim notAllFacilities As Boolean = Not chkValidForAllFacilities.Checked
		rowAddtoFacility.Visible = notAllFacilities
		btnAddAllFacilities.Visible = notAllFacilities
		btnDeleteFromFacility.Visible = notAllFacilities
		btnDeleteAllFacilities.Visible = notAllFacilities
		lstFacility.Visible = notAllFacilities
	End Sub

	Private Sub ddlDrivers_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ddlDrivers.SelectedIndexChanged
		lblStatus.Text = ""
		Dim driverId As Guid = Guid.Parse(ddlDrivers.SelectedValue)
		_customFieldData.Clear()
		PopulateDriverData(driverId)
		btnDelete.Enabled = driverId <> Guid.Empty AndAlso _currentUserPermission(_currentTableName).Delete
		PopulateAccountsList(driverId)
		PopulateFacilitiesList(driverId)
		PopulateDriverInterfaceList(driverId)
		SetControlUsabilityFromPermissions()
		Utilities.CreateDynamicCustomFieldPanelControls(_customFields, _customFieldData, lstCustomFields, Page)
	End Sub

	Private Sub lstAccount_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles lstAccount.SelectedIndexChanged
		btnDeleteFromAccount.Enabled = lstAccount.SelectedIndex <> -1
	End Sub

	Private Sub lstFacility_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles lstFacility.SelectedIndexChanged
		btnDeleteFromFacility.Enabled = lstFacility.SelectedIndex <> -1
	End Sub
#End Region

	Private Sub PopulateDriversList(driverId As Guid)
		ddlDrivers.Items.Clear()
		If _currentUserPermission(_currentTableName).Create Then
			ddlDrivers.Items.Add(New ListItem("Enter a new driver", Guid.Empty.ToString()))
		Else
			ddlDrivers.Items.Add(New ListItem("Select a driver", Guid.Empty.ToString()))
		End If
		For Each driver As KaDriver In KaDriver.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
			ddlDrivers.Items.Add(New ListItem(driver.Name, driver.Id.ToString()))
			If driver.Id = driverId Then ddlDrivers.SelectedIndex = ddlDrivers.Items.Count - 1
		Next
		If ddlDrivers.SelectedIndex = -1 Then ddlDrivers.SelectedIndex = 0
		ddlDrivers_SelectedIndexChanged(ddlDrivers, Nothing)
	End Sub

	Private Sub PopulateAccountsList(ByVal driverId As Guid)
		lstAccount.Items.Clear()
		Dim getAccountsDriverRdr As OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), "SELECT customer_accounts.id, customer_accounts.name " &
													 "FROM customer_account_drivers " &
													 "INNER JOIN customer_accounts ON customer_accounts.id = customer_account_drivers.customer_account_id " &
													 "WHERE (customer_accounts.deleted = 0) " &
														"AND (customer_account_drivers.deleted = 0) " &
														"AND (customer_account_drivers.driver_id = " & Q(driverId) & ") " &
													 "ORDER BY customer_accounts.name")

		Do While getAccountsDriverRdr.Read
			lstAccount.Items.Add(New ListItem(getAccountsDriverRdr.Item("Name"), getAccountsDriverRdr.Item("Id").ToString))
		Loop
		getAccountsDriverRdr.Close()

		If lstAccount.Items.Count > 0 Then
			btnDeleteAllAccounts.Enabled = True
		Else
			btnDeleteAllAccounts.Enabled = False
		End If
	End Sub

	Private Sub PopulateAccountsCombo()
		ddlAccounts.Items.Clear()
		ddlAccounts.Items.Add(New ListItem("Select an account", Guid.Empty.ToString()))
		For Each account As KaCustomerAccount In KaCustomerAccount.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND is_supplier=0" & IIf(_currentUser.OwnerId <> Guid.Empty, String.Format(" AND (owner_id={0} OR owner_id={1})", Q(Guid.Empty), Q(_currentUser.OwnerId)), ""), "name ASC")
			ddlAccounts.Items.Add(New ListItem(account.Name, account.Id.ToString()))
		Next
	End Sub

	Private Sub PopulateFacilitiesList(ByVal driverId As Guid)
		lstFacility.Items.Clear()
		Dim getFacilityDriverRdr As OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), "SELECT locations.id, locations.name " &
													 "FROM location_driver_access " &
													 "INNER JOIN locations ON locations.id = location_driver_access.location_id " &
													 "WHERE (locations.deleted = 0) " &
														"AND (location_driver_access.deleted = 0) " &
														"AND (location_driver_access.driver_id = " & Q(driverId) & ") " &
													 "ORDER BY locations.name")
		Do While getFacilityDriverRdr.Read
			lstFacility.Items.Add(New ListItem(getFacilityDriverRdr.Item("Name"), getFacilityDriverRdr.Item("Id").ToString))
		Loop
		getFacilityDriverRdr.Close()
		If lstFacility.Items.Count > 0 Then
			btnDeleteAllFacilities.Enabled = True
		Else
			btnDeleteAllFacilities.Enabled = False
		End If
	End Sub

	Private Sub PopulateFacilitiesCombo()
		ddlFacilities.Items.Clear()
		ddlFacilities.Items.Add(New ListItem("Select a facility", Guid.Empty.ToString()))
		For Each facility As KaLocation In KaLocation.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
			ddlFacilities.Items.Add(New ListItem(facility.Name, facility.Id.ToString()))
		Next
	End Sub

	Private Sub PopulateDriverData(ByVal driverId As Guid)
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim driver As KaDriver
		Try
			driver = New KaDriver(connection, driverId)
			For Each customFieldValue As KaCustomFieldData In KaCustomFieldData.GetAll(connection, String.Format("deleted = 0 AND {0} = {1}", KaCustomFieldData.FN_RECORD_ID, Q(driver.Id)), KaCustomFieldData.FN_LAST_UPDATED)
				_customFieldData.Add(customFieldValue)
			Next
		Catch ex As RecordNotFoundException ' user is entering a new driver
			driver = New KaDriver()

			Boolean.TryParse(KaSetting.GetSetting(GetUserConnection(_currentUser.Id), KaSetting.SN_DEFAULT_NEW_DRIVERS_AS_VALID_FOR_ALL_ACCOUNTS, KaSetting.SD_DEFAULT_NEW_DRIVERS_AS_VALID_FOR_ALL_ACCOUNTS), driver.ValidForAllAccounts)

			_customFieldData.Clear()
		End Try
		With driver
			tbxNumber.Text = .Number
			tbxPassword.Text = .Password
			tbxName.Text = .Name
			tbxStreet.Text = .Street
			tbxCity.Text = .City
			tbxState.Text = .State
			tbxZip.Text = .ZipCode
			tbxCountry.Text = .Country
			tbxPhone.Text = .Phone
			tbxEmail.Text = .Email
			tbxNotes.Text = .Notes
			cbxDisabled.Checked = .Disabled
			chkValidForAllAccounts.Checked = .ValidForAllAccounts
			chkValidForAllAccounts_CheckedChanged(chkValidForAllAccounts, New EventArgs)
			chkValidForAllFacilities.Checked = .ValidForAllLocations
			chkValidForAllFacilities_CheckedChanged(chkValidForAllFacilities, New EventArgs)

			If Not Boolean.TryParse(driver.GetSetting(connection, KaDriverSettings.SN_DRIVER_CAN_LOAD_PARTIAL_ORDERS, False, False, Nothing).Value, cbxSelfServePartialOrdersAllowed.Checked) Then cbxSelfServePartialOrdersAllowed.Checked = False
			If Not Boolean.TryParse(driver.GetSetting(connection, KaDriverSettings.SN_DRIVER_CAN_DO_HAND_ADDS, False, False, Nothing).Value, cbxSelfServeHandAddsAllowed.Checked) Then cbxSelfServeHandAddsAllowed.Checked = False

			SetInFacilityStatus(connection, driverId)
		End With
	End Sub

	Private Sub SetInFacilityStatus(connection As OleDbConnection, driverId As Guid)
		Dim lastInFacilityRecord As KaDriverInFacility = Nothing
		Try
			For Each driverInFacilityRecord As KaDriverInFacility In KaDriverInFacility.GetAll(connection, String.Format("driver_id = {0} AND deleted = 0", Q(driverId)), "created desc, last_updated desc")
				lastInFacilityRecord = driverInFacilityRecord
				Exit For
			Next
		Catch ex As Exception

		End Try
		If lastInFacilityRecord Is Nothing Then
			pnlLastInFacilityInfo.Visible = False
		Else
			pnlLastInFacilityInfo.Visible = True
			If lastInFacilityRecord.EnteredAt > New DateTime(1900, 1, 1) Then
				lblLastEnteredFacility.Text = String.Format("{0:g}", lastInFacilityRecord.EnteredAt)
				liLastEnteredFacility.Visible = True
			Else
				liLastEnteredFacility.Visible = False
			End If
			If lastInFacilityRecord.ExitedAt > New DateTime(1900, 1, 1) Then
				lblLastExitedFacility.Text = String.Format("{0:g}", lastInFacilityRecord.ExitedAt)
				liLastExitedFacility.Visible = True
			Else
				liLastExitedFacility.Visible = False
			End If
			Try
				lblLastInFacilityLocation.Text = New KaLocation(connection, lastInFacilityRecord.LocationId).Name
			Catch ex As RecordNotFoundException
				lblLastInFacilityLocation.Text = "Not specified"
			End Try
			If lastInFacilityRecord.InFacility Then
				lblLastInFacilityStatus.Text = "In facility"
			Else
				lblLastInFacilityStatus.Text = "Not in facility"
			End If
			btnClearLastInFacilityStatus.Visible = lastInFacilityRecord.InFacility
		End If
	End Sub

	Private Function DriverNumberAlreadyExists(ByVal driverNumber As String) As Boolean
		Dim driverId As Guid = Guid.Parse(ddlDrivers.SelectedValue)
		If driverId <> Guid.Empty Then ' existing driver
			Return KaDriver.GetAll(GetUserConnection(_currentUser.Id), "number=" & Q(driverNumber) & " AND deleted=0 AND id<>" & Q(driverId), "").Count > 0
		Else ' new driver
			Return KaDriver.GetAll(GetUserConnection(_currentUser.Id), "number=" & Q(driverNumber) & " AND deleted=0", "").Count > 0
		End If
	End Function

	Private Function ValidateFields() As Boolean
		If tbxName.Text.Trim = "" Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidName", Utilities.JsAlert("Please specify a name for the driver.")) : Return False
		Dim conditions As String = String.Format("deleted=0 AND name={0} AND id<>{1}", Q(tbxName.Text), Q(ddlDrivers.SelectedValue))
		If KaDriver.GetAll(GetUserConnection(_currentUser.Id), conditions, "").Count > 0 Then
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidDuplicateName", Utilities.JsAlert("A driver with the name """ & tbxName.Text & """ already exists. Please enter a unique name for the driver."))
			Return False
		End If
		Dim message As String = ""
		If Not Utilities.IsEmailFieldValid(tbxEmail.Text, message) Then
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidEmail", Utilities.JsAlert(message))
			Return False
		End If
		If Not String.IsNullOrEmpty(tbxNumber.Text) AndAlso DriverNumberAlreadyExists(Utilities.StripSqlInjection(tbxNumber.Text)) Then
			pnlMain.Visible = False
			pnlWarning.Visible = True
			lblWarning.Text = "Warning !! This driver number is already listed in the database. Do you want to continue?"
			Return False
		End If
		Return True
	End Function

	Private Sub SaveDriver()
		With New KaDriver()
			.Id = Guid.Parse(ddlDrivers.SelectedValue)
			.GetChildren(GetUserConnection(_currentUser.Id), Nothing)
			.Name = Utilities.StripSqlInjection(tbxName.Text.Trim)
			.Number = Utilities.StripSqlInjection(tbxNumber.Text.Trim)
			.Password = Utilities.StripSqlInjection(tbxPassword.Text.Trim)
			.Street = Utilities.StripSqlInjection(tbxStreet.Text.Trim)
			.City = Utilities.StripSqlInjection(tbxCity.Text.Trim)
			.State = Utilities.StripSqlInjection(tbxState.Text.Trim)
			.Phone = Utilities.StripSqlInjection(tbxPhone.Text.Trim)
			.ZipCode = Utilities.StripSqlInjection(tbxZip.Text.Trim)
			.Country = Utilities.StripSqlInjection(tbxCountry.Text.Trim)
			.Email = Utilities.StripSqlInjection(tbxEmail.Text.Trim)
			.Notes = Utilities.StripSqlInjection(tbxNotes.Text.Trim)
			.Disabled = cbxDisabled.Checked
			.ValidForAllAccounts = chkValidForAllAccounts.Checked
			.ValidForAllLocations = chkValidForAllFacilities.Checked

			Dim status As String = ""
			Dim connection As OleDbConnection = New OleDbConnection(Tm2Database.GetDbConnection())
			Dim transaction As OleDbTransaction = Nothing
			Try
				connection.Open()
				transaction = connection.BeginTransaction
				If .HasSettingChanged(connection, KaDriverSettings.SN_DRIVER_CAN_LOAD_PARTIAL_ORDERS, cbxSelfServePartialOrdersAllowed.Checked.ToString(), transaction) Then
					.WriteSetting(connection, KaDriverSettings.SN_DRIVER_CAN_LOAD_PARTIAL_ORDERS, cbxSelfServePartialOrdersAllowed.Checked, transaction)
				End If
				If .HasSettingChanged(connection, KaDriverSettings.SN_DRIVER_CAN_DO_HAND_ADDS, cbxSelfServeHandAddsAllowed.Checked.ToString(), transaction) Then
					.WriteSetting(connection, KaDriverSettings.SN_DRIVER_CAN_DO_HAND_ADDS, cbxSelfServeHandAddsAllowed.Checked, transaction)
				End If
				If .Id.Equals(Guid.Empty) Then
					status = "New driver added successfully"
				Else
					status = "Selected driver updated successfully"
				End If
				.SqlUpdateInsertIfNotFound(connection, transaction, Database.ApplicationIdentifier, _currentUser.Name)
				SaveAccounts(connection, transaction, .Id)
				SaveLocations(connection, transaction, .Id)
				Utilities.ConvertCustomFieldPanelToLists(_customFields, _customFieldData, lstCustomFields)
				For Each customData As KaCustomFieldData In _customFieldData
					customData.RecordId = .Id
					customData.SqlUpdateInsertIfNotFound(connection, transaction, Database.ApplicationIdentifier, _currentUser.Name)
				Next

				transaction.Commit()
				transaction.Dispose()
				transaction = Nothing
			Catch ex As Exception
				If transaction IsNot Nothing AndAlso connection.State = ConnectionState.Open Then transaction.Rollback()
				Throw
			Finally
				If transaction IsNot Nothing AndAlso connection.State = ConnectionState.Open Then transaction.Dispose()
				connection.Close()
			End Try

			PopulateDriversList(.Id)
			ddlDrivers.SelectedValue = .Id.ToString
			ddlDrivers_SelectedIndexChanged(ddlDrivers, New EventArgs())
			lblStatus.Text = status
		End With
		btnDelete.Enabled = True
	End Sub

	Private Sub SaveAccounts(connection As OleDbConnection, transaction As OleDbTransaction, ByVal driverId As Guid)
		If driverId <> Guid.Empty Then
			Dim customerAccountDrivers As ArrayList = KaCustomerAccountDriver.GetAll(connection, "driver_id = " & Q(driverId), "last_updated desc", transaction)
			Dim updatedCustomerAccountDriversGuids As List(Of Guid) = New List(Of Guid)
			Dim existingCustomerAccountDriversGuids As List(Of Guid) = New List(Of Guid)

			'Compile list of Customer Account Drivers records to be inserted/updated from lstAccount
			For Each li As ListItem In lstAccount.Items
				updatedCustomerAccountDriversGuids.Add(Guid.Parse(li.Value))
			Next

			'Iterate through existing customer account drivers records from database, to update previously created records (either set deleted to true or false)
			For Each customerAccountDriver As KaCustomerAccountDriver In customerAccountDrivers
				If (Not existingCustomerAccountDriversGuids.Contains(customerAccountDriver.CustomerAccountId)) Then
					If (updatedCustomerAccountDriversGuids.Contains(customerAccountDriver.CustomerAccountId)) Then
						customerAccountDriver.Deleted = False
					Else
						customerAccountDriver.Deleted = True
					End If
					customerAccountDriver.LastUpdated = Now
					customerAccountDriver.SqlUpdate(connection, transaction, Database.ApplicationIdentifier, _currentUser.Name)
					existingCustomerAccountDriversGuids.Add(customerAccountDriver.CustomerAccountId)
				End If
			Next

			'Iterate through each Guid in the lstAccount list box to determine if a new record should be inserted
			For Each customerAccountId As Guid In updatedCustomerAccountDriversGuids
				If (Not existingCustomerAccountDriversGuids.Contains(customerAccountId)) Then
					With New KaCustomerAccountDriver()
						.Id = Guid.Empty
						.LastUpdated = Now
						.CustomerAccountId = customerAccountId
						.DriverId = driverId
						.Deleted = False
						.SqlInsert(connection, transaction, Database.ApplicationIdentifier, _currentUser.Name)
					End With
				End If
			Next
		End If
	End Sub

	Private Sub SaveLocations(connection As OleDbConnection, transaction As OleDbTransaction, ByVal driverId As Guid)
		If driverId <> Guid.Empty Then
			Dim locationDrivers As ArrayList = KaLocationDriverAccess.GetAll(connection, "driver_id = " & Q(driverId), "last_updated desc", transaction)
			Dim updatedLocationDriversGuids As List(Of Guid) = New List(Of Guid)
			Dim existingLocationDriversGuids As List(Of Guid) = New List(Of Guid)

			'Compile list of Location Drivers records to be inserted/updated from lstLocations
			For Each li As ListItem In lstFacility.Items
				updatedLocationDriversGuids.Add(Guid.Parse(li.Value))
			Next

			'Iterate through existing location drivers records from database, to update previously created records (either set deleted to true or false)
			For Each locationDriver As KaLocationDriverAccess In locationDrivers
				If (Not existingLocationDriversGuids.Contains(locationDriver.LocationId)) Then
					If (updatedLocationDriversGuids.Contains(locationDriver.LocationId)) Then
						locationDriver.Deleted = False
					Else
						locationDriver.Deleted = True
					End If
					locationDriver.LastUpdated = Now
					locationDriver.SqlUpdate(connection, transaction, Database.ApplicationIdentifier, _currentUser.Name)
					existingLocationDriversGuids.Add(locationDriver.LocationId)
				End If
			Next

			'Iterate through each Guid in the lstFacility list box to determine if a new record should be inserted
			For Each locationId As Guid In updatedLocationDriversGuids
				If (Not existingLocationDriversGuids.Contains(locationId)) Then
					With New KaLocationDriverAccess()
						.Id = Guid.Empty
						.LastUpdated = Now
						.LocationId = locationId
						.DriverId = driverId
						.Deleted = False
						.SqlInsert(connection, transaction, Database.ApplicationIdentifier, _currentUser.Name)
					End With
				End If
			Next
		End If
	End Sub

	Private Function AccountListContains(ByVal accountName As String) As Boolean
		For Each li As ListItem In lstAccount.Items
			If (li.Text = accountName) Then
				Return True
			End If
		Next
		Return False
	End Function

	Private Function FacilityListContains(ByVal facilityName As String) As Boolean
		For Each li As ListItem In lstFacility.Items
			If (li.Text = facilityName) Then
				Return True
			End If
		Next
		Return False
	End Function

	Private Sub SetTextboxMaxLengths()
		tbxCity.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaDriver.TABLE_NAME, "city"))
		tbxEmail.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaDriver.TABLE_NAME, "email"))
		tbxName.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaDriver.TABLE_NAME, "name"))
		tbxNotes.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaDriver.TABLE_NAME, "notes"))
		tbxNumber.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaDriver.TABLE_NAME, "number"))
		'tbxPassword.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaDriver.TABLE_NAME, ""))
		tbxPhone.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaDriver.TABLE_NAME, "phone"))
		tbxState.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaDriver.TABLE_NAME, "state"))
		tbxStreet.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaDriver.TABLE_NAME, "street"))
		tbxZip.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaDriver.TABLE_NAME, "zip_code"))
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
					"AND ((" & KaInterfaceTypes.FN_SHOW_DRIVER_INTERFACE & " = 1) " &
					"OR (interfaces.id IN (SELECT " & KaDriverInterfaceSettings.TABLE_NAME & ".interface_id " &
											"FROM " & KaDriverInterfaceSettings.TABLE_NAME & " " &
											"WHERE (deleted=0) " &
												"AND (" & KaDriverInterfaceSettings.TABLE_NAME & "." & KaDriverInterfaceSettings.FN_DRIVER_ID & " = " & Q(ddlDrivers.SelectedValue) & ") " &
											"AND (" & KaDriverInterfaceSettings.TABLE_NAME & "." & KaDriverInterfaceSettings.FN_DRIVER_ID & " <> " & Q(Guid.Empty) & ")))) " &
				"ORDER BY interfaces.name")
		Do While getInterfaceRdr.Read
			ddlInterface.Items.Add(New ListItem(getInterfaceRdr.Item("name"), getInterfaceRdr.Item("id").ToString()))
		Loop
		getInterfaceRdr.Close()
		pnlInterfaceSetup.Visible = (ddlInterface.Items.Count > 1) AndAlso ddlDrivers.SelectedValue <> Guid.Empty.ToString
	End Sub

	Private Sub SaveInterface()
		lblStatus.Text = ""
		If Guid.Parse(ddlDrivers.SelectedValue) = Guid.Empty Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidDriverNotSaved", Utilities.JsAlert("You must save the driver before you can set up interface cross references.")) : Exit Sub
		If Guid.Parse(ddlInterface.SelectedValue) = Guid.Empty Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidInterface", Utilities.JsAlert("An interface must be selected. Interface settings not saved.")) : Exit Sub
		If tbxInterfaceCrossReference.Text.Length = 0 Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidCrossReference", Utilities.JsAlert("A cross reference must be specified. Interface settings not saved.")) : Exit Sub

		' If this is not export only, check if there are any other settings with the same cross reference ID
		If Not chkExportOnly.Checked Then
			Dim allInterfaceSettings As ArrayList = KaDriverInterfaceSettings.GetAll(GetUserConnection(_currentUser.Id), KaDriverInterfaceSettings.FN_CROSS_REFERENCE & " = " & Q(tbxInterfaceCrossReference.Text.Trim) &
																							" AND " & KaDriverInterfaceSettings.FN_INTERFACE_ID & " = " & Q(ddlInterface.SelectedValue) &
																							" AND " & KaDriverInterfaceSettings.FN_DELETED & " = 0 " &
																							" AND " & KaDriverInterfaceSettings.FN_EXPORT_ONLY & " = 0 " &
																							" AND " & KaDriverInterfaceSettings.FN_ID & " <> " & Q(ddlDriverInterface.SelectedValue), "")
			If allInterfaceSettings.Count > 0 Then
				ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidCrossReferenceDuplicated", Utilities.JsAlert("A cross reference of " & tbxInterfaceCrossReference.Text.Trim & " already exists for this interface."))
				Exit Sub
			End If
		End If

		Dim driverInterfaceId As Guid = Guid.Parse(ddlDriverInterface.SelectedValue)
		If driverInterfaceId = Guid.Empty Then
			Dim driverInterface As KaDriverInterfaceSettings = New KaDriverInterfaceSettings
			driverInterface.DriverId = Guid.Parse(ddlDrivers.SelectedValue)
			driverInterface.InterfaceId = Guid.Parse(ddlInterface.SelectedValue)
			driverInterface.CrossReference = tbxInterfaceCrossReference.Text.Trim
			driverInterface.DefaultSetting = chkDefaultSetting.Checked
			driverInterface.ExportOnly = chkExportOnly.Checked
			driverInterface.SqlInsert(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
			driverInterfaceId = driverInterface.Id
		Else
			Dim driverInterface As KaDriverInterfaceSettings = New KaDriverInterfaceSettings(GetUserConnection(_currentUser.Id), driverInterfaceId)
			driverInterface.DriverId = Guid.Parse(ddlDrivers.SelectedValue)
			driverInterface.InterfaceId = Guid.Parse(ddlInterface.SelectedValue)
			driverInterface.CrossReference = tbxInterfaceCrossReference.Text.Trim
			driverInterface.DefaultSetting = chkDefaultSetting.Checked
			driverInterface.ExportOnly = chkExportOnly.Checked
			driverInterface.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
		End If

		PopulateDriverInterfaceList(Guid.Parse(ddlDrivers.SelectedValue))
		ddlDriverInterface.SelectedValue = driverInterfaceId.ToString
		ddlDriverInterface_SelectedIndexChanged(ddlDriverInterface, New EventArgs)
		btnRemoveInterface.Enabled = True
	End Sub

	Private Sub RemoveInterface()
		Dim selectedId As Guid = Guid.Parse(ddlDriverInterface.SelectedValue)
		If selectedId <> Guid.Empty Then
			Dim prodInterfaceSetting As KaDriverInterfaceSettings = New KaDriverInterfaceSettings(GetUserConnection(_currentUser.Id), selectedId)
			prodInterfaceSetting.Deleted = True
			prodInterfaceSetting.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
		End If
		PopulateDriverInterfaceList(Guid.Parse(ddlDrivers.SelectedValue))
		btnRemoveInterface.Enabled = PopulateInterfaceInformation(Guid.Empty)
	End Sub

	Private Sub DeleteInterfaceInformation(ByVal driverId As Guid)
		For Each r As KaDriverInterfaceSettings In KaDriverInterfaceSettings.GetAll(GetUserConnection(_currentUser.Id), KaDriverInterfaceSettings.FN_DELETED & " = 0 and " & KaDriverInterfaceSettings.FN_DRIVER_ID & " = " & Q(driverId), "")
			r.Deleted = True
			r.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
		Next
	End Sub

	Private Sub PopulateDriverInterfaceList(ByVal driverId As Guid)
		PopulateInterfaceList()
		ddlDriverInterface.Items.Clear()
		ddlDriverInterface.Items.Add(New ListItem(IIf(ddlInterface.Items.Count > 1, "Add an interface", ""), Guid.Empty.ToString))
		Dim getInterfaceRdr As OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), "SELECT " & KaDriverInterfaceSettings.TABLE_NAME & ".id, interfaces.name, " & KaDriverInterfaceSettings.TABLE_NAME & ".cross_reference " &
				"FROM " & KaDriverInterfaceSettings.TABLE_NAME & " " &
				"INNER JOIN interfaces ON " & KaDriverInterfaceSettings.TABLE_NAME & ".interface_id = interfaces.id " &
				"INNER JOIN interface_types ON interfaces.interface_type_id = interface_types.id " &
				"WHERE (" & KaDriverInterfaceSettings.TABLE_NAME & ".deleted = 0) " &
					"AND (interfaces.deleted = 0) " &
					"AND (interface_types.deleted = 0) " &
					"AND (" & KaDriverInterfaceSettings.TABLE_NAME & "." & KaDriverInterfaceSettings.FN_DRIVER_ID & "=" & Q(driverId) & ") " &
				"ORDER BY interfaces.name, " & KaDriverInterfaceSettings.TABLE_NAME & ".cross_reference")
		Do While getInterfaceRdr.Read
			ddlDriverInterface.Items.Add(New ListItem(getInterfaceRdr.Item("name") & " (" & getInterfaceRdr.Item("cross_reference") & ")", getInterfaceRdr.Item("id").ToString()))
		Loop
		getInterfaceRdr.Close()
		ddlDriverInterface.SelectedIndex = 0
		ddlDriverInterface_SelectedIndexChanged(ddlDriverInterface, New EventArgs())
	End Sub

	Private Function PopulateInterfaceInformation(ByVal driverInterfaceId As Guid) As Boolean
		Dim retval As Boolean = False
		If driverInterfaceId <> Guid.Empty Then
			Dim driverInterfaceSetting As KaDriverInterfaceSettings = New KaDriverInterfaceSettings(GetUserConnection(_currentUser.Id), driverInterfaceId)
			ddlInterface.SelectedValue = driverInterfaceSetting.InterfaceId.ToString
			tbxInterfaceCrossReference.Text = driverInterfaceSetting.CrossReference
			chkDefaultSetting.Checked = driverInterfaceSetting.DefaultSetting
			chkExportOnly.Checked = driverInterfaceSetting.ExportOnly
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

	Protected Sub ddlDriverInterface_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles ddlDriverInterface.SelectedIndexChanged
		btnRemoveInterface.Enabled = PopulateInterfaceInformation(Guid.Parse(ddlDriverInterface.SelectedValue))
	End Sub

	Private Sub ddlInterface_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles ddlInterface.SelectedIndexChanged
		If Guid.Parse(ddlDriverInterface.SelectedValue) = Guid.Empty Then
			'Only do this check if we are a new interface setting
			Dim count As Integer = 0
			Try
				Dim rdr As OleDb.OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), "SELECT count(*) " &
																			 "FROM " & KaDriverInterfaceSettings.TABLE_NAME & " " &
																			 "WHERE " & KaDriverInterfaceSettings.FN_DELETED & " = 0 " &
																			 "AND " & KaDriverInterfaceSettings.FN_INTERFACE_ID & " = " & Q(Guid.Parse(ddlInterface.SelectedValue)) & " " &
																			 "AND " & KaDriverInterfaceSettings.FN_DRIVER_ID & " = " & Q(Guid.Parse(ddlDrivers.SelectedValue)))
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
			Dim shouldEnable = (.Edit AndAlso ddlDrivers.SelectedIndex > 0) OrElse (.Create AndAlso ddlDrivers.SelectedIndex = 0)
			pnlGeneral.Enabled = shouldEnable
			pnlInterfaceSettings.Enabled = shouldEnable
			btnSave.Enabled = shouldEnable
			btnDelete.Enabled = Not Guid.Parse(ddlDrivers.SelectedValue).Equals(Guid.Empty) AndAlso .Edit AndAlso .Delete
		End With
	End Sub

	Private Sub btnClearLastInFacilityStatus_Click(sender As Object, e As EventArgs) Handles btnClearLastInFacilityStatus.Click
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim driverId As Guid = Guid.Parse(ddlDrivers.SelectedValue)
		Tm2Database.ExecuteNonQuery(connection, String.Format("UPDATE drivers_in_facility SET in_facility = 0 WHERE driver_id = {0} and in_facility = 1 and deleted = 0", Q(driverId)))

		SetInFacilityStatus(connection, driverId)
	End Sub
End Class
