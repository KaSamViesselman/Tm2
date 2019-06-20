Imports KahlerAutomation.KaTm2Database

Public Class BulkProductAllocationView
    Inherits System.Web.UI.Page

    Private _currentUser As KaUser

#Region "Events"
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Response.Cache.SetCacheability(HttpCacheability.NoCache)
        _currentUser = Utilities.GetUser(Me)
        If Not Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaProduct.TABLE_NAME}), "Products")(KaProduct.TABLE_NAME).Read Then Response.Redirect("Welcome.aspx")

        If Not Page.IsPostBack Then
            Dim facilityId As Guid = Guid.Parse(Request.QueryString("facility_id"))
            Dim accountId As Guid = Guid.Parse(Request.QueryString("account_id"))
            Dim bulkProductId As Guid = Guid.Parse(Request.QueryString("bulk_product_id"))
            Dim viewUnitId As Guid = Guid.Parse(Request.QueryString("display_unit_id"))
            PopulateReport(facilityId, accountId, bulkProductId, viewUnitId)
        End If
    End Sub
#End Region

    Private Sub PopulateReport(ByVal facilityId As Guid, ByVal accountId As Guid, ByVal bulkProductId As Guid, ByVal viewUnitId As Guid)
        'Build subtitle
        Dim subTitle As String = "Facility: " & New KaLocation(GetUserConnection(_currentUser.Id), facilityId).Name

        If accountId <> Guid.Empty Then
            subTitle &= ", Customer: " & New KaCustomerAccount(GetUserConnection(_currentUser.Id), accountId).Name
        End If

        If bulkProductId <> Guid.Empty Then
            subTitle &= ", Bulk Product: " & New KaBulkProduct(GetUserConnection(_currentUser.Id), bulkProductId).Name
        End If

        If viewUnitId <> Guid.Empty Then
            subTitle &= ", Displaying in: " & New KaUnit(GetUserConnection(_currentUser.Id), viewUnitId).Name & "."
        Else
            subTitle &= ", Displaying in bulk product default unit of measure."
        End If
        litReport.Text = KaReports.GetTableHtml("Assigned Bulk Products", subTitle, GetBulkProductAllocationTable(GetUserConnection(_currentUser.Id), accountId, bulkProductId, viewUnitId, facilityId))
    End Sub
End Class