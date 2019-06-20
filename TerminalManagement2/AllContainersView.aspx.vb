Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class AllContainersView : Inherits System.Web.UI.Page

	Private _currentUser As KaUser
	Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
	Private _currentTableName As String = KaContainer.TABLE_NAME

#Region "Events"
	Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
		Response.Cache.SetCacheability(HttpCacheability.NoCache)

		If Request.QueryString("orderBy") Is Nothing Then Exit Sub

		_currentUser = Utilities.GetUser(Me)
		_currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Containers")
		If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")
		Dim search As String = ""
		If Request.QueryString("search") IsNot Nothing Then search = Request.QueryString("search")
		Dim locationId As Guid = Guid.Empty
		If Request.QueryString("locationId") IsNot Nothing Then Guid.TryParse(Request.QueryString("locationId"), locationId)
		Dim status As String = ""
		If Request.QueryString("status") IsNot Nothing Then status = Request.QueryString("status")
		Dim productId As Guid = Guid.Empty
		If Request.QueryString("productId") IsNot Nothing Then Guid.TryParse(Request.QueryString("productId"), productId)
		Dim ownerId As Guid = Guid.Empty
		If Request.QueryString("ownerId") IsNot Nothing Then Guid.TryParse(Request.QueryString("ownerId"), ownerId)
		Dim customerId As Guid = Guid.Empty
		If Request.QueryString("customerId") IsNot Nothing Then Guid.TryParse(Request.QueryString("customerId"), customerId)
		Dim showDeleted As Boolean = False
		If Request.QueryString("showDeleted") IsNot Nothing Then Boolean.TryParse(Request.QueryString("showDeleted"), showDeleted)
		Dim orderBy As String = Request.QueryString("orderBy")
		Dim reportMediaType As String = KaReports.MEDIA_TYPE_HTML
		If Request.QueryString("reportMediaType") IsNot Nothing Then reportMediaType = Request.QueryString("reportMediaType")
		Dim columnsDisplayed As ULong = AllContainers.GetCurrentColumnSetting(_currentUser.Id)
		If Request.QueryString("columnsDisplayed") IsNot Nothing Then columnsDisplayed = Request.QueryString("columnsDisplayed")

		Dim tableAttributes As String = ""
		Dim headerRowAttributes As String = ""
		Dim headerCellAttributes As New List(Of String)
		Dim rowAttributes As String = ""
		Dim columnAttributes As New List(Of String)
		Dim cellAttributes As New List(Of String)

		Dim url As String = ""
		If Utilities.ConvertWebPageUrlDomainToRequestedPagesDomain(GetUserConnection(_currentUser.Id)) Then url = Tm2Database.GetCurrentPageUrlWithOriginalPort(Me.Request)
		Dim containersTable As ArrayList = KaReports.GetContainerTable(GetUserConnection(_currentUser.Id), reportMediaType, GenerateWhereClause(search, locationId, status, productId, ownerId, customerId, showDeleted), orderBy, -1, -1, url, columnsDisplayed, tableAttributes, headerRowAttributes, headerCellAttributes, rowAttributes, columnAttributes, cellAttributes)
		litContainers.Text = KaReports.GetTableHtml("", "", containersTable, False, tableAttributes, headerRowAttributes, headerCellAttributes, rowAttributes, columnAttributes, cellAttributes, True)
	End Sub
#End Region

	Private Function GenerateWhereClause(ByVal search As String, ByVal locationId As Guid, ByVal status As String, ByVal productId As Guid, ByVal ownerId As Guid, ByVal customerId As Guid, ByVal showDeleted As Boolean) As String
		Dim conditions As String = "(containers.number LIKE '%" & search & "%')"
		If Not locationId.Equals(Guid.Empty) Then _
			conditions &= " AND (containers.location_id=" & Q(locationId) & ")"
		If status.Length > 0 Then _
			conditions &= " AND (containers.status = " & Q(status) & ")"
		If Not productId.Equals(Guid.Empty) Then _
			conditions &= " AND (containers.bulk_product_id = " & Q(productId) & ")"
		If Not ownerId.Equals(Guid.Empty) Then _
			conditions &= " AND (containers.owner_id = " & Q(ownerId) & ")"
		If Not customerId.Equals(Guid.Empty) Then _
			conditions &= " AND (orders.deleted = 0)" &
				" AND (order_customer_accounts.deleted = 0)" &
				" AND (order_customer_accounts.customer_account_id = " & Q(customerId) & ")"
		If Not showDeleted Then _
			conditions &= " AND (containers.deleted = 0)"

		Dim validContainers As String = ""
		Dim getValidContainersRdr As OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), "SELECT DISTINCT containers.id " &
														 "FROM containers " &
														 IIf(Not customerId.Equals(Guid.Empty), "INNER JOIN orders ON orders.id = containers.for_order_id " &
																"INNER JOIN order_customer_accounts ON orders.id = order_customer_accounts.order_id ", "") &
														 "WHERE " & conditions)
		Do While getValidContainersRdr.Read
			validContainers &= IIf(validContainers.Length > 0, ",", "") & Q(getValidContainersRdr.Item("id"))
		Loop
		getValidContainersRdr.Close()

		If validContainers.Length > 0 Then
			Return "id in (" & validContainers & ")"
		Else
			Return "id=" & Q(Guid.Empty)
		End If
	End Function


End Class