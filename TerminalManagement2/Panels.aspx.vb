Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class Panels
	Inherits System.Web.UI.Page

	Private _currentUser As KaUser
	Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
	Private _currentTableName As String = KaPanel.TABLE_NAME
	Private _customFields As New List(Of KaCustomField)
	Private _customFieldData As New List(Of KaCustomFieldData)

#Region "Events"
	Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
		Response.Cache.SetCacheability(HttpCacheability.NoCache)
		_currentUser = Utilities.GetUser(Me)
		_currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Panels")
		If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")
		lblStatus.Text = ""
		If Not Page.IsPostBack Then
			SetTextboxMaxLengths()
			_customFields.Clear()
			For Each customField As KaCustomField In KaCustomField.GetAll(GetUserConnection(_currentUser.Id), String.Format("deleted=0 AND {0} = {1}", KaCustomField.FN_TABLE_NAME, Q(KaPanel.TABLE_NAME)), KaCustomField.FN_FIELD_NAME)
				_customFields.Add(customField)
			Next
			PopulateBaudRateList()
			PopulateConnectionTypeList()
			PopulateConnectionTypeList()
			PopulateDataBitsList()
			PopulateLocationList()
			Try
				ddlFacilityFilter.SelectedValue = KaSetting.GetSetting(GetUserConnection(_currentUser.Id), "PanelsPage:" & _currentUser.Id.ToString & "/LastFacilityUsed", Guid.Empty.ToString())
				ddlFacilityFilter_SelectedIndexChanged(ddlFacilityFilter, New EventArgs())
			Catch ex As ArgumentOutOfRangeException
				ddlFacilityFilter.SelectedValue = Guid.Empty.ToString()
			End Try

			PopulatePanelList()
			PopulateParityList()
			PopulateSerialPortList()
			PopulateStopBitsList()
			PopulateUnitLists()
			PopulateRoleList()
			PopulateBaseUnitLists()
			SetControlUsabilityFromPermissions()
			If Page.Request("PanelId") IsNot Nothing Then
				Try
					ddlPanels.SelectedValue = Page.Request("PanelId")
				Catch ex As ArgumentOutOfRangeException
					ddlPanels.SelectedIndex = 0
				End Try
			End If
			ddlPanels_SelectedIndexChanged(ddlPanels, New EventArgs())
			Utilities.ConfirmBox(Me.btnDelete, "Are you sure you want to delete this panel?") ' Delete confirmation box setup
			Utilities.SetFocus(tbxName, Me) ' set focus to the first textbox on the page
		End If
	End Sub

	Protected Sub ddlPanels_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles ddlPanels.SelectedIndexChanged
		btnDelete.Enabled = PopulatePanelInformation(Guid.Parse(ddlPanels.SelectedValue)) AndAlso _currentUserPermission(_currentTableName).Delete
		SetControlUsabilityFromPermissions()
	End Sub

	Protected Sub ddlConnectionType_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles ddlConnectionType.SelectedIndexChanged
		UpdateConnectionTypeSettings()
	End Sub

	Protected Sub btnSave_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnSave.Click
		Dim panelId As Guid = Guid.Parse(ddlPanels.SelectedValue)
		If SavePanelInformation(panelId) Then
			PopulatePanelList()
			ddlPanels.SelectedValue = panelId.ToString()
			ddlPanels_SelectedIndexChanged(ddlPanels, New EventArgs())
		End If
	End Sub

	Protected Sub btnDelete_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnDelete.Click
		If DeletePanelInformation(Guid.Parse(ddlPanels.SelectedValue)) Then
			PopulatePanelList()
			ddlPanels.SelectedValue = Guid.Empty.ToString()
			btnDelete.Enabled = PopulatePanelInformation(Guid.Empty) AndAlso _currentUserPermission(_currentTableName).Delete
			lblStatus.Text = "Panel successfully deleted."
			ddlPanels_SelectedIndexChanged(Nothing, Nothing)
		End If
	End Sub

	Protected Sub lbtConfigure_Click(ByVal sender As Object, ByVal e As EventArgs) Handles lbtConfigure.Click
		ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), "OpenKa2000Configure", Utilities.JsWindowOpen("http://" & tbxIpAddress.Text), False)
	End Sub

	Protected Sub ddlRole_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlRole.SelectedIndexChanged
		UpdateRoleSettings()
	End Sub
#End Region

	Private Sub UpdateConnectionTypeSettings()
		pnlTcpConnection.Visible = (ddlConnectionType.SelectedValue = KaPanel.PanelConnectionType.Ethernet) OrElse (ddlConnectionType.SelectedValue = KaPanel.PanelConnectionType.ModbusTcp)
		pnlSerialConnection.Visible = ddlConnectionType.SelectedValue = KaPanel.PanelConnectionType.SerialPort
		pnlEmulation.Visible = ddlConnectionType.SelectedValue = KaPanel.PanelConnectionType.Emulate
		If ddlPanels.SelectedIndex = 0 Then ' user is creating a new panel
			If ddlConnectionType.SelectedValue = KaPanel.PanelConnectionType.Ethernet Then
				tbxTcpPort.Text = "2000"
			ElseIf ddlConnectionType.SelectedValue = KaPanel.PanelConnectionType.ModbusTcp Then
				tbxTcpPort.Text = "502"
			End If
		End If
	End Sub

	Private Sub UpdateRoleSettings()
		If ddlRole.SelectedIndex >= 0 Then
			Dim role As KaPanel.PanelRole = Integer.Parse(ddlRole.SelectedValue)
			pnlPsSettings.Visible = role <> KaPanel.PanelRole.TLM4 AndAlso role <> KaPanel.PanelRole.TLM5
			If role = KaPanel.PanelRole.TLM5 Then
				ddlConnectionType.SelectedValue = KaPanel.PanelConnectionType.ModbusTcp
				UpdateConnectionTypeSettings()
				lblSystemAddress.InnerText = "Register base address"
			Else
				lblSystemAddress.InnerText = "System address"
			End If
		End If
	End Sub

	Private Sub PopulatePanelList()
		Dim currentPanelId As String = Guid.Empty.ToString()
		If ddlPanels.SelectedIndex >= 0 Then currentPanelId = ddlPanels.SelectedValue
		ddlPanels.Items.Clear()
		If _currentUserPermission(_currentTableName).Create Then
			ddlPanels.Items.Add(New ListItem("Enter a new panel", Guid.Empty.ToString()))
		Else
			ddlPanels.Items.Add(New ListItem("Select a new panel", Guid.Empty.ToString()))
		End If
		ddlPanels.SelectedIndex = 0
		Dim facilityId As Guid = Guid.Empty
		Guid.TryParse(ddlFacilityFilter.SelectedValue, facilityId)
		For Each r As KaPanel In KaPanel.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
			If facilityId.Equals(Guid.Empty) OrElse facilityId.Equals(r.LocationId) Then ddlPanels.Items.Add(New ListItem(r.Name, r.Id.ToString()))
		Next
		Try
			ddlPanels.SelectedValue = currentPanelId
		Catch ex As ArgumentOutOfRangeException

		End Try
		ddlPanels_SelectedIndexChanged(ddlPanels, New EventArgs)
	End Sub

	Private Sub PopulateLocationList()
		Dim facilityId As Guid = Guid.Empty
		Guid.TryParse(ddlFacilityFilter.SelectedValue, facilityId)
		ddlLocation.Items.Clear()
		ddlFacilityFilter.Items.Clear()
		ddlLocation.Items.Add(New ListItem("", Guid.Empty.ToString()))
		ddlFacilityFilter.Items.Add(New ListItem("All facilities", Guid.Empty.ToString()))
		For Each locationInfo As KaLocation In KaLocation.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
			If facilityId.Equals(Guid.Empty) OrElse facilityId.Equals(locationInfo.Id) Then ddlLocation.Items.Add(New ListItem(locationInfo.Name, locationInfo.Id.ToString()))
			ddlFacilityFilter.Items.Add(New ListItem(locationInfo.Name, locationInfo.Id.ToString))
		Next
		Try
			ddlFacilityFilter.SelectedValue = facilityId.ToString()
		Catch ex As ArgumentOutOfRangeException

		End Try
	End Sub

	Private Sub PopulateConnectionTypeList()
		ddlConnectionType.Items.Clear()
		ddlConnectionType.Items.Add(New ListItem("Emulate", KaPanel.PanelConnectionType.Emulate))
		ddlConnectionType.Items.Add(New ListItem("Ethernet (Encapsulated MODBUS)", KaPanel.PanelConnectionType.Ethernet))
		ddlConnectionType.Items.Add(New ListItem("Ethernet (MODBUS/TCP)", KaPanel.PanelConnectionType.ModbusTcp))
		ddlConnectionType.Items.Add(New ListItem("Serial", KaPanel.PanelConnectionType.SerialPort))
	End Sub

	Private Sub PopulateRoleList()
		ddlRole.Items.Clear()
		ddlRole.Items.Add(New ListItem("Bulk-weigher", KaPanel.PanelRole.BulkWeigher))
		ddlRole.Items.Add(New ListItem("Declining weight hopper", KaPanel.PanelRole.DecliningWeightHopper))
		ddlRole.Items.Add(New ListItem("Dry tower mixer", KaPanel.PanelRole.DryTowerMixer))
		ddlRole.Items.Add(New ListItem("Dry tower surge tank", KaPanel.PanelRole.DryTowerSurgeTank))
		ddlRole.Items.Add(New ListItem("Dry tower weigh-hopper", KaPanel.PanelRole.DryTowerWeighHopper))
		ddlRole.Items.Add(New ListItem("Dry tower weigh-hopper (pair)", KaPanel.PanelRole.DryTowerWeighHopperPair))
		ddlRole.Items.Add(New ListItem("Dry tower weigh-hoppers (combined)", KaPanel.PanelRole.DryTowerCombinedWeighHoppers))
		ddlRole.Items.Add(New ListItem("Dry tower weigh-mixer", KaPanel.PanelRole.DryTowerWeighMixer))
		ddlRole.Items.Add(New ListItem("Line blender", KaPanel.PanelRole.LineBlenderFillOnly))
		ddlRole.Items.Add(New ListItem("Line blender with weigh-hopper", KaPanel.PanelRole.LineBlenderFillDischarge))
		ddlRole.Items.Add(New ListItem("Liquid blender", KaPanel.PanelRole.LiquidBlender))
		ddlRole.Items.Add(New ListItem("Meter (mass-flow/volumetric)", KaPanel.PanelRole.MeterFillOnly))
		ddlRole.Items.Add(New ListItem("Multi-meter (mass-flow)", KaPanel.PanelRole.MultiMeterFillOnly))
		ddlRole.Items.Add(New ListItem("Multi-meter simultaneous (mass-flow)", KaPanel.PanelRole.SimultaneousMeterFillOnly))
		ddlRole.Items.Add(New ListItem("Multi-meter simultaneous (volumetric)", KaPanel.PanelRole.MultiMeterFillOnly2))
		ddlRole.Items.Add(New ListItem("Multiple declining weight hoppers", KaPanel.PanelRole.MultipleDecliningWeightHoppers))
		ddlRole.Items.Add(New ListItem("Platform scale", KaPanel.PanelRole.ScaleFillOnly))
		ddlRole.Items.Add(New ListItem("Rotary mixer", KaPanel.PanelRole.RotaryMixer))
		ddlRole.Items.Add(New ListItem("Seed system hopper", KaPanel.PanelRole.SeedSystemHopper))
		ddlRole.Items.Add(New ListItem("Seed system treater", KaPanel.PanelRole.SeedSystemTreater))
		ddlRole.Items.Add(New ListItem("TLM-5", KaPanel.PanelRole.TLM5))
		ddlRole.Items.Add(New ListItem("Truck scale", KaPanel.PanelRole.TruckScale))
		ddlRole.Items.Add(New ListItem("Weigh-tank", KaPanel.PanelRole.WeighTank))
	End Sub

	Private Sub PopulateUnitLists()
		ddlWeightUnit.Items.Clear()
		ddlVolumeUnit.Items.Clear()
		ddlHoldThresholdUnit.Items.Clear()
		ddlPanelMinScaleDivisionUnit.Items.Clear()
		ddlPanelMinScaleDivisionDensityWeightUnit.Items.Clear()
		ddlPanelMinScaleDivisionDensityVolumeUnit.Items.Clear()

		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		For Each r As KaUnit In KaUnit.GetAll(connection, "deleted=0", "name ASC")
			If Not KaUnit.IsTime(r.BaseUnit) Then
				ddlHoldThresholdUnit.Items.Add(New ListItem(r.Abbreviation, r.Id.ToString()))
			End If
			If r.BaseUnit <> KaUnit.Unit.Pulses AndAlso r.BaseUnit <> KaUnit.Unit.Seconds Then
				If KaUnit.IsWeight(r.BaseUnit) Then
					ddlWeightUnit.Items.Add(New ListItem(r.Name, r.Id.ToString()))
					ddlPanelMinScaleDivisionDensityWeightUnit.Items.Add(New ListItem(r.Name, r.Id.ToString()))
				Else
					ddlVolumeUnit.Items.Add(New ListItem(r.Name, r.Id.ToString()))
					ddlPanelMinScaleDivisionDensityVolumeUnit.Items.Add(New ListItem(r.Name, r.Id.ToString()))
				End If
				ddlPanelMinScaleDivisionUnit.Items.Add(New ListItem(r.Name, r.Id.ToString()))
			End If
		Next
		Try
			ddlPanelMinScaleDivisionUnit.SelectedValue = KaUnit.GetSystemDefaultMassUnitOfMeasure(connection, Nothing).ToString()
		Catch ex As ArgumentOutOfRangeException

		End Try
		Try
			ddlPanelMinScaleDivisionDensityWeightUnit.SelectedValue = KaUnit.GetSystemDefaultMassUnitOfMeasure(connection, Nothing).ToString()
		Catch ex As ArgumentOutOfRangeException

		End Try
		Try
			ddlPanelMinScaleDivisionDensityVolumeUnit.SelectedValue = KaUnit.GetSystemDefaultVolumeUnitOfMeasure(connection, Nothing).ToString()
		Catch ex As ArgumentOutOfRangeException

		End Try
	End Sub

	Private Sub PopulateBaseUnitLists()
		ddlEmulateBaseUnit.Items.Clear()
		ddlEmulateBaseUnit.Items.Add(New ListItem(KaUnit.GetBaseUnitAbbreviation(KaUnit.Unit.CubicFeet), KaUnit.Unit.CubicFeet))
		ddlEmulateBaseUnit.Items.Add(New ListItem(KaUnit.GetBaseUnitAbbreviation(KaUnit.Unit.FluidOunces), KaUnit.Unit.FluidOunces))
		ddlEmulateBaseUnit.Items.Add(New ListItem(KaUnit.GetBaseUnitAbbreviation(KaUnit.Unit.Gallons), KaUnit.Unit.Gallons))
		ddlEmulateBaseUnit.Items.Add(New ListItem(KaUnit.GetBaseUnitAbbreviation(KaUnit.Unit.Kilograms), KaUnit.Unit.Kilograms))
		ddlEmulateBaseUnit.Items.Add(New ListItem(KaUnit.GetBaseUnitAbbreviation(KaUnit.Unit.Liters), KaUnit.Unit.Liters))
		ddlEmulateBaseUnit.Items.Add(New ListItem(KaUnit.GetBaseUnitAbbreviation(KaUnit.Unit.Ounces), KaUnit.Unit.Ounces))
		ddlEmulateBaseUnit.Items.Add(New ListItem(KaUnit.GetBaseUnitAbbreviation(KaUnit.Unit.Pints), KaUnit.Unit.Pints))
		ddlEmulateBaseUnit.Items.Add(New ListItem(KaUnit.GetBaseUnitAbbreviation(KaUnit.Unit.Pounds), KaUnit.Unit.Pounds))
		'ddlEmulateBaseUnit.Items.Add(New ListItem(KaUnit.GetBaseUnitAbbreviation(KaUnit.Unit.Pulses), KaUnit.Unit.Pulses))
		ddlEmulateBaseUnit.Items.Add(New ListItem(KaUnit.GetBaseUnitAbbreviation(KaUnit.Unit.Quarts), KaUnit.Unit.Quarts))
		'ddlEmulateBaseUnit.Items.Add(New ListItem(KaUnit.GetBaseUnitAbbreviation(KaUnit.Unit.Seconds), KaUnit.Unit.Seconds))
		ddlEmulateBaseUnit.Items.Add(New ListItem(KaUnit.GetBaseUnitAbbreviation(KaUnit.Unit.Tons), KaUnit.Unit.Tons))
	End Sub

	Private Sub PopulateSerialPortList()
		ddlSerialPort.Items.Clear()
		Dim i As Integer = 1
		Do While i < 256
			ddlSerialPort.Items.Add(New ListItem("COM" & i, i))
			i += 1
		Loop
	End Sub

	Private Sub PopulateBaudRateList()
		ddlBaudRate.Items.Clear()
		ddlBaudRate.Items.Add(New ListItem("9600", "9600"))
		ddlBaudRate.Items.Add(New ListItem("19200", "19200"))
		ddlBaudRate.Items.Add(New ListItem("38400", "38400"))
		ddlBaudRate.Items.Add(New ListItem("57600", "57600"))
		ddlBaudRate.Items.Add(New ListItem("115200", "115200"))
	End Sub

	Private Sub PopulateParityList()
		ddlParity.Items.Clear()
		ddlParity.Items.Add(New ListItem("Even", System.IO.Ports.Parity.Even))
		ddlParity.Items.Add(New ListItem("Mark", System.IO.Ports.Parity.Mark))
		ddlParity.Items.Add(New ListItem("None", System.IO.Ports.Parity.None))
		ddlParity.Items.Add(New ListItem("Odd", System.IO.Ports.Parity.Odd))
		ddlParity.Items.Add(New ListItem("Space", System.IO.Ports.Parity.Space))
	End Sub

	Private Sub PopulateDataBitsList()
		ddlDataBits.Items.Clear()
		Dim i As Integer = 4
		Do While i < 9
			ddlDataBits.Items.Add(New ListItem(i, i))
			i += 1
		Loop
	End Sub

	Private Sub PopulateStopBitsList()
		ddlStopBits.Items.Clear()
		ddlStopBits.Items.Add(New ListItem("None", System.IO.Ports.StopBits.None))
		ddlStopBits.Items.Add(New ListItem("1", System.IO.Ports.StopBits.One))
		ddlStopBits.Items.Add(New ListItem("1.5", System.IO.Ports.StopBits.OnePointFive))
		ddlStopBits.Items.Add(New ListItem("2", System.IO.Ports.StopBits.Two))
	End Sub

	Private Function PopulatePanelInformation(ByVal panelId As Guid) As Boolean
		_customFieldData.Clear()
		With New KaPanel()
			.Id = panelId
			If panelId <> Guid.Empty Then
				.SqlSelect(GetUserConnection(_currentUser.Id))

				For Each customFieldValue As KaCustomFieldData In KaCustomFieldData.GetAll(GetUserConnection(_currentUser.Id), String.Format("deleted = 0 AND {0} = {1}", KaCustomFieldData.FN_RECORD_ID, Q(.Id)), KaCustomFieldData.FN_LAST_UPDATED)
					_customFieldData.Add(customFieldValue)
				Next
			End If
			tbxName.Text = .Name
			Try
				If .LocationId.Equals(Guid.Empty) AndAlso ddlLocation.Items.Count = 2 Then
					ddlLocation.SelectedIndex = 1
				Else
					ddlLocation.SelectedValue = .LocationId.ToString()
				End If
			Catch ex As ArgumentOutOfRangeException
				ddlLocation.SelectedIndex = 0
				ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), "InvalidFacilityId", Utilities.JsAlert("Record not found in facilities where ID = " & .LocationId.ToString() & ". Facility not set."), False)
			End Try
			Try
				ddlConnectionType.SelectedValue = .ConnectionType
			Catch ex As ArgumentOutOfRangeException
				ddlConnectionType.SelectedValue = KaPanel.PanelConnectionType.Ethernet
				ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), "InvalidConnectiontype", Utilities.JsAlert("Invalid connection type (" & .ConnectionType & "). Connection type set to ""Ethernet""."), False)
			End Try
			UpdateConnectionTypeSettings()
			Try
				ddlSerialPort.SelectedValue = .SerialPort
			Catch ex As ArgumentOutOfRangeException
				ddlSerialPort.SelectedValue = "1"
				ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), "InvalidSerialPort", Utilities.JsAlert("Invalid serial port (COM" & .SerialPort & "). Serial port set to ""COM1""."), False)
			End Try
			Try
				ddlBaudRate.SelectedValue = .BaudRate
			Catch ex As ArgumentOutOfRangeException
				ddlBaudRate.SelectedValue = "19200"
				ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), "InvalidBaudRate", Utilities.JsAlert("Invalid baud rate (" & .BaudRate & "). Baud rate set to ""19200""."), False)
			End Try
			Try
				ddlParity.SelectedValue = .Parity
			Catch ex As ArgumentOutOfRangeException
				ddlParity.SelectedValue = System.IO.Ports.Parity.None
				ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), "InvalidParity", Utilities.JsAlert("Invalid parity (" & .Parity & "). Parity set to ""None""."), False)
			End Try
			Try
				ddlDataBits.SelectedValue = .DataBits
			Catch ex As Exception
				ddlDataBits.SelectedValue = "8"
				ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), "InvalidDataBits", Utilities.JsAlert("Invalid data bits (" & .DataBits & "). Data bits set to ""8""."), False)
			End Try
			Try
				ddlStopBits.SelectedValue = .StopBits
			Catch ex As Exception
				ddlStopBits.SelectedValue = System.IO.Ports.StopBits.One
				ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), "InvalidStopBits", Utilities.JsAlert("Invalid stop bits (" & .StopBits & "). Stop bits set to ""One""."), False)
			End Try
			Try
				ddlRole.SelectedValue = .Role
			Catch ex As ArgumentException
				ddlRole.SelectedValue = KaPanel.PanelRole.MultiMeterFillOnly
				If panelId <> Guid.Empty Then ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), "InvalidRole", Utilities.JsAlert("Invalid role (" & .Role & "). Role set to ""Multi-meter (mass flow)""."), False)
			End Try
			UpdateRoleSettings()
			tbxRank.Text = .Rank
			tbxFollowBy.Text = .FollowBy
			tbxRinseThreshold.Text = .RinseThreshold
			Try
				If .WeightUnitId = Guid.Empty Then Throw New ArgumentException()
				ddlWeightUnit.SelectedValue = .WeightUnitId.ToString()
			Catch ex As ArgumentException
				Try
					ddlWeightUnit.SelectedValue = KaUnit.GetSystemDefaultMassUnitOfMeasure(GetUserConnection(_currentUser.Id), Nothing).ToString()
					If ddlPanels.SelectedIndex > 0 Then ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), "InvalidMassUnit", Utilities.JsAlert("The mass unit was not defined.  It will be set to the default weight unit of measure."), False) ' Don't warn if the panel has not been selected
				Catch ex2 As ArgumentException
					If ddlWeightUnit.Items.Count > 0 Then ddlWeightUnit.SelectedIndex = 0
					If ddlPanels.SelectedIndex > 0 Then ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), "InvalidMassUnitNotDefined", Utilities.JsAlert("The mass unit was not defined."), False) ' Don't warn if the panel has not been selected
				End Try
			End Try
			Try
				If .VolumeUnitId = Guid.Empty Then Throw New ArgumentException()
				ddlVolumeUnit.SelectedValue = .VolumeUnitId.ToString()
			Catch ex As ArgumentException
				Try
					ddlVolumeUnit.SelectedValue = KaUnit.GetSystemDefaultVolumeUnitOfMeasure(GetUserConnection(_currentUser.Id), Nothing).ToString()
					If ddlPanels.SelectedIndex > 0 Then ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), "InvalidVolumeUnit", Utilities.JsAlert("The volume unit was not defined.  It will be set to the default volume unit of measure."), False) ' Don't warn if the panel has not been selected
				Catch ex2 As ArgumentException
					If ddlVolumeUnit.Items.Count > 0 Then ddlVolumeUnit.SelectedIndex = 0
					If ddlPanels.SelectedIndex > 0 Then ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), "InvalidVolumeUnit", Utilities.JsAlert("The volume unit was not defined."), False) ' Don't warn if the panel has not been selected
				End Try
			End Try
			tbxHoldThreshold.Text = .HoldThreshold
			Try
				ddlHoldThresholdUnit.SelectedValue = .HoldThresholdUnitId.ToString()
			Catch ex As ArgumentException
				Try
					ddlWeightUnit.SelectedValue = KaUnit.GetUnitIdForBaseUnit(GetUserConnection(_currentUser.Id), KaUnit.Unit.Pounds).ToString()
				Catch ex2 As ArgumentException
					If ddlWeightUnit.Items.Count > 0 Then ddlWeightUnit.SelectedIndex = 0
				End Try
			End Try
			tbxSlaveNumber.Text = .SlaveNumber
			tbxSystemAddress.Text = .SystemAddress
			tbxIpAddress.Text = .IpAddress
			tbxTcpPort.Text = .TcpPort
			tbxEmulateUnitsPerSecond.Text = .EmulateUnitsPerSecond
			Try
				ddlEmulateBaseUnit.SelectedValue = .EmulateBaseUnit.ToString()
			Catch ex As ArgumentException
				ddlEmulateBaseUnit.SelectedValue = "0"
			End Try
			chkUseReservations.Checked = .UseReservations
			Dim reservations As ArrayList = KaPanelReservation.GetAll(GetUserConnection(_currentUser.Id), String.Format("panel_id='{0}'", .Id.ToString()), "")
			If reservations.Count > 0 Then ' a reservation record exists
				If CType(reservations(0), KaPanelReservation).ApplicationId.Trim().Length > 0 Then ' there is a reservation
					litCurrentReservation.Text = "<br \>Current: " & CType(reservations(0), KaPanelReservation).ApplicationId
					btnClearReservation.Visible = True
				Else ' no reservation at this time
					litCurrentReservation.Text = ""
					btnClearReservation.Visible = False
				End If
			Else ' no reservation record
				litCurrentReservation.Text = ""
				btnClearReservation.Visible = False
			End If
			If .Ka2000ApplicationIdentifier > 0 Then
				lblKa2000Application.Text = KaController.Controller.ApplicationIdentifierDescription(.Ka2000ApplicationIdentifier)
				Try
					Dim systemIdentifier As KaPanel.PanelRole = .Ka2000SystemIdentifier

					lblKa2000System.Text = System.Text.RegularExpressions.Regex.Replace(systemIdentifier.ToString(), "[A-Z]", " $0")
				Catch ex As Exception
					lblKa2000System.Text = String.Format("{0:0}", .Ka2000SystemIdentifier)
				End Try
				Dim major As Byte = 0
				Dim minor As Byte = 0
				KaTm2LoadFramework.LfControllers.ParseVersion(.Ka2000Version, major, minor)
				lblKa2000Version.Text = String.Format("{0:0}.{1:0}", major, minor)

				pnlCurrrentPanelSettings.Visible = True
			Else
				pnlCurrrentPanelSettings.Visible = False
			End If
			Dim d As Integer = .Ka2000ScalePrecision >> 8
			Dim n As Integer = .Ka2000ScalePrecision And &HFF&
			Try
				If n <> 0 Then
					ddlPanelMinScaleDivision.SelectedValue = d.ToString()
					If n > 127 Then ' negative number
						ddlPanelMinScaleDivisionMultiplier.SelectedValue = String.Format("{0:#########0.##########}", (10 ^ (128 - n)))
					Else
						ddlPanelMinScaleDivisionMultiplier.SelectedValue = String.Format("{0:#########0.##########}", (10 ^ (n - 1)))
					End If
					lblKa2000SuppliedPrecision.Text = String.Format("{0} x {1}", ddlPanelMinScaleDivision.SelectedValue, ddlPanelMinScaleDivisionMultiplier.SelectedValue)
					pnlKa2000SuppliedPrecision.Visible = True
				Else
					Throw New ArgumentOutOfRangeException()
				End If
			Catch ex As ArgumentOutOfRangeException
				pnlKa2000SuppliedPrecision.Visible = False
				ddlPanelMinScaleDivision.SelectedValue = "1"
				ddlPanelMinScaleDivisionMultiplier.SelectedValue = "0.1"
			End Try

			PopulatePanelUnitPrecisionList(panelId)
		End With
		pnlPanelUnitPrecisions.Visible = Not panelId.Equals(Guid.Empty)
		Utilities.CreateDynamicCustomFieldPanelControls(_customFields, _customFieldData, lstCustomFields, Page)

		Return Not panelId.Equals(Guid.Empty)
	End Function

	Private Function SavePanelInformation(ByRef panelId As Guid) As Boolean
		If tbxName.Text.Trim().Length = 0 Then ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), "InvalidName", Utilities.JsAlert("Please specify a name for the panel."), False) : Return False
		If ddlLocation.SelectedIndex <= 0 Then ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), "InvalidFacility", Utilities.JsAlert("Please select a facility."), False) : Return False
		If KaPanel.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND id<>" & Q(panelId) & " AND name=" & Q(tbxName.Text), "").Count > 0 Then ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), "InvalidNameExists", Utilities.JsAlert("Panel already exists with name """ & tbxName.Text & """. Please enter a unique name for the panel."), False) : Return False
		If Not IsNumeric(tbxRank.Text) Then ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), "InvalidRank", Utilities.JsAlert("Please enter a numeric value for the rank."), False) : Return False
		If Not IsNumeric(tbxFollowBy.Text) Then ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), "InvalidFollowby", Utilities.JsAlert("Please enter a numeric value for the follow by."), False) : Return False
		If Not IsNumeric(tbxRinseThreshold.Text) Then ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), "InvalidRinseThreshhold", Utilities.JsAlert("Please enter a numeric value for the rinse threshold."), False) : Return False
		If Not IsNumeric(tbxHoldThreshold.Text) Then ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), "InvalidHoldThreshhold", Utilities.JsAlert("Please enter a numeric value for the hold threshold."), False) : Return False
		If Not IsNumeric(tbxSlaveNumber.Text) Then ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), "InvalidControllerNumber", Utilities.JsAlert("Please enter a numeric value for the controller number."), False) : Return False
		If Not IsNumeric(tbxSystemAddress.Text) Then ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), "InvalidSystemAddress", Utilities.JsAlert("Please enter a numeric value for the system address."), False) : Return False
		Select Case Integer.Parse(ddlConnectionType.SelectedValue)
			Case KaPanel.PanelConnectionType.Emulate ' emulate specific validation
				If Not IsNumeric(tbxEmulateUnitsPerSecond.Text) Then ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), "InvalidEmulationRate", Utilities.JsAlert("Please enter a numeric value for the emulate rate."), False) : Return False
			Case KaPanel.PanelConnectionType.Ethernet ' Ethernet specific validation
				If tbxIpAddress.Text.Trim().Length = 0 Then ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), "InvalidIPAddress", Utilities.JsAlert("Please specify an IP address for the panel."), False) : Return False
				If Not IsNumeric(tbxTcpPort.Text) Then ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), "InvalidTCPPort", Utilities.JsAlert("Please enter a numeric value for the TCP Port (typically 2000 or 502)."), False) : Return False
		End Select
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		With New KaPanel()
			.Id = panelId
			Try
				If Not panelId.Equals(Guid.Empty) Then .SqlSelect(connection)
			Catch ex As RecordNotFoundException

			End Try
			.BaudRate = ddlBaudRate.SelectedValue
			.ConnectionType = ddlConnectionType.SelectedValue
			.DataBits = ddlDataBits.SelectedValue
			.IpAddress = tbxIpAddress.Text
			.LocationId = Guid.Parse(ddlLocation.SelectedValue)
			.Name = tbxName.Text
			.Parity = ddlParity.SelectedValue
			.SerialPort = ddlSerialPort.SelectedValue
			.StopBits = ddlStopBits.SelectedValue
			Try ' to convert the entered TCP port value to an integer...
				.TcpPort = tbxTcpPort.Text
			Catch ex As FormatException ' this was validated earlier, no need to notify user since it is probably not used per the connection type selection...
			End Try
			.Role = ddlRole.SelectedItem.Value
			.Rank = Integer.Parse(tbxRank.Text)
			.FollowBy = Math.Min(Math.Max(Double.Parse(tbxFollowBy.Text), 0), 100)
			.RinseThreshold = Math.Max(Double.Parse(tbxRinseThreshold.Text), 0)
			.HoldThreshold = Double.Parse(tbxHoldThreshold.Text)
			.HoldThresholdUnitId = Guid.Parse(ddlHoldThresholdUnit.SelectedValue)
			.WeightUnitId = Guid.Parse(ddlWeightUnit.SelectedItem.Value)
			.VolumeUnitId = Guid.Parse(ddlVolumeUnit.SelectedItem.Value)
			.SlaveNumber = Math.Min(Math.Max(Integer.Parse(tbxSlaveNumber.Text), 1), 255)
			.SystemAddress = Math.Min(Math.Max(Integer.Parse(tbxSystemAddress.Text), UShort.MinValue), UShort.MaxValue)
			.EmulateBaseUnit = ddlEmulateBaseUnit.SelectedValue
			Try ' to convert the entered emulation rate to a double...
				.EmulateUnitsPerSecond = Double.Parse(tbxEmulateUnitsPerSecond.Text)
			Catch ex As FormatException ' this was validated earlier, no need to notify user since it is probably not used per the connection type selection...
			End Try
			.UseReservations = chkUseReservations.Checked

			If .Id = Guid.Empty Then
				Dim newDischargeLocation As New KaDischargeLocation()
				With newDischargeLocation
					.Name = tbxName.Text
					.FillLimitUnitId = KaUnit.GetSystemDefaultMassUnitOfMeasure(connection, Nothing)
					.SqlInsert(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
				End With
				.DischargeLocationId = newDischargeLocation.Id
				.SqlInsert(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
				lblStatus.Text = "Panel successfully added."
			Else
				.SqlUpdate(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
				lblStatus.Text = "Panel successfully updated."
			End If
			panelId = .Id

			Utilities.ConvertCustomFieldPanelToLists(_customFields, _customFieldData, lstCustomFields)
			For Each customData As KaCustomFieldData In _customFieldData
				customData.RecordId = .Id
				customData.SqlUpdateInsertIfNotFound(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
			Next
		End With
		Return True
	End Function

	Private Function DeletePanelInformation(ByVal panelId As Guid) As Boolean
		Dim tanks As ArrayList = KaTank.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND panel_id=" & Q(panelId), "")
		If tanks.Count > 0 Then
			Dim warning As String = "Can not delete panel. Panel is referenced by the following records:\n\nTanks:\n\n"
			For Each tank As KaTank In tanks
				warning &= tank.Name & " "
			Next
			ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), "InvalidPanelUsedByTanks", Utilities.JsAlert(warning), False)
			Return False
		Else
			With New KaPanel(GetUserConnection(_currentUser.Id), panelId)
				.Deleted = True
				.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
			End With
			DeletePanelBulkProducts(panelId)
			lblStatus.Text = "Panel successfully deleted."
			Return True
		End If
	End Function

	Private Sub DeletePanelBulkProducts(ByVal panelId As Guid)
		For Each r As KaBulkProductPanelSettings In KaBulkProductPanelSettings.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND panel_id=" & Q(panelId), "")
			r.Deleted = True
			r.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
		Next
	End Sub

#Region " Panel Unit Precision "
	Protected Sub PopulatePanelUnitPrecisionList(panelId As Guid)
		Dim table As HtmlTable = Page.FindControl("tblPanelUnitPrecisions")
		If table Is Nothing Then Exit Sub
		Do While table.Rows.Count > 1
			table.Rows.RemoveAt(1)
		Loop
		If panelId <> Guid.Empty Then
			Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
			For Each unit As KaUnit In KaUnit.GetAll(connection, "deleted=0", "name ASC")
				If KaUnit.IsTime(unit.BaseUnit) Then Continue For
				Dim row As New HtmlTableRow
				row.ID = "Unit" & unit.Id.ToString()
				table.Rows.Add(row)
				' row ID
				Dim cell As New HtmlTableCell()
				row.Cells.Add(cell)
				Dim input As New HtmlInputText
				With input
					.Attributes("type") = "hidden"
					.ID = row.ID & "_Id"
					.Value = unit.Id.ToString
				End With
				cell.Controls.Add(input)

				' unit name
				Dim label As New Label()
				With label
					.ID = row.ID & "_Name"
					.Text = unit.Name
				End With
				cell.Controls.Add(label)

				' current format
				cell = New HtmlTableCell()
				cell.Attributes("style") = "text-align: left;"
				row.Cells.Add(cell)

				label = New Label()
				With label
					.ID = row.ID & "_PanelUnitPrecision"
					.Text = KaPanel.GetUnitPrecision(connection, panelId, unit.Id)
					.Attributes("style") = "text-align: left;"
				End With
				cell.Controls.Add(label)

				' whole digits control
				cell = New HtmlTableCell()
				cell.Attributes("style") = "text-align: center;"
				row.Cells.Add(cell)
				Dim linkButton As New LinkButton()
				With linkButton
					.ID = row.ID & "_btnUnitPrecisionMoreWhole"
					.Text = "+"
					.CssClass = "button"
					.Attributes("style") = "width: auto; text-align: center;"
					AddHandler .Click, AddressOf UnitPrecisionMoreWhole
				End With
				cell.Controls.Add(linkButton)

				cell = New HtmlTableCell()
				cell.Attributes("style") = "text-align: center;"
				row.Cells.Add(cell)
				linkButton = New LinkButton()
				With linkButton
					.ID = row.ID & "_btnUnitPrecisionLessWhole"
					.Text = "-"
					.CssClass = "button"
					.Attributes("style") = "width: auto; text-align: center;"
					AddHandler .Click, AddressOf UnitPrecisionLessWhole
				End With
				cell.Controls.Add(linkButton)

				' fraction digits control
				cell = New HtmlTableCell()
				cell.Attributes("style") = "text-align: center;"
				row.Cells.Add(cell)
				linkButton = New LinkButton()
				With linkButton
					.ID = row.ID & "_btnUnitPrecisionMoreFractional"
					.Text = "+"
					.CssClass = "button"
					.Attributes("style") = "width: auto; text-align: center;"
					AddHandler .Click, AddressOf UnitPrecisionMoreFractional
				End With
				cell.Controls.Add(linkButton)

				cell = New HtmlTableCell()
				cell.Attributes("style") = "text-align: center;"
				row.Cells.Add(cell)
				linkButton = New LinkButton()
				With linkButton
					.ID = row.ID & "_btnUnitPrecisionLessFractional"
					.Text = "-"
					.CssClass = "button"
					.Attributes("style") = "width: auto; text-align: center;"
					AddHandler .Click, AddressOf UnitPrecisionLessFractional
				End With
				cell.Controls.Add(linkButton)

				' default
				cell = New HtmlTableCell()
				cell.Attributes("style") = "text-align: left;"
				row.Cells.Add(cell)
				label = New Label()
				With label
					.ID = row.ID & "_UnitDefaultPrecision"
					.Text = unit.UnitPrecision
					.Attributes("style") = "text-align: left;"
				End With
				cell.Controls.Add(label)

				' use default
				cell = New HtmlTableCell()
				cell.Attributes("style") = "text-align: center;"
				row.Cells.Add(cell)
				Dim button As New Button()
				With button
					.ID = row.ID & "_btnUnitPrecisionUseDefault"
					.Text = "Use Default"
					.Attributes("style") = "width: auto; text-align: center;"
					AddHandler .Click, AddressOf UnitPrecisionUseDefaultButtonClick
				End With
				cell.Controls.Add(button)
			Next
		End If
		btnPanelMinScaleDivision_Click(Nothing, Nothing)
	End Sub

	Protected Sub UnitPrecisionMoreWhole(sender As Object, e As EventArgs)
		Dim controlIdStrings() As String = CType(sender, LinkButton).ID.Split("_")
		Dim currentUnitRowId As String = controlIdStrings(0)
		Dim unitId As Guid = Guid.Parse(CType(FindControl(currentUnitRowId & "_Id"), HtmlInputText).Value)
		Dim currentPrecision As String = CType(FindControl(currentUnitRowId & "_PanelUnitPrecision"), Label).Text
		Dim panelId As Guid = Guid.Parse(ddlPanels.SelectedValue)
		Dim whole As Integer
		Dim fractional As Integer
		Tm2Database.GetPrecisionWholeAndFractionalDigitCount(currentPrecision, whole, fractional)
		currentPrecision = Tm2Database.GeneratePrecisionFormat(Math.Max(whole + 1, 0), fractional)
		KaPanel.SetUnitPrecision(GetUserConnection(_currentUser.Id), panelId, unitId, currentPrecision)
		PopulatePanelUnitPrecisionList(panelId)
	End Sub

	Protected Sub UnitPrecisionLessWhole(sender As Object, e As EventArgs)
		Dim controlIdStrings() As String = CType(sender, LinkButton).ID.Split("_")
		Dim currentUnitRowId As String = controlIdStrings(0)
		Dim unitId As Guid = Guid.Parse(CType(FindControl(currentUnitRowId & "_Id"), HtmlInputText).Value)
		Dim currentPrecision As String = CType(FindControl(currentUnitRowId & "_PanelUnitPrecision"), Label).Text
		Dim panelId As Guid = Guid.Parse(ddlPanels.SelectedValue)
		Dim whole As Integer
		Dim fractional As Integer
		Tm2Database.GetPrecisionWholeAndFractionalDigitCount(currentPrecision, whole, fractional)
		currentPrecision = Tm2Database.GeneratePrecisionFormat(Math.Max(whole - 1, 0), fractional)
		KaPanel.SetUnitPrecision(GetUserConnection(_currentUser.Id), panelId, unitId, currentPrecision)
		PopulatePanelUnitPrecisionList(panelId)
	End Sub

	Protected Sub UnitPrecisionMoreFractional(sender As Object, e As EventArgs)
		Dim controlIdStrings() As String = CType(sender, LinkButton).ID.Split("_")
		Dim currentUnitRowId As String = controlIdStrings(0)
		Dim unitId As Guid = Guid.Parse(CType(FindControl(currentUnitRowId & "_Id"), HtmlInputText).Value)
		Dim currentPrecision As String = CType(FindControl(currentUnitRowId & "_PanelUnitPrecision"), Label).Text
		Dim panelId As Guid = Guid.Parse(ddlPanels.SelectedValue)
		Dim whole As Integer
		Dim fractional As Integer
		Tm2Database.GetPrecisionWholeAndFractionalDigitCount(currentPrecision, whole, fractional)
		currentPrecision = Tm2Database.GeneratePrecisionFormat(whole, Math.Max(fractional + 1, 0))
		KaPanel.SetUnitPrecision(GetUserConnection(_currentUser.Id), panelId, unitId, currentPrecision)
		PopulatePanelUnitPrecisionList(panelId)
	End Sub

	Protected Sub UnitPrecisionLessFractional(sender As Object, e As EventArgs)
		Dim controlIdStrings() As String = CType(sender, LinkButton).ID.Split("_")
		Dim currentUnitRowId As String = controlIdStrings(0)
		Dim unitId As Guid = Guid.Parse(CType(FindControl(currentUnitRowId & "_Id"), HtmlInputText).Value)
		Dim currentPrecision As String = CType(FindControl(currentUnitRowId & "_PanelUnitPrecision"), Label).Text
		Dim panelId As Guid = Guid.Parse(ddlPanels.SelectedValue)
		Dim whole As Integer
		Dim fractional As Integer
		Tm2Database.GetPrecisionWholeAndFractionalDigitCount(currentPrecision, whole, fractional)
		currentPrecision = Tm2Database.GeneratePrecisionFormat(whole, Math.Max(fractional - 1, 0))
		KaPanel.SetUnitPrecision(GetUserConnection(_currentUser.Id), panelId, unitId, currentPrecision)
		PopulatePanelUnitPrecisionList(panelId)
	End Sub

	Private Sub UnitPrecisionUseDefaultButtonClick(sender As Object, e As EventArgs)
		Dim controlIdStrings() As String = CType(sender, Button).ID.Split("_")
		Dim currentUnitRowId As String = controlIdStrings(0)
		Dim unitId As Guid = Guid.Parse(CType(FindControl(currentUnitRowId & "_Id"), HtmlInputText).Value)
		Dim unitPrecision As String = CType(FindControl(currentUnitRowId & "_UnitDefaultPrecision"), Label).Text
		Dim panelId As Guid = Guid.Parse(ddlPanels.SelectedValue)
		KaPanel.SetUnitPrecision(GetUserConnection(_currentUser.Id), panelId, unitId, unitPrecision)
		PopulatePanelUnitPrecisionList(panelId)
	End Sub

	Protected Sub btnResetAllPrecisiontoUnits_Click(sender As Object, e As EventArgs) Handles btnResetAllPrecisiontoUnits.Click
		Dim panelId As Guid = Guid.Empty
		If ddlPanels.SelectedIndex >= 0 Then Guid.TryParse(ddlPanels.SelectedValue, panelId)
		If Not panelId.Equals(Guid.Empty) Then
			Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
			For Each unitInfo As KaUnit In KaUnit.GetAll(connection, "deleted=0", "name ASC")
				If KaUnit.IsTime(unitInfo.BaseUnit) Then Continue For
				KaPanel.SetUnitPrecision(GetUserConnection(_currentUser.Id), panelId, unitInfo.Id, unitInfo.UnitPrecision)
			Next
		End If
		PopulatePanelUnitPrecisionList(panelId)
	End Sub
#End Region

	Protected Sub btnClearReservation_Click(sender As Object, e As EventArgs) Handles btnClearReservation.Click
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		For Each reservation As KaPanelReservation In KaPanelReservation.GetAll(connection, String.Format("panel_id='{0}'", ddlPanels.SelectedValue), "")
			If reservation.ApplicationId.Trim().Length > 0 Then
				If KaPanelReservation.ReservePanel(connection, reservation.ApplicationId, Guid.Parse(ddlPanels.SelectedValue), True) = 1 Then
					litCurrentReservation.Text = ""
					btnClearReservation.Visible = False
				End If
			End If
		Next
	End Sub

	Private Sub SetTextboxMaxLengths()
		tbxEmulateUnitsPerSecond.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaPanel.TABLE_NAME, "emulate_units_per_second"))
		tbxFollowBy.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaPanel.TABLE_NAME, "follow_by"))
		tbxHoldThreshold.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaPanel.TABLE_NAME, "hold_threshold"))
		tbxIpAddress.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaPanel.TABLE_NAME, "ip_address"))
		tbxName.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaPanel.TABLE_NAME, "name"))
		tbxRank.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaPanel.TABLE_NAME, "rank"))
		tbxRinseThreshold.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaPanel.TABLE_NAME, "rinse_threshold"))
		tbxSlaveNumber.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaPanel.TABLE_NAME, "slave_number"))
		tbxSystemAddress.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaPanel.TABLE_NAME, KaPanel.FN_SYSTEM_ADDRESS))
		tbxTcpPort.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaPanel.TABLE_NAME, "tcp_port"))
	End Sub

	Protected Overrides Function SaveViewState() As Object
		Dim viewState(3) As Object
		'Saving the grid values to the View State
		Utilities.ConvertCustomFieldPanelToLists(_customFields, _customFieldData, lstCustomFields)
		viewState(0) = MyBase.SaveViewState()
		viewState(1) = _customFields
		viewState(2) = _customFieldData
		Dim panelId As Guid = Guid.Empty
		If ddlPanels.SelectedIndex >= 0 Then Guid.TryParse(ddlPanels.SelectedValue, panelId)

		viewState(3) = panelId
		Return viewState
	End Function

	Protected Overrides Sub LoadViewState(savedState As Object)
		'Getting the dropdown list value from view state.
		If savedState IsNot Nothing AndAlso CType(savedState, Object).Length = 4 Then
			_currentUser = Utilities.GetUser(Me)
			Dim viewState As Object() = savedState
			MyBase.LoadViewState(viewState(0))
			_customFields = viewState(1)
			_customFieldData = viewState(2)
			Utilities.CreateDynamicCustomFieldPanelControls(_customFields, _customFieldData, lstCustomFields, Page)
			PopulatePanelUnitPrecisionList(viewState(3))
		Else
			MyBase.LoadViewState(savedState)
		End If
	End Sub

	Private Sub SetControlUsabilityFromPermissions()
		With _currentUserPermission(_currentTableName)
			Dim shouldEnable = (.Edit AndAlso ddlPanels.SelectedIndex > 0) OrElse (.Create AndAlso ddlPanels.SelectedIndex = 0)
			pnlEven.Enabled = shouldEnable
			pnlOdd.Enabled = shouldEnable
			pnlPanelUnitPrecisions.Enabled = shouldEnable
			btnSave.Enabled = shouldEnable
			btnResetAllPrecisiontoUnits.Enabled = shouldEnable
			btnAssignScaleDivisionPrecision.Enabled = shouldEnable
			Dim value = Guid.Parse(ddlPanels.SelectedValue)
			btnDelete.Enabled = .Edit AndAlso .Delete AndAlso value <> Guid.Empty
		End With
	End Sub

	Protected Sub btnPanelMinScaleDivision_Click(sender As Object, e As EventArgs) Handles btnPanelMinScaleDivision.Click
		Do While tblPanelMinScaleDivisionRecommendations.Rows.Count > 1
			tblPanelMinScaleDivisionRecommendations.Rows.RemoveAt(1)
		Loop
		Try
			Dim currentUnit As New KaQuantity(Double.Parse(ddlPanelMinScaleDivision.SelectedValue) * Double.Parse(ddlPanelMinScaleDivisionMultiplier.SelectedValue), Guid.Parse(ddlPanelMinScaleDivisionUnit.SelectedValue))
			Dim density As New KaRatio(0, Guid.Parse(ddlPanelMinScaleDivisionDensityWeightUnit.SelectedValue), Guid.Parse(ddlPanelMinScaleDivisionDensityVolumeUnit.SelectedValue))
			Double.TryParse(tbxPanelMinScaleDivisionDensity.Text, density.Numeric)
			Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
			For Each targetUnit As KaUnit In KaUnit.GetAll(connection, "deleted=0", "name ASC")
				If Not KaUnit.IsTime(targetUnit.BaseUnit) AndAlso targetUnit.BaseUnit <> KaUnit.Unit.Pulses AndAlso targetUnit.BaseUnit <> KaUnit.Unit.Seconds Then
					Dim unitName As New HtmlTableCell
					unitName.InnerText = targetUnit.Name
					Dim unitConversion As New HtmlTableCell
					Try
						unitConversion.InnerText = String.Format("{0:#########0.##########}", KaUnit.Convert(connection, currentUnit, density, targetUnit.Id).Numeric)
					Catch ex As UnitConversionException
						Continue For
					End Try
					Dim unitPrecision As New HtmlTableCell
					unitPrecision.InnerText = GetScaleDivisionRecommendations(unitConversion.InnerText, targetUnit.Id, currentUnit.UnitId)
					Dim newRow As New HtmlTableRow
					tblPanelMinScaleDivisionRecommendations.Rows.Add(newRow)
					newRow.Controls.Add(unitName)
					newRow.Controls.Add(unitConversion)
					newRow.Controls.Add(unitPrecision)
				End If
			Next
		Catch ex As Exception
			Do While tblPanelMinScaleDivisionRecommendations.Rows.Count > 1
				tblPanelMinScaleDivisionRecommendations.Rows.RemoveAt(1)
			Loop
		End Try

		btnAssignScaleDivisionPrecision.Visible = ddlPanels.SelectedIndex >= 0 AndAlso Not Guid.Empty.Equals(Guid.Parse(ddlPanels.SelectedValue)) AndAlso tblPanelMinScaleDivisionRecommendations.Rows.Count > 1
	End Sub

	Private Function GetScaleDivisionRecommendations(convertedvalue As String, unitId As Guid, meterUnitId As Guid) As String
		Dim decimalFound As Boolean = False
		Dim wholeUnits As Integer = 0
		Dim decimalUnits As Integer = 0
		If unitId.Equals(meterUnitId) Then
			For index As Integer = 0 To convertedvalue.Length - 1
				If convertedvalue(index) = "0" Then
					If decimalFound Then
						decimalUnits += 1
					ElseIf wholeUnits > 0 Then
						wholeUnits += 1
					End If
				ElseIf convertedvalue(index) = "." Then
					decimalFound = True
				ElseIf IsNumeric(convertedvalue(index)) Then
					If decimalFound Then
						decimalUnits += 1
						Exit For
					Else
						wholeUnits += 1
					End If
				End If
			Next
		Else
			For index As Integer = 0 To convertedvalue.Length - 1
				If convertedvalue(index) = "0" Then
					If decimalFound Then
						decimalUnits += 1
					End If
				ElseIf convertedvalue(index) = "." Then
					decimalFound = True
					If wholeUnits > 0 Then Exit For
				ElseIf IsNumeric(convertedvalue(index)) Then
					If decimalFound Then
						decimalUnits += 1
						Exit For
					Else
						wholeUnits += 1
					End If
				End If
			Next
			wholeUnits = 1
		End If
		Return Tm2Database.GeneratePrecisionFormat(Math.Max(1, wholeUnits), decimalUnits)
	End Function

	Protected Sub btnAssignScaleDivisionPrecision_Click(sender As Object, e As EventArgs) Handles btnAssignScaleDivisionPrecision.Click
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim currentUnit As New KaQuantity(Double.Parse(ddlPanelMinScaleDivision.SelectedValue) * Double.Parse(ddlPanelMinScaleDivisionMultiplier.SelectedValue), Guid.Parse(ddlPanelMinScaleDivisionUnit.SelectedValue))
		Dim density As New KaRatio(0, Guid.Parse(ddlPanelMinScaleDivisionDensityWeightUnit.SelectedValue), Guid.Parse(ddlPanelMinScaleDivisionDensityVolumeUnit.SelectedValue))
		Double.TryParse(tbxPanelMinScaleDivisionDensity.Text, density.Numeric)
		Dim panelId As Guid = Guid.Parse(ddlPanels.SelectedValue)
		For Each targetUnit As KaUnit In KaUnit.GetAll(connection, "deleted=0", "name ASC")
			If Not KaUnit.IsTime(targetUnit.BaseUnit) AndAlso targetUnit.BaseUnit <> KaUnit.Unit.Pulses AndAlso targetUnit.BaseUnit <> KaUnit.Unit.Seconds Then
				Try
					KaPanel.SetUnitPrecision(connection, panelId, targetUnit.Id, GetScaleDivisionRecommendations(String.Format("{0:#########0.##########}", KaUnit.Convert(connection, currentUnit, density, targetUnit.Id).Numeric), targetUnit.Id, currentUnit.UnitId))
				Catch ex As UnitConversionException
				End Try
			End If
		Next
		PopulatePanelUnitPrecisionList(panelId)
	End Sub

	Protected Sub ddlFacilityFilter_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlFacilityFilter.SelectedIndexChanged
		PopulateLocationList()
		PopulatePanelList()
		KaSetting.WriteSetting(GetUserConnection(_currentUser.Id), "PanelsPage:" & _currentUser.Id.ToString & "/LastFacilityUsed", ddlFacilityFilter.SelectedValue)
	End Sub
	Protected Sub ScriptManager1_AsyncPostBackError(ByVal sender As Object, ByVal e As System.Web.UI.AsyncPostBackErrorEventArgs)
		ToolkitScriptManager1.AsyncPostBackErrorMessage = e.Exception.Message
	End Sub
End Class

