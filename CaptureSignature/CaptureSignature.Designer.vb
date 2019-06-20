<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class CaptureSignature
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(CaptureSignature))
        Me.pnlNoResponse = New System.Windows.Forms.Panel()
        Me.lblNoResponse = New System.Windows.Forms.Label()
        Me.btnSaveAsFile = New System.Windows.Forms.Button()
        Me.btnClear = New System.Windows.Forms.Button()
        Me.signature = New Topaz.SigPlusNET()
        Me.btnSaveToUser = New System.Windows.Forms.Button()
        Me.cmbComPort = New System.Windows.Forms.ComboBox()
        Me.cmbUsers = New System.Windows.Forms.ComboBox()
        Me.btnSaveToTicket = New System.Windows.Forms.Button()
        Me.cmbTickets = New System.Windows.Forms.ComboBox()
        Me.rbUserSignature = New System.Windows.Forms.RadioButton()
        Me.rbDriverSignature = New System.Windows.Forms.RadioButton()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.GroupBox3 = New System.Windows.Forms.GroupBox()
        Me.SaveFileDialog1 = New System.Windows.Forms.SaveFileDialog()
        Me.pnlNoResponse.SuspendLayout()
        Me.GroupBox1.SuspendLayout()
        Me.GroupBox2.SuspendLayout()
        Me.GroupBox3.SuspendLayout()
        Me.SuspendLayout()
        '
        'pnlNoResponse
        '
        Me.pnlNoResponse.Controls.Add(Me.lblNoResponse)
        Me.pnlNoResponse.Location = New System.Drawing.Point(12, 50)
        Me.pnlNoResponse.Name = "pnlNoResponse"
        Me.pnlNoResponse.Size = New System.Drawing.Size(355, 122)
        Me.pnlNoResponse.TabIndex = 0
        Me.pnlNoResponse.Visible = False
        '
        'lblNoResponse
        '
        Me.lblNoResponse.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblNoResponse.Location = New System.Drawing.Point(60, 36)
        Me.lblNoResponse.Name = "lblNoResponse"
        Me.lblNoResponse.Size = New System.Drawing.Size(234, 50)
        Me.lblNoResponse.TabIndex = 0
        Me.lblNoResponse.Text = "No response from signature pad (check connections)"
        Me.lblNoResponse.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'btnSaveAsFile
        '
        Me.btnSaveAsFile.Font = New System.Drawing.Font("Microsoft Sans Serif", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnSaveAsFile.Location = New System.Drawing.Point(244, 16)
        Me.btnSaveAsFile.Name = "btnSaveAsFile"
        Me.btnSaveAsFile.Size = New System.Drawing.Size(114, 59)
        Me.btnSaveAsFile.TabIndex = 2
        Me.btnSaveAsFile.Text = "Save as File"
        Me.btnSaveAsFile.UseVisualStyleBackColor = True
        '
        'btnClear
        '
        Me.btnClear.Font = New System.Drawing.Font("Microsoft Sans Serif", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnClear.Location = New System.Drawing.Point(3, 16)
        Me.btnClear.Name = "btnClear"
        Me.btnClear.Size = New System.Drawing.Size(114, 59)
        Me.btnClear.TabIndex = 1
        Me.btnClear.Text = "Clear"
        Me.btnClear.UseVisualStyleBackColor = True
        '
        'signature
        '
        Me.signature.BackColor = System.Drawing.Color.White
        Me.signature.ForeColor = System.Drawing.Color.Black
        Me.signature.Location = New System.Drawing.Point(12, 50)
        Me.signature.Name = "signature"
        Me.signature.Size = New System.Drawing.Size(355, 122)
        Me.signature.TabIndex = 4
        Me.signature.Text = "sigPlusNET1"
        '
        'btnSaveToUser
        '
        Me.btnSaveToUser.Font = New System.Drawing.Font("Microsoft Sans Serif", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnSaveToUser.Location = New System.Drawing.Point(244, 16)
        Me.btnSaveToUser.Name = "btnSaveToUser"
        Me.btnSaveToUser.Size = New System.Drawing.Size(114, 59)
        Me.btnSaveToUser.TabIndex = 3
        Me.btnSaveToUser.Text = "Save to user"
        Me.btnSaveToUser.UseVisualStyleBackColor = True
        '
        'cmbComPort
        '
        Me.cmbComPort.FormattingEnabled = True
        Me.cmbComPort.Location = New System.Drawing.Point(9, 12)
        Me.cmbComPort.Name = "cmbComPort"
        Me.cmbComPort.Size = New System.Drawing.Size(360, 32)
        Me.cmbComPort.TabIndex = 5
        '
        'cmbUsers
        '
        Me.cmbUsers.FormattingEnabled = True
        Me.cmbUsers.Location = New System.Drawing.Point(3, 30)
        Me.cmbUsers.Name = "cmbUsers"
        Me.cmbUsers.Size = New System.Drawing.Size(239, 32)
        Me.cmbUsers.TabIndex = 6
        '
        'btnSaveToTicket
        '
        Me.btnSaveToTicket.Font = New System.Drawing.Font("Microsoft Sans Serif", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnSaveToTicket.Location = New System.Drawing.Point(244, 18)
        Me.btnSaveToTicket.Name = "btnSaveToTicket"
        Me.btnSaveToTicket.Size = New System.Drawing.Size(114, 59)
        Me.btnSaveToTicket.TabIndex = 7
        Me.btnSaveToTicket.Text = "Save to ticket"
        Me.btnSaveToTicket.UseVisualStyleBackColor = True
        '
        'cmbTickets
        '
        Me.cmbTickets.FormattingEnabled = True
        Me.cmbTickets.Location = New System.Drawing.Point(3, 32)
        Me.cmbTickets.Name = "cmbTickets"
        Me.cmbTickets.Size = New System.Drawing.Size(239, 32)
        Me.cmbTickets.TabIndex = 8
        '
        'rbUserSignature
        '
        Me.rbUserSignature.AutoSize = True
        Me.rbUserSignature.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!)
        Me.rbUserSignature.Location = New System.Drawing.Point(3, 83)
        Me.rbUserSignature.Name = "rbUserSignature"
        Me.rbUserSignature.Size = New System.Drawing.Size(134, 24)
        Me.rbUserSignature.TabIndex = 9
        Me.rbUserSignature.TabStop = True
        Me.rbUserSignature.Text = "User Signature"
        Me.rbUserSignature.UseVisualStyleBackColor = True
        '
        'rbDriverSignature
        '
        Me.rbDriverSignature.AutoSize = True
        Me.rbDriverSignature.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!)
        Me.rbDriverSignature.Location = New System.Drawing.Point(144, 83)
        Me.rbDriverSignature.Name = "rbDriverSignature"
        Me.rbDriverSignature.Size = New System.Drawing.Size(141, 24)
        Me.rbDriverSignature.TabIndex = 10
        Me.rbDriverSignature.TabStop = True
        Me.rbDriverSignature.Text = "Driver Signature"
        Me.rbDriverSignature.UseVisualStyleBackColor = True
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.btnClear)
        Me.GroupBox1.Controls.Add(Me.btnSaveAsFile)
        Me.GroupBox1.Location = New System.Drawing.Point(9, 178)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(360, 81)
        Me.GroupBox1.TabIndex = 11
        Me.GroupBox1.TabStop = False
        '
        'GroupBox2
        '
        Me.GroupBox2.Controls.Add(Me.cmbUsers)
        Me.GroupBox2.Controls.Add(Me.btnSaveToUser)
        Me.GroupBox2.Location = New System.Drawing.Point(9, 265)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Size = New System.Drawing.Size(360, 82)
        Me.GroupBox2.TabIndex = 12
        Me.GroupBox2.TabStop = False
        '
        'GroupBox3
        '
        Me.GroupBox3.Controls.Add(Me.cmbTickets)
        Me.GroupBox3.Controls.Add(Me.btnSaveToTicket)
        Me.GroupBox3.Controls.Add(Me.rbDriverSignature)
        Me.GroupBox3.Controls.Add(Me.rbUserSignature)
        Me.GroupBox3.Location = New System.Drawing.Point(9, 353)
        Me.GroupBox3.Name = "GroupBox3"
        Me.GroupBox3.Size = New System.Drawing.Size(360, 110)
        Me.GroupBox3.TabIndex = 13
        Me.GroupBox3.TabStop = False
        '
        'CaptureSignature
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.AutoSize = True
        Me.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.ClientSize = New System.Drawing.Size(379, 468)
        Me.Controls.Add(Me.GroupBox3)
        Me.Controls.Add(Me.GroupBox2)
        Me.Controls.Add(Me.cmbComPort)
        Me.Controls.Add(Me.GroupBox1)
        Me.Controls.Add(Me.pnlNoResponse)
        Me.Controls.Add(Me.signature)
        Me.Font = New System.Drawing.Font("Microsoft Sans Serif", 14.25!)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "CaptureSignature"
        Me.Text = "Capture Signature"
        Me.pnlNoResponse.ResumeLayout(False)
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox2.ResumeLayout(False)
        Me.GroupBox3.ResumeLayout(False)
        Me.GroupBox3.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents pnlNoResponse As System.Windows.Forms.Panel
    Private WithEvents lblNoResponse As System.Windows.Forms.Label
    Private WithEvents btnSaveAsFile As System.Windows.Forms.Button
    Private WithEvents btnClear As System.Windows.Forms.Button
    Private WithEvents signature As Topaz.SigPlusNET
    Private WithEvents btnSaveToUser As System.Windows.Forms.Button
    Friend WithEvents cmbComPort As System.Windows.Forms.ComboBox
    Friend WithEvents cmbUsers As System.Windows.Forms.ComboBox
    Private WithEvents btnSaveToTicket As System.Windows.Forms.Button
    Friend WithEvents cmbTickets As System.Windows.Forms.ComboBox
    Friend WithEvents rbUserSignature As System.Windows.Forms.RadioButton
    Friend WithEvents rbDriverSignature As System.Windows.Forms.RadioButton
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Friend WithEvents GroupBox2 As System.Windows.Forms.GroupBox
    Friend WithEvents GroupBox3 As System.Windows.Forms.GroupBox
    Friend WithEvents SaveFileDialog1 As System.Windows.Forms.SaveFileDialog

End Class
