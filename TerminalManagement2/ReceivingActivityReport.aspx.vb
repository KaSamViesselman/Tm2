Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb
Imports System.Math
Imports System.IO
Imports KaCommonObjects

Public Class ReceivingActivityReport : Inherits System.Web.UI.Page

	Private _currentUser As KaUser
	Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
	Private _currentTableName As String = "reports"

#Region "Events"
	Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
		Response.Cache.SetCacheability(HttpCacheability.NoCache)
		_currentUser = Utilities.GetUser(Me)
		_currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "reports")
		If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")

		If Not Page.IsPostBack Then
			Dim minDate As New DateTime(Now.Year, Now.Month, Now.Day, 0, 0, 0)
			Dim maxDate As New DateTime(Now.Year, Now.Month, Now.Day, Now.Hour, Now.Minute, Now.Second)
			tbxFromDate.Value = minDate
			tbxToDate.Value = maxDate
			PopulateSupplierAccountList(_currentUser)
			PopulateBulkProductList(_currentUser)
			PopulateOwnerList(_currentUser)
			PopulateFacilityList()
			PopulateDriverList()
			PopulateTransportList()
			PopulateCarrierList()
			PopulateUnitList()
			PopulateSortList()
			PopulateInitialOptions()
			PopulateEmailAddressList()
		End If
		litEmailConfirmation.Text = ""
	End Sub

	Protected Sub btnShowReport_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnShowReport.Click, btnPrinterFriendly.Click
		If ValidateOptions() Then
			Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
			Dim col As UInt64 = GetColumnsDisplayed()
			Dim unit As KaUnit = New KaUnit(connection, Guid.Parse(ddlUnit.SelectedValue))
			Dim totalUnits As New List(Of KaUnit)
			Dim totalUnitsAndDecimals As String = ""

			SaveOptionsSelected(connection, col, unit, totalUnits, totalUnitsAndDecimals)

			Dim address As String = "ReceivingActivityReportView.aspx?"
			If ddlSupplierAccount.SelectedValue <> Guid.Empty.ToString Then address &= "&supplier_account_id=" & Server.HtmlEncode(ddlSupplierAccount.SelectedValue)
			If ddlBulkProduct.SelectedValue <> Guid.Empty.ToString Then address &= "&bulk_product_id=" & Server.HtmlEncode(ddlBulkProduct.SelectedValue)
			If ddlOwner.SelectedValue <> Guid.Empty.ToString Then address &= "&owner_id=" & Server.HtmlEncode(ddlOwner.SelectedValue)
			If ddlFacility.SelectedValue <> Guid.Empty.ToString Then address &= "&facility_id=" & Server.HtmlEncode(ddlFacility.SelectedValue)
			If ddlDriver.SelectedValue <> Guid.Empty.ToString Then address &= "&driver_id=" & Server.HtmlEncode(ddlDriver.SelectedValue)
			If ddlTransport.SelectedValue <> Guid.Empty.ToString Then address &= "&transport_id=" & Server.HtmlEncode(ddlTransport.SelectedValue)
			If ddlCarrier.SelectedValue <> Guid.Empty.ToString Then address &= "&carrier_id=" & Server.HtmlEncode(ddlCarrier.SelectedValue)
			address &= "&unit_id=" & Server.HtmlEncode(ddlUnit.SelectedValue)
			address &= "&current_user_id=" & Server.HtmlEncode(_currentUser.Id.ToString)
			address &= "&sort=" & Server.HtmlEncode(ddlSort.SelectedIndex)
			address &= "&include_void=" & Server.HtmlEncode(cbxIncludeVoidedTickets.Checked)
			address &= "&ticket_number=" & Server.HtmlEncode(cbxTicketNumber.Checked)
			address &= "&from_date=" & Server.HtmlEncode(tbxFromDate.Value)
			address &= "&to_date=" & Server.HtmlEncode(tbxToDate.Value)
			address &= "&decimals_displayed=" & Server.HtmlEncode(ddlDigitsAfterDecimalPoint.SelectedIndex)
			address &= "&columns=" & col
			If sender Is btnPrinterFriendly Then
				address &= "&media_type=" & KaReports.MEDIA_TYPE_PFV
			Else
				address &= "&media_type=" & KaReports.MEDIA_TYPE_HTML
			End If
			address &= "&total_units_displayed=" & totalUnitsAndDecimals

			ClientScript.RegisterClientScriptBlock(Me.GetType(), "ShowReport", Utilities.JsWindowOpen(address))
		End If
	End Sub

	Private Sub btnDownload_Click(sender As Object, e As System.EventArgs) Handles btnDownload.Click
		If ValidateOptions() Then
			Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
			Dim col As UInt64 = GetColumnsDisplayed()
			Dim unit As KaUnit = New KaUnit(connection, Guid.Parse(ddlUnit.SelectedValue))
			Dim totalUnits As New List(Of KaUnit)
			Dim totalUnitsAndDecimals As String = ""

			SaveOptionsSelected(connection, col, unit, totalUnits, totalUnitsAndDecimals)

			Dim url As String = ""
			If Utilities.ConvertWebPageUrlDomainToRequestedPagesDomain(GetUserConnection(_currentUser.Id)) Then url = Tm2Database.GetCurrentPageUrlWithOriginalPort(Me.Request)
			Dim commaString As String = KaReports.GetReceivingActivityTable(connection, KaReports.MEDIA_TYPE_COMMA, GenerateQuery(), GenerateHeader(), col, unit, "", "", "", totalUnits, url, True, True, False)

			Dim fileName As String = String.Format("ReceivingActivityReport{0:yyyyMMddHHmmss}.csv", Now)

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
	End Sub
#End Region

	Private Function GetColumnsDisplayed() As UInt64
		Dim col As UInt64 = 0
		If cbxDateTime.Checked Then col += 2 ^ KaReports.ReceivingActivityReportColumns.RcDateTime
		If cbxPurchaseOrderNumber.Checked Then col += 2 ^ KaReports.ReceivingActivityReportColumns.RcOrderNumber
		If cbxTicketNumber.Checked Then col += 2 ^ KaReports.ReceivingActivityReportColumns.RcTicketNumber
		If cbxSupplier.Checked Then col += 2 ^ KaReports.ReceivingActivityReportColumns.RcSupplier
		If cbxOwner.Checked Then col += 2 ^ KaReports.ReceivingActivityReportColumns.RcOwner
		If cbxFacility.Checked Then col += 2 ^ KaReports.ReceivingActivityReportColumns.RcFacility
		If cbxPanel.Checked Then col += 2 ^ KaReports.ReceivingActivityReportColumns.RcPanel
		If cbxDriver.Checked Then col += 2 ^ KaReports.ReceivingActivityReportColumns.RcDriver
		If cbxTransports.Checked Then col += 2 ^ KaReports.ReceivingActivityReportColumns.RcTransport
		If cbxCarrier.Checked Then col += 2 ^ KaReports.ReceivingActivityReportColumns.RcCarrier
		If cbxNotes.Checked Then col += 2 ^ KaReports.ReceivingActivityReportColumns.RcNotes
		If cbxLotNumber.Checked Then col += 2 ^ KaReports.ReceivingActivityReportColumns.RcLotNumber

		Return col
	End Function

	Private Function GenerateQuery() As String
		Return GenerateQuery(ddlSupplierAccount.SelectedValue, ddlBulkProduct.SelectedValue, ddlOwner.SelectedValue, ddlFacility.SelectedValue, ddlDriver.SelectedValue, ddlTransport.SelectedValue, ddlCarrier.SelectedValue, ddlSort.SelectedIndex, cbxIncludeVoidedTickets.Checked, cbxTicketNumber.Checked, tbxFromDate.Value, tbxToDate.Value)
	End Function

	Public Shared Function GenerateQuery(supplierAccountId As String, bulkProductId As String, ownerId As String, facilityId As String, driverId As String, transportId As String, carrierId As String, sortIndex As Integer, includeVoidedTickets As Boolean, ticketNumber As Boolean, fromDay As String, toDay As String) As String
		Dim fromDate As DateTime = DateTime.Parse(fromDay)
		Dim toDate As DateTime = DateTime.Parse(toDay)
		Dim query As String = "SELECT receiving_tickets.id, receiving_tickets.bulk_product_id " &
			"FROM receiving_tickets " &
			"WHERE receiving_tickets.date_of_delivery>=" & Q(fromDate) &
				" AND receiving_tickets.date_of_delivery<=" & Q(toDate) &
				 IIf(Guid.Parse(supplierAccountId).Equals(Guid.Empty), "", " AND receiving_tickets.supplier_account_id=" & Q(Guid.Parse(supplierAccountId))) &
				 IIf(Guid.Parse(bulkProductId).Equals(Guid.Empty), "", " AND receiving_tickets.bulk_product_id=" & Q(Guid.Parse(bulkProductId))) &
				 IIf(Guid.Parse(ownerId).Equals(Guid.Empty), "", " AND receiving_tickets.owner_id=" & Q(Guid.Parse(ownerId))) &
				 IIf(Guid.Parse(facilityId).Equals(Guid.Empty), "", " AND receiving_tickets.location_id=" & Q(Guid.Parse(facilityId))) &
				 IIf(Guid.Parse(driverId).Equals(Guid.Empty), "", " AND receiving_tickets.driver_id=" & Q(Guid.Parse(driverId))) &
				 IIf(Guid.Parse(transportId).Equals(Guid.Empty), "", " AND receiving_tickets.transport_id=" & Q(Guid.Parse(transportId))) &
				 IIf(Guid.Parse(carrierId).Equals(Guid.Empty), "", " AND receiving_tickets.carrier_id=" & Q(Guid.Parse(carrierId))) &
				 IIf(includeVoidedTickets AndAlso ticketNumber, "", " AND receiving_tickets.voided=0") &
			" ORDER BY " & GetSortBy(sortIndex) & " ASC"
		Return query
	End Function

	Private Function GenerateHeader() As String
		Return GenerateHeader(ddlSupplierAccount.SelectedValue, ddlBulkProduct.SelectedValue, ddlOwner.SelectedValue, ddlFacility.SelectedValue, ddlDriver.SelectedValue, ddlTransport.SelectedValue, ddlCarrier.SelectedValue, tbxFromDate.Value, tbxToDate.Value, _currentUser.Id.ToString)
	End Function

	Public Shared Function GenerateHeader(supplierAccountId As String, bulkProductId As String, ownerId As String, facilityId As String, driverId As String, transportId As String, carrierId As String, fromDay As String, toDay As String, currentUserId As String)
		Dim header As String = "Receiving activity report From " & fromDay &
	" to " & toDay
		If Guid.Parse(supplierAccountId) <> Guid.Empty Then
			header &= ", for supplier account '" & (New KaSupplierAccount(GetUserConnection(Guid.Parse(currentUserId)), Guid.Parse(supplierAccountId))).Name & "'"
		End If
		If Guid.Parse(bulkProductId) <> Guid.Empty Then
			header &= ", with product '" & (New KaBulkProduct(GetUserConnection(Guid.Parse(currentUserId)), Guid.Parse(bulkProductId))).Name & "'"
		End If
		If Guid.Parse(ownerId) <> Guid.Empty Then
			header &= ", for owner '" & (New KaOwner(GetUserConnection(Guid.Parse(currentUserId)), Guid.Parse(ownerId))).Name & "'"
		End If
		If Guid.Parse(facilityId) <> Guid.Empty Then
			header &= ", at the '" & (New KaLocation(GetUserConnection(Guid.Parse(currentUserId)), Guid.Parse(facilityId))).Name & "' facility"
		End If
		If Guid.Parse(driverId) <> Guid.Empty Then
			header &= ", with driver '" & (New KaDriver(GetUserConnection(Guid.Parse(currentUserId)), Guid.Parse(driverId))).Name & "'"
		End If
		If Guid.Parse(transportId) <> Guid.Empty Then
			header &= ", in transport '" & (New KaTransport(GetUserConnection(Guid.Parse(currentUserId)), Guid.Parse(transportId))).Name & "'"
		End If
		If Guid.Parse(carrierId) <> Guid.Empty Then
			header &= ", using carrier '" & (New KaCarrier(GetUserConnection(Guid.Parse(currentUserId)), Guid.Parse(carrierId))).Name & "'"
		End If
		Return header
	End Function

	Private Shared Function GetSortBy(ByVal sortBy As Integer) As String
		Select Case sortBy
			Case 0 : Return "receiving_tickets.supplier_account_name"
			Case 1 : Return "receiving_tickets.date_of_delivery"
			Case 2 : Return "receiving_tickets.driver_name"
			Case 3 : Return "receiving_tickets.location_name"
			Case 4 : Return "receiving_tickets.purchase_order_number"
			Case 5 : Return "receiving_tickets.owner_name"
			Case 6 : Return "receiving_tickets.bulk_product_name"
			Case Else : Return "receiving_tickets.date_of_delivery"
		End Select
	End Function

	Private Sub PopulateSupplierAccountList(ByVal currentUser As KaUser)
		ddlSupplierAccount.Items.Clear()
		ddlSupplierAccount.Items.Add(New ListItem("All supplier accounts", Guid.Empty.ToString()))
		For Each r As KaSupplierAccount In KaSupplierAccount.GetAll(GetUserConnection(currentUser.Id), "deleted=0" & IIf(currentUser.OwnerId.Equals(Guid.Empty), "", " AND owner_id=" & Q(currentUser.OwnerId)), "name ASC")
			ddlSupplierAccount.Items.Add(New ListItem(r.Name, r.Id.ToString()))
		Next
	End Sub

	Private Sub PopulateBulkProductList(ByVal currentUser As KaUser)
		ddlBulkProduct.Items.Clear()
		ddlBulkProduct.Items.Add(New ListItem("All bulk products", Guid.Empty.ToString()))
		For Each r As KaBulkProduct In KaBulkProduct.GetAll(GetUserConnection(currentUser.Id), "deleted=0" & IIf(currentUser.OwnerId.Equals(Guid.Empty), "", " AND owner_id=" & Q(currentUser.OwnerId)), "name ASC")
			If Not r.IsFunction(GetUserConnection(currentUser.Id)) Then
				ddlBulkProduct.Items.Add(New ListItem(r.Name, r.Id.ToString()))
			End If
		Next
	End Sub

	Private Sub PopulateOwnerList(ByVal currentUser As KaUser)
		ddlOwner.Items.Clear()
		If currentUser.OwnerId.Equals(Guid.Empty) Then ddlOwner.Items.Add(New ListItem("All owners", Guid.Empty.ToString()))
		For Each r As KaOwner In KaOwner.GetAll(GetUserConnection(currentUser.Id), "deleted=0" & IIf(currentUser.OwnerId.Equals(Guid.Empty), "", " AND id=" & Q(currentUser.OwnerId)), "name ASC")
			ddlOwner.Items.Add(New ListItem(r.Name, r.Id.ToString()))
		Next
	End Sub

	Private Sub PopulateFacilityList()
		ddlFacility.Items.Clear()
		ddlFacility.Items.Add(New ListItem("All facilities", Guid.Empty.ToString()))
		For Each r As KaLocation In KaLocation.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
			ddlFacility.Items.Add(New ListItem(r.Name, r.Id.ToString()))
		Next
		ddlFacility.SelectedIndex = 0
	End Sub

	Private Sub PopulateDriverList()
		ddlDriver.Items.Clear()
		ddlDriver.Items.Add(New ListItem("All drivers", Guid.Empty.ToString()))
		For Each r As KaDriver In KaDriver.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
			ddlDriver.Items.Add(New ListItem(r.Name, r.Id.ToString()))
		Next
	End Sub

	Private Sub PopulateTransportList()
		ddlTransport.Items.Clear()
		ddlTransport.Items.Add(New ListItem("All transports", Guid.Empty.ToString()))
		For Each r As KaTransport In KaTransport.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
			ddlTransport.Items.Add(New ListItem(r.Name, r.Id.ToString()))
		Next
	End Sub

	Private Sub PopulateCarrierList()
		ddlCarrier.Items.Clear()
		ddlCarrier.Items.Add(New ListItem("All carriers", Guid.Empty.ToString()))
		For Each r As KaCarrier In KaCarrier.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
			ddlCarrier.Items.Add(New ListItem(r.Name, r.Id.ToString()))
		Next
	End Sub

	Private Sub PopulateUnitList()
		ddlUnit.Items.Clear()
		For Each r As KaUnit In KaUnit.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND base_unit<>9", "abbreviation ASC")
			ddlUnit.Items.Add(New ListItem(r.Abbreviation, r.Id.ToString()))
			If r.BaseUnit = KaUnit.Unit.Pounds AndAlso r.Factor = 1 Then ddlUnit.SelectedIndex = ddlUnit.Items.Count - 1 ' This should be Pounds
		Next
	End Sub

	Private Sub PopulateSortList()
		ddlSort.Items.Clear()
		ddlSort.Items.Add("Supplier account")
		ddlSort.Items.Add("Date/time")
		ddlSort.Items.Add("Driver")
		ddlSort.Items.Add("Facility")
		ddlSort.Items.Add("Order number")
		ddlSort.Items.Add("Owner")
		ddlSort.Items.Add("Product")
		ddlSort.SelectedIndex = 1
	End Sub

	Private Sub PopulateInitialOptions()
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim col As Integer = 0
		Integer.TryParse(KaSetting.GetSetting(connection, "ReceivingActivityReport:" & _currentUser.Id.ToString & "/CheckedOptions", "0"), col)
		cbxDateTime.Checked = ((col And 2 ^ KaReports.ReceivingActivityReportColumns.RcDateTime) <> 0)
		cbxPurchaseOrderNumber.Checked = ((col And 2 ^ KaReports.ReceivingActivityReportColumns.RcOrderNumber) <> 0)
		cbxTicketNumber.Checked = ((col And 2 ^ KaReports.ReceivingActivityReportColumns.RcTicketNumber) <> 0)
		cbxSupplier.Checked = ((col And 2 ^ KaReports.ReceivingActivityReportColumns.RcSupplier) <> 0)
		cbxOwner.Checked = ((col And 2 ^ KaReports.ReceivingActivityReportColumns.RcOwner) <> 0)
		cbxFacility.Checked = ((col And 2 ^ KaReports.ReceivingActivityReportColumns.RcDateTime) <> 0)
		cbxPanel.Checked = ((col And 2 ^ KaReports.ReceivingActivityReportColumns.RcPanel) <> 0)
		cbxDriver.Checked = ((col And 2 ^ KaReports.ReceivingActivityReportColumns.RcDriver) <> 0)
		cbxTransports.Checked = ((col And 2 ^ KaReports.ReceivingActivityReportColumns.RcTransport) <> 0)
		cbxCarrier.Checked = ((col And 2 ^ KaReports.ReceivingActivityReportColumns.RcCarrier) <> 0)
		cbxNotes.Checked = ((col And 2 ^ KaReports.ReceivingActivityReportColumns.RcNotes) <> 0)
		cbxLotNumber.Checked = ((col And 2 ^ KaReports.ReceivingActivityReportColumns.RcLotNumber) <> 0)
		cbxTicketNumber_CheckedChanged(cbxTicketNumber, New EventArgs())

		Try
			ddlUnit.SelectedValue = KaSetting.GetSetting(connection, "ReceivingActivityReport:" & _currentUser.Id.ToString & "/LastUsedUnit", KaUnit.GetSystemDefaultMassUnitOfMeasure(connection, Nothing).ToString())

			If Not Integer.TryParse(KaSetting.GetSetting(connection, "ReceivingActivityReport:" & _currentUser.Id.ToString & "/LastNumberOfDecimals", "0"), ddlDigitsAfterDecimalPoint.SelectedIndex) Then
				ddlUnit_SelectedIndexChanged(ddlUnit, New EventArgs)
			End If
		Catch ex As Exception
			' Unit no longer found, try the default unit
			Try
				ddlUnit.SelectedValue = KaUnit.GetSystemDefaultMassUnitOfMeasure(connection, Nothing).ToString()
			Catch ex2 As Exception

			End Try
			ddlUnit_SelectedIndexChanged(ddlUnit, New EventArgs) ' Whether this was successful, or not, set the digits to the currently selected unit
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
		Dim totalUnitsAndDecimals As String = KaSetting.GetSetting(connection, "ReceivingActivityReport:" & _currentUser.Id.ToString & "/TotalUnitsAndDecimals", tempguid)
		If totalUnitsAndDecimals = tempguid Then
			'Look up original values
			totalUnitsAndDecimals = ""
			If ((col And 2 ^ KaReports.ReceivingActivityReportColumns.RcTicketTotalQuantity) <> 0) Then
				Dim unitId As String = KaSetting.GetSetting(connection, "ReceivingActivityReport:" & _currentUser.Id.ToString & "/LastUsedUnit", Guid.Empty.ToString())
				Try
					Dim unitInfo As New KaUnit(connection, Guid.Parse(unitId))
					Dim precision As String = unitInfo.UnitPrecision
					Dim decimalCount As Integer = 0
					If precision.IndexOf(".") >= 0 Then decimalCount = Math.Max(0, Math.Min(6, precision.Length - precision.IndexOf(".") - 1))
					Integer.TryParse(KaSetting.GetSetting(connection, "ReceivingActivityReport:" & _currentUser.Id.ToString & "/LastNumberOfDecimals", decimalCount), decimalCount)
					totalUnitsAndDecimals = unitId & ":" & decimalCount.ToString() & ":true"
				Catch ex As RecordNotFoundException
				End Try
			End If
			KaSetting.WriteSetting(connection, "ReceivingActivityReport:" & _currentUser.Id.ToString & "/TotalUnitsAndDecimals", totalUnitsAndDecimals)
		End If
		For Each unitItem As String In totalUnitsAndDecimals.Split("|")
			Dim values() As String = unitItem.Split(":")
			If units.ContainsKey(values(0)) AndAlso values.Length > 1 Then Integer.TryParse(values(1), units(values(0)))
			If unitsSelected.ContainsKey(values(0)) AndAlso values.Length > 2 Then Boolean.TryParse(values(2), unitsSelected(values(0)))
		Next
		PopulateTotalUnits(units, unitsSelected)

		Boolean.TryParse(KaSetting.GetSetting(connection, "ReceivingActivityReport:" & _currentUser.Id.ToString & "/IncludeVoidedTickets", False), cbxIncludeVoidedTickets.Checked)
	End Sub

	Protected Sub ddlUnit_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlUnit.SelectedIndexChanged
		Try
			Dim unitInfo As New KaUnit(GetUserConnection(_currentUser.Id), Guid.Parse(ddlUnit.SelectedValue))
			Dim precision As String = unitInfo.UnitPrecision
			Dim decimalCount As Integer = 0
			If precision.IndexOf(".") >= 0 Then decimalCount = Math.Max(0, precision.Length - precision.IndexOf(".") - 1)
			ddlDigitsAfterDecimalPoint.SelectedIndex = Math.Min(decimalCount, ddlDigitsAfterDecimalPoint.Items.Count - 1)
		Catch ex As Exception

		End Try
	End Sub

	Private Function ValidateOptions()
		Dim retVal As Boolean = True
		Dim fromDate As DateTime
		Try
			fromDate = DateTime.Parse(tbxFromDate.Value) ' converting string value to datetime value for comparison in IF statement
		Catch ex As FormatException
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidstartDate", Utilities.JsAlert("Please enter a valid date for the beginning (From) date"))
			Return False
		End Try
		Dim toDate As DateTime
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

		Return retVal
	End Function

	Private Sub SaveOptionsSelected(connection As OleDbConnection, ByRef col As UInt64, unit As KaUnit, ByRef totalUnits As List(Of KaUnit), ByRef totalUnitsAndDecimalsDisplayed As String)
		Dim totalUnitsAndDecimals As String = ""
		KaSetting.WriteSetting(connection, "ReceivingActivityReport:" & _currentUser.Id.ToString & "/CheckedOptions", col)
		KaSetting.WriteSetting(connection, "ReceivingActivityReport:" & _currentUser.Id.ToString & "/LastUsedUnit", ddlUnit.SelectedValue)
		KaSetting.WriteSetting(connection, "ReceivingActivityReport:" & _currentUser.Id.ToString & "/LastNumberOfDecimals", ddlDigitsAfterDecimalPoint.SelectedIndex)
		KaSetting.WriteSetting(connection, "ReceivingActivityReport:" & _currentUser.Id.ToString & "/IncludeVoidedTickets", cbxIncludeVoidedTickets.Checked)

		Dim decimalsDisplayed As Integer = ddlDigitsAfterDecimalPoint.SelectedIndex
		unit.UnitPrecision = "#,###,###0" & IIf(decimalsDisplayed > 0, ".".PadRight(decimalsDisplayed + 1, "0"), "")

		For Each unitItem As ListItem In cblTotalUnits.Items
			If unitItem.Selected Then
				Dim values() As String = unitItem.Value.Split(":")
				Try
					Dim totalUnit As New KaUnit(connection, Guid.Parse(values(0)))
					decimalsDisplayed = 0
					If Integer.TryParse(values(1), decimalsDisplayed) Then
						totalUnit.UnitPrecision = "#,###,###0" & IIf(decimalsDisplayed > 0, ".".PadRight(decimalsDisplayed + 1, "0"), "")
					End If
					If Not totalUnits.Contains(totalUnit) Then totalUnits.Add(totalUnit)
				Catch ex As Exception

				End Try
				If totalUnitsAndDecimalsDisplayed.Length > 0 Then totalUnitsAndDecimalsDisplayed &= "|"
				totalUnitsAndDecimalsDisplayed &= unitItem.Value & ":" & unitItem.Selected.ToString
			End If
			If totalUnitsAndDecimals.Length > 0 Then totalUnitsAndDecimals &= "|"
			totalUnitsAndDecimals &= unitItem.Value & ":" & unitItem.Selected.ToString
		Next
		KaSetting.WriteSetting(connection, "ReceivingActivityReport:" & _currentUser.Id.ToString & "/TotalUnitsAndDecimals", totalUnitsAndDecimals)
	End Sub

	Protected Sub btnSendEmail_Click(sender As Object, e As EventArgs) Handles btnSendEmail.Click
		Dim message As String = ""
		If Not ValidateOptions() Then
			Exit Sub
		End If
		If Not Utilities.IsEmailFieldValid(tbxEmailTo.Text, message) Then
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidEmail", Utilities.JsAlert(message))
			Exit Sub
		End If
		Dim emailAddresses As String = ""
		If tbxEmailTo.Text.Trim().Length > 0 Then
			Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
			Dim col As UInt64 = GetColumnsDisplayed()
			Dim unit As KaUnit = New KaUnit(connection, Guid.Parse(ddlUnit.SelectedValue))
			Dim totalUnits As New List(Of KaUnit)
			Dim totalUnitsAndDecimals As String = ""

			SaveOptionsSelected(connection, col, unit, totalUnits, totalUnitsAndDecimals)

			Dim header As String = GenerateHeader()

			Dim url As String = ""
			If Utilities.ConvertWebPageUrlDomainToRequestedPagesDomain(GetUserConnection(_currentUser.Id)) Then url = Tm2Database.GetCurrentPageUrlWithOriginalPort(Me.Request)
			Dim body As String = KaReports.GetReceivingActivityTable(connection, KaReports.MEDIA_TYPE_HTML, GenerateQuery(), header, col, unit, "", "", "", totalUnits, url, True, True, False)

			Dim emailTo() As String = tbxEmailTo.Text.Split(New Char() {";", ","})
			For Each emailRecipient As String In emailTo
				If emailRecipient.Trim.Length > 0 Then
					Dim newEmail As New KaEmail()
					newEmail.ApplicationId = APPLICATION_ID
					newEmail.Body = Utilities.CreateSiteCssStyle() & body
					newEmail.BodyIsHtml = True
					newEmail.OwnerID = _currentUser.OwnerId
					newEmail.Recipients = emailRecipient.Trim
					newEmail.ReportType = KaEmailReport.ReportTypes.ReceivingActivityReport
					newEmail.Subject = header
					Dim attachments As New List(Of System.Net.Mail.Attachment)
					attachments.Add(New System.Net.Mail.Attachment(New MemoryStream(Encoding.UTF8.GetBytes(newEmail.Body)), "ReceivingActivityReport.html", System.Net.Mime.MediaTypeNames.Text.Html))
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

	Protected Sub cbxTicketNumber_CheckedChanged(sender As Object, e As EventArgs) Handles cbxTicketNumber.CheckedChanged
		cbxIncludeVoidedTickets.Enabled = cbxTicketNumber.Checked
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

	Private Sub PopulateTotalUnits(ByVal units As Dictionary(Of String, Integer), unitsSelected As Dictionary(Of String, Boolean))
		cblTotalUnits.Items.Clear()
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		For Each unitId As String In units.Keys
			Try
				Dim unitInfo As New KaUnit(connection, Guid.Parse(unitId))
				Dim decimalDisplay As String = unitInfo.UnitPrecision
				Dim decimalCount As Integer = units(unitId)
				decimalDisplay = Tm2Database.GeneratePrecisionFormat(1, decimalCount)
				cblTotalUnits.Items.Add(New ListItem(unitInfo.Name & " (" & decimalDisplay & ")", unitId & ":" & units(unitId).ToString()))
				cblTotalUnits.Items(cblTotalUnits.Items.Count - 1).Selected = unitsSelected(unitId)
			Catch ex As RecordNotFoundException

			End Try
		Next
	End Sub

	Protected Sub btnIncreaseTotalDecimalDigits_Click(sender As Object, e As EventArgs) Handles btnIncreaseTotalDecimalDigits.Click
		Dim units As New Dictionary(Of String, Integer)
		Dim unitsSelected As New Dictionary(Of String, Boolean)

		For Each li As ListItem In cblTotalUnits.Items
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

		For Each li As ListItem In cblTotalUnits.Items
			Dim values() As String = li.Value.Split(":")
			units.Add(values(0), values(1))
			unitsSelected.Add(values(0), li.Selected)
		Next
		If units.ContainsKey(ddlTotalUnitsDecimals.SelectedValue) Then units(ddlTotalUnitsDecimals.SelectedValue) = Math.Max(0, units(ddlTotalUnitsDecimals.SelectedValue) - 1)
		PopulateTotalUnits(units, unitsSelected)
	End Sub
End Class
