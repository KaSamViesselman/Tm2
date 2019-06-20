Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class ReceivingPFV : Inherits System.Web.UI.Page

    Private _currentUser As KaUser

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Request.QueryString("po_id") <> Nothing Then
            _currentUser = Utilities.GetUser(Me)

            Dim poId As Guid = Guid.Parse(Request.QueryString("po_id"))
            Dim po As KaReceivingPurchaseOrder = New KaReceivingPurchaseOrder(GetUserConnection(_currentUser.Id), poId)

            Dim owner As KaOwner = New KaOwner
            If po.OwnerId <> Guid.Empty Then
                owner = New KaOwner(GetUserConnection(_currentUser.Id), po.OwnerId)
            End If

            Dim supplier As KaSupplierAccount = New KaSupplierAccount
            If po.SupplierAccountId <> Guid.Empty Then
                supplier = New KaSupplierAccount(GetUserConnection(_currentUser.Id), po.SupplierAccountId)
            End If

            Dim unit As KaUnit = New KaUnit()
            If po.UnitId <> Guid.Empty Then
                unit = New KaUnit(GetUserConnection(_currentUser.Id), po.UnitId)
            End If

            Dim bulkProd As KaBulkProduct = New KaBulkProduct()
            If po.BulkProductId <> Guid.Empty Then
                bulkProd = New KaBulkProduct(GetUserConnection(_currentUser.Id), po.BulkProductId)
            End If


            Dim htmlString As String = ""
            htmlString &= "<table cellpadding=2 cellspacing=2>"

            'First Row
            htmlString &= "<tr>"
            htmlString &= "<td width=""200px"">Bill of Lading Number:</td>"
            htmlString &= "<td width=""600px"">" & po.Number & "</td>"
            htmlString &= "<td></td>"
            htmlString &= "<td></td>"
            htmlString &= "</tr>"

            'Second Row
            htmlString &= "<tr>"
            htmlString &= "<td width=""200px"">Owner:</td>"
            htmlString &= "<td width=""600px"">" & owner.Name & "</td>"
            htmlString &= "<td></td>"
            htmlString &= "<td></td>"
            htmlString &= "</tr>"

            'Third Row
            htmlString &= "<tr>"
            htmlString &= "<td width=""200px"">Account:</td>"
            htmlString &= "<td width=""600px"" colspan=3>" & supplier.Name & "</td>"
            htmlString &= "</tr>"

            ''Next Row
            'If carrierInfo.Name.Trim.Length > 0 Then
            '    htmlString &= "<tr>"
            '    htmlString &= "<td width=""200px"">Carrier:</td>"
            '    htmlString &= "<td width=""600px"" colspan=3>" & carrierInfo.Name & "</td>"
            '    htmlString &= "</tr>"
            'End If

            ''Next Row
            'If .CarrierNumber.Trim.Length > 0 Then
            '    htmlString &= "<tr>"
            '    htmlString &= "<td width=""200px"">Carrier Number:</td>"
            '    htmlString &= "<td width=""600px"" colspan=3>" & .CarrierNumber & "</td>"
            '    htmlString &= "</tr>"
            'End If

            ''Next Row
            'If driverInfo.Name.Trim.Length > 0 Then
            '    htmlString &= "<tr>"
            '    htmlString &= "<td width=""200px"">Driver:</td>"
            '    htmlString &= "<td width=""600px"" colspan=3>" & driverInfo.Name & "</td>"
            '    htmlString &= "</tr>"
            'End If

            ''Next Row
            'If .DriverNumber.Trim.Length > 0 Then
            '    htmlString &= "<tr>"
            '    htmlString &= "<td width=""200px"">Driver Number:</td>"
            '    htmlString &= "<td width=""600px"" colspan=3>" & .DriverNumber & "</td>"
            '    htmlString &= "</tr>"
            'End If

            ''Next Row
            'If transportInfo.Name.Trim.Length > 0 Then
            '    htmlString &= "<tr>"
            '    htmlString &= "<td width=""200px"">Transport:</td>"
            '    htmlString &= "<td width=""600px"" colspan=3>" & transportInfo.Name & "</td>"
            '    htmlString &= "</tr>"
            'End If

            ''Next Row
            'If .TruckNumber.Trim.Length > 0 Then
            '    htmlString &= "<tr>"
            '    htmlString &= "<td width=""200px"">Transport Number:</td>"
            '    htmlString &= "<td width=""600px"" colspan=3>" & .TruckNumber & "</td>"
            '    htmlString &= "</tr>"
            'End If

            'Next Row
            htmlString &= "<tr>"
            htmlString &= "<td width=""200px"">Primary Measurement:</td>"
            htmlString &= "<td width=""600px"">" & unit.Name & "</td>"
            htmlString &= "</tr>"

            'Next Row
            htmlString &= "<tr>"
            htmlString &= "<td width=""200px"">Product</td>"
            htmlString &= "<td width=""600px"">Amount (Delivered/Requested)</td>"
            htmlString &= "<td></td>"
            htmlString &= "<td></td>"
            htmlString &= "</tr>"

            'Next Row
            htmlString &= "<tr>"
            htmlString &= "<td width=""200px"">" & bulkProd.Name & "</td>"
            htmlString &= "<td width=""600px"">" & Format(po.Delivered, unit.UnitPrecision) & " / " & Format(po.Purchased, unit.UnitPrecision) & "</td>"
            htmlString &= "<td></td>"
            htmlString &= "<td></td>"
            htmlString &= "</tr>"

            htmlString &= "</table>"
            htmlString &= "<hr width= 100% color=""#99ccff"">"

            'List Receiving Tickets
            htmlString &= "<table border=""1"" width=""100%""><tr><td><strong>Date/time</strong></td><td><strong>Facility/time</strong></td><td><strong>Delivered</strong></td><td><strong>Carrier</strong></td><td><strong>Transport</strong></td><td><strong>Driver</strong></td></tr>"

            For Each r As KaReceivingTicket In KaReceivingTicket.GetAll(GetUserConnection(_currentUser.Id), "voided=0 AND deleted = " & Q(False) & " and receiving_purchase_order_id = " & Q(po.Id), "date_of_delivery asc")
                htmlString &= "<tr><td>" + r.DateOfDelivery + "</td><td>" + r.LocationName + "</td><td>" + Format(r.Delivered, unit.UnitPrecision) + " " + unit.Name + "</td><td>" + r.CarrierName + " (" + r.CarrierNumber + ")</td><td>" + r.TransportName + " (" + r.TransportNumber + ")</td><td>" + r.DriverName + " (" + r.DriverNumber + ")</td></tr>"
            Next

            htmlString &= "</table>"
            htmlString &= "<hr width= 100% color=""#99ccff"">"

            'Send string to the literal for display
            litOutput.Text = htmlString
        End If
    End Sub
End Class