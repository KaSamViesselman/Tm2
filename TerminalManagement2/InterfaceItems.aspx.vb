Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class InterfaceUpdateCopy
    Inherits System.Web.UI.Page
    Private _currentUser As KaUser
    Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
    Private _currentTableName As String = KaInterface.TABLE_NAME
    Private _currentItemType As String = ""
    Private _selectedInterface1Value As Guid = Guid.Empty
    Private _selectedInterface2Value As Guid = Guid.Empty

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Response.Cache.SetCacheability(HttpCacheability.NoCache)
        _currentUser = Utilities.GetUser(Me)
        _currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Interfaces")
        If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")
        btnSave.Enabled = _currentUserPermission(_currentTableName).Edit
        If Not Page.IsPostBack Then
            Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
            Dim sql As String = "SELECT COUNT(*) " & _
                "FROM interface_types " & _
                "INNER JOIN interfaces ON interface_types.id = interfaces.interface_type_id " & _
                "WHERE (interface_types.deleted = 0) AND (interfaces.deleted = 0) AND "
            ddlItemType.Items.Add(New ListItem("Select type", ""))
            If Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaApplicator.TABLE_NAME}), "Applicators")(KaApplicator.TABLE_NAME).Edit Then
                Dim rdr As OleDbDataReader = Tm2Database.ExecuteReader(connection, sql & "((" & KaInterfaceTypes.FN_SHOW_APPLICATOR_INTERFACE & " = 1) " & _
                                                                       "OR (interfaces.id IN (SELECT interface_id " & _
                                                                       "FROM " & KaApplicatorInterfaceSettings.TABLE_NAME & " " & _
                                                                       "WHERE (deleted=0))))")
                If rdr.Read AndAlso rdr.Item(0) > 0 Then ddlItemType.Items.Add(New ListItem("Applicators", "Applicators"))
            End If
            If Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaBranch.TABLE_NAME}), "Branches")(KaBranch.TABLE_NAME).Edit Then
                Dim rdr As OleDbDataReader = Tm2Database.ExecuteReader(connection, sql & "((" & KaInterfaceTypes.FN_SHOW_BRANCH_INTERFACE & " = 1) " & _
                                                                       "OR (interfaces.id IN (SELECT interface_id " & _
                                                                       "FROM " & KaBranchInterfaceSettings.TABLE_NAME & " " & _
                                                                       "WHERE (deleted=0))))")
                If rdr.Read AndAlso rdr.Item(0) > 0 Then ddlItemType.Items.Add(New ListItem("Branches", "Branches"))
            End If
            If Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaProduct.TABLE_NAME}), "Products")(KaProduct.TABLE_NAME).Edit Then
                Dim rdr As OleDbDataReader = Tm2Database.ExecuteReader(connection, sql & "((" & KaInterfaceTypes.FN_SHOW_BULK_PRODUCT_INTERFACE & " = 1) " & _
                                                                       "OR (interfaces.id IN (SELECT interface_id " & _
                                                                       "FROM " & KaBulkProductInterfaceSettings.TABLE_NAME & " " & _
                                                                       "WHERE (deleted=0))))")
                If rdr.Read AndAlso rdr.Item(0) > 0 Then ddlItemType.Items.Add(New ListItem("Bulk products", "Bulk Products"))
            End If
            If Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaCarrier.TABLE_NAME}), "Carriers")(KaCarrier.TABLE_NAME).Edit Then
                Dim rdr As OleDbDataReader = Tm2Database.ExecuteReader(connection, sql & "((" & KaInterfaceTypes.FN_SHOW_CARRIER_INTERFACE & " = 1) " & _
                                                                       "OR (interfaces.id IN (SELECT interface_id " & _
                                                                       "FROM " & KaCarrierInterfaceSettings.TABLE_NAME & " " & _
                                                                       "WHERE (deleted=0))))")
                If rdr.Read AndAlso rdr.Item(0) > 0 Then ddlItemType.Items.Add(New ListItem("Carriers", "Carriers"))
            End If
            If Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaCustomerAccount.TABLE_NAME}), "Accounts")(KaCustomerAccount.TABLE_NAME).Edit Then
                ddlItemType.Items.Add(New ListItem("Customer account destinations", "Customer Account Destinations"))
                ddlItemType.Items.Add(New ListItem("Customer accounts", "Customer Accounts"))
            End If
            If Utilities.GetUserPagePermission(_currentUser,New List(Of String)({KaDriver.TABLE_NAME}),"Drivers")(KaDriver.TABLE_NAME).Edit Then
                Dim rdr As OleDbDataReader = Tm2Database.ExecuteReader(connection, sql & "((" & KaInterfaceTypes.FN_SHOW_DRIVER_INTERFACE & " = 1) " & _
                                                                       "OR (interfaces.id IN (SELECT interface_id " & _
                                                                       "FROM " & KaDriverInterfaceSettings.TABLE_NAME & " " & _
                                                                       "WHERE (deleted=0))))")
                If rdr.Read AndAlso rdr.Item(0) > 0 Then ddlItemType.Items.Add(New ListItem("Drivers", "Drivers"))
            End If
            If Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaLocation.TABLE_NAME}), "Facilities")(KaLocation.TABLE_NAME).Edit Then
                Dim rdr As OleDbDataReader = Tm2Database.ExecuteReader(connection, sql & "((" & KaInterfaceTypes.FN_SHOW_LOCATION_INTERFACE & " = 1) " & _
                                                                       "OR (interfaces.id IN (SELECT interface_id " & _
                                                                       "FROM " & KaLocationInterfaceSettings.TABLE_NAME & " " & _
                                                                       "WHERE (deleted=0))))")
                If rdr.Read AndAlso rdr.Item(0) > 0 Then ddlItemType.Items.Add(New ListItem("Facilities", "Facilities"))
            End If
            If Utilities.GetUserPagePermission(_currentUser,New List(Of String)({KaOwner.TABLE_NAME}),"Owners")(KaOwner.TABLE_NAME).Edit Then
                Dim rdr As OleDbDataReader = Tm2Database.ExecuteReader(connection, sql & "((" & KaInterfaceTypes.FN_SHOW_OWNER_INTERFACE & " = 1) " & _
                                                                       "OR (interfaces.id IN (SELECT interface_id " & _
                                                                       "FROM " & KaOwnerInterfaceSettings.TABLE_NAME & " " & _
                                                                       "WHERE (deleted=0))))")
                If rdr.Read AndAlso rdr.Item(0) > 0 Then ddlItemType.Items.Add(New ListItem("Owners", "Owners"))
            End If
            If Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaProduct.TABLE_NAME}), "Products")(KaProduct.TABLE_NAME).Edit Then ddlItemType.Items.Add(New ListItem("Products", "Products"))
            If Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaReceivingPurchaseOrder.TABLE_NAME}), "PurchaseOrders")(KaReceivingPurchaseOrder.TABLE_NAME).Edit Then ddlItemType.Items.Add(New ListItem("Suppliers", "Suppliers"))

            If Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaTank.TABLE_NAME}), "Tanks")(KaTank.TABLE_NAME).Read Then
                Dim rdr As OleDbDataReader = Tm2Database.ExecuteReader(connection, sql & "((" & KaInterfaceTypes.FN_SHOW_TANKS_INTERFACE & " = 1) " & _
                                                                      "OR (interfaces.id IN (SELECT interface_id " & _
                                                                      "FROM " & KaTankInterfaceSettings.TABLE_NAME & " " & _
                                                                      "WHERE (deleted=0))))")
                If rdr.Read AndAlso rdr.Item(0) > 0 Then ddlItemType.Items.Add(New ListItem("Tanks", "Tanks"))
            End If
            If Utilities.GetUserPagePermission(_currentUser,New List(Of String)({KaTransport.TABLE_NAME}),"Transports")(KaTransport.TABLE_NAME).Edit Then
                Dim rdr As OleDbDataReader = Tm2Database.ExecuteReader(connection, sql & "((" & KaInterfaceTypes.FN_SHOW_TRANSPORT_INTERFACE & " = 1) " & _
                                                                       "OR (interfaces.id IN (SELECT interface_id " & _
                                                                       "FROM " & KaTransportInterfaceSettings.TABLE_NAME & " " & _
                                                                       "WHERE (deleted=0))))")
                If rdr.Read AndAlso rdr.Item(0) > 0 Then ddlItemType.Items.Add(New ListItem("Transports", "Transports"))
                Dim rdr2 As OleDbDataReader = Tm2Database.ExecuteReader(connection, sql & "((" & KaInterfaceTypes.FN_SHOW_TRANSPORT_TYPE_INTERFACE & " = 1) " & _
                                                                       "OR (interfaces.id IN (SELECT interface_id " & _
                                                                       "FROM " & KaTransportTypeInterfaceSettings.TABLE_NAME & " " & _
                                                                       "WHERE (deleted=0))))")
                If rdr2.Read AndAlso rdr2.Item(0) > 0 Then ddlItemType.Items.Add(New ListItem("Transport types", "Transport Types"))
            End If
            If Utilities.GetUserPagePermission(_currentUser,New List(Of String)({KaUnit.TABLE_NAME}),"Units")(KaUnit.TABLE_NAME).Edit Then
                Dim rdr As OleDbDataReader = Tm2Database.ExecuteReader(connection, sql & "((" & KaInterfaceTypes.FN_SHOW_UNITS_INTERFACE & " = 1) " & _
                                                                       "OR (interfaces.id IN (SELECT interface_id " & _
                                                                       "FROM " & KaUnitInterfaceSettings.TABLE_NAME & " " & _
                                                                       "WHERE (deleted=0))))")
                If rdr.Read AndAlso rdr.Item(0) > 0 Then ddlItemType.Items.Add(New ListItem("Units", "Units"))
            End If
            Utilities.SetFocus(ddlItemType, Me) ' set focus to the first textbox on the page

            ddlItemType.SelectedIndex = 0
            ddlItemType_SelectedIndexChanged(ddlItemType, New EventArgs)
            'Else
            '  
        End If
        _currentItemType = ddlItemType.SelectedValue
        'PopulateItems()
        lblStatus.Text = ""
    End Sub

    Private Sub ddlItemType_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles ddlItemType.SelectedIndexChanged
        If ddlPageNumber.Items.Count > 0 Then
            ddlPageNumber.SelectedIndex = 0
        Else
            ddlPageNumber.SelectedIndex = -1
        End If
        Dim interface1SelectedId As String = ddlInterface1.SelectedValue
        Dim interface2SelectedId As String = ddlInterface2.SelectedValue
        PopulateInterfaceList(interface1SelectedId, interface2SelectedId)
        PopulateItems()
    End Sub

    Private Sub PopulateInterfaceList(ByVal interface1SelectedId As String, ByVal interface2SelectedId As String)
        ddlInterface1.Items.Clear()
        ddlInterface2.Items.Clear()
        ddlInterface1.Items.Add(New ListItem("Select an interface", Guid.Empty.ToString()))
        ddlInterface2.Items.Add(New ListItem("Select an interface", Guid.Empty.ToString()))

        Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
        Dim interfaceList As New ArrayList
        Select Case ddlItemType.SelectedValue.ToLower
            Case "Applicators".ToLower
                Dim getInterfaceRdr As OleDbDataReader = Tm2Database.ExecuteReader(connection, "SELECT interfaces.id " & _
                        "FROM interfaces " & _
                        "INNER JOIN interface_types ON interfaces.interface_type_id = interface_types.id " & _
                        "WHERE (interfaces.deleted = 0) " & _
                            "AND (interface_types.deleted = 0) " & _
                            "AND ((" & KaInterfaceTypes.FN_SHOW_APPLICATOR_INTERFACE & " = 1) " & _
                            "OR (interfaces.id IN (SELECT " & KaApplicatorInterfaceSettings.TABLE_NAME & ".interface_id " & _
                                                    "FROM " & KaApplicatorInterfaceSettings.TABLE_NAME & " " & _
                                                    "WHERE (deleted=0)))) " & _
                        "ORDER BY interfaces.name")
                Do While getInterfaceRdr.Read
                    interfaceList.Add(New KaInterface(connection, getInterfaceRdr.Item("id")))
                Loop
                getInterfaceRdr.Close()
            Case "Branches".ToLower
                Dim getInterfaceRdr As OleDbDataReader = Tm2Database.ExecuteReader(connection, "SELECT interfaces.id " & _
                        "FROM interfaces " & _
                        "INNER JOIN interface_types ON interfaces.interface_type_id = interface_types.id " & _
                        "WHERE (interfaces.deleted = 0) " & _
                            "AND (interface_types.deleted = 0) " & _
                            "AND ((" & KaInterfaceTypes.FN_SHOW_BRANCH_INTERFACE & " = 1) " & _
                            "OR (interfaces.id IN (SELECT " & KaBranchInterfaceSettings.TABLE_NAME & ".interface_id " & _
                                                    "FROM " & KaBranchInterfaceSettings.TABLE_NAME & " " & _
                                                    "WHERE (deleted=0)))) " & _
                        "ORDER BY interfaces.name")
                Do While getInterfaceRdr.Read
                    interfaceList.Add(New KaInterface(connection, getInterfaceRdr.Item("id")))
                Loop
                getInterfaceRdr.Close()
            Case "Bulk Products".ToLower
                Dim getInterfaceRdr As OleDbDataReader = Tm2Database.ExecuteReader(connection, "SELECT interfaces.id " & _
                        "FROM interfaces " & _
                        "INNER JOIN interface_types ON interfaces.interface_type_id = interface_types.id " & _
                        "WHERE (interfaces.deleted = 0) " & _
                            "AND (interface_types.deleted = 0) " & _
                            "AND ((" & KaInterfaceTypes.FN_SHOW_BULK_PRODUCT_INTERFACE & " = 1) " & _
                            "OR (interfaces.id IN (SELECT " & KaBulkProductInterfaceSettings.TABLE_NAME & ".interface_id " & _
                                                    "FROM " & KaBulkProductInterfaceSettings.TABLE_NAME & " " & _
                                                    "WHERE (deleted=0)))) " & _
                        "ORDER BY interfaces.name")
                Do While getInterfaceRdr.Read
                    interfaceList.Add(New KaInterface(connection, getInterfaceRdr.Item("id")))
                Loop
                getInterfaceRdr.Close()
            Case "Carriers"
                Dim getInterfaceRdr As OleDbDataReader = Tm2Database.ExecuteReader(connection, "SELECT interfaces.id " & _
                                "FROM interfaces " & _
                                "INNER JOIN interface_types ON interfaces.interface_type_id = interface_types.id " & _
                                "WHERE (interfaces.deleted = 0) " & _
                                    "AND (interface_types.deleted = 0) " & _
                                    "AND ((" & KaInterfaceTypes.FN_SHOW_CARRIER_INTERFACE & " = 1) " & _
                                    "OR (interfaces.id IN (SELECT " & KaCarrierInterfaceSettings.TABLE_NAME & ".interface_id " & _
                                                            "FROM " & KaCarrierInterfaceSettings.TABLE_NAME & " " & _
                                                            "WHERE (deleted=0)))) " & _
                                "ORDER BY interfaces.name")
                Do While getInterfaceRdr.Read
                    interfaceList.Add(New KaInterface(connection, getInterfaceRdr.Item("id")))
                Loop
                getInterfaceRdr.Close()
            Case "Drivers"
                Dim getInterfaceRdr As OleDbDataReader = Tm2Database.ExecuteReader(connection, "SELECT interfaces.id " & _
                            "FROM interfaces " & _
                            "INNER JOIN interface_types ON interfaces.interface_type_id = interface_types.id " & _
                            "WHERE (interfaces.deleted = 0) " & _
                                "AND (interface_types.deleted = 0) " & _
                                "AND ((" & KaInterfaceTypes.FN_SHOW_DRIVER_INTERFACE & " = 1) " & _
                                "OR (interfaces.id IN (SELECT " & KaDriverInterfaceSettings.TABLE_NAME & ".interface_id " & _
                                                        "FROM " & KaDriverInterfaceSettings.TABLE_NAME & " " & _
                                                        "WHERE (deleted=0)))) " & _
                            "ORDER BY interfaces.name")
                Do While getInterfaceRdr.Read
                    interfaceList.Add(New KaInterface(connection, getInterfaceRdr.Item("id")))
                Loop
                getInterfaceRdr.Close()
            Case "Facilities".ToLower
                Dim getInterfaceRdr As OleDbDataReader = Tm2Database.ExecuteReader(connection, "SELECT interfaces.id " & _
                            "FROM interfaces " & _
                            "INNER JOIN interface_types ON interfaces.interface_type_id = interface_types.id " & _
                            "WHERE (interfaces.deleted = 0) " & _
                                "AND (interface_types.deleted = 0) " & _
                                "AND ((" & KaInterfaceTypes.FN_SHOW_LOCATION_INTERFACE & " = 1) " & _
                                "OR (interfaces.id IN (SELECT " & KaLocationInterfaceSettings.TABLE_NAME & ".interface_id " & _
                                                        "FROM " & KaLocationInterfaceSettings.TABLE_NAME & " " & _
                                                        "WHERE (deleted=0)))) " & _
                            "ORDER BY interfaces.name")
                Do While getInterfaceRdr.Read
                    interfaceList.Add(New KaInterface(connection, getInterfaceRdr.Item("id")))
                Loop
                getInterfaceRdr.Close()
            Case "Owners".ToLower
                Dim getInterfaceRdr As OleDbDataReader = Tm2Database.ExecuteReader(connection, "SELECT interfaces.id " & _
                            "FROM interfaces " & _
                            "INNER JOIN interface_types ON interfaces.interface_type_id = interface_types.id " & _
                            "WHERE (interfaces.deleted = 0) " & _
                                "AND (interface_types.deleted = 0) " & _
                                "AND ((" & KaInterfaceTypes.FN_SHOW_OWNER_INTERFACE & " = 1) " & _
                                "OR (interfaces.id IN (SELECT " & KaOwnerInterfaceSettings.TABLE_NAME & ".interface_id " & _
                                                        "FROM " & KaOwnerInterfaceSettings.TABLE_NAME & " " & _
                                                        "WHERE (deleted=0)))) " & _
                            "ORDER BY interfaces.name")
                Do While getInterfaceRdr.Read
                    interfaceList.Add(New KaInterface(connection, getInterfaceRdr.Item("id")))
                Loop
                getInterfaceRdr.Close()
            Case "Tanks"
                Dim getInterfaceRdr As OleDbDataReader = Tm2Database.ExecuteReader(connection, "SELECT interfaces.id " & _
                            "FROM interfaces " & _
                            "INNER JOIN interface_types ON interfaces.interface_type_id = interface_types.id " & _
                            "WHERE (interfaces.deleted = 0) " & _
                                "AND (interface_types.deleted = 0) " & _
                                "AND ((" & KaInterfaceTypes.FN_SHOW_TANKS_INTERFACE & " = 1) " & _
                                "OR (interfaces.id IN (SELECT " & KaTankInterfaceSettings.TABLE_NAME & ".interface_id " & _
                                                        "FROM " & KaTankInterfaceSettings.TABLE_NAME & " " & _
                                                        "WHERE (deleted=0)))) " & _
                            "ORDER BY interfaces.name")
                Do While getInterfaceRdr.Read
                    interfaceList.Add(New KaInterface(connection, getInterfaceRdr.Item("id")))
                Loop
                getInterfaceRdr.Close()
            Case "Transports"
                Dim getInterfaceRdr As OleDbDataReader = Tm2Database.ExecuteReader(connection, "SELECT interfaces.id " & _
                               "FROM interfaces " & _
                               "INNER JOIN interface_types ON interfaces.interface_type_id = interface_types.id " & _
                               "WHERE (interfaces.deleted = 0) " & _
                                   "AND (interface_types.deleted = 0) " & _
                                   "AND ((" & KaInterfaceTypes.FN_SHOW_TRANSPORT_INTERFACE & " = 1) " & _
                                   "OR (interfaces.id IN (SELECT " & KaTransportInterfaceSettings.TABLE_NAME & ".interface_id " & _
                                                           "FROM " & KaTransportInterfaceSettings.TABLE_NAME & " " & _
                                                           "WHERE (deleted=0)))) " & _
                               "ORDER BY interfaces.name")
                Do While getInterfaceRdr.Read
                    interfaceList.Add(New KaInterface(connection, getInterfaceRdr.Item("id")))
                Loop
                getInterfaceRdr.Close()
            Case "Transport Types"
                Dim getInterfaceRdr As OleDbDataReader = Tm2Database.ExecuteReader(connection, "SELECT interfaces.id " & _
                           "FROM interfaces " & _
                           "INNER JOIN interface_types ON interfaces.interface_type_id = interface_types.id " & _
                           "WHERE (interfaces.deleted = 0) " & _
                               "AND (interface_types.deleted = 0) " & _
                               "AND ((" & KaInterfaceTypes.FN_SHOW_TRANSPORT_TYPE_INTERFACE & " = 1) " & _
                               "OR (interfaces.id IN (SELECT " & KaTransportTypeInterfaceSettings.TABLE_NAME & ".interface_id " & _
                                                       "FROM " & KaTransportTypeInterfaceSettings.TABLE_NAME & " " & _
                                                       "WHERE (deleted=0)))) " & _
                           "ORDER BY interfaces.name")
                Do While getInterfaceRdr.Read
                    interfaceList.Add(New KaInterface(connection, getInterfaceRdr.Item("id")))
                Loop
                getInterfaceRdr.Close()
            Case "Units"
                Dim getInterfaceRdr As OleDbDataReader = Tm2Database.ExecuteReader(connection, "SELECT interfaces.id " & _
                                    "FROM interfaces " & _
                                    "INNER JOIN interface_types ON interfaces.interface_type_id = interface_types.id " & _
                                    "WHERE (interfaces.deleted = 0) " & _
                                        "AND (interface_types.deleted = 0) " & _
                                        "AND ((" & KaInterfaceTypes.FN_SHOW_UNITS_INTERFACE & " = 1) " & _
                                        "OR (interfaces.id IN (SELECT " & KaUnitInterfaceSettings.TABLE_NAME & ".interface_id " & _
                                                                "FROM " & KaUnitInterfaceSettings.TABLE_NAME & " " & _
                                                                "WHERE (deleted=0)))) " & _
                                    "ORDER BY interfaces.name")
                Do While getInterfaceRdr.Read
                    interfaceList.Add(New KaInterface(connection, getInterfaceRdr.Item("id")))
                Loop
                getInterfaceRdr.Close()
            Case Else
                interfaceList = KaInterface.GetAll(connection, "deleted=0", "name asc")
        End Select

        For Each r As KaInterface In interfaceList
            Dim interfaceValid As Boolean = True
            Try
                Dim interfaceTypeInfo As New KaInterfaceTypes(connection, r.InterfaceTypeId)
                Select Case ddlItemType.SelectedValue.ToLower
                    Case "Applicators".ToLower
                        interfaceValid = interfaceTypeInfo.ShowApplicatorInterface
                    Case "Branches".ToLower
                        interfaceValid = interfaceTypeInfo.ShowBranchInterface
                    Case "Bulk Products".ToLower
                        interfaceValid = interfaceTypeInfo.ShowBulkProductInterface
                    Case "Carriers".ToLower
                        interfaceValid = interfaceTypeInfo.ShowCarrierInterface
                    Case "Drivers".ToLower
                        interfaceValid = interfaceTypeInfo.ShowDriversInterface
                    Case "Facilities".ToLower
                        interfaceValid = interfaceTypeInfo.ShowLocationInterface
                    Case "Owners".ToLower
                        interfaceValid = interfaceTypeInfo.ShowOwnerInterface
                    Case "Tanks".ToLower
                        interfaceValid = interfaceTypeInfo.ShowTanksInterface
                    Case "Transports".ToLower
                        interfaceValid = interfaceTypeInfo.ShowTransportInterface
                    Case "Transport Types".ToLower
                        interfaceValid = interfaceTypeInfo.ShowTransportTypeInterface
                    Case "Units".ToLower
                        interfaceValid = interfaceTypeInfo.ShowUnitsInterface
                    Case Else ' Default to showing it
                        interfaceValid = True
                End Select
            Catch ex As RecordNotFoundException
                interfaceValid = True
            End Try

            If interfaceValid Then
                ddlInterface1.Items.Add(New ListItem(r.Name, r.Id.ToString()))
                ddlInterface2.Items.Add(New ListItem(r.Name, r.Id.ToString()))
            End If
        Next

        With ddlInterface1
            Try
                .SelectedValue = interface1SelectedId
            Catch ex As ArgumentOutOfRangeException
                If .Items.Count = 1 Then
                    .Items.Clear()
                    .Items.Add(New ListItem("No interfaces available", Guid.Empty.ToString))
                End If
                .SelectedIndex = 0
            End Try
        End With
        With ddlInterface2
            Try
                .SelectedValue = interface2SelectedId
            Catch ex As ArgumentOutOfRangeException
                If .Items.Count = 1 Then
                    .Items.Clear()
                    .Items.Add(New ListItem("No interfaces available", Guid.Empty.ToString))
                End If
                If .Items.Count = 2 Then
                    .SelectedIndex = 1
                Else
                    .SelectedIndex = 0
                End If
            End Try
        End With
    End Sub

#Region " Populate Items "
    Private Sub PopulateItems()
        Do While tblInterfaceSetting.Rows.Count > 3
            tblInterfaceSetting.Rows.RemoveAt(2)
        Loop

        Select Case _currentItemType
            Case "Applicators"
                lblItemType.Text = "Applicator"
                PopulateApplicatorList()
            Case "Branches"
                lblItemType.Text = "Branch"
                PopulateBranchList()
            Case "Bulk Products"
                lblItemType.Text = "Bulk Product"
                PopulateBulkProductList()
            Case "Carriers"
                lblItemType.Text = "Carriers"
                PopulateCarriersList()
            Case "Customer Account Destinations"
                lblItemType.Text = "Customer Account Destination"
                PopulateCustomerAccountDestinationList()
            Case "Customer Accounts"
                lblItemType.Text = "Customer Account"
                PopulateCustomerAccountList()
            Case "Drivers"
                lblItemType.Text = "Drivers"
                PopulateDriversList()
            Case "Facilities"
                lblItemType.Text = "Facility"
                PopulateFacilityList()
            Case "Owners"
                lblItemType.Text = "Owner"
                PopulateOwnerList()
            Case "Products"
                lblItemType.Text = "Product"
                PopulateProductList()
            Case "Suppliers"
                lblItemType.Text = "Supplier"
                PopulateSupplierAccountList()
            Case "Tanks"
                lblItemType.Text = "Tanks"
                PopulateTanksList()
            Case "Transports"
                lblItemType.Text = "Transports"
                PopulateTransportsList()
            Case "Transport Types"
                lblItemType.Text = "Transport Types"
                PopulateTransportTypesList()
            Case "Units"
                lblItemType.Text = "Units"
                PopulateUnitsList()
        End Select
        ddlInterface1_SelectedIndexChanged(ddlInterface1, New EventArgs)
        ddlInterface2_SelectedIndexChanged(ddlInterface2, New EventArgs)

        SetEnabledStatusForColumns()
        pnlPageNumbers.Visible = (ddlPageNumber.Items.Count > 1)
        pnlItemsToDisplay.Visible = (ddlItemType.SelectedIndex > 0)
        divInterfaceSettings.Visible = (ddlItemType.SelectedIndex > 0)
    End Sub

    Private Sub PopulateApplicatorList()
        Dim applicatorTable As New DataTable
        Dim applicatorDa As New OleDbDataAdapter("SELECT id, name FROM " & KaApplicator.TABLE_NAME & " WHERE deleted=0 ORDER BY name", GetUserConnection(_currentUser.Id))
        If Tm2Database.CommandTimeout > 0 Then applicatorDa.SelectCommand.CommandTimeout = Tm2Database.CommandTimeout
        applicatorDa.Fill(applicatorTable)
        AddTableItemsToPage(applicatorTable)
    End Sub

    Private Sub PopulateBranchList()
        Dim branchTable As New DataTable
        Dim branchDa As New OleDbDataAdapter("SELECT id, name FROM " & KaBranch.TABLE_NAME & " WHERE deleted=0 ORDER BY name", GetUserConnection(_currentUser.Id))
        If Tm2Database.CommandTimeout > 0 Then branchDa.SelectCommand.CommandTimeout = Tm2Database.CommandTimeout
        branchDa.Fill(branchTable)
        AddTableItemsToPage(branchTable)
    End Sub

    Private Sub PopulateBulkProductList()
        Dim bulkProductTable As New DataTable
        Dim bulkProductDa As New OleDbDataAdapter("SELECT DISTINCT id, name " & _
                                                  "FROM bulk_products " & _
                                                  "WHERE (deleted = 0) " & _
                                                  "AND (NOT (id IN (SELECT bulk_product_id FROM bulk_product_panel_settings WHERE (deleted = 0) AND (product_number >= 80) AND (product_number < 99))))" & _
                                                  IIf(_currentUser.OwnerId = Guid.Empty, "", " AND (owner_id=" & Q(_currentUser.OwnerId) & " OR owner_id=" & Q(Guid.Empty) & ")") & " ORDER BY name", GetUserConnection(_currentUser.Id))
        If Tm2Database.CommandTimeout > 0 Then bulkProductDa.SelectCommand.CommandTimeout = Tm2Database.CommandTimeout
        bulkProductDa.Fill(bulkProductTable)
        AddTableItemsToPage(bulkProductTable)
    End Sub

    Private Sub PopulateCustomerAccountList()
        Dim customerAccountTable As New DataTable
        Dim customerAccountDa As New OleDbDataAdapter("SELECT id, name FROM " & KaCustomerAccount.TABLE_NAME & " WHERE deleted=0 AND is_supplier=0" & IIf(_currentUser.OwnerId = Guid.Empty, "", " AND (owner_id=" & Q(_currentUser.OwnerId) & " OR owner_id=" & Q(Guid.Empty) & ")") & " ORDER BY name", GetUserConnection(_currentUser.Id))
        If Tm2Database.CommandTimeout > 0 Then customerAccountDa.SelectCommand.CommandTimeout = Tm2Database.CommandTimeout
        customerAccountDa.Fill(customerAccountTable)
        AddTableItemsToPage(customerAccountTable)
    End Sub

    Private Sub PopulateCustomerAccountDestinationList()
        Dim customerAccountDestinationTable As New DataTable
        Dim customerAccountDestDa As New OleDbDataAdapter("SELECT id, 'All Customers (' + name + ')' + CASE WHEN cross_reference > '' THEN ' {' + cross_reference + '}' ELSE '' END AS name, 1 " & _
                                           "FROM customer_account_locations " & _
                                           "WHERE (deleted = 0) AND (customer_account_id = " & Q(Guid.Empty) & ") " & _
                                           "UNION " & _
                                           "SELECT customer_account_locations.id, customer_accounts.name + ' (' + customer_account_locations.name + ')' + CASE WHEN customer_account_locations.cross_reference > '' THEN ' {' + customer_account_locations.cross_reference + '}' ELSE '' END AS name, 2  " & _
                                           "FROM customer_accounts " & _
                                           "INNER JOIN customer_account_locations ON customer_accounts.id = customer_account_locations.customer_account_id " & _
                                           "WHERE (customer_account_locations.deleted = 0) AND (customer_accounts.deleted = 0) " & _
                                           "ORDER BY 3, name", GetUserConnection(_currentUser.Id))

        If Tm2Database.CommandTimeout > 0 Then customerAccountDestDa.SelectCommand.CommandTimeout = Tm2Database.CommandTimeout
        customerAccountDestDa.Fill(customerAccountDestinationTable)
        AddTableItemsToPage(customerAccountDestinationTable)
    End Sub

    Private Sub PopulateFacilityList()
        Dim locationTable As New DataTable
        Dim locationDa As New OleDbDataAdapter("SELECT id, name FROM " & KaLocation.TABLE_NAME & " WHERE deleted=0 ORDER BY name", GetUserConnection(_currentUser.Id))
        If Tm2Database.CommandTimeout > 0 Then locationDa.SelectCommand.CommandTimeout = Tm2Database.CommandTimeout
        locationDa.Fill(locationTable)
        AddTableItemsToPage(locationTable)
    End Sub

    Private Sub PopulateOwnerList()
        Dim ownerTable As New DataTable
        Dim ownerDa As New OleDbDataAdapter("SELECT id, name FROM " & KaOwner.TABLE_NAME & " WHERE deleted=0" & IIf(_currentUser.OwnerId = Guid.Empty, "", " AND (owner_id=" & Q(_currentUser.OwnerId) & ")") & " ORDER BY name", GetUserConnection(_currentUser.Id))
        If Tm2Database.CommandTimeout > 0 Then ownerDa.SelectCommand.CommandTimeout = Tm2Database.CommandTimeout
        ownerDa.Fill(ownerTable)
        AddTableItemsToPage(ownerTable)
    End Sub

    Private Sub PopulateProductList()
        Dim productTable As New DataTable
        Dim productDa As New OleDbDataAdapter("SELECT id, name FROM " & KaProduct.TABLE_NAME & " WHERE deleted=0" & IIf(_currentUser.OwnerId = Guid.Empty, "", " AND (owner_id=" & Q(_currentUser.OwnerId) & " OR owner_id=" & Q(Guid.Empty) & ")") & " ORDER BY name", GetUserConnection(_currentUser.Id))
        If Tm2Database.CommandTimeout > 0 Then productDa.SelectCommand.CommandTimeout = Tm2Database.CommandTimeout
        productDa.Fill(productTable)
        AddTableItemsToPage(productTable)
    End Sub

    Private Sub PopulateSupplierAccountList()
        Dim supplierAccountTable As New DataTable
        Dim supplierAccountDa As New OleDbDataAdapter("SELECT id, name FROM " & KaSupplierAccount.TABLE_NAME & " WHERE deleted=0 AND is_supplier=1" & IIf(_currentUser.OwnerId = Guid.Empty, "", " AND (owner_id=" & Q(_currentUser.OwnerId) & " OR owner_id=" & Q(Guid.Empty) & ")") & " ORDER BY name", GetUserConnection(_currentUser.Id))
        If Tm2Database.CommandTimeout > 0 Then supplierAccountDa.SelectCommand.CommandTimeout = Tm2Database.CommandTimeout
        supplierAccountDa.Fill(supplierAccountTable)
        AddTableItemsToPage(supplierAccountTable)
    End Sub

    Private Sub PopulateTanksList()
        Dim tankTable As New DataTable
        Dim tankDa As New OleDbDataAdapter("SELECT id, name FROM " & KaTank.TABLE_NAME & " WHERE deleted=0 ORDER BY name", GetUserConnection(_currentUser.Id))
        If Tm2Database.CommandTimeout > 0 Then tankDa.SelectCommand.CommandTimeout = Tm2Database.CommandTimeout
        tankDa.Fill(tankTable)
        AddTableItemsToPage(tankTable)
    End Sub

    Private Sub PopulateTransportTypesList()
        Dim transportTypeTable As New DataTable
        Dim transportTypeDa As New OleDbDataAdapter("SELECT id, name FROM " & KaTransportTypes.TABLE_NAME & " WHERE deleted=0 ORDER BY name", GetUserConnection(_currentUser.Id))
        If Tm2Database.CommandTimeout > 0 Then transportTypeDa.SelectCommand.CommandTimeout = Tm2Database.CommandTimeout
        transportTypeDa.Fill(transportTypeTable)
        AddTableItemsToPage(transportTypeTable)
    End Sub

    Private Sub PopulateUnitsList()
        Dim unitTable As New DataTable
        Dim unitDa As New OleDbDataAdapter("SELECT id, name FROM " & KaUnit.TABLE_NAME & " WHERE deleted=0 ORDER BY name", GetUserConnection(_currentUser.Id))
        If Tm2Database.CommandTimeout > 0 Then unitDa.SelectCommand.CommandTimeout = Tm2Database.CommandTimeout
        unitDa.Fill(unitTable)
        AddTableItemsToPage(unitTable)
    End Sub

    Private Sub PopulateDriversList()
        Dim driverTable As New DataTable
        Dim driverDa As New OleDbDataAdapter("SELECT id, name FROM " & KaDriver.TABLE_NAME & " WHERE deleted=0 ORDER BY name", GetUserConnection(_currentUser.Id))
        If Tm2Database.CommandTimeout > 0 Then driverDa.SelectCommand.CommandTimeout = Tm2Database.CommandTimeout
        driverDa.Fill(driverTable)
        AddTableItemsToPage(driverTable)
    End Sub

    Private Sub PopulateCarriersList()
        Dim carrierTable As New DataTable
        Dim carrierDa As New OleDbDataAdapter("SELECT id, name FROM " & KaCarrier.TABLE_NAME & " WHERE deleted=0 ORDER BY name", GetUserConnection(_currentUser.Id))
        If Tm2Database.CommandTimeout > 0 Then carrierDa.SelectCommand.CommandTimeout = Tm2Database.CommandTimeout
        carrierDa.Fill(carrierTable)
        AddTableItemsToPage(carrierTable)
    End Sub

    Private Sub PopulateTransportsList()
        Dim transportTable As New DataTable
        Dim transportDa As New OleDbDataAdapter("SELECT id, name FROM " & KaTransport.TABLE_NAME & " WHERE deleted=0 ORDER BY name", GetUserConnection(_currentUser.Id))
        If Tm2Database.CommandTimeout > 0 Then transportDa.SelectCommand.CommandTimeout = Tm2Database.CommandTimeout
        transportDa.Fill(transportTable)
        AddTableItemsToPage(transportTable)
    End Sub

    Private Sub AddTableItemsToPage(ByVal itemTable As DataTable)
        Dim interfaceType1 As KaInterfaceTypes
        Try
            Dim interfaceId1 As Guid = Guid.Empty
            Guid.TryParse(ddlInterface1.SelectedValue, interfaceId1)
            interfaceType1 = New KaInterfaceTypes(Tm2Database.Connection, New KaInterface(Tm2Database.Connection, interfaceId1).InterfaceTypeId)
        Catch ex As RecordNotFoundException
            interfaceType1 = New KaInterfaceTypes()
        End Try
        Dim interfaceType2 As KaInterfaceTypes
        Try
            Dim interfaceId2 As Guid = Guid.Empty
            Guid.TryParse(ddlInterface2.SelectedValue, interfaceId2)
            interfaceType2 = New KaInterfaceTypes(Tm2Database.Connection, New KaInterface(Tm2Database.Connection, interfaceId2).InterfaceTypeId)
        Catch ex As RecordNotFoundException
            interfaceType2 = New KaInterfaceTypes()
        End Try

        Dim numberOfItemsPerPage As Integer = Integer.Parse(ddlItemsPerPage.SelectedValue)
        Dim maxNumberOfPages As Integer = Math.Ceiling(itemTable.Rows.Count / numberOfItemsPerPage)
        Dim visiblePageNumber As Integer = Math.Max(1, Math.Min(ddlPageNumber.SelectedIndex + 1, maxNumberOfPages))

        ddlPageNumber.Items.Clear()
        For currentPageNumber As Integer = 1 To maxNumberOfPages
            Dim startingExportIndex As Integer = Math.Max(0, (currentPageNumber * numberOfItemsPerPage) - numberOfItemsPerPage)
            Dim endingExportIndex As Integer = Math.Min(itemTable.Rows.Count, startingExportIndex + numberOfItemsPerPage) - 1
            ddlPageNumber.Items.Add(itemTable.Rows(startingExportIndex).Item("Name") & " - " & itemTable.Rows(endingExportIndex).Item("Name"))
            If currentPageNumber = visiblePageNumber Then
                For rowNumber As Integer = startingExportIndex To endingExportIndex
                    Dim exportedRow As DataRow = itemTable.Rows(rowNumber)
                    AddInterfaceRow(New ItemCrossReference(exportedRow.Item("id").ToString, exportedRow.Item("Name"), New List(Of ItemCrossReference.CrossReference), New List(Of ItemCrossReference.CrossReference)), interfaceType1, interfaceType2)
                Next
            End If
        Next

        If ddlPageNumber.Items.Count >= visiblePageNumber Then
            ddlPageNumber.SelectedIndex = visiblePageNumber - 1
        ElseIf ddlPageNumber.Items.Count > 0 Then
            ddlPageNumber.SelectedIndex = 0
        End If
        btnPreviousPage.Enabled = ddlPageNumber.Items.Count > 1 AndAlso ddlPageNumber.SelectedIndex > 0
        btnNextPage.Enabled = ddlPageNumber.Items.Count > 1 AndAlso ddlPageNumber.SelectedIndex < ddlPageNumber.Items.Count - 1
    End Sub
#End Region

    Private Sub AddInterfaceRow(ByVal interfaceSetting As ItemCrossReference, ByVal interfaceType1 As KaInterfaceTypes, ByVal interfaceType2 As KaInterfaceTypes)
        Dim newTableRow As New HtmlTableRow
        newTableRow.ID = "itemRow" & tblInterfaceSetting.Rows.Count - 2
        Dim itemNameCell As New HtmlTableCell
        With itemNameCell
            .ID = newTableRow.ID & "Name"
            .InnerText = interfaceSetting.ItemName
            .EnableViewState = True
            .Attributes("Style") = "width: auto;"
        End With
        newTableRow.Cells.Add(itemNameCell)

        Dim itemIdCell As New HtmlTableCell
        With itemIdCell
            .InnerText = interfaceSetting.ItemId.ToString
            .Visible = False
            .ID = newTableRow.ID & "Id"
            .EnableViewState = True
        End With
        newTableRow.Cells.Add(itemIdCell)

        Dim interface1DisplayCell As New HtmlTableCell
        With interface1DisplayCell
            .ID = newTableRow.ID & "Interface1Value"
            .EnableViewState = True
            .Attributes("Style") = "width: auto;"
        End With
        newTableRow.Cells.Add(interface1DisplayCell)

        Dim itemCopyCell As New HtmlTableCell
        Dim itemCopyButton As New Button
        With itemCopyButton
            .ID = newTableRow.ID & "CopyButton"
            .Text = "-- Copy ->"
            .EnableViewState = True
            '.Width = 120
            '.Attributes("width") = "120px"
            .Attributes("Style") = "width: auto;"
            AddHandler .Click, AddressOf itemCopyButton_Click
        End With
        itemCopyCell.Controls.Add(itemCopyButton)
        newTableRow.Cells.Add(itemCopyCell)

        Dim interface2ValueCell As New HtmlTableCell
        With interface2ValueCell
            .ID = newTableRow.ID & "Interface2Value"
            .EnableViewState = True
            .Attributes("Style") = "width: auto;"
        End With
        newTableRow.Cells.Add(interface2ValueCell)

        'Add a New Button
        Dim newCrossReferenceRowButton As New Button
        With newCrossReferenceRowButton
            .Text = "New"
            .ID = "btn" & newTableRow.ID & "NewCrossReference"
            ' .Width = 50
            .Attributes("Style") = "width: auto; valign: bottom; float: right;"
            AddHandler .Click, AddressOf itemNewCrossRefButton_Click
        End With
        Dim newCrossReferenceCell As New HtmlTableCell
        With newCrossReferenceCell
            .Controls.Add(newCrossReferenceRowButton)
        End With
        newTableRow.Cells.Add(newCrossReferenceCell)

        tblInterfaceSetting.Rows.Insert(tblInterfaceSetting.Rows.Count - 1, newTableRow)

        AddCrossReferencesToInterface1(newTableRow, interfaceSetting.Interface1, interfaceType1)
        AddCrossReferencesToInterface2(newTableRow, interfaceSetting.Interface2, interfaceType2)
    End Sub

    Private Sub ddlInterface1_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles ddlInterface1.SelectedIndexChanged
        _selectedInterface1Value = Guid.Parse(ddlInterface1.SelectedValue)
        PopulateInterfaceListValues(1)
        SetEnabledStatusForColumns()
    End Sub

    Private Sub ddlInterface2_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles ddlInterface2.SelectedIndexChanged
        _selectedInterface2Value = Guid.Parse(ddlInterface2.SelectedValue)
        PopulateInterfaceListValues(2)
        SetEnabledStatusForColumns()
    End Sub

    Private Sub PopulateInterfaceListValues(ByVal interfaceNumber As Integer)
        Dim guidEmptyString As String = Guid.Empty.ToString()
        Dim interfaceType1 As KaInterfaceTypes
        Try
            Dim interfaceId1 As Guid = Guid.Empty
            Guid.TryParse(ddlInterface1.SelectedValue, interfaceId1)
            interfaceType1 = New KaInterfaceTypes(Tm2Database.Connection, New KaInterface(Tm2Database.Connection, interfaceId1).InterfaceTypeId)
        Catch ex As RecordNotFoundException
            interfaceType1 = New KaInterfaceTypes()
        End Try
        Dim interfaceType2 As KaInterfaceTypes
        Try
            Dim interfaceId2 As Guid = Guid.Empty
            Guid.TryParse(ddlInterface2.SelectedValue, interfaceId2)
            interfaceType2 = New KaInterfaceTypes(Tm2Database.Connection, New KaInterface(Tm2Database.Connection, interfaceId2).InterfaceTypeId)
        Catch ex As RecordNotFoundException
            interfaceType2 = New KaInterfaceTypes()
        End Try

        For rowCounter As Integer = 2 To tblInterfaceSetting.Rows.Count - 2
            Dim currentItemRow As HtmlTableRow = tblInterfaceSetting.Rows(rowCounter)

            Dim itemId As Guid = Guid.Empty
            Dim interfaceId As Guid = Guid.Empty
            Dim selectedInterface As String = guidEmptyString
            If interfaceNumber = 1 Then
                selectedInterface = ddlInterface1.SelectedValue
            ElseIf interfaceNumber = 2 Then
                selectedInterface = ddlInterface2.SelectedValue
            End If
            Dim interfaceList As New List(Of ItemCrossReference.CrossReference)

            If Guid.TryParse(CType(currentItemRow.FindControl(currentItemRow.ID & "Id"), HtmlTableCell).InnerText, itemId) AndAlso _
                Guid.TryParse(selectedInterface, interfaceId) Then

                Try
                    Select Case _currentItemType.ToLower
                        Case "Applicators".ToLower
                            Dim crossReferenceList As ArrayList = KaApplicatorInterfaceSettings.GetAll(GetUserConnection(_currentUser.Id), KaApplicatorInterfaceSettings.FN_DELETED & " = 0 AND " & KaApplicatorInterfaceSettings.FN_APPLICATOR_ID & " = " & Q(itemId) & " AND " & KaApplicatorInterfaceSettings.FN_INTERFACE_ID & "=" & Q(interfaceId) & " AND " & KaApplicatorInterfaceSettings.FN_INTERFACE_ID & "<>" & Q(Guid.Empty), "")
                            For Each crossReference As KaApplicatorInterfaceSettings In crossReferenceList
                                interfaceList.Add(New ItemCrossReference.CrossReference(crossReference.Id.ToString, crossReference.CrossReference, guidEmptyString, guidEmptyString, crossReference.DefaultSetting, 0, crossReference.ExportOnly, guidEmptyString))
                            Next
                        Case "Branches".ToLower
                            Dim crossReferenceList As ArrayList = KaBranchInterfaceSettings.GetAll(GetUserConnection(_currentUser.Id), "deleted = 0 and branch_id = " & Q(itemId) & " AND interface_id=" & Q(interfaceId) & " AND interface_id<>" & Q(Guid.Empty), "")
                            For Each crossReference As KaBranchInterfaceSettings In crossReferenceList
                                interfaceList.Add(New ItemCrossReference.CrossReference(crossReference.Id.ToString, crossReference.CrossReference, guidEmptyString, guidEmptyString, crossReference.DefaultSetting, 0, crossReference.ExportOnly, guidEmptyString))
                            Next
                        Case "Bulk Products".ToLower
                            Dim crossReferenceList As ArrayList = KaBulkProductInterfaceSettings.GetAll(GetUserConnection(_currentUser.Id), "deleted = 0 and bulk_product_id = " & Q(itemId) & " AND interface_id=" & Q(interfaceId) & " AND interface_id<>" & Q(Guid.Empty), "")
                            For Each crossReference As KaBulkProductInterfaceSettings In crossReferenceList
                                interfaceList.Add(New ItemCrossReference.CrossReference(crossReference.Id.ToString, crossReference.CrossReference, crossReference.UnitId.ToString, guidEmptyString, crossReference.DefaultSetting, 0, crossReference.ExportOnly, guidEmptyString))
                            Next
                        Case "Carriers".ToLower
                            Dim crossReferenceList As ArrayList = KaCarrierInterfaceSettings.GetAll(GetUserConnection(_currentUser.Id), "deleted = 0 and " & KaCarrierInterfaceSettings.FN_CARRIER_ID & " = " & Q(itemId) & " AND interface_id=" & Q(interfaceId) & " AND interface_id<>" & Q(Guid.Empty), "")
                            For Each crossReference As KaCarrierInterfaceSettings In crossReferenceList
                                interfaceList.Add(New ItemCrossReference.CrossReference(crossReference.Id.ToString, crossReference.CrossReference, guidEmptyString, guidEmptyString, crossReference.DefaultSetting, 0, crossReference.ExportOnly, guidEmptyString))
                            Next
                        Case "Customer Account Destinations".ToLower
                            Dim crossReferenceList As ArrayList = KaCustomerAccountLocationInterfaceSettings.GetAll(GetUserConnection(_currentUser.Id), "deleted = 0 and customer_account_location_id = " & Q(itemId) & " AND interface_id=" & Q(interfaceId) & " AND interface_id<>" & Q(Guid.Empty), "")
                            For Each crossReference As KaCustomerAccountLocationInterfaceSettings In crossReferenceList
                                interfaceList.Add(New ItemCrossReference.CrossReference(crossReference.Id.ToString, crossReference.CrossReference, guidEmptyString, guidEmptyString, crossReference.DefaultSetting, 0, crossReference.ExportOnly, guidEmptyString))
                            Next
                        Case "Customer Accounts".ToLower
                            Dim crossReferenceList As ArrayList = KaCustomerAccountInterfaceSettings.GetAll(GetUserConnection(_currentUser.Id), "deleted = 0 and customer_account_id = " & Q(itemId) & " AND interface_id=" & Q(interfaceId) & " AND interface_id<>" & Q(Guid.Empty), "")
                            For Each crossReference As KaCustomerAccountInterfaceSettings In crossReferenceList
                                interfaceList.Add(New ItemCrossReference.CrossReference(crossReference.Id.ToString, crossReference.CrossReference, guidEmptyString, guidEmptyString, crossReference.DefaultSetting, 0, crossReference.ExportOnly, guidEmptyString))
                            Next
                        Case "Drivers".ToLower
                            Dim crossReferenceList As ArrayList = KaDriverInterfaceSettings.GetAll(GetUserConnection(_currentUser.Id), "deleted = 0 and " & KaDriverInterfaceSettings.FN_DRIVER_ID & " = " & Q(itemId) & " AND interface_id=" & Q(interfaceId) & " AND interface_id<>" & Q(Guid.Empty), "")
                            For Each crossReference As KaDriverInterfaceSettings In crossReferenceList
                                interfaceList.Add(New ItemCrossReference.CrossReference(crossReference.Id.ToString, crossReference.CrossReference, guidEmptyString, guidEmptyString, crossReference.DefaultSetting, 0, crossReference.ExportOnly, guidEmptyString))
                            Next
                        Case "Facilities".ToLower
                            Dim crossReferenceList As ArrayList = KaLocationInterfaceSettings.GetAll(GetUserConnection(_currentUser.Id), KaLocationInterfaceSettings.FN_DELETED & " = 0 AND " & KaLocationInterfaceSettings.FN_LOCATION_ID & " = " & Q(itemId) & " AND " & KaLocationInterfaceSettings.FN_INTERFACE_ID & "=" & Q(interfaceId) & " AND " & KaLocationInterfaceSettings.FN_INTERFACE_ID & "<>" & Q(Guid.Empty), "")
                            For Each crossReference As KaLocationInterfaceSettings In crossReferenceList
                                interfaceList.Add(New ItemCrossReference.CrossReference(crossReference.Id.ToString, crossReference.CrossReference, guidEmptyString, guidEmptyString, crossReference.DefaultSetting, 0, crossReference.ExportOnly, guidEmptyString))
                            Next
                        Case "Owners".ToLower
                            Dim crossReferenceList As ArrayList = KaOwnerInterfaceSettings.GetAll(GetUserConnection(_currentUser.Id), "deleted = 0 and owner_id = " & Q(itemId) & " AND interface_id=" & Q(interfaceId) & " AND interface_id<>" & Q(Guid.Empty), "")
                            For Each crossReference As KaOwnerInterfaceSettings In crossReferenceList
                                interfaceList.Add(New ItemCrossReference.CrossReference(crossReference.Id.ToString, crossReference.CrossReference, guidEmptyString, guidEmptyString, crossReference.DefaultSetting, 0, crossReference.ExportOnly, guidEmptyString))
                            Next
                        Case "Products".ToLower
                            Dim crossReferenceList As ArrayList = KaProductInterfaceSettings.GetAll(GetUserConnection(_currentUser.Id), "deleted = 0 and product_id = " & Q(itemId) & " AND interface_id=" & Q(interfaceId) & " AND interface_id<>" & Q(Guid.Empty), "")
                            For Each crossReference As KaProductInterfaceSettings In crossReferenceList
                                interfaceList.Add(New ItemCrossReference.CrossReference(crossReference.Id.ToString, crossReference.CrossReference, crossReference.UnitId.ToString, crossReference.OrderItemUnitId.ToString(), crossReference.DefaultSetting, crossReference.ProductPriority, crossReference.ExportOnly, crossReference.ProductSplitFormulationFacilityId.ToString))
                            Next
                        Case "Suppliers".ToLower
                            Dim crossReferenceList As ArrayList = KaSupplierAccountInterfaceSettings.GetAll(GetUserConnection(_currentUser.Id), "deleted = 0 and supplier_account_id = " & Q(itemId) & " AND interface_id=" & Q(interfaceId) & " AND interface_id<>" & Q(Guid.Empty), "")
                            For Each crossReference As KaSupplierAccountInterfaceSettings In crossReferenceList
                                interfaceList.Add(New ItemCrossReference.CrossReference(crossReference.Id.ToString, crossReference.CrossReference, guidEmptyString, guidEmptyString, crossReference.DefaultSetting, 0, crossReference.ExportOnly, guidEmptyString))
                            Next
                        Case "Tanks".ToLower
                            Dim crossReferenceList As ArrayList = KaTankInterfaceSettings.GetAll(GetUserConnection(_currentUser.Id), "deleted = 0 and " & KaTankInterfaceSettings.FN_TANK_ID & " = " & Q(itemId) & " AND interface_id=" & Q(interfaceId) & " AND interface_id<>" & Q(Guid.Empty), "")
                            For Each crossReference As KaTankInterfaceSettings In crossReferenceList
                                interfaceList.Add(New ItemCrossReference.CrossReference(crossReference.Id.ToString, crossReference.CrossReference, guidEmptyString, guidEmptyString, crossReference.DefaultSetting, 0, crossReference.ExportOnly, guidEmptyString))
                            Next
                        Case "Transports".ToLower
                            Dim crossReferenceList As ArrayList = KaTransportInterfaceSettings.GetAll(GetUserConnection(_currentUser.Id), "deleted = 0 and " & KaTransportInterfaceSettings.FN_TRANSPORT_ID & " = " & Q(itemId) & " AND interface_id=" & Q(interfaceId) & " AND interface_id<>" & Q(Guid.Empty), "")
                            For Each crossReference As KaTransportInterfaceSettings In crossReferenceList
                                interfaceList.Add(New ItemCrossReference.CrossReference(crossReference.Id.ToString, crossReference.CrossReference, guidEmptyString, guidEmptyString, crossReference.DefaultSetting, 0, crossReference.ExportOnly, guidEmptyString))
                            Next
                        Case "Transport Types".ToLower
                            Dim crossReferenceList As ArrayList = KaTransportTypeInterfaceSettings.GetAll(GetUserConnection(_currentUser.Id), "deleted = 0 and " & KaTransportTypeInterfaceSettings.FN_TRANSPORT_TYPE_ID & " = " & Q(itemId) & " AND interface_id=" & Q(interfaceId) & " AND interface_id<>" & Q(Guid.Empty), "")
                            For Each crossReference As KaTransportTypeInterfaceSettings In crossReferenceList
                                interfaceList.Add(New ItemCrossReference.CrossReference(crossReference.Id.ToString, crossReference.CrossReference, guidEmptyString, guidEmptyString, crossReference.DefaultSetting, 0, crossReference.ExportOnly, guidEmptyString))
                            Next
                        Case "Units".ToLower
                            Dim crossReferenceList As ArrayList = KaUnitInterfaceSettings.GetAll(GetUserConnection(_currentUser.Id), "deleted = 0 and " & KaUnitInterfaceSettings.FN_UNIT_ID & " = " & Q(itemId) & " AND interface_id=" & Q(interfaceId) & " AND interface_id<>" & Q(Guid.Empty), "")
                            For Each crossReference As KaUnitInterfaceSettings In crossReferenceList
                                interfaceList.Add(New ItemCrossReference.CrossReference(crossReference.Id.ToString, crossReference.CrossReference, guidEmptyString, guidEmptyString, crossReference.DefaultSetting, 0, crossReference.ExportOnly, guidEmptyString))
                            Next
                    End Select
                Catch ex As RecordNotFoundException

                End Try
                If interfaceNumber = 1 Then
                    AddCrossReferencesToInterface1(currentItemRow, interfaceList, interfaceType1)
                Else
                    AddCrossReferencesToInterface2(currentItemRow, interfaceList, interfaceType2)
                End If
            End If
        Next
    End Sub

    Private Sub SetEnabledStatusForColumns()
        Dim interface1Selected As Boolean = (ddlInterface1.SelectedIndex > 0)
        Dim interface2Selected As Boolean = (ddlInterface2.SelectedIndex > 0)
        btnCopy.Visible = interface1Selected And interface2Selected
        btnSave.Visible = interface2Selected

        For rowCounter As Integer = 2 To tblInterfaceSetting.Rows.Count - 2
            Dim currentItemRow As HtmlTableRow = tblInterfaceSetting.Rows(rowCounter)
            Dim copyItemButton As Button = currentItemRow.FindControl(currentItemRow.ID & "CopyButton")
            If copyItemButton IsNot Nothing Then copyItemButton.Visible = interface1Selected And interface2Selected
            Dim interface2TableCell As HtmlTableCell = currentItemRow.FindControl(currentItemRow.ID & "Interface2Value")
            If interface2TableCell IsNot Nothing AndAlso interface2TableCell.Controls.Count > 0 Then interface2TableCell.Controls(0).Visible = interface2Selected
            Dim newInterfaceButton As Button = currentItemRow.FindControl("btn" & currentItemRow.ID & "NewCrossReference")
            If newInterfaceButton IsNot Nothing Then newInterfaceButton.Parent.Visible = interface2Selected
        Next
    End Sub

    Private Sub AddCrossReferencesToInterface1(ByRef currentItemRow As HtmlTableRow, ByVal interfaceList As List(Of ItemCrossReference.CrossReference), ByVal interfaceType As KaInterfaceTypes)
        For Each rowObject As Control In currentItemRow.Controls
            If rowObject.ID = currentItemRow.ID & "Interface1Value" Then
                With CType(rowObject, HtmlTableCell)
                    Dim interface1Table As HtmlGenericControl
                    .Controls.Clear()
                    interface1Table = New HtmlGenericControl("ul")
                    interface1Table.EnableViewState = True

                    .Controls.Add(interface1Table)
                    For Each interfaceItem As ItemCrossReference.CrossReference In interfaceList
                        If interfaceItem.Text.Trim.Length > 0 Then
                            If interface1Table.Controls.Count > 0 Then
                                Dim newLinerow As New HtmlGenericControl("li")
                                newLinerow.InnerHtml = "<hr />"
                                interface1Table.Controls.Add(newLinerow)
                            End If

                            Dim newInterfaceRow As New HtmlGenericControl("li")
                            newInterfaceRow.ID = "Interface1" & interfaceItem.Id
                            newInterfaceRow.EnableViewState = True
                            Dim interface1ValueLabel As New Label
                            With interface1ValueLabel
                                .Text = "Cross reference: "
                            End With
                            newInterfaceRow.Controls.Add(interface1ValueLabel)

                            Dim interface1ValueBox As New Label
                            With interface1ValueBox
                                .ID = newInterfaceRow.ID & "Interface1Value"
                                .Text = interfaceItem.Text
                                .EnableViewState = True
                            End With
                            newInterfaceRow.Controls.Add(interface1ValueBox)

                            Dim itemUnitOfMeasureId As New Label
                            With itemUnitOfMeasureId
                                .ID = newInterfaceRow.ID & "Interface1UnitOfMeasure"
                                .EnableViewState = True
                                .Text = interfaceItem.UofM
                                .Visible = False
                            End With
                            newInterfaceRow.Controls.Add(itemUnitOfMeasureId)

                            Dim itemUnitOfMeasure As New Label
                            With itemUnitOfMeasure
                                .ID = newInterfaceRow.ID & "Interface1UnitOfMeasureText"
                                .EnableViewState = True
                                .Visible = interfaceType.ShowInterfaceExchangeUnit AndAlso (_currentItemType = "Bulk Products" OrElse _currentItemType = "Products")
                                Try
                                    .Text = "<br />    Interface exchange unit: " & New KaUnit(GetUserConnection(_currentUser.Id), Guid.Parse(interfaceItem.UofM)).Abbreviation
                                Catch ex As RecordNotFoundException
                                    .Text = ""
                                End Try
                            End With
                            newInterfaceRow.Controls.Add(itemUnitOfMeasure)

                            Dim orderItemUnitOfMeasureId As New Label
                            With orderItemUnitOfMeasureId
                                .ID = newInterfaceRow.ID & "Interface1OrderItemUnitOfMeasure"
                                .EnableViewState = True
                                .Text = interfaceItem.OrderItemUofM
                                .Visible = False
                            End With
                            newInterfaceRow.Controls.Add(orderItemUnitOfMeasureId)

                            Dim orderItemUnitOfMeasure As New Label
                            With orderItemUnitOfMeasure
                                .ID = newInterfaceRow.ID & "Interface1OrderItemUnitOfMeasureText"
                                .EnableViewState = True
                                .Visible = Not interfaceType.UseInterfaceUnitAsOrderItemUnit AndAlso _currentItemType = "Products"
                                Try
                                    .Text = "<br />    Order Import Unit: " & New KaUnit(GetUserConnection(_currentUser.Id), Guid.Parse(interfaceItem.OrderItemUofM)).Abbreviation
                                Catch ex As RecordNotFoundException
                                    .Text = ""
                                End Try
                            End With
                            newInterfaceRow.Controls.Add(orderItemUnitOfMeasure)

                            Dim SplitProductFacilityId As New Label
                            With SplitProductFacilityId
                                .ID = newInterfaceRow.ID & "Interface1SplitProductFacilityId"
                                .EnableViewState = True
                                .Text = interfaceItem.SplitProductFacilityId
                                .Visible = False
                            End With
                            newInterfaceRow.Controls.Add(SplitProductFacilityId)

                            Dim splitProductFacility As New Label
                            With splitProductFacility
                                .ID = newInterfaceRow.ID & "Interface1SplitProductFacilitytext"
                                .EnableViewState = True
                                .Visible = interfaceType.SplitProductIntoComponents AndAlso _currentItemType = "Products"
                                Try
                                    .Text = "<br />    Split product according to formulation at facility: " & New KaLocation(GetUserConnection(_currentUser.Id), Guid.Parse(interfaceItem.SplitProductFacilityId)).Name
                                Catch ex As RecordNotFoundException
                                    .Text = "<br />    Do not split product into components"
                                End Try
                            End With
                            newInterfaceRow.Controls.Add(splitProductFacility)

                            Dim isDefaultId As New Label
                            With isDefaultId
                                .ID = newInterfaceRow.ID & "Interface1IsDefault"
                                .EnableViewState = True
                                .Text = interfaceItem.IsDefaultSetting.ToString
                                .Visible = False
                            End With
                            newInterfaceRow.Controls.Add(isDefaultId)
                            Dim isDefault As New Label
                            With isDefault
                                .ID = newInterfaceRow.ID & "Interface1IsDefaultText"
                                .Text = IIf(interfaceItem.IsDefaultSetting, "<br />    Default Setting", "")
                            End With
                            newInterfaceRow.Controls.Add(isDefault)

                            Dim isExportOnlyId As New Label
                            With isExportOnlyId
                                .ID = newInterfaceRow.ID & "Interface1ExportOnly"
                                .EnableViewState = True
                                .Text = interfaceItem.ExportOnly.ToString
                                .Visible = False
                            End With
                            newInterfaceRow.Controls.Add(isExportOnlyId)
                            Dim isExportOnly As New Label
                            With isExportOnly
                                .ID = newInterfaceRow.ID & "Interface1ExportOnlyText"
                                .Text = IIf(interfaceItem.ExportOnly, "<br />    Export Only", "")
                            End With
                            newInterfaceRow.Controls.Add(isExportOnly)

                            Dim priority1Label As New Label
                            With priority1Label
                                .Text = "<br />    Order item sort priority: "
                                .Visible = (_currentItemType = "Products")
                            End With
                            newInterfaceRow.Controls.Add(priority1Label)
                            Dim priority1Value As New Label
                            With priority1Value
                                .ID = newInterfaceRow.ID & "Priority1Value"
                                .Text = interfaceItem.ProductPriority
                                .EnableViewState = True
                                .Visible = (_currentItemType = "Products")
                            End With
                            newInterfaceRow.Controls.Add(priority1Value)

                            interface1Table.Controls.Add(newInterfaceRow)
                        End If
                    Next
                End With
            End If
        Next
    End Sub

    Private Sub AddCrossReferencesToInterface2(ByRef currentItemRow As HtmlTableRow, ByVal interfaceList As List(Of ItemCrossReference.CrossReference), ByVal interfaceType As KaInterfaceTypes)
        Dim maxLengthCrossReference As Integer
        Select Case _currentItemType
            Case "Applicators"
                maxLengthCrossReference = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaApplicatorInterfaceSettings.TABLE_NAME, KaApplicatorInterfaceSettings.FN_CROSS_REFERENCE))
            Case "Branches"
                maxLengthCrossReference = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaBranchInterfaceSettings.TABLE_NAME, KaBranchInterfaceSettings.FN_CROSS_REFERENCE))
            Case "Bulk Products"
                maxLengthCrossReference = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaBulkProductInterfaceSettings.TABLE_NAME, KaBulkProductInterfaceSettings.FN_CROSS_REFERENCE))
            Case "Carriers"
                maxLengthCrossReference = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaCarrierInterfaceSettings.TABLE_NAME, KaCarrierInterfaceSettings.FN_CROSS_REFERENCE))
            Case "Customer Account Destinations"
                maxLengthCrossReference = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaCustomerAccountLocationInterfaceSettings.TABLE_NAME, KaCustomerAccountLocationInterfaceSettings.FN_CROSS_REFERENCE))
            Case "Customer Accounts"
                maxLengthCrossReference = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaCustomerAccountInterfaceSettings.TABLE_NAME, KaCustomerAccountInterfaceSettings.FN_CROSS_REFERENCE))
            Case "Drivers"
                maxLengthCrossReference = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaDriverInterfaceSettings.TABLE_NAME, KaDriverInterfaceSettings.FN_CROSS_REFERENCE))
            Case "Facilities"
                maxLengthCrossReference = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaLocationInterfaceSettings.TABLE_NAME, KaLocationInterfaceSettings.FN_CROSS_REFERENCE))
            Case "Owners"
                maxLengthCrossReference = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaOwnerInterfaceSettings.TABLE_NAME, KaOwnerInterfaceSettings.FN_CROSS_REFERENCE))
            Case "Products"
                maxLengthCrossReference = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaProductInterfaceSettings.TABLE_NAME, KaProductInterfaceSettings.FN_CROSS_REFERENCE))
            Case "Suppliers"
                maxLengthCrossReference = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaSupplierAccountInterfaceSettings.TABLE_NAME, KaSupplierAccountInterfaceSettings.FN_CROSS_REFERENCE))
            Case "Tanks"
                maxLengthCrossReference = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaTankInterfaceSettings.TABLE_NAME, KaTankInterfaceSettings.FN_CROSS_REFERENCE))
            Case "Transports"
                maxLengthCrossReference = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaTransportInterfaceSettings.TABLE_NAME, KaTransportInterfaceSettings.FN_CROSS_REFERENCE))
            Case "Transport Types"
                maxLengthCrossReference = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaTransportTypeInterfaceSettings.TABLE_NAME, KaTransportTypeInterfaceSettings.FN_CROSS_REFERENCE))
            Case "Units"
                maxLengthCrossReference = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaUnitInterfaceSettings.TABLE_NAME, KaUnitInterfaceSettings.FN_CROSS_REFERENCE))
            Case Else
                maxLengthCrossReference = 0
        End Select

        Dim interfaceType2 As KaInterfaceTypes
        Try
            Dim interfaceId2 As Guid = Guid.Empty
            Guid.TryParse(ddlInterface2.SelectedValue, interfaceId2)
            interfaceType2 = New KaInterfaceTypes(Tm2Database.Connection, New KaInterface(Tm2Database.Connection, interfaceId2).InterfaceTypeId)
        Catch ex As RecordNotFoundException
            interfaceType2 = New KaInterfaceTypes()
        End Try

        Dim defaultMassUnitId As Guid = KaUnit.GetSystemDefaultMassUnitOfMeasure(Tm2Database.Connection, Nothing)
        For Each rowObject As Control In currentItemRow.Controls
            If rowObject.ID = currentItemRow.ID & "Interface2Value" Then
                With CType(rowObject, HtmlTableCell)
                    Dim interface2Table As HtmlGenericControl
                    .Controls.Clear()
                    interface2Table = New HtmlGenericControl("ul")
                    With interface2Table
                        .EnableViewState = True
                    End With

                    .Controls.Add(interface2Table)

                    For Each interfaceItem As ItemCrossReference.CrossReference In interfaceList
                        If interface2Table.Controls.Count > 0 Then
                            Dim newLinerow As New HtmlGenericControl("li")
                            newLinerow.InnerHtml = "<hr />"
                            interface2Table.Controls.Add(newLinerow)
                        End If

                        interface2Table.Controls.Add(Interface2Row(interfaceItem.Id, interfaceItem.Text, interfaceItem.UofM, interfaceItem.OrderItemUofM, interfaceItem.IsDefaultSetting, interfaceItem.ProductPriority, interfaceItem.ExportOnly, interfaceItem.SplitProductFacilityId, interfaceType2))
                    Next

                    If interface2Table.Controls.Count = 0 Then interface2Table.Controls.Add(Interface2Row("NewRow" & Guid.NewGuid.ToString, "", defaultMassUnitId.ToString, Guid.Empty.ToString, True, "100", False, Guid.Empty.ToString, interfaceType2))
                End With
            End If
        Next
    End Sub

    Private Function Interface2Row(ByVal interfaceItemId As String, ByVal crossReferenceValue As String, ByVal interfaceItemUofM As String, ByVal interfaceOrderItemUofM As String, ByVal isDefaultSetting As Boolean, ByVal productPriority As String, ByVal exportOnly As Boolean, ByVal formulationFacilityId As String, ByVal interfaceType As KaInterfaceTypes) As HtmlGenericControl
        Dim newInterfaceRow As New HtmlGenericControl("li")
        '  newInterfaceRow.Attributes("style") = "vertical-align: top;"
        newInterfaceRow.ID = interfaceItemId
        newInterfaceRow.EnableViewState = True

        'Dim pnlInterfaceOptions As New HtmlGenericControl("div")
        'pnlInterfaceOptions.Attributes("Class") = "SectionOdd"
        'newInterfaceRow.Controls.Add(pnlInterfaceOptions)

        Dim interfaceOptions As New HtmlGenericControl("ul")
        ' interfaceOptions.Attributes("style") = "width: 45%;"
        newInterfaceRow.Controls.Add(interfaceOptions)

        Dim interface2ValueRow As New HtmlGenericControl("li")
        interfaceOptions.Controls.Add(interface2ValueRow)
        Dim interface2ValueLabel As New HtmlGenericControl("Label")
        With interface2ValueLabel
            .InnerText = "Cross Reference"
        End With
        interface2ValueRow.Controls.Add(interface2ValueLabel)
        Dim interface2ValueBox As New TextBox
        With interface2ValueBox
            .ID = newInterfaceRow.ID & "Interface2Value"
            .Text = crossReferenceValue
            .EnableViewState = True
            .MaxLength = 50
        End With
        interface2ValueRow.Controls.Add(interface2ValueBox)

        Dim itemUnitOfMeasureRow As New HtmlGenericControl("li")
        itemUnitOfMeasureRow.Visible = interfaceType.ShowInterfaceExchangeUnit AndAlso (_currentItemType = "Bulk Products" OrElse _currentItemType = "Products")
        interfaceOptions.Controls.Add(itemUnitOfMeasureRow)
        Dim itemUnitOfMeasureLabel As New HtmlGenericControl("Label")
        With itemUnitOfMeasureLabel
            .InnerText = "Interface exchange unit"
        End With
        itemUnitOfMeasureRow.Controls.Add(itemUnitOfMeasureLabel)
        Dim itemUnitOfMeasure As New DropDownList
        With itemUnitOfMeasure
            .CssClass = "input"
            .ID = newInterfaceRow.ID & "Interface2UnitOfMeasure"
            .EnableViewState = True
        End With
        PopulateUnitDropdownLists(itemUnitOfMeasure, interfaceItemUofM)
        itemUnitOfMeasureRow.Controls.Add(itemUnitOfMeasure)

        Dim orderItemUnitOfMeasureRow As New HtmlGenericControl("li")
        orderItemUnitOfMeasureRow.Visible = (Not interfaceType.UseInterfaceUnitAsOrderItemUnit AndAlso _currentItemType = "Products")
        interfaceOptions.Controls.Add(orderItemUnitOfMeasureRow)
        Dim orderItemUnitOfMeasureLabel As New HtmlGenericControl("Label")
        With orderItemUnitOfMeasureLabel
            .InnerText = "Order item unit"
        End With
        orderItemUnitOfMeasureRow.Controls.Add(orderItemUnitOfMeasureLabel)
        Dim orderItemUnitOfMeasure As New DropDownList
        With orderItemUnitOfMeasure
            .CssClass = "input"
            .ID = newInterfaceRow.ID & "Interface2OrderItemUnitOfMeasure"
            .EnableViewState = True
        End With
        PopulateUnitDropdownLists(orderItemUnitOfMeasure, interfaceOrderItemUofM)
        orderItemUnitOfMeasureRow.Controls.Add(orderItemUnitOfMeasure)

        Dim splitProductFacilityIdRow As New HtmlGenericControl("li")
        splitProductFacilityIdRow.Visible = (interfaceType.SplitProductIntoComponents AndAlso _currentItemType = "Products")
        interfaceOptions.Controls.Add(splitProductFacilityIdRow)
        Dim splitProductFacilityIdLabel As New HtmlGenericControl("Label")
        With splitProductFacilityIdLabel
            .InnerText = "Split product formulation facility"
        End With
        splitProductFacilityIdRow.Controls.Add(splitProductFacilityIdLabel)
        Dim SplitProductFacilityId As New DropDownList
        With SplitProductFacilityId
            .CssClass = "input"
            .ID = newInterfaceRow.ID & "Interface2SplitProductFacilityId"
            .EnableViewState = True
        End With
        PopulateFacilityForSplitProductDropdownLists(SplitProductFacilityId, formulationFacilityId)
        splitProductFacilityIdRow.Controls.Add(SplitProductFacilityId)

        Dim isDefaultRow As New HtmlGenericControl("li")
        interfaceOptions.Controls.Add(isDefaultRow)
        Dim isDefaultLabel As New HtmlGenericControl("Label")
        With isDefaultLabel
            .InnerText = "Default setting"
        End With
        isDefaultRow.Controls.Add(isDefaultLabel)
        Dim isDefaultControl As New CheckBox
        With isDefaultControl
            .CssClass = "input"
            .ID = newInterfaceRow.ID & "Interface2IsDefault"
            .EnableViewState = True
            .Text = ""
            .Checked = isDefaultSetting
            .AutoPostBack = True
            .Visible = True
            .TextAlign = TextAlign.Left
            AddHandler .CheckedChanged, AddressOf IsDefaultChecked_Checked
        End With
        isDefaultRow.Controls.Add(isDefaultControl)

        Dim isExportOnlyRow As New HtmlGenericControl("li")
        interfaceOptions.Controls.Add(isExportOnlyRow)
        Dim isExportOnlyLabel As New HtmlGenericControl("Label")
        With isExportOnlyLabel
            .InnerText = "Export Only Setting"
        End With
        isExportOnlyRow.Controls.Add(isExportOnlyLabel)
        Dim isExportOnlyControl As New CheckBox
        With isExportOnlyControl
            .CssClass = "input"
            .ID = newInterfaceRow.ID & "Interface2ExportOnly"
            .EnableViewState = True
            .Text = ""
            .Checked = exportOnly
            .Visible = True
            .TextAlign = TextAlign.Left
        End With
        isExportOnlyRow.Controls.Add(isExportOnlyControl)

        Dim priority2Row As New HtmlGenericControl("li")
        priority2Row.Visible = (_currentItemType = "Products")
        interfaceOptions.Controls.Add(priority2Row)
        Dim priority2Label As New HtmlGenericControl("Label")
        With priority2Label
            .InnerText = " Order item sort priority"
        End With
        priority2Row.Controls.Add(priority2Label)
        Dim priority2ValueBox As New TextBox
        With priority2ValueBox
            .CssClass = "input"
            .ID = newInterfaceRow.ID & "Priority2Value"
            .Text = productPriority
            .EnableViewState = True
            .MaxLength = 3
        End With
        priority2Row.Controls.Add(priority2ValueBox)

        Return newInterfaceRow
    End Function

    Private Sub PopulateUnitDropdownLists(ByRef ddlInterfaceUnit As DropDownList, ByVal currentValue As String)
        ddlInterfaceUnit.Items.Clear()
        ddlInterfaceUnit.Items.Add(New ListItem("", Guid.Empty.ToString()))
        Dim unitFound As Boolean = False
        For Each u As KaUnit In KaUnit.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
            If Not KaUnit.IsTime(u.BaseUnit) Then
                ddlInterfaceUnit.Items.Add(New ListItem(u.Abbreviation, u.Id.ToString()))
                If currentValue = u.Id.ToString Then
                    unitFound = True
                    ddlInterfaceUnit.SelectedIndex = ddlInterfaceUnit.Items.Count - 1
                End If
            End If
        Next
        If Not unitFound Then
            Try
                ddlInterfaceUnit.SelectedValue = KaUnit.GetSystemDefaultMassUnitOfMeasure(GetUserConnection(_currentUser.Id), Nothing).ToString()
            Catch ex As Exception
                ddlInterfaceUnit.SelectedIndex = 0
            End Try
        End If
    End Sub

    Private Sub PopulateFacilityForSplitProductDropdownLists(ByRef ddlInterfaceFacility As DropDownList, ByVal currentValue As String)
        ddlInterfaceFacility.Items.Clear()
        ddlInterfaceFacility.Items.Add(New ListItem("Do not split product", Guid.Empty.ToString()))
        For Each u As KaLocation In KaLocation.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
            ddlInterfaceFacility.Items.Add(New ListItem(u.Name, u.Id.ToString()))
        Next
        Try
            ddlInterfaceFacility.SelectedValue = currentValue
        Catch ex As Exception
            ddlInterfaceFacility.SelectedIndex = 0
        End Try
    End Sub

#Region " Item Row Handlers "
    Private Sub copyButton_Click(sender As Object, e As System.EventArgs) Handles btnCopy.Click
        Dim interfaceType2 As KaInterfaceTypes
        Try
            Dim interfaceId2 As Guid = Guid.Empty
            Guid.TryParse(ddlInterface2.SelectedValue, interfaceId2)
            interfaceType2 = New KaInterfaceTypes(Tm2Database.Connection, New KaInterface(Tm2Database.Connection, interfaceId2).InterfaceTypeId)
        Catch ex As RecordNotFoundException
            interfaceType2 = New KaInterfaceTypes()
        End Try
        For rowCounter As Integer = 2 To tblInterfaceSetting.Rows.Count - 2
            Dim currentItemRow As HtmlTableRow = tblInterfaceSetting.Rows(rowCounter)
            Dim interfaceList As New List(Of ItemCrossReference.CrossReference)
            Dim interfaceCell As HtmlTableCell = currentItemRow.FindControl(currentItemRow.ID & "Interface1Value")
            If interfaceCell.Controls.Count > 0 Then
                With CType(interfaceCell.Controls(0), HtmlGenericControl)
                    For Each interfaceRow As HtmlGenericControl In .Controls
                        If currentItemRow.FindControl(interfaceRow.ID & "Interface1Value") Is Nothing Then Continue For
                        Dim showDefaultSetting As Boolean = False
                        interfaceList.Add(New ItemCrossReference.CrossReference("NewRow" & Guid.NewGuid.ToString, _
                            CType(currentItemRow.FindControl(interfaceRow.ID & "Interface1Value"), Label).Text, _
                            CType(currentItemRow.FindControl(interfaceRow.ID & "Interface1UnitOfMeasure"), Label).Text.Replace("<br />", "").Trim, _
                            CType(currentItemRow.FindControl(interfaceRow.ID & "Interface1OrderItemUnitOfMeasure"), Label).Text.Replace("<br />", "").Trim, _
                            Boolean.Parse(CType(currentItemRow.FindControl(interfaceRow.ID & "Interface1IsDefault"), Label).Text.Replace("<br />", "").Trim), _
                            CType(currentItemRow.FindControl(interfaceRow.ID & "Priority1Value"), Label).Text.Replace("<br />", "").Trim,
                            Boolean.Parse(CType(currentItemRow.FindControl(interfaceRow.ID & "Interface1ExportOnly"), Label).Text.Replace("<br />", "").Trim), _
                            CType(currentItemRow.FindControl(interfaceRow.ID & "Interface1SplitProductFacilityId"), Label).Text.Replace("<br />", "").Trim))
                    Next
                End With
            End If
            AddCrossReferencesToInterface2(currentItemRow, interfaceList, interfaceType2)
        Next
        SetEnabledStatusForColumns()
    End Sub

    Private Sub itemCopyButton_Click(sender As Object, e As System.EventArgs)
        Dim interfaceType2 As KaInterfaceTypes
        Try
            Dim interfaceId2 As Guid = Guid.Empty
            Guid.TryParse(ddlInterface2.SelectedValue, interfaceId2)
            interfaceType2 = New KaInterfaceTypes(Tm2Database.Connection, New KaInterface(Tm2Database.Connection, interfaceId2).InterfaceTypeId)
        Catch ex As RecordNotFoundException
            interfaceType2 = New KaInterfaceTypes()
        End Try

        Dim currentItemRow As HtmlTableRow = GetParentRow(sender)
        Dim interfaceList As New List(Of ItemCrossReference.CrossReference)
        Dim interfaceCell As HtmlTableCell = currentItemRow.FindControl(currentItemRow.ID & "Interface1Value")
        If interfaceCell.Controls.Count > 0 Then
            With CType(interfaceCell.Controls(0), HtmlGenericControl)
                For Each interfaceRow As HtmlGenericControl In .Controls
                    If currentItemRow.FindControl(interfaceRow.ID & "Interface1Value") Is Nothing Then Continue For
                    interfaceList.Add(New ItemCrossReference.CrossReference("NewRow" & Guid.NewGuid.ToString, _
                        CType(currentItemRow.FindControl(interfaceRow.ID & "Interface1Value"), Label).Text, _
                        CType(currentItemRow.FindControl(interfaceRow.ID & "Interface1UnitOfMeasure"), Label).Text.Replace("<br />", "").Trim, _
                        CType(currentItemRow.FindControl(interfaceRow.ID & "Interface1OrderItemUnitOfMeasure"), Label).Text.Replace("<br />", "").Trim, _
                        Boolean.Parse(CType(currentItemRow.FindControl(interfaceRow.ID & "Interface1IsDefault"), Label).Text.Replace("<br />", "").Trim), _
                        CType(currentItemRow.FindControl(interfaceRow.ID & "Priority1Value"), Label).Text.Replace("<br />", "").Trim, _
                        Boolean.Parse(CType(currentItemRow.FindControl(interfaceRow.ID & "Interface1ExportOnly"), Label).Text.Replace("<br />", "").Trim), _
                        CType(currentItemRow.FindControl(interfaceRow.ID & "Interface1SplitProductFacilityId"), Label).Text.Replace("<br />", "").Trim))
                Next
            End With
        End If
        AddCrossReferencesToInterface2(currentItemRow, interfaceList, interfaceType2)
        SetEnabledStatusForColumns()
    End Sub

    Private Sub itemNewCrossRefButton_Click(sender As Object, e As System.EventArgs)
        Dim interfaceType2 As KaInterfaceTypes
        Try
            Dim interfaceId2 As Guid = Guid.Empty
            Guid.TryParse(ddlInterface2.SelectedValue, interfaceId2)
            interfaceType2 = New KaInterfaceTypes(Tm2Database.Connection, New KaInterface(Tm2Database.Connection, interfaceId2).InterfaceTypeId)
        Catch ex As RecordNotFoundException
            interfaceType2 = New KaInterfaceTypes()
        End Try
        Dim defaultMassUnitId As Guid = KaUnit.GetSystemDefaultMassUnitOfMeasure(Tm2Database.Connection, Nothing)
        Dim currentItemRow As HtmlTableRow = GetParentRow(sender)
        Dim interfaceList As New List(Of ItemCrossReference.CrossReference)
        Dim interfaceCell As HtmlTableCell = currentItemRow.FindControl(currentItemRow.ID & "Interface2Value")

        If interfaceCell.Controls.Count > 0 Then
            With CType(interfaceCell.Controls(0), HtmlGenericControl)
                For Each interfaceRow As HtmlGenericControl In .Controls
                    If currentItemRow.FindControl(interfaceRow.ID & "Interface2Value") Is Nothing Then Continue For
                    Dim crossReference As String = CType(currentItemRow.FindControl(interfaceRow.ID & "Interface2Value"), TextBox).Text
                    Dim priority As String = CType(currentItemRow.FindControl(interfaceRow.ID & "Priority2Value"), TextBox).Text
                    interfaceList.Add(New ItemCrossReference.CrossReference(interfaceRow.ID, crossReference, _
                        CType(currentItemRow.FindControl(interfaceRow.ID & "Interface2UnitOfMeasure"), DropDownList).SelectedValue, _
                        CType(currentItemRow.FindControl(interfaceRow.ID & "Interface2OrderItemUnitOfMeasure"), DropDownList).SelectedValue, _
                        CType(currentItemRow.FindControl(interfaceRow.ID & "Interface2IsDefault"), CheckBox).Checked, _
                        priority, _
                        CType(currentItemRow.FindControl(interfaceRow.ID & "Interface2ExportOnly"), CheckBox).Checked, _
                        CType(currentItemRow.FindControl(interfaceRow.ID & "Interface2SplitProductFacilityId"), DropDownList).SelectedValue))
                Next
            End With
        End If

        interfaceList.Add(New ItemCrossReference.CrossReference("NewRow" & Guid.NewGuid.ToString, "", defaultMassUnitId.ToString, defaultMassUnitId.ToString(), False, 100, False, Guid.Empty.ToString))
        AddCrossReferencesToInterface2(currentItemRow, interfaceList, interfaceType2)
        SetEnabledStatusForColumns()
    End Sub

    Private Sub IsDefaultChecked_Checked(sender As Object, e As System.EventArgs)
        Dim currentCheckbox As CheckBox = CType(sender, CheckBox)
        Dim currentItemRow As HtmlTableRow = GetParentRow(sender)
        Dim interfaceCell As HtmlTableCell = currentItemRow.FindControl(currentItemRow.ID & "Interface2Value")

        If interfaceCell.Controls.Count > 0 Then
            With CType(interfaceCell.Controls(0), HtmlGenericControl)
                For Each interfaceRow As HtmlGenericControl In .Controls
                    If currentItemRow.FindControl(interfaceRow.ID & "Interface2IsDefault") Is Nothing Then Continue For
                    Dim checkBoxControl As CheckBox = CType(currentItemRow.FindControl(interfaceRow.ID & "Interface2IsDefault"), CheckBox)
                    If checkBoxControl.ID <> currentCheckbox.ID Then
                        ' Uncheck all default check boxes except the one that was just checked.
                        checkBoxControl.Checked = False
                    End If
                Next
            End With
        End If

        SetEnabledStatusForColumns()
    End Sub

    'Private Sub itemUnitOfMeasure_SelectedIndexChanged(sender As Object, e As System.EventArgs)
    '    Dim currentItemRow As HtmlTableRow = GetParentRow(sender)
    '    CType(currentItemRow.FindControl(currentItemRow.ID & "Interface2Value"), TextBox).Text = CType(currentItemRow.FindControl(currentItemRow.ID & "Interface1Value"), HtmlTableCell).InnerText
    '    CType(currentItemRow.FindControl(currentItemRow.ID & "Interface2Id"), HtmlTableCell).InnerText = Guid.Empty.ToString
    'End Sub

    'Private Sub orderItemUnitOfMeasure_SelectedIndexChanged(sender As Object, e As System.EventArgs)
    '    Dim currentItemRow As HtmlTableRow = GetParentRow(sender)
    '    CType(currentItemRow.FindControl(currentItemRow.ID & "Interface2Value"), TextBox).Text = CType(currentItemRow.FindControl(currentItemRow.ID & "Interface1Value"), HtmlTableCell).InnerText
    '    CType(currentItemRow.FindControl(currentItemRow.ID & "Interface2Id"), HtmlTableCell).InnerText = Guid.Empty.ToString
    'End Sub

    Private Function GetParentRow(ByVal webObject As Object) As HtmlTableRow
        If webObject.parent Is Nothing Then
            Return Nothing
        ElseIf TypeOf webObject.parent Is HtmlTableRow Then
            Return webObject.parent
        Else
            Return GetParentRow(webObject.parent)
        End If
    End Function
#End Region

#Region " Save settings "
    Private Sub copySave_Click(sender As Object, e As System.EventArgs) Handles btnSave.Click
        Dim interfaceId As Guid = Guid.Empty
        Dim interfaceType As New KaInterfaceTypes()

        If ValidateSettings(interfaceId, interfaceType) Then
            Dim connection As New OleDbConnection(Tm2Database.GetDbConnection)
            Dim transaction As OleDbTransaction = Nothing
            Try
                connection.Open()
                transaction = connection.BeginTransaction

                For rowCounter As Integer = 2 To tblInterfaceSetting.Rows.Count - 2
                    Dim currentItemRow As HtmlTableRow = tblInterfaceSetting.Rows(rowCounter)
                    Dim itemId As Guid = Guid.Empty

                    If Guid.TryParse(CType(currentItemRow.FindControl(currentItemRow.ID & "Id"), HtmlTableCell).InnerText, itemId) AndAlso _
                            Not itemId.Equals(Guid.Empty) Then

                        Dim interface2Cell As HtmlTableCell = currentItemRow.FindControl(currentItemRow.ID & "Interface2Value")
                        If interface2Cell.Controls.Count > 0 Then
                            With CType(interface2Cell.Controls(0), HtmlGenericControl)
                                Dim validIds As String = ""
                                For Each interfaceRow As HtmlGenericControl In .Controls
                                    If currentItemRow.FindControl(interfaceRow.ID & "Interface2Value") Is Nothing Then Continue For
                                    Dim crossReference As String = CType(currentItemRow.FindControl(interfaceRow.ID & "Interface2Value"), TextBox).Text.Trim
                                    If crossReference.Length = 0 Then Continue For

                                    Dim itemXrefId As Guid = Guid.Empty
                                    Guid.TryParse(interfaceRow.ID, itemXrefId)

                                    Dim unitOfMeasureId As Guid = Guid.Empty
                                    If currentItemRow.FindControl(interfaceRow.ID & "Interface2UnitOfMeasure") IsNot Nothing Then
                                        Guid.TryParse(CType(currentItemRow.FindControl(interfaceRow.ID & "Interface2UnitOfMeasure"), DropDownList).SelectedValue, unitOfMeasureId)
                                    End If

                                    Dim orderItemUnitOfMeasureId As Guid = Guid.Empty
                                    If currentItemRow.FindControl(interfaceRow.ID & "Interface2OrderItemUnitOfMeasure") IsNot Nothing Then
                                        Guid.TryParse(CType(currentItemRow.FindControl(interfaceRow.ID & "Interface2OrderItemUnitOfMeasure"), DropDownList).SelectedValue, orderItemUnitOfMeasureId)
                                    End If

                                    Dim isDefault As Boolean = False
                                    If currentItemRow.FindControl(interfaceRow.ID & "Interface2IsDefault") IsNot Nothing Then
                                        isDefault = CType(currentItemRow.FindControl(interfaceRow.ID & "Interface2IsDefault"), CheckBox).Checked
                                    End If

                                    Dim priority As Double = CType(currentItemRow.FindControl(interfaceRow.ID & "Priority2Value"), TextBox).Text.Trim

                                    Dim exportOnly As Boolean = False
                                    If currentItemRow.FindControl(interfaceRow.ID & "Interface2ExportOnly") IsNot Nothing Then
                                        exportOnly = CType(currentItemRow.FindControl(interfaceRow.ID & "Interface2ExportOnly"), CheckBox).Checked
                                    End If

                                    Dim splitProductFacilityId As Guid = Guid.Empty
                                    If currentItemRow.FindControl(interfaceRow.ID & "Interface2SplitProductFacilityId") IsNot Nothing Then
                                        Guid.TryParse(CType(currentItemRow.FindControl(interfaceRow.ID & "Interface2SplitProductFacilityId"), DropDownList).SelectedValue, splitProductFacilityId)
                                    End If

                                    Select Case _currentItemType
                                        Case "Applicators"
                                            Dim newCrossReference As New KaApplicatorInterfaceSettings
                                            With newCrossReference
                                                .Id = itemXrefId
                                                .ApplicatorId = itemId
                                                .CrossReference = crossReference
                                                .Deleted = False
                                                .ExportOnly = exportOnly
                                                .InterfaceId = interfaceId
                                                .DefaultSetting = isDefault
                                                .SqlUpdateInsertIfNotFound(connection, transaction, Database.ApplicationIdentifier, _currentUser.Name)
                                                If validIds.Length > 0 Then validIds &= ","
                                                validIds &= Q(.Id)
                                            End With
                                        Case "Branches"
                                            Dim newCrossReference As New KaBranchInterfaceSettings
                                            With newCrossReference
                                                .Id = itemXrefId
                                                .BranchId = itemId
                                                .CrossReference = crossReference
                                                .Deleted = False
                                                .ExportOnly = exportOnly
                                                .InterfaceId = interfaceId
                                                .DefaultSetting = isDefault
                                                .SqlUpdateInsertIfNotFound(connection, transaction, Database.ApplicationIdentifier, _currentUser.Name)
                                                If validIds.Length > 0 Then validIds &= ","
                                                validIds &= Q(.Id)
                                            End With
                                        Case "Bulk Products"
                                            Dim newCrossReference As New KaBulkProductInterfaceSettings
                                            With newCrossReference
                                                .Id = itemXrefId
                                                .BulkProductId = itemId
                                                .CrossReference = crossReference
                                                .Deleted = False
                                                .ExportOnly = exportOnly
                                                If interfaceType.ShowInterfaceExchangeUnit Then .UnitId = unitOfMeasureId
                                                .InterfaceId = interfaceId
                                                .DefaultSetting = isDefault
                                                .SqlUpdateInsertIfNotFound(connection, transaction, Database.ApplicationIdentifier, _currentUser.Name)
                                                If validIds.Length > 0 Then validIds &= ","
                                                validIds &= Q(.Id)
                                            End With
                                        Case "Carriers"
                                            Dim newCrossReference As New KaCarrierInterfaceSettings
                                            With newCrossReference
                                                .CrossReference = crossReference
                                                .Deleted = False
                                                .ExportOnly = exportOnly
                                                .Id = itemXrefId
                                                .InterfaceId = interfaceId
                                                .DefaultSetting = isDefault
                                                .CarrierId = itemId
                                                .SqlUpdateInsertIfNotFound(connection, transaction, Database.ApplicationIdentifier, _currentUser.Name)
                                                If validIds.Length > 0 Then validIds &= ","
                                                validIds &= Q(.Id)
                                            End With
                                        Case "Customer Account Destinations"
                                            Dim newCrossReference As New KaCustomerAccountLocationInterfaceSettings
                                            With newCrossReference
                                                .CrossReference = crossReference
                                                .CustomerAccountLocationId = itemId
                                                .Deleted = False
                                                .ExportOnly = exportOnly
                                                .Id = itemXrefId
                                                .InterfaceId = interfaceId
                                                .DefaultSetting = isDefault
                                                .SqlUpdateInsertIfNotFound(connection, transaction, Database.ApplicationIdentifier, _currentUser.Name)
                                                If validIds.Length > 0 Then validIds &= ","
                                                validIds &= Q(.Id)
                                            End With
                                        Case "Customer Accounts"
                                            Dim newCrossReference As New KaCustomerAccountInterfaceSettings
                                            With newCrossReference
                                                .CrossReference = crossReference
                                                .CustomerAccountId = itemId
                                                .Deleted = False
                                                .ExportOnly = exportOnly
                                                .Id = itemXrefId
                                                .InterfaceId = interfaceId
                                                .DefaultSetting = isDefault
                                                .SqlUpdateInsertIfNotFound(connection, transaction, Database.ApplicationIdentifier, _currentUser.Name)
                                                If validIds.Length > 0 Then validIds &= ","
                                                validIds &= Q(.Id)
                                            End With
                                        Case "Drivers"
                                            Dim newCrossReference As New KaDriverInterfaceSettings
                                            With newCrossReference
                                                .CrossReference = crossReference
                                                .Deleted = False
                                                .ExportOnly = exportOnly
                                                .Id = itemXrefId
                                                .InterfaceId = interfaceId
                                                .DefaultSetting = isDefault
                                                .DriverId = itemId
                                                .SqlUpdateInsertIfNotFound(connection, transaction, Database.ApplicationIdentifier, _currentUser.Name)
                                                If validIds.Length > 0 Then validIds &= ","
                                                validIds &= Q(.Id)
                                            End With
                                        Case "Facilities"
                                            Dim newCrossReference As New KaLocationInterfaceSettings
                                            With newCrossReference
                                                .Id = itemXrefId
                                                .CrossReference = crossReference
                                                .DefaultSetting = isDefault
                                                .Deleted = False
                                                .ExportOnly = exportOnly
                                                .InterfaceId = interfaceId
                                                .LocationId = itemId
                                                .SqlUpdateInsertIfNotFound(connection, transaction, Database.ApplicationIdentifier, _currentUser.Name)
                                                If validIds.Length > 0 Then validIds &= ","
                                                validIds &= Q(.Id)
                                            End With
                                        Case "Owners"
                                            Dim newCrossReference As New KaOwnerInterfaceSettings
                                            With newCrossReference
                                                .Id = itemXrefId
                                                .CrossReference = crossReference
                                                .DefaultSetting = isDefault
                                                .Deleted = False
                                                .ExportOnly = exportOnly
                                                .InterfaceId = interfaceId
                                                .OwnerId = itemId
                                                .SqlUpdateInsertIfNotFound(connection, transaction, Database.ApplicationIdentifier, _currentUser.Name)
                                                If validIds.Length > 0 Then validIds &= ","
                                                validIds &= Q(.Id)
                                            End With
                                        Case "Products"
                                            Dim newCrossReference As New KaProductInterfaceSettings
                                            With newCrossReference
                                                .Id = itemXrefId
                                                .CrossReference = crossReference
                                                .Deleted = False
                                                .ExportOnly = exportOnly
                                                .InterfaceId = interfaceId
                                                .ProductId = itemId
                                                If interfaceType.ShowInterfaceExchangeUnit Then .UnitId = unitOfMeasureId
                                                If Not interfaceType.UseInterfaceUnitAsOrderItemUnit Then .OrderItemUnitId = orderItemUnitOfMeasureId
                                                If interfaceType.SplitProductIntoComponents Then .ProductSplitFormulationFacilityId = splitProductFacilityId
                                                .DefaultSetting = isDefault
                                                .ProductPriority = priority
                                                .SqlUpdateInsertIfNotFound(connection, transaction, Database.ApplicationIdentifier, _currentUser.Name)
                                                If validIds.Length > 0 Then validIds &= ","
                                                validIds &= Q(.Id)
                                            End With
                                        Case "Suppliers"
                                            Dim newCrossReference As New KaSupplierAccountInterfaceSettings
                                            With newCrossReference
                                                .CrossReference = crossReference
                                                .Deleted = False
                                                .ExportOnly = exportOnly
                                                .Id = itemXrefId
                                                .InterfaceId = interfaceId
                                                .SupplierAccountId = itemId
                                                .DefaultSetting = isDefault
                                                .SqlUpdateInsertIfNotFound(connection, transaction, Database.ApplicationIdentifier, _currentUser.Name)
                                                If validIds.Length > 0 Then validIds &= ","
                                                validIds &= Q(.Id)
                                            End With
                                        Case "Tanks"
                                            Dim newCrossReference As New KaTankInterfaceSettings
                                            With newCrossReference
                                                .CrossReference = crossReference
                                                .Deleted = False
                                                .ExportOnly = exportOnly
                                                .Id = itemXrefId
                                                .InterfaceId = interfaceId
                                                .DefaultSetting = isDefault
                                                .TankId = itemId
                                                .SqlUpdateInsertIfNotFound(connection, transaction, Database.ApplicationIdentifier, _currentUser.Name)
                                                If validIds.Length > 0 Then validIds &= ","
                                                validIds &= Q(.Id)
                                            End With
                                        Case "Transports"
                                            Dim newCrossReference As New KaTransportInterfaceSettings
                                            With newCrossReference
                                                .CrossReference = crossReference
                                                .Deleted = False
                                                .ExportOnly = exportOnly
                                                .Id = itemXrefId
                                                .InterfaceId = interfaceId
                                                .DefaultSetting = isDefault
                                                .TransportId = itemId
                                                .SqlUpdateInsertIfNotFound(connection, transaction, Database.ApplicationIdentifier, _currentUser.Name)
                                                If validIds.Length > 0 Then validIds &= ","
                                                validIds &= Q(.Id)
                                            End With
                                        Case "Transport Types"
                                            Dim newCrossReference As New KaTransportTypeInterfaceSettings
                                            With newCrossReference
                                                .CrossReference = crossReference
                                                .Deleted = False
                                                .ExportOnly = exportOnly
                                                .Id = itemXrefId
                                                .InterfaceId = interfaceId
                                                .DefaultSetting = isDefault
                                                .TransportTypeId = itemId
                                                .SqlUpdateInsertIfNotFound(connection, transaction, Database.ApplicationIdentifier, _currentUser.Name)
                                                If validIds.Length > 0 Then validIds &= ","
                                                validIds &= Q(.Id)
                                            End With
                                        Case "Units"
                                            Dim newCrossReference As New KaUnitInterfaceSettings
                                            With newCrossReference
                                                .CrossReference = crossReference
                                                .Deleted = False
                                                .ExportOnly = exportOnly
                                                .Id = itemXrefId
                                                .InterfaceId = interfaceId
                                                .DefaultSetting = isDefault
                                                .UnitId = itemId
                                                .SqlUpdateInsertIfNotFound(connection, transaction, Database.ApplicationIdentifier, _currentUser.Name)
                                                If validIds.Length > 0 Then validIds &= ","
                                                validIds &= Q(.Id)
                                            End With
                                    End Select
                                Next
                                Dim commandText As String = ""
                                Select Case _currentItemType
                                    Case "Applicators"
                                        commandText = "UPDATE [" & KaApplicatorInterfaceSettings.TABLE_NAME & "] " & _
                                             "SET [" & KaApplicatorInterfaceSettings.FN_DELETED & "] = 1 " & _
                                             "WHERE [" & KaApplicatorInterfaceSettings.FN_APPLICATOR_ID & "] = " & Q(itemId) & _
                                                 " AND [" & KaApplicatorInterfaceSettings.FN_INTERFACE_ID & "] =" & Q(interfaceId) & _
                                                 IIf(validIds.Length > 0, " AND NOT [" & KaApplicatorInterfaceSettings.FN_ID & "] in (" & validIds & ")", "")
                                    Case "Branches"
                                        commandText = "UPDATE [" & KaBranchInterfaceSettings.TABLE_NAME & "] SET [deleted] = 1 WHERE [" & KaBranchInterfaceSettings.FN_BRANCH_ID & "] = " & Q(itemId) & " AND [interface_id] =" & Q(interfaceId) & IIf(validIds.Length > 0, " AND NOT [id] in (" & validIds & ")", "")
                                    Case "Bulk Products"
                                        commandText = "UPDATE [bulk_product_interface_settings] SET [deleted] = 1 WHERE [bulk_product_id] = " & Q(itemId) & " AND [interface_id] =" & Q(interfaceId) & IIf(validIds.Length > 0, " AND NOT [id] in (" & validIds & ")", "")
                                    Case "Carriers"
                                        commandText = "UPDATE [" & KaCarrierInterfaceSettings.TABLE_NAME & "] " & _
                                            "SET [" & KaCarrierInterfaceSettings.FN_DELETED & "] = 1 " & _
                                            "WHERE [" & KaCarrierInterfaceSettings.FN_CARRIER_ID & "] = " & Q(itemId) & _
                                                " AND [" & KaCarrierInterfaceSettings.FN_INTERFACE_ID & "] =" & Q(interfaceId) & _
                                                IIf(validIds.Length > 0, " AND NOT [" & KaCarrierInterfaceSettings.FN_ID & "] in (" & validIds & ")", "")
                                    Case "Customer Account Destinations"
                                        commandText = "UPDATE [customer_account_location_interface_settings] SET [deleted] = 1 WHERE [customer_account_location_id] = " & Q(itemId) & " AND [interface_id] =" & Q(interfaceId) & IIf(validIds.Length > 0, " AND NOT [id] in (" & validIds & ")", "")
                                    Case "Customer Accounts"
                                        commandText = "UPDATE [customer_account_interface_settings] SET [deleted] = 1 WHERE [customer_account_id] = " & Q(itemId) & " AND [interface_id] =" & Q(interfaceId) & IIf(validIds.Length > 0, " AND NOT [id] in (" & validIds & ")", "")
                                    Case "Drivers"
                                        commandText = "UPDATE [" & KaDriverInterfaceSettings.TABLE_NAME & "] " & _
                                            "SET [" & KaDriverInterfaceSettings.FN_DELETED & "] = 1 " & _
                                            "WHERE [" & KaDriverInterfaceSettings.FN_DRIVER_ID & "] = " & Q(itemId) & _
                                                " AND [" & KaDriverInterfaceSettings.FN_INTERFACE_ID & "] =" & Q(interfaceId) & _
                                                IIf(validIds.Length > 0, " AND NOT [" & KaDriverInterfaceSettings.FN_ID & "] in (" & validIds & ")", "")
                                    Case "Facilities"
                                        commandText = "UPDATE [" & KaLocationInterfaceSettings.TABLE_NAME & "] " & _
                                             "SET [" & KaLocationInterfaceSettings.FN_DELETED & "] = 1 " & _
                                             "WHERE [" & KaLocationInterfaceSettings.FN_LOCATION_ID & "] = " & Q(itemId) & _
                                                 " AND [" & KaLocationInterfaceSettings.FN_INTERFACE_ID & "] =" & Q(interfaceId) & _
                                                 IIf(validIds.Length > 0, " AND NOT [" & KaLocationInterfaceSettings.FN_ID & "] in (" & validIds & ")", "")
                                    Case "Owners"
                                        commandText = "UPDATE [" & KaOwnerInterfaceSettings.TABLE_NAME & "] " & _
                                             "SET [" & KaOwnerInterfaceSettings.FN_DELETED & "] = 1 " & _
                                             "WHERE [" & KaOwnerInterfaceSettings.FN_OWNER_ID & "] = " & Q(itemId) & _
                                                 " AND [" & KaOwnerInterfaceSettings.FN_INTERFACE_ID & "] =" & Q(interfaceId) & _
                                                 IIf(validIds.Length > 0, " AND NOT [" & KaOwnerInterfaceSettings.FN_ID & "] in (" & validIds & ")", "")
                                    Case "Products"
                                        commandText = "UPDATE [product_interface_settings] SET [deleted] = 1 WHERE product_id = " & Q(itemId) & " AND [interface_id] =" & Q(interfaceId) & IIf(validIds.Length > 0, " AND NOT [id] in (" & validIds & ")", "")
                                    Case "Suppliers"
                                        commandText = "UPDATE [" & KaSupplierAccountInterfaceSettings.TABLE_NAME & "] " & _
                                            "SET [" & KaSupplierAccountInterfaceSettings.FN_DELETED & "] = 1 " & _
                                            "WHERE [" & KaSupplierAccountInterfaceSettings.FN_SUPPLIER_ACCOUNT_ID & "] = " & Q(itemId) & _
                                                " AND [" & KaSupplierAccountInterfaceSettings.FN_INTERFACE_ID & "] =" & Q(interfaceId) & _
                                                IIf(validIds.Length > 0, " AND NOT [" & KaSupplierAccountInterfaceSettings.FN_ID & "] in (" & validIds & ")", "")
                                    Case "Tanks"
                                        commandText = "UPDATE [" & KaTankInterfaceSettings.TABLE_NAME & "] " & _
                                            "SET [" & KaTankInterfaceSettings.FN_DELETED & "] = 1 " & _
                                            "WHERE [" & KaTankInterfaceSettings.FN_TANK_ID & "] = " & Q(itemId) & _
                                                " AND [" & KaTankInterfaceSettings.FN_INTERFACE_ID & "] =" & Q(interfaceId) & _
                                                IIf(validIds.Length > 0, " AND NOT [" & KaTankInterfaceSettings.FN_ID & "] in (" & validIds & ")", "")
                                    Case "Transports"
                                        commandText = "UPDATE [" & KaTransportInterfaceSettings.TABLE_NAME & "] " & _
                                            "SET [" & KaTransportInterfaceSettings.FN_DELETED & "] = 1 " & _
                                            "WHERE [" & KaTransportInterfaceSettings.FN_TRANSPORT_ID & "] = " & Q(itemId) & _
                                                " AND [" & KaTransportInterfaceSettings.FN_INTERFACE_ID & "] =" & Q(interfaceId) & _
                                                IIf(validIds.Length > 0, " AND NOT [" & KaTransportInterfaceSettings.FN_ID & "] in (" & validIds & ")", "")
                                    Case "Transport Types"
                                        commandText = "UPDATE [" & KaTransportTypeInterfaceSettings.TABLE_NAME & "] " & _
                                            "SET [" & KaTransportTypeInterfaceSettings.FN_DELETED & "] = 1 " & _
                                            "WHERE [" & KaTransportTypeInterfaceSettings.FN_TRANSPORT_TYPE_ID & "] = " & Q(itemId) & _
                                                " AND [" & KaTransportTypeInterfaceSettings.FN_INTERFACE_ID & "] =" & Q(interfaceId) & _
                                                IIf(validIds.Length > 0, " AND NOT [" & KaTransportTypeInterfaceSettings.FN_ID & "] in (" & validIds & ")", "")
                                    Case "Units"
                                        commandText = "UPDATE [" & KaUnitInterfaceSettings.TABLE_NAME & "] " & _
                                            "SET [" & KaUnitInterfaceSettings.FN_DELETED & "] = 1 " & _
                                            "WHERE [" & KaUnitInterfaceSettings.FN_UNIT_ID & "] = " & Q(itemId) & _
                                                " AND [" & KaUnitInterfaceSettings.FN_INTERFACE_ID & "] =" & Q(interfaceId) & _
                                                IIf(validIds.Length > 0, " AND NOT [" & KaUnitInterfaceSettings.FN_ID & "] in (" & validIds & ")", "")
                                End Select
                                Tm2Database.ExecuteNonQuery(connection, transaction, commandText)
                            End With
                        End If
                    End If
                Next
                transaction.Commit()
            Catch ex As Exception
                If transaction IsNot Nothing Then transaction.Rollback()
                Throw ex
            Finally
                If transaction IsNot Nothing Then transaction.Dispose()
                connection.Close()
            End Try
            ddlInterface2_SelectedIndexChanged(ddlInterface2, New EventArgs)
            lblStatus.Text = "Interface values saved successfully"
        End If
    End Sub

    Private Function ValidateSettings(ByRef interfaceId As Guid, ByRef interfaceType As KaInterfaceTypes) As Boolean
        If Guid.TryParse(ddlInterface2.SelectedValue, interfaceId) AndAlso Not interfaceId.Equals(Guid.Empty) Then
            Try
                interfaceType = New KaInterfaceTypes(GetUserConnection(_currentUser.Id), New KaInterface(GetUserConnection(_currentUser.Id), interfaceId).InterfaceTypeId)
            Catch ex As RecordNotFoundException
                interfaceType = New KaInterfaceTypes()
            End Try

            ' Verify that we are only using the cross reference once
            Dim crossReferenceUsedList As New Dictionary(Of String, Guid)
            Dim itemIdsShown As New List(Of Guid)

            For rowCounter As Integer = 2 To tblInterfaceSetting.Rows.Count - 2
                Dim currentItemRow As HtmlTableRow = tblInterfaceSetting.Rows(rowCounter)
                Dim itemId As Guid = Guid.Empty

                If Guid.TryParse(CType(currentItemRow.FindControl(currentItemRow.ID & "Id"), HtmlTableCell).InnerText, itemId) AndAlso _
                        Not itemId.Equals(Guid.Empty) AndAlso Not itemIdsShown.Contains(itemId) Then itemIdsShown.Add(itemId)

                Dim interface2Cell As HtmlTableCell = currentItemRow.FindControl(currentItemRow.ID & "Interface2Value")
                If interface2Cell.Controls.Count > 0 Then
                    With CType(interface2Cell.Controls(0), HtmlGenericControl)
                        Dim validIds As String = ""
                        For Each interfaceRow As HtmlGenericControl In .Controls
                            If currentItemRow.FindControl(interfaceRow.ID & "Interface2Value") Is Nothing Then Continue For
                            Dim exportOnly As Boolean = CType(interfaceRow.FindControl(interfaceRow.ID & "Interface2ExportOnly"), CheckBox).Checked
                            Dim crossReference As String = CType(interfaceRow.FindControl(interfaceRow.ID & "Interface2Value"), TextBox).Text.Trim
                            If crossReference.Length > 0 Then
                                If crossReferenceUsedList.ContainsKey(crossReference.ToLower) AndAlso Not exportOnly Then
                                    ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidCrossReferenceExists", Utilities.JsAlert("The cross references for this interface must be unique (" & crossReference & ")."))
                                    SetEnabledStatusForColumns()
                                    SetFocus(interfaceRow.ID & "Interface2Value")
                                    Return False
                                ElseIf Not exportOnly Then
                                    crossReferenceUsedList.Add(crossReference.ToLower, itemId) ' Add this to the check list
                                End If

                                If interfaceType.ShowInterfaceExchangeUnit AndAlso ((interfaceType.ShowBulkProductInterface AndAlso _currentItemType = "Bulk Products") OrElse _currentItemType = "Products") Then
                                    'Ensure that the unit of measure is set for both Products and Bulk Products
                                    Dim unitOfMeasureId As Guid = Guid.Empty
                                    If interfaceRow.FindControl(interfaceRow.ID & "Interface2UnitOfMeasure") IsNot Nothing Then
                                        Guid.TryParse(CType(interfaceRow.FindControl(interfaceRow.ID & "Interface2UnitOfMeasure"), DropDownList).SelectedValue, unitOfMeasureId)
                                    End If
                                    If unitOfMeasureId = Guid.Empty Then
                                        ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidNoUnit", Utilities.JsAlert("No unit of measure selected (" & crossReference & ")."))
                                        SetEnabledStatusForColumns()
                                        Return False
                                    End If
                                End If

                                If Not interfaceType.UseInterfaceUnitAsOrderItemUnit AndAlso _currentItemType = "Products" Then
                                    Dim orderItemUnitOfMeasureId As Guid = Guid.Empty
                                    If interfaceRow.FindControl(interfaceRow.ID & "Interface2OrderItemUnitOfMeasure") IsNot Nothing Then
                                        Guid.TryParse(CType(interfaceRow.FindControl(interfaceRow.ID & "Interface2OrderItemUnitOfMeasure"), DropDownList).SelectedValue, orderItemUnitOfMeasureId)
                                    End If
                                    If orderItemUnitOfMeasureId = Guid.Empty Then
                                        ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidNoOrderItemUnit", Utilities.JsAlert("No order item unit of measure selected (" & crossReference & ")."))
                                        SetEnabledStatusForColumns()
                                        Return False
                                    End If

                                    If Not IsNumeric(CType(interfaceRow.FindControl(interfaceRow.ID & "Priority2Value"), TextBox).Text.Trim) Then
                                        ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidProductSortPriority", Utilities.JsAlert("Product order item sort priority must be a number, default is 100 (" & crossReference & ")."))
                                        SetEnabledStatusForColumns()
                                        Return False
                                    End If
                                End If
                            End If
                        Next
                    End With
                End If
            Next
            If itemIdsShown.Count > 0 AndAlso crossReferenceUsedList.Count > 0 Then
                ' Check to make sure that the cross reference isn't used on a different page
                Dim itemIdsToIgnore As String = ""
                For Each itemId As Guid In itemIdsShown
                    If itemIdsToIgnore.Length > 0 Then itemIdsToIgnore &= ","
                    itemIdsToIgnore &= Q(itemId)
                Next

                Dim crossReferencesToSearch As String = ""
                For Each crossReferenceUsed As String In crossReferenceUsedList.Keys
                    If crossReferencesToSearch.Length > 0 Then crossReferencesToSearch &= ","
                    crossReferencesToSearch &= Q(crossReferenceUsed)
                Next

                Dim commandText As String = ""
                Select Case _currentItemType
                    Case "Applicators"
                        commandText = "SELECT [" & KaApplicatorInterfaceSettings.FN_CROSS_REFERENCE & "] " & _
                            "FROM [" & KaApplicatorInterfaceSettings.TABLE_NAME & "] " & _
                            "WHERE [" & KaApplicatorInterfaceSettings.FN_INTERFACE_ID & "] =" & Q(interfaceId) & _
                            "AND [" & KaApplicatorInterfaceSettings.FN_DELETED & "] = 0 " & _
                            "AND NOT [" & KaApplicatorInterfaceSettings.FN_APPLICATOR_ID & "] IN (" & itemIdsToIgnore & ") " & _
                            "AND [" & KaApplicatorInterfaceSettings.FN_CROSS_REFERENCE & "] IN (" & crossReferencesToSearch & ")" & _
                            "AND [" & KaApplicatorInterfaceSettings.FN_EXPORT_ONLY & "] = 0"
                    Case "Branches"
                        commandText = "SELECT [" & KaBranchInterfaceSettings.FN_CROSS_REFERENCE & "] " & _
                              "FROM [" & KaBranchInterfaceSettings.TABLE_NAME & "] " & _
                              "WHERE [" & KaBranchInterfaceSettings.FN_INTERFACE_ID & "] =" & Q(interfaceId) & _
                              "AND [" & KaBranchInterfaceSettings.FN_DELETED & "] = 0 " & _
                              "AND NOT [" & KaBranchInterfaceSettings.FN_BRANCH_ID & "] IN (" & itemIdsToIgnore & ") " & _
                              "AND [" & KaBranchInterfaceSettings.FN_CROSS_REFERENCE & "] IN (" & crossReferencesToSearch & ")" & _
                              "AND [" & KaBranchInterfaceSettings.FN_EXPORT_ONLY & "] = 0"
                    Case "Bulk Products"
                        commandText = "SELECT [" & KaBulkProductInterfaceSettings.FN_CROSS_REFERENCE & "] " & _
                            "FROM [" & KaBulkProductInterfaceSettings.TABLE_NAME & "] " & _
                            "WHERE [" & KaBulkProductInterfaceSettings.FN_INTERFACE_ID & "] =" & Q(interfaceId) & _
                            "AND [" & KaBulkProductInterfaceSettings.FN_DELETED & "] = 0 " & _
                            "AND NOT [" & KaBulkProductInterfaceSettings.FN_BULK_PRODUCT_ID & "] IN (" & itemIdsToIgnore & ") " & _
                            "AND [" & KaBulkProductInterfaceSettings.FN_CROSS_REFERENCE & "] IN (" & crossReferencesToSearch & ")" & _
                            "AND [" & KaBulkProductInterfaceSettings.FN_EXPORT_ONLY & "] = 0"
                    Case "Carriers"
                        commandText = "SELECT [" & KaCarrierInterfaceSettings.FN_CROSS_REFERENCE & "] " & _
                             "FROM [" & KaCarrierInterfaceSettings.TABLE_NAME & "] " & _
                             "WHERE [" & KaCarrierInterfaceSettings.FN_INTERFACE_ID & "] =" & Q(interfaceId) & _
                             "AND [" & KaCarrierInterfaceSettings.FN_DELETED & "] = 0 " & _
                             "AND NOT [" & KaCarrierInterfaceSettings.FN_CARRIER_ID & "] IN (" & itemIdsToIgnore & ") " & _
                             "AND [" & KaCarrierInterfaceSettings.FN_CROSS_REFERENCE & "] IN (" & crossReferencesToSearch & ")" & _
                             "AND [" & KaCarrierInterfaceSettings.FN_EXPORT_ONLY & "] = 0"
                    Case "Customer Account Destinations"
                        commandText = "SELECT [" & KaCustomerAccountLocationInterfaceSettings.FN_CROSS_REFERENCE & "] " & _
                             "FROM [" & KaCustomerAccountLocationInterfaceSettings.TABLE_NAME & "] " & _
                             "WHERE [" & KaCustomerAccountLocationInterfaceSettings.FN_INTERFACE_ID & "] =" & Q(interfaceId) & _
                             "AND [" & KaCustomerAccountLocationInterfaceSettings.FN_DELETED & "] = 0 " & _
                             "AND NOT [" & KaCustomerAccountLocationInterfaceSettings.FN_CUSTOMER_ACCOUNT_LOCATION_ID & "] IN (" & itemIdsToIgnore & ") " & _
                             "AND [" & KaCustomerAccountLocationInterfaceSettings.FN_CROSS_REFERENCE & "] IN (" & crossReferencesToSearch & ")" & _
                             "AND [" & KaCustomerAccountLocationInterfaceSettings.FN_EXPORT_ONLY & "] = 0"
                    Case "Customer Accounts"
                        commandText = "SELECT [" & KaCustomerAccountInterfaceSettings.FN_CROSS_REFERENCE & "] " & _
                          "FROM [" & KaCustomerAccountInterfaceSettings.TABLE_NAME & "] " & _
                          "WHERE [" & KaCustomerAccountInterfaceSettings.FN_INTERFACE_ID & "] =" & Q(interfaceId) & _
                          "AND [" & KaCustomerAccountInterfaceSettings.FN_DELETED & "] = 0 " & _
                          "AND NOT [" & KaCustomerAccountInterfaceSettings.FN_CUSTOMER_ACCOUNT_ID & "] IN (" & itemIdsToIgnore & ") " & _
                          "AND [" & KaCustomerAccountInterfaceSettings.FN_CROSS_REFERENCE & "] IN (" & crossReferencesToSearch & ")" & _
                          "AND [" & KaCustomerAccountInterfaceSettings.FN_EXPORT_ONLY & "] = 0"
                    Case "Drivers"
                        commandText = "SELECT [" & KaDriverInterfaceSettings.FN_CROSS_REFERENCE & "] " & _
                            "FROM [" & KaDriverInterfaceSettings.TABLE_NAME & "] " & _
                            "WHERE [" & KaDriverInterfaceSettings.FN_INTERFACE_ID & "] =" & Q(interfaceId) & _
                            "AND [" & KaDriverInterfaceSettings.FN_DELETED & "] = 0 " & _
                            "AND NOT [" & KaDriverInterfaceSettings.FN_DRIVER_ID & "] IN (" & itemIdsToIgnore & ") " & _
                            "AND [" & KaDriverInterfaceSettings.FN_CROSS_REFERENCE & "] IN (" & crossReferencesToSearch & ")" & _
                            "AND [" & KaDriverInterfaceSettings.FN_EXPORT_ONLY & "] = 0"
                    Case "Facilities"
                        commandText = "SELECT [" & KaLocationInterfaceSettings.FN_CROSS_REFERENCE & "] " & _
                            "FROM [" & KaLocationInterfaceSettings.TABLE_NAME & "] " & _
                            "WHERE [" & KaLocationInterfaceSettings.FN_INTERFACE_ID & "] =" & Q(interfaceId) & _
                            "AND [" & KaLocationInterfaceSettings.FN_DELETED & "] = 0 " & _
                            "AND NOT [" & KaLocationInterfaceSettings.FN_LOCATION_ID & "] IN (" & itemIdsToIgnore & ") " & _
                            "AND [" & KaLocationInterfaceSettings.FN_CROSS_REFERENCE & "] IN (" & crossReferencesToSearch & ")" & _
                            "AND [" & KaLocationInterfaceSettings.FN_EXPORT_ONLY & "] = 0"
                    Case "Owners"
                        commandText = "SELECT [" & KaOwnerInterfaceSettings.FN_CROSS_REFERENCE & "] " & _
                            "FROM [" & KaOwnerInterfaceSettings.TABLE_NAME & "] " & _
                            "WHERE [" & KaOwnerInterfaceSettings.FN_INTERFACE_ID & "] =" & Q(interfaceId) & _
                            "AND [" & KaOwnerInterfaceSettings.FN_DELETED & "] = 0 " & _
                            "AND NOT [" & KaOwnerInterfaceSettings.FN_OWNER_ID & "] IN (" & itemIdsToIgnore & ") " & _
                            "AND [" & KaOwnerInterfaceSettings.FN_CROSS_REFERENCE & "] IN (" & crossReferencesToSearch & ")" & _
                            "AND [" & KaOwnerInterfaceSettings.FN_EXPORT_ONLY & "] = 0"
                    Case "Products"
                        commandText = "SELECT [" & KaProductInterfaceSettings.FN_CROSS_REFERENCE & "] " & _
                            "FROM [" & KaProductInterfaceSettings.TABLE_NAME & "] " & _
                            "WHERE [" & KaProductInterfaceSettings.FN_INTERFACE_ID & "] =" & Q(interfaceId) & _
                            "AND [" & KaProductInterfaceSettings.FN_DELETED & "] = 0 " & _
                            "AND NOT [" & KaProductInterfaceSettings.FN_PRODUCT_ID & "] IN (" & itemIdsToIgnore & ") " & _
                            "AND [" & KaProductInterfaceSettings.FN_CROSS_REFERENCE & "] IN (" & crossReferencesToSearch & ")" & _
                            "AND [" & KaProductInterfaceSettings.FN_EXPORT_ONLY & "] = 0"
                    Case "Suppliers"
                        commandText = "SELECT [" & KaSupplierAccountInterfaceSettings.FN_CROSS_REFERENCE & "] " & _
                            "FROM [" & KaSupplierAccountInterfaceSettings.TABLE_NAME & "] " & _
                            "WHERE [" & KaSupplierAccountInterfaceSettings.FN_INTERFACE_ID & "] =" & Q(interfaceId) & _
                            "AND [" & KaSupplierAccountInterfaceSettings.FN_DELETED & "] = 0 " & _
                            "AND NOT [" & KaSupplierAccountInterfaceSettings.FN_SUPPLIER_ACCOUNT_ID & "] IN (" & itemIdsToIgnore & ") " & _
                            "AND [" & KaSupplierAccountInterfaceSettings.FN_CROSS_REFERENCE & "] IN (" & crossReferencesToSearch & ")" & _
                            "AND [" & KaSupplierAccountInterfaceSettings.FN_EXPORT_ONLY & "] = 0"
                    Case "Tanks"
                        commandText = "SELECT [" & KaTankInterfaceSettings.FN_CROSS_REFERENCE & "] " & _
                            "FROM [" & KaTankInterfaceSettings.TABLE_NAME & "] " & _
                            "WHERE [" & KaTankInterfaceSettings.FN_INTERFACE_ID & "] =" & Q(interfaceId) & _
                            "AND [" & KaTankInterfaceSettings.FN_DELETED & "] = 0 " & _
                            "AND NOT [" & KaTankInterfaceSettings.FN_TANK_ID & "] IN (" & itemIdsToIgnore & ") " & _
                            "AND [" & KaTankInterfaceSettings.FN_CROSS_REFERENCE & "] IN (" & crossReferencesToSearch & ")" & _
                            "AND [" & KaTankInterfaceSettings.FN_EXPORT_ONLY & "] = 0"
                    Case "Transports"
                        commandText = "SELECT [" & KaTransportInterfaceSettings.FN_CROSS_REFERENCE & "] " & _
                            "FROM [" & KaTransportInterfaceSettings.TABLE_NAME & "] " & _
                            "WHERE [" & KaTransportInterfaceSettings.FN_INTERFACE_ID & "] =" & Q(interfaceId) & _
                            "AND [" & KaTransportInterfaceSettings.FN_DELETED & "] = 0 " & _
                            "AND NOT [" & KaTransportInterfaceSettings.FN_TRANSPORT_ID & "] IN (" & itemIdsToIgnore & ") " & _
                            "AND [" & KaTransportInterfaceSettings.FN_CROSS_REFERENCE & "] IN (" & crossReferencesToSearch & ")" & _
                            "AND [" & KaTransportInterfaceSettings.FN_EXPORT_ONLY & "] = 0"
                    Case "Transport Types"
                        commandText = "SELECT [" & KaTransportTypeInterfaceSettings.FN_CROSS_REFERENCE & "] " & _
                            "FROM [" & KaTransportTypeInterfaceSettings.TABLE_NAME & "] " & _
                            "WHERE [" & KaTransportTypeInterfaceSettings.FN_INTERFACE_ID & "] =" & Q(interfaceId) & _
                            "AND [" & KaTransportTypeInterfaceSettings.FN_DELETED & "] = 0 " & _
                            "AND NOT [" & KaTransportTypeInterfaceSettings.FN_TRANSPORT_TYPE_ID & "] IN (" & itemIdsToIgnore & ") " & _
                            "AND [" & KaTransportTypeInterfaceSettings.FN_CROSS_REFERENCE & "] IN (" & crossReferencesToSearch & ")" & _
                            "AND [" & KaTransportTypeInterfaceSettings.FN_EXPORT_ONLY & "] = 0"
                    Case "Units"
                        commandText = "SELECT [" & KaUnitInterfaceSettings.FN_CROSS_REFERENCE & "] " & _
                            "FROM [" & KaUnitInterfaceSettings.TABLE_NAME & "] " & _
                            "WHERE [" & KaUnitInterfaceSettings.FN_INTERFACE_ID & "] =" & Q(interfaceId) & _
                            "AND [" & KaUnitInterfaceSettings.FN_DELETED & "] = 0 " & _
                            "AND NOT [" & KaUnitInterfaceSettings.FN_UNIT_ID & "] IN (" & itemIdsToIgnore & ") " & _
                            "AND [" & KaUnitInterfaceSettings.FN_CROSS_REFERENCE & "] IN (" & crossReferencesToSearch & ")" & _
                            "AND [" & KaUnitInterfaceSettings.FN_EXPORT_ONLY & "] = 0"

                End Select
                Dim itemXRefRdr As OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), commandText)
                If itemXRefRdr.Read() Then
                    ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidCrossReferenceExists", Utilities.JsAlert("The cross references for this interface must be unique (" & itemXRefRdr.Item(0).ToString & ")."))
                    SetEnabledStatusForColumns()
                    SetFocus(crossReferenceUsedList(itemXRefRdr.Item(0).ToString.ToLower).ToString & "Interface2Value")
                    Return False
                End If
                itemXRefRdr.Close()
            End If
        Else
            Return False
        End If
        Return True
    End Function
#End Region

    <Serializable()> _
    Public Class ItemCrossReference
        Public ItemId As String = Guid.Empty.ToString
        Public ItemName As String = ""
        Public Interface1 As New List(Of CrossReference)
        Public Interface2 As New List(Of CrossReference)
        Public Sub New()
        End Sub
        Public Sub New(ByVal id As String, ByVal name As String, ByVal int1 As List(Of CrossReference), ByVal int2 As List(Of CrossReference))
            ItemId = id
            ItemName = name
            Interface1 = int1
            Interface2 = int2
        End Sub

        <Serializable()> _
        Public Class CrossReference
            Public Id As String = Guid.Empty.ToString
            Public Text As String = ""
            Public UofM As String = Guid.Empty.ToString
            Public OrderItemUofM As String = Guid.Empty.ToString
            Public IsDefaultSetting As Boolean = False
            Public ProductPriority As Double = 100
            Public ExportOnly As Boolean = False
            Public SplitProductFacilityId As String = Guid.Empty.ToString

            Public Sub New()
            End Sub

            Public Sub New(ByVal interfaceId As String, ByVal interfaceValue As String, ByVal interfaceUofM As String, ByVal interfaceOrderItemUofM As String, ByVal isDefault As Boolean, ByVal priority As Double, ByVal export As Boolean, ByVal splitProductLocationId As String)
                Id = interfaceId
                Text = interfaceValue
                UofM = interfaceUofM
                OrderItemUofM = interfaceOrderItemUofM
                IsDefaultSetting = isDefault
                ProductPriority = priority
                ExportOnly = export
                SplitProductFacilityId = splitProductLocationId
            End Sub
        End Class
    End Class

    Protected Sub ddlItemsPerPage_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlItemsPerPage.SelectedIndexChanged
        ddlPageNumber.SelectedIndex = 0
        PopulateItems()
    End Sub

    Private Sub ddlPageNumber_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles ddlPageNumber.SelectedIndexChanged
        PopulateItems()
    End Sub

    Private Sub btnPreviousPage_Click(sender As Object, e As System.EventArgs) Handles btnPreviousPage.Click
        ddlPageNumber.SelectedIndex = Math.Max(0, ddlPageNumber.SelectedIndex - 1)
        PopulateItems()
    End Sub

    Private Sub btnNextPage_Click(sender As Object, e As System.EventArgs) Handles btnNextPage.Click
        ddlPageNumber.SelectedIndex = Math.Min(ddlPageNumber.Items.Count - 1, ddlPageNumber.SelectedIndex + 1)
        PopulateItems()
    End Sub

    Protected Overrides Function SaveViewState() As Object
        Dim viewState(4) As Object
        'Saving the grid values to the View State
        Dim crossreferenceList As New List(Of ItemCrossReference)
        For rowCounter As Integer = 2 To tblInterfaceSetting.Rows.Count - 2
            Dim currentItemRow As HtmlTableRow = tblInterfaceSetting.Rows(rowCounter)
            Dim interface1List As New List(Of ItemCrossReference.CrossReference)
            Dim interface1Cell As HtmlTableCell = currentItemRow.FindControl(currentItemRow.ID & "Interface1Value")
            If interface1Cell IsNot Nothing AndAlso interface1Cell.Controls.Count > 0 Then
                With CType(interface1Cell.Controls(0), HtmlGenericControl)
                    For Each interfaceRow As HtmlGenericControl In .Controls
                        If currentItemRow.FindControl(interfaceRow.ID & "Interface1Value") Is Nothing Then Continue For
                        Dim crossReference As String = CType(currentItemRow.FindControl(interfaceRow.ID & "Interface1Value"), Label).Text
                        If crossReference.Length > 0 Then
                            interface1List.Add(New ItemCrossReference.CrossReference("NewRow" & Guid.NewGuid.ToString, _
                                CType(currentItemRow.FindControl(interfaceRow.ID & "Interface1Value"), Label).Text, _
                                CType(currentItemRow.FindControl(interfaceRow.ID & "Interface1UnitOfMeasure"), Label).Text.Replace("<br />", "").Trim, _
                                CType(currentItemRow.FindControl(interfaceRow.ID & "Interface1OrderItemUnitOfMeasure"), Label).Text.Replace("<br />", "").Trim, _
                                Boolean.Parse(CType(currentItemRow.FindControl(interfaceRow.ID & "Interface1IsDefault"), Label).Text.Replace("<br />", "").Trim), _
                                CType(currentItemRow.FindControl(interfaceRow.ID & "Priority1Value"), Label).Text.Replace("<br />", "").Trim, _
                                Boolean.Parse(CType(currentItemRow.FindControl(interfaceRow.ID & "Interface1ExportOnly"), Label).Text.Replace("<br />", "").Trim), _
                                CType(currentItemRow.FindControl(interfaceRow.ID & "Interface1SplitProductFacilityId"), Label).Text.Replace("<br />", "").Trim))
                        End If
                    Next
                End With
            End If
            Dim interface2List As New List(Of ItemCrossReference.CrossReference)
            Dim interface2Cell As HtmlTableCell = currentItemRow.FindControl(currentItemRow.ID & "Interface2Value")
            If interface2Cell IsNot Nothing AndAlso interface2Cell.Controls.Count > 0 Then
                With CType(interface2Cell.Controls(0), HtmlGenericControl)
                    For Each interfaceRow As HtmlGenericControl In .Controls
                        If currentItemRow.FindControl(interfaceRow.ID & "Interface2Value") Is Nothing Then Continue For
                        Dim crossReference As String = CType(currentItemRow.FindControl(interfaceRow.ID & "Interface2Value"), TextBox).Text
                        Dim priority As String = CType(currentItemRow.FindControl(interfaceRow.ID & "Priority2Value"), TextBox).Text
                        interface2List.Add(New ItemCrossReference.CrossReference(interfaceRow.ID, crossReference, _
                            CType(currentItemRow.FindControl(interfaceRow.ID & "Interface2UnitOfMeasure"), DropDownList).SelectedValue, _
                            CType(currentItemRow.FindControl(interfaceRow.ID & "Interface2OrderItemUnitOfMeasure"), DropDownList).SelectedValue, _
                            CType(currentItemRow.FindControl(interfaceRow.ID & "Interface2IsDefault"), CheckBox).Checked, _
                            priority, _
                            CType(currentItemRow.FindControl(interfaceRow.ID & "Interface2ExportOnly"), CheckBox).Checked, _
                            CType(currentItemRow.FindControl(interfaceRow.ID & "Interface2SplitProductFacilityId"), DropDownList).SelectedValue))
                    Next
                End With
            End If

            crossreferenceList.Add(New ItemCrossReference(CType(currentItemRow.FindControl(currentItemRow.ID & "Id"), HtmlTableCell).InnerText, _
                                                          CType(currentItemRow.FindControl(currentItemRow.ID & "Name"), HtmlTableCell).InnerText, _
                                                          interface1List, _
                                                          interface2List))
        Next
        viewState(0) = crossreferenceList
        viewState(1) = _currentItemType
        viewState(2) = MyBase.SaveViewState()
        _selectedInterface1Value = Guid.Parse(ddlInterface1.SelectedValue)
        viewState(3) = _selectedInterface1Value
        _selectedInterface2Value = Guid.Parse(ddlInterface2.SelectedValue)
        viewState(4) = _selectedInterface2Value
        Return viewState
    End Function

    Protected Overrides Sub LoadViewState(savedState As Object)
        'Getting the dropdown list value from view state.
        If savedState IsNot Nothing AndAlso CType(savedState, Object).Length = 5 Then
            _currentUser = Utilities.GetUser(Me)
            Dim viewState As Object() = savedState
            _currentItemType = viewState(1)
            _selectedInterface1Value = viewState(3)
            _selectedInterface2Value = viewState(4)
            Dim interfaceType1 As KaInterfaceTypes
            Try
                interfaceType1 = New KaInterfaceTypes(Tm2Database.Connection, New KaInterface(Tm2Database.Connection, _selectedInterface1Value).InterfaceTypeId)
            Catch ex As RecordNotFoundException
                interfaceType1 = New KaInterfaceTypes()
            End Try
            Dim interfaceType2 As KaInterfaceTypes
            Try
                interfaceType2 = New KaInterfaceTypes(Tm2Database.Connection, New KaInterface(Tm2Database.Connection, _selectedInterface2Value).InterfaceTypeId)
            Catch ex As RecordNotFoundException
                interfaceType2 = New KaInterfaceTypes()
            End Try

            For Each interfaceSetting As ItemCrossReference In viewState(0)
                AddInterfaceRow(interfaceSetting, interfaceType1, interfaceType2)
            Next
            MyBase.LoadViewState(viewState(2))
            SetEnabledStatusForColumns()
        Else
            MyBase.LoadViewState(savedState)
        End If
    End Sub
End Class