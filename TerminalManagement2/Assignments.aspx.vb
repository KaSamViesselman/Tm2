Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class Assignments : Inherits System.Web.UI.Page
    Private _currentUser As KaUser
    Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
    Private _currentTableName As String = KaLocation.TABLE_NAME

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Response.Cache.SetCacheability(HttpCacheability.NoCache)
        _currentUser = Utilities.GetUser(Me)
        _currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Facilities")
        If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")
        pnlAccounts.Visible = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaCustomerAccount.TABLE_NAME}), "Accounts")(KaCustomerAccount.TABLE_NAME).Read
        pnlDrivers.Visible = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaDriver.TABLE_NAME}), "Drivers")(KaDriver.TABLE_NAME).Read
        If Not Page.IsPostBack Then
            PopulateLocationsList()
            Utilities.ConfirmBox(Me.btnRemoveDriver, "Are you sure you want to remove this driver?")
            Utilities.ConfirmBox(Me.btnRemoveAllDrivers, "Are you sure you want to remove all drivers?")
            Utilities.ConfirmBox(Me.btnRemoveAccount, "Are you sure you want to remove this account?")
            Utilities.ConfirmBox(Me.btnRemoveAllAccounts, "Are you sure you want to remove all accounts?")
            ddlLocations_SelectedIndexChanged(ddlLocations, New EventArgs())
        End If
    End Sub

    Protected Sub ddlLocations_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles ddlLocations.SelectedIndexChanged
        lblStatus.Text = ""
        Dim locationId As Guid = Guid.Parse(ddlLocations.SelectedValue)
        If locationId <> Guid.Empty Then
            With _currentUserPermission(_currentTableName)
                SetControlUsability(.Edit)
            End With
        Else
            SetControlUsability(False)
            lstDrivers.Items.Clear()
            lstAccounts.Items.Clear()
        End If
        PopulateDriverData(locationId)
        PopulateAccountData(locationId)
    End Sub

    Protected Sub ddlDrivers_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlDrivers.SelectedIndexChanged
        Dim driverId As Guid = Guid.Parse(ddlDrivers.SelectedValue)
        If driverId <> Guid.Empty Then
            Dim notFound As Boolean = True
            For Each item As ListItem In lstDrivers.Items
                If New KaLocationDriverAccess(GetUserConnection(_currentUser.Id), Guid.Parse(item.Value)).DriverId = driverId Then
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
                If New KaLocationCustomerAccountAccess(GetUserConnection(_currentUser.Id), Guid.Parse(item.Value)).CustomerAccountId = accountId Then
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

    Protected Sub btnAddDriver_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnAddDriver.Click
        If Guid.Parse(ddlLocations.SelectedValue) <> Guid.Empty And _
          Guid.Parse(ddlDrivers.SelectedValue) <> Guid.Empty Then
            Dim existingList As ArrayList = DriverEntryExists(ddlLocations.SelectedValue, ddlDrivers.SelectedValue)
            If Not existingList.Count > 0 Then
                With New KaLocationDriverAccess()
                    .DriverId = Guid.Parse(ddlDrivers.SelectedValue)
                    .LocationId = Guid.Parse(ddlLocations.SelectedValue)
                    .Deleted = False
                    .SqlInsert(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                End With
                lblStatus.Text = "Driver added to selected facility"
                PopulateDriverData(Guid.Parse(ddlLocations.SelectedValue))
            Else
                Dim existingAcctDriver As KaLocationDriverAccess = existingList.Item(0)
                If existingAcctDriver.Deleted Then
                    With existingAcctDriver
                        .Deleted = False
                        .SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                    End With
                    lblStatus.Text = "Driver added to selected facility"
                    PopulateDriverData(Guid.Parse(ddlLocations.SelectedValue))
                End If
            End If
        End If
        ddlDrivers_SelectedIndexChanged(Nothing, Nothing)
    End Sub

    Protected Sub btnAddAccount_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnAddAccount.Click
        If Guid.Parse(ddlLocations.SelectedValue) <> Guid.Empty And _
          Guid.Parse(ddlAccounts.SelectedValue) <> Guid.Empty Then
            Dim existingList As ArrayList = DriverEntryExists(ddlLocations.SelectedValue, ddlAccounts.SelectedValue)
            If Not existingList.Count > 0 Then
                With New KaLocationCustomerAccountAccess()
                    .CustomerAccountId = Guid.Parse(ddlAccounts.SelectedValue)
                    .LocationId = Guid.Parse(ddlLocations.SelectedValue)
                    .Deleted = False
                    .SqlInsert(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                End With
                lblStatus.Text = "Account added to selected facility"
                PopulateAccountData(Guid.Parse(ddlLocations.SelectedValue))
            Else
                Dim existingAcctDriver As KaLocationCustomerAccountAccess = existingList.Item(0)
                If existingAcctDriver.Deleted Then
                    With existingAcctDriver
                        .Deleted = False
                        .SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                    End With
                    lblStatus.Text = "Account added to selected facility"
                    PopulateAccountData(Guid.Parse(ddlLocations.SelectedValue))
                End If
            End If
        End If
        ddlAccounts_SelectedIndexChanged(Nothing, Nothing)
    End Sub

    Protected Sub btnAddAllDrivers_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnAddAllDrivers.Click
        Dim drivers As ArrayList = New ArrayList

        'Get all driver ids from active Locations
        For Each driver As KaDriver In KaDriver.GetAll(GetUserConnection(_currentUser.Id), "", "")
            drivers.Add(driver.Id)
        Next

        'Insert this location into all of the drivers gathered
        Dim index As Integer = 0
        For index = 0 To drivers.Count - 1
            Dim existingList As ArrayList = DriverEntryExists(ddlLocations.SelectedValue, drivers(index).ToString)
            If Not existingList.Count > 0 Then
                With New KaLocationDriverAccess()
                    .DriverId = drivers(index)
                    .LocationId = Guid.Parse(ddlLocations.SelectedValue)
                    .Deleted = False
                    .SqlInsert(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                End With
            Else
                Dim existingLocationDriver As KaLocationDriverAccess = existingList.Item(0)
                If existingLocationDriver.Deleted Then
                    With existingLocationDriver
                        .Deleted = False
                        .SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                    End With
                End If
            End If
        Next
        lblStatus.Text = "All drivers added to this facility"
        PopulateDriverData(Guid.Parse(ddlLocations.SelectedValue))
        ddlDrivers_SelectedIndexChanged(Nothing, Nothing)
    End Sub

    Protected Sub btnAddAllAccounts_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnAddAllAccounts.Click
        Dim accounts As ArrayList = New ArrayList

        'Get all account ids from active Locations
        For Each account As KaCustomerAccount In KaCustomerAccount.GetAll(GetUserConnection(_currentUser.Id), "", "")
            If Not account.IsSupplier Then
                accounts.Add(account.Id)
            End If
        Next
        'Insert this location into all of the accounts gathered
        Dim index As Integer = 0
        For index = 0 To accounts.Count - 1
            Dim existingList As ArrayList = AccountEntryExists(ddlLocations.SelectedValue, accounts(index).ToString)

            If Not existingList.Count > 0 Then
                With New KaLocationCustomerAccountAccess()
                    .CustomerAccountId = accounts(index)
                    .LocationId = Guid.Parse(ddlLocations.SelectedValue)
                    .Deleted = False
                    .SqlInsert(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                End With
            Else
                Dim existingLocationAccount As KaLocationCustomerAccountAccess = existingList.Item(0)
                If existingLocationAccount.Deleted Then
                    With existingLocationAccount
                        .Deleted = False
                        .SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                    End With
                End If
            End If
        Next
        lblStatus.Text = "All accounts added to this facility"
        PopulateAccountData(Guid.Parse(ddlLocations.SelectedValue))
        ddlAccounts_SelectedIndexChanged(Nothing, Nothing)
    End Sub

    Protected Sub btnRemoveDriver_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnRemoveDriver.Click
        lblStatus.Text = ""
        If lstDrivers.SelectedIndex <> -1 Then
            If Guid.Parse(lstDrivers.SelectedValue) <> Guid.Empty Then
                Dim result As ArrayList = KaLocationDriverAccess.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND id='" & lstDrivers.SelectedValue & "'", "")
                Dim customerDriver As KaLocationDriverAccess = result(0)
                With customerDriver
                    .Deleted = True
                    .SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                End With
                lstDrivers.Items.RemoveAt(lstDrivers.SelectedIndex)
                ddlDrivers.Items.Clear()
                ddlLocations_SelectedIndexChanged(ddlLocations, New EventArgs())
                lstDrivers_SelectedIndexChanged(Nothing, Nothing)
                lblStatus.Text = "Selected driver removed successfully"
            End If
        End If
    End Sub

    Protected Sub btnRemoveAccount_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnRemoveAccount.Click
        lblStatus.Text = ""
        If lstAccounts.SelectedIndex <> -1 Then
            If Guid.Parse(lstAccounts.SelectedValue) <> Guid.Empty Then
                Dim result As ArrayList = KaLocationCustomerAccountAccess.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND id='" & lstAccounts.SelectedValue & "'", "")
                Dim customeraccount As KaLocationCustomerAccountAccess = result(0)
                With customeraccount
                    .Deleted = True
                    .SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                End With
                lstAccounts.Items.RemoveAt(lstAccounts.SelectedIndex)
                ddlAccounts.Items.Clear()
                ddlLocations_SelectedIndexChanged(ddlLocations, New EventArgs())
                lstAccounts_SelectedIndexChanged(Nothing, Nothing)
                lblStatus.Text = "Selected account removed successfully"
            End If
        End If
    End Sub

    Protected Sub btnRemoveAllDrivers_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnRemoveAllDrivers.Click
        lblStatus.Text = ""
        Dim allEntries As ArrayList = New ArrayList
        If Guid.Parse(ddlLocations.SelectedValue) <> Guid.Empty Then
            allEntries = KaLocationDriverAccess.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND location_id = '" & ddlLocations.SelectedValue & "'", "")
            Dim counter As Integer = 0
            For counter = 0 To allEntries.Count - 1
                With CType(allEntries(counter), KaLocationDriverAccess)
                    .Deleted = True
                    .SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                End With
            Next
            ddlDrivers.Items.Clear()
            PopulateDriverData(Guid.Parse(ddlLocations.SelectedValue))
            lblStatus.Text = "All drivers removed successfully"
        End If
    End Sub

    Protected Sub btnRemoveAllAccounts_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnRemoveAllAccounts.Click
        lblStatus.Text = ""
        Dim allEntries As ArrayList = New ArrayList
        If Guid.Parse(ddlLocations.SelectedValue) <> Guid.Empty Then
            allEntries = KaLocationCustomerAccountAccess.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND location_id = '" & ddlLocations.SelectedValue & "'", "")
            Dim counter As Integer = 0
            For counter = 0 To allEntries.Count - 1
                With CType(allEntries(counter), KaLocationCustomerAccountAccess)
                    .Deleted = True
                    .SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                End With
            Next
            ddlAccounts.Items.Clear()
            PopulateAccountData(Guid.Parse(ddlLocations.SelectedValue))
            lblStatus.Text = "All accounts removed successfully"
        End If
    End Sub

    Private Sub PopulateLocationsList()
        ddlLocations.Items.Clear()
        ddlLocations.Items.Add(New ListItem("Select a facility", Guid.Empty.ToString()))
        For Each location As KaLocation In KaLocation.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
            ddlLocations.Items.Add(New ListItem(location.Name, location.Id.ToString()))
        Next
    End Sub

    Private Function DriverEntryExists(ByVal locationId As String, ByVal driverId As String) As ArrayList
        Dim clauseString As String = "deleted=0 AND driver_id='" & driverId & "' AND location_id ='" & locationId & "'"
        Return KaLocationDriverAccess.GetAll(GetUserConnection(_currentUser.Id), clauseString, "")
    End Function

    Private Function AccountEntryExists(ByVal locationId As String, ByVal accountId As String) As ArrayList
        Dim clauseString As String = "deleted=0 AND customer_account_id='" & accountId & "' AND location_id ='" & locationId & "'"
        Return KaLocationCustomerAccountAccess.GetAll(GetUserConnection(_currentUser.Id), clauseString, "")
    End Function

    Private Function DriverName(ByVal driverId As Guid)
        Dim name As String = "?"
        For Each driver As KaDriver In KaDriver.GetAll(GetUserConnection(_currentUser.Id), "id=" & Q(driverId.ToString), "")
            name = driver.Name
        Next
        Return name
    End Function

    Private Function AccountName(ByVal accountId As Guid)
        Dim name As String = "?"
        For Each account As KaCustomerAccount In KaCustomerAccount.GetAll(GetUserConnection(_currentUser.Id), "id=" & Q(accountId.ToString), "")
            name = account.Name
        Next
        Return name
    End Function

    Private Sub PopulateDriverData(ByVal locationId As Guid)
        lstDrivers.Items.Clear()

        Dim getAssignedDriversRdr As OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), "SELECT        location_driver_access.id, drivers.name + CASE WHEN drivers.valid_for_all_locations = 1 THEN ' (Valid for all locations)' ELSE '' END AS name " & _
                                    "FROM location_driver_access " & _
                                    "INNER JOIN drivers ON drivers.id = location_driver_access.driver_id " & _
                                    "WHERE (location_driver_access.deleted = 0) AND (location_driver_access.location_id = " & Q(locationId) & ") " & _
                                    "ORDER BY name")
        Do While getAssignedDriversRdr.Read()
            lstDrivers.Items.Add(New ListItem(getAssignedDriversRdr.Item("name"), getAssignedDriversRdr.Item("id").ToString))
        Loop
        getAssignedDriversRdr.Close()
        ddlDrivers.Items.Clear()
        ddlDrivers.Items.Add(New ListItem("Select a driver", Guid.Empty.ToString()))
        Dim getDriversAvailRdr As OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), "SELECT id, name, valid_for_all_locations FROM " & KaDriver.TABLE_NAME & " WHERE deleted=0 AND NOT id IN (SELECT driver_id FROM " & KaLocationDriverAccess.TABLE_NAME & " WHERE deleted=0 AND location_driver_access.location_id =" & Q(locationId) & ") order by name")

        Do While getDriversAvailRdr.Read()
            ddlDrivers.Items.Add(New ListItem(getDriversAvailRdr.Item("Name") & IIf(getDriversAvailRdr.Item("valid_for_all_locations"), " (Valid for all locations)", ""), getDriversAvailRdr.Item("Id").ToString()))
        Loop
        getDriversAvailRdr.Close()
        ddlDrivers_SelectedIndexChanged(Nothing, Nothing)
        lstDrivers_SelectedIndexChanged(Nothing, Nothing)
    End Sub

    Private Sub PopulateAccountData(ByVal locationId As Guid)
        lstAccounts.Items.Clear()

        Dim getAssignedAccountsRdr As OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), "SELECT location_customer_access.id, customer_accounts.name + CASE WHEN customer_accounts.valid_for_all_locations = 1 THEN ' (Valid for all locations)' ELSE '' END AS name " &
                                    "FROM location_customer_access " &
                                    "INNER JOIN customer_accounts ON customer_accounts.id = location_customer_access.customer_account_id " &
                                    "WHERE (customer_accounts.deleted = 0) AND (location_customer_access.deleted = 0) AND (location_customer_access.location_id = " & Q(locationId) & ") " &
                                    "ORDER BY name")
        Do While getAssignedAccountsRdr.Read()
            lstAccounts.Items.Add(New ListItem(getAssignedAccountsRdr.Item("name"), getAssignedAccountsRdr.Item("id").ToString))
        Loop
        getAssignedAccountsRdr.Close()
        ddlAccounts.Items.Clear()
        ddlAccounts.Items.Add(New ListItem("Select an account", Guid.Empty.ToString()))
        Dim getAccountsAvailRdr As OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), "SELECT id, name, valid_for_all_locations FROM " & KaCustomerAccount.TABLE_NAME & " WHERE deleted=0 AND is_supplier = 0 AND NOT id IN (SELECT customer_account_id FROM " & KaLocationCustomerAccountAccess.TABLE_NAME & " WHERE deleted=0 AND location_customer_access.location_id =" & Q(locationId) & ") order by name")

        Do While getAccountsAvailRdr.Read()
            ddlAccounts.Items.Add(New ListItem(getAccountsAvailRdr.Item("Name") & IIf(getAccountsAvailRdr.Item("valid_for_all_locations"), " (Valid for all locations)", ""), getAccountsAvailRdr.Item("Id").ToString()))
        Loop
        getAccountsAvailRdr.Close()
        ddlAccounts_SelectedIndexChanged(Nothing, Nothing)
        lstAccounts_SelectedIndexChanged(Nothing, Nothing)
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
End Class