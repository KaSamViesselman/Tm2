Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class InterfaceReceivingTicketStatusView : Inherits System.Web.UI.Page
    Private _currentUser As KaUser
    Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
    Private _currentTableName As String = KaOrder.TABLE_NAME

#Region "Events"
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Response.Cache.SetCacheability(HttpCacheability.NoCache)
        _currentUser = Utilities.GetUser(Me)
        _currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Orders")
        If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")

        Dim header As String = Server.UrlDecode(Request.QueryString("header"))
        Dim userId As String = Server.UrlDecode(Request.QueryString("user_id"))
        Dim interfaceId As String = Server.UrlDecode(Request.QueryString("interface_id"))
        Dim sortBy As String = Server.UrlDecode(Request.QueryString("sort_by"))
        Dim showTicketsExported As Boolean
        Dim includeTicketsMarkedManually As Boolean
        Dim includeTicketsWithError As Boolean
        Dim includeTicketsWithIgnoredError As Boolean
        Dim limitOrdersToInterface As Boolean
        Boolean.TryParse(Request.QueryString("tickets_exported"), showTicketsExported)
        Boolean.TryParse(Request.QueryString("tickets_marked_manually"), includeTicketsMarkedManually)
        Boolean.TryParse(Request.QueryString("tickets_with_error"), includeTicketsWithError)
        Boolean.TryParse(Request.QueryString("tickets_with_ignored_error"), includeTicketsWithIgnoredError)
        Boolean.TryParse(Request.QueryString("interface_orders_only"), limitOrdersToInterface)

        If Not Page.IsPostBack Then
            litReport.Text = KaReports.GetTableHtml(header, "", KaReports.GetInterfaceReceivingTicketExportStatusReport(GetUserConnection(New Guid(userId)), New Guid(interfaceId), sortBy, showTicketsExported, includeTicketsMarkedManually, includeTicketsWithError, includeTicketsWithIgnoredError, limitOrdersToInterface))
        End If
    End Sub
#End Region
End Class