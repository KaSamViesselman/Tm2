Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class Lots
	Inherits System.Web.UI.Page
	Private _currentUser As KaUser
	Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
	Private _currentTableName As String = KaLot.TABLE_NAME

	Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
		Response.Cache.SetCacheability(HttpCacheability.NoCache)
		_currentUser = Utilities.GetUser(Me)
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		_currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Products")
		If Not Tm2Database.SystemItemTraceabilityEnabled OrElse Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")
		lblStatus.Text = ""
		If Not Page.IsPostBack Then
			SetTextboxMaxLengths()
			PopulateBulkProductList(connection) ' this needs to populate first to set the bulk products
			PopulateLotList(connection)
			SetControlUsabilityFromPermissions()
			Utilities.ConfirmBox(Me.btnDelete, "Are you sure you want to remove this lot?") ' confirmation box setup
		End If
	End Sub

	Private Sub PopulateLotList(connection As OleDbConnection)
		Dim currentLotSelected As String
		If ddlLots.SelectedIndex >= 0 AndAlso ddlLots.Items.Count > 0 Then
			currentLotSelected = ddlLots.SelectedValue
		Else
			currentLotSelected = Guid.Empty.ToString()
		End If
		ddlLots.Items.Clear() ' populate the bulk products list
		If _currentUserPermission(_currentTableName).Create Then
			ddlLots.Items.Add(New ListItem("Enter a new lot", Guid.Empty.ToString()))
		Else
			ddlLots.Items.Add(New ListItem("Select a lot", Guid.Empty.ToString()))
		End If
		Dim lotRdr As OleDbDataReader = Nothing
		Dim conditions As String = $"{KaLot.TABLE_NAME}.{KaLot.FN_DELETED} = 0 AND {KaBulkProduct.TABLE_NAME}.deleted = 0"
		If ddlBulkProductFilter.SelectedIndex > 0 Then conditions &= $" AND {KaLot.TABLE_NAME}.{KaLot.FN_BULK_PRODUCT_ID} = {Q(Guid.Parse(ddlBulkProductFilter.SelectedValue))}"
		If _currentUser.OwnerId <> Guid.Empty Then conditions &= $" AND ({KaBulkProduct.TABLE_NAME}.owner_id = {Q(Guid.Empty)} OR {KaBulkProduct.TABLE_NAME}.owner_id = {Q(_currentUser.OwnerId)})"
		Try
			lotRdr = Tm2Database.ExecuteReader(connection, $"SELECT {KaLot.TABLE_NAME}.{KaLot.FN_ID}, {KaLot.TABLE_NAME}.{KaLot.FN_NUMBER} FROM {KaLot.TABLE_NAME} INNER JOIN {KaBulkProduct.TABLE_NAME} ON {KaLot.TABLE_NAME}.{KaLot.FN_BULK_PRODUCT_ID} = {KaBulkProduct.TABLE_NAME}.{KaBulkProduct.FN_ID} WHERE {conditions} ORDER BY {KaLot.TABLE_NAME}.{KaLot.FN_NUMBER}")
			While lotRdr.Read()
				ddlLots.Items.Add(New ListItem(lotRdr.Item(KaLot.FN_NUMBER), lotRdr.Item(KaLot.FN_ID).ToString()))
			End While
		Finally
			If lotRdr IsNot Nothing Then lotRdr.Close()
		End Try
		Try
			ddlLots.SelectedValue = currentLotSelected
		Catch ex As ArgumentOutOfRangeException
			ddlLots.SelectedIndex = 0
		End Try
		ddlLots_SelectedIndexChanged(ddlLots, New EventArgs())
	End Sub

	Private Sub PopulateBulkProductList(connection As OleDbConnection)
		Dim currentBulkProductFilterSelected As Guid = Guid.Empty
		If ddlBulkProductFilter.SelectedIndex >= 0 AndAlso ddlBulkProductFilter.Items.Count > 0 Then Guid.TryParse(ddlBulkProductFilter.SelectedValue, currentBulkProductFilterSelected)

		ddlBulkProductFilter.Items.Clear() ' populate the bulk products list
		ddlBulkProduct.Items.Clear() ' populate the bulk products list
		ddlBulkProductFilter.Items.Add(New ListItem("Show all lots", Guid.Empty.ToString()))
		If currentBulkProductFilterSelected.Equals(Guid.Empty) Then ddlBulkProduct.Items.Add(New ListItem("Select a bulk product", Guid.Empty.ToString()))
		Dim conditions As String = $"{KaBulkProduct.TABLE_NAME}.deleted = 0"
		If _currentUser.OwnerId <> Guid.Empty Then conditions &= $" AND ({KaBulkProduct.TABLE_NAME}.owner_id = {Q(Guid.Empty)} OR {KaBulkProduct.TABLE_NAME}.owner_id = {Q(_currentUser.OwnerId)})"
		Dim bpRdr As OleDbDataReader = Nothing
		Try
			bpRdr = Tm2Database.ExecuteReader(connection, $"SELECT {KaBulkProduct.TABLE_NAME}.{KaBulkProduct.FN_ID}, {KaBulkProduct.TABLE_NAME}.name FROM {KaBulkProduct.TABLE_NAME} WHERE {conditions} ORDER BY {KaBulkProduct.TABLE_NAME}.name")
			While bpRdr.Read()
				ddlBulkProductFilter.Items.Add(New ListItem(bpRdr.Item("name"), bpRdr.Item(KaBulkProduct.FN_ID).ToString()))
				If currentBulkProductFilterSelected.Equals(Guid.Empty) OrElse currentBulkProductFilterSelected.Equals(bpRdr.Item(KaBulkProduct.FN_ID)) Then ddlBulkProduct.Items.Add(New ListItem(bpRdr.Item("name"), bpRdr.Item(KaBulkProduct.FN_ID).ToString()))
			End While
		Finally
			If bpRdr IsNot Nothing Then bpRdr.Close()
		End Try
		Try
			ddlBulkProductFilter.SelectedValue = currentBulkProductFilterSelected.ToString()
		Catch ex As ArgumentOutOfRangeException
			ddlBulkProductFilter.SelectedIndex = 0
		End Try
	End Sub

	Private Sub SetTextboxMaxLengths()
		tbxLotNumber.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaLot.TABLE_NAME, KaLot.FN_NUMBER))
	End Sub

	Private Sub ddlBulkProductFilter_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlBulkProductFilter.SelectedIndexChanged
		PopulateBulkProductList(GetUserConnection(_currentUser.Id))
		PopulateLotList(GetUserConnection(_currentUser.Id))
	End Sub

	Private Sub ddlLots_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlLots.SelectedIndexChanged
		SetControlUsabilityFromPermissions()
		btnDelete.Enabled = PopulateLotInformation(Guid.Parse(ddlLots.SelectedValue)) AndAlso _currentUserPermission(_currentTableName).Delete
		Utilities.SetFocus(tbxLotNumber, Me)
	End Sub

	Private Function PopulateLotInformation(ByVal lotId As Guid) As Boolean
		Dim lotInfo As KaLot
		Try
			lotInfo = New KaLot(GetUserConnection(_currentUser.Id), lotId, Nothing)

		Catch ex As RecordNotFoundException
			lotInfo = New KaLot() With {
				.Number = "",
				.BulkProductId = Guid.Empty,
				.Complete = False
		}
		End Try

		With lotInfo
			tbxLotNumber.Text = .Number
			Try
				ddlBulkProduct.SelectedValue = .BulkProductId.ToString()
			Catch ex As ArgumentOutOfRangeException
				ddlBulkProduct.SelectedIndex = 0 '
				If Not .BulkProductId.Equals(Guid.Empty) Then ' Only alert if the bulk product was previously assigned
					DisplayJavaScriptMessage("InvalidBulkProductId", Utilities.JsAlert($"Record not found in bulk products where ID = {Q(.BulkProductId)}. Bulk product not set."))
				End If
			End Try
			cbxUsageTrackingComplete.Checked = .Complete
		End With

		Return Not lotInfo.Id.Equals(Guid.Empty)
	End Function

	Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
		Dim lotId As Guid = Guid.Parse(ddlLots.SelectedValue)
		SaveLot(lotId)
	End Sub

	Private Sub SaveLot(ByRef lotId As Guid)
		If ValidateFields(lotId) Then
			Dim newLotAdded As Boolean = False
			With New KaLot()
				.Id = lotId
				.Number = tbxLotNumber.Text
				.BulkProductId = Guid.Parse(ddlBulkProduct.SelectedValue)
				.Complete = cbxUsageTrackingComplete.Checked

				Dim status As String = ""
				If .Id = Guid.Empty Then
					.SqlInsert(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
					status = "Lot added successfully"
					newLotAdded = True
				Else
					.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
					status = "Lot updated successfully"
				End If
				lotId = .Id

				PopulateLotList(GetUserConnection(_currentUser.Id))
				ddlLots.SelectedValue = .Id.ToString
				ddlLots_SelectedIndexChanged(ddlLots, New EventArgs())
				lblStatus.Text = status
			End With
		End If
	End Sub

	Private Function ValidateFields(ByVal lotId As Guid) As Boolean
		If tbxLotNumber.Text.Length = 0 Then
			DisplayJavaScriptMessage("InvalidNumber", Utilities.JsAlert("Lot number must be entered"))
			Return False
		ElseIf ddlBulkProduct.SelectedValue = Guid.Empty.ToString() Then
			DisplayJavaScriptMessage("InvalidBulkProduct", Utilities.JsAlert("A bulk product must be selected."))
			Return False
		ElseIf KaLot.GetAll(GetUserConnection(_currentUser.Id), $"{KaLot.FN_ID} <> {Q(lotId)} AND {KaLot.FN_DELETED} = 0 AND {KaLot.FN_NUMBER} = {Q(tbxLotNumber.Text)} AND {KaLot.FN_BULK_PRODUCT_ID} = {Q(ddlBulkProduct.SelectedValue)}", "").Count > 0 Then
			DisplayJavaScriptMessage("InvalidDuplicatenumber", Utilities.JsAlert("A lot number already exists with the number specified for the specified bulk product."))
			Return False
		End If

		Return True
	End Function

	Private Sub btnDelete_Click(sender As Object, e As EventArgs) Handles btnDelete.Click
		If DeleteLot(Guid.Parse(ddlLots.SelectedValue)) Then
			PopulateLotList(GetUserConnection(_currentUser.Id))
			Try
				ddlLots.SelectedValue = Guid.Empty.ToString()
			Catch ex As ArgumentOutOfRangeException
				ddlLots.SelectedIndex = 0
			End Try
			ddlLots_SelectedIndexChanged(ddlLots, New EventArgs())
			PopulateLotInformation(Guid.Empty)
			btnDelete.Enabled = False
		End If
	End Sub

	Private Function DeleteLot(ByVal lotId As Guid) As Boolean
		With New KaLot(GetUserConnection(_currentUser.Id), lotId, Nothing)
			.Deleted = True
			.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
		End With
		lblStatus.Text = "Selected lot deleted successfully"
		Return True
	End Function

	Private Sub SetControlUsabilityFromPermissions()
		With _currentUserPermission(_currentTableName)
			Dim shouldEnable = (.Edit AndAlso ddlLots.SelectedIndex > 0) OrElse (.Create AndAlso ddlLots.SelectedIndex = 0)
			pnlGeneralInformation.Enabled = shouldEnable
			btnSave.Enabled = shouldEnable
			Dim value = Guid.Parse(ddlLots.SelectedValue)
			btnDelete.Enabled = .Edit AndAlso .Delete AndAlso value <> Guid.Empty
		End With
	End Sub

	Protected Sub ToolkitScriptManager1_AsyncPostBackError(ByVal sender As Object, ByVal e As System.Web.UI.AsyncPostBackErrorEventArgs)
		ToolkitScriptManager1.AsyncPostBackErrorMessage = e.Exception.Message
	End Sub

	Private Sub DisplayJavaScriptMessage(key As String, script As String)
		ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), key, script, False)
	End Sub
End Class