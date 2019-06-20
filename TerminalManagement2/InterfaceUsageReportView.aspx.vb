Imports KahlerAutomation.KaTm2Database

Public Class InterfaceUsageReportView
    Inherits System.Web.UI.Page

    Private _currentUser As KaUser
    Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
    Private _currentTableName As String = KaBulkProductInventory.TABLE_NAME

#Region "Events"
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Response.Cache.SetCacheability(HttpCacheability.NoCache)
        _currentUser = Utilities.GetUser(Me)
        _currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Inventory")
        If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")

        If Not Page.IsPostBack Then PopulateReport(_currentUser)
    End Sub
#End Region

    Private Sub PopulateReport(ByVal currentUser As KaUser)
        litReport.Text = KaReports.GetTableHtml(IIf(Page.Request("ReportTitle") IsNot Nothing AndAlso Page.Request("ReportTitle").Trim.Length > 0, "Interface Usage Report - " & Page.Request("ReportTitle"), ""), KaReports.GetInterfaceUsageReport(GetUserConnection(currentUser.Id), Page.Request("ReportType"), currentUser.OwnerId))
    End Sub
End Class