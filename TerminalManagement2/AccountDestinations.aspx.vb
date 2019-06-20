Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class Destinations : Inherits System.Web.UI.Page
    Private _currentUser As KaUser
    Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
    Private _currentTableName As String = KaCustomerAccount.TABLE_NAME
    Private _customFields As New List(Of KaCustomField)
    Private _customFieldData As New List(Of KaCustomFieldData)

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Response.Cache.SetCacheability(HttpCacheability.NoCache)
        _currentUser = Utilities.GetUser(Me)
        _currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Accounts")
        If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")

        If Not Page.IsPostBack Then
            SetTextboxMaxLengths()
            PopulateAccountsList(_currentUser)
            PopulateInterfaceList()
            _customFields.Clear()
            For Each customField As KaCustomField In KaCustomField.GetAll(GetUserConnection(_currentUser.Id), String.Format("deleted=0 AND {0} = {1}", KaCustomField.FN_TABLE_NAME, Q(KaCustomerAccountLocation.TABLE_NAME)), KaCustomField.FN_FIELD_NAME)
                _customFields.Add(customField)
            Next
            Try
                Dim custAcctLocInfo As New KaCustomerAccount(GetUserConnection(_currentUser.Id), Guid.Parse(Page.Request("CustomerAccountLocationId")))
                ddlAccounts.SelectedValue = custAcctLocInfo.Id.ToString()
                ddlAccounts_SelectedIndexChanged(ddlAccounts, New EventArgs())
                ddlDestinations.SelectedValue = custAcctLocInfo.Id.ToString()
                ddlDestinations_SelectedIndexChanged(ddlDestinations, New EventArgs())
            Catch ex As Exception
                ddlAccounts.SelectedIndex = 0
                ddlAccounts_SelectedIndexChanged(ddlAccounts, New EventArgs)
            End Try
            Utilities.ConfirmBox(Me.btnDelete, "Are you sure you want to delete this account?")
            Utilities.SetFocus(tbxName, Me)
        End If
    End Sub

    Private Sub PopulateAccountsList(ByVal currentUser As KaUser)
        ddlAccounts.Items.Clear()
        ddlAccounts2.Items.Clear()
        ddlAccounts.Items.Add(New ListItem("Select an account", ""))
        ddlAccounts2.Items.Add(New ListItem("Select an account", ""))
        ddlAccounts.Items.Add(New ListItem("Available for all accounts", Guid.Empty.ToString()))
        ddlAccounts2.Items.Add(New ListItem("Available for all accounts", Guid.Empty.ToString()))
        For Each account As KaCustomerAccount In KaCustomerAccount.GetAll(GetUserConnection(currentUser.Id), "deleted=0 and is_supplier = 0" & IIf(currentUser.OwnerId <> Guid.Empty, String.Format(" AND (owner_id={0} OR owner_id={1})", Q(Guid.Empty), Q(currentUser.OwnerId)), ""), "name ASC")
            ddlAccounts.Items.Add(New ListItem(account.Name, account.Id.ToString()))
            ddlAccounts2.Items.Add(New ListItem(account.Name, account.Id.ToString()))
        Next
    End Sub

    Protected Sub ddlAccounts_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles ddlAccounts.SelectedIndexChanged
        lblStatus.Text = ""
        Dim accountId As Guid = Guid.Empty
        Dim validAccount As Boolean = Guid.TryParse(ddlAccounts.SelectedValue, accountId)
        ddlDestinations.Enabled = validAccount
        tbxAcres.Enabled = validAccount
        tbxCity.Enabled = validAccount
        tbxCrossRef.Enabled = validAccount
        tbxName.Enabled = validAccount
        ddlAccounts2.Enabled = validAccount
        ddlAccounts2.SelectedValue = accountId.ToString() ' setting account 2 drop down to selected value in header 
        tbxState.Enabled = validAccount
        tbxStreet.Enabled = validAccount
        tbxZip.Enabled = validAccount
        tbxCountry.Enabled = validAccount
        tbxEmail.Enabled = validAccount
        tbxNotes.Enabled = validAccount
        btnSave.Enabled = validAccount
        btnDelete.Enabled = False
        PopulateDestinationList(accountId)
        ddlDestinations.SelectedIndex = 0
        ddlDestinations_SelectedIndexChanged(ddlDestinations, New EventArgs)
    End Sub

    Protected Sub PopulateDestinationList(ByVal accountId As Guid)
        ddlDestinations.Items.Clear()
        ddlDestinations.Items.Add(New ListItem("Enter a new destination", Guid.Empty.ToString()))
        For Each destination As KaCustomerAccountLocation In KaCustomerAccountLocation.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND customer_account_id =" & Q(accountId.ToString), "name")
            ddlDestinations.Items.Add(New ListItem(destination.Name, destination.Id.ToString()))
        Next
    End Sub

    Protected Sub ddlDestinations_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles ddlDestinations.SelectedIndexChanged
        lblStatus.Text = ""
        Dim destinationId As Guid = Guid.Empty
        Guid.TryParse(ddlDestinations.SelectedValue, destinationId)
        PopulateDestinationForm(destinationId)
        SetControlUsabilityFromPermissions()
        pnlAccountLocationInterfaceSetup.Visible = (destinationId <> Guid.Empty)
        Utilities.CreateDynamicCustomFieldPanelControls(_customFields, _customFieldData, lstCustomFields, Page)
        Utilities.SetFocus(tbxName, Me)
    End Sub

    Protected Sub PopulateDestinationForm(ByVal id As Guid)
        Dim l As New KaCustomerAccountLocation()
        l.Id = id
        _customFieldData.Clear()

        If id <> Guid.Empty Then
            l.SqlSelect(GetUserConnection(_currentUser.Id))
            For Each customFieldValue As KaCustomFieldData In KaCustomFieldData.GetAll(GetUserConnection(_currentUser.Id), String.Format("deleted = 0 AND {0} = {1}", KaCustomFieldData.FN_RECORD_ID, Q(id)), KaCustomFieldData.FN_LAST_UPDATED)
                _customFieldData.Add(customFieldValue)
            Next
        End If
        tbxName.Text = l.Name
        If l.Id <> Guid.Empty Then
            'Don't do this as we want to keep the ddlAccounts2 set at the value it was set at earlier (in the ddlAccounts selectedIndexChanged).
            ddlAccounts2.SelectedValue = l.CustomerAccountId.ToString()
        End If
        tbxStreet.Text = l.Street
        tbxCity.Text = l.City
        tbxState.Text = l.State
        tbxZip.Text = l.ZipCode
        tbxCountry.Text = l.Country
        tbxAcres.Text = l.Acres
        tbxCrossRef.Text = l.CrossReference
        tbxEmail.Text = l.Email
        tbxNotes.Text = l.Notes
        PopulateAccountInterfaceList(id)
    End Sub

    Protected Sub btnSave_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnSave.Click
        If ValidateFields() Then
            With New KaCustomerAccountLocation()
                If Guid.Parse(ddlDestinations.SelectedValue) <> Guid.Empty Then .Id = Guid.Parse(ddlDestinations.SelectedValue)
                .CustomerAccountId = Guid.Parse(ddlAccounts2.SelectedValue) ' so when user saves account both drop down values will be the same
                .Name = tbxName.Text.Trim
                .Street = tbxStreet.Text.Trim
                .City = tbxCity.Text.Trim
                .State = tbxState.Text.Trim
                .ZipCode = tbxZip.Text.Trim
                .Country = tbxCountry.Text.Trim
                .Acres = tbxAcres.Text.Trim
                .CrossReference = tbxCrossRef.Text.Trim
                .Email = tbxEmail.Text.Trim
                .Notes = tbxNotes.Text.Trim
                Dim status As String = ""
                If .Id = Guid.Empty Then
                    .SqlInsert(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                    status = "New destination added successfully"
                Else
                    .SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                    status = "Selected destination updated successfully"
                End If

                Utilities.ConvertCustomFieldPanelToLists(_customFields, _customFieldData, lstCustomFields)
                For Each customData As KaCustomFieldData In _customFieldData
                    customData.RecordId = .Id
                    customData.SqlUpdateInsertIfNotFound(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                Next
                ddlAccounts.SelectedValue = .CustomerAccountId.ToString() ' when user saves the account drop down will match second account drop down 
                PopulateDestinationList(Guid.Parse(ddlAccounts2.SelectedValue)) ' why did I change this? 
                ddlDestinations.SelectedValue = .Id.ToString()
                ddlDestinations_SelectedIndexChanged(ddlDestinations, New EventArgs())
                lblStatus.Text = status
                pnlAccountLocationInterfaceSetup.Visible = True
            End With
            SetControlUsabilityFromPermissions()
        End If
    End Sub

    Private Function ValidateFields() As Boolean
        Dim retval As Boolean = True
        If tbxName.Text.Length = 0 Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidName", Utilities.JsAlert("Please specify a name for this destination.")) : Return False
        If KaCustomerAccountLocation.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND id <> " & Q(Guid.Parse(ddlDestinations.SelectedValue)) & " AND name=" & Q(tbxName.Text.Trim) & " and customer_account_id = " & Q(Guid.Parse(ddlAccounts.SelectedValue)), "").Count > 0 Then
            ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidDuplicateName", Utilities.JsAlert("The name " & tbxName.Text & " is already used by another destination. Please enter a different name."))
            Return False
        End If
        If Not IsNumeric(tbxAcres.Text) Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidCharAcres", Utilities.JsAlert("Please enter a numeric value for acres.")) : Return False
        Dim message As String = ""
        If Not Utilities.IsEmailFieldValid(tbxEmail.Text, message) Then
            ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidEmail", Utilities.JsAlert(message))
            Return False
        End If
        Return True
    End Function

    Protected Sub btnDelete_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnDelete.Click
        lblStatus.Text = ""
        Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
        Dim reader As OleDbDataReader = Tm2Database.ExecuteReader(connection, String.Format("SELECT DISTINCT orders.id AS order_id FROM orders, customer_accounts, customer_account_locations WHERE customer_account_locations.id={0} AND customer_account_locations.id=orders.customer_account_location_id AND orders.completed=0 AND orders.deleted=0", Q(ddlDestinations.SelectedValue)))
        Dim conditions As String = "id=" & Q(Guid.Empty)
        Do While reader.Read()
            conditions &= " OR id=" & Q(reader("order_id"))
        Loop
        reader.Close()
        Dim orders As ArrayList = KaOrder.GetAll(connection, conditions, "number ASC")

        If orders.Count > 0 Then
            Dim warning As String = "Destination is associated with other records (see below for details). Destination information not deleted. \n\nDetails:\n\nOrders:\n"
            For Each order In orders
                warning &= New KaOrder(connection, order.id).Number & " "
            Next
            ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidDestinationInUse", Utilities.JsAlert(warning))
        Else
            With New KaCustomerAccountLocation(connection, Guid.Parse(ddlDestinations.SelectedValue))
                .Deleted = True
                .SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                PopulateDestinationList(Guid.Parse(ddlAccounts.SelectedValue))
                ddlDestinations.SelectedIndex = 0
                PopulateDestinationForm(Guid.Empty)
                lblStatus.Text = "Destination deleted successfully"
                btnDelete.Enabled = False
                pnlAccountLocationInterfaceSetup.Visible = False
            End With
        End If
    End Sub

    Protected Sub btnRemoveInterface_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnRemoveInterface.Click
        RemoveInterface()
    End Sub

    Protected Sub btnSaveInterface_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnSaveInterface.Click
        SaveInterface()
    End Sub

    Protected Sub ddlAccountLocationInterface_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles ddlAccountLocationInterface.SelectedIndexChanged
        Dim selectedId As Guid = Guid.Empty
        Guid.TryParse(ddlAccountLocationInterface.SelectedValue, selectedId)
        btnRemoveInterface.Enabled = PopulateInterfaceInformation(selectedId)
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
        If Guid.Parse(ddlDestinations.SelectedValue) = Guid.Empty Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidNotSaved", Utilities.JsAlert("You must save the Destination before you can set up interface cross references.")) : Exit Sub
        If Guid.Parse(ddlInterface.SelectedValue) = Guid.Empty Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidInterface", Utilities.JsAlert("An interface must be selected. Interface settings not saved.")) : Exit Sub
        If tbxInterfaceCrossReference.Text.Length = 0 Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidCrossReference", Utilities.JsAlert("A cross reference must be specified. Interface settings not saved.")) : Exit Sub

        ' If this is not export only, check if there are any other settings with the same cross reference ID
        If Not chkExportOnly.Checked Then
            Dim allInterfaceSettings As ArrayList = KaCustomerAccountLocationInterfaceSettings.GetAll(GetUserConnection(_currentUser.Id), KaCustomerAccountLocationInterfaceSettings.FN_CROSS_REFERENCE & " = " & Q(tbxInterfaceCrossReference.Text.Trim) & _
                                                                                            " AND " & KaCustomerAccountLocationInterfaceSettings.FN_INTERFACE_ID & " = " & Q(ddlInterface.SelectedValue) & _
                                                                                            " AND " & KaCustomerAccountLocationInterfaceSettings.FN_DELETED & " = 0 " & _
                                                                                            " AND " & KaCustomerAccountLocationInterfaceSettings.FN_EXPORT_ONLY & " = 0 " & _
                                                                                            " AND " & KaCustomerAccountLocationInterfaceSettings.FN_ID & " <> " & Q(ddlAccountLocationInterface.SelectedValue), "")
            If allInterfaceSettings.Count > 0 Then
                ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidCrossReferenceDuplicate", Utilities.JsAlert("A cross reference of " & tbxInterfaceCrossReference.Text.Trim & " already exists for this interface."))
                Exit Sub
            End If
        End If

        Dim acctInterfaceId As Guid = Guid.Parse(ddlAccountLocationInterface.SelectedValue)
        If acctInterfaceId = Guid.Empty Then
            Dim destinationInterface As KaCustomerAccountLocationInterfaceSettings = New KaCustomerAccountLocationInterfaceSettings
            destinationInterface.CustomerAccountLocationId = Guid.Parse(ddlDestinations.SelectedValue)
            destinationInterface.InterfaceId = Guid.Parse(ddlInterface.SelectedValue)
            destinationInterface.CrossReference = tbxInterfaceCrossReference.Text.Trim
            destinationInterface.DefaultSetting = chkDefaultSetting.Checked
            destinationInterface.ExportOnly = chkExportOnly.Checked
            destinationInterface.SqlInsert(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
            acctInterfaceId = destinationInterface.Id
        Else
            Dim destinationInterface As KaCustomerAccountLocationInterfaceSettings = New KaCustomerAccountLocationInterfaceSettings(GetUserConnection(_currentUser.Id), acctInterfaceId)
            destinationInterface.CustomerAccountLocationId = Guid.Parse(ddlDestinations.SelectedValue)
            destinationInterface.InterfaceId = Guid.Parse(ddlInterface.SelectedValue)
            destinationInterface.CrossReference = tbxInterfaceCrossReference.Text.Trim
            destinationInterface.DefaultSetting = chkDefaultSetting.Checked
            destinationInterface.ExportOnly = chkExportOnly.Checked
            destinationInterface.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
        End If

        PopulateAccountInterfaceList(Guid.Parse(ddlDestinations.SelectedValue))
        ddlAccountLocationInterface.SelectedValue = acctInterfaceId.ToString
        btnRemoveInterface.Enabled = True
        ddlAccountLocationInterface_SelectedIndexChanged(ddlAccountLocationInterface, New EventArgs)
    End Sub

    Private Sub RemoveInterface()
        Dim selectedId As Guid = Guid.Parse(ddlAccountLocationInterface.SelectedValue)
        If selectedId <> Guid.Empty Then
            Dim prodInterfaceSetting As KaCustomerAccountLocationInterfaceSettings = New KaCustomerAccountLocationInterfaceSettings(GetUserConnection(_currentUser.Id), selectedId)
            prodInterfaceSetting.Deleted = True
            prodInterfaceSetting.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
        End If
        PopulateAccountInterfaceList(Guid.Parse(ddlDestinations.SelectedValue))
        btnRemoveInterface.Enabled = PopulateInterfaceInformation(Guid.Empty)
    End Sub

    Private Sub DeleteInterfaceInformation(ByVal bulkProductId As Guid)
        For Each r As KaCustomerAccountLocationInterfaceSettings In KaCustomerAccountLocationInterfaceSettings.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND customer_account_location_id=" & Q(bulkProductId), "")
            r.Deleted = True
            r.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
        Next
    End Sub

    Private Sub PopulateAccountInterfaceList(ByVal acctLocIntId As Guid)
        ddlAccountLocationInterface.Items.Clear()
        ddlAccountLocationInterface.Items.Add(New ListItem("Add an interface", Guid.Empty.ToString))
        For Each i As KaCustomerAccountLocationInterfaceSettings In KaCustomerAccountLocationInterfaceSettings.GetAll(GetUserConnection(_currentUser.Id), "deleted = 0 and customer_account_location_id = " & Q(acctLocIntId), "")
            Dim inter As New KaInterface(GetUserConnection(_currentUser.Id), i.InterfaceId)
            Dim li As ListItem = New ListItem
            li.Text = inter.Name & " (" & i.CrossReference & ")"
            li.Value = i.Id.ToString
            ddlAccountLocationInterface.Items.Add(li)
        Next
        ddlAccountLocationInterface_SelectedIndexChanged(ddlAccountLocationInterface, New EventArgs)
    End Sub

    Private Function PopulateInterfaceInformation(ByVal acctInterfaceId As Guid) As Boolean
        Dim retval As Boolean = False

        lblStatus.Text = ""
        If acctInterfaceId <> Guid.Empty Then
            Dim destinationInterfaceSetting As KaCustomerAccountLocationInterfaceSettings = New KaCustomerAccountLocationInterfaceSettings(GetUserConnection(_currentUser.Id), acctInterfaceId)
            ddlInterface.SelectedValue = destinationInterfaceSetting.InterfaceId.ToString
            tbxInterfaceCrossReference.Text = destinationInterfaceSetting.CrossReference
            chkDefaultSetting.Checked = destinationInterfaceSetting.DefaultSetting
            chkExportOnly.Checked = destinationInterfaceSetting.ExportOnly
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
        tbxAcres.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaCustomerAccountLocation.TABLE_NAME, KaCustomerAccountLocation.FN_ACRES))
        tbxCity.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaCustomerAccountLocation.TABLE_NAME, KaCustomerAccountLocation.FN_CITY))
        tbxCrossRef.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaCustomerAccountLocation.TABLE_NAME, KaCustomerAccountLocation.FN_CROSS_REFERENCE))
        tbxEmail.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaCustomerAccountLocation.TABLE_NAME, KaCustomerAccountLocation.FN_EMAIL))
        tbxInterfaceCrossReference.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaCustomerAccountLocationInterfaceSettings.TABLE_NAME, "cross_reference"))
        tbxName.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaCustomerAccountLocation.TABLE_NAME, KaCustomerAccountLocation.FN_NAME))
        tbxNotes.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaCustomerAccountLocation.TABLE_NAME, KaCustomerAccountLocation.FN_NOTES))
        tbxState.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaCustomerAccountLocation.TABLE_NAME, KaCustomerAccountLocation.FN_STATE))
        tbxStreet.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaCustomerAccountLocation.TABLE_NAME, KaCustomerAccountLocation.FN_STREET))
        tbxZip.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaCustomerAccountLocation.TABLE_NAME, KaCustomerAccountLocation.FN_ZIP_CODE))
    End Sub

    Private Sub ddlInterface_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles ddlInterface.SelectedIndexChanged
        Dim count As Integer = 0
        Try
            Dim rdr As OleDb.OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), "SELECT count(*) " & _
                                                                         "FROM " & KaCustomerAccountLocationInterfaceSettings.TABLE_NAME & " " & _
                                                                         "WHERE " & KaCustomerAccountLocationInterfaceSettings.FN_DELETED & " = 0 " & _
                                                                         "AND " & KaCustomerAccountLocationInterfaceSettings.FN_INTERFACE_ID & " = " & Q(Guid.Parse(ddlInterface.SelectedValue)) & " " & _
                                                                         "AND " & KaCustomerAccountLocationInterfaceSettings.FN_CUSTOMER_ACCOUNT_LOCATION_ID & " = " & Q(Guid.Parse(ddlDestinations.SelectedValue)))
            If rdr.Read Then count = rdr.Item(0)
        Catch ex As Exception

        End Try
        chkDefaultSetting.Checked = (count = 0)
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
            Dim shouldEnable = (.Edit AndAlso ddlAccounts.SelectedIndex > 0) OrElse (.Create AndAlso ddlDestinations.SelectedIndex = 0 AndAlso ddlAccounts.SelectedIndex > 0)
            pnlEven.Enabled = shouldEnable
            pnlInterfaceSettings.Enabled = shouldEnable
            btnSave.Enabled = shouldEnable
            btnDelete.Enabled = .Edit AndAlso .Delete AndAlso ddlAccounts.SelectedIndex > 0
        End With
    End Sub
End Class
