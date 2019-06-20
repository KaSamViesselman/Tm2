Imports KahlerAutomation.KaTm2Database
Imports System.IO.Ports
Imports System.Data.OleDb
Imports System.Reflection
Imports System.IO

Public Class CaptureSignatureAx
    Private _port As Integer
    Private _message As String

    Private Sub CaptureSignatureAx_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load
        Common.PopulateComPorts(cmbComPortAx)
        Common.PopulateUsers(cmbUsersAx)
        Common.PopulateTickets(cmbTicketsAx)
        cmbComPortAx.SelectedIndex = 0
        cmbTicketsAx.SelectedIndex = 0
        cmbUsersAx.SelectedIndex = 0
    End Sub

    Private Sub CaptureSignatureAx_FormClosing(sender As Object, e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        Try
            With signatureAx
                .TabletState = 1
                .ClearTablet()
                .LCDRefresh(0, 0, 0, 240, 64)
            End With
        Catch ex As Exception

        End Try
    End Sub

    Private Sub cmbComPort_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles cmbComPortAx.SelectedIndexChanged
        signatureAx.TabletState = 0
        _port = CType(cmbComPortAx.SelectedItem, SignaturePadConnection).Port
        If Not GetSignature() Then
            pnlNoResponseAx.Visible = True
            lblNoResponse.Visible = True
        ElseIf cmbComPortAx.SelectedIndex = 0 Then
            pnlNoResponseAx.Visible = True
            lblNoResponse.Visible = True
        Else
            pnlNoResponseAx.Visible = False
            lblNoResponse.Visible = False
        End If
    End Sub

    Private Sub btnClear_Click(sender As System.Object, e As System.EventArgs) Handles btnClearAx.Click
        If cmbComPortAx.SelectedIndex > 0 And signatureAx.TabletState <> 0 Then
            pnlNoResponseAx.Visible = Not GetSignature()
        End If
    End Sub

    Private Sub btnSaveAsFile_Click(sender As System.Object, e As System.EventArgs) Handles btnSaveAsFileAx.Click
        If cmbComPortAx.SelectedIndex > 0 And signatureAx.TabletState <> 0 Then
            With SaveFileDialog1
                Common.SaveSignatureAsFile(SaveFileDialog1)
                If .ShowDialog() = Windows.Forms.DialogResult.OK Then
                    Try
                        GetImage(SaveFileDialog1.FileName)
                        Close()
                    Catch ex As Exception
                        MessageBox.Show(ex.ToString)
                        cmbComPort_SelectedIndexChanged(cmbComPortAx, New EventArgs)
                    End Try
                End If
            End With
        End If
    End Sub

    Private Sub btnSaveToUser_Click(sender As System.Object, e As System.EventArgs) Handles btnSaveToUserAx.Click
        If cmbUsersAx.SelectedIndex > 0 And cmbComPortAx.SelectedIndex > 0 And signatureAx.TabletState <> 0 Then
            Try
                Common.SaveSignatureToUser(CType(cmbUsersAx.SelectedItem, KaCommonObjects.ComboBoxItem).Value, GetImage())
                Close()
            Catch ex As Exception
                MessageBox.Show(ex.ToString)
                cmbComPort_SelectedIndexChanged(cmbComPortAx, New EventArgs)
            End Try
        End If
    End Sub

    Private Sub btnSaveToTicket_Click(sender As System.Object, e As System.EventArgs) Handles btnSaveToTicketAx.Click
        If cmbTicketsAx.SelectedIndex > 0 And (rbDriverSignatureAx.Checked Or rbUserSignatureAx.Checked) And cmbComPortAx.SelectedIndex > 0 And signatureAx.TabletState() <> 0 Then
            Try
                Common.SaveSignatureToTicket(GetImage(), CType(cmbTicketsAx.SelectedItem, KaCommonObjects.ComboBoxItem).Value, rbDriverSignatureAx)
                Close()
            Catch ex As Exception
                MessageBox.Show(ex.ToString)
                cmbComPort_SelectedIndexChanged(cmbComPortAx, New EventArgs)
            End Try
        End If
    End Sub

    Private Function SignaturePadConnected() As Boolean
        If _port = -3 Then
            signatureAx.TabletComPort = 1
            signatureAx.TabletType = 6
        ElseIf _port = -2 Then
            signatureAx.TabletComPort = 1
            signatureAx.TabletType = 2
        Else ' serial port
            signatureAx.TabletComPort = _message
            signatureAx.TabletType = 0
        End If
        signatureAx.TabletComTest = 1
        signatureAx.TabletState = 1
        Dim connected As Boolean = signatureAx.TabletState() <> 0
        signatureAx.TabletComTest = 0
        Return connected
    End Function

    Public Function GetSignature() As Boolean
        If SignaturePadConnected() Then
            With signatureAx
                .TabletState = 1
                .ClearTablet()
                .LCDRefresh(0, 0, 0, 240, 64)
                .LCDSetTabletMap(0, 240, 64, 100, 0, 1900, 700)
                .TabletXStart = 400
                .TabletXStop = 2400
                .TabletYStart = 350
                .TabletYStop = 1050
                .TabletLogicalXSize = 2000
                .TabletLogicalYSize = 700
                .LCDRefresh(0, 0, 0, 240, 64)
                .LCDCaptureMode = 2
                .LCDSetFont(10, 0, 550, 0, 0, 0, "Courier")
                .LCDWriteString(0, 2, 55, 4, 0, 0, 0, "Please sign")
                .LCDWriteBitmap(0, 2, 0, 20, 238, 43, My.Resources.signaturebox.GetHbitmap().ToInt32())
                .LCDSetWindow(0, 22, 240, 40) ' set signature area
                .SetSigWindow(1, 0, 22, 240, 40)
                .SetEventEnableMask(3)
                Return True
            End With
        Else
            Return False
        End If
    End Function

    Public Function GetImage() As Image
        Return GetImage(Path.Combine(My.Application.Info.DirectoryPath, String.Format("tm2_signature_{0:yyyyMMddHHmmssff}.jpg", DateTime.Now))) ' TODO signature path needs to be matching at this point 
    End Function

    Public Function GetImage(path As String) As Image
        Try
            If Not pnlNoResponseAx.Visible Then
                signatureAx.ImageFileFormat = 4
                signatureAx.ImageXSize = 300
                signatureAx.ImageYSize = 80
                signatureAx.WriteImageFile(path)
                signatureAx.LCDRefresh(0, 0, 0, 240, 64)
                signatureAx.LCDSetFont(13, 0, 800, 0, 0, 0, "Arial")
                signatureAx.LCDWriteString(0, 2, 15, 25, 0, 0, 0, "Thank you")
                signatureAx.KeyPadClearHotSpotList()
                signatureAx.SetSigWindow(1, 0, 0, 0, 0)
                signatureAx.LCDSetWindow(0, 0, 0, 0)
                Dim retVal As Image = Image.FromFile(path)
                Try
                    IO.File.Delete(path)
                Catch ex As Exception

                End Try
                Return retVal
            End If
        Catch pnlNoResponseNotVisible As Exception
            MessageBox.Show(pnlNoResponseNotVisible.ToString())
        End Try
        Return New Bitmap(1, 1)
    End Function
End Class