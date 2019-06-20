Imports KahlerAutomation.KaTm2Database

Public Class InventoryChangeReportView
    Inherits System.Web.UI.Page

    Private _currentUser As KaUser
    Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
    Private _currentTableName As String = KaBulkProductInventory.TABLE_NAME

#Region "Events"
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Response.Cache.SetCacheability(HttpCacheability.NoCache)
        _currentUser = Utilities.GetUser(Me)
        _currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Inventory")
        If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")

        If Not Page.IsPostBack Then PopulateReport(_currentUser)
    End Sub
#End Region

    Private Sub PopulateReport(ByVal currentUser As KaUser)
        Dim connection As OleDb.OleDbConnection = GetUserConnection(currentUser.Id)
        Dim ownerId As Guid = Guid.Empty
        If Page.Request("OwnerId") IsNot Nothing Then Guid.TryParse(Page.Request("OwnerId"), ownerId)
        Dim locationId As Guid = Guid.Empty
        If Page.Request("LocationId") IsNot Nothing Then Guid.TryParse(Page.Request("LocationId"), locationId)
        Dim bulkProductId As Guid = Guid.Empty
        If Page.Request("BulkProductId") IsNot Nothing Then Guid.TryParse(Page.Request("BulkProductId"), bulkProductId)
        Dim fromDate As DateTime = DateTime.Now.AddDays(-1)
        If Page.Request("FromDate") IsNot Nothing Then DateTime.TryParse(Page.Request("FromDate"), fromDate)
        Dim toDate As DateTime = DateTime.Now
        If Page.Request("ToDate") IsNot Nothing Then DateTime.TryParse(Page.Request("ToDate"), toDate)
        Dim additionalUnitsString As String = ""
        If Page.Request("AdditionalUnits") IsNot Nothing Then additionalUnitsString = Page.Request("AdditionalUnits")
        Dim additionalUnits As List(Of Guid) = GetAdditionalUnitsFromString(additionalUnitsString)

        Dim tableAttributes As String = "border=1; width=100%;"
        Dim headerRowAttributes As String = ""
        Dim rowAttributes As String = ""
		Dim reportDataList As ArrayList = KaReports.GetInventoryChangeTable(connection, ownerId, locationId, bulkProductId, fromDate, toDate, additionalUnits, KaReports.MEDIA_TYPE_HTML)
		Dim headerRowList As ArrayList = reportDataList(0) 'This will be the header row

        Dim headerCellAttributeList As New List(Of String)
        Dim detailCellAttributeList As New List(Of String)
        Dim columnCount As Integer = 0
        For i = 1 To 4
            headerCellAttributeList.Add("style=""font-weight: bold; text-align: left;""")
            detailCellAttributeList.Add("style=""text-align: left;""")
            columnCount += 1
        Next

        For i = 5 To 7
            headerCellAttributeList.Add("style=""font-weight: bold; text-align: right;""")
            detailCellAttributeList.Add("style=""text-align: right;""")
            columnCount += 1
        Next
        For i = 8 To headerRowList.Count - 1
            headerCellAttributeList.Add("style=""font-weight: bold; text-align: left;""")
            detailCellAttributeList.Add("style=""text-align: left;""")
            columnCount += 1
        Next

        Dim reportTitle As String = ""
        Try
            reportTitle &= IIf(reportTitle.Length > 0, ", ", "") & "Facility: " & New KaLocation(connection, locationId).Name
        Catch ex As Exception

        End Try
        Try
            reportTitle &= IIf(reportTitle.Length > 0, ", ", "") & "Owner: " & New KaOwner(connection, ownerId).Name
        Catch ex As Exception

        End Try
        Try
            reportTitle &= IIf(reportTitle.Length > 0, ", ", "") & "Bulk product: " & New KaBulkProduct(connection, bulkProductId).Name
        Catch ex As Exception

        End Try
        If reportTitle.Length > 0 Then reportTitle = "For " & reportTitle & ", "

        litReport.Text = KaReports.GetTableHtml("Inventory Change Report", reportTitle & "Date: " & fromDate.ToString() & " to " & toDate.ToString(), reportDataList, False, tableAttributes, headerRowAttributes, headerCellAttributeList, rowAttributes, detailCellAttributeList)
    End Sub

    Private Function GetAdditionalUnitsFromString(ByVal additionalUnitsString As String) As List(Of Guid)
        Dim retval As List(Of Guid) = New List(Of Guid)
        If additionalUnitsString.Trim.Length > 0 Then
            For Each unitId As String In additionalUnitsString.Split(",")
                retval.Add(Guid.Parse(unitId))
            Next
        End If
        Return retval
    End Function
End Class