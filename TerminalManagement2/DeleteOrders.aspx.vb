Imports KahlerAutomation.KaTm2Database
Imports System.Environment
Imports System.Data.OleDb

Public Class DeleteOrders
	Inherits System.Web.UI.Page

	Private _currentUser As KaUser
	Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
	Private _currentTableName As String = KaOrder.TABLE_NAME
	Private _customFields As New List(Of KaCustomField)
	Private _customFieldData As New List(Of KaCustomFieldData)
	Private _checkedOrders As New Dictionary(Of Guid, OrderStatus)
	Private _stagedOrders As New List(Of String)
	Private _ordersWithTickets As New List(Of String)
	Private _ordersWithProgressRecords As New List(Of String)
	Private _ordersLocked As New List(Of String)
	Private _reportDataList As List(Of OrderDetailsRowInfo)

	Private Structure OrderStatus
		Public OrderInfo As KaOrder
		Public IsStaged As Boolean
		Public HasTickets As Boolean
		Public HasProgressRecords As Boolean
		Public IsLocked As Boolean
	End Structure

#Region "Events"
	Protected Sub Page_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Init
		_currentUser = Utilities.GetUser(Me)
	End Sub

	Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
		Response.Cache.SetCacheability(HttpCacheability.NoCache)
		_currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Orders")
		If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")
		btnDelete.Enabled = _currentUserPermission(_currentTableName).Delete
		btnComplete.Enabled = _currentUserPermission(_currentTableName).Edit
		lblStatus.Text = ""
		If Not Page.IsPostBack Then
			PopulateCustomerAccounts()
			PopulateOwnerList()
			PopulateFacilityList()
			Dim orderWarnings As List(Of String) = New List(Of String)
			If Request.QueryString("staged") IsNot Nothing Then
				Dim stagedOrders As String = Request.QueryString("staged")
				orderWarnings.Add("The following orders are staged and could not be marked as complete: " & stagedOrders)
			End If
			If Request.QueryString("tickets") IsNot Nothing Then
				Dim ordersWithTickets As String = Request.QueryString("tickets")
				orderWarnings.Add("The following orders have tickets in history and could not be deleted, but were marked as complete: " & ordersWithTickets)
			End If
			If Request.QueryString("inProgress") IsNot Nothing Then
				Dim ordersWithTickets As String = Request.QueryString("inProgress")
				orderWarnings.Add("The following orders are in progress and could not be marked as complete: " & ordersWithTickets)
			End If
			If Request.QueryString("locked") IsNot Nothing Then
				Dim ordersWithTickets As String = Request.QueryString("locked")
				orderWarnings.Add("The following orders are locked and could not be marked as complete: " & ordersWithTickets)
			End If
			_reportDataList = New List(Of OrderDetailsRowInfo)
			If orderWarnings.Count > 0 Then
				btnFilter_Click(btnFilter, New EventArgs)
			Else
				PopulateOrdersTable()
				pnlOrders.Visible = False
				btnComplete.Visible = False
				btnDelete.Visible = False
			End If
			lblStatus.Text = String.Join("<br />", orderWarnings)
		End If
		Utilities.ConfirmBox(Me.btnComplete, "Are you sure you want to continue marking the selected orders as complete?")
		Utilities.ConfirmBox(Me.btnDelete, "Are you sure you want to continue deleting the selected orders?")
	End Sub

	Protected Sub btnComplete_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnComplete.Click
		lblStatus.Text = String.Empty
		IterateThroughChildren(tblOrders)
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		For Each orderId As Guid In _checkedOrders.Keys
			With _checkedOrders(orderId)
				Dim orderInfo As KaOrder = .OrderInfo
				If .HasProgressRecords Then
					If Not _ordersWithProgressRecords.Contains(orderInfo.Number) Then _ordersWithProgressRecords.Add(orderInfo.Number)
				ElseIf .IsLocked Then
					If Not _ordersLocked.Contains(orderInfo.Number) Then _ordersLocked.Add(orderInfo.Number)
				ElseIf .IsStaged Then
					If Not _stagedOrders.Contains(orderInfo.Number) Then _stagedOrders.Add(orderInfo.Number)
				Else
					orderInfo.Completed = True
					orderInfo.SqlUpdate(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
				End If
			End With
		Next
		Dim parameters As String = GetRedirectParameters(False)
		Response.Redirect(HttpContext.Current.Request.Url.ToString().Split("?")(0) & IIf(parameters.Equals(String.Empty), "", "?" & parameters), True)
	End Sub

	Protected Sub btnDelete_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnDelete.Click
		lblStatus.Text = String.Empty
		IterateThroughChildren(tblOrders)
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		For Each orderId As Guid In _checkedOrders.Keys
			With _checkedOrders(orderId)
				Dim orderInfo As KaOrder = .OrderInfo
				If .HasProgressRecords Then
					If Not _ordersWithProgressRecords.Contains(orderInfo.Number) Then _ordersWithProgressRecords.Add(orderInfo.Number)
				ElseIf .IsLocked Then
					If Not _ordersLocked.Contains(orderInfo.Number) Then _ordersLocked.Add(orderInfo.Number)
				ElseIf .IsStaged Then
					If Not _stagedOrders.Contains(orderInfo.Number) Then _stagedOrders.Add(orderInfo.Number)
				ElseIf .HasTickets Then
					If Not _ordersWithTickets.Contains(orderInfo.Number) Then _ordersWithTickets.Add(orderInfo.Number)
					orderInfo.Completed = True
					orderInfo.SqlUpdate(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
				Else
					orderInfo.Deleted = True
					orderInfo.SqlUpdate(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
				End If
			End With
		Next
		Dim parameters As String = GetRedirectParameters(True)
		Response.Redirect(HttpContext.Current.Request.Url.ToString().Split("?")(0) & IIf(parameters.Equals(String.Empty), "", "?" & parameters), True)
	End Sub

	Private Function GetRedirectParameters(ByVal includeTickets As Boolean) As String
		Dim parameters As String = ""
		If _stagedOrders.Count > 0 Then parameters = "staged=" & String.Join(",", _stagedOrders.ToArray())
		If _ordersWithTickets.Count > 0 Then parameters &= IIf(parameters.Length > 0, "&", "") & "tickets=" & String.Join(",", _ordersWithTickets.ToArray())
		If _ordersWithProgressRecords.Count > 0 Then parameters &= IIf(parameters.Length > 0, "&", "") & "inProgress=" & String.Join(",", _ordersWithProgressRecords.ToArray())
		If _ordersLocked.Count > 0 Then parameters &= IIf(parameters.Length > 0, "&", "") & "locked=" & String.Join(",", _ordersLocked.ToArray())
		Return parameters
	End Function

	Protected Sub btnFilter_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnFilter.Click
		lblStatus.Text = String.Empty
		_checkedOrders.Clear()
		CreateQueryAndPopulateTicketList()
		Page.ClientScript.RegisterStartupScript(Page.ClientScript.GetType(), "ResetScrollPosition;", "resetDotNetScrollPosition();", True)
	End Sub

	Protected Sub cbxCheckAll_CheckedChanged(sender As Object, e As EventArgs) Handles cbxCheckAll.CheckedChanged
		lblStatus.Text = String.Empty
		CreateQueryAndPopulateTicketList()
	End Sub

	Protected Sub CreateQueryAndPopulateTicketList()
		Dim fromDate As DateTime
		If Not String.IsNullOrEmpty(tbxFromDate.Value) Then
			Try
				fromDate = DateTime.Parse(tbxFromDate.Value) ' converting string value to datetime value for comparison in IF statement
			Catch ex As FormatException
				EmptyTable()
				DisplayJavaScriptMessage("InvalidStartDate", Utilities.JsAlert("Please enter a valid date for the beginning (From) date"))
			End Try
		End If

		Dim toDate As DateTime
		If Not String.IsNullOrEmpty(tbxFromDate.Value) Then
			Try
				toDate = DateTime.Parse(tbxToDate.Value)
			Catch ex As FormatException
				EmptyTable()
				DisplayJavaScriptMessage("InvalidEndDate", Utilities.JsAlert("Please enter a valid date for the ending (To) date"))
			End Try
		End If
		If fromDate > toDate Then ' check if "From" date is later then "To" date 
			DisplayJavaScriptMessage("InvalidStartEndDate", Utilities.JsAlert("Please select an ending date (To) that is later than the beginning date (From)"))
		Else
			If String.IsNullOrEmpty(tbxFromDate.Value) And Not String.IsNullOrEmpty(tbxToDate.Value) Then
				EmptyTable()
				DisplayJavaScriptMessage("InvalidStartDate", Utilities.JsAlert("Please enter a valid date for the beginning (From) date"))
			Else
				Dim customerName As String = ""
				Dim customerId As Guid = Guid.Parse(ddlAccounts.SelectedValue)
				If (Not customerId.Equals(Guid.Empty)) Then
					Dim customers As ArrayList = KaCustomerAccount.GetAll(GetUserConnection(_currentUser.Id), "id = " & Q(customerId), "")
					customerName = customers(0).name
				End If
				Dim ownerId As Guid = Guid.Parse(ddlOwner.SelectedValue)
				Dim locationId As Guid = Guid.Parse(ddlFacility.SelectedValue)
				Dim ordersInString As String = ""
				If locationId <> Guid.Empty Then
					Dim tempOrders As List(Of KaOrder) = KaOrder.GetAllForLocationAndBays(GetUserConnection(_currentUser.Id), Nothing, locationId, Nothing, ownerId, "")

					For Each order As KaOrder In tempOrders
						ordersInString &= "," & Q(order.Id)
					Next
					If (ordersInString.Length > 0) Then
						ordersInString = ordersInString.Substring(1)
					End If
				End If
				Dim query As String = "SELECT orders.id, orders.number, owners.name AS owner_name, orders.created " &
									  "FROM orders " &
									  "INNER JOIN owners ON orders.owner_id = owners.id " &
									  "WHERE orders.completed = 0 AND orders.deleted = 0 " &
									  IIf(String.IsNullOrEmpty(tbxFromDate.Value), "", "AND orders.created >= " & Q(fromDate) & " AND orders.created <= " & Q(toDate) & " ") &
									  IIf(String.IsNullOrEmpty(ordersInString), "", "AND orders.id in (" & ordersInString + ") ") &
									  IIf(ownerId.Equals(Guid.Empty), "", $"AND {KaOrder.TABLE_NAME}.{KaOrder.FN_OWNER_ID} = {Q(ownerId)} ") &
									  IIf(tbxOrderNumberContains.Text.Length = 0, "", $"AND {KaOrder.TABLE_NAME}.{KaOrder.FN_NUMBER} LIKE {Database.Q($"%{tbxOrderNumberContains.Text.Trim}%")} ") &
									  "ORDER BY orders.created ASC"
				PopulateOrderList(query, customerName)
			End If
		End If
	End Sub
#End Region

#Region " Orders Table "
	Protected Sub PopulateOrderList(query As String, customerName As String)
		_reportDataList = New List(Of OrderDetailsRowInfo)
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		If (String.IsNullOrEmpty(query)) Then
			query = "SELECT orders.id, orders.number, owners.name AS owner_name, orders.created " &
					"FROM orders " &
					"INNER JOIN owners ON orders.owner_id = owners.id " &
					"WHERE orders.completed = 0 AND orders.deleted = 0 " &
					IIf(_currentUser.OwnerId.Equals(Guid.Empty), "", "AND orders.owner_id = " & Q(_currentUser.OwnerId) & " ") &
					"ORDER BY orders.created ASC"
		End If
		Dim getOrdersRdr As OleDbDataReader = Tm2Database.ExecuteReader(connection, query)
		Do While getOrdersRdr.Read()
			Dim detailRowList As New OrderDetailsRowInfo() With {
				.Number = getOrdersRdr.Item("number"),
				.CustomerAccounts = "",
				.OwnerName = getOrdersRdr.Item("owner_name"),
				.Products = "",
				.Created = getOrdersRdr.Item("created"),
				.OrderId = getOrdersRdr.Item("id"),
				.Notes = ""
			}
			_reportDataList.Add(detailRowList)
		Loop
		getOrdersRdr.Close()
		Dim customers As New Dictionary(Of Guid, KaCustomerAccount)
		Dim products As New Dictionary(Of Guid, KaProduct)
		Dim rowCounter As Integer = 0
		Do While rowCounter < _reportDataList.Count
			Dim detailRowList As OrderDetailsRowInfo = _reportDataList(rowCounter)
			Dim orderId As Guid = detailRowList.OrderId
			Dim orderInfo As New KaOrder(connection, orderId)
			For Each orderCust As KaOrderCustomerAccount In orderInfo.OrderAccounts
				If Not orderCust.Deleted Then
					If Not customers.ContainsKey(orderCust.CustomerAccountId) Then
						Try
							customers.Add(orderCust.CustomerAccountId, New KaCustomerAccount(connection, orderCust.CustomerAccountId))
						Catch ex As RecordNotFoundException
							customers.Add(orderCust.CustomerAccountId, New KaCustomerAccount() With {.Name = "Unknown"})
						End Try
					End If
					If Not String.IsNullOrEmpty(detailRowList.CustomerAccounts) Then detailRowList.CustomerAccounts &= "<br/>"

					detailRowList.CustomerAccounts &= Server.HtmlEncode(customers(orderCust.CustomerAccountId).Name)
					If orderInfo.OrderAccounts.Count > 1 Then detailRowList.CustomerAccounts &= String.Format(" {0:0.00}%", orderCust.Percentage)
				End If
			Next

			If (customerName.Equals(String.Empty) Or (Not customerName.Equals(String.Empty) And detailRowList.CustomerAccounts.Contains(customerName))) Then
				For Each orderItem As KaOrderItem In orderInfo.OrderItems
					If Not orderItem.Deleted Then
						If Not products.ContainsKey(orderItem.ProductId) Then
							Try
								products.Add(orderItem.ProductId, New KaProduct(connection, orderItem.ProductId))
							Catch ex As RecordNotFoundException
								products.Add(orderItem.ProductId, New KaProduct() With {.Name = "Unknown"})
							End Try
						End If
						If Not String.IsNullOrEmpty(detailRowList.Products) Then detailRowList.Products &= ", "
						detailRowList.Products &= Server.HtmlEncode(products(orderItem.ProductId).Name)
					End If
				Next

				If orderInfo.OrderReferencedInOtherProgressRecords(connection, Nothing, Guid.Empty) Then  'Figure out if order is in progress of being dispensed
					detailRowList.Notes = "Order in progress, cannot mark complete or delete."
				ElseIf orderInfo.IsOrderStaged(connection, Nothing) Then  'Figure out if order is staged
					detailRowList.Notes = "Staged order, cannot mark complete or delete."
				ElseIf orderInfo.Locked Then
					detailRowList.Notes = "Order locked, cannot mark complete or delete."
				ElseIf orderInfo.HasTickets(connection, Nothing) Then 'Figure out if order has tickets  
					detailRowList.Notes = "Has tickets, cannot delete."
				End If

				rowCounter += 1
			Else
				_reportDataList.RemoveAt(rowCounter)
			End If
		Loop

		PopulateOrdersTable()
	End Sub

	Protected Sub PopulateOrdersTable()
		Dim table As HtmlTable = Page.FindControl("tblOrders")
		If table Is Nothing Then Exit Sub
		Do While table.Rows.Count > 1
			table.Rows.RemoveAt(1)
		Loop
		For Each detailRowList As OrderDetailsRowInfo In _reportDataList
			Dim row As New HtmlTableRow
			table.Rows.Add(row)
			' row ID
			Dim cell As New HtmlTableCell()
			row.Cells.Add(cell)
			' order number
			Dim label As New Label()
			label.Text = detailRowList.Number
			cell.Controls.Add(label)
			' accounts
			cell = New HtmlTableCell()
			row.Cells.Add(cell)
			label = New Label()
			label.Text = detailRowList.CustomerAccounts
			cell.Controls.Add(label)
			' owner
			cell = New HtmlTableCell()
			row.Cells.Add(cell)
			label = New Label()
			label.Text = detailRowList.OwnerName
			cell.Controls.Add(label)
			' product
			cell = New HtmlTableCell()
			row.Cells.Add(cell)
			label = New Label()
			label.Text = detailRowList.Products
			cell.Controls.Add(label)
			' created date
			cell = New HtmlTableCell()
			row.Cells.Add(cell)
			label = New Label()
			label.Text = String.Format("{0:g}", detailRowList.Created)
			cell.Controls.Add(label)
			' notes
			cell = New HtmlTableCell()
			row.Cells.Add(cell)
			label = New Label()
			label.Text = detailRowList.Notes
			cell.Controls.Add(label)
			' checkbox
			cell = New HtmlTableCell()
			row.Cells.Add(cell)
			Dim checkbox As New CheckBox()
			checkbox.ID = "cbxOrderId" & detailRowList.OrderId.ToString()
			If (cbxCheckAll.Checked) Then
				checkbox.Checked = True
			Else
				checkbox.Checked = False
			End If
			cell.Controls.Add(checkbox)
		Next
		If tblOrders.Rows.Count > 1 Then
			pnlOrders.Visible = True
			btnComplete.Visible = True
			btnDelete.Visible = True
		Else
			pnlOrders.Visible = False
			btnComplete.Visible = False
			btnDelete.Visible = False
		End If
	End Sub
#End Region

	Private Sub IterateThroughChildren(parent As Control)
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		For Each c As Control In parent.Controls
			If (TypeOf c Is CheckBox AndAlso c.ID.StartsWith("cbxOrderId")) Then
				Dim checkbox As CheckBox = CType(c, CheckBox)
				Dim orderId As Guid = Guid.Parse(checkbox.ID.Substring(10))
				If (checkbox.Checked) Then

					Dim status As New OrderStatus
					With status
						.OrderInfo = New KaOrder(connection, orderId)
						.HasProgressRecords = .OrderInfo.OrderReferencedInOtherProgressRecords(connection, Nothing, Guid.Empty)
						.HasTickets = .OrderInfo.HasTickets(connection, Nothing)
						.IsLocked = .OrderInfo.Locked
						.IsStaged = .OrderInfo.IsOrderStaged(connection, Nothing)
					End With
					_checkedOrders.Add(orderId, status)
				Else
					_checkedOrders.Remove(orderId)
				End If
			End If

			If (c.Controls.Count > 0) Then
				IterateThroughChildren(c)
			End If
		Next
	End Sub

	Private Function CanMarkComplete(connection As OleDbConnection, order As KaOrder) As Boolean
		With order
			If .OrderReferencedInOtherProgressRecords(connection, Nothing, Guid.Empty) Then
				If Not _ordersWithProgressRecords.Contains(.Number) Then _ordersWithProgressRecords.Add(.Number)
				Return False
			ElseIf .Locked Then
				If Not _ordersLocked.Contains(.Number) Then _ordersLocked.Add(.Number)
				Return False
			ElseIf .IsOrderStaged(connection, Nothing) Then
				If Not _stagedOrders.Contains(.Number) Then _stagedOrders.Add(.Number)
				Return False
			Else
				Return True
			End If
		End With
	End Function

	Private Sub PopulateCustomerAccounts()
		ddlAccounts.Items.Clear()
		ddlAccounts.Items.Add(New ListItem("All accounts", Guid.Empty.ToString))
		For Each account As KaCustomerAccount In KaCustomerAccount.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND is_supplier=0" & IIf(_currentUser.OwnerId <> Guid.Empty, String.Format(" AND (owner_id={0} OR owner_id={1})", Q(Guid.Empty), Q(_currentUser.OwnerId)), ""), "name")
			ddlAccounts.Items.Add(New ListItem(account.Name, account.Id.ToString))
		Next
	End Sub

	Private Sub PopulateOwnerList()
		ddlOwner.Items.Clear()

		If _currentUser.OwnerId = Guid.Empty Then
			Dim li As ListItem = New ListItem
			li.Text = "All owners"
			li.Value = Guid.Empty.ToString
			ddlOwner.Items.Add(li)

			For Each owner As KaOwner In KaOwner.GetAll(GetUserConnection(_currentUser.Id), "deleted = 0", "name asc")
				li = New ListItem
				li.Text = owner.Name
				li.Value = owner.Id.ToString
				ddlOwner.Items.Add(li)
			Next
		Else
			Dim owner As KaOwner = New KaOwner(GetUserConnection(_currentUser.Id), _currentUser.OwnerId)
			Dim li As ListItem = New ListItem
			li.Text = owner.Name
			li.Value = owner.Id.ToString
			ddlOwner.Items.Add(li)
		End If
	End Sub

	Private Sub PopulateFacilityList()
		ddlFacility.Items.Clear()

		Dim li As ListItem = New ListItem
		li.Text = "All facilities"
		li.Value = Guid.Empty.ToString
		ddlFacility.Items.Add(li)

		For Each facility As KaLocation In KaLocation.GetAll(GetUserConnection(_currentUser.Id), "deleted = 0", "name asc")
			li = New ListItem
			li.Text = facility.Name
			li.Value = facility.Id.ToString
			ddlFacility.Items.Add(li)
		Next
	End Sub

	Private Sub EmptyTable()
		Dim table As HtmlTable = Page.FindControl("tblOrders")
		Do While table.Rows.Count > 1
			table.Rows.RemoveAt(1)
		Loop
	End Sub

	Protected Sub ToolkitScriptManager1_AsyncPostBackError(ByVal sender As Object, ByVal e As System.Web.UI.AsyncPostBackErrorEventArgs)
		ToolkitScriptManager1.AsyncPostBackErrorMessage = e.Exception.Message
	End Sub

	Private Sub DisplayJavaScriptMessage(key As String, script As String)
		ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), key, script, False)
	End Sub

	Protected Overrides Function SaveViewState() As Object
		Dim viewState(1) As Object
		viewState(0) = _reportDataList
		viewState(1) = MyBase.SaveViewState()
		Return viewState
	End Function

	Protected Overrides Sub LoadViewState(savedState As Object)
		'Getting the dropdown list value from view state.
		If savedState IsNot Nothing AndAlso CType(savedState, Object).Length > 1 Then
			Dim viewState As Object() = savedState
			_reportDataList = viewState(0)
			PopulateOrdersTable()
			MyBase.LoadViewState(viewState(1))
		Else
			MyBase.LoadViewState(savedState)
		End If
	End Sub

	<Serializable>
	Protected Class OrderDetailsRowInfo
		Public OrderId As Guid
		Public Number As String
		Public CustomerAccounts As String
		Public OwnerName As String
		Public Products As String
		Public Created As DateTime
		Public Notes As String
	End Class
End Class