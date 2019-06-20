Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb
Imports System.IO

Partial Public Class TrackReport : Inherits System.Web.UI.Page

#Region " Web Form Designer Generated Code "
	'This call is required by the Web Form Designer.
	<System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
	End Sub

	Private Sub Page_Init(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Init
		InitializeComponent()
	End Sub

#End Region

	Private _changeAuthorization As Boolean = False
	Private _currentUser As KaUser
	Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
	Private _currentTableName As String = "reports"

	Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
		Response.Cache.SetCacheability(HttpCacheability.NoCache)
		_currentUser = Utilities.GetUser(Me)
		_currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "reports")
		If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")

		Dim delimString As String = ","
		Dim delim As Char() = delimString.ToCharArray()
		Dim x As Integer = 0
		Dim permArray As String() = Nothing
		Dim authorizedForThisPage As Boolean = False
		Dim authorizedForSomePages As Boolean = False
		Dim authorizedToModify As Boolean = False
		If Not Page.IsPostBack Then
			FillTracksComboBox()
			tbxFromDate.Value = Now.AddDays(-1).ToString("d")
			tbxToDate.Value = Now.ToString("d")
			Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
			Try
				ddlTracks.SelectedValue = KaSetting.GetSetting(connection, "TrackReport:" & _currentUser.Id.ToString & "/LastTrackFilter", Guid.Empty.ToString())
			Catch ex As Exception

			End Try
			Boolean.TryParse(KaSetting.GetSetting(connection, "TrackReport:" & _currentUser.Id.ToString & "/LastShowOperator", True), cbxShowOperator.Checked)
			Boolean.TryParse(KaSetting.GetSetting(connection, "TrackReport:" & _currentUser.Id.ToString & "/LastShowRfid", True), cbxShowRfid.Checked)
			Boolean.TryParse(KaSetting.GetSetting(connection, "TrackReport:" & _currentUser.Id.ToString & "/LastShowCarNumber", True), cbxShowCarNumber.Checked)
			Boolean.TryParse(KaSetting.GetSetting(connection, "TrackReport:" & _currentUser.Id.ToString & "/LastShowTrack", True), cbxShowTrack.Checked)
			Boolean.TryParse(KaSetting.GetSetting(connection, "TrackReport:" & _currentUser.Id.ToString & "/LastShowScanTime", True), cbxShowScannedTime.Checked)
			Boolean.TryParse(KaSetting.GetSetting(connection, "TrackReport:" & _currentUser.Id.ToString & "/LastShowReverseOrder", False), cbxShowReverseOrder.Checked)
			PopulateEmailAddressList()
		End If
		litEmailConfirmation.Text = ""
	End Sub

	Private Sub FillTracksComboBox()
		ddlTracks.Items.Add(New ListItem("All tracks", Guid.Empty.ToString))
		For Each track As KaTrack In KaTrack.GetAll(GetUserConnection(_currentUser.Id), "deleted = 0", "name")
			ddlTracks.Items.Add(New ListItem(track.Name, track.Id.ToString))
		Next
	End Sub

	Protected Sub RunQuery(sender As Object, e As EventArgs) Handles btnShowReport.Click, btnDownload.Click
		'clear any previous reports
		Dim startDate As DateTime = Now
		Dim endDate As DateTime = Now

		If Not DateTime.TryParse(tbxFromDate.Value, startDate) Then
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidStartDate", Utilities.JsAlert("Please select a valid starting date."))
		ElseIf Not DateTime.TryParse(tbxToDate.Value, endDate) Then
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidendDate", Utilities.JsAlert("Please select a valid ending date."))
		ElseIf startDate > endDate Then
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidStartEndDate", Utilities.JsAlert("Select a start date before the end date."))
		ElseIf Not (cbxShowCarNumber.Checked Or cbxShowScannedTime.Checked Or cbxShowTrack.Checked Or cbxShowOperator.Checked Or cbxShowRfid.Checked) Then
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidColumnCount", Utilities.JsAlert("Select at least 1 column to display."))
		ElseIf sender Is btnShowReport Then
			SaveOptionsSelected(GetUserConnection(_currentUser.Id))
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "ShowReport", Utilities.JsWindowOpen("TrackReportView.aspx?start_date=" & startDate.ToString &
					   "&end_date=" & endDate.ToString &
					   "&track_id=" & ddlTracks.SelectedValue &
					   "&show_operator=" & cbxShowOperator.Checked &
					   "&show_rfid=" & cbxShowRfid.Checked &
					   "&show_car_number=" & cbxShowCarNumber.Checked &
					   "&show_track=" & cbxShowTrack.Checked &
					   "&show_scan_time=" & cbxShowScannedTime.Checked &
					   "&show_reverse_order=" & cbxShowReverseOrder.Checked &
					   "&media_type=" & KaReports.MEDIA_TYPE_HTML))
		ElseIf sender Is btnDownload Then
			Dim fileName As String = String.Format("TrackReport{0:yyyyMMddHHmmss}.csv", Now)
			Dim caption As String = "Track report for dates " & startDate.ToString("d") & " to " & endDate.ToString("d")
			If ddlTracks.SelectedIndex > 0 Then caption &= " for " & ddlTracks.SelectedItem.Text
			Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
			KaReports.CreateCsvFromTable(caption, KaReports.GetTrackReportTable(connection, startDate, endDate.AddDays(1), Guid.Parse(ddlTracks.SelectedValue), cbxShowOperator.Checked, cbxShowRfid.Checked, cbxShowCarNumber.Checked, cbxShowTrack.Checked, cbxShowScannedTime.Checked, cbxShowReverseOrder.Checked), DownloadDirectory(Me) & fileName)
			SaveOptionsSelected(connection)
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "DownloadReport", Utilities.JsWindowOpen("./download/" & fileName))
		End If
	End Sub

	Private Sub SaveOptionsSelected(ByVal connection As OleDbConnection)
		KaSetting.WriteSetting(connection, "TrackReport:" & _currentUser.Id.ToString & "/LastTrackFilter", ddlTracks.SelectedValue)
		KaSetting.WriteSetting(connection, "TrackReport:" & _currentUser.Id.ToString & "/LastShowOperator", cbxShowOperator.Checked)
		KaSetting.WriteSetting(connection, "TrackReport:" & _currentUser.Id.ToString & "/LastShowRfid", cbxShowRfid.Checked)
		KaSetting.WriteSetting(connection, "TrackReport:" & _currentUser.Id.ToString & "/LastShowCarNumber", cbxShowCarNumber.Checked)
		KaSetting.WriteSetting(connection, "TrackReport:" & _currentUser.Id.ToString & "/LastShowTrack", cbxShowTrack.Checked)
		KaSetting.WriteSetting(connection, "TrackReport:" & _currentUser.Id.ToString & "/LastShowScanTime", cbxShowScannedTime.Checked)
		KaSetting.WriteSetting(connection, "TrackReport:" & _currentUser.Id.ToString & "/LastShowReverseOrder", cbxShowReverseOrder.Checked)
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
		Dim startDate As DateTime = Now
		Dim endDate As DateTime = Now
		Dim message As String = ""

		If Not DateTime.TryParse(tbxFromDate.Value, startDate) Then
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidStartDate", Utilities.JsAlert("Please select a valid starting date."))
		ElseIf Not DateTime.TryParse(tbxToDate.Value, endDate) Then
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidendDate", Utilities.JsAlert("Please select a valid ending date."))
		ElseIf startDate > endDate Then
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidStartEndDate", Utilities.JsAlert("Select a start date before the end date."))
		ElseIf Not (cbxShowCarNumber.Checked Or cbxShowScannedTime.Checked Or cbxShowTrack.Checked Or cbxShowOperator.Checked Or cbxShowRfid.Checked) Then
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidColumnCount", Utilities.JsAlert("Select at least 1 column to display."))
		ElseIf Not Utilities.IsEmailFieldValid(tbxEmailTo.Text, message) Then
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidEmail", Utilities.JsAlert(message))
		Else
			Dim emailAddresses As String = ""
			If tbxEmailTo.Text.Trim().Length > 0 Then
				Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
				SaveOptionsSelected(connection)
				Dim header As String = "Track Report"

				Dim body As String = KaReports.GetTableHtml(header, KaReports.GetTrackReportTable(connection, startDate, endDate.AddDays(1), Guid.Parse(ddlTracks.SelectedValue), cbxShowOperator.Checked, cbxShowRfid.Checked, cbxShowCarNumber.Checked, cbxShowTrack.Checked, cbxShowScannedTime.Checked, cbxShowReverseOrder.Checked))

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
						attachments.Add(New System.Net.Mail.Attachment(New IO.MemoryStream(Encoding.UTF8.GetBytes(newEmail.Body)), "TrackReport.html", System.Net.Mime.MediaTypeNames.Text.Html))
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
		End If
	End Sub
#End Region
End Class