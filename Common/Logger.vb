Imports System.IO
Imports KahlerAutomation.KaTm2Database

Public Class Logger
	Enum LogFileType
		Alarm
		Information
		Processing
		Warnings
	End Enum

	Private _branch As String
	Private _appAssembly As Reflection.Assembly
	Private _appName As String
	Private _defaultLogFileSettingName As String
	Private _logPath As String

	Public ReadOnly Property LogPath As String
		Get
			Return _logPath
		End Get
	End Property

	Public Sub New(defaultLogFileSettingName As String, defaultLogFileDirectory As String, appName As String, appAssembly As Reflection.Assembly)
		_defaultLogFileSettingName = defaultLogFileSettingName
		_appName = appName
		_branch = KaCommonObjects.Assembly.GetDevBranch(appAssembly)
		_appAssembly = appAssembly
		LoadPath(defaultLogFileDirectory)
	End Sub

	Public Sub Write(message As String, type As LogFileType)
		If (String.IsNullOrEmpty(_logPath)) Then Return

		Dim logging As New KaCommonObjects.Logging()
		Dim logFile As String = Path.Combine(_logPath, _appName)
		Try
			Select Case type
				Case LogFileType.Alarm
					logFile &= "_AlarmHistory.txt"
				Case LogFileType.Information
					logFile &= "_ProcessLog.txt"
				Case LogFileType.Processing
					logFile &= "_ProcessLog.txt"
				Case LogFileType.Warnings
					logFile &= "_AlarmHistory.txt"
				Case Else
					logFile &= "_AlarmHistory.txt"
			End Select
			logging.WriteToLog(message, logFile, Reflection.Assembly.GetEntryAssembly(), _branch, False)
		Catch ex As Exception
			Console.WriteLine("Failed to log: " & KaCommonObjects.Alerts.FormatException(ex))
		End Try
	End Sub

	Public Sub LoadPath(defaultLogFileDirectory As String)
		If (Not String.IsNullOrEmpty(_logPath)) Then Return
		Try
			Dim tempPath = String.Empty
			tempPath = KaSetting.GetSetting(Tm2Database.Connection, _defaultLogFileSettingName, defaultLogFileDirectory)

			If (Not String.IsNullOrEmpty(tempPath)) Then
				_logPath = tempPath
			End If
		Catch ex As Exception
		End Try
	End Sub
End Class
