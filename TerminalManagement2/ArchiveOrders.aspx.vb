Imports KahlerAutomation.KaTm2Database
Imports System.Environment
Imports System.Data.OleDb

Public Class ArchiveOrders
	Inherits System.Web.UI.Page

	Private _currentUser As KaUser
	Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
	Private _currentTableName As String = KaOrder.TABLE_NAME
	Private _customFields As New List(Of KaCustomField)
	Private _customFieldData As New List(Of KaCustomFieldData)
	Private _checkedOrders As New List(Of Guid)
	Private _checkedTickets As New List(Of Guid)
	Private _reportDataList As List(Of OrderDetailsRowInfo)

#Region "Events"
	Protected Sub Page_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Init
		_currentUser = Utilities.GetUser(Me)
	End Sub

	Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
		Response.Cache.SetCacheability(HttpCacheability.NoCache)
		_currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Orders")
		If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")
		btnArchive.Enabled = _currentUserPermission(_currentTableName).Edit
		If (cbxShowArchived.Checked) Then
			btnArchive.Text = "Unarchive"
			cbxCheckAllOrders.Text = "Unarchive Order"
			cbxCheckAllTickets.Text = "Unarchive Tickets"
		Else
			btnArchive.Text = "Archive"
			cbxCheckAllOrders.Text = "Archive Order"
			cbxCheckAllTickets.Text = "Archive Tickets"
		End If
		If Not Page.IsPostBack Then
			PopulateCustomerAccounts()
			PopulateOwnerList()
			PopulateFacilityList()
			_reportDataList = New List(Of OrderDetailsRowInfo)
			PopulateOrdersTable()
			pnlOrders.Visible = False
			btnArchive.Visible = False
		End If
		lblStatus.Text = ""
	End Sub

	Protected Sub btnArchive_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnArchive.Click
		IterateThroughChildrenForOrders(tblOrders)
		IterateThroughChildrenForTickets(tblOrders)
		Dim archived As Boolean = cbxShowArchived.Checked
		For Each orderId As Guid In _checkedOrders
			Dim orderInfo As New KaOrder(GetUserConnection(_currentUser.Id), orderId)
			orderInfo.Archived = Not orderInfo.Archived
			orderInfo.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
		Next
		For Each orderId As Guid In _checkedTickets
			Dim allTickets As ArrayList = KaTicket.GetAll(GetUserConnection(_currentUser.Id), "voided=0 AND order_id = " & Q(orderId), "loaded_at ASC")
			For Each ticketInfo As KaTicket In allTickets
				ticketInfo.Archived = Not archived
				ticketInfo.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
			Next
		Next
		btnFilter_Click(btnFilter, New EventArgs())
	End Sub

	Protected Sub btnFilter_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnFilter.Click
		_checkedOrders.Clear()
		_checkedTickets.Clear()
		CreateQueryAndPopulateTicketList()
		DisplayJavaScriptMessage("ResetScrollPosition;", "resetDotNetScrollPosition();", True)
	End Sub

	Protected Sub cbxCheckAllOrders_CheckedChanged(sender As Object, e As EventArgs) Handles cbxCheckAllOrders.CheckedChanged
		CreateQueryAndPopulateTicketList()
	End Sub

	Protected Sub cbxCheckAllTickets_CheckedChanged(sender As Object, e As EventArgs) Handles cbxCheckAllTickets.CheckedChanged
		CreateQueryAndPopulateTicketList()
	End Sub

	Protected Sub CreateQueryAndPopulateTicketList()
		Dim fromDate As DateTime
		If Not String.IsNullOrEmpty(tbxFromDate.Value) Then
			Try
				fromDate = DateTime.Parse(tbxFromDate.Value) ' converting string value to datetime value for comparison in IF statement
			Catch ex As FormatException
				EmptyTable()
				DisplayJavaScriptMessage("InvalidStartDate", Utilities.JsAlert("Please enter a valid date for the beginning (From) date"), False)
			End Try
		End If

		Dim toDate As DateTime
		If Not String.IsNullOrEmpty(tbxFromDate.Value) Then
			Try
				toDate = DateTime.Parse(tbxToDate.Value)
			Catch ex As FormatException
				EmptyTable()
				DisplayJavaScriptMessage("InvalidEndDate", Utilities.JsAlert("Please enter a valid date for the ending (To) date"), False)
			End Try
		End If
		If fromDate > toDate Then ' check if "From" date is later then "To" date 
			DisplayJavaScriptMessage("InvalidStartEndDate", Utilities.JsAlert("Please select an ending date (To) that is later than the beginning date (From)"), False)
		Else
			If String.IsNullOrEmpty(tbxFromDate.Value) And Not String.IsNullOrEmpty(tbxToDate.Value) Then
				EmptyTable()
				DisplayJavaScriptMessage("InvalidStartDate", Utilities.JsAlert("Please enter a valid date for the beginning (From) date"), False)
			Else
				Dim customerName As String = ""
				Dim customerId As Guid = Guid.Parse(ddlAccounts.SelectedValue)
				If (Not customerId.Equals(Guid.Empty)) Then
					Dim customers As ArrayList = KaCustomerAccount.GetAll(GetUserConnection(_currentUser.Id), "id = " & Q(customerId), "")
					customerName = customers(0).name
				End If
				Dim ownerId As Guid = Guid.Parse(ddlOwner.SelectedValue)
				Dim locationId As Guid = Guid.Parse(ddlFacility.SelectedValue)
				Dim showArchived As Boolean = cbxShowArchived.Checked
				Dim ordersInString As String = ""
				Dim query As String = "SELECT orders.id, orders.number, owners.name AS owner_name, orders.created, orders.archived " &
									  "FROM orders " &
									  "INNER JOIN owners ON orders.owner_id = owners.id " &
									  "WHERE orders.completed = 1 " &
									  IIf(showArchived, "AND orders.archived = 1 ", "AND orders.archived = 0 ") & " " &
									  IIf(String.IsNullOrEmpty(tbxFromDate.Value), "", "AND orders.created >= " & Q(fromDate) & " AND orders.created <= " & Q(toDate) & " ") &
									  IIf(locationId = Guid.Empty, "", "AND orders.id IN (SELECT order_id FROM order_items WHERE (deleted = 0) AND product_id in (select product_id from " &
										  "product_bulk_products where location_id = " & Q(locationId) & " and deleted = 0))") &
									  IIf(ownerId.Equals(Guid.Empty), "", $"AND {KaOrder.TABLE_NAME}.{KaOrder.FN_OWNER_ID} = {Q(ownerId)} ") &
									  IIf(tbxOrderNumberContains.Text.Length = 0, "", $"AND {KaOrder.TABLE_NAME}.{KaOrder.FN_NUMBER} LIKE {Database.Q($"%{tbxOrderNumberContains.Text.Trim}%")} ") &
									  "ORDER BY orders.created ASC"
				PopulateOrderList(query, customerName)
			End If
		End If
		If tblOrders.Rows.Count > 1 Then
			pnlOrders.Visible = True
			btnArchive.Visible = True
		Else
			pnlOrders.Visible = False
			btnArchive.Visible = False
		End If
		Utilities.ConfirmBox(Me.btnArchive, "Are you sure you want to continue with the archiving process?")
	End Sub
#End Region

#Region " Orders Table "
	Protected Sub PopulateOrderList(query As String, customerName As String)
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		If (String.IsNullOrEmpty(query)) Then
			query = $"SELECT {KaOrder.TABLE_NAME}.{KaOrder.FN_ID}, {KaOrder.TABLE_NAME}.{KaOrder.FN_NUMBER}, {KaOwner.TABLE_NAME}.{KaOwner.FN_NAME} AS owner_name, {KaOrder.TABLE_NAME}.{KaOrder.FN_CREATED}, {KaOrder.TABLE_NAME}.{KaOrder.FN_ARCHIVED} " &
					$"FROM {KaOrder.TABLE_NAME} " &
					$"INNER JOIN {KaOwner.TABLE_NAME} ON {KaOrder.TABLE_NAME}.{KaOrder.FN_OWNER_ID} = {KaOwner.TABLE_NAME}.{KaOwner.FN_ID} " &
					$"WHERE {KaOrder.TABLE_NAME}.{KaOrder.FN_COMPLETED} = 1 " &
					IIf(_currentUser.OwnerId.Equals(Guid.Empty), "", $"AND {KaOrder.TABLE_NAME}.owner_id = {Q(_currentUser.OwnerId)} ") &
					$"ORDER BY {KaOrder.TABLE_NAME}.{KaOrder.FN_CREATED}, {KaOrder.TABLE_NAME}.{KaOrder.FN_NUMBER}"
		End If
		Dim getOrdersRdr As OleDbDataReader = Tm2Database.ExecuteReader(connection, query)
		_reportDataList = New List(Of OrderDetailsRowInfo)
		Do While getOrdersRdr.Read()
			Dim detailRowList As New OrderDetailsRowInfo() With {
				.Number = getOrdersRdr.Item("number"),
				.CustomerAccounts = "",
				.OwnerName = getOrdersRdr.Item("owner_name"),
				.Products = "",
				.Created = getOrdersRdr.Item("created"),
				.OrderId = getOrdersRdr.Item("id"),
				.Archived = getOrdersRdr.Item("archived")
			}
			_reportDataList.Add(detailRowList)
		Loop
		getOrdersRdr.Close()
		Dim rowCounter As Integer = 0
		Do While rowCounter < _reportDataList.Count
			Dim detailRowList As OrderDetailsRowInfo = _reportDataList(rowCounter)
			Dim getCustomersRdr As OleDbDataReader = Tm2Database.ExecuteReader(connection,
					$"SELECT DISTINCT  {KaCustomerAccount.TABLE_NAME}.{KaCustomerAccount.FN_NAME} AS customer_account_name, {KaOrderCustomerAccount.TABLE_NAME}.{KaOrderCustomerAccount.FN_PERCENTAGE} AS percentage " &
					$"FROM {KaOrderCustomerAccount.TABLE_NAME} INNER JOIN {KaCustomerAccount.TABLE_NAME} " &
					$"ON {KaOrderCustomerAccount.TABLE_NAME}.{KaOrderCustomerAccount.FN_CUSTOMER_ACCOUNT_ID} = {KaCustomerAccount.TABLE_NAME}.{KaCustomerAccount.FN_ID} " &
					$"WHERE {KaOrderCustomerAccount.TABLE_NAME}.{KaOrderCustomerAccount.FN_DELETED} = 0  " &
						$"AND {KaOrderCustomerAccount.TABLE_NAME}.{KaOrderCustomerAccount.FN_ORDER_ID} = {Q(detailRowList.OrderId)} " &
					$"ORDER BY {KaCustomerAccount.TABLE_NAME}.{KaCustomerAccount.FN_NAME}")
			Do While getCustomersRdr.Read()
				Dim tempCustomerName As String = getCustomersRdr.Item("customer_account_name")
				If (String.IsNullOrEmpty(detailRowList.CustomerAccounts)) Then
					detailRowList.CustomerAccounts = tempCustomerName
				Else
					detailRowList.CustomerAccounts &= " <br/> " & tempCustomerName
				End If
				detailRowList.CustomerAccounts &= " " & String.Format(" {0:0.00}%", getCustomersRdr.Item("percentage"))
			Loop
			getCustomersRdr.Close()

			If (customerName.Equals(String.Empty) Or (Not customerName.Equals(String.Empty) And detailRowList.CustomerAccounts.ToString.Contains(customerName))) Then
				Dim getProductsRdr As OleDbDataReader = Tm2Database.ExecuteReader(connection,
							$"SELECT DISTINCT {KaProduct.TABLE_NAME}.{KaProduct.FN_NAME} AS product_name " &
							$"FROM {KaOrderItem.TABLE_NAME} " &
							$"INNER JOIN {KaProduct.TABLE_NAME} ON {KaOrderItem.TABLE_NAME}.{KaOrderItem.FN_PRODUCT_ID} = {KaProduct.TABLE_NAME}.{KaProduct.FN_ID} " &
							$"WHERE {KaOrderItem.TABLE_NAME}.{KaOrderItem.FN_DELETED} = 0  " &
								$"AND {KaOrderItem.TABLE_NAME}.{KaOrderItem.FN_ORDER_ID} = {Q(detailRowList.OrderId)} " &
							$"ORDER BY {KaProduct.TABLE_NAME}.{KaProduct.FN_NAME}")
				Do While getProductsRdr.Read()
					Dim productName As String = getProductsRdr.Item("product_name")
					If (String.IsNullOrEmpty(detailRowList.Products)) Then
						detailRowList.Products = productName
					Else
						detailRowList.Products &= ", " & productName
					End If
				Loop
				getProductsRdr.Close()
				rowCounter += 1
			Else
				_reportDataList.RemoveAt(rowCounter)
			End If
		Loop

		PopulateOrdersTable()
	End Sub

	Protected Sub PopulateOrdersTable()
		Dim table As HtmlTable = Page.FindControl("tblOrders")
		Do While table.Rows.Count > 1
			table.Rows.RemoveAt(1)
		Loop
		If table Is Nothing Then Exit Sub
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
			' checkbox for orders
			cell = New HtmlTableCell()
			row.Cells.Add(cell)
			Dim checkbox As New CheckBox()
			checkbox.ID = "cbxOrderId" & detailRowList.OrderId.ToString()
			If (cbxCheckAllOrders.Checked) Then
				checkbox.Checked = True
			Else
				checkbox.Checked = False
			End If
			cell.Controls.Add(checkbox)
			' checkbox for tickets
			cell = New HtmlTableCell()
			row.Cells.Add(cell)
			Dim checkboxTickets As New CheckBox()
			checkboxTickets.ID = "cbxTickets" & detailRowList.OrderId.ToString()
			If (cbxCheckAllTickets.Checked) Then
				checkboxTickets.Checked = True
			Else
				checkboxTickets.Checked = False
			End If
			cell.Controls.Add(checkboxTickets)
			If cbxShowArchived.Checked <> detailRowList.Archived Then
				row.Visible = False
			End If
		Next
	End Sub
#End Region

	Private Sub IterateThroughChildrenForOrders(parent As Control)
		For Each c As Control In parent.Controls
			If (TypeOf c Is CheckBox AndAlso c.ID.StartsWith("cbxOrderId")) Then
				Dim checkbox As CheckBox = CType(c, CheckBox)
				Dim orderId As Guid = Guid.Parse(checkbox.ID.Substring(10))
				If (checkbox.Checked) Then
					_checkedOrders.Add(orderId)
				Else
					_checkedOrders.Remove(orderId)
				End If
			End If

			If (c.Controls.Count > 0) Then
				IterateThroughChildrenForOrders(c)
			End If
		Next
	End Sub

	Private Sub IterateThroughChildrenForTickets(parent As Control)
		For Each c As Control In parent.Controls
			If (TypeOf c Is CheckBox AndAlso c.ID.StartsWith("cbxTickets")) Then
				Dim checkbox As CheckBox = CType(c, CheckBox)
				Dim orderId As Guid = Guid.Parse(checkbox.ID.Substring(10))
				If (checkbox.Checked) Then
					_checkedTickets.Add(orderId)
				Else
					_checkedTickets.Remove(orderId)
				End If
			End If

			If (c.Controls.Count > 0) Then
				IterateThroughChildrenForTickets(c)
			End If
		Next
	End Sub

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

	Private Sub DisplayJavaScriptMessage(key As String, script As String, addScriptTags As Boolean)
		ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), key, script, addScriptTags)
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
		Public Archived As String
	End Class
End Class