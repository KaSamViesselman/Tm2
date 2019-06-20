Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class AllTransportsView : Inherits System.Web.UI.Page

    Private _currentUser As KaUser
    Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing

#Region "Events"
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Response.Cache.SetCacheability(HttpCacheability.NoCache)
        _currentUser = Utilities.GetUser(Me)
        _currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaTransport.TABLE_NAME}), "Transports")
        If Not _currentUserPermission(KaTransport.TABLE_NAME).Read Then Response.Redirect("Welcome.aspx")
        If Not Page.IsPostBack Then PopulateReport()
    End Sub
#End Region

    Private Sub PopulateReport()
        litReport.Text = KaReports.GetTableHtml("All transports", KaReports.GetTransportTable(GetUserConnection(_currentUser.Id)))
    End Sub
End Class