Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class PastOrders : Inherits System.Web.UI.Page
#Region "Events"
    Private _currentUser As KaUser
    Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
    Private _currentTableName As String = KaOrder.TABLE_NAME
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Response.Cache.SetCacheability(HttpCacheability.NoCache)
        _currentUser = Utilities.GetUser(Me)
        _currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Orders")
        If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")
        If Not Page.IsPostBack Then
            SetTextboxMaxLengths()
            PopulateFacilityList()
            If Page.Request("OrderId") Is Nothing Then
                Try
                    ddlFacilityFilter.SelectedValue = KaSetting.GetSetting(GetUserConnection(_currentUser.Id), "OrdersPage:" & _currentUser.Id.ToString & "/LastFacilityUsed", Guid.Empty.ToString())
                Catch ex As ArgumentOutOfRangeException

                End Try
            Else
                ddlFacilityFilter.SelectedIndex = 0
            End If
            PopulatePastOrders()
            Dim orderId As Guid = Guid.Empty
            Guid.TryParse(Page.Request("OrderId"), orderId)
            Try
                ddlPastOrders.SelectedValue = orderId.ToString()
            Catch ex As ArgumentOutOfRangeException

            End Try
            ddlPastOrders_SelectedIndexChanged(ddlPastOrders, New EventArgs())
            Utilities.ConfirmBox(btnMarkIncomplete, "Are you sure that you want to mark this order as incomplete?")
            Utilities.ConfirmBox(btnArchive, "Are you sure that you want to archive this order?")
            Utilities.ConfirmBox(btnUnarchive, "Are you sure that you want to remove the Archive flag from this order?")
        ElseIf Page.IsPostBack And Request("__EVENTARGUMENT").StartsWith("ArchiveTickets") Then
            ArchiveTickets(Request("__EVENTARGUMENT").Replace("ArchiveTickets('", "").Replace("')", ""))
        End If
    End Sub

    Protected Sub ddlPastOrders_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles ddlPastOrders.SelectedIndexChanged
        Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
        Dim markIncompleteEnabled As Boolean = (_currentUserPermission(_currentTableName).Edit)
        Dim markIncompleteDisabledReasons As String = ""
        If ddlPastOrders.SelectedIndex > 0 Then
            Try
                Dim orderId As Guid = Guid.Parse(ddlPastOrders.SelectedValue)
                Dim order As New KaOrder(connection, orderId)
                Dim url As String = Reports.OrderSummaryUrlForOwner(connection, order.OwnerId)
                If url.ToLower.IndexOf("order_id=") = -1 Then
                    url &= IIf(url.IndexOf("?") = -1, "?", "&") & "order_id="
                End If
                url &= ddlPastOrders.SelectedValue
                If url.ToLower.IndexOf("location_id=") = -1 Then
                    url &= IIf(url.IndexOf("?") = -1, "?", "&") & "location_id=" & ddlFacilityFilter.SelectedValue
                End If
                If Utilities.ConvertWebPageUrlDomainToRequestedPagesDomain(connection) Then url = Tm2Database.GetUrlInCurrentDomain(Tm2Database.GetCurrentPageUrlWithOriginalPort(Me.Request), url)
                orderSummaryFrame.Attributes("src") = url
                If order.Archived Then
                    markIncompleteEnabled = False
                    markIncompleteDisabledReasons = "Order archived.  "
                End If

                Dim interfaceTypeRdr As OleDbDataReader = Tm2Database.ExecuteReader(connection, String.Format("SELECT orders.id, interface_types.allow_order_status_change_tickets_exist, COUNT(tickets.id) AS ticketCount " &
                                    "FROM interface_types " &
                                    "INNER JOIN interfaces ON interfaces.interface_type_id = interface_types.id " &
                                    "INNER JOIN orders ON orders.interface_id = interfaces.id " &
                                    "LEFT OUTER JOIN tickets ON tickets.order_id = orders.id " &
                                    "WHERE (interface_types.deleted = 0) AND (interfaces.deleted = 0)  AND (orders.id = {0}) " &
                                    "GROUP BY orders.id, interface_types.allow_order_status_change_tickets_exist", Q(orderId)))
                If interfaceTypeRdr.Read() AndAlso Not interfaceTypeRdr.Item("allow_order_status_change_tickets_exist") AndAlso interfaceTypeRdr.Item("ticketCount") > 0 Then
                    markIncompleteEnabled = False
                    markIncompleteDisabledReasons &= IIf(markIncompleteDisabledReasons.Length > 0, vbCrLf, "") & "Interface type ticket rule."
                End If
                btnArchive.Enabled = (_currentUserPermission(_currentTableName).Edit) AndAlso Not order.Archived
                btnUnarchive.Enabled = (_currentUserPermission(_currentTableName).Edit) AndAlso order.Archived
                btnPrinterFriendly.Enabled = True

                Try
                    Dim owner As New KaOwner(connection, order.OwnerId)
                    If owner.Deleted Then
                        markIncompleteEnabled = False
                        markIncompleteDisabledReasons &= IIf(markIncompleteDisabledReasons.Length > 0, vbCrLf, "") & "Owner " & owner.Name & " deleted.  "
                    End If
                Catch ex As RecordNotFoundException
                    btnMarkIncomplete.Enabled = False
                    markIncompleteDisabledReasons &= IIf(markIncompleteDisabledReasons.Length > 0, vbCrLf, "") & "Owner not defined.  "

                End Try

                Try
                    Dim branch As New KaBranch(connection, order.BranchId)
                    If branch.Deleted Then
                        markIncompleteEnabled = False
                        markIncompleteDisabledReasons &= IIf(markIncompleteDisabledReasons.Length > 0, vbCrLf, "") & "Branch " & branch.Name & " deleted.  "
                    End If
                Catch ex As RecordNotFoundException

                End Try

                Try
                    For Each orderItem As KaOrderItem In order.OrderItems
                        Dim product As New KaProduct(connection, orderItem.ProductId)
                        If product.Deleted Then
                            markIncompleteEnabled = False
                            markIncompleteDisabledReasons &= IIf(markIncompleteDisabledReasons.Length > 0, vbCrLf, "") & "Product " & product.Name & " deleted."
                        End If
                    Next
                    For Each orderAccount As KaOrderCustomerAccount In order.OrderAccounts
                        Dim customerAccount As New KaCustomerAccount(connection, orderAccount.CustomerAccountId)
                        If customerAccount.Deleted Then
                            markIncompleteEnabled = False
                            markIncompleteDisabledReasons &= IIf(markIncompleteDisabledReasons.Length > 0, vbCrLf, "") & "Customer " & customerAccount.Name & " deleted."
                        End If
                    Next
                    If order.CustomerAccountLocationId <> Guid.Empty Then
                        Dim customerAccountLocation As New KaCustomerAccountLocation(connection, order.CustomerAccountLocationId)
                        If customerAccountLocation.Deleted Then
                            markIncompleteEnabled = False
                            markIncompleteDisabledReasons &= IIf(markIncompleteDisabledReasons.Length > 0, vbCrLf, "") & "Customer destination " & customerAccountLocation.Name & " deleted."
                        End If
                    End If
                Catch ex As Exception
                    markIncompleteEnabled = False
                End Try
				Try
					Dim applicator As New KaApplicator(connection, order.ApplicatorId)
					If applicator.Deleted Then
						markIncompleteEnabled = False
						markIncompleteDisabledReasons &= IIf(markIncompleteDisabledReasons.Length > 0, vbCrLf, "") & "Applicator " & applicator.Name & " deleted.  "
					End If
				Catch ex3 As RecordNotFoundException
				End Try
				Try
					Dim i As New KaInterface(connection, order.InterfaceId)
					If i.Deleted Then
						markIncompleteEnabled = False
						markIncompleteDisabledReasons &= IIf(markIncompleteDisabledReasons.Length > 0, vbCrLf, "") & "Interface " & i.Name & " deleted.  "
					End If
				Catch ex As RecordNotFoundException
				End Try
			Catch ex As RecordNotFoundException
				orderSummaryFrame.Attributes("src") = ""
                markIncompleteEnabled = False
                btnArchive.Enabled = False
                btnUnarchive.Enabled = False
            End Try
        Else
            orderSummaryFrame.Attributes("src") = ""
            markIncompleteEnabled = False
            btnArchive.Enabled = False
            btnUnarchive.Enabled = False
            btnPrinterFriendly.Enabled = False
        End If
        btnMarkIncomplete.Enabled = markIncompleteEnabled
        btnMarkIncomplete.ToolTip = IIf(btnMarkIncomplete.Enabled, "", "Cannot mark incomplete due to: " & vbCrLf & Server.HtmlEncode(markIncompleteDisabledReasons))
    End Sub

    Protected Sub btnPrinterFriendly_Click(sender As Object, e As EventArgs) Handles btnPrinterFriendly.Click
        Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
        Dim orderId As Guid = Guid.Parse(ddlPastOrders.SelectedValue)
        Dim order As New KaOrder(connection, orderId)
        Dim url As String = Reports.OrderSummaryUrlForOwner(connection, order.OwnerId)
        If url.ToLower.IndexOf("order_id=") = -1 Then
            url &= IIf(url.IndexOf("?") = -1, "?", "&") & "order_id="
        End If
        url &= ddlPastOrders.SelectedValue & "&pfv=true"
        If url.ToLower.IndexOf("location_id=") = -1 Then
            url &= IIf(url.IndexOf("?") = -1, "?", "&") & "location_id=" & ddlFacilityFilter.SelectedValue
        End If

        If Utilities.ConvertWebPageUrlDomainToRequestedPagesDomain(connection) Then url = Tm2Database.GetUrlInCurrentDomain(Tm2Database.GetCurrentPageUrlWithOriginalPort(Me.Request), url)
        ClientScript.RegisterClientScriptBlock(Me.GetType(), "ShowReport", Utilities.JsWindowOpen(url))
    End Sub

    Protected Sub btnMarkIncomplete_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnMarkIncomplete.Click
        With New KaOrder(GetUserConnection(_currentUser.Id), Guid.Parse(ddlPastOrders.SelectedValue))
            .Completed = False
            .Archived = False
            .SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
            PopulatePastOrders()
        End With
    End Sub

    Protected Sub btnFind_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnFind.Click
        Dim orders As New List(Of Guid) ' get a list of the order IDs
        If tbxFind.Text.Trim.Length > 0 Then orders = KaOrder.GetOrderIdsWithKeyword(GetUserConnection(_currentUser.Id), Nothing, _currentUser.OwnerId, tbxFind.Text.Trim, True) ' get a list of the order IDs

        Dim found As Boolean = False
        Dim i As Integer = ddlPastOrders.SelectedIndex ' begin with the next order in the drop-down list
        Do
            If i + 1 = ddlPastOrders.Items.Count Then i = 0 Else i += 1 ' wrap around to the beginning of the drop-down list
            If orders.IndexOf(Guid.Parse(ddlPastOrders.Items(i).Value)) <> -1 Then ' this is one of the orders that was found, select it
                ddlPastOrders.SelectedIndex = i
                found = True
                Exit Do ' no need to look any further
            End If
        Loop While i <> ddlPastOrders.SelectedIndex ' continue until we've come back to where we started
        If found Then
            ddlPastOrders_SelectedIndexChanged(sender, e)
        Else
            ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidKeyword", Utilities.JsAlert("Order not found containing keywords: " & tbxFind.Text))
        End If
    End Sub
#End Region

    Private Sub SetTextboxMaxLengths()
        tbxFind.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaOrder.TABLE_NAME, "number"))
    End Sub

    Private Sub PopulatePastOrders()
        Dim currentOrderId As String = Guid.Empty.ToString()
        If ddlPastOrders.SelectedIndex >= 0 Then currentOrderId = ddlPastOrders.SelectedValue

        Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
        Dim showReleaseNumber As Boolean = Boolean.Parse(KaSetting.GetSetting(connection, "General/ShowReleaseNumberInOrderList", "False"))
        ddlPastOrders.Items.Clear()
        ddlPastOrders.Items.Add(New ListItem("Select an order", Guid.Empty.ToString))

        Dim getOrdersRdr As OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), "SELECT id, number, release_number, archived FROM orders " &
                        "WHERE completed = 1 AND deleted = 0 " &
                        IIf(cbxIncludeArchived.Checked, "", "AND archived = 0 ") &
                        IIf(ddlFacilityFilter.SelectedValue = Guid.Empty.ToString(), "", "AND id IN (SELECT order_id FROM order_items WHERE (deleted = 0) AND product_id in (select product_id from product_bulk_products where location_id = " & Q(ddlFacilityFilter.SelectedValue) & " and deleted = 0))") &
                        IIf(_currentUser.OwnerId.Equals(Guid.Empty), "", "AND orders.owner_id = " & Q(_currentUser.OwnerId) & " ") &
                        "ORDER BY number ASC")
        Do While getOrdersRdr.Read
            Dim displayText As String = getOrdersRdr.Item("number")
            If getOrdersRdr.Item("release_number") <> "" Then
                displayText &= " (" & getOrdersRdr.Item("release_number") & ")"
            End If
            If getOrdersRdr.Item("archived") Then displayText &= " (Archived)"
            ddlPastOrders.Items.Add(New ListItem(displayText, getOrdersRdr.Item("id").ToString()))
        Loop
        getOrdersRdr.Close()

        Try
            ddlPastOrders.SelectedValue = currentOrderId
        Catch ex As ArgumentOutOfRangeException

        End Try
        ddlPastOrders_SelectedIndexChanged(ddlPastOrders, New EventArgs)
    End Sub

    Private Sub PopulateFacilityList()
        ddlFacilityFilter.Items.Clear()
        ddlFacilityFilter.Items.Add(New ListItem("All facilities", Guid.Empty.ToString()))
        For Each u As KaLocation In KaLocation.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
            ddlFacilityFilter.Items.Add(New ListItem(u.Name, u.Id.ToString))
        Next
    End Sub

    Protected Sub ddlFacilityFilter_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlFacilityFilter.SelectedIndexChanged
        PopulatePastOrders()
        KaSetting.WriteSetting(GetUserConnection(_currentUser.Id), "OrdersPage:" & _currentUser.Id.ToString & "/LastFacilityUsed", ddlFacilityFilter.SelectedValue)
    End Sub

    Protected Sub cbxIncludeArchived_CheckedChanged(sender As Object, e As EventArgs) Handles cbxIncludeArchived.CheckedChanged
        PopulatePastOrders()
    End Sub

    Protected Sub btnArchive_Click(sender As Object, e As EventArgs) Handles btnArchive.Click
        Try
            Dim orderInfo As New KaOrder(GetUserConnection(_currentUser.Id), Guid.Parse(ddlPastOrders.SelectedValue))
            orderInfo.Archived = True
            orderInfo.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)

            Dim getUnarchivedTicketCountRdr As OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), "SELECT count(*) FROM tickets WHERE order_id = " & Q(orderInfo.Id) & " AND archived = 0")

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
        PopulatePastOrders()
    End Sub

    Private Sub btnUnarchive_Click(sender As Object, e As System.EventArgs) Handles btnUnarchive.Click
        Try
            Dim orderInfo As New KaOrder(GetUserConnection(_currentUser.Id), Guid.Parse(ddlPastOrders.SelectedValue))
            orderInfo.Archived = False
            orderInfo.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
        Catch ex As RecordNotFoundException

        End Try
        PopulatePastOrders()
    End Sub

    Private Sub ArchiveTickets(ByVal orderId As String)
        Tm2Database.ExecuteNonQuery(GetUserConnection(_currentUser.Id), String.Format("UPDATE tickets " &
                    "SET archived = 1, last_updated_application = {0}, last_updated_user = {1} " &
                    "WHERE order_id = {2} AND archived = 0", Q(Database.ApplicationIdentifier), Q(_currentUser.Name), Q(orderId)))
    End Sub

    Private Function EncodeAsHtml(ByVal text As String) As String
        Return Server.HtmlEncode(text).Replace(vbCrLf, "<br />").Replace(vbCr, "<br />").Replace(vbLf, "<br />")
    End Function
End Class