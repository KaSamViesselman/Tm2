Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb
Imports System.IO
Imports KaCommonObjects

Public Class ValidLoads : Inherits System.Web.UI.Page
	Private _currentUser As KaUser
	Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
	Private _currentTableName As String = KaOrder.TABLE_NAME

#Region "Events"
	Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
		Response.Cache.SetCacheability(HttpCacheability.NoCache)
		_currentUser = Utilities.GetUser(Me)
		_currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Orders")
		If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")

		If Not Page.IsPostBack Then
			PopulateSortBy()
			PopulateAscDescList()
			PopulateCustomerAccounts()
			PopulateOwnerList()
			PopulateFacilityList()
			PopulateReportTypes()
			PopulateEmailAddressList()
			PopulateLockedOrderFilterList()

			Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
			Try
				ddlSortBy.SelectedValue = KaSetting.GetSetting(connection, "OrderListReport:" & _currentUser.Id.ToString & "/SortBy", ddlSortBy.SelectedValue)
			Catch ex As ArgumentOutOfRangeException
			End Try
			Try
				ddlAscDesc.SelectedValue = KaSetting.GetSetting(connection, "OrderListReport:" & _currentUser.Id.ToString & "/SortByAscDesc", ddlAscDesc.SelectedValue)
			Catch ex As ArgumentOutOfRangeException
			End Try
			Try
				ddlAccounts.SelectedValue = KaSetting.GetSetting(connection, "OrderListReport:" & _currentUser.Id.ToString & "/SelectedAccountId", ddlAccounts.SelectedValue)
			Catch ex As ArgumentOutOfRangeException
			End Try
			Try
				ddlOwner.SelectedValue = KaSetting.GetSetting(connection, "OrderListReport:" & _currentUser.Id.ToString & "/SelectedOwnerId", ddlOwner.SelectedValue)
			Catch ex As ArgumentOutOfRangeException
			End Try
			Try
				ddlReportType.SelectedValue = KaSetting.GetSetting(connection, "OrderListReport:" & _currentUser.Id.ToString & "/ReportType", ddlReportType.SelectedValue)
			Catch ex As ArgumentOutOfRangeException
			End Try
			Try
				chkDisplayLockedOrderColumn.Checked = Boolean.Parse(KaSetting.GetSetting(connection, "OrderListReport:" & _currentUser.Id.ToString & "/ShowLockedColumn", chkDisplayLockedOrderColumn.Checked.ToString))
			Catch ex As ArgumentOutOfRangeException
			End Try
			Try
				ddlLokcedOrderFilter.SelectedValue = KaSetting.GetSetting(connection, "OrderListReport:" & _currentUser.Id.ToString & "/OrderLockedFilter", ddlLokcedOrderFilter.SelectedValue)
			Catch ex As ArgumentOutOfRangeException
			End Try
		End If
		litEmailConfirmation.Text = ""
	End Sub

	Protected Sub btnShowReport_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnShowReport.Click
		Dim sortBy As String = ddlSortBy.SelectedValue & " " & ddlAscDesc.SelectedValue
		Dim selectedAccountId As Guid = Guid.Parse(ddlAccounts.SelectedValue)
		Dim selectedOwnerId As Guid = Guid.Parse(ddlOwner.SelectedValue)
		Dim selectedFacilityId As Guid = Guid.Parse(ddlFacility.SelectedValue)

		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		SaveSelectedFilters(connection)
		Dim reportTitle As String = "Order List"
		Dim reportSubtitle As String = GenerateReportSubtitle(connection, selectedFacilityId, selectedOwnerId, selectedAccountId)

		ClientScript.RegisterClientScriptBlock(Me.GetType(), "ShowReport",
											   Utilities.JsWindowOpen("OrderListView.aspx?title=" & reportTitle & "&subtitle=" &
											   reportSubtitle & "&sortby=" & sortBy & "&account_id=" & selectedAccountId.ToString &
											   "&owner_id=" & selectedOwnerId.ToString & "&facility_id=" & selectedFacilityId.ToString &
											   "&report_type=" & ddlReportType.SelectedValue.ToString & "&show_locked_column=" & chkDisplayLockedOrderColumn.Checked.ToString &
											   "&locked_filter=" & ddlLokcedOrderFilter.SelectedValue.ToString))
	End Sub

	Protected Sub btnDownload_Click(sender As Object, e As EventArgs) Handles btnDownload.Click
		Dim sortBy As String = ddlSortBy.SelectedValue & " " & ddlAscDesc.SelectedValue
		Dim selectedAccountId As Guid = Guid.Parse(ddlAccounts.SelectedValue)
		Dim selectedOwnerId As Guid = Guid.Parse(ddlOwner.SelectedValue)
		Dim selectedFacilityId As Guid = Guid.Parse(ddlFacility.SelectedValue)
		Dim reportType As KaReports.OrderListReportType
		Select Case ddlReportType.SelectedValue
			Case KaReports.OrderListReportType.OneProductPerLine.ToString
				reportType = OrderListReportType.OneProductPerLine
			Case KaReports.OrderListReportType.MultipleProductsOneColumn.ToString
				reportType = OrderListReportType.MultipleProductsOneColumn
			Case Else
				reportType = OrderListReportType.OneProductPerColumn
		End Select

		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		SaveSelectedFilters(connection)

		Dim fileName As String = String.Format("ValidLoadsReport{0:yyyyMMddHHmmss}.csv", Now)
		KaReports.CreateCsvFromTable("Order List", "", KaReports.GetOrdersTable(connection, selectedOwnerId, KaReports.MEDIA_TYPE_COMMA, sortBy, selectedAccountId, selectedFacilityId, reportType), DownloadDirectory(Me) & fileName)

		ClientScript.RegisterClientScriptBlock(Me.GetType(), "DownloadReport", Utilities.JsWindowOpen("./download/" & fileName))
	End Sub
#End Region

	Private Sub PopulateSortBy()
		ddlSortBy.Items.Clear()
        ddlSortBy.Items.Add(New ListItem("Order number", "order_number"))
        ddlSortBy.Items.Add(New ListItem("Account name", "customer_accounts_name"))
        ddlSortBy.Items.Add(New ListItem("Product name", "products_name"))
        ddlSortBy.Items.Add(New ListItem("Owner", "owners_name"))
		ddlSortBy.Items.Add(New ListItem("Created", "order_created"))
	End Sub

	Private Sub PopulateCustomerAccounts()
		ddlAccounts.Items.Clear()
        ddlAccounts.Items.Add(New ListItem("All accounts", Guid.Empty.ToString))
        For Each account As KaCustomerAccount In KaCustomerAccount.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND is_supplier=0" & IIf(_currentUser.OwnerId <> Guid.Empty, String.Format(" AND (owner_id={0} OR owner_id={1})", Q(Guid.Empty), Q(_currentUser.OwnerId)), ""), "name")
			ddlAccounts.Items.Add(New ListItem(account.Name, account.Id.ToString))
		Next
	End Sub

	Private Sub PopulateAscDescList()
		ddlAscDesc.Items.Clear()
		ddlAscDesc.Items.Add(New ListItem("Asc", "asc"))
		ddlAscDesc.Items.Add(New ListItem("Desc", "desc"))
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

	Private Sub PopulateFacilityList()
		ddlFacility.Items.Clear()

		Dim li As ListItem = New ListItem
        li.Text = "All facilities"
        li.Value = Guid.Empty.ToString
		ddlFacility.Items.Add(li)

		For Each facility As KaLocation In KaLocation.GetAll(GetUserConnection(_currentUser.Id), "deleted = 0", "name asc")
			li = New ListItem
			li.Text = facility.Name
			li.Value = facility.Id.ToString
			ddlFacility.Items.Add(li)
		Next
	End Sub

	Private Sub PopulateReportTypes()
		ddlReportType.Items.Clear()
		ddlReportType.Items.Add(New ListItem("Each product has individual column", KaReports.OrderListReportType.OneProductPerColumn.ToString()))
		ddlReportType.Items.Add(New ListItem("Each order item has individual row", KaReports.OrderListReportType.OneProductPerLine.ToString()))
		ddlReportType.Items.Add(New ListItem("Each order has single row", KaReports.OrderListReportType.MultipleProductsOneColumn.ToString()))
		ddlReportType.SelectedIndex = 0
	End Sub

	Private Sub PopulateLockedOrderFilterList()
		ddlLokcedOrderFilter.Items.Clear()
		ddlLokcedOrderFilter.Items.Add(New ListItem("Show all open orders", KaReports.OrderListLockedOrderDisplayType.AllOrders))
		ddlLokcedOrderFilter.Items.Add(New ListItem("Only show unlocked orders", KaReports.OrderListLockedOrderDisplayType.OnlyUnlocked))
		ddlLokcedOrderFilter.Items.Add(New ListItem("Only show locked orders", KaReports.OrderListLockedOrderDisplayType.OnlyLocked))
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
		Dim selectedAccountId As Guid = Guid.Parse(ddlAccounts.SelectedValue)
		Dim selectedOwnerId As Guid = Guid.Parse(ddlOwner.SelectedValue)
		Dim selectedFacilityId As Guid = Guid.Parse(ddlFacility.SelectedValue)
		Dim reportType As KaReports.OrderListReportType
		Select Case ddlReportType.SelectedValue
			Case KaReports.OrderListReportType.OneProductPerLine.ToString
				reportType = OrderListReportType.OneProductPerLine
			Case KaReports.OrderListReportType.MultipleProductsOneColumn.ToString
				reportType = OrderListReportType.MultipleProductsOneColumn
			Case Else
				reportType = OrderListReportType.OneProductPerColumn
		End Select

		Dim emailAddresses As String = ""
		If tbxEmailTo.Text.Trim().Length > 0 Then
			Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
			SaveSelectedFilters(connection)

			Dim tblOrders As New HtmlTable

			Dim reportTitle As String = "Order List"
			Dim reportSubtitle As String = GenerateReportSubtitle(connection, selectedFacilityId, selectedOwnerId, selectedAccountId)
			Dim reportDataList As ArrayList = KaReports.GetOrdersTable(connection, selectedOwnerId, KaReports.MEDIA_TYPE_HTML, sortBy, selectedAccountId, selectedFacilityId, reportType)
			Dim headerRowList As ArrayList = reportDataList(0) 'This will be the header row
			Dim tableAttributes As String = "border=1; width=100%;"
			Dim headerRowAttributes As String = ""
			Dim rowAttributes As String = ""

			Dim headerCellAttributeList As New List(Of String)
			Dim detailCellAttributeList As New List(Of String)
			KaReports.GetOrderListHtmlTableFormatting(tableAttributes, headerRowAttributes, rowAttributes, headerCellAttributeList, detailCellAttributeList, headerRowList)

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
					attachments.Add(New System.Net.Mail.Attachment(New MemoryStream(Encoding.UTF8.GetBytes(newEmail.Body)), "OrderListReport.html", System.Net.Mime.MediaTypeNames.Text.Html))
					newEmail.SerializeAttachments(attachments)
					newEmail.SqlInsert(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
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
			reportTitle &= IIf(reportTitle.Length > 0, ", ", "") & "Account: " & New KaCustomerAccount(connection, customerId).Name
		Catch ex As Exception

		End Try
		If reportTitle.Length > 0 Then reportTitle = "For " & reportTitle
		Return reportTitle
	End Function

	Private Sub SaveSelectedFilters(ByVal connection As OleDbConnection)
		KaSetting.WriteSetting(connection, "OrderListReport:" & _currentUser.Id.ToString & "/SortBy", ddlSortBy.SelectedValue)
		KaSetting.WriteSetting(connection, "OrderListReport:" & _currentUser.Id.ToString & "/SortByAscDesc", ddlAscDesc.SelectedValue)
		KaSetting.WriteSetting(connection, "OrderListReport:" & _currentUser.Id.ToString & "/SelectedAccountId", ddlAccounts.SelectedValue)
		KaSetting.WriteSetting(connection, "OrderListReport:" & _currentUser.Id.ToString & "/SelectedOwnerId", ddlOwner.SelectedValue)
		KaSetting.WriteSetting(connection, "OrderListReport:" & _currentUser.Id.ToString & "/ReportType", ddlReportType.SelectedValue)
		KaSetting.WriteSetting(connection, "OrderListReport:" & _currentUser.Id.ToString & "/ShowLockedColumn", chkDisplayLockedOrderColumn.Checked.ToString)
		KaSetting.WriteSetting(connection, "OrderListReport:" & _currentUser.Id.ToString & "/OrderLockedFilter", ddlLokcedOrderFilter.SelectedValue)
	End Sub
End Class