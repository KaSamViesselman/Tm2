Imports KahlerAutomation.KaTm2Database
Public Class DefaultEthanolAnalysis : Inherits System.Web.UI.Page
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
			PopulateLatestAnalysis()
		End If
	End Sub

	Private Sub PopulateLatestAnalysis()
		Dim allAnalysis As ArrayList = KaAnalysis.GetAll(GetUserConnection(_currentUser.Id), "table_name = " & Q(_tableName) & " and record_id = " & Q(_recordId) & " and template_id = " & Q(_templateId), "analyzed_at desc")
		If allAnalysis.Count > 0 Then
			Dim analysis As KaAnalysis = allAnalysis.Item(0)
			lblLastAnalysisAt.Text = "Last Analysis At: " & String.Format("{0:G}", Now)
			For Each entry As AnalysisEntry In analysis.Entrys
				Select Case entry.ControlTypeValue
					Case AnalysisEntry.ControlType.ctCheckBox
					Case AnalysisEntry.ControlType.ctDropDownList
					Case AnalysisEntry.ControlType.ctLabel
					Case AnalysisEntry.ControlType.ctTextBox
						Dim textBox As TextBox = FindControl(entry.ControlName)
						If Not textBox Is Nothing Then
							textBox.Text = entry.Data
						End If
				End Select
				Me.FindControl(entry.ControlName)
			Next
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
				.Deleted = False
				.TemplateId = _templateId

				.Entrys.Add(New AnalysisEntry(tbxVisualAppearance.ID, lblVisualAppearance.Text, tbxVisualAppearance.Text.Trim, "", AnalysisEntry.DataType.dtString, AnalysisEntry.ControlType.ctTextBox))
				.Entrys.Add(New AnalysisEntry(tbxDenaturantVolume.ID, lblDenaturantVolume.Text, tbxDenaturantVolume.Text.Trim, "%", AnalysisEntry.DataType.dtDouble, AnalysisEntry.ControlType.ctTextBox))
				.Entrys.Add(New AnalysisEntry(tbxApparentProof.ID, lblApparentProof.Text, tbxApparentProof.Text.Trim, "%", AnalysisEntry.DataType.dtDouble, AnalysisEntry.ControlType.ctTextBox))
				.Entrys.Add(New AnalysisEntry(tbxSulfur.ID, lblSulfur.Text, tbxSulfur.Text.Trim, "PPM", AnalysisEntry.DataType.dtDouble, AnalysisEntry.ControlType.ctTextBox))
				.Entrys.Add(New AnalysisEntry(tbxPercentWater.ID, lblPercentWater.Text, tbxPercentWater.Text.Trim, "%", AnalysisEntry.DataType.dtDouble, AnalysisEntry.ControlType.ctTextBox))
				.Entrys.Add(New AnalysisEntry(tbxSulfates.ID, lblSulfates.Text, tbxSulfates.Text.Trim, "PPM", AnalysisEntry.DataType.dtDouble, AnalysisEntry.ControlType.ctTextBox))
				.Entrys.Add(New AnalysisEntry(tbxAcidity.ID, lblAcidity.Text, tbxAcidity.Text.Trim, "%", AnalysisEntry.DataType.dtDouble, AnalysisEntry.ControlType.ctTextBox))
				.Entrys.Add(New AnalysisEntry(tbxOlefins.ID, lblOlefins.Text, tbxOlefins.Text.Trim, "%", AnalysisEntry.DataType.dtDouble, AnalysisEntry.ControlType.ctTextBox))
				.Entrys.Add(New AnalysisEntry(tbxPHE.ID, lblPHE.Text, tbxPHE.Text.Trim, "%", AnalysisEntry.DataType.dtDouble, AnalysisEntry.ControlType.ctTextBox))
				.Entrys.Add(New AnalysisEntry(tbxAromatic.ID, lblAromatic.Text, tbxAromatic.Text.Trim, "%", AnalysisEntry.DataType.dtDouble, AnalysisEntry.ControlType.ctTextBox))
				.Entrys.Add(New AnalysisEntry(tbxSource.ID, lblSource.Text, tbxSource.Text.Trim, "", AnalysisEntry.DataType.dtString, AnalysisEntry.ControlType.ctTextBox))
				.Entrys.Add(New AnalysisEntry(tbxEthanolVolume.ID, lblEthanolVolumePercent.Text, tbxEthanolVolume.Text.Trim, "%", AnalysisEntry.DataType.dtDouble, AnalysisEntry.ControlType.ctTextBox))
				.Entrys.Add(New AnalysisEntry(tbxBenzene.ID, lblBenzene.Text, tbxBenzene.Text.Trim, "%", AnalysisEntry.DataType.dtDouble, AnalysisEntry.ControlType.ctTextBox))
				.Entrys.Add(New AnalysisEntry(tbxScaleNumber.ID, lblScaleNumber.Text, tbxScaleNumber.Text.Trim, "EA", AnalysisEntry.DataType.dtString, AnalysisEntry.ControlType.ctTextBox))
				.Entrys.Add(New AnalysisEntry(tbxCopperContent.ID, lblCopperContent.Text, tbxCopperContent.Text.Trim, "PPM", AnalysisEntry.DataType.dtDouble, AnalysisEntry.ControlType.ctTextBox))
				.Entrys.Add(New AnalysisEntry(tbxInorganicChlorideContent.ID, lblInorganicChlorideContent.Text, tbxInorganicChlorideContent.Text.Trim, "PPM", AnalysisEntry.DataType.dtDouble, AnalysisEntry.ControlType.ctTextBox))
				.Entrys.Add(New AnalysisEntry(tbxMethanol.ID, lblMethanol.Text, tbxMethanol.Text.Trim, "%", AnalysisEntry.DataType.dtDouble, AnalysisEntry.ControlType.ctTextBox))
				.Entrys.Add(New AnalysisEntry(tbxSolventWashedGum.ID, lblSolventWashedGum.Text, tbxSolventWashedGum.Text.Trim, "PPM", AnalysisEntry.DataType.dtDouble, AnalysisEntry.ControlType.ctTextBox))
				.Entrys.Add(New AnalysisEntry(tbxAnalysisBy.ID, lblAnalysisBy.Text, tbxAnalysisBy.Text.Trim, "", AnalysisEntry.DataType.dtString, AnalysisEntry.ControlType.ctTextBox))
				.Entrys.Add(New AnalysisEntry(tbxAnalysisDate.ID, lblAnalysisDate.Text, tbxAnalysisDate.Text.Trim, "", AnalysisEntry.DataType.dtString, AnalysisEntry.ControlType.ctTextBox))

				.SqlInsert(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
				lblStatus.Text = "Analysis Saved."
			End With
		End If
	End Sub

	Private Function ValidateFields() As Boolean
		If Not IsNumeric(tbxAcidity.Text.Trim) Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidAcidity", Utilities.JsAlert("Acidity must be a number")) : Return False
		If Not IsNumeric(tbxApparentProof.Text.Trim) Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidApparentProof", Utilities.JsAlert("Apparent Proof must be a number")) : Return False
		If Not IsNumeric(tbxAromatic.Text.Trim) Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidAromatic", Utilities.JsAlert("Aromatic must be a number")) : Return False
		If Not IsNumeric(tbxBenzene.Text.Trim) Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidBenzene", Utilities.JsAlert("Benzene must be a number")) : Return False
		If Not IsNumeric(tbxCopperContent.Text.Trim) Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidCopper", Utilities.JsAlert("Copper Content must be a number")) : Return False
		If Not IsNumeric(tbxDenaturantVolume.Text.Trim) Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidDenaturant", Utilities.JsAlert("Denaturant Volume must be a number")) : Return False
		If Not IsNumeric(tbxEthanolVolume.Text.Trim) Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidEthanolPercent", Utilities.JsAlert("Ethanol Volume % must be a number")) : Return False
		If Not IsNumeric(tbxInorganicChlorideContent.Text.Trim) Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidInorganic", Utilities.JsAlert("Inorganic Chloride Content must be a number")) : Return False
		If Not IsNumeric(tbxMethanol.Text.Trim) Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidMethanol", Utilities.JsAlert("Methanol must be a number")) : Return False
		If Not IsNumeric(tbxOlefins.Text.Trim) Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidOlefines", Utilities.JsAlert("Olefines must be a number")) : Return False
		If Not IsNumeric(tbxPHE.Text.Trim) Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidPHE", Utilities.JsAlert("PHE must be a number")) : Return False
		If Not IsNumeric(tbxPercentWater.Text.Trim) Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidPercentWater", Utilities.JsAlert("Percent Water must be a number")) : Return False
		If Not IsNumeric(tbxSolventWashedGum.Text.Trim) Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidSolvent", Utilities.JsAlert("Solvent Washed Gum must be a number")) : Return False
		If Not IsNumeric(tbxSulfates.Text.Trim) Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidSulfates", Utilities.JsAlert("Sulfates must be a number")) : Return False
		If Not IsNumeric(tbxSulfur.Text.Trim) Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidSulfur", Utilities.JsAlert("Sulfur must be a number")) : Return False
		If tbxAnalysisDate.Text.Trim = "" Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidAnalysisDate", Utilities.JsAlert("You must enter an Analysis Date")) : Return False
		If tbxAnalysisBy.Text.Trim = "" Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidAnalysisBy", Utilities.JsAlert("You must enter an Analysis By")) : Return False
		Return True
	End Function
End Class