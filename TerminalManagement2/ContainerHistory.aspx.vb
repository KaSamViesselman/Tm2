Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb
Imports System.IO
Imports KaCommonObjects

Public Class ContainerHistory : Inherits System.Web.UI.Page

	Private _currentUser As KaUser
	Private _pageHeader As String

#Region "Events"

	Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
		Response.Cache.SetCacheability(HttpCacheability.NoCache)
		_currentUser = Utilities.GetUser(Me)
		If Not Utilities.GetUserPagePermission(_currentUser, New List(Of String)({"reports"}), "Reports")("reports").Read Then Response.Redirect("Welcome.aspx")
		If Not Page.IsPostBack Then
			FillColumnDisplayCbxs()
			PopulateEmailAddressList()
		End If
		Dim containerId As Guid = Guid.Parse(Request.QueryString("container_id"))
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		_pageHeader = "Container History for " & New KaContainer(connection, containerId).Number
		Dim cellAttributes As New List(Of String)
		litContainers.Text = KaReports.GetTableHtml(_pageHeader, "", KaReports.GetContainerHistoryTable(connection, KaReports.MEDIA_TYPE_HTML, containerId, GetDisplayedColumns(), SQL_MINDATE, SQL_MAXDATE, cellAttributes), False, "class=""display"" width=""100%""", "", New List(Of String), "", New List(Of String), cellAttributes, True)
		litEmailConfirmation.Text = ""
	End Sub

	Public Function GetControl(ByVal target As String) As System.Web.UI.Control
		Return FindControl(target)
	End Function
#End Region

	Protected Sub btnConfigure_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnConfigure.Click
		SaveConfiguration()
		Dim containerId As Guid = Guid.Parse(Request.QueryString("container_id"))
		Dim cellAttributes As New List(Of String)
		litContainers.Text = KaReports.GetTableHtml("", "", GetContainerHistoryTable(GetUserConnection(_currentUser.Id), KaReports.MEDIA_TYPE_HTML, containerId, GetDisplayedColumns(), SQL_MINDATE, SQL_MAXDATE, cellAttributes), False, "class=""display"" width=""100%""", "", New List(Of String), "", New List(Of String), cellAttributes, True)
	End Sub

	Function GetDisplayedColumns() As ULong
		Dim columnsDisplayed As ULong = 0
		If cbxNumberColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcNumber
		If cbxLocationColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcLocation
		If cbxStatusColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcStatus
		If cbxProductColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcProduct
		If cbxOwnerColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcOwner
		If cbxAccountColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcAccount
		If cbxLastTransactionColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcLastTransaction
		If cbxEmptyWeightColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcEmptyWeight
		If cbxVolumeColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcVolume
		If cbxProductWeightColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcProductWeight
		If cbxInServiceColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcInService
		If cbxLastFilledColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcLastFilled
		If cbxBulkProdEpaColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcBulkProdEpa
		If cbxSealNumberColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcSealNumber
		If cbxCreatedColumnShow.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcCreated
		If cbxTypeColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcType
		If cbxConditionColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcCondition
		If cbxLastChangedColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcLastChanged
		If cbxManufacturedColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcManufactured
		If cbxLastInspectedColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcLastInspected
		If cbxPassedInspectionColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcPassedInspection
		If cbxRefillableColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcRefillable
		If cbxLastCleanedColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcLastCleaned
		If cbxNotesColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcNotes
		If cbxSealBrokenColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcSealBroken
		If cbxPassedPressureTestColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcPassedPressureTest
		If cbxOneWayValvePresentColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcOneWayValvePresent
		If cbxForOrderIdColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcForOrderId
		If cbxEquipmentColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcEquipment
		If cbxLastUserIdColumnShown.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcLastUserId
		If cbxAssignedLotColumnShow.Checked Then columnsDisplayed = columnsDisplayed Or 2 ^ KaReports.ContainerReportColumns.RcLot

		Return columnsDisplayed
	End Function

	Public Shared Function GetCurrentColumnSetting(currentUserId As Guid) As ULong
		Dim connection As OleDbConnection = GetUserConnection(currentUserId)
		Dim i As Integer
		If Integer.TryParse(KaSetting.GetSetting(connection, $"ContainerHistoryColumns:{currentUserId}", -1), i) AndAlso i >= 0 Then
			Return i
		Else
			Dim columnSettings As ArrayList = KaSetting.GetAll(connection, "name LIKE 'containers_history%'", "")
			Return KaReports.GetContainerColumnsDisplayed(columnSettings, True)
		End If
	End Function

	Private Sub FillColumnDisplayCbxs()
		Dim columnsDisplayed As ULong = GetCurrentColumnSetting(_currentUser.Id)

		cbxNumberColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcNumber)
		cbxLocationColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcLocation)
		cbxStatusColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcStatus)
		cbxProductColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcProduct)
		cbxOwnerColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcOwner)
		cbxAccountColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcAccount)
		cbxLastTransactionColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcLastTransaction)
		cbxEmptyWeightColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcEmptyWeight)
		cbxVolumeColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcVolume)
		cbxProductWeightColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcProductWeight)
		cbxInServiceColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcInService)
		cbxLastFilledColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcLastFilled)
		cbxBulkProdEpaColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcBulkProdEpa)
		cbxSealNumberColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcSealNumber)
		cbxCreatedColumnShow.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcCreated)
		cbxTypeColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcType)
		cbxConditionColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcCondition)
		cbxLastChangedColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcLastChanged)
		cbxManufacturedColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcManufactured)
		cbxLastInspectedColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcLastInspected)
		cbxPassedInspectionColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcPassedInspection)
		cbxRefillableColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcRefillable)
		cbxLastCleanedColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcLastCleaned)
		cbxNotesColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcNotes)
		cbxSealBrokenColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcSealBroken)
		cbxPassedPressureTestColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcPassedPressureTest)
		cbxOneWayValvePresentColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcOneWayValvePresent)
		cbxForOrderIdColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcForOrderId)
		cbxEquipmentColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcEquipment)
		cbxLastUserIdColumnShown.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcLastUserId)
		cbxAssignedLotColumnShow.Checked = KaReports.ContainerReportIncludesColumn(columnsDisplayed, KaReports.ContainerReportColumns.RcLot)
	End Sub

	Private Sub SaveConfiguration()
		KaSetting.WriteSetting(GetUserConnection(_currentUser.Id), $"ContainerHistoryColumns:{_currentUser.Id}", GetDisplayedColumns())
	End Sub

	Private Sub btnPrinterFriendly_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnPrinterFriendly.Click
		Dim containerId As Guid = Guid.Parse(Request.QueryString("container_id"))
		ClientScript.RegisterClientScriptBlock(Me.GetType(), "ShowReport", Utilities.JsWindowOpen($"ContainerHistoryView.aspx?containerId={containerId.ToString}&columnsDisplayed={GetDisplayedColumns()}"))
	End Sub

	Private Sub btnDownload_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnDownload.Click
		Dim fileName As String = String.Format("ContainerHistory{0:yyyyMMddHHmmss}.csv", Now)
		Dim containerId As Guid = Guid.Parse(Request.QueryString("container_id"))
		Dim commaString As String = KaReports.GetTableCsv("", "", KaReports.GetContainerHistoryTable(GetUserConnection(_currentUser.Id), KaReports.MEDIA_TYPE_COMMA, containerId, GetDisplayedColumns(), SQL_MINDATE, SQL_MAXDATE, New List(Of String)))

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
			Dim containerId As Guid = Guid.Parse(Request.QueryString("container_id"))
			Dim header As String = _pageHeader
			Dim body As String = litContainers.Text

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
					attachments.Add(New System.Net.Mail.Attachment(New IO.MemoryStream(Encoding.UTF8.GetBytes(newEmail.Body)), "ContainerHistory.html", System.Net.Mime.MediaTypeNames.Text.Html))
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