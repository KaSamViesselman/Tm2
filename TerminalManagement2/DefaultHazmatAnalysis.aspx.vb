Imports KahlerAutomation.KaTm2Database

Public Class DefaultHazmatAnalysis
    Inherits System.Web.UI.Page
    Private _currentUser As KaUser
    Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
    Private _currentTableName As String = KaProduct.TABLE_NAME
    Private _recordId As Guid
    Private _tableName As String
    Private _templateId As String

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        _currentUser = Utilities.GetUser(Me)
        _currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Products")
        If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")
        _recordId = Guid.Parse(Request.QueryString("record_id"))
        _tableName = Request.QueryString("table_name")
        _templateId = Request.QueryString("template_id")
        If _recordId = Guid.Empty Then
            lblStatus.Text = "Invalid Record GUID"
            Exit Sub
        End If
        btnSave.Enabled = _currentUserPermission(_currentTableName).Edit
        If Not Page.IsPostBack Then
            PopulateUnitList()
            PopulateLatestAnalysis()
        End If
    End Sub

    Private Sub PopulateUnitList()
        ddlUnits.Items.Clear()
        ddlUnits.Items.Add(New ListItem("Not Hazardous", Guid.Empty.ToString()))
        For Each r As KaUnit In KaUnit.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
            ddlUnits.Items.Add(New ListItem(r.Name, r.Id.ToString()))
        Next
    End Sub

    Private Sub PopulateLatestAnalysis()
        Dim allAnalysis As ArrayList = KaAnalysis.GetAll(GetUserConnection(_currentUser.Id), "table_name = " & Q(_tableName) & " and record_id = " & Q(_recordId) & " and template_id = " & Q(_templateId), "analyzed_at desc")
        If allAnalysis.Count > 0 Then
            Dim analysis As KaAnalysis = allAnalysis.Item(0)
            lblLastAnalysisAt.Text = "Last Analysis At: " & String.Format("{0:G}", analysis.AnalyzedAt)
            Dim entry = analysis.Entrys(0)
            If ddlUnits IsNot Nothing Then
                If ddlUnits.Items Is Nothing OrElse ddlUnits.Items.Count = 0 Then
                    PopulateUnitList()
                End If
                Try
                    ddlUnits.SelectedValue = entry.UnitOfMeasure
                Catch ex As ArgumentOutOfRangeException
                    ddlUnits.SelectedIndex = 0
                End Try
            End If
            tbxDescription.Text = entry.Label
            tbxReportableQuantity.Text = entry.Maximum
        End If
    End Sub

    Private Sub btnSave_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnSave.Click
        If ValidateFields() Then
            'Save analysis
            Dim analysis As KaAnalysis = New KaAnalysis()
            With analysis
                Select Case _tableName
                    Case KaBulkProduct.TABLE_NAME
                        .TableName = KaBulkProduct.TABLE_NAME
                    Case KaTank.TABLE_NAME
                        .TableName = KaTank.TABLE_NAME
                End Select
                .RecordId = _recordId
                .AnalyzedAt = Now
                lblLastAnalysisAt.Text = "Last Analysis At: " & String.Format("{0:G}", .AnalyzedAt)
                .Deleted = False
                .TemplateId = _templateId

                .Entrys.Add(New AnalysisEntry(tbxDescription.ID,
                                              tbxDescription.Text.Trim,
                                              "",
                                              ddlUnits.SelectedValue,
                                              AnalysisEntry.DataType.dtString,
                                              AnalysisEntry.ControlType.ctTextBox) With {
                            .Maximum = Double.Parse(tbxReportableQuantity.Text),
                            .MaximumExceededWarning = "RQ"})
                '.Entrys.Add(New AnalysisEntry(tbxReportableQuantity.ID, lblReportableQuantity.Text, tbxReportableQuantity.Text.Trim, "", AnalysisEntry.DataType.dtDouble, AnalysisEntry.ControlType.ctTextBox))
                '.Entrys.Add(New AnalysisEntry(ddlUnits.ID, lblReportableQuantity.Text & " Unit", ddlUnits.SelectedValue.Trim, "", AnalysisEntry.DataType.dtGuid, AnalysisEntry.ControlType.ctDropDownList))

                .SqlInsert(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                lblStatus.Text = "Analysis Saved."
            End With
        End If
    End Sub

    Private Function ValidateFields() As Boolean
        If Not IsNumeric(tbxReportableQuantity.Text.Trim) Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidReportableQuantity", Utilities.JsAlert("Reportable Quantity must be a number")) : Return False
        If ddlUnits.SelectedIndex < 0 Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidUnit", Utilities.JsAlert("A unit for the reportable quantity must be selected.")) : Return False
        Return True
    End Function
End Class