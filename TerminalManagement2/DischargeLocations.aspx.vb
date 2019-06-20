Imports KahlerAutomation.KaTm2Database
Imports System.Environment
Imports System.Data.OleDb

Public Class DischargeLocations : Inherits System.Web.UI.Page
#Region " Web Form Designer Generated Code "
	'This call is required by the Web Form Designer.
	<System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()

	End Sub

	Protected WithEvents btnSave As Button
	Protected WithEvents btnDelete As Button
	Protected WithEvents lblStatus As Label
	Protected WithEvents ddlDischargeLocations As DropDownList
	Protected WithEvents tbxName As TextBox
	Protected WithEvents tbxFillLimit As TextBox
	Protected WithEvents ddlFillLimitUnit As DropDownList
	Protected WithEvents tbxSecondaryFillLimit As TextBox
	Protected WithEvents ddlSecondaryFillLimitUnit As DropDownList
	Protected WithEvents cbxConfirmEmpty As CheckBox
	Protected WithEvents cbxAcceptsBlends As CheckBox
	Protected WithEvents btnClear As Button
	Protected WithEvents ddlPanel As DropDownList
	Protected WithEvents btnAddPanel As Button
	Protected WithEvents lstPanels As ListBox
	Protected WithEvents btnRemovePanel As Button
	Protected WithEvents cblDiverters As CheckBoxList
	Protected WithEvents tbxPurgeTimeMultiplier As TextBox
	Protected WithEvents tbxAnticipationMultiplier As TextBox
	Protected WithEvents lblLastTicket As Label
	Protected WithEvents btnSavePanel As Button
	Protected WithEvents ddlBay As DropDownList
	Protected WithEvents lblPanelStatus As Label
	Protected WithEvents pnlMain As Panel
	Protected WithEvents cbxFinalPanelAutoDischarge As CheckBox
	Protected WithEvents ddlFacilityFilter As DropDownList
	Protected WithEvents ToolkitScriptManager1 As Global.System.Web.UI.ScriptManager
	Protected WithEvents PleaseWaitPopup As Global.AjaxControlToolkit.ModalPopupExtender
	Protected WithEvents ddlStorageLocations As DropDownList
	Protected WithEvents btnAddStorageLocations As Button
	Protected WithEvents lstStorageLocations As ListBox
	Protected WithEvents btnRemoveStorageLocations As Button
	Protected WithEvents pnlPanels As Panel
	Protected WithEvents pnlStorageLocations As Panel

	Private Sub Page_Init(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Init
		'CODEGEN: This method call is required by the Web Form Designer
		'Do not modify it using the code editor.
		InitializeComponent()
	End Sub
#End Region

	Private _currentUser As KaUser
	Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
	Private _currentTableName As String = KaPanel.TABLE_NAME

#Region "Events"
	Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
		Response.Cache.SetCacheability(HttpCacheability.NoCache)
		_currentUser = Utilities.GetUser(Me)
		_currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Panels")
		If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")
		lblStatus.Text = ""
		lblPanelStatus.Text = ""
		If Not Page.IsPostBack Then
			SetTextboxMaxLengths()
			PopulateLocationList()
			Try
				ddlFacilityFilter.SelectedValue = KaSetting.GetSetting(GetUserConnection(_currentUser.Id), "PanelsPage:" & _currentUser.Id.ToString & "/LastFacilityUsed", Guid.Empty.ToString())
			Catch ex As ArgumentOutOfRangeException
				ddlFacilityFilter.SelectedValue = Guid.Empty.ToString()
			End Try
			ddlFacilityFilter_SelectedIndexChanged(ddlFacilityFilter, New EventArgs())
			PopulateUnitList()
			PopulateBayList()
			SetControlUsabilityFromPermissions()
			If Page.Request("DischargeLocationId") IsNot Nothing Then
				Try
					ddlDischargeLocations.SelectedValue = Page.Request("DischargeLocationId")
				Catch ex As ArgumentOutOfRangeException
					ddlDischargeLocations.SelectedIndex = 0
				End Try
			End If
			ddlDischargeLocations_SelectedIndexChanged(ddlDischargeLocations, New EventArgs())
			Utilities.ConfirmBox(Me.btnDelete, "Are you sure you want to delete this discharge location?") ' Delete confirmation box setup
			Utilities.SetFocus(tbxName, Me) ' set focus to the first textbox on the page
		End If
	End Sub

	Protected Sub ddlDischargeLocations_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles ddlDischargeLocations.SelectedIndexChanged
		Dim dischargeLocationId As Guid = Guid.Parse(ddlDischargeLocations.SelectedValue)
		PopulateDischargeLocationInformation(dischargeLocationId)
		SetControlUsabilityFromPermissions()
		Dim panelId As Guid = Guid.Parse(ddlPanel.SelectedValue)
		btnAddPanel.Enabled = dischargeLocationId <> Guid.Empty AndAlso panelId <> Guid.Empty AndAlso Not IsPanelInList(panelId)
	End Sub

	Protected Sub ddlPanel_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles ddlPanel.SelectedIndexChanged
		Dim dischargeLocationId As Guid = Guid.Parse(ddlDischargeLocations.SelectedValue)
		Dim panelId As Guid = Guid.Parse(ddlPanel.SelectedValue)
		btnAddPanel.Enabled = dischargeLocationId <> Guid.Empty AndAlso panelId <> Guid.Empty AndAlso Not IsPanelInList(panelId)
	End Sub

	Protected Sub btnAddPanel_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnAddPanel.Click
		Dim pathLoop As String = "" ' make sure the addition of this panel to this discharge location doesn't create a infinite loop
		If Not CreatesDischargeLocationLoop(pathLoop) Then
			Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
			Dim dischargeLocationPanelSettings As New KaDischargeLocationPanelSettings()
			dischargeLocationPanelSettings.PanelId = Guid.Parse(ddlPanel.SelectedValue)
			dischargeLocationPanelSettings.DischargeLocationId = Guid.Parse(ddlDischargeLocations.SelectedValue)
			dischargeLocationPanelSettings.SqlInsert(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
			lstPanels.Items.Add(New ListItem(ddlPanel.SelectedItem.Text, dischargeLocationPanelSettings.Id.ToString())) ' add these panel settings to the list
			lstPanels.SelectedValue = dischargeLocationPanelSettings.Id.ToString() ' automatically select these panel settings
			PopulateDischargeLocationPanelSettings()
			UpdatePanelSettingsControls(True)
		Else
			DisplayJavaScriptMessage("InvalidProductFlow", Utilities.JsAlert("Could not add panel to discharge location because it would result in a product flow loop (" & pathLoop & ")"))
		End If
	End Sub

	Protected Sub lstPanels_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles lstPanels.SelectedIndexChanged
		If lstPanels.SelectedItem IsNot Nothing Then
			PopulateDischargeLocationPanelSettings()
			UpdatePanelSettingsControls(True)
		End If
	End Sub

	Protected Sub btnSavePanel_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnSavePanel.Click
		If Not IsNumeric(tbxPurgeTimeMultiplier.Text) Then ' the purge time multiplier isn't numeric
			DisplayJavaScriptMessage("InvalidCharPurgeTimeMultiplier", Utilities.JsAlert("Please enter a numeric value for the purge time multiplier."))
			Exit Sub
		End If
		If Double.Parse(tbxPurgeTimeMultiplier.Text) < 0 Then
			DisplayJavaScriptMessage("InvalidPurgeTimeMultiplier", Utilities.JsAlert("Please enter a value greater than zero for the purge time multiplier."))
			Exit Sub
		End If
		If Not IsNumeric(tbxAnticipationMultiplier.Text) Then ' the anticipation multiplier isn't numeric
			DisplayJavaScriptMessage("InvalidCharAnticipationMultiplier", Utilities.JsAlert("Please enter a numeric value for the anticipation multiplier."))
			Exit Sub
		End If
		If Double.Parse(tbxAnticipationMultiplier.Text) < 0 Then
			DisplayJavaScriptMessage("InvalidAnticipationMultiplier", Utilities.JsAlert("Please enter a value greater than zero for the anticipation multiplier."))
			Exit Sub
		End If
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim conditions As String = String.Format("id={0}", Q(lstPanels.SelectedValue))
		Dim dischargeLocationPanelSettings As New KaDischargeLocationPanelSettings(connection, Guid.Parse(lstPanels.SelectedValue))
		dischargeLocationPanelSettings.PurgeTimeMultiplier = Double.Parse(tbxPurgeTimeMultiplier.Text)
		dischargeLocationPanelSettings.AnticipationMultiplier = Double.Parse(tbxAnticipationMultiplier.Text)
		dischargeLocationPanelSettings.Diverters = 0 ' record the diverter settings
		For i As Integer = 0 To cblDiverters.Items.Count - 1
			dischargeLocationPanelSettings.Diverters += IIf(cblDiverters.Items(i).Selected, 2 ^ i, 0)
		Next
		dischargeLocationPanelSettings.FinalPanelAutoDischarge = cbxFinalPanelAutoDischarge.Checked
		dischargeLocationPanelSettings.SqlUpdate(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
		lstPanels.SelectedItem.Value = dischargeLocationPanelSettings.Id.ToString()
		lblPanelStatus.Text = "Panel settings saved"
	End Sub

	Protected Sub btnRemovePanel_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnRemovePanel.Click
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim dischargeLocationPanelSettings As New KaDischargeLocationPanelSettings(connection, Guid.Parse(lstPanels.SelectedValue))
		dischargeLocationPanelSettings.Deleted = True
		dischargeLocationPanelSettings.SqlUpdate(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
		lstPanels.Items.RemoveAt(lstPanels.SelectedIndex) ' remove the panel from the list
		UpdatePanelSettingsControls(False)
		lblPanelStatus.Text = "Panel removed"
	End Sub

	Protected Sub btnSave_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnSave.Click
		Dim dl As KaDischargeLocation = New KaDischargeLocation()
		With dl
			.Id = Guid.Parse(ddlDischargeLocations.SelectedValue)
			ValidateSettings(.Id)

			If .Id <> Guid.Empty Then .SqlSelect(GetUserConnection(_currentUser.Id))

			If Tm2Database.SystemItemTraceabilityEnabled Then
				Dim currentStorageLocations As Dictionary(Of Guid, KaDischargeLocationStorageLocation) = New Dictionary(Of Guid, KaDischargeLocationStorageLocation)
				For Each sl As KaDischargeLocationStorageLocation In .StorageLocations
					currentStorageLocations.Add(sl.StorageLocationId, sl)
				Next

				Dim storageLocationIds As List(Of Guid) = AssignedStorageLocationIds()
				For Each slId As Guid In storageLocationIds
					If Not currentStorageLocations.ContainsKey(slId) Then currentStorageLocations.Add(slId, New KaDischargeLocationStorageLocation() With {.StorageLocationId = slId})
				Next
				.StorageLocations.Clear()
				For Each slId As Guid In currentStorageLocations.Keys
					.StorageLocations.Add(currentStorageLocations(slId))
				Next
			Else
				' Leave the list as-is
			End If
			.ConfirmEmpty = cbxConfirmEmpty.Checked
			.AcceptsBlends = cbxAcceptsBlends.Checked
			.FillLimit = tbxFillLimit.Text
			.FillLimit2 = tbxSecondaryFillLimit.Text
			.FillLimitUnitId = Guid.Parse(ddlFillLimitUnit.SelectedValue)
			.FillLimit2UnitId = Guid.Parse(ddlSecondaryFillLimitUnit.SelectedValue)
			.BayId = Guid.Parse(ddlBay.SelectedValue)
			.Name = tbxName.Text
			If .Id <> Guid.Empty Then
				.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
				lblStatus.Text = "Discharge location updated successfully."
			Else
				.SqlInsert(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
				lblStatus.Text = "Discharge location added successfully."
			End If
			PopulateDischargeLocationList()
			ddlDischargeLocations.SelectedValue = .Id.ToString()
		End With
		ddlDischargeLocations_SelectedIndexChanged(ddlDischargeLocations, New EventArgs())
	End Sub

	Private Function ValidateSettings(dischargeLocationId As Guid) As Boolean
		If tbxName.Text.Length = 0 Then
			DisplayJavaScriptMessage("InvalidName", Utilities.JsAlert("Please enter a name for the discharge location."))
			Return False
		ElseIf KaDischargeLocation.GetAll(GetUserConnection(_currentUser.Id), $"{KaDischargeLocation.FN_DELETED} = 0 AND {KaDischargeLocation.FN_ID} <> {Q(dischargeLocationId)} AND {KaDischargeLocation.FN_NAME} = {Q(tbxName.Text)}", "").Count > 0 Then
			DisplayJavaScriptMessage("InvalidDuplicateName", Utilities.JsAlert("Discharge location named " & tbxName.Text & " already exists. Please enter a unique name for the discharge location."))
			Return False
		ElseIf Not IsNumeric(tbxFillLimit.Text) Then
			DisplayJavaScriptMessage("InvalidCharFillLimit", Utilities.JsAlert("Please enter a numeric value for the fill limit."))
			Return False
		ElseIf Not IsNumeric(tbxSecondaryFillLimit.Text) Then
			DisplayJavaScriptMessage("InvalidCharSecondaryFillLimit", Utilities.JsAlert("Please enter a numeric value for the secondary fill limit."))
			Return False
		ElseIf Double.Parse(tbxFillLimit.Text) > 0 AndAlso Guid.Parse(ddlFillLimitUnit.SelectedValue) = Guid.Empty Then
			DisplayJavaScriptMessage("InvalidFillLimitUnit", Utilities.JsAlert("Please select a unit for the fill limit."))
			Return False
		ElseIf Double.Parse(tbxSecondaryFillLimit.Text) > 0 AndAlso Guid.Parse(ddlSecondaryFillLimitUnit.SelectedValue) = Guid.Empty Then
			DisplayJavaScriptMessage("InvalidSecondaryFillLimitUnit", Utilities.JsAlert("Please select a unit for the secondary fill limit."))
			Return False
		End If
		Return True
	End Function

	Protected Sub btnDelete_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnDelete.Click
		With New KaDischargeLocation(GetUserConnection(_currentUser.Id), Guid.Parse(ddlDischargeLocations.SelectedValue))
			.Deleted = True
			.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
			lblStatus.Text = "Discharge location deleted successfully."
		End With
		PopulateDischargeLocationList()
		PopulateDischargeLocationInformation(Guid.Empty)
		UpdatePanelSettingsControls(False)
		btnDelete.Enabled = False
	End Sub

	Protected Sub btnClear_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnClear.Click
		With New KaDischargeLocation(GetUserConnection(_currentUser.Id), Guid.Parse(ddlDischargeLocations.SelectedValue))
			.LastTicketId = Guid.Empty
			.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
		End With
	End Sub
#End Region

	Private Function IsPanelInList(ByVal panelId As Guid) As Boolean
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		For Each item As ListItem In lstPanels.Items
			If New KaDischargeLocationPanelSettings(connection, Guid.Parse(item.Value)).PanelId = panelId Then Return True
		Next
		Return False
	End Function

	Private Sub PopulateDischargeLocationPanelSettings()
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim dischargeLocationPanelSettings As KaDischargeLocationPanelSettings
		Try
			dischargeLocationPanelSettings = New KaDischargeLocationPanelSettings(connection, Guid.Parse(lstPanels.SelectedValue))
		Catch ex As Exception
			dischargeLocationPanelSettings = New KaDischargeLocationPanelSettings()
		End Try
		For i As Integer = 0 To 7
			cblDiverters.Items(i).Selected = ((2 ^ i) And dischargeLocationPanelSettings.Diverters) <> 0
		Next
		tbxPurgeTimeMultiplier.Text = dischargeLocationPanelSettings.PurgeTimeMultiplier
		tbxAnticipationMultiplier.Text = dischargeLocationPanelSettings.AnticipationMultiplier
		cbxFinalPanelAutoDischarge.Checked = dischargeLocationPanelSettings.FinalPanelAutoDischarge
	End Sub

	Private Sub UpdatePanelSettingsControls(ByVal enabled As Boolean)
		Dim panelId As Guid = Guid.Parse(ddlPanel.SelectedValue)
		btnAddPanel.Enabled = panelId <> Guid.Empty AndAlso Not IsPanelInList(panelId)
		btnRemovePanel.Enabled = lstPanels.SelectedItem IsNot Nothing
		btnSavePanel.Enabled = enabled
		cblDiverters.Enabled = enabled
		tbxPurgeTimeMultiplier.Enabled = enabled
		tbxAnticipationMultiplier.Enabled = enabled
		cbxFinalPanelAutoDischarge.Enabled = enabled

		If Not enabled Then
			tbxPurgeTimeMultiplier.Text = "1"
			tbxAnticipationMultiplier.Text = "1"
			For Each i As ListItem In cblDiverters.Items
				i.Selected = False
			Next
		End If
	End Sub

	Private Sub PopulateBayList()
		ddlBay.Items.Clear()
		ddlBay.Items.Add(New ListItem("None", Guid.Empty.ToString()))
		For Each bay As KaBay In KaBay.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
			ddlBay.Items.Add(New ListItem(bay.Name, bay.Id.ToString()))
		Next
	End Sub

	Private Sub PopulateDischargeLocationInformation(ByVal dischargeLocationId As Guid)
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		With New KaDischargeLocation()
			.Id = dischargeLocationId
			If dischargeLocationId <> Guid.Empty Then .SqlSelect(connection)
			PopulatePanelList(.Id)
			PopulateStorageLocationsAssigned(.Id)
			PopulateStorageLocationList(.Id)
			tbxName.Text = .Name
			tbxFillLimit.Text = .FillLimit
			tbxSecondaryFillLimit.Text = .FillLimit2
			Try
				ddlFillLimitUnit.SelectedValue = .FillLimitUnitId.ToString()
			Catch ex As ArgumentOutOfRangeException
				ddlFillLimitUnit.SelectedValue = Guid.Empty.ToString()
				DisplayJavaScriptMessage("InvalidFillLimitUnitId", Utilities.JsAlert("Record not found in units where ID = " & .FillLimitUnitId.ToString() & ". Fill limit unit not set."))
			End Try
			Try
				ddlSecondaryFillLimitUnit.SelectedValue = .FillLimit2UnitId.ToString()
			Catch ex As ArgumentOutOfRangeException
				ddlSecondaryFillLimitUnit.SelectedValue = Guid.Empty.ToString()
				DisplayJavaScriptMessage("InvalidSecondaryFillLimitId", Utilities.JsAlert("Record not found in units where ID = " & .FillLimit2UnitId.ToString() & ". Secondary fill limit unit not set."))
			End Try
			Try
				ddlBay.SelectedValue = .BayId.ToString()
			Catch ex As ArgumentOutOfRangeException
				ddlBay.SelectedValue = Guid.Empty.ToString()
				DisplayJavaScriptMessage("InvalidBayId", Utilities.JsAlert("Record not found in bays where ID = " & .BayId.ToString() & ". Bay not set."))
			End Try
			cbxConfirmEmpty.Checked = .ConfirmEmpty
			cbxAcceptsBlends.Checked = .AcceptsBlends
			Try
				If .LastTicketId <> Guid.Empty Then
					lblLastTicket.Text = New KaTicket(connection, .LastTicketId).Number
					btnClear.Enabled = True
				Else
					lblLastTicket.Text = ""
					btnClear.Enabled = False
				End If
			Catch ex As RecordNotFoundException
				lblLastTicket.Text = ""
				btnClear.Enabled = False
				DisplayJavaScriptMessage("InvalidTicketId", Utilities.JsAlert("Record not found in tickets where ID = " & .LastTicketId.ToString() & ". Last ticket number not displayed."))
			End Try
			lstPanels.Items.Clear()
			If dischargeLocationId <> Guid.Empty Then
				Dim conditions As String = "deleted=0 AND discharge_location_id=" & Q(dischargeLocationId)
				For Each dischargeLocationPanelSettings As KaDischargeLocationPanelSettings In KaDischargeLocationPanelSettings.GetAll(connection, conditions, "")
					Try ' to add the panel settings to the list using the panel name...
						Dim panelInfo As New KaPanel(connection, dischargeLocationPanelSettings.PanelId)
						lstPanels.Items.Add(New ListItem(panelInfo.Name & IIf(panelInfo.Deleted, " (deleted)", ""), dischargeLocationPanelSettings.Id.ToString()))
						If panelInfo.Deleted Then
							DisplayJavaScriptMessage("InvalidPanelDeleted", Utilities.JsAlert("The panel " & panelInfo.Name & " has been deleted.  Please remove it from the panel list."))
						End If
					Catch ex As RecordNotFoundException ' the panel wasn't found in the database...
						lstPanels.Items.Add(New ListItem("Unknown panel", dischargeLocationPanelSettings.Id.ToString()))
						DisplayJavaScriptMessage("InvalidPanelId", Utilities.JsAlert("Record not found in panels where ID = " & dischargeLocationPanelSettings.PanelId.ToString() & ". Please remove it from the panel list."))
					End Try
				Next
			End If
			UpdatePanelSettingsControls(False)
		End With
		btnDelete.Enabled = dischargeLocationId <> Guid.Empty AndAlso _currentUserPermission(_currentTableName).Delete
		pnlPanels.Visible = Not dischargeLocationId.Equals(Guid.Empty)
		pnlStorageLocations.Visible = ddlStorageLocations.Items.Count > 0 AndAlso Not dischargeLocationId.Equals(Guid.Empty)
	End Sub

	Private Sub PopulateDischargeLocationList()
		Dim facilityId As Guid = Guid.Empty
		Guid.TryParse(ddlFacilityFilter.SelectedValue, facilityId)
		Dim bays As New Dictionary(Of Guid, KaBay)
		Dim panels As New Dictionary(Of Guid, KaPanel)
		Dim panelDischargeLocations As New Dictionary(Of Guid, KaPanel)

		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		For Each panel As KaPanel In KaPanel.GetAll(connection, $"{KaPanel.FN_DELETED} = 0", $"{KaPanel.FN_NAME}")
			If Not panel.DischargeLocationId = Guid.Empty AndAlso Not panelDischargeLocations.ContainsKey(panel.DischargeLocationId) Then panelDischargeLocations.Add(panel.DischargeLocationId, panel)
			If Not panels.ContainsKey(panel.Id) Then panels.Add(panel.Id, panel)
		Next

		ddlDischargeLocations.Items.Clear()
		If _currentUserPermission(_currentTableName).Create Then
			ddlDischargeLocations.Items.Add(New ListItem("Enter a new discharge location", Guid.Empty.ToString()))
		Else
			ddlDischargeLocations.Items.Add(New ListItem("Select a discharge location", Guid.Empty.ToString()))
		End If
		ddlDischargeLocations.SelectedIndex = 0
		For Each r As KaDischargeLocation In KaDischargeLocation.GetAll(connection, $"{KaDischargeLocation.FN_DELETED} = 0", $"{KaDischargeLocation.FN_NAME} ASC")
			Dim dischLocAssignedToFacility As Boolean = facilityId.Equals(Guid.Empty)
			If Not dischLocAssignedToFacility Then
				If panelDischargeLocations.ContainsKey(r.Id) Then
					dischLocAssignedToFacility = panelDischargeLocations(r.Id).LocationId = facilityId ' This is a panel, and it is  assigned to this facility
				ElseIf r.BayId = Guid.Empty Then ' There isn't a bay assigned, see if there are any panels assigned to this discharge location that are for this facility
					Dim panelsAssigned As ArrayList = KaDischargeLocationPanelSettings.GetAll(connection, $"{KaDischargeLocationPanelSettings.FN_DELETED} = 0 AND {KaDischargeLocationPanelSettings.FN_DISCHARGE_LOCATION_ID} = {Q(r.Id)}", "")
					For Each panelId As KaDischargeLocationPanelSettings In panelsAssigned
						If panels.ContainsKey(panelId.PanelId) AndAlso panels(panelId.PanelId).LocationId = facilityId Then ' Assigned to a panel that is assigned to this facility
							dischLocAssignedToFacility = True
							Exit For
						End If
					Next
					If Not dischLocAssignedToFacility Then dischLocAssignedToFacility = panelsAssigned.Count = 0 ' No panels assigned, include all
				Else
					Try
						If Not bays.ContainsKey(r.BayId) Then bays.Add(r.BayId, New KaBay(connection, r.BayId))
						If bays(r.BayId).LocationId = facilityId Then dischLocAssignedToFacility = True
					Catch ex As RecordNotFoundException

					End Try
				End If
			End If
			If dischLocAssignedToFacility Then ddlDischargeLocations.Items.Add(New ListItem(r.Name & IIf(panelDischargeLocations.ContainsKey(r.Id), " (Panel)", ""), r.Id.ToString()))
		Next
	End Sub

	Private Sub PopulateLocationList()
		ddlFacilityFilter.Items.Clear()
		ddlFacilityFilter.Items.Add(New ListItem("All facilities", Guid.Empty.ToString()))
		For Each locationInfo As KaLocation In KaLocation.GetAll(GetUserConnection(_currentUser.Id), $"{KaLocation.FN_DELETED} = 0", $"{KaLocation.FN_NAME} ASC")
			ddlFacilityFilter.Items.Add(New ListItem(locationInfo.Name, locationInfo.Id.ToString))
		Next
	End Sub

	Private Sub PopulatePanelList(dischargeLocationId As Guid)
		Dim facilityId As Guid = Guid.Empty
		Guid.TryParse(ddlFacilityFilter.SelectedValue, facilityId)

		ddlPanel.Items.Clear()
		ddlPanel.Items.Add(New ListItem("", Guid.Empty.ToString()))
		For Each r As KaPanel In KaPanel.GetAll(GetUserConnection(_currentUser.Id), $"{KaPanel.FN_DELETED} = 0", $"{KaPanel.FN_NAME} ASC")
			' don't show the panel if it matches the discharge location name
			If r.DischargeLocationId <> dischargeLocationId AndAlso (facilityId.Equals(Guid.Empty) OrElse facilityId.Equals(r.LocationId)) Then ddlPanel.Items.Add(New ListItem(r.Name, r.Id.ToString()))
		Next
		ddlPanel_SelectedIndexChanged(ddlPanel, New EventArgs)
	End Sub

	Private Sub PopulateUnitList()
		ddlFillLimitUnit.Items.Clear()
		ddlSecondaryFillLimitUnit.Items.Clear()
		ddlFillLimitUnit.Items.Add(New ListItem("", Guid.Empty.ToString()))
		ddlSecondaryFillLimitUnit.Items.Add(New ListItem("", Guid.Empty.ToString()))
		For Each r As KaUnit In KaUnit.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND base_unit<>9", "abbreviation ASC")
			ddlFillLimitUnit.Items.Add(New ListItem(r.Abbreviation, r.Id.ToString()))
			ddlSecondaryFillLimitUnit.Items.Add(New ListItem(r.Abbreviation, r.Id.ToString()))
		Next
	End Sub

	Private Function CheckDischargeLocationPath(dischargeLocationName As String, dischargeLocationsVisited As List(Of String), path As String, ByRef pathLoop As String) As Boolean
		Dim myPath As String = path & IIf(path.Length > 0, " to ", "") & dischargeLocationName
		If dischargeLocationsVisited.IndexOf(dischargeLocationName) <> -1 Then ' have we been to this discharge location?
			pathLoop = myPath ' if so, this is a loop
			Return True
		End If
		Dim myDischargeLocationsVisited As New List(Of String) ' create a new instance of the list so that this path isn't contaminated by another path
		myDischargeLocationsVisited.AddRange(dischargeLocationsVisited) ' add all the discharge locations visited on the way to this discharge location
		myDischargeLocationsVisited.Add(dischargeLocationName) ' add this discharge location to the path
		Dim conditions As String = "deleted=0 AND name='" & dischargeLocationName.Replace("'", "''") & "'" ' does this panel represent a discharge location?
		For Each panel As KaPanel In KaPanel.GetAll(GetUserConnection(_currentUser.Id), conditions, "")
			conditions = "deleted=0 AND panel_id='{" & panel.Id.ToString() & "}'" ' follow the path from this panel
			For Each panelSettings As KaDischargeLocationPanelSettings In KaDischargeLocationPanelSettings.GetAll(GetUserConnection(_currentUser.Id), conditions, "")
				Dim dischargeLocation As New KaDischargeLocation(GetUserConnection(_currentUser.Id), panelSettings.DischargeLocationId)
				If Not dischargeLocation.Deleted Then ' there's no need to check this path, the discharge location is deleted
					If CheckDischargeLocationPath(dischargeLocation.Name, myDischargeLocationsVisited, myPath, pathLoop) Then Return True
				End If
			Next
		Next
		Return False
	End Function

	Private Function CreatesDischargeLocationLoop(ByRef pathLoop As String) As Boolean
		Dim myPath As String = ddlPanel.SelectedItem.Text
		Dim myDischargeLocationsVisited As New List(Of String)
		myDischargeLocationsVisited.Add(ddlPanel.SelectedItem.Text)
		Return CheckDischargeLocationPath(ddlDischargeLocations.SelectedItem.Text, myDischargeLocationsVisited, myPath, pathLoop)
	End Function

	Private Sub SetTextboxMaxLengths()
		tbxFillLimit.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaDischargeLocation.TABLE_NAME, "fill_limit"))
		tbxSecondaryFillLimit.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaDischargeLocation.TABLE_NAME, "fill_limit2"))
		tbxName.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaDischargeLocation.TABLE_NAME, "name"))
		tbxAnticipationMultiplier.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaDischargeLocationPanelSettings.TABLE_NAME, KaDischargeLocationPanelSettings.FN_ANTICIPATION_MULTIPLIER))
		tbxPurgeTimeMultiplier.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaDischargeLocationPanelSettings.TABLE_NAME, KaDischargeLocationPanelSettings.FN_PURGE_TIME_MULTIPLIER))
	End Sub

	Private Sub SetControlUsabilityFromPermissions()
		With _currentUserPermission(_currentTableName)
			Dim shouldEnable = (.Edit AndAlso ddlDischargeLocations.SelectedIndex > 0) OrElse (.Create AndAlso ddlDischargeLocations.SelectedIndex = 0)
			pnlMain.Enabled = shouldEnable
			btnSave.Enabled = shouldEnable
			Dim value = Guid.Parse(ddlDischargeLocations.SelectedValue)
			btnDelete.Enabled = .Edit AndAlso .Delete AndAlso value <> Guid.Empty
			btnAddStorageLocations.Enabled = shouldEnable AndAlso ddlStorageLocations.SelectedIndex > 0
			btnRemoveStorageLocations.Enabled = shouldEnable AndAlso lstStorageLocations.SelectedIndex >= 0
		End With
	End Sub

	Protected Sub ddlFacilityFilter_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlFacilityFilter.SelectedIndexChanged
		Dim dischLoc As KaDischargeLocation = New KaDischargeLocation()
		Try
			If ddlDischargeLocations.SelectedIndex >= 0 Then dischLoc = New KaDischargeLocation(GetUserConnection(_currentUser.Id), Guid.Parse(ddlDischargeLocations.SelectedValue))
		Catch ex As RecordNotFoundException

		End Try

		PopulateDischargeLocationList()
		Try
			ddlDischargeLocations.SelectedValue = dischLoc.Id.ToString()
		Catch ex As ArgumentOutOfRangeException
			dischLoc = New KaDischargeLocation()
		End Try
		ddlDischargeLocations_SelectedIndexChanged(ddlDischargeLocations, New EventArgs())
		KaSetting.WriteSetting(GetUserConnection(_currentUser.Id), "PanelsPage:" & _currentUser.Id.ToString & "/LastFacilityUsed", ddlFacilityFilter.SelectedValue)
	End Sub

	Protected Sub ScriptManager1_AsyncPostBackError(ByVal sender As Object, ByVal e As System.Web.UI.AsyncPostBackErrorEventArgs)
		ToolkitScriptManager1.AsyncPostBackErrorMessage = e.Exception.Message
	End Sub

	Private Sub DisplayJavaScriptMessage(key As String, script As String)
		ClientScript.RegisterClientScriptBlock(Me.GetType(), key, script)
	End Sub

#Region " Storage locations "
	Private Sub PopulateStorageLocationList()
		Dim dischargeLocationId As Guid = Guid.Empty
		Try
			If ddlDischargeLocations.SelectedIndex >= 0 Then Guid.TryParse(ddlDischargeLocations.SelectedValue, dischargeLocationId)
		Catch ex As RecordNotFoundException

		End Try
		PopulateStorageLocationList(dischargeLocationId)
	End Sub

	Private Sub PopulateStorageLocationList(dischargeLocationId As Guid)
		Dim locationId As Guid = Guid.Empty
		Guid.TryParse(ddlFacilityFilter.SelectedValue, locationId)

		ddlStorageLocations.Items.Clear()
		If Tm2Database.SystemItemTraceabilityEnabled Then
			ddlStorageLocations.Items.Add(New ListItem("", Guid.Empty.ToString()))
			Dim alreadyAssignedIds As List(Of Guid) = AssignedStorageLocationIds()

			Dim sql As String
			sql = $"SELECT 1 AS tableIndex, {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_ID}, {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_NAME} " &
							$"FROM {KaStorageLocation.TABLE_NAME} " &
							$"WHERE ({KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_DELETED} = 0) " &
								$"AND ({KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_TANK_ID} = {Q(Guid.Empty)}) " &
								$"AND ({KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_CONTAINER_ID} = {Q(Guid.Empty)}) " &
								IIf(locationId.Equals(Guid.Empty), "", $"AND ({KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_LOCATION_ID} = {Q(locationId)}) ") & ' if Facility filter is not set, display all possible storage locations
					"UNION " &
					$"SELECT 1 AS tableIndex, {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_ID}, {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_NAME} " &
							$"FROM {KaStorageLocation.TABLE_NAME} " &
							$"WHERE ({KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_DELETED} = 0) " &
								$"AND ({KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_TANK_ID} = {Q(Guid.Empty)}) " &
								$"AND ({KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_CONTAINER_ID} = {Q(Guid.Empty)}) " &
								$"AND ({KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_LOCATION_ID} = {Q(Guid.Empty)}) " &
					"UNION " &
					$"SELECT 2 AS tableIndex, {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_ID}, " &
								$"CASE WHEN {KaTank.TABLE_NAME}.name = {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_NAME} THEN 'Tank: ' + {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_NAME} ELSE {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_NAME} + ' (Tank: ' + {KaTank.TABLE_NAME}.name + ')' END AS {KaStorageLocation.FN_NAME} " &
							$"FROM {KaStorageLocation.TABLE_NAME} INNER JOIN {KaTank.TABLE_NAME} ON {KaTank.TABLE_NAME}.{KaTank.FN_ID} = {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_TANK_ID} " &
							$"WHERE ({KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_DELETED} = 0) " &
								$"AND ({KaTank.TABLE_NAME}.deleted = {Q(False)}) " &
								$"AND ({KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_TANK_ID} <> {Q(Guid.Empty)}) " &
								IIf(locationId.Equals(Guid.Empty), "", $"AND ({KaTank.TABLE_NAME}.location_id = {Q(locationId)}) ") & ' if Facility filter is not set, display all possible storage locations
					"UNION " &
					$"SELECT 2 AS tableIndex, {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_ID}, " &
								$"CASE WHEN {KaTank.TABLE_NAME}.name = {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_NAME} THEN 'Tank: ' + {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_NAME} ELSE {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_NAME} + ' (Tank: ' + {KaTank.TABLE_NAME}.name + ')' END AS {KaStorageLocation.FN_NAME} " &
							$"FROM {KaStorageLocation.TABLE_NAME} INNER JOIN {KaTank.TABLE_NAME} ON {KaTank.TABLE_NAME}.{KaTank.FN_ID} = {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_TANK_ID} " &
							$"WHERE ({KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_DELETED} = 0) " &
								$"AND ({KaTank.TABLE_NAME}.deleted = {Q(False)}) " &
								$"AND ({KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_TANK_ID} <> {Q(Guid.Empty)}) " &
								$"AND ({KaTank.TABLE_NAME}.location_id = {Q(Guid.Empty)}) " &
					"UNION " &
					$"SELECT 2 AS tableIndex, {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_ID}, " &
								$"CASE WHEN {KaContainer.TABLE_NAME}.number = {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_NAME} THEN 'Container: ' + {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_NAME} ELSE {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_NAME} + ' (Container: ' + {KaContainer.TABLE_NAME}.number + ')' END AS {KaStorageLocation.FN_NAME} " &
							$"FROM {KaStorageLocation.TABLE_NAME} " &
							$"INNER JOIN {KaContainer.TABLE_NAME} ON {KaContainer.TABLE_NAME}.{KaContainer.FN_ID} = {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_CONTAINER_ID} " &
							$"WHERE ({KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_DELETED} = 0) " &
								$"AND ({KaContainer.TABLE_NAME}.deleted = {Q(False)}) " &
								$"AND ({KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_CONTAINER_ID} <> {Q(Guid.Empty)}) " &
							$"ORDER BY tableIndex, {KaStorageLocation.FN_NAME}"
			Dim storageLocationsRdr As OleDbDataReader = Nothing
			Try
				storageLocationsRdr = Tm2Database.ExecuteReader(Tm2Database.Connection, sql)
				While storageLocationsRdr.Read()
					Dim slId As Guid = storageLocationsRdr.Item(KaStorageLocation.FN_ID)
					If Not alreadyAssignedIds.Contains(slId) Then ddlStorageLocations.Items.Add(New ListItem(storageLocationsRdr.Item(KaStorageLocation.FN_NAME), slId.ToString()))
				End While
			Finally
				If storageLocationsRdr IsNot Nothing Then storageLocationsRdr.Close()
			End Try
		End If
		ddlStorageLocations_SelectedIndexChanged(ddlStorageLocations, New EventArgs())
	End Sub

	Private Sub PopulateStorageLocationsAssigned(dischargeLocationId As Guid)
		lstStorageLocations.Items.Clear()
		If Tm2Database.SystemItemTraceabilityEnabled Then
			Dim sql As String
			sql = $"SELECT 1 AS tableIndex, {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_ID}, {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_NAME} " &
							$"FROM {KaStorageLocation.TABLE_NAME} " &
							$"INNER JOIN {KaDischargeLocationStorageLocation.TABLE_NAME} ON {KaDischargeLocationStorageLocation.TABLE_NAME}.{KaDischargeLocationStorageLocation.FN_STORAGE_LOCATION_ID} = {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_ID} " &
							$"WHERE ({KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_DELETED} = 0) " &
								$"AND ({KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_TANK_ID} = {Q(Guid.Empty)}) " &
								$"AND ({KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_CONTAINER_ID} = {Q(Guid.Empty)}) " &
								$"AND ({KaDischargeLocationStorageLocation.TABLE_NAME}.{KaDischargeLocationStorageLocation.FN_DELETED} = {Q(False)}) " &
								$"AND ({KaDischargeLocationStorageLocation.TABLE_NAME}.{KaDischargeLocationStorageLocation.FN_DISCHARGE_LOCATION_ID} = {Q(dischargeLocationId)}) " &
								$"AND ({KaDischargeLocationStorageLocation.TABLE_NAME}.{KaDischargeLocationStorageLocation.FN_DISCHARGE_LOCATION_ID} <> {Q(Guid.Empty)}) " &
					"UNION " &
					$"SELECT 2 AS tableIndex, {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_ID}, " &
								$"CASE WHEN {KaTank.TABLE_NAME}.name = {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_NAME} THEN 'Tank: ' + {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_NAME} ELSE {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_NAME} + ' (Tank: ' + {KaTank.TABLE_NAME}.name + ')' END AS {KaStorageLocation.FN_NAME} " &
							$"FROM {KaStorageLocation.TABLE_NAME} " &
							$"INNER JOIN {KaTank.TABLE_NAME} ON {KaTank.TABLE_NAME}.{KaTank.FN_ID} = {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_TANK_ID} " &
							$"INNER JOIN {KaDischargeLocationStorageLocation.TABLE_NAME} ON {KaDischargeLocationStorageLocation.TABLE_NAME}.{KaDischargeLocationStorageLocation.FN_STORAGE_LOCATION_ID} = {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_ID} " &
							$"WHERE ({KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_DELETED} = 0) " &
								$"AND ({KaTank.TABLE_NAME}.deleted = {Q(False)}) " &
								$"AND ({KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_TANK_ID} <> {Q(Guid.Empty)}) " &
								$"AND ({KaDischargeLocationStorageLocation.TABLE_NAME}.{KaDischargeLocationStorageLocation.FN_DELETED} = {Q(False)}) " &
								$"AND ({KaDischargeLocationStorageLocation.TABLE_NAME}.{KaDischargeLocationStorageLocation.FN_DISCHARGE_LOCATION_ID} = {Q(dischargeLocationId)}) " &
								$"AND ({KaDischargeLocationStorageLocation.TABLE_NAME}.{KaDischargeLocationStorageLocation.FN_DISCHARGE_LOCATION_ID} <> {Q(Guid.Empty)}) " &
					"UNION " &
					$"SELECT 2 AS tableIndex, {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_ID}, " &
								$"CASE WHEN {KaContainer.TABLE_NAME}.number = {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_NAME} THEN 'Container: ' + {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_NAME} ELSE {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_NAME} + ' (Container: ' + {KaContainer.TABLE_NAME}.number + ')' END AS {KaStorageLocation.FN_NAME} " &
							$"FROM {KaStorageLocation.TABLE_NAME} " &
							$"INNER JOIN {KaContainer.TABLE_NAME} ON {KaContainer.TABLE_NAME}.{KaContainer.FN_ID} = {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_CONTAINER_ID} " &
							$"INNER JOIN {KaDischargeLocationStorageLocation.TABLE_NAME} ON {KaDischargeLocationStorageLocation.TABLE_NAME}.{KaDischargeLocationStorageLocation.FN_STORAGE_LOCATION_ID} = {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_ID} " &
							$"WHERE ({KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_DELETED} = 0) " &
								$"AND ({KaContainer.TABLE_NAME}.deleted = {Q(False)}) " &
								$"AND ({KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_CONTAINER_ID} <> {Q(Guid.Empty)}) " &
								$"AND ({KaDischargeLocationStorageLocation.TABLE_NAME}.{KaDischargeLocationStorageLocation.FN_DELETED} = {Q(False)}) " &
								$"AND ({KaDischargeLocationStorageLocation.TABLE_NAME}.{KaDischargeLocationStorageLocation.FN_DISCHARGE_LOCATION_ID} = {Q(dischargeLocationId)}) " &
								$"AND ({KaDischargeLocationStorageLocation.TABLE_NAME}.{KaDischargeLocationStorageLocation.FN_DISCHARGE_LOCATION_ID} <> {Q(Guid.Empty)}) " &
					$"ORDER BY tableIndex, {KaStorageLocation.FN_NAME}"
			Dim storageLocationsRdr As OleDbDataReader = Nothing
			Try
				storageLocationsRdr = Tm2Database.ExecuteReader(Tm2Database.Connection, sql)
				While storageLocationsRdr.Read()
					lstStorageLocations.Items.Add(New ListItem(storageLocationsRdr.Item(KaStorageLocation.FN_NAME), storageLocationsRdr.Item(KaStorageLocation.FN_ID).ToString()))
				End While
			Finally
				If storageLocationsRdr IsNot Nothing Then storageLocationsRdr.Close()
			End Try
		End If
		lstStorageLocations_SelectedIndexChanged(lstStorageLocations, New EventArgs())
	End Sub

	Private Function AssignedStorageLocationIds() As List(Of Guid)
		Dim alreadyAssignedIds As List(Of Guid) = New List(Of Guid)
		For Each li As ListItem In lstStorageLocations.Items
			Dim slId As Guid = Guid.Empty
			If Guid.TryParse(li.Value, slId) AndAlso Not slId.Equals(Guid.Empty) AndAlso Not alreadyAssignedIds.Contains(slId) Then alreadyAssignedIds.Add(slId)
		Next
		Return alreadyAssignedIds
	End Function

	Protected Sub ddlStorageLocations_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlStorageLocations.SelectedIndexChanged
		SetControlUsabilityFromPermissions()
	End Sub

	Protected Sub lstStorageLocations_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lstStorageLocations.SelectedIndexChanged
		SetControlUsabilityFromPermissions()
	End Sub

	Protected Sub btnAddStorageLocations_Click(sender As Object, e As EventArgs) Handles btnAddStorageLocations.Click
		If ddlStorageLocations.SelectedIndex > 0 Then
			lstStorageLocations.Items.Add(New ListItem(ddlStorageLocations.SelectedItem.Text, ddlStorageLocations.SelectedItem.Value))
		End If
		PopulateStorageLocationList()
	End Sub

	Protected Sub btnRemoveStorageLocations_Click(sender As Object, e As EventArgs) Handles btnRemoveStorageLocations.Click
		If lstStorageLocations.SelectedIndex >= 0 Then
			lstStorageLocations.Items.RemoveAt(lstStorageLocations.SelectedIndex)
		End If
		PopulateStorageLocationList()
	End Sub
#End Region
End Class