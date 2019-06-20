Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb
Imports System.IO
Imports KaCommonObjects

Public Class ReceivingPurhcaseOrderList : Inherits System.Web.UI.Page
	Private _currentUser As KaUser
	Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
	Private _currentTableName As String = KaOrder.TABLE_NAME

#Region "Events"
	Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
		Response.Cache.SetCacheability(HttpCacheability.NoCache)
		_currentUser = Utilities.GetUser(Me)
		_currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "PurchaseOrders")
		If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")

		If Not Page.IsPostBack Then
			PopulateSortBy()
			PopulateAscDescList()
			PopulateFacilityList()
			PopulateSuppliers()
			PopulateBulkProducts()
			PopulateOwnerList()
			PopulateEmailAddressList()

			Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
			Try
				ddlSortBy.SelectedValue = KaSetting.GetSetting(connection, "ReceivingPurchaseOrderListReport:" & _currentUser.Id.ToString & "/SortBy", ddlSortBy.SelectedValue)
			Catch ex As ArgumentOutOfRangeException
			End Try
			Try
				ddlAscDesc.SelectedValue = KaSetting.GetSetting(connection, "ReceivingPurchaseOrderListReport:" & _currentUser.Id.ToString & "/SortByAscDesc", ddlAscDesc.SelectedValue)
			Catch ex As ArgumentOutOfRangeException
			End Try
			Try
				ddlFacilityFilter.SelectedValue = KaSetting.GetSetting(connection, "ReceivingPurchaseOrderListReport:" & _currentUser.Id.ToString & "/SelectedLocationId", ddlFacilityFilter.SelectedValue)
			Catch ex As ArgumentOutOfRangeException
			End Try
			Try
				ddlSuppliers.SelectedValue = KaSetting.GetSetting(connection, "ReceivingPurchaseOrderListReport:" & _currentUser.Id.ToString & "/SelectedSupplierId", ddlSuppliers.SelectedValue)
			Catch ex As ArgumentOutOfRangeException
			End Try
			Try
				ddlOwner.SelectedValue = KaSetting.GetSetting(connection, "ReceivingPurchaseOrderListReport:" & _currentUser.Id.ToString & "/SelectedOwnerId", ddlOwner.SelectedValue)
			Catch ex As ArgumentOutOfRangeException
			End Try
			Try
				ddlBulkProducts.SelectedValue = KaSetting.GetSetting(connection, "ReceivingPurchaseOrderListReport:" & _currentUser.Id.ToString & "/SelectedBulkProductId", ddlOwner.SelectedValue)
			Catch ex As ArgumentOutOfRangeException
			End Try
		End If
		litEmailConfirmation.Text = ""
	End Sub

	Protected Sub btnShowReport_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnShowReport.Click
		Dim sortBy As String = ddlSortBy.SelectedValue & " " & ddlAscDesc.SelectedValue

		Dim selectedLocationId As Guid = Guid.Parse(ddlFacilityFilter.SelectedValue)
		Dim selectedSupplierId As Guid = Guid.Parse(ddlSuppliers.SelectedValue)
		Dim selectedOwnerId As Guid = Guid.Parse(ddlOwner.SelectedValue)
		Dim selectedBulkProductId As Guid = Guid.Parse(ddlBulkProducts.SelectedValue)

		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		SaveSelectedFilters(connection)

		Dim reportTitle As String = "Receiving Purchase Order List"
		Dim reportSubtitle As String = GenerateReportSubtitle(connection, selectedLocationId, selectedOwnerId, selectedSupplierId)
		ClientScript.RegisterClientScriptBlock(Me.GetType(), "ShowReport",
											   Utilities.JsWindowOpen("ReceivingPurchaseOrderListView.aspx?title=" & reportTitle & "&subtitle=" &
											   reportSubtitle & "&sortby=" & sortBy & "&supplier_id=" & selectedSupplierId.ToString &
											   "&owner_id=" & selectedOwnerId.ToString & "&bulk_product_id=" & selectedBulkProductId.ToString & "&location_id=" & selectedLocationId.ToString))

		KaSetting.WriteSetting(connection, "ReceivingPurchaseOrderListReport:" & _currentUser.Id.ToString & "/SortBy", ddlSortBy.SelectedValue)
		KaSetting.WriteSetting(connection, "ReceivingPurchaseOrderListReport:" & _currentUser.Id.ToString & "/SortByAscDesc", ddlAscDesc.SelectedValue)
		KaSetting.WriteSetting(connection, "ReceivingPurchaseOrderListReport:" & _currentUser.Id.ToString & "/SelectedLocationId", ddlFacilityFilter.SelectedValue)
		KaSetting.WriteSetting(connection, "ReceivingPurchaseOrderListReport:" & _currentUser.Id.ToString & "/SelectedSupplierId", ddlSuppliers.SelectedValue)
		KaSetting.WriteSetting(connection, "ReceivingPurchaseOrderListReport:" & _currentUser.Id.ToString & "/SelectedOwnerId", ddlOwner.SelectedValue)
		KaSetting.WriteSetting(connection, "ReceivingPurchaseOrderListReport:" & _currentUser.Id.ToString & "/SelectedBulkProductId", ddlOwner.SelectedValue)
	End Sub

	Protected Sub btnDownload_Click(sender As Object, e As EventArgs) Handles btnDownload.Click
		Dim sortBy As String = ddlSortBy.SelectedValue & " " & ddlAscDesc.SelectedValue
		Dim selectedLocationId As Guid = Guid.Parse(ddlFacilityFilter.SelectedValue)
		Dim selectedSupplierId As Guid = Guid.Parse(ddlSuppliers.SelectedValue)
		Dim selectedOwnerId As Guid = Guid.Parse(ddlOwner.SelectedValue)
		Dim selectedBulkProductId As Guid = Guid.Parse(ddlBulkProducts.SelectedValue)
		Dim url As String = ""
        If Utilities.ConvertWebPageUrlDomainToRequestedPagesDomain(GetUserConnection(_currentUser.Id)) Then url = Tm2Database.GetCurrentPageUrlWithOriginalPort(Me.Request)

        Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		SaveSelectedFilters(connection)
		Dim reportSubtitle As String = GenerateReportSubtitle(connection, selectedLocationId, selectedOwnerId, selectedSupplierId)

		Dim fileName As String = String.Format("ReceivingPurchaseOrderReport{0:yyyyMMddHHmmss}.csv", Now)
		KaReports.CreateCsvFromTable("Receiving Purchase Order List", reportSubtitle, KaReports.GetReceivingPurchaseOrdersListTable(connection, selectedOwnerId, KaReports.MEDIA_TYPE_COMMA, sortBy, selectedSupplierId, selectedBulkProductId, url, True, selectedLocationId), DownloadDirectory(Me) & fileName)

		ClientScript.RegisterClientScriptBlock(Me.GetType(), "DownloadReport", Utilities.JsWindowOpen("./download/" & fileName))

		KaSetting.WriteSetting(connection, "ReceivingPurchaseOrderListReport:" & _currentUser.Id.ToString & "/SortBy", ddlSortBy.SelectedValue)
		KaSetting.WriteSetting(connection, "ReceivingPurchaseOrderListReport:" & _currentUser.Id.ToString & "/SortByAscDesc", ddlAscDesc.SelectedValue)
		KaSetting.WriteSetting(connection, "ReceivingPurchaseOrderListReport:" & _currentUser.Id.ToString & "/SelectedLocationId", ddlFacilityFilter.SelectedValue)
		KaSetting.WriteSetting(connection, "ReceivingPurchaseOrderListReport:" & _currentUser.Id.ToString & "/SelectedSupplierId", ddlSuppliers.SelectedValue)
		KaSetting.WriteSetting(connection, "ReceivingPurchaseOrderListReport:" & _currentUser.Id.ToString & "/SelectedOwnerId", ddlOwner.SelectedValue)
		KaSetting.WriteSetting(connection, "ReceivingPurchaseOrderListReport:" & _currentUser.Id.ToString & "/SelectedBulkProductId", ddlOwner.SelectedValue)
	End Sub
#End Region

	Private Sub PopulateSortBy()
		ddlSortBy.Items.Clear()
        ddlSortBy.Items.Add(New ListItem("Purchase order number", "purchase_order_number"))
        ddlSortBy.Items.Add(New ListItem("Supplier name", "supplier_name"))
        ddlSortBy.Items.Add(New ListItem("Bulk product name", "bulk_product_name"))
        ddlSortBy.Items.Add(New ListItem("Owner", "owner_name"))
		ddlSortBy.Items.Add(New ListItem("Created", "purchase_order_created"))
	End Sub

	Private Sub PopulateSuppliers()
		ddlSuppliers.Items.Clear()
        ddlSuppliers.Items.Add(New ListItem("All suppliers", Guid.Empty.ToString))
        For Each account As KaCustomerAccount In KaCustomerAccount.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND is_supplier=1" & IIf(_currentUser.OwnerId <> Guid.Empty, String.Format(" AND (owner_id={0} OR owner_id={1})", Q(Guid.Empty), Q(_currentUser.OwnerId)), ""), "name")
			ddlSuppliers.Items.Add(New ListItem(account.Name, account.Id.ToString))
		Next
	End Sub

	Private Sub PopulateBulkProducts()
		ddlBulkProducts.Items.Clear()
        ddlBulkProducts.Items.Add(New ListItem("All bulk products", Guid.Empty.ToString))
        For Each bulkProduct As KaBulkProduct In KaBulkProduct.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name asc")
			ddlBulkProducts.Items.Add(New ListItem(bulkProduct.Name, bulkProduct.Id.ToString))
		Next
	End Sub

	Private Sub PopulateAscDescList()
		ddlAscDesc.Items.Clear()
		ddlAscDesc.Items.Add(New ListItem("Asc", "asc"))
		ddlAscDesc.Items.Add(New ListItem("Desc", "desc"))
	End Sub

	Private Sub PopulateFacilityList()
		ddlFacilityFilter.Items.Clear()
		ddlFacilityFilter.Items.Add(New ListItem("All facilities", Guid.Empty.ToString()))
		For Each u As KaLocation In KaLocation.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
			ddlFacilityFilter.Items.Add(New ListItem(u.Name, u.Id.ToString))
		Next
	End Sub

	Private Sub PopulateOwnerList()
		ddlOwner.Items.Clear()

		If _currentUser.OwnerId = Guid.Empty Then
			Dim li As ListItem = New ListItem
            li.Text = "All owners"
            li.Value = Guid.Empty.ToString
			ddlOwner.Items.Add(li)

			For Each owner As KaOwner In KaOwner.GetAll(GetUserConnection(_currentUser.Id), "deleted = 0", "name asc")
				li = New ListItem
				li.Text = owner.Name
				li.Value = owner.Id.ToString
				ddlOwner.Items.Add(li)
			Next
		Else
			Dim owner As KaOwner = New KaOwner(GetUserConnection(_currentUser.Id), _currentUser.OwnerId)
			Dim li As ListItem = New ListItem
			li.Text = owner.Name
			li.Value = owner.Id.ToString
			ddlOwner.Items.Add(li)
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

	Protected Sub btnSendEmail_Click(sender As Object, e As EventArgs) Handles btnSendEmail.Click
		Dim message As String = ""
		If Not Utilities.IsEmailFieldValid(tbxEmailTo.Text, message) Then
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidEmail", Utilities.JsAlert(message))
			Exit Sub
		End If
		Dim sortBy As String = ddlSortBy.SelectedValue & " " & ddlAscDesc.SelectedValue
		Dim selectedLocationId As Guid = Guid.Parse(ddlFacilityFilter.SelectedValue)
		Dim selectedSupplierId As Guid = Guid.Parse(ddlSuppliers.SelectedValue)
		Dim selectedOwnerId As Guid = Guid.Parse(ddlOwner.SelectedValue)
		Dim selectedBulkProductId As Guid = Guid.Parse(ddlBulkProducts.SelectedValue)
		Dim url As String = ""
        If Utilities.ConvertWebPageUrlDomainToRequestedPagesDomain(GetUserConnection(_currentUser.Id)) Then url = Tm2Database.GetCurrentPageUrlWithOriginalPort(Me.Request)

        Dim emailAddresses As String = ""
		If tbxEmailTo.Text.Trim().Length > 0 Then
			Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
			SaveSelectedFilters(connection)

			Dim reportTitle As String = "Receiving Purchase Order List"
			Dim reportSubtitle As String = GenerateReportSubtitle(connection, selectedLocationId, selectedOwnerId, selectedSupplierId)
			Dim reportDataList As ArrayList = KaReports.GetReceivingPurchaseOrdersListTable(connection, selectedOwnerId, KaReports.MEDIA_TYPE_HTML, sortBy, selectedSupplierId, selectedBulkProductId, url, True, selectedLocationId)
			Dim headerRowList As ArrayList = reportDataList(0) 'This will be the header row
			Dim tableAttributes As String = "border=1; width=100%;"
			Dim headerRowAttributes As String = ""
			Dim rowAttributes As String = ""

			Dim headerCellAttributeList As New List(Of String)
			Dim detailCellAttributeList As New List(Of String)
			KaReports.GetReceivingPurchaseOrderListHtmlTableFormatting(tableAttributes, headerRowAttributes, rowAttributes, headerCellAttributeList, detailCellAttributeList, headerRowList)

			Dim body As String = KaReports.GetTableHtml(reportTitle, reportSubtitle, reportDataList, False, tableAttributes, headerRowAttributes, headerCellAttributeList, rowAttributes, detailCellAttributeList)

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
					newEmail.Subject = reportTitle & " " & reportSubtitle
					Dim attachments As New List(Of System.Net.Mail.Attachment)
					attachments.Add(New System.Net.Mail.Attachment(New MemoryStream(Encoding.UTF8.GetBytes(newEmail.Body)), "ReceivingPurchaseOrderListReport.html", System.Net.Mime.MediaTypeNames.Text.Html))
					newEmail.SerializeAttachments(attachments)
					newEmail.SqlInsert(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
					If emailAddresses.Length > 0 Then emailAddresses &= ", "
					emailAddresses &= newEmail.Recipients
				End If
			Next

			KaSetting.WriteSetting(connection, "ReceivingPurchaseOrderListReport:" & _currentUser.Id.ToString & "/SortBy", ddlSortBy.SelectedValue)
			KaSetting.WriteSetting(connection, "ReceivingPurchaseOrderListReport:" & _currentUser.Id.ToString & "/SortByAscDesc", ddlAscDesc.SelectedValue)
			KaSetting.WriteSetting(connection, "ReceivingPurchaseOrderListReport:" & _currentUser.Id.ToString & "/SelectedLocationId", ddlFacilityFilter.SelectedValue)
			KaSetting.WriteSetting(connection, "ReceivingPurchaseOrderListReport:" & _currentUser.Id.ToString & "/SelectedSupplierId", ddlSuppliers.SelectedValue)
			KaSetting.WriteSetting(connection, "ReceivingPurchaseOrderListReport:" & _currentUser.Id.ToString & "/SelectedOwnerId", ddlOwner.SelectedValue)
			KaSetting.WriteSetting(connection, "ReceivingPurchaseOrderListReport:" & _currentUser.Id.ToString & "/SelectedBulkProductId", ddlOwner.SelectedValue)
		End If
		If emailAddresses.Length > 0 Then
			litEmailConfirmation.Text = "Report sent to " & emailAddresses
		Else
			litEmailConfirmation.Text = "Report not sent.  No e-mail addresses."
		End If
	End Sub

	Private Function GenerateReportSubtitle(connection As OleDbConnection, locationId As Guid, ownerId As Guid, customerId As Guid) As String
		Dim reportTitle As String = ""
		Try
			reportTitle &= IIf(reportTitle.Length > 0, ", ", "") & "Facility: " & New KaLocation(connection, locationId).Name
		Catch ex As Exception

		End Try
		Try
			reportTitle &= IIf(reportTitle.Length > 0, ", ", "") & "Owner: " & New KaOwner(connection, ownerId).Name
		Catch ex As Exception

		End Try
		Try
			reportTitle &= IIf(reportTitle.Length > 0, ", ", "") & "Supplier: " & New KaCustomerAccount(connection, customerId).Name
		Catch ex As Exception

		End Try
		If reportTitle.Length > 0 Then reportTitle = "For " & reportTitle
		Return reportTitle
	End Function

	Private Sub SaveSelectedFilters(ByVal connection As OleDbConnection)
		KaSetting.WriteSetting(connection, "ReceivingPurchaseOrderListReport:" & _currentUser.Id.ToString & "/SortBy", ddlSortBy.SelectedValue)
		KaSetting.WriteSetting(connection, "ReceivingPurchaseOrderListReport:" & _currentUser.Id.ToString & "/SortByAscDesc", ddlAscDesc.SelectedValue)
		KaSetting.WriteSetting(connection, "ReceivingPurchaseOrderListReport:" & _currentUser.Id.ToString & "/SelectedAccountId", ddlSuppliers.SelectedValue)
		KaSetting.WriteSetting(connection, "ReceivingPurchaseOrderListReport:" & _currentUser.Id.ToString & "/SelectedOwnerId", ddlOwner.SelectedValue)
		KaSetting.WriteSetting(connection, "ReceivingPurchaseOrderListReport:" & _currentUser.Id.ToString & "/SelectedBulkProductId", ddlOwner.SelectedValue)
	End Sub

	Protected Sub ddlFacilityFilter_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlFacilityFilter.SelectedIndexChanged

	End Sub
End Class