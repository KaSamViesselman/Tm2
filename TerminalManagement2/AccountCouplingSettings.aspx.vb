Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class AccountCouplingSettings : Inherits System.Web.UI.Page

    Private _currentUser As KaUser
Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
    Private _currentTableName As String = KaSetting.TABLE_NAME

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
        Dim c As OleDbConnection = GetUserConnection(_currentUser.Id)
        cbxCoupledAccounts.Checked = KaSetting.GetSetting(c, "General/UseCoupledAccounts", "False")
        cbxShowCoupledBoxOnOrders.Checked = KaSetting.GetSetting(c, "General/DisplayCoupledAccountsCbx", "False")
    End Sub

    Private Sub btnSave_Click(sender As Object, e As System.EventArgs) Handles btnSave.Click
        Dim c As OleDbConnection = GetUserConnection(_currentUser.Id)
        KaSetting.WriteSetting(c, "General/UseCoupledAccounts", cbxCoupledAccounts.Checked)
        KaSetting.WriteSetting(c, "General/DisplayCoupledAccountsCbx", cbxShowCoupledBoxOnOrders.Checked)

        lblSave.Visible = True
    End Sub
End Class