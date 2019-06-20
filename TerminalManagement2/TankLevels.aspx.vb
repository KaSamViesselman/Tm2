Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb
Imports System.IO

Public Class TankLevels : Inherits System.Web.UI.Page

    Private _currentUser As KaUser
    Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing

#Region "Events"
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Response.Cache.SetCacheability(HttpCacheability.NoCache)
        _currentUser = Utilities.GetUser(Me)
        _currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaTank.TABLE_NAME}), "Tanks")
        If Not _currentUserPermission(KaTank.TABLE_NAME).Read Then Response.Redirect("Welcome.aspx")
        If Not Page.IsPostBack Then
            PopulateDisplayUnitList()
            PopulateBulkProductList()
            PopulateLocationList()
            PopulateOwnerList()
            PopulateTankList()
            PopulateTankGroupList()
            PopulateInitialOptions()
        End If
    End Sub
#End Region

    Private Sub PopulateLocationList()
        ddlLocation.Items.Clear()
        ddlLocation.Items.Add(New ListItem("All locations", Guid.Empty.ToString()))
        For Each r As KaLocation In KaLocation.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
            ddlLocation.Items.Add(New ListItem(r.Name, r.Id.ToString()))
        Next
        ddlLocation.SelectedIndex = 0
    End Sub

    Private Sub PopulateOwnerList()
        ddlOwner.Items.Clear()
        If _currentUser.OwnerId = Guid.Empty Then ddlOwner.Items.Add(New ListItem("All owners", Guid.Empty.ToString()))
        For Each r As KaOwner In KaOwner.GetAll(GetUserConnection(_currentUser.Id), "deleted=0" & IIf(_currentUser.OwnerId = Guid.Empty, "", " AND id=" & Q(_currentUser.OwnerId)), "name ASC")
            ddlOwner.Items.Add(New ListItem(r.Name, r.Id.ToString()))
        Next
        ddlOwner.SelectedIndex = 0
    End Sub

    Private Sub PopulateBulkProductList()
        ddlBulkProduct.Items.Clear()
        ddlBulkProduct.Items.Add(New ListItem("All bulk products", Guid.Empty.ToString()))
        For Each r As KaBulkProduct In KaBulkProduct.GetAll(GetUserConnection(_currentUser.Id), "deleted=0" & IIf(_currentUser.OwnerId = Guid.Empty, "", " AND owner_id=" & Q(_currentUser.OwnerId)), "name ASC")
            ddlBulkProduct.Items.Add(New ListItem(r.Name, r.Id.ToString()))
        Next
        ddlBulkProduct.SelectedIndex = 0
    End Sub

    Private Sub PopulateTankList()
        Dim ownerId As Guid = Guid.Parse(ddlOwner.SelectedValue)
        Dim locationId As Guid = Guid.Parse(ddlLocation.SelectedValue)
        Dim bulkProductId As Guid = Guid.Parse(ddlBulkProduct.SelectedValue)
        Dim writer As New StringWriter()
        Dim htmlWriter As New HtmlTextWriter(writer)
        KaReports.GetTankLevelReport(GetUserConnection(_currentUser.Id), ownerId, locationId, bulkProductId, True, Guid.Parse(ddlDisplayUnit.SelectedValue)).RenderControl(htmlWriter)
        litTankList.Text = writer.ToString()
    End Sub

    Private Sub PopulateTankGroupList()
        Dim ownerId As Guid = Guid.Parse(ddlOwner.SelectedValue)
        Dim locationId As Guid = Guid.Parse(ddlOwner.SelectedValue)
        Dim bulkProductId As Guid = Guid.Parse(ddlBulkProduct.SelectedValue)
        Dim writer As New StringWriter()
        Dim htmlWriter As New HtmlTextWriter(writer)
        KaReports.GetTankGroupLevelReport(GetUserConnection(_currentUser.Id), ownerId, locationId, bulkProductId, True).RenderControl(htmlWriter)
        litTankList.Text &= "<hr style=""width: 100%; color: #99ccff;"" />" & writer.ToString()
    End Sub

    Private Sub PopulateDisplayUnitList()
        ddlDisplayUnit.Items.Clear()
        ddlDisplayUnit.Items.Add(New ListItem("Tank's unit", Guid.Empty.ToString))
        Dim conditions As String = "deleted=0 AND base_unit!=2 AND base_unit!=9" ' only show mass and volume units (exclude: 2 = pulses and 9 = seconds)
        For Each unit As KaUnit In KaUnit.GetAll(GetUserConnection(_currentUser.Id), conditions, "name ASC")
            ddlDisplayUnit.Items.Add(New ListItem(unit.Name, unit.Id.ToString))
        Next
    End Sub

    Private Sub PopulateInitialOptions()
        Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
        Try
            ddlBulkProduct.SelectedValue = KaSetting.GetSetting(connection, "TankLevels:" & _currentUser.Id.ToString & "/LastUsedBulkProduct", Guid.Empty.ToString())
        Catch ex As Exception
            ddlBulkProduct.SelectedIndex = 0
        End Try

        Try
            ddlDisplayUnit.SelectedValue = KaSetting.GetSetting(connection, "TankLevels:" & _currentUser.Id.ToString & "/LastUsedDisplayUnit", KaUnit.GetSystemDefaultVolumeUnitOfMeasure(connection, Nothing).ToString())
        Catch ex As Exception
            ' Unit no longer found, try the default unit
            Try
                ddlDisplayUnit.SelectedValue = KaUnit.GetSystemDefaultVolumeUnitOfMeasure(connection, Nothing).ToString()
            Catch ex2 As Exception
                ddlDisplayUnit.SelectedIndex = 0
            End Try
        End Try

        Try
            ddlLocation.SelectedValue = KaSetting.GetSetting(connection, "TankLevels:" & _currentUser.Id.ToString & "/LastUsedLocation", Guid.Empty.ToString())
        Catch ex As Exception
            ddlLocation.SelectedIndex = 0
        End Try

        Try
            ddlOwner.SelectedValue = KaSetting.GetSetting(connection, "TankLevels:" & _currentUser.Id.ToString & "/LastUsedOwner", Guid.Empty.ToString())
        Catch ex As Exception
            ddlOwner.SelectedIndex = 0
        End Try
        btnShowReport_Click(btnShowReport, New EventArgs())
    End Sub

    Protected Sub btnShowReport_Click(sender As Object, e As EventArgs) Handles btnShowReport.Click
        PopulateTankList()
        PopulateTankGroupList()
        SaveInitialOptions()
    End Sub

    Private Sub SaveInitialOptions()
        Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)

        KaSetting.WriteSetting(connection, "TankLevels:" & _currentUser.Id.ToString & "/LastUsedLocation", ddlLocation.SelectedValue)
        KaSetting.WriteSetting(connection, "TankLevels:" & _currentUser.Id.ToString & "/LastUsedOwner", ddlOwner.SelectedValue)
        KaSetting.WriteSetting(connection, "TankLevels:" & _currentUser.Id.ToString & "/LastUsedBulkProduct", ddlBulkProduct.SelectedValue)
        KaSetting.WriteSetting(connection, "TankLevels:" & _currentUser.Id.ToString & "/LastUsedDisplayUnit", ddlDisplayUnit.SelectedValue)
    End Sub
End Class