Imports KahlerAutomation.KaTm2Database
Imports System.Collections

Public Class ReceivingTicket
	Inherits System.Web.UI.Page
	Private _showDate As Boolean = True
	Private _showTime As Boolean = True
	Private _showOwner As Boolean = True
	Private _showSupplier As Boolean = True
	Private _showCarrierId As Boolean = True
	Private _showTransport As Boolean = True
	Private _showTransportTareWeights As Boolean = True
	Private _showDensityOnTicket As Boolean = True
	Private _densityUnitPrecision = New Dictionary(Of String, String)
	Private _showDriverName As Boolean = True
	Private _showDriverNumber As Boolean = True
	Private _showEmailAddress As Boolean = True
	Private _showLotNumber As Boolean = False
	Private _ownerMessage As String = ""
	Private _ownerDisclaimer As String = ""
	Private _ownerLogoPath As String = ""
	Private _truckWeightOrder() As String = {"T", "G", "N"}
	Private _additionalUnitsToDisplay As New List(Of Guid)
	Private _ticketCustomFieldsTable As New DataTable

	Protected Sub Page_Load(sender As Object, e As System.EventArgs) Handles Me.Load
		AssignTicketStyleSheet()
		Dim receivingTicketId As Guid = Guid.Empty
		Try
			Dim ticket As KaReceivingTicket
			If Not Guid.TryParse(Request.QueryString("receiving_ticket_id"), receivingTicketId) OrElse receivingTicketId.Equals(Guid.Empty) Then
				Guid.TryParse(Request.QueryString("ticket_id"), receivingTicketId)
			End If

			ticket = New KaReceivingTicket(Tm2Database.Connection, receivingTicketId)
			GetSettings(ticket)
			litTicketNumber.Text = ticket.Number
			litOrderNumber.Text = ticket.PurchaseOrderNumber
			If _showDate AndAlso _showTime Then
				litDateTime.Text = String.Format("{0:g}", ticket.DateOfDelivery)
			ElseIf _showDate Then
				litDateTime.Text = String.Format("{0:d}", ticket.DateOfDelivery)
			Else
				litDateTime.Text = ""
			End If
			pnlDateTime.Visible = _showDate

			' populate the sold by information
			litSoldTo.Text = ticket.OwnerName
			Try
				Dim owner As New KaOwner(Tm2Database.Connection, ticket.OwnerId)
				If litSoldTo.Text = "" Then litSoldTo.Text = owner.Name
				If owner.Street.Length > 0 Then litSoldTo.Text &= vbCrLf & owner.Street
				If owner.City.Length + owner.State.Length + owner.ZipCode.Length > 0 Then litSoldTo.Text &= vbCrLf & owner.City & " " & owner.State & " " & owner.ZipCode
				If owner.Phone.Length > 0 Then litSoldTo.Text &= vbCrLf & owner.Phone
				If _showEmailAddress AndAlso owner.Email.Length > 0 Then litSoldTo.Text &= vbCrLf & owner.Email
                Utilities.GetCustomField(litSoldTo.Text, _ticketCustomFieldsTable, KaOwner.TABLE_NAME, ticket.Id)
                pnlSoldTo.Visible = _showOwner
			Catch ex As RecordNotFoundException ' suppress exception
				pnlSoldTo.Visible = False
			End Try
			litSoldTo.Text = EncodeAsHtml(litSoldTo.Text)

			' populate the comments
			litComments.Text = IIf(ticket.Notes.Trim().Length > 0, "<strong>Comments:</strong><br>", "") & ticket.Notes

			' populate the sold to information
			litSoldBy.Text = ticket.SupplierAccountName
			Try
				Dim supplierAccount As New KaSupplierAccount(Tm2Database.Connection, ticket.SupplierAccountId)
				If litSoldBy.Text.Length = 0 Then litSoldBy.Text = supplierAccount.Name
				If supplierAccount.Street.Length > 0 Then litSoldBy.Text &= vbCrLf & supplierAccount.Street
				If supplierAccount.City.Length + supplierAccount.State.Length + supplierAccount.ZipCode.Length > 0 Then litSoldBy.Text &= vbCrLf & supplierAccount.City & " " & supplierAccount.State & " " & supplierAccount.ZipCode
				If supplierAccount.Phone.Length > 0 Then litSoldBy.Text &= vbCrLf & supplierAccount.Phone
				If _showEmailAddress AndAlso supplierAccount.Email.Length > 0 Then litSoldBy.Text &= vbCrLf & supplierAccount.Email
                Utilities.GetCustomField(litSoldBy.Text, _ticketCustomFieldsTable, KaSupplierAccount.TABLE_NAME, ticket.Id)
                pnlSoldBy.Visible = _showSupplier
			Catch ex As RecordNotFoundException
				pnlSoldBy.Visible = False
			End Try
			litSoldBy.Text = EncodeAsHtml(litSoldBy.Text)

			' populate the facility information
			litFacility.Text &= ticket.LocationName
			Try
				Dim locationInfo As New KaLocation(Tm2Database.Connection, ticket.LocationId)
				If litFacility.Text.Length = 0 Then litFacility.Text = locationInfo.Name
				If locationInfo.Street.Length > 0 Then litFacility.Text &= vbCrLf & locationInfo.Street
				If locationInfo.City.Length + locationInfo.State.Length + locationInfo.ZipCode.Length > 0 Then litFacility.Text &= vbCrLf & locationInfo.City & " " & locationInfo.State & " " & locationInfo.ZipCode
				If locationInfo.Phone.Length > 0 Then litFacility.Text &= vbCrLf & locationInfo.Phone
				If _showEmailAddress AndAlso locationInfo.Email.Length > 0 Then litFacility.Text &= vbCrLf & locationInfo.Email
                Utilities.GetCustomField(litFacility.Text, _ticketCustomFieldsTable, KaLocation.TABLE_NAME, ticket.Id)
            Catch ex As RecordNotFoundException
			End Try
			litFacility.Text = EncodeAsHtml(litFacility.Text)

			'Get the Transport(s) Info
			Dim transportsText As String = "<table style=""width:100%;"">"
			Dim deliveredQty As Double = 0.0
			Dim deliveredQtyValid As Boolean = True
			If ticket.LinkedTicketsId.Equals(Guid.Empty) Then
				transportsText &= GetTransportInfo(Tm2Database.Connection, ticket, deliveredQty, ticket.UnitId, deliveredQtyValid)
			Else
				Dim tickets As ArrayList = KaReceivingTicket.GetAll(Tm2Database.Connection, "voided=0 AND deleted=0 AND linked_tickets_id=" & Q(ticket.LinkedTicketsId), "transport_name, transport_number")
				For Each ticketTotal As KaReceivingTicket In tickets
					transportsText &= GetTransportInfo(Tm2Database.Connection, ticketTotal, deliveredQty, ticket.UnitId, deliveredQtyValid)
				Next

			End If
			transportsText &= "</table>"
			litTransports.Text = transportsText
			rowTransport.Visible = _showTransport AndAlso litTransports.Text.Trim.Length > 0

			' populate product information
			Dim linkedTicket As Boolean = Not ticket.LinkedTicketsId.Equals(Guid.Empty)
			litProducts.Text = "<table cellspacing=""0"" style=""width: 100%;"">"
			litProducts.Text &= vbTab & "<tr>"
			litProducts.Text &= vbTab & vbTab & "<td style=""border-bottom: 1px solid black;""><strong>Item</strong></td>"
			If _showLotNumber Then litProducts.Text &= vbTab & vbTab & "<td style=""border-bottom: 1px solid black;""><strong>Lot</strong></td>"
			litProducts.Text &= vbTab & vbTab & "<td style=""border-bottom: 1px solid black;""><strong>Net delivered</strong></td>"
			If _showDensityOnTicket Then litProducts.Text &= vbTab & vbTab & "<td style=""border-bottom: 1px solid black;""><strong>Density</strong></td>"
			litProducts.Text &= vbTab & "</tr>"

			litProducts.Text &= vbTab & "<tr>"
			litProducts.Text &= vbTab & vbTab & "<td style=""border-bottom: 1px solid black; vertical-align: top;"">" & ticket.BulkProductName & "</td>"
			If _showLotNumber Then litProducts.Text &= vbTab & vbTab & "<td style=""border-bottom: 1px solid black; vertical-align: top;"">" & ticket.LotNumber & "</td>"
			' Received Quantity
			litProducts.Text &= vbTab & vbTab & "<td style=""border-bottom: 1px solid black; vertical-align: top;"">"
			Dim units As New Dictionary(Of Guid, KaUnit)
			Dim unit As KaUnit = GetUnitById(ticket.UnitId, units)
			Dim precision As String = ticket.UnitPrecision
			If precision.Trim.Length = 0 Then precision = KaPanel.GetUnitPrecision(Tm2Database.Connection, ticket.PanelId, ticket.UnitId)
			Dim density As New KaRatio(ticket.Density, ticket.WeightUnitId, ticket.VolumeUnitId)
			If density.Numeric = 0.0 Then
				Try
					With New KaBulkProduct(Tm2Database.Connection, ticket.BulkProductId)
						density = New KaRatio(.Density, .WeightUnitId, .VolumeUnitId)
					End With
				Catch ex As RecordNotFoundException

				End Try
			End If

			litProducts.Text &= String.Format("{0:" & precision & "} " & unit.Abbreviation, deliveredQty)
			Dim additionalUnits As String = ""
			For Each additionalUnitId As Guid In _additionalUnitsToDisplay
				If additionalUnitId.Equals(ticket.UnitId) Then Continue For ' This is already covered, so continue on
				Dim additionalUnit As KaUnit = GetUnitById(additionalUnitId, units)
				Dim additionalUnitPrecision As String = KaPanel.GetUnitPrecision(Tm2Database.Connection, ticket.PanelId, additionalUnitId)
				If additionalUnits.Length > 0 Then additionalUnits &= ", "
				Try
					additionalUnits &= String.Format("{0:" & precision & "} ", KaUnit.Convert(Tm2Database.Connection, New KaQuantity(deliveredQty, ticket.UnitId), density, additionalUnitId).Numeric)
				Catch ex As UnitConversionException
					additionalUnits &= "N/A "
				End Try
				additionalUnits &= additionalUnit.Abbreviation
			Next
			If additionalUnits.Length > 0 Then litProducts.Text &= " (" & additionalUnits & ")"
			litProducts.Text &= vbTab & vbTab & "</td>"
			If _showDensityOnTicket Then
				litProducts.Text &= vbTab & vbTab & "<td style=""border-bottom: 1px solid black; vertical-align: top;"">"

				If density.Numeric > 0.0 Then ' display the density
					Dim densityUnitPrecision As String
					Try ' to lookup the unit precision for these density units...
						densityUnitPrecision = _densityUnitPrecision(String.Format("{0}|{1}", density.NumeratorUnitId.ToString(), density.DenominatorUnitId.ToString()))
					Catch ex As KeyNotFoundException ' these density units are not in the dictionary...
						Dim massWhole As UInteger = 0, massFractional As UInteger = UInteger.MaxValue
						KaUnit.GetPrecisionDigits(GetUnitById(density.NumeratorUnitId, units).UnitPrecision, massWhole, massFractional)
						Dim volumeWhole As UInteger = 0, volumeFractional As UInteger = UInteger.MaxValue
						KaUnit.GetPrecisionDigits(GetUnitById(density.NumeratorUnitId, units).UnitPrecision, volumeWhole, volumeFractional)
						densityUnitPrecision = KaUnit.GetPrecisionString(Math.Max(massWhole, volumeWhole), Math.Min(massFractional, volumeFractional), ",", 0)
					End Try
					litProducts.Text &= String.Format("{0:" & densityUnitPrecision & "} {1}/{2}", density.Numeric, GetUnitById(density.NumeratorUnitId, units).Abbreviation, GetUnitById(density.DenominatorUnitId, units).Abbreviation)
				Else ' density isn't available
					litProducts.Text &= "n/a"
				End If
				litProducts.Text &= vbTab & vbTab & "</td>"
			End If
			litProducts.Text &= vbTab & "</tr>"
			litProducts.Text &= "</table>"

			Dim ticketCustomFields As String = ""
            Utilities.GetCustomField(ticketCustomFields, _ticketCustomFieldsTable, KaInterface.TABLE_NAME, ticket.Id)
            Utilities.GetCustomField(ticketCustomFields, _ticketCustomFieldsTable, KaLocation.TABLE_NAME, ticket.Id)
            Utilities.GetCustomField(ticketCustomFields, _ticketCustomFieldsTable, KaReceivingPurchaseOrder.TABLE_NAME, ticket.Id)

            tdTicketCustomFields.InnerHtml = EncodeAsHtml(ticketCustomFields)
			trTicketCustomFields.Visible = ticketCustomFields.Length > 0

			'Get the Carrier Info
			litCarrier.Text = ticket.CarrierName
            Utilities.GetCustomField(litCarrier.Text, _ticketCustomFieldsTable, KaCarrier.TABLE_NAME, ticket.Id)
            litCarrier.Text = EncodeAsHtml(litCarrier.Text)
			rowCarrier.Visible = _showCarrierId AndAlso litCarrier.Text.Trim.Length > 0

			'Get the driver Info
			litDriver.Text = ticket.DriverName & IIf(_showDriverNumber AndAlso ticket.DriverNumber.Length > 0, " (" & ticket.DriverNumber & ")", "")
            Utilities.GetCustomField(litDriver.Text, _ticketCustomFieldsTable, KaDriver.TABLE_NAME, ticket.Id)
            If ticket.DriverSignature.Length > 0 Then
				litDriver.Text &= " <img alt=""Signature"" src=""data:image/jpg;base64," & ticket.DriverSignature & """ />"
			End If
			litDriver.Text = EncodeAsHtml(litDriver.Text)
			rowDriver.Visible = _showDriverName AndAlso litDriver.Text.Trim.Length > 0

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

			litOwnerMessage.Text = EncodeAsHtml(_ownerMessage)
			pnlOwnerMessage.Visible = _ownerMessage.Trim.Length > 0

			'Set the disclaimer
			litDisclaimer.Text = "<small>" & EncodeAsHtml(_ownerDisclaimer) & "</small>"
			rowDisclaimer.Visible = _ownerDisclaimer.Trim.Length > 0

		Catch ex As RecordNotFoundException
			litTicketNumber.Text = "Invalid ticket ID (" & receivingTicketId.ToString() & ")"
		Catch ex As ArgumentNullException ' suppress exception
		End Try
	End Sub

	Private Sub AssignTicketStyleSheet()
		If IO.File.Exists(Server.MapPath("") & "\Styles\ReceivingTicketCustom.css") Then
			StyleSheet.Text = "<link href=""Styles/ReceivingTicketCustom.css"" type=""text/css"" rel=""stylesheet"" />"
		Else
			Dim css As String = "<link href=""style.css"" type=""text/css"" rel=""stylesheet"" />"
			css &= vbCrLf & "<style type=""text/css"">"
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

	Private Function GetOrdinalNumber(number As Integer) As String
		Dim suffix As String
		Dim offset As Integer = number Mod 10
		If offset = 1 AndAlso number <> 11 Then
			suffix = "st"
		ElseIf offset = 2 AndAlso number <> 12 Then
			suffix = "nd"
		ElseIf offset = 3 AndAlso number <> 13 Then
			suffix = "rd"
		Else
			suffix = "th"
		End If
		Return String.Format("{0:0}{1}", number, suffix)
	End Function

	Private Function GetTransportInfo(ByVal oConn As OleDb.OleDbConnection, ByVal ticket As KaReceivingTicket, ByRef quantityDelivered As Double, ByVal unitId As Guid, ByRef totalValid As Boolean) As String
		Dim transportsText As String = "<tr>"
		transportsText &= "<td style=""vertical-align:text-top;"">" & ticket.TransportName & IIf(ticket.TransportNumber.Length > 0, " (", "") & ticket.TransportNumber & IIf(ticket.TransportNumber.Length > 0, ")</td>", "")
		If _showTransportTareWeights Then
			Try
				quantityDelivered += KaUnit.Convert(oConn, New KaQuantity(ticket.Delivered, ticket.UnitId), New KaRatio(ticket.Density, ticket.WeightUnitId, ticket.VolumeUnitId), unitId).Numeric
			Catch ex As UnitConversionException
				totalValid = False
			End Try

			Try
				Dim transport As KaTransport = Nothing
				If ticket.TransportId = Guid.Empty Then
					'Transport weights may still be valid even if there is not a transport selected.  This is possible with the 2025 and staged order list.
					transport = New KaTransport
					transport.UnitId = unitId
				Else
					transport = New KaTransport(oConn, ticket.TransportId)
				End If
				If _truckWeightOrder.Length > 0 Then
					Dim unitInfo As New KaUnit(oConn, transport.UnitId)
					Dim total As Double = 0.0
					Dim truckTotalValid As Boolean = True
					Try
						total = KaUnit.Convert(oConn, New KaQuantity(ticket.Delivered, ticket.UnitId), New KaRatio(ticket.Density, ticket.WeightUnitId, ticket.VolumeUnitId), transport.UnitId).Numeric()
					Catch ex As UnitConversionException
						truckTotalValid = False
					End Try
					Dim tareValid As Boolean = ticket.TareWeight > 0.0 OrElse ticket.TareDate > New DateTime(1900, 1, 1, 0, 0, 0)
					Dim unitFormat As String = unitInfo.UnitPrecision
					If truckTotalValid Then
						Dim grossLine As String = "<td>&nbsp;</td><td style=""text-align:right;"">Gross:</td>" & "<td>" & Server.HtmlEncode(Format(ticket.TareWeight + total, unitFormat)) & " " & Server.HtmlEncode(unitInfo.Abbreviation) & "</td>"
						Dim tareLine As String = "<td>&nbsp;</td><td style=""text-align:right;"">Tare:</td>" & "<td>" & Server.HtmlEncode(Format(ticket.TareWeight, unitFormat)) & " " & Server.HtmlEncode(unitInfo.Abbreviation) & "</td>"
						Dim netLine As String = "<td>&nbsp;</td><td style=""text-align:right;"">Net:</td>" & "<td>" & Server.HtmlEncode(Format(total, unitFormat)) & " " & Server.HtmlEncode(unitInfo.Abbreviation) & "</td>"

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
                Utilities.GetCustomField(transportsText, _ticketCustomFieldsTable, KaTransport.TABLE_NAME, ticket.Id)
            Catch ex As RecordNotFoundException

			End Try
		End If
		transportsText &= "</tr>"

		Return transportsText
	End Function

	Private Sub GetSettings(ByVal ticket As KaReceivingTicket)
		Dim connection As OleDb.OleDbConnection = Tm2Database.Connection
		Dim order As New KaReceivingPurchaseOrder(Tm2Database.Connection, ticket.ReceivingPurchaseOrderId)
		Dim ownerId As Guid = order.OwnerId

		Boolean.TryParse(DefaultReceivingWebTicketSettings.GetReceivingWebTicketSettingByOwnerId(connection, ownerId, KaSetting.DefaultReceivingWebTicketSettings.SN_SHOW_DATE_ON_TICKET, _showDate), _showDate)
		Boolean.TryParse(DefaultReceivingWebTicketSettings.GetReceivingWebTicketSettingByOwnerId(connection, ownerId, KaSetting.DefaultReceivingWebTicketSettings.SN_SHOW_TIME_ON_TICKET, _showTime), _showTime)
		Boolean.TryParse(DefaultReceivingWebTicketSettings.GetReceivingWebTicketSettingByOwnerId(connection, ownerId, KaSetting.DefaultReceivingWebTicketSettings.SN_SHOW_OWNER_ON_TICKET, _showOwner), _showOwner)
		Boolean.TryParse(DefaultReceivingWebTicketSettings.GetReceivingWebTicketSettingByOwnerId(connection, ownerId, KaSetting.DefaultReceivingWebTicketSettings.SN_SHOW_SUPPLIER_ON_TICKET, _showSupplier), _showSupplier)
		Boolean.TryParse(DefaultReceivingWebTicketSettings.GetReceivingWebTicketSettingByOwnerId(connection, ownerId, KaSetting.DefaultReceivingWebTicketSettings.SN_SHOW_CARRIER_ON_TICKET, _showCarrierId), _showCarrierId)
		Boolean.TryParse(DefaultReceivingWebTicketSettings.GetReceivingWebTicketSettingByOwnerId(connection, ownerId, KaSetting.DefaultReceivingWebTicketSettings.SN_SHOW_TRANSPORT_ON_TICKET, _showTransport), _showTransport)

		Boolean.TryParse(DefaultReceivingWebTicketSettings.GetReceivingWebTicketSettingByOwnerId(connection, ownerId, KaSetting.DefaultReceivingWebTicketSettings.SN_SHOW_DENSITY_ON_TICKET, _showDensityOnTicket), _showDensityOnTicket)

		If Boolean.TryParse(DefaultReceivingWebTicketSettings.GetReceivingWebTicketSettingByOwnerId(connection, ownerId, KaSetting.DefaultReceivingWebTicketSettings.SN_SHOW_TRANSPORT_WEIGHTS_ON_TICKET, _showTransportTareWeights), _showTransportTareWeights) AndAlso _showTransportTareWeights Then
			_truckWeightOrder = DefaultReceivingWebTicketSettings.GetReceivingWebTicketSettingByOwnerId(connection, ownerId, KaSetting.DefaultReceivingWebTicketSettings.SN_TARE_GROSS_NET_ORDER, "T-G-N").Split("-")
		End If

		Boolean.TryParse(DefaultReceivingWebTicketSettings.GetReceivingWebTicketSettingByOwnerId(connection, ownerId, KaSetting.DefaultReceivingWebTicketSettings.SN_SHOW_DRIVER_ON_TICKET, _showDriverName), _showDriverName)
		Boolean.TryParse(DefaultReceivingWebTicketSettings.GetReceivingWebTicketSettingByOwnerId(connection, ownerId, KaSetting.DefaultReceivingWebTicketSettings.SN_SHOW_DRIVER_NUMBER_ON_TICKET, _showDriverNumber), _showDriverNumber)
		Boolean.TryParse(DefaultReceivingWebTicketSettings.GetReceivingWebTicketSettingByOwnerId(connection, ownerId, KaSetting.DefaultReceivingWebTicketSettings.SN_SHOW_EMAIL_ADDRESSES_ON_TICKET, _showEmailAddress), _showEmailAddress)
		Boolean.TryParse(DefaultReceivingWebTicketSettings.GetReceivingWebTicketSettingByOwnerId(connection, ownerId, KaSetting.DefaultReceivingWebTicketSettings.SN_SHOW_LOT_NUMBER_ON_TICKET, _showLotNumber), _showLotNumber)
		_showLotNumber = _showLotNumber AndAlso Tm2Database.SystemItemTraceabilityEnabled

		_ownerLogoPath = DefaultReceivingWebTicketSettings.GetReceivingWebTicketSettingByOwnerId(connection, ownerId, KaSetting.DefaultReceivingWebTicketSettings.SN_SHOW_LOGO_PATH_ON_TICKET, _ownerLogoPath)
		_ownerMessage = DefaultReceivingWebTicketSettings.GetReceivingWebTicketSettingByOwnerId(connection, ownerId, KaSetting.DefaultReceivingWebTicketSettings.SN_SHOW_OWNER_MESSAGE_ON_TICKET, _ownerMessage)
		_ownerDisclaimer = DefaultReceivingWebTicketSettings.GetReceivingWebTicketSettingByOwnerId(connection, ownerId, KaSetting.DefaultReceivingWebTicketSettings.SN_SHOW_DISCLAIMER_ON_TICKET, _ownerDisclaimer)
		For Each densityUnitPrecision As String In DefaultReceivingWebTicketSettings.GetReceivingWebTicketSettingByOwnerId(connection, ownerId, KaSetting.DefaultReceivingWebTicketSettings.SN_DENSITY_UNIT_OF_PRECISION, "").ToString().Split(",")
			Dim parts() As String = densityUnitPrecision.Split("|")
			If parts.Length = 3 Then _densityUnitPrecision(String.Format("{0}|{1}", parts(0), parts(1))) = parts(2)
		Next
		For Each unitId As String In DefaultReceivingWebTicketSettings.GetReceivingWebTicketSettingByOwnerId(connection, ownerId, KaSetting.DefaultReceivingWebTicketSettings.SN_ADDITIONAL_TICKET_UNITS, "").Trim().Split(",")
			Dim additionalUnit As Guid = Guid.Empty
			Guid.TryParse(unitId, additionalUnit)
			If Not additionalUnit.Equals(Guid.Empty) AndAlso Not _additionalUnitsToDisplay.Contains(additionalUnit) Then _additionalUnitsToDisplay.Add(additionalUnit)
		Next

		' Custom fields     
		Dim showAllCustomFieldsOnDeliveryTicket As Boolean = True
		Boolean.TryParse(DefaultReceivingWebTicketSettings.GetReceivingWebTicketSettingByOwnerId(connection, ownerId, KaSetting.DefaultReceivingWebTicketSettings.SN_SHOW_ALL_CUSTOM_FIELDS_ON_DELIVERY_TICKET, showAllCustomFieldsOnDeliveryTicket), showAllCustomFieldsOnDeliveryTicket)
		Dim customFieldsShown As String = ""
		For Each customFieldShown As String In DefaultReceivingWebTicketSettings.GetReceivingWebTicketSettingByOwnerId(connection, ownerId, KaSetting.DefaultReceivingWebTicketSettings.SN_CUSTOM_FIELDS_ON_DELIVERY_TICKET, "").ToString().Split(",")
			If customFieldShown.Length > 0 Then
				If customFieldsShown.Length > 0 Then customFieldsShown &= ","
				customFieldsShown &= Q(customFieldShown)
			End If
		Next
		' not showing all custom fields, and not having any checked will cause the IIF statement to cause the select query to not return any records
		Dim getAllTicketAndChildrenIdsSql As String = $"SELECT id FROM {KaReceivingTicket.TABLE_NAME} WHERE (id = {Q(ticket.Id)}) "
		Dim getCustomFieldsDA As New OleDb.OleDbDataAdapter($"SELECT {KaCustomField.TABLE_NAME}.id, {KaCustomField.TABLE_NAME}.table_name, {KaCustomField.TABLE_NAME}.field_name, {KaCustomField.TABLE_NAME}_1.table_name AS source_table, {KaCustomFieldData.TABLE_NAME}.value " &
			$"FROM {KaCustomField.TABLE_NAME} " &
			$"INNER JOIN {KaCustomField.TABLE_NAME} AS {KaCustomField.TABLE_NAME}_1 ON {KaCustomField.TABLE_NAME}.{KaCustomField.FN_DELIVERY_TICKET_CREATION_SOURCE_FIELD} = {KaCustomField.TABLE_NAME}_1.id " &
			$"INNER JOIN {KaCustomFieldData.TABLE_NAME} ON {KaCustomField.TABLE_NAME}.id = {KaCustomFieldData.TABLE_NAME}.{KaCustomFieldData.FN_CUSTOM_FIELD_ID} " &
			$"WHERE ({KaCustomFieldData.TABLE_NAME}.{KaCustomFieldData.FN_RECORD_ID} = {Q(ticket.Id)}) AND ({KaCustomField.TABLE_NAME}.deleted = 0) AND ({KaCustomField.TABLE_NAME}_1.deleted = 0) AND ({KaCustomFieldData.TABLE_NAME}.deleted = 0) " &
			IIf(showAllCustomFieldsOnDeliveryTicket, "", IIf(customFieldsShown.Length > 0, $"AND ({KaCustomFieldData.TABLE_NAME}.{KaCustomFieldData.FN_CUSTOM_FIELD_ID} IN (" & customFieldsShown & ")) ", $"AND ({KaCustomField.TABLE_NAME}.deleted = 1)")) &
			$"ORDER BY {KaCustomField.TABLE_NAME}.{KaCustomField.FN_TABLE_NAME}, {KaCustomField.TABLE_NAME}.{KaCustomField.FN_FIELD_NAME}, source_table", Tm2Database.Connection)

		If Tm2Database.CommandTimeout > 0 Then getCustomFieldsDA.SelectCommand.CommandTimeout = Tm2Database.CommandTimeout
		getCustomFieldsDA.Fill(_ticketCustomFieldsTable)

	End Sub

	Private Function GetUnitById(unitId As Guid, unitList As Dictionary(Of Guid, KaUnit)) As KaUnit
		If Not unitList.ContainsKey(unitId) Then unitList(unitId) = New KaUnit(Tm2Database.Connection, unitId)

		Return unitList(unitId)
	End Function

    Private Function EncodeAsHtml(ByVal text As String) As String
        Return Server.HtmlEncode(text).Replace(vbCrLf, "<br />").Replace(vbCr, "<br />").Replace(vbLf, "<br />")
    End Function
End Class
