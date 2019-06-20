Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class Suppliers : Inherits System.Web.UI.Page

	Private _currentUser As KaUser
	Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
	Private _currentTableName As String = KaReceivingPurchaseOrder.TABLE_NAME

#Region "Events"
	Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
		Response.Cache.SetCacheability(HttpCacheability.NoCache)
		_currentUser = Utilities.GetUser(Me)
		_currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "PurchaseOrders")
		If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")

		If Not Page.IsPostBack Then
			SetTextboxMaxLengths()
			PopulateSupplierAccountList(_currentUser)
			PopulateOwnerList(_currentUser)
			PopulateInterfaceList()
			PopulateSupplierAccount()
			SetControlUsabilityFromPermissions()
			If Page.Request("SupplierAccountId") IsNot Nothing Then
				Try
					ddlSupplierAccounts.SelectedValue = Page.Request("SupplierAccountId")
				Catch ex As ArgumentOutOfRangeException
					ddlSupplierAccounts.SelectedIndex = 0
				End Try
			End If
			ddlSupplierAccounts_SelectedIndexChanged(ddlSupplierAccounts, New EventArgs)
			Utilities.ConfirmBox(Me.btnDelete, "Are you sure you want to delete this supplier account?")
			Utilities.SetFocus(tbxName, Me)
		End If
	End Sub

	Protected Sub ddlSupplierAccounts_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles ddlSupplierAccounts.SelectedIndexChanged
		PopulateSupplierAccount()
		SetControlUsabilityFromPermissions()
	End Sub

	Protected Sub btnSave_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnSave.Click
		lblStatus.Text = ""
		If tbxName.Text.Trim().Length = 0 Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidName", Utilities.JsAlert("Please specify a name")) : Exit Sub
		Dim message As String = ""
		If Not Utilities.IsEmailFieldValid(tbxEmail.Text, message) Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidEmail", Utilities.JsAlert(message))
		With New KaSupplierAccount()
			Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
			.Id = Guid.Parse(ddlSupplierAccounts.SelectedValue)
			If KaSupplierAccount.GetAll(connection, "deleted = 0 AND id <> " & Q(.Id) & " AND name = " & Q(tbxName.Text.Trim) & " AND is_supplier = 1", "").Count > 0 Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidDuplicateName", Utilities.JsAlert("The name """ & tbxName.Text & """ is already in use. Please specify a unique name for this supplier.")) : Exit Sub
			If tbxAccountNumber.Text.Trim.Length > 0 AndAlso KaSupplierAccount.GetAll(connection, "deleted=0 AND id <> " & Q(.Id) & " AND account_number = " & Q(tbxAccountNumber.Text.Trim), "").Count > 0 Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidCrossReference", Utilities.JsAlert("The default cross reference number """ & tbxAccountNumber.Text & """ is already in use. Please specify a unique default cross reference number for this supplier.")) : Exit Sub
			If Not .Id.Equals(Guid.Empty) Then .SqlSelect(connection)
			.OwnerId = Guid.Parse(ddlOwner.SelectedValue)
			.Name = tbxName.Text
			.AccountNumber = tbxAccountNumber.Text
			.Street = tbxStreet.Text
			.City = tbxCity.Text
			.State = tbxState.Text
			.ZipCode = tbxZipcode.Text
			.Phone = tbxPhone.Text
			.Email = tbxEmail.Text
			.IsSupplier = True
			If .Id.Equals(Guid.Empty) Then
				.SqlUpdateInsertIfNotFound(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
				lblStatus.Text = "Supplier account successfully added."
				btnDelete.Enabled = True
				pnlInterfaceSetup.Visible = KaInterface.GetAll(connection, "deleted=0", "name ASC").Count > 0
			Else
				.SqlUpdateInsertIfNotFound(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
				lblStatus.Text = "Supplier account successfully updated."
			End If
			PopulateSupplierAccountList(Utilities.GetUser(Me))
			ddlSupplierAccounts.SelectedValue = .Id.ToString()
		End With
	End Sub

	Protected Sub btnDelete_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnDelete.Click
		lblStatus.Text = ""
		With New KaSupplierAccount(GetUserConnection(_currentUser.Id), Guid.Parse(ddlSupplierAccounts.SelectedValue))
			.Deleted = True
			.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
			PopulateSupplierAccountList(Utilities.GetUser(Me))
			PopulateSupplierAccount()
			lblStatus.Text = "Supplier account successfully deleted."
		End With
	End Sub
#End Region

	Private Sub PopulateSupplierAccountList(ByVal currentUser As KaUser)
		ddlSupplierAccounts.Items.Clear()
		If _currentUserPermission(_currentTableName).Create Then
			ddlSupplierAccounts.Items.Add(New ListItem("Enter a new supplier account", Guid.Empty.ToString()))
		Else
			ddlSupplierAccounts.Items.Add(New ListItem("Select a supplier account", Guid.Empty.ToString()))
		End If
		For Each r As KaSupplierAccount In KaSupplierAccount.GetAll(GetUserConnection(currentUser.Id), "deleted=0" & IIf(currentUser.OwnerId = Guid.Empty, "", " AND (owner_id = " & Q(currentUser.OwnerId) & " OR owner_id = " & Q(Guid.Empty) & ")"), "name ASC")
			ddlSupplierAccounts.Items.Add(New ListItem(r.Name, r.Id.ToString()))
		Next
	End Sub

	Private Sub PopulateOwnerList(ByVal currentUser As KaUser)
		ddlOwner.Items.Clear()
		ddlOwner.Items.Add(New ListItem("All owners", Guid.Empty.ToString()))
		For Each r As KaOwner In KaOwner.GetAll(GetUserConnection(currentUser.Id), "deleted=0" & IIf(currentUser.OwnerId = Guid.Empty, "", " AND id=" & Q(currentUser.OwnerId)), "name ASC")
			ddlOwner.Items.Add(New ListItem(r.Name, r.Id.ToString()))
		Next
	End Sub

	Private Sub PopulateSupplierAccount()
		lblStatus.Text = ""
		With New KaSupplierAccount()
			.Id = Guid.Parse(ddlSupplierAccounts.SelectedValue)
			If .Id = Guid.Empty Then
				btnDelete.Enabled = False
				pnlInterfaceSetup.Visible = False
			Else
				Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
				.SqlSelect(connection)
				btnDelete.Enabled = True
				pnlInterfaceSetup.Visible = KaInterface.GetAll(connection, "deleted=0", "name ASC").Count > 0
			End If
			Try
				ddlOwner.SelectedValue = .OwnerId.ToString()
			Catch ex As ArgumentOutOfRangeException
				ddlOwner.SelectedIndex = 0
				If Not .Id.Equals(Guid.Empty) Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidOwnerId", Utilities.JsAlert("Record not found in owners where ID = " & .OwnerId.ToString() & " Owner not set."))
			End Try
			tbxName.Text = .Name
			tbxAccountNumber.Text = .AccountNumber
			tbxStreet.Text = .Street
			tbxCity.Text = .City
			tbxState.Text = .State
			tbxZipcode.Text = .ZipCode
			tbxPhone.Text = .Phone
			tbxEmail.Text = .Email
			PopulateAccountInterfaceList(.Id)
		End With
	End Sub

	Protected Sub btnRemoveInterface_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnRemoveInterface.Click
		RemoveInterface()
	End Sub

	Protected Sub btnSaveInterface_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnSaveInterface.Click
		SaveInterface()
	End Sub

	Protected Sub ddlSupplierInterface_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles ddlSupplierInterface.SelectedIndexChanged
		btnRemoveInterface.Enabled = PopulateInterfaceInformation(Guid.Parse(ddlSupplierInterface.SelectedValue))
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
		If Guid.Parse(ddlSupplierAccounts.SelectedValue) = Guid.Empty Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidNotSaved", Utilities.JsAlert("You must save the Account before you can set up interface cross references.")) : Exit Sub
		If Guid.Parse(ddlInterface.SelectedValue) = Guid.Empty Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidInterface", Utilities.JsAlert("An interface must be selected. Interface settings not saved.")) : Exit Sub
		If tbxInterfaceCrossReference.Text.Length = 0 Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidCrossReference", Utilities.JsAlert("A cross reference must be specified. Interface settings not saved.")) : Exit Sub

		' If this is not export only, check if there are any other settings with the same cross reference ID
		If Not chkExportOnly.Checked Then
			Dim allInterfaceSettings As ArrayList = KaSupplierAccountInterfaceSettings.GetAll(GetUserConnection(_currentUser.Id), KaSupplierAccountInterfaceSettings.FN_CROSS_REFERENCE & " = " & Q(tbxInterfaceCrossReference.Text.Trim) &
						" AND " & KaSupplierAccountInterfaceSettings.FN_INTERFACE_ID & " = " & Q(ddlInterface.SelectedValue) &
						" AND " & KaSupplierAccountInterfaceSettings.FN_DELETED & " = 0 " &
						" AND " & KaSupplierAccountInterfaceSettings.FN_EXPORT_ONLY & " = 0 " &
						" AND " & KaSupplierAccountInterfaceSettings.FN_ID & " <> " & Q(ddlSupplierInterface.SelectedValue), "")
			If allInterfaceSettings.Count > 0 Then
				ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidCrossReferenceExists", Utilities.JsAlert("A cross reference of " & tbxInterfaceCrossReference.Text.Trim & " already exists for this interface."))
				Exit Sub
			End If
		End If

		Dim acctInterfaceId As Guid = Guid.Parse(ddlSupplierInterface.SelectedValue)
		If acctInterfaceId = Guid.Empty Then
			Dim supplierInterface As KaSupplierAccountInterfaceSettings = New KaSupplierAccountInterfaceSettings
			supplierInterface.SupplierAccountId = Guid.Parse(ddlSupplierAccounts.SelectedValue)
			supplierInterface.InterfaceId = Guid.Parse(ddlInterface.SelectedValue)
			supplierInterface.CrossReference = tbxInterfaceCrossReference.Text.Trim
			supplierInterface.DefaultSetting = chkDefaultSetting.Checked
			supplierInterface.ExportOnly = chkExportOnly.Checked
			supplierInterface.SqlInsert(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
			acctInterfaceId = supplierInterface.Id
		Else
			Dim supplierInterface As KaSupplierAccountInterfaceSettings = New KaSupplierAccountInterfaceSettings(GetUserConnection(_currentUser.Id), acctInterfaceId)
			supplierInterface.SupplierAccountId = Guid.Parse(ddlSupplierAccounts.SelectedValue)
			supplierInterface.InterfaceId = Guid.Parse(ddlInterface.SelectedValue)
			supplierInterface.CrossReference = tbxInterfaceCrossReference.Text.Trim
			supplierInterface.DefaultSetting = chkDefaultSetting.Checked
			supplierInterface.ExportOnly = chkExportOnly.Checked
			supplierInterface.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
		End If

		PopulateAccountInterfaceList(Guid.Parse(ddlSupplierAccounts.SelectedValue))
		ddlSupplierInterface.SelectedValue = acctInterfaceId.ToString
		ddlSupplierInterface_SelectedIndexChanged(ddlSupplierInterface, New EventArgs)
		btnRemoveInterface.Enabled = True
	End Sub

	Private Sub RemoveInterface()
		Dim selectedId As Guid = Guid.Parse(ddlSupplierInterface.SelectedValue)
		If selectedId <> Guid.Empty Then
			Dim prodInterfaceSetting As KaSupplierAccountInterfaceSettings = New KaSupplierAccountInterfaceSettings(GetUserConnection(_currentUser.Id), selectedId)
			prodInterfaceSetting.Deleted = True
			prodInterfaceSetting.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
		End If
		PopulateAccountInterfaceList(Guid.Parse(ddlSupplierAccounts.SelectedValue))
		btnRemoveInterface.Enabled = PopulateInterfaceInformation(Guid.Empty)
	End Sub

	Private Sub DeleteInterfaceInformation(ByVal bulkProductId As Guid)
		For Each r As KaSupplierAccountInterfaceSettings In KaSupplierAccountInterfaceSettings.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND supplier_account_id=" & Q(bulkProductId), "")
			r.Deleted = True
			r.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
		Next
	End Sub

	Private Sub PopulateAccountInterfaceList(ByVal supplierId As Guid)
		ddlSupplierInterface.Items.Clear()
		ddlSupplierInterface.Items.Add(New ListItem("Add an interface", Guid.Empty.ToString))
		For Each i As KaSupplierAccountInterfaceSettings In KaSupplierAccountInterfaceSettings.GetAll(GetUserConnection(_currentUser.Id), "deleted = 0 and supplier_account_id = " & Q(supplierId), "")
			Dim inter As New KaInterface(GetUserConnection(_currentUser.Id), i.InterfaceId)
			Dim li As ListItem = New ListItem
			li.Text = inter.Name & " (" & i.CrossReference & ")"
			li.Value = i.Id.ToString
			ddlSupplierInterface.Items.Add(li)
		Next
		ddlSupplierInterface_SelectedIndexChanged(ddlSupplierInterface, New EventArgs)
	End Sub

	Private Function PopulateInterfaceInformation(ByVal acctInterfaceId As Guid) As Boolean
		Dim retval As Boolean = False

		lblStatus.Text = ""
		If acctInterfaceId <> Guid.Empty Then
			Dim supplierInterfaceSetting As KaSupplierAccountInterfaceSettings = New KaSupplierAccountInterfaceSettings(GetUserConnection(_currentUser.Id), acctInterfaceId)
			ddlInterface.SelectedValue = supplierInterfaceSetting.InterfaceId.ToString
			tbxInterfaceCrossReference.Text = supplierInterfaceSetting.CrossReference
			chkDefaultSetting.Checked = supplierInterfaceSetting.DefaultSetting
			chkExportOnly.Checked = supplierInterfaceSetting.ExportOnly
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
#End Region

	Private Sub SetTextboxMaxLengths()
		tbxAccountNumber.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaSupplierAccount.TABLE_NAME, "account_number"))
		tbxCity.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaSupplierAccount.TABLE_NAME, "city"))
		tbxEmail.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaSupplierAccount.TABLE_NAME, "email"))
		tbxInterfaceCrossReference.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaSupplierAccountInterfaceSettings.TABLE_NAME, "cross_reference"))
		tbxName.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaSupplierAccount.TABLE_NAME, "name"))
		tbxPhone.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaSupplierAccount.TABLE_NAME, "phone"))
		tbxState.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaSupplierAccount.TABLE_NAME, "state"))
		tbxStreet.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaSupplierAccount.TABLE_NAME, "street"))
		tbxZipcode.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaSupplierAccount.TABLE_NAME, "zip_code"))
	End Sub

	Private Sub ddlInterface_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles ddlInterface.SelectedIndexChanged
		Dim count As Integer = 0
		Try
			Dim rdr As OleDb.OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), "SELECT count(*) " &
					"FROM " & KaSupplierAccountInterfaceSettings.TABLE_NAME & " " &
					"WHERE " & KaSupplierAccountInterfaceSettings.FN_DELETED & " = 0 " &
					"AND " & KaSupplierAccountInterfaceSettings.FN_INTERFACE_ID & " = " & Q(Guid.Parse(ddlInterface.SelectedValue)) & " " &
					"AND " & KaSupplierAccountInterfaceSettings.FN_SUPPLIER_ACCOUNT_ID & " = " & Q(Guid.Parse(ddlSupplierAccounts.SelectedValue)))
			If rdr.Read Then count = rdr.Item(0)
		Catch ex As Exception

		End Try
		chkDefaultSetting.Checked = (count = 0)
	End Sub

	Private Sub SetControlUsabilityFromPermissions()
		With _currentUserPermission(_currentTableName)
			Dim shouldEnable = (.Edit AndAlso ddlSupplierAccounts.SelectedIndex > 0) OrElse (.Create AndAlso ddlSupplierAccounts.SelectedIndex = 0)
			pnlEven.Enabled = shouldEnable
			pnlInterfaceSettings.Enabled = shouldEnable
			btnSave.Enabled = shouldEnable
			Dim value = Guid.Parse(ddlSupplierAccounts.SelectedValue)
			btnDelete.Enabled = .Edit AndAlso .Delete AndAlso value <> Guid.Empty
		End With
	End Sub
End Class