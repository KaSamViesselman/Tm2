Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class ReceivingPurchaseOrderListView : Inherits System.Web.UI.Page
	Private _currentUser As KaUser
	Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
	Private _currentTableName As String = KaOrder.TABLE_NAME

#Region "Events"
	Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
		Response.Cache.SetCacheability(HttpCacheability.NoCache)
		_currentUser = Utilities.GetUser(Me)
		_currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "PurchaseOrders")
		If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")

		Dim title As String = "Receiving Purchase Order List"
		If Request.QueryString("title") IsNot Nothing Then title = Request.QueryString("title")
		Dim subtitle As String = ""
		If Request.QueryString("subtitle") IsNot Nothing Then subtitle = Request.QueryString("subtitle")
		Dim sortBy As String = Request.QueryString("sortby")

		Dim selectedLocationId As Guid = Guid.Empty
		Guid.TryParse(Request.QueryString("location_id"), selectedLocationId)
		Dim selectedSupplierId As Guid = Guid.Parse(Request.QueryString("supplier_id"))
		Dim selectedOwnerId As Guid = Guid.Parse(Request.QueryString("owner_id"))
		Dim selectedBulkProductId As Guid = Guid.Parse(Request.QueryString("bulk_product_id"))
		Dim url As String = ""
        If Utilities.ConvertWebPageUrlDomainToRequestedPagesDomain(GetUserConnection(_currentUser.Id)) Then url = Tm2Database.GetCurrentPageUrlWithOriginalPort(Me.Request)

        If Not Page.IsPostBack Then
			Dim reportDataList As ArrayList = KaReports.GetReceivingPurchaseOrdersListTable(GetUserConnection(_currentUser.Id), selectedOwnerId, KaReports.MEDIA_TYPE_HTML, sortBy, selectedSupplierId, selectedBulkProductId, url, True, selectedLocationId)

			Dim headerRowList As ArrayList = reportDataList(0) 'This will be the header row
			Dim tableAttributes As String = "border=1; width=100%;"
			Dim headerRowAttributes As String = ""
			Dim rowAttributes As String = ""

			Dim headerCellAttributeList As New List(Of String)
			Dim detailCellAttributeList As New List(Of String)
			KaReports.GetReceivingPurchaseOrderListHtmlTableFormatting(tableAttributes, headerRowAttributes, rowAttributes, headerCellAttributeList, detailCellAttributeList, headerRowList)

			litReport.Text = KaReports.GetTableHtml(title, subtitle, reportDataList, False, tableAttributes, "", headerCellAttributeList, "", detailCellAttributeList)
		End If
	End Sub
#End Region
End Class