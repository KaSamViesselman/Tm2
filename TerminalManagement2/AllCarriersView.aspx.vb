Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class AllCarriersView : Inherits System.Web.UI.Page
    Private _currentUser As KaUser
    Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
    Private _currentTableName As String = KaCarrier.TABLE_NAME

#Region "Events"
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Response.Cache.SetCacheability(HttpCacheability.NoCache)
        _currentUser = Utilities.GetUser(Me)
        _currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Carriers")
        If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")
        If Not Page.IsPostBack Then PopulateReport()
    End Sub
#End Region

    Private Sub PopulateReport()
        litReport.Text = KaReports.GetTableHtml("All carriers", KaReports.GetCarrierTable(GetUserConnection(_currentUser.Id)))
    End Sub
End Class