Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class Orders
	Inherits System.Web.UI.Page
	Private Const MAX_PRODUCT_COUNT As Integer = 32
	Private Const PNL_PRODUCT As String = "pnlProduct{0:0}"
	Private Const LBL_ORDER_ITEM_ID As String = "lblOrderItemId{0:0}"
	Private Const DDL_PRODUCT As String = "ddlProduct{0:0}"
	Private Const TBX_PRODUCT_AMOUNT As String = "tbxProductAmount{0:0}"
	Private Const DDL_UNITS As String = "ddlUnits{0:0}"
	Private Const LBL_DELIV As String = "lblDeliv{0:0}"
	Private Const BTN_MOVE_PRODUCT_UP As String = "btnMoveProductUp{0:0}"
	Private Const BTN_MOVE_PRODUCT_DOWN As String = "btnMoveProductDown{0:0}"
	Private Const TBX_PRODUCT_NOTES As String = "tbxProductNotes{0:0}"
	Private Const BTN_REMOVE_PRODUCT As String = "btnRemoveProduct{0:0}"
	Private Const DDL_ORDER_ITEM_GROUPING As String = "ddlOrderItemGrouping{0:0}"

	Private Const MAX_ACCOUNT_COUNT As Integer = 12
	Private Const DDL_ACCOUNT As String = "ddlAccount{0:0}"
	Private Const PNL_ACCOUNT As String = "pnlAccount{0:0}"
	Private Const TBX_ACCOUNT_PERCENT As String = "tbxPercent{0:0}"
	Private Const BTN_REMOVE_ACCOUNT As String = "btnRemoveAccount{0:0}"

	Private Const AUTOMATIC_ORDER_NUMBER_PLACEHOLDER As String = "Automatically generated"
	Private _customFields As New List(Of KaCustomField)
	Private _customFieldData As New List(Of KaCustomFieldData)
	'Private _userAutorizedChangeOrder As Boolean = False
	Private _interfaceAllowsProductChange As Boolean = True
	Private _interfaceAllowsCustomerChange As Boolean = True
	Private _interfaceAllowsOrderItemGroupingChange As Boolean = True
	Private _orderItemGroupings As New List(Of KaOrderItemGroup)
	Private _orderItemGroupingTable As New DataTable

#Region " Web Form Designer Generated Code "

	'This call is required by the Web Form Designer.
	<System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()

	End Sub

	Protected WithEvents ddlAccounts As System.Web.UI.WebControls.DropDownList

	Private Sub Page_Init(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Init
		'CODEGEN: This method call is required by the Web Form Designer
		'Do not modify it using the code editor.
		InitializeComponent()
	End Sub

#End Region
	Private _currentUser As KaUser
	Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
	Private _currentTableName As String = KaOrder.TABLE_NAME
	Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
		Response.Cache.SetCacheability(HttpCacheability.NoCache)
		_currentUser = Utilities.GetUser(Me)
		_currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Orders")
		If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")
		lblStatus.Text = ""

		If Not Page.IsPostBack Then
			Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
			SetTextboxMaxLengths()
			PopulateFacilityList()
			PopulateOwnersList()
			If Page.Request("OrderId") Is Nothing Then
				Try
					ddlFacilityFilter.SelectedValue = KaSetting.GetSetting(connection, "OrdersPage:" & _currentUser.Id.ToString & "/LastFacilityUsed", Guid.Empty.ToString())
					ddlFacilityFilter_SelectedIndexChanged(Nothing, Nothing)
				Catch ex As ArgumentOutOfRangeException

				End Try
			Else
				ddlFacilityFilter.SelectedIndex = 0
			End If
			_customFields.Clear()
			For Each customField As KaCustomField In KaCustomField.GetAll(connection, String.Format("deleted=0 AND {0} = {1}", KaCustomField.FN_TABLE_NAME, Q(KaOrder.TABLE_NAME)), KaCustomField.FN_FIELD_NAME)
				_customFields.Add(customField)
			Next

			PopulateOrdersList()
			PopulateBranchesList()
			PopulateApplicatorsList()
			PopulateTransportTypesList()
			PopulateProductCombos()
			PopulateAccountCombos()
			Session.Add("tempShipToName", "")
			Session.Add("tempShipToStreet", "")
			Session.Add("tempShipToCity", "")
			Session.Add("tempShipToState", "")
			Session.Add("tempShipToZip", "")
			LockControls()
			pnlInterface.Visible = KaInterface.GetAll(connection, "deleted=0", "").Count > 0
			SetControlUsabilityFromPermissions()
			Dim orderId As Guid = Guid.Empty
			Guid.TryParse(Page.Request("OrderId"), orderId)
			Try
				ddlOrders.SelectedValue = orderId.ToString()
			Catch ex As ArgumentOutOfRangeException

			End Try
			ddlOrders_SelectedIndexChanged(ddlOrders, New EventArgs())
			Utilities.ConfirmBox(Me.btnDelete, "Are you sure you want to delete this order?") ' setup for delete confirmation box
		ElseIf Page.IsPostBack And Request("__EVENTARGUMENT").StartsWith("SaveOrder") Then
			Dim saveAndNewClicked As Boolean = False
			Boolean.TryParse(Request("__EVENTARGUMENT").Split(",")(0).Replace("SaveOrder('", "").Replace("'", ""), saveAndNewClicked)
			Dim ignoreLastUpdated As Boolean = True
			Boolean.TryParse(Request("__EVENTARGUMENT").Split(",")(1).Replace("'", "").Replace(")", ""), ignoreLastUpdated)
			SaveOrder(saveAndNewClicked, ignoreLastUpdated)
		End If
	End Sub

	Private Sub LockControls()
		If KaSetting.GetSetting(GetUserConnection(_currentUser.Id), "General/LockOwnerDDL", "False") Then
			ddlOwners.Enabled = False
		End If
		If KaSetting.GetSetting(GetUserConnection(_currentUser.Id), "General/LockBranchesDDL", "False") Then
			ddlBranches.Enabled = False
		End If
		If KaSetting.GetSetting(GetUserConnection(_currentUser.Id), "General/LockRunOverPercent", "False") Then
			tbxOverun.Enabled = False
		End If
		If KaSetting.GetSetting(GetUserConnection(_currentUser.Id), "General/LockPrimaryUnits", "False") Then
			For i = 1 To MAX_PRODUCT_COUNT
				Dim ddlUnits As System.Web.UI.WebControls.DropDownList = FindControl(String.Format(DDL_UNITS, i))
				ddlUnits.Enabled = False
			Next
		End If
	End Sub

	Private Sub SelectDefaults(ByRef order As KaOrder)
		Dim c As OleDbConnection = GetUserConnection(_currentUser.Id)
		If ddlOwners.Items.Count = 2 Then ' there's only one owner available (2 because "Select an owner" is an option)
			order.OwnerId = Guid.Parse(ddlOwners.Items(1).Value) ' select the only owner by default
		Else ' force the user to select an owner
			Try ' to select the default owner ID...
				Dim id As Guid = Guid.Parse(KaSetting.GetSetting(c, "General/Owner", Guid.Empty.ToString())) ' parsing ensures the ID format is correct
				order.OwnerId = id
			Catch ex As Exception ' either the default ID wasn't formatted correctly or the owner isn't in the list...
				order.OwnerId = Guid.Empty  ' force the user to select an owner
			End Try
		End If
		Dim defaultBranch As String = KaSetting.GetSetting(c, "General/Branch", Guid.Empty.ToString)
		If defaultBranch <> "" Then
			Try
				Dim defaultBranchId As Guid = Guid.Parse(defaultBranch)
				If defaultBranchId <> Guid.Empty Then
					Try
						order.BranchId = defaultBranchId
					Catch ex As Exception
						'Not in list.  Don't add because user doesn't have access to that branch?
					End Try
				End If
			Catch ex As Exception
				'Suppress
			End Try
		End If

		If AutoGenerateOrderNumber Then
			order.Number = AUTOMATIC_ORDER_NUMBER_PLACEHOLDER
			tbxOrderNum.Enabled = UserCanChangeOrderNumber
		Else
			order.Number = ""
			tbxOrderNum.Enabled = True
		End If

		order.OverScalePercent = KaSetting.GetSetting(c, "General/RunOverPercent", "0")
		order.DeliveredBatches = 0
		order.RequestedBatches = 1
	End Sub

	Private Sub PopulateOrdersList()
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim showReleaseNumber As Boolean = Boolean.Parse(KaSetting.GetSetting(connection, "General/ShowReleaseNumberInOrderList", "False"))
		ddlOrders.Items.Clear()
		If _currentUserPermission(_currentTableName).Create Then
			ddlOrders.Items.Add(New ListItem("Enter a new order", Guid.Empty.ToString()))
		Else
			ddlOrders.Items.Add(New ListItem("Select an order", Guid.Empty.ToString()))
		End If
		Dim sqlStatement As String = "SELECT id, number, release_number FROM orders " &
				"WHERE completed = 0 AND deleted = 0 AND archived = 0 "
		If Not _currentUser.OwnerId.Equals(Guid.Empty) Then sqlStatement &= "AND ((owner_id = " & Q(_currentUser.OwnerId) & ") OR (owner_id = " & Q(Guid.Empty) & ")) "
		If ddlFacilityFilter.SelectedValue <> Guid.Empty.ToString() Then sqlStatement &= "AND (NOT (id IN (SELECT order_id FROM order_items WHERE (deleted = 0) AND (NOT (product_id IN (SELECT product_id FROM product_bulk_products WHERE (deleted = 0) AND (location_id = " & Q(ddlFacilityFilter.SelectedValue) & "))))))) "
		sqlStatement &= " ORDER BY number ASC"

		Dim getOrdersRdr As OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), sqlStatement)

		Do While getOrdersRdr.Read
			Dim displayText As String = getOrdersRdr.Item("number")
			If getOrdersRdr.Item("release_number") <> "" Then
				displayText &= " (" & getOrdersRdr.Item("release_number") & ")"
			End If
			ddlOrders.Items.Add(New ListItem(displayText, getOrdersRdr.Item("id").ToString()))
		Loop
		getOrdersRdr.Close()
		DisplayJavaScriptMessage("ResetScrollPosition;", "resetDotNetScrollPosition();", True)
	End Sub

	Private Sub PopulateFacilityList()
		ddlFacilityFilter.Items.Clear()
		ddlFacilityFilter.Items.Add(New ListItem("All facilities", Guid.Empty.ToString()))
		For Each u As KaLocation In KaLocation.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
			ddlFacilityFilter.Items.Add(New ListItem(u.Name, u.Id.ToString))
		Next
	End Sub

	Private Sub PopulateOwnersList()
		ddlOwners.Items.Clear()
		ddlOwners.Items.Add(New ListItem("Select an owner", Guid.Empty.ToString))
		For Each u As KaOwner In KaOwner.GetAll(GetUserConnection(_currentUser.Id), "deleted=0" & IIf(_currentUser.OwnerId <> Guid.Empty, " AND id=" & Q(_currentUser.OwnerId), ""), "name ASC")
			ddlOwners.Items.Add(New ListItem(u.Name, u.Id.ToString))
		Next
	End Sub

	Private Sub PopulateBranchesList()
		ddlBranches.Items.Clear()
		ddlBranches.Items.Add(New ListItem("Select a branch", Guid.Empty.ToString))
		For Each u As KaBranch In KaBranch.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
			ddlBranches.Items.Add(New ListItem(u.Name, u.Id.ToString))
		Next
	End Sub

	Private Sub PopulateApplicatorsList()
		ddlApplicator.Items.Clear()
		ddlApplicator.Items.Add(New ListItem("", Guid.Empty.ToString))
		For Each u As KaApplicator In KaApplicator.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
			ddlApplicator.Items.Add(New ListItem(u.Name, u.Id.ToString))
		Next
	End Sub

	Private Sub PopulateTransportTypesList()
		ddlTransportType.Items.Clear()
		ddlTransportType.Items.Add(New ListItem("", Guid.Empty.ToString))
		For Each u As KaTransportTypes In KaTransportTypes.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
			ddlTransportType.Items.Add(New ListItem(u.Name, u.Id.ToString))
		Next
	End Sub

	Protected Sub ddlOrders_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles ddlOrders.SelectedIndexChanged
		lblOrderDetailStatus.Text = ""
		pnlOrderDetailStatus.Visible = False
		btnSetLockedStatus.Visible = False
		btnClearLockedStatus.Visible = False
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim order As KaOrder
		Try
			order = New KaOrder(connection, Guid.Parse(ddlOrders.SelectedValue))
		Catch ex As Exception
			order = New KaOrder
			SelectDefaults(order) 'Order object is ByRef here.  This will set the defaults on the object and the controls will get populated below.
		End Try

		tbxLastUpdated.Value = order.LastUpdated
		If order.Id <> Guid.Empty AndAlso order.IsOrderStaged(connection, Nothing) Then AddOrderStatusDetails("Order assigned to staged order")
		If order.Id <> Guid.Empty AndAlso order.OrderReferencedInOtherProgressRecords(connection, Nothing, Guid.Empty) Then AddOrderStatusDetails("Order in progress")
		If order.Locked Then
			AddOrderStatusDetails("Order locked")
			btnClearLockedStatus.Visible = True
		Else
			btnSetLockedStatus.Visible = True
		End If
		cbxDoNotBlend.Checked = order.DoNotBlend
		tbxOrderNum.Text = order.Number
		tbxReleaseNumber.Text = order.ReleaseNumber
		tbxPurchaseOrderNum.Text = order.PurchaseOrder
		tbxOverun.Text = order.OverScalePercent
		ddlOwners.SelectedValue = order.OwnerId.ToString

		_interfaceAllowsProductChange = True
		_interfaceAllowsCustomerChange = True
		_interfaceAllowsOrderItemGroupingChange = True
		Try
			Dim interfaceInfo As New KaInterface(connection, order.InterfaceId)
			Dim interfaceType As New KaInterfaceTypes(connection, interfaceInfo.InterfaceTypeId)
			lblInterfaceName.Text = interfaceInfo.Name
			_interfaceAllowsProductChange = interfaceType.AllowOrderProductChange
			_interfaceAllowsOrderItemGroupingChange = interfaceType.AllowOrderItemGroupingChange
			_interfaceAllowsCustomerChange = interfaceType.AllowOrderCustomerChange
			btnSortUsingProductPriority.Visible = True
		Catch ex As RecordNotFoundException
			lblInterfaceName.Text = "Interface not assigned"
			btnSortUsingProductPriority.Visible = False
		End Try
		_orderItemGroupings.Clear()
		_orderItemGroupings.Add(New KaOrderItemGroup())
		For Each orderItemGroup As KaOrderItemGroup In KaOrderItemGroup.GetAll(connection, String.Format("(deleted = 0) AND (id IN (SELECT order_item_group_id FROM order_items WHERE (order_items.deleted = 0) AND (order_items.order_id = {0})))", Q(order.Id)), "name", KaOrderItemGroup.TABLE_NAME)
			_orderItemGroupings.Add(orderItemGroup)
		Next
		PopulateOrderItemGroupTable()

		tbxNotes.Text = order.Notes
		tbxInternalNotes.Text = order.InternalNotes
		ddlBranches.SelectedValue = order.BranchId.ToString
		tbxShipTo.Text = order.ShipToName
		tbxShipToName.Text = order.ShipToName
		tbxShipToStreet.Text = order.ShipToStreet
		tbxShipToCity.Text = order.ShipToCity
		tbxShipToState.Text = order.ShipToState
		tbxShipToZip.Text = order.ShipToZipCode
		tbxShipToCountry.Text = order.ShipToCountry
		Session("tempShipToName") = tbxShipToName.Text
		Session("tempShipToStreet") = tbxShipToStreet.Text
		Session("tempShipToCity") = tbxShipToCity.Text
		Session("tempShipToState") = tbxShipToState.Text
		Session("tempShipToZip") = tbxShipToZip.Text
		Session("tempShipToCountry") = tbxShipToCountry.Text
		tbxDeliveredBatches.Text = order.DeliveredBatches.ToString
		tbxRequestedBatches.Text = order.RequestedBatches.ToString
		tbxAcres.Text = order.Acres
		ddlApplicator.SelectedValue = order.ApplicatorId.ToString
		Try ' to set the transport type...
			ddlTransportType.SelectedValue = order.TransportTypeId.ToString()
		Catch ex As ArgumentOutOfRangeException ' the specified transport type isn't available...
			Try ' to get the name of the transport type from the database...
				Dim transportType As New KaTransportTypes(connection, order.TransportTypeId)
				DisplayJavaScriptMessage("InvalidtypeNotAvail", Utilities.JsAlert(String.Format("Transport type ""{0}"" is no longer available. Transport type for order was not selected.", transportType.Name)))
			Catch ex2 As RecordNotFoundException ' the transport type isn't in the database...
				DisplayJavaScriptMessage("InvalidTypeId", Utilities.JsAlert(String.Format("Transport type (ID = {0}) is no longer available. Transport type for order was not selected.", order.TransportTypeId.ToString())))
			End Try
		End Try
		If order.ExpirationDate > New DateTime(1900, 1, 1) Then
			tbxExpirationDate.Value = order.ExpirationDate
		Else
			tbxExpirationDate.Value = ""
		End If
		PopulateExistingAccounts(order.Id)
		ShowProductsDropDownByOrderItemsCount(order.OrderItems.Count)
		PopulateExistingProducts(order.Id)
		PopulateOrderDestination(order.CustomerAccountLocationId)

		If order.Id = Guid.Empty Then
			'New order, don't display any tickets
			pnlTickets.Visible = False
		Else
			Dim ownerWebTicketAddress As New Dictionary(Of Guid, String)
			Dim allTickets As ArrayList = KaTicket.GetAll(connection, "voided=0 AND order_id = " & Q(order.Id), "loaded_at ASC")
			If allTickets.Count = 0 Then
				pnlTickets.Visible = False
			Else
				litTickets.Text = "<table border=""1"" width=""100%""><tr><td><strong>Date/time</strong></td><td><strong>Ticket</strong></td></tr>"
				For Each r As KaTicket In allTickets
					Dim webticket As String
					Dim ticketOwnerId As Guid = GetOwnerIdForTicket(connection, r.Id)
					If ownerWebTicketAddress.ContainsKey(ticketOwnerId) Then
						webticket = ownerWebTicketAddress(ticketOwnerId)
					Else
						webticket = Reports.WebTicketUrlForOwner(connection, ticketOwnerId).Replace("?ticket_id=", "").Replace("&ticket_id=", "").Trim
						If Utilities.ConvertWebPageUrlDomainToRequestedPagesDomain(connection) Then webticket = Tm2Database.GetUrlInCurrentDomain(Tm2Database.GetCurrentPageUrlWithOriginalPort(Me.Request), webticket)
						ownerWebTicketAddress.Add(ticketOwnerId, webticket)
					End If

					litTickets.Text &= "<tr><td>" & r.LoadedAt & "</td><td><a href=""" & webticket & IIf(webticket.Contains("?"), "&", "?") & "ticket_id=" & r.Id.ToString & "&instanceGuid=" & Guid.NewGuid.ToString & """ target=""_blank"">" & r.Number & IIf(Not r.Voided AndAlso r.Archived, " (archived)", "") & "</a></td></tr>"
				Next
				litTickets.Text &= "</table>"

				pnlTickets.Visible = True
			End If
		End If

		_customFieldData.Clear()
		For Each customFieldValue As KaCustomFieldData In KaCustomFieldData.GetAll(connection, String.Format("deleted = 0 AND {0} = {1}", KaCustomFieldData.FN_RECORD_ID, Q(order.Id)), KaCustomFieldData.FN_LAST_UPDATED)
			_customFieldData.Add(customFieldValue)
		Next
		Utilities.CreateDynamicCustomFieldPanelControls(_customFields, _customFieldData, lstCustomFields, Page)

		Session("prodIndex_1") = -1
		SetControlUsabilityFromPermissions()
		pnlShipTo.Visible = False
		cbxDoNotBlend_CheckedChanged(Nothing, Nothing)
	End Sub

	Private Sub ClearFields()
		tbxLastUpdated.Value = DateTime.MinValue

		tbxReleaseNumber.Text = ""
		cbxDoNotBlend.Checked = False
		lblOrderDetailStatus.Text = ""
		pnlOrderDetailStatus.Visible = False
		btnSetLockedStatus.Visible = False
		btnClearLockedStatus.Visible = True
		tbxPurchaseOrderNum.Text = ""
		tbxNotes.Text = ""
		tbxInternalNotes.Text = ""
		ddlBranches.SelectedIndex = 0
		PopulateExistingProducts(New ArrayList())
		ResetAccounts()
		ddlOwners_SelectedIndexChanged(Nothing, Nothing)
		btnDelete.Enabled = False
		tbxDeliveredBatches.Text = "0"
		tbxRequestedBatches.Text = "1"
		tbxAcres.Text = "0"
		ddlApplicator.SelectedIndex = 0
		ddlTransportType.SelectedIndex = 0
		tbxExpirationDate.Value = ""
		litTickets.Text = ""
		PopulateOrderDestination(Guid.Empty)
		_customFieldData.Clear()
	End Sub

	Protected Sub btnFind_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnFind.Click
		Dim orders As New List(Of Guid)
		If tbxFind.Text.Trim.Length > 0 Then orders = KaOrder.GetOrderIdsWithKeyword(GetUserConnection(_currentUser.Id), Nothing, _currentUser.OwnerId, tbxFind.Text.Trim, False) ' get a list of the order IDs

		Dim found As Boolean = False
		Dim i As Integer = ddlOrders.SelectedIndex ' begin with the next order in the drop-down list
		Do
			If i + 1 = ddlOrders.Items.Count Then i = 0 Else i += 1 ' wrap around to the beginning of the drop-down list
			If orders.IndexOf(Guid.Parse(ddlOrders.Items(i).Value)) <> -1 Then ' this is one of the orders that was found, select it
				ddlOrders.SelectedIndex = i
				found = True
				Exit Do ' no need to look any further
			End If
		Loop While i <> ddlOrders.SelectedIndex ' continue until we've come back to where we started
		If found Then
			ddlOrders_SelectedIndexChanged(sender, e)
		Else
			DisplayJavaScriptMessage("InvalidKeyword", Utilities.JsAlert("Order not found containing keywords: " & tbxFind.Text))
		End If
	End Sub

	Private Enum OrderItemField
		ProductId = 0
		Request = 1
		Delivered = 2
		UnitId = 3
		Notes = 4
		OrderItemId = 5
		OrderItemGroupingId = 6
	End Enum

	Private Function GetListOfItems(ByVal transaction As OleDbTransaction) As List(Of Dictionary(Of OrderItemField, String))
		Dim list As New List(Of Dictionary(Of OrderItemField, String))
		For i As Integer = 1 To MAX_PRODUCT_COUNT
			If FindControl(String.Format(PNL_PRODUCT, i)).Visible Then
				Dim orderItem As DropDownList = FindControl(String.Format(DDL_PRODUCT, i))
				Dim item As New Dictionary(Of OrderItemField, String)
				item(OrderItemField.ProductId) = orderItem.SelectedValue
				item(OrderItemField.Request) = CType(FindControl(String.Format(TBX_PRODUCT_AMOUNT, i)), TextBox).Text
				item(OrderItemField.Delivered) = CType(FindControl(String.Format(LBL_DELIV, i)), Label).Text
				item(OrderItemField.UnitId) = IIf(orderItem.SelectedIndex = 0, Guid.Empty.ToString(), CType(FindControl(String.Format(DDL_UNITS, i)), DropDownList).SelectedValue)
				item(OrderItemField.Notes) = CType(FindControl(String.Format(TBX_PRODUCT_NOTES, i)), TextBox).Text
				item(OrderItemField.OrderItemGroupingId) = IIf(orderItem.SelectedIndex = 0, Guid.Empty.ToString(), CType(FindControl(String.Format(DDL_ORDER_ITEM_GROUPING, i)), DropDownList).SelectedValue)

				Dim orderItemId As Guid = Guid.Empty
				Guid.TryParse(CType(FindControl(String.Format(LBL_ORDER_ITEM_ID, i)), Label).Text, orderItemId)
				If orderItemId <> Guid.Empty Then
					Dim tempOrderItem As KaOrderItem = New KaOrderItem(GetUserConnection(_currentUser.Id), orderItemId, transaction)
					item(OrderItemField.OrderItemId) = orderItemId.ToString
				Else
					item(OrderItemField.OrderItemId) = Guid.Empty.ToString
				End If
				list.Add(item)
			End If
		Next
		Return list
	End Function

	Private Enum OrderAccountField
		CustomerAccountId = 0
		Percent = 1
	End Enum

	Private Function GetListOfAccounts() As List(Of Dictionary(Of OrderAccountField, String))
		Dim list As New List(Of Dictionary(Of OrderAccountField, String))
		For i As Integer = 1 To MAX_ACCOUNT_COUNT
			Dim accountDropDown As DropDownList = FindControl(String.Format("ddlAccount{0:0}", i))
			If accountDropDown.SelectedIndex > 0 Then
				Dim account As New Dictionary(Of OrderAccountField, String)
				account(OrderAccountField.CustomerAccountId) = accountDropDown.SelectedValue
				account(OrderAccountField.Percent) = CType(FindControl(String.Format("tbxPercent{0:0}", i)), TextBox).Text
				list.Add(account)
			End If
		Next
		Return list
	End Function

	Private Function CreateNewDesinationFromOrderShipToInformation(connection As OleDbConnection, transaction As OleDbTransaction, shipToName As String, shipToStreet As String, shipToCity As String, shipToState As String, shipToZipCode As String, shipToCountry As String, order As KaOrder) As Guid
		Dim orderAccount As KaOrderCustomerAccount = Nothing
		For Each item In order.OrderAccounts
			If orderAccount Is Nothing OrElse item.Percentage > orderAccount.Percentage Then orderAccount = item
		Next
		Dim shipToLocation As New KaCustomerAccountLocation()
		shipToLocation.CustomerAccountId = orderAccount.CustomerAccountId
		shipToLocation.Name = shipToName
		shipToLocation.Street = shipToStreet
		shipToLocation.City = shipToCity
		shipToLocation.State = shipToState
		shipToLocation.ZipCode = shipToZipCode
		shipToLocation.Country = shipToCountry
		shipToLocation.SqlInsert(connection, transaction, Database.ApplicationIdentifier, _currentUser.Name)
		Return shipToLocation.Id
	End Function

	Private Function SaveOrder(ByVal saveAndNewClicked As Boolean, ByVal ignoreLastUpdatedTime As Boolean) As Boolean
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim createNewDestinationFromShipToInformationSettingEnabled As Boolean = Settings.GetMySetting(connection)
		If ValidateForm(connection, saveAndNewClicked, ignoreLastUpdatedTime) Then
			Dim orderNumberPerOwner As Boolean = SeparateOrderNumberPerOwner ' do this before starting a transaction
			Dim generateOrderNumber As Boolean = AutoGenerateOrderNumber
			Dim productUsesQuantity As New Dictionary(Of Guid, Boolean) ' Guid = product ID
			Dim orderGroupsUsed As New List(Of Guid)
			For Each item As Dictionary(Of OrderItemField, String) In GetListOfItems(Nothing) ' get a list of products that will require quantities
				Dim productId As Guid = Guid.Parse(item(OrderItemField.ProductId))
				If Not productUsesQuantity.ContainsKey(productId) Then
					Dim product As New KaProduct(connection, productId)
					productUsesQuantity(productId) = Not product.IsFunction(connection) OrElse product.IsTimedFunction(connection)
				End If
				Dim groupingId As Guid = Guid.Empty
				If Guid.TryParse(item(OrderItemField.OrderItemGroupingId), groupingId) AndAlso Not groupingId.Equals(Guid.Empty) AndAlso Not orderGroupsUsed.Contains(groupingId) Then orderGroupsUsed.Add(groupingId)
			Next
			Dim transaction As OleDbTransaction = connection.BeginTransaction(IsolationLevel.Serializable)
			Try
				For Each orderItemGroup As KaOrderItemGroup In _orderItemGroupings
					If orderGroupsUsed.Contains(orderItemGroup.Id) Then
						orderItemGroup.SqlUpdateInsertIfNotFound(connection, transaction, Database.ApplicationIdentifier, _currentUser.Name)
					ElseIf Not orderItemGroup.Id.Equals(Guid.Empty) Then
						orderItemGroup.Deleted = True
						orderItemGroup.SqlUpdate(connection, transaction, Database.ApplicationIdentifier, _currentUser.Name)
					End If
				Next
				Dim orderId As Guid = Guid.Parse(ddlOrders.SelectedValue)
				Dim order As KaOrder
				If orderId <> Guid.Empty Then order = New KaOrder(connection, orderId, transaction) Else order = New KaOrder()
				order.GetChildren(connection, transaction, True)
				order.OwnerId = Guid.Parse(ddlOwners.SelectedValue)
				If order.Id = Guid.Empty AndAlso generateOrderNumber AndAlso
				   (tbxOrderNum.Text = AUTOMATIC_ORDER_NUMBER_PLACEHOLDER OrElse tbxOrderNum.Text.Trim().Length = 0) Then ' generate an order number
					tbxOrderNum.Text = KaOrder.GetNextOrderNumber(connection, transaction, IIf(orderNumberPerOwner, order.OwnerId, Guid.Empty))
				End If
				order.Number = tbxOrderNum.Text
				order.ReleaseNumber = tbxReleaseNumber.Text.Trim
				order.PurchaseOrder = tbxPurchaseOrderNum.Text
				order.Notes = tbxNotes.Text
				order.InternalNotes = tbxInternalNotes.Text
				order.BranchId = Guid.Parse(ddlBranches.SelectedValue)
				order.OverScalePercent = Double.Parse(tbxOverun.Text)
				order.DoNotBlend = cbxDoNotBlend.Checked
				order.CustomerAccountLocationId = Guid.Parse(ddlCustomerSite.SelectedValue)
				order.DeliveredBatches = Integer.Parse(tbxDeliveredBatches.Text)
				order.RequestedBatches = Integer.Parse(tbxRequestedBatches.Text)
				order.Acres = Double.Parse(tbxAcres.Text)
				order.ApplicatorId = Guid.Parse(ddlApplicator.SelectedValue)
				order.TransportTypeId = Guid.Parse(ddlTransportType.SelectedValue)
				Dim expirationDate As DateTime = SQL_MINDATE
				If DateTime.TryParse(tbxExpirationDate.Value, expirationDate) Then
					order.ExpirationDate = expirationDate
				Else
					order.ExpirationDate = SQL_MINDATE
				End If
				Dim orderItems As List(Of KaOrderItem) = New List(Of KaOrderItem)(order.OrderItems) ' keep copy of the old list
				order.OrderItems.Clear() ' start with a clear list
				Dim position As Integer = 0 ' keep track of the position of each order item
				For Each item As Dictionary(Of OrderItemField, String) In GetListOfItems(transaction)
					Dim orderItem As KaOrderItem = Nothing
					Dim orderItemId As Guid = Guid.Empty
					Guid.TryParse(item(OrderItemField.OrderItemId), orderItemId)

					Dim productId As Guid = Guid.Parse(item(OrderItemField.ProductId))
					Dim request As Double = 0
					If productUsesQuantity(productId) Then
						Double.TryParse(item(OrderItemField.Request), request)
					Else
						request = 0
					End If
					Dim unitId As Guid = Guid.Parse(item(OrderItemField.UnitId))
					Dim notes As String = item(OrderItemField.Notes)
					Dim itemCounter As Integer = 0
					Do While itemCounter < orderItems.Count
						Dim existingOrderItem As KaOrderItem = orderItems(itemCounter)
						If existingOrderItem.Id.Equals(orderItemId) Then
							orderItem = orderItems(itemCounter)
							orderItems.RemoveAt(itemCounter)
							Exit Do
						Else
							itemCounter += 1
						End If
					Loop
					If orderItem Is Nothing Then orderItem = New KaOrderItem()
					orderItem.ProductId = productId
					orderItem.Request = request
					orderItem.UnitId = unitId
					orderItem.Position = position
					orderItem.Notes = notes
					orderItem.OrderItemGroupId = Guid.Parse(item(OrderItemField.OrderItemGroupingId))
					position += 1
					order.OrderItems.Add(orderItem)
				Next
				For Each orderItem As KaOrderItem In orderItems ' remove any order items that didn't get used
					If orderItem.Id <> Guid.Empty Then ' this record exists in the database
						orderItem.Deleted = True
						orderItem.SqlUpdate(connection, transaction, Database.ApplicationIdentifier, _currentUser.Name)
					End If
				Next
				UpdateOrderCustomerAccounts(connection, transaction, order)
				If createNewDestinationFromShipToInformationSettingEnabled = True AndAlso
					Guid.Parse(ddlCustomerSite.SelectedValue) = Guid.Empty AndAlso tbxShipToName.Text.Trim().Length > 0 AndAlso
																(tbxShipToCity.Text.Trim().Length > 0 OrElse
																tbxShipToStreet.Text.Trim().Length > 0 OrElse
																tbxShipToCountry.Text.Trim().Length > 0 OrElse
																tbxShipToState.Text.Trim().Length > 0 OrElse
																tbxShipToZip.Text.Trim().Length > 0) Then
					order.CustomerAccountLocationId = CreateNewDesinationFromOrderShipToInformation(connection, transaction, tbxShipToName.Text, tbxShipToStreet.Text, tbxShipToCity.Text, tbxShipToState.Text, tbxShipToZip.Text, tbxShipToCountry.Text, order)
					order.ShipToName = ""
					order.ShipToStreet = ""
					order.ShipToCity = ""
					order.ShipToState = ""
					order.ShipToZipCode = ""
					order.ShipToCountry = ""
				Else
					order.ShipToName = tbxShipToName.Text
					order.ShipToStreet = tbxShipToStreet.Text
					order.ShipToCity = tbxShipToCity.Text
					order.ShipToState = tbxShipToState.Text
					order.ShipToZipCode = tbxShipToZip.Text
					order.ShipToCountry = tbxShipToCountry.Text
				End If
				If order.Id = Guid.Empty Then ' this is a new order
					order.Created = Now ' we only set this once
					order.SqlInsert(connection, transaction, Database.ApplicationIdentifier, _currentUser.Name)
				Else ' this order already exists, update it
					order.SqlUpdateInsertIfNotFound(connection, transaction, Database.ApplicationIdentifier, _currentUser.Name)
				End If
				Utilities.ConvertCustomFieldPanelToLists(_customFields, _customFieldData, lstCustomFields)
				For Each customData As KaCustomFieldData In _customFieldData
					customData.RecordId = order.Id
					customData.SqlUpdateInsertIfNotFound(connection, transaction, Database.ApplicationIdentifier, _currentUser.Name)
				Next
				transaction.Commit()
				If ddlOrders.SelectedIndex = 0 Then
					ddlOrders.Items(ddlOrders.SelectedIndex).Value = order.Id.ToString()
					ddlOrders.Items(ddlOrders.SelectedIndex).Text = order.Number
				End If

			Catch ex As Exception
				transaction.Rollback()
				Throw ex ' re-throw the exception
			End Try
			Dim temp As String
			If saveAndNewClicked Then
				temp = Guid.Empty.ToString
			Else
				temp = ddlOrders.SelectedValue
			End If
			lblStatus.Text = "Order saved successfully"
			PopulateOrdersList()
			Try
				ddlOrders.SelectedValue = temp
			Catch ex As ArgumentOutOfRangeException

			End Try
			ddlOrders_SelectedIndexChanged(ddlOrders, New EventArgs())
			Return True
		Else
			Return False
		End If
	End Function

	Private Sub UpdateOrderCustomerAccounts(connection As OleDbConnection, transaction As OleDbTransaction, order As KaOrder)
		Dim orderAccounts As List(Of KaOrderCustomerAccount) = New List(Of KaOrderCustomerAccount)(order.OrderAccounts)
		order.OrderAccounts.Clear()
		For Each account As Dictionary(Of OrderAccountField, String) In GetListOfAccounts()
			Dim orderAccount As KaOrderCustomerAccount
			Dim customerAccountId As Guid = Guid.Parse(account(OrderAccountField.CustomerAccountId))
			Dim percentage As Double = Double.Parse(account(OrderAccountField.Percent))
			If orderAccounts.Count > 0 Then ' check to see if we can update the order record
				If orderAccounts(0).CustomerAccountId = customerAccountId Then ' this order account is for the same account
					orderAccount = orderAccounts(0) ' we'll update this order account record
					orderAccounts.RemoveAt(0) ' remove it from the list so that we don't use it again
				Else ' the order account doesn't match, create a new one
					orderAccount = New KaOrderCustomerAccount()
					orderAccount.CustomerAccountId = customerAccountId
					orderAccounts.Add(orderAccounts(0)) ' move this record to the end of the list
					orderAccounts.RemoveAt(0)
				End If
			Else ' no order accounts available to update, create a new order account
				orderAccount = New KaOrderCustomerAccount()
				orderAccount.CustomerAccountId = customerAccountId
			End If
			orderAccount.Percentage = percentage
			order.OrderAccounts.Add(orderAccount)
		Next
		For Each orderAccount As KaOrderCustomerAccount In orderAccounts ' remove any order accounts that didn't get used
			If orderAccount.Id <> Guid.Empty Then ' this record exists in the database
				orderAccount.Deleted = True
				orderAccount.SqlUpdate(connection, transaction, Database.ApplicationIdentifier, _currentUser.Name)
			End If
		Next
	End Sub

	Protected Sub btnSave_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnSave.Click
		SaveOrder(False, False)
	End Sub

	Protected Sub btnSaveNew_Click(sender As Object, e As EventArgs) Handles btnSaveNew.Click
		SaveOrder(True, False)
	End Sub

	Private ReadOnly Property AutoGenerateOrderNumber As Boolean
		Get
			Return Boolean.Parse(KaSetting.GetSetting(GetUserConnection(_currentUser.Id), KaSetting.SN_AUTO_GENERATE_ORDER_NUMBER, KaSetting.SD_AUTO_GENERATE_ORDER_NUMBER))
		End Get
	End Property

	Private Function ValidateForm(ByVal connection As OleDbConnection, ByVal saveAndNewClicked As Boolean, ByVal ignoreLastUpdatedTime As Boolean) As Boolean
		Dim order As KaOrder
		Try
			order = New KaOrder(connection, Guid.Parse(ddlOrders.SelectedValue))

			If order.IsOrderStaged(connection, Nothing) Then
				DisplayJavaScriptMessage("InvalidStagedOrder", Utilities.JsAlert("Unable to save.  The order is assigned to an existing staged order."))
				Return False
			End If
			If order.Locked Then
				DisplayJavaScriptMessage("InvalidLocked", Utilities.JsAlert("Unable to save.  The order is locked."))
				Return False
			End If
			If order.Completed Then
				DisplayJavaScriptMessage("InvalidCompleted", Utilities.JsAlert("Unable to save.  The order has been marked as completed."))
				Return False
			End If
		Catch ex As RecordNotFoundException
			' Not an existing order...
			order = New KaOrder()
		End Try

		If ddlOwners.SelectedIndex < 1 Then
			DisplayJavaScriptMessage("InvalidOwner", Utilities.JsAlert("Please select an owner."))
			Return False
		End If

		UpdateTotalPercent()
		Dim orderId As Guid = Guid.Empty
		Guid.TryParse(ddlOrders.SelectedValue, orderId)
		Dim orderNumber As String = tbxOrderNum.Text.Trim
		If AutoGenerateOrderNumber AndAlso orderNumber = AUTOMATIC_ORDER_NUMBER_PLACEHOLDER Then orderNumber = ""

		' This is an existing order, we should not allow the number to be blank unless we are going to auto-assign again
		If Not AutoGenerateOrderNumber AndAlso orderNumber.Trim().Length = 0 Then
			DisplayJavaScriptMessage("InvalidOrderNumber", Utilities.JsAlert("Please enter an order number."))
			Return False
		End If

		If Not IsNumeric(tbxOverun.Text) Then
			DisplayJavaScriptMessage("InvalidCharOverScaling", Utilities.JsAlert("Please enter a numeric value for over scaling."))
			Return False
		ElseIf Double.Parse(tbxOverun.Text) < 0 Then
			DisplayJavaScriptMessage("InvalidOverScaling", Utilities.JsAlert("Please enter a value greater than or equal to zero for over scaling."))
			Return False
		End If

		If Not IsNumeric(tbxDeliveredBatches.Text) Then
			DisplayJavaScriptMessage("InvalidCharDeliveredBatches", Utilities.JsAlert("Please enter a numeric value for the number of delivered batches."))
			Return False
		ElseIf Double.Parse(tbxDeliveredBatches.Text) < 0 Then
			DisplayJavaScriptMessage("InvalidDeliveredBatches", Utilities.JsAlert("Please enter a value greater than or equal to zero for the number of delivered batches."))
			Return False
		End If

		If Not IsNumeric(tbxRequestedBatches.Text) Then
			DisplayJavaScriptMessage("InvalidCharRequestedBatches", Utilities.JsAlert("Please enter a numeric value for the number of requested batches."))
			Return False
		ElseIf Double.Parse(tbxRequestedBatches.Text) < 0 Then
			DisplayJavaScriptMessage("InvalidRequestedBatches", Utilities.JsAlert("Please enter a value greater than or equal to zero for the number of requested batches."))
			Return False
		End If

		' do not let the user save an order with a product not selected
		For i As Integer = 1 To MAX_PRODUCT_COUNT
			Dim productList As DropDownList = FindControl(String.Format(DDL_PRODUCT, i))
			If productList.Visible AndAlso productList.SelectedIndex = 0 Then
				DisplayJavaScriptMessage("InvalidProduct", Utilities.JsAlert(String.Format("Please select a product (or function) for order item {0:0} under ""Product(s) to be dispensed"".", i)))
				Return False
			End If
		Next
		For i As Integer = 1 To MAX_PRODUCT_COUNT ' make sure products have a qty entered.
			Dim tbxProductAmount As TextBox = FindControl(String.Format(TBX_PRODUCT_AMOUNT, i))
			If tbxProductAmount.Visible Then ' only look at visible products.
				Dim productId As Guid = Guid.Parse(CType(FindControl(String.Format(DDL_PRODUCT, i)), DropDownList).SelectedValue)
				Dim productAmount As Double = 0.0
				If Not Utilities.IsProductParameterlessFunction(productId, _currentUser.Id) Then
					If Not Double.TryParse(tbxProductAmount.Text, productAmount) Then
						DisplayJavaScriptMessage("InvalidCharProdQty", Utilities.JsAlert("Please enter a numeric request amount for each product"))
						Return False
					ElseIf productAmount <= 0 Then
						DisplayJavaScriptMessage("InvalidProdQty", Utilities.JsAlert("Please enter a request amount greater than zero for each product"))
						Return False
					End If
				End If
			End If
		Next

		Dim customerAccounts As New List(Of Guid) ' check for duplicate accounts
		For i As Integer = 1 To MAX_ACCOUNT_COUNT
			Dim customerAccountList As DropDownList = FindControl(String.Format(DDL_ACCOUNT, i))
			If customerAccountList.SelectedIndex > 0 Then
				Dim customerAccountId As Guid = Guid.Parse(customerAccountList.SelectedValue)
				If customerAccounts.Contains(customerAccountId) Then
					Dim duplicateCustomer As KaCustomerAccount = New KaCustomerAccount(connection, customerAccountId)
					DisplayJavaScriptMessage("InvalidDuplicateCust", Utilities.JsAlert(duplicateCustomer.Name & " may only be selected once as a customer account. Please remove all but one instance of this account."))
					Return False
				Else
					customerAccounts.Add(customerAccountId)
				End If
			ElseIf customerAccountList.Visible Then
				DisplayJavaScriptMessage("InvalidCustSelected", Utilities.JsAlert(String.Format("Please select a customer account for customer account {0:0} under ""Account(s) to be billed"".", i)))
				Return False
			End If
		Next
		Dim totalPct As Double = 0
		If UpdateTotalPercent() AndAlso Double.TryParse(tbxTotalPercent.Text, totalPct) AndAlso totalPct <> 100 Then ' make sure the account percentage equals 100%
			DisplayJavaScriptMessage("InvalidCustPercentage", Utilities.JsAlert("Account percentages do not add up to 100%. Please adjust account percentages to total 100%."))
			Return False
		End If

		If orderNumber.Trim.Length > 0 AndAlso (orderNumber.Trim <> order.Number OrElse ddlOwners.SelectedValue <> order.OwnerId.ToString) Then
			Dim orderNumberUsedRdr As OleDbDataReader = Tm2Database.ExecuteReader(connection, "SELECT count(*) AS ordCount, completed " &
													   "FROM orders " &
													   "WHERE (deleted = 0) " &
															"AND (id <> " & Q(ddlOrders.SelectedValue) & ") " &
															"AND (owner_id = " & Q(ddlOwners.SelectedValue) & ") " &
															"AND (number = " & Q(orderNumber) & ") " &
													   "GROUP BY completed " &
													   "ORDER BY completed")
			Do While orderNumberUsedRdr.Read()
				Dim orderCount As Integer = orderNumberUsedRdr.Item("ordCount")
				If orderCount > 0 Then
					If Not orderNumberUsedRdr.Item("completed") Then
						DisplayJavaScriptMessage("InvalidOrdNumberUsed", Utilities.JsAlert("Order number " & orderNumber & " is currently assigned to " & IIf(orderCount = 1, "a current order", " current orders") & " for this owner.  Verify that this is a valid number."))
						Exit Do
					Else
						DisplayJavaScriptMessage("InvalidOrdNumberUsed", Utilities.JsAlert("Order number " & orderNumber & " is assigned to " & IIf(orderCount = 1, "a past order", " past orders") & " for this owner.  Verify that this is a valid number."))
						Exit Do
					End If
				End If
			Loop
			orderNumberUsedRdr.Close()
		End If

		Dim lastupdated As DateTime = DateTime.MinValue
		DateTime.TryParse(tbxLastUpdated.Value, lastupdated)
		Dim orderLastupdated As DateTime
		With order.LastUpdated
			orderLastupdated = New DateTime(.Year, .Month, .Day, .Hour, .Minute, .Second)
		End With

		If tbxLastUpdated.Value > New DateTime(1900, 1, 1) AndAlso lastupdated < orderLastupdated Then
			If ignoreLastUpdatedTime Then
				Utilities.CreateEventLogEntry(KaEventLog.Categories.Warning, "User " & _currentUser.Name & " saved order " & order.Number & " with older last updated time.", connection)
			Else
				Dim javaScript As String =
							 "<script language='JavaScript'>" &
							 "if (confirm('The order has been updated since the last time it was saved.\nAre you sure you want to save this order?') == true)" &
							 "{" &
							  ClientScript.GetPostBackEventReference(Me, "SaveOrder('" & saveAndNewClicked.ToString & "','True')") &
							 "}" &
							 "</script>"
				DisplayJavaScriptMessage("ConfirmInvalidLastUpdatedDateScript", javaScript)
				Return False
			End If
		End If
		Return True
	End Function

	Protected Sub btnMarkComplete_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnMarkComplete.Click
		Dim orderId As Guid = Guid.Empty
		Guid.TryParse(ddlOrders.SelectedValue, orderId)
		With New KaOrder(GetUserConnection(_currentUser.Id), orderId)
			If .OrderReferencedInOtherProgressRecords(GetUserConnection(_currentUser.Id), Nothing, Guid.Empty) Then
				DisplayJavaScriptMessage("InvalidProgressRecords", Utilities.JsAlert("The order is currently in progress of being dispensed, so it cannot be marked as completed."))
				Exit Sub
			ElseIf .IsOrderStaged(GetUserConnection(_currentUser.Id), Nothing) Then
				DisplayJavaScriptMessage("InvalidStaged", Utilities.JsAlert("The order is currently assigned to a staged order, so it cannot be marked as completed."))
				Exit Sub
			End If
			.Completed = True
			.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
			lblStatus.Text = "Selected order has been marked complete"
		End With
		ClearFields()
		PopulateOrdersList()
		Try
			ddlOrders.SelectedValue = orderId.ToString()
		Catch ex As ArgumentOutOfRangeException

		End Try
		ddlOrders_SelectedIndexChanged(ddlOrders, New EventArgs())
	End Sub

	Protected Sub btnCopy_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnCopy.Click
		tbxStartNumber.Text = tbxOrderNum.Text
		pnlMain.Visible = False
		pnlCopyOrder.Visible = True
	End Sub

	Protected Sub btnCreateCopies_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnCreateCopies.Click
		Dim copies As Int64
		Try ' to parse the number of copies to make...
			copies = Int64.Parse(tbxNumOfCopys.Text)
		Catch ex As FormatException ' the user didn't enter a valid number of copies...
			DisplayJavaScriptMessage("InvalidCopies", Utilities.JsAlert("Please enter a numeric value for copies to be made"))
			Exit Sub ' can't proceed any further
		End Try
		Dim orderId As Guid = Guid.Parse(ddlOrders.SelectedValue)
		Dim ownerId As Guid
		If SeparateOrderNumberPerOwner Then ' determine the owner ID
			ownerId = Guid.Parse(ddlOwners.SelectedItem.Value.ToString)
		Else ' no need to determine the owner ID
			ownerId = Guid.Empty
		End If
		Dim orderNumber As String = tbxStartNumber.Text
		Dim updateNextOrderNumber As Boolean = AutoGenerateOrderNumber
		Dim order As KaOrder = Nothing
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim transaction As OleDbTransaction = connection.BeginTransaction(IsolationLevel.Serializable)
		Try
			If orderNumber = AUTOMATIC_ORDER_NUMBER_PLACEHOLDER Then orderNumber = KaOrder.GetNextOrderNumber(connection, transaction, ownerId)
			order = New KaOrder(connection, orderId, transaction)
			order.DeliveredBatches = 0
			order.LastLoaded = SQL_MINDATE
			order.Locked = False
			order.Created = Now
			Dim orderItemGroupXRef As New Dictionary(Of Guid, Guid)
			For Each orderNumber In KaOrder.GetSequenceOfOrderNumbers(connection, transaction, orderNumber, copies, ownerId, updateNextOrderNumber)
				For Each orderItem As KaOrderItem In order.OrderItems ' this forces the order to fetch the order item records
					orderItem.Id = Guid.Empty ' break the connection from the original record
					orderItem.OrderId = Guid.Empty ' break the connection from the original order record
					orderItem.Delivered = 0 ' allow new copied order to be dispensed fully
					If Not orderItem.OrderItemGroupId.Equals(Guid.Empty) Then
						If Not orderItemGroupXRef.ContainsKey(orderItem.OrderItemGroupId) Then
							Try
								Dim orderItemGroup = New KaOrderItemGroup(connection, orderItem.OrderItemGroupId, transaction)
								orderItemGroup.Id = Guid.Empty
								orderItemGroup.SqlInsert(connection, transaction, Database.ApplicationIdentifier, _currentUser.Name)
								orderItemGroupXRef.Add(orderItem.OrderItemGroupId, orderItemGroup.Id)
							Catch ex As RecordNotFoundException
								orderItemGroupXRef.Add(orderItem.OrderItemGroupId, Guid.NewGuid)
							End Try
						End If
						orderItem.OrderItemGroupId = orderItemGroupXRef(orderItem.OrderItemGroupId)
					End If
				Next
				For Each orderAccount As KaOrderCustomerAccount In order.OrderAccounts ' this forces the order to fetch the order account records
					orderAccount.Id = Guid.Empty ' break the connection from the original record
					orderAccount.OrderId = Guid.Empty ' break the connection from the original order record
				Next
				order.Id = Guid.Empty ' break the connection from the original record
				order.Number = orderNumber
				order.SqlInsert(connection, transaction, Database.ApplicationIdentifier, _currentUser.Name)
			Next
			transaction.Commit()
		Catch ex As Exception
			transaction.Rollback()
			Throw ex
		End Try
		PopulateOrdersList()
		pnlMain.Visible = True
		pnlCopyOrder.Visible = False
		Dim index As Integer = 0
		For i As Integer = 0 To ddlOrders.Items.Count - 1
			If ddlOrders.Items(i).Value = order.Id.ToString Then
				index = i
				Exit For
			End If
		Next
		ddlOrders.SelectedIndex = index
		ddlOrders_SelectedIndexChanged(Nothing, Nothing)
	End Sub

	Private ReadOnly Property UserCanChangeOrderNumber As Boolean
		Get
			Return Boolean.Parse(KaSetting.GetSetting(GetUserConnection(_currentUser.Id), KaSetting.SN_USER_CAN_CHANGE_ORDER_NUMBER, KaSetting.SD_USER_CAN_CHANGE_ORDER_NUMBER))
		End Get
	End Property

	Private ReadOnly Property SeparateOrderNumberPerOwner As Boolean
		Get
			Return Boolean.Parse(KaSetting.GetSetting(GetUserConnection(_currentUser.Id), KaSetting.SN_SEPARATE_ORDER_NUMBER_PER_OWNER, KaSetting.SD_SEPARATE_ORDER_NUMBER_PER_OWNER))
		End Get
	End Property

	Private Property NextOrderNumberForOwner(ownerId As Guid) As String
		Get
			Return KaSetting.GetSetting(GetUserConnection(_currentUser.Id), KaSetting.SN_NEXT_ORDER_NUMBER_FOR_OWNER & ownerId.ToString(), KaSetting.SD_NEXT_ORDER_NUMBER_FOR_OWNER)
		End Get
		Set(value As String)
			KaSetting.WriteSetting(GetUserConnection(_currentUser.Id), KaSetting.SN_NEXT_ORDER_NUMBER_FOR_OWNER & ownerId.ToString(), value)
		End Set
	End Property

	Private Property NextOrderNumber As String
		Get
			Return KaSetting.GetSetting(GetUserConnection(_currentUser.Id), KaSetting.SN_NEXT_ORDER_NUMBER, KaSetting.SD_NEXT_ORDER_NUMBER)
		End Get
		Set(value As String)
			KaSetting.WriteSetting(GetUserConnection(_currentUser.Id), KaSetting.SN_NEXT_ORDER_NUMBER, value)
		End Set
	End Property

	Private Function GetNextOrderNumber(ownerId As Guid) As String
		Dim orderNumber As String
		Dim c As OleDbConnection = GetUserConnection(_currentUser.Id)
		If AutoGenerateOrderNumber Then
			If SeparateOrderNumberPerOwner() Then
				orderNumber = NextOrderNumberForOwner(ownerId)
				NextOrderNumberForOwner(ownerId) = KaTicket.IncrementAlphaNumeric(orderNumber)
			Else
				orderNumber = NextOrderNumber
				NextOrderNumber = KaTicket.IncrementAlphaNumeric(orderNumber)
			End If
		Else
			orderNumber = ""
		End If
		Return orderNumber
	End Function

	Private Function CheckOrderNumber(ByVal orderNumber) As Boolean
		Dim validNumber As Boolean = False
		Dim o As ArrayList = KaOrder.GetAll(GetUserConnection(_currentUser.Id), "deleted = 0 AND number = '" & orderNumber & "'", "")
		If o.Count = 0 Then validNumber = True
		Return validNumber
	End Function

	Protected Sub btnCancelCopies_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnCancelCopies.Click
		pnlMain.Visible = True
		pnlCopyOrder.Visible = False
	End Sub

	Protected Sub btnPrintOrder_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnPrintOrder.Click
		Dim orderId As Guid = Guid.Parse(ddlOrders.SelectedValue)
		DisplayJavaScriptMessage("PrintOrder", Utilities.JsWindowOpen("OrderSummary.aspx?order_id=" & orderId.ToString & "&pfv=true&location_id=" & ddlFacilityFilter.SelectedValue)) ', "toolbar=yes,menubar=yes,scrollbars=yes,resizable=yes,width=700,height=500,top=50,left=50", True)) 'Open new window to view order to be printed.
	End Sub

	Protected Sub btnPointOfSale_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnPointOfSale.Click
		If ddlOrders.SelectedItem.Text <> "Enter a new order" AndAlso SaveOrder(False, False) Then
			Response.Redirect("PointOfSale.aspx?OrderId=" & ddlOrders.SelectedValue)
		End If
	End Sub

	Protected Sub btnShipTo_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnShipTo.Click
		If pnlShipTo.Visible = False Then
			Session("tempShipToName") = tbxShipToName.Text
			Session("tempShipToStreet") = tbxShipToStreet.Text
			Session("tempShipToCity") = tbxShipToCity.Text
			Session("tempShipToState") = tbxShipToState.Text
			Session("tempShipToZip") = tbxShipToZip.Text
			Session("tempShipToCountry") = tbxShipToCountry.Text
			pnlShipTo.Visible = True
		Else
			pnlShipTo.Visible = False
		End If
	End Sub

	Protected Sub btnShipToOk_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnShipToOk.Click
		pnlShipTo.Visible = False
		tbxShipTo.Text = tbxShipToName.Text
		tbxShipTo.Text &= IIf(tbxShipTo.Text.Trim().Length > 0, ", ", "") & tbxShipToStreet.Text
		tbxShipTo.Text &= IIf(tbxShipTo.Text.Trim().Length > 0, ", ", "") & tbxShipToCity.Text
		tbxShipTo.Text &= IIf(tbxShipTo.Text.Trim().Length > 0, ", ", "") & tbxShipToState.Text
		tbxShipTo.Text &= IIf(tbxShipTo.Text.Trim().Length > 0, ", ", "") & tbxShipToZip.Text
		tbxShipTo.Text &= IIf(tbxShipTo.Text.Trim().Length > 0, ", ", "") & tbxShipToCountry.Text
		ShipToChanged(tbxShipTo, New EventArgs)
	End Sub

	Protected Sub btnShipToCancel_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnShipToCancel.Click
		tbxShipToName.Text = Session("tempShipToName")
		tbxShipTo.Text = tbxShipToName.Text
		tbxShipToStreet.Text = Session("tempShipToStreet")
		tbxShipToCity.Text = Session("tempShipToCity")
		tbxShipToState.Text = Session("tempShipToState")
		tbxShipToZip.Text = Session("tempShipToZip")
		tbxShipToCountry.Text = Session("tempShipToCountry")
		pnlShipTo.Visible = False
		ShipToChanged(tbxShipTo, New EventArgs)
	End Sub

	Protected Sub btnDelete_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnDelete.Click
		Dim order As KaOrder = New KaOrder(GetUserConnection(_currentUser.Id), Guid.Parse(ddlOrders.SelectedValue))
		If order.OrderReferencedInOtherProgressRecords(GetUserConnection(_currentUser.Id), Nothing, Guid.Empty) Then
			DisplayJavaScriptMessage("CanNotDeleteOrder", Utilities.JsAlert("Not able to delete this order, it has active in progress records."))
			Return
		ElseIf order.Locked Then
			DisplayJavaScriptMessage("CanNotDeleteOrder", Utilities.JsAlert("Not able to delete this order, it is marked as locked."))
			Return
		ElseIf order.IsOrderStaged(GetUserConnection(_currentUser.Id), Nothing) Then
			DisplayJavaScriptMessage("CanNotDeleteOrder", Utilities.JsAlert("Not able to delete this order, it assigned to a staged order."))
			Return
		ElseIf order.HasTickets(GetUserConnection(_currentUser.Id), Nothing) Then
			DisplayJavaScriptMessage("CanNotDeleteOrder", Utilities.JsAlert("Not able to delete this order, it has tickets against it."))
			Return
		Else
			With order
				.Deleted = True
				.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
			End With
			ClearFields()
			PopulateOrdersList()
			If ddlOrders.Items.Count > 0 Then ddlOrders.SelectedIndex = 0
			ddlOrders_SelectedIndexChanged(ddlOrders, New EventArgs())
			lblStatus.Text = "Selected order deleted successfully"
		End If
	End Sub

	Private Sub ddlOwners_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlOwners.SelectedIndexChanged
		PopulateAccountCombos()
		PopulateProductCombos()
	End Sub

#Region "Products"
	Private Sub PopulateUnitsCombo(index As Integer, allUnits As ArrayList, selectedUnitId As Guid)
		Dim unit As DropDownList = FindControl(String.Format(DDL_UNITS, index))
		unit.Items.Clear()
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim product As DropDownList = FindControl(String.Format(DDL_PRODUCT, index))
		If product.SelectedIndex > 0 Then ' only populate the unit list if a product has been selected
			Dim showTime As Boolean
			Dim productId As Guid = Guid.Parse(product.SelectedValue)
			If productId <> Guid.Empty Then
				Try ' to determine if the product is time based...
					showTime = New KaProduct(connection, productId).IsTimedFunction(connection)
				Catch ex As RecordNotFoundException ' couldn't get the product details...
					showTime = False ' assume that it isn't time based
				End Try
			End If
			For Each u As KaUnit In allUnits
				If showTime Xor Not KaUnit.IsTime(u.BaseUnit) Then
					unit.Items.Add(New ListItem(u.Name, u.Id.ToString()))
					If u.Id = selectedUnitId Then unit.SelectedValue = selectedUnitId.ToString()
				End If
			Next
		End If
	End Sub

	Private _productsTable As New DataTable

	Private Sub PopulateProductCombos()
		_productsTable = New DataTable
		Dim productsDA As New OleDbDataAdapter("SELECT DISTINCT products.id, products.name " &
											  "FROM products " &
											  IIf(ddlFacilityFilter.SelectedValue = Guid.Empty.ToString(), "", "INNER JOIN product_bulk_products ON products.id = product_bulk_products.product_id ") &
											  "WHERE (products.owner_id = " & Q(ddlOwners.SelectedValue) & " OR products.owner_id = " & Q(Guid.Empty) & ") AND products.deleted = 0 " &
											  IIf(ddlFacilityFilter.SelectedValue = Guid.Empty.ToString(), "", "AND product_bulk_products.deleted = 0 AND product_bulk_products.location_id = " & Q(ddlFacilityFilter.SelectedValue) & " ") &
											  "ORDER BY name, id", GetUserConnection(_currentUser.Id))
		If Tm2Database.CommandTimeout > 0 Then productsDA.SelectCommand.CommandTimeout = Tm2Database.CommandTimeout
		productsDA.Fill(_productsTable)
		Dim emptyProd As DataRow = _productsTable.NewRow
		emptyProd.Item("id") = Guid.Empty
		emptyProd.Item("name") = "Select a product"
		_productsTable.Rows.InsertAt(emptyProd, 0)
		For i = 1 To MAX_PRODUCT_COUNT
			PopulateProductCombo(i)
		Next
	End Sub

	Private Sub PopulateProductCombo(ByVal i As Integer)
		Dim unitList As ArrayList = KaUnit.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "abbreviation ASC")

		Dim ddl As DropDownList = FindControl(String.Format(DDL_PRODUCT, i))
		If ddl.Visible Then
			Dim unit As DropDownList = FindControl(String.Format(DDL_UNITS, i))
			Dim selectedUnitId As Guid = Guid.Empty
			Try ' to get the unit that is currently selected
				selectedUnitId = Guid.Parse(unit.SelectedValue)
			Catch ex As FormatException ' a unit isn't selected
				selectedUnitId = Guid.Empty
			End Try
			Dim selectedId As String = IIf(ddl.SelectedIndex >= 0, ddl.SelectedValue, Guid.Empty.ToString)
			ddl.SelectedIndex = -1 ' This resolves an error with "The 'SelectedIndex' and 'SelectedValue' attributes are mutually exclusive"
			ddl.DataTextField = "name"
			ddl.DataValueField = "id"
			ddl.DataSource = _productsTable
			ddl.DataBind()

			Try
				ddl.SelectedValue = selectedId
			Catch ex As ArgumentOutOfRangeException
				'Suppress
			End Try
			PopulateUnitsCombo(i, unitList, selectedUnitId)
		End If
	End Sub

	Private Sub PopulateOrderItemGroupCombo(ByVal i As Integer)
		Dim ddl As DropDownList = FindControl(String.Format(DDL_ORDER_ITEM_GROUPING, i))
		If ddl IsNot Nothing Then
			Dim selectedId As String = IIf(ddl.SelectedIndex >= 0, ddl.SelectedValue, Guid.Empty.ToString)
			ddl.SelectedIndex = -1 ' This resolves an error with "The 'SelectedIndex' and 'SelectedValue' attributes are mutually exclusive"
			ddl.DataTextField = "name"
			ddl.DataValueField = "id"
			ddl.DataSource = _orderItemGroupingTable
			ddl.DataBind()

			Try
				ddl.SelectedValue = selectedId
			Catch ex As ArgumentOutOfRangeException
				'Suppress
			End Try
			Dim product As DropDownList = FindControl(String.Format(DDL_PRODUCT, i))

			ddl.Visible = product IsNot Nothing AndAlso product.SelectedIndex > 0 AndAlso _orderItemGroupings.Count > 1
		End If
	End Sub

	Public Function ProductArrayContains(ByVal products As ArrayList, ByVal productId As Guid, ByVal position As Integer) As Boolean
		For Each product In products
			Dim stringArray As String() = product.Split(",")
			If Guid.Parse(stringArray(0)) = productId And stringArray(3) = position Then
				Return True
			End If
		Next
		Return False
	End Function

	Protected Sub btnAddProduct_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnAddProduct.Click
		For index = 1 To MAX_PRODUCT_COUNT ' find next available product panel
			With FindControl(String.Format(PNL_PRODUCT, index))
				If Not .Visible Then
					.Visible = True
					ResetProductPanel(index)
					ShowProductControls(index)
					btnAddProduct.Visible = _currentUserPermission(_currentTableName).Edit AndAlso _interfaceAllowsProductChange AndAlso (index < MAX_PRODUCT_COUNT)

					PopulateProductCombos()
					Exit For
				End If
			End With
		Next
	End Sub

	Private Sub ResetProductPanel(index As Integer)
		With CType(FindControl(String.Format(DDL_PRODUCT, index)), DropDownList)
			.SelectedIndex = -1 ' reset the product selection
			If ddlOwners.SelectedIndex <= 0 Then .Items.Clear()
		End With
		CType(FindControl(String.Format(LBL_ORDER_ITEM_ID, index)), Label).Text = Guid.Empty.ToString()
		CType(FindControl(String.Format(DDL_UNITS, index)), DropDownList).Items.Clear()
		CType(FindControl(String.Format(TBX_PRODUCT_AMOUNT, index)), TextBox).Text = ""
		CType(FindControl(String.Format(LBL_DELIV, index)), Label).Text = "0"
		CType(FindControl(String.Format(TBX_PRODUCT_NOTES, index)), TextBox).Text = ""
		CType(FindControl(String.Format(DDL_ORDER_ITEM_GROUPING, index)), DropDownList).Items.Clear()
	End Sub

	Private Sub RemoveProductPanel(index As Integer)
		ResetProductPanel(index)

		FindControl(String.Format(PNL_PRODUCT, index)).Visible = False
	End Sub

	Private Sub SelectedDefaultUnit(ByVal index As Integer)
		Dim pnl = FindControl(String.Format(PNL_PRODUCT, index)) 'ToDo: define as
		If pnl.Visible Then
			Dim ddlProduct As System.Web.UI.WebControls.DropDownList = FindControl(String.Format(DDL_PRODUCT, index))
			If ddlProduct.SelectedIndex > 0 Then
				Dim ddlUnits As System.Web.UI.WebControls.DropDownList = FindControl(String.Format(DDL_UNITS, index))
				Dim product As KaProduct = New KaProduct(GetUserConnection(_currentUser.Id), Guid.Parse(ddlProduct.SelectedValue))
				Dim indexOfUnit = IndexOf(ddlUnits, product.DefaultUnitId)
				If indexOfUnit <> -1 Then
					ddlUnits.SelectedIndex = indexOfUnit
				End If
			End If
		End If
	End Sub

	Private Sub ShowProductsDropDownByOrderItemsCount(ByVal count As Integer)
		For i As Integer = 1 To MAX_PRODUCT_COUNT
			If i <= count Then
				FindControl(String.Format(PNL_PRODUCT, i)).Visible = True
			Else
				FindControl(String.Format(PNL_PRODUCT, i)).Visible = False
			End If
		Next
	End Sub

	Private Sub PopulateExistingProducts(ByVal orderId As Guid)
		Dim existingProducts As ArrayList = KaOrderItem.GetAll(GetUserConnection(_currentUser.Id), "order_id = " & Q(orderId) & " and deleted = 0", "position")
		PopulateExistingProducts(existingProducts)
	End Sub

	Private Sub PopulateExistingProducts(ByVal existingProducts As ArrayList)
		PopulateProductCombos()

		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim units As ArrayList = KaUnit.GetAll(connection, "deleted=0", "abbreviation ASC")
		ResetProducts(existingProducts.Count)
		If existingProducts.Count = 0 Then existingProducts.Add(New KaOrderItem)

		Dim index As Integer = 1
		For Each orderProduct As KaOrderItem In existingProducts
			Dim panel = FindControl(String.Format(PNL_PRODUCT, index))
			Dim orderItemId As Label = FindControl(String.Format(LBL_ORDER_ITEM_ID, index))
			Dim product As DropDownList = FindControl(String.Format(DDL_PRODUCT, index))
			Dim delivered As Label = FindControl(String.Format(LBL_DELIV, index))
			Dim requested As TextBox = FindControl(String.Format(TBX_PRODUCT_AMOUNT, index))
			Dim unit As DropDownList = FindControl(String.Format(DDL_UNITS, index))
			Dim notes As TextBox = FindControl(String.Format(TBX_PRODUCT_NOTES, index))
			Dim orderItemGrouping As DropDownList = FindControl(String.Format(DDL_ORDER_ITEM_GROUPING, index))

			panel.Visible = True
			PopulateProductCombo(index)
			orderItemId.Text = orderProduct.Id.ToString
			Dim indexOfProduct = IndexOf(product, orderProduct.ProductId)
			product.SelectedIndex = indexOfProduct
			If indexOfProduct = -1 AndAlso Not orderProduct.ProductId.Equals(Guid.Empty) Then
				Dim productErrorString As String = ""
				Try
					productErrorString = "The product " & New KaProduct(connection, orderProduct.ProductId).Name & " was assigned to position " & index.ToString() & ", but was unable to be set.  Verify the product before continuing."
				Catch ex As RecordNotFoundException
					productErrorString = "The product assigned to position " & index.ToString() & " was unable to be set.  Verify the product before continuing."
				End Try
				DisplayJavaScriptMessage("InvalidProduct" & index.ToString(), Utilities.JsAlert(productErrorString))
			End If
			PopulateUnitsCombo(index, units, orderProduct.UnitId)
			If orderProduct.Request = 0 Then
				requested.Text = ""
			Else
				requested.Text = orderProduct.Request
			End If
			delivered.Text = orderProduct.Delivered
			Dim indexOfUnit = IndexOf(unit, orderProduct.UnitId)
			unit.SelectedIndex = indexOfUnit
			notes.Text = orderProduct.Notes
			PopulateOrderItemGroupCombo(index)
			Dim indexOfGroup As Integer = IndexOf(orderItemGrouping, orderProduct.OrderItemGroupId)
			orderItemGrouping.SelectedIndex = indexOfGroup
			ShowProductControls(index)
			index += 1
		Next
		btnAddProduct.Visible = _currentUserPermission(_currentTableName).Edit AndAlso _interfaceAllowsProductChange AndAlso (existingProducts.Count < MAX_PRODUCT_COUNT)
	End Sub

	Private Function IndexOf(ByVal ddl As System.Web.UI.WebControls.DropDownList, ByVal findGuid As Guid) As Integer
		Dim index As Integer = 0
		For Each li As ListItem In ddl.Items
			If li.Value <> "Select a Product" Then
				If Guid.Parse(li.Value) = findGuid Then
					Return index
				End If
			End If
			index += 1
		Next
		Return -1
	End Function

	Private Sub ResetProducts(ByVal itemCount As Integer)
		For i As Integer = 1 To itemCount
			ResetProductPanel(i)
		Next
		For i = (itemCount + 1) To MAX_PRODUCT_COUNT
			RemoveProductPanel(i)
		Next
	End Sub

#Region "Remove Product Buttons"
	Private Sub btnRemoveProductClick(ByVal productPanelIndex As Integer)
		RemoveProductPanel(productPanelIndex)
		Dim position As Integer = 0 ' keep track of the position of each order item
		Dim itemList As New SortedList(Of Integer, KaOrderItem)
		For Each item As Dictionary(Of OrderItemField, String) In GetListOfItems(Nothing)
			Dim orderItem As New KaOrderItem
			Guid.TryParse(item(OrderItemField.OrderItemId), orderItem.Id)
			Guid.TryParse(item(OrderItemField.ProductId), orderItem.ProductId)
			Double.TryParse(item(OrderItemField.Request), orderItem.Request)
			Double.TryParse(item(OrderItemField.Delivered), orderItem.Delivered)
			Guid.TryParse(item(OrderItemField.UnitId), orderItem.UnitId)
			orderItem.Notes = item(OrderItemField.Notes)
			orderItem.OrderItemGroupId = Guid.Parse(item(OrderItemField.OrderItemGroupingId))
			orderItem.Position = position
			position += 1

			itemList.Add(orderItem.Position, orderItem)
		Next
		Dim existingProducts As New ArrayList
		For n As Integer = 0 To itemList.Count - 1
			existingProducts.Add(itemList(n))
		Next

		PopulateExistingProducts(existingProducts)
	End Sub

	Private Sub btnRemoveProduct1_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnRemoveProduct1.Click
		btnRemoveProductClick(1)
	End Sub
	Private Sub btnRemoveProduct2_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnRemoveProduct2.Click
		btnRemoveProductClick(2)
	End Sub
	Private Sub btnRemoveProduct3_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnRemoveProduct3.Click
		btnRemoveProductClick(3)
	End Sub
	Private Sub btnRemoveProduct4_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnRemoveProduct4.Click
		btnRemoveProductClick(4)
	End Sub
	Private Sub btnRemoveProduct5_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnRemoveProduct5.Click
		btnRemoveProductClick(5)
	End Sub
	Private Sub btnRemoveProduct6_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnRemoveProduct6.Click
		btnRemoveProductClick(6)
	End Sub
	Private Sub btnRemoveProduct7_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnRemoveProduct7.Click
		btnRemoveProductClick(7)
	End Sub
	Private Sub btnRemoveProduct8_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnRemoveProduct8.Click
		btnRemoveProductClick(8)
	End Sub
	Private Sub btnRemoveProduct9_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnRemoveProduct9.Click
		btnRemoveProductClick(9)
	End Sub
	Private Sub btnRemoveProduct10_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnRemoveProduct10.Click
		btnRemoveProductClick(10)
	End Sub
	Private Sub btnRemoveProduct11_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnRemoveProduct11.Click
		btnRemoveProductClick(11)
	End Sub
	Private Sub btnRemoveProduct12_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnRemoveProduct12.Click
		btnRemoveProductClick(12)
	End Sub
	Private Sub btnRemoveProduct13_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnRemoveProduct13.Click
		btnRemoveProductClick(13)
	End Sub
	Private Sub btnRemoveProduct14_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnRemoveProduct14.Click
		btnRemoveProductClick(14)
	End Sub
	Private Sub btnRemoveProduct15_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnRemoveProduct15.Click
		btnRemoveProductClick(15)
	End Sub
	Private Sub btnRemoveProduct16_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnRemoveProduct16.Click
		btnRemoveProductClick(16)
	End Sub
	Private Sub btnRemoveProduct17_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnRemoveProduct17.Click
		btnRemoveProductClick(17)
	End Sub
	Private Sub btnRemoveProduct18_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnRemoveProduct18.Click
		btnRemoveProductClick(18)
	End Sub
	Private Sub btnRemoveProduct19_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnRemoveProduct19.Click
		btnRemoveProductClick(19)
	End Sub
	Private Sub btnRemoveProduct20_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnRemoveProduct20.Click
		btnRemoveProductClick(20)
	End Sub
	Private Sub btnRemoveProduct21_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnRemoveProduct21.Click
		btnRemoveProductClick(21)
	End Sub
	Private Sub btnRemoveProduct22_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnRemoveProduct22.Click
		btnRemoveProductClick(22)
	End Sub
	Private Sub btnRemoveProduct23_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnRemoveProduct23.Click
		btnRemoveProductClick(23)
	End Sub
	Private Sub btnRemoveProduct24_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnRemoveProduct24.Click
		btnRemoveProductClick(24)
	End Sub
	Private Sub btnRemoveProduct25_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnRemoveProduct25.Click
		btnRemoveProductClick(25)
	End Sub
	Private Sub btnRemoveProduct26_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnRemoveProduct26.Click
		btnRemoveProductClick(26)
	End Sub
	Private Sub btnRemoveProduct27_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnRemoveProduct27.Click
		btnRemoveProductClick(27)
	End Sub
	Private Sub btnRemoveProduct28_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnRemoveProduct28.Click
		btnRemoveProductClick(28)
	End Sub
	Private Sub btnRemoveProduct29_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnRemoveProduct29.Click
		btnRemoveProductClick(29)
	End Sub
	Private Sub btnRemoveProduct30_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnRemoveProduct30.Click
		btnRemoveProductClick(30)
	End Sub
	Private Sub btnRemoveProduct31_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnRemoveProduct31.Click
		btnRemoveProductClick(31)
	End Sub
	Private Sub btnRemoveProduct32_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnRemoveProduct32.Click
		btnRemoveProductClick(32)
	End Sub
#End Region

	Private Sub ShowProductControls(index As Integer)
		Dim product As DropDownList = FindControl(String.Format(DDL_PRODUCT, index))
		Dim delivered As Label = FindControl(String.Format(LBL_DELIV, index))
		Dim requested As TextBox = FindControl(String.Format(TBX_PRODUCT_AMOUNT, index))
		Dim unit As DropDownList = FindControl(String.Format(DDL_UNITS, index))
		Dim orderItemGrouping As DropDownList = FindControl(String.Format(DDL_ORDER_ITEM_GROUPING, index))
		Dim notes As TextBox = FindControl(String.Format(TBX_PRODUCT_NOTES, index))
		If product.SelectedIndex <= 0 OrElse Utilities.IsProductParameterlessFunction(Guid.Parse(product.SelectedValue), _currentUser.Id) Then
			delivered.Visible = False
			requested.Visible = False
			unit.Visible = False
			notes.Visible = False
			orderItemGrouping.Visible = False
		Else
			delivered.Visible = True
			requested.Visible = True
			unit.Visible = True
			notes.Visible = True
			orderItemGrouping.Enabled = _interfaceAllowsOrderItemGroupingChange
		End If
	End Sub

	Private Sub ddlProduct1_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlProduct1.SelectedIndexChanged, ddlProduct2.SelectedIndexChanged, ddlProduct3.SelectedIndexChanged, ddlProduct4.SelectedIndexChanged, ddlProduct5.SelectedIndexChanged, ddlProduct6.SelectedIndexChanged, ddlProduct7.SelectedIndexChanged, ddlProduct8.SelectedIndexChanged, ddlProduct9.SelectedIndexChanged, ddlProduct10.SelectedIndexChanged, ddlProduct11.SelectedIndexChanged, ddlProduct12.SelectedIndexChanged, ddlProduct13.SelectedIndexChanged, ddlProduct14.SelectedIndexChanged, ddlProduct15.SelectedIndexChanged, ddlProduct16.SelectedIndexChanged, ddlProduct17.SelectedIndexChanged, ddlProduct18.SelectedIndexChanged, ddlProduct19.SelectedIndexChanged, ddlProduct20.SelectedIndexChanged, ddlProduct21.SelectedIndexChanged, ddlProduct22.SelectedIndexChanged, ddlProduct23.SelectedIndexChanged, ddlProduct24.SelectedIndexChanged, ddlProduct25.SelectedIndexChanged, ddlProduct26.SelectedIndexChanged, ddlProduct27.SelectedIndexChanged, ddlProduct28.SelectedIndexChanged, ddlProduct29.SelectedIndexChanged, ddlProduct30.SelectedIndexChanged, ddlProduct31.SelectedIndexChanged, ddlProduct32.SelectedIndexChanged
		Dim ddl As DropDownList = sender
		Dim indexString As String = ddl.ID.Substring(10, ddl.ID.Length - 10)
		Dim index As Integer = Integer.Parse(indexString)
		PopulateUnitsCombo(index, KaUnit.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "abbreviation ASC"), Guid.Empty)
		SelectedDefaultUnit(index)
		PopulateOrderItemGroupCombo(index)
		ShowProductControls(index)
	End Sub

#End Region

#Region "Accounts"
	Private Sub PopulateAccountCombos()
		Dim customersTable As New DataTable
		Dim customersDA As New OleDbDataAdapter("SELECT id, name + CASE WHEN deleted = 1 OR disabled = 1 THEN ' * ' ELSE '' END AS name FROM customer_accounts " &
												"WHERE ((deleted = 0) AND (disabled = 0) AND (is_supplier = 0) AND (owner_id=" & Q(ddlOwners.SelectedValue.ToString) & " OR " &
												"owner_id=" & Q(Guid.Empty) & ")) OR (id IN (SELECT customer_account_id FROM order_customer_accounts " &
												"WHERE (deleted = 0) AND (order_id = " & Q(ddlOrders.SelectedValue) & ") AND (order_id <> " & Q(Guid.Empty) & "))) " &
												"ORDER BY name, id", GetUserConnection(_currentUser.Id))
		If Tm2Database.CommandTimeout > 0 Then customersDA.SelectCommand.CommandTimeout = Tm2Database.CommandTimeout
		customersDA.Fill(customersTable)
		Dim emptyCust As DataRow = customersTable.NewRow
		emptyCust.Item("id") = Guid.Empty
		emptyCust.Item("name") = "Select an account"
		customersTable.Rows.InsertAt(emptyCust, 0)
		For i As Integer = 1 To MAX_ACCOUNT_COUNT
			Dim ddl As System.Web.UI.WebControls.DropDownList = FindControl(String.Format(DDL_ACCOUNT, i))
			Dim selectedId As String = IIf(ddl.SelectedIndex >= 0, ddl.SelectedValue, Guid.Empty.ToString)
			ddl.SelectedIndex = -1 ' This resolves an error with "The 'SelectedIndex' and 'SelectedValue' attributes are mutually exclusive"
			ddl.DataTextField = "name"
			ddl.DataValueField = "id"
			ddl.DataSource = customersTable
			ddl.DataBind()

			Try
				ddl.SelectedValue = selectedId
			Catch ex As ArgumentOutOfRangeException
				'Suppress
			End Try
		Next

		UpdateTotalPercent()
	End Sub

	Private Sub PopulateExistingAccounts(ByVal orderId As Guid)
		Dim existingAccounts As ArrayList = KaOrderCustomerAccount.GetAll(GetUserConnection(_currentUser.Id), "order_id = " & Q(orderId) & " and deleted <> 'True'", "")

		PopulateExistingAccounts(existingAccounts)
	End Sub

	Private Sub PopulateExistingAccounts(ByVal existingAccounts As ArrayList)
		PopulateAccountCombos()

		ResetAccounts()
		If existingAccounts.Count = 0 Then
			Dim orderAccount As KaOrderCustomerAccount = New KaOrderCustomerAccount()
			orderAccount.Percentage = 100
			existingAccounts.Add(orderAccount)
		End If
		Dim index As Integer = 1
		For Each orderAccount As KaOrderCustomerAccount In existingAccounts
			Dim ddlAccountName = "ddlAccount" & index
			Dim tbxPercentName = "tbxPercent" & index
			Dim pnlAccountName = "pnlAccount" & index
			Dim ddl As System.Web.UI.WebControls.DropDownList = FindControl(ddlAccountName)
			Dim tbx As System.Web.UI.WebControls.TextBox = FindControl(tbxPercentName)
			Dim pnl As HtmlGenericControl = FindControl(pnlAccountName)

			Dim indexOfAcc = IndexOfAccount(ddl, orderAccount.CustomerAccountId)
			ddl.SelectedIndex = indexOfAcc
			tbx.Text = orderAccount.Percentage
			pnl.Visible = True
			index += 1
		Next
		UpdateTotalPercent()
	End Sub

	Private Function IndexOfAccount(ByVal ddl As System.Web.UI.WebControls.DropDownList, ByVal accountId As Guid) As Integer
		For i = 0 To ddl.Items.Count - 1
			Dim li As ListItem = ddl.Items(i)
			If li.Text <> "Select an Account" Then
				If Guid.Parse(li.Value) = accountId Then
					Return i
				End If
			End If
		Next
		Return -1
	End Function

	Private Sub ResetAccounts()
		ddlAccount1.SelectedIndex = -1
		tbxPercent1.Text = "100"
		If ddlOrders.SelectedIndex <= 0 Then
			ddlAccount1.Items.Clear()
		End If
		For i = 2 To MAX_ACCOUNT_COUNT
			RemoveAccountPanel(i)
		Next
	End Sub

	Private Function btnAddAccount_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) As Integer Handles btnAddAccount.Click
		For i = 1 To MAX_ACCOUNT_COUNT
			Dim pnl As HtmlGenericControl = FindControl(String.Format(PNL_ACCOUNT, i))
			If pnl.Visible = False Then
				pnl.Visible = True
				PopulateAccountCombos()
				Return i
			End If
		Next
		UpdateTotalPercent()
		Return -1
	End Function

	Private Sub RemoveAccountPanel(ByVal panelNumb As Integer)
		Dim ddl As System.Web.UI.WebControls.DropDownList = FindControl(String.Format(DDL_ACCOUNT, panelNumb))
		ddl.SelectedIndex = -1
		If ddlOwners.SelectedIndex <= 0 Then
			ddl.Items.Clear()
		End If
		Dim tbx As System.Web.UI.WebControls.TextBox = FindControl(String.Format(TBX_ACCOUNT_PERCENT, panelNumb))
		tbx.Text = "0"
		Dim pnl As HtmlGenericControl = FindControl(String.Format(PNL_ACCOUNT, panelNumb))
		pnl.Visible = False

		PopulateAccountCombos()
	End Sub

	Public Function UpdateTotalPercent() As Boolean
		Dim total As Decimal = 0
		Dim numberOfAccounts As Integer = 0
		For i = 1 To MAX_ACCOUNT_COUNT
			Dim pnl As HtmlGenericControl = FindControl(String.Format(PNL_ACCOUNT, i))
			Dim ddl As System.Web.UI.WebControls.DropDownList = FindControl(String.Format(DDL_ACCOUNT, i))
			If pnl.Visible Then numberOfAccounts += 1
			If pnl.Visible And ddl.SelectedIndex > 0 Then
				Dim tbx As System.Web.UI.WebControls.TextBox = FindControl(String.Format(TBX_ACCOUNT_PERCENT, i))
				Dim percent As Double = 0.0
				If Not Double.TryParse(tbx.Text, percent) Then
					DisplayJavaScriptMessage("InvalidAcctPercentage", Utilities.JsAlert("The account percentage for " & CType(FindControl(String.Format(DDL_ACCOUNT, i)), DropDownList).SelectedItem.Text & " is not a valid percentage.  Please enter a valid percentage."))
					Return False
				End If
				total += percent
			End If
		Next
		tbxTotalPercent.Text = total
		btnAddAccount.Visible = _currentUserPermission(_currentTableName).Edit AndAlso _interfaceAllowsCustomerChange AndAlso (numberOfAccounts < MAX_ACCOUNT_COUNT)
		Return True
	End Function

	Private Sub btnUpdatePercent_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnUpdatePercent.Click
		UpdateTotalPercent()
	End Sub

	Private Sub AddCoupledAccount(ByVal accountIndex As Integer)
		If cbxAccountCoupling.Checked Then
			Dim ddlAccountName As String = "ddlAccount" & accountIndex
			Dim ddl As System.Web.UI.WebControls.DropDownList = FindControl(ddlAccountName)
			If ddl.SelectedIndex > 0 Then
				'check if the selected account is coupled
				Dim li As ListItem = ddl.SelectedItem
				Dim accountId As Guid = Guid.Parse(li.Value)
				Dim coupledAccounts As ArrayList = KaCustomerAccountCombination.GetAll(GetUserConnection(_currentUser.Id), "customer_account_id = " & Q(accountId) & " and deleted <> 'True'", "")
				If coupledAccounts.Count > 0 Then
					'Get all of the accounts for this association
					Dim coupledAccount As KaCustomerAccountCombination = coupledAccounts.Item(0)
					Dim allAccounts As ArrayList = KaCustomerAccountCombination.GetAll(GetUserConnection(_currentUser.Id), "combination_id = " & Q(coupledAccount.CombinationId), "")
					For Each ca As KaCustomerAccountCombination In allAccounts
						If ca.CustomerAccountId <> accountId Then
							'Skip the original account, it has already been added
							Dim nextIndex As Integer = btnAddAccount_Click(vbNull, System.EventArgs.Empty)
							ddlAccountName = "ddlAccount" & nextIndex
							ddl = FindControl(ddlAccountName)
							Dim listIndex As Integer = IndexOfAccount(ddl, ca.CustomerAccountId)
							If listIndex <> -1 Then
								Dim account As KaCustomerAccount = New KaCustomerAccount(GetUserConnection(_currentUser.Id), ca.CustomerAccountId)
								ddl.Items.RemoveAt(listIndex)
								ddl.Items.Insert(listIndex, New ListItem(account.Name, account.Id.ToString))
								ddl.SelectedIndex = listIndex

								Dim tbxName As String = "tbxPercent" & nextIndex
								Dim tbx As System.Web.UI.WebControls.TextBox = FindControl(tbxName)
								tbx.Text = ca.Percentage
							End If
						Else
							'Just need to set the percentage here
							Dim tbxName As String = "tbxPercent" & accountIndex
							Dim tbx As System.Web.UI.WebControls.TextBox = FindControl(tbxName)
							tbx.Text = ca.Percentage
						End If
					Next
				End If
			End If
		End If
		UpdateTotalPercent()
	End Sub

#Region "Remove Account Buttons"
	Private Sub btnRemoveAccountClick(ByVal productPanelIndex As Integer)
		RemoveAccountPanel(productPanelIndex)
		Dim orderAccounts As New ArrayList
		For Each account As Dictionary(Of OrderAccountField, String) In GetListOfAccounts()
			Dim orderAccount As KaOrderCustomerAccount
			Dim customerAccountId As Guid = Guid.Empty
			Guid.TryParse(account(OrderAccountField.CustomerAccountId), customerAccountId)
			Dim percentage As Double = 100
			Double.TryParse(account(OrderAccountField.Percent), percentage)
			If orderAccounts.Count > 0 Then ' check to see if we can update the order record
				If orderAccounts(0).CustomerAccountId = customerAccountId Then ' this order account is for the same account
					orderAccount = orderAccounts(0) ' we'll update this order account record
					orderAccounts.RemoveAt(0) ' remove it from the list so that we don't use it again
				Else ' the order account doesn't match, create a new one
					orderAccount = New KaOrderCustomerAccount()
					orderAccount.CustomerAccountId = customerAccountId
					orderAccounts.Add(orderAccounts(0)) ' move this record to the end of the list
					orderAccounts.RemoveAt(0)
				End If
			Else ' no order accounts available to update, create a new order account
				orderAccount = New KaOrderCustomerAccount()
				orderAccount.CustomerAccountId = customerAccountId
			End If
			orderAccount.Percentage = percentage
			orderAccounts.Add(orderAccount)
		Next

		PopulateExistingAccounts(orderAccounts)
	End Sub

	Private Sub btnRemoveAccount1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRemoveAccount1.Click
		btnRemoveAccountClick(1)
	End Sub

	Private Sub btnRemoveAccount2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRemoveAccount2.Click
		btnRemoveAccountClick(2)
	End Sub

	Private Sub btnRemoveAccount3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRemoveAccount3.Click
		btnRemoveAccountClick(3)
	End Sub

	Private Sub btnRemoveAccount4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRemoveAccount4.Click
		btnRemoveAccountClick(4)
	End Sub

	Private Sub btnRemoveAccount5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRemoveAccount5.Click
		btnRemoveAccountClick(5)
	End Sub

	Private Sub btnRemoveAccount6_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRemoveAccount6.Click
		btnRemoveAccountClick(6)
	End Sub

	Private Sub btnRemoveAccount7_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRemoveAccount7.Click
		btnRemoveAccountClick(7)
	End Sub

	Private Sub btnRemoveAccount8_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRemoveAccount8.Click
		btnRemoveAccountClick(8)
	End Sub

	Private Sub btnRemoveAccount9_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRemoveAccount9.Click
		btnRemoveAccountClick(9)
	End Sub

	Private Sub btnRemoveAccount10_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRemoveAccount10.Click
		btnRemoveAccountClick(10)
	End Sub

	Private Sub btnRemoveAccount11_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRemoveAccount11.Click
		btnRemoveAccountClick(11)
	End Sub

	Private Sub btnRemoveAccount12_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRemoveAccount12.Click
		btnRemoveAccountClick(12)
	End Sub

#End Region

#Region "ddlAccount Actions"

	Protected Sub ddlAccount1_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlAccount1.SelectedIndexChanged
		AddCoupledAccount(1)
		PopulateOrderDestination()
	End Sub

	Protected Sub ddlAccount2_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlAccount2.SelectedIndexChanged
		AddCoupledAccount(2)
		PopulateOrderDestination()
	End Sub

	Protected Sub ddlAccount3_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlAccount3.SelectedIndexChanged
		AddCoupledAccount(3)
		PopulateOrderDestination()
	End Sub

	Protected Sub ddlAccount4_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlAccount4.SelectedIndexChanged
		AddCoupledAccount(4)
		PopulateOrderDestination()
	End Sub

	Protected Sub ddlAccount5_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlAccount5.SelectedIndexChanged
		AddCoupledAccount(5)
		PopulateOrderDestination()
	End Sub

	Protected Sub ddlAccount6_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlAccount6.SelectedIndexChanged
		AddCoupledAccount(6)
		PopulateOrderDestination()
	End Sub

	Protected Sub ddlAccount7_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlAccount7.SelectedIndexChanged
		AddCoupledAccount(7)
		PopulateOrderDestination()
	End Sub

	Protected Sub ddlAccount8_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlAccount8.SelectedIndexChanged
		AddCoupledAccount(8)
		PopulateOrderDestination()
	End Sub

	Protected Sub ddlAccount9_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlAccount9.SelectedIndexChanged
		AddCoupledAccount(9)
		PopulateOrderDestination()
	End Sub

	Protected Sub ddlAccount10_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlAccount10.SelectedIndexChanged
		AddCoupledAccount(10)
		PopulateOrderDestination()
	End Sub

	Protected Sub ddlAccount11_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlAccount11.SelectedIndexChanged
		AddCoupledAccount(11)
		PopulateOrderDestination()
	End Sub

	Protected Sub ddlAccount12_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlAccount12.SelectedIndexChanged
		AddCoupledAccount(12)
		PopulateOrderDestination()
	End Sub

#End Region

#End Region

#Region " Order Item Groups "
	Private Sub PopulateOrderItemGroupTable()
		_orderItemGroupingTable = New DataTable()
		_orderItemGroupingTable.Columns.Add(New DataColumn("id", GetType(System.Guid)))
		_orderItemGroupingTable.Columns.Add(New DataColumn("name", GetType(System.String)))

		For Each orderItemGroup As KaOrderItemGroup In _orderItemGroupings
			Dim newRow As DataRow = _orderItemGroupingTable.NewRow
			newRow.Item("id") = orderItemGroup.Id
			newRow.Item("name") = orderItemGroup.Name
			_orderItemGroupingTable.Rows.Add(newRow)
		Next
		If _orderItemGroupingTable.Rows.Count > 1 Then
			pnlGroupingHeader.InnerText = "Grouping"
		Else
			pnlGroupingHeader.InnerText = ""
		End If
	End Sub
#End Region

	Protected Sub ShipToChanged(sender As Object, e As EventArgs) Handles ddlCustomerSite.SelectedIndexChanged, tbxShipTo.TextChanged
		If tbxShipToName.Text.Trim().Length > 0 OrElse
				   tbxShipToStreet.Text.Trim().Length > 0 OrElse
				   tbxShipToCity.Text.Trim().Length > 0 OrElse
				   tbxShipToState.Text.Trim().Length > 0 OrElse
				   tbxShipToZip.Text.Trim().Length > 0 OrElse
				   tbxShipToCountry.Text.Trim().Length > 0 Then
			pnlCustomerSite.Visible = False
			pnlShipToInfo.Visible = True
		Else
			pnlCustomerSite.Visible = ddlCustomerSite.Items.Count > 1
			pnlShipToInfo.Visible = (ddlCustomerSite.SelectedIndex = 0)
		End If
	End Sub

	Private Sub PopulateOrderDestination()
		Dim selectedAcctLocId As Guid = Guid.Empty
		Guid.TryParse(ddlCustomerSite.SelectedValue, selectedAcctLocId)

		PopulateOrderDestination(selectedAcctLocId)
	End Sub

	Private Sub PopulateOrderDestination(selectedId As Guid)
		ddlCustomerSite.Items.Clear()
		ddlCustomerSite.Items.Add(New ListItem("Select a location", Guid.Empty.ToString()))
		Dim conditions As String = Q(Guid.Empty)
		For Each account As Dictionary(Of OrderAccountField, String) In GetListOfAccounts()
			conditions &= "," & Q(account(OrderAccountField.CustomerAccountId))
		Next
		If conditions.Length > 0 Then
			conditions = String.Format("customer_account_id IN ({0}) AND (deleted=0 OR id={1})", conditions, Q(selectedId))
			For Each customerLocInfo As KaCustomerAccountLocation In KaCustomerAccountLocation.GetAll(GetUserConnection(_currentUser.Id), conditions, "UPPER(name) ASC")
				ddlCustomerSite.Items.Add(New ListItem(customerLocInfo.Name, customerLocInfo.Id.ToString))
			Next
		End If
		Try ' to select the previously selected customer site...
			ddlCustomerSite.SelectedValue = selectedId.ToString()
		Catch ex As Exception ' the previously selected customer site is not in the list...
			ddlCustomerSite.SelectedIndex = 0
		End Try
		ShipToChanged(ddlCustomerSite, New EventArgs)
	End Sub

	Private Sub SetTextboxMaxLengths()
		tbxAcres.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaOrder.TABLE_NAME, KaOrder.FN_ACRES))
		tbxDeliveredBatches.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaOrder.TABLE_NAME, KaOrder.FN_DELIVERED_BATCHES))
		'  tbxFind.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaOrder.TABLE_NAME, ))
		tbxNotes.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaOrder.TABLE_NAME, KaOrder.FN_NOTES))
		tbxInternalNotes.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaOrder.TABLE_NAME, KaOrder.FN_INTERNAL_NOTES))
		' tbxNumOfCopys.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaOrder.TABLE_NAME, ))
		tbxOrderNum.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaOrder.TABLE_NAME, KaOrder.FN_NUMBER))
		tbxOverun.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaOrder.TABLE_NAME, KaOrder.FN_OVER_SCALE_PERCENT))
		tbxPurchaseOrderNum.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaOrder.TABLE_NAME, KaOrder.FN_PURCHASE_ORDER))
		tbxReleaseNumber.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaOrder.TABLE_NAME, KaOrder.FN_RELEASE_NUMBER))
		tbxRequestedBatches.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaOrder.TABLE_NAME, KaOrder.FN_REQUESTED_BATCHES))
		'  tbxShipTo.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaOrder.TABLE_NAME, ))
		tbxShipToCity.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaOrder.TABLE_NAME, KaOrder.FN_SHIP_TO_CITY))
		tbxShipToName.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaOrder.TABLE_NAME, KaOrder.FN_SHIP_TO_NAME))
		tbxShipToState.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaOrder.TABLE_NAME, KaOrder.FN_SHIP_TO_STATE))
		tbxShipToStreet.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaOrder.TABLE_NAME, KaOrder.FN_SHIP_TO_STREET))
		tbxShipToZip.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaOrder.TABLE_NAME, KaOrder.FN_SHIP_TO_ZIP_CODE))
		tbxShipToCountry.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaOrder.TABLE_NAME, KaOrder.FN_SHIP_TO_COUNTRY))
		tbxStartNumber.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaOrder.TABLE_NAME, KaOrder.FN_NUMBER))
		'   tbxTotalPercent.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaOrder.TABLE_NAME, ))
	End Sub

	Protected Sub btnSortUsingProductPriority_Click(sender As Object, e As EventArgs) Handles btnSortUsingProductPriority.Click
		Dim order As New KaOrder
		order.OrderItems.Clear() ' start with a clear list
		Dim position As Integer = 0 ' keep track of the position of each order item
		For Each item As Dictionary(Of OrderItemField, String) In GetListOfItems(Nothing)
			Dim orderItem As New KaOrderItem
			Dim orderItemId As Guid = Guid.Parse(item(OrderItemField.OrderItemId))
			Dim productId As Guid = Guid.Parse(item(OrderItemField.ProductId))
			Dim request As Double = 0
			Double.TryParse(item(OrderItemField.Request), request)
			Dim delivered As Double = Double.Parse(item(OrderItemField.Delivered))
			Dim unitId As Guid = Guid.Parse(item(OrderItemField.UnitId))
			Dim notes As String = item(OrderItemField.Notes)
			orderItem.Id = orderItemId
			orderItem.ProductId = productId
			orderItem.Request = request
			orderItem.Delivered = delivered
			orderItem.UnitId = unitId
			orderItem.Position = position
			orderItem.Notes = notes
			orderItem.OrderItemGroupId = Guid.Parse(item(OrderItemField.OrderItemGroupingId))
			position += 1
			order.OrderItems.Add(orderItem)
		Next

		Try
			order.InterfaceId = New KaOrder(GetUserConnection(_currentUser.Id), Guid.Parse(ddlOrders.SelectedValue)).InterfaceId
		Catch ex As Exception

		End Try
		order.SortOrderItemsUsingProductPriority(GetUserConnection(_currentUser.Id), Nothing)

		Dim itemList As New SortedList(Of Integer, KaOrderItem)
		For Each orderItem As KaOrderItem In order.OrderItems
			itemList.Add(orderItem.Position, orderItem)
		Next
		Dim existingProducts As New ArrayList
		For n As Integer = 0 To itemList.Count - 1
			existingProducts.Add(itemList(n))
		Next
		PopulateExistingProducts(existingProducts)
	End Sub

	Private Sub SetOrderItemMoveButtonVisibility()
		Dim orderItemCount As Integer = 1 ' determine the number of order items
		For i As Integer = 1 To MAX_PRODUCT_COUNT - 1
			If FindControl(String.Format(PNL_PRODUCT, i)).Visible Then
				orderItemCount += 1
			Else ' no more order items
				Exit For
			End If
		Next
		For i As Integer = 1 To orderItemCount
			' should we display the down arrow for this item?
			If i < orderItemCount Then ' this isn't the last product; determine if we should show the down arrow
				Dim moveDown As ImageButton = FindControl(String.Format(BTN_MOVE_PRODUCT_DOWN, i))
				Dim thisOrderItem As DropDownList = FindControl(String.Format(DDL_PRODUCT, i))
				Dim nextOrderItem As DropDownList = FindControl(String.Format(DDL_PRODUCT, i + 1))
				moveDown.Visible = thisOrderItem.SelectedIndex > 0 AndAlso i < MAX_PRODUCT_COUNT AndAlso nextOrderItem.SelectedIndex > 0 AndAlso ddlOwners.SelectedValue <> Guid.Empty.ToString()
			ElseIf i < MAX_PRODUCT_COUNT Then ' this is the last order item (but not the max); it can't be moved down
				FindControl(String.Format(BTN_MOVE_PRODUCT_DOWN, i)).Visible = False
			End If
			' should we display the up arrow for this item?
			If i > 1 Then ' this isn't the first product; determine if we should show the up arrow
				Dim moveUp As ImageButton = FindControl(String.Format(BTN_MOVE_PRODUCT_UP, i))
				Dim thisOrderItem As DropDownList = FindControl(String.Format(DDL_PRODUCT, i))
				Dim lastOrderItem As DropDownList = FindControl(String.Format(DDL_PRODUCT, i - 1))
				moveUp.Visible = thisOrderItem.SelectedIndex > 0 AndAlso lastOrderItem.SelectedIndex > 0 AndAlso ddlOwners.SelectedValue <> Guid.Empty.ToString()
			End If
		Next
	End Sub

	Protected Sub btnMoveProductUpClick(sender As Object, e As System.Web.UI.ImageClickEventArgs) Handles btnMoveProductUp2.Click, btnMoveProductUp3.Click, btnMoveProductUp4.Click, btnMoveProductUp5.Click, btnMoveProductUp6.Click, btnMoveProductUp7.Click, btnMoveProductUp8.Click, btnMoveProductUp9.Click, btnMoveProductUp10.Click, btnMoveProductUp11.Click, btnMoveProductUp12.Click, btnMoveProductUp13.Click, btnMoveProductUp14.Click, btnMoveProductUp15.Click, btnMoveProductUp16.Click, btnMoveProductUp17.Click, btnMoveProductUp18.Click, btnMoveProductUp19.Click, btnMoveProductUp20.Click, btnMoveProductUp21.Click, btnMoveProductUp22.Click, btnMoveProductUp23.Click, btnMoveProductUp24.Click, btnMoveProductUp25.Click, btnMoveProductUp26.Click, btnMoveProductUp27.Click, btnMoveProductUp28.Click, btnMoveProductUp29.Click, btnMoveProductUp30.Click, btnMoveProductUp31.Click, btnMoveProductUp32.Click
		Dim index As Integer = Integer.Parse(CType(sender, ImageButton).ID.Replace("btnMoveProductUp", ""))
		Dim order As New KaOrder
		order.OrderItems.Clear() ' start with a clear list
		Dim position As Integer = 0 ' keep track of the position of each order item
		For Each item As Dictionary(Of OrderItemField, String) In GetListOfItems(Nothing)
			Dim orderItem As New KaOrderItem
			Dim orderItemId As Guid = Guid.Parse(item(OrderItemField.OrderItemId))
			Dim productId As Guid = Guid.Parse(item(OrderItemField.ProductId))
			Dim request As Double = 0
			Double.TryParse(item(OrderItemField.Request), request)
			Dim delivered As Double = Double.Parse(item(OrderItemField.Delivered))
			Dim unitId As Guid = Guid.Parse(item(OrderItemField.UnitId))
			Dim notes As String = item(OrderItemField.Notes)
			orderItem.Id = orderItemId
			orderItem.ProductId = productId
			orderItem.Request = request
			orderItem.Delivered = delivered
			orderItem.UnitId = unitId
			orderItem.Position = position
			orderItem.Notes = notes
			orderItem.OrderItemGroupId = Guid.Parse(item(OrderItemField.OrderItemGroupingId))
			position += 1
			order.OrderItems.Add(orderItem)
		Next
		If index > 1 Then
			order.OrderItems(index - 2).Position = index - 1
			order.OrderItems(index - 1).Position = index - 2
		End If
		Dim itemList As New SortedList(Of Integer, KaOrderItem)
		For Each orderItem As KaOrderItem In order.OrderItems
			itemList.Add(orderItem.Position, orderItem)
		Next
		Dim existingProducts As New ArrayList
		For n As Integer = 0 To itemList.Count - 1
			existingProducts.Add(itemList(n))
		Next
		PopulateExistingProducts(existingProducts)
	End Sub

	Protected Sub btnMoveProductDownClick(sender As Object, e As System.Web.UI.ImageClickEventArgs) Handles btnMoveProductDown1.Click, btnMoveProductDown2.Click, btnMoveProductDown3.Click, btnMoveProductDown4.Click, btnMoveProductDown5.Click, btnMoveProductDown6.Click, btnMoveProductDown7.Click, btnMoveProductDown8.Click, btnMoveProductDown9.Click, btnMoveProductDown10.Click, btnMoveProductDown11.Click, btnMoveProductDown12.Click, btnMoveProductDown13.Click, btnMoveProductDown14.Click, btnMoveProductDown15.Click, btnMoveProductDown16.Click, btnMoveProductDown17.Click, btnMoveProductDown18.Click, btnMoveProductDown19.Click, btnMoveProductDown20.Click, btnMoveProductDown21.Click, btnMoveProductDown22.Click, btnMoveProductDown23.Click, btnMoveProductDown24.Click, btnMoveProductDown25.Click, btnMoveProductDown26.Click, btnMoveProductDown27.Click, btnMoveProductDown28.Click, btnMoveProductDown29.Click, btnMoveProductDown30.Click, btnMoveProductDown31.Click
		Dim index As Integer = Integer.Parse(CType(sender, ImageButton).ID.Replace("btnMoveProductDown", ""))
		Dim order As New KaOrder
		order.OrderItems.Clear() ' start with a clear list
		Dim position As Integer = 0 ' keep track of the position of each order item
		For Each item As Dictionary(Of OrderItemField, String) In GetListOfItems(Nothing)
			Dim orderItem As New KaOrderItem
			Dim orderItemId As Guid = Guid.Parse(item(OrderItemField.OrderItemId))
			Dim productId As Guid = Guid.Parse(item(OrderItemField.ProductId))
			Dim request As Double = 0
			Double.TryParse(item(OrderItemField.Request), request)
			Dim delivered As Double = Double.Parse(item(OrderItemField.Delivered))
			Dim unitId As Guid = Guid.Parse(item(OrderItemField.UnitId))
			Dim notes As String = item(OrderItemField.Notes)
			Dim orderItemGroup As String =
			orderItem.Id = orderItemId
			orderItem.ProductId = productId
			orderItem.Request = request
			orderItem.Delivered = delivered
			orderItem.UnitId = unitId
			orderItem.Position = position
			orderItem.Notes = notes
			orderItem.OrderItemGroupId = Guid.Parse(item(OrderItemField.OrderItemGroupingId))
			position += 1
			order.OrderItems.Add(orderItem)
		Next
		If index < order.OrderItems.Count Then
			order.OrderItems(index).Position = index - 1
			order.OrderItems(index - 1).Position = index
		End If
		Dim itemList As New SortedList(Of Integer, KaOrderItem)
		For Each orderItem As KaOrderItem In order.OrderItems
			itemList.Add(orderItem.Position, orderItem)
		Next
		Dim existingProducts As New ArrayList
		For n As Integer = 0 To itemList.Count - 1
			existingProducts.Add(itemList(n))
		Next
		PopulateExistingProducts(existingProducts)
	End Sub

	Private Sub Page_PreRender(sender As Object, e As System.EventArgs) Handles Me.PreRender
		SetOrderItemMoveButtonVisibility()
	End Sub

	Protected Sub ddlFacilityFilter_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlFacilityFilter.SelectedIndexChanged
		Dim currentOrderId As String = ddlOrders.SelectedValue
		PopulateOrdersList()
		Try
			ddlOrders.SelectedValue = currentOrderId
		Catch ex As ArgumentOutOfRangeException

		End Try
		ddlOrders_SelectedIndexChanged(ddlOrders, New EventArgs)
		KaSetting.WriteSetting(GetUserConnection(_currentUser.Id), "OrdersPage:" & _currentUser.Id.ToString & "/LastFacilityUsed", ddlFacilityFilter.SelectedValue)
	End Sub

	Protected Overrides Function SaveViewState() As Object
		Dim viewState(3) As Object
		'Saving the grid values to the View State
		Utilities.ConvertCustomFieldPanelToLists(_customFields, _customFieldData, lstCustomFields)
		viewState(0) = MyBase.SaveViewState()
		viewState(1) = _customFields
		viewState(2) = _customFieldData
		viewState(3) = _orderItemGroupings
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
			_orderItemGroupings = viewState(3)
			PopulateOrderItemGroupTable()
			Utilities.CreateDynamicCustomFieldPanelControls(_customFields, _customFieldData, lstCustomFields, Page)
		Else
			MyBase.LoadViewState(savedState)
		End If
	End Sub

	Private Sub AddOrderStatusDetails(ByVal message As String)
		If message.Length > 0 AndAlso Not lblOrderDetailStatus.Text.Contains(message) Then
			If lblOrderDetailStatus.Text.Trim.Length > 0 Then lblOrderDetailStatus.Text &= "<br />"
			lblOrderDetailStatus.Text &= message
		End If
		pnlOrderDetailStatus.Visible = (lblOrderDetailStatus.Text.Trim.Length > 0)
	End Sub

	Private Sub btnClearLockedStatus_Click(sender As Object, e As System.EventArgs) Handles btnClearLockedStatus.Click
		Try
			Dim orderId As Guid = Guid.Empty
			Guid.TryParse(ddlOrders.SelectedValue, orderId)
			Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
			Dim order As New KaOrder(connection, orderId)
			Tm2Database.ExecuteNonQuery(connection, "UPDATE orders SET locked = 0 WHERE (id = " & Q(orderId) & ")")
		Catch ex As RecordNotFoundException

		End Try
		ddlOrders_SelectedIndexChanged(ddlOrders, New EventArgs())
	End Sub

	Private Sub btnSetLockedStatus_Click(sender As Object, e As System.EventArgs) Handles btnSetLockedStatus.Click
		Try
			Dim orderId As Guid = Guid.Empty
			Guid.TryParse(ddlOrders.SelectedValue, orderId)
			Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
			Dim order As New KaOrder(connection, orderId)
			Tm2Database.ExecuteNonQuery(connection, "UPDATE orders SET locked = 1 WHERE (id = " & Q(orderId) & ")")
		Catch ex As RecordNotFoundException

		End Try
		ddlOrders_SelectedIndexChanged(ddlOrders, New EventArgs())
	End Sub

	Protected Sub btnAddOrderItemGrouping_Click(sender As Object, e As EventArgs) Handles btnAddOrderItemGrouping.Click
		If tbxOrderItemGroupingName.Text.Trim.Length > 0 Then
			Dim newGrouping As New KaOrderItemGroup
			With newGrouping
				.Id = Guid.NewGuid()
				.Name = tbxOrderItemGroupingName.Text.Trim
				.InterfaceBlendGroupData = ""
			End With

			_orderItemGroupings.Add(newGrouping)
			tbxOrderItemGroupingName.Text = ""

			PopulateOrderItemGroupTable()
			For index As Integer = 1 To MAX_PRODUCT_COUNT
				PopulateOrderItemGroupCombo(index)
			Next
		End If
	End Sub

	Private Sub cbxDoNotBlend_CheckedChanged(sender As Object, e As System.EventArgs) Handles cbxDoNotBlend.CheckedChanged
		pnlAddOrderItemGrouping.Visible = _interfaceAllowsOrderItemGroupingChange  'Does this interface allow?
		pnlAddOrderItemGrouping.Visible = cbxDoNotBlend.Checked 'Is the do not blend checked?
	End Sub

	Private Sub SetControlUsabilityFromPermissions()
		With _currentUserPermission(_currentTableName)
			Dim shouldEnable = (.Edit AndAlso ddlOrders.SelectedIndex > 0) OrElse (.Create AndAlso ddlOrders.SelectedIndex = 0)
			pnlAll.Enabled = shouldEnable
			btnSave.Enabled = shouldEnable
			btnSaveNew.Enabled = shouldEnable
			Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
			Dim orderInfo As KaOrder
			Try
				Dim value = Guid.Empty
				Guid.TryParse(ddlOrders.SelectedValue, value)
				orderInfo = New KaOrder(connection, value)
				btnMarkComplete.Enabled = .Edit AndAlso Not orderInfo.Locked AndAlso Not orderInfo.IsOrderStaged(connection, Nothing) AndAlso Not orderInfo.OrderReferencedInOtherProgressRecords(connection, Nothing, Guid.Empty)
				btnCopy.Enabled = .Edit
				btnPrintOrder.Enabled = True
				btnPointOfSale.Enabled = .Edit
				'        btnDelete.Enabled = _userAutorizedChangeOrder ' this will be handled by the number of tickets
				btnClearLockedStatus.Enabled = .Edit
				btnSetLockedStatus.Enabled = .Edit
				btnDelete.Enabled = .Edit AndAlso .Delete AndAlso orderInfo.Id <> Guid.Empty AndAlso Not orderInfo.Locked AndAlso Not orderInfo.IsOrderStaged(connection, Nothing) AndAlso Not orderInfo.OrderReferencedInOtherProgressRecords(connection, Nothing, Guid.Empty) AndAlso Not orderInfo.HasTickets(connection, Nothing)
			Catch ex As RecordNotFoundException
				orderInfo = New KaOrder
				btnCopy.Enabled = False
				btnPrintOrder.Enabled = False
				btnMarkComplete.Enabled = False
				btnPointOfSale.Enabled = False
				btnDelete.Enabled = False
				btnClearLockedStatus.Enabled = False
				btnSetLockedStatus.Enabled = False
			End Try

			For i As Integer = 1 To MAX_PRODUCT_COUNT
				Dim orderItem As DropDownList = FindControl(String.Format(DDL_PRODUCT, i))
				orderItem.Enabled = .Edit AndAlso _interfaceAllowsProductChange
				Dim orderRemoveItem As Button = FindControl(String.Format(BTN_REMOVE_PRODUCT, i))
				orderRemoveItem.Visible = .Edit AndAlso _interfaceAllowsProductChange
			Next
			For i As Integer = 1 To MAX_ACCOUNT_COUNT
				Dim orderAccount As DropDownList = FindControl(String.Format(DDL_ACCOUNT, i))
				orderAccount.Enabled = .Edit AndAlso _interfaceAllowsCustomerChange
				Dim orderRemoveAccount As Button = FindControl(String.Format(BTN_REMOVE_ACCOUNT, i))
				orderRemoveAccount.Visible = .Edit AndAlso _interfaceAllowsCustomerChange
			Next
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
