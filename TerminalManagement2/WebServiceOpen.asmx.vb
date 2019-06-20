Imports System.Web.Services
Imports System.ComponentModel
Imports KahlerAutomation.KaTm2Database
Imports System.Xml.Serialization

' To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line.
' <System.Web.Script.Services.ScriptService()> _
<System.Web.Services.WebService(Namespace:="KahlerAutomation")>
<System.Web.Services.WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)>
<ToolboxItem(False)>
Public Class WebServiceOpen
	Inherits System.Web.Services.WebService

	''' <summary>
	''' 
	''' </summary>
	''' <param name="ipAddress">The IP address of the controller</param>
	''' <param name="controllerNumber">The system number on the controller</param>
	''' <param name="productNumber">The product number to look up the bulk product name of</param>
	''' <returns>A string value of the bulk products assigned to this controller for the specified product number</returns>
	''' <remarks></remarks>
	<WebMethod()>
	Public Function PanelBinBulkProduct(ByVal ipAddress As String, ByVal controllerNumber As Integer, ByVal productNumber As Integer) As String
		Dim bulkProductNames As New List(Of String)
		Dim connection As OleDb.OleDbConnection = Tm2Database.Connection
		Dim panels As ArrayList = KaPanel.GetAll(connection, String.Format("(deleted = 0) AND (ip_address = {0}) AND (connection_type = 0) AND (slave_number = {1})", Q(ipAddress), Q(controllerNumber)), "slave_number, name")
		For Each panel As KaPanel In panels
			If ((panel.Ka2000ApplicationIdentifier = KaController.Controller.ControllerApplicationIdentifier._2886ps OrElse panel.Ka2000ApplicationIdentifier = KaController.Controller.ControllerApplicationIdentifier._2888ps) AndAlso controllerNumber = 3) OrElse ' 1+2 systems
				(panel.Ka2000ApplicationIdentifier = KaController.Controller.ControllerApplicationIdentifier.RANCO OrElse panel.Ka2000ApplicationIdentifier = KaController.Controller.ControllerApplicationIdentifier.SACKETT OrElse panel.Ka2000ApplicationIdentifier = KaController.Controller.ControllerApplicationIdentifier.YARGUS) OrElse ' line blenders
				(panel.Ka2000ApplicationIdentifier = KaController.Controller.ControllerApplicationIdentifier._2866ps) Then '2866 systems
				Dim bulkProductRdr As OleDb.OleDbDataReader = Tm2Database.ExecuteReader(Tm2Database.Connection, String.Format("SELECT DISTINCT bulk_products.name " &
						"FROM bulk_product_panel_settings " &
						"INNER JOIN panels ON bulk_product_panel_settings.panel_id = panels.id " &
						"INNER JOIN bulk_products ON bulk_product_panel_settings.bulk_product_id = bulk_products.id " &
						"WHERE (bulk_product_panel_settings.deleted = 0) AND (panels.deleted = 0) AND (bulk_products.deleted = 0) AND (bulk_product_panel_settings.disabled = 0) AND (panels.ip_address = {0}) AND (panels.connection_type = 0) AND (panels.ka2000_application_identifier = {1}) AND (bulk_product_panel_settings.product_number = {2}) " &
						"ORDER BY bulk_products.name", Q(ipAddress), Q(panel.Ka2000ApplicationIdentifier), Q(productNumber)))
				Do While bulkProductRdr.Read()
					Dim bulkProductName As String = bulkProductRdr("name").ToString().Trim()
					If bulkProductName.Length > 0 Then
						If Not bulkProductNames.Contains(bulkProductName) Then bulkProductNames.Add(bulkProductName)
					End If
				Loop
				bulkProductRdr.Close()
			Else
				Dim bulkProductRdr As OleDb.OleDbDataReader = Tm2Database.ExecuteReader(Tm2Database.Connection, String.Format("SELECT DISTINCT bulk_products.name " &
						"FROM bulk_product_panel_settings " &
						"INNER JOIN bulk_products ON bulk_product_panel_settings.bulk_product_id = bulk_products.id " &
						"WHERE (bulk_product_panel_settings.deleted = 0) AND (bulk_products.deleted = 0) AND (bulk_product_panel_settings.disabled = 0) AND (bulk_product_panel_settings.panel_id = {0}) AND (bulk_product_panel_settings.product_number = {1}) " &
						"ORDER BY bulk_products.name", Q(panel.Id), Q(productNumber)))
				Do While bulkProductRdr.Read()
					Dim bulkProductName As String = bulkProductRdr("name").ToString().Trim()
					If bulkProductName.Length > 0 Then
						If Not bulkProductNames.Contains(bulkProductName) Then bulkProductNames.Add(bulkProductName)
					End If
				Loop
				bulkProductRdr.Close()
			End If
		Next

		bulkProductNames.Sort(StringComparer.OrdinalIgnoreCase)

		Dim bulkProductNamesReturnValue As String = ""
		For Each bulkProductName As String In bulkProductNames
			If bulkProductNamesReturnValue.Length > 0 Then bulkProductNamesReturnValue &= ", "
			bulkProductNamesReturnValue &= bulkProductName.Trim()
		Next
		Return bulkProductNamesReturnValue
	End Function

	''' <summary>
	'''
	''' </summary>
	''' <param name="ipAddress">The IP address of the controller</param>
	''' <param name="controllerNumber">The system number on the controller</param>
	''' <returns>A comma separated list of product numbers used on this controller</returns>
	''' <remarks></remarks>
	<WebMethod()>
	Public Function PanelBulkProductNumbersUsed(ByVal ipAddress As String, ByVal controllerNumber As Integer) As String
		Dim productNumbersUsed As New List(Of Integer)
		Dim connection As OleDb.OleDbConnection = Tm2Database.Connection
		Dim panels As ArrayList = KaPanel.GetAll(connection, String.Format("(deleted = 0) AND (ip_address = {0}) AND (connection_type = 0) AND (slave_number = {1})", Q(ipAddress), Q(controllerNumber)), "slave_number, name")

		For Each panel As KaPanel In panels
			If ((panel.Ka2000ApplicationIdentifier = KaController.Controller.ControllerApplicationIdentifier._2886ps OrElse panel.Ka2000ApplicationIdentifier = KaController.Controller.ControllerApplicationIdentifier._2888ps) AndAlso controllerNumber = 3) OrElse ' 1+2 systems
				(panel.Ka2000ApplicationIdentifier = KaController.Controller.ControllerApplicationIdentifier.RANCO OrElse panel.Ka2000ApplicationIdentifier = KaController.Controller.ControllerApplicationIdentifier.SACKETT OrElse panel.Ka2000ApplicationIdentifier = KaController.Controller.ControllerApplicationIdentifier.YARGUS) OrElse ' line blenders
				(panel.Ka2000ApplicationIdentifier = KaController.Controller.ControllerApplicationIdentifier._2866ps) Then '2866 systems

				Dim productNumberUsedRdr As OleDb.OleDbDataReader = Tm2Database.ExecuteReader(Tm2Database.Connection, String.Format("SELECT DISTINCT bulk_product_panel_settings.product_number " &
				"FROM bulk_product_panel_settings " &
				"INNER JOIN panels ON bulk_product_panel_settings.panel_id = panels.id " &
				"INNER JOIN bulk_products ON bulk_product_panel_settings.bulk_product_id = bulk_products.id " &
				"WHERE (bulk_product_panel_settings.deleted = 0) AND (panels.deleted = 0) AND (bulk_products.deleted = 0) AND (bulk_product_panel_settings.disabled = 0) AND (bulk_product_panel_settings.product_number < 80) AND (panels.ip_address = {0}) AND (panels.connection_type = 0) AND (panels.ka2000_application_identifier = {1}) " &
				"ORDER BY bulk_product_panel_settings.product_number", Q(ipAddress), Q(panel.Ka2000ApplicationIdentifier)))
				Do While productNumberUsedRdr.Read()
					If Not productNumbersUsed.Contains(productNumberUsedRdr("product_number")) Then productNumbersUsed.Add(productNumberUsedRdr("product_number"))
				Loop
				productNumberUsedRdr.Close()
			Else
				Dim productNumberUsedRdr As OleDb.OleDbDataReader = Tm2Database.ExecuteReader(Tm2Database.Connection, String.Format("SELECT DISTINCT bulk_product_panel_settings.product_number " &
				"FROM bulk_product_panel_settings " &
				"INNER JOIN panels ON bulk_product_panel_settings.panel_id = panels.id " &
				"INNER JOIN bulk_products ON bulk_product_panel_settings.bulk_product_id = bulk_products.id " &
				"WHERE (bulk_product_panel_settings.deleted = 0) AND (bulk_product_panel_settings.panel_id = {0}) AND (bulk_products.deleted = 0) AND (bulk_product_panel_settings.disabled = 0) AND (bulk_product_panel_settings.product_number < 80) " &
				"ORDER BY bulk_product_panel_settings.product_number", Q(panel.Id)))
				Do While productNumberUsedRdr.Read()
					If Not productNumbersUsed.Contains(productNumberUsedRdr("product_number")) Then productNumbersUsed.Add(productNumberUsedRdr("product_number"))
				Loop
				productNumberUsedRdr.Close()
			End If
		Next

		productNumbersUsed.Sort()

		Dim productNumberUsedReturnValue As String = ""
		For Each prodNumber As Integer In productNumbersUsed
			If productNumberUsedReturnValue.Length > 0 Then productNumberUsedReturnValue &= ","
			productNumberUsedReturnValue &= prodNumber.ToString().Trim()
		Next
		Return productNumberUsedReturnValue
	End Function

	<WebMethod()> Public Function GetAllAnalysis(username As String, password As String, conditions As String, sortBy As String) As KaAnalysis()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaAnalysis.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaAnalysis))
		End If
	End Function

	<WebMethod()> Public Function GetAllApplicators(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaApplicator()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaApplicator.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaApplicator))
		End If
	End Function

	<WebMethod()> Public Function GetAllBays(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaBay()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaBay.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaBay))
		End If
	End Function

	<WebMethod()> Public Function GetAllBranches(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaBranch()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaBranch.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaBranch))
		End If
	End Function

	<WebMethod()> Public Function GetAllBulkProducts(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaBulkProduct()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaBulkProduct.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaBulkProduct))
		End If
	End Function

	<WebMethod()> Public Function GetAllBulkProductCropTypes(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaBulkProductCropType()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaBulkProductCropType.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaBulkProductCropType))
		End If
	End Function

	<WebMethod()> Public Function GetAllBulkProductInterfaceSettings(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaBulkProductInterfaceSettings()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaBulkProductInterfaceSettings.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaBulkProductInterfaceSettings))
		End If
	End Function

	<WebMethod()> Public Function GetAllBulkProductInventories(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaBulkProductInventory()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaBulkProductInventory.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaBulkProductInventory))
		End If
	End Function

	<WebMethod()> Public Function GetAllBulkProductPanelSettings(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaBulkProductPanelSettings()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaBulkProductPanelSettings.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaBulkProductPanelSettings))
		End If
	End Function

	<WebMethod()> Public Function GetAllCarriers(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaCarrier()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaCarrier.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaCarrier))
		End If
	End Function

	<WebMethod()> Public Function GetAllContainers(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaContainer()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaContainer.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaContainer))
		End If
	End Function

	<WebMethod()> Public Function GetAllContainerEquipments(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaContainerEquipment()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaContainerEquipment.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaContainerEquipment))
		End If
	End Function

	<WebMethod()> Public Function GetAllContainerEquipmentTypes(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaContainerEquipmentType()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaContainerEquipmentType.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaContainerEquipmentType))
		End If
	End Function

	<WebMethod()> Public Function GetAllContainerTypes(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaContainerType()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaContainerType.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaContainerType))
		End If
	End Function

	<WebMethod()> Public Function GetAllCropTypes(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaCropType()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaCropType.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaCropType))
		End If
	End Function

	<WebMethod()> Public Function GetAllCustomerAccounts(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaCustomerAccount()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaCustomerAccount.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaCustomerAccount))
		End If
	End Function

	<WebMethod()> Public Function GetAllCustomerAccountCombinations(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaCustomerAccountCombination()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaCustomerAccountCombination.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaCustomerAccountCombination))
		End If
	End Function

	<WebMethod()> Public Function GetAllCustomerAccountDrivers(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaCustomerAccountDriver()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaCustomerAccountDriver.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaCustomerAccountDriver))
		End If
	End Function

	<WebMethod()> Public Function GetAllCustomerAccountLocations(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaCustomerAccountLocation()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaCustomerAccountLocation.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaCustomerAccountLocation))
		End If
	End Function

	<WebMethod()> Public Function GetAllDischargeLocations(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaDischargeLocation()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaDischargeLocation.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaDischargeLocation))
		End If
	End Function

	<WebMethod()> Public Function GetAllDischargeLocationPanelSettings(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaDischargeLocationPanelSettings()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaDischargeLocationPanelSettings.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaDischargeLocationPanelSettings))
		End If
	End Function

	<WebMethod()> Public Function GetAllDrivers(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaDriver()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaDriver.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaDriver))
		End If
	End Function

	<WebMethod()> Public Function GetAllDriverInFacilities(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaDriverInFacility()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaDriverInFacility.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaDriverInFacility))
		End If
	End Function

	<WebMethod()> Public Function GetAllEventLogs(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaEventLog()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaEventLog.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaEventLog))
		End If
	End Function

	<WebMethod()>
	Public Function GetAllInProgresses(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaInProgress()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaInProgress.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaInProgress))
		End If
	End Function

	<WebMethod()> Public Function GetAllInProgressOrders(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaInProgressOrder()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaInProgressOrder.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaInProgressOrder))
		End If
	End Function

	<WebMethod()> Public Function GetAllInProgressWeighments(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaInProgressWeighment()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaInProgressWeighment.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaInProgressWeighment))
		End If
	End Function

	<WebMethod()> Public Function GetAllInterfaces(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaInterface()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaInterface.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaInterface))
		End If
	End Function

	<WebMethod()> Public Function GetAllInventoryChanges(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaInventoryChange()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaInventoryChange.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaInventoryChange))
		End If
	End Function

	<WebMethod()> Public Function GetAllLocations(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaLocation()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaLocation.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaLocation))
		End If
	End Function

	<WebMethod()> Public Function GetAllOrders(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaOrder()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Dim l As ArrayList = KaOrder.GetAll(GetUserConnection(userId), conditions, sortBy)
			For Each order As KaOrder In l
				order.OrderItems.Clear()
				order.OrderAccounts.Clear()
			Next
			Return l.ToArray(GetType(KaOrder))
		End If
	End Function

	<WebMethod()> Public Function GetAllOrderItems(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaOrderItem()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaOrderItem.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaOrderItem))
		End If
	End Function

	<WebMethod()> Public Function GetAllOrderCustomerAccounts(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaOrderCustomerAccount()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaOrderCustomerAccount.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaOrderCustomerAccount))
		End If
	End Function

	<WebMethod()> Public Function GetAllOwners(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaOwner()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaOwner.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaOwner))
		End If
	End Function

	<WebMethod()> Public Function GetAllPanels(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaPanel()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaPanel.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaPanel))
		End If
	End Function

	<WebMethod()> Public Function GetAllProducts(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaProduct()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Dim list As ArrayList = KaProduct.GetAll(GetUserConnection(userId), conditions, sortBy)
			For i As Integer = 0 To list.Count - 1
				CType(list(i), KaProduct).ProductBulkItems.Clear()
				CType(list(i), KaProduct).ProductInterfaces.Clear()
			Next
			Return list.ToArray(GetType(KaProduct))
		End If
	End Function

	<WebMethod()> Public Function GetAllProductBulkProducts(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaProductBulkProduct()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaProductBulkProduct.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaProductBulkProduct))
		End If
	End Function

	<WebMethod()> Public Function GetAllProductInterfaceSettings(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaProductInterfaceSettings()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaProductInterfaceSettings.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaProductInterfaceSettings))
		End If
	End Function

	<WebMethod()> Public Function GetAllReceivingPurchaseOrders(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaReceivingPurchaseOrder()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaReceivingPurchaseOrder.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaReceivingPurchaseOrder))
		End If
	End Function

	<WebMethod()> Public Function GetAllReceivingTickets(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaReceivingTicket()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaReceivingTicket.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaReceivingTicket))
		End If
	End Function

	<WebMethod()> Public Function GetAllSeals(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaSeal()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaSeal.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaSeal))
		End If
	End Function

	<WebMethod()> Public Function GetAllStagedOrders(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaStagedOrder()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaStagedOrder.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaStagedOrder))
		End If
	End Function

	<WebMethod()> Public Function GetAllStagedOrderCompartments(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaStagedOrderCompartment()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaStagedOrderCompartment.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaStagedOrderCompartment))
		End If
	End Function

	<WebMethod()> Public Function GetAllStagedOrderTransports(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaStagedOrderTransport()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaStagedOrderTransport.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaStagedOrderTransport))
		End If
	End Function

	<WebMethod()> Public Function GetAllSupplierAccounts(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaSupplierAccount()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaSupplierAccount.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaSupplierAccount))
		End If
	End Function

	<WebMethod()> Public Function GetAllTanks(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaTank()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaTank.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaTank))
		End If
	End Function

	<WebMethod()> Public Function GetAllTankAlarmHistories(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaTankAlarmHistory()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaTankAlarmHistory.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaTankAlarmHistory))
		End If
	End Function

	<WebMethod()> Public Function GetAllTankGroups(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaTankGroup()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaTankGroup.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaTankGroup))
		End If
	End Function

	<WebMethod()> Public Function GetAllTankGroupTanks(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaTankGroupTank()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaTankGroupTank.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaTankGroupTank))
		End If
	End Function

	<WebMethod()> Public Function GetAllTankLevelTrends(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaTankLevelTrend()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaTankLevelTrend.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaTankLevelTrend))
		End If
	End Function

	<WebMethod()> Public Function GetAllTankLevelTrendData(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaTankLevelTrendData()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaTankLevelTrendData.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaTankLevelTrendData))
		End If
	End Function

	'''' <summary>
	'''' Sets the current status of a tank with the associated values
	'''' </summary>
	'''' <param name="ipAddress">address of the panel reporting</param>
	'''' <param name="sensor">The number of the sensor used in determining which tank(s) to update</param>
	'''' <param name="si">0 = Imperial, 1 = SI (metric)</param>
	'''' <param name="status">The status word for alarms/acks/alarms enabled</param>
	'''' <param name="height">0.1 inch or centimeter</param>
	'''' <param name="volume">current calculated quantity of the product in the tank. Whole Gallons or Liters</param>
	'''' <param name="rate">the amount of change in level that has occurred in the last hour (in/hr or cm/hr)</param>
	'''' <param name="maxHeight">0.1 inch or centimeter</param>
	'''' <param name="maxVolume">max capacity of the tank. Whole Gallons or Liters</param>
	'''' <param name="name">The default name of the tank</param>
	'''' <param name="highHighLimit">0.1 inch or centimeter</param>
	'''' <param name="highLimit">0.1 inch or centimeter</param>
	'''' <param name="lowLimit">0.1 inch or centimeter</param>
	'''' <param name="lowLowLimit">0.1 inch or centimeter</param>
	'''' <param name="rateLimit">in/hr or cm/hr</param>
	'''' <param name="density">0.001 lb/gal or kg/m3</param>
	'<WebMethod()>
	'Public Sub UpdateTank(username As String, password As String, ipAddress As String, sensor As UInt32, si As UInt32, status As UInt16, height As UInt16, volume As UInt32, rate As Short, maxHeight As UInt16, maxVolume As UInt32, name As String, highHighLimit As UInt16, highLimit As UInt16, lowLimit As UInt16, lowLowLimit As UInt16, rateLimit As UInt16, density As UInt16)
	'	UpdateTankWithTemperature(username, password, ipAddress, 0, sensor, si, status, height, volume, rate, maxHeight, maxVolume, name, highHighLimit, highLimit, lowLimit, lowLowLimit, rateLimit, density, 0)
	'End Sub

	''' <summary>
	''' Sets the current status of a tank with the associated values
	''' </summary>
	''' <param name="ipAddress">address of the panel reporting</param>
	''' <param name="systemAddress">The base register address of the panel reporting. this is used in determining the panel to apply the tank reading to.</param>
	''' <param name="sensor">The number of the sensor used in determining which tank(s) to update</param>
	''' <param name="si">0 = Imperial, 1 = SI (metric)</param>
	''' <param name="status">The status word for alarms/acks/alarms enabled</param>
	''' <param name="height">0.1 inch or centimeter</param>
	''' <param name="volume">current calculated quantity of the product in the tank. Whole Gallons or Liters</param>
	''' <param name="rate">the amount of change in level that has occurred in the last hour (in/hr or cm/hr)</param>
	''' <param name="maxHeight">0.1 inch or centimeter</param>
	''' <param name="maxVolume">max capacity of the tank. Whole Gallons or Liters</param>
	''' <param name="name">The default name of the tank</param>
	''' <param name="highHighLimit">0.1 inch or centimeter</param>
	''' <param name="highLimit">0.1 inch or centimeter</param>
	''' <param name="lowLimit">0.1 inch or centimeter</param>
	''' <param name="lowLowLimit">0.1 inch or centimeter</param>
	''' <param name="rateLimit">in/hr or cm/hr</param>
	''' <param name="density">0.001 lb/gal or kg/m3</param>
	''' <param name="temperature">0.1° Fahrenheit or Centigrade</param>
	<WebMethod()>
	Public Sub UpdateTank(username As String, password As String, ipAddress As String, sensor As UInt32, si As UInt32, status As UInt16, height As UInt16, volume As UInt32, rate As Short, maxHeight As UInt16, maxVolume As UInt32, name As String, highHighLimit As UInt16, highLimit As UInt16, lowLimit As UInt16, lowLowLimit As UInt16, rateLimit As UInt16, density As UInt16, systemAddress As UInt16, temperature As UInt16)
		Dim userId As Guid = GetUserId(username, password)
		Dim currentUser As KaUser
		Try
			currentUser = New KaUser(Tm2Database.Connection, userId)
		Catch ex As RecordNotFoundException
			Throw New UnauthorizedAccessException("Username Or password are Not correct.")
		End Try
		Dim _currentUserPermission As Dictionary(Of String, KaTablePermission) = Utilities.GetUserPagePermission(currentUser, New List(Of String)({KaTank.TABLE_NAME}), "Tanks")
		If Not _currentUserPermission(KaTank.TABLE_NAME).Edit Then Throw New UnauthorizedAccessException("User does not have modify access for tanks.")

		Dim panel As KaPanel
		Dim tlmPanels As ArrayList = KaPanel.GetAll(Tm2Database.Connection, $"{KaPanel.FN_DELETED} = 0 AND {KaPanel.FN_ROLE} = {Q(KaPanel.PanelRole.TLM5)} AND {KaPanel.FN_IP_ADDRESS} = {Q(ipAddress)} AND {KaPanel.FN_SYSTEM_ADDRESS} = {Q(systemAddress)} AND ({KaPanel.FN_CONNECTION_TYPE} = {Q(KaPanel.PanelConnectionType.Ethernet)} OR {KaPanel.FN_CONNECTION_TYPE} = {Q(KaPanel.PanelConnectionType.ModbusTcp)})", KaPanel.FN_NAME)
		If tlmPanels.Count > 0 Then
			panel = tlmPanels(0)
		Else
			Throw New RecordNotFoundException
		End If
		Dim tanks As ArrayList = KaTank.GetAll(Tm2Database.Connection, "deleted=0 And panel_id=" & Q(panel.Id), "")

		Dim updateTanks As New List(Of KaTank)
		For Each tank As KaTank In tanks
			If tank.Sensor = sensor - 1 Then updateTanks.Add(tank)
		Next
		If (status And 8192) = 0 AndAlso updateTanks.Count = 0 Then ' KA-2000 says this tank is enabled, but database doesn't have a tank for this sensor
			' create a new tank
			If name.Trim.Length = 0 Then
				name = $"{panel.Name} - Tank {sensor}"
			End If
			Dim tank As New KaTank() With {
				.Sensor = sensor - 1,
				.PanelId = panel.Id,
				.LocationId = panel.LocationId,
				.Name = name
			}

			updateTanks.Add(tank)
		End If
		For Each tank As KaTank In updateTanks : tank.UpdateTank(Tm2Database.Connection, Nothing, status, CType(height, Double) / 10, volume, si, rate, CType(maxHeight, Double) / 10, maxVolume, CType(highHighLimit, Double) / 10, CType(highLimit, Double) / 10, CType(lowLimit, Double) / 10, CType(lowLowLimit, Double) / 10, rateLimit, CType(temperature, Double) / 10, "TLM Web Service", username) : Next ' update the tanks that use this sensor
	End Sub

	<WebMethod()> Public Function GetAllTickets(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaTicket()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaTicket.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaTicket))
		End If
	End Function

	<WebMethod()> Public Function GetAllTicketBulkItems(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaTicketBulkItem()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaTicketBulkItem.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaTicketBulkItem))
		End If
	End Function

	<WebMethod()> Public Function GetAllTicketBulkItemAnlysis(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaTicketBulkItemAnalysis()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaTicketBulkItemAnalysis.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaTicketBulkItemAnalysis))
		End If
	End Function

	<WebMethod()> Public Function GetAllTicketBulkItemCropTypes(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaTicketBulkItemCropTypes()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaTicketBulkItemCropTypes.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaTicketBulkItemCropTypes))
		End If
	End Function

	<WebMethod()> Public Function GetAllTicketCustomerAccounts(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaTicketCustomerAccount()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaTicketCustomerAccount.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaTicketCustomerAccount))
		End If
	End Function

	<WebMethod()> Public Function GetAllTicketItems(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaTicketItem()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaTicketItem.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaTicketItem))
		End If
	End Function

	<WebMethod()> Public Function GetAllTicketTransports(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaTicketTransport()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaTicketTransport.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaTicketTransport))
		End If
	End Function

	<WebMethod()> Public Function GetAllTracks(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaTrack()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaTrack.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaTrack))
		End If
	End Function

	<WebMethod()> Public Function GetAllTransports(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaTransport()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaTransport.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaTransport))
		End If
	End Function

	<WebMethod()> Public Function GetAllTransportCompartments(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaTransportCompartment()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaTransportCompartment.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaTransportCompartment))
		End If
	End Function

	<WebMethod()> Public Function GetAllTransportInFacilities(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaTransportInFacility()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaTransportInFacility.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaTransportInFacility))
		End If
	End Function

	<WebMethod()> Public Function GetAllTransportInspectionData(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaTransportInspectionData()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaTransportInspectionData.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaTransportInspectionData))
		End If
	End Function

	<WebMethod()> Public Function GetAllTransportInspectionFields(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaTransportInspectionFields()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaTransportInspectionFields.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaTransportInspectionFields))
		End If
	End Function

	<WebMethod()> Public Function GetAllTransportsOnTracks(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaTransportsOnTracks()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaTransportsOnTracks.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaTransportsOnTracks))
		End If
	End Function

	<WebMethod()> Public Function GetAllTransportTypes(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaTransportTypes()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaTransportTypes.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaTransportTypes))
		End If
	End Function

	<WebMethod()> Public Function GetAllUnits(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaUnit()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaUnit.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaUnit))
		End If
	End Function

	<WebMethod()>
	<XmlInclude(GetType(KaUserPermission))>
	<XmlInclude(GetType(System.Drawing.Bitmap))>
	Public Function GetAllUsers(ByVal username As String, ByVal password As String, conditions As String, sortBy As String) As KaUser()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Return KaUser.GetAll(GetUserConnection(userId), conditions, sortBy).ToArray(GetType(KaUser))
		End If
	End Function

	<WebMethod()> Public Function GetAllOrdersForContainer(ByVal username As String, ByVal password As String, containerId As Guid) As KaOrder()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Dim connection As OleDb.OleDbConnection = GetUserConnection(userId)
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
		End If
	End Function

	<WebMethod()> Public Function GetContainerWithNumber(ByVal username As String, ByVal password As String, number As String) As KaContainer()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Dim connection As OleDb.OleDbConnection = GetUserConnection(userId)
			Dim list As ArrayList = KaContainer.GetAll(connection, String.Format("[deleted]=0 AND [number]={0}", Q(number)), "[number] ASC")
			If list.Count = 1 Then
				Return list.ToArray(GetType(KaContainer))
			Else
				list = KaContainer.GetAll(connection, String.Format("[deleted]=0 AND [number] LIKE {0}", Q(String.Format("%{0}%", number))), "[number] ASC")
				Return list.ToArray(GetType(KaContainer))
			End If
		End If
	End Function

	<WebMethod()> Public Function CreateNewContainer(ByVal username As String, ByVal password As String, number As String) As Guid
		Dim user As KaUser = GetUser(username, password)
		If user Is Nothing OrElse user.Id.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Dim connection As OleDb.OleDbConnection = GetUserConnection(user.Id)
			If KaContainer.GetAll(connection, String.Format("[deleted]=0 AND [number]={0}", Q(number)), "").Count > 0 Then
				Throw New TooManyRecordsException(String.Format("A container with number {0} already exists.", number))
			Else
				Dim container As New KaContainer() With {
				.VolumeUnitId = KaUnit.GetSystemDefaultVolumeUnitOfMeasure(connection, Nothing),
				.WeightUnitId = KaUnit.GetSystemDefaultMassUnitOfMeasure(connection, Nothing),
				.Number = number
			}
				container.SqlInsert(connection, Nothing, Database.ApplicationIdentifier, user.Username)
				Return container.Id
			End If
		End If
	End Function

	<WebMethod()> Public Sub SellContainer(ByVal username As String, ByVal password As String, containerId As Guid, orderId As Guid)
		Dim user As KaUser = GetUser(username, password)
		If user Is Nothing OrElse user.Id.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Dim connection As OleDb.OleDbConnection = GetUserConnection(user.Id)
			Dim container As New KaContainer(connection, containerId)
			Dim packagedInventoryLocationId As Guid = Utilities.GetContainerPackagedInventoryLocationId(connection)
			container.Sell(connection, Nothing, orderId, packagedInventoryLocationId, APPLICATION_ID, user.Username)
		End If
	End Sub

	<WebMethod()> Public Sub ReturnContainer(ByVal username As String, ByVal password As String, containerId As Guid, grossWeight As Double, unitId As Guid, facilityLocationId As Guid)
		Dim user As KaUser = GetUser(username, password)
		If user Is Nothing OrElse user.Id.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Dim connection As OleDb.OleDbConnection = GetUserConnection(user.Id)
			Dim container As New KaContainer(connection, containerId)
			Dim packagedInventoryLocationId As Guid = Utilities.GetContainerPackagedInventoryLocationId(connection)
			container.CheckIn(connection, Nothing, New KaQuantity(grossWeight, unitId), facilityLocationId, packagedInventoryLocationId, APPLICATION_ID, user.Username)
		End If
	End Sub

	<WebMethod()> Public Sub ChangeContainerBulkProduct(ByVal username As String, ByVal password As String, containerId As Guid, bulkProductId As Guid, ownerId As Guid)
		Dim user As KaUser = GetUser(username, password)
		If user Is Nothing OrElse user.Id.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Dim connection As OleDb.OleDbConnection = GetUserConnection(user.Id)
			Dim container As New KaContainer(connection, containerId)
			Dim previous As KaContainer = container.Clone()
			container.BulkProductId = bulkProductId
			Dim packagedInventoryLocationId As Guid = Utilities.GetContainerPackagedInventoryLocationId(connection)
			container.HandleBulkProductChange(connection, Nothing, previous, container.LocationId, packagedInventoryLocationId, ownerId, APPLICATION_ID, user.Username)
			container.SqlUpdate(connection, Nothing, APPLICATION_ID, user.Username)
		End If
	End Sub

	<WebMethod()> Public Sub ChangeContainerProductWeight(ByVal username As String, ByVal password As String, containerId As Guid, productWeight As Double, ownerId As Guid)
		Dim user As KaUser = GetUser(username, password)
		If user Is Nothing OrElse user.Id.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Dim connection As OleDb.OleDbConnection = GetUserConnection(user.Id)
			Dim container As New KaContainer(connection, containerId)
			Dim previous As KaContainer = container.Clone()
			container.ProductWeight = productWeight
			Dim packagedInventoryLocationId As Guid = Utilities.GetContainerPackagedInventoryLocationId(connection)
			container.HandleBulkProductChange(connection, Nothing, previous, container.LocationId, packagedInventoryLocationId, ownerId, APPLICATION_ID, user.Username)
			container.SqlUpdate(connection, Nothing, APPLICATION_ID, user.Username)
		End If
	End Sub

	<WebMethod()> Public Function GetContainerEquipmentWithNumber(ByVal username As String, ByVal password As String, number As String) As KaContainerEquipment()
		Dim userId As Guid = GetUserId(username, password)
		If userId.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Dim connection As OleDb.OleDbConnection = GetUserConnection(userId)
			Dim list As ArrayList = KaContainerEquipment.GetAll(connection, String.Format("[deleted]=0 AND [barcode_id]={0}", Q(number)), "[name] ASC")
			If list.Count = 1 Then
				Return list.ToArray(GetType(KaContainerEquipment))
			Else
				list = KaContainerEquipment.GetAll(connection, String.Format("[deleted]=0 AND [barcode_id] LIKE {0}", Q(String.Format("%{0}%", number))), "[name] ASC")
				Return list.ToArray(GetType(KaContainerEquipment))
			End If
		End If
	End Function

	<WebMethod()> Public Function CreateNewContainerEquipment(ByVal username As String, ByVal password As String, number As String) As Guid
		Dim user As KaUser = GetUser(username, password)
		If user Is Nothing OrElse user.Id.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Dim connection As OleDb.OleDbConnection = GetUserConnection(user.Id)
			If KaContainerEquipment.GetAll(connection, String.Format("[deleted]=0 AND [barcode_id]={0}", Q(number)), "").Count > 0 Then
				Throw New TooManyRecordsException(String.Format("Container equipment with number {0} already exists.", number))
			Else
				Dim equipment As New KaContainerEquipment() With {.Name = number, .BarcodeId = number}
				equipment.SqlInsert(connection, Nothing, Database.ApplicationIdentifier, user.Username)
				Return equipment.Id
			End If
		End If
	End Function

	Public Shared Function GetUser(ByVal username As String, ByVal password As String) As KaUser
		Dim userList As ArrayList = KaUser.GetAll(Tm2Database.Connection, "deleted=0 AND disabled = 0 AND username=" & Q(username), "")
		For Each u As KaUser In userList
			If u.Password = password Then
				Return u
			End If
		Next
		Return Nothing
	End Function

	Public Shared Function GetUserId(ByVal username As String, ByVal password As String) As Guid
		Dim user As KaUser = GetUser(username, password)
		If user IsNot Nothing Then
			Return user.Id
		Else
			Return Guid.Empty
		End If
	End Function

	<WebMethod()> Public Sub UpdateRecord(ByVal username As String, ByVal password As String, recordType As String, id As Guid, propertyName As String, value As String)
		Dim user As KaUser = GetUser(username, password)
		If user Is Nothing OrElse user.Id.Equals(Guid.Empty) Then
			Throw New UnauthorizedAccessException("Username or password are not correct.")
		Else
			Dim assembly As System.Reflection.Assembly = System.Reflection.Assembly.GetAssembly(GetType(KaUser))
			Dim type As Type = assembly.GetType(String.Format("KahlerAutomation.KaTm2Database.{0}", recordType))
			Dim prop As System.Reflection.PropertyInfo = type.GetProperty(propertyName)
			Dim parsedValue As Object
			If prop.PropertyType.IsEnum Then
				parsedValue = [Enum].Parse(prop.PropertyType, value)
			Else
				Dim method As System.Reflection.MethodInfo = prop.PropertyType.GetMethod("Parse", New Type() {GetType(String)})
				If method IsNot Nothing Then
					parsedValue = method.Invoke(Nothing, New Object() {value})
				Else
					parsedValue = value
				End If
			End If
			Dim connection As OleDb.OleDbConnection = GetUserConnection(user.Id)
			Dim requireUniqueness As Boolean = (recordType.Equals("KaContainer") AndAlso propertyName.Equals("Number")) OrElse (recordType.Equals("KaContainerEquipment") AndAlso propertyName.Equals("BarcodeId"))
			Dim reader As OleDb.OleDbDataReader
			Dim tableName As String = GetSqlName(recordType.Substring(2, recordType.Length - 2))
			reader = Tm2Database.ExecuteReader(connection, "EXEC sp_tables")
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
				reader = Tm2Database.ExecuteReader(connection, String.Format("SELECT [id] FROM [{0}] WHERE [id]<>{1} AND [{2}]={3}", tableName, Q(id), fieldName, Q(parsedValue)))
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
			reader = Tm2Database.ExecuteReader(connection, String.Format("EXEC sp_columns {0}", tableName))
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
			Tm2Database.ExecuteNonQuery(connection, String.Format("UPDATE [{0}] SET [{1}]={2}" & IIf(hasLastUpdatedApplication, ", [last_updated_application]={3}", "") & IIf(hasLastUpdatedUser, ", [last_updated_user]={4}", "") & " WHERE [id]={5}", tableName, fieldName, Q(parsedValue), Q(APPLICATION_ID), Q(user.Username), Q(id)))
		End If
	End Sub

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
End Class