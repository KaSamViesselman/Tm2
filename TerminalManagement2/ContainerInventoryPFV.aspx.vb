Imports KahlerAutomation.KaTm2Database
Public Class ContainerInventoryPFV
    Inherits System.Web.UI.Page
#Region "Events"
    Private _currentUser As KaUser
    Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
    Private _currentTableName As String = KaBulkProductInventory.TABLE_NAME

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Response.Cache.SetCacheability(HttpCacheability.NoCache)
        _currentUser = Utilities.GetUser(Me)
        _currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Inventory")
        If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")

        PopulateReport(_currentUser)
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
        Dim onlyNonzeroEntries As Boolean = False
        If Page.Request("OnlyNonzero") IsNot Nothing Then Boolean.TryParse(Page.Request("OnlyNonzero"), onlyNonzeroEntries)
        Dim additionalUnitIdsString As String = ""
        If Page.Request("AdditionalUnitsString") IsNot Nothing Then additionalUnitIdsString = Page.Request("AdditionalUnitsString")
        Dim bulkProductIds As New List(Of Guid)
        If Page.Request("inventory_group_id") IsNot Nothing Then
            Try
                Dim inventoryGroup As New KaInventoryGroup(connection, Guid.Parse(Page.Request("inventory_group_id")))
                For Each bulkProd As KaInventoryGroupBulkProduct In inventoryGroup.BulkProducts
                    If Not bulkProd.Deleted AndAlso Not bulkProd.Equals(Guid.Empty) AndAlso Not bulkProductIds.Contains(bulkProd.BulkProductId) Then bulkProductIds.Add(bulkProd.BulkProductId)
                Next
            Catch ex As RecordNotFoundException

            End Try
        ElseIf Not bulkProductId.Equals(Guid.Empty) Then
            bulkProductIds.Add(bulkProductId)
        End If
        Dim currentStatus As Integer = -1
        If Page.Request("CurrentStatus") IsNot Nothing Then Integer.TryParse(Page.Request("CurrentStatus"), currentStatus)

        Dim tableAttributes As String = "border=1; width=100%;"
        Dim headerRowAttributes As String = ""
        Dim rowAttributes As String = ""

        Dim reportDataList As ArrayList = KaReports.GetContainerInventoryTable(connection, ownerId, locationId, bulkProductIds, onlyNonzeroEntries, KaReports.MEDIA_TYPE_PFV, Me.Page.Request.Url.AbsolutePath, currentStatus, GetAdditionalUnitIds(additionalUnitIdsString))
        Dim headerRowList As ArrayList = reportDataList(0) 'This will be the header row

        Dim headerCellAttributeList As New List(Of String)
        Dim detailCellAttributeList As New List(Of String)
        Dim columnCount As Integer = 0
        For i = 0 To 3
            headerCellAttributeList.Add("style=""font-weight: bold; text-align: left;""")
            detailCellAttributeList.Add("style=""text-align: left;""")
            columnCount += 1
        Next

        For i = 4 To headerRowList.Count - 1
            headerCellAttributeList.Add("style=""font-weight: bold; text-align: right;""")
            detailCellAttributeList.Add("style=""text-align: right;""")
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
        If reportTitle.Length > 0 Then reportTitle = "For " & reportTitle

        litReport.Text = KaReports.GetTableHtml("Container Inventory Report", reportTitle, reportDataList, True, tableAttributes, headerRowAttributes, headerCellAttributeList, rowAttributes, detailCellAttributeList)
    End Sub

    Private Function GetAdditionalUnitIds(ByVal additionalUnitsString As String) As List(Of Guid)
        Dim retval As List(Of Guid) = New List(Of Guid)
        If additionalUnitsString.Trim.Length > 0 Then
            For Each additionalUnit As String In additionalUnitsString.Split(",")
                retval.Add(Guid.Parse(additionalUnit))
            Next
        End If
        Return retval
    End Function
End Class