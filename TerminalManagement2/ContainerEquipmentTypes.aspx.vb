Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class ContainerEquipmentTypes : Inherits System.Web.UI.Page
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
            PopulateContainerEquipmentTypeList()
            Utilities.ConfirmBox(Me.btnDelete, "Are you sure you want to delete this container equipment type?") ' Delete confirmation box setup
            If Page.Request("ContainerEquipmentTypeId") IsNot Nothing Then
                Try
                    ddlContainerEquipmentTypes.SelectedValue = Page.Request("ContainerEquipmentTypeId")
                Catch ex As ArgumentOutOfRangeException
                    ddlContainerEquipmentTypes.SelectedIndex = 0
                End Try
            End If
            ddlContainerEquipmentTypes_SelectedIndexChanged(ddlContainerEquipmentTypes, New EventArgs())
            Utilities.SetFocus(tbxName, Me) ' set focus to the first textbox on the page
        End If
    End Sub

    Protected Sub ddlContainerEquipmentTypes_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles ddlContainerEquipmentTypes.SelectedIndexChanged
        Dim id As Guid = Guid.Parse(ddlContainerEquipmentTypes.SelectedValue)
        PopulateContainerEquipmentTypeInformation(id)
        UpdateControls(id)

    End Sub

    Protected Sub btnSave_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnSave.Click
        lblStatus.Text = ""
        If ValidateFields() Then
            With New KaContainerEquipmentType()
                .Id = Guid.Parse(ddlContainerEquipmentTypes.SelectedValue)
                .Name = tbxName.Text
                .UseGeneralInspectionDates = cbxUseGeneralSettingsForInterval.Checked
                .InspectionInterval = tbxEquipmentInspectionInterval.Text
                .InspectionWarning = tbxEquipmentInspectionWarning.Text
                If .Id <> Guid.Empty Then
                    .SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                    lblStatus.Text = "Container equipment type updated successfully."
                Else
                    .SqlInsert(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                    lblStatus.Text = "Container equipment type added successfully."
                End If
                PopulateContainerEquipmentTypeList()
                UpdateControls(.Id)
            End With
        End If
    End Sub

    Protected Sub btnDelete_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnDelete.Click
        Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
        Dim containerEquipments As String = "" ' find any containers that reference this container type
        For Each containerEquipment As KaContainerEquipment In KaContainerEquipment.GetAll(connection, String.Format("deleted=0 AND equipment_type_id='{0}'", ddlContainerEquipmentTypes.SelectedValue), "name ASC")
            containerEquipments &= IIf(containerEquipments.Length > 0, ", ", "") & containerEquipment.Name
        Next
        If containerEquipments.Length > 0 Then ' this container equipment type is referenced by container records, can't delete
            ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidTypeUsed", Utilities.JsAlert("Cannot delete container equipment type """ & tbxName.Text & """ because it is referenced by these container equipment: " & containerEquipments & "."))
        Else ' this container equipment type is not referenced, it's OK to delete
            With New KaContainerEquipmentType(connection, Guid.Parse(ddlContainerEquipmentTypes.SelectedValue))
                .Deleted = True
                .SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
            End With
            lblStatus.Text = "Container equipment type deleted successfully."
            PopulateContainerEquipmentTypeList()
            PopulateContainerEquipmentTypeInformation(Guid.Empty)
            UpdateControls(Guid.Empty)
        End If
    End Sub
#End Region

    Private Sub UpdateControls(id As Guid)
        ddlContainerEquipmentTypes.SelectedValue = id.ToString()
        btnDelete.Enabled = id <> Guid.Empty AndAlso _currentUserPermission(_currentTableName).Delete
        SetControlUsabilityFromPermissions()
    End Sub

    Private Function ValidateFields() As Boolean
        If tbxName.Text.Trim().Length = 0 Then
            ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidContainerEquipmentName", Utilities.JsAlert("Please specify a name for the container equipment type."))
            Return False
        ElseIf Not IsNumeric(tbxEquipmentInspectionInterval.Text) Then
            ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidContainerEquipmentInspectionPercent", Utilities.JsAlert("Please enter a numeric value for the container equipment inspection interval setting."))
            Return False
        ElseIf Not IsNumeric(tbxEquipmentInspectionWarning.Text) Then
            ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidContainerEquipmentInspSetting", Utilities.JsAlert("Please enter a numeric value for the container equipment inspection warning setting."))
            Return False
        Else
            Dim list As ArrayList = KaContainerEquipmentType.GetAll(GetUserConnection(_currentUser.Id), "name = " & Q(tbxName.Text.Trim), "")
            If list.Count > 0 AndAlso list(0).Id <> Guid.Parse(ddlContainerEquipmentTypes.SelectedValue) Then
                ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidDuplicateName", Utilities.JsAlert("A container equipment type with the name """ & tbxName.Text.Trim() & """ already exists. Please enter a unique name."))
                Return False
            End If
            Return True
        End If
    End Function

    Private Sub PopulateContainerEquipmentTypeList()
        ddlContainerEquipmentTypes.Items.Clear()
        If _currentUserPermission(_currentTableName).Create Then ddlContainerEquipmentTypes.Items.Add(New ListItem("Enter a new container equipment type", Guid.Empty.ToString())) Else ddlContainerEquipmentTypes.Items.Add(New ListItem("Select a container equipment type", Guid.Empty.ToString()))
        For Each r As KaContainerEquipmentType In KaContainerEquipmentType.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
            ddlContainerEquipmentTypes.Items.Add(New ListItem(r.Name, r.Id.ToString()))
        Next
    End Sub

    Private Sub PopulateContainerEquipmentTypeInformation(ByVal equipmentTypeId As Guid)
        With New KaContainerEquipmentType()
            .Id = equipmentTypeId
            If .Id <> Guid.Empty Then .SqlSelect(GetUserConnection(_currentUser.Id))
            tbxName.Text = .Name
            cbxUseGeneralSettingsForInterval.Checked = .UseGeneralInspectionDates
            tbxEquipmentInspectionInterval.Text = .InspectionInterval
            tbxEquipmentInspectionWarning.Text = .InspectionWarning
        End With
    End Sub

    Private Sub SetTextboxMaxLengths()
        tbxName.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaContainerEquipmentType.TABLE_NAME, "name"))
    End Sub

    Private Sub SetControlUsabilityFromPermissions()
        With _currentUserPermission(_currentTableName)
            Dim shouldEnable = (.Edit AndAlso ddlContainerEquipmentTypes.SelectedIndex > 0) OrElse (.Create AndAlso ddlContainerEquipmentTypes.SelectedIndex = 0)
            tbxName.Enabled = shouldEnable
            btnSave.Enabled = shouldEnable
            btnDelete.Enabled = Not Guid.Parse(ddlContainerEquipmentTypes.SelectedValue).Equals(Guid.Empty) AndAlso .Edit AndAlso .Delete
        End With
    End Sub
End Class