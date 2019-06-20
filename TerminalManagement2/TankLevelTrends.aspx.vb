Imports KahlerAutomation.KaTm2Database
Imports System.Environment
Imports System.Data.OleDb

Public Class TankLevelTrends : Inherits System.Web.UI.Page
	Private _currentUser As KaUser
	Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing

#Region "Events"
	Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
		Response.Cache.SetCacheability(HttpCacheability.NoCache)
		_currentUser = Utilities.GetUser(Me)
		_currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaTank.TABLE_NAME}), "Tanks")
		If Not _currentUserPermission(KaTank.TABLE_NAME).Read Then Response.Redirect("Welcome.aspx")
		lblStatus.Text = ""
		litEmailConfirmation.Text = ""
		If Not Page.IsPostBack Then
			SetTextboxMaxLengths()
			PopulateTankLevelTrendList()
			PopulateTankList()
			PopulatePeriodList()
			Dim minDate As New DateTime(Now.Year, Now.Month, Now.Day, 0, 0, 0)
			Dim maxDate As New DateTime(Now.Year, Now.Month, Now.Day, 23, 59, 59)
			tbxFromDate.Value = minDate 'setting default values for timepicker
			tbxToDate.Value = maxDate

			Utilities.ConfirmBox(Me.btnDelete, "Are you sure you want to delete this tank level trend?")
			Utilities.SetFocus(tbxName, Me)
			ddlTankLevelTrends_SelectedIndexChanged(ddlTankLevelTrends, Nothing)
			PopulateTankLevelTrendInformation(Guid.Empty)
			PopulateEmailAddressList()
		End If
	End Sub

	Protected Sub ddlTankLevelTrends_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles ddlTankLevelTrends.SelectedIndexChanged
		PopulateTankLevelTrendInformation(Guid.Parse(ddlTankLevelTrends.SelectedValue))
		SetControlUsabilityFromPermissions()
	End Sub

	Protected Sub btnSave_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnSave.Click
		If tbxName.Text.Length = 0 Then DisplayJavaScriptMessage("InvalidName", Utilities.JsAlert("Name must be specified.")) : Exit Sub
		If KaTankLevelTrend.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND id<>" & Q(Guid.Parse(ddlTankLevelTrends.SelectedValue)) & " AND name=" & Q(tbxName.Text), "").Count > 0 Then DisplayJavaScriptMessage("InvalidDuplicateName", Utilities.JsAlert("Name already in use.")) : Exit Sub
		If Guid.Parse(ddlTank.SelectedValue) = Guid.Empty Then DisplayJavaScriptMessage("InvalidTank", Utilities.JsAlert("A tank must be specified.")) : Exit Sub
		If Not IsNumeric(tbxInterval.Text) OrElse Integer.Parse(tbxInterval.Text) <= 0 Then DisplayJavaScriptMessage("InvalidInterval", Utilities.JsAlert("Interval must be a numeric value greater than zero.")) : Exit Sub
		Dim beginningAtDate As DateTime
		Try ' checking for improper values in textbox
			beginningAtDate = DateTime.Parse(tbxBeginningAtDate.Value)
		Catch ex As FormatException
			DisplayJavaScriptMessage("InvalidBeginningDate", Utilities.JsAlert("Please enter a valid date for the (Beginning at) date"))
			Return
		End Try
		If DateTime.Parse(tbxBeginningAtDate.Value) < SQL_MINDATE Then
			DisplayJavaScriptMessage("InvalidBeginningYear", Utilities.JsAlert("Beginning at year must be a numeric value greater than " & SQL_MINDATE & "."))
			Return
		Else
			With New KaTankLevelTrend()
				.Id = Guid.Parse(ddlTankLevelTrends.SelectedValue)
				If .Id <> Guid.Empty Then .SqlSelect(GetUserConnection(_currentUser.Id))
				.Name = tbxName.Text
				.TankId = Guid.Parse(ddlTank.SelectedValue)
				.Interval = tbxInterval.Text
				.Period = ddlPeriod.SelectedValue
				.BeginningAt = DateTime.Parse(tbxBeginningAtDate.Value)
				If .Id = Guid.Empty Then
					.SqlInsert(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
					PopulateTankLevelTrendList()
					ddlTankLevelTrends.SelectedValue = .Id.ToString()
					lblStatus.Text = "Tank level trend successfully added."
				Else
					.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
					lblStatus.Text = "Tank level trend successfully updated."
				End If
			End With
		End If
	End Sub

	Protected Sub btnDelete_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnDelete.Click
		With New KaTankLevelTrend(GetUserConnection(_currentUser.Id), Guid.Parse(ddlTankLevelTrends.SelectedValue))
			.Deleted = True
			.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
			PopulateTankLevelTrendList()
			ddlTankLevelTrends.SelectedValue = Guid.Empty.ToString()
			PopulateTankLevelTrendInformation(Guid.Empty)
			lblStatus.Text = "Tank level trend successfully deleted."
		End With
	End Sub
#End Region

	Private Sub PopulateTankLevelTrendList()
		ddlTankLevelTrends.Items.Clear()
		If _currentUserPermission(KaTank.TABLE_NAME).Create Then
			ddlTankLevelTrends.Items.Add(New ListItem("Enter a new tank level trend", Guid.Empty.ToString()))
		Else
			ddlTankLevelTrends.Items.Add(New ListItem("Select a tank level trend", Guid.Empty.ToString()))
		End If
		For Each r As KaTankLevelTrend In KaTankLevelTrend.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
			Try
				If r.TankId = Guid.Empty OrElse _currentUser.OwnerId = Guid.Empty OrElse New KaTank(GetUserConnection(_currentUser.Id), r.TankId).OwnerId = _currentUser.OwnerId Then ddlTankLevelTrends.Items.Add(New ListItem(r.Name, r.Id.ToString()))
			Catch ex As RecordNotFoundException
				DisplayJavaScriptMessage("InvalidTankId", Utilities.JsAlert("Record not found in tanks where ID = " & r.TankId.ToString() & " while populating tank level trend list."))
			End Try
		Next
	End Sub

	Private Sub PopulateTankList()
		ddlTank.Items.Clear()
		ddlTank.Items.Add(New ListItem("", Guid.Empty.ToString()))
		For Each r As KaTank In KaTank.GetAll(GetUserConnection(_currentUser.Id), "deleted=0" & IIf(_currentUser.OwnerId = Guid.Empty, "", " AND owner_id=" & Q(_currentUser.OwnerId)), "name ASC")
			ddlTank.Items.Add(New ListItem(r.Name, r.Id.ToString()))
		Next
	End Sub

	Private Sub PopulatePeriodList()
		ddlPeriod.Items.Clear()
		ddlPeriod.Items.Add(New ListItem("Minute(s)", KaTankLevelTrend.TimePeriod.Minutes))
		ddlPeriod.Items.Add(New ListItem("Hour(s)", KaTankLevelTrend.TimePeriod.Hours))
		ddlPeriod.Items.Add(New ListItem("Day(s)", KaTankLevelTrend.TimePeriod.Days))
		ddlPeriod.Items.Add(New ListItem("Month(s)", KaTankLevelTrend.TimePeriod.Months))
	End Sub

	Private Sub PopulateTankLevelTrendInformation(ByVal tankLevelTrendId As Guid)
		pnlTankLevelTrendDataDisplay.Visible = False
		With New KaTankLevelTrend()
			.Id = tankLevelTrendId
			If .Id <> Guid.Empty Then .SqlSelect(GetUserConnection(_currentUser.Id))
			tbxName.Text = .Name
			Try
				ddlTank.SelectedValue = .TankId.ToString()
			Catch ex As ArgumentOutOfRangeException
				ddlTank.SelectedIndex = 0
				DisplayJavaScriptMessage("InvalidTankId", Utilities.JsAlert("Record not found in tanks where ID = " & .TankId.ToString() & ". Tank not set."))
			End Try
			tbxInterval.Text = .Interval
			Try
				ddlPeriod.SelectedValue = .Period
			Catch ex As ArgumentOutOfRangeException
				ddlPeriod.SelectedValue = KaTankLevelTrend.TimePeriod.Minutes
				DisplayJavaScriptMessage("InvalidPeriod", Utilities.JsAlert("Invalid period (" & .Period & "). Period set to ""minute(s)""."))
			End Try
			tbxBeginningAtDate.Value = IIf(.BeginningAt > New Date(1900, 1, 1), .BeginningAt, GetMinTankDataDate())
			pnlTankLevelTrendData.Visible = Not .Id.Equals(Guid.Empty)

			PopulateDisplayUoM(tankLevelTrendId)
		End With
	End Sub

	Private Sub SetTextboxMaxLengths()
		tbxInterval.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaTankLevelTrend.TABLE_NAME, "interval"))
		tbxName.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaTankLevelTrend.TABLE_NAME, "name"))
	End Sub

	Private Function GetMinTankDataDate() As DateTime
		Dim minDateTime As New Date(Now.Year, Now.Month, Now.Day)
		Dim minDateTrendRdr As OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), "SELECT MIN(read_at) AS read_at FROM tank_level_trend_data WHERE (deleted = 0) AND (read_at > '19000101')")
		If minDateTrendRdr.Read Then
			If Not minDateTrendRdr.Item("read_at").Equals(DBNull.Value) AndAlso minDateTrendRdr.Item("read_at") < minDateTime Then minDateTime = minDateTrendRdr.Item("read_at")
		End If
		minDateTrendRdr.Close()
		Dim minDateTankRdr As OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), "SELECT MIN(last_read) AS last_read FROM tanks WHERE (deleted = 0) AND (last_read > '19000101')")
		If minDateTankRdr.Read Then
			If Not minDateTankRdr.Item("last_read").Equals(DBNull.Value) AndAlso minDateTankRdr.Item("last_read") < minDateTime Then minDateTime = minDateTankRdr.Item("last_read")
		End If
		minDateTankRdr.Close()
		Return minDateTime
	End Function

	Protected Sub btnDownload_Click(sender As Object, e As EventArgs) Handles btnDownload.Click
		Dim fromDate As DateTime
		Dim toDate As DateTime
		If ValidateOptions(fromDate, toDate) Then
			Dim tankLevelTrendId As Guid = Guid.Parse(ddlTankLevelTrends.SelectedValue)
			Dim fileName As String = String.Format("TankLevelTrendData_{0:yyyyMMddHHmmss}.csv", Now)
			Dim header As String = "Tank Level Trend Data from " & fromDate.ToString() & " to " & toDate.ToString()
			KaReports.CreateCsvFromTable(header, New KaTankLevelTrend(GetUserConnection(_currentUser.Id), tankLevelTrendId).Name, KaReports.GetTankLevelTrendTable(GetUserConnection(_currentUser.Id), tankLevelTrendId, DateTime.Parse(tbxFromDate.Value), DateTime.Parse(tbxToDate.Value), True, Guid.Parse(ddlDisplayUnit.SelectedValue), cbxDisplayTemperature.Checked), DownloadDirectory(Me) & fileName)
			DisplayJavaScriptMessage("DownloadFile", Utilities.JsWindowOpen($"./download/{fileName}"))
			'Page.Response.Redirect("./download/" & fileName)
		End If
	End Sub

	Private Sub PopulateDisplayUoM(tankLevelTrendId As Guid)
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)

		Dim massOk As Boolean = False
		Dim volumeOk As Boolean = False
		Try ' to determine which units are compatible with this trend...
			Dim tankLevelTrend As New KaTankLevelTrend(connection, tankLevelTrendId)
			Dim tank As New KaTank(connection, tankLevelTrend.TankId)
			Dim tankUnit As New KaUnit(connection, tank.UnitId)
			If KaUnit.IsWeight(tankUnit.BaseUnit) Then
				massOk = True
			Else ' is volume
				volumeOk = True
			End If
			Dim bulkProduct As New KaBulkProduct(connection, tank.BulkProductId)
			If bulkProduct.Density > 0 Then ' with density, both mass and volume are OK
				massOk = True
				volumeOk = True
			End If
		Catch ex As Exception ' suppress
		End Try
		ddlDisplayUnit.Items.Clear()
		ddlDisplayUnit.Items.Add(New ListItem("Tank's Unit", Guid.Empty.ToString))
		For Each n As KaUnit In KaUnit.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
			If Not KaUnit.IsTime(n.BaseUnit) Then
				Dim isWeight As Boolean = KaUnit.IsWeight(n.BaseUnit)
				If ((massOk AndAlso isWeight) OrElse (volumeOk AndAlso Not isWeight)) Then ddlDisplayUnit.Items.Add(New ListItem(n.Name, n.Id.ToString()))
			End If
		Next
	End Sub

	Protected Sub btnShowData_Click(sender As Object, e As EventArgs) Handles btnShowData.Click
		pnlTankLevelTrendDataDisplay.Visible = False
		Try
			Dim startDate As DateTime
			Dim endDate As DateTime
			If ValidateOptions(startDate, endDate) Then
				Dim tankLevelTrendId As Guid = Guid.Parse(ddlTankLevelTrends.SelectedValue)
				Dim displayUnitId As Guid = Guid.Parse(ddlDisplayUnit.SelectedValue)
				Dim includeTemp As Boolean = cbxDisplayTemperature.Checked
				litData.Text = KaReports.GetTableHtml(New KaTankLevelTrend(GetUserConnection(_currentUser.Id), tankLevelTrendId).Name, KaReports.GetTankLevelTrendTable(GetUserConnection(_currentUser.Id), tankLevelTrendId, startDate, endDate, False, displayUnitId, includeTemp))
				imgGraph.Src = $"data:image/png;base64,{ConvertImageToData(KaReports.GetTankLevelTrendGraph(GetUserConnection(_currentUser.Id), tankLevelTrendId, startDate, endDate, 4.5, displayUnitId, includeTemp, TankLevelTrendData.LevelFillColor, TankLevelTrendData.TemperatureFillColor))}"
				imgGraph.Attributes().Item("width") = "100%"
				imgGraph.Attributes().Item("height") = "auto"

			End If
			pnlTankLevelTrendDataDisplay.Visible = True
		Catch ex As Exception

		End Try
	End Sub

	Private Function ValidateOptions(ByRef fromDate As DateTime, ByRef toDate As DateTime) As Boolean
		Try
			fromDate = DateTime.Parse(tbxFromDate.Value) ' converting string value to datetime value for comparison in IF statement
		Catch ex As FormatException
			DisplayJavaScriptMessage("InvalidStartDate", Utilities.JsAlert("Please enter a valid date for the beginning (From) date"))
			Return False
		End Try
		Try
			toDate = DateTime.Parse(tbxToDate.Value)
		Catch ex As FormatException
			DisplayJavaScriptMessage("InvalidEndDate", Utilities.JsAlert("Please enter a valid date for the ending (To) date"))
			Return False
		End Try
		If fromDate > toDate Then ' check if "From" date is later then "To" date 
			DisplayJavaScriptMessage("InvalidStartEndDate", Utilities.JsAlert("Please select an ending date (To) that is later than the beginning date (From)"))
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
		If ValidateOptions(fromDate, toDate) Then
			Dim message As String = ""
			If Not Utilities.IsEmailFieldValid(tbxEmailTo.Text, message) Then
				DisplayJavaScriptMessage("InvalidEmail", Utilities.JsAlert(message))
				Exit Sub
			End If
			Dim emailAddresses As String = ""
			If tbxEmailTo.Text.Trim().Length > 0 Then
				Dim tankLevelTrendId As Guid = Guid.Parse(ddlTankLevelTrends.SelectedValue)
				Dim header As String = New KaTankLevelTrend(GetUserConnection(_currentUser.Id), tankLevelTrendId).Name & " from " & fromDate.ToString() & " to " & toDate.ToString()
				Dim body As String = $"<img alt=""Picture"" style=""width:100%; height:auto;"" src=""data:image/png;base64,{ConvertImageToData(KaReports.GetTankLevelTrendGraph(GetUserConnection(_currentUser.Id), tankLevelTrendId, fromDate, toDate, 4.5, Guid.Parse(ddlDisplayUnit.SelectedValue), cbxDisplayTemperature.Checked, TankLevelTrendData.LevelFillColor, TankLevelTrendData.TemperatureFillColor))}"" />" &
					   "<br />" &
					   KaReports.GetTableHtml(header, New KaTankLevelTrend(GetUserConnection(_currentUser.Id), tankLevelTrendId).Name, KaReports.GetTankLevelTrendTable(GetUserConnection(_currentUser.Id), tankLevelTrendId, fromDate, toDate, True, Guid.Parse(ddlDisplayUnit.SelectedValue), cbxDisplayTemperature.Checked))

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
						attachments.Add(New System.Net.Mail.Attachment(New IO.MemoryStream(Encoding.UTF8.GetBytes(newEmail.Body)), "TankLevelTrendData.html", System.Net.Mime.MediaTypeNames.Text.Html))
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

	Private Sub SetControlUsabilityFromPermissions()
		With _currentUserPermission(KaTank.TABLE_NAME)
			Dim shouldEnable = (.Edit AndAlso ddlTankLevelTrends.SelectedIndex > 0) OrElse (.Create AndAlso ddlTankLevelTrends.SelectedIndex = 0)
			pnlEven.Enabled = shouldEnable
			btnSave.Enabled = shouldEnable
			Dim value = Guid.Parse(ddlTankLevelTrends.SelectedValue)
			btnDelete.Enabled = .Edit AndAlso .Delete AndAlso value <> Guid.Empty
		End With
	End Sub

	Protected Sub btnPrinterFriendly_Click(sender As Object, e As EventArgs) Handles btnPrinterFriendly.Click
		Dim fromDate As DateTime
		Dim toDate As DateTime
		If ValidateOptions(fromDate, toDate) Then
			DisplayJavaScriptMessage("ViewData", Utilities.JsWindowOpen($"TankLevelTrendData.aspx?tank_level_trend_id={ddlTankLevelTrends.SelectedValue}&DisplayUnitId={ddlDisplayUnit.SelectedValue}&StartDate={Server.HtmlEncode(tbxFromDate.Value)}&EndDate={Server.HtmlEncode(tbxToDate.Value)}&IncludeTemp={cbxDisplayTemperature.Checked}"))
		End If
	End Sub

	Protected Sub ToolkitScriptManager1_AsyncPostBackError(ByVal sender As Object, ByVal e As System.Web.UI.AsyncPostBackErrorEventArgs)
		ToolkitScriptManager1.AsyncPostBackErrorMessage = e.Exception.Message
	End Sub

	Private Sub DisplayJavaScriptMessage(key As String, script As String)
		ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), key, script, False)
	End Sub
End Class