Imports KahlerAutomation.KaTm2Database

Public Class CustomAnalysisTemplate
	Inherits System.Web.UI.Page
	Private _currentUser As KaUser
	Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
	Private _currentTableName As String = KaCustomPages.TABLE_NAME
	Private _recordId As Guid = Guid.Empty
	Private _tableName As String
	Private _analysisEntries As List(Of AnalysisEntry)
	Private _configuration As Boolean = False
	Private _analysisTypeId As Guid = Guid.Empty

	Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
		_currentUser = Utilities.GetUser(Me)
		_currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "CustomPages")
		lblStatus.Text = ""

		If Request.QueryString("configure") IsNot Nothing AndAlso Boolean.Parse(Request.QueryString("configure")) AndAlso Guid.TryParse(Request.QueryString("template_id"), _analysisTypeId) Then
			'If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")
			_configuration = True
			If Not IsPostBack Then
				Title = "Custom analysis configuration"
				_analysisEntries = GetLatestAnalysis(KaAnalysisTypes.TABLE_NAME, _analysisTypeId, _analysisTypeId.ToString()).Entrys
				Dim editable As Boolean = _currentUserPermission(_currentTableName).Edit
				btnSave.Enabled = editable
				btnAddAnalysis.Visible = editable
				lblLastAnalysisAt.Visible = False
				CreateDynamicControls()
			End If
		ElseIf Guid.TryParse(Request.QueryString("template_id"), _analysisTypeId) AndAlso Guid.TryParse(Request.QueryString("record_id"), _recordId) AndAlso Not _recordId.Equals(Guid.Empty) Then
			_tableName = Request.QueryString("table_name")
			Dim customPage As New KaCustomPages(GetUserConnection(_currentUser.Id), _analysisTypeId)
			If customPage.BulkProductAnalysis Then
				_currentTableName = KaProduct.TABLE_NAME
				_currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Products")
			ElseIf customPage.TankAnalysis Then
				_currentTableName = KaTank.TABLE_NAME
				_currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Tanks")
			End If
			If Not IsPostBack Then
				Title = New KaCustomPages(GetUserConnection(_currentUser.Id), _analysisTypeId).PageLabel
				Try
					Dim analysisTemplate As KaAnalysis = GetLatestAnalysis(KaAnalysisTypes.TABLE_NAME, _analysisTypeId, _analysisTypeId.ToString())
					_analysisEntries = analysisTemplate.ApplyAnalysisTypeToAnalysis(GetLatestAnalysis(_tableName, _recordId, _analysisTypeId.ToString())).Entrys
					'_analysisEntries = New List(Of AnalysisEntry)
					'	For Each templateEntry As AnalysisEntry In analysisTemplate.Entrys
					'		If templateEntry.DataTypeValue = AnalysisEntry.DataType.dtDouble OrElse templateEntry.DataTypeValue = AnalysisEntry.DataType.dtInteger Then
					'			templateEntry.Data = 0
					'		Else
					'			templateEntry.Data = ""
					'		End If
					'		For Each entry As AnalysisEntry In .Entrys
					'			If templateEntry.Id.Equals(entry.Id) Then
					'				templateEntry.Data = entry.Data

					'				For Each templateComponent As AnalysisEntry In templateEntry.Components
					'					If templateComponent.DataTypeValue = AnalysisEntry.DataType.dtDouble OrElse templateComponent.DataTypeValue = AnalysisEntry.DataType.dtInteger Then
					'						templateComponent.Data = 0
					'					Else
					'						templateComponent.Data = ""
					'					End If
					'					For Each component As AnalysisEntry In entry.Components
					'						If templateComponent.Id.Equals(component.Id) Then
					'							templateComponent.Data = component.Data
					'							Exit For
					'						End If
					'					Next
					'				Next

					'				Exit For
					'			End If
					'		Next
					'		_analysisEntries.Add(templateEntry)
					'	Next
					'End With
				Catch ex As Exception
					_analysisEntries = GetLatestAnalysis(KaAnalysisTypes.TABLE_NAME, _analysisTypeId, _analysisTypeId.ToString()).Entrys
				End Try
				Dim editable As Boolean = _currentUserPermission(_currentTableName).Edit

				btnSave.Enabled = editable
				btnAddAnalysis.Visible = False
				CreateDynamicControls()
			End If
		Else
			lblStatus.Text = "Invalid Record GUID"
			pnlAnalysis.Visible = False
			btnSave.Enabled = False
			btnAddAnalysis.Visible = False
		End If
	End Sub

	Private Sub CreateDynamicControls()
		tblAnalysisEntries.Rows.Clear()
		If _configuration Then
			Dim headerRow As New TableRow()
			tblAnalysisEntries.Rows.Add(headerRow)

			headerRow.Attributes.Add("IsEntry", False)

			Dim headerLabelCell As New TableHeaderCell()
			With headerLabelCell
				.Attributes("colspan") = "2"
				.Text = "Label"
			End With
			headerRow.Cells.Add(headerLabelCell)
			'headerRow.Cells.Add(New TableHeaderCell() With {.Text = "Data type"})
			headerRow.Cells.Add(New TableHeaderCell() With {.Text = "Abbreviation"})
			'headerRow.Cells.Add(New TableHeaderCell() With {.Text = "Selection type"})
			headerRow.Cells.Add(New TableHeaderCell() With {.Text = "Minimum"})
			headerRow.Cells.Add(New TableHeaderCell() With {.Text = "Maximum"})
			headerRow.Cells.Add(New TableHeaderCell() With {.Text = "Max warning"})

			For Each entry As AnalysisEntry In _analysisEntries
				tblAnalysisEntries.Rows.Add(CreateDynamicConfigurationRow(entry, False))
				For Each component As AnalysisEntry In entry.Components
					tblAnalysisEntries.Rows.Add(CreateDynamicConfigurationRow(component, True))
				Next
			Next
		Else
			For Each entry As AnalysisEntry In _analysisEntries
				tblAnalysisEntries.Rows.Add(CreateDynamicAnalysisRow(entry, False))
				For Each component As AnalysisEntry In entry.Components
					tblAnalysisEntries.Rows.Add(CreateDynamicAnalysisRow(component, True))
				Next
			Next
		End If
	End Sub

	Private Function CreateDynamicConfigurationRow(entry As AnalysisEntry, isComponent As Boolean) As TableRow
		Dim rowCounter As String = tblAnalysisEntries.Rows.Count.ToString()
		Dim entryRow As New TableRow

		entryRow.Attributes.Add("IsEntry", True)
		entryRow.Attributes.Add("RowNumber", rowCounter)
		entryRow.Attributes.Add("IsComponent", isComponent)
		entryRow.Attributes.Add("AnalysisId", entry.Id.ToString)
		entryRow.Attributes.Add("AnalysisXml", Tm2Database.ToXml(entry, GetType(AnalysisEntry)))
		Dim labelCell As New TableCell()
		With labelCell
			If isComponent Then

				entryRow.Cells.Add(New TableCell() With {.Text = "Component:"})
			Else
				.Attributes("colspan") = "2"
			End If
		End With
		entryRow.Cells.Add(labelCell)
		Dim newEntryLabel As New TextBox
		With newEntryLabel
			.ID = "tbxEntryLabel" & rowCounter
			.Attributes("Style") = "width:auto; min-width:30px;"
			.AutoPostBack = True
			.Text = entry.Label
			AddHandler .TextChanged, AddressOf EntryLabelChanged
		End With
		Dim entryLabelSpan As New HtmlGenericControl("span")
		entryLabelSpan.Attributes("class") = "required"
		labelCell.Controls.Add(entryLabelSpan)
		entryLabelSpan.Controls.Add(newEntryLabel)

		'Dim dataTypeCell As New TableCell()
		'entryRow.Cells.Add(dataTypeCell)
		'' Add the data type list
		'Dim dataTypeDropdownList As New DropDownList
		'With dataTypeDropdownList
		'	.ID = "ddlDataType" & rowCounter
		'	.AutoPostBack = True
		'	.Attributes("Style") = "width:auto; min-width: 30px;"
		'	AddHandler .SelectedIndexChanged, AddressOf DataTypeChanged
		'End With
		'PopulateDataTypeList(dataTypeDropdownList, entry.DataTypeValue)
		'dataTypeCell.Controls.Add(dataTypeDropdownList)

		Dim abbreviationCell As New TableCell()
		entryRow.Cells.Add(abbreviationCell)
		Dim newAbbreviation As New TextBox
		With newAbbreviation
			.ID = "tbxAbbreviation" & rowCounter
			.Attributes("Style") = "width:auto; min-width:30px;"
			.AutoPostBack = True
			.Text = entry.UnitOfMeasure
			AddHandler .TextChanged, AddressOf AbbreviationChanged
		End With
		Dim AbbreviationSpan As New HtmlGenericControl("span")
		AbbreviationSpan.Attributes("class") = "required"
		abbreviationCell.Controls.Add(AbbreviationSpan)
		AbbreviationSpan.Controls.Add(newAbbreviation)

		'Dim controlTypeCell As New TableCell()
		'entryRow.Cells.Add(controlTypeCell)
		'' Add the control type list
		'Dim controlTypeDropdownList As New DropDownList
		'With controlTypeDropdownList
		'	.ID = "ddlControlType" & rowCounter
		'	.AutoPostBack = True
		'	.Attributes("Style") = "width:auto; min-width: 30px;"
		'End With
		'PopulateControlTypeList(controlTypeDropdownList, entry.ControlTypeValue)
		'controlTypeCell.Controls.Add(controlTypeDropdownList)
		'AddHandler controlTypeDropdownList.SelectedIndexChanged, AddressOf ControlTypeChanged

		' Add the minimum value
		Dim minimumValueCell As New TableCell()
		entryRow.Cells.Add(minimumValueCell)
		Dim minimumValueDropdownList As New TextBox
		With minimumValueDropdownList
			.ID = "tbxMinimumValue" & rowCounter
			.AutoPostBack = True
			.Attributes("Style") = "width:auto; min-width: 30px;"
			.Text = entry.Minimum
			AddHandler .TextChanged, AddressOf MinimumValueChanged
		End With
		minimumValueCell.Controls.Add(minimumValueDropdownList)

		' Add the maximum value
		Dim maximumValueCell As New TableCell()
		entryRow.Cells.Add(maximumValueCell)
		Dim maximumValueDropdownList As New TextBox
		With maximumValueDropdownList
			.ID = "tbxMaximumValue" & rowCounter
			.AutoPostBack = True
			.Attributes("Style") = "width:auto; min-width: 30px;"
			.Text = entry.Maximum
			AddHandler .TextChanged, AddressOf MaximumValueChanged
		End With
		maximumValueCell.Controls.Add(maximumValueDropdownList)

		' Add the maximumWarning value
		Dim maximumWarningValueCell As New TableCell()
		entryRow.Cells.Add(maximumWarningValueCell)
		Dim maximumWarningValueDropdownList As New TextBox
		With maximumWarningValueDropdownList
			.ID = "tbxMaximumExceededMessage" & rowCounter
			.AutoPostBack = True
			.Attributes("Style") = "width:auto; min-width: 30px;"
			.Text = entry.MaximumExceededWarning
			.TextMode = TextBoxMode.MultiLine
			AddHandler .TextChanged, AddressOf MaximumWarningValueChanged
		End With
		maximumWarningValueCell.Controls.Add(maximumWarningValueDropdownList)

		' Add the remove button
		Dim removeAnalysisCell As New TableCell()
		entryRow.Cells.Add(removeAnalysisCell)
		Dim removeAnalysisButton As New Button
		With removeAnalysisButton
			.ID = "btnRemoveButton" & rowCounter
			.Attributes("Style") = "width:auto; min-width: 30px;"
			.Text = "Remove"
			AddHandler .Click, AddressOf removeClick
		End With
		removeAnalysisCell.Controls.Add(removeAnalysisButton)

		If Not isComponent Then
			Dim addAnalysisCell As New TableCell()
			entryRow.Cells.Add(addAnalysisCell)
			' Add the add component button
			Dim addComponentButton As New Button
			With addComponentButton
				.ID = "btnAddComponent" & rowCounter
				.Attributes("Style") = "width:auto; min-width: 30px;"
				.Text = "Add component"
				AddHandler .Click, AddressOf addComponentClick
			End With
			addAnalysisCell.Controls.Add(addComponentButton)
		End If

		Return entryRow
	End Function

	Private Function CreateDynamicAnalysisRow(entry As AnalysisEntry, isComponent As Boolean) As TableRow
		Dim rowCounter As String = tblAnalysisEntries.Controls.Count.ToString()
		Dim entryRow As New TableRow()

		entryRow.Attributes.Add("IsEntry", True)
		entryRow.Attributes.Add("RowNumber", rowCounter)
		entryRow.Attributes.Add("IsComponent", isComponent)
		entryRow.Attributes.Add("AnalysisId", entry.Id.ToString)
		entryRow.Attributes.Add("AnalysisXml", Tm2Database.ToXml(entry, GetType(AnalysisEntry)))

		Dim dataEntered As WebControl
		If entry.DataTypeValue = AnalysisEntry.DataType.dtBoolean Then
			dataEntered = New CheckBox
			With CType(dataEntered, CheckBox)
				.ID = "objData" & rowCounter
				.AutoPostBack = True
				.Attributes("Style") = "width:auto; min-width: 30px;"
				Boolean.TryParse(entry.Data, .Checked)
				AddHandler .CheckedChanged, AddressOf AnalysisDataChanged
			End With
			'Case AnalysisEntry.DataType.dtDate,
			'Case AnalysisEntry.DataType.dtDouble,
			'Case AnalysisEntry.DataType.dtGuid,
			'Case AnalysisEntry.DataType.dtInteger,
			'Case AnalysisEntry.DataType.dtString,
		Else
			dataEntered = New TextBox
			With CType(dataEntered, TextBox)
				.ID = "objData" & rowCounter
				.AutoPostBack = True
				.Attributes("Style") = "width:auto; min-width: 30px; text-align:right;"
				.Text = entry.Data
				AddHandler .TextChanged, AddressOf AnalysisDataChanged
			End With
		End If
		Dim datacell As New TableCell
		datacell.Controls.Add(dataEntered)
		If isComponent Then
			Dim a As New Label
			With a
				.Text = " "
				.Attributes("style") = "width:auto; min-width: 30px;"
			End With
			entryRow.Controls.Add(New TableCell)
			entryRow.Controls.Add(datacell)
			entryRow.Controls.Add(New TableCell() With {.Text = entry.UnitOfMeasure & " " & entry.Label})
		Else
			entryRow.Controls.Add(New TableCell() With {.Text = entry.Label})
			entryRow.Controls.Add(datacell)
			entryRow.Controls.Add(New TableCell() With {.Text = entry.UnitOfMeasure})
		End If
		Return entryRow
	End Function

	Private Function ConvertPageToAnalysisEntries() As List(Of AnalysisEntry)
		Dim retval As List(Of AnalysisEntry) = New List(Of AnalysisEntry)
		If _configuration Then
			For rowNumber As Integer = 0 To tblAnalysisEntries.Rows.Count - 1
				Dim entryRow As TableRow = tblAnalysisEntries.Rows(rowNumber)
				If entryRow.Attributes("IsEntry") Then
					Dim entry As AnalysisEntry = Tm2Database.FromXml(entryRow.Attributes("AnalysisXml"), GetType(AnalysisEntry))
					With entry
						.Components.Clear()
						.Label = CType(entryRow.FindControl("tbxEntryLabel" & rowNumber.ToString()), TextBox).Text
						.UnitOfMeasure = CType(entryRow.FindControl("tbxAbbreviation" & rowNumber.ToString()), TextBox).Text
						.Minimum = CType(entryRow.FindControl("tbxMinimumValue" & rowNumber.ToString()), TextBox).Text
						.Maximum = CType(entryRow.FindControl("tbxMaximumValue" & rowNumber.ToString()), TextBox).Text
						.MaximumExceededWarning = CType(entryRow.FindControl("tbxMaximumExceededMessage" & rowNumber.ToString()), TextBox).Text
					End With
					If entryRow.Attributes("IsComponent") Then
						retval(retval.Count - 1).Components.Add(entry)
					Else
						retval.Add(entry)
					End If
				End If
			Next
		Else
			For rowNumber As Integer = 0 To tblAnalysisEntries.Rows.Count - 1
				Dim entryRow As TableRow = tblAnalysisEntries.Rows(rowNumber)
				If entryRow.Attributes("IsEntry") Then
					Dim entry As AnalysisEntry = Tm2Database.FromXml(entryRow.Attributes("AnalysisXml"), GetType(AnalysisEntry))
					With entry
						.Components.Clear()
						If .DataTypeValue = AnalysisEntry.DataType.dtBoolean Then
							.Data = CType(entryRow.FindControl("objData" & rowNumber.ToString()), CheckBox).Checked
						Else
							.Data = CType(entryRow.FindControl("objData" & rowNumber.ToString()), TextBox).Text
						End If
					End With
					If entryRow.Attributes("IsComponent") Then
						retval(retval.Count - 1).Components.Add(entry)
					Else
						retval.Add(entry)
					End If
				End If
			Next
		End If
		Return retval
	End Function

	Protected Overrides Function SaveViewState() As Object
		Dim viewState(2) As Object
		'Saving the grid values to the View State
		viewState(0) = ConvertPageToAnalysisEntries()
		viewState(1) = _configuration
		viewState(2) = MyBase.SaveViewState()
		Return viewState
	End Function

	Protected Overrides Sub LoadViewState(savedState As Object)
		'Getting the dropdown list value from view state.
		If savedState IsNot Nothing AndAlso CType(savedState, Object).Length > 1 Then
			_currentUser = Utilities.GetUser(Me)
			Dim viewState As Object() = savedState
			_analysisEntries = viewState(0)
			_configuration = viewState(1)
			CreateDynamicControls()
			If viewState(2) IsNot Nothing Then MyBase.LoadViewState(viewState(2))
		Else
			MyBase.LoadViewState(savedState)
		End If
	End Sub

	Private Sub EntryLabelChanged(sender As Object, e As EventArgs)
		Dim currentRow As AnalysisEntry = GetCurrentAnalysisEntry(sender)
		If currentRow IsNot Nothing Then currentRow.Label = CType(sender, TextBox).Text

		CreateDynamicControls()
	End Sub

	Private Sub AbbreviationChanged(sender As Object, e As EventArgs)
		Dim currentRow As AnalysisEntry = GetCurrentAnalysisEntry(sender)
		If currentRow IsNot Nothing Then currentRow.UnitOfMeasure = CType(sender, TextBox).Text

		CreateDynamicControls()
	End Sub

	Private Sub PopulateDataTypeList(ByRef dataTypeDropdownList As DropDownList, ByVal dataTypeValue As AnalysisEntry.DataType)
		dataTypeDropdownList.Items.Clear()
		dataTypeDropdownList.Items.Add(New ListItem("String", 0))
		dataTypeDropdownList.Items.Add(New ListItem("Integer", 1))
		dataTypeDropdownList.Items.Add(New ListItem("Double", 2))
		dataTypeDropdownList.Items.Add(New ListItem("Boolean", 3))
		dataTypeDropdownList.Items.Add(New ListItem("Guid", 4))
		dataTypeDropdownList.Items.Add(New ListItem("Date", 5))
		dataTypeDropdownList.SelectedIndex = dataTypeValue
	End Sub

	Private Sub DataTypeChanged(sender As Object, e As EventArgs)
		If ValidateFields(sender) Then
			Dim currentRow As AnalysisEntry = GetCurrentAnalysisEntry(sender)
			If currentRow IsNot Nothing Then currentRow.DataTypeValue = CType(sender, DropDownList).SelectedValue

			CreateDynamicControls()
		End If
	End Sub

	Private Sub PopulateControlTypeList(ByRef controlTypeDropdownList As DropDownList, ByVal controlTypeValue As AnalysisEntry.ControlType)
		controlTypeDropdownList.Items.Clear()
		controlTypeDropdownList.Items.Add(New ListItem("TextBox", 0))
		controlTypeDropdownList.Items.Add(New ListItem("CheckBox", 1))
		controlTypeDropdownList.Items.Add(New ListItem("DropDownList", 2))
		controlTypeDropdownList.Items.Add(New ListItem("Label", 3))
		controlTypeDropdownList.SelectedIndex = controlTypeValue
	End Sub

	Private Sub ControlTypeChanged(sender As Object, e As EventArgs)
		Dim currentRow As AnalysisEntry = GetCurrentAnalysisEntry(sender)
		If currentRow IsNot Nothing Then currentRow.ControlTypeValue = CType(sender, DropDownList).SelectedValue

		CreateDynamicControls()
	End Sub

	Private Sub MinimumValueChanged(sender As Object, e As EventArgs)
		If ValidateFields(sender) Then
			Dim currentRow As AnalysisEntry = GetCurrentAnalysisEntry(sender)
			If currentRow IsNot Nothing Then Double.TryParse(CType(sender, TextBox).Text, currentRow.Minimum)

			CreateDynamicControls()
		End If
	End Sub

	Private Sub MaximumValueChanged(sender As Object, e As EventArgs)
		If ValidateFields(sender) Then
			Dim currentRow As AnalysisEntry = GetCurrentAnalysisEntry(sender)
			If currentRow IsNot Nothing Then Double.TryParse(CType(sender, TextBox).Text, currentRow.Maximum)

			CreateDynamicControls()
		End If
	End Sub

	Private Sub MaximumWarningValueChanged(sender As Object, e As EventArgs)
		Dim currentRow As AnalysisEntry = GetCurrentAnalysisEntry(sender)
		If currentRow IsNot Nothing Then currentRow.MaximumExceededWarning = CType(sender, TextBox).Text

		CreateDynamicControls()
	End Sub

	Protected Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
		Dim connection As OleDb.OleDbConnection = GetUserConnection(_currentUser.Id)
		If _configuration Then
			With New KaAnalysis()
				.TemplateId = _analysisTypeId.ToString()
				.RecordId = _analysisTypeId
				.Id = Guid.NewGuid
				.Entrys = _analysisEntries
				.Created = Now
				.TableName = KaAnalysisTypes.TABLE_NAME
				.AnalyzedAt = Now

				.SqlInsert(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)

				' Go through each record currently using this template, and create a new analysis for it, so it has the current settings saved for it.
				Dim analysisRecordsRdr As OleDb.OleDbDataReader = Tm2Database.ExecuteReader(connection, String.Format("SELECT DISTINCT record_id, table_name FROM analysis WHERE (deleted = 0) AND (table_name <> {0}) AND (template_id = {1})", Q(KaAnalysisTypes.TABLE_NAME), Q(.TemplateId)))
				Do While analysisRecordsRdr.Read()
					Dim analysis As KaAnalysis = KaAnalysis.GetLatestAnalysis(connection, Nothing, analysisRecordsRdr.Item("table_name"), analysisRecordsRdr.Item("record_id"), _analysisTypeId.ToString(), _analysisTypeId)
					Dim newAnalysis As KaAnalysis = .ApplyAnalysisTypeToAnalysis(analysis)
					newAnalysis.Id = Guid.Empty
					newAnalysis.AnalyzedAt = .AnalyzedAt
					newAnalysis.SqlInsert(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
				Loop
				analysisRecordsRdr.Close()
			End With
		Else
			With New KaAnalysis()
				.TemplateId = _analysisTypeId.ToString()
				.RecordId = _recordId
				.Id = Guid.NewGuid
				.Entrys = _analysisEntries
				.Created = Now
				.TableName = _tableName
				.AnalyzedAt = Now

				.SqlInsert(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)

				_analysisEntries = GetLatestAnalysis(_tableName, _recordId, _analysisTypeId.ToString()).Entrys
				CreateDynamicControls()
			End With
		End If
		lblStatus.Text = "Analysis saved successfully"
	End Sub

	Protected Sub btnAddAnalysis_Click(sender As Object, e As EventArgs) Handles btnAddAnalysis.Click
		_analysisEntries.Add(New AnalysisEntry())
		CreateDynamicControls()
	End Sub

	Protected Sub addComponentClick(sender As Object, e As EventArgs)
		Dim currentRow As AnalysisEntry = GetCurrentAnalysisEntry(sender)
		If currentRow IsNot Nothing Then currentRow.Components.Add(New AnalysisEntry)

		CreateDynamicControls()
	End Sub

	Protected Sub removeClick(sender As Object, e As EventArgs)
		Dim currentRow As AnalysisEntry = GetCurrentAnalysisEntry(sender)
		Dim rowCounter As Integer = 0
		If currentRow IsNot Nothing Then
			Do While rowCounter < _analysisEntries.Count
				If _analysisEntries(rowCounter).Id.Equals(currentRow.Id) Then
					_analysisEntries.RemoveAt(rowCounter)
				Else
					Dim componentCounter As Integer = 0
					If currentRow IsNot Nothing Then
						Do While componentCounter < _analysisEntries(rowCounter).Components.Count
							If _analysisEntries(rowCounter).Components(componentCounter).Id.Equals(currentRow.Id) Then
								_analysisEntries(rowCounter).Components.RemoveAt(componentCounter)
							Else
								componentCounter += 1
							End If
						Loop
					End If
					rowCounter += 1
				End If
			Loop
		End If
		CreateDynamicControls()
	End Sub

	Private Function GetCurrentAnalysisEntry(sender As Object) As AnalysisEntry
		Dim currentRow As TableRow = GetCurrentRow(sender)
		If currentRow IsNot Nothing AndAlso currentRow.Attributes("AnalysisXml") IsNot Nothing Then
			Dim temprow As AnalysisEntry = Tm2Database.FromXml(currentRow.Attributes("AnalysisXml"), GetType(AnalysisEntry))
			For Each entry As AnalysisEntry In _analysisEntries
				If entry.Id.Equals(temprow.Id) Then Return entry
				For Each component As AnalysisEntry In entry.Components
					If component.Id.Equals(temprow.Id) Then Return component
				Next
			Next
		End If
		Return Nothing
	End Function

	Private Function GetCurrentRow(sender As Object) As TableRow
		If TypeOf sender Is TableRow Then
			Return sender
		ElseIf sender.Parent IsNot Nothing Then
			Return GetCurrentRow(sender.Parent)
		End If
		Return Nothing
	End Function

	Private Function GetLatestAnalysis(tableName As String, recordId As Guid, template_id As String) As KaAnalysis
		Dim analysis As KaAnalysis = KaAnalysis.GetLatestAnalysis(GetUserConnection(_currentUser.Id), Nothing, tableName, recordId, template_id, _analysisTypeId)
		If analysis.Id.Equals(Guid.Empty) Then
			lblLastAnalysisAt.Text = "No previous analysis saved."
		ElseIf analysis.AnalyzedAt > New Datetime(1900, 1, 1) Then
			lblLastAnalysisAt.Text = "Last Analysis At: " & String.Format("{0:G}", analysis.AnalyzedAt)
		ElseIf analysis.Created > New Datetime(1900, 1, 1) Then
			lblLastAnalysisAt.Text = "Last Analysis At: " & String.Format("{0:G}", analysis.Created)
		Else
			lblLastAnalysisAt.Text = "Last Analysis At: " & String.Format("{0:G}", analysis.LastUpdated)
		End If

		Return analysis
	End Function

	Private Sub AnalysisDataChanged(sender As Object, e As EventArgs)
		If ValidateFields(sender) Then
			Dim currentRow As AnalysisEntry = GetCurrentAnalysisEntry(sender)
			If TypeOf sender Is CheckBox Then
				If currentRow IsNot Nothing Then currentRow.Data = CType(sender, CheckBox).Checked
			Else
				If currentRow IsNot Nothing Then currentRow.Data = CType(sender, TextBox).Text
			End If
			CreateDynamicControls()
		End If
	End Sub

	Private Function ValidateFields(sender As Object) As Boolean
		Dim entryRow As TableRow = GetCurrentRow(sender)
		Dim rowNumber As String = entryRow.Attributes("RowNumber")
		If _configuration Then
			'Dim dataTypeList As DropDownList = entryRow.FindControl("ddlDataType" & rowNumber.ToString())
			'Dim minimum As TextBox = entryRow.FindControl("tbxMinimumValue" & rowNumber.ToString())
			'Dim maximum As TextBox = entryRow.FindControl("tbxMaximumValue" & rowNumber.ToString())
			'If dataTypeList IsNot Nothing AndAlso dataTypeList.SelectedValue = AnalysisEntry.DataType.dtDouble Then
			'	Dim tempValue As Double = 0
			'	If minimum IsNot Nothing AndAlso minimum.Text.Trim.Length > 0 AndAlso Not Double.TryParse(minimum.Text, tempValue) Then
			'		ScriptManager.RegisterClientScriptBlock(Me, Me.GetType(), "InvalidMinimumValue", Utilities.JsAlert("Minimum display value must be numeric."), False)
			'		Return False
			'	ElseIf maximum IsNot Nothing AndAlso maximum.Text.Trim.Length > 0 AndAlso Not Double.TryParse(minimum.Text, tempValue) Then
			'		ScriptManager.RegisterClientScriptBlock(Me, Me.GetType(), "InvalidMaximumValue", Utilities.JsAlert("Maximum value for warning must be numeric."), False)
			'		Return False
			'	End If
			'ElseIf dataTypeList IsNot Nothing AndAlso dataTypeList.SelectedValue = AnalysisEntry.DataType.dtInteger Then
			'	Dim tempValue As Integer = 0
			'	If minimum IsNot Nothing AndAlso minimum.Text.Trim.Length > 0 AndAlso Not Integer.TryParse(minimum.Text, tempValue) Then
			'		ScriptManager.RegisterClientScriptBlock(Me, Me.GetType(), "InvalidMinimumValue", Utilities.JsAlert("Minimum display value must be numeric."), False)
			'		Return False
			'	ElseIf maximum IsNot Nothing AndAlso maximum.Text.Trim.Length > 0 AndAlso Not Integer.TryParse(minimum.Text, tempValue) Then
			'		ScriptManager.RegisterClientScriptBlock(Me, Me.GetType(), "InvalidMaximumValue", Utilities.JsAlert("Maximum value for warning must be numeric."), False)
			'		Return False
			'	End If
			'End If
		Else
			Dim currentRow As AnalysisEntry = GetCurrentAnalysisEntry(sender)
			If currentRow.DataTypeValue = AnalysisEntry.DataType.dtDouble Then
				Dim objData As TextBox = entryRow.FindControl("objData" & rowNumber)
				Dim tempValue As Double = 0
				If objData IsNot Nothing AndAlso objData.Text.Trim.Length > 0 AndAlso Not Double.TryParse(objData.Text, tempValue) Then
					ScriptManager.RegisterClientScriptBlock(Me, Me.GetType(), "InvalidValue", Utilities.JsAlert(String.Format("The value for {0} must be numeric.", currentRow.Label)), False)
					Return False
				End If
			ElseIf currentRow.DataTypeValue = AnalysisEntry.DataType.dtInteger Then
				Dim objData As TextBox = entryRow.FindControl("objData" & rowNumber)
				Dim tempValue As Integer = 0
				If objData IsNot Nothing AndAlso objData.Text.Trim.Length > 0 AndAlso Not Integer.TryParse(objData.Text, tempValue) Then
					ScriptManager.RegisterClientScriptBlock(Me, Me.GetType(), "InvalidValue", Utilities.JsAlert(String.Format("The value for {0} must be numeric.", currentRow.Label)), False)
					Return False
				End If
			End If
		End If
		Return True
	End Function
	Protected Sub ScriptManager1_AsyncPostBackError(ByVal sender As Object, ByVal e As System.Web.UI.AsyncPostBackErrorEventArgs)
		ToolkitScriptManager1.AsyncPostBackErrorMessage = e.Exception.Message
	End Sub
End Class