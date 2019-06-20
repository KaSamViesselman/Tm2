Imports KahlerAutomation.KaTm2Database

Public Class TrackReportView
    Inherits System.Web.UI.Page
    Private _currentUser As KaUser
    Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
    Private _currentTableName As String = "reports"

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Response.Cache.SetCacheability(HttpCacheability.NoCache)
        _currentUser = Utilities.GetUser(Me)
        _currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "reports")
        If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")
        Dim startDate As DateTime = DateTime.Parse(Page.Request("start_date"))
        Dim endDate As DateTime = DateTime.Parse(Page.Request("end_date"))
        Dim trackId As Guid = Guid.Parse(Page.Request("track_id"))
        Dim showOperator As Boolean = Boolean.Parse(Page.Request("show_operator"))
        Dim showRfid As Boolean = Boolean.Parse(Page.Request("show_rfid"))
        Dim showCarNumber As Boolean = Boolean.Parse(Page.Request("show_car_number"))
        Dim showTrack As Boolean = Boolean.Parse(Page.Request("show_track"))
        Dim showScanTime As Boolean = Boolean.Parse(Page.Request("show_scan_time"))
        Dim showReverseOrder As Boolean = Boolean.Parse(Page.Request("show_reverse_order"))

        Dim caption As String = "Track report for dates " & startDate.ToString("d") & " to " & endDate.ToString("d")
        If Not trackId.Equals(Guid.Empty) Then caption &= " for " & New KaTrack(GetUserConnection(_currentUser.Id), trackId).Name
        litReport.Text = KaReports.GetTableHtml(caption, KaReports.GetTrackReportTable(GetUserConnection(_currentUser.Id), startDate, endDate.AddDays(1), trackId, showOperator, showRfid, showCarNumber, showTrack, showScanTime, showReverseOrder))
    End Sub

End Class