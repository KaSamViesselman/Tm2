Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class ArchiveReceivingPurchaseOrderTickets
	Inherits System.Web.UI.Page
	Private _currentUser As KaUser
	Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
	Private _currentTableName As String = KaOrder.TABLE_NAME
	Private _customFields As New List(Of KaCustomField)
	Private _customFieldData As New List(Of KaCustomFieldData)
	Private _checkedTickets As New List(Of Guid)
	Private _receivingPurchaseOrderTicketIds As New List(Of Guid)

#Region "Events"
	Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
		Response.Cache.SetCacheability(HttpCacheability.NoCache)
		_currentUser = Utilities.GetUser(Me)
		_currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Orders")
		If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")
		lblStatus.Text = ""
		btnArchive.Enabled = _currentUserPermission(_currentTableName).Edit
		btnVoid.Enabled = _currentUserPermission(_currentTableName).Delete

		If (cbxShowArchived.Checked) Then
			btnArchive.Text = "Toggle Archive"
		Else
			btnArchive.Text = "Archive"
		End If
		If Not Page.IsPostBack Then
			PopulateSuppliers()
			PopulateFacilityList()
			PopulateOwnerList(_currentUser)
			pnlTickets.Visible = False
			btnArchive.Visible = False
			btnVoid.Visible = False
		End If
		Utilities.ConfirmBox(Me.btnArchive, "Are you sure you want to continue with the archiving process?")
		Utilities.ConfirmBox(Me.btnVoid, "Are you sure you want to continue with the voiding process?")
	End Sub

	Protected Sub btnArchive_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnArchive.Click
		IterateThroughChildren(Me)
		Dim ticketsVoided As String = ""
		For Each ticketId As Guid In _checkedTickets
			Dim ticketInfo As New KaReceivingTicket(GetUserConnection(_currentUser.Id), ticketId)
			ticketInfo.Archived = Not ticketInfo.Archived
			ticketInfo.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
			ticketsVoided &= ", " & ticketInfo.Number
		Next

		CreateQueryAndPopulateTicketList()
		If ticketsVoided.Length > 2 Then
			If cbxShowArchived.Checked Then
				lblStatus.Text = "Selected tickets unarchived: " & ticketsVoided.Substring(2)
			Else
				lblStatus.Text = "Selected tickets archived: " & ticketsVoided.Substring(2)
			End If
		End If
	End Sub

	Protected Sub btnVoid_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnVoid.Click
		IterateThroughChildren(Me)
		Dim ticketsVoided As String = ""
		For Each ticketId As Guid In _checkedTickets
			Dim ticketInfo As New KaReceivingTicket(GetUserConnection(_currentUser.Id), ticketId)
			If Not ticketInfo.Voided Then
				Dim connection As New OleDbConnection(Tm2Database.GetDbConnection())
				Dim transaction As OleDbTransaction = Nothing
				connection.Open()
				Try
					transaction = connection.BeginTransaction
					ticketInfo.VoidTicket(connection, transaction, "Ticket " & ticketInfo.Number & " voided by " & _currentUser.Name, Database.ApplicationIdentifier, _currentUser.Name)
					If transaction IsNot Nothing Then transaction.Commit()
					ticketsVoided &= ", " & ticketInfo.Number
				Catch ex As Exception
					If transaction IsNot Nothing Then transaction.Rollback()
					Throw ex
				Finally
					connection.Close()
				End Try
			End If
		Next
		CreateQueryAndPopulateTicketList()
		If ticketsVoided.Length > 2 Then lblStatus.Text = "Selected tickets voided: " & ticketsVoided.Substring(2)
	End Sub

	Protected Sub btnFilter_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnFilter.Click
		_checkedTickets.Clear()
		CreateQueryAndPopulateTicketList()
	End Sub

	Protected Sub cbxCheckAll_CheckedChanged(sender As Object, e As EventArgs) Handles cbxCheckAll.CheckedChanged
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
			_receivingPurchaseOrderTicketIds.Clear()
			Dim supplierId As Guid = Guid.Empty
			Guid.TryParse(ddlAccounts.SelectedValue, supplierId)
			Dim ownerId As Guid = Guid.Empty
			Guid.TryParse(ddlOwner.SelectedValue, ownerId)
			Dim locationId As Guid = Guid.Empty
			Guid.TryParse(ddlFacility.SelectedValue, locationId)

			_receivingPurchaseOrderTicketIds.Clear()
			Dim query As String = "SELECT id " &
				"FROM receiving_tickets " &
				"WHERE " &
					String.Format("archived = {0} ", Q(cbxShowArchived.Checked)) &
					String.Format("AND voided = {0} ", Q(cbxShowVoided.Checked)) &
					IIf(String.IsNullOrEmpty(tbxFromDate.Value), "", "AND date_of_delivery >= " & Q(fromDate) & " AND date_of_delivery <= " & Q(toDate) & " ") &
					IIf(supplierId.Equals(Guid.Empty), "", "AND supplier_account_id = " & Q(supplierId) & " ") &
					IIf(locationId.Equals(Guid.Empty), "", "AND location_id = " & Q(locationId) & " ") &
					IIf(ownerId.Equals(Guid.Empty), "", "AND owner_id = " & Q(ownerId) & " ") &
					IIf(_currentUser.OwnerId.Equals(Guid.Empty), "", String.Format("AND ((owner_id = {0}) OR (owner_id = {1})) ", Q(_currentUser.OwnerId), Q(Guid.Empty))) &
					IIf(tbxOrderNumberContains.Text.Length = 0, "", $"AND purchase_order_number LIKE {Database.Q($"%{tbxOrderNumberContains.Text.Trim}%")} ") &
					IIf(tbxTicketNumberContains.Text.Length = 0, "", $"AND number LIKE {Database.Q($"%{tbxTicketNumberContains.Text.Trim}%")} ")

			Dim receivingRdr As OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), query)
			Do While receivingRdr.Read()
				_receivingPurchaseOrderTicketIds.Add(receivingRdr.Item("Id"))
			Loop
			receivingRdr.Close()

			DisplayJavaScriptMessage("ResetScrollPosition;", "resetDotNetScrollPosition();", True)
			PopulateTicketList()
		End If
	End Sub
#End Region

#Region " Tickets Table "
	Protected Sub PopulateTicketList()
		Dim table As HtmlTable = Page.FindControl("tblTickets")
		If table Is Nothing Then Exit Sub
		Do While table.Rows.Count > 1
			table.Rows.RemoveAt(1)
		Loop
		If _receivingPurchaseOrderTicketIds.Count > 0 Then
			Dim ticketIds As String = ""
			For Each ticketId As Guid In _receivingPurchaseOrderTicketIds
				ticketIds &= "," & Q(ticketId)
			Next
			Dim query As String = String.Format("SELECT id, number, purchase_order_number, supplier_account_name, bulk_product_name, owner_name, date_of_delivery, archived, voided " &
			"FROM receiving_tickets " &
			"WHERE (id IN ({0})) " &
			"ORDER BY date_of_delivery", ticketIds.Substring(1))

			Dim tickets As List(Of Guid) = New List(Of Guid)
			Dim getTicketsRdr As OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), query)
			Do While getTicketsRdr.Read()
				Dim ticketId As Guid = getTicketsRdr.Item("id")

				tickets.Add(ticketId)
				Dim row As New HtmlTableRow
				table.Rows.Add(row)
				' row ID
				Dim cell As New HtmlTableCell()
				row.Cells.Add(cell)
				' ticket number
				Dim label As New Label()
				label.Text = getTicketsRdr.Item("number")
				cell.Controls.Add(label)
				' order number
				cell = New HtmlTableCell()
				row.Cells.Add(cell)
				label = New Label()
				label.Text = getTicketsRdr.Item("purchase_order_number")
				cell.Controls.Add(label)
				' customer
				cell = New HtmlTableCell()
				row.Cells.Add(cell)
				label = New Label()
				label.ID = "lblAccountTicketId" & getTicketsRdr.Item("id").ToString()
				label.Text = getTicketsRdr.Item("supplier_account_name")
				cell.Controls.Add(label)
				' owner
				cell = New HtmlTableCell()
				row.Cells.Add(cell)
				label = New Label()
				label.Text = getTicketsRdr.Item("owner_name")
				cell.Controls.Add(label)
				'bulk_product_name
				cell = New HtmlTableCell()
				row.Cells.Add(cell)
				label = New Label()
				label.Text = getTicketsRdr.Item("bulk_product_name")
				cell.Controls.Add(label)
				' loaded date
				cell = New HtmlTableCell()
				row.Cells.Add(cell)
				label = New Label()
				label.Text = String.Format("{0:g}", getTicketsRdr.Item("date_of_delivery"))
				cell.Controls.Add(label)
				' checkbox
				cell = New HtmlTableCell()
				row.Cells.Add(cell)
				Dim checkbox As New CheckBox()
				checkbox.ID = "cbxTicketId" & getTicketsRdr.Item("id").ToString()
				If getTicketsRdr.Item("voided") Then
					checkbox.Enabled = False
				Else
					checkbox.Checked = cbxCheckAll.Checked
				End If
				cell.Controls.Add(checkbox)
			Loop
			getTicketsRdr.Close()
		End If
		If table.Rows.Count > 1 Then
			pnlTickets.Visible = True
			btnArchive.Visible = True
			btnVoid.Visible = True
		Else
			pnlTickets.Visible = False
			btnArchive.Visible = False
			btnVoid.Visible = False
		End If
	End Sub
#End Region

	Private Sub IterateThroughChildren(parent As Control)
		For Each c As Control In parent.Controls
			If (TypeOf c Is CheckBox AndAlso c.ID.StartsWith("cbxTicketId")) Then
				Dim checkbox As CheckBox = CType(c, CheckBox)
				Dim ticketId As Guid = Guid.Parse(checkbox.ID.Substring(11))
				If (checkbox.Checked) Then
					_checkedTickets.Add(ticketId)
				Else
					_checkedTickets.Remove(ticketId)
				End If
			End If

			If (c.Controls.Count > 0) Then
				IterateThroughChildren(c)
			End If
		Next
	End Sub

	Private Sub PopulateSuppliers()
		ddlAccounts.Items.Clear()
		ddlAccounts.Items.Add(New ListItem("All suppliers", Guid.Empty.ToString))
		For Each account As KaSupplierAccount In KaSupplierAccount.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND is_supplier=1" & IIf(_currentUser.OwnerId <> Guid.Empty, String.Format(" AND (owner_id={0} OR owner_id={1})", Q(Guid.Empty), Q(_currentUser.OwnerId)), ""), "name")
			ddlAccounts.Items.Add(New ListItem(account.Name, account.Id.ToString))
		Next
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

	Private Sub PopulateOwnerList(ByVal currentUser As KaUser)
		ddlOwner.Items.Clear()
		If currentUser.OwnerId.Equals(Guid.Empty) Then ddlOwner.Items.Add(New ListItem("All owners", Guid.Empty.ToString()))
		For Each r As KaOwner In KaOwner.GetAll(GetUserConnection(currentUser.Id), "deleted=0" & IIf(currentUser.OwnerId.Equals(Guid.Empty), "", " AND id=" & Q(currentUser.OwnerId)), "name ASC")
			ddlOwner.Items.Add(New ListItem(r.Name, r.Id.ToString()))
		Next
	End Sub

	Protected Overrides Function SaveViewState() As Object
		Dim viewState(1) As Object
		viewState(0) = _receivingPurchaseOrderTicketIds
		viewState(1) = MyBase.SaveViewState()
		Return viewState
	End Function

	Protected Overrides Sub LoadViewState(savedState As Object)
		If savedState IsNot Nothing AndAlso CType(savedState, Object).Length > 0 Then
			_currentUser = Utilities.GetUser(Me)
			_receivingPurchaseOrderTicketIds = savedState(0)
			PopulateTicketList()
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