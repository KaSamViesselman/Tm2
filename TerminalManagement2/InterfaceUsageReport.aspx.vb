Imports KahlerAutomation.KaTm2Database

Public Class InterfaceUsageReport
	Inherits System.Web.UI.Page
	Private _currentUser As KaUser
	Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
	Private _currentTableName As String = KaInterface.TABLE_NAME

#Region "Events"
	Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
		Response.Cache.SetCacheability(HttpCacheability.NoCache)
		_currentUser = Utilities.GetUser(Me)
		_currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Interfaces")
		If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")

		If Not Page.IsPostBack Then
			Dim connection As OleDb.OleDbConnection = GetUserConnection(_currentUser.Id)
			Dim sql As String = "SELECT COUNT(*) " &
				"FROM interface_types " &
				"INNER JOIN interfaces ON interface_types.id = interfaces.interface_type_id " &
				"WHERE (interface_types.deleted = 0) AND (interfaces.deleted = 0) AND "
			With ddlInterfaceItemType.Items
                .Add(New ListItem("Select report type", ""))
                If Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaApplicator.TABLE_NAME}), "Applicators")(KaApplicator.TABLE_NAME).Read Then
					Dim rdr As OleDb.OleDbDataReader = Tm2Database.ExecuteReader(connection, sql & "((" & KaInterfaceTypes.FN_SHOW_APPLICATOR_INTERFACE & " = 1) " &
																		   "OR (interfaces.id IN (SELECT interface_id " &
																		   "FROM " & KaApplicatorInterfaceSettings.TABLE_NAME & " " &
																		   "WHERE (deleted=0))))")
					If rdr.Read AndAlso rdr.Item(0) > 0 Then .Add(New ListItem("Applicators", "Applicators"))
				End If
				If Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaBranch.TABLE_NAME}), "Branches")(KaBranch.TABLE_NAME).Read Then
					Dim rdr As OleDb.OleDbDataReader = Tm2Database.ExecuteReader(connection, sql & "((" & KaInterfaceTypes.FN_SHOW_BRANCH_INTERFACE & " = 1) " &
																		   "OR (interfaces.id IN (SELECT interface_id " &
																		   "FROM " & KaBranchInterfaceSettings.TABLE_NAME & " " &
																		   "WHERE (deleted=0))))")
					If rdr.Read AndAlso rdr.Item(0) > 0 Then .Add(New ListItem("Branches", "Branches"))
				End If
				If Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaProduct.TABLE_NAME}), "Products")(KaProduct.TABLE_NAME).Read Then
					Dim rdr As OleDb.OleDbDataReader = Tm2Database.ExecuteReader(connection, sql & "((" & KaInterfaceTypes.FN_SHOW_BULK_PRODUCT_INTERFACE & " = 1) " &
																		   "OR (interfaces.id IN (SELECT interface_id " &
																		   "FROM " & KaBulkProductInterfaceSettings.TABLE_NAME & " " &
																		   "WHERE (deleted=0))))")
                    If rdr.Read AndAlso rdr.Item(0) > 0 Then .Add(New ListItem("Bulk products", "BulkProducts"))
                End If
				If Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaCarrier.TABLE_NAME}), "Carriers")(KaCarrier.TABLE_NAME).Read Then
					Dim rdr As OleDb.OleDbDataReader = Tm2Database.ExecuteReader(connection, sql & "((" & KaInterfaceTypes.FN_SHOW_CARRIER_INTERFACE & " = 1) " &
																		   "OR (interfaces.id IN (SELECT interface_id " &
																		   "FROM " & KaCarrierInterfaceSettings.TABLE_NAME & " " &
																		   "WHERE (deleted=0))))")
					If rdr.Read AndAlso rdr.Item(0) > 0 Then .Add(New ListItem("Carriers", "Carriers"))
				End If
				If Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaCustomerAccount.TABLE_NAME}), "Accounts")(KaCustomerAccount.TABLE_NAME).Read Then
                    .Add(New ListItem("Customer account destinations", "CustomerAccountDestinations"))
                    .Add(New ListItem("Customer accounts", "CustomerAccounts"))
                End If
				If Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaDriver.TABLE_NAME}), "Drivers")(KaDriver.TABLE_NAME).Read Then
                    Dim rdr As OleDb.OleDbDataReader = Tm2Database.ExecuteReader(connection, sql & "((" & KaInterfaceTypes.FN_SHOW_DRIVER_INTERFACE & " = 1) " &
                                                                           "OR (interfaces.id IN (SELECT interface_id " &
                                                                           "FROM " & KaDriverInterfaceSettings.TABLE_NAME & " " &
                                                                           "WHERE (deleted=0))))")
                    If rdr.Read AndAlso rdr.Item(0) > 0 Then .Add(New ListItem("Drivers", "Drivers"))
				End If
				If Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaDriver.TABLE_NAME}), "Facilities")(KaDriver.TABLE_NAME).Read Then
					Dim rdr As OleDb.OleDbDataReader = Tm2Database.ExecuteReader(connection, sql & "((" & KaInterfaceTypes.FN_SHOW_LOCATION_INTERFACE & " = 1) " &
																		   "OR (interfaces.id IN (SELECT interface_id " &
																		   "FROM " & KaLocationInterfaceSettings.TABLE_NAME & " " &
																		   "WHERE (deleted=0))))")
					If rdr.Read AndAlso rdr.Item(0) > 0 Then .Add(New ListItem("Facilities", "Facilities"))
				End If
				If Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaOwner.TABLE_NAME}), "Owners")(KaOwner.TABLE_NAME).Read Then
					Dim rdr As OleDb.OleDbDataReader = Tm2Database.ExecuteReader(connection, sql & "((" & KaInterfaceTypes.FN_SHOW_OWNER_INTERFACE & " = 1) " &
																		   "OR (interfaces.id IN (SELECT interface_id " &
																		   "FROM " & KaOwnerInterfaceSettings.TABLE_NAME & " " &
																		   "WHERE (deleted=0))))")
					If rdr.Read AndAlso rdr.Item(0) > 0 Then .Add(New ListItem("Owners", "Owners"))
				End If
				If Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaProduct.TABLE_NAME}), "Products")(KaProduct.TABLE_NAME).Read Then .Add(New ListItem("Products", "Products"))
				If Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaReceivingPurchaseOrder.TABLE_NAME}), "PurchaseOrders")(KaReceivingPurchaseOrder.TABLE_NAME).Read Then .Add(New ListItem("Suppliers", "Suppliers"))
				If Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaTank.TABLE_NAME}), "Tanks")(KaTank.TABLE_NAME).Read Then
					Dim rdr As OleDb.OleDbDataReader = Tm2Database.ExecuteReader(connection, sql & "((" & KaInterfaceTypes.FN_SHOW_TANKS_INTERFACE & " = 1) " &
																		   "OR (interfaces.id IN (SELECT interface_id " &
																		   "FROM " & KaTankInterfaceSettings.TABLE_NAME & " " &
																		   "WHERE (deleted=0))))")
					If rdr.Read AndAlso rdr.Item(0) > 0 Then .Add(New ListItem("Tanks", "Tanks"))
				End If
				If Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaTransport.TABLE_NAME}), "Transports")(KaTransport.TABLE_NAME).Read Then
					Dim rdr As OleDb.OleDbDataReader = Tm2Database.ExecuteReader(connection, sql & "((" & KaInterfaceTypes.FN_SHOW_TRANSPORT_INTERFACE & " = 1) " &
																		   "OR (interfaces.id IN (SELECT interface_id " &
																		   "FROM " & KaTransportInterfaceSettings.TABLE_NAME & " " &
																		   "WHERE (deleted=0))))")
					If rdr.Read AndAlso rdr.Item(0) > 0 Then .Add(New ListItem("Transports", "Transports"))
				End If
				If Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaTransport.TABLE_NAME}), "Transports")(KaTransport.TABLE_NAME).Read Then
					Dim rdr As OleDb.OleDbDataReader = Tm2Database.ExecuteReader(connection, sql & "((" & KaInterfaceTypes.FN_SHOW_TRANSPORT_TYPE_INTERFACE & " = 1) " &
																		   "OR (interfaces.id IN (SELECT interface_id " &
																		   "FROM " & KaTransportTypeInterfaceSettings.TABLE_NAME & " " &
																		   "WHERE (deleted=0))))")
                    If rdr.Read AndAlso rdr.Item(0) > 0 Then .Add(New ListItem("Transport types", "TransportTypes"))
                End If
				If Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaUnit.TABLE_NAME}), "Units")(KaUnit.TABLE_NAME).Read Then
					Dim rdr As OleDb.OleDbDataReader = Tm2Database.ExecuteReader(connection, sql & "((" & KaInterfaceTypes.FN_SHOW_UNITS_INTERFACE & " = 1) " &
																		   "OR (interfaces.id IN (SELECT interface_id " &
																		   "FROM " & KaUnitInterfaceSettings.TABLE_NAME & " " &
																		   "WHERE (deleted=0))))")
					If rdr.Read AndAlso rdr.Item(0) > 0 Then .Add(New ListItem("Units", "Units"))
				End If
			End With
			ddlInterfaceItemType.SelectedIndex = 0
			PopulateInterfaceTable()
			PopulateEmailAddressList()
		End If
		litEmailConfirmation.Text = ""
	End Sub

	Protected Sub ddlInterfaceItemType_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles ddlInterfaceItemType.SelectedIndexChanged
		PopulateInterfaceTable()
	End Sub

	Protected Sub btnPrinterFriendly_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnPrinterFriendly.Click
		ClientScript.RegisterClientScriptBlock(Me.GetType(), "ShowReport", Utilities.JsWindowOpen("InterfaceUsageReportView.aspx?ReportTitle=" & ddlInterfaceItemType.SelectedItem.Text & "&ReportType=" & ddlInterfaceItemType.SelectedValue))
	End Sub

	Protected Sub btnDownload_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnDownload.Click
		Dim fileName As String = String.Format("InterfaceUsage{0:yyyyMMddHHmmss}.csv", Now)
		KaReports.CreateCsvFromTable("Interface Usage", KaReports.GetInterfaceUsageReport(GetUserConnection(_currentUser.Id), ddlInterfaceItemType.SelectedValue, _currentUser.OwnerId), DownloadDirectory(Me) & fileName)
		ClientScript.RegisterClientScriptBlock(Me.GetType(), "DownloadReport", Utilities.JsWindowOpen("./download/" & fileName))
	End Sub
#End Region

	Private Sub PopulateInterfaceTable()
		litInterfaceUsageReport.Text = KaReports.GetTableHtml(IIf(ddlInterfaceItemType.SelectedIndex > 0, "Interface Usage Report - " & ddlInterfaceItemType.SelectedItem.Text, ""), KaReports.GetInterfaceUsageReport(GetUserConnection(_currentUser.Id), ddlInterfaceItemType.SelectedValue, _currentUser.OwnerId))
		pnlReport.Visible = (ddlInterfaceItemType.SelectedIndex > 0)
		pnlSendEmail.Visible = (ddlInterfaceItemType.SelectedIndex > 0)
	End Sub

#Region " E-mail report "
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
			Dim header As String = "Interface Usage Report - " & ddlInterfaceItemType.SelectedItem.Text
			Dim body As String = KaReports.GetTableHtml(header, KaReports.GetInterfaceUsageReport(GetUserConnection(_currentUser.Id), ddlInterfaceItemType.SelectedValue, _currentUser.OwnerId))

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
					attachments.Add(New System.Net.Mail.Attachment(New IO.MemoryStream(Encoding.UTF8.GetBytes(newEmail.Body)), "InterfaceUsageReport" & ddlInterfaceItemType.SelectedItem.Text & ".html", System.Net.Mime.MediaTypeNames.Text.Html))
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
#End Region
End Class