Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class EventLog : Inherits System.Web.UI.Page
	Private _currentUser As KaUser
	Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
	Private _currentTableName As String = "reports"

	Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
		_currentUser = Utilities.GetUser(Me)
		_currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "reports")
		If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")

		If Not Page.IsPostBack Then
			PopulateApplicationList()
			PopulateCategoryList()
			PopulateComputerList()
			tbxFrom.Value = String.Format("{0:g}", New DateTime(Now.Year, Now.Month, Now.Day, 0, 0, 0))
			tbxTo.Value = String.Format("{0:g}", New DateTime(Now.Year, Now.Month, Now.Day, 23, 59, 59))
			PopulateEmailAddressList()
		End If
		litEmailConfirmation.Text = ""
	End Sub

	Protected Sub btnShowLogEntries_Click(sender As Object, e As EventArgs) Handles btnShowLogEntries.Click
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim conditions As String = String.Format("created>={0} AND created<={1}", Q(DateTime.Parse(tbxFrom.Value)), Q(DateTime.Parse(tbxTo.Value)))
		If ddlApplication.SelectedIndex > 0 Then conditions &= String.Format(" AND application_identifier={0}", Q(ddlApplication.Text))
		If ddlCategory.SelectedIndex > 0 Then conditions &= String.Format(" AND category={0}", ddlCategory.SelectedValue)
		If ddlComputer.SelectedIndex > 0 Then conditions &= String.Format(" AND computer={0}", Q(ddlComputer.Text))
		Dim row As TableRow = logEntries.Rows(0)
		logEntries.Rows.Clear()
		logEntries.Rows.Add(row)
		For Each entry As KaEventLog In KaEventLog.GetAll(connection, conditions, "created ASC")
			row = New TableRow()
			row.Cells.AddRange({
				New TableCell() With {.Text = String.Format("{0:g}", entry.Created)},
				New TableCell() With {.Text = entry.Computer},
				New TableCell() With {.Text = entry.ApplicationIdentifier},
				New TableCell() With {.Text = entry.ApplicationVersion},
				New TableCell() With {.Text = entry.Category.ToString()},
				New TableCell() With {.Text = entry.Description}})
			Select Case entry.Category
				Case KaEventLog.Categories.Failure : row.Cells(4).ForeColor = Drawing.Color.Red
				Case KaEventLog.Categories.Warning : row.Cells(4).ForeColor = Drawing.Color.DarkGoldenrod
			End Select
			logEntries.Rows.Add(row)
		Next
	End Sub

	Private Sub PopulateApplicationList()
		ddlApplication.Items.Clear()
		ddlApplication.Items.Add("All applications")
		Dim reader As OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), "SELECT DISTINCT(application_identifier) FROM event_log ORDER BY application_identifier ASC")
		Do While reader.Read()
			ddlApplication.Items.Add(reader(0))
		Loop
		reader.Close()
	End Sub

	Private Sub PopulateCategoryList()
		ddlCategory.Items.Clear()
		ddlCategory.Items.Add("All categories")
		For Each value As KaEventLog.Categories In System.Enum.GetValues(GetType(KaEventLog.Categories))
			ddlCategory.Items.Add(System.Enum.GetName(GetType(KaEventLog.Categories), value))
			ddlCategory.Items(ddlCategory.Items.Count - 1).Value = value
		Next
	End Sub

	Private Sub PopulateComputerList()
		ddlComputer.Items.Clear()
		ddlComputer.Items.Add("All computers")
		Dim reader As OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), "SELECT DISTINCT(computer) FROM event_log ORDER BY computer ASC")
		Do While reader.Read()
			ddlComputer.Items.Add(reader(0))
		Loop
		reader.Close()
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
		btnShowLogEntries_Click(btnShowLogEntries, e)
	End Sub

	Private Sub tbxEmailTo_TextChanged(sender As Object, e As System.EventArgs) Handles tbxEmailTo.TextChanged
		PopulateEmailAddressList()
		btnShowLogEntries_Click(btnShowLogEntries, e)
	End Sub

	Protected Sub btnSendEmail_Click(sender As Object, e As EventArgs) Handles btnSendEmail.Click
		Dim message As String = ""
		If Not Utilities.IsEmailFieldValid(tbxEmailTo.Text, message) Then
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidEmail", Utilities.JsAlert(message))
			Exit Sub
		End If
		Dim emailAddresses As String = ""
		If tbxEmailTo.Text.Trim().Length > 0 Then
			Dim header As String = "Event Log"
			btnShowLogEntries_Click(btnShowLogEntries, e)
			Dim body As String = Utilities.GenerateHTML(logEntries)

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
					attachments.Add(New System.Net.Mail.Attachment(New IO.MemoryStream(Encoding.UTF8.GetBytes(newEmail.Body)), "EventLog.html", System.Net.Mime.MediaTypeNames.Text.Html))
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