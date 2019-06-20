Imports KahlerAutomation.KaTm2Database
Imports System.Drawing
Imports System.IO.Ports
Imports System.Data.OleDb

Module Common
     Public Sub SaveSignatureToUser(userId As Guid, signatureImage As Image)
        Dim user As New KaUser(Tm2Database.Connection, userId)
        user.SignatureImage = signatureImage
        user.SqlUpdate(Tm2Database.Connection, Nothing, System.Net.Dns.GetHostName() & "/" & My.Application.Info.ProductName, "")
    End Sub

    Public Sub SaveSignatureAsFile(saveFileDialog As SaveFileDialog)
        With saveFileDialog
            .DefaultExt = "jpg"
            .FileName = "UserSignature"
            .Filter = "JPEG|*.jpg"
            .InitialDirectory = My.Application.Info.DirectoryPath
            .Title = "Save signature as"
            .CheckPathExists = True
            .CheckFileExists = False
        End With
    End Sub

    Public Sub SaveSignatureToTicket(signatureImage As Image, userId As Guid, rbDriver As RadioButton)
        Dim ticket As New KaTicket(Tm2Database.Connection, userId)
        If rbDriver.Checked Then
            ticket.DriverSignatureImage = signatureImage
        Else
            ticket.UserSignatureImage = signatureImage
        End If
        ticket.SqlUpdate(Tm2Database.Connection, Nothing, System.Net.Dns.GetHostName() & "/" & My.Application.Info.ProductName, "")
    End Sub

    Public Sub PopulateComPorts(cmbComPort As ComboBox)
        With cmbComPort.Items
            .Clear()
            .Add(New SignaturePadConnection("None", -1))
            .Add(New SignaturePadConnection("USB (HSB)", -3))
            For Each comPort As String In SerialPort.GetPortNames()
                If comPort.Substring(0, 3).ToLower() = "com" AndAlso comPort.Length > 3 Then
                    Dim port As Integer = 0
                    If Integer.TryParse(comPort.Substring(3, comPort.Length - 3), port) Then
                        .Add(New SignaturePadConnection(comPort, port))
                    End If
                End If
            Next
        End With
        'cmbComPort.SelectedIndex = 0
    End Sub

    Public Sub PopulateUsers(cmbUsers As ComboBox)
        With cmbUsers.Items
            .Clear()
            .Add(New KaCommonObjects.ComboBoxItem("Select User", Guid.Empty))

            For Each user As KaUser In KaUser.GetAll(Tm2Database.Connection, "deleted=0", "name")
                .Add(New KaCommonObjects.ComboBoxItem(user.Name, user.Id))
            Next
        End With
    End Sub

    Public Sub PopulateTickets(cmbTickets As ComboBox)
        With cmbTickets.Items
            .Clear()
            .Add(New KaCommonObjects.ComboBoxItem("Select Ticket", Guid.Empty))

            'For Each ticket As KaTicket In KaTicket.GetAll(Tm2Database.Connection, "", "number")
            '    .Add(New KaCommonObjects.ComboBoxItem(ticket.Number, ticket.Id))
            'Next

            Dim getTicketsCmd As New OleDbCommand("SELECT id, number " & _
                                                  "FROM tickets " & _
                                                  "ORDER BY number", Tm2Database.Connection)
            Dim getTicketsRdr As OleDbDataReader = getTicketsCmd.ExecuteReader
            Do While getTicketsRdr.Read()
                .Add(New KaCommonObjects.ComboBoxItem(getTicketsRdr.Item("number"), getTicketsRdr.Item("id")))
            Loop

        End With
    End Sub

    Public Class SignaturePadConnection
        Public Sub New(name As String, port As Integer)
            _name = name
            _port = port
        End Sub

        Private _name As String
        Public ReadOnly Property Name As String
            Get
                Return _name
            End Get
        End Property
        Private _port As Integer
        Public ReadOnly Property Port() As Integer
            Get
                Return _port
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return _name
        End Function
    End Class
End Module
