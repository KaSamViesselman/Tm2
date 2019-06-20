Imports KahlerAutomation.KaTm2Database

Public Class ProductAllocationView : Inherits System.Web.UI.Page

    Private _currentUser As KaUser
    Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
    Private _currentTableName As String = KaProduct.TABLE_NAME


#Region "Events"
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Response.Cache.SetCacheability(HttpCacheability.NoCache)
        _currentUser = Utilities.GetUser(Me)
        _currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Products")
        If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")

        If Not Page.IsPostBack Then
            Dim facilityId As Guid = Guid.Parse(Request.QueryString("facility_id"))
            Dim accountId As Guid = Guid.Parse(Request.QueryString("account_id"))
            Dim productId As Guid = Guid.Parse(Request.QueryString("product_id"))
            Dim viewUnitId As Guid = Guid.Parse(Request.QueryString("display_unit_id"))
            Dim onlyShowProductsWithBulkProductsAtLocation As Boolean = False
            If Request.QueryString("show_prods_with_formula") IsNot Nothing Then Boolean.TryParse(Request.QueryString("show_prods_with_formula"), onlyShowProductsWithBulkProductsAtLocation)
            PopulateReport(facilityId, accountId, productId, viewUnitId, onlyShowProductsWithBulkProductsAtLocation)
        End If
    End Sub
#End Region

    Private Sub PopulateReport(ByVal facilityId As Guid, ByVal accountId As Guid, ByVal productId As Guid, ByVal viewUnitId As Guid, ByVal onlyShowProductsWithBulkProductsAtLocation As Boolean)
        'Build subtitle
        Dim subTitle As String = "Facility: " & New KaLocation(GetUserConnection(_currentUser.Id), facilityId).Name

        If accountId <> Guid.Empty Then
            subTitle &= ", Customer: " & New KaCustomerAccount(GetUserConnection(_currentUser.Id), accountId).Name
        End If

        If productId <> Guid.Empty Then
            subTitle &= ", Product: " & New KaProduct(GetUserConnection(_currentUser.Id), productId).Name
        End If

        If viewUnitId <> Guid.Empty Then
            subTitle &= ", Displaying in: " & New KaUnit(GetUserConnection(_currentUser.Id), viewUnitId).Name & "."
        Else
            subTitle &= ", Displaying in product default unit of measure."
        End If
        litReport.Text = KaReports.GetTableHtml("Assigned Products", subTitle, GetProductAllocationTable(GetUserConnection(_currentUser.Id), accountId, productId, viewUnitId, facilityId, onlyShowProductsWithBulkProductsAtLocation))
    End Sub
End Class