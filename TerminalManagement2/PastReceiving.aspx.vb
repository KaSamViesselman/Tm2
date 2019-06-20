Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class PastReceiving : Inherits System.Web.UI.Page
	Private _currentUser As KaUser
	Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
	Private _currentTableName As String = KaReceivingPurchaseOrder.TABLE_NAME

#Region "Events"
	Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
		Response.Cache.SetCacheability(HttpCacheability.NoCache)
		_currentUser = Utilities.GetUser(Me)
		_currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "PurchaseOrders")
		If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")

		If Not Page.IsPostBack Then
			btnMarkIncomplete.Enabled = _currentUserPermission(_currentTableName).Edit
			SetTextboxMaxLengths()
			Utilities.ConfirmBox(btnMarkIncomplete, "Are you sure that you want to mark this receiving purchase order as incomplete?")
			Utilities.ConfirmBox(Me.btnVoidTicket, "Are you sure you want to void this ticket?")
			PopulateFacilityList()
			If Page.Request("ReceivingPurchaseOrderId") Is Nothing Then
				Try
					ddlFacilityFilter.SelectedValue = KaSetting.GetSetting(GetUserConnection(_currentUser.Id), "PurchaseOrdersPage:" & _currentUser.Id.ToString & "/LastFacilityUsed", Guid.Empty.ToString())
				Catch ex As ArgumentOutOfRangeException

				End Try
			Else
				ddlFacilityFilter.SelectedIndex = 0
			End If
			PopulatePastReceivingPurchaseOrders(_currentUser)
			Dim receivingPurchaseOrderId As Guid = Guid.Empty
			Guid.TryParse(Page.Request("ReceivingPurchaseOrderId"), receivingPurchaseOrderId)
			Try
				ddlPastReceivingPurchaseOrders.SelectedValue = receivingPurchaseOrderId.ToString()
			Catch ex As ArgumentOutOfRangeException

			End Try

			ddlPastReceivingPurchaseOrders_SelectedIndexChanged(ddlPastReceivingPurchaseOrders, New EventArgs())
		ElseIf Page.IsPostBack And Request("__EVENTARGUMENT").StartsWith("ArchiveTickets") Then
			ArchiveTickets(Request("__EVENTARGUMENT").Replace("ArchiveTickets('", "").Replace("')", ""))
		End If
		pnlReceivingPoTicketUsage.Attributes("display") = "none"
		pnlReceivingTicketUsage.Attributes("display") = "none"
	End Sub

	Protected Sub ddlPastReceivingPurchaseOrders_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles ddlPastReceivingPurchaseOrders.SelectedIndexChanged
		PopulateReceivingPurchaseOrder()
		SetControlUsabilityFromPermissions()
	End Sub

	Protected Sub btnMarkIncomplete_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnMarkIncomplete.Click
		With New KaReceivingPurchaseOrder(GetUserConnection(_currentUser.Id), Guid.Parse(ddlPastReceivingPurchaseOrders.SelectedValue))
			.Completed = False
			.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
			PopulatePastReceivingPurchaseOrders(Utilities.GetUser(Me))
			PopulateReceivingPurchaseOrder()
		End With
	End Sub

	Protected Sub btnFind_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnFind.Click
		If tbxFind.Text.Trim().Length > 0 Then
			Dim i As Integer = ddlPastReceivingPurchaseOrders.SelectedIndex
			Do
				If i + 1 = ddlPastReceivingPurchaseOrders.Items.Count Then i = 0 Else i += 1
				If ddlPastReceivingPurchaseOrders.Items(i).Text.Trim().ToLower().Contains(tbxFind.Text.Trim().ToLower()) Then
					ddlPastReceivingPurchaseOrders.SelectedIndex = i
					PopulateReceivingPurchaseOrder()
					Exit Sub
				End If
			Loop While i <> ddlPastReceivingPurchaseOrders.SelectedIndex
			DisplayJavaScriptMessage("InvalidNumber", Utilities.JsAlert("Record not found in past receiving purchase orders where number = " & tbxFind.Text))
		End If
	End Sub
#End Region

	Private Sub PopulateFacilityList()
		ddlFacilityFilter.Items.Clear()
		ddlFacilityFilter.Items.Add(New ListItem("All facilities", Guid.Empty.ToString()))
		For Each u As KaLocation In KaLocation.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
			ddlFacilityFilter.Items.Add(New ListItem(u.Name, u.Id.ToString))
		Next
	End Sub

	Private Sub PopulatePastReceivingPurchaseOrders(ByVal currentUser As KaUser)
		ddlPastReceivingPurchaseOrders.Items.Clear()
		ddlPastReceivingPurchaseOrders.Items.Add(New ListItem("", Guid.Empty.ToString()))
		For Each r As KaReceivingPurchaseOrder In KaReceivingPurchaseOrder.GetAll(GetUserConnection(currentUser.Id), "deleted = 0 AND completed = 1" &
						IIf(cbxIncludeArchived.Checked, "", " AND archived = 0 ") &
						IIf(currentUser.OwnerId = Guid.Empty, "", " AND owner_id=" & Q(currentUser.OwnerId)) &
						IIf(ddlFacilityFilter.SelectedIndex = 0, "", String.Format("AND (bulk_product_id IN (SELECT bulk_product_id FROM product_bulk_products WHERE (deleted = 0) AND (location_id = {0}))) ", Q(Guid.Parse(ddlFacilityFilter.SelectedValue)))), "number ASC")
			ddlPastReceivingPurchaseOrders.Items.Add(New ListItem(r.Number, r.Id.ToString()))
		Next
	End Sub

	Private Sub PopulateReceivingPurchaseOrder()
		Dim markIncompleteEnabled As Boolean = (_currentUserPermission(_currentTableName).Edit)
		Dim markIncompleteDisabledReasons As String = ""
		Dim receivingPo As KaReceivingPurchaseOrder = Nothing
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Try
			receivingPo = New KaReceivingPurchaseOrder(connection, Guid.Parse(ddlPastReceivingPurchaseOrders.SelectedValue))
			With receivingPo
				Try
					If .OwnerId = Guid.Empty Then lblOwner.Text = "" Else lblOwner.Text = New KaOwner(connection, .OwnerId).Name
				Catch ex As RecordNotFoundException
					lblOwner.Text = ""
					DisplayJavaScriptMessage("InvalidOwnerId", Utilities.JsAlert("Record not found in owners where ID = " & .OwnerId.ToString()))
				End Try
				Try
					If .SupplierAccountId = Guid.Empty Then lblSupplierAccount.Text = "" Else lblSupplierAccount.Text = New KaSupplierAccount(connection, .SupplierAccountId).Name
				Catch ex As RecordNotFoundException
					lblSupplierAccount.Text = ""
					DisplayJavaScriptMessage("InvalidSupplierId", Utilities.JsAlert("Record not found in supplier accounts where ID = " & .SupplierAccountId.ToString()))
				End Try
				Try
					If .BulkProductId = Guid.Empty Then lblBulkProduct.Text = "" Else lblBulkProduct.Text = New KaBulkProduct(connection, .BulkProductId).Name
				Catch ex As RecordNotFoundException
					lblBulkProduct.Text = ""
					DisplayJavaScriptMessage("InvalidBulkProductId", Utilities.JsAlert("Record not found in bulk products where ID = " & .BulkProductId.ToString()))
				End Try
				Dim unitInfo As New KaUnit()
				Try
					unitInfo = New KaUnit(connection, .UnitId)
				Catch ex As RecordNotFoundException
					unitInfo.Abbreviation = ""
				End Try
				lblPurchased.Text = Format(.Purchased, unitInfo.UnitPrecision) + " " + unitInfo.Abbreviation
				lblDelivered.Text = Format(.Delivered, unitInfo.UnitPrecision) + " " + unitInfo.Abbreviation

				If .Archived Then
					markIncompleteEnabled = False
					markIncompleteDisabledReasons = "Order archived.  "
				End If

				Try
					Dim i As New KaInterface(connection, .InterfaceId)
					If i.Deleted Then
						markIncompleteEnabled = False
						markIncompleteDisabledReasons &= IIf(markIncompleteDisabledReasons.Length > 0, vbCrLf, "") & "Interface " & i.Name & " deleted.  "
					End If
				Catch ex As RecordNotFoundException
				End Try
				btnArchive.Enabled = (_currentUserPermission(_currentTableName).Edit) AndAlso Not .Archived
				btnUnarchive.Enabled = (_currentUserPermission(_currentTableName).Edit) AndAlso .Archived

				ClearTickets(True)
				PopulateReceivingTickets()
				ddlReceivingTickets_SelectedIndexChanged(ddlReceivingTickets, New EventArgs())
			End With
			pnlMain.Visible = True
		Catch ex As Exception
			receivingPo = Nothing
			pnlMain.Visible = False
			markIncompleteEnabled = False
			btnArchive.Enabled = False
			btnUnarchive.Enabled = False
			btnShowTicketUsages.Visible = False
		End Try
		GetReceivingPoUsages(receivingPo)
		btnMarkIncomplete.Enabled = markIncompleteEnabled
		btnMarkIncomplete.ToolTip = IIf(btnMarkIncomplete.Enabled, "", "Cannot mark incomplete due to: " & vbCrLf & Server.HtmlEncode(markIncompleteDisabledReasons))
	End Sub

	Private Sub btnPrintPo_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnPrintPo.Click
		If Guid.Parse(ddlPastReceivingPurchaseOrders.SelectedValue) <> Guid.Empty Then
			DisplayJavaScriptMessage("PrintPo", Utilities.JsWindowOpen("ReceivingPFV.aspx?po_id=" & ddlPastReceivingPurchaseOrders.SelectedValue)) ', "toolbar=yes,menubar=yes,scrollbars=yes,resizable=yes,width=700,height=500,top=50,left=50", True))
		End If
	End Sub

	Private Sub GetReceivingPoUsages(receivingPo As KaReceivingPurchaseOrder)
		Dim receivingPoUsageList As ArrayList = GetReceivingPoUsagesTable(receivingPo)
		If receivingPoUsageList.Count > 1 Then
			litReceivingPoTicketUsage.Text = KaReports.GetTableHtml("", "", receivingPoUsageList, False, "class=""label, input""", "", New List(Of String), "", New List(Of String))
		Else
			litReceivingPoTicketUsage.Text = ""
		End If
		btnShowPoUsages.Visible = receivingPoUsageList.Count > 1
	End Sub

	Private Function GetReceivingPoUsagesTable(receivingPo As KaReceivingPurchaseOrder) As ArrayList
		Dim receivingTicketUsageList As ArrayList = New ArrayList
		If Tm2Database.SystemItemTraceabilityEnabled AndAlso receivingPo IsNot Nothing Then
			Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
			Dim tickets As Dictionary(Of Guid, KaTicket) = receivingPo.GetTicketUsagesForMovements(connection, Nothing, chkShowVoidedTickets.Checked)
			For Each ticketId As Guid In tickets.Keys
				Dim ticket As KaTicket = tickets(ticketId)
				Dim webTicketAddress As String = $"<a href=""Receipts.aspx?TicketId={ticket.Id.ToString()}"" target=""_blank"">{System.Web.HttpUtility.HtmlEncode(Receipts.GetTicketNumber(GetUserConnection(_currentUser.Id), Nothing, ticket))}</a>"
				Dim orderAddress As String
				Try
					If New KaOrder(connection, ticket.OrderId).Completed Then
						orderAddress = "<a href=""PastOrders.aspx?OrderId="
					Else
						orderAddress = "<a href=""Orders.aspx?OrderId="
					End If
					orderAddress &= ticket.OrderId.ToString & """ target=""_blank"">" & System.Web.HttpUtility.HtmlEncode(ticket.OrderNumber) & "</a>"
				Catch ex As RecordNotFoundException
					orderAddress = ticket.OrderNumber
				End Try
				receivingTicketUsageList.Add(New ArrayList({String.Format(ticket.LoadedAt, "G"), webTicketAddress, orderAddress}))
			Next
			If receivingTicketUsageList.Count > 0 Then receivingTicketUsageList.Insert(0, New ArrayList({"Loaded at", "Ticket", "Order number"}))
		End If

		Return receivingTicketUsageList
	End Function


#Region "Receiving Tickets"
	Private Sub PopulateReceivingTickets()
		Dim currentTicketId As String = ddlReceivingTickets.SelectedValue
		ddlReceivingTickets.Items.Clear()
		Dim li As ListItem = New ListItem
		li.Text = ""
		li.Value = Guid.Empty.ToString
		ddlReceivingTickets.Items.Add(li)
		Dim ticketsTable As New DataTable
		If Guid.Parse(ddlPastReceivingPurchaseOrders.SelectedValue) <> Guid.Empty Then
			Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
			Dim ticketsDA As New OleDbDataAdapter("SELECT  id, number, delivered, unit_id, density, weight_unit_id, volume_unit_id, date_of_delivery, linked_tickets_id, panel_id, voided, owner_id " &
					  "FROM receiving_tickets " &
					  "WHERE (deleted = 0) " &
						  "AND (archived = 0) " &
						  "AND receiving_purchase_order_id = " & Q(Guid.Parse(ddlPastReceivingPurchaseOrders.SelectedValue)) & " " &
					  "ORDER BY number asc, date_of_delivery asc", connection)
			If Tm2Database.CommandTimeout > 0 Then ticketsDA.SelectCommand.CommandTimeout = Tm2Database.CommandTimeout
			ticketsDA.Fill(ticketsTable)
			' If Boolean.Parse(KaSetting.GetSetting(GetUserConnection(currentUser.Id), "Use_Receiving_PO_Web_Ticket", "False")) AndAlso _
			'If Reports.ReceivingPoWebTicketUrlForOwner(connection, .Item("date_of_delivery")).Trim.Length > 0 Then
			Dim dateOfDelivery As DateTime
			Dim ticketNumber As String
			Dim deliveredQty As Double
			Dim unitId As Guid
			Dim linkedId As Guid
			Dim rowCounter As Integer = 0
			Do While rowCounter < ticketsTable.Rows.Count
				With ticketsTable.Rows(rowCounter)
					If chkShowVoidedTickets.Checked OrElse Not .Item("voided") Then
						dateOfDelivery = .Item("date_of_delivery")
						ticketNumber = .Item("number")
						deliveredQty = .Item("delivered")
						unitId = .Item("unit_id")
						linkedId = .Item("linked_tickets_id")
						If Not .Item("linked_tickets_id").Equals(Guid.Empty) Then
							' Grouped ticket
							Dim secondRowCounter As Integer = rowCounter + 1
							Do While secondRowCounter < ticketsTable.Rows.Count
								With ticketsTable.Rows(secondRowCounter)
									If .Item("linked_tickets_id").Equals(linkedId) Then
										' Grouped ticket
										deliveredQty += KaUnit.Convert(connection, New KaQuantity(.Item("delivered"), .Item("unit_id")), New KaRatio(.Item("density"), .Item("weight_unit_id"), .Item("volume_unit_id")), unitId).Numeric
										ticketsTable.Rows.RemoveAt(secondRowCounter)
									Else
										secondRowCounter += 1
									End If
								End With
							Loop
						End If
						li = New ListItem
						li.Text = ticketNumber & IIf(.Item("voided"), " (voided)", " {" & dateOfDelivery & " - " & Format(deliveredQty, KaPanel.GetUnitPrecision(connection, .Item("panel_id"), unitId)) & " " & New KaUnit(connection, unitId).Abbreviation & "}")
						li.Value = .Item("id").ToString
						ddlReceivingTickets.Items.Add(li)
					End If
				End With
				rowCounter += 1
			Loop
		End If

		pnlTickets.Visible = (ticketsTable.Rows.Count > 0)
		Try
			ddlReceivingTickets.SelectedValue = currentTicketId
		Catch ex As ArgumentOutOfRangeException
			ddlReceivingTickets.SelectedIndex = 0
		End Try
		ddlReceivingTickets_SelectedIndexChanged(ddlReceivingTickets, New EventArgs())
	End Sub

	Private Sub ddlReceivingTickets_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlReceivingTickets.SelectedIndexChanged
		ClearTickets(False)
		Try
			Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
			Dim receivingTicket As KaReceivingTicket = New KaReceivingTicket(connection, Guid.Parse(ddlReceivingTickets.SelectedValue))
			Dim htmlAddress As String = Receiving.GetReceivingTicketHtmlAddress(receivingTicket, _currentUser)
			If htmlAddress.Trim.Length > 0 Then
				If Utilities.ConvertWebPageUrlDomainToRequestedPagesDomain(connection) Then htmlAddress = Tm2Database.GetUrlInCurrentDomain(Tm2Database.GetCurrentPageUrlWithOriginalPort(Me.Request), htmlAddress)
				frmTicket.Attributes("src") = htmlAddress
				frmTicket.Visible = True
				pnlTextTicket.Visible = False
			Else
				litTextTicketOutput.Text = receivingTicket.ReceiptTicketText(connection).Replace(Environment.NewLine, "<br>")
				frmTicket.Visible = False
				pnlTextTicket.Visible = True
			End If
			btnPrintTicket.Enabled = True
			GetReceivingTicketUsages(receivingTicket)
			SetControlUsabilityFromPermissions()
		Catch ex As RecordNotFoundException
			btnPrintTicket.Enabled = False
			btnVoidTicket.Enabled = False
			btnShowTicketUsages.Visible = False
			frmTicket.Visible = False
			pnlTextTicket.Visible = False
		End Try
	End Sub

	Private Sub ClearTickets(ByVal resetDropDown As Boolean)
		If resetDropDown AndAlso ddlReceivingTickets.Items.Count > 0 Then ddlReceivingTickets.SelectedIndex = 0
		btnPrintTicket.Enabled = False
		frmTicket.Visible = False
		pnlTextTicket.Visible = False
	End Sub

	Private Sub GetReceivingTicketUsages(receivingTicket As KaReceivingTicket)
		Dim receivingTicketUsageList As ArrayList = GetReceivingTicketUsagesTable(receivingTicket)
		If receivingTicketUsageList.Count > 1 Then
			litReceivingTicketUsage.Text = KaReports.GetTableHtml("", "", receivingTicketUsageList, False, "class=""label, input""", "", New List(Of String), "", New List(Of String))
		Else
			litReceivingTicketUsage.Text = ""
		End If
		btnShowTicketUsages.Visible = receivingTicketUsageList.Count > 1
	End Sub

	Private Function GetReceivingTicketUsagesTable(receivingTicket As KaReceivingTicket) As ArrayList
		Dim receivingTicketUsageList As ArrayList = New ArrayList
		If Tm2Database.SystemItemTraceabilityEnabled AndAlso receivingTicket IsNot Nothing Then
			Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
			Dim tickets As Dictionary(Of Guid, KaTicket) = receivingTicket.GetTicketUsagesForMovement(connection, Nothing, chkShowVoidedTickets.Checked)
			For Each ticketId As Guid In tickets.Keys
				Dim ticket As KaTicket = tickets(ticketId)
				Dim webTicketAddress As String = $"<a href=""Receipts.aspx?TicketId={ticket.Id.ToString()}"" target=""_blank"">{System.Web.HttpUtility.HtmlEncode(Receipts.GetTicketNumber(GetUserConnection(_currentUser.Id), Nothing, ticket))}</a>"
				Dim orderAddress As String
				Try
					If New KaOrder(connection, ticket.OrderId).Completed Then
						orderAddress = "<a href=""PastOrders.aspx?OrderId="
					Else
						orderAddress = "<a href=""Orders.aspx?OrderId="
					End If
					orderAddress &= ticket.OrderId.ToString & """ target=""_blank"">" & System.Web.HttpUtility.HtmlEncode(ticket.OrderNumber) & "</a>"
				Catch ex As RecordNotFoundException
					orderAddress = ticket.OrderNumber
				End Try
				receivingTicketUsageList.Add(New ArrayList({String.Format(ticket.LoadedAt, "G"), webTicketAddress, orderAddress}))
			Next
			If receivingTicketUsageList.Count > 0 Then receivingTicketUsageList.Insert(0, New ArrayList({"Loaded at", "Ticket", "Order number"}))
		End If

		Return receivingTicketUsageList
	End Function

	Private Sub btnPrintTicket_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnPrintTicket.Click
		Dim receivingTicket As KaReceivingTicket = New KaReceivingTicket(GetUserConnection(_currentUser.Id), Guid.Parse(ddlReceivingTickets.SelectedValue))

		Dim htmlAddress As String = Receiving.GetReceivingTicketHtmlAddress(receivingTicket, _currentUser)
		If htmlAddress.Trim().Length > 0 Then
			DisplayJavaScriptMessage("PrintTicket", Utilities.JsWindowOpen(htmlAddress)) ', "toolbar=yes,menubar=yes,scrollbars=yes,resizable=yes,width=700,height=500,top=50,left=50", True))
		Else
			'Open new window to view Receiving PO to be printed.
			DisplayJavaScriptMessage("PrintTicket", Utilities.JsWindowOpen("ReceivingTicketPFV.aspx?receiving_ticket_id=" & receivingTicket.Id.ToString)) ', "toolbar=yes,menubar=yes,scrollbars=yes,resizable=yes,width=700,height=500,top=50,left=50", True))
		End If
	End Sub
#End Region

	Private Sub SetTextboxMaxLengths()
		tbxFind.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(_currentTableName, "number"))
	End Sub

	Protected Sub chkShowVoidedTickets_CheckedChanged(sender As Object, e As EventArgs) Handles chkShowVoidedTickets.CheckedChanged
		PopulateReceivingTickets()
	End Sub

	Protected Sub btnVoidTicket_Click(sender As Object, e As EventArgs) Handles btnVoidTicket.Click
		Dim ticketId As Guid = Guid.Parse(ddlReceivingTickets.SelectedValue)
		Dim ticket As New KaReceivingTicket(GetUserConnection(_currentUser.Id), ticketId)
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
		PopulateReceivingTickets()
	End Sub

	Private Sub SetControlUsabilityFromPermissions()
		With _currentUserPermission(_currentTableName)
			Dim shouldEnable = (.Edit AndAlso ddlPastReceivingPurchaseOrders.SelectedIndex > 0)
			pnlEven.Enabled = shouldEnable
			btnVoidTicket.Enabled = ddlReceivingTickets.SelectedIndex <> 0 AndAlso .Edit AndAlso .Delete
		End With
	End Sub

	Protected Sub cbxIncludeArchived_CheckedChanged(sender As Object, e As EventArgs) Handles cbxIncludeArchived.CheckedChanged
		PopulatePastReceivingPurchaseOrders(_currentUser)
	End Sub

	Protected Sub btnArchive_Click(sender As Object, e As EventArgs) Handles btnArchive.Click
		Try
			Dim orderInfo As New KaReceivingPurchaseOrder(GetUserConnection(_currentUser.Id), Guid.Parse(ddlPastReceivingPurchaseOrders.SelectedValue))
			orderInfo.Archived = True
			orderInfo.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)

			Dim getUnarchivedTicketCountRdr As OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), "SELECT count(*) as ticket_count " &
									"FROM receiving_tickets " &
									"WHERE voided = 0 AND receiving_purchase_order_id = " & Q(orderInfo.Id))

			If getUnarchivedTicketCountRdr.Read AndAlso getUnarchivedTicketCountRdr.Item(0) > 0 Then
				'Check if we want to add a Product for this Bulk Product
				Dim javaScript As String =
							  "<script language='JavaScript'>" &
							  "if ( confirm('Do you want to archive the tickets associated with this order?') == true )" &
							  "{" &
							   ClientScript.GetPostBackEventReference(Me, "ArchiveTickets('" & orderInfo.Id.ToString & "')") &
							  "}" &
							  "</script>"
				ClientScript.RegisterStartupScript(Me.GetType(), "ConfirmArchiveScript", javaScript)
			End If
			getUnarchivedTicketCountRdr.Close()
		Catch ex As RecordNotFoundException

		End Try
		PopulatePastReceivingPurchaseOrders(_currentUser)
	End Sub

	Private Sub btnUnarchive_Click(sender As Object, e As System.EventArgs) Handles btnUnarchive.Click
		Try
			Dim orderInfo As New KaReceivingPurchaseOrder(GetUserConnection(_currentUser.Id), Guid.Parse(ddlPastReceivingPurchaseOrders.SelectedValue))
			orderInfo.Archived = False
			orderInfo.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
		Catch ex As RecordNotFoundException

		End Try
		PopulatePastReceivingPurchaseOrders(_currentUser)
	End Sub

	Private Sub ArchiveTickets(ByVal orderId As String)
		Tm2Database.ExecuteNonQuery(GetUserConnection(_currentUser.Id), String.Format("UPDATE receiving_tickets " &
					"SET archived = 1, last_updated_application = {0}, last_updated_user = {1} " &
					"WHERE receiving_purchase_order_id = {2} AND archived = 0", Q(Database.ApplicationIdentifier), Q(_currentUser.Name), Q(orderId)))
	End Sub

	Protected Sub ddlFacilityFilter_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlFacilityFilter.SelectedIndexChanged
		PopulatePastReceivingPurchaseOrders(_currentUser)
		KaSetting.WriteSetting(GetUserConnection(_currentUser.Id), "PurchaseOrdersPage:" & _currentUser.Id.ToString & "/LastFacilityUsed", ddlFacilityFilter.SelectedValue)
	End Sub

	Protected Sub ToolkitScriptManager1_AsyncPostBackError(ByVal sender As Object, ByVal e As System.Web.UI.AsyncPostBackErrorEventArgs)
		ToolkitScriptManager1.AsyncPostBackErrorMessage = e.Exception.Message
	End Sub

	Private Sub DisplayJavaScriptMessage(key As String, script As String)
		ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), key, script, False)
	End Sub
End Class