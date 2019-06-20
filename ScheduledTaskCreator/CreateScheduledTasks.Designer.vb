<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class CreateScheduledTasks
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(CreateScheduledTasks))
        Me.btnCreateBackupScheduledTask = New System.Windows.Forms.Button()
        Me.cmbInterface = New System.Windows.Forms.ComboBox()
        Me.btnCreateEmailScheduledTask = New System.Windows.Forms.Button()
        Me.btnCreateInterfaceScheduledTask = New System.Windows.Forms.Button()
        Me.gpbInterface = New System.Windows.Forms.GroupBox()
        Me.gpbBackups = New System.Windows.Forms.GroupBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.nudNumberOfBackupCopies = New System.Windows.Forms.NumericUpDown()
        Me.lblNumberOfBackupCopies = New System.Windows.Forms.Label()
        Me.btnBackupFolder = New System.Windows.Forms.Button()
        Me.tbxBackupFolder = New System.Windows.Forms.TextBox()
        Me.lblBackupFolder = New System.Windows.Forms.Label()
        Me.BackupFolderBrowserDialog = New System.Windows.Forms.FolderBrowserDialog()
        Me.gpbEmails = New System.Windows.Forms.GroupBox()
        Me.gpbCreateTimeSyncFiles = New System.Windows.Forms.GroupBox()
        Me.btnTimeSyncFolder = New System.Windows.Forms.Button()
        Me.tbxTimeSyncFolder = New System.Windows.Forms.TextBox()
        Me.lblTimeSyncFolder = New System.Windows.Forms.Label()
        Me.btnCreateTimeSyncFiles = New System.Windows.Forms.Button()
        Me.gpbExpiredOrders = New System.Windows.Forms.GroupBox()
        Me.btnCreateExpiredOrdersScheduledTask = New System.Windows.Forms.Button()
        Me.gpbCleanEventLog = New System.Windows.Forms.GroupBox()
        Me.btnCreateCleanEventLogScheduledTask = New System.Windows.Forms.Button()
        Me.gpbInterface.SuspendLayout()
        Me.gpbBackups.SuspendLayout()
        CType(Me.nudNumberOfBackupCopies, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.gpbEmails.SuspendLayout()
        Me.gpbCreateTimeSyncFiles.SuspendLayout()
        Me.gpbExpiredOrders.SuspendLayout()
        Me.gpbCleanEventLog.SuspendLayout()
        Me.SuspendLayout()
        '
        'btnCreateBackupScheduledTask
        '
        Me.btnCreateBackupScheduledTask.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.btnCreateBackupScheduledTask.Location = New System.Drawing.Point(362, 25)
        Me.btnCreateBackupScheduledTask.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.btnCreateBackupScheduledTask.Name = "btnCreateBackupScheduledTask"
        Me.btnCreateBackupScheduledTask.Size = New System.Drawing.Size(105, 50)
        Me.btnCreateBackupScheduledTask.TabIndex = 6
        Me.btnCreateBackupScheduledTask.Text = "Create Task"
        Me.btnCreateBackupScheduledTask.UseVisualStyleBackColor = True
        '
        'cmbInterface
        '
        Me.cmbInterface.FormattingEnabled = True
        Me.cmbInterface.Location = New System.Drawing.Point(4, 25)
        Me.cmbInterface.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.cmbInterface.Name = "cmbInterface"
        Me.cmbInterface.Size = New System.Drawing.Size(319, 28)
        Me.cmbInterface.TabIndex = 0
        '
        'btnCreateEmailScheduledTask
        '
        Me.btnCreateEmailScheduledTask.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.btnCreateEmailScheduledTask.Location = New System.Drawing.Point(15, 22)
        Me.btnCreateEmailScheduledTask.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.btnCreateEmailScheduledTask.Name = "btnCreateEmailScheduledTask"
        Me.btnCreateEmailScheduledTask.Size = New System.Drawing.Size(105, 50)
        Me.btnCreateEmailScheduledTask.TabIndex = 0
        Me.btnCreateEmailScheduledTask.Text = "Create Task"
        Me.btnCreateEmailScheduledTask.UseVisualStyleBackColor = True
        '
        'btnCreateInterfaceScheduledTask
        '
        Me.btnCreateInterfaceScheduledTask.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.btnCreateInterfaceScheduledTask.Location = New System.Drawing.Point(362, 14)
        Me.btnCreateInterfaceScheduledTask.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.btnCreateInterfaceScheduledTask.Name = "btnCreateInterfaceScheduledTask"
        Me.btnCreateInterfaceScheduledTask.Size = New System.Drawing.Size(105, 50)
        Me.btnCreateInterfaceScheduledTask.TabIndex = 1
        Me.btnCreateInterfaceScheduledTask.Text = "Create Task"
        Me.btnCreateInterfaceScheduledTask.UseVisualStyleBackColor = True
        '
        'gpbInterface
        '
        Me.gpbInterface.Controls.Add(Me.btnCreateInterfaceScheduledTask)
        Me.gpbInterface.Controls.Add(Me.cmbInterface)
        Me.gpbInterface.Location = New System.Drawing.Point(14, 236)
        Me.gpbInterface.Name = "gpbInterface"
        Me.gpbInterface.Size = New System.Drawing.Size(471, 79)
        Me.gpbInterface.TabIndex = 4
        Me.gpbInterface.TabStop = False
        Me.gpbInterface.Text = "Interface"
        '
        'gpbBackups
        '
        Me.gpbBackups.Controls.Add(Me.Label1)
        Me.gpbBackups.Controls.Add(Me.nudNumberOfBackupCopies)
        Me.gpbBackups.Controls.Add(Me.lblNumberOfBackupCopies)
        Me.gpbBackups.Controls.Add(Me.btnBackupFolder)
        Me.gpbBackups.Controls.Add(Me.tbxBackupFolder)
        Me.gpbBackups.Controls.Add(Me.lblBackupFolder)
        Me.gpbBackups.Controls.Add(Me.btnCreateBackupScheduledTask)
        Me.gpbBackups.Location = New System.Drawing.Point(13, 7)
        Me.gpbBackups.Name = "gpbBackups"
        Me.gpbBackups.Size = New System.Drawing.Size(471, 135)
        Me.gpbBackups.TabIndex = 0
        Me.gpbBackups.TabStop = False
        Me.gpbBackups.Text = "Backups"
        '
        'Label1
        '
        Me.Label1.ForeColor = System.Drawing.Color.Red
        Me.Label1.Location = New System.Drawing.Point(5, 85)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(460, 47)
        Me.Label1.TabIndex = 5
        Me.Label1.Text = "Leave folder blank and number of copies at 0 to use settings assigned in Terminal" & _
    " Management 2"
        Me.Label1.TextAlign = System.Drawing.ContentAlignment.TopCenter
        '
        'nudNumberOfBackupCopies
        '
        Me.nudNumberOfBackupCopies.Location = New System.Drawing.Point(248, 57)
        Me.nudNumberOfBackupCopies.Name = "nudNumberOfBackupCopies"
        Me.nudNumberOfBackupCopies.Size = New System.Drawing.Size(75, 26)
        Me.nudNumberOfBackupCopies.TabIndex = 4
        Me.nudNumberOfBackupCopies.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'lblNumberOfBackupCopies
        '
        Me.lblNumberOfBackupCopies.AutoSize = True
        Me.lblNumberOfBackupCopies.Location = New System.Drawing.Point(1, 59)
        Me.lblNumberOfBackupCopies.Name = "lblNumberOfBackupCopies"
        Me.lblNumberOfBackupCopies.Size = New System.Drawing.Size(133, 20)
        Me.lblNumberOfBackupCopies.TabIndex = 3
        Me.lblNumberOfBackupCopies.Text = "Number of copies"
        '
        'btnBackupFolder
        '
        Me.btnBackupFolder.BackgroundImage = Global.ScheduledTaskCreator.My.Resources.Resources.SearchFolderHS
        Me.btnBackupFolder.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center
        Me.btnBackupFolder.Location = New System.Drawing.Point(329, 25)
        Me.btnBackupFolder.Name = "btnBackupFolder"
        Me.btnBackupFolder.Size = New System.Drawing.Size(26, 26)
        Me.btnBackupFolder.TabIndex = 2
        Me.btnBackupFolder.UseVisualStyleBackColor = True
        '
        'tbxBackupFolder
        '
        Me.tbxBackupFolder.Location = New System.Drawing.Point(61, 25)
        Me.tbxBackupFolder.Name = "tbxBackupFolder"
        Me.tbxBackupFolder.Size = New System.Drawing.Size(262, 26)
        Me.tbxBackupFolder.TabIndex = 1
        '
        'lblBackupFolder
        '
        Me.lblBackupFolder.AutoSize = True
        Me.lblBackupFolder.Location = New System.Drawing.Point(1, 28)
        Me.lblBackupFolder.Name = "lblBackupFolder"
        Me.lblBackupFolder.Size = New System.Drawing.Size(54, 20)
        Me.lblBackupFolder.TabIndex = 0
        Me.lblBackupFolder.Text = "Folder"
        '
        'BackupFolderBrowserDialog
        '
        Me.BackupFolderBrowserDialog.RootFolder = System.Environment.SpecialFolder.MyComputer
        '
        'gpbEmails
        '
        Me.gpbEmails.Controls.Add(Me.btnCreateEmailScheduledTask)
        Me.gpbEmails.Location = New System.Drawing.Point(13, 148)
        Me.gpbEmails.Name = "gpbEmails"
        Me.gpbEmails.Size = New System.Drawing.Size(134, 80)
        Me.gpbEmails.TabIndex = 1
        Me.gpbEmails.TabStop = False
        Me.gpbEmails.Text = "Email service"
        '
        'gpbCreateTimeSyncFiles
        '
        Me.gpbCreateTimeSyncFiles.Controls.Add(Me.btnTimeSyncFolder)
        Me.gpbCreateTimeSyncFiles.Controls.Add(Me.tbxTimeSyncFolder)
        Me.gpbCreateTimeSyncFiles.Controls.Add(Me.lblTimeSyncFolder)
        Me.gpbCreateTimeSyncFiles.Controls.Add(Me.btnCreateTimeSyncFiles)
        Me.gpbCreateTimeSyncFiles.Location = New System.Drawing.Point(14, 321)
        Me.gpbCreateTimeSyncFiles.Name = "gpbCreateTimeSyncFiles"
        Me.gpbCreateTimeSyncFiles.Size = New System.Drawing.Size(471, 80)
        Me.gpbCreateTimeSyncFiles.TabIndex = 5
        Me.gpbCreateTimeSyncFiles.TabStop = False
        Me.gpbCreateTimeSyncFiles.Text = "Time sync"
        '
        'btnTimeSyncFolder
        '
        Me.btnTimeSyncFolder.BackgroundImage = Global.ScheduledTaskCreator.My.Resources.Resources.SearchFolderHS
        Me.btnTimeSyncFolder.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center
        Me.btnTimeSyncFolder.Location = New System.Drawing.Point(329, 27)
        Me.btnTimeSyncFolder.Name = "btnTimeSyncFolder"
        Me.btnTimeSyncFolder.Size = New System.Drawing.Size(26, 26)
        Me.btnTimeSyncFolder.TabIndex = 2
        Me.btnTimeSyncFolder.UseVisualStyleBackColor = True
        '
        'tbxTimeSyncFolder
        '
        Me.tbxTimeSyncFolder.Location = New System.Drawing.Point(61, 27)
        Me.tbxTimeSyncFolder.Name = "tbxTimeSyncFolder"
        Me.tbxTimeSyncFolder.Size = New System.Drawing.Size(262, 26)
        Me.tbxTimeSyncFolder.TabIndex = 1
        Me.tbxTimeSyncFolder.Text = "C:\Kaco\TimeSyncFiles"
        '
        'lblTimeSyncFolder
        '
        Me.lblTimeSyncFolder.AutoSize = True
        Me.lblTimeSyncFolder.Location = New System.Drawing.Point(1, 30)
        Me.lblTimeSyncFolder.Name = "lblTimeSyncFolder"
        Me.lblTimeSyncFolder.Size = New System.Drawing.Size(54, 20)
        Me.lblTimeSyncFolder.TabIndex = 0
        Me.lblTimeSyncFolder.Text = "Folder"
        '
        'btnCreateTimeSyncFiles
        '
        Me.btnCreateTimeSyncFiles.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.btnCreateTimeSyncFiles.Location = New System.Drawing.Point(362, 15)
        Me.btnCreateTimeSyncFiles.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.btnCreateTimeSyncFiles.Name = "btnCreateTimeSyncFiles"
        Me.btnCreateTimeSyncFiles.Size = New System.Drawing.Size(105, 50)
        Me.btnCreateTimeSyncFiles.TabIndex = 3
        Me.btnCreateTimeSyncFiles.Text = "Create Files"
        Me.btnCreateTimeSyncFiles.UseVisualStyleBackColor = True
        '
        'gpbExpiredOrders
        '
        Me.gpbExpiredOrders.Controls.Add(Me.btnCreateExpiredOrdersScheduledTask)
        Me.gpbExpiredOrders.Location = New System.Drawing.Point(181, 148)
        Me.gpbExpiredOrders.Name = "gpbExpiredOrders"
        Me.gpbExpiredOrders.Size = New System.Drawing.Size(134, 80)
        Me.gpbExpiredOrders.TabIndex = 2
        Me.gpbExpiredOrders.TabStop = False
        Me.gpbExpiredOrders.Text = "Expired orders"
        '
        'btnCreateExpiredOrdersScheduledTask
        '
        Me.btnCreateExpiredOrdersScheduledTask.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.btnCreateExpiredOrdersScheduledTask.Location = New System.Drawing.Point(15, 22)
        Me.btnCreateExpiredOrdersScheduledTask.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.btnCreateExpiredOrdersScheduledTask.Name = "btnCreateExpiredOrdersScheduledTask"
        Me.btnCreateExpiredOrdersScheduledTask.Size = New System.Drawing.Size(105, 50)
        Me.btnCreateExpiredOrdersScheduledTask.TabIndex = 0
        Me.btnCreateExpiredOrdersScheduledTask.Text = "Create Task"
        Me.btnCreateExpiredOrdersScheduledTask.UseVisualStyleBackColor = True
        '
        'gpbCleanEventLog
        '
        Me.gpbCleanEventLog.Controls.Add(Me.btnCreateCleanEventLogScheduledTask)
        Me.gpbCleanEventLog.Location = New System.Drawing.Point(350, 148)
        Me.gpbCleanEventLog.Name = "gpbCleanEventLog"
        Me.gpbCleanEventLog.Size = New System.Drawing.Size(134, 80)
        Me.gpbCleanEventLog.TabIndex = 3
        Me.gpbCleanEventLog.TabStop = False
        Me.gpbCleanEventLog.Text = "Clean event log"
        '
        'btnCreateCleanEventLogScheduledTask
        '
        Me.btnCreateCleanEventLogScheduledTask.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.btnCreateCleanEventLogScheduledTask.Location = New System.Drawing.Point(15, 22)
        Me.btnCreateCleanEventLogScheduledTask.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.btnCreateCleanEventLogScheduledTask.Name = "btnCreateCleanEventLogScheduledTask"
        Me.btnCreateCleanEventLogScheduledTask.Size = New System.Drawing.Size(105, 50)
        Me.btnCreateCleanEventLogScheduledTask.TabIndex = 0
        Me.btnCreateCleanEventLogScheduledTask.Text = "Create Task"
        Me.btnCreateCleanEventLogScheduledTask.UseVisualStyleBackColor = True
        '
        'CreateScheduledTasks
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.AutoSize = True
        Me.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.ClientSize = New System.Drawing.Size(497, 411)
        Me.Controls.Add(Me.gpbCleanEventLog)
        Me.Controls.Add(Me.gpbExpiredOrders)
        Me.Controls.Add(Me.gpbCreateTimeSyncFiles)
        Me.Controls.Add(Me.gpbEmails)
        Me.Controls.Add(Me.gpbBackups)
        Me.Controls.Add(Me.gpbInterface)
        Me.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "CreateScheduledTasks"
        Me.Text = "Create Scheduled Tasks"
        Me.gpbInterface.ResumeLayout(False)
        Me.gpbBackups.ResumeLayout(False)
        Me.gpbBackups.PerformLayout()
        CType(Me.nudNumberOfBackupCopies, System.ComponentModel.ISupportInitialize).EndInit()
        Me.gpbEmails.ResumeLayout(False)
        Me.gpbCreateTimeSyncFiles.ResumeLayout(False)
        Me.gpbCreateTimeSyncFiles.PerformLayout()
        Me.gpbExpiredOrders.ResumeLayout(False)
        Me.gpbCleanEventLog.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents btnCreateBackupScheduledTask As System.Windows.Forms.Button
    Friend WithEvents cmbInterface As System.Windows.Forms.ComboBox
    Friend WithEvents btnCreateEmailScheduledTask As System.Windows.Forms.Button
    Friend WithEvents btnCreateInterfaceScheduledTask As System.Windows.Forms.Button
    Friend WithEvents gpbInterface As System.Windows.Forms.GroupBox
    Friend WithEvents gpbBackups As System.Windows.Forms.GroupBox
    Friend WithEvents BackupFolderBrowserDialog As System.Windows.Forms.FolderBrowserDialog
    Friend WithEvents nudNumberOfBackupCopies As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblNumberOfBackupCopies As System.Windows.Forms.Label
    Friend WithEvents btnBackupFolder As System.Windows.Forms.Button
    Friend WithEvents tbxBackupFolder As System.Windows.Forms.TextBox
    Friend WithEvents lblBackupFolder As System.Windows.Forms.Label
    Friend WithEvents gpbEmails As System.Windows.Forms.GroupBox
    Friend WithEvents gpbCreateTimeSyncFiles As System.Windows.Forms.GroupBox
    Friend WithEvents btnTimeSyncFolder As System.Windows.Forms.Button
    Friend WithEvents tbxTimeSyncFolder As System.Windows.Forms.TextBox
    Friend WithEvents lblTimeSyncFolder As System.Windows.Forms.Label
    Friend WithEvents btnCreateTimeSyncFiles As System.Windows.Forms.Button
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents gpbExpiredOrders As System.Windows.Forms.GroupBox
    Friend WithEvents btnCreateExpiredOrdersScheduledTask As System.Windows.Forms.Button
    Friend WithEvents gpbCleanEventLog As System.Windows.Forms.GroupBox
    Friend WithEvents btnCreateCleanEventLogScheduledTask As System.Windows.Forms.Button

End Class
