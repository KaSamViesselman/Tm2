Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb
Imports System.Environment
Imports System.Math

Public Class PrintTickets : Inherits System.Web.UI.Page
    Private _currentUser As KaUser
    Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
    Private _currentTableName As String = "reports"

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Response.Cache.SetCacheability(HttpCacheability.NoCache)
        _currentUser = Utilities.GetUser(Me)
        _currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "reports")
        If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")
        If Not Page.IsPostBack Then
            PopulateReport()
        End If
    End Sub

    Private Sub PopulateReport()
        Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
        Dim ticketIds As New List(Of Guid) ' Guid = ticket ID
        Dim query As String = Server.HtmlDecode(Request.QueryString("query"))
        If (Not String.IsNullOrEmpty(query)) Then
            Dim r As OleDbDataReader = Tm2Database.ExecuteReader(connection, query)
            Do While r.Read()
                Dim ticketId As Guid = r(0)
                If Not ticketIds.Contains(ticketId) Then ticketIds.Add(ticketId)
            Loop
            r.Close()
            For Each ticketId As Guid In ticketIds
                Dim webTicketAddress As String = WebTicketUrlForOwner(connection, GetOwnerIdForTicket(connection, ticketId))
                If webTicketAddress.ToLower.IndexOf("ticket_id=") < 0 Then
                    If webTicketAddress.IndexOf("?") >= 0 Then
                        webTicketAddress &= "&"
                    Else
                        webTicketAddress &= "?"
                    End If
                    webTicketAddress &= "ticket_id="
                End If
                webTicketAddress &= ticketId.ToString()
                If Utilities.ConvertWebPageUrlDomainToRequestedPagesDomain(connection) Then webTicketAddress = Tm2Database.GetUrlInCurrentDomain(Tm2Database.GetCurrentPageUrlWithOriginalPort(Me.Request), webTicketAddress)
                lblTicketSources.Text &= "," & webTicketAddress
            Next
            lblTicketSources.Text = lblTicketSources.Text.Substring(1)
        End If
    End Sub
End Class