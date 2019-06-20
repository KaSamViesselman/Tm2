Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb
Imports System.Math
Imports System.IO
Imports KaCommonObjects

Public Class CustomerActivityReport : Inherits System.Web.UI.Page
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
			Dim minDate As New DateTime(Now.Year, Now.Month, Now.Day, 0, 0, 0) ' setting default "From" time to 12:00AM of the current date
			Dim maxDate As DateTime = minDate.AddDays(1) ' setting default "To" time to the current time 
			tbxFromDate.Value = minDate.ToString("G") ' setting "From" datepicker to default times 
			tbxToDate.Value = maxDate.ToString("G") ' setting "To" datepicker to default time
			PopulateCustomerAccountList(_currentUser)
			PopulateProductList(_currentUser)
			PopulateOwnerList(_currentUser)
			PopulateBranchList()
			PopulateFacilityList()
			PopulateBayList()
			PopulateDriverList()
			PopulateTransportList()
			PopulateCarrierList()
			PopulateUnitList()
			PopulateApplicatorList()
			PopulateUserList()
			PopulateInterfaceList()
			PopulateSortList()
			PopulateInitialOptions()
			PopulateEmailAddressList()
		End If
		litEmailConfirmation.Text = ""
	End Sub

	Protected Sub btnShowReport_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnShowReport.Click, btnPrinterFriendly.Click, btnPrintTickets.Click
		If ValidateOptions() Then
			If (sender Is btnPrintTickets) Then
				Dim query As String = GenerateQuery()
				Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
				Dim ownerIds As New List(Of Guid) ' Guid = owner ID
				If (Not String.IsNullOrEmpty(query)) Then
					Dim r As OleDbDataReader = Tm2Database.ExecuteReader(connection, query)
					Do While r.Read()
						Dim ownerId As Guid = r(2)
						If Not ownerIds.Contains(ownerId) Then ownerIds.Add(ownerId)
					Loop
					r.Close()

					For Each ownerId As Guid In ownerIds
						Dim ownerQuery As String = query
						If (Guid.Parse(ddlOwner.SelectedValue).Equals(Guid.Empty)) Then
							ownerQuery = ownerQuery.Replace(" FROM tickets,", " FROM tickets, orders,").Replace(" WHERE tickets.internal_transfer=0 ", " WHERE tickets.internal_transfer=0 AND (tickets.owner_id = " & Q(ownerId) & " OR (tickets.order_id=orders.id AND orders.owner_id=" & Q(ownerId) & ")) ")
						Else
							ownerQuery = ownerQuery.Replace(" AND (tickets.owner_id = " & Q(Guid.Parse(ddlOwner.SelectedValue)) & " OR (tickets.order_id=orders.id AND orders.owner_id=" & Q(Guid.Parse(ddlOwner.SelectedValue)) & "))", " AND (tickets.owner_id = " & Q(ownerId) & " OR (tickets.order_id=orders.id AND orders.owner_id=" & Q(ownerId) & "))")
						End If
						Dim address As String = "PrintTickets.aspx?query=" & Server.UrlEncode(ownerQuery)
						ClientScript.RegisterClientScriptBlock(Me.GetType(), "PrintTickets" & ownerId.ToString.Replace("-", ""), Utilities.JsWindowOpen(address))
					Next
				End If
			Else
				Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
				Dim col As UInt64 = GetColumnsDisplayed()
				Dim unit As KaUnit = New KaUnit(connection, Guid.Parse(ddlUnit.SelectedValue))
				Dim totalUnits As New List(Of KaUnit)
				Dim totalUnitsAndDecimals As String = ""
				SaveOptionsSelected(connection, col, unit, totalUnits, totalUnitsAndDecimals)

				Dim address As String = "CustomerActivityReportView.aspx?interface_id=" & Server.UrlEncode(ddlInterface.SelectedValue)
				address &= "&columns=" & col
				address &= "&decimals_displayed=" & Server.UrlEncode(ddlDigitsAfterDecimalPoint.SelectedIndex)
				address &= "&from_date=" & Server.UrlEncode(tbxFromDate.Value)
				address &= "&to_date=" & Server.UrlEncode(tbxToDate.Value)
				address &= "&product_display=" & Server.UrlEncode(ddlProductDisplayOptions.SelectedIndex)
				address &= "&sort=" & Server.UrlEncode(ddlSort.SelectedIndex)
				If cbxIncludeVoidedTickets.Checked Then address &= "&include_void=" & Server.UrlEncode(cbxIncludeVoidedTickets.Checked.ToString())
				If cbxTicketNumber.Checked Then address &= "&ticket_number=" & Server.UrlEncode(cbxTicketNumber.Checked.ToString())
				address &= "&total_units_displayed=" & Server.UrlEncode(totalUnitsAndDecimals)
				If _currentUser.Id.ToString <> Guid.Empty.ToString Then address &= "&current_user_id=" & Server.UrlEncode(_currentUser.Id.ToString)
				If ddlApplicator.SelectedValue <> Guid.Empty.ToString Then address &= "&applicator_id=" & Server.UrlEncode(ddlApplicator.SelectedValue)
				If ddlBay.SelectedValue <> Guid.Empty.ToString Then address &= "&bay_id=" & Server.UrlEncode(ddlBay.SelectedValue)
				If ddlBranch.SelectedValue <> Guid.Empty.ToString Then address &= "&branch_id=" & Server.UrlEncode(ddlBranch.SelectedValue)
				If ddlCarrier.SelectedValue <> Guid.Empty.ToString Then address &= "&carrier_id=" & Server.UrlEncode(ddlCarrier.SelectedValue)
				If ddlCustomerAccount.SelectedValue <> Guid.Empty.ToString Then address &= "&customer_account_id=" & Server.UrlEncode(ddlCustomerAccount.SelectedValue)
				If ddlCustomerDestination.SelectedValue <> Guid.Empty.ToString Then address &= "&customer_destination_id=" & Server.UrlEncode(ddlCustomerDestination.SelectedValue)
				If ddlDriver.SelectedValue <> Guid.Empty.ToString Then address &= "&driver_id=" & Server.UrlEncode(ddlDriver.SelectedValue)
				If ddlFacility.SelectedValue <> Guid.Empty.ToString Then address &= "&facility_id=" & Server.UrlEncode(ddlFacility.SelectedValue)
				If ddlOwner.SelectedValue <> Guid.Empty.ToString Then address &= "&owner_id=" & Server.UrlEncode(ddlOwner.SelectedValue)
				If ddlProduct.SelectedValue <> Guid.Empty.ToString Then address &= "&product_id=" & Server.UrlEncode(ddlProduct.SelectedValue)
				If ddlTransport.SelectedValue <> Guid.Empty.ToString Then address &= "&transport_id=" & Server.UrlEncode(ddlTransport.SelectedValue)
				If ddlUnit.SelectedValue <> Guid.Empty.ToString Then address &= "&unit_id=" & Server.UrlEncode(ddlUnit.SelectedValue)
				If ddlUser.SelectedIndex > 0 Then address &= "&username=" & Server.UrlEncode(ddlUser.SelectedValue)

				If sender Is btnPrinterFriendly Then
					address &= "&media_type=" & KaReports.MEDIA_TYPE_PFV
				Else
					address &= "&media_type=" & KaReports.MEDIA_TYPE_HTML
				End If
				ClientScript.RegisterStartupScript(Me.GetType(), "CarPfv", Utilities.JsWindowOpen(address))
			End If
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

			Dim commaString As String = KaReports.GetCustomerActivityTable(connection, KaReports.MEDIA_TYPE_COMMA, GenerateQuery(), ddlProductDisplayOptions.SelectedIndex, GenerateHeader(), col, unit, "", "", "", totalUnits, "", True, True, False)

			Dim fileName As String = String.Format("CustomerActivityReport{0:yyyyMMddHHmmss}.csv", Now)

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

	Private Function ValidateOptions()
		Dim retVal As Boolean = True
		Dim fromDate As DateTime
		Try
			fromDate = DateTime.Parse(tbxFromDate.Value) ' converting string value to datetime value for comparison in IF statement
		Catch ex As FormatException
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidStartDate", Utilities.JsAlert("Please enter a valid date for the beginning (From) date"))
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
#End Region

	Private Sub SaveOptionsSelected(ByVal connection As OleDbConnection, ByRef col As Integer, unit As KaUnit, ByRef totalUnits As List(Of KaUnit), ByRef totalUnitsAndDecimalsDisplayed As String)
		Dim totalUnitsAndDecimals As String = ""
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

		KaSetting.WriteSetting(connection, "CustomerActivityReport:" & _currentUser.Id.ToString & "/CheckedOptions", col)
		KaSetting.WriteSetting(connection, "CustomerActivityReport:" & _currentUser.Id.ToString & "/LastUsedUnit", ddlUnit.SelectedValue)
		KaSetting.WriteSetting(connection, "CustomerActivityReport:" & _currentUser.Id.ToString & "/LastNumberOfDecimals", ddlDigitsAfterDecimalPoint.SelectedIndex)
		KaSetting.WriteSetting(connection, "CustomerActivityReport:" & _currentUser.Id.ToString & "/ProductDisplay", ddlProductDisplayOptions.SelectedIndex)
		KaSetting.WriteSetting(connection, "CustomerActivityReport:" & _currentUser.Id.ToString & "/IncludeVoidedTickets", cbxIncludeVoidedTickets.Checked)
		KaSetting.WriteSetting(connection, "CustomerActivityReport:" & _currentUser.Id.ToString & "/TotalUnitsAndDecimals", totalUnitsAndDecimals)
	End Sub

	Private Function GetColumnsDisplayed() As UInt64
		Dim col As UInt64 = 0
		If cbxDateTime.Checked Then col += 2 ^ KaReports.CustomerActivityReportColumns.RcDateTime
		If cbxOrderNumber.Checked Then col += 2 ^ KaReports.CustomerActivityReportColumns.RcOrderNumber
		If cbxTicketNumber.Checked Then col += 2 ^ KaReports.CustomerActivityReportColumns.RcTicketNumber
		If cbxCustomer.Checked Then col += 2 ^ KaReports.CustomerActivityReportColumns.RcCustomer
		If cbxCustomerDestination.Checked Then col += 2 ^ KaReports.CustomerActivityReportColumns.RcCustomerDestination
		If cbxOwner.Checked Then col += 2 ^ KaReports.CustomerActivityReportColumns.RcOwner
		If cbxBranch.Checked Then col += 2 ^ KaReports.CustomerActivityReportColumns.RcBranch
		If cbxFacility.Checked Then col += 2 ^ KaReports.CustomerActivityReportColumns.RcFacility
		If cbxPanel.Checked Then col += 2 ^ KaReports.CustomerActivityReportColumns.RcPanel
		If cbxDriver.Checked Then col += 2 ^ KaReports.CustomerActivityReportColumns.RcDriver
		If cbxTransports.Checked Then col += 2 ^ KaReports.CustomerActivityReportColumns.RcTransport
		If cbxCarrier.Checked Then col += 2 ^ KaReports.CustomerActivityReportColumns.RcCarrier
		If cbxDischargeLocations.Checked Then col += 2 ^ KaReports.CustomerActivityReportColumns.RcDischargeLocations
		If cbxApplicator.Checked Then col += 2 ^ KaReports.CustomerActivityReportColumns.RcApplicator
		If cbxUser.Checked Then col += 2 ^ KaReports.CustomerActivityReportColumns.RcUser
		If cbxInterface.Checked Then col += 2 ^ KaReports.CustomerActivityReportColumns.RcInterface
		If cbxNotes.Checked Then col += 2 ^ KaReports.CustomerActivityReportColumns.RcNotes
		Return col
	End Function

	Private Function GenerateQuery() As String
		Dim customerAccountId As Guid, transportId As Guid, ownerId As Guid, customerDestinationId As Guid, productId As Guid, branchId As Guid, facilityId As Guid, bayId As Guid, driverId As Guid, carrierId As Guid, applicatorId As Guid
		'TryParse will ensure invalid values are set to Guid.empty 
		Guid.TryParse(ddlCustomerAccount.SelectedValue, customerAccountId)
		Guid.TryParse(ddlTransport.SelectedValue, transportId)
		Guid.TryParse(ddlOwner.SelectedValue, ownerId)
		Guid.TryParse(ddlCustomerDestination.SelectedValue, customerDestinationId)
		Guid.TryParse(ddlProduct.SelectedValue, productId)
		Guid.TryParse(ddlBranch.SelectedValue, branchId)
		Guid.TryParse(ddlFacility.SelectedValue, facilityId)
		Guid.TryParse(ddlBay.SelectedValue, bayId)
		Guid.TryParse(ddlDriver.SelectedValue, driverId)
		Guid.TryParse(ddlCarrier.SelectedValue, carrierId)
		Guid.TryParse(ddlApplicator.SelectedValue, applicatorId)

		Return GenerateQuery(ddlInterface.SelectedValue, customerAccountId, transportId, ownerId, customerDestinationId, productId, branchId, facilityId, bayId, driverId, carrierId, applicatorId, IIf(ddlUser.SelectedIndex > 0, ddlUser.SelectedValue, ""), ddlProductDisplayOptions.SelectedIndex, ddlSort.SelectedIndex, cbxIncludeVoidedTickets.Checked, cbxTicketNumber.Checked, tbxFromDate.Value, tbxToDate.Value)
	End Function

	Public Shared Function GenerateQuery(interfaceId As String, customerAccountId As Guid, transportId As Guid, ownerId As Guid, customerDestinationId As Guid, productId As Guid, branchId As Guid, facilityId As Guid, bayId As Guid, driverId As Guid, carrierId As Guid, applicatorId As Guid, userName As String, productDisplayOptionsIndex As Integer, sortIndex As Integer, includeVoidedTickets As Boolean, ticketNumber As Boolean, fromDate As String, toDate As String)
		Dim query As String = "SELECT tickets.id, " & IIf(productDisplayOptionsIndex = 0 Or productDisplayOptionsIndex = 2, "ticket_items.product_id", "ticket_bulk_items.bulk_product_id") &
		 ",tickets.owner_id, tickets.interface_id FROM tickets, ticket_items, ticket_bulk_items" &
		 IIf(customerAccountId.Equals(Guid.Empty), "", ", ticket_customer_accounts") &
		 IIf(transportId.Equals(Guid.Empty), "", ", ticket_transports") &
		 IIf(ownerId.Equals(Guid.Empty), "", ", orders") &
		 " WHERE tickets.internal_transfer=0 AND " &
		 "tickets.loaded_at>=" & Q(fromDate) &
		 " AND tickets.loaded_at<=" & Q(toDate) &
		 " AND ticket_items.ticket_id=tickets.id" &
		 " AND ticket_bulk_items.ticket_item_id=ticket_items.id" &
		 IIf(customerAccountId.Equals(Guid.Empty), "", " AND tickets.id=ticket_customer_accounts.ticket_id AND ticket_customer_accounts.customer_account_id=" & Q(customerAccountId)) &
		 IIf(customerDestinationId.Equals(Guid.Empty), "", " AND tickets.customer_account_location_id=" & Q(customerDestinationId)) &
		 IIf(productId.Equals(Guid.Empty), "", " AND tickets.id=ticket_items.ticket_id AND ticket_items.product_id=" & Q(productId)) &
		 IIf(ownerId.Equals(Guid.Empty), "", " AND (tickets.owner_id = " & Q(ownerId) & " OR (tickets.order_id=orders.id AND orders.owner_id=" & Q(ownerId) & "))") &
		 IIf(branchId.Equals(Guid.Empty), "", " AND tickets.branch_id=" & Q(branchId)) &
		 IIf(facilityId.Equals(Guid.Empty), "", " AND tickets.location_id=" & Q(facilityId)) &
		 IIf(bayId.Equals(Guid.Empty), "", " AND ticket_bulk_items.bay_id=" & Q(bayId)) &
		 IIf(driverId.Equals(Guid.Empty), "", " AND tickets.driver_id=" & Q(driverId)) &
		 IIf(transportId.Equals(Guid.Empty), "", " AND tickets.id=ticket_transports.ticket_id AND ticket_transports.transport_id=" & Q(transportId)) &
		 IIf(carrierId.Equals(Guid.Empty), "", " AND tickets.carrier_id=" & Q(carrierId)) &
		 IIf(applicatorId.Equals(Guid.Empty), "", " AND tickets.applicator_id=" & Q(applicatorId)) &
		 IIf(interfaceId.Equals("-1"), "", " AND tickets.interface_id=" & Q(interfaceId)) &
		 IIf(includeVoidedTickets AndAlso ticketNumber, "", " AND tickets.voided=0") &
		 IIf(userName.Length = 0, "", " AND tickets.username=" & Q(userName)) &
		" ORDER BY " & GetSortBy(sortIndex) & " ASC"
		Return query
	End Function

	Private Function GenerateHeader() As String
		Return GenerateHeader(ddlInterface.SelectedValue, ddlCustomerAccount.SelectedValue, ddlTransport.SelectedValue, ddlOwner.SelectedValue, ddlCustomerDestination.SelectedValue, ddlProduct.SelectedValue, ddlBranch.SelectedValue, ddlFacility.SelectedValue, ddlBay.SelectedValue, ddlDriver.SelectedValue, ddlCarrier.SelectedValue, IIf(ddlUser.SelectedIndex > 0, ddlUser.SelectedValue, ""), _currentUser.Id, tbxFromDate.Value, tbxToDate.Value)
	End Function

	''' <summary>
	''' Use Guid.Empty.ToString for values that are not accessible, not ""
	''' </summary> 
	''' <returns></returns>
	Public Shared Function GenerateHeader(interfaceId As String, customerAccountId As String, transportId As String, ownerId As String, customerDestinationId As String, productId As String, branchId As String, facilityId As String, bayId As String, driverId As String, carrierId As String, userName As String, currentUserId As Guid, fromDate As String, toDate As String) As String
		Dim header As String = "Customer activity report From " & fromDate & " to " & toDate
		If Guid.Parse(customerAccountId) <> Guid.Empty Then
			header &= ", for customer account '" & (New KaCustomerAccount(GetUserConnection(currentUserId), Guid.Parse(customerAccountId))).Name & "'"
		End If
		If Guid.Parse(customerDestinationId) <> Guid.Empty Then
			header &= ", for customer destination '" & (New KaCustomerAccountLocation(GetUserConnection(currentUserId), Guid.Parse(customerDestinationId))).Name & "'"
		End If
		If Guid.Parse(productId) <> Guid.Empty Then
			header &= ", with product '" & (New KaProduct(GetUserConnection(currentUserId), Guid.Parse(productId))).Name & "'"
		End If
		If Guid.Parse(ownerId) <> Guid.Empty Then
			header &= ", for owner '" & (New KaOwner(GetUserConnection(currentUserId), Guid.Parse(ownerId))).Name & "'"
		End If
		If Guid.Parse(branchId) <> Guid.Empty Then
			header &= ", for branch '" & (New KaBranch(GetUserConnection(currentUserId), Guid.Parse(branchId))).Name & "'"
		End If
		If Guid.Parse(facilityId) <> Guid.Empty Then
			header &= ", at the '" & (New KaLocation(GetUserConnection(currentUserId), Guid.Parse(facilityId))).Name & "' facility"
		End If
		If Guid.Parse(bayId) <> Guid.Empty Then
			header &= ", in the '" & (New KaBay(GetUserConnection(currentUserId), Guid.Parse(bayId))).Name & "' bay"
		End If
		If Guid.Parse(driverId) <> Guid.Empty Then
			header &= ", with driver '" & (New KaDriver(GetUserConnection(currentUserId), Guid.Parse(driverId))).Name & "'"
		End If
		If Guid.Parse(transportId) <> Guid.Empty Then
			header &= ", in transport '" & (New KaTransport(GetUserConnection(currentUserId), Guid.Parse(transportId))).Name & "'"
		End If
		If Guid.Parse(carrierId) <> Guid.Empty Then
			header &= ", using carrier '" & (New KaCarrier(GetUserConnection(currentUserId), Guid.Parse(carrierId))).Name & "'"
		End If
		If Not interfaceId.Equals("-1") Then
			If Guid.Parse(interfaceId) = Guid.Empty Then
				header &= ", for manually entered orders"
			Else
				header &= ", from interface '" & (New KaInterface(GetUserConnection(currentUserId), Guid.Parse(interfaceId))).Name & "'"
			End If
		End If
		If userName.Length > 0 Then
			header &= ", loaded by '" & userName & "'"
		End If
		Return header
	End Function

	Private Shared Function GetSortBy(ByVal sortBy As Integer) As String
		Select Case sortBy
			'Case 0 : Return ""
			Case 1 : Return "tickets.loaded_at"
			Case 2 : Return "tickets.driver_name"
				'Case 3 : Return ""
			Case 4 : Return "tickets.order_number"
				'Case 5 : Return ""
				'Case 6 : Return ""
				'Case 7 : Return ""
			Case Else : Return "tickets.loaded_at"
		End Select
	End Function

	Private Sub PopulateNumericList(ByRef ddl As DropDownList, ByVal startValue As Integer, ByVal endValue As Integer, ByVal numericFormat As String)
		ddl.Items.Clear()
		Do While startValue <= endValue
			ddl.Items.Add(New ListItem(Format(startValue, numericFormat), startValue))
			startValue += 1
		Loop
	End Sub

	Private Sub PopulateCustomerAccountList(ByVal currentUser As KaUser)
		ddlCustomerAccount.Items.Clear()
		ddlCustomerAccount.Items.Add(New ListItem("All customer accounts", Guid.Empty.ToString()))
		For Each r As KaCustomerAccount In KaCustomerAccount.GetAll(GetUserConnection(currentUser.Id), "deleted=0 AND is_supplier=0" & IIf(currentUser.OwnerId.Equals(Guid.Empty), "", String.Format(" AND (owner_id={0} OR owner_id={1})", Q(Guid.Empty), Q(currentUser.OwnerId))), "name ASC")
			ddlCustomerAccount.Items.Add(New ListItem(r.Name, r.Id.ToString()))
		Next

		ddlCustomerAccount.SelectedIndex = 0
		ddlCustomerAccount_SelectedIndexChanged(ddlCustomerAccount, New EventArgs)
	End Sub

	Private Sub PopulateProductList(ByVal currentUser As KaUser)
		ddlProduct.Items.Clear()
		ddlProduct.Items.Add(New ListItem("All products", Guid.Empty.ToString()))
		For Each r As KaProduct In KaProduct.GetAll(GetUserConnection(currentUser.Id), "deleted=0" & IIf(currentUser.OwnerId.Equals(Guid.Empty), "", String.Format(" AND (owner_id={0} OR owner_id={1})", Q(Guid.Empty), Q(currentUser.OwnerId))), "name ASC")
			If Not r.IsFunction(GetUserConnection(currentUser.Id)) Then
				ddlProduct.Items.Add(New ListItem(r.Name, r.Id.ToString()))
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
			'ddlTotalUnit.Items.Add(New ListItem(r.Abbreviation, r.Id.ToString()))
			If r.BaseUnit = KaUnit.Unit.Pounds AndAlso r.Factor = 1 Then ' This should be Pounds
				ddlUnit.SelectedIndex = ddlUnit.Items.Count - 1
				'ddlTotalUnit.SelectedIndex = ddlUnit.Items.Count - 1
			End If
		Next
	End Sub

	Private Sub PopulateApplicatorList()
		ddlApplicator.Items.Clear()
		ddlApplicator.Items.Add(New ListItem("All applicators", Guid.Empty.ToString))
		For Each a As KaApplicator In KaApplicator.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name asc")
			ddlApplicator.Items.Add(New ListItem(a.Name, a.Id.ToString))
		Next
	End Sub

	Private Sub PopulateUserList()
		ddlUser.Items.Clear()
		ddlUser.Items.Add("All users")
		Dim activeUsersRdr As OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), "SELECT username FROM tickets WHERE username > '' union SELECT name AS username FROM users WHERE name >'' AND deleted = 0 ORDER BY username")
		Do While activeUsersRdr.Read()
			ddlUser.Items.Add(activeUsersRdr.Item("username"))
		Loop
	End Sub

	Private Sub PopulateInterfaceList()
		ddlInterface.Items.Clear()
		ddlInterface.Items.Add(New ListItem("All interfaces", "-1"))
		ddlInterface.Items.Add(New ListItem("No interface", Guid.Empty.ToString))
		For Each i As KaInterface In KaInterface.GetAll(Tm2Database.Connection, "deleted=0", "name asc")
			ddlInterface.Items.Add(New ListItem(i.Name, i.Id.ToString))
		Next
	End Sub

	Private Sub PopulateSortList()
		ddlSort.Items.Clear()
		ddlSort.Items.Add("Customer account")
		ddlSort.Items.Add("Date/time")
		ddlSort.Items.Add("Driver")
		ddlSort.Items.Add("Facility")
		ddlSort.Items.Add("Order number")
		ddlSort.Items.Add("Owner")
		ddlSort.Items.Add("Panel")
		ddlSort.Items.Add("Product")
		ddlSort.SelectedIndex = 1
	End Sub

	Private Sub ddlCustomerAccount_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles ddlCustomerAccount.SelectedIndexChanged
		ddlCustomerDestination.Items.Clear()
		ddlCustomerDestination.Items.Add(New ListItem("All customer destinations", Guid.Empty.ToString()))
		For Each r As KaCustomerAccountLocation In KaCustomerAccountLocation.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND (customer_account_id=" & Q(Guid.Empty) & " OR customer_account_id=" & Q(Guid.Parse(ddlCustomerAccount.SelectedValue.ToString())) & ")", "name ASC")
			ddlCustomerDestination.Items.Add(New ListItem(r.Name, r.Id.ToString()))
		Next
	End Sub

	Private Sub PopulateBayList()
		ddlBay.Items.Clear()
		ddlBay.Items.Add(New ListItem("All bays", Guid.Empty.ToString()))
		For Each r As KaBay In KaBay.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
			ddlBay.Items.Add(New ListItem(r.Name, r.Id.ToString()))
		Next

	End Sub

	Private Sub PopulateBranchList()
		ddlBranch.Items.Clear()
		ddlBranch.Items.Add(New ListItem("All branches", Guid.Empty.ToString()))
		For Each r As KaBranch In KaBranch.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
			ddlBranch.Items.Add(New ListItem(r.Name, r.Id.ToString()))
		Next
	End Sub

	Private Sub PopulateInitialOptions()
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim col As Integer = 0
		Integer.TryParse(KaSetting.GetSetting(connection, "CustomerActivityReport:" & _currentUser.Id.ToString & "/CheckedOptions", "0"), col)
		cbxDateTime.Checked = ((col And 2 ^ KaReports.CustomerActivityReportColumns.RcDateTime) <> 0)
		cbxOrderNumber.Checked = ((col And 2 ^ KaReports.CustomerActivityReportColumns.RcOrderNumber) <> 0)
		cbxTicketNumber.Checked = ((col And 2 ^ KaReports.CustomerActivityReportColumns.RcTicketNumber) <> 0)
		cbxCustomer.Checked = ((col And 2 ^ KaReports.CustomerActivityReportColumns.RcCustomer) <> 0)
		cbxCustomerDestination.Checked = ((col And 2 ^ KaReports.CustomerActivityReportColumns.RcCustomerDestination) <> 0)
		cbxOwner.Checked = ((col And 2 ^ KaReports.CustomerActivityReportColumns.RcOwner) <> 0)
		cbxBranch.Checked = ((col And 2 ^ KaReports.CustomerActivityReportColumns.RcBranch) <> 0)
		cbxFacility.Checked = ((col And 2 ^ KaReports.CustomerActivityReportColumns.RcFacility) <> 0)
		cbxPanel.Checked = ((col And 2 ^ KaReports.CustomerActivityReportColumns.RcPanel) <> 0)
		cbxDriver.Checked = ((col And 2 ^ KaReports.CustomerActivityReportColumns.RcDriver) <> 0)
		cbxTransports.Checked = ((col And 2 ^ KaReports.CustomerActivityReportColumns.RcTransport) <> 0)
		cbxCarrier.Checked = ((col And 2 ^ KaReports.CustomerActivityReportColumns.RcCarrier) <> 0)
		cbxDischargeLocations.Checked = ((col And 2 ^ KaReports.CustomerActivityReportColumns.RcDischargeLocations) <> 0)
		cbxApplicator.Checked = ((col And 2 ^ KaReports.CustomerActivityReportColumns.RcApplicator) <> 0)
		cbxUser.Checked = ((col And 2 ^ KaReports.CustomerActivityReportColumns.RcUser) <> 0)
		cbxInterface.Checked = ((col And 2 ^ KaReports.CustomerActivityReportColumns.RcInterface) <> 0)
		cbxNotes.Checked = ((col And 2 ^ KaReports.CustomerActivityReportColumns.RcNotes) <> 0)

		Try
			ddlUnit.SelectedValue = KaSetting.GetSetting(connection, "CustomerActivityReport:" & _currentUser.Id.ToString & "/LastUsedUnit", KaUnit.GetSystemDefaultMassUnitOfMeasure(connection, Nothing).ToString())

			If Not Integer.TryParse(KaSetting.GetSetting(connection, "CustomerActivityReport:" & _currentUser.Id.ToString & "/LastNumberOfDecimals", "0"), ddlDigitsAfterDecimalPoint.SelectedIndex) Then
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
		Dim totalUnitsAndDecimals As String = KaSetting.GetSetting(connection, "CustomerActivityReport:" & _currentUser.Id.ToString & "/TotalUnitsAndDecimals", tempguid)
		If totalUnitsAndDecimals = tempguid Then
			'Look up original values
			totalUnitsAndDecimals = ""
			If ((col And 2 ^ KaReports.CustomerActivityReportColumns.RcTicketTotalQuantity) <> 0) Then
				Dim unitId As String = KaSetting.GetSetting(connection, "CustomerActivityReport:" & _currentUser.Id.ToString & "/LastTotalUsedUnit", Guid.Empty.ToString())
				Try
					Dim unitInfo As New KaUnit(connection, Guid.Parse(unitId))
					Dim precision As String = unitInfo.UnitPrecision
					Dim decimalCount As Integer = 0
					If precision.IndexOf(".") >= 0 Then decimalCount = Math.Max(0, Math.Min(6, precision.Length - precision.IndexOf(".") - 1))
					Integer.TryParse(KaSetting.GetSetting(connection, "CustomerActivityReport:" & _currentUser.Id.ToString & "/LastTotalNumberOfDecimals", decimalCount), decimalCount)
					totalUnitsAndDecimals = unitId & ":" & decimalCount.ToString() & ":true"
				Catch ex As RecordNotFoundException
				End Try
			End If
			KaSetting.WriteSetting(connection, "CustomerActivityReport:" & _currentUser.Id.ToString & "/TotalUnitsAndDecimals", totalUnitsAndDecimals)
		End If
		For Each unitItem As String In totalUnitsAndDecimals.Split("|")
			Dim values() As String = unitItem.Split(":")
			If units.ContainsKey(values(0)) AndAlso values.Length > 1 Then Integer.TryParse(values(1), units(values(0)))
			If unitsSelected.ContainsKey(values(0)) AndAlso values.Length > 2 Then Boolean.TryParse(values(2), unitsSelected(values(0)))
		Next
		PopulateTotalUnits(units, unitsSelected)

		Try
			If Not Integer.TryParse(KaSetting.GetSetting(connection, "CustomerActivityReport:" & _currentUser.Id.ToString & "/ProductDisplay", "0"), ddlProductDisplayOptions.SelectedIndex) Then ddlProductDisplayOptions.SelectedIndex = 0
		Catch ex As Exception
			ddlProductDisplayOptions.SelectedIndex = 0
		End Try
		ddlProductDisplayOptions_SelectedIndexChanged(ddlProductDisplayOptions, New EventArgs())

		Boolean.TryParse(KaSetting.GetSetting(connection, "CustomerActivityReport:" & _currentUser.Id.ToString & "/IncludeVoidedTickets", False), cbxIncludeVoidedTickets.Checked)
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

	Protected Sub btnSendEmail_Click(sender As Object, e As EventArgs) Handles btnSendEmail.Click
		If Not ValidateOptions() Then
			Exit Sub
		End If
		Dim message As String = ""
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

			Dim url As String = ""
			If Utilities.ConvertWebPageUrlDomainToRequestedPagesDomain(GetUserConnection(_currentUser.Id)) Then url = Tm2Database.GetCurrentPageUrlWithOriginalPort(Me.Request)
			Dim header As String = GenerateHeader()
			Dim body As String = KaReports.GetCustomerActivityTable(connection, KaReports.MEDIA_TYPE_HTML, GenerateQuery(), ddlProductDisplayOptions.SelectedIndex, header, col, unit, "", "", "", totalUnits, url, True, True, False)

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
					attachments.Add(New System.Net.Mail.Attachment(New MemoryStream(Encoding.UTF8.GetBytes(newEmail.Body)), "CustomerActivityReport.html", System.Net.Mime.MediaTypeNames.Text.Html))
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

	Protected Sub cbxTicketNumber_CheckedChanged(sender As Object, e As EventArgs) Handles cbxTicketNumber.CheckedChanged
		If cbxTicketNumber.Checked Then
			cbxIncludeVoidedTickets.Enabled = True
		Else
			cbxIncludeVoidedTickets.Enabled = False
			cbxIncludeVoidedTickets.Checked = False
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

	Private Sub ddlProductDisplayOptions_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles ddlProductDisplayOptions.SelectedIndexChanged
		If ddlProductDisplayOptions.SelectedValue = "2" Then
			pnlUnitDisplay.Visible = False
			pnlUnitDecimalDisplay.Visible = False
		Else
			pnlUnitDisplay.Visible = True
			pnlUnitDecimalDisplay.Visible = True
		End If
	End Sub
End Class
