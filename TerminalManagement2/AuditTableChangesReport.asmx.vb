Imports System.Web.Services
Imports System.Web.Services.Protocols
Imports System.ComponentModel
Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb
Imports System.Xml.Serialization

' To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line.
' <System.Web.Script.Services.ScriptService()> _
<System.Web.Services.WebService(Namespace:="KahlerAutomation")> _
<System.Web.Services.WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<ToolboxItem(False)> _
Public Class AuditTableChangesReport
    Inherits System.Web.Services.WebService

    <WebMethod()> _
    Public Function GetAuditedDataReport(ByVal reportId As Guid) As ArrayList
        Dim connection As OleDbConnection = Tm2Database.Connection
        Dim report As New KaEmailReport(connection, reportId)
        Dim fromDate As DateTime = report.LastSent
        Dim toDate As DateTime = DateTime.Now ' The custom reports always run until today, there is no end date... report.GetEndDateFromReportTriggers(DateTime.Now)
        Dim parameters As String = KaSetting.GetSetting(connection, "AuditTableChangesReport:" & reportId.ToString, "")
        Dim applicationIdentifier As String = EmailReports.GetParameter("audit_application", report.ReportParameters, "")
        Dim tableName As String = EmailReports.GetParameter("table_name", report.ReportParameters, "")
        Dim auditType As KaReports.AuditedDataType = EmailReports.GetParameter("audit_type", report.ReportParameters, KaReports.AuditedDataType.All)
        Dim username As String = EmailReports.GetParameter("audit_user", report.ReportParameters, "")

        Return KaReports.GetAuditedDataReport(connection, fromDate, toDate, tableName, applicationIdentifier, username, auditType)
    End Function
End Class