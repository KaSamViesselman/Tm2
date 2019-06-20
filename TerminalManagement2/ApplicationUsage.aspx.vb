Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class ApplicationUsage : Inherits System.Web.UI.Page

	Private _currentUser As KaUser
	Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
	Private _currentTableName As String = "reports"

	Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
		Response.Cache.SetCacheability(HttpCacheability.NoCache)
		_currentUser = Utilities.GetUser(Me)
		_currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "reports")
		If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")
		If Not Page.IsPostBack Then
			PopulateApplicationList()
			PopulateEmailAddressList()
		End If
		litEmailConfirmation.Text = ""
	End Sub

	Protected Sub btnPrinterFriendlyVersion_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnPrinterFriendlyVersion.Click
		ClientScript.RegisterClientScriptBlock(Me.GetType(), "ShowReport", Utilities.JsWindowOpen("ApplicationUsageView.aspx"))
	End Sub

	Protected Sub btnDownload_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnDownload.Click
		Dim fileName As String = String.Format("ApplicationUsage{0:yyyyMMddHHmmss}.csv", Now)
		KaReports.CreateCsvFromTable("Application Usage", KaReports.GetApplicationUsageTable(GetUserConnection(_currentUser.Id)), DownloadDirectory(Me) & fileName)
		ClientScript.RegisterClientScriptBlock(Me.GetType(), "DownloadReport", Utilities.JsWindowOpen("./download/" & fileName))
	End Sub

	Private Sub PopulateApplicationList()
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		litReport.Text = KaReports.GetTableHtml("Application Usage", KaReports.GetApplicationUsageTable(connection)) &
		 "<br /><hr /><br />" &
		 "Current Database Version: " & KaSetting.GetSetting(connection, KaSetting.SN_DATABASE_VERSION, "0")

		Dim applications As New SortedDictionary(Of String, KaApplicationInformation)
		For Each applicationInformation As KaApplicationInformation In KaApplicationInformation.GetAll(connection, $"{KaApplicationInformation.FN_DELETED} = 0", $"{KaApplicationInformation.FN_PC_NAME},{KaApplicationInformation.FN_APPLICATION_NAME}")
			If applicationInformation.ApplicationName.Length = 0 Then applicationInformation.ApplicationName = applicationInformation.ProductName

			Dim pcAppName As String = (applicationInformation.ApplicationName & "@" & applicationInformation.PcName).ToUpper
			If Not applications.ContainsKey(pcAppName) Then applications.Add(pcAppName, applicationInformation)
		Next

		For Each application As KaSetting In KaSetting.GetAll(connection, "deleted=0 AND name LIKE " & Q("ApplicationVersionFor:%"), "name, value")
			Dim settings() As String = application.Name.Split(New Char() {":", "@"})
			Dim applicationInformation As KaApplicationInformation = New KaApplicationInformation With {
				.ApplicationName = settings(1),
				.PcName = settings(2)
			}
			Dim pcAppName As String = (applicationInformation.ApplicationName & "@" & applicationInformation.PcName).ToUpper
			If Not applications.ContainsKey(pcAppName) Then applications.Add(pcAppName, applicationInformation)
		Next
		For Each dbVersion As KaSetting In KaSetting.GetAll(connection, "deleted=0 AND name LIKE " & Q("ApplicationDatabaseVersionFor:%"), "name, value")
			Dim settings() As String = dbVersion.Name.Split(New Char() {":", "@"})
			Dim appVersInfo As KaApplicationInformation
			Dim pcAppName As String = (settings(1) & "@" & settings(2)).ToUpper
			If applications.ContainsKey(pcAppName) Then
				appVersInfo = applications(pcAppName)
			Else
				appVersInfo = New KaApplicationInformation
				appVersInfo.ApplicationName = settings(1)
				appVersInfo.PcName = settings(2)
				applications.Add(pcAppName, appVersInfo)
			End If
		Next
		lstApplicationPcUsage.Items.Clear()
		For Each appVers As String In applications.Keys
			With applications(appVers)
				lstApplicationPcUsage.Items.Add(New ListItem(.PcName & "/" & .ApplicationName, KaCommonObjects.XmlMethods.ToXml(applications(appVers))))
			End With
		Next
		pnlRemoveApplicationPcUsage.Visible = lstApplicationPcUsage.Items.Count > 0
	End Sub

	Private Sub btnRemoveApplicationPcUsage_Click(sender As Object, e As System.EventArgs) Handles btnRemoveApplicationPcUsage.Click
		If lstApplicationPcUsage.SelectedIndex >= 0 Then
			Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
			Dim applicationInformation As KaApplicationInformation = KaCommonObjects.XmlMethods.FromXml(lstApplicationPcUsage.SelectedItem.Value, GetType(KaApplicationInformation))
			If applicationInformation.ApplicationName.Length = 0 Then applicationInformation.ApplicationName = applicationInformation.ProductName

			Dim pcAppName As String = $"{applicationInformation.ApplicationName}@{applicationInformation.PcName}"
			Tm2Database.ExecuteNonQuery(connection, $"UPDATE {KaSetting.TABLE_NAME}  " &
						$"SET {KaSetting.FN_DELETED} = 1  " &
						$"WHERE {KaSetting.FN_DELETED} = 0  " &
						$"AND (({KaSetting.FN_NAME} = 'ApplicationVersionFor:{pcAppName}')  " &
							$"OR ({KaSetting.FN_NAME} = 'ApplicationDatabaseVersionFor:{pcAppName}')  " &
							$"OR ({KaSetting.FN_NAME} = 'ApplicationLastRunFor:{pcAppName}'))")

			Dim appName As String = applicationInformation.ApplicationName
			Dim pcName As String = applicationInformation.PcName
			For Each applicationInfo As KaApplicationInformation In KaApplicationInformation.GetAll(connection,
						$"({KaApplicationInformation.FN_PC_NAME} = {Q(pcName)}) " &
						$"AND ({KaApplicationInformation.FN_APPLICATION_NAME} = {Q(appName)} " &
							$"OR ({KaApplicationInformation.FN_APPLICATION_NAME}={Q("")} AND {KaApplicationInformation.FN_PRODUCT_NAME} = {Q(appName)}))", "")
				applicationInfo.Deleted = True
				applicationInfo.SqlUpdate(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
			Next
		End If
		PopulateApplicationList()
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
			Dim header As String = "Application Usage"
			Dim body As String = litReport.Text

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
					attachments.Add(New System.Net.Mail.Attachment(New IO.MemoryStream(Encoding.UTF8.GetBytes(newEmail.Body)), "ApplicationUsage.html", System.Net.Mime.MediaTypeNames.Text.Html))
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