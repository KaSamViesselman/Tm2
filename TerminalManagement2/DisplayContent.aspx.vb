Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb
Imports System.Net.Mail
Imports System.IO
Public Class DisplayContent : Inherits System.Web.UI.Page
#Region "Events"
	Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
		Dim connection As OleDbConnection = GetUserConnection(Utilities.GetUser(User).Id)
		Dim type As String = Request.QueryString("type")
		Select Case type
			Case "emails" ' display the e-mail's body
				Dim id As Guid = Guid.Parse(Request.QueryString("id"))
				Dim emailToDisplay As New KaEmail(connection, id)
				If emailToDisplay.BodyIsHtml Then ' body is already HTML
					litBody.Text = emailToDisplay.Body
				Else ' convert body to HTML
					litBody.Text = EncodeForWebBrowser(emailToDisplay.Body)
				End If
			Case "email_attachments" ' display the e-mail attachment
				Dim id As Guid = Guid.Parse(Request.QueryString("id"))
				Dim emailToDisplay As New KaEmail(connection, id)
				Dim name As String = Request.QueryString("name")
				litBody.Text = ""
				For Each attachment As Attachment In emailToDisplay.DeserializeAttachments()
					If attachment.Name = name Then
						attachment.ContentStream.Seek(0, SeekOrigin.Begin)
						Dim reader As New StreamReader(attachment.ContentStream)
						While Not reader.EndOfStream
							If attachment.ContentType.MediaType = "text/html" Then
								litBody.Text &= reader.ReadLine()
							Else
								litBody.Text &= EncodeForWebBrowser(reader.ReadLine()) & "<br />"
							End If
						End While
					End If
				Next
			Case Else ' caller didn't specify what they wanted
				litBody.Text = ""
		End Select
	End Sub
#End Region

	Private Function EncodeForWebBrowser(body As String) As String
		Return body.Replace("'", "&apos;").Replace(vbCrLf, "<br />").Replace(vbCr, "<br />").Replace(vbLf, "<br />")
	End Function
End Class