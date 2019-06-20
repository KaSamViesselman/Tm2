Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class Transports : Inherits System.Web.UI.Page

    Private _currentUser As KaUser
    Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
    Private _customFields As New List(Of KaCustomField)
    Private _customFieldData As New List(Of KaCustomFieldData)

#Region " Web Form Designer Generated Code "

    'This call is required by the Web Form Designer.
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()

    End Sub


    Private Sub Page_Init(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Init
        'CODEGEN: This method call is required by the Web Form Designer
        'Do not modify it using the code editor.
        InitializeComponent()
    End Sub

#End Region

    Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load, Me.Load
        Response.Cache.SetCacheability(HttpCacheability.NoCache)
        _currentUser = Utilities.GetUser(Me)
        _currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaTransport.TABLE_NAME}), "Transports")
        If Not _currentUserPermission(KaTransport.TABLE_NAME).Read Then Response.Redirect("Welcome.aspx")
        lblStatus.Text = ""
        If Not Page.IsPostBack Then
            SetTextboxMaxLengths()
            _customFields.Clear()
            For Each customField As KaCustomField In KaCustomField.GetAll(GetUserConnection(_currentUser.Id), String.Format("deleted=0 AND {0} = {1}", KaCustomField.FN_TABLE_NAME, Q(KaTransport.TABLE_NAME)), KaCustomField.FN_FIELD_NAME)
                _customFields.Add(customField)
            Next
            PopulateTransportTypes()
            PopulateTransportsList()
            PopulateCarriersList()
            PopulateOrderList()
            PopulateTransportUnits()
            PopulateCompartmentUnits()
            PopulateLengthUnitsList()
            Dim transportId As Guid = Guid.Empty
            If Page.Request("TransportId") IsNot Nothing Then Guid.TryParse(Page.Request("TransportId"), transportId)
            Try
                ddlTransports.SelectedValue = transportId.ToString()
            Catch ex As ArgumentOutOfRangeException
                ddlTransports.SelectedIndex = 0
            End Try
            ddlTransports_SelectedIndexChanged(ddlTransports, New EventArgs())
            Utilities.ConfirmBox(Me.btnDelete, "Are you sure you want to delete this transport?")
            Utilities.SetFocus(tbxName, Me)
        End If
    End Sub

    Private Sub PopulateTransportTypes()
        ddlTransportType.Items.Clear()
        Dim li As ListItem = New ListItem
        li.Text = ""
        li.Value = Guid.Empty.ToString
        ddlTransportType.Items.Add(li)
        For Each transportType As KaTransportTypes In KaTransportTypes.GetAll(GetUserConnection(_currentUser.Id), "deleted = " & Q(False), "")
            li = New ListItem
            li.Text = transportType.Name
            li.Value = transportType.Id.ToString
            ddlTransportType.Items.Add(li)
        Next
    End Sub

    Private Sub PopulateOrderList()
        ddlCurrentOrder.Items.Clear()
        ddlCurrentOrder.Items.Add(New ListItem("", Guid.Empty.ToString()))
        For Each r As KaOrder In KaOrder.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 and completed=0" & IIf(_currentUser.OwnerId = Guid.Empty, "", " AND owner_id=" & Q(_currentUser.OwnerId)), "number ASC")
            ddlCurrentOrder.Items.Add(New ListItem(r.Number, r.Id.ToString()))
        Next
    End Sub

    Private Sub PopulateTransportsList()
        ddlTransports.Items.Clear()
        If _currentUserPermission(KaTransport.TABLE_NAME).Create Then
            ddlTransports.Items.Add(New ListItem("Enter a new transport", Guid.Empty.ToString))
        Else
            ddlTransports.Items.Add(New ListItem("Select a transport", Guid.Empty.ToString))
        End If
        For Each u As KaTransport In KaTransport.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
            ddlTransports.Items.Add(New ListItem(u.Name, u.Id.ToString))
        Next
        Page.ClientScript.RegisterStartupScript(Page.ClientScript.GetType(), "ResetScrollPosition;", "resetDotNetScrollPosition();", True)
    End Sub

    Private Sub PopulateCarriersList()
        ddlCarriers.Items.Clear()
        ddlCarriers.Items.Add(New ListItem("", Guid.Empty.ToString))
        For Each u As KaCarrier In KaCarrier.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
            ddlCarriers.Items.Add(New ListItem(u.Name, u.Id.ToString))
        Next
    End Sub

    Private Sub PopulateCompartmentsList(transport As KaTransport)
        Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
        ddlCompartments.Items.Clear()
        For Each compartment As KaTransportCompartment In transport.Compartments
            ddlCompartments.Items.Add(New ListItem(compartment.Position + 1, compartment.Id.ToString()))
        Next
        If ddlCompartments.Items.Count > 0 Then ddlCompartments.SelectedIndex = 0
        ddlCompartments_SelectedIndexChanged(Nothing, Nothing)
    End Sub

    Private Sub PopulateLengthUnitsList()
        ddlLengthUnit.Items.Clear()
        Dim li As New ListItem
        li.Text = "Feet"
        li.Value = "f"
        ddlLengthUnit.Items.Add(li)
        li = New ListItem
        li.Text = "Meters"
        li.Value = "m"
        ddlLengthUnit.Items.Add(li)
        ddlLengthUnit.SelectedIndex = 0
    End Sub

    Private Sub ddlTransports_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ddlTransports.SelectedIndexChanged
        _customFieldData.Clear()
        Dim transportId As Guid = Guid.Empty
        If ddlTransports.SelectedIndex >= 0 Then Guid.TryParse(ddlTransports.SelectedValue, transportId)
        Dim c As OleDbConnection = GetUserConnection(_currentUser.Id)
        Dim defaultUnitId As Guid = KaUnit.GetSystemDefaultMassUnitOfMeasure(c, Nothing)
        PopulateTransportData(transportId)
        If transportId <> Guid.Empty Then
            With _currentUserPermission(KaTransport.TABLE_NAME)
                btnDelete.Enabled = .Edit AndAlso .Delete
                btnAddCompartment.Enabled = .Edit
            End With
        Else
            btnDelete.Enabled = False
            btnAddCompartment.Enabled = False
            ddlCompartments_SelectedIndexChanged(ddlCompartments, New EventArgs())
        End If
        lstRfidNumber_SelectedIndexChanged(lstRfidNumber, New EventArgs)
        btnSave.Enabled = True
        PopulateTransportInterfaceList(Guid.Parse(ddlTransports.SelectedValue))
        SetControlUsabilityFromPermissions()
		Utilities.CreateDynamicCustomFieldPanelControls(_customFields, _customFieldData, lstCustomFields, Page)
		Utilities.SetFocus(tbxName, Me)
    End Sub

    Private Sub PopulateTransportData(ByVal transportId As Guid)
        Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
        Dim transport As KaTransport
        Try
            transport = New KaTransport(connection, transportId)
        Catch ex As RecordNotFoundException
            transport = New KaTransport
            transport.Id = transportId
        End Try
        With transport
            tbxName.Text = .Name
            tbxNumber.Text = .Number
            lstRfidNumber.Items.Clear()
            For Each currentRfidTag As KaTransportRfid In .RfidTags
                If Not currentRfidTag.Deleted Then lstRfidNumber.Items.Add(New ListItem(currentRfidTag.RfidNumber, currentRfidTag.Id.ToString))
            Next
            If lstRfidNumber.Items.Count > 0 Then lstRfidNumber.SelectedIndex = 0

            tbxEmptyWeight.Text = .TareWeight
            tbxMaxWeight.Text = .MaximumWeightStandard
            tbxTempOverweightAmount.Text = .TemporaryOverweightAmount
            If (.TemporaryOverweightExpirationDate > New DateTime(1900, 1, 1)) Then
                tbxTempOverweightExpirationDate.Value = String.Format("{0:g}", .TemporaryOverweightExpirationDate)
            Else
                tbxTempOverweightExpirationDate.Value = ""
            End If
            tbxLength.Text = .Length
            ddlLengthUnit.SelectedValue = IIf(.Metric, "m", "f")
            Try
                ddlTransportType.SelectedValue = .TransportTypeId.ToString
            Catch ex As ArgumentOutOfRangeException
                ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidTransportTypeId", Utilities.JsAlert("Transport Type not found where ID = " & .TransportTypeId.ToString & ". Transport Type not set."))
                ddlCarriers.SelectedIndex = 0
            End Try
            Try
                ddlCarriers.SelectedValue = .CarrierId.ToString
            Catch ex As ArgumentOutOfRangeException
                ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidCarrierId", Utilities.JsAlert("Carrier not found where ID = " & .CarrierId.ToString & ". Carrier not set."))
                ddlCarriers.SelectedIndex = 0
            End Try
            Try
                ddlCurrentOrder.SelectedValue = .OrderId.ToString()
            Catch ex As ArgumentOutOfRangeException
                ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidOrderId", Utilities.JsAlert("Order not found where ID = " & .OrderId.ToString() & ". Order not set."))
                ddlCurrentOrder.SelectedIndex = 0
            End Try
            Try
                ddlTransportUnitId.SelectedValue = .UnitId.ToString
            Catch ex As ArgumentOutOfRangeException
                ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidUnitId", Utilities.JsAlert("Unit not found where ID = " & .UnitId.ToString() & ". Unit not set."))
                ddlTransportUnitId.SelectedIndex = 0
            End Try
            PopulateCompartmentsList(transport)

            _customFieldData.Clear()
            For Each customFieldValue As KaCustomFieldData In KaCustomFieldData.GetAll(connection, String.Format("deleted = 0 AND {0} = {1}", KaCustomFieldData.FN_RECORD_ID, Q(.Id)), KaCustomFieldData.FN_LAST_UPDATED)
                _customFieldData.Add(customFieldValue)
            Next

            SetInFacilityStatus(connection, transportId)
        End With
    End Sub

    Private Sub SetInFacilityStatus(connection As OleDbConnection, transportId As Guid)
        Dim lastInFacilityRecord As KaTransportInFacility = Nothing
        Try
            For Each transportInFacilityRecord As KaTransportInFacility In KaTransportInFacility.GetAll(connection, String.Format("transport_id = {0} AND deleted = 0", Q(transportId)), "created desc, last_updated desc")
                lastInFacilityRecord = transportInFacilityRecord
                Exit For
            Next
        Catch ex As Exception

        End Try
        If lastInFacilityRecord Is Nothing Then
            pnlLastInFacilityInfo.Visible = False
        Else
            pnlLastInFacilityInfo.Visible = True
            If lastInFacilityRecord.EnteredAt > New DateTime(1900, 1, 1) Then
                lblLastEnteredFacility.Text = String.Format("{0:g}", lastInFacilityRecord.EnteredAt)
                liLastEnteredFacility.Visible = True
            Else
                liLastEnteredFacility.Visible = False
            End If
            If lastInFacilityRecord.ExitedAt > New DateTime(1900, 1, 1) Then
                lblLastExitedFacility.Text = String.Format("{0:g}", lastInFacilityRecord.ExitedAt)
                liLastExitedFacility.Visible = True
            Else
                liLastExitedFacility.Visible = False
            End If
            Try
                lblLastInFacilityLocation.Text = New KaLocation(connection, lastInFacilityRecord.LocationId).Name
            Catch ex As RecordNotFoundException
                lblLastInFacilityLocation.Text = "Not specified"
            End Try
            If lastInFacilityRecord.InFacility Then
                lblLastInFacilityStatus.Text = "In facility"
            Else
                lblLastInFacilityStatus.Text = "Not in facility"
            End If
            btnClearLastInFacilityStatus.Visible = lastInFacilityRecord.InFacility
        End If
    End Sub

    Private Sub PopulateTransportUnits()
        ddlTransportUnitId.Items.Clear()
        Dim units As ArrayList = KaUnit.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND base_unit<>9", "name ASC")
        units = KaUnit.FilterOutVolumeUnits(units)
		For Each u As KaUnit In units 'Units are for tare weight and maximum weight of the transport.  Does not make sense to allow volume units here, no place to pull density from.
			If Not KaUnit.IsTime(u.BaseUnit) Then ' transports can't be tared in or hold time
				ddlTransportUnitId.Items.Add(New ListItem(u.Name, u.Id.ToString))
			End If
		Next
		Try
			ddlTransportUnitId.SelectedValue = KaUnit.GetSystemDefaultMassUnitOfMeasure(GetUserConnection(_currentUser.Id), Nothing).ToString()
		Catch ex As ArgumentOutOfRangeException

		End Try
	End Sub

    Private Sub PopulateCompartmentUnits()
        ddlCompartmnetUnitId.Items.Clear()
		For Each u As KaUnit In KaUnit.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND base_unit<>9", "name ASC")
			ddlCompartmnetUnitId.Items.Add(New ListItem(u.Name, u.Id.ToString))
		Next
		Try
			ddlCompartmnetUnitId.SelectedValue = KaUnit.GetSystemDefaultMassUnitOfMeasure(GetUserConnection(_currentUser.Id), Nothing).ToString()
		Catch ex As ArgumentOutOfRangeException

		End Try
	End Sub

    Private Sub ClearCompartmentData()
        ddlCompartments.Items.Clear()
        ddlCompartments_SelectedIndexChanged(Nothing, Nothing)
    End Sub

    Private Sub btnSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSave.Click
        If ValidateFields() Then
            SaveTransport()
            btnAddCompartment.Enabled = True
        End If
    End Sub

    Private Function ValidateFields() As Boolean
        If tbxName.Text.Trim().Length = 0 Then
            ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidName", Utilities.JsAlert("Please specify a name for the transport."))
            Return False
        End If
        If Guid.Parse(ddlTransportUnitId.SelectedValue) = Guid.Empty Then
            ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidUnit", Utilities.JsAlert("Please select a unit of measure for the transport."))
            Return False
        End If
        Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
        Dim allTransports As ArrayList = KaTransport.GetAll(connection, "deleted = 0 AND name=" & Q(tbxName.Text), "")
        If allTransports.Count > 0 Then
            If CType(allTransports.Item(0), KaTransport).Id <> Guid.Parse(ddlTransports.SelectedValue) Then
                ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidDuplicateName", Utilities.JsAlert("A transport with name """ & tbxName.Text.Trim & """ already exists. Please enter a unique name for the transport.")) : Return False
            End If
        End If
        If tbxNumber.Text.Length > 0 Then
            allTransports = KaTransport.GetAll(connection, "deleted = 0 AND number=" & Q(tbxNumber.Text), "")
            If allTransports.Count > 0 Then
                If CType(allTransports.Item(0), KaTransport).Id <> Guid.Parse(ddlTransports.SelectedValue) Then
                    ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidDuplicateNumber", Utilities.JsAlert("A transport with number """ & tbxName.Text.Trim & """ already exists. Please enter a unique number for the transport.")) : Return False
                End If
            End If
        End If
        Dim numberValidation As Double
        If Not Double.TryParse(tbxEmptyWeight.Text.Trim, numberValidation) Then
            ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidEmptyWeight", Utilities.JsAlert("Please enter a numeric value for transport empty weight."))
            Return False
        End If
        If Not Double.TryParse(tbxMaxWeight.Text.Trim, numberValidation) Then
            ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidMaxWeight", Utilities.JsAlert("Please enter a numeric value for transport max weight."))
            Return False
        End If
        If Not Double.TryParse(tbxLength.Text, numberValidation) Then
            ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidLength", Utilities.JsAlert("Please enter a numeric value greater than zero for the length of the transport."))
            Return False
        End If
        If tbxTempOverweightAmount.Text.Trim.Length > 0 AndAlso Not Double.TryParse(tbxTempOverweightAmount.Text, numberValidation) Then
            ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidTemporaryEmptyWeight", Utilities.JsAlert("Please enter a numeric value for temporary transport overweight."))
            Return False
        End If
        Dim dateValidation As DateTime
        If tbxTempOverweightExpirationDate.Value.Trim.Length > 0 AndAlso Not DateTime.TryParse(tbxTempOverweightExpirationDate.Value, dateValidation) Then
            ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidTemporaryEmptyDate", Utilities.JsAlert("Please enter a valid date for temporary transport overweight expiration."))
            Return False
        End If
        Return True
    End Function

    Private Sub SaveTransport()
        Dim transportInfo As KaTransport
        Dim transportId As Guid = Guid.Empty
        Guid.TryParse(ddlTransports.SelectedValue, transportId)
        Try
            transportInfo = New KaTransport(GetUserConnection(_currentUser.Id), transportId)
        Catch ex As RecordNotFoundException
            transportInfo = New KaTransport()
        End Try
        With transportInfo
            .Id = transportId
            .Name = tbxName.Text
            .Number = tbxNumber.Text
            For Each currentRfidTag As KaTransportRfid In .RfidTags
                currentRfidTag.Deleted = True
            Next
            For Each newTag As ListItem In lstRfidNumber.Items
                Dim tagFound As Boolean = False
                For Each currentRfidTag As KaTransportRfid In .RfidTags
                    If currentRfidTag.Id.ToString = newTag.Value Then
                        tagFound = True
                        currentRfidTag.RfidNumber = newTag.Text
                        currentRfidTag.Deleted = False
                        Exit For
                    End If
                Next
                If Not tagFound Then
                    Dim newRfidTag As New KaTransportRfid
                    newRfidTag.TransportId = .Id
                    newRfidTag.RfidNumber = newTag.Text
                    .RfidTags.Add(newRfidTag)
                End If
            Next
            .TareWeight = Convert.ToDouble(tbxEmptyWeight.Text)
            .MaximumWeightStandard = Convert.ToDouble(tbxMaxWeight.Text)
            Dim temporaryOverweightAmount As Double
            If Not Double.TryParse(tbxTempOverweightAmount.Text, temporaryOverweightAmount) Then
                tbxTempOverweightAmount.Text = .TemporaryOverweightAmount
            Else
                .TemporaryOverweightAmount = temporaryOverweightAmount
            End If
            Dim temporaryOverweightExpirationDate As DateTime
            If Not DateTime.TryParse(tbxTempOverweightExpirationDate.Value, temporaryOverweightExpirationDate) Then
                If (.TemporaryOverweightExpirationDate > New DateTime(1900, 1, 1)) Then
                    tbxTempOverweightExpirationDate.Value = String.Format("{0:g}", .TemporaryOverweightExpirationDate)
                Else
                    tbxTempOverweightExpirationDate.Value = ""
                End If
            Else
                .TemporaryOverweightExpirationDate = temporaryOverweightExpirationDate
            End If
            .Length = Convert.ToDouble(tbxLength.Text)
            .Metric = IIf(ddlLengthUnit.SelectedValue = "m", True, False)
            .TransportTypeId = Guid.Parse(ddlTransportType.SelectedValue)
            .UnitId = Guid.Parse(ddlTransportUnitId.SelectedValue)
            .CarrierId = New Guid(ddlCarriers.SelectedValue)
            .OrderId = Guid.Parse(ddlCurrentOrder.SelectedValue)

            'check if we should update the Tare Manual and Tare Date fields
            Dim originalTareWeight As Double = 0.0
            Dim originalTareUnit As Guid = .UnitId
            Try
                Dim oldTransportInfo As New KaTransport(GetUserConnection(_currentUser.Id), .Id)
                originalTareWeight = oldTransportInfo.TareWeight
                originalTareUnit = oldTransportInfo.UnitId
            Catch ex As RecordNotFoundException

            End Try
            If originalTareWeight <> .TareWeight OrElse (.TareWeight <> 0.0 AndAlso Not originalTareUnit.Equals(.UnitId)) Then
                ' They have changed one of the 2 variables
                .TareManual = True
                .TaredAt = DateTime.Now
            End If
            Dim status As String = ""
            If .Id = Guid.Empty Then
                .SqlInsert(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                status = "New transport added successfully"
            Else
                .SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                status = "Selected transport updated successfully"
            End If

            Utilities.ConvertCustomFieldPanelToLists(_customFields, _customFieldData, lstCustomFields)
            For Each customData As KaCustomFieldData In _customFieldData
                customData.RecordId = .Id
                customData.SqlUpdateInsertIfNotFound(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
            Next

            PopulateTransportsList()
            Dim carrierId As String = ddlCarriers.SelectedValue.ToString
            PopulateCarriersList()
            ddlCarriers.SelectedValue = carrierId
            ddlTransports.SelectedValue = .Id.ToString()
            ddlTransports_SelectedIndexChanged(ddlTransports, New EventArgs())
            lblStatus.Text = status
        End With
    End Sub

    Protected Sub btnDelete_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnDelete.Click
        If Guid.Parse(ddlTransports.SelectedValue) <> Guid.Empty Then
            With New KaTransport(GetUserConnection(_currentUser.Id), Guid.Parse(ddlTransports.SelectedValue))
                .Deleted = True
                .SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                PopulateTransportsList()
                PopulateCarriersList()
                ddlTransports.SelectedIndex = 0
                ddlTransports_SelectedIndexChanged(ddlTransports, New EventArgs())
                lblStatus.Text = "Selected Transport deleted successfully"
            End With
        End If
    End Sub

    Protected Sub btnAddCompartment_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnAddCompartment.Click
        Dim compartment As New KaTransportCompartment()
        compartment.Position = ddlCompartments.Items.Count
        compartment.TransportId = Guid.Parse(ddlTransports.SelectedValue)
        compartment.UnitId = Guid.Parse(ddlTransportUnitId.SelectedValue)
        Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
        compartment.SqlInsert(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
        ddlCompartments.Items.Add(New ListItem(compartment.Position + 1, compartment.Id.ToString()))
        ddlCompartments.SelectedIndex = ddlCompartments.Items.Count - 1
        ddlCompartments_SelectedIndexChanged(Nothing, Nothing)
        lblStatus.Text = "Compartment added to transport"
    End Sub

    Protected Sub ddlCompartments_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles ddlCompartments.SelectedIndexChanged
        If ddlCompartments.SelectedIndex <> -1 Then
            Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
            Dim compartment As New KaTransportCompartment(connection, Guid.Parse(ddlCompartments.SelectedValue))
            tbxCompartmentCapacity.Text = compartment.Capacity
            Try
                ddlCompartmnetUnitId.SelectedValue = compartment.UnitId.ToString()
            Catch ex As ArgumentOutOfRangeException
                ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidUnit", Utilities.JsAlert("Unit not found for compartment where ID = " & compartment.UnitId.ToString() & ". Unit not set."))
                ddlCompartmnetUnitId.SelectedIndex = 0
            End Try

            With _currentUserPermission(KaTransport.TABLE_NAME)
                tbxCompartmentCapacity.Enabled = .Edit
                ddlCompartmnetUnitId.Enabled = .Edit
                btnSaveCompartment.Enabled = .Edit
                btnDeleteCompartment.Enabled = .Edit
            End With
        Else
            tbxCompartmentCapacity.Text = "0"
            ddlCompartmnetUnitId.SelectedIndex = 0
            tbxCompartmentCapacity.Enabled = False
            ddlCompartmnetUnitId.Enabled = False
            btnSaveCompartment.Enabled = False
            btnDeleteCompartment.Enabled = False
        End If
    End Sub

    Protected Sub btnSaveCompartment_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnSaveCompartment.Click
        If Not IsNumeric(tbxCompartmentCapacity.Text.Trim) Then
            ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidCapacity", Utilities.JsAlert("Please enter a numeric value for the compartment capacity."))
            Exit Sub
        End If
        Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
        Dim compartment As New KaTransportCompartment(connection, Guid.Parse(ddlCompartments.SelectedValue))
        compartment.Capacity = Double.Parse(tbxCompartmentCapacity.Text)
        compartment.UnitId = Guid.Parse(ddlCompartmnetUnitId.SelectedValue)
        compartment.SqlUpdate(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
        lblStatus.Text = "Compartment information saved successfully"
    End Sub

    Protected Sub btnDeleteCompartment_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnDeleteCompartment.Click
        Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
        Dim compartment As New KaTransportCompartment(connection, Guid.Parse(ddlCompartments.SelectedValue))
        compartment.Deleted = True
        compartment.SqlUpdate(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
        ddlCompartments.Items.RemoveAt(ddlCompartments.SelectedIndex)
        For i As Integer = 0 To ddlCompartments.Items.Count - 1
            compartment = New KaTransportCompartment(connection, Guid.Parse(ddlCompartments.Items(i).Value))
            compartment.Position = i
            compartment.SqlUpdate(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
            ddlCompartments.Items(i).Text = (i + 1).ToString()
        Next
        If ddlCompartments.Items.Count > 0 Then ddlCompartments.SelectedIndex = 0
        ddlCompartments_SelectedIndexChanged(Nothing, Nothing)
        lblStatus.Text = "Compartment removed from transport"
    End Sub

    Private Sub lstRfidNumber_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles lstRfidNumber.SelectedIndexChanged
        If lstRfidNumber.SelectedIndex >= 0 Then
            btnRemoveRfidTag.Visible = True
            btnSetRfidTag.Visible = True
            tbxRfidNumber.Text = lstRfidNumber.SelectedItem.Text
        Else
            btnRemoveRfidTag.Visible = False
            btnSetRfidTag.Visible = False
            tbxRfidNumber.Text = ""
        End If
    End Sub

    Private Sub btnSetRfidTag_Click(sender As Object, e As System.EventArgs) Handles btnSetRfidTag.Click
        If tbxRfidNumber.Text.Trim.Length = 0 Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidTagId", Utilities.JsAlert("The RFID Tag Id cannot be empty.")) : Exit Sub
        lstRfidNumber.SelectedItem.Text = tbxRfidNumber.Text.Trim
    End Sub

    Private Sub btnAddRfidTag_Click(sender As Object, e As System.EventArgs) Handles btnAddRfidTag.Click
        If tbxRfidNumber.Text.Trim.Length = 0 Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidTagId", Utilities.JsAlert("The RFID Tag Id cannot be empty.")) : Exit Sub
        lstRfidNumber.Items.Add(New ListItem(tbxRfidNumber.Text.Trim, "-1"))
    End Sub

    Private Sub btnRemoveRfidTag_Click(sender As Object, e As System.EventArgs) Handles btnRemoveRfidTag.Click
        If lstRfidNumber.SelectedIndex >= 0 Then lstRfidNumber.Items.RemoveAt(lstRfidNumber.SelectedIndex)
    End Sub

    Private Sub SetTextboxMaxLengths()
        tbxCompartmentCapacity.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaTransportCompartment.TABLE_NAME, "capacity"))
        tbxEmptyWeight.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaTransport.TABLE_NAME, "tare_weight"))
        tbxLength.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaTransport.TABLE_NAME, "length"))
        tbxMaxWeight.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaTransport.TABLE_NAME, "maximum_weight"))
        tbxName.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaTransport.TABLE_NAME, "name"))
        tbxNumber.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaTransport.TABLE_NAME, "number"))
        tbxRfidNumber.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaTransportRfid.TABLE_NAME, "rfid_number"))
    End Sub

#Region "Interfaces"
    Private Sub PopulateInterfaceList()
        ddlInterface.Items.Clear()
        ddlInterface.Items.Add(New ListItem("", Guid.Empty.ToString()))
        Dim getInterfaceRdr As OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), "SELECT interfaces.id, interfaces.name " &
                "FROM interfaces " &
                "INNER JOIN interface_types ON interfaces.interface_type_id = interface_types.id " &
                "WHERE (interfaces.deleted = 0) " &
                    "AND (interface_types.deleted = 0) " &
                    "AND ((" & KaInterfaceTypes.FN_SHOW_TRANSPORT_INTERFACE & " = 1) " &
                    "OR (interfaces.id IN (SELECT " & KaTransportInterfaceSettings.TABLE_NAME & ".interface_id " &
                                            "FROM " & KaTransportInterfaceSettings.TABLE_NAME & " " &
                                            "WHERE (deleted=0) " &
                                                "AND (" & KaTransportInterfaceSettings.TABLE_NAME & "." & KaTransportInterfaceSettings.FN_TRANSPORT_ID & " = " & Q(ddlTransports.SelectedValue) & ") " &
                                            "AND (" & KaTransportInterfaceSettings.TABLE_NAME & "." & KaTransportInterfaceSettings.FN_TRANSPORT_ID & " <> " & Q(Guid.Empty) & ")))) " &
                "ORDER BY interfaces.name")
        Do While getInterfaceRdr.Read
            ddlInterface.Items.Add(New ListItem(getInterfaceRdr.Item("name"), getInterfaceRdr.Item("id").ToString()))
        Loop
        getInterfaceRdr.Close()
        pnlInterfaceSetup.Visible = (ddlInterface.Items.Count > 1) AndAlso ddlTransports.SelectedValue <> Guid.Empty.ToString 'TODO: is this an error? should the > 1 be a > 0
    End Sub

    Private Sub SaveInterface()
        If Guid.Parse(ddlTransports.SelectedValue) = Guid.Empty Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidNotSaved", Utilities.JsAlert("You must save the transport before you can set up interface cross references.")) : Exit Sub
        If Guid.Parse(ddlInterface.SelectedValue) = Guid.Empty Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidInterface", Utilities.JsAlert("An interface must be selected. Interface settings not saved.")) : Exit Sub
        If tbxInterfaceCrossReference.Text.Length = 0 Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidCrossReference", Utilities.JsAlert("A cross reference must be specified. Interface settings not saved.")) : Exit Sub

        ' If this is not export only, check if there are any other settings with the same cross reference ID
        If Not chkExportOnly.Checked Then
            Dim allInterfaceSettings As ArrayList = KaTransportInterfaceSettings.GetAll(GetUserConnection(_currentUser.Id), KaTransportInterfaceSettings.FN_CROSS_REFERENCE & " = " & Q(tbxInterfaceCrossReference.Text.Trim) &
                                                                                            " AND " & KaTransportInterfaceSettings.FN_INTERFACE_ID & " = " & Q(ddlInterface.SelectedValue) &
                                                                                            " AND " & KaTransportInterfaceSettings.FN_DELETED & " = 0 " &
                                                                                            " AND " & KaTransportInterfaceSettings.FN_EXPORT_ONLY & " = 0 " &
                                                                                            " AND " & KaTransportInterfaceSettings.FN_ID & " <> " & Q(ddlTransportInterface.SelectedValue), "")
            If allInterfaceSettings.Count > 0 Then
                ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidCrossReferenceExists", Utilities.JsAlert("A cross reference of " & tbxInterfaceCrossReference.Text.Trim & " already exists for this interface."))
                Exit Sub
            End If
        End If

        Dim transportInterfaceId As Guid = Guid.Parse(ddlTransportInterface.SelectedValue)
        If transportInterfaceId = Guid.Empty Then
            Dim transportInterface As KaTransportInterfaceSettings = New KaTransportInterfaceSettings
            transportInterface.TransportId = Guid.Parse(ddlTransports.SelectedValue)
            transportInterface.InterfaceId = Guid.Parse(ddlInterface.SelectedValue)
            transportInterface.CrossReference = tbxInterfaceCrossReference.Text.Trim
            transportInterface.DefaultSetting = chkDefaultSetting.Checked
            transportInterface.ExportOnly = chkExportOnly.Checked
            transportInterface.SqlInsert(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
            transportInterfaceId = transportInterface.Id
        Else
            Dim transportInterface As KaTransportInterfaceSettings = New KaTransportInterfaceSettings(GetUserConnection(_currentUser.Id), transportInterfaceId)
            transportInterface.TransportId = Guid.Parse(ddlTransports.SelectedValue)
            transportInterface.InterfaceId = Guid.Parse(ddlInterface.SelectedValue)
            transportInterface.CrossReference = tbxInterfaceCrossReference.Text.Trim
            transportInterface.DefaultSetting = chkDefaultSetting.Checked
            transportInterface.ExportOnly = chkExportOnly.Checked
            transportInterface.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
        End If

        PopulateTransportInterfaceList(Guid.Parse(ddlTransports.SelectedValue))
        ddlTransportInterface.SelectedValue = transportInterfaceId.ToString
        ddltransportInterface_SelectedIndexChanged(ddlInterface, New EventArgs)
        btnRemoveInterface.Enabled = True
    End Sub

    Private Sub RemoveInterface()
        Dim selectedId As Guid = Guid.Parse(ddlTransportInterface.SelectedValue)
        If selectedId <> Guid.Empty Then
            Dim prodInterfaceSetting As KaTransportInterfaceSettings = New KaTransportInterfaceSettings(GetUserConnection(_currentUser.Id), selectedId)
            prodInterfaceSetting.Deleted = True
            prodInterfaceSetting.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
        End If
        PopulateTransportInterfaceList(Guid.Parse(ddlTransports.SelectedValue))
        btnRemoveInterface.Enabled = PopulateInterfaceInformation(Guid.Empty)
    End Sub

    Private Sub DeleteInterfaceInformation(ByVal transportId As Guid)
        For Each r As KaTransportInterfaceSettings In KaTransportInterfaceSettings.GetAll(GetUserConnection(_currentUser.Id), KaTransportInterfaceSettings.FN_DELETED & " = 0 and " & KaTransportInterfaceSettings.FN_TRANSPORT_ID & " = " & Q(transportId), "")
            r.Deleted = True
            r.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
        Next
    End Sub

    Private Sub PopulateTransportInterfaceList(ByVal transportId As Guid)
        PopulateInterfaceList()
        ddlTransportInterface.Items.Clear()
        ddlTransportInterface.Items.Add(New ListItem(IIf(ddlInterface.Items.Count > 1, "Add an interface", ""), Guid.Empty.ToString))
        Dim getInterfaceRdr As OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), "SELECT " & KaTransportInterfaceSettings.TABLE_NAME & ".id, interfaces.name, " & KaTransportInterfaceSettings.TABLE_NAME & ".cross_reference " &
                "FROM " & KaTransportInterfaceSettings.TABLE_NAME & " " &
                "INNER JOIN interfaces ON " & KaTransportInterfaceSettings.TABLE_NAME & ".interface_id = interfaces.id " &
                "INNER JOIN interface_types ON interfaces.interface_type_id = interface_types.id " &
                "WHERE (" & KaTransportInterfaceSettings.TABLE_NAME & ".deleted = 0) " &
                    "AND (interfaces.deleted = 0) " &
                    "AND (interface_types.deleted = 0) " &
                    "AND (" & KaTransportInterfaceSettings.TABLE_NAME & "." & KaTransportInterfaceSettings.FN_TRANSPORT_ID & "=" & Q(transportId) & ") " &
                "ORDER BY interfaces.name, " & KaTransportInterfaceSettings.TABLE_NAME & ".cross_reference")
        Do While getInterfaceRdr.Read
            ddlTransportInterface.Items.Add(New ListItem(getInterfaceRdr.Item("name") & " (" & getInterfaceRdr.Item("cross_reference") & ")", getInterfaceRdr.Item("id").ToString()))
        Loop
        getInterfaceRdr.Close()
    End Sub

    Private Function PopulateInterfaceInformation(ByVal transportInterfaceId As Guid) As Boolean
        Dim retval As Boolean = False
        If transportInterfaceId <> Guid.Empty Then
            Dim transportInterfaceSetting As KaTransportInterfaceSettings = New KaTransportInterfaceSettings(GetUserConnection(_currentUser.Id), transportInterfaceId)
            ddlInterface.SelectedValue = transportInterfaceSetting.InterfaceId.ToString
            tbxInterfaceCrossReference.Text = transportInterfaceSetting.CrossReference
            chkDefaultSetting.Checked = transportInterfaceSetting.DefaultSetting
            chkExportOnly.Checked = transportInterfaceSetting.ExportOnly
            retval = True
        Else
            ddlInterface.SelectedIndex = 0
            tbxInterfaceCrossReference.Text = ""
            ddlInterface_SelectedIndexChanged(ddlInterface, New EventArgs)
            chkExportOnly.Checked = False
            retval = False
        End If

        Return retval
    End Function

    Protected Sub btnRemoveInterface_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnRemoveInterface.Click
        RemoveInterface()
    End Sub

    Protected Sub btnSaveInterface_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnSaveInterface.Click
        SaveInterface()
    End Sub

    Protected Sub ddltransportInterface_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles ddlTransportInterface.SelectedIndexChanged
        btnRemoveInterface.Enabled = PopulateInterfaceInformation(Guid.Parse(ddlTransportInterface.SelectedValue))
    End Sub

    Private Sub ddlInterface_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles ddlInterface.SelectedIndexChanged
        If Guid.Parse(ddlTransportInterface.SelectedValue) = Guid.Empty Then
            'Only do this check if we are a new interface setting
            Dim count As Integer = 0
            Try
                Dim rdr As OleDb.OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), "SELECT count(*) " &
                                                                             "FROM " & KaTransportInterfaceSettings.TABLE_NAME & " " &
                                                                             "WHERE " & KaTransportInterfaceSettings.FN_DELETED & " = 0 " &
                                                                             "AND " & KaTransportInterfaceSettings.FN_INTERFACE_ID & " = " & Q(Guid.Parse(ddlInterface.SelectedValue)) & " " &
                                                                             "AND " & KaTransportInterfaceSettings.FN_TRANSPORT_ID & " = " & Q(Guid.Parse(ddlTransports.SelectedValue)))
                If rdr.Read Then count = rdr.Item(0)
            Catch ex As Exception

            End Try
            chkDefaultSetting.Checked = (count = 0)
        End If
    End Sub
#End Region

    Protected Overrides Function SaveViewState() As Object
        Dim viewState(2) As Object

        Utilities.ConvertCustomFieldPanelToLists(_customFields, _customFieldData, lstCustomFields)
        viewState(0) = MyBase.SaveViewState()
        viewState(1) = _customFields
        viewState(2) = _customFieldData
        Return viewState
    End Function

    Protected Overrides Sub LoadViewState(savedState As Object)
        If savedState IsNot Nothing AndAlso CType(savedState, Object).Length = 3 Then
            Dim viewState As Object() = savedState
            MyBase.LoadViewState(viewState(0))
            _customFields = viewState(1)
            _customFieldData = viewState(2)
			Utilities.CreateDynamicCustomFieldPanelControls(_customFields, _customFieldData, lstCustomFields, Page)
		Else
            MyBase.LoadViewState(savedState)
        End If
    End Sub

    Private Sub SetControlUsabilityFromPermissions()
        With _currentUserPermission(KaTransport.TABLE_NAME)
            Dim shouldEnable = (.Edit AndAlso ddlTransports.SelectedIndex > 0) OrElse (.Create AndAlso ddlTransports.SelectedIndex = 0)
            pnlEven.Enabled = shouldEnable
            pnlInterfaceSettings.Enabled = shouldEnable
            btnSave.Enabled = shouldEnable
        End With
    End Sub

    Protected Sub btnClearLastInFacilityStatus_Click(sender As Object, e As EventArgs) Handles btnClearLastInFacilityStatus.Click
        Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
        Dim transportId As Guid = Guid.Parse(ddlTransports.SelectedValue)
        Tm2Database.ExecuteNonQuery(connection, String.Format("UPDATE transports_in_facility SET in_facility = 0 WHERE transport_id= {0} and in_facility = 1 and deleted = 0", Q(transportId)))

        SetInFacilityStatus(connection, transportId)
    End Sub

    Protected Sub ddlTransportUnitId_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlTransportUnitId.SelectedIndexChanged
        Dim unitAbbr As String = ""
        Try
            unitAbbr = New KaUnit(GetUserConnection(_currentUser.Id), Guid.Parse(ddlTransportUnitId.SelectedValue)).Abbreviation
        Catch ex As RecordNotFoundException

        End Try
        lblEmptyWeightUnit.Text = unitAbbr
        lblMaxWeightUnit.Text = unitAbbr
        lblTempOverweightAmountUnit.Text = unitAbbr
    End Sub
End Class