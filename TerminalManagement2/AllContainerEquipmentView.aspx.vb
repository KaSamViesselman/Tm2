Imports System.Data.OleDb
Imports KahlerAutomation.KaTm2Database

Public Class AllContainerEquipmentView : Inherits System.Web.UI.Page

    Private _currentUser As KaUser
    Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
    Private _currentTableName As String = KaContainer.TABLE_NAME

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Response.Cache.SetCacheability(HttpCacheability.NoCache)
        _currentUser = Utilities.GetUser(Me)
        _currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Containers")
        If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")
        Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
        Dim sortCriteria As String
        Const SORT_CRITERIA_KEY As String = "sort_criteria"
        If Request.QueryString(SORT_CRITERIA_KEY) IsNot Nothing Then
            sortCriteria = Request.QueryString(SORT_CRITERIA_KEY)
        Else ' caller didn't specify how to sort the results
            sortCriteria = ""
        End If
        Dim locationId As Guid
        Const LOCATION_ID_KEY As String = "location_id"
        If Request.QueryString(LOCATION_ID_KEY) IsNot Nothing Then
            locationId = Guid.Parse(Request.QueryString(LOCATION_ID_KEY))
        Else ' caller didn't specify a location
            locationId = Guid.Empty
        End If
        Dim status As Integer
        Const STATUS_KEY As String = "status"
        If Request.QueryString(STATUS_KEY) IsNot Nothing Then
            status = Integer.Parse(Request.QueryString(STATUS_KEY))
        Else ' caller didn't specify a status
            status = -1
        End If
        Dim ownerId As Guid
        Const OWNER_ID_KEY As String = "owner_id"
        If Request.QueryString(OWNER_ID_KEY) IsNot Nothing Then
            ownerId = Guid.Parse(Request.QueryString(OWNER_ID_KEY))
        Else ' caller didn't specify an owner
            ownerId = Guid.Empty
        End If
        Dim customerAccountId As Guid
        Const CUSTOMER_ACCOUNT_ID_KEY As String = "customer_account_id"
        If Request.QueryString(CUSTOMER_ACCOUNT_ID_KEY) IsNot Nothing Then
            customerAccountId = Guid.Parse(Request.QueryString(CUSTOMER_ACCOUNT_ID_KEY))
        Else ' caller didn't specify a customer account
            customerAccountId = Guid.Empty
        End If
        Dim number As String
        Const NUMBER_KEY As String = "number"
        If Request.QueryString(NUMBER_KEY) IsNot Nothing Then
            number = Request.QueryString(NUMBER_KEY)
        Else ' caller didn't specify a number
            number = ""
        End If
        Dim showDeleted As Boolean
        Const SHOW_DELETED_KEY As String = "show_deleted"
        If Request.QueryString(SHOW_DELETED_KEY) IsNot Nothing Then
            showDeleted = Boolean.Parse(Request.QueryString(SHOW_DELETED_KEY))
        Else ' caller didn't specify that we should show deleted records
            showDeleted = False
        End If

        Dim tableAttributes As String = ""
        Dim headerRowAttributes As String = ""
        Dim headerCellAttributes As New List(Of String)
        Dim rowAttributes As String = ""
        Dim columnAttributes As New List(Of String)
        Dim cellAttributes As New List(Of String)

        Dim url As String = ""
        If Utilities.ConvertWebPageUrlDomainToRequestedPagesDomain(GetUserConnection(_currentUser.Id)) Then url = Tm2Database.GetCurrentPageUrlWithOriginalPort(Me.Request)
        Dim containersTable As ArrayList = KaReports.GetContainerEquipmentTable(connection, Request.QueryString("media_type"), KaReports.GetContainerEquipmentConditions(connection, locationId, ownerId, customerAccountId, status, number, showDeleted), sortCriteria, -1, -1, tableAttributes, headerRowAttributes, headerCellAttributes, rowAttributes, columnAttributes, cellAttributes)
        litReport.Text = KaReports.GetTableHtml("", "", containersTable, False, tableAttributes, headerRowAttributes, headerCellAttributes, rowAttributes, columnAttributes, cellAttributes, True)
    End Sub
End Class