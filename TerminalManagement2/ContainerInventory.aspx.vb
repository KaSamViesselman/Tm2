Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb
Public Class ContainerInventory
	Inherits System.Web.UI.Page

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
			PopulateBulkProductList()
			PopulateLocationList()
			PopulateOwnerList()
			PopulateAdditionalUnitsList()
			ddl_SelectedIndexChanged(Nothing, Nothing) ' let the user know that they need to click "Show inventory"
			btnShowInventory.Attributes.Add("onclick", "pleaseWait();")
			PopulateEmailAddressList()
			PopulateInitialOptions()
		End If
		litEmailConfirmation.Text = ""
	End Sub

	Protected Sub ddl_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles ddlOwner.SelectedIndexChanged, ddlLocation.SelectedIndexChanged, ddlBulkProduct.SelectedIndexChanged, ddlCurrentStatus.SelectedIndexChanged, cbxOnlyShowBulkProductsWithNonZeroInventory.CheckedChanged
		litFiltersChanged.Text = "<span style=""color: red;"">Click ""Show inventory"" to refresh table</span>"
	End Sub

	Protected Sub btnShowInventory_Click(sender As Object, e As EventArgs) Handles btnShowInventory.Click
		litFiltersChanged.Text = ""
		PopulateInventoryTable()
		SaveOptionsSelected()
	End Sub
#End Region

	Private Sub PopulateBulkProductList()
		ddlBulkProduct.Items.Clear()
		ddlBulkProduct.Items.Add(New ListItem("All bulk products", Guid.Empty.ToString()))
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim rdr As OleDbDataReader = Tm2Database.ExecuteReader(connection, "SELECT id, name, 'B' AS recordType, UPPER(name) AS upperName FROM bulk_products WHERE deleted = 0 UNION SELECT id, name, 'G' AS recordType, UPPER(name) AS upperName FROM inventory_groups WHERE deleted = 0 ORDER BY recordType, upperName")
		Do While rdr.Read
			If rdr.Item("recordType") = "B" Then
				Dim r As New KaBulkProduct(connection, rdr.Item("id"))
				If Not r.IsFunction(connection) Then
					ddlBulkProduct.Items.Add(New ListItem(r.Name, r.Id.ToString()))
				End If
			ElseIf rdr.Item("recordType") = "G" Then
				ddlBulkProduct.Items.Add(New ListItem(rdr.Item("name") & " (Grouped inventory)", rdr.Item("id").ToString()))
			End If
		Loop
	End Sub

	Private Sub PopulateLocationList()
		ddlLocation.Items.Clear()
		ddlLocation.Items.Add(New ListItem("All facilities", Guid.Empty.ToString()))
		Dim packagedInventoryLocationId As Guid = Utilities.GetContainerPackagedInventoryLocationId(GetUserConnection(_currentUser.Id))
		For Each r As KaLocation In KaLocation.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
			If Not r.Id.Equals(packagedInventoryLocationId) Then ddlLocation.Items.Add(New ListItem(r.Name, r.Id.ToString()))
		Next
	End Sub

	Private Sub PopulateOwnerList()
		ddlOwner.Items.Clear()
		If _currentUser.OwnerId = Guid.Empty Then ddlOwner.Items.Add(New ListItem("All owners", Guid.Empty.ToString()))
		For Each r As KaOwner In KaOwner.GetAll(GetUserConnection(_currentUser.Id), "deleted=0" & IIf(_currentUser.OwnerId = Guid.Empty, "", " AND id=" & Q(_currentUser.OwnerId)), "name ASC")
			ddlOwner.Items.Add(New ListItem(r.Name, r.Id.ToString()))
		Next
	End Sub

	Private Sub PopulateAdditionalUnitsList()
		cblAdditionalUnits.Items.Clear()
		For Each n As KaUnit In KaUnit.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
			If n.BaseUnit <> KaUnit.Unit.Pulses AndAlso n.BaseUnit <> KaUnit.Unit.Seconds Then
				cblAdditionalUnits.Items.Add(New ListItem(n.Name, n.Id.ToString()))
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

		Dim containerInventoryUrl As String = "ContainerInventoryPFV.aspx" '?OwnerId=" & ddlOwner.SelectedValue & "&LocationId=" & ddlLocation.SelectedValue & "&BulkProductId=" & ddlBulkProduct.SelectedValue & "&OnlyNonzero=" & cbxOnlyShowBulkProductsWithNonZeroInventory.Checked
        'Dim additionalUnitsString As String = GetAdditionalUnitsString()
        'If additionalUnitsString.Trim.Length > 0 Then containerInventoryUrl &= "&AdditionalUnitsString=" & additionalUnitsString
        If Utilities.ConvertWebPageUrlDomainToRequestedPagesDomain(GetUserConnection(_currentUser.Id)) Then containerInventoryUrl = Tm2Database.GetUrlInCurrentDomain(Tm2Database.GetCurrentPageUrlWithOriginalPort(Me.Request), "http://localhost/TerminalManagement2/" & containerInventoryUrl)

        Dim reportDataList As ArrayList = KaReports.GetContainerInventoryTable(connection, ownerId, locationId, bulkProductIds, cbxOnlyShowBulkProductsWithNonZeroInventory.Checked, KaReports.MEDIA_TYPE_HTML, containerInventoryUrl, ddlCurrentStatus.SelectedIndex - 1, GetAdditionalUnits())
		Dim headerRowList As ArrayList = reportDataList(0) 'This will be the header row

		Dim headerCellAttributeList As New List(Of String)
		Dim detailCellAttributeList As New List(Of String)
		Dim columnCount As Integer = 0
		For i = 0 To 3
			headerCellAttributeList.Add("style=""font-weight: bold; text-align: left;""")
			detailCellAttributeList.Add("style=""text-align: left;""")
			columnCount += 1
		Next

		For i = 4 To headerRowList.Count - 1
			headerCellAttributeList.Add("style=""font-weight: bold; text-align: right;""")
			detailCellAttributeList.Add("style=""text-align: right;""")
			columnCount += 1
		Next

		litInventory.Text = KaReports.GetTableHtml("", "", reportDataList, False, tableAttributes, headerRowAttributes, headerCellAttributeList, rowAttributes, detailCellAttributeList)
	End Sub

	Private Sub SetTextboxMaxLengths()

	End Sub

	Protected Sub btnPrinterFriendly_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnPrinterFriendly.Click
		SaveOptionsSelected()

		Dim containerInventoryUrl As String = "ContainerInventoryPFV.aspx?OwnerId=" & ddlOwner.SelectedValue & "&LocationId=" & ddlLocation.SelectedValue & "&BulkProductId=" & ddlBulkProduct.SelectedValue & "&OnlyNonzero=" & cbxOnlyShowBulkProductsWithNonZeroInventory.Checked & "&CurrentStatus=" & (ddlCurrentStatus.SelectedIndex - 1).ToString()
		Dim additionalUnitsString As String = GetAdditionalUnitsString()
		If additionalUnitsString.Trim.Length > 0 Then containerInventoryUrl &= "&AdditionalUnitsString=" & additionalUnitsString
        If Utilities.ConvertWebPageUrlDomainToRequestedPagesDomain(GetUserConnection(_currentUser.Id)) Then containerInventoryUrl = Tm2Database.GetUrlInCurrentDomain(Tm2Database.GetCurrentPageUrlWithOriginalPort(Me.Request), "http://localhost/TerminalManagement2/" & containerInventoryUrl)


        ClientScript.RegisterClientScriptBlock(Me.GetType(), "ShowReport", Utilities.JsWindowOpen(containerInventoryUrl))
	End Sub

	Protected Sub btnDownload_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnDownload.Click
		SaveOptionsSelected()
		Dim fileName As String = String.Format("ContainerInventory{0:yyyyMMddHHmmss}.csv", Now)
		Dim ownerId As Guid = Guid.Parse(ddlOwner.SelectedValue)
		Dim locationId As Guid = Guid.Parse(ddlLocation.SelectedValue)
		Dim bulkProductId As Guid = Guid.Parse(ddlBulkProduct.SelectedValue)
		Dim bulkProductIds As New List(Of Guid)
		If Not bulkProductId.Equals(Guid.Empty) Then
			bulkProductIds.Add(bulkProductId)
		End If

		KaReports.CreateCsvFromTable("Container Inventory", KaReports.GetContainerInventoryTable(GetUserConnection(_currentUser.Id), ownerId, locationId, bulkProductIds, cbxOnlyShowBulkProductsWithNonZeroInventory.Checked, KaReports.MEDIA_TYPE_COMMA, "", ddlCurrentStatus.SelectedIndex - 1, GetAdditionalUnits()), DownloadDirectory(Me) & fileName)
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
			Dim header As String = "Container Inventory"

			Dim body As String = KaReports.GetTableHtml(header, KaReports.GetContainerInventoryTable(GetUserConnection(_currentUser.Id), ownerId, locationId, bulkProductIds, cbxOnlyShowBulkProductsWithNonZeroInventory.Checked, KaReports.MEDIA_TYPE_PFV, "", ddlCurrentStatus.SelectedIndex - 1, GetAdditionalUnits()))

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
					attachments.Add(New System.Net.Mail.Attachment(New IO.MemoryStream(Encoding.UTF8.GetBytes(newEmail.Body)), "ContainerInventoryReport.html", System.Net.Mime.MediaTypeNames.Text.Html))
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
			ddlOwner.SelectedValue = KaSetting.GetSetting(connection, "ContainerInventoryReport:" & _currentUser.Id.ToString & "/LastUsedOwner", ddlOwner.SelectedValue)
		Catch ex As Exception
		End Try
		Try
			ddlLocation.SelectedValue = KaSetting.GetSetting(connection, "ContainerInventoryReport:" & _currentUser.Id.ToString & "/LastUsedLocation", ddlLocation.SelectedValue)
		Catch ex As Exception
		End Try
		Try
			ddlBulkProduct.SelectedValue = KaSetting.GetSetting(connection, "ContainerInventoryReport:" & _currentUser.Id.ToString & "/LastUsedBulkProduct", ddlBulkProduct.SelectedValue)
		Catch ex As Exception
		End Try
		Boolean.TryParse(KaSetting.GetSetting(connection, "ContainerInventoryReport:" & _currentUser.Id.ToString & "/LastUsedOnlyShowBulkProductsWithNonZeroInventory", True), cbxOnlyShowBulkProductsWithNonZeroInventory.Checked)

		Dim additionalUnitsString As String = KaSetting.GetSetting(connection, "ContainerInventoryReport:" & _currentUser.Id.ToString & "/AdditionalUnits", "")
		For Each li As ListItem In cblAdditionalUnits.Items
			li.Selected = additionalUnitsString.Contains(li.Value)
		Next

		Try
			Integer.TryParse(KaSetting.GetSetting(connection, "ContainerInventoryReport:" & _currentUser.Id.ToString & "/CurrentStatus", "0"), ddlCurrentStatus.SelectedIndex)
		Catch ex As Exception
		End Try
	End Sub

	Private Sub SaveOptionsSelected()
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		KaSetting.WriteSetting(connection, "ContainerInventoryReport:" & _currentUser.Id.ToString & "/LastUsedOwner", ddlOwner.SelectedValue)
		KaSetting.WriteSetting(connection, "ContainerInventoryReport:" & _currentUser.Id.ToString & "/LastUsedLocation", ddlLocation.SelectedValue)
		KaSetting.WriteSetting(connection, "ContainerInventoryReport:" & _currentUser.Id.ToString & "/LastUsedBulkProduct", ddlBulkProduct.SelectedValue)
		KaSetting.WriteSetting(connection, "ContainerInventoryReport:" & _currentUser.Id.ToString & "/LastUsedOnlyShowBulkProductsWithNonZeroInventory", cbxOnlyShowBulkProductsWithNonZeroInventory.Checked)
		KaSetting.WriteSetting(connection, "ContainerInventoryReport:" & _currentUser.Id.ToString & "/AdditionalUnits", GetAdditionalUnitsString())
		KaSetting.WriteSetting(connection, "ContainerInventoryReport:" & _currentUser.Id.ToString & "/CurrentStatus", ddlCurrentStatus.SelectedIndex)
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