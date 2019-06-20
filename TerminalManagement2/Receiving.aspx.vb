Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class Receiving : Inherits System.Web.UI.Page

	Private _currentUser As KaUser
	Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
	Private _currentTableName As String = KaReceivingPurchaseOrder.TABLE_NAME
	Private _customFields As New List(Of KaCustomField)
	Private _customFieldData As New List(Of KaCustomFieldData)

#Region "Events"
	Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
		Response.Cache.SetCacheability(HttpCacheability.NoCache)
		_currentUser = Utilities.GetUser(Me)
		_currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "PurchaseOrders")
		If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")
		lblStatus.Text = ""
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		If Not Page.IsPostBack Then
			SetTextboxMaxLengths()
			PopulateFacilityList()
			If Page.Request("ReceivingPurchaseOrderId") Is Nothing Then
				Try
					ddlFacilityFilter.SelectedValue = KaSetting.GetSetting(connection, "PurchaseOrdersPage:" & _currentUser.Id.ToString & "/LastFacilityUsed", Guid.Empty.ToString())
				Catch ex As ArgumentOutOfRangeException

				End Try
			Else
				ddlFacilityFilter.SelectedIndex = 0
			End If

			_customFields.Clear()
			For Each customField As KaCustomField In KaCustomField.GetAll(connection, String.Format("deleted=0 AND {0} = {1}", KaCustomField.FN_TABLE_NAME, Q(KaReceivingPurchaseOrder.TABLE_NAME)), KaCustomField.FN_FIELD_NAME)
				_customFields.Add(customField)
			Next

			PopulateReceivingPurchaseOrders()
			PopulateOwnerList()
			PopulateSupplierAccountList()
			PopulateBulkProductList()
			PopulateUnitList()
			PopulateCarrierList()
			PopulateTransportList()
			PopulateDriverList()
			PopulateLocationList()

			Dim receivingPurchaseOrderId As Guid = Guid.Empty
			Guid.TryParse(Page.Request("ReceivingPurchaseOrderId"), receivingPurchaseOrderId)
			Try
				ddlPurchaseOrders.SelectedValue = receivingPurchaseOrderId.ToString()
			Catch ex As ArgumentOutOfRangeException
			End Try

			ddlPurchaseOrders_SelectedIndexChanged(ddlPurchaseOrders, New EventArgs())

			Dim modifiable As Boolean = _currentUserPermission(_currentTableName).Edit
			btnSave.Enabled = modifiable OrElse _currentUserPermission(_currentTableName).Create
			btnNewCarrier.Enabled = modifiable AndAlso Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaCarrier.TABLE_NAME}), "Carriers")(KaCarrier.TABLE_NAME).Edit
			btnNewTransport.Enabled = modifiable AndAlso Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaTransport.TABLE_NAME}), "Transports")(KaTransport.TABLE_NAME).Edit
			btnNewDriver.Enabled = modifiable AndAlso Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaDriver.TABLE_NAME}), "Drivers")(KaDriver.TABLE_NAME).Edit
			Utilities.ConfirmBox(Me.btnMarkComplete, "Are you sure you want to mark this Receiving Purchase Order as completed?")
			Utilities.ConfirmBox(Me.btnDelete, "Are you sure you want to delete this receiving purchase order?") ' Delete confirmation box setup
			Utilities.ConfirmBox(Me.btnVoidTicket, "Are you sure you want to void this receiving purchase order ticket?")
			Utilities.SetFocus(tbxNumber, Me) ' set focus to the first textbox on the page
		End If
		pnlReceivingTicketUsage.Attributes("display") = "none"
	End Sub

	Protected Sub ddlPurchaseOrders_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles ddlPurchaseOrders.SelectedIndexChanged
		PopulateReceivingPurchaseOrder()
		SetControlUsabilityFromPermissions()
	End Sub

	Protected Sub btnSave_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnSave.Click
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		If ValidateSaveFields(connection) Then
			With New KaReceivingPurchaseOrder()
				.Id = Guid.Parse(ddlPurchaseOrders.SelectedValue)

				If .Id <> Guid.Empty Then .SqlSelect(connection)
				.OwnerId = Guid.Parse(ddlOwner.SelectedValue)
				Dim orderNumber As String = tbxNumber.Text.Trim
				Dim orderNumbersByOwner As Boolean = SeparateOrderNumberPerOwner()
				If orderNumber.Length = 0 OrElse orderNumber.ToLower = "automatically generated" Then
					Do
						' get the next available order number that's not already in use
						orderNumber = GetNextOrderNumber(.OwnerId, orderNumbersByOwner)
					Loop While Not CheckOrderNumber(.Id, orderNumber, .OwnerId, orderNumbersByOwner)
				End If
				.Number = orderNumber
				.SupplierAccountId = Guid.Parse(ddlSupplier.SelectedValue)
				.BulkProductId = Guid.Parse(ddlBulkProduct.SelectedValue)
				.Notes = tbxNotes.Text.Trim
				.Purchased = tbxPurchased.Text
				.UnitId = Guid.Parse(ddlUnit.SelectedValue)
				Dim status As String = ""

				Dim transaction As OleDbTransaction = connection.BeginTransaction(IsolationLevel.Serializable)
				Try
					If .Id = Guid.Empty Then
						.SqlInsert(connection, transaction, Database.ApplicationIdentifier, _currentUser.Name)
						btnDelete.Enabled = _currentUserPermission(_currentTableName).Edit AndAlso _currentUserPermission(_currentTableName).Delete
						status = "Receiving purchase order successfully added."
					Else
						.SqlUpdate(connection, transaction, Database.ApplicationIdentifier, _currentUser.Name)
						status = "Receiving purchase order successfully updated."
					End If
					Utilities.ConvertCustomFieldPanelToLists(_customFields, _customFieldData, lstCustomFields)
					For Each customData As KaCustomFieldData In _customFieldData
						customData.RecordId = .Id
						customData.SqlUpdateInsertIfNotFound(connection, transaction, Database.ApplicationIdentifier, _currentUser.Name)
					Next

					transaction.Commit()
				Catch ex As Exception
					transaction.Rollback()
					Throw ex ' re-throw the exception
				End Try
				PopulateReceivingPurchaseOrders()
				Try
					ddlPurchaseOrders.SelectedValue = .Id.ToString()
				Catch ex As ArgumentOutOfRangeException

				End Try
				ddlPurchaseOrders_SelectedIndexChanged(ddlPurchaseOrders, New EventArgs())
				lblStatus.Text = status
			End With
		End If
	End Sub

	Private Function ValidateSaveFields(connection As OleDbConnection) As Boolean
		If ddlOwner.SelectedIndex <= 0 Then DisplayJavaScriptMessage("InvalidOwner", Utilities.JsAlert("An owner must be selected.")) : Return False
		If ddlSupplier.SelectedIndex <= 0 Then DisplayJavaScriptMessage("InvalidSupplier", Utilities.JsAlert("A supplier must be selected.")) : Return False
		If ddlBulkProduct.SelectedIndex <= 0 Then DisplayJavaScriptMessage("InvalidBulkProduct", Utilities.JsAlert("A bulk product must be selected.")) : Return False
		If Not IsNumeric(tbxPurchased.Text) OrElse Double.Parse(tbxPurchased.Text) <= 0 Then DisplayJavaScriptMessage("InvalidQuantity", Utilities.JsAlert("Purchased quantity must be a numeric value greater than zero.")) : Return False
		If ddlUnit.SelectedIndex = 0 Then DisplayJavaScriptMessage("InvalidUnit", Utilities.JsAlert("A unit of measure must be selected.")) : Return False
		Dim poId As Guid = Guid.Parse(ddlPurchaseOrders.SelectedValue)
		Dim currentPo As KaReceivingPurchaseOrder
		Try
			currentPo = New KaReceivingPurchaseOrder(connection, poId)
		Catch ex As RecordNotFoundException
			currentPo = New KaReceivingPurchaseOrder()
		End Try
		If currentPo.Number <> tbxNumber.Text AndAlso Not CheckOrderNumber(poId, tbxNumber.Text, Guid.Parse(ddlOwner.SelectedValue), SeparateOrderNumberPerOwner()) Then
			DisplayJavaScriptMessage("InvalidNumberAlreadyUsed", Utilities.JsAlert("Purchased number " & tbxNumber.Text & " already in use."))
			Return False
		End If

		Return True
	End Function

	Protected Sub btnDelete_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnDelete.Click
		With New KaReceivingPurchaseOrder(GetUserConnection(_currentUser.Id), Guid.Parse(ddlPurchaseOrders.SelectedValue))
			.Deleted = True
			.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
			PopulateReceivingPurchaseOrders()
			ddlPurchaseOrders.SelectedIndex = 0
			ddlPurchaseOrders_SelectedIndexChanged(ddlPurchaseOrders, New EventArgs())
			DisplayJavaScriptMessage("DeletionSuccessful", "Receiving purchase order successfully deleted.")
		End With
	End Sub

	Protected Sub ddlCopy_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnCopy.Click
		Dim number As String = tbxNumber.Text
		Do While KaReceivingPurchaseOrder.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND number=" & Q(number) & IIf(_currentUser.OwnerId = Guid.Empty, "", " AND owner_id=" & Q(_currentUser.OwnerId)), "").Count > 0
			number = IncrementAlphaNumeric(number)
		Loop
		tbxStartNumber.Text = number
		tbxCopies.Text = "1"
		pnlMain.Visible = False
		pnlCopy.Visible = True
		pnlTickets.Visible = False
	End Sub

	Protected Sub btnCreateCopies_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnCreateCopies.Click
		If tbxStartNumber.Text.Trim().Length = 0 Then DisplayJavaScriptMessage("InvalidStartNumber", Utilities.JsAlert("Start number must be specified.")) : Exit Sub
		If Not IsNumeric(tbxCopies.Text) OrElse Integer.Parse(tbxCopies.Text) <= 0 Then DisplayJavaScriptMessage("InvalidCopiesValue", Utilities.JsAlert("Copies must be a numeric value greater than zero.")) : Exit Sub
		Dim r As New KaReceivingPurchaseOrder(GetUserConnection(_currentUser.Id), Guid.Parse(ddlPurchaseOrders.SelectedValue))
		Dim number As String = tbxStartNumber.Text
		Dim copies As Integer = Integer.Parse(tbxCopies.Text)
		Dim i As Integer = 0
		Do While i < copies
			Do While KaReceivingPurchaseOrder.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND number=" & Q(number) & IIf(_currentUser.OwnerId = Guid.Empty, "", " AND owner_id=" & Q(_currentUser.OwnerId)), "").Count > 0
				number = IncrementAlphaNumeric(number)
			Loop
			r.Number = number
			r.SqlInsert(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
			i += 1
		Loop
		PopulateReceivingPurchaseOrders()
		ddlPurchaseOrders.SelectedValue = r.Id.ToString()
		pnlMain.Visible = True
		pnlCopy.Visible = False
	End Sub

	Protected Sub btnCancelCopy_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnCancelCopy.Click
		pnlMain.Visible = True
		pnlCopy.Visible = False
		ddlPurchaseOrders_SelectedIndexChanged(ddlPurchaseOrders, New EventArgs())
	End Sub

	Protected Sub btnNewCarrier_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnNewCarrier.Click
		pnlExistingCarrier.Visible = False
		pnlNewCarrier.Visible = True
	End Sub

	Protected Sub btnListCarriers_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnListCarriers.Click
		pnlExistingCarrier.Visible = True
		pnlNewCarrier.Visible = False
	End Sub

	Protected Sub btnNewTransport_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnNewTransport.Click
		pnlExistingTransport.Visible = False
		pnlNewTransport.Visible = True
	End Sub

	Protected Sub btnListTransports_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnListTransports.Click
		pnlExistingTransport.Visible = True
		pnlNewTransport.Visible = False
	End Sub

	Protected Sub btnNewDriver_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnNewDriver.Click
		pnlExistingDriver.Visible = False
		pnlNewDriver.Visible = True
	End Sub

	Protected Sub btnListDrivers_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnListDrivers.Click
		pnlExistingDriver.Visible = True
		pnlNewDriver.Visible = False
	End Sub

	Protected Sub btnReceiveOk_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnReceiveOk.Click
		If ValidateReceiveFields() Then
			btnSave_Click(sender, e)
			Dim id As String = ddlPurchaseOrders.SelectedValue
			Dim ticketId As Guid = Guid.Empty
			Dim connection As OleDbConnection = Nothing
			Dim transaction As OleDbTransaction = Nothing
			Try
				connection = New OleDbConnection(Tm2Database.GetDbConnection())
				connection.Open()
				Dim emailTicket As Boolean = True
				Boolean.TryParse(KaSetting.GetSetting(connection, ReceivingPoSettings.SN_EMAIL_RECEIVING_POINT_OF_SALE_TICKETS, ReceivingPoSettings.SD_EMAIL_RECEIVING_POINT_OF_SALE_TICKETS), emailTicket)
				transaction = connection.BeginTransaction
				Dim inProgress As KaInProgress = CreateInProgress()
				If Tm2Database.SystemItemTraceabilityEnabled Then
					Dim lotId As Guid = Guid.Empty
					If ddlReceivingLot.SelectedIndex = 1 AndAlso ddlReceivingLot.SelectedValue = "New" Then
						Dim newLot As KaLot = New KaLot() With {
							.Number = tbxNewReceivingLotNumber.Text.Trim(),
							.BulkProductId = inProgress.Weighments(0).BulkProductId
						}
						newLot.SqlInsert(connection, transaction, Database.ApplicationIdentifier, _currentUser.Name)
						lotId = newLot.Id
					ElseIf Guid.TryParse(ddlReceivingLot.SelectedValue, lotId) Then
						Try
							Dim lot As KaLot = New KaLot(connection, lotId, transaction)
							Dim bp As New KaBulkProduct(connection, lot.BulkProductId, transaction)
							If lot.Complete AndAlso bp.LotUsageMethod <> KaBulkProduct.LotUsage.NotDefined Then ' Reset the complete flag since this is a new receipt
								lot.Complete = False
								lot.SqlUpdate(connection, transaction, Database.ApplicationIdentifier, _currentUser.Name)
							End If
						Catch ex As RecordNotFoundException

						End Try
					End If
					For Each weighment As KaInProgressWeighment In inProgress.Weighments
						weighment.LotId = lotId
					Next
				End If

				Dim ticketNumbers As List(Of String) = New List(Of String)
				Dim processStartDate As DateTime = DateTime.Now
				Dim ticket As Object = inProgress.CreateTicket(connection, transaction)
				If GetType(ticket) Is GetType(KaReceivingTicket) Then
					Dim rt As KaReceivingTicket = CType(ticket, KaReceivingTicket)
					rt.DoNotEmail = Not emailTicket
					rt.SqlUpdate(connection, transaction, Database.ApplicationIdentifier, _currentUser.Name)
					For Each i As ListItem In cblStorageLocations.Items
						If i.Selected Then
							Dim storageLocationId As Guid = Guid.Parse(i.Value)
							KaReceivingTicket.CreateStorageLocationMovementRecords(connection, transaction, rt.Id, processStartDate, processStartDate, True, storageLocationId, rt.LotId, Database.ApplicationIdentifier, _currentUser.Name)
						End If
					Next
					ticketId = rt.Id
					ticketNumbers.Add(rt.Number)
				Else  ' multiple tickets
					For Each rt As KaReceivingTicket In CType(ticket, List(Of KaReceivingTicket))
						rt.DoNotEmail = Not emailTicket
						rt.SqlUpdate(connection, transaction, Database.ApplicationIdentifier, _currentUser.Name)
						For Each i As ListItem In cblStorageLocations.Items
							If i.Selected Then
								Dim storageLocationId As Guid = Guid.Parse(i.Value)
								KaReceivingTicket.CreateStorageLocationMovementRecords(connection, transaction, rt.Id, processStartDate, processStartDate, True, storageLocationId, rt.LotId, Database.ApplicationIdentifier, _currentUser.Name)
							End If
						Next
						ticketId = rt.Id
						If Not ticketNumbers.Contains(rt.Number) Then ticketNumbers.Add(rt.Number)
					Next
				End If
				transaction.Commit()
				If ticketNumbers.Count > 0 Then lblStatus.Text = $"Ticket number{IIf(ticketNumbers.Count > 1, "s", "")} created: {String.Join(", ", ticketNumbers)}"
			Catch ex As Exception
				If transaction IsNot Nothing Then transaction.Rollback()
			Finally
				If transaction IsNot Nothing Then transaction.Dispose()
				If connection IsNot Nothing Then connection.Close()
			End Try

			PopulateReceivingPurchaseOrders()
			pnlReceive.Visible = False

			pnlMain.Visible = True
			Try
				ddlPurchaseOrders.SelectedValue = id
			Catch ex As ArgumentOutOfRangeException
				ddlPurchaseOrders.SelectedIndex = 0
			End Try
			ddlPurchaseOrders_SelectedIndexChanged(ddlPurchaseOrders, New EventArgs())
			Try
				ddlReceivingTickets.SelectedValue = ticketId.ToString()
				ddlReceivingTickets_SelectedIndexChanged(ddlReceivingTickets, New EventArgs())
			Catch ex As ArgumentOutOfRangeException
			End Try
		End If
	End Sub

	Private Function CreateInProgress() As KaInProgress
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim retval As KaInProgress = New KaInProgress
		retval.Weighments.Clear()
		retval.Orders.Clear()
		retval.ManufacturedOrders.Clear()
		retval.UsedBy = "TM2\Receiving"

		Dim receivingPurcahseOrder As KaReceivingPurchaseOrder = New KaReceivingPurchaseOrder(connection, Guid.Parse(ddlPurchaseOrders.SelectedValue))
		Dim bulkProd As New KaBulkProduct(connection, Guid.Parse(ddlBulkProduct.SelectedValue))
		Dim amount As Double = Double.Parse(tbxDelivered.Text)

		With retval
			.ReceivingPurchaseOrderId = receivingPurcahseOrder.Id
			.LocationId = Guid.Parse(ddlLocation.SelectedValue)
			.OrderType = KaInProgress.OrderTypes.ReceivingPurchaseOrder
			PopulateTicketCarrier()
			PopulateTicketDriver()
			.CarrierId = Guid.Parse(ddlCarrier.SelectedValue)
			.DriverId = Guid.Parse(ddlDriver.SelectedValue)
			.Notes = tbxReceivingNotes.Text
			.PointOfSale = True
			.LastUpdatedApplication = Database.ApplicationIdentifier
			.LastUpdatedUser = _currentUser.Name
			.SqlInsert(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
		End With

		Dim timestamp As DateTime = DateTime.Now

		Dim weighment As New KaInProgressWeighment
		With weighment
			.InProgressId = retval.Id
			.Requested = amount
			.AverageDensity = bulkProd.Density()
			.Complete = True
			.Gross = amount
			.GrossDate = timestamp
			.GrossManual = True
			.ProductId = Nothing
			.BulkProductId = bulkProd.Id
			.OrderItemId = Nothing
			.Tare = 0
			.TareDate = timestamp
			.TareManual = True
			PopulateTicketTransport()
			.TransportId = Guid.Parse(ddlTransport.SelectedValue)
			.UnitId = Guid.Parse(ddlDeliveredUnit.SelectedValue)
			.UserId = _currentUser.Id
			.VolumeUnitId = bulkProd.VolumeUnitId
			.WeightUnitId = bulkProd.WeightUnitId
			.PanelId = Nothing
			.Delivered = amount
			.DeliveredWithoutTempAdjust = amount
			.UseDelivered = True
			.StartDate = timestamp
			.CompletedDate = timestamp
			.UserId = _currentUser.Id

			' Attempt to set the tare information from the transport information
			Dim specifiedTare As New KaQuantity(0, .UnitId)
			Double.TryParse(tbxTransportTareWeight.Text, specifiedTare.Numeric)
			Guid.TryParse(ddlTransportUnit.SelectedValue, specifiedTare.UnitId)
			If specifiedTare.Numeric > 0 AndAlso tbxTransportTareDate.Text.Trim.Length > 0 Then DateTime.TryParse(tbxTransportTareDate.Text, .TareDate)
			If .TareDate <= SQL_MINDATE Then .TareDate = timestamp
			Try
				Dim transportInfo As New KaTransport(connection, weighment.TransportId)
				Dim emptyDensity As New KaRatio(0, .WeightUnitId, .VolumeUnitId)
				Dim transportTare As KaQuantity = KaUnit.Convert(connection, New KaQuantity(transportInfo.TareWeight, transportInfo.UnitId), emptyDensity, .UnitId)
				.Tare = KaUnit.Convert(connection, specifiedTare, emptyDensity, .UnitId).Numeric
				.Gross = amount + .Tare
				If transportTare.Numeric = .Tare AndAlso transportTare.UnitId = .UnitId AndAlso .TareDate = transportInfo.TaredAt Then
					.TareManual = transportInfo.TareManual
				End If
			Catch ex As Exception

			End Try

			.LastUpdatedApplication = Database.ApplicationIdentifier
			.LastUpdatedUser = _currentUser.Name
			.SqlInsert(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
		End With

		retval.Weighments.Add(weighment)

		retval.SqlUpdate(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
		Return retval
	End Function

	Private Function ValidateReceiveFields() As Boolean
		If Guid.Parse(ddlLocation.SelectedValue) = Guid.Empty Then DisplayJavaScriptMessage("InvalidFacility", Utilities.JsAlert("A Facility must be selected.")) : Return False
		If Not IsNumeric(tbxDelivered.Text) OrElse Double.Parse(tbxDelivered.Text) <= 0 Then DisplayJavaScriptMessage("InvalidQuantity", Utilities.JsAlert("Delivered must be a numeric value greater than zero.")) : Return False
		If ddlDeliveredUnit.SelectedIndex = 0 Then DisplayJavaScriptMessage("InvalidUnitOfMeasure", Utilities.JsAlert("Delivered unit of measure must be selected.")) : Return False
		If pnlNewCarrier.Visible Then
			If tbxCarrierName.Text.Trim().Length = 0 Then DisplayJavaScriptMessage("InvalidCarrier", Utilities.JsAlert("Carrier name must be specified.")) : Return False
			If KaCarrier.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND name=" & Q(tbxCarrierName.Text), "").Count > 0 Then DisplayJavaScriptMessage("InvalidNameExists", Utilities.JsAlert("Carrier already exists with name = " & tbxCarrierName.Text & ".")) : Return False
			If tbxCarrierNumber.Text.Trim().Length > 0 AndAlso KaCarrier.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND number=" & Q(tbxCarrierNumber.Text), "").Count > 0 Then DisplayJavaScriptMessage("InvalidnumberUsed", Utilities.JsAlert("Carrier already exists with number = " & tbxCarrierNumber.Text & ".")) : Return False
		End If
		If pnlNewTransport.Visible Then
			If tbxTransportName.Text.Trim().Length = 0 Then DisplayJavaScriptMessage("InvalidTransport", Utilities.JsAlert("Transport name must be specified.")) : Return False
			If KaTransport.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND name=" & Q(tbxTransportName.Text), "").Count > 0 Then DisplayJavaScriptMessage("InvalidNameUsed", Utilities.JsAlert("Transport already exists with name = " & tbxTransportName.Text & ".")) : Return False
			If tbxTransportNumber.Text.Trim().Length > 0 AndAlso KaTransport.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND number=" & Q(tbxTransportNumber.Text), "").Count > 0 Then DisplayJavaScriptMessage("InvalidNumberUsed", Utilities.JsAlert("Transport already exists with number = " & tbxTransportNumber.Text & ".")) : Return False
			If Not IsNumeric(tbxTransportTareWeight.Text) OrElse Double.Parse(tbxTransportTareWeight.Text) < 0 Then DisplayJavaScriptMessage("InvalidTareWeight", Utilities.JsAlert("Transport tare weight must be a numeric value greater than or equal to zero.")) : Return False
			If ddlTransportUnit.SelectedIndex = 0 Then DisplayJavaScriptMessage("InvalidTareUnit", Utilities.JsAlert("Transport tare weight unit of measure must be selected.")) : Return False
		End If
		If pnlNewDriver.Visible Then
			If tbxDriverName.Text.Trim().Length = 0 Then DisplayJavaScriptMessage("InvalidDriver", Utilities.JsAlert("Driver name must be specified.")) : Return False
			If KaDriver.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND name=" & Q(tbxDriverName.Text), "").Count > 0 Then DisplayJavaScriptMessage("InvalidDriverNameExists", Utilities.JsAlert("Driver already exists with name = " & tbxDriverName.Text & ".")) : Return False
			If tbxDriverNumber.Text.Trim().Length > 0 AndAlso KaDriver.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND number=" & Q(tbxDriverNumber.Text), "").Count > 0 Then DisplayJavaScriptMessage("InvalidDriverNumberExists", Utilities.JsAlert("Driver already exists with number = " & tbxDriverNumber.Text & ".")) : Return False
		End If
		If ddlReceivingLot.SelectedIndex = 1 AndAlso ddlReceivingLot.SelectedValue = "New" Then
			If tbxNewReceivingLotNumber.Text.Trim().Length = 0 Then DisplayJavaScriptMessage("InvalidLotNumber", Utilities.JsAlert("New lot number must be specified.")) : Return False
		End If

		Return True
	End Function

	Private Sub PopulateTicketCarrier()
		With New KaCarrier()
			If pnlNewCarrier.Visible Then
				.Name = tbxCarrierName.Text
				.Number = tbxCarrierNumber.Text
				.SqlInsert(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
				PopulateCarrierList()
				ddlCarrier.SelectedValue = .Id.ToString()
				btnListCarriers_Click(btnListCarriers, New EventArgs())
			End If
		End With
	End Sub

	Private Sub PopulateTicketDriver()
		With New KaDriver()
			If pnlNewDriver.Visible Then
				.Name = tbxDriverName.Text
				.Number = tbxDriverNumber.Text
				.SqlInsert(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
				PopulateDriverList()
				ddlDriver.SelectedValue = .Id.ToString()
				btnListDrivers_Click(btnListDrivers, New EventArgs())
			End If
		End With
	End Sub

	Private Sub PopulateTicketTransport()
		With New KaTransport()
			If pnlNewTransport.Visible Then
				.Name = tbxTransportName.Text
				.Number = tbxTransportNumber.Text
				Dim taredAt As DateTime = SQL_MINDATE
				If DateTime.TryParse(tbxTransportTareDate.Text, taredAt) Then
					.TaredAt = taredAt
				Else
					.TaredAt = SQL_MINDATE
				End If
				.TareWeight = tbxTransportTareWeight.Text
				.UnitId = Guid.Parse(ddlTransportUnit.SelectedValue)
				.SqlInsert(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
				PopulateTransportList()
				ddlTransport.SelectedValue = .Id.ToString()
				btnListTransports_Click(btnListTransports, New EventArgs())
			End If
		End With
	End Sub

	Protected Sub btnReceiveCancel_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnReceiveCancel.Click
		pnlReceive.Visible = False
		pnlMain.Visible = True
		ddlPurchaseOrders_SelectedIndexChanged(ddlPurchaseOrders, New EventArgs())
	End Sub

	Protected Sub btnReceive_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnReceive.Click
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		If Guid.Parse(ddlPurchaseOrders.SelectedValue) <> Guid.Empty AndAlso ValidateSaveFields(connection) Then
			Dim po As KaReceivingPurchaseOrder = New KaReceivingPurchaseOrder(connection, Guid.Parse(ddlPurchaseOrders.SelectedValue))
			lblNumber.Text = tbxNumber.Text
			lblOwner.Text = ddlOwner.SelectedItem.Text
			lblSupplierAccount.Text = ddlSupplier.SelectedItem.Text
			lblBulkProduct.Text = ddlBulkProduct.SelectedItem.Text
			lblPurchased.Text = tbxPurchased.Text & " " & ddlUnit.SelectedItem.Text
			lblTotalDelivered.Text = lblDelivered.Text
			tbxDelivered.Text = IIf(po.Purchased - po.Delivered > 0, (po.Purchased - po.Delivered).ToString, "0")
			ddlDeliveredUnit.SelectedValue = ddlUnit.SelectedValue
			ddlTransportUnit.SelectedValue = ddlUnit.SelectedValue
			ddlCarrier.SelectedIndex = 0
			tbxCarrierName.Text = ""
			tbxCarrierNumber.Text = ""
			tbxTransportName.Text = ""
			tbxTransportNumber.Text = ""
			tbxTransportTareWeight.Text = 0
			tbxTransportTareDate.Text = ""
			ddlTransport.SelectedIndex = 0
			ddlTransport_SelectedIndexChanged(ddlTransport, New EventArgs())
			ddlDriver.SelectedIndex = 0
			tbxDriverName.Text = ""
			tbxDriverNumber.Text = ""
			tbxReceivingNotes.Text = ""
			pnlReceive.Visible = True
			pnlMain.Visible = False
			pnlTickets.Visible = False
			PopulateReceivingLots(po)
			pnlReceivingLot.Visible = Tm2Database.SystemItemTraceabilityEnabled
			ddlReceivingLot_SelectedIndexChanged(ddlReceivingLot, New EventArgs())

			cblStorageLocations.Items.Clear()
			If Tm2Database.SystemItemTraceabilityEnabled Then
				Dim storageLocationRdr As OleDbDataReader = Nothing
				Try
					storageLocationRdr = Tm2Database.ExecuteReader(Tm2Database.Connection, $"SELECT {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_ID}, {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_NAME} + CASE WHEN {KaTank.TABLE_NAME}.name IS NULL OR {KaTank.TABLE_NAME}.name = {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_NAME} THEN CASE WHEN {KaContainer.TABLE_NAME}.number IS NULL OR {KaContainer.TABLE_NAME}.number = {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_NAME} THEN '' ELSE ' (Container: ' + {KaContainer.TABLE_NAME}.number + ')' END ELSE ' (Tank: ' + {KaTank.TABLE_NAME}.name + ')' END AS {KaStorageLocation.FN_NAME} " &
							$"FROM {KaStorageLocation.TABLE_NAME} " &
							$"LEFT OUTER JOIN {KaTank.TABLE_NAME} ON {KaTank.TABLE_NAME}.id = {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_TANK_ID} AND {KaTank.TABLE_NAME}.deleted = 0 " &
							$"LEFT OUTER JOIN {KaContainer.TABLE_NAME} ON {KaContainer.TABLE_NAME}.id= {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_CONTAINER_ID} AND {KaContainer.TABLE_NAME}.deleted = 0 " &
							$"WHERE {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_DELETED} = 0 " &
							$"ORDER BY 2")
					While storageLocationRdr.Read()
						cblStorageLocations.Items.Add(New ListItem(storageLocationRdr.Item(KaStorageLocation.FN_NAME), storageLocationRdr.Item(KaStorageLocation.FN_ID).ToString()))
					End While
				Finally
					If storageLocationRdr IsNot Nothing Then storageLocationRdr.Close()
				End Try
			End If
			pnlReceiveIntoStorageLocation.Visible = cblStorageLocations.Items.Count > 0
		End If
	End Sub
#End Region

	Private Function IncrementAlphaNumeric(ByVal text As String) As String
		Dim i As Integer = 1
		Do While i < text.Length AndAlso
				 text.Substring(text.Length - i, 1) <> " " AndAlso
				 text.Substring(text.Length - i, 1) <> "-" AndAlso
				 IsNumeric(text.Substring(text.Length - i, i))
			i += 1
		Loop
		If text.Length > 0 AndAlso Not IsNumeric(text) Then i -= 1
		If i > 0 Then
			text = text.Substring(0, text.Length - i) & Format(Integer.Parse(text.Substring(text.Length - i, i)) + 1, StrDup(i, "0"))
		Else
			text = text & "-001"
		End If
		Return text
	End Function

	Private Sub PopulateLocationList()
		ddlLocation.Items.Clear()
		For Each r As KaLocation In KaLocation.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
			ddlLocation.Items.Add(New ListItem(r.Name, r.Id.ToString()))
		Next
	End Sub

	Private Sub PopulateCarrierList()
		ddlCarrier.Items.Clear()
		ddlCarrier.Items.Add(New ListItem("", Guid.Empty.ToString()))
		For Each r As KaCarrier In KaCarrier.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
			ddlCarrier.Items.Add(New ListItem(r.Name, r.Id.ToString()))
		Next
	End Sub

	Private Sub PopulateTransportList()
		ddlTransport.Items.Clear()
		ddlTransport.Items.Add(New ListItem("", Guid.Empty.ToString()))
		For Each r As KaTransport In KaTransport.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
			ddlTransport.Items.Add(New ListItem(r.Name, r.Id.ToString()))
		Next
	End Sub

	Private Sub PopulateDriverList()
		ddlDriver.Items.Clear()
		ddlDriver.Items.Add(New ListItem("", Guid.Empty.ToString()))
		For Each r As KaDriver In KaDriver.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
			ddlDriver.Items.Add(New ListItem(r.Name, r.Id.ToString()))
		Next
	End Sub

	Private Sub PopulateReceivingPurchaseOrders()
		Dim currentSelectedId As String = Guid.Empty.ToString()
		If ddlPurchaseOrders.SelectedIndex >= 0 Then currentSelectedId = ddlPurchaseOrders.SelectedValue
		ddlPurchaseOrders.Items.Clear()
		If _currentUserPermission(_currentTableName).Create Then ddlPurchaseOrders.Items.Add(New ListItem("Enter new receiving purchase order", Guid.Empty.ToString())) Else ddlPurchaseOrders.Items.Add(New ListItem("Select a purchase order", Guid.Empty.ToString()))
		Dim whereClause As String = "deleted = 0 AND completed = 0"
		If Not _currentUser.OwnerId.Equals(Guid.Empty) Then whereClause &= " AND owner_id = " & Q(_currentUser.OwnerId)
		Dim locationId As Guid = Guid.Empty
		If Guid.TryParse(ddlFacilityFilter.SelectedValue, locationId) AndAlso Not locationId.Equals(Guid.Empty) Then
			whereClause &= $" AND (bulk_product_id IN (SELECT bulk_product_id FROM product_bulk_products WHERE (deleted = 0) AND (location_id = {Q(locationId)}) UNION SELECT bulk_product_id FROM bulk_product_panel_settings bpps INNER JOIN panels ON panels.id = bpps.panel_id WHERE (bpps.deleted = 0) AND (panels.deleted = 0) AND (location_id = {Q(locationId)})))"
		End If
		For Each r As KaReceivingPurchaseOrder In KaReceivingPurchaseOrder.GetAll(GetUserConnection(_currentUser.Id), whereClause, "number ASC")
			ddlPurchaseOrders.Items.Add(New ListItem(r.Number, r.Id.ToString()))
		Next
		Try
			ddlPurchaseOrders.SelectedValue = currentSelectedId
		Catch ex As ArgumentOutOfRangeException

		End Try
		ddlPurchaseOrders_SelectedIndexChanged(ddlPurchaseOrders, New EventArgs())
	End Sub

	Private Sub PopulateFacilityList()
		ddlFacilityFilter.Items.Clear()
		ddlFacilityFilter.Items.Add(New ListItem("All facilities", Guid.Empty.ToString()))
		For Each u As KaLocation In KaLocation.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
			ddlFacilityFilter.Items.Add(New ListItem(u.Name, u.Id.ToString))
		Next
	End Sub

	Private Sub PopulateOwnerList()
		ddlOwner.Items.Clear()
		ddlOwner.Items.Add(New ListItem("", Guid.Empty.ToString()))
		For Each r As KaOwner In KaOwner.GetAll(GetUserConnection(_currentUser.Id), "deleted=0" & IIf(_currentUser.OwnerId = Guid.Empty, "", " AND id=" & Q(_currentUser.OwnerId)), "name ASC")
			ddlOwner.Items.Add(New ListItem(r.Name, r.Id.ToString()))
		Next
	End Sub

	Private Sub PopulateSupplierAccountList()
		ddlSupplier.Items.Clear()
		ddlSupplier.Items.Add(New ListItem("", Guid.Empty.ToString()))
		For Each r As KaSupplierAccount In KaSupplierAccount.GetAll(GetUserConnection(_currentUser.Id), "deleted=0" & IIf(_currentUser.OwnerId = Guid.Empty, "", " AND owner_id=" & Q(_currentUser.OwnerId)), "name ASC")
			ddlSupplier.Items.Add(New ListItem(r.Name, r.Id.ToString()))
		Next
	End Sub

	Private Sub PopulateBulkProductList()
		ddlBulkProduct.Items.Clear()
		ddlBulkProduct.Items.Add(New ListItem("", Guid.Empty.ToString()))
		Dim whereClause As String = "deleted = 0"
		Dim ownerId As Guid = Guid.Empty
		If Guid.TryParse(ddlOwner.SelectedValue, ownerId) AndAlso Not ownerId.Equals(Guid.Empty) Then
			whereClause &= $" AND (owner_id = {Q(ownerId)} OR owner_id = {Q(Guid.Empty)})"
		End If
		Dim locationId As Guid = Guid.Empty
		If Guid.TryParse(ddlFacilityFilter.SelectedValue, locationId) AndAlso Not locationId.Equals(Guid.Empty) Then
			whereClause &= $" AND (id IN (SELECT bulk_product_id FROM product_bulk_products WHERE (deleted = 0) AND (location_id = {Q(locationId)}) UNION SELECT bulk_product_id FROM bulk_product_panel_settings bpps INNER JOIN panels ON panels.id = bpps.panel_id WHERE (bpps.deleted = 0) AND (panels.deleted = 0) AND (location_id = {Q(locationId)})))"
		End If

		For Each r As KaBulkProduct In KaBulkProduct.GetAll(GetUserConnection(_currentUser.Id), whereClause, "name ASC")
			If Not r.IsFunction(GetUserConnection(_currentUser.Id)) Then
				ddlBulkProduct.Items.Add(New ListItem(r.Name, r.Id.ToString()))
			End If
		Next
	End Sub

	Private Sub PopulateUnitList()
		ddlUnit.Items.Clear()
		ddlDeliveredUnit.Items.Clear()
		ddlUnit.Items.Add(New ListItem("", Guid.Empty.ToString()))
		ddlDeliveredUnit.Items.Add(New ListItem("", Guid.Empty.ToString()))
		ddlTransportUnit.Items.Clear()
		ddlTransportUnit.Items.Add(New ListItem("", Guid.Empty.ToString()))
		For Each r As KaUnit In KaUnit.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND base_unit<>9", "abbreviation ASC")
			ddlUnit.Items.Add(New ListItem(r.Abbreviation, r.Id.ToString()))
			ddlDeliveredUnit.Items.Add(New ListItem(r.Abbreviation, r.Id.ToString()))
			ddlTransportUnit.Items.Add(New ListItem(r.Abbreviation, r.Id.ToString()))
		Next
	End Sub

	Private Sub PopulateReceivingLots(po As KaReceivingPurchaseOrder)
		ddlReceivingLot.Items.Clear()
		ddlReceivingLot.Items.Add(New ListItem("", Guid.Empty.ToString()))

		Dim lotCreationUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaLot.TABLE_NAME}), "Products")

		With lotCreationUserPermission(KaLot.TABLE_NAME)
			If .Create Then
				ddlReceivingLot.Items.Add(New ListItem("Create new lot", "New"))
			End If
		End With

		For Each lot As KaLot In KaLot.GetAll(GetUserConnection(_currentUser.Id), $"{KaLot.FN_DELETED} = 0 AND {KaLot.FN_BULK_PRODUCT_ID} = {Q(po.BulkProductId)}", $"{KaLot.FN_NUMBER} ASC")
			ddlReceivingLot.Items.Add(New ListItem(lot.Number, lot.Id.ToString()))
		Next
	End Sub

	Private Sub PopulateReceivingPurchaseOrder()
		Dim modifiable As Boolean = (_currentUserPermission(_currentTableName).Edit)
		_customFieldData.Clear()
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim receivingPo As New KaReceivingPurchaseOrder()
		With receivingPo
			.Id = Guid.Parse(ddlPurchaseOrders.SelectedValue)
			If .Id = Guid.Empty Then
				btnDelete.Enabled = False
				btnSave.Enabled = _currentUserPermission(_currentTableName).Create
				btnCopy.Enabled = False
				btnReceive.Enabled = False
				btnPrintPo.Enabled = False
				btnMarkComplete.Enabled = False
				tbxNotes.Text = ""
				If AutoGenerateOrderNumber() Then
					tbxNumber.Text = "Automatically generated"
					tbxNumber.Enabled = UserCanChangeOrderNumber()
				Else
					tbxNumber.Text = ""
					tbxNumber.Enabled = _currentUserPermission(_currentTableName).Edit
				End If
				pnlTickets.Visible = False
			Else
				.SqlSelect(connection)
				btnDelete.Enabled = _currentUserPermission(_currentTableName).Delete
				btnSave.Enabled = modifiable
				btnCopy.Enabled = modifiable
				btnPrintPo.Enabled = True
				btnMarkComplete.Enabled = modifiable
				tbxNotes.Text = .Notes
				btnReceive.Enabled = modifiable
				tbxNumber.Text = .Number
				pnlTickets.Visible = True
				For Each customFieldValue As KaCustomFieldData In KaCustomFieldData.GetAll(connection, String.Format("deleted = 0 AND {0} = {1}", KaCustomFieldData.FN_RECORD_ID, Q(.Id)), KaCustomFieldData.FN_LAST_UPDATED)
					_customFieldData.Add(customFieldValue)
				Next
			End If
			Try
				ddlOwner.SelectedValue = .OwnerId.ToString()
			Catch ex As ArgumentOutOfRangeException
				ddlOwner.SelectedIndex = 0
				DisplayJavaScriptMessage("InvalidOwnerId", Utilities.JsAlert("Record not found in owners where ID = " + .OwnerId.ToString() + " Owner not set."))
			End Try
			Try
				ddlSupplier.SelectedValue = .SupplierAccountId.ToString()
			Catch ex As ArgumentOutOfRangeException
				ddlSupplier.SelectedIndex = 0
				DisplayJavaScriptMessage("InvalidSupplierId", Utilities.JsAlert("Record not found in supplier accounts where ID = " + .SupplierAccountId.ToString() + " Supplier account not set."))
			End Try
			Try
				ddlBulkProduct.SelectedValue = .BulkProductId.ToString()
			Catch ex As ArgumentOutOfRangeException
				ddlBulkProduct.SelectedIndex = 0
				DisplayJavaScriptMessage("InvalidBulkProductId", Utilities.JsAlert("Record not found in bulk products where ID = " + .BulkProductId.ToString() + ". Bulk product not set."))
			End Try
			tbxPurchased.Text = .Purchased
			Try
				ddlUnit.SelectedValue = .UnitId.ToString()
			Catch ex As ArgumentOutOfRangeException
				ddlUnit.SelectedIndex = 0
				DisplayJavaScriptMessage("InvalidUnitid", Utilities.JsAlert("Record not found in units where ID = " + .UnitId.ToString() + ". Unit not set."))
			End Try
			Dim unit As KaUnit = New KaUnit()
			If .UnitId <> Guid.Empty Then
				unit = New KaUnit(connection, .UnitId)
			End If
			lblDelivered.Text = Format(.Delivered, unit.UnitPrecision) & " " & unit.Name
		End With
		GetReceivingPoUsages(receivingPo)
		ClearTickets(True)
		PopulateReceivingTickets()
		Utilities.CreateDynamicCustomFieldPanelControls(_customFields, _customFieldData, lstCustomFields, Page)
	End Sub


	Private Sub GetReceivingPoUsages(receivingPo As KaReceivingPurchaseOrder)
		Dim receivingPoUsageList As ArrayList = GetReceivingPoUsagesTable(receivingPo)
		If receivingPoUsageList.Count > 1 Then
			litReceivingPoTicketUsage.Text = KaReports.GetTableHtml("", "", receivingPoUsageList, False, "class=""label, input""", "", New List(Of String), "", New List(Of String))
		Else
			litReceivingPoTicketUsage.Text = ""
		End If
		btnShowPoUsages.Visible = receivingPoUsageList.Count > 1
	End Sub

	Private Function GetReceivingPoUsagesTable(receivingPo As KaReceivingPurchaseOrder) As ArrayList
		Dim receivingTicketUsageList As ArrayList = New ArrayList
		If Tm2Database.SystemItemTraceabilityEnabled AndAlso receivingPo IsNot Nothing Then
			Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
			Dim tickets As Dictionary(Of Guid, KaTicket) = receivingPo.GetTicketUsagesForMovements(connection, Nothing, chkShowVoidedTickets.Checked)
			For Each ticketId As Guid In tickets.Keys
				Dim ticket As KaTicket = tickets(ticketId)
				Dim webTicketAddress As String = $"<a href=""Receipts.aspx?TicketId={ticket.Id.ToString()}"" target=""_blank"">{System.Web.HttpUtility.HtmlEncode(Receipts.GetTicketNumber(GetUserConnection(_currentUser.Id), Nothing, ticket))}</a>"
				Dim orderAddress As String
				Try
					If New KaOrder(connection, ticket.OrderId).Completed Then
						orderAddress = "<a href=""PastOrders.aspx?OrderId="
					Else
						orderAddress = "<a href=""Orders.aspx?OrderId="
					End If
					orderAddress &= ticket.OrderId.ToString & """ target=""_blank"">" & System.Web.HttpUtility.HtmlEncode(ticket.OrderNumber) & "</a>"
				Catch ex As RecordNotFoundException
					orderAddress = ticket.OrderNumber
				End Try
				receivingTicketUsageList.Add(New ArrayList({String.Format(ticket.LoadedAt, "G"), webTicketAddress, orderAddress}))
			Next
			If receivingTicketUsageList.Count > 0 Then receivingTicketUsageList.Insert(0, New ArrayList({"Loaded at", "Ticket", "Order number"}))
		End If

		Return receivingTicketUsageList
	End Function

	Private Sub ddlOwner_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlOwner.SelectedIndexChanged
		PopulateBulkProductList()
	End Sub

	Private Sub ddlBulkProduct_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlBulkProduct.SelectedIndexChanged
		If Guid.Parse(ddlBulkProduct.SelectedValue) <> Guid.Empty Then
			Dim bulkProd As KaBulkProduct = New KaBulkProduct(GetUserConnection(_currentUser.Id), Guid.Parse(ddlBulkProduct.SelectedValue))
			ddlUnit.SelectedValue = bulkProd.DefaultUnitId.ToString
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

#Region "Receiving Tickets"
	Private Sub PopulateReceivingTickets()
		Dim nonVoidedTickets As Integer = 0
		ddlReceivingTickets.Items.Clear()
		Dim li As ListItem = New ListItem
		li.Text = ""
		li.Value = Guid.Empty.ToString
		ddlReceivingTickets.Items.Add(li)
		If Guid.Parse(ddlPurchaseOrders.SelectedValue) <> Guid.Empty Then
			Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
			Dim ticketsDA As New OleDbDataAdapter("SELECT  id, number, delivered, unit_id, density, weight_unit_id, volume_unit_id, date_of_delivery, linked_tickets_id, panel_id, voided " &
					  "FROM receiving_tickets " &
					  "WHERE (deleted = 0) " &
						  "AND (archived = 0) " &
						  "AND receiving_purchase_order_id = " & Q(Guid.Parse(ddlPurchaseOrders.SelectedValue)) & " " &
					  "ORDER BY number ASC, date_of_delivery ASC", connection)
			Dim ticketsTable As New DataTable
			If Tm2Database.CommandTimeout > 0 Then ticketsDA.SelectCommand.CommandTimeout = Tm2Database.CommandTimeout
			ticketsDA.Fill(ticketsTable)
			'  If Boolean.Parse(KaSetting.GetSetting(GetUserConnection(currentUser.Id), "Use_Receiving_PO_Web_Ticket", "False")) AndAlso _
			'If KaSetting.GetSetting(GetUserConnection(_currentUser.Id), "Receiving_PO_Web_Ticket_Address", "http://localhost/TerminalManagement2/ReceivingTicket.aspx").Trim.Length > 0 Then
			Dim dateOfDelivery As DateTime
			Dim ticketNumber As String
			Dim deliveredQty As Double
			Dim unitId As Guid
			Dim linkedId As Guid
			Dim rowCounter As Integer = 0
			Do While rowCounter < ticketsTable.Rows.Count
				With ticketsTable.Rows(rowCounter)
					If chkShowVoidedTickets.Checked OrElse Not .Item("voided") Then
						dateOfDelivery = .Item("date_of_delivery")
						ticketNumber = .Item("number")
						deliveredQty = .Item("delivered")
						unitId = .Item("unit_id")
						linkedId = .Item("linked_tickets_id")
						If Not .Item("linked_tickets_id").Equals(Guid.Empty) Then
							' Grouped ticket
							Dim secondRowCounter As Integer = rowCounter + 1
							Do While secondRowCounter < ticketsTable.Rows.Count
								With ticketsTable.Rows(secondRowCounter)
									If .Item("linked_tickets_id").Equals(linkedId) Then
										' Grouped ticket
										deliveredQty += KaUnit.Convert(connection, New KaQuantity(.Item("delivered"), .Item("unit_id")), New KaRatio(.Item("density"), .Item("weight_unit_id"), .Item("volume_unit_id")), unitId).Numeric
										ticketsTable.Rows.RemoveAt(secondRowCounter)
									Else
										secondRowCounter += 1
									End If
								End With
							Loop
						End If
						li = New ListItem
						li.Text = ticketNumber & IIf(.Item("voided") AndAlso Not ticketNumber.Contains("(voided)"), " (voided)", " {" & dateOfDelivery & " - " & Format(deliveredQty, KaPanel.GetUnitPrecision(connection, .Item("panel_id"), unitId)) & " " & New KaUnit(connection, unitId).Abbreviation & "}")
						li.Value = .Item("id").ToString
						ddlReceivingTickets.Items.Add(li)
					End If
					If Not .Item("voided") Then nonVoidedTickets += 1
				End With
				rowCounter += 1
			Loop

			pnlTickets.Visible = (ticketsTable.Rows.Count > 0)
			btnDelete.Enabled = (nonVoidedTickets = 0) AndAlso (_currentUserPermission(_currentTableName).Delete)
		Else
			pnlTickets.Visible = False
			btnDelete.Enabled = False
		End If

	End Sub

	Private Sub ddlReceivingTickets_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlReceivingTickets.SelectedIndexChanged
		ClearTickets(False)
		Dim ticketId As Guid = Guid.Empty
		If Guid.TryParse(ddlReceivingTickets.SelectedValue, ticketId) AndAlso Not ticketId.Equals(Guid.Empty) Then
			Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
			Dim receivingTicket As KaReceivingTicket = New KaReceivingTicket(connection, ticketId)
			Dim htmlAddress As String = GetReceivingTicketHtmlAddress(receivingTicket, _currentUser)

			If (htmlAddress.Trim().Length > 0) Then
				pnlTextTicket.Visible = False
				pnlHtmlticket.Visible = True
				If Utilities.ConvertWebPageUrlDomainToRequestedPagesDomain(connection) Then htmlAddress = Tm2Database.GetUrlInCurrentDomain(Tm2Database.GetCurrentPageUrlWithOriginalPort(Me.Request), htmlAddress)
				divHtmlTicket.InnerHtml = "<iframe name=""iframe"" src=""" & htmlAddress & """ class=""iframe"" width=""100%"" height=""400px"" scrolling=""auto"" style=""border: 1 solid #000000;"" onclick=""$('#pnlReceivingTicketUsage').fadeOut();""></iframe>"
			Else
				pnlTextTicket.Visible = True
				pnlHtmlticket.Visible = False
				litTextTicketOutput.Text = receivingTicket.ReceiptTicketText(connection).Replace(Environment.NewLine, "<br>")
			End If
			btnPrintTicket.Enabled = True
			btnVoidTicket.Enabled = Not receivingTicket.Voided AndAlso _currentUserPermission(_currentTableName).Delete
			GetReceivingTicketUsages(receivingTicket)
		End If
	End Sub

	Public Shared Function GetReceivingTicketHtmlAddress(ByVal receivingTicket As KaReceivingTicket, ByVal currentUser As KaUser) As String
		Dim htmlAddress As String = Reports.ReceivingPoWebTicketUrlForOwner(Tm2Database.Connection, receivingTicket.OwnerId)
		Dim dbFound As Boolean = False
		Dim ticketIdFound As Boolean = False
		Dim truckIdFound As Boolean = False
		Dim parameters As String = ""
		If htmlAddress.Trim().Length > 0 Then
			If htmlAddress.IndexOf("?") > 0 Then
				Dim htmlParameters() As String = htmlAddress.Substring(htmlAddress.IndexOf("?") + 1).Split("&")
				For Each htmlParam As String In htmlParameters
					If htmlParam.Length > 0 Then
						If parameters.Length > 0 Then parameters &= "&"
						Dim param() As String = htmlParam.Split("=")
						Select Case param(0).Trim.ToLower
							Case "db"
								dbFound = True
								If param.Length > 1 Then
									If param(1).Trim.Length > 0 Then
										parameters &= param(0) & "=" & param(1)
									Else
										parameters &= param(0) & "=TM2"
									End If
								Else
									parameters &= param(0) & "=TM2"
								End If
							Case "ticket_id"
								ticketIdFound = True
								parameters &= param(0) & "=" + receivingTicket.Id.ToString
							Case "truck_id"
								truckIdFound = True
								parameters &= param(0) & "=" + receivingTicket.TransportId.ToString
							Case Else
								parameters &= htmlParam
						End Select
					End If
				Next
				htmlAddress = htmlAddress.Substring(0, htmlAddress.IndexOf("?"))
			End If
			If (htmlAddress.Trim().Length > 0) Then
				If Not dbFound Then
					If parameters.Length > 0 Then parameters &= "&"
					parameters &= "db=TM2"
				End If
				If Not ticketIdFound Then
					parameters &= "&ticket_id=" + receivingTicket.Id.ToString
				End If
				If Not truckIdFound Then
					parameters &= "&truck_id=" + receivingTicket.TransportId.ToString
				End If
				htmlAddress &= "?" & parameters & "&instanceGuid=" & Guid.NewGuid.ToString
			End If
		End If
		'   End If
		Return htmlAddress
	End Function

	Private Sub ClearTickets(ByVal resetDropDown As Boolean)
		If resetDropDown Then ddlReceivingTickets.SelectedIndex = 0
		btnPrintTicket.Enabled = False
		btnVoidTicket.Enabled = False
		btnShowTicketUsages.Visible = False
		litTextTicketOutput.Text = ""
		divHtmlTicket.InnerHtml = ""
		pnlTextTicket.Visible = False
		pnlHtmlticket.Visible = False
		litReceivingTicketUsage.Text = ""
	End Sub

	Private Sub GetReceivingTicketUsages(receivingTicket As KaReceivingTicket)
		Dim receivingTicketUsageList As ArrayList = GetReceivingTicketUsagesTable(receivingTicket)
		If receivingTicketUsageList.Count > 1 Then
			litReceivingTicketUsage.Text = KaReports.GetTableHtml("", "", receivingTicketUsageList, False, "class=""label, input""", "", New List(Of String), "", New List(Of String))
		Else
			litReceivingTicketUsage.Text = ""
		End If
		btnShowTicketUsages.Visible = receivingTicketUsageList.Count > 1
	End Sub

	Private Function GetReceivingTicketUsagesTable(receivingTicket As KaReceivingTicket) As ArrayList
		Dim receivingTicketUsageList As ArrayList = New ArrayList
		If Tm2Database.SystemItemTraceabilityEnabled Then
			Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
			Dim tickets As Dictionary(Of Guid, KaTicket) = receivingTicket.GetTicketUsagesForMovement(connection, Nothing, chkShowVoidedTickets.Checked)
			For Each ticketId As Guid In tickets.Keys
				Dim ticket As KaTicket = tickets(ticketId)
				Dim ownerId As Guid = GetOwnerIdForTicket(connection, ticketId)
				Dim webTicketAddress As String = $"<a href=""Receipts.aspx?TicketId={ticket.Id.ToString()}"" target=""_blank"">{System.Web.HttpUtility.HtmlEncode(Receipts.GetTicketNumber(GetUserConnection(_currentUser.Id), Nothing, ticket))}</a>"
				Dim orderAddress As String
				Try
					If New KaOrder(connection, ticket.OrderId).Completed Then
						orderAddress = "<a href=""PastOrders.aspx?OrderId="
					Else
						orderAddress = "<a href=""Orders.aspx?OrderId="
					End If
					orderAddress &= ticket.OrderId.ToString & """ target=""_blank"">" & System.Web.HttpUtility.HtmlEncode(ticket.OrderNumber) & "</a>"
				Catch ex As RecordNotFoundException
					orderAddress = ticket.OrderNumber
				End Try
				receivingTicketUsageList.Add(New ArrayList({String.Format(ticket.LoadedAt, "G"), webTicketAddress, orderAddress}))
			Next
			If receivingTicketUsageList.Count > 0 Then receivingTicketUsageList.Insert(0, New ArrayList({"Loaded at", "Ticket", "Order number"}))
		End If

		Return receivingTicketUsageList
	End Function

	Private Sub btnPrintTicket_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnPrintTicket.Click
		Dim receivingTicket As KaReceivingTicket = New KaReceivingTicket(GetUserConnection(_currentUser.Id), Guid.Parse(ddlReceivingTickets.SelectedValue))
		Dim htmlAddress As String = GetReceivingTicketHtmlAddress(receivingTicket, _currentUser)
		If htmlAddress.Trim.Length > 0 Then
			DisplayJavaScriptMessage("PrintTicket", Utilities.JsWindowOpen(htmlAddress)) ', "toolbar=yes, menubar = yes, ScrollBars = yes, resizable = yes, width = 700, height = 500, top = 50, Left() = 50", True))
		Else
			'Open new window to view Receiving PO to be printed.
			DisplayJavaScriptMessage("PrintTicket", Utilities.JsWindowOpen("ReceivingTicketPFV.aspx?receiving_ticket_id=" & receivingTicket.Id.ToString)) ', "toolbar=yes, menubar = yes, ScrollBars = yes, resizable = yes, width = 700, height = 500, top = 50, Left() = 50", True))
		End If
	End Sub

	Private Sub btnbtnVoidTicket_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnVoidTicket.Click
		Dim receivingTicket As KaReceivingTicket = New KaReceivingTicket(GetUserConnection(_currentUser.Id), Guid.Parse(ddlReceivingTickets.SelectedValue))
		If Not receivingTicket.Voided Then
			receivingTicket.VoidTicket(GetUserConnection(_currentUser.Id), Nothing, _currentUser.Name & " manual void ticket", Database.ApplicationIdentifier, _currentUser.Name)
		End If
		ddlPurchaseOrders_SelectedIndexChanged(ddlPurchaseOrders, New EventArgs())
	End Sub

	Protected Sub chkShowVoidedTickets_CheckedChanged(sender As Object, e As EventArgs) Handles chkShowVoidedTickets.CheckedChanged
		ClearTickets(True)
		PopulateReceivingTickets()
	End Sub
#End Region

	Private Sub btnMarkComplete_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnMarkComplete.Click
		If Guid.Parse(ddlPurchaseOrders.SelectedValue) <> Guid.Empty Then
			Dim po As KaReceivingPurchaseOrder = New KaReceivingPurchaseOrder(GetUserConnection(_currentUser.Id), Guid.Parse(ddlPurchaseOrders.SelectedValue))
			With po
				.Completed = True
				.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
			End With
			PopulateReceivingPurchaseOrders()
			ddlPurchaseOrders_SelectedIndexChanged(ddlPurchaseOrders, New EventArgs())
		End If
	End Sub

	Private Sub btnPrintPo_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnPrintPo.Click
		If Guid.Parse(ddlPurchaseOrders.SelectedValue) <> Guid.Empty Then
			DisplayJavaScriptMessage("PrintPo", Utilities.JsWindowOpen("ReceivingPFV.aspx?po_id=" & ddlPurchaseOrders.SelectedValue)) ', "toolbar=yes, menubar = yes, ScrollBars = yes, resizable = yes, width = 700, height = 500, top = 50, Left() = 50", True))
		End If
	End Sub

	Private Function AutoGenerateOrderNumber() As Boolean
		Return Boolean.Parse(KaSetting.GetSetting(GetUserConnection(_currentUser.Id), KaSetting.SN_AUTO_GENERATE_RECEIVING_ORDER_NUMBER, KaSetting.SD_AUTO_GENERATE_RECEIVING_ORDER_NUMBER))
	End Function

	Private Function UserCanChangeOrderNumber() As Boolean
		Return Boolean.Parse(KaSetting.GetSetting(GetUserConnection(_currentUser.Id), KaSetting.SN_USER_CAN_CHANGE_RECEIVING_ORDER_NUMBER, KaSetting.SD_USER_CAN_CHANGE_RECEIVING_ORDER_NUMBER)) AndAlso _currentUserPermission(_currentTableName).Create
	End Function

	Private Function SeparateOrderNumberPerOwner() As Boolean
		Return Boolean.Parse(KaSetting.GetSetting(GetUserConnection(_currentUser.Id), KaSetting.SN_SEPARATE_RECEIVING_ORDER_NUMBER_PER_OWNER, KaSetting.SD_SEPARATE_RECEIVING_ORDER_NUMBER_PER_OWNER))
	End Function

	Private Function NextOrderNumberForOwner(ownerId As Guid) As String
		Return KaSetting.GetSetting(GetUserConnection(_currentUser.Id), KaSetting.SN_NEXT_RECEIVING_ORDER_NUMBER_FOR_OWNER & ownerId.ToString(), KaSetting.SD_NEXT_RECEIVING_ORDER_NUMBER_FOR_OWNER)
	End Function

	Private Sub NextOrderNumberForOwner(ownerId As Guid, number As String)
		KaSetting.WriteSetting(GetUserConnection(_currentUser.Id), KaSetting.SN_NEXT_RECEIVING_ORDER_NUMBER_FOR_OWNER & ownerId.ToString(), number)
	End Sub

	Private Property NextOrderNumber As String
		Get
			Return KaSetting.GetSetting(GetUserConnection(_currentUser.Id), KaSetting.SN_NEXT_RECEIVING_ORDER_NUMBER, KaSetting.SD_NEXT_RECEIVING_ORDER_NUMBER)
		End Get
		Set(value As String)
			KaSetting.WriteSetting(GetUserConnection(_currentUser.Id), KaSetting.SN_NEXT_RECEIVING_ORDER_NUMBER, value)
		End Set
	End Property

	Private Function GetNextOrderNumber(ownerId As Guid, orderNumbersByOwner As Boolean) As String
		Dim orderNumber As String
		Dim c As OleDbConnection = GetUserConnection(_currentUser.Id)
		If AutoGenerateOrderNumber() Then
			If orderNumbersByOwner Then
				orderNumber = NextOrderNumberForOwner(ownerId)
				NextOrderNumberForOwner(ownerId, KaTicket.IncrementAlphaNumeric(orderNumber))
			Else
				orderNumber = NextOrderNumber
				NextOrderNumber = KaTicket.IncrementAlphaNumeric(orderNumber)
			End If
		Else
			orderNumber = ""
		End If
		Return orderNumber
	End Function

	Private Function CheckOrderNumber(ByVal poId As Guid, ByVal orderNumber As String, ownerId As Guid, separateOrderNumberPerOwner As Boolean) As Boolean
		Dim validNumber As Boolean = False
		For Each rpo As KaReceivingPurchaseOrder In KaReceivingPurchaseOrder.GetOrdersThatMatchNumber(GetUserConnection(_currentUser.Id), Nothing, orderNumber, True)
			If Not poId.Equals(rpo.Id) AndAlso
					(Not separateOrderNumberPerOwner OrElse rpo.OwnerId.Equals(ownerId)) Then
				Return False
			End If
		Next
		Return True
	End Function

	Protected Sub btnFind_Click(sender As Object, e As EventArgs) Handles btnFind.Click
		If tbxFind.Text.Trim().Length > 0 Then
			Dim i As Integer = ddlPurchaseOrders.SelectedIndex
			Do
				If i + 1 = ddlPurchaseOrders.Items.Count Then i = 0 Else i += 1
				If ddlPurchaseOrders.Items(i).Text.Trim().ToLower().Contains(tbxFind.Text.Trim().ToLower()) Then
					ddlPurchaseOrders.SelectedIndex = i
					ddlPurchaseOrders_SelectedIndexChanged(ddlPurchaseOrders, New EventArgs())
					Exit Sub
				End If
			Loop While i <> ddlPurchaseOrders.SelectedIndex
			DisplayJavaScriptMessage("InvalidNumber", Utilities.JsAlert("Record not found in past receiving purchase orders where number = " & tbxFind.Text))
		End If
	End Sub

	Private Sub SetTextboxMaxLengths()
		tbxCarrierName.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaCarrier.TABLE_NAME, "name"))
		tbxCarrierNumber.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaCarrier.TABLE_NAME, "number"))
		'   tbxCopies.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(_currentTableName, ""))
		tbxDelivered.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(_currentTableName, "delivered"))
		tbxDriverName.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaDriver.TABLE_NAME, "name"))
		tbxDriverNumber.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaDriver.TABLE_NAME, "number"))
		tbxFind.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(_currentTableName, "number"))
		tbxNumber.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(_currentTableName, "number"))
		tbxPurchased.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(_currentTableName, "purchased"))
		tbxStartNumber.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(_currentTableName, "number"))
		tbxTransportName.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaTransport.TABLE_NAME, "name"))
		tbxTransportNumber.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaTransport.TABLE_NAME, "number"))
		tbxTransportTareWeight.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaTransport.TABLE_NAME, "tare_weight"))
		tbxNewReceivingLotNumber.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaLot.TABLE_NAME, KaLot.FN_NUMBER))
	End Sub

	Private Sub SetControlUsabilityFromPermissions()
		With _currentUserPermission(_currentTableName)
			Dim shouldEnable = (.Edit OrElse (.Create AndAlso ddlPurchaseOrders.SelectedIndex = 0))
			pnlGeneral.Enabled = shouldEnable

			btnVoidTicket.Enabled = .Edit AndAlso .Delete AndAlso ddlPurchaseOrders.SelectedIndex > 0 AndAlso ddlReceivingTickets.SelectedIndex > 0
			Dim nonVoidedTickets As Integer = 0
			If ddlPurchaseOrders.SelectedIndex > 0 Then
				Dim rdr As OleDbDataReader = Nothing
				Try
					rdr = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), $"SELECT COUNT(*) AS ticket_count FROM receiving_tickets WHERE        (receiving_purchase_order_id = {Q(ddlPurchaseOrders.SelectedValue)}) AND (voided = 0)")
					If rdr.Read() Then nonVoidedTickets = rdr.Item("ticket_count")
				Finally
					If rdr IsNot Nothing Then rdr.Close()
				End Try
			End If
			btnDelete.Enabled = .Edit AndAlso .Delete AndAlso ddlPurchaseOrders.SelectedIndex > 0 AndAlso nonVoidedTickets = 0
		End With
	End Sub

	Protected Sub ddlFacilityFilter_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlFacilityFilter.SelectedIndexChanged
		PopulateBulkProductList()
		PopulateReceivingPurchaseOrders()
		KaSetting.WriteSetting(GetUserConnection(_currentUser.Id), "PurchaseOrdersPage:" & _currentUser.Id.ToString & "/LastFacilityUsed", ddlFacilityFilter.SelectedValue)
	End Sub

	Protected Sub ddlTransport_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlTransport.SelectedIndexChanged
		Try
			With New KaTransport(GetUserConnection(_currentUser.Id), Guid.Parse(ddlTransport.SelectedValue))
				Dim uofMInfo As New KaUnit(GetUserConnection(_currentUser.Id), .UnitId)
				lblCurrentTransportTareInfo.Text = "<ul class=""input"" style=""width: auto; vertical-align:center;""><li>" & Format(.TareWeight, uofMInfo.UnitPrecision) & " " & uofMInfo.Abbreviation & "</li>" & IIf(.TaredAt > New DateTime(1900, 1, 1), "<li>" & .TaredAt & "</li>", "") & "</ul>"
				pnlCurrentTransportTareInfo.Visible = (.TareWeight > 0)
			End With
		Catch ex As Exception
			pnlCurrentTransportTareInfo.Visible = False
		End Try
	End Sub

	Protected Sub btnAssignTareFromTransport_Click(sender As Object, e As EventArgs) Handles btnAssignTareFromTransport.Click
		Try
			With New KaTransport(GetUserConnection(_currentUser.Id), Guid.Parse(ddlTransport.SelectedValue))
				ddlTransportUnit.SelectedValue = .UnitId.ToString()
				tbxTransportTareWeight.Text = .TareWeight.ToString()
				tbxTransportTareDate.Text = String.Format("{0:g}", .TaredAt)
			End With
		Catch ex As Exception

		End Try
	End Sub

	Protected Sub ToolkitScriptManager1_AsyncPostBackError(ByVal sender As Object, ByVal e As System.Web.UI.AsyncPostBackErrorEventArgs)
		ToolkitScriptManager1.AsyncPostBackErrorMessage = e.Exception.Message
	End Sub

	Private Sub DisplayJavaScriptMessage(key As String, script As String)
		ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), key, script, False)
	End Sub

	Protected Sub ddlReceivingLot_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlReceivingLot.SelectedIndexChanged
		If ddlReceivingLot.SelectedIndex = 1 AndAlso ddlReceivingLot.SelectedValue = "New" Then
			pnlNewReceivingLot.Visible = Tm2Database.SystemItemTraceabilityEnabled
			tbxNewReceivingLotNumber.Text = ""
		Else
			pnlNewReceivingLot.Visible = False
		End If
	End Sub
End Class