Imports KahlerAutomation.KaTm2Database

Public Class TankAnalysis : Inherits System.Web.UI.Page

	Private _currentUser As KaUser
	Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing

#Region "Events"
	Private Sub TankAnalysis_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
		Response.Cache.SetCacheability(HttpCacheability.NoCache)
		_currentUser = Utilities.GetUser(Me)
		_currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaTank.TABLE_NAME}), "Tanks")
		If Not _currentUserPermission(KaTank.TABLE_NAME).Read Then Response.Redirect("Welcome.aspx")
		If Not Page.IsPostBack Then
			PopulateTanks()
			PopulateAnalysisCombo()
		End If
	End Sub

	Private Sub ddlTankAnalysis_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlTankAnalysis.SelectedIndexChanged
		PopulateAnalysisIFrame()
	End Sub

	Private Sub ddlTanks_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlTanks.SelectedIndexChanged
		If Guid.Parse(ddlTanks.SelectedValue) = Guid.Empty Then
			ddlTankAnalysis.Enabled = False
			pnlAnalysis.Visible = False
		Else
			ddlTankAnalysis.Enabled = True
			pnlAnalysis.Visible = True
			Dim tank As KaTank = New KaTank(GetUserConnection(_currentUser.Id), Guid.Parse(ddlTanks.SelectedValue))
			If tank.AnalysisTypeId <> Guid.Empty Then
				Dim analysisType As KaAnalysisTypes = New KaAnalysisTypes(GetUserConnection(_currentUser.Id), tank.AnalysisTypeId)
				Try
					ddlTankAnalysis.SelectedValue = analysisType.TemplateNameId
				Catch ex As Exception
					'If it fails, the analysis type probably wasn't in the list (deleted possibly)
				End Try
			End If
			PopulateAnalysisIFrame()
		End If
	End Sub
#End Region

	Private Sub PopulateTanks()
		Dim li As ListItem = New ListItem
		li.Value = Guid.Empty.ToString
		li.Text = ""
		ddlTanks.Items.Add(li)
		For Each tank As KaTank In KaTank.GetAll(GetUserConnection(_currentUser.Id), "deleted = " & Q(False), "name asc")
			li = New ListItem
			li.Value = tank.Id.ToString
			li.Text = tank.Name
			ddlTanks.Items.Add(li)
		Next
	End Sub

	Private Sub PopulateAnalysisCombo()
		Dim analysisTypes As New SortedDictionary(Of String, String)

        analysisTypes.Add("Default ethanol analysis", "DefaultEthanolAnalysis")
        analysisTypes.Add("Default grain analysis", "DefaultGrainAnalysis")
        analysisTypes.Add("Default chemical analysis", "DefaultChemicalAnalysis")
        For Each customPage As KaCustomPages In KaCustomPages.GetAll(GetUserConnection(_currentUser.Id), "tank_analysis = " & Q(True) & " and deleted = " & Q(False), "page_label asc")
			analysisTypes.Add(customPage.PageLabel, customPage.Id.ToString())
		Next

		ddlTankAnalysis.Items.Clear()
		For Each typeName As String In analysisTypes.Keys
			ddlTankAnalysis.Items.Add(New ListItem() With {.Value = analysisTypes(typeName), .Text = typeName})
		Next
		SelectDefaultAnalysis()
	End Sub

	Private Sub SelectDefaultAnalysis()
		Dim defaultAnalysis As String = KaSetting.GetSetting(GetUserConnection(_currentUser.Id), "General/DefaultTankAnalysis", "", False, Nothing)
		Try
			ddlTankAnalysis.SelectedValue = defaultAnalysis
		Catch ex As ArgumentOutOfRangeException
		End Try
	End Sub

	Private Sub PopulateAnalysisIFrame()
		Dim url As String = ""
		Dim selectedAnanlysis As Object = ddlTankAnalysis.SelectedValue
		Dim connection As OleDb.OleDbConnection = GetUserConnection(_currentUser.Id)
		Try
			Dim selectedAnalysisGuid As Guid = Guid.Parse(selectedAnanlysis)
			Dim customPage As KaCustomPages = New KaCustomPages(connection, selectedAnalysisGuid)
			url = customPage.PageURL
		Catch ex As Exception
			url = CType(selectedAnanlysis, String) & ".aspx"
		End Try

		If url <> "" Then
            If Utilities.ConvertWebPageUrlDomainToRequestedPagesDomain(connection) Then url = Tm2Database.GetUrlInCurrentDomain(Tm2Database.GetCurrentPageUrlWithOriginalPort(Me.Request), url)

            analysisFrame.Attributes("src") = url & "?record_id=" & ddlTanks.SelectedValue & "&table_name=" & KaTank.TABLE_NAME & "&template_id=" & selectedAnanlysis
		End If
	End Sub
End Class