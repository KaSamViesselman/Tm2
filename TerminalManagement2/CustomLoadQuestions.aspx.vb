Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class CustomLoadQuestions : Inherits System.Web.UI.Page
	Private _currentUser As KaUser
	Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
	Private _currentTableName As String = KaLocation.TABLE_NAME

#Region "Events"
	Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
		Response.Cache.SetCacheability(HttpCacheability.NoCache)
		_currentUser = Utilities.GetUser(Me)
		_currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Facilities")
		If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")
		lblStatus.Text = ""
		If Not Page.IsPostBack Then
			SetTextboxMaxLengths()
			PopulateBayList()
			PopulateInspectionQuestionsList(Guid.Empty)
			PopulateOwnersList()
			PopulateTableList()
			UpdateQuestionControls()
			If _currentUserPermission(_currentTableName).Delete Then Utilities.ConfirmBox(Me.btnRemoveQuestion, "Are you sure you want to remove this question?")
		End If
	End Sub

	Protected Sub ddlBay_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles ddlBay.SelectedIndexChanged
		lblStatus.Text = ""
		PopulateInspectionQuestionsList(Guid.Empty)
		UpdateQuestionControls()
	End Sub

	Protected Sub ddlPrePost_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles ddlPrePost.SelectedIndexChanged
		lblStatus.Text = ""
		PopulateInspectionQuestionsList(Guid.Empty)
		UpdateQuestionControls()
	End Sub

	Protected Sub btnAddQuestion_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnAddQuestion.Click
		Dim n As New KaCustomLoadQuestionFields()
		n.Name = "New question"
		n.PromptText = n.Name
		n.InputType = KaCustomLoadQuestionFields.InputTypes.YesNo
		n.BayId = Guid.Parse(ddlBay.SelectedValue)
		n.PostLoad = ddlPrePost.SelectedValue = "Post"
		n.Index = lbxQuestions.Items.Count
		n.Url = ""
		n.Disabled = False
		n.SqlInsert(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
		PopulateInspectionQuestionsList(n.Id)
		lbxQuestions_SelectedIndexChanged(Nothing, Nothing)
		UpdateQuestionControls()
	End Sub

	Protected Sub btnRemoveQuestion_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRemoveQuestion.Click
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Tm2Database.ExecuteNonQuery(connection, String.Format("UPDATE {0} SET deleted=1 WHERE id={1}", KaCustomLoadQuestionFields.TABLE_NAME, Q(lbxQuestions.SelectedValue)))
		lbxQuestions.Items.Remove(lbxQuestions.SelectedItem)
		ResequenceQuestions()
		PopulateInspectionQuestionsList(Guid.Empty)
		UpdateQuestionControls()
	End Sub

	Protected Sub btnMoveQuestionUp_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnMoveQuestionUp.Click
		Dim index As Integer = lbxQuestions.SelectedIndex - 1
		Dim item As ListItem = lbxQuestions.SelectedItem
		lbxQuestions.Items.Remove(item)
		lbxQuestions.Items.Insert(index, item)
		ResequenceQuestions()
		UpdateQuestionControls()
	End Sub

	Protected Sub btnMoveQuestionDown_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnMoveQuestionDown.Click
		Dim index As Integer = lbxQuestions.SelectedIndex + 1
		Dim item As ListItem = lbxQuestions.SelectedItem
		lbxQuestions.Items.Remove(item)
		lbxQuestions.Items.Insert(index, item)
		ResequenceQuestions()
		UpdateQuestionControls()
	End Sub

	Protected Sub btnSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSave.Click
		If lbxQuestions.SelectedIndex >= 0 AndAlso ValidateFields() Then
			Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
			Dim n As New KaCustomLoadQuestionFields(connection, Guid.Parse(lbxQuestions.SelectedValue))
			n.Name = tbxName.Text
			n.PromptText = tbxPromptText.Text
			n.InputType = ddlQuestionType.SelectedIndex
			If ddlQuestionType.SelectedValue = KaCustomLoadQuestionFields.InputTypes.TableLookup Then
				n.Options = Tm2Database.ToXml(New KaCustomQuestionTableLookup(ddlTableLookupTableName.SelectedValue, ddlTableLookupFieldName.SelectedValue), GetType(KaCustomQuestionTableLookup))
			Else
				n.Options = ""
				For Each parameter As ListItem In lbxParameters.Items
					n.Options &= IIf(n.Options.Length > 0, "|", "") & parameter.Value
				Next
			End If
			n.BayId = Guid.Parse(ddlBay.SelectedValue)
			n.PostLoad = cbxPostLoad.Checked
			n.OwnerId = Guid.Parse(ddlOwner.SelectedValue)
			n.Url = tbxUrl.Text
			n.EnterTextMinCharactersRequired = tbxMinimumCharactersRequired.Text.Trim
			n.EnterTextMaxCharactersAllowed = tbxMaximumCharactersAllowed.Text.Trim
			n.EnterTextOnlyAllowNumericValues = chkAllowOnlyNumericValues.Checked
			n.Disabled = cbxDisabled.Checked
			n.SqlUpdate(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
			PopulateInspectionQuestionsList(n.Id)
			lblStatus.Text = "Selected question updated successfully"
			UpdateQuestionControls()
		End If
	End Sub

	Private Function ValidateFields() As Boolean
		If Not IsNumeric(tbxMinimumCharactersRequired.Text.Trim) Then
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "MinimumCharactersRequired", Utilities.JsAlert("The minimum number of characters value must be an integer."))
			Return False
		ElseIf Not IsNumeric(tbxMaximumCharactersAllowed.Text.Trim) Then
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "MaximumCharactersRequired", Utilities.JsAlert("The maximum number of characters value must be an integer."))
			Return False
		ElseIf ddlQuestionType.SelectedValue = KaCustomLoadQuestionFields.InputTypes.TableLookup AndAlso ddlTableLookupTableName.SelectedIndex = 0 Then
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "TableNameRequired", Utilities.JsAlert("A table must be selected for a table lookup question type."))
			Return False
		ElseIf ddlQuestionType.SelectedValue = KaCustomLoadQuestionFields.InputTypes.TableLookup AndAlso ddlTableLookupFieldName.SelectedIndex = 0 Then
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "FieldNameRequired", Utilities.JsAlert("A field must be selected for a table lookup question type."))
			Return False
		Else
			Return True
		End If
	End Function

	Protected Sub ddlQuestionType_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles ddlQuestionType.SelectedIndexChanged
		UpdateQuestionControls()
	End Sub

	Protected Sub lbxQuestions_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles lbxQuestions.SelectedIndexChanged
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim n As New KaCustomLoadQuestionFields(connection, Guid.Parse(lbxQuestions.SelectedValue))
		tbxName.Text = n.Name
		cbxPostLoad.Checked = n.PostLoad
		tbxPromptText.Text = n.PromptText
		ddlQuestionType.SelectedIndex = n.InputType
		For i As Integer = 0 To ddlOwner.Items.Count - 1
			If ddlOwner.Items(i).Value = n.OwnerId.ToString() Then
				ddlOwner.SelectedIndex = i
				Exit For ' no need to look any further
			End If
		Next
		tbxUrl.Text = n.Url
		tbxMinimumCharactersRequired.Text = n.EnterTextMinCharactersRequired
		tbxMaximumCharactersAllowed.Text = n.EnterTextMaxCharactersAllowed
		chkAllowOnlyNumericValues.Checked = n.EnterTextOnlyAllowNumericValues
		cbxDisabled.Checked = n.Disabled
		lbxParameters.Items.Clear()

		Dim tableLookupOption As New KaCustomQuestionTableLookup()
		Try
			tableLookupOption = Tm2Database.FromXml(n.Options, GetType(KaCustomQuestionTableLookup))
			Try
				ddlTableLookupTableName.SelectedValue = tableLookupOption.TableName.ToUpper()
			Catch ex As ArgumentOutOfRangeException
				ddlTableLookupTableName.SelectedValue = ""
			End Try
			ddlTableLookupTableName_SelectedIndexChanged(ddlTableLookupTableName, New EventArgs())
			Try
				ddlTableLookupFieldName.SelectedValue = tableLookupOption.FieldName.ToUpper()
			Catch ex As ArgumentOutOfRangeException
				ddlTableLookupFieldName.SelectedValue = ""
			End Try
		Catch ex As Exception
			If n.Options.Trim().Length > 0 Then
				For Each parameter As String In n.Options.Split("|")
					lbxParameters.Items.Add(GetParameterListItem(parameter))
				Next
			End If
			ddlTableLookupTableName.SelectedIndex = 0
			ddlTableLookupTableName_SelectedIndexChanged(ddlTableLookupTableName, New EventArgs())
			ddlTableLookupFieldName.SelectedIndex = 0
		End Try

		tbxParameter.Text = ""
		UpdateQuestionControls()
	End Sub

	Protected Sub lbxParameters_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles lbxParameters.SelectedIndexChanged
		tbxParameter.Text = lbxParameters.SelectedItem.Value
		UpdateQuestionControls()
	End Sub

	Protected Sub btnAddParameter_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnAddParameter.Click
		Dim newParameterString As String = "New option"
		Dim newListItem As ListItem = GetParameterListItem(newParameterString)
		Dim i As Integer = 0
		Do While lbxParameters.Items.Contains(newListItem)
			i += 1
			newListItem = GetParameterListItem(newParameterString & " " & i.ToString())
		Loop
		lbxParameters.Items.Add(newListItem)
		lbxParameters.SelectedIndex = lbxParameters.Items.Count - 1
		lbxParameters_SelectedIndexChanged(Nothing, Nothing)
		UpdateQuestionControls()
	End Sub

	Protected Sub btnRemoveParameter_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnRemoveParameter.Click
		lbxParameters.Items.Remove(lbxParameters.SelectedItem)
		UpdateQuestionControls()
	End Sub

	Protected Sub btnUpdateParameter_Click(sender As Object, e As EventArgs) Handles btnUpdateParameter.Click
		If lbxParameters.SelectedItem IsNot Nothing Then
			Dim selectedIndex As Integer = lbxParameters.SelectedIndex
			Dim parameter As String = tbxParameter.Text
			lbxParameters.Items.RemoveAt(selectedIndex)
			lbxParameters.Items.Insert(selectedIndex, GetParameterListItem(parameter))
			lbxParameters.SelectedIndex = selectedIndex
			UpdateQuestionControls()
		End If
	End Sub

	Private Sub btnMoveParameterUp_Click(sender As Object, e As System.EventArgs) Handles btnMoveParameterUp.Click
		Dim selectedIndex As Integer = lbxParameters.SelectedIndex
		If selectedIndex > 0 Then
			Dim currentParameter As String = lbxParameters.Items(selectedIndex).Value
			lbxParameters.Items.RemoveAt(selectedIndex)
			lbxParameters.Items.Insert(selectedIndex - 1, GetParameterListItem(currentParameter))
			lbxParameters.SelectedIndex = selectedIndex - 1
			UpdateQuestionControls()
		End If
	End Sub

	Private Sub btnMoveParameterDown_Click(sender As Object, e As System.EventArgs) Handles btnMoveParameterDown.Click
		Dim selectedIndex As Integer = lbxParameters.SelectedIndex
		If selectedIndex >= 0 AndAlso selectedIndex < lbxParameters.Items.Count - 1 Then
			Dim currentParameter As String = lbxParameters.Items(selectedIndex).Value
			lbxParameters.Items.RemoveAt(selectedIndex)
			lbxParameters.Items.Insert(selectedIndex + 1, GetParameterListItem(currentParameter))
			lbxParameters.SelectedIndex = selectedIndex + 1
			UpdateQuestionControls()
		End If
	End Sub
#End Region

	Private Sub ResequenceQuestions()
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		For i As Integer = 0 To lbxQuestions.Items.Count - 1 ' re-sequence the remaining questions
			Tm2Database.ExecuteNonQuery(connection, String.Format("UPDATE {0} SET [index]={1:0} WHERE id={2}", KaCustomLoadQuestionFields.TABLE_NAME, i, Q(lbxQuestions.Items(i).Value)))
		Next
	End Sub

	Private Sub PopulateBayList()
		ddlBay.Items.Clear()
		For Each bay As KaBay In KaBay.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
			ddlBay.Items.Add(New ListItem(bay.Name, bay.Id.ToString()))
		Next
	End Sub

	Private Sub PopulateOwnersList()
		ddlOwner.Items.Clear()
		ddlOwner.Items.Add(New ListItem("All owners", Guid.Empty.ToString()))
		For Each owner As KaOwner In KaOwner.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
			ddlOwner.Items.Add(New ListItem(owner.Name, owner.Id.ToString()))
		Next
	End Sub

	Private Sub PopulateInspectionQuestionsList(id As Guid)
		Dim selectedId As String ' keep track of the item that the user currently has selected
		If id = Guid.Empty Then
			If lbxQuestions.SelectedItem IsNot Nothing Then
				selectedId = lbxQuestions.SelectedItem.Value
			Else ' the user hasn't selected anything
				selectedId = ""
			End If
		Else ' use the ID supplied by the caller
			selectedId = id.ToString()
		End If
		lbxQuestions.Items.Clear()
		Dim conditions As String = "deleted=0 AND post_load=" & IIf(ddlPrePost.SelectedValue = "Post", "1", "0")
		If ddlBay.SelectedItem IsNot Nothing Then ' only show questions for the selected bay
			conditions &= String.Format(" AND bay_id={0}", Q(ddlBay.SelectedItem.Value))
		Else ' the user hasn't selected a bay
			Exit Sub ' don't show any questions
		End If
		For Each question As KaCustomLoadQuestionFields In KaCustomLoadQuestionFields.GetAll(GetUserConnection(_currentUser.Id), conditions, "[index] ASC")
			lbxQuestions.Items.Add(New ListItem(question.Name & IIf(question.Name.Trim().ToUpper() <> question.PromptText.Trim().ToUpper(), ": " & question.PromptText, "") & IIf(question.Disabled, " (disabled)", ""), question.Id.ToString()))
			Dim index As Integer = lbxQuestions.Items.Count - 1
			If lbxQuestions.Items(index).Value = selectedId Then lbxQuestions.SelectedIndex = index
		Next
	End Sub

	Private Sub UpdateQuestionControls()
		liURL.Visible = False
		liMinCharsRequired.Visible = False
		liMaxCharsAllowed.Visible = False
		liAllowOnlyNumericValues.Visible = False
		liTableLookupOptions.Visible = False

		btnAddQuestion.Visible = False
		btnRemoveQuestion.Visible = False
		btnMoveQuestionUp.Visible = False
		btnMoveQuestionDown.Visible = False
		If ddlBay.SelectedIndex >= 0 Then
			btnAddQuestion.Visible = True
			If lbxQuestions.SelectedIndex >= 0 Then
				btnRemoveQuestion.Visible = True
				If lbxQuestions.SelectedIndex > 0 Then btnMoveQuestionUp.Visible = True
				If lbxQuestions.SelectedIndex < lbxQuestions.Items.Count - 1 Then btnMoveQuestionDown.Visible = True
			End If
		End If
		tbxName.Enabled = False
		cbxPostLoad.Enabled = False
		tbxPromptText.Enabled = False
		ddlQuestionType.Enabled = False
		ddlOwner.Enabled = False
		tbxParameter.Enabled = False
		lbxParameters.Enabled = False
		btnAddParameter.Visible = False
		btnRemoveParameter.Visible = False
		btnMoveParameterUp.Visible = False
		btnMoveParameterDown.Visible = False
		btnUpdateParameter.Enabled = False
		cbxDisabled.Enabled = False
		btnSave.Enabled = False
		If lbxQuestions.SelectedIndex >= 0 Then
			tbxName.Enabled = True
			cbxPostLoad.Enabled = True
			tbxPromptText.Enabled = True
			ddlQuestionType.Enabled = True
			ddlOwner.Enabled = True
			cbxDisabled.Enabled = True
			btnSave.Enabled = True
			If ddlQuestionType.SelectedIndex = KaCustomLoadQuestionFields.InputTypes.List Then
				lbxParameters.Enabled = True
				btnAddParameter.Visible = True
				Dim selectedIndex As Integer = -1
				If lbxParameters.SelectedItem IsNot Nothing Then selectedIndex = lbxParameters.SelectedIndex

				btnRemoveParameter.Visible = selectedIndex >= 0
				btnMoveParameterUp.Visible = selectedIndex > 0
				btnMoveParameterDown.Visible = selectedIndex >= 0 AndAlso selectedIndex < lbxParameters.Items.Count - 1
				btnUpdateParameter.Enabled = selectedIndex >= 0
				tbxParameter.Enabled = selectedIndex >= 0
			ElseIf ddlQuestionType.SelectedIndex = KaCustomLoadQuestionFields.InputTypes.Url Then
				liURL.Visible = True
			ElseIf ddlQuestionType.SelectedIndex = KaCustomLoadQuestionFields.InputTypes.TextField Then
				liMinCharsRequired.Visible = True
				liMaxCharsAllowed.Visible = True
				liAllowOnlyNumericValues.Visible = True
			ElseIf ddlQuestionType.SelectedIndex = KaCustomLoadQuestionFields.InputTypes.TableLookup Then
				liTableLookupOptions.Visible = True
			End If
		End If
		SetControlUsabilityFromPermissions()
	End Sub

	Private Sub SetTextboxMaxLengths()
		tbxName.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaCustomLoadQuestionFields.TABLE_NAME, "name"))
		tbxPromptText.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaCustomLoadQuestionFields.TABLE_NAME, "prompt_text"))
	End Sub

	Public Function GetParameterListItem(ByVal parameterName As String) As ListItem
		Return New ListItem(IIf(parameterName.Trim().Length > 0, parameterName, "(blank)"), parameterName)
	End Function

	Private Sub SetControlUsabilityFromPermissions()
		With _currentUserPermission(_currentTableName)
			Dim shouldEnable = (.Edit) OrElse (.Create AndAlso lbxQuestions.SelectedIndex = -1)
			pnlDetails.Enabled = shouldEnable
			btnSave.Enabled = shouldEnable
			btnRemoveQuestion.Enabled = .Edit AndAlso .Delete
			btnAddQuestion.Enabled = shouldEnable
			btnMoveQuestionUp.Enabled = .Edit
			btnMoveQuestionDown.Enabled = .Edit
		End With
	End Sub

	Private Sub PopulateTableList()
		ddlTableLookupTableName.Items.Clear()
		ddlTableLookupTableName.Items.Add(New ListItem("Select table", ""))
		Dim r As OleDbDataReader = Tm2Database.ExecuteReader(Tm2Database.Connection, Nothing, "SELECT name FROM sysobjects WHERE type = 'U' ORDER BY name")
		Do While r.Read()
			ddlTableLookupTableName.Items.Add(New ListItem(r(0), r(0).ToString().ToUpper()))
		Loop
	End Sub

	Private Sub ddlTableLookupTableName_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlTableLookupTableName.SelectedIndexChanged
		ddlTableLookupFieldName.Items.Clear()
		ddlTableLookupFieldName.Items.Add(New ListItem("Select field", ""))
		Try
			Dim r As OleDbDataReader = Tm2Database.ExecuteReader(Tm2Database.Connection, Nothing, "SELECT syscolumns.name FROM sysobjects,syscolumns WHERE syscolumns.id = sysobjects.id AND UPPER(sysobjects.name) = " & Q(ddlTableLookupTableName.SelectedValue) & " ORDER BY syscolumns.name")
			Do While r.Read()
				ddlTableLookupFieldName.Items.Add(New ListItem(r(0), r(0).ToString().ToUpper()))
			Loop
		Catch ex As Exception
		End Try
	End Sub
End Class