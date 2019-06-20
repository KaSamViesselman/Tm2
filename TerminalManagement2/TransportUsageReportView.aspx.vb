Imports KahlerAutomation.KaTm2Database

Public Class TransportUsageReportView : Inherits System.Web.UI.Page
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not Utilities.GetUserPagePermission(Utilities.GetUser(Me), New List(Of String)({KaTransport.TABLE_NAME}), "Transports")(KaTransport.TABLE_NAME).Read Then Response.Redirect("Welcome.aspx")
        Dim startDate As DateTime = IIf(Request.QueryString("start_date") <> Nothing, DateTime.Parse(Request.QueryString("start_date")), DateTime.Now)
        Dim endDate As DateTime = IIf(Request.QueryString("end_date") <> Nothing, DateTime.Parse(Request.QueryString("end_date")), DateTime.Now)
        Dim customerAccountId As Guid = IIf(Request.QueryString("customer_account_id") <> Nothing, Guid.Parse(Request.QueryString("customer_account_id")), Guid.Empty)
        Dim transportId As Guid = IIf(Request.QueryString("transport_id") <> Nothing, Guid.Parse(Request.QueryString("transport_id")), Guid.Empty)
        Dim table As ArrayList = KaReports.GetTransportUsageReportTable(GetUserConnection(Utilities.GetUser(Me).Id), startDate, endDate, customerAccountId, transportId)
        litReport.Text = KaReports.GetTableHtml(String.Format("Transport usage report ({0:d} to {1:d})", startDate, endDate), table)
    End Sub
End Class