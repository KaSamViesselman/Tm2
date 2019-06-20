Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class StorageLocations
	Inherits System.Web.UI.Page
	Private _currentUser As KaUser
	Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
	Private _currentTableName As String = KaStorageLocation.TABLE_NAME
	Private _customFields As New List(Of KaCustomField)
	Private _customFieldData As New List(Of KaCustomFieldData)

	Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
		Response.Cache.SetCacheability(HttpCacheability.NoCache)
		_currentUser = Utilities.GetUser(Me)
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		_currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Facilities")
		If Not Tm2Database.SystemItemTraceabilityEnabled OrElse Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")
		lblStatus.Text = ""
		litStorageLocationContentsAsOfDate.Text = ""
		If Not Page.IsPostBack Then
			_customFields.Clear()
			For Each customField As KaCustomField In KaCustomField.GetAll(connection, String.Format("deleted=0 AND {0} = {1}", KaCustomField.FN_TABLE_NAME, Q(KaStorageLocation.TABLE_NAME)), KaCustomField.FN_FIELD_NAME)
				_customFields.Add(customField)
			Next
			SetTextboxMaxLengths()
			tbxCleanoutDate.Value = DateTime.Now.ToString("G") ' setting "Cleanout" datepicker to default times 
			tbxTransferStart.Value = tbxCleanoutDate.Value ' setting "Transfer start" datepicker to default times 
			tbxTransferStop.Value = tbxCleanoutDate.Value ' setting "transfer stop" datepicker to default times 
			tbxStorageLocationContentsAsOfDate.Value = tbxCleanoutDate.Value ' setting "Current contents as of date" datepicker to default times 
			rblStorageLocationType_SelectedIndexChanged(rblStorageLocationType, New EventArgs())
			Utilities.ConfirmBox(Me.btnDelete, "Are you sure you want to remove this storage location?") ' confirmation box setup
		End If
	End Sub

	Private Sub PopulateLocations(connection As OleDbConnection)
		ddlLocation.Items.Clear()
		ddlLocation.Items.Add(New ListItem("Select facility", Guid.Empty.ToString()))
		Dim locationRdr As OleDbDataReader = Nothing
		Try
			locationRdr = Tm2Database.ExecuteReader(connection, $"SELECT {KaLocation.FN_ID}, {KaLocation.FN_NAME} FROM {KaLocation.TABLE_NAME} WHERE {KaLocation.FN_DELETED} = 0 ORDER BY {KaLocation.FN_NAME}")
			Do While locationRdr.Read()
				ddlLocation.Items.Add(New ListItem(locationRdr.Item("Name"), locationRdr.Item("Id").ToString()))
			Loop
		Finally
			If locationRdr IsNot Nothing Then locationRdr.Close()
		End Try
	End Sub

	Private Sub PopulateTanks(connection As OleDbConnection)
		Dim currentTankId As String = Guid.Empty.ToString()
		If ddlTank.SelectedIndex > 0 Then currentTankId = ddlTank.SelectedValue

		ddlTank.Items.Clear()
		ddlTank.Items.Add(New ListItem("Select tank", Guid.Empty.ToString()))
		Dim whereClause As String = $"deleted = 0"
		Dim tankRdr As OleDbDataReader = Nothing
		Try
			tankRdr = Tm2Database.ExecuteReader(connection, $"SELECT {KaTank.FN_ID}, name FROM {KaTank.TABLE_NAME} WHERE {whereClause} ORDER BY name")
			Do While tankRdr.Read()
				ddlTank.Items.Add(New ListItem(tankRdr.Item("Name"), tankRdr.Item(KaTank.FN_ID).ToString()))
			Loop
		Finally
			If tankRdr IsNot Nothing Then tankRdr.Close()
		End Try
		Try
			ddlTank.SelectedValue = currentTankId
		Catch ex As ArgumentOutOfRangeException
			ddlTank.SelectedIndex = 0
			DisplayJavaScriptMessage("TankNotFoundWarning", Utilities.JsAlert(String.Format($"The tank assigned to the storage location could not be assigned, possibly because it has been previously deleted.")))
		End Try
	End Sub

	Private Sub PopulateContainers(connection As OleDbConnection)
		ddlContainer.Items.Clear()
		ddlContainer.Items.Add(New ListItem("Select container", Guid.Empty.ToString()))
		Dim containerRdr As OleDbDataReader = Nothing
		Try
			containerRdr = Tm2Database.ExecuteReader(connection, $"SELECT {KaContainer.FN_ID}, number FROM {KaContainer.TABLE_NAME} WHERE deleted = 0 ORDER BY number")
			Do While containerRdr.Read()
				ddlContainer.Items.Add(New ListItem(containerRdr.Item("number"), containerRdr.Item(KaContainer.FN_ID).ToString()))
			Loop
		Finally
			If containerRdr IsNot Nothing Then containerRdr.Close()
		End Try
	End Sub

	Private Sub PopulateStorageLocationsList(storageLocationId As String)
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		ddlStorageLocations.Items.Clear()

		Dim sql As String

		Select Case rblStorageLocationType.SelectedValue
			Case "TankStorage"
				ddlStorageLocations.Items.Add(New ListItem("New storage location for tank", Guid.Empty.ToString()))
				sql = $"SELECT {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_ID}, " &
							$"CASE WHEN {KaTank.TABLE_NAME}.name = {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_NAME} THEN {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_NAME} ELSE {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_NAME} + ' (Tank: ' + {KaTank.TABLE_NAME}.name + ')' END AS {KaStorageLocation.FN_NAME} " &
						$"FROM {KaStorageLocation.TABLE_NAME} INNER JOIN {KaTank.TABLE_NAME} ON {KaTank.TABLE_NAME}.{KaTank.FN_ID} = {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_TANK_ID} " &
						$"WHERE ({KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_DELETED} = 0) " &
							$"AND ({KaTank.TABLE_NAME}.deleted = {Q(False)}) " &
							$"AND ({KaStorageLocation.FN_TANK_ID} <> {Q(Guid.Empty)}) " &
						$"ORDER BY {KaStorageLocation.FN_NAME}"
			Case "ContainerStorage"
				ddlStorageLocations.Items.Add(New ListItem("New storage location for container", Guid.Empty.ToString()))
				sql = $"SELECT {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_ID}, " &
							$"CASE WHEN {KaContainer.TABLE_NAME}.number = {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_NAME} THEN {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_NAME} ELSE {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_NAME} + ' (Container: ' + {KaContainer.TABLE_NAME}.number + ')' END AS {KaStorageLocation.FN_NAME} " &
						$"FROM {KaStorageLocation.TABLE_NAME} " &
						$"INNER JOIN {KaContainer.TABLE_NAME} ON {KaContainer.TABLE_NAME}.{KaContainer.FN_ID} = {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_CONTAINER_ID} " &
						$"WHERE ({KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_DELETED} = 0) " &
							$"AND ({KaContainer.TABLE_NAME}.deleted = {Q(False)}) " &
							$"AND ({KaStorageLocation.FN_CONTAINER_ID} <> {Q(Guid.Empty)}) " &
						$"ORDER BY {KaStorageLocation.FN_NAME}"
			Case Else
				ddlStorageLocations.Items.Add(New ListItem("New storage location", Guid.Empty.ToString()))
				sql = $"SELECT {KaStorageLocation.FN_ID}, {KaStorageLocation.FN_NAME} " &
						$"FROM {KaStorageLocation.TABLE_NAME} " &
						$"WHERE ({KaStorageLocation.FN_DELETED} = 0) " &
							$"AND ({KaStorageLocation.FN_TANK_ID} = {Q(Guid.Empty)}) " &
							$"AND ({KaStorageLocation.FN_CONTAINER_ID} = {Q(Guid.Empty)}) " &
						$"ORDER BY {KaStorageLocation.FN_NAME}"
		End Select

		Dim storageLocationRdr As OleDbDataReader = Nothing
		Try
			storageLocationRdr = Tm2Database.ExecuteReader(connection, sql)
			Do While storageLocationRdr.Read()
				ddlStorageLocations.Items.Add(New ListItem(storageLocationRdr.Item(KaStorageLocation.FN_NAME), storageLocationRdr.Item(KaStorageLocation.FN_ID).ToString()))
			Loop
		Finally
			If storageLocationRdr IsNot Nothing Then storageLocationRdr.Close()
		End Try
		Try
			ddlStorageLocations.SelectedValue = storageLocationId
		Catch ex As ArgumentOutOfRangeException
			ddlStorageLocations.SelectedIndex = 0
			DisplayJavaScriptMessage("StorageLocationNotFoundWarning", Utilities.JsAlert(String.Format($"The storage location selected could not be assigned, possibly because it has been previously deleted.")))
		End Try
		ddlStorageLocations_SelectedIndexChanged(ddlStorageLocations, New EventArgs())
	End Sub

	Private Sub SetTextboxMaxLengths()
		tbxName.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaStorageLocation.TABLE_NAME, KaStorageLocation.FN_NAME))
	End Sub

	Private Sub ddlStorageLocations_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlStorageLocations.SelectedIndexChanged
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim storageLocation As KaStorageLocation = Nothing
		Try
			If ddlStorageLocations.SelectedIndex > 0 Then storageLocation = New KaStorageLocation(connection, Guid.Parse(ddlStorageLocations.SelectedValue), Nothing)
		Catch ex As RecordNotFoundException
			DisplayJavaScriptMessage("StorageLocationNotFoundWarning", Utilities.JsAlert(String.Format($"The storage location could not be assigned, possibly because it has been previously deleted.")))
			ddlStorageLocations.SelectedIndex = 0
			Exit Sub
		End Try
		If storageLocation Is Nothing Then storageLocation = New KaStorageLocation
		tbxName.Text = storageLocation.Name
		Select Case rblStorageLocationType.SelectedValue
			Case "TankStorage"
				Try
					ddlTank.SelectedValue = storageLocation.TankId.ToString()
				Catch ex As ArgumentOutOfRangeException
					ddlTank.SelectedIndex = 0
					DisplayJavaScriptMessage("TankNotFoundWarning", Utilities.JsAlert(String.Format($"The tank assigned to the storage location could not be assigned, possibly because it has been previously deleted.")))
				End Try
			Case "ContainerStorage"
				Try
					ddlContainer.SelectedValue = storageLocation.ContainerId.ToString()
				Catch ex As ArgumentOutOfRangeException
					ddlContainer.SelectedIndex = 0
					DisplayJavaScriptMessage("ContainerNotFoundWarning", Utilities.JsAlert(String.Format($"The container assigned to the storage location could not be assigned, possibly because it has been previously deleted.")))
				End Try
			Case Else
				Try
					ddlLocation.SelectedValue = storageLocation.LocationId.ToString()
				Catch ex As ArgumentOutOfRangeException
					ddlLocation.SelectedIndex = 0
					DisplayJavaScriptMessage("LocationNotFoundWarning", Utilities.JsAlert(String.Format($"The facility assigned to the storage location could not be assigned, possibly because it has been previously deleted.")))
				End Try
		End Select
		ddlTransferToStorageLocation.Items.Clear()
		ddlTransferToStorageLocation.Items.Add(New ListItem("Select storage location", Guid.Empty.ToString()))
		Dim storageLocationRdr As OleDbDataReader = Nothing
		Try
			storageLocationRdr = Tm2Database.ExecuteReader(connection, $"SELECT {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_ID}, {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_NAME} + CASE WHEN {KaTank.TABLE_NAME}.name IS NULL OR {KaTank.TABLE_NAME}.name = {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_NAME} THEN CASE WHEN {KaContainer.TABLE_NAME}.number IS NULL OR {KaContainer.TABLE_NAME}.number = {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_NAME} THEN '' ELSE ' (Container: ' + {KaContainer.TABLE_NAME}.number + ')' END ELSE ' (Tank: ' + {KaTank.TABLE_NAME}.name + ')' END AS {KaStorageLocation.FN_NAME} " &
							$"FROM {KaStorageLocation.TABLE_NAME} " &
							$"LEFT OUTER JOIN {KaTank.TABLE_NAME} ON {KaTank.TABLE_NAME}.id = {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_TANK_ID} AND {KaTank.TABLE_NAME}.deleted = 0 " &
							$"LEFT OUTER JOIN {KaContainer.TABLE_NAME} ON {KaContainer.TABLE_NAME}.id= {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_CONTAINER_ID} AND {KaContainer.TABLE_NAME}.deleted = 0 " &
							$"WHERE {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_DELETED} = 0 " &
							$"ORDER BY 2")
			While storageLocationRdr.Read()
				If Not storageLocation.Id.Equals(storageLocationRdr.Item(KaStorageLocation.FN_ID)) Then ddlTransferToStorageLocation.Items.Add(New ListItem(storageLocationRdr.Item(KaStorageLocation.FN_NAME), storageLocationRdr.Item(KaStorageLocation.FN_ID).ToString()))
			End While
		Finally
		End Try

		_customFieldData.Clear()
		For Each customFieldValue As KaCustomFieldData In KaCustomFieldData.GetAll(connection, String.Format("deleted = 0 AND {0} = {1}", KaCustomFieldData.FN_RECORD_ID, Q(storageLocation.Id)), KaCustomFieldData.FN_LAST_UPDATED)
			_customFieldData.Add(customFieldValue)
		Next
		Utilities.CreateDynamicCustomFieldPanelControls(_customFields, _customFieldData, lstCustomFields, Page)

		SetLastCleanoutTime(storageLocation)
		pnlStorageLocationContentsAsOfDate.Visible = storageLocation IsNot Nothing AndAlso Not storageLocation.Id.Equals(Guid.Empty)
		SetControlUsabilityFromPermissions()
	End Sub

	Private Sub SetLastCleanoutTime(storageLocation As KaStorageLocation)
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim lastCleanoutMovement As KaStorageLocationMovement = storageLocation.LastCleanoutPriorToDate(connection, Nothing, DateTime.Now, True)
		If lastCleanoutMovement Is Nothing Then
			lblDateOfLastCleanout.Text = "N/A"
		Else
			lblDateOfLastCleanout.Text = String.Format(lastCleanoutMovement.StartDate, "G")
		End If
	End Sub

	Private Sub rblStorageLocationType_SelectedIndexChanged(sender As Object, e As EventArgs) Handles rblStorageLocationType.SelectedIndexChanged
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		If rblStorageLocationType.SelectedIndex < 0 OrElse rblStorageLocationType.SelectedValue Is Nothing Then rblStorageLocationType.SelectedValue = "BulkProductStorage"
		Select Case rblStorageLocationType.SelectedValue
			Case "TankStorage"
				pnlLocation.Visible = False
				pnlTank.Visible = True
				pnlContainer.Visible = False
				PopulateTanks(connection)
			Case "ContainerStorage"
				pnlLocation.Visible = False
				pnlTank.Visible = False
				pnlContainer.Visible = True
				PopulateContainers(connection)
			Case Else
				pnlLocation.Visible = True
				pnlTank.Visible = False
				pnlContainer.Visible = False
				PopulateLocations(connection)
		End Select

		PopulateStorageLocationsList(Guid.Empty.ToString())
	End Sub

	Protected Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
		If ValidateSettings() Then SaveSettings()
	End Sub

	Private Function ValidateSettings() As Boolean
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim testGuid As Guid = Guid.Empty
		If Not Guid.TryParse(ddlStorageLocations.SelectedValue, testGuid) Then
			DisplayJavaScriptMessage("IdWarning", Utilities.JsAlert(Utilities.JsAlert("Unable to parse storage location id")))
			Return False
		ElseIf tbxName.Text.Trim.Length = 0 Then
			DisplayJavaScriptMessage("NameWarning", Utilities.JsAlert(Utilities.JsAlert("Please specify a name")))
			Return False
		End If
		If rblStorageLocationType.SelectedValue = "BulkProductStorage" Then
			If KaStorageLocation.GetAll(connection, $"{KaStorageLocation.FN_DELETED} = 0 AND {KaStorageLocation.FN_ID} <> {Q(testGuid)} AND {KaStorageLocation.FN_NAME} = {Q(tbxName.Text.Trim)} AND {KaStorageLocation.FN_TANK_ID} = {Q(Guid.Empty)} AND {KaStorageLocation.FN_CONTAINER_ID} = {Q(Guid.Empty)}", "").Count > 0 Then
				DisplayJavaScriptMessage("InvalidDuplicateName", Utilities.JsAlert($"The name ""{tbxName.Text.Trim}"" is already in use. Please specify a unique name for this storage location."))
				Return False
			End If
		ElseIf rblStorageLocationType.SelectedValue = "TankStorage" Then
			If Not Guid.TryParse(ddlTank.SelectedValue, testGuid) OrElse testGuid.Equals(Guid.Empty) Then
				DisplayJavaScriptMessage("TankNotAssignedWarning", Utilities.JsAlert(String.Format($"A tank must be assigned to the storage location.")))
				Return False
			ElseIf KaStorageLocation.GetAll(connection, $"{KaStorageLocation.FN_DELETED} = 0 AND {KaStorageLocation.FN_ID} <> {Q(testGuid)} AND {KaStorageLocation.FN_NAME} = {Q(tbxName.Text.Trim)} AND {KaStorageLocation.FN_TANK_ID} = {Q(testGuid)}", "").Count > 0 Then
				DisplayJavaScriptMessage("InvalidDuplicateName", Utilities.JsAlert($"The name ""{tbxName.Text.Trim}"" is already in use for the specified tank. Please specify a unique name for this storage location."))
				Return False
			End If
		ElseIf rblStorageLocationType.SelectedValue = "ContainerStorage" Then
			If Not Guid.TryParse(ddlContainer.SelectedValue, testGuid) OrElse testGuid.Equals(Guid.Empty) Then
				DisplayJavaScriptMessage("ContainerNotAssignedWarning", Utilities.JsAlert(String.Format($"A container must be assigned to the storage location.")))
				Return False
			ElseIf KaStorageLocation.GetAll(connection, $"{KaStorageLocation.FN_DELETED} = 0 AND {KaStorageLocation.FN_ID} <> {Q(testGuid)} AND {KaStorageLocation.FN_NAME} = {Q(tbxName.Text.Trim)} AND {KaStorageLocation.FN_CONTAINER_ID} = {Q(testGuid)}", "").Count > 0 Then
				DisplayJavaScriptMessage("InvalidDuplicateName", Utilities.JsAlert($"The name ""{tbxName.Text.Trim}"" is already in use for the specified container. Please specify a unique name for this storage location."))
				Return False
			End If
		End If
		Return True
	End Function

	Private Function SaveSettings() As Boolean
		Dim successful As Boolean = False

		Dim connection As OleDbConnection = New OleDbConnection(Tm2Database.DbConnection)
		Dim transaction As OleDbTransaction = Nothing
		Try
			connection.Open()
			Dim storageLocation As KaStorageLocation
			Try
				storageLocation = New KaStorageLocation(connection, Guid.Parse(ddlStorageLocations.SelectedValue), Nothing)
			Catch ex As RecordNotFoundException
				storageLocation = New KaStorageLocation
			End Try
			storageLocation.Name = tbxName.Text
			Select Case rblStorageLocationType.SelectedValue
				Case "TankStorage"
					Guid.TryParse(ddlTank.SelectedValue, storageLocation.TankId)
				Case "ContainerStorage"
					Guid.TryParse(ddlContainer.SelectedValue, storageLocation.ContainerId)
				Case Else
					Guid.TryParse(ddlLocation.SelectedValue, storageLocation.LocationId)
			End Select
			transaction = connection.BeginTransaction
			If storageLocation.Id.Equals(Guid.Empty) Then
				storageLocation.SqlUpdateInsertIfNotFound(connection, transaction, Database.ApplicationIdentifier, _currentUser.Name)
				lblStatus.Text = "Storage location successfully added."
			Else
				storageLocation.SqlUpdateInsertIfNotFound(connection, transaction, Database.ApplicationIdentifier, _currentUser.Name)
				lblStatus.Text = "Storage location successfully updated."
			End If
			Utilities.ConvertCustomFieldPanelToLists(_customFields, _customFieldData, lstCustomFields)
			For Each customData As KaCustomFieldData In _customFieldData
				customData.RecordId = storageLocation.Id
				customData.SqlUpdateInsertIfNotFound(connection, transaction, Database.ApplicationIdentifier, _currentUser.Name)
			Next
			transaction.Commit()
			PopulateStorageLocationsList(storageLocation.Id.ToString())
			successful = True
		Catch ex As Exception
			If transaction IsNot Nothing Then transaction.Rollback()
		Finally
			If transaction IsNot Nothing Then transaction.Dispose()
			connection.Close()
		End Try
		Return successful
	End Function

	Protected Sub btnDelete_Click(sender As Object, e As EventArgs) Handles btnDelete.Click
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim storageLocation As KaStorageLocation
		Try
			storageLocation = New KaStorageLocation(connection, Guid.Parse(ddlStorageLocations.SelectedValue), Nothing)
			storageLocation.Deleted = True
			storageLocation.SqlUpdate(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
			lblStatus.Text = "Storage location successfully deleted."
			PopulateStorageLocationsList(Guid.Empty.ToString())
		Catch ex As RecordNotFoundException
			DisplayJavaScriptMessage("StorageLocationNotFoundWarning", Utilities.JsAlert(String.Format($"The storage location selected could not be found in the database, possibly because it has been previously deleted.")))
		End Try
	End Sub

	Protected Sub btnMarkedAsCleanedOut_Click(sender As Object, e As EventArgs) Handles btnMarkedAsCleanedOut.Click
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim storageLocation As KaStorageLocation
		Try
			storageLocation = New KaStorageLocation(connection, Guid.Parse(ddlStorageLocations.SelectedValue), Nothing)
			Dim dateOfCleanout As DateTime = DateTime.Parse(tbxCleanoutDate.Value) ' converting string value to datetime value

			storageLocation.MarkStorageLocationAsCleanedOut(connection, Nothing, dateOfCleanout, Database.ApplicationIdentifier, _currentUser.Name)
			SetLastCleanoutTime(storageLocation)
			lblStatus.Text = $"Storage location marked as cleaned out at {dateOfCleanout.ToString()}"
		Catch ex As FormatException
			DisplayJavaScriptMessage("InvalidCleanoutDate", Utilities.JsAlert("Please enter a valid date for the clean-out date"))
		Catch ex As RecordNotFoundException
			DisplayJavaScriptMessage("StorageLocationNotFoundWarning", Utilities.JsAlert(String.Format($"The storage location selected could not be found in the database, possibly because it has been previously deleted.")))
		End Try
	End Sub

	Protected Sub btnTransferToStorageLocation_Click(sender As Object, e As EventArgs) Handles btnTransferToStorageLocation.Click
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim storageLocation As KaStorageLocation
		Dim transferStorageLocation As KaStorageLocation
		Try
			storageLocation = New KaStorageLocation(connection, Guid.Parse(ddlStorageLocations.SelectedValue), Nothing)
			Try
				transferStorageLocation = New KaStorageLocation(connection, Guid.Parse(ddlTransferToStorageLocation.SelectedValue), Nothing)
			Catch ex As RecordNotFoundException
				DisplayJavaScriptMessage("StorageLocationNotFoundWarning", Utilities.JsAlert(String.Format($"The transfer storage location selected could not be found in the database, possibly because it has been previously deleted.")))
				Exit Sub
			End Try
			Dim dateOfTransferStart As DateTime
			Try
				dateOfTransferStart = DateTime.Parse(tbxTransferStart.Value) ' converting string value to datetime value
			Catch ex As FormatException
				DisplayJavaScriptMessage("InvalidTransferStartDate", Utilities.JsAlert("Please enter a valid date for the transfer start date"))
				Exit Sub
			End Try
			Dim dateOfTransferStop As DateTime
			Try
				dateOfTransferStop = DateTime.Parse(tbxTransferStop.Value) ' converting string value to datetime value
			Catch ex As FormatException
				DisplayJavaScriptMessage("InvalidTransferStopDate", Utilities.JsAlert("Please enter a valid date for the transfer stop date"))
				Exit Sub
			End Try
			If dateOfTransferStart > dateOfTransferStop Then
				DisplayJavaScriptMessage("InvalidTransferDate", Utilities.JsAlert("The transfer start date must occur prior to the transfer stop date"))
				Exit Sub
			End If
			storageLocation.TransferToStorageLocation(connection, Nothing, transferStorageLocation.Id, dateOfTransferStart, dateOfTransferStop, Database.ApplicationIdentifier, _currentUser.Name)
			lblStatus.Text = $"Storage location transferred to {transferStorageLocation.Name} at {dateOfTransferStart.ToString()}"
		Catch ex As RecordNotFoundException
			DisplayJavaScriptMessage("StorageLocationNotFoundWarning", Utilities.JsAlert(String.Format($"The storage location selected could not be found in the database, possibly because it has been previously deleted.")))
		End Try
	End Sub

	Private Sub SetControlUsabilityFromPermissions()
		With _currentUserPermission(_currentTableName)
			Dim shouldEnable = (.Edit AndAlso ddlStorageLocations.SelectedIndex > 0) OrElse (.Create AndAlso ddlStorageLocations.SelectedIndex = 0)
			tbxName.Enabled = shouldEnable
			pnlGeneralInformation.Enabled = shouldEnable
			btnSave.Enabled = shouldEnable
			Dim value = Guid.Parse(ddlStorageLocations.SelectedValue)
			plnStorageLocationFunctions.Visible = value <> Guid.Empty
			btnDelete.Enabled = .Edit AndAlso .Delete AndAlso value <> Guid.Empty
			btnMarkedAsCleanedOut.Enabled = .Edit AndAlso value <> Guid.Empty
			btnTransferToStorageLocation.Enabled = .Edit AndAlso value <> Guid.Empty
		End With
	End Sub

	Protected Sub ToolkitScriptManager1_AsyncPostBackError(ByVal sender As Object, ByVal e As System.Web.UI.AsyncPostBackErrorEventArgs)
		ToolkitScriptManager1.AsyncPostBackErrorMessage = e.Exception.Message
	End Sub

	Private Sub DisplayJavaScriptMessage(key As String, script As String)
		ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), key, script, False)
	End Sub

	Protected Sub btnStorageLocationContentsAsOfDate_Click(sender As Object, e As EventArgs) Handles btnStorageLocationContentsAsOfDate.Click
		Dim storageLocation As KaStorageLocation
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim storageLocationContentsAsOfDate As DateTime
		Try
			storageLocationContentsAsOfDate = DateTime.Parse(tbxStorageLocationContentsAsOfDate.Value) ' converting string value to datetime value
		Catch ex As FormatException
			DisplayJavaScriptMessage("InvalidTransferStartDate", Utilities.JsAlert("Please enter a valid date for the start date"))
			Exit Sub
		End Try
		Dim receivingTicketUsageList As ArrayList = New ArrayList
		Dim bulkProducts As Dictionary(Of Guid, KaBulkProduct) = New Dictionary(Of Guid, KaBulkProduct)
		Dim owners As Dictionary(Of Guid, KaOwner) = New Dictionary(Of Guid, KaOwner)
		Try
			storageLocation = New KaStorageLocation(connection, Guid.Parse(ddlStorageLocations.SelectedValue), Nothing)
			Dim listOfStorageLocationMovements As List(Of KaStorageLocationMovement) = storageLocation.GetStorageLocationMovementsAsOfDate(connection, Nothing, storageLocationContentsAsOfDate)
			For Each slm As KaStorageLocationMovement In listOfStorageLocationMovements
				If Not slm.ReceivingTicketId.Equals(Guid.Empty) Then
					Try
						Dim receivingTicket As KaReceivingTicket = New KaReceivingTicket(connection, slm.ReceivingTicketId)
						Dim webTicketAddress As String = "ReceivingTicketPFV.aspx?receiving_ticket_id=" & receivingTicket.Id.ToString()
						webTicketAddress &= "&instanceGuid=" & Guid.NewGuid.ToString()
						webTicketAddress = $"<a href=""{webTicketAddress}"" target=""_blank"">{System.Web.HttpUtility.HtmlEncode(receivingTicket.Number)}{IIf(receivingTicket.Voided, $" (voided)", "")}</a>"
						Dim orderAddress As String
						Try
							If New KaReceivingPurchaseOrder(connection, receivingTicket.ReceivingPurchaseOrderId).Completed Then
								orderAddress = "<a href=""PastReceiving.aspx?ReceivingPurchaseOrderId="
							Else
								orderAddress = "<a href=""Receiving.aspx?ReceivingPurchaseOrderId="
							End If
							orderAddress &= receivingTicket.ReceivingPurchaseOrderId.ToString & """ target=""_blank"">" & System.Web.HttpUtility.HtmlEncode(receivingTicket.PurchaseOrderNumber) & "</a>"
						Catch ex As RecordNotFoundException
							orderAddress = receivingTicket.PurchaseOrderNumber
						End Try
						receivingTicketUsageList.Add(New ArrayList({String.Format(receivingTicket.DateOfDelivery, "G"), KaOwner.GetOwner(connection, receivingTicket.OwnerId, owners, Nothing).Name, KaBulkProduct.GetBulkProduct(connection, receivingTicket.BulkProductId, bulkProducts, Nothing).Name, orderAddress, webTicketAddress, String.Format(slm.StartDate, "G"), String.Format(slm.StopDate, "G")}))
					Catch ex As RecordNotFoundException
					End Try
				End If
			Next
		Catch ex As RecordNotFoundException
		End Try
		If receivingTicketUsageList.Count > 0 Then
			receivingTicketUsageList.Insert(0, New ArrayList({"Received at", "Owner", "Bulk product", "Purchase order number", "Ticket", "Start time", "Stop time"}))
			litStorageLocationContentsAsOfDate.Text = KaReports.GetTableHtml("", "", receivingTicketUsageList, False, "style=""margin: 0.5em; width: 100%;""", "", New List(Of String), "", New List(Of String))
		Else
			DisplayJavaScriptMessage("NoStorageLocationMovemntsfound", Utilities.JsAlert("No records found"))
		End If
	End Sub

	Protected Overrides Function SaveViewState() As Object
		Dim viewState(2) As Object
		'Saving the grid values to the View State
		Utilities.ConvertCustomFieldPanelToLists(_customFields, _customFieldData, lstCustomFields)
		viewState(0) = MyBase.SaveViewState()
		viewState(1) = _customFields
		viewState(2) = _customFieldData
		Return viewState
	End Function

	Protected Overrides Sub LoadViewState(savedState As Object)
		'Getting the dropdown list value from view state.
		If savedState IsNot Nothing AndAlso CType(savedState, Object).Length > 1 Then
			_currentUser = Utilities.GetUser(Me)
			Dim viewState As Object() = savedState
			MyBase.LoadViewState(viewState(0))
			_customFields = viewState(1)
			_customFieldData = viewState(2)
			Utilities.CreateDynamicCustomFieldPanelControls(_customFields, _customFieldData, lstCustomFields, Page)
		Else
			MyBase.LoadViewState(savedState)
		End If
	End Sub
End Class