Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class ordersummary : Inherits System.Web.UI.Page
    Private _showAcres As Boolean = False
    Private _showBranch As Boolean = False
    Private _showCustomerAccountNumber As Boolean = False
    Private _showShipto As Boolean = False
    Private _showEmailAddress As Boolean = False
    Private _showInterface As Boolean = False
    Private _showNotes As Boolean = False
    Private _showPoNumber As Boolean = False
    Private _showReleaseNumber As Boolean = False
    Private _orderSummaryCustomFieldsTable As New DataTable
    Private _useTicketDeliveredAmounts As Boolean = False
    Private _additionalUnitsList As New List(Of KaUnit)
    Private _locationId As Guid = Guid.Empty

    Public Const SN_SHOW_ACRES_ON_ORDER_SUMMARY As String = "ShowAcres"
    Public Const SN_SHOW_BRANCH_ON_ORDER_SUMMARY As String = "ShowBranch"
    Public Const SN_SHOW_CUSTOMER_ACCOUNT_NUMBER_ON_ORDER_SUMMARY As String = "ShowCustomerAccountNumber"
    Public Const SN_SHOW_EMAIL_ADDRESS_ON_ORDER_SUMMARY As String = "ShowEmailAddress"
    Public Const SN_SHOW_INTERFACE_ON_ORDER_SUMMARY As String = "ShowInterface"
    Public Const SN_SHOW_NOTES_ON_ORDER_SUMMARY As String = "ShowNotes"
    Public Const SN_SHOW_SHIP_TO_ON_ORDER_SUMMARY As String = "ShowShipTo"
    Public Const SN_SHOW_PO_NUMBER_ON_ORDER_SUMMARY As String = "ShowPoNumber"
    Public Const SN_SHOW_RELEASE_NUMBER_ON_ORDER_SUMMARY As String = "ShowReleaseNumber"
    Public Const SN_USE_TICKET_DELIVERED_AMOUNTS_ON_ORDER_SUMMARY As String = "UseTicketDeliveredAmounts"
    Public Const SN_ADDITIONAL_UNITS_ON_ORDER_SUMMARY As String = "AdditionalUnits"
    Public Const SN_SHOW_ALL_CUSTOM_FIELDS_ON_ORDER_SUMMARY As String = "ShowAllCustomFields"
    Public Const SN_CUSTOM_FIELDS_ON_ORDER_SUMMARY As String = "CustomFieldsShown"
    Public Const SN_ADDITIONAL_UNITS_FOR_PRODUCT_GROUPS = "AdditionalUnitsForProductGroup"

#Region "Events"
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim showTicketLinks As Boolean = True
        If Request.QueryString("pfv") IsNot Nothing Then showTicketLinks = False
        Try ' to parse the supplied order ID...
            Dim orderId As Guid = Guid.Parse(Request.QueryString("order_id"))
            If Request.QueryString("location_id") Is Nothing OrElse Not Guid.TryParse(Request.QueryString("location_id"), _locationId) Then _locationId = Guid.Empty
            Dim connection As OleDbConnection = Tm2Database.Connection
            Dim order As New KaOrder(connection, orderId)
            GetSettings(order)
            litOrderNumber.Text = order.Number
            litSoldBy.Text = "<ul>"
            Try ' to get the sold by information
                Dim owner As New KaOwner(connection, order.OwnerId)
                litSoldBy.Text = String.Format("<li class=""name"">{0}</li>", owner.Name)
                If owner.Street.Trim().Length > 0 Then litSoldBy.Text &= String.Format("<li>{0}</li>", owner.Street)
                If (owner.City & owner.State & owner.ZipCode).Trim().Length > 0 Then litSoldBy.Text &= String.Format("<li>{0} {1} {2}</li>", owner.City, owner.State, owner.ZipCode)
                If owner.Phone.Trim().Length > 0 Then litSoldBy.Text &= String.Format("<li>{0}</li>", owner.Phone)
                If _showEmailAddress AndAlso owner.Email.Trim().Length > 0 Then litSoldBy.Text &= String.Format("<li>{0}</li>", owner.Email)
                Utilities.GetCustomField(litSoldBy.Text, _orderSummaryCustomFieldsTable, KaOwner.TABLE_NAME, order.Id)
            Catch ex As RecordNotFoundException
                litSoldBy.Text = ""
            End Try
            litSoldBy.Text &= "</ul>"

            litBranch.Text = "<ul>"
            Try ' to get the sold by information
                Dim branch As New KaBranch(connection, order.BranchId)
                pnlBranchSection.Visible = _showBranch
                litBranch.Text = String.Format("<li class=""name"">{0}</li>", branch.Name)
                If branch.Street.Trim().Length > 0 Then litBranch.Text &= String.Format("<li>{0}</li>", branch.Street)
                If (branch.City & branch.State & branch.ZipCode).Trim().Length > 0 Then litBranch.Text &= String.Format("<li>{0} {1} {2}</li>", branch.City, branch.State, branch.ZipCode)
                If branch.Phone.Trim().Length > 0 Then litBranch.Text &= String.Format("<li>{0}</li>", branch.Phone)
                If _showEmailAddress AndAlso branch.Email.Trim().Length > 0 Then litBranch.Text &= String.Format("<li>{0}</li>", branch.Email)
                Utilities.GetCustomField(litBranch.Text, _orderSummaryCustomFieldsTable, KaBranch.TABLE_NAME, order.Id)
            Catch ex As RecordNotFoundException
                pnlBranchSection.Visible = False
                litBranch.Text = ""
            End Try
            litBranch.Text &= "</ul>"

            litSoldTo.Text = "<ul>" ' get the customer information
            For Each orderCustomerAccount As KaOrderCustomerAccount In order.OrderAccounts
                Try
                    Dim customerAccount As New KaCustomerAccount(connection, orderCustomerAccount.CustomerAccountId)
                    Dim accountNumber As String = GetCustomerNumber(orderCustomerAccount)
                    litSoldTo.Text &= String.Format("<li class=""name"">{0}", customerAccount.Name) & IIf(_showCustomerAccountNumber AndAlso accountNumber.Trim.Length > 0, " (Number: " & accountNumber & ")", "") & IIf(order.OrderAccounts.Count > 1, String.Format(" ({0:0.0}%)", orderCustomerAccount.Percentage), "") & "</li>"
                    If customerAccount.Street.Trim().Length > 0 Then litSoldTo.Text &= String.Format("<li>{0}</li>", customerAccount.Street)
                    If (customerAccount.City & customerAccount.State & customerAccount.ZipCode).Trim().Length > 0 Then litSoldTo.Text &= String.Format("<li>{0} {1} {2}</li>", customerAccount.City, customerAccount.State, customerAccount.ZipCode)
                    If customerAccount.Phone.Trim().Length > 0 Then litSoldTo.Text &= String.Format("<li>{0}</li>", customerAccount.Phone)
                    If _showEmailAddress AndAlso customerAccount.Email.Trim().Length > 0 Then litSoldTo.Text &= String.Format("<li>{0}</li>", customerAccount.Email)
                    Utilities.GetCustomField(litSoldTo.Text, _orderSummaryCustomFieldsTable, KaCustomerAccount.TABLE_NAME, order.Id)
                Catch ex As Exception
                End Try
            Next
            litSoldTo.Text &= "</ul>"

            If _showShipto Then ' get the ship to information
                Dim shipToInfo As String = ""
                If (order.ShipToName.Trim & order.ShipToStreet.Trim & order.ShipToCity.Trim & order.ShipToState.Trim & order.ShipToZipCode.Trim & order.ShipToCountry.Trim).Length > 0 Then
                    shipToInfo &= String.Format("<li class=""name"">{0}</li>", order.ShipToName)
                    If order.ShipToStreet.Trim().Length > 0 Then shipToInfo &= String.Format("<li>{0}</li>", order.ShipToStreet)
                    If (order.ShipToCity & order.ShipToState & order.ShipToZipCode).Trim().Length > 0 Then shipToInfo &= String.Format("<li>{0} {1} {2}</li>", order.ShipToCity, order.ShipToState, order.ShipToZipCode)
                Else
                    Try
                        Dim customerAccountLocation As New KaCustomerAccountLocation(connection, order.CustomerAccountLocationId)
                        shipToInfo &= String.Format("<li class=""name"">{0}</li>", customerAccountLocation.Name)
                        If customerAccountLocation.Street.Trim().Length > 0 Then shipToInfo &= String.Format("<li>{0}</li>", customerAccountLocation.Street)
                        If (customerAccountLocation.City & customerAccountLocation.State & customerAccountLocation.ZipCode).Trim().Length > 0 Then shipToInfo &= String.Format("<li>{0} {1} {2}</li>", customerAccountLocation.City, customerAccountLocation.State, customerAccountLocation.ZipCode)
                        If _showEmailAddress AndAlso customerAccountLocation.Email.Trim().Length > 0 Then shipToInfo &= String.Format("<li>{0}</li>", customerAccountLocation.Email)
                        Utilities.GetCustomField(litShipToSection.Text, _orderSummaryCustomFieldsTable, KaCustomerAccountLocation.TABLE_NAME, order.Id)
                    Catch ex As Exception
                    End Try
                End If

                litShipToSection.Text = "<ul>" & shipToInfo & "</ul>"
                pnlShipToSection.Visible = (shipToInfo.Length > 0)
            Else
                pnlShipToSection.Visible = False
            End If

            Dim orderHeaderSection As String = ""
            If _showPoNumber AndAlso order.PurchaseOrder.Trim.Length > 0 Then orderHeaderSection &= String.Format("<li><span style=""font-weight: bold;"">PO number:</span> {0}</li>", order.PurchaseOrder)
            If _showReleaseNumber AndAlso order.ReleaseNumber.Trim.Length > 0 Then orderHeaderSection &= String.Format("<li><span style=""font-weight: bold;"">Release number:</span> {0}</li>", order.ReleaseNumber)
            If _showAcres AndAlso order.Acres > 0 Then orderHeaderSection &= String.Format("<li><span style=""font-weight: bold;"">Acres:</span> {0}</li>", order.Acres)
            If _showInterface Then
                Try
                    orderHeaderSection &= String.Format("<li><span style=""font-weight: bold;"">Interface:</span> {0}</li>", New KaInterface(connection, order.InterfaceId).Name)
                Catch ex As RecordNotFoundException

                End Try
            End If
            If _showNotes AndAlso order.Notes.Trim.Length > 0 Then orderHeaderSection &= String.Format("<li><span style=""font-weight: bold;"">Notes:</span> {0}</li>", order.Notes)
            If orderHeaderSection.Length > 0 Then
                litOrderHeaderSection.Text = "<ul>" & orderHeaderSection & "</ul>"
                pnlOrderHeaderSection.Visible = True
            Else
                pnlOrderHeaderSection.Visible = False
            End If

            Dim ticketUrl As String = WebTicketUrlForOwner(connection, order.OwnerId)
            If ticketUrl.ToLower.IndexOf("ticket_id=") = -1 Then
                ticketUrl &= IIf(ticketUrl.IndexOf("?") = -1, "?", "&") & "ticket_id="
            End If
            If Utilities.ConvertWebPageUrlDomainToRequestedPagesDomain(connection) Then ticketUrl = Tm2Database.GetUrlInCurrentDomain(Tm2Database.GetCurrentPageUrlWithOriginalPort(Me.Request), ticketUrl)
            ticketUrl = "<a href=""" & ticketUrl & "{1}"" target=""_blank"">{0}</a>"
            Dim table As HtmlTable = FindControl("tblTickets")
            For Each ticket As KaTicket In KaTicket.GetAll(connection, "order_id=" & Q(orderId), "loaded_at ASC")
                Dim row As New HtmlTableRow()
                Dim cell As New HtmlTableCell()
                cell.InnerText = String.Format("{0:g}", ticket.LoadedAt)
                row.Cells.Add(cell)
                cell = New HtmlTableCell()
                cell.InnerText = ticket.Number
                If (showTicketLinks) Then
                    cell.InnerHtml = String.Format(ticketUrl, ticket.Number, ticket.Id.ToString())
                End If
                row.Cells.Add(cell)
                cell = New HtmlTableCell()
                cell.InnerText = ticket.ApplicatorName
                row.Cells.Add(cell)
                cell = New HtmlTableCell()
                cell.InnerText = ticket.ShipToName
                row.Cells.Add(cell)
                cell = New HtmlTableCell()
                cell.InnerText = String.Format("{0:0.0}", ticket.Acres)
                row.Cells.Add(cell)
                cell = New HtmlTableCell()
                cell.InnerText = ""
                For Each ticketTransport As KaTicketTransport In ticket.TicketTransports
                    cell.InnerText &= IIf(cell.InnerText.Length > 0, ", ", "") & ticketTransport.Name & IIf(ticketTransport.Number.Length > 0 AndAlso ticketTransport.Number <> ticketTransport.Name, " (" & ticketTransport.Number & ")", "")
                Next
                row.Cells.Add(cell)
                table.Rows.Add(row)
            Next
            pnlticketSection.Visible = (table.Rows.Count > 1)

            litOrderDetails.Text = ""
            litOrderDetails.Text &= "<table>"
            Dim pastHeader As Boolean = False

            Dim productGroupAdditionalUnits As New Dictionary(Of Guid, List(Of KaUnit))
            For Each productGroup As KaProductGroup In KaProductGroup.GetAll(connection, "deleted=0", "name")
                Dim orderSummarySettingFormat As String = "OrderSummarySetting:{0}:{1}/" & SN_ADDITIONAL_UNITS_FOR_PRODUCT_GROUPS
                Dim orderSummarySetting As String = String.Format(orderSummarySettingFormat, Guid.Empty.ToString(), productGroup.Id.ToString())
                Dim defaultOwnerProductUnits As String = KaSetting.GetSetting(Tm2Database.Connection, orderSummarySetting, "")
                orderSummarySetting = String.Format(orderSummarySettingFormat, order.OwnerId.ToString(), productGroup.Id.ToString())
                productGroupAdditionalUnits.Add(productGroup.Id, New List(Of KaUnit))
                For Each unitIdString As String In KaSetting.GetSetting(Tm2Database.Connection, orderSummarySetting, defaultOwnerProductUnits, False, Nothing).Trim().Split(",")
                    Try ' to parse the unit ID...
                        Dim unitId As Guid = Guid.Empty
                        If Guid.TryParse(unitIdString, unitId) AndAlso Not unitId.Equals(Guid.Empty) Then
                            Dim unitInfo As New KaUnit(connection, unitId)
                            If Not productGroupAdditionalUnits(productGroup.Id).Contains(unitInfo) Then productGroupAdditionalUnits(productGroup.Id).Add(unitInfo)
                        End If
                    Catch ex As FormatException ' couldn't parse the unit ID, ignore it...
                    Catch ex As RecordNotFoundException ' couldn't parse the unit ID, ignore it...
                    End Try
                Next
            Next
            For Each row As ArrayList In KaReports.GetOrderProductSummary(connection, orderId, _useTicketDeliveredAmounts, True, Integer.MaxValue, _additionalUnitsList, _locationId, productGroupAdditionalUnits)
                litOrderDetails.Text &= "<tr>"
                For Each column As String In row
                    litOrderDetails.Text &= String.Format(IIf(pastHeader, "<td>{0}</td>", "<th>{0}</th>"), column)
                Next
                litOrderDetails.Text &= "</tr>"
                If Not pastHeader Then pastHeader = True
            Next
            litOrderDetails.Text &= "</table>"
        Catch ex As ArgumentNullException ' the order ID wasn't specified...
        Catch ex As FormatException ' the order ID isn't formatted correctly...
        Catch ex As RecordNotFoundException ' the order isn't in the database...
        End Try
    End Sub

    Private Sub GetSettings(order As KaOrder)
        Boolean.TryParse(GetSettingByOwnerId(order.OwnerId, SN_SHOW_ACRES_ON_ORDER_SUMMARY, True.ToString()), _showAcres)
        Boolean.TryParse(GetSettingByOwnerId(order.OwnerId, SN_SHOW_BRANCH_ON_ORDER_SUMMARY, False.ToString()), _showBranch)
        Boolean.TryParse(GetSettingByOwnerId(order.OwnerId, SN_SHOW_CUSTOMER_ACCOUNT_NUMBER_ON_ORDER_SUMMARY, False.ToString()), _showCustomerAccountNumber)
        Boolean.TryParse(GetSettingByOwnerId(order.OwnerId, SN_SHOW_EMAIL_ADDRESS_ON_ORDER_SUMMARY, False.ToString()), _showEmailAddress)
        Boolean.TryParse(GetSettingByOwnerId(order.OwnerId, SN_SHOW_INTERFACE_ON_ORDER_SUMMARY, False.ToString()), _showInterface)
        Boolean.TryParse(GetSettingByOwnerId(order.OwnerId, SN_SHOW_NOTES_ON_ORDER_SUMMARY, True.ToString()), _showNotes)
        Boolean.TryParse(GetSettingByOwnerId(order.OwnerId, SN_SHOW_SHIP_TO_ON_ORDER_SUMMARY, False.ToString()), _showShipto)
        Boolean.TryParse(GetSettingByOwnerId(order.OwnerId, SN_SHOW_PO_NUMBER_ON_ORDER_SUMMARY, True.ToString()), _showPoNumber)
        Boolean.TryParse(GetSettingByOwnerId(order.OwnerId, SN_SHOW_RELEASE_NUMBER_ON_ORDER_SUMMARY, True.ToString()), _showReleaseNumber)
        For Each unitId As String In GetSettingByOwnerId(order.OwnerId, SN_ADDITIONAL_UNITS_ON_ORDER_SUMMARY, "").Trim().Split(",")
            Try
                Dim newUnitId As Guid = Guid.Empty
                If Guid.TryParse(unitId, newUnitId) Then _additionalUnitsList.Add(New KaUnit(Tm2Database.Connection, newUnitId))
            Catch ex As RecordNotFoundException

            End Try
        Next

        Boolean.TryParse(GetSettingByOwnerId(order.OwnerId, SN_USE_TICKET_DELIVERED_AMOUNTS_ON_ORDER_SUMMARY, True.ToString()), _useTicketDeliveredAmounts)

        GetCustomFields(order.OwnerId, order.Id)
    End Sub

    Private Sub GetCustomFields(ByVal ownerId As Guid, ByVal orderId As Guid)
        ' Custom fields     
        Dim showAllCustomFieldsOnDeliveryTicket As Boolean = True
        Boolean.TryParse(GetSettingByOwnerId(ownerId, SN_SHOW_ALL_CUSTOM_FIELDS_ON_ORDER_SUMMARY, showAllCustomFieldsOnDeliveryTicket), showAllCustomFieldsOnDeliveryTicket)
        Dim customFieldsShown As String = ""
        For Each customFieldShown As String In GetSettingByOwnerId(ownerId, SN_CUSTOM_FIELDS_ON_ORDER_SUMMARY, "").ToString().Split(",")
            If customFieldShown.Length > 0 Then
                If customFieldsShown.Length > 0 Then customFieldsShown &= ","
                customFieldsShown &= Q(customFieldShown)
            End If
        Next
        ' not showing all custom fields, and not having any checked will cause the IIF statement to cause the select query to not return any records
        Dim getAllTicketAndChildrenIdsSql As String = "SELECT id FROM orders WHERE (orders.deleted = 0) AND (orders.id=" & Q(orderId) & ") " & _
            "UNION " & _
            "SELECT order_customer_accounts.customer_account_id FROM order_customer_accounts INNER JOIN orders ON orders.id = order_customer_accounts.order_id WHERE (order_customer_accounts.deleted = 0) AND (orders.deleted = 0) AND (orders.id=" & Q(orderId) & ") " & _
            "UNION " & _
            "SELECT order_items.product_id FROM order_items INNER JOIN orders ON orders.id = order_items.order_id WHERE (order_items.deleted = 0) AND (orders.deleted = 0) AND (orders.id=" & Q(orderId) & ")"

        Dim getCustomFieldsDA As New OleDb.OleDbDataAdapter("SELECT custom_fields.id, custom_fields.field_name as field_name, LOWER(custom_fields.table_name) AS source_table, custom_field_data.value " & _
                                                            "FROM custom_fields " & _
                                                            "INNER JOIN custom_field_data ON custom_fields.id = custom_field_data.custom_field_id " & _
                                                            "WHERE (custom_field_data.record_id IN (" & getAllTicketAndChildrenIdsSql & ") ) AND (custom_fields.deleted = 0) AND  (custom_field_data.deleted = 0) " & _
                                                            IIf(showAllCustomFieldsOnDeliveryTicket, "", IIf(customFieldsShown.Length > 0, "AND (custom_field_data.custom_field_id IN (" & customFieldsShown & ")) ", "AND (custom_fields.deleted = 1)")) & _
                                                            "ORDER BY custom_fields.table_name, custom_fields.field_name", Tm2Database.Connection)
        If Tm2Database.CommandTimeout > 0 Then getCustomFieldsDA.SelectCommand.CommandTimeout = Tm2Database.CommandTimeout
        getCustomFieldsDA.Fill(_orderSummaryCustomFieldsTable)
    End Sub

    Private Function GetSettingByOwnerId(ByVal ownerId As Guid, ByVal settingName As String, ByVal defaultValue As Object) As Object
        'Find the owner specific setting.
        Dim allSettings As ArrayList = KaSetting.GetAll(Tm2Database.Connection, "name = " & Q("OrderSummarySetting:" & ownerId.ToString & "/" & settingName) & " and deleted = 0", "")
        If allSettings.Count = 1 Then
            Return allSettings.Item(0).value
        End If

        'If there isn't an owner specific setting, get the All Owners setting, if that doesn't exist either, use the default value.
        Dim retval As String = KaSetting.GetSetting(Tm2Database.Connection, "OrderSummarySetting:" & Guid.Empty.ToString & "/" & settingName, defaultValue.ToString)
        Return retval
    End Function

    Private Function EncodeAsHtml(ByVal text As String) As String
        Return Server.HtmlEncode(text).Replace(vbCrLf, "<br />").Replace(vbCr, "<br />").Replace(vbLf, "<br />")
    End Function

    Private Function GetCustomerNumber(ByVal orderCustomerAccount As KaOrderCustomerAccount) As String
        Dim retval As String = ""
        If orderCustomerAccount.AccountInterfaceSettingId <> Guid.Empty Then
            Dim cais As KaCustomerAccountInterfaceSettings = New KaCustomerAccountInterfaceSettings(Tm2Database.Connection, orderCustomerAccount.AccountInterfaceSettingId)
            retval = cais.CrossReference
        Else
            Dim account As KaCustomerAccount = New KaCustomerAccount(Tm2Database.Connection, orderCustomerAccount.CustomerAccountId)
            retval = account.AccountNumber
        End If
        Return retval
    End Function
#End Region
End Class