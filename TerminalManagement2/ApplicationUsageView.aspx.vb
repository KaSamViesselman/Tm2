Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class ApplicationUsageView : Inherits System.Web.UI.Page
    Private _currentUser As KaUser

#Region "Events"
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Response.Cache.SetCacheability(HttpCacheability.NoCache)
        _currentUser = Utilities.GetUser(Me)
        If Not Page.IsPostBack Then PopulateReport()
    End Sub
#End Region

    Private Sub PopulateReport()
        litReport.Text = KaReports.GetTableHtml("Application Usage", KaReports.GetApplicationUsageTable(GetUserConnection(_currentUser.Id))) & _
            "<br /><hr /><br />" & _
            "Current Database Version: " & KaSetting.GetSetting(GetUserConnection(_currentUser.Id), KaSetting.SN_DATABASE_VERSION, "0")
    End Sub
End Class