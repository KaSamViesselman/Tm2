Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb
Imports KahlerAutomation.KaTm2LoadFramework

Public Class StagedOrders
	Inherits System.Web.UI.Page
#Region " Web Form Designer Generated Code "

	'This call is required by the Web Form Designer.
	<System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()

	End Sub

	Private Sub Page_Init(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Init
		'CODEGEN: This method call is required by the Web Form Designer
		'Do not modify it using the code editor.
		InitializeComponent()
	End Sub

#End Region

	Private _currentUser As KaUser
	Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
	Private _currentTableName As String = KaStagedOrder.TABLE_NAME
	Private _currentOrderItemIds As New List(Of Guid)
	Private _stagedOrderInfo As New KaStagedOrder()
	Private _originalTareValues As New List(Of OriginalTareInfo)
	Private _assignProductError As Boolean

	Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
		Response.Cache.SetCacheability(HttpCacheability.NoCache)
		_currentUser = Utilities.GetUser(Me)
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		_currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "StagedOrders")
		If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")
		lblStatus.Text = ""
		If Not Page.IsPostBack Then
			SetTextboxMaxLengths()
			PopulateStagedOrdersList()
			Dim defaultWeightUnitId As Guid = KaUnit.GetSystemDefaultMassUnitOfMeasure(connection, Nothing)
			PopulateUnitOfMeasureList(ddlSpecifyTotalQuantity, defaultWeightUnitId, True)
			Boolean.TryParse(KaSetting.GetSetting(Tm2Database.Connection, "StagedOrder/UseOrderPercentage", False), chkUseOrderPercents.Checked)
			Try
				Dim stagedOrderId As Guid = Guid.Empty
				Guid.TryParse(Page.Request("StagedOrderId"), stagedOrderId)
				For n = 0 To ddlStagedOrders.Items.Count - 1
					With CType(ddlStagedOrders.Items(n), ListItem)
						If .Value = stagedOrderId.ToString() Then
							ddlStagedOrders.SelectedIndex = n
							Exit For
						End If
					End With
				Next
			Catch ex As Exception

			End Try
			SetControlUsabilityFromPermissions()
			ddlStagedOrders_SelectedIndexChanged(ddlStagedOrders, New EventArgs)
			RefreshOrderDetails(connection, GetOrderInfo())
			Utilities.ConfirmBox(Me.btnDelete, "Are you sure you want to delete this staged order?")

			LfDatabase.DefaultMassUnitId = KaUnit.GetSystemDefaultMassUnitOfMeasure(connection, Nothing)
			LfDatabase.DefaultVolumeUnitId = KaUnit.GetSystemDefaultVolumeUnitOfMeasure(connection, Nothing)
			Boolean.TryParse(KaSetting.GetSetting(connection, "StagedOrder/ShowUseRemainingOrderQuantityShortcut", True), pnlUseRemainingOrderQuantityShortcut.Visible)
			Boolean.TryParse(KaSetting.GetSetting(connection, "StagedOrder/ShowUseOriginalOrderQuantityShortcut", True), pnlUseOriginalOrderQuantityShortcut.Visible)
			Boolean.TryParse(KaSetting.GetSetting(connection, "StagedOrder/ShowUseApplicationRateShortcut", True), pnlUseApplicationRateShortcut.Visible)
			Boolean.TryParse(KaSetting.GetSetting(connection, "StagedOrder/ShowUseTransportCapacityShortcut", True), pnlUseTransportCapacityShortcut.Visible)
			Boolean.TryParse(KaSetting.GetSetting(connection, "StagedOrder/ShowUseBatchQuantityShortcut", True), pnlUseBatchQuantityShortcut.Visible)
			Boolean.TryParse(KaSetting.GetSetting(connection, "StagedOrder/ShowSpecifyTotalQuantityShortcut", True), pnlSpecifyTotalQuantityShortcut.Visible)
		End If
	End Sub

	Private Sub PopulateStagedOrdersList()
		PopulateStagedOrdersList(Guid.Empty)
	End Sub

	Private Sub PopulateStagedOrdersList(ByVal currentStagedOrderId As Guid)
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim currentId As String = currentStagedOrderId.ToString
		With ddlStagedOrders
			Dim currentIndex As Integer = 0
			If .SelectedIndex > 0 Then currentIndex = .SelectedIndex

			.Items.Clear()
			.Items.Add(New ListItem("New staged order", Guid.Empty.ToString))
			Dim stagedOrderTable As New DataTable
			Dim stagedOrderSql As String = "SELECT DISTINCT staged_orders.id, MAX(staged_order_transports.tared_at) AS last_tare, staged_orders.last_updated, '' AS number " &
				"FROM staged_orders " &
				"INNER JOIN staged_order_orders ON staged_orders.id = staged_order_orders.staged_order_id " &
				"INNER JOIN orders ON staged_order_orders.order_id = orders.id " &
				"INNER JOIN staged_order_transports ON staged_orders.id = staged_order_transports.staged_order_id " &
				"WHERE (staged_orders.deleted = 0) AND " &
					"(staged_order_orders.deleted = 0) AND " &
					"(orders.deleted = 0) AND " &
					IIf(_currentUser.OwnerId.Equals(Guid.Empty), "", "(orders.owner_id = " & Q(_currentUser.OwnerId) & ") AND ") &
					"(staged_order_transports.deleted = 0) " &
				"GROUP BY staged_orders.id, staged_orders.last_updated " &
				"UNION " &
				"SELECT DISTINCT staged_orders.id, '1900-01-01 00:00:00.000' AS last_tare, staged_orders.last_updated, '' AS number " &
				"FROM staged_orders " &
				"INNER JOIN staged_order_orders ON staged_orders.id = staged_order_orders.staged_order_id " &
				"INNER JOIN orders ON staged_order_orders.order_id = orders.id " &
				"WHERE (staged_orders.deleted = 0) AND " &
					"(staged_order_orders.deleted = 0) AND " &
					"(orders.deleted = 0) AND " &
					IIf(_currentUser.OwnerId.Equals(Guid.Empty), "", "(orders.owner_id = " & Q(_currentUser.OwnerId) & ") AND ") &
					"(NOT (staged_orders.id IN (SELECT staged_order_id FROM staged_order_transports WHERE (deleted = 0)))) " &
				"ORDER BY staged_orders.last_updated"
			Dim stagedOrderDA As New OleDbDataAdapter(stagedOrderSql, connection)
			If Tm2Database.CommandTimeout > 0 Then stagedOrderDA.SelectCommand.CommandTimeout = Tm2Database.CommandTimeout
			stagedOrderDA.Fill(stagedOrderTable)
			For Each stagedOrderRow As DataRow In stagedOrderTable.Rows
				Dim orderText As String = ""
				Dim stagedOrderOrdersRdr As OleDbDataReader = Tm2Database.ExecuteReader(connection, "SELECT orders.number " &
								"FROM staged_order_orders " &
								"INNER JOIN orders ON orders.id = staged_order_orders.order_id " &
								"WHERE (staged_order_orders.deleted = 0) AND " &
									"staged_order_orders.staged_order_id = " & Q(stagedOrderRow.Item("id")) &
								"ORDER BY orders.number")
				Do While stagedOrderOrdersRdr.Read
					If Not IsDBNull(stagedOrderOrdersRdr.Item("number")) Then
						If orderText.Length > 0 Then orderText &= ", "
						orderText &= stagedOrderOrdersRdr.Item("number")
					End If
				Loop
				stagedOrderOrdersRdr.Close()
				If orderText = "" Then
					If IsDBNull(stagedOrderRow.Item("number")) Then
						orderText = "Order Not Assigned "
					Else
						orderText = stagedOrderRow.Item("number").ToString.Trim
					End If
				End If
				If Not IsDBNull(stagedOrderRow.Item("last_tare")) AndAlso stagedOrderRow.Item("last_tare") > New DateTime(1900, 1, 1, 0, 0, 0) Then orderText &= " {" & stagedOrderRow.Item("last_tare").ToString.Trim & "}"
				Dim newListItem As New ListItem(orderText, stagedOrderRow.Item("id").ToString)
				'  newListItem.Attributes.Add("OrderId", stagedOrderRdr.Item("order_id"))
				.Items.Add(newListItem)
				If newListItem.Value = currentId AndAlso Not currentId.Equals(Guid.Empty.ToString) Then
					currentIndex = .Items.Count - 1
				End If
			Next
			.SelectedIndex = Math.Min(currentIndex, .Items.Count - 1)
		End With
		Page.ClientScript.RegisterStartupScript(Page.ClientScript.GetType(), "ResetScrollPosition;", "resetDotNetScrollPosition();", True)
	End Sub

	Private Sub PopulateTransportsList(ByRef transportDropdownList As DropDownList, ByVal stagedTransportInfoTransportId As Guid)
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim currentCarrierId As Guid = _stagedOrderInfo.CarrierId
		Dim currentTransportId As String = Guid.Empty.ToString

		Dim limitTransportsToCarrierSelected As Boolean = False
		Boolean.TryParse(KaSetting.GetSetting(connection, "PointOfSale/LimitTransportsToCarrierSelected", False), limitTransportsToCarrierSelected)

		Dim transportFound As Boolean = False
		transportDropdownList.Items.Clear()

		Dim transportIdsUsed As New List(Of Guid)
		For Each stagedTransport As KaStagedOrderTransport In _stagedOrderInfo.Transports
			If Not stagedTransport.Deleted AndAlso Not stagedTransport.TransportId.Equals(Guid.Empty) AndAlso Not transportIdsUsed.Contains(stagedTransport.TransportId) Then
				transportIdsUsed.Add(stagedTransport.TransportId)
			End If
		Next
		Dim transportDdl As DropDownList = FindControl(transportDropdownList.ClientID)
		transportDropdownList.Items.Add(New ListItem("Transport not selected", Guid.Empty.ToString))
		Dim transportList As ArrayList = KaTransport.GetAll(connection, " (deleted = 0) OR (id = " & Q(stagedTransportInfoTransportId) & ")", "name ASC")
		For Each transportInfo As KaTransport In transportList
			If Not limitTransportsToCarrierSelected OrElse currentCarrierId = Guid.Empty OrElse transportInfo.CarrierId = currentCarrierId Then
				Dim isTransportUsed As Boolean = Not transportInfo.Id.Equals(stagedTransportInfoTransportId) AndAlso transportIdsUsed.Contains(transportInfo.Id) ' It is not this transportId and it is not used elsewhere
				If Not isTransportUsed Then ' when transport is not previously used in alternate dropdown then add to dropdown list
					transportDropdownList.Items.Add(New ListItem(transportInfo.Name.Trim & IIf(transportInfo.Number.Trim.Length > 0, " (" & transportInfo.Number.Trim & ")", ""), transportInfo.Id.ToString))
				End If
			End If
		Next
		Try
			transportDropdownList.SelectedValue = stagedTransportInfoTransportId.ToString
		Catch ex As Exception
			transportDropdownList.SelectedIndex = 0
		End Try
	End Sub

	Private Sub PopulateTransportCompartmentList(ByRef transportCompartmentDropdownList As DropDownList, ByVal transportCompartmentId As Guid, ByVal transportId As Guid)
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim transportFound As Boolean = False
		transportCompartmentDropdownList.Items.Clear()

		transportCompartmentDropdownList.Items.Add(New ListItem("Transport compartment not selected", Guid.Empty.ToString))
		Dim transportList As ArrayList = KaTransportCompartment.GetAll(connection, " (deleted = 0 AND transport_id=" & Q(transportId) & ")", "position ASC")
		For Each transportInfo As KaTransportCompartment In transportList
			Dim compartmentUnit As KaUnit = Nothing
			Try
				compartmentUnit = New KaUnit(connection, transportInfo.UnitId)
			Catch ex As RecordNotFoundException

			End Try
			Dim capacity As String = ""
			If transportInfo.Capacity > 0 AndAlso compartmentUnit IsNot Nothing Then
				capacity = Format(transportInfo.Capacity, compartmentUnit.UnitPrecision) & " " & compartmentUnit.Abbreviation
			End If
			transportCompartmentDropdownList.Items.Add(New ListItem((transportInfo.Position + 1).ToString.Trim & IIf(capacity.Trim.Length > 0, " (" & capacity.Trim & ")", ""), transportInfo.Id.ToString()))
			If transportInfo.Id.Equals(transportCompartmentId) Then
				transportCompartmentDropdownList.SelectedIndex = transportCompartmentDropdownList.Items.Count - 1
				transportFound = True
			End If
		Next

		If Not transportFound Then
			transportCompartmentDropdownList.SelectedIndex = 0
		End If
	End Sub

	Private Sub PopulateDriversList(ByVal currentDriverId As Guid, ByVal currentOrders As List(Of KaStagedOrderOrder))
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim driverFound As Boolean = False
		With ddlDriver
			.Items.Clear()
			.Items.Add(New ListItem("Driver not selected", Guid.Empty.ToString)) ' populate the driver list
			Dim limitDriversToDriversAssignedToAccount As Boolean = False
			Boolean.TryParse(KaSetting.GetSetting(connection, "PointOfSale/LimitDriversToDriversAssignedToAccount", False), limitDriversToDriversAssignedToAccount)
			Dim drivers As New List(Of Guid) ' get a list of drivers that are associated with the list of accounts
			drivers.Add(currentDriverId)
			If limitDriversToDriversAssignedToAccount Then
				For Each driverAllowedForAll As KaDriver In KaDriver.GetAll(connection, "(disabled = 0) AND (deleted=0) AND valid_for_all_accounts=1", "")
					drivers.Add(driverAllowedForAll.Id)
				Next

				Dim accounts As New ArrayList() ' get a list of accounts associated with the selected order
				For Each stagedOrderOrder As KaStagedOrderOrder In currentOrders
					For Each orderAccount As KaOrderCustomerAccount In KaOrderCustomerAccount.GetAll(connection, "deleted=0 AND order_id=" & Q(stagedOrderOrder.OrderId), "")
						accounts.Add(orderAccount.CustomerAccountId)
					Next
				Next
				For Each accountId As Guid In accounts
					For Each accountDriver As KaCustomerAccountDriver In KaCustomerAccountDriver.GetAll(connection, "deleted=0 AND customer_account_id=" & Q(accountId), "")
						If drivers.IndexOf(accountDriver.DriverId) = -1 Then drivers.Add(accountDriver.DriverId)
					Next
				Next
			Else
				For Each driverInfo As KaDriver In KaDriver.GetAll(connection, "(disabled = 0) AND (deleted=0)", "name ASC")
					drivers.Add(driverInfo.Id)
				Next
			End If
			Dim validDriverIdList As String = ""
			For Each driverId As Guid In drivers
				If validDriverIdList.Length > 0 Then validDriverIdList &= ","
				validDriverIdList &= Q(driverId)
			Next
			If validDriverIdList.Length > 0 Then
				For Each driverAvailable As KaDriver In KaDriver.GetAll(connection, "(disabled = 0) AND (deleted=0) AND (id IN (" & validDriverIdList & "))", "name")
					.Items.Add(New ListItem(driverAvailable.Name, driverAvailable.Id.ToString()))
					If driverAvailable.Id.Equals(currentDriverId) Then
						.SelectedIndex = .Items.Count - 1
						driverFound = True
					End If
				Next

				If Not driverFound Then
					.SelectedIndex = 0
				End If
			End If
		End With
	End Sub

	Private Sub PopulateCarriersList(ByVal currentCarrierId As Guid)
		Dim carrierFound As Boolean = False
		With ddlCarrier
			.Items.Clear()

			.Items.Add(New ListItem("Carrier not selected", Guid.Empty.ToString))

			Dim carrierList As ArrayList = KaCarrier.GetAll(GetUserConnection(_currentUser.Id), " (deleted = 0) OR (id = " & Q(currentCarrierId) & ")", "name ASC")
			For Each carrierInfo As KaCarrier In carrierList
				.Items.Add(New ListItem(carrierInfo.Name.Trim, carrierInfo.Id.ToString))
				If carrierInfo.Id.Equals(currentCarrierId) Then
					.SelectedIndex = .Items.Count - 1
					carrierFound = True
				End If
			Next

			If Not carrierFound Then
				.SelectedIndex = 0
			End If
		End With
	End Sub

	Private Sub PopulateFacilityList(ByVal currentFacilityId As Guid)
		Dim facilityFound As Boolean = False
		With ddlFacility
			.Items.Clear()

			.Items.Add(New ListItem("Facility not selected", Guid.Empty.ToString))

			Dim facilityList As ArrayList = KaLocation.GetAll(GetUserConnection(_currentUser.Id), " (deleted = 0) OR (id = " & Q(currentFacilityId) & ")", "name ASC")
			For Each facilityInfo As KaLocation In facilityList
				.Items.Add(New ListItem(facilityInfo.Name.Trim, facilityInfo.Id.ToString))
				If facilityInfo.Id.Equals(currentFacilityId) Then
					.SelectedIndex = .Items.Count - 1
					facilityFound = True
				End If
			Next

			If Not facilityFound Then
				If .Items.Count = 2 Then
					.Items.RemoveAt(0) ' Remove the Facility not selected option
				End If
				.SelectedIndex = 0
			End If
		End With
		ddlFacility_SelectedIndexChanged(ddlFacility, New EventArgs)
	End Sub

	Private Sub PopulateBaysList()
		Dim currentBayId As Guid = _stagedOrderInfo.BayId
		Dim bayFound As Boolean = False
		Dim facilityId As Guid = _stagedOrderInfo.LocationId
		With ddlBayAssigned
			.Items.Clear()

			.Items.Add(New ListItem("Bay not selected", Guid.Empty.ToString))

			Dim bayList As ArrayList = KaBay.GetAll(GetUserConnection(_currentUser.Id), " ((location_id = " & Q(facilityId) & ") AND (deleted = 0)) OR (id = " & Q(currentBayId) & ")", "name ASC")
			For Each bayInfo As KaBay In bayList
				.Items.Add(New ListItem(bayInfo.Name.Trim, bayInfo.Id.ToString))
				If bayInfo.Id.Equals(currentBayId) Then
					.SelectedIndex = .Items.Count - 1
					bayFound = True
				End If
			Next

			If Not bayFound Then
				.SelectedIndex = 0
			End If
		End With
	End Sub

	Private Sub PopulateApplicatorsList(ByVal currentApplicatorId As Guid)
		Dim applicatorFound As Boolean = False
		With ddlOrderApplicator
			.Items.Clear()

			.Items.Add(New ListItem("Applicator not selected", Guid.Empty.ToString))

			Dim applicatorList As ArrayList = KaApplicator.GetAll(GetUserConnection(_currentUser.Id), "(deleted = 0) OR (id = " & Q(currentApplicatorId) & ")", "name ASC")
			For Each applicatorInfo As KaApplicator In applicatorList
				.Items.Add(New ListItem(applicatorInfo.Name.Trim, applicatorInfo.Id.ToString))
				If applicatorInfo.Id.Equals(currentApplicatorId) Then
					.SelectedIndex = .Items.Count - 1
					applicatorFound = True
				End If
			Next

			If Not applicatorFound Then
				.SelectedIndex = 0
			End If
		End With
	End Sub

	Private Sub PopulateProductList(ByRef ddlProductList As DropDownList, ByVal orderInfoList As List(Of KaOrder), ByVal currentCompartmentItemId As Guid)
		Dim orderItemFound As Boolean = False
		ddlProductList.Items.Clear()

		Dim orderIdList As String = ""
		For Each orderInfo As KaOrder In orderInfoList
			If orderIdList.Length > 0 Then orderIdList &= ","
			orderIdList &= Q(orderInfo.Id)
		Next
		If orderIdList.Length = 0 Then orderIdList = Q("")

		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim currentItemsTable As New DataTable
		Dim currentItemsDA As New OleDbDataAdapter("SELECT orders.number, order_items.id, order_items.unit_id, order_items.request, products.name, order_items.product_id " &
									"FROM order_items " &
									"INNER JOIN orders ON orders.id = order_items.order_id " &
									"INNER JOIN products ON products.id = order_items.product_id " &
									"WHERE (order_items.deleted = 0) " &
										"AND order_items.order_id IN (" & orderIdList & ")" &
									"ORDER BY products.name ASC, orders.number ASC", connection)
		If Tm2Database.CommandTimeout > 0 Then currentItemsDA.SelectCommand.CommandTimeout = Tm2Database.CommandTimeout
		currentItemsDA.Fill(currentItemsTable)
		If chkUseOrderPercents.Checked Then
			Dim locationId As Guid = Guid.Empty
			Try
				locationId = New KaLocation(connection, Guid.Parse(ddlFacility.SelectedValue)).Id
			Catch ex As RecordNotFoundException
				locationId = _stagedOrderInfo.LocationId
			End Try
			'Combine like products
			GroupOrderItemsIntoProducts(locationId, connection, currentItemsTable)
		End If

		If orderInfoList.Count <= 1 OrElse (chkUseOrderPercents.Checked AndAlso _currentOrderItemIds.Count = 0) Then ddlProductList.Items.Add(New ListItem("All products", Guid.Empty.ToString))
		If orderInfoList.Count = 0 Then
			ddlProductList.SelectedIndex = 0
			orderItemFound = True
		Else
			If Not chkUseOrderPercents.Checked Then
				For n As Integer = 0 To orderInfoList.Count - 1
					Dim orderInfo As KaOrder = orderInfoList(n)
					If (Not orderInfo.Deleted) AndAlso orderInfo.Id <> Guid.Empty Then
						ddlProductList.Items.Add(New ListItem("All products - " & orderInfo.Number, orderInfo.Id.ToString))
						If orderInfo.Id.Equals(currentCompartmentItemId) Then
							ddlProductList.SelectedIndex = ddlProductList.Items.Count - 1
							orderItemFound = True
						End If
					End If
				Next
			End If

			If orderInfoList.Count <= 1 OrElse Not chkUseOrderPercents.Checked OrElse _currentOrderItemIds.Count > 0 Then
				For Each rdr As DataRow In currentItemsTable.Rows
					Dim unitInfo As KaUnit
					Try
						unitInfo = New KaUnit(connection, rdr.Item("unit_id"))
					Catch ex As RecordNotFoundException
						unitInfo = New KaUnit
					End Try
					Dim productDisplay As String = rdr.Item("name").ToString().Trim & " (" & Format(rdr.Item("request"), unitInfo.UnitPrecision) & " " & unitInfo.Abbreviation & ")"
					If orderInfoList.Count > 1 AndAlso Not chkUseOrderPercents.Checked Then productDisplay &= " - " & rdr.Item("number").ToString().Trim
					ddlProductList.Items.Add(New ListItem(productDisplay, rdr.Item("id").ToString()))
					If rdr.Item("id").Equals(currentCompartmentItemId) Then
						ddlProductList.SelectedIndex = ddlProductList.Items.Count - 1
						orderItemFound = True
					End If
				Next
			End If
		End If

		If Not orderItemFound Then
			If Not _stagedOrderInfo.Id.Equals(Guid.Empty) AndAlso Not currentCompartmentItemId.Equals(Guid.Empty) Then
				DisplayJavaScriptMessage("ProductNotFoundWarning", Utilities.JsAlert(String.Format($"Not all compartment entries could be assigned.")), False)
				_assignProductError = True
			End If
			ddlProductList.SelectedIndex = 0
		End If
	End Sub

	Private Sub GroupOrderItemsIntoProducts(ByVal locationId As Guid, ByVal oConn As OleDbConnection, ByRef currentItemsTable As DataTable)
		Dim orderItemCounter As Integer = 0
		Do While orderItemCounter < currentItemsTable.Rows.Count
			Dim firstRow As DataRow = currentItemsTable.Rows(orderItemCounter)
			Dim orderItemCounter2 As Integer = orderItemCounter + 1
			Do While orderItemCounter2 < currentItemsTable.Rows.Count
				Dim secondRow As DataRow = currentItemsTable.Rows(orderItemCounter2)
				If firstRow.Item("product_id").Equals(secondRow.Item("product_id")) Then
					Try
						If _currentOrderItemIds.Contains(secondRow.Item("id")) Then
							' The second row is already on this staged order, so we need to leave it
							If _currentOrderItemIds.Contains(firstRow.Item("id")) Then
								' The first row is already on this staged order, so we need to leave it as well, and continue with the next product
								orderItemCounter2 += 1
								Continue Do
							End If
							secondRow.Item("request") += KaUnit.Convert(oConn, New KaQuantity(firstRow.Item("request"), firstRow.Item("unit_id")), New KaProduct(oConn, firstRow.Item("product_id")).GetDensity(locationId), secondRow.Item("unit_id")).Numeric
							currentItemsTable.Rows.RemoveAt(orderItemCounter)
							GroupOrderItemsIntoProducts(locationId, oConn, currentItemsTable)
							Exit Sub 'We can exit the sub, because it was called recursively, and the indexes all changed.
						End If
						firstRow.Item("request") += KaUnit.Convert(oConn, New KaQuantity(secondRow.Item("request"), secondRow.Item("unit_id")), New KaProduct(oConn, secondRow.Item("product_id")).GetDensity(locationId), firstRow.Item("unit_id")).Numeric
						currentItemsTable.Rows.RemoveAt(orderItemCounter2)
					Catch ex As UnitConversionException
						orderItemCounter2 += 1
					End Try
				Else
					orderItemCounter2 += 1
				End If
			Loop
			orderItemCounter += 1
		Loop
	End Sub

	Private Sub PopulateUnitOfMeasureList(ByRef ddlUnitOfMeasure As DropDownList, ByVal currentUnitId As Guid, Optional ByVal includeVolume As Boolean = True)
		ddlUnitOfMeasure.Items.Clear()
		For Each unit As KaUnit In KaUnit.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 OR id=" & Q(currentUnitId), "name ASC")
			If Not KaUnit.IsTime(unit.BaseUnit) AndAlso (includeVolume OrElse KaUnit.IsWeight(unit.BaseUnit)) Then
				ddlUnitOfMeasure.Items.Add(New ListItem(unit.Name.Trim, unit.Id.ToString()))
				If unit.Id = currentUnitId Then ddlUnitOfMeasure.SelectedIndex = ddlUnitOfMeasure.Items.Count - 1
			End If
		Next
		If ddlUnitOfMeasure.SelectedIndex = -1 AndAlso ddlUnitOfMeasure.Items.Count > 0 Then ddlUnitOfMeasure.SelectedIndex = 0
	End Sub

	Private Sub PopulateOrdersList()
		Dim currentOrders As List(Of KaStagedOrderOrder) = _stagedOrderInfo.Orders
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim stagedOrderId As Guid = Guid.Empty
		If _stagedOrderInfo IsNot Nothing Then
			stagedOrderId = _stagedOrderInfo.Id
		ElseIf ddlStagedOrders.SelectedIndex >= 0 Then
			Guid.TryParse(ddlStagedOrders.SelectedValue, stagedOrderId)
		End If

		Dim ordersAssignedDA As New OleDbDataAdapter("SELECT DISTINCT staged_order_orders.order_id " &
										   "FROM staged_order_orders " &
										   "INNER JOIN staged_orders ON staged_orders.id = staged_order_orders.staged_order_id " &
										   "WHERE (staged_order_orders.deleted = 0) " &
												"AND (staged_orders.deleted = 0) " &
												"AND (staged_orders.id <> " & Q(stagedOrderId) & ")", connection)
		If Tm2Database.CommandTimeout > 0 Then ordersAssignedDA.SelectCommand.CommandTimeout = Tm2Database.CommandTimeout
		Dim ordersAssignedTable As New DataTable
		ordersAssignedDA.Fill(ordersAssignedTable)
		Dim orderIdsAssignedElsewhereString As String = Q(Guid.Empty)
		For Each orderAssignedRow As DataRow In ordersAssignedTable.Rows
			orderIdsAssignedElsewhereString &= "," & Q(orderAssignedRow.Item(0))
		Next

		Dim orderFound As Boolean = False
		Dim unusedOrdersIndex As Integer = lstUnusedOrders.SelectedIndex
		Dim usedOrdersIndex As Integer = lstUsedOrders.SelectedIndex

		lstUnusedOrders.Items.Clear()
		lstUsedOrders.Items.Clear()
		Dim locationId As Guid = Guid.Empty
		Try
			locationId = New KaLocation(connection, Guid.Parse(ddlFacility.SelectedValue)).Id
		Catch ex As RecordNotFoundException
			locationId = _stagedOrderInfo.LocationId
		End Try

		Dim comparisonPercentage As Double = KaSetting.GetSetting(connection, KaSetting.SN_ORDER_COMPARISON_PERCENT_TOLERANCE, KaSetting.SD_ORDER_COMPARISON_PERCENT_TOLERANCE)
		Dim currentOrderString As String = ""
		Dim currentOrderIds As New List(Of Guid)
		Dim percentageAssigned As Double = 0.0
		Dim currentOrdersAreVrt As Boolean = False
		For Each currentOrder As KaStagedOrderOrder In currentOrders
			If Not currentOrder.Deleted AndAlso Not currentOrderIds.Contains(currentOrder.OrderId) Then
				Try
					Dim orderInfo As New KaOrder(connection, currentOrder.OrderId)
					If orderInfo.Deleted Then Continue For
					If currentOrderString.Length > 0 Then currentOrderString &= ","
					currentOrderString &= Q(currentOrder.OrderId)
					currentOrderIds.Add(currentOrder.OrderId)
					percentageAssigned += currentOrder.Percentage
					If orderInfo.DoNotBlend Then currentOrdersAreVrt = True
				Catch ex As RecordNotFoundException

				End Try
			End If
		Next

		If chkUseOrderPercents.Checked Then
			litPercentageAssigned.Text = $"<label>Total Assigned</label><span class=""input"" style=""text-align:right;"">{percentageAssigned.ToString()}%</span>"
		Else
			litPercentageAssigned.Text = ""
		End If

		Dim currentOrderInfoList(currentOrderIds.Count - 1) As KaOrder
		For n As Integer = 0 To currentOrderIds.Count - 1
			currentOrderInfoList(n) = New KaOrder(connection, currentOrderIds(n))
		Next
		Dim ordersCompatible As Boolean = True
		Try
			If currentOrderInfoList.Length > 0 AndAlso chkUseOrderPercents.Checked Then ordersCompatible = KaOrder.OrdersCompatible(locationId, comparisonPercentage, currentOrderInfoList)
		Catch ex As UnitConversionException
		End Try

		Dim showExtraOrderOptions As Boolean = (currentOrderIds.Count > 1)

		Dim blendedLoadsOnly As Boolean = ordersCompatible AndAlso chkUseOrderPercents.Checked
		Dim showPercentages As Boolean = showExtraOrderOptions And blendedLoadsOnly

		Dim orderList As New ArrayList
		Dim allowOrdersToBeAssignedToMultipleStagedOrders As Boolean = True
		Boolean.TryParse(KaSetting.GetSetting(connection, "StagedOrder/AllowOrdersToBeAssignedToMultipleStagedOrders", True), allowOrdersToBeAssignedToMultipleStagedOrders)
		If blendedLoadsOnly AndAlso currentOrders.Count > 0 Then
			' Limit the initial list to only orders that contain these products to speed up processing
			Dim currentProductList As New List(Of Guid)
			For Each currentOrder As KaOrder In currentOrderInfoList
				For Each orderItem As KaOrderItem In currentOrder.OrderItems
					If Not orderItem.Deleted AndAlso Not currentProductList.Contains(orderItem.ProductId) Then currentProductList.Add(orderItem.ProductId)
				Next
			Next
			If currentProductList.Count > 0 Then
				Dim validProducts As String = ""
				For Each productId As Guid In currentProductList
					If validProducts.Length > 0 Then validProducts &= ","
					validProducts &= Q(productId)
				Next
				' Get only the orders that include these products, and don't include any that have other products
				Dim sql As String = ""
				If currentOrderIds.Count > 0 Then sql &= "SELECT orders.* FROM orders WHERE (id IN (" & currentOrderString & ")) UNION "
				sql &= "SELECT orders.* " &
						"FROM orders " &
						"INNER JOIN order_items ON orders.id = order_items.order_id " &
						"WHERE ((order_items.deleted = 0) " &
						"AND (orders.deleted = 0) " &
						"AND (orders.completed = 0) "
				If chkUseOrderPercents.Checked Then sql &= "AND (orders.do_not_blend = " & Q(currentOrdersAreVrt) & ") "
				sql &= "AND (NOT (orders.id IN (SELECT order_customer_accounts.order_id FROM order_customer_accounts INNER JOIN customer_accounts ON order_customer_accounts.customer_account_id = customer_accounts.id WHERE (order_customer_accounts.deleted = 0) AND (order_customer_accounts.percentage > 0) AND ((customer_accounts.deleted = 1) OR (customer_accounts.disabled = 1))))) "
				If Not _currentUser.OwnerId.Equals(Guid.Empty) Then sql &= " AND (orders.owner_id=" & Q(_currentUser.OwnerId) & ") "
				If Not allowOrdersToBeAssignedToMultipleStagedOrders Then sql &= "AND (NOT orders.id IN (" & orderIdsAssignedElsewhereString & ")) "
				sql &= "AND (order_items.product_id in (" & validProducts & ")) " &
				"AND NOT (orders.id in (SELECT DISTINCT orders.id " &
										"FROM orders " &
										"INNER JOIN order_items ON orders.id = order_items.order_id " &
										"WHERE (order_items.deleted = 0) " &
										"AND (orders.deleted = 0) " &
										"AND NOT (order_items.product_id in (" & validProducts & "))))) "
				sql &= "ORDER BY orders.number"

				Dim getCurrentProductIdsRdr As OleDbDataReader = Tm2Database.ExecuteReader(connection, sql)
				Do While getCurrentProductIdsRdr.Read
					Try
						Dim newOrder As New KaOrder(getCurrentProductIdsRdr)
						If currentOrderInfoList(0).OrderItems.Count = newOrder.OrderItems.Count Then orderList.Add(newOrder)
					Catch ex As RecordNotFoundException

					End Try
				Loop
				getCurrentProductIdsRdr.Close()
			End If
		Else
			Dim ordersSql As String = "SELECT * FROM orders WHERE ((completed=0) AND (deleted=0) " &
									   "AND (NOT (orders.id IN (SELECT order_customer_accounts.order_id " &
																"FROM order_customer_accounts " &
																"INNER JOIN customer_accounts ON order_customer_accounts.customer_account_id = customer_accounts.id " &
																"WHERE (order_customer_accounts.deleted = 0) " &
																	"AND (order_customer_accounts.percentage > 0) " &
																	"AND ((customer_accounts.deleted = 1) OR (customer_accounts.disabled = 1))))) "
			If Not _currentUser.OwnerId.Equals(Guid.Empty) Then ordersSql &= " AND (orders.owner_id=" & Q(_currentUser.OwnerId) & ")"
			If Not allowOrdersToBeAssignedToMultipleStagedOrders Then ordersSql &= "AND (NOT orders.id IN (" & orderIdsAssignedElsewhereString & ")) "
			ordersSql &= ")"
			If currentOrderIds.Count > 0 Then ordersSql &= " UNION SELECT * FROM orders WHERE (id IN (" & currentOrderString & "))"
			ordersSql &= " ORDER BY number ASC"
			Dim ordersRdr As OleDbDataReader = Tm2Database.ExecuteReader(connection, ordersSql)
			Try
				While ordersRdr.Read()
					orderList.Add(New KaOrder(ordersRdr))
				End While
			Finally
				ordersRdr.Close()
			End Try
		End If

		Dim filteredOrderIdList As New List(Of Guid)
		If tbxFind.Text.Trim.Length > 0 Then filteredOrderIdList = KaOrder.GetOrderIdsWithKeyword(connection, Nothing, _currentUser.OwnerId, tbxFind.Text, False)

		For Each orderInfo As KaOrder In orderList
			Dim orderAssignedElsewhere As Boolean = False
			For Each orderAssignedRow As DataRow In ordersAssignedTable.Rows
				If orderAssignedRow.Item(0).Equals(orderInfo.Id) Then
					orderAssignedElsewhere = True
					Exit For
				End If
			Next
			If currentOrderIds.Contains(orderInfo.Id) Then
				Dim newStagedOrderOrder As KaStagedOrderOrder = Nothing
				For Each currentOrder As KaStagedOrderOrder In currentOrders
					If currentOrder.OrderId.Equals(orderInfo.Id) Then
						newStagedOrderOrder = currentOrder
						Exit For
					End If
				Next
				If newStagedOrderOrder Is Nothing Then
					newStagedOrderOrder = New KaStagedOrderOrder
					newStagedOrderOrder.OrderId = orderInfo.Id
					newStagedOrderOrder.Percentage = 100
				End If
				lstUsedOrders.Items.Add(New ListItem(orderInfo.Number.Trim & IIf(orderAssignedElsewhere, " *", "") & IIf(showPercentages, " " & newStagedOrderOrder.Percentage & "%", "") & IIf(orderInfo.DoNotBlend, " (Do Not blend)", ""), Tm2Database.ToXml(newStagedOrderOrder, GetType(KaStagedOrderOrder))))
			Else
				Dim addOrder As Boolean = True

				If tbxFind.Text.Trim.Length > 0 Then
					addOrder = filteredOrderIdList.Contains(orderInfo.Id) ' orderInfo.Number.ToUpper.Contains(tbxFind.Text.Trim.ToUpper)
				End If
				If addOrder AndAlso blendedLoadsOnly AndAlso currentOrderInfoList.Length > 0 Then
					'Check to see if they are compatible
					Dim ordersToCheck(currentOrderInfoList.Length) As KaOrder
					currentOrderInfoList.CopyTo(ordersToCheck, 0)
					ordersToCheck(ordersToCheck.Length - 1) = orderInfo
					Try
						If Not KaOrder.OrdersCompatible(locationId, comparisonPercentage, ordersToCheck) Then
							addOrder = False
						End If
					Catch ex As UnitConversionException
						addOrder = False
					End Try
				End If
				If addOrder Then
					lstUnusedOrders.Items.Add(New ListItem(orderInfo.Number.Trim & IIf(orderAssignedElsewhere, " *", "") & IIf(orderInfo.DoNotBlend, " (Do Not blend)", ""), orderInfo.Id.ToString))
				End If
			End If
		Next
		Dim currentDriverId As Guid = Guid.Empty
		Guid.TryParse(ddlDriver.SelectedValue, currentDriverId)
		PopulateDriversList(currentDriverId, currentOrders)

		lblOrderPercentage.Visible = showPercentages

		If lstUnusedOrders.Items.Count > 0 Then lstUnusedOrders.SelectedIndex = Math.Max(0, Math.Min(unusedOrdersIndex, lstUnusedOrders.Items.Count - 1))
		If lstUsedOrders.Items.Count > 0 Then lstUsedOrders.SelectedIndex = Math.Max(0, Math.Min(usedOrdersIndex, lstUsedOrders.Items.Count - 1))

		lstUsedOrders_SelectedIndexChanged(lstUsedOrders, New EventArgs)
		chkUseOrderPercents.Enabled = (lstUsedOrders.Items.Count <= 1)
		Dim ordersSelected As Boolean = (lstUsedOrders.Items.Count > 0)
		rowAutofill.Visible = ordersSelected AndAlso Not locationId.Equals(Guid.Empty)
	End Sub

	Private Sub ddlStagedOrders_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles ddlStagedOrders.SelectedIndexChanged
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim selectedStagedOrderId As Guid = Guid.Empty
		Guid.TryParse(ddlStagedOrders.SelectedValue, selectedStagedOrderId)

		Dim userOrderPercentage As Boolean
		Boolean.TryParse(KaSetting.GetSetting(connection, "StagedOrder/UseOrderPercentage", False), userOrderPercentage)
		If (_stagedOrderInfo IsNot Nothing AndAlso Not _stagedOrderInfo.Id.Equals(selectedStagedOrderId)) Then pnlStagedTransports.Controls.Clear()

		Try
			_stagedOrderInfo = New KaStagedOrder(connection, selectedStagedOrderId)
			_stagedOrderInfo.GetChildren(connection, Nothing, True)
			userOrderPercentage = _stagedOrderInfo.UseOrderPercents
			If _stagedOrderInfo.Orders.Count = 0 AndAlso Not _stagedOrderInfo.OrderId.Equals(Guid.Empty) Then
				Dim newStagedOrderOrder As New KaStagedOrderOrder
				With newStagedOrderOrder
					.OrderId = _stagedOrderInfo.OrderId
					.Percentage = 100
					.LastUpdated = Now
					.StagedOrderId = _stagedOrderInfo.Id
					.Acres = _stagedOrderInfo.Acres
					.CustomerAccountLocationId = .CustomerAccountLocationId
				End With
				_stagedOrderInfo.Orders.Add(newStagedOrderOrder)
				_stagedOrderInfo.OrderId = Guid.Empty
				_stagedOrderInfo.SqlUpdate(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
			End If
		Catch ex As RecordNotFoundException
			_stagedOrderInfo = Nothing
		End Try
		If _stagedOrderInfo Is Nothing Then _stagedOrderInfo = New KaStagedOrder
		_stagedOrderInfo.GetChildren(connection, Nothing, True)
		Try
			RemoveOrderItemListHandlers()

			With _stagedOrderInfo
				_currentOrderItemIds.Clear()
				For Each compartment As KaStagedOrderCompartment In .Compartments
					If Not compartment.Deleted Then
						For Each item As KaStagedOrderCompartmentItem In compartment.CompartmentItems
							If Not item.Deleted AndAlso Not item.OrderItemId.Equals(Guid.Empty) Then
								_currentOrderItemIds.Add(item.OrderItemId)
							End If
						Next
					End If
				Next
				ddlCustomerSite.Items.Clear()
				ddlCustomerSite.Items.Add(New ListItem("", .CustomerAccountLocationId.ToString()))
				ddlCustomerSite.SelectedIndex = 0
				PopulateCarriersList(.CarrierId)
				PopulateDriversList(.DriverId, .Orders)
				.UseOrderPercents = userOrderPercentage
				chkUseOrderPercents.Checked = .UseOrderPercents
				PopulateFacilityList(.LocationId)
				lstUsedOrders_SelectedIndexChanged(lstUsedOrders, New EventArgs)
				Dim orderInfoList As List(Of KaOrder) = GetOrderInfo()

				If .LossInWeight Then
					rblTicketCreationSource.SelectedValue = "TicketsAreCreatedForAmountUsedOffsite"
				Else
					rblTicketCreationSource.SelectedValue = "TicketsAreCreatedForAmountFilledInFacility"
				End If

				For Each stagedTransportInfo As KaStagedOrderTransport In .Transports
					If Not .Deleted Then
						Dim originalTareInfo As New OriginalTareInfo
						With originalTareInfo
							.StagedTransportId = stagedTransportInfo.Id
							.OriginalTareInfo = New KaTimestampedQuantity(stagedTransportInfo.TareWeight, stagedTransportInfo.TareUnitId, stagedTransportInfo.TaredAt, stagedTransportInfo.TareManual)
						End With
						_originalTareValues.Add(originalTareInfo)
					End If
				Next

				tbxNotes.Text = .Notes
				' Set visibility of locked status controls
				btnSetLockedStatus.Visible = False
				btnClearLockedStatus.Visible = False
				Dim shouldEnable As Boolean = _currentUserPermission(_currentTableName).Edit AndAlso Not .Id.Equals(Guid.Empty)
				If .Locked Then
					lblOrderDetailStatus.Text = "Staged order locked"
					If .SentToPcName.Trim.Length > 0 Then lblOrderDetailStatus.Text &= " by " & Server.HtmlEncode(.SentToPcName)
					pnlOrderDetailStatus.Visible = True
					btnClearLockedStatus.Visible = shouldEnable
					btnDelete.Enabled = False
				Else
					If .Complete Then
						lblOrderDetailStatus.Text = "Staged order completed"
						pnlOrderDetailStatus.Visible = True
					Else
						'check if there are in progress records for this staged order
						Dim inProgressRecordCount As Integer = 0
						If .Id <> Guid.Empty Then
							Dim inProgressReader As OleDbDataReader = Tm2Database.ExecuteReader(connection, $"SELECT COUNT(*) FROM {KaInProgress.TABLE_NAME} WHERE {KaInProgress.FN_STAGED_ORDER_ID} = {Database.Q(.Id)}")
							Try
								If inProgressReader.Read() Then
									inProgressRecordCount = inProgressReader.Item(0)
								End If
							Finally
								inProgressReader.Close()
							End Try
						End If
						If inProgressRecordCount = 0 Then
							lblOrderDetailStatus.Text = ""
							pnlOrderDetailStatus.Visible = False
						Else
							lblOrderDetailStatus.Text = "Staged order has in progress records"
							pnlOrderDetailStatus.Visible = True
						End If
					End If
					btnSetLockedStatus.Visible = shouldEnable
					btnDelete.Enabled = shouldEnable AndAlso _currentUserPermission(_currentTableName).Delete
				End If

				CreateDynamicControls()
			End With
			'SetTareWeightOnTransports.Value = "False"
			CompartmentItemQuantityChanged(sender, e)
		Finally
			AddOrderItemListHandlers()
		End Try
		SetControlUsabilityFromPermissions()
	End Sub

	Private Sub AddNewTransportButton()
		Dim addTransportRow As New HtmlGenericControl("li")
		pnlStagedTransports.Controls.Add(addTransportRow)

		Dim addTransportButton As New LinkButton
		With addTransportButton
			.ID = "btnAddTransport"
			.CssClass = "button"
			.Text = "+"
			.ToolTip = "Add transport"
		End With
		AddHandler addTransportButton.Click, AddressOf AddTransportClicked
		addTransportRow.Controls.Add(addTransportButton)

		Dim addTransportLabel As New HtmlGenericControl("label")
		With addTransportLabel
			.Attributes("style") = "text-align: left;"
			.InnerText = "Add transport"
		End With
		addTransportRow.Controls.Add(addTransportLabel)
	End Sub

	Protected Overrides Function SaveViewState() As Object
		Dim viewState(2) As Object
		'Saving the grid values to the View State
		_stagedOrderInfo = ConvertPageToStagedOrder()
		Dim originalTareList As New List(Of OriginalTareInfo)
		ConvertToOriginalTareWeights(originalTareList)
		viewState(0) = _stagedOrderInfo
		viewState(1) = originalTareList
		viewState(2) = MyBase.SaveViewState()
		Return viewState
	End Function

	Protected Overrides Sub LoadViewState(savedState As Object)
		'Getting the dropdown list value from view state.
		_assignProductError = False
		If savedState IsNot Nothing AndAlso CType(savedState, Object).Length = 3 Then
			_currentUser = Utilities.GetUser(Me)
			Dim viewState As Object() = savedState
			_stagedOrderInfo = viewState(0)
			_originalTareValues = viewState(1)
			CreateDynamicControls()
			MyBase.LoadViewState(viewState(2))
		Else
			MyBase.LoadViewState(savedState)
		End If
	End Sub

	Private Sub RemoveTransportClicked(ByVal sender As Object, ByVal e As EventArgs)
		' Transports Compartment table is transportRow.ID & "_compTable"
		Dim controlIdStrings() As String = CType(sender, LinkButton).ID.Split("_")
		Dim currentTransportRowId As String = controlIdStrings(0)
		Dim stagedOrderTransportId As Guid = Guid.Parse(CType(FindControl(currentTransportRowId & "_Id"), HtmlInputHidden).Value)

		Dim rowCounter As Integer = 0
		Do While rowCounter < _stagedOrderInfo.Transports.Count
			If _stagedOrderInfo.Transports(rowCounter).Id = stagedOrderTransportId Then
				_stagedOrderInfo.Transports.RemoveAt(rowCounter)
				Exit Do
			End If
			rowCounter += 1
		Loop

		If pnlStagedTransports.Controls.Count <= 1 Then
			AddTransportClicked(sender, e)
		End If
		CreateDynamicControls()
	End Sub

	Private Sub AddTransportClicked(ByVal sender As Object, ByVal e As EventArgs) 'Handles btnAddTransport.Click
		Dim stagedTransportInfo As New KaStagedOrderTransport
		With stagedTransportInfo
			.Deleted = False
			.Id = Guid.NewGuid
			.TransportId = Guid.Empty
		End With
		_stagedOrderInfo.Transports.Add(stagedTransportInfo)

		CreateDynamicControls()
	End Sub

	Private Sub RemoveCompartmentClicked(ByVal sender As Object, ByVal e As EventArgs)
		' Staged Comp ID is currentCompRow.ID & "_Id"
		' Transports table is transportRow.ID & "_compTable"
		Dim controlIdStrings() As String = CType(sender, LinkButton).ID.Split("_")
		Dim currentTransportRowId As String = controlIdStrings(0)
		Dim currentStagedCompId As Guid = Guid.Parse(CType(FindControl(controlIdStrings(0) & "_" & controlIdStrings(1) & "_" & controlIdStrings(2) & "_Id"), HtmlInputHidden).Value)
		Dim stagedOrderTransportId As Guid = Guid.Parse(CType(FindControl(currentTransportRowId & "_Id"), HtmlInputHidden).Value)

		Dim compartmentCounter As Integer = 0
		Do While compartmentCounter < _stagedOrderInfo.Compartments.Count
			If _stagedOrderInfo.Compartments(compartmentCounter).Id = currentStagedCompId OrElse _stagedOrderInfo.Compartments(compartmentCounter).Deleted Then
				_stagedOrderInfo.Compartments.RemoveAt(compartmentCounter)
			End If
			compartmentCounter += 1
		Loop

		If _stagedOrderInfo.Compartments.Count < 1 Then
			AddCompartmentClicked(sender, e)
		End If

		compartmentCounter = 0
		Do While compartmentCounter < _stagedOrderInfo.Compartments.Count
			If _stagedOrderInfo.Compartments(compartmentCounter).Deleted Then
				_stagedOrderInfo.Compartments.RemoveAt(compartmentCounter)
			Else
				_stagedOrderInfo.Compartments(compartmentCounter).Position = compartmentCounter
				compartmentCounter += 1
			End If
		Loop
		CreateDynamicControls()
	End Sub

	Private Sub MoveCompartmentUp(ByVal sender As Object, ByVal e As EventArgs)
		' Staged Comp ID is currentCompRow.ID & "_Id"
		' Transports table is transportRow.ID & "_compTable"
		Dim controlIdStrings() As String = CType(sender, LinkButton).ID.Split("_")
		Dim currentTransportRowId As String = controlIdStrings(0)
		Dim currentStagedCompId As Guid = Guid.Parse(CType(FindControl(controlIdStrings(0) & "_" & controlIdStrings(1) & "_" & controlIdStrings(2) & "_Id"), HtmlInputHidden).Value)
		Dim stagedOrderTransportId As Guid = Guid.Parse(CType(FindControl(currentTransportRowId & "_Id"), HtmlInputHidden).Value)

		Dim compartmentCounter As Integer = 0
		Do While compartmentCounter < _stagedOrderInfo.Compartments.Count
			If _stagedOrderInfo.Compartments(compartmentCounter).Id = currentStagedCompId OrElse _stagedOrderInfo.Compartments(compartmentCounter).Deleted Then
				Dim stagedOrderCompartment As KaStagedOrderCompartment = _stagedOrderInfo.Compartments(compartmentCounter)
				_stagedOrderInfo.Compartments.RemoveAt(compartmentCounter)
				_stagedOrderInfo.Compartments.Insert(Math.Max(0, compartmentCounter - 1), stagedOrderCompartment)
				Exit Do
			End If
			compartmentCounter += 1
		Loop

		CreateDynamicControls()
	End Sub

	Private Sub MoveCompartmentDown(ByVal sender As Object, ByVal e As EventArgs)
		' Staged Comp ID is currentCompRow.ID & "_Id"
		' Transports table is transportRow.ID & "_compTable"
		Dim controlIdStrings() As String = CType(sender, LinkButton).ID.Split("_")
		Dim currentTransportRowId As String = controlIdStrings(0)
		Dim currentStagedCompId As Guid = Guid.Parse(CType(FindControl(controlIdStrings(0) & "_" & controlIdStrings(1) & "_" & controlIdStrings(2) & "_Id"), HtmlInputHidden).Value)
		Dim stagedOrderTransportId As Guid = Guid.Parse(CType(FindControl(currentTransportRowId & "_Id"), HtmlInputHidden).Value)

		Dim compartmentCounter As Integer = 0
		Do While compartmentCounter < _stagedOrderInfo.Compartments.Count
			If _stagedOrderInfo.Compartments(compartmentCounter).Id = currentStagedCompId OrElse _stagedOrderInfo.Compartments(compartmentCounter).Deleted Then
				Dim stagedOrderCompartment As KaStagedOrderCompartment = _stagedOrderInfo.Compartments(compartmentCounter)
				_stagedOrderInfo.Compartments.RemoveAt(compartmentCounter)
				If compartmentCounter < _stagedOrderInfo.Compartments.Count Then
					_stagedOrderInfo.Compartments.Insert(compartmentCounter + 1, stagedOrderCompartment)
				Else
					_stagedOrderInfo.Compartments.Add(stagedOrderCompartment)
				End If
				Exit Do
			End If
			compartmentCounter += 1
		Loop

		CreateDynamicControls()
	End Sub

	Private Sub AddCompartmentClicked(ByVal sender As Object, ByVal e As EventArgs)
		' Button Id is transportRow.ID & "_btnAddCompartment" & itemCounter.ToString
		' Staged Transport ID is transportRow.ID & "_Id"
		Dim controlIdStrings() As String = CType(sender, LinkButton).ID.Split("_")
		Dim currentTransportRowId As String = controlIdStrings(0)
		Dim stagedOrderTransportId As Guid = Guid.Parse(CType(FindControl(currentTransportRowId & "_Id"), HtmlInputHidden).Value)
		Dim newCompartmentInfo As New KaStagedOrderCompartment
		With newCompartmentInfo
			Dim newCompItemInfo As New KaStagedOrderCompartmentItem
			With newCompItemInfo
				.Deleted = False
				.Id = Guid.NewGuid
				.OrderItemId = Guid.Empty
				.Quantity = 0.0
				.UnitId = KaUnit.GetSystemDefaultMassUnitOfMeasure(GetUserConnection(_currentUser.Id), Nothing)
			End With
			.CompartmentItems.Add(newCompItemInfo)
			.Deleted = False
			.Id = Guid.NewGuid
			.StagedOrderTransportId = stagedOrderTransportId
		End With
		_stagedOrderInfo.Compartments.Add(newCompartmentInfo)
		Dim rowCounter As Integer = 0
		For Each compartment As KaStagedOrderCompartment In _stagedOrderInfo.Compartments
			compartment.Position = rowCounter
			rowCounter += 1
		Next
		CreateDynamicControls()
	End Sub

	Private Sub RemoveCompartmentItemClicked(ByVal sender As Object, ByVal e As EventArgs)
		Dim controlIdStrings() As String = CType(sender, LinkButton).ID.Split("_")
		Dim currentTransportRowId As String = controlIdStrings(0)
		Dim currentStagedCompId As Guid = Guid.Parse(CType(FindControl(controlIdStrings(0) & "_" & controlIdStrings(1) & "_" & controlIdStrings(2) & "_Id"), HtmlInputHidden).Value)
		Dim currentStagedCompItemId As Guid = Guid.Parse(CType(FindControl(controlIdStrings(0) & "_" & controlIdStrings(1) & "_" & controlIdStrings(2) & "_" & controlIdStrings(3) & "_" & controlIdStrings(4) & "_Id"), HtmlInputHidden).Value)

		For Each compartment As KaStagedOrderCompartment In _stagedOrderInfo.Compartments
			If compartment.Id.Equals(currentStagedCompId) Then
				Dim rowCounter As Integer = 0
				Do While rowCounter < compartment.CompartmentItems.Count
					If compartment.CompartmentItems(rowCounter).Id = currentStagedCompItemId OrElse compartment.CompartmentItems(rowCounter).Deleted Then
						compartment.CompartmentItems.RemoveAt(rowCounter)
					End If
					rowCounter += 1
				Loop
				If compartment.CompartmentItems.Count < 1 Then
					AddCompartmentItemClicked(sender, e)
				End If
				rowCounter = 0
				For Each item As KaStagedOrderCompartmentItem In compartment.CompartmentItems
					item.Position = rowCounter
					rowCounter += 1
				Next
				Exit For
			End If
		Next

		CreateDynamicControls()
	End Sub

	Private Sub AddCompartmentItemClicked(ByVal sender As Object, ByVal e As EventArgs)
		' Button Id is transportRow.ID & "_btnAddCompartmentItem" & itemCounter.ToString
		' Staged Comp ID is transportRow.ID & "_Id"
		' products table is newCompartmentRow.ID & "_ProdTable"
		Dim controlIdStrings() As String = CType(sender, LinkButton).ID.Split("_")
		Dim currentTransportRowId As String = controlIdStrings(0)
		Dim currentStagedCompId As Guid = Guid.Parse(CType(FindControl(controlIdStrings(0) & "_" & controlIdStrings(1) & "_" & controlIdStrings(2) & "_Id"), HtmlInputHidden).Value)

		Dim newCompItemInfo As New KaStagedOrderCompartmentItem
		With newCompItemInfo
			.Deleted = False
			.Id = Guid.NewGuid
			.OrderItemId = Guid.Empty
			.Quantity = 0.0
			.UnitId = KaUnit.GetSystemDefaultMassUnitOfMeasure(GetUserConnection(_currentUser.Id), Nothing)
			.StagedOrderCompartmentId = currentStagedCompId
		End With
		For Each compartment As KaStagedOrderCompartment In _stagedOrderInfo.Compartments
			If compartment.Id.Equals(currentStagedCompId) Then
				compartment.CompartmentItems.Add(newCompItemInfo)
				Dim rowCounter As Integer = 0
				For Each item As KaStagedOrderCompartmentItem In compartment.CompartmentItems
					item.Position = rowCounter
					rowCounter += 1
				Next
			End If
		Next
		CreateDynamicControls()
	End Sub

#Region " Convert Page, Transport and Compartment Rows into Objects "
	Private Function ConvertPageToStagedOrder() As KaStagedOrder
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)

		With _stagedOrderInfo
			Guid.TryParse(CType(ddlFacility.SelectedItem, ListItem).Value.ToString(), .LocationId)
			Guid.TryParse(CType(ddlBayAssigned.SelectedItem, ListItem).Value, .BayId)
			.LossInWeight = rblTicketCreationSource.SelectedValue = "TicketsAreCreatedForAmountUsedOffsite"

			.Orders = GetCurrentStagedOrderOrders()
			Dim transportList As New List(Of KaStagedOrderTransport)
			Dim compartmentList As New List(Of KaStagedOrderCompartment)
			Dim originalTareList As New List(Of OriginalTareInfo)
			ConvertTransportTableToObjects(transportList, compartmentList)
			.Compartments.Clear()
			.Compartments.AddRange(compartmentList)
			.Transports.Clear()
			.Transports.AddRange(transportList)

			Guid.TryParse(CType(ddlCarrier.SelectedItem, ListItem).Value.ToString(), .CarrierId)
			Guid.TryParse(CType(ddlDriver.SelectedItem, ListItem).Value.ToString(), .DriverId)
			Guid.TryParse(CType(ddlCustomerSite.SelectedItem, ListItem).Value.ToString(), .CustomerAccountLocationId)

			Dim massUnitOfMeasure As New KaUnit(connection, KaUnit.GetSystemDefaultMassUnitOfMeasure(connection, Nothing))
			Dim volumeUnitOfMeasure As New KaUnit(connection, KaUnit.GetSystemDefaultVolumeUnitOfMeasure(connection, Nothing))
			Dim orderInfoList As List(Of KaOrder) = GetOrderInfo()

			'Reassign positions
			Dim compartmentIndexes As New Hashtable()

			Dim position As Integer = 0
			For Each compartmentInfo As KaStagedOrderCompartment In .Compartments
				compartmentInfo.Position = position
				position += 1
			Next

			.UseOrderPercents = chkUseOrderPercents.Checked
			.Notes = tbxNotes.Text
			If ddlStagedOrders.SelectedIndex = 0 Then
				.Source = String.Format("{0}/{1}", System.Net.Dns.GetHostName(), "TM2")
			End If
		End With
		Return _stagedOrderInfo
	End Function

	Private Sub ConvertTransportTableToObjects(ByRef transportList As List(Of KaStagedOrderTransport), ByRef compartmentList As List(Of KaStagedOrderCompartment))
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		For Each stagedObject As Object In pnlStagedTransports.Controls
			If Not TypeOf stagedObject Is HtmlGenericControl Then Continue For
			Dim transportRow As HtmlGenericControl = stagedObject
			If transportRow.ID IsNot Nothing AndAlso transportRow.ID.Contains("Transport") Then
				' This is a transport row
				Dim rowNumber As Integer = CInt(CType(transportRow.FindControl(transportRow.ID & "_RowNumber"), HtmlInputHidden).Value)
				Dim stagedOrderTransportId As Guid = Guid.NewGuid
				Guid.TryParse(CType(transportRow.FindControl(transportRow.ID & "_Id"), HtmlInputHidden).Value, stagedOrderTransportId)
				Dim stagedOrderTransportInfo As KaStagedOrderTransport
				Try
					stagedOrderTransportInfo = New KaStagedOrderTransport(connection, stagedOrderTransportId)
				Catch ex As Exception
					stagedOrderTransportInfo = New KaStagedOrderTransport
					stagedOrderTransportInfo.Id = stagedOrderTransportId
				End Try
				Dim txtStagedOrderTareWeight As TextBox = FindControl(transportRow.ID & "_txtStagedOrderTareWeight")
				Dim ddlTareWeightUofM As DropDownList = FindControl(transportRow.ID & "_ddlTareWeightUofM")
				Dim tbxTareDate As HtmlInputText = FindControl(transportRow.ID & "_tbxTransportTareDate")

				Dim originalTareWeight As HtmlInputHidden = FindControl(transportRow.ID & "_OriginalTareWeight")
				Dim originalTareDate As HtmlInputHidden = FindControl(transportRow.ID & "_OriginalTareDate")
				Dim originalTareWeightUofM As HtmlInputHidden = FindControl(transportRow.ID & "_OriginalTareWeightUofM")
				Dim originalTareManual As HtmlInputHidden = FindControl(transportRow.ID & "_OriginalTareManual")

				With stagedOrderTransportInfo
					.Deleted = False
					.StagedOrderId = _stagedOrderInfo.Id
					Guid.TryParse(CType(transportRow.FindControl(transportRow.ID & "_ddlTransport"), DropDownList).SelectedValue, .TransportId)
					Double.TryParse(txtStagedOrderTareWeight.Text, .TareWeight)
					Guid.TryParse(ddlTareWeightUofM.SelectedValue, .TareUnitId)
					Dim tareDate As DateTime = Now
					If tbxTareDate.Value.Trim.Length > 0 AndAlso DateTime.TryParse(tbxTareDate.Value, tareDate) AndAlso tareDate > New DateTime(1900, 1, 1) Then
						.TaredAt = tareDate
						'Else
						'    .TaredAt = Now
					End If
					.TareManual = Boolean.Parse(originalTareManual.Value) OrElse Date.Parse(originalTareDate.Value) <> .TaredAt OrElse originalTareWeight.Value <> .TareWeight OrElse originalTareWeightUofM.Value <> ddlTareWeightUofM.SelectedValue
				End With
				transportList.Add(stagedOrderTransportInfo)
				ConvertTransportCompartmentToObject(compartmentList, transportRow.FindControl(transportRow.ID & "_compTable"), stagedOrderTransportId)
			End If
		Next
	End Sub

	Private Sub ConvertTransportCompartmentToObject(ByRef compartmentList As List(Of KaStagedOrderCompartment), ByVal transportCompartmentTable As HtmlGenericControl, ByVal stagedOrderTransportId As Guid)
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		For Each listObject As Object In transportCompartmentTable.Controls
			If Not TypeOf listObject Is HtmlGenericControl Then Continue For
			Dim compartmentRow As HtmlGenericControl = listObject
			If compartmentRow.ID IsNot Nothing AndAlso compartmentRow.ID.Contains(transportCompartmentTable.ID & "_CompRow") Then
				' This is a compartment row
				Dim rowNumber As Integer = CInt(CType(compartmentRow.FindControl(compartmentRow.ID & "_RowNumber"), HtmlInputHidden).Value)
				Dim stagedOrderCompId As Guid = Guid.NewGuid
				Guid.TryParse(CType(compartmentRow.FindControl(compartmentRow.ID & "_Id"), HtmlInputHidden).Value, stagedOrderCompId)

				Dim transportCompartmentId As Guid = Guid.Empty
				Guid.TryParse(CType(compartmentRow.FindControl(compartmentRow.ID & "_ddlTransportCompartment"), DropDownList).SelectedValue.ToString(), transportCompartmentId)

				Dim stagedOrderCompInfo As KaStagedOrderCompartment
				Try
					stagedOrderCompInfo = New KaStagedOrderCompartment(connection, stagedOrderCompId)
				Catch ex As Exception
					stagedOrderCompInfo = New KaStagedOrderCompartment
					stagedOrderCompInfo.Id = stagedOrderCompId
				End Try
				With stagedOrderCompInfo
					.Deleted = False
					.StagedOrderId = _stagedOrderInfo.Id
					.StagedOrderTransportId = stagedOrderTransportId
					.TransportCompartmentId = transportCompartmentId

					.CompartmentItems.Clear()
				End With
				ConvertCompartmentItemsToObject(stagedOrderCompInfo, compartmentRow.FindControl(compartmentRow.ID & "_ProdTable"))

				compartmentList.Add(stagedOrderCompInfo)
			End If
		Next
	End Sub

	Private Sub ConvertCompartmentItemsToObject(ByRef stagedOrderCompInfo As KaStagedOrderCompartment, compartmentItemTable As HtmlGenericControl)
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		For Each listObject As Object In compartmentItemTable.Controls
			If Not TypeOf listObject Is HtmlGenericControl Then Continue For
			Dim compProdRow As HtmlGenericControl = listObject
			If compProdRow.ID IsNot Nothing AndAlso compProdRow.ID.Contains(compartmentItemTable.ID & "_ItemRow") Then
				' This is a compartment row
				Dim rowNumber As Integer = CInt(CType(compProdRow.FindControl(compProdRow.ID & "_RowNumber"), HtmlInputHidden).Value)
				Dim stagedOrderCompId As Guid = Guid.NewGuid
				Guid.TryParse(CType(compProdRow.FindControl(compProdRow.ID & "_Id"), HtmlInputHidden).Value, stagedOrderCompId)
				Dim stagedOrderCompItemInfo As KaStagedOrderCompartmentItem
				Try
					stagedOrderCompItemInfo = New KaStagedOrderCompartmentItem(connection, stagedOrderCompId)
				Catch ex As Exception
					stagedOrderCompItemInfo = New KaStagedOrderCompartmentItem
					stagedOrderCompItemInfo.Id = stagedOrderCompId
				End Try
				With stagedOrderCompItemInfo
					.Deleted = False
					Guid.TryParse(CType(compProdRow.FindControl(compProdRow.ID & "_ddlProduct" & rowNumber.ToString), DropDownList).SelectedValue, .OrderItemId)
					Double.TryParse(CType(compProdRow.FindControl(compProdRow.ID & "_tbxProductAmount" & rowNumber.ToString), TextBox).Text, .Quantity)
					.StagedOrderCompartmentId = stagedOrderCompInfo.Id
					Guid.TryParse(CType(compProdRow.FindControl(compProdRow.ID & "_ddlUnits" & rowNumber.ToString), DropDownList).SelectedValue, .UnitId)
					.Position = stagedOrderCompInfo.CompartmentItems.Count
				End With

				stagedOrderCompInfo.CompartmentItems.Add(stagedOrderCompItemInfo)
			End If
		Next
	End Sub

	Private Sub ConvertToOriginalTareWeights(ByRef originalTareList As List(Of OriginalTareInfo))
		For Each stagedObject As Object In pnlStagedTransports.Controls
			If Not TypeOf stagedObject Is HtmlGenericControl Then Continue For
			Dim transportRow As HtmlGenericControl = stagedObject
			If transportRow.ID IsNot Nothing AndAlso transportRow.ID.Contains("Transport") Then
				' This is a transport row
				Dim rowNumber As Integer = CInt(CType(transportRow.FindControl(transportRow.ID & "_RowNumber"), HtmlInputHidden).Value)
				Dim stagedOrderTransportId As Guid = Guid.NewGuid
				Guid.TryParse(CType(transportRow.FindControl(transportRow.ID & "_Id"), HtmlInputHidden).Value, stagedOrderTransportId)
				Dim originalTareWeight As HtmlInputHidden = FindControl(transportRow.ID & "_OriginalTareWeight")
				Dim originalTareDate As HtmlInputHidden = FindControl(transportRow.ID & "_OriginalTareDate")
				Dim originalTareWeightUofM As HtmlInputHidden = FindControl(transportRow.ID & "_OriginalTareWeightUofM")
				Dim originalTareManual As HtmlInputHidden = FindControl(transportRow.ID & "_OriginalTareManual")
				Dim originalTareInfo As New OriginalTareInfo
				With originalTareInfo
					.StagedTransportId = stagedOrderTransportId
					.OriginalTareInfo = New KaTimestampedQuantity(originalTareWeight.Value, Guid.Parse(originalTareWeightUofM.Value), originalTareDate.Value, originalTareManual.Value)
				End With
				originalTareList.Add(originalTareInfo)
			End If
		Next
	End Sub

#End Region

#Region " Add Transport and Compartment Rows "
	Private Sub CreateDynamicControls()
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		_currentOrderItemIds.Clear()
		For Each compartment As KaStagedOrderCompartment In _stagedOrderInfo.Compartments
			If Not compartment.Deleted Then
				For Each item As KaStagedOrderCompartmentItem In compartment.CompartmentItems
					If Not item.Deleted AndAlso Not item.OrderItemId.Equals(Guid.Empty) Then
						_currentOrderItemIds.Add(item.OrderItemId)
					End If
				Next
			End If
		Next
		Dim orderInfo As List(Of KaOrder) = GetOrderInfo(_stagedOrderInfo.Orders)

		pnlStagedTransports.Controls.Clear()
		Dim transportCount As Integer = 0
		For Each stagedTransport As KaStagedOrderTransport In _stagedOrderInfo.Transports
			If Not stagedTransport.Deleted Then
				transportCount += 1
				AddStagedTransport(stagedTransport, _stagedOrderInfo.Compartments, transportCount, orderInfo)
			End If
		Next
		If transportCount = 0 Then
			Dim stagedTransport As New KaStagedOrderTransport
			With stagedTransport
				.Id = Guid.NewGuid
				.TransportId = Guid.Empty
			End With
			transportCount += 1
			AddStagedTransport(stagedTransport, _stagedOrderInfo.Compartments, transportCount, orderInfo)
		End If

		AddNewTransportButton()

		' Add shortcuts
		Dim c As OleDbConnection = GetUserConnection(Utilities.GetUser(Me).Id)
		Dim selectedShortcuts As List(Of Guid)
		Try
			selectedShortcuts = Tm2Database.FromXml(KaSetting.GetSetting(c, "StagedOrder/ShortcutsAvailable", ""), GetType(List(Of Guid)))
		Catch ex As Exception
			selectedShortcuts = New List(Of Guid)
		End Try
		Dim defaultWeightUnitId As Guid = KaUnit.GetSystemDefaultMassUnitOfMeasure(connection, Nothing)
		Dim customShortcutRow As HtmlGenericControl = Nothing
		Dim shortcutsPerRow As Integer = pnlShortcuts.Controls(0).Controls.Count
		For Each shortcutId As Guid In selectedShortcuts
			Try
				Dim shortcut As New KaCustomPages(c, shortcutId)
				Dim buttonId As String = "btnShortcut" & shortcut.Id.ToString()
				If Page.FindControl(buttonId) IsNot Nothing Then Continue For
				If customShortcutRow Is Nothing OrElse customShortcutRow.Controls.Count >= 3 Then
					customShortcutRow = New HtmlGenericControl("li")
					pnlShortcuts.Controls.Add(customShortcutRow)
				End If
				Dim shortcutCell As New HtmlGenericControl("div")
				With shortcutCell
					.Attributes("class") = "shortcutPanel"
				End With
				If shortcut.CustomShortcutPrompt = KaCustomPages.CustomShortcutPromptType.QuantityWithUnit Then
					'   shortcutCell.Controls.Add(New HtmlGenericControl("br"))
					Dim tbxQuantity As New TextBox
					With tbxQuantity
						.ID = "tbxQuantity" & shortcut.Id.ToString()
						.Attributes("Style") = "width: 40%; text-align: right;"
						.Text = "0"
					End With
					shortcutCell.Controls.Add(tbxQuantity)
					Dim ddlQuantityUnit As New DropDownList
					With ddlQuantityUnit
						.ID = "ddlQuantityUnit" & shortcut.Id.ToString()
						.Attributes("Style") = "width: auto; max-width: 50%; min-width: 10%;"
						.Text = "0"
					End With
					PopulateUnitOfMeasureList(ddlQuantityUnit, defaultWeightUnitId, True)
					shortcutCell.Controls.Add(ddlQuantityUnit)
				End If
				Dim shortcutButton As New Button
				With shortcutButton
					.Text = shortcut.PageLabel
					.ID = buttonId
					AddHandler .Click, AddressOf CustomShortcutClicked
					.Attributes("align") = "center"
					.Style("Width") = "95%"
				End With
				shortcutCell.Controls.Add(shortcutButton)
				customShortcutRow.Controls.Add(shortcutCell)
			Catch ex As RecordNotFoundException

			End Try
		Next
	End Sub

	Private Sub AddStagedTransport(ByVal stagedTransportInfo As KaStagedOrderTransport, ByVal compartmentList As List(Of KaStagedOrderCompartment), ByVal itemCounter As Integer, ByVal orderInfoList As List(Of KaOrder))
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim transportInfo As KaTransport
		Try
			transportInfo = New KaTransport(connection, stagedTransportInfo.TransportId)
		Catch ex As Exception
			transportInfo = New KaTransport
		End Try
		Dim newTransportRow As New HtmlGenericControl("li")
		With newTransportRow
			.ID = "Transport" & itemCounter.ToString
		End With
		pnlStagedTransports.Controls.Add(newTransportRow)

		Dim stagedTransportId As New HtmlInputHidden
		With stagedTransportId
			.Attributes("type") = "hidden"
			.ID = newTransportRow.ID & "_Id"
			.Value = stagedTransportInfo.Id.ToString
			.Style.Item("padding-top") = "0px"
		End With
		newTransportRow.Controls.Add(stagedTransportId)
		Dim rowNumber As New HtmlInputHidden
		With rowNumber
			.Attributes("type") = "hidden"
			.ID = newTransportRow.ID & "_RowNumber"
			.Value = itemCounter.ToString
			.Style.Item("padding-top") = "0px"
		End With
		newTransportRow.Controls.Add(rowNumber)

		Dim removeTransButton As New LinkButton
		With removeTransButton
			.CssClass = "button"
			.ID = newTransportRow.ID & "_btnRemove"
			.Text = "x"
			.ToolTip = "Remove transport"
			.Attributes("Style") = "vertical-align: top;"
			AddHandler .Click, AddressOf RemoveTransportClicked
		End With
		newTransportRow.Controls.Add(removeTransButton)
		Dim transportLabel As New Label
		With transportLabel
			.Text = "Transport"
			.Attributes("style") = "width: auto; font-weight: bold; vertical-align: top;"
		End With
		newTransportRow.Controls.Add(transportLabel)

		Dim transportDiv As New HtmlGenericControl("div")
		With transportDiv
			.Attributes("class") = "input"
			.Attributes("style") = "width: 80%; vertical-align: top;"
		End With
		newTransportRow.Controls.Add(transportDiv)

		Dim transportDropdownList As New DropDownList
		With transportDropdownList
			.ID = newTransportRow.ID & "_ddlTransport"
			.AutoPostBack = True
			.Attributes("Style") = "width: auto; min-width: 20%; vertical-align: top;"
			PopulateTransportsList(transportDropdownList, stagedTransportInfo.TransportId)
			AddHandler transportDropdownList.SelectedIndexChanged, AddressOf TransportChanged
		End With
		transportDiv.Controls.Add(transportDropdownList)

		Dim transportDivPanel As New HtmlGenericControl("ul")
		With transportDivPanel
			.ID = newTransportRow.ID & "_compTable"
		End With
		transportDiv.Controls.Add(transportDivPanel)

		Dim tareInfoPanel As New HtmlGenericControl("li")
		transportDivPanel.Controls.Add(tareInfoPanel)

		Dim tareLabel As New HtmlGenericControl("label")
		With tareLabel
			.Attributes("style") = "width: 15%; vertical-align:center;"
			If _stagedOrderInfo.LossInWeight Then
				.InnerText = "Gross (outbound) weight"
			Else
				.InnerText = "Tare weight"
			End If
		End With
		tareInfoPanel.Controls.Add(tareLabel)

		Dim tareInfoDiv As New HtmlGenericControl("div")
		With tareInfoDiv
			.Attributes("class") = "input"
			.Attributes("style") = "width: 80%; display: inline-block;"
		End With
		tareInfoPanel.Controls.Add(tareInfoDiv)

		If Not _stagedOrderInfo.LossInWeight Then
			Dim newTransportTareInfoLabel As New Label
			With newTransportTareInfoLabel
				.ID = newTransportRow.ID & "_TareInfo"
				Try
					Dim uofMInfo As New KaUnit(connection, transportInfo.UnitId)
					.Text = "<ul class=""input"" style=""width: auto; vertical-align:center;""><li>" & Format(transportInfo.TareWeight, uofMInfo.UnitPrecision) & " " & uofMInfo.Abbreviation & "</li>" &
						IIf(transportInfo.TaredAt > New DateTime(1900, 1, 1), "<li>" & transportInfo.TaredAt & "</li>", "") & "</ul>"
				Catch ex As RecordNotFoundException
					.Text = ""
				End Try
				.Visible = (transportInfo.TareWeight <> 0.0 OrElse transportInfo.TaredAt > New DateTime(1900, 1, 1)) AndAlso .Text.Length > 0
				.CssClass = "label"
				.Attributes("Style") = "width: auto;"
			End With
			tareInfoDiv.Controls.Add(newTransportTareInfoLabel)

			Dim newTransportAssignTareButton As New LinkButton
			With newTransportAssignTareButton
				.ID = newTransportRow.ID & "_btnAssignTare"
				.Text = "r" '
				.ToolTip = "Assign Tare" '  "Assign Tare ->"
				.CssClass = "button"
				.Attributes("Style") = "width:auto;"
				.Visible = (transportInfo.TareWeight <> 0.0 OrElse transportInfo.TaredAt > New DateTime(1900, 1, 1)) AndAlso newTransportTareInfoLabel.Text.Length > 0
			End With
			AddHandler newTransportAssignTareButton.Click, AddressOf TransportAssignTareButtonClicked
			tareInfoDiv.Controls.Add(newTransportAssignTareButton)
		End If

		Dim stagedTareInfoPanel As New HtmlGenericControl("ul")
		With stagedTareInfoPanel
			.Attributes("class") = "input"
			.Attributes("Style") = "vertical-align: top;"
		End With
		tareInfoDiv.Controls.Add(stagedTareInfoPanel)

		Dim stagedTareWeightPanel As New HtmlGenericControl("li")
		stagedTareInfoPanel.Controls.Add(stagedTareWeightPanel)

		Dim txtStagedOrderTareWeight As New TextBox
		Dim ddlTareWeightUofM As New DropDownList
		Dim originalTareWeight As New HtmlInputHidden
		Dim originalTareWeightUofM As New HtmlInputHidden
		Dim originalTareManual As New HtmlInputHidden

		' Add Staged Transport Tare Info 
		With txtStagedOrderTareWeight
			.ID = newTransportRow.ID & "_txtStagedOrderTareWeight"
			.Attributes("Style") = "width: 40%; text-align: right;"
			.AutoPostBack = True
		End With
		With ddlTareWeightUofM
			.ID = newTransportRow.ID & "_ddlTareWeightUofM"
			.Attributes("Style") = "width:auto;"
			.AutoPostBack = True
		End With

		originalTareWeight.ID = newTransportRow.ID & "_OriginalTareWeight"
		originalTareWeightUofM.ID = newTransportRow.ID & "_OriginalTareWeightUofM"
		originalTareManual.ID = newTransportRow.ID & "_OriginalTareManual"

		stagedTareWeightPanel.Controls.Add(txtStagedOrderTareWeight)
		stagedTareWeightPanel.Controls.Add(ddlTareWeightUofM)
		stagedTareWeightPanel.Controls.Add(originalTareWeight)
		stagedTareWeightPanel.Controls.Add(originalTareWeightUofM)
		stagedTareWeightPanel.Controls.Add(originalTareManual)

		Dim stagedTareDatePanel As New HtmlGenericControl("li")
		stagedTareInfoPanel.Controls.Add(stagedTareDatePanel)

		Dim tbxTareDate As New HtmlInputText
		Dim originalTareDate As New HtmlInputHidden
		originalTareDate.ID = newTransportRow.ID & "_OriginalTareDate"
		stagedTareDatePanel.Controls.Add(tbxTareDate)
		stagedTareDatePanel.Controls.Add(originalTareDate)

		With tbxTareDate
			'.Name = newTransportRow.ID & "_tbxTransportTareDate"
			.ID = newTransportRow.ID & "_tbxTransportTareDate"
			.Attributes("class") = "input"
			'.AutoPostBack = True
			.Attributes("Style") = "width: 60%; min-width: 15em;"
		End With
		If _currentUser IsNot Nothing Then DisplayJavaScriptMessage(String.Format("{0}_tbxTransportTareDateScript", newTransportRow.ID), "<script type=""text/javascript"">$('#" & newTransportRow.ID & "_tbxTransportTareDate').datetimepicker({ " &
						   "timeFormat: 'h:mm:ss TT', " &
						   "showSecond: true, " &
						   "showOn: ""both"", " &
						   "buttonImage: 'Images/Calendar_scheduleHS.png'," &
						   "buttonImageOnly: true," &
						   "buttonText: ""Show calendar""});</script>", False)

		' Add this as a startup script for the first transport. 
		If itemCounter = 1 Then
			Dim csname As [String] = newTransportRow.ID & "_tbxTransportTareDateStartupScript"
			Dim cstype As Type = Me.[GetType]()

			' Get a ClientScriptManager reference from the Page class. 
			Dim cs As ClientScriptManager = Page.ClientScript

			' Check to see if the startup script is already registered. 
			If Not cs.IsStartupScriptRegistered(cstype, csname) Then
				Dim cstext1 As New StringBuilder()
				Dim maxdate As DateTime = Now
				If maxdate < stagedTransportInfo.TaredAt Then maxdate = stagedTransportInfo.TaredAt
				cstext1.Append("<script type=""text/javascript"">$('#" & newTransportRow.ID & "_tbxTransportTareDate').datetimepicker({ " &
							   "timeFormat:  'h:mm:ss TT', " &
							   "showSecond: true, " &
							   "showOn: ""both"", " &
							   "buttonImage:    'Images/Calendar_scheduleHS.png'," &
							   "buttonImageOnly: true," &
							   "buttonText: ""Show calendar""," &
							   "maxDate: new Date(" &
							   (maxdate.Year).ToString & "," & (maxdate.Month - 1).ToString & "," & (maxdate.Day).ToString & "," &
							   (maxdate.Hour).ToString & "," & (maxdate.Minute).ToString & "," & (maxdate.Second).ToString & ") " &
							   "});</script>") ' this will set the max date to now.  Note - the month is 0 based, so needs to be adjusted (jQuery UI bug?)

				cs.RegisterStartupScript(Me.[GetType](), csname, cstext1.ToString())
			End If
		End If

		txtStagedOrderTareWeight.Text = stagedTransportInfo.TareWeight
		If stagedTransportInfo.TareUnitId.Equals(Guid.Empty) Then stagedTransportInfo.TareUnitId = KaUnit.GetSystemDefaultMassUnitOfMeasure(connection, Nothing)
		PopulateUnitOfMeasureList(ddlTareWeightUofM, stagedTransportInfo.TareUnitId, False)
		If stagedTransportInfo.TaredAt > New DateTime(1900, 1, 1) Then
			tbxTareDate.Value = stagedTransportInfo.TaredAt
		Else
			tbxTareDate.Value = ""
		End If
		Dim originalTareFound As Boolean = False
		For Each originalTare As OriginalTareInfo In _originalTareValues
			If originalTare.StagedTransportId.Equals(stagedTransportInfo.Id) Then
				originalTareWeightUofM.Value = originalTare.OriginalTareInfo.UnitId.ToString
				originalTareWeight.Value = originalTare.OriginalTareInfo.Numeric
				originalTareDate.Value = String.Format("{0:G}", originalTare.OriginalTareInfo.Timestamp)
				originalTareManual.Value = originalTare.OriginalTareInfo.Manual
				originalTareFound = True
				Exit For
			End If
		Next
		If Not originalTareFound Then
			originalTareWeightUofM.Value = stagedTransportInfo.TareUnitId.ToString
			originalTareWeight.Value = stagedTransportInfo.TareWeight
			originalTareDate.Value = String.Format("{0:G}", stagedTransportInfo.TaredAt)
			originalTareManual.Value = stagedTransportInfo.TareManual
		End If
		originalTareWeightUofM.Attributes("type") = "hidden"
		originalTareWeight.Attributes("type") = "hidden"
		originalTareDate.Attributes("type") = "hidden"
		originalTareManual.Attributes("type") = "hidden"

		'ToDo: Add Compartment Totals Row
		'        Dim transportSummaryRow As New HtmlTableRow
		'        transportSummaryRow.Style.Item("padding-top") = "0px"
		'        transportSummaryRow.Style.Item("font-weight") = "bold"
		'        transportCompartmentTable.Rows.Add(transportSummaryRow)

		'        Dim transportCompartmentBlankCell As New HtmlTableCell
		'        With transportCompartmentBlankCell
		'            .InnerText = "Total: "
		'            .ColSpan = 2
		'        End With
		'        transportSummaryRow.Controls.Add(transportCompartmentBlankCell)

		'        transportSummaryRow.Controls.Add(transportTotalCell)
		'        Dim newCompartmentEmptyCell As New HtmlTableCell
		'        With newCompartmentEmptyCell
		'            .InnerText = " "
		'        End With
		'        transportSummaryRow.Cells.Add(newCompartmentEmptyCell)

		'        Dim newCompartmentAddCell As New HtmlTableCell
		'        With newCompartmentAddCell
		'        End With
		'        transportSummaryRow.Cells.Add(newCompartmentAddCell)

		'        Dim newCompartmentAddButton As New Button
		'        With newCompartmentAddButton
		'            .ID = transportRow.ID & "_btnAddCompartment" & itemCounter.ToString
		'            .Text = "Add Compartment"
		'            .Attributes("Style") = "width:auto;"
		'        End With
		'        AddHandler newCompartmentAddButton.Click, AddressOf AddCompartmentClicked
		'        newCompartmentAddCell.Controls.Add(newCompartmentAddButton)

		Dim compartmentCount As Integer = 0
		For Each compartmentInfo As KaStagedOrderCompartment In compartmentList
			If Not compartmentInfo.Deleted AndAlso compartmentInfo.StagedOrderTransportId.Equals(stagedTransportInfo.Id) Then
				compartmentCount += 1
				AddStagedCompartment(connection, transportDivPanel, compartmentInfo, compartmentCount, orderInfoList, stagedTransportInfo.TransportId)
			End If
		Next
		If compartmentCount = 0 Then
			Dim newCompInfo As New KaStagedOrderCompartment
			With newCompInfo
				.Id = Guid.NewGuid
				.Deleted = False
				.Position = 0
				.StagedOrderTransportId = stagedTransportInfo.Id
			End With
			compartmentCount += 1
			AddStagedCompartment(connection, transportDivPanel, newCompInfo, compartmentCount, orderInfoList, stagedTransportInfo.TransportId)
		End If

		' Add the Add New Compartment Button 
		Dim newCompartmentAddCompRow As New HtmlGenericControl("li")
		newTransportRow.Controls.Add(newCompartmentAddCompRow)

		Dim newCompartmentAddCompButton As New LinkButton
		With newCompartmentAddCompButton
			.ID = newTransportRow.ID & "_btnAddCompartment"
			.CssClass = "button"
			.Text = "+"
			.ToolTip = "Add compartment"
		End With
		AddHandler newCompartmentAddCompButton.Click, AddressOf AddCompartmentClicked
		transportDivPanel.Controls.Add(newCompartmentAddCompButton)

		Dim addCompartmentLabel As New HtmlGenericControl("label")
		With addCompartmentLabel
			.Attributes("style") = "text-align: left;"
			.InnerText = "Add compartment"
		End With
		transportDivPanel.Controls.Add(addCompartmentLabel)

		Dim transportTotalCell As New HtmlGenericControl("li")
		With transportTotalCell
			.Attributes("style") = "width: 100%; text-align: center; font-weight: bold;"
			.ID = newTransportRow.ID & "_Total"
		End With
		newTransportRow.Controls.Add(transportTotalCell)
	End Sub

	Private Sub AddStagedCompartment(connection As OleDbConnection, ByRef transportDivPanel As HtmlGenericControl, compartmentInfo As KaStagedOrderCompartment, ByVal transportItemCounter As Integer, ByVal orderInfo As List(Of KaOrder), ByVal transportId As Guid)

		Dim newCompartmentRow As New HtmlGenericControl("li")
		newCompartmentRow.ID = transportDivPanel.ID & "_CompRow" & transportItemCounter.ToString
		transportDivPanel.Controls.Add(newCompartmentRow)

		Dim stagedTransportId As New HtmlInputHidden
		With stagedTransportId
			.Attributes("type") = "hidden"
			.ID = newCompartmentRow.ID & "_Id"
			.Value = compartmentInfo.Id.ToString
			.Style.Item("padding-top") = "0px"
		End With
		transportDivPanel.Controls.Add(stagedTransportId)

		Dim rowNumber As New HtmlInputHidden
		With rowNumber
			.Attributes("type") = "hidden"
			.ID = newCompartmentRow.ID & "_RowNumber"
			.Value = transportItemCounter.ToString
			.Style.Item("padding-top") = "0px"
		End With
		transportDivPanel.Controls.Add(rowNumber)

		Dim newCompartmentRemoveButton As New LinkButton
		With newCompartmentRemoveButton
			.ID = newCompartmentRow.ID & "_btnRemove" & transportItemCounter.ToString
			.CssClass = "button"
			.Text = "x"
			.ToolTip = "Remove compartment"
		End With
		AddHandler newCompartmentRemoveButton.Click, AddressOf RemoveCompartmentClicked
		newCompartmentRow.Controls.Add(newCompartmentRemoveButton)

		Dim marginLeft As String = "margin-left: 0.1em;"
		If transportItemCounter > 1 Then
			Dim newMoveCompartmentUpButton As New LinkButton
			With newMoveCompartmentUpButton
				.Attributes("Style") = marginLeft
				.ID = newCompartmentRow.ID & "_btnMoveUp" & transportItemCounter.ToString
				.CssClass = "button"
				.Text = "u"
				.ToolTip = "Move compartment up"
			End With
			AddHandler newMoveCompartmentUpButton.Click, AddressOf MoveCompartmentUp
			newCompartmentRow.Controls.Add(newMoveCompartmentUpButton)
		Else
			marginLeft = "margin-left: 1.5em;"
		End If
		Dim compartmentsInTransport As Integer = 0
		For Each compartment As KaStagedOrderCompartment In _stagedOrderInfo.Compartments
			If compartment.StagedOrderTransportId.Equals(compartmentInfo.StagedOrderTransportId) Then compartmentsInTransport += 1
		Next
		If transportItemCounter < compartmentsInTransport Then
			Dim newMoveCompartmentDownButton As New LinkButton
			With newMoveCompartmentDownButton
				.Attributes("Style") = marginLeft
				.ID = newCompartmentRow.ID & "_btnMoveDown" & transportItemCounter.ToString
				.CssClass = "button"
				.Text = "d"
				.ToolTip = "Move compartment down"
			End With
			AddHandler newMoveCompartmentDownButton.Click, AddressOf MoveCompartmentDown
			newCompartmentRow.Controls.Add(newMoveCompartmentDownButton)
		End If
		Dim compLabel As New HtmlGenericControl("label")
		With compLabel
			.Attributes("Style") = "width:15%;"
		End With
		newCompartmentRow.Controls.Add(compLabel)

		Dim newCompNumberCell As New Label
		With newCompNumberCell
			.ID = newCompartmentRow.ID & "_CompNumber"
			.Text = "Compartment " & transportItemCounter.ToString() & ": "
			.Attributes("Style") = "font-weight: bold;"
		End With
		compLabel.Controls.Add(newCompNumberCell)

		' Add the Transport Compartment Dropdown list
		Dim transportCompartmentDropdownList As New DropDownList
		With transportCompartmentDropdownList
			.ID = newCompartmentRow.ID & "_ddlTransportCompartment"
			.AutoPostBack = True
			.Attributes("Style") = "width:auto; padding-top: 0px;"
		End With
		PopulateTransportCompartmentList(transportCompartmentDropdownList, compartmentInfo.TransportCompartmentId, transportId)
		newCompartmentRow.Controls.Add(transportCompartmentDropdownList)

		Dim newCompProdTable As New HtmlGenericControl("ul")
		With newCompProdTable
			.ID = newCompartmentRow.ID & "_ProdTable"
		End With
		newCompartmentRow.Controls.Add(newCompProdTable)

		Dim itemCount As Integer = 0
		For Each compartmentItem As KaStagedOrderCompartmentItem In compartmentInfo.CompartmentItems
			With compartmentItem
				If Not .Deleted Then
					itemCount += 1
					AddStagedCompartmentItem(compartmentItem, newCompProdTable, orderInfo, itemCount)
				End If
			End With
		Next

		If itemCount = 0 Then
			Dim compartmentItem As New KaStagedOrderCompartmentItem
			With compartmentItem
				.Id = Guid.NewGuid
				.Deleted = False
				.Position = 0
				.OrderItemId = Guid.Empty
				.UnitId = KaUnit.GetSystemDefaultMassUnitOfMeasure(connection, Nothing)
			End With
			itemCount += 1
			AddStagedCompartmentItem(compartmentItem, newCompProdTable, orderInfo, itemCount)
		End If

		' Add the Add New Product Button 
		Dim newCompartmentAddProdRow As New HtmlGenericControl("li")
		newCompProdTable.Controls.Add(newCompartmentAddProdRow)

		Dim addProdSection As New HtmlGenericControl("label")
		With addProdSection
			.Attributes("Style") = "width: 15%;"
		End With
		newCompartmentAddProdRow.Controls.Add(addProdSection)

		Dim newCompartmentAddProdButton As New LinkButton
		With newCompartmentAddProdButton
			.ID = newCompartmentRow.ID & "_btnAddProd" & transportItemCounter.ToString
			.CssClass = "button"
			.Text = "+"
			.ToolTip = "Add product"
		End With
		AddHandler newCompartmentAddProdButton.Click, AddressOf AddCompartmentItemClicked
		addProdSection.Controls.Add(newCompartmentAddProdButton)

		Dim addProdLabel As New HtmlGenericControl("label")
		With addProdLabel
			.Attributes("style") = "text-align: left;"
			.InnerText = "Add product"
		End With
		addProdSection.Controls.Add(addProdLabel)
	End Sub

	Private Sub AddStagedCompartmentItem(ByVal compartmentItem As KaStagedOrderCompartmentItem, ByRef newCompProdTable As HtmlGenericControl, ByVal orderInfoList As List(Of KaOrder), itemCounter As Integer)
		Dim newCompProdRow As New HtmlGenericControl("li")
		newCompProdRow.ID = newCompProdTable.ID & "_ItemRow" & itemCounter.ToString
		newCompProdTable.Controls.Add(newCompProdRow)
		Dim stagedTransportId As New HtmlInputHidden
		With stagedTransportId
			.Attributes("type") = "hidden"
			.ID = newCompProdRow.ID & "_Id"
			.Value = compartmentItem.Id.ToString
			.Style.Item("padding-top") = "0px"
		End With
		newCompProdRow.Controls.Add(stagedTransportId)
		Dim rowNumber As New HtmlInputHidden
		With rowNumber
			.Attributes("type") = "hidden"
			.ID = newCompProdRow.ID & "_RowNumber"
			.Value = itemCounter.ToString
			.Style.Item("padding-top") = "0px"
		End With
		newCompProdRow.Controls.Add(rowNumber)

		Dim productLabel As New HtmlGenericControl("label")
		With productLabel
			.Attributes("Style") = "width: 15%;"
		End With
		newCompProdRow.Controls.Add(productLabel)

		Dim newProductRemoveButton As New LinkButton
		With newProductRemoveButton
			.ID = newCompProdRow.ID & "_btnRemove" & itemCounter.ToString
			.CssClass = "button"
			.Text = "x"
			.ToolTip = "Remove product"
		End With
		AddHandler newProductRemoveButton.Click, AddressOf RemoveCompartmentItemClicked
		productLabel.Controls.Add(newProductRemoveButton)

		'Set up the product cell
		Dim productPanel As New HtmlGenericControl("span")
		With productPanel
			.Attributes("class") = "input"
			.Attributes("style") = "width: 80%; display: inline-block;"
		End With
		newCompProdRow.Controls.Add(productPanel)
		Dim newProductList As New DropDownList
		With newProductList
			.ID = newCompProdRow.ID & "_ddlProduct" & itemCounter.ToString
			.AutoPostBack = True
			.Attributes("Style") = "width:auto; min-width: 30px;"
		End With
		PopulateProductList(newProductList, orderInfoList, compartmentItem.OrderItemId)
		productPanel.Controls.Add(newProductList)
		AddHandler newProductList.SelectedIndexChanged, AddressOf CompartmentItemQuantityChanged

		Dim newProductRequested As New Label
		newProductRequested.Text = "Requested"
		productPanel.Controls.Add(newProductRequested)

		Dim newProductAmount As New TextBox
		With newProductAmount
			.ID = newCompProdRow.ID & "_tbxProductAmount" & itemCounter.ToString
			.Attributes("Style") = "width:auto; text-align:right; min-width:30px;"
			.AutoPostBack = True
			.Text = compartmentItem.Quantity
		End With
		AddHandler newProductAmount.TextChanged, AddressOf CompartmentItemQuantityChanged
		Dim prodAmountSpan As New HtmlGenericControl("span")
		prodAmountSpan.Attributes("class") = "required"
		productPanel.Controls.Add(prodAmountSpan)
		prodAmountSpan.Controls.Add(newProductAmount)

		Dim newProductUnitList As New DropDownList
		With newProductUnitList
			.ID = newCompProdRow.ID & "_ddlUnits" & itemCounter.ToString
			.AutoPostBack = True
			.Attributes("Style") = "width:auto; min-width: 20px;"
		End With
		PopulateUnitOfMeasureList(newProductUnitList, compartmentItem.UnitId)
		AddHandler newProductUnitList.SelectedIndexChanged, AddressOf CompartmentItemQuantityChanged
		productPanel.Controls.Add(newProductUnitList)
	End Sub
#End Region

	Private Sub ddlFacility_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles ddlFacility.SelectedIndexChanged
		Guid.TryParse(ddlFacility.SelectedValue, _stagedOrderInfo.LocationId)
		CompartmentItemQuantityChanged(sender, e)
		PopulateOrdersList()
		PopulateBaysList()
	End Sub

	Private Sub CompartmentItemQuantityChanged(sender As Object, e As System.EventArgs)
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim totalMass As Double = 0.0
		Dim totalMassValid As Boolean = True
		Dim orderInfoList As List(Of KaOrder) = GetOrderInfo()
		Dim locationId As Guid = _stagedOrderInfo.LocationId
		Dim massUnitOfMeasure As New KaUnit(connection, KaUnit.GetSystemDefaultMassUnitOfMeasure(connection, Nothing))
		Dim volumeUnitOfMeasure As New KaUnit(connection, KaUnit.GetSystemDefaultVolumeUnitOfMeasure(connection, Nothing))
		Dim lbsUnitOfMeasureId As Guid = KaUnit.GetUnitIdForBaseUnit(connection, KaUnit.Unit.Pounds)
		Dim tonsUnitOfMeasure As KaUnit = KaUnit.GetUnitForBaseUnit(connection, KaUnit.Unit.Tons)
		Dim massFormat As String = massUnitOfMeasure.UnitPrecision
		Dim tonsFormat As String = tonsUnitOfMeasure.UnitPrecision

		For Each stagedObject As Object In pnlStagedTransports.Controls
			If Not TypeOf stagedObject Is HtmlGenericControl Then Continue For
			Dim transportRow As HtmlGenericControl = stagedObject
			Dim transportMass As Double = 0.0
			Dim transportMassValid As Boolean = True
			Dim compTable As HtmlGenericControl = transportRow.FindControl(transportRow.ID & "_compTable")
			If compTable Is Nothing Then Continue For ' This is not a compartment row
			For Each listObject As Object In compTable.Controls
				If Not TypeOf listObject Is HtmlGenericControl Then Continue For
				Dim compartmentRow As HtmlGenericControl = listObject
				Dim compartmentMass As Double = 0.0
				Dim compartmentMassValid As Boolean = True
				Dim compProdTable As HtmlGenericControl = compartmentRow.FindControl(compartmentRow.ID & "_ProdTable")
				If compProdTable Is Nothing Then Continue For ' This is not a compartment product row
				For Each listObject2 As Object In compProdTable.Controls
					If Not TypeOf listObject2 Is HtmlGenericControl Then Continue For
					Dim compProdRow As HtmlGenericControl = listObject2
					Dim rowNumber As Integer
					Dim rowNumberCell As HtmlInputHidden = compProdRow.FindControl(compProdRow.ID & "_RowNumber")
					If rowNumberCell Is Nothing OrElse Not Integer.TryParse(rowNumberCell.Value, rowNumber) Then
						Continue For
					End If
					Dim productDropdown As DropDownList = compProdRow.FindControl(compProdRow.ID & "_ddlProduct" & rowNumber.ToString)
					Dim currentSelectedItem As Guid = Guid.Empty
					Guid.TryParse(productDropdown.SelectedValue, currentSelectedItem)
					Dim productAmount As Double = 0.0
					Double.TryParse(CType(compProdRow.FindControl(compProdRow.ID & "_tbxProductAmount" & rowNumber.ToString), TextBox).Text, productAmount)
					Dim prodUofMList As DropDownList = compProdRow.FindControl(compProdRow.ID & "_ddlUnits" & rowNumber.ToString)
					Dim currentSelectedUofM As Guid = Guid.Empty
					If Guid.TryParse(prodUofMList.SelectedValue, currentSelectedUofM) Then
						Dim density As Double = 0.0
						Dim densityWeight As Guid = massUnitOfMeasure.Id
						Dim densityVolume As Guid = volumeUnitOfMeasure.Id
						Dim compartmentUofM As New KaUnit(connection, currentSelectedUofM)
						Dim entryMass As Double = GetCompartmentQuantity(connection, currentSelectedItem, productAmount, orderInfoList, locationId, massUnitOfMeasure, massUnitOfMeasure, volumeUnitOfMeasure, compartmentUofM, compartmentMassValid)
						compartmentMass += entryMass
					Else
						compartmentMassValid = False
					End If
				Next
				If compartmentMassValid Then
					transportMass += compartmentMass
				Else
					transportMassValid = False
				End If
			Next
			Dim transportTotalCell As HtmlGenericControl = transportRow.FindControl(transportRow.ID & "_Total")
			If transportMassValid Then
				transportTotalCell.InnerText = "Transport total: " & Format(transportMass, massFormat) & " " & massUnitOfMeasure.Abbreviation & IIf(massUnitOfMeasure.Id.Equals(lbsUnitOfMeasureId), "   " & Format(transportMass / 2000.0, tonsFormat) & " " & tonsUnitOfMeasure.Abbreviation, "")
				totalMass += transportMass
			Else
				transportTotalCell.InnerText = "N/A"
				totalMassValid = False
			End If
		Next
		If totalMassValid Then
			lblStagedOrderTotals.Text = "Staged order total: " & Format(totalMass, massFormat) & " " & massUnitOfMeasure.Abbreviation & IIf(massUnitOfMeasure.Id.Equals(lbsUnitOfMeasureId), "   " & Format(totalMass / 2000.0, tonsFormat) & " " & tonsUnitOfMeasure.Abbreviation, "")
		Else
			lblStagedOrderTotals.Text = "N/A"
		End If

		RefreshOrderDetails(connection, orderInfoList)
	End Sub

	''' <summary>
	''' 
	''' </summary>
	''' <param name="userConn">Database OleDbConnection</param>
	''' <param name="currentSelectedItem">Order Item ID.  Can be GUID.Empty for all products.</param>
	''' <param name="productAmount">Amount of the Staged Order Item</param>
	''' <param name="orderInfoList">List of Orders</param>
	''' <param name="locationId"></param>
	''' <param name="toUnitOfMeasure">Unit of measure to convert to.</param>
	''' <param name="massUnitOfMeasure"></param>
	''' <param name="volumeUnitOfMeasure"></param>
	''' <param name="currentSelectedUnitOfMeasure">Unit of measure of the Staged Order Item</param>
	''' <param name="compartmentWeightValid">Reference variable that returns the unit conversion successful</param>
	''' <returns>The numeric amount of the individual Staged order item.</returns>
	''' <remarks></remarks>
	Private Shared Function GetCompartmentQuantity(ByVal userConn As OleDbConnection, ByVal currentSelectedItem As Guid, ByVal productAmount As Double, ByVal orderInfoList As List(Of KaOrder), ByVal locationId As Guid, ByVal toUnitOfMeasure As KaUnit, ByVal massUnitOfMeasure As KaUnit, ByVal volumeUnitOfMeasure As KaUnit, ByVal currentSelectedUnitOfMeasure As KaUnit, ByRef compartmentWeightValid As Boolean) As Double
		Dim compartmentWeight As Double = 0.0
		Dim density As Double = 0.0
		Dim densityWeight As Guid = massUnitOfMeasure.Id
		Dim densityVolume As Guid = volumeUnitOfMeasure.Id
		If currentSelectedItem.Equals(Guid.Empty) Then
			Dim orderTotalRequestedMass As Double = 0.0
			For Each orderInfo As KaOrder In orderInfoList
				Try
					orderTotalRequestedMass += orderInfo.GetRequested(locationId, massUnitOfMeasure.Id).Numeric
				Catch ex As UnitConversionException
					compartmentWeightValid = False
					Return 0.0
				End Try
			Next
			If Not compartmentWeightValid OrElse orderTotalRequestedMass <= 0.0 Then Return 0.0

			For Each orderInfo As KaOrder In orderInfoList
				For Each orderItemInfo As KaOrderItem In orderInfo.OrderItems
					If Not orderItemInfo.Deleted Then
						Try
							Dim productInfo As New KaProduct(userConn, orderItemInfo.ProductId)
							Dim densityInfo As KaRatio = productInfo.GetDensity(userConn, locationId)
							density = densityInfo.Numeric
							densityWeight = densityInfo.NumeratorUnitId
							densityVolume = densityInfo.DenominatorUnitId
							Dim weightUnitInfo As New KaUnit(userConn, densityWeight)
							Dim volumeUnitInfo As New KaUnit(userConn, densityVolume)
							Dim fromUnitInfo As New KaUnit(userConn, orderItemInfo.UnitId)
							Dim orderItemRequestedMass As Double = KaUnit.Convert(orderItemInfo.Request, fromUnitInfo, massUnitOfMeasure, density, weightUnitInfo, volumeUnitInfo)
							compartmentWeight += KaUnit.Convert(productAmount * (orderItemRequestedMass / orderTotalRequestedMass), currentSelectedUnitOfMeasure, toUnitOfMeasure, density, weightUnitInfo, volumeUnitInfo)
						Catch ex As RecordNotFoundException
							compartmentWeightValid = False
							Return 0.0
						Catch ex As UnitConversionException
							compartmentWeightValid = False
							Return 0.0
						End Try
					End If
				Next
			Next
		Else
			Try
				Dim orderInfo As New KaOrder(userConn, currentSelectedItem)
				Dim orderTotalRequestedMass As Double = orderInfo.GetRequested(locationId, massUnitOfMeasure.Id).Numeric
				For Each orderItemInfo As KaOrderItem In orderInfo.OrderItems
					If Not orderItemInfo.Deleted Then
						Try
							Dim productInfo As New KaProduct(userConn, orderItemInfo.ProductId)
							Dim densityInfo As KaRatio = productInfo.GetDensity(userConn, locationId)
							density = densityInfo.Numeric
							densityWeight = densityInfo.NumeratorUnitId
							densityVolume = densityInfo.DenominatorUnitId
							Dim weightUnitInfo As New KaUnit(userConn, densityWeight)
							Dim volumeUnitInfo As New KaUnit(userConn, densityVolume)
							Dim fromUnitInfo As New KaUnit(userConn, orderItemInfo.UnitId)
							Dim orderItemRequestedMass As Double = KaUnit.Convert(orderItemInfo.Request, fromUnitInfo, massUnitOfMeasure, density, weightUnitInfo, volumeUnitInfo)
							compartmentWeight += KaUnit.Convert(productAmount * (orderItemRequestedMass / orderTotalRequestedMass), currentSelectedUnitOfMeasure, toUnitOfMeasure, density, weightUnitInfo, volumeUnitInfo)
						Catch ex As RecordNotFoundException
							compartmentWeightValid = False
							Return 0.0
						Catch ex As UnitConversionException
							compartmentWeightValid = False
							Return 0.0
						End Try
					End If
				Next
			Catch orderEx As RecordNotFoundException
				Try
					Dim orderItemInfo As New KaOrderItem(userConn, currentSelectedItem)
					Dim productInfo As New KaProduct(userConn, orderItemInfo.ProductId)
					Dim densityInfo As KaRatio = productInfo.GetDensity(userConn, locationId)
					density = densityInfo.Numeric
					densityWeight = densityInfo.NumeratorUnitId
					densityVolume = densityInfo.DenominatorUnitId
					Dim weightUnitInfo As New KaUnit(userConn, densityWeight)
					Dim volumeUnitInfo As New KaUnit(userConn, densityVolume)
					compartmentWeight += KaUnit.Convert(productAmount, currentSelectedUnitOfMeasure, toUnitOfMeasure, density, weightUnitInfo, volumeUnitInfo)
				Catch ex As RecordNotFoundException
					compartmentWeightValid = False
					Return 0.0
				Catch ex As UnitConversionException
					compartmentWeightValid = False
					Return 0.0
				End Try
			Catch orderEx As UnitConversionException
				compartmentWeightValid = False
				Return 0.0
			End Try
		End If
		Return compartmentWeight
	End Function

	Private Sub ddlCustomerSite_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles ddlCustomerSite.SelectedIndexChanged
		If lstUsedOrders.SelectedIndex >= 0 Then
			Dim stagedOrderOrder As KaStagedOrderOrder = GetStagedOrderOrder(lstUsedOrders.SelectedValue)
			For Each currentOrder As KaStagedOrderOrder In _stagedOrderInfo.Orders
				If currentOrder.OrderId = stagedOrderOrder.OrderId Then
					Guid.TryParse(ddlCustomerSite.SelectedValue, currentOrder.CustomerAccountLocationId)
					Exit For
				End If
			Next
		End If
		PopulateOrdersList()
	End Sub

	Private Sub ShipToInformationChanged(sender As Object, e As System.EventArgs) Handles ddlCustomerSite.SelectedIndexChanged
		If litShipTo.Text.Trim.Length > 0 Then
			ddlCustomerSite.Visible = False
			litShipTo.Visible = True
		Else
			ddlCustomerSite.Visible = ddlCustomerSite.Items.Count > 1
			litShipTo.Visible = False ' (ddlCustomerSite.SelectedIndex = 0)
		End If
		lblShipTo.Visible = ddlCustomerSite.Visible OrElse litShipTo.Visible
	End Sub

	Private Sub lstUsedOrders_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles lstUsedOrders.SelectedIndexChanged
		Dim stagedOrderOrder As New KaStagedOrderOrder
		If lstUsedOrders.SelectedIndex >= 0 Then
			stagedOrderOrder = GetStagedOrderOrder(lstUsedOrders.SelectedValue)
			tbxOrderPercentage.Text = stagedOrderOrder.Percentage
			PopulateApplicatorsList(stagedOrderOrder.ApplicatorId)
			tbxOrderAcres.Text = stagedOrderOrder.Acres.ToString
			rowStagedOrderOrderAcresInfo.Visible = True
			rowStagedOrderOrderApplicatorInfo.Visible = True
		Else
			rowStagedOrderOrderAcresInfo.Visible = False
			rowStagedOrderOrderApplicatorInfo.Visible = False
		End If

		Dim orderInfoList As List(Of KaOrder) = GetOrderInfo()

		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim locationId As Guid = Guid.Empty
		Try
			locationId = New KaLocation(connection, Guid.Parse(ddlFacility.SelectedValue)).Id
		Catch ex As RecordNotFoundException
			locationId = _stagedOrderInfo.LocationId
		End Try
		Dim massUnitOfMeasure As New KaUnit(connection, KaUnit.GetSystemDefaultMassUnitOfMeasure(connection, Nothing))
		Dim volumeUnitOfMeasure As New KaUnit(connection, KaUnit.GetSystemDefaultVolumeUnitOfMeasure(connection, Nothing))

		For Each stagedObject As Object In pnlStagedTransports.Controls
			If Not TypeOf stagedObject Is HtmlGenericControl Then Continue For
			Dim transportRow As HtmlGenericControl = stagedObject
			If transportRow.ID IsNot Nothing AndAlso transportRow.ID.Contains("Transport") Then
				' This is a transport row
				Dim transportCompartmentTable As HtmlGenericControl = transportRow.FindControl(transportRow.ID & "_compTable")
				For Each listObject As Object In transportCompartmentTable.Controls
					If Not TypeOf listObject Is HtmlGenericControl Then Continue For
					Dim compartmentRow As HtmlGenericControl = listObject
					If compartmentRow.ID IsNot Nothing AndAlso compartmentRow.ID.Contains(transportCompartmentTable.ID & "_CompRow") Then
						' This is a compartment row
						Dim compartmentItemTable As HtmlGenericControl = compartmentRow.FindControl(compartmentRow.ID & "_ProdTable")

						For Each listObject2 As Object In compartmentItemTable.Controls
							If Not TypeOf listObject2 Is HtmlGenericControl Then Continue For
							Dim compProdRow As HtmlGenericControl = listObject2
							If compProdRow.ID IsNot Nothing AndAlso compProdRow.ID.Contains(compartmentItemTable.ID & "_ItemRow") Then
								' This is a compartment row
								Dim rowNumber As Integer = CInt(CType(compProdRow.FindControl(compProdRow.ID & "_RowNumber"), HtmlInputHidden).Value)
								Dim productDropdown As DropDownList = compProdRow.FindControl(compProdRow.ID & "_ddlProduct" & rowNumber.ToString)
								Dim currentSelectedItem As Guid = Guid.Empty
								Guid.TryParse(productDropdown.SelectedValue, currentSelectedItem)
								PopulateProductList(productDropdown, orderInfoList, currentSelectedItem)
							End If
						Next
					End If
				Next
			End If
		Next

		RefreshOrderDetails(connection, orderInfoList)

		Dim currentCustomerAccountLocationId As Guid = stagedOrderOrder.CustomerAccountLocationId

		ddlCustomerSite.Items.Clear()
		ddlCustomerSite.Items.Add(New ListItem("Select a location", Guid.Empty.ToString))
		Dim customerString As String = ""
		Dim accountIdList As New List(Of Guid)
		Dim accountIdsString As String = Q(Guid.Empty) ' Capture the empty Account ID for the account locations that are for all accounts
		' For Each orderInfo As KaOrder In orderInfoList
		Dim orderInfo As KaOrder
		Try
			orderInfo = New KaOrder(connection, stagedOrderOrder.OrderId)
		Catch ex As RecordNotFoundException
			ddlCustomerSite.SelectedIndex = 0
			litCustomerName.Text = customerString
			Exit Sub
		End Try
		For Each orderAccount As KaOrderCustomerAccount In orderInfo.OrderAccounts
			If Not orderAccount.Deleted AndAlso Not accountIdList.Contains(orderAccount.CustomerAccountId) Then accountIdList.Add(orderAccount.CustomerAccountId)
		Next
		For Each accountId As Guid In accountIdList
			accountIdsString &= "," & Q(accountId)
		Next
		'  Next
		Dim accountLocationFound As Boolean = False

		Dim accountLocationIds As String = ""

		Dim customerLocList As New ArrayList
		If accountIdsString.Length > 0 Then
			Dim customerList As ArrayList = KaCustomerAccount.GetAll(connection, "id in (" & accountIdsString & ")", "UPPER(name)")
			For Each customerInfo As KaCustomerAccount In customerList
				If customerString.Length > 0 Then customerString &= "<br />"
				customerString &= customerInfo.Name.Trim & IIf(customerInfo.AccountNumber.Trim.Length > 0, "(" & customerInfo.AccountNumber & ")", "")
			Next
			litCustomerName.Text = customerString
			customerLocList = KaCustomerAccountLocation.GetAll(connection, "customer_account_id in (" & accountIdsString & ") AND ((deleted=0) OR (id = " & Q(currentCustomerAccountLocationId) & "))", "UPPER(name)")
		End If

		For Each customerLocInfo As KaCustomerAccountLocation In customerLocList
			ddlCustomerSite.Items.Add(New ListItem(customerLocInfo.Name, customerLocInfo.Id.ToString))
			If customerLocInfo.Id.Equals(currentCustomerAccountLocationId) Then
				ddlCustomerSite.SelectedIndex = ddlCustomerSite.Items.Count - 1
				accountLocationFound = True
			End If
		Next

		If Not accountLocationFound Then ddlCustomerSite.SelectedIndex = 0
		litCustomerName.Text = customerString
		With orderInfo
			litShipTo.Text = .ShipToName
			If .ShipToStreet.Length > 0 Then
				If litShipTo.Text.Length > 0 Then litShipTo.Text &= "<br />"
				litShipTo.Text &= .ShipToStreet
			End If
			If .ShipToCity.Length + .ShipToState.Length + .ShipToZipCode.Length > 0 Then
				If litShipTo.Text.Length > 0 Then litShipTo.Text &= "<br />"
				litShipTo.Text &= .ShipToCity & ", " & .ShipToState & " " & .ShipToZipCode
			End If
		End With
		ShipToInformationChanged(ddlCustomerSite, New EventArgs)
	End Sub

	Private Sub RefreshOrderDetails(connection As OleDbConnection, ByVal orderInfoList As List(Of KaOrder))
		Dim locationId As Guid = Guid.Empty
		Try
			locationId = New KaLocation(connection, Guid.Parse(ddlFacility.SelectedValue)).Id
		Catch ex As RecordNotFoundException
			locationId = _stagedOrderInfo.LocationId
		End Try
		Dim defaultMassUnitInfo As KaUnit = New KaUnit(connection, KaUnit.GetSystemDefaultMassUnitOfMeasure(connection, Nothing))
		Dim defaultVolumeUnitInfo As KaUnit = New KaUnit(connection, KaUnit.GetSystemDefaultVolumeUnitOfMeasure(connection, Nothing))
		Dim orderItemsAssignedAmounts As Dictionary(Of Guid, KaQuantity) = GetOrderItemAssignedAmounts(connection, orderInfoList, locationId, defaultMassUnitInfo, defaultVolumeUnitInfo)
		Dim orderDetails As String = "<table width=""100%"">"
		If Not locationId.Equals(Guid.Empty) Then
			For Each orderInfo As KaOrder In orderInfoList
				orderDetails &= "<tr><td width=""100%""><table width=""100%"">"
				orderDetails &= "<tr><th colspan=""5"">Order #" & orderInfo.Number & IIf(orderInfo.DoNotBlend, " (Do not blend)", "") & "<br /><hr /></th></tr>"
				orderDetails &= "<tr><th>Product</th>" & vbCrLf &
					"<th style=""text-align: right;"">Order remaining</th>" & vbCrLf &
					"<th style=""text-align: right;"">Order remaining (" & defaultMassUnitInfo.Abbreviation & ")</th>" & vbCrLf &
					"<th style=""text-align: right;"">Assigned</th>" & vbCrLf &
					"<th style=""text-align: right;"">Remaining</th>" & vbCrLf &
					"</tr>"
				With orderInfo
					Dim orderTotalRemMass = 0.0
					Dim orderTotalReqMass = 0.0
					Dim entryCounter As Integer = 0
					Do While entryCounter < .OrderItems.Count
						Dim orderEntry As KaOrderItem = .OrderItems(entryCounter)
						Try
							Dim productInfo As New KaProduct(connection, orderEntry.ProductId)
							Dim entryUom As New KaUnit(connection, orderEntry.UnitId)

							Dim productBulkItemsForFacility As List(Of KaProductBulkProduct) = productInfo.ProductBulkItemsForFacility(locationId)
							If productBulkItemsForFacility.Count = 0 Then
								orderDetails &= "<tr><td>" & productInfo.Name.Trim & "</td><td colspan=""5"">Not available for this facility</td></tr>"
							Else
								For Each prodBulkProd As KaProductBulkProduct In productBulkItemsForFacility
									Try
										Dim bulkProdInfo As New KaBulkProduct(connection, prodBulkProd.BulkProductId)
										Dim entryWeight As String = Format(Math.Max(0.0, orderEntry.Request - orderEntry.Delivered) * prodBulkProd.Portion / 100, entryUom.UnitPrecision)
										Dim entryRemMass As String = ""

										Dim bulkProdWeightUnitInfo As New KaUnit(connection, bulkProdInfo.WeightUnitId)
										Dim bulkProdVolumeUnitInfo As New KaUnit(connection, bulkProdInfo.VolumeUnitId)

										Dim currentEntryMass As Double = 0
										Try
											currentEntryMass = KaUnit.Convert(Math.Max(0.0, orderEntry.Request - orderEntry.Delivered) * prodBulkProd.Portion / 100, entryUom, defaultMassUnitInfo, bulkProdInfo.Density, bulkProdWeightUnitInfo, bulkProdVolumeUnitInfo)
											orderTotalRemMass += currentEntryMass
											orderTotalReqMass += KaUnit.Convert(orderEntry.Request * prodBulkProd.Portion / 100, entryUom, defaultMassUnitInfo, bulkProdInfo.Density, bulkProdWeightUnitInfo, bulkProdVolumeUnitInfo)
											entryRemMass = Format(currentEntryMass, defaultMassUnitInfo.UnitPrecision)
										Catch ex As UnitConversionException
											entryRemMass = "Conversion Error"
										End Try
										orderDetails &= "<tr><td>" & bulkProdInfo.Name.Trim & IIf(productBulkItemsForFacility.Count = 1, "", " (" & productInfo.Name.Trim & ")") & "</td>"
										If entryUom.Id.Equals(defaultMassUnitInfo.Id) Then
											orderDetails &= "<td>&nbsp;</td>"
										Else
											orderDetails &= "<td style=""text-align: right;"">" & entryWeight & " " & entryUom.Abbreviation & "</td>"
										End If
										orderDetails &= "<td style=""text-align: right;"">" & entryRemMass & " " & defaultMassUnitInfo.Abbreviation & "</td>"
										Dim assignedWeight As String = Format(0.0, entryUom.UnitPrecision) & " " & entryUom.Abbreviation
										Dim entryRemainingWeight As String = Format(0.0, entryUom.UnitPrecision) & " " & entryUom.Abbreviation
										If orderItemsAssignedAmounts.ContainsKey(orderEntry.Id) Then
											Try
												Dim assignedAmount As Double = KaUnit.Convert(connection, orderItemsAssignedAmounts(orderEntry.Id), New KaRatio(bulkProdInfo.Density, bulkProdInfo.WeightUnitId, bulkProdInfo.VolumeUnitId), entryUom.Id).Numeric
												assignedWeight = Format(Math.Max(0.0, assignedAmount) * prodBulkProd.Portion / 100, entryUom.UnitPrecision) & " " & entryUom.Abbreviation
												entryRemainingWeight = Format((orderEntry.Request - orderEntry.Delivered - assignedAmount) * prodBulkProd.Portion / 100, entryUom.UnitPrecision) & " " & entryUom.Abbreviation
											Catch ex As UnitConversionException
												assignedWeight = "Conversion Error"
												entryRemainingWeight = "Conversion Error"
											End Try
										End If
										orderDetails &= "<td style=""text-align: right;"">" & assignedWeight & "&nbsp;</td>"
										orderDetails &= "<td style=""text-align: right;"">" & entryRemainingWeight & "&nbsp;</td>"
										orderDetails &= "<td>&nbsp;</td></tr>"
									Catch ex As RecordNotFoundException
									End Try
								Next
							End If
						Catch ex As RecordNotFoundException

						End Try
						entryCounter += 1
					Loop
					orderDetails &= "<tr><td colspan=""5""><hr /></td></tr>"
					orderDetails &= "<tr><td>Order Remaining Total</td>"
					orderDetails &= "<td>&nbsp;</td> "
					orderDetails &= "<td style=""text-align: right;"">" & Format(orderTotalRemMass, "#,###,##0.##") & " " & defaultMassUnitInfo.Abbreviation & "</td>"
					orderDetails &= "<td colspan=""5"">&nbsp;</td></tr>"

					orderDetails &= "</table></td></tr>"
					orderDetails &= "<tr><td><br /> <br /></td></tr>"
				End With
			Next
		End If
		orderDetails &= "</table>"
		litOrderDetails.Text = orderDetails
		orderDetailsCell.Visible = Not locationId.Equals(Guid.Empty) AndAlso (orderInfoList.Count > 1 OrElse (orderInfoList.Count = 1 AndAlso Not orderInfoList(0).Id.Equals(Guid.Empty)))
	End Sub

	Private Function GetOrderItemAssignedAmounts(connection As OleDbConnection, ByVal orderInfoList As List(Of KaOrder), ByVal locationId As Guid, ByVal massUnitOfMeasure As KaUnit, ByVal volumeUnitOfMeasure As KaUnit) As Dictionary(Of Guid, KaQuantity)
		Dim orderItemsAssignedAmounts As New Dictionary(Of Guid, KaQuantity)
		Dim transportList As New List(Of KaStagedOrderTransport)
		Dim compartmentList As New List(Of KaStagedOrderCompartment)
		ConvertTransportTableToObjects(transportList, compartmentList)
		For Each compartment As KaStagedOrderCompartment In compartmentList
			For Each compItem As KaStagedOrderCompartmentItem In compartment.CompartmentItems
				Dim orderItemId As Guid = compItem.OrderItemId
				Try
					Dim productAmount As Double = compItem.Quantity
					Dim compartmentUofM As New KaUnit(connection, compItem.UnitId)

					Dim entryMass As Double = 0.0
					If orderItemId.Equals(Guid.Empty) Then 'All Products from all orders
						For Each allProdOrderInfo As KaOrder In orderInfoList
							Dim totalAmount As Double = 0.0
							Dim orderItemMass As New Dictionary(Of Guid, Double)
							For Each orderItem As KaOrderItem In allProdOrderInfo.OrderItems
								If Not orderItemsAssignedAmounts.ContainsKey(orderItem.Id) Then orderItemsAssignedAmounts.Add(orderItem.Id, New KaQuantity(0.0, massUnitOfMeasure.Id))
								Dim productInfo As New KaProduct(connection, orderItem.ProductId)
								orderItemMass(orderItem.Id) = KaUnit.Convert(connection, New KaQuantity(orderItem.Request, orderItem.UnitId), productInfo.GetDensity(connection, locationId), massUnitOfMeasure.Id).Numeric
								totalAmount += orderItemMass(orderItem.Id)
							Next

							If totalAmount > 0 Then
								For Each orderItem As KaOrderItem In allProdOrderInfo.OrderItems
									Dim productInfo As New KaProduct(connection, orderItem.ProductId)
									Dim compartmentWeightValid As Boolean = True
									entryMass = GetCompartmentQuantity(connection, orderItem.Id, productAmount, orderInfoList, locationId, massUnitOfMeasure, massUnitOfMeasure, volumeUnitOfMeasure, compartmentUofM, compartmentWeightValid)

									If compartmentWeightValid Then
										orderItemsAssignedAmounts(orderItem.Id).Numeric += entryMass * orderItemMass(orderItem.Id) / totalAmount
									Else
										Throw New UnitConversionException("Conversion Error")
									End If
								Next
							End If
						Next
					Else
						Try
							Dim allProdOrderInfo As New KaOrder(connection, orderItemId) ' All products from this order
							Dim totalAmount As Double = 0.0
							Dim orderItemMass As New Dictionary(Of Guid, Double)
							For Each orderItem As KaOrderItem In allProdOrderInfo.OrderItems
								If Not orderItemsAssignedAmounts.ContainsKey(orderItem.Id) Then orderItemsAssignedAmounts.Add(orderItem.Id, New KaQuantity(0.0, massUnitOfMeasure.Id))
								Dim productInfo As New KaProduct(connection, orderItem.ProductId)
								orderItemMass(orderItem.Id) = KaUnit.Convert(connection, New KaQuantity(orderItem.Request, orderItem.UnitId), productInfo.GetDensity(connection, locationId), massUnitOfMeasure.Id).Numeric
								totalAmount += orderItemMass(orderItem.Id)
							Next

							If totalAmount > 0 Then
								For Each orderItem As KaOrderItem In allProdOrderInfo.OrderItems
									Dim compartmentWeightValid As Boolean = True
									entryMass = GetCompartmentQuantity(connection, orderItem.Id, productAmount, orderInfoList, locationId, massUnitOfMeasure, massUnitOfMeasure, volumeUnitOfMeasure, compartmentUofM, compartmentWeightValid)

									If compartmentWeightValid Then
										orderItemsAssignedAmounts(orderItem.Id).Numeric += entryMass * orderItemMass(orderItem.Id) / totalAmount
									Else
										Throw New UnitConversionException("Conversion Error")
									End If
								Next
							End If
						Catch ex As RecordNotFoundException
							Dim compartmentWeightValid As Boolean = True
							entryMass = GetCompartmentQuantity(connection, orderItemId, productAmount, orderInfoList, locationId, massUnitOfMeasure, massUnitOfMeasure, volumeUnitOfMeasure, compartmentUofM, compartmentWeightValid)

							If compartmentWeightValid Then
								If Not orderItemsAssignedAmounts.ContainsKey(orderItemId) Then orderItemsAssignedAmounts.Add(orderItemId, New KaQuantity(0.0, massUnitOfMeasure.Id))
								orderItemsAssignedAmounts(orderItemId).Numeric += entryMass
							Else
								Throw New UnitConversionException("Conversion Error")
							End If
						End Try
					End If
				Catch ex As UnitConversionException
					orderItemsAssignedAmounts = New Dictionary(Of Guid, KaQuantity)
				End Try
			Next
		Next
		Return orderItemsAssignedAmounts
	End Function

	Private Function GetOrderInfo() As List(Of KaOrder)
		Dim orderIdList As New List(Of KaStagedOrderOrder)
		For Each orderListItem As ListItem In lstUsedOrders.Items
			Dim stagedOrderOrder As KaStagedOrderOrder = GetStagedOrderOrder(orderListItem.Value)
			Dim orderfound As Boolean = False
			For Each order As KaStagedOrderOrder In orderIdList
				If order.OrderId.Equals(stagedOrderOrder.OrderId) Then
					orderfound = True
					Exit For
				End If
			Next
			If Not orderfound Then orderIdList.Add(stagedOrderOrder)
		Next

		Return GetOrderInfo(orderIdList)
	End Function

	Private Function GetOrderInfo(orderIdList As List(Of KaStagedOrderOrder)) As List(Of KaOrder)
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim orderList As New List(Of KaOrder)
		For Each stagedOrderOrder As KaStagedOrderOrder In orderIdList
			Try
				Dim orderInfo As New KaOrder(connection, stagedOrderOrder.OrderId)
				orderInfo.GetChildren(connection, Nothing, True)
				orderList.Add(orderInfo)
			Catch ex As RecordNotFoundException
			End Try
		Next
		If orderList.Count = 0 Then orderList.Add(New KaOrder)

		Return orderList
	End Function

	Private Function GetCurrentStagedOrderOrders() As List(Of KaStagedOrderOrder)
		Dim currentStagedOrderOrders As New List(Of KaStagedOrderOrder)
		For Each usedOrder As ListItem In lstUsedOrders.Items
			currentStagedOrderOrders.Add(GetStagedOrderOrder(usedOrder.Value))
		Next
		If currentStagedOrderOrders.Count = 1 Then currentStagedOrderOrders(0).Percentage = 100
		Return currentStagedOrderOrders
	End Function

	Private Function GetStagedOrderOrder(ByVal stagedOrderOrderXml As String) As KaStagedOrderOrder
		Dim stagedOrderOrder As KaStagedOrderOrder

		Try
			Dim xmlHandler As New System.Xml.Serialization.XmlSerializer(GetType(KaStagedOrderOrder))
			Dim xmlStringReader As New IO.StringReader(Server.HtmlDecode(stagedOrderOrderXml))
			stagedOrderOrder = xmlHandler.Deserialize(xmlStringReader)
		Catch ex As Exception
			stagedOrderOrder = New KaStagedOrderOrder
		End Try
		Return stagedOrderOrder
	End Function
	Private Function ValidateStagedOrder() As Boolean
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Try
			Dim locationInfo As New KaLocation(connection, Guid.Parse(CType(ddlFacility.SelectedItem, ListItem).Value.ToString))
			If locationInfo.Deleted Then
				DisplayJavaScriptMessage("LocationDeletedWarning", Utilities.JsAlert(String.Format($"The facility {locationInfo.Name} ({locationInfo.Id}) has been marked as deleted.")), False)
				Return False
			End If
		Catch ex As RecordNotFoundException
			DisplayJavaScriptMessage("InvalidFacility", Utilities.JsAlert("A Facility must be Selected"), False)
			Return False
		End Try
		If lstUsedOrders.Items.Count = 0 Then
			DisplayJavaScriptMessage("InvalidOrder", Utilities.JsAlert("An Order must be Selected"), False)
			Return False
		End If

		With _stagedOrderInfo
			Dim stagedOrderIdsUsed As String = ""
			Dim percentage As Double = 0.0
			For Each currentOrder As KaStagedOrderOrder In .Orders
				If Not currentOrder.Deleted Then
					percentage += currentOrder.Percentage
					If stagedOrderIdsUsed.Length > 0 Then stagedOrderIdsUsed &= ","
					stagedOrderIdsUsed &= Q(currentOrder.OrderId)
					If Not currentOrder.ApplicatorId.Equals(Guid.Empty) Then
						Try
							Dim applicator As New KaApplicator(LfDatabase.Connection, currentOrder.ApplicatorId)
							If applicator.Deleted Then
								DisplayJavaScriptMessage("ApplicatorDeletedWarning", Utilities.JsAlert(String.Format($"The applicator {applicator.Name} ({applicator.Id}) assigned to order {New KaOrder(connection, currentOrder.OrderId).Number} has been marked as deleted.")), False)
								Return False
							End If
						Catch ex As RecordNotFoundException
							DisplayJavaScriptMessage("ApplicatorNotFoundWarning", Utilities.JsAlert(String.Format($"The applicator assigned to order {New KaOrder(connection, currentOrder.OrderId).Number} could not be found.")), False)
							Return False
						End Try
					End If
					If Not currentOrder.CustomerAccountLocationId.Equals(Guid.Empty) Then
						Try
							Dim customerAccountLocation As New KaCustomerAccountLocation(LfDatabase.Connection, currentOrder.CustomerAccountLocationId)
							If customerAccountLocation.Deleted Then
								DisplayJavaScriptMessage("CustomerAccountLocationDeletedWarning", Utilities.JsAlert(String.Format($"The customer account destination {customerAccountLocation.Name} ({customerAccountLocation.Id}) assigned to order {New KaOrder(connection, currentOrder.OrderId).Number} has been marked as deleted.")), False)
								Return False
							End If
						Catch ex As RecordNotFoundException
							DisplayJavaScriptMessage("CustomerAccountLocationNotFoundWarning", Utilities.JsAlert(String.Format($"The customer account destination assigned to order {New KaOrder(connection, currentOrder.OrderId).Number} could not be found.")), False)
							Return False
						End Try
					End If
				End If
			Next
			If chkUseOrderPercents.Checked AndAlso .Orders.Count > 1 Then
				If Not Utilities.NearlyEqual(percentage, 100, 0.01) Then
					DisplayJavaScriptMessage("InvalidPercent", Utilities.JsAlert("The total percentage assigned must be 100%.  It currently is " & percentage & "%."), False)
					Return False
				End If
			End If
			For Each stagedComp As KaStagedOrderCompartment In .Compartments
				Dim allOrderItemIds As List(Of Guid) = New List(Of Guid)
				For Each stagedItem As KaStagedOrderCompartmentItem In stagedComp.CompartmentItems
					If allOrderItemIds.Contains(stagedItem.OrderItemId) Then
						DisplayJavaScriptMessage("InvalidCompOrderItems", Utilities.JsAlert("A compartment contains the same entry twice.  Please ensure an entry is only specified once per compartment."), False)
						Return False
					Else
						allOrderItemIds.Add(stagedItem.OrderItemId)
					End If
					If Not stagedItem.Deleted AndAlso Not stagedItem.Quantity > 0 Then
						DisplayJavaScriptMessage("InvalidCompAmt", Utilities.JsAlert("A requested amount must be assigned to each compartment quantity."), False)
						Return False
					End If
					If Not stagedItem.Deleted AndAlso Not stagedItem.OrderItemId.Equals(Guid.Empty) Then
						Try
							Dim orderItem As New KaOrderItem(LfDatabase.Connection, stagedItem.OrderItemId)
							If orderItem.Deleted Then
								Dim errorMessage As String = ""
								Try
									errorMessage = String.Format($"{ New KaProduct(LfDatabase.Connection, orderItem.ProductId).Name} located at position {(orderItem.Position + 1).ToString()} for order {New KaOrder(LfDatabase.Connection, orderItem.OrderId).Number} has been marked as deleted.")
								Catch ex As RecordNotFoundException
									errorMessage = "An item assigned to an order has been marked as deleted."
								End Try
								DisplayJavaScriptMessage("OrderItemNotFoundWarning", Utilities.JsAlert(errorMessage), False)
								Return False
							End If
						Catch ex As RecordNotFoundException
							Try
								For Each orderItem As KaOrderItem In New KaOrder(LfDatabase.Connection, stagedItem.OrderItemId).OrderItems
									If allOrderItemIds.Contains(orderItem.Id) Then
										DisplayJavaScriptMessage("InvalidCompOrderItems", Utilities.JsAlert("A compartment contains the same entry twice.  Please ensure an entry is only specified once per compartment."), False)
										Return False
									Else
										allOrderItemIds.Add(orderItem.Id)
									End If
								Next
							Catch ex2 As RecordNotFoundException
								DisplayJavaScriptMessage("OrderItemNotFoundWarning", Utilities.JsAlert(String.Format($"The orderItem assigned to the staged order could not be found.")), False)
								Return False
							End Try
						End Try
					End If
				Next
			Next
			Dim transportCount As Integer = 0
			Dim validTransportCount As Integer = 0
			For Each currentStagedTransport As KaStagedOrderTransport In .Transports
				If Not currentStagedTransport.Deleted Then
					If Not currentStagedTransport.TransportId.Equals(Guid.Empty) Then
						If Not currentStagedTransport.TransportId.Equals(Guid.Empty) Then
							Try
								Dim transport As New KaTransport(LfDatabase.Connection, currentStagedTransport.TransportId)
								If transport.Deleted Then
									DisplayJavaScriptMessage("TransportDeletedWarning", Utilities.JsAlert(String.Format($"The transport {transport.Name} ({transport.Id}) has been marked as deleted.")), False)
									Return False
								End If
							Catch ex As RecordNotFoundException
								DisplayJavaScriptMessage("TransportNotFoundWarning", Utilities.JsAlert(String.Format($"The transport assigned to the staged order could not be found.")), False)
								Return False
							End Try
						End If
						validTransportCount += 1
					End If
					transportCount += 1
				End If
			Next

			If transportCount > 1 AndAlso transportCount <> validTransportCount Then
				DisplayJavaScriptMessage("InvalidTransportCount", Utilities.JsAlert("There are more than 1 transport assigned to this order, but not all transports have been defined."), False)
				Return False
			End If

			If .Compartments.Count > 8 Then
				DisplayJavaScriptMessage("InvalidCompartmentCount", Utilities.JsAlert("A maximum of 8 compartments may be staged."), False)
				Return False
			End If

			Dim massUnitOfMeasure As New KaUnit(connection, KaUnit.GetSystemDefaultMassUnitOfMeasure(connection, Nothing))
			Dim volumeUnitOfMeasure As New KaUnit(connection, KaUnit.GetSystemDefaultVolumeUnitOfMeasure(connection, Nothing))
			Dim orderInfoList As List(Of KaOrder) = GetOrderInfo()
			For Each currentStagedTransport As KaStagedOrderTransport In .Transports
				If Not currentStagedTransport.Deleted Then
					Dim maxWeight As Double = 0.0
					Dim transportUnitId As Guid = Guid.Empty
					Dim transportInfo As New KaTransport
					Try
						transportInfo = New KaTransport(connection, currentStagedTransport.TransportId)
						maxWeight = transportInfo.MaximumWeight
					Catch ex As RecordNotFoundException
						maxWeight = 0.0
					End Try
					If transportUnitId.Equals(Guid.Empty) Then transportUnitId = KaUnit.GetSystemDefaultMassUnitOfMeasure(connection, Nothing)
					Dim transportUnitInfo As New KaUnit(connection, transportUnitId)
					If maxWeight > 0 Then
						Try
							maxWeight -= KaUnit.Convert(connection, New KaQuantity(currentStagedTransport.TareWeight, currentStagedTransport.TareUnitId), New KaRatio(0.0, massUnitOfMeasure.Id, volumeUnitOfMeasure.Id), transportUnitId).Numeric
						Catch ex As UnitConversionException

						End Try
					End If
					For Each compartmentInfo As KaStagedOrderCompartment In .Compartments
						If Not compartmentInfo.Deleted Then
							' Now, validate the max transport weight
							Dim transportCompInfo As KaTransportCompartment = Nothing
							Try
								transportCompInfo = New KaTransportCompartment(connection, compartmentInfo.TransportCompartmentId)
								If transportCompInfo.Capacity = 0 Then
									transportCompInfo = Nothing ' We can reset it, since it is a don't care scenario
								ElseIf transportCompInfo.Deleted Then
									DisplayJavaScriptMessage("TransportCompartmentDeletedWarning", Utilities.JsAlert(String.Format($"The compartment located at position {(.Compartments.IndexOf(compartmentInfo) + 1).ToString()} ({ compartmentInfo.TransportCompartmentId.ToString()}) for the transport {New KaTransport(connection, transportCompInfo.TransportId).Name} has been marked As deleted.")), False)
									Return False
								End If
							Catch ex As RecordNotFoundException
							End Try

							If maxWeight > 0.0 Then
								For Each compartmentItem As KaStagedOrderCompartmentItem In compartmentInfo.CompartmentItems
									If Not compartmentItem.Deleted AndAlso compartmentInfo.StagedOrderTransportId.Equals(currentStagedTransport.Id) Then
										Dim compartmentMassValid As Boolean = True
										Try
											Dim compartmentUofM As New KaUnit(connection, compartmentItem.UnitId)
											Dim compartmentMass As Double = GetCompartmentQuantity(connection, compartmentItem.OrderItemId, compartmentItem.Quantity, orderInfoList, .LocationId, transportUnitInfo, massUnitOfMeasure, volumeUnitOfMeasure, compartmentUofM, compartmentMassValid)
											maxWeight -= compartmentMass

											If transportCompInfo IsNot Nothing Then
												Dim compartmentUnitInfo As New KaUnit(connection, transportCompInfo.UnitId)
												Dim compartmentQty As Double = GetCompartmentQuantity(connection, compartmentItem.OrderItemId, compartmentItem.Quantity, orderInfoList, .LocationId, compartmentUnitInfo, massUnitOfMeasure, volumeUnitOfMeasure, compartmentUofM, compartmentMassValid)
												transportCompInfo.Capacity -= compartmentQty

												If transportCompInfo.Capacity < 0 Then
													DisplayJavaScriptMessage("InvalidCompCapacity", Utilities.JsAlert("The capacity has been exceeded For compartment " & (transportCompInfo.Position + 1).ToString & " For the transport " & transportInfo.Name & "."), False)
													Return False
												End If
											End If
										Catch ex As RecordNotFoundException
											compartmentMassValid = False
										End Try
										If Not compartmentMassValid Then
											DisplayJavaScriptMessage("InvalidMaxWeight", Utilities.JsAlert("Unable To calculate the maximum weight For the order For the transport " & transportInfo.Name & "."), False)
											Return False
										ElseIf maxWeight < 0.0 AndAlso Not .LossInWeight Then
											DisplayJavaScriptMessage("InvalidMaxWeight", Utilities.JsAlert("Maximum weight exceeded For transport " & transportInfo.Name & "."), False)
											Return False
										End If
									End If
								Next
							End If
						End If
					Next
				End If
			Next
			.UseOrderPercents = chkUseOrderPercents.Checked

			Dim allowOrdersToBeAssignedToMultipleStagedOrders As Boolean = True
			Boolean.TryParse(KaSetting.GetSetting(connection, "StagedOrder/AllowOrdersToBeAssignedToMultipleStagedOrders", True), allowOrdersToBeAssignedToMultipleStagedOrders)
			If stagedOrderIdsUsed.Length > 0 AndAlso Not allowOrdersToBeAssignedToMultipleStagedOrders Then
				Dim getOrdersAssignedToOtherStagedOrdersSql As String = String.Format("Select COUNT(*) " &
						"FROM staged_order_orders AS soo " &
						"INNER JOIN staged_orders AS so ON soo.staged_order_id = so.id " &
						"WHERE (soo.deleted = 0) " &
							"AND (so.deleted = 0) " &
							"AND (soo.order_id IN ({0})) " &
							"AND (so.id <> {1})", stagedOrderIdsUsed, Q(.Id))
				Dim getOrdersAssignedToOtherStagedOrdersRdr As OleDbDataReader = Tm2Database.ExecuteReader(connection, getOrdersAssignedToOtherStagedOrdersSql)
				If getOrdersAssignedToOtherStagedOrdersRdr.Read() AndAlso getOrdersAssignedToOtherStagedOrdersRdr.Item(0) > 0 Then
					DisplayJavaScriptMessage("OrderAssignedToDifferentStagedOrder", Utilities.JsAlert("The staged order contains a reference to an order that is already assigned to a different staged order."), False)
					Return False

				End If
			End If
			If Not .CarrierId.Equals(Guid.Empty) Then
				Try
					Dim carrier As New KaCarrier(LfDatabase.Connection, .CarrierId)
					If carrier.Deleted Then
						DisplayJavaScriptMessage("CarrierDeletedWarning", Utilities.JsAlert(String.Format($"The carrier {carrier.Name} ({carrier.Id}) has been marked as deleted.")), False)
						Return False
					End If
				Catch ex As RecordNotFoundException
					DisplayJavaScriptMessage("CarrierNotFoundWarning", Utilities.JsAlert(String.Format($"The carrier assigned to the staged order could not be found.")), False)
					Return False
				End Try
			End If
			If Not .DriverId.Equals(Guid.Empty) Then
				Try
					Dim driver As New KaDriver(LfDatabase.Connection, .DriverId)
					If driver.Deleted Then
						DisplayJavaScriptMessage("driverDeletedWarning", Utilities.JsAlert(String.Format($"The driver {driver.Name} ({driver.Id}) has been marked as deleted.")), False)
						Return False
					End If
				Catch ex As RecordNotFoundException
					DisplayJavaScriptMessage("driverNotFoundWarning", Utilities.JsAlert(String.Format($"The driver assigned to the staged order could not be found.")), False)
					Return False
				End Try
			End If
			If Not .BayId.Equals(Guid.Empty) Then
				Try
					Dim bay As New KaBay(LfDatabase.Connection, .BayId)
					If bay.Deleted Then
						DisplayJavaScriptMessage("BayDeletedWarning", Utilities.JsAlert(String.Format($"The bay {bay.Name} ({bay.Id}) has been marked as deleted.")), False)
						Return False
					End If
				Catch ex As RecordNotFoundException
					DisplayJavaScriptMessage("BayNotFoundWarning", Utilities.JsAlert(String.Format($"The bay assigned to the staged order could not be found.")), False)
					Return False
				End Try
			End If
		End With

		Return True
	End Function

	Private Function SaveStagedOrder(ByRef stagedOrderId As Guid) As Boolean
		_stagedOrderInfo = ConvertPageToStagedOrder()
		UpdateStagedTransportsWithTransportTares()
		Dim saveValid As Boolean = Not _assignProductError AndAlso ValidateStagedOrder()

		If saveValid Then
			' Check to see if the All Products from Order was selected.  If so, split into individual components
			Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
			Dim massUnitId As Guid = KaUnit.GetSystemDefaultMassUnitOfMeasure(connection, Nothing)
			Dim volumeUnitId As Guid = KaUnit.GetSystemDefaultVolumeUnitOfMeasure(connection, Nothing)

			For Each stagedCompartment As KaStagedOrderCompartment In _stagedOrderInfo.Compartments
				' Compartment items that are for all products on an order need to be saved individually
				Dim itemCounter As Integer = 0
				Dim positionCounter As Integer = 0
				Do While itemCounter < stagedCompartment.CompartmentItems.Count
					Dim stagedCompartmentItem As KaStagedOrderCompartmentItem = stagedCompartment.CompartmentItems(itemCounter)
					If Not stagedCompartmentItem.Deleted Then
						Try
							Dim order As New KaOrder(connection, stagedCompartmentItem.OrderItemId) ' this will be set to the order id if they want all products from the order
							Dim requestedQty As KaQuantity = order.GetRequested(_stagedOrderInfo.LocationId, stagedCompartmentItem.UnitId)
							For Each orderItem As KaOrderItem In order.OrderItems
								If Not orderItem.Deleted Then
									Dim newCompItem As KaStagedOrderCompartmentItem = stagedCompartmentItem.Clone()
									newCompItem.Id = Guid.NewGuid
									newCompItem.OrderItemId = orderItem.Id
									If requestedQty.Numeric > 0 Then
										Dim density As KaRatio = New KaProduct(connection, orderItem.ProductId).GetDensity(connection, _stagedOrderInfo.LocationId)
										newCompItem.Quantity = (KaUnit.Convert(connection, New KaQuantity(orderItem.Request, orderItem.UnitId), density, stagedCompartmentItem.UnitId).Numeric / requestedQty.Numeric) * stagedCompartmentItem.Quantity
									Else
										newCompItem.Quantity = 0
									End If
									newCompItem.Position = positionCounter
									stagedCompartment.CompartmentItems.Insert(stagedCompartment.CompartmentItems.IndexOf(stagedCompartmentItem), newCompItem)
									itemCounter += 1 ' increment the counter to not process the current item again
									positionCounter += 1
								End If
							Next
							stagedCompartmentItem.Deleted = True
						Catch ex As RecordNotFoundException
						End Try
					End If
					If Not stagedCompartmentItem.Deleted Then positionCounter += 1

					itemCounter += 1
				Loop
			Next

			With _stagedOrderInfo
				Dim updateConnection As OleDbConnection = New OleDbConnection(Tm2Database.GetDbConnection())
				Dim updateTransaction As OleDbTransaction = Nothing
				Try
					updateConnection.Open()
					updateTransaction = updateConnection.BeginTransaction

					.SqlUpdateInsertIfNotFound(updateConnection, updateTransaction, Database.ApplicationIdentifier, _currentUser.Name)

					' Per TFS ID 847, the tare weight on the staged order page should always be saved.
					For Each stagedOrderTransport As KaStagedOrderTransport In .Transports
						If Not stagedOrderTransport.Deleted AndAlso Not stagedOrderTransport.TareManual Then
							Try
								Dim transportInfo As New KaTransport(updateConnection, stagedOrderTransport.TransportId, updateTransaction)
								transportInfo.TareManual = stagedOrderTransport.TareManual
								transportInfo.TareWeight = KaUnit.Convert(updateConnection, New KaQuantity(stagedOrderTransport.TareWeight, stagedOrderTransport.TareUnitId), New KaRatio(0.0, massUnitId, volumeUnitId), transportInfo.UnitId, updateTransaction).Numeric
								transportInfo.TaredAt = stagedOrderTransport.TaredAt
								transportInfo.SqlUpdateInsertIfNotFound(updateConnection, updateTransaction, Database.ApplicationIdentifier, _currentUser.Name)
							Catch ex As RecordNotFoundException

							End Try
						End If
					Next

					updateTransaction.Commit()
				Catch ex As Exception
					If updateTransaction IsNot Nothing Then updateTransaction.Rollback()
					Page.Response.Redirect("ErrorForm.aspx?er=" & Utilities.StripCrLf("Staged Orders - Save Staged Order " & ex.Message))
				Finally
					If updateTransaction IsNot Nothing Then updateTransaction.Dispose()
					updateConnection.Close()
				End Try
				stagedOrderId = .Id
			End With
		End If
		Return saveValid
	End Function

	''' <summary>
	''' This will update the Staged Order Transport Tare information with the tare information from the transport, if it has not been set previously
	''' </summary>
	''' <remarks></remarks>
	Private Sub UpdateStagedTransportsWithTransportTares()
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		With _stagedOrderInfo
			' Check if the tare weights have been assigned. If not, assign them.
			For Each currentStagedTransport As KaStagedOrderTransport In .Transports
				If Not currentStagedTransport.Deleted AndAlso currentStagedTransport.TareWeight = 0 AndAlso currentStagedTransport.TaredAt <= New DateTime(1900, 1, 1, 0, 0, 0) Then
					' The tare weights have not been set, so set them to the transports current values
					Try
						Dim transportInfo As New KaTransport(connection, currentStagedTransport.TransportId)
						currentStagedTransport.TareWeight = transportInfo.TareWeight
						currentStagedTransport.TaredAt = transportInfo.TaredAt
						currentStagedTransport.TareUnitId = transportInfo.UnitId
						currentStagedTransport.TareManual = transportInfo.TareManual
					Catch ex As RecordNotFoundException

					End Try
				End If
			Next
		End With
	End Sub

	Private Sub btnSave_Click(sender As Object, e As System.EventArgs) Handles btnSave.Click
		Dim stagedOrderId As Guid = Guid.Empty
		If SaveStagedOrder(stagedOrderId) Then
			PopulateStagedOrdersList(stagedOrderId)
			ddlStagedOrders_SelectedIndexChanged(ddlStagedOrders, New EventArgs())
			lblStatus.Text = "Staged Order saved successfully"
		Else
			lblStatus.Text = "Staged Order not saved"
		End If
	End Sub

	Private Sub TransportChanged(sender As Object, e As System.EventArgs)
		' Transports Compartment table is transportRow.ID & "_compTable"
		Dim transportRow As HtmlGenericControl = CType(sender, DropDownList).Parent.Parent
		Dim controlIdStrings() As String = CType(sender, DropDownList).ID.Split("_")
		Dim currentTransportRowId As String = controlIdStrings(0)
		Dim currentStagedTransportId As Guid = Guid.Parse(CType(transportRow.FindControl(controlIdStrings(0) & "_Id"), HtmlInputHidden).Value)
		Dim transportId As Guid = Guid.Empty
		Dim transportList As DropDownList = sender
		Guid.TryParse(transportList.SelectedValue, transportId)
		For Each transport As KaStagedOrderTransport In _stagedOrderInfo.Transports
			If transport.Id = currentStagedTransportId Then
				transport.TransportId = transportId
			End If
		Next

		CreateDynamicControls() ' This needs to be called to set the Tare information, and the compartment labels
	End Sub

#Region " Staged Order Tare "
	Private Sub TransportAssignTareButtonClicked(sender As Object, e As System.EventArgs)
		' Transports Compartment table is transportRow.ID & "_compTable"
		Dim controlIdStrings() As String = CType(sender, LinkButton).ID.Split("_")
		Dim currentTransportRowId As String = controlIdStrings(0)
		Dim transportId As Guid = Guid.Empty
		Dim transportList As DropDownList = FindControl(currentTransportRowId & "_ddlTransport")
		Guid.TryParse(transportList.SelectedValue, transportId)

		Try
			Dim transportInfo As New KaTransport(GetUserConnection(_currentUser.Id), transportId)
			AssignTareWeightToTransport(currentTransportRowId, transportInfo.TareWeight, transportInfo.UnitId, transportInfo.TaredAt, transportInfo.TareManual)
			'SetTareWeightOnTransports.Value = "false"
		Catch ex As RecordNotFoundException

		End Try
	End Sub

	Public Sub AssignTareWeightToTransport(ByVal currentTransportRowId As String, ByVal tareWeight As Double, ByVal tareUnitId As Guid, ByVal tareDate As DateTime, ByVal tareManual As Boolean)
		Dim txtStagedOrderTareWeight As TextBox = FindControl(currentTransportRowId & "_txtStagedOrderTareWeight")
		Dim ddlTareWeightUofM As DropDownList = FindControl(currentTransportRowId & "_ddlTareWeightUofM")
		Dim tbxTareDate As HtmlInputText = FindControl(currentTransportRowId & "_tbxTransportTareDate")
		Dim originalTareWeight As HtmlInputHidden = FindControl(currentTransportRowId & "_OriginalTareWeight")
		Dim originalTareDate As HtmlInputHidden = FindControl(currentTransportRowId & "_OriginalTareDate")
		Dim originalTareWeightUofM As HtmlInputHidden = FindControl(currentTransportRowId & "_OriginalTareWeightUofM")
		Dim originalTareManual As HtmlInputHidden = FindControl(currentTransportRowId & "_OriginalTareManual")

		txtStagedOrderTareWeight.Text = tareWeight
		originalTareWeight.Value = tareWeight
		ddlTareWeightUofM.SelectedValue = tareUnitId.ToString
		originalTareWeightUofM.Value = tareUnitId.ToString
		originalTareDate.Value = String.Format("{0:G}", tareDate)
		tbxTareDate.Value = tareDate

		originalTareManual.Value = tareManual
	End Sub
#End Region

	Private Sub btnDelete_Click(sender As Object, e As System.EventArgs) Handles btnDelete.Click
		Try
			' Get the current status of the staged order. Do not allow delete if the order is locked
			Dim stagedOder As KaStagedOrder = New KaStagedOrder(Tm2Database.Connection, _stagedOrderInfo.Id)
			If stagedOder.Locked Then
				DisplayJavaScriptMessage("CanNotDeleteOrder", Utilities.JsAlert("Not able to delete this staged order, it is marked as locked."), False)
				ddlStagedOrders_SelectedIndexChanged(ddlStagedOrders, New EventArgs)
				Return
			End If
		Catch ex As RecordNotFoundException
			' suppress
		End Try

		Dim inProgressInfo As KaInProgress
		Dim connection As New OleDbConnection(Tm2Database.GetDbConnection())
		Dim transaction As OleDbTransaction = Nothing
		connection.Open()
		Try
			Dim getTransportWeighmentRdr As OleDb.OleDbDataReader = Tm2Database.ExecuteReader(connection, "SELECT id " &
										 "FROM in_progress " &
										 "WHERE (in_progress.staged_order_id = " & Q(_stagedOrderInfo.Id) & ")")
			If getTransportWeighmentRdr.Read Then
				inProgressInfo = New KaInProgress(connection, getTransportWeighmentRdr.Item("id"))
			Else
				inProgressInfo = New KaInProgress
			End If
			getTransportWeighmentRdr.Close()
			inProgressInfo.GetChildren(connection, Nothing, True)

			'ToDo:Remove completed orders better...

			If inProgressInfo.Weighments.Count > 1 OrElse (inProgressInfo.Weighments.Count > 0 AndAlso inProgressInfo.Weighments(0).Complete) Then
				' There are weighments complete.  Need to create a ticket.
				For Each weighment As KaInProgressWeighment In inProgressInfo.Weighments
					With weighment
						If Not .Complete AndAlso ((.UseDelivered And .Delivered = 0.0) OrElse (Not .UseDelivered And .Gross = 0.0)) Then
							If Not .UseDelivered And .Gross = 0.0 Then
								.Gross = .Tare
								.GrossDate = .TareDate
								.GrossManual = False
							End If

							'Check the product ID
							If .ProductId.Equals(Guid.Empty) Or .BulkProductId.Equals(Guid.Empty) Then
								For Each stagedCompartment As KaStagedOrderCompartment In _stagedOrderInfo.Compartments
									If Not stagedCompartment.Deleted Then
										For Each compItem As KaStagedOrderCompartmentItem In stagedCompartment.CompartmentItems
											If Not compItem.Deleted AndAlso Not compItem.OrderItemId.Equals(Guid.Empty) Then
												.OrderItemId = compItem.OrderItemId
												.StagedOrderCompartmentItemId = compItem.Id
												For Each transComp As KaStagedOrderTransport In _stagedOrderInfo.Transports
													If transComp.Id.Equals(stagedCompartment.TransportCompartmentId) Then
														.TransportId = transComp.TransportId
														Exit For
													End If
												Next
												Exit For
											End If
										Next

										If Not .OrderItemId.Equals(Guid.Empty) Then Exit For
									End If
								Next
								If .OrderItemId.Equals(Guid.Empty) Then
									For Each stagedCompartment As KaStagedOrderCompartment In _stagedOrderInfo.Compartments
										If Not stagedCompartment.Deleted Then
											For Each compItem As KaStagedOrderCompartmentItem In stagedCompartment.CompartmentItems
												If Not compItem.Deleted Then
													For Each stagedOrderOrder As KaStagedOrderOrder In _stagedOrderInfo.Orders
														If Not stagedOrderOrder.Deleted Then
															Try
																Dim orderInfo As New KaOrder(connection, stagedOrderOrder.OrderId)
																For Each entry As KaOrderItem In orderInfo.OrderItems
																	If Not entry.Deleted Then
																		.OrderItemId = entry.Id
																		Exit For
																	End If
																Next
															Catch ex As RecordNotFoundException

															End Try
														End If
														If Not .OrderItemId.Equals(Guid.Empty) Then Exit For
													Next
													If .OrderItemId.Equals(Guid.Empty) Then
														Try
															Dim orderInfo As New KaOrder(connection, _stagedOrderInfo.OrderId)
															For Each entry As KaOrderItem In orderInfo.OrderItems
																If Not entry.Deleted Then
																	.OrderItemId = entry.Id
																	Exit For
																End If
															Next
														Catch ex As RecordNotFoundException

														End Try
													End If
													.StagedOrderCompartmentItemId = compItem.Id
													For Each transComp As KaStagedOrderTransport In _stagedOrderInfo.Transports
														If transComp.Id.Equals(stagedCompartment.TransportCompartmentId) Then
															.TransportId = transComp.TransportId
															Exit For
														End If
													Next
													Exit For
												End If
											Next

											If Not .OrderItemId.Equals(Guid.Empty) Then Exit For
										End If
									Next
								End If
								If .OrderItemId.Equals(Guid.Empty) Then
									For Each stagedOrderOrder As KaStagedOrderOrder In _stagedOrderInfo.Orders
										If Not stagedOrderOrder.Deleted Then
											Try
												Dim orderInfo As New KaOrder(connection, stagedOrderOrder.OrderId)
												For Each entry As KaOrderItem In orderInfo.OrderItems
													If Not entry.Deleted Then
														.OrderItemId = entry.Id
														Exit For
													End If
												Next
											Catch ex As RecordNotFoundException

											End Try
										End If
										If Not .OrderItemId.Equals(Guid.Empty) Then Exit For ' It is set, we can skip checking the rest of the orders
									Next
								End If
								If .OrderItemId.Equals(Guid.Empty) Then
									Try
										Dim orderInfo As New KaOrder(connection, _stagedOrderInfo.OrderId)
										For Each entry As KaOrderItem In orderInfo.OrderItems
											If Not entry.Deleted Then
												.OrderItemId = entry.Id
												Exit For
											End If
										Next
									Catch ex As RecordNotFoundException

									End Try
								End If
								If Not .OrderItemId.Equals(Guid.Empty) Then
									Dim entryInfo As New KaOrderItem(connection, .OrderItemId)
									.ProductId = entryInfo.ProductId
									Dim productInfo As New KaProduct(connection, .ProductId)
									For Each pbp As KaProductBulkProduct In productInfo.ProductBulkItems
										If Not pbp.Deleted AndAlso pbp.LocationId.Equals(_stagedOrderInfo.LocationId) Then
											Try
												Dim bulkProdInfo As KaBulkProduct = New KaBulkProduct(connection, pbp.BulkProductId)
												If Not bulkProdInfo.IsFunction(connection) Then
													.BulkProductId = pbp.BulkProductId
													Exit For
												End If
											Catch ex As RecordNotFoundException
											End Try
										End If
									Next
								End If
							End If
						End If
					End With
				Next
				transaction = connection.BeginTransaction
				inProgressInfo.SqlUpdateInsertIfNotFound(connection, transaction, Database.ApplicationIdentifier, _currentUser.Name)
				' Print a Delivery Ticket
				inProgressInfo.CreateTicket(connection, transaction)
				inProgressInfo.SqlDelete(connection, transaction, Database.ApplicationIdentifier, _currentUser.Name)
			Else
				transaction = connection.BeginTransaction
				If Not inProgressInfo.Id.Equals(Guid.Empty) Then inProgressInfo.SqlDelete(connection, transaction, Database.ApplicationIdentifier, _currentUser.Name)
			End If

			Dim deleteStagedOrderInProgCountRdr As OleDb.OleDbDataReader = Tm2Database.ExecuteReader(connection, transaction, "SELECT COUNT(*) AS InProgCount FROM in_progress WHERE staged_order_id = " & Q(_stagedOrderInfo.Id))
			If deleteStagedOrderInProgCountRdr.Read Then
				Dim count As Integer = deleteStagedOrderInProgCountRdr.Item(0)
				If count = 0 Then
					_stagedOrderInfo.Deleted = True
					_stagedOrderInfo.SqlUpdate(connection, transaction, Database.ApplicationIdentifier, _currentUser.Name)
				End If
			End If
			deleteStagedOrderInProgCountRdr.Close()
			transaction.Commit()
		Catch ex As Exception
			If transaction IsNot Nothing Then transaction.Rollback()
			Throw New Exception(ex.Message, ex)
		Finally
			If transaction IsNot Nothing Then transaction.Dispose()
			connection.Close()
		End Try
		_stagedOrderInfo = New KaStagedOrder()
		PopulateStagedOrdersList(Guid.Empty)
		ddlStagedOrders.SelectedIndex = 0
		pnlStagedTransports.Controls.Clear()
		ddlStagedOrders_SelectedIndexChanged(ddlStagedOrders, New EventArgs)
	End Sub

	Private Sub btnClearLockedStatus_Click(sender As Object, e As System.EventArgs) Handles btnClearLockedStatus.Click
		Try
			Tm2Database.ExecuteNonQuery(GetUserConnection(_currentUser.Id), "UPDATE staged_orders SET locked = 0 WHERE (id = " & Q(_stagedOrderInfo.Id) & ")")
		Catch ex As RecordNotFoundException

		End Try
		ddlStagedOrders_SelectedIndexChanged(ddlStagedOrders, New EventArgs())
	End Sub

	Private Sub btnSetLockedStatus_Click(sender As Object, e As System.EventArgs) Handles btnSetLockedStatus.Click
		Try
			Tm2Database.ExecuteNonQuery(GetUserConnection(_currentUser.Id), "UPDATE staged_orders SET locked = 1 WHERE (id = " & Q(_stagedOrderInfo.Id) & ")")
		Catch ex As RecordNotFoundException

		End Try
		ddlStagedOrders_SelectedIndexChanged(ddlStagedOrders, New EventArgs())
	End Sub

	Private Sub ddlCarrier_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles ddlCarrier.SelectedIndexChanged
		_stagedOrderInfo.CarrierId = Guid.Parse(ddlCarrier.SelectedValue)
		For Each stagedObject As Object In pnlStagedTransports.Controls
			If Not TypeOf stagedObject Is HtmlGenericControl Then Continue For
			Dim transportRow As HtmlGenericControl = stagedObject
			If transportRow.ID IsNot Nothing AndAlso transportRow.ID.Contains("Transport") Then
				' This is a transport row
				Dim transportDropdownList As DropDownList = transportRow.FindControl(transportRow.ID & "_ddlTransport")
				PopulateTransportsList(transportDropdownList, Guid.Parse(transportDropdownList.SelectedValue))
			End If
		Next
	End Sub

	Private Sub btnAddOrderToStagedOrder_Click(sender As Object, e As System.EventArgs) Handles btnAddOrderToStagedOrder.Click
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim currentStagedOrderOrdersList As List(Of KaStagedOrderOrder) = _stagedOrderInfo.Orders

		If lstUnusedOrders.SelectedIndex >= 0 Then
			Dim selectedOrderId As Guid = Guid.Empty

			Guid.TryParse(lstUnusedOrders.SelectedItem.Value, selectedOrderId)
			If Not selectedOrderId.Equals(Guid.Empty) Then
				Dim percentage As Double = 0.0
				For Each currentStagedOrderOrder As KaStagedOrderOrder In currentStagedOrderOrdersList
					percentage += currentStagedOrderOrder.Percentage
				Next
				Try
					Dim orderInfo As New KaOrder(connection, selectedOrderId)
					Dim newStagedOrderOrder As New KaStagedOrderOrder
					With newStagedOrderOrder
						.Acres = orderInfo.Acres
						.ApplicatorId = orderInfo.ApplicatorId
						.OrderId = selectedOrderId
						.Percentage = 100
						.LastUpdated = Now
						.CustomerAccountLocationId = orderInfo.CustomerAccountLocationId
					End With
					currentStagedOrderOrdersList.Add(newStagedOrderOrder)
				Catch ex As RecordNotFoundException

				End Try
				If chkUseOrderPercents.Checked Then 'AndAlso percentage = 100 Then
					' Change all of the percents to their order percentage
					Dim massUnitId As Guid = KaUnit.GetSystemDefaultMassUnitOfMeasure(connection, Nothing)
					Dim locationId As Guid = Guid.Empty
					Dim totalWeight As Double = 0.0
					Dim orderWeight As New Dictionary(Of Guid, Double)
					If Guid.TryParse(ddlFacility.SelectedValue, locationId) Then
						For Each currentStagedOrderOrder As KaStagedOrderOrder In currentStagedOrderOrdersList
							Try
								Dim orderInfo As New KaOrder(connection, currentStagedOrderOrder.OrderId)
								For Each orderItem As KaOrderItem In orderInfo.OrderItems
									If Not orderItem.Deleted Then
										Dim itemAmount As Double = KaUnit.Convert(connection, New KaQuantity(orderItem.Request, orderItem.UnitId), New KaProduct(connection, orderItem.ProductId).GetDensity(locationId), massUnitId).Numeric
										totalWeight += itemAmount
										Try
											orderWeight(orderInfo.Id) += itemAmount
										Catch ex As KeyNotFoundException
											orderWeight.Add(orderInfo.Id, itemAmount)
										End Try
									End If
								Next
							Catch ex As Exception

							End Try
						Next

					End If
					If totalWeight = 0 Then
						For Each currentStagedOrderOrder As KaStagedOrderOrder In currentStagedOrderOrdersList
							orderWeight.Add(currentStagedOrderOrder.OrderId, 1)
							totalWeight += 1
						Next
					End If
					If totalWeight > 0 Then
						Dim totalPercent As Double = 0.0
						Dim largestPercent As Double = 0.0
						Dim largestGuid As Guid = Guid.Empty
						For Each currentStagedOrderOrder As KaStagedOrderOrder In currentStagedOrderOrdersList
							Dim percent As Double = Math.Round((orderWeight(currentStagedOrderOrder.OrderId) / totalWeight) * 100, 2)
							currentStagedOrderOrder.Percentage = percent
							totalPercent += percent
							If largestPercent < percent OrElse largestGuid.Equals(Guid.Empty) Then
								largestPercent = percent
								largestGuid = currentStagedOrderOrder.OrderId
							End If
						Next
						For Each currentStagedOrderOrder As KaStagedOrderOrder In currentStagedOrderOrdersList
							If currentStagedOrderOrder.OrderId.Equals(largestGuid) Then
								currentStagedOrderOrder.Percentage += 100 - totalPercent
								Exit For
							End If
						Next
					End If
				End If
			End If
		End If
		_stagedOrderInfo.Orders = New List(Of KaStagedOrderOrder)(currentStagedOrderOrdersList)
		PopulateOrdersList()
	End Sub

	Private Sub btnRemoveOrderFromStagedOrder_Click(sender As Object, e As System.EventArgs) Handles btnRemoveOrderFromStagedOrder.Click
		If lstUsedOrders.SelectedIndex >= 0 Then
			Dim selectedOrderId As Guid = GetStagedOrderOrder(lstUsedOrders.SelectedItem.Value).OrderId
			Dim orderCounter As Integer = 0
			Do While orderCounter < _stagedOrderInfo.Orders.Count
				If selectedOrderId = _stagedOrderInfo.Orders(orderCounter).OrderId Then
					_stagedOrderInfo.Orders.RemoveAt(orderCounter)
					Exit Do
				End If
				orderCounter += 1
			Loop
		End If

		PopulateOrdersList()
	End Sub

	Protected Sub tbxOrderPercentage_TextChanged(sender As Object, e As EventArgs) Handles tbxOrderPercentage.TextChanged
		If lstUsedOrders.SelectedIndex >= 0 Then
			Dim stagedOrderOrder As KaStagedOrderOrder = GetStagedOrderOrder(lstUsedOrders.SelectedValue)
			For Each currentOrder As KaStagedOrderOrder In _stagedOrderInfo.Orders
				If currentOrder.OrderId = stagedOrderOrder.OrderId Then
					If Not Double.TryParse(tbxOrderPercentage.Text, currentOrder.Percentage) Then
						DisplayJavaScriptMessage("InvalidCharPercent", Utilities.JsAlert("A valid number must be entered for the percentage."))
						Exit Sub
					End If
					Exit For
				End If
			Next
		End If
		PopulateOrdersList()
	End Sub

	Private Sub chkUseOrderPercents_CheckedChanged(sender As Object, e As System.EventArgs) Handles chkUseOrderPercents.CheckedChanged
		PopulateOrdersList()
	End Sub

	Protected Sub ddlOrderApplicator_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlOrderApplicator.SelectedIndexChanged
		If lstUsedOrders.SelectedIndex >= 0 Then
			Dim stagedOrderOrder As KaStagedOrderOrder = GetStagedOrderOrder(lstUsedOrders.SelectedValue)
			For Each currentOrder As KaStagedOrderOrder In _stagedOrderInfo.Orders
				If currentOrder.OrderId = stagedOrderOrder.OrderId Then
					Guid.TryParse(ddlOrderApplicator.SelectedValue.ToString, currentOrder.ApplicatorId)
					Exit For
				End If
			Next
		End If
		PopulateOrdersList()
	End Sub

	Protected Sub tbxOrderAcres_TextChanged(sender As Object, e As EventArgs) Handles tbxOrderAcres.TextChanged
		If lstUsedOrders.SelectedIndex >= 0 Then
			Dim stagedOrderOrder As KaStagedOrderOrder = GetStagedOrderOrder(lstUsedOrders.SelectedValue)
			For Each currentOrder As KaStagedOrderOrder In _stagedOrderInfo.Orders
				If currentOrder.OrderId = stagedOrderOrder.OrderId Then
					If Not Double.TryParse(tbxOrderAcres.Text, currentOrder.Acres) Then
						DisplayJavaScriptMessage("InvalidAcres", Utilities.JsAlert("A valid number must be entered for the acres."))
						Exit Sub
					End If
					Exit For
				End If
			Next
		End If
		PopulateOrdersList()
	End Sub

	Private Sub RemoveOrderItemListHandlers()
		RemoveHandler ddlCustomerSite.SelectedIndexChanged, AddressOf ddlCustomerSite_SelectedIndexChanged
		RemoveHandler tbxOrderPercentage.TextChanged, AddressOf tbxOrderPercentage_TextChanged
		RemoveHandler chkUseOrderPercents.CheckedChanged, AddressOf chkUseOrderPercents_CheckedChanged
		RemoveHandler ddlOrderApplicator.SelectedIndexChanged, AddressOf ddlOrderApplicator_SelectedIndexChanged
	End Sub

	Private Sub AddOrderItemListHandlers()
		AddHandler ddlCustomerSite.SelectedIndexChanged, AddressOf ddlCustomerSite_SelectedIndexChanged
		AddHandler tbxOrderPercentage.TextChanged, AddressOf tbxOrderPercentage_TextChanged
		AddHandler chkUseOrderPercents.CheckedChanged, AddressOf chkUseOrderPercents_CheckedChanged
		AddHandler ddlOrderApplicator.SelectedIndexChanged, AddressOf ddlOrderApplicator_SelectedIndexChanged
	End Sub

	Private Function ConvertStagedOrderToLfLoad(ByRef transportTareList As Dictionary(Of Guid, OriginalTareInfo)) As LfLoad
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		_stagedOrderInfo = ConvertPageToStagedOrder()
		Dim locationId As Guid = Guid.Empty
		Try
			locationId = New KaLocation(connection, Guid.Parse(ddlFacility.SelectedValue)).Id
		Catch ex As RecordNotFoundException
			locationId = _stagedOrderInfo.LocationId
		End Try
		Dim controllerPerPanel As Dictionary(Of Guid, KaController.Controller) = InitializeControllers() ' We need to simulate creating the controllers for the lfLoad
		Return ConvertStagedOrderToLfLoad(connection, _stagedOrderInfo, transportTareList, locationId, controllerPerPanel, chkUseOrderPercents.Checked)
	End Function

	Public Shared Function ConvertStagedOrderToLfLoad(connection As OleDbConnection, ByRef stagedOrderInfo As KaStagedOrder, ByRef transportTareList As Dictionary(Of Guid, OriginalTareInfo), locationId As Guid, controllerPerPanel As Dictionary(Of Guid, KaController.Controller), useOrderPercents As Boolean) As LfLoad

		Dim load As LfLoad = Nothing
		LfDatabase.LocationId = locationId
		load = New LfLoad(locationId, controllerPerPanel)

		With load
			.UseOrderPercents = useOrderPercents
			If stagedOrderInfo.Orders.Count > 0 Then
				For Each n As KaStagedOrderOrder In stagedOrderInfo.Orders
					Dim order As New KaOrder(connection, n.OrderId)
					Dim customerAccountLocation As KaCustomerAccountLocation
					Try ' to load the customer account from the database
						customerAccountLocation = New KaCustomerAccountLocation(connection, n.CustomerAccountLocationId)
					Catch ex As RecordNotFoundException  ' customer account isn't in the database
						customerAccountLocation = New KaCustomerAccountLocation()
					End Try
					Dim applicator As KaApplicator
					Try  ' to load the applicator from the database
						applicator = New KaApplicator(connection, n.ApplicatorId)
					Catch ex As RecordNotFoundException ' applicator isn't in the database
						applicator = New KaApplicator()
					End Try
					.Orders.Add(New LfLoadOrder(order, n.Percentage, customerAccountLocation, applicator, n.Acres))
				Next
			Else
				Dim order As New KaOrder(connection, stagedOrderInfo.OrderId)
				Dim custAcctLocation As KaCustomerAccountLocation
				Try
					custAcctLocation = New KaCustomerAccountLocation(connection, stagedOrderInfo.CustomerAccountLocationId)
				Catch ex As Exception
					custAcctLocation = New KaCustomerAccountLocation()
				End Try
				Dim applicator As New KaApplicator()
				.Orders.Add(New LfLoadOrder(order, 100, custAcctLocation, applicator))
			End If
			.CarrierId = stagedOrderInfo.CarrierId
			.DriverId = stagedOrderInfo.DriverId
			For Each stagedCompartment As KaStagedOrderCompartment In stagedOrderInfo.Compartments ' add the compartments
				Dim compartment As New LfLoadCompartment()
				compartment.Index = .Compartments.Count
				For Each stagedTransport As KaStagedOrderTransport In stagedOrderInfo.Transports ' find the transport that this compartment is in
					If stagedTransport.Id.Equals(stagedCompartment.StagedOrderTransportId) Then  ' this is the compartment
						If Not stagedTransport.TransportId.Equals(Guid.Empty) Then compartment.Transport = New KaTransport(connection, stagedTransport.TransportId)
						Exit For ' no need to iterate through the transports any further
					End If
				Next
				If Not stagedCompartment.TransportCompartmentId.Equals(Guid.Empty) Then ' link the transport compartment
					compartment.TransportCompartment = New KaTransportCompartment(connection, stagedCompartment.TransportCompartmentId)
				End If
				Dim densities As New Dictionary(Of Guid, KaRatio) ' Guid = product ID
				Dim orderItemRequests As New Dictionary(Of Guid, KaQuantity) ' Guid = order item ID
				For Each stagedItem As KaStagedOrderCompartmentItem In stagedCompartment.CompartmentItems ' add the compartment contents
					Try
						Dim order As New KaOrder(connection, stagedItem.OrderItemId)
						stagedItem.OrderItemId = Guid.Empty ' Treat this as a all products, since the shortcuts don't have this functionality
					Catch orderEx As RecordNotFoundException
					End Try
					If stagedItem.OrderItemId.Equals(Guid.Empty) Then
						.SetCompartmentToBlend(compartment, New KaQuantity(stagedItem.Quantity, stagedItem.UnitId), LfLoad.AdjustBy.Original, 100)
					Else
						Dim orderItem As New KaOrderItem(connection, stagedItem.OrderItemId)
						Dim quantity As New KaQuantity(stagedItem.Quantity, stagedItem.UnitId)
						compartment.Entries.Add(New LfLoadCompartmentEntry(locationId, orderItem, quantity, stagedItem.Id))
					End If
				Next
				.Compartments.Add(compartment)
			Next
		End With

		For Each stagedTransport As KaStagedOrderTransport In stagedOrderInfo.Transports
			With stagedTransport
				If Not .Deleted Then
					If Not .TransportId.Equals(Guid.Empty) Then
						Try
							Dim transportInfo As New KaTransport(connection, .TransportId)
							load.TransportTareWeights.Add(transportInfo.Id, New KaTimestampedQuantity(.TareWeight, .TareUnitId, .TaredAt, .TareManual))
						Catch ex As RecordNotFoundException

						End Try
					End If
					transportTareList.Add(.Id, New OriginalTareInfo(.Id, .TareWeight, .TaredAt, .TareUnitId, .TareManual))
				End If
			End With
		Next

		Return load
	End Function

	Private Function InitializeControllers() As Dictionary(Of Guid, KaController.Controller) ' We need to simulate creating the controllers for the lfLoad
		Return InitializeControllers(GetUserConnection(_currentUser.Id))
	End Function

	Public Shared Function InitializeControllers(connection As OleDbConnection) As Dictionary(Of Guid, KaController.Controller) ' We need to simulate creating the controllers for the lfLoad
		Dim controllerPerPanel As New Dictionary(Of Guid, KaController.Controller)

		For Each panel As KaPanel In KaPanel.GetAll(connection, "deleted=0", "")  ' ignore all panels
			KahlerAutomation.KaTm2LoadFramework.LfControllers.IgnoredPanels.Add(panel.Id)
		Next
		Return controllerPerPanel
	End Function

	<Serializable()>
	Public Class OriginalTareInfo
		Public StagedTransportId As Guid = Guid.Empty
		Public OriginalTareInfo As New KaTimestampedQuantity(0, Guid.Empty, Now, False)

		Public Sub New()
		End Sub

		Public Sub New(id As Guid, tareWeight As Double, tareDate As DateTime, tareWeightUofM As Guid, tareManual As Boolean)
			StagedTransportId = id
			OriginalTareInfo = New KaTimestampedQuantity(tareWeight, tareWeightUofM, tareDate, tareManual)
		End Sub
	End Class

	Protected Sub btnUseOriginalOrderQuantity_Click(sender As Object, e As EventArgs) Handles btnUseOriginalOrderQuantityShortcut.Click
		' Try to use the original order(s) request quantity distributed over the existing compartments...
		Dim transportTareList As New Dictionary(Of Guid, OriginalTareInfo)
		Dim load As LfLoad = ConvertStagedOrderToLfLoad(transportTareList)

		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		If LoadHasNonBlendedOrders(load) Then
			load.DistributeLoadInCompartments(LfLoad.AdjustBy.Original, 0)
		Else
			Dim orderTotal As KaQuantity = Nothing
			Dim orderDensity As KaRatio = Nothing
			GetTotalRequestAndDensity(orderTotal, orderDensity, load, connection)
			Dim transportCapacities As New Dictionary(Of Guid, KaQuantity)
			Dim transportCapacitiesFromCompartments As New Dictionary(Of Guid, KaQuantity)
			Dim totalTransportCapacity As KaQuantity = load.GetTransportCapacities(Guid.Empty, orderTotal.UnitId, orderDensity, transportCapacities)
			Dim totalCompartmentCapacity As KaQuantity = load.GetTransportCapacitiesFromCompartments(Guid.Empty, orderTotal.UnitId, orderDensity, transportCapacitiesFromCompartments)
			Dim loadTotal As New KaQuantity(orderTotal.Numeric, orderTotal.UnitId)
			' force the load total to fit in the smallest constraint
			If totalTransportCapacity IsNot Nothing AndAlso totalTransportCapacity.Numeric < loadTotal.Numeric Then loadTotal.Numeric = totalTransportCapacity.Numeric
			If totalCompartmentCapacity IsNot Nothing AndAlso totalCompartmentCapacity.Numeric < loadTotal.Numeric Then loadTotal.Numeric = totalCompartmentCapacity.Numeric
			load.DistributeLoadInCompartments(loadTotal, orderDensity, transportCapacities, transportCapacitiesFromCompartments, LfLoad.AdjustBy.Original, 100)
		End If

		ConvertLfLoadToStagedOrder(connection, load, transportTareList, True)
	End Sub

	Protected Sub btnUseRemainingOrderQuantity_Click(sender As Object, e As EventArgs) Handles btnUseRemainingOrderQuantityShortcut.Click
		Dim transportTareList As New Dictionary(Of Guid, OriginalTareInfo)
		Dim load As LfLoad = ConvertStagedOrderToLfLoad(transportTareList)
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		If (LoadHasNonBlendedOrders(load)) Then
			load.DistributeLoadInCompartments(LfLoad.AdjustBy.Remaining, 0)
		Else
			Dim totalRequested As KaQuantity = Nothing
			Dim orderDensity As KaRatio = Nothing
			GetTotalRequestAndDensity(totalRequested, orderDensity, load, connection)
			If totalRequested IsNot Nothing Then
				Dim totalDelivered As KaQuantity = GetTotalDelivered(load, connection)
				Dim transportCapacities As New Dictionary(Of Guid, KaQuantity)
				Dim transportCapacitiesFromCompartments As New Dictionary(Of Guid, KaQuantity)
				Dim totalTransportCapacity As KaQuantity = load.GetTransportCapacities(Guid.Empty, totalRequested.UnitId, orderDensity, transportCapacities)
				Dim totalCompartmentCapacity As KaQuantity = load.GetTransportCapacitiesFromCompartments(Guid.Empty, totalRequested.UnitId, orderDensity, transportCapacitiesFromCompartments)
				Dim loadTotal As New KaQuantity(Math.Max(totalRequested.Numeric - totalDelivered.Numeric, 0), totalRequested.UnitId)
				' force the load total to fit in the smallest constraint
				If totalTransportCapacity IsNot Nothing AndAlso totalTransportCapacity.Numeric < loadTotal.Numeric Then loadTotal.Numeric = totalTransportCapacity.Numeric
				If totalCompartmentCapacity IsNot Nothing AndAlso totalCompartmentCapacity.Numeric < loadTotal.Numeric Then loadTotal.Numeric = totalCompartmentCapacity.Numeric
				load.DistributeLoadInCompartments(loadTotal, orderDensity, transportCapacities, transportCapacitiesFromCompartments, LfLoad.AdjustBy.Remaining, 100)
			Else
				Throw New Exception("Could not determine total requested")
			End If
		End If

		ConvertLfLoadToStagedOrder(connection, load, transportTareList, True)
	End Sub

	Protected Sub btnUseApplicationRate_Click(sender As Object, e As EventArgs) Handles btnUseApplicationRateShortcut.Click
		Dim transportTareList As New Dictionary(Of Guid, OriginalTareInfo)
		Dim load As LfLoad = ConvertStagedOrderToLfLoad(transportTareList)
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		If (LoadHasNonBlendedOrders(load)) Then
			load.DistributeLoadInCompartments(LfLoad.AdjustBy.ApplicationRate, 0)
		Else
			Dim orderTotal As KaQuantity = Nothing
			Dim orderDensity As KaRatio = Nothing
			GetTotalRequestAndDensity(orderTotal, orderDensity, load, connection)
			Dim acresForOrder As Double = 0
			Dim acresForLoad As Double = 0
			For Each loadOrder As LfLoadOrder In load.Orders
				acresForOrder += loadOrder.Order.Acres
				acresForLoad += loadOrder.Acres
			Next
			Dim factor As Double = 1
			If acresForOrder > 0 Then
				factor = acresForLoad / acresForOrder
			Else
				factor = 1
			End If
			Dim transportCapacities As New Dictionary(Of Guid, KaQuantity)
			Dim transportCapacitiesFromCompartments As New Dictionary(Of Guid, KaQuantity)
			Dim totalTransportCapacity As KaQuantity = load.GetTransportCapacities(Guid.Empty, orderTotal.UnitId, orderDensity, transportCapacities)
			Dim totalCompartmentCapacity As KaQuantity = load.GetTransportCapacitiesFromCompartments(Guid.Empty, orderTotal.UnitId, orderDensity, transportCapacitiesFromCompartments)
			Dim loadTotal As New KaQuantity(orderTotal.Numeric * factor, orderTotal.UnitId)
			' force the load total to fit in the smallest constraint
			If totalTransportCapacity IsNot Nothing AndAlso totalTransportCapacity.Numeric < loadTotal.Numeric Then loadTotal.Numeric = totalTransportCapacity.Numeric
			If totalCompartmentCapacity IsNot Nothing AndAlso totalCompartmentCapacity.Numeric < loadTotal.Numeric Then loadTotal.Numeric = totalCompartmentCapacity.Numeric
			load.DistributeLoadInCompartments(loadTotal, orderDensity, transportCapacities, transportCapacitiesFromCompartments, LfLoad.AdjustBy.ApplicationRate, 100)
		End If

		ConvertLfLoadToStagedOrder(connection, load, transportTareList, True)
	End Sub

	Protected Sub btnUseTransportCapacity_Click(sender As Object, e As EventArgs) Handles btnUseTransportCapacityShortcut.Click
		Dim transportTareList As New Dictionary(Of Guid, OriginalTareInfo)
		Dim load As LfLoad = ConvertStagedOrderToLfLoad(transportTareList)
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		If LoadHasNonBlendedOrders(load) Then
			load.DistributeLoadInCompartments(LfLoad.AdjustBy.Original, 0)
		Else
			Dim totalRequested As KaQuantity = Nothing
			Dim orderDensity As KaRatio = Nothing
			GetTotalRequestAndDensity(totalRequested, orderDensity, load, connection)
			If totalRequested IsNot Nothing Then
				Dim transportCapacities As New Dictionary(Of Guid, KaQuantity)
				Dim transportCapacitiesFromCompartments As New Dictionary(Of Guid, KaQuantity)
				Dim totalTransportCapacity As KaQuantity = load.GetTransportCapacities(Guid.Empty, totalRequested.UnitId, orderDensity, transportCapacities)
				Dim totalCompartmentCapacity As KaQuantity = load.GetTransportCapacitiesFromCompartments(Guid.Empty, totalRequested.UnitId, orderDensity, transportCapacitiesFromCompartments)
				Dim loadTotal As KaQuantity = Nothing
				If totalTransportCapacity IsNot Nothing AndAlso (loadTotal Is Nothing OrElse totalTransportCapacity.Numeric < loadTotal.Numeric) Then loadTotal = totalTransportCapacity
				If totalCompartmentCapacity IsNot Nothing AndAlso (loadTotal Is Nothing OrElse totalCompartmentCapacity.Numeric < loadTotal.Numeric) Then loadTotal = totalCompartmentCapacity
				If loadTotal Is Nothing Then loadTotal = totalRequested
				load.DistributeLoadInCompartments(loadTotal, orderDensity, transportCapacities, transportCapacitiesFromCompartments, LfLoad.AdjustBy.Original, 100)
			Else

			End If
		End If

		ConvertLfLoadToStagedOrder(connection, load, transportTareList, True)
	End Sub

	Protected Sub btnUseBatchQuantityShortcut_Click(sender As Object, e As EventArgs) Handles btnUseBatchQuantityShortcut.Click
		Dim transportTareList As New Dictionary(Of Guid, OriginalTareInfo)
		Dim load As LfLoad = ConvertStagedOrderToLfLoad(transportTareList)
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		If LoadHasNonBlendedOrders(load) Then
			load.DistributeLoadInCompartments(LfLoad.AdjustBy.Original, 0)
		Else
			Dim totalRequested As KaQuantity = Nothing
			Dim orderDensity As KaRatio = Nothing
			GetTotalRequestAndDensity(totalRequested, orderDensity, load, connection)
			Dim totalBatches As Integer = 0
			For Each loadOrder As LfLoadOrder In load.Orders
				totalBatches += Math.Max(loadOrder.Order.RequestedBatches, 1)
			Next
			Dim transportCapacities As New Dictionary(Of Guid, KaQuantity)
			Dim transportCapacitiesFromCompartments As New Dictionary(Of Guid, KaQuantity)
			Dim totalTransportCapacity As KaQuantity = load.GetTransportCapacities(Guid.Empty, totalRequested.UnitId, orderDensity, transportCapacities)
			Dim totalCompartmentCapacity As KaQuantity = load.GetTransportCapacitiesFromCompartments(Guid.Empty, totalRequested.UnitId, orderDensity, transportCapacitiesFromCompartments)
			Dim loadTotal As KaQuantity = New KaQuantity(totalRequested.Numeric / totalBatches, totalRequested.UnitId)
			' force the load total to fit in the smallest constraint
			If totalTransportCapacity IsNot Nothing AndAlso (loadTotal Is Nothing OrElse totalTransportCapacity.Numeric < loadTotal.Numeric) Then loadTotal = totalTransportCapacity
			If totalCompartmentCapacity IsNot Nothing AndAlso (loadTotal Is Nothing OrElse totalCompartmentCapacity.Numeric < loadTotal.Numeric) Then loadTotal = totalCompartmentCapacity
			load.DistributeLoadInCompartments(loadTotal, orderDensity, transportCapacities, transportCapacitiesFromCompartments, LfLoad.AdjustBy.BatchQuantity, 100)
		End If

		ConvertLfLoadToStagedOrder(connection, load, transportTareList, True)
	End Sub

	Protected Sub btnSpecifyTotalQuantityShortcut_Click(sender As Object, e As EventArgs) Handles btnSpecifyTotalQuantityShortcut.Click
		Dim requestedQty As Double = 0.0
		If Not Double.TryParse(tbxSpecifyTotalQuantity.Text, requestedQty) Then
			DisplayJavaScriptMessage("InvalidLoadAmount", Utilities.JsAlert("A valid amount to load must be selected."))
			Exit Sub
		End If
		Dim transportTareList As New Dictionary(Of Guid, OriginalTareInfo)
		Dim load As LfLoad = ConvertStagedOrderToLfLoad(transportTareList)
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim orderTotal As KaQuantity = Nothing
		Dim orderDensity As KaRatio = Nothing
		GetTotalRequestAndDensity(orderTotal, orderDensity, load, connection)
		Dim loadTotal As KaQuantity = KaUnit.Convert(connection, New KaQuantity(requestedQty, Guid.Parse(ddlSpecifyTotalQuantity.SelectedValue)), orderDensity, orderTotal.UnitId)
		If LoadHasNonBlendedOrders(load) Then
			Dim percent As Double = 0
			If orderTotal.Numeric > 0 Then
				percent = 100 * loadTotal.Numeric / orderTotal.Numeric
			Else
				percent = 100
			End If
			If percent <= 0 Then percent = 1
			load.DistributeLoadInCompartments(LfLoad.AdjustBy.Percent, percent)
		Else
			Dim transportCapacities As New Dictionary(Of Guid, KaQuantity)
			Dim transportCapacitiesFromCompartments As New Dictionary(Of Guid, KaQuantity)
			Dim totalTransportCapacity As KaQuantity = load.GetTransportCapacities(Guid.Empty, orderTotal.UnitId, orderDensity, transportCapacities)
			Dim totalCompartmentCapacity As KaQuantity = load.GetTransportCapacitiesFromCompartments(Guid.Empty, orderTotal.UnitId, orderDensity, transportCapacitiesFromCompartments)
			' force the load total to fit in the smallest constraint
			If totalTransportCapacity IsNot Nothing AndAlso (loadTotal Is Nothing OrElse totalTransportCapacity.Numeric < loadTotal.Numeric) Then loadTotal = totalTransportCapacity
			If totalCompartmentCapacity IsNot Nothing AndAlso (loadTotal Is Nothing OrElse totalCompartmentCapacity.Numeric < loadTotal.Numeric) Then loadTotal = totalCompartmentCapacity
			load.DistributeLoadInCompartments(loadTotal, orderDensity, transportCapacities, transportCapacitiesFromCompartments, LfLoad.AdjustBy.Original, 100)
		End If
		ConvertLfLoadToStagedOrder(connection, load, transportTareList, True)
	End Sub

	Private Sub CustomShortcutClicked(sender As Object, e As EventArgs)
		Dim shortcutId As Guid = Guid.Parse(CType(sender, Button).ID.Substring("btnShortcut".Length))
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim customPage As New KaCustomPages(connection, shortcutId)
		Dim stagedOrder = ConvertPageToStagedOrder()
		Dim originalTaresList As New List(Of OriginalTareInfo)
		ConvertToOriginalTareWeights(originalTaresList)
		Dim request As New LfCustomShortcuts.Request()
		request.StagedOrder = stagedOrder
		If customPage.CustomShortcutPrompt = KaCustomPages.CustomShortcutPromptType.QuantityWithUnit Then
			Dim tbxQuantity As TextBox = FindControl("tbxQuantity" & shortcutId.ToString())
			Dim ddlQuantityUnit As DropDownList = FindControl("ddlQuantityUnit" & shortcutId.ToString())

			Dim requestedQty As Double = 0.0
			If Not Double.TryParse(tbxQuantity.Text, requestedQty) Then
				DisplayJavaScriptMessage("InvalidLoadAmount", Utilities.JsAlert("A valid amount to load must be selected."))
				Exit Sub
			End If
			request.Quantity = New KaQuantity(requestedQty, Guid.Parse(ddlQuantityUnit.SelectedValue))
		End If
		_stagedOrderInfo = LfCustomShortcuts.PerformShortcut(customPage.PageURL, request)
		CreateDynamicControls()
		CompartmentItemQuantityChanged(Nothing, Nothing)
	End Sub

	Private Shared Function GetUnit(ByVal connection As OleDbConnection, ByVal unitId As Guid, ByRef units As Dictionary(Of Guid, KaUnit)) As KaUnit
		If Not units.ContainsKey(unitId) Then
			Try
				units.Add(unitId, New KaUnit(connection, unitId))
			Catch ex As RecordNotFoundException
				Return New KaUnit
			End Try
		End If
		Return units(unitId)
	End Function

	Private Function LoadHasNonBlendedOrders(ByVal load As LfLoad) As Boolean
		For Each order As LfLoadOrder In load.Orders
			If order.Order.DoNotBlend Then Return True
		Next
		Return False
	End Function

	Private Sub GetTotalRequestAndDensity(ByRef total As KaQuantity, ByRef density As KaRatio, ByVal load As LfLoad, ByVal connection As OleDbConnection)
		' determine total requested for all orders
		Dim defaultMassUnitId As Guid = KaUnit.GetSystemDefaultMassUnitOfMeasure(connection, Nothing)
		Dim defaultVolumeUnitId As Guid = KaUnit.GetSystemDefaultVolumeUnitOfMeasure(connection, Nothing)
		Dim totalMass As New KaQuantity(0, defaultMassUnitId)
		Dim totalVolume As New KaQuantity(0, defaultVolumeUnitId)
		For Each loadOrder As LfLoadOrder In load.Orders
			If totalMass IsNot Nothing Then
				Try  ' to add this order's request quantity to the total...
					totalMass.Numeric += loadOrder.Order.GetRequested(load.LocationId, defaultMassUnitId, connection).Numeric
				Catch unitEx As UnitConversionException ' couldn't convert all requests to the default mass unit of measure...
					totalMass = Nothing ' invalidate the total mass
				Catch recordEx As RecordNotFoundException  ' couldn't load all order items
					totalMass = Nothing ' invalidate the total mass and volume
					totalVolume = Nothing
				End Try
			End If
			If totalVolume IsNot Nothing Then
				Try  ' to add this order's request quantity to the total...
					totalVolume.Numeric += loadOrder.Order.GetRequested(load.LocationId, defaultVolumeUnitId, connection).Numeric
				Catch unitEx As UnitConversionException ' couldn't convert all requests to the default volume unit of measure...
					totalVolume = Nothing ' invalidate the total volume
				Catch recordEx As RecordNotFoundException ' couldn't load all order items
					totalMass = Nothing ' invalidate the total mass and volume
					totalVolume = Nothing
				End Try
			End If
		Next
		If totalMass IsNot Nothing AndAlso totalVolume IsNot Nothing AndAlso totalVolume.Numeric > 0 Then ' calculate the order density from the totals
			density = New KaRatio(totalMass.Numeric / totalVolume.Numeric, totalMass.UnitId, totalVolume.UnitId)
		Else ' order density isn't available
			density = New KaRatio(0, defaultMassUnitId, defaultVolumeUnitId)
		End If
		total = IIf(totalMass IsNot Nothing, totalMass, totalVolume)
	End Sub

	Private Function GetTotalDelivered(ByVal load As LfLoad, ByVal connection As OleDbConnection) As KaQuantity
		' determine total requested for all orders
		Dim defaultMassUnitId As Guid = KaUnit.GetSystemDefaultMassUnitOfMeasure(connection, Nothing)
		Dim defaultVolumeUnitId As Guid = KaUnit.GetSystemDefaultVolumeUnitOfMeasure(connection, Nothing)
		Dim totalMass As New KaQuantity(0, defaultMassUnitId)
		Dim totalVolume As New KaQuantity(0, defaultVolumeUnitId)
		For Each loadOrder As LfLoadOrder In load.Orders
			If totalMass IsNot Nothing Then
				Try  ' to add this order's request quantity to the total...
					totalMass.Numeric += loadOrder.Order.GetDelivered(load.LocationId, defaultMassUnitId, connection).Numeric
				Catch unitEx As UnitConversionException ' couldn't convert all requests to the default mass unit of measure...
					totalMass = Nothing ' invalidate the total mass
				Catch recordEx As RecordNotFoundException ' couldn't load all order items
					totalMass = Nothing ' invalidate the total mass and volume
					totalVolume = Nothing
				End Try
			End If
			If totalVolume IsNot Nothing Then
				Try  ' to add this order's request quantity to the total...
					totalVolume.Numeric += loadOrder.Order.GetDelivered(load.LocationId, defaultVolumeUnitId, connection).Numeric
				Catch unitEx As UnitConversionException ' couldn't convert all requests to the default volume unit of measure...
					totalVolume = Nothing ' invalidate the total volume
				Catch recordEx As RecordNotFoundException ' couldn't load all order items
					totalMass = Nothing ' invalidate the total mass and volume
					totalVolume = Nothing
				End Try
			End If
		Next
		Return IIf(totalMass IsNot Nothing, totalMass, totalVolume)
	End Function

	Private Sub ConvertLfLoadToStagedOrder(ByVal connection As OleDbConnection, ByVal load As LfLoad, ByVal transportTareList As Dictionary(Of Guid, OriginalTareInfo), ByVal roundEntriesToUnitPrecision As Boolean)
		Dim currentStagedOrder As KaStagedOrder = ConvertPageToStagedOrder()
		Dim originalTaresList As New List(Of OriginalTareInfo)
		ConvertToOriginalTareWeights(originalTaresList)
		_stagedOrderInfo = ConvertLfLoadToStagedOrder(connection, load, currentStagedOrder, transportTareList, roundEntriesToUnitPrecision, chkUseOrderPercents.Checked)
		CreateDynamicControls()
		CompartmentItemQuantityChanged(Nothing, Nothing)
	End Sub

	Public Shared Function ConvertLfLoadToStagedOrder(ByVal connection As OleDbConnection, ByVal load As LfLoad, currentStagedOrder As KaStagedOrder, ByVal transportTareList As Dictionary(Of Guid, OriginalTareInfo), ByVal roundEntriesToUnitPrecision As Boolean, useOrderPercents As Boolean) As KaStagedOrder

		Dim _stagedOrderInfo As KaStagedOrder = load.MakeStagedOrder(currentStagedOrder.Id)

		Dim compartmentMassValid As Boolean = True
		Dim orderInfoList As New List(Of KaOrder)
		For Each order As KaStagedOrderOrder In currentStagedOrder.Orders
			If Not order.Deleted Then
				Try
					orderInfoList.Add(New KaOrder(connection, order.OrderId))
				Catch ex As Exception

				End Try
			End If
		Next
		Dim massUnitOfMeasure As New KaUnit(connection, KaUnit.GetSystemDefaultMassUnitOfMeasure(connection, Nothing))
		Dim volumeUnitOfMeasure As KaUnit = New KaUnit(connection, KaUnit.GetSystemDefaultVolumeUnitOfMeasure(connection, Nothing))

		' Copy Compartment content info over

		Dim compartmentCounter As Integer = 0
		Dim currentStagedCompartmentCounter As Integer = 0

		Do While compartmentCounter < _stagedOrderInfo.Compartments.Count
			Dim currentStagedCompartment As KaStagedOrderCompartment = Nothing
			Do While currentStagedCompartment Is Nothing AndAlso currentStagedCompartmentCounter < currentStagedOrder.Compartments.Count
				If Not currentStagedOrder.Compartments(currentStagedCompartmentCounter).Deleted Then
					currentStagedCompartment = currentStagedOrder.Compartments(currentStagedCompartmentCounter)
				End If
				currentStagedCompartmentCounter += 1
			Loop
			If currentStagedCompartment Is Nothing Then
				'Create a new one
				currentStagedCompartment = New KaStagedOrderCompartment
				currentStagedCompartment.Id = Guid.NewGuid
				currentStagedOrder.Compartments.Add(currentStagedCompartment)
				currentStagedCompartmentCounter += 1
			End If
			With _stagedOrderInfo.Compartments(compartmentCounter)
				currentStagedCompartment.StagedOrderId = currentStagedOrder.Id
				currentStagedCompartment.DischargeLocationId = .DischargeLocationId
				currentStagedCompartment.Position = .Position
				currentStagedCompartment.StagedOrderTransportId = .StagedOrderTransportId
				currentStagedCompartment.TransportCompartmentId = .TransportCompartmentId

				'Set all of the compartment items
				For Each currentStagedCompartmentItem As KaStagedOrderCompartmentItem In currentStagedCompartment.CompartmentItems
					currentStagedCompartmentItem.Quantity = 0.0
				Next
				Dim compartmentItemCounter As Integer = 0
				Dim currentStagedCompartmentItemCounter As Integer = 0

				Do While compartmentItemCounter < .CompartmentItems.Count
					Dim currentStagedCompartmentItem As KaStagedOrderCompartmentItem = Nothing
					Do While currentStagedCompartmentItem Is Nothing AndAlso currentStagedCompartmentItemCounter < currentStagedCompartment.CompartmentItems.Count
						If Not currentStagedCompartment.CompartmentItems(currentStagedCompartmentItemCounter).Deleted Then
							currentStagedCompartmentItem = currentStagedCompartment.CompartmentItems(currentStagedCompartmentItemCounter)
						End If
						If Not useOrderPercents Then currentStagedCompartmentItemCounter += 1
					Loop
					If currentStagedCompartmentItem Is Nothing Then
						'Create a new one
						currentStagedCompartmentItem = New KaStagedOrderCompartmentItem
						currentStagedCompartmentItem.Id = Guid.NewGuid
						currentStagedCompartment.CompartmentItems.Add(currentStagedCompartmentItem)
						currentStagedCompartmentItem.UnitId = .CompartmentItems(compartmentItemCounter).UnitId
						currentStagedCompartmentItemCounter += 1
					End If
					With .CompartmentItems(compartmentItemCounter)
						If useOrderPercents Then
							currentStagedCompartmentItem.OrderItemId = Guid.Empty
						Else
							currentStagedCompartmentItem.OrderItemId = .OrderItemId
						End If
						currentStagedCompartmentItem.Position = .Position

						Dim compartmentUofM As New KaUnit(connection, currentStagedCompartmentItem.UnitId)
						Dim loadCompUofM As New KaUnit(connection, .UnitId)

						currentStagedCompartmentItem.Quantity += GetCompartmentQuantity(connection, currentStagedCompartmentItem.OrderItemId, .Quantity, orderInfoList, load.LocationId, compartmentUofM, massUnitOfMeasure, volumeUnitOfMeasure, loadCompUofM, compartmentMassValid)

						currentStagedCompartmentItem.StagedOrderCompartmentId = .StagedOrderCompartmentId
					End With

					compartmentItemCounter += 1
				Loop

				' Clean up any unused compartment items
				If useOrderPercents Then currentStagedCompartmentItemCounter += 1
				Do While currentStagedCompartmentItemCounter < currentStagedCompartment.CompartmentItems.Count
					currentStagedCompartment.CompartmentItems.RemoveAt(currentStagedCompartmentItemCounter)
				Loop
			End With

			compartmentCounter += 1
		Loop

		' Clean up any unused compartments
		Do While currentStagedCompartmentCounter < currentStagedOrder.Compartments.Count
			currentStagedOrder.Compartments.RemoveAt(currentStagedCompartmentCounter)
		Loop

		Dim transportCounter As Integer = 0
		Dim currentStagedTransportCounter As Integer = 0
		Do While transportCounter < _stagedOrderInfo.Transports.Count
			Dim currentStagedTransport As KaStagedOrderTransport = Nothing
			Do While currentStagedTransport Is Nothing AndAlso currentStagedTransportCounter < currentStagedOrder.Transports.Count
				If Not currentStagedOrder.Transports(currentStagedTransportCounter).Deleted Then
					currentStagedTransport = currentStagedOrder.Transports(currentStagedTransportCounter)
				End If
				currentStagedTransportCounter += 1
			Loop
			If currentStagedTransport Is Nothing Then
				'Create a new one
				currentStagedTransport = New KaStagedOrderTransport
				currentStagedTransport.Id = Guid.NewGuid
				currentStagedOrder.Transports.Add(currentStagedTransport)
				currentStagedTransportCounter += 1
			End If
			With _stagedOrderInfo.Transports(transportCounter)
				currentStagedTransport.StagedOrderId = currentStagedOrder.Id
				currentStagedTransport.TaredAt = .TaredAt
				currentStagedTransport.TareManual = .TareManual
				currentStagedTransport.TareUnitId = .TareUnitId
				currentStagedTransport.TareWeight = .TareWeight
				If transportTareList.ContainsKey(currentStagedTransport.Id) Then
					With transportTareList(currentStagedTransport.Id).OriginalTareInfo
						currentStagedTransport.TaredAt = .Timestamp
						currentStagedTransport.TareManual = .Manual
						currentStagedTransport.TareUnitId = .UnitId
						currentStagedTransport.TareWeight = .Numeric
					End With
				End If
				currentStagedTransport.TransportId = .TransportId
				' Set all of the staged compartments to the correct staged transport Id
				For Each stagedCompartment As KaStagedOrderCompartment In currentStagedOrder.Compartments
					If stagedCompartment.StagedOrderTransportId.Equals(.Id) Then
						stagedCompartment.StagedOrderTransportId = currentStagedTransport.Id
					End If
				Next
			End With

			transportCounter += 1
		Loop

		' Clean up any unused transports
		Do While currentStagedTransportCounter < currentStagedOrder.Transports.Count
			currentStagedOrder.Transports.RemoveAt(currentStagedTransportCounter)
		Loop
		If roundEntriesToUnitPrecision Then
			Dim units As New Dictionary(Of Guid, KaUnit)
			For Each compartment As KaStagedOrderCompartment In currentStagedOrder.Compartments
				For Each compItem As KaStagedOrderCompartmentItem In compartment.CompartmentItems
					Try
						compItem.Quantity = Double.Parse(Format(compItem.Quantity, GetUnit(connection, compItem.UnitId, units).UnitPrecision))
					Catch ex As Exception

					End Try
				Next
			Next
		End If
		_stagedOrderInfo = currentStagedOrder
		Return _stagedOrderInfo
	End Function

	Private Sub SetTextboxMaxLengths()
		tbxNotes.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaStagedOrder.TABLE_NAME, "notes"))
		tbxOrderAcres.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaStagedOrderOrder.TABLE_NAME, KaStagedOrderOrder.FN_ACRES))
		tbxOrderPercentage.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaStagedOrder.TABLE_NAME, KaStagedOrderOrder.FN_PERCENTAGE))
		tbxSpecifyTotalQuantity.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaStagedOrderCompartmentItem.TABLE_NAME, "quantity"))
	End Sub

	Protected Sub btnFind_Click(sender As Object, e As EventArgs) Handles btnFind.Click
		PopulateOrdersList()
	End Sub

	Private Sub btnPointOfSale_Click(sender As Object, e As System.EventArgs) Handles btnPointOfSale.Click
		Try
			' Get the current status of the staged order. Do not allow delete if the order is locked
			Dim stagedOrder As KaStagedOrder = New KaStagedOrder(Tm2Database.Connection, _stagedOrderInfo.Id)
			If stagedOrder.Locked Then
				DisplayJavaScriptMessage("CanNotCreatePointOfSale", Utilities.JsAlert("Not able to create a point of sale for this staged order, it is marked as locked."), False)
				ddlStagedOrders_SelectedIndexChanged(ddlStagedOrders, New EventArgs)
				Exit Sub
			ElseIf stagedOrder.Complete Then
				Dim inProgress As ArrayList = KaInProgress.GetAll(Tm2Database.Connection, $"{KaInProgress.FN_STAGED_ORDER_ID}={Q(stagedOrder.Id)}", "")
				If inProgress.Count > 0 Then
					Dim dispensingApplication As String
					If stagedOrder.SentToPcName.Length > 0 Then
						dispensingApplication = stagedOrder.SentToPcName
					ElseIf CType(inProgress(0), KaInProgress).UsedBy.Length > 0 Then
						dispensingApplication = CType(inProgress(0), KaInProgress).UsedBy
					Else
						dispensingApplication = "a dispensing application"
					End If
					DisplayJavaScriptMessage("CanNotCreatePointOfSale", Utilities.JsAlert($"Not able to create a point of sale for this staged order, it is has been processed by {dispensingApplication}."), False)
					ddlStagedOrders_SelectedIndexChanged(ddlStagedOrders, New EventArgs)
					Exit Sub
				End If
			End If
		Catch ex As RecordNotFoundException
			' suppress
		End Try

		Dim stagedOrderId As Guid = Guid.Empty
		If SaveStagedOrder(stagedOrderId) Then
			DisplayJavaScriptMessage("CreatePointOfSale", $"window.location.href = 'PointOfSale.aspx?StagedOrderId={stagedOrderId.ToString()}'", True)
		End If
	End Sub

	Protected Sub rblTicketCreationSource_SelectedIndexChanged(sender As Object, e As EventArgs) Handles rblTicketCreationSource.SelectedIndexChanged
		_stagedOrderInfo.LossInWeight = rblTicketCreationSource.SelectedValue = "TicketsAreCreatedForAmountUsedOffsite"
		CreateDynamicControls()
	End Sub

	Protected Sub btnPrintPickticket_Click(sender As Object, e As EventArgs) Handles btnPrintPickticket.Click
		Dim stagedOrderId As Guid = Guid.Parse(ddlStagedOrders.SelectedValue)
		If SaveStagedOrder(stagedOrderId) Then
			DisplayJavaScriptMessage("PrintPickticket", Utilities.JsWindowOpen("PickTicket.aspx?staged_order_id=" & stagedOrderId.ToString & "&pfv=true"), False) ', "toolbar=yes,menubar=yes,scrollbars=yes,resizable=yes,width=700,height=500,top=50,left=50", True)) 'Open new window to view order to be printed.
		End If
	End Sub

	Private Sub SetControlUsabilityFromPermissions()
		With _currentUserPermission(_currentTableName)
			Dim shouldEnable = (.Edit AndAlso ddlStagedOrders.SelectedIndex > 0) OrElse (.Create AndAlso ddlStagedOrders.SelectedIndex = 0)
			pnlGeneralOrderInformation.Enabled = shouldEnable
			btnPointOfSale.Enabled = shouldEnable
			btnSave.Enabled = shouldEnable
		End With
	End Sub

	Protected Sub ToolkitScriptManager1_AsyncPostBackError(ByVal sender As Object, ByVal e As System.Web.UI.AsyncPostBackErrorEventArgs)
		ToolkitScriptManager1.AsyncPostBackErrorMessage = e.Exception.Message
	End Sub

	Private Sub DisplayJavaScriptMessage(key As String, script As String)
		DisplayJavaScriptMessage(key, script, False)
	End Sub

	Private Sub DisplayJavaScriptMessage(key As String, script As String, addScriptTags As Boolean)
		ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), key, script, addScriptTags)
	End Sub
End Class