Imports KahlerAutomation.KaTm2Database

Public Class TransportTrackingReportPFV : Inherits System.Web.UI.Page
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim currentUserId As Guid = Guid.Parse(Request.QueryString("current_user_id"))
        Dim orderBy As String = Request.QueryString("order_by")
        Dim ascDesc As String = Request.QueryString("asc_desc")
        Dim displayUnitId As Guid = Guid.Parse(Request.QueryString("display_unit_id"))
        Dim connection As OleDb.OleDbConnection = GetUserConnection(currentUserId)
        KaReports.GetTransportTrackingReport(connection, tblTransports, KaReports.MEDIA_TYPE_HTML, orderBy & " " & ascDesc, displayUnitId, Utilities.ConvertWebPageUrlDomainToRequestedPagesDomain(connection))
    End Sub
End Class