Imports KahlerAutomation.KaTm2Database
Imports System.Collections
Imports System.Linq

Public Class DefaultContainerLabel
	Inherits System.Web.UI.Page

#Region " Variable declaration "
#Region "     Settings "
	Private _labelHeight As Double = KaSetting.DefaultContainerLabelSettings.SD_LABEL_HEIGHT
	Private _labelWidth As Double = KaSetting.DefaultContainerLabelSettings.SD_LABEL_WIDTH
	Private _showTicketNumber As Boolean = KaSetting.DefaultContainerLabelSettings.SD_SHOW_TICKET_NUMBER
	Private _showOrderNumber As Boolean = KaSetting.DefaultContainerLabelSettings.SD_SHOW_ORDER_NUMBER
	Private _analysisEntriesRoundedDown As Boolean = KaSetting.DefaultContainerLabelSettings.SD_ANALYSIS_ENTRIES_ROUNDED_DOWN
	Private _blank1 As String = KaSetting.DefaultContainerLabelSettings.SD_BLANK1
	Private _blank2 As String = KaSetting.DefaultContainerLabelSettings.SD_BLANK2
	Private _blank3 As String = KaSetting.DefaultContainerLabelSettings.SD_BLANK3
	Private _displayBlendGroupNameAsProductName As Boolean = KaSetting.DefaultContainerLabelSettings.SD_DISPLAY_BLEND_GROUP_NAME_AS_PRODUCT_NAME
	Private _gradeAnalysisDecimalCountGreaterThanOne As Integer = KaSetting.DefaultContainerLabelSettings.SD_GRADE_ANALYSIS_DECIMAL_COUNT_GREATER_THAN_ONE
	Private _gradeAnalysisDecimalCountLessThanOne As Integer = KaSetting.DefaultContainerLabelSettings.SD_GRADE_ANALYSIS_DECIMAL_COUNT_LESS_THAN_ONE
	Private _hideZeroPercentAnalysisNutrients As Boolean = KaSetting.DefaultContainerLabelSettings.SD_HIDE_ZERO_PERCENT_ANALYSIS_NUTRIENTS
	Private _ntepCompliant As Boolean = KaSetting.DefaultContainerLabelSettings.SD_NTEP_COMPLIANT
	Private _disclaimer As String = KaSetting.DefaultContainerLabelSettings.SD_DISCLAIMER
	Private _showAcres As Boolean = KaSetting.DefaultContainerLabelSettings.SD_SHOW_ACRES
	Private _showAllCustomPostLoadQuestions As Boolean = KaSetting.DefaultContainerLabelSettings.SD_SHOW_ALL_CUSTOM_POST_LOAD_QUESTIONS
	Private _showAllCustomPreLoadQuestions As Boolean = KaSetting.DefaultContainerLabelSettings.SD_SHOW_ALL_CUSTOM_PRE_LOAD_QUESTIONS
	Private _showApplicationRateOnTicket As Boolean = KaSetting.DefaultContainerLabelSettings.SD_SHOW_APPLICATION_RATE_ON_TICKET
	Private _showApplicator As Boolean = KaSetting.DefaultContainerLabelSettings.SD_SHOW_APPLICATOR
	Private _showBranchLocation As Boolean = KaSetting.DefaultContainerLabelSettings.SD_SHOW_BRANCH_LOCATION
	Private _showBulkProductEpaNumberSummaryTotals As Boolean = KaSetting.DefaultContainerLabelSettings.SD_SHOW_BULK_PRODUCT_EPA_NUMBER_ON_SUMMARY_ON_TICKET
	Private _showBulkProductNotesSummaryTotals As Boolean = KaSetting.DefaultContainerLabelSettings.SD_SHOW_BULK_PRODUCT_NOTES_ON_SUMMARY_ON_TICKET
	Private _showBulkProductSummaryTotals As Boolean = KaSetting.DefaultContainerLabelSettings.SD_SHOW_BULK_PRODUCT_SUMMARY_TOTALS
	Private _showCarrier As Boolean = KaSetting.DefaultContainerLabelSettings.SD_SHOW_CARRIER
	Private _showCustomer As Boolean = KaSetting.DefaultContainerLabelSettings.SD_SHOW_CUSTOMER
	Private _showCustomerDestinationNotesOnTicket As Boolean = KaSetting.DefaultContainerLabelSettings.SD_SHOW_CUSTOMER_DESTINATION_NOTES_ON_TICKET
	Private _showCustomerLocation As Boolean = KaSetting.DefaultContainerLabelSettings.SD_SHOW_CUSTOMER_LOCATION
	Private _showCustomerNotesOnTicket As Boolean = KaSetting.DefaultContainerLabelSettings.SD_SHOW_CUSTOMER_NOTES_ON_TICKET
	Private _showDate As Boolean = KaSetting.DefaultContainerLabelSettings.SD_SHOW_DATE
	Private _showDensityOnTicket As Boolean = KaSetting.DefaultContainerLabelSettings.SD_SHOW_DENSITY_ON_TICKET
	Private _showDerivedFromOnTicket As Boolean = KaSetting.DefaultContainerLabelSettings.SD_SHOW_DERIVED_FROM_ON_TICKET
	Private _showDriver As Boolean = KaSetting.DefaultContainerLabelSettings.SD_SHOW_DRIVER
	Private _showDriverNumber As Boolean = KaSetting.DefaultContainerLabelSettings.SD_SHOW_DRIVER_NUMBER
	Private _showFertilizerGrade As Boolean = KaSetting.DefaultContainerLabelSettings.SD_SHOW_FERTILIZER_GRADE
	Private _showFertilizerGuaranteedAnalysis As Boolean = KaSetting.DefaultContainerLabelSettings.SD_SHOW_FERTILIZER_GUARANTEED_ANALYSIS
	Private _showLoadedByOnTicket As Boolean = KaSetting.DefaultContainerLabelSettings.SD_SHOW_LOADED_BY_ON_TICKET
	Private _showOwner As Boolean = KaSetting.DefaultContainerLabelSettings.SD_SHOW_OWNER
	Private _showProductHazardousMaterial As Boolean = KaSetting.DefaultContainerLabelSettings.SD_SHOW_PRODUCT_HAZARDOUS_MATERIAL
	Private _showProductNotesSummaryTotals As Boolean = KaSetting.DefaultContainerLabelSettings.SD_SHOW_PRODUCT_NOTES_ON_SUMMARY_ON_TICKET
	Private _showProductSummaryTotals As Boolean = KaSetting.DefaultContainerLabelSettings.SD_SHOW_PRODUCT_SUMMARY_TOTALS
	Private _showRinseEntries As Boolean = KaSetting.DefaultContainerLabelSettings.SD_SHOW_RINSE_ENTRIES
	Private _showRequestedQty As Boolean = KaSetting.DefaultContainerLabelSettings.SD_SHOW_REQUESTED_QUANTITIES
	Private _showTime As Boolean = KaSetting.DefaultContainerLabelSettings.SD_SHOW_TIME
	Private _showTotal As Boolean = KaSetting.DefaultContainerLabelSettings.SD_SHOW_TOTAL
	Private _showTransport As Boolean = KaSetting.DefaultContainerLabelSettings.SD_SHOW_TRANSPORT
	Private _useOriginalOrdersApplicationRate As Boolean = KaSetting.DefaultContainerLabelSettings.SD_USE_ORIGINAL_ORDERS_APPLICATION_RATE_ON_TICKET
	Private _showComments As Boolean = KaSetting.DefaultContainerLabelSettings.SD_SHOW_COMMENTS
	Private _showLabelCount As Boolean = KaSetting.DefaultContainerLabelSettings.SD_SHOW_LABEL_COUNT
#End Region

	Private _bulkProducts As New Dictionary(Of Guid, KaBulkProduct)
	Private _products As New Dictionary(Of Guid, KaProduct)
	Private _units As New Dictionary(Of Guid, KaUnit)
	Private _densityUnitPrecision As Dictionary(Of String, String) ' Of String = <mass unit ID>|<volume unit ID>; String = precision string	
	Private _gradeAnalysisFormatGreaterThanOne As String = "0"
	Private _gradeAnalysisFormatLessThanOne As String = "0.00"
	Private _hazMatTypeId As String = ""
	Private _selectedCustomPostLoadQuestions As String = ""
	Private _selectedCustomPreLoadQuestions As String = ""
	Private _showAdditionalUnitList As New List(Of Guid)
	Private _ticketCustomFieldsTable As New DataTable
	Private _truckWeightOrder() As String = {"T", "G", "N"}
	Private _totalMassRequestedPerProductGroup As New Dictionary(Of Guid, KaQuantity) ' Guid = product group ID
	Private _totalVolumeRequestedPerProductGroup As New Dictionary(Of Guid, KaQuantity) ' Guid = product group ID
	Private _totalMassRequestedPerItem As New Dictionary(Of Guid, KaQuantity) ' Guid = ticket item ID
	Private _totalVolumeRequestedPerItem As New Dictionary(Of Guid, KaQuantity) ' Guid = ticket item ID
	Private _totalMassRequested As KaQuantity
	Private _totalVolumeRequested As KaQuantity
	Private _totalMassDeliveredPerProductGroup As New Dictionary(Of Guid, KaQuantity) ' Guid = product group ID
	Private _totalVolumeDeliveredPerProductGroup As New Dictionary(Of Guid, KaQuantity) ' Guid = product group ID
	Private _totalMassDeliveredPerItem As New Dictionary(Of Guid, KaQuantity) ' Guid = ticket item ID
	Private _totalVolumeDeliveredPerItem As New Dictionary(Of Guid, KaQuantity) ' Guid = ticket item ID
	Private _productGroups As New List(Of Guid)
	Private _totalMassDelivered As KaQuantity
	Private _totalVolumeDelivered As KaQuantity
	Private _productDisplayedNotes As New Dictionary(Of Guid, List(Of String)) ' Dictionary(Of ProductId/GroupingId, List(Of Notes))
	Private _productOtherUnits As New Dictionary(Of Guid, List(Of Guid)) ' Dictionary(Of ProductId/GroupingId, List(Of UnitIds))
	Private _productGroupingIds As New Dictionary(Of Guid, Guid) ' Dictionary(Of TicketItemId, ProductId/GroupingId)
#End Region

	Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
		AssignTicketStyleSheet()
		Dim connection As OleDb.OleDbConnection = Tm2Database.Connection
		Dim orderInfo As KaOrder
		Try
			Dim ticket As New KaTicket(connection, Guid.Parse(Request.QueryString("ticket_id")))
			Dim compIndex As Integer = -1
			Dim batchIndex As Integer = -1
			If Request.QueryString("compIndex") IsNot Nothing AndAlso Integer.TryParse(Request.QueryString("compIndex"), compIndex) AndAlso compIndex >= 0 Then
				If Request.QueryString("batchIndex") IsNot Nothing Then Integer.TryParse(Request.QueryString("batchIndex"), batchIndex)
				Dim tiCounter As Integer = 0
				Dim tbiCounter As Integer = 0
				Do While tiCounter < ticket.TicketItems.Count
					If ticket.TicketItems(tiCounter).Compartment <> compIndex Then
						ticket.TicketItems.RemoveAt(tiCounter)
					Else
						If batchIndex >= 0 Then
							Dim ti As KaTicketItem = ticket.TicketItems(tiCounter)
							tbiCounter = 0
							Do While tbiCounter < ti.TicketBulkItems.Count
								If ti.TicketBulkItems(tbiCounter).Batch <> batchIndex Then
									ti.TicketBulkItems.RemoveAt(tbiCounter)
								Else
									tbiCounter += 1
								End If
							Loop
							ti.Delivered = 0
							ti.Requested = 0
							For Each tbi As KaTicketBulkItem In ti.TicketBulkItems
								ti.Requested += KaUnit.FastConvert(connection, New KaQuantity(tbi.Requested, tbi.UnitId), New KaRatio(tbi.RequestedDensity, tbi.RequestedWeightUnitId, tbi.RequestedVolumeUnitId), ti.UnitId, _units).Numeric
								ti.Delivered += KaUnit.FastConvert(connection, New KaQuantity(tbi.Delivered, tbi.UnitId), New KaRatio(tbi.Density, tbi.WeightUnitId, tbi.VolumeUnitId), ti.UnitId, _units).Numeric
							Next
						End If
						tiCounter += 1
					End If
				Loop
				Dim tfCounter As Integer = 0
				Do While tfCounter < ticket.TicketFunctions.Count
					Dim tf As KaTicketFunction = ticket.TicketFunctions(tfCounter)
					If Not tf.TicketItemId.Equals(Guid.Empty) AndAlso tf.Compartment <> compIndex Then
						ticket.TicketFunctions.RemoveAt(tfCounter)
					ElseIf Not tf.TicketItemId.Equals(Guid.Empty) AndAlso batchIndex >= 0 AndAlso tf.Compartment = compIndex AndAlso tf.Batch <> batchIndex Then
						ticket.TicketFunctions.RemoveAt(tfCounter)
					Else
						tfCounter += 1
					End If
				Loop
			End If

			Dim totalLabelCount As Integer = -1
			Dim labelNumber As Integer = -1
			If Request.QueryString("totalLabelCount") IsNot Nothing AndAlso Integer.TryParse(Request.QueryString("totalLabelCount"), totalLabelCount) AndAlso totalLabelCount > 0 Then
				If Request.QueryString("labelNumber") IsNot Nothing Then Integer.TryParse(Request.QueryString("labelNumber"), labelNumber)
			End If

			Try
				orderInfo = New KaOrder(Tm2Database.Connection, ticket.OrderId)
			Catch ex As RecordNotFoundException
				orderInfo = New KaOrder()
			End Try

			GetSettings(ticket, orderInfo)
			If ticket.InternalTransfer Then _showTicketNumber = False
			If orderInfo.Id.Equals(Guid.Empty) Then _showOrderNumber = False
			If _showApplicationRateOnTicket AndAlso _useOriginalOrdersApplicationRate AndAlso orderInfo.Acres = 0 Then _showApplicationRateOnTicket = False
			If _showApplicationRateOnTicket AndAlso Not _useOriginalOrdersApplicationRate AndAlso ticket.Acres = 0 Then _showApplicationRateOnTicket = False

			Dim style As String = "height: 2in;"
			If _labelHeight > 0 Then style &= $" height: {_labelHeight}in;"
			If _labelWidth > 0 Then style &= $" width: {_labelWidth}in;"

			form1.Attributes("Style") = style

			lblLabelCount.Text = $"{labelNumber} of {totalLabelCount}"
			lblLabelCount.Visible = _showLabelCount AndAlso totalLabelCount > 1 AndAlso labelNumber >= 1

			Dim units As New Dictionary(Of Guid, KaUnit)
			CalculateTicketTotals(ticket, units)
			litTicketNumber.Text = EncodeAsHtml(ticket.Number)
			If ticket.PointOfSale AndAlso _ntepCompliant Then litTicketNumber.Text &= " (point of sale)"
			pnlTicketNumber.Visible = _showTicketNumber
			litOrderNumber.Text = EncodeAsHtml(ticket.OrderNumber)
			pnlOrderNumber.Visible = _showOrderNumber
			litDateTime.Text = String.Format(IIf(_showDate AndAlso _showTime, "{0:g}", IIf(_showDate, "{0:d} ", "") & IIf(_showTime, "{0:t}", "")), ticket.LoadedAt)
			pnlDateTime.Visible = _showDate OrElse _showTime

			' populate the sold by information
			Try
				Dim owner As New KaOwner()
				Try
					owner = New KaOwner(connection, ticket.OwnerId)
				Catch ex As Exception
					'This will be LEGACY only.  Only gets here if they are running an old enough version of software that does not stamp the owner_id to the ticket.
					Try
						owner = New KaOwner(connection, orderInfo.OwnerId)
					Catch ex2 As RecordNotFoundException
						_showOwner = False
					End Try
				End Try
				litSoldBy.Text = owner.Name
				litSoldBy.Text = EncodeAsHtml(litSoldBy.Text)
				pnlSoldBy.Visible = _showOwner
			Catch ex As RecordNotFoundException ' suppress exception
				pnlSoldBy.Visible = False
			End Try

			' populate the branch information
			Try
				Dim branchInfo As New KaBranch(connection, ticket.BranchId)
				litBranchLocation.Text = branchInfo.Name
				litBranchLocation.Text = EncodeAsHtml(litBranchLocation.Text)
			Catch ex As RecordNotFoundException ' suppress exception
				_showBranchLocation = False
			End Try
			pnlBranch.Visible = _showBranchLocation AndAlso litBranchLocation.Text.Length > 0

			' populate the comments
			If _showComments Then
				litComments.Text = IIf(ticket.Notes.Trim().Length > 0, "<strong>Comments:</strong><br />", "") & EncodeAsHtml(ticket.Notes)
			Else
				litComments.Visible = False
			End If

			' populate the sold to information
			litSoldTo.Text = ""
			For Each customerAccount As KaTicketCustomerAccount In ticket.TicketCustomerAccounts
				If litSoldTo.Text.Length > 0 Then litSoldTo.Text &= vbCrLf & vbCrLf
				litSoldTo.Text &= customerAccount.Name
				If ticket.TicketCustomerAccounts.Count > 1 Then litSoldTo.Text &= String.Format(" ({0:0.0}%)", customerAccount.Percent)
				If _showCustomerNotesOnTicket AndAlso customerAccount.Notes.Trim.Length > 0 Then litSoldTo.Text &= vbCrLf & customerAccount.Notes
				Dim accountNumber As String = customerAccount.AccountNumber
				If customerAccount.AccountInterfaceSettingId <> Guid.Empty Then 'get account number from customer_account_interface_settings table
					Dim accountInterfaceSetting As New KaCustomerAccountInterfaceSettings(connection, customerAccount.AccountInterfaceSettingId)
					accountNumber = accountInterfaceSetting.CrossReference
				End If
				If Not String.IsNullOrEmpty(accountNumber) Then
					litSoldTo.Text &= vbCrLf & "Customer Account:" & accountNumber
				End If
			Next
			litSoldTo.Text = EncodeAsHtml(litSoldTo.Text)
			pnlSoldTo.Visible = _showCustomer AndAlso litSoldTo.Text.Trim.Length > 0

			' populate the ship to information
			litShipTo.Text = ticket.ShipToName
			litShipTo.Text = EncodeAsHtml(litShipTo.Text)
			pnlShipTo.Visible = _showCustomerLocation AndAlso litShipTo.Text.Length > 0

			'Populate the product information

			litProducts.Text = ""

			Dim totalMass As New KaQuantity(0.0, KaUnit.GetUnitIdForBaseUnit(connection, KaUnit.Unit.Pounds))
			Dim totalMassValid As Boolean = True
			Dim totalVolume As New KaQuantity(0.0, KaUnit.GetUnitIdForBaseUnit(connection, KaUnit.Unit.Gallons))
			Dim totalVolumeValid As Boolean = True
			Dim totalRequestedMass As New KaQuantity(0.0, KaUnit.GetUnitIdForBaseUnit(connection, KaUnit.Unit.Pounds))
			Dim totalRequestedVolume As New KaQuantity(0.0, KaUnit.GetUnitIdForBaseUnit(connection, KaUnit.Unit.Gallons))
			Dim fertilizerAnalysis As New Dictionary(Of String, AnalysisEntry)
			Dim hazMatAnalysis As New Dictionary(Of Guid, AnalysisEntry)
			Dim compartmentFertilizerAnalysis As New Dictionary(Of Integer, Dictionary(Of String, AnalysisEntry))
			Dim derivedFromList As New List(Of String)
			ticket.GetBulkItemAnalysisTotals(totalMass, totalMassValid, totalVolume, totalVolumeValid, fertilizerAnalysis, hazMatAnalysis, totalRequestedMass, totalRequestedVolume, derivedFromList, _bulkProducts, _units, compartmentFertilizerAnalysis)

			' Set Fertilizer analysis values
			If totalMassValid Then
				For Each nutrient As String In fertilizerAnalysis.Keys
					If totalMass.Numeric > 0 Then
						fertilizerAnalysis(nutrient).Data = 100.0 * Double.Parse(fertilizerAnalysis(nutrient).Data) / totalMass.Numeric ' get percentage from nutrient mass and total mass
					Else
						fertilizerAnalysis(nutrient).Data = 0
					End If
				Next
			Else
				For Each nutrient As String In fertilizerAnalysis.Keys
					fertilizerAnalysis(nutrient).Data = 0
				Next
			End If

			' Hazardous Material
			If _showProductHazardousMaterial Then
				litProducts.Text &= vbCrLf & GetHazmatAnalysis(hazMatAnalysis)
			End If

			' Product Summary
			If _showProductSummaryTotals Then
				Dim productSummary As ArrayList = KaReports.GetTicketDeliveredProductSummary(connection, ticket, _showAdditionalUnitList, _showRequestedQty, _densityUnitPrecision, ticket.Acres, _showProductNotesSummaryTotals, _ntepCompliant, _showRinseEntries, _showDensityOnTicket, _showApplicationRateOnTicket, _useOriginalOrdersApplicationRate, _displayBlendGroupNameAsProductName)
				style = String.Format("style=""border-bottom: 1px solid black; width:{0:0.0}%;""", 100.0 / CType(CType(productSummary(0), ArrayList).Count, Double))
				Dim productHeaderAttributeList As New List(Of String)
				For i As Integer = 1 To CType(productSummary(0), ArrayList).Count
					productHeaderAttributeList.Add(style)
				Next
				litProducts.Text &= vbCrLf & KaReports.GetTableHtml("", "", productSummary, False, "cellspacing=""0"" style=""width: 100%;""", "", productHeaderAttributeList, "style=""vertical-align:top;""", New List(Of String))
			End If

			' Bulk Products
			If _showBulkProductSummaryTotals Then
				Dim bulkProductSummary As ArrayList = KaReports.GetTicketDeliveredBulkProductSummary(connection, ticket, _showAdditionalUnitList, _showRequestedQty, _densityUnitPrecision, ticket.Acres, _showBulkProductNotesSummaryTotals, _ntepCompliant, _showBulkProductEpaNumberSummaryTotals, _showRinseEntries, _showDensityOnTicket, _showApplicationRateOnTicket, _useOriginalOrdersApplicationRate)
				style = String.Format("style=""border-bottom: 1px solid black; width:{0:0.0}%;""", 100.0 / CType(CType(bulkProductSummary(0), ArrayList).Count, Double))
				Dim bulkProductHeaderAttributeList As New List(Of String)
				For i As Integer = 1 To CType(bulkProductSummary(0), ArrayList).Count
					bulkProductHeaderAttributeList.Add(style)
				Next

				litProducts.Text &= vbCrLf & KaReports.GetTableHtml("", "", bulkProductSummary, False, "cellspacing=""0"" style=""width: 100%;""", "", bulkProductHeaderAttributeList, "style=""vertical-align:top;""", New List(Of String))
			End If

			If _showTotal Then
				litProducts.Text &= vbCrLf & GetNonCompartmentTotals(ticket, orderInfo, units)
			End If

			If totalMassValid Then
				If _showFertilizerGrade Then
					litProducts.Text &= vbCrLf & GetFertilizerGrade(fertilizerAnalysis)
				End If
				If _showFertilizerGuaranteedAnalysis Then
					litProducts.Text &= vbCrLf & GetFertilizerGuaranteedAnalysis(fertilizerAnalysis)
				End If
			End If
			If _showDerivedFromOnTicket Then
				Dim derived As String = ""
				derivedFromList.Sort(StringComparer.OrdinalIgnoreCase)
				For Each derivedFrom As String In derivedFromList
					If derivedFrom.Trim.Length > 0 Then
						If derived.Length > 0 Then derived &= ", "
						derived &= derivedFrom.Trim
					End If
				Next
				If derived.Length > 0 Then litProducts.Text &= vbCrLf & "<span class=""DerivedFromLabel"">Derived from: </span><span class=""DerivedFrom"">" & derived & "</span>"
			End If

			If totalMassValid Then
				Dim nutrientAmountWarningList As New List(Of String)
				For Each nutrient As String In fertilizerAnalysis.Keys
					With fertilizerAnalysis(nutrient)
						Dim percent As Double
						Try
							percent = Double.Parse(.Data)
							Dim decimalPrecision As String = IIf(percent > 0 AndAlso percent < 1, _gradeAnalysisFormatLessThanOne, _gradeAnalysisFormatGreaterThanOne)
							percent = Double.Parse(String.Format("{0:" & decimalPrecision & "}", percent))
						Catch ex As KeyNotFoundException
							Continue For
						End Try
						If .MaximumExceededWarning IsNot Nothing AndAlso .MaximumExceededWarning.Length > 0 AndAlso percent > Double.Parse(.Maximum) Then
							If Not nutrientAmountWarningList.Contains(.MaximumExceededWarning, StringComparer.OrdinalIgnoreCase) Then nutrientAmountWarningList.Add(.MaximumExceededWarning)
						End If
					End With
				Next
				Dim warnings As String = ""
				For Each warning As String In nutrientAmountWarningList
					warnings &= "<br /><span class=""nutrientAmountWarning"">" & EncodeAsHtml(warning) & "</span>"
				Next
				If warnings.Length > 0 Then litProducts.Text &= vbCrLf & warnings
			End If
			' Custom Fields
			If _showProductSummaryTotals Then
				Dim prodCustFieldUsed As Boolean = False
				Dim productCustomFields As New Dictionary(Of String, List(Of String))
				For Each ticketItem As KaTicketItem In ticket.TicketItems
					Dim productCustomField As String = ""
                    Utilities.GetCustomField(productCustomField, _ticketCustomFieldsTable, KaCropType.TABLE_NAME, ticketItem.Id)
                    Utilities.GetCustomField(productCustomField, _ticketCustomFieldsTable, KaOrderItem.TABLE_NAME, ticketItem.Id)
                    Utilities.GetCustomField(productCustomField, _ticketCustomFieldsTable, KaOwner.TABLE_NAME, ticketItem.Id)
                    Utilities.GetCustomField(productCustomField, _ticketCustomFieldsTable, KaProduct.TABLE_NAME, ticketItem.Id)
                    Utilities.GetCustomField(productCustomField, _ticketCustomFieldsTable, KaUnit.TABLE_NAME, ticketItem.Id)
                    If productCustomField.Trim.Length > 0 Then
						If Not productCustomFields.ContainsKey(ticketItem.Name) Then productCustomFields.Add(ticketItem.Name, New List(Of String))
						For Each custField As String In productCustomField.Split(vbCrLf)
							If Not productCustomFields(ticketItem.Name).Contains(custField.Replace(vbCrLf, "").Replace(vbCr, "").Replace(vbLf, "")) AndAlso custField.Trim.Length > 0 Then productCustomFields(ticketItem.Name).Add(custField.Replace(vbCrLf, "").Replace(vbCr, "").Replace(vbLf, "")) : prodCustFieldUsed = True
						Next
					End If
				Next
				If prodCustFieldUsed Then
					litProducts.Text &= vbCrLf & "<table><tr><td colspan=""2"">Products</td></tr>"
					For Each product As String In productCustomFields.Keys
						Dim prodCust As String = ""
						For Each custField As String In productCustomFields(product)
							If prodCust.Trim.Length > 0 Then prodCust &= vbCrLf
							prodCust &= custField
						Next
						If prodCust.Length > 0 Then
							litProducts.Text &= "<tr><td stlye=""vertical-align: top;"">" & EncodeAsHtml(product) & ":</td><td stlye=""vertical-align: top;"">" & EncodeAsHtml(prodCust) & "</td></tr>"
						End If
					Next
					litProducts.Text &= "</table>"
				End If
			End If
			If _showBulkProductSummaryTotals Then
				Dim bulkProdCustFieldUsed As Boolean = False
				Dim bulkProductCustomFields As New Dictionary(Of String, List(Of String))
				For Each ticketItem As KaTicketItem In ticket.TicketItems
					For Each ticketBulkItem As KaTicketBulkItem In ticketItem.TicketBulkItems
						Dim bulkProductCustomField As String = ""
                        Utilities.GetCustomField(bulkProductCustomField, _ticketCustomFieldsTable, KaBay.TABLE_NAME, ticketBulkItem.Id)
                        Utilities.GetCustomField(bulkProductCustomField, _ticketCustomFieldsTable, KaBulkProduct.TABLE_NAME, ticketBulkItem.Id)
                        Utilities.GetCustomField(bulkProductCustomField, _ticketCustomFieldsTable, KaCropType.TABLE_NAME, ticketBulkItem.Id)
                        Utilities.GetCustomField(bulkProductCustomField, _ticketCustomFieldsTable, KaOwner.TABLE_NAME, ticketBulkItem.Id)
                        Utilities.GetCustomField(bulkProductCustomField, _ticketCustomFieldsTable, KaPanel.TABLE_NAME, ticketBulkItem.Id)
                        Utilities.GetCustomField(bulkProductCustomField, _ticketCustomFieldsTable, KaUnit.TABLE_NAME, ticketBulkItem.Id)
                        If bulkProductCustomField.Trim.Length > 0 Then
							If Not bulkProductCustomFields.ContainsKey(ticketBulkItem.Name) Then bulkProductCustomFields.Add(ticketBulkItem.Name, New List(Of String))
							For Each custField As String In bulkProductCustomField.Split(vbCrLf)
								If Not bulkProductCustomFields(ticketBulkItem.Name).Contains(custField.Replace(vbCrLf, "").Replace(vbCr, "").Replace(vbLf, "")) AndAlso custField.Trim.Length > 0 Then bulkProductCustomFields(ticketBulkItem.Name).Add(custField.Replace(vbCrLf, "").Replace(vbCr, "").Replace(vbLf, "")) : bulkProdCustFieldUsed = True
							Next
						End If
					Next
				Next
				If bulkProdCustFieldUsed Then
					litProducts.Text &= vbCrLf & "<table><tr><td colspan=""2"">Bulk Products</td></tr>"
					For Each bulkProduct As String In bulkProductCustomFields.Keys
						Dim bulkProdCust As String = ""
						For Each custField As String In bulkProductCustomFields(bulkProduct)
							If bulkProdCust.Trim.Length > 0 Then bulkProdCust &= vbCrLf
							bulkProdCust &= custField
						Next
						If bulkProdCust.Length > 0 Then
							litProducts.Text &= "<tr><td stlye=""vertical-align: top;"">" & EncodeAsHtml(bulkProduct) & ":</td><td stlye=""vertical-align: top;"">" & EncodeAsHtml(bulkProdCust) & "</td></tr> "
						End If
					Next
					litProducts.Text &= "</table>"
				End If
			End If

			Dim ticketCustomFields As String = ""
            Utilities.GetCustomField(ticketCustomFields, _ticketCustomFieldsTable, KaInterface.TABLE_NAME, ticket.Id)
            Utilities.GetCustomField(ticketCustomFields, _ticketCustomFieldsTable, KaLocation.TABLE_NAME, ticket.Id)
            Utilities.GetCustomField(ticketCustomFields, _ticketCustomFieldsTable, KaOrder.TABLE_NAME, ticket.Id)

            'Get the Carrier Info
            Dim carrier As String = ticket.CarrierName & IIf(ticket.CarrierNumber.Length > 0, " (", "") & ticket.CarrierNumber & IIf(ticket.CarrierNumber.Length > 0, ")", "")
            Utilities.GetCustomField(carrier, _ticketCustomFieldsTable, KaCarrier.TABLE_NAME, ticket.Id)
            litCarrier.Text = EncodeAsHtml(carrier)
			rowCarrier.Visible = _showCarrier AndAlso litCarrier.Text.Trim.Length > 0

			'Get the Transport(s) Info
			litTransports.Text = GetTransportInfo(connection, ticket)
			rowTransport.Visible = _showTransport AndAlso litTransports.Text.Trim.Length > 0

			'Get the driver Info
			Dim driver As String = ticket.DriverName & IIf(_showDriverNumber, IIf(ticket.DriverNumber.Length > 0, " (", "") & ticket.DriverNumber & IIf(ticket.DriverNumber.Length > 0, ")", ""), "")
            Utilities.GetCustomField(driver, _ticketCustomFieldsTable, KaDriver.TABLE_NAME, ticket.Id)
            litDriver.Text = EncodeAsHtml(driver)
			If ticket.DriverSignature.Length > 0 Then
				litDriver.Text &= " <img alt=""Signature"" src=""data:image/jpg;base64," & ticket.DriverSignature & """ />"
			End If
			rowDriver.Visible = _showDriver AndAlso litDriver.Text.Trim.Length > 0

			litUser.Text = EncodeAsHtml(ticket.Username)
			If ticket.UserSignature.Length > 0 Then
				litUser.Text &= " <img alt=""UserSignature"" src=""data:image/jpg;base64," & ticket.UserSignature & """ />"
			End If
			rowUser.Visible = _showLoadedByOnTicket AndAlso litUser.Text.Trim().Length > 0

			'Show Custom Pre/Post Load Questions
			DisplayCustomPreAndPostLoadQuestions(connection, ticket)

			'Set the disclaimer
			litDisclaimer.Text = EncodeAsHtml(_disclaimer)
			rowDisclaimer.Visible = _disclaimer.Trim.Length > 0

			'Show the extra blanks
			If _blank1.Trim.Length > 0 Then
				lblBlank1.Text = EncodeAsHtml(_blank1)
				tdBlank1.Visible = True
				lineCell1.Visible = True
			End If
			If _blank2.Trim.Length > 0 Then
				lblBlank2.Text = EncodeAsHtml(_blank2)
				tdBlank2.Visible = True
				lineCell2.Visible = True
			End If
			If _blank3.Trim.Length > 0 Then
				lblBlank3.Text = EncodeAsHtml(_blank3)
				tdBlank3.Visible = True
				lineCell3.Visible = True
			End If
		Catch ex As FormatException
			litTicketNumber.Text = "Invalid ticket ID (" & Request.QueryString("ticket_id") & ")"
		Catch ex As ArgumentNullException ' suppress exception
		End Try
	End Sub

	Private Sub CalculateTicketTotals(ticket As KaTicket, units As Dictionary(Of Guid, KaUnit))
		Dim connection As OleDb.OleDbConnection = Tm2Database.Connection
		Dim massUnitId As Guid = KaUnit.GetSystemDefaultMassUnitOfMeasure(connection, Nothing)
		Dim volumeUnitId As Guid = KaUnit.GetSystemDefaultVolumeUnitOfMeasure(connection, Nothing)
		_totalMassRequested = New KaQuantity(0.0, massUnitId)
		_totalVolumeRequested = New KaQuantity(0.0, volumeUnitId)
		_totalMassDelivered = New KaQuantity(0.0, massUnitId)
		_totalVolumeDelivered = New KaQuantity(0.0, volumeUnitId)
		_productDisplayedNotes = New Dictionary(Of Guid, List(Of String)) ' Dictionary(Of ProductId/GroupingId, List(Of Notes))
		_productOtherUnits = New Dictionary(Of Guid, List(Of Guid))
		_productGroupingIds = New Dictionary(Of Guid, Guid)

		For Each ticketFunction In ticket.TicketFunctions
			If _showRinseEntries AndAlso ticketFunction.ProductNumber = 91 Then
				Dim ticketItem As KaTicketItem = ticketFunction.ConvertToTicketItemAndTicketBulkItem(connection, Nothing)
				If ticketItem.Id = Guid.Empty Then
					ticketItem.Id = Guid.NewGuid
					For Each bulkItem As KaTicketBulkItem In ticketItem.TicketBulkItems
						bulkItem.TicketItemId = ticketItem.Id
					Next
				End If
			End If
		Next
	End Sub

	Private Function GetUnitListForProductGroup(bayId As Guid, productGroupId As Guid) As List(Of Guid)
		Dim list As New List(Of Guid)
		Dim conditions As String = $"deleted=0 AND name={Q($"ContainerLabelSetting:{bayId}:{productGroupId}/{KaSetting.DefaultContainerLabelSettings.SN_ADDITIONAL_UNITS_FOR_PRODUCT_GROUPS}")}"
		Dim settings As ArrayList = KaSetting.GetAll(Tm2Database.Connection, conditions, "")
		If settings.Count = 0 Then
			conditions = $"deleted=0 AND name={Q($"ContainerLabelSetting:{Guid.Empty}:{productGroupId}/{KaSetting.DefaultContainerLabelSettings.SN_ADDITIONAL_UNITS_FOR_PRODUCT_GROUPS}")}"
			settings = KaSetting.GetAll(Tm2Database.Connection, conditions, "")
		End If
		If settings.Count > 0 Then
			For Each value As String In CType(settings(0), KaSetting).Value.Trim().Split(",")
				Try ' to parse the unit ID...
					Dim unitId As Guid = Guid.Parse(value)
					If unitId <> Guid.Empty AndAlso Not list.Contains(unitId) Then list.Add(unitId)
				Catch ex As FormatException ' couldn't parse the unit ID, ignore it...
				End Try
			Next
		End If
		Return list
	End Function

	Private Function GetProductGroupsPerUnit(bayId As Guid, productGroups As List(Of Guid)) As Dictionary(Of Guid, List(Of Guid))
		Dim productGroupsPerUnit As New Dictionary(Of Guid, List(Of Guid))
		For Each unitId As Guid In _showAdditionalUnitList
			productGroupsPerUnit(unitId) = New List(Of Guid)({Guid.Empty})
		Next
		If productGroups IsNot Nothing Then
			For Each productGroupId In productGroups
				For Each unitId As Guid In GetUnitListForProductGroup(bayId, productGroupId)
					Dim list As List(Of Guid)
					Try ' to lookup the product group list for this unit...
						list = productGroupsPerUnit(unitId)
					Catch ex As KeyNotFoundException ' there isn't a product group list for this unit yet...
						list = New List(Of Guid) ' start a new list
						productGroupsPerUnit(unitId) = list
					End Try
					If Not list.Contains(productGroupId) Then list.Add(productGroupId)
				Next
			Next
		End If
		Return productGroupsPerUnit
	End Function

	Private Enum FormatValues
		Delivered = 0
		Requested = 1
		ApplicationRate = 2
	End Enum

	Private Function FormatTotalQuantity(ByVal ticket As KaTicket, ByVal units As Dictionary(Of Guid, KaUnit), ByVal what As FormatValues, ByVal order As KaOrder) As String
		Dim bayId As Guid = Guid.Empty
		For Each ti As KaTicketItem In ticket.TicketItems
			For Each tbi As KaTicketBulkItem In ti.TicketBulkItems
				bayId = tbi.BayId
				If Not bayId.Equals(Guid.Empty) Then Exit For
			Next
			If Not bayId.Equals(Guid.Empty) Then Exit For
		Next
		Dim value As String = ""
		If what <> FormatValues.ApplicationRate OrElse ticket.Acres > 0.0 Then
			Dim density As New KaRatio(0.0, KaUnit.GetUnitIdForBaseUnit(Tm2Database.Connection, KaUnit.Unit.Pounds), KaUnit.GetUnitIdForBaseUnit(Tm2Database.Connection, KaUnit.Unit.Gallons))
			Dim productGroupsPerUnits As Dictionary(Of Guid, List(Of Guid)) = GetProductGroupsPerUnit(bayId, _productGroups)
			For Each unitId In productGroupsPerUnits.Keys
				Dim total As New KaQuantity(0.0, unitId)
				Dim unit As KaUnit = KaUnit.GetUnit(unitId, units)
				Dim quantity As KaQuantity
				If productGroupsPerUnits(unitId).Contains(Guid.Empty) Then ' try to display this unit for all product groups
					If KaUnit.IsWeight(unit.BaseUnit) Then
						If what = FormatValues.Requested Then
							quantity = _totalMassRequested
						Else ' delivered or application rate
							quantity = _totalMassDelivered
						End If
					Else
						If what = FormatValues.Requested Then
							quantity = _totalVolumeRequested
						Else ' delivered or application rate
							quantity = _totalVolumeDelivered
						End If
					End If
				Else ' this unit will only be used by some product groups
					quantity = Nothing
				End If
				Dim productGroupsForUnit As List(Of Guid)
				If quantity IsNot Nothing Then
					total.Numeric += KaUnit.FastConvert(Tm2Database.Connection, quantity, density, total.UnitId, units).Numeric
					productGroupsForUnit = New List(Of Guid)({Guid.Empty})
				Else
					Dim unitIdsToRemove As New List(Of Guid)
					productGroupsForUnit = productGroupsPerUnits(unitId)
					For Each productGroupId As Guid In productGroupsForUnit
						If productGroupId <> Guid.Empty Then ' we've already established that we can't display the complete total in this unit of measure
							Try
								If KaUnit.IsWeight(unit.BaseUnit) Then
									If what = FormatValues.Requested Then
										quantity = _totalMassRequestedPerProductGroup(productGroupId)
									Else ' delivered or application rate
										quantity = _totalMassDeliveredPerProductGroup(productGroupId)
									End If
								Else
									If what = FormatValues.Requested Then
										quantity = _totalVolumeRequestedPerProductGroup(productGroupId)
									Else ' delivered or application rate
										quantity = _totalVolumeDeliveredPerProductGroup(productGroupId)
									End If
								End If
								If quantity IsNot Nothing Then
									total.Numeric += KaUnit.FastConvert(Tm2Database.Connection, quantity, density, total.UnitId, units).Numeric
								Else ' this product group can't be displayed in this unit of measure, remove it from the list so that we don't show it's label later
									unitIdsToRemove.Add(productGroupId)
								End If
							Catch ex As KeyNotFoundException
								unitIdsToRemove.Add(productGroupId)
							End Try
						End If
					Next
					For Each unitIdToRemove As Guid In unitIdsToRemove
						productGroupsForUnit.Remove(unitIdToRemove)
					Next
				End If
				If productGroupsForUnit.Count > 0 Then
					If what = FormatValues.ApplicationRate Then
						If _useOriginalOrdersApplicationRate Then
							Dim connection As OleDb.OleDbConnection = Tm2Database.Connection
							Dim orderTotal As Double = 0.0
							If order.Acres > 0.0 Then
								Try
									For Each oi As KaOrderItem In order.OrderItems
										orderTotal += KaUnit.Convert(connection, New KaQuantity(oi.Request, oi.UnitId), New KaProduct(connection, oi.ProductId).GetDensity(connection, ticket.LocationId), unit.Id).Numeric
									Next
								Catch ex As UnitConversionException
									orderTotal = 0.0
								End Try
							End If
							value &= IIf(value.Length > 0, ", ", "") & String.Format("{0:" & unit.UnitPrecision & "} {1}/acre", orderTotal, unit.Abbreviation)
						Else
							If ticket.Acres > 0.0 Then
								total.Numeric /= ticket.Acres
							Else
								total.Numeric = 0.0
							End If
							value &= IIf(value.Length > 0, ", ", "") & String.Format("{0:" & unit.UnitPrecision & "} {1}" & "/acre", total.Numeric, unit.Abbreviation)
						End If
					Else
						value &= IIf(value.Length > 0, ", ", "") & String.Format("{0:" & unit.UnitPrecision & "} {1}", total.Numeric, unit.Abbreviation)
					End If
					Dim productGroupLabel As String = ""
					For Each productGroupId As Guid In productGroupsForUnit
						Try ' to get the product group name from the database...
							If productGroupId <> Guid.Empty Then productGroupLabel &= IIf(productGroupLabel.Length > 0, " & ", "") & New KaProductGroup(Tm2Database.Connection, productGroupId).Name
						Catch ex As RecordNotFoundException ' the product group isn't in the database...
							productGroupLabel &= IIf(productGroupLabel.Length > 0, " & ", "") & "?"
						End Try
					Next
					If productGroupLabel.Length > 0 Then value &= String.Format(" ({0})", productGroupLabel)
				End If
			Next
		Else
			value = ""
		End If
		Return value
	End Function
	Private Sub AssignTicketStyleSheet()
		If IO.File.Exists(Server.MapPath("") & "\Styles\TicketCustom.css") Then
			StyleSheet.Text = "<link href=""Styles/TicketCustom.css"" type=""text/css"" rel=""stylesheet"" />"
		Else
			Dim css As String = "<link href=""style.css"" type=""text/css"" rel=""stylesheet"" />"
			css &= vbCrLf & "<style type=""text/css"">"
			css &= vbCrLf & "/*Hazmat Styles*/"
			css &= vbCrLf & ".HazmatAnalysis{"
			css &= vbCrLf & "    width:100%;"
			css &= vbCrLf & "    border-spacing: 0px;"
			css &= vbCrLf & "}"
			css &= vbCrLf & ".HazmatAnalysis th{"
			css &= vbCrLf & "    border-bottom: 1px solid black"
			css &= vbCrLf & "}"
			css &= vbCrLf & ".FertilizerGuaranteedAnalysis {"
			css &= vbCrLf & "    border-collapse: collapse;"
			css &= vbCrLf & "}"
			'css &= vbCrLf & "@media screen"
			'css &= vbCrLf & "{"
			'css &= vbCrLf & "    #litComments"
			'css &= vbCrLf & "    {"
			'css &= vbCrLf & "       "
			'css &= vbCrLf & "    }"
			'css &= vbCrLf & "    #FertilizerGrade"
			'css &= vbCrLf & "    {"
			'css &= vbCrLf & "        "
			'css &= vbCrLf & "    }"
			'css &= vbCrLf & "    #FertilizerGuaranteedAnalysis"
			'css &= vbCrLf & "    {"
			'css &= vbCrLf & "        "
			'css &= vbCrLf & "    }"
			'css &= vbCrLf & "    #TransportTable"
			'css &= vbCrLf & "    {"
			'css &= vbCrLf & "        "
			'css &= vbCrLf & "    }"
			'css &= vbCrLf & "    #CompartmentDetails"
			'css &= vbCrLf & "    {"
			'css &= vbCrLf & "        "
			'css &= vbCrLf & "    }"
			'css &= vbCrLf & "}"
			css &= vbCrLf & "/*This section should try to print the ticket on 1 screen */"
			css &= vbCrLf & "@media print"
			css &= vbCrLf & "{"
			css &= vbCrLf & "    *"
			css &= vbCrLf & "    {"
			css &= vbCrLf & "        margin: 0 !important;"
			css &= vbCrLf & "        padding: 0 !important;"
			css &= vbCrLf & "    }"
			css &= vbCrLf & "    #controls, .footer, .footerarea"
			css &= vbCrLf & "    {"
			css &= vbCrLf & "        display: none;"
			css &= vbCrLf & "    }"
			css &= vbCrLf & "    html, body"
			css &= vbCrLf & "    {"
			css &= vbCrLf & "        /*changing width to 100% causes huge overflow and wrap*/"
			css &= vbCrLf & "        height: 100%;"
			css &= vbCrLf & "        background: #FFF;"
			css &= vbCrLf & "        overflow: visible !important;"
			css &= vbCrLf & "    }"
			css &= vbCrLf
			css &= vbCrLf & "    .template()"
			css &= vbCrLf & "    {"
			css &= vbCrLf & "        width: auto;"
			css &= vbCrLf & "        left: 0;"
			css &= vbCrLf & "        top: 0;"
			css &= vbCrLf & "    }"
			css &= vbCrLf & "    li"
			css &= vbCrLf & "    {"
			css &= vbCrLf & "        margin: 0 0 10px 20px !important;"
			css &= vbCrLf & "    }"
			css &= vbCrLf & "}"
			css &= vbCrLf & "</style>"
			StyleSheet.Text = css
		End If
	End Sub

	Private Function GetNonCompartmentTotals(ByVal ticket As KaTicket, ByVal orderInfo As KaOrder, units As Dictionary(Of Guid, KaUnit)) As String
		Dim connection As OleDb.OleDbConnection = Tm2Database.Connection
		Dim linkedTicket As Boolean = Not ticket.LinkedTicketsId.Equals(Guid.Empty)
		Dim orderDensityUnits As New List(Of String) 'value of each string is the 'mass unit:volume unit:product group id'.  Mass unit id, colon, volume unit id, colon, product group id.
		For Each item As KaTicketItem In ticket.TicketItems
			For Each bulkItem As KaTicketBulkItem In item.TicketBulkItems
				Dim densityUnitsString As String = bulkItem.WeightUnitId.ToString & ":" & bulkItem.VolumeUnitId.ToString & ":" & Guid.Empty.ToString
				If Not orderDensityUnits.Contains(densityUnitsString) Then
					orderDensityUnits.Add(densityUnitsString)
				End If
			Next
		Next
		' start the table
		Dim totalTable As String = "<table ID=""CompartmentDetails"" cellspacing=""0"" style=""width: 100%;"">"
		' add the header row
		totalTable &= "<tr>"
		If _showRequestedQty Then totalTable &= "<th style=""border-bottom: 1px solid black;"" class=""CompartmentHeader"">Requested</th>"
		totalTable &= "<th style=""border-bottom: 1px solid black;"" class=""CompartmentHeader"">" & IIf(linkedTicket, "Net allocated", "Net delivered") & "</th>"
		If _showDensityOnTicket Then totalTable &= "<th style=""border-bottom: 1px solid black;"" class=""CompartmentHeader"">Density</th>"
		If _showApplicationRateOnTicket Then totalTable &= "<th style=""border-bottom: 1px solid black;"" class=""CompartmentHeader"">Application rate</th>"
		totalTable &= "</tr>"

		' show totals
		totalTable &= "<tr>"
		If _showRequestedQty Then totalTable &= "<td class""CompartmentTotal"">" & FormatTotalQuantity(ticket, units, FormatValues.Requested, orderInfo) & " </td>"
		totalTable &= "<td class=""CompartmentTotal"">" & FormatTotalQuantity(ticket, units, FormatValues.Delivered, orderInfo) & "</td>"
		If _showDensityOnTicket Then
			Dim value As String = ""
			If _totalMassDelivered IsNot Nothing AndAlso _totalVolumeDelivered IsNot Nothing AndAlso _totalVolumeDelivered.Numeric > 0.0 Then
				For Each n As String In orderDensityUnits
					Dim charSplitArray As Char() = New Char() {":"c}
					Dim massUnitId As Guid = Guid.Parse(n.Split(charSplitArray)(0))
					Dim volumeUnitId As Guid = Guid.Parse(n.Split(charSplitArray)(1))
					Dim productGroupId As Guid = Guid.Parse(n.Split(charSplitArray)(2))
					Dim productGroupName As String = ""
					If productGroupId <> Guid.Empty Then
						Dim productGroup As KaProductGroup = New KaProductGroup(connection, productGroupId, Nothing)
						productGroupName = productGroup.Name
					End If
					Dim mass As KaQuantity = KaUnit.FastConvert(connection, _totalMassDelivered, New KaRatio(0.0, _totalMassDelivered.UnitId, _totalVolumeDelivered.UnitId), massUnitId, units)
					Dim volume As KaQuantity = KaUnit.FastConvert(connection, _totalVolumeDelivered, New KaRatio(0.0, _totalMassDelivered.UnitId, _totalVolumeDelivered.UnitId), volumeUnitId, units)
					Dim density As New KaRatio(mass.Numeric / volume.Numeric, mass.UnitId, volume.UnitId)

					Dim densityUnitPrecision As String
					Try ' to lookup the unit precision for these density units...
						densityUnitPrecision = _densityUnitPrecision(String.Format("{0}|{1}", density.NumeratorUnitId.ToString(), density.DenominatorUnitId.ToString()))
					Catch ex As KeyNotFoundException ' these density units are not in the dictionary...
						Dim massWhole As UInteger = 0, massFractional As UInteger = UInteger.MaxValue
						KaUnit.GetPrecisionDigits(KaUnit.GetUnit(connection, density.NumeratorUnitId, units).UnitPrecision, massWhole, massFractional)
						Dim volumeWhole As UInteger = 0, volumeFractional As UInteger = UInteger.MaxValue
						KaUnit.GetPrecisionDigits(KaUnit.GetUnit(connection, density.DenominatorUnitId, units).UnitPrecision, volumeWhole, volumeFractional)
						densityUnitPrecision = KaUnit.GetPrecisionString(Math.Max(massWhole, volumeWhole), Math.Min(massFractional, volumeFractional), ",", 0)
					End Try
					value &= IIf(value.Length > 0, ", ", "") & String.Format("{0:" & densityUnitPrecision & "} {1}/{2}", density.Numeric, KaUnit.GetUnit(connection, density.NumeratorUnitId, units).Abbreviation, KaUnit.GetUnit(connection, density.DenominatorUnitId, units).Abbreviation) & IIf(productGroupName.Trim.Length > 0, " (" & productGroupName & ")", "")
				Next
			Else
				value = "n/a"
			End If
			totalTable &= "<td>" & value & "</td>"
		End If
		If _showApplicationRateOnTicket Then
			totalTable &= String.Format("<td>{0}</td>", FormatTotalQuantity(ticket, units, FormatValues.ApplicationRate, orderInfo))
		End If
		totalTable &= "</tr>"
		totalTable &= "</table>"
		Return totalTable
	End Function

	Private Function GetFertilizerGrade(analysis As Dictionary(Of String, AnalysisEntry)) As String
		Return GetFertilizerGrade(analysis, False)
	End Function

	Private Function GetFertilizerGrade(analysis As Dictionary(Of String, AnalysisEntry), ByVal isCompartment As Boolean) As String
		Dim html As String = ""
		Dim nutrients() As String = {"N", "P", "K", "Ca", "Mg", "S", "B", "Cl", "Cu", "Fe", "Mn", "Mo", "Zn"}
		For Each nutrient As String In nutrients
			Dim percent As Double
			Try
				percent = Double.Parse(analysis(nutrient).Data)
				If _analysisEntriesRoundedDown Then
					If percent > 0 AndAlso percent < 1 Then
						percent = Math.Floor(percent * 10 ^ _gradeAnalysisDecimalCountLessThanOne) / 10 ^ _gradeAnalysisDecimalCountLessThanOne
					Else
						percent = Math.Floor(percent * 10 ^ _gradeAnalysisDecimalCountGreaterThanOne) / 10 ^ _gradeAnalysisDecimalCountGreaterThanOne
					End If
				Else
					percent = Double.Parse(String.Format("{0:" & IIf(percent > 0 AndAlso percent < 1, _gradeAnalysisFormatLessThanOne, _gradeAnalysisFormatGreaterThanOne) & "}", percent))
				End If
			Catch ex As KeyNotFoundException
				percent = 0
			End Try
			Dim include As Boolean = nutrient = "N" OrElse nutrient = "P" OrElse nutrient = "K"
			If Not include AndAlso percent > 0 AndAlso percent > Double.Parse(analysis(nutrient).Minimum) Then include = True
			If include Then html &= IIf(html.Length > 0, "-", "") & percent.ToString() & IIf(nutrient <> "N" AndAlso nutrient <> "P" AndAlso nutrient <> "K", nutrient, "")
		Next
		If Not isCompartment Then If html.Length > 0 Then html = "<br /><p ID=""FertilizerGrade""><strong>Grade:</strong> " & html & "</p>"
		Return html
	End Function

	Private Function GetNutrientLabel(nutrient As String) As String
		Select Case nutrient
			Case "N" : Return "Total Nitrogen (N)"
			Case "P" : Return "Available Phosphate (P2O5)"
			Case "K" : Return "Soluble Potash (K2O)"
			Case "Ca" : Return "Calcium (Ca)"
			Case "Mg" : Return "Magnesium (Mg)"
			Case "S" : Return "Sulfur (S)"
			Case "B" : Return "Boron (B)"
			Case "Cl" : Return "Chlorine (Cl)"
			Case "Cu" : Return "Copper (Cu)"
			Case "Fe" : Return "Iron (Fe)"
			Case "Mn" : Return "Manganese (Mn)"
			Case "Mo" : Return "Molybdenum (Mo)"
			Case "Zn" : Return "Zinc (Zn)"
			Case "Co" : Return "Cobalt (Co)"
			Case "Ni" : Return "Nickel (Ni)"
			Case "Na" : Return "Sodium (Na)"
			Case Else : Return ""
		End Select
	End Function

	Private Function GetFertilizerGuaranteedAnalysis(analysis As Dictionary(Of String, AnalysisEntry)) As String
		Dim html As String = ""
		For Each nutrient As String In analysis.Keys
			Dim decimalPrecision As String = ""
			Dim percent As Double
			Try
				percent = Double.Parse(analysis(nutrient).Data)
				decimalPrecision = IIf(percent > 0 AndAlso percent < 1, _gradeAnalysisFormatLessThanOne, _gradeAnalysisFormatGreaterThanOne)
				If _analysisEntriesRoundedDown Then
					If percent > 0 AndAlso percent < 1 Then
						percent = Math.Floor(percent * 10 ^ _gradeAnalysisDecimalCountLessThanOne) / 10 ^ _gradeAnalysisDecimalCountLessThanOne
					Else
						percent = Math.Floor(percent * 10 ^ _gradeAnalysisDecimalCountGreaterThanOne) / 10 ^ _gradeAnalysisDecimalCountGreaterThanOne
					End If
				Else
					percent = Double.Parse(String.Format("{0:" & decimalPrecision & "}", percent))
				End If
			Catch ex As KeyNotFoundException
				percent = 0
			End Try
			Dim include As Boolean = Not _hideZeroPercentAnalysisNutrients AndAlso (nutrient = "N" OrElse nutrient = "P" OrElse nutrient = "K")
			If Not include AndAlso percent > 0 AndAlso percent > Double.Parse(analysis(nutrient).Minimum) Then include = True
			If include Then
				Dim nutrientLabel As String = GetNutrientLabel(nutrient)
				If nutrientLabel.Length > 0 Then
					html &= String.Format("<tr><td style=""border-bottom: 1px dashed black;"" colspan=""2"" class=""FertilizerGuaranteedAnalysis"">{0}:</td><td class=""FertilizerGuaranteedAnalysis"">{1:" & decimalPrecision & "}%</td></tr>", nutrientLabel, percent)
				Else
					html &= String.Format("<tr><td></td><td colspan=""2"" class=""FertilizerGuaranteedAnalysis"">{0:" & decimalPrecision & "}% {1}</td></tr>", percent, nutrient)
				End If
			End If
		Next
		If html.Length > 0 Then html = "<table class=""FertilizerGuaranteedAnalysis""><tr style=""visibility: hidden;""><td style=""width:20px;""></td><td style=""width:180px;""></td><td style=""width:20px;""></td></tr><tr><td colspan=""3""><strong>Guaranteed Analysis:</strong></td></tr>" & html & "</table>"
		Return html
	End Function

	Private Function GetHazmatAnalysis(analysis As Dictionary(Of Guid, AnalysisEntry))
		Dim html As String = ""
		Dim list As New SortedDictionary(Of Integer, SortedDictionary(Of Double, List(Of AnalysisEntry)))
		For i = 0 To 2
			list.Add(i, New SortedDictionary(Of Double, Generic.List(Of AnalysisEntry)))
		Next
		For Each hazMatItem As Guid In analysis.Keys
			Dim entry As AnalysisEntry = analysis(hazMatItem)
			Dim amountLoaded As Double = Double.Parse(entry.Data)
			Dim isReportableQuantity As Boolean = amountLoaded >= entry.Maximum
			Dim unit As KaUnit
			Try
				unit = New KaUnit(Tm2Database.Connection, Guid.Parse(entry.UnitOfMeasure))
			Catch ex As Exception
				unit = Nothing
			End Try
			Dim rank As Integer
			If unit IsNot Nothing AndAlso amountLoaded >= entry.Maximum Then
				rank = 0
			ElseIf unit IsNot Nothing AndAlso amountLoaded > 0 Then
				rank = 1
			Else
				rank = 2
			End If
			If Not list(rank).ContainsKey(amountLoaded) Then list(rank).Add(amountLoaded, New List(Of AnalysisEntry))
			list(rank)(amountLoaded).Add(entry)
		Next
		For Each rank As Integer In list.Keys
			For Each amountLoaded As Double In list(rank).Keys
				For Each entry As AnalysisEntry In list(rank)(amountLoaded)
					Dim unit As KaUnit
					Try
						unit = New KaUnit(Tm2Database.Connection, Guid.Parse(entry.UnitOfMeasure))
					Catch ex As Exception
						unit = New KaUnit(Tm2Database.Connection, Guid.Parse(entry.Components.First.UnitOfMeasure)) 'Use fall back unit
					End Try
					Dim isReportableQuantity As Boolean = amountLoaded >= entry.Maximum
					html &= String.Format("<tr><td>Bulk</td><td >{0}</td><td>{1}</td><td>{2:" & unit.UnitPrecision & "} {3}</td></tr>",
								  IIf(rank = 0, "RQ", IIf(rank = 1, "LQ", "")), entry.Label, amountLoaded, unit.Abbreviation)
				Next
			Next
		Next
		If html.Length > 0 Then html = "<br><h3>Hazardous material(s)</h3><table class=""HazmatAnalysis""><tr><th>Packaging</th><th>HM</th><th>Description</th><th>Quantity</th></tr>" & html & "</table>"
		Return html
	End Function

	Private Function GetTransportInfo(ByVal oConn As OleDb.OleDbConnection, ByVal ticket As KaTicket) As String
		Dim transportsText As String = "<table class=""Transports"">"

		For Each transport As KaTicketTransport In ticket.TicketTransports
			transportsText &= "<tr>"
			transportsText &= "<td style=""vertical-align:text-top;"" class=""Transports"">" & transport.Name & IIf(transport.Number.Length > 0, " (", "") & transport.Number & IIf(transport.Number.Length > 0, ")</td>", "")
			If _truckWeightOrder.Length > 0 Then
				Dim unitInfo As New KaUnit(oConn, transport.UnitId)
				Dim total As Double = 0.0
				Dim totalRinse As Double = 0.0
				Dim totalValid As Boolean = True
				Dim tareValid As Boolean = transport.TareWeight > 0.0 OrElse transport.TaredAt > New DateTime(1900, 1, 1, 0, 0, 0)
				For Each ticketItem As KaTicketItem In ticket.TicketItems
					If ticketItem.TicketTransportId.Equals(transport.Id) Then
						For Each ticketBulkItem As KaTicketBulkItem In ticketItem.TicketBulkItems
							Try
								total += KaUnit.Convert(oConn, New KaQuantity(ticketBulkItem.Delivered, ticketBulkItem.UnitId), New KaRatio(ticketBulkItem.Density, ticketBulkItem.WeightUnitId, ticketBulkItem.VolumeUnitId), transport.UnitId).Numeric
							Catch ex As UnitConversionException
								totalValid = False
							End Try
						Next
					End If
				Next
				For Each ticketFunction As KaTicketFunction In ticket.TicketFunctions
					If ticketFunction.TicketTransportId.Equals(transport.Id) Then
						Try
							If ticketFunction.ProductNumber = 91 Then
								'Only include rinses here
								total += KaUnit.Convert(oConn, New KaQuantity(ticketFunction.Delivered, ticketFunction.UnitId), New KaRatio(ticketFunction.Density, ticketFunction.WeightUnitId, ticketFunction.VolumeUnitId), transport.UnitId).Numeric
								totalRinse += KaUnit.Convert(oConn, New KaQuantity(ticketFunction.Delivered, ticketFunction.UnitId), New KaRatio(ticketFunction.Density, ticketFunction.WeightUnitId, ticketFunction.VolumeUnitId), transport.UnitId).Numeric
							End If
						Catch ex As Exception
							totalValid = False
						End Try
					End If
				Next
				Dim unitFormat As String = "#,###,##0"
				If totalValid Then
					Dim grossLine As String = "<td class=""TransportsWeights"">&nbsp;</td><td style=""text-align:right;"" class=""TransportsWeights"">Gross:</td>" & "<td class=""TransportsWeights"">" & EncodeAsHtml(Format(transport.TareWeight + total, unitFormat)) & " " & EncodeAsHtml(unitInfo.Abbreviation) & "</td>"
					Dim tareLine As String = "<td class=""TransportsWeights"">&nbsp;</td><td style=""text-align:right;"" class=""TransportsWeights"">Tare:</td>" & "<td class=""TransportsWeights"">" & EncodeAsHtml(Format(transport.TareWeight, unitFormat)) & " " & EncodeAsHtml(unitInfo.Abbreviation) & "</td>"
					Dim netLine As String = "<td class=""TransportsWeights"">&nbsp;</td><td style=""text-align:right;"" class=""TransportsWeights"">Net:</td>" & "<td class=""TransportsWeights"">" & EncodeAsHtml(Format(total, unitFormat)) & " " & EncodeAsHtml(unitInfo.Abbreviation) & IIf(totalRinse > 0, " (" & EncodeAsHtml(Format(totalRinse, unitFormat)) & " " & EncodeAsHtml(unitInfo.Abbreviation) & " from rinse)", "") & "</td>"

					Select Case _truckWeightOrder(0)
						Case "G"
							If tareValid And total > 0.0 Then transportsText &= grossLine
						Case "T"
							If tareValid Then transportsText &= tareLine
						Case "N"
							If total > 0.0 Then transportsText &= netLine
					End Select
					If _truckWeightOrder.Length > 1 Then
						Select Case _truckWeightOrder(1)
							Case "G"
								If tareValid And total > 0.0 Then transportsText &= grossLine
							Case "T"
								If tareValid Then transportsText &= tareLine
							Case "N"
								If total > 0.0 Then transportsText &= netLine
						End Select
						If _truckWeightOrder.Length > 2 Then
							Select Case _truckWeightOrder(2)
								Case "G"
									If tareValid And total > 0.0 Then transportsText &= grossLine
								Case "T"
									If tareValid Then transportsText &= tareLine
								Case "N"
									If total > 0.0 Then transportsText &= netLine
							End Select
						End If
					End If
				End If
			End If
			Dim transportCustomFields As String = ""
            Utilities.GetCustomField(transportCustomFields, _ticketCustomFieldsTable, KaTransport.TABLE_NAME, transport.Id)
            Utilities.GetCustomField(transportCustomFields, _ticketCustomFieldsTable, KaTransportTypes.TABLE_NAME, transport.Id)
            Utilities.GetCustomField(transportCustomFields, _ticketCustomFieldsTable, KaUnit.TABLE_NAME, transport.Id)
            If transportCustomFields.Length > 0 Then transportsText &= "<td style=""vertical-align:text-top;"" class=""Transports"">" & EncodeAsHtml(transportCustomFields) & "</td>"
			transportsText &= "</tr>"
		Next

		transportsText &= "</table>"
		Return transportsText
	End Function

	Private Sub GetSettings(ByVal ticket As KaTicket, ByVal orderInfo As KaOrder)
		Dim bayId As Guid = Guid.Empty
		For Each ti As KaTicketItem In ticket.TicketItems
			For Each tbi As KaTicketBulkItem In ti.TicketBulkItems
				bayId = tbi.BayId
				If Not bayId.Equals(Guid.Empty) Then Exit For
			Next
			If Not bayId.Equals(Guid.Empty) Then Exit For
		Next

		If Not Double.TryParse(GetSettingByBayId(bayId, KaSetting.DefaultContainerLabelSettings.SN_LABEL_HEIGHT, KaSetting.DefaultContainerLabelSettings.SD_LABEL_HEIGHT), _labelHeight) Then _labelHeight = 0
		If Not Double.TryParse(GetSettingByBayId(bayId, KaSetting.DefaultContainerLabelSettings.SN_LABEL_WIDTH, KaSetting.DefaultContainerLabelSettings.SD_LABEL_WIDTH), _labelWidth) Then _labelWidth = 0
		Boolean.TryParse(GetSettingByBayId(bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_TICKET_NUMBER, KaSetting.DefaultContainerLabelSettings.SD_SHOW_TICKET_NUMBER), _showTicketNumber)
		Boolean.TryParse(GetSettingByBayId(bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_ORDER_NUMBER, KaSetting.DefaultContainerLabelSettings.SD_SHOW_ORDER_NUMBER), _showOrderNumber)
		Boolean.TryParse(GetSettingByBayId(bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_FERTILIZER_GRADE, KaSetting.DefaultContainerLabelSettings.SD_SHOW_FERTILIZER_GRADE), _showFertilizerGrade)
		Boolean.TryParse(GetSettingByBayId(bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_FERTILIZER_GUARANTEED_ANALYSIS, KaSetting.DefaultContainerLabelSettings.SD_SHOW_FERTILIZER_GUARANTEED_ANALYSIS), _showFertilizerGuaranteedAnalysis)
		Boolean.TryParse(GetSettingByBayId(bayId, KaSetting.DefaultContainerLabelSettings.SN_ANALYSIS_ENTRIES_ROUNDED_DOWN, KaSetting.DefaultContainerLabelSettings.SD_ANALYSIS_ENTRIES_ROUNDED_DOWN), _analysisEntriesRoundedDown)
		Dim analysisShowTrailingZeros As Boolean = False
		Boolean.TryParse(GetSettingByBayId(bayId, KaSetting.DefaultContainerLabelSettings.SN_ANALYSIS_SHOW_TRAILING_ZEROS, KaSetting.DefaultContainerLabelSettings.SD_ANALYSIS_SHOW_TRAILING_ZEROS), analysisShowTrailingZeros)
		Integer.TryParse(GetSettingByBayId(bayId, KaSetting.DefaultContainerLabelSettings.SN_GRADE_ANALYSIS_DECIMAL_COUNT_GREATER_THAN_ONE, KaSetting.DefaultContainerLabelSettings.SD_GRADE_ANALYSIS_DECIMAL_COUNT_GREATER_THAN_ONE), _gradeAnalysisDecimalCountGreaterThanOne)
		_gradeAnalysisFormatGreaterThanOne = "0" & IIf(_gradeAnalysisDecimalCountGreaterThanOne = 0, "", "." & "".PadRight(_gradeAnalysisDecimalCountGreaterThanOne, IIf(analysisShowTrailingZeros, "0", "#")))
		Integer.TryParse(GetSettingByBayId(bayId, KaSetting.DefaultContainerLabelSettings.SN_GRADE_ANALYSIS_DECIMAL_COUNT_LESS_THAN_ONE, KaSetting.DefaultContainerLabelSettings.SD_GRADE_ANALYSIS_DECIMAL_COUNT_LESS_THAN_ONE), _gradeAnalysisDecimalCountLessThanOne)
		_gradeAnalysisFormatLessThanOne = "0" & IIf(_gradeAnalysisDecimalCountLessThanOne = 0, "", "." & "".PadRight(_gradeAnalysisDecimalCountLessThanOne, IIf(analysisShowTrailingZeros, "0", "#")))

		For Each unitId As String In GetSettingByBayId(bayId, KaSetting.DefaultContainerLabelSettings.SN_ADDITIONAL_UNITS_FOR_TICKET, KaSetting.DefaultContainerLabelSettings.SD_ADDITIONAL_UNITS_FOR_TICKET).Trim().Split(",")
			Try ' to parse the unit ID...
				_showAdditionalUnitList.Add(Guid.Parse(unitId))
			Catch ex As FormatException ' couldn't parse the unit ID, ignore it...
			End Try
		Next
		Boolean.TryParse(GetSettingByBayId(bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_BULK_PRODUCT_SUMMARY_TOTALS, KaSetting.DefaultContainerLabelSettings.SD_SHOW_BULK_PRODUCT_SUMMARY_TOTALS), _showBulkProductSummaryTotals)
		Boolean.TryParse(GetSettingByBayId(bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_BULK_PRODUCT_NOTES_ON_SUMMARY_ON_TICKET, KaSetting.DefaultContainerLabelSettings.SD_SHOW_BULK_PRODUCT_NOTES_ON_SUMMARY_ON_TICKET), _showBulkProductNotesSummaryTotals)
		Boolean.TryParse(GetSettingByBayId(bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_BULK_PRODUCT_EPA_NUMBER_ON_SUMMARY_ON_TICKET, KaSetting.DefaultContainerLabelSettings.SD_SHOW_BULK_PRODUCT_EPA_NUMBER_ON_SUMMARY_ON_TICKET), _showBulkProductEpaNumberSummaryTotals)
		Boolean.TryParse(GetSettingByBayId(bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_PRODUCT_SUMMARY_TOTALS, KaSetting.DefaultContainerLabelSettings.SD_SHOW_PRODUCT_SUMMARY_TOTALS), _showProductSummaryTotals)
		Boolean.TryParse(GetSettingByBayId(bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_PRODUCT_NOTES_ON_SUMMARY_ON_TICKET, KaSetting.DefaultContainerLabelSettings.SD_SHOW_PRODUCT_NOTES_ON_SUMMARY_ON_TICKET), _showProductNotesSummaryTotals)
		Boolean.TryParse(GetSettingByBayId(bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_DERIVED_FROM_ON_TICKET, KaSetting.DefaultContainerLabelSettings.SD_SHOW_DERIVED_FROM_ON_TICKET), _showDerivedFromOnTicket)
		Boolean.TryParse(GetSettingByBayId(bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_REQUESTED_QUANTITIES, KaSetting.DefaultContainerLabelSettings.SD_SHOW_REQUESTED_QUANTITIES), _showRequestedQty)
		Boolean.TryParse(GetSettingByBayId(bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_DATE, KaSetting.DefaultContainerLabelSettings.SD_SHOW_DATE), _showDate)
		Boolean.TryParse(GetSettingByBayId(bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_TIME, KaSetting.DefaultContainerLabelSettings.SD_SHOW_TIME), _showTime)
		Boolean.TryParse(GetSettingByBayId(bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_CUSTOMER_LOCATION, KaSetting.DefaultContainerLabelSettings.SD_SHOW_CUSTOMER_LOCATION), _showCustomerLocation)
		Boolean.TryParse(GetSettingByBayId(bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_ACRES, KaSetting.DefaultContainerLabelSettings.SD_SHOW_ACRES), _showAcres)
		Boolean.TryParse(GetSettingByBayId(bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_CUSTOMER, KaSetting.DefaultContainerLabelSettings.SD_SHOW_CUSTOMER), _showCustomer)
		Boolean.TryParse(GetSettingByBayId(bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_TOTAL, KaSetting.DefaultContainerLabelSettings.SD_SHOW_TOTAL), _showTotal)
		Boolean.TryParse(GetSettingByBayId(bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_DRIVER, KaSetting.DefaultContainerLabelSettings.SD_SHOW_DRIVER), _showDriver)
		Boolean.TryParse(GetSettingByBayId(bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_DRIVER_NUMBER, KaSetting.DefaultContainerLabelSettings.SD_SHOW_DRIVER_NUMBER), _showDriverNumber)
		Boolean.TryParse(GetSettingByBayId(bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_CARRIER, KaSetting.DefaultContainerLabelSettings.SD_SHOW_CARRIER), _showCarrier)
		Boolean.TryParse(GetSettingByBayId(bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_BRANCH_LOCATION, KaSetting.DefaultContainerLabelSettings.SD_SHOW_BRANCH_LOCATION), _showBranchLocation)
		Boolean.TryParse(GetSettingByBayId(bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_TRANSPORT, KaSetting.DefaultContainerLabelSettings.SD_SHOW_TRANSPORT), _showTransport)
		Boolean.TryParse(GetSettingByBayId(bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_OWNER, KaSetting.DefaultContainerLabelSettings.SD_SHOW_OWNER), _showOwner)
		Boolean.TryParse(GetSettingByBayId(bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_APPLICATOR, KaSetting.DefaultContainerLabelSettings.SD_SHOW_APPLICATOR), _showApplicator)
		Boolean.TryParse(GetSettingByBayId(bayId, KaSetting.DefaultContainerLabelSettings.SN_NTEP_COMPLIANT, KaSetting.DefaultContainerLabelSettings.SD_NTEP_COMPLIANT), _ntepCompliant)
		Boolean.TryParse(GetSettingByBayId(bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_LOADED_BY_ON_TICKET, KaSetting.DefaultContainerLabelSettings.SD_SHOW_LOADED_BY_ON_TICKET), _showLoadedByOnTicket)
		_disclaimer = GetSettingByBayId(bayId, KaSetting.DefaultContainerLabelSettings.SN_DISCLAIMER, KaSetting.DefaultContainerLabelSettings.SD_DISCLAIMER)
		_blank1 = GetSettingByBayId(bayId, KaSetting.DefaultContainerLabelSettings.SN_BLANK1, KaSetting.DefaultContainerLabelSettings.SD_BLANK1)
		_blank2 = GetSettingByBayId(bayId, KaSetting.DefaultContainerLabelSettings.SN_BLANK2, KaSetting.DefaultContainerLabelSettings.SD_BLANK2)
		_blank3 = GetSettingByBayId(bayId, KaSetting.DefaultContainerLabelSettings.SN_BLANK3, KaSetting.DefaultContainerLabelSettings.SD_BLANK3)
		Dim showTransportTareWeights As Boolean = False
		If Boolean.TryParse(GetSettingByBayId(bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_TRANSPORT_TARE_WEIGHTS, KaSetting.DefaultContainerLabelSettings.SD_SHOW_TRANSPORT_TARE_WEIGHTS), showTransportTareWeights) AndAlso showTransportTareWeights Then
			_truckWeightOrder = GetSettingByBayId(bayId, KaSetting.DefaultContainerLabelSettings.SN_TRUCK_TGN_ORDER, KaSetting.DefaultContainerLabelSettings.SD_TRUCK_TGN_ORDER).Split("-")
		End If
		Boolean.TryParse(GetSettingByBayId(bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_CUSTOMER_NOTES_ON_TICKET, KaSetting.DefaultContainerLabelSettings.SD_SHOW_CUSTOMER_NOTES_ON_TICKET), _showCustomerNotesOnTicket)
		Boolean.TryParse(GetSettingByBayId(bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_CUSTOMER_DESTINATION_NOTES_ON_TICKET, KaSetting.DefaultContainerLabelSettings.SD_SHOW_CUSTOMER_DESTINATION_NOTES_ON_TICKET), _showCustomerDestinationNotesOnTicket)
		Boolean.TryParse(GetSettingByBayId(bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_DENSITY_ON_TICKET, KaSetting.DefaultContainerLabelSettings.SD_SHOW_DENSITY_ON_TICKET), _showDensityOnTicket)
		Boolean.TryParse(GetSettingByBayId(bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_PRODUCT_HAZARDOUS_MATERIAL, KaSetting.DefaultContainerLabelSettings.SD_SHOW_PRODUCT_HAZARDOUS_MATERIAL), _showProductHazardousMaterial)
		_densityUnitPrecision = New Dictionary(Of String, String)
		For Each densityUnitPrecision As String In GetSettingByBayId(bayId, KaSetting.DefaultContainerLabelSettings.SN_DENSITY_UNIT_PRECISION, KaSetting.DefaultContainerLabelSettings.SD_DENSITY_UNIT_PRECISION).ToString().Split(",")
			Dim parts() As String = densityUnitPrecision.Split("|")
			If parts.Length = 3 Then _densityUnitPrecision(String.Format("{0}|{1}", parts(0), parts(1))) = parts(2)
		Next
		Boolean.TryParse(GetSettingByBayId(bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_APPLICATION_RATE_ON_TICKET, KaSetting.DefaultContainerLabelSettings.SD_SHOW_APPLICATION_RATE_ON_TICKET), _showApplicationRateOnTicket)
		Boolean.TryParse(GetSettingByBayId(bayId, KaSetting.DefaultContainerLabelSettings.SN_USE_ORIGINAL_ORDERS_APPLICATION_RATE_ON_TICKET, KaSetting.DefaultContainerLabelSettings.SD_USE_ORIGINAL_ORDERS_APPLICATION_RATE_ON_TICKET), _useOriginalOrdersApplicationRate)
		Boolean.TryParse(GetSettingByBayId(bayId, KaSetting.DefaultContainerLabelSettings.SN_DISPLAY_BLEND_GROUP_NAME_AS_PRODUCT_NAME, KaSetting.DefaultContainerLabelSettings.SD_DISPLAY_BLEND_GROUP_NAME_AS_PRODUCT_NAME), _displayBlendGroupNameAsProductName)
		Boolean.TryParse(GetSettingByBayId(bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_RINSE_ENTRIES, KaSetting.DefaultContainerLabelSettings.SD_SHOW_RINSE_ENTRIES), _showRinseEntries)
		Boolean.TryParse(GetSettingByBayId(bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_COMMENTS, KaSetting.DefaultContainerLabelSettings.SD_SHOW_COMMENTS), _showComments)
		Boolean.TryParse(GetSettingByBayId(bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_LABEL_COUNT, KaSetting.DefaultContainerLabelSettings.SD_SHOW_LABEL_COUNT), _showLabelCount)

		' Custom fields     
		Dim showAllCustomFieldsOnDeliveryTicket As Boolean = False
		Boolean.TryParse(GetSettingByBayId(bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_ALL_CUSTOM_FIELDS_ON_DELIVERY_TICKET, KaSetting.DefaultContainerLabelSettings.SD_SHOW_ALL_CUSTOM_FIELDS_ON_DELIVERY_TICKET), showAllCustomFieldsOnDeliveryTicket)
		Dim customFieldsShown As String = ""
		For Each customFieldShown As String In GetSettingByBayId(bayId, KaSetting.DefaultContainerLabelSettings.SN_CUSTOM_FIELDS_ON_DELIVERY_TICKET, KaSetting.DefaultContainerLabelSettings.SD_CUSTOM_FIELDS_ON_DELIVERY_TICKET).ToString().Split(",")
			If customFieldShown.Length > 0 Then
				If customFieldsShown.Length > 0 Then customFieldsShown &= ","
				customFieldsShown &= Q(customFieldShown)
			End If
		Next
		' not showing all custom fields, and not having any checked will cause the IIF statement to cause the select query to not return any records
		Dim getAllTicketAndChildrenIdsSql As String = "SELECT id FROM tickets WHERE (id = " & Q(ticket.Id) & ") " &
			"UNION SELECT ticket_items.id FROM tickets INNER JOIN ticket_items ON tickets.id = ticket_items.ticket_id WHERE (tickets.id = " & Q(ticket.Id) & ") " &
			"UNION SELECT ticket_bulk_items.id FROM tickets INNER JOIN ticket_items AS ticket_items ON tickets.id = ticket_items.ticket_id INNER JOIN ticket_bulk_items ON ticket_bulk_items.ticket_item_id = ticket_items.id WHERE (tickets.id = " & Q(ticket.Id) & ") " &
			"UNION SELECT ticket_bulk_item_crop_types.id FROM tickets INNER JOIN ticket_items AS ticket_items ON tickets.id = ticket_items.ticket_id INNER JOIN ticket_bulk_items AS ticket_bulk_items ON ticket_bulk_items.ticket_item_id = ticket_items.id INNER JOIN ticket_bulk_item_crop_types ON ticket_bulk_item_crop_types.ticket_bulk_item_id = ticket_bulk_items.id WHERE (tickets.id = " & Q(ticket.Id) & ") " &
			"UNION SELECT ticket_customer_accounts.id FROM tickets INNER JOIN ticket_customer_accounts ON ticket_customer_accounts.ticket_id = tickets.id WHERE (tickets.id = " & Q(ticket.Id) & ") " &
			"UNION SELECT ticket_transports.id FROM tickets INNER JOIN ticket_transports ON ticket_transports.ticket_id = tickets.id WHERE (tickets.id = " & Q(ticket.Id) & ") " &
			"UNION SELECT ticket_transport_compartments.id FROM tickets INNER JOIN ticket_transport_compartments ON tickets.id = ticket_transport_compartments.ticket_id WHERE (tickets.id = " & Q(ticket.Id) & ")"
		Dim getCustomFieldsDA As New OleDb.OleDbDataAdapter("SELECT custom_fields.id, custom_fields.table_name, custom_fields.field_name, custom_fields_1.table_name AS source_table, custom_field_data.value " &
															"FROM custom_fields " &
															"INNER JOIN custom_fields AS custom_fields_1 ON custom_fields.delivery_ticket_creation_source_field = custom_fields_1.id " &
															"INNER JOIN custom_field_data ON custom_fields.id = custom_field_data.custom_field_id " &
															"WHERE (custom_field_data.record_id IN (" & getAllTicketAndChildrenIdsSql & ") ) AND (custom_fields.deleted = 0) AND (custom_fields_1.deleted = 0) AND (custom_field_data.deleted = 0) " &
															IIf(showAllCustomFieldsOnDeliveryTicket, "", IIf(customFieldsShown.Length > 0, "AND (custom_field_data.custom_field_id IN (" & customFieldsShown & ")) ", "AND (custom_fields.deleted = 1)")) &
															"ORDER BY custom_fields.table_name, custom_fields.field_name, source_table", Tm2Database.Connection)

		If Tm2Database.CommandTimeout > 0 Then getCustomFieldsDA.SelectCommand.CommandTimeout = Tm2Database.CommandTimeout
		getCustomFieldsDA.Fill(_ticketCustomFieldsTable)

		Boolean.TryParse(GetSettingByBayId(bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_ALL_CUSTOM_PRE_LOAD_QUESTIONS, KaSetting.DefaultContainerLabelSettings.SD_SHOW_ALL_CUSTOM_PRE_LOAD_QUESTIONS), _showAllCustomPreLoadQuestions)
		_selectedCustomPreLoadQuestions = GetSettingByBayId(bayId, KaSetting.DefaultContainerLabelSettings.SN_CUSTOM_PRE_LOAD_QUESTIONS, KaSetting.DefaultContainerLabelSettings.SD_CUSTOM_PRE_LOAD_QUESTIONS)
		Boolean.TryParse(GetSettingByBayId(bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_ALL_CUSTOM_POST_LOAD_QUESTIONS, KaSetting.DefaultContainerLabelSettings.SD_SHOW_ALL_CUSTOM_POST_LOAD_QUESTIONS), _showAllCustomPostLoadQuestions)
		_selectedCustomPostLoadQuestions = GetSettingByBayId(bayId, KaSetting.DefaultContainerLabelSettings.SN_CUSTOM_POST_LOAD_QUESTIONS, KaSetting.DefaultContainerLabelSettings.SD_CUSTOM_POST_LOAD_QUESTIONS)
		Boolean.TryParse(GetSettingByBayId(bayId, KaSetting.DefaultContainerLabelSettings.SN_HIDE_ZERO_PERCENT_ANALYSIS_NUTRIENTS, KaSetting.DefaultContainerLabelSettings.SD_HIDE_ZERO_PERCENT_ANALYSIS_NUTRIENTS), _hideZeroPercentAnalysisNutrients)
		_hazMatTypeId = KaSetting.GetSetting(Tm2Database.Connection, "General/DefaultHazmatAnalysis", "", False, Nothing)
	End Sub

	Public Shared Function GetSettingByBayId(ByVal bayId As Guid, ByVal settingName As String, ByVal defaultValue As Object) As Object
		Return GetSettingByBayId(Tm2Database.Connection, bayId, settingName, defaultValue)
	End Function

	Public Shared Function GetSettingByBayId(ByVal connection As OleDb.OleDbConnection, ByVal bayId As Guid, ByVal settingName As String, ByVal defaultValue As Object) As Object
		'Find the owner specific setting.
		Dim allSettings As ArrayList = KaSetting.GetAll(connection, "name = " & Q("ContainerLabelSetting:" & bayId.ToString & "/" & settingName) & " AND deleted = 0", "")
		If allSettings.Count = 1 Then
			Return allSettings.Item(0).value
		End If

		'If there isn't an owner specific setting, get the All Owners setting, if that doesn't exist either, use the default value.
		Dim retval As String = KaSetting.GetSetting(connection, "ContainerLabelSetting:" & Guid.Empty.ToString & "/" & settingName, defaultValue.ToString, False, Nothing)
		Return retval
	End Function

	Private Function EncodeAsHtml(ByVal text As String) As String
		Return Server.HtmlEncode(text).Replace(vbCrLf, "<br />").Replace(vbCr, "<br />").Replace(vbLf, "<br />")
	End Function

    Private Sub DisplayCustomPreAndPostLoadQuestions(ByVal connection As OleDb.OleDbConnection, ByVal ticket As KaTicket)
		Dim ticketPreLoadDatas As ArrayList = New ArrayList
		Dim ticketPostLoadDatas As ArrayList = New ArrayList

		'Sort through all the Custom Load Question Datas that are associated with the ticket.
		For Each tclqd As KaTicketCustomLoadQuestionData In ticket.CustomLoadQuestionDatas
			Dim clqd As KaCustomLoadQuestionData = New KaCustomLoadQuestionData(connection, tclqd.CustomLoadQuestionDataID)
			Dim clqf As KaCustomLoadQuestionFields = New KaCustomLoadQuestionFields(connection, clqd.CustomLoadQuestionFieldsId)

			If Not clqf.PostLoad AndAlso (_showAllCustomPreLoadQuestions OrElse _selectedCustomPreLoadQuestions.Contains(clqd.CustomLoadQuestionFieldsId.ToString)) Then
				ticketPreLoadDatas.Add(clqd)
			End If
			If clqf.PostLoad AndAlso (_showAllCustomPostLoadQuestions OrElse _selectedCustomPostLoadQuestions.Contains(clqd.CustomLoadQuestionFieldsId.ToString)) Then
				ticketPostLoadDatas.Add(clqd)
			End If
		Next

		'Display pre load questions
		rowCustomPreLoadQuestions.Visible = ticketPreLoadDatas.Count > 0
		For Each clqd As KaCustomLoadQuestionData In ticketPreLoadDatas
			Dim clqf As KaCustomLoadQuestionFields = New KaCustomLoadQuestionFields(connection, clqd.CustomLoadQuestionFieldsId)
			litCustomPreLoadQuestions.Text &= IIf(litCustomPreLoadQuestions.Text.Length > 0, "<br/>", "")
			litCustomPreLoadQuestions.Text &= clqf.Name & ": " & clqd.Data
		Next

		'Display post load questions
		rowCustomPostLoadQuestions.Visible = ticketPostLoadDatas.Count > 0
		For Each clqd As KaCustomLoadQuestionData In ticketPostLoadDatas
			Dim clqf As KaCustomLoadQuestionFields = New KaCustomLoadQuestionFields(connection, clqd.CustomLoadQuestionFieldsId)
			litCustomPostLoadQuestions.Text &= IIf(litCustomPostLoadQuestions.Text.Length > 0, "<br/>", "")
			litCustomPostLoadQuestions.Text &= clqf.Name & ": " & clqd.Data
		Next
	End Sub

	Protected Overrides Sub Finalize()
		MyBase.Finalize()
	End Sub
End Class