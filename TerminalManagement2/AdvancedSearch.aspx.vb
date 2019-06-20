Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class AdvancedSearch
    Inherits System.Web.UI.Page
    Private _currentUser As KaUser
    Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
    Private _currentTableName As String
    Private _searchType As String = ""
    Private _sourceTitle As String = ""

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Response.Cache.SetCacheability(HttpCacheability.NoCache)
        _currentUser = Utilities.GetUser(Me)
        ' Dim currentUserPermission As String = ""
        If Page.Request("SearchType") IsNot Nothing Then
            _searchType = Page.Request("SearchType").ToLower()
        End If
        If Page.Request("SourceTitle") IsNot Nothing Then
            _sourceTitle = System.Uri.UnescapeDataString(Page.Request("SourceTitle")).ToLower()
        End If
        If _searchType = "orders" Then
            _currentTableName = KaOrder.TABLE_NAME
            _currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Orders")
            Me.Title = "Orders : Orders"
        ElseIf _searchType = "pastorders" Then
            _currentTableName = KaOrder.TABLE_NAME
            _currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Orders")
            Me.Title = "Orders : Past Orders"
        ElseIf _searchType = "tickets" Then
            _currentTableName = "reports"
            _currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Reports")
            Me.Title = "Reports : Receipts"
        ElseIf _searchType = "interfaceorders" Then
            _currentTableName = KaOrder.TABLE_NAME
            _currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Orders")
            Dim _currentTableNameInterface = KaInterface.TABLE_NAME
            Dim _currentUserPermissionInterface = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableNameInterface}), "Interfaces")
            If Not _currentUserPermission(_currentTableName).Read OrElse Not _currentUserPermissionInterface(_currentTableNameInterface).Read Then Response.Redirect("Welcome.aspx")
            Me.Title = "Interfaces : Assign Interface to Orders"
        ElseIf _searchType = "receiving" Then
            _currentTableName = KaReceivingPurchaseOrder.TABLE_NAME
            _currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "PurchaseOrders")
            Me.Title = "Purchase Orders : Receiving Purchase Orders"
        ElseIf _searchType = "pastreceiving" Then
            _currentTableName = KaReceivingPurchaseOrder.TABLE_NAME
            _currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "PurchaseOrders")
            Me.Title = "Purchase Orders : Past Receiving Purchase Orders"
        ElseIf _searchType = "searchall" Then
            _currentTableName = _searchType
            'currentUserPermission = "V" 'TODO: lost functionality? is searchall even used?
            _currentUserPermission = New Dictionary(Of String, KaTablePermission)
            _currentUserPermission.Add(_currentTableName, New KaTablePermission())
            _currentUserPermission(_currentTableName).Read = True
            Me.Title = "Welcome : Welcome"
        End If

        If _currentUserPermission Is Nothing OrElse Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")
        If Not Page.IsPostBack Then
            Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
            PopulateCustomersList(connection)
            PopulateSuppliersList(connection)
            PopulateOwnersList(connection)
            PopulateBranchesList(connection)
            PopulateProductsList(connection)
            PopulateBulkProductsList(connection)
            PopulateSortList()
            PopulateAscDescList()
            tbxFind.Text = ""
            If _searchType = "tickets" Then
                pnlCustomerFilter.Visible = True
                pnlSupplierFilter.Visible = False
                pnlOwnerFilter.Visible = True
                pnlBranchFilter.Visible = True
                pnlProductFilter.Visible = True
                pnlBulkProductFilter.Visible = False
                pnlActivityReportColumns.Visible = True
                pnlFromDate.Visible = True
                pnlToDate.Visible = True
                pnlSortBy.Visible = True
                pnlIncludeInterfaceCrossReferences.Visible = False

                Dim col As Integer = 0
                Integer.TryParse(KaSetting.GetSetting(connection, "AdvancedSearch:" & _currentUser.Id.ToString & "/CheckedOptions", "15"), col)
                cbxDateTime.Checked = ((col And 2 ^ KaReports.CustomerActivityReportColumns.RcDateTime) <> 0)
                cbxOrderNumber.Checked = ((col And 2 ^ KaReports.CustomerActivityReportColumns.RcOrderNumber) <> 0)
                cbxTicketNumber.Checked = ((col And 2 ^ KaReports.CustomerActivityReportColumns.RcTicketNumber) <> 0)
                cbxCustomer.Checked = ((col And 2 ^ KaReports.CustomerActivityReportColumns.RcCustomer) <> 0)
                cbxCustomerDestination.Checked = ((col And 2 ^ KaReports.CustomerActivityReportColumns.RcCustomerDestination) <> 0)
                cbxOwner.Checked = ((col And 2 ^ KaReports.CustomerActivityReportColumns.RcOwner) <> 0)
                cbxBranch.Checked = ((col And 2 ^ KaReports.CustomerActivityReportColumns.RcBranch) <> 0)
                cbxFacility.Checked = ((col And 2 ^ KaReports.CustomerActivityReportColumns.RcFacility) <> 0)
                cbxPanel.Checked = ((col And 2 ^ KaReports.CustomerActivityReportColumns.RcPanel) <> 0)
                cbxDriver.Checked = ((col And 2 ^ KaReports.CustomerActivityReportColumns.RcDriver) <> 0)
                cbxTransports.Checked = ((col And 2 ^ KaReports.CustomerActivityReportColumns.RcTransport) <> 0)
                cbxCarrier.Checked = ((col And 2 ^ KaReports.CustomerActivityReportColumns.RcCarrier) <> 0)
                cbxDischargeLocations.Checked = ((col And 2 ^ KaReports.CustomerActivityReportColumns.RcDischargeLocations) <> 0)
                cbxApplicator.Checked = ((col And 2 ^ KaReports.CustomerActivityReportColumns.RcApplicator) <> 0)
            ElseIf _searchType = "searchall" Then
                tbxFind.Text = System.Uri.UnescapeDataString(Page.Request("Keyword"))
                pnlCustomerFilter.Visible = False
                pnlSupplierFilter.Visible = False
                pnlOwnerFilter.Visible = False
                pnlBranchFilter.Visible = False
                pnlProductFilter.Visible = False
                pnlBulkProductFilter.Visible = False
                pnlFromDate.Visible = False
                pnlToDate.Visible = False
                pnlActivityReportColumns.Visible = False
                pnlSortBy.Visible = False
                pnlIncludeInterfaceCrossReferences.Visible = True
                btnFilter_Click(btnFilter, New EventArgs())
            ElseIf _searchType = "receiving" OrElse _searchType = "pastreceiving" Then
                pnlCustomerFilter.Visible = False
                pnlSupplierFilter.Visible = True
                pnlOwnerFilter.Visible = True
                pnlBranchFilter.Visible = False
                pnlProductFilter.Visible = False
                pnlBulkProductFilter.Visible = True
                pnlActivityReportColumns.Visible = False
                pnlFromDate.Visible = False
                pnlToDate.Visible = False
                pnlSortBy.Visible = False
                pnlIncludeInterfaceCrossReferences.Visible = False
            Else
                pnlCustomerFilter.Visible = True
                pnlSupplierFilter.Visible = False
                pnlOwnerFilter.Visible = True
                pnlBranchFilter.Visible = True
                pnlProductFilter.Visible = True
                pnlBulkProductFilter.Visible = False
                pnlActivityReportColumns.Visible = False
                pnlFromDate.Visible = False
                pnlToDate.Visible = False
                pnlSortBy.Visible = True
                pnlIncludeInterfaceCrossReferences.Visible = False
            End If
        End If
    End Sub

#Region " Populate dropdown lists "
    Private Sub PopulateCustomersList(ByVal connection As OleDbConnection)
        With ddlCustomerFilter.Items
            .Clear()
            .Add(New ListItem("All customer accounts", Guid.Empty.ToString()))
            Dim customersRdr As OleDbDataReader = Tm2Database.ExecuteReader(connection, "SELECT id, name FROM customer_accounts " & _
                                                    "WHERE (deleted = 0) AND (disabled = 0) AND (is_supplier = 0) " & _
                                                    IIf(_currentUser.OwnerId.Equals(Guid.Empty), "", "AND (owner_id=" & Q(_currentUser.OwnerId) & " OR owner_id=" & Q(Guid.Empty) & ") ") & _
                                                    "ORDER BY UPPER(name), id")
            Do While customersRdr.Read()
                .Add(New ListItem(customersRdr.Item("name"), customersRdr.Item("id").ToString()))
            Loop
        End With
    End Sub

    Private Sub PopulateSuppliersList(ByVal connection As OleDbConnection)
        With ddlSupplierFilter.Items
            .Clear()
            .Add(New ListItem("All supplier accounts", Guid.Empty.ToString()))
            Dim suppliersRdr As OleDbDataReader = Tm2Database.ExecuteReader(connection, "SELECT id, name FROM customer_accounts " & _
                                                    "WHERE (deleted = 0) AND (disabled = 0) AND (is_supplier = 1) " & _
                                                    IIf(_currentUser.OwnerId.Equals(Guid.Empty), "", "AND (owner_id=" & Q(_currentUser.OwnerId) & " OR owner_id=" & Q(Guid.Empty) & ") ") & _
                                                    "ORDER BY UPPER(name), id")
            Do While suppliersRdr.Read()
                .Add(New ListItem(suppliersRdr.Item("name"), suppliersRdr.Item("id").ToString()))
            Loop
        End With
    End Sub

    Private Sub PopulateOwnersList(ByVal connection As OleDbConnection)
        With ddlOwnerFilter.Items
            .Clear()
            Dim ownersRdr As OleDbDataReader = Tm2Database.ExecuteReader(connection, "SELECT id, name FROM owners " & _
                                                    "WHERE (deleted = 0) " & _
                                                    IIf(_currentUser.OwnerId.Equals(Guid.Empty), "", "AND (id=" & Q(_currentUser.OwnerId) & ") ") & _
                                                    "ORDER BY UPPER(name), id")
            Do While ownersRdr.Read()
                .Add(New ListItem(ownersRdr.Item("name"), ownersRdr.Item("id").ToString()))
            Loop
            If .Count <> 1 Then .Insert(0, New ListItem("All owners", Guid.Empty.ToString()))
            If .Count > 0 Then ddlOwnerFilter.SelectedIndex = 0
        End With
    End Sub

    Private Sub PopulateBranchesList(ByVal connection As OleDbConnection)
        With ddlBranchFilter.Items
            .Clear()
            .Add(New ListItem("All branches", Guid.Empty.ToString()))
            Dim branchesRdr As OleDbDataReader = Tm2Database.ExecuteReader(connection, "SELECT id, name FROM branches " & _
                                                    "WHERE (deleted = 0) " & _
                                                    "ORDER BY UPPER(name), id")
            Do While branchesRdr.Read()
                .Add(New ListItem(branchesRdr.Item("name"), branchesRdr.Item("id").ToString()))
            Loop
        End With
    End Sub

    Private Sub PopulateProductsList(ByVal connection As OleDbConnection)
        With ddlProductFilter.Items
            .Clear()
            .Add(New ListItem("All products", Guid.Empty.ToString()))
            Dim productsRdr As OleDbDataReader = Tm2Database.ExecuteReader(connection, "SELECT id, name FROM products " & _
                                                    "WHERE (deleted = 0) " & _
                                                    "ORDER BY UPPER(name), id")
            Do While productsRdr.Read()
                .Add(New ListItem(productsRdr.Item("name"), productsRdr.Item("id").ToString()))
            Loop
        End With
    End Sub

    Private Sub PopulateBulkProductsList(ByVal connection As OleDbConnection)
        With ddlBulkProductFilter.Items
            .Clear()
            .Add(New ListItem("All bulk products", Guid.Empty.ToString()))
            Dim bulkProductsRdr As OleDbDataReader = Tm2Database.ExecuteReader(connection, "SELECT id, name FROM bulk_products " & _
                                                    "WHERE (deleted = 0) " & _
                                                    "ORDER BY UPPER(name), id")
            Do While bulkProductsRdr.Read()
                .Add(New ListItem(bulkProductsRdr.Item("name"), bulkProductsRdr.Item("id").ToString()))
            Loop
        End With
    End Sub

    Private Sub PopulateSortList()
        ddlSortBy.Items.Clear()
        If _searchType = "tickets" Then
            ddlSortBy.Items.Add(New ListItem("Date/time", "tickets.loaded_at"))
            ddlSortBy.Items.Add(New ListItem("Order number", "tickets.order_number"))
        ElseIf _searchType = "receiving" OrElse _searchType = "pastreceiving" Then
            ddlSortBy.Items.Add(New ListItem("Number", "receiving_purchase_orders.number"))
            ddlSortBy.Items.Add(New ListItem("Bulk product", "UPPER(bulk_products_name)"))
            ddlSortBy.Items.Add(New ListItem("Supplier", "UPPER(supplier_name)"))
        Else
            ddlSortBy.Items.Add(New ListItem("Order Number", "order_number"))
            ddlSortBy.Items.Add(New ListItem("Account Name", "customer_accounts_name"))
            ddlSortBy.Items.Add(New ListItem("Product Name", "products_name"))
            ddlSortBy.Items.Add(New ListItem("Owner", "owners_name"))
            ddlSortBy.Items.Add(New ListItem("Created", "order_created"))
        End If
        ddlSortBy.SelectedIndex = 0
    End Sub

    Private Sub PopulateAscDescList()
        ddlAscDesc.Items.Clear()
        ddlAscDesc.Items.Add(New ListItem("Asc", "ASC"))
        ddlAscDesc.Items.Add(New ListItem("Desc", "DESC"))
        If _searchType = "tickets" Then
            ddlAscDesc.SelectedIndex = 1
        Else
            ddlAscDesc.SelectedIndex = 0
        End If
    End Sub
#End Region

    Protected Sub btnFilter_Click(sender As Object, e As EventArgs) Handles btnFilter.Click
        Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
        If _searchType = "tickets" Then
            Dim unit As KaUnit = New KaUnit(connection, KaUnit.GetSystemDefaultMassUnitOfMeasure(connection, Nothing))
            Dim col As ULong = GetTicketsColumnsDisplayed()
            KaSetting.WriteSetting(connection, "AdvancedSearch:" & _currentUser.Id.ToString & "/CheckedOptions", col)

            Dim url As String = ""
            If Utilities.ConvertWebPageUrlDomainToRequestedPagesDomain(GetUserConnection(_currentUser.Id)) Then url = Tm2Database.GetCurrentPageUrlWithOriginalPort(Me.Request)
            litReport.Text = KaReports.GetCustomerActivityTable(connection, KaReports.MEDIA_TYPE_HTML, GenerateTicketsQuery(), CustomerActivityReportProductDisplayOptions.ProductAsColumn, "", col, unit, "", "", "", New List(Of KaUnit), url, False, False, False)
        ElseIf _searchType = "orders" OrElse _searchType = "pastorders" OrElse _searchType = "interfaceorders" Then
            Dim sql As String = GenerateOrdersQuery(connection, _searchType = "pastorders")

            Dim reportDataList As ArrayList = KaReports.GetOrdersTableMultipleProductsOneColumn(connection, sql, KaReports.MEDIA_TYPE_HTML, New List(Of KaOrder), False, False)

            Dim headerRowList As ArrayList = reportDataList(0) 'This will be the header row
            Dim tableAttributes As String = "border=1; width=100%;"
            Dim headerRowAttributes As String = ""
            Dim rowAttributes As String = ""

            Dim headerCellAttributeList As New List(Of String)
            Dim detailCellAttributeList As New List(Of String)
            KaReports.GetOrderListHtmlTableFormatting(tableAttributes, headerRowAttributes, rowAttributes, headerCellAttributeList, detailCellAttributeList, headerRowList)

            If _searchType = "interfaceorders" Then ' Parse the data to specify the proper page
                For Each orderRow As ArrayList In reportDataList
                    For i = 0 To orderRow.Count - 1
                        Dim dataitem As String = orderRow(i)
                        orderRow(i) = dataitem.Replace("Orders.aspx", "InterfaceAssignOrder.aspx")
                    Next
                Next
            End If

            litReport.Text = KaReports.GetTableHtml("", "", reportDataList, False, tableAttributes, "", headerCellAttributeList, "", detailCellAttributeList)
        ElseIf _searchType = "receiving" OrElse _searchType = "pastreceiving" Then
            Dim sql As String = GenerateReceivingOrdersQuery(connection, _searchType = "pastreceiving")

            Dim reportDataList As ArrayList = KaReports.GetReceivingPurchaseOrdersTable(connection, sql, KaReports.MEDIA_TYPE_HTML, False, Page.Request.Url.ToString)

            Dim headerRowList As ArrayList = reportDataList(0) 'This will be the header row
            Dim tableAttributes As String = "border=1; width=100%;"
            Dim headerRowAttributes As String = ""
            Dim rowAttributes As String = ""

            Dim headerCellAttributeList As New List(Of String)
            Dim detailCellAttributeList As New List(Of String)
            ' KaReports.GetOrderListHtmlTableFormatting(tableAttributes, headerRowAttributes, rowAttributes, headerCellAttributeList, detailCellAttributeList, headerRowList)

            litReport.Text = KaReports.GetTableHtml("", "", reportDataList, False, tableAttributes, "", headerCellAttributeList, "", detailCellAttributeList)
        ElseIf _searchType = "searchall" Then
            litReport.Text = GenerateAllEntitySearch(connection)
        Else
            litReport.Text = ""
        End If
    End Sub

    Private Function GenerateTicketsQuery() As String
        Dim customerAccountId As Guid = Guid.Parse(ddlCustomerFilter.SelectedValue)
        Dim ownerId As Guid = Guid.Parse(ddlOwnerFilter.SelectedValue)
        Dim productId As Guid = Guid.Parse(ddlProductFilter.SelectedValue)
        Dim branchId As Guid = Guid.Parse(ddlBranchFilter.SelectedValue)
        Dim fromDate As DateTime = DateTime.Parse(SQL_MINDATE)
        Dim fromDateValid As Boolean = DateTime.TryParse(tbxFromDate.Value, fromDate)
        Dim toDate As DateTime = DateTime.Now.AddYears(100)
        Dim toDateValid As Boolean = DateTime.TryParse(tbxToDate.Value, toDate)

        If _searchType = "searchall" OrElse Not fromDateValid OrElse Not toDateValid Then
            fromDate = DateTime.Parse(SQL_MINDATE)
            toDate = DateTime.Now.AddYears(100)
        End If

        If _searchType = "searchall" Then
            customerAccountId = Guid.Empty
            ownerId = _currentUser.OwnerId
            productId = Guid.Empty
            branchId = Guid.Empty
        End If

        Dim tickets As New List(Of Guid) ' get a list of the order IDs
        If tbxFind.Text.Trim.Length > 0 Then tickets = KaTicket.GetTicketIdsWithKeyword(GetUserConnection(_currentUser.Id), Nothing, _currentUser.OwnerId, tbxFind.Text.Trim) ' get a list of the ticket IDs 
        Dim validTicketIds As String = Q(Guid.Empty)
        For Each ticketId As Guid In tickets
            validTicketIds &= "," & Q(ticketId)
        Next

        Dim query As String = "SELECT tickets.id, ticket_items.product_id, tickets.owner_id " & _
            "FROM tickets, ticket_items " & _
            IIf(customerAccountId.Equals(Guid.Empty), "", ", ticket_customer_accounts") & _
            IIf(ownerId.Equals(Guid.Empty), "", ", orders") & _
            " WHERE tickets.internal_transfer=0" & _
                " AND tickets.loaded_at>=" & Q(fromDate) & _
                " AND tickets.loaded_at<=" & Q(toDate) & _
                IIf(customerAccountId.Equals(Guid.Empty), "", " AND tickets.id=ticket_customer_accounts.ticket_id AND ticket_customer_accounts.customer_account_id=" & Q(customerAccountId)) & _
                IIf(productId.Equals(Guid.Empty), "", " AND tickets.id=ticket_items.ticket_id AND ticket_items.product_id=" & Q(productId)) & _
                IIf(ownerId.Equals(Guid.Empty), "", " AND (tickets.owner_id = " & Q(ownerId) & " OR (tickets.order_id=orders.id AND orders.owner_id=" & Q(ownerId) & "))") & _
                IIf(branchId.Equals(Guid.Empty), "", " AND tickets.branch_id=" & Q(branchId)) & _
                 " AND ticket_items.ticket_id=tickets.id AND tickets.voided=0" & _
                 IIf(tbxFind.Text.Trim.Length = 0, "", " AND tickets.id in (" & validTicketIds & ")") & _
             " ORDER BY tickets.loaded_at DESC"

        Return query
    End Function

    Private Function GetTicketsColumnsDisplayed() As UInt64
        Dim col As UInt64 = 0
        If cbxDateTime.Checked Then col += 2 ^ KaReports.CustomerActivityReportColumns.RcDateTime
        If cbxOrderNumber.Checked Then col += 2 ^ KaReports.CustomerActivityReportColumns.RcOrderNumber
        If cbxTicketNumber.Checked Then col += 2 ^ KaReports.CustomerActivityReportColumns.RcTicketNumber
        If cbxCustomer.Checked Then col += 2 ^ KaReports.CustomerActivityReportColumns.RcCustomer
        If cbxCustomerDestination.Checked Then col += 2 ^ KaReports.CustomerActivityReportColumns.RcCustomerDestination
        If cbxOwner.Checked Then col += 2 ^ KaReports.CustomerActivityReportColumns.RcOwner
        If cbxBranch.Checked Then col += 2 ^ KaReports.CustomerActivityReportColumns.RcBranch
        If cbxFacility.Checked Then col += 2 ^ KaReports.CustomerActivityReportColumns.RcFacility
        If cbxPanel.Checked Then col += 2 ^ KaReports.CustomerActivityReportColumns.RcPanel
        If cbxDriver.Checked Then col += 2 ^ KaReports.CustomerActivityReportColumns.RcDriver
        If cbxTransports.Checked Then col += 2 ^ KaReports.CustomerActivityReportColumns.RcTransport
        If cbxCarrier.Checked Then col += 2 ^ KaReports.CustomerActivityReportColumns.RcCarrier
        If cbxDischargeLocations.Checked Then col += 2 ^ KaReports.CustomerActivityReportColumns.RcDischargeLocations
        If cbxApplicator.Checked Then col += 2 ^ KaReports.CustomerActivityReportColumns.RcApplicator
        Return col
    End Function

    Private Function GenerateOrdersQuery(connection As OleDbConnection, ByVal pastOrders As Boolean) As String
        Dim customerAccountId As Guid = Guid.Parse(ddlCustomerFilter.SelectedValue)
        Dim ownerId As Guid = Guid.Parse(ddlOwnerFilter.SelectedValue)
        Dim productId As Guid = Guid.Parse(ddlProductFilter.SelectedValue)
        Dim branchId As Guid = Guid.Parse(ddlBranchFilter.SelectedValue)
        Dim sortBy As String = ddlSortBy.SelectedValue & " " & ddlAscDesc.SelectedValue

        If _searchType = "searchall" Then
            customerAccountId = Guid.Empty
            ownerId = _currentUser.OwnerId
            productId = Guid.Empty
            branchId = Guid.Empty
            sortBy = "order_number ASC"
        End If

        Dim orders As New List(Of Guid) ' get a list of the order IDs
        If tbxFind.Text.Trim.Length > 0 Then orders = KaOrder.GetOrderIdsWithKeyword(GetUserConnection(_currentUser.Id), Nothing, _currentUser.OwnerId, tbxFind.Text.Trim, pastOrders) ' get a list of the ticket IDs 
        Dim validOrderIds As String = Q(Guid.Empty)
        For Each ticketId As Guid In orders
            validOrderIds &= "," & Q(ticketId)
        Next

        Dim sql As String = "SELECT DISTINCT orders.id AS orders_id, orders.number AS order_number, orders.completed AS orders_completed, order_customer_accounts.customer_account_id AS order_customer_accounts_customer_account_id, customer_accounts.name AS customer_accounts_name, order_items.id AS order_items_id, products.name AS products_name, owners.name AS owners_name, orders.created AS order_created, order_items.product_id AS order_items_product_id " & _
                "FROM orders, order_customer_accounts, customer_accounts, order_items, products, owners " & _
                "WHERE orders.completed = " & Q(pastOrders) & " AND orders.deleted = 0 AND order_customer_accounts.order_id = orders.id " & _
                    "AND order_customer_accounts.customer_account_id = customer_accounts.id AND order_items.order_id = orders.id " & _
                    "AND order_items.product_id = products.id AND orders.owner_id = owners.id " & _
                    IIf(customerAccountId = Guid.Empty, "", "AND order_customer_accounts.customer_account_id = " & Q(customerAccountId) & " ") & _
                    IIf(ownerId = Guid.Empty, "", "AND owners.id = " & Q(ownerId) & " ") &
                    IIf(branchId.Equals(Guid.Empty), "", "AND orders.branch_id = " & Q(branchId) & " ") & _
                    IIf(productId.Equals(Guid.Empty), "", " AND orders.id IN (SELECT order_id FROM order_items WHERE product_id = " & Q(productId) & " AND deleted = 0) ") & _
                    IIf(tbxFind.Text.Trim.Length = 0, "", "AND orders.id IN (" & validOrderIds & ") ") & _
                "ORDER BY " & sortBy

        Return sql
    End Function

    Private Function GenerateReceivingTicketsQuery() As String
        Dim supplierAccountId As Guid = Guid.Parse(ddlSupplierFilter.SelectedValue)
        Dim ownerId As Guid = Guid.Parse(ddlOwnerFilter.SelectedValue)
        Dim bulkProductId As Guid = Guid.Parse(ddlBulkProductFilter.SelectedValue)
        Dim fromDate As DateTime = DateTime.Parse(SQL_MINDATE)
        Dim fromDateValid As Boolean = DateTime.TryParse(tbxFromDate.Value, fromDate)
        Dim toDate As DateTime = DateTime.Now.AddYears(100)
        Dim toDateValid As Boolean = DateTime.TryParse(tbxToDate.Value, toDate)

        If _searchType = "searchall" OrElse Not fromDateValid OrElse Not toDateValid Then
            fromDate = DateTime.Parse(SQL_MINDATE)
            toDate = DateTime.Now.AddYears(100)
        End If

        If _searchType = "searchall" Then
            supplierAccountId = Guid.Empty
            ownerId = _currentUser.OwnerId
            bulkProductId = Guid.Empty
        End If

        Dim tickets As New List(Of Guid) ' get a list of the order IDs
        If tbxFind.Text.Trim.Length > 0 Then tickets = KaReceivingTicket.GetReceivingTicketIdsWithKeyword(GetUserConnection(_currentUser.Id), Nothing, _currentUser.OwnerId, tbxFind.Text.Trim) ' get a list of the ticket IDs 
        Dim validTicketIds As String = Q(Guid.Empty)
        For Each ticketId As Guid In tickets
            validTicketIds &= "," & Q(ticketId)
        Next

        Dim query As String = "SELECT receiving_tickets.id, receiving_tickets.bulk_product_id " & _
            "FROM receiving_tickets " & _
            "WHERE receiving_tickets.voided = 0 AND receiving_tickets.date_of_delivery>=" & Q(fromDate) & _
                " AND receiving_tickets.date_of_delivery<=" & Q(toDate) & _
                 IIf(supplierAccountId.Equals(Guid.Empty), "", " AND receiving_tickets.supplier_account_id=" & Q(supplierAccountId)) & _
                 IIf(bulkProductId.Equals(Guid.Empty), "", " AND receiving_tickets.bulk_product_id=" & Q(bulkProductId)) & _
                 IIf(ownerId.Equals(Guid.Empty), "", " AND receiving_tickets.owner_id=" & Q(ownerId)) & _
                 IIf(tbxFind.Text.Trim.Length = 0, "", " AND receiving_tickets.id IN(" & validTicketIds & ")") & _
            " ORDER BY receiving_tickets.date_of_delivery DESC"
        Return query

    End Function

    Private Function GenerateReceivingOrdersQuery(connection As OleDbConnection, ByVal pastOrders As Boolean) As String
        Dim supplierAccountId As Guid = Guid.Parse(ddlSupplierFilter.SelectedValue)
        Dim ownerId As Guid = Guid.Parse(ddlOwnerFilter.SelectedValue)
        Dim bulkProductId As Guid = Guid.Parse(ddlBulkProductFilter.SelectedValue)
        Dim sortBy As String = ddlSortBy.SelectedValue & " " & ddlAscDesc.SelectedValue

        If _searchType = "searchall" Then
            supplierAccountId = Guid.Empty
            ownerId = _currentUser.OwnerId
            bulkProductId = Guid.Empty
            sortBy = "number ASC"
        End If

        Dim orders As New List(Of Guid) ' get a list of the order IDs
        If tbxFind.Text.Trim.Length > 0 Then orders = KaReceivingPurchaseOrder.GetReceivingPurchaseOrderIdsWithKeyword(GetUserConnection(_currentUser.Id), Nothing, _currentUser.OwnerId, tbxFind.Text.Trim, pastOrders) ' get a list of the ticket IDs 
        Dim validOrderIds As String = Q(Guid.Empty)
        For Each ticketId As Guid In orders
            validOrderIds &= "," & Q(ticketId)
        Next

        Dim sql As String = "SELECT DISTINCT receiving_purchase_orders.id as order_id, receiving_purchase_orders.number, receiving_purchase_orders.completed, " & _
                        "sa.name as supplier_name, bulk_products.name AS bulk_products_name, " & _
                        "CASE WHEN owners.name IS NULL THEN '' ELSE owners.name END AS owners_name, " & _
                        "receiving_purchase_orders.created, receiving_purchase_orders.bulk_product_id, " & _
                        "receiving_purchase_orders.purchased, receiving_purchase_orders.delivered, " & _
                        "receiving_purchase_orders.unit_id " & _
                "FROM receiving_purchase_orders " & _
                "INNER JOIN bulk_products ON receiving_purchase_orders.bulk_product_id = bulk_products.id " & _
                "LEFT OUTER JOIN owners ON receiving_purchase_orders.owner_id = owners.id " & _
                "INNER JOIN customer_accounts AS sa ON receiving_purchase_orders.supplier_account_id = sa.id " & _
                "WHERE receiving_purchase_orders.completed = " & Q(pastOrders) & " AND receiving_purchase_orders.deleted = 0 " & _
                    IIf(supplierAccountId = Guid.Empty, "", "AND receiving_purchase_orders.supplier_account_id = " & Q(supplierAccountId) & " ") & _
                    IIf(ownerId = Guid.Empty, "", "AND owners.id = " & Q(ownerId) & " ") &
                    IIf(bulkProductId.Equals(Guid.Empty), "", " AND receiving_purchase_orders.bulk_product_id = " & Q(bulkProductId) & " ") & _
                    IIf(tbxFind.Text.Trim.Length = 0, "", "AND receiving_purchase_orders.id IN (" & validOrderIds & ") ") & _
                "ORDER BY number ASC"

        Return sql
    End Function

    Private Function GenerateAllEntitySearch(connection As OleDbConnection) As String
        Dim keyword As String = tbxFind.Text.Trim
        If keyword.Trim.Length = 0 Then Return ""

        Dim includeInterfaceCrossReferences As Boolean = cbxIncludeInterfaceCrossReferences.Checked

        Dim entityType As String = ""
        Dim entities As New Dictionary(Of String, String)
        Dim titleSplit() As String = _sourceTitle.Split(":")
        If titleSplit.Length > 1 Then
            entityType = titleSplit(1).Trim
        Else
            entityType = titleSplit(0).Trim
        End If
        If entityType = "customer activity report" Then entityType = "orders"
        If entityType.Length > 0 Then AddEntityType(connection, entityType, keyword, entities, includeInterfaceCrossReferences)
        AddEntityType(connection, "applicators", keyword, entities, includeInterfaceCrossReferences)
        AddEntityType(connection, "bays", keyword, entities, includeInterfaceCrossReferences)
        AddEntityType(connection, "branches", keyword, entities, includeInterfaceCrossReferences)
        AddEntityType(connection, "bulk products", keyword, entities, includeInterfaceCrossReferences)
        AddEntityType(connection, "carriers", keyword, entities, includeInterfaceCrossReferences)
        AddEntityType(connection, "containers", keyword, entities, includeInterfaceCrossReferences)
        AddEntityType(connection, "container types", keyword, entities, includeInterfaceCrossReferences)
        AddEntityType(connection, "container equipment", keyword, entities, includeInterfaceCrossReferences)
        AddEntityType(connection, "container equipment types", keyword, entities, includeInterfaceCrossReferences)
        AddEntityType(connection, "crop types", keyword, entities, includeInterfaceCrossReferences)
        AddEntityType(connection, "accounts", keyword, entities, includeInterfaceCrossReferences)
        AddEntityType(connection, "destinations", keyword, entities, includeInterfaceCrossReferences)
        AddEntityType(connection, "discharge locations", keyword, entities, includeInterfaceCrossReferences)
        AddEntityType(connection, "drivers", keyword, entities, includeInterfaceCrossReferences)
        AddEntityType(connection, "facilities", keyword, entities, includeInterfaceCrossReferences)
        AddEntityType(connection, "interfaces", keyword, entities, includeInterfaceCrossReferences)
        AddEntityType(connection, "interface types", keyword, entities, includeInterfaceCrossReferences)
        AddEntityType(connection, "inventory groups", keyword, entities, includeInterfaceCrossReferences)
        AddEntityType(connection, "orders", keyword, entities, includeInterfaceCrossReferences)
        AddEntityType(connection, "owners", keyword, entities, includeInterfaceCrossReferences)
        AddEntityType(connection, "panels", keyword, entities, includeInterfaceCrossReferences)
        AddEntityType(connection, "panel groups", keyword, entities, includeInterfaceCrossReferences)
        AddEntityType(connection, "products", keyword, entities, includeInterfaceCrossReferences)
        AddEntityType(connection, "product groups", keyword, entities, includeInterfaceCrossReferences)
        AddEntityType(connection, "receiving purchase orders", keyword, entities, includeInterfaceCrossReferences)
        AddEntityType(connection, "past receiving purchase orders", keyword, entities, includeInterfaceCrossReferences)
        AddEntityType(connection, "receiving activity report", keyword, entities, includeInterfaceCrossReferences)
        AddEntityType(connection, "seals", keyword, entities, includeInterfaceCrossReferences)
        AddEntityType(connection, "suppliers", keyword, entities, includeInterfaceCrossReferences)
        AddEntityType(connection, "tanks", keyword, entities, includeInterfaceCrossReferences)
        AddEntityType(connection, "tank groups", keyword, entities, includeInterfaceCrossReferences)
        AddEntityType(connection, "receipts", keyword, entities, includeInterfaceCrossReferences)
        AddEntityType(connection, "tracks", keyword, entities, includeInterfaceCrossReferences)
        AddEntityType(connection, "transports", keyword, entities, includeInterfaceCrossReferences)
        AddEntityType(connection, "transport types", keyword, entities, includeInterfaceCrossReferences)
        AddEntityType(connection, "units", keyword, entities, includeInterfaceCrossReferences)
        AddEntityType(connection, "users", keyword, entities, includeInterfaceCrossReferences)

        Dim retVal As String = ""
        For Each entity As String In entities.Keys
            If entities(entity).Length > 0 Then
                retVal &= "<div class=""section"">" & entities(entity) & "</div>"
            End If
        Next
        If retVal.Length = 0 Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidKeyword", Utilities.JsAlert("Nothing found containing keywords: " & tbxFind.Text))
        Return retVal
    End Function

    Private Sub AddEntityType(ByVal connection As OleDbConnection, ByVal entityType As String, ByVal keyword As String, ByRef entities As Dictionary(Of String, String), ByVal includeInterfaceCrossReferences As Boolean)
        Dim entityTable As String = ""
        Dim currentUserPermission As String = ""
        Dim _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
        Dim _currentTableName As String = Nothing
        Dim headerRowAttributes As String = "style=""font-weight:bold;"""
        If Not entities.ContainsKey(entityType) Then
            Select Case entityType
                Case "applicators"
                    If Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaApplicator.TABLE_NAME}), "Applicators")(KaApplicator.TABLE_NAME).Read Then
                        Dim applicatorsTable As New ArrayList()

                        For Each applicator As KaApplicator In KaApplicator.GetApplicatorsWithKeyword(connection, Nothing, keyword, includeInterfaceCrossReferences)
                            applicatorsTable.Add(New ArrayList({"<a href=""Applicators.aspx?ApplicatorId=" & applicator.Id.ToString & """>" & Server.HtmlEncode(applicator.Name) & "</a>"}))
                        Next
                        If applicatorsTable.Count > 0 Then
                            applicatorsTable.Insert(0, New ArrayList({"Name"}))
                            entityTable = KaReports.GetTableHtml("Applicators", "", applicatorsTable, False, "", headerRowAttributes, New List(Of String), "", New List(Of String), True)
                        End If
                    End If
                Case "bays"
                    If Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaLocation.TABLE_NAME}), "Facilities")(KaLocation.TABLE_NAME).Read Then
                        Dim baysTable As New ArrayList()

                        For Each bay As KaBay In KaBay.GetBaysWithKeyword(connection, Nothing, keyword)
                            baysTable.Add(New ArrayList({"<a href=""Bays.aspx?BayId=" & bay.Id.ToString & """>" & Server.HtmlEncode(bay.Name) & "</a>"}))
                        Next
                        If baysTable.Count > 0 Then
                            baysTable.Insert(0, New ArrayList({"Name"}))
                            entityTable = KaReports.GetTableHtml("Bays", "", baysTable, False, "", headerRowAttributes, New List(Of String), "", New List(Of String), True)
                        End If
                    End If
                Case "branches"
                    If Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaBranch.TABLE_NAME}), "Branches")(KaBranch.TABLE_NAME).Read Then
                        Dim branchesTable As New ArrayList()

                        For Each branch As KaBranch In KaBranch.GetBranchesWithKeyword(connection, Nothing, keyword, includeInterfaceCrossReferences)
                            branchesTable.Add(New ArrayList({"<a href=""Branches.aspx?BranchId=" & branch.Id.ToString & """>" & Server.HtmlEncode(branch.Name) & "</a>"}))
                        Next
                        If branchesTable.Count > 0 Then
                            branchesTable.Insert(0, New ArrayList({"Name"}))
                            entityTable = KaReports.GetTableHtml("Branches", "", branchesTable, False, "", headerRowAttributes, New List(Of String), "", New List(Of String), True)
                        End If
                    End If
                Case "bulk products"
                    If Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaProduct.TABLE_NAME}), "Products")(KaProduct.TABLE_NAME).Read Then
                        Dim bulkProductsTable As New ArrayList()

                        For Each bulkProduct As KaBulkProduct In KaBulkProduct.GetBulkProductsWithKeyword(connection, Nothing, _currentUser.OwnerId, keyword, includeInterfaceCrossReferences)
                            bulkProductsTable.Add(New ArrayList({"<a href=""BulkProducts.aspx?BulkProductId=" & bulkProduct.Id.ToString & """>" & Server.HtmlEncode(bulkProduct.Name) & "</a>"}))
                        Next
                        If bulkProductsTable.Count > 0 Then
                            bulkProductsTable.Insert(0, New ArrayList({"Name"}))
                            entityTable = KaReports.GetTableHtml("Bulk Products", "", bulkProductsTable, False, "", headerRowAttributes, New List(Of String), "", New List(Of String), True)
                        End If
                    End If
                Case "carriers"
                    If Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaCarrier.TABLE_NAME}), "Carriers")(KaCarrier.TABLE_NAME).Read Then
                        Dim carriersTable As New ArrayList()

                        For Each carrier As KaCarrier In KaCarrier.GetCarriersWithKeyword(connection, Nothing, keyword, includeInterfaceCrossReferences)
                            carriersTable.Add(New ArrayList({"<a href=""Carriers.aspx?CarrierId=" & carrier.Id.ToString & """>" & Server.HtmlEncode(carrier.Name) & "</a>"}))
                        Next
                        If carriersTable.Count > 0 Then
                            carriersTable.Insert(0, New ArrayList({"Name"}))
                            entityTable = KaReports.GetTableHtml("Carriers", "", carriersTable, False, "", headerRowAttributes, New List(Of String), "", New List(Of String), True)
                        End If
                    End If
                Case "containers"
                    If Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaContainer.TABLE_NAME}), "Containers")(KaContainer.TABLE_NAME).Read Then
                        Dim containersTable As New ArrayList()

                        For Each container As KaContainer In KaContainer.GetContainersWithKeyword(connection, Nothing, _currentUser.OwnerId, keyword)
                            containersTable.Add(New ArrayList({"<a href=""Containers.aspx?id=" & container.Id.ToString & """>" & Server.HtmlEncode(container.Number) & "</a>"}))
                        Next
                        If containersTable.Count > 0 Then
                            containersTable.Insert(0, New ArrayList({"Number"}))
                            entityTable = KaReports.GetTableHtml("Containers", "", containersTable, False, "", headerRowAttributes, New List(Of String), "", New List(Of String), True)
                        End If
                    End If
                Case "container types"
                    If Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaContainer.TABLE_NAME}), "Containers")(KaContainer.TABLE_NAME).Read Then
                        Dim containerTypesTable As New ArrayList()

                        For Each containerType As KaContainerType In KaContainerType.GetContainerTypesWithKeyword(connection, Nothing, keyword)
                            containerTypesTable.Add(New ArrayList({"<a href=""ContainerTypes.aspx?ContainerTypeId=" & containerType.Id.ToString & """>" & Server.HtmlEncode(containerType.Name) & "</a>"}))
                        Next
                        If containerTypesTable.Count > 0 Then
                            containerTypesTable.Insert(0, New ArrayList({"Name"}))
                            entityTable = KaReports.GetTableHtml("Container Types", "", containerTypesTable, False, "", headerRowAttributes, New List(Of String), "", New List(Of String), True)
                        End If
                    End If
                Case "container equipment"
                    If Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaContainer.TABLE_NAME}), "Containers")(KaContainer.TABLE_NAME).Read Then
                        Dim containerEquipmentTable As New ArrayList()

                        For Each containerEquipment As KaContainerEquipment In KaContainerEquipment.GetContainerEquipmentWithKeyword(connection, Nothing, _currentUser.OwnerId, keyword)
                            containerEquipmentTable.Add(New ArrayList({"<a href=""ContainerEquipment.aspx?ContainerEquipmentId=" & containerEquipment.Id.ToString & """>" & Server.HtmlEncode(containerEquipment.Name) & "</a>"}))
                        Next
                        If containerEquipmentTable.Count > 0 Then
                            containerEquipmentTable.Insert(0, New ArrayList({"Name"}))
                            entityTable = KaReports.GetTableHtml("Container Equipment", "", containerEquipmentTable, False, "", headerRowAttributes, New List(Of String), "", New List(Of String), True)
                        End If
                    End If
                Case "container equipment types"
                    If Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaContainer.TABLE_NAME}), "Containers")(KaContainer.TABLE_NAME).Read Then
                        Dim containerEquipmentTypesTable As New ArrayList()

                        For Each containerEquipmentType As KaContainerEquipmentType In KaContainerEquipmentType.GetContainerEquipmentTypesWithKeyword(connection, Nothing, keyword)
                            containerEquipmentTypesTable.Add(New ArrayList({"<a href=""ContainerEquipmentTypes.aspx?ContainerEquipmentTypeId=" & containerEquipmentType.Id.ToString & """>" & Server.HtmlEncode(containerEquipmentType.Name) & "</a>"}))
                        Next
                        If containerEquipmentTypesTable.Count > 0 Then
                            containerEquipmentTypesTable.Insert(0, New ArrayList({"Name"}))
                            entityTable = KaReports.GetTableHtml("Container Equipment Types", "", containerEquipmentTypesTable, False, "", headerRowAttributes, New List(Of String), "", New List(Of String), True)
                        End If
                    End If
                Case "crop types"
                    If Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaCropType.TABLE_NAME}), "Crops")(KaCropType.TABLE_NAME).Read Then
                        Dim cropTypesTable As New ArrayList()

                        For Each cropType As KaCropType In KaCropType.GetCropTypesWithKeyword(connection, Nothing, keyword)
                            cropTypesTable.Add(New ArrayList({"<a href=""CropTypes.aspx?CropTypeId=" & cropType.Id.ToString & """>" & Server.HtmlEncode(cropType.Name) & "</a>"}))
                        Next
                        If cropTypesTable.Count > 0 Then
                            cropTypesTable.Insert(0, New ArrayList({"Name"}))
                            entityTable = KaReports.GetTableHtml("Crop Types", "", cropTypesTable, False, "", headerRowAttributes, New List(Of String), "", New List(Of String), True)
                        End If
                    End If
                Case "accounts"
                    If Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaCustomerAccount.TABLE_NAME}), "Accounts")(KaCustomerAccount.TABLE_NAME).Read Then
                        Dim customerAccountsTable As New ArrayList()

                        For Each customerAccount As KaCustomerAccount In KaCustomerAccount.GetCustomerAccountsWithKeyword(connection, Nothing, _currentUser.OwnerId, keyword, includeInterfaceCrossReferences)
                            customerAccountsTable.Add(New ArrayList({"<a href=""Accounts.aspx?CustomerAccountId=" & customerAccount.Id.ToString & """>" & Server.HtmlEncode(customerAccount.Name) & "</a>", Server.HtmlEncode(customerAccount.AccountNumber), Server.HtmlEncode(customerAccount.Street), Server.HtmlEncode(customerAccount.City), Server.HtmlEncode(customerAccount.State), Server.HtmlEncode(customerAccount.ZipCode), Server.HtmlEncode(customerAccount.Phone)}))
                        Next
                        If customerAccountsTable.Count > 0 Then
                            customerAccountsTable.Insert(0, New ArrayList({"Name", "Default cross reference", "Street", "City", "State", "ZipCode", "Phone"}))
                            entityTable = KaReports.GetTableHtml("Customers", "", customerAccountsTable, False, "", headerRowAttributes, New List(Of String), "", New List(Of String), True)
                        End If
                    End If
                Case "destinations"
                    If Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaCustomerAccount.TABLE_NAME}), "Accounts")(KaCustomerAccount.TABLE_NAME).Read Then
                        Dim customerAccountLocationsTable As New ArrayList()

                        For Each customerAccountLocation As KaCustomerAccountLocation In KaCustomerAccountLocation.GetCustomerAccountLocationsWithKeyword(connection, Nothing, _currentUser.OwnerId, keyword, includeInterfaceCrossReferences)
                            Dim customerName As String = "All customers"
                            Try
                                customerName = New KaCustomerAccount(connection, customerAccountLocation.CustomerAccountId).Name
                            Catch ex As RecordNotFoundException

                            End Try
                            customerAccountLocationsTable.Add(New ArrayList({"<a href=""AccountDestinations.aspx?CustomerAccountLocationId=" & customerAccountLocation.Id.ToString & """>" & Server.HtmlEncode(customerAccountLocation.Name) & "</a>", Server.HtmlEncode(customerAccountLocation.CrossReference), Server.HtmlEncode(customerAccountLocation.Street), Server.HtmlEncode(customerAccountLocation.City), Server.HtmlEncode(customerAccountLocation.State), Server.HtmlEncode(customerAccountLocation.ZipCode), Server.HtmlEncode(customerName)}))
                        Next
                        If customerAccountLocationsTable.Count > 0 Then
                            customerAccountLocationsTable.Insert(0, New ArrayList({"Name", "Default cross reference", "Street", "City", "State", "ZipCode", "Customer"}))
                            entityTable = KaReports.GetTableHtml("Destinations", "", customerAccountLocationsTable, False, "", headerRowAttributes, New List(Of String), "", New List(Of String), True)
                        End If
                    End If
                Case "discharge locations"
                    If Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaPanel.TABLE_NAME}), "Panels")(KaPanel.TABLE_NAME).Read Then
                        Dim dischargeLocationsTable As New ArrayList()

                        For Each dischargeLocation As KaDischargeLocation In KaDischargeLocation.GetDischargeLocationsWithKeyword(connection, Nothing, keyword)
                            dischargeLocationsTable.Add(New ArrayList({"<a href=""DischargeLocations.aspx?DischargeLocationId=" & dischargeLocation.Id.ToString & """>" & Server.HtmlEncode(dischargeLocation.Name) & "</a>"}))
                        Next
                        If dischargeLocationsTable.Count > 0 Then
                            dischargeLocationsTable.Insert(0, New ArrayList({"Name"}))
                            entityTable = KaReports.GetTableHtml("DischargeLocations", "", dischargeLocationsTable, False, "", headerRowAttributes, New List(Of String), "", New List(Of String), True)
                        End If
                    End If
                Case "drivers"
                    If Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaDriver.TABLE_NAME}), "Drivers")(KaDriver.TABLE_NAME).Read Then
                        Dim driversTable As New ArrayList()

                        For Each driver As KaDriver In KaDriver.GetDriversWithKeyword(connection, Nothing, keyword, includeInterfaceCrossReferences)
                            driversTable.Add(New ArrayList({"<a href=""Drivers.aspx?DriverId=" & driver.Id.ToString & """>" & Server.HtmlEncode(driver.Name) & "</a>"}))
                        Next
                        If driversTable.Count > 0 Then
                            driversTable.Insert(0, New ArrayList({"Name"}))
                            entityTable = KaReports.GetTableHtml("Drivers", "", driversTable, False, "", headerRowAttributes, New List(Of String), "", New List(Of String), True)
                        End If
                    End If
                Case "interfaces"
                    If Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaInterface.TABLE_NAME}), "Interfaces")(KaInterface.TABLE_NAME).Read Then
                        Dim interfacesTable As New ArrayList()

                        For Each interfaceInfo As KaInterface In KaInterface.GetInterfacesWithKeyword(connection, Nothing, keyword)
                            Try
                                Dim interfaceType As New KaInterfaceTypes(connection, interfaceInfo.InterfaceTypeId)
                                If interfaceType.ConfigUrl.Trim().Length > 0 Then ' show the interface type's configuration page
                                    interfacesTable.Add(New ArrayList({"<a href=""custom.aspx?page_title=Interfaces:" & System.Web.HttpUtility.HtmlEncode(interfaceInfo.Name) & "&url=" & interfaceType.ConfigUrl & "?interface_id=" & interfaceInfo.Id.ToString & """>" & Server.HtmlEncode(interfaceInfo.Name) & "</a>"}))
                                Else
                                    interfacesTable.Add(New ArrayList({"<a href=""Interfaces.aspx?InterfaceId=" & interfaceInfo.Id.ToString & """>" & Server.HtmlEncode(interfaceInfo.Name) & "</a>"}))
                                End If
                            Catch ex As RecordNotFoundException

                            End Try
                        Next
                        If interfacesTable.Count > 0 Then
                            interfacesTable.Insert(0, New ArrayList({"Name"}))
                            entityTable = KaReports.GetTableHtml("Interfaces", "", interfacesTable, False, "", headerRowAttributes, New List(Of String), "", New List(Of String), True)
                        End If
                    End If
                Case "interface types"
                    If Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaInterface.TABLE_NAME}), "Interfaces")(KaInterface.TABLE_NAME).Read Then
                        Dim interfaceTypesTable As New ArrayList()

                        For Each interfaceType As KaInterfaceTypes In KaInterfaceTypes.GetInterfaceTypesWithKeyword(connection, Nothing, keyword)
                            interfaceTypesTable.Add(New ArrayList({"<a href=""InterfaceTypes.aspx?InterfaceTypeId=" & interfaceType.Id.ToString & """>" & Server.HtmlEncode(interfaceType.Name) & "</a>"}))
                        Next
                        If interfaceTypesTable.Count > 0 Then
                            interfaceTypesTable.Insert(0, New ArrayList({"Name"}))
                            entityTable = KaReports.GetTableHtml("Interface Types", "", interfaceTypesTable, False, "", headerRowAttributes, New List(Of String), "", New List(Of String), True)
                        End If
                    End If
                Case "inventory groups"
                    If Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaBulkProductInventory.TABLE_NAME}), "Inventory")(KaBulkProductInventory.TABLE_NAME).Read Then
                        Dim inventoryGroupsTable As New ArrayList()

                        For Each inventoryGroup As KaInventoryGroup In KaInventoryGroup.GetInventoryGroupsWithKeyword(connection, Nothing, keyword)
                            inventoryGroupsTable.Add(New ArrayList({"<a href=""InventoryGroups.aspx?InventoryGroupId=" & inventoryGroup.Id.ToString & """>" & Server.HtmlEncode(inventoryGroup.Name) & "</a>"}))
                        Next
                        If inventoryGroupsTable.Count > 0 Then
                            inventoryGroupsTable.Insert(0, New ArrayList({"Name"}))
                            entityTable = KaReports.GetTableHtml("Inventory Groups", "", inventoryGroupsTable, False, "", headerRowAttributes, New List(Of String), "", New List(Of String), True)
                        End If
                    End If
                Case "facilities"
                    If Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaLocation.TABLE_NAME}), "Facilities")(KaLocation.TABLE_NAME).Read Then
                        Dim locationsTable As New ArrayList()

                        For Each location As KaLocation In KaLocation.GetLocationsWithKeyword(connection, Nothing, keyword, includeInterfaceCrossReferences)
                            locationsTable.Add(New ArrayList({"<a href=""Facilities.aspx?LocationId=" & location.Id.ToString & """>" & Server.HtmlEncode(location.Name) & "</a>"}))
                        Next
                        If locationsTable.Count > 0 Then
                            locationsTable.Insert(0, New ArrayList({"Name"}))
                            entityTable = KaReports.GetTableHtml("Facilities", "", locationsTable, False, "", headerRowAttributes, New List(Of String), "", New List(Of String), True)
                        End If
                    End If
                Case "orders"
                    If Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaOrder.TABLE_NAME}), "Orders")(KaOrder.TABLE_NAME).Read Then
                        Dim sql As String = GenerateOrdersQuery(connection, False)

                        Dim reportDataList As ArrayList = KaReports.GetOrdersTableMultipleProductsOneColumn(connection, sql, KaReports.MEDIA_TYPE_HTML, New List(Of KaOrder), False, False)
                        If reportDataList.Count > 1 Then
                            Dim headerRowList As ArrayList = reportDataList(0) 'This will be the header row
                            Dim tableAttributes As String = "border=1; width=100%;"
                            ' Dim headerRowAttributes As String = ""
                            Dim rowAttributes As String = ""

                            Dim headerCellAttributeList As New List(Of String)
                            Dim detailCellAttributeList As New List(Of String)
                            KaReports.GetOrderListHtmlTableFormatting(tableAttributes, headerRowAttributes, rowAttributes, headerCellAttributeList, detailCellAttributeList, headerRowList)

                            entityTable = KaReports.GetTableHtml("Orders", "", reportDataList, False, tableAttributes, "", headerCellAttributeList, "", detailCellAttributeList)
                        End If
                    End If
                Case "past orders"
                    If Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaOrder.TABLE_NAME}), "Orders")(KaOrder.TABLE_NAME).Read Then
                        Dim sql As String = GenerateOrdersQuery(connection, True)

                        Dim reportDataList As ArrayList = KaReports.GetOrdersTableMultipleProductsOneColumn(connection, sql, KaReports.MEDIA_TYPE_HTML, New List(Of KaOrder), False, False)
                        If reportDataList.Count > 1 Then
                            Dim headerRowList As ArrayList = reportDataList(0) 'This will be the header row
                            Dim tableAttributes As String = "border=1; width=100%;"
                            ' Dim headerRowAttributes As String = ""
                            Dim rowAttributes As String = ""

                            Dim headerCellAttributeList As New List(Of String)
                            Dim detailCellAttributeList As New List(Of String)
                            KaReports.GetOrderListHtmlTableFormatting(tableAttributes, headerRowAttributes, rowAttributes, headerCellAttributeList, detailCellAttributeList, headerRowList)

                            entityTable = KaReports.GetTableHtml("Past Orders", "", reportDataList, False, tableAttributes, "", headerCellAttributeList, "", detailCellAttributeList)
                        End If
                    End If
                Case "owners"
                    If Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaOwner.TABLE_NAME}), "Owners")(KaOwner.TABLE_NAME).Read Then
                        Dim ownersTable As New ArrayList()

                        For Each owner As KaOwner In KaOwner.GetOwnersWithKeyword(connection, Nothing, keyword, includeInterfaceCrossReferences)
                            ownersTable.Add(New ArrayList({"<a href=""Owners.aspx?OwnerId=" & owner.Id.ToString & """>" & Server.HtmlEncode(owner.Name) & "</a>"}))
                        Next
                        If ownersTable.Count > 0 Then
                            ownersTable.Insert(0, New ArrayList({"Name"}))
                            entityTable = KaReports.GetTableHtml("Owners", "", ownersTable, False, "", headerRowAttributes, New List(Of String), "", New List(Of String), True)
                        End If
                    End If
                Case "panels"
                    If Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaPanel.TABLE_NAME}), "Panels")(KaPanel.TABLE_NAME).Read Then
                        Dim panelsTable As New ArrayList()

                        For Each panel As KaPanel In KaPanel.GetPanelsWithKeyword(connection, Nothing, keyword)
                            panelsTable.Add(New ArrayList({"<a href=""Panels.aspx?PanelId=" & panel.Id.ToString & """>" & Server.HtmlEncode(panel.Name) & "</a>"}))
                        Next
                        If panelsTable.Count > 0 Then
                            panelsTable.Insert(0, New ArrayList({"Name"}))
                            entityTable = KaReports.GetTableHtml("Panels", "", panelsTable, False, "", headerRowAttributes, New List(Of String), "", New List(Of String), True)
                        End If
                    End If
                Case "panel groups"
                    If Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaPanel.TABLE_NAME}), "Panels")(KaPanel.TABLE_NAME).Read Then
                        Dim panelGroupsTable As New ArrayList()

                        For Each panelGroup As KaPanelGroup In KaPanelGroup.GetPanelGroupsWithKeyword(connection, Nothing, keyword)
                            panelGroupsTable.Add(New ArrayList({"<a href=""PanelGroups.aspx?PanelGroupId=" & panelGroup.Id.ToString & """>" & Server.HtmlEncode(panelGroup.Name) & "</a>"}))
                        Next
                        If panelGroupsTable.Count > 0 Then
                            panelGroupsTable.Insert(0, New ArrayList({"Name"}))
                            entityTable = KaReports.GetTableHtml("Panel Groups", "", panelGroupsTable, False, "", headerRowAttributes, New List(Of String), "", New List(Of String), True)
                        End If
                    End If
                Case "products"
                    If Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaProduct.TABLE_NAME}), "Products")(KaProduct.TABLE_NAME).Read Then
                        Dim productsTable As New ArrayList()

                        For Each product As KaProduct In KaProduct.GetProductsWithKeyword(connection, Nothing, _currentUser.OwnerId, keyword, includeInterfaceCrossReferences)
                            productsTable.Add(New ArrayList({"<a href=""Products.aspx?ProductId=" & product.Id.ToString & """>" & Server.HtmlEncode(product.Name) & "</a>"}))
                        Next
                        If productsTable.Count > 0 Then
                            productsTable.Insert(0, New ArrayList({"Name"}))
                            entityTable = KaReports.GetTableHtml("Products", "", productsTable, False, "", headerRowAttributes, New List(Of String), "", New List(Of String), True)
                        End If
                    End If
                Case "product groups"
                    If Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaProduct.TABLE_NAME}), "Products")(KaProduct.TABLE_NAME).Read Then
                        Dim productGroupsTable As New ArrayList()

                        For Each productGroup As KaProductGroup In KaProductGroup.GetProductGroupsWithKeyword(connection, Nothing, keyword)
                            productGroupsTable.Add(New ArrayList({"<a href=""ProductGroups.aspx?ProductGroupId=" & productGroup.Id.ToString & """>" & Server.HtmlEncode(productGroup.Name) & "</a>"}))
                        Next
                        If productGroupsTable.Count > 0 Then
                            productGroupsTable.Insert(0, New ArrayList({"Name"}))
                            entityTable = KaReports.GetTableHtml("Product Groups", "", productGroupsTable, False, "", headerRowAttributes, New List(Of String), "", New List(Of String), True)
                        End If
                    End If
                Case "receiving purchase orders"
                    If Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaReceivingPurchaseOrder.TABLE_NAME}), "PurchaseOrders")(KaReceivingPurchaseOrder.TABLE_NAME).Read Then
                        Dim sql As String = GenerateReceivingOrdersQuery(connection, False)

                        Dim reportDataList As ArrayList = KaReports.GetReceivingPurchaseOrdersTable(connection, sql, KaReports.MEDIA_TYPE_HTML, False, Page.Request.Url.ToString)
                        If reportDataList.Count > 1 Then
                            Dim headerRowList As ArrayList = reportDataList(0) 'This will be the header row
                            Dim tableAttributes As String = "border=1; width=100%;"
                            headerRowAttributes = ""
                            Dim rowAttributes As String = ""

                            Dim headerCellAttributeList As New List(Of String)
                            Dim detailCellAttributeList As New List(Of String)
                            ' KaReports.GetOrderListHtmlTableFormatting(tableAttributes, headerRowAttributes, rowAttributes, headerCellAttributeList, detailCellAttributeList, headerRowList)

                            entityTable = KaReports.GetTableHtml("Purchase orders", "", reportDataList, False, tableAttributes, "", headerCellAttributeList, "", detailCellAttributeList)
                        End If
                    End If
                Case "past receiving purchase orders"
                    If Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaReceivingPurchaseOrder.TABLE_NAME}), "PurchaseOrders")(KaReceivingPurchaseOrder.TABLE_NAME).Read Then
                        Dim sql As String = GenerateReceivingOrdersQuery(connection, True)

                        Dim reportDataList As ArrayList = KaReports.GetReceivingPurchaseOrdersTable(connection, sql, KaReports.MEDIA_TYPE_HTML, False, Page.Request.Url.ToString)
                        If reportDataList.Count > 1 Then
                            Dim headerRowList As ArrayList = reportDataList(0) 'This will be the header row
                            Dim tableAttributes As String = "border=1; width=100%;"
                            headerRowAttributes = ""
                            Dim rowAttributes As String = ""

                            Dim headerCellAttributeList As New List(Of String)
                            Dim detailCellAttributeList As New List(Of String)
                            ' KaReports.GetOrderListHtmlTableFormatting(tableAttributes, headerRowAttributes, rowAttributes, headerCellAttributeList, detailCellAttributeList, headerRowList)

                            entityTable = KaReports.GetTableHtml("Past purchase orders", "", reportDataList, False, tableAttributes, "", headerCellAttributeList, "", detailCellAttributeList)
                        End If
                    End If
                Case "receiving activity report"
                    If Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaReceivingPurchaseOrder.TABLE_NAME}), "PurchaseOrders")(KaReceivingPurchaseOrder.TABLE_NAME).Read Then
                        Dim unit As KaUnit = New KaUnit(connection, KaUnit.GetSystemDefaultMassUnitOfMeasure(connection, Nothing))
                        Dim col As UInt64 = 0
                        col += 2 ^ KaReports.ReceivingActivityReportColumns.RcDateTime
                        col += 2 ^ KaReports.ReceivingActivityReportColumns.RcOrderNumber
                        col += 2 ^ KaReports.ReceivingActivityReportColumns.RcTicketNumber
                        col += 2 ^ KaReports.ReceivingActivityReportColumns.RcSupplier
                        col += 2 ^ KaReports.ReceivingActivityReportColumns.RcOwner

                        Dim url As String = ""
                        If Utilities.ConvertWebPageUrlDomainToRequestedPagesDomain(GetUserConnection(_currentUser.Id)) Then url = Tm2Database.GetCurrentPageUrlWithOriginalPort(Me.Request)
                        entityTable = KaReports.GetReceivingActivityTable(connection, KaReports.MEDIA_TYPE_HTML, GenerateReceivingTicketsQuery(), "Receiving PO Tickets", col, unit, "", "", "", New List(Of KaUnit), url, False, False, True)
                    End If
                Case "seals"
                    If Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaSeal.TABLE_NAME}), "Seals")(KaSeal.TABLE_NAME).Read Then
                        Dim sealsTable As New ArrayList()

                        For Each seal As KaSeal In KaSeal.GetSealsWithKeyword(connection, Nothing, keyword)
                            sealsTable.Add(New ArrayList({Server.HtmlEncode(seal.Number)}))
                        Next
                        If sealsTable.Count > 0 Then
                            sealsTable.Insert(0, New ArrayList({"Number"}))
                            entityTable = KaReports.GetTableHtml("Seals", "", sealsTable, False, "", headerRowAttributes, New List(Of String), "", New List(Of String), True)
                        End If
                    End If
                Case "suppliers"
                    If Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaReceivingPurchaseOrder.TABLE_NAME}), "PurchaseOrders")(KaReceivingPurchaseOrder.TABLE_NAME).Read Then
                        Dim supplierAccountsTable As New ArrayList()

                        For Each supplierAccount As KaSupplierAccount In KaSupplierAccount.GetSupplierAccountsWithKeyword(connection, Nothing, _currentUser.OwnerId, keyword, includeInterfaceCrossReferences)
                            supplierAccountsTable.Add(New ArrayList({"<a href=""Suppliers.aspx?SupplierAccountId=" & supplierAccount.Id.ToString & """>" & Server.HtmlEncode(supplierAccount.Name) & "</a>", Server.HtmlEncode(supplierAccount.AccountNumber), Server.HtmlEncode(supplierAccount.Street), Server.HtmlEncode(supplierAccount.City), Server.HtmlEncode(supplierAccount.State), Server.HtmlEncode(supplierAccount.ZipCode), Server.HtmlEncode(supplierAccount.Phone)}))
                        Next
                        If supplierAccountsTable.Count > 0 Then
                            supplierAccountsTable.Insert(0, New ArrayList({"Name", "Default cross reference", "Street", "City", "State", "ZipCode", "Phone"}))
                            entityTable = KaReports.GetTableHtml("Suppliers", "", supplierAccountsTable, False, "", headerRowAttributes, New List(Of String), "", New List(Of String), True)
                        End If
                    End If
                Case "tanks"
                    If Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaTank.TABLE_NAME}), "Tanks")(KaTank.TABLE_NAME).Read Then
                        Dim tanksTable As New ArrayList()

                        For Each tank As KaTank In KaTank.GetTanksWithKeyword(connection, Nothing, _currentUser.OwnerId, keyword, includeInterfaceCrossReferences)
                            tanksTable.Add(New ArrayList({"<a href=""Tanks.aspx?TankId=" & tank.Id.ToString & """>" & Server.HtmlEncode(tank.Name) & "</a>"}))
                        Next
                        If tanksTable.Count > 0 Then
                            tanksTable.Insert(0, New ArrayList({"Name"}))
                            entityTable = KaReports.GetTableHtml("Tanks", "", tanksTable, False, "", headerRowAttributes, New List(Of String), "", New List(Of String), True)
                        End If
                    End If
                Case "tank groups"
                    If Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaTank.TABLE_NAME}), "Tanks")(KaTank.TABLE_NAME).Read Then
                        Dim tankGroupsTable As New ArrayList()

                        For Each tankGroup As KaTankGroup In KaTankGroup.GettankGroupsWithKeyword(connection, Nothing, keyword)
                            tankGroupsTable.Add(New ArrayList({"<a href=""TankGroups.aspx?TankGroupId=" & tankGroup.Id.ToString & """>" & Server.HtmlEncode(tankGroup.Name) & "</a>"}))
                        Next
                        If tankGroupsTable.Count > 0 Then
                            tankGroupsTable.Insert(0, New ArrayList({"Name"}))
                            entityTable = KaReports.GetTableHtml("Tank Groups", "", tankGroupsTable, False, "", headerRowAttributes, New List(Of String), "", New List(Of String), True)
                        End If
                    End If
                Case "receipts"
                    If Utilities.GetUserPagePermission(_currentUser, New List(Of String)({"reports"}), "Reports")("reports").Read Then
                        Dim unit As KaUnit = New KaUnit(connection, KaUnit.GetSystemDefaultMassUnitOfMeasure(connection, Nothing))
                        Dim col As UInt64 = 0
                        col += 2 ^ KaReports.CustomerActivityReportColumns.RcDateTime
                        col += 2 ^ KaReports.CustomerActivityReportColumns.RcOrderNumber
                        col += 2 ^ KaReports.CustomerActivityReportColumns.RcTicketNumber
                        col += 2 ^ KaReports.CustomerActivityReportColumns.RcCustomer
                        col += 2 ^ KaReports.CustomerActivityReportColumns.RcCustomerDestination
                        col += 2 ^ KaReports.CustomerActivityReportColumns.RcOwner
                        col += 2 ^ KaReports.CustomerActivityReportColumns.RcBranch
                        col += 2 ^ KaReports.CustomerActivityReportColumns.RcFacility
                        col += 2 ^ KaReports.CustomerActivityReportColumns.RcDriver
                        col += 2 ^ KaReports.CustomerActivityReportColumns.RcTransport
                        col += 2 ^ KaReports.CustomerActivityReportColumns.RcCarrier

                        Dim url As String = ""
                        If Utilities.ConvertWebPageUrlDomainToRequestedPagesDomain(GetUserConnection(_currentUser.Id)) Then url = Tm2Database.GetCurrentPageUrlWithOriginalPort(Me.Request)
                        entityTable = KaReports.GetCustomerActivityTable(connection, KaReports.MEDIA_TYPE_HTML, GenerateTicketsQuery(), CustomerActivityReportProductDisplayOptions.ProductAsColumn, "Tickets", col, unit, "", "", "", New List(Of KaUnit), url, False, False, True)
                    End If
                Case "tracks"
                    If Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaTransport.TABLE_NAME}), "Transports")(KaTransport.TABLE_NAME).Read Then
                        Dim TracksTable As New ArrayList()

                        For Each Track As KaTrack In KaTrack.GetTracksWithKeyword(connection, Nothing, keyword)
                            TracksTable.Add(New ArrayList({"<a href=""Tracks.aspx?TrackId=" & Track.Id.ToString & """>" & Server.HtmlEncode(Track.Name) & "</a>"}))
                        Next
                        If TracksTable.Count > 0 Then
                            TracksTable.Insert(0, New ArrayList({"Name"}))
                            entityTable = KaReports.GetTableHtml("Tracks", "", TracksTable, False, "", headerRowAttributes, New List(Of String), "", New List(Of String), True)
                        End If
                    End If
                Case "transports"
                    If Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaTransport.TABLE_NAME}), "Transports")(KaTransport.TABLE_NAME).Read Then
                        Dim transportsTable As New ArrayList()

                        For Each transport As KaTransport In KaTransport.GetTransportsWithKeyword(connection, Nothing, keyword, includeInterfaceCrossReferences)
                            transportsTable.Add(New ArrayList({"<a href=""Transports.aspx?TransportId=" & transport.Id.ToString & """>" & Server.HtmlEncode(transport.Name) & "</a>", Server.HtmlEncode(transport.Number)}))
                        Next
                        If transportsTable.Count > 0 Then
                            transportsTable.Insert(0, New ArrayList({"Name", "Number"}))
                            entityTable = KaReports.GetTableHtml("Transports", "", transportsTable, False, "", headerRowAttributes, New List(Of String), "", New List(Of String), True)
                        End If
                    End If
                Case "transport types"
                    If Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaTransport.TABLE_NAME}), "Transports")(KaTransport.TABLE_NAME).Read Then
                        Dim transportTypesTable As New ArrayList()

                        For Each transportType As KaTransportTypes In KaTransportTypes.GetTransportTypesWithKeyword(connection, Nothing, keyword, includeInterfaceCrossReferences)
                            transportTypesTable.Add(New ArrayList({"<a href=""TransportTypes.aspx?TransportTypeId=" & transportType.Id.ToString & """>" & Server.HtmlEncode(transportType.Name) & "</a>"}))
                        Next
                        If transportTypesTable.Count > 0 Then
                            transportTypesTable.Insert(0, New ArrayList({"Name"}))
                            entityTable = KaReports.GetTableHtml("TransportTypes", "", transportTypesTable, False, "", headerRowAttributes, New List(Of String), "", New List(Of String), True)
                        End If
                    End If
                Case "units"
                    If Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaUnit.TABLE_NAME}), "Units")(KaUnit.TABLE_NAME).Read Then
                        Dim unitsTable As New ArrayList()

                        For Each unit As KaUnit In KaUnit.GetUnitsWithKeyword(connection, Nothing, keyword, includeInterfaceCrossReferences)
                            unitsTable.Add(New ArrayList({"<a href=""Units.aspx?UnitId=" & unit.Id.ToString & """>" & Server.HtmlEncode(unit.Name) & "</a>", Server.HtmlEncode(unit.Abbreviation)}))
                        Next
                        If unitsTable.Count > 0 Then
                            unitsTable.Insert(0, New ArrayList({"Name", "Abbreviation"}))
                            entityTable = KaReports.GetTableHtml("Units", "", unitsTable, False, "", headerRowAttributes, New List(Of String), "", New List(Of String), True)
                        End If
                    End If
                Case "users"
                    If Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaUser.TABLE_NAME}), "Users")(KaUser.TABLE_NAME).Read Then
                        Dim usersTable As New ArrayList()

                        For Each user As KaUser In KaUser.GetUsersWithKeyword(connection, Nothing, _currentUser.OwnerId, keyword)
                            usersTable.Add(New ArrayList({"<a href=""Users.aspx?UserId=" & user.Id.ToString & """>" & Server.HtmlEncode(user.Name) & "</a>", Server.HtmlEncode(user.Username)}))
                        Next
                        If usersTable.Count > 0 Then
                            usersTable.Insert(0, New ArrayList({"Name", "Username"}))
                            entityTable = KaReports.GetTableHtml("Users", "", usersTable, False, "", headerRowAttributes, New List(Of String), "", New List(Of String), True)
                        End If
                    End If
                Case Else
                    ' There wasn't one found, try the menu item
                    Dim titleSplit() As String = _sourceTitle.Split(":")
                    If entityType <> titleSplit(0).Trim Then AddEntityType(connection, titleSplit(0).Trim, keyword, entities, includeInterfaceCrossReferences)
            End Select
            entities.Add(entityType, entityTable)
        End If
    End Sub
End Class