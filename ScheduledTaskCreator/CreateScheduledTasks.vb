Imports KahlerAutomation.KaTm2Database
Imports Microsoft.Win32.TaskScheduler
Imports System.Xml

Public Class CreateScheduledTasks
    Private Sub Form1_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load
        Dim tmDb As New KahlerAutomation.KaTm2Database.Tm2Database(KahlerAutomation.KaTm2Database.Tm2Database.GetDbConnection())
        tmDb.CheckDatabase(KahlerAutomation.KaTm2Database.Tm2Database.Connection, True, False)

        cmbInterface.Items.Clear()
        For Each interfaceObject As KaInterface In KahlerAutomation.KaTm2Database.KaInterface.GetAll(KahlerAutomation.KaTm2Database.Tm2Database.Connection, "deleted = 0", "name asc")
            cmbInterface.Items.Add(New KaCommonObjects.ComboBoxItem(interfaceObject.Name, interfaceObject.Id))
        Next
        If cmbInterface.Items.Count > 0 Then cmbInterface.SelectedIndex = 0
        gpbInterface.Enabled = (cmbInterface.Items.Count > 0)
    End Sub

    Private Sub btnCreateBackupScheduledTask_Click(sender As System.Object, e As System.EventArgs) Handles btnCreateBackupScheduledTask.Click
        Dim arguments As String = ""
        Dim ts As New TaskService()
        Dim backupTask As TaskDefinition = CreateBasicTask(ts, LOCAL_SYSTEM_SID)

        Dim backupDirectory = tbxBackupFolder.Text.Trim()
        If backupDirectory.Length > 0 Then
            If Not IO.Directory.Exists(backupDirectory) Then
                Try
                    IO.Directory.CreateDirectory(backupDirectory)
                Catch ex As Exception

                End Try
            End If
            arguments = "-p """ & backupDirectory & """"
        End If
        If nudNumberOfBackupCopies.Value > 0 Then arguments &= " -c " & nudNumberOfBackupCopies.Value.ToString()
        Dim action As New ExecAction(My.Application.Info.DirectoryPath & "\Tm2Backup.exe", arguments, My.Application.Info.DirectoryPath)
        With backupTask
            Dim backupTrigger As New DailyTrigger(1)
            backupTrigger.StartBoundary = New DateTime(Now.Year, Now.Month, Now.Day, 0, 0, 0)

            .Triggers.Add(backupTrigger)
            .Actions.Add(action)
        End With
        Dim taskName As String = GetTaskName(ts, action, "TM2 Backup")
        Try
            ' If (ts.HighestSupportedVersion >= New Version(1, 2)) Then
            ts.RootFolder.RegisterTaskDefinition(taskName, backupTask)
            'Else
            '    ts.RootFolder.RegisterTaskDefinition(taskName, backupTask, TaskCreation.CreateOrUpdate, "NT AUTHORITY\System", Nothing, TaskLogonType.ServiceAccount, Nothing)
            'End If
            MessageBox.Show("Backup Scheduled Task added")
        Catch ex As Exception
            MessageBox.Show(KaCommonObjects.Alerts.FormatException(ex))
        End Try
    End Sub

    Private Sub btnCreateEmailScheduledTask_Click(sender As System.Object, e As System.EventArgs) Handles btnCreateEmailScheduledTask.Click
        Dim ts As New TaskService()
        Dim emailTask As TaskDefinition = CreateBasicTask(ts, NETWORK_SERVICE_SID)
        Dim action As New ExecAction(My.Application.Info.DirectoryPath & "\Tm2EmailService.exe", "", My.Application.Info.DirectoryPath)
        With emailTask
            Dim emailTrigger As New DailyTrigger(1)
            emailTrigger.SetRepetition(New TimeSpan(0, 5, 0), New TimeSpan(24, 0, 0), False)
            emailTrigger.StartBoundary = New DateTime(Now.Year, Now.Month, Now.Day, 0, 0, 0)

            .Triggers.Add(emailTrigger)
            .Actions.Add(action)
        End With
        Dim taskName As String = GetTaskName(ts, action, "TM2 Email")
        Try
            ' If (ts.HighestSupportedVersion >= New Version(1, 2)) Then
            ts.RootFolder.RegisterTaskDefinition(taskName, emailTask)
            'Else
            '    ts.RootFolder.RegisterTaskDefinition(taskName, emailTask , TaskCreation.CreateOrUpdate, "NT AUTHORITY\System", Nothing, TaskLogonType.ServiceAccount, Nothing)
            'End If
            MessageBox.Show("Email Scheduled Task added")
        Catch ex As Exception
            MessageBox.Show(KaCommonObjects.Alerts.FormatException(ex))
        End Try
    End Sub

    Private Sub btnCreateInterfaceScheduledTask_Click(sender As System.Object, e As System.EventArgs) Handles btnCreateInterfaceScheduledTask.Click
        If cmbInterface.Items.Count > 0 And cmbInterface.SelectedIndex >= 0 Then
            Dim selectedItem As KaCommonObjects.ComboBoxItem = cmbInterface.SelectedItem
            Dim ts As New TaskService()
            Dim interfaceTask As TaskDefinition = CreateBasicTask(ts, LOCAL_SYSTEM_SID)
            With interfaceTask
                Dim interfaceTrigger As New DailyTrigger(1)
                interfaceTrigger.SetRepetition(New TimeSpan(0, 2, 0), New TimeSpan(24, 0, 0), False)
                interfaceTrigger.StartBoundary = New DateTime(Now.Year, Now.Month, Now.Day, 0, 0, 0)

                .Triggers.Add(interfaceTrigger)
				.Actions.Add(New ExecAction(My.Application.Info.DirectoryPath & "..\Tm2Interface\Tm2InterfaceRunner.exe", selectedItem.Value.ToString(), My.Application.Info.DirectoryPath & "..\Tm2Interface"))
			End With
            Try
                'If (ts.HighestSupportedVersion >= New Version(1, 2)) Then
                ts.RootFolder.RegisterTaskDefinition("TM2 " & selectedItem.Text, interfaceTask)
                'Else
                '    ts.RootFolder.RegisterTaskDefinition("TM2 " & selectedItem.Text, interfaceTask, TaskCreation.CreateOrUpdate, "NT AUTHORITY\System", Nothing, TaskLogonType.ServiceAccount, Nothing)
                'End If
                MessageBox.Show("Interface Scheduled Task added." & vbCrLf &
                                "You will need to set up the correct user to run as, and finish setting up the executable to run and the directory to run in from within the Scheduled Task Management console.")
                Try
                    Dim taskSchedulerRunning As Boolean = False
                    For Each getProcess As Process In Process.GetProcesses()

                        If (getProcess.ProcessName.Contains("mmc")) AndAlso getProcess.MainWindowTitle = "Task Scheduler" Then
                            taskSchedulerRunning = True
                            Exit For
                        End If
                    Next

                    If Not taskSchedulerRunning Then Process.Start("Taskschd.msc")
                Catch ex As Exception

                End Try
            Catch ex As Exception
                MessageBox.Show(KaCommonObjects.Alerts.FormatException(ex))
            End Try
        End If
    End Sub

    Private Const LOCAL_AUTHORITY_SID As String = "S-1-2" ' Description: An identifier authority.
    Private Const LOCAL_SID As String = "S-1-2-0" ' Description: A group that includes all users who have logged on locally.
    Private Const EVERYONE_SID As String = "S-1-1-0" ' Description: A group that includes all users, even anonymous users and guests. Membership is controlled by the operating system. Note By default, the Everyone group no longer includes anonymous users on a computer that is running Windows XP Service Pack 2 (SP2).
    Private Const SERVICE_SID As String = "S-1-5-6" ' Description: A group that includes all security principals that have logged on as a service. Membership is controlled by the operating system.
    Private Const LOCAL_SYSTEM_SID As String = "S-1-5-18" ' Description: A service account that is used by the operating system.
    Private Const NT_AUTHORITY_SID As String = "S-1-5-19" ' Description: Local Service
    Private Const NETWORK_SERVICE_SID As String = "S-1-5-20" ' Description: Network Service  

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="ts"></param>
    ''' <param name="securityIdentifierId"></param>
    ''' <returns></returns>
    ''' <remarks>Common Security Identifiers list can be found at http://support.microsoft.com/en-us/kb/243330
    ''' </remarks>
    Private Function CreateBasicTask(ts As TaskService, ByVal securityIdentifierId As String) As TaskDefinition
        Dim newTaskDef As TaskDefinition = ts.NewTask()
        With newTaskDef
            If (ts.HighestSupportedVersion >= New Version(1, 2)) Then
                With .Principal
                    .Id = "Author"
                    .UserId = securityIdentifierId
                    .RunLevel = TaskRunLevel.Highest
                End With
                With .Settings
                    .AllowDemandStart = True
                    .AllowHardTerminate = True
                    .DisallowStartIfOnBatteries = False
                    .DisallowStartOnRemoteAppSession = False
                    .Enabled = True
                    .ExecutionTimeLimit = New TimeSpan(1, 0, 0)
                    .Hidden = True
                    .MultipleInstances = TaskInstancesPolicy.IgnoreNew
                    .RunOnlyIfIdle = False
                    .StartWhenAvailable = True
                    .StopIfGoingOnBatteries = False
                    .WakeToRun = True
                End With
            Else
                With .Settings
                    .ExecutionTimeLimit = New TimeSpan(1, 0, 0)
                End With
            End If
        End With
        Return newTaskDef
    End Function

    Private Function GetTaskName(ByVal ts As TaskService, ByVal action As ExecAction, ByVal taskName As String) As String
        For Each scheduledTask As Task In ts.RootFolder.Tasks
            If scheduledTask.Definition.Actions.Count > 0 Then
                For Each taskAction As Object In scheduledTask.Definition.Actions
                    If TypeOf taskAction Is ExecAction Then
                        With CType(taskAction, ExecAction)
                            If .ActionType = TaskActionType.Execute AndAlso .Path.ToLower = action.Path.ToLower AndAlso .Arguments.ToLower().Replace(" ", "") = action.Arguments.ToLower().Replace(" ", "") Then
                                Return scheduledTask.Name
                            End If
                        End With '
                    End If
                Next
            End If
        Next

        Return taskName
    End Function

    Private Sub btnBackupFolder_Click(sender As System.Object, e As System.EventArgs) Handles btnBackupFolder.Click
        With BackupFolderBrowserDialog
            .Description = "Backup directory"
            .RootFolder = Environment.SpecialFolder.MyComputer
            .SelectedPath = tbxBackupFolder.Text
            .ShowNewFolderButton = True
            If .ShowDialog(Me) = Windows.Forms.DialogResult.OK Then
                tbxBackupFolder.Text = .SelectedPath
            End If
        End With
    End Sub

    Private Sub btnCreateTimeSyncFiles_Click(sender As System.Object, e As System.EventArgs) Handles btnCreateTimeSyncFiles.Click
        Try
            tbxTimeSyncFolder.Text = tbxTimeSyncFolder.Text.Trim

            Dim ts As New TaskService()
            Dim timeSyncTask As TaskDefinition = CreateBasicTask(ts, LOCAL_SYSTEM_SID)
            With timeSyncTask
                If (ts.HighestSupportedVersion >= New Version(1, 2)) Then
                    With .Principal
                        .Id = "Author"
                        .UserId = System.Security.Principal.WindowsIdentity.GetCurrent().Name
                        .LogonType = TaskLogonType.InteractiveToken
                    End With
                End If
                Dim timeSyncLogonTrigger As New LogonTrigger()
                timeSyncLogonTrigger.Delay = New TimeSpan(0, 1, 0)
                timeSyncLogonTrigger.Enabled = True
                timeSyncLogonTrigger.StartBoundary = New DateTime(Now.Year, Now.Month, Now.Day, 0, 0, 0)
                .Triggers.Add(timeSyncLogonTrigger)

                Dim timeSyncIntervalTrigger As New DailyTrigger(1)
                timeSyncIntervalTrigger.StartBoundary = New DateTime(Now.Year, Now.Month, Now.Day, 0, 0, 0)
                .Triggers.Add(timeSyncIntervalTrigger)

                .Actions.Add(New ExecAction(IO.Path.Combine(tbxTimeSyncFolder.Text, "SyncTimewithServer.bat")))
            End With

            If Not IO.Directory.Exists(tbxTimeSyncFolder.Text) Then
                IO.Directory.CreateDirectory(tbxTimeSyncFolder.Text)
            End If
            Dim taskFileName As String = IO.Path.Combine(tbxTimeSyncFolder.Text, "Sync Time with Server.xml")
            If IO.File.Exists(taskFileName) Then
                IO.File.Delete(taskFileName)
            End If
            IO.File.WriteAllText(taskFileName, timeSyncTask.XmlText)

            Dim ipAddress As String = "127.0.0.1"
            Try
                ipAddress = GetIpV4()
            Catch ex As Exception
                MessageBox.Show(ex.Message)
            End Try
            Dim batchFileName As String = IO.Path.Combine(tbxTimeSyncFolder.Text, "SyncTimewithServer.bat")
            If IO.File.Exists(batchFileName) Then
                IO.File.Delete(batchFileName)
            End If
            IO.File.WriteAllText(batchFileName, "net time \\" & ipAddress & " /set /y")

            MessageBox.Show("Files created." & vbCrLf &
                            "You will need to assign the correct user to the scheduled task when the Scheduled Task is imported on the target computer.")
        Catch ex As Exception
            MessageBox.Show(KaCommonObjects.Alerts.FormatException(ex))
        End Try
    End Sub

    Private Sub btnTimeSyncFolder_Click(sender As System.Object, e As System.EventArgs) Handles btnTimeSyncFolder.Click
        With BackupFolderBrowserDialog
            .Description = "Time sync files directory"
            .RootFolder = Environment.SpecialFolder.MyComputer
            .SelectedPath = tbxTimeSyncFolder.Text
            .ShowNewFolderButton = True
            If .ShowDialog(Me) = Windows.Forms.DialogResult.OK Then
                tbxTimeSyncFolder.Text = .SelectedPath
            End If
        End With
    End Sub

    Public Function GetIpV4() As String
        Dim myHost As String = System.Net.Dns.GetHostName
        Dim ipEntry As System.Net.IPHostEntry = System.Net.Dns.GetHostEntry(myHost)
        Dim ip As String = ""

        For Each tmpIpAddress As System.Net.IPAddress In ipEntry.AddressList
            If tmpIpAddress.AddressFamily = System.Net.Sockets.AddressFamily.InterNetwork Then
                Dim ipAddress As String = tmpIpAddress.ToString
                ip = ipAddress
                Exit For
            End If
        Next

        If ip = "" Then
            Throw New Exception("No 10. IP found!")
        End If

        Return ip
    End Function

    Private Sub btnCreateExpiredOrdersScheduledTask_Click(sender As System.Object, e As System.EventArgs) Handles btnCreateExpiredOrdersScheduledTask.Click
        Dim ts As New TaskService()
        Dim removeExpiredOrdersTask As TaskDefinition = CreateBasicTask(ts, LOCAL_SYSTEM_SID)
        Dim action As New ExecAction(My.Application.Info.DirectoryPath & "\Tm2BackgroundServices.exe", "-expired_orders", My.Application.Info.DirectoryPath)
        With removeExpiredOrdersTask
            Dim removeExpiredOrdersTrigger As New DailyTrigger(1)
            removeExpiredOrdersTrigger.SetRepetition(New TimeSpan(1, 0, 0), New TimeSpan(24, 0, 0), False)
            removeExpiredOrdersTrigger.StartBoundary = New DateTime(Now.Year, Now.Month, Now.Day, 0, 0, 0)

            .Triggers.Add(removeExpiredOrdersTrigger)
            .Actions.Add(action)
        End With
        Dim taskName As String = GetTaskName(ts, action, "TM2 Remove Expired Orders")
        Try
            ' If (ts.HighestSupportedVersion >= New Version(1, 2)) Then
            ts.RootFolder.RegisterTaskDefinition(taskName, removeExpiredOrdersTask)
            'Else
            '    ts.RootFolder.RegisterTaskDefinition(taskName, emailTask , TaskCreation.CreateOrUpdate, "NT AUTHORITY\System", Nothing, TaskLogonType.ServiceAccount, Nothing)
            'End If
            MessageBox.Show("Remove Expired Orders Scheduled Task added")
        Catch ex As Exception
            MessageBox.Show(KaCommonObjects.Alerts.FormatException(ex))
        End Try
    End Sub

    Private Sub btnCreateCleanEventLogScheduledTask_Click(sender As System.Object, e As System.EventArgs) Handles btnCreateCleanEventLogScheduledTask.Click
        Dim ts As New TaskService()
        Dim cleanEventLogTask As TaskDefinition = CreateBasicTask(ts, LOCAL_SYSTEM_SID)
        Dim action As New ExecAction(My.Application.Info.DirectoryPath & "\Tm2BackgroundServices.exe", "-event_log", My.Application.Info.DirectoryPath)
        With cleanEventLogTask
            Dim cleanEventLogTrigger As New DailyTrigger(1)
            'cleanEventLogTrigger.SetRepetition(New TimeSpan(1, 0, 0), New TimeSpan(24, 0, 0), False)
            cleanEventLogTrigger.StartBoundary = New DateTime(Now.Year, Now.Month, Now.Day, 0, 0, 0)

            .Triggers.Add(cleanEventLogTrigger)
            .Actions.Add(action)
        End With
        Dim taskName As String = GetTaskName(ts, action, "TM2 Clean Event Log")
        Try
            ' If (ts.HighestSupportedVersion >= New Version(1, 2)) Then
            ts.RootFolder.RegisterTaskDefinition(taskName, cleanEventLogTask)
            'Else
            '    ts.RootFolder.RegisterTaskDefinition(taskName, emailTask , TaskCreation.CreateOrUpdate, "NT AUTHORITY\System", Nothing, TaskLogonType.ServiceAccount, Nothing)
            'End If
            MessageBox.Show("Clean Event Log Scheduled Task added")
        Catch ex As Exception
            MessageBox.Show(KaCommonObjects.Alerts.FormatException(ex))
        End Try
    End Sub
End Class
