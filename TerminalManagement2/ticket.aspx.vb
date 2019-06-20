Imports KahlerAutomation.KaTm2Database
Imports System.Collections
Imports System.Linq

Partial Class ticket : Inherits System.Web.UI.Page
	Private _analysisEntriesRoundedDown As Boolean = False
	Private _blank1 As String = ""
	Private _blank2 As String = ""
	Private _blank3 As String = ""
	Private _bulkProducts As New Dictionary(Of Guid, KaBulkProduct)
	Private _compartmentFertilizerAnalysis As New Dictionary(Of Integer, Dictionary(Of String, AnalysisEntry))
	Private _densityUnitPrecision As Dictionary(Of String, String) ' Of String = <mass unit ID>|<volume unit ID>; String = precision string	
	Private _displayBlendGroupNameAsProductName As Boolean = False
	Private _gradeAnalysisDecimalCountGreaterThanOne As Integer = 0
	Private _gradeAnalysisDecimalCountLessThanOne As Integer = 2
	Private _gradeAnalysisFormatGreaterThanOne As String = "0"
	Private _gradeAnalysisFormatLessThanOne As String = "0.00"
	Private _hazMatTypeId As String = ""
	Private _hideZeroPercentAnalysisNutrients As Boolean = True
	Private _ntepCompliant As Boolean = True
	Private _ownerDisclaimer As String = ""
	Private _ownerLogoPath As String = ""
	Private _ownerMessage As String = ""
	Private _products As New Dictionary(Of Guid, KaProduct)
	Private _selectedCustomPostLoadQuestions As String = ""
	Private _selectedCustomPreLoadQuestions As String = ""
	Private _showAcres As Boolean = False
	Private _showAdditionalUnitList As New List(Of Guid)
	Private _showAllCustomPostLoadQuestions As Boolean = False
	Private _showAllCustomPreLoadQuestions As Boolean = False
	Private _showApplicationRateOnTicket As Boolean = False
	Private _showApplicator As Boolean = True
	Private _showBranchLocation As Boolean = True
	Private _showBulkProductEpaNumberSummaryTotals As Boolean = False
	Private _showBulkProductNotesSummaryTotals As Boolean = False
	Private _showBulkProductSummaryTotals As Boolean = False
	Private _showCarrier As Boolean = True
	Private _showCompartmentBulkIngredientEpaNumbers As Boolean = False
	Private _showCompartmentBulkIngredientLotNumber As Boolean = False
	Private _showCompartmentBulkIngredientNotes As Boolean = False
	Private _showCompartmentBulkIngredients As Boolean = True
	Private _showCompartmentFertilizerGrade As Boolean = False
	Private _showCompartmentFertilizerGuaranteedAnalysis As Boolean = True
	Private _showCompartmentLoadedIndex As Boolean = True
	Private _showCompartmentProductNotes As Boolean = True
	Private _showCompartments As Boolean = True
	Private _showCompartmentTotals As Boolean = False
	Private _showCustomer As Boolean = True
	Private _showCustomerDestinationNotesOnTicket As Boolean = False
	Private _showCustomerLocation As Boolean = True
	Private _showCustomerNotesOnTicket As Boolean = False
	Private _showDate As Boolean = True
	Private _showDensityOnTicket As Boolean = False
	Private _showDerivedFromOnTicket As Boolean = False
	Private _showDischargeLocation As Boolean = False
	Private _showDriver As Boolean = True
	Private _showDriverNumber As Boolean = True
	Private _showEmailAddress As Boolean = True
	Private _showFacility As Boolean = False
	Private _showFertilizerGrade As Boolean = False
	Private _showFertilizerGuaranteedAnalysis As Boolean = False
	Private _showLoadedByOnTicket As Boolean = False
	Private _showOrderSummary As Boolean = False
	Private _showOrderSummaryHistorical As Boolean = False
	Private _showOwner As Boolean = True
	Private _showProductHazardousMaterial As Boolean = False
	Private _showProductNotesSummaryTotals As Boolean = False
	Private _showProductSummaryTotals As Boolean = False
	Private _showPurchaseOrderNumber As Boolean = True
	Private _showRinseEntries As Boolean = False
	Private _showReleaseNumber As Boolean = False
	Private _showRequestedQty As Boolean = True
	Private _showTime As Boolean = True
	Private _showTotal As Boolean = True
	Private _showTransport As Boolean = True
	Private _ticketAddonUrl As String = ""
	Private _ticketCustomFieldsTable As New DataTable
	Private _truckWeightOrder() As String = {"T", "G", "N"}
	Private _units As New Dictionary(Of Guid, KaUnit)
	Private _useOriginalOrdersApplicationRate As Boolean = False

	Protected Sub Page_Load(sender As Object, e As System.EventArgs) Handles Me.Load
		AssignTicketStyleSheet()
		Dim connection As OleDb.OleDbConnection = Tm2Database.Connection
		Dim orderInfo As KaOrder
		Try
			Dim ticket As New KaTicket(connection, Guid.Parse(Request.QueryString("ticket_id")))
			Try
				orderInfo = New KaOrder(Tm2Database.Connection, ticket.OrderId)
			Catch ex As RecordNotFoundException
				orderInfo = New KaOrder()
			End Try
			GetSettings(ticket, orderInfo)
			If _showApplicationRateOnTicket AndAlso _useOriginalOrdersApplicationRate AndAlso orderInfo.Acres = 0 Then _showApplicationRateOnTicket = False
			If _showApplicationRateOnTicket AndAlso Not _useOriginalOrdersApplicationRate AndAlso ticket.Acres = 0 Then _showApplicationRateOnTicket = False

			Dim units As New Dictionary(Of Guid, KaUnit)
			CalculateTicketTotals(ticket, units)
			litTicketNumber.Text = EncodeAsHtml(ticket.Number)
			If ticket.PointOfSale AndAlso _ntepCompliant Then litTicketNumber.Text &= " (point of sale)"
			litOrderNumber.Text = EncodeAsHtml(ticket.OrderNumber)
			litDateTime.Text = String.Format(IIf(_showDate AndAlso _showTime, "{0:g}", IIf(_showDate, "{0:d} ", "") & IIf(_showTime, "{0:t}", "")), ticket.LoadedAt)
			pnlDateTime.Visible = _showDate OrElse _showTime
			If _showFacility Then
				Dim facilityText As String = ""
				Dim facilityInfo As New KaLocation()
				Try
					facilityInfo = New KaLocation(connection, ticket.LocationId)
				Catch ex As Exception
					facilityInfo = New KaLocation()
				End Try
				facilityText = facilityInfo.Name
				If facilityInfo.Street.Length > 0 Then facilityText &= vbCrLf & facilityInfo.Street
				If facilityInfo.City.Length + facilityInfo.State.Length + facilityInfo.ZipCode.Length > 0 Then facilityText &= vbCrLf & facilityInfo.City & " " & facilityInfo.State & " " & facilityInfo.ZipCode
				If facilityInfo.Phone.Length > 0 Then facilityText &= vbCrLf & facilityInfo.Phone
				If facilityInfo.Email.Length > 0 AndAlso _showEmailAddress Then facilityText &= vbCrLf & facilityInfo.Email
                Utilities.GetCustomField(litSoldBy.Text, _ticketCustomFieldsTable, KaLocation.TABLE_NAME, ticket.Id)

                litFacility.Text = EncodeAsHtml(facilityText)
			Else
				litFacility.Text = ""
			End If
			pnlFacility.Visible = litFacility.Text.Length > 0
			If _showDischargeLocation Then
				litDischargeLocation.Text = IIf(litFacility.Text.Length > 0, "<br />", "") & ticket.DischargeLocations
			Else
				litDischargeLocation.Text = ""
			End If
			pnlDischargeLocation.Visible = litDischargeLocation.Text.Length > 0
			cellDischargeLocation.Visible = pnlFacility.Visible Or pnlDischargeLocation.Visible
			cellPurchaseOrderNumber.Visible = _showPurchaseOrderNumber
			cellReleaseNumber.Visible = _showReleaseNumber
			If ticket.PurchaseOrder.Length > 0 Then litPurchaseOrderNumber.Text = EncodeAsHtml(ticket.PurchaseOrder)
			If _showReleaseNumber AndAlso ticket.ReleaseNumber.Length > 0 Then litReleaseNumber.Text = EncodeAsHtml(ticket.ReleaseNumber)

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
						owner = New KaOwner
					End Try
				End Try
				litSoldBy.Text = owner.Name
				If owner.Street.Length > 0 Then litSoldBy.Text &= vbCrLf & owner.Street
				If owner.City.Length + owner.State.Length + owner.ZipCode.Length > 0 Then litSoldBy.Text &= vbCrLf & owner.City & " " & owner.State & " " & owner.ZipCode
				If owner.Phone.Length > 0 Then litSoldBy.Text &= vbCrLf & owner.Phone
				If owner.Email.Length > 0 AndAlso _showEmailAddress Then litSoldBy.Text &= vbCrLf & owner.Email
                Utilities.GetCustomField(litSoldBy.Text, _ticketCustomFieldsTable, KaOwner.TABLE_NAME, ticket.Id)

                litSoldBy.Text = EncodeAsHtml(litSoldBy.Text)
				pnlSoldBy.Visible = _showOwner
			Catch ex As RecordNotFoundException ' suppress exception
				pnlSoldBy.Visible = False
			End Try

			' populate the branch information
			Try
				Dim branchInfo As New KaBranch(connection, ticket.BranchId)
				litBranchLocation.Text = branchInfo.Name
				If branchInfo.Street.Length > 0 Then litBranchLocation.Text &= vbCrLf & branchInfo.Street
				If branchInfo.City.Length + branchInfo.State.Length + branchInfo.ZipCode.Length > 0 Then litBranchLocation.Text &= vbCrLf & branchInfo.City & " " & branchInfo.State & " " & branchInfo.ZipCode
				If branchInfo.Phone.Length > 0 Then litBranchLocation.Text &= vbCrLf & branchInfo.Phone
				If branchInfo.Email.Length > 0 AndAlso _showEmailAddress Then litBranchLocation.Text &= vbCrLf & branchInfo.Email
                Utilities.GetCustomField(litBranchLocation.Text, _ticketCustomFieldsTable, KaBranch.TABLE_NAME, ticket.Id)

                litBranchLocation.Text = EncodeAsHtml(litBranchLocation.Text)
				pnlBranch.Visible = _showBranchLocation AndAlso litBranchLocation.Text.Length > 0
			Catch ex As RecordNotFoundException ' suppress exception
				pnlBranch.Visible = False
			End Try

			' populate the acres information
			Try
				litAcres.Text = ticket.Acres.ToString()
				litAcres.Text = EncodeAsHtml(litAcres.Text)
				pnlAcres.Visible = _showAcres AndAlso ticket.Acres > 0
			Catch ex As RecordNotFoundException
				pnlAcres.Visible = False
			End Try

			' populate the comments
			litComments.Text = IIf(ticket.Notes.Trim().Length > 0, "<strong>Comments:</strong><br />", "") & EncodeAsHtml(ticket.Notes)

			' populate the sold to information
			litSoldTo.Text = ""
			For Each customerAccount As KaTicketCustomerAccount In ticket.TicketCustomerAccounts
				If litSoldTo.Text.Length > 0 Then litSoldTo.Text &= vbCrLf & vbCrLf
				litSoldTo.Text &= customerAccount.Name
				If ticket.TicketCustomerAccounts.Count > 1 Then litSoldTo.Text &= String.Format(" ({0:0.0}%)", customerAccount.Percent)
				If customerAccount.Street.Length > 0 Then litSoldTo.Text &= vbCrLf & customerAccount.Street
				If customerAccount.City.Length + customerAccount.State.Length + customerAccount.ZipCode.Length > 0 Then litSoldTo.Text &= vbCrLf & customerAccount.City & " " & customerAccount.State & " " & customerAccount.ZipCode
				If customerAccount.Phone.Length > 0 Then litSoldTo.Text &= vbCrLf & customerAccount.Phone
				If customerAccount.Email.Length > 0 AndAlso _showEmailAddress Then litSoldTo.Text &= vbCrLf & customerAccount.Email
				If _showCustomerNotesOnTicket AndAlso customerAccount.Notes.Trim.Length > 0 Then litSoldTo.Text &= vbCrLf & customerAccount.Notes
                Utilities.GetCustomField(litSoldTo.Text, _ticketCustomFieldsTable, KaCustomerAccount.TABLE_NAME, customerAccount.Id)
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
			pnlSoldTo.Visible = _showCustomer

			' populate the ship to information
			litShipTo.Text = ticket.ShipToName
			If ticket.ShipToStreet.Length > 0 Then litShipTo.Text &= vbCrLf & ticket.ShipToStreet
			If ticket.ShipToCity.Length + ticket.ShipToState.Length + ticket.ShipToZipCode.Length > 0 Then litShipTo.Text &= vbCrLf & ticket.ShipToCity & " " & ticket.ShipToState & " " & ticket.ShipToZipCode
			If _showCustomerDestinationNotesOnTicket AndAlso ticket.ShipToNotes.Trim.Length > 0 Then litShipTo.Text &= vbCrLf & ticket.ShipToNotes
            Utilities.GetCustomField(litShipTo.Text, _ticketCustomFieldsTable, KaCustomerAccountLocation.TABLE_NAME, ticket.Id)

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
			Dim derivedFromList As New List(Of String)
			ticket.GetBulkItemAnalysisTotals(totalMass, totalMassValid, totalVolume, totalVolumeValid, fertilizerAnalysis, hazMatAnalysis, totalRequestedMass, totalRequestedVolume, derivedFromList, _bulkProducts, _units, _compartmentFertilizerAnalysis)

			' Set Fertilizer analysis values
			If totalMassValid Then
				For Each nutrient As String In fertilizerAnalysis.Keys
					If totalMass.Numeric > 0 Then
						fertilizerAnalysis(nutrient).Data = 100.0 * Double.Parse(fertilizerAnalysis(nutrient).Data) / totalMass.Numeric ' get percentage from nutrient mass and total mass
					Else
						fertilizerAnalysis(nutrient).Data = 0
					End If
				Next

				For Each compartmentNumber As Integer In _compartmentContents.Keys
					If Not _compartmentFertilizerAnalysis.ContainsKey(compartmentNumber) Then
						_compartmentFertilizerAnalysis(compartmentNumber) = New Dictionary(Of String, AnalysisEntry)
						For Each nutrient As String In fertilizerAnalysis.Keys
							_compartmentFertilizerAnalysis(compartmentNumber).Add(nutrient, Tm2Database.FromXml(Tm2Database.ToXml(fertilizerAnalysis(nutrient), GetType(AnalysisEntry)), GetType(AnalysisEntry)))
							_compartmentFertilizerAnalysis(compartmentNumber)(nutrient).Data = 0
						Next
					Else
						For Each nutrient As String In _compartmentFertilizerAnalysis(compartmentNumber).Keys
							Try
								_compartmentFertilizerAnalysis(compartmentNumber)(nutrient).Data = 100.0 * Double.Parse(_compartmentFertilizerAnalysis(compartmentNumber)(nutrient).Data) / _totalMassDeliveredPerCompartment(compartmentNumber).Numeric
							Catch ex As Exception
								_compartmentFertilizerAnalysis(compartmentNumber)(nutrient).Data = 0
							End Try
						Next
					End If
				Next
			Else
				For Each nutrient As String In fertilizerAnalysis.Keys
					fertilizerAnalysis(nutrient).Data = 0
				Next
				For Each compartmentNumber As Integer In _compartmentContents.Keys
					If Not _compartmentFertilizerAnalysis.ContainsKey(compartmentNumber) Then
						_compartmentFertilizerAnalysis(compartmentNumber) = New Dictionary(Of String, AnalysisEntry)
						For Each nutrient As String In fertilizerAnalysis.Keys
							_compartmentFertilizerAnalysis(compartmentNumber).Add(nutrient, Tm2Database.FromXml(Tm2Database.ToXml(fertilizerAnalysis(nutrient), GetType(AnalysisEntry)), GetType(AnalysisEntry)))
							_compartmentFertilizerAnalysis(compartmentNumber)(nutrient).Data = 0
						Next
					Else
						For Each nutrient As String In _compartmentFertilizerAnalysis(compartmentNumber).Keys
							_compartmentFertilizerAnalysis(compartmentNumber)(nutrient).Data = 0
						Next
					End If
				Next
			End If

			' Hazardous Material
			If _showProductHazardousMaterial Then
				litProducts.Text &= vbCrLf & GetHazmatAnalysis(hazMatAnalysis)
			End If

			' Product Summary
			If _showProductSummaryTotals Then
				Dim displayNtepInfo As Boolean = _ntepCompliant And Not _showCompartments ' If compartments are shown, they will take care of these warnings.  Shouldn't need to duplicate the messages
				Dim productSummary As ArrayList = KaReports.GetTicketDeliveredProductSummary(connection, ticket, _showAdditionalUnitList, _showRequestedQty, _densityUnitPrecision, ticket.Acres, _showProductNotesSummaryTotals, displayNtepInfo, _showRinseEntries, _showDensityOnTicket, _showApplicationRateOnTicket, _useOriginalOrdersApplicationRate, _displayBlendGroupNameAsProductName)

				Dim style As String = String.Format("style=""border-bottom: 1px solid black; width:{0:0.0}%;""", 100.0 / CType(CType(productSummary(0), ArrayList).Count, Double))
				Dim productHeaderAttributeList As New List(Of String)
				For i As Integer = 1 To CType(productSummary(0), ArrayList).Count
					productHeaderAttributeList.Add(style)
				Next
				litProducts.Text &= vbCrLf & "<br />" & vbCrLf & KaReports.GetTableHtml("", "Product(s) dispensed", productSummary, False, "cellspacing=""0"" style=""width: 100%;""", "", productHeaderAttributeList, "style=""vertical-align:top;""", New List(Of String))
			End If

			' Bulk Products
			If _showBulkProductSummaryTotals Then
				Dim displayNtepInfo As Boolean = _ntepCompliant And Not (_showCompartments AndAlso _showCompartmentBulkIngredients) ' If compartments are shown, they will take care of these warnings.  Shouldn't need to duplicate the messages

				Dim bulkProductSummary As ArrayList = KaReports.GetTicketDeliveredBulkProductSummary(connection, ticket, _showAdditionalUnitList, _showRequestedQty, _densityUnitPrecision, ticket.Acres, _showBulkProductNotesSummaryTotals, displayNtepInfo, _showBulkProductEpaNumberSummaryTotals, _showRinseEntries, _showDensityOnTicket, _showApplicationRateOnTicket, _useOriginalOrdersApplicationRate)
				Dim style As String = String.Format("style=""border-bottom: 1px solid black; width:{0:0.0}%;""", 100.0 / CType(CType(bulkProductSummary(0), ArrayList).Count, Double))
				Dim bulkProductHeaderAttributeList As New List(Of String)
				For i As Integer = 1 To CType(bulkProductSummary(0), ArrayList).Count
					bulkProductHeaderAttributeList.Add(style)
				Next
				litProducts.Text &= vbCrLf & "<br />" & vbCrLf & KaReports.GetTableHtml("", "Bulk product(s) dispensed", bulkProductSummary, False, "cellspacing=""0"" style=""width: 100%;""", "", bulkProductHeaderAttributeList, "style=""vertical-align:top;""", New List(Of String))
			End If

			' Order Summary
			If _showOrderSummary Then
				Dim columnCount As Integer = 2 + IIf(_showRequestedQty, 1, 0) + IIf(_showDensityOnTicket, 1, 0)
				Dim style As String = String.Format("style=""border-bottom: 1px solid black; width:{0:0.0}%;""", 100.0 / CType(columnCount, Double))
				Dim orderSummaryHeaderAttributeList As New List(Of String)
				For i As Integer = 1 To columnCount
					orderSummaryHeaderAttributeList.Add(style)
				Next
				Dim addlUnits As New List(Of KaUnit)
				For Each unitId As Guid In _showAdditionalUnitList
					Try
						addlUnits.Add(New KaUnit(connection, unitId))
					Catch ex As RecordNotFoundException
					End Try
				Next

				Dim productGroupAdditionalUnits As New Dictionary(Of Guid, List(Of KaUnit))
				For Each productGroup As KaProductGroup In KaProductGroup.GetAll(connection, "deleted=0", "name")
					Dim webTicketSettingFormat As String = "WebTicketSetting:{0}:{1}/AdditionalUnitsForProductGroup"
					Dim webTicketSetting As String = String.Format(webTicketSettingFormat, Guid.Empty.ToString(), productGroup.Id.ToString())
					Dim defaultOwnerProductUnits As String = KaSetting.GetSetting(Tm2Database.Connection, webTicketSetting, "")
					webTicketSetting = String.Format(webTicketSettingFormat, ticket.OwnerId.ToString(), productGroup.Id.ToString())
					productGroupAdditionalUnits.Add(productGroup.Id, New List(Of KaUnit))
					For Each unitIdString As String In KaSetting.GetSetting(Tm2Database.Connection, webTicketSetting, defaultOwnerProductUnits, False, Nothing).Trim().Split(",")
						Try ' to parse the unit ID...
							Dim unitId As Guid = Guid.Empty
							If Guid.TryParse(unitIdString, unitId) AndAlso Not unitId.Equals(Guid.Empty) Then
								Dim unitInfo As New KaUnit(connection, unitId)
								If Not productGroupAdditionalUnits(productGroup.Id).Contains(unitInfo) Then productGroupAdditionalUnits(productGroup.Id).Add(unitInfo)
							End If
						Catch ex As FormatException ' couldn't parse the unit ID, ignore it...
						Catch ex As RecordNotFoundException ' couldn't parse the unit ID, ignore it...
						End Try
					Next
				Next
				litProducts.Text &= vbCrLf & "<br />" & vbCrLf & KaReports.GetTableHtml("", "Order summary", KaReports.GetOrderProductSummary(connection, ticket.OrderId, True, _showRequestedQty, IIf(_showOrderSummaryHistorical, ticket.Batch, Integer.MaxValue), addlUnits, ticket.LocationId, productGroupAdditionalUnits), False, "cellspacing=""0"" style=""width: 100%;""", "", orderSummaryHeaderAttributeList, "", New List(Of String))
				Dim indexOfLastRow As Integer = litProducts.Text.LastIndexOf("<tr >")
				Dim lastRow As String = litProducts.Text.Substring(indexOfLastRow)
				Dim addBorderToLastRow As String = lastRow.Replace("<td>", "<td style='border-top: 1px solid black;'>")
				litProducts.Text = litProducts.Text.Replace(lastRow, addBorderToLastRow)
			End If

			' Compartments 
			If _showCompartments Then
				litProducts.Text &= vbCrLf & "<br /><h3>Compartment(s)</h3><br />" & vbCrLf & GetCompartmentContents(ticket, orderInfo, units)
			ElseIf _showTotal Then
				litProducts.Text &= vbCrLf & "<br /><h3>Totals</h3><br />" & vbCrLf & GetNonCompartmentTotals(ticket, orderInfo, units)
			End If

			If totalMassValid Then
				If _showFertilizerGrade Then
					If Not _showCompartmentFertilizerGrade OrElse _showCompartmentFertilizerGrade And Not orderInfo.DoNotBlend Then
						litProducts.Text &= vbCrLf & GetFertilizerGrade(fertilizerAnalysis)
					End If
					If _showCompartmentFertilizerGrade And orderInfo.DoNotBlend And Not _showCompartments Then
						Dim htmlCompartments As String = ""
						Dim htmlGrades As String = "<tr><td style=""border-bottom: 1px dashed black;"" class=""FertilizerGrade""></td>"
						For Each compartmentNumber As Integer In _compartmentContents.Keys
							htmlCompartments &= "<td align=""center""><strong>" & compartmentNumber + 1 & "</strong></td>"
							htmlGrades &= "<td style=""border-bottom: 1px dashed black;"" align=""center"" class=""FertilizerGrade"">"
							If _compartmentFertilizerAnalysis.ContainsKey(compartmentNumber) Then
								htmlGrades &= GetFertilizerGrade(_compartmentFertilizerAnalysis(compartmentNumber), True)
							Else
								htmlGrades &= " "
							End If
							htmlGrades &= "</td>"
						Next
						htmlGrades &= "</tr>"
						litProducts.Text &= vbCrLf & "<table class=""FertilizerGrade""><tr style=""visibility: hidden;""><td style=""width:220px;""/><td style=""width:60px;""></td><td style=""width:60px;""></td><td style=""width:60px;""></td></tr><tr><td><strong>Grade by Compartment</strong></td>" & htmlCompartments & "</tr>" & htmlGrades & "</table><br />"
					End If
				End If
				If _showFertilizerGuaranteedAnalysis Then
					If _showCompartmentFertilizerGuaranteedAnalysis And orderInfo.DoNotBlend Then
						litProducts.Text &= vbCrLf & GetCompartmentFertilizerGuaranteedAnalysis(fertilizerAnalysis)
					Else
						litProducts.Text &= vbCrLf & GetFertilizerGuaranteedAnalysis(fertilizerAnalysis)
					End If
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
			If _showProductSummaryTotals OrElse _showCompartments Then
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
			If _showBulkProductSummaryTotals OrElse (_showCompartments AndAlso _showCompartmentBulkIngredients) Then
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

            tdTicketCustomFields.InnerHtml = EncodeAsHtml(ticketCustomFields)
			trTicketCustomFields.Visible = ticketCustomFields.Length > 0

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

			'Get the Applicator Info
			Dim applicator As String = ticket.ApplicatorName & IIf(ticket.ApplicatorLicense.Length > 0, " (" & ticket.ApplicatorLicense & ")", "")
            Utilities.GetCustomField(applicator, _ticketCustomFieldsTable, KaApplicator.TABLE_NAME, ticket.Id)
            litApplicator.Text = EncodeAsHtml(applicator)
			rowApplicator.Visible = _showApplicator AndAlso litApplicator.Text.Trim.Length > 0

			litUser.Text = EncodeAsHtml(ticket.Username)
			If ticket.UserSignature.Length > 0 Then
				litUser.Text &= " <img alt=""UserSignature"" src=""data:image/jpg;base64," & ticket.UserSignature & """ />"
			End If
			rowUser.Visible = _showLoadedByOnTicket AndAlso litUser.Text.Trim().Length > 0

			'Show Custom Pre/Post Load Questions
			DisplayCustomPreAndPostLoadQuestions(connection, ticket)

			'Set the disclaimer
			litDisclaimer.Text = EncodeAsHtml(_ownerDisclaimer)
			rowDisclaimer.Visible = _ownerDisclaimer.Trim.Length > 0

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

			'Show Owner Message
			If _ownerMessage.Trim.Length > 0 Then
				litOwnerMessage.Text = EncodeAsHtml(_ownerMessage)
				trOwnerMessage.Visible = True
			End If

			'Set the logo
			Try
				If IO.File.Exists(_ownerLogoPath) Then
					Dim logoImage As Drawing.Image = Drawing.Image.FromFile(_ownerLogoPath)
					Dim toStream As New System.IO.MemoryStream
					logoImage.Save(toStream, logoImage.RawFormat)

					imgLogo.Src = "data:image/" & Utilities.GetMimeType(logoImage.RawFormat) & ";base64," & Convert.ToBase64String(toStream.GetBuffer)
					imgLogo.Visible = True
				Else
					imgLogo.Visible = False
				End If
			Catch ex As Exception
				imgLogo.Visible = False
			End Try

			If _ticketAddonUrl <> "" Then
				Try
					Dim webClient As New System.Net.WebClient
					Dim result As String = webClient.DownloadString(IIf(_ticketAddonUrl.Contains("\"), _ticketAddonUrl.Replace("\", "/"), _ticketAddonUrl) & "?ticket_id=" & ticket.Id.ToString)
					litTicketAddon.Text = result.Substring(result.IndexOf("<form"), result.IndexOf("</form>") - result.IndexOf("<form") + 7)
					litTicketAddon.Visible = True
				Catch ex As Exception
					'Suppress
				End Try
			End If
		Catch ex As FormatException
			litTicketNumber.Text = "Invalid ticket ID (" & Request.QueryString("ticket_id") & ")"
		Catch ex As ArgumentNullException ' suppress exception
		End Try
	End Sub

	Private _totalMassRequestedPerCompartment As New Dictionary(Of Integer, KaQuantity) ' Integer = compartment index
	Private _totalVolumeRequestedPerCompartment As New Dictionary(Of Integer, KaQuantity) ' Integer = compartment index
	Private _totalMassRequestedPerProductGroup As New Dictionary(Of Guid, KaQuantity) ' Guid = product group ID
	Private _totalVolumeRequestedPerProductGroup As New Dictionary(Of Guid, KaQuantity) ' Guid = product group ID
	Private _totalMassRequestedPerProductGroupPerCompartment As New Dictionary(Of String, KaQuantity) ' String = product group ID + compartment index
	Private _totalVolumeRequestedPerProductGroupPerCompartment As New Dictionary(Of String, KaQuantity) ' String = product group ID + compartment index
	Private _totalMassRequestedPerItem As New Dictionary(Of Guid, KaQuantity) ' Guid = ticket item ID
	Private _totalVolumeRequestedPerItem As New Dictionary(Of Guid, KaQuantity) ' Guid = ticket item ID
	Private _totalMassRequested As KaQuantity
	Private _totalVolumeRequested As KaQuantity
	Private _totalMassDeliveredPerCompartment As New Dictionary(Of Integer, KaQuantity) ' Integer = compartment index
	Private _totalVolumeDeliveredPerCompartment As New Dictionary(Of Integer, KaQuantity) ' Integer = compartment index
	Private _totalMassDeliveredPerProductGroup As New Dictionary(Of Guid, KaQuantity) ' Guid = product group ID
	Private _totalVolumeDeliveredPerProductGroup As New Dictionary(Of Guid, KaQuantity) ' Guid = product group ID
	Private _totalMassDeliveredPerProductGroupPerCompartment As New Dictionary(Of String, KaQuantity) ' Guid = product group ID + compartment index
	Private _totalVolumeDeliveredPerProductGroupPerCompartment As New Dictionary(Of String, KaQuantity) ' Guid = product group ID + compartment index
	Private _totalMassDeliveredPerItem As New Dictionary(Of Guid, KaQuantity) ' Guid = ticket item ID
	Private _totalVolumeDeliveredPerItem As New Dictionary(Of Guid, KaQuantity) ' Guid = ticket item ID
	Private _productGroups As New List(Of Guid)
	Private _productGroupsPerCompartment As New Dictionary(Of Integer, List(Of Guid))
	Private _totalMassDelivered As KaQuantity
	Private _totalVolumeDelivered As KaQuantity

	Private Function LookupQuantity(key As Guid, quantities As Dictionary(Of Guid, KaQuantity), defaultUnitId As Guid) As KaQuantity
		Dim quantity As KaQuantity
		Try
			quantity = quantities(key)
		Catch ex As KeyNotFoundException
			quantity = New KaQuantity(0.0, defaultUnitId)
			quantities(key) = quantity
		End Try
		Return quantity
	End Function

	Private Function LookupQuantity(key As Integer, quantities As Dictionary(Of Integer, KaQuantity), defaultUnitId As Guid) As KaQuantity
		Dim quantity As KaQuantity
		Try
			quantity = quantities(key)
		Catch ex As KeyNotFoundException
			quantity = New KaQuantity(0.0, defaultUnitId)
			quantities(key) = quantity
		End Try
		Return quantity
	End Function

	Private Function LookupQuantity(key As String, quantities As Dictionary(Of String, KaQuantity), defaultUnitId As Guid) As KaQuantity
		Dim quantity As KaQuantity
		Try
			quantity = quantities(key)
		Catch ex As KeyNotFoundException
			quantity = New KaQuantity(0.0, defaultUnitId)
			quantities(key) = quantity
		End Try
		Return quantity
	End Function

	Private Sub UpdateTotal(key As Guid, quantities As Dictionary(Of Guid, KaQuantity), quantity As KaQuantity, density As KaRatio, defaultUnitId As Guid, units As Dictionary(Of Guid, KaUnit))
		Dim total As KaQuantity = LookupQuantity(key, quantities, defaultUnitId)
		If total IsNot Nothing Then
			Try
				total.Numeric += KaUnit.FastConvert(Tm2Database.Connection, quantity, density, total.UnitId, units).Numeric
			Catch ex As UnitConversionException
				quantities(key) = Nothing
			End Try
		End If
	End Sub

	Private Sub UpdateTotal(key As Integer, quantities As Dictionary(Of Integer, KaQuantity), quantity As KaQuantity, density As KaRatio, defaultUnitId As Guid, units As Dictionary(Of Guid, KaUnit))
		Dim total As KaQuantity = LookupQuantity(key, quantities, defaultUnitId)
		If total IsNot Nothing Then
			Try
				total.Numeric += KaUnit.FastConvert(Tm2Database.Connection, quantity, density, total.UnitId, units).Numeric
			Catch ex As UnitConversionException
				quantities(key) = Nothing
			End Try
		End If
	End Sub

	Private Sub UpdateTotal(key As String, quantities As Dictionary(Of String, KaQuantity), quantity As KaQuantity, density As KaRatio, defaultUnitId As Guid, units As Dictionary(Of Guid, KaUnit))
		Dim total As KaQuantity = LookupQuantity(key, quantities, defaultUnitId)
		If total IsNot Nothing Then
			Try
				total.Numeric += KaUnit.FastConvert(Tm2Database.Connection, quantity, density, total.UnitId, units).Numeric
			Catch ex As UnitConversionException
				quantities(key) = Nothing
			End Try
		End If
	End Sub

	Private Sub UpdateTotal(ByRef total As KaQuantity, quantity As KaQuantity, density As KaRatio, units As Dictionary(Of Guid, KaUnit))
		If total IsNot Nothing Then
			Try
				total.Numeric += KaUnit.FastConvert(Tm2Database.Connection, quantity, density, total.UnitId, units).Numeric
			Catch ex As UnitConversionException
				total = Nothing
			End Try
		End If
	End Sub

	Private _compartmentContents As New Dictionary(Of Integer, Dictionary(Of Guid, KaTicketItem))
	Private _productDisplayedNotes As New Dictionary(Of Guid, List(Of String)) ' Dictionary(Of ProductId/GroupingId, List(Of Notes))
	Private _productOtherUnits As New Dictionary(Of Guid, List(Of Guid)) ' Dictionary(Of ProductId/GroupingId, List(Of UnitIds))
	Private _productGroupingIds As New Dictionary(Of Guid, Guid) ' Dictionary(Of TicketItemId, ProductId/GroupingId)

	Private Sub CalculateTicketTotals(ticket As KaTicket, units As Dictionary(Of Guid, KaUnit))
		Dim connection As OleDb.OleDbConnection = Tm2Database.Connection
		Dim massUnitId As Guid = KaUnit.GetSystemDefaultMassUnitOfMeasure(connection, Nothing)
		Dim volumeUnitId As Guid = KaUnit.GetSystemDefaultVolumeUnitOfMeasure(connection, Nothing)
		_totalMassRequested = New KaQuantity(0.0, massUnitId)
		_totalVolumeRequested = New KaQuantity(0.0, volumeUnitId)
		_totalMassDelivered = New KaQuantity(0.0, massUnitId)
		_totalVolumeDelivered = New KaQuantity(0.0, volumeUnitId)
		_compartmentContents = New Dictionary(Of Integer, Dictionary(Of Guid, KaTicketItem))
		_productDisplayedNotes = New Dictionary(Of Guid, List(Of String)) ' Dictionary(Of ProductId/GroupingId, List(Of Notes))
		_productOtherUnits = New Dictionary(Of Guid, List(Of Guid))
		_productGroupingIds = New Dictionary(Of Guid, Guid)

		For Each ticketItem As KaTicketItem In ticket.TicketItems
			AddTicketItemToCompartmentContents(ticket, ticketItem)
		Next

		For Each ticketFunction In ticket.TicketFunctions
			If _showRinseEntries AndAlso ticketFunction.ProductNumber = 91 Then
				Dim ticketItem As KaTicketItem = ticketFunction.ConvertToTicketItemAndTicketBulkItem(connection, Nothing)
				If ticketItem.Id = Guid.Empty Then
					ticketItem.Id = Guid.NewGuid
					For Each bulkItem As KaTicketBulkItem In ticketItem.TicketBulkItems
						bulkItem.TicketItemId = ticketItem.Id
					Next
				End If

				'ToDo: move to method
				AddTicketItemToCompartmentContents(ticket, ticketItem)
			End If
		Next

		For Each compartmentNumber As Integer In _compartmentContents.Keys
			Dim productCounter As Integer = 0
			For Each productGroupingId As Guid In _compartmentContents(compartmentNumber).Keys
				Dim lastItemInCompartment As Boolean = (productCounter = _compartmentContents(compartmentNumber).Keys.Count - 1)
				Dim firstItemInCompartment As Boolean = (productCounter = 0)
				Dim item As KaTicketItem = _compartmentContents(compartmentNumber)(productGroupingId)
				Dim product As KaProduct ' get the product so that we can determine if it is part of a group
				Try ' to look up the product in the dictionary...
					product = _products(item.ProductId)
				Catch ex As KeyNotFoundException ' the product isn't in the dictionary...
					Try ' to load the product from the database...
						product = New KaProduct(connection, item.ProductId)
					Catch ex2 As RecordNotFoundException ' the product isn't in the database...
						product = New KaProduct()
					End Try
					_products(item.ProductId) = product
				End Try
				For Each bulkItem As KaTicketBulkItem In item.TicketBulkItems
					Dim bulkProduct As KaBulkProduct ' get the bulk product so that we have access to it's density (if specified)
					Try ' to look up the bulk product in the dictionary...
						bulkProduct = _bulkProducts(bulkItem.BulkProductId)
					Catch ex As KeyNotFoundException ' the bulk product isn't in the dictionary...
						Try ' to load the bulk product from the dictionary...
							bulkProduct = New KaBulkProduct(connection, bulkItem.BulkProductId)
						Catch ex2 As Exception ' the bulk product isn't in the database...
							bulkProduct = New KaBulkProduct()
						End Try
						_bulkProducts(bulkItem.BulkProductId) = bulkProduct
					End Try
					' update the totals for this bulk product
					Dim deliveredDensity As KaRatio = New KaRatio(bulkItem.Density, bulkItem.WeightUnitId, bulkItem.VolumeUnitId)
					If deliveredDensity.Numeric <= 0 OrElse deliveredDensity.NumeratorUnitId.Equals(Guid.Empty) OrElse deliveredDensity.DenominatorUnitId.Equals(Guid.Empty) Then deliveredDensity = New KaRatio(bulkProduct.Density, bulkProduct.WeightUnitId, bulkProduct.VolumeUnitId)
					Dim requestedDensity As KaRatio = New KaRatio(bulkItem.RequestedDensity, bulkItem.RequestedWeightUnitId, bulkItem.RequestedVolumeUnitId)
					If requestedDensity.Numeric <= 0 OrElse requestedDensity.NumeratorUnitId.Equals(Guid.Empty) OrElse requestedDensity.DenominatorUnitId.Equals(Guid.Empty) Then requestedDensity = deliveredDensity
					Dim requested As New KaQuantity(bulkItem.Requested, bulkItem.UnitId)
					Dim delivered As New KaQuantity(bulkItem.Delivered, bulkItem.UnitId)
					UpdateTotal(_totalMassRequested, requested, requestedDensity, units)
					UpdateTotal(_totalVolumeRequested, requested, requestedDensity, units)
					UpdateTotal(_totalMassDelivered, delivered, deliveredDensity, units)
					UpdateTotal(_totalVolumeDelivered, delivered, deliveredDensity, units)
					'UpdateTotal(bulkItem.BulkProductId, _totalMassRequestedPerBulkProduct, requested, density, massUnitId, units)
					'UpdateTotal(bulkItem.BulkProductId, _totalVolumeRequestedPerBulkProduct, requested, density, volumeUnitId, units)
					'UpdateTotal(bulkItem.BulkProductId, _totalMassDeliveredPerBulkProduct, delivered, density, massUnitId, units)
					'UpdateTotal(bulkItem.BulkProductId, _totalVolumeDeliveredPerBulkProduct, delivered, density, volumeUnitId, units)
					'UpdateTotal(item.ProductId, _totalMassRequestedPerProduct, requested, density, massUnitId, units)
					'UpdateTotal(item.ProductId, _totalVolumeRequestedPerProduct, requested, density, volumeUnitId, units)
					'UpdateTotal(item.ProductId, _totalMassDeliveredPerProduct, delivered, density, massUnitId, units)
					'UpdateTotal(item.ProductId, _totalVolumeDeliveredPerProduct, delivered, density, volumeUnitId, units)
					UpdateTotal(item.Compartment, _totalMassRequestedPerCompartment, requested, requestedDensity, massUnitId, units)
					UpdateTotal(item.Compartment, _totalVolumeRequestedPerCompartment, requested, requestedDensity, volumeUnitId, units)
					UpdateTotal(item.Compartment, _totalMassDeliveredPerCompartment, delivered, deliveredDensity, massUnitId, units)
					UpdateTotal(item.Compartment, _totalVolumeDeliveredPerCompartment, delivered, deliveredDensity, volumeUnitId, units)
					UpdateTotal(item.Id, _totalMassRequestedPerItem, requested, requestedDensity, massUnitId, units)
					UpdateTotal(item.Id, _totalVolumeRequestedPerItem, requested, requestedDensity, volumeUnitId, units)
					UpdateTotal(item.Id, _totalMassDeliveredPerItem, delivered, deliveredDensity, massUnitId, units)
					UpdateTotal(item.Id, _totalVolumeDeliveredPerItem, delivered, deliveredDensity, volumeUnitId, units)
					If product.ProductGroupId <> Guid.Empty Then ' this product is a member of a product group
						If Not _productGroups.Contains(product.ProductGroupId) Then _productGroups.Add(product.ProductGroupId)
						Dim compartmentProductGroups As List(Of Guid)
						Try ' to lookup the compartment product group list...
							compartmentProductGroups = _productGroupsPerCompartment(item.Compartment)
						Catch ex As KeyNotFoundException ' the list hasn't be started for this compartment...
							compartmentProductGroups = New List(Of Guid) ' create a new list
							_productGroupsPerCompartment(item.Compartment) = compartmentProductGroups
						End Try
						If Not compartmentProductGroups.Contains(product.ProductGroupId) Then compartmentProductGroups.Add(product.ProductGroupId)
						UpdateTotal(product.ProductGroupId, _totalMassRequestedPerProductGroup, requested, requestedDensity, massUnitId, units)
						UpdateTotal(product.ProductGroupId, _totalVolumeRequestedPerProductGroup, requested, requestedDensity, volumeUnitId, units)
						UpdateTotal(product.ProductGroupId, _totalMassDeliveredPerProductGroup, delivered, deliveredDensity, massUnitId, units)
						UpdateTotal(product.ProductGroupId, _totalVolumeDeliveredPerProductGroup, delivered, deliveredDensity, volumeUnitId, units)
						UpdateTotal(product.ProductGroupId.ToString() & item.Compartment.ToString(), _totalMassRequestedPerProductGroupPerCompartment, requested, requestedDensity, massUnitId, units)
						UpdateTotal(product.ProductGroupId.ToString() & item.Compartment.ToString(), _totalVolumeRequestedPerProductGroupPerCompartment, requested, requestedDensity, volumeUnitId, units)
						UpdateTotal(product.ProductGroupId.ToString() & item.Compartment.ToString(), _totalMassDeliveredPerProductGroupPerCompartment, delivered, deliveredDensity, massUnitId, units)
						UpdateTotal(product.ProductGroupId.ToString() & item.Compartment.ToString(), _totalVolumeDeliveredPerProductGroupPerCompartment, delivered, deliveredDensity, volumeUnitId, units)
					End If
				Next
			Next
		Next
	End Sub
	''' <summary>
	''' Adds the ticket item to the appropriate global variables 
	''' </summary>
	Private Sub AddTicketItemToCompartmentContents(ticket As KaTicket, ByRef ticketItem As KaTicketItem)
		Dim connection As OleDb.OleDbConnection = Tm2Database.Connection

		If Not _compartmentContents.ContainsKey(ticketItem.Compartment) Then _compartmentContents.Add(ticketItem.Compartment, New Dictionary(Of Guid, KaTicketItem))
		With _compartmentContents(ticketItem.Compartment)

			Dim productGroupingId As Guid = Guid.Empty
			If _displayBlendGroupNameAsProductName Then productGroupingId = ticketItem.OrderItemBlendGroupId(connection, Nothing, True)
			If productGroupingId.Equals(Guid.Empty) Then
				productGroupingId = ticketItem.Id
			Else
				ticketItem.Name = ticketItem.OrderItemBlendGroupName(connection, Nothing, productGroupingId)
				_productGroupingIds.Add(ticketItem.Id, productGroupingId)
			End If

			If Not .ContainsKey(productGroupingId) Then
				.Add(productGroupingId, ticketItem.Clone)
			Else ' Add notes, amounts, bulk items, etc. to the existing ticket item
				With _compartmentContents(ticketItem.Compartment)(productGroupingId)
					.Requested += KaUnit.Convert(connection, New KaQuantity(ticketItem.Requested, ticketItem.UnitId), ticketItem.GetRequestedDensity(connection, Nothing, _showRinseEntries), .UnitId).Numeric
					.Delivered += KaUnit.Convert(connection, New KaQuantity(ticketItem.Delivered, ticketItem.UnitId), ticketItem.GetDensity(connection, Nothing, _showRinseEntries), .UnitId).Numeric
					.HazardousMaterial = .HazardousMaterial OrElse ticketItem.HazardousMaterial
					.TicketBulkItems.AddRange(ticketItem.TicketBulkItems)
				End With
			End If
			If Not _productDisplayedNotes.ContainsKey(productGroupingId) Then _productDisplayedNotes.Add(productGroupingId, New List(Of String))
			Dim ticketItemNote As String = ticketItem.Notes.Trim
			If ticketItemNote.Length > 0 AndAlso Not _productDisplayedNotes(productGroupingId).Contains(ticketItemNote) Then _productDisplayedNotes(productGroupingId).Add(ticketItemNote)

			If Not _productOtherUnits.ContainsKey(productGroupingId) Then _productOtherUnits.Add(productGroupingId, New List(Of Guid))
			If Not _productOtherUnits(productGroupingId).Contains(ticketItem.UnitId) Then _productOtherUnits(productGroupingId).Add(ticketItem.UnitId)
			Try
				Dim product As KaProduct = GetProduct(ticketItem.ProductId)
				If product.ProductGroupId <> Guid.Empty Then
					For Each unitId As Guid In GetUnitListForProductGroup(ticket.OwnerId, product.ProductGroupId)
						If Not _productOtherUnits(productGroupingId).Contains(unitId) Then _productOtherUnits(productGroupingId).Add(unitId)
					Next
				End If
			Catch ex As RecordNotFoundException
			End Try
		End With
	End Sub

	Private Function GetUnitListForProductGroup(ownerId As Guid, productGroupId As Guid) As List(Of Guid)
		Dim list As New List(Of Guid)
		Dim conditions As String = String.Format("deleted=0 AND name={0}", Q("WebTicketSetting:" & ownerId.ToString() & ":" & productGroupId.ToString() & "/AdditionalUnitsForProductGroup"))
		Dim settings As ArrayList = KaSetting.GetAll(Tm2Database.Connection, conditions, "")
		If settings.Count = 0 Then
			conditions = String.Format("deleted=0 AND name={0}", Q("WebTicketSetting:" & Guid.Empty.ToString() & ":" & productGroupId.ToString() & "/AdditionalUnitsForProductGroup"))
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


	Private Function GetProductGroupsPerUnit(ownerId As Guid, productGroups As List(Of Guid)) As Dictionary(Of Guid, List(Of Guid))
		Dim productGroupsPerUnit As New Dictionary(Of Guid, List(Of Guid))
		For Each unitId As Guid In _showAdditionalUnitList
			productGroupsPerUnit(unitId) = New List(Of Guid)({Guid.Empty})
		Next
		If productGroups IsNot Nothing Then
			For Each productGroupId In productGroups
				For Each unitId As Guid In GetUnitListForProductGroup(ownerId, productGroupId)
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

	Private Function FormatCompartmentTotalQuantity(compartment As Integer, ownerId As Guid, units As Dictionary(Of Guid, KaUnit), requested As Boolean) As String
		Dim value As String = ""
		Dim density As New KaRatio(0.0, KaUnit.GetUnitIdForBaseUnit(Tm2Database.Connection, KaUnit.Unit.Pounds), KaUnit.GetUnitIdForBaseUnit(Tm2Database.Connection, KaUnit.Unit.Gallons))
		Dim productGroups As List(Of Guid)
		Try ' to lookup the list of product groups referenced in this compartment
			productGroups = _productGroupsPerCompartment(compartment)
		Catch ex As KeyNotFoundException ' there isn't a list of product groups referenced in this compartment
			productGroups = Nothing
		End Try
		Dim productGroupsPerUnits As Dictionary(Of Guid, List(Of Guid)) = GetProductGroupsPerUnit(ownerId, productGroups)
		For Each unitId In productGroupsPerUnits.Keys
			Dim total As New KaQuantity(0.0, unitId)
			Dim unit As KaUnit = KaUnit.GetUnit(unitId, units)
			Dim quantity As KaQuantity
			If productGroupsPerUnits(unitId).Contains(Guid.Empty) Then ' try to display this unit for all product groups
				If KaUnit.IsWeight(unit.BaseUnit) Then
					If requested Then
						quantity = _totalMassRequestedPerCompartment(compartment)
					Else ' delivered
						quantity = _totalMassDeliveredPerCompartment(compartment)
					End If
				Else
					If requested Then
						quantity = _totalVolumeRequestedPerCompartment(compartment)
					Else ' delivered
						quantity = _totalVolumeDeliveredPerCompartment(compartment)
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
				productGroupsForUnit = productGroupsPerUnits(unitId)
				Dim productGroupIdsToRemove As List(Of Guid) = New List(Of Guid)

				For Each productGroupId As Guid In productGroupsForUnit
					If productGroupId <> Guid.Empty Then ' we've already established that we can't display the complete total in this unit of measure
						Try
							If KaUnit.IsWeight(unit.BaseUnit) Then
								If requested Then
									quantity = _totalMassRequestedPerProductGroupPerCompartment(productGroupId.ToString() & compartment.ToString())
								Else ' delivered
									quantity = _totalMassDeliveredPerProductGroupPerCompartment(productGroupId.ToString() & compartment.ToString())
								End If
							Else
								If requested Then
									quantity = _totalVolumeRequestedPerProductGroupPerCompartment(productGroupId.ToString() & compartment.ToString())
								Else ' delivered
									quantity = _totalVolumeDeliveredPerProductGroupPerCompartment(productGroupId.ToString() & compartment.ToString())
								End If
							End If
							If quantity IsNot Nothing Then
								total.Numeric += KaUnit.FastConvert(Tm2Database.Connection, quantity, density, total.UnitId, units).Numeric
							Else ' this product group can't be displayed in this unit of measure, remove it from the list so that we don't show it's label later
								productGroupIdsToRemove.Add(productGroupId)
							End If
						Catch ex As KeyNotFoundException
							productGroupIdsToRemove.Add(productGroupId)
						End Try
					End If
				Next

				For Each productGroupId As Guid In productGroupIdsToRemove
					If productGroupsForUnit.Contains(productGroupId) Then
						productGroupsForUnit.Remove(productGroupId)
					End If
				Next
			End If
			If productGroupsForUnit.Count > 0 Then
				value &= IIf(value.Length > 0, ", ", "") & String.Format("{0:" & unit.UnitPrecision & "} {1}", total.Numeric, unit.Abbreviation)
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
		Return value
	End Function

	Private Enum FormatValues
		Delivered = 0
		Requested = 1
		ApplicationRate = 2
	End Enum

	Private Function FormatTotalQuantity(ByVal ticket As KaTicket, ByVal units As Dictionary(Of Guid, KaUnit), ByVal what As FormatValues, ByVal order As KaOrder) As String
		Dim value As String = ""
		If what <> FormatValues.ApplicationRate OrElse ticket.Acres > 0.0 Then
			Dim density As New KaRatio(0.0, KaUnit.GetUnitIdForBaseUnit(Tm2Database.Connection, KaUnit.Unit.Pounds), KaUnit.GetUnitIdForBaseUnit(Tm2Database.Connection, KaUnit.Unit.Gallons))
			Dim productGroupsPerUnits As Dictionary(Of Guid, List(Of Guid)) = GetProductGroupsPerUnit(ticket.OwnerId, _productGroups)
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

	Private Function FormatBulkItemQuantity(bulkItem As KaTicketBulkItem, units As Dictionary(Of Guid, KaUnit), requested As Boolean) As String
		Dim unit As KaUnit = KaUnit.GetUnit(bulkItem.UnitId, units)
		Dim quantity As New KaQuantity(IIf(requested, bulkItem.Requested, bulkItem.Delivered), bulkItem.UnitId)
		Dim value As String = String.Format("{0:" & bulkItem.UnitPrecision & "} {1}", quantity.Numeric, unit.Abbreviation)
		Dim density As New KaRatio(bulkItem.Density, bulkItem.WeightUnitId, bulkItem.VolumeUnitId)
		Return value
	End Function

	Private Function GetProduct(productId As Guid) As KaProduct
		Dim product As KaProduct
		Try
			product = _products(productId)
		Catch ex As Exception
			product = New KaProduct(Tm2Database.Connection, productId)
			_products(productId) = product
		End Try
		Return product
	End Function

	Private Function FormatItemQuantity(item As KaTicketItem, ownerId As Guid, units As Dictionary(Of Guid, KaUnit), requested As Boolean) As String
		Dim unitPrecisions As New Dictionary(Of Guid, String) ' Guid = unit ID, String = most restrictive precision
		For Each bulkItem As KaTicketBulkItem In item.TicketBulkItems
			Dim whole As UInteger = 0, entryWhole As UInteger = 0, fractional As UInteger = UInteger.MaxValue, entryFractional As UInteger = UInteger.MaxValue
			KaUnit.GetPrecisionDigits(bulkItem.UnitPrecision, entryWhole, entryFractional)
			Try
				KaUnit.GetPrecisionDigits(unitPrecisions(bulkItem.UnitId), whole, fractional)
			Catch ex As KeyNotFoundException
			End Try
			If entryWhole > whole Then whole = entryWhole
			If entryFractional < fractional Then fractional = entryFractional
			unitPrecisions(bulkItem.UnitId) = KaUnit.GetPrecisionString(whole, fractional, ",", "0")
		Next
		Dim density As New KaRatio(0.0, KaUnit.GetSystemDefaultMassUnitOfMeasure(Tm2Database.Connection, Nothing), KaUnit.GetSystemDefaultVolumeUnitOfMeasure(Tm2Database.Connection, Nothing))
		Dim unit As KaUnit = KaUnit.GetUnit(item.UnitId, units)
		Dim quantity As KaQuantity
		If KaUnit.IsWeight(unit.BaseUnit) Then
			If requested Then
				quantity = _totalMassRequestedPerItem(item.Id)
			Else ' delivered
				quantity = _totalMassDeliveredPerItem(item.Id)
			End If
		Else
			If requested Then
				quantity = _totalVolumeRequestedPerItem(item.Id)
			Else ' delivered
				quantity = _totalVolumeDeliveredPerItem(item.Id)
			End If
		End If
		Dim value As String
		Dim precision As String
		Try
			precision = unitPrecisions(unit.Id)
		Catch ex As KeyNotFoundException
			precision = unit.UnitPrecision
		End Try
		If quantity Is Nothing Then
			value = String.Format("#Err {0}", unit.Abbreviation)
		Else
			value = String.Format("{0:" & precision & "} {1}", KaUnit.FastConvert(Tm2Database.Connection, quantity, density, unit.Id, units).Numeric, unit.Abbreviation)
		End If
		Dim otherUnits As New List(Of Guid)
		For Each unitId As Guid In _showAdditionalUnitList
			If unitId <> item.UnitId AndAlso Not otherUnits.Contains(unitId) Then otherUnits.Add(unitId)
		Next
		Try
			Dim primaryKey As Guid = item.Id
			If _productGroupingIds.ContainsKey(item.Id) Then primaryKey = _productGroupingIds(item.Id)
			For Each unitId As Guid In _productOtherUnits(primaryKey)
				If unitId <> item.UnitId AndAlso Not otherUnits.Contains(unitId) Then otherUnits.Add(unitId)
			Next
		Catch ex As RecordNotFoundException ' can't get product information
		End Try
		Dim otherValues As String = ""
		For Each unitId As Guid In otherUnits
			If unitId <> item.UnitId Then
				Try
					unit = KaUnit.GetUnit(unitId, units)
					If KaUnit.IsWeight(unit.BaseUnit) Then
						If requested Then
							quantity = _totalMassRequestedPerItem(item.Id)
						Else ' delivered
							quantity = _totalMassDeliveredPerItem(item.Id)
						End If
					Else
						If requested Then
							quantity = _totalVolumeRequestedPerItem(item.Id)
						Else ' delivered
							quantity = _totalVolumeDeliveredPerItem(item.Id)
						End If
					End If
					Try
						precision = unitPrecisions(unit.Id)
					Catch ex As KeyNotFoundException
						precision = unit.UnitPrecision
					End Try
					If quantity Is Nothing Then
						' do not include this unit
					Else
						otherValues &= IIf(otherValues.Length > 0, ", ", "") & String.Format("{0:" & precision & "} {1}", KaUnit.FastConvert(Tm2Database.Connection, quantity, density, unitId, units).Numeric, unit.Abbreviation)
					End If
				Catch ex As UnitConversionException ' do not include this unit
				End Try
			End If
		Next
		If otherValues.Length > 0 Then value &= " (" & otherValues & ")"
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

	Private Sub AddUnitsToList(list As List(Of Guid), units As Dictionary(Of Guid, KaUnit))
		For Each unitId As Guid In list ' make sure these units are in the dictionary
			Try ' to lookup the unit in the dictionary...
				Dim unit As KaUnit = units(unitId)
			Catch ex As KeyNotFoundException ' the unit wasn't in the dictionary...
				Try ' to load the unit from the database...
					units(unitId) = New KaUnit(Tm2Database.Connection, unitId)
				Catch ex2 As RecordNotFoundException ' the unit wasn't in the database...
					units(unitId) = New KaUnit() ' we don't know the abbreviation for this unit
				End Try
			End Try
		Next
	End Sub

	Private Function GetCompartmentRowCount(ticket As KaTicket, compartment As Integer) As Integer
		Dim count As Integer = 0
		If _showCompartmentBulkIngredients Then
			For Each productGroupingId As Guid In _compartmentContents(compartment).Keys
				count += _compartmentContents(compartment)(productGroupingId).TicketBulkItems.Count
			Next
		Else
			count = _compartmentContents(compartment).Count
		End If
		Return count
	End Function

	Private Function GetCompartmentContents(ByVal ticket As KaTicket, ByVal orderInfo As KaOrder, units As Dictionary(Of Guid, KaUnit)) As String
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
		Dim compartmentProductContents As String = "<table ID=""CompartmentDetails"" cellspacing=""0"" style=""width: 100%;"">"
		' add the header row
		compartmentProductContents &= "<tr>"
		compartmentProductContents &= "<td style=""border-bottom: 1px solid black;"" class=""CompartmentHeader""><strong>Compartment</strong></td>"
		If _showProductHazardousMaterial Then compartmentProductContents &= "<td style=""border-bottom: 1px solid black;"" class=""CompartmentHeader""><strong>&nbsp;HM&nbsp;</strong></td>"
		compartmentProductContents &= "<td style=""border-bottom: 1px solid black;"" class=""CompartmentHeader""><strong>Item</strong></td>"
		If _showRequestedQty Then compartmentProductContents &= "<td style=""border-bottom: 1px solid black;"" class=""CompartmentHeader""><strong>Requested</strong></td>"
		compartmentProductContents &= "<td style=""border-bottom: 1px solid black;"" class=""CompartmentHeader""><strong>" & IIf(linkedTicket, "Net allocated", "Net delivered") & "</strong></td>"
		If _showCompartmentBulkIngredients Then compartmentProductContents &= "<td style=""border-bottom: 1px solid black;"" class=""CompartmentHeader""><strong>Sub item</strong></td>"
		If _showCompartmentBulkIngredients AndAlso _showCompartmentBulkIngredientLotNumber Then compartmentProductContents &= "<td style=""border-bottom: 1px solid black;"" class=""CompartmentHeader""><strong>Lot</strong></td>"
		If _showCompartmentBulkIngredients And _showRequestedQty Then compartmentProductContents &= "<td style=""border-bottom: 1px solid black;"" class=""CompartmentHeader""><strong>Requested for sub item</strong></td>"
		If _showCompartmentBulkIngredients Then compartmentProductContents &= "<td style=""border-bottom: 1px solid black;"" class=""CompartmentHeader""><strong>" & IIf(linkedTicket, "Net allocated", "Net delivered") & " for sub item</strong></td>"
		If _showDensityOnTicket Then compartmentProductContents &= "<td style=""border-bottom: 1px solid black;"" class=""CompartmentHeader""><strong>Density</strong></td>"
		If _showApplicationRateOnTicket Then compartmentProductContents &= "<td style=""border-bottom: 1px solid black;"" class=""CompartmentHeader""><strong>Application rate</strong></td>"
		If _showCompartmentTotals Then compartmentProductContents &= "<td style=""border-bottom: 1px solid black;"" class=""CompartmentHeader""><strong>Total</strong></td>"
		If _showCompartmentFertilizerGrade And orderInfo.DoNotBlend Then compartmentProductContents &= "<td style=""border-bottom: 1px solid black;"" class=""CompartmentHeader""><strong>Grade</strong></td>"
		compartmentProductContents &= "</tr>"

		' add the rows
		For Each compartmentNumber As Integer In _compartmentContents.Keys
			Dim firstItemInCompartment As Boolean = True
			Dim lastItemId As Guid = Guid.Empty
			For Each productGroupingId As Guid In _compartmentContents(compartmentNumber).Keys
				Dim item As KaTicketItem = _compartmentContents(compartmentNumber)(productGroupingId)

				Dim product As KaProduct
				Try
					product = New KaProduct(connection, item.ProductId)
				Catch ex As RecordNotFoundException
					product = New KaProduct()
					With product
						.Name = item.Name

					End With
				End Try

				Dim firstItemRow As Boolean = Not lastItemId.Equals(item.ProductId)

				Dim attributes As String
				Dim value As String = ""
				' add the compartment detail cell
				If firstItemInCompartment Then
					Dim rowspan As Integer = GetCompartmentRowCount(ticket, item.Compartment)
					' setup the attributes for this table cell
					attributes = " class=""CompartmentDetail"" style=""vertical-align: top; border-bottom: 1px solid black;""" & IIf(rowspan > 1, String.Format(" rowspan=""{0:0}""", rowspan), "")
					Dim ticketTransport As KaTicketTransport = Nothing ' get a reference to the transport
					For Each r As KaTicketTransport In ticket.TicketTransports
						If r.Id = item.TicketTransportId Then
							ticketTransport = r
							Exit For ' no need to look any further
						End If
					Next
					Dim ticketCompartment As KaTicketTransportCompartment = Nothing ' get a reference to the transport compartment
					For Each r As KaTicketTransportCompartment In ticket.TicketTransportCompartments
						If r.Id = item.TicketTransportCompartmentId OrElse (item.TicketTransportId = Guid.Empty AndAlso r.Position = item.Compartment) Then
							ticketCompartment = r
							Exit For ' no need to look any further
						End If
					Next
					If _showCompartmentLoadedIndex Then
						value = String.Format("<span class=""CompartmentIndex"" id=""CompartmentIndex{0:0}"">{0:0} {1}</span>", item.Compartment + 1, IIf((ticketCompartment IsNot Nothing AndAlso ticketCompartment.PositionOnTransport <> -1) OrElse (ticketCompartment IsNot Nothing AndAlso ticketCompartment.Label.Trim().Length > 0), ": ", " "))
					End If
					If ticketCompartment IsNot Nothing AndAlso ticketCompartment.PositionOnTransport <> -1 Then value &= Utilities.GetOrdinalNumber(ticketCompartment.PositionOnTransport + 1) & " compartment"
					If ticketTransport IsNot Nothing AndAlso ticketCompartment IsNot Nothing AndAlso ticketCompartment.PositionOnTransport <> -1 Then value &= " on "
					If ticketTransport IsNot Nothing Then value &= ticketTransport.Name

					If ticketCompartment IsNot Nothing AndAlso ticketCompartment.Label.Trim().Length > 0 Then value &= " (" & ticketCompartment.Label & ")"
					If value.Length = 0 Then value = "&nbsp;"
					compartmentProductContents &= String.Format("<td{0}>{1}</td>", attributes, value)
				End If
				attributes = " class=""CompartmentDetail"" style=""vertical-align: top; border-bottom: 1px solid black;""" & IIf(_showCompartmentBulkIngredients AndAlso item.TicketBulkItems.Count > 1, String.Format(" rowspan=""{0:0}""", item.TicketBulkItems.Count), "")
				If _showProductHazardousMaterial Then ' show the hazardous material column
					compartmentProductContents &= String.Format("<td{0}>{1}</td>", attributes, IIf(item.HazardousMaterial, "X", "&nbsp;"))
				End If
				value = item.Name

				If _showCompartmentProductNotes Then
					Dim ticketNotes As String = ""
					For Each ticketNote As String In _productDisplayedNotes(productGroupingId)
						If ticketNotes.Length > 0 Then ticketNotes &= vbCrLf
						ticketNotes &= ticketNote
					Next
					If ticketNotes.Length > 0 Then value &= String.Format("<br /><small>{0}</small>", EncodeAsHtml(ticketNotes))
				End If
				compartmentProductContents &= String.Format("<td{0}>{1}</td>", attributes, value)
				If _showRequestedQty Then compartmentProductContents &= String.Format("<td{0}>{1}</td>", attributes, FormatItemQuantity(item, ticket.OwnerId, units, True))
				value = FormatItemQuantity(item, ticket.OwnerId, units, False)
				If Not _showCompartmentBulkIngredients AndAlso _ntepCompliant Then
					Dim hasEmulated As Boolean = False ' does this item contain an emulated bulk item?
					Dim hasHandadd As Boolean = False ' does this item contain a hand-add?
					Dim hasMotion As Boolean = False ' does this item contain an bulk item that was in motion when the final measurement was taken
					Dim hasUnverifiedMeasurement As Boolean = False ' does this item contain an bulk item measurement that wasn't verified
					For Each n As KaTicketBulkItem In item.TicketBulkItems
						If n.ProductNumber = 99 Then hasHandadd = True ' this item contains a hand-add
						If n.Emulated Then hasEmulated = True ' this item was emulated
						If n.Motion Then hasMotion = True ' this item was in motion
						If n.UnverifiedMeasurement Then hasUnverifiedMeasurement = True ' this item has an unverified measurement
					Next
					Dim ntepNotes As String = ""
					If hasEmulated Then ntepNotes &= "includes emulated ingredients"
					If hasHandadd Then ntepNotes &= IIf(ntepNotes.Length > 0, ", ", "") & "includes hand-added ingredients"
					If hasMotion Then ntepNotes &= IIf(ntepNotes.Length > 0, ", ", "") & "motion"
					If hasUnverifiedMeasurement Then ntepNotes &= IIf(ntepNotes.Length > 0, ", ", "") & "unverified measurement"
					If ntepNotes.Length > 0 Then value &= String.Format(" ({0})", ntepNotes)
				End If
				compartmentProductContents &= String.Format("<td{0}>{1}</td>", attributes, value)

				For j As Integer = 0 To item.TicketBulkItems.Count - 1
					Dim bulkItem As KaTicketBulkItem = item.TicketBulkItems(j)
					If _showCompartmentBulkIngredients Then
						attributes = " class=""CompartmentDetail"" style=""vertical-align: top; border-bottom: 1px solid black;"""
						Dim bulkProductName As String = bulkItem.Name
						If _showCompartmentBulkIngredientEpaNumbers AndAlso bulkItem.BulkProduct IsNot Nothing AndAlso bulkItem.BulkProduct.EpaNumber.Length > 0 Then bulkProductName &= String.Format("<br /><small>{0}</small>", EncodeAsHtml("EPA: " & bulkItem.BulkProduct.EpaNumber))
						If _showCompartmentBulkIngredientNotes AndAlso bulkItem.BulkProductNotes.Length > 0 Then bulkProductName &= String.Format("<br /><small>{0}</small>", EncodeAsHtml(bulkItem.BulkProductNotes))
						compartmentProductContents &= String.Format("<td{0}>{1}</td>", attributes, bulkProductName)
						If _showCompartmentBulkIngredientLotNumber Then compartmentProductContents &= String.Format("<td{0}>{1}</td>", attributes, bulkItem.LotNumber)
						If _showRequestedQty Then compartmentProductContents &= String.Format("<td{0}>{1}</td>", attributes, FormatBulkItemQuantity(bulkItem, units, True))
						Dim ntepNotes As String = ""
						If _ntepCompliant Then
							If bulkItem.Emulated Then ntepNotes &= "emulated"
							If bulkItem.ProductNumber = 99 Then ntepNotes &= IIf(ntepNotes.Length > 0, ", ", "") & "hand-add"
							If bulkItem.Motion Then ntepNotes &= IIf(ntepNotes.Length > 0, ", ", "") & "motion"
							If bulkItem.UnverifiedMeasurement Then ntepNotes &= IIf(ntepNotes.Length > 0, ", ", "") & "unverified measurement"
							If ntepNotes.Length > 0 Then ntepNotes = String.Format(" ({0})", ntepNotes)
						End If
						compartmentProductContents &= String.Format("<td{0}>{1}</td>", attributes, FormatBulkItemQuantity(bulkItem, units, False) & ntepNotes)
						Dim density As New KaRatio(bulkItem.Density, bulkItem.WeightUnitId, bulkItem.VolumeUnitId)
						If _showDensityOnTicket Then ' show bulk product density
							density = KaReports.GetDensityDisplayBasedOnProductGroup(density, product.ProductGroupId, ticket.OwnerId, units, orderDensityUnits)
							If density.Numeric > 0.0 Then ' display the density
								Dim densityUnitPrecision As String
								Try ' to lookup the unit precision for these density units...
									densityUnitPrecision = _densityUnitPrecision(String.Format("{0}|{1}", density.NumeratorUnitId.ToString(), density.DenominatorUnitId.ToString()))
								Catch ex As KeyNotFoundException ' these density units are not in the dictionary...
									Dim massWhole As UInteger = 0, massFractional As UInteger = UInteger.MaxValue
									KaUnit.GetPrecisionDigits(KaUnit.GetUnit(connection, density.NumeratorUnitId, units).UnitPrecision, massWhole, massFractional)
									Dim volumeWhole As UInteger = 0, volumeFractional As UInteger = UInteger.MaxValue
									KaUnit.GetPrecisionDigits(KaUnit.GetUnit(connection, density.NumeratorUnitId, units).UnitPrecision, volumeWhole, volumeFractional)
									densityUnitPrecision = KaUnit.GetPrecisionString(Math.Max(massWhole, volumeWhole), Math.Min(massFractional, volumeFractional), ",", 0)
								End Try
								value = String.Format("{0:" & densityUnitPrecision & "} {1}/{2}", density.Numeric, KaUnit.GetUnit(connection, density.NumeratorUnitId, units).Abbreviation, KaUnit.GetUnit(connection, density.DenominatorUnitId, units).Abbreviation)
							Else ' density isn't available
								value = "n/a"
							End If
							compartmentProductContents &= String.Format("<td{0}>{1}</td>", attributes, value)
						End If
						If _showApplicationRateOnTicket Then ' show bulk product application rate
							Dim applicationRateAttributes As String = attributes
							Dim applicationRateString As String = ""
							If _useOriginalOrdersApplicationRate Then
								If firstItemRow Then
									Try
										Dim oi As New KaOrderItem(connection, item.OrderItemId)
										Dim order As New KaOrder(connection, oi.OrderId)
										applicationRateString = KaReports.GetApplicationRate(New KaQuantity(oi.Request, oi.UnitId), order.Acres, density, _showAdditionalUnitList, units)
									Catch ex As RecordNotFoundException
										applicationRateString = KaReports.GetApplicationRate(New KaQuantity(bulkItem.Delivered, bulkItem.UnitId), ticket.Acres, density, _showAdditionalUnitList, units)
									End Try
								End If
							Else
								applicationRateString = KaReports.GetApplicationRate(New KaQuantity(bulkItem.Delivered, bulkItem.UnitId), ticket.Acres, density, _showAdditionalUnitList, units)
							End If
							If _useOriginalOrdersApplicationRate AndAlso firstItemRow AndAlso _showCompartmentBulkIngredients Then _
								applicationRateAttributes &= " rowspan=""" & item.TicketBulkItems.Count & """"

							If Not _useOriginalOrdersApplicationRate OrElse firstItemRow Then _
								compartmentProductContents &= String.Format("<td{0}>{1}</td>", applicationRateAttributes, IIf(applicationRateString.Trim.Length > 0, applicationRateString, "&nbsp;"))
						End If
					ElseIf firstItemRow Then ' do not show bulk items
						Dim mass As KaQuantity = _totalMassDeliveredPerItem(item.Id)
						Dim volume As KaQuantity = _totalVolumeDeliveredPerItem(item.Id)
						Dim density As KaRatio
						If mass IsNot Nothing AndAlso volume IsNot Nothing AndAlso volume.Numeric > 0.0 Then ' display the density
							density = New KaRatio(mass.Numeric / volume.Numeric, mass.UnitId, volume.UnitId)
							density = KaReports.GetDensityDisplayBasedOnProductGroup(density, product.ProductGroupId, ticket.OwnerId, units, orderDensityUnits)
						Else
							density = Nothing
						End If
						If _showDensityOnTicket Then ' show bulk product density
							attributes = " class=""CompartmentDetail"" style=""vertical-align: top; border-bottom: 1px solid black;"""
							If density IsNot Nothing Then ' display the density
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
								value = String.Format("{0:" & densityUnitPrecision & "} {1}/{2}", density.Numeric, KaUnit.GetUnit(connection, density.NumeratorUnitId, units).Abbreviation, KaUnit.GetUnit(connection, density.DenominatorUnitId, units).Abbreviation)
							Else ' density isn't available
								value = "n/a"
							End If
							compartmentProductContents &= String.Format("<td{0}>{1}</td>", attributes, value)
						End If
						If _showApplicationRateOnTicket Then
							attributes = " class=""CompartmentDetail"" style=""vertical-align: top; border-bottom: 1px solid black;"""
							Dim otherUnits As New List(Of Guid)
							For Each unitId As Guid In _showAdditionalUnitList
								If Not otherUnits.Contains(unitId) Then otherUnits.Add(unitId)
							Next
							Dim primaryKey As Guid = item.Id
							If _productGroupingIds.ContainsKey(item.Id) Then primaryKey = _productGroupingIds(item.Id)
							For Each unitId As Guid In _productOtherUnits(primaryKey)
								If Not otherUnits.Contains(unitId) Then otherUnits.Add(unitId)
							Next
							Dim applicationRateString As String
							If _useOriginalOrdersApplicationRate Then
								Try
									Dim oi As New KaOrderItem(connection, item.OrderItemId)
									Dim order As New KaOrder(connection, oi.OrderId)
									applicationRateString = KaReports.GetApplicationRate(New KaQuantity(oi.Request, oi.UnitId), order.Acres, density, _showAdditionalUnitList, units)
								Catch ex As RecordNotFoundException
									applicationRateString = KaReports.GetApplicationRate(New KaQuantity(bulkItem.Delivered, bulkItem.UnitId), ticket.Acres, density, _showAdditionalUnitList, units)
								End Try
							Else
								applicationRateString = KaReports.GetApplicationRate(New KaQuantity(bulkItem.Delivered, bulkItem.UnitId), ticket.Acres, density, _showAdditionalUnitList, units)
							End If
							compartmentProductContents &= String.Format("<td{0}>{1}</td>", attributes, IIf(applicationRateString.Trim.Length > 0, applicationRateString, "&nbsp;"))
						End If
					End If
					If firstItemInCompartment AndAlso _showCompartmentTotals Then
						Dim rowspan As Integer = GetCompartmentRowCount(ticket, item.Compartment)
						attributes = " class=""CompartmentDetail"" style=""vertical-align: top; border-bottom: 1px solid black;""" & IIf(rowspan > 1, String.Format(" rowspan=""{0:0}""", rowspan), "")
						compartmentProductContents &= String.Format("<td{0}>{1}</td>", attributes, FormatCompartmentTotalQuantity(item.Compartment, ticket.OwnerId, units, False))
					End If

					If _showCompartmentFertilizerGrade And orderInfo.DoNotBlend And firstItemInCompartment Then
						Dim compartmenetFertilizerGrade As String

						If _compartmentFertilizerAnalysis.ContainsKey(compartmentNumber) Then
							compartmenetFertilizerGrade = GetFertilizerGrade(_compartmentFertilizerAnalysis(compartmentNumber), True)
						Else
							compartmenetFertilizerGrade = " "
						End If
						Dim rowspan As Integer = GetCompartmentRowCount(ticket, item.Compartment)
						attributes = " class=""CompartmentDetail"" style=""vertical-align: top; border-bottom: 1px solid black;""" & IIf(rowspan > 1, String.Format(" rowspan=""{0:0}""", rowspan), "")
						compartmentProductContents &= String.Format("<td{0}>{1}</td>", attributes, compartmenetFertilizerGrade)
					End If

					If _showCompartmentBulkIngredients OrElse j + 1 = item.TicketBulkItems.Count OrElse item.TicketBulkItems(j + 1).TicketItemId <> bulkItem.TicketItemId Then
						compartmentProductContents &= "</tr>"
					End If
					lastItemId = item.ProductId
					firstItemInCompartment = False
					firstItemRow = Not lastItemId.Equals(item.ProductId)
				Next
			Next
		Next

		' show totals
		If _showTotal Then
			compartmentProductContents &= "<tr><td class=""CompartmentTotal""><strong>Total</strong></td><td></td>"
			If _showProductHazardousMaterial Then compartmentProductContents &= "<td></td>"
			If _showRequestedQty Then compartmentProductContents &= "<td class""CompartmentTotal""><strong>" & FormatTotalQuantity(ticket, units, FormatValues.Requested, orderInfo) & " </strong></td>"
			compartmentProductContents &= "<td class=""CompartmentTotal""><strong>" & FormatTotalQuantity(ticket, units, FormatValues.Delivered, orderInfo) & "</strong></td>"
			If _showCompartmentBulkIngredients Then
				If _showRequestedQty Then compartmentProductContents &= "<td></td>"
				compartmentProductContents &= "<td></td><td></td>"
			End If
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
				compartmentProductContents &= "<td><strong>" & value & "</strong></td>"
			End If
			If _showApplicationRateOnTicket Then
				compartmentProductContents &= String.Format("<td><strong>{0}</strong></td>", FormatTotalQuantity(ticket, units, FormatValues.ApplicationRate, orderInfo))
			End If
			compartmentProductContents &= "</tr>"
		End If
		compartmentProductContents &= "</table>"
		Return compartmentProductContents
	End Function

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

	Private Sub GetBulkItemAnalysisTotals(ByVal ticket As KaTicket, ByRef totalMass As KaQuantity, ByRef totalMassValid As Boolean, ByRef totalVolume As KaQuantity, ByRef totalVolumeValid As Boolean, ByRef fertilizerAnalysis As Dictionary(Of String, AnalysisEntry), ByRef hazMatAnalysis As Dictionary(Of Guid, AnalysisEntry), ByRef totalRequestedMass As KaQuantity, ByRef totalRequestedVolume As KaQuantity, ByRef derivedFromList As List(Of String))
		Dim additionalUnitTotal As Double = 0
		Dim linkedTicket As Boolean = Not ticket.LinkedTicketsId.Equals(Guid.Empty)
		Dim lastCompartment As Integer = -1
		For Each ticketItem As KaTicketItem In ticket.TicketItems
			For Each ticketBulkItem As KaTicketBulkItem In ticketItem.TicketBulkItems
				'Get the derived from information
				Dim bulkProductInfo As KaBulkProduct = Nothing
				Try
					bulkProductInfo = ticketBulkItem.BulkProduct
				Catch ex As RecordNotFoundException
				End Try
				If bulkProductInfo Is Nothing Then
					Try
						bulkProductInfo = Tm2Database.GetBulkProduct(Tm2Database.Connection, Nothing, ticketBulkItem.BulkProductId, _bulkProducts)
					Catch ex As RecordNotFoundException
					End Try
				End If
				If bulkProductInfo IsNot Nothing Then
					For Each derivedFrom As String In bulkProductInfo.DerivedFrom
						If Not derivedFromList.Contains(derivedFrom, StringComparer.OrdinalIgnoreCase) Then derivedFromList.Add(derivedFrom)
					Next
				End If

				Dim unit As KaUnit = Tm2Database.GetUnit(Tm2Database.Connection, Nothing, ticketBulkItem.UnitId, _units)

				' update total
				Dim density As New KaRatio(ticketBulkItem.Density, ticketBulkItem.WeightUnitId, ticketBulkItem.VolumeUnitId)
				Dim delivered As New KaQuantity(ticketBulkItem.Delivered, ticketBulkItem.UnitId)
				Dim requested As New KaQuantity(ticketBulkItem.Requested, ticketBulkItem.UnitId)
				If totalMassValid Then
					Dim mass As Double
					Dim requestedMass As Double
					Try ' to convert the delivered quantity to the mass unit of measure
						mass = KaUnit.Convert(Tm2Database.Connection, delivered, density, totalMass.UnitId).Numeric
						totalMass.Numeric += mass
					Catch ex As UnitConversionException
						totalMassValid = False
					End Try
					Try ' to convert the requested quantity to the mass unit of measure
						requestedMass = KaUnit.Convert(Tm2Database.Connection, requested, density, totalMass.UnitId).Numeric
						totalRequestedMass.Numeric += requestedMass
					Catch ex As UnitConversionException
						totalMassValid = False
					End Try
					For Each tbia As KaTicketBulkItemAnalysis In ticketBulkItem.TicketBulkItemAnalysis
						Try
							Dim analysis As New KaAnalysis(Tm2Database.Connection, tbia.AnalysisId)
							If analysis.TemplateId = _hazMatTypeId Then
								For Each analysisEntry As AnalysisEntry In analysis.Entrys
									Try
										Dim u As New KaUnit
										If analysisEntry.UnitOfMeasure.Contains(Guid.Empty.ToString) Then
											u = New KaUnit() With {.Id = Guid.Empty}
										Else
											u = New KaUnit(Tm2Database.Connection, Guid.Parse(analysisEntry.UnitOfMeasure))
										End If
										If Not hazMatAnalysis.ContainsKey(analysis.RecordId) Then
											hazMatAnalysis.Add(analysis.RecordId,
																		New AnalysisEntry() With {
																			.Data = "0",
																			.Id = analysisEntry.Id,
																			.Label = analysisEntry.Label,
																			.DataTypeValue = analysisEntry.DataTypeValue,
																			.Maximum = analysisEntry.Maximum,
																			.MaximumExceededWarning = analysisEntry.MaximumExceededWarning,
																			.UnitOfMeasure = analysisEntry.UnitOfMeasure,
															   .Components = {New AnalysisEntry() With {.UnitOfMeasure = totalMass.UnitId.ToString}}.ToList}) 'Fall back unit of measure
										End If
										hazMatAnalysis(analysis.RecordId).Data = Double.Parse(hazMatAnalysis(analysis.RecordId).Data) + KaUnit.Convert(Tm2Database.Connection, New KaQuantity(mass, totalMass.UnitId), density, IIf(u.Id = Guid.Empty, totalMass.UnitId, u.Id)).Numeric
									Catch ex As FormatException
									Catch ex As RecordNotFoundException

									End Try
								Next
							Else
								Dim analysisTypePage As New KaCustomPages(Tm2Database.Connection, analysis.TypeId)
								If analysisTypePage.BulkProductAnalysis Then ' handle fertilizer analysis information
									For Each analysisEntry As AnalysisEntry In analysis.Entrys
										If analysisEntry.UnitOfMeasure = "%" Then
											If Not fertilizerAnalysis.ContainsKey(analysisEntry.Label) Then fertilizerAnalysis(analysisEntry.Label) = New AnalysisEntry() With {
												.Data = "0", .Id = analysisEntry.Id, .Label = analysisEntry.Label, .DataTypeValue = analysisEntry.DataTypeValue, .Maximum = Double.MaxValue, .MaximumExceededWarning = "", .Minimum = Double.MaxValue, .UnitOfMeasure = analysisEntry.UnitOfMeasure}
											Dim analysisPct = Double.Parse(analysisEntry.Data) / 100.0
											If analysisPct > 0 Then
												fertilizerAnalysis(analysisEntry.Label).Data = Double.Parse(fertilizerAnalysis(analysisEntry.Label).Data) + mass * analysisPct

												Dim tempValue As Double = 0
												If Double.TryParse(analysisEntry.Minimum, tempValue) AndAlso (fertilizerAnalysis(analysisEntry.Label).Minimum = Double.MaxValue OrElse Double.Parse(fertilizerAnalysis(analysisEntry.Label).Minimum) > tempValue) Then
													fertilizerAnalysis(analysisEntry.Label).Minimum = tempValue
												End If
												If Double.TryParse(analysisEntry.Maximum, tempValue) AndAlso (fertilizerAnalysis(analysisEntry.Label).Maximum = Double.MaxValue OrElse Double.Parse(fertilizerAnalysis(analysisEntry.Label).Maximum) > tempValue) Then
													fertilizerAnalysis(analysisEntry.Label).Maximum = tempValue
													fertilizerAnalysis(analysisEntry.Label).MaximumExceededWarning = analysisEntry.MaximumExceededWarning
												End If
											End If
										Else
											'How to handle other types?
										End If

										' now, do the components for this nutrient
										For Each componentEntry As AnalysisEntry In analysisEntry.Components
											If componentEntry.UnitOfMeasure = "%" Then
												If Not fertilizerAnalysis.ContainsKey(componentEntry.Label) Then fertilizerAnalysis(componentEntry.Label) = New AnalysisEntry() With {
													.Data = "0", .Id = componentEntry.Id, .Label = componentEntry.Label, .DataTypeValue = componentEntry.DataTypeValue, .Maximum = Double.MaxValue, .MaximumExceededWarning = "", .Minimum = Double.MaxValue, .UnitOfMeasure = componentEntry.UnitOfMeasure}
												Dim analysisPct = Double.Parse(componentEntry.Data) / 100.0
												If analysisPct > 0 Then
													fertilizerAnalysis(componentEntry.Label).Data = Double.Parse(fertilizerAnalysis(componentEntry.Label).Data) + mass * analysisPct

													Dim tempValue As Double = 0
													If Double.TryParse(componentEntry.Minimum, tempValue) AndAlso (fertilizerAnalysis(componentEntry.Label).Minimum = Double.MaxValue OrElse Double.Parse(fertilizerAnalysis(componentEntry.Label).Minimum) > tempValue) Then
														fertilizerAnalysis(componentEntry.Label).Minimum = tempValue
													End If
													If Double.TryParse(componentEntry.Maximum, tempValue) AndAlso (fertilizerAnalysis(componentEntry.Label).Maximum = Double.MaxValue OrElse Double.Parse(fertilizerAnalysis(componentEntry.Label).Maximum) > tempValue) Then
														fertilizerAnalysis(componentEntry.Label).Maximum = tempValue
														fertilizerAnalysis(componentEntry.Label).MaximumExceededWarning = componentEntry.MaximumExceededWarning
													End If
												End If
											Else
												'How to handle other types?
											End If
										Next
									Next
								End If
							End If
						Catch ex As Exception

						End Try
					Next
				End If
				If totalVolumeValid Then
					Try ' to convert the delivered quantity to the volume unit of measure
						totalVolume.Numeric += KaUnit.Convert(Tm2Database.Connection, delivered, density, totalVolume.UnitId).Numeric
					Catch ex As UnitConversionException
						totalVolumeValid = False
					End Try
					Try ' to convert the requested quantity to the volume unit of measure
						totalRequestedVolume.Numeric += KaUnit.Convert(Tm2Database.Connection, requested, density, totalVolume.UnitId).Numeric
					Catch ex As UnitConversionException
						totalVolumeValid = False
					End Try
				End If
			Next
		Next
	End Sub

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

	Private Function GetCompartmentFertilizerGuaranteedAnalysis(analysis As Dictionary(Of String, AnalysisEntry)) As String
		Dim html As String = ""
		Dim htmlCompartmentsHeader As String = "<tr><th style=""width:280px;"">Guaranteed Analysis by Compartment</th>"
		Dim htmlNutrients As String = ""
		Dim nutrientsList As ArrayList = New ArrayList
		Dim compartmentNutrientsList As New List(Of String)
		For Each compartmentNumber As Integer In _compartmentContents.Keys
			If _compartmentFertilizerAnalysis.ContainsKey(compartmentNumber) Then
				For Each nutrient As String In _compartmentFertilizerAnalysis(compartmentNumber).Keys
					Dim decimalPrecision As String = ""
					Dim percent As Double
					Try
						percent = Double.Parse(_compartmentFertilizerAnalysis(compartmentNumber)(nutrient).Data)
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
					If Not include AndAlso percent > 0 AndAlso percent > Double.Parse(_compartmentFertilizerAnalysis(compartmentNumber)(nutrient).Minimum) Then include = True
					If Not include AndAlso analysis(nutrient).Data > 0 Then include = True
					If include Then
						Dim nutrientLabel As String = GetNutrientLabel(nutrient)
						Dim index As Integer = -1
						If nutrientLabel.Length > 0 Then
							If Not nutrientsList.Contains(nutrientLabel) Then nutrientsList.Add(nutrientLabel)
							If compartmentNutrientsList.Count = 0 Then compartmentNutrientsList.Add(nutrientLabel)
						End If
						For i As Integer = 0 To compartmentNutrientsList.Count - 1
							If compartmentNutrientsList(i).Contains(nutrientLabel) Then
								index = i
							End If
						Next
						If index > -1 Then
							compartmentNutrientsList(index) = compartmentNutrientsList(index) & "," & percent.ToString
						Else
							compartmentNutrientsList.Add(nutrientLabel & "," & percent.ToString)
						End If
					End If
				Next
			End If
			htmlCompartmentsHeader &= $"<th style=""text-align:right; width:2em;"">{ compartmentNumber + 1 }</th><th></th>"
		Next
		htmlCompartmentsHeader &= "</tr>"
		If nutrientsList.Count > 0 Then
			For Each nutrientString As String In nutrientsList
				Dim percentString As String = ""
				For i As Integer = 0 To compartmentNutrientsList.Count - 1
					If compartmentNutrientsList(i).Contains(nutrientString) Then
						Dim a As String() = compartmentNutrientsList(i).Split(",")
						For item As Integer = 1 To a.Count - 1
							percentString += $"<td style=""text-align: right;"" class=""FertilizerGuaranteedAnalysis"">&nbsp;{a(item)}</td><td style=""text-align: left;"" class=""FertilizerGuaranteedAnalysis"">%&nbsp;</td>"
						Next
					End If
				Next
				htmlNutrients &= "<tr><td style=""border-bottom: 1px dashed black;"" class=""FertilizerGuaranteedAnalysis"">" & nutrientString & ":</td>" & percentString & "</tr>"
			Next
			html = "<table class=""FertilizerGuaranteedAnalysis"">" & htmlCompartmentsHeader & htmlNutrients & "</table>"
		End If
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
		Dim ownerId As Guid = ticket.OwnerId
		Try
			If ownerId.Equals(Guid.Empty) Then ownerId = orderInfo.OwnerId
		Catch ex As RecordNotFoundException

		End Try
		Boolean.TryParse(GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_DISCHARGE_LOCATION, "False"), _showDischargeLocation)
		Boolean.TryParse(GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_COMPARTMENT_FERTILIZER_GRADE, "False"), _showCompartmentFertilizerGrade)
		Boolean.TryParse(GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_COMPARTMENT_FERTILIZER_GUARANTEED_ANALYSIS, "False"), _showCompartmentFertilizerGuaranteedAnalysis)
		Boolean.TryParse(GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_FERTILIZER_GRADE, "False"), _showFertilizerGrade)
		Boolean.TryParse(GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_FERTILIZER_GUARANTEED_ANALYSIS, "False"), _showFertilizerGuaranteedAnalysis)
		Boolean.TryParse(GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_ANALYSIS_ENTRIES_ROUNDED_DOWN, "False"), _analysisEntriesRoundedDown)
		Dim analysisShowTrailingZeros As Boolean = True
		Boolean.TryParse(GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_ANALYSIS_SHOW_TRAILING_ZEROS, "True"), analysisShowTrailingZeros)
		Integer.TryParse(GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_GRADE_ANALYSIS_DECIMAL_COUNT_GREATER_THAN_ONE, 0), _gradeAnalysisDecimalCountGreaterThanOne)
		_gradeAnalysisFormatGreaterThanOne = "0" & IIf(_gradeAnalysisDecimalCountGreaterThanOne = 0, "", "." & "".PadRight(_gradeAnalysisDecimalCountGreaterThanOne, IIf(analysisShowTrailingZeros, "0", "#")))
		Integer.TryParse(GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_GRADE_ANALYSIS_DECIMAL_COUNT_LESS_THAN_ONE, 2), _gradeAnalysisDecimalCountLessThanOne)
		_gradeAnalysisFormatLessThanOne = "0" & IIf(_gradeAnalysisDecimalCountLessThanOne = 0, "", "." & "".PadRight(_gradeAnalysisDecimalCountLessThanOne, IIf(analysisShowTrailingZeros, "0", "#")))

		For Each unitId As String In GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_ADDITIONAL_UNITS_FOR_TICKET, "").Trim().Split(",")
			Try ' to parse the unit ID...
				_showAdditionalUnitList.Add(Guid.Parse(unitId))
			Catch ex As FormatException ' couldn't parse the unit ID, ignore it...
			End Try
		Next
		Boolean.TryParse(GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_BULK_PRODUCT_SUMMARY_TOTALS, False), _showBulkProductSummaryTotals)
		Boolean.TryParse(GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_BULK_PRODUCT_NOTES_ON_SUMMARY_ON_TICKET, False), _showBulkProductNotesSummaryTotals)
		Boolean.TryParse(GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_BULK_PRODUCT_EPA_NUMBER_ON_SUMMARY_ON_TICKET, False), _showBulkProductEpaNumberSummaryTotals)
		Boolean.TryParse(GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_PRODUCT_SUMMARY_TOTALS, False), _showProductSummaryTotals)
		Boolean.TryParse(GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_PRODUCT_NOTES_ON_SUMMARY_ON_TICKET, False), _showProductNotesSummaryTotals)
		Boolean.TryParse(GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_ORDER_SUMMARY, "False"), _showOrderSummary)
		Boolean.TryParse(GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_ORDER_SUMMARY_HISTORICAL, "False"), _showOrderSummaryHistorical)
		Boolean.TryParse(GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_COMPARTMENTS, "True"), _showCompartments)
		Boolean.TryParse(GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_PRODUCT_NOTES_ON_COMPARTMENT_ON_TICKET, True), _showCompartmentProductNotes)
		Boolean.TryParse(GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_COMPARTMENT_BULK_INGREDIENTS, GetSettingByOwnerId(ownerId, "ShowBulkIngredients", "True")), _showCompartmentBulkIngredients)
		Boolean.TryParse(GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_BULK_PRODUCT_NOTES_ON_COMPARTMENT_ON_TICKET, False), _showCompartmentBulkIngredientNotes)
		Boolean.TryParse(GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_BULK_PRODUCT_EPA_NUMBER_ON_COMPARTMENT_ON_TICKET, False), _showCompartmentBulkIngredientEpaNumbers)
		Boolean.TryParse(GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_COMPARTMENT_TOTALS, "False"), _showCompartmentTotals)
		Boolean.TryParse(GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_LOADED_INDEX_FOR_COMPARTMENT_ON_TICKET, True), _showCompartmentLoadedIndex)
		Boolean.TryParse(GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_DERIVED_FROM_ON_TICKET, False), _showDerivedFromOnTicket)
		Boolean.TryParse(GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_REQUESTED_QUANTITIES, "True"), _showRequestedQty)
		Dim showDateTime As String = GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_DATE_AND_TIME, "True")
		Boolean.TryParse(GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_DATE, showDateTime), _showDate)
		Boolean.TryParse(GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_TIME, showDateTime), _showTime)
		Boolean.TryParse(GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_CUSTOMER_LOCATION, "True"), _showCustomerLocation)
		Boolean.TryParse(GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_ACRES, "False"), _showAcres)
		Boolean.TryParse(GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_EMAIL_ADDRESS, "True"), _showEmailAddress)
		Boolean.TryParse(GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_FACILITY_ON_TICKET, _showFacility), _showFacility)
		Boolean.TryParse(GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_CUSTOMER, "True"), _showCustomer)
		Boolean.TryParse(GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_TOTAL, "True"), _showTotal)
		Boolean.TryParse(GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_DRIVER, "True"), _showDriver)
		Boolean.TryParse(GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_DRIVER_NUMBER, "True"), _showDriverNumber)
		Boolean.TryParse(GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_CARRIER, "True"), _showCarrier)
		Boolean.TryParse(GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_BRANCH_LOCATION, "True"), _showBranchLocation)
		Boolean.TryParse(GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_TRANSPORT, "True"), _showTransport)
		Boolean.TryParse(GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_OWNER, "True"), _showOwner)
		Boolean.TryParse(GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_APPLICATOR, "True"), _showApplicator)
		Boolean.TryParse(GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_NTEP_COMPLIANT, "True"), _ntepCompliant)
		Boolean.TryParse(GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_LOADED_BY_ON_TICKET, "False"), _showLoadedByOnTicket)
		_ownerDisclaimer = GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_DISCLAIMER, "")
		_blank1 = GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_BLANK1, "")
		_blank2 = GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_BLANK2, "")
		_blank3 = GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_BLANK3, "")
		_ownerMessage = GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_OWNER_MESSAGE, "")
		_ownerLogoPath = GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_LOGO_PATH, "")
		Dim showTransportTareWeights As Boolean = True
		If Boolean.TryParse(GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_TRANSPORT_TARE_WEIGHTS, "True"), showTransportTareWeights) AndAlso showTransportTareWeights Then
			_truckWeightOrder = GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_TRUCK_TGN_ORDER, "T-G-N").Split("-")
		End If
		Boolean.TryParse(GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_CUSTOMER_NOTES_ON_TICKET, _showCustomerNotesOnTicket), _showCustomerNotesOnTicket)
		Boolean.TryParse(GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_CUSTOMER_DESTINATION_NOTES_ON_TICKET, _showCustomerDestinationNotesOnTicket), _showCustomerDestinationNotesOnTicket)
		Boolean.TryParse(GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_DENSITY_ON_TICKET, _showDensityOnTicket), _showDensityOnTicket)
		_ticketAddonUrl = GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_TICKET_ADDON_URL, "")
		Boolean.TryParse(GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_RELEASE_NUMBER, "False"), _showReleaseNumber)
		Boolean.TryParse(GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_PURCHASE_ORDER_NUMBER, "True"), _showPurchaseOrderNumber)
		Boolean.TryParse(GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_PRODUCT_HAZARDOUS_MATERIAL, "False"), _showProductHazardousMaterial)
		_densityUnitPrecision = New Dictionary(Of String, String)
		For Each densityUnitPrecision As String In GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_DENSITY_UNIT_PRECISION, "").ToString().Split(",")
			Dim parts() As String = densityUnitPrecision.Split("|")
			If parts.Length = 3 Then _densityUnitPrecision(String.Format("{0}|{1}", parts(0), parts(1))) = parts(2)
		Next
		Boolean.TryParse(GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_APPLICATION_RATE_ON_TICKET, _showApplicationRateOnTicket), _showApplicationRateOnTicket)
		Boolean.TryParse(GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_USE_ORIGINAL_ORDERS_APPLICATION_RATE_ON_TICKET, _useOriginalOrdersApplicationRate), _useOriginalOrdersApplicationRate)
		Boolean.TryParse(GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_DISPLAY_BLEND_GROUP_NAME_AS_PRODUCT_NAME, False.ToString()), _displayBlendGroupNameAsProductName)
		Boolean.TryParse(GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_RINSE_ENTRIES, _showRinseEntries), _showRinseEntries)
		If Tm2Database.SystemItemTraceabilityEnabled Then
			Boolean.TryParse(GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_COMPARTMENT_BULK_INGREDIENT_LOT_NUMBER, _showCompartmentBulkIngredientLotNumber), _showCompartmentBulkIngredientLotNumber)
		Else
			_showCompartmentBulkIngredientLotNumber = False
		End If

		' Custom fields     
		Dim showAllCustomFieldsOnDeliveryTicket As Boolean = True
		Boolean.TryParse(GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_ALL_CUSTOM_FIELDS_ON_DELIVERY_TICKET, showAllCustomFieldsOnDeliveryTicket), showAllCustomFieldsOnDeliveryTicket)
		Dim customFieldsShown As String = ""
		For Each customFieldShown As String In GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_CUSTOM_FIELDS_ON_DELIVERY_TICKET, "").ToString().Split(",")
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

		Boolean.TryParse(GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_ALL_CUSTOM_PRE_LOAD_QUESTIONS, "False"), _showAllCustomPreLoadQuestions)
		_selectedCustomPreLoadQuestions = GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_CUSTOM_PRE_LOAD_QUESTIONS, "")
		Boolean.TryParse(GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_ALL_CUSTOM_POST_LOAD_QUESTIONS, "False"), _showAllCustomPostLoadQuestions)
		_selectedCustomPostLoadQuestions = GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_CUSTOM_POST_LOAD_QUESTIONS, "")
		Boolean.TryParse(GetSettingByOwnerId(ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_HIDE_ZERO_PERCENT_ANALYSIS_NUTRIENTS, "True"), _hideZeroPercentAnalysisNutrients)
		_hazMatTypeId = KaSetting.GetSetting(Tm2Database.Connection, "General/DefaultHazmatAnalysis", "", False, Nothing)
	End Sub

	Public Shared Function GetSettingByOwnerId(ByVal ownerId As Guid, ByVal settingName As String, ByVal defaultValue As Object) As Object
		'Find the owner specific setting.
		Dim allSettings As ArrayList = KaSetting.GetAll(Tm2Database.Connection, "name = " & Q("WebTicketSetting:" & ownerId.ToString & "/" & settingName) & " and deleted = 0", "")
		If allSettings.Count = 1 Then
			Return allSettings.Item(0).value
		End If

		'If there isn't an owner specific setting, get the All Owners setting, if that doesn't exist either, use the default value.
		Dim retval As String = KaSetting.GetSetting(Tm2Database.Connection, "WebTicketSetting:" & Guid.Empty.ToString & "/" & settingName, defaultValue.ToString, False, Nothing)
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
