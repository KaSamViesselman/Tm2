Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb
Imports System.IO

Public Class PointOfSale : Inherits System.Web.UI.Page

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
	Private _currentTableName As String
	Private _currentOrderItemIds As New List(Of Guid)
	Private _currentProductIds As New List(Of Guid)
	Private _stagedOrderInfo As New KaStagedOrder
	Private _originalTareValues As New List(Of StagedOrders.OriginalTareInfo)
	Private _products As New Dictionary(Of Guid, KaProduct)
	Private _productIdOrderItemIdMap As New Dictionary(Of Guid, Guid)
	Private _orderItems As New Dictionary(Of Guid, KaOrderItem)
	Private _units As New Dictionary(Of Guid, KaUnit)
	Private _originalCompartmentItems As New Dictionary(Of Guid, KaStagedOrderCompartmentItem)
	Private _customQuestionFieldData As New List(Of KaCustomLoadQuestionData)
	Private _bayPreloadQuestions As List(Of KaCustomLoadQuestionFields)
	Private _bayPostloadQuestions As List(Of KaCustomLoadQuestionFields)
	Private _customLoadQuestionsAvailable As List(Of Guid)
	Private _selectedSourceStorageLocations As Dictionary(Of String, List(Of String))

	Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
		Response.Cache.SetCacheability(HttpCacheability.NoCache)
		_currentUser = Utilities.GetUser(Me)
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim currentUserPermission As String = ""
		If Page.Request("StagedOrderId") IsNot Nothing Then
			_currentTableName = KaStagedOrder.TABLE_NAME
			_currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "StagedOrders")
			Me.Title = "Orders : Staged Orders"
		ElseIf Page.Request("OrderId") IsNot Nothing Then
			_currentTableName = KaOrder.TABLE_NAME
			_currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Orders")
			Me.Title = "Orders : Orders"
		Else
			Response.Redirect("Orders.aspx")
		End If

		If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")
		lblStatus.Text = ""

		If Not Page.IsPostBack Then
			btnCreateTicket.Attributes.Add("onclick", " this.disabled = true; " & ClientScript.GetPostBackEventReference(btnCreateTicket, Nothing) & ";")
			Try
				_customLoadQuestionsAvailable = Tm2Database.FromXml(KaSetting.GetSetting(connection, OrderSettings.SN_POS_PRELOAD_QUESTIONS_AVAILABLE, ""), GetType(List(Of Guid)))
			Catch ex As Exception
				_customLoadQuestionsAvailable = New List(Of Guid)
			End Try
			Try
				_customLoadQuestionsAvailable.AddRange(Tm2Database.FromXml(KaSetting.GetSetting(connection, OrderSettings.SN_POS_POSTLOAD_QUESTIONS_AVAILABLE, ""), GetType(List(Of Guid))))
			Catch ex As Exception
			End Try

			If Page.Request("StagedOrderId") IsNot Nothing Then
				_stagedOrderInfo = New KaStagedOrder(connection, Guid.Parse(Page.Request("StagedOrderId")))
				_stagedOrderInfo.GetChildren(connection, Nothing, True)
				Dim controllerPerPanel As Dictionary(Of Guid, KaController.Controller) = StagedOrders.InitializeControllers(connection) ' We need to simulate creating the controllers for the lfLoad
				'Dim load As KaTm2LoadFramework.LfLoad = New KaTm2LoadFramework.LfLoad(_stagedOrderInfo.LocationId, controllerPerPanel)
				'_stagedOrderInfo = load.MakeStagedOrder(Guid.Parse(Page.Request("StagedOrderId")))
				Dim transportTareList As New Dictionary(Of Guid, StagedOrders.OriginalTareInfo)
				Dim bayId As Guid = _stagedOrderInfo.BayId
				Dim load As KaTm2LoadFramework.LfLoad = StagedOrders.ConvertStagedOrderToLfLoad(connection, _stagedOrderInfo, transportTareList, _stagedOrderInfo.LocationId, controllerPerPanel, _stagedOrderInfo.UseOrderPercents)
				_stagedOrderInfo = StagedOrders.ConvertLfLoadToStagedOrder(connection, load, _stagedOrderInfo, transportTareList, False, _stagedOrderInfo.UseOrderPercents)
				If _stagedOrderInfo.BayId.Equals(Guid.Empty) Then _stagedOrderInfo.BayId = bayId
			ElseIf Page.Request("OrderId") IsNot Nothing Then
				Dim orderInfo As New KaOrder(connection, Guid.Parse(Page.Request("OrderId")))
				_stagedOrderInfo = New KaStagedOrder()
				With _stagedOrderInfo
					.Id = Guid.NewGuid
					Dim newStagedOrderOrder As New KaStagedOrderOrder
					With newStagedOrderOrder
						.CustomerAccountLocationId = orderInfo.CustomerAccountLocationId
						.OrderId = orderInfo.Id
						.Percentage = 100
						.ApplicatorId = orderInfo.ApplicatorId
						.StagedOrderId = _stagedOrderInfo.Id
					End With
					.Orders.Add(newStagedOrderOrder)
					Dim newStagedOrderTransport As New KaStagedOrderTransport
					With newStagedOrderTransport
						.Id = Guid.NewGuid
						.StagedOrderId = _stagedOrderInfo.Id
					End With
					.Transports.Add(newStagedOrderTransport)
					Dim newStagedOrderCompartment As New KaStagedOrderCompartment
					With newStagedOrderCompartment
						.Id = Guid.NewGuid
						For Each orderItem As KaOrderItem In orderInfo.OrderItems
							If orderItem.Deleted Then Continue For
							Dim newStagedOrderCompartmentItem As New KaStagedOrderCompartmentItem
							With newStagedOrderCompartmentItem
								.Id = Guid.NewGuid
								.OrderItemId = orderItem.Id
								.Position = newStagedOrderCompartment.CompartmentItems.Count
								.Quantity = Math.Max(0, orderItem.Request - orderItem.Delivered)
								.UnitId = orderItem.UnitId
								.StagedOrderCompartmentId = newStagedOrderCompartment.Id
							End With
							.CompartmentItems.Add(newStagedOrderCompartmentItem)
						Next
						.Position = 0
						.StagedOrderId = _stagedOrderInfo.Id
						.StagedOrderTransportId = _stagedOrderInfo.Transports(0).Id
						.TransportCompartmentId = Guid.Empty
					End With
					.Compartments.Add(newStagedOrderCompartment)
					.Source = String.Format("{0}/{1}", System.Net.Dns.GetHostName(), "TM2")
				End With
			End If

			_originalCompartmentItems = New Dictionary(Of Guid, KaStagedOrderCompartmentItem)()
			For Each stagedComp As KaStagedOrderCompartment In _stagedOrderInfo.Compartments
				If Not stagedComp.Deleted Then

					For Each stagedCompItem As KaStagedOrderCompartmentItem In stagedComp.CompartmentItems
						If Not stagedCompItem.Deleted Then
							_originalCompartmentItems.Add(stagedCompItem.Id, stagedCompItem.Clone())
						End If
					Next
				End If
			Next

			SetTextboxMaxLengths()
			Dim defaultWeightUnitId As Guid = KaUnit.GetSystemDefaultMassUnitOfMeasure(connection, Nothing)
			GetCustomQuestionInspectionQuestionsList()

			PopulateStagedOrderInfo()
		End If
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
			transportCompartmentDropdownList.Items.Add(New ListItem((transportInfo.Position + 1).ToString.Trim & IIf(capacity.Trim.Length > 0, " (" & capacity.Trim & ")", ""), transportInfo.Id.ToString))
			If transportInfo.Id.ToString = transportCompartmentId.ToString Then
				transportCompartmentDropdownList.SelectedIndex = transportCompartmentDropdownList.Items.Count - 1
				transportFound = True
			End If
		Next

		If Not transportFound Then
			transportCompartmentDropdownList.SelectedIndex = 0
		End If
	End Sub

	Private _currentSourceStorageLocations As Dictionary(Of Guid, String)
	Private ReadOnly Property CurrentSourceStorageLocations As Dictionary(Of Guid, String)
		Get
			If _currentSourceStorageLocations Is Nothing Then
				Dim locationId As Guid = Guid.Empty
				If ddlFacility.SelectedIndex > 0 Then Guid.TryParse(ddlFacility.SelectedValue, locationId)
				PopulateStorageLocationsForFacility(locationId)
			End If
			Return _currentSourceStorageLocations
		End Get
	End Property

	Private _currentDestinationStorageLocations As Dictionary(Of Guid, String)
	Private ReadOnly Property CurrentDestinationStorageLocations As Dictionary(Of Guid, String)
		Get
			If _currentDestinationStorageLocations Is Nothing Then
				Dim locationId As Guid = Guid.Empty
				If ddlFacility.SelectedIndex > 0 Then Guid.TryParse(ddlFacility.SelectedValue, locationId)
				PopulateStorageLocationsForFacility(locationId)
			End If
			Return _currentDestinationStorageLocations
		End Get
	End Property

	Private Sub PopulateCompartmentSourceStorageLocationList(ByRef transportCompartmentDischargeStorageLocationList As CheckBoxList, compartmentRowID As String)
		transportCompartmentDischargeStorageLocationList.Items.Clear()

		Dim selectedList As List(Of String)
		If _selectedSourceStorageLocations IsNot Nothing AndAlso _selectedSourceStorageLocations.ContainsKey(compartmentRowID) Then
			selectedList = _selectedSourceStorageLocations(compartmentRowID)
		Else
			selectedList = New List(Of String)
		End If
		For Each slId As Guid In CurrentSourceStorageLocations.Keys
			Dim li As ListItem = New ListItem(_currentSourceStorageLocations(slId), slId.ToString())
			transportCompartmentDischargeStorageLocationList.Items.Add(li)
			li.Selected = selectedList.Contains(slId.ToString())
		Next
	End Sub

	Private Sub PopulatetDischargeStorageLocationList()
		cblDischargeStorageLocations.Items.Clear()
		For Each slId As Guid In CurrentDestinationStorageLocations.Keys
			Dim li As ListItem = New ListItem(_currentDestinationStorageLocations(slId), slId.ToString())
			cblDischargeStorageLocations.Items.Add(li)
			li.Selected = False
		Next
		pnlReceiveIntoStorageLocation.Visible = _currentDestinationStorageLocations.Count > 0
	End Sub

	Private Sub PopulateDriversList(ByVal currentDriverId As Guid, ByVal currentOrders As List(Of KaStagedOrderOrder))
		Dim driverFound As Boolean = False
		With ddlDriver
			Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
			.Items.Clear()
			.Items.Add(New ListItem("Driver not selected", Guid.Empty.ToString)) ' populate the driver list
			Dim limitDriversToDriversAssignedToAccount As Boolean = False
			Boolean.TryParse(KaSetting.GetSetting(connection, "PointOfSale/LimitDriversToDriversAssignedToAccount", False), limitDriversToDriversAssignedToAccount)
			Dim drivers As New List(Of Guid) ' get a list of drivers that are associated with the list of accounts
			drivers.Add(currentDriverId)
			If limitDriversToDriversAssignedToAccount Then
				For Each driverAllowedForAll As KaDriver In KaDriver.GetAll(connection, "(disabled = 0) AND (deleted = 0) AND (valid_for_all_accounts = 1)", "")
					drivers.Add(driverAllowedForAll.Id)
				Next

				Dim accounts As New ArrayList() ' get a list of accounts associated with the selected order
				For Each stagedOrderOrder As KaStagedOrderOrder In currentOrders
					For Each orderAccount As KaOrderCustomerAccount In KaOrderCustomerAccount.GetAll(connection, "deleted = 0 AND order_id = " & Q(stagedOrderOrder.OrderId), "")
						accounts.Add(orderAccount.CustomerAccountId)
					Next
				Next
				For Each accountId As Guid In accounts
					For Each accountDriver As KaCustomerAccountDriver In KaCustomerAccountDriver.GetAll(connection, "deleted = 0 AND customer_account_id =" & Q(accountId), "")
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
					If driverAvailable.Id.ToString = currentDriverId.ToString Then
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
				If carrierInfo.Id.ToString = currentCarrierId.ToString Then
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
				If facilityInfo.Id.ToString = currentFacilityId.ToString Then
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

	Private Sub PopulateBaysList(ByVal currentBayId As Guid)
		Dim bayFound As Boolean = False
		Dim facilityId As Guid = Guid.Empty
		Guid.TryParse(ddlFacility.SelectedValue, facilityId)
		With ddlBayAssigned
			.Items.Clear()

			.Items.Add(New ListItem("Bay not selected", Guid.Empty.ToString))

			Dim bayList As ArrayList = KaBay.GetAll(GetUserConnection(_currentUser.Id), " ((location_id = " & Q(facilityId) & ") AND (deleted = 0)) OR (id = " & Q(currentBayId) & ")", "name ASC")
			For Each bayInfo As KaBay In bayList
				.Items.Add(New ListItem(bayInfo.Name.Trim, bayInfo.Id.ToString))
				If bayInfo.Id.ToString = currentBayId.ToString Then
					.SelectedIndex = .Items.Count - 1
					bayFound = True
				End If
			Next

			If Not bayFound Then
				.SelectedIndex = 0
			End If
		End With
		ddlBayAssigned_SelectedIndexChanged(ddlBayAssigned, New EventArgs())
	End Sub

	Private Sub PopulatePanelsList()
		Dim currentPanel As String = Guid.Empty.ToString
		If ddlPanel.SelectedIndex >= 0 Then currentPanel = ddlPanel.SelectedValue
		ddlPanel.Items.Clear()
		ddlPanel.Items.Add(New ListItem("", Guid.Empty.ToString))
		Dim whereClause As String = "deleted=0"
		Try
			whereClause &= " AND location_id=" & Q(New KaLocation(GetUserConnection(_currentUser.Id), Guid.Parse(ddlFacility.SelectedValue)).Id)
		Catch ex As Exception

		End Try
		For Each u As KaPanel In KaPanel.GetAll(GetUserConnection(_currentUser.Id), whereClause, "name ASC")
			ddlPanel.Items.Add(New ListItem(u.Name, u.Id.ToString))
		Next
		Try
			ddlPanel.SelectedValue = currentPanel
		Catch ex As ArgumentOutOfRangeException

		End Try
	End Sub

	Private Sub PopulateApplicatorsList(ByVal currentApplicatorId As Guid)
		Dim applicatorFound As Boolean = False
		With ddlOrderApplicator
			.Items.Clear()

			.Items.Add(New ListItem("Applicator not selected", Guid.Empty.ToString))

			Dim applicatorList As ArrayList = KaApplicator.GetAll(GetUserConnection(_currentUser.Id), "(deleted = 0) OR (id = " & Q(currentApplicatorId) & ")", "name ASC")
			For Each applicatorInfo As KaApplicator In applicatorList
				.Items.Add(New ListItem(applicatorInfo.Name.Trim, applicatorInfo.Id.ToString))
				If applicatorInfo.Id.ToString = currentApplicatorId.ToString Then
					.SelectedIndex = .Items.Count - 1
					applicatorFound = True
				End If
			Next

			If Not applicatorFound Then
				.SelectedIndex = 0
			End If
		End With
	End Sub

	Private Sub PopulateProductList(ByRef ddlProductList As DropDownList, ByVal orderInfoList As List(Of KaOrder), ByVal currentCompartmentItem As Guid)
		Dim orderItemFound As Boolean = False
		ddlProductList.Items.Clear()

		Dim orderIdList As String = ""
		For Each orderInfo As KaOrder In orderInfoList
			If orderIdList.Length > 0 Then orderIdList &= ","
			orderIdList &= Q(orderInfo.Id)
		Next
		If orderIdList.Length = 0 Then orderIdList = Q("")

		Dim oConn As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim currentItemsTable As New DataTable
		Dim currentItemsDA As New OleDbDataAdapter("SELECT orders.number, order_items.id, order_items.unit_id, order_items.request, products.name, order_items.product_id " &
									"FROM order_items " &
									"INNER JOIN orders ON orders.id = order_items.order_id " &
									"INNER JOIN products ON products.id = order_items.product_id " &
									"WHERE (order_items.deleted = 0) " &
										"AND order_items.order_id IN (" & orderIdList & ")" &
									"ORDER BY products.name ASC, orders.number ASC", oConn)
		If Tm2Database.CommandTimeout > 0 Then currentItemsDA.SelectCommand.CommandTimeout = Tm2Database.CommandTimeout
		currentItemsDA.Fill(currentItemsTable)

		If lstUsedOrders.Items.Count <= 1 OrElse _currentOrderItemIds.Count > 0 Then
			For Each rdr As DataRow In currentItemsTable.Rows
				Dim unitInfo As KaUnit
				Try
					unitInfo = New KaUnit(GetUserConnection(_currentUser.Id), rdr.Item("unit_id"))
				Catch ex As RecordNotFoundException
					unitInfo = New KaUnit
				End Try
				Dim productDisplay As String = rdr.Item("name").ToString.Trim & " (" & Format(rdr.Item("request"), unitInfo.UnitPrecision) & " " & unitInfo.Abbreviation & ")"
				If orderInfoList.Count > 1 Then productDisplay &= " - " & rdr.Item("number").ToString.Trim
				ddlProductList.Items.Add(New ListItem(productDisplay, rdr.Item("id").ToString))
				If rdr.Item("id").ToString = currentCompartmentItem.ToString Then
					ddlProductList.SelectedIndex = ddlProductList.Items.Count - 1
					orderItemFound = True
				End If
			Next
		End If

		If Not orderItemFound Then
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

	Private Sub PopulateOrdersList(ByVal currentOrders As List(Of KaStagedOrderOrder))
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		lstUsedOrders.Items.Clear()
		For Each stagedOrder As KaStagedOrderOrder In currentOrders
			If stagedOrder.Deleted Then Continue For
			Dim orderInfo As New KaOrder(connection, stagedOrder.OrderId)
			lstUsedOrders.Items.Add(New ListItem(orderInfo.Number.Trim, GetStagedOrderOrderXml(stagedOrder)))
		Next

		If lstUsedOrders.Items.Count > 0 Then lstUsedOrders.SelectedIndex = 0
		lstUsedOrders_SelectedIndexChanged(lstUsedOrders, New EventArgs)
	End Sub

	Private Sub PopulateStagedOrderInfo()
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
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
			ddlCustomerSite.Items.Add(New ListItem("", .CustomerAccountLocationId.ToString))
			ddlCustomerSite.SelectedIndex = 0
			PopulateCarriersList(.CarrierId)
			PopulateDriversList(.DriverId, .Orders)
			PopulateFacilityList(.LocationId)
			PopulateBaysList(.BayId)
			PopulateOrdersList(.Orders)
			PopulateApplicatorsList(.Orders(0).ApplicatorId)
			PopulatetDischargeStorageLocationList()

			Dim orderInfoList As List(Of KaOrder) = GetOrderInfo()
			Dim originalTareList As New List(Of StagedOrders.OriginalTareInfo)
			pnlStagedTransports.Controls.Clear()

			For Each stagedTransportInfo As KaStagedOrderTransport In .Transports
				If Not .Deleted Then
					Dim originalTareInfo As New StagedOrders.OriginalTareInfo
					With originalTareInfo
						.StagedTransportId = stagedTransportInfo.Id
						.OriginalTareInfo = New KaTimestampedQuantity(stagedTransportInfo.TareWeight, stagedTransportInfo.TareUnitId, stagedTransportInfo.TaredAt, stagedTransportInfo.TareManual)
					End With
					originalTareList.Add(originalTareInfo)
				End If
			Next

			CreateDynamicControls()

			lblInternalNotes.Text = .Notes
			pnlInternalNotes.Visible = (.Notes.Trim.Length > 0)
		End With
		lstUsedOrders_SelectedIndexChanged(lstUsedOrders, New EventArgs)
	End Sub

	Protected Overrides Function SaveViewState() As Object
		Dim viewState(10) As Object
		'Saving the grid values to the View State
		_stagedOrderInfo = ConvertPageToStagedOrder()
		ConvertPageCustomLoadQuestionData()
		Dim originalCompartmentItems As New List(Of KaStagedOrderCompartmentItem)(_originalCompartmentItems.Values)

		Dim originalTareList As New List(Of StagedOrders.OriginalTareInfo)
		ConvertToOriginalTareWeights(originalTareList)
		viewState(0) = _stagedOrderInfo
		viewState(1) = originalCompartmentItems
		viewState(2) = originalTareList
		viewState(3) = _customQuestionFieldData
		viewState(4) = _bayPreloadQuestions
		viewState(5) = _bayPostloadQuestions
		viewState(6) = _customLoadQuestionsAvailable
		viewState(7) = _currentDestinationStorageLocations
		viewState(8) = _currentSourceStorageLocations
		Dim selectedSourceStorageLocationList As List(Of KeyValuePair(Of String, List(Of String))) = New List(Of KeyValuePair(Of String, List(Of String)))
		If _selectedSourceStorageLocations IsNot Nothing Then
			For Each id As String In _selectedSourceStorageLocations.Keys
				selectedSourceStorageLocationList.Add(New KeyValuePair(Of String, List(Of String))(id, _selectedSourceStorageLocations(id)))
			Next
		End If
		viewState(9) = selectedSourceStorageLocationList
		viewState(10) = MyBase.SaveViewState()
		Return viewState
	End Function

	Protected Overrides Sub LoadViewState(savedState As Object)
		'Getting the dropdown list value from view state.
		If savedState IsNot Nothing AndAlso CType(savedState, Object).Length > 1 Then
			_currentUser = Utilities.GetUser(Me)
			Dim viewState As Object() = savedState
			_stagedOrderInfo = viewState(0)
			Dim originalCompartmentItems As List(Of KaStagedOrderCompartmentItem) = viewState(1)
			For Each compItem As KaStagedOrderCompartmentItem In originalCompartmentItems
				If Not compItem.Deleted Then _originalCompartmentItems.Add(compItem.Id, compItem.Clone())
			Next
			_originalTareValues = viewState(2)
			_customQuestionFieldData = viewState(3)
			_bayPreloadQuestions = viewState(4)
			_bayPostloadQuestions = viewState(5)
			_customLoadQuestionsAvailable = viewState(6)
			_currentDestinationStorageLocations = viewState(7)
			_currentSourceStorageLocations = viewState(8)
			_selectedSourceStorageLocations = New Dictionary(Of String, List(Of String))
			For Each kvp As KeyValuePair(Of String, List(Of String)) In viewState(9)
				_selectedSourceStorageLocations.Add(kvp.Key, kvp.Value)
			Next
			CreateDynamicControls()
			MyBase.LoadViewState(viewState(10))
		Else
			MyBase.LoadViewState(savedState)
		End If
	End Sub

#Region " Convert Page, Transport and Compartment Rows into Objects "
	Private Function ConvertPageToStagedOrder() As KaStagedOrder
		Dim currentConnection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim stagedOrderInfo As KaStagedOrder = _stagedOrderInfo.Clone()
		With stagedOrderInfo
			Guid.TryParse(CType(ddlFacility.SelectedItem, ListItem).Value.ToString, .LocationId)
			Guid.TryParse(CType(ddlBayAssigned.SelectedItem, ListItem).Value, .BayId)

			.Orders = GetCurrentStagedOrderOrders()
			Dim transportList As New List(Of KaStagedOrderTransport)
			Dim compartmentList As New List(Of KaStagedOrderCompartment)
			ConvertTransportTableToObjects(transportList, compartmentList)
			.Compartments.Clear()
			.Compartments.AddRange(compartmentList)
			.Transports.Clear()
			.Transports.AddRange(transportList)

			Guid.TryParse(CType(ddlCarrier.SelectedItem, ListItem).Value.ToString, .CarrierId)
			Guid.TryParse(CType(ddlDriver.SelectedItem, ListItem).Value.ToString, .DriverId)
			Guid.TryParse(CType(ddlCustomerSite.SelectedItem, ListItem).Value.ToString, .CustomerAccountLocationId)

			'Reassign positions
			Dim position As Integer = 0
			For Each compartmentInfo As KaStagedOrderCompartment In .Compartments
				compartmentInfo.Position = position
				position += 1
			Next
			.Notes = lblInternalNotes.Text
		End With
		Return stagedOrderInfo
	End Function

	Private Sub ConvertTransportTableToObjects(ByRef transportList As List(Of KaStagedOrderTransport), ByRef compartmentList As List(Of KaStagedOrderCompartment))
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
					stagedOrderTransportInfo = New KaStagedOrderTransport(GetUserConnection(_currentUser.Id), stagedOrderTransportId)
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
					.TransportId = Guid.Parse(CType(transportRow.FindControl(transportRow.ID & "_ddlTransport"), DropDownList).SelectedValue)
					.TareWeight = Double.Parse(txtStagedOrderTareWeight.Text)
					.TareUnitId = Guid.Parse(ddlTareWeightUofM.SelectedValue)
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
		For Each listObject As Object In transportCompartmentTable.Controls
			If Not TypeOf listObject Is HtmlGenericControl Then Continue For
			Dim compartmentRow As HtmlGenericControl = listObject
			If compartmentRow.ID IsNot Nothing AndAlso compartmentRow.ID.Contains(transportCompartmentTable.ID & "_CompRow") Then
				' This is a compartment row
				Dim rowNumber As Integer = CInt(CType(compartmentRow.FindControl(compartmentRow.ID & "_RowNumber"), HtmlInputHidden).Value)
				Dim stagedOrderCompId As Guid = Guid.NewGuid
				Guid.TryParse(CType(compartmentRow.FindControl(compartmentRow.ID & "_Id"), HtmlInputHidden).Value, stagedOrderCompId)

				Dim transportCompartmentId As Guid = Guid.Empty
				If compartmentRow.FindControl(compartmentRow.ID & "_ddlTransportCompartment") IsNot Nothing Then
					Guid.TryParse(CType(compartmentRow.FindControl(compartmentRow.ID & "_ddlTransportCompartment"), DropDownList).SelectedValue.ToString, transportCompartmentId)
				End If

				' Add discharge storage locations
				If _selectedSourceStorageLocations Is Nothing Then _selectedSourceStorageLocations = New Dictionary(Of String, List(Of String))
				Dim selectedList As List(Of String) = New List(Of String)
				If compartmentRow.FindControl(compartmentRow.ID & "_cblDischargeCompartmentStorageLocation") IsNot Nothing Then
					Dim transportCompartmentDischargeStorageLocationList As CheckBoxList = compartmentRow.FindControl(compartmentRow.ID & "_cblDischargeCompartmentStorageLocation")
					For Each li As ListItem In transportCompartmentDischargeStorageLocationList.Items
						If li.Selected Then
							selectedList.Add(li.Value)
						End If
					Next
				End If
				_selectedSourceStorageLocations(stagedOrderCompId.ToString()) = selectedList

				Dim stagedOrderCompInfo As KaStagedOrderCompartment
				Try
					stagedOrderCompInfo = New KaStagedOrderCompartment(GetUserConnection(_currentUser.Id), stagedOrderCompId)
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
					stagedOrderCompItemInfo = New KaStagedOrderCompartmentItem(GetUserConnection(_currentUser.Id), stagedOrderCompId)
				Catch ex As Exception
					stagedOrderCompItemInfo = New KaStagedOrderCompartmentItem
					stagedOrderCompItemInfo.Id = stagedOrderCompId
				End Try
				With stagedOrderCompItemInfo
					.Deleted = False
					Guid.TryParse(CType(compProdRow.FindControl(compProdRow.ID & "_orderItemId" & rowNumber.ToString), HtmlInputHidden).Value, .OrderItemId)
					Double.TryParse(CType(compProdRow.FindControl(compProdRow.ID & "_tbxProductAmount" & rowNumber.ToString), TextBox).Text, .Quantity)
					.StagedOrderCompartmentId = stagedOrderCompInfo.Id
					Guid.TryParse(CType(compProdRow.FindControl(compProdRow.ID & "_ddlUnits" & rowNumber.ToString), DropDownList).SelectedValue, .UnitId)
				End With
				stagedOrderCompInfo.CompartmentItems.Add(stagedOrderCompItemInfo)
			End If
		Next
	End Sub

	Private Sub ConvertToOriginalTareWeights(ByRef originalTareList As List(Of StagedOrders.OriginalTareInfo))
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
				Dim originalTareInfo As New StagedOrders.OriginalTareInfo
				With originalTareInfo
					.StagedTransportId = stagedOrderTransportId
					.OriginalTareInfo = New KaTimestampedQuantity(originalTareWeight.Value, Guid.Parse(originalTareWeightUofM.Value), originalTareDate.Value, originalTareManual.Value)
				End With
				originalTareList.Add(originalTareInfo)
			End If
		Next
	End Sub
#End Region

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
		PopulateStorageLocationsForFacility(Guid.Empty)
		Dim transportCount As Integer = 0
		For Each stagedTransport As KaStagedOrderTransport In _stagedOrderInfo.Transports
			If Not stagedTransport.Deleted Then
				transportCount += 1
				AddStagedTransport(connection, stagedTransport, _stagedOrderInfo.Compartments, transportCount, orderInfo)
			End If
		Next
		If transportCount = 0 Then
			Dim stagedTransport As New KaStagedOrderTransport
			With stagedTransport
				.Id = Guid.NewGuid
				.TransportId = Guid.Empty
			End With
			transportCount += 1
			AddStagedTransport(connection, stagedTransport, _stagedOrderInfo.Compartments, transportCount, orderInfo)
		End If
		PopulateStorageLocationsForFacility(_stagedOrderInfo.LocationId)
		AddCustomLoadQuestions()
	End Sub

#Region " Add Transport and Compartment Rows "
	Private Sub AddStagedTransport(connection As OleDbConnection, ByVal stagedTransportInfo As KaStagedOrderTransport, ByVal compartmentList As List(Of KaStagedOrderCompartment), ByVal itemCounter As Integer, ByVal orderInfoList As List(Of KaOrder))
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
			.InnerText = "Tare"
		End With
		tareInfoPanel.Controls.Add(tareLabel)

		Dim tareInfoDiv As New HtmlGenericControl("div")
		With tareInfoDiv
			.Attributes("class") = "input"
			.Attributes("style") = "width: 80%; display: inline-block;"
		End With
		tareInfoPanel.Controls.Add(tareInfoDiv)

		Dim newTransportTareInfoLabel As New Label
		With newTransportTareInfoLabel
			.ID = newTransportRow.ID & "_TareInfo"
			Try
				Dim uofMInfo As New KaUnit(GetUserConnection(_currentUser.Id), transportInfo.UnitId)
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
		Dim OriginalTareDate As New HtmlInputHidden
		OriginalTareDate.ID = newTransportRow.ID & "_OriginalTareDate"
		stagedTareDatePanel.Controls.Add(tbxTareDate)
		stagedTareDatePanel.Controls.Add(OriginalTareDate)

		With tbxTareDate
			.Name = newTransportRow.ID & "_tbxTransportTareDate"
			.ID = newTransportRow.ID & "_tbxTransportTareDate"
			.Attributes("class") = "hasDatePicker"
			.Attributes("Style") = "width: 60%; min-width: 15em;"
			If _currentUser IsNot Nothing Then ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), String.Format("{0}_tbxTransportTareDateScript", newTransportRow.ID), "<script type=""text/javascript"">$('#" & newTransportRow.ID & "_tbxTransportTareDate').datetimepicker({ " &
						   "timeFormat: 'h:mm:ss TT', " &
						   "showSecond: true, " &
						   "showOn: ""both"", " &
						   "buttonImage: 'Images/Calendar_scheduleHS.png'," &
						   "buttonImageOnly: true," &
						   "buttonText: ""Show calendar""});</script>", False)

			' Define the name and type of the client scripts on the page. 
			Dim csname As [String] = newTransportRow.ID & "_tbxTransportTareDateScript"
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

				cs.RegisterStartupScript(cstype, csname, cstext1.ToString())
			End If
		End With

		txtStagedOrderTareWeight.Text = stagedTransportInfo.TareWeight
		If stagedTransportInfo.TareUnitId.Equals(Guid.Empty) Then stagedTransportInfo.TareUnitId = KaUnit.GetSystemDefaultMassUnitOfMeasure(GetUserConnection(_currentUser.Id), Nothing)
		PopulateUnitOfMeasureList(ddlTareWeightUofM, stagedTransportInfo.TareUnitId, False)
		If stagedTransportInfo.TaredAt > New DateTime(1900, 1, 1) Then
			tbxTareDate.Value = stagedTransportInfo.TaredAt
		Else
			tbxTareDate.Value = ""
		End If
		Dim originalTareFound As Boolean = False
		For Each originalTare As StagedOrders.OriginalTareInfo In _originalTareValues
			If originalTare.StagedTransportId.Equals(stagedTransportInfo.Id) Then
				originalTareWeightUofM.Value = originalTare.OriginalTareInfo.UnitId.ToString
				originalTareWeight.Value = originalTare.OriginalTareInfo.Numeric
				OriginalTareDate.Value = String.Format("{0:G}", originalTare.OriginalTareInfo.Timestamp)
				originalTareManual.Value = originalTare.OriginalTareInfo.Manual
				originalTareFound = True
				Exit For
			End If
		Next
		If Not originalTareFound Then
			originalTareWeightUofM.Value = stagedTransportInfo.TareUnitId.ToString
			originalTareWeight.Value = stagedTransportInfo.TareWeight
			OriginalTareDate.Value = String.Format("{0:G}", stagedTransportInfo.TaredAt)
			originalTareManual.Value = stagedTransportInfo.TareManual
		End If
		originalTareWeightUofM.Attributes("type") = "hidden"
		originalTareWeight.Attributes("type") = "hidden"
		OriginalTareDate.Attributes("type") = "hidden"
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
			If Not compartmentInfo.Deleted AndAlso compartmentInfo.StagedOrderTransportId.ToString = stagedTransportInfo.Id.ToString Then
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

		'ToDo:  TransportChanged(transportDropdownList, New System.EventArgs)

		'			<div transportDiv>
		'				<ul transportDivPanel>
		'					<li tareInfoPanel>
		'						<label tareLabel> </label>
		'						<div tareInfoDiv>
		'							<asp:Label newTransportTareInfoLabel></asp:Label>
		'							<asp:Button newTransportAssignTareButton />
		'							<ul stagedTareInfoPanel >
		'								<li stagedTareWeightPanel>
		'									<asp:TextBox txtStagedOrderTareWeight></asp:TextBox>
		'									<asp:DropDownList ddlTareWeightUofM />
		'								</li>
		'								<li>
		'									<asp:TextBox tbxTareDate></asp:TextBox>
		'								</li>
		'							</ul>
		'						</div>
		'					</li>
		'					<li>
		'						<%--Trans1Comp1--%>
	End Sub

	Private Sub AddStagedCompartment(connection As OleDbConnection, ByRef transportDivPanel As HtmlGenericControl, currentCompartmentInfo As KaStagedOrderCompartment, ByVal itemCounter As Integer, ByVal orderInfo As List(Of KaOrder), ByVal transportId As Guid)

		Dim newCompartmentRow As New HtmlGenericControl("li")
		newCompartmentRow.ID = transportDivPanel.ID & "_CompRow" & itemCounter.ToString
		transportDivPanel.Controls.Add(newCompartmentRow)

		Dim stagedTransportId As New HtmlInputHidden
		With stagedTransportId
			.Attributes("type") = "hidden"
			.ID = newCompartmentRow.ID & "_Id"
			.Value = currentCompartmentInfo.Id.ToString
			.Style.Item("padding-top") = "0px"
		End With
		newCompartmentRow.Controls.Add(stagedTransportId)

		Dim rowNumber As New HtmlInputHidden
		With rowNumber
			.Attributes("type") = "hidden"
			.ID = newCompartmentRow.ID & "_RowNumber"
			.Value = itemCounter.ToString
			.Style.Item("padding-top") = "0px"
		End With
		newCompartmentRow.Controls.Add(rowNumber)

		Dim newCompNumberCell As New HtmlGenericControl("label")
		With newCompNumberCell
			.ID = newCompartmentRow.ID & "_CompNumber"
			.InnerText = "Compartment " & itemCounter.ToString() & ": "
			.EnableViewState = True
			.Attributes("Style") = "width:15%; font-weight: bold;"
		End With
		newCompartmentRow.Controls.Add(newCompNumberCell)

		Dim compartmentInfoPanel As New HtmlGenericControl("ul")
		With compartmentInfoPanel
			.Attributes("class") = "input"
			.Attributes("style") = "width: 80%;"
		End With

		newCompartmentRow.Controls.Add(compartmentInfoPanel)
		Dim transportCompartmentPanel As New HtmlGenericControl("li")
		compartmentInfoPanel.Controls.Add(transportCompartmentPanel)

		' Add the Transport compartment Dropdown list
		Dim transportCompartmentDropdownList As New DropDownList
		With transportCompartmentDropdownList
			.ID = newCompartmentRow.ID & "_ddlTransportCompartment"
			.AutoPostBack = True
			.Attributes("Style") = "width:auto; padding-top: 0px;"
		End With
		PopulateTransportCompartmentList(transportCompartmentDropdownList, currentCompartmentInfo.TransportCompartmentId, transportId)
		transportCompartmentPanel.Controls.Add(transportCompartmentDropdownList)

		Dim compartmentItemsRowPanel As New HtmlGenericControl("li")
		compartmentInfoPanel.Controls.Add(compartmentItemsRowPanel)
		compartmentItemsRowPanel.Controls.Add(New HtmlGenericControl("label") With {.InnerHtml = "&nbsp;"})

		Dim newCompProdTable As New HtmlGenericControl("ul")
		With newCompProdTable
			.ID = newCompartmentRow.ID & "_ProdTable"
		End With
		compartmentItemsRowPanel.Controls.Add(newCompProdTable)

		Dim itemCount As Integer = 0
		For Each compartmentItem As KaStagedOrderCompartmentItem In currentCompartmentInfo.CompartmentItems
			With compartmentItem
				If Not .Deleted Then
					itemCount += 1
					AddStagedCompartmentItem(connection, compartmentItem, newCompProdTable, orderInfo, itemCount)
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
				.UnitId = KaUnit.GetUnitIdForBaseUnit(GetUserConnection(_currentUser.Id), KaUnit.Unit.Pounds)
			End With
			itemCount += 1
			AddStagedCompartmentItem(connection, compartmentItem, newCompProdTable, orderInfo, itemCount)
		End If

		' Add discharge storage locations
		Dim transportCompartmentStorageLocationPanel As New HtmlGenericControl("li")
		compartmentInfoPanel.Controls.Add(transportCompartmentStorageLocationPanel)
		Dim transportCompartmentDischargeStorageLocationList As New CheckBoxList()
		With transportCompartmentDischargeStorageLocationList
			.ID = newCompartmentRow.ID & "_cblDischargeCompartmentStorageLocation"
			.RepeatLayout = RepeatLayout.UnorderedList
			.CssClass = "input"
		End With
		PopulateCompartmentSourceStorageLocationList(transportCompartmentDischargeStorageLocationList, currentCompartmentInfo.Id.ToString())
		transportCompartmentStorageLocationPanel.Controls.Add(New HtmlGenericControl("label") With {.InnerText = "Mark storage location as source"})
		transportCompartmentStorageLocationPanel.Controls.Add(transportCompartmentDischargeStorageLocationList)
		transportCompartmentStorageLocationPanel.Visible = transportCompartmentDischargeStorageLocationList.Items.Count > 0


		'				<ul transportDivPanel>
		'					<li newCompartmentRow>
		'						<%--Trans1Comp1--%>
		'						<asp:LinkButton newCompartmentRemoveButton></asp:LinkButton>
		'						<label newCompNumberCell>
		'						</label>
		'						<asp:DropDownList transportCompartmentDropdownList></asp:DropDownList>
		'					</li>  
		'					<li newCompProdRow>
		'						<ul newCompProdTable>
		'							<li>
		'								<%--Trans1Comp1Product1--%>
		'								<label style="width: 15%;">
		'									<asp:LinkButton ID="LinkButton2" runat="server" CssClass="button" Text="x" ToolTip="Remove product"></asp:LinkButton>
		'								</label>
		'								<span class="input" style="width: 80%; display: inline-block;">
		'									<asp:DropDownList ID="DropDownList2" runat="server" Style="width: 35%;">
		'									</asp:DropDownList>
		'									<asp:TextBox ID="tbxTrans1Comp1Prod1" runat="server" Text="0" Style="width: 35%;"></asp:TextBox>
		'									<asp:DropDownList ID="DropDownList4" runat="server" Style="width: 15%;">
		'									</asp:DropDownList>
		'								</span></li>
		'							<li newCompartmentAddProdRow>
		'								<%--Trans1Comp1AddProduct--%>
		'								<label newCompartmentAddProdButton>
		'								</label>
		'							</li>
		'						</ul>
		'					</li>  
		'					<li newCompartmentDischargeLocarionRow>
		'						<asp:CheckedListBox cblDischargeCompartmentStorageLocation></asp:CheckedListBox>
		'					</li>  
	End Sub

	Private Sub AddStagedCompartmentItem(connection As OleDbConnection, ByVal currentCompartmentItem As KaStagedOrderCompartmentItem, ByRef newCompProdTable As HtmlGenericControl, ByVal orderInfoList As List(Of KaOrder), itemCounter As Integer)
		If Not _originalCompartmentItems.ContainsKey(currentCompartmentItem.Id) Then
			_originalCompartmentItems.Add(currentCompartmentItem.Id, currentCompartmentItem.Clone())
			_originalCompartmentItems(currentCompartmentItem.Id).Quantity = 0
		End If
		Dim requestedCompartmentItem As KaStagedOrderCompartmentItem = _originalCompartmentItems(currentCompartmentItem.Id)

		Dim newCompProdRow As New HtmlGenericControl("li")
		newCompProdRow.ID = newCompProdTable.ID & "_ItemRow" & itemCounter.ToString
		newCompProdTable.Controls.Add(newCompProdRow)

		Dim stagedTransportId As New HtmlInputHidden
		With stagedTransportId
			.Attributes("type") = "hidden"
			.ID = newCompProdRow.ID & "_Id"
			.Value = currentCompartmentItem.Id.ToString
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

		'Set up the product cell
		Dim productPanel As New HtmlGenericControl("span")
		With productPanel
			.Attributes("class") = "input"
			.Attributes("style") = "width: 80%; display: inline-block;"
		End With
		newCompProdRow.Controls.Add(productPanel)

		If Not _orderItems.ContainsKey(currentCompartmentItem.OrderItemId) Then _orderItems.Add(currentCompartmentItem.OrderItemId, New KaOrderItem(connection, currentCompartmentItem.OrderItemId))
		Dim orderItem As KaOrderItem = _orderItems(currentCompartmentItem.OrderItemId)
		If Not _products.ContainsKey(orderItem.ProductId) Then _products.Add(orderItem.ProductId, New KaProduct(connection, orderItem.ProductId))

		Dim productName As New HtmlGenericControl("label")
		With productName
			Dim prodName As String = _products(orderItem.ProductId).Name
			If _stagedOrderInfo.Orders.Count > 1 Then
				If _stagedOrderInfo.UseOrderPercents Then
					prodName &= " (All Orders)"
				Else
					Dim orderNumber As String = ""
					Dim orderCount As Integer = 0
					For Each order As KaOrder In orderInfoList
						If (orderItem.OrderId.Equals(Guid.Empty) OrElse order.Id.Equals(orderItem.OrderId)) AndAlso order.Number.Trim().Length > 0 Then
							If orderNumber.Trim().Length > 0 Then orderNumber &= ", "
							orderNumber &= order.Number.Trim()
							orderCount += 1
						End If
					Next
					If orderNumber.Trim().Length > 0 Then prodName &= String.Format(" (Order{0}: {1})", IIf(orderCount > 1, "s", ""), orderNumber)
				End If
			End If
			.InnerText = prodName
			.Attributes("Style") = "width:30%; min-width:30px;"
		End With
		productPanel.Controls.Add(productName)

		Dim originalProductAmount As New HtmlGenericControl("label")
		With originalProductAmount
			.ID = newCompProdRow.ID & "_productAmount" & itemCounter.ToString
			.InnerText = String.Format("{0} {1}", requestedCompartmentItem.Quantity, GetUnit(connection, requestedCompartmentItem.UnitId, _units).Abbreviation)
			.Attributes("Style") = "width:15%; min-width:30px;"
		End With
		productPanel.Controls.Add(originalProductAmount)
		Dim originalOrderItemId As New HtmlInputHidden
		With originalOrderItemId
			.ID = newCompProdRow.ID & "_orderItemId" & itemCounter.ToString
			.Value = currentCompartmentItem.OrderItemId.ToString()
		End With
		productPanel.Controls.Add(originalOrderItemId)
		Dim originalUnitId As New HtmlInputHidden
		With originalUnitId
			.ID = newCompProdRow.ID & "_unitId" & itemCounter.ToString
			.Value = currentCompartmentItem.UnitId.ToString()
		End With
		productPanel.Controls.Add(originalUnitId)

		Dim newProductAmount As New TextBox
		With newProductAmount
			.ID = newCompProdRow.ID & "_tbxProductAmount" & itemCounter.ToString
			.Attributes("Style") = "width:auto; text-align:right; min-width:30px;"
			'.AutoPostBack = True
			.Text = currentCompartmentItem.Quantity
		End With

		Dim prodAmountSpan As New HtmlGenericControl("span")
		prodAmountSpan.Attributes("class") = "required"
		productPanel.Controls.Add(prodAmountSpan)
		prodAmountSpan.Controls.Add(newProductAmount)

		Dim newProductUnitList As New DropDownList
		With newProductUnitList
			.ID = newCompProdRow.ID & "_ddlUnits" & itemCounter.ToString
			'.AutoPostBack = True
			.Attributes("Style") = "width:auto; min-width: 20px;"
		End With
		PopulateUnitOfMeasureList(newProductUnitList, currentCompartmentItem.UnitId)
		productPanel.Controls.Add(newProductUnitList)
	End Sub
#End Region

#Region " Add Custom Load questions "
	Private Sub AddCustomLoadQuestions()
		AddCustomLoadQuestionstoPanel(_bayPreloadQuestions, ulCustomPreLoadQuestions)
		AddCustomLoadQuestionstoPanel(_bayPostloadQuestions, ulCustomPostLoadQuestions)

		pnlCustomLoadQuestions.Visible = (ulCustomPreLoadQuestions.Controls.Count + ulCustomPostLoadQuestions.Controls.Count > 0)
		pnlCustomPreLoadQuestions.Visible = (ulCustomPreLoadQuestions.Controls.Count > 0)
		pnlCustomPostLoadQuestions.Visible = (ulCustomPostLoadQuestions.Controls.Count > 0)
	End Sub

	Private Sub AddCustomLoadQuestionstoPanel(ByVal customLoadQuestions As List(Of KaCustomLoadQuestionFields), ByRef lstCustomFields As HtmlGenericControl)
		lstCustomFields.Controls.Clear()
		For Each question As KaCustomLoadQuestionFields In customLoadQuestions
			Dim customListItem As New HtmlGenericControl("li")
			Dim customFieldPrompt As New HtmlGenericControl("label")
			customFieldPrompt.InnerText = question.PromptText
			' customFieldPrompt.CssClass = "label"
			customListItem.Controls.Add(customFieldPrompt)
			Dim data As KaCustomLoadQuestionData = Nothing
			For Each possibleData As KaCustomLoadQuestionData In _customQuestionFieldData
				If possibleData.CustomLoadQuestionFieldsId.Equals(question.Id) Then
					data = possibleData
					Exit For
				End If
			Next
			If data Is Nothing Then
				data = New KaCustomLoadQuestionData
				data.CustomLoadQuestionFieldsId = question.Id
				data.Data = ""
				_customQuestionFieldData.Add(data)
			End If

			Select Case question.InputType
				Case KaCustomLoadQuestionFields.InputTypes.TextField
					Dim tbx As New TextBox
					With tbx
						.ID = "tbxCustomField" & question.Id.ToString
						.Text = data.Data
						.CssClass = "input"
						.EnableViewState = True
						.AutoPostBack = True
					End With
					customListItem.Controls.Add(tbx)
				Case KaCustomLoadQuestionFields.InputTypes.List
					Dim ddl As New DropDownList
					With ddl
						.ID = "ddlCustomField" & question.Id.ToString
						For Each parameter As String In question.Options.Split("|")
							.Items.Add(parameter)
						Next
						Try
							.SelectedValue = data.Data
						Catch ex As ArgumentOutOfRangeException

						End Try
						.CssClass = "input"
						.EnableViewState = True
						.AutoPostBack = True
					End With
					customListItem.Controls.Add(ddl)
				Case KaCustomLoadQuestionFields.InputTypes.DateAndTime,
					 KaCustomLoadQuestionFields.InputTypes.DateOnly,
					 KaCustomLoadQuestionFields.InputTypes.TimeOnly
					Dim span As New HtmlGenericControl("div")
					span.Attributes("class") = "input"
					customListItem.Controls.Add(span)

					Dim tbx As New HtmlInputText
					With tbx
						.ID = "tbxCustomField" & question.Id.ToString
						.Value = data.Data
						.Attributes("class") = "input"
						'.CssClass = "input"
						.EnableViewState = True
						'.AutoPostBack = True
					End With
					span.Controls.Add(tbx)

					Dim script As String
					If question.InputType = KaCustomLoadQuestionFields.InputTypes.DateAndTime Then
						script = "$('#" & tbx.ID & "').datetimepicker({" &
							"timeOnly: false," &
							"showTime: true," &
							"timeFormat: 'h:mm:ss TT'," &
							"showSecond: true,"
					ElseIf question.InputType = KaCustomLoadQuestionFields.InputTypes.DateOnly Then
						script = "$('#" & tbx.ID & "').datepicker({" &
							"timeOnly: false," &
							"showTime: false," &
							"showTimepicker: false,"
					Else
						script = "$('#" & tbx.ID & "').timepicker({" &
							"timeOnly: true," &
							"showTime: true," &
							"timeFormat: 'h:mm:ss TT'," &
							"showSecond: true,"
					End If
					script &= "showOn: ""both""," &
						"buttonImage: 'Images/Calendar_scheduleHS.png'," &
						"buttonImageOnly: true," &
						"buttonText: ""Show calendar""" &
						"});"

					If _currentUser IsNot Nothing Then ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), String.Format("{0}Script", tbx.ID), "<script type=""text/javascript"">" & script & "</script>", False)

					Dim csname As [String] = String.Format("{0}StartupScript", tbx.ID)
					Dim cstype As Type = Me.[GetType]()

					' Get a ClientScriptManager reference from the Page class. 
					Dim cs As ClientScriptManager = Page.ClientScript

					' Check to see if the startup script is already registered. 
					If Not cs.IsStartupScriptRegistered(cstype, csname) Then
						Dim cstext1 As New StringBuilder()
						cstext1.Append("<script type=""text/javascript"">" & script & "</script>")

						cs.RegisterStartupScript(Page.Header.[GetType](), csname, cstext1.ToString())
					End If
				Case KaCustomLoadQuestionFields.InputTypes.YesNo
					Dim cbx As New CheckBox
					With cbx
						.ID = "cbxCustomField" & question.Id.ToString
						If Not Boolean.TryParse(data.Data, .Checked) Then .Checked = (data.Data.ToUpper().StartsWith("Y"))
						'.Text = customFieldPrompt.InnerText
						'customFieldPrompt.InnerText = ""
						.CssClass = "input"
						.EnableViewState = True
						.AutoPostBack = True
					End With
					customListItem.Controls.Add(cbx)
				Case KaCustomLoadQuestionFields.InputTypes.TableLookup
					Dim ddl As New DropDownList
					With ddl
						.ID = "ddlCustomField" & question.Id.ToString
						.Items.Clear()

						Try
							Dim tableLookup As KaCustomQuestionTableLookup = Tm2Database.FromXml(question.Options, GetType(KaCustomQuestionTableLookup))
							Dim r As OleDbDataReader = Tm2Database.ExecuteReader(Tm2Database.Connection, String.Format("SELECT DISTINCT {0} FROM {1} WHERE deleted = 0 ORDER BY {0}", tableLookup.FieldName, tableLookup.TableName))
							Do While (r.Read())
								Dim parameter As String = IsNull(r.Item(0).ToString().Trim(), "")
								Dim listOption As New ListItem(IIf(parameter.Length > 0, parameter, "(blank)"), parameter)
								.Items.Add(listOption)
							Loop
						Catch ex As Exception
							.Items.Clear()

							Try
								Dim tableLookup As KaCustomQuestionTableLookup = Tm2Database.FromXml(question.Options, GetType(KaCustomQuestionTableLookup))
								Dim r As OleDbDataReader = Tm2Database.ExecuteReader(Tm2Database.Connection, String.Format("SELECT DISTINCT {0} FROM {1} ORDER BY {0}", tableLookup.FieldName, tableLookup.TableName))
								Do While (r.Read())
									Dim parameter As String = IsNull(r.Item(0).ToString().Trim(), "")
									Dim listOption As New ListItem(IIf(parameter.Length > 0, parameter, "(blank)"), parameter)
									.Items.Add(listOption)
								Loop
							Catch ex2 As Exception
							End Try
						End Try
						Try
							.SelectedValue = data.Data
						Catch ex As ArgumentOutOfRangeException

						End Try
						.CssClass = "input"
						.EnableViewState = True
						'.AutoPostBack = True
					End With
					customListItem.Controls.Add(ddl)
				Case KaCustomLoadQuestionFields.InputTypes.Url
					Continue For
				Case Else
					Continue For
			End Select
			Dim value As New HtmlInputHidden()
			With value
				.ID = question.Id.ToString
				.Value = Tm2Database.ToXml(data, GetType(KaCustomLoadQuestionData))
				.EnableViewState = True
			End With
			customListItem.Controls.Add(value)

			lstCustomFields.Controls.Add(customListItem)
		Next
	End Sub

	Private Sub ConvertPageCustomLoadQuestionData()
		ConvertCustomFieldPanelToLists(_bayPreloadQuestions, ulCustomPreLoadQuestions)
		ConvertCustomFieldPanelToLists(_bayPostloadQuestions, ulCustomPostLoadQuestions)
	End Sub

	Public Sub ConvertCustomFieldPanelToLists(ByRef customLoadQuestionFields As List(Of KaCustomLoadQuestionFields), ByVal lstCustomFields As HtmlGenericControl)
		For Each customFieldItem As Object In lstCustomFields.Controls
			If TypeOf customFieldItem Is HtmlGenericControl Then
				For Each childObject As Object In customFieldItem.Controls
					If TypeOf childObject Is HtmlInputHidden Then
						Dim tempData As KaCustomLoadQuestionData = Tm2Database.FromXml(CType(childObject, HtmlInputHidden).Value, GetType(KaCustomLoadQuestionData))
						Dim data As KaCustomLoadQuestionData = Nothing
						For Each possibleData As KaCustomLoadQuestionData In _customQuestionFieldData
							If possibleData.CustomLoadQuestionFieldsId.Equals(tempData.CustomLoadQuestionFieldsId) Then
								data = possibleData
								Exit For
							End If
						Next
						Dim customField As KaCustomLoadQuestionFields = Nothing
						If data IsNot Nothing Then
							For Each possibleCustomField As KaCustomLoadQuestionFields In customLoadQuestionFields
								If possibleCustomField.Id.Equals(data.CustomLoadQuestionFieldsId) Then
									customField = possibleCustomField
									Exit For
								End If
							Next
						End If
						If data Is Nothing Then
							data = tempData
							_customQuestionFieldData.Add(data)
						End If
						If customField Is Nothing Then Continue For
						Select Case customField.InputType
							Case KaCustomLoadQuestionFields.InputTypes.TextField
								Dim tbx As TextBox = customFieldItem.FindControl("tbxCustomField" & customField.Id.ToString)
								If tbx IsNot Nothing Then data.Data = tbx.Text
							Case KaCustomLoadQuestionFields.InputTypes.List, KaCustomLoadQuestionFields.InputTypes.TableLookup
								Dim ddl As DropDownList = customFieldItem.FindControl("ddlCustomField" & customField.Id.ToString)
								If ddl IsNot Nothing Then data.Data = ddl.SelectedValue
							Case KaCustomLoadQuestionFields.InputTypes.DateAndTime, KaCustomLoadQuestionFields.InputTypes.DateOnly, KaCustomLoadQuestionFields.InputTypes.TimeOnly
								Dim tbx As HtmlInputText = customFieldItem.FindControl("tbxCustomField" & customField.Id.ToString)
								If tbx IsNot Nothing Then data.Data = tbx.Value
							Case KaCustomLoadQuestionFields.InputTypes.YesNo
								Dim cbx As CheckBox = customFieldItem.FindControl("cbxCustomField" & customField.Id.ToString)
								If cbx IsNot Nothing Then data.Data = IIf(cbx.Checked, "Yes", "No")
						End Select
						Exit For ' Continue to the next list item
					End If
				Next
			End If
		Next
	End Sub

	Private Sub GetCustomQuestionInspectionQuestionsList()
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)

		_bayPreloadQuestions = New List(Of KaCustomLoadQuestionFields)
		_bayPostloadQuestions = New List(Of KaCustomLoadQuestionFields)
		Dim bayId As Guid = _stagedOrderInfo.BayId
		Dim ownerIds As New List(Of Guid)
		For Each order As KaStagedOrderOrder In _stagedOrderInfo.Orders
			Try
				Dim ownerId As Guid = New KaOrder(connection, order.OrderId).OwnerId
				If Not ownerIds.Contains(ownerId) Then ownerIds.Add(ownerId)
			Catch ex As RecordNotFoundException

			End Try
		Next
		Dim ownerIdString As String = Q(Guid.Empty)
		For Each ownerId As Guid In ownerIds
			ownerIdString &= Q(ownerId)
		Next
		Dim conditions As String = String.Format("deleted=0 AND disabled=0 AND (bay_id={0} OR bay_id={1}) AND owner_id IN ({2})", Q(Guid.Empty), Q(bayId), ownerIdString)
		For Each question As KaCustomLoadQuestionFields In KaCustomLoadQuestionFields.GetAll(GetUserConnection(_currentUser.Id), conditions, "[index] ASC")
			If _customLoadQuestionsAvailable.Contains(question.Id) Then
				If question.PostLoad Then
					_bayPostloadQuestions.Add(question)
				Else
					_bayPreloadQuestions.Add(question)
				End If
			End If
		Next
		AddCustomLoadQuestions()
	End Sub

#End Region

	Private Sub ddlFacility_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles ddlFacility.SelectedIndexChanged
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Guid.TryParse(ddlFacility.SelectedValue, _stagedOrderInfo.LocationId)
		PopulateStorageLocationsForFacility(_stagedOrderInfo.LocationId)

		For Each compartment As KaStagedOrderCompartment In _stagedOrderInfo.Compartments
			'Only add each product once per compartment.
			Dim compartmentProducts As Dictionary(Of Guid, KaQuantity) = New Dictionary(Of Guid, KaQuantity)

			If compartment.Deleted Then Continue For
			Dim items As New List(Of KaStagedOrderCompartmentItem)(compartment.CompartmentItems)
			compartment.CompartmentItems.Clear()
			Dim densities As New Dictionary(Of Guid, KaRatio)
			For Each item As KaStagedOrderCompartmentItem In items
				If Not item.Deleted Then
					If item.OrderItemId.Equals(Guid.Empty) Then
						'If we get here, we KNOW we are using order percents on the staged order because a Guid.Empty represents an 'All Product (for all orders)' compartment entry.
						Dim orderTotalAmount As Double = 0

						For Each stagedOrderOrder As KaStagedOrderOrder In _stagedOrderInfo.Orders
							If stagedOrderOrder.Deleted Then Continue For
							Dim orderInfo As New KaOrder(connection, stagedOrderOrder.OrderId)
							For Each orderItem As KaOrderItem In orderInfo.OrderItems
								If orderItem.Deleted Then Continue For
								If Not _orderItems.ContainsKey(orderItem.Id) Then _orderItems.Add(orderItem.Id, orderItem)
								If orderTotalAmount > Double.MinValue Then
									Try
										If Not _products.ContainsKey(orderItem.ProductId) Then _products.Add(orderItem.ProductId, New KaProduct(connection, orderItem.ProductId))
										If Not densities.ContainsKey(orderItem.ProductId) Then densities.Add(orderItem.ProductId, _products(orderItem.ProductId).GetDensity(connection, Guid.Parse(ddlFacility.SelectedValue)))
										orderTotalAmount += KaUnit.FastConvert(connection, New KaQuantity(orderItem.Request, orderItem.UnitId), densities(orderItem.ProductId), item.UnitId, _units).Numeric
									Catch ex As Exception
										orderTotalAmount = Double.MinValue
									End Try
								End If
							Next
						Next

						If orderTotalAmount = Double.MinValue Then orderTotalAmount = 0

						For Each stagedOrderOrder As KaStagedOrderOrder In _stagedOrderInfo.Orders

							If stagedOrderOrder.Deleted Then Continue For
							Dim orderInfo As New KaOrder(connection, stagedOrderOrder.OrderId)
							For Each orderItem As KaOrderItem In orderInfo.OrderItems
								If orderItem.Deleted Then Continue For

								Dim qty As KaQuantity = New KaQuantity(0, item.UnitId)
								If compartmentProducts.ContainsKey(orderItem.ProductId) Then
									compartmentProducts.TryGetValue(orderItem.ProductId, qty)
								End If

								qty.Numeric += KaUnit.FastConvert(connection, New KaQuantity(orderItem.Request, orderItem.UnitId), densities(orderItem.ProductId), item.UnitId, _units).Numeric * item.Quantity / orderTotalAmount

								If compartmentProducts.ContainsKey(orderItem.ProductId) Then
									compartmentProducts.Item(orderItem.ProductId) = qty
								Else
									compartmentProducts.Add(orderItem.ProductId, qty)
									If Not _productIdOrderItemIdMap.ContainsKey(orderItem.ProductId) Then
										_productIdOrderItemIdMap.Add(orderItem.ProductId, orderItem.Id)
									End If
								End If
							Next
						Next
					Else
						compartment.CompartmentItems.Add(item)
					End If
				End If
			Next

			'Create a compartment item for each product (these products are the combined product quantities for all orders).
			For Each productId As Guid In compartmentProducts.Keys
				Dim qty As KaQuantity = Nothing
				compartmentProducts.TryGetValue(productId, qty)

				Dim newStagedOrderCompartmentItem As New KaStagedOrderCompartmentItem
				With newStagedOrderCompartmentItem
					.Id = Guid.NewGuid
					_productIdOrderItemIdMap.TryGetValue(productId, .OrderItemId)
					.Position = compartment.CompartmentItems.Count
					.Quantity = qty.Numeric
					.UnitId = qty.UnitId
					.StagedOrderCompartmentId = compartment.Id
				End With
				compartment.CompartmentItems.Add(newStagedOrderCompartmentItem)
			Next
		Next

		Dim currentBayId As Guid = Guid.Empty
		Guid.TryParse(ddlBayAssigned.SelectedValue, currentBayId)
		PopulateBaysList(currentBayId)
		PopulatetDischargeStorageLocationList()
	End Sub

	Private Sub PopulateStorageLocationsForFacility(locationId As Guid)
		_currentDestinationStorageLocations = New Dictionary(Of Guid, String)
		_currentSourceStorageLocations = New Dictionary(Of Guid, String)
		If Tm2Database.SystemItemTraceabilityEnabled Then
			Dim storageLocationRdr As OleDbDataReader = Nothing
			Try
				storageLocationRdr = Tm2Database.ExecuteReader(Tm2Database.Connection,
					$"SELECT {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_ID}, {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_LOCATION_ID}, " &
						$"{KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_NAME} " &
					$"FROM {KaStorageLocation.TABLE_NAME} " &
					$"WHERE {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_DELETED} = 0 AND {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_TANK_ID} = {Q(Guid.Empty)} AND {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_CONTAINER_ID} = {Q(Guid.Empty)} " &
					"UNION " &
					$"SELECT {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_ID}, {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_LOCATION_ID}, " &
						$"{KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_NAME} + CASE WHEN {KaTank.TABLE_NAME}.name = {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_NAME} THEN '' ELSE ' (Tank: ' + {KaTank.TABLE_NAME}.name + ')' END AS {KaStorageLocation.FN_NAME} " &
					$"FROM {KaStorageLocation.TABLE_NAME} " &
					$"INNER JOIN {KaTank.TABLE_NAME} ON {KaTank.TABLE_NAME}.id = {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_TANK_ID} AND {KaTank.TABLE_NAME}.deleted = 0 " &
					$"WHERE {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_DELETED} = 0 " &
					"UNION " &
					$"SELECT {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_ID}, {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_LOCATION_ID}, " &
						$"{KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_NAME} + CASE WHEN {KaContainer.TABLE_NAME}.number IS NULL OR {KaContainer.TABLE_NAME}.number = {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_NAME} THEN '' ELSE ' (Container: ' + {KaContainer.TABLE_NAME}.number + ')' END AS {KaStorageLocation.FN_NAME} " &
					$"FROM {KaStorageLocation.TABLE_NAME} " &
					$"INNER JOIN {KaContainer.TABLE_NAME} ON {KaContainer.TABLE_NAME}.id= {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_CONTAINER_ID} AND {KaContainer.TABLE_NAME}.deleted = 0 " &
					$"WHERE {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_DELETED} = 0 " &
					$"ORDER BY 3")
				While storageLocationRdr.Read()
					If Not locationId.Equals(Guid.Empty) AndAlso storageLocationRdr.Item(KaStorageLocation.FN_LOCATION_ID).Equals(locationId) Then _currentDestinationStorageLocations.Add(storageLocationRdr.Item(KaStorageLocation.FN_ID), storageLocationRdr.Item(KaStorageLocation.FN_NAME))
					_currentSourceStorageLocations.Add(storageLocationRdr.Item(KaStorageLocation.FN_ID), storageLocationRdr.Item(KaStorageLocation.FN_NAME))
				End While
			Finally
				If storageLocationRdr IsNot Nothing Then storageLocationRdr.Close()
			End Try
		End If
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
	''' <param name="lbsUnitOfMeasure"></param>
	''' <param name="galsUnitOfMeasure"></param>
	''' <param name="currentSelectedUnitOfMeasure">Unit of measure of the Staged Order Item</param>
	''' <param name="compartmentWeightValid">Reference variable that returns the unit conversion successful</param>
	''' <returns>The numeric amount of the individual Staged order item.</returns>
	''' <remarks></remarks>
	Private Function GetCompartmentQuantity(ByVal userConn As OleDbConnection, ByVal currentSelectedItem As Guid, ByVal productAmount As Double, ByVal orderInfoList As List(Of KaOrder), ByVal locationId As Guid, ByVal toUnitOfMeasure As KaUnit, ByVal lbsUnitOfMeasure As KaUnit, ByVal galsUnitOfMeasure As KaUnit, ByVal currentSelectedUnitOfMeasure As KaUnit, ByRef compartmentWeightValid As Boolean) As Double
		Dim compartmentWeight As Double = 0.0
		Dim density As Double = 0.0
		Dim densityWeight As Guid = lbsUnitOfMeasure.Id
		Dim densityVolume As Guid = galsUnitOfMeasure.Id
		If currentSelectedItem.Equals(Guid.Empty) Then
			Dim orderTotalRequestedLbs As Double = 0.0
			For Each orderInfo As KaOrder In orderInfoList
				Try
					orderTotalRequestedLbs += orderInfo.GetRequested(locationId, lbsUnitOfMeasure.Id).Numeric
				Catch ex As UnitConversionException
					compartmentWeightValid = False
					Return 0.0
				End Try
			Next
			If Not compartmentWeightValid OrElse orderTotalRequestedLbs <= 0.0 Then Return 0.0

			For Each orderInfo As KaOrder In orderInfoList
				For Each orderItemInfo As KaOrderItem In orderInfo.OrderItems
					If Not orderItemInfo.Deleted Then
						Try
							Dim productInfo As New KaProduct(GetUserConnection(_currentUser.Id), orderItemInfo.ProductId)
							Dim densityInfo As KaRatio = productInfo.GetDensity(userConn, locationId)
							density = densityInfo.Numeric
							densityWeight = densityInfo.NumeratorUnitId
							densityVolume = densityInfo.DenominatorUnitId
							Dim weightUnitInfo As New KaUnit(userConn, densityWeight)
							Dim volumeUnitInfo As New KaUnit(userConn, densityVolume)
							Dim fromUnitInfo As New KaUnit(userConn, orderItemInfo.UnitId)
							Dim orderItemRequestedLbs As Double = KaUnit.Convert(orderItemInfo.Request, fromUnitInfo, lbsUnitOfMeasure, density, weightUnitInfo, volumeUnitInfo)
							compartmentWeight += KaUnit.Convert(productAmount * (orderItemRequestedLbs / orderTotalRequestedLbs), currentSelectedUnitOfMeasure, toUnitOfMeasure, density, weightUnitInfo, volumeUnitInfo)
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
				Dim orderInfo As New KaOrder(GetUserConnection(_currentUser.Id), currentSelectedItem)
				Dim orderTotalRequestedLbs As Double = orderInfo.GetRequested(locationId, lbsUnitOfMeasure.Id).Numeric
				For Each orderItemInfo As KaOrderItem In orderInfo.OrderItems
					If Not orderItemInfo.Deleted Then
						Try
							Dim productInfo As New KaProduct(GetUserConnection(_currentUser.Id), orderItemInfo.ProductId)
							Dim densityInfo As KaRatio = productInfo.GetDensity(userConn, locationId)
							density = densityInfo.Numeric
							densityWeight = densityInfo.NumeratorUnitId
							densityVolume = densityInfo.DenominatorUnitId
							Dim weightUnitInfo As New KaUnit(userConn, densityWeight)
							Dim volumeUnitInfo As New KaUnit(userConn, densityVolume)
							Dim fromUnitInfo As New KaUnit(userConn, orderItemInfo.UnitId)
							Dim orderItemRequestedLbs As Double = KaUnit.Convert(orderItemInfo.Request, fromUnitInfo, lbsUnitOfMeasure, density, weightUnitInfo, volumeUnitInfo)
							compartmentWeight += KaUnit.Convert(productAmount * (orderItemRequestedLbs / orderTotalRequestedLbs), currentSelectedUnitOfMeasure, toUnitOfMeasure, density, weightUnitInfo, volumeUnitInfo)
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
					Dim orderItemInfo As New KaOrderItem(GetUserConnection(_currentUser.Id), currentSelectedItem)
					Dim productInfo As New KaProduct(GetUserConnection(_currentUser.Id), orderItemInfo.ProductId)
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
		Dim currentStagedOrderOrdersList As List(Of KaStagedOrderOrder) = GetCurrentStagedOrderOrders()
		If lstUsedOrders.SelectedIndex >= 0 Then
			Dim stagedOrderOrder As KaStagedOrderOrder = GetStagedOrderOrder(lstUsedOrders.SelectedValue)
			For Each currentOrder As KaStagedOrderOrder In currentStagedOrderOrdersList
				If currentOrder.OrderId = stagedOrderOrder.OrderId Then
					If Not Guid.TryParse(ddlCustomerSite.SelectedValue, currentOrder.CustomerAccountLocationId) Then
						ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidCustomerSite", Utilities.JsAlert("Unable to assign customer site."))
						tbxOrderAcres.Focus()
					End If
					SetStagedOrderOrderDataOnOrderList(currentOrder)
					Exit For
				End If
			Next
		End If
	End Sub

	Private Sub ShipToInformationChanged(sender As Object, e As System.EventArgs) Handles ddlCustomerSite.SelectedIndexChanged
		If litShipTo.Text.Trim.Length > 0 Then
			ddlCustomerSite.Visible = False
			litShipTo.Visible = True
		Else
			ddlCustomerSite.Visible = ddlCustomerSite.Items.Count > 1
			litShipTo.Visible = False ' (ddlCustomerSite.SelectedIndex = 0)
		End If
		pnlShipTo.Visible = ddlCustomerSite.Visible OrElse litShipTo.Visible
	End Sub

	Private Sub lstUsedOrders_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles lstUsedOrders.SelectedIndexChanged

		Dim stagedOrderOrder As New KaStagedOrderOrder
		If lstUsedOrders.SelectedIndex >= 0 Then
			stagedOrderOrder = GetStagedOrderOrder(lstUsedOrders.SelectedValue)
			PopulateApplicatorsList(stagedOrderOrder.ApplicatorId)
			tbxOrderAcres.Text = stagedOrderOrder.Acres.ToString
			pnlAcres.Visible = True
			pnlApplicator.Visible = True
		Else
			pnlAcres.Visible = False
			pnlApplicator.Visible = False
		End If

		Dim orderInfoList As List(Of KaOrder) = GetOrderInfo()

		Dim userConn As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim locationId As Guid = Guid.Empty
		Try
			locationId = New KaLocation(userConn, Guid.Parse(ddlFacility.SelectedValue)).Id
		Catch ex As RecordNotFoundException
			locationId = _stagedOrderInfo.LocationId
		End Try

		Dim selectedAcctLocId As Guid = stagedOrderOrder.CustomerAccountLocationId

		Dim currentCustomerAccountLocationId As Guid = selectedAcctLocId

		ddlCustomerSite.Items.Clear()
		ddlCustomerSite.Items.Add(New ListItem("Select a location", Guid.Empty.ToString))
		Dim customerString As String = ""
		Dim accountIdList As New List(Of Guid)
		Dim accountIdsString As String = Q(Guid.Empty) ' Capture the empty Account ID for the account locations that are for all accounts
		' For Each orderInfo As KaOrder In orderInfoList
		Dim orderInfo As KaOrder
		Try
			orderInfo = New KaOrder(GetUserConnection(_currentUser.Id), stagedOrderOrder.OrderId)
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
			Dim customerList As ArrayList = KaCustomerAccount.GetAll(GetUserConnection(_currentUser.Id), "id in (" & accountIdsString & ")", "UPPER(name)")
			For Each customerInfo As KaCustomerAccount In customerList
				If customerString.Length > 0 Then customerString &= "<br />"
				customerString &= customerInfo.Name.Trim & IIf(customerInfo.AccountNumber.Trim.Length > 0, "(" & customerInfo.AccountNumber & ")", "")
			Next
			customerLocList = KaCustomerAccountLocation.GetAll(GetUserConnection(_currentUser.Id), "customer_account_id in (" & accountIdsString & ") AND ((deleted=0) OR (id = " & Q(currentCustomerAccountLocationId) & "))", "UPPER(name)")
		End If

		For Each customerLocInfo As KaCustomerAccountLocation In customerLocList
			ddlCustomerSite.Items.Add(New ListItem(customerLocInfo.Name, customerLocInfo.Id.ToString))
			If customerLocInfo.Id.ToString = currentCustomerAccountLocationId.ToString Then
				ddlCustomerSite.SelectedIndex = ddlCustomerSite.Items.Count - 1
				accountLocationFound = True
			End If
		Next

		If Not accountLocationFound Then ddlCustomerSite.SelectedIndex = 0
		litCustomerName.Text = "<span class=""input"">" & customerString & "</span>"
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
		Dim orderList As New List(Of KaOrder)
		For Each stagedOrderOrder As KaStagedOrderOrder In orderIdList
			Try
				Dim orderInfo As New KaOrder(GetUserConnection(_currentUser.Id), stagedOrderOrder.OrderId)
				orderInfo.GetChildren(GetUserConnection(_currentUser.Id), Nothing, True)
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
		Return currentStagedOrderOrders
	End Function

	Private Function GetStagedOrderOrder(ByVal stagedOrderOrderXml As String) As KaStagedOrderOrder
		Try
			Return Tm2Database.FromXml(Server.HtmlDecode(stagedOrderOrderXml), GetType(KaStagedOrderOrder))
		Catch ex As Exception
			Return New KaStagedOrderOrder
		End Try
	End Function

	Private Function GetStagedOrderOrderXml(ByVal stagedOrderOrder As KaStagedOrderOrder) As String
		Return Server.HtmlEncode(Tm2Database.ToXml(stagedOrderOrder, GetType(KaStagedOrderOrder)))
	End Function

	Private Function ValidateStagedOrder() As Boolean
		Dim currentConnection As OleDbConnection = GetUserConnection(_currentUser.Id)

		With _stagedOrderInfo
			' Validate the facility is selected
			Try
				Dim locationInfo As New KaLocation(currentConnection, .LocationId)
			Catch ex As RecordNotFoundException
				ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidFacility", Utilities.JsAlert("A Facility must be Selected"))
				ddlFacility.Focus()
				Return False
			End Try

			' Validate that all customers are active
			For Each order As KaStagedOrderOrder In _stagedOrderInfo.Orders
				Dim orderInfo As New KaOrder(currentConnection, order.OrderId)
				For Each orderAccount As KaOrderCustomerAccount In orderInfo.OrderAccounts
					If Not orderAccount.Deleted AndAlso orderAccount.Percentage > 0 Then
						Dim customer As New KaCustomerAccount(currentConnection, orderAccount.CustomerAccountId)
						If customer.Deleted Then
							ClientScript.RegisterClientScriptBlock(Me.GetType(), "CustomerDeleted", Utilities.JsAlert("The customer (" & customer.Name & ") assigned to order " & orderInfo.Number & " has been deleted."))
							Return False
						ElseIf customer.Disabled Then
							ClientScript.RegisterClientScriptBlock(Me.GetType(), "CustomerDisabled", Utilities.JsAlert("The customer (" & customer.Name & ") assigned to order " & orderInfo.Number & " has been disabled."))
							Return False
						End If
					End If
				Next
			Next

			' Validate product quantities
			For Each stagedComp As KaStagedOrderCompartment In .Compartments
				For Each stagedItem As KaStagedOrderCompartmentItem In stagedComp.CompartmentItems
					If Not stagedItem.Deleted AndAlso Not stagedItem.Quantity >= 0 Then
						ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidCompAmt", Utilities.JsAlert("A requested amount must be assigned to each compartment quantity."))
						Return False
					End If
				Next
			Next
		End With

		'Validate custom load questions
		Dim testValue As Double = 0
		GetCustomQuestionInspectionQuestionsList()
		Dim questions As New List(Of KaCustomLoadQuestionFields)
		questions.AddRange(_bayPreloadQuestions)
		questions.AddRange(_bayPostloadQuestions)
		For Each loadQuestion As KaCustomLoadQuestionFields In questions
			If loadQuestion.InputType = KaCustomLoadQuestionFields.InputTypes.TextField Then
				For Each loadAnswer As KaCustomLoadQuestionData In _customQuestionFieldData
					If loadAnswer.CustomLoadQuestionFieldsId.Equals(loadQuestion.Id) Then
						If loadQuestion.EnterTextOnlyAllowNumericValues AndAlso Not Double.TryParse(loadAnswer.Data, testValue) Then
							ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidQuestionValue", Utilities.JsAlert(String.Format("An amount must be entered for the Pre-load question: {1}", loadQuestion.EnterTextMinCharactersRequired, loadQuestion.PromptText)))
							Return False
						ElseIf loadQuestion.EnterTextMinCharactersRequired > 0 Then
							If loadAnswer.Data.Length < loadQuestion.EnterTextMinCharactersRequired Then
								ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidQuestionLength", Utilities.JsAlert(String.Format("An answer of minimum length {0} must be entered for the Pre-load question: {1}", loadQuestion.EnterTextMinCharactersRequired, loadQuestion.PromptText)))
								Return False
							ElseIf loadAnswer.Data.Length > loadQuestion.EnterTextMaxCharactersAllowed Then
								ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidQuestionLength", Utilities.JsAlert(String.Format("An answer of maximum length {0} must be entered for the Pre-load question: {1}", loadQuestion.EnterTextMaxCharactersAllowed, loadQuestion.PromptText)))
								Return False
							End If
							Exit For
						End If
					End If
				Next
			End If
		Next
		Return True
	End Function

	Private Function CreatePointOfSaleTicket() As Boolean
		Dim applicationName As String = Database.ApplicationIdentifier
		Dim currentUserName As String = _currentUser.Name
		Dim ticketCreated As Boolean = False
		Dim connection As OleDbConnection = New OleDbConnection(Tm2Database.GetDbConnection())
		Dim transaction As OleDbTransaction = Nothing
		Try
			connection.Open()
			UpdateStagedTransportsWithTransportTares()
			If _selectedSourceStorageLocations Is Nothing Then _selectedSourceStorageLocations = New Dictionary(Of String, List(Of String))
			Dim inProgressRecord As KaInProgress = CreateInProgress(connection)
			_customQuestionFieldData.Clear()
			If _stagedOrderInfo IsNot Nothing Then
				For Each data As KaStagedOrderCustomLoadQuestionData In _stagedOrderInfo.CustomLoadQuestionDatas
					Try
						_customQuestionFieldData.Add(New KaCustomLoadQuestionData(connection, data.CustomLoadQuestionsDataId))
					Catch ex As RecordNotFoundException

					End Try
				Next
			End If
			ConvertPageCustomLoadQuestionData()
			transaction = connection.BeginTransaction
			_stagedOrderInfo.SqlUpdateInsertIfNotFound(connection, transaction, applicationName, currentUserName)
			For Each customQuestionAnswer As KaCustomLoadQuestionData In _customQuestionFieldData
				customQuestionAnswer.SqlUpdateInsertIfNotFound(connection, transaction, applicationName, currentUserName)
				Dim newInprogressAnswer As New KaInProgressCustomLoadQuestionData
				newInprogressAnswer.CustomLoadQuestionDataId = customQuestionAnswer.Id
				newInprogressAnswer.InProgressId = inProgressRecord.Id
				inProgressRecord.CustomLoadQuestionDatas.Add(newInprogressAnswer)
			Next
			inProgressRecord.SqlUpdateInsertIfNotFound(connection, transaction, applicationName, currentUserName)

			Dim createTicketReturnObject As Object = inProgressRecord.CreateTicket(connection, transaction)
			Dim ticket As KaTicket = Nothing
			Dim ticketList As List(Of KaTicket) = Nothing
			If TypeOf createTicketReturnObject Is KaTicket Then
				ticket = createTicketReturnObject
			ElseIf TypeOf createTicketReturnObject Is List(Of KaTicket) Then
				ticketList = createTicketReturnObject
			Else
				Throw New InvalidCastException($"KaInProgress.CreateRecord returned an invalid object of type '{createTicketReturnObject.GetType().Name}'")
			End If

			Dim dischargeStorageLocationIds As List(Of Guid) = New List(Of Guid)
			For Each li As ListItem In cblDischargeStorageLocations.Items
				If li.Selected Then
					Dim dischargeStorageLocationId As Guid = Guid.Empty
					If Guid.TryParse(li.Value, dischargeStorageLocationId) AndAlso Not dischargeStorageLocationId.Equals(Guid.Empty) AndAlso Not dischargeStorageLocationIds.Contains(dischargeStorageLocationId) Then dischargeStorageLocationIds.Add(dischargeStorageLocationId)
				End If
			Next
			'Assign storage location movements
			Dim storageLocationMovementsByCompartment As Dictionary(Of String, Dictionary(Of Guid, Dictionary(Of Integer, List(Of StartEndDate)))) = New Dictionary(Of String, Dictionary(Of Guid, Dictionary(Of Integer, List(Of StartEndDate))))
			For Each ipc As KaInProgressCompartment In inProgressRecord.Compartments
				If _selectedSourceStorageLocations.ContainsKey(ipc.Id.ToString()) Then
					Dim selectedCompartmentSourceStorageLocations As List(Of String) = _selectedSourceStorageLocations(ipc.Id.ToString())
					For Each weighment As KaInProgressWeighment In inProgressRecord.Weighments
						If weighment.Compartment = ipc.Position Then
							If ticket IsNot Nothing Then
								AssignStorageLocationTransfers(connection, transaction, ticket, weighment, dischargeStorageLocationIds, selectedCompartmentSourceStorageLocations, storageLocationMovementsByCompartment)
							ElseIf ticketList IsNot Nothing Then
								For Each t As KaTicket In ticketList
									AssignStorageLocationTransfers(connection, transaction, t, weighment, dischargeStorageLocationIds, selectedCompartmentSourceStorageLocations, storageLocationMovementsByCompartment)
								Next
							End If
						End If
					Next
				End If
			Next

			If ticket IsNot Nothing Then
				InsertStorageLocationMovements(connection, transaction, ticket, storageLocationMovementsByCompartment, applicationName, currentUserName)
			ElseIf ticketList IsNot Nothing Then
				For Each t As KaTicket In ticketList
					InsertStorageLocationMovements(connection, transaction, t, storageLocationMovementsByCompartment, applicationName, currentUserName)
				Next
			End If

			inProgressRecord.SqlDelete(connection, transaction, applicationName, currentUserName)
			_stagedOrderInfo.Deleted = True
			_stagedOrderInfo.SqlUpdateInsertIfNotFound(connection, transaction, applicationName, currentUserName)

			Dim emailTicket As Boolean = True
			Boolean.TryParse(KaSetting.GetSetting(connection, OrderSettings.SN_EMAIL_POINT_OF_SALE_TICKETS, OrderSettings.SD_EMAIL_POINT_OF_SALE_TICKETS, transaction), emailTicket)
			If ticket IsNot Nothing Then
				ticket.DoNotEmail = Not emailTicket
				ticket.SqlUpdate(connection, transaction, applicationName, currentUserName)
			ElseIf ticketList IsNot Nothing Then ' multiple tickets
				For Each t As KaTicket In ticketList
					t.DoNotEmail = Not emailTicket
					t.SqlUpdate(connection, transaction, applicationName, currentUserName)
				Next
			End If

			transaction.Commit()
			ticketCreated = ticket IsNot Nothing OrElse ticketList IsNot Nothing
		Catch ex As Exception
			If transaction IsNot Nothing Then transaction.Rollback()
			Utilities.CreateEventLogEntry(KaEventLog.Categories.Failure, KaCommonObjects.Alerts.FormatException(ex), Tm2Database.Connection)
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "TicketCreationError", Utilities.JsAlert(ex.Message))
		Finally
			If Not transaction Is Nothing Then transaction.Dispose()
			connection.Close()
		End Try
		Return ticketCreated
	End Function

	Private Function CreateInProgress(connection As OleDbConnection) As KaInProgress
		Dim weighmentDateTime As DateTime = DateTime.Now
		UpdateStagedTransportsWithTransportTares()
		Dim retval As KaInProgress = Nothing
		For Each inProg As KaInProgress In KaInProgress.GetAll(connection, KaInProgress.FN_STAGED_ORDER_ID & " = " & Q(_stagedOrderInfo.Id), "")
			retval = inProg
			Exit For
		Next
		If retval Is Nothing Then
			retval = New KaInProgress()
		End If

		Dim stagedOrderWasSource As Boolean = Page.Request("StagedOrderId") IsNot Nothing
		With retval
			.Weighments.Clear()
			.Orders.Clear()
			.ManufacturedOrders.Clear()
			.Compartments.Clear()

			.UsedBy = "TM2/Point of sale"

			.OrderId = Guid.Empty
			.LocationId = _stagedOrderInfo.LocationId
			.OrderType = KaInProgress.OrderTypes.StagedOrder
			.CarrierId = _stagedOrderInfo.CarrierId
			.DriverId = _stagedOrderInfo.DriverId
			.Notes = _stagedOrderInfo.Notes
			.StagedOrderId = _stagedOrderInfo.Id
			.UseOrderPercents = _stagedOrderInfo.UseOrderPercents
			.PointOfSale = True
			.Id = Guid.NewGuid
			For Each stagedOrder As KaStagedOrderOrder In _stagedOrderInfo.Orders
				Dim order As KaOrder = New KaOrder(connection, stagedOrder.OrderId)

				Dim newInProgOrder As New KaInProgressOrder
				With newInProgOrder
					.Acres = stagedOrder.Acres
					.ApplicatorId = stagedOrder.ApplicatorId
					.CustomerAccountLocationId = stagedOrder.CustomerAccountLocationId
					.InProgressId = retval.Id
					.OrderId = order.Id
					If _stagedOrderInfo.UseOrderPercents Then
						.Percentage = stagedOrder.Percentage
					Else
						.Percentage = 100
					End If
				End With
				.Orders.Add(newInProgOrder)
			Next
			For Each compartment As KaStagedOrderCompartment In _stagedOrderInfo.Compartments
				If compartment.Deleted Then Continue For
				Dim inProgressCompartment As New KaInProgressCompartment()
				With inProgressCompartment
					.Id = Guid.NewGuid()
					.InProgressId = retval.Id
					Try
						.Label = New KaTransportCompartment(connection, compartment.TransportCompartmentId).Position + 1
					Catch ex As Exception
						.Label = compartment.Position + 1
					End Try

					.Position = compartment.Position
					.TransportCompartmentId = compartment.TransportCompartmentId
				End With
				.Compartments.Add(inProgressCompartment)

				' Add discharge storage locations
				If _selectedSourceStorageLocations.ContainsKey(compartment.Id.ToString) Then _selectedSourceStorageLocations(inProgressCompartment.Id.ToString()) = _selectedSourceStorageLocations(compartment.Id.ToString)

				For Each item As KaStagedOrderCompartmentItem In compartment.CompartmentItems
					If item.Deleted Then Continue For
					Dim orderItem As New KaOrderItem(connection, item.OrderItemId)
					Dim product As New KaProduct(connection, orderItem.ProductId)
					Dim delivered As Double = item.Quantity

					Dim requested As Double = 0
					If stagedOrderWasSource AndAlso _originalCompartmentItems.ContainsKey(item.Id) Then
						Try
							requested = KaUnit.FastConvert(New KaQuantity(_originalCompartmentItems(item.Id).Quantity, _originalCompartmentItems(item.Id).UnitId), product.GetDensity(connection, retval.LocationId), item.UnitId, _units).Numeric
						Catch ex As UnitConversionException
							requested = item.Quantity
						End Try
					Else
						requested = item.Quantity
					End If

					Dim allProdBulkProds As ArrayList = KaProductBulkProduct.GetAll(connection, $"{KaProductBulkProduct.FN_PRODUCT_ID} = {Q(product.Id)} AND {KaProductBulkProduct.FN_LOCATION_ID} = {Q(retval.LocationId)} AND {KaProductBulkProduct.FN_DELETED} = 0", "")
					If allProdBulkProds.Count = 0 Then Throw New Exception("No Product/Bulk Product setup for facility: " & CType(ddlFacility.SelectedItem, ListItem).Text)

					For Each prodBulkProd As KaProductBulkProduct In allProdBulkProds
						Dim bulkProduct As New KaBulkProduct(connection, prodBulkProd.BulkProductId)
						If Not bulkProduct.IsFunction(connection) Then
							Dim weighment As New KaInProgressWeighment
							With weighment
								.InProgressId = retval.Id
								.BayId = _stagedOrderInfo.BayId
								.AverageDensity = bulkProduct.Density
								.Complete = True
								.ProductId = product.Id
								.BulkProductId = prodBulkProd.BulkProductId
								.OrderItemId = orderItem.Id
								.Tare = 0
								.TareDate = weighmentDateTime
								.TareManual = True
								.TransportId = Guid.Empty
								For Each transport As KaStagedOrderTransport In _stagedOrderInfo.Transports
									If compartment.StagedOrderTransportId.Equals(transport.Id) Then
										.TransportId = transport.TransportId
										'.Tare = KaUnit.FastConvert(connection, New KaQuantity(transport.TareWeight, transport.TareUnitId), New KaRatio(bulkProduct.Density, bulkProduct.WeightUnitId, bulkProduct.VolumeUnitId), item.UnitId, _units).Numeric
										'.TareDate = transport.TaredAt
										'.TareManual = transport.TareManual
										Exit For
									End If
								Next
								.Gross = delivered * (prodBulkProd.Portion / 100)
								.GrossDate = weighmentDateTime
								.GrossManual = True
								.UnitId = item.UnitId
								.VolumeUnitId = bulkProduct.VolumeUnitId
								.WeightUnitId = bulkProduct.WeightUnitId
								.PanelId = Guid.Parse(ddlPanel.SelectedValue)
								.Delivered = delivered * (prodBulkProd.Portion / 100)
								.Requested = requested * (prodBulkProd.Portion / 100)
								If .Delivered <= 0.0 Then Continue For
								.DeliveredWithoutTempAdjust = delivered * (prodBulkProd.Portion / 100)
								.UseDelivered = True
								.StartDate = weighmentDateTime
								.CompletedDate = weighmentDateTime
								.StagedOrderCompartmentItemId = item.Id
								.Compartment = compartment.Position
							End With
							retval.Weighments.Add(weighment)
						End If
					Next
				Next
			Next
			For Each weighment As KaInProgressWeighment In .Weighments
				weighment.UserId = _currentUser.Id
			Next
			For Each inspectionData As KaStagedOrderInspectionData In _stagedOrderInfo.InspectionData
				Try
					Dim trans As KaTransportInspectionData = New KaTransportInspectionData(connection, inspectionData.TransportInspectionDataId)
					For Each weighment As KaInProgressWeighment In .Weighments
						If weighment.TransportId.Equals(trans.TransportId) AndAlso Not weighment.InspectionDataContainsField(connection, trans.TransportInspectionFieldsId, Nothing) Then
							weighment.InspectionData.Add(New KaInProgressWeighmentsInspectionData() With {
														.TransportInspectionDataId = inspectionData.TransportInspectionDataId})
						End If
					Next
				Catch ex As RecordNotFoundException

				End Try
			Next
		End With
		Return retval
	End Function

	Private Sub AssignStorageLocationTransfers(connection As OleDbConnection, transaction As OleDbTransaction, ticket As KaTicket, weighment As KaInProgressWeighment, ByVal dischargeStorageLocations As List(Of Guid), ByVal selectedCompartmentSourceStorageLocations As List(Of String), ByRef storageLocationMovementsByCompartment As Dictionary(Of String, Dictionary(Of Guid, Dictionary(Of Integer, List(Of StartEndDate)))))
		For Each ti As KaTicketItem In ticket.TicketItems
			For Each tbi As KaTicketBulkItem In ti.TicketBulkItems
				If tbi.InProgressWeighmentId.Equals(weighment.Id) Then
					For Each sl As String In selectedCompartmentSourceStorageLocations
						Dim tbisl As KaTicketBulkItemStorageLocation = New KaTicketBulkItemStorageLocation With {
								.StorageLocationId = Guid.Parse(sl),
								.TicketBulkItemId = tbi.Id
							}
						tbisl.SqlInsert(connection, transaction, Database.ApplicationIdentifier, _currentUser.Name)
						tbi.TicketBulkItemStorageLocations.Add(tbisl)
						Dim minimumValidDate As New DateTime(1900, 1, 1)
						Dim startDateUtc As DateTime = ticket.LoadedAt
						Dim stopDateUtc As DateTime = DateTime.MinValue

						' Set the start dates
						If tbi.StartDate > minimumValidDate Then
							If startDateUtc > tbi.StartDate Then startDateUtc = tbi.StartDate
						ElseIf tbi.CompletedDate > minimumValidDate Then
							If startDateUtc > tbi.CompletedDate Then startDateUtc = tbi.CompletedDate
						End If

						' Set the end dates
						If tbi.CompletedDate > minimumValidDate Then
							If stopDateUtc < tbi.CompletedDate Then stopDateUtc = tbi.CompletedDate
						ElseIf tbi.StartDate > minimumValidDate Then
							If stopDateUtc < tbi.StartDate Then stopDateUtc = tbi.StartDate
						End If
						If stopDateUtc.Equals(DateTime.MinValue) Then stopDateUtc = ticket.LoadedAt

						If Not storageLocationMovementsByCompartment.ContainsKey(sl) Then storageLocationMovementsByCompartment.Add(sl, New Dictionary(Of Guid, Dictionary(Of Integer, List(Of StartEndDate))))
						For Each dischargeStorageLocationId As Guid In dischargeStorageLocations
							If Not storageLocationMovementsByCompartment(sl).ContainsKey(dischargeStorageLocationId) Then storageLocationMovementsByCompartment(sl).Add(dischargeStorageLocationId, New Dictionary(Of Integer, List(Of StartEndDate)))
							If Not storageLocationMovementsByCompartment(sl)(dischargeStorageLocationId).ContainsKey(weighment.Compartment) Then storageLocationMovementsByCompartment(sl)(dischargeStorageLocationId).Add(weighment.Compartment, New List(Of StartEndDate))
							If stopDateUtc < startDateUtc Then
								storageLocationMovementsByCompartment(sl)(dischargeStorageLocationId)(weighment.Compartment).Add(New StartEndDate() With {.StartDate = stopDateUtc, .StopDate = startDateUtc})
							Else
								storageLocationMovementsByCompartment(sl)(dischargeStorageLocationId)(weighment.Compartment).Add(New StartEndDate() With {.StartDate = startDateUtc, .StopDate = stopDateUtc})
							End If
						Next
					Next
				End If
			Next
		Next
	End Sub

	Private Sub InsertStorageLocationMovements(connection As OleDbConnection, transaction As OleDbTransaction, ticket As KaTicket, storageLocationMovementsByCompartment As Dictionary(Of String, Dictionary(Of Guid, Dictionary(Of Integer, List(Of StartEndDate)))), applicationName As String, currentUserName As String)
		Dim compartmentsUsed As List(Of Integer)
		For Each ticketFunction As KaTicketFunction In ticket.TicketFunctions
			If ticketFunction.TicketItemId.Equals(Guid.Empty) Then
				For Each sourceSlId As String In storageLocationMovementsByCompartment.Keys
					For Each destSlId As Guid In storageLocationMovementsByCompartment(sourceSlId).Keys
						compartmentsUsed = New List(Of Integer)(storageLocationMovementsByCompartment(sourceSlId)(destSlId).Keys)
						If compartmentsUsed.Count > 0 Then
							Dim compartmentStartEndDates As List(Of StartEndDate) = storageLocationMovementsByCompartment(sourceSlId)(destSlId)(0)
							For Each sed As StartEndDate In compartmentStartEndDates
								sed.StopDate = ticket.LoadedAt
							Next
							For comparmentKeyIndex As Integer = 1 To compartmentsUsed.Count - 1
								Dim compartmentToRemoveIndex As Integer = compartmentsUsed(comparmentKeyIndex)
								For Each sed As StartEndDate In storageLocationMovementsByCompartment(sourceSlId)(destSlId)(compartmentToRemoveIndex)
									compartmentStartEndDates.Add(New StartEndDate() With {.StartDate = sed.StartDate, .StopDate = ticket.LoadedAt})
								Next
								storageLocationMovementsByCompartment(sourceSlId)(destSlId).Remove(compartmentToRemoveIndex)
							Next
						End If
					Next
				Next
				Exit For
			End If
		Next

		Dim sourceSlIds As List(Of String) = New List(Of String)(storageLocationMovementsByCompartment.Keys)
		For Each sourceSlId As String In sourceSlIds
			Dim slSourceId As Guid = Guid.Parse(sourceSlId)
			Dim destSlIds As List(Of Guid) = New List(Of Guid)(storageLocationMovementsByCompartment(sourceSlId).Keys)
			For Each destSlId As Guid In destSlIds
				compartmentsUsed = New List(Of Integer)(storageLocationMovementsByCompartment(sourceSlId)(destSlId).Keys)
				For Each compartmentsUsedIndex As Integer In compartmentsUsed
					If storageLocationMovementsByCompartment(sourceSlId)(destSlId).ContainsKey(compartmentsUsedIndex) AndAlso storageLocationMovementsByCompartment(sourceSlId)(destSlId).ContainsKey(compartmentsUsedIndex + 1) Then
						' There is a storage movement for the next compartment as well, so combine them together
						Dim firstCompartmentSlmDates As List(Of StartEndDate) = storageLocationMovementsByCompartment(sourceSlId)(destSlId)(compartmentsUsedIndex)
						Dim secondCompartmentSlmDates As List(Of StartEndDate) = storageLocationMovementsByCompartment(sourceSlId)(destSlId)(compartmentsUsedIndex + 1)
						If firstCompartmentSlmDates IsNot Nothing AndAlso secondCompartmentSlmDates IsNot Nothing Then
							Dim compartmentStartEndDates As List(Of StartEndDate) = New List(Of StartEndDate)
							For Each sed As StartEndDate In storageLocationMovementsByCompartment(sourceSlId)(destSlId)(compartmentsUsedIndex)
								compartmentStartEndDates.Add(New StartEndDate() With {.StartDate = sed.StartDate, .StopDate = sed.StopDate})
							Next
							For Each sed As StartEndDate In storageLocationMovementsByCompartment(sourceSlId)(destSlId)(compartmentsUsedIndex + 1)
								compartmentStartEndDates.Add(New StartEndDate() With {.StartDate = sed.StartDate, .StopDate = sed.StopDate})
							Next
							compartmentStartEndDates = StartEndDate.CombineStartEndDates(compartmentStartEndDates)
							If compartmentStartEndDates.Count = 1 Then
								storageLocationMovementsByCompartment(sourceSlId)(destSlId)(compartmentsUsedIndex + 1) = New List(Of StartEndDate)({
										New StartEndDate() With {.StartDate = compartmentStartEndDates(0).StartDate, .StopDate = compartmentStartEndDates(0).StopDate}
									})
								storageLocationMovementsByCompartment(sourceSlId)(destSlId).Remove(compartmentsUsedIndex)
							End If
						End If
					End If
				Next
			Next

			For Each destSlId As Guid In destSlIds
				compartmentsUsed = New List(Of Integer)(storageLocationMovementsByCompartment(sourceSlId)(destSlId).Keys)
				For Each compartmentsUsedIndex As Integer In compartmentsUsed
					Dim slmDates As List(Of StartEndDate) = StartEndDate.CombineStartEndDates(storageLocationMovementsByCompartment(sourceSlId)(destSlId)(compartmentsUsedIndex))
					For Each slmDate As StartEndDate In slmDates
						Dim slm As KaStorageLocationMovement = New KaStorageLocationMovement() With {
								.ConfirmedEmpty = False,
								.TicketId = ticket.Id,
								.TransferFromStorageLocationId = slSourceId,
								.StartDate = slmDate.StartDate,
								.StopDate = slmDate.StopDate,
								.StorageLocationId = destSlId
							}
						slm.SqlUpdateInsertIfNotFound(connection, transaction, applicationName, currentUserName)
					Next
				Next
			Next
		Next
	End Sub

	''' <summary>
	''' This will update the Staged Order Transport Tare information with the tare information from the transport, if it has not been set previously
	''' </summary>
	''' <remarks></remarks>
	Private Sub UpdateStagedTransportsWithTransportTares()
		With _stagedOrderInfo
			' Check if the tare weights have been assigned. If not, assign them.
			For Each currentStagedTransport As KaStagedOrderTransport In .Transports
				If Not currentStagedTransport.Deleted AndAlso currentStagedTransport.TareWeight = 0 AndAlso currentStagedTransport.TaredAt <= New DateTime(1900, 1, 1, 0, 0, 0) Then
					' The tare weights have not been set, so set them to the transports current values
					Try
						Dim transportInfo As New KaTransport(GetUserConnection(_currentUser.Id), currentStagedTransport.TransportId)
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

	Private Sub btnCreateTicket_Click(sender As Object, e As System.EventArgs) Handles btnCreateTicket.Click
		Dim returnPage As String = "Orders.aspx"
		_stagedOrderInfo = ConvertPageToStagedOrder()
		If ValidateStagedOrder() AndAlso CreatePointOfSaleTicket() Then
			If Page.Request("StagedOrderId") IsNot Nothing Then
				returnPage = "StagedOrders.aspx?StagedOrderId=" & Page.Request("StagedOrderId")
			ElseIf Page.Request("OrderId") IsNot Nothing Then
				Try
					Dim order As New KaOrder(GetUserConnection(_currentUser.Id), Guid.Parse(Page.Request("OrderId")))
					If order.Completed Then
						returnPage = "PastOrders.aspx?orderId=" & Page.Request("OrderId")
					Else
						returnPage = "Orders.aspx?orderId=" & Page.Request("OrderId")
					End If
				Catch ex As Exception
					returnPage = "Orders.aspx?orderId=" & Page.Request("OrderId")
				End Try
			End If
			Response.Redirect(returnPage)
		Else
			btnCreateTicket.Enabled = True
		End If
	End Sub

	Private Sub TransportChanged(sender As Object, e As System.EventArgs)
		Dim transportRow As HtmlGenericControl = CType(sender, DropDownList).Parent.Parent
		Dim controlIdStrings() As String = CType(sender, DropDownList).ID.Split("_")
		Dim currentTransportRowId As String = controlIdStrings(0)
		Dim currentStagedTransportId As Guid = Guid.Parse(CType(transportRow.FindControl(controlIdStrings(0) & "_Id"), HtmlInputHidden).Value)
		Dim transportId As Guid = Guid.Empty
		Dim transportList As DropDownList = sender
		Guid.TryParse(transportList.SelectedValue, transportId)
		For Each transport As KaStagedOrderTransport In _stagedOrderInfo.Transports
			If transport.Id.Equals(currentStagedTransportId) Then
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

	Private Sub btnCancel_Click(sender As Object, e As System.EventArgs) Handles btnCancel.Click
		If Page.Request("StagedOrderId") IsNot Nothing Then
			Response.Redirect("StagedOrders.aspx?StagedOrderId=" & Page.Request("StagedOrderId"))
		ElseIf Page.Request("OrderId") IsNot Nothing Then
			Response.Redirect("Orders.aspx?orderId=" & Page.Request("OrderId"))
		End If
	End Sub

	Private Sub ddlCarrier_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles ddlCarrier.SelectedIndexChanged
		Guid.TryParse(ddlCarrier.SelectedValue, _stagedOrderInfo.CarrierId)

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

	Protected Sub ddlOrderApplicator_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlOrderApplicator.SelectedIndexChanged
		Dim currentStagedOrderOrdersList As List(Of KaStagedOrderOrder) = GetCurrentStagedOrderOrders()
		If lstUsedOrders.SelectedIndex >= 0 Then
			Dim stagedOrderOrder As KaStagedOrderOrder = GetStagedOrderOrder(lstUsedOrders.SelectedValue)
			For Each currentOrder As KaStagedOrderOrder In currentStagedOrderOrdersList
				If currentOrder.OrderId = stagedOrderOrder.OrderId Then
					If Not Guid.TryParse(ddlOrderApplicator.SelectedValue.ToString, currentOrder.ApplicatorId) Then
						ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidApplicator", Utilities.JsAlert("Unable to assign applicator."))
						tbxOrderAcres.Focus()
					End If
					SetStagedOrderOrderDataOnOrderList(currentOrder)
					Exit For
				End If
			Next
		End If
	End Sub

	Protected Sub tbxOrderAcres_TextChanged(sender As Object, e As EventArgs) Handles tbxOrderAcres.TextChanged
		Dim currentStagedOrderOrdersList As List(Of KaStagedOrderOrder) = GetCurrentStagedOrderOrders()
		If lstUsedOrders.SelectedIndex >= 0 Then
			Dim stagedOrderOrder As KaStagedOrderOrder = GetStagedOrderOrder(lstUsedOrders.SelectedValue)
			For Each currentOrder As KaStagedOrderOrder In currentStagedOrderOrdersList
				If currentOrder.OrderId = stagedOrderOrder.OrderId Then
					If Not Double.TryParse(tbxOrderAcres.Text, currentOrder.Acres) Then
						ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidAcres", Utilities.JsAlert("A valid number must be entered for the acres."))
						tbxOrderAcres.Focus()
					End If
					SetStagedOrderOrderDataOnOrderList(currentOrder)
					Exit For
				End If
			Next
		End If
	End Sub

	Private Sub SetStagedOrderOrderDataOnOrderList(ByVal stagedOrderOrder As KaStagedOrderOrder)
		Dim currentSelectedIndex As Integer = lstUsedOrders.SelectedIndex
		Dim orderInfo As New KaOrder(GetUserConnection(_currentUser.Id), stagedOrderOrder.OrderId)
		If currentSelectedIndex >= 0 Then
			lstUsedOrders.Items.RemoveAt(currentSelectedIndex)
		Else
			currentSelectedIndex = lstUsedOrders.Items.Count
		End If
		lstUsedOrders.Items.Insert(currentSelectedIndex, New ListItem(orderInfo.Number.Trim, GetStagedOrderOrderXml(stagedOrderOrder)))
		lstUsedOrders.SelectedIndex = currentSelectedIndex
		lstUsedOrders_SelectedIndexChanged(lstUsedOrders, New EventArgs())
	End Sub

	Private Function GetUnit(ByVal connection As OleDbConnection, ByVal unitId As Guid, ByRef units As Dictionary(Of Guid, KaUnit)) As KaUnit
		If Not units.ContainsKey(unitId) Then
			Try
				units.Add(unitId, New KaUnit(connection, unitId))
			Catch ex As RecordNotFoundException
				Return New KaUnit
			End Try
		End If
		Return units(unitId)
	End Function

	'<Serializable()>
	'Public Class OriginalTareInfo
	'	Public StagedTransportId As Guid = Guid.Empty
	'	Public OriginalTareInfo As New KaTimestampedQuantity(0, Guid.Empty, Now, False)

	'	Public Sub New()
	'	End Sub

	'	Public Sub New(id As Guid, tareWeight As Double, tareDate As DateTime, tareWeightUofM As Guid, tareManual As Boolean)
	'		StagedTransportId = id
	'		OriginalTareInfo = New KaTimestampedQuantity(tareWeight, tareWeightUofM, tareDate, tareManual)
	'	End Sub
	'End Class

	Private Sub SetTextboxMaxLengths()
		tbxOrderAcres.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaStagedOrderOrder.TABLE_NAME, KaStagedOrderOrder.FN_ACRES))
	End Sub

	Private Sub ddlBayAssigned_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles ddlBayAssigned.SelectedIndexChanged
		Guid.TryParse(ddlBayAssigned.SelectedValue, _stagedOrderInfo.BayId)
		PopulatePanelsList()
		GetCustomQuestionInspectionQuestionsList()
	End Sub
	Protected Sub ScriptManager1_AsyncPostBackError(ByVal sender As Object, ByVal e As System.Web.UI.AsyncPostBackErrorEventArgs)
		ToolkitScriptManager1.AsyncPostBackErrorMessage = e.Exception.Message
	End Sub
End Class
