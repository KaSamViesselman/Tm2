Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb
Imports System.Environment
Imports System.Math
Public Class CustomerActivityReportView : Inherits System.Web.UI.Page
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
		Dim unit As New KaUnit(connection, Guid.Parse(Request.QueryString("unit_id")))
		Dim decimalsDisplayed As Integer = 0
		If Request.QueryString("decimals_displayed") IsNot Nothing AndAlso Integer.TryParse(Request.QueryString("decimals_displayed"), decimalsDisplayed) Then
			' Set the unit's formatting
			unit.UnitPrecision = "#,###,###0" & IIf(decimalsDisplayed > 0, ".".PadRight(decimalsDisplayed + 1, "0"), "")
		End If

		Dim totalUnits As New List(Of KaUnit)
		Dim totalUnitsAndDecimals As String = Request.QueryString("total_units_displayed")
		If totalUnitsAndDecimals IsNot Nothing Then
			For Each unitItem As String In totalUnitsAndDecimals.Split("|")
				Dim values() As String = unitItem.Split(":")
				Try
					Dim totalUnit As New KaUnit(connection, Guid.Parse(values(0)))
					decimalsDisplayed = 0
					If Integer.TryParse(values(1), decimalsDisplayed) Then
						totalUnit.UnitPrecision = "#,###,###0" & IIf(decimalsDisplayed > 0, ".".PadRight(decimalsDisplayed + 1, "0"), "")
					End If
					If Not totalUnits.Contains(totalUnit) Then totalUnits.Add(totalUnit)
				Catch ex As Exception

				End Try
			Next
		End If

		Dim productDisplay As KaReports.CustomerActivityReportProductDisplayOptions = CustomerActivityReportProductDisplayOptions.ProductAsColumn
		If Request.QueryString("product_display") IsNot Nothing Then productDisplay = Integer.Parse(Request.QueryString("product_display"))

		Dim queryHeader As String() = GetQueryAndHeaderFromUrl()
		Dim query As String = queryHeader(0)
		Dim columns As UInt64 = Request.QueryString("columns")
		Dim header As String = queryHeader(1)
		Dim tableDisplayAttributes As String = "border=""1""" ' "Style=""border-width: 1px 1px 1px 1px; border-spacing: 2px; border-style: outset outset outset outset; border-color: black black black black; border-collapse: collapse; background-color: white;"
		Dim tableRowDisplayAttributes As String = "" ' "Style=""border-width: 1px 1px 1px 1px; padding: 3px 3px 3px 3px; border-style: inset inset inset inset; border-color: gray gray gray gray; background-color: white; -moz-border-radius: 0px 0px 0px 0px;"
		Dim tableDetailDisplayAttributes As String = "" ' "Style=""border-width: 1px 1px 1px 1px; padding: 3px 3px 3px 3px; border-style: inset inset inset inset; border-color: gray gray gray gray; background-color: white; -moz-border-radius: 0px 0px 0px 0px;"
		Dim mediaType As String = KaReports.MEDIA_TYPE_HTML
		If Request.QueryString("media_type") IsNot Nothing Then mediaType = Request.QueryString("media_type")

		Dim url As String = ""
		If Utilities.ConvertWebPageUrlDomainToRequestedPagesDomain(GetUserConnection(_currentUser.Id)) Then url = Tm2Database.GetCurrentPageUrlWithOriginalPort(Me.Request)

		litReport.Text = KaReports.GetCustomerActivityTable(connection, mediaType, query, productDisplay, header, columns, unit, tableDisplayAttributes, tableRowDisplayAttributes, tableRowDisplayAttributes, totalUnits, url, True, True, False)
	End Sub

	''' <returns>At index 0 is the query and index 1 stores the header</returns>
	Private Function GetQueryAndHeaderFromUrl() As String()
		Dim interfaceId As String = ""
		Dim fromDate As String
		Dim toDate As String
		Dim productDisplayIndex As Integer = 0
		Dim sortIndex As Integer = 0
		Dim includeVoidedTickets As Boolean = False
		Dim ticketNumber As Boolean = False
		If Request.QueryString("interface_id") IsNot Nothing Then interfaceId = Server.HtmlDecode(Request.QueryString("interface_id"))
		fromDate = Server.HtmlDecode(Request.QueryString("from_date"))
		toDate = Server.HtmlDecode(Request.QueryString("to_date"))
		Integer.TryParse(Server.HtmlDecode(Request.QueryString("product_display")), productDisplayIndex)
		Integer.TryParse(Server.HtmlDecode(Request.QueryString("sort")), sortIndex)
		Boolean.TryParse(Server.HtmlDecode(Request.QueryString("include_void")), includeVoidedTickets)
		If includeVoidedTickets Then
			ticketNumber = True
		Else
			Boolean.TryParse(Server.HtmlDecode(Request.QueryString("ticket_number")), ticketNumber)
		End If

		Dim customerAccountId As Guid, transportId As Guid, ownerId As Guid, customerDestinationId As Guid, productId As Guid, branchId As Guid, facilityId As Guid, bayId As Guid, driverId As Guid, carrierId As Guid, applicatorId As Guid, currentUserId As Guid
		Dim userName As String = ""
		'TryParse will ensure invalid values are set to Guid.empty 
		Guid.TryParse(Server.HtmlDecode(Request.QueryString("customer_account_id")), customerAccountId)
		Guid.TryParse(Server.HtmlDecode(Request.QueryString("transport_id")), transportId)
		Guid.TryParse(Server.HtmlDecode(Request.QueryString("owner_id")), ownerId)
		Guid.TryParse(Server.HtmlDecode(Request.QueryString("customer_destination_id")), customerDestinationId)
		Guid.TryParse(Server.HtmlDecode(Request.QueryString("product_id")), productId)
		Guid.TryParse(Server.HtmlDecode(Request.QueryString("branch_id")), branchId)
		Guid.TryParse(Server.HtmlDecode(Request.QueryString("facility_id")), facilityId)
		Guid.TryParse(Server.HtmlDecode(Request.QueryString("bay_id")), bayId)
		Guid.TryParse(Server.HtmlDecode(Request.QueryString("driver_id")), driverId)
		Guid.TryParse(Server.HtmlDecode(Request.QueryString("carrier_id")), carrierId)
		Guid.TryParse(Server.HtmlDecode(Request.QueryString("applicator_id")), applicatorId)
		Guid.TryParse(Server.HtmlDecode(Request.QueryString("current_user_id")), currentUserId)
		If Request.QueryString("username") IsNot Nothing Then userName = Server.HtmlDecode(Request.QueryString("username"))

		Return {CustomerActivityReport.GenerateQuery(interfaceId, customerAccountId, transportId, ownerId, customerDestinationId, productId, branchId, facilityId, bayId, driverId, carrierId, applicatorId, userName, productDisplayIndex, sortIndex, includeVoidedTickets, ticketNumber, fromDate, toDate), CustomerActivityReport.GenerateHeader(interfaceId, customerAccountId.ToString, transportId.ToString, ownerId.ToString, customerDestinationId.ToString, productId.ToString, branchId.ToString, facilityId.ToString, bayId.ToString, driverId.ToString, carrierId.ToString, userName, currentUserId, fromDate, toDate)}
	End Function


End Class