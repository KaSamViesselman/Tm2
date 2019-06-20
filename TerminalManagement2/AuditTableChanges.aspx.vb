Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb
Imports System.IO
Imports System.Xml

Public Class AuditTableChanges
	Inherits System.Web.UI.Page
	Private _currentUser As KaUser
	Private _emailReportId As Guid = Guid.Empty

	Protected Sub Page_Load(ByVal sender As Object, ByVal e6 As System.EventArgs) Handles Me.Load
		Response.Cache.SetCacheability(HttpCacheability.NoCache)
		_currentUser = Utilities.GetUser(Me)

		If Request.QueryString("email_report_id") IsNot Nothing Then Guid.TryParse(Request.QueryString("email_report_id"), _emailReportId)

		Dim config As Boolean = False
		If Request.QueryString("config") IsNot Nothing Then Boolean.TryParse(Request.QueryString("config"), config)

		Dim pageValid As Boolean = True
		If config Then
			pageValid = Not _emailReportId.Equals(Guid.Empty)
		Else
			pageValid = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({"reports"}), "Reports")("reports").Read
		End If
		If Not pageValid Then Response.Redirect("Welcome.aspx")

		lblStatus.Text = ""
        If Not Page.IsPostBack Then
            Dim minDate As New DateTime(Now.Year, Now.Month, Now.Day, 0, 0, 0) ' setting default "From" time to 12:00AM of the current date
            Dim maxDate As DateTime = minDate.AddDays(1) ' setting default "To" time to the current time 
            tbxFromDate.Value = minDate.ToString("G") ' setting "From" datepicker to default times 
            tbxToDate.Value = maxDate.ToString("G") ' setting "To" datepicker to default time
            Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
			PopulateTableNames(connection)
			PopulateApplications(connection)
			PopulateUsernames(connection)
			PopulateAuditTypes()
			PopulateEmailAddressList()

			If config Then
				Dim parameters As String = KaSetting.GetSetting(GetUserConnection(_currentUser.Id), "AuditTableChangesReport:" & _emailReportId.ToString, "")
				Dim applicationIdentifier As String = EmailReports.GetParameter("audit_application", parameters, "")
				Dim tableName As String = EmailReports.GetParameter("table_name", parameters, "")
				Dim auditType As KaReports.AuditedDataType = EmailReports.GetParameter("audit_type", parameters, KaReports.AuditedDataType.All)
				Dim username As String = EmailReports.GetParameter("audit_user", parameters, "")

				Try
					ddlApplication.SelectedValue = applicationIdentifier
				Catch ex As ArgumentOutOfRangeException
					ddlApplication.SelectedIndex = 0
				End Try
				Try
					ddlTableName.SelectedValue = tableName
				Catch ex As ArgumentOutOfRangeException
					ddlTableName.SelectedIndex = 0
				End Try
				Try
					ddlAuditType.SelectedIndex = auditType
				Catch ex As ArgumentOutOfRangeException
					ddlAuditType.SelectedIndex = 0
				End Try
				Try
					ddlUser.SelectedValue = username
				Catch ex As ArgumentOutOfRangeException
					ddlUser.SelectedIndex = 0
				End Try
			End If
			pnlDateFilters.Visible = Not config
			btnDownload.Visible = Not config
			btnShowReport.Visible = Not config
			btnSave.Visible = config
			pnlSendEmail.Visible = Not config
			lblStatus.Visible = config
		End If
	End Sub

	Private Sub PopulateTableNames(ByVal connection As OleDbConnection)
		ddlTableName.Items.Clear()
		ddlTableName.Items.Add(New ListItem("All tables"))
		Dim rdr As OleDbDataReader = Tm2Database.ExecuteReader(connection, String.Format("SELECT DISTINCT {0}, UPPER({0}) AS {0}_index FROM " & KaAudit.TABLE_NAME & " ORDER BY 2", KaAudit.FN_TABLE_NAME))
		Do While rdr.Read()
			ddlTableName.Items.Add(New ListItem(rdr.Item(KaAudit.FN_TABLE_NAME)))
		Loop
		rdr.Close()
	End Sub

	Private Sub PopulateApplications(ByVal connection As OleDbConnection)
		With ddlApplication.Items
			.Clear()
			.Add(New ListItem("All applications"))
			Dim rdr As OleDbDataReader = Tm2Database.ExecuteReader(connection, String.Format("SELECT DISTINCT {0}, UPPER({0}) AS {0}_index FROM " & KaAudit.TABLE_NAME & " ORDER BY 2", KaAudit.FN_LAST_UPDATED_APPLICATION))
			Do While rdr.Read()
				If rdr.Item(KaAudit.FN_LAST_UPDATED_APPLICATION).Length > 0 Then .Add(New ListItem(rdr.Item(KaAudit.FN_LAST_UPDATED_APPLICATION)))
			Loop
			rdr.Close()
		End With
	End Sub

	Private Sub PopulateUsernames(ByVal connection As OleDbConnection)
		With ddlUser.Items
			.Clear()
			.Add(New ListItem("All users"))
			Dim rdr As OleDbDataReader = Tm2Database.ExecuteReader(connection, String.Format("SELECT DISTINCT {0}, UPPER({0}) AS {0}_index FROM " & KaAudit.TABLE_NAME & " ORDER BY 2", KaAudit.FN_LAST_UPDATED_USER))
			Do While rdr.Read()
				If rdr.Item(KaAudit.FN_LAST_UPDATED_USER).Length > 0 Then .Add(New ListItem(rdr.Item(KaAudit.FN_LAST_UPDATED_USER)))
			Loop
			rdr.Close()
		End With
	End Sub

	Private Sub PopulateAuditTypes()
		With ddlAuditType.Items
			.Clear()
			.Add(New ListItem("All types"))
			.Add(New ListItem("Inserted"))
			.Add(New ListItem("Updated"))
			.Add(New ListItem("Deleted"))
		End With
	End Sub

	Protected Sub btnShowReport_Click(sender As Object, e As EventArgs) Handles btnShowReport.Click
		If ValidateOptions() Then
			Dim report As ArrayList = GetAuditReport()
			Dim cellParameters As New List(Of String)
			For n = 0 To report(0).Count - 1
				cellParameters.Add("style=""text-align:left;""")
			Next
			Dim subheader As String = GetSubHeaderReport()

			litReport.Text = KaReports.GetTableHtml("Audited table changes", subheader, report, False, "width=""100%""", "", cellParameters, "", cellParameters)
		Else
			litReport.Text = ""
		End If
	End Sub

	Protected Sub btnDownload_Click(sender As Object, e As EventArgs) Handles btnDownload.Click
		If ValidateOptions() Then
			Dim report As ArrayList = GetAuditReport()
			Dim subheader As String = GetSubHeaderReport()

			Dim fileName As String = String.Format("AuditedTableChanges{0:yyyyMMddHHmmss}.csv", Now)
			KaReports.CreateCsvFromTable("Audited table changes", subheader, report, DownloadDirectory(Me) & fileName)

			ClientScript.RegisterClientScriptBlock(Me.GetType(), "DownloadReport", Utilities.JsWindowOpen("./download/" & fileName))
		End If
	End Sub

	Protected Sub btnSendEmail_Click(sender As Object, e As EventArgs) Handles btnSendEmail.Click
		If Not ValidateOptions() Then
			Exit Sub
		End If
		Dim message As String = ""
		If Not Utilities.IsEmailFieldValid(tbxEmailTo.Text, message) Then
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidEmail", Utilities.JsAlert(message))
			Exit Sub
		End If
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim report As ArrayList = GetAuditReport()
		Dim subheader As String = GetSubHeaderReport()
		Dim cellParameters As New List(Of String)
		For n = 0 To report(0).Count - 1
			cellParameters.Add("style=""text-align:left; border:1px solid black;""")
		Next

		Dim body As String = KaReports.GetTableHtml("Audited table changes", subheader, report, False, "width=""100%""", "", cellParameters, "", cellParameters)
		Dim emailAddresses As String = ""
		If tbxEmailTo.Text.Trim().Length > 0 Then
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
					newEmail.Subject = "Audited table changes " & subheader
					Dim attachments As New List(Of System.Net.Mail.Attachment)
					attachments.Add(New System.Net.Mail.Attachment(New MemoryStream(Encoding.UTF8.GetBytes(newEmail.Body)), "AuditedTableChanges.html", System.Net.Mime.MediaTypeNames.Text.Html))
					newEmail.SerializeAttachments(attachments)
					newEmail.SqlInsert(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
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

	Private Function ValidateOptions()
		Dim retVal As Boolean = True
		Dim fromDate As DateTime
		Try
			fromDate = DateTime.Parse(tbxFromDate.Value) ' converting string value to datetime value for comparison in IF statement
		Catch ex As FormatException
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidStartDate", Utilities.JsAlert("Please enter a valid date for the beginning (From) date"))
			Return False
		End Try
		Dim toDate As DateTime
		Try
			toDate = DateTime.Parse(tbxToDate.Value)
		Catch ex As FormatException
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidEndDate", Utilities.JsAlert("Please enter a valid date for the ending (To) date"))
			Return False
		End Try
		If fromDate > toDate Then ' check if "From" date is later then "To" date 
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidStartEndDate", Utilities.JsAlert("Please select an ending date (To) that is later than the beginning date (From)"))
			Return False
		End If
		Return retVal
	End Function

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

	Private Function GetAuditReport() As ArrayList
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim tableName As String = ""
		If ddlTableName.SelectedIndex > 0 Then tableName = ddlTableName.SelectedValue
		Dim application As String = ""
		If ddlApplication.SelectedIndex > 0 Then application = ddlApplication.SelectedValue
		Dim username As String = ""
		If ddlUser.SelectedIndex > 0 Then username = ddlUser.SelectedValue
		Dim auditType As KaReports.AuditedDataType = AuditedDataType.All
		If ddlAuditType.SelectedIndex > 0 Then auditType = ddlAuditType.SelectedIndex
		Return KaReports.GetAuditedDataReport(connection, tbxFromDate.Value, tbxToDate.Value, tableName, application, username, auditType)
	End Function

	Private Function GetSubHeaderReport() As String
		Dim subheader As String = ""
		If ddlTableName.SelectedIndex > 0 Then subheader &= " for table " & Q(ddlTableName.SelectedValue)
		Select Case ddlAuditType.SelectedIndex
			Case 1
				subheader &= IIf(subheader.Length = 0, " for", ",") & " inserted records"
			Case 2
				subheader &= IIf(subheader.Length = 0, " for", ",") & " updated records"
			Case 3
				subheader &= IIf(subheader.Length = 0, " for", ",") & " deleted records"
		End Select
		If ddlApplication.SelectedIndex > 0 Then subheader &= IIf(subheader.Length = 0, " for", ",") & " application " & Q(ddlApplication.SelectedValue)
		If ddlUser.SelectedIndex > 0 Then subheader &= IIf(subheader.Length = 0, " for", ",") & " user " & Q(ddlUser.SelectedValue)

		subheader = "from " & tbxFromDate.Value & " to " & tbxToDate.Value & subheader
		Return subheader
	End Function

	Private Sub btnSave_Click(sender As Object, e As System.EventArgs) Handles btnSave.Click
		Dim s As New MemoryStream() ' the XML writer will use this to build the XML data
		Dim w As XmlWriter = XmlWriter.Create(s)
		w.WriteStartElement("parameters")
		w.WriteElementString("audit_application", ddlApplication.SelectedValue)
		w.WriteElementString("table_name", ddlTableName.SelectedValue)
		w.WriteElementString("audit_type", ddlAuditType.SelectedIndex)
		w.WriteElementString("audit_user", ddlUser.SelectedValue)
		w.WriteEndElement() ' close the parameters tag
		w.Flush()
		s.Seek(0, SeekOrigin.Begin) ' move to the beginning of the stream

		Dim parameters As String = New StreamReader(s).ReadToEnd() ' convert the stream to a string

		KaSetting.WriteSetting(Tm2Database.Connection, "AuditTableChangesReport:" & _emailReportId.ToString, parameters)
		lblStatus.Text = "Settings saved"
	End Sub
End Class