Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class CustomFields
	Inherits System.Web.UI.Page

	Private _currentUser As KaUser
	Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
	Private _currentTableName As String = KaCustomPages.TABLE_NAME

	Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
		Response.Cache.SetCacheability(HttpCacheability.NoCache)
		_currentUser = Utilities.GetUser(Me)
		_currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "CustomPages")
		If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")

		If Not Page.IsPostBack Then
			PopulateTables()
			PopulateCustomFields()
			PopulateFieldTypes()
			PopulateTableList()
			ddlCustomFields_SelectedIndexChanged(ddlCustomFields, New EventArgs())
			Utilities.ConfirmBox(Me.btnDelete, "Are you sure you want to delete this custom field?")
			SetTextboxMaxLengths()
		End If
		lblStatus.Text = ""
	End Sub

	Private Sub PopulateCustomFields()
		ddlCustomFields.Items.Clear()
		If _currentUserPermission(_currentTableName).Create Then ddlCustomFields.Items.Add(New ListItem("Enter new custom field", Guid.Empty.ToString())) Else ddlCustomFields.Items.Add(New ListItem("Select a custom field", Guid.Empty.ToString()))

		For Each customField As KaCustomField In KaCustomField.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", KaCustomField.FN_TABLE_NAME & " ASC, " & KaCustomField.FN_FIELD_NAME & " ASC")
			If ddlTableSource.Items.FindByValue(customField.TableName) Is Nothing Then Continue For ' This is here,  in case we ever remove access to a table for custom fields, and the table is no longer valid.
			ddlCustomFields.Items.Add(New ListItem(Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(customField.TableName.Replace(KaLocation.TABLE_NAME, "Facilities").Replace("_", " ") & ": " & customField.FieldName), customField.Id.ToString()))
		Next
	End Sub

	Private Sub PopulateFieldTypes()
		With ddlInputType.Items
			.Clear()
			.Add(New ListItem("Text", KaCustomField.UserInputType.TextBox))
			.Add(New ListItem("List (Single select)", KaCustomField.UserInputType.ListSingleSelect))
			.Add(New ListItem("List (Multiple select)", KaCustomField.UserInputType.ListMultipleSelect))
			.Add(New ListItem("Drop down list", KaCustomField.UserInputType.Combo))
			.Add(New ListItem("Date & Time", KaCustomField.UserInputType.DateTime))
			.Add(New ListItem("Checkbox", KaCustomField.UserInputType.CheckBox))
			.Add(New ListItem("Table lookup (Single select)", KaCustomField.UserInputType.TableLookupSingleSelect))
			.Add(New ListItem("Table lookup (Multiple select)", KaCustomField.UserInputType.TableLookupMultipleSelect))
		End With
	End Sub

	Private Sub PopulateTables()
		With ddlTableSource.Items
			.Clear()
			'.Add(New ListItem("Applicators", KaApplicator.TABLE_NAME))
			'.Add(New ListItem("Bays", KaBay.TABLE_NAME))
			'.Add(New ListItem("Branches", KaBranch.TABLE_NAME))
			'.Add(New ListItem("Bulk products", KaBulkProduct.TABLE_NAME))
			'.Add(New ListItem("Carriers", KaCarrier.TABLE_NAME))
			'.Add(New ListItem("Crop types", KaCropType.TABLE_NAME))
			.Add(New ListItem("Customer accounts", KaCustomerAccount.TABLE_NAME))
			'.Add(New ListItem("Customer destinations", KaCustomerAccountLocation.TABLE_NAME))
			'.Add(New ListItem("Drivers", KaDriver.TABLE_NAME))
			'.Add(New ListItem("Facilities", KaLocation.TABLE_NAME))
			'.Add(New ListItem("Interfaces", KaInterface.TABLE_NAME))
			.Add(New ListItem("Orders", KaOrder.TABLE_NAME))
			'   .Add(New ListItem("Order items", KaOrderItem.TABLE_NAME)) ' We will add this if needed at a future date
			'  .Add(New ListItem("Order accounts", KaOrderCustomerAccount.TABLE_NAME)) ' We will add this if needed at a future date
			'.Add(New ListItem("Owners", KaOwner.TABLE_NAME))
			'.Add(New ListItem("Panels", KaPanel.TABLE_NAME))
			'.Add(New ListItem("Products", KaProduct.TABLE_NAME))
			'.Add(New ListItem("Units", KaUnit.TABLE_NAME))
			.Add(New ListItem("Receiving purchase order", KaReceivingPurchaseOrder.TABLE_NAME))
			.Add(New ListItem("Receiving ticket", KaReceivingTicket.TABLE_NAME))
			.Add(New ListItem("Storage locations", KaStorageLocation.TABLE_NAME))
			.Add(New ListItem("Tickets", KaTicket.TABLE_NAME))
			.Add(New ListItem("Ticket accounts", KaTicketCustomerAccount.TABLE_NAME))
			'.Add(New ListItem("Ticket products", KaTicketItem.TABLE_NAME))
			'.Add(New ListItem("Ticket bulk products", KaTicketBulkItem.TABLE_NAME))
			'.Add(New ListItem("Ticket transports", KaTicketTransport.TABLE_NAME))
			''  .Add(New ListItem("Ticket transport compartment", KaTicketTransportCompartment.TABLE_NAME)) ' We will add this if needed at a future date
			'.Add(New ListItem("Transports", KaTransport.TABLE_NAME))
			''  .Add(New ListItem("Transport compartments", KaTransportCompartment.TABLE_NAME)) ' We will add this if needed at a future date
			'.Add(New ListItem("Transport types", KaTransportTypes.TABLE_NAME))
		End With
	End Sub

	Private Sub SaveCustomField()
		If ValidateFields() Then
			With New KaCustomField()
				.Id = Guid.Parse(ddlCustomFields.SelectedValue)
				.FieldName = tbxName.Text
				.InputType = ddlInputType.SelectedValue
				If pnlOptions.Visible Then
					Dim optionList As New List(Of ListItem)
					For Each customOption As ListItem In lstOptions.Items
						optionList.Add(customOption)
					Next

					.Options = Utilities.SerializeCustomFieldOptions(optionList)
				End If
				If pnlTableLookupOptions.Visible Then
					.Options = Tm2Database.ToXml(New KaCustomQuestionTableLookup(ddlTableLookupTableName.SelectedValue, ddlTableLookupFieldName.SelectedValue), GetType(KaCustomQuestionTableLookup))
				End If
				.TableName = ddlTableSource.SelectedValue
				If pnlTicketSource.Visible Then Guid.TryParse(ddlTicketSource.SelectedValue, .DeliveryTicketCreationSourceField)

				Dim status As String = ""
				If .Id <> Guid.Empty Then
					.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
					status = "Custom field updated successfully."
				Else
					.SqlInsert(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
					status = "Custom field added successfully."
				End If
				PopulateCustomFields()
				ddlCustomFields.SelectedValue = .Id.ToString()
				btnDelete.Enabled = _currentUserPermission(_currentTableName).Delete
				lblStatus.Text = status
			End With
		End If
	End Sub

	Private Function ValidateFields() As Boolean
		If tbxName.Text.Trim().Length = 0 Then ' user didn't enter a name
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidName", Utilities.JsAlert("Please specify a name."))
			Return False
		ElseIf ddlInputType.SelectedIndex < 0 Then ' user didn't select a type
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidType", Utilities.JsAlert("Please specify a type of custom field."))
			Return False
		ElseIf ddlTableSource.SelectedIndex < 0 Then ' user didn't select a table
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidTable", Utilities.JsAlert("Please specify a table."))
			Return False
		ElseIf pnlTableLookupOptions.Visible AndAlso ddlTableLookupTableName.SelectedIndex = 0 Then
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "TableNameRequired", Utilities.JsAlert("A table must be selected for a table lookup question type."))
			Return False
		ElseIf pnlTableLookupOptions.Visible AndAlso ddlTableLookupFieldName.SelectedIndex = 0 Then
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "FieldNameRequired", Utilities.JsAlert("A field must be selected for a table lookup question type."))
			Return False
		Else
			Dim conditions As String = String.Format("{0}={1} AND {2}={3} AND {4}={5} AND id<>{6}", KaCustomField.FN_DELETED, Q(False), KaCustomField.FN_TABLE_NAME, Q(ddlTableSource.SelectedValue), KaCustomField.FN_FIELD_NAME, Q(tbxName.Text), Q(ddlCustomFields.SelectedValue))
			If KaCustomField.GetAll(GetUserConnection(_currentUser.Id), conditions, "").Count > 0 Then ' a crop type with the specified name already exists
				ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidDuplicateName", Utilities.JsAlert("A custom field with the name """ & tbxName.Text & """ already exists for the table " & ddlTableSource.SelectedValue & ". Please specify a unique name and table for this custom field."))
				Return False
			Else
				Return True
			End If
		End If
	End Function

	Private Sub btnSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSave.Click
		SaveCustomField()
	End Sub

	Protected Sub btnDelete_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnDelete.Click
		lblStatus.Text = ""
		With New KaCustomField(GetUserConnection(_currentUser.Id), Guid.Parse(ddlCustomFields.SelectedValue), Nothing)
			.Deleted = True
			.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
		End With
		PopulateCustomFields()
		ddlCustomFields.SelectedIndex = 0
		ddlCustomFields_SelectedIndexChanged(ddlCustomFields, New EventArgs())
		lblStatus.Text = "Selected custom page deleted successfully"
		btnDelete.Enabled = False
	End Sub

	Protected Sub ddlCustomFields_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlCustomFields.SelectedIndexChanged
		Try
			Dim cf As New KaCustomField(GetUserConnection(_currentUser.Id), Guid.Parse(ddlCustomFields.SelectedValue), Nothing)
			With cf
				tbxName.Text = .FieldName
				ddlTableSource.SelectedValue = .TableName
				ddlTableSource_SelectedIndexChanged(ddlTableSource, New EventArgs())
				Try
					ddlTicketSource.SelectedValue = .DeliveryTicketCreationSourceField.ToString()
				Catch ex As ArgumentOutOfRangeException

				End Try
				Try
					ddlInputType.SelectedValue = .InputType
				Catch ex As ArgumentOutOfRangeException

				End Try
				ddlInputType_SelectedIndexChanged(ddlInputType, New EventArgs())

				Try
					Dim tableLookupOption As KaCustomQuestionTableLookup = Tm2Database.FromXml(.Options, GetType(KaCustomQuestionTableLookup))
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
					ddlTableLookupTableName.SelectedIndex = 0
					ddlTableLookupTableName_SelectedIndexChanged(ddlTableLookupTableName, New EventArgs())
					ddlTableLookupFieldName.SelectedIndex = 0
				End Try

				lstOptions.Items.Clear()
				For Each fieldOption As ListItem In Utilities.DeserializeCustomFieldOptions(.Options)
					lstOptions.Items.Add(fieldOption)
				Next
			End With
		Catch ex As RecordNotFoundException
			tbxName.Text = ""
			ddlInputType.SelectedIndex = 0
			ddlInputType_SelectedIndexChanged(ddlInputType, New EventArgs())
			ddlTableSource.SelectedIndex = 0
			ddlTableSource_SelectedIndexChanged(ddlTableSource, New EventArgs())
			ddlTicketSource.SelectedIndex = 0
			lstOptions.Items.Clear()
		End Try
		SetControlUsabilityFromPermissions()
	End Sub

	Private Sub ddlTableSource_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles ddlTableSource.SelectedIndexChanged
		Dim tableList As New List(Of String)
		With ddlTicketSource.Items
			.Clear()
			.Add(New ListItem("Select source for ticket creation", Guid.Empty.ToString()))
			Select Case ddlTableSource.SelectedValue.ToLower()
				Case KaReceivingTicket.TABLE_NAME.ToLower
					tableList.Add(KaCarrier.TABLE_NAME.ToLower())
					tableList.Add(KaDriver.TABLE_NAME.ToLower())
					tableList.Add(KaLocation.TABLE_NAME.ToLower())
					tableList.Add(KaInterface.TABLE_NAME.ToLower())
					tableList.Add(KaReceivingPurchaseOrder.TABLE_NAME.ToLower())
					tableList.Add(KaOwner.TABLE_NAME.ToLower())
					tableList.Add(KaTransport.TABLE_NAME.ToLower())
					tableList.Add(KaTransportTypes.TABLE_NAME.ToLower())
				Case KaTicket.TABLE_NAME.ToLower
					tableList.Add(KaApplicator.TABLE_NAME.ToLower())
					tableList.Add(KaBranch.TABLE_NAME.ToLower())
					tableList.Add(KaCarrier.TABLE_NAME.ToLower())
					tableList.Add(KaCustomerAccountLocation.TABLE_NAME.ToLower())
					tableList.Add(KaDriver.TABLE_NAME.ToLower())
					tableList.Add(KaLocation.TABLE_NAME.ToLower())
					tableList.Add(KaInterface.TABLE_NAME.ToLower())
					tableList.Add(KaOrder.TABLE_NAME.ToLower())
					tableList.Add(KaOwner.TABLE_NAME.ToLower())
				Case KaTicketCustomerAccount.TABLE_NAME.ToLower()
					tableList.Add(KaCustomerAccount.TABLE_NAME.ToLower())
				Case KaTicketItem.TABLE_NAME.ToLower
					tableList.Add(KaCropType.TABLE_NAME.ToLower())
					tableList.Add(KaOrderItem.TABLE_NAME.ToLower())
					tableList.Add(KaOwner.TABLE_NAME.ToLower())
					tableList.Add(KaProduct.TABLE_NAME.ToLower())
					tableList.Add(KaUnit.TABLE_NAME.ToLower())
				Case KaTicketBulkItem.TABLE_NAME.ToLower
					tableList.Add(KaBay.TABLE_NAME.ToLower())
					tableList.Add(KaBulkProduct.TABLE_NAME.ToLower())
					tableList.Add(KaCropType.TABLE_NAME.ToLower())
					tableList.Add(KaOwner.TABLE_NAME.ToLower())
					tableList.Add(KaPanel.TABLE_NAME.ToLower())
					tableList.Add(KaUnit.TABLE_NAME.ToLower())
				Case KaTicketTransport.TABLE_NAME.ToLower
					tableList.Add(KaTransport.TABLE_NAME.ToLower())
					tableList.Add(KaTransportTypes.TABLE_NAME.ToLower())
					tableList.Add(KaUnit.TABLE_NAME.ToLower())
				Case KaTicketTransportCompartment.TABLE_NAME.ToLower
					tableList.Add(KaTransportCompartment.TABLE_NAME.ToLower())
			End Select
			If tableList.Count > 0 Then
				Dim tables As String = ""
				For Each tm2Table As String In tableList
					If tables.Length > 0 Then tables &= ","
					tables &= Q(tm2Table)
				Next
				For Each customFieldAvailable As KaCustomField In KaCustomField.GetAll(GetUserConnection(_currentUser.Id), "deleted = 0 AND " & KaCustomField.FN_TABLE_NAME & " IN (" & tables & ")", KaCustomField.FN_TABLE_NAME & " ASC, " & KaCustomField.FN_FIELD_NAME & " ASC")
					.Add(New ListItem(Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(customFieldAvailable.TableName.Replace(KaLocation.TABLE_NAME, "Facilities").Replace("_", " ") & ": " & customFieldAvailable.FieldName), customFieldAvailable.Id.ToString()))
				Next
			End If
			pnlTicketSource.Visible = (.Count > 1)
			pnlInputType.Visible = Not IsTicketCustomField()
		End With
	End Sub

	Private Sub btnAddOption_Click(sender As Object, e As System.EventArgs) Handles btnAddOption.Click
		If Not lstOptions.Items.Contains(New ListItem(tbxOption.Text.Trim)) Then lstOptions.Items.Add(New ListItem(tbxOption.Text.Trim)) : tbxOption.Text = ""
	End Sub

	Private Sub btnRemoveOption_Click(sender As Object, e As System.EventArgs) Handles btnRemoveOption.Click
		If lstOptions.SelectedIndex >= 0 AndAlso lstOptions.Items.Count > 0 Then lstOptions.Items.RemoveAt(lstOptions.SelectedIndex)
	End Sub

	Private Sub ddlInputType_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles ddlInputType.SelectedIndexChanged
		pnlOptions.Visible = Not IsTicketCustomField() AndAlso (ddlInputType.SelectedValue = KaCustomField.UserInputType.Combo OrElse ddlInputType.SelectedValue = KaCustomField.UserInputType.ListSingleSelect OrElse ddlInputType.SelectedValue = KaCustomField.UserInputType.ListMultipleSelect)
		pnlTableLookupOptions.Visible = Not IsTicketCustomField() AndAlso (ddlInputType.SelectedValue = KaCustomField.UserInputType.TableLookupSingleSelect OrElse ddlInputType.SelectedValue = KaCustomField.UserInputType.TableLookupMultipleSelect)
	End Sub

	Private Function IsTicketCustomField() As Boolean
		Select Case ddlTableSource.SelectedValue.ToLower()
			Case KaReceivingTicket.TABLE_NAME.ToLower
				Return True
			Case KaTicket.TABLE_NAME.ToLower
				Return True
			Case KaTicketCustomerAccount.TABLE_NAME.ToLower()
				Return True
			Case KaTicketItem.TABLE_NAME.ToLower
				Return True
			Case KaTicketBulkItem.TABLE_NAME.ToLower
				Return True
			Case KaTicketTransport.TABLE_NAME.ToLower
				Return True
			Case KaTicketTransportCompartment.TABLE_NAME.ToLower
				Return True
			Case Else
				Return False
		End Select
	End Function

	Private Sub SetTextboxMaxLengths()
		tbxName.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaOrder.TABLE_NAME, KaCustomField.FN_FIELD_NAME))
	End Sub

	Protected Sub btnSortOptions_Click(sender As Object, e As EventArgs) Handles btnSortOptions.Click
		Dim listItemList As New SortedList(Of String, ListItem)
		For Each item As ListItem In lstOptions.Items
			listItemList.Add(item.Text, item)
		Next

		lstOptions.Items.Clear()
		For Each item As String In listItemList.Keys
			lstOptions.Items.Add(listItemList(item))
		Next
	End Sub

	Private Sub PopulateTableList()
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		ddlTableLookupTableName.Items.Clear()
		ddlTableLookupTableName.Items.Add(New ListItem("Select table", ""))
		Dim r As OleDbDataReader = Tm2Database.ExecuteReader(connection, Nothing, "SELECT name FROM sysobjects WHERE type = 'U' ORDER BY name")
		Do While r.Read()
			ddlTableLookupTableName.Items.Add(New ListItem(r(0), r(0).ToString().ToUpper()))
		Loop
	End Sub

	Private Sub ddlTableLookupTableName_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlTableLookupTableName.SelectedIndexChanged
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		ddlTableLookupFieldName.Items.Clear()
		ddlTableLookupFieldName.Items.Add(New ListItem("Select field", ""))
		Try
			Dim r As OleDbDataReader = Tm2Database.ExecuteReader(connection, Nothing, "SELECT syscolumns.name FROM sysobjects,syscolumns WHERE syscolumns.id = sysobjects.id AND UPPER(sysobjects.name) = " & Q(ddlTableLookupTableName.SelectedValue) & " ORDER BY syscolumns.name")
			Do While r.Read()
				ddlTableLookupFieldName.Items.Add(New ListItem(r(0), r(0).ToString().ToUpper()))
			Loop
		Catch ex As Exception
		End Try
	End Sub

	Private Sub SetControlUsabilityFromPermissions()
		With _currentUserPermission(_currentTableName)
			Dim shouldEnable = (.Edit AndAlso ddlCustomFields.SelectedIndex > 0) OrElse (.Create AndAlso ddlCustomFields.SelectedIndex = 0)
			tbxName.Enabled = shouldEnable
			ddlTableSource.Enabled = shouldEnable
			ddlInputType.Enabled = shouldEnable
			btnSave.Enabled = shouldEnable
			btnDelete.Enabled = Not Guid.Parse(ddlCustomFields.SelectedValue).Equals(Guid.Empty) AndAlso .Edit AndAlso .Delete
		End With
	End Sub
End Class