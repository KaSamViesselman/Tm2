Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class AnalysisSettings : Inherits System.Web.UI.Page

	Private _currentUser As KaUser
	Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
	Private _currentTableName As String = KaSetting.TABLE_NAME

	Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
		Response.Cache.SetCacheability(HttpCacheability.NoCache)
		_currentUser = Utilities.GetUser(Me)
		_currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "GeneralSettings")
		If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")
		btnSave.Enabled = _currentUserPermission(_currentTableName).Edit
		If Not Page.IsPostBack Then
			PopulateAnalysisCombos()
			DisplaySettings()
		End If
	End Sub

	Private Sub PopulateAnalysisCombos()
		ddlDefaultBulkProductAnalysis.Items.Clear()
        ddlDefaultTankAnalysis.Items.Clear()
        ddlDefaultHazmatAnalysis.Items.Clear()
        Dim li As ListItem = New ListItem
        li.Value = ""
        li.Text = "Analysis disabled"
        ddlDefaultBulkProductAnalysis.Items.Add(li)
		li = New ListItem
        li.Value = ""
        li.Text = "Analysis disabled"
        ddlDefaultTankAnalysis.Items.Add(li)
        li = New ListItem
        li.Value = ""
        li.Text = "Analysis disabled"
        ddlDefaultHazmatAnalysis.Items.Add(li)
        li = New ListItem
        li.Value = "DefaultEthanolAnalysis"
        li.Text = "Default ethanol analysis"
        ddlDefaultBulkProductAnalysis.Items.Add(li)
        li = New ListItem
        li.Value = "DefaultEthanolAnalysis"
        li.Text = "Default ethanol analysis"
        ddlDefaultTankAnalysis.Items.Add(li)
        li = New ListItem
		li.Value = "DefaultGrainAnalysis"
        li.Text = "Default grain analysis"
        ddlDefaultBulkProductAnalysis.Items.Add(li)
		li = New ListItem
		li.Value = "DefaultGrainAnalysis"
        li.Text = "Default grain analysis"
        ddlDefaultTankAnalysis.Items.Add(li)
		li = New ListItem
		li.Value = "DefaultChemicalAnalysis"
        li.Text = "Default chemical analysis"
        ddlDefaultBulkProductAnalysis.Items.Add(li)
		li = New ListItem
		li.Value = "DefaultChemicalAnalysis"
        li.Text = "Default chemical analysis"
        ddlDefaultTankAnalysis.Items.Add(li)
        li = New ListItem
        li.Value = "DefaultHazmatAnalysis"
        li.Text = "Default hazardous material analysis"
        ddlDefaultHazmatAnalysis.Items.Add(li)

        'Bulk Product Analysis
        For Each customPage As KaCustomPages In KaCustomPages.GetAll(GetUserConnection(_currentUser.Id), "deleted = 0 AND bulk_product_analysis = 1", "page_label asc")
			li = New ListItem
			li.Value = customPage.Id.ToString
			li.Text = customPage.PageLabel
			ddlDefaultBulkProductAnalysis.Items.Add(li)
		Next

		'Tank Analysis
		For Each customPage As KaCustomPages In KaCustomPages.GetAll(GetUserConnection(_currentUser.Id), "deleted = 0 AND tank_analysis = 1", "page_label asc")
			li = New ListItem
			li.Value = customPage.Id.ToString
			li.Text = customPage.PageLabel
			ddlDefaultTankAnalysis.Items.Add(li)
		Next
	End Sub

	Private Sub DisplaySettings()
		Dim c As OleDbConnection = GetUserConnection(_currentUser.Id)
		Try
			ddlDefaultBulkProductAnalysis.SelectedValue = KaSetting.GetSetting(c, "General/DefaultBulkProductAnalysis", "", False, Nothing)
		Catch ex As ArgumentOutOfRangeException
		End Try
        Try
            ddlDefaultTankAnalysis.SelectedValue = KaSetting.GetSetting(c, "General/DefaultTankAnalysis", "", False, Nothing)
        Catch ex As ArgumentOutOfRangeException
        End Try
        Try
            ddlDefaultHazmatAnalysis.SelectedValue = KaSetting.GetSetting(c, "General/DefaultHazmatAnalysis", "", False, Nothing)
        Catch ex As ArgumentOutOfRangeException
        End Try
    End Sub

	Private Sub btnSave_Click(sender As Object, e As System.EventArgs) Handles btnSave.Click
		Dim c As OleDbConnection = GetUserConnection(_currentUser.Id)
        KaSetting.WriteSetting(c, "General/DefaultBulkProductAnalysis", ddlDefaultBulkProductAnalysis.SelectedValue)
        KaSetting.WriteSetting(c, "General/DefaultTankAnalysis", ddlDefaultTankAnalysis.SelectedValue)
        KaSetting.WriteSetting(c, "General/DefaultHazmatAnalysis", ddlDefaultHazmatAnalysis.SelectedValue)

        lblSave.Visible = True
	End Sub
End Class