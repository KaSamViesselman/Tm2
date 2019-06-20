﻿Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class BulkProductAllocation
	Inherits System.Web.UI.Page

	Private _currentUser As KaUser
	Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
	Private _currentTableName As String = KaProduct.TABLE_NAME

#Region "Events"
	Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
		Response.Cache.SetCacheability(HttpCacheability.NoCache)
		_currentUser = Utilities.GetUser(Me)
		_currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Products")
		If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")

		If Not Page.IsPostBack Then
			PopulateFacilities()
			PopulateAccounts()
			PopulateBulkProducts()
			PopulateDisplayUnits()
			PopulateEmailAddressList()
		End If
		litEmailConfirmation.Text = ""
	End Sub

	Protected Sub btnPrinterFriendly_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnPrinterFriendly.Click
		ClientScript.RegisterClientScriptBlock(Me.GetType(), "ShowReport", Utilities.JsWindowOpen("BulkProductAllocationView.aspx?facility_id=" & ddlFacility.SelectedValue & "&account_id=" & ddlAccounts.SelectedValue & "&bulk_product_id=" & ddlBulkProducts.SelectedValue & "&display_unit_id=" & ddlDisplayUnit.SelectedValue))
	End Sub

	Protected Sub btnDownload_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnDownload.Click
		If Guid.Parse(ddlFacility.SelectedValue) = Guid.Empty Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidFacility", Utilities.JsAlert("A facility must be selected.")) : Return

		'Build subtitle
		Dim subTitle As String = "Facility: " & New KaLocation(GetUserConnection(_currentUser.Id), Guid.Parse(ddlFacility.SelectedValue)).Name

		If Guid.Parse(ddlAccounts.SelectedValue) <> Guid.Empty Then
			subTitle &= ", Customer: " & New KaCustomerAccount(GetUserConnection(_currentUser.Id), Guid.Parse(ddlAccounts.SelectedValue)).Name
		End If

		If Guid.Parse(ddlBulkProducts.SelectedValue) <> Guid.Empty Then
			subTitle &= ", Bulk Product: " & New KaBulkProduct(GetUserConnection(_currentUser.Id), Guid.Parse(ddlBulkProducts.SelectedValue)).Name
		End If

		If Guid.Parse(ddlDisplayUnit.SelectedValue) <> Guid.Empty Then
			subTitle &= ", Displaying in: " & New KaUnit(GetUserConnection(_currentUser.Id), Guid.Parse(ddlDisplayUnit.SelectedValue)).Name & "."
		Else
			subTitle &= ", Displaying in bulk product default unit of measure."
		End If

		Dim fileName As String = String.Format("BulkProductAllocation{0:yyyyMMddHHmmss}.csv", Now)
		KaReports.CreateCsvFromTable("Bulk Product Allocation", subTitle, KaReports.GetBulkProductAllocationTable(GetUserConnection(_currentUser.Id), Guid.Parse(ddlAccounts.SelectedValue), Guid.Parse(ddlBulkProducts.SelectedValue), Guid.Parse(ddlDisplayUnit.SelectedValue), Guid.Parse(ddlFacility.SelectedValue)), DownloadDirectory(Me) & fileName)
		ClientScript.RegisterClientScriptBlock(Me.GetType(), "DownloadReport", Utilities.JsWindowOpen("./download/" & fileName))
	End Sub

	Private Sub btnRefresh_Click(sender As Object, e As System.EventArgs) Handles btnShow.Click
		RefreshTable()
	End Sub
#End Region

	Private Sub PopulateFacilities()
		ddlFacility.Items.Clear()
		For Each location As KaLocation In KaLocation.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
			ddlFacility.Items.Add(New ListItem(location.Name, location.Id.ToString()))
		Next
		If ddlFacility.Items.Count > 0 Then ddlFacility.SelectedIndex = 0
	End Sub

	Private Sub PopulateAccounts()
		ddlAccounts.Items.Clear()
		Dim li As ListItem = New ListItem
        li.Text = "All accounts"
        li.Value = Guid.Empty.ToString
		ddlAccounts.Items.Add(li)
		For Each account As KaCustomerAccount In KaCustomerAccount.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND is_supplier=0" & IIf(_currentUser.OwnerId.Equals(Guid.Empty), "", String.Format(" AND (owner_id={0} OR owner_id={1})", Q(Guid.Empty), Q(_currentUser.OwnerId))), "name asc")
			li = New ListItem
			li.Text = account.Name
			li.Value = account.Id.ToString
			ddlAccounts.Items.Add(li)
		Next
	End Sub

	Private Sub PopulateBulkProducts()
		ddlBulkProducts.Items.Clear()
		Dim li As ListItem = New ListItem
        li.Text = "All bulk products"
        li.Value = Guid.Empty.ToString
		ddlBulkProducts.Items.Add(li)
		For Each product As KaBulkProduct In KaBulkProduct.GetAll(GetUserConnection(_currentUser.Id), "deleted = 0" & IIf(_currentUser.OwnerId.Equals(Guid.Empty), "", String.Format(" AND (owner_id={0} OR owner_id={1})", Q(Guid.Empty), Q(_currentUser.OwnerId))), "name asc")
			If Not product.IsFunction(GetUserConnection(_currentUser.Id)) Then
				li = New ListItem
				li.Text = product.Name
				li.Value = product.Id.ToString
				ddlBulkProducts.Items.Add(li)
			End If
		Next
	End Sub

	Private Sub PopulateDisplayUnits()
		ddlDisplayUnit.Items.Clear()
		Dim li As ListItem = New ListItem
        li.Text = "Bulk product default"
        li.Value = Guid.Empty.ToString
		ddlDisplayUnit.Items.Add(li)
		For Each u As KaUnit In KaUnit.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND base_unit<>" & KaUnit.Unit.Seconds, "name asc")
			li = New ListItem
			li.Text = u.Name
			li.Value = u.Id.ToString
			ddlDisplayUnit.Items.Add(li)
		Next
	End Sub

	Private Sub RefreshTable()
		If Guid.Parse(ddlFacility.SelectedValue) = Guid.Empty Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidFacility", Utilities.JsAlert("A facility must be selected.")) : Return

		'Build subtitle
		Dim subTitle As String = GetSubTitle()

		litReport.Text = KaReports.GetTableHtml("Assigned Bulk Products", subTitle, KaReports.GetBulkProductAllocationTable(GetUserConnection(_currentUser.Id), Guid.Parse(ddlAccounts.SelectedValue), Guid.Parse(ddlBulkProducts.SelectedValue), Guid.Parse(ddlDisplayUnit.SelectedValue), Guid.Parse(ddlFacility.SelectedValue)))
	End Sub

	Private Function GetSubTitle() As String
		Dim subTitle As String = "Facility: " & New KaLocation(GetUserConnection(_currentUser.Id), Guid.Parse(ddlFacility.SelectedValue)).Name

		If Guid.Parse(ddlAccounts.SelectedValue) <> Guid.Empty Then
			subTitle &= ", Customer: " & New KaCustomerAccount(GetUserConnection(_currentUser.Id), Guid.Parse(ddlAccounts.SelectedValue)).Name
		End If

		If Guid.Parse(ddlBulkProducts.SelectedValue) <> Guid.Empty Then
			subTitle &= ", Bulk Product: " & New KaBulkProduct(GetUserConnection(_currentUser.Id), Guid.Parse(ddlBulkProducts.SelectedValue)).Name
		End If

		If Guid.Parse(ddlDisplayUnit.SelectedValue) <> Guid.Empty Then
			subTitle &= ", Displaying in: " & New KaUnit(GetUserConnection(_currentUser.Id), Guid.Parse(ddlDisplayUnit.SelectedValue)).Name & "."
		Else
			subTitle &= ", Displaying in bulk product default unit of measure."
		End If
		Return subTitle
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
		If litReport.Text.Trim.Length = 0 Then Exit Sub
		Dim message As String = ""
		If Not Utilities.IsEmailFieldValid(tbxEmailTo.Text, message) Then
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidEmail", Utilities.JsAlert(message))
			Exit Sub
		End If
		Dim emailAddresses As String = ""
		If tbxEmailTo.Text.Trim().Length > 0 Then
			Dim header As String = "Bulk Product Allocation - " & GetSubTitle()
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
					attachments.Add(New System.Net.Mail.Attachment(New IO.MemoryStream(Encoding.UTF8.GetBytes(newEmail.Body)), "BulkProductAllocation.html", System.Net.Mime.MediaTypeNames.Text.Html))
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