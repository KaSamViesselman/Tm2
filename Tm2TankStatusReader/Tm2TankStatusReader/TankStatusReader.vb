Imports KahlerAutomation.KaModbusTcp
Imports KahlerAutomation.KaTm2Database
Imports System.Math
Imports System.Reflection
Imports System.Threading
Imports KahlerAutomation.TerminalManagement2

Module TankStatusReader
	Private _panels As New List(Of TlmPanel)
	Private _applicationIdentifier As String = My.Application.Info.AssemblyName
	Private _thisAssembly = System.Reflection.Assembly.GetAssembly(GetType(Tm2TankStatusReader.TlmPanel))
	Private _logger As Common.Logger = New Common.Logger(KaSetting.SN_TANK_STATUS_LOG_PATH, KaSetting.SD_TANK_STATUS_LOG_PATH, _applicationIdentifier, _thisAssembly)

	Private ReadOnly Property Done As Boolean
		Get ' the status of the data retrieval
			For Each p As TlmPanel In _panels ' check every panel...
				For i As Integer = 0 To p.MaximumNumberOfTanks - 1 ' check every sensor...
					If Not p.TanksRead(i) Then Return False ' we're not done
				Next
			Next
			Return True
		End Get
	End Property

	Private Sub FetchTankLevelDataForPanel(panel As TlmPanel)
		WriteToLog($"Fetching tank level data for panel ""{panel.Panel.Name}""...", Common.Logger.LogFileType.Processing)
		WriteToLog(vbTab & $"IP Address: {panel.Panel.IpAddress}", Common.Logger.LogFileType.Processing)
		WriteToLog(vbTab & $"Base Address: {panel.Panel.SystemAddress}", Common.Logger.LogFileType.Processing)
		For i As Integer = 0 To panel.MaximumNumberOfTanks - 1 Step 5
			Dim command As New ModbusTcpCommand()
			Dim upper As Integer = Min(i + 4, panel.MaximumNumberOfTanks - 1)
			command.Tag = $"{panel.Panel.Id.ToString()}|{i}|{upper}"
			command.MakeReadHoldingRegistersPacket(ModbusTcpId, 1, panel.Panel.SystemAddress + (i * panel.TankDataRegisterLength), panel.TankDataRegisterLength * (upper - i + 1))
			panel.Network.SendCommand(command)
		Next
	End Sub

	Public Sub Main()
		Try
			Common.SetRunning()

			If (Common.CheckInstalling()) Then
				WriteToLog("Tank Status Reader cannot run because an update is installing", Common.Logger.LogFileType.Alarm, False)
				End
			End If

			Dim applicationVersion = KaCommonObjects.Assembly.GetAppVersion(_thisAssembly)

			WriteToLog($"{_thisAssembly.GetName().Name} Version {applicationVersion} (CLR {Environment.Version.ToString()}) started.", Common.Logger.LogFileType.Processing)
			Dim tm2Db As New Tm2Database(Tm2Database.GetDbConnection())
			WriteToLog("Database connection: " & Tm2Database.DbConnection, Common.Logger.LogFileType.Processing)
			WriteToLog("Checking database...", Common.Logger.LogFileType.Processing)
			Dim connection As OleDb.OleDbConnection = Tm2Database.Connection
			tm2Db.CheckDatabase(connection, True, False)
			WriteToLog("Done.", Common.Logger.LogFileType.Processing)
			_logger.LoadPath(KaSetting.SD_TANK_STATUS_LOG_PATH)

			If (Not CheckActivation()) Then Environment.Exit(-1)

			Tm2Database.SetApplicationVersion(Tm2Database.Connection, Nothing, My.Computer.Name, My.Application.Info.Title, applicationVersion, Common.Tm2Authorization.ProductCode, KaCommonObjects.Assembly.GetGuid(_thisAssembly), "")

			For Each panel As KaPanel In Database.GetPanelList()
				WriteToLog(String.Format("Connecting to {0}...", panel.Name), Common.Logger.LogFileType.Processing)
				Try
					If panel.SystemAddress = 1024 Then
						Dim panelBaseAddressChecked As Boolean = False
						Boolean.TryParse(KaSetting.GetSetting(Tm2Database.Connection, $"PanelBaseAddressChecked:{panel.Id.ToString()}", False), panelBaseAddressChecked)
						If Not panelBaseAddressChecked Then
							Dim oldSystemAddress As Integer = panel.SystemAddress
							panel.SystemAddress = 0
							panel.SqlUpdate(Tm2Database.Connection, Nothing, ApplicationIdentifier, "")
							KaSetting.WriteSetting(Tm2Database.Connection, $"PanelBaseAddressChecked:{panel.Id.ToString()}", True, Nothing, ApplicationIdentifier, "")
							WriteToLog($"Changed the base address for panel '{panel.Name}' from {oldSystemAddress} to {panel.SystemAddress}.", Common.Logger.LogFileType.Processing)
						End If
					End If

					Dim p As New TlmPanel(panel, AddressOf ReadResponse)
					FetchTankLevelDataForPanel(p)
					_panels.Add(p)
				Catch ex As Exception
					ExceptionHandler("failed to connect to panel (" & ex.Message & ")... ", "Main")
				End Try
			Next
			Do While Not Done
				Thread.Sleep(0)
			Loop
		Finally
			Common.SetNotRunning()
		End Try
	End Sub

	Private _modbusTcpId As UShort
	Private ReadOnly Property ModbusTcpId As UShort
		Get
			If _modbusTcpId < UShort.MaxValue Then _modbusTcpId += 1 Else _modbusTcpId = 0
			Return _modbusTcpId
		End Get
	End Property

	Private Sub ReadResponse(c As ModbusTcpCommand)
		Dim parts() As String = c.Tag.Split("|")
		Dim panelId As Guid
		If parts.Length < 1 OrElse Not Guid.TryParse(parts(0), panelId) Then
			WriteToLog("Unable to determine the TLM panel to apply readings to.", Common.Logger.LogFileType.Processing)
			Exit Sub
		End If

		Dim lower As Integer
		If parts.Length < 2 OrElse Not Integer.TryParse(parts(1), lower) Then
			WriteToLog("Unable to determine the beginning tank number to apply readings to.", Common.Logger.LogFileType.Processing)
			Exit Sub
		End If

		Dim upper As Integer
		If parts.Length < 3 OrElse Not Integer.TryParse(parts(2), upper) Then
			WriteToLog("Unable to determine the ending tank number to apply readings to.", Common.Logger.LogFileType.Processing)
			Exit Sub
		End If

		Dim tlmPanelObject As TlmPanel = Nothing
		Dim panelObject As KaPanel = Nothing
		For Each tlmp As TlmPanel In _panels
			If tlmp.Panel.Id.Equals(panelId) Then
				tlmPanelObject = tlmp
				panelObject = tlmp.Panel
				Exit For
			End If
		Next

		Try
			If panelObject Is Nothing Then panelObject = New KaPanel(Tm2Database.Connection, panelId)
			WriteToLog($"Using tank level data from panel '{panelObject.Name}' (sensors {(lower + 1)} to {(upper + 1)})... ", Common.Logger.LogFileType.Processing)
			' check to see if the base register offset needs to be reset to 0 for the panel
			If c.ExceptionOccured Then
				Dim s As String
				Select Case c.ExceptionCode
					Case ModbusTcpCommand.ExceptionCodes.IllegalDataAddress : s = "illegal data address"
					Case ModbusTcpCommand.ExceptionCodes.IllegalDataValue : s = "illegal data value"
					Case ModbusTcpCommand.ExceptionCodes.IllegalFunction : s = "illegal function"
					Case ModbusTcpCommand.ExceptionCodes.MemoryParityError : s = "memory parity error"
					Case ModbusTcpCommand.ExceptionCodes.NegativeAcknowledge : s = "negative acknowledge"
					Case ModbusTcpCommand.ExceptionCodes.SlaveDeviceBusy : s = "server device busy"
					Case ModbusTcpCommand.ExceptionCodes.SlaveDeviceFailure : s = "server device failure"
					Case Else : s = c.ExceptionCode
				End Select
				ExceptionHandler($"failed for panel '{panelObject.Name}' (exception: " & s & ")", "ReadResponse")
			ElseIf c.TimedOut Then
				ExceptionHandler($"failed (no response from the panel '{panelObject.Name}').", "ReadResponse")
			Else
				Dim tanks As ArrayList = KaTank.GetAll(Tm2Database.Connection, "deleted=0 AND panel_id=" & Q(panelObject.Id), "")
				For sensor As Integer = lower To upper
					Dim updateTanks As New ArrayList()
					For Each tank As KaTank In tanks
						If tank.Sensor = sensor Then updateTanks.Add(tank)
					Next
					Dim offset As Integer = (sensor - lower) * tlmPanelObject.TankDataRegisterLength ' record where the data for this tank starts
					If (c.Data(offset) And 8192) = 0 AndAlso updateTanks.Count = 0 Then ' KA-2000 says this tank is enabled, but database doesn't have a tank for this sensor
						WriteToLog($"Created a new tank for sensor {sensor} for panel '{panelObject.Name}'.", Common.Logger.LogFileType.Processing)
						Dim tank As New KaTank() ' create a new tank
						tank.Sensor = sensor
						tank.PanelId = panelObject.Id
						tank.LocationId = panelObject.LocationId
						updateTanks.Add(tank)
					End If
					For Each tank As KaTank In updateTanks : UpdateTank(panelObject, tank, c, offset) : Next ' update the tanks that use this sensor
				Next
				WriteToLog($"Done using tank level data for panel '{panelObject.Name}' (sensors {(lower + 1)} to {(upper + 1)}).", Common.Logger.LogFileType.Processing)
			End If
		Catch ex As Exception
			ExceptionHandler($"failed to read tank level data on panel '{panelObject.Name}': ({KaCommonObjects.Alerts.FormatException(ex)}).", "ReadResponse")
		End Try
		If tlmPanelObject IsNot Nothing Then ' this is the panel...
			For i As Integer = lower To upper ' for each sensor read in this range...
				tlmPanelObject.TanksRead(i) = True ' mark the tank sensor entries as read
			Next
			Dim done As Boolean = True ' determine if the panel is done
			For i As Integer = 0 To tlmPanelObject.MaximumNumberOfTanks - 1 ' check to see if all tank sensors have been read
				If Not tlmPanelObject.TanksRead(i) Then ' this sensor has not been read yet
					done = False ' this panel is not done
					Exit For
				End If
			Next
			If done Then ' this panel is done
				tlmPanelObject.Network.UnsubscribeDataReceived(AddressOf ReadResponse)
				tlmPanelObject.Network.Close() ' stop the communication thread
			End If
		End If
	End Sub

	Public Sub WriteToEventLog(message As String, category As KaEventLog.Categories)
		Dim newEvent As New KaEventLog
		Dim interval = KaSetting.GetSetting(Tm2Database.Connection, "@" & Net.Dns.GetHostName() & "/Tm2TankStatusReader/ResendInterval", "12")
		With newEvent
			.ApplicationIdentifier = _applicationIdentifier
			.ApplicationVersion = Tm2Database.FormatVersion(My.Application.Info.Version, "X")
			.Category = category
			.Computer = My.Computer.Name
			.Description = message
		End With
		KaEventLog.CreateEventLog(Tm2Database.Connection, newEvent, interval, Nothing, _applicationIdentifier, "")
	End Sub

	Public Sub WriteToLog(message As String, logType As Common.Logger.LogFileType)
		WriteToLog(message, logType, True)
	End Sub

	Public Sub WriteToLog(message As String, logType As Common.Logger.LogFileType, writeToTm2EventLog As Boolean)
		If (writeToTm2EventLog) Then
			Select Case logType
				Case Common.Logger.LogFileType.Alarm
					WriteToEventLog(message, KaEventLog.Categories.Failure)
				Case Common.Logger.LogFileType.Information
					WriteToEventLog(message, KaEventLog.Categories.Information)
				Case Common.Logger.LogFileType.Warnings
					WriteToEventLog(message, KaEventLog.Categories.Warning)
			End Select
		End If

		_logger.Write(message, logType)
		Console.WriteLine(message)
	End Sub

	Public Sub ExceptionHandler(message As String, errorMethod As String)
		WriteToLog($"{errorMethod}: {message}", Common.Logger.LogFileType.Alarm)  ' This will log the interface error in the application's log

		Dim interval = KaSetting.GetSetting(Tm2Database.Connection, "@" & Net.Dns.GetHostName() & "/Tm2TankStatusReader/ResendInterval", "12")
		Dim recipients = KaSetting.GetSetting(Tm2Database.Connection, "@" & Net.Dns.GetHostName() & "/Tm2TankStatusReader/EmailAddress", "12")

		Dim email As KaEmail = New KaEmail
		With email
			.ApplicationId = My.Application.Info.AssemblyName
			.Body = message
			.Deleted = False
			.ErrorDetails = errorMethod
			.Recipients = recipients
			.Subject = _applicationIdentifier & " Alert"
		End With
		KaEmail.CreateEmail(Tm2Database.Connection, email, interval, Nothing, _applicationIdentifier, "")  'Will only create if it hasn't already been sent for the errorDetails within the resend interval (in hours)
	End Sub

	Private Function CheckActivation() As Boolean
		WriteToLog("Checking License...", Common.Logger.LogFileType.Processing)
		Return Common.CheckAuthorized(False, AddressOf LicenseAlmostExpired, AddressOf InTrialMode, AddressOf DeactivatedMessage, True)
	End Function

	Private Sub LicenseAlmostExpired(daysLeft As Integer)
		Dim logMessage = daysLeft.ToString() & " day" + IIf(daysLeft = 1, " ", "s ") & "remaining before license expires"
		WriteToLog(logMessage, Common.Logger.LogFileType.Warnings, False)
	End Sub

	Private Sub InTrialMode(daysLeft As Integer)
		Dim logMessage = "Running in trial mode. " & daysLeft.ToString() & " day" + IIf(daysLeft = 1, " ", "s ") & "remaining in trial cycle."
		WriteToLog(logMessage, Common.Logger.LogFileType.Warnings, False)
	End Sub

	Private Sub DeactivatedMessage(message As String)
		WriteToLog(message, Common.Logger.LogFileType.Alarm)
	End Sub
End Module
