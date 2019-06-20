Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class InProgressRecords
    Inherits System.Web.UI.Page
    Private _currentUser As KaUser
    Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
    Private _currentTableName As String = KaOrder.TABLE_NAME

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Response.Cache.SetCacheability(HttpCacheability.NoCache)
        _currentUser = Utilities.GetUser(Me)
        Dim purchaseOrderPage As Boolean = False
        If Request.QueryString("menu_tab") IsNot Nothing Then
            If Request.QueryString("menu_tab").ToLower() = "receiving" Then
                Me.Title = "Purchase Orders : In Progress"
                purchaseOrderPage = True
            End If
        End If
        _currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Orders")
        If Not (_currentUserPermission(_currentTableName).Read OrElse Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaReceivingPurchaseOrder.TABLE_NAME}), "PurchaseOrders")(KaReceivingPurchaseOrder.TABLE_NAME).Read) Then Response.Redirect("Welcome.aspx")

        Dim inProgressText As String = KaReports.GetOpenInProgressReport(GetUserConnection(_currentUser.Id), Guid.Empty, cbxShowIndividualWeighments.Checked, True, IIf(purchaseOrderPage, KaReports.InProgressFilter.OnlyPurchaseOrders, KaReports.InProgressFilter.AllRecordsExceptPurchaseOrders))
        If inProgressText.Length > 0 Then
            litInProgressData.Text = inProgressText
        Else
            litInProgressData.Text = "No current In Progress records"
        End If
    End Sub
End Class