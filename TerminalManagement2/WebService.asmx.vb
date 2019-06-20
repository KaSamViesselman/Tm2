Imports System.Web.Services
Imports System.ComponentModel
Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb
Imports System.Xml.Serialization
Imports System.Reflection

' To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line.
' <System.Web.Script.Services.ScriptService()> _
<System.Web.Services.WebService(Namespace:="KahlerAutomation")>
<System.Web.Services.WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)>
<ToolboxItem(False)>
Public Class WebService : Inherits System.Web.Services.WebService
	<WebMethod()> Public Function GetNavigationStructure() As List(Of Tm2Page)
		Return Utilities.GetListOfPagesForUser(Utilities.GetUser(User), False)
	End Function

	<WebMethod()> Public Function GetWebPageTitle() As String
		Return KaSetting.GetSetting(Tm2Database.GetDbConnection(), KaSetting.SN_WEB_PAGE_TITLE, "")
	End Function

    <WebMethod()> Public Function GetDisplayNotification() As Boolean
        Return Utilities.GetDisplayNotification()
    End Function

    <WebMethod()> Public Sub ExecuteNonQuery(command As String)
		Dim c As New OleDbConnection(Tm2Database.GetDbConnection())
		Try
			c.Open()
			Tm2Database.ExecuteNonQuery(c, command)
		Finally
			c.Close()
		End Try
	End Sub

	<WebMethod()> Public Function GetAllAnalysis(conditions As String, sortBy As String) As KaAnalysis()
		Return KaAnalysis.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaAnalysis))
	End Function

	<WebMethod()> Public Function GetAllApplicators(conditions As String, sortBy As String) As KaApplicator()
		Return KaApplicator.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaApplicator))
	End Function

	<WebMethod()> Public Function GetAllBays(conditions As String, sortBy As String) As KaBay()
		Return KaBay.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaBay))
	End Function

	<WebMethod()> Public Function GetAllBranches(conditions As String, sortBy As String) As KaBranch()
		Return KaBranch.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaBranch))
	End Function

	<WebMethod()> Public Function GetAllBulkProducts(conditions As String, sortBy As String) As KaBulkProduct()
		Return KaBulkProduct.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaBulkProduct))
	End Function

	<WebMethod()> Public Function GetAllBulkProductCropTypes(conditions As String, sortBy As String) As KaBulkProductCropType()
		Return KaBulkProductCropType.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaBulkProductCropType))
	End Function

	<WebMethod()> Public Function GetAllBulkProductInterfaceSettings(conditions As String, sortBy As String) As KaBulkProductInterfaceSettings()
		Return KaBulkProductInterfaceSettings.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaBulkProductInterfaceSettings))
	End Function

	<WebMethod()> Public Function GetAllBulkProductInventories(conditions As String, sortBy As String) As KaBulkProductInventory()
		Return KaBulkProductInventory.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaBulkProductInventory))
	End Function

	<WebMethod()> Public Function GetAllBulkProductPanelSettings(conditions As String, sortBy As String) As KaBulkProductPanelSettings()
		Return KaBulkProductPanelSettings.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaBulkProductPanelSettings))
	End Function

	<WebMethod()> Public Function GetAllCarriers(conditions As String, sortBy As String) As KaCarrier()
		Return KaCarrier.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaCarrier))
	End Function

	<WebMethod()> Public Function GetAllContainers(conditions As String, sortBy As String) As KaContainer()
		Return KaContainer.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaContainer))
	End Function

	<WebMethod()> Public Function GetAllContainerEquipments(conditions As String, sortBy As String) As KaContainerEquipment()
		Return KaContainerEquipment.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaContainerEquipment))
	End Function

	<WebMethod()> Public Function GetAllContainerEquipmentTypes(conditions As String, sortBy As String) As KaContainerEquipmentType()
		Return KaContainerEquipmentType.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaContainerEquipmentType))
	End Function

	<WebMethod()> Public Function GetAllContainerTypes(conditions As String, sortBy As String) As KaContainerType()
		Return KaContainerType.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaContainerType))
	End Function

	<WebMethod()> Public Function GetAllCropTypes(conditions As String, sortBy As String) As KaCropType()
		Return KaCropType.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaCropType))
	End Function

	<WebMethod()> Public Function GetAllCustomerAccounts(conditions As String, sortBy As String) As KaCustomerAccount()
		Return KaCustomerAccount.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaCustomerAccount))
	End Function

	<WebMethod()> Public Function GetAllCustomerAccountCombinations(conditions As String, sortBy As String) As KaCustomerAccountCombination()
		Return KaCustomerAccountCombination.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaCustomerAccountCombination))
	End Function

	<WebMethod()> Public Function GetAllCustomerAccountDrivers(conditions As String, sortBy As String) As KaCustomerAccountDriver()
		Return KaCustomerAccountDriver.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaCustomerAccountDriver))
	End Function

	<WebMethod()> Public Function GetAllCustomerAccountLocations(conditions As String, sortBy As String) As KaCustomerAccountLocation()
		Return KaCustomerAccountLocation.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaCustomerAccountLocation))
	End Function

	<WebMethod()> Public Function GetAllDischargeLocations(conditions As String, sortBy As String) As KaDischargeLocation()
		Return KaDischargeLocation.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaDischargeLocation))
	End Function

	<WebMethod()> Public Function GetAllDischargeLocationPanelSettings(conditions As String, sortBy As String) As KaDischargeLocationPanelSettings()
		Return KaDischargeLocationPanelSettings.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaDischargeLocationPanelSettings))
	End Function

	<WebMethod()> Public Function GetAllDrivers(conditions As String, sortBy As String) As KaDriver()
		Return KaDriver.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaDriver))
	End Function

	<WebMethod()> Public Function GetAllDriverInFacilities(conditions As String, sortBy As String) As KaDriverInFacility()
		Return KaDriverInFacility.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaDriverInFacility))
	End Function

	<WebMethod()> Public Function GetAllEventLogs(conditions As String, sortBy As String) As KaEventLog()
		Return KaEventLog.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaEventLog))
	End Function

	<WebMethod()>
	Public Function GetAllInProgresses(conditions As String, sortBy As String) As KaInProgress()
		Return KaInProgress.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaInProgress))
	End Function

	<WebMethod()> Public Function GetAllInProgressOrders(conditions As String, sortBy As String) As KaInProgressOrder()
		Return KaInProgressOrder.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaInProgressOrder))
	End Function

	<WebMethod()> Public Function GetAllInProgressWeighments(conditions As String, sortBy As String) As KaInProgressWeighment()
		Return KaInProgressWeighment.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaInProgressWeighment))
	End Function

	<WebMethod()> Public Function GetAllInterfaces(conditions As String, sortBy As String) As KaInterface()
		Return KaInterface.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaInterface))
	End Function

	<WebMethod()> Public Function GetAllInventoryChanges(conditions As String, sortBy As String) As KaInventoryChange()
		Return KaInventoryChange.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaInventoryChange))
	End Function

	<WebMethod()> Public Function GetAllLocations(conditions As String, sortBy As String) As KaLocation()
		Return KaLocation.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaLocation))
	End Function

	<WebMethod()> Public Function GetAllOrders(conditions As String, sortBy As String) As KaOrder()
		Dim l As ArrayList = KaOrder.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy)
		For Each order As KaOrder In l
			order.OrderItems.Clear()
			order.OrderAccounts.Clear()
		Next
		Return l.ToArray(GetType(KaOrder))
	End Function

	<WebMethod()> Public Function GetAllOrderItems(conditions As String, sortBy As String) As KaOrderItem()
		Return KaOrderItem.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaOrderItem))
	End Function

	<WebMethod()> Public Function GetAllOrderCustomerAccounts(conditions As String, sortBy As String) As KaOrderCustomerAccount()
		Return KaOrderCustomerAccount.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaOrderCustomerAccount))
	End Function

	<WebMethod()> Public Function GetAllOwners(conditions As String, sortBy As String) As KaOwner()
		Return KaOwner.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaOwner))
	End Function

	<WebMethod()> Public Function GetAllPanels(conditions As String, sortBy As String) As KaPanel()
		Return KaPanel.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaPanel))
	End Function

	<WebMethod()> Public Function GetAllProducts(conditions As String, sortBy As String) As KaProduct()
		Dim list As ArrayList = KaProduct.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy)
		For i As Integer = 0 To list.Count - 1
			CType(list(i), KaProduct).ProductBulkItems.Clear()
			CType(list(i), KaProduct).ProductInterfaces.Clear()
		Next
		Return list.ToArray(GetType(KaProduct))
	End Function

	<WebMethod()> Public Function GetAllProductBulkProducts(conditions As String, sortBy As String) As KaProductBulkProduct()
		Return KaProductBulkProduct.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaProductBulkProduct))
	End Function

	<WebMethod()> Public Function GetAllProductInterfaceSettings(conditions As String, sortBy As String) As KaProductInterfaceSettings()
		Return KaProductInterfaceSettings.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaProductInterfaceSettings))
	End Function

	<WebMethod()> Public Function GetAllReceivingPurchaseOrders(conditions As String, sortBy As String) As KaReceivingPurchaseOrder()
		Return KaReceivingPurchaseOrder.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaReceivingPurchaseOrder))
	End Function

	<WebMethod()> Public Function GetAllReceivingTickets(conditions As String, sortBy As String) As KaReceivingTicket()
		Return KaReceivingTicket.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaReceivingTicket))
	End Function

	<WebMethod()> Public Function GetAllSeals(conditions As String, sortBy As String) As KaSeal()
		Return KaSeal.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaSeal))
	End Function

	<WebMethod()> Public Function GetAllStagedOrders(conditions As String, sortBy As String) As KaStagedOrder()
		Return KaStagedOrder.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaStagedOrder))
	End Function

	<WebMethod()> Public Function GetAllStagedOrderCompartments(conditions As String, sortBy As String) As KaStagedOrderCompartment()
		Return KaStagedOrderCompartment.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaStagedOrderCompartment))
	End Function

	<WebMethod()> Public Function GetAllStagedOrderTransports(conditions As String, sortBy As String) As KaStagedOrderTransport()
		Return KaStagedOrderTransport.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaStagedOrderTransport))
	End Function

	<WebMethod()> Public Function GetAllSupplierAccounts(conditions As String, sortBy As String) As KaSupplierAccount()
		Return KaSupplierAccount.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaSupplierAccount))
	End Function

	<WebMethod()> Public Function GetAllTanks(conditions As String, sortBy As String) As KaTank()
		Return KaTank.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaTank))
	End Function

	<WebMethod()> Public Function GetAllTankAlarmHistories(conditions As String, sortBy As String) As KaTankAlarmHistory()
		Return KaTankAlarmHistory.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaTankAlarmHistory))
	End Function

	<WebMethod()> Public Function GetAllTankGroups(conditions As String, sortBy As String) As KaTankGroup()
		Return KaTankGroup.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaTankGroup))
	End Function

	<WebMethod()> Public Function GetAllTankGroupTanks(conditions As String, sortBy As String) As KaTankGroupTank()
		Return KaTankGroupTank.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaTankGroupTank))
	End Function

	<WebMethod()> Public Function GetAllTankLevelTrends(conditions As String, sortBy As String) As KaTankLevelTrend()
		Return KaTankLevelTrend.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaTankLevelTrend))
	End Function

	<WebMethod()> Public Function GetAllTankLevelTrendData(conditions As String, sortBy As String) As KaTankLevelTrendData()
		Return KaTankLevelTrendData.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaTankLevelTrendData))
	End Function

	<WebMethod()> Public Function GetAllTickets(conditions As String, sortBy As String) As KaTicket()
		Return KaTicket.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaTicket))
	End Function

	<WebMethod()> Public Function GetAllTicketBulkItems(conditions As String, sortBy As String) As KaTicketBulkItem()
		Return KaTicketBulkItem.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaTicketBulkItem))
	End Function

	<WebMethod()> Public Function GetAllTicketBulkItemAnlysis(conditions As String, sortBy As String) As KaTicketBulkItemAnalysis()
		Return KaTicketBulkItemAnalysis.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaTicketBulkItemAnalysis))
	End Function

	<WebMethod()> Public Function GetAllTicketBulkItemCropTypes(conditions As String, sortBy As String) As KaTicketBulkItemCropTypes()
		Return KaTicketBulkItemCropTypes.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaTicketBulkItemCropTypes))
	End Function

	<WebMethod()> Public Function GetAllTicketCustomerAccounts(conditions As String, sortBy As String) As KaTicketCustomerAccount()
		Return KaTicketCustomerAccount.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaTicketCustomerAccount))
	End Function

	<WebMethod()> Public Function GetAllTicketItems(conditions As String, sortBy As String) As KaTicketItem()
		Return KaTicketItem.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaTicketItem))
	End Function

	<WebMethod()> Public Function GetAllTicketTransports(conditions As String, sortBy As String) As KaTicketTransport()
		Return KaTicketTransport.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaTicketTransport))
	End Function

	<WebMethod()> Public Function GetAllTracks(conditions As String, sortBy As String) As KaTrack()
		Return KaTrack.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaTrack))
	End Function

	<WebMethod()> Public Function GetAllTransports(conditions As String, sortBy As String) As KaTransport()
		Return KaTransport.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaTransport))
	End Function

	<WebMethod()> Public Function GetAllTransportCompartments(conditions As String, sortBy As String) As KaTransportCompartment()
		Return KaTransportCompartment.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaTransportCompartment))
	End Function

	<WebMethod()> Public Function GetAllTransportInFacilities(conditions As String, sortBy As String) As KaTransportInFacility()
		Return KaTransportInFacility.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaTransportInFacility))
	End Function

	<WebMethod()> Public Function GetAllTransportInspectionData(conditions As String, sortBy As String) As KaTransportInspectionData()
		Return KaTransportInspectionData.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaTransportInspectionData))
	End Function

	<WebMethod()> Public Function GetAllTransportInspectionFields(conditions As String, sortBy As String) As KaTransportInspectionFields()
		Return KaTransportInspectionFields.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaTransportInspectionFields))
	End Function

	<WebMethod()> Public Function GetAllTransportsOnTracks(conditions As String, sortBy As String) As KaTransportsOnTracks()
		Return KaTransportsOnTracks.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaTransportsOnTracks))
	End Function

	<WebMethod()> Public Function GetAllTransportTypes(conditions As String, sortBy As String) As KaTransportTypes()
		Return KaTransportTypes.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaTransportTypes))
	End Function

	<WebMethod()> Public Function GetAllUnits(conditions As String, sortBy As String) As KaUnit()
		Return KaUnit.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaUnit))
	End Function

	<WebMethod()>
	<XmlInclude(GetType(KaUserPermission))>
	<XmlInclude(GetType(System.Drawing.Bitmap))>
	Public Function GetAllUsers(conditions As String, sortBy As String) As KaUser()
		Return KaUser.GetAll(Tm2Database.GetDbConnection(), conditions, sortBy).ToArray(GetType(KaUser))
	End Function

	<WebMethod()> Public Function GetAllOrdersForContainer(containerId As Guid) As KaOrder()
		Dim connection As OleDbConnection = GetUserConnection(Utilities.GetUser(User).Id)
		Dim container As New KaContainer(connection, containerId)
		Dim bulkProduct As New KaBulkProduct(connection, container.BulkProductId)
		Dim product As New KaProduct(connection, Database.GetProductIdForBulkProductId(connection, container.BulkProductId))
		Dim list As New ArrayList()
		Dim orders As String = String.Format("[id]={0}", Q(Guid.Empty))
		For Each orderItem As KaOrderItem In KaOrderItem.GetAll(connection, String.Format("[deleted]=0 AND [delivered] < [request] AND [product_id]={0}", Q(product.Id)), "")
			If orders.IndexOf(orderItem.OrderId.ToString()) = -1 Then
				orders &= String.Format(" OR [id]={0}", Q(orderItem.OrderId))
			End If
		Next
		Dim ownerId As Guid
		If Not container.OwnerId.Equals(Guid.Empty) Then
			ownerId = container.OwnerId
		ElseIf Not bulkProduct.OwnerId.Equals(Guid.Empty) Then
			ownerId = bulkProduct.OwnerId
		ElseIf Not product.OwnerId.Equals(Guid.Empty) Then
			ownerId = product.OwnerId
		Else
			ownerId = Guid.Empty
		End If
		For Each order As KaOrder In KaOrder.GetAll(connection, String.Format("[deleted]=0 AND ({0})" & IIf(ownerId.Equals(Guid.Empty), "", " AND [owner_id]={1}"), orders, Q(ownerId)), "[number] ASC")
			order.OrderAccounts.Clear()
			order.OrderItems.Clear()
			list.Add(order)
		Next
		Return list.ToArray(GetType(KaOrder))
	End Function

	<WebMethod()> Public Function GetContainerWithNumber(number As String) As KaContainer()
		Dim connection As OleDbConnection = GetUserConnection(Utilities.GetUser(User).Id)
		Dim list As ArrayList = KaContainer.GetAll(connection, String.Format("[deleted]=0 AND [number]={0}", Q(number)), "[number] ASC")
		If list.Count = 1 Then
			Return list.ToArray(GetType(KaContainer))
		Else
			list = KaContainer.GetAll(connection, String.Format("[deleted]=0 AND [number] LIKE {0}", Q(String.Format("%{0}%", number))), "[number] ASC")
			Return list.ToArray(GetType(KaContainer))
		End If
	End Function

	<WebMethod()> Public Function CreateNewContainer(number As String) As Guid
		Dim user As KaUser = Utilities.GetUser(Me.User)
		Dim connection As OleDbConnection = GetUserConnection(user.Id)
		If KaContainer.GetAll(connection, String.Format("[deleted]=0 AND [number]={0}", Q(number)), "").Count > 0 Then
			Throw New TooManyRecordsException(String.Format("A container with number {0} already exists.", number))
		Else
			Dim container As New KaContainer() With {
				.VolumeUnitId = KaUnit.GetUnitForBaseUnit(connection, KaUnit.Unit.Gallons).Id,
				.WeightUnitId = KaUnit.GetUnitForBaseUnit(connection, KaUnit.Unit.Pounds).Id,
				.Number = number
			}
			container.SqlInsert(connection, Nothing, Database.ApplicationIdentifier, user.Username)
			Return container.Id
		End If
	End Function

	<WebMethod()> Public Sub SellContainer(containerId As Guid, orderId As Guid)
		Dim user As KaUser = Utilities.GetUser(Me.User)
		Dim connection As OleDbConnection = GetUserConnection(user.Id)
		Dim container As New KaContainer(connection, containerId)
		Dim packagedInventoryLocationId As Guid = Utilities.GetContainerPackagedInventoryLocationId(connection)
		container.Sell(connection, Nothing, orderId, packagedInventoryLocationId, APPLICATION_ID, user.Username)
	End Sub

	<WebMethod()> Public Sub ReturnContainer(containerId As Guid, grossWeight As Double, unitId As Guid, facilityLocationId As Guid)
		Dim user As KaUser = Utilities.GetUser(Me.User)
		Dim connection As OleDbConnection = GetUserConnection(user.Id)
		Dim container As New KaContainer(connection, containerId)
		Dim packagedInventoryLocationId As Guid = Utilities.GetContainerPackagedInventoryLocationId(connection)
		container.CheckIn(connection, Nothing, New KaQuantity(grossWeight, unitId), facilityLocationId, packagedInventoryLocationId, APPLICATION_ID, user.Username)
	End Sub

	<WebMethod()> Public Sub ChangeContainerBulkProduct(containerId As Guid, bulkProductId As Guid, ownerId As Guid)
		Dim user As KaUser = Utilities.GetUser(Me.User)
		Dim connection As OleDbConnection = GetUserConnection(user.Id)
		Dim container As New KaContainer(connection, containerId)
		Dim previous As KaContainer = container.Clone()
		container.BulkProductId = bulkProductId
		Dim packagedInventoryLocationId As Guid = Utilities.GetContainerPackagedInventoryLocationId(connection)
		container.HandleBulkProductChange(connection, Nothing, previous, container.LocationId, packagedInventoryLocationId, ownerId, APPLICATION_ID, user.Username)
		container.SqlUpdate(connection, Nothing, APPLICATION_ID, user.Username)
	End Sub

	<WebMethod()> Public Sub ChangeContainerProductWeight(containerId As Guid, productWeight As Double, ownerId As Guid)
		Dim user As KaUser = Utilities.GetUser(Me.User)
		Dim connection As OleDbConnection = GetUserConnection(user.Id)
		Dim container As New KaContainer(connection, containerId)
		Dim previous As KaContainer = container.Clone()
		container.ProductWeight = productWeight
		Dim packagedInventoryLocationId As Guid = Utilities.GetContainerPackagedInventoryLocationId(connection)
		container.HandleBulkProductChange(connection, Nothing, previous, container.LocationId, packagedInventoryLocationId, ownerId, APPLICATION_ID, user.Username)
		container.SqlUpdate(connection, Nothing, APPLICATION_ID, user.Username)
	End Sub

	<WebMethod()> Public Function GetContainerEquipmentWithNumber(number As String) As KaContainerEquipment()
		Dim connection As OleDbConnection = GetUserConnection(Utilities.GetUser(User).Id)
		Dim list As ArrayList = KaContainerEquipment.GetAll(connection, String.Format("[deleted]=0 AND [barcode_id]={0}", Q(number)), "[name] ASC")
		If list.Count = 1 Then
			Return list.ToArray(GetType(KaContainerEquipment))
		Else
			list = KaContainerEquipment.GetAll(connection, String.Format("[deleted]=0 AND [barcode_id] LIKE {0}", Q(String.Format("%{0}%", number))), "[name] ASC")
			Return list.ToArray(GetType(KaContainerEquipment))
		End If
	End Function

	<WebMethod()> Public Function CreateNewContainerEquipment(number As String) As Guid
		Dim user As KaUser = Utilities.GetUser(Me.User)
		Dim connection As OleDbConnection = GetUserConnection(user.Id)
		If KaContainerEquipment.GetAll(connection, String.Format("[deleted]=0 AND [barcode_id]={0}", Q(number)), "").Count > 0 Then
			Throw New TooManyRecordsException(String.Format("Container equipment with number {0} already exists.", number))
		Else
			Dim equipment As New KaContainerEquipment() With {.Name = number, .BarcodeId = number}
			equipment.SqlInsert(connection, Nothing, Database.ApplicationIdentifier, user.Username)
			Return equipment.Id
		End If
	End Function

	Private Function GetSqlName(name As String) As String
		Dim sqlName As String = ""
		For i As Integer = 0 To name.Length - 1
			Dim c As String = name.Substring(i, 1)
			If c.Equals(c.ToUpper()) Then
				If i > 0 Then sqlName &= "_"
				c = c.ToLower()
			End If
			sqlName &= c
		Next
		Return sqlName
	End Function

	<WebMethod()> Public Sub UpdateRecord(recordType As String, id As Guid, propertyName As String, value As String)
		Dim assembly As Assembly = Assembly.GetAssembly(GetType(KaUser))
		Dim type As Type = assembly.GetType(String.Format("KahlerAutomation.KaTm2Database.{0}", recordType))
		Dim prop As PropertyInfo = type.GetProperty(propertyName)
		Dim parsedValue As Object
		If prop.PropertyType.IsEnum Then
			parsedValue = [Enum].Parse(prop.PropertyType, value)
		Else
			Dim method As MethodInfo = prop.PropertyType.GetMethod("Parse", New Type() {GetType(String)})
			If method IsNot Nothing Then
				parsedValue = method.Invoke(Nothing, New Object() {value})
			Else
				parsedValue = value
			End If
		End If
		Dim user As KaUser = Utilities.GetUser(Me.User)
		Dim connection As OleDbConnection = GetUserConnection(user.Id)
		Dim requireUniqueness As Boolean =
			(recordType.Equals("KaContainer") AndAlso propertyName.Equals("Number")) OrElse
			(recordType.Equals("KaContainerEquipment") AndAlso propertyName.Equals("BarcodeId"))
		Dim command As OleDbCommand
		Dim reader As OleDbDataReader
		Dim tableName As String = GetSqlName(recordType.Substring(2, recordType.Length - 2))
		command = New OleDbCommand("EXEC sp_tables", connection)
		reader = command.ExecuteReader()
		Try
			Do While reader.Read()
				If reader("table_name").Equals(tableName) Then
					Exit Do
				ElseIf reader("table_name").Equals(tableName & "s") Then
					tableName &= "s"
					Exit Do
				ElseIf reader("table_name").Equals(tableName & "es") Then
					tableName &= "es"
					Exit Do
				End If
			Loop
		Finally
			reader.Close()
		End Try
		Dim fieldName As String = GetSqlName(propertyName)
		If requireUniqueness Then
			command = New OleDbCommand(String.Format("SELECT [id] FROM [{0}] WHERE [id]<>{1} AND [{2}]={3}", tableName, Q(id), fieldName, Q(parsedValue)), connection)
			reader = command.ExecuteReader()
			Try
				If reader.Read() Then
					Throw New Exception(String.Format("The value specified ({0}) would result in a duplicate record. Please enter a unique value.", value))
				End If
			Finally
				reader.Close()
			End Try
		End If
		Dim hasLastUpdatedApplication As Boolean = False
		Dim hasLastUpdatedUser As Boolean = False
		command = New OleDbCommand(String.Format("EXEC sp_columns {0}", tableName), connection)
		reader = command.ExecuteReader()
		Try
			Do While reader.Read()
				If reader("column_name").Equals("last_updated_application") Then
					hasLastUpdatedApplication = True
				ElseIf reader("column_name").Equals("last_updated_user") Then
					hasLastUpdatedUser = True
				End If
			Loop
		Finally
			reader.Close()
		End Try
		command = New OleDbCommand(String.Format("UPDATE [{0}] SET [{1}]={2}" & IIf(hasLastUpdatedApplication, ", [last_updated_application]={3}", "") & IIf(hasLastUpdatedUser, ", [last_updated_user]={4}", "") & " WHERE [id]={5}", tableName, fieldName, Q(parsedValue), Q(APPLICATION_ID), Q(user.Username), Q(id)), connection)
		command.ExecuteNonQuery()
	End Sub
End Class