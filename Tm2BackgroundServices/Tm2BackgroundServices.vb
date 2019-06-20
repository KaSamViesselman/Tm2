Imports KahlerAutomation.KaTm2Database
Imports KaCommonObjects
Imports KahlerAutomation
Imports KahlerAutomation.TerminalManagement2

Public Class Tm2BackgroundServices

	Private _logger As Common.Logger = New Common.Logger(KaSetting.SN_BACKGROUND_SERVICES_LOG_PATH, KaSetting.SD_BACKGROUND_SERVICES_LOG_PATH, My.Application.Info.AssemblyName, Reflection.Assembly.GetEntryAssembly())

	Private Sub Form1_Load(sender As Object, e As System.EventArgs) Handles Me.Load
		Me.WindowState = FormWindowState.Minimized
		Me.ShowInTaskbar = False

		Dim args As Collections.ObjectModel.ReadOnlyCollection(Of String) = My.Application.CommandLineArgs

		Dim i As Integer = 0
		If args.Count = 0 Then
			ShowHelp()
			End
		End If

		If (Common.CheckInstalling()) Then
			_logger.Write("Tm2BackgroundServices cannot run because an update is installing", Common.Logger.LogFileType.Alarm)
			End
		End If

		Try
			Common.SetRunning()
			Dim invalidParameterSent = False
			Do While i < args.Count
				Select Case args(i).ToLower.Trim
					Case "-expired_orders"
						CheckForExpiredOrders()
					Case "-event_log"
						CleanEventLog()
					Case Else
						invalidParameterSent = True
				End Select
				i += 1
			Loop
			If invalidParameterSent Then
				ShowHelp()
			End If
		Finally
			Common.SetNotRunning()
		End Try
		End
	End Sub

	Private Sub ShowHelp()
		_logger.Write("Usage: Tm2BackgroundServices [-expired_orders][-event_log]" & vbCrLf &
					   "-expired_orders (Will check for expired orders)" & vbCrLf &
					   "-event_log (will clean event log)", Common.Logger.LogFileType.Alarm)
	End Sub

	Private Sub CheckForExpiredOrders()
		Dim allExpiredOrders As ArrayList = KaOrder.GetAll(Tm2Database.Connection, "expiration_date < " & Q(Now) & " AND deleted = 0 AND completed = 0 AND expiration_date > " & Q(New DateTime(1900, 1, 1)), "")
		For Each order As KaOrder In allExpiredOrders
			order.Notes += IIf(order.Notes.Length > 0, Environment.NewLine, "") & "Order Expired, marked complete"
			order.Completed = True
			order.SqlUpdate(Tm2Database.Connection, Nothing, System.Net.Dns.GetHostName() & "/" & My.Application.Info.ProductName, "")
			_logger.Write("Marked order: " & order.Number & " complete because it was expired.", Common.Logger.LogFileType.Alarm)
		Next
	End Sub

	Private Sub CleanEventLog()
		Dim eventLogMinDate As Integer = 365
		Integer.TryParse(KaSetting.GetSetting(Tm2Database.Connection, "General/DaysToKeepEventLogRecords", eventLogMinDate), eventLogMinDate)
		_logger.Write(String.Format("Keep event log records for {0:0} days", eventLogMinDate), Common.Logger.LogFileType.Alarm)

		' delete old e-mail records
		Tm2Database.ExecuteNonQuery(Tm2Database.Connection, String.Format("DELETE FROM event_log WHERE last_updated < '{0:M/d/yyyy}'", Now.Subtract(New TimeSpan(eventLogMinDate, 0, 0, 0))))
	End Sub
End Class
