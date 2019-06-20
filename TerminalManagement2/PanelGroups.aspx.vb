Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class PanelGroups : Inherits System.Web.UI.Page
    Private _currentUser As KaUser
Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
    Private _currentTableName As String = KaPanel.TABLE_NAME

#Region "Events"
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Response.Cache.SetCacheability(HttpCacheability.NoCache)
        _currentUser = Utilities.GetUser(Me)
        _currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Panels")
        If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")
        If Not Page.IsPostBack Then
            Utilities.ConfirmBox(btnDelete, "Are you sure that you want to delete this panel group?")
            SetTextboxMaxLengths()
            PopulateRecords()
            SetControlUsabilityFromPermissions()
            If Page.Request("PanelGroupId") IsNot Nothing Then
                Try
                    ddlRecords.SelectedValue = Page.Request("PanelGroupId")
                Catch ex As ArgumentOutOfRangeException
                    ddlRecords.SelectedIndex = 0
                End Try
            End If
            ddlRecords_SelectedIndexChanged(ddlRecords, New EventArgs())
        End If
    End Sub

    Protected Sub ddlRecords_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlRecords.SelectedIndexChanged
        Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
        Dim record As KaPanelGroup
        If ddlRecords.SelectedIndex > 0 Then
            record = New KaPanelGroup(connection, Guid.Parse(ddlRecords.SelectedValue))
        Else
            record = New KaPanelGroup()
        End If
        ' update the group section
        tbxName.Text = record.Name
        cbxCannotFillSimultaneously.Checked = record.CannotFillSimultaneously
        lblMessage.Text = ""
        UpdateMemberLists()
        UpdateButtons()
        SetControlUsabilityFromPermissions()
    End Sub

    Protected Sub ddlPanel_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlPanel.SelectedIndexChanged
        UpdateButtons()
    End Sub

    Protected Sub lstPanels_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lstPanels.SelectedIndexChanged
        UpdateButtons()
    End Sub

    Protected Sub btnAddPanel_Click(sender As Object, e As EventArgs) Handles btnAddPanel.Click
        Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
        Dim member As New KaPanelGroupPanel()
        member.PanelGroupId = Guid.Parse(ddlRecords.SelectedValue)
        member.PanelId = Guid.Parse(ddlPanel.SelectedValue)
        member.SqlInsert(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
        UpdateMemberLists()
        UpdateButtons()
    End Sub

    Protected Sub btnRemovePanel_Click(sender As Object, e As EventArgs) Handles btnRemovePanel.Click
        Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
        Dim conditions As String = String.Format("deleted=0 AND panel_group_id={0} AND panel_id={1}", Q(ddlRecords.SelectedValue), Q(lstPanels.SelectedValue))
        For Each member As KaPanelGroupPanel In KaPanelGroupPanel.GetAll(connection, conditions, "last_updated DESC")
            member.Deleted = True
            member.SqlUpdate(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
        Next
        UpdateMemberLists()
        UpdateButtons()
    End Sub

    Protected Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
        If tbxName.Text.Trim().Length = 0 Then
            ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), "InvalidName", Utilities.JsAlert("Please specify a name for the panel group."), False)
            Exit Sub
        End If
        If KaPanelGroup.GetAll(connection, String.Format("deleted=0 AND id<>{0} AND name={1}", Q(ddlRecords.SelectedValue), Q(tbxName.Text)), "name ASC").Count > 0 Then
            ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), "InvalidNameExists", Utilities.JsAlert("A panel group with the name """ & tbxName.Text & """ already exists. Please specify a unique name for the panel group."), False)
            Exit Sub
        End If
        Dim record As KaPanelGroup
        If ddlRecords.SelectedIndex > 0 Then
            record = New KaPanelGroup(connection, Guid.Parse(ddlRecords.SelectedValue))
        Else
            record = New KaPanelGroup()
        End If
        record.Name = tbxName.Text
        record.CannotFillSimultaneously = cbxCannotFillSimultaneously.Checked
        If record.Id = Guid.Empty Then
            record.SqlInsert(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
            lblMessage.Text = String.Format("{0} added", record.Name)
        Else
            record.SqlUpdate(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
            lblMessage.Text = String.Format("{0} updated", record.Name)
        End If
        PopulateRecords()
        Dim id As String = record.Id.ToString()
        For i As Integer = 0 To ddlRecords.Items.Count - 1
            If ddlRecords.Items(i).Value = id Then
                ddlRecords.SelectedIndex = i
                Exit For
            End If
        Next
        UpdateButtons()
    End Sub

    Protected Sub btnDelete_Click(sender As Object, e As EventArgs) Handles btnDelete.Click
        Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
        Dim record As New KaPanelGroup(connection, Guid.Parse(ddlRecords.SelectedValue))
        record.Deleted = True
        record.SqlUpdate(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
        PopulateRecords()
        ddlRecords.SelectedIndex = 0
        ddlRecords_SelectedIndexChanged(ddlRecords, New EventArgs())
        lblMessage.Text = String.Format("{0} deleted", record.Name)
    End Sub
#End Region

    Private Sub SetTextboxMaxLengths()
        tbxName.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaPanelGroup.TABLE_NAME, KaPanelGroup.FN_NAME))
    End Sub

    Private Sub PopulateRecords()
        Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
        ddlRecords.Items.Clear()
        If _currentUserPermission(_currentTableName).Create Then
            ddlRecords.Items.Add(New ListItem("Enter a new group", Guid.Empty.ToString()))
        Else
			ddlRecords.Items.Add(New ListItem("Select a group", Guid.Empty.ToString()))
		End If
        For Each record As KaPanelGroup In KaPanelGroup.GetAll(connection, "deleted=0", "name ASC")
            ddlRecords.Items.Add(New ListItem(record.Name, record.Id.ToString()))
        Next
    End Sub

    Private Sub UpdateMemberLists()
        Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
        Dim members As ArrayList = KaPanelGroupPanel.GetAll(connection, "deleted=0 AND panel_group_id=" & Q(ddlRecords.SelectedValue), "last_updated DESC")
        ddlPanel.Items.Clear()
        lstPanels.Items.Clear()
        For Each panel As KaPanel In KaPanel.GetAll(connection, "deleted=0", "name ASC")
            Dim isMember As Boolean = False
            For Each member As KaPanelGroupPanel In members
                If member.PanelId = panel.Id Then
                    isMember = True
                    Exit For
                End If
            Next
            If isMember Then
                lstPanels.Items.Add(New ListItem(panel.Name, panel.Id.ToString()))
            Else
                ddlPanel.Items.Add(New ListItem(panel.Name, panel.Id.ToString()))
            End If
        Next
        If ddlPanel.Items.Count > 0 Then ddlPanel.SelectedIndex = 0
    End Sub

    Private Sub UpdateButtons()
        If ddlRecords.SelectedIndex >= 0 AndAlso Guid.Parse(ddlRecords.SelectedValue) <> Guid.Empty Then
            btnDelete.Enabled = _currentUserPermission(_currentTableName).Delete
            btnAddPanel.Enabled = ddlPanel.SelectedIndex >= 0
            btnRemovePanel.Enabled = lstPanels.SelectedIndex >= 0
        Else
            btnDelete.Enabled = False
            btnAddPanel.Enabled = False
            btnRemovePanel.Enabled = False
        End If
    End Sub
	Private Sub SetControlUsabilityFromPermissions()
		With _currentUserPermission(_currentTableName)
			Dim shouldEnable = (.Edit AndAlso ddlRecords.SelectedIndex > 0) OrElse (.Create AndAlso ddlRecords.SelectedIndex = 0)
			pnlMain.Enabled = shouldEnable
			btnSave.Enabled = shouldEnable
			Dim value = Guid.Parse(ddlRecords.SelectedValue)
			btnDelete.Enabled = .Edit AndAlso .Delete AndAlso value <> Guid.Empty
		End With
	End Sub
	Protected Sub ScriptManager1_AsyncPostBackError(ByVal sender As Object, ByVal e As System.Web.UI.AsyncPostBackErrorEventArgs)
		ToolkitScriptManager1.AsyncPostBackErrorMessage = e.Exception.Message
	End Sub
End Class