Imports KahlerAutomation.KaTm2Database
Imports System.Environment
Imports System.Data.OleDb

Public Class TankGroups : Inherits System.Web.UI.Page

    Private _currentUser As KaUser
    Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing

#Region "Events"
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Response.Cache.SetCacheability(HttpCacheability.NoCache)
        _currentUser = Utilities.GetUser(Me)
        _currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaTank.TABLE_NAME}), "Tanks")
        If Not _currentUserPermission(KaTank.TABLE_NAME).Read Then Response.Redirect("Welcome.aspx")
        If Not Page.IsPostBack Then
            SetTextboxMaxLengths()
            PopulateTankGroupList()
            PopulateTankList()
            If Page.Request("TankGroupId") IsNot Nothing Then
                Try
                    ddlTankGroups.SelectedValue = Page.Request("TankGroupId")
                Catch ex As ArgumentOutOfRangeException
                    ddlTankGroups.SelectedIndex = 0
                End Try
            End If
            ddlTankGroups_SelectedIndexChanged(ddlTankGroups, New EventArgs())

            Utilities.ConfirmBox(Me.btnDelete, "Are you sure you want to delete this tank group?")
            Utilities.SetFocus(tbxName, Me)
        End If
    End Sub

    Protected Sub ddlTankGroups_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles ddlTankGroups.SelectedIndexChanged
        ddlTank.SelectedIndex = 0
        PopulateTankGroupInformation(Guid.Parse(ddlTankGroups.SelectedValue))
        SetControlUsabilityFromPermissions()
    End Sub

    Protected Sub ddlTank_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles ddlTank.SelectedIndexChanged
        Dim tankId As Guid = Guid.Parse(ddlTank.SelectedValue)
        btnAddTank.Enabled = tankId <> Guid.Empty AndAlso Not TankInList(tankId)
    End Sub

    Protected Sub lstTanks_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles lstTanks.SelectedIndexChanged
        btnRemoveTank.Enabled = lstTanks.SelectedItem IsNot Nothing
    End Sub

    Protected Sub btnRemoveTank_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnRemoveTank.Click
        lstTanks.Items.RemoveAt(lstTanks.SelectedIndex)
        Dim tankId As Guid = Guid.Parse(ddlTank.SelectedValue)
        btnAddTank.Enabled = tankId <> Guid.Empty AndAlso Not TankInList(tankId)
        btnDelete.Enabled = lstTanks.SelectedItem IsNot Nothing
    End Sub

    Protected Sub btnAddTank_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnAddTank.Click
        lstTanks.Items.Add(New ListItem(ddlTank.SelectedItem.Text, ddlTank.SelectedItem.Value))
        btnAddTank.Enabled = False
        btnRemoveTank.Enabled = lstTanks.SelectedItem IsNot Nothing
    End Sub

    Protected Sub btnDelete_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnDelete.Click
        lblStatus.Text = ""
        With New KaTankGroup(GetUserConnection(_currentUser.Id), Guid.Parse(ddlTankGroups.SelectedValue))
            .Deleted = True
            .SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
            For Each r As KaTankGroupTank In KaTankGroupTank.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND tank_group_id=" & Q(.Id), "")
                r.Deleted = True
                r.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
            Next
            lblStatus.Text = "Tank group successfully deleted."
            PopulateTankGroupList()
            tbxName.Text = ""
            PopulateTankGroupTankList(New Guid)
            ddlTankGroups.SelectedValue = Guid.Empty.ToString()
        End With
    End Sub

    Protected Sub btnSave_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnSave.Click
        lblStatus.Text = ""
        If tbxName.Text.Length = 0 Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidName", Utilities.JsAlert("Name must be specified.")) : Exit Sub
        With New KaTankGroup()
            .Id = Guid.Parse(ddlTankGroups.SelectedValue)
            .Name = tbxName.Text
            If .Id = Guid.Empty Then
                .SqlInsert(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                lblStatus.Text = "Tank group successfully added."
                PopulateTankGroupList()
                ddlTankGroups.SelectedValue = .Id.ToString()
            Else
                .SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                lblStatus.Text = "Tank group successfully updated."
            End If
            SaveTankGroupTanks(.Id)
        End With
        SetControlUsabilityFromPermissions()
    End Sub
#End Region

    Private Function TankInList(ByVal tankId As Guid) As Boolean
        For Each i As ListItem In lstTanks.Items
            If Guid.Parse(i.Value) = tankId Then Return True
        Next
        Return False
    End Function

    Private Sub SaveTankGroupTanks(ByVal tankGroupId As Guid)
        Dim l As ArrayList = KaTankGroupTank.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND tank_group_id=" & Q(tankGroupId), "")
        For Each i As ListItem In lstTanks.Items
            Dim j As Integer = 0
            Do While j < l.Count
                If CType(l(j), KaTankGroupTank).TankId = Guid.Parse(i.Value) Then Exit Do Else j += 1
            Loop
            Dim r As KaTankGroupTank
            If j < l.Count Then ' record found
                r = l(j)
                l.RemoveAt(j) ' remove the record so that it isn't deleted later
            Else ' record not found
                r = New KaTankGroupTank()
                r.TankGroupId = tankGroupId
                r.TankId = Guid.Parse(i.Value)
            End If
            If r.Id = Guid.Empty Then r.SqlInsert(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name) Else r.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
        Next
        For Each i As KaTankGroupTank In l ' delete the remaining records (not found in the current list of tanks, must have been deleted)
            i.Deleted = True
            i.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
        Next
    End Sub

    Private Function TankGroupOwnerOk(ByVal tankGroup As KaTankGroup, ByVal ownerId As Guid) As Boolean
        For Each r As KaTankGroupTank In KaTankGroupTank.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND tank_group_id=" & Q(tankGroup.Id), "")
            Try
                If New KaTank(GetUserConnection(_currentUser.Id), r.TankId).OwnerId <> ownerId Then Return False
            Catch ex As RecordNotFoundException
                ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidTankId", Utilities.JsAlert("Record not found in tanks where ID = " & r.TankId.ToString() & " while checking tank group ownership."))
            End Try
        Next
        Return True
    End Function

    Private Sub PopulateTankGroupList()
        ddlTankGroups.Items.Clear()
        If _currentUserPermission(KaTank.TABLE_NAME).Create Then
            ddlTankGroups.Items.Add(New ListItem("Enter a new tank group", Guid.Empty.ToString()))
        Else
            ddlTankGroups.Items.Add(New ListItem("Select a tank group", Guid.Empty.ToString()))
        End If
        For Each r As KaTankGroup In KaTankGroup.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
            If _currentUser.OwnerId = Guid.Empty OrElse TankGroupOwnerOk(r, _currentUser.OwnerId) Then ddlTankGroups.Items.Add(New ListItem(r.Name, r.Id.ToString()))
        Next
    End Sub

    Private Sub PopulateTankList()
        ddlTank.Items.Clear()
        ddlTank.Items.Add(New ListItem("", Guid.Empty.ToString()))
        For Each r As KaTank In KaTank.GetAll(GetUserConnection(_currentUser.Id), "deleted=0" & IIf(_currentUser.OwnerId = Guid.Empty, "", " AND owner_id=" & Q(_currentUser.OwnerId)), "name ASC")
            ddlTank.Items.Add(New ListItem(r.Name, r.Id.ToString()))
        Next
    End Sub

    Private Sub PopulateTankGroupTankList(ByVal tankGroupId As Guid)
        lstTanks.Items.Clear()
        For Each r As KaTankGroupTank In KaTankGroupTank.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND tank_group_id=" & Q(tankGroupId), "")
            Try
                lstTanks.Items.Add(New ListItem(New KaTank(GetUserConnection(_currentUser.Id), r.TankId).Name, r.TankId.ToString()))
            Catch ex As RecordNotFoundException
                ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidTankId", Utilities.JsAlert("Record not found in tanks where ID = " & r.TankId.ToString() & " while populating tank list."))
            End Try
        Next
        btnRemoveTank.Enabled = False
        Dim tankId As Guid = Guid.Parse(ddlTank.SelectedValue)
        btnAddTank.Enabled = tankId <> Guid.Empty AndAlso Not TankInList(tankId)
    End Sub

    Private Sub PopulateTankGroupInformation(ByVal tankGroupId As Guid)
        With New KaTankGroup()
            .Id = tankGroupId
            If .Id <> Guid.Empty Then .SqlSelect(GetUserConnection(_currentUser.Id))
            tbxName.Text = .Name
            PopulateTankGroupTankList(.Id)
        End With
    End Sub

    Private Sub SetTextboxMaxLengths()
        tbxName.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaTankGroup.TABLE_NAME, "name"))
    End Sub
    Private Sub SetControlUsabilityFromPermissions()
        With _currentUserPermission(KaTank.TABLE_NAME)
            Dim shouldEnable = (.Edit AndAlso ddlTankGroups.SelectedIndex > 0) OrElse (.Create AndAlso ddlTankGroups.SelectedIndex = 0)
            pnlEven.Enabled = shouldEnable
            pnlOdd.Enabled = shouldEnable
            btnSave.Enabled = shouldEnable
            Dim value = Guid.Parse(ddlTankGroups.SelectedValue)
            btnDelete.Enabled = .Edit AndAlso .Delete AndAlso value <> Guid.Empty
        End With
    End Sub
End Class