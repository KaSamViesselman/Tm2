Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb
Imports KaCommonObjects
Imports System.IO

Public Class BulkProductUsageReport
	Inherits System.Web.UI.Page

	Private _currentUser As KaUser
	Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
	Private _currentTableName As String = "reports"
	Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
		Response.Cache.SetCacheability(HttpCacheability.NoCache)
		_currentUser = Utilities.GetUser(Me)
		_currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "reports")
		If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")

		If Not Page.IsPostBack Then
			Dim minDate As New DateTime(Now.Year, Now.Month, Now.Day, 0, 0, 0) ' setting default "From" time to 12:00AM of the current date
			Dim maxDate As DateTime = minDate.AddDays(1) ' setting default "To" time to the current time 
			tbxFromDate.Value = minDate.ToString("G") ' setting "From" datepicker to default times 
			tbxToDate.Value = maxDate.ToString("G") ' setting "To" datepicker to default time
			PopulateOwnersList()
			PopulateBaysList()
			PopulatePanelsList()
			PopulateBulkProductList()
			PopulateInitialOptions()
			PopulateEmailAddressList()
		End If
		litEmailConfirmation.Text = ""
	End Sub

	Private Sub PopulateInitialOptions()
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)

		Try
			ddlOwner.SelectedValue = KaSetting.GetSetting(connection, "BulkProductUsageReportView:" & _currentUser.Id.ToString & "/OwnerId", Guid.Empty.ToString())
		Catch ex As ArgumentOutOfRangeException
			ddlOwner.SelectedIndex = 0
		End Try
		Try
			ddlPanel.SelectedValue = KaSetting.GetSetting(connection, "BulkProductUsageReportView:" & _currentUser.Id.ToString & "/PanelId", Guid.Empty.ToString())
		Catch ex As ArgumentOutOfRangeException
			ddlPanel.SelectedIndex = 0
		End Try
		Try
			ddlBay.SelectedValue = KaSetting.GetSetting(connection, "BulkProductUsageReportView:" & _currentUser.Id.ToString & "/BayId", Guid.Empty.ToString())
		Catch ex As ArgumentOutOfRangeException
			ddlBay.SelectedIndex = 0
		End Try

		Dim units As New Dictionary(Of String, Integer)
		Dim unitsSelected As New Dictionary(Of String, Boolean)
		For Each unitInfo As KaUnit In KaUnit.GetAll(connection, "deleted=0 AND base_unit<>9", "name ASC")
			Dim precision As String = unitInfo.UnitPrecision
			Dim decimalCount As Integer = 0
			If precision.IndexOf(".") >= 0 Then decimalCount = Math.Max(0, Math.Min(6, precision.Length - precision.IndexOf(".") - 1))
			units.Add(unitInfo.Id.ToString(), decimalCount)

			unitsSelected.Add(unitInfo.Id.ToString(), False)
			ddlTotalUnitsDecimals.Items.Add(New ListItem(unitInfo.Name, unitInfo.Id.ToString()))
		Next
		Dim tempguid As String = Guid.NewGuid.ToString()
		Dim totalUnitsAndDecimals As String = KaSetting.GetSetting(connection, "BulkProductUsageReportView:" & _currentUser.Id.ToString & "/UnitsAndDecimals", tempguid)
		If totalUnitsAndDecimals = tempguid Then
			totalUnitsAndDecimals = ""
			KaSetting.WriteSetting(connection, "BulkProductUsageReportView:" & _currentUser.Id.ToString & "/TotalUnitsAndDecimals", totalUnitsAndDecimals)
		End If
		For Each unitItem As String In totalUnitsAndDecimals.Split("|")
			Dim values() As String = unitItem.Split(":")
			If units.ContainsKey(values(0)) AndAlso values.Length > 1 Then Integer.TryParse(values(1), units(values(0)))
			If unitsSelected.ContainsKey(values(0)) AndAlso values.Length > 2 Then Boolean.TryParse(values(2), unitsSelected(values(0)))
		Next
		PopulateTotalUnits(units, unitsSelected)

		Boolean.TryParse(KaSetting.GetSetting(connection, "BulkProductUsageReportView:" & _currentUser.Id.ToString & "/IncludeAllBulkProducts", True), cbxIncludeAllBulkProducts.Checked)
		Dim bulkProductList As New List(Of Guid)
		Try
			bulkProductList = Tm2Database.FromXml(KaSetting.GetSetting(connection, "BulkProductUsageReportView:" & _currentUser.Id.ToString & "/BulkProductIds", ""), GetType(List(Of Guid)))
		Catch ex As Exception

		End Try
		For Each bulkProd As ListItem In cblIncludedBulkProducts.Items
			Dim bulkProdId As Guid = Guid.Empty

			bulkProd.Selected = Guid.TryParse(bulkProd.Value, bulkProdId) AndAlso bulkProductList.Contains(bulkProdId)
		Next
		cbxIncludeAllBulkProducts_CheckedChanged(cbxIncludeAllBulkProducts, New EventArgs())
		Boolean.TryParse(KaSetting.GetSetting(connection, "BulkProductUsageReportView:" & _currentUser.Id.ToString & "/IncludeVoidedTickets", cbxIncludeVoidedTickets.Checked), cbxIncludeVoidedTickets.Checked)
	End Sub

	Protected Sub btnShowReport_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnShowReport.Click, btnPrinterFriendly.Click
		Dim fromDate As DateTime
		Dim toDate As DateTime

		If ValidateReportOptions(fromDate, toDate) Then
			Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
			Dim ownerId As Guid = Guid.Empty
			Dim panelId As Guid = Guid.Empty
			Dim bayId As Guid = Guid.Empty
			Dim units As New List(Of KaUnit)
			Dim unitsAndDecimals As String = ""
			Dim bulkProductList As New List(Of Guid)
			Dim includeVoidedTickets As Boolean = False
			If SetReportOptions(connection, ownerId, panelId, bayId, units, unitsAndDecimals, bulkProductList, includeVoidedTickets) Then
				Dim address As String = "BulkProductUsageReportView.aspx?"
				address &= "fromDate=" & fromDate
				address &= "&toDate=" & toDate
				address &= "&ownerId=" & ddlOwner.SelectedValue
				address &= "&bayId=" & ddlBay.SelectedValue
				address &= "&panelId=" & ddlPanel.SelectedValue
				address &= "&units_displayed=" & unitsAndDecimals
				address &= "&include_voided=" & includeVoidedTickets
				If sender Is btnPrinterFriendly Then
					address &= "&media_type=" & KaReports.MEDIA_TYPE_PFV
				Else
					address &= "&media_type=" & KaReports.MEDIA_TYPE_HTML
				End If
				If Not cbxIncludeAllBulkProducts.Checked Then address &= "&bulkProdList=" & Tm2Database.ToXml(bulkProductList, GetType(List(Of Guid)))

				ClientScript.RegisterClientScriptBlock(Me.GetType(), "ShowReport", Utilities.JsWindowOpen(address))
			End If
		End If
	End Sub

	Private Sub btnDownload_Click(sender As Object, e As System.EventArgs) Handles btnDownload.Click
		Dim fromDate As DateTime
		Dim toDate As DateTime

		If ValidateReportOptions(fromDate, toDate) Then
			Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
			Dim ownerId As Guid = Guid.Empty
			Dim panelId As Guid = Guid.Empty
			Dim bayId As Guid = Guid.Empty
			Dim units As New List(Of KaUnit)
			Dim unitsAndDecimals As String = ""
			Dim bulkProductList As New List(Of Guid)
			Dim includeVoidedTickets As Boolean = False
			If SetReportOptions(connection, ownerId, panelId, bayId, units, unitsAndDecimals, bulkProductList, includeVoidedTickets) Then
				Dim reportTitle As String = BulkProductUsageReportView.GenerateHeader(connection, bulkProductList, fromDate, toDate, ownerId, bayId, panelId)
				Dim commaString As String = KaReports.GetTableCsv(reportTitle, "", KaReports.GetBulkProductUsageReport(connection, fromDate, toDate, ownerId, panelId, bayId, units, bulkProductList, KaReports.MEDIA_TYPE_COMMA, includeVoidedTickets))

				Dim fileName As String = String.Format("BulkProductUsageReport{0:yyyyMMddHHmmss}.csv", Now)

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
			End If
		End If
	End Sub

	Private Function SetReportOptions(connection As OleDbConnection, ByRef ownerId As Guid, ByRef panelId As Guid, ByRef bayId As Guid, ByRef units As List(Of KaUnit), ByRef unitsAndDecimals As String, ByRef bulkProductList As List(Of Guid), ByRef includeVoidedRecords As Boolean) As Boolean
		Guid.TryParse(ddlOwner.SelectedValue, ownerId)
		Guid.TryParse(ddlPanel.SelectedValue, panelId)
		Guid.TryParse(ddlBay.SelectedValue, bayId)
		bulkProductList.Clear()
		If Not cbxIncludeAllBulkProducts.Checked Then
			For Each bulkProd As ListItem In cblIncludedBulkProducts.Items
				If bulkProd.Selected Then bulkProductList.Add(Guid.Parse(bulkProd.Value))
			Next
			If bulkProductList.Count = 0 Then
				ClientScript.RegisterClientScriptBlock(Me.GetType(), "NoBulkProductsSelected", Utilities.JsAlert("There are no bulk products selected for this report. Please select at least 1 bulk product to display."))
				Return False
			End If
			KaSetting.WriteSetting(connection, "BulkProductUsageReportView:" & _currentUser.Id.ToString & "/BulkProductIds", Tm2Database.ToXml(bulkProductList, GetType(List(Of Guid))))
		End If
		includeVoidedRecords = cbxIncludeVoidedTickets.Checked

		KaSetting.WriteSetting(connection, "BulkProductUsageReportView:" & _currentUser.Id.ToString & "/OwnerId", ownerId.ToString)
		KaSetting.WriteSetting(connection, "BulkProductUsageReportView:" & _currentUser.Id.ToString & "/PanelId", panelId.ToString)
		KaSetting.WriteSetting(connection, "BulkProductUsageReportView:" & _currentUser.Id.ToString & "/BayId", bayId.ToString)

		Dim decimalsDisplayed As Integer = 0
		For Each unitItem As ListItem In cblUnits.Items
			If unitItem.Selected Then
				Dim values() As String = unitItem.Value.Split(":")
				Try
					Dim unit As New KaUnit(connection, Guid.Parse(values(0)))
					decimalsDisplayed = 0
					If Integer.TryParse(values(1), decimalsDisplayed) Then
						unit.UnitPrecision = Tm2Database.GeneratePrecisionFormat(1, decimalsDisplayed).Replace(",", "")
					End If

					If Not units.Contains(unit) Then units.Add(unit)
				Catch ex As Exception

				End Try
				If unitsAndDecimals.Length > 0 Then unitsAndDecimals &= "|"
				unitsAndDecimals &= unitItem.Value & ":" & unitItem.Selected.ToString
			End If
		Next

		KaSetting.WriteSetting(connection, "BulkProductUsageReportView:" & _currentUser.Id.ToString & "/UnitsAndDecimals", unitsAndDecimals)
		KaSetting.WriteSetting(connection, "BulkProductUsageReportView:" & _currentUser.Id.ToString & "/IncludeAllBulkProducts", cbxIncludeAllBulkProducts.Checked)
		KaSetting.WriteSetting(connection, "BulkProductUsageReportView:" & _currentUser.Id.ToString & "/IncludeVoidedTickets", cbxIncludeVoidedTickets.Checked)

		Return True
	End Function

	Private Function ValidateReportOptions(ByRef fromDate As DateTime, ByRef toDate As DateTime) As Boolean
		Try
			fromDate = DateTime.Parse(tbxFromDate.Value) ' converting string value to datetime value for comparison in IF statement
		Catch ex As FormatException
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidStartDate", Utilities.JsAlert("Please enter a valid date for the beginning (From) date"))
			Return False
		End Try
		Try
			toDate = DateTime.Parse(tbxToDate.Value)
		Catch ex As FormatException
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidEndDate", Utilities.JsAlert("Please enter a valid date for the ending (To) date"))
			Return False
		End Try
		If fromDate > toDate Then ' check if "From" date is later then "To" date 
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidStartEndDate", Utilities.JsAlert("Please select an ending date (To) that is later than the beginning date (From)"))
			Return False
		End If
		Return True
	End Function

	Private Sub PopulateOwnersList()
		ddlOwner.Items.Clear()
		If _currentUser.OwnerId.Equals(Guid.Empty) Then ddlOwner.Items.Add(New ListItem("All owners", Guid.Empty.ToString()))
		For Each r As KaOwner In KaOwner.GetAll(GetUserConnection(_currentUser.Id), "deleted=0" & IIf(_currentUser.OwnerId.Equals(Guid.Empty), "", " AND id=" & Q(_currentUser.OwnerId)), "name ASC")
			ddlOwner.Items.Add(New ListItem(r.Name, r.Id.ToString()))
		Next
	End Sub

	Private Sub PopulatePanelsList()
		ddlPanel.Items.Clear()
		ddlPanel.Items.Add(New ListItem("All panels", Guid.Empty.ToString()))
		For Each r As KaPanel In KaPanel.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
			ddlPanel.Items.Add(New ListItem(r.Name, r.Id.ToString()))
		Next
	End Sub

	Private Sub PopulateBaysList()
		ddlBay.Items.Clear()
		ddlBay.Items.Add(New ListItem("All bays", Guid.Empty.ToString()))
		For Each r As KaBay In KaBay.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
			ddlBay.Items.Add(New ListItem(r.Name, r.Id.ToString()))
		Next
	End Sub

	Private Sub PopulateTotalUnits(ByVal units As Dictionary(Of String, Integer), unitsSelected As Dictionary(Of String, Boolean))
		cblUnits.Items.Clear()
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		For Each unitId As String In units.Keys
			Try
				Dim unitInfo As New KaUnit(connection, Guid.Parse(unitId))
				Dim decimalDisplay As String = unitInfo.UnitPrecision
				Dim decimalCount As Integer = units(unitId)
				decimalDisplay = Tm2Database.GeneratePrecisionFormat(1, decimalCount)
				cblUnits.Items.Add(New ListItem(unitInfo.Name & " (" & decimalDisplay & ")", unitId & ":" & units(unitId).ToString()))
				cblUnits.Items(cblUnits.Items.Count - 1).Selected = unitsSelected(unitId)
			Catch ex As RecordNotFoundException

			End Try
		Next
	End Sub

	Private Sub PopulateBulkProductList()
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		cblIncludedBulkProducts.Items.Clear()
		For Each bulkProduct As KaBulkProduct In KaBulkProduct.GetAll(connection, "deleted = 0", "name")
			If Not bulkProduct.IsFunction(connection) Then
				cblIncludedBulkProducts.Items.Add(New ListItem(bulkProduct.Name, bulkProduct.Id.ToString()))
			End If
		Next
	End Sub

	Protected Sub btnIncreaseTotalDecimalDigits_Click(sender As Object, e As EventArgs) Handles btnIncreaseTotalDecimalDigits.Click
		Dim units As New Dictionary(Of String, Integer)
		Dim unitsSelected As New Dictionary(Of String, Boolean)

		For Each li As ListItem In cblUnits.Items
			Dim values() As String = li.Value.Split(":")
			units.Add(values(0), values(1))
			unitsSelected.Add(values(0), li.Selected)
		Next
		If units.ContainsKey(ddlTotalUnitsDecimals.SelectedValue) Then units(ddlTotalUnitsDecimals.SelectedValue) = Math.Min(6, units(ddlTotalUnitsDecimals.SelectedValue) + 1)
		PopulateTotalUnits(units, unitsSelected)
	End Sub

	Private Sub btnDecreaseTotalDecimalDigits_Click(sender As Object, e As System.EventArgs) Handles btnDecreaseTotalDecimalDigits.Click
		Dim units As New Dictionary(Of String, Integer)
		Dim unitsSelected As New Dictionary(Of String, Boolean)

		For Each li As ListItem In cblUnits.Items
			Dim values() As String = li.Value.Split(":")
			units.Add(values(0), values(1))
			unitsSelected.Add(values(0), li.Selected)
		Next
		If units.ContainsKey(ddlTotalUnitsDecimals.SelectedValue) Then units(ddlTotalUnitsDecimals.SelectedValue) = Math.Max(0, units(ddlTotalUnitsDecimals.SelectedValue) - 1)
		PopulateTotalUnits(units, unitsSelected)
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
		Dim fromDate As DateTime
		Dim toDate As DateTime

		If ValidateReportOptions(fromDate, toDate) Then
			Dim message As String = ""
			If Not Utilities.IsEmailFieldValid(tbxEmailTo.Text, message) Then
				ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidEmail", Utilities.JsAlert(message))
				Exit Sub
			End If
			Dim emailAddresses As String = ""
			If tbxEmailTo.Text.Trim().Length > 0 Then
				Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
				Dim ownerId As Guid = Guid.Empty
				Dim panelId As Guid = Guid.Empty
				Dim bayId As Guid = Guid.Empty
				Dim units As New List(Of KaUnit)
				Dim unitsAndDecimals As String = ""
				Dim bulkProductList As New List(Of Guid)
				Dim includeVoidedTickets As Boolean = False
				If SetReportOptions(connection, ownerId, panelId, bayId, units, unitsAndDecimals, bulkProductList, includeVoidedTickets) Then
					Dim reportTitle As String = BulkProductUsageReportView.GenerateHeader(connection, bulkProductList, fromDate, toDate, ownerId, bayId, panelId)
					Dim body As String = KaReports.GetTableHtml(reportTitle, KaReports.GetBulkProductUsageReport(connection, fromDate, toDate, ownerId, panelId, bayId, units, bulkProductList, KaReports.MEDIA_TYPE_HTML, includeVoidedTickets))

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
							newEmail.Subject = reportTitle
							Dim attachments As New List(Of System.Net.Mail.Attachment)
							attachments.Add(New System.Net.Mail.Attachment(New IO.MemoryStream(Encoding.UTF8.GetBytes(newEmail.Body)), "BulkProductUsageReport.html", System.Net.Mime.MediaTypeNames.Text.Html))
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
			End If
		End If
	End Sub
#End Region

	Private Sub cbxIncludeAllBulkProducts_CheckedChanged(sender As Object, e As System.EventArgs) Handles cbxIncludeAllBulkProducts.CheckedChanged
		pnlIncludedBulkProducts.Visible = Not cbxIncludeAllBulkProducts.Checked
	End Sub
End Class