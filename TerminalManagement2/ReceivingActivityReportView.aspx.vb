Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb
Imports System.Environment
Imports System.Math

Public Class ReceivingActivityReportView : Inherits System.Web.UI.Page

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
        Dim queryHeader As String() = GetQueryAndHeader()
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
        litReport.Text = KaReports.GetReceivingActivityTable(connection, mediaType, query, header, columns, unit, tableDisplayAttributes, tableRowDisplayAttributes, tableRowDisplayAttributes, totalUnits, url, True, True, False)
    End Sub

    Private Function GetQueryAndHeader() As String()
        Dim supplierAccountId As String = Server.HtmlDecode(Request.QueryString("supplier_account_id")),
            bulkProductId As String = Server.HtmlDecode(Request.QueryString("bulk_product_id")),
            ownerId As String = Server.HtmlDecode(Request.QueryString("owner_id")),
            facilityId As String = Server.HtmlDecode(Request.QueryString("facility_id")),
            driverId As String = Server.HtmlDecode(Request.QueryString("driver_id")),
            transportId As String = Server.HtmlDecode(Request.QueryString("transport_id")),
            carrierId As String = Server.HtmlDecode(Request.QueryString("carrier_id")),
            fromDate As String = Server.HtmlDecode(Request.QueryString("from_date")),
            toDate As String = Server.HtmlDecode(Request.QueryString("to_date")),
            currentUserId As String = Server.HtmlDecode(Request.QueryString("current_user_id")),
        sortIndex As Integer, includeVoidedTickets As Boolean, ticketNumber As Boolean

        If supplierAccountId Is Nothing Then supplierAccountId = Guid.Empty.ToString
        If bulkProductId Is Nothing Then bulkProductId = Guid.Empty.ToString
        If ownerId Is Nothing Then ownerId = Guid.Empty.ToString
        If facilityId Is Nothing Then facilityId = Guid.Empty.ToString
        If driverId Is Nothing Then driverId = Guid.Empty.ToString
        If transportId Is Nothing Then transportId = Guid.Empty.ToString
        If carrierId Is Nothing Then carrierId = Guid.Empty.ToString

        Integer.TryParse(Server.HtmlDecode(Request.QueryString("sort")), sortIndex)
        Boolean.TryParse(Server.HtmlDecode(Request.QueryString("include_void")), includeVoidedTickets)
        Boolean.TryParse(Server.HtmlDecode(Request.QueryString("ticket_number")), ticketNumber)

        Return {ReceivingActivityReport.GenerateQuery(supplierAccountId, bulkProductId, ownerId, facilityId, driverId, transportId, carrierId, sortIndex, includeVoidedTickets, ticketNumber, fromDate, toDate), ReceivingActivityReport.GenerateHeader(supplierAccountId, bulkProductId, ownerId, facilityId, driverId, transportId, carrierId, fromDate, toDate, currentUserId)}
    End Function

End Class