Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class DriverHistory
    Inherits System.Web.UI.Page

    Private _currentUser As KaUser
    Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
    Private _currentTableName As String = KaDriver.TABLE_NAME

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Response.Cache.SetCacheability(HttpCacheability.NoCache)
        _currentUser = Utilities.GetUser(Me)
        _currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Drivers")
        If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")
        If Not Page.IsPostBack Then
            tbxFromDate.Value = New DateTime(Now.Year, Now.Month, Now.Day).ToString("G")
            tbxToDate.Value = Now.ToString("G")
            PopulateEmailAddressList()
        End If
        litEmailConfirmation.Text = ""
    End Sub

    Protected Sub RunQuery(sender As Object, e As EventArgs) Handles btnShowReport.Click, btnDownload.Click, btnPrinterFriendly.Click
        litOutput.Text = ""
        'clear any previous reports
        Dim startDate As DateTime = Now
        Dim endDate As DateTime = Now

        If ValidateOptions(startDate, endDate) Then
            If sender Is btnShowReport Then
                litOutput.Text = KaReports.GetTableHtml("Driver history for dates " & startDate.ToString("G") & " to " & endDate.ToString("G"), KaReports.GetDriverHistoryTable(GetUserConnection(_currentUser.Id), startDate, endDate))
            ElseIf sender Is btnPrinterFriendly Then
                ClientScript.RegisterClientScriptBlock(Me.GetType(), "ShowReport", Utilities.JsWindowOpen("DriverHistoryView.aspx?start_date=" & startDate.ToString & "&end_date=" & endDate.ToString & "&media_type=" & KaReports.MEDIA_TYPE_HTML))
            ElseIf sender Is btnDownload Then
                Dim fileName As String = String.Format("DriverHistory{0:yyyyMMddHHmmss}.csv", Now)
                KaReports.CreateCsvFromTable("Driver history for dates " & startDate.ToString("G") & " to " & endDate.ToString("G"), KaReports.GetDriverHistoryTable(GetUserConnection(_currentUser.Id), startDate, endDate), DownloadDirectory(Me) & fileName)
                ClientScript.RegisterClientScriptBlock(Me.GetType(), "DownloadReport", Utilities.JsWindowOpen("./download/" & fileName))
            End If
        End If
    End Sub

    Private Function ValidateOptions(ByRef startDate As DateTime, ByRef endDate As DateTime) As Boolean
        If Not DateTime.TryParse(tbxFromDate.Value, startDate) Then
            ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidStartDate", Utilities.JsAlert("Please select a valid starting date."))
            Return False
        ElseIf Not DateTime.TryParse(tbxToDate.Value, endDate) Then
            ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidEndDate", Utilities.JsAlert("Please select a valid ending date."))
            Return False
        ElseIf startDate > endDate Then
            ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidStartEndDate", Utilities.JsAlert("Select a start date before the end date."))
            Return False
        Else
            Return True
        End If
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
        Dim startDate As DateTime = Now
        Dim endDate As DateTime = Now

        If Not ValidateOptions(startDate, endDate) Then
            Exit Sub
        End If

        Dim message As String = ""
        If Not Utilities.IsEmailFieldValid(tbxEmailTo.Text, message) Then
            ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidEmail", Utilities.JsAlert(message))
            Exit Sub
        End If
        Dim emailAddresses As String = ""
        If tbxEmailTo.Text.Trim().Length > 0 Then
            Dim header As String = "Driver history for dates " & startDate.ToString("G") & " to " & endDate.ToString("G")
            Dim body As String = KaReports.GetTableHtml(header, KaReports.GetDriverHistoryTable(GetUserConnection(_currentUser.Id), startDate, endDate))

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
                    attachments.Add(New System.Net.Mail.Attachment(New IO.MemoryStream(Encoding.UTF8.GetBytes(newEmail.Body)), "DriversInFacilityHistory.html", System.Net.Mime.MediaTypeNames.Text.Html))
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