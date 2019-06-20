Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class DefaultOrderSummarySettings : Inherits System.Web.UI.Page

    Private _currentUser As KaUser
Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
    Private _currentTableName As String = KaSetting.TABLE_NAME

    Public Const SN_ADDITIONAL_UNITS_FOR_PRODUCT_GROUPS = "AdditionalUnitsForProductGroup"

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Response.Cache.SetCacheability(HttpCacheability.NoCache)
        _currentUser = Utilities.GetUser(Me)
        _currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "GeneralSettings")
        If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")
        btnSaveOwnerOrderSummarySettings.Enabled = _currentUserPermission(_currentTableName).Edit
        btnSaveOrderSummaryProductGroupAdditionalUnits.Enabled = btnSaveOwnerOrderSummarySettings.Enabled
        btnDeleteOwnerOrderSummarySettings.Enabled = _currentUserPermission(_currentTableName).Delete
        If Not Page.IsPostBack Then
            PopulateOwnersList()
            PopulateProductGroupsCombo()
            PopulateAdditionalUnitsList()
            PopulateOrderSummaryOwnerSettings(Guid.Empty)

            Utilities.ConfirmBox(Me.btnDeleteOwnerOrderSummarySettings, "Are you sure you want to delete the order summary settings for this owner?")
        End If
    End Sub

    Private Sub PopulateOwnersList()
        ddlOrderSummaryOwner.Items.Clear()
        ddlOrderSummaryOwner.Items.Add(New ListItem("All owners", Guid.Empty.ToString))
        For Each o As KaOwner In KaOwner.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
            ddlOrderSummaryOwner.Items.Add(New ListItem(o.Name, o.Id.ToString))
        Next
    End Sub

#Region " Default order summary settings "
    'Public Const SN_SHOW_ACRES_ON_ORDER_SUMMARY As String = "ShowAcres"
    'Public Const SN_SHOW_BRANCH_ON_ORDER_SUMMARY As String = "ShowBranch"
    'Public Const SN_SHOW_EMAIL_ADDRESS_ON_ORDER_SUMMARY As String = "ShowEmailAddress"
    'Public Const SN_SHOW_INTERFACE_ON_ORDER_SUMMARY As String = "ShowInterface"
    'Public Const SN_SHOW_NOTES_ON_ORDER_SUMMARY As String = "ShowNotes"
    'Public Const SN_SHOW_SHIP_TO_ON_ORDER_SUMMARY As String = "ShowShipTo"
    'Public Const SN_SHOW_PO_NUMBER_ON_ORDER_SUMMARY As String = "ShowPoNumber"
    'Public Const SN_SHOW_RELEASE_NUMBER_ON_ORDER_SUMMARY As String = "ShowReleaseNumber"
    'Public Const SN_USE_TICKET_DELIVERED_AMOUNTS_ON_ORDER_SUMMARY As String = "UseTicketDeliveredAmounts"
    'Public Const SN_ADDITIONAL_UNITS_ON_ORDER_SUMMARY As String = "AdditionalUnits"
    'Public Const SN_SHOW_ALL_CUSTOM_FIELDS_ON_ORDER_SUMMARY As String = "ShowAllCustomFields"
    'Public Const SN_CUSTOM_FIELDS_ON_ORDER_SUMMARY As String = "CustomFieldsShown"

    Private Sub PopulateOrderSummaryOwnerSettings(ByVal ownerId As Guid)
        'Reset fields
        lblOrderSummarySettingsExist.Visible = False
        cbxOrderSummaryShowAcres.Checked = False
        cbxOrderSummaryShowBranch.Checked = False
        cbxOrderSummaryShowEmailAddress.Checked = False
        cbxOrderSummaryShowInterface.Checked = False
        cbxOrderSummaryShowNotes.Checked = False
        cbxOrderSummaryShowShipTo.Checked = False
        cbxOrderSummaryShowPoNumber.Checked = False
        cbxOrderSummaryShowReleaseNumber.Checked = False
        cbxShowAllCustomFieldsOnOrderSummary.Checked = True
        cbxOrderSummaryUseTicketDeliveredAmounts.Checked = False
        cblAdditionalUnitsForOrderSummary.ClearSelection()

        Dim c As OleDbConnection = GetUserConnection(_currentUser.Id)
        Dim allSettings As ArrayList = KaSetting.GetAll(c, "name like 'OrderSummarySetting:" & ownerId.ToString & "%' and deleted=0", "")
        lblOrderSummarySettingsExist.Visible = (allSettings.Count > 0)
        lblOrderSummarySettingsExist.Text = "Settings exist"
        If ownerId.Equals(Guid.Empty) Then
            Dim settingsValidForOwners As String = ""
            For Each possibleOwner As KaOwner In KaOwner.GetAll(c, "deleted = 0", "name ASC")
                Dim ownerTicketSettingsCountRdr As OleDbDataReader = Tm2Database.ExecuteReader(c, "SELECT COUNT(*) FROM settings WHERE name LIKE 'OrderSummarySetting:" & possibleOwner.Id.ToString & "%' and deleted=0")
                If ownerTicketSettingsCountRdr.Read() AndAlso ownerTicketSettingsCountRdr.Item(0) = 0 Then
                    If settingsValidForOwners.Length > 0 Then settingsValidForOwners &= ", "
                    settingsValidForOwners &= possibleOwner.Name
                End If
                ownerTicketSettingsCountRdr.Close()
            Next
            If settingsValidForOwners.Length > 0 Then lblOrderSummarySettingsExist.Text = "These settings are valid for " & settingsValidForOwners
        End If

        Boolean.TryParse(GetOrderSummarySettingByOwnerId(c, ownerId, ordersummary.SN_SHOW_ACRES_ON_ORDER_SUMMARY, True.ToString()), cbxOrderSummaryShowAcres.Checked)
        Boolean.TryParse(GetOrderSummarySettingByOwnerId(c, ownerId, ordersummary.SN_SHOW_BRANCH_ON_ORDER_SUMMARY, False.ToString()), cbxOrderSummaryShowBranch.Checked)
        Boolean.TryParse(GetOrderSummarySettingByOwnerId(c, ownerId, ordersummary.SN_SHOW_CUSTOMER_ACCOUNT_NUMBER_ON_ORDER_SUMMARY, False.ToString()), cbxShowCustomerAccountNumber.Checked)
        Boolean.TryParse(GetOrderSummarySettingByOwnerId(c, ownerId, ordersummary.SN_SHOW_EMAIL_ADDRESS_ON_ORDER_SUMMARY, False.ToString()), cbxOrderSummaryShowEmailAddress.Checked)
        Boolean.TryParse(GetOrderSummarySettingByOwnerId(c, ownerId, ordersummary.SN_SHOW_INTERFACE_ON_ORDER_SUMMARY, False.ToString()), cbxOrderSummaryShowInterface.Checked)
        Boolean.TryParse(GetOrderSummarySettingByOwnerId(c, ownerId, ordersummary.SN_SHOW_NOTES_ON_ORDER_SUMMARY, True.ToString()), cbxOrderSummaryShowNotes.Checked)
        Boolean.TryParse(GetOrderSummarySettingByOwnerId(c, ownerId, ordersummary.SN_SHOW_SHIP_TO_ON_ORDER_SUMMARY, False.ToString()), cbxOrderSummaryShowShipTo.Checked)
        Boolean.TryParse(GetOrderSummarySettingByOwnerId(c, ownerId, ordersummary.SN_SHOW_PO_NUMBER_ON_ORDER_SUMMARY, True.ToString()), cbxOrderSummaryShowPoNumber.Checked)
        Boolean.TryParse(GetOrderSummarySettingByOwnerId(c, ownerId, ordersummary.SN_SHOW_RELEASE_NUMBER_ON_ORDER_SUMMARY, True.ToString()), cbxOrderSummaryShowReleaseNumber.Checked)
        Boolean.TryParse(GetOrderSummarySettingByOwnerId(c, ownerId, ordersummary.SN_USE_TICKET_DELIVERED_AMOUNTS_ON_ORDER_SUMMARY, True.ToString()), cbxOrderSummaryUseTicketDeliveredAmounts.Checked)
        For Each unitId As String In GetOrderSummarySettingByOwnerId(c, ownerId, ordersummary.SN_ADDITIONAL_UNITS_ON_ORDER_SUMMARY, "").Trim().Split(",")
            For Each item As ListItem In cblAdditionalUnitsForOrderSummary.Items
                If item.Value = unitId Then
                    item.Selected = True
                    Exit For
                End If
            Next
        Next

        Boolean.TryParse(GetOrderSummarySettingByOwnerId(c, ownerId, ordersummary.SN_SHOW_ALL_CUSTOM_FIELDS_ON_ORDER_SUMMARY, True.ToString()), cbxShowAllCustomFieldsOnOrderSummary.Checked)
        PopulateOrderSummaryCustomFieldsShownSettings(c, ownerId)

        cbxShowAllCustomFieldsOnOrderSummary_CheckedChanged(cbxShowAllCustomFieldsOnOrderSummary, New EventArgs())
        Page.ClientScript.RegisterStartupScript(Page.ClientScript.GetType(), "ResetScrollPosition;", "resetDotNetScrollPosition();", True)
    End Sub

    Private Function GetOrderSummarySettingByOwnerId(ByVal connection As OleDbConnection, ByVal ownerId As Guid, ByVal settingName As String, ByVal defaultValue As Object) As Object
        'Find the owner specific setting.
        Dim allSettings As ArrayList = KaSetting.GetAll(connection, "name = " & Q("OrderSummarySetting:" & ownerId.ToString() & "/" & settingName) & " and deleted = 0", "")
        If allSettings.Count = 1 Then
            Return allSettings.Item(0).value
        End If

        'If there isn't an owner specific setting, get the All Owners setting, if that doesn't exist either, use the default value.
        Dim retval As String = KaSetting.GetSetting(connection, "OrderSummarySetting:" & Guid.Empty.ToString & "/" & settingName, defaultValue.ToString)
        Return retval
    End Function

    Private Sub PopulateAdditionalUnitsToDisplayForOrderSummaryProductGroups(ByVal productGroupId As Guid)
        cblOrderSummaryAdditionalUnitsForProductGroup.ClearSelection()

        If productGroupId <> Guid.Empty Then
            Dim c As OleDbConnection = GetUserConnection(_currentUser.Id)
            cblOrderSummaryAdditionalUnitsForProductGroup.ClearSelection()
            Dim orderSummarySettingFormat As String = "OrderSummarySetting:{0}:{1}/" & SN_ADDITIONAL_UNITS_FOR_PRODUCT_GROUPS
            Dim orderSummarySetting As String = String.Format(orderSummarySettingFormat, Guid.Empty.ToString(), productGroupId.ToString())
            Dim defaultOwnerProductUnits As String = KaSetting.GetSetting(Tm2Database.Connection, orderSummarySetting, "")
            orderSummarySetting = String.Format(orderSummarySettingFormat, ddlOrderSummaryOwner.SelectedValue, productGroupId.ToString())
            For Each unitIdString As String In KaSetting.GetSetting(Tm2Database.Connection, orderSummarySetting, defaultOwnerProductUnits, False, Nothing).Trim().Split(",")
                For Each item As ListItem In cblOrderSummaryAdditionalUnitsForProductGroup.Items
                    If item.Value = unitIdString Then
                        item.Selected = True
                        Exit For
                    End If
                Next
            Next
        End If
    End Sub

    Protected Sub btnSaveOrderSummaryProductGroupAdditionalUnits_Click(sender As Object, e As EventArgs) Handles btnSaveOrderSummaryProductGroupAdditionalUnits.Click
        If Guid.Parse(ddlOrderSummaryProductGroup.SelectedValue) <> Guid.Empty Then
            Dim c As OleDbConnection = GetUserConnection(_currentUser.Id)
            Dim webTicketSetting As String = "OrderSummarySetting:" & ddlOrderSummaryOwner.SelectedValue & ":" & ddlOrderSummaryProductGroup.SelectedValue

            Dim list As String = ""
            For Each item As ListItem In cblOrderSummaryAdditionalUnitsForProductGroup.Items
                If item.Selected Then list &= IIf(list.Length > 0, ",", "") & item.Value
            Next
            KaSetting.WriteSetting(c, webTicketSetting & "/" & SN_ADDITIONAL_UNITS_FOR_PRODUCT_GROUPS, list)
        End If
    End Sub

    Private Sub SaveOrderSummaryOwnerSettings(ByVal ownerId As Guid)
        Dim c As OleDbConnection = GetUserConnection(_currentUser.Id)

        Dim webTicketSetting As String = "OrderSummarySetting:" & ownerId.ToString() & "/"

        KaSetting.WriteSetting(c, webTicketSetting & ordersummary.SN_SHOW_ACRES_ON_ORDER_SUMMARY, cbxOrderSummaryShowAcres.Checked)
        KaSetting.WriteSetting(c, webTicketSetting & ordersummary.SN_SHOW_BRANCH_ON_ORDER_SUMMARY, cbxOrderSummaryShowBranch.Checked)
        KaSetting.WriteSetting(c, webTicketSetting & ordersummary.SN_SHOW_CUSTOMER_ACCOUNT_NUMBER_ON_ORDER_SUMMARY, cbxShowCustomerAccountNumber.Checked)
        KaSetting.WriteSetting(c, webTicketSetting & ordersummary.SN_SHOW_EMAIL_ADDRESS_ON_ORDER_SUMMARY, cbxOrderSummaryShowEmailAddress.Checked)
        KaSetting.WriteSetting(c, webTicketSetting & ordersummary.SN_SHOW_INTERFACE_ON_ORDER_SUMMARY, cbxOrderSummaryShowInterface.Checked)
        KaSetting.WriteSetting(c, webTicketSetting & ordersummary.SN_SHOW_NOTES_ON_ORDER_SUMMARY, cbxOrderSummaryShowNotes.Checked)
        KaSetting.WriteSetting(c, webTicketSetting & ordersummary.SN_SHOW_SHIP_TO_ON_ORDER_SUMMARY, cbxOrderSummaryShowShipTo.Checked)
        KaSetting.WriteSetting(c, webTicketSetting & ordersummary.SN_SHOW_PO_NUMBER_ON_ORDER_SUMMARY, cbxOrderSummaryShowPoNumber.Checked)
        KaSetting.WriteSetting(c, webTicketSetting & ordersummary.SN_SHOW_RELEASE_NUMBER_ON_ORDER_SUMMARY, cbxOrderSummaryShowReleaseNumber.Checked)
        KaSetting.WriteSetting(c, webTicketSetting & ordersummary.SN_USE_TICKET_DELIVERED_AMOUNTS_ON_ORDER_SUMMARY, cbxOrderSummaryUseTicketDeliveredAmounts.Checked)
        Dim list As String = ""
        For Each item As ListItem In cblAdditionalUnitsForOrderSummary.Items
            If item.Selected Then list &= IIf(list.Length > 0, ",", "") & item.Value
        Next
        KaSetting.WriteSetting(c, webTicketSetting & ordersummary.SN_ADDITIONAL_UNITS_ON_ORDER_SUMMARY, list)
        KaSetting.WriteSetting(c, webTicketSetting & ordersummary.SN_SHOW_ALL_CUSTOM_FIELDS_ON_ORDER_SUMMARY, cbxShowAllCustomFieldsOnOrderSummary.Checked)

        SaveOrderSummaryShowCustomFieldsSetting(webTicketSetting)
        PopulateOrderSummaryOwnerSettings(ownerId)
    End Sub


    Private Sub DeleteOrderSummaryOwnerSettings(ByVal ownerId As Guid)
        'Delete will set all the ticket settings to their 'default' values
        Dim c As OleDbConnection = GetUserConnection(_currentUser.Id)
        Dim allSettings As ArrayList = KaSetting.GetAll(c, "name like 'OrderSummarySetting:" & ownerId.ToString & "%' and deleted=0", "")
        For Each setting As KaSetting In allSettings
            Tm2Database.ExecuteNonQuery(c, "Delete from settings where id = " & Q(setting.Id))
        Next
        PopulateOrderSummaryOwnerSettings(ownerId)
    End Sub

    Private Sub ddlOrderSummaryOwner_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles ddlOrderSummaryOwner.SelectedIndexChanged
        PopulateOrderSummaryOwnerSettings(Guid.Parse(ddlOrderSummaryOwner.SelectedValue))
    End Sub

    Protected Sub btnSaveOwnerOrderSummarySettings_Click(sender As Object, e As EventArgs) Handles btnSaveOwnerOrderSummarySettings.Click
        SaveOrderSummaryOwnerSettings(Guid.Parse(ddlOrderSummaryOwner.SelectedValue))
    End Sub

    Protected Sub btnDeleteOwnerOrderSummarySettings_Click(sender As Object, e As EventArgs) Handles btnDeleteOwnerOrderSummarySettings.Click
        DeleteOrderSummaryOwnerSettings(Guid.Parse(ddlOrderSummaryOwner.SelectedValue))
    End Sub

    Private Sub PopulateOrderSummaryCustomFieldsShownSettings(ByVal connection As OleDbConnection, ownerId As Guid)
        cblShowCustomFieldsOnOrderSummary.Items.Clear()
        Dim customFieldsShownList As New List(Of String)
        customFieldsShownList.AddRange(GetOrderSummarySettingByOwnerId(connection, ownerId, ordersummary.SN_CUSTOM_FIELDS_ON_ORDER_SUMMARY, "").Split(","))
        Dim validTables As String = Q(KaCustomerAccount.TABLE_NAME) & "," & _
            Q(KaOrder.TABLE_NAME)
        For Each customField As KaCustomField In KaCustomField.GetAll(connection, String.Format("{0} = {1} AND {2} IN ({3})", KaCustomField.FN_DELETED, Q(False), KaCustomField.FN_TABLE_NAME, validTables), KaCustomField.FN_FIELD_NAME)
            cblShowCustomFieldsOnOrderSummary.Items.Add(New ListItem(Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(customField.TableName.Replace(KaLocation.TABLE_NAME, "Facilities").Replace("_", " ") & ": " & customField.FieldName), customField.Id.ToString()))
            cblShowCustomFieldsOnOrderSummary.Items(cblShowCustomFieldsOnOrderSummary.Items.Count - 1).Selected = (customFieldsShownList.Contains(customField.Id.ToString()))
        Next
        pnlOrderSummaryCustomFieldsAssigned.Visible = (cblShowCustomFieldsOnOrderSummary.Items.Count > 0)
    End Sub

    Private Sub SaveOrderSummaryShowCustomFieldsSetting(ByVal orderSummarySetting As String)
        Dim customFieldsShown As String = ""
        For Each customField As ListItem In cblShowCustomFieldsOnOrderSummary.Items
            If customField.Selected Then
                If customFieldsShown.Length > 0 Then customFieldsShown &= ","
                customFieldsShown &= customField.Value
            End If
        Next
        KaSetting.WriteSetting(GetUserConnection(_currentUser.Id), orderSummarySetting & ordersummary.SN_CUSTOM_FIELDS_ON_ORDER_SUMMARY, customFieldsShown)
    End Sub

    Private Sub cbxShowAllCustomFieldsOnOrderSummary_CheckedChanged(sender As Object, e As System.EventArgs) Handles cbxShowAllCustomFieldsOnOrderSummary.CheckedChanged
        cblShowCustomFieldsOnOrderSummary.Enabled = Not cbxShowAllCustomFieldsOnOrderSummary.Checked
    End Sub
#End Region

    Private Sub PopulateProductGroupsCombo()
        ddlOrderSummaryProductGroup.Items.Clear()
        ddlOrderSummaryProductGroup.Items.Add(New ListItem("", Guid.Empty.ToString()))
        Dim allProductGroups As ArrayList = KaProductGroup.GetAll(GetUserConnection(_currentUser.Id), "deleted = " & Q(False), "name asc")
        For Each productGroup As KaProductGroup In allProductGroups
            ddlOrderSummaryProductGroup.Items.Add(New ListItem(productGroup.Name, productGroup.Id.ToString))
        Next
        ddlOrderSummaryProductGroup.SelectedIndex = 0
    End Sub

    Private Sub PopulateAdditionalUnitsList()
        cblAdditionalUnitsForOrderSummary.Items.Clear()
        For Each n As KaUnit In KaUnit.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
            If n.BaseUnit <> KaUnit.Unit.Pulses AndAlso n.BaseUnit <> KaUnit.Unit.Seconds Then
                cblAdditionalUnitsForOrderSummary.Items.Add(New ListItem(n.Name, n.Id.ToString()))
                cblOrderSummaryAdditionalUnitsForProductGroup.Items.Add(New ListItem(n.Name, n.Id.ToString()))
            End If
        Next
    End Sub

    Protected Sub ddlOrderSummaryProductGroup_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles ddlOrderSummaryProductGroup.SelectedIndexChanged
        PopulateAdditionalUnitsToDisplayForOrderSummaryProductGroups(Guid.Parse(ddlOrderSummaryProductGroup.SelectedValue))
    End Sub
End Class