Imports KahlerAutomation.KaTm2Database

Public Class BulkProductAnalysis : Inherits System.Web.UI.Page
    Private _currentUser As KaUser
    Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
    Private _currentTableName As String = KaProduct.TABLE_NAME

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Response.Cache.SetCacheability(HttpCacheability.NoCache)
        _currentUser = Utilities.GetUser(Me)
        _currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Products")
        If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")

        If Not Page.IsPostBack Then
            PopulateBulkProducts()
            PopulateAnalysisCombo()
        End If
    End Sub

    Private Sub PopulateBulkProducts()
        Dim li As ListItem = New ListItem
        li.Value = Guid.Empty.ToString
        li.Text = ""
        ddlBulkProducts.Items.Add(li)
        For Each bulkProd As KaBulkProduct In KaBulkProduct.GetAll(GetUserConnection(_currentUser.Id), "deleted = " & Q(False), "name asc")
            If Not bulkProd.IsFunction(GetUserConnection(_currentUser.Id)) Then
                li = New ListItem
                li.Value = bulkProd.Id.ToString
                li.Text = bulkProd.Name
                ddlBulkProducts.Items.Add(li)
            End If
        Next
    End Sub

    Private Sub PopulateAnalysisCombo()
        Dim analysisTypes As New SortedDictionary(Of String, String)

        analysisTypes.Add("Default ethanol analysis", "DefaultEthanolAnalysis")
        analysisTypes.Add("Default grain analysis", "DefaultGrainAnalysis")
        analysisTypes.Add("Default chemical analysis", "DefaultChemicalAnalysis")
        analysisTypes.Add("Default hazardous materials analysis", "DefaultHazmatAnalysis")
        For Each customPage As KaCustomPages In KaCustomPages.GetAll(GetUserConnection(_currentUser.Id), "bulk_product_analysis = " & Q(True) & " and deleted = " & Q(False), "page_label asc")
            analysisTypes.Add(customPage.PageLabel, customPage.Id.ToString())
        Next

        ddlBulkProductAnalysis.Items.Clear()
        For Each typeName As String In analysisTypes.Keys
            ddlBulkProductAnalysis.Items.Add(New ListItem() With {.Value = analysisTypes(typeName), .Text = typeName})
        Next
        SelectDefaultAnalysis()
    End Sub

    Private Sub SelectDefaultAnalysis()
        Dim defaultAnalysis As String = KaSetting.GetSetting(GetUserConnection(_currentUser.Id), "General/DefaultBulkProductAnalysis", "", False, Nothing)
        Try
            ddlBulkProductAnalysis.SelectedValue = defaultAnalysis
        Catch ex As ArgumentOutOfRangeException

        End Try
    End Sub

    Private Sub ddlBulkProductAnalysis_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlBulkProductAnalysis.SelectedIndexChanged
        PopulateAnalysisIFrame()
    End Sub

    Private Sub PopulateAnalysisIFrame()
        Dim url As String = ""
        Dim selectedAnanlysis As Object = ddlBulkProductAnalysis.SelectedValue

        Try
            Dim selectedAnalysisGuid As Guid = Guid.Parse(selectedAnanlysis)
            Dim customPage As KaCustomPages = New KaCustomPages(GetUserConnection(_currentUser.Id), selectedAnalysisGuid)
            url = customPage.PageURL
            If Utilities.ConvertWebPageUrlDomainToRequestedPagesDomain(GetUserConnection(_currentUser.Id)) Then url = Tm2Database.GetUrlInCurrentDomain(Tm2Database.GetCurrentPageUrlWithOriginalPort(Me.Request), url)
        Catch ex As Exception
            url = CType(selectedAnanlysis, String) & ".aspx"
        End Try

        If url <> "" Then
            analysisFrame.Attributes("src") = url & "?record_id=" & ddlBulkProducts.SelectedValue & "&table_name=" & KaBulkProduct.TABLE_NAME & "&template_id=" & selectedAnanlysis
        End If
        Dim removeAnalysisVisible As Boolean = False
        Dim rdr As OleDb.OleDbDataReader = Nothing
        Try
            rdr = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), $"SELECT COUNT(*) FROM analysis WHERE deleted = 0 AND record_id={Q(ddlBulkProducts.SelectedValue)} AND table_name={Q(KaBulkProduct.TABLE_NAME)} AND template_id={Q(selectedAnanlysis)}")
            If rdr.Read() AndAlso rdr.Item(0) > 0 Then removeAnalysisVisible = True
        Finally
            If rdr IsNot Nothing Then rdr.Close()
        End Try
        pnlRecordControl.Visible = removeAnalysisVisible
    End Sub

    Private Sub ddlBulkProducts_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlBulkProducts.SelectedIndexChanged
        If Guid.Parse(ddlBulkProducts.SelectedValue) = Guid.Empty Then
            ddlBulkProductAnalysis.Enabled = False
            analysisFrame.Visible = False
        Else
            ddlBulkProductAnalysis.Enabled = True
            analysisFrame.Visible = True
            Dim bulkProd As KaBulkProduct = New KaBulkProduct(GetUserConnection(_currentUser.Id), Guid.Parse(ddlBulkProducts.SelectedValue))
            If bulkProd.AnalysisTypeId <> Guid.Empty Then
                Dim analysisType As KaAnalysisTypes = New KaAnalysisTypes(GetUserConnection(_currentUser.Id), bulkProd.AnalysisTypeId)
                Try
                    ddlBulkProductAnalysis.SelectedValue = analysisType.TemplateNameId
                Catch ex As Exception
                    'If it fails, the analysis type probably wasn't in the list (deleted possibly)
                End Try
            End If
            PopulateAnalysisIFrame()
        End If
    End Sub

    Protected Sub btnRemoveAnalysisRecord_Click(sender As Object, e As EventArgs) Handles btnRemoveAnalysisRecord.Click
        Dim selectedAnanlysis As Object = ddlBulkProductAnalysis.SelectedValue
        Tm2Database.ExecuteNonQuery(GetUserConnection(_currentUser.Id), $"UPDATE analysis SET deleted = 1 WHERE id IN (SELECT TOP 1 id FROM analysis WHERE (deleted = 0) AND (record_id = {Q(ddlBulkProducts.SelectedValue)}) AND (table_name = {Q(KaBulkProduct.TABLE_NAME)}) AND (template_id = {Q(selectedAnanlysis)}) ORDER BY analyzed_at DESC)")
        PopulateAnalysisIFrame()
    End Sub
End Class