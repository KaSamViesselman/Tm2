Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class ReceivingPoSettings : Inherits System.Web.UI.Page
    Private _currentUser As KaUser
    Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
    Private _currentTableName As String = KaSetting.TABLE_NAME
    Public Const SN_EMAIL_RECEIVING_POINT_OF_SALE_TICKETS As String = "ReceivingPointOfSale/EmailCreatedPointOfSaleTickets"
    Public Const SD_EMAIL_RECEIVING_POINT_OF_SALE_TICKETS As String = "True"

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Response.Cache.SetCacheability(HttpCacheability.NoCache)
        _currentUser = Utilities.GetUser(Me)
        _currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "GeneralSettings")
        If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")
        btnSave.Enabled = _currentUserPermission(_currentTableName).Edit
        If Not Page.IsPostBack Then
            DisplaySettings()
        End If
    End Sub

    Private Sub DisplaySettings()
        Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
        cbxAutoGenerateReceivingOrderNumbers.Checked = Boolean.Parse(KaSetting.GetSetting(connection, KaSetting.SN_AUTO_GENERATE_RECEIVING_ORDER_NUMBER, KaSetting.SD_AUTO_GENERATE_RECEIVING_ORDER_NUMBER))
        cbxAllowModificationReceivingOrderNumber.Checked = Boolean.Parse(KaSetting.GetSetting(connection, KaSetting.SN_USER_CAN_CHANGE_RECEIVING_ORDER_NUMBER, KaSetting.SD_USER_CAN_CHANGE_RECEIVING_ORDER_NUMBER))
        cbxSeparateReceivingOrderNumberPerOwner.Checked = Boolean.Parse(KaSetting.GetSetting(connection, KaSetting.SN_SEPARATE_RECEIVING_ORDER_NUMBER_PER_OWNER, KaSetting.SD_SEPARATE_RECEIVING_ORDER_NUMBER_PER_OWNER))
        tbxStartingReceivingOrderNumber.Enabled = Not cbxSeparateReceivingOrderNumberPerOwner.Checked
        tbxStartingReceivingOrderNumber.Text = KaSetting.GetSetting(connection, KaSetting.SN_NEXT_RECEIVING_ORDER_NUMBER, KaSetting.SD_NEXT_RECEIVING_ORDER_NUMBER)
        Boolean.TryParse(KaSetting.GetSetting(connection, SN_EMAIL_RECEIVING_POINT_OF_SALE_TICKETS, SD_EMAIL_RECEIVING_POINT_OF_SALE_TICKETS), cbxEmailCreatedPointOfSaleTickets.Checked)
        Boolean.TryParse(KaSetting.GetSetting(connection, KaSetting.SN_EMAIL_RECEIVING_TICKET_OWNER, KaSetting.SD_EMAIL_RECEIVING_TICKET_OWNER), cbxEmailReceivingTicketOwner.Checked)
        Boolean.TryParse(KaSetting.GetSetting(connection, KaSetting.SN_EMAIL_RECEIVING_TICKET_SUPPLIER, KaSetting.SD_EMAIL_RECEIVING_TICKET_SUPPLIER), cbxEmailReceivingTicketSupplierAccount.Checked)
        Boolean.TryParse(KaSetting.GetSetting(connection, KaSetting.SN_EMAIL_RECEIVING_TICKET_CARRIER, KaSetting.SD_EMAIL_RECEIVING_TICKET_CARRIER), cbxEmailReceivingTicketCarrier.Checked)
        Boolean.TryParse(KaSetting.GetSetting(connection, KaSetting.SN_EMAIL_RECEIVING_TICKET_DRIVER, KaSetting.SD_EMAIL_RECEIVING_TICKET_DRIVER), cbxEmailReceivingTicketDriver.Checked)
        Boolean.TryParse(KaSetting.GetSetting(connection, KaSetting.SN_EMAIL_RECEIVING_TICKET_LOCATION, KaSetting.SD_EMAIL_RECEIVING_TICKET_LOCATION), cbxEmailReceivingTicketLocation.Checked)
    End Sub

    Private Sub btnSave_Click(sender As Object, e As System.EventArgs) Handles btnSave.Click
        Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
        KaSetting.WriteSetting(connection, KaSetting.SN_AUTO_GENERATE_RECEIVING_ORDER_NUMBER, cbxAutoGenerateReceivingOrderNumbers.Checked)
        KaSetting.WriteSetting(connection, KaSetting.SN_USER_CAN_CHANGE_RECEIVING_ORDER_NUMBER, cbxAllowModificationReceivingOrderNumber.Checked)
        KaSetting.WriteSetting(connection, KaSetting.SN_SEPARATE_RECEIVING_ORDER_NUMBER_PER_OWNER, cbxSeparateReceivingOrderNumberPerOwner.Checked)
        KaSetting.WriteSetting(connection, KaSetting.SN_NEXT_RECEIVING_ORDER_NUMBER, tbxStartingReceivingOrderNumber.Text)
        KaSetting.WriteSetting(connection, SN_EMAIL_RECEIVING_POINT_OF_SALE_TICKETS, cbxEmailCreatedPointOfSaleTickets.Checked)
        KaSetting.WriteSetting(connection, KaSetting.SN_EMAIL_RECEIVING_TICKET_OWNER, cbxEmailReceivingTicketOwner.Checked)
        KaSetting.WriteSetting(connection, KaSetting.SN_EMAIL_RECEIVING_TICKET_SUPPLIER, cbxEmailReceivingTicketSupplierAccount.Checked)
        KaSetting.WriteSetting(connection, KaSetting.SN_EMAIL_RECEIVING_TICKET_CARRIER, cbxEmailReceivingTicketCarrier.Checked)
        KaSetting.WriteSetting(connection, KaSetting.SN_EMAIL_RECEIVING_TICKET_DRIVER, cbxEmailReceivingTicketDriver.Checked)
        KaSetting.WriteSetting(connection, KaSetting.SN_EMAIL_RECEIVING_TICKET_LOCATION, cbxEmailReceivingTicketLocation.Checked)
        lblSave.Visible = True
    End Sub
End Class