Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class ArchiveReceivingPurchaseOrders
	Inherits System.Web.UI.Page

	Private _currentUser As KaUser
	Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
	Private _currentTableName As String = KaReceivingPurchaseOrder.TABLE_NAME
	Private _customFields As New List(Of KaCustomField)
	Private _customFieldData As New List(Of KaCustomFieldData)
	Private _checkedOrders As New List(Of Guid)
	Private _checkedTickets As New List(Of Guid)

	Private _receivingPurchaseOrderIds As New List(Of Guid)

#Region "Events"
	Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
		Response.Cache.SetCacheability(HttpCacheability.NoCache)
		_currentUser = Utilities.GetUser(Me)
		_currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "PurchaseOrders")
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
			pnlOrders.Visible = False
			btnArchive.Visible = False
		End If
		lblStatus.Text = ""
	End Sub

	Protected Sub btnArchive_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnArchive.Click
		IterateThroughChildrenForOrders(Me)
		IterateThroughChildrenForTickets(Me)
		Dim archiveStatustoSet As Boolean = cbxShowArchived.Checked
		For Each orderId As Guid In _checkedOrders
			Dim orderInfo As New KaReceivingPurchaseOrder(GetUserConnection(_currentUser.Id), orderId)
			If orderInfo.Archived = archiveStatustoSet Then
				orderInfo.Archived = Not archiveStatustoSet
				orderInfo.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
			End If
		Next
		For Each orderId As Guid In _checkedTickets
			Dim allTickets As ArrayList = KaReceivingTicket.GetAll(GetUserConnection(_currentUser.Id), "voided=0 AND receiving_purchase_order_id = " & Q(orderId), "date_of_delivery ASC")
			For Each ticketInfo As KaReceivingTicket In allTickets
				If ticketInfo.Archived = archiveStatustoSet Then
					ticketInfo.Archived = Not archiveStatustoSet
					ticketInfo.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
				End If
			Next
		Next
		CreateQueryAndPopulateTicketList()
	End Sub

	Protected Sub btnFilter_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnFilter.Click
		_checkedOrders.Clear()
		_checkedTickets.Clear()
		CreateQueryAndPopulateTicketList()
	End Sub

	Protected Sub cbxCheckAllOrders_CheckedChanged(sender As Object, e As EventArgs) Handles cbxCheckAllOrders.CheckedChanged
		CreateQueryAndPopulateTicketList()
	End Sub

	Protected Sub cbxCheckAllTickets_CheckedChanged(sender As Object, e As EventArgs) Handles cbxCheckAllTickets.CheckedChanged
		CreateQueryAndPopulateTicketList()
	End Sub

	Protected Sub CreateQueryAndPopulateTicketList()
		Dim fromDate As DateTime = DateTime.MaxValue
		Dim toDate As DateTime = DateTime.MinValue
		If Not String.IsNullOrEmpty(tbxFromDate.Value) AndAlso Not DateTime.TryParse(tbxFromDate.Value, fromDate) Then
			DisplayJavaScriptMessage("InvalidStartDate", Utilities.JsAlert("Please enter a valid date for the beginning (From) date"))
		ElseIf Not String.IsNullOrEmpty(tbxToDate.Value) AndAlso Not DateTime.TryParse(tbxToDate.Value, toDate) Then
			DisplayJavaScriptMessage("InvalidEndDate", Utilities.JsAlert("Please enter a valid date for the ending (To) date"))
		ElseIf fromDate <> DateTime.MaxValue AndAlso toDate <> DateTime.MinValue AndAlso fromDate > toDate Then ' check if "From" date is later then "To" date 
			DisplayJavaScriptMessage("InvalidStartEndDate", Utilities.JsAlert("Please select an ending date (To) that is later than the beginning date (From)"))
		ElseIf String.IsNullOrEmpty(tbxFromDate.Value) And Not String.IsNullOrEmpty(tbxToDate.Value) Then
			DisplayJavaScriptMessage("InvalidStartDate", Utilities.JsAlert("Please enter a valid date for the beginning (From) date"))
		Else
			Dim supplierId As Guid = Guid.Empty
			Guid.TryParse(ddlAccounts.SelectedValue, supplierId)
			Dim ownerId As Guid = Guid.Empty
			Guid.TryParse(ddlOwner.SelectedValue, ownerId)
			Dim locationId As Guid = Guid.Empty
			Guid.TryParse(ddlFacility.SelectedValue, locationId)

			_receivingPurchaseOrderIds.Clear()

			Dim query As String = "SELECT DISTINCT receiving_purchase_orders.id " &
					"FROM receiving_purchase_orders " &
					"INNER JOIN bulk_products ON bulk_products.id = receiving_purchase_orders.bulk_product_id " &
					"INNER JOIN owners ON receiving_purchase_orders.owner_id = owners.id " &
					"INNER JOIN customer_accounts ON customer_accounts.id = receiving_purchase_orders.supplier_account_id " &
					"WHERE (receiving_purchase_orders.deleted = 0) " &
						"AND (receiving_purchase_orders.completed = 1) " &
						String.Format("AND (receiving_purchase_orders.archived = {0}) ", Q(cbxShowArchived.Checked)) &
						IIf(fromDate = DateTime.MaxValue OrElse toDate = DateTime.MinValue, "", "AND receiving_purchase_orders.created >= " & Q(fromDate) & " AND receiving_purchase_orders.created <= " & Q(toDate) & " ") &
						IIf(locationId.Equals(Guid.Empty), "", String.Format("AND (receiving_purchase_orders.bulk_product_id IN (SELECT bulk_product_id FROM product_bulk_products WHERE (deleted = 0) AND (location_id = {0}))) ", Q(locationId))) &
						IIf(ownerId.Equals(Guid.Empty), "", String.Format("AND (receiving_purchase_orders.owner_id = {0}) ", Q(ownerId))) &
						IIf(_currentUser.OwnerId.Equals(Guid.Empty), "", String.Format("AND ((receiving_purchase_orders.owner_id = {0}) OR (receiving_purchase_orders.owner_id = {1})) ", Q(_currentUser.OwnerId), Q(Guid.Empty))) &
						IIf(tbxOrderNumberContains.Text.Length = 0, "", $"AND {KaReceivingPurchaseOrder.TABLE_NAME}.number LIKE {Database.Q($"%{tbxOrderNumberContains.Text.Trim}%")} ") &
						IIf(supplierId.Equals(Guid.Empty), "", String.Format("AND (receiving_purchase_orders.supplier_account_id = {0}) ", Q(supplierId)))
			Dim receivingRdr As OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), query)
			Do While receivingRdr.Read()
				_receivingPurchaseOrderIds.Add(receivingRdr.Item("Id"))
			Loop
			receivingRdr.Close()

			PopulatePurchaseOrderList()

			DisplayJavaScriptMessage("ResetScrollPosition;", "resetDotNetScrollPosition();", True)
		End If
	End Sub
#End Region

#Region " Orders Table "
	Protected Sub PopulatePurchaseOrderList()
		Dim table As HtmlTable = Page.FindControl("tblOrders")
		If table Is Nothing Then Exit Sub
		Do While table.Rows.Count > 1
			table.Rows.RemoveAt(1)
		Loop

		If _receivingPurchaseOrderIds.Count > 0 Then
			Dim poIds As String = ""
			For Each poId As Guid In _receivingPurchaseOrderIds
				If poIds.Length > 0 Then poIds &= ","
				poIds &= Q(poId)
			Next

			Dim query As String = String.Format("SELECT DISTINCT receiving_purchase_orders.id, receiving_purchase_orders.number, owners.name AS owner_name, receiving_purchase_orders.created, bulk_products.name AS product_name, customer_accounts.name AS supplier_name, receiving_purchase_orders.completed, receiving_purchase_orders.archived " &
							"FROM receiving_purchase_orders " &
							"INNER JOIN bulk_products ON bulk_products.id = receiving_purchase_orders.bulk_product_id " &
							"INNER JOIN owners ON receiving_purchase_orders.owner_id = owners.id " &
							"INNER JOIN customer_accounts ON customer_accounts.id = receiving_purchase_orders.supplier_account_id " &
							"WHERE (receiving_purchase_orders.id IN ({0})) " &
							"ORDER BY receiving_purchase_orders.created ASC", poIds)

			Dim receivingTable As New DataTable
			Dim receivingDa As New OleDbDataAdapter(query, GetUserConnection(_currentUser.Id))
			receivingDa.SelectCommand.CommandTimeout = Tm2Database.CommandTimeout
			receivingDa.Fill(receivingTable)

			For Each detailRowList As DataRow In receivingTable.Rows
				'Figure out if this has tickets
				Dim notes As String = ""
				Dim hastickets As Boolean = False
				If detailRowList.Item("completed") Then notes = "Completed. "

				' check if there are tickets
				Dim getTicketsRdr As OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), "SELECT count(*) as ticket_count " &
										"FROM receiving_tickets " &
										"WHERE voided = 0 AND receiving_purchase_order_id = " & Q(detailRowList.Item("id")))

				If getTicketsRdr.Read() Then
					hastickets = getTicketsRdr.Item("ticket_count") > 0
				End If
				getTicketsRdr.Close()

				Dim row As New HtmlTableRow
				table.Rows.Add(row)
				' row ID
				Dim cell As New HtmlTableCell()
				row.Cells.Add(cell)
				' order number
				Dim label As New Label()
				label.Text = detailRowList.Item("number")
				cell.Controls.Add(label)
				' supplier
				cell = New HtmlTableCell()
				row.Cells.Add(cell)
				label = New Label()
				label.Text = detailRowList.Item("supplier_name")
				cell.Controls.Add(label)
				' owner
				cell = New HtmlTableCell()
				row.Cells.Add(cell)
				label = New Label()
				label.Text = detailRowList.Item("owner_name")
				cell.Controls.Add(label)
				' bulk product
				cell = New HtmlTableCell()
				row.Cells.Add(cell)
				label = New Label()
				label.Text = detailRowList.Item("product_name")
				cell.Controls.Add(label)
				' created date
				cell = New HtmlTableCell()
				row.Cells.Add(cell)
				label = New Label()
				label.Text = String.Format("{0:g}", detailRowList.Item("created"))
				cell.Controls.Add(label)
				' checkbox for orders
				cell = New HtmlTableCell()
				row.Cells.Add(cell)
				Dim checkbox As New CheckBox()
				checkbox.ID = "cbxOrderId" & detailRowList.Item("id").ToString()
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
				checkboxTickets.ID = "cbxTickets" & detailRowList.Item("id").ToString()
				checkboxTickets.Visible = hastickets
				If (cbxCheckAllTickets.Checked) Then
					checkboxTickets.Checked = True
				Else
					checkboxTickets.Checked = False
				End If
				cell.Controls.Add(checkboxTickets)
			Next
		End If

		If table.Rows.Count > 1 Then
			pnlOrders.Visible = True
			btnArchive.Visible = True
		Else
			pnlOrders.Visible = False
			btnArchive.Visible = False
		End If
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
		ddlAccounts.Items.Add(New ListItem("All suppliers", Guid.Empty.ToString))
		For Each account As KaCustomerAccount In KaCustomerAccount.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND is_supplier=1" & IIf(_currentUser.OwnerId <> Guid.Empty, String.Format(" AND (owner_id={0} OR owner_id={1})", Q(Guid.Empty), Q(_currentUser.OwnerId)), ""), "name")
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

	Protected Overrides Function SaveViewState() As Object
		Dim viewState(1) As Object
		viewState(0) = _receivingPurchaseOrderIds
		viewState(1) = MyBase.SaveViewState()
		Return viewState
	End Function

	Protected Overrides Sub LoadViewState(savedState As Object)
		If savedState IsNot Nothing AndAlso CType(savedState, Object).Length > 0 Then
			_currentUser = Utilities.GetUser(Me)
			_receivingPurchaseOrderIds = savedState(0)
			PopulatePurchaseOrderList()
			MyBase.LoadViewState(savedState(1))
		Else
			MyBase.LoadViewState(savedState)
		End If
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