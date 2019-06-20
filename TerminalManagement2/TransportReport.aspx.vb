Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class TransportReport : Inherits System.Web.UI.Page
	Private _currentUser As KaUser
	Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing

	Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
		Response.Cache.SetCacheability(HttpCacheability.NoCache)
		_currentUser = Utilities.GetUser(Me)
		_currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaTransport.TABLE_NAME}), "Transports")
		If Not _currentUserPermission(KaTransport.TABLE_NAME).Read Then Response.Redirect("Welcome.aspx")

		DisplayResults()
		If Not Page.IsPostBack Then PopulateEmailAddressList()
		litEmailConfirmation.Text = ""
	End Sub

	Private Sub DisplayResults()
		litOutput.Text = KaReports.GetTableHtml("Transports in Facility", GetTransportsInFacilityTable(GetUserConnection(_currentUser.Id)))
	End Sub

#Region " E-mail report "
	Private Sub PopulateEmailAddressList()
		Utilities.PopulateEmailAddressList(tbxEmailTo, ddlAddEmailAddress, btnAddEmailAddress)
		rowAddAddress.Visible = ddlAddEmailAddress.Items.Count > 1
	End Sub

	Protected Sub btnAddEmailAddress_Click(sender As Object, e As EventArgs) Handles btnAddEmailAddress.Click
		If ddlAddEmailAddress.SelectedIndex > 0 Then
			If tbxEmailTo.Text.Trim.Length > 0 Then tbxEmailTo.Text &= ", "
			tbxEmailTo.Text &= ddlAddEmailAddress.SelectedValue
			PopulateEmailAddressList()
		End If
	End Sub

	Private Sub tbxEmailTo_TextChanged(sender As Object, e As System.EventArgs) Handles tbxEmailTo.TextChanged
		PopulateEmailAddressList()
	End Sub

	Protected Sub btnSendEmail_Click(sender As Object, e As EventArgs) Handles btnSendEmail.Click
		If tbxEmailTo.Text.Trim().Length > 0 Then
			Dim message As String = ""
			If Not Utilities.IsEmailFieldValid(tbxEmailTo.Text, message) Then
				ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidEmail", Utilities.JsAlert(message))
				Exit Sub
			End If
			Dim header As String = "Transports in Facility"
			Dim body As String = KaReports.GetTableHtml(header, GetTransportsInFacilityTable(GetUserConnection(_currentUser.Id)))

			Dim emailAddresses As String = ""
			Dim emailTo() As String = tbxEmailTo.Text.Split(New Char() {";", ","})
			For Each emailRecipient As String In emailTo
				If emailRecipient.Trim.Length > 0 Then
					Dim newEmail As New KaEmail()
					newEmail.ApplicationId = APPLICATION_ID
					newEmail.Body = Utilities.CreateSiteCssStyle() & body
					newEmail.BodyIsHtml = True
					newEmail.OwnerID = _currentUser.OwnerId
					newEmail.Recipients = emailRecipient.Trim
					newEmail.ReportType = KaEmailReport.ReportTypes.Generic
					newEmail.Subject = header
					Dim attachments As New List(Of System.Net.Mail.Attachment)
					attachments.Add(New System.Net.Mail.Attachment(New IO.MemoryStream(Encoding.UTF8.GetBytes(newEmail.Body)), "TransportsInFacility.html", System.Net.Mime.MediaTypeNames.Text.Html))
					newEmail.SerializeAttachments(attachments)
					KaEmail.CreateEmail(GetUserConnection(_currentUser.Id), newEmail, -1, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
					If emailAddresses.Length > 0 Then emailAddresses &= ", "
					emailAddresses &= newEmail.Recipients
				End If
			Next
			If emailAddresses.Length > 0 Then
				litEmailConfirmation.Text = "Report sent to " & emailAddresses
			Else
				litEmailConfirmation.Text = "Report not sent.  No e-mail addresses."
			End If
		End If
	End Sub
#End Region
End Class