<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class CaptureSignatureAx
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(CaptureSignatureAx))
        Me.btnClearAx = New System.Windows.Forms.Button()
        Me.GroupBox1Ax = New System.Windows.Forms.GroupBox()
        Me.btnSaveAsFileAx = New System.Windows.Forms.Button()
        Me.GroupBox2Ax = New System.Windows.Forms.GroupBox()
        Me.cmbUsersAx = New System.Windows.Forms.ComboBox()
        Me.btnSaveToUserAx = New System.Windows.Forms.Button()
        Me.GroupBox3Ax = New System.Windows.Forms.GroupBox()
        Me.rbDriverSignatureAx = New System.Windows.Forms.RadioButton()
        Me.rbUserSignatureAx = New System.Windows.Forms.RadioButton()
        Me.cmbTicketsAx = New System.Windows.Forms.ComboBox()
        Me.btnSaveToTicketAx = New System.Windows.Forms.Button()
        Me.cmbComPortAx = New System.Windows.Forms.ComboBox()
        Me.signatureAx = New AxSIGPLUSLib.AxSigPlus()
        Me.pnlNoResponseAx = New System.Windows.Forms.Panel()
        Me.lblNoResponse = New System.Windows.Forms.Label()
        Me.SaveFileDialog1 = New System.Windows.Forms.SaveFileDialog()
        Me.GroupBox1Ax.SuspendLayout()
        Me.GroupBox2Ax.SuspendLayout()
        Me.GroupBox3Ax.SuspendLayout()
        CType(Me.signatureAx, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.pnlNoResponseAx.SuspendLayout()
        Me.SuspendLayout()
        '
        'btnClearAx
        '
        Me.btnClearAx.Font = New System.Drawing.Font("Microsoft Sans Serif", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnClearAx.Location = New System.Drawing.Point(3, 16)
        Me.btnClearAx.Name = "btnClearAx"
        Me.btnClearAx.Size = New System.Drawing.Size(114, 59)
        Me.btnClearAx.TabIndex = 1
        Me.btnClearAx.Text = "Clear"
        Me.btnClearAx.UseVisualStyleBackColor = True
        '
        'GroupBox1Ax
        '
        Me.GroupBox1Ax.Controls.Add(Me.btnSaveAsFileAx)
        Me.GroupBox1Ax.Controls.Add(Me.btnClearAx)
        Me.GroupBox1Ax.Font = New System.Drawing.Font("Microsoft Sans Serif", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.GroupBox1Ax.Location = New System.Drawing.Point(9, 178)
        Me.GroupBox1Ax.Name = "GroupBox1Ax"
        Me.GroupBox1Ax.Size = New System.Drawing.Size(360, 81)
        Me.GroupBox1Ax.TabIndex = 11
        Me.GroupBox1Ax.TabStop = False
        '
        'btnSaveAsFileAx
        '
        Me.btnSaveAsFileAx.Location = New System.Drawing.Point(244, 16)
        Me.btnSaveAsFileAx.Name = "btnSaveAsFileAx"
        Me.btnSaveAsFileAx.Size = New System.Drawing.Size(114, 59)
        Me.btnSaveAsFileAx.TabIndex = 2
        Me.btnSaveAsFileAx.Text = "Save as File"
        Me.btnSaveAsFileAx.UseVisualStyleBackColor = True
        '
        'GroupBox2Ax
        '
        Me.GroupBox2Ax.Controls.Add(Me.cmbUsersAx)
        Me.GroupBox2Ax.Controls.Add(Me.btnSaveToUserAx)
        Me.GroupBox2Ax.Font = New System.Drawing.Font("Microsoft Sans Serif", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.GroupBox2Ax.Location = New System.Drawing.Point(9, 265)
        Me.GroupBox2Ax.Name = "GroupBox2Ax"
        Me.GroupBox2Ax.Size = New System.Drawing.Size(360, 82)
        Me.GroupBox2Ax.TabIndex = 12
        Me.GroupBox2Ax.TabStop = False
        '
        'cmbUsersAx
        '
        Me.cmbUsersAx.DropDownWidth = 239
        Me.cmbUsersAx.FormattingEnabled = True
        Me.cmbUsersAx.Location = New System.Drawing.Point(3, 30)
        Me.cmbUsersAx.Name = "cmbUsersAx"
        Me.cmbUsersAx.Size = New System.Drawing.Size(239, 32)
        Me.cmbUsersAx.TabIndex = 6
        '
        'btnSaveToUserAx
        '
        Me.btnSaveToUserAx.Location = New System.Drawing.Point(244, 16)
        Me.btnSaveToUserAx.Name = "btnSaveToUserAx"
        Me.btnSaveToUserAx.Size = New System.Drawing.Size(114, 59)
        Me.btnSaveToUserAx.TabIndex = 3
        Me.btnSaveToUserAx.Text = "Save to user"
        Me.btnSaveToUserAx.UseVisualStyleBackColor = True
        '
        'GroupBox3Ax
        '
        Me.GroupBox3Ax.Controls.Add(Me.rbDriverSignatureAx)
        Me.GroupBox3Ax.Controls.Add(Me.rbUserSignatureAx)
        Me.GroupBox3Ax.Controls.Add(Me.cmbTicketsAx)
        Me.GroupBox3Ax.Controls.Add(Me.btnSaveToTicketAx)
        Me.GroupBox3Ax.Font = New System.Drawing.Font("Microsoft Sans Serif", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.GroupBox3Ax.Location = New System.Drawing.Point(9, 353)
        Me.GroupBox3Ax.Name = "GroupBox3Ax"
        Me.GroupBox3Ax.Size = New System.Drawing.Size(360, 110)
        Me.GroupBox3Ax.TabIndex = 13
        Me.GroupBox3Ax.TabStop = False
        '
        'rbDriverSignatureAx
        '
        Me.rbDriverSignatureAx.AutoSize = True
        Me.rbDriverSignatureAx.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.rbDriverSignatureAx.Location = New System.Drawing.Point(144, 83)
        Me.rbDriverSignatureAx.Name = "rbDriverSignatureAx"
        Me.rbDriverSignatureAx.Size = New System.Drawing.Size(141, 24)
        Me.rbDriverSignatureAx.TabIndex = 10
        Me.rbDriverSignatureAx.TabStop = True
        Me.rbDriverSignatureAx.Text = "Driver Signature"
        Me.rbDriverSignatureAx.UseVisualStyleBackColor = True
        '
        'rbUserSignatureAx
        '
        Me.rbUserSignatureAx.AutoSize = True
        Me.rbUserSignatureAx.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.rbUserSignatureAx.Location = New System.Drawing.Point(3, 83)
        Me.rbUserSignatureAx.Name = "rbUserSignatureAx"
        Me.rbUserSignatureAx.Size = New System.Drawing.Size(134, 24)
        Me.rbUserSignatureAx.TabIndex = 9
        Me.rbUserSignatureAx.TabStop = True
        Me.rbUserSignatureAx.Text = "User Signature"
        Me.rbUserSignatureAx.UseVisualStyleBackColor = True
        '
        'cmbTicketsAx
        '
        Me.cmbTicketsAx.DropDownWidth = 239
        Me.cmbTicketsAx.FormattingEnabled = True
        Me.cmbTicketsAx.Location = New System.Drawing.Point(3, 32)
        Me.cmbTicketsAx.Name = "cmbTicketsAx"
        Me.cmbTicketsAx.Size = New System.Drawing.Size(239, 32)
        Me.cmbTicketsAx.TabIndex = 8
        '
        'btnSaveToTicketAx
        '
        Me.btnSaveToTicketAx.Location = New System.Drawing.Point(244, 18)
        Me.btnSaveToTicketAx.Name = "btnSaveToTicketAx"
        Me.btnSaveToTicketAx.Size = New System.Drawing.Size(114, 59)
        Me.btnSaveToTicketAx.TabIndex = 7
        Me.btnSaveToTicketAx.Text = "Save to ticket"
        Me.btnSaveToTicketAx.UseVisualStyleBackColor = True
        '
        'cmbComPortAx
        '
        Me.cmbComPortAx.Font = New System.Drawing.Font("Microsoft Sans Serif", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmbComPortAx.FormattingEnabled = True
        Me.cmbComPortAx.Location = New System.Drawing.Point(9, 12)
        Me.cmbComPortAx.Name = "cmbComPortAx"
        Me.cmbComPortAx.Size = New System.Drawing.Size(360, 32)
        Me.cmbComPortAx.TabIndex = 5
        '
        'signatureAx
        '
        Me.signatureAx.Enabled = True
        Me.signatureAx.Location = New System.Drawing.Point(12, 50)
        Me.signatureAx.Name = "signatureAx"
        Me.signatureAx.OcxState = CType(resources.GetObject("signatureAx.OcxState"), System.Windows.Forms.AxHost.State)
        Me.signatureAx.Size = New System.Drawing.Size(355, 122)
        Me.signatureAx.TabIndex = 4
        '
        'pnlNoResponseAx
        '
        Me.pnlNoResponseAx.Controls.Add(Me.lblNoResponse)
        Me.pnlNoResponseAx.Font = New System.Drawing.Font("Microsoft Sans Serif", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.pnlNoResponseAx.Location = New System.Drawing.Point(12, 50)
        Me.pnlNoResponseAx.Name = "pnlNoResponseAx"
        Me.pnlNoResponseAx.Size = New System.Drawing.Size(355, 122)
        Me.pnlNoResponseAx.TabIndex = 0
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
        'CaptureSignatureAx
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.AutoSize = True
        Me.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.ClientSize = New System.Drawing.Size(379, 468)
        Me.Controls.Add(Me.pnlNoResponseAx)
        Me.Controls.Add(Me.signatureAx)
        Me.Controls.Add(Me.cmbComPortAx)
        Me.Controls.Add(Me.GroupBox3Ax)
        Me.Controls.Add(Me.GroupBox2Ax)
        Me.Controls.Add(Me.GroupBox1Ax)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "CaptureSignatureAx"
        Me.Text = "Capture Signature"
        Me.GroupBox1Ax.ResumeLayout(False)
        Me.GroupBox2Ax.ResumeLayout(False)
        Me.GroupBox3Ax.ResumeLayout(False)
        Me.GroupBox3Ax.PerformLayout()
        CType(Me.signatureAx, System.ComponentModel.ISupportInitialize).EndInit()
        Me.pnlNoResponseAx.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents btnClearAx As System.Windows.Forms.Button
    Friend WithEvents GroupBox1Ax As System.Windows.Forms.GroupBox
    Friend WithEvents GroupBox2Ax As System.Windows.Forms.GroupBox
    Friend WithEvents GroupBox3Ax As System.Windows.Forms.GroupBox
    Friend WithEvents btnSaveAsFileAx As System.Windows.Forms.Button
    Friend WithEvents btnSaveToUserAx As System.Windows.Forms.Button
    Friend WithEvents btnSaveToTicketAx As System.Windows.Forms.Button
    Friend WithEvents cmbComPortAx As System.Windows.Forms.ComboBox
    Friend WithEvents cmbUsersAx As System.Windows.Forms.ComboBox
    Friend WithEvents cmbTicketsAx As System.Windows.Forms.ComboBox
    Friend WithEvents rbUserSignatureAx As System.Windows.Forms.RadioButton
    Friend WithEvents rbDriverSignatureAx As System.Windows.Forms.RadioButton
    Private WithEvents pnlNoResponseAx As System.Windows.Forms.Panel
    Private WithEvents signatureAx As AxSIGPLUSLib.AxSigPlus
    Private WithEvents lblNoResponse As System.Windows.Forms.Label
    Friend WithEvents SaveFileDialog1 As System.Windows.Forms.SaveFileDialog
End Class
