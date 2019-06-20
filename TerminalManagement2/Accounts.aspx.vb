Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class Accounts
	Inherits System.Web.UI.Page

	Private _currentUser As KaUser
	Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
	Private _currentTableName As String = KaCustomerAccount.TABLE_NAME
	Private _customFields As New List(Of KaCustomField)
	Private _customFieldData As New List(Of KaCustomFieldData)

	Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
		Response.Cache.SetCacheability(HttpCacheability.NoCache)
		_currentUser = Utilities.GetUser(Me)
		_currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Accounts")
		If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")

		If Not Page.IsPostBack Then
			SetTextboxMaxLengths()
			PopulateAccountsList(_currentUser)
			PopulateOwnersList(_currentUser)
			PopulateFacilitiesCombo()
			PopulateDriversCombo()
			PopulateInterfaceList()
			_customFields.Clear()
			For Each customField As KaCustomField In KaCustomField.GetAll(GetUserConnection(_currentUser.Id), String.Format("deleted=0 AND {0} = {1}", KaCustomField.FN_TABLE_NAME, Q(KaCustomerAccount.TABLE_NAME)), KaCustomField.FN_FIELD_NAME)
				_customFields.Add(customField)
			Next
			If Page.Request("CustomerAccountId") IsNot Nothing Then
				Try
					ddlCustomers.SelectedValue = Page.Request("CustomerAccountId")
				Catch ex As ArgumentOutOfRangeException
					ddlCustomers.SelectedIndex = 0
				End Try
			End If
			ddlCustomers_SelectedIndexChanged(ddlCustomers, New EventArgs)
			cbxValidForAllFacilities_CheckedChanged(Nothing, Nothing)
			Utilities.ConfirmBox(Me.btnDelete, "Are you sure you want to delete this account?")
			Utilities.SetFocus(tbxName, Me)
		End If
		SetControlUsabilityFromPermissions()
	End Sub

	Private Sub PopulateAccountsList(ByVal currentUser As KaUser)
		ddlCustomers.Items.Clear()
		If _currentUserPermission(_currentTableName).Create Then ddlCustomers.Items.Add(New ListItem("Enter a new account", Guid.Empty.ToString())) Else ddlCustomers.Items.Add(New ListItem("Select an account", Guid.Empty.ToString()))
		Dim conditions As String = "deleted=0 AND is_supplier=0" & IIf(currentUser.OwnerId <> Guid.Empty, String.Format(" AND (owner_id={0} OR owner_id={1})", Q(Guid.Empty), Q(currentUser.OwnerId)), "")
		For Each account As KaCustomerAccount In KaCustomerAccount.GetAll(GetUserConnection(currentUser.Id), conditions, "name ASC")
			ddlCustomers.Items.Add(New ListItem(account.Name, account.Id.ToString()))
		Next
	End Sub

	Private Sub PopulateOwnersList(ByVal currentUser As KaUser)
		ddlOwners.Items.Clear()
        ddlOwners.Items.Add(New ListItem("All owners", Guid.Empty.ToString()))
        Dim conditions As String = "deleted=0" & IIf(currentUser.OwnerId <> Guid.Empty, String.Format(" AND id={0}", Q(currentUser.OwnerId)), "")
		For Each owner As KaOwner In KaOwner.GetAll(GetUserConnection(currentUser.Id), conditions, "name ASC")
			ddlOwners.Items.Add(New ListItem(owner.Name, owner.Id.ToString()))
		Next
	End Sub

	Private Sub PopulateFacilitiesList(ByVal accountId As Guid)
		lstFacility.Items.Clear()
		Dim getFacilityAccountRdr As OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), "SELECT locations.id, locations.name " &
													 "FROM location_customer_access " &
													 "INNER JOIN locations ON locations.id = location_customer_access.location_id " &
													 "WHERE (locations.deleted = 0) " &
														"AND (location_customer_access.deleted = 0) " &
														"AND (location_customer_access.customer_account_id = " & Q(accountId) & ") " &
													 "ORDER BY locations.name")
		Do While getFacilityAccountRdr.Read
			lstFacility.Items.Add(New ListItem(getFacilityAccountRdr.Item("Name"), getFacilityAccountRdr.Item("Id").ToString))
		Loop
		getFacilityAccountRdr.Close()
		If lstFacility.Items.Count > 0 Then
			btnDeleteAllFacilities.Enabled = True
		Else
			btnDeleteAllFacilities.Enabled = False
		End If
	End Sub

	Private Sub PopulateDriversList(ByVal accountId As Guid)
		lstDrivers.Items.Clear()
		Dim getAssignedDriversRdr As OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), "SELECT customer_account_drivers.driver_id, drivers.name + CASE WHEN drivers.valid_for_all_accounts = 1 THEN ' (Valid for all accounts)' ELSE '' END AS name " & "FROM customer_account_drivers " & "INNER JOIN drivers ON drivers.id = customer_account_drivers.driver_id " & "WHERE (customer_account_drivers.deleted = 0) AND (customer_account_id = " & Q(accountId) & ") " & "ORDER BY name")
		Do While getAssignedDriversRdr.Read
			lstDrivers.Items.Add(New ListItem(getAssignedDriversRdr.Item("Name"), getAssignedDriversRdr.Item("driver_id").ToString))
		Loop
		If lstDrivers.Items.Count > 0 Then
			btnDeleteAllDrivers.Enabled = True
		Else
			btnDeleteAllDrivers.Enabled = False
		End If
		getAssignedDriversRdr.Close()
	End Sub

	Private Sub PopulateFacilitiesCombo()
		ddlFacilities.Items.Clear()
        ddlFacilities.Items.Add(New ListItem("Select a facility", Guid.Empty.ToString()))
        For Each facility As KaLocation In KaLocation.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
			ddlFacilities.Items.Add(New ListItem(facility.Name, facility.Id.ToString()))
		Next
	End Sub

	Private Sub PopulateDriversCombo()
		ddlDrivers.Items.Clear()
        ddlDrivers.Items.Add(New ListItem("Select a driver", Guid.Empty.ToString()))
        Dim getDriversAvailRdr As OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), "SELECT id, name, valid_for_all_accounts FROM " & KaDriver.TABLE_NAME & " WHERE deleted=0 order by name")

		Do While getDriversAvailRdr.Read()
			ddlDrivers.Items.Add(New ListItem(getDriversAvailRdr.Item("Name") & IIf(getDriversAvailRdr.Item("valid_for_all_accounts"), " (Valid for all accounts)", ""), getDriversAvailRdr.Item("Id").ToString()))
		Loop
		getDriversAvailRdr.Close()
	End Sub

	Private Sub ddlCustomers_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ddlCustomers.SelectedIndexChanged
		lblStatus.Text = ""
		Dim customerId As Guid = Guid.Parse(ddlCustomers.SelectedValue)
		If customerId <> Guid.Empty Then
			PopulateAccountData(customerId)
			PopulateFacilitiesList(customerId)
			PopulateDriversList(customerId)
			With _currentUserPermission(_currentTableName)
				lstDrivers.Enabled = .Edit
				btnAddAllDrivers.Enabled = .Edit
				btnDeleteAllDrivers.Enabled = .Edit
				ddlDrivers.Enabled = .Edit
				pnlDrivers.Enabled = .Edit
			End With
			btnDelete.Enabled = _currentUserPermission(_currentTableName).Delete
			pnlInterfaceSetup.Visible = KaInterface.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC").Count > 0
			_customFieldData.Clear()
			For Each customFieldValue As KaCustomFieldData In KaCustomFieldData.GetAll(GetUserConnection(_currentUser.Id), String.Format("deleted = 0 AND {0} = {1}", KaCustomFieldData.FN_RECORD_ID, Q(customerId)), KaCustomFieldData.FN_LAST_UPDATED)
				_customFieldData.Add(customFieldValue)
			Next
		Else ' new user
			ClearUserData()
			btnDelete.Enabled = False
			pnlInterfaceSetup.Visible = False
			pnlDrivers.Enabled = False
			lstDrivers.Items.Clear()
			Utilities.SetFocus(tbxAcctNum, Me)
		End If
		Utilities.CreateDynamicCustomFieldPanelControls(_customFields, _customFieldData, lstCustomFields, Page)
		SetDisabledCondition()
		SetControlUsabilityFromPermissions()
	End Sub

	Private Sub PopulateAccountData(ByVal accountId As Guid)
		With New KaCustomerAccount(GetUserConnection(_currentUser.Id), accountId)
			Try
				ddlOwners.SelectedValue = .OwnerId.ToString()
			Catch ex As ArgumentOutOfRangeException
				ddlOwners.SelectedIndex = 0
				ClientScript.RegisterClientScriptBlock(Me.GetType(), "NoOwnerFound", Utilities.JsAlert("Record not found in owners where ID = " & .OwnerId.ToString() & ". Owner set to blank instead."))
			End Try
			tbxAcctNum.Text = .AccountNumber
			tbxName.Text = .Name
			tbxAddress.Text = .Street
			tbxCity.Text = .City
			tbxCounty.Text = .County
			tbxState.Text = .State
			tbxZip.Text = .ZipCode
			tbxCountry.Text = .Country
			tbxPhone.Text = .Phone
			tbxEmail.Text = .Email
			tbxBillingNum.Text = .BillingAccountNumber
			tbxCoop.Text = .Coop
			tbxNotes.Text = .Notes
			cbxDisabled.Checked = .Disabled
			PopulateAccountInterfaceList(.Id)
			pnlInterfaceSetup.Visible = True
			cbxValidForAllFacilities.Checked = .ValidForAllLocations
			cbxValidForAllFacilities_CheckedChanged(cbxValidForAllFacilities, New EventArgs)
		End With
	End Sub

	Private Sub ClearUserData()
		ddlOwners.SelectedValue = Guid.Empty.ToString
		tbxAcctNum.Text = ""
		tbxName.Text = ""
		tbxAddress.Text = ""
		tbxCity.Text = ""
		tbxCounty.Text = ""
		tbxState.Text = ""
		tbxZip.Text = ""
		tbxCountry.Text = ""
		tbxPhone.Text = ""
		tbxEmail.Text = ""
		tbxBillingNum.Text = ""
		tbxCoop.Text = ""
		tbxNotes.Text = ""
		cbxDisabled.Checked = False

		_customFieldData.Clear()
	End Sub

	Private Sub SetDisabledCondition()
		If cbxDisabled.Checked Then
			ddlOwners.Enabled = False : tbxAcctNum.Enabled = False : tbxName.Enabled = False
			tbxAddress.Enabled = False : tbxCity.Enabled = False : tbxState.Enabled = False
			tbxZip.Enabled = False : tbxCounty.Enabled = False : tbxPhone.Enabled = False
			tbxEmail.Enabled = False : tbxCoop.Enabled = False : tbxCountry.Enabled = False
			tbxBillingNum.Enabled = False : tbxNotes.Enabled = False
		Else
			ddlOwners.Enabled = True : tbxAcctNum.Enabled = True : tbxName.Enabled = True
			tbxAddress.Enabled = True : tbxCity.Enabled = True : tbxState.Enabled = True
			tbxZip.Enabled = True : tbxCounty.Enabled = True : tbxPhone.Enabled = True
			tbxEmail.Enabled = True : tbxCoop.Enabled = True : tbxCountry.Enabled = True
			tbxBillingNum.Enabled = True : tbxNotes.Enabled = True
		End If
	End Sub

	Private Sub SetDropListToCurrent(ByVal currentUserId As Guid)
		ddlCustomers.SelectedValue = currentUserId.ToString()
	End Sub

	Private Sub btnSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSave.Click
		If ValidateFields() Then
			SaveAccount()
		End If
	End Sub

	Private Sub lstFacility_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles lstFacility.SelectedIndexChanged
		btnDeleteFromFacility.Enabled = lstFacility.SelectedIndex <> -1
	End Sub

	Protected Sub cbxValidForAllFacilities_CheckedChanged(sender As Object, e As EventArgs) Handles cbxValidForAllFacilities.CheckedChanged
		Dim notAllFacilities As Boolean = Not cbxValidForAllFacilities.Checked
		rowAddtoFacility.Visible = notAllFacilities
		btnAddAllFacilities.Visible = notAllFacilities
		btnDeleteFromFacility.Visible = notAllFacilities
		btnDeleteAllFacilities.Visible = notAllFacilities
		lstFacility.Visible = notAllFacilities
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

	Protected Sub btnDeleteAllFacilities_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnDeleteAllFacilities.Click
		lstFacility.Items.Clear()
		btnDeleteFromFacility.Enabled = False
		btnDeleteAllFacilities.Enabled = False
	End Sub

	Private Function FacilityListContains(ByVal facilityName As String) As Boolean
		For Each li As ListItem In lstFacility.Items
			If (li.Text = facilityName) Then
				Return True
			End If
		Next
		Return False
	End Function

	Private Function ValidateFields()
		If tbxName.Text.Trim().Length = 0 Then
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidName", Utilities.JsAlert("Please specify a name."))
			Return False
		End If

		If KaCustomerAccount.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND is_supplier=0 AND id<>" & Q(Guid.Parse(ddlCustomers.SelectedValue)) & " AND name=" & Q(tbxName.Text), "").Count > 0 Then
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidDuplicateName", Utilities.JsAlert("The name """ & tbxName.Text & """ is already used by another customer account. Please specify a unique name for this customer account."))
			Return False
		End If

		If tbxAcctNum.Text.Trim.Length > 0 AndAlso KaCustomerAccount.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND is_supplier=0 AND id<>" & Q(Guid.Parse(ddlCustomers.SelectedValue)) & " AND account_number=" & Q(tbxAcctNum.Text), "").Count > 0 Then
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidDuplicateAccountNumber", Utilities.JsAlert("The account number """ & tbxAcctNum.Text & """ is already used by another customer account. Please specify a unique number for this customer account."))
			Return False
		End If

		Dim message As String = ""
		If Not Utilities.IsEmailFieldValid(tbxEmail.Text, message) Then
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidEmail", Utilities.JsAlert(message))
			Return False
		End If

		Return True
	End Function

	Private Sub SaveAccount()
		With New KaCustomerAccount()
			.Id = Guid.Parse(ddlCustomers.SelectedValue)
			.OwnerId = Guid.Parse(ddlOwners.SelectedValue)
			.AccountNumber = Utilities.StripSqlInjection(tbxAcctNum.Text.Trim)
			.Name = Utilities.StripSqlInjection(tbxName.Text.Trim)
			.Street = Utilities.StripSqlInjection(tbxAddress.Text.Trim)
			.City = Utilities.StripSqlInjection(tbxCity.Text.Trim)
			.County = Utilities.StripSqlInjection(tbxCounty.Text.Trim)
			.State = Utilities.StripSqlInjection(tbxState.Text.Trim)
			.ZipCode = Utilities.StripSqlInjection(tbxZip.Text.Trim)
			.Country = Utilities.StripSqlInjection(tbxCountry.Text.Trim)
			.Phone = Utilities.StripSqlInjection(tbxPhone.Text.Trim)
			.Email = Utilities.StripSqlInjection(tbxEmail.Text.Trim)
			.BillingAccountNumber = Utilities.StripSqlInjection(tbxBillingNum.Text.Trim)
			.Coop = Utilities.StripSqlInjection(tbxCoop.Text.Trim)
			.Notes = Utilities.StripSqlInjection(tbxNotes.Text.Trim)
			.Disabled = cbxDisabled.Checked
			.ValidForAllLocations = cbxValidForAllFacilities.Checked
			Dim status As String = ""
			If .Id = Guid.Empty Then
				.SqlInsert(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
				status = "New account added successfully"
			Else
				.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
				status = "Selected account updated successfully"
			End If

			Utilities.ConvertCustomFieldPanelToLists(_customFields, _customFieldData, lstCustomFields)
			For Each customData As KaCustomFieldData In _customFieldData
				customData.RecordId = .Id
				customData.SqlUpdateInsertIfNotFound(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
			Next
			PopulateAccountsList(_currentUser)
			SetDropListToCurrent(.Id)
			PopulateAccountData(.Id)
			SaveLocations(.Id)
			SaveDrivers(.Id)
			lblStatus.Text = status
		End With
		btnDelete.Enabled = _currentUserPermission(_currentTableName).Delete
		SetDisabledCondition()
	End Sub

	Private Sub SaveLocations(ByVal accountId As Guid)
		If accountId <> Guid.Empty Then
			Dim c As OleDbConnection = GetUserConnection(_currentUser.Id)
			Dim locationAccounts As ArrayList = KaLocationCustomerAccountAccess.GetAll(GetUserConnection(_currentUser.Id), "customer_account_id = " & Q(accountId), "last_updated desc")
			Dim updatedLocationAccountsGuids As List(Of Guid) = New List(Of Guid)
			Dim existingLocationAccountsGuids As List(Of Guid) = New List(Of Guid)

			'Compile list of Location Accounts records to be inserted/updated from lstLocations
			For Each li As ListItem In lstFacility.Items
				updatedLocationAccountsGuids.Add(Guid.Parse(li.Value))
			Next

			'Iterate through existing location Accounts records from database, to update previously created records (either set deleted to true or false)
			For Each locationAccount As KaLocationCustomerAccountAccess In locationAccounts
				If (Not existingLocationAccountsGuids.Contains(locationAccount.LocationId)) Then
					If (updatedLocationAccountsGuids.Contains(locationAccount.LocationId)) Then
						locationAccount.Deleted = False
					Else
						locationAccount.Deleted = True
					End If
					locationAccount.LastUpdated = Now
					locationAccount.SqlUpdate(c, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
					existingLocationAccountsGuids.Add(locationAccount.LocationId)
				End If
			Next

			'Iterate through each Guid in the lstFacility list box to determine if a new record should be inserted
			For Each locationId As Guid In updatedLocationAccountsGuids
				If (Not existingLocationAccountsGuids.Contains(locationId)) Then
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

	Private Sub btnDelete_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDelete.Click
		lblStatus.Text = ""
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim reader As OleDbDataReader = Tm2Database.ExecuteReader(connection, String.Format("SELECT DISTINCT orders.id AS order_id FROM orders, order_customer_accounts, customer_accounts WHERE order_customer_accounts.customer_account_id={0} AND order_customer_accounts.deleted=0 AND customer_accounts.deleted=0 AND order_customer_accounts.order_id = orders.id AND orders.deleted=0 AND orders.completed=0", Q(ddlCustomers.SelectedValue)))
		Dim conditions As String = "id=" & Q(Guid.Empty)
		Do While reader.Read()
			conditions &= " OR id=" & Q(reader("order_id"))
		Loop
		reader.Close()
		Dim orders As ArrayList = KaOrder.GetAll(connection, conditions, "number ASC")
		If orders.Count > 0 Then
			Dim warning As String = "Account is associated with other records (see below for details). Account information not deleted. \n\nDetails:\n\nOrders:\n"
			For Each order As KaOrder In orders
				warning &= New KaOrder(connection, order.Id).Number & " "
			Next
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidUsedInOrders", Utilities.JsAlert(warning))
		ElseIf KaCustomerAccountCombination.GetAll(connection, String.Format("deleted=0 AND customer_account_id='{0}'", ddlCustomers.SelectedValue), "").Count > 0 Then
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidCustAcctCombo", Utilities.JsAlert("Could not delete customer account """ & tbxName.Text & """ because it is part of a customer account combination."))
		Else ' it's OK to delete this customer account
			If Guid.Parse(ddlCustomers.SelectedValue) <> Guid.Empty Then
				With New KaCustomerAccount(GetUserConnection(_currentUser.Id), Guid.Parse(ddlCustomers.SelectedValue))
					.Deleted = True
					.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
				End With
				ClearUserData()
				PopulateAccountsList(Utilities.GetUser(Me))
				lblStatus.Text = "Selected account deleted successfully"
				btnDelete.Enabled = False
				pnlInterfaceSetup.Visible = False
			End If
			SetDisabledCondition()
		End If
	End Sub

	Protected Sub btnRemoveInterface_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnRemoveInterface.Click
		RemoveInterface()
	End Sub

	Protected Sub btnSaveInterface_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnSaveInterface.Click
		SaveInterface()
	End Sub

	Protected Sub ddlAccountInterface_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles ddlAccountInterface.SelectedIndexChanged
		btnRemoveInterface.Enabled = PopulateInterfaceInformation(Guid.Parse(ddlAccountInterface.SelectedValue))
	End Sub

#Region "Interfaces"
	Private Sub PopulateInterfaceList()
		ddlInterface.Items.Clear()
		ddlInterface.Items.Add(New ListItem("", Guid.Empty.ToString()))
		For Each i As KaInterface In KaInterface.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
			ddlInterface.Items.Add(New ListItem(i.Name, i.Id.ToString()))
		Next
	End Sub

	Private Sub SaveInterface()
		lblStatus.Text = ""
		If Guid.Parse(ddlCustomers.SelectedValue) = Guid.Empty Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidAccountNotSaved", Utilities.JsAlert("You must save the Account before you can set up interface cross references.")) : Exit Sub
		If Guid.Parse(ddlInterface.SelectedValue) = Guid.Empty Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidInterfaceSelected", Utilities.JsAlert("An interface must be selected. Interface settings not saved.")) : Exit Sub
		If tbxInterfaceCrossReference.Text.Length = 0 Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidCrossReference", Utilities.JsAlert("A cross reference must be specified. Interface settings not saved.")) : Exit Sub

		' If this is not export only, check if there are any other settings with the same cross reference ID
		If Not cbxExportOnly.Checked Then
			Dim allInterfaceSettings As ArrayList = KaCustomerAccountInterfaceSettings.GetAll(GetUserConnection(_currentUser.Id), KaCustomerAccountInterfaceSettings.FN_CROSS_REFERENCE & " = " & Q(tbxInterfaceCrossReference.Text.Trim) &
																							" AND " & KaCustomerAccountInterfaceSettings.FN_INTERFACE_ID & " = " & Q(ddlInterface.SelectedValue) &
																							" AND " & KaCustomerAccountInterfaceSettings.FN_DELETED & " = 0 " &
																							" AND " & KaCustomerAccountInterfaceSettings.FN_EXPORT_ONLY & " = 0 " &
																							" AND " & KaCustomerAccountInterfaceSettings.FN_ID & " <> " & Q(ddlAccountInterface.SelectedValue), "")
			If allInterfaceSettings.Count > 0 Then
				ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidDuplicateCrossReference", Utilities.JsAlert("A cross reference of " & tbxInterfaceCrossReference.Text.Trim & " already exists for this interface."))
				Exit Sub
			End If
		End If

		Dim acctInterfaceId As Guid = Guid.Parse(ddlAccountInterface.SelectedValue)
		If acctInterfaceId = Guid.Empty Then
			Dim acctInterface As New KaCustomerAccountInterfaceSettings
			acctInterface.CustomerAccountId = Guid.Parse(ddlCustomers.SelectedValue)
			acctInterface.InterfaceId = Guid.Parse(ddlInterface.SelectedValue)
			acctInterface.CrossReference = tbxInterfaceCrossReference.Text.Trim
			acctInterface.DefaultSetting = cbxDefaultSetting.Checked
			acctInterface.ExportOnly = cbxExportOnly.Checked
			acctInterface.SqlInsert(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
			acctInterfaceId = acctInterface.Id
		Else
			Dim acctInterface As New KaCustomerAccountInterfaceSettings(GetUserConnection(_currentUser.Id), acctInterfaceId)
			acctInterface.CustomerAccountId = Guid.Parse(ddlCustomers.SelectedValue)
			acctInterface.InterfaceId = Guid.Parse(ddlInterface.SelectedValue)
			acctInterface.CrossReference = tbxInterfaceCrossReference.Text.Trim
			acctInterface.DefaultSetting = cbxDefaultSetting.Checked
			acctInterface.ExportOnly = cbxExportOnly.Checked
			acctInterface.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
		End If

		PopulateAccountInterfaceList(Guid.Parse(ddlCustomers.SelectedValue))
		ddlAccountInterface.SelectedValue = acctInterfaceId.ToString
		ddlAccountInterface_SelectedIndexChanged(ddlAccountInterface, New EventArgs)
		btnRemoveInterface.Enabled = True
	End Sub

	Private Sub RemoveInterface()
		Dim selectedId As Guid = Guid.Parse(ddlAccountInterface.SelectedValue)
		If selectedId <> Guid.Empty Then
			Dim prodInterfaceSetting As New KaCustomerAccountInterfaceSettings(GetUserConnection(_currentUser.Id), selectedId)
			prodInterfaceSetting.Deleted = True
			prodInterfaceSetting.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
		End If
		PopulateAccountInterfaceList(Guid.Parse(ddlCustomers.SelectedValue))
		btnRemoveInterface.Enabled = PopulateInterfaceInformation(Guid.Empty)
	End Sub

	Private Sub DeleteInterfaceInformation(ByVal bulkProductId As Guid)
		For Each r As KaCustomerAccountInterfaceSettings In KaCustomerAccountInterfaceSettings.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND customer_account_id=" & Q(bulkProductId), "")
			r.Deleted = True
			r.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
		Next
	End Sub

	Private Sub PopulateAccountInterfaceList(ByVal supplierId As Guid)
		ddlAccountInterface.Items.Clear()
		ddlAccountInterface.Items.Add(New ListItem("Add an interface", Guid.Empty.ToString))
		For Each i As KaCustomerAccountInterfaceSettings In KaCustomerAccountInterfaceSettings.GetAll(GetUserConnection(_currentUser.Id), "deleted = 0 and customer_account_id = " & Q(supplierId), "")
			Dim inter As New KaInterface(GetUserConnection(_currentUser.Id), i.InterfaceId)
			Dim li As ListItem = New ListItem
			li.Text = inter.Name & " (" & i.CrossReference & ")"
			li.Value = i.Id.ToString
			ddlAccountInterface.Items.Add(li)
		Next
		ddlAccountInterface_SelectedIndexChanged(ddlAccountInterface, New EventArgs)
	End Sub

	Private Function PopulateInterfaceInformation(ByVal acctInterfaceId As Guid) As Boolean
		Dim retval As Boolean = False

		lblStatus.Text = ""
		If acctInterfaceId <> Guid.Empty Then
			Dim accountInterfaceSetting As New KaCustomerAccountInterfaceSettings(GetUserConnection(_currentUser.Id), acctInterfaceId)
			ddlInterface.SelectedValue = accountInterfaceSetting.InterfaceId.ToString
			tbxInterfaceCrossReference.Text = accountInterfaceSetting.CrossReference
			cbxDefaultSetting.Checked = accountInterfaceSetting.DefaultSetting
			cbxExportOnly.Checked = accountInterfaceSetting.ExportOnly
			retval = True
		Else
			ddlInterface.SelectedIndex = 0
			tbxInterfaceCrossReference.Text = ""
			ddlInterface_SelectedIndexChanged(ddlInterface, New EventArgs)
			cbxExportOnly.Checked = False
			retval = False
		End If

		Return retval
	End Function
#End Region

	Private Sub SetTextboxMaxLengths()
		tbxAcctNum.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaCustomerAccount.TABLE_NAME, "account_number"))
		tbxAddress.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaCustomerAccount.TABLE_NAME, "street"))
		tbxBillingNum.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaCustomerAccount.TABLE_NAME, "billing_account_number"))
		tbxCity.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaCustomerAccount.TABLE_NAME, "city"))
		tbxCoop.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaCustomerAccount.TABLE_NAME, "coop"))
		tbxCounty.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaCustomerAccount.TABLE_NAME, "county"))
		tbxEmail.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaCustomerAccount.TABLE_NAME, "email"))
		tbxInterfaceCrossReference.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaCustomerAccountInterfaceSettings.TABLE_NAME, "cross_reference"))
		tbxName.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaCustomerAccount.TABLE_NAME, "name"))
		tbxNotes.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaCustomerAccount.TABLE_NAME, "notes"))
		tbxPhone.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaCustomerAccount.TABLE_NAME, "phone"))
		tbxState.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaCustomerAccount.TABLE_NAME, "state"))
		tbxZip.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaCustomerAccount.TABLE_NAME, "zip_code"))
		tbxCountry.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaBranch.TABLE_NAME, "country"))
	End Sub

	Private Sub ddlInterface_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles ddlInterface.SelectedIndexChanged
		Dim count As Integer = 0
		Try
			Dim rdr As OleDb.OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), "SELECT count(*) " &
																		 "FROM " & KaCustomerAccountInterfaceSettings.TABLE_NAME & " " &
																		 "WHERE " & KaCustomerAccountInterfaceSettings.FN_DELETED & " = 0 " &
																		 "AND " & KaCustomerAccountInterfaceSettings.FN_INTERFACE_ID & " = " & Q(Guid.Parse(ddlInterface.SelectedValue)) & " " &
																		 "AND " & KaCustomerAccountInterfaceSettings.FN_CUSTOMER_ACCOUNT_ID & " = " & Q(Guid.Parse(ddlCustomers.SelectedValue)))
			If rdr.Read Then count = rdr.Item(0)
		Catch ex As Exception

		End Try
		cbxDefaultSetting.Checked = (count = 0)
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
			Dim shouldEnable = (.Edit AndAlso ddlCustomers.SelectedIndex > 0) OrElse (.Create AndAlso ddlCustomers.SelectedIndex = 0)
			pnlMain.Enabled = shouldEnable
			pnlInterfaceSettings.Enabled = shouldEnable
			btnSave.Enabled = shouldEnable
			btnDelete.Enabled = Not Guid.Parse(ddlCustomers.SelectedValue).Equals(Guid.Empty) AndAlso .Edit AndAlso .Delete
		End With
	End Sub

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

	Protected Sub lstDrivers_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lstDrivers.SelectedIndexChanged
		If lstDrivers.SelectedIndex >= 0 Then
			btnDeleteFromDriver.Enabled = _currentUserPermission(_currentTableName).Edit
		Else
			btnDeleteFromDriver.Enabled = False
		End If
	End Sub

	Private Function DriverName(ByVal driverId As Guid)
		Dim name As String = "?"
		For Each driver As KaDriver In KaDriver.GetAll(GetUserConnection(_currentUser.Id), "id=" & Q(driverId.ToString), "")
			name = driver.Name
		Next
		Return name
	End Function

	Protected Sub btnAddDriver_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnAddDriver.Click
		If ddlDrivers.SelectedIndex > 0 Then
			If ListContains(ddlDrivers.SelectedItem.Value, lstDrivers) Then
				Exit Sub
			End If
			lstDrivers.Items.Add(New ListItem(ddlDrivers.SelectedItem.Text, ddlDrivers.SelectedItem.Value))
			lstDrivers.SelectedIndex = lstDrivers.Items.Count - 1
			btnDeleteFromDriver.Enabled = True
			btnDeleteAllDrivers.Enabled = True
		End If
		ddlDrivers_SelectedIndexChanged(Nothing, Nothing)
	End Sub

	Private Function ListContains(ByVal driverId As String, list As ListBox) As Boolean
		For Each li As ListItem In list.Items
			If (li.Value = driverId) Then
				Return True
			End If
		Next
		Return False
	End Function

	Private Function EntryExists(ByVal accountId As String, ByVal driverId As String) As ArrayList
		Dim clauseString As String = "deleted=0 AND driver_id='" & driverId & "' AND customer_account_id ='" & accountId & "'"
		Return KaCustomerAccountDriver.GetAll(GetUserConnection(_currentUser.Id), clauseString, "")
	End Function

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
				btnDeleteFromDriver.Enabled = True
				btnDeleteAllDrivers.Enabled = True
			End If
		Next
		ddlDrivers_SelectedIndexChanged(Nothing, Nothing)
	End Sub

	Protected Sub btnDeleteFromDriver_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnDeleteFromDriver.Click
		lblStatus.Text = ""
		If lstDrivers.SelectedIndex <> -1 Then
			lstDrivers.Items.RemoveAt(lstDrivers.SelectedIndex)
			lstDrivers.SelectedIndex = -1
			btnDeleteFromDriver.Enabled = False
			If lstDrivers.Items.Count = 0 Then
				btnDeleteAllDrivers.Enabled = False
			End If
		End If
	End Sub

	Protected Sub btnDeleteAllDrivers_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnDeleteAllDrivers.Click
		lblStatus.Text = ""
		lstDrivers.Items.Clear()
		btnDeleteFromDriver.Enabled = False
		btnDeleteAllDrivers.Enabled = False
	End Sub
	Private Sub SaveDrivers(ByVal accountId As Guid)
		If accountId <> Guid.Empty Then
			Dim c As OleDbConnection = GetUserConnection(_currentUser.Id)
			Dim drivers As ArrayList = KaCustomerAccountDriver.GetAll(GetUserConnection(_currentUser.Id), "customer_account_id = " & Q(accountId), "last_updated desc")
			Dim updatedDriversGuids As List(Of Guid) = New List(Of Guid)
			Dim existingDriversGuids As List(Of Guid) = New List(Of Guid)

			'Compile list of Drivers records to be inserted/updated from lstDriver
			For Each li As ListItem In lstDrivers.Items
				updatedDriversGuids.Add(Guid.Parse(li.Value))
			Next

			'Iterate through existing drivers records from database, to update previously created records (either set deleted to true or false)
			For Each driver As KaCustomerAccountDriver In drivers
				If (Not existingDriversGuids.Contains(driver.DriverId)) Then
					If (updatedDriversGuids.Contains(driver.DriverId)) Then
						driver.Deleted = False
					Else
						driver.Deleted = True
					End If
					driver.LastUpdated = Now
					driver.SqlUpdate(c, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
					existingDriversGuids.Add(driver.DriverId)
				End If
			Next

			'Iterate through each Guid in the lstDriver list box to determine if a new record should be inserted
			For Each driverId As Guid In updatedDriversGuids
				If (Not existingDriversGuids.Contains(driverId)) Then
					With New KaCustomerAccountDriver()
						.Id = Guid.Empty
						.LastUpdated = Now
						.DriverId = driverId
						.CustomerAccountId = accountId
						.Deleted = False
						.SqlInsert(c, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
					End With
				End If
			Next
		End If
	End Sub
End Class
