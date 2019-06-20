Imports KahlerAutomation.KaTm2Database
Imports System.IO
Imports KaCommonObjects

Public Class TransportTrackingReport : Inherits System.Web.UI.Page
	Private _currentUser As KaUser
	Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing

	Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
		Response.Cache.SetCacheability(HttpCacheability.NoCache)
		_currentUser = Utilities.GetUser(Me)
		_currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaTransport.TABLE_NAME}), "Transports")
		If Not _currentUserPermission(KaTransport.TABLE_NAME).Read Then Response.Redirect("Welcome.aspx")

		If Not Page.IsPostBack Then
			PopulateOrderByCombo()
			PopulateAscDescCombo()
			PopulateUnitList()
			PopulateEmailAddressList()
			btnDisplayReport.Attributes.Add("onclick", "pleaseWait();")
			btnDownload.Attributes.Add("onclick", "pleaseWait();")
		End If

		litEmailConfirmation.Text = ""
	End Sub

	Private Sub PopulateOrderByCombo()
		ddlOrderBy.Items.Clear()
		ddlOrderBy.Items.Add(New ListItem("Transport", "name"))
		ddlOrderBy.Items.Add(New ListItem("Loaded Date", "loaded_at"))
		ddlOrderBy.Items.Add(New ListItem("Ticket Number", "number"))
		ddlOrderBy.SelectedIndex = 0
	End Sub

	Private Sub PopulateAscDescCombo()
		ddlAscDesc.Items.Clear()
		ddlAscDesc.Items.Add(New ListItem("Asc", "asc"))
		ddlAscDesc.Items.Add(New ListItem("Desc", "desc"))
		ddlAscDesc.SelectedIndex = 0
	End Sub

	Private Sub PopulateUnitList()
		ddlDisplayUnit.Items.Clear()
		For Each r As KaUnit In KaUnit.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND base_unit<>9", "abbreviation ASC")
			ddlDisplayUnit.Items.Add(New ListItem(r.Abbreviation, r.Id.ToString()))
			If r.Factor = 1 AndAlso r.BaseUnit = KaUnit.Unit.Pounds Then
				ddlDisplayUnit.SelectedIndex = ddlDisplayUnit.Items.Count - 1
			End If
		Next
	End Sub

	Private Sub DisplayResults()
		Dim connection As OleDb.OleDbConnection = GetUserConnection(_currentUser.Id)
		KaReports.GetTransportTrackingReport(connection, tblTransports, KaReports.MEDIA_TYPE_HTML, ddlOrderBy.SelectedValue & " " & ddlAscDesc.SelectedValue, Guid.Parse(ddlDisplayUnit.SelectedValue), Utilities.ConvertWebPageUrlDomainToRequestedPagesDomain(connection))
	End Sub

	Private Sub btnDownload_Click(sender As Object, e As System.EventArgs) Handles btnDownload.Click
		Dim connection As OleDb.OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim commaString As String = KaReports.GetTransportTrackingReport(connection, tblTransports, KaReports.MEDIA_TYPE_COMMA, ddlOrderBy.SelectedValue & " " & ddlAscDesc.SelectedValue, Guid.Parse(ddlDisplayUnit.SelectedValue), Utilities.ConvertWebPageUrlDomainToRequestedPagesDomain(connection))

		Dim fileName As String = String.Format("TransportTrackingReport{0:yyyyMMddHHmmss}.csv", Now)

		Dim writer As StreamWriter = Nothing
		Try
			Dim fileOps As FileOperations = New FileOperations
			writer = fileOps.WriteFile(DownloadDirectory(Me) & fileName, New Alerts)
			writer.WriteLine(commaString)
		Finally
			If Not writer Is Nothing Then
				writer.Close()
			End If
		End Try
		ClientScript.RegisterClientScriptBlock(Me.GetType(), "DownloadReport", Utilities.JsWindowOpen("./download/" & fileName))
	End Sub

	Private Sub btnPrinterFriendlyVersion_Click(sender As Object, e As System.EventArgs) Handles btnPrinterFriendlyVersion.Click
		ClientScript.RegisterClientScriptBlock(Me.GetType(), "ShowReport", Utilities.JsWindowOpen("TransportTrackingReportPFV.aspx?current_user_id=" & _currentUser.Id.ToString & "&order_by=" & ddlOrderBy.SelectedValue & "&asc_desc=" & ddlAscDesc.SelectedValue & "&display_unit_id=" & ddlDisplayUnit.SelectedValue))

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
		Dim message As String = ""
		If Not Utilities.IsEmailFieldValid(tbxEmailTo.Text, message) Then
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidEmail", Utilities.JsAlert(message))
			Exit Sub
		End If
		Dim emailAddresses As String = ""
		If tbxEmailTo.Text.Trim().Length > 0 Then
			Dim header As String = "Transport Tracking Report"
			Dim body As String = Utilities.GenerateHTML(tblTransports)

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
					attachments.Add(New System.Net.Mail.Attachment(New IO.MemoryStream(Encoding.UTF8.GetBytes(newEmail.Body)), "TransportTrackingReport.html", System.Net.Mime.MediaTypeNames.Text.Html))
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

	Protected Sub btnDisplayReport_Click(sender As Object, e As EventArgs) Handles btnDisplayReport.Click
		DisplayResults()
	End Sub
End Class