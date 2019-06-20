Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb
Imports System.IO

Public Class Inventory : Inherits System.Web.UI.Page
#Region "Events"
    Private _currentUser As KaUser
    Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
    Private _currentTableName As String = KaBulkProductInventory.TABLE_NAME
    Private _userAuthorizedChangeInventory As Boolean = False

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Response.Cache.SetCacheability(HttpCacheability.NoCache)
        _currentUser = Utilities.GetUser(Me)
        _currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Inventory")
        If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")
        _userAuthorizedChangeInventory = _currentUserPermission(_currentTableName).Edit

        If Not Page.IsPostBack Then
            SetTextboxMaxLengths()
            PopulateBulkProductList()
            PopulateLocationList()
            PopulateOwnerList()
            PopulateUnitList()
            ddl_SelectedIndexChanged(Nothing, Nothing) ' let the user know that they need to click "Show inventory"
            btnShowInventory.Attributes.Add("onclick", "pleaseWait();")
            Utilities.ConfirmBox(Me.btnResetDispensed, "Are you sure that you want to zero the dispensed quantity?") ' Delete confirmation box setup
            PopulateEmailAddressList()
            UpdateButtons(Nothing, Nothing)
            PopulateInitialOptions()
        End If
        litEmailConfirmation.Text = ""
    End Sub

    Protected Sub btnAdjustInventoryApply_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnAdjustInventoryApply.Click
        Dim adjustmentAmount As Double = 0.0
        If Double.TryParse(txtAdjustInventory.Text, adjustmentAmount) Then
            AdjustInventory(adjustmentAmount, txtNotes.Text)
            txtAdjustInventory.Text = ""
            txtNotes.Text = ""
            PopulateInventoryTable()
        Else
            ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidAdjustAmount", Utilities.JsAlert("Adjust amount must be numeric."))
        End If
    End Sub

    Protected Sub btnResetDispensed_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnResetDispensed.Click
        Dim bulkProductId As Guid = Guid.Parse(ddlAdjustBulkProduct.SelectedValue)
        Dim ownerId As Guid = Guid.Parse(ddlAdjustOwner.SelectedValue)
        Dim locationId As Guid = Guid.Parse(ddlAdjustLocation.SelectedValue)
        Dim unitId As Guid = Guid.Parse(ddlAdjustInventoryUnit.SelectedValue)
        Dim connection As New OleDbConnection(Tm2Database.GetDbConnection())
        Dim transaction As OleDbTransaction = Nothing
        Try
            connection.Open()
            transaction = connection.BeginTransaction()
            KaBulkProductInventory.ResetDispensed(connection, transaction, Database.ApplicationIdentifier, _currentUser.Name, bulkProductId, locationId, ownerId, "Reset dispensed quantity", Guid.Empty, "", New Dictionary(Of String, Object))
            transaction.Commit()
            PopulateInventoryTable()
        Catch ex As Exception
            If transaction IsNot Nothing AndAlso Not connection.State = ConnectionState.Open Then transaction.Rollback()
            ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidResetDispensed", Utilities.JsAlert("Unable to reset the dispensed amount. " & vbCrLf & ex.Message))
        Finally
            If transaction IsNot Nothing AndAlso Not connection.State = ConnectionState.Open Then transaction.Dispose()
            connection.Close()
        End Try

        PopulateInventoryTable()
    End Sub

    Protected Sub btnChangeUnitOfMeasureApply_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnChangeUnitOfMeasureApply.Click
        Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
        Dim bulkProduct As New KaBulkProduct(connection, Guid.Parse(ddlAdjustBulkProduct.SelectedValue))
        Dim ownerId As Guid = Guid.Parse(ddlAdjustOwner.SelectedValue)
        Dim locationId As Guid = Guid.Parse(ddlAdjustLocation.SelectedValue)

        Try
            With KaBulkProductInventory.GetBulkProductInventory(connection, Nothing, bulkProduct.Id, locationId, ownerId, bulkProduct.DefaultUnitId, Database.ApplicationIdentifier, _currentUser.Name)
                Dim oldUnit As New KaUnit(connection, .UnitId)
                Dim newUnit As New KaUnit(connection, Guid.Parse(ddlUnitOfMeasure.SelectedValue))
                If cbxConvert.Checked Then
                    If Not UnitsCompatible(.UnitId, newUnit.Id, bulkProduct.Density, bulkProduct.WeightUnitId, bulkProduct.VolumeUnitId) Then
                        ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidConversionError", Utilities.JsAlert("Unable to convert the inventory and dispensed quantities. Units are not compatible."))
                        Exit Try
                    End If
                    Dim weightUnit As KaUnit : If bulkProduct.WeightUnitId <> Guid.Empty Then weightUnit = New KaUnit(connection, bulkProduct.WeightUnitId) Else weightUnit = New KaUnit()
                    Dim volumeUnit As KaUnit : If bulkProduct.VolumeUnitId <> Guid.Empty Then volumeUnit = New KaUnit(connection, bulkProduct.VolumeUnitId) Else volumeUnit = New KaUnit()
                    .Inventory = KaUnit.Convert(.Inventory, oldUnit, newUnit, bulkProduct.Density, weightUnit, volumeUnit)
                    .Dispensed = KaUnit.Convert(.Dispensed, oldUnit, newUnit, bulkProduct.Density, weightUnit, volumeUnit)
                End If
                .UnitId = Guid.Parse(ddlUnitOfMeasure.SelectedValue)
                .SqlUpdateInsertIfNotFound(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
            End With
        Catch ex As Exception
            ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidConversionError", Utilities.JsAlert("Unable to convert the inventory and dispensed quantities.  " & ex.Message & ""))
        End Try
        PopulateInventoryTable()
    End Sub

    Protected Sub ddl_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles ddlOwner.SelectedIndexChanged, ddlLocation.SelectedIndexChanged, ddlBulkProduct.SelectedIndexChanged, cbxOnlyShowBulkProductsWithNonZeroInventory.CheckedChanged, cbxAssignPhysicalInventoryToOwner.CheckedChanged
        litFiltersChanged.Text = "<span style=""color: red;"">Click ""Show inventory"" to refresh table</span>"
    End Sub

    Protected Sub btnShowInventory_Click(sender As Object, e As EventArgs) Handles btnShowInventory.Click
        litFiltersChanged.Text = ""
        Try
            ddlAdjustOwner.SelectedValue = ddlOwner.SelectedValue
        Catch ex As Exception
            'The item didn't exist in both lists
        End Try
        Try
            ddlAdjustLocation.SelectedValue = ddlLocation.SelectedValue
        Catch ex As Exception
            'The item didn't exist in both lists
        End Try
        Try
            ddlAdjustBulkProduct.SelectedValue = ddlBulkProduct.SelectedValue
        Catch ex As Exception
            'The item didn't exist in both lists
        End Try
        PopulateInventoryTable()
        UpdateButtons(Nothing, Nothing)
        SaveOptionsSelected()
    End Sub
#End Region

    Private Function UnitsCompatible(ByVal unit1Id As Guid, ByVal unit2Id As Guid, ByVal density As Double, ByVal weightUnitId As Guid, ByVal volumeUnitId As Guid) As Boolean
        Return unit1Id <> Guid.Empty AndAlso
               unit2Id <> Guid.Empty AndAlso
               (Not (KaUnit.IsWeight(New KaUnit(GetUserConnection(_currentUser.Id), unit1Id).BaseUnit) Xor KaUnit.IsWeight(New KaUnit(GetUserConnection(_currentUser.Id), unit2Id).BaseUnit)) OrElse
                (density > 0 AndAlso weightUnitId <> Guid.Empty AndAlso volumeUnitId <> Guid.Empty))
    End Function

    Private Sub PopulateBulkProductList()
        ddlBulkProduct.Items.Clear()
        ddlAdjustBulkProduct.Items.Clear()
        ddlBulkProduct.Items.Add(New ListItem("All bulk products", Guid.Empty.ToString()))
        ddlAdjustBulkProduct.Items.Add(New ListItem("Select a bulk product", Guid.Empty.ToString()))
        Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
        Dim rdr As OleDbDataReader = Tm2Database.ExecuteReader(connection, "SELECT id, name, 'B' AS recordType, UPPER(name) AS upperName FROM bulk_products WHERE deleted = 0 UNION SELECT id, name, 'G' AS recordType, UPPER(name) AS upperName FROM inventory_groups WHERE deleted = 0 ORDER BY recordType, upperName")
        Do While rdr.Read
            If rdr.Item("recordType") = "B" Then
                Dim r As New KaBulkProduct(connection, rdr.Item("id"))
                If Not r.IsFunction(connection) Then
                    ddlBulkProduct.Items.Add(New ListItem(r.Name, r.Id.ToString()))
                    ddlAdjustBulkProduct.Items.Add(New ListItem(r.Name, r.Id.ToString()))
                End If
            ElseIf rdr.Item("recordType") = "G" Then
                ddlBulkProduct.Items.Add(New ListItem(rdr.Item("name") & " (Grouped inventory)", rdr.Item("id").ToString()))
            End If
        Loop
    End Sub

    Private Sub PopulateLocationList()
        ddlLocation.Items.Clear()
        ddlTransferToLocation.Items.Clear()
        ddlAdjustLocation.Items.Clear()
        ddlLocation.Items.Add(New ListItem("All facilities", Guid.Empty.ToString()))
        ddlAdjustLocation.Items.Add(New ListItem("Select a facility", Guid.Empty.ToString()))
        For Each r As KaLocation In KaLocation.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
            ddlLocation.Items.Add(New ListItem(r.Name, r.Id.ToString()))
            ddlTransferToLocation.Items.Add(New ListItem(r.Name, r.Id.ToString()))
            ddlAdjustLocation.Items.Add(New ListItem(r.Name, r.Id.ToString()))
        Next
    End Sub

    Private Sub PopulateOwnerList()
        ddlOwner.Items.Clear()
        ddlTransferToOwner.Items.Clear()
        ddlAdjustOwner.Items.Clear()
        If _currentUser.OwnerId = Guid.Empty Then ddlOwner.Items.Add(New ListItem("All owners", Guid.Empty.ToString()))
        ddlAdjustOwner.Items.Add(New ListItem("Select an owner", Guid.Empty.ToString()))
        For Each r As KaOwner In KaOwner.GetAll(GetUserConnection(_currentUser.Id), "deleted=0" & IIf(_currentUser.OwnerId = Guid.Empty, "", " AND id=" & Q(_currentUser.OwnerId)), "name ASC")
            ddlOwner.Items.Add(New ListItem(r.Name, r.Id.ToString()))
            ddlTransferToOwner.Items.Add(New ListItem(r.Name, r.Id.ToString()))
            ddlAdjustOwner.Items.Add(New ListItem(r.Name, r.Id.ToString()))
        Next
    End Sub

    Private Sub PopulateUnitList()
        ddlUnitOfMeasure.Items.Clear()
        ddlTransferUnit.Items.Clear()
        ddlAdjustInventoryUnit.Items.Clear()
        cblAdditionalUnits.Items.Clear()
        For Each r As KaUnit In KaUnit.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "abbreviation ASC")
            If Not KaUnit.IsTime(r.BaseUnit) AndAlso r.BaseUnit <> KaUnit.Unit.Pulses Then
                ddlUnitOfMeasure.Items.Add(New ListItem(r.Abbreviation, r.Id.ToString()))
                ddlTransferUnit.Items.Add(New ListItem(r.Abbreviation, r.Id.ToString()))
                ddlAdjustInventoryUnit.Items.Add(New ListItem(r.Abbreviation, r.Id.ToString()))
                cblAdditionalUnits.Items.Add(New ListItem(r.Name, r.Id.ToString()))
                cblAdditionalUnits.Items(cblAdditionalUnits.Items.Count - 1).Selected = False
            End If
        Next
    End Sub

    Private Sub PopulateInventoryTable()
        Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
        Dim ownerId As Guid = Guid.Parse(ddlOwner.SelectedValue)
        Dim locationId As Guid = Guid.Parse(ddlLocation.SelectedValue)
        Dim bulkProductId As Guid = Guid.Parse(ddlBulkProduct.SelectedValue)
        Dim tableAttributes As String = "border=1; width=100%;"
        Dim headerRowAttributes As String = ""
        Dim rowAttributes As String = ""

        Dim bulkProductIds As New List(Of Guid)
        If Not bulkProductId.Equals(Guid.Empty) Then bulkProductIds.Add(bulkProductId)

        Dim url As String = "InventoryReportView.aspx?OnlyNonzero=" & cbxOnlyShowBulkProductsWithNonZeroInventory.Checked & "&AssignPhysicalByOwner=" & cbxAssignPhysicalInventoryToOwner.Checked
        If Utilities.ConvertWebPageUrlDomainToRequestedPagesDomain(GetUserConnection(_currentUser.Id)) Then url = Tm2Database.GetUrlInCurrentDomain(Tm2Database.GetCurrentPageUrlWithOriginalPort(Me.Request), "http://localhost/TerminalManagement2/" & url)

        Dim containerInventoryUrl As String = "ContainerInventoryPFV.aspx" '?OwnerId=" & ddlOwner.SelectedValue & "&LocationId=" & ddlLocation.SelectedValue & "&BulkProductId=" & ddlBulkProduct.SelectedValue & "&OnlyNonzero=" & cbxOnlyShowBulkProductsWithNonZeroInventory.Checked
        'Dim additionalUnitsString As String = GetAdditionalUnitsString()
        'If additionalUnitsString.Trim.Length > 0 Then containerInventoryUrl &= "&AdditionalUnitsString=" & additionalUnitsString
        If Utilities.ConvertWebPageUrlDomainToRequestedPagesDomain(GetUserConnection(_currentUser.Id)) Then containerInventoryUrl = Tm2Database.GetUrlInCurrentDomain(Tm2Database.GetCurrentPageUrlWithOriginalPort(Me.Request), "http://localhost/TerminalManagement2/" & containerInventoryUrl)

        Dim reportDataList As ArrayList = KaReports.GetInventoryTable(connection, ownerId, locationId, bulkProductIds, cbxOnlyShowBulkProductsWithNonZeroInventory.Checked, KaReports.MEDIA_TYPE_HTML, cbxAssignPhysicalInventoryToOwner.Checked, url, GetAdditionalUnits(), containerInventoryUrl)
        Dim headerRowList As ArrayList = reportDataList(0) 'This will be the header row

        Dim headerCellAttributeList As New List(Of String)
        Dim detailCellAttributeList As New List(Of String)
        Dim columnCount As Integer = 0
        For i = 0 To 2
            headerCellAttributeList.Add("style=""font-weight: bold; text-align: left;""")
            detailCellAttributeList.Add("style=""text-align: left;""")
            columnCount += 1
        Next

        For i = 3 To headerRowList.Count - 1
            headerCellAttributeList.Add("style=""font-weight: bold; text-align: right;""")
            detailCellAttributeList.Add("style=""text-align: right;""")
            columnCount += 1
        Next

        litInventory.Text = KaReports.GetTableHtml("", "", reportDataList, False, tableAttributes, headerRowAttributes, headerCellAttributeList, rowAttributes, detailCellAttributeList)

        Dim defaultUnitId As Guid
        Dim list As ArrayList = KaBulkProductInventory.GetAll(connection, String.Format("deleted=0 AND bulk_product_id='{0}' AND owner_id='{1}' AND location_id='{2}'", ddlBulkProduct.SelectedValue, ddlOwner.SelectedValue, ddlLocation.SelectedValue), "last_updated DESC")
        If list.Count > 0 Then
            defaultUnitId = CType(list(0), KaBulkProductInventory).UnitId
        Else
            defaultUnitId = KaUnit.GetSystemDefaultMassUnitOfMeasure(Tm2Database.Connection, Nothing)
        End If

        Try
            ddlUnitOfMeasure.SelectedValue = defaultUnitId.ToString()
        Catch ex As ArgumentOutOfRangeException
            ddlUnitOfMeasure.SelectedIndex = 0
        End Try
        Try
            ddlTransferUnit.SelectedValue = defaultUnitId.ToString()
        Catch ex As ArgumentOutOfRangeException
            ddlTransferUnit.SelectedIndex = 0
        End Try
        Try
            ddlAdjustInventoryUnit.SelectedValue = defaultUnitId.ToString()
        Catch ex As ArgumentOutOfRangeException
            ddlAdjustInventoryUnit.SelectedIndex = 0
        End Try

        cbxConvert.Checked = False
        tbxTransferNotes.Text = ""
        tbxTransferQuantity.Text = ""
        txtAdjustInventory.Text = ""
        txtNotes.Text = ""
    End Sub

    Private Sub UpdateButtons(sender As Object, e As EventArgs) Handles ddlAdjustOwner.SelectedIndexChanged, ddlAdjustLocation.SelectedIndexChanged, ddlAdjustBulkProduct.SelectedIndexChanged
        Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
        Dim ownerId As Guid = Guid.Parse(ddlAdjustOwner.SelectedValue)
        Dim locationId As Guid = Guid.Parse(ddlAdjustLocation.SelectedValue)
        Dim bulkProductId As Guid = Guid.Parse(ddlAdjustBulkProduct.SelectedValue)

        Dim enabled As Boolean = ownerId <> Guid.Empty AndAlso locationId <> Guid.Empty AndAlso bulkProductId <> Guid.Empty
        pnlInventoryButtons.Visible = enabled AndAlso _userAuthorizedChangeInventory
    End Sub

    Private Sub AdjustInventory(ByVal change As Double, ByVal notes As String)
        Dim bulkProductId As Guid = Guid.Parse(ddlAdjustBulkProduct.SelectedValue)
        Dim ownerId As Guid = Guid.Parse(ddlAdjustOwner.SelectedValue)
        Dim locationId As Guid = Guid.Parse(ddlAdjustLocation.SelectedValue)
        Dim unitId As Guid = Guid.Parse(ddlAdjustInventoryUnit.SelectedValue)
        Dim connection As New OleDbConnection(Tm2Database.GetDbConnection())
        Dim transaction As OleDbTransaction = Nothing
        Try
            connection.Open()
            transaction = connection.BeginTransaction()
            KaBulkProductInventory.AdjustInventory(connection, transaction, Database.ApplicationIdentifier, _currentUser.Name, New KaQuantity(change, unitId), bulkProductId, locationId, ownerId, notes, Guid.Empty, "", New Dictionary(Of String, Object))
            transaction.Commit()
            PopulateInventoryTable()
        Catch ex As Exception
            If transaction IsNot Nothing AndAlso Not connection.State = ConnectionState.Open Then transaction.Rollback()
            ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidAdjustmentAmount", Utilities.JsAlert("Unable to adjust the quantity. " & vbCrLf & ex.Message))
        Finally
            If transaction IsNot Nothing AndAlso Not connection.State = ConnectionState.Open Then transaction.Dispose()
            connection.Close()
        End Try
    End Sub

    Protected Sub btnApplyTransfer_Click(sender As Object, e As EventArgs) Handles btnApplyTransfer.Click
        Dim connection As New OleDbConnection(Tm2Database.GetDbConnection())
        Dim transaction As OleDbTransaction = Nothing
        Try
            connection.Open()
            transaction = connection.BeginTransaction()
            Dim bulkProduct As New KaBulkProduct(connection, Guid.Parse(ddlAdjustBulkProduct.SelectedValue), transaction)
            Dim sourceOwner As New KaOwner(connection, Guid.Parse(ddlAdjustOwner.SelectedValue), transaction)
            Dim destinationOwner As New KaOwner(connection, Guid.Parse(ddlTransferToOwner.SelectedValue), transaction)
            Dim sourceLocation As New KaLocation(connection, Guid.Parse(ddlAdjustLocation.SelectedValue), transaction)
            Dim destinationLocation As New KaLocation(connection, Guid.Parse(ddlTransferToLocation.SelectedValue), transaction)
            Dim notes As String = String.Format("Transfer from {0} at {1} to {2} at {3}", sourceOwner.Name, sourceLocation.Name, destinationOwner.Name, destinationLocation.Name) & " " & tbxTransferNotes.Text
            KaBulkProductInventory.TransferInventory(connection, transaction, Database.ApplicationIdentifier, _currentUser.Name, New KaQuantity(Double.Parse(tbxTransferQuantity.Text), Guid.Parse(ddlTransferUnit.SelectedValue)), bulkProduct.Id, sourceLocation.Id, sourceOwner.Id, bulkProduct.Id, destinationLocation.Id, destinationOwner.Id, notes, Guid.Empty, "", New Dictionary(Of String, Object))

            transaction.Commit()
            PopulateInventoryTable()
        Catch ex As Exception
            If transaction IsNot Nothing AndAlso Not connection.State = ConnectionState.Open Then transaction.Rollback()
            ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidCharTransferQty", Utilities.JsAlert("Unable to transfer the quantity. " & ex.Message))
        Finally
            If transaction IsNot Nothing AndAlso Not connection.State = ConnectionState.Open Then transaction.Dispose()
            connection.Close()
        End Try
    End Sub

    Private Sub SetTextboxMaxLengths()
        tbxTransferNotes.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaInventoryChange.TABLE_NAME, "notes"))
        tbxTransferQuantity.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaInventoryChange.TABLE_NAME, "change"))
    End Sub

    Protected Sub btnPrinterFriendly_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnPrinterFriendly.Click
        SaveOptionsSelected()
        ClientScript.RegisterClientScriptBlock(Me.GetType(), "ShowReport", Utilities.JsWindowOpen("InventoryReportView.aspx?OwnerId=" & ddlOwner.SelectedValue & "&LocationId=" & ddlLocation.SelectedValue & "&BulkProductId=" & ddlBulkProduct.SelectedValue & "&OnlyNonzero=" & cbxOnlyShowBulkProductsWithNonZeroInventory.Checked & "&AssignPhysicalByOwner=" & cbxAssignPhysicalInventoryToOwner.Checked & "&AdditionalUnitsString=" & GetAdditionalUnitsString()))
    End Sub

    Protected Sub btnDownload_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnDownload.Click
        SaveOptionsSelected()
        Dim fileName As String = String.Format("Inventory{0:yyyyMMddHHmmss}.csv", Now)
        Dim ownerId As Guid = Guid.Parse(ddlOwner.SelectedValue)
        Dim locationId As Guid = Guid.Parse(ddlLocation.SelectedValue)
        Dim bulkProductId As Guid = Guid.Parse(ddlBulkProduct.SelectedValue)
        Dim bulkProductIds As New List(Of Guid)
        If Not bulkProductId.Equals(Guid.Empty) Then
            bulkProductIds.Add(bulkProductId)
        End If

        KaReports.CreateCsvFromTable("Inventory", KaReports.GetInventoryTable(GetUserConnection(_currentUser.Id), ownerId, locationId, bulkProductIds, cbxOnlyShowBulkProductsWithNonZeroInventory.Checked, KaReports.MEDIA_TYPE_COMMA, cbxAssignPhysicalInventoryToOwner.Checked, "", GetAdditionalUnits(), ""), DownloadDirectory(Me) & fileName)
        ClientScript.RegisterClientScriptBlock(Me.GetType(), "DownloadReport", Utilities.JsWindowOpen("./download/" & fileName))
    End Sub

    Protected Sub btnSendEmail_Click(sender As Object, e As EventArgs) Handles btnSendEmail.Click
        Dim message As String = ""
        If Not Utilities.IsEmailFieldValid(tbxEmailTo.Text, message) Then
            ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidEmail", Utilities.JsAlert(message))
            Exit Sub
        End If
        Dim emailAddresses As String = ""
        If tbxEmailTo.Text.Trim().Length > 0 Then
            SaveOptionsSelected()
            Dim ownerId As Guid = Guid.Parse(ddlOwner.SelectedValue)
            Dim locationId As Guid = Guid.Parse(ddlLocation.SelectedValue)
            Dim bulkProductId As Guid = Guid.Parse(ddlBulkProduct.SelectedValue)
            Dim bulkProductIds As New List(Of Guid)
            If Not bulkProductId.Equals(Guid.Empty) Then
                bulkProductIds.Add(bulkProductId)
            End If
            Dim header As String = "Inventory"

            Dim body As String = KaReports.GetTableHtml(header, KaReports.GetInventoryTable(GetUserConnection(_currentUser.Id), ownerId, locationId, bulkProductIds, cbxOnlyShowBulkProductsWithNonZeroInventory.Checked, KaReports.MEDIA_TYPE_PFV, cbxAssignPhysicalInventoryToOwner.Checked, "", GetAdditionalUnits(), ""))

            Dim emailTo() As String = tbxEmailTo.Text.Split(New Char() {";", ","})
            For Each emailRecipient As String In emailTo
                If emailRecipient.Trim.Length > 0 Then
                    Dim newEmail As New KaEmail()
                    newEmail.ApplicationId = APPLICATION_ID
                    newEmail.Body = Utilities.CreateSiteCssStyle() & body
                    newEmail.BodyIsHtml = True
                    newEmail.OwnerID = _currentUser.OwnerId
                    newEmail.Recipients = emailRecipient.Trim
                    newEmail.ReportType = KaEmailReport.ReportTypes.Inventory
                    newEmail.Subject = header
                    Dim attachments As New List(Of System.Net.Mail.Attachment)
                    attachments.Add(New System.Net.Mail.Attachment(New IO.MemoryStream(Encoding.UTF8.GetBytes(newEmail.Body)), "InventoryReport.html", System.Net.Mime.MediaTypeNames.Text.Html))
                    newEmail.SerializeAttachments(attachments)
                    KaEmail.CreateEmail(GetUserConnection(_currentUser.Id), newEmail, -1, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                    If emailAddresses.Length > 0 Then emailAddresses &= ", "
                    emailAddresses &= newEmail.Recipients
                End If
            Next
        End If
        If emailAddresses.Length > 0 Then
            litEmailConfirmation.Text = "Report sent to " & emailAddresses
        Else
            litEmailConfirmation.Text = "Report not sent.  No e-mail addresses."
        End If
    End Sub

    Private Sub PopulateEmailAddressList()
        Utilities.PopulateEmailAddressList(tbxEmailTo, ddlAddEmailAddress, btnAddEmailAddress)
        rowAddAddress.Visible = ddlAddEmailAddress.Items.Count > 1
    End Sub

    Protected Sub btnAddEmailAddress_Click(sender As Object, e As EventArgs) Handles btnAddEmailAddress.Click
        If ddlAddEmailAddress.SelectedIndex > 0 Then
            If tbxEmailTo.Text.Trim.Length > 0 Then tbxEmailTo.Text &= ", "
            tbxEmailTo.Text &= ddlAddEmailAddress.SelectedValue
            PopulateEmailAddressList()
        End If
    End Sub

    Private Sub tbxEmailTo_TextChanged(sender As Object, e As System.EventArgs) Handles tbxEmailTo.TextChanged
        PopulateEmailAddressList()
    End Sub

    Private Sub PopulateInitialOptions()
        Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
        Try
            ddlOwner.SelectedValue = KaSetting.GetSetting(connection, "InventoryReport:" & _currentUser.Id.ToString & "/LastUsedOwner", ddlOwner.SelectedValue)
        Catch ex As Exception
        End Try
        Try
            ddlLocation.SelectedValue = KaSetting.GetSetting(connection, "InventoryReport:" & _currentUser.Id.ToString & "/LastUsedLocation", ddlLocation.SelectedValue)
        Catch ex As Exception
        End Try
        Try
            ddlBulkProduct.SelectedValue = KaSetting.GetSetting(connection, "InventoryReport:" & _currentUser.Id.ToString & "/LastUsedBulkProduct", ddlBulkProduct.SelectedValue)
        Catch ex As Exception
        End Try
        Boolean.TryParse(KaSetting.GetSetting(connection, "InventoryReport:" & _currentUser.Id.ToString & "/LastUsedOnlyShowBulkProductsWithNonZeroInventory", True), cbxOnlyShowBulkProductsWithNonZeroInventory.Checked)
        Boolean.TryParse(KaSetting.GetSetting(connection, "InventoryReport:" & _currentUser.Id.ToString & "/LastUsedAssignPhysicalInventoryToOwner", True), cbxAssignPhysicalInventoryToOwner.Checked)

        Dim additionalUnitsString As String = KaSetting.GetSetting(connection, "InventoryReport:" & _currentUser.Id.ToString & "/AdditionalUnits", "")
        For Each li As ListItem In cblAdditionalUnits.Items
            li.Selected = additionalUnitsString.Contains(li.Value)
        Next
    End Sub

    Private Sub SaveOptionsSelected()
        Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
        KaSetting.WriteSetting(connection, "InventoryReport:" & _currentUser.Id.ToString & "/LastUsedOwner", ddlOwner.SelectedValue)
        KaSetting.WriteSetting(connection, "InventoryReport:" & _currentUser.Id.ToString & "/LastUsedLocation", ddlLocation.SelectedValue)
        KaSetting.WriteSetting(connection, "InventoryReport:" & _currentUser.Id.ToString & "/LastUsedBulkProduct", ddlBulkProduct.SelectedValue)
        KaSetting.WriteSetting(connection, "InventoryReport:" & _currentUser.Id.ToString & "/LastUsedOnlyShowBulkProductsWithNonZeroInventory", cbxOnlyShowBulkProductsWithNonZeroInventory.Checked)
        KaSetting.WriteSetting(connection, "InventoryReport:" & _currentUser.Id.ToString & "/LastUsedAssignPhysicalInventoryToOwner", cbxAssignPhysicalInventoryToOwner.Checked)
        KaSetting.WriteSetting(connection, "InventoryReport:" & _currentUser.Id.ToString & "/AdditionalUnits", GetAdditionalUnitsString())
    End Sub

    Private Function GetAdditionalUnitsString() As String
        Dim retval As String = ""
        For Each li As ListItem In cblAdditionalUnits.Items
            If li.Selected Then
                retval &= IIf(retval.Trim.Length > 0, ",", "") & li.Value.ToString()
            End If
        Next
        Return retval
    End Function

    Private Function GetAdditionalUnits() As List(Of Guid)
        Dim retval As List(Of Guid) = New List(Of Guid)
        For Each li As ListItem In cblAdditionalUnits.Items
            If li.Selected Then
                retval.Add(Guid.Parse(li.Value))
            End If
        Next
        Return retval
    End Function
End Class
