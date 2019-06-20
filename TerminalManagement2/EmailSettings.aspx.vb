Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb
Imports System.Net.Mail

Public Class EmailSettings : Inherits System.Web.UI.Page

    Private _currentUser As KaUser
    Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
    Private _currentTableName As String = KaSetting.TABLE_NAME

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Response.Cache.SetCacheability(HttpCacheability.NoCache)
        _currentUser = Utilities.GetUser(Me)
        _currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "GeneralSettings")
        If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")
        btnSave.Enabled = _currentUserPermission(_currentTableName).Edit
        btnTestEmail.Enabled = btnSave.Enabled
        If Not Page.IsPostBack Then
            DisplaySettings()
        End If
    End Sub

    Private Sub DisplaySettings()
        Dim c As OleDbConnection = GetUserConnection(_currentUser.Id)
        tbxServerEmail.Text = KaSetting.GetSetting(c, KaSetting.SN_EMAIL_ADDRESS, "")
        tbxServerSmtp.Text = KaSetting.GetSetting(c, KaSetting.SN_SMTP_URL, "")
        tbxUsername.Text = KaSetting.GetSetting(c, KaSetting.SN_EMAIL_USERNAME, "")
        tbxEmailServerPort.Text = KaSetting.GetSetting(c, KaSetting.SN_SMTP_PORT, KaSetting.SD_SMTP_PORT)
        cbxEmailServerUseSsl.Checked = Boolean.Parse(KaSetting.GetSetting(c, KaSetting.SN_EMAIL_SERVER_USE_SSL, KaSetting.SD_EMAIL_SERVER_USE_SSL))
        tbxDaysToKeepEmailRecords.Text = KaSetting.GetSetting(c, KaSetting.SN_DAYS_TO_KEEP_EMAIL_RECORDS, KaSetting.SD_DAYS_TO_KEEP_EMAIL_RECORDS)
        cbxMarkEmailsWithNoRecipientAsSent.Checked = Boolean.Parse(KaSetting.GetSetting(c, "General/MarkEmailsWithNoRecipientAsSent", True))
    End Sub

    Private Sub btnSave_Click(sender As Object, e As System.EventArgs) Handles btnSave.Click
        ' validate settings
        If Not IsNumeric(tbxEmailServerPort.Text) Then
            ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidSMTPServerPort", Utilities.JsAlert("Please enter a numeric value for the outgoing e-mail server port setting."))
            Exit Sub
        End If
        If Not IsNumeric(tbxDaysToKeepEmailRecords.Text.Trim) Then
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidEmailsMaxDays", Utilities.JsAlert("Please enter a numeric value for days to keep e-mail records setting."))
			Exit Sub
        End If
        Dim c As OleDbConnection = GetUserConnection(_currentUser.Id)
        KaSetting.WriteSetting(c, KaSetting.SN_EMAIL_ADDRESS, tbxServerEmail.Text)
        KaSetting.WriteSetting(c, KaSetting.SN_SMTP_URL, tbxServerSmtp.Text)
        KaSetting.WriteSetting(c, KaSetting.SN_EMAIL_USERNAME, tbxUsername.Text)
        KaSetting.WriteSetting(c, KaSetting.SN_EMAIL_PASSWORD, tbxPassword.Text)
        KaSetting.WriteSetting(c, KaSetting.SN_SMTP_PORT, tbxEmailServerPort.Text)
        KaSetting.WriteSetting(c, KaSetting.SN_EMAIL_SERVER_USE_SSL, cbxEmailServerUseSsl.Checked.ToString().ToLower())
        KaSetting.WriteSetting(c, KaSetting.SN_DAYS_TO_KEEP_EMAIL_RECORDS, tbxDaysToKeepEmailRecords.Text.Trim)
        KaSetting.WriteSetting(c, "General/MarkEmailsWithNoRecipientAsSent", cbxMarkEmailsWithNoRecipientAsSent.Checked.ToString().ToLower())

        lblSave.Visible = True
    End Sub

    Private Sub Page_PreRenderComplete(sender As Object, e As System.EventArgs) Handles Me.PreRenderComplete
        tbxPassword.Attributes("value") = KaSetting.GetSetting(GetUserConnection(_currentUser.Id), KaSetting.SN_EMAIL_PASSWORD, "")
    End Sub

    Protected Sub btnTestEmail_Click(sender As Object, e As EventArgs) Handles btnTestEmail.Click
        lblTestEmail.Text = ""
        If CanSendTestEmail() Then
            Try
                Dim client As New SmtpClient(tbxServerSmtp.Text.Trim, tbxEmailServerPort.Text.Trim)
                client.EnableSsl = cbxEmailServerUseSsl.Checked
                Dim username As String = tbxUsername.Text.Trim
                Dim password As String = tbxPassword.Text.Trim
				If username.Length > 0 Then
					client.UseDefaultCredentials = False
					client.Credentials = New System.Net.NetworkCredential(username, password)
				End If
				Dim m As New MailMessage(tbxServerEmail.Text.Trim, tbxTestEmailAddress.Text.Trim.Replace(";", ","), "Test E-mail From Kahler Software", "This is a test e-mail from the Kahler Automation Terminal Management 2 software package.")
				client.Send(m)
				lblTestEmail.Text = "E-mail sent.  Please verify it was received.  E-mail may take a couple of minutes to arrive."
			Catch ex As Exception
                lblTestEmail.Text = ex.Message
            End Try
        End If
    End Sub

    Private Function CanSendTestEmail() As Boolean
        If tbxServerEmail.Text.Trim.Length = 0 Then
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "MissingServerEmail", Utilities.JsAlert("Please specify a server e-mail address."))
			Return False
        End If
        If tbxServerSmtp.Text.Trim.Length = 0 Then
            ClientScript.RegisterClientScriptBlock(Me.GetType(), "MissingServerSMTP", Utilities.JsAlert("Please specify a SMTP address."))
            Return False
        End If
        If tbxTestEmailAddress.Text.Trim.Length = 0 Then
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "MissingTestEmail", Utilities.JsAlert("Please specify an e-mail address to send the test e-mail to."))
			Return False
        End If

        Return True
    End Function
End Class