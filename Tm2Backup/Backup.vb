Imports System.Environment
Imports System.Math
Imports System.Xml
Imports System.Data.OleDb
Imports System.IO
Imports System.Net
Imports System.Net.Mail
Imports KahlerAutomation.KaTm2Database
Imports System.Linq
Imports KahlerAutomation
Imports KahlerAutomation.TerminalManagement2
Imports System.Reflection

Module Backup

	Private _logger As Common.Logger = New Common.Logger(KaSetting.SN_TM2_BACKUP_LOG_PATH, KaSetting.SD_TM2_BACKUP_LOG_PATH, My.Application.Info.AssemblyName, Assembly.GetEntryAssembly())
	Sub Main()
		Dim applicationArguments As String = Interaction.Command()
		Try
			ParseOptions(applicationArguments, "-h")
			DisplayHelp()
		Catch ex As ArgumentException
		End Try

		If (Common.CheckInstalling()) Then
			_logger.Write("Tm2Backup cannot run because an update is installing", Common.Logger.LogFileType.Alarm)
			End
		End If

		Dim systemBackupOptions As String = ""
		Dim systemBackupFilename As String = ""
		Try
			Common.SetRunning()

			Try
				systemBackupOptions = ParseOptions(applicationArguments, "-d").ToLower()
				Try
					systemBackupFilename = ParseOptions(applicationArguments, "-f")
				Catch ex As ArgumentException
					systemBackupFilename = ""
				End Try
				If systemBackupOptions = "backup" AndAlso systemBackupFilename.Length = 0 Then
					Console.WriteLine("System backup file name must be specified.")
					DisplayHelp()
				ElseIf systemBackupOptions = "restore" AndAlso (systemBackupFilename.Length = 0 OrElse Not IO.File.Exists(systemBackupFilename)) Then
					Console.WriteLine("System backup file does not exist.")
					DisplayHelp()
				End If
			Catch ex As ArgumentException

			End Try
			If systemBackupOptions = "backup" Then
				BackupSystemSettings(systemBackupFilename)
			ElseIf systemBackupOptions = "restore" Then
				RestoreSystemSettings(systemBackupFilename)
			Else
				BackupDatabase(applicationArguments)
			End If
		Finally
			Common.SetNotRunning()
		End Try

		End
	End Sub

	Private Sub BackupDatabase(applicationArguments As String)
		Dim emailServer As String = "smtp.kahlerautomation.com"
		Dim emailAddress As String = "customer@kahlerautomation.com"
		Dim emailUsername As String = "customer@kahlerautomation.com"
		Dim emailPassword As String = "customer1"
		Dim emailRecipient As String = ""
		Dim emailErrorSubject As String = ""
		Dim emailValidSubject As String = ""
		Dim backupTimeout As Integer = -1
		Dim message As String = ""
		Dim backupMoveException As Exception = Nothing
		Try
			Dim timeStamp As String = Format(Now, "yyyyMMddHHmmss")
			Try
				emailRecipient = ParseOptions(applicationArguments, "-e")
			Catch ex As ArgumentException
			End Try
			Try
				emailErrorSubject = ParseOptions(applicationArguments, "-s")
			Catch ex As Exception
			End Try
			Try
				emailValidSubject = ParseOptions(applicationArguments, "-v")
			Catch ex As ArgumentException
			End Try

			Dim dbConnection As String = Tm2Database.GetDbConnection()
			AddMessageToTextAndWriteToConsole(message, "Database connection: " & dbConnection & NewLine)

			Dim backupPath As String = ""
			Try
				backupPath = ParseOptions(applicationArguments, "-p")
			Catch ex As Exception
				Dim backupPathSetting As ArrayList = KaSetting.GetAll(dbConnection, "deleted = 0 AND name = 'General/DatabaseBackupDestinationPath'", "last_updated")
				If backupPathSetting.Count > 0 Then
					backupPath = CType(backupPathSetting(0), KaSetting).Value
				End If
			End Try
			If backupPath.Trim.Length = 0 Then Throw New ArgumentException("Database backup destination path not defined.")
			AddMessageToTextAndWriteToConsole(message, "Backup path: " & backupPath & NewLine)

			Dim maxBackupSets As Integer = -1
			Try
				maxBackupSets = Integer.Parse(ParseOptions(applicationArguments, "-c"))
			Catch ex As Exception
				Dim maxBackupSetsSetting As ArrayList = KaSetting.GetAll(dbConnection, "deleted = 0 AND name = 'General/DatabaseBackupMaxSets'", "last_updated")
				If maxBackupSetsSetting.Count > 0 Then
					Integer.TryParse(CType(maxBackupSetsSetting(0), KaSetting).Value, maxBackupSets)
				End If
			End Try
			If maxBackupSets <= 0 Then Throw New ArgumentException("Maximum number of backup sets not defined.")
			AddMessageToTextAndWriteToConsole(message, "Backup sets: " & maxBackupSets & NewLine)

			Try
				backupTimeout = Integer.Parse(ParseOptions(applicationArguments, "-t"))
			Catch ex As Exception
				Dim backupTimeoutSettings As ArrayList = KaSetting.GetAll(dbConnection, "deleted = 0 AND name = 'General/DatabaseBackupTimeout'", "last_updated")
				If backupTimeoutSettings.Count > 0 Then
					Integer.TryParse(CType(backupTimeoutSettings(0), KaSetting).Value, backupTimeout)
				End If
			End Try
			If backupTimeout <= 0 Then backupTimeout = Tm2Database.CommandTimeout

			emailServer = ReadSetting(dbConnection, "General/OutgoingSMTP", emailServer)
			AddMessageToTextAndWriteToConsole(message, "E-mail server: " & emailServer & NewLine)
			emailAddress = ReadSetting(dbConnection, "General/ServerEmailAddress", emailAddress)
			AddMessageToTextAndWriteToConsole(message, "E-mail address: " & emailAddress & NewLine)
			emailUsername = ReadSetting(dbConnection, "General/EmailUsername", emailUsername)
			AddMessageToTextAndWriteToConsole(message, "E-mail username: " & emailUsername & NewLine)
			emailPassword = ReadSetting(dbConnection, "General/EmailPassword", emailPassword)
			Dim databaseName As String = GetDatabaseName(dbConnection)
			AddMessageToTextAndWriteToConsole(message, "Database name: " & databaseName & NewLine)
			Dim databasePath As String = GetDatabasePath(dbConnection, databaseName)
			AddMessageToTextAndWriteToConsole(message, "Database path: " & databasePath & NewLine)
			AddMessageToTextAndWriteToConsole(message, "Backing up database... ")
			Dim backupFile As String = BackupDatabase(dbConnection, databaseName, timeStamp, backupTimeout)
			WriteToEventLog("Backed up " & databaseName & " with name " & backupFile, KaEventLog.Categories.Information)
			AddMessageToTextAndWriteToConsole(message, "Done" & NewLine)
			Dim backupLocation As String = GetBackupLocation(dbConnection, databaseName, databasePath & "\" & backupFile)
			AddMessageToTextAndWriteToConsole(message, "Database backed up to " & backupLocation & NewLine)
			AddMessageToTextAndWriteToConsole(message, "Moving database backup to " & backupPath & "... ")
			Try
				File.Move(backupLocation, backupPath & "\" & backupFile)
				WriteToEventLog("Backed moved to " & backupPath & "\" & backupFile, KaEventLog.Categories.Information)
				AddMessageToTextAndWriteToConsole(message, "File moved." & NewLine)
				AddMessageToTextAndWriteToConsole(message, "Copying configuration file to " & backupPath & "...")
				If File.Exists("c:\kaco\TerminalManagement2\config.xml") Then File.Copy("c:\kaco\TerminalManagement2\config.xml", backupPath & "\" & timeStamp & "_config.xml")
				AddMessageToTextAndWriteToConsole(message, "File copied." & NewLine)
			Catch ex As IOException
				backupPath = IO.Path.GetDirectoryName(backupLocation)
				backupMoveException = ex
				AddMessageToTextAndWriteToConsole(message, "Unable to move file." & NewLine & "Backup located at " & backupPath & NewLine)
				WriteToEventLog("Unable to move file." & NewLine & "Backup located at " & backupPath, KaEventLog.Categories.Failure)
			End Try
			AddMessageToTextAndWriteToConsole(message, "Cleaning up old backup sets..." & NewLine)
			Dim patters() As String = {"_" & databaseName & ".bak", "_config.xml"}
			For Each p As String In patters
				Dim oldest As ULong = 99999999999999
				Do While GetFileCount(backupPath & "\", "*" & p, oldest) > maxBackupSets
					AddMessageToTextAndWriteToConsole(message, "Deleting " & backupPath & "\" & Format(oldest, "00000000000000") & p & "... ")
					File.Delete(backupPath & "\" & Format(oldest, "00000000000000") & p)
					AddMessageToTextAndWriteToConsole(message, "File deleted." & NewLine)
					oldest = 99999999999999
				Loop
			Next
			If backupMoveException IsNot Nothing Then Throw backupMoveException
			SendEmail(emailServer, emailUsername, emailPassword, emailAddress, emailRecipient, emailValidSubject, message)
		Catch ex As ArgumentException
			message = "Tm2Backup: missing path and/or set count operands" & NewLine & "Try 'Tm2Backup -h' for more information."
			Console.WriteLine(message)
			WriteToEventLog(message, KaEventLog.Categories.Failure)
			_logger.Write(message & NewLine & NewLine & applicationArguments, Common.Logger.LogFileType.Alarm)
			SendEmail(emailServer, emailUsername, emailPassword, emailAddress, emailRecipient, emailErrorSubject, "The following error was encountered when attempting to backup Kahler Automation Tm2's database:" & NewLine & NewLine & message & NewLine & NewLine & applicationArguments)
			Environment.Exit(System.Runtime.InteropServices.Marshal.GetHRForException(ex))
		Catch ex As Exception
			message &= NewLine & "Unhandled exception:" & NewLine & NewLine & ex.ToString() & NewLine & NewLine
			WriteToEventLog(message, KaEventLog.Categories.Failure)
			If ex.InnerException IsNot Nothing Then message &= "   Inner exception: " & NewLine & NewLine & ex.InnerException.ToString() & NewLine & NewLine
			message &= "Check backup configuration, backup may not have completed correctly."
			Console.WriteLine(message)
			_logger.Write(message, Common.Logger.LogFileType.Alarm)
			SendEmail(emailServer, emailUsername, emailPassword, emailAddress, emailRecipient, emailErrorSubject, "The following error was encountered when attempting to backup Kahler Automation Tm2's database:" & NewLine & NewLine & message)
			Environment.Exit(System.Runtime.InteropServices.Marshal.GetHRForException(ex))
		End Try
	End Sub

	Private Sub BackupSystemSettings(systemBackupFilename As String)
		Dim dbConnection As String = Tm2Database.GetDbConnection()
		Dim message As String = ""
		AddMessageToTextAndWriteToConsole(message, "Database connection: " & dbConnection & NewLine)
		AddMessageToTextAndWriteToConsole(message, "Backup path: " & systemBackupFilename & NewLine)
		Try
			Dim connection As OleDbConnection = Tm2Database.Connection

			AddMessageToTextAndWriteToConsole(message, "Checking database...")
			Dim tm2Db As New Tm2Database(dbConnection)
			tm2Db.CheckDatabase(connection, True, False)
			AddMessageToTextAndWriteToConsole(message, "done." & NewLine)
			_logger.LoadPath(KaSetting.SD_TM2_BACKUP_LOG_PATH)

			Dim systemBackup As New SystemBackupItems
			With systemBackup
				.Bays = KaBay.GetAll(connection, "deleted=0", "name").Cast(Of KaBay)().ToList()
				.BulkProductPanelSettings = KaBulkProductPanelSettings.GetAll(connection, "deleted=0", "").Cast(Of KaBulkProductPanelSettings)().ToList()
				.BulkProductPanelStorageLocations = KaBulkProductPanelStorageLocation.GetAll(connection, $"{KaBulkProductPanelStorageLocation.FN_DELETED} = 0 AND {KaBulkProductPanelStorageLocation.FN_STORAGE_LOCATION_ID} IN (SELECT {KaStorageLocation.FN_ID} FROM {KaStorageLocation.TABLE_NAME} WHERE {KaStorageLocation.FN_DELETED} = 0 AND {KaStorageLocation.FN_CONTAINER_ID} = {Guid.Empty})", "").Cast(Of KaBulkProductPanelStorageLocation)().ToList()
				.BulkProducts = KaBulkProduct.GetAll(connection, "deleted=0", "name").Cast(Of KaBulkProduct)().ToList()
				.DischargeLocationPanelSettings = KaDischargeLocationPanelSettings.GetAll(connection, "deleted=0", "").Cast(Of KaDischargeLocationPanelSettings)().ToList()
				.DischargeLocationStorageLocations = KaDischargeLocationStorageLocation.GetAll(connection, $"{KaDischargeLocationStorageLocation.FN_DELETED} = 0 AND {KaDischargeLocationStorageLocation.FN_STORAGE_LOCATION_ID} IN (SELECT {KaStorageLocation.FN_ID} FROM {KaStorageLocation.TABLE_NAME} WHERE {KaStorageLocation.FN_DELETED} = 0 AND {KaStorageLocation.FN_CONTAINER_ID} = {Guid.Empty})", "").Cast(Of KaDischargeLocationStorageLocation)().ToList()
				.DischargeLocations = KaDischargeLocation.GetAll(connection, "deleted=0", "name").Cast(Of KaDischargeLocation)().ToList()
				.Locations = KaLocation.GetAll(connection, "deleted=0", "name").Cast(Of KaLocation)().ToList()
				.PanelGroupPanels = KaPanelGroupPanel.GetAll(connection, "deleted=0", "").Cast(Of KaPanelGroupPanel)().ToList()
				.PanelGroups = KaPanelGroup.GetAll(connection, "deleted=0", "name").Cast(Of KaPanelGroup)().ToList()
				.Panels = KaPanel.GetAll(connection, "deleted=0", "name").Cast(Of KaPanel)().ToList()
				.StorageLocations = KaStorageLocation.GetAll(connection, $"{KaStorageLocation.FN_DELETED} = 0 AND {KaStorageLocation.FN_CONTAINER_ID} = {Guid.Empty}", "").Cast(Of KaStorageLocation)().ToList()
				.TankGroups = KaTankGroup.GetAll(connection, "deleted=0", "name").Cast(Of KaTankGroup)().ToList()
				.TankGroupTanks = KaTankGroupTank.GetAll(connection, "deleted=0", "").Cast(Of KaTankGroupTank)().ToList()
				.TankLevelTrends = KaTankLevelTrend.GetAll(connection, "deleted=0", "name").Cast(Of KaTankLevelTrend)().ToList()
				.Tanks = KaTank.GetAll(connection, "deleted=0", "name").Cast(Of KaTank)().ToList()
				.Tracks = KaTrack.GetAll(connection, "deleted=0", "name").Cast(Of KaTrack)().ToList()
				.Units = KaUnit.GetAll(connection, "deleted=0", "name").Cast(Of KaUnit)().ToList()
			End With
			AddCustomFields(connection, systemBackup)

			If IO.File.Exists(systemBackupFilename) Then
				IO.File.Delete(systemBackupFilename)
				'IO.File.Move(systemBackupFilename, systemBackupFilename.Replace(IO.Path.GetExtension(systemBackupFilename), String.Format(".{0:yyyyMMddHHmmss}{1}", DateTime.Now, IO.Path.GetExtension(systemBackupFilename))))
			End If
			IO.File.WriteAllText(systemBackupFilename, Tm2Database.ToXml(systemBackup, GetType(SystemBackupItems)))
		Catch ex As Exception
			Dim errorMessage As String = "Unhandled exception:" & NewLine & NewLine & ex.ToString() & NewLine & NewLine
			If ex.InnerException IsNot Nothing Then errorMessage &= "   Inner exception: " & NewLine & NewLine & ex.InnerException.ToString() & NewLine & NewLine
			errorMessage &= "Check backup configuration, backup may not have completed correctly."
			WriteToEventLog(message & NewLine & errorMessage, KaEventLog.Categories.Failure)
			Console.WriteLine(errorMessage)
			_logger.Write(message & NewLine & errorMessage, Common.Logger.LogFileType.Alarm)
		End Try
	End Sub

	Private Sub AddCustomFields(connection As OleDbConnection, ByRef systemBackup As SystemBackupItems)
		For Each tableName As String In New List(Of String)({KaBay.TABLE_NAME, KaBulkProductPanelSettings.TABLE_NAME, KaBulkProduct.TABLE_NAME, KaDischargeLocationPanelSettings.TABLE_NAME, KaDischargeLocation.TABLE_NAME, KaLocation.TABLE_NAME, KaPanelGroupPanel.TABLE_NAME, KaPanelGroup.TABLE_NAME, KaPanel.TABLE_NAME, KaTankGroup.TABLE_NAME, KaTankGroupTank.TABLE_NAME, KaTankLevelTrend.TABLE_NAME, KaTank.TABLE_NAME, KaTrack.TABLE_NAME, KaUnit.TABLE_NAME})
			systemBackup.CustomFields.AddRange(KaCustomField.GetAll(connection, "deleted = 0 AND table_name = " & Database.Q(tableName), "field_name").Cast(Of KaCustomField).ToList())
			systemBackup.CustomFieldData.AddRange(KaCustomFieldData.GetAll(connection, String.Format("id IN (SELECT cfd.id FROM custom_field_data AS cfd INNER JOIN custom_fields AS cf ON cfd.custom_field_id = cf.id INNER JOIN {0} ON {0}.id = cfd.record_id WHERE ({0}.deleted = 0) AND (cf.deleted = 0) AND (cfd.deleted = 0))", tableName), "id").Cast(Of KaCustomFieldData).ToList())
		Next
	End Sub

	Private Sub RestoreSystemSettings(systemBackupFilename As String)
		Dim message As String = ""
		Dim dbConnection As String = Tm2Database.GetDbConnection()
		AddMessageToTextAndWriteToConsole(message, "Database connection: " & dbConnection & NewLine)
		AddMessageToTextAndWriteToConsole(message, "Backup path: " & systemBackupFilename & NewLine)
		Dim connection As New OleDbConnection(dbConnection)
		Dim transaction As OleDbTransaction = Nothing
		Try
			connection.Open()
			AddMessageToTextAndWriteToConsole(message, "Checking database...")
			Dim tm2Db As New Tm2Database(dbConnection)
			tm2Db.CheckDatabase(connection, True, False)
			AddMessageToTextAndWriteToConsole(message, "done." & NewLine)
			_logger.LoadPath(KaSetting.SD_TM2_BACKUP_LOG_PATH)

			Dim systemBackup As SystemBackupItems = Tm2Database.FromXml(IO.File.ReadAllText(systemBackupFilename), GetType(SystemBackupItems))
			transaction = connection.BeginTransaction
			With systemBackup
				For Each bay As KaBay In .Bays
					If Not Tm2ObjectExists(connection, transaction, KaBay.TABLE_NAME, bay.Id) Then bay.SqlUpdateInsertIfNotFound(connection, transaction, "TM2 Backup System Restore", My.User.Name)
				Next
				For Each setting As KaBulkProductPanelSettings In .BulkProductPanelSettings
					If Not Tm2ObjectExists(connection, transaction, KaBulkProductPanelSettings.TABLE_NAME, setting.Id) Then setting.SqlUpdateInsertIfNotFound(connection, transaction, "TM2 Backup System Restore", My.User.Name)
				Next
				For Each bppsl As KaBulkProductPanelStorageLocation In .BulkProductPanelStorageLocations
					If Not Tm2ObjectExists(connection, transaction, KaBulkProductPanelStorageLocation.TABLE_NAME, bppsl.Id) Then bppsl.SqlUpdateInsertIfNotFound(connection, transaction, "TM2 Backup System Restore", My.User.Name)
				Next
				For Each bulkProduct As KaBulkProduct In .BulkProducts
					If Not Tm2ObjectExists(connection, transaction, KaBulkProduct.TABLE_NAME, bulkProduct.Id) Then bulkProduct.SqlUpdateInsertIfNotFound(connection, transaction, "TM2 Backup System Restore", My.User.Name)
				Next
				For Each setting As KaDischargeLocationPanelSettings In .DischargeLocationPanelSettings
					If Not Tm2ObjectExists(connection, transaction, KaDischargeLocationPanelSettings.TABLE_NAME, setting.Id) Then setting.SqlUpdateInsertIfNotFound(connection, transaction, "TM2 Backup System Restore", My.User.Name)
				Next
				For Each dlsl As KaDischargeLocationStorageLocation In .DischargeLocationStorageLocations
					If Not Tm2ObjectExists(connection, transaction, KaDischargeLocationStorageLocation.TABLE_NAME, dlsl.Id) Then dlsl.SqlUpdateInsertIfNotFound(connection, transaction, "TM2 Backup System Restore", My.User.Name)
				Next
				For Each location As KaDischargeLocation In .DischargeLocations
					If Not Tm2ObjectExists(connection, transaction, KaDischargeLocation.TABLE_NAME, location.Id) Then location.SqlUpdateInsertIfNotFound(connection, transaction, "TM2 Backup System Restore", My.User.Name)
				Next
				For Each location As KaLocation In .Locations
					If Not Tm2ObjectExists(connection, transaction, KaLocation.TABLE_NAME, location.Id) Then location.SqlUpdateInsertIfNotFound(connection, transaction, "TM2 Backup System Restore", My.User.Name)
				Next
				For Each panel As KaPanelGroupPanel In .PanelGroupPanels
					If Not Tm2ObjectExists(connection, transaction, KaPanelGroupPanel.TABLE_NAME, panel.Id) Then panel.SqlUpdateInsertIfNotFound(connection, transaction, "TM2 Backup System Restore", My.User.Name)
				Next
				For Each group As KaPanelGroup In .PanelGroups
					If Not Tm2ObjectExists(connection, transaction, KaPanelGroup.TABLE_NAME, group.Id) Then group.SqlUpdateInsertIfNotFound(connection, transaction, "TM2 Backup System Restore", My.User.Name)
				Next
				For Each panel As KaPanel In .Panels
					If Not Tm2ObjectExists(connection, transaction, KaPanel.TABLE_NAME, panel.Id) Then
						panel.SqlUpdateInsertIfNotFound(connection, transaction, "TM2 Backup System Restore", My.User.Name)
						panel.SqlUpdateKa2000Information(connection, transaction, "TM2 Backup System Restore", My.User.Name)
					End If
				Next
				For Each sl As KaStorageLocation In .StorageLocations
					If Not Tm2ObjectExists(connection, transaction, KaStorageLocation.TABLE_NAME, sl.Id) Then sl.SqlUpdateInsertIfNotFound(connection, transaction, "TM2 Backup System Restore", My.User.Name)
				Next
				For Each group As KaTankGroup In .TankGroups
					If Not Tm2ObjectExists(connection, transaction, KaTankGroup.TABLE_NAME, group.Id) Then group.SqlUpdateInsertIfNotFound(connection, transaction, "TM2 Backup System Restore", My.User.Name)
				Next
				For Each tank As KaTankGroupTank In .TankGroupTanks
					If Not Tm2ObjectExists(connection, transaction, KaTankGroupTank.TABLE_NAME, tank.Id) Then tank.SqlUpdateInsertIfNotFound(connection, transaction, "TM2 Backup System Restore", My.User.Name)
				Next
				For Each trend As KaTankLevelTrend In .TankLevelTrends
					If Not Tm2ObjectExists(connection, transaction, KaTankLevelTrend.TABLE_NAME, trend.Id) Then trend.SqlUpdateInsertIfNotFound(connection, transaction, "TM2 Backup System Restore", My.User.Name)
				Next
				For Each track As KaTank In .Tanks
					If Not Tm2ObjectExists(connection, transaction, KaTank.TABLE_NAME, track.Id) Then track.SqlUpdateInsertIfNotFound(connection, transaction, "TM2 Backup System Restore", My.User.Name)
				Next
				For Each track As KaTrack In .Tracks
					If Not Tm2ObjectExists(connection, transaction, KaTrack.TABLE_NAME, track.Id) Then track.SqlUpdateInsertIfNotFound(connection, transaction, "TM2 Backup System Restore", My.User.Name)
				Next
				For Each unit As KaUnit In .Units
					If Not Tm2ObjectExists(connection, transaction, KaUnit.TABLE_NAME, unit.Id) Then unit.SqlUpdateInsertIfNotFound(connection, transaction, "TM2 Backup System Restore", My.User.Name)
				Next
				For Each field As KaCustomField In .CustomFields
					If Not Tm2ObjectExists(connection, transaction, KaCustomField.TABLE_NAME, field.Id) Then field.SqlUpdateInsertIfNotFound(connection, transaction, "TM2 Backup System Restore", My.User.Name)
				Next
				For Each unit As KaCustomFieldData In .CustomFieldData
					If Not Tm2ObjectExists(connection, transaction, KaCustomFieldData.TABLE_NAME, unit.Id) Then unit.SqlUpdateInsertIfNotFound(connection, transaction, "TM2 Backup System Restore", My.User.Name)
				Next
			End With
			transaction.Commit()
		Catch ex As Exception
			If transaction IsNot Nothing Then transaction.Rollback()
			Dim errorMessage As String = "Unhandled exception:" & NewLine & NewLine & ex.ToString() & NewLine & NewLine
			If ex.InnerException IsNot Nothing Then errorMessage &= "   Inner exception: " & NewLine & NewLine & ex.InnerException.ToString() & NewLine & NewLine
			errorMessage &= "Check restore configuration, restore may not have completed correctly."
			WriteToEventLog(message & NewLine & errorMessage, KaEventLog.Categories.Failure)
			Console.WriteLine(errorMessage)
			WriteLog(message & NewLine & errorMessage)
		Finally
			If transaction IsNot Nothing Then transaction.Dispose()
			If connection IsNot Nothing Then connection.Close()
		End Try
	End Sub

	Private Function Tm2ObjectExists(connection As OleDbConnection, transaction As OleDbTransaction, tableName As String, id As Guid) As Boolean
		Tm2Database.ExecuteNonQuery(connection, transaction, String.Format("DELETE FROM {0} WHERE deleted = 1 AND id = {1}", tableName, Database.Q(id)))
		Dim count As Integer = 0
		Dim rdr As OleDbDataReader = Tm2Database.ExecuteReader(connection, transaction, String.Format("SELECT COUNT(*) FROM {0} WHERE id = {1}", tableName, Database.Q(id)))

		If rdr.Read() Then
			count = rdr.Item(0)
		Else
			Throw New Exception(String.Format("Unable to determine if ID {0} exists in {1}", Database.Q(id), tableName))
		End If
		If count > 0 Then
			Return True
		Else
			Tm2Database.ExecuteNonQuery(connection, transaction, String.Format("INSERT INTO {0} (id) VALUES ({1})", tableName, Database.Q(id)))
			Return False
		End If
	End Function

	Private Sub DisplayHelp()
		Console.WriteLine("")
		Console.WriteLine("Usage: Tm2Backup [[-p PATH] [-c SET COUNT] [-e EMAIL RECIPIENT [-s ERROR EMAIL SUBJECT] [-v VALID EMAIL SUBJECT]] [-t BACKUP TIMEOUT]] | [-d BACKUP|RESTORE -f FILENAME]")
		Console.WriteLine("")
		Console.WriteLine("Stores a number of backup sets, specified by SET COUNT, of the TM2 database to path specified by PATH.")
		Console.WriteLine("If an error occurs during the database backup, an e-mail may be sent to EMAIL RECIPIENT with the ERROR EMAIL SUBJECT indicating where the error is coming from. Omit EMAIL RECIPIENT or ERROR EMAIL SUBJECT to prevent email from being generated.")
		Console.WriteLine("If the database backup is successful, an e-mail may be sent to EMAIL RECIPIENT with the VALID EMAIL SUBJECT indicating that it was successful. Omit EMAIL RECIPIENT or VALID EMAIL SUBJECT to prevent email from being generated.")
		Console.WriteLine("")
		Console.WriteLine("Using the -d parameter sets the backup into system backup/restore mode.  It will use the BACKUP or RESTORE option to specify the process, and will use the file specified with the FILENAME parameter.")
		Console.WriteLine("    For example: Tm2Backup -d backup -f Tm2SystemBackup.xml")
		End
	End Sub

	Private Sub WriteToEventLog(ByVal message As String, ByVal category As KaEventLog.Categories)
		Try
			Dim eventLogEntry As KaEventLog = New KaEventLog
			eventLogEntry.ApplicationIdentifier = System.Net.Dns.GetHostName() & "/" & "Tm2Backup"
			eventLogEntry.ApplicationVersion = Tm2Database.FormatVersion(System.Reflection.Assembly.GetEntryAssembly().GetName().Version, "X")
			eventLogEntry.Category = category
			eventLogEntry.Computer = System.Net.Dns.GetHostName()
			eventLogEntry.Description = message
			eventLogEntry.LastUpdatedApplication = "Tm2Backup"
			eventLogEntry.LastUpdatedUser = ""
			eventLogEntry.SqlInsert(Tm2Database.Connection, Nothing, "", "")
		Catch ex As Exception
			' Suppress
		End Try
	End Sub

	Private Function ParseOptions(ByVal arguments As String, ByVal flag As String) As String
		flag = flag.ToLower()
		Dim i As Integer = arguments.ToLower().IndexOf(flag)
		If i > -1 AndAlso flag = "-h" Then
			Return ""
		ElseIf i > -1 Then
			Dim inQuotes As Boolean = False
			arguments = arguments.Substring(i + 3, arguments.Length - (i + 3))
			i = 0
			Do While i < arguments.Length
				If arguments.Substring(i, 1) = """" Then
					inQuotes = Not inQuotes
				ElseIf Not inQuotes AndAlso arguments.Substring(i, 1) = " " Then
					Exit Do
				End If
				i += 1
			Loop
			If arguments.Length > 0 Then Return arguments.Substring(0, i).Trim().Replace("""", "") Else Return ""
		End If
		Throw New ArgumentException("Argument not found")
	End Function

	Private Sub AddMessageToTextAndWriteToConsole(ByRef text As String, ByVal message As String)
		text &= message & Environment.NewLine
		Console.Write(message)
	End Sub

	Private Function ReadSetting(ByVal dbConnection As String, ByVal setting As String, ByVal defaultValue As String) As String
		With New OleDbCommand("SELECT value FROM settings WHERE name = '" & setting.Replace("'", "''") & "'", New OleDbConnection(dbConnection))
			Try
				.Connection.Open()
				Dim r As OleDbDataReader = .ExecuteReader()
				If r.Read() Then Return r("value") Else Return defaultValue
			Finally
				.Connection.Close()
			End Try
		End With
	End Function

	Private Function GetDatabaseName(ByVal dbConnection As String) As String
		Dim parameters() As String = {"database", "initial catalog"}
		Dim i As Integer = 0
		For Each parameter As String In parameters
			Dim j As Integer = dbConnection.ToLower().IndexOf(parameter)
			If j > -1 Then
				dbConnection = dbConnection.Substring(j + parameter.Length, dbConnection.Length - (j + parameter.Length))
				j = dbConnection.IndexOf("=") + 1
				Dim k As Integer = dbConnection.IndexOf(";")
				If k = -1 Then k = dbConnection.Length
				Return dbConnection.Substring(j, k - j).Trim()
			End If
		Next
		Throw New Exception("Database name not found")
	End Function

	Private Function GetDatabasePath(ByVal dbConnection As String, ByVal databaseName As String) As String
		With New OleDbCommand("SELECT filename FROM master.dbo.sysaltfiles WHERE name = db_name()", New OleDbConnection(dbConnection))
			Try
				.Connection.Open()
				Dim r As OleDbDataReader = .ExecuteReader()
				Do While r.Read()
					Dim tempFilename As String = r("filename").ToLower()
					tempFilename = tempFilename.Substring(tempFilename.IndexOf("\data\") + 6, tempFilename.Length - tempFilename.IndexOf("\data\") - 6)
					If tempFilename.ToLower = databaseName.ToLower & ".mdf" Then
						Return r("filename").ToLower().Replace("\data\" & databaseName.ToLower & ".mdf", "")
					End If
				Loop
			Finally
				.Connection.Close()
			End Try
		End With
		With New OleDbCommand("select physical_name FROM sys.master_files where DB_NAME(database_id)=db_name() AND type_desc='ROWS'", New OleDbConnection(dbConnection))
			Try
				.Connection.Open()
				Dim r As OleDbDataReader = .ExecuteReader()
				Do While r.Read()
					Dim tempFilename As String = r("physical_name").ToLower()
					tempFilename = tempFilename.Substring(tempFilename.IndexOf("\data\") + 6, tempFilename.Length - tempFilename.IndexOf("\data\") - 6)
					If tempFilename.ToLower = databaseName.ToLower & ".mdf" Then
						Return r("physical_name").ToLower().Replace("\data\" & databaseName.ToLower & ".mdf", "")
					End If
				Loop
			Finally
				.Connection.Close()
			End Try
		End With
		Throw New Exception("Database path not found")
	End Function

	Private Function BackupDatabase(ByVal dbConnection As String, ByVal databaseName As String, ByVal timeStamp As String, ByVal backupTimeout As Integer) As String
		Dim backupFile As String = timeStamp & "_" & databaseName & ".bak"
		With New OleDbCommand("BACKUP DATABASE " & databaseName & " TO DISK='" & backupFile & "'", New OleDbConnection(dbConnection))
			Try
				If backupTimeout >= 0 Then .CommandTimeout = backupTimeout
				.Connection.Open()
				.ExecuteNonQuery()
				Try
					.CommandText = "BEGIN" & vbCrLf &
									  "	DECLARE @logName AS NVARCHAR(128)" & vbCrLf &
									  "	SELECT        @logName= name" & vbCrLf &
									  "		FROM            sys.master_files" & vbCrLf &
									  "		WHERE        (DB_NAME(database_id) = DB_NAME()) AND (type_desc = 'LOG')" & vbCrLf &
									  "	DBCC SHRINKFILE (  @logName, 1)" & vbCrLf &
									  "END"
					.ExecuteNonQuery()
				Catch ex As Exception
					' Suppress the truncate log
				End Try

			Finally
				.Connection.Close()
			End Try
		End With
		Return backupFile
	End Function

	Private Function GetBackupLocation(ByVal dbConnection As String, ByVal databaseName As String, defaultBackupLocation As String) As String
		' Code pulled from http://stackoverflow.com/questions/17591942/get-default-backup-path-of-sql-server-prgrammatiacally
		Dim backupLocationCmd As New OleDbCommand("BEGIN" & vbCrLf &
													 "        SET NOCOUNT ON;" & vbCrLf &
													 "        DECLARE @SQLVer SQL_VARIANT" & vbCrLf &
													 "            ,@DBName VARCHAR(128)" & vbCrLf &
													 "            ,@NumDays   SMALLINT" & vbCrLf &
													 "            ,@SQL       VARCHAR(1024)" & vbCrLf &
													 "            ,@WhereClause   VARCHAR(256)" & vbCrLf &
													 "" & vbCrLf &
													 "    SET @DBName = Null" & vbCrLf &
													 "    ;" & vbCrLf &
													 "    SET @NumDays = 14" & vbCrLf &
													 "    ;" & vbCrLf &
													 "    SET @SQLVer = CONVERT(INTEGER, PARSENAME(CONVERT(VARCHAR(20),SERVERPROPERTY('ProductVersion')),4));" & vbCrLf &
													 "" & vbCrLf &
													 "    SET @WhereClause = 'WHERE a.type IN (''D'',''I'')" & vbCrLf &
													 "            And a.backup_start_date > GETDATE()- ' + CAST(@NumDays AS VARCHAR)+''" & vbCrLf &
													 "    IF @DBName IS NOT NULL" & vbCrLf &
													 "    BEGIN" & vbCrLf &
													 "        SET @WhereClause = @WhereClause + '" & vbCrLf &
													 "            AND a.database_name = '''+ @DBName +''''" & vbCrLf &
													 "    END" & vbCrLf &
													 "" & vbCrLf &
													 "    SET @SQL = '" & vbCrLf &
													 "    SELECT a.database_name,a.backup_start_date" & vbCrLf &
													 "            ,b.physical_device_name AS BackupPath" & vbCrLf &
													 "            ,a.position" & vbCrLf &
													 "            ,a.type" & vbCrLf &
													 "            ,a.backup_size/1024/1024 AS BackupSizeMB" & vbCrLf &
													 "            ,' + CASE " & vbCrLf &
													 "                WHEN @SQLVer < 10 " & vbCrLf &
													 "                    THEN '0'" & vbCrLf &
													 "                    ELSE 'a.compressed_backup_size/1024/1024'" & vbCrLf &
													 "                END + ' AS CompressedBackMB" & vbCrLf &
													 "        FROM msdb.dbo.backupset a" & vbCrLf &
													 "            INNER JOIN msdb.dbo.backupmediafamily b" & vbCrLf &
													 "                ON a.media_set_id = b.media_set_id" & vbCrLf &
													 "        ' + @WhereClause + '" & vbCrLf &
													 "        ORDER BY a.backup_start_date DESC;';" & vbCrLf &
													 "         --PRINT @SQL" & vbCrLf &
													 "     EXECUTE (@SQL);" & vbCrLf &
													 "    END;", New OleDbConnection(dbConnection))
		Try
			backupLocationCmd.Connection.Open()
			Dim backupLocationRdr As OleDbDataReader = backupLocationCmd.ExecuteReader
			Do While backupLocationRdr.Read
				If backupLocationRdr.Item("database_name").ToString.ToLower = databaseName.ToLower AndAlso IO.File.Exists(backupLocationRdr.Item("BackupPath")) Then
					defaultBackupLocation = backupLocationRdr.Item("BackupPath")
					Exit Do
				End If
			Loop
		Catch ex As Exception
		Finally
			backupLocationCmd.Connection.Close()
		End Try
		Return defaultBackupLocation
	End Function

	Private Function GetFileCount(ByVal folder As String, ByVal pattern As String, ByRef oldest As ULong) As Integer
		Dim fileCount As Integer = 0
		For Each f As String In Directory.GetFiles(folder, pattern)
			Try
				Dim parts() As String = f.Split("\")
				oldest = Min(oldest, ULong.Parse(parts(parts.Length - 1).Substring(0, 14)))
				fileCount += 1
			Catch ex As FormatException
			End Try
		Next
		Return fileCount
	End Function

	Public Sub WriteLog(message As String)
		_logger.Write(message, Common.Logger.LogFileType.Alarm)
	End Sub

	Private Sub SendEmail(ByVal emailServer As String, ByVal emailUsername As String, ByVal emailPassword As String, ByVal emailAddress As String, ByVal emailRecipient As String, ByVal emailSubject As String, ByVal message As String)
		If emailRecipient.Trim().Length > 0 AndAlso emailSubject.Trim().Length > 0 Then
			Try
				Dim s As New SmtpClient(emailServer)
				s.Credentials = New NetworkCredential(emailUsername, emailPassword)
				s.Send(New MailMessage(emailAddress, emailRecipient, emailSubject, message))
			Catch ex As Exception
			End Try
		End If
	End Sub

	Private Function GetCompleteFilePath(ByVal filepath As String) As String
		filepath = filepath.Trim
		Return filepath & IIf(filepath.Substring(filepath.Length - 1, 1) = "\", "", "\")
	End Function
End Module
