Imports KahlerAutomation.KaTm2Database
Imports System.IO.Ports
Imports System.Data.OleDb
Imports System.Reflection

Public Class CaptureSignature
    Private _port As Integer
    Private Const CLEAR_HOTSPOT As Integer = 0
    Private Const OK_HOTSPOT As Integer = 1
    Private _message As String

#Region " Events "
    Private Sub CaptureSignature_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load
        Common.PopulateComPorts(cmbComPort)
        Common.PopulateUsers(cmbUsers)
        Common.PopulateTickets(cmbTickets)
        cmbComPort.SelectedIndex = 0
        cmbTickets.SelectedIndex = 0
        cmbUsers.SelectedIndex = 0
    End Sub

    Private Sub CaptureSignature_FormClosing(sender As Object, e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        Try
            If SignaturePadConnected() Then
                signature.SetTabletState(1)
                signature.LCDRefresh(0, 0, 0, 240, 64)
                signature.ClearTablet()
            End If
        Catch ex As Exception

        End Try
    End Sub

    Private Sub cmbComPort_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles cmbComPort.SelectedIndexChanged
        signature.SetTabletState(0)
        _port = CType(cmbComPort.SelectedItem, SignaturePadConnection).Port
        pnlNoResponse.Visible = Not GetSignature()
    End Sub

    Private Sub btnClear_Click(sender As System.Object, e As System.EventArgs) Handles btnClear.Click
        If cmbComPort.SelectedIndex > 0 Then
            pnlNoResponse.Visible = Not GetSignature()
        End If
    End Sub

    Private Sub btnSaveAsFile_Click(sender As System.Object, e As System.EventArgs) Handles btnSaveAsFile.Click
        If cmbComPort.SelectedIndex > 0 And signature.GetTabletState() <> 0 Then
            With SaveFileDialog1
                Common.SaveSignatureAsFile(SaveFileDialog1)
                If .ShowDialog() = Windows.Forms.DialogResult.OK Then
                    Dim signature As Image = GetImage()
                    Dim fileLocation As String = My.Application.Info.DirectoryPath & "\" & "UserSignature.jpg"
                    Try
                        fileLocation = SaveFileDialog1.FileName
                        signature.Save(fileLocation)
                        Close()
                    Catch ex As Exception
                        MessageBox.Show(ex.ToString)
                        cmbComPort_SelectedIndexChanged(cmbComPort, New EventArgs)
                    End Try
                End If
            End With
        End If
    End Sub

    Private Sub btnSaveToUser_Click(sender As System.Object, e As System.EventArgs) Handles btnSaveToUser.Click
        If cmbUsers.SelectedIndex > 0 And cmbComPort.SelectedIndex > 0 And signature.GetTabletState() <> 0 Then
            Try
                Common.SaveSignatureToUser(CType(cmbUsers.SelectedItem, KaCommonObjects.ComboBoxItem).Value, GetImage())
                Close()
            Catch ex As Exception
                MessageBox.Show(ex.ToString)
                cmbComPort_SelectedIndexChanged(cmbComPort, New EventArgs)
            End Try
        End If
    End Sub

    Private Sub btnSaveToTicket_Click(sender As System.Object, e As System.EventArgs) Handles btnSaveToTicket.Click
        If cmbTickets.SelectedIndex > 0 And (rbDriverSignature.Checked Or rbUserSignature.Checked) And cmbComPort.SelectedIndex > 0 And signature.GetTabletState() <> 0 Then
            Try
                Common.SaveSignatureToTicket(GetImage(), CType(cmbTickets.SelectedItem, KaCommonObjects.ComboBoxItem).Value, rbDriverSignature)
                Close()
            Catch ex As Exception
                MessageBox.Show(ex.ToString)
                cmbComPort_SelectedIndexChanged(cmbComPort, New EventArgs)
            End Try
        End If
    End Sub
#End Region

    Private Function SignaturePadConnected() As Boolean
        If _port = -3 Then
            signature.SetTabletComPort(1)
            signature.SetTabletType(6)
        ElseIf _port = -2 Then
            signature.SetTabletComPort(1)
            signature.SetTabletType(2)
        Else ' serial port
            signature.SetTabletComPort(_port)
            signature.SetTabletType(0)
        End If
        signature.SetTabletComTest(1)
        signature.SetTabletState(1)
        Dim connected As Boolean = signature.GetTabletState() <> 0
        signature.SetTabletComTest(0)
        Return connected
    End Function

    Public Function GetSignature() As Boolean
        If cmbComPort.SelectedIndex > 0 Then
            If SignaturePadConnected() Then
                signature.SetTabletState(1)
                signature.LCDRefresh(0, 0, 0, 240, 64)
                signature.SetTranslateBitmapEnable(False)
                signature.LCDSetTabletMap(0, 240, 64, 100, 0, 1900, 700)
                signature.SetTabletXStart(400)
                signature.SetTabletXStop(2400)
                signature.SetTabletYStart(350)
                signature.SetTabletYStop(1050)
                signature.SetTabletLogicalXSize(2000)
                signature.SetTabletLogicalYSize(700)
                signature.LCDRefresh(0, 0, 0, 240, 64)
                signature.LCDWriteString(0, 2, 55, 4, New Font("Courier", 10), "Please sign")
                signature.LCDSendGraphic(0, 2, 0, 20, My.Resources.signaturebox)  ' set background image
                signature.ClearTablet()
                signature.LCDSetWindow(0, 22, 240, 40)  ' set signature area
                signature.SetSigWindow(1, 0, 22, 240, 40)
                signature.SetLCDCaptureMode(2) ' do not auto clear ink
                Return True
            Else
                Return False
            End If
        Else
            Return False
        End If
    End Function

    Private Function GetImage() As Image
        If Not cmbComPort.SelectedIndex = 0 Then
            Try
                If Not pnlNoResponse.Visible Then ' update signature panel if connected
                    Dim signatureImage As Image = New Bitmap(1, 1)
                    signature.SetImageFileFormat(4)
                    signature.SetImageXSize(300)
                    signature.SetImageYSize(80)
                    signatureImage = signature.GetSigImage()
                    signature.LCDRefresh(0, 0, 0, 240, 64)
                    signature.LCDWriteString(0, 2, 15, 25, New Font("Arial", 9, FontStyle.Bold), "Thank you")
                    signature.KeyPadClearHotSpotList()
                    signature.SetSigWindow(1, 0, 0, 0, 0)
                    signature.LCDSetWindow(0, 0, 0, 0)
                    Return signatureImage
                End If
            Catch pnlNoResponseNotVisible As Exception
                MessageBox.Show(pnlNoResponseNotVisible.ToString)
            End Try
        End If
        Return New Bitmap(1, 1)
    End Function

End Class
