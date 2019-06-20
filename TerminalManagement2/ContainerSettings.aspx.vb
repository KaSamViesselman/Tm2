Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class ContainerSettings : Inherits System.Web.UI.Page

    Private _currentUser As KaUser
    Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
    Private _currentTableName As String = KaSetting.TABLE_NAME

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        lblSave.Visible = False
        Response.Cache.SetCacheability(HttpCacheability.NoCache)
        _currentUser = Utilities.GetUser(Me)
        _currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "GeneralSettings")
        If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")
        btnSave.Enabled = _currentUserPermission(_currentTableName).Edit
        If Not Page.IsPostBack Then
            PopulateLocations()
            DisplaySettings()
        End If
    End Sub

    Private Sub PopulateLocations()
        Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
        Dim packagedInventoryLocationId As Guid = Utilities.GetContainerPackagedInventoryLocationId(connection)
        ddlDefaultPackagedInventoryLocation.Items.Clear()
        ddlDefaultPackagedInventoryLocation.Items.Add(New ListItem("Select container packaged facility", Guid.Empty.ToString()))
        For Each location As KaLocation In KaLocation.GetAll(connection, "deleted = 0", "Name")
            ddlDefaultPackagedInventoryLocation.Items.Add(New ListItem(location.Name, location.Id.ToString()))
            If location.Id.Equals(packagedInventoryLocationId) Then ddlDefaultPackagedInventoryLocation.SelectedIndex = ddlDefaultPackagedInventoryLocation.Items.Count - 1
        Next
    End Sub
    Private Sub DisplaySettings()
        Dim c As OleDbConnection = GetUserConnection(_currentUser.Id)
        tbxContainerInspectionInterval.Text = KaSetting.GetSetting(c, "Containers/InspectionIntervalDays", "365")
        tbxContainerInspectionWarning.Text = KaSetting.GetSetting(c, "Containers/InspectionWarningDays", "30")
        tbxEquipmentInspectionInterval.Text = KaSetting.GetSetting(c, "ContainerEquipment/InspectionIntervalDays", "365")
        tbxEquipmentInspectionWarning.Text = KaSetting.GetSetting(c, "ContainerEquipment/InspectionWarningDays", "30")
    End Sub

    Private Sub btnSave_Click(sender As Object, e As System.EventArgs) Handles btnSave.Click
        ' validate settings
        If Not IsNumeric(tbxContainerInspectionInterval.Text) Then
            ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidContainerInspectionPercent", Utilities.JsAlert("Please enter a numeric value for the container inspection interval setting."))
            Exit Sub
        ElseIf Not IsNumeric(tbxContainerInspectionWarning.Text) Then
            ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidContainerInspSetting", Utilities.JsAlert("Please enter a numeric value for the container inspection warning setting."))
            Exit Sub
        ElseIf Not IsNumeric(tbxEquipmentInspectionInterval.Text) Then
            ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidContainerEquipmentInspectionPercent", Utilities.JsAlert("Please enter a numeric value for the container equipment inspection interval setting."))
            Exit Sub
        ElseIf Not IsNumeric(tbxEquipmentInspectionWarning.Text) Then
            ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidContainerEquipmentInspSetting", Utilities.JsAlert("Please enter a numeric value for the container equipment inspection warning setting."))
            Exit Sub
        Else
            Dim c As OleDbConnection = GetUserConnection(_currentUser.Id)

            Dim packagedInventoryLocationId As Guid = Guid.Parse(ddlDefaultPackagedInventoryLocation.SelectedValue)
            If packagedInventoryLocationId.Equals(Guid.Empty) Then
                Tm2Database.ExecuteNonQuery(c, Nothing, String.Format("UPDATE {0} SET {1} = 0, {2} = {3}, {4} = {5} WHERE deleted = 0 AND {6}=1", KaLocation.TABLE_NAME, KaLocation.FN_IS_CONTAINER_PACKAGED_INVENTORY, KaLocation.FN_LAST_UPDATED_APPLICATION, Database.Q(Database.ApplicationIdentifier), KaLocation.FN_LAST_UPDATED_USER, Database.Q(_currentUser.Name), KaLocation.FN_IS_CONTAINER_PACKAGED_INVENTORY))
            Else
                Tm2Database.ExecuteNonQuery(c, Nothing, String.Format("UPDATE {0} SET {1} = 1,{2} = {3}, {4} = {5} WHERE deleted = 0 AND {1} = 0 AND {6}={7}", KaLocation.TABLE_NAME, KaLocation.FN_IS_CONTAINER_PACKAGED_INVENTORY, KaLocation.FN_LAST_UPDATED_APPLICATION, Database.Q(Database.ApplicationIdentifier), KaLocation.FN_LAST_UPDATED_USER, Database.Q(_currentUser.Name), KaLocation.FN_ID, Database.Q(packagedInventoryLocationId)))
            End If
            KaSetting.WriteSetting(c, "Containers/InspectionIntervalDays", tbxContainerInspectionInterval.Text)
            KaSetting.WriteSetting(c, "Containers/InspectionWarningDays", tbxContainerInspectionWarning.Text)
            KaSetting.WriteSetting(c, "ContainerEquipment/InspectionIntervalDays", tbxEquipmentInspectionInterval.Text)
            KaSetting.WriteSetting(c, "ContainerEquipment/InspectionWarningDays", tbxEquipmentInspectionWarning.Text)

            lblSave.Visible = True
        End If
    End Sub
End Class