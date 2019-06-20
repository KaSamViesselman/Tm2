Imports KahlerAutomation.KaTm2Database

Public Class DriverHistoryView
    Inherits System.Web.UI.Page

    Private _currentUser As KaUser
    Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
    Private _currentTableName As String = KaDriver.TABLE_NAME


#Region "Events"
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Response.Cache.SetCacheability(HttpCacheability.NoCache)
        _currentUser = Utilities.GetUser(Me)
        _currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Drivers")
        If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")
        Dim startDate As DateTime = DateTime.Parse(Page.Request("start_date"))
        Dim endDate As DateTime = DateTime.Parse(Page.Request("end_date"))

        litReport.Text = KaReports.GetTableHtml("Driver history for dates " & startDate.ToString("G") & " to " & endDate.ToString("G"), KaReports.GetDriverHistoryTable(GetUserConnection(_currentUser.Id), startDate, endDate))
    End Sub
#End Region
End Class