Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb
Imports System.IO
Imports System.Xml

Public Class EmailReports : Inherits System.Web.UI.Page
#Region "Events"
	Private _currentUser As KaUser
	Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
	Private _currentTableName As String = KaEmail.TABLE_NAME

	Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
		Response.Cache.SetCacheability(HttpCacheability.NoCache)
		_currentUser = Utilities.GetUser(Me)
		_currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Emails")
		If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")
		'litTabs.Text = Utilities.BuildAuthorizedMenuAndSetPageTitle(Me) ' get current user permissions and build menu accordingly
		If Not Page.IsPostBack Then
			btnSave.Attributes.Add("onclick", "SaveIframeData();")
			SetTextboxMaxLengths()
			PopulateEmailReports()
			PopulateTransportTrackingOrderBy()
			PopulateTransportTrackingAscDesc()
			PopulateOrderListOrderBy()
			PopulateOrderListAscDesc()
			PopulateReceivingPurchaseOrderListOrderBy()
			PopulateReceivingPurchaseOrderListAscDesc()
			PopulateOrderListReportTypes()
			PopulateCustomReports()
			PopulateDatesInMonth()
			ddlEmailReport_SelectedIndexChanged(Nothing, Nothing)
			PopulateCustomReportConfig(Guid.Empty)
			Utilities.ConfirmBox(Me.btnDelete, "Are you sure you want to delete this e-mail?")
			PopulateEmailAddressList()
			PopulateDaysOfWeek()
		End If
		lblStatus.Text = ""
	End Sub

	Private Sub PopulateTransportTrackingOrderBy()
		ddlTransportTrackingOrderBy.Items.Clear()
		ddlTransportTrackingOrderBy.Items.Add(New ListItem("Transport", "name"))
		ddlTransportTrackingOrderBy.Items.Add(New ListItem("Loaded date", "loaded_at"))
		ddlTransportTrackingOrderBy.Items.Add(New ListItem("Ticket number", "number"))
		ddlTransportTrackingOrderBy.SelectedIndex = 0
	End Sub

	Private Sub PopulateTransportTrackingAscDesc()
		ddlTransportTrackingAscDesc.Items.Clear()
		ddlTransportTrackingAscDesc.Items.Add(New ListItem("Asc", "asc"))
		ddlTransportTrackingAscDesc.Items.Add(New ListItem("Desc", "desc"))
		ddlTransportTrackingAscDesc.SelectedIndex = 0
	End Sub

	Private Sub PopulateOrderListOrderBy()
		ddlOrderListOrderBy.Items.Clear()
		ddlOrderListOrderBy.Items.Add(New ListItem("Order number", "order_number"))
		ddlOrderListOrderBy.Items.Add(New ListItem("Account name", "customer_accounts_name"))
		ddlOrderListOrderBy.Items.Add(New ListItem("Product name", "products_name"))
		ddlOrderListOrderBy.Items.Add(New ListItem("Owner", "owners_name"))
		ddlOrderListOrderBy.Items.Add(New ListItem("Created", "order_created"))
		ddlOrderListOrderBy.SelectedIndex = 0
	End Sub

	Private Sub PopulateOrderListAscDesc()
		ddlOrderListAscDesc.Items.Clear()
		ddlOrderListAscDesc.Items.Add(New ListItem("Asc", "asc"))
		ddlOrderListAscDesc.Items.Add(New ListItem("Desc", "desc"))
		ddlOrderListAscDesc.SelectedIndex = 0
	End Sub

	Private Sub PopulateReceivingPurchaseOrderListOrderBy()
		ddlReceivingPurchaseOrderListOrderBy.Items.Clear()
		ddlReceivingPurchaseOrderListOrderBy.Items.Add(New ListItem("Purchase order number", "purchase_order_number"))
		ddlReceivingPurchaseOrderListOrderBy.Items.Add(New ListItem("Supplier name", "supplier_name"))
		ddlReceivingPurchaseOrderListOrderBy.Items.Add(New ListItem("Bulk product name", "bulk_product_name"))
		ddlReceivingPurchaseOrderListOrderBy.Items.Add(New ListItem("Owner", "owner_name"))
		ddlReceivingPurchaseOrderListOrderBy.Items.Add(New ListItem("Created", "purchase_order_created"))
		ddlReceivingPurchaseOrderListOrderBy.SelectedIndex = 0
	End Sub

	Private Sub PopulateReceivingPurchaseOrderListAscDesc()
		ddlReceivingPurchaseOrderListAscDesc.Items.Clear()
		ddlReceivingPurchaseOrderListAscDesc.Items.Add(New ListItem("Asc", "asc"))
		ddlReceivingPurchaseOrderListAscDesc.Items.Add(New ListItem("Desc", "desc"))
		ddlReceivingPurchaseOrderListAscDesc.SelectedIndex = 0
	End Sub

	Private Sub PopulateOrderListReportTypes()
		ddlOrderListReportType.Items.Clear()
		ddlOrderListReportType.Items.Add(New ListItem("Each product has individual column", KaReports.OrderListReportType.OneProductPerColumn.ToString()))
		ddlOrderListReportType.Items.Add(New ListItem("Each order item has individual row", KaReports.OrderListReportType.OneProductPerLine.ToString()))
		ddlOrderListReportType.Items.Add(New ListItem("Each order has single row", KaReports.OrderListReportType.MultipleProductsOneColumn.ToString()))
		ddlOrderListReportType.SelectedIndex = 0
	End Sub

	Private Sub PopulateCustomReports()
		Dim allCustomReports As ArrayList = KaCustomPages.GetAll(GetUserConnection(_currentUser.Id), "email_report = " & Q(True) & " and deleted = " & Q(False), "page_label asc")
		For Each customReport As KaCustomPages In allCustomReports
			ddlReportType.Items.Add(New ListItem(customReport.PageLabel, customReport.Id.ToString))
		Next
	End Sub

	Private Sub PopulateTicketSort(rt As KaEmailReport.ReportTypes)
		ddlTicketSort.Items.Clear()
		ddlTicketSort.Items.Add(New ListItem("Exported date (Asc)", "exported_at ASC"))
		ddlTicketSort.Items.Add(New ListItem("Exported date (Desc)", "exported_at DESC"))
		If rt = KaEmailReport.ReportTypes.InterfaceTicketReceivingExportStatusReport Then
			ddlTicketSort.Items.Add(New ListItem("Received date (Asc)", "date_of_delivery ASC"))
			ddlTicketSort.Items.Add(New ListItem("Received date (Desc)", "date_of_delivery DESC"))
		Else
			ddlTicketSort.Items.Add(New ListItem("Loaded date (Asc)", "loaded_at ASC"))
			ddlTicketSort.Items.Add(New ListItem("Loaded date (Desc)", "loaded_at DESC"))
		End If
		ddlTicketSort.Items.Add(New ListItem("Ticket number (Asc)", "number ASC"))
		ddlTicketSort.Items.Add(New ListItem("Ticket number (Desc)", "number DESC"))
	End Sub

	Protected Sub ddlEmailReport_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlEmailReport.SelectedIndexChanged
		lblStatus.Text = ""
		If ddlEmailReport.SelectedIndex >= 0 Then
			Dim id As Guid = Guid.Parse(ddlEmailReport.SelectedValue)
			Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
			Dim report As KaEmailReport
			Try
				report = New KaEmailReport(connection, id)
				btnDelete.Enabled = _currentUserPermission(_currentTableName).Delete
				pnlReportTriggers.Enabled = True
			Catch ex As RecordNotFoundException
				report = New KaEmailReport()
				report.Name = "New e-mail report"
				report.ReportType = KaEmailReport.ReportTypes.CarrierList
				report.ReportRunType = KaEmailReport.ReportRunTypes.Email
				btnDelete.Enabled = False
				pnlReportTriggers.Enabled = False
			End Try

			tbxName.Text = report.Name
			litId.Text = IIf(report.Id = Guid.Empty, "", report.Id.ToString())
			tbxRecipients.Text = report.Recipients
			tbxReportDomainURL.Text = report.ReportDomainURL
			tbxSubject.Text = report.Subject
			cbxDisabled.Checked = report.Disabled
			cbxMonthToDate.Checked = report.IsMonthToDate
			tbxLastSent.Value = report.LastSent
			Select Case report.ReportRunType
				Case KaEmailReport.ReportRunTypes.SaveAsFile
					ddlReportRuntype.SelectedValue = "SaveFile"
				Case Else
					ddlReportRuntype.SelectedIndex = 0
			End Select
			tbxReportFileSaveLocation.Text = report.FileSaveLocation
			ddlReportRuntype_SelectedIndexChanged(ddlReportRuntype, New EventArgs())

			If report.CustomPageId <> Guid.Empty Then
				Try
					ddlReportType.SelectedValue = report.CustomPageId.ToString
				Catch ex As Exception
					'Suppress
				End Try
			Else
				Try
					ddlReportType.SelectedValue = report.ReportType.ToString()
				Catch ex As Exception
				End Try
			End If

			ddlReportType_SelectedIndexChanged(Nothing, Nothing)
			If report.ReportType = KaEmailReport.ReportTypes.BulkProductUsageReport OrElse
			   report.ReportType = KaEmailReport.ReportTypes.CarrierList OrElse
			   report.ReportType = KaEmailReport.ReportTypes.ContainerList OrElse
			   report.ReportType = KaEmailReport.ReportTypes.ContainerHistory OrElse
			   report.ReportType = KaEmailReport.ReportTypes.CustomerActivityReport OrElse
			   report.ReportType = KaEmailReport.ReportTypes.DriverList OrElse
			   report.ReportType = KaEmailReport.ReportTypes.InterfaceTicketExportStatusReport OrElse
			   report.ReportType = KaEmailReport.ReportTypes.InterfaceTicketReceivingExportStatusReport OrElse
			   report.ReportType = KaEmailReport.ReportTypes.Inventory OrElse
			   report.ReportType = KaEmailReport.ReportTypes.InventoryChangeReport OrElse
			   report.ReportType = KaEmailReport.ReportTypes.OrderList OrElse
			   report.ReportType = KaEmailReport.ReportTypes.ProductAllocation OrElse
			   report.ReportType = KaEmailReport.ReportTypes.ReceivingActivityReport OrElse
			   report.ReportType = KaEmailReport.ReportTypes.TankLevelTrend OrElse
			   report.ReportType = KaEmailReport.ReportTypes.TransportList OrElse
			   report.ReportType = KaEmailReport.ReportTypes.TransportUsageReport OrElse
			   report.ReportType = KaEmailReport.ReportTypes.TankLevels OrElse
			   report.ReportType = KaEmailReport.ReportTypes.TankAlarmHistory OrElse
			   report.ReportType = KaEmailReport.ReportTypes.TransportTrackingReport OrElse
			   report.ReportType = KaEmailReport.ReportTypes.CustomReport OrElse
			   report.ReportType = KaEmailReport.ReportTypes.ReceivingPurchaseOrderList Then
				ddlFormat.SelectedValue = GetParameter("format", report.ReportParameters, KaReports.MEDIA_TYPE_HTML)
			End If
			If report.ReportType = KaEmailReport.ReportTypes.CustomerActivityReport Then
				Try
					ddlApplicator.SelectedValue = GetParameter("applicator_id", report.ReportParameters, Guid.Empty.ToString())
				Catch ex As Exception ' suppress exception
				End Try
			End If
			If report.ReportType = KaEmailReport.ReportTypes.CustomerActivityReport OrElse
				report.ReportType = KaEmailReport.ReportTypes.BulkProductUsageReport Then
				Try
					ddlBay.SelectedValue = GetParameter("bay_id", report.ReportParameters, Guid.Empty.ToString())
				Catch ex As Exception ' suppress exception
				End Try
			End If
			If report.ReportType = KaEmailReport.ReportTypes.Inventory OrElse
			   report.ReportType = KaEmailReport.ReportTypes.InventoryChangeReport OrElse
			   report.ReportType = KaEmailReport.ReportTypes.ReceivingActivityReport OrElse
			   report.ReportType = KaEmailReport.ReportTypes.TankLevels OrElse
			   report.ReportType = KaEmailReport.ReportTypes.TransportTrackingReport OrElse
				report.ReportType = KaEmailReport.ReportTypes.ReceivingPurchaseOrderList Then
				Try
					ddlBulkProduct.SelectedValue = GetParameter("bulk_product_id", report.ReportParameters, Guid.Empty.ToString())
				Catch ex As Exception ' suppress exception
				End Try
			End If
			If report.ReportType = KaEmailReport.ReportTypes.CustomerActivityReport OrElse
			   report.ReportType = KaEmailReport.ReportTypes.TransportTrackingReport OrElse
			   report.ReportType = KaEmailReport.ReportTypes.TransportTrackingReport Then
				Try
					ddlCarrier.SelectedValue = GetParameter("carrier_id", report.ReportParameters, Guid.Empty.ToString())
				Catch ex As Exception ' suppress exception
				End Try
			End If
			If report.ReportType = KaEmailReport.ReportTypes.ContainerHistory Then
				Try
					ddlContainer.SelectedValue = GetParameter("container_id", report.ReportParameters, Guid.Empty.ToString())
				Catch ex As Exception ' suppress exception
				End Try
			End If
			If report.ReportType = KaEmailReport.ReportTypes.ContainerList OrElse
				report.ReportType = KaEmailReport.ReportTypes.ContainerHistory Then
				Dim columnsDisplayed As ULong = GetDisplayedContainerColumns()
				columnsDisplayed = GetParameter("columns", report.ReportParameters, columnsDisplayed)
				FillContainerColumnDisplayed(columnsDisplayed)
			End If
			If report.ReportType = KaEmailReport.ReportTypes.CustomerActivityReport OrElse
			   report.ReportType = KaEmailReport.ReportTypes.ProductAllocation OrElse
			   report.ReportType = KaEmailReport.ReportTypes.TransportUsageReport OrElse
			   report.ReportType = KaEmailReport.ReportTypes.OrderList Then
				Try
					ddlCustomerAccount.SelectedValue = GetParameter("customer_account_id", report.ReportParameters, Guid.Empty.ToString())
					ddlCustomerAccount_SelectedIndexChanged(ddlCustomerAccount, New EventArgs)
				Catch ex As Exception ' suppress exception
				End Try
			End If

			If report.ReportType = KaEmailReport.ReportTypes.ReceivingActivityReport OrElse
				 report.ReportType = KaEmailReport.ReportTypes.ReceivingPurchaseOrderList Then
				Try
					ddlSupplierAccount.SelectedValue = GetParameter("supplier_account_id", report.ReportParameters, Guid.Empty.ToString())
				Catch ex As Exception ' suppress exception
				End Try
			End If

			If report.ReportType = KaEmailReport.ReportTypes.CustomerActivityReport OrElse
			   report.ReportType = KaEmailReport.ReportTypes.ReceivingActivityReport OrElse
			   report.ReportType = KaEmailReport.ReportTypes.TransportTrackingReport Then
				Try
					ddlDriver.SelectedValue = GetParameter("driver_id", report.ReportParameters, Guid.Empty.ToString())
				Catch ex As Exception ' suppress exception
				End Try
			End If
			If report.ReportType = KaEmailReport.ReportTypes.CustomerActivityReport OrElse
			   report.ReportType = KaEmailReport.ReportTypes.TransportTrackingReport OrElse
			   report.ReportType = KaEmailReport.ReportTypes.Inventory OrElse
			   report.ReportType = KaEmailReport.ReportTypes.InventoryChangeReport OrElse
			   report.ReportType = KaEmailReport.ReportTypes.ProductAllocation OrElse
			   report.ReportType = KaEmailReport.ReportTypes.ProductList OrElse
			   report.ReportType = KaEmailReport.ReportTypes.ReceivingActivityReport OrElse
			   report.ReportType = KaEmailReport.ReportTypes.ReceivingPurchaseOrderList OrElse
			   report.ReportType = KaEmailReport.ReportTypes.OrderList OrElse
			   report.ReportType = KaEmailReport.ReportTypes.TankLevels Then
				Try
					ddlLocation.SelectedValue = GetParameter("location_id", report.ReportParameters, Guid.Empty.ToString())
				Catch ex As Exception ' suppress exception
				End Try
			End If
			If report.ReportType = KaEmailReport.ReportTypes.Inventory Then
				Boolean.TryParse(GetParameter("only_show_bulk_products_with_non_zero_inventory", report.ReportParameters, True.ToString()), cbxOnlyShowBulkProductsWithNonZeroInventory.Checked)
				Boolean.TryParse(GetParameter("assign_physical_inventory_to_owner", report.ReportParameters, True.ToString()), cbxAssignPhysicalInventoryToOwner.Checked)
			End If
			Try
				Dim bogusId As Guid = Guid.NewGuid
				Dim ownerId As Guid = Guid.Parse(GetParameter("owner_id", report.ReportParameters, bogusId.ToString()))
				If ownerId = bogusId Then
#Disable Warning BC40000
					'For backwards compatibility
					ownerId = report.OwnerId
#Enable Warning BC40000 ' Type or member is obsolete
				End If
				ddlOwner.SelectedValue = ownerId.ToString
			Catch ex As Exception ' suppress exception
			End Try
			If report.ReportType = KaEmailReport.ReportTypes.BulkProductUsageReport Then
				Try
					ddlPanel.SelectedValue = GetParameter("panel_id", report.ReportParameters, Guid.Empty.ToString())
				Catch ex As Exception ' suppress exception
				End Try
				If Not Boolean.TryParse(GetParameter("all_bulk_products", report.ReportParameters, True), cbxIncludeAllBulkProducts.Checked) Then cbxIncludeAllBulkProducts.Checked = True

				Dim bulkProductList As New List(Of Guid)
				Try
					bulkProductList = Tm2Database.FromXml(GetParameter("bulk_product_ids", report.ReportParameters, ""), GetType(List(Of Guid)))
				Catch ex As Exception

				End Try
				For Each bulkProd As ListItem In cblBulkProductList.Items
					Dim bulkProdId As Guid = Guid.Empty

					bulkProd.Selected = Guid.TryParse(bulkProd.Value, bulkProdId) AndAlso bulkProductList.Contains(bulkProdId)
				Next
				cbxIncludeAllBulkProducts_CheckedChanged(cbxIncludeAllBulkProducts, New EventArgs())

				Boolean.TryParse(GetParameter("include_voided_tickets", report.ReportParameters, False), cbxIncludeVoidedTickets.Checked)
			End If
			If report.ReportType = KaEmailReport.ReportTypes.CustomerActivityReport OrElse
			   report.ReportType = KaEmailReport.ReportTypes.ProductAllocation Then
				Try
					ddlProduct.SelectedValue = GetParameter("product_id", report.ReportParameters, Guid.Empty.ToString())
				Catch ex As Exception ' suppress exception
				End Try
			End If
			If report.ReportType = KaEmailReport.ReportTypes.TankAlarmHistory Then
				Try
					ddlTank.SelectedValue = GetParameter("tank_id", report.ReportParameters, Guid.Empty.ToString())
				Catch ex As Exception ' suppress exception
				End Try
			End If
			If report.ReportType = KaEmailReport.ReportTypes.TankLevelTrend Then
				Try
					ddlTankLevelTrend.SelectedValue = GetParameter("tank_level_trend_id", report.ReportParameters, Guid.Empty.ToString())
				Catch ex As Exception ' suppress exception
				End Try
				cbxShowTemperature.Checked = False
				Try
					Boolean.TryParse(GetParameter("show_temperature", report.ReportParameters, False.ToString()), cbxShowTemperature.Checked)
				Catch ex As Exception ' suppress exception
				End Try
			End If
			If report.ReportType = KaEmailReport.ReportTypes.CustomerActivityReport OrElse
			   report.ReportType = KaEmailReport.ReportTypes.ReceivingActivityReport OrElse
			   report.ReportType = KaEmailReport.ReportTypes.TransportTrackingReport OrElse
			   report.ReportType = KaEmailReport.ReportTypes.TransportUsageReport Then
				Try
					ddlTransport.SelectedValue = GetParameter("transport_id", report.ReportParameters, Guid.Empty.ToString())
				Catch ex As Exception ' suppress exception
				End Try
			End If
			If report.ReportType = KaEmailReport.ReportTypes.CustomerActivityReport OrElse
			   report.ReportType = KaEmailReport.ReportTypes.ProductAllocation OrElse
			   report.ReportType = KaEmailReport.ReportTypes.ReceivingActivityReport OrElse
			   report.ReportType = KaEmailReport.ReportTypes.TransportTrackingReport Then
				Try
					ddlUnit.SelectedValue = GetParameter("unit_id", report.ReportParameters, Guid.Empty.ToString())
					ddlUnit_SelectedIndexChanged(ddlUnit, New EventArgs)
				Catch ex As Exception ' suppress exception
				End Try
			End If
			If report.ReportType = KaEmailReport.ReportTypes.TankLevelTrend OrElse
				report.ReportType = KaEmailReport.ReportTypes.TankLevels OrElse
				report.ReportType = KaEmailReport.ReportTypes.TransportTrackingReport Then
				Try
					ddlDisplayUnit.SelectedValue = GetParameter("display_unit_id", report.ReportParameters, Guid.Empty.ToString())
				Catch ex As Exception ' suppress exception
				End Try
			End If
			If report.ReportType = KaEmailReport.ReportTypes.CustomerActivityReport Then
				Try
					If Not Integer.TryParse(GetParameter("product_display_options", report.ReportParameters, IIf(Not Boolean.Parse(GetParameter("show_bulk_products", report.ReportParameters, "false")), "0", "1")), ddlProductDisplayOptions.SelectedIndex) Then ddlProductDisplayOptions.SelectedIndex = 0
				Catch ex As Exception
					ddlProductDisplayOptions.SelectedIndex = 0
				End Try

				Try
					ddlSortBy.SelectedValue = GetParameter("sort_by", report.ReportParameters, "tickets.loaded_at")
				Catch ex As Exception ' suppress exception
				End Try
				Dim columns As ULong = ULong.Parse(GetParameter("columns", report.ReportParameters, Long.MaxValue.ToString()))
				cbxDateTime.Checked = ((columns And 2 ^ KaReports.CustomerActivityReportColumns.RcDateTime) <> 0)
				cbxOrderNumber.Checked = ((columns And 2 ^ KaReports.CustomerActivityReportColumns.RcOrderNumber) <> 0)
				cbxTicketNumber.Checked = ((columns And 2 ^ KaReports.CustomerActivityReportColumns.RcTicketNumber) <> 0)
				cbxCustomer.Checked = ((columns And 2 ^ KaReports.CustomerActivityReportColumns.RcCustomer) <> 0)
				cbxCustomerDestination.Checked = ((columns And 2 ^ KaReports.CustomerActivityReportColumns.RcCustomerDestination) <> 0)
				cbxOwner.Checked = ((columns And 2 ^ KaReports.CustomerActivityReportColumns.RcOwner) <> 0)
				cbxBranch.Checked = ((columns And 2 ^ KaReports.CustomerActivityReportColumns.RcBranch) <> 0)
				cbxFacility.Checked = ((columns And 2 ^ KaReports.CustomerActivityReportColumns.RcFacility) <> 0)
				cbxPanel.Checked = ((columns And 2 ^ KaReports.CustomerActivityReportColumns.RcPanel) <> 0)
				cbxDriver.Checked = ((columns And 2 ^ KaReports.CustomerActivityReportColumns.RcDriver) <> 0)
				cbxTransports.Checked = ((columns And 2 ^ KaReports.CustomerActivityReportColumns.RcTransport) <> 0)
				cbxCarrier.Checked = ((columns And 2 ^ KaReports.CustomerActivityReportColumns.RcCarrier) <> 0)
				cbxDischargeLocations.Checked = ((columns And 2 ^ KaReports.CustomerActivityReportColumns.RcDischargeLocations) <> 0)
				cbxApplicator.Checked = ((columns And 2 ^ KaReports.CustomerActivityReportColumns.RcApplicator) <> 0)
				cbxUser.Checked = ((columns And 2 ^ KaReports.CustomerActivityReportColumns.RcUser) <> 0)
				cbxInterface.Checked = ((columns And 2 ^ KaReports.CustomerActivityReportColumns.RcInterface) <> 0)
				cbxNotes.Checked = ((columns And 2 ^ KaReports.CustomerActivityReportColumns.RcNotes) <> 0)
				Try
					ddlBranch.SelectedValue = GetParameter("branch_id", report.ReportParameters, Guid.Empty.ToString())
				Catch ex As Exception ' suppress exception
				End Try
				Try
					ddlCustomerAccountDestination.SelectedValue = GetParameter("customer_account_location_id", report.ReportParameters, Guid.Empty.ToString())
				Catch ex As Exception ' suppress exception
				End Try
				Try
					Dim tempguid As String = Guid.NewGuid.ToString()
					Dim totalUnitsAndDecimals As String = GetParameter("total_units_and_decimals", report.ReportParameters, tempguid)
					If totalUnitsAndDecimals = tempguid Then
						'Look up original values
						totalUnitsAndDecimals = ""
						If ((columns And 2 ^ KaReports.CustomerActivityReportColumns.RcTicketTotalQuantity) <> 0) Then
							Try
								Dim totalUnitInfo As New KaUnit(connection, Guid.Parse(GetParameter("total_unit_id", report.ReportParameters, Guid.Empty.ToString())))
								Dim totalDecimalsDisplayed As Integer = 0
								If Integer.TryParse(GetParameter("total_number_of_digits_after_decimal", report.ReportParameters, totalDecimalsDisplayed.ToString()), totalDecimalsDisplayed) Then
									' Set the unit's formatting
									totalUnitInfo.UnitPrecision = "#,###,###0" & IIf(totalDecimalsDisplayed > 0, ".".PadRight(totalDecimalsDisplayed + 1, "0"), "")
								End If
								totalUnitsAndDecimals = totalUnitInfo.Id.ToString() & ":" & totalDecimalsDisplayed.ToString() & ":true"
							Catch ex As RecordNotFoundException

							End Try
						End If
					End If
					PopulateTotalUnitsColumn(connection, totalUnitsAndDecimals)
				Catch ex As Exception ' suppress exception
				End Try
				Integer.TryParse(GetParameter("number_of_digits_after_decimal", report.ReportParameters, "0"), ddlUnitDigitsAfterDecimalPoint.SelectedIndex)
				Try
					ddlUser.SelectedValue = GetParameter("username", report.ReportParameters, "")
				Catch ex As Exception

				End Try
			End If
			If report.ReportType = KaEmailReport.ReportTypes.ReceivingActivityReport Then
				Try
					ddlSortBy.SelectedValue = GetParameter("sort_by", report.ReportParameters, "receiving_tickets.date_of_delivery")
				Catch ex As Exception ' suppress exception
				End Try
				Dim columns As ULong = ULong.Parse(GetParameter("columns", report.ReportParameters, Long.MaxValue.ToString()))
				cbxDateTime.Checked = ((columns And 2 ^ KaReports.ReceivingActivityReportColumns.RcDateTime) <> 0)
				cbxOrderNumber.Checked = ((columns And 2 ^ KaReports.ReceivingActivityReportColumns.RcOrderNumber) <> 0)
				cbxTicketNumber.Checked = ((columns And 2 ^ KaReports.ReceivingActivityReportColumns.RcTicketNumber) <> 0)
				cbxSupplier.Checked = ((columns And 2 ^ KaReports.ReceivingActivityReportColumns.RcSupplier) <> 0)
				cbxOwner.Checked = ((columns And 2 ^ KaReports.ReceivingActivityReportColumns.RcOwner) <> 0)
				cbxFacility.Checked = ((columns And 2 ^ KaReports.ReceivingActivityReportColumns.RcFacility) <> 0)
				cbxPanel.Checked = ((columns And 2 ^ KaReports.ReceivingActivityReportColumns.RcPanel) <> 0)
				cbxDriver.Checked = ((columns And 2 ^ KaReports.ReceivingActivityReportColumns.RcDriver) <> 0)
				cbxTransports.Checked = ((columns And 2 ^ KaReports.ReceivingActivityReportColumns.RcTransport) <> 0)
				cbxCarrier.Checked = ((columns And 2 ^ KaReports.ReceivingActivityReportColumns.RcCarrier) <> 0)
				cbxNotes.Checked = ((columns And 2 ^ KaReports.ReceivingActivityReportColumns.RcNotes) <> 0)
				cbxLotNumber.Checked = ((columns And 2 ^ KaReports.ReceivingActivityReportColumns.RcLotNumber) <> 0)
				Integer.TryParse(GetParameter("number_of_digits_after_decimal", report.ReportParameters, "0"), ddlUnitDigitsAfterDecimalPoint.SelectedIndex)
				Dim tempguid As String = Guid.NewGuid.ToString()
				Dim totalUnitsAndDecimals As String = GetParameter("total_units_and_decimals", report.ReportParameters, tempguid)
				If totalUnitsAndDecimals = tempguid Then
					'Look up original values
					totalUnitsAndDecimals = ""
					If ((columns And 2 ^ KaReports.ReceivingActivityReportColumns.RcTicketTotalQuantity) <> 0) Then
						totalUnitsAndDecimals = GetParameter("unit_id", report.ReportParameters, Guid.Empty.ToString()) & ":" & ddlUnitDigitsAfterDecimalPoint.SelectedIndex.ToString() & ":true"
					End If
				End If
				PopulateTotalUnitsColumn(connection, totalUnitsAndDecimals)
			End If
			If report.ReportType = KaEmailReport.ReportTypes.CustomerActivityReport OrElse
					report.ReportType = KaEmailReport.ReportTypes.ReceivingActivityReport Then
				Boolean.TryParse(GetParameter("include_voided_tickets", report.ReportParameters, False), cbxIncludeVoidedTickets.Checked)
				cbxTicketNumber_CheckedChanged(cbxTicketNumber, New EventArgs())
			End If

			If report.ReportType = KaEmailReport.ReportTypes.TransportTrackingReport Then
				Try
					ddlTransportTrackingOrderBy.SelectedValue = GetParameter("transport_tracking_order_by", report.ReportParameters, "name")
				Catch ex As Exception ' suppress exception
				End Try
				Try
					ddlTransportTrackingAscDesc.SelectedValue = GetParameter("transport_tracking_asc_desc", report.ReportParameters, "asc")
				Catch ex As Exception ' suppress exception
				End Try
			End If
			If report.ReportType = KaEmailReport.ReportTypes.OrderList Then
				Try
					ddlOrderListOrderBy.SelectedValue = GetParameter("order_list_order_by", report.ReportParameters, "order_number")
				Catch ex As Exception ' This value was changed 12/3/2014, try the old settings
					Try
						Select Case GetParameter("order_list_order_by", report.ReportParameters, "order_number").ToLower
							Case "orders.number"
								ddlOrderListOrderBy.SelectedValue = "order_number"
							Case "customer_accounts.name"
								ddlOrderListOrderBy.SelectedValue = "customer_accounts_name"
							Case "products.name"
								ddlOrderListOrderBy.SelectedValue = "products_name"
							Case "owners.name"
								ddlOrderListOrderBy.SelectedValue = "owners_name"
							Case "created"
								ddlOrderListOrderBy.SelectedValue = "order_created"
							Case Else
								ddlOrderListOrderBy.SelectedValue = "order_number"
						End Select
					Catch ex2 As Exception ' suppress exception
					End Try
				End Try
				Try
					ddlOrderListAscDesc.SelectedValue = GetParameter("order_list_asc_desc", report.ReportParameters, "asc")
				Catch ex As Exception ' suppress exception
				End Try
				Try
					ddlOrderListReportType.SelectedValue = GetParameter("order_list_report_type", report.ReportParameters, KaSetting.GetSetting(connection, "OrderListReport:" & _currentUser.Id.ToString & "/ReportType", ddlOrderListReportType.SelectedValue))
				Catch ex As Exception 'suppress exception

				End Try
			End If
			If report.ReportType = KaEmailReport.ReportTypes.ReceivingPurchaseOrderList Then
				ddlReceivingPurchaseOrderListOrderBy.SelectedValue = GetParameter("receiving_purchase_order_list_order_by", report.ReportParameters, "purchase_order_number")
				ddlReceivingPurchaseOrderListAscDesc.SelectedValue = GetParameter("receiving_purchase_order_list_asc_desc", report.ReportParameters, "asc")
			End If
			If report.ReportType = KaEmailReport.ReportTypes.BulkProductUsageReport Then
				Try
					PopulateTotalUnitsColumn(connection, GetParameter("total_units_and_decimals", report.ReportParameters, ""))
				Catch ex As Exception ' suppress exception
				End Try
			End If
			If report.ReportType = KaEmailReport.ReportTypes.TrackReport Then
				Try
					ddlTrack.SelectedValue = GetParameter("track_id", report.ReportParameters, Guid.Empty.ToString())
				Catch ex As Exception ' suppress exception
				End Try
				Boolean.TryParse(GetParameter("show_operator", report.ReportParameters, True), cbxShowOperator.Checked)
				Boolean.TryParse(GetParameter("show_rfid", report.ReportParameters, True), cbxShowRfid.Checked)
				Boolean.TryParse(GetParameter("show_car_number", report.ReportParameters, True), cbxShowCarNumber.Checked)
				Boolean.TryParse(GetParameter("show_track", report.ReportParameters, True), cbxShowTrack.Checked)
				Boolean.TryParse(GetParameter("show_scan_time", report.ReportParameters, True), cbxShowScannedTime.Checked)
				Boolean.TryParse(GetParameter("show_reverse_order", report.ReportParameters, False), cbxShowReverseOrder.Checked)
			End If
			If report.ReportType = KaEmailReport.ReportTypes.ProductAllocation Then
				Boolean.TryParse(GetParameter("show_prods_with_formula", report.ReportParameters, True), cbxOnlyShowProductsWithBulkProductsAtLocation.Checked)
			End If

			If report.ReportType = KaEmailReport.ReportTypes.CustomerActivityReport OrElse
			  report.ReportType = KaEmailReport.ReportTypes.InterfaceTicketExportStatusReport OrElse
			  report.ReportType = KaEmailReport.ReportTypes.InterfaceTicketReceivingExportStatusReport Then
				Dim interfaceId As String = GetParameter("interface_id", report.ReportParameters, "")
				Try
					ddlInterface.SelectedValue = interfaceId
				Catch ex As Exception
					'suppress
				End Try
			End If

			If report.ReportType = KaEmailReport.ReportTypes.Inventory OrElse
			   report.ReportType = KaEmailReport.ReportTypes.InventoryChangeReport Then
				Dim additionalUnits As String = GetParameter("show_additional_units", report.ReportParameters, "")
				PopulateAdditionalUnitList(additionalUnits)
			End If

			If report.ReportType = KaEmailReport.ReportTypes.InterfaceTicketExportStatusReport OrElse
				report.ReportType = KaEmailReport.ReportTypes.InterfaceTicketReceivingExportStatusReport Then
				Boolean.TryParse(GetParameter("show_tickets_exported", report.ReportParameters, False), rbShowTicketsExported.Checked)
				If rbShowTicketsExported.Checked Then
					Boolean.TryParse(GetParameter("include_tickets_marked_manually", report.ReportParameters, False), cbxIncludeTicketsMarkedManually.Checked)
					rbShowTicketsNotExported.Checked = False
					rbIncludeTicketsWithoutErrors.Checked = False
					rbIncludeTicketsWithIgnoredError.Checked = False
					rbIncludeTicketsWithError.Checked = False
					cbxOnlyIncludeOrdersForThisInterface.Checked = False
				Else
					rbShowTicketsNotExported.Checked = True
					Boolean.TryParse(GetParameter("include_tickets_with_error", report.ReportParameters, False), rbIncludeTicketsWithError.Checked)
					Boolean.TryParse(GetParameter("include_tickets_with_ignored_error", report.ReportParameters, False), rbIncludeTicketsWithIgnoredError.Checked)
					If Not (rbIncludeTicketsWithError.Checked AndAlso rbIncludeTicketsWithIgnoredError.Checked) Then
						rbIncludeTicketsWithoutErrors.Checked = True
						Boolean.TryParse(GetParameter("only_include_orders_for_this_interface", report.ReportParameters, True), cbxOnlyIncludeOrdersForThisInterface.Checked)
					End If
				End If
				rbIncludeTicketsWithError_CheckedChanged(Nothing, Nothing)
				PopulateTicketSort(report.ReportType)
				ddlTicketSort.SelectedValue = GetParameter("ticket_sort", report.ReportParameters, "exported_at DESC")
			End If

			PopulateReportTriggers(report.ReportTriggers, Guid.Empty)
		Else
			pnlReportTriggers.Enabled = False
		End If
		PopulateCustomReportConfig(Guid.Parse(ddlEmailReport.SelectedValue))
		PopulateEmailAddressList()
		SetControlUsabilityFromPermissions()
	End Sub

	Protected Sub ddlReportType_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlReportType.SelectedIndexChanged
		pnlFormat.Visible = False
		Dim bulkProductOptional As Boolean = False
		pnlBulkProduct.Visible = False
		Dim carrierOptional As Boolean = False
		pnlCarrier.Visible = False
		Dim containerOptional As Boolean = False
		pnlContainer.Visible = False
		Dim customerAccountOptional As Boolean = False
		pnlCustomerAccount.Visible = False
		Dim supplierAccountOptional As Boolean = False
		pnlSupplierAccount.Visible = False
		pnlCustomerActivityReport.Visible = False
		pnlIncludeVoidedTickets.Visible = False
		Dim driverOptional As Boolean = False
		pnlDriver.Visible = False
		Dim locationOptional As Boolean = False
		Dim includeAllLocations As Boolean = False
		pnlLocation.Visible = False
		Dim ownerOptional As Boolean = False
		Dim includeAllOwners As Boolean = False
		pnlPanels.Visible = False
		Dim productOptional As Boolean = False
		pnlProduct.Visible = False
		Dim tankOptional As Boolean = False
		pnlTank.Visible = False
		pnlTankLevelTrend.Visible = False
		pnlTrack.Visible = False
		pnlTrackReportColumns.Visible = False
		Dim transportOptional As Boolean = False
		pnlTransport.Visible = False
		pnlUnit.Visible = False
		pnlTotalUnitColumn.Visible = False
		Dim showTanksUnitOfMeasureOption As Boolean = True
		pnlDisplayUnit.Visible = False
		pnlTransportTrackingOrderBy.Visible = False
		pnlBay.Visible = False
		pnlBranch.Visible = False
		pnlCustomerAccountDestination.Visible = False
		lblUnitDigitsAfterDecimalPoint.Visible = False
		ddlUnitDigitsAfterDecimalPoint.Visible = False
		ddlProductDisplayOptions.Visible = False
		cbxDischargeLocations.Visible = False
		cbxBranch.Visible = False
		cbxCustomer.Visible = False
		cbxCustomerDestination.Visible = False
		cbxSupplier.Visible = False
		pnlInventory.Visible = False
		tblCustomReportConfig.Visible = False
		pnlOrderListOrderBy.Visible = False
		pnlOrderListReportType.Visible = False
		pnlBulkProductList.Visible = False
		pnlIncludeAllBulkProducts.Visible = False
		pnlOnlyShowProductsWithBulkProductsAtLocation.Visible = False
		pnlUser.Visible = False
		cbxUser.Visible = False
		cbxInterface.Visible = False
		pnlInterface.Visible = False
		cbxNotes.Visible = False
		pnlAdditionalUnits.Visible = False
		pnlReportDomainURL.Visible = False
		pnlTicketExportStatus.Visible = False
		pnlShowTemperature.Visible = False

		pnlLotNumber.Visible = False
		pnlContainerDisplayedColumns.Visible = False

		Dim reportType As KaEmailReport.ReportTypes = GetReportTypeEnumeration(ddlReportType.SelectedValue)
		Select Case reportType
			Case KaEmailReport.ReportTypes.BulkProductUsageReport
				pnlFormat.Visible = True
				ownerOptional = (_currentUser.OwnerId = Guid.Empty)
				pnlBay.Visible = True
				pnlPanels.Visible = True
				pnlTotalUnitColumn.Visible = True
				pnlIncludeAllBulkProducts.Visible = True
				pnlBulkProductList.Visible = True
				pnlIncludeVoidedTickets.Visible = True
			Case KaEmailReport.ReportTypes.CarrierList
				pnlFormat.Visible = True
			Case KaEmailReport.ReportTypes.ContainerHistory
				pnlFormat.Visible = True
				pnlContainer.Visible = True : containerOptional = True
				pnlContainerDisplayedColumns.Visible = True
			Case KaEmailReport.ReportTypes.ContainerList
				pnlReportDomainURL.Visible = True
				pnlFormat.Visible = True
				pnlContainerDisplayedColumns.Visible = True
			Case KaEmailReport.ReportTypes.CustomerActivityReport
				pnlReportDomainURL.Visible = True
				pnlFormat.Visible = True
				pnlProduct.Visible = True : productOptional = True
				ownerOptional = (_currentUser.OwnerId = Guid.Empty)
				pnlApplicator.Visible = True
				pnlBay.Visible = True
				pnlBranch.Visible = True
				pnlCarrier.Visible = True : carrierOptional = True
				pnlDriver.Visible = True : driverOptional = True
				pnlCustomerAccount.Visible = True : customerAccountOptional = True
				pnlCustomerAccountDestination.Visible = True
				pnlLocation.Visible = True : locationOptional = True
				pnlTransport.Visible = True : transportOptional = True
				pnlUnit.Visible = True
				lblUnitDigitsAfterDecimalPoint.Visible = True
				ddlUnitDigitsAfterDecimalPoint.Visible = True
				pnlTotalUnitColumn.Visible = True
				pnlCustomerActivityReport.Visible = True
				pnlIncludeVoidedTickets.Visible = True
				cbxCustomer.Visible = True
				cbxCustomerDestination.Visible = True
				ddlProductDisplayOptions.Visible = True
				cbxDischargeLocations.Visible = True
				cbxBranch.Visible = True
				pnlUser.Visible = True
				cbxUser.Visible = True
				cbxInterface.Visible = True
				pnlInterface.Visible = True
				cbxNotes.Visible = True
				PopulateSortBy()
			Case KaEmailReport.ReportTypes.DriverInFacilityHistoryReport
				pnlFormat.Visible = True
				ownerOptional = (_currentUser.OwnerId = Guid.Empty)
			Case KaEmailReport.ReportTypes.DriverList
				pnlFormat.Visible = True
			Case KaEmailReport.ReportTypes.Inventory
				pnlReportDomainURL.Visible = True
				pnlFormat.Visible = True
				ownerOptional = (_currentUser.OwnerId = Guid.Empty)
				pnlLocation.Visible = True : locationOptional = True
				pnlBulkProduct.Visible = True : bulkProductOptional = True
				pnlInventory.Visible = True
				pnlAdditionalUnits.Visible = True
			Case KaEmailReport.ReportTypes.InventoryChangeReport
				pnlFormat.Visible = True
				ownerOptional = (_currentUser.OwnerId = Guid.Empty)
				pnlLocation.Visible = True : locationOptional = True
				pnlBulkProduct.Visible = True : bulkProductOptional = True
				pnlAdditionalUnits.Visible = True
			Case KaEmailReport.ReportTypes.OrderList
				pnlOrderListOrderBy.Visible = True
				pnlOrderListReportType.Visible = True
				pnlCustomerAccount.Visible = True : customerAccountOptional = True
				pnlLocation.Visible = True : includeAllLocations = True
				pnlFormat.Visible = True
				includeAllOwners = True
			Case KaEmailReport.ReportTypes.ProductAllocation
				pnlFormat.Visible = True
				pnlCustomerAccount.Visible = True : customerAccountOptional = True
				pnlProduct.Visible = True : productOptional = True
				pnlUnit.Visible = True
				pnlLocation.Visible = True
				pnlOnlyShowProductsWithBulkProductsAtLocation.Visible = True
			Case KaEmailReport.ReportTypes.ProductList
				ownerOptional = (_currentUser.OwnerId = Guid.Empty)
				pnlLocation.Visible = True : locationOptional = True
			Case KaEmailReport.ReportTypes.ReceivingActivityReport
				pnlReportDomainURL.Visible = True
				pnlFormat.Visible = True
				pnlProduct.Visible = True : productOptional = True
				ownerOptional = (_currentUser.OwnerId = Guid.Empty)
				pnlCarrier.Visible = True : carrierOptional = True
				pnlDriver.Visible = True : driverOptional = True
				pnlSupplierAccount.Visible = True : supplierAccountOptional = True
				pnlLocation.Visible = True : locationOptional = True
				pnlTransport.Visible = True : transportOptional = True
				pnlUnit.Visible = True
				lblUnitDigitsAfterDecimalPoint.Visible = True
				ddlUnitDigitsAfterDecimalPoint.Visible = True
				pnlCustomerActivityReport.Visible = True
				pnlIncludeVoidedTickets.Visible = True
				cbxSupplier.Visible = True
				cbxNotes.Visible = True
				PopulateSortBy()
				pnlTotalUnitColumn.Visible = True
				pnlLotNumber.Visible = True
			Case KaEmailReport.ReportTypes.ReceivingPurchaseOrderList
				pnlReceivingPurchageOrderListOrderBy.Visible = True
				pnlReportDomainURL.Visible = True
				pnlFormat.Visible = True
				pnlBulkProduct.Visible = True : bulkProductOptional = True
				ownerOptional = (_currentUser.OwnerId = Guid.Empty)
				pnlLocation.Visible = True : includeAllLocations = True
				pnlSupplierAccount.Visible = True : supplierAccountOptional = True
				cbxSupplier.Visible = True
				cbxNotes.Visible = True
				PopulateSortBy()
			Case KaEmailReport.ReportTypes.TankAlarmHistory
				pnlFormat.Visible = True
				ownerOptional = (_currentUser.OwnerId = Guid.Empty)
				pnlTank.Visible = True : tankOptional = True
				pnlLocation.Visible = True : locationOptional = True
			Case KaEmailReport.ReportTypes.TankLevels
				pnlFormat.Visible = True
				pnlLocation.Visible = True : locationOptional = True
				ownerOptional = (_currentUser.OwnerId = Guid.Empty)
				pnlBulkProduct.Visible = True : bulkProductOptional = True
				pnlDisplayUnit.Visible = True
			Case KaEmailReport.ReportTypes.TankLevelTrend
				pnlFormat.Visible = True
				pnlTankLevelTrend.Visible = True
				pnlDisplayUnit.Visible = True
				pnlShowTemperature.Visible = True
			Case KaEmailReport.ReportTypes.TrackReport
				pnlFormat.Visible = True
				ownerOptional = (_currentUser.OwnerId = Guid.Empty)
				pnlTrack.Visible = True
				pnlTrackReportColumns.Visible = True
			Case KaEmailReport.ReportTypes.TransportList
				pnlFormat.Visible = True
			Case KaEmailReport.ReportTypes.TransportUsageReport
				pnlFormat.Visible = True
				pnlCustomerAccount.Visible = True : customerAccountOptional = True
				pnlTransport.Visible = True : transportOptional = True
			Case KaEmailReport.ReportTypes.TransportTrackingReport
				pnlFormat.Visible = True
				pnlTransportTrackingOrderBy.Visible = True
				pnlDisplayUnit.Visible = True
				ownerOptional = True
				showTanksUnitOfMeasureOption = False
			Case KaEmailReport.ReportTypes.TransportInFacilityHistoryReport
				pnlFormat.Visible = True
				ownerOptional = (_currentUser.OwnerId = Guid.Empty)
			Case KaEmailReport.ReportTypes.CustomReport
				pnlFormat.Visible = True
			Case KaEmailReport.ReportTypes.InterfaceTicketExportStatusReport, KaEmailReport.ReportTypes.InterfaceTicketReceivingExportStatusReport
				pnlTicketExportStatus.Visible = True
				pnlInterface.Visible = True
		End Select
		pnlFormat.Visible = (reportType <> KaEmailReport.ReportTypes.ProductList)

		If ddlUnitDigitsAfterDecimalPoint.Visible Then SetNumberOfDigitsAfterDecimal()
		If pnlAdditionalUnits.Visible Then PopulateAdditionalUnitList("")
		If pnlApplicator.Visible Then PopulateApplicatorList()
		If pnlBay.Visible Then PopulateBayList()
		If pnlBranch.Visible Then PopulateBranchList()
		If pnlBulkProduct.Visible OrElse pnlBulkProductList.Visible Then PopulateBulkProductList(bulkProductOptional, reportType = KaEmailReport.ReportTypes.Inventory Or reportType = KaEmailReport.ReportTypes.InventoryChangeReport, reportType = KaEmailReport.ReportTypes.Inventory Or reportType = KaEmailReport.ReportTypes.InventoryChangeReport)
		If pnlCarrier.Visible Then PopulateCarrierList(carrierOptional)
		If pnlContainer.Visible Then PopulateContainerList(containerOptional)
		If pnlCustomerAccount.Visible Then PopulateCustomerAccountList(customerAccountOptional) : ddlCustomerAccount_SelectedIndexChanged(ddlCustomerAccount, New EventArgs)
		If pnlDisplayUnit.Visible Then PopulateDefaultUnitList(showTanksUnitOfMeasureOption)
		If pnlDriver.Visible Then PopulateDriverList(driverOptional)
		If pnlIncludeAllBulkProducts.Visible Then cbxIncludeAllBulkProducts_CheckedChanged(cbxIncludeAllBulkProducts, New EventArgs())
		If pnlInterface.Visible Then PopulateInterfaceList()
		If pnlLocation.Visible Then PopulateLocationList(locationOptional, includeAllLocations)
		If pnlPanels.Visible Then PopulatePanelsList(True)
		If pnlProduct.Visible Then PopulateProductList(productOptional)
		If pnlSupplierAccount.Visible Then PopulateSupplierAccountList(supplierAccountOptional)
		If pnlTank.Visible Then PopulateTankList(tankOptional)
		If pnlTankLevelTrend.Visible Then PopulateTankLevelTrendList()
		If pnlTicketExportStatus.Visible Then PopulateTicketSort(reportType)
		If pnlTotalUnitColumn.Visible AndAlso ddlEmailReport.SelectedIndex = 0 Then PopulateTotalUnitsColumn(GetUserConnection(_currentUser.Id), "")
		If pnlTrack.Visible Then PopulateTrackList(True)
		If pnlTransport.Visible Then PopulateTransportList(transportOptional)
		If pnlUnit.Visible Then PopulateUnitList() : ddlUnit_SelectedIndexChanged(ddlUnit, New EventArgs)
		If pnlUser.Visible Then PopulateUserList(True)
		If pnlContainerDisplayedColumns.Visible Then
			Dim columnsDisplayed As ULong = 0
			For columnIndex = 0 To KaReports.ContainerReportColumns.RcLot
				columnsDisplayed += 2 ^ columnIndex
			Next
			FillContainerColumnDisplayed(columnsDisplayed)
		End If
		PopulateOwnerList(ownerOptional, includeAllOwners)
	End Sub

	Private Sub PopulateCustomReportConfig(ByVal emailId As Guid)
		With New KaEmailReport
			.Id = emailId
			If emailId <> Guid.Empty Then
				.SqlSelect(GetUserConnection(_currentUser.Id))

				If .CustomPageId <> Guid.Empty Then
					Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
					Dim customPage As KaCustomPages = New KaCustomPages(connection, .CustomPageId)

					If customPage.PageURL.Trim().Length > 0 Then ' show the interface type's configuration page
						tblCustomReportConfig.Visible = True
						Dim url As String = customPage.PageURL
						If Utilities.ConvertWebPageUrlDomainToRequestedPagesDomain(connection) Then url = Tm2Database.GetUrlInCurrentDomain(Tm2Database.GetCurrentPageUrlWithOriginalPort(Me.Request), url)

						frmConfig.Attributes("src") = url & IIf(url.Contains("?"), "&", "?") & "email_report_id=" & .Id.ToString & "&config=true"
					Else ' interface type doesn't have a configuration page, show nothing
						frmConfig.Attributes("src") = ""
					End If
				End If
			End If
		End With
	End Sub

	Protected Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
		lblStatus.Text = ""

		If tbxName.Text.Trim().Length = 0 Then ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), "InvalidName", Utilities.JsAlert("Please specify a name for this report."), False) : Exit Sub
		If KaEmailReport.GetAll(GetUserConnection(_currentUser.Id), String.Format("deleted=0 AND name={0} AND id<>{1}", Q(tbxName.Text), Q(ddlEmailReport.SelectedValue)), "name ASC").Count > 0 Then ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), "InvalidDuplicateName", Utilities.JsAlert("The name """ & tbxName.Text & """ is already in use by another e-mail report. Please enter a unique name for this e-mail report."), False) : Exit Sub
		Dim message As String = ""
		If tbxSubject.Text.Trim().Length = 0 Then ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), "InvalidSubject", Utilities.JsAlert("Please specify a subject."), False) : Exit Sub
		Dim id As Guid = Guid.Parse(ddlEmailReport.SelectedValue)
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim report As KaEmailReport
		Try
			report = New KaEmailReport(connection, id)
		Catch ex As RecordNotFoundException
			report = New KaEmailReport()
			report.Name = "New e-mail report"
			report.ReportType = KaEmailReport.ReportTypes.CarrierList
		End Try

		report.Name = tbxName.Text
		report.Recipients = tbxRecipients.Text
		report.ReportDomainURL = IIf(pnlReportDomainURL.Visible, tbxReportDomainURL.Text.Trim, "")
		report.Subject = tbxSubject.Text
		report.ReportType = GetReportTypeEnumeration(ddlReportType.SelectedValue)
		report.Disabled = cbxDisabled.Checked
		report.IsMonthToDate = cbxMonthToDate.Checked

		If ddlReportRuntype.SelectedValue = "SaveFile" Then
			report.FileSaveLocation = tbxReportFileSaveLocation.Text
			If report.FileSaveLocation.Trim.Length = 0 Then ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), "InvalidFileSaveLocation", Utilities.JsAlert("The location to save the file is not valid. Please enter a valid directory."), False) : tbxReportFileSaveLocation.Text = report.FileSaveLocation : Exit Sub
			report.ReportRunType = KaEmailReport.ReportRunTypes.SaveAsFile
		Else
			If tbxRecipients.Text.Trim().Length = 0 Then ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), "InvalidRecipient", Utilities.JsAlert("Please specify at least one recipient."), False) : Exit Sub
			If Not Utilities.IsEmailFieldValid(tbxRecipients.Text, message) Then ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), "InvalidEmailAddress", Utilities.JsAlert(message), False) : Exit Sub
			report.ReportRunType = KaEmailReport.ReportRunTypes.Email
		End If


		Dim lastSentDate As DateTime = report.LastSent
		If Not DateTime.TryParse(tbxLastSent.Value, lastSentDate) Then ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), "InvalidLastSentDate", Utilities.JsAlert("The last sent date is not valid. Please check the date entered."), False) : tbxLastSent.Value = report.LastSent : Exit Sub
		report.LastSent = lastSentDate

		Dim customPageId As Guid = Guid.Empty
		Try
			customPageId = Guid.Parse(ddlReportType.SelectedValue)
		Catch ex As Exception
			'Suppress
		End Try
		report.CustomPageId = customPageId

		Dim s As New MemoryStream() ' the XML writer will use this to build the XML data
		Dim w As XmlWriter = XmlWriter.Create(s)
		w.WriteStartElement("parameters")
		w.WriteElementString("report_id", customPageId.ToString())
		If report.ReportType = KaEmailReport.ReportTypes.BulkProductUsageReport OrElse
			report.ReportType = KaEmailReport.ReportTypes.CarrierList OrElse
			report.ReportType = KaEmailReport.ReportTypes.ContainerList OrElse
			report.ReportType = KaEmailReport.ReportTypes.ContainerHistory OrElse
			report.ReportType = KaEmailReport.ReportTypes.CustomerActivityReport OrElse
			report.ReportType = KaEmailReport.ReportTypes.DriverList OrElse
			report.ReportType = KaEmailReport.ReportTypes.DriverInFacilityHistoryReport OrElse
			report.ReportType = KaEmailReport.ReportTypes.InterfaceTicketExportStatusReport OrElse
			report.ReportType = KaEmailReport.ReportTypes.InterfaceTicketReceivingExportStatusReport OrElse
			report.ReportType = KaEmailReport.ReportTypes.Inventory OrElse
			report.ReportType = KaEmailReport.ReportTypes.InventoryChangeReport OrElse
			report.ReportType = KaEmailReport.ReportTypes.OrderList OrElse
			report.ReportType = KaEmailReport.ReportTypes.ProductAllocation OrElse
			report.ReportType = KaEmailReport.ReportTypes.ReceivingActivityReport OrElse
			report.ReportType = KaEmailReport.ReportTypes.TankLevelTrend OrElse
			report.ReportType = KaEmailReport.ReportTypes.TrackReport OrElse
			report.ReportType = KaEmailReport.ReportTypes.TransportList OrElse
			report.ReportType = KaEmailReport.ReportTypes.TransportUsageReport OrElse
			report.ReportType = KaEmailReport.ReportTypes.TankLevels OrElse
			report.ReportType = KaEmailReport.ReportTypes.TankAlarmHistory OrElse
			report.ReportType = KaEmailReport.ReportTypes.TransportTrackingReport OrElse
			report.ReportType = KaEmailReport.ReportTypes.TransportInFacilityHistoryReport OrElse
			report.ReportType = KaEmailReport.ReportTypes.CustomReport OrElse
			report.ReportType = KaEmailReport.ReportTypes.ReceivingPurchaseOrderList Then
			w.WriteElementString("format", ddlFormat.SelectedValue)
		End If
		If report.ReportType = KaEmailReport.ReportTypes.Inventory OrElse
		   report.ReportType = KaEmailReport.ReportTypes.InventoryChangeReport OrElse
		   report.ReportType = KaEmailReport.ReportTypes.ReceivingActivityReport OrElse
		   report.ReportType = KaEmailReport.ReportTypes.TankLevels OrElse
			report.ReportType = KaEmailReport.ReportTypes.ReceivingPurchaseOrderList Then
			w.WriteElementString("bulk_product_id", ddlBulkProduct.SelectedValue)
		End If
		If report.ReportType = KaEmailReport.ReportTypes.CustomerActivityReport OrElse
		   report.ReportType = KaEmailReport.ReportTypes.ReceivingActivityReport Then
			w.WriteElementString("carrier_id", ddlCarrier.SelectedValue)
		End If
		If report.ReportType = KaEmailReport.ReportTypes.ContainerList OrElse
			 report.ReportType = KaEmailReport.ReportTypes.ContainerHistory Then
			w.WriteElementString("columns", GetDisplayedContainerColumns())
		End If
		If report.ReportType = KaEmailReport.ReportTypes.ContainerHistory Then
			w.WriteElementString("container_id", ddlContainer.SelectedValue)
		End If
		If report.ReportType = KaEmailReport.ReportTypes.CustomerActivityReport OrElse
		   report.ReportType = KaEmailReport.ReportTypes.OrderList OrElse
		   report.ReportType = KaEmailReport.ReportTypes.ProductAllocation OrElse
		   report.ReportType = KaEmailReport.ReportTypes.TransportUsageReport Then
			w.WriteElementString("customer_account_id", ddlCustomerAccount.SelectedValue)
		End If
		If report.ReportType = KaEmailReport.ReportTypes.ReceivingActivityReport OrElse
		   report.ReportType = KaEmailReport.ReportTypes.ReceivingPurchaseOrderList Then
			w.WriteElementString("supplier_account_id", ddlCustomerAccount.SelectedValue)
		End If
		If report.ReportType = KaEmailReport.ReportTypes.CustomerActivityReport OrElse
		   report.ReportType = KaEmailReport.ReportTypes.ReceivingActivityReport Then
			w.WriteElementString("driver_id", ddlDriver.SelectedValue)
		End If
		If report.ReportType = KaEmailReport.ReportTypes.CustomerActivityReport OrElse
		   report.ReportType = KaEmailReport.ReportTypes.Inventory OrElse
		   report.ReportType = KaEmailReport.ReportTypes.InventoryChangeReport OrElse
		   report.ReportType = KaEmailReport.ReportTypes.ProductAllocation OrElse
		   report.ReportType = KaEmailReport.ReportTypes.ProductList OrElse
		   report.ReportType = KaEmailReport.ReportTypes.ReceivingActivityReport OrElse
		   report.ReportType = KaEmailReport.ReportTypes.OrderList OrElse
		   report.ReportType = KaEmailReport.ReportTypes.TankLevels Then
			w.WriteElementString("location_id", ddlLocation.SelectedValue)
		End If
		If report.ReportType = KaEmailReport.ReportTypes.Inventory Then
			w.WriteElementString("only_show_bulk_products_with_non_zero_inventory", cbxOnlyShowBulkProductsWithNonZeroInventory.Checked.ToString)
			w.WriteElementString("assign_physical_inventory_to_owner", cbxAssignPhysicalInventoryToOwner.Checked.ToString)
		End If
		w.WriteElementString("owner_id", ddlOwner.SelectedValue)
		If report.ReportType = KaEmailReport.ReportTypes.CustomerActivityReport OrElse
		   report.ReportType = KaEmailReport.ReportTypes.ProductAllocation Then
			w.WriteElementString("product_id", ddlProduct.SelectedValue)
		End If
		If report.ReportType = KaEmailReport.ReportTypes.TankAlarmHistory Then
			w.WriteElementString("tank_id", ddlTank.SelectedValue)
		End If
		If report.ReportType = KaEmailReport.ReportTypes.TankLevelTrend Then
			w.WriteElementString("tank_level_trend_id", ddlTankLevelTrend.SelectedValue)
			w.WriteElementString("show_temperature", cbxShowTemperature.Checked)
		End If
		If report.ReportType = KaEmailReport.ReportTypes.CustomerActivityReport OrElse
		   report.ReportType = KaEmailReport.ReportTypes.ReceivingActivityReport OrElse
		   report.ReportType = KaEmailReport.ReportTypes.TransportUsageReport Then
			w.WriteElementString("transport_id", ddlTransport.SelectedValue)
		End If
		If report.ReportType = KaEmailReport.ReportTypes.CustomerActivityReport OrElse
		   report.ReportType = KaEmailReport.ReportTypes.ProductAllocation OrElse
		   report.ReportType = KaEmailReport.ReportTypes.ReceivingActivityReport Then
			w.WriteElementString("unit_id", ddlUnit.SelectedValue)
		End If
		If report.ReportType = KaEmailReport.ReportTypes.TankLevelTrend OrElse
			report.ReportType = KaEmailReport.ReportTypes.TankLevels OrElse
			report.ReportType = KaEmailReport.ReportTypes.TransportTrackingReport Then
			w.WriteElementString("display_unit_id", ddlDisplayUnit.SelectedValue)
		End If
		If report.ReportType = KaEmailReport.ReportTypes.CustomerActivityReport Then
			w.WriteElementString("product_display_options", ddlProductDisplayOptions.SelectedIndex.ToString())
			w.WriteElementString("sort_by", ddlSortBy.SelectedValue)
			Dim columns As ULong = GetCustomerActivityReportColumnsDisplayed()
			w.WriteElementString("columns", columns.ToString())
			w.WriteElementString("applicator_id", ddlApplicator.SelectedValue)
			w.WriteElementString("bay_id", ddlBay.SelectedValue)
			w.WriteElementString("branch_id", ddlBranch.SelectedValue)
			w.WriteElementString("customer_account_location_id", ddlCustomerAccountDestination.SelectedValue)
			w.WriteElementString("number_of_digits_after_decimal", ddlUnitDigitsAfterDecimalPoint.SelectedIndex.ToString())
			w.WriteElementString("total_units_and_decimals", ConvertTotalUnitsAndDecimals())
			w.WriteElementString("include_voided_tickets", cbxIncludeVoidedTickets.Checked)
			w.WriteElementString("username", ddlUser.SelectedValue)
		End If
		If report.ReportType = KaEmailReport.ReportTypes.ReceivingActivityReport Then
			w.WriteElementString("sort_by", ddlSortBy.SelectedValue)
			Dim columns As ULong = GetReceivingActivityReportColumnsDisplayed()
			w.WriteElementString("columns", columns.ToString())
			w.WriteElementString("number_of_digits_after_decimal", ddlUnitDigitsAfterDecimalPoint.SelectedIndex.ToString())
			w.WriteElementString("include_voided_tickets", cbxIncludeVoidedTickets.Checked)
			w.WriteElementString("total_units_and_decimals", ConvertTotalUnitsAndDecimals())
		End If
		If report.ReportType = KaEmailReport.ReportTypes.TransportTrackingReport Then
			w.WriteElementString("transport_tracking_order_by", ddlTransportTrackingOrderBy.SelectedValue)
			w.WriteElementString("transport_tracking_asc_desc", ddlTransportTrackingAscDesc.SelectedValue)
		End If
		If report.ReportType = KaEmailReport.ReportTypes.OrderList Then
			w.WriteElementString("order_list_order_by", ddlOrderListOrderBy.SelectedValue)
			w.WriteElementString("order_list_asc_desc", ddlOrderListAscDesc.SelectedValue)
			w.WriteElementString("order_list_report_type", ddlOrderListReportType.SelectedValue)
		End If
		If report.ReportType = KaEmailReport.ReportTypes.ReceivingPurchaseOrderList Then
			w.WriteElementString("receiving_purchase_order_list_order_by", ddlReceivingPurchaseOrderListOrderBy.SelectedValue)
			w.WriteElementString("receiving_purchase_order_list_asc_desc", ddlReceivingPurchaseOrderListAscDesc.SelectedValue)
		End If
		If report.ReportType = KaEmailReport.ReportTypes.BulkProductUsageReport Then
			Dim convertTotalUnitsAndDecimalsString As String = ConvertTotalUnitsAndDecimals()
			If convertTotalUnitsAndDecimalsString.Length = 0 Then
				ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), "NoTotalUnitsSelected", Utilities.JsAlert("There are no units selected for this report. Please select at least 1 unit to display."), False)
				Exit Sub
			End If
			Dim bulkProductList As New List(Of Guid)
			If Not cbxIncludeAllBulkProducts.Checked Then
				For Each bulkProd As ListItem In cblBulkProductList.Items
					If bulkProd.Selected Then bulkProductList.Add(Guid.Parse(bulkProd.Value))
				Next
				If bulkProductList.Count = 0 Then
					ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), "NoBulkProductsSelected", Utilities.JsAlert("There are no bulk products selected for this report. Please select at least 1 bulk product to display."), False)
					Exit Sub
				End If
				w.WriteElementString("bulk_product_ids", Tm2Database.ToXml(bulkProductList, GetType(List(Of Guid))))
			End If
			w.WriteElementString("all_bulk_products", cbxIncludeAllBulkProducts.Checked)
			w.WriteElementString("bay_id", ddlBay.SelectedValue)
			w.WriteElementString("panel_id", ddlPanel.SelectedValue)
			w.WriteElementString("total_units_and_decimals", convertTotalUnitsAndDecimalsString)
			w.WriteElementString("include_voided_tickets", cbxIncludeVoidedTickets.Checked)
		End If
		If report.ReportType = KaEmailReport.ReportTypes.TrackReport Then
			w.WriteElementString("track_id", ddlTrack.SelectedValue)
			w.WriteElementString("show_operator", cbxShowOperator.Checked)
			w.WriteElementString("show_rfid", cbxShowRfid.Checked)
			w.WriteElementString("show_car_number", cbxShowCarNumber.Checked)
			w.WriteElementString("show_track", cbxShowTrack.Checked)
			w.WriteElementString("show_scan_time", cbxShowScannedTime.Checked)
			w.WriteElementString("show_reverse_order", cbxShowReverseOrder.Checked)
		End If
		If report.ReportType = KaEmailReport.ReportTypes.ProductAllocation Then
			w.WriteElementString("show_prods_with_formula", cbxOnlyShowProductsWithBulkProductsAtLocation.Checked)
		End If
		If report.ReportType = KaEmailReport.ReportTypes.CustomerActivityReport OrElse
			report.ReportType = KaEmailReport.ReportTypes.InterfaceTicketExportStatusReport OrElse
			report.ReportType = KaEmailReport.ReportTypes.InterfaceTicketReceivingExportStatusReport Then
			w.WriteElementString("interface_id", ddlInterface.SelectedValue)
		End If
		If report.ReportType = KaEmailReport.ReportTypes.Inventory OrElse
			report.ReportType = KaEmailReport.ReportTypes.InventoryChangeReport Then
			Dim additionalUnits As String = ""
			For Each li As ListItem In cblShowAdditionalUnits.Items
				If li.Selected Then
					additionalUnits &= IIf(additionalUnits.Length > 0, ",", "") & li.Value
				End If
			Next
			w.WriteElementString("show_additional_units", additionalUnits)
		End If
		If report.ReportType = KaEmailReport.ReportTypes.InterfaceTicketExportStatusReport OrElse
			report.ReportType = KaEmailReport.ReportTypes.InterfaceTicketReceivingExportStatusReport Then
			w.WriteElementString("show_tickets_exported", rbShowTicketsExported.Checked)
			w.WriteElementString("include_tickets_marked_manually", cbxIncludeTicketsMarkedManually.Checked)
			w.WriteElementString("include_tickets_with_error", rbIncludeTicketsWithError.Checked)
			w.WriteElementString("include_tickets_with_ignored_error", rbIncludeTicketsWithIgnoredError.Checked)
			w.WriteElementString("only_include_orders_for_this_interface", cbxOnlyIncludeOrdersForThisInterface.Checked)
			w.WriteElementString("ticket_sort", ddlTicketSort.SelectedValue)
		End If

		w.WriteEndElement() ' close the parameters tag
		w.Flush()
		s.Seek(0, SeekOrigin.Begin) ' move to the beginning of the stream
		report.ReportParameters = New StreamReader(s).ReadToEnd() ' convert the stream to a string
		report.ReportTriggers.Clear()
		If lstEmailSchedule.SelectedIndex >= 0 Then btnUpdateSchedule_Click(btnUpdateSchedule, New EventArgs())
		For Each reportTrigger As ListItem In lstEmailSchedule.Items
			Try
				report.ReportTriggers.Add(Tm2Database.FromXml(reportTrigger.Value, GetType(KaEmailReportTrigger)))
			Catch ex As Exception

			End Try
		Next
		report.SqlUpdateInsertIfNotFound(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
		Dim status As String = ""
		If report.Id = Guid.Empty Then
			status = "E-mail report created."
		Else
			status = "E-mail report updated."
		End If
		PopulateEmailReports()
		ddlEmailReport.SelectedValue = report.Id.ToString()
		ddlEmailReport_SelectedIndexChanged(ddlEmailReport, New EventArgs())
		lblStatus.Text = status
	End Sub

	Protected Sub btnDelete_Click(sender As Object, e As EventArgs) Handles btnDelete.Click
		lblStatus.Text = ""
		If ddlEmailReport.SelectedIndex > 0 Then
			Tm2Database.ExecuteNonQuery(GetUserConnection(_currentUser.Id), String.Format("UPDATE {0} SET {1}=1 WHERE id={2}", KaEmailReport.TABLE_NAME, KaEmailReport.FN_DELETED, Q(ddlEmailReport.SelectedValue)))
			PopulateEmailReports()
			ddlEmailReport.SelectedValue = Guid.Empty.ToString()
			ddlEmailReport_SelectedIndexChanged(Nothing, Nothing)
			lblStatus.Text = "E-mail report deleted."
			PopulateCustomReportConfig(Guid.Empty)
		End If
	End Sub
#End Region

	Private Sub PopulateEmailReports()
		ddlEmailReport.Items.Clear()
		If _currentUserPermission(_currentTableName).Create Then ddlEmailReport.Items.Add(New ListItem("Enter new e-mail report", Guid.Empty.ToString())) Else ddlEmailReport.Items.Add(New ListItem("Select an e-mail report", Guid.Empty.ToString()))
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim conditions As String = KaEmailReport.FN_DELETED & "=0" & IIf(_currentUser.OwnerId = Guid.Empty, "", " AND (" & KaEmailReport.FN_OWNER_ID & "=" & Q(_currentUser.OwnerId) & " OR " & KaEmailReport.FN_OWNER_ID & "=" & Q(Guid.Empty) & ")")
		For Each report As KaEmailReport In KaEmailReport.GetAll(connection, conditions, KaEmailReport.FN_DISABLED & " ASC, " & KaEmailReport.FN_NAME & " ASC")
			ddlEmailReport.Items.Add(New ListItem(report.Name.Trim() & IIf(report.Disabled, " (Disabled)", ""), report.Id.ToString()))
		Next
		ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), "ResetScrollPosition;", "resetDotNetScrollPosition();", True)
	End Sub

	Private Sub PopulateApplicatorList()
		ddlApplicator.Items.Clear()
		ddlApplicator.Items.Add(New ListItem("", Guid.Empty.ToString()))
		For Each a As KaApplicator In KaApplicator.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
			ddlApplicator.Items.Add(New ListItem(a.Name, a.Id.ToString()))
		Next
	End Sub

	Private Sub PopulateBayList()
		ddlBay.Items.Clear()
		ddlBay.Items.Add(New ListItem("", Guid.Empty.ToString()))
		For Each r As KaBay In KaBay.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
			ddlBay.Items.Add(New ListItem(r.Name, r.Id.ToString()))
		Next
	End Sub

	Private Sub PopulateBranchList()
		ddlBranch.Items.Clear()
		ddlBranch.Items.Add(New ListItem("", Guid.Empty.ToString()))
		For Each r As KaBranch In KaBranch.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
			ddlBranch.Items.Add(New ListItem(r.Name, r.Id.ToString()))
		Next
	End Sub

	Private Sub PopulateBulkProductList(ByVal isOptional As Boolean, ByVal includeTotalizedInventoryGroups As Boolean, ByVal includeNonTotalizedInventoryGroups As Boolean)
		Dim selected As String
		If ddlBulkProduct.SelectedIndex >= 0 Then selected = ddlBulkProduct.SelectedValue Else selected = Guid.Empty.ToString()
		ddlBulkProduct.Items.Clear()
		cblBulkProductList.Items.Clear()
		If isOptional Then ddlBulkProduct.Items.Add(New ListItem("", Guid.Empty.ToString()))
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim conditions As String = "deleted=0" & IIf(_currentUser.OwnerId = Guid.Empty, "", " AND (owner_id=" & Q(_currentUser.OwnerId) & " OR owner_id=" & Q(Guid.Empty) & ")")
		For Each r As KaBulkProduct In KaBulkProduct.GetAll(connection, conditions, "name ASC")
			If Not r.IsFunction(GetUserConnection(_currentUser.Id)) Then
				ddlBulkProduct.Items.Add(New ListItem(r.Name, r.Id.ToString()))
				cblBulkProductList.Items.Add(New ListItem(r.Name, r.Id.ToString()))
			End If
		Next
		If includeTotalizedInventoryGroups OrElse includeNonTotalizedInventoryGroups Then
			For Each invGroup As KaInventoryGroup In KaInventoryGroup.GetAll(connection, "(deleted = 0)", "name")
				invGroup.Name &= " (Grouped inventory)"
				If (includeNonTotalizedInventoryGroups AndAlso Not invGroup.IsTotalizedGrouping) OrElse (includeTotalizedInventoryGroups AndAlso invGroup.IsTotalizedGrouping) Then
					ddlBulkProduct.Items.Add(New ListItem(invGroup.Name, invGroup.Id.ToString()))
					cblBulkProductList.Items.Add(New ListItem(invGroup.Name, invGroup.Id.ToString()))
				End If
			Next
		End If
		Try
			ddlBulkProduct.SelectedValue = selected
		Catch ex As Exception ' suppress exception
		End Try
	End Sub

	Private Sub PopulateCarrierList(isOptional As Boolean)
		Dim selected As String
		If ddlCarrier.SelectedIndex >= 0 Then selected = ddlCarrier.SelectedValue Else selected = Guid.Empty.ToString()
		ddlCarrier.Items.Clear()
		If isOptional Then ddlCarrier.Items.Add(New ListItem("", Guid.Empty.ToString()))
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim conditions As String = "deleted=0"
		For Each r As KaCarrier In KaCarrier.GetAll(connection, conditions, "name ASC")
			ddlCarrier.Items.Add(New ListItem(r.Name, r.Id.ToString()))
		Next
		Try
			ddlCarrier.SelectedValue = selected
		Catch ex As Exception ' suppress exception
		End Try
	End Sub

	Private Sub PopulateContainerList(isOptional As Boolean)
		Dim selected As String
		If ddlContainer.SelectedIndex >= 0 Then selected = ddlContainer.SelectedValue Else selected = Guid.Empty.ToString()
		ddlContainer.Items.Clear()
		If isOptional Then ddlContainer.Items.Add(New ListItem("", Guid.Empty.ToString()))
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim conditions As String = "deleted=0" & IIf(_currentUser.OwnerId = Guid.Empty, "", " AND (owner_id=" & Q(_currentUser.OwnerId) & " OR owner_id=" & Q(Guid.Empty) & ")")
		For Each r As KaContainer In KaContainer.GetAll(connection, conditions, "number ASC")
			ddlContainer.Items.Add(New ListItem(r.Number, r.Id.ToString()))
		Next
		Try
			ddlContainer.SelectedValue = selected
		Catch ex As Exception ' suppress exception
		End Try
	End Sub

	Private Sub PopulateCustomerAccountList(isOptional As Boolean)
		Dim selected As String
		If ddlCustomerAccount.SelectedIndex >= 0 Then selected = ddlCustomerAccount.SelectedValue Else selected = Guid.Empty.ToString()
		ddlCustomerAccount.Items.Clear()
		If isOptional Then ddlCustomerAccount.Items.Add(New ListItem("", Guid.Empty.ToString()))
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim conditions As String = "deleted=0 AND is_supplier=0" & IIf(_currentUser.OwnerId <> Guid.Empty, String.Format(" AND (owner_id={0} OR owner_id={1})", Q(Guid.Empty), Q(_currentUser.OwnerId)), "")
		For Each r As KaCustomerAccount In KaCustomerAccount.GetAll(connection, conditions, "name ASC")
			ddlCustomerAccount.Items.Add(New ListItem(r.Name, r.Id.ToString()))
		Next
		Try
			ddlCustomerAccount.SelectedValue = selected
		Catch ex As Exception ' suppress exception
		End Try
	End Sub

	Private Sub PopulateDriverList(isOptional As Boolean)
		Dim selected As String
		If ddlDriver.SelectedIndex >= 0 Then selected = ddlDriver.SelectedValue Else selected = Guid.Empty.ToString()
		ddlDriver.Items.Clear()
		If isOptional Then ddlDriver.Items.Add(New ListItem("", Guid.Empty.ToString()))
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim conditions As String = "deleted=0"
		For Each r As KaDriver In KaDriver.GetAll(connection, conditions, "name ASC")
			ddlDriver.Items.Add(New ListItem(r.Name, r.Id.ToString()))
		Next
		Try
			ddlDriver.SelectedValue = selected
		Catch ex As Exception ' suppress exception
		End Try
	End Sub

	Private Sub PopulateLocationList(ByVal isOptional As Boolean, ByVal includeAllLocations As Boolean)
		Dim selected As String
		If ddlLocation.SelectedIndex >= 0 Then selected = ddlLocation.SelectedValue Else selected = Guid.Empty.ToString()
		ddlLocation.Items.Clear()
		If isOptional Then
			ddlLocation.Items.Add(New ListItem("", Guid.Empty.ToString()))
		ElseIf includeAllLocations Then
			ddlLocation.Items.Add(New ListItem("All facilities", Guid.Empty.ToString()))
			selected = Guid.Empty.ToString
		End If

		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim conditions As String = "deleted=0"
		For Each r As KaLocation In KaLocation.GetAll(connection, conditions, "name ASC")
			ddlLocation.Items.Add(New ListItem(r.Name, r.Id.ToString()))
		Next
		Try
			ddlLocation.SelectedValue = selected
		Catch ex As Exception ' suppress exception
		End Try
	End Sub

	Private Sub PopulateOwnerList(ByVal isOptional As Boolean, ByVal includeAllOwners As Boolean)
		Dim selected As String
		If ddlOwner.SelectedIndex >= 0 Then selected = ddlOwner.SelectedValue Else selected = Guid.Empty.ToString()
		ddlOwner.Items.Clear()
		If isOptional Then
			ddlOwner.Items.Add(New ListItem("", Guid.Empty.ToString()))
		ElseIf includeAllOwners Then
			ddlOwner.Items.Add(New ListItem("All owners", Guid.Empty.ToString()))
			selected = Guid.Empty.ToString
		End If
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim conditions As String = "deleted=0" & IIf(_currentUser.OwnerId = Guid.Empty, "", " AND id=" & Q(_currentUser.OwnerId))
		For Each r As KaOwner In KaOwner.GetAll(connection, conditions, "name ASC")
			ddlOwner.Items.Add(New ListItem(r.Name, r.Id.ToString()))
		Next
		Try
			ddlOwner.SelectedValue = selected
		Catch ex As Exception ' suppress exception
		End Try
	End Sub

	Private Sub PopulatePanelsList(isOptional As Boolean)
		Dim selected As String
		If ddlPanel.SelectedIndex >= 0 Then selected = ddlPanel.SelectedValue Else selected = Guid.Empty.ToString()
		ddlPanel.Items.Clear()
		If isOptional Then ddlPanel.Items.Add(New ListItem("", Guid.Empty.ToString()))
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim conditions As String = "deleted=0"
		For Each r As KaPanel In KaPanel.GetAll(connection, conditions, "name ASC")
			ddlPanel.Items.Add(New ListItem(r.Name, r.Id.ToString()))
		Next
		Try
			ddlPanel.SelectedValue = selected
		Catch ex As Exception ' suppress exception
		End Try
	End Sub

	Private Sub PopulateProductList(isOptional As Boolean)
		Dim selected As String
		If ddlProduct.SelectedIndex >= 0 Then selected = ddlProduct.SelectedValue Else selected = Guid.Empty.ToString()
		ddlProduct.Items.Clear()
		If isOptional Then ddlProduct.Items.Add(New ListItem("", Guid.Empty.ToString()))
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim conditions As String = "deleted=0" & IIf(_currentUser.OwnerId = Guid.Empty, "", " AND (owner_id=" & Q(_currentUser.OwnerId) & " OR owner_id=" & Q(Guid.Empty) & ")")
		For Each r As KaProduct In KaProduct.GetAll(connection, conditions, "name ASC")
			If Not r.IsFunction(connection) Then
				ddlProduct.Items.Add(New ListItem(r.Name, r.Id.ToString()))
			End If
		Next
		Try
			ddlProduct.SelectedValue = selected
		Catch ex As Exception ' suppress exception
		End Try
	End Sub

	Private Sub PopulateSortBy()
		With ddlSortBy
			Dim defaultSelected As String = ""
			Dim selected As String = ""
			If ddlSortBy.SelectedIndex >= 0 Then selected = ddlSortBy.SelectedValue
			.Items.Clear()
			Select Case GetReportTypeEnumeration(ddlReportType.SelectedValue)
				Case KaEmailReport.ReportTypes.CustomerActivityReport
					.Items.Add(New ListItem("Date/time", "tickets.loaded_at"))
					.Items.Add(New ListItem("Driver", "tickets.driver_name"))
					.Items.Add(New ListItem("Order number", "tickets.order_number"))
					defaultSelected = "tickets.loaded_at"
				Case KaEmailReport.ReportTypes.ReceivingActivityReport
					.Items.Add(New ListItem("Bulk product name", "receiving_tickets.bulk_product_name"))
					.Items.Add(New ListItem("Date/time", "receiving_tickets.date_of_delivery"))
					.Items.Add(New ListItem("Driver", "receiving_tickets.driver_name"))
					.Items.Add(New ListItem("Facility", "receiving_tickets.location_name"))
					.Items.Add(New ListItem("Owner", "receiving_tickets.owner_name"))
					.Items.Add(New ListItem("Purchase order number", "receiving_tickets.purchase_order_number"))
					.Items.Add(New ListItem("Supplier", "receiving_tickets.supplier_account_name"))
					defaultSelected = "receiving_tickets.date_of_delivery"
				Case KaEmailReport.ReportTypes.ReceivingPurchaseOrderList
					.Items.Add(New ListItem("Purchase order number", "purchase_order_number"))
					.Items.Add(New ListItem("Supplier name", "supplier_name"))
					.Items.Add(New ListItem("Bulk product name", "bulk_product_name"))
					.Items.Add(New ListItem("Owner", "owner_name"))
					.Items.Add(New ListItem("Created", "purchase_order_created"))
					defaultSelected = "purchase_order_number"
				Case Else
					.Items.Add(New ListItem("", ""))
			End Select

			Try
				ddlSortBy.SelectedValue = selected
			Catch ex As Exception
				Try
					ddlSortBy.SelectedValue = defaultSelected
				Catch ex2 As Exception
					ddlSortBy.SelectedIndex = 0
				End Try
			End Try
		End With
	End Sub

	Private Sub PopulateSupplierAccountList(isOptional As Boolean)
		With ddlSupplierAccount
			Dim selected As String = Guid.Empty.ToString()
			If .SelectedIndex >= 0 Then selected = .SelectedValue
			.Items.Clear()
			If isOptional Then .Items.Add(New ListItem("", Guid.Empty.ToString()))
			Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
			Dim conditions As String = "deleted=0" & IIf(_currentUser.OwnerId = Guid.Empty, "", " AND (owner_id=" & Q(_currentUser.OwnerId) & " OR owner_id=" & Q(Guid.Empty) & ")")
			For Each r As KaSupplierAccount In KaSupplierAccount.GetAll(connection, conditions, "name ASC")
				.Items.Add(New ListItem(r.Name, r.Id.ToString()))
			Next
			Try
				.SelectedValue = selected
			Catch ex As Exception ' suppress exception
			End Try
		End With
	End Sub

	Private Sub PopulateTankList(isOptional As Boolean)
		Dim selected As String
		If ddlTank.SelectedIndex >= 0 Then selected = ddlTank.SelectedValue Else selected = Guid.Empty.ToString()
		ddlTank.Items.Clear()
		If isOptional Then ddlTank.Items.Add(New ListItem("", Guid.Empty.ToString()))
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim conditions As String = "deleted=0" & IIf(_currentUser.OwnerId = Guid.Empty, "", " AND (owner_id=" & Q(_currentUser.OwnerId) & " OR owner_id=" & Q(Guid.Empty) & ")")
		For Each r As KaTank In KaTank.GetAll(connection, conditions, "name ASC")
			ddlTank.Items.Add(New ListItem(r.Name, r.Id.ToString()))
		Next
		Try
			ddlTank.SelectedValue = selected
		Catch ex As Exception ' suppress exception
		End Try
	End Sub

	Private Sub PopulateTankLevelTrendList()
		Dim selected As String
		If ddlTankLevelTrend.SelectedIndex >= 0 Then selected = ddlTankLevelTrend.SelectedValue Else selected = Guid.Empty.ToString()
		ddlTankLevelTrend.Items.Clear()
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim conditions As String = "deleted=0"
		For Each r As KaTankLevelTrend In KaTankLevelTrend.GetAll(connection, conditions, "name ASC")
			ddlTankLevelTrend.Items.Add(New ListItem(r.Name, r.Id.ToString()))
		Next
		Try
			ddlTankLevelTrend.SelectedValue = selected
		Catch ex As Exception ' suppress exception
		End Try
	End Sub

	Private Sub PopulateTrackList(isOptional As Boolean)
		Dim selected As String
		If ddlTrack.SelectedIndex >= 0 Then selected = ddlTrack.SelectedValue Else selected = Guid.Empty.ToString()
		ddlTrack.Items.Clear()
		If isOptional Then ddlTrack.Items.Add(New ListItem("", Guid.Empty.ToString()))
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim conditions As String = "deleted = 0"
		For Each r As KaTrack In KaTrack.GetAll(connection, conditions, "name ASC")
			ddlTrack.Items.Add(New ListItem(r.Name, r.Id.ToString()))
		Next
		Try
			ddlTrack.SelectedValue = selected
		Catch ex As Exception ' suppress exception
		End Try
	End Sub

	Private Sub PopulateTransportList(isOptional As Boolean)
		Dim selected As String
		If ddlTransport.SelectedIndex >= 0 Then selected = ddlTransport.SelectedValue Else selected = Guid.Empty.ToString()
		ddlTransport.Items.Clear()
		If isOptional Then ddlTransport.Items.Add(New ListItem("", Guid.Empty.ToString()))
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim conditions As String = "deleted=0"
		For Each r As KaTransport In KaTransport.GetAll(connection, conditions, "name ASC")
			ddlTransport.Items.Add(New ListItem(r.Name, r.Id.ToString()))
		Next
		Try
			ddlTransport.SelectedValue = selected
		Catch ex As Exception ' suppress exception
		End Try
	End Sub

	Private Sub PopulateUserList(isOptional As Boolean)
		ddlUser.Items.Clear()
		ddlUser.Items.Add(New ListItem("All users", ""))
		Select Case GetReportTypeEnumeration(ddlReportType.SelectedValue)
			Case KaEmailReport.ReportTypes.CustomerActivityReport
				Dim activeUsersRdr As OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), "SELECT username FROM tickets WHERE username > '' union SELECT name AS username FROM users WHERE name >'' AND deleted = 0 ORDER BY username")
				Do While activeUsersRdr.Read()
					ddlUser.Items.Add(New ListItem(activeUsersRdr.Item("username"), activeUsersRdr.Item("username")))
				Loop
			Case Else
				Dim activeUsersRdr As OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), "SELECT DISTINCT name FROM users WHERE name >'' AND deleted = 0 ORDER BY name")
				Do While activeUsersRdr.Read()
					ddlUser.Items.Add(New ListItem(activeUsersRdr.Item("name"), activeUsersRdr.Item("name")))
				Loop
		End Select
	End Sub

	Private Sub PopulateUnitList()
		Dim selected As String = Guid.Empty.ToString()
		Dim totalSelected As String = Guid.Empty.ToString()
		If ddlUnit.SelectedIndex >= 0 Then selected = ddlUnit.SelectedValue
		ddlUnit.Items.Clear()

		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim conditions As String = "deleted=0"
		For Each r As KaUnit In KaUnit.GetAll(connection, conditions, "name ASC")
			ddlUnit.Items.Add(New ListItem(r.Name, r.Id.ToString()))
			If r.BaseUnit = KaUnit.Unit.Pounds AndAlso r.Factor = 1 Then ' This should be Pounds
				ddlUnit.SelectedIndex = ddlUnit.Items.Count - 1
			End If
		Next
		Try
			ddlUnit.SelectedValue = selected
		Catch ex As Exception ' suppress exception

		End Try
	End Sub

	Private Sub PopulateDefaultUnitList(ByVal showTanksUnitOfMeasureOption As Boolean)
		Dim selected As String = ""
		If ddlDisplayUnit.SelectedIndex >= 0 Then selected = ddlDisplayUnit.SelectedValue Else selected = Guid.Empty.ToString
		ddlDisplayUnit.Items.Clear()
		If showTanksUnitOfMeasureOption Then
			ddlDisplayUnit.Items.Add(New ListItem("Tank's Unit", Guid.Empty.ToString))
		End If
		For Each r As KaUnit In KaUnit.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name asc")
			ddlDisplayUnit.Items.Add(New ListItem(r.Name, r.Id.ToString))
		Next
		Try
			ddlUnit.SelectedValue = selected
		Catch ex As Exception ' suppress exception
		End Try
	End Sub

	Private Sub PopulateAdditionalUnitList(ByVal selectedUnits As String)
		cblShowAdditionalUnits.Items.Clear()
		For Each r As KaUnit In KaUnit.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name asc")
			cblShowAdditionalUnits.Items.Add(New ListItem(r.Name, r.Id.ToString))
			cblShowAdditionalUnits.Items(cblShowAdditionalUnits.Items.Count - 1).Selected = selectedUnits.Contains(r.Id.ToString)
		Next
	End Sub

	Private Sub PopulateInterfaceList()
		ddlInterface.Items.Clear()
		If pnlTicketExportStatus.Visible = False Then 'Report cannot handle all interfaces, also no interface is already a cbx option.
			ddlInterface.Items.Add(New ListItem("All interfaces", "-1"))
			ddlInterface.Items.Add(New ListItem("No interface", Guid.Empty.ToString))
		End If
		For Each i As KaInterface In KaInterface.GetAll(Tm2Database.Connection, "deleted=0", "name asc")
			ddlInterface.Items.Add(New ListItem(i.Name, i.Id.ToString))
		Next
	End Sub

	Private Sub SetNumberOfDigitsAfterDecimal()
		Dim id As Guid = Guid.Parse(ddlEmailReport.SelectedValue)
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim report As New KaEmailReport
		Try
			report = New KaEmailReport(connection, id)
		Catch ex As RecordNotFoundException

		End Try

		Dim precision As String = ""
		Try
			precision = New KaUnit(connection, Guid.Parse(ddlUnit.SelectedValue)).UnitPrecision
		Catch ex As Exception

		End Try
		Dim decimalCount As Integer = 0
		If precision.IndexOf(".") >= 0 Then decimalCount = Math.Max(0, precision.Length - precision.IndexOf(".") - 1)
		Integer.TryParse(GetParameter("number_of_digits_after_decimal", report.ReportParameters, decimalCount.ToString()), decimalCount)
		ddlUnitDigitsAfterDecimalPoint.SelectedIndex = decimalCount
	End Sub

	Public Shared Function GetParameter(name As String, xml As String, defaultValue As String) As String
		Try
			Dim reader As XmlReader = XmlReader.Create(New MemoryStream(Encoding.UTF8.GetBytes(xml)))
			Dim elementName As String = ""
			Do While reader.Read()
				Select Case reader.NodeType
					Case XmlNodeType.Element
						elementName = reader.Name.ToLower()
					Case XmlNodeType.Text
						If elementName.ToLower() = name.ToLower() Then Return reader.Value
				End Select
			Loop
		Catch ex As Exception
		End Try
		Return defaultValue
	End Function

	Private Function GetReportTypeEnumeration(value As String) As KaEmailReport.ReportTypes
		Select Case value
			Case KaEmailReport.ReportTypes.Inventory.ToString() : Return KaEmailReport.ReportTypes.Inventory
			Case KaEmailReport.ReportTypes.CarrierList.ToString() : Return KaEmailReport.ReportTypes.CarrierList
			Case KaEmailReport.ReportTypes.ContainerList.ToString() : Return KaEmailReport.ReportTypes.ContainerList
			Case KaEmailReport.ReportTypes.ContainerHistory.ToString() : Return KaEmailReport.ReportTypes.ContainerHistory
			Case KaEmailReport.ReportTypes.CustomerActivityReport.ToString() : Return KaEmailReport.ReportTypes.CustomerActivityReport
			Case KaEmailReport.ReportTypes.DriverList.ToString() : Return KaEmailReport.ReportTypes.DriverList
			Case KaEmailReport.ReportTypes.OrderList.ToString() : Return KaEmailReport.ReportTypes.OrderList
			Case KaEmailReport.ReportTypes.ProductAllocation.ToString() : Return KaEmailReport.ReportTypes.ProductAllocation
			Case KaEmailReport.ReportTypes.ProductList.ToString() : Return KaEmailReport.ReportTypes.ProductList
			Case KaEmailReport.ReportTypes.ReceivingActivityReport.ToString() : Return KaEmailReport.ReportTypes.ReceivingActivityReport
			Case KaEmailReport.ReportTypes.TankLevelTrend.ToString() : Return KaEmailReport.ReportTypes.TankLevelTrend
			Case KaEmailReport.ReportTypes.TransportList.ToString() : Return KaEmailReport.ReportTypes.TransportList
			Case KaEmailReport.ReportTypes.TransportUsageReport.ToString() : Return KaEmailReport.ReportTypes.TransportUsageReport
			Case KaEmailReport.ReportTypes.TankLevels.ToString() : Return KaEmailReport.ReportTypes.TankLevels
			Case KaEmailReport.ReportTypes.TankAlarmHistory.ToString() : Return KaEmailReport.ReportTypes.TankAlarmHistory
			Case KaEmailReport.ReportTypes.TransportTrackingReport.ToString : Return KaEmailReport.ReportTypes.TransportTrackingReport
			Case KaEmailReport.ReportTypes.InventoryChangeReport.ToString() : Return KaEmailReport.ReportTypes.InventoryChangeReport
			Case KaEmailReport.ReportTypes.DriverInFacilityHistoryReport.ToString() : Return KaEmailReport.ReportTypes.DriverInFacilityHistoryReport
			Case KaEmailReport.ReportTypes.BulkProductUsageReport.ToString() : Return KaEmailReport.ReportTypes.BulkProductUsageReport
			Case KaEmailReport.ReportTypes.TransportInFacilityHistoryReport.ToString() : Return KaEmailReport.ReportTypes.TransportInFacilityHistoryReport
			Case KaEmailReport.ReportTypes.TrackReport.ToString() : Return KaEmailReport.ReportTypes.TrackReport
			Case KaEmailReport.ReportTypes.ReceivingPurchaseOrderList.ToString() : Return KaEmailReport.ReportTypes.ReceivingPurchaseOrderList
			Case KaEmailReport.ReportTypes.InterfaceTicketExportStatusReport.ToString() : Return KaEmailReport.ReportTypes.InterfaceTicketExportStatusReport
			Case KaEmailReport.ReportTypes.InterfaceTicketReceivingExportStatusReport.ToString() : Return KaEmailReport.ReportTypes.InterfaceTicketReceivingExportStatusReport
			Case Else
				Try
					Dim reportId As Guid = New Guid(value)
					'It is a Guid, it is a custom report
					Return KaEmailReport.ReportTypes.CustomReport
				Catch ex As Exception
					'It is not a defined report or a custom report at this point
					Return KaEmailReport.ReportTypes.Generic
				End Try
		End Select
	End Function

	Private Function GetCustomerActivityReportColumnsDisplayed() As UInt64
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

	Private Function GetReceivingActivityReportColumnsDisplayed() As UInt64
		Dim col As UInt64 = 0
		If cbxDateTime.Checked Then col += 2 ^ KaReports.ReceivingActivityReportColumns.RcDateTime
		If cbxOrderNumber.Checked Then col += 2 ^ KaReports.ReceivingActivityReportColumns.RcOrderNumber
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

	Private Sub ddlUnit_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles ddlUnit.SelectedIndexChanged
		If ddlUnitDigitsAfterDecimalPoint.Visible Then
			Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)

			Dim decimalCount As Integer = ddlUnitDigitsAfterDecimalPoint.SelectedIndex
			Dim precision As String = ""
			Try
				precision = New KaUnit(connection, Guid.Parse(ddlUnit.SelectedValue)).UnitPrecision
			Catch ex As Exception

			End Try
			If precision.IndexOf(".") >= 0 Then decimalCount = Math.Max(0, precision.Length - precision.IndexOf(".") - 1)
			ddlUnitDigitsAfterDecimalPoint.SelectedIndex = decimalCount
		End If
	End Sub

	Private Sub ddlCustomerAccount_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles ddlCustomerAccount.SelectedIndexChanged
		Dim selectedValue As String = Guid.Empty.ToString()
		If ddlCustomerAccountDestination.SelectedIndex >= 0 Then selectedValue = ddlCustomerAccountDestination.SelectedValue
		ddlCustomerAccountDestination.Items.Clear()
		If pnlCustomerAccountDestination.Visible Then
			ddlCustomerAccountDestination.Items.Add(New ListItem("", Guid.Empty.ToString()))
			For Each r As KaCustomerAccountLocation In KaCustomerAccountLocation.GetAll(GetUserConnection(_currentUser.Id), $"{KaCustomerAccountLocation.FN_DELETED} = 0 AND ({KaCustomerAccountLocation.FN_CUSTOMER_ACCOUNT_ID} = {Q(Guid.Empty)} OR {KaCustomerAccountLocation.FN_CUSTOMER_ACCOUNT_ID} = {Q(Guid.Parse(ddlCustomerAccount.SelectedValue.ToString()))})", "name ASC")
				ddlCustomerAccountDestination.Items.Add(New ListItem(r.Name, r.Id.ToString()))
			Next
			Try
				ddlCustomerAccountDestination.SelectedValue = selectedValue
			Catch ex As ArgumentOutOfRangeException
				ddlCustomerAccountDestination.SelectedIndex = 0
			End Try
		End If
	End Sub

	Private Sub SetTextboxMaxLengths()
		tbxName.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaEmailReport.TABLE_NAME, KaEmailReport.FN_NAME))
		tbxRecipients.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaEmailReport.TABLE_NAME, KaEmailReport.FN_RECIPIENTS))
		tbxSubject.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaEmailReport.TABLE_NAME, KaEmailReport.FN_SUBJECT))
	End Sub

	Protected Sub cbxTicketNumber_CheckedChanged(sender As Object, e As EventArgs) Handles cbxTicketNumber.CheckedChanged
		cbxIncludeVoidedTickets.Enabled = cbxTicketNumber.Checked
	End Sub

	Private Sub PopulateEmailAddressList()
		Utilities.PopulateEmailAddressList(tbxRecipients, ddlAddEmailAddress, btnAddEmailAddress)
		ddlReportRuntype_SelectedIndexChanged(ddlReportRuntype, New EventArgs())
		ddlAddEmailAddress.SelectedIndex = 0
	End Sub

	Protected Sub btnAddEmailAddress_Click(sender As Object, e As EventArgs) Handles btnAddEmailAddress.Click
		If ddlAddEmailAddress.SelectedIndex > 0 Then
			If tbxRecipients.Text.Trim.Length > 0 Then tbxRecipients.Text &= ";"
			tbxRecipients.Text &= ddlAddEmailAddress.SelectedValue
			PopulateEmailAddressList()
		End If
	End Sub

	Private Sub tbxRecipients_TextChanged(sender As Object, e As System.EventArgs) Handles tbxRecipients.TextChanged
		PopulateEmailAddressList()
	End Sub

	Private Sub PopulateTotalUnitsColumn(ByVal connection As OleDbConnection, ByVal totalUnitsAndDecimals As String)
		Dim units As New Dictionary(Of String, Integer)
		Dim unitsSelected As New Dictionary(Of String, Boolean)
		For Each unitInfo As KaUnit In KaUnit.GetAll(connection, $"deleted=0 AND base_unit<>9", "name ASC")
			Dim precision As String = unitInfo.UnitPrecision
			Dim decimalCount As Integer = 0
			If precision.IndexOf(".") >= 0 Then decimalCount = Math.Max(0, Math.Min(6, precision.Length - precision.IndexOf(".") - 1))
			units.Add(unitInfo.Id.ToString(), decimalCount)

			unitsSelected.Add(unitInfo.Id.ToString(), False)
			ddlTotalUnitsDecimals.Items.Add(New ListItem(unitInfo.Name, unitInfo.Id.ToString()))
		Next
		For Each unitItem As String In totalUnitsAndDecimals.Split("|")
			Dim values() As String = unitItem.Split(":")
			If units.ContainsKey(values(0)) AndAlso values.Length > 1 Then Integer.TryParse(values(1), units(values(0)))
			If unitsSelected.ContainsKey(values(0)) AndAlso values.Length > 2 Then Boolean.TryParse(values(2), unitsSelected(values(0)))
		Next

		PopulateTotalUnits(units, unitsSelected)
	End Sub

	Private Sub PopulateTotalUnits(ByVal units As Dictionary(Of String, Integer), unitsSelected As Dictionary(Of String, Boolean))
		cblTotalUnits.Items.Clear()
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		For Each unitId As String In units.Keys
			Try
				Dim unitInfo As New KaUnit(connection, Guid.Parse(unitId))
				Dim decimalDisplay As String = "0" & IIf(units(unitId) > 0, "." & "".PadRight(units(unitId), "0"), "")
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

	Private Function ConvertTotalUnitsAndDecimals() As String
		Dim totalUnitsAndDecimals As String = ""
		If pnlTotalUnitColumn.Visible Then
			For Each unitItem As ListItem In cblTotalUnits.Items
				If totalUnitsAndDecimals.Length > 0 Then totalUnitsAndDecimals &= "|"
				totalUnitsAndDecimals &= unitItem.Value & ":" & unitItem.Selected.ToString
			Next
		End If
		Return totalUnitsAndDecimals
	End Function

#Region " E-mail Scheduling "
	Private Sub PopulateDaysOfWeek()
		cblReportRunDays.Items.Clear()
		For n As DayOfWeek = DayOfWeek.Sunday To DayOfWeek.Saturday
			cblReportRunDays.Items.Add(New ListItem(n.ToString, n))
		Next
	End Sub

	Private Sub PopulateDatesInMonth()
		With ddlReportRunOnSpecificDateOfMonth.Items
			.Clear()
			For n As Integer = 1 To 31
				.Add(New ListItem(Tm2Database.AddOrdinalToInteger(n), n.ToString()))
			Next
			.Add(New ListItem("Last day", "32"))
		End With
	End Sub

	Private Sub PopulateReportTriggers(ByVal reportTriggers As List(Of KaEmailReportTrigger), ByVal selectedTriggerId As Guid)
		Dim triggerFound As Boolean = False
		lstEmailSchedule.Items.Clear()
		For Each emailTrigger As KaEmailReportTrigger In reportTriggers
			lstEmailSchedule.Items.Add(New ListItem(emailTrigger.Name, Tm2Database.ToXml(emailTrigger, GetType(KaEmailReportTrigger))))
			If selectedTriggerId.Equals(emailTrigger.Id) Then lstEmailSchedule.SelectedIndex = lstEmailSchedule.Items.Count - 1 : triggerFound = True
		Next
		If Not triggerFound AndAlso lstEmailSchedule.Items.Count > 0 Then lstEmailSchedule.SelectedIndex = 0
		lstEmailSchedule_SelectedIndexChanged(lstEmailSchedule, New EventArgs())
	End Sub

	Private Sub rblEmailScheduleType_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles rblEmailScheduleType.SelectedIndexChanged
		SetTriggerPanelVisibility()
	End Sub

	Private Sub lstEmailSchedule_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles lstEmailSchedule.SelectedIndexChanged
		Dim trigger As KaEmailReportTrigger = CurrentEmailTriggerSelected()
		If trigger IsNot Nothing Then
			With trigger
				For Each daysSelected As ListItem In cblReportRunDays.Items
					daysSelected.Selected = .DaysOfWeek.Contains(daysSelected.Value)
				Next
				cbxReportTriggerDisabled.Checked = .Disabled
				tbxReportRunScheduledInterval.Text = .RepetitionMinutes
				cbxReportRunScheduledIntervalCatchesUpAfterOneRun.Checked = .RepetitionRunsOnceIfNotCaughtUp
				tbxTriggerSendTime.Value = Format(.TimeToRun, "h:mm:ss tt")
				If .TriggerType = KaEmailReportTrigger.EmailReportTriggerType.Time Then
					rblEmailScheduleType.SelectedValue = "SendAtSpecificTime"
				ElseIf .TriggerType = KaEmailReportTrigger.EmailReportTriggerType.DateOfMonth Then
					rblEmailScheduleType.SelectedValue = "SendOnSpecificDateOfMonth"
					Try
						ddlReportRunOnSpecificDateOfMonth.SelectedValue = .DateInMonth.ToString()
					Catch ex As Exception
						ddlReportRunOnSpecificDateOfMonth.SelectedIndex = 0
					End Try
				ElseIf .TriggerType = KaEmailReportTrigger.EmailReportTriggerType.DayOfMonth Then
					rblEmailScheduleType.SelectedValue = "SendOnSpecificDayOfMonth"
					Try
						ddlReportRunOnSpecificDayOfMonth.SelectedValue = .DateInMonth.ToString()
					Catch ex As Exception
						ddlReportRunOnSpecificDayOfMonth.SelectedIndex = 0
					End Try
				Else
					rblEmailScheduleType.SelectedValue = "SendOnScheduledPeriod"
				End If
				rblEmailScheduleType_SelectedIndexChanged(rblEmailScheduleType, New EventArgs())
			End With
		End If
		SetTriggerPanelVisibility()
	End Sub

	Private Sub SetTriggerPanelVisibility()
		Dim triggerSelected As Boolean = lstEmailSchedule.Items.Count > 0 AndAlso lstEmailSchedule.SelectedIndex >= 0
		Dim scheduledAtSpecificTime As Boolean = rblEmailScheduleType.SelectedIndex >= 0 AndAlso rblEmailScheduleType.SelectedValue = "SendAtSpecificTime"
		Dim scheduledAtSpecificDateOfMonth As Boolean = rblEmailScheduleType.SelectedIndex >= 0 AndAlso rblEmailScheduleType.SelectedValue = "SendOnSpecificDateOfMonth"
		Dim scheduledAtSpecificDayOfMonth As Boolean = rblEmailScheduleType.SelectedIndex >= 0 AndAlso rblEmailScheduleType.SelectedValue = "SendOnSpecificDayOfMonth"
		pnlEmailScheduleType.Visible = triggerSelected
		pnlReportRunDays.Visible = triggerSelected And Not scheduledAtSpecificDateOfMonth
		pnlReportTriggerDisabled.Visible = triggerSelected
		btnUpdateSchedule.Visible = triggerSelected
		btnRemoveSchedule.Visible = triggerSelected
		pnlReportRunScheduledIntervalCatchesUpAfterOneRun.Visible = triggerSelected
		pnlTriggerSendTime.Visible = triggerSelected AndAlso (scheduledAtSpecificTime OrElse scheduledAtSpecificDateOfMonth OrElse scheduledAtSpecificDayOfMonth)
		pnlReportRunScheduledInterval.Visible = Not scheduledAtSpecificTime And Not scheduledAtSpecificDateOfMonth And Not scheduledAtSpecificDayOfMonth And triggerSelected
		pnlReportRunOnSpecificDateOfMonth.Visible = triggerSelected AndAlso scheduledAtSpecificDateOfMonth
		pnlReportRunOnSpecificDayOfMonth.Visible = triggerSelected AndAlso scheduledAtSpecificDayOfMonth
	End Sub

	Private Function CurrentEmailTriggerSelected() As KaEmailReportTrigger
		Try
			Return Tm2Database.FromXml(lstEmailSchedule.SelectedValue, GetType(KaEmailReportTrigger))
		Catch ex As Exception
			Return Nothing
		End Try
	End Function

	Private Function GetCurrentEmailTriggers() As List(Of KaEmailReportTrigger)
		Dim triggerList As New List(Of KaEmailReportTrigger)
		Try
			triggerList = New KaEmailReport(GetUserConnection(_currentUser.Id), Guid.Parse(ddlEmailReport.SelectedValue)).ReportTriggers
		Catch ex As Exception
		End Try
		Return triggerList
	End Function

	Private Sub btnRemoveSchedule_Click(sender As Object, e As System.EventArgs) Handles btnRemoveSchedule.Click
		If lstEmailSchedule.SelectedIndex >= 0 Then
			Try
				With CurrentEmailTriggerSelected()
					.Deleted = True
					.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
				End With
			Catch ex As RecordNotFoundException

			End Try
		End If
		PopulateReportTriggers(GetCurrentEmailTriggers(), Guid.Empty)
	End Sub

	Private Sub btnAddSchedule_Click(sender As Object, e As System.EventArgs) Handles btnAddSchedule.Click
		Dim emailTrigger As New KaEmailReportTrigger
		emailTrigger.EmailReportId = Guid.Parse(ddlEmailReport.SelectedValue)
		emailTrigger.SqlInsert(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
		PopulateReportTriggers(GetCurrentEmailTriggers(), emailTrigger.Id)
	End Sub

	Protected Sub btnUpdateSchedule_Click(sender As Object, e As EventArgs) Handles btnUpdateSchedule.Click
		Dim trigger As KaEmailReportTrigger = CurrentEmailTriggerSelected()
		If trigger Is Nothing Then Exit Sub
		With trigger
			Dim days As New List(Of DayOfWeek)
			For Each daysSelected As ListItem In cblReportRunDays.Items
				If daysSelected.Selected Then days.Add(CType(daysSelected.Value, DayOfWeek))
			Next
			.DaysOfWeek = days
			.Disabled = cbxReportTriggerDisabled.Checked
			.RepetitionMinutes = Double.Parse(tbxReportRunScheduledInterval.Text)
			.RepetitionRunsOnceIfNotCaughtUp = cbxReportRunScheduledIntervalCatchesUpAfterOneRun.Checked
			DateTime.TryParse(SQL_MINDATE.ToShortDateString & " " & tbxTriggerSendTime.Value, .TimeToRun)
			If rblEmailScheduleType.SelectedValue = "SendAtSpecificTime" Then
				.TriggerType = KaEmailReportTrigger.EmailReportTriggerType.Time
			ElseIf rblEmailScheduleType.SelectedValue = "SendOnSpecificDateOfMonth" Then
				.TriggerType = KaEmailReportTrigger.EmailReportTriggerType.DateOfMonth
				.DateInMonth = ddlReportRunOnSpecificDateOfMonth.SelectedValue
				.DaysOfWeek = New List(Of DayOfWeek)
			ElseIf rblEmailScheduleType.SelectedValue = "SendOnSpecificDayOfMonth" Then
				.TriggerType = KaEmailReportTrigger.EmailReportTriggerType.DayOfMonth
				.DateInMonth = ddlReportRunOnSpecificDayOfMonth.SelectedValue
			Else
				.TriggerType = KaEmailReportTrigger.EmailReportTriggerType.TimeSpan
			End If
			.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
			PopulateReportTriggers(GetCurrentEmailTriggers(), .Id)
		End With
	End Sub
#End Region

	Private Sub cbxIncludeAllBulkProducts_CheckedChanged(sender As Object, e As System.EventArgs) Handles cbxIncludeAllBulkProducts.CheckedChanged
		pnlBulkProductList.Visible = Not cbxIncludeAllBulkProducts.Checked AndAlso GetReportTypeEnumeration(ddlReportType.SelectedValue) = KaEmailReport.ReportTypes.BulkProductUsageReport
	End Sub

	Protected Sub ddlReportRuntype_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlReportRuntype.SelectedIndexChanged
		Dim saveFile As Boolean = ddlReportRuntype.SelectedValue = "SaveFile"
		pnlReportFileSaveLocation.Visible = saveFile
		pnlRecipients.Visible = Not saveFile
		rowAddAddress.Visible = Not saveFile AndAlso ddlAddEmailAddress.Items.Count > 1
	End Sub
	Private Sub SetControlUsabilityFromPermissions()
		With _currentUserPermission(_currentTableName)
			Dim shouldEnable = (.Edit AndAlso ddlEmailReport.SelectedIndex > 0) OrElse (.Create AndAlso ddlEmailReport.SelectedIndex = 0)
			pnlMain.Enabled = shouldEnable
			btnSave.Enabled = shouldEnable
			btnDelete.Enabled = Not Guid.Parse(ddlEmailReport.SelectedValue).Equals(Guid.Empty) AndAlso .Edit AndAlso .Delete
		End With
	End Sub

	Private Sub rbIncludeTicketsWithError_CheckedChanged(sender As Object, e As EventArgs) Handles rbShowTicketsExported.CheckedChanged, rbShowTicketsNotExported.CheckedChanged, rbIncludeTicketsWithoutErrors.CheckedChanged, rbIncludeTicketsWithError.CheckedChanged, rbIncludeTicketsWithIgnoredError.CheckedChanged
		If sender Is rbIncludeTicketsWithoutErrors AndAlso rbIncludeTicketsWithoutErrors.Checked Then
			rbIncludeTicketsWithError.Checked = False
			rbIncludeTicketsWithIgnoredError.Checked = False
		ElseIf sender Is rbIncludeTicketsWithError AndAlso rbIncludeTicketsWithError.Checked Then
			rbIncludeTicketsWithoutErrors.Checked = False
			rbIncludeTicketsWithIgnoredError.Checked = False
			cbxOnlyIncludeOrdersForThisInterface.Checked = False
		ElseIf sender Is rbIncludeTicketsWithIgnoredError AndAlso rbIncludeTicketsWithIgnoredError.Checked Then
			rbIncludeTicketsWithoutErrors.Checked = False
			rbIncludeTicketsWithError.Checked = False
			cbxOnlyIncludeOrdersForThisInterface.Checked = False
		ElseIf sender Is rbShowTicketsExported AndAlso rbShowTicketsExported.Checked Then
			rbShowTicketsNotExported.Checked = False
			rbIncludeTicketsWithoutErrors.Checked = False
			rbIncludeTicketsWithError.Checked = False
			rbIncludeTicketsWithIgnoredError.Checked = False
			cbxOnlyIncludeOrdersForThisInterface.Checked = False
		ElseIf sender Is rbShowTicketsNotExported AndAlso rbShowTicketsNotExported.Checked Then
			rbShowTicketsExported.Checked = False
			cbxIncludeTicketsMarkedManually.Checked = False
		End If

		cbxIncludeTicketsMarkedManually.Enabled = rbShowTicketsExported.Checked
		cbxOnlyIncludeOrdersForThisInterface.Enabled = rbShowTicketsNotExported.Checked And rbIncludeTicketsWithoutErrors.Checked
		rbIncludeTicketsWithoutErrors.Enabled = rbShowTicketsNotExported.Checked
		rbIncludeTicketsWithError.Enabled = rbShowTicketsNotExported.Checked
		rbIncludeTicketsWithIgnoredError.Enabled = rbShowTicketsNotExported.Checked
	End Sub

	Protected Sub ToolkitScriptManager1_AsyncPostBackError(ByVal sender As Object, ByVal e As System.Web.UI.AsyncPostBackErrorEventArgs)
		ToolkitScriptManager1.AsyncPostBackErrorMessage = e.Exception.Message
	End Sub

	Private Sub FillContainerColumnDisplayed(columnsDisplayed As ULong)
		cbxContainerNumberColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcNumber)
		cbxContainerLocationColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcLocation)
		cbxContainerStatusColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcStatus)
		cbxContainerProductColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcProduct)
		cbxContainerOwnerColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcOwner)
		cbxContainerAccountColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcAccount)
		cbxContainerLastTransactionColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcLastTransaction)
		cbxContainerEmptyWeightColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcEmptyWeight)
		cbxContainerVolumeColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcVolume)
		cbxContainerProductWeightColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcProductWeight)
		cbxContainerInServiceColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcInService)
		cbxContainerLastFilledColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcLastFilled)
		cbxContainerBulkProdEpaColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcBulkProdEpa)
		cbxContainerSealNumberColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcSealNumber)
		cbxContainerCreatedColumnShow.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcCreated)
		cbxContainerTypeColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcType)
		cbxContainerConditionColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcCondition)
		cbxContainerLastChangedColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcLastChanged)
		cbxContainerManufacturedColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcManufactured)
		cbxContainerLastInspectedColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcLastInspected)
		cbxContainerPassedInspectionColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcPassedInspection)
		cbxContainerRefillableColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcRefillable)
		cbxContainerLastCleanedColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcLastCleaned)
		cbxContainerNotesColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcNotes)
		cbxContainerSealBrokenColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcSealBroken)
		cbxContainerPassedPressureTestColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcPassedPressureTest)
		cbxContainerOneWayValvePresentColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcOneWayValvePresent)
		cbxContainerForOrderIdColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcForOrderId)
		cbxContainerEquipmentColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcEquipment)
		cbxContainerLastUserIdColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcLastUserId)
		cbxContainerAssignedLotColumnShow.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcLot)
	End Sub


	Function GetDisplayedContainerColumns() As ULong
		Dim columnsDisplayed As ULong = 0
		If cbxContainerNumberColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcNumber
		If cbxContainerLocationColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcLocation
		If cbxContainerStatusColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcStatus
		If cbxContainerProductColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcProduct
		If cbxContainerOwnerColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcOwner
		If cbxContainerAccountColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcAccount
		If cbxContainerLastTransactionColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcLastTransaction
		If cbxContainerEmptyWeightColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcEmptyWeight
		If cbxContainerVolumeColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcVolume
		If cbxContainerProductWeightColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcProductWeight
		If cbxContainerInServiceColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcInService
		If cbxContainerLastFilledColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcLastFilled
		If cbxContainerBulkProdEpaColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcBulkProdEpa
		If cbxContainerSealNumberColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcSealNumber
		If cbxContainerCreatedColumnShow.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcCreated
		If cbxContainerTypeColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcType
		If cbxContainerConditionColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcCondition
		If cbxContainerLastChangedColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcLastChanged
		If cbxContainerManufacturedColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcManufactured
		If cbxContainerLastInspectedColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcLastInspected
		If cbxContainerPassedInspectionColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcPassedInspection
		If cbxContainerRefillableColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcRefillable
		If cbxContainerLastCleanedColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcLastCleaned
		If cbxContainerNotesColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcNotes
		If cbxContainerSealBrokenColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcSealBroken
		If cbxContainerPassedPressureTestColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcPassedPressureTest
		If cbxContainerOneWayValvePresentColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcOneWayValvePresent
		If cbxContainerForOrderIdColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcForOrderId
		If cbxContainerEquipmentColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcEquipment
		If cbxContainerLastUserIdColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcLastUserId
		If cbxContainerAssignedLotColumnShow.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcLot

		Return columnsDisplayed
	End Function

End Class
