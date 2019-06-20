<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ConfigurationForm
	Inherits System.Windows.Forms.Form

	'Form overrides dispose to clean up the component list.
	<System.Diagnostics.DebuggerNonUserCode()>
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
	<System.Diagnostics.DebuggerStepThrough()>
	Private Sub InitializeComponent()
		Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ConfigurationForm))
		Me.lblConnectionString = New System.Windows.Forms.Label()
		Me.tbxConnectionString = New System.Windows.Forms.TextBox()
		Me.lblConnectionRetries = New System.Windows.Forms.Label()
		Me.nudConnectionRetries = New System.Windows.Forms.NumericUpDown()
		Me.nudCommandTimeout = New System.Windows.Forms.NumericUpDown()
		Me.lblCommandTimeout = New System.Windows.Forms.Label()
		Me.nudQueryRetries = New System.Windows.Forms.NumericUpDown()
		Me.lblQueryRetries = New System.Windows.Forms.Label()
		Me.nudRetryWait = New System.Windows.Forms.NumericUpDown()
		Me.lblRetryWait = New System.Windows.Forms.Label()
		Me.cbxUseDefaultConnectionRetries = New System.Windows.Forms.CheckBox()
		Me.cbxUseDefaultCommandTimeout = New System.Windows.Forms.CheckBox()
		Me.cbxUseDefaultQueryRetries = New System.Windows.Forms.CheckBox()
		Me.cbxUseDefaultRetryWait = New System.Windows.Forms.CheckBox()
		Me.btnSave = New System.Windows.Forms.Button()
		Me.btnClose = New System.Windows.Forms.Button()
		Me.btnImportSettings = New System.Windows.Forms.Button()
		Me.btnExportSettings = New System.Windows.Forms.Button()
		Me.openFileDialog1 = New System.Windows.Forms.OpenFileDialog()
		Me.saveFileDialog1 = New System.Windows.Forms.SaveFileDialog()
		Me.btnCreateConfigXml = New System.Windows.Forms.Button()
		Me.btnSetDefaultConnectionString = New System.Windows.Forms.Button()
		CType(Me.nudConnectionRetries, System.ComponentModel.ISupportInitialize).BeginInit()
		CType(Me.nudCommandTimeout, System.ComponentModel.ISupportInitialize).BeginInit()
		CType(Me.nudQueryRetries, System.ComponentModel.ISupportInitialize).BeginInit()
		CType(Me.nudRetryWait, System.ComponentModel.ISupportInitialize).BeginInit()
		Me.SuspendLayout()
		'
		'lblConnectionString
		'
		Me.lblConnectionString.AutoSize = True
		Me.lblConnectionString.Location = New System.Drawing.Point(7, 15)
		Me.lblConnectionString.Margin = New System.Windows.Forms.Padding(6, 0, 6, 0)
		Me.lblConnectionString.Name = "lblConnectionString"
		Me.lblConnectionString.Size = New System.Drawing.Size(160, 24)
		Me.lblConnectionString.TabIndex = 1
		Me.lblConnectionString.Text = "Connection String"
		'
		'tbxConnectionString
		'
		Me.tbxConnectionString.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
			Or System.Windows.Forms.AnchorStyles.Left) _
			Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
		Me.tbxConnectionString.Location = New System.Drawing.Point(179, 12)
		Me.tbxConnectionString.Multiline = True
		Me.tbxConnectionString.Name = "tbxConnectionString"
		Me.tbxConnectionString.Size = New System.Drawing.Size(513, 86)
		Me.tbxConnectionString.TabIndex = 2
		Me.tbxConnectionString.Text = "Provider=SQLOLEDB;Data Source=localhost;Initial Catalog=TM2;User Id=sa; Password=" &
	"Kahler6648; Persist Security Info=True;"
		'
		'lblConnectionRetries
		'
		Me.lblConnectionRetries.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
		Me.lblConnectionRetries.AutoSize = True
		Me.lblConnectionRetries.Location = New System.Drawing.Point(7, 106)
		Me.lblConnectionRetries.Margin = New System.Windows.Forms.Padding(6, 0, 6, 0)
		Me.lblConnectionRetries.Name = "lblConnectionRetries"
		Me.lblConnectionRetries.Size = New System.Drawing.Size(163, 24)
		Me.lblConnectionRetries.TabIndex = 3
		Me.lblConnectionRetries.Text = "Connection retries"
		'
		'nudConnectionRetries
		'
		Me.nudConnectionRetries.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
		Me.nudConnectionRetries.Enabled = False
		Me.nudConnectionRetries.Location = New System.Drawing.Point(179, 104)
		Me.nudConnectionRetries.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
		Me.nudConnectionRetries.Name = "nudConnectionRetries"
		Me.nudConnectionRetries.Size = New System.Drawing.Size(91, 29)
		Me.nudConnectionRetries.TabIndex = 4
		Me.nudConnectionRetries.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
		Me.nudConnectionRetries.Value = New Decimal(New Integer() {3, 0, 0, 0})
		'
		'nudCommandTimeout
		'
		Me.nudCommandTimeout.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
		Me.nudCommandTimeout.Enabled = False
		Me.nudCommandTimeout.Increment = New Decimal(New Integer() {30, 0, 0, 0})
		Me.nudCommandTimeout.Location = New System.Drawing.Point(179, 209)
		Me.nudCommandTimeout.Maximum = New Decimal(New Integer() {65535, 0, 0, 0})
		Me.nudCommandTimeout.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
		Me.nudCommandTimeout.Name = "nudCommandTimeout"
		Me.nudCommandTimeout.Size = New System.Drawing.Size(91, 29)
		Me.nudCommandTimeout.TabIndex = 13
		Me.nudCommandTimeout.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
		Me.nudCommandTimeout.Value = New Decimal(New Integer() {30, 0, 0, 0})
		'
		'lblCommandTimeout
		'
		Me.lblCommandTimeout.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
		Me.lblCommandTimeout.AutoSize = True
		Me.lblCommandTimeout.Location = New System.Drawing.Point(7, 211)
		Me.lblCommandTimeout.Margin = New System.Windows.Forms.Padding(6, 0, 6, 0)
		Me.lblCommandTimeout.Name = "lblCommandTimeout"
		Me.lblCommandTimeout.Size = New System.Drawing.Size(164, 24)
		Me.lblCommandTimeout.TabIndex = 12
		Me.lblCommandTimeout.Text = "Command timeout"
		'
		'nudQueryRetries
		'
		Me.nudQueryRetries.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
		Me.nudQueryRetries.Enabled = False
		Me.nudQueryRetries.Location = New System.Drawing.Point(179, 139)
		Me.nudQueryRetries.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
		Me.nudQueryRetries.Name = "nudQueryRetries"
		Me.nudQueryRetries.Size = New System.Drawing.Size(91, 29)
		Me.nudQueryRetries.TabIndex = 7
		Me.nudQueryRetries.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
		Me.nudQueryRetries.Value = New Decimal(New Integer() {3, 0, 0, 0})
		'
		'lblQueryRetries
		'
		Me.lblQueryRetries.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
		Me.lblQueryRetries.AutoSize = True
		Me.lblQueryRetries.Location = New System.Drawing.Point(7, 141)
		Me.lblQueryRetries.Margin = New System.Windows.Forms.Padding(6, 0, 6, 0)
		Me.lblQueryRetries.Name = "lblQueryRetries"
		Me.lblQueryRetries.Size = New System.Drawing.Size(118, 24)
		Me.lblQueryRetries.TabIndex = 6
		Me.lblQueryRetries.Text = "Query retries"
		'
		'nudRetryWait
		'
		Me.nudRetryWait.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
		Me.nudRetryWait.Enabled = False
		Me.nudRetryWait.Increment = New Decimal(New Integer() {50, 0, 0, 0})
		Me.nudRetryWait.Location = New System.Drawing.Point(179, 174)
		Me.nudRetryWait.Maximum = New Decimal(New Integer() {65535, 0, 0, 0})
		Me.nudRetryWait.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
		Me.nudRetryWait.Name = "nudRetryWait"
		Me.nudRetryWait.Size = New System.Drawing.Size(91, 29)
		Me.nudRetryWait.TabIndex = 10
		Me.nudRetryWait.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
		Me.nudRetryWait.Value = New Decimal(New Integer() {100, 0, 0, 0})
		'
		'lblRetryWait
		'
		Me.lblRetryWait.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
		Me.lblRetryWait.AutoSize = True
		Me.lblRetryWait.Location = New System.Drawing.Point(7, 176)
		Me.lblRetryWait.Margin = New System.Windows.Forms.Padding(6, 0, 6, 0)
		Me.lblRetryWait.Name = "lblRetryWait"
		Me.lblRetryWait.Size = New System.Drawing.Size(90, 24)
		Me.lblRetryWait.TabIndex = 9
		Me.lblRetryWait.Text = "Retry wait"
		'
		'cbxUseDefaultConnectionRetries
		'
		Me.cbxUseDefaultConnectionRetries.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
		Me.cbxUseDefaultConnectionRetries.AutoSize = True
		Me.cbxUseDefaultConnectionRetries.Checked = True
		Me.cbxUseDefaultConnectionRetries.CheckState = System.Windows.Forms.CheckState.Checked
		Me.cbxUseDefaultConnectionRetries.Location = New System.Drawing.Point(276, 105)
		Me.cbxUseDefaultConnectionRetries.Name = "cbxUseDefaultConnectionRetries"
		Me.cbxUseDefaultConnectionRetries.Size = New System.Drawing.Size(327, 28)
		Me.cbxUseDefaultConnectionRetries.TabIndex = 5
		Me.cbxUseDefaultConnectionRetries.Text = "Use system default values (3 retries)"
		Me.cbxUseDefaultConnectionRetries.UseVisualStyleBackColor = True
		'
		'cbxUseDefaultCommandTimeout
		'
		Me.cbxUseDefaultCommandTimeout.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
		Me.cbxUseDefaultCommandTimeout.AutoSize = True
		Me.cbxUseDefaultCommandTimeout.Checked = True
		Me.cbxUseDefaultCommandTimeout.CheckState = System.Windows.Forms.CheckState.Checked
		Me.cbxUseDefaultCommandTimeout.Location = New System.Drawing.Point(276, 210)
		Me.cbxUseDefaultCommandTimeout.Name = "cbxUseDefaultCommandTimeout"
		Me.cbxUseDefaultCommandTimeout.Size = New System.Drawing.Size(358, 28)
		Me.cbxUseDefaultCommandTimeout.TabIndex = 14
		Me.cbxUseDefaultCommandTimeout.Text = "Use system default values (30 seconds)"
		Me.cbxUseDefaultCommandTimeout.UseVisualStyleBackColor = True
		'
		'cbxUseDefaultQueryRetries
		'
		Me.cbxUseDefaultQueryRetries.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
		Me.cbxUseDefaultQueryRetries.AutoSize = True
		Me.cbxUseDefaultQueryRetries.Checked = True
		Me.cbxUseDefaultQueryRetries.CheckState = System.Windows.Forms.CheckState.Checked
		Me.cbxUseDefaultQueryRetries.Location = New System.Drawing.Point(276, 140)
		Me.cbxUseDefaultQueryRetries.Name = "cbxUseDefaultQueryRetries"
		Me.cbxUseDefaultQueryRetries.Size = New System.Drawing.Size(327, 28)
		Me.cbxUseDefaultQueryRetries.TabIndex = 8
		Me.cbxUseDefaultQueryRetries.Text = "Use system default values (3 retries)"
		Me.cbxUseDefaultQueryRetries.UseVisualStyleBackColor = True
		'
		'cbxUseDefaultRetryWait
		'
		Me.cbxUseDefaultRetryWait.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
		Me.cbxUseDefaultRetryWait.AutoSize = True
		Me.cbxUseDefaultRetryWait.Checked = True
		Me.cbxUseDefaultRetryWait.CheckState = System.Windows.Forms.CheckState.Checked
		Me.cbxUseDefaultRetryWait.Location = New System.Drawing.Point(276, 175)
		Me.cbxUseDefaultRetryWait.Name = "cbxUseDefaultRetryWait"
		Me.cbxUseDefaultRetryWait.Size = New System.Drawing.Size(400, 28)
		Me.cbxUseDefaultRetryWait.TabIndex = 11
		Me.cbxUseDefaultRetryWait.Text = "Use system default values (100 milliseconds)"
		Me.cbxUseDefaultRetryWait.UseVisualStyleBackColor = True
		'
		'btnSave
		'
		Me.btnSave.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
		Me.btnSave.Location = New System.Drawing.Point(601, 244)
		Me.btnSave.Name = "btnSave"
		Me.btnSave.Size = New System.Drawing.Size(113, 65)
		Me.btnSave.TabIndex = 17
		Me.btnSave.Text = "&Save"
		Me.btnSave.UseVisualStyleBackColor = True
		'
		'btnClose
		'
		Me.btnClose.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
		Me.btnClose.Location = New System.Drawing.Point(720, 244)
		Me.btnClose.Name = "btnClose"
		Me.btnClose.Size = New System.Drawing.Size(113, 65)
		Me.btnClose.TabIndex = 0
		Me.btnClose.Text = "&Close"
		Me.btnClose.UseVisualStyleBackColor = True
		'
		'btnImportSettings
		'
		Me.btnImportSettings.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
		Me.btnImportSettings.Location = New System.Drawing.Point(179, 244)
		Me.btnImportSettings.Name = "btnImportSettings"
		Me.btnImportSettings.Size = New System.Drawing.Size(113, 65)
		Me.btnImportSettings.TabIndex = 15
		Me.btnImportSettings.Text = "&Import settings"
		Me.btnImportSettings.UseVisualStyleBackColor = True
		'
		'btnExportSettings
		'
		Me.btnExportSettings.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
		Me.btnExportSettings.Location = New System.Drawing.Point(298, 244)
		Me.btnExportSettings.Name = "btnExportSettings"
		Me.btnExportSettings.Size = New System.Drawing.Size(113, 65)
		Me.btnExportSettings.TabIndex = 16
		Me.btnExportSettings.Text = "&Export settings"
		Me.btnExportSettings.UseVisualStyleBackColor = True
		'
		'openFileDialog1
		'
		Me.openFileDialog1.DefaultExt = "xml"
		Me.openFileDialog1.FileName = "KahlerDataAccessSettings"
		Me.openFileDialog1.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*"
		'
		'saveFileDialog1
		'
		Me.saveFileDialog1.DefaultExt = "xml"
		Me.saveFileDialog1.FileName = "KahlerDataAccessSettings"
		Me.saveFileDialog1.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*"
		'
		'btnCreateConfigXml
		'
		Me.btnCreateConfigXml.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
		Me.btnCreateConfigXml.Location = New System.Drawing.Point(12, 244)
		Me.btnCreateConfigXml.Name = "btnCreateConfigXml"
		Me.btnCreateConfigXml.Size = New System.Drawing.Size(113, 65)
		Me.btnCreateConfigXml.TabIndex = 18
		Me.btnCreateConfigXml.TabStop = False
		Me.btnCreateConfigXml.Text = "Create Config.xml"
		Me.btnCreateConfigXml.UseVisualStyleBackColor = True
		'
		'btnSetDefaultConnectionString
		'
		Me.btnSetDefaultConnectionString.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
		Me.btnSetDefaultConnectionString.Location = New System.Drawing.Point(698, 63)
		Me.btnSetDefaultConnectionString.Name = "btnSetDefaultConnectionString"
		Me.btnSetDefaultConnectionString.Size = New System.Drawing.Size(136, 35)
		Me.btnSetDefaultConnectionString.TabIndex = 19
		Me.btnSetDefaultConnectionString.TabStop = False
		Me.btnSetDefaultConnectionString.Text = "<- Set Default"
		Me.btnSetDefaultConnectionString.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
		Me.btnSetDefaultConnectionString.UseVisualStyleBackColor = True
		'
		'ConfigurationForm
		'
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
		Me.ClientSize = New System.Drawing.Size(846, 321)
		Me.Controls.Add(Me.btnSetDefaultConnectionString)
		Me.Controls.Add(Me.btnCreateConfigXml)
		Me.Controls.Add(Me.btnExportSettings)
		Me.Controls.Add(Me.btnImportSettings)
		Me.Controls.Add(Me.btnClose)
		Me.Controls.Add(Me.btnSave)
		Me.Controls.Add(Me.cbxUseDefaultRetryWait)
		Me.Controls.Add(Me.cbxUseDefaultQueryRetries)
		Me.Controls.Add(Me.cbxUseDefaultCommandTimeout)
		Me.Controls.Add(Me.cbxUseDefaultConnectionRetries)
		Me.Controls.Add(Me.nudRetryWait)
		Me.Controls.Add(Me.lblRetryWait)
		Me.Controls.Add(Me.nudQueryRetries)
		Me.Controls.Add(Me.lblQueryRetries)
		Me.Controls.Add(Me.nudCommandTimeout)
		Me.Controls.Add(Me.lblCommandTimeout)
		Me.Controls.Add(Me.nudConnectionRetries)
		Me.Controls.Add(Me.lblConnectionRetries)
		Me.Controls.Add(Me.tbxConnectionString)
		Me.Controls.Add(Me.lblConnectionString)
		Me.Font = New System.Drawing.Font("Microsoft Sans Serif", 14.25!)
		Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
		Me.Margin = New System.Windows.Forms.Padding(6)
		Me.MinimumSize = New System.Drawing.Size(700, 305)
		Me.Name = "ConfigurationForm"
		Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
		Me.Text = "Kahler Data Access Configuration"
		CType(Me.nudConnectionRetries, System.ComponentModel.ISupportInitialize).EndInit()
		CType(Me.nudCommandTimeout, System.ComponentModel.ISupportInitialize).EndInit()
		CType(Me.nudQueryRetries, System.ComponentModel.ISupportInitialize).EndInit()
		CType(Me.nudRetryWait, System.ComponentModel.ISupportInitialize).EndInit()
		Me.ResumeLayout(False)
		Me.PerformLayout()

	End Sub

	Friend WithEvents lblConnectionString As Windows.Forms.Label
	Friend WithEvents tbxConnectionString As Windows.Forms.TextBox
	Friend WithEvents lblConnectionRetries As Windows.Forms.Label
	Friend WithEvents nudConnectionRetries As Windows.Forms.NumericUpDown
	Friend WithEvents nudCommandTimeout As Windows.Forms.NumericUpDown
	Friend WithEvents lblCommandTimeout As Windows.Forms.Label
	Friend WithEvents nudQueryRetries As Windows.Forms.NumericUpDown
	Friend WithEvents lblQueryRetries As Windows.Forms.Label
	Friend WithEvents nudRetryWait As Windows.Forms.NumericUpDown
	Friend WithEvents lblRetryWait As Windows.Forms.Label
	Friend WithEvents cbxUseDefaultConnectionRetries As Windows.Forms.CheckBox
	Friend WithEvents cbxUseDefaultCommandTimeout As Windows.Forms.CheckBox
	Friend WithEvents cbxUseDefaultQueryRetries As Windows.Forms.CheckBox
	Friend WithEvents cbxUseDefaultRetryWait As Windows.Forms.CheckBox
	Friend WithEvents btnSave As Windows.Forms.Button
	Friend WithEvents btnClose As Windows.Forms.Button
	Friend WithEvents btnImportSettings As Windows.Forms.Button
	Friend WithEvents btnExportSettings As Windows.Forms.Button
	Friend WithEvents openFileDialog1 As Windows.Forms.OpenFileDialog
	Friend WithEvents saveFileDialog1 As Windows.Forms.SaveFileDialog
	Friend WithEvents btnCreateConfigXml As Windows.Forms.Button
	Friend WithEvents btnSetDefaultConnectionString As Windows.Forms.Button
End Class
