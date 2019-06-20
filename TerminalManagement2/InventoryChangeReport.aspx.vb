Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb
Imports System.Math
Public Class InventoryChangeReport : Inherits System.Web.UI.Page

	Private _currentUser As KaUser
	Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
	Private _currentTableName As String = KaBulkProductInventory.TABLE_NAME

#Region "Events"
	Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
		Response.Cache.SetCacheability(HttpCacheability.NoCache)
		_currentUser = Utilities.GetUser(Me)
		_currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Inventory")
		If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")
		If Not Page.IsPostBack Then
			PopulateBulkProductList()
			PopulateLocationList()
			PopulateOwnerList()
			PopulateAdditionalUnitsList()
			Dim minYear As Integer
			Dim maxYear As Integer
			tbxFromDate.Value = New DateTime(Now.Year, Now.Month, Now.Day, 0, 0, 0) ' setting default values for datetimepicker
			tbxToDate.Value = New DateTime(Now.Year, Now.Month, Now.Day, 0, 0, 0).AddDays(1)
			GetYearRange(_currentUser, minYear, maxYear)
			PopulateEmailAddressList()
		End If
		litEmailConfirmation.Text = ""
	End Sub
#End Region

	Private Sub PopulateBulkProductList()
		ddlBulkProduct.Items.Clear()
		ddlBulkProduct.Items.Add(New ListItem("All bulk products", Guid.Empty.ToString()))
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim rdr As OleDbDataReader = Tm2Database.ExecuteReader(connection, "SELECT id, name, 'B' AS recordType, UPPER(name) AS upperName FROM bulk_products WHERE deleted = 0 UNION SELECT id, name, 'G' AS recordType, UPPER(name) AS upperName FROM inventory_groups WHERE deleted = 0 ORDER BY recordType, upperName")
		Do While rdr.Read
			If rdr.Item("recordType") = "B" Then
				Dim r As New KaBulkProduct(connection, rdr.Item("id"))
				If Not r.IsFunction(connection) Then
					ddlBulkProduct.Items.Add(New ListItem(r.Name, r.Id.ToString()))
				End If
			ElseIf rdr.Item("recordType") = "G" Then
				ddlBulkProduct.Items.Add(New ListItem(rdr.Item("name") & " (Grouped inventory)", rdr.Item("id").ToString()))
			End If
		Loop
	End Sub

	Private Sub PopulateLocationList()
		ddlLocation.Items.Clear()
		ddlLocation.Items.Add(New ListItem("All facilities", Guid.Empty.ToString()))
		For Each r As KaLocation In KaLocation.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
			ddlLocation.Items.Add(New ListItem(r.Name, r.Id.ToString()))
		Next
	End Sub

	Private Sub PopulateOwnerList()
		ddlOwner.Items.Clear()
		If _currentUser.OwnerId = Guid.Empty Then ddlOwner.Items.Add(New ListItem("All owners", Guid.Empty.ToString()))
		For Each r As KaOwner In KaOwner.GetAll(GetUserConnection(_currentUser.Id), "deleted=0" & IIf(_currentUser.OwnerId = Guid.Empty, "", " AND id=" + Q(_currentUser.OwnerId)), "name ASC")
			ddlOwner.Items.Add(New ListItem(r.Name, r.Id.ToString()))
		Next
	End Sub

	Private Sub PopulateAdditionalUnitsList()
		cblAdditionalUnits.Items.Clear()
		For Each n As KaUnit In KaUnit.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
			If n.BaseUnit <> KaUnit.Unit.Pulses AndAlso n.BaseUnit <> KaUnit.Unit.Seconds Then
				cblAdditionalUnits.Items.Add(New ListItem(n.Name, n.Id.ToString()))
				cblAdditionalUnits.Items(cblAdditionalUnits.Items.Count - 1).Selected = False
			End If
		Next
	End Sub

	Private Sub GetYearRange(ByVal currentUser As KaUser, ByRef minYear As Integer, ByRef maxYear As Integer)
		Dim ownerId As Guid = Guid.Parse(ddlOwner.SelectedValue)
		Dim locationId As Guid = Guid.Parse(ddlLocation.SelectedValue)
		Dim bulkProductId As Guid = Guid.Parse(ddlBulkProduct.SelectedValue)
		Dim conditions As String = ""
		If ownerId <> Guid.Empty Then conditions &= IIf(conditions.Length > 0, " AND ", "") & String.Format("owner_id={0}", Q(ownerId))
		If locationId <> Guid.Empty Then conditions &= IIf(conditions.Length > 0, " AND ", "") & String.Format("location_id={0}", Q(locationId))
		If bulkProductId <> Guid.Empty Then conditions &= IIf(conditions.Length > 0, " AND ", "") & String.Format("bulk_product_id={0}", Q(bulkProductId))
		Dim r As OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(currentUser.Id), "SELECT MIN(applied_at), MAX(applied_at) FROM inventory_changes" & IIf(conditions.Length > 0, " WHERE " & conditions, ""))
		If r.Read() Then
			minYear = IsNull(r(0), Now).Year
			maxYear = IsNull(r(1), Now).Year
		End If
		r.Close()
	End Sub

	Protected Sub btnShowReport_Click(sender As Object, e As EventArgs) Handles btnShowReport.Click
		Dim fromDate As DateTime
		Try
			fromDate = GetFromDate()
		Catch ex As FormatException
			Return
		End Try
		Dim toDate As DateTime
		Try
			toDate = GetToDate()
		Catch ex As FormatException
			Return
		End Try
		If fromDate > toDate Then ' check to make sure "From" date isn't later then "To" date
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidStartEndDate", Utilities.JsAlert("Please select an ending date (To) that is later than the beginning date (From)"))
		Else
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "ShowReport", Utilities.JsWindowOpen("InventoryChangeReportView.aspx?OwnerId=" & ddlOwner.SelectedValue & "&LocationId=" & ddlLocation.SelectedValue & "&BulkProductId=" & ddlBulkProduct.SelectedValue & "&FromDate=" & fromDate & "&ToDate=" & toDate & "&AdditionalUnits=" & GetAdditionalUnitsString()))
		End If
	End Sub

	Protected Sub btnDownload_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnDownload.Click
		Dim fileName As String = String.Format("InventoryChange{0:yyyyMMddHHmmss}.csv", Now)
		Dim ownerId As Guid = Guid.Parse(ddlOwner.SelectedValue)
		Dim locationId As Guid = Guid.Parse(ddlLocation.SelectedValue)
		Dim bulkProductId As Guid = Guid.Parse(ddlBulkProduct.SelectedValue)
		Dim fromDate As DateTime
		Try
			fromDate = GetFromDate()
		Catch ex As FormatException
			Return
		End Try
		Dim toDate As DateTime
		Try
			toDate = GetToDate()
		Catch ex As FormatException
			Return
		End Try
		If fromDate > toDate Then ' check to make sure "From" date isn't later then "To" date
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidStartEndDate", Utilities.JsAlert("Please select an ending date (To) that is later than the beginning date (From)"))
		Else
			KaReports.CreateCsvFromTable("Inventory Change - " & fromDate.ToString() & " to " & toDate.ToString(), KaReports.GetInventoryChangeTable(GetUserConnection(_currentUser.Id), ownerId, locationId, bulkProductId, fromDate, toDate, GetAdditionalUnits(), KaReports.MEDIA_TYPE_COMMA), DownloadDirectory(Me) & fileName)
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "DownloadReport", Utilities.JsWindowOpen("./download/" & fileName))
		End If
	End Sub

	Private Function GetFromDate() As DateTime
		Dim fromDate As DateTime
		Try ' check for any improper entry in "From" textbox
			fromDate = DateTime.Parse(tbxFromDate.Value) ' converting string value to datetime value for comparison in IF statement
		Catch ex As FormatException
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidStartDate", Utilities.JsAlert("Please enter a valid date for the beginning (From) date"))
			Throw
		End Try
		Return fromDate
	End Function

	Private Function GetToDate() As DateTime
		Dim toDate As DateTime
		Try ' check for any improper entry in "To" textbox
			toDate = DateTime.Parse(tbxToDate.Value) ' converting string value to datetime value for comparison in IF statement
		Catch ex As FormatException
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidStartDate", Utilities.JsAlert("Please enter a valid date for the ending (To) date"))
			Throw
		End Try
		Return toDate
	End Function

	Protected Sub btnSendEmail_Click(sender As Object, e As EventArgs) Handles btnSendEmail.Click
		Dim message As String = ""
		If Not Utilities.IsEmailFieldValid(tbxEmailTo.Text, message) Then
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidEmail", Utilities.JsAlert(message))
			Exit Sub
		End If
		Dim emailAddresses As String = ""
		If tbxEmailTo.Text.Trim().Length > 0 Then
			Dim ownerId As Guid = Guid.Parse(ddlOwner.SelectedValue)
			Dim locationId As Guid = Guid.Parse(ddlLocation.SelectedValue)
			Dim bulkProductId As Guid = Guid.Parse(ddlBulkProduct.SelectedValue)
			Dim fromDate As DateTime
			Try
				fromDate = GetFromDate()
			Catch ex As FormatException
				Return
			End Try
			Dim toDate As DateTime
			Try
				toDate = GetToDate()
			Catch ex As FormatException
				Return
			End Try
			If fromDate > toDate Then ' check to make sure "From" date isn't later then "To" date
				ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidStartEndDate", Utilities.JsAlert("Please select an ending date (To) that is later than the beginning date (From)"))
				Exit Sub
			Else
				Dim header As String = "Inventory Change - " & fromDate.ToString() & " to " & toDate.ToString()
				Dim body As String = KaReports.GetTableHtml(header, KaReports.GetInventoryChangeTable(GetUserConnection(_currentUser.Id), ownerId, locationId, bulkProductId, fromDate, toDate, GetAdditionalUnits(), KaReports.MEDIA_TYPE_HTML))

				Dim emailTo() As String = tbxEmailTo.Text.Split(New Char() {";", ","})
				For Each emailRecipient As String In emailTo
					If emailRecipient.Trim.Length > 0 Then
						Dim newEmail As New KaEmail()
						newEmail.ApplicationId = APPLICATION_ID
						newEmail.Body = Utilities.CreateSiteCssStyle() & body
						newEmail.BodyIsHtml = True
						newEmail.OwnerID = _currentUser.OwnerId
						newEmail.Recipients = emailRecipient.Trim
						newEmail.ReportType = KaEmailReport.ReportTypes.InventoryChangeReport
						newEmail.Subject = header
						Dim attachments As New List(Of System.Net.Mail.Attachment)
						attachments.Add(New System.Net.Mail.Attachment(New IO.MemoryStream(Encoding.UTF8.GetBytes(newEmail.Body)), "InventoryChangeReport.html", System.Net.Mime.MediaTypeNames.Text.Html))
						newEmail.SerializeAttachments(attachments)
						KaEmail.CreateEmail(GetUserConnection(_currentUser.Id), newEmail, -1, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
						If emailAddresses.Length > 0 Then emailAddresses &= ", "
						emailAddresses &= newEmail.Recipients
					End If
				Next
			End If
			If emailAddresses.Length > 0 Then
				litEmailConfirmation.Text = "Report sent to " & emailAddresses
			Else
				litEmailConfirmation.Text = "Report not sent.  No e-mail addresses."
			End If
		End If
	End Sub

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

	Private Function GetAdditionalUnitsString() As String
		Dim retval As String = ""
		For Each li As ListItem In cblAdditionalUnits.Items
			If li.Selected Then
				retval &= IIf(retval.Length > 0, ",", "") & li.Value
			End If
		Next
		Return retval
	End Function

	Private Function GetAdditionalUnits() As List(Of Guid)
		Dim retval As List(Of Guid) = New List(Of Guid)
		For Each li As ListItem In cblAdditionalUnits.Items
			If li.Selected Then
				retval.Add(Guid.Parse(li.Value))
			End If
		Next
		Return retval
	End Function
End Class