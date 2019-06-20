Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class ValidLoadsView : Inherits System.Web.UI.Page
    Private _currentUser As KaUser
    Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
    Private _currentTableName As String = KaOrder.TABLE_NAME

#Region "Events"
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Response.Cache.SetCacheability(HttpCacheability.NoCache)
        _currentUser = Utilities.GetUser(Me)
        _currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Orders")
        If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")

        Dim title As String = "Order List"
        If Request.QueryString("title") IsNot Nothing Then title = Request.QueryString("title")
        Dim subtitle As String = ""
        If Request.QueryString("subtitle") IsNot Nothing Then subtitle = Request.QueryString("subtitle")
        Dim sortBy As String = Request.QueryString("sortby")
        Dim selectedAccountId As Guid = Guid.Parse(Request.QueryString("account_id"))
        Dim selectedOwnerId As Guid = Guid.Parse(Request.QueryString("owner_id"))
        Dim selectedFacilityId As Guid = Guid.Parse(Request.QueryString("facility_id"))
        Dim reportType As KaReports.OrderListReportType
        Select Case Request.QueryString("report_type")
            Case KaReports.OrderListReportType.OneProductPerLine.ToString
                reportType = OrderListReportType.OneProductPerLine
            Case KaReports.OrderListReportType.MultipleProductsOneColumn.ToString
                reportType = OrderListReportType.MultipleProductsOneColumn
            Case Else
                reportType = OrderListReportType.OneProductPerColumn
        End Select
        Dim showLockedColumn As Boolean = Boolean.Parse(Request.QueryString("show_locked_column"))
        Dim orderLockedFilter As Integer = Integer.Parse(Request.QueryString("locked_filter"))
        If Not Page.IsPostBack Then
            Dim reportDataList As ArrayList = KaReports.GetOrdersTable(GetUserConnection(_currentUser.Id), selectedOwnerId, KaReports.MEDIA_TYPE_PFV, sortBy, selectedAccountId, selectedFacilityId, reportType, showLockedColumn, orderLockedFilter)

            Dim headerRowList As ArrayList = reportDataList(0) 'This will be the header row
            Dim tableAttributes As String = "border=1; width=100%;"
            Dim headerRowAttributes As String = ""
            Dim rowAttributes As String = ""

            Dim headerCellAttributeList As New List(Of String)
            Dim detailCellAttributeList As New List(Of String)
            KaReports.GetOrderListHtmlTableFormatting(tableAttributes, headerRowAttributes, rowAttributes, headerCellAttributeList, detailCellAttributeList, headerRowList)

            litReport.Text = KaReports.GetTableHtml(title, "", reportDataList, False, tableAttributes, "", headerCellAttributeList, "", detailCellAttributeList)
        End If
    End Sub
#End Region
End Class