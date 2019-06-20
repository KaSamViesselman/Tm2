Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class ContainerTypes : Inherits System.Web.UI.Page
    Private _currentUser As KaUser
    Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
    Private _currentTableName As String = KaContainer.TABLE_NAME

#Region "Events"
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        Response.Cache.SetCacheability(HttpCacheability.NoCache)
        _currentUser = Utilities.GetUser(Me)
        _currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Containers")
        If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")
        If Not Page.IsPostBack Then
            SetTextboxMaxLengths()
            PopulateContainerTypeList()
            Utilities.ConfirmBox(Me.btnDelete, "Are you sure you want to delete this container type?") ' Delete confirmation box setup
            Dim containerTypeId As Guid = Guid.Empty
            Guid.TryParse(Page.Request("ContainerTypeId"), containerTypeId)

            If Page.Request("ContainerTypeId") IsNot Nothing Then
                Try
                    ddlContainerTypes.SelectedValue = Page.Request("ContainerTypeId")
                Catch ex As ArgumentOutOfRangeException
                    ddlContainerTypes.SelectedIndex = 0
                End Try
            End If
            ddlContainerTypes_SelectedIndexChanged(ddlContainerTypes, New EventArgs())
            Utilities.SetFocus(tbxName, Me) ' set focus to the first textbox on the page
        End If
    End Sub

    Protected Sub ddlContainerTypes_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles ddlContainerTypes.SelectedIndexChanged
        Dim id As Guid = Guid.Parse(ddlContainerTypes.SelectedValue)
        PopulateContainerTypeInformation(id)
        UpdateControls(id)
    End Sub

    Protected Sub btnSave_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnSave.Click
        lblStatus.Text = ""
        If ValidateFields() Then
            Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
            With New KaContainerType()
                .Id = Guid.Parse(ddlContainerTypes.SelectedValue)
                .Name = tbxName.Text
                .UseGeneralInspectionDates = cbxUseGeneralSettingsForInterval.Checked
                .InspectionInterval = tbxContainerInspectionInterval.Text
                .InspectionWarning = tbxContainerInspectionWarning.Text
                If .Id <> Guid.Empty Then
                    .SqlUpdate(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                    lblStatus.Text = "Container type updated successfully."
                Else
                    .SqlInsert(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                    lblStatus.Text = "Container type added successfully."
                End If
                PopulateContainerTypeList()
                UpdateControls(.Id)
            End With
        End If
    End Sub

    Protected Sub btnDelete_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnDelete.Click
        lblStatus.Text = "Container type deleted successfully."
        Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
        Dim containers As String = "" ' find any containers that reference this container type
        For Each container As KaContainer In KaContainer.GetAll(connection, String.Format("deleted=0 AND container_type_id='{0}'", ddlContainerTypes.SelectedValue), "number ASC")
            containers &= IIf(containers.Length > 0, ", ", "") & container.Number
        Next
        If containers.Length > 0 Then ' this container type is referenced by container records, can't delete
            ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidTypeUsed", Utilities.JsAlert("Cannot delete container type """ & tbxName.Text & """ because it is referenced by these containers: " & containers & "."))
        Else ' this container type is not referenced, it's OK to delete
            With New KaContainerType(connection, Guid.Parse(ddlContainerTypes.SelectedValue))
                .Deleted = True
                .SqlUpdate(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
            End With
            lblStatus.Text = "Container type deleted successfully."
            PopulateContainerTypeList()
            PopulateContainerTypeInformation(Guid.Empty)
            UpdateControls(Guid.Empty)
        End If
    End Sub
#End Region

    Private Sub UpdateControls(id As Guid)
        ddlContainerTypes.SelectedValue = id.ToString()
        btnDelete.Enabled = id <> Guid.Empty
        SetControlUsabilityFromPermissions()
    End Sub

    Private Function ValidateFields() As Boolean
        If tbxName.Text.Trim().Length = 0 Then
            ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidName", Utilities.JsAlert("Please specify a name for the container type."))
            Return False
        ElseIf Not IsNumeric(tbxContainerInspectionInterval.Text) Then
            ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidContainerInspectionPercent", Utilities.JsAlert("Please enter a numeric value for the container inspection interval setting."))
            Return False
        ElseIf Not IsNumeric(tbxContainerInspectionWarning.Text) Then
            ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidContainerInspSetting", Utilities.JsAlert("Please enter a numeric value for the container inspection warning setting."))
            Return False
        Else
            Dim list As ArrayList = KaContainerType.GetAll(GetUserConnection(_currentUser.Id), "name = " & Q(tbxName.Text.Trim), "")
            If list.Count > 0 AndAlso list(0).Id <> Guid.Parse(ddlContainerTypes.SelectedValue) Then
                ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidNameUsed", Utilities.JsAlert("A container type with the name """ & tbxName.Text.Trim() & """ already exists. Please enter a unique name."))
                Return False
            End If
            Return True
        End If
    End Function

    Private Sub PopulateContainerTypeList()
        ddlContainerTypes.Items.Clear()
        If _currentUserPermission(_currentTableName).Create Then ddlContainerTypes.Items.Add(New ListItem("Enter a new container type", Guid.Empty.ToString())) Else ddlContainerTypes.Items.Add(New ListItem("Select a container type", Guid.Empty.ToString()))
        For Each r As KaContainerType In KaContainerType.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
            ddlContainerTypes.Items.Add(New ListItem(r.Name, r.Id.ToString()))
        Next
    End Sub

    Private Sub PopulateContainerTypeInformation(id As Guid)
        With New KaContainerType()
            .Id = id
            If .Id <> Guid.Empty Then .SqlSelect(GetUserConnection(_currentUser.Id))
            tbxName.Text = .Name
            cbxUseGeneralSettingsForInterval.Checked = .UseGeneralInspectionDates
            tbxContainerInspectionInterval.Text = .InspectionInterval
            tbxContainerInspectionWarning.Text = .InspectionWarning
        End With
    End Sub

    Private Sub SetTextboxMaxLengths()
        tbxName.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaContainerType.TABLE_NAME, "name"))
    End Sub

    Private Sub SetControlUsabilityFromPermissions()
        With _currentUserPermission(_currentTableName)
            Dim shouldEnable = (.Edit AndAlso ddlContainerTypes.SelectedIndex > 0) OrElse (.Create AndAlso ddlContainerTypes.SelectedIndex = 0)
            tbxName.Enabled = shouldEnable
            btnSave.Enabled = shouldEnable
            btnDelete.Enabled = Not Guid.Parse(ddlContainerTypes.SelectedValue).Equals(Guid.Empty) AndAlso .Edit AndAlso .Delete
        End With
    End Sub
End Class
