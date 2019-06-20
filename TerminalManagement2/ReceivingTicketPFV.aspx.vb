Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class ReceivingTicketPFV : Inherits System.Web.UI.Page

    Private _currentUser As KaUser

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Request.QueryString("receiving_ticket_id") <> Nothing Then
            _currentUser = Utilities.GetUser(Me)

            Dim receivingTicketId As Guid = Guid.Parse(Request.QueryString("receiving_ticket_id"))
            Dim receivingTicket As KaReceivingTicket = New KaReceivingTicket(GetUserConnection(_currentUser.Id), receivingTicketId)
            Dim htmlAddress As String = Receiving.GetReceivingTicketHtmlAddress(receivingTicket, _currentUser)
            If htmlAddress.Length > 0 Then
                Response.Redirect(htmlAddress)
            Else
                'Send string to the literal for display
                litOutput.Text = receivingTicket.ReceiptTicketText(GetUserConnection(_currentUser.Id)).Replace(Environment.NewLine, "<br>")
            End If
        End If
    End Sub
End Class