Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb
Imports System.IO
Imports KaCommonObjects

Public Class AllContainers : Inherits System.Web.UI.Page
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
			FillColumnDisplayCbxs()
			PopulateLocations()
			PopulateBulkItems()
			PopulateOwners()
			PopulateAccounts()
			PopulateEmailAddressList()
		End If
		litEmailConfirmation.Text = ""
	End Sub

	Protected Sub btnShowReport_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnShowReport.Click, btnPrinterFriendly.Click
		ClientScript.RegisterClientScriptBlock(Me.GetType(), "ShowReport", Utilities.JsWindowOpen("AllContainersView.aspx?" &
				   "orderBy=" & GenerateOrderBy() &
				   $"&columnsDisplayed={GetDisplayedColumns()}" &
				   IIf(txbSearch.Text.Trim.Length > 0, "&search=" & txbSearch.Text.Trim, "") &
				   IIf(ddlLocation.SelectedIndex > 0, "&locationId=" & ddlLocation.SelectedValue, "") &
				   IIf(ddlStatus.SelectedIndex > 0, "&status=" & ddlStatus.SelectedValue, "") &
				   IIf(ddlProduct.SelectedIndex > 0, "&productId=" & ddlProduct.SelectedValue, "") &
				   IIf(ddlOwner.SelectedIndex > 0, "&ownerId=" & ddlOwner.SelectedValue, "") &
				   IIf(ddlCustomerAccount.SelectedIndex > 0, "&customerId=" & ddlCustomerAccount.SelectedValue, "") &
				   "&showDeleted=" & cbxShowDeleted.Checked.ToString() &
				   "&reportMediaType=" & IIf(sender Is btnPrinterFriendly, KaReports.MEDIA_TYPE_PFV, KaReports.MEDIA_TYPE_HTML)))
		' "',null,'toolbar=yes,menubar=yes,scrollbars=yes,resizable=yes,width=750,height=500,top=50,left=50','true');</script>")
	End Sub

	Protected Sub btnDownload_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnDownload.Click
		Dim fileName As String = String.Format("AllContainers{0:yyyyMMddHHmmss}.csv", Now)
		Dim commaString As String = KaReports.GetTableCsv("", "", KaReports.GetContainerTable(GetUserConnection(_currentUser.Id), KaReports.MEDIA_TYPE_COMMA, GenerateWhereClause, GenerateOrderBy))
		Dim writer As StreamWriter = Nothing
		Try
			Dim fileOps As FileOperations = New FileOperations
			writer = fileOps.WriteFile(DownloadDirectory(Me) & fileName, New Alerts)
			writer.WriteLine(commaString)
		Finally
			If Not writer Is Nothing Then
				writer.Close()
			End If
		End Try
		ClientScript.RegisterClientScriptBlock(Me.GetType(), "DownloadReport", Utilities.JsWindowOpen("./download/" & fileName))
	End Sub
#End Region

	Protected Sub btnConfigure_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnConfigure.Click
		SaveConfiguration()
	End Sub

	Private Function GenerateOrderBy() As String
		Return ddlOrderBy.SelectedValue & " " & ddlAscDesc.SelectedValue
	End Function

	Private Function GenerateWhereClause() As String
		Dim conditions As String = "(containers.number LIKE '%" & txbSearch.Text.Trim() & "%')"
		If ddlLocation.SelectedIndex > 0 Then _
			conditions &= " AND (containers.location_id=" & Q(ddlLocation.SelectedValue) & ")"
		If ddlStatus.SelectedIndex > 0 Then _
			conditions &= " AND (containers.status = " & Q(ddlStatus.SelectedValue) & ")"
		If ddlProduct.SelectedIndex > 0 Then _
			conditions &= " AND (containers.bulk_product_id = " & Q(ddlProduct.SelectedValue) & ")"
		If ddlOwner.SelectedIndex > 0 Then _
			conditions &= " AND (containers.owner_id = " & Q(ddlOwner.SelectedValue) & ")"
		If ddlCustomerAccount.SelectedIndex > 0 Then _
			conditions &= " AND (orders.deleted = 0)" &
				" AND (order_customer_accounts.deleted = 0)" &
				" AND (order_customer_accounts.customer_account_id = " & Q(ddlCustomerAccount.SelectedValue) & ")"
		If Not cbxShowDeleted.Checked Then _
			conditions &= " AND (containers.deleted = 0)"

		Dim validContainers As String = ""
		Dim getValidContainersRdr As OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), "SELECT DISTINCT containers.id " &
														 "FROM containers " &
														 IIf(ddlCustomerAccount.SelectedIndex > 0, "INNER JOIN orders ON orders.id = containers.for_order_id " &
																"INNER JOIN order_customer_accounts ON orders.id = order_customer_accounts.order_id ", "") &
														 "WHERE " & conditions)
		Do While getValidContainersRdr.Read
			validContainers &= IIf(validContainers.Length > 0, ",", "") & Q(getValidContainersRdr.Item("id"))
		Loop
		getValidContainersRdr.Close()

		If validContainers.Length > 0 Then
			Return "id in (" & validContainers & ")"
		Else
			Return "id=" & Q(Guid.Empty)
		End If
	End Function

	Private Sub PopulateLocations()
		ddlLocation.Items.Clear()
		Dim li As ListItem = New ListItem
		li.Text = "All facilities"
		li.Value = Guid.Empty.ToString
		ddlLocation.Items.Add(li)
		Dim allLocations As ArrayList = KaLocation.GetAll(GetUserConnection(_currentUser.Id), "deleted = " & Q(False), "name asc")
		For Each location As KaLocation In allLocations
			li = New ListItem
			li.Text = location.Name
			li.Value = location.Id.ToString
			ddlLocation.Items.Add(li)
		Next
	End Sub

	Private Sub PopulateBulkItems()
		ddlProduct.Items.Clear()
		Dim li As ListItem = New ListItem
		li.Text = "All bulk products"
		li.Value = Guid.Empty.ToString
		ddlProduct.Items.Add(li)
		Dim allBulkProducts As ArrayList = KaBulkProduct.GetAll(GetUserConnection(_currentUser.Id), "deleted=0" & IIf(_currentUser.OwnerId <> Guid.Empty, String.Format(" AND (owner_id={0} OR owner_id={1})", Q(Guid.Empty), Q(_currentUser.OwnerId)), ""), "name asc")
		For Each bulkProd As KaBulkProduct In allBulkProducts
			If Not bulkProd.IsFunction(GetUserConnection(_currentUser.Id)) Then
				li = New ListItem
				li.Text = bulkProd.Name
				li.Value = bulkProd.Id.ToString
				ddlProduct.Items.Add(li)
			End If
		Next
	End Sub

	Private Sub PopulateOwners()
		ddlOwner.Items.Clear()
		Dim li As ListItem = New ListItem
		li.Text = "All owners"
		li.Value = Guid.Empty.ToString
		ddlOwner.Items.Add(li)
		Dim allOwners As ArrayList = KaOwner.GetAll(GetUserConnection(_currentUser.Id), "deleted=0" & IIf(_currentUser.OwnerId <> Guid.Empty, String.Format(" AND id={0}", Q(_currentUser.OwnerId)), ""), "name asc")
		For Each owner As KaOwner In allOwners
			li = New ListItem
			li.Text = owner.Name
			li.Value = owner.Id.ToString
			ddlOwner.Items.Add(li)
		Next
	End Sub

	Private Sub PopulateAccounts()
		ddlCustomerAccount.Items.Clear()
		Dim li As ListItem = New ListItem
		li.Text = "All accounts"
		li.Value = Guid.Empty.ToString
		ddlCustomerAccount.Items.Add(li)
		Dim allAccounts As ArrayList = KaCustomerAccount.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND is_supplier=0" & IIf(_currentUser.OwnerId <> Guid.Empty, String.Format(" AND (owner_id={0} OR owner_id={1})", Q(Guid.Empty), Q(_currentUser.OwnerId)), ""), "name asc")
		For Each account As KaCustomerAccount In allAccounts
			li = New ListItem
			li.Text = account.Name
			li.Value = account.Id.ToString
			ddlCustomerAccount.Items.Add(li)
		Next
	End Sub

	Private Sub FillColumnDisplayCbxs()
		Dim columnsDisplayed As ULong = GetCurrentColumnSetting(_currentUser.Id)

		cbxNumberColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcNumber)
		cbxLocationColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcLocation)
		cbxStatusColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcStatus)
		cbxProductColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcProduct)
		cbxOwnerColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcOwner)
		cbxAccountColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcAccount)
		cbxLastTransactionColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcLastTransaction)
		cbxEmptyWeightColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcEmptyWeight)
		cbxVolumeColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcVolume)
		cbxProductWeightColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcProductWeight)
		cbxInServiceColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcInService)
		cbxLastFilledColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcLastFilled)
		cbxBulkProdEpaColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcBulkProdEpa)
		cbxSealNumberColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcSealNumber)
		cbxCreatedColumnShow.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcCreated)
		cbxTypeColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcType)
		cbxConditionColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcCondition)
		cbxLastChangedColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcLastChanged)
		cbxManufacturedColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcManufactured)
		cbxLastInspectedColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcLastInspected)
		cbxPassedInspectionColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcPassedInspection)
		cbxRefillableColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcRefillable)
		cbxLastCleanedColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcLastCleaned)
		cbxNotesColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcNotes)
		cbxSealBrokenColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcSealBroken)
		cbxPassedPressureTestColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcPassedPressureTest)
		cbxOneWayValvePresentColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcOneWayValvePresent)
		cbxForOrderIdColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcForOrderId)
		cbxEquipmentColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcEquipment)
		cbxLastUserIdColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcLastUserId)
		cbxAssignedLotColumnShow.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcLot)
	End Sub

	Public Shared Function GetCurrentColumnSetting(currentUserId As Guid) As ULong
		Dim connection As OleDbConnection = GetUserConnection(currentUserId)
		Dim i As Integer
		If Integer.TryParse(KaSetting.GetSetting(connection, $"ContainerColumns:{currentUserId}", -1), i) AndAlso i >= 0 Then
			Return i
		Else
			Dim columnSettings As ArrayList = KaSetting.GetAll(connection, "name LIKE 'containers%'", "")
			Return KaReports.GetContainerColumnsDisplayed(columnSettings, True)
		End If
	End Function

	Private Sub SaveConfiguration()
		KaSetting.WriteSetting(GetUserConnection(_currentUser.Id), $"ContainerColumns:{_currentUser.Id}", GetDisplayedColumns())
	End Sub

	Function GetDisplayedColumns() As ULong
		Dim columnsDisplayed As ULong = 0
		If cbxNumberColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcNumber
		If cbxLocationColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcLocation
		If cbxStatusColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcStatus
		If cbxProductColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcProduct
		If cbxOwnerColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcOwner
		If cbxAccountColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcAccount
		If cbxLastTransactionColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcLastTransaction
		If cbxEmptyWeightColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcEmptyWeight
		If cbxVolumeColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcVolume
		If cbxProductWeightColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcProductWeight
		If cbxInServiceColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcInService
		If cbxLastFilledColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcLastFilled
		If cbxBulkProdEpaColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcBulkProdEpa
		If cbxSealNumberColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcSealNumber
		If cbxCreatedColumnShow.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcCreated
		If cbxTypeColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcType
		If cbxConditionColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcCondition
		If cbxLastChangedColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcLastChanged
		If cbxManufacturedColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcManufactured
		If cbxLastInspectedColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcLastInspected
		If cbxPassedInspectionColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcPassedInspection
		If cbxRefillableColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcRefillable
		If cbxLastCleanedColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcLastCleaned
		If cbxNotesColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcNotes
		If cbxSealBrokenColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcSealBroken
		If cbxPassedPressureTestColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcPassedPressureTest
		If cbxOneWayValvePresentColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcOneWayValvePresent
		If cbxForOrderIdColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcForOrderId
		If cbxEquipmentColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcEquipment
		If cbxLastUserIdColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcLastUserId
		If cbxAssignedLotColumnShow.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcLot

		Return columnsDisplayed
	End Function

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

	Protected Sub btnSendEmail_Click(sender As Object, e As EventArgs) Handles btnSendEmail.Click
		Dim message As String = ""
		If Not Utilities.IsEmailFieldValid(tbxEmailTo.Text, message) Then
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidEmail", Utilities.JsAlert(message))
			Exit Sub
		End If
		Dim emailAddresses As String = ""
		If tbxEmailTo.Text.Trim().Length > 0 Then
			Dim header As String = "All containers"
			Dim url As String = ""
			If Utilities.ConvertWebPageUrlDomainToRequestedPagesDomain(GetUserConnection(_currentUser.Id)) Then url = Tm2Database.GetCurrentPageUrlWithOriginalPort(Me.Request)
			Dim body As String = KaReports.GetTableHtml("", KaReports.GetContainerTable(GetUserConnection(_currentUser.Id), IIf(sender Is btnPrinterFriendly, KaReports.MEDIA_TYPE_PFV, KaReports.MEDIA_TYPE_HTML), GenerateWhereClause(), GenerateOrderBy(), -1, -1, url))

			Dim emailTo() As String = tbxEmailTo.Text.Split(New Char() {";", ","})
			For Each emailRecipient As String In emailTo
				If emailRecipient.Trim.Length > 0 Then
					Dim newEmail As New KaEmail()
					newEmail.ApplicationId = APPLICATION_ID
					newEmail.Body = Utilities.CreateSiteCssStyle() & body
					newEmail.BodyIsHtml = True
					newEmail.OwnerID = _currentUser.OwnerId
					newEmail.Recipients = emailRecipient.Trim
					newEmail.ReportType = KaEmailReport.ReportTypes.Generic
					newEmail.Subject = header
					Dim attachments As New List(Of System.Net.Mail.Attachment)
					attachments.Add(New System.Net.Mail.Attachment(New IO.MemoryStream(Encoding.UTF8.GetBytes(newEmail.Body)), "AllContainersReport.html", System.Net.Mime.MediaTypeNames.Text.Html))
					newEmail.SerializeAttachments(attachments)
					KaEmail.CreateEmail(GetUserConnection(_currentUser.Id), newEmail, -1, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
					If emailAddresses.Length > 0 Then emailAddresses &= ", "
					emailAddresses &= newEmail.Recipients
				End If
			Next
			If emailAddresses.Length > 0 Then
				litEmailConfirmation.Text = "Report sent to " & emailAddresses
			Else
				litEmailConfirmation.Text = "Report not sent.  No e-mail addresses."
			End If
		End If
	End Sub
End Class