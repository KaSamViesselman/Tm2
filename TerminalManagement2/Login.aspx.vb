Imports KahlerAutomation.KaTm2Database
Imports System.Web.Security
Imports System.Reflection
Imports System.Data.OleDb

Public Class Login
	Inherits System.Web.UI.Page

#Region " Web Form Designer Generated Code "

	'This call is required by the Web Form Designer.
	<System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()

	End Sub

	Protected WithEvents tbxUserName As System.Web.UI.WebControls.TextBox
	Protected WithEvents tbxPassword As System.Web.UI.WebControls.TextBox
	Protected WithEvents btnSubmit As System.Web.UI.WebControls.Button
	Protected WithEvents lblVersion As System.Web.UI.WebControls.Label
	Protected WithEvents lblWarning As System.Web.UI.WebControls.Label
	Protected WithEvents lblDatabaseVersion As System.Web.UI.WebControls.Label
	Protected WithEvents lblCopyright As System.Web.UI.WebControls.Label
	Protected WithEvents pnlActivationWarning As System.Web.UI.HtmlControls.HtmlGenericControl
	Protected WithEvents pnlActivation As System.Web.UI.HtmlControls.HtmlGenericControl
	Protected WithEvents pnlLogin As System.Web.UI.HtmlControls.HtmlGenericControl
	Protected WithEvents lblActivationWarning As System.Web.UI.WebControls.Label
	Protected WithEvents btnAuthorization As System.Web.UI.WebControls.Button

	Private Sub Page_Init(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Init
		'CODEGEN: This method call is required by the Web Form Designer
		'Do not modify it using the code editor.
		InitializeComponent()
	End Sub

#End Region

	Protected Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load, Me.Load
		lblActivationWarning.Text = ""
		CheckActivation()
		If Utilities.UseWindowsAuthentication() Then Response.Redirect("Welcome.aspx") ' The user is technically already logged in.
		If Request.QueryString("new_session") = "true" Then FormsAuthentication.SignOut()
		Utilities.SetFocus(tbxUserName, Me) ' Set focus to the username textbox for easy entry
		Response.Cache.SetCacheability(HttpCacheability.NoCache)
		SetApplicationVersions()
	End Sub

	Private Sub SetApplicationVersions()
        lblVersion.Text = Common.ApplicationVersion()

        Dim attributes As Object()
		Try
            attributes = Common.ApplicationAssembly().GetCustomAttributes(GetType(AssemblyCopyrightAttribute), False)
            If (attributes.Length <> 0) Then
				lblCopyright.Text = CType(attributes(0), AssemblyCopyrightAttribute).Copyright
			End If
		Catch ex As Exception ' Suppress
		End Try

		Dim tm2DbVersion As Integer = Tm2Database.GetDatabaseVersion(Tm2Database.Connection, Nothing)
		lblDatabaseVersion.Text = $"Database Version {tm2DbVersion}"
		lblDatabaseVersion.Visible = (tm2DbVersion > 0)
	End Sub

	Protected Sub btnSubmit_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSubmit.Click
		Dim userList As ArrayList = KaUser.GetAll(Tm2Database.Connection, "deleted=0 AND username=" & Q(tbxUserName.Text), "")
		For Each u As KaUser In userList
			If u.Password = Utilities.StripSqlInjection(tbxPassword.Text) Then
				If Not u.Disabled Then
					Dim connection As OleDbConnection = New OleDbConnection(Tm2Database.GetDbConnection())
					connection.Open()
					AddUserConnection(u.Id, connection)

					FormsAuthentication.SetAuthCookie(u.Id.ToString, False)
					If Page.Request.Params("ReturnUrl") IsNot Nothing Then
						Response.Redirect(Page.Request.Params("ReturnUrl"))
					Else
						Response.Redirect("Welcome.aspx")
					End If
					Exit Sub
				Else
					lblWarning.Visible = True
					lblWarning.Text = "User disabled"
					Exit Sub
				End If
			End If
		Next
		lblWarning.Visible = True
		lblWarning.Text = "Incorrect username or password"
		Utilities.SetFocus(tbxUserName, Me)
	End Sub

#Region " Activation "
	Private Sub CheckActivation()
		pnlLogin.Visible = False
		pnlActivationWarning.Visible = False

        If (Common.CheckAuthorized(True, AddressOf LicenseAlmostExpiredNotification, AddressOf LicenseInTrialModeNotification, AddressOf LicenseNotActivatedNotification, True)) Then
            pnlLogin.Visible = True
        End If

        pnlActivation.Visible = pnlActivationWarning.Visible
	End Sub

	Private Sub LicenseNotActivatedNotification(message As String)
		lblActivationWarning.Text = message
		pnlActivationWarning.Visible = True
		pnlLogin.Visible = False
	End Sub

	Private Sub LicenseAlmostExpiredNotification(daysLeft As Integer)
		lblActivationWarning.Text = "Warning: " & daysLeft.ToString() & " day" + IIf(daysLeft = 1, " ", "s ") & "remaining before license expires"
		pnlActivationWarning.Visible = True
		pnlLogin.Visible = True
	End Sub

	Private Sub LicenseInTrialModeNotification(daysLeft As Integer)
		lblActivationWarning.Text = "Running in trial mode. " & daysLeft.ToString() & " day" + IIf(daysLeft = 1, " ", "s ") & "remaining in trial cycle."
		lblActivationWarning.Text &= "<br /><br />Please contact customer support for assistance with activating the license"
		pnlActivationWarning.Visible = True
		pnlLogin.Visible = True
	End Sub

	Protected Sub btnAuthorization_Click(sender As Object, e As EventArgs) Handles btnAuthorization.Click
		Response.Redirect("About.aspx")
	End Sub
#End Region
End Class
