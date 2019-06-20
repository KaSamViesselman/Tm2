Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb
Imports System.IO
Imports KaCommonObjects

Public Class AllContainerEquipment : Inherits System.Web.UI.Page
	Private _currentUser As KaUser
	Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
	Private _currentTableName As String = KaContainer.TABLE_NAME

#Region "Events"
	Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
		Response.Cache.SetCacheability(HttpCacheability.NoCache)
		_currentUser = Utilities.GetUser(Me)
		_currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Containers")
		If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")
		If Not Page.IsPostBack Then
			Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
			PopulateLocationList(connection)
			PopulateOwnerList(connection, _currentUser)
			PopulateCustomerAccountList(connection, _currentUser)
			PopulateEmailAddressList(connection)
		End If
	End Sub

	Protected Sub btnShowReport_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnShowReport.Click, btnPrinterFriendly.Click
		ClientScript.RegisterClientScriptBlock(Me.GetType(), "ShowReport", Utilities.JsWindowOpen("AllContainerEquipmentView.aspx?" &
			"sort_criteria=" & SortCriteria &
			IIf(Guid.Parse(ddlLocation.SelectedValue) <> Guid.Empty, "&location_id=" & ddlLocation.SelectedValue, "") &
			IIf(ddlStatus.SelectedIndex > 0, "&status=" & ddlStatus.SelectedValue, "") &
			IIf(Guid.Parse(ddlOwner.SelectedValue) <> Guid.Empty, "&owner_id=" & ddlOwner.SelectedValue, "") &
			IIf(Guid.Parse(ddlCustomerAccount.SelectedValue) <> Guid.Empty, "&customer_account_id=" & ddlCustomerAccount.SelectedValue, "") &
			IIf(tbxNumber.Text.Trim().Length > 0, "&number=" & tbxNumber.Text, "") &
			"&show_deleted=" & cbxShowDeleted.Checked.ToString() &
			"&media_type=" & IIf(sender Is btnPrinterFriendly, KaReports.MEDIA_TYPE_PFV, KaReports.MEDIA_TYPE_HTML)))
	End Sub

	Protected Sub btnDownload_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnDownload.Click
		Dim user As KaUser = Utilities.GetUser(Me)
		Dim connection As OleDbConnection = GetUserConnection(user.Id)
		Dim filename As String = String.Format("AllContainerEquipment{0:yyyyMMddHHmmss}.csv", Now)
		Dim writer As StreamWriter = Nothing
		Try ' to write the data to a file...
			writer = New FileOperations().WriteFile(DownloadDirectory(Me) & filename, New Alerts())
			writer.WriteLine(KaReports.GetTableCsv("", "", KaReports.GetContainerEquipmentTable(connection, KaReports.MEDIA_TYPE_COMMA, Conditions, SortCriteria, -1, -1)))
		Finally ' make sure to close the file...
			If Not writer Is Nothing Then writer.Close()
		End Try
		ClientScript.RegisterClientScriptBlock(Me.GetType(), "DownloadReport", Utilities.JsWindowOpen("./download/" & filename))
	End Sub

	Private Sub tbxEmailTo_TextChanged(sender As Object, e As System.EventArgs) Handles tbxEmailTo.TextChanged
		PopulateEmailAddressList(GetUserConnection(Utilities.GetUser(Me).Id))
	End Sub

	Protected Sub btnAddEmailAddress_Click(sender As Object, e As EventArgs) Handles btnAddEmailAddress.Click
		If ddlAddEmailAddress.SelectedIndex > 0 Then
			If tbxEmailTo.Text.Trim.Length > 0 Then tbxEmailTo.Text &= ", "
			tbxEmailTo.Text &= ddlAddEmailAddress.SelectedValue
			PopulateEmailAddressList(GetUserConnection(Utilities.GetUser(Me).Id))
		End If
	End Sub

	Protected Sub btnSendEmail_Click(sender As Object, e As EventArgs) Handles btnSendEmail.Click
		Dim user As KaUser = Utilities.GetUser(Me)
		Dim connection As OleDbConnection = GetUserConnection(user.Id)
		Dim message As String = ""
		If Not Utilities.IsEmailFieldValid(tbxEmailTo.Text, message) Then
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidEmail", Utilities.JsAlert(message))
			Exit Sub
		End If
		Dim emailAddresses As String = ""
		If tbxEmailTo.Text.Trim().Length > 0 Then
			Dim header As String = "All container equipment"
			Dim body As String = KaReports.GetTableHtml("", KaReports.GetContainerEquipmentTable(connection, KaReports.MEDIA_TYPE_HTML, Conditions, SortCriteria, -1, -1))
			Dim emailTo() As String = tbxEmailTo.Text.Split(New Char() {";", ","})
			For Each emailRecipient As String In emailTo
				If emailRecipient.Trim.Length > 0 Then
					Dim newEmail As New KaEmail()
					newEmail.ApplicationId = APPLICATION_ID
					newEmail.Body = Utilities.CreateSiteCssStyle() & body
					newEmail.BodyIsHtml = True
					newEmail.OwnerID = user.OwnerId
					newEmail.Recipients = emailRecipient.Trim
					newEmail.ReportType = KaEmailReport.ReportTypes.Generic
					newEmail.Subject = header
					Dim attachments As New List(Of System.Net.Mail.Attachment)
					attachments.Add(New System.Net.Mail.Attachment(New IO.MemoryStream(Encoding.UTF8.GetBytes(newEmail.Body)), "AllContainerEquipmentReport.html", System.Net.Mime.MediaTypeNames.Text.Html))
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
#End Region

	Private ReadOnly Property Conditions As String
		Get
			Return KaReports.GetContainerEquipmentConditions(GetUserConnection(Utilities.GetUser(Me).Id), Guid.Parse(ddlLocation.SelectedValue), Guid.Parse(ddlOwner.SelectedValue), Guid.Parse(ddlCustomerAccount.SelectedValue), Integer.Parse(ddlStatus.SelectedValue), tbxNumber.Text, cbxShowDeleted.Checked)
		End Get
	End Property

	Private ReadOnly Property SortCriteria As String
		Get
			Return String.Format("{0} {1}", ddlOrderBy.SelectedValue, ddlAscDesc.SelectedValue)
		End Get
	End Property

	Private Sub PopulateLocationList(connection As OleDbConnection)
		For Each location As KaLocation In KaLocation.GetAll(connection, "deleted=0", "name ASC")
			ddlLocation.Items.Add(New ListItem(location.Name, location.Id.ToString()))
		Next
	End Sub

	Private Sub PopulateOwnerList(connection As OleDbConnection, user As KaUser)
		Dim conditions As String = "deleted=0"
		If user.OwnerId <> Guid.Empty Then ' this user is limited to a single owner
			ddlOwner.Items.Clear() ' get rid of the "All owners" option
			conditions &= String.Format(" AND id={0}", Q(user.OwnerId))
		End If
		For Each owner As KaOwner In KaOwner.GetAll(connection, conditions, "name ASC")
			ddlOwner.Items.Add(New ListItem(owner.Name, owner.Id.ToString()))
		Next
	End Sub

	Private Sub PopulateCustomerAccountList(connection As OleDbConnection, user As KaUser)
		Dim conditions As String = "deleted=0 AND is_supplier=0" & IIf(_currentUser.OwnerId <> Guid.Empty, String.Format(" AND (owner_id={0} OR owner_id={1})", Q(Guid.Empty), Q(_currentUser.OwnerId)), "")
		For Each customerAccount As KaCustomerAccount In KaCustomerAccount.GetAll(connection, conditions, "name ASC")
			ddlCustomerAccount.Items.Add(New ListItem(customerAccount.Name, customerAccount.Id.ToString()))
		Next
	End Sub

	Private Sub PopulateEmailAddressList(connection As OleDbConnection)
		Utilities.PopulateEmailAddressList(tbxEmailTo, ddlAddEmailAddress, btnAddEmailAddress)
		rowAddAddress.Visible = ddlAddEmailAddress.Items.Count > 1
	End Sub
End Class