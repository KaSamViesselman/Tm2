Imports KahlerAutomation.KaTm2Database
Imports System.Collections

Public Class ReceivingPickTicket
	Inherits System.Web.UI.Page

	Private _showDate As Boolean = True
	Private _showTime As Boolean = True
	Private _showOwner As Boolean = True
	Private _showSupplier As Boolean = True
	Private _showCarrierId As Boolean = True
	Private _showTransport As Boolean = True
	Private _showDensityOnTicket As Boolean = True
	Private _densityUnitPrecision = New Dictionary(Of String, String)
	Private _showDriverName As Boolean = True
	Private _showDriverNumber As Boolean = True
	Private _showEmailAddress As Boolean = True
	Private _ownerMessage As String = ""
	Private _ownerDisclaimer As String = ""
	Private _ownerLogoPath As String = ""
	Private _additionalUnitsToDisplay As New List(Of Guid)
	Private _showGrossWeight As Boolean = True
	Private _showFacility As Boolean = True

	Protected Sub Page_Load(sender As Object, e As System.EventArgs) Handles Me.Load
		'htmlAddress &= "weighment_id=" & .Id.ToString
		'htmlAddress &= "&bol_id=" & inProgInfo.ReceivingPurchaseOrderId.ToString
		'htmlAddress &= "&transport_id=" & .TransportId.ToString
		'htmlAddress &= "&tran_in_fac_id=" & .TransportInFacilityId.ToString
		'htmlAddress &= "&carrier_id=" & inProgInfo.CarrierId.ToString
		'htmlAddress &= "&driver_id=" & inProgInfo.DriverId.ToString
		'htmlAddress &= "&driver_in_fac_id=" & inProgInfo.DriverInFacilityId.ToString
		'htmlAddress &= "&gross_wt=" & .Gross.ToString
		AssignTicketStyleSheet()

		Dim connection As OleDb.OleDbConnection = Tm2Database.Connection
		If Request.QueryString("weighment_id") IsNot Nothing Then
			PopulateInfoFromKaInProgressWeighment(connection, Guid.Parse(Request.QueryString("weighment_id")))
		ElseIf Request.QueryString("bol_id") IsNot Nothing Then
			PopulateInfoFromReceivingOrder(connection, Guid.Parse(Request.QueryString("bol_id")))
		ElseIf Request.QueryString("po_id") IsNot Nothing Then
			PopulateInfoFromReceivingOrder(connection, Guid.Parse(Request.QueryString("po_id")))
		End If
	End Sub

	Private Sub AssignTicketStyleSheet()
		If IO.File.Exists(Server.MapPath("") & "\Styles\ReceivingPickTicketCustom.css") Then
			StyleSheet.Text = "<link href=""Styles/ReceivingPickTicketCustom.css"" type=""text/css"" rel=""stylesheet"" />"
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

	Private Sub PopulateInfoFromKaInProgressWeighment(ByVal connection As OleDb.OleDbConnection, ByVal weighmentId As Guid)
		Dim weighmentInfo As New KaInProgressWeighment(connection, weighmentId)
		Dim inProgInfo As New KaInProgress(connection, weighmentInfo.InProgressId)
		PopulateInfoFromReceivingOrder(connection, inProgInfo.ReceivingPurchaseOrderId)
		If _showDate AndAlso _showTime Then
			litDateTime.Text = String.Format("{0:g}", weighmentInfo.GrossDate)
		ElseIf _showDate Then
			litDateTime.Text = String.Format("{0:d}", weighmentInfo.GrossDate)
		Else
			litDateTime.Text = ""
		End If
		pnlDateTime.Visible = _showDate AndAlso litDateTime.Text.Trim.Length > 0

		If Request.QueryString("transport_id") Is Nothing Then
			Try
				Dim transportInfo As New KaTransport(connection, weighmentInfo.TransportId)
				litTransports.Text = transportInfo.Name
			Catch ex As RecordNotFoundException

			End Try
			rowTransport.Visible = _showTransport AndAlso litTransports.Text.Trim.Length > 0
		End If

		If Request.QueryString("carrier_id") Is Nothing Then
			Try
				Dim carrierInfo As New KaCarrier(connection, inProgInfo.CarrierId)
				litCarrier.Text = carrierInfo.Name
			Catch ex As RecordNotFoundException

			End Try
			rowCarrier.Visible = _showCarrierId AndAlso litCarrier.Text.Trim.Length > 0
		End If

		If Request.QueryString("driver_id") Is Nothing Then
			Try
				Dim driverInfo As New KaDriver(connection, inProgInfo.DriverId)
				litDriver.Text = driverInfo.Name
				If _showDriverNumber AndAlso driverInfo.Number.Trim.Length > 0 AndAlso Not driverInfo.Name.Trim().ToUpper().Equals(driverInfo.Number.Trim().ToUpper()) Then _
					litDriver.Text &= String.Format(" ({0})", driverInfo.Number.Trim())
			Catch ex As RecordNotFoundException

			End Try
			rowDriver.Visible = _showDriverName AndAlso litDriver.Text.Trim.Length > 0
		End If

		' Show gross weight information
		If Request.QueryString("gross_wt") Is Nothing AndAlso weighmentInfo.Gross > 0 Then
			Try
				Dim unitInfo As New KaUnit(connection, weighmentInfo.UnitId)
				litGrossWeight.Text = String.Format("{0:" & unitInfo.UnitPrecision & "} {1}", weighmentInfo.Gross, unitInfo.Abbreviation)
			Catch ex As RecordNotFoundException

			End Try
		End If
		rowGrossWeight.Visible = _showGrossWeight AndAlso litGrossWeight.Text.Trim.Length > 0

		' populate the sold by information
		litFacility.Text = ""
		Try
			Dim facility As New KaLocation(connection, inProgInfo.LocationId)
			If litFacility.Text = "" Then litFacility.Text = facility.Name
			If litFacility.Text.Length > 0 Then litFacility.Text &= "<br><br>"
			If facility.Street.Length > 0 Then litFacility.Text &= "<br>" & facility.Street
			If facility.City.Length + facility.State.Length + facility.ZipCode.Length > 0 Then litFacility.Text &= "<br>" & facility.City & " " & facility.State & " " & facility.ZipCode
			If facility.Phone.Length > 0 Then litFacility.Text &= "<br>" & facility.Phone
			If _showEmailAddress AndAlso facility.Email.Length > 0 Then litFacility.Text &= "<br>" & facility.Email
		Catch ex As RecordNotFoundException ' suppress exception
		End Try

		pnlFacility.Visible = _showFacility AndAlso litFacility.Text.Trim.Length > 0
	End Sub

	Private Sub PopulateInfoFromReceivingOrder(ByVal connection As OleDb.OleDbConnection, ByVal receivingOrderId As Guid)
		Dim receivingPO As New KaReceivingPurchaseOrder(connection, receivingOrderId)
		SetSettings(receivingPO)
		litOrderNumber.Text = receivingPO.Number

		pnlDateTime.Visible = False

		' populate the sold by information
		litSoldTo.Text = ""
		Try
			Dim owner As New KaOwner(connection, receivingPO.OwnerId)
			If litSoldTo.Text = "" Then litSoldTo.Text = owner.Name
			If litSoldTo.Text.Length > 0 Then litSoldTo.Text &= "<br><br>"
			If owner.Street.Length > 0 Then litSoldTo.Text &= "<br>" & owner.Street
			If owner.City.Length + owner.State.Length + owner.ZipCode.Length > 0 Then litSoldTo.Text &= "<br>" & owner.City & " " & owner.State & " " & owner.ZipCode
			If owner.Phone.Length > 0 Then litSoldTo.Text &= "<br>" & owner.Phone
			If _showEmailAddress AndAlso owner.Email.Length > 0 Then litSoldTo.Text &= "<br>" & owner.Email
			pnlSoldTo.Visible = _showOwner
		Catch ex As RecordNotFoundException ' suppress exception
			pnlSoldTo.Visible = False
		End Try

		' populate the comments
		litComments.Text = ""

		' populate the sold to information
		litSoldBy.Text &= ""
		Try
			Dim supplierAccount As New KaSupplierAccount(connection, receivingPO.SupplierAccountId)
			If litSoldBy.Text.Length = 0 Then litSoldBy.Text = supplierAccount.Name
			If litSoldBy.Text.Length > 0 Then litSoldBy.Text &= "<br><br>"
			If supplierAccount.Street.Length > 0 Then litSoldBy.Text &= "<br>" & supplierAccount.Street
			If supplierAccount.City.Length + supplierAccount.State.Length + supplierAccount.ZipCode.Length > 0 Then litSoldBy.Text &= "<br>" & supplierAccount.City & " " & supplierAccount.State & " " & supplierAccount.ZipCode
			If supplierAccount.Phone.Length > 0 Then litSoldBy.Text &= "<br>" & supplierAccount.Phone
			If _showEmailAddress AndAlso supplierAccount.Email.Length > 0 Then litSoldBy.Text &= "<br>" & supplierAccount.Email
			pnlSoldBy.Visible = _showSupplier
		Catch ex As RecordNotFoundException
			pnlSoldBy.Visible = False
		End Try

		'Get the Transport(s) Info
		litTransports.Text = ""
		Dim transportId As Guid = Guid.Empty
		If Request.QueryString("transport_id") IsNot Nothing AndAlso Guid.TryParse(Request.QueryString("transport_id"), transportId) Then
			Try
				Dim transportInfo As New KaTransport(connection, transportId)
				litTransports.Text = transportInfo.Name
			Catch ex As RecordNotFoundException

			End Try
		End If
		rowTransport.Visible = _showTransport AndAlso litTransports.Text.Trim.Length > 0

		' populate product information
		Dim bulkProductInfo As New KaBulkProduct(connection, receivingPO.BulkProductId)
		litProducts.Text = "<table cellspacing=""0"" style=""width: 100%;"">"
		litProducts.Text &= "   <tr>"
		litProducts.Text &= "       <td style=""border-bottom: 1px solid black;""><strong>Item</strong></td>"
		litProducts.Text &= "       <td style=""border-bottom: 1px solid black;""><strong>Requested</strong></td>"
		If receivingPO.Delivered > 0 Then litProducts.Text &= "       <td style=""border-bottom: 1px solid black;""><strong>Remaining</strong></td>"
		If _showDensityOnTicket Then litProducts.Text &= "       <td style=""border-bottom: 1px solid black;""><strong>Density</strong></td>"
		litProducts.Text &= "   </tr>"

		litProducts.Text &= "<tr>"
		litProducts.Text &= "<td style=""border-bottom: 1px solid black; vertical-align: top;"">" & bulkProductInfo.Name & "</td>"
		' Received Quantity
		litProducts.Text &= "<td style=""border-bottom: 1px solid black; vertical-align: top;"">"
		Dim units As New Dictionary(Of Guid, KaUnit)
		Dim unit As KaUnit = GetUnitById(receivingPO.UnitId, units)
		Dim precision As String = unit.UnitPrecision
		Dim density As New KaRatio(bulkProductInfo.Density, bulkProductInfo.WeightUnitId, bulkProductInfo.VolumeUnitId)

		litProducts.Text &= String.Format("{0:" & precision & "} " & unit.Abbreviation, receivingPO.Purchased)
		Dim additionalUnits As String = ""
		For Each additionalUnitId As Guid In _additionalUnitsToDisplay
			If additionalUnitId.Equals(receivingPO.UnitId) Then Continue For ' This is already covered, so continue on
			Dim additionalUnit As KaUnit = GetUnitById(additionalUnitId, units)
			Dim additionalUnitPrecision As String = additionalUnit.UnitPrecision
			If additionalUnits.Length > 0 Then additionalUnits &= ", "
			Try
				additionalUnits &= String.Format("{0:" & additionalUnitPrecision & "} " & additionalUnit.Abbreviation, KaUnit.Convert(connection, New KaQuantity(receivingPO.Purchased, receivingPO.UnitId), density, additionalUnitId).Numeric)
			Catch ex As UnitConversionException
				additionalUnits &= "N/A"
			End Try
			additionalUnits &= " " & additionalUnit.Abbreviation
		Next
		If additionalUnits.Length > 0 Then litProducts.Text &= " (" & additionalUnits & ")"
		litProducts.Text &= "</td>"
		If receivingPO.Delivered > 0 Then
			litProducts.Text &= "<td style=""border-bottom: 1px solid black; vertical-align: top;"">"
			Dim deliveredQty As Double = Math.Max(0, receivingPO.Purchased - receivingPO.Delivered)
			litProducts.Text &= String.Format("{0:" & precision & "} " & unit.Abbreviation, deliveredQty)
			additionalUnits = ""
			For Each additionalUnitId As Guid In _additionalUnitsToDisplay
				If additionalUnitId.Equals(receivingPO.UnitId) Then Continue For ' This is already covered, so continue on
				Dim additionalUnit As KaUnit = GetUnitById(additionalUnitId, units)
				If additionalUnits.Length > 0 Then additionalUnits &= ", "
				Try
					additionalUnits &= String.Format("{0:" & additionalUnit.UnitPrecision & "} " & additionalUnit.Abbreviation, KaUnit.Convert(connection, New KaQuantity(deliveredQty, receivingPO.UnitId), density, additionalUnit.Id).Numeric)
				Catch ex As UnitConversionException
					additionalUnits &= "N/A"
				End Try
				additionalUnits &= " " & additionalUnit.Abbreviation
			Next
			If additionalUnits.Length > 0 Then litProducts.Text &= " (" & additionalUnits & ")"
			litProducts.Text &= "</td>"
		End If

		If _showDensityOnTicket Then
			litProducts.Text &= "<td style=""border-bottom: 1px solid black; vertical-align: top;"">"

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
			litProducts.Text &= "</td>"
		End If
		litProducts.Text &= "</tr>"
		litProducts.Text &= "</table>"

		'Get the Carrier Info
		litCarrier.Text = ""
		Dim carrierId As Guid = Guid.Empty
		If _showCarrierId AndAlso Request.QueryString("carrier_id") IsNot Nothing AndAlso Guid.TryParse(Request.QueryString("carrier_id"), carrierId) Then
			Try
				Dim carrierInfo As New KaCarrier(connection, carrierId)
				litCarrier.Text = carrierInfo.Name
			Catch ex As RecordNotFoundException

			End Try
		End If
		rowCarrier.Visible = _showCarrierId AndAlso litCarrier.Text.Trim.Length > 0

		'Get the driver Info
		litDriver.Text = ""
		Dim driverId As Guid = Guid.Empty
		If Request.QueryString("driver_id") IsNot Nothing AndAlso Guid.TryParse(Request.QueryString("driver_id"), driverId) Then
			Try
				Dim driverInfo As New KaDriver(connection, driverId)
				litDriver.Text = driverInfo.Name
				If _showDriverNumber AndAlso driverInfo.Number.Trim.Length > 0 AndAlso Not driverInfo.Name.Trim().ToUpper().Equals(driverInfo.Number.Trim().ToUpper()) Then _
				   litDriver.Text &= String.Format(" ({0})", driverInfo.Number.Trim())
			Catch ex As RecordNotFoundException

			End Try
		End If
		rowDriver.Visible = _showDriverName AndAlso litDriver.Text.Trim.Length > 0

		' Show gross weight information
		litGrossWeight.Text = ""
		Dim grossWeight As Double = 0
		If Request.QueryString("gross_wt") IsNot Nothing AndAlso Double.TryParse(Request.QueryString("gross_wt"), grossWeight) AndAlso grossWeight > 0 Then
			litGrossWeight.Text = grossWeight
		End If
		rowGrossWeight.Visible = _showGrossWeight AndAlso litGrossWeight.Text.Trim.Length > 0


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

		pnlFacility.Visible = False
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

	Private Sub SetSettings(ByVal order As KaReceivingPurchaseOrder)
		Dim connection As OleDb.OleDbConnection = Tm2Database.Connection
		Dim ownerId As Guid = order.OwnerId

		Boolean.TryParse(DefaultReceivingWebPickTicketSettings.GetReceivingWebPickTicketSettingByOwnerId(connection, ownerId, DefaultReceivingWebPickTicketSettings.SN_SHOW_DATE_ON_TICKET, _showDate), _showDate)
		Boolean.TryParse(DefaultReceivingWebPickTicketSettings.GetReceivingWebPickTicketSettingByOwnerId(connection, ownerId, DefaultReceivingWebPickTicketSettings.SN_SHOW_TIME_ON_TICKET, _showTime), _showTime)
		Boolean.TryParse(DefaultReceivingWebPickTicketSettings.GetReceivingWebPickTicketSettingByOwnerId(connection, ownerId, DefaultReceivingWebPickTicketSettings.SN_SHOW_OWNER_ON_TICKET, _showOwner), _showOwner)
		Boolean.TryParse(DefaultReceivingWebPickTicketSettings.GetReceivingWebPickTicketSettingByOwnerId(connection, ownerId, DefaultReceivingWebPickTicketSettings.SN_SHOW_SUPPLIER_ON_TICKET, _showSupplier), _showSupplier)
		Boolean.TryParse(DefaultReceivingWebPickTicketSettings.GetReceivingWebPickTicketSettingByOwnerId(connection, ownerId, DefaultReceivingWebPickTicketSettings.SN_SHOW_CARRIER_ON_TICKET, _showCarrierId), _showCarrierId)
		Boolean.TryParse(DefaultReceivingWebPickTicketSettings.GetReceivingWebPickTicketSettingByOwnerId(connection, ownerId, DefaultReceivingWebPickTicketSettings.SN_SHOW_TRANSPORT_ON_TICKET, _showTransport), _showTransport)

		Boolean.TryParse(DefaultReceivingWebPickTicketSettings.GetReceivingWebPickTicketSettingByOwnerId(connection, ownerId, DefaultReceivingWebPickTicketSettings.SN_SHOW_DENSITY_ON_TICKET, _showDensityOnTicket), _showDensityOnTicket)

		Boolean.TryParse(DefaultReceivingWebPickTicketSettings.GetReceivingWebPickTicketSettingByOwnerId(connection, ownerId, DefaultReceivingWebPickTicketSettings.SN_SHOW_DRIVER_ON_TICKET, _showDriverName), _showDriverName)
		Boolean.TryParse(DefaultReceivingWebPickTicketSettings.GetReceivingWebPickTicketSettingByOwnerId(connection, ownerId, DefaultReceivingWebPickTicketSettings.SN_SHOW_DRIVER_NUMBER_ON_TICKET, _showDriverNumber), _showDriverNumber)
		Boolean.TryParse(DefaultReceivingWebPickTicketSettings.GetReceivingWebPickTicketSettingByOwnerId(connection, ownerId, DefaultReceivingWebPickTicketSettings.SN_SHOW_EMAIL_ADDRESSES_ON_TICKET, _showEmailAddress), _showEmailAddress)
		_ownerLogoPath = DefaultReceivingWebPickTicketSettings.GetReceivingWebPickTicketSettingByOwnerId(connection, ownerId, DefaultReceivingWebPickTicketSettings.SN_SHOW_LOGO_PATH_ON_TICKET, _ownerLogoPath)
		_ownerMessage = DefaultReceivingWebPickTicketSettings.GetReceivingWebPickTicketSettingByOwnerId(connection, ownerId, DefaultReceivingWebPickTicketSettings.SN_SHOW_OWNER_MESSAGE_ON_TICKET, _ownerMessage)
		_ownerDisclaimer = DefaultReceivingWebPickTicketSettings.GetReceivingWebPickTicketSettingByOwnerId(connection, ownerId, DefaultReceivingWebPickTicketSettings.SN_SHOW_DISCLAIMER_ON_TICKET, _ownerDisclaimer)
		For Each densityUnitPrecision As String In DefaultReceivingWebPickTicketSettings.GetReceivingWebPickTicketSettingByOwnerId(connection, ownerId, DefaultReceivingWebPickTicketSettings.SN_DENSITY_UNIT_OF_PRECISION, "").ToString().Split(",")
			Dim parts() As String = densityUnitPrecision.Split("|")
			If parts.Length = 3 Then _densityUnitPrecision(String.Format("{0}|{1}", parts(0), parts(1))) = parts(2)
		Next
		For Each unitId As String In DefaultReceivingWebPickTicketSettings.GetReceivingWebPickTicketSettingByOwnerId(connection, ownerId, DefaultReceivingWebPickTicketSettings.SN_ADDITIONAL_TICKET_UNITS, "").Trim().Split(",")
			Dim additionalUnit As Guid = Guid.Empty
			Guid.TryParse(unitId, additionalUnit)
			If Not additionalUnit.Equals(Guid.Empty) AndAlso Not _additionalUnitsToDisplay.Contains(additionalUnit) Then _additionalUnitsToDisplay.Add(additionalUnit)
		Next
		Boolean.TryParse(DefaultReceivingWebPickTicketSettings.GetReceivingWebPickTicketSettingByOwnerId(connection, ownerId, DefaultReceivingWebPickTicketSettings.SN_SHOW_GROSS_WEIGHT_ON_TICKET, _showGrossWeight), _showGrossWeight)
		Boolean.TryParse(DefaultReceivingWebPickTicketSettings.GetReceivingWebPickTicketSettingByOwnerId(connection, ownerId, DefaultReceivingWebPickTicketSettings.SN_SHOW_FACILITY_ON_TICKET, _showFacility), _showFacility)
	End Sub

	Private Function GetUnitById(unitId As Guid, unitList As Dictionary(Of Guid, KaUnit)) As KaUnit
		If Not unitList.ContainsKey(unitId) Then unitList(unitId) = New KaUnit(Tm2Database.Connection, unitId)

		Return unitList(unitId)
	End Function

	Private Function EncodeAsHtml(ByVal text As String) As String
		Return Server.HtmlEncode(text).Replace(vbCrLf, "<br />").Replace(vbCr, "<br />").Replace(vbLf, "<br />")
	End Function
End Class