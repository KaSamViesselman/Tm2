Imports KahlerAutomation.KaTm2Database

Public Class TransportUsageReport : Inherits System.Web.UI.Page
	Private _currentUser As KaUser = Nothing
	Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing

#Region "Events"
	Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load, Me.Load
		Response.Cache.SetCacheability(HttpCacheability.NoCache)
		_currentUser = Utilities.GetUser(Me)
		_currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaTransport.TABLE_NAME}), "Transports")
		If Not _currentUserPermission(KaTransport.TABLE_NAME).Read Then Response.Redirect("Welcome.aspx")
		If Not Page.IsPostBack Then
			tbxFromDate.Value = Now
			tbxToDate.Value = Now
			PopulateCustomerList()
			PopulateTransportList()
			PopulateEmailAddressList()
		End If
		litEmailConfirmation.Text = ""
	End Sub

	Protected Sub lbPrinterFriendlyVersion_Click(sender As Object, e As EventArgs) Handles btnPrinterFriendly.Click
		Dim fromDate As DateTime
		Dim toDate As DateTime
		If ValidateOptions(fromDate, toDate) Then
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "ShowReport", Utilities.JsWindowOpen(String.Format("TransportUsageReportView.aspx?start_date={0}&end_date={1}&customer_account_id={2}&transport_id={3}", fromDate, toDate, ddlCustomer.SelectedValue, ddlTransport.SelectedValue)))
		End If
	End Sub
#End Region

	Private Sub PopulateCustomerList()
		ddlCustomer.Items.Clear()
		ddlCustomer.Items.Add(New ListItem("All customers", Guid.Empty.ToString()))
		Dim conditions As String = "deleted=0 AND is_supplier=0" & IIf(_currentUser.OwnerId <> Guid.Empty, String.Format(" AND (owner_id={0} OR owner_id={1})", Q(Guid.Empty), Q(_currentUser.OwnerId)), "")
		For Each customerAccount As KaCustomerAccount In KaCustomerAccount.GetAll(GetUserConnection(_currentUser.Id), conditions, "name ASC")
			ddlCustomer.Items.Add(New ListItem(customerAccount.Name, customerAccount.Id.ToString()))
		Next
	End Sub

	Private Sub PopulateTransportList()
		ddlTransport.Items.Clear()
		ddlTransport.Items.Add(New ListItem("All transports", Guid.Empty.ToString()))
		For Each transport As KaTransport In KaTransport.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
			ddlTransport.Items.Add(New ListItem(transport.Name, transport.Id.ToString()))
		Next
	End Sub

	Protected Sub btnCreateReport_Click(sender As Object, e As EventArgs) Handles btnCreateReport.Click
		litReport.Text = ""
		Dim fromDate As DateTime
		Dim toDate As DateTime
		If ValidateOptions(fromDate, toDate) Then
			'We are valid at this point, run report
			RunReport(fromDate, toDate)
		End If
	End Sub

	Private Sub RunReport(ByVal fromDate As DateTime, ByVal toDate As DateTime)
		litReport.Text = KaReports.GetTableHtml(String.Format("Transport usage report ({0:d} to {1:d})", fromDate, toDate), KaReports.GetTransportUsageReportTable(GetUserConnection(_currentUser.Id), fromDate, toDate, Guid.Parse(ddlCustomer.SelectedValue), Guid.Parse(ddlTransport.SelectedValue)))
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
			Dim emailAddresses As String = ""
			Dim header As String = String.Format("Transport usage report ({0:d} to {1:d})", fromDate, toDate)
			Dim body As String = KaReports.GetTableHtml(header, KaReports.GetTransportUsageReportTable(GetUserConnection(_currentUser.Id), fromDate, toDate, Guid.Parse(ddlCustomer.SelectedValue), Guid.Parse(ddlTransport.SelectedValue)))

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
					attachments.Add(New System.Net.Mail.Attachment(New IO.MemoryStream(Encoding.UTF8.GetBytes(newEmail.Body)), "TransportUsageReport.html", System.Net.Mime.MediaTypeNames.Text.Html))
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