<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Configuration
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Configuration))
        Me.btnOk = New System.Windows.Forms.Button()
        Me.btnCancel = New System.Windows.Forms.Button()
        Me.btnApply = New System.Windows.Forms.Button()
        Me.TabControl1 = New System.Windows.Forms.TabControl()
        Me.tpLocations = New System.Windows.Forms.TabPage()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.clbLocations = New System.Windows.Forms.CheckedListBox()
        Me.tpEmail = New System.Windows.Forms.TabPage()
        Me.lblEmailAddress = New System.Windows.Forms.Label()
        Me.tbxEmail = New System.Windows.Forms.TextBox()
        Me.lblError = New System.Windows.Forms.Label()
        Me.tbxResendInterval = New System.Windows.Forms.TextBox()
        Me.lblInterval = New System.Windows.Forms.Label()
        Me.lblVersion = New System.Windows.Forms.Label()
        Me.TabControl1.SuspendLayout()
        Me.tpLocations.SuspendLayout()
        Me.tpEmail.SuspendLayout()
        Me.SuspendLayout()
        '
        'btnOk
        '
        Me.btnOk.Location = New System.Drawing.Point(131, 297)
        Me.btnOk.Margin = New System.Windows.Forms.Padding(4)
        Me.btnOk.Name = "btnOk"
        Me.btnOk.Padding = New System.Windows.Forms.Padding(1)
        Me.btnOk.Size = New System.Drawing.Size(100, 28)
        Me.btnOk.TabIndex = 0
        Me.btnOk.Text = "OK"
        Me.btnOk.UseVisualStyleBackColor = True
        '
        'btnCancel
        '
        Me.btnCancel.Location = New System.Drawing.Point(239, 297)
        Me.btnCancel.Margin = New System.Windows.Forms.Padding(4)
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.Padding = New System.Windows.Forms.Padding(1)
        Me.btnCancel.Size = New System.Drawing.Size(100, 28)
        Me.btnCancel.TabIndex = 1
        Me.btnCancel.Text = "Cancel"
        Me.btnCancel.UseVisualStyleBackColor = True
        '
        'btnApply
        '
        Me.btnApply.Location = New System.Drawing.Point(347, 297)
        Me.btnApply.Margin = New System.Windows.Forms.Padding(4)
        Me.btnApply.Name = "btnApply"
        Me.btnApply.Padding = New System.Windows.Forms.Padding(1)
        Me.btnApply.Size = New System.Drawing.Size(100, 28)
        Me.btnApply.TabIndex = 2
        Me.btnApply.Text = "Apply"
        Me.btnApply.UseVisualStyleBackColor = True
        '
        'TabControl1
        '
        Me.TabControl1.Controls.Add(Me.tpLocations)
        Me.TabControl1.Controls.Add(Me.tpEmail)
        Me.TabControl1.Location = New System.Drawing.Point(5, 5)
        Me.TabControl1.Margin = New System.Windows.Forms.Padding(4)
        Me.TabControl1.Name = "TabControl1"
        Me.TabControl1.SelectedIndex = 0
        Me.TabControl1.Size = New System.Drawing.Size(441, 289)
        Me.TabControl1.TabIndex = 3
        '
        'tpLocations
        '
        Me.tpLocations.Controls.Add(Me.Label1)
        Me.tpLocations.Controls.Add(Me.clbLocations)
        Me.tpLocations.Location = New System.Drawing.Point(4, 25)
        Me.tpLocations.Margin = New System.Windows.Forms.Padding(4)
        Me.tpLocations.Name = "tpLocations"
        Me.tpLocations.Padding = New System.Windows.Forms.Padding(1)
        Me.tpLocations.Size = New System.Drawing.Size(433, 260)
        Me.tpLocations.TabIndex = 0
        Me.tpLocations.Text = "Locations"
        Me.tpLocations.UseVisualStyleBackColor = True
        '
        'Label1
        '
        Me.Label1.Location = New System.Drawing.Point(5, 16)
        Me.Label1.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(420, 54)
        Me.Label1.TabIndex = 2
        Me.Label1.Text = "Select the locations that the service running on this computer should read. If no" &
    "ne of the locations are selected the service will attempt to read tanks for all " &
    "locations."
        '
        'clbLocations
        '
        Me.clbLocations.CheckOnClick = True
        Me.clbLocations.FormattingEnabled = True
        Me.clbLocations.Location = New System.Drawing.Point(5, 81)
        Me.clbLocations.Margin = New System.Windows.Forms.Padding(4)
        Me.clbLocations.Name = "clbLocations"
        Me.clbLocations.Size = New System.Drawing.Size(419, 157)
        Me.clbLocations.TabIndex = 1
        '
        'tpEmail
        '
        Me.tpEmail.Controls.Add(Me.lblEmailAddress)
        Me.tpEmail.Controls.Add(Me.tbxEmail)
        Me.tpEmail.Controls.Add(Me.lblError)
        Me.tpEmail.Controls.Add(Me.tbxResendInterval)
        Me.tpEmail.Controls.Add(Me.lblInterval)
        Me.tpEmail.Location = New System.Drawing.Point(4, 25)
        Me.tpEmail.Name = "tpEmail"
        Me.tpEmail.Size = New System.Drawing.Size(433, 260)
        Me.tpEmail.TabIndex = 1
        Me.tpEmail.Text = "Email"
        Me.tpEmail.UseVisualStyleBackColor = True
        '
        'lblEmailAddress
        '
        Me.lblEmailAddress.AutoSize = True
        Me.lblEmailAddress.Location = New System.Drawing.Point(99, 23)
        Me.lblEmailAddress.Name = "lblEmailAddress"
        Me.lblEmailAddress.Size = New System.Drawing.Size(97, 17)
        Me.lblEmailAddress.TabIndex = 7
        Me.lblEmailAddress.Text = "Email address"
        '
        'tbxEmail
        '
        Me.tbxEmail.Location = New System.Drawing.Point(202, 20)
        Me.tbxEmail.Name = "tbxEmail"
        Me.tbxEmail.Size = New System.Drawing.Size(228, 22)
        Me.tbxEmail.TabIndex = 1
        '
        'lblError
        '
        Me.lblError.AutoSize = True
        Me.lblError.Location = New System.Drawing.Point(6, 191)
        Me.lblError.Name = "lblError"
        Me.lblError.Size = New System.Drawing.Size(0, 17)
        Me.lblError.TabIndex = 6
        '
        'tbxResendInterval
        '
        Me.tbxResendInterval.Location = New System.Drawing.Point(202, 48)
        Me.tbxResendInterval.Name = "tbxResendInterval"
        Me.tbxResendInterval.Size = New System.Drawing.Size(228, 22)
        Me.tbxResendInterval.TabIndex = 5
        '
        'lblInterval
        '
        Me.lblInterval.AutoSize = True
        Me.lblInterval.Location = New System.Drawing.Point(6, 51)
        Me.lblInterval.Name = "lblInterval"
        Me.lblInterval.Size = New System.Drawing.Size(190, 17)
        Me.lblInterval.TabIndex = 2
        Me.lblInterval.Text = "Email resend interval (hours)"
        '
        'lblVersion
        '
        Me.lblVersion.AutoSize = True
        Me.lblVersion.Location = New System.Drawing.Point(9, 304)
        Me.lblVersion.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblVersion.Name = "lblVersion"
        Me.lblVersion.Size = New System.Drawing.Size(0, 17)
        Me.lblVersion.TabIndex = 4
        '
        'Configuration
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(452, 330)
        Me.Controls.Add(Me.lblVersion)
        Me.Controls.Add(Me.TabControl1)
        Me.Controls.Add(Me.btnApply)
        Me.Controls.Add(Me.btnCancel)
        Me.Controls.Add(Me.btnOk)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Margin = New System.Windows.Forms.Padding(4)
        Me.MaximizeBox = False
        Me.Name = "Configuration"
        Me.Padding = New System.Windows.Forms.Padding(1)
        Me.Text = "Tank Level Status Reader Configuration"
        Me.TabControl1.ResumeLayout(False)
        Me.tpLocations.ResumeLayout(False)
        Me.tpEmail.ResumeLayout(False)
        Me.tpEmail.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents btnOk As System.Windows.Forms.Button
    Friend WithEvents btnCancel As System.Windows.Forms.Button
    Friend WithEvents btnApply As System.Windows.Forms.Button
    Friend WithEvents TabControl1 As System.Windows.Forms.TabControl
    Friend WithEvents tpLocations As System.Windows.Forms.TabPage
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents clbLocations As System.Windows.Forms.CheckedListBox
    Friend WithEvents lblVersion As System.Windows.Forms.Label
    Friend WithEvents tpEmail As TabPage
    Friend WithEvents lblInterval As Label
    Friend WithEvents tbxEmail As TextBox
    Friend WithEvents tbxResendInterval As TextBox
    Friend WithEvents lblError As Label
    Friend WithEvents lblEmailAddress As Label
End Class
