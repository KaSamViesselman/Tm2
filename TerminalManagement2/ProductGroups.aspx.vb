Imports KahlerAutomation.KaTm2Database

Public Class ProductGroups : Inherits System.Web.UI.Page

    Private _currentUser As KaUser
Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
    Private _currentTableName As String = KaProduct.TABLE_NAME

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Response.Cache.SetCacheability(HttpCacheability.NoCache)
        _currentUser = Utilities.GetUser(Me)
        _currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Products")
        If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")

        If Not Page.IsPostBack Then
            SetTextboxMaxLengths()
            PopulateProductGroups()
            SetControlUsabilityFromPermissions()
            If Page.Request("ProductGroupId") IsNot Nothing Then
                Try
                    ddlProductGroups.SelectedValue = Page.Request("ProductGroupId")
                Catch ex As ArgumentOutOfRangeException
                    ddlProductGroups.SelectedIndex = 0
                End Try
            End If
            ddlProductGroups_SelectedIndexChanged(ddlProductGroups, New EventArgs())
            Utilities.ConfirmBox(Me.btnDelete, "Are you sure you want to delete this product group?")
            Utilities.SetFocus(tbxName, Me)
        End If
    End Sub

    Private Sub PopulateProductGroups()
        ddlProductGroups.Items.Clear()
        Dim li As ListItem = New ListItem
        If _currentUserPermission(_currentTableName).Create Then
            li.Text = "Enter a new product group"
        Else
			li.Text = "Select a product group"
		End If
        li.Value = Guid.Empty.ToString
        ddlProductGroups.Items.Add(li)
        For Each productGroup As KaProductGroup In KaProductGroup.GetAll(GetUserConnection(_currentUser.Id), "deleted = " & Q(False), "name asc")
            li = New ListItem
            li.Text = productGroup.Name
            li.Value = productGroup.Id.ToString
            ddlProductGroups.Items.Add(li)
        Next
    End Sub

    Private Sub ddlProductGroups_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlProductGroups.SelectedIndexChanged
        lblStatus.Text = ""
        Dim productGroupId As Guid = Guid.Parse(ddlProductGroups.SelectedValue)
        If productGroupId <> Guid.Empty Then
            btnDelete.Enabled = _currentUserPermission(_currentTableName).Delete
        Else
            btnDelete.Enabled = False
            Utilities.SetFocus(tbxName, Me)
        End If
        PopulateProductGroupsData()
        SetControlUsabilityFromPermissions()
    End Sub

    Private Sub PopulateProductGroupsData()
        ClearProductGroupData()
        If Guid.Parse(ddlProductGroups.SelectedValue) <> Guid.Empty Then
            Dim productGroup As KaProductGroup = New KaProductGroup(GetUserConnection(_currentUser.Id), Guid.Parse(ddlProductGroups.SelectedValue))
            tbxName.Text = productGroup.Name
        End If
    End Sub

    Private Sub ClearProductGroupData()
        tbxName.Text = ""
    End Sub

    Private Sub btnSave_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnSave.Click
        If (ValidateFields()) Then
            Dim productGroup As KaProductGroup = New KaProductGroup
            productGroup.Name = tbxName.Text.Trim
            Dim inserted As Boolean = False
            If ddlProductGroups.SelectedIndex > 0 Then
                productGroup.Id = Guid.Parse(ddlProductGroups.SelectedValue)
                productGroup.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                inserted = False
            Else
                productGroup.SqlInsert(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                inserted = True
            End If
            PopulateProductGroups()
            ddlProductGroups.SelectedValue = productGroup.Id.ToString
            ddlProductGroups_SelectedIndexChanged(ddlProductGroups, New EventArgs)
            If inserted Then
                lblStatus.Text = "Product group successfully added"
            Else
                lblStatus.Text = "Product group successfully updated"
            End If
        End If
    End Sub

    Private Function ValidateFields() As Boolean
        If tbxName.Text.Trim = "" Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidName", Utilities.JsAlert("Please specify a name for this product group.")) : Return False
        Dim conditions As String = String.Format("deleted=0 AND name={0} AND id<>{1}", Q(tbxName.Text), Q(ddlProductGroups.SelectedValue))
        If KaProductGroup.GetAll(GetUserConnection(_currentUser.Id), conditions, "").Count > 0 Then
            ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidDuplicateName", Utilities.JsAlert("A product group with the name """ & tbxName.Text & """ already exists. Please specify a unique name for this product group.")) : Return False
        End If
        Return True
    End Function

    Private Sub btnDelete_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnDelete.Click
        lblStatus.Text = ""
        If Guid.Parse(ddlProductGroups.SelectedValue) <> Guid.Empty Then
            With New KaProductGroup(GetUserConnection(_currentUser.Id), Guid.Parse(ddlProductGroups.SelectedValue))
                .Deleted = True
                .SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                ddlProductGroups.SelectedIndex = 0
                ClearProductGroupData()
                lblStatus.Text = "Selected Product Group deleted successfully"
                btnSave.Enabled = True
                btnDelete.Enabled = False
                PopulateProductGroups()
                ddlProductGroups.SelectedIndex = 0
            End With
        End If
    End Sub

    Private Sub SetTextboxMaxLengths()
        tbxName.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaProductGroup.TABLE_NAME, "name"))
    End Sub
    Private Sub SetControlUsabilityFromPermissions()
        With _currentUserPermission(_currentTableName)
            Dim shouldEnable = (.Edit AndAlso ddlProductGroups.SelectedIndex > 0) OrElse (.Create AndAlso ddlProductGroups.SelectedIndex = 0)
            tbxName.Enabled = shouldEnable
            btnSave.Enabled = shouldEnable
            Dim value = Guid.Parse(ddlProductGroups.SelectedValue)
            btnDelete.Enabled = .Edit AndAlso .Delete AndAlso value <> Guid.Empty
        End With
    End Sub
End Class