Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb
Public Class DefaultReceivingWebTicketSettings : Inherits System.Web.UI.Page

	Private _currentUser As KaUser
	Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
	Private _currentTableName As String = KaSetting.TABLE_NAME

	Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
		Response.Cache.SetCacheability(HttpCacheability.NoCache)
		_currentUser = Utilities.GetUser(Me)
		_currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "GeneralSettings")
		If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")
		btnSaveOwnerWebTicketSettings.Enabled = _currentUserPermission(_currentTableName).Edit
		btnDeleteOwnerWebTicketSettings.Enabled = _currentUserPermission(_currentTableName).Delete
		If Not Page.IsPostBack Then
			pnlShowLotNumber.Visible = Tm2Database.SystemItemTraceabilityEnabled
			PopulateOwnersList()
			PopulateAdditionalUnitsList()
			PopulateWebTicketDensityUnits()
			PopulateWebTicketOwnerSettings(Guid.Empty)

			Utilities.ConfirmBox(Me.btnDeleteOwnerWebTicketSettings, "Are you sure you want to delete the web ticket settings for this owner?")
		End If
	End Sub

	Private Sub PopulateOwnersList()
		ddlWebTicketOwner.Items.Clear()
		ddlWebTicketOwner.Items.Add(New ListItem("All owners", Guid.Empty.ToString))
		For Each o As KaOwner In KaOwner.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
			ddlWebTicketOwner.Items.Add(New ListItem(o.Name, o.Id.ToString))
		Next
	End Sub

#Region " Default web ticket settings "
	Private Sub PopulateWebTicketOwnerSettings(ByVal ownerId As Guid)
		'Reset fields
		cblAdditionalUnitsForTicket.ClearSelection()
		lblWebTicketSettingsExist.Visible = False

		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim allSettings As ArrayList = KaSetting.GetAll(connection, "name like 'ReceivingWebTicketSetting:" & ownerId.ToString & "%' and deleted=0", "")
		lblWebTicketSettingsExist.Visible = (allSettings.Count > 0)
		lblWebTicketSettingsExist.Text = "Settings exist"
		If ownerId.Equals(Guid.Empty) Then
			Dim settingsValidForOwners As String = ""
			For Each possibleOwner As KaOwner In KaOwner.GetAll(connection, "deleted = 0", "name ASC")
				Dim ownerTicketSettingsCountRdr As OleDbDataReader = Tm2Database.ExecuteReader(connection, "SELECT COUNT(*) FROM settings WHERE name LIKE 'ReceivingWebTicketSetting:" & possibleOwner.Id.ToString & "%' AND deleted = 0")
				If ownerTicketSettingsCountRdr.Read() AndAlso ownerTicketSettingsCountRdr.Item(0) = 0 Then
					If settingsValidForOwners.Length > 0 Then settingsValidForOwners &= ", "
					settingsValidForOwners &= possibleOwner.Name
				End If
				ownerTicketSettingsCountRdr.Close()
			Next
			If settingsValidForOwners.Length > 0 Then lblWebTicketSettingsExist.Text = "These settings are valid for " & settingsValidForOwners
		End If

		For Each unitId As String In GetReceivingWebTicketSettingByOwnerId(connection, ownerId, KaSetting.DefaultReceivingWebTicketSettings.SN_ADDITIONAL_TICKET_UNITS, "").Trim().Split(",")
			For Each item As ListItem In cblAdditionalUnitsForTicket.Items
				If item.Value = unitId Then
					item.Selected = True
					Exit For
				End If
			Next
		Next

		Boolean.TryParse(GetReceivingWebTicketSettingByOwnerId(connection, ownerId, KaSetting.DefaultReceivingWebTicketSettings.SN_SHOW_DATE_ON_TICKET, "True"), cbxShowDate.Checked)
		Boolean.TryParse(GetReceivingWebTicketSettingByOwnerId(connection, ownerId, KaSetting.DefaultReceivingWebTicketSettings.SN_SHOW_TIME_ON_TICKET, "True"), cbxShowTime.Checked)
		cbxShowDate_CheckedChanged(cbxShowDate, New EventArgs())
		Boolean.TryParse(GetReceivingWebTicketSettingByOwnerId(connection, ownerId, KaSetting.DefaultReceivingWebTicketSettings.SN_SHOW_OWNER_ON_TICKET, "True"), cbxShowOwner.Checked)
		Boolean.TryParse(GetReceivingWebTicketSettingByOwnerId(connection, ownerId, KaSetting.DefaultReceivingWebTicketSettings.SN_SHOW_SUPPLIER_ON_TICKET, "True"), cbxShowSupplier.Checked)
		Boolean.TryParse(GetReceivingWebTicketSettingByOwnerId(connection, ownerId, KaSetting.DefaultReceivingWebTicketSettings.SN_SHOW_CARRIER_ON_TICKET, "True"), cbxShowCarrierId.Checked)
		Boolean.TryParse(GetReceivingWebTicketSettingByOwnerId(connection, ownerId, KaSetting.DefaultReceivingWebTicketSettings.SN_SHOW_TRANSPORT_ON_TICKET, "True"), cbxShowTransport.Checked)
		Boolean.TryParse(GetReceivingWebTicketSettingByOwnerId(connection, ownerId, KaSetting.DefaultReceivingWebTicketSettings.SN_SHOW_TRANSPORT_WEIGHTS_ON_TICKET, "True"), chkShowTicketTransportTareInfo.Checked)
		Boolean.TryParse(GetReceivingWebTicketSettingByOwnerId(connection, ownerId, KaSetting.DefaultReceivingWebTicketSettings.SN_SHOW_DENSITY_ON_TICKET, False.ToString()), cbxShowDensityOnTicket.Checked)
		Dim truckWeightOrder() As String = GetReceivingWebTicketSettingByOwnerId(connection, ownerId, KaSetting.DefaultReceivingWebTicketSettings.SN_TARE_GROSS_NET_ORDER, "T-G-N").Split("-")
		If truckWeightOrder.Length > 0 Then
			PopulateTicketTransportTareOptions(ddlTransportTareFirstOption, truckWeightOrder(0))
		Else
			PopulateTicketTransportTareOptions(ddlTransportTareFirstOption, "")
		End If
		If truckWeightOrder.Length > 1 Then
			PopulateTicketTransportTareOptions(ddlTransportTareSecondOption, truckWeightOrder(1))
		Else
			PopulateTicketTransportTareOptions(ddlTransportTareSecondOption, "")
		End If
		If truckWeightOrder.Length > 2 Then
			PopulateTicketTransportTareOptions(ddlTransportTareThirdOption, truckWeightOrder(2))
		Else
			PopulateTicketTransportTareOptions(ddlTransportTareThirdOption, "")
		End If
		cbxShowTransport_CheckedChanged(chkShowTicketTransportTareInfo, New EventArgs)
		Boolean.TryParse(GetReceivingWebTicketSettingByOwnerId(connection, ownerId, KaSetting.DefaultReceivingWebTicketSettings.SN_SHOW_DRIVER_ON_TICKET, "True"), cbxShowDriverName.Checked)
		Boolean.TryParse(GetReceivingWebTicketSettingByOwnerId(connection, ownerId, KaSetting.DefaultReceivingWebTicketSettings.SN_SHOW_DRIVER_NUMBER_ON_TICKET, "True"), cbxShowDriverNumber.Checked)
		Boolean.TryParse(GetReceivingWebTicketSettingByOwnerId(connection, ownerId, KaSetting.DefaultReceivingWebTicketSettings.SN_SHOW_EMAIL_ADDRESSES_ON_TICKET, "True"), cbxShowEmailAddress.Checked)
		Boolean.TryParse(GetReceivingWebTicketSettingByOwnerId(connection, ownerId, KaSetting.DefaultReceivingWebTicketSettings.SN_SHOW_LOT_NUMBER_ON_TICKET, False), cbxShowLotNumber.Checked)
		cbxShowDriverNumber.Enabled = cbxShowDriverName.Checked
		tbxTicketLogo.Text = GetReceivingWebTicketSettingByOwnerId(connection, ownerId, KaSetting.DefaultReceivingWebTicketSettings.SN_SHOW_LOGO_PATH_ON_TICKET, "")
		tbxOwnerMessage.Text = GetReceivingWebTicketSettingByOwnerId(connection, ownerId, KaSetting.DefaultReceivingWebTicketSettings.SN_SHOW_OWNER_MESSAGE_ON_TICKET, "")
		tbxOwnerDisclaimer.Text = GetReceivingWebTicketSettingByOwnerId(connection, ownerId, KaSetting.DefaultReceivingWebTicketSettings.SN_SHOW_DISCLAIMER_ON_TICKET, "")
		PopulateWebTicketDensitySettings(connection, ownerId)
		Boolean.TryParse(GetReceivingWebTicketSettingByOwnerId(connection, ownerId, KaSetting.DefaultReceivingWebTicketSettings.SN_SHOW_ALL_CUSTOM_FIELDS_ON_DELIVERY_TICKET, True.ToString()), cbxShowAllCustomFieldsOnReceivingTicket.Checked)
		PopulateWebTicketCustomFieldsShownSettings(connection, ownerId)

		DisplayJavaScriptMessage("ResetScrollPosition;", "resetDotNetScrollPosition();", True)
	End Sub

	Public Shared Function GetReceivingWebTicketSettingByOwnerId(ByVal connection As OleDbConnection, ByVal ownerId As Guid, ByVal settingName As String, ByVal defaultValue As Object) As Object
		'Find the owner specific setting.
		Dim allSettings As ArrayList = KaSetting.GetAll(connection, "name = " & Q("ReceivingWebTicketSetting:" & ownerId.ToString & "/" & settingName) & " and deleted = 0", "")
		If allSettings.Count = 1 Then
			Return allSettings.Item(0).value
		End If

		'If there isn't an owner specific setting, get the All Owners setting, if that doesn't exist either, use the default value.
		Dim retval As String = KaSetting.GetSetting(connection, "ReceivingWebTicketSetting:" & Guid.Empty.ToString & "/" & settingName, defaultValue.ToString)
		Return retval
	End Function

	Private Sub SaveReceivingWebTicketOwnerSettings(ByVal ownerId As Guid)
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)

		Dim webTicketSetting As String = "ReceivingWebTicketSetting:" & ownerId.ToString() & "/"
		Dim list As String = ""
		For Each item As ListItem In cblAdditionalUnitsForTicket.Items
			If item.Selected Then list &= IIf(list.Length > 0, ",", "") & item.Value
		Next
		KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultReceivingWebTicketSettings.SN_ADDITIONAL_TICKET_UNITS, list)
		KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultReceivingWebTicketSettings.SN_SHOW_DATE_ON_TICKET, cbxShowDate.Checked)
		KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultReceivingWebTicketSettings.SN_SHOW_TIME_ON_TICKET, cbxShowTime.Checked)
		KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultReceivingWebTicketSettings.SN_SHOW_OWNER_ON_TICKET, cbxShowOwner.Checked)
		KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultReceivingWebTicketSettings.SN_SHOW_SUPPLIER_ON_TICKET, cbxShowSupplier.Checked)
		KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultReceivingWebTicketSettings.SN_SHOW_CARRIER_ON_TICKET, cbxShowCarrierId.Checked)
		KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultReceivingWebTicketSettings.SN_SHOW_DENSITY_ON_TICKET, cbxShowDensityOnTicket.Checked)

		KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultReceivingWebTicketSettings.SN_SHOW_TRANSPORT_ON_TICKET, cbxShowTransport.Checked)
		If cbxShowTransport.Checked Then
			KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultReceivingWebTicketSettings.SN_SHOW_TRANSPORT_WEIGHTS_ON_TICKET, chkShowTicketTransportTareInfo.Checked)
			Dim truckTGNOrder As String = ""
			If ddlTransportTareFirstOption.SelectedIndex > 0 Then
				truckTGNOrder = ddlTransportTareFirstOption.SelectedValue
				If ddlTransportTareSecondOption.SelectedIndex > 0 Then
					truckTGNOrder &= "-" & ddlTransportTareSecondOption.SelectedValue
					If ddlTransportTareThirdOption.SelectedIndex > 0 Then
						truckTGNOrder &= "-" & ddlTransportTareThirdOption.SelectedValue
					End If
				End If
			End If
			KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultReceivingWebTicketSettings.SN_TARE_GROSS_NET_ORDER, truckTGNOrder)
		End If

		KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultReceivingWebTicketSettings.SN_SHOW_DRIVER_ON_TICKET, cbxShowDriverName.Checked)
		KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultReceivingWebTicketSettings.SN_SHOW_DRIVER_NUMBER_ON_TICKET, cbxShowDriverNumber.Checked)
		KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultReceivingWebTicketSettings.SN_SHOW_EMAIL_ADDRESSES_ON_TICKET, cbxShowEmailAddress.Checked)
		If Tm2Database.SystemItemTraceabilityEnabled Then KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultReceivingWebTicketSettings.SN_SHOW_LOT_NUMBER_ON_TICKET, cbxShowLotNumber.Checked)
		KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultReceivingWebTicketSettings.SN_SHOW_LOGO_PATH_ON_TICKET, tbxTicketLogo.Text)
		KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultReceivingWebTicketSettings.SN_SHOW_OWNER_MESSAGE_ON_TICKET, tbxOwnerMessage.Text)
		KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultReceivingWebTicketSettings.SN_SHOW_DISCLAIMER_ON_TICKET, tbxOwnerDisclaimer.Text)
		SaveWebTicketDensitySettings(webTicketSetting)
		KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultReceivingWebTicketSettings.SN_SHOW_ALL_CUSTOM_FIELDS_ON_DELIVERY_TICKET, cbxShowAllCustomFieldsOnReceivingTicket.Checked)
		SaveWebTicketShowCustomFieldsSetting(webTicketSetting)
		PopulateWebTicketOwnerSettings(ownerId)
	End Sub

	Private Sub PopulateTicketTransportTareOptions(ByRef ddlTransportTareOption As DropDownList, ByVal transportTareValue As String)
		ddlTransportTareOption.Items.Clear()
		Dim valueFound As Boolean = False
		ddlTransportTareOption.Items.Add(New ListItem("Not Shown", ""))
		ddlTransportTareOption.Items.Add(New ListItem("Tare", "T"))
		If transportTareValue = "T" Then ddlTransportTareOption.SelectedIndex = 1 : valueFound = True
		ddlTransportTareOption.Items.Add(New ListItem("Gross", "G"))
		If transportTareValue = "G" Then ddlTransportTareOption.SelectedIndex = 2 : valueFound = True
		ddlTransportTareOption.Items.Add(New ListItem("Net", "N"))
		If transportTareValue = "N" Then ddlTransportTareOption.SelectedIndex = 3 : valueFound = True

		If Not valueFound Then ddlTransportTareOption.SelectedIndex = 0
		transportTareOptionSelectionChanged(ddlTransportTareOption, New EventArgs)
	End Sub

	Private Sub DeleteWebTicketOwnerSettings(ByVal ownerId As Guid)
		'Delete will set all the ticket settings to their 'default' values
		Dim c As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim allSettings As ArrayList = KaSetting.GetAll(c, "name like 'ReceivingWebTicketSetting:" & ownerId.ToString & "%' and deleted=0", "")
		For Each setting As KaSetting In allSettings
			Tm2Database.ExecuteNonQuery(c, "Delete from settings where id = " & Q(setting.Id))
		Next
		PopulateWebTicketOwnerSettings(ownerId)
	End Sub

	Private Sub ddlWebTicketOwner_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles ddlWebTicketOwner.SelectedIndexChanged
		PopulateWebTicketOwnerSettings(Guid.Parse(ddlWebTicketOwner.SelectedValue))
	End Sub

	Protected Sub btnSaveOwnerWebTicketSettings_Click(sender As Object, e As EventArgs) Handles btnSaveOwnerWebTicketSettings.Click
		SaveReceivingWebTicketOwnerSettings(Guid.Parse(ddlWebTicketOwner.SelectedValue))
	End Sub

	Protected Sub btbDeleteOwnerWebTicketSettings_Click(sender As Object, e As EventArgs) Handles btnDeleteOwnerWebTicketSettings.Click
		DeleteWebTicketOwnerSettings(Guid.Parse(ddlWebTicketOwner.SelectedValue))
	End Sub
#End Region

	Protected Sub cbxShowTransport_CheckedChanged(sender As Object, e As EventArgs) Handles cbxShowTransport.CheckedChanged
		chkShowTicketTransportTareInfo.Enabled = cbxShowTransport.Checked
		tblTransportTareOptions.Visible = cbxShowTransport.Checked
	End Sub

	Private Sub transportTareOptionSelectionChanged(sender As Object, e As System.EventArgs) Handles chkShowTicketTransportTareInfo.CheckedChanged, ddlTransportTareFirstOption.SelectedIndexChanged, ddlTransportTareSecondOption.SelectedIndexChanged
		rowTransportTareFirstOption.Visible = chkShowTicketTransportTareInfo.Checked
		rowTransportTareSecondOption.Visible = chkShowTicketTransportTareInfo.Checked AndAlso (ddlTransportTareFirstOption.SelectedIndex > 0)
		rowTransportTareThirdOption.Visible = chkShowTicketTransportTareInfo.Checked AndAlso (ddlTransportTareFirstOption.SelectedIndex > 0) AndAlso (ddlTransportTareSecondOption.SelectedIndex > 0)
	End Sub

	Protected Sub chkDriverName_CheckedChanged(sender As Object, e As EventArgs) Handles cbxShowDriverName.CheckedChanged
		cbxShowDriverNumber.Enabled = cbxShowDriverName.Checked
	End Sub

	Private Sub PopulateAdditionalUnitsList()
		cblAdditionalUnitsForTicket.Items.Clear()
		For Each n As KaUnit In KaUnit.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
			If n.BaseUnit <> KaUnit.Unit.Pulses AndAlso n.BaseUnit <> KaUnit.Unit.Seconds Then
				cblAdditionalUnitsForTicket.Items.Add(New ListItem(n.Name, n.Id.ToString()))
			End If
		Next
	End Sub

	Private Sub PopulateWebTicketDensityUnits()
		ddlWebTicketDensityMass.Items.Clear()
		ddlWebTicketDensityVolume.Items.Clear()
		For Each n As KaUnit In KaUnit.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "abbreviation ASC")
			If Not KaUnit.IsTime(n.BaseUnit) Then ' ignore time unit of measures
				If KaUnit.IsWeight(n.BaseUnit) Then ' add to the mass unit list
					ddlWebTicketDensityMass.Items.Add(New ListItem(n.Abbreviation, n.Id.ToString()))
				Else ' add to the volume unit list
					ddlWebTicketDensityVolume.Items.Add(New ListItem(n.Abbreviation, n.Id.ToString()))
				End If
			End If
		Next
	End Sub

	Protected Sub lstWebTicketDensityList_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lstWebTicketDensityList.SelectedIndexChanged
		UpdateWebTicketDensityRemoveEnabled()
		UpdateWebTicketPrecisionVisible()
	End Sub

	Private Function IsWebTicketDensityInList(massUnitId As Guid, volumeUnitId As Guid) As Boolean
		For Each item As ListItem In lstWebTicketDensityList.Items
			Dim parts() As String = item.Value.Split("|") ' should produce 3 parts (<mass Guid>|<volume Guid>|precision), no need to check array dimensions as long as the routine that populates the list only adds items that conform the format
			If Guid.Parse(parts(0)) = massUnitId AndAlso Guid.Parse(parts(1)) = volumeUnitId Then Return True
		Next
		Return False
	End Function

	Private Sub UpdateWebTicketDensityAddEnabled()
		btnWebTicketDensityAdd.Enabled = Not IsWebTicketDensityInList(Guid.Parse(ddlWebTicketDensityMass.SelectedValue), Guid.Parse(ddlWebTicketDensityVolume.SelectedValue)) AndAlso _currentUserPermission(_currentTableName).Edit
	End Sub

	Private Sub UpdateWebTicketDensityRemoveEnabled()
		btnWebTicketDensityRemove.Enabled = lstWebTicketDensityList.SelectedIndex >= 0 AndAlso _currentUserPermission(_currentTableName).Edit
	End Sub

	Private Sub UpdateWebTicketPrecisionVisible()
		trWebTicketDensityPrecisionControls.Visible = lstWebTicketDensityList.SelectedIndex >= 0
	End Sub

	Protected Sub ddlWebTicketDensityUnit_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlWebTicketDensityMass.SelectedIndexChanged, ddlWebTicketDensityVolume.SelectedIndexChanged
		UpdateWebTicketDensityAddEnabled()
	End Sub

	Protected Sub btnWebTicketDensityAdd_Click(sender As Object, e As EventArgs) Handles btnWebTicketDensityAdd.Click
		Dim text As String = String.Format("{0}/{1} 0.00", ddlWebTicketDensityMass.SelectedItem.Text, ddlWebTicketDensityVolume.SelectedItem.Text)
		Dim value As String = String.Format("{0}|{1}|0.00", ddlWebTicketDensityMass.SelectedValue, ddlWebTicketDensityVolume.SelectedValue)
		Dim item As New ListItem(text, value)
		lstWebTicketDensityList.Items.Add(item)
		lstWebTicketDensityList.SelectedIndex = lstWebTicketDensityList.Items.Count - 1 ' select the item we just added
		UpdateWebTicketDensityAddEnabled()
		UpdateWebTicketDensityRemoveEnabled()
		UpdateWebTicketPrecisionVisible()
	End Sub

	Protected Sub btnWebTicketDensityRemove_Click(sender As Object, e As EventArgs) Handles btnWebTicketDensityRemove.Click
		lstWebTicketDensityList.Items.RemoveAt(lstWebTicketDensityList.SelectedIndex)
		UpdateWebTicketDensityAddEnabled()
		UpdateWebTicketDensityRemoveEnabled()
		UpdateWebTicketPrecisionVisible()
	End Sub

	Protected Sub btnWebTicketDensityAddWhole_Click(sender As Object, e As EventArgs) Handles btnWebTicketDensityAddWhole.Click
		Dim parts() As String = lstWebTicketDensityList.SelectedValue.Split("|") ' should produce 3 parts (<mass Guid>|<volume Guid>|precision), no need to check array dimensions as long as the routine that populates the list only adds items that conform the format
		Dim whole As UInteger = 0
		Dim fractional As UInteger = UInteger.MaxValue
		KaUnit.GetPrecisionDigits(parts(2), whole, fractional)
		Dim precision As String = KaUnit.GetPrecisionString(whole + 1, fractional, ",", 0)
		lstWebTicketDensityList.Items(lstWebTicketDensityList.SelectedIndex).Value = String.Format("{0}|{1}|{2}", parts(0), parts(1), precision)
		parts = lstWebTicketDensityList.Items(lstWebTicketDensityList.SelectedIndex).Text.Split(" ")
		lstWebTicketDensityList.Items(lstWebTicketDensityList.SelectedIndex).Text = String.Format("{0} {1}", parts(0), precision)
	End Sub

	Protected Sub btnWebTicketDensityRemoveWhole_Click(sender As Object, e As EventArgs) Handles btnWebTicketDensityRemoveWhole.Click
		Dim parts() As String = lstWebTicketDensityList.SelectedValue.Split("|") ' should produce 3 parts (<mass Guid>|<volume Guid>|precision), no need to check array dimensions as long as the routine that populates the list only adds items that conform the format
		Dim whole As UInteger = 0
		Dim fractional As UInteger = UInteger.MaxValue
		KaUnit.GetPrecisionDigits(parts(2), whole, fractional)
		Dim precision As String = KaUnit.GetPrecisionString(Math.Max(whole - 1, 0), fractional, ",", 0)
		lstWebTicketDensityList.Items(lstWebTicketDensityList.SelectedIndex).Value = String.Format("{0}|{1}|{2}", parts(0), parts(1), precision)
		parts = lstWebTicketDensityList.Items(lstWebTicketDensityList.SelectedIndex).Text.Split(" ")
		lstWebTicketDensityList.Items(lstWebTicketDensityList.SelectedIndex).Text = String.Format("{0} {1}", parts(0), precision)
	End Sub

	Protected Sub btnWebTicketDensityAddFractional_Click(sender As Object, e As EventArgs) Handles btnWebTicketDensityAddFractional.Click
		Dim parts() As String = lstWebTicketDensityList.SelectedValue.Split("|") ' should produce 3 parts (<mass Guid>|<volume Guid>|precision), no need to check array dimensions as long as the routine that populates the list only adds items that conform the format
		Dim whole As UInteger = 0
		Dim fractional As UInteger = UInteger.MaxValue
		KaUnit.GetPrecisionDigits(parts(2), whole, fractional)
		Dim precision As String = KaUnit.GetPrecisionString(whole, fractional + 1, ",", 0)
		lstWebTicketDensityList.Items(lstWebTicketDensityList.SelectedIndex).Value = String.Format("{0}|{1}|{2}", parts(0), parts(1), precision)
		parts = lstWebTicketDensityList.Items(lstWebTicketDensityList.SelectedIndex).Text.Split(" ")
		lstWebTicketDensityList.Items(lstWebTicketDensityList.SelectedIndex).Text = String.Format("{0} {1}", parts(0), precision)
	End Sub

	Protected Sub btnWebTicketDensityRemoveFractional_Click(sender As Object, e As EventArgs) Handles btnWebTicketDensityRemoveFractional.Click
		Dim parts() As String = lstWebTicketDensityList.SelectedValue.Split("|") ' should produce 3 parts (<mass Guid>|<volume Guid>|precision), no need to check array dimensions as long as the routine that populates the list only adds items that conform the format
		Dim whole As UInteger = 0
		Dim fractional As UInteger = UInteger.MaxValue
		KaUnit.GetPrecisionDigits(parts(2), whole, fractional)
		Dim precision As String = KaUnit.GetPrecisionString(whole, Math.Max(fractional - 1, 0), ",", 0)
		lstWebTicketDensityList.Items(lstWebTicketDensityList.SelectedIndex).Value = String.Format("{0}|{1}|{2}", parts(0), parts(1), precision)
		parts = lstWebTicketDensityList.Items(lstWebTicketDensityList.SelectedIndex).Text.Split(" ")
		lstWebTicketDensityList.Items(lstWebTicketDensityList.SelectedIndex).Text = String.Format("{0} {1}", parts(0), precision)
	End Sub

	Private Sub PopulateWebTicketDensitySettings(connection As OleDbConnection, ownerId As Guid)
		lstWebTicketDensityList.Items.Clear()
		Dim unitAbbreviations As New Dictionary(Of Guid, String)
		Dim densityUnitSettings() As String = GetReceivingWebTicketSettingByOwnerId(connection, ownerId, KaSetting.DefaultReceivingWebTicketSettings.SN_DENSITY_UNIT_OF_PRECISION, "").Split(",")
		For Each densityUnitPrecision As String In densityUnitSettings
			Dim parts() As String = densityUnitPrecision.Split("|")
			If parts.Length = 3 Then
				Try ' to parse the unit precision unit IDs
					Dim massUnitId As Guid = Guid.Parse(parts(0))
					Dim volumeUnitId As Guid = Guid.Parse(parts(1))
					Dim text As String
					Try ' to get the unit abbreviation for the mass unit from the dictionary...
						text = unitAbbreviations(massUnitId)
					Catch ex As KeyNotFoundException ' the abbreviation wasn't in the dictionary...
						Dim abbreviation As String = New KaUnit(connection, massUnitId).Abbreviation
						unitAbbreviations(massUnitId) = abbreviation
						text = abbreviation
					End Try
					Try ' to get the unit abbreviation for the volume unit from the dictionary...
						text &= "/" & unitAbbreviations(volumeUnitId)
					Catch ex As KeyNotFoundException ' the abbreviation wasn't in the dictionary...
						Dim abbreviation As String = New KaUnit(connection, volumeUnitId).Abbreviation
						unitAbbreviations(volumeUnitId) = abbreviation
						text &= "/" & abbreviation
					End Try
					text &= " " & parts(2)
					lstWebTicketDensityList.Items.Add(New ListItem(text, densityUnitPrecision))
				Catch ex As Exception ' the density unit precision setting wasn't formatted correctly...
				End Try
			End If
		Next
		UpdateWebTicketDensityAddEnabled()
		UpdateWebTicketDensityRemoveEnabled()
		UpdateWebTicketPrecisionVisible()
	End Sub

	Private Sub SaveWebTicketDensitySettings(webTicketSetting As String)
		Dim value As String = ""
		For Each item As ListItem In lstWebTicketDensityList.Items
			value &= IIf(value.Length > 0, ",", "") & item.Value
		Next
		KaSetting.WriteSetting(GetUserConnection(_currentUser.Id), webTicketSetting & KaSetting.DefaultReceivingWebTicketSettings.SN_DENSITY_UNIT_OF_PRECISION, value)
	End Sub

	Private Sub PopulateWebTicketCustomFieldsShownSettings(connection As OleDbConnection, ownerId As Guid)
		cblShowCustomFieldsOnReceivingTicket.Items.Clear()
		Dim customFieldsShownList As New List(Of String)
		customFieldsShownList.AddRange(GetReceivingWebTicketSettingByOwnerId(connection, ownerId, KaSetting.DefaultReceivingWebTicketSettings.SN_CUSTOM_FIELDS_ON_DELIVERY_TICKET, "").Split(","))
		Dim validTicketTables As String = Q(KaReceivingTicket.TABLE_NAME)

		For Each customTicketField As KaCustomField In KaCustomField.GetAll(connection, String.Format("{0} = {1} AND {2} IN ({3})", KaCustomField.FN_DELETED, Q(False), KaCustomField.FN_TABLE_NAME, validTicketTables), KaCustomField.FN_FIELD_NAME)
			cblShowCustomFieldsOnReceivingTicket.Items.Add(New ListItem(Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(customTicketField.TableName.Replace(KaLocation.TABLE_NAME, "Facilities").Replace("_", " ") & ": " & customTicketField.FieldName), customTicketField.Id.ToString()))
			cblShowCustomFieldsOnReceivingTicket.Items(cblShowCustomFieldsOnReceivingTicket.Items.Count - 1).Selected = (customFieldsShownList.Contains(customTicketField.Id.ToString()))
		Next
        pnlReceivingTicketCustomFieldsAssigned.Visible = (cblShowCustomFieldsOnReceivingTicket.Items.Count > 0)
        cbxShowAllCustomFieldsOnReceivingTicket_CheckedChanged(Nothing, Nothing)
    End Sub

	Private Sub SaveWebTicketShowCustomFieldsSetting(ByVal webTicketSetting As String)
		Dim customFieldsShown As String = ""
		For Each customTicketField As ListItem In cblShowCustomFieldsOnReceivingTicket.Items
			If customTicketField.Selected Then
				If customFieldsShown.Length > 0 Then customFieldsShown &= ","
				customFieldsShown &= customTicketField.Value
			End If
		Next
		KaSetting.WriteSetting(GetUserConnection(_currentUser.Id), webTicketSetting & KaSetting.DefaultReceivingWebTicketSettings.SN_CUSTOM_FIELDS_ON_DELIVERY_TICKET, customFieldsShown)
	End Sub

	Protected Sub cbxShowDate_CheckedChanged(sender As Object, e As EventArgs) Handles cbxShowDate.CheckedChanged
		cbxShowTime.Enabled = cbxShowDate.Checked
	End Sub

	Protected Sub cbxShowAllCustomFieldsOnReceivingTicket_CheckedChanged(sender As Object, e As EventArgs) Handles cbxShowAllCustomFieldsOnReceivingTicket.CheckedChanged
		cblShowCustomFieldsOnReceivingTicket.Enabled = Not cbxShowAllCustomFieldsOnReceivingTicket.Checked
	End Sub

	Protected Sub ToolkitScriptManager1_AsyncPostBackError(ByVal sender As Object, ByVal e As System.Web.UI.AsyncPostBackErrorEventArgs)
		ToolkitScriptManager1.AsyncPostBackErrorMessage = e.Exception.Message
	End Sub

	Private Sub DisplayJavaScriptMessage(key As String, script As String)
		DisplayJavaScriptMessage(key, script, False)
	End Sub

	Private Sub DisplayJavaScriptMessage(key As String, script As String, addScriptTags As Boolean)
		ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), key, script, addScriptTags)
	End Sub
End Class