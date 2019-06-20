Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class GeneralSettings : Inherits System.Web.UI.Page

	Private _currentUser As KaUser
	Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
	Private _currentTableName As String = KaSetting.TABLE_NAME

	Public Const SN_CONVERT_WEB_PAGE_URL_DOMAIN_TO_REQUESTED_DOMAIN As String = "ConvertWebPageUrlDomainToRequestedPagesDomain"

	Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
		Response.Cache.SetCacheability(HttpCacheability.NoCache)
		_currentUser = Utilities.GetUser(Me)
		_currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "GeneralSettings")
		If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")
		btnSaveGeneralSettings.Enabled = _currentUserPermission(_currentTableName).Edit
		lblGeneralSettingsSave.Visible = False
		If Not Page.IsPostBack Then
			PopulateDefaultMassUnitCombo()
			PopulateDefaultVolumeUnitCombo()
			PopulateNumberOfBackupSets()
			DisplaySettings()
		End If
	End Sub

	Private Sub PopulateDefaultMassUnitCombo()
		ddlDefaultMassUnit.Items.Clear()
		Dim li As ListItem = New ListItem
		Dim allUnits As ArrayList = KaUnit.GetAll(GetUserConnection(_currentUser.Id), "deleted = " & Q(False), "name asc")
		Dim massUnits As ArrayList = KaUnit.FilterOutVolumeUnits(allUnits)
		For Each unit As KaUnit In massUnits
			li = New ListItem
			li.Text = unit.Name
			li.Value = unit.Id.ToString
			ddlDefaultMassUnit.Items.Add(li)
		Next
	End Sub

	Private Sub PopulateDefaultVolumeUnitCombo()
		ddlDefaultVolumeUnit.Items.Clear()
		Dim li As ListItem = New ListItem
		Dim allUnits As ArrayList = KaUnit.GetAll(GetUserConnection(_currentUser.Id), "deleted = " & Q(False), "name asc")
		Dim volumeUnits As ArrayList = KaUnit.FilterOutMassUnits(allUnits)
		For Each unit As KaUnit In volumeUnits
			li = New ListItem
			li.Text = unit.Name
			li.Value = unit.Id.ToString
			ddlDefaultVolumeUnit.Items.Add(li)
		Next
	End Sub

	Private Sub PopulateNumberOfBackupSets()
		ddlDatabaseBackupNumberOfSets.Items.Clear()
		ddlDatabaseBackupNumberOfSets.Items.Add("Require -c argument for Scheduled Task")
		For i As Integer = 1 To 30
			ddlDatabaseBackupNumberOfSets.Items.Add(i.ToString())
		Next
	End Sub

	Private Sub DisplaySettings()
		Dim c As OleDbConnection = GetUserConnection(_currentUser.Id)
		ddlDefaultMassUnit.SelectedValue = KaUnit.GetSystemDefaultMassUnitOfMeasure(c, Nothing).ToString
		ddlDefaultVolumeUnit.SelectedValue = KaUnit.GetSystemDefaultVolumeUnitOfMeasure(c, Nothing).ToString
		tbxWebPageTitle.Text = KaSetting.GetSetting(c, KaSetting.SN_WEB_PAGE_TITLE, KaSetting.SD_WEB_PAGE_TITLE)
		tbxDatabaseChangeEmailAddress.Text = KaSetting.GetSetting(c, KaSetting.SN_DATABASE_STRUCTURE_CHANGED_EMAIL_TO_ADDRESS, KaSetting.SD_DATABASE_STRUCTURE_CHANGED_EMAIL_TO_ADDRESS)
		Boolean.TryParse(KaSetting.GetSetting(c, SN_CONVERT_WEB_PAGE_URL_DOMAIN_TO_REQUESTED_DOMAIN, True), cbxConvertWebPageUrlDomainToRequestedPagesDomain.Checked)
		Boolean.TryParse(KaSetting.GetSetting(c, KaSetting.SN_DEFAULT_NEW_DRIVERS_AS_VALID_FOR_ALL_ACCOUNTS, KaSetting.SD_DEFAULT_NEW_DRIVERS_AS_VALID_FOR_ALL_ACCOUNTS), cbxDefaultNewDriversValidForAllAccounts.Checked)
		cbxItemTraceabilityEnabled.Checked = Tm2Database.SystemItemTraceabilityEnabled
		ddlDatabaseBackupNumberOfSets.SelectedIndex = Integer.Parse(KaSetting.GetSetting(c, "General/DatabaseBackupMaxSets", "0"))
		tbxDatabaseBackupFolder.Text = KaSetting.GetSetting(c, "General/DatabaseBackupDestinationPath", "")
		tbxDatabaseBackupTimeout.Text = KaSetting.GetSetting(c, "General/DatabaseBackupTimeout", Tm2Database.CommandTimeout.ToString())
		Dim eventLogMinDate As Integer = 365
		Integer.TryParse(KaSetting.GetSetting(c, "General/DaysToKeepEventLogRecords", eventLogMinDate), eventLogMinDate)
		tbxDaysToKeepEventLogRecords.Text = eventLogMinDate.ToString()
		tbxProxyAddress.Text = KaSetting.GetSetting(c, KaSetting.SN_PROXY_ADDRESS, KaSetting.SD_PROXY_ADDRESS)
		tbxProxyPort.Text = KaSetting.GetSetting(c, KaSetting.SN_PROXY_PORT, KaSetting.SD_PROXY_PORT)
		tbxActivationAlertEmailRecipients.Text = KaSetting.GetSetting(c, KaSetting.SN_ACTIVATION_ALERT_EMAIL_RECIPIENTS, KaSetting.SD_ACTIVATION_ALERT_EMAIL_RECIPIENTS)
		tbxBackgroundServicesLog.Text = KaSetting.GetSetting(c, KaSetting.SN_BACKGROUND_SERVICES_LOG_PATH, KaSetting.SD_BACKGROUND_SERVICES_LOG_PATH)
		tbxEmailServiceLog.Text = KaSetting.GetSetting(c, KaSetting.SN_EMAIL_SERVICE_LOG_PATH, KaSetting.SD_EMAIL_SERVICE_LOG_PATH)
		tbxTankStatusLog.Text = KaSetting.GetSetting(c, KaSetting.SN_TANK_STATUS_LOG_PATH, KaSetting.SD_TANK_STATUS_LOG_PATH)
		tbxBackupLog.Text = KaSetting.GetSetting(c, KaSetting.SN_TM2_BACKUP_LOG_PATH, KaSetting.SD_TM2_BACKUP_LOG_PATH)
	End Sub

	Private Function ValidateSettings(c As OleDbConnection) As Boolean
		Dim temp As Integer = 0
		Dim emailErrorMessage As String = String.Empty
		Dim logFileErrorMessages As String = ""
		If Not Integer.TryParse(tbxDaysToKeepEventLogRecords.Text, temp) OrElse temp <= 0 Then
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidDaysToKeepEventLogRecords", Utilities.JsAlert("Please specify the number of days to keep event log records."))
			Return False
		ElseIf Not Integer.TryParse(tbxDatabaseBackupTimeout.Text, temp) OrElse temp <= 0 Then
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidtbxDatabaseBackupTimeout", Utilities.JsAlert("Please specify the number of seconds for the database backup."))
			Return False
		ElseIf Not ProxyFieldsValid() Then
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidProxy", Utilities.JsAlert("Please specify a valid proxy address and port"))
			Return False
		ElseIf Not Utilities.IsEmailFieldValid(tbxActivationAlertEmailRecipients.Text, emailErrorMessage) Then ' the e-mail field is not formatted correctly
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidEmail", Utilities.JsAlert(emailErrorMessage))
			Return False
		ElseIf Not CreateLogFolders(logFileErrorMessages) Then
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidLogFolder", Utilities.JsAlert(logFileErrorMessages))
			Return False
		Else
			Return True
		End If
	End Function

	Private Function ProxyFieldsValid() As Boolean
		If Not String.IsNullOrWhiteSpace(tbxProxyPort.Text) Or Not String.IsNullOrWhiteSpace(tbxProxyAddress.Text) Then
			Dim proxy As System.Net.WebProxy = Nothing
			If (Not KaLicenseActivation.Utilities.TryCreateProxy(tbxProxyAddress.Text, tbxProxyPort.Text, proxy)) Then
				ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidProxy", Utilities.JsAlert("Please specify a valid proxy address and port"))
				Return False
			End If
		End If
		Return True
	End Function

	Private Function CreateLogFolders(ByRef logFileErrorMessages As String) As Boolean
		Try
			Dim logLocation As String = tbxBackupLog.Text.TrimEnd(vbCr, vbLf, "\")
			logFileErrorMessages = "Error while creating Database Backup Log folder"
			If logLocation.Length > 0 AndAlso Not IO.Directory.Exists(logLocation) Then IO.Directory.CreateDirectory(logLocation)

			logLocation = tbxBackgroundServicesLog.Text.TrimEnd(vbCr, vbLf, "\")
			logFileErrorMessages = "Error while creating Background Services Log folder"
			If logLocation.Length > 0 AndAlso Not IO.Directory.Exists(logLocation) Then IO.Directory.CreateDirectory(logLocation)

			logLocation = tbxEmailServiceLog.Text.TrimEnd(vbCr, vbLf, "\")
			logFileErrorMessages = "Error while creating Email Services Log folder"
			If logLocation.Length > 0 AndAlso Not IO.Directory.Exists(logLocation) Then IO.Directory.CreateDirectory(logLocation)

			logLocation = tbxTankStatusLog.Text.TrimEnd(vbCr, vbLf, "\")
			logFileErrorMessages = "Error while creating Tank Status Reader Log folder"
			If logLocation.Length > 0 AndAlso Not IO.Directory.Exists(logLocation) Then IO.Directory.CreateDirectory(logLocation)

			Return True
		Catch ex As Exception
			Return False
		End Try
	End Function

	Private Sub SaveSettings(c As OleDbConnection)
		' validate settings
		KaUnit.SetSystemDefaultMassUnitOfMeasure(c, Nothing, Guid.Parse(ddlDefaultMassUnit.SelectedValue))
		KaUnit.SetSystemDefaultVolumeUnitOfMeasure(c, Nothing, Guid.Parse(ddlDefaultVolumeUnit.SelectedValue))
		KaSetting.WriteSetting(c, KaSetting.SN_WEB_PAGE_TITLE, tbxWebPageTitle.Text.Trim)
		KaSetting.WriteSetting(c, KaSetting.SN_DATABASE_STRUCTURE_CHANGED_EMAIL_TO_ADDRESS, tbxDatabaseChangeEmailAddress.Text)
		KaSetting.WriteSetting(c, SN_CONVERT_WEB_PAGE_URL_DOMAIN_TO_REQUESTED_DOMAIN, cbxConvertWebPageUrlDomainToRequestedPagesDomain.Checked)
		KaSetting.WriteSetting(c, KaSetting.SN_DEFAULT_NEW_DRIVERS_AS_VALID_FOR_ALL_ACCOUNTS, cbxDefaultNewDriversValidForAllAccounts.Checked)
		KaSetting.WriteSetting(c, "General/DatabaseBackupMaxSets", ddlDatabaseBackupNumberOfSets.SelectedIndex)
		KaSetting.WriteSetting(c, "General/DatabaseBackupDestinationPath", tbxDatabaseBackupFolder.Text)
		KaSetting.WriteSetting(c, "General/DatabaseBackupTimeout", tbxDatabaseBackupTimeout.Text)
		KaSetting.WriteSetting(c, "General/DaysToKeepEventLogRecords", tbxDaysToKeepEventLogRecords.Text)
		KaSetting.WriteSetting(c, KaSetting.SN_BACKGROUND_SERVICES_LOG_PATH, tbxBackgroundServicesLog.Text.TrimEnd(vbCr, vbLf, "\"))
		KaSetting.WriteSetting(c, KaSetting.SN_EMAIL_SERVICE_LOG_PATH, tbxEmailServiceLog.Text.TrimEnd(vbCr, vbLf, "\"))
		KaSetting.WriteSetting(c, KaSetting.SN_TANK_STATUS_LOG_PATH, tbxTankStatusLog.Text.TrimEnd(vbCr, vbLf, "\"))
		KaSetting.WriteSetting(c, KaSetting.SN_PROXY_ADDRESS, tbxProxyAddress.Text)
		KaSetting.WriteSetting(c, KaSetting.SN_PROXY_PORT, tbxProxyPort.Text)
		KaSetting.WriteSetting(c, KaSetting.SN_ACTIVATION_ALERT_EMAIL_RECIPIENTS, tbxActivationAlertEmailRecipients.Text)
		KaSetting.WriteSetting(c, KaSetting.SN_SYSTEM_ITEM_TRACEABILITY_ENABLED, cbxItemTraceabilityEnabled.Checked)
		KaSetting.WriteSetting(c, KaSetting.SN_TM2_BACKUP_LOG_PATH, tbxBackupLog.Text)

		Tm2Database.RefreshSystemItemTraceabilityEnabled()
		lblGeneralSettingsSave.Visible = True
	End Sub

	Protected Sub btnSaveGeneralSettings_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnSaveGeneralSettings.Click
		Dim c As OleDbConnection = GetUserConnection(_currentUser.Id)
		If ValidateSettings(c) Then SaveSettings(c)
	End Sub
End Class
