Imports KahlerAutomation.KaTm2Database

Public Class ProductListView : Inherits System.Web.UI.Page
    Private _currentUser As KaUser
    Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
    Private _currentTableName As String = KaProduct.TABLE_NAME

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Response.Cache.SetCacheability(HttpCacheability.NoCache)
        _currentUser = Utilities.GetUser(Me)
        _currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Products")
        If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")
        Dim locationId As Guid
        Dim locationName As String
        Try
            locationId = Guid.Parse(Request.QueryString("location_id"))
            If locationId <> Guid.Empty Then
                locationName = " for " & New KaLocation(GetUserConnection(_currentUser.Id), locationId).Name
            Else
                locationName = ""
            End If
        Catch ex As ArgumentNullException
            locationId = Guid.Empty
            locationName = ""
        End Try
        litReport.Text = String.Format("<h1>Product List{0} as of {1:d}</h1>{2}", locationName, Now, KaReports.GetProductListReportHtml(GetUserConnection(_currentUser.Id), _currentUser.OwnerId, locationId))
    End Sub
End Class