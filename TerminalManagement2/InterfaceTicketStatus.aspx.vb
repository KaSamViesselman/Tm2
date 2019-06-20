Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class InterfaceTicketStatus
    Inherits System.Web.UI.Page

    Private _interfaceId As Guid = Guid.Empty
    Private _currentUser As KaUser
    Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
    Private _currentTableName As String = KaInterface.TABLE_NAME

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Response.Cache.SetCacheability(HttpCacheability.NoCache)
        _currentUser = Utilities.GetUser(Me)
        _currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Interfaces")
        If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")

        If Not Page.IsPostBack Then
            PopulateInterfacesList()
            ddlInterfaces_SelectedIndexChanged(ddlInterfaces, New EventArgs())
            rbShowTicketsExportedCheckedChanged(rbShowTicketsExported, New EventArgs)
            'ToolkitScriptManager1.AsyncPostBackTimeout = Math.Max(90, Tm2Database.CommandTimeout)
        Else
            Guid.TryParse(ddlInterfaces.SelectedValue, _interfaceId)
            If Not _interfaceId.Equals(Guid.Empty) Then
                ticketDisplayOptions.Visible = True
                ticketSortOptions.Visible = True
                divTickets.Visible = True
                pnlSendEmail.Visible = True
                pnlRecordControl.Visible = True
            Else
                ticketDisplayOptions.Visible = False
                ticketSortOptions.Visible = False
                pnlPageNumbers.Visible = False
                divTickets.Visible = False
                pnlSendEmail.Visible = False
                pnlRecordControl.Visible = False
            End If
        End If
        GetTicketInfo() ' This is needed to recreate the table for click events
    End Sub

    Private Sub PopulateInterfacesList()
        ddlInterfaces.Items.Clear()
        ddlInterfaces.Items.Add(New ListItem("Select an interface", Guid.Empty.ToString()))
        For Each r As KaInterface In KaInterface.GetAll(GetUserConnection(_currentUser.Id), "deleted = " & Q(False), "name ASC")
            ddlInterfaces.Items.Add(New ListItem(r.Name, r.Id.ToString()))
        Next
    End Sub

    Private Sub ddlInterfaces_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles ddlInterfaces.SelectedIndexChanged
        If ddlPageNumber.Items.Count > 0 Then ddlPageNumber.SelectedIndex = 0
        Guid.TryParse(ddlInterfaces.SelectedValue, _interfaceId)
		If Not _interfaceId.Equals(Guid.Empty) Then
			ticketDisplayOptions.Visible = True
			ticketSortOptions.Visible = True
			divTickets.Visible = True
			pnlSendEmail.Visible = True
			pnlRecordControl.Visible = True
		Else
			ticketDisplayOptions.Visible = False
			ticketSortOptions.Visible = False
			pnlPageNumbers.Visible = False
			divTickets.Visible = False
			pnlSendEmail.Visible = False
			pnlRecordControl.Visible = False
		End If
	End Sub

    Private Sub GetTicketInfo()
        tblTickets.Rows.Clear()
        Dim connection As OleDbConnection = Tm2Database.Connection

        SetTableHeader()
        Dim tempInterface As KaInterface
        Try
            tempInterface = New KaInterface(connection, _interfaceId)
        Catch ex As RecordNotFoundException
            Exit Sub
        End Try

        Dim exportedTicketsTable = tempInterface.GetInterfaceTicketExportStatusTable(connection, rbShowTicketsExported.Checked, chkIncludeTicketsMarkedManually.Checked, rbIncludeTicketsWithError.Checked, rbIncludeTicketsWithIgnoredError.Checked, chkOnlyIncludeOrdersForThisInterface.Checked)

        Dim sortCriteria As String = "exported_at ASC, number ASC, loaded_at ASC"
        Select Case ddlSortBy.SelectedValue
            Case "ExportedAtDateAsc"
                sortCriteria = "exported_at ASC"
            Case "ExportedAtDateDesc"
                sortCriteria = "exported_at DESC"
            Case "LoadedAtDateAsc"
                sortCriteria = "loaded_at ASC"
            Case "LoadedAtDateDesc"
                sortCriteria = "loaded_at DESC"
            Case "TicketAsc"
                sortCriteria = "number ASC"
            Case "TicketDesc"
                sortCriteria = "number DESC"
        End Select
        Dim exported As DataRow() = exportedTicketsTable.Select("", sortCriteria)

        Dim numberOfTicketsPerPage As Integer = Integer.Parse(ddlTicketsPerPage.SelectedValue)
        If numberOfTicketsPerPage < 0 Then numberOfTicketsPerPage = exported.Length
        Dim maxNumberOfPages As Integer = Math.Ceiling(exported.Length / numberOfTicketsPerPage)
        Dim visiblePageNumber As Integer = Math.Max(1, Math.Min(ddlPageNumber.SelectedIndex + 1, maxNumberOfPages))

        ddlPageNumber.Items.Clear()
        Dim ownerTicketAddresses As New Dictionary(Of Guid, String)
        For currentPageNumber As Integer = 1 To maxNumberOfPages
            Dim startingExportIndex As Integer = Math.Max(0, (currentPageNumber * numberOfTicketsPerPage) - numberOfTicketsPerPage)
            Dim endingExportIndex As Integer = Math.Min(exported.Length, startingExportIndex + numberOfTicketsPerPage) - 1
            ddlPageNumber.Items.Add(exported(startingExportIndex).Item("number") & " - " & exported(endingExportIndex).Item("number"))
            If currentPageNumber = visiblePageNumber Then
                For rowNumber As Integer = startingExportIndex To endingExportIndex
                    Dim exportedRow As DataRow = exported(rowNumber)
                    AddTicketToTable(connection, exportedRow.Item("id"), exportedRow.Item("ticket_interface_export_id"), exportedRow.Item("number"), exportedRow.Item("loaded_at"), exportedRow.Item("exported_at"), exportedRow.Item("order_number"), exportedRow.Item("interface_id"), exportedRow.Item("exported"), exportedRow.Item("exported_data"), exportedRow.Item("order_id"), exportedRow.Item("orders_completed"), ownerTicketAddresses)
                Next
            End If
        Next

        If ddlPageNumber.Items.Count >= visiblePageNumber Then
            ddlPageNumber.SelectedIndex = visiblePageNumber - 1
        ElseIf ddlPageNumber.Items.Count > 0 Then
            ddlPageNumber.SelectedIndex = 0
        End If
        btnPreviousPage.Enabled = (ddlPageNumber.Items.Count > 1) AndAlso (ddlPageNumber.SelectedIndex > 0)
        btnNextPage.Enabled = (ddlPageNumber.Items.Count > 1) AndAlso (ddlPageNumber.SelectedIndex < ddlPageNumber.Items.Count - 1)
        pnlPageNumbers.Visible = (ddlInterfaces.SelectedIndex > 0) AndAlso ((ddlPageNumber.Items.Count > 1) OrElse (ddlTicketsPerPage.SelectedValue < 0))
    End Sub

    Private Sub SetTableHeader()
        Dim ticketHeaderRow As New HtmlTableRow()
        With ticketHeaderRow
            .ID = "TicketExportHeader"
            .VAlign = "top"
        End With
        tblTickets.Rows.Add(ticketHeaderRow)

        Dim ticketNumberCell As New HtmlTableCell("th")
        Dim ticketNumber As New Label
        With ticketNumber
            .ID = ticketHeaderRow.ID & "_Number"
            .Text = "Number"
        End With
        ticketNumberCell.Controls.Add(ticketNumber)
        ticketHeaderRow.Controls.Add(ticketNumberCell)

        Dim ticketOrderCell As New HtmlTableCell("th")
        Dim ticketOrder As New Label
        With ticketOrder
            .ID = ticketHeaderRow.ID & "_Order"
            .Text = "Order"
        End With
        ticketOrderCell.Controls.Add(ticketOrder)
        ticketHeaderRow.Controls.Add(ticketOrderCell)

        Dim ticketCustomerCell As New HtmlTableCell("th")
        Dim ticketCustomer As New Label
        With ticketCustomer
            .ID = ticketHeaderRow.ID & "_Customer"
            .Text = "Customer"
        End With
        ticketCustomerCell.Controls.Add(ticketCustomer)
        ticketHeaderRow.Controls.Add(ticketCustomerCell)

        Dim ticketLoadedDateCell As New HtmlTableCell("th")
        Dim ticketLoadedDate As New Label
        With ticketLoadedDate
            .ID = ticketHeaderRow.ID & "_LoadedDate"
            .Text = "Loaded Date"
        End With
        ticketLoadedDateCell.Controls.Add(ticketLoadedDate)
        ticketHeaderRow.Controls.Add(ticketLoadedDateCell)

        If rbShowTicketsExported.Checked OrElse (rbShowTicketsNotExported.Checked AndAlso (rbIncludeTicketsWithError.Checked OrElse rbIncludeTicketsWithIgnoredError.Checked)) Then
            Dim ticketExportedDateCell As New HtmlTableCell("th")
            Dim ticketExportedDate As New Label
            With ticketExportedDate
                .ID = ticketHeaderRow.ID & "_ExportedDate"
                .Text = "Exported Date"
            End With
            ticketExportedDateCell.Controls.Add(ticketExportedDate)
            ticketHeaderRow.Controls.Add(ticketExportedDateCell)
        End If

        Dim ticketOptionCell As New HtmlTableCell("th")
        With ticketOptionCell
            .InnerHtml = "&nbsp;"
        End With
        ticketHeaderRow.Controls.Add(ticketOptionCell)

        Dim ticketReasonCell As New HtmlTableCell("th")
        With ticketReasonCell
            .InnerHtml = "&nbsp;"
        End With
        ticketHeaderRow.Controls.Add(ticketReasonCell)
    End Sub

    Private Sub AddTicketToTable(ByVal connection As OleDbConnection, ByVal ticketId As Guid, ByVal ticketExportId As Guid, ticketNumber As String, ticketLoadedAt As DateTime, ticketExportedAt As DateTime, ticketOrderNumber As String, ticketInterfaceId As Guid, ticketExported As Boolean, exportedData As String, ByVal ticketOrderId As Guid, ByVal orderCompleted As Boolean, ByRef ownerTicketAddresses As Dictionary(Of Guid, String))
        Try
            Dim bgColor As String = ""
            Dim ticketRow As New HtmlTableRow
            With ticketRow
                If ticketExportId.Equals(Guid.Empty) Then
                    .ID = "TicketExport" & ticketId.ToString
                Else
                    .ID = "TicketExport" & ticketExportId.ToString
                End If
                If exportedData.ToLower.StartsWith(KaTicketInterfaceExport.EXPORT_IGNORED_STATUS_STRING.ToLower) Then
                    bgColor = "Yellow"
                ElseIf exportedData.ToLower.StartsWith(KaTicketInterfaceExport.EXPORT_MANUAL_STATUS_STRING.ToLower) Then
                    bgColor = "Yellow"
                ElseIf exportedData.ToLower.StartsWith(KaTicketInterfaceExport.EXPORT_FAILED_STATUS_STRING.ToLower) Then
                    bgColor = "Red"
                End If

                If bgColor <> "" Then .BgColor = bgColor
                .EnableViewState = True
            End With
            tblTickets.Rows.Add(ticketRow)

            Dim ticketNumberCell As New HtmlTableCell()
            With ticketNumberCell
                .Style.Item("vertical-align") = "top"
                If bgColor <> "" Then .Style.Item("background-color") = ticketRow.BgColor
            End With
            Dim ticketIdField As New HtmlInputText
            With ticketIdField
                .Attributes("type") = "hidden"
                .ID = ticketRow.ID & "_Id"
                .Value = ticketId.ToString
                .EnableViewState = True
            End With
            ticketNumberCell.Controls.Add(ticketIdField)

            Dim ticketExportIdField As New HtmlInputText
            With ticketExportIdField
                .Attributes("type") = "hidden"
                .ID = ticketRow.ID & "_ExportId"
                .Value = ticketExportId.ToString
                .EnableViewState = True
            End With
            ticketNumberCell.Controls.Add(ticketExportIdField)

            Dim ticketNumberLabel As New Label
            With ticketNumberLabel
                .ID = ticketRow.ID & "_Number"
                Dim ticketOwnerId As Guid = GetOwnerIdForTicket(connection, ticketId)
                Dim webticket As String
                If ownerTicketAddresses.ContainsKey(ticketOwnerId) Then
                    webticket = ownerTicketAddresses(ticketOwnerId)
                Else
                    webticket = Reports.WebTicketUrlForOwner(connection, ticketOwnerId).Replace("?ticket_id=", "").Replace("&ticket_id=", "").Trim
                    If Utilities.ConvertWebPageUrlDomainToRequestedPagesDomain(connection) Then webticket = Tm2Database.GetUrlInCurrentDomain(Tm2Database.GetCurrentPageUrlWithOriginalPort(Me.Request), webticket)
                    ownerTicketAddresses.Add(ticketOwnerId, webticket)
                End If

                .Text = "<a href=""" & webticket & IIf(webticket.Contains("?"), "&", "?") & "ticket_id=" & ticketId.ToString & "&instanceGuid=" & Guid.NewGuid.ToString & """ target=""_blank"">" & EncodeAsHtml(ticketNumber.ToString) & "</a>"
            End With
            ticketNumberCell.Controls.Add(ticketNumberLabel)
            ticketRow.Controls.Add(ticketNumberCell)

            Dim ticketOrderCell As New HtmlTableCell()
            With ticketOrderCell
                .Style.Item("vertical-align") = "top"
                If bgColor <> "" Then .Style.Item("background-color") = ticketRow.BgColor
            End With
            Dim ticketOrder As New Label
            With ticketOrder
                .ID = ticketRow.ID & "_Order"
                If orderCompleted.Equals(DBNull.Value) Then
                    .Text = EncodeAsHtml(ticketOrderNumber.ToString)
                ElseIf orderCompleted Then
                    .Text = "<a href=""PastOrders.aspx?OrderId=" & ticketOrderId.ToString & """ target=""_blank"">" & EncodeAsHtml(ticketOrderNumber.ToString) & "</a>"
                Else
                    .Text = "<a href=""Orders.aspx?OrderId=" & ticketOrderId.ToString & """ target=""_blank"">" & EncodeAsHtml(ticketOrderNumber.ToString) & "</a>"
                End If
            End With
            ticketOrderCell.Controls.Add(ticketOrder)
            ticketRow.Controls.Add(ticketOrderCell)

            Dim ticketCustomerCell As New HtmlTableCell()
            With ticketCustomerCell
                .Style.Item("vertical-align") = "top"
                If bgColor <> "" Then .Style.Item("background-color") = ticketRow.BgColor
            End With
            Dim ticketCustomer As New Label
            With ticketCustomer
                .ID = ticketRow.ID & "_Customer"
                Dim customerName As String = ""
                Dim ticketCustomerRdr As OleDbDataReader = Tm2Database.ExecuteReader(connection, "SELECT name " &
                                                          "FROM ticket_customer_accounts " &
                                                          "WHERE (deleted = 0) AND ticket_id=" & Q(ticketId) & " " &
                                                          "ORDER BY name")
                Do While ticketCustomerRdr.Read
                    If ticketCustomerRdr("name").Trim.Length = 0 Then Continue Do
                    If customerName.Length > 0 Then customerName &= ", "
                    customerName &= ticketCustomerRdr("name").Trim
                Loop
                ticketCustomerRdr.Close()
                .Text = EncodeAsHtml(customerName)
            End With
            ticketCustomerCell.Controls.Add(ticketCustomer)
            ticketRow.Controls.Add(ticketCustomerCell)

            Dim ticketLoadedDateCell As New HtmlTableCell()
            With ticketLoadedDateCell
                .Style.Item("vertical-align") = "top"
                If bgColor <> "" Then .Style.Item("background-color") = ticketRow.BgColor
            End With
            Dim ticketLoadedDate As New Label
            With ticketLoadedDate
                .ID = ticketRow.ID & "_LoadedDate"
                .Text = EncodeAsHtml(String.Format("{0:g}", ticketLoadedAt))
                .Style.Item("vertical-align") = "top"
            End With
            ticketLoadedDateCell.Controls.Add(ticketLoadedDate)
            ticketRow.Controls.Add(ticketLoadedDateCell)

            If rbShowTicketsExported.Checked OrElse (rbShowTicketsNotExported.Checked AndAlso (rbIncludeTicketsWithError.Checked OrElse rbIncludeTicketsWithIgnoredError.Checked)) Then
                Dim ticketExportedDateCell As New HtmlTableCell()
                With ticketExportedDateCell
                    .Style.Item("vertical-align") = "top"
                    If bgColor <> "" Then .Style.Item("background-color") = ticketRow.BgColor
                End With
                Dim ticketExportedDate As New Label
                With ticketExportedDate
                    .ID = ticketRow.ID & "_ExportedDate"
                    .Text = EncodeAsHtml(String.Format("{0:g}", ticketExportedAt))
                    .Style.Item("vertical-align") = "top"
                End With
                ticketExportedDateCell.Controls.Add(ticketExportedDate)
                ticketRow.Controls.Add(ticketExportedDateCell)
            End If

            Dim ticketOptionsCell As New HtmlTableCell()
            With ticketOptionsCell
                If bgColor <> "" Then .Style.Item("background-color") = ticketRow.BgColor
                .Style.Item("vertical-align") = "top"
                .Style.Item("min-width") = "100px"
            End With
            If exportedData.ToUpper.StartsWith(KaTicketInterfaceExport.EXPORT_FAILED_STATUS_STRING.ToUpper) Then
                Dim ticketClearError As New Button
                With ticketClearError
                    .ID = ticketRow.ID & "_ClearError"
                    .Text = "Clear Error Status"
                    .Enabled = _currentUserPermission(_currentTableName).Edit
                    .EnableViewState = True
                    .Attributes("style") = "min-width : 100px; width: auto;"
                    AddHandler .Click, AddressOf ClearErrorClicked
                End With
                ticketOptionsCell.Controls.Add(ticketClearError)

                Dim ticketIgnoreTicket As New Button
                With ticketIgnoreTicket
                    .ID = ticketRow.ID & "_IgnoreTicket"
                    .Text = "Ignore Error Status"
                    .Enabled = _currentUserPermission(_currentTableName).Edit
                    .EnableViewState = True
                    .Attributes("style") = "min-width : 100px; width: auto;"
                    AddHandler ticketIgnoreTicket.Click, AddressOf IgnoreTicketClicked
                End With
                ticketOptionsCell.Controls.Add(ticketIgnoreTicket)
            ElseIf exportedData.ToUpper.StartsWith(KaTicketInterfaceExport.EXPORT_IGNORED_STATUS_STRING.ToUpper) Then
                Dim ticketClearIgnore As New Button
                With ticketClearIgnore
                    .ID = ticketRow.ID & "_ClearIgnore"
                    .Text = "Clear Ignore Status"
                    .Enabled = _currentUserPermission(_currentTableName).Edit
                    .EnableViewState = True
                    .Attributes("style") = "min-width : 100px; width: auto;"
                    AddHandler .Click, AddressOf Me.ClearErrorClicked
                End With
                ticketOptionsCell.Controls.Add(ticketClearIgnore)
            ElseIf (ticketExported AndAlso ticketInterfaceId = _interfaceId) OrElse Not ticketExportId.Equals(Guid.Empty) Then
                Dim ticketClearStatus As New Button
                With ticketClearStatus
                    .ID = ticketRow.ID & "_ClearStatus"
                    .Text = "Clear Exported Status"
                    .CausesValidation = False
                    .Enabled = _currentUserPermission(_currentTableName).Edit
                    .EnableViewState = True
                    .Attributes("style") = "min-width : 100px; width: auto;"
                    AddHandler .Click, AddressOf ClearExportedStatusClicked
                End With
                ticketOptionsCell.Controls.Add(ticketClearStatus)
            Else
                Dim ticketSetExported As New Button
                With ticketSetExported
                    .ID = ticketRow.ID & "_SetExported"
                    .Text = "Set Exported Status"
                    .Enabled = _currentUserPermission(_currentTableName).Edit
                    .EnableViewState = True
                    .Attributes("style") = "min-width : 100px; width: auto;"
                    AddHandler .Click, AddressOf SetExportedStatusClicked
                End With
                ticketOptionsCell.Controls.Add(ticketSetExported)
            End If
            ticketRow.Controls.Add(ticketOptionsCell)

            Dim ticketReasonCell As New HtmlTableCell()
            With ticketReasonCell
                .Style.Item("vertical-align") = "top"
                If bgColor <> "" Then .Style.Item("background-color") = ticketRow.BgColor
            End With
            Dim ticketReason As New Label
            With ticketReason
                .ID = ticketRow.ID & "_Reason"
                .Text = EncodeAsHtml(exportedData)
                .Style.Item("vertical-align") = "top"
            End With
            ticketReasonCell.Controls.Add(ticketReason)
            ticketRow.Controls.Add(ticketReasonCell)
        Catch ex As RecordNotFoundException

        End Try
    End Sub

    Protected Sub SetExportedStatusClicked(ByVal sender As Object, ByVal e As EventArgs)
        Dim connection As OleDbConnection = Tm2Database.Connection
        Dim controlIdStrings() As String = CType(sender, Button).ID.Split("_")
        Dim currentTicketExportRowId As String = controlIdStrings(0)
        Dim ticketIdTextbox As HtmlInputText = tblTickets.FindControl(currentTicketExportRowId & "_Id")
        Dim ticketId As Guid = Guid.Empty
        If ticketIdTextbox IsNot Nothing AndAlso Guid.TryParse(ticketIdTextbox.Value.ToString, ticketId) Then
            Dim ticketExportRecord As New KaTicketInterfaceExport()
            With ticketExportRecord
                .ExportedAt = Now
                .ExportedData = KaTicketInterfaceExport.EXPORT_MANUAL_STATUS_STRING & "User: " & _currentUser.Name
                .InterfaceId = _interfaceId
                .TicketId = ticketId
                .SqlInsert(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
            End With
        End If
        GetTicketInfo()
    End Sub

    Protected Sub ClearExportedStatusClicked(ByVal sender As Object, ByVal e As EventArgs)
        Dim connection As OleDbConnection = Tm2Database.Connection
        Dim controlIdStrings() As String = CType(sender, Button).ID.Split("_")
        Dim currentTicketExportRowId As String = controlIdStrings(0)
        Dim ticketIdTextbox As HtmlInputText = tblTickets.FindControl(currentTicketExportRowId & "_Id")
        Dim ticketId As Guid = Guid.Empty
        If ticketIdTextbox IsNot Nothing AndAlso Guid.TryParse(ticketIdTextbox.Value.ToString, ticketId) Then
            Try
                Dim ticketInfo As New KaTicket(connection, ticketId)
                If ticketInfo.InterfaceId.Equals(_interfaceId) AndAlso ticketInfo.Exported Then
                    ticketInfo.Exported = False
                    ticketInfo.SqlUpdate(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                End If
            Catch ex As RecordNotFoundException

            End Try
        End If

        Dim ticketExportIdTextbox As HtmlInputText = tblTickets.FindControl(currentTicketExportRowId & "_ExportId")
        Dim ticketExportId As Guid = Guid.Empty
        If ticketExportIdTextbox IsNot Nothing AndAlso Guid.TryParse(ticketExportIdTextbox.Value.ToString, ticketExportId) Then
            Tm2Database.ExecuteNonQuery(connection, "DELETE FROM ticket_interface_export " &
                    "WHERE (ticket_interface_export.interface_id=" & Q(_interfaceId) & ") AND (id=" & Q(ticketExportId) & ")")
        End If
        GetTicketInfo()
    End Sub

    Protected Sub ClearErrorClicked(ByVal sender As Object, ByVal e As EventArgs)
        Dim connection As OleDbConnection = Tm2Database.Connection
        Dim controlIdStrings() As String = CType(sender, Button).ID.Split("_")
        Dim currentTicketExportRowId As String = controlIdStrings(0)
        Dim ticketExportIdTextbox As HtmlInputText = tblTickets.FindControl(currentTicketExportRowId & "_ExportId")
        Dim ticketExportId As Guid = Guid.Empty
        If ticketExportIdTextbox IsNot Nothing AndAlso Guid.TryParse(ticketExportIdTextbox.Value.ToString, ticketExportId) Then
            Tm2Database.ExecuteNonQuery(connection, "DELETE FROM ticket_interface_export " &
                    "WHERE (ticket_interface_export.interface_id=" & Q(_interfaceId) & ") AND (id=" & Q(ticketExportId) & ")")
        End If
        GetTicketInfo()
    End Sub

    Protected Sub IgnoreTicketClicked(ByVal sender As Object, ByVal e As EventArgs)
        Dim connection As OleDbConnection = Tm2Database.Connection
        Dim controlIdStrings() As String = CType(sender, Button).ID.Split("_")
        Dim currentTicketExportRowId As String = controlIdStrings(0)
        Dim ticketExportIdTextbox As HtmlInputText = tblTickets.FindControl(currentTicketExportRowId & "_ExportId")
        Dim ticketExportId As Guid = Guid.Empty
        If ticketExportIdTextbox IsNot Nothing AndAlso Guid.TryParse(ticketExportIdTextbox.Value.ToString, ticketExportId) Then
            Try
                Dim ticketExport As New KaTicketInterfaceExport(connection, ticketExportId)
                ticketExport.ExportedData = ticketExport.ExportedData.Replace(KaTicketInterfaceExport.EXPORT_FAILED_STATUS_STRING, KaTicketInterfaceExport.EXPORT_IGNORED_STATUS_STRING)
                ticketExport.SqlUpdate(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
            Catch ex As RecordNotFoundException

            End Try
        End If
        GetTicketInfo()
    End Sub

    Private Sub rbShowTicketsExportedCheckedChanged(sender As Object, e As System.EventArgs) Handles rbShowTicketsExported.CheckedChanged, rbShowTicketsNotExported.CheckedChanged, rbIncludeTicketsWithoutErrors.CheckedChanged, rbIncludeTicketsWithError.CheckedChanged, rbIncludeTicketsWithIgnoredError.CheckedChanged
        If sender Is rbIncludeTicketsWithoutErrors AndAlso rbIncludeTicketsWithoutErrors.Checked Then
            rbIncludeTicketsWithError.Checked = False
            rbIncludeTicketsWithIgnoredError.Checked = False
        ElseIf sender Is rbIncludeTicketsWithError AndAlso rbIncludeTicketsWithError.Checked Then
            rbIncludeTicketsWithoutErrors.Checked = False
            rbIncludeTicketsWithIgnoredError.Checked = False
        ElseIf sender Is rbIncludeTicketsWithIgnoredError AndAlso rbIncludeTicketsWithIgnoredError.Checked Then
            rbIncludeTicketsWithoutErrors.Checked = False
            rbIncludeTicketsWithError.Checked = False
        ElseIf sender Is rbShowTicketsExported AndAlso rbShowTicketsExported.Checked Then
            rbShowTicketsNotExported.Checked = False
        ElseIf sender Is rbShowTicketsNotExported AndAlso rbShowTicketsNotExported.Checked Then
            rbShowTicketsExported.Checked = False
        End If

        chkIncludeTicketsMarkedManually.Enabled = rbShowTicketsExported.Checked
        chkOnlyIncludeOrdersForThisInterface.Enabled = rbShowTicketsNotExported.Checked And rbIncludeTicketsWithoutErrors.Checked
        rbIncludeTicketsWithoutErrors.Enabled = rbShowTicketsNotExported.Checked
        rbIncludeTicketsWithError.Enabled = rbShowTicketsNotExported.Checked
        rbIncludeTicketsWithIgnoredError.Enabled = rbShowTicketsNotExported.Checked

        GetTicketInfo() ' This is needed to recreate the table for click events
    End Sub

    Private Function EncodeAsHtml(ByVal text As String) As String
        Return Server.HtmlEncode(text).Replace(" ", "&nbsp;").Replace(vbCrLf, "<br />").Replace(vbCr, "<br />").Replace(vbLf, "<br />")
    End Function

    Protected Sub ddlSortBy_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlSortBy.SelectedIndexChanged
        ddlPageNumber.SelectedIndex = 0
        GetTicketInfo()
    End Sub

    Protected Sub ddlTicketsPerPage_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlTicketsPerPage.SelectedIndexChanged
        ddlPageNumber.SelectedIndex = 0
        GetTicketInfo()
    End Sub

    Private Sub ddlPageNumber_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles ddlPageNumber.SelectedIndexChanged
        GetTicketInfo()
    End Sub

    Private Sub btnPreviousPage_Click(sender As Object, e As System.EventArgs) Handles btnPreviousPage.Click
        ddlPageNumber.SelectedIndex = Math.Max(0, Math.Min(ddlPageNumber.SelectedIndex - 1, ddlPageNumber.Items.Count - 1))
        GetTicketInfo()
    End Sub

    Private Sub btnNextPage_Click(sender As Object, e As System.EventArgs) Handles btnNextPage.Click
        ddlPageNumber.SelectedIndex = Math.Max(0, Math.Min(ddlPageNumber.SelectedIndex + 1, ddlPageNumber.Items.Count - 1))
        GetTicketInfo()
    End Sub


#Region " E-mail report "
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

    Protected Sub btnSendEmail_Click(sender As Object, e As EventArgs) Handles btnSendEmail.Click
        Dim message As String = ""
        If Not Utilities.IsEmailFieldValid(tbxEmailTo.Text, message) Then
            ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), "InvalidEmail", Utilities.JsAlert(message), False)
            Exit Sub
        End If

        Dim emailAddresses As String = ""
        If tbxEmailTo.Text.Trim().Length > 0 Then
            Dim sortCriteria As String = "exported_at ASC, number ASC, loaded_at ASC"
            Select Case ddlSortBy.SelectedValue
                Case "ExportedAtDateAsc"
                    sortCriteria = "exported_at ASC"
                Case "ExportedAtDateDesc"
                    sortCriteria = "exported_at DESC"
                Case "LoadedAtDateAsc"
                    sortCriteria = "loaded_at ASC"
                Case "LoadedAtDateDesc"
                    sortCriteria = "loaded_at DESC"
                Case "TicketAsc"
                    sortCriteria = "number ASC"
                Case "TicketDesc"
                    sortCriteria = "number DESC"
            End Select
            Dim header As String = "Ticket Export Status Report - " & ddlInterfaces.SelectedItem.Text
            Dim body As String = KaReports.GetTableHtml(header, KaReports.GetInterfaceTicketExportStatusReport(GetUserConnection(_currentUser.Id), New Guid(ddlInterfaces.SelectedValue), sortCriteria, rbShowTicketsExported.Checked, chkIncludeTicketsMarkedManually.Checked, rbIncludeTicketsWithError.Checked, rbIncludeTicketsWithIgnoredError.Checked, chkOnlyIncludeOrdersForThisInterface.Checked))

            Dim emailTo() As String = tbxEmailTo.Text.Split(New Char() {";", ","})
            For Each emailRecipient As String In emailTo
                If emailRecipient.Trim.Length > 0 Then
                    Dim newEmail As New KaEmail()
                    newEmail.ApplicationId = APPLICATION_ID
                    newEmail.Body = Utilities.CreateSiteCssStyle() & body
                    newEmail.BodyIsHtml = True
                    newEmail.OwnerID = _currentUser.OwnerId
                    newEmail.Recipients = emailRecipient.Trim
                    newEmail.ReportType = KaEmailReport.ReportTypes.Generic
                    newEmail.Subject = header
                    Dim attachments As New List(Of System.Net.Mail.Attachment)
                    attachments.Add(New System.Net.Mail.Attachment(New IO.MemoryStream(Encoding.UTF8.GetBytes(newEmail.Body)), "TicketExportStatusReport" & ddlInterfaces.SelectedItem.Text & ".html", System.Net.Mime.MediaTypeNames.Text.Html))
                    newEmail.SerializeAttachments(attachments)
                    KaEmail.CreateEmail(GetUserConnection(_currentUser.Id), newEmail, -1, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                    If emailAddresses.Length > 0 Then emailAddresses &= ", "
                    emailAddresses &= newEmail.Recipients
                End If
            Next
            If emailAddresses.Length > 0 Then
                litEmailConfirmation.Text = "Report sent to " & emailAddresses
            Else
                litEmailConfirmation.Text = "Report not sent.  No e-mail addresses."
            End If
        End If
    End Sub

    Private Sub btnPrinterFriendly_Click(sender As Object, e As EventArgs) Handles btnPrinterFriendly.Click
        Dim header As String = Server.UrlEncode("Ticket Export Status Report - " & ddlInterfaces.SelectedItem.Text)
        Dim userId As String = Server.UrlEncode(_currentUser.Id.ToString)
        Dim interfaceId As String = Server.UrlEncode(ddlInterfaces.SelectedValue)
        Dim showTicketsExported As String = rbShowTicketsExported.Checked
        Dim includeTicketsMarkedManually As String = chkIncludeTicketsMarkedManually.Checked
        Dim includeTicketsWithError As String = rbIncludeTicketsWithError.Checked
        Dim includeTicketsWithIgnoredError As String = rbIncludeTicketsWithIgnoredError.Checked
        Dim limitOrdersToInterface As String = chkOnlyIncludeOrdersForThisInterface.Checked
        Dim sortCriteria As String = "exported_at ASC, number ASC, loaded_at ASC"
        Select Case ddlSortBy.SelectedValue
            Case "ExportedAtDateAsc"
                sortCriteria = "exported_at ASC"
            Case "ExportedAtDateDesc"
                sortCriteria = "exported_at DESC"
            Case "LoadedAtDateAsc"
                sortCriteria = "loaded_at ASC"
            Case "LoadedAtDateDesc"
                sortCriteria = "loaded_at DESC"
            Case "TicketAsc"
                sortCriteria = "number ASC"
            Case "TicketDesc"
                sortCriteria = "number DESC"
        End Select

        ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), "PrinterFriendly",
                                               Utilities.JsWindowOpen("InterfaceTicketStatusView.aspx?header=" & header &
                                               "&user_id=" & userId &
                                               "&interface_id=" & interfaceId &
                                               "&sort_by=" & Server.UrlEncode(sortCriteria) &
                                               "&tickets_exported=" & showTicketsExported &
                                               "&tickets_marked_manually=" & includeTicketsMarkedManually &
                                               "&tickets_with_error=" & includeTicketsWithError &
                                               "&tickets_with_ignored_error=" & includeTicketsWithIgnoredError &
                                               "&interface_orders_only=" & limitOrdersToInterface), False)

    End Sub

	Private Sub btnDownload_Click(sender As Object, e As EventArgs) Handles btnDownload.Click
		Dim header As String = Server.UrlEncode("Ticket Export Status Report - " & ddlInterfaces.SelectedItem.Text)
		Dim userId As String = Server.UrlEncode(_currentUser.Id.ToString)
		Dim interfaceId As String = Server.UrlEncode(ddlInterfaces.SelectedValue)
		Dim showTicketsExported As String = rbShowTicketsExported.Checked
		Dim includeTicketsMarkedManually As String = chkIncludeTicketsMarkedManually.Checked
		Dim includeTicketsWithError As String = rbIncludeTicketsWithError.Checked
		Dim includeTicketsWithIgnoredError As String = rbIncludeTicketsWithIgnoredError.Checked
		Dim limitOrdersToInterface As String = chkOnlyIncludeOrdersForThisInterface.Checked
		Dim sortCriteria As String = "exported_at ASC, number ASC, loaded_at ASC"
		Select Case ddlSortBy.SelectedValue
			Case "ExportedAtDateAsc"
				sortCriteria = "exported_at ASC"
			Case "ExportedAtDateDesc"
				sortCriteria = "exported_at DESC"
			Case "LoadedAtDateAsc"
				sortCriteria = "loaded_at ASC"
			Case "LoadedAtDateDesc"
				sortCriteria = "loaded_at DESC"
			Case "TicketAsc"
				sortCriteria = "number ASC"
			Case "TicketDesc"
				sortCriteria = "number DESC"
		End Select
		Dim fileName As String = String.Format("TicketStatusReport{0:yyyyMMddHHmmss}.csv", Now)
		KaReports.CreateCsvFromTable(header, "", KaReports.GetInterfaceTicketExportStatusReport(GetUserConnection(New Guid(userId)), New Guid(interfaceId), sortCriteria, showTicketsExported, includeTicketsMarkedManually, includeTicketsWithError, includeTicketsWithIgnoredError, limitOrdersToInterface), DownloadDirectory(Me) & fileName)

		ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), "DownloadReport", Utilities.JsWindowOpen("./download/" & fileName), False)
	End Sub

	Protected Sub ScriptManager1_AsyncPostBackError(ByVal sender As Object, ByVal e As System.Web.UI.AsyncPostBackErrorEventArgs)
		ToolkitScriptManager1.AsyncPostBackErrorMessage = e.Exception.Message
	End Sub
#End Region

End Class
