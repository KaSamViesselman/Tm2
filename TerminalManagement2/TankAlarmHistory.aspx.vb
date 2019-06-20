Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb
Imports System.IO

Public Class TankAlarmHistory : Inherits System.Web.UI.Page

	Private _currentUser As KaUser
	Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing

#Region "Events"
	Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
		Response.Cache.SetCacheability(HttpCacheability.NoCache)
		_currentUser = Utilities.GetUser(Me)
		_currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaTank.TABLE_NAME}), "Tanks")
		If Not _currentUserPermission(KaTank.TABLE_NAME).Read Then Response.Redirect("Welcome.aspx")
		If Not Page.IsPostBack Then
			PopulateTankList()
			ddlTank.SelectedIndex = 0
			Dim minDate As New DateTime(Now.Year, Now.Month, Now.Day, 0, 0, 0)
			Dim maxDate As New DateTime(Now.Year, Now.Month, Now.Day, Now.Hour, Now.Minute, Now.Second)
			tbxFromDate.Value = minDate ' setting default dates 
			tbxToDate.Value = maxDate
			Dim minYear, maxYear As Integer
			GetMinAndMaxYear(Guid.Empty, minYear, maxYear)
			PopulateTankAlarmList()
			PopulateEmailAddressList()
		End If
		litEmailConfirmation.Text = ""
	End Sub

	Protected Sub ddlTank_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles ddlTank.SelectedIndexChanged
		Dim minYear, maxYear As Integer
		GetMinAndMaxYear(Guid.Parse(ddlTank.SelectedValue), minYear, maxYear)
		PopulateTankAlarmList()
	End Sub
#End Region

	Private Sub PopulateTankList()
		ddlTank.Items.Clear()
		ddlTank.Items.Add(New ListItem("All tanks", Guid.Empty.ToString()))
		For Each r As KaTank In KaTank.GetAll(GetUserConnection(_currentUser.Id), "deleted=0" & IIf(_currentUser.OwnerId = Guid.Empty, "", " AND owner_id=" & Q(_currentUser.OwnerId)), "name ASC")
			ddlTank.Items.Add(New ListItem(r.Name, r.Id.ToString()))
		Next
	End Sub

	Private Sub PopulateTankAlarmList()
		Dim tankId As Guid = Guid.Parse(ddlTank.SelectedValue)
		Dim startDate As DateTime = DateTime.Parse(tbxFromDate.Value)
		Dim endDate As DateTime = DateTime.Parse(tbxToDate.Value)
		Dim table As HtmlTable = KaReports.GetTankAlarmHistoryReport(GetUserConnection(_currentUser.Id), _currentUser.OwnerId, tankId, startDate, endDate)
		Dim writer As New StringWriter()
		Dim htmlWriter As New HtmlTextWriter(writer)
		table.RenderControl(htmlWriter)
		litList.Text = writer.ToString()
	End Sub

	Private Sub GetMinAndMaxYear(ByVal tankId As Guid, ByRef minYear As Integer, ByRef maxYear As Integer)
		Dim r As OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), "SELECT MIN(alarm_at), MAX(alarm_at) FROM tank_alarm_history" & IIf(tankId = Guid.Empty, "", " WHERE tank_id=" & Q(tankId)))
		If r.Read() Then
			minYear = IsNull(r(0), Now).Year
			maxYear = IsNull(r(1), Now).Year
		Else
			minYear = Now.Year
			maxYear = Now.Year
		End If
	End Sub

	Protected Sub btnShowAlarmHistory_Click(sender As Object, e As EventArgs) Handles btnShowAlarmHistory.Click
		Dim fromDate As DateTime
		Dim toDate As DateTime
		If ValidateOptions(fromDate, toDate) Then
			PopulateTankAlarmList()
		End If
	End Sub

	Private Function ValidateOptions(ByRef fromDate As DateTime, ByRef toDate As DateTime) As Boolean
		Try
			fromDate = DateTime.Parse(tbxFromDate.Value) ' converting string value to datetime value for comparison in IF statement
		Catch ex As FormatException
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidBeginningDate", Utilities.JsAlert("Please enter a valid date for the beginning (From) date"))
			Return False
		End Try
		Try
			toDate = DateTime.Parse(tbxToDate.Value)
		Catch ex As FormatException
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidEndingDate", Utilities.JsAlert("Please enter a valid date for the ending (To) date"))
			Return False
		End Try
		If fromDate > toDate Then ' check if "From" date is later then "To" date 
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidStartEndDate", Utilities.JsAlert("Please select an ending date (To) that is later than the beginning date (From)"))
			Return False
		End If
		Return True
	End Function

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
		Dim fromDate As DateTime
		Dim toDate As DateTime
		If tbxEmailTo.Text.Trim().Length > 0 AndAlso ValidateOptions(fromDate, toDate) Then
			Dim message As String = ""
			If Not Utilities.IsEmailFieldValid(tbxEmailTo.Text, message) Then
				ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidEmail", Utilities.JsAlert(message))
				Exit Sub
			End If
			Dim tankId As Guid = Guid.Parse(ddlTank.SelectedValue)
			Dim header As String = "Tank Alarm History from " & fromDate.ToString() & " to " & toDate.ToString()
			Dim body As String = Utilities.GenerateHTML(KaReports.GetTankAlarmHistoryReport(GetUserConnection(_currentUser.Id), _currentUser.OwnerId, tankId, fromDate, toDate))

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
					attachments.Add(New System.Net.Mail.Attachment(New IO.MemoryStream(Encoding.UTF8.GetBytes(newEmail.Body)), "TankAlarmHistory.html", System.Net.Mime.MediaTypeNames.Text.Html))
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