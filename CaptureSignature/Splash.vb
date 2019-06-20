Imports System.Reflection
Imports System.Timers


Public Class Splash
    Private _done As Boolean = False

    Private Sub tmrDelay_Tick(sender As System.Object, e As System.EventArgs) Handles tmrDelay.Tick
        tmrDelay.Enabled = False
        If Not _done = True Then
            Try
                Dim f As New CaptureSignatureAx()
                f.Show()
            Catch ex As System.Runtime.InteropServices.COMException
                ' This will fail if the ActiveX control is not registered
                Dim f As New CaptureSignature()
                f.Show()
            End Try
            _done = True
            Close()
        End If
    End Sub

End Class