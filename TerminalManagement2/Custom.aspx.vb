Imports KahlerAutomation.KaTm2Database

Public Class Custom : Inherits System.Web.UI.Page
    Private _currentUser As KaUser

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Response.Cache.SetCacheability(HttpCacheability.NoCache)
        _currentUser = Utilities.GetUser(Me)
        If Not Page.IsPostBack Then
            Dim url As String = ""
            If Request.QueryString("url") IsNot Nothing Then url = CStr(Request.QueryString("url"))
            If Request.QueryString("page_title") IsNot Nothing Then Title = CStr(Request.QueryString("page_title"))
            Dim pageId As Guid = Guid.Empty
            Dim customPage As KaCustomPages = New KaCustomPages
            Try
                Dim connection As OleDb.OleDbConnection = GetUserConnection(_currentUser.Id)
                Guid.TryParse(Request.QueryString("pageId"), pageId)
                customPage = New KaCustomPages(connection, pageId)

                url = customPage.PageURL
                If Utilities.ConvertWebPageUrlDomainToRequestedPagesDomain(connection) Then url = Tm2Database.GetUrlInCurrentDomain(Tm2Database.GetCurrentPageUrlWithOriginalPort(Me.Request), url)

                If customPage.EmailReport Then
                    Page.Title = "Reports : " & customPage.PageLabel
                Else
                    Page.Title = customPage.PageLabel & " : " & customPage.PageLabel
                    If customPage.MainMenuLink AndAlso Not Utilities.GetUserPagePermission(_currentUser, New List(Of String)({pageId.ToString}), pageId.ToString)(pageId.ToString).Read Then Response.Redirect("Welcome.aspx")
                End If
            Catch ex As Exception

            End Try
            customPageFrame.Attributes("src") = url & IIf(url.Contains("?"), "&", "?") & "userId=" & _currentUser.Id.ToString & "&pageId=" & pageId.ToString & IIf(customPage.MainMenuLink, "&CreateMainMenu=false", "")
        End If
    End Sub
End Class