Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class ContainerEquipment : Inherits System.Web.UI.Page
    Private _currentUser As KaUser
    Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
    Private _currentTableName As String = KaContainer.TABLE_NAME

#Region "Events"
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Response.Cache.SetCacheability(HttpCacheability.NoCache)
        _currentUser = Utilities.GetUser(Me)
        _currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Containers")
        If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")

        If Not Page.IsPostBack Then
            SetTextboxMaxLengths()
            PopulateFacilityList()
            PopulateContainerEquipmentList()
            PopulateContainerEquipmentTypeList()
            PopulateOwnerList()
            PopulateLocationList()
            PopulateContainerList()
            Utilities.ConfirmBox(Me.btnDelete, "Are you sure you want to delete this container equipment?") ' Delete confirmation box setup
            If Page.Request("ContainerEquipmentId") IsNot Nothing Then
                ddlFacilityFilter.SelectedIndex = 0

                Try
                    ddlContainerEquipment.SelectedValue = Page.Request("ContainerEquipmentId")
                Catch ex As ArgumentOutOfRangeException
                    ddlContainerEquipment.SelectedIndex = 0
                End Try
            Else
                Try
                    ddlFacilityFilter.SelectedValue = KaSetting.GetSetting(GetUserConnection(_currentUser.Id), "ContainerEquipmentPage:" & _currentUser.Id.ToString & "/LastFacilityUsed", Guid.Empty.ToString())
                    ddlFacilityFilter_SelectedIndexChanged(Nothing, Nothing)
                Catch ex As ArgumentOutOfRangeException
                    'Suppress
                End Try
            End If
            ddlContainerEquipment_SelectedIndexChanged(ddlContainerEquipment, New EventArgs())
            Utilities.SetFocus(tbxName, Me) ' set focus to the first textbox on the page
        End If
    End Sub

    Private Sub btnSave_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnSave.Click
        lblStatus.Text = ""
        If tbxName.Text.Trim = "" Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidName", Utilities.JsAlert("Please specify a name.")) : Exit Sub
        Dim lastInspectedDate As DateTime
        Try ' check for improper date formatting
            lastInspectedDate = DateTime.Parse(tbxLastInspectedDate.Value)
            If lastInspectedDate < SQL_MINDATE Then ' check that date is later then sql min date 
                Throw New FormatException
            End If
        Catch ex As FormatException
            ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidLastInspectedDate", Utilities.JsAlert("Please enter a valid date for the (Last inspected) date"))
            Return
        End Try
        If KaContainerEquipment.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND id<>" & Q(Guid.Parse(ddlContainerEquipment.SelectedValue)) & " AND name=" & Q(tbxName.Text.Trim), "").Count > 0 Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidDuplicateName", Utilities.JsAlert("Container equipment named """ & tbxName.Text & """ already exists. Please specify a unique name for the container equipment.")) : Exit Sub
        With New KaContainerEquipment()
            .Id = Guid.Parse(ddlContainerEquipment.SelectedValue)
            If .Id <> Guid.Empty Then .SqlSelect(GetUserConnection(_currentUser.Id))
            .Name = tbxName.Text.Trim
            .EquipmentTypeId = Guid.Parse(ddlType.SelectedValue)
            .OwnerId = Guid.Parse(ddlOwner.SelectedValue)
            .LocationId = Guid.Parse(ddlFacility.SelectedValue)
            .WithCustomer = chkWithCustomer.Checked
            Dim oldContainerId As Guid = .ContainerId
            Dim newContainerId As Guid = Guid.Parse(ddlContainer.SelectedValue)
            .ContainerId = Guid.Parse(ddlContainer.SelectedValue)
            .BarcodeId = tbxBarcode.Text.Trim
            .LastInspected = lastInspectedDate
            .PassedInspection = chkPassedInspection.Checked

            If .Id <> Guid.Empty Then
                .SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                lblStatus.Text = "Container equipment successfully updated."
            Else
                .SqlInsert(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                PopulateContainerEquipmentList()
                ddlContainerEquipment.SelectedValue = .Id.ToString()
                btnDelete.Enabled = _currentUserPermission(_currentTableName).Delete
                lblStatus.Text = "Container equipment successfully added."
            End If
            If oldContainerId <> Guid.Empty Then
                Dim selectedContainer As KaContainer = New KaContainer(GetUserConnection(_currentUser.Id), oldContainerId)
                selectedContainer.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
            End If
            If newContainerId <> Guid.Empty And newContainerId <> oldContainerId Then
                Dim selectedContainer As KaContainer = New KaContainer(GetUserConnection(_currentUser.Id), newContainerId)
                selectedContainer.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
            End If
        End With
    End Sub

    Protected Sub btnDelete_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnDelete.Click
        lblStatus.Text = ""
        With New KaContainerEquipment(GetUserConnection(_currentUser.Id), Guid.Parse(ddlContainerEquipment.SelectedValue))
            .Deleted = True
            .SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
            lblStatus.Text = "Container equipment successfully deleted."
        End With
        PopulateContainerEquipmentList()
        ddlContainerEquipment.SelectedValue = Guid.Empty.ToString()
        PopulateContainerEquipmentInformation(Guid.Empty)
        btnDelete.Enabled = False
    End Sub

    Private Sub ddlContainerEquipment_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlContainerEquipment.SelectedIndexChanged
        PopulateContainerEquipmentInformation(Guid.Parse(ddlContainerEquipment.SelectedValue))
        SetControlUsabilityFromPermissions()
    End Sub
#End Region

    Private Sub PopulateFacilityList()
        ddlFacilityFilter.Items.Clear()
        ddlFacilityFilter.Items.Add(New ListItem("All facilities", Guid.Empty.ToString()))
        For Each l As KaLocation In KaLocation.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
            ddlFacilityFilter.Items.Add(New ListItem(l.Name, l.Id.ToString))
        Next
    End Sub

    Private Sub PopulateContainerEquipmentList()
        ddlContainerEquipment.Items.Clear()
        If _currentUserPermission(_currentTableName).Create Then ddlContainerEquipment.Items.Add(New ListItem("Enter a new container equipment", Guid.Empty.ToString())) Else ddlContainerEquipment.Items.Add(New ListItem("Select a container equipment", Guid.Empty.ToString()))
        For Each equip As KaContainerEquipment In KaContainerEquipment.GetAll(GetUserConnection(_currentUser.Id), "deleted <> 1" & IIf(ddlFacilityFilter.SelectedIndex > 0, " AND (location_id = " & Q(ddlFacilityFilter.SelectedValue) & " OR location_id = " & Q(Guid.Empty) & ")", ""), "name asc")
            ddlContainerEquipment.Items.Add(New ListItem(equip.Name, equip.Id.ToString))
        Next
    End Sub

    Private Sub PopulateContainerEquipmentTypeList()
        ddlType.Items.Clear()
        ddlType.Items.Add(New ListItem("", Guid.Empty.ToString()))
        For Each equipType As KaContainerEquipmentType In KaContainerEquipmentType.GetAll(GetUserConnection(_currentUser.Id), "deleted <> 1", "name asc")
            ddlType.Items.Add(New ListItem(equipType.Name, equipType.Id.ToString))
        Next
    End Sub

    Private Sub PopulateOwnerList()
        ddlOwner.Items.Clear()
        If _currentUser.OwnerId = Guid.Empty Then ddlOwner.Items.Add(New ListItem("All owners", Guid.Empty.ToString()))
        For Each r As KaOwner In KaOwner.GetAll(GetUserConnection(_currentUser.Id), "deleted <> 1" & IIf(_currentUser.OwnerId = Guid.Empty, "", " AND id=" & Q(_currentUser.OwnerId)), "name ASC")
            ddlOwner.Items.Add(New ListItem(r.Name, r.Id.ToString()))
        Next
    End Sub

    Private Sub PopulateLocationList()
        ddlFacility.Items.Clear()
        ddlFacility.Items.Add(New ListItem("", Guid.Empty.ToString()))
        For Each r As KaLocation In KaLocation.GetAll(GetUserConnection(_currentUser.Id), "deleted <> 1" & IIf(ddlFacilityFilter.SelectedIndex > 0, " AND id = " & Q(ddlFacilityFilter.SelectedValue), ""), "name ASC")
            ddlFacility.Items.Add(New ListItem(r.Name, r.Id.ToString()))
        Next
    End Sub

    Private Sub PopulateContainerList()
        ddlContainer.Items.Clear()
        ddlContainer.Items.Add(New ListItem("", Guid.Empty.ToString()))
        For Each r As KaContainer In KaContainer.GetAll(GetUserConnection(_currentUser.Id), "deleted <> 1", "number asc")
            ddlContainer.Items.Add(New ListItem(r.Number, r.Id.ToString()))
        Next
    End Sub

    Private Sub PopulateContainerEquipmentInformation(ByVal equipmentId As Guid)
        lblStatus.Text = ""
        With New KaContainerEquipment()
            .Id = equipmentId
            If .Id <> Guid.Empty Then .SqlSelect(GetUserConnection(_currentUser.Id))

            tbxName.Text = .Name
            Try
                ddlType.SelectedValue = .EquipmentTypeId.ToString()
            Catch ex As ArgumentOutOfRangeException
                ddlType.SelectedIndex = 0
                ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidContainerEquiptypeId", Utilities.JsAlert("Record not found in container equipment types where ID = " & .EquipmentTypeId.ToString() & ". Container equipment type not set."))
            End Try
            Try
                ddlOwner.SelectedValue = .OwnerId.ToString()
            Catch ex As ArgumentOutOfRangeException
                ddlOwner.SelectedValue = Utilities.GetUser(Me).OwnerId.ToString()
                ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidOwnerId", Utilities.JsAlert("Record not found in owners where ID = " & .OwnerId.ToString() & ". Owner not set."))
            End Try
            Try
                ddlFacility.SelectedValue = .LocationId.ToString()
            Catch ex As ArgumentOutOfRangeException
                ddlFacility.SelectedIndex = 0
                ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidFacilityId", Utilities.JsAlert("Record not found in facilities where ID = " & .LocationId.ToString() & ". Facility not set."))
            End Try
            chkWithCustomer.Checked = .WithCustomer
            Try
                ddlContainer.SelectedValue = .ContainerId.ToString
            Catch ex As Exception
                ddlContainer.SelectedValue = 0
                ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidContainerId", Utilities.JsAlert("Record not found in containers where ID = " & .ContainerId.ToString() & ". Container not set."))
            End Try
            tbxBarcode.Text = .BarcodeId
            tbxLastInspectedDate.Value = .LastInspected
            chkPassedInspection.Checked = .PassedInspection
        End With
        btnDelete.Enabled = equipmentId <> Guid.Empty AndAlso _currentUserPermission(_currentTableName).Delete
    End Sub

    Private Sub SetTextboxMaxLengths()
        tbxBarcode.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaContainerEquipment.TABLE_NAME, "barcode_id"))
        tbxName.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaContainerEquipment.TABLE_NAME, "name"))
    End Sub
    Private Sub SetControlUsabilityFromPermissions()
        With _currentUserPermission(_currentTableName)
            Dim shouldEnable = (.Edit AndAlso ddlContainerEquipment.SelectedIndex > 0) OrElse (.Create AndAlso ddlContainerEquipment.SelectedIndex = 0)
            pnlEven.Enabled = shouldEnable
            btnSave.Enabled = shouldEnable
            btnDelete.Enabled = Not Guid.Parse(ddlContainerEquipment.SelectedValue).Equals(Guid.Empty) AndAlso .Edit AndAlso .Delete
        End With
    End Sub

    Private Sub ddlFacilityFilter_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlFacilityFilter.SelectedIndexChanged
        Dim currentContainerEquipmentId As String = ddlContainerEquipment.SelectedValue
        PopulateContainerEquipmentList()
        PopulateLocationList()
        Try
            ddlContainerEquipment.SelectedValue = currentContainerEquipmentId
        Catch ex As ArgumentOutOfRangeException

        End Try
        ddlContainerEquipment_SelectedIndexChanged(ddlContainerEquipment, New EventArgs)
        KaSetting.WriteSetting(GetUserConnection(_currentUser.Id), "ContainerEquipmentPage:" & _currentUser.Id.ToString & "/LastFacilityUsed", ddlFacilityFilter.SelectedValue)
    End Sub
End Class