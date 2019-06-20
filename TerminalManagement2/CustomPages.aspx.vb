Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class CustomPages : Inherits System.Web.UI.Page

	Private _currentUser As KaUser
	Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
	Private _currentTableName As String = KaCustomPages.TABLE_NAME

#Region "Events"
	Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
		Response.Cache.SetCacheability(HttpCacheability.NoCache)
		_currentUser = Utilities.GetUser(Me)
		_currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "CustomPages")
		If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")

		If Not Page.IsPostBack Then
			SetTextboxMaxLengths()
			PopulateCustomPagesList()
			ResetButtons()
			ddlCustomPages_SelectedIndexChanged(Nothing, Nothing)
			Utilities.ConfirmBox(Me.btnDelete, "Are you sure you want to delete this custom page?")
			Utilities.SetFocus(tbxPageLabel, Me)
		End If
	End Sub

	Private Sub ddlCustomPages_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ddlCustomPages.SelectedIndexChanged
		lblStatus.Text = ""
		Dim customPageId As Guid = Guid.Parse(ddlCustomPages.SelectedValue)
		PopulateCustomPageData(customPageId)
		btnDelete.Enabled = customPageId <> Guid.Empty AndAlso _currentUserPermission(_currentTableName).Delete
		btnSave.Enabled = (_currentUserPermission(_currentTableName).Edit AndAlso ddlCustomPages.SelectedIndex > 0) OrElse (_currentUserPermission(_currentTableName).Create AndAlso ddlCustomPages.SelectedIndex = 0)
		Utilities.SetFocus(tbxPageLabel, Me)
	End Sub

	Private Sub btnSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSave.Click
		SaveCustomPage()
	End Sub

	Protected Sub btnDelete_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnDelete.Click
		lblStatus.Text = ""
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim id As Guid = Guid.Parse(ddlCustomPages.SelectedValue)
		With New KaCustomPages(connection, id)
			.Deleted = True
			.SqlUpdate(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
		End With
		For Each analysisType As KaAnalysisTypes In KaAnalysisTypes.GetAll(connection, "id = " & Q(id), "")
			analysisType.Deleted = True
			analysisType.SqlUpdate(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
		Next
		For Each analysis As KaAnalysis In KaAnalysis.GetAll(connection, "[type_id] = " & Q(id), "")
			analysis.Deleted = True
			analysis.SqlUpdate(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
		Next
		PopulateCustomPageData(Guid.Empty)
		PopulateCustomPagesList()
		lblStatus.Text = "Selected custom page deleted successfully"
		btnDelete.Enabled = False
	End Sub

	Private Sub chkEmailReport_CheckedChanged(sender As Object, e As System.EventArgs) Handles chkEmailReport.CheckedChanged
		pnlEmailWebServiceInfo.Visible = chkEmailReport.Checked
	End Sub
#End Region

	Private Sub PopulateCustomPagesList()
		ddlCustomPages.Items.Clear() ' populate the custom pages list
		If _currentUserPermission(_currentTableName).Create Then ddlCustomPages.Items.Add(New ListItem("Enter a new custom page", Guid.Empty.ToString())) Else ddlCustomPages.Items.Add(New ListItem("Select a custom page", Guid.Empty.ToString()))
		For Each r As KaCustomPages In KaCustomPages.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "page_label ASC")
			ddlCustomPages.Items.Add(New ListItem(r.PageLabel, r.Id.ToString()))
		Next
	End Sub

	Private Sub PopulateCustomPageData(ByVal customPageId As Guid)
		Dim r As New KaCustomPages() : r.Id = customPageId
		If r.Id <> Guid.Empty Then r.SqlSelect(GetUserConnection(_currentUser.Id))
		tbxPageLabel.Text = r.PageLabel
		tbxPageURL.Text = r.PageURL
		pnlAnalysisConfiguration.Visible = False
		If r.MainMenuLink Then
			rdoPageType.SelectedValue = "mainMenuLink"
			ResetButtons()
		ElseIf r.Report Then
			rdoPageType.SelectedValue = "report"
			ResetButtons()
			chkViewReport.Checked = r.ViewReport
			chkEmailReport.Checked = r.EmailReport
		ElseIf r.Analysis Then
			rdoPageType.SelectedValue = "analysis"
			ResetButtons()
			cbxBulkProductAnalysis.Checked = r.BulkProductAnalysis
			cbxTankAnalysis.Checked = r.TankAnalysis
			pnlAnalysisConfiguration.Visible = True
			cbxAnalysisUrlHasConfigOption.Checked = r.UrlHasConfigurationOption
			iframeAnalysisConfiguration.Visible = r.UrlHasConfigurationOption
			If r.UrlHasConfigurationOption Then iframeAnalysisConfiguration.Attributes("src") = r.PageURL & IIf(r.PageURL.Contains("?"), "&", "?") & "template_id=" & r.Id.ToString & "&table_name=CustomPages&configure=true&CreateMainMenu=false"
		ElseIf r.CustomShortcut Then
			rdoPageType.SelectedValue = "custom_shortcut"
			ResetButtons()
			ddlCustomShortcutPromptType.SelectedValue = r.CustomShortcutPrompt
		End If
		tbxEmailWebServiceUrl.Text = r.ReportWebServiceURL
		tbxEmailWebServiceServiceName.Text = r.ReportWebServiceServiceName
		tbxEmailWebServiceMethodName.Text = r.ReportWebServiceMethodName

		chkEmailReport_CheckedChanged(Nothing, Nothing)
	End Sub

	Private Sub SaveCustomPage()
		If tbxPageLabel.Text.Trim() = "" Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidLabel", Utilities.JsAlert("Please specify a label for the custom page.")) : Exit Sub

		If rdoPageType.SelectedValue = "analysis" And Not cbxBulkProductAnalysis.Checked And Not cbxTankAnalysis.Checked Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidAnalysistype", Utilities.JsAlert("Please specify at least one analysis type.")) : Exit Sub

		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		If KaCustomPages.GetAll(connection, "deleted=0 AND id<>" & Q(Guid.Parse(ddlCustomPages.SelectedValue)) & " AND page_label=" & Q(tbxPageLabel.Text), "").Count > 0 Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidDuplicateName", Utilities.JsAlert("A custom page already exists with the specified label. Please specify a unique label.")) : Exit Sub

		With New KaCustomPages()
			.Id = Guid.Parse(ddlCustomPages.SelectedValue)
			Try
				.SqlSelect(connection)
			Catch ex As RecordNotFoundException

			End Try
			.PageLabel = tbxPageLabel.Text.Trim
			.PageURL = tbxPageURL.Text.Trim
			.MainMenuLink = IIf(rdoPageType.SelectedValue = "mainMenuLink", True, False)
			.Report = IIf(rdoPageType.SelectedValue = "report", True, False)
			.Analysis = IIf(rdoPageType.SelectedValue = "analysis", True, False)
			.CustomShortcut = IIf(rdoPageType.SelectedValue = "custom_shortcut", True, False)
			.BulkProductAnalysis = False
			.TankAnalysis = False

			'Reset these booleans
			.BulkProductAnalysis = False
			.TankAnalysis = False
			.ViewReport = False
			.EmailReport = False

			If .Analysis Then
				.BulkProductAnalysis = cbxBulkProductAnalysis.Checked
				.TankAnalysis = cbxTankAnalysis.Checked
			ElseIf .Report Then
				.ViewReport = chkViewReport.Checked
				.EmailReport = chkEmailReport.Checked
			ElseIf .CustomShortcut Then
				.CustomShortcutPrompt = ddlCustomShortcutPromptType.SelectedValue
			End If

			.ReportWebServiceURL = tbxEmailWebServiceUrl.Text.Trim
			.ReportWebServiceServiceName = tbxEmailWebServiceServiceName.Text
			.ReportWebServiceMethodName = tbxEmailWebServiceMethodName.Text

			.UrlHasConfigurationOption = cbxAnalysisUrlHasConfigOption.Checked
			If .Id = Guid.Empty Then
				.SqlInsert(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
				lblStatus.Text = "New custom page added successfully"
			Else
				.SqlUpdate(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
				lblStatus.Text = "Selected custom page updated successfully"
			End If
			If .Analysis AndAlso KaAnalysisTypes.GetAll(connection, "deleted=0 AND template_name_id = " & Q(.Id.ToString()), "created").Count = 0 Then
				Dim newAnalysisType As New KaAnalysisTypes()
				newAnalysisType.Id = .Id
				newAnalysisType.TemplateNameId = .Id.ToString()
				newAnalysisType.Deleted = False
				newAnalysisType.SqlUpdateInsertIfNotFound(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
			End If
			PopulateCustomPagesList()
			ddlCustomPages.SelectedValue = .Id.ToString()
			ddlCustomPages_SelectedIndexChanged(ddlCustomPages, Nothing)
		End With
		btnDelete.Enabled = _currentUserPermission(_currentTableName).Delete
	End Sub

	Private Sub rdoPageType_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles rdoPageType.SelectedIndexChanged
		ResetButtons()
	End Sub

	Private Sub ResetButtons()
		pnlAnalysisOptions.Visible = False
		cbxBulkProductAnalysis.Checked = False
		cbxTankAnalysis.Checked = False

		pnlReportOptions.Visible = False
		chkViewReport.Checked = False
		chkEmailReport.Checked = False

		pnlCustomShortcut.Visible = False
		ddlCustomShortcutPromptType.SelectedIndex = 0

		If rdoPageType.SelectedValue = "mainMenuLink" Then

		ElseIf rdoPageType.SelectedValue = "report" Then
			pnlReportOptions.Visible = True
		ElseIf rdoPageType.SelectedValue = "analysis" Then
			pnlAnalysisOptions.Visible = True
		ElseIf rdoPageType.SelectedValue = "custom_shortcut" Then
			pnlCustomShortcut.Visible = True
		End If
		chkEmailReport_CheckedChanged(Nothing, Nothing)
	End Sub

	Private Sub SetTextboxMaxLengths()
		tbxPageLabel.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaCustomPages.TABLE_NAME, "page_label"))
		tbxPageURL.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaCustomPages.TABLE_NAME, "page_url"))
	End Sub
End Class