Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class ArchiveTickets
	Inherits System.Web.UI.Page

	Private _currentUser As KaUser
	Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
	Private _currentTableName As String = KaOrder.TABLE_NAME
	Private _customFields As New List(Of KaCustomField)
	Private _customFieldData As New List(Of KaCustomFieldData)
	Private _checkedTickets As New List(Of Guid)
	Protected _tickets As DataTable

#Region "Events"
	Protected Sub Page_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Init
		_currentUser = Utilities.GetUser(Me)
	End Sub

	Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
		Response.Cache.SetCacheability(HttpCacheability.NoCache)
		_currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Orders")
		If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")
		btnArchive.Enabled = _currentUserPermission(_currentTableName).Edit
		btnVoid.Enabled = _currentUserPermission(_currentTableName).Delete

		If (cbxShowArchived.Checked) Then
			btnArchive.Text = "Toggle Archive"
		Else
			btnArchive.Text = "Archive"
		End If
		If Not Page.IsPostBack Then
			PopulateOwnerList()
			PopulateTicketList("")
			pnlTickets.Visible = False
			btnVoid.Visible = False
			btnArchive.Visible = False
		End If
		Utilities.ConfirmBox(Me.btnArchive, "Are you sure you want to continue with the archiving process?")
		Utilities.ConfirmBox(Me.btnVoid, "Are you sure you want to continue with the voiding process?")
		lblStatus.Text = ""
	End Sub

	Protected Sub btnArchive_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnArchive.Click
		IterateThroughChildren(tblTickets)
		For Each ticketId As Guid In _checkedTickets
			Dim ticketInfo As New KaTicket(GetUserConnection(_currentUser.Id), ticketId)
			ticketInfo.Archived = Not ticketInfo.Archived
			ticketInfo.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
		Next
		btnFilter_Click(btnFilter, New EventArgs())
	End Sub

	Protected Sub btnVoid_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnVoid.Click
		IterateThroughChildren(tblTickets)
		For Each ticketId As Guid In _checkedTickets
			Dim ticket As New KaTicket(GetUserConnection(_currentUser.Id), ticketId)
			If Not ticket.Voided Then
				Dim connection As New OleDbConnection(Tm2Database.GetDbConnection())
				Dim transaction As OleDbTransaction = Nothing
				connection.Open()
				Try
					transaction = connection.BeginTransaction
					ticket.VoidTicket(connection, transaction, "Ticket " & ticket.Number & " voided by " & _currentUser.Name, Database.ApplicationIdentifier, _currentUser.Name)
					If transaction IsNot Nothing Then transaction.Commit()
				Catch ex As Exception
					If transaction IsNot Nothing Then transaction.Rollback()
					Throw ex
				Finally
					connection.Close()
				End Try
			End If
		Next
		btnFilter_Click(btnFilter, New EventArgs())
	End Sub

	Protected Sub btnFilter_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnFilter.Click
		_checkedTickets.Clear()
		CreateQueryAndPopulateTicketList()
		Page.ClientScript.RegisterStartupScript(Page.ClientScript.GetType(), "ResetScrollPosition;", "resetDotNetScrollPosition();", True)
	End Sub

	Protected Sub cbxCheckAll_CheckedChanged(sender As Object, e As EventArgs) Handles cbxCheckAll.CheckedChanged
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
				Dim ownerId As Guid = Guid.Parse(ddlOwner.SelectedValue)
				Dim showArchived As Boolean = cbxShowArchived.Checked
				Dim showVoided As Boolean = cbxShowVoided.Checked
				Dim query As String = "SELECT tickets.id, tickets.number, tickets.order_number, ticket_customer_accounts.name AS customer_name, ticket_customer_accounts.[percent] AS customer_percent, " &
					"owners.name AS owner_name, tickets.loaded_at, tickets.archived, tickets.voided " &
					"FROM tickets " &
					"INNER JOIN ticket_customer_accounts ON ticket_customer_accounts.ticket_id = tickets.id " &
					"INNER JOIN owners ON owners.id = tickets.owner_id " &
					IIf(showArchived, "WHERE tickets.archived IN (0,1) ", "WHERE tickets.archived = 0 ") & " " &
					IIf(String.IsNullOrEmpty(tbxFromDate.Value), "", "AND tickets.loaded_at >= " & Q(fromDate) & " AND tickets.loaded_at <= " & Q(toDate) & " ") &
					IIf(ownerId.Equals(Guid.Empty), "", "AND tickets.owner_id = " & Q(ownerId) & " ") &
					IIf(showVoided, "AND tickets.voided IN (0,1) ", "AND tickets.voided = 0 ") & " " &
					IIf(tbxOrderNumberContains.Text.Length = 0, "", $"AND {KaTicket.TABLE_NAME}.order_number LIKE {Database.Q($"%{tbxOrderNumberContains.Text.Trim}%")} ") &
					IIf(tbxTicketNumberContains.Text.Length = 0, "", $"AND {KaTicket.TABLE_NAME}.number LIKE {Database.Q($"%{tbxTicketNumberContains.Text.Trim}%")} ") &
					"ORDER BY tickets.loaded_at ASC, tickets.number ASC"
				PopulateTicketList(query)
			End If
		End If
	End Sub
#End Region

#Region " Tickets Table "
	Protected Sub PopulateTicketList(query As String)
		If (String.IsNullOrEmpty(query)) Then ' create an empty table
			query = "SELECT tickets.id, tickets.number, tickets.order_number, '' AS customer_name, 100 AS customer_percent, '' AS owner_name, " &
					"tickets.loaded_at, tickets.archived, tickets.voided " &
					"FROM tickets " &
					"WHERE tickets.owner_id = " & Q(Guid.NewGuid)
		End If
		_tickets = New DataTable
		Dim da As OleDbDataAdapter = New OleDbDataAdapter(query, GetUserConnection(_currentUser.Id))
		da.Fill(_tickets)
		PopulateTicketTable()
	End Sub

	Protected Sub PopulateTicketTable()
		Dim table As HtmlTable = Page.FindControl("tblTickets")
		If table Is Nothing Then Exit Sub
		Do While table.Rows.Count > 1
			table.Rows.RemoveAt(1)
		Loop
		Dim tickets As List(Of Guid) = New List(Of Guid)
		'Dim row As OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), Query)
		'Do While row.Read()
		For Each ticketRow As DataRow In _tickets.Rows
			Dim ticketId As Guid = ticketRow.Item("id")
			If tickets.Contains(ticketId) Then
				Dim controlName As String = "lblAccountTicketId" & ticketId.ToString()
				Dim accountLbl As Label = CType(FindControl(controlName), Label)
				accountLbl.Text = accountLbl.Text & "</br>" & ticketRow.Item("customer_name") & String.Format(" {0:0.00}%", ticketRow.Item("customer_percent"))
			Else
				tickets.Add(ticketId)
				Dim row As New HtmlTableRow
				table.Rows.Add(row)
				' row ID
				Dim cell As New HtmlTableCell()
				row.Cells.Add(cell)
				' ticket number
				Dim label As New Label()
				label.Text = ticketRow.Item("number")
				cell.Controls.Add(label)
				' order number
				cell = New HtmlTableCell()
				row.Cells.Add(cell)
				label = New Label()
				label.Text = ticketRow.Item("order_number")
				cell.Controls.Add(label)
				' customer
				cell = New HtmlTableCell()
				row.Cells.Add(cell)
				label = New Label()
				label.ID = "lblAccountTicketId" & ticketRow.Item("id").ToString()
				label.Text = ticketRow.Item("customer_name") & String.Format(" {0:0.00}%", ticketRow.Item("customer_percent"))
				cell.Controls.Add(label)
				' owner
				cell = New HtmlTableCell()
				row.Cells.Add(cell)
				label = New Label()
				label.Text = ticketRow.Item("owner_name")
				cell.Controls.Add(label)
				' loaded date
				cell = New HtmlTableCell()
				row.Cells.Add(cell)
				label = New Label()
				label.Text = ticketRow.Item("loaded_at")
				cell.Controls.Add(label)
				' checkbox
				cell = New HtmlTableCell()
				row.Cells.Add(cell)
				Dim checkbox As New CheckBox()
				checkbox.ID = "cbxTicketId" & ticketRow.Item("id").ToString()
				If (ticketRow.Item("voided").ToString.Equals("True")) Then
					checkbox.Enabled = False
				Else
					If (cbxCheckAll.Checked) Then
						checkbox.Checked = True
					Else
						checkbox.Checked = False
					End If
				End If
				cell.Controls.Add(checkbox)
				If ticketRow.Item("archived") <> cbxShowArchived.Checked Then
					row.Visible = False
				ElseIf ticketRow.Item("voided") <> cbxShowVoided.Checked Then
					row.Visible = False
				End If
			End If
		Next

		If tblTickets.Rows.Count > 1 Then
			pnlTickets.Visible = True
			btnVoid.Visible = True
			btnArchive.Visible = True
		Else
			pnlTickets.Visible = False
			btnVoid.Visible = False
			btnArchive.Visible = False
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

	Private Sub PopulateOwnerList()
		ddlOwner.Items.Clear()
		If _currentUser.OwnerId.Equals(Guid.Empty) Then ddlOwner.Items.Add(New ListItem("All owners", Guid.Empty.ToString()))
		For Each r As KaOwner In KaOwner.GetAll(GetUserConnection(_currentUser.Id), "deleted=0" & IIf(_currentUser.OwnerId.Equals(Guid.Empty), "", " AND id=" & Q(_currentUser.OwnerId)), "name ASC")
			ddlOwner.Items.Add(New ListItem(r.Name, r.Id.ToString()))
		Next
	End Sub

	Private Sub EmptyTable()
		Dim table As HtmlTable = Page.FindControl("tblTickets")
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
		viewState(0) = _tickets
		viewState(1) = MyBase.SaveViewState()
		Return viewState
	End Function

	Protected Overrides Sub LoadViewState(savedState As Object)
		'Getting the dropdown list value from view state.
		If savedState IsNot Nothing AndAlso CType(savedState, Object).Length > 1 Then
			Dim viewState As Object() = savedState
			_tickets = viewState(0)
			PopulateTicketTable()
			MyBase.LoadViewState(viewState(1))
		Else
			MyBase.LoadViewState(savedState)
		End If
	End Sub
End Class