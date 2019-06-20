Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb
Imports System.IO
Imports System.Net.Mail

Public Class Emails : Inherits System.Web.UI.Page
	Private _currentUser As KaUser
	Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
	Private _currentTableName As String = KaEmail.TABLE_NAME
#Region "Events"
	Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
		Response.Cache.SetCacheability(HttpCacheability.NoCache)
		_currentUser = Utilities.GetUser(Me)
		_currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Emails")
		If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")
		If Not Page.IsPostBack Then
			SetTextboxMaxLengths()
			PopulateEmails(Integer.Parse(ddlMaxMessageAge.SelectedValue), Boolean.Parse(ddlShowSentMessages.SelectedValue))
		End If
		Utilities.ConfirmBox(btnDeleteAttachment, "Are you sure you want to delete this attachment?")
	End Sub

	Protected Sub ddlMaxMessageAge_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlMaxMessageAge.SelectedIndexChanged
		PopulateEmails(Integer.Parse(ddlMaxMessageAge.SelectedValue), Boolean.Parse(ddlShowSentMessages.SelectedValue))
	End Sub

	Protected Sub ddlShowSentMessages_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlShowSentMessages.SelectedIndexChanged
		PopulateEmails(Integer.Parse(ddlMaxMessageAge.SelectedValue), Boolean.Parse(ddlShowSentMessages.SelectedValue))
	End Sub

	Protected Sub lstMessages_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lstMessages.SelectedIndexChanged
		If lstMessages.SelectedIndex >= 0 Then
			Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
			Dim emailToDisplay As New KaEmail(connection, Guid.Parse(lstMessages.SelectedValue))
			cbxDeleted.Disabled = False
			cbxDeleted.Checked = emailToDisplay.Deleted
			tbxSubject.Enabled = True
			tbxSubject.Text = emailToDisplay.Subject
			tbxRecipients.Enabled = True
			tbxRecipients.Text = emailToDisplay.Recipients
			lstAttachments.Enabled = True
			lstAttachments.Items.Clear()
			For Each attachment As Attachment In emailToDisplay.DeserializeAttachments()
				lstAttachments.Items.Add(New ListItem(attachment.Name))
			Next
			frmEmailBody.Attributes("src") = "DisplayContent.aspx?type=emails&id=" & lstMessages.SelectedValue
			frmEmailBody.Visible = True
			btnSave.Enabled = True
		Else
			cbxDeleted.Disabled = True
			cbxDeleted.Checked = False
			tbxSubject.Enabled = False
			tbxSubject.Text = ""
			tbxRecipients.Enabled = False
			tbxRecipients.Text = ""
			lstAttachments.Enabled = False
			lstAttachments.Items.Clear()
			frmEmailBody.Visible = False
			btnSave.Enabled = False
		End If
		lstAttachments_SelectedIndexChanged(Nothing, Nothing)
	End Sub

	Protected Sub btnOpenAttachment_Click(sender As Object, e As EventArgs) Handles btnOpenAttachment.Click
		If lstMessages.SelectedIndex >= 0 AndAlso lstAttachments.SelectedIndex >= 0 Then
			Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
			Dim emailToOpen As New KaEmail(connection, Guid.Parse(lstMessages.SelectedValue))
			For Each attachment As Attachment In emailToOpen.DeserializeAttachments()
				If attachment.Name = lstAttachments.SelectedItem.Text Then
					If attachment.ContentType.MediaType.ToLower() = "text/html" Then
						DisplayJavaScriptMessage("DisplayContents", Utilities.JsWindowOpen("DisplayContent.aspx?type=email_attachments&id=" & lstMessages.SelectedValue & "&name=" & lstAttachments.SelectedValue))
					Else
						Try
							Dim fileName As String = String.Format("{0:yyyyMMddHHmmss}_{1}", Now, attachment.Name)
							Dim fileStream As New FileStream(DownloadDirectory(Me) & "\" & fileName, FileMode.Create)
							attachment.ContentStream.Seek(0, SeekOrigin.Begin)
							Dim b As Integer = attachment.ContentStream.ReadByte()
							Do While b <> -1
								fileStream.WriteByte(CType(b, Byte))
								b = attachment.ContentStream.ReadByte()
							Loop
							fileStream.Flush()
							fileStream.Close()
							DisplayJavaScriptMessage("DownloadReport", Utilities.JsWindowOpen("./download/" & fileName))
						Catch ex As Exception
							DisplayJavaScriptMessage("AttachmentOpenError", Utilities.JsAlert("Could not open attachment: " & ex.Message))
						End Try
					End If
					Exit For
				End If
			Next
		End If
	End Sub

	Protected Sub btnDeleteAttachment_Click(sender As Object, e As EventArgs) Handles btnDeleteAttachment.Click
		If lstMessages.SelectedIndex >= 0 AndAlso lstAttachments.SelectedIndex >= 0 Then
			lstAttachments.Items.Remove(lstAttachments.SelectedItem)
		End If
	End Sub

	Protected Sub lstAttachments_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lstAttachments.SelectedIndexChanged
		If lstAttachments.SelectedIndex >= 0 Then
			btnOpenAttachment.Enabled = True
			btnDeleteAttachment.Enabled = True
		Else
			btnOpenAttachment.Enabled = False
			btnDeleteAttachment.Enabled = False
		End If
	End Sub

	Protected Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
		Dim user As KaUser = Utilities.GetUser(Me)
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim emailToSave As New KaEmail(connection, Guid.Parse(lstMessages.SelectedValue))
		Dim attachments As List(Of Attachment) = emailToSave.DeserializeAttachments()
		emailToSave.Deleted = cbxDeleted.Checked
		emailToSave.Subject = tbxSubject.Text
		emailToSave.Recipients = tbxRecipients.Text
		Dim i As Integer = 0
		Do While i < attachments.Count
			Dim found As Boolean = False
			For Each item As ListItem In lstAttachments.Items
				If item.Text = attachments(i).Name Then
					found = True
					Exit For
				End If
			Next
			If found Then i += 1 Else attachments.RemoveAt(i)
		Loop
		emailToSave.SerializeAttachments(attachments)
		emailToSave.SqlUpdate(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
		PopulateEmails(Integer.Parse(ddlMaxMessageAge.SelectedValue), Boolean.Parse(ddlShowSentMessages.SelectedValue))
	End Sub
#End Region

	Private Sub PopulateEmails(maximumAge As Integer, showDeleted As Boolean)
		Dim selected As String
		If lstMessages.SelectedIndex >= 0 Then selected = lstMessages.SelectedValue Else selected = ""
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		lstMessages.Items.Clear()
		Dim conditions As String = "((sent_at <>" & Q(New Date(1753, 1, 1)) & " AND sent_at>" & Q(Now.Subtract(New TimeSpan(maximumAge, 0, 0, 0))) & ")" &
			 " OR (sent_at =" & Q(New Date(1753, 1, 1)) & " AND created>" & Q(Now.Subtract(New TimeSpan(maximumAge, 0, 0, 0))) & "))" &
			IIf(showDeleted, "", " AND " & KaEmail.FN_DELETED & "=0") &
			IIf(_currentUser.OwnerId = Guid.Empty, "", " AND (" & KaEmail.FN_OWNER_ID & "=" & Q(_currentUser.OwnerId) & " OR " & KaEmail.FN_OWNER_ID & "=" & Q(Guid.Empty) & ")")
		For Each emailToDisplay As KaEmail In KaEmail.GetAll(connection, conditions, KaEmail.FN_SENT_AT & " DESC, " & KaEmail.FN_CREATED & " DESC")
			If emailToDisplay.SentAt > New Date(1900, 1, 1) Then
				lstMessages.Items.Add(New ListItem(emailToDisplay.SentAt.ToString() & String.Format(": {0} sent to {1}", emailToDisplay.Subject, emailToDisplay.Recipients), emailToDisplay.Id.ToString()))
			Else
				lstMessages.Items.Add(New ListItem(String.Format("{0} pending for {1}", emailToDisplay.Subject, emailToDisplay.Recipients), emailToDisplay.Id.ToString()))
			End If
			If emailToDisplay.Id.ToString() = selected Then
				lstMessages.SelectedIndex = lstMessages.Items.Count - 1
			End If
		Next
		lstMessages_SelectedIndexChanged(Nothing, Nothing)
	End Sub

	Private Sub SetTextboxMaxLengths()
		tbxRecipients.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaEmail.TABLE_NAME, KaEmail.FN_RECIPIENTS))
		tbxSubject.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaEmail.TABLE_NAME, KaEmail.FN_SUBJECT))
	End Sub

	Protected Sub ToolkitScriptManager1_AsyncPostBackError(ByVal sender As Object, ByVal e As System.Web.UI.AsyncPostBackErrorEventArgs)
		ToolkitScriptManager1.AsyncPostBackErrorMessage = e.Exception.Message
	End Sub

	Private Sub DisplayJavaScriptMessage(key As String, script As String)
		ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), key, script, False)
	End Sub
End Class