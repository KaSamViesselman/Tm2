Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class PickTicket
	Inherits System.Web.UI.Page
	''' <summary>
	''' Parameters include staged_order_id, order_id, truck_id, tare_wt, scale_uom, tran_in_fac_id
	''' </summary>
	''' <param name="sender"></param>
	''' <param name="e"></param>
	''' <remarks></remarks>
	Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
		Dim connection As OleDb.OleDbConnection = Tm2Database.Connection
		AssignTicketStyleSheet()

		If Request.QueryString("staged_order_id") IsNot Nothing Then
			'this should be a staged order
			PopulateStagedOrderInfo(connection, Guid.Parse(Request.QueryString("staged_order_id")))
		ElseIf Request.QueryString("order_id") IsNot Nothing Then
			PopulateOrderInfo(connection, Guid.Parse(Request.QueryString("order_id")))
		Else
			litTicket.Text = "Invalid ticket ID"
		End If
	End Sub

	Private Sub AssignTicketStyleSheet()
		If IO.File.Exists(Server.MapPath("") & "\Styles\PickTicketCustom.css") Then
			StyleSheet.Text = "<link href=""Styles/PickTicketCustom.css"" type=""text/css"" rel=""stylesheet"" />"
		Else
			Dim css As String = "<link href=""style.css"" type=""text/css"" rel=""stylesheet"" />"
			css &= vbCrLf & "<style type=""text/css"">"
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

#Region " Staged orders "
	Private Sub PopulateStagedOrderInfo(ByVal connection As OleDb.OleDbConnection, ByVal stagedOrderId As Guid)
		Dim stagedOrderInfo As New KaStagedOrder(connection, stagedOrderId)
		Dim units As Dictionary(Of Guid, KaUnit) = New Dictionary(Of Guid, KaUnit)
		Dim defaultMassUnitInfo As KaUnit = KaUnit.GetUnit(connection, KaUnit.GetSystemDefaultMassUnitOfMeasure(connection, Nothing), units)
		Dim defaultVolumeUnitInfo As KaUnit = KaUnit.GetUnit(connection, KaUnit.GetSystemDefaultVolumeUnitOfMeasure(connection, Nothing), units)
		Dim products As Dictionary(Of Guid, KaProduct) = New Dictionary(Of Guid, KaProduct)
		Dim productDensities As Dictionary(Of Guid, KaRatio) = New Dictionary(Of Guid, KaRatio)
		Dim bulkProducts As Dictionary(Of Guid, KaBulkProduct) = New Dictionary(Of Guid, KaBulkProduct)
		Dim bulkProductIsFunction As Dictionary(Of Guid, Boolean) = New Dictionary(Of Guid, Boolean)
		Dim orders As Dictionary(Of Guid, KaOrder) = New Dictionary(Of Guid, KaOrder)
		Dim customerAccounts As Dictionary(Of Guid, KaCustomerAccount) = New Dictionary(Of Guid, KaCustomerAccount)
		Dim stagedTransportAmountsInScaleUnits As New Dictionary(Of Guid, Double)
		Dim ticketText As String = ""
		'Set the logo
		Dim logoList As New List(Of String)
		For Each stagedOrder As KaStagedOrderOrder In stagedOrderInfo.Orders
			If stagedOrder.Deleted Then Continue For
			Try
				Dim orderInfo As New KaOrder(connection, stagedOrder.OrderId)
				Dim ownerLogoPath As String = ticket.GetSettingByOwnerId(orderInfo.OwnerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_LOGO_PATH, "")
				If ownerLogoPath.Trim.Length > 0 AndAlso Not logoList.Contains(ownerLogoPath) Then logoList.Add(ownerLogoPath)
			Catch ex As Exception

			End Try
		Next

		For Each ownerLogoPath As String In logoList
			Try
				Dim logoImage As Drawing.Image = Drawing.Image.FromFile(ownerLogoPath)
				Dim toStream As New System.IO.MemoryStream
				logoImage.Save(toStream, Drawing.Imaging.ImageFormat.Jpeg)

				ticketText &= "<img id=""imgLogo"" runat=""server"" src="" data:image/gif;base64," & Convert.ToBase64String(toStream.GetBuffer) & """ />" & vbCrLf
			Catch ex As Exception
				ticketText &= "<img id=""imgLogo"" runat=""server"" src=""" & ownerLogoPath & """/>" & vbCrLf
			End Try
		Next
		ticketText &= "<h1 id=""lblInboundTicket"" runat=""server"">Inbound Ticket</h1>" & "<hr/>" & "<br/>" & vbCrLf
		Dim scaleUnitInfo As KaUnit
		Try
			scaleUnitInfo = New KaUnit(connection, Guid.Parse(Request.QueryString("scale_uom")))
		Catch ex As Exception
			scaleUnitInfo = defaultMassUnitInfo
		End Try

		ticketText &= "<div style=""width: 48%; float: left;"">"

		' Ship to
		Dim custAcctLocValid As Boolean = True
		Try
			ticketText &= "<br/>" & "<span id=""lblShipTo"" runat=""server"">Ship to: " & EncodeAsHtml((New KaCustomerAccountLocation(connection, stagedOrderInfo.CustomerAccountLocationId)).Name.Trim) & "</span><br/>" & "<hr/>" & vbCrLf
		Catch ex As RecordNotFoundException
			custAcctLocValid = False
		End Try

		' Notes
		If stagedOrderInfo.Notes.Trim.Length > 0 Then ticketText &= "<br/>" & "<span id=""lblLoadingInstructions"" runat=""server"" Text=""Loading Instructions: " & EncodeAsHtml(stagedOrderInfo.Notes.Trim) & """></span><br/>" & "<hr/>" & vbCrLf

		' Bay
		Try
			ticketText &= "<br/>" & "<span id=""lblBay"" runat=""server"">Bay: " & EncodeAsHtml(New KaBay(connection, stagedOrderInfo.BayId).Name.Trim) & "</span><br/>" & "<hr/>" & vbCrLf
		Catch ex As RecordNotFoundException

		End Try

		Dim orderItemAssigned As Dictionary(Of Guid, KaQuantity) = stagedOrderInfo.GetOrderItemAssignedAmounts(connection, Nothing, defaultMassUnitInfo.Id)

		' Orders
		ticketText &= "<table style=""width:100%;"">"
		For Each stagedOrderOrder As KaStagedOrderOrder In stagedOrderInfo.Orders
			If Not stagedOrderOrder.Deleted Then
				Try
					Dim orderInfo As KaOrder = KaOrder.GetOrder(connection, stagedOrderOrder.OrderId, orders, Nothing)
					' Order Number
					ticketText &= "<tr><td colspan=""2""><h2 id=""lblOrderNumber"" runat=""server"">Order Number: " & EncodeAsHtml(orderInfo.Number) & "</h2></td></tr>" & vbCrLf

					' Accounts
					Dim customerAccountNames As Dictionary(Of Guid, String) = New Dictionary(Of Guid, String)
					For Each orderAccount As KaOrderCustomerAccount In orderInfo.OrderAccounts
						If Not orderAccount.Deleted AndAlso orderAccount.Percentage > 0 AndAlso Not customerAccountNames.ContainsKey(orderAccount.CustomerAccountId) Then
							If Not customerAccounts.ContainsKey(orderAccount.CustomerAccountId) Then
								Try
									customerAccounts.Add(orderAccount.CustomerAccountId, New KaCustomerAccount(connection, orderAccount.CustomerAccountId))
								Catch ex As RecordNotFoundException
									customerAccounts.Add(orderAccount.CustomerAccountId, New KaCustomerAccount() With {.Name = "?"})
								End Try
							End If
							customerAccountNames.Add(orderAccount.CustomerAccountId, customerAccounts(orderAccount.CustomerAccountId).Name)
						End If
					Next

					Dim customerAccountNumber As String = ""
					For Each customerId As Guid In customerAccountNames.Keys
						If (customerAccountNumber.Length > 0) Then customerAccountNumber &= "<br />"
						customerAccountNumber &= customerAccountNames(customerId)
					Next
					If customerAccountNumber.Length = 0 Then customerAccountNumber = " ? "
					ticketText &= "<tr><td id=""lblAccountText"" runat=""server"" style=""width: 20%; vertical-align: top;"">Account: </td><td style=""width: 75%;"">" & customerAccountNumber & "</td></tr>"

					' ShipTo
					If Not custAcctLocValid Then
						Try
							ticketText &= "<tr><td class=""ShipToNames"" style=""vertical-align: top;"">Ship to: </td><td>" & EncodeAsHtml(New KaCustomerAccountLocation(connection, stagedOrderOrder.CustomerAccountLocationId).Name.Trim) & "</td></tr><tr><td colspan=""2""><hr/></td></tr>" & vbCrLf
						Catch ex As RecordNotFoundException
							If orderInfo.ShipToName.Length > 0 Then
								ticketText &= "<tr><td class=""ShipToNames"" style=""vertical-align: top;"">Ship to: </td><td>" & EncodeAsHtml(orderInfo.ShipToName.Trim) & "</td></tr><tr><td colspan=""2""><hr/></td></tr>" & vbCrLf
							End If
						End Try
					End If

					' Notes
					If orderInfo.Notes.Length > 0 Then ticketText &= "<tr><td class=""OrderNotes"" style=""vertical-align: top;"">Notes: </td><td>" & EncodeAsHtml(orderInfo.Notes.Trim) & "</td></tr><tr><td colspan=""2""><hr/></td></tr>" & vbCrLf

					' Order Details
					ticketText &= "<tr><td colspan=""2""><table style=""width:95%;""><tr><th colspan=""2"" style=""width:65%;"">Item</th><th style=""width:35%; text-align:right;"">Amount</th></tr>"

					For Each orderItem As KaOrderItem In orderInfo.OrderItems
						If Not orderItem.Deleted Then
							Dim requestedLoadAmount As Double = 0.0

							If orderItemAssigned.ContainsKey(orderItem.Id) Then
								If Not productDensities.ContainsKey(orderItem.ProductId) Then
									Try
										productDensities.Add(orderItem.ProductId, KaProduct.GetProduct(connection, orderItem.ProductId, products, Nothing).GetDensity(stagedOrderInfo.LocationId))
									Catch ex As Exception
										productDensities.Add(orderItem.ProductId, New KaRatio(0, defaultMassUnitInfo.Id, defaultVolumeUnitInfo.Id))
									End Try
								End If
								Try
									requestedLoadAmount = KaUnit.FastConvert(connection, orderItemAssigned(orderItem.Id), productDensities(orderItem.ProductId), orderItem.UnitId, units).Numeric
								Catch ex As Exception

								End Try
							End If

							If requestedLoadAmount > 0 Then ticketText &= OrderDetailDisplay(connection, stagedOrderInfo, orderItem.ProductId, orderItem.Request, Math.Max(0.0, orderItem.Request - orderItem.Delivered), requestedLoadAmount, orderItem.UnitId, True, units, products, bulkProducts, bulkProductIsFunction)
						End If
					Next
					ticketText &= "</table></td></tr>"
				Catch ex As RecordNotFoundException
				End Try
				ticketText &= "<tr><td colspan=""2""><hr/></td></tr>"
			End If
		Next
		ticketText &= "</table>"
		ticketText &= "</div>"
		ticketText &= "<div style=""width:48%; float: right;"">"
		' Carrier
		Try
			ticketText &= "<br/>" & "<span id=""lblCarrier"" runat=""server"">Carrier: " & EncodeAsHtml(New KaCarrier(connection, stagedOrderInfo.CarrierId).Name.Trim) & "</span><br/>" & "<hr/>" & vbCrLf
		Catch ex As RecordNotFoundException
		End Try

		' Driver
		Try
			ticketText &= "<br/>" & "<span id=""lblDriver"" runat=""server"">Driver: " & EncodeAsHtml(New KaCarrier(connection, stagedOrderInfo.DriverId).Name.Trim) & "</span><br/>" & "<hr/>" & vbCrLf
		Catch ex As RecordNotFoundException
		End Try

		For Each compInfo As KaStagedOrderCompartment In stagedOrderInfo.Compartments
			If Not stagedTransportAmountsInScaleUnits.ContainsKey(compInfo.StagedOrderTransportId) Then stagedTransportAmountsInScaleUnits.Add(compInfo.StagedOrderTransportId, 0.0)
			For Each compItem As KaStagedOrderCompartmentItem In compInfo.CompartmentItems
				If Not compItem.Deleted Then
					If Not units.ContainsKey(compItem.UnitId) Then units.Add(compItem.UnitId, New KaUnit(connection, compItem.UnitId))
					Dim compItemUnitInfo As KaUnit = units(compItem.UnitId)
					stagedTransportAmountsInScaleUnits(compInfo.StagedOrderTransportId) += KaUnit.Convert(compItem.Quantity, compItemUnitInfo, scaleUnitInfo, 0.0, defaultMassUnitInfo, defaultVolumeUnitInfo)
				End If
			Next
		Next

		' Transport
		For Each stagedTrans As KaStagedOrderTransport In stagedOrderInfo.Transports
			If stagedTrans.Deleted Then Continue For
			Dim stagedTransUnit As KaUnit
			Try
				stagedTransUnit = KaUnit.GetUnit(connection, stagedTrans.TareUnitId, units)
			Catch ex As RecordNotFoundException
				stagedTransUnit = New KaUnit
				stagedTransUnit.Abbreviation = ""
				stagedTransUnit.UnitPrecision = "#,##0"
			End Try

			Try
				Dim truckInfo As KaTransport = New KaTransport(connection, stagedTrans.TransportId)
				If truckInfo.Name.Length > 0 Then ticketText &= "<span id=""lblTransport"" runat=""server""><h2>Transport: " & EncodeAsHtml(truckInfo.Name & IIf(truckInfo.Number.Trim.Length > 0, " {" & truckInfo.Number & "}", "")) & "</h2></span><br/>"
			Catch ex As RecordNotFoundException
			End Try
			If stagedTrans.TareWeight > 0 Then
				ticketText &= "<span id=""lblTransportTare"" runat=""server"">Tare Weight: " & Format(stagedTrans.TareWeight, stagedTransUnit.UnitPrecision) & " " & stagedTransUnit.Abbreviation & "</span><br/>"
				Try
					If stagedTransportAmountsInScaleUnits(stagedTrans.Id) > 0 Then ticketText &= "<span id=""lblExpectedGrossWeight"" runat=""server"">Expected Gross Weight: " & Format(stagedTrans.TareWeight + KaUnit.FastConvert(connection, New KaQuantity(stagedTransportAmountsInScaleUnits(stagedTrans.Id), scaleUnitInfo.Id), New KaRatio(0.0, defaultMassUnitInfo.Id, defaultVolumeUnitInfo.Id), stagedTrans.TareUnitId, units).Numeric, stagedTransUnit.UnitPrecision) & " " & stagedTransUnit.Abbreviation & "</span><br/>" & vbCrLf
				Catch ex As ArgumentOutOfRangeException
				Catch ex As UnitConversionException
				End Try
			End If

			' Compartment product Details
			For Each compartment As KaStagedOrderCompartment In stagedOrderInfo.Compartments
				If Not compartment.Deleted AndAlso compartment.StagedOrderTransportId = stagedTrans.Id Then
					ticketText &= CompartmentDisplay(connection, compartment, stagedOrderInfo, units, orders, products, productDensities, bulkProducts, bulkProductIsFunction, defaultMassUnitInfo, defaultVolumeUnitInfo)
				End If
			Next

		Next
		ticketText &= "</div>"
		litTicket.Text = ticketText
	End Sub

	Public Function OrderDetailDisplay(connection As OleDb.OleDbConnection, stagedorderInfo As KaStagedOrder, id As Guid, requestAmount As Double, remainingAmount As Double, requestedForThisLoad As Double, unitId As Guid, isProduct As Boolean, units As Dictionary(Of Guid, KaUnit), products As Dictionary(Of Guid, KaProduct), bulkProducts As Dictionary(Of Guid, KaBulkProduct), bulkProductIsFunction As Dictionary(Of Guid, Boolean)) As String
		Dim orderDetailText As String = ""
		Dim className As String

		Dim unit As KaUnit = Nothing
		Try
			unit = KaUnit.GetUnit(connection, unitId, units)
		Catch ex As RecordNotFoundException
			unit = Nothing

		End Try
		If isProduct Then
			className = "OrderDetailProduct"
			Try
				Dim productInfo As KaProduct = KaProduct.GetProduct(connection, id, products, Nothing)
				orderDetailText &= $"<tr><td colspan=""3"" class=""{className}"">{ EncodeAsHtml(productInfo.Name)}</td></tr>"
				If unit Is Nothing Then
					orderDetailText &= $"<tr><td style=""width:10%;"">&nbsp;</td><td class=""{className}"">Order requested:</td><td class=""{className}"" style=""text-align:right;"">{requestAmount.ToString()}</td></tr>"
					orderDetailText &= $"<tr><td>&nbsp;</td><td class=""{className}"">Order remaining:</td><td class=""{className}"" style=""text-align:right;"">{remainingAmount.ToString()}</td></tr>"
					orderDetailText &= $"<tr><td>&nbsp;</td><td class=""{className}"">Requested for load:</td><td class=""{className}"" style=""text-align:right;"">{requestedForThisLoad.ToString()}</td></tr>"
				Else
					orderDetailText &= $"<tr><td style=""width:10%;"">&nbsp;</td><td class=""{className}"">Order requested:</td><td class=""{className}"" style=""text-align:right;"">{String.Format("{0:" + unit.UnitPrecision + "}", requestAmount)} { EncodeAsHtml(unit.Abbreviation)}</td></tr>"
					orderDetailText &= $"<tr><td>&nbsp;</td><td class=""{className}"">Order remaining:</td><td class=""{className}"" style=""text-align:right;"">{String.Format("{0:" + unit.UnitPrecision + "}", remainingAmount)} { EncodeAsHtml(unit.Abbreviation)}</td></tr>"
					orderDetailText &= $"<tr><td>&nbsp;</td><td class=""{className}"">Requested for load:</td><td class=""{className}"" style=""text-align:right;"">{String.Format("{0:" + unit.UnitPrecision + "}", requestedForThisLoad)} { EncodeAsHtml(unit.Abbreviation)}</td></tr>"
				End If
				Dim bulkProductText As String = ""
				For Each bulkProduct As KaProductBulkProduct In productInfo.ProductBulkItems
					If Not bulkProduct.Deleted AndAlso bulkProduct.LocationId = stagedorderInfo.LocationId Then
						bulkProductText &= OrderDetailDisplay(connection, stagedorderInfo, bulkProduct.BulkProductId, requestAmount * bulkProduct.Portion / 100, 0.0, requestedForThisLoad * bulkProduct.Portion / 100, unitId, False, units, products, bulkProducts, bulkProductIsFunction)
					End If
				Next
				If bulkProductText.Length > 0 Then orderDetailText &= $"<tr><td>&nbsp;</td><td colspan=""2""><table style=""width:95%;""><tr><th colspan=""2"" style=""width:65%;font-style: italic;"">Bulk products for {EncodeAsHtml(productInfo.Name)}</th><th style=""width:35%; text-align:right;"">&nbsp;</th></tr>" & bulkProductText & "</table></td></tr>"
			Catch ex As RecordNotFoundException
				orderDetailText &= $"<tr><td colspan=""3"" class=""{className}"">&nbsp;?&nbsp;</td></tr>"
			End Try
		Else
			className = "OrderDetailBulkProduct"
			Try
				Dim bulkProduct As KaBulkProduct = KaBulkProduct.GetBulkProduct(connection, id, bulkProducts, Nothing)
				If Not bulkProductIsFunction.ContainsKey(id) Then bulkProductIsFunction(id) = bulkProduct.IsFunction(connection, Nothing)
				If Not bulkProductIsFunction(id) Then
					orderDetailText &= $"<tr><td colspan=""3"" class=""{className}"" style=""font-style: italic;"">{ EncodeAsHtml(bulkProduct.Name)}</td></tr>"
					If unit Is Nothing Then
						orderDetailText &= $"<tr><td style=""width:10%;"">&nbsp;</td><td class=""{className}"" style=""font-style: italic;width:55%;"">Order requested:</td><td class=""{className}"" style=""font-style: italic; text-align:right;width:30%;"">{requestAmount.ToString()}</td></tr>"
						orderDetailText &= $"<tr><td>&nbsp;</td><td class=""{className}"">Requested for load:</td><td class=""{className}"" style=""font-style: italic; text-align:right;"">{requestedForThisLoad.ToString()}</td></tr>"
					Else
						orderDetailText &= $"<tr style=""width:10%;""><td>&nbsp;</td><td class=""{className}"" style=""font-style: italic;width:55%;"">Order requested:</td><td class=""{className}"" style=""font-style: italic; text-align:right;width:30%;"">{String.Format("{0:" + unit.UnitPrecision + "}", requestAmount)} { EncodeAsHtml(unit.Abbreviation)}</td></tr>"
						orderDetailText &= $"<tr><td>&nbsp;</td><td class=""{className}"" style=""font-style: italic;"">Requested for load:</td><td class=""{className}"" style=""font-style: italic; text-align:right;"">{String.Format("{0:" + unit.UnitPrecision + "}", requestedForThisLoad)} { EncodeAsHtml(unit.Abbreviation)}</td></tr>"
					End If
				End If
			Catch ex As RecordNotFoundException
				orderDetailText &= $"<tr><td>&nbsp;</td><td colspan=""2"" class=""{className}"" style=""font-style: italic;"">&nbsp;?&nbsp;</td></tr>"
			End Try
		End If
		Return orderDetailText
	End Function

	Public Function CompartmentDisplay(connection As OleDbConnection, compartment As KaStagedOrderCompartment, stagedOrderInfo As KaStagedOrder, units As Dictionary(Of Guid, KaUnit), orders As Dictionary(Of Guid, KaOrder), products As Dictionary(Of Guid, KaProduct), productDensities As Dictionary(Of Guid, KaRatio), bulkProducts As Dictionary(Of Guid, KaBulkProduct), bulkProductIsFunction As Dictionary(Of Guid, Boolean), weightUnitInfo As KaUnit, volumeUnitInfo As KaUnit) As String
		Dim compartmentText As String = $"<span class=""CompartmentDisplay""><h2>Compartment {(compartment.Position + 1).ToString()}</h2></span>"
		Dim productItemAmount As Dictionary(Of Guid, Double) = New Dictionary(Of Guid, Double)
		Dim productItemUnit As Dictionary(Of Guid, Guid) = New Dictionary(Of Guid, Guid)
		Dim locationId As Guid = stagedOrderInfo.LocationId
		compartmentText &= "<table style=""width:95%;""><tr><th colspan=""2"" style=""width:65%;"">Item</th><th style=""width:35%; text-align:right;"">Amount</th></tr>"
		For Each compartmentItem As KaStagedOrderCompartmentItem In compartment.CompartmentItems
			If Not compartmentItem.Deleted Then
				Dim compItemUnitInfo As KaUnit = KaUnit.GetUnit(connection, compartmentItem.UnitId, units)
				If compartmentItem.OrderItemId.Equals(Guid.Empty) Then
					' Blend all

					Dim orderTotalWeightQty As Double = 0.0
					Dim orderItemWeight As Dictionary(Of Guid, Double) = New Dictionary(Of Guid, Double)
					For Each order As KaStagedOrderOrder In stagedOrderInfo.Orders
						If Not order.Deleted Then
							Try
								Dim orderInfo As KaOrder = KaOrder.GetOrder(connection, order.OrderId, orders, Nothing)
								For Each orderItem As KaOrderItem In orderInfo.OrderItems
									If Not orderItem.Deleted Then
										Dim orderEntryAmount As Double = GetEntryWeight(connection, compartmentItem.Quantity, compartmentItem.OrderItemId, locationId, compItemUnitInfo, weightUnitInfo, volumeUnitInfo, products, productDensities)
										If Not orderItemWeight.ContainsKey(orderItem.Id) Then orderItemWeight(orderItem.Id) = 0.0

										orderItemWeight(orderItem.Id) += orderEntryAmount
										orderTotalWeightQty += orderEntryAmount
									End If
								Next
							Catch ex As Exception
								Throw New UnitConversionException("Unit Conversion Error", ex)
							End Try
						End If
					Next
					For Each order As KaStagedOrderOrder In stagedOrderInfo.Orders
						If Not order.Deleted Then
							Try
								Dim orderInfo As KaOrder = KaOrder.GetOrder(connection, order.OrderId, orders, Nothing)
								For Each orderItem As KaOrderItem In orderInfo.OrderItems
									If Not orderItem.Deleted Then
										Dim currentEntryWeight As Double = orderItemWeight(orderItem.Id)

										If Not productDensities.ContainsKey(orderItem.ProductId) Then
											Try
												productDensities(orderItem.ProductId) = KaProduct.GetProduct(connection, orderItem.ProductId, products, Nothing).GetDensity(locationId)
											Catch ex As RecordNotFoundException
												productDensities(orderItem.ProductId) = New KaRatio(0.0, weightUnitInfo.Id, volumeUnitInfo.Id)
											End Try
										End If

										If Not productItemAmount.ContainsKey(orderItem.ProductId) Then
											productItemAmount(orderItem.ProductId) = 0.0
											Try
												Dim conversion As KaQuantity = KaUnit.Convert(connection, New KaQuantity(compartmentItem.Quantity, compartmentItem.UnitId), productDensities(orderItem.ProductId), weightUnitInfo.Id)
												productItemUnit(orderItem.ProductId) = weightUnitInfo.Id
											Catch uex As UnitConversionException
												productItemUnit(orderItem.ProductId) = compartmentItem.UnitId
											End Try
										End If

										If (orderTotalWeightQty > 0.0) Then productItemAmount(orderItem.ProductId) += KaUnit.Convert(connection, New KaQuantity(compartmentItem.Quantity * (currentEntryWeight / orderTotalWeightQty), compartmentItem.UnitId), productDensities(orderItem.ProductId), productItemUnit(orderItem.ProductId)).Numeric
									End If
								Next
							Catch ex As Exception
								Throw New UnitConversionException("Unit Conversion Error", ex)
							End Try
						End If
					Next
				Else
					Try
						Dim orderItem As KaOrderItem = New KaOrderItem(connection, compartmentItem.OrderItemId)
						If Not productDensities.ContainsKey(orderItem.ProductId) Then
							Try
								productDensities(orderItem.ProductId) = KaProduct.GetProduct(connection, orderItem.ProductId, products, Nothing).GetDensity(locationId)
							Catch ex As RecordNotFoundException
								productDensities(orderItem.ProductId) = New KaRatio(0.0, weightUnitInfo.Id, volumeUnitInfo.Id)
							End Try
						End If

						If Not productItemAmount.ContainsKey(orderItem.ProductId) Then
							productItemAmount(orderItem.ProductId) = 0.0
							Try
								Dim conversion As KaQuantity = KaUnit.Convert(connection, New KaQuantity(compartmentItem.Quantity, compartmentItem.UnitId), productDensities(orderItem.ProductId), weightUnitInfo.Id)
								productItemUnit(orderItem.ProductId) = weightUnitInfo.Id
							Catch uex As UnitConversionException
								productItemUnit(orderItem.ProductId) = compartmentItem.UnitId
							End Try
						End If

						productItemAmount(orderItem.ProductId) += KaUnit.Convert(connection, New KaQuantity(compartmentItem.Quantity, compartmentItem.UnitId), productDensities(orderItem.ProductId), productItemUnit(orderItem.ProductId)).Numeric
					Catch ex As RecordNotFoundException
					End Try
				End If
			End If
		Next
		For Each productId As Guid In productItemAmount.Keys
			If productItemAmount(productId) > 0.0 Then compartmentText &= CompartmentContentsDisplay(connection, productId, productItemAmount(productId), productItemUnit(productId), True, units, products, bulkProducts, bulkProductIsFunction, locationId)
		Next
		compartmentText &= "</table><hr />"
		Return compartmentText
	End Function

	Private Function CompartmentContentsDisplay(connection As OleDbConnection, id As Guid, requestAmount As Double, unitId As Guid, isProduct As Boolean, units As Dictionary(Of Guid, KaUnit), products As Dictionary(Of Guid, KaProduct), bulkProducts As Dictionary(Of Guid, KaBulkProduct), bulkProductIsFunction As Dictionary(Of Guid, Boolean), locationId As Guid) As String
		Dim compartmentContentText As String = ""
		Dim unit As KaUnit = Nothing
		Try
			unit = KaUnit.GetUnit(connection, unitId, units)
		Catch ex As RecordNotFoundException
		End Try
		If isProduct Then
			Dim className As String = "CompartmentProduct"
			Try
				Dim productInfo As KaProduct = KaProduct.GetProduct(connection, id, products, Nothing)
				compartmentContentText &= $"<tr><td class=""{className}"" colspan=""2"">{EncodeAsHtml(productInfo.Name)}</td>"
				If unit IsNot Nothing Then
					compartmentContentText &= $"<td class=""{className}"" style=""text-align:right;"">{String.Format("{0:" + unit.UnitPrecision + "}", requestAmount)} {EncodeAsHtml(unit.Abbreviation)}</td></tr>"
				Else
					compartmentContentText &= $"<td class=""{className}"" style=""text-align:right;"">{requestAmount.ToString()}</td></tr>"
				End If
				Dim bulkProductText As String = ""
				For Each bulkProduct As KaProductBulkProduct In productInfo.ProductBulkItems
					If Not bulkProduct.Deleted AndAlso bulkProduct.LocationId.Equals(locationId) Then
						bulkProductText &= CompartmentContentsDisplay(connection, bulkProduct.BulkProductId, requestAmount * bulkProduct.Portion / 100, unitId, False, units, products, bulkProducts, bulkProductIsFunction, locationId)
					End If
				Next
				If bulkProductText.Length > 0 Then compartmentContentText &= $"<tr><td style=""width:10%"">&nbsp;</td><td colspan=""2""><table style=""width:95%;""><tr><th style=""font-style: italic;width:65%;"">Bulk products for {EncodeAsHtml(productInfo.Name)}</th><th style=""width:35%; text-align:right;"">&nbsp;</th></tr>" & bulkProductText & "</table></td></tr>"
			Catch ex As RecordNotFoundException
				compartmentContentText &= "<tr><td colspan=""2"" style=""font-style: italic;width:65%;"">&nbsp;?&nbsp;</td><td>&nbsp;</td></tr>"
			End Try
		Else
			Dim bulkProductInfo As KaBulkProduct = KaBulkProduct.GetBulkProduct(connection, id, bulkProducts, Nothing)
			If Not bulkProductIsFunction.ContainsKey(id) Then bulkProductIsFunction(id) = bulkProductInfo.IsFunction(connection, Nothing)
			If Not bulkProductIsFunction(id) Then
				Dim className As String = "CompartmentBulkProduct"
				Try
					compartmentContentText &= $"<tr><td class=""{className}"" style=""font-style: italic;"">{EncodeAsHtml(bulkProductInfo.Name)}</td>"
				Catch ex As RecordNotFoundException
					compartmentContentText &= $"<tr><td class=""{className}"" style=""font-style: italic;"">&nbsp;?&nbsp;</td>"
				End Try
				If unit IsNot Nothing Then
					compartmentContentText &= $"<td class=""{className}"" style=""text-align:right;font-style: italic;"">{String.Format("{0:" + unit.UnitPrecision + "}", requestAmount)} {EncodeAsHtml(unit.Abbreviation)}</td></tr>"
				Else
					compartmentContentText &= $"<td class=""{className}"" style=""text-align:right;font-style: italic;"">{requestAmount.ToString()}</td></tr>"
				End If
			End If
		End If
		Return compartmentContentText
	End Function

	Private Function GetEntryWeight(connection As OleDbConnection, compItemQuantity As Double, orderEntryId As Guid, locationId As Guid, compItemUnitInfo As KaUnit, weightUnitInfo As KaUnit, volumeUnitInfo As KaUnit, products As Dictionary(Of Guid, KaProduct), productDensities As Dictionary(Of Guid, KaRatio)) As Double
		Dim density As KaRatio = New KaRatio(0.0, weightUnitInfo.Id, volumeUnitInfo.Id)
		Try
			Dim entryInfo As KaOrderItem = New KaOrderItem(connection, orderEntryId)
			If Not productDensities.ContainsKey(entryInfo.ProductId) Then
				Dim productInfo As KaProduct = KaProduct.GetProduct(connection, entryInfo.ProductId, products, Nothing)
				productDensities.Add(entryInfo.ProductId, productInfo.GetDensity(locationId))
			End If
			density = productDensities(entryInfo.ProductId)
		Catch ex As Exception
		End Try
		Return KaUnit.Convert(connection, New KaQuantity(compItemQuantity, compItemUnitInfo.Id), density, weightUnitInfo.Id).Numeric
	End Function
#End Region

	Private Sub PopulateOrderInfo(ByVal connection As OleDb.OleDbConnection, ByVal orderId As Guid)
		Dim orderInfo As New KaOrder(connection, orderId)
		Dim ticketText As String = ""
		Dim scaleUnitInfo As KaUnit
		Dim defaultMassUnitInfo As New KaUnit(connection, KaUnit.GetSystemDefaultMassUnitOfMeasure(connection, Nothing))
		Dim defaultVolumeUnitInfo As New KaUnit(connection, KaUnit.GetSystemDefaultVolumeUnitOfMeasure(connection, Nothing))
		Try
			scaleUnitInfo = New KaUnit(connection, Guid.Parse(Request.QueryString("scale_uom")))
		Catch ex As Exception
			scaleUnitInfo = New KaUnit(connection, KaUnit.GetSystemDefaultMassUnitOfMeasure(connection, Nothing))
		End Try
		'Set the logo
		Dim ownerLogoPath As String = ticket.GetSettingByOwnerId(orderInfo.OwnerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_LOGO_PATH, "")
		Try
			Dim logoImage As Drawing.Image = Drawing.Image.FromFile(ownerLogoPath)
			Dim toStream As New System.IO.MemoryStream
			logoImage.Save(toStream, Drawing.Imaging.ImageFormat.Jpeg)

			ticketText &= "<img id=""imgLogo"" runat=""server"" src="" data:image/gif;base64," & Convert.ToBase64String(toStream.GetBuffer) & """ />"
		Catch ex As Exception
			ticketText &= "<img id=""imgLogo"" runat=""server"" src=""" & ownerLogoPath & """/>"
		End Try

		ticketText &= "<h1 id=""lblInboundTicket"" runat=""server"">Inbound Ticket</h1><hr/><br/>" & vbCrLf
		Dim netWeight As New KaQuantity(0.0, scaleUnitInfo.Id)

		ticketText &= "<h2 id=""lblOrderNumber"" runat=""server"">Order Number: " & EncodeAsHtml(orderInfo.Number) & "</h2><br/>" & vbCrLf

		Dim accountText As String = ""
		Dim accountCounter As Integer = 0
		For Each orderCust As KaOrderCustomerAccount In orderInfo.OrderAccounts
			If Not orderCust.CustomerAccountId.Equals(Guid.Empty) Then
				accountCounter += 1
				If accountText.Trim.Length > 0 Then accountText &= ", "
				Try
					Dim c As New KaCustomerAccount(connection, orderCust.CustomerAccountId)
					accountText &= EncodeAsHtml(c.Name & IIf(c.AccountNumber.Length > 0, " (" & c.AccountNumber & ")", ""))
				Catch ex As RecordNotFoundException
					accountText &= "Unknown"
				End Try
			End If
		Next
		If accountText.Trim.Length > 0 Then
			accountText = "Account" & IIf(accountCounter > 1, "s", "") & ": " & accountText
			If accountText.Trim.Length > 0 Then ticketText &= "<span id=""lblAccountText"" runat=""server"">" & accountText & "</span><br/>" & vbCrLf & "<br/>" & vbCrLf
		End If

		ticketText &= "<table style=""width:95%;"">"
		ticketText &= "<tr><th style=""width:44%;"">Product</th><th style=""width:28%; text-align:right;"">Requested Amount</th><th style=""width:28%; text-align:right;"">Remaining Amount</th></tr>" & vbCrLf
		For Each orderItem As KaOrderItem In orderInfo.OrderItems
			ticketText &= "<tr>"
			With orderItem
				Dim productInfo As New KaProduct(connection, .ProductId)
				ticketText &= "<td class="" CompartmentContent"">" & EncodeAsHtml(productInfo.Name) & "</td>"
				Dim orderItemUnitInfo As New KaUnit(connection, .UnitId)
				ticketText &= "<td style=""text-align:right;"" class=""CompartmentWeight"">" & Format(.Request, orderItemUnitInfo.UnitPrecision) & " " & New KaUnit(connection, .UnitId).Abbreviation & "</td>"
				ticketText &= "<td style=""text-align:right;"" class=""CompartmentWeight"">" & Format(Math.Max(0.0, .Request - .Delivered), orderItemUnitInfo.UnitPrecision) & " " & New KaUnit(connection, .UnitId).Abbreviation & "</td>"
				Try
					If netWeight.Numeric > Double.MinValue Then
						netWeight.Numeric += KaUnit.Convert(connection, New KaQuantity(Math.Max(0.0, .Request - .Delivered), .UnitId), New KaRatio(0.0, defaultMassUnitInfo.Id, defaultVolumeUnitInfo.Id), scaleUnitInfo.Id).Numeric
					End If
				Catch ex As UnitConversionException
					netWeight.Numeric = Double.MinValue
				End Try
			End With
			ticketText &= "</tr>" & vbCrLf
		Next

		ticketText &= "</table> " & vbCrLf
		ticketText &= "<hr/>"
		Try
			Dim truckInfo As New KaTransport(connection, Guid.Parse(Request.QueryString("truck_id")))
			ticketText &= "<span id=""lblTransport"" runat=""server"">Transport: " & truckInfo.Name & IIf(truckInfo.Number.Trim.Length > 0, " {" & truckInfo.Number & "}", "") & "</span><br/>" & vbCrLf
			Dim tareWeight As Double = truckInfo.TareWeight

			If Request.QueryString("tare_wt") IsNot Nothing Then
				tareWeight = Request.QueryString("tare_wt")
			Else
				scaleUnitInfo = New KaUnit(connection, truckInfo.UnitId)
			End If
			ticketText &= "<span id=""lblTransportTare"" runat=""server"">Tare Weight: " & Format(tareWeight, scaleUnitInfo.UnitPrecision) & " " & scaleUnitInfo.Abbreviation & "</span><br/>" & vbCrLf
			Try
				If netWeight.Numeric > 0 Then ticketText &= "<span id=""lblExpectedGrossWeight"" runat=""server"">Expected Gross Weight: " & Format(tareWeight + KaUnit.Convert(connection, netWeight, New KaRatio(0.0, defaultMassUnitInfo.Id, defaultVolumeUnitInfo.Id), scaleUnitInfo.Id).Numeric, scaleUnitInfo.UnitPrecision) & " " & scaleUnitInfo.Abbreviation & "</span><br/>" & vbCrLf
			Catch ex As ArgumentOutOfRangeException
			Catch ex As UnitConversionException

			End Try
			ticketText &= "<br/>" & vbCrLf & "<hr/>" & vbCrLf & "<br/>"
		Catch ex As Exception
		End Try

		litTicket.Text = ticketText
	End Sub

	Private Function EncodeAsHtml(ByVal text As String) As String
		Return Server.HtmlEncode(text).Replace(vbCrLf, "<br />").Replace(vbCr, "<br />").Replace(vbLf, "<br />")
	End Function
End Class