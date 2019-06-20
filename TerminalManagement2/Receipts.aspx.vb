Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb
Imports System.IO
Imports System.Net
Imports System.Net.Mail
Imports System.Net.Mime

Public Class Receipts : Inherits System.Web.UI.Page
#Region "Events"
	Private _currentUser As KaUser
	Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
	Private _currentTableName As String = "reports"

	Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
		Response.Cache.SetCacheability(HttpCacheability.NoCache)
		_currentUser = Utilities.GetUser(Me)
		_currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Reports")
		If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")

		With _currentUserPermission(_currentTableName)
			btnArchive.Enabled = .Edit
			btnVoid.Enabled = .Edit AndAlso .Delete
		End With
		If Not Page.IsPostBack Then
			Utilities.ConfirmBox(Me.btnArchive, "Are you sure you want to change the archived status of this ticket?")
			Utilities.ConfirmBox(Me.btnVoid, "Are you sure you want to void this ticket?")
			Dim locationId As Guid = Guid.Empty
			If Page.Request("TicketId") Is Nothing AndAlso (Page.Request("LocationId") Is Nothing OrElse Not Guid.TryParse(Page.Request("LocationId"), locationId)) Then
				Guid.TryParse(KaSetting.GetSetting(GetUserConnection(_currentUser.Id), "Receipts:" & _currentUser.Id.ToString & "/LastLocationId", locationId.ToString), locationId)
			End If
			PopulateLocationList(locationId)
			Dim ticketId As Guid = Guid.Empty
			Guid.TryParse(Page.Request("TicketId"), ticketId)
			PopulateTicketList(ticketId)
			ddlTicket_SelectedIndexChanged(ddlTicket, New EventArgs)
			PopulateEmailAddressList()
		End If
		litEmailConfirmation.Text = ""
		pnlTicketReceivingSources.Attributes("display") = "none"
	End Sub

	Protected Sub btnPrinterFriendlyVersion_Click(sender As Object, e As EventArgs) Handles btnPrinterFriendlyVersion.Click
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim webTicketAddress As String = Reports.WebTicketUrlForOwner(connection, GetOwnerIdForTicket(connection, Guid.Parse(ddlTicket.SelectedItem.Value))).ToString.ToLower.Replace("?ticket_id=", "").Replace("&ticket_id=", "").Trim
		If Utilities.ConvertWebPageUrlDomainToRequestedPagesDomain(connection) Then webTicketAddress = Tm2Database.GetUrlInCurrentDomain(Tm2Database.GetCurrentPageUrlWithOriginalPort(Me.Request), webTicketAddress)
		DisplayJavaScriptMessage("ShowTicket", Utilities.JsWindowOpen(webTicketAddress & IIf(webTicketAddress.Contains("?"), "&", "?") & "ticket_id=" & ddlTicket.SelectedItem.Value & "&instanceGuid=" & Guid.NewGuid.ToString))
	End Sub

	Protected Sub ddlLocation_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles ddlLocation.SelectedIndexChanged, ddlSortBy.SelectedIndexChanged
		If ddlLocation.SelectedIndex > 0 Then KaSetting.WriteSetting(GetUserConnection(_currentUser.Id), "Receipts:" & _currentUser.Id.ToString & "/LastLocationId", ddlLocation.SelectedValue)

		Dim ticketId As Guid = Guid.Empty
		Guid.TryParse(ddlTicket.SelectedValue, ticketId)
		PopulateTicketList(ticketId)
	End Sub

	Protected Sub ddlTicket_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles ddlTicket.SelectedIndexChanged
		If ddlTicket.SelectedIndex > 0 Then
			ShowTicket(Guid.Parse(ddlTicket.SelectedItem.Value))
			tblTicketOptions.Visible = True
			frmTicket.Visible = True
		Else
			tblTicketOptions.Visible = False
			pnlLinkedTickets.Visible = False
			frmTicket.Visible = False
		End If
	End Sub

	Private Sub GetTicketSources(ticket As KaTicket)
		Dim receivingTicketUsageList As ArrayList = New ArrayList
		Dim manufacturingTicketUsageList As ArrayList = New ArrayList
		Dim products As Dictionary(Of Guid, KaProduct) = New Dictionary(Of Guid, KaProduct)
		Dim bulkProducts As Dictionary(Of Guid, KaBulkProduct) = New Dictionary(Of Guid, KaBulkProduct)
		GetTicketSourcesTable(ticket, receivingTicketUsageList, manufacturingTicketUsageList, products, bulkProducts)

		litReceivingTicketUsage.Text = ""
		If receivingTicketUsageList.Count > 1 Then
			litReceivingTicketUsage.Text &= "<h2>Receiving sources</h2>" & KaReports.GetTableHtml("", "", receivingTicketUsageList, False, "style=""margin: 0.5em;""", "", New List(Of String), "", New List(Of String))
		End If
		If manufacturingTicketUsageList.Count > 1 Then
			litReceivingTicketUsage.Text &= "<h2>Manufacturing sources</h2>" & KaReports.GetTableHtml("", "", manufacturingTicketUsageList, False, "style=""margin: 0.5em;""", "", New List(Of String), "", New List(Of String))
		End If

		Dim ticketUsagesList As ArrayList = GetTicketUsagesList(ticket)
		If ticketUsagesList.Count > 0 Then
			litReceivingTicketUsage.Text &= "<h2>Usages</h2>" & KaReports.GetTableHtml("", "", ticketUsagesList, False, "style=""margin: 0.5em;""", "", New List(Of String), "", New List(Of String))
		End If

		btnShowTicketSources.Visible = (receivingTicketUsageList.Count + manufacturingTicketUsageList.Count + ticketUsagesList.Count) > 1
		If (receivingTicketUsageList.Count + manufacturingTicketUsageList.Count) > 0 AndAlso ticketUsagesList.Count = 0 Then
			btnShowTicketSources.Text = "Show sources"
		ElseIf (receivingTicketUsageList.Count + manufacturingTicketUsageList.Count) = 0 AndAlso ticketUsagesList.Count > 0 Then
			btnShowTicketSources.Text = "Show usages"
		Else
			btnShowTicketSources.Text = "Show sources"
		End If
	End Sub

	Private Sub GetTicketSourcesTable(ticket As KaTicket, ByRef receivingTicketUsageList As ArrayList, ByRef manufacturingTicketUsageList As ArrayList, ByRef products As Dictionary(Of Guid, KaProduct), ByRef bulkProducts As Dictionary(Of Guid, KaBulkProduct))
		If Tm2Database.SystemItemTraceabilityEnabled Then
			Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
			Dim storageLocationMovements As List(Of KaStorageLocationMovement) = ticket.GetStorageLocationMovements(connection, Nothing)
			Dim ownerReceivingTicketAddresses As Dictionary(Of Guid, String) = New Dictionary(Of Guid, String)
			For Each slm As KaStorageLocationMovement In storageLocationMovements
				Try
					If Not slm.ReceivingTicketId.Equals(Guid.Empty) Then
						Dim receivingTicket As KaReceivingTicket = New KaReceivingTicket(connection, slm.ReceivingTicketId)
						Dim ownerId As Guid = receivingTicket.OwnerId
						Dim webTicketAddress As String
						If Not ownerReceivingTicketAddresses.ContainsKey(ownerId) Then
							webTicketAddress = Reports.ReceivingPoWebTicketUrlForOwner(connection, ownerId).ToString.ToLower.Replace("?ticket_id=", "").Replace("&ticket_id=", "").Replace("?receiving_ticket_id=", "").Replace("&receiving_ticket_id=", "").Trim
							If Utilities.ConvertWebPageUrlDomainToRequestedPagesDomain(connection) Then webTicketAddress = Tm2Database.GetUrlInCurrentDomain(Tm2Database.GetCurrentPageUrlWithOriginalPort(Me.Request), webTicketAddress)
							ownerReceivingTicketAddresses.Add(receivingTicket.OwnerId, webTicketAddress)
						End If
						webTicketAddress = ownerReceivingTicketAddresses(ownerId)
						webTicketAddress &= IIf(webTicketAddress.Contains("?"), "&", "?")
						webTicketAddress &= "ticket_id=" & receivingTicket.Id.ToString()
						webTicketAddress &= "&receiving_ticket_id=" & receivingTicket.Id.ToString()
						webTicketAddress &= "&instanceGuid=" & Guid.NewGuid.ToString()
						webTicketAddress = $"<a href=""{webTicketAddress}"" target=""_blank"">{System.Web.HttpUtility.HtmlEncode(receivingTicket.Number)}{IIf(receivingTicket.Voided, $" (voided)", "")}</a>"
						Dim orderAddress As String
						Try
							If New KaReceivingPurchaseOrder(connection, receivingTicket.ReceivingPurchaseOrderId).Completed Then
								orderAddress = "<a href=""PastReceiving.aspx?ReceivingPurchaseOrderId="
							Else
								orderAddress = "<a href=""Receiving.aspx?ReceivingPurchaseOrderId="
							End If
							orderAddress &= receivingTicket.ReceivingPurchaseOrderId.ToString & """ target=""_blank"">" & System.Web.HttpUtility.HtmlEncode(receivingTicket.PurchaseOrderNumber) & "</a>"
						Catch ex As RecordNotFoundException
							orderAddress = receivingTicket.PurchaseOrderNumber
						End Try
						receivingTicketUsageList.Add(New ArrayList({String.Format(receivingTicket.DateOfDelivery, "G"), webTicketAddress, orderAddress, KaBulkProduct.GetBulkProduct(connection, receivingTicket.BulkProductId, bulkProducts, Nothing).Name, receivingTicket.LotNumber}))
					ElseIf Not slm.TicketId.Equals(Guid.Empty) AndAlso Not slm.LotId.Equals(Guid.Empty) Then
						Dim mfgTicket As KaTicket = New KaTicket(connection, slm.TicketId)
						Dim webTicketAddress As String = $"<a href=""Receipts.aspx?TicketId={mfgTicket.Id.ToString()}"">{System.Web.HttpUtility.HtmlEncode(GetTicketNumber(GetUserConnection(_currentUser.Id), Nothing, mfgTicket))}</a>"
						Dim orderAddress As String
						Try
							If New KaOrder(connection, mfgTicket.OrderId).Completed Then
								orderAddress = "<a href=""PastOrders.aspx?OrderId="
							Else
								orderAddress = "<a href=""Orders.aspx?OrderId="
							End If
							orderAddress &= mfgTicket.OrderId.ToString & """ target=""_blank"">" & System.Web.HttpUtility.HtmlEncode(mfgTicket.OrderNumber) & "</a>"
						Catch ex As RecordNotFoundException
							orderAddress = mfgTicket.OrderNumber
						End Try

						manufacturingTicketUsageList.Add(New ArrayList({String.Format(mfgTicket.LoadedAt, "G"), webTicketAddress, orderAddress, KaProduct.GetProduct(connection, mfgTicket.TicketItems(0).ProductId, products, Nothing).Name, mfgTicket.LotNumber}))
					End If
				Catch ex As RecordNotFoundException
				End Try
			Next
			If receivingTicketUsageList.Count > 0 Then receivingTicketUsageList.Insert(0, New ArrayList({"Received at", "Ticket", "Purchase order number", "Bulk product", "Lot number"}))
			If manufacturingTicketUsageList.Count > 0 Then manufacturingTicketUsageList.Insert(0, New ArrayList({"Manufactured at", "Ticket", "Order number", "Product", "Lot number"}))
		End If
	End Sub

	Private Function GetTicketUsagesList(sourceTicket As KaTicket) As ArrayList
		Dim ticketUsageList As ArrayList = New ArrayList
		If Tm2Database.SystemItemTraceabilityEnabled AndAlso sourceTicket IsNot Nothing Then
			Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
			Dim tickets As Dictionary(Of Guid, KaTicket) = sourceTicket.GetTicketUsagesForMovements(connection, Nothing, cbxShowVoided.Checked)
			For Each ticketId As Guid In tickets.Keys
				Dim ticket As KaTicket = tickets(ticketId)
				Dim webTicketAddress As String = $"<a href=""Receipts.aspx?TicketId={ticket.Id.ToString()}"">{System.Web.HttpUtility.HtmlEncode(GetTicketNumber(GetUserConnection(_currentUser.Id), Nothing, ticket))}</a>"
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
				ticketUsageList.Add(New ArrayList({String.Format(ticket.LoadedAt, "G"), webTicketAddress, orderAddress}))
			Next
			If ticketUsageList.Count > 0 Then ticketUsageList.Insert(0, New ArrayList({"Loaded at", "Ticket", "Order number"}))
		End If

		Return ticketUsageList
	End Function

	Public Shared Function GetTicketNumber(connection As OleDbConnection, transaction As OleDbTransaction, ticket As KaTicket)
		Dim ticketNumber As String = ticket.Number.Trim
		If ticket.InternalTransfer Then
			If ticket.OrderNumber.Trim.Length = 0 Then
				' Get the first Item
				Dim ticketProdRdr As OleDbDataReader = Tm2Database.ExecuteReader(connection, transaction, "SELECT TOP 1 name FROM ticket_items WHERE ticket_id = " & Q(ticket.Id))
				If ticketProdRdr.Read Then
					ticketNumber &= " - Product: " & ticketProdRdr("name").Trim
				End If
				ticketProdRdr.Close()
			Else
				ticketNumber &= " - " & ticket.OrderNumber.Trim
			End If
		End If
		If ticket.Voided Then
			ticketNumber &= " (voided)"
		ElseIf ticket.Archived Then
			ticketNumber &= " (archived)"
		End If
		Return ticketNumber
	End Function


	''' <summary>
	''' find tickets where ticket matches keyword
	''' </summary>
	''' <param name="sender"></param>
	''' <param name="e"></param>
	''' <remarks></remarks>
	Protected Sub btnFind_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnFind.Click
		Dim tickets As New List(Of Guid) ' get a list of the ticket IDs
		If tbxFind.Text.Trim.Length > 0 Then tickets = KaTicket.GetTicketIdsWithKeyword(GetUserConnection(_currentUser.Id), Nothing, _currentUser.OwnerId, tbxFind.Text.Trim)

		Dim found As Boolean = False
		Dim i As Integer = ddlTicket.SelectedIndex ' begin with the next ticket in the drop-down list
		Do
			If i + 1 = ddlTicket.Items.Count Then i = 0 Else i += 1 ' wrap around to the beginning of the drop-down list
			If tickets.IndexOf(Guid.Parse(ddlTicket.Items(i).Value)) <> -1 Then ' this is one of the tickets that was found, select it
				ddlTicket.SelectedIndex = i
				found = True
				Exit Do ' no need to look any further
			End If
		Loop While i <> ddlTicket.SelectedIndex ' continue until we've come back to where we started
		If found Then
			ddlTicket_SelectedIndexChanged(sender, e)
		Else
			DisplayJavaScriptMessage("InvalidKeyword", Utilities.JsAlert("Ticket Not found containing keywords " & tbxFind.Text))
		End If
	End Sub

	Protected Sub btnSendEmail_Click(sender As Object, e As EventArgs) Handles btnSendEmail.Click
		Dim message As String = ""
		If Not Utilities.IsEmailFieldValid(tbxEmailTo.Text, message) Then
			DisplayJavaScriptMessage("InvalidEmail", Utilities.JsAlert(message))
			Exit Sub
		End If
		Dim ticketId As Guid = Guid.Empty
		Guid.TryParse(ddlTicket.SelectedValue, ticketId)
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim webTicketAddress As String = WebTicketUrlForOwner(connection, GetOwnerIdForTicket(connection, ticketId))
		If webTicketAddress.ToLower.IndexOf("ticket_id=") < 0 Then
			If webTicketAddress.IndexOf("?") >= 0 Then
				webTicketAddress &= "&"
			Else
				webTicketAddress &= "?"
			End If
			webTicketAddress &= "ticket_id="
		End If
		webTicketAddress &= ticketId.ToString()
		If Utilities.ConvertWebPageUrlDomainToRequestedPagesDomain(connection) Then webTicketAddress = Tm2Database.GetUrlInCurrentDomain(Tm2Database.GetCurrentPageUrlWithOriginalPort(Me.Request), webTicketAddress)
		Dim emailBody As String = GetTicketHtml(webTicketAddress)
		Dim emailAddresses As String = ""
		Dim emailTo() As String = tbxEmailTo.Text.Split(New Char() {";", ","})
		For Each emailRecipient As String In emailTo
			If emailRecipient.Trim.Length > 0 Then
				Dim newEmail As New KaEmail()
				newEmail.ApplicationId = APPLICATION_ID
				newEmail.Body = emailBody
				newEmail.BodyIsHtml = True
				newEmail.OwnerID = _currentUser.OwnerId
				newEmail.Recipients = emailRecipient.Trim
				newEmail.ReportType = KaEmailReport.ReportTypes.Ticket
				newEmail.Subject = "Ticket " & ddlTicket.SelectedItem.Text
				Dim attachments As New List(Of Attachment)
				attachments.Add(New Attachment(New MemoryStream(Encoding.UTF8.GetBytes(newEmail.Body)), "ticket.html", MediaTypeNames.Text.Html))
				newEmail.SerializeAttachments(attachments)
				KaEmail.CreateEmail(GetUserConnection(_currentUser.Id), newEmail, -1, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
				If emailAddresses.Length > 0 Then emailAddresses &= ", "
				emailAddresses &= newEmail.Recipients
			End If
		Next

		If emailAddresses.Length > 0 Then
			litEmailConfirmation.Text = "Ticket " & ddlTicket.SelectedItem.Text & " sent to " & emailAddresses
		Else
			litEmailConfirmation.Text = "Ticket Not sent.  No e-mail address."
		End If
	End Sub
#End Region

	Private Function GetTicketHtml(url As String) As String
		Dim html As String = New UTF8Encoding().GetString(New WebClient().DownloadData(url))
		Dim i As Integer = 0
		Do ' remove any ASP.NET state variables
			i = html.IndexOf("<input type=""hidden""")
			If i > -1 Then
				Dim j As Integer = html.IndexOf(">", i)
				html = html.Substring(0, i) & html.Substring(j + 1, html.Length - j - 1)
			End If
		Loop While i <> -1
		Return html
	End Function

	Private Sub PopulateLocationList(ByVal locationId As Guid)
		ddlLocation.Items.Clear()
		ddlLocation.Items.Add(New ListItem("All facilities", Guid.Empty.ToString()))
		If locationId.Equals(Guid.Empty) Then
			ddlLocation.SelectedIndex = ddlLocation.Items.Count - 1
			ddlLocation.SelectedValue = Guid.Empty.ToString()
		End If
		For Each l As KaLocation In KaLocation.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
			ddlLocation.Items.Add(New ListItem(l.Name, l.Id.ToString()))
			If l.Id.Equals(locationId) Then
				ddlLocation.SelectedIndex = ddlLocation.Items.Count - 1
				ddlLocation.SelectedValue = l.Id.ToString()
			End If
		Next
	End Sub

	Private Sub PopulateTicketList(ByVal ticketId As Guid)
		Const FN_ID As String = "id"
		Const FN_NUMBER As String = "number"
		Const FN_ARCHIVED As String = "archived"
		ddlTicket.Items.Clear()
		ddlTicket.Items.Add(New ListItem("", Guid.Empty.ToString()))
		Dim locationId As Guid = Guid.Empty
		Guid.TryParse(ddlLocation.SelectedValue, locationId)

		Dim user As KaUser = Utilities.GetUser(Me)
		Dim conditions As String = ""
		If ddlLocation.SelectedValue <> Guid.Empty.ToString Then conditions &= IIf(conditions.Length > 0, " AND ", "") & "tickets.location_id = " & Q(locationId)
		If Not cbxShowArchived.Checked Then conditions &= IIf(conditions.Length > 0, " AND ", "") & "tickets.archived=0"
		If Not cbxShowVoided.Checked Then conditions &= IIf(conditions.Length > 0, " AND ", "") & "tickets.voided=0"
		If Not cbxShowInternalTransfer.Checked Then conditions &= IIf(conditions.Length > 0, " AND ", "") & "tickets.internal_transfer=0"
		If Not user.OwnerId.Equals(Guid.Empty) Then conditions &= IIf(conditions.Length > 0, " AND ", "") & String.Format("(tickets.owner_id={0} OR (NOT orders.owner_id IS NULL AND orders.owner_id={0}))", Q(user.OwnerId))

		Dim commandText As String = $"SELECT DISTINCT {KaTicket.TABLE_NAME}.id, REPLICATE('0', 500 - LEN({KaTicket.TABLE_NAME}.{FN_NUMBER})) + {KaTicket.TABLE_NAME}.{FN_NUMBER}, {KaTicket.TABLE_NAME}.loaded_at, {KaTicket.TABLE_NAME}.{FN_NUMBER}, {KaTicket.TABLE_NAME}.archived, {KaTicket.TABLE_NAME}.{KaTicket.FN_INTERNAL_TRANSFER}, order_number " &
			$"FROM {KaTicket.TABLE_NAME} " &
			$"LEFT OUTER JOIN orders ON {KaTicket.TABLE_NAME}.order_id=orders.id " &
			IIf(conditions.Length > 0, "WHERE " & conditions & " ", "")
		If Not ticketId.Equals(Guid.Empty) Then
			commandText &= " UNION " &
			$"SELECT DISTINCT {KaTicket.TABLE_NAME}.id, REPLICATE('0', 500 - LEN({KaTicket.TABLE_NAME}.{FN_NUMBER})) + {KaTicket.TABLE_NAME}.{FN_NUMBER}, {KaTicket.TABLE_NAME}.loaded_at, {KaTicket.TABLE_NAME}.{FN_NUMBER}, {KaTicket.TABLE_NAME}.archived, {KaTicket.TABLE_NAME}.{KaTicket.FN_INTERNAL_TRANSFER}, order_number " &
			$"FROM {KaTicket.TABLE_NAME} " &
			$"WHERE {KaTicket.TABLE_NAME}.id = {Q(ticketId)} "
		End If
		commandText &= "ORDER BY "
		Select Case ddlSortBy.SelectedValue
			Case "DateAsc" : commandText &= "3 ASC, 2 ASC"
			Case "DateDesc" : commandText &= "3 DESC, 2 DESC"
			Case "TicketAsc" : commandText &= "2 ASC, 3 ASC"
			Case "TicketDesc" : commandText &= "2 DESC, 3 DESC"
			Case Else : commandText &= "2 ASC, 3 ASC"
		End Select
		Dim getTicketsDA As New OleDbDataAdapter(commandText, GetUserConnection(_currentUser.Id))
		Dim dataTable As New DataTable()
		If Tm2Database.CommandTimeout > 0 Then getTicketsDA.SelectCommand.CommandTimeout = Tm2Database.CommandTimeout
		getTicketsDA.Fill(dataTable)
		For Each row As DataRow In dataTable.Rows
			Dim id As Guid = row(FN_ID)
			Dim ticketNumber As String = row(FN_NUMBER).Trim
			If row(KaTicket.FN_INTERNAL_TRANSFER) Then
				If row("order_number").Trim.Length = 0 Then
					' Get the first Item
					Dim ticketProdRdr As OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), "SELECT TOP 1 name FROM ticket_items WHERE ticket_id = " & Q(row(FN_ID)))
					If ticketProdRdr.Read Then
						ticketNumber &= " - Product: " & ticketProdRdr("name").Trim & " " & row("loaded_at").ToString()
					End If
					ticketProdRdr.Close()
				Else
					ticketNumber &= " - " & row("order_number").Trim
				End If
			End If
			If row(FN_ARCHIVED) Then
				ticketNumber &= " (archived)"
			End If
			ddlTicket.Items.Add(New ListItem(ticketNumber, id.ToString()))

			If id = ticketId Then ddlTicket.SelectedIndex = ddlTicket.Items.Count - 1
		Next
	End Sub

	Private Sub ShowTicket(ByVal ticketId As Guid)
		If ticketId = Guid.Empty Then
			btnPrinterFriendlyVersion.Visible = False
		Else
			btnPrinterFriendlyVersion.Visible = True
			Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
			Dim webTicketAddress As String = WebTicketUrlForOwner(connection, GetOwnerIdForTicket(connection, ticketId))
			If webTicketAddress.ToLower.IndexOf("ticket_id=") < 0 Then
				If webTicketAddress.IndexOf("?") >= 0 Then
					webTicketAddress &= "&"
				Else
					webTicketAddress &= "?"
				End If
				webTicketAddress &= "ticket_id="
			End If
			webTicketAddress &= ticketId.ToString()
			If Utilities.ConvertWebPageUrlDomainToRequestedPagesDomain(connection) Then webTicketAddress = Tm2Database.GetUrlInCurrentDomain(Tm2Database.GetCurrentPageUrlWithOriginalPort(Me.Request), webTicketAddress)
			frmTicket.Attributes("src") = webTicketAddress
			Dim ticket As New KaTicket(connection, ticketId)
			If ticket.Voided Then
				btnVoid.Visible = False
				btnArchive.Visible = False
			ElseIf ticket.Archived Then
				btnArchive.Text = "Un-archive"
				btnVoid.Visible = _currentUserPermission(_currentTableName).Delete
				btnArchive.Visible = True
			Else
				btnArchive.Text = "Archive"
				btnVoid.Visible = _currentUserPermission(_currentTableName).Delete
				btnArchive.Visible = True
			End If
			GetTicketSources(ticket)
		End If

		Dim linkedTickets As String = ""
		Dim otherTicketsTable As New DataTable
		Dim otherTicketsDA As New OleDbDataAdapter("SELECT id, number, order_number " &
												"FROM tickets " &
												"WHERE (tickets.voided=0) " &
													"AND (linked_tickets_id IN (SELECT linked_tickets_id FROM tickets AS tickets_orig " &
														"WHERE (id = " & Q(ticketId) & "))) " &
													"AND (id <> " & Q(ticketId) & ")" &
													"AND (linked_tickets_id <> " & Q(Guid.Empty) & ")", GetUserConnection(_currentUser.Id))
		If Tm2Database.CommandTimeout > 0 Then otherTicketsDA.SelectCommand.CommandTimeout = Tm2Database.CommandTimeout
		otherTicketsDA.Fill(otherTicketsTable)
		If otherTicketsTable.Rows.Count > 0 Then
			For Each otherTicket As DataRow In otherTicketsTable.Rows
				If linkedTickets.Length > 0 Then linkedTickets &= "<br />"
				linkedTickets &= "<a href=""Receipts.aspx?LocationId=" & ddlLocation.SelectedValue.ToString & "&TicketId=" & otherTicket.Item("id").ToString & """ >Ticket Number " & otherTicket.Item("number") & " (Order Number " & otherTicket.Item("order_number") & ")</a>"
			Next
		End If
		litLinkedTickets.Text = linkedTickets
		pnlLinkedTickets.Visible = (linkedTickets.Length > 0)
	End Sub

	Private Sub SetTextboxMaxLengths()
		tbxEmailTo.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaEmail.TABLE_NAME, KaEmail.FN_RECIPIENTS))
		tbxFind.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaTicket.TABLE_NAME, "number"))
	End Sub

	Protected Sub btnArchive_Click(sender As Object, e As EventArgs) Handles btnArchive.Click
		Dim ticketId As Guid = Guid.Parse(ddlTicket.SelectedValue)
		Dim ticketInfo As New KaTicket(GetUserConnection(_currentUser.Id), ticketId)
		ticketInfo.Archived = Not ticketInfo.Archived
		ticketInfo.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
		PopulateTicketList(ticketId)
		' ddlTicket.SelectedIndex = 0
		ddlTicket_SelectedIndexChanged(ddlTicket, New EventArgs)
	End Sub

	Protected Sub cbxShowArchived_CheckedChanged(sender As Object, e As EventArgs) Handles cbxShowArchived.CheckedChanged
		Dim ticketId As Guid
		Try ' to parse out ticket Id from ticket drop down list 
			ticketId = Guid.Parse(ddlTicket.SelectedValue)
		Catch ex As FormatException ' ticket Id wasn't available 
			ticketId = Guid.Empty ' no ticket selected 
		End Try
		PopulateTicketList(ticketId)
		ddlTicket_SelectedIndexChanged(Nothing, Nothing)
	End Sub

	Protected Sub btnVoid_Click(sender As Object, e As EventArgs) Handles btnVoid.Click
		Dim ticketId As Guid = Guid.Parse(ddlTicket.SelectedValue)
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

		PopulateTicketList(IIf(cbxShowVoided.Checked, ticketId, Guid.Empty))
		ddlTicket.SelectedIndex = 0
		ddlTicket_SelectedIndexChanged(ddlTicket, New EventArgs)
	End Sub

	Protected Sub cbxShowVoided_CheckedChanged(sender As Object, e As EventArgs) Handles cbxShowVoided.CheckedChanged
		Dim ticketId As Guid
		Try ' to parse out ticket Id from ticket drop down list 
			ticketId = Guid.Parse(ddlTicket.SelectedValue)
		Catch ex As FormatException ' ticket Id wasn't available 
			ticketId = Guid.Empty ' no ticket selected 
		End Try
		PopulateTicketList(ticketId)
		ddlTicket_SelectedIndexChanged(Nothing, Nothing)
	End Sub

	Private Sub cbxShowInternalTransfer_CheckedChanged(sender As Object, e As System.EventArgs) Handles cbxShowInternalTransfer.CheckedChanged
		Dim ticketId As Guid
		Try ' to parse out ticket Id from ticket drop down list 
			ticketId = Guid.Parse(ddlTicket.SelectedValue)
		Catch ex As FormatException ' ticket Id wasn't available 
			ticketId = Guid.Empty ' no ticket selected 
		End Try
		PopulateTicketList(ticketId)
		ddlTicket_SelectedIndexChanged(Nothing, Nothing)
	End Sub

	Private Sub PopulateEmailAddressList()
		Utilities.PopulateEmailAddressList(tbxEmailTo, ddlAddEmailAddress, btnAddEmailAddress)
		rowAddAddress.Visible = ddlAddEmailAddress.Items.Count > 1
	End Sub

	Protected Sub btnAddEmailAddress_Click(sender As Object, e As EventArgs) Handles btnAddEmailAddress.Click
		If ddlAddEmailAddress.SelectedIndex > 0 Then
			If tbxEmailTo.Text.Trim.Length > 0 Then tbxEmailTo.Text &= ", "
			tbxEmailTo.Text &= ddlAddEmailAddress.SelectedValue
			PopulateEmailAddressList()
		End If
	End Sub

	Private Sub tbxEmailTo_TextChanged(sender As Object, e As System.EventArgs) Handles tbxEmailTo.TextChanged
		PopulateEmailAddressList()
	End Sub

	Protected Sub ToolkitScriptManager1_AsyncPostBackError(ByVal sender As Object, ByVal e As System.Web.UI.AsyncPostBackErrorEventArgs)
		ToolkitScriptManager1.AsyncPostBackErrorMessage = e.Exception.Message
	End Sub

	Private Sub DisplayJavaScriptMessage(key As String, script As String)
		ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), key, script, False)
	End Sub
End Class