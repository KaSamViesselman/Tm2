Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class InterfaceAssignOrder
    Inherits System.Web.UI.Page

    Private _currentUser As KaUser
Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
    Private _currentTableName As String = KaInterface.TABLE_NAME
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Response.Cache.SetCacheability(HttpCacheability.NoCache)
        _currentUser = Utilities.GetUser(Me)
        _currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Interfaces")
        If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")
        lblStatus.Text = ""
        btnAssignInterfaceToOrder.Enabled = _currentUserPermission(_currentTableName).Edit
        If Not Page.IsPostBack Then
            Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
            PopulateFacilityList()
            If Page.Request("OrderId") Is Nothing Then
                Try
                    ddlFacilityFilter.SelectedValue = KaSetting.GetSetting(connection, "OrdersPage:" & _currentUser.Id.ToString & "/LastFacilityUsed", Guid.Empty.ToString())
                    ddlFacilityFilter_SelectedIndexChanged(Nothing, Nothing)
                Catch ex As ArgumentOutOfRangeException

                End Try
            End If
            PopulateOrdersList()
            PopulateInterfaceList()

            Dim orderId As Guid = Guid.Empty
            If Guid.TryParse(Page.Request("OrderId"), orderId) Then
                Try
                    ddlOrders.SelectedValue = orderId.ToString()
                Catch ex As ArgumentOutOfRangeException

                End Try
                ddlOrders_SelectedIndexChanged(ddlOrders, New EventArgs())
            End If
        End If
    End Sub

    Private Sub PopulateOrdersList()
        Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
        Dim showReleaseNumber As Boolean = Boolean.Parse(KaSetting.GetSetting(connection, "General/ShowReleaseNumberInOrderList", "False"))
        ddlOrders.Items.Clear()
        ddlOrders.Items.Add(New ListItem("Enter a new order", Guid.Empty.ToString()))
        Dim getOrdersRdr As OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), "SELECT id, number, release_number FROM orders " & _
                "WHERE completed = 0 AND deleted = 0 AND archived = 0 " & _
                IIf(_currentUser.OwnerId.Equals(Guid.Empty), "", "AND ((owner_id = " & Q(_currentUser.OwnerId) & ") OR (owner_id = " & Q(Guid.Empty) & ")) ") & _
                IIf(ddlFacilityFilter.SelectedValue = Guid.Empty.ToString(), "", "AND id IN (SELECT order_id FROM order_items WHERE (deleted = 0) AND product_id in (select product_id from product_bulk_products where location_id = " & Q(ddlFacilityFilter.SelectedValue) & " and deleted = 0))") & _
                " ORDER BY number ASC")
        Do While getOrdersRdr.Read
            Dim displayText As String = getOrdersRdr.Item("number")
            If getOrdersRdr.Item("release_number") <> "" Then
                displayText &= " (" & getOrdersRdr.Item("release_number") & ")"
            End If
            ddlOrders.Items.Add(New ListItem(displayText, getOrdersRdr.Item("id").ToString()))
        Loop
        getOrdersRdr.Close()
        Page.ClientScript.RegisterStartupScript(Page.ClientScript.GetType(), "ResetScrollPosition;", "resetDotNetScrollPosition();", True)
    End Sub

    Private Sub PopulateFacilityList()
        ddlFacilityFilter.Items.Clear()
        ddlFacilityFilter.Items.Add(New ListItem("All facilities", Guid.Empty.ToString()))
        For Each u As KaLocation In KaLocation.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
            ddlFacilityFilter.Items.Add(New ListItem(u.Name, u.Id.ToString))
        Next
    End Sub

    Private Sub PopulateInterfaceList()
        ddlInterfaces.Items.Clear()
        ddlInterfaces.Items.Add(New ListItem("Interface not assigned", Guid.Empty.ToString))
        For Each i As KaInterface In KaInterface.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
            ddlInterfaces.Items.Add(New ListItem(i.Name, i.Id.ToString))
        Next
    End Sub

    Protected Sub ddlFacilityFilter_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlFacilityFilter.SelectedIndexChanged
        Dim currentOrderId As String = ddlOrders.SelectedValue
        PopulateOrdersList()
        Try
            ddlOrders.SelectedValue = currentOrderId
        Catch ex As ArgumentOutOfRangeException

        End Try
        ddlOrders_SelectedIndexChanged(ddlOrders, New EventArgs)
        KaSetting.WriteSetting(GetUserConnection(_currentUser.Id), "OrdersPage:" & _currentUser.Id.ToString & "/LastFacilityUsed", ddlFacilityFilter.SelectedValue)
    End Sub

    Private Sub ddlOrders_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles ddlOrders.SelectedIndexChanged
        Try
            Dim orderInfo As New KaOrder(GetUserConnection(_currentUser.Id), Guid.Parse(ddlOrders.SelectedValue))
            Try
                ddlInterfaces.SelectedValue = orderInfo.InterfaceId.ToString()
            Catch ex As ArgumentOutOfRangeException
                ddlInterfaces.SelectedIndex = 0
            End Try
            btnAssignInterfaceToOrder.Enabled = _currentUserPermission(_currentTableName).Edit
            ddlInterfaces.Enabled = _currentUserPermission(_currentTableName).Edit
        Catch ex As Exception
            btnAssignInterfaceToOrder.Enabled = False
            ddlInterfaces.Enabled = False
        End Try
    End Sub

    Private Sub btnAssignInterfaceToOrder_Click(sender As Object, e As System.EventArgs) Handles btnAssignInterfaceToOrder.Click
        Try
            Dim orderInfo As New KaOrder(GetUserConnection(_currentUser.Id), Guid.Parse(ddlOrders.SelectedValue))
            If Guid.TryParse(ddlInterfaces.SelectedValue, orderInfo.InterfaceId) Then
                orderInfo.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                lblStatus.Text = "Order updated successfully"
            End If
        Catch ex As Exception
            lblStatus.Text = "There was an error updating the order: " & ex.Message
        End Try
    End Sub

    Private Sub btnFind_Click(sender As Object, e As System.EventArgs) Handles btnFind.Click
        Dim orders As New List(Of Guid) ' get a list of the order IDs
        If tbxFind.Text.Trim.Length > 0 Then orders = KaOrder.GetOrderIdsWithKeyword(GetUserConnection(_currentUser.Id), Nothing, _currentUser.OwnerId, tbxFind.Text.Trim, False)

        Dim found As Boolean = False
        Dim i As Integer = ddlOrders.SelectedIndex ' begin with the next order in the drop-down list
        Do
            If i + 1 = ddlOrders.Items.Count Then i = 0 Else i += 1 ' wrap around to the beginning of the drop-down list
            If orders.IndexOf(Guid.Parse(ddlOrders.Items(i).Value)) <> -1 Then ' this is one of the orders that was found, select it
                ddlOrders.SelectedIndex = i
                found = True
                Exit Do ' no need to look any further
            End If
        Loop While i <> ddlOrders.SelectedIndex ' continue until we've come back to where we started
        If found Then
            ddlOrders_SelectedIndexChanged(sender, e)
        Else
            ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidKeyword", Utilities.JsAlert("Order not found containing keywords: " & tbxFind.Text))
        End If
    End Sub
End Class