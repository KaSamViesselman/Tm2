Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class Owners
    Inherits System.Web.UI.Page

    Private _currentUser As KaUser
    Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
    Private _currentTableName As String = KaOwner.TABLE_NAME
    Private _customFields As New List(Of KaCustomField)
    Private _customFieldData As New List(Of KaCustomFieldData)

    Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load, Me.Load
        Response.Cache.SetCacheability(HttpCacheability.NoCache)
        _currentUser = Utilities.GetUser(Me)
        _currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Owners")
        If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")
        If Not Page.IsPostBack Then
            SetTextboxMaxLengths()
            _customFields.Clear()
            For Each customField As KaCustomField In KaCustomField.GetAll(GetUserConnection(_currentUser.Id), String.Format("deleted=0 AND {0} = {1}", KaCustomField.FN_TABLE_NAME, Q(KaOwner.TABLE_NAME)), KaCustomField.FN_FIELD_NAME)
                _customFields.Add(customField)
            Next
            PopulateOwnerList(_currentUser)
            SetControlUsabilityFromPermissions()
            If Page.Request("OwnerId") IsNot Nothing Then
                Try
                    ddlOwners.SelectedValue = Page.Request("OwnerId")
                Catch ex As ArgumentOutOfRangeException
                    Try
                        ddlOwners.SelectedValue = _currentUser.OwnerId.ToString() ' if the user is restricted by owner then load the owner they are authorized to change
                    Catch ex2 As ArgumentOutOfRangeException
                        ddlOwners.SelectedIndex = 0
                    End Try
                End Try
            End If
            ddlOwners_SelectedIndexChanged(ddlOwners, New EventArgs())
            Utilities.SetFocus(tbxName, Me)
            Utilities.ConfirmBox(Me.btnDelete, "Are you sure you want to delete this owner?") ' setup for delete confirmation box
        End If
    End Sub

    Private Sub PopulateOwnerList(ByVal currentUser As KaUser)
        ddlOwners.Items.Clear() ' populate the owners list
        If _currentUserPermission(_currentTableName).Create Then
            If currentUser.OwnerId = Guid.Empty Then ddlOwners.Items.Add(New ListItem("Enter a new owner", Guid.Empty.ToString()))
        Else
			If currentUser.OwnerId = Guid.Empty Then ddlOwners.Items.Add(New ListItem("Select an owner", Guid.Empty.ToString()))
		End If
        For Each o As KaOwner In KaOwner.GetAll(GetUserConnection(currentUser.Id), "deleted=0" & IIf(currentUser.OwnerId = Guid.Empty, "", " AND id=" & Q(currentUser.OwnerId)), "name ASC")
            ddlOwners.Items.Add(New ListItem(o.Name, o.Id.ToString()))
        Next
        ddlOwners_SelectedIndexChanged(Nothing, Nothing)
    End Sub

    Private Sub PopulateOwnerInformation()
        Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
        Dim o As New KaOwner()
        o.Id = Guid.Parse(ddlOwners.SelectedValue)
        _customFieldData.Clear()
        If o.Id <> Guid.Empty Then
            o.SqlSelect(connection)
            If Boolean.Parse(KaSetting.GetSetting(connection, KaSetting.SN_SEPARATE_ORDER_NUMBER_PER_OWNER, KaSetting.SD_SEPARATE_ORDER_NUMBER_PER_OWNER)) Then
                tbxNextOrderNumber.Enabled = True
                btnSaveNextOrderNumber.Enabled = True
            Else
                tbxNextOrderNumber.Enabled = False
                btnSaveNextOrderNumber.Enabled = False
            End If
            If Boolean.Parse(KaSetting.GetSetting(connection, KaSetting.SN_SEPARATE_TICKET_NUMBER_PER_OWNER, KaSetting.SD_SEPARATE_TICKET_NUMBER_PER_OWNER)) Then
                tbxNextTicketNumber.Enabled = True
                btnSaveNextTicketNumber.Enabled = True
            Else
                tbxNextTicketNumber.Enabled = False
                btnSaveNextTicketNumber.Enabled = False
            End If
            tbxTicketPrefix.Enabled = Boolean.Parse(KaSetting.GetSetting(connection, KaSetting.SN_SEPARATE_TICKET_PREFIX_PER_OWNER, KaSetting.SD_SEPARATE_TICKET_PREFIX_PER_OWNER))
            tbxTicketSuffix.Enabled = Boolean.Parse(KaSetting.GetSetting(connection, KaSetting.SN_SEPARATE_TICKET_SUFFIX_PER_OWNER, KaSetting.SD_SEPARATE_TICKET_SUFFIX_PER_OWNER))

            For Each customFieldValue As KaCustomFieldData In KaCustomFieldData.GetAll(connection, String.Format("deleted = 0 AND {0} = {1}", KaCustomFieldData.FN_RECORD_ID, Q(o.Id)), KaCustomFieldData.FN_LAST_UPDATED)
                _customFieldData.Add(customFieldValue)
            Next
        Else
            tbxNextOrderNumber.Enabled = False
            btnSaveNextOrderNumber.Enabled = False
            tbxNextTicketNumber.Enabled = False
            btnSaveNextTicketNumber.Enabled = False
            tbxTicketPrefix.Enabled = False
            tbxTicketSuffix.Enabled = False
        End If
        tbxName.Text = o.Name
        tbxOwnerNumber.Text = o.Number
        tbxAddress.Text = o.Street
        tbxCity.Text = o.City
        tbxState.Text = o.State
        tbxZip.Text = o.ZipCode
        tbxCountry.Text = o.Country
        tbxPhone.Text = o.Phone
        tbxEmail.Text = o.Email
        tbxNotes.Text = o.Notes
        cbxUsePercentageToDetermineOrderCompletion.Checked = o.UsePercentToDetermineOrderCompletion
        tbxCompletionPercentage.Text = o.CompletionPercentage
        cbxUseBatchCountToDetermineOrderCompletion.Checked = o.UseBatchCountToDetermineOrderCompletion
        tbxTicketURL.Text = Reports.WebTicketUrlForOwner(connection, o.Id)
        tbxOrderSummaryUrl.Text = OrderSummaryUrlForOwner(connection, o.Id)
        tbxNextOrderNumber.Text = NextOrderNumberForOwner(o.Id, _currentUser)
        tbxNextTicketNumber.Text = NextTicketNumberForOwner(o.Id, _currentUser)
        tbxTicketPrefix.Text = KaSetting.GetSetting(connection, KaSetting.SN_TICKET_PREFIX_FOR_OWNER & o.Id.ToString, KaSetting.SD_TICKET_PREFIX_FOR_OWNER)
        tbxTicketSuffix.Text = KaSetting.GetSetting(connection, KaSetting.SN_TICKET_SUFFIX_FOR_OWNER & o.Id.ToString, KaSetting.SD_TICKET_SUFFIX_FOR_OWNER)
        cbxUsePercentageToDetermineReceivingOrderCompletion.Checked = o.UsePercentToDetermineReceivingPoCompletion
        tbxReceivingOrderCompletionPercentage.Text = o.ReceivingPoCompletionPercentage
        tbxReceivingTicketURL.Text = Reports.ReceivingPoWebTicketUrlForOwner(connection, o.Id)

        UpdateControlsEnabled()
        lblSaveNextOrderNumber.Visible = False
        lblSaveNextTicketNumber.Visible = False
        PopulateInterfaceInformation(Guid.Empty)
        Utilities.CreateDynamicCustomFieldPanelControls(_customFields, _customFieldData, lstCustomFields, Page)
    End Sub

    Private Sub ddlOwners_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ddlOwners.SelectedIndexChanged
        SetCurrentOwner()
        SetControlUsabilityFromPermissions()
    End Sub

    Private Sub SetCurrentOwner()
        lblStatus.Text = ""
        PopulateOwnerInformation()
        btnDelete.Enabled = Guid.Parse(ddlOwners.SelectedValue) <> Guid.Empty AndAlso _currentUserPermission(_currentTableName).Delete
        PopulateOwnerInterfaceList(Guid.Parse(ddlOwners.SelectedValue))
        Utilities.SetFocus(tbxName, Me)
    End Sub

    Private Sub btnDelete_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDelete.Click
        lblStatus.Text = ""
        Dim ownerId As Guid = Guid.Parse(ddlOwners.SelectedValue)
        Dim orders As ArrayList = KaOrder.GetAll(GetUserConnection(_currentUser.Id), $"{KaOrder.FN_DELETED} = 0 AND {KaOrder.FN_COMPLETED} = 0 AND {KaOrder.FN_OWNER_ID} = {Q(ownerId)}", "number ASC")
        If orders.Count > 0 Then
            Dim warning As String = "Owner is associated with other records (see below for details). Owner information not deleted.\n\nDetails:\n\nOrders:\n"
            Dim lastOrderId As Guid = Guid.Empty
            For Each order As KaOrder In orders
                warning &= New KaOrder(GetUserConnection(_currentUser.Id), order.Id).Number & " "
                lastOrderId = order.Id
            Next
            ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidProductUsed", Utilities.JsAlert(warning))
        Else
            Dim pastOrders As ArrayList = KaOrder.GetAll(GetUserConnection(_currentUser.Id), $"{KaOrder.FN_DELETED} = 0 AND {KaOrder.FN_COMPLETED} = 1 AND {KaOrder.FN_OWNER_ID} = {Q(ownerId)}", "number ASC")
			Dim accounts As ArrayList = KaCustomerAccount.GetAll(GetUserConnection(_currentUser.Id), $"{KaCustomerAccount.FN_DELETED} = 0 AND {KaCustomerAccount.FN_OWNER_ID} = {Q(ownerId)}", "name ASC")
			Dim users As ArrayList = KaUser.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND owner_id=" & Q(ownerId), "name ASC")
            Dim products As ArrayList = KaProduct.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND owner_id=" & Q(ownerId), "name ASC")
            Dim bulkProducts As ArrayList = KaBulkProduct.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND owner_id=" & Q(ownerId), "name ASC")
            If pastOrders.Count > 0 OrElse accounts.Count > 0 OrElse users.Count > 0 OrElse products.Count > 0 OrElse bulkProducts.Count > 0 Then
                pnlMain.Visible = False
                pnlOwnerDeleteWarning.Visible = True
                lblDeleteWarning.Text = "Warning: This owner is tied to other records.<br><br> Deleting this owner will remove the reference to this owner from these items:<br>"
                If pastOrders.Count > 0 Then lblDeleteWarning.Text &= "<br>Past Orders:<br>" ' add all the past orders tied to the owner to the list
                For Each r As KaOrder In pastOrders : lblDeleteWarning.Text &= r.Number & "<br>" : Next
                If accounts.Count > 0 Then lblDeleteWarning.Text &= "<br>Accounts:<br>" ' add all the accounts tied to the owner to the list
                For Each r As KaCustomerAccount In accounts : lblDeleteWarning.Text &= r.Name & "(" & r.AccountNumber & ")<br>" : Next
                If users.Count > 0 Then lblDeleteWarning.Text &= "<br>Users:<br>" ' add all the users tied to the owner to the list
                For Each r As KaUser In users : lblDeleteWarning.Text &= r.Name & "<br>" : Next
                If products.Count > 0 Then lblDeleteWarning.Text &= "<br>Products:<br>" ' add all the products that reference this owner
                For Each r As KaProduct In products : lblDeleteWarning.Text &= r.Name & "<br>" : Next
                If bulkProducts.Count > 0 Then lblDeleteWarning.Text &= "<br>Bulk Products:<br>" ' add all the bulk products that reference this owner
                For Each r As KaBulkProduct In bulkProducts : lblDeleteWarning.Text &= r.Name & "<br>" : Next
                ' add all inventory tied to the owner to the list
                lblDeleteWarning.Text &= "<br>Inventory:<br>All references to this owner in the inventory will no longer be viewable.<br><br>Are you sure you want to delete this owner?"
            Else
                Dim owner As New KaOwner(GetUserConnection(_currentUser.Id), ownerId)
                owner.Deleted = True
                owner.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                _currentUser = Utilities.GetUser(Me)
                PopulateOwnerList(_currentUser)
                ddlOwners.SelectedValue = _currentUser.OwnerId.ToString()
                SetCurrentOwner()
                lblStatus.Text = "Selected owner deleted successfully"
            End If
        End If
    End Sub

    Private Sub btnSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSave.Click
        SaveOwner()
    End Sub

    Private Sub SaveOwner()
        lblStatus.Text = ""
        If ValidateSettings() Then
            Dim insertedRecord As Boolean = False
            With New KaOwner()
                .Id = Guid.Parse(ddlOwners.SelectedValue)
                .City = tbxCity.Text.Trim
                .Email = tbxEmail.Text.Trim
                .Name = tbxName.Text.Trim
                .Number = tbxOwnerNumber.Text.Trim
                .Notes = tbxNotes.Text.Trim
                .Phone = tbxPhone.Text.Trim
                .State = tbxState.Text.Trim
                .Street = tbxAddress.Text.Trim
                .ZipCode = tbxZip.Text.Trim
                .Country = tbxCountry.Text.Trim
                .UsePercentToDetermineOrderCompletion = cbxUsePercentageToDetermineOrderCompletion.Checked
                .CompletionPercentage = Double.Parse(tbxCompletionPercentage.Text)
                .UseBatchCountToDetermineOrderCompletion = cbxUseBatchCountToDetermineOrderCompletion.Checked
                .UsePercentToDetermineReceivingPoCompletion = cbxUsePercentageToDetermineReceivingOrderCompletion.Checked
                .ReceivingPoCompletionPercentage = tbxReceivingOrderCompletionPercentage.Text
                Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
                If .Id = Guid.Empty Then
                    .SqlInsert(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                    insertedRecord = True
                Else
                    .SqlUpdate(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                End If
                If .Id = Guid.Empty Then
                    .SqlInsert(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                    insertedRecord = True
                Else
                    .SqlUpdate(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                End If

                Utilities.ConvertCustomFieldPanelToLists(_customFields, _customFieldData, lstCustomFields)
                For Each customData As KaCustomFieldData In _customFieldData
                    customData.RecordId = .Id
                    customData.SqlUpdateInsertIfNotFound(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                Next

                WebTicketUrlForOwner(connection, .Id) = tbxTicketURL.Text.Trim
                ReceivingPoWebTicketUrlForOwner(connection, .Id) = tbxReceivingTicketURL.Text.Trim
                OrderSummaryUrlForOwner(connection, .Id) = tbxOrderSummaryUrl.Text.Trim()
                KaSetting.WriteSetting(connection, KaSetting.SN_TICKET_PREFIX_FOR_OWNER & .Id.ToString, tbxTicketPrefix.Text.Trim)
                KaSetting.WriteSetting(connection, KaSetting.SN_TICKET_SUFFIX_FOR_OWNER & .Id.ToString, tbxTicketSuffix.Text.Trim)

                _currentUser = Utilities.GetUser(Me)
                PopulateOwnerList(_currentUser)
                ddlOwners.SelectedValue = .Id.ToString()
                SetCurrentOwner()
                If insertedRecord Then
                    lblStatus.Text = "New owner added successfully"
                Else
                    lblStatus.Text = "Selected owner updated successfully"
                End If
            End With
        End If
    End Sub

    Private Function ValidateSettings() As Boolean
        If tbxName.Text.Length = 0 Then
            ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidName", Utilities.JsAlert("Please enter a name for the owner"))
            Return False
        End If
        If KaOwner.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND id <> " & Q(Guid.Parse(ddlOwners.SelectedValue)) & " AND name=" & Q(tbxName.Text), "").Count > 0 Then
            ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidDuplicateName", Utilities.JsAlert("The name specified is already used for another owner. Please enter a unique name."))
            Return False
        End If
        If Not IsNumeric(tbxCompletionPercentage.Text) Then
            ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidOrderCompletionPercent", Utilities.JsAlert("Please specify a numeric value for the order completion percentage."))
            Return False
        End If
        If tbxNextTicketNumber.Text.Trim().Length = 0 AndAlso Boolean.Parse(KaSetting.GetSetting(GetUserConnection(_currentUser.Id), KaSetting.SN_SEPARATE_TICKET_NUMBER_PER_OWNER, KaSetting.SD_SEPARATE_TICKET_NUMBER_PER_OWNER)) Then
            ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidNextticketNumber", Utilities.JsAlert("Please specify the next ticket number."))
            Return False
        End If
        Dim message As String = ""
        If Not Utilities.IsEmailFieldValid(tbxEmail.Text, message) Then
            ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidEmail", Utilities.JsAlert(message))
            Return False
        End If
        Return True
    End Function

    Protected Sub btnOwnerDeleteNo_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnOwnerDeleteNo.Click
        pnlOwnerDeleteWarning.Visible = False
        pnlMain.Visible = True
        ddlOwners.SelectedValue = Utilities.GetUser(Me).OwnerId.ToString()
    End Sub

    Protected Sub btnOwnerDeleteYes_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnOwnerDeleteYes.Click
        Dim ownerId As Guid = Guid.Parse(ddlOwners.SelectedValue)
        For Each order As KaOrder In KaOrder.GetAll(GetUserConnection(_currentUser.Id), "owner_id=" & Q(ownerId), "")
            order.OwnerId = Guid.Empty
            order.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
        Next
        For Each account As KaCustomerAccount In KaCustomerAccount.GetAll(GetUserConnection(_currentUser.Id), "deleted = 0 AND owner_id=" & Q(ownerId), "")
            account.OwnerId = Guid.Empty
            account.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
        Next
        For Each usr As KaUser In KaUser.GetAll(GetUserConnection(_currentUser.Id), "owner_id=" & Q(ownerId), "")
            usr.OwnerId = Guid.Empty
            usr.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
        Next
        With New KaOwner(GetUserConnection(_currentUser.Id), ownerId)
            .Deleted = True
            .SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
        End With
        pnlOwnerDeleteWarning.Visible = False
        lblDeleteWarning.Text = ""
        pnlMain.Visible = True
        _currentUser = Utilities.GetUser(Me)
        PopulateOwnerList(_currentUser)
        ddlOwners.SelectedValue = _currentUser.OwnerId.ToString()
        SetCurrentOwner()
        lblStatus.Text = "Owner and all references data deleted successfully"
    End Sub

    Protected Sub cbxUsePercentageToDetermineOrderCompletion_CheckedChanged(sender As Object, e As EventArgs) Handles cbxUsePercentageToDetermineOrderCompletion.CheckedChanged
        UpdateControlsEnabled()
    End Sub

    Protected Sub cbxUsePercentageToDetermineReceivingOrderCompletion_CheckedChanged(sender As Object, e As EventArgs) Handles cbxUsePercentageToDetermineReceivingOrderCompletion.CheckedChanged
        UpdateControlsEnabled()
    End Sub

    Private Sub UpdateControlsEnabled()
        tbxCompletionPercentage.Enabled = cbxUsePercentageToDetermineOrderCompletion.Checked
        tbxReceivingOrderCompletionPercentage.Enabled = cbxUsePercentageToDetermineReceivingOrderCompletion.Checked
    End Sub

#Region "Interfaces"
    Private Sub PopulateInterfaceList()
        ddlInterface.Items.Clear()
        ddlInterface.Items.Add(New ListItem("", Guid.Empty.ToString()))
        Dim getInterfaceRdr As OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), "SELECT interfaces.id, interfaces.name " &
                "FROM interfaces " &
                "INNER JOIN interface_types ON interfaces.interface_type_id = interface_types.id " &
                "WHERE (interfaces.deleted = 0) " &
                    "AND (interface_types.deleted = 0) " &
                    "AND ((" & KaInterfaceTypes.FN_SHOW_OWNER_INTERFACE & " = 1) " &
                    "OR (interfaces.id IN (SELECT " & KaOwnerInterfaceSettings.TABLE_NAME & ".interface_id " &
                                "FROM " & KaOwnerInterfaceSettings.TABLE_NAME & " " &
                                "WHERE (deleted=0) " &
                                    "AND (" & KaOwnerInterfaceSettings.TABLE_NAME & "." & KaOwnerInterfaceSettings.FN_OWNER_ID & " = " & Q(ddlOwners.SelectedValue) & ") " &
                                    "AND (" & KaOwnerInterfaceSettings.TABLE_NAME & "." & KaOwnerInterfaceSettings.FN_OWNER_ID & " <> " & Q(Guid.Empty) & ")))) " &
                "ORDER BY interfaces.name")
        Do While getInterfaceRdr.Read
            ddlInterface.Items.Add(New ListItem(getInterfaceRdr.Item("name"), getInterfaceRdr.Item("id").ToString()))
        Loop
        getInterfaceRdr.Close()
        pnlInterfaceSetup.Visible = (ddlInterface.Items.Count > 1) AndAlso ddlOwners.SelectedValue <> Guid.Empty.ToString
    End Sub

    Private Sub DeleteInterfaceInformation(ByVal ownerId As Guid)
        For Each r As KaOwnerInterfaceSettings In KaOwnerInterfaceSettings.GetAll(GetUserConnection(_currentUser.Id), KaOwnerInterfaceSettings.FN_DELETED & " = 0 and " & KaOwnerInterfaceSettings.FN_OWNER_ID & " = " & Q(ownerId), "")
            r.Deleted = True
            r.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
        Next
    End Sub

    Private Sub PopulateOwnerInterfaceList(ByVal ownerId As Guid)
        PopulateInterfaceList()
        ddlOwnerInterface.Items.Clear()
        ddlOwnerInterface.Items.Add(New ListItem(IIf(ddlInterface.Items.Count > 1, "Add an interface", ""), Guid.Empty.ToString))
        Dim getInterfaceRdr As OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), "SELECT " & KaOwnerInterfaceSettings.TABLE_NAME & ".id, interfaces.name, " & KaOwnerInterfaceSettings.TABLE_NAME & ".cross_reference " &
                    "FROM " & KaOwnerInterfaceSettings.TABLE_NAME & " " &
                    "INNER JOIN interfaces ON " & KaOwnerInterfaceSettings.TABLE_NAME & ".interface_id = interfaces.id " &
                    "INNER JOIN interface_types ON interfaces.interface_type_id = interface_types.id " &
                    "WHERE (" & KaOwnerInterfaceSettings.TABLE_NAME & ".deleted = 0) " &
                        "AND (interfaces.deleted = 0) " &
                        "AND (interface_types.deleted = 0) " &
                        "AND (" & KaOwnerInterfaceSettings.TABLE_NAME & "." & KaOwnerInterfaceSettings.FN_OWNER_ID & "=" & Q(ownerId) & ") " &
                    "ORDER BY interfaces.name, " & KaOwnerInterfaceSettings.TABLE_NAME & ".cross_reference")
        Do While getInterfaceRdr.Read
            ddlOwnerInterface.Items.Add(New ListItem(getInterfaceRdr.Item("name") & " (" & getInterfaceRdr.Item("cross_reference") & ")", getInterfaceRdr.Item("id").ToString()))
        Loop
        getInterfaceRdr.Close()
    End Sub

    Private Sub PopulateInterfaceInformation(ownerInterfaceId As Guid)
        Dim settings As KaOwnerInterfaceSettings
        If ownerInterfaceId <> Guid.Empty Then ' user has selected an interface
            settings = New KaOwnerInterfaceSettings(GetUserConnection(_currentUser.Id), ownerInterfaceId)
        Else ' user has selected "Add an interface"
            settings = New KaOwnerInterfaceSettings()
        End If
        ddlInterface.SelectedValue = settings.InterfaceId.ToString
        tbxInterfaceCrossReference.Text = settings.CrossReference
        chkDefaultSetting.Checked = settings.DefaultSetting
        chkExportOnly.Checked = settings.ExportOnly
        btnRemoveInterface.Enabled = settings.Id <> Guid.Empty
    End Sub

    Protected Sub btnRemoveInterface_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnRemoveInterface.Click
        Dim id As Guid = Guid.Parse(ddlOwnerInterface.SelectedValue)
        If id <> Guid.Empty Then
            Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
            Dim settings As KaOwnerInterfaceSettings = New KaOwnerInterfaceSettings(connection, id)
            settings.Deleted = True
            settings.SqlUpdate(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
        End If
        PopulateOwnerInterfaceList(Guid.Parse(ddlOwners.SelectedValue))
        PopulateInterfaceInformation(Guid.Empty)
    End Sub

    Protected Sub btnSaveInterface_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnSaveInterface.Click
        lblStatus.Text = ""
        If Guid.Parse(ddlOwners.SelectedValue) = Guid.Empty Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidOwnerNotSaved", Utilities.JsAlert("You must save the Owner before you can set up interface cross references.")) : Exit Sub
        If Guid.Parse(ddlInterface.SelectedValue) = Guid.Empty Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidInterface", Utilities.JsAlert("An interface must be selected. Interface settings not saved.")) : Exit Sub
        If tbxInterfaceCrossReference.Text.Length = 0 Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidCrossReference", Utilities.JsAlert("A cross reference must be specified. Interface settings not saved.")) : Exit Sub

        ' If this is not export only, check if there are any other settings with the same cross reference ID
        If Not chkExportOnly.Checked Then
            Dim allInterfaceSettings As ArrayList = KaOwnerInterfaceSettings.GetAll(GetUserConnection(_currentUser.Id), KaOwnerInterfaceSettings.FN_CROSS_REFERENCE & " = " & Q(tbxInterfaceCrossReference.Text.Trim) &
                                                                                            " AND " & KaOwnerInterfaceSettings.FN_INTERFACE_ID & " = " & Q(ddlInterface.SelectedValue) &
                                                                                            " AND " & KaOwnerInterfaceSettings.FN_DELETED & " = 0 " &
                                                                                            " AND " & KaOwnerInterfaceSettings.FN_EXPORT_ONLY & " = 0 " &
                                                                                            " AND " & KaOwnerInterfaceSettings.FN_ID & " <> " & Q(ddlOwnerInterface.SelectedValue), "")
            If allInterfaceSettings.Count > 0 Then
                ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidCrossReferenceExists", Utilities.JsAlert("A cross reference of " & tbxInterfaceCrossReference.Text.Trim & " already exists for this interface."))
                Exit Sub
            End If
        End If

        Dim ownerInterfaceId As Guid = Guid.Parse(ddlOwnerInterface.SelectedValue)
        If ownerInterfaceId = Guid.Empty Then
            Dim ownerInterface As KaOwnerInterfaceSettings = New KaOwnerInterfaceSettings
            ownerInterface.OwnerId = Guid.Parse(ddlOwners.SelectedValue)
            ownerInterface.InterfaceId = Guid.Parse(ddlInterface.SelectedValue)
            ownerInterface.CrossReference = tbxInterfaceCrossReference.Text.Trim
            ownerInterface.DefaultSetting = chkDefaultSetting.Checked
            ownerInterface.ExportOnly = chkExportOnly.Checked
            ownerInterface.SqlInsert(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
            ownerInterfaceId = ownerInterface.Id
        Else
            Dim ownerInterface As KaOwnerInterfaceSettings = New KaOwnerInterfaceSettings(GetUserConnection(_currentUser.Id), ownerInterfaceId)
            ownerInterface.OwnerId = Guid.Parse(ddlOwners.SelectedValue)
            ownerInterface.InterfaceId = Guid.Parse(ddlInterface.SelectedValue)
            ownerInterface.CrossReference = tbxInterfaceCrossReference.Text.Trim
            ownerInterface.DefaultSetting = chkDefaultSetting.Checked
            ownerInterface.ExportOnly = chkExportOnly.Checked
            ownerInterface.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
        End If

        PopulateOwnerInterfaceList(Guid.Parse(ddlOwners.SelectedValue))
        ddlOwnerInterface.SelectedValue = ownerInterfaceId.ToString
        ddlOwnerInterface_SelectedIndexChanged(ddlOwnerInterface, New EventArgs)
        btnRemoveInterface.Enabled = True
    End Sub

    Protected Sub ddlOwnerInterface_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles ddlOwnerInterface.SelectedIndexChanged
        PopulateInterfaceInformation(Guid.Parse(ddlOwnerInterface.SelectedValue))
    End Sub
#End Region

    Private Sub SetTextboxMaxLengths()
        tbxAddress.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaOwner.TABLE_NAME, "street"))
        tbxCity.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaOwner.TABLE_NAME, "city"))
        tbxCompletionPercentage.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaOwner.TABLE_NAME, KaOwner.FN_COMPLETION_PERCENTAGE))
        tbxInterfaceCrossReference.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaOwnerInterfaceSettings.TABLE_NAME, KaOwnerInterfaceSettings.FN_CROSS_REFERENCE))
        tbxEmail.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaOwner.TABLE_NAME, "email"))
        tbxName.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaOwner.TABLE_NAME, "name"))
        tbxNextOrderNumber.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaOrder.TABLE_NAME, "number"))
        tbxNextTicketNumber.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaTicket.TABLE_NAME, "number"))
        tbxNotes.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaOwner.TABLE_NAME, "notes"))
        tbxOwnerNumber.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaOwner.TABLE_NAME, "owner_number"))
        tbxPhone.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaOwner.TABLE_NAME, "phone"))
        tbxState.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaOwner.TABLE_NAME, "state"))
        tbxZip.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaOwner.TABLE_NAME, "zip_code"))
        tbxCountry.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaBranch.TABLE_NAME, "country"))
    End Sub

    Protected Sub btnSaveNextOrderNumber_Click(sender As Object, e As EventArgs) Handles btnSaveNextOrderNumber.Click
        NextOrderNumberForOwner(Guid.Parse(ddlOwners.SelectedValue), _currentUser) = tbxNextOrderNumber.Text.Trim()
        lblSaveNextOrderNumber.Visible = True
    End Sub

    Protected Sub btnSaveNextTicketNumber_Click(sender As Object, e As EventArgs) Handles btnSaveNextTicketNumber.Click
        NextTicketNumberForOwner(Guid.Parse(ddlOwners.SelectedValue), _currentUser) = tbxNextTicketNumber.Text.Trim
        lblSaveNextTicketNumber.Visible = True
    End Sub

    Private Sub ddlInterface_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles ddlInterface.SelectedIndexChanged
        If Guid.Parse(ddlOwnerInterface.SelectedValue) = Guid.Empty Then
            'Only do this check if we are a new interface setting
            Dim count As Integer = 0
            Try
                Dim rdr As OleDb.OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), "SELECT count(*) " &
                                                                             "FROM " & KaOwnerInterfaceSettings.TABLE_NAME & " " &
                                                                             "WHERE " & KaOwnerInterfaceSettings.FN_DELETED & " = 0 " &
                                                                             "AND " & KaOwnerInterfaceSettings.FN_INTERFACE_ID & " = " & Q(Guid.Parse(ddlInterface.SelectedValue)) & " " &
                                                                             "AND " & KaOwnerInterfaceSettings.FN_OWNER_ID & " = " & Q(Guid.Parse(ddlOwners.SelectedValue)))
                If rdr.Read Then count = rdr.Item(0)
            Catch ex As Exception

            End Try
            chkDefaultSetting.Checked = (count = 0)
        End If
    End Sub

    Protected Overrides Function SaveViewState() As Object
        Dim viewState(2) As Object
        'Saving the grid values to the View State
        Utilities.ConvertCustomFieldPanelToLists(_customFields, _customFieldData, lstCustomFields)
        viewState(0) = MyBase.SaveViewState()
        viewState(1) = _customFields
        viewState(2) = _customFieldData
        Return viewState
    End Function

    Protected Overrides Sub LoadViewState(savedState As Object)
        'Getting the dropdown list value from view state.
        If savedState IsNot Nothing AndAlso CType(savedState, Object).Length = 3 Then
            Dim viewState As Object() = savedState
            MyBase.LoadViewState(viewState(0))
            _customFields = viewState(1)
            _customFieldData = viewState(2)
            Utilities.CreateDynamicCustomFieldPanelControls(_customFields, _customFieldData, lstCustomFields, Page)
        Else
            MyBase.LoadViewState(savedState)
        End If
    End Sub
    Private Sub SetControlUsabilityFromPermissions()
        With _currentUserPermission(_currentTableName)
            Dim shouldEnable = (.Edit AndAlso ddlOwners.SelectedIndex > 0) OrElse (.Create AndAlso ddlOwners.SelectedIndex = 0)
            pnlEven.Enabled = shouldEnable
            pnlInterfaceSettings.Enabled = shouldEnable
            btnSave.Enabled = shouldEnable
            Dim value = Guid.Parse(ddlOwners.SelectedValue)
            btnDelete.Enabled = .Edit AndAlso .Delete AndAlso value <> Guid.Empty
        End With
    End Sub
End Class