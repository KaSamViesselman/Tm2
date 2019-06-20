Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb
Imports System.Collections.Generic

Public Class PanelBulkProducts : Inherits System.Web.UI.Page
	Private _currentUser As KaUser
	Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
	Private Const DISABLED_CHECKBOX_NAME As String = "cbxDisabled{0}"
	Private Const DISABLED_CHECKBOX_FOR_OTHER_PANEL_NAME As String = "cbxDisabledOther{0}_{1}"
	Private _panelFunctionNames As New Dictionary(Of Integer, String)
	Private _printerFriendlyViewSelected As Boolean = False
	Private _bulkProductId As Guid = Guid.Empty
	Private _facilityId As Guid = Guid.Empty
	Private _panelId As Guid = Guid.Empty
	Private _isReadOnly As Boolean = True

#Region "Events"
	Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
		Response.Cache.SetCacheability(HttpCacheability.NoCache)
		_currentUser = Utilities.GetUser(Me)
		_currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaBulkProductPanelSettings.TABLE_NAME}), "PanelBulkProductSettings")
		If Not _currentUserPermission(KaBulkProductPanelSettings.TABLE_NAME).Read Then Response.Redirect("Welcome.aspx")

		lblBulkProductStatus.Text = ""
		litEmailConfirmation.Text = ""
		If Page.Request("media_type") IsNot Nothing Then _printerFriendlyViewSelected = (Page.Request("media_type").ToUpper = KaReports.MEDIA_TYPE_PFV.ToUpper)
		_isReadOnly = _printerFriendlyViewSelected OrElse Not _currentUserPermission(KaBulkProductPanelSettings.TABLE_NAME).Edit
		pnlRecordControl.Visible = Not _printerFriendlyViewSelected
		pnlFilters.Visible = Not _printerFriendlyViewSelected
		pnlSendEmail.Visible = Not _printerFriendlyViewSelected
		PopulatePanelFunctionNames()
		If Not Page.IsPostBack Then
			' Initialize the bulk product dropdown value
			If Page.Request("bulk_product_id") IsNot Nothing Then Guid.TryParse(Page.Request("bulk_product_id"), _bulkProductId)
			ddlBulkProduct.Items.Add(New ListItem("", _bulkProductId.ToString()))
			ddlBulkProduct.SelectedValue = _bulkProductId.ToString()

			' Initialize the panel dropdown value
			If Page.Request("panel_id") IsNot Nothing Then Guid.TryParse(Page.Request("panel_id"), _panelId)
			ddlPanel.Items.Add(New ListItem("", _panelId.ToString()))
			ddlPanel.SelectedValue = _panelId.ToString()

			' Initialize the facility dropdown value
			If Page.Request("location_id") IsNot Nothing Then
				Guid.TryParse(Page.Request("location_id"), _facilityId)
			ElseIf Not _printerFriendlyViewSelected Then
				Guid.TryParse(KaSetting.GetSetting(GetUserConnection(_currentUser.Id), "PanelsPage:" & _currentUser.Id.ToString & "/LastFacilityUsed", Guid.Empty.ToString()), _facilityId)
			End If
			ddlFacility.Items.Add(New ListItem("", _facilityId.ToString()))
			ddlFacility.SelectedValue = _facilityId.ToString()
			Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
			PopulateBulkProducts(connection)
			PopulateLocations(connection)
			PopulatePanels(connection)
			PopulateEmailAddressList()
			PopulatePanelBulkProducts()
		End If
	End Sub

	Protected Sub cbxDisabled_CheckedChanged(sender As Object, e As EventArgs)
		Dim cb As CheckBox = sender
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim id As Guid = Guid.Empty
		Dim panelId As Guid = Guid.Empty

		Dim otherStartIndex As Integer = DISABLED_CHECKBOX_FOR_OTHER_PANEL_NAME.IndexOf("{")
		If cb.ID.StartsWith(DISABLED_CHECKBOX_FOR_OTHER_PANEL_NAME.Substring(0, otherStartIndex - 1)) Then
			Dim panelStartIndex As Integer = cb.ID.IndexOf("_")
			Guid.TryParse(cb.ID.Substring(otherStartIndex, panelStartIndex - otherStartIndex), id)
			panelStartIndex += 1
			Guid.TryParse(cb.ID.Substring(panelStartIndex, cb.ID.Length - panelStartIndex), panelId)
		Else
			Dim startIndex As Integer = DISABLED_CHECKBOX_NAME.IndexOf("{")
			Guid.TryParse(cb.ID.Substring(startIndex, cb.ID.Length - startIndex), id)
		End If

		Dim bpps As New KaBulkProductPanelSettings(connection, id)
		If Not panelId.Equals(Guid.Empty) AndAlso Not bpps.PanelId.Equals(panelId) Then ' Need to insert a new record for this panel, since the setting used was for a different panel
			bpps.PanelId = panelId
			bpps.SqlInsert(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
		End If
		Dim disable As Boolean = cb.Checked
		If (Not disable) Then 'enable
			Dim enabledSettings As ArrayList = KaBulkProductPanelSettings.GetAll(connection, "deleted = 0 AND disabled = 0 AND bulk_product_id = " & Q(bpps.BulkProductId) & " AND panel_id = " & Q(bpps.PanelId) & " ", "")
			For Each bulkProductPanelSetting As KaBulkProductPanelSettings In enabledSettings
				bulkProductPanelSetting.Disabled = True
				bulkProductPanelSetting.SqlUpdate(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
			Next
		End If
		bpps.Disabled = disable
		bpps.SqlUpdate(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
		Try
			lblBulkProductStatus.Text = String.Format("Setting {0} for {1}.", IIf(disable, "disabled", "enabled"), New KaBulkProduct(connection, bpps.BulkProductId).Name)
		Catch ex As RecordNotFoundException

		End Try
		PopulatePanelBulkProducts()
		'Response.Redirect(HttpContext.Current.Request.Url.ToString(), True)
	End Sub
#End Region

	Private _originalbulkProductPanelSettings As List(Of KaBulkProductPanelSettings)
	Private _panelReferencedSettings As List(Of KaBulkProductPanelSettings)

	Private Sub ConvertSettingsToTable()
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		PopulatePanelFunctionNames()

		Do While tblPanelBulkProducts.Rows.Count > 1
			tblPanelBulkProducts.Rows.RemoveAt(1)
		Loop
		Dim units As New ArrayList
		Dim bulkProductIdList As New List(Of Guid)
		Dim isBulkProductFunction As New Dictionary(Of Guid, Boolean) ' Guid = bulk product ID
		Dim bulkProducts As New Dictionary(Of Guid, KaBulkProduct) ' Guid = bulk product ID
		Dim panels As New Dictionary(Of Guid, KaPanel)
		Dim originalSettings As Dictionary(Of Guid, KaBulkProductPanelSettings) = New Dictionary(Of Guid, KaBulkProductPanelSettings)
		For Each bulkProdPanelSetting As KaBulkProductPanelSettings In _originalbulkProductPanelSettings
			If Not originalSettings.ContainsKey(bulkProdPanelSetting.Id) Then originalSettings.Add(bulkProdPanelSetting.Id, bulkProdPanelSetting)
		Next
		Dim panelSettings As Dictionary(Of Guid, Dictionary(Of Guid, List(Of KaBulkProductPanelSettings))) = New Dictionary(Of Guid, Dictionary(Of Guid, List(Of KaBulkProductPanelSettings)))
		For Each bulkProdPanelSetting As KaBulkProductPanelSettings In _panelReferencedSettings
			If Not panelSettings.ContainsKey(bulkProdPanelSetting.PanelId) Then panelSettings.Add(bulkProdPanelSetting.PanelId, New Dictionary(Of Guid, List(Of KaBulkProductPanelSettings)))
			If Not panelSettings(bulkProdPanelSetting.PanelId).ContainsKey(bulkProdPanelSetting.BulkProductId) Then panelSettings(bulkProdPanelSetting.PanelId).Add(bulkProdPanelSetting.BulkProductId, New List(Of KaBulkProductPanelSettings))
			panelSettings(bulkProdPanelSetting.PanelId)(bulkProdPanelSetting.BulkProductId).Add(originalSettings(bulkProdPanelSetting.Id)) ' Add in the original setting, not the setting with the possible mismatched panelId
		Next

		For Each panelId As Guid In panelSettings.Keys
			Dim panelDisplayed As KaPanel = KaPanel.GetPanel(connection, Nothing, panels, panelId)
			If panelDisplayed IsNot Nothing Then
				Dim settings As Dictionary(Of Guid, List(Of KaBulkProductPanelSettings)) = panelSettings(panelId)
				For Each bulkProductId As Guid In settings.Keys
					Dim bulkProduct As KaBulkProduct
					If Not bulkProducts.ContainsKey(bulkProductId) Then
						Try
							bulkProducts.Add(bulkProductId, New KaBulkProduct(connection, bulkProductId))
						Catch ex As RecordNotFoundException
							bulkProduct = New KaBulkProduct
							bulkProduct.Name = "?"
							bulkProduct.Id = bulkProductId
							bulkProducts.Add(bulkProductId, bulkProduct)
						End Try
					End If
					bulkProduct = bulkProducts(bulkProductId)
					For Each bpps As KaBulkProductPanelSettings In settings(bulkProductId)
						Dim panel As KaPanel
						Try
							panel = KaPanel.GetPanel(connection, Nothing, panels, bpps.PanelId)

							Dim isFunction As Boolean
							Try ' to determine if the bulk product is a function by looking it up in the dictionary...
								isFunction = isBulkProductFunction(bpps.BulkProductId)
							Catch ex As KeyNotFoundException ' the bulk product wasn't found in the dictionary...
								Try ' to load the bulk product from the database...
									isFunction = bulkProduct.IsFunction(connection)
								Catch ex2 As Exception ' the bulk product wasn't in the dictionary...
									isFunction = False
								End Try
								isBulkProductFunction(bpps.BulkProductId) = isFunction ' store in the dictionary
							End Try
							If Not isFunction Then ' only show non-function bulk products
								Dim row As New HtmlTableRow()
								Dim cell As New HtmlTableCell()
								cell.InnerText = bulkProduct.Name
								row.Cells.Add(cell)
								cell = New HtmlTableCell()
								cell.InnerText = _panelFunctionNames.Item(bpps.ProductNumber)
								row.Cells.Add(cell)
								cell = New HtmlTableCell()
								cell.InnerText = panelDisplayed.Name & IIf(bpps.PanelId.Equals(panelId), "", " (Panel source: " & panel.Name & ")")
								row.Cells.Add(cell)
								cell = New HtmlTableCell()
								cell.Attributes("style") = "text-align:center;"
								If _printerFriendlyViewSelected OrElse _isReadOnly Then
									cell.InnerText = IIf(bpps.Disabled, "X", "")
								Else
									Dim cbxDisabled As New CheckBox()
									If bpps.PanelId.Equals(panelId) Then
										cbxDisabled.ID = String.Format(DISABLED_CHECKBOX_NAME, bpps.Id.ToString().Replace("-", ""))
									Else
										cbxDisabled.ID = String.Format(DISABLED_CHECKBOX_FOR_OTHER_PANEL_NAME, bpps.Id.ToString().Replace("-", ""), panelId.ToString().Replace("-", ""))
									End If
									cbxDisabled.Checked = bpps.Disabled
									cbxDisabled.AutoPostBack = True
									AddHandler cbxDisabled.CheckedChanged, AddressOf cbxDisabled_CheckedChanged
									cell.Controls.Add(cbxDisabled)
								End If
								row.Cells.Add(cell)
								cell = New HtmlTableCell()
								cell.InnerText = bpps.StartParameter
								cell.Attributes("style") = "text-align:right;"
								row.Cells.Add(cell)
								cell = New HtmlTableCell()
								cell.InnerText = bpps.FinishParameter
								cell.Attributes("style") = "text-align:right;"
								row.Cells.Add(cell)
								cell = New HtmlTableCell()
								cell.Attributes("style") = "text-align:center;"
								cell.InnerText = IIf(bpps.AlwaysUseFinishParameter, "X", "")
								row.Cells.Add(cell)
								cell = New HtmlTableCell()
								cell.Attributes("style") = "text-align:right;"
								Dim anticipationUnit As KaUnit
								Try
									anticipationUnit = GetUnitById(bpps.AnticipationUnitId, units, _currentUser)
								Catch ex As RecordNotFoundException
									anticipationUnit = New KaUnit()
									With anticipationUnit
										.Id = bpps.AnticipationUnitId
										.Abbreviation = "?"
										.Name = "?"
									End With
									units.Add(anticipationUnit)
								End Try
								cell.InnerText = bpps.Anticipation & " " & anticipationUnit.Abbreviation
								cell.Attributes("style") = "text-align:right;"
								row.Cells.Add(cell)
								cell = New HtmlTableCell()
								cell.Attributes("style") = "text-align:right;"
								cell.InnerText = bpps.AnticipationUpdateFactor
								row.Cells.Add(cell)
								cell = New HtmlTableCell()
								Dim conversionUnit As KaUnit
								Try
									conversionUnit = GetUnitById(bpps.ConversionFactorUnitId, units, _currentUser)
								Catch ex As RecordNotFoundException
									conversionUnit = New KaUnit()
									With conversionUnit
										.Id = bpps.AnticipationUnitId
										.Abbreviation = "?"
										.Name = "?"
									End With
									units.Add(conversionUnit)
								End Try
								cell.InnerText = bpps.ConversionFactor & " pulses / " & conversionUnit.Abbreviation
								cell.Attributes("style") = "text-align:right;"
								row.Cells.Add(cell)
								cell = New HtmlTableCell()
								cell.Attributes("style") = "text-align:center;"
								cell.InnerText = IIf(bpps.UpdateDensityUsingMeter, "X", "")
								row.Cells.Add(cell)
								cell = New HtmlTableCell()
								cell.Attributes("style") = "text-align:center;"
								cell.InnerText = IIf(bpps.UseAverageDensityForTicket, "X", "")
								row.Cells.Add(cell)
								cell = New HtmlTableCell()
								cell.InnerText = bpps.DumpTime
								cell.Attributes("style") = "text-align:right;"
								row.Cells.Add(cell)
								tblPanelBulkProducts.Rows.Add(row)
							End If
						Catch ex As Exception

						End Try
					Next
				Next
			End If
		Next
	End Sub

	Private Sub PopulatePanelBulkProducts()
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim selectedLocationId As Guid = Guid.Empty
		Guid.TryParse(ddlFacility.SelectedValue, selectedLocationId)
		Dim selectedPanelId As Guid = Guid.Empty
		Guid.TryParse(ddlPanel.SelectedValue, selectedPanelId)
		Dim panelIdsToDisplay As New List(Of Guid)
		If (selectedPanelId.Equals(Guid.Empty)) Then
			For Each panel As KaPanel In KaPanel.GetAll(connection, $"{KaPanel.FN_DELETED} = 0{IIf(selectedPanelId.Equals(Guid.Empty), IIf(selectedLocationId.Equals(Guid.Empty), "", $" AND {KaPanel.FN_LOCATION_ID} = {Q(selectedLocationId)}"), $" AND {KaPanel.FN_ID} = {Q(selectedPanelId)}")}", "name")
				If Not panelIdsToDisplay.Contains(panel.Id) Then panelIdsToDisplay.Add(panel.Id)
			Next
		Else
			panelIdsToDisplay.Add(selectedPanelId)
		End If

		Dim selectedBulkProductId As Guid = Guid.Empty
		Guid.TryParse(ddlBulkProduct.SelectedValue, selectedBulkProductId)
		Dim bulkProductIdList As New List(Of Guid)
		If selectedBulkProductId.Equals(Guid.Empty) Then
			For Each bp As KaBulkProduct In KaBulkProduct.GetAll(connection, "deleted=0", "name")
				bulkProductIdList.Add(bp.Id)
			Next
		Else
			bulkProductIdList.Add(selectedBulkProductId)
		End If

		_panelReferencedSettings = New List(Of KaBulkProductPanelSettings)
		_originalbulkProductPanelSettings = New List(Of KaBulkProductPanelSettings)
		Dim originalBpps As New Dictionary(Of Guid, KaBulkProductPanelSettings)
		For Each panelId As Guid In panelIdsToDisplay
			Dim panels As New Dictionary(Of Guid, KaPanel)
			Dim panelDisplayed As KaPanel = KaPanel.GetPanel(connection, Nothing, panels, panelId)
			If panelDisplayed IsNot Nothing Then
				For Each panel As KaPanel In KaPanel.GetPanelsShareCommunicationConnection(connection, Nothing, panelId)
					If Not panels.ContainsKey(panel.Id) Then panels.Add(panel.Id, panel)
				Next
				Dim settings As Dictionary(Of Guid, List(Of KaBulkProductPanelSettings)) = New Dictionary(Of Guid, List(Of KaBulkProductPanelSettings))
				KaBulkProductPanelSettings.GetBulkProductAndSettingsForPanel(connection, Nothing, panelId, panels, bulkProductIdList, True, True, settings)

				For Each bulkProductId As Guid In settings.Keys
					For Each bpps As KaBulkProductPanelSettings In settings(bulkProductId)
						If Not originalBpps.ContainsKey(bpps.Id) Then originalBpps.Add(bpps.Id, bpps)
						Dim panelBpps As KaBulkProductPanelSettings = bpps.Clone()
						panelBpps.PanelId = panelId
						_panelReferencedSettings.Add(panelBpps)
					Next
				Next
			End If
		Next
		For Each bppsId As Guid In originalBpps.Keys
			_originalbulkProductPanelSettings.Add(originalBpps(bppsId))
		Next
		ConvertSettingsToTable()
		lblBulkProductStatus.Text = ""
	End Sub

	Private Sub PopulatePanelFunctionNames()
		_panelFunctionNames = New Dictionary(Of Integer, String)
		Dim i As Integer = 1
		Do While i < 80
			_panelFunctionNames.Add(i, "Product " & i)
			i += 1
		Loop
		_panelFunctionNames.Add(80, "Measured discharge")
		_panelFunctionNames.Add(81, "Timed mix")
		_panelFunctionNames.Add(82, "Discharge")
		_panelFunctionNames.Add(83, "Timed purge")
		_panelFunctionNames.Add(84, "Automatic discharge")
		_panelFunctionNames.Add(85, "Pause")
		_panelFunctionNames.Add(86, "Start recirculation")
		_panelFunctionNames.Add(87, "Stop recirculation")
		_panelFunctionNames.Add(88, "Start agitator")
		_panelFunctionNames.Add(89, "Stop agitator")
		_panelFunctionNames.Add(90, "Timed agitate")
		_panelFunctionNames.Add(99, "Hand-add")
	End Sub

	Protected Sub btnPrinterFriendly_Click(sender As Object, e As EventArgs) Handles btnPrinterFriendly.Click
		Guid.TryParse(ddlBulkProduct.SelectedValue, _bulkProductId)
		Guid.TryParse(ddlPanel.SelectedValue, _panelId)
		Guid.TryParse(ddlFacility.SelectedValue, _facilityId)
		ScriptManager.RegisterClientScriptBlock(PleaseWaitPanel, PleaseWaitPanel.GetType(), "PanelBulkProductsPfv", Utilities.JsWindowOpen("PanelBulkProducts.aspx?media_type=" & KaReports.MEDIA_TYPE_PFV &
										   "&bulk_product_id=" & ddlBulkProduct.SelectedValue &
										   "&panel_id=" & ddlPanel.SelectedValue &
										   "&location_id=" & ddlFacility.SelectedValue), False)
	End Sub

	Private Sub btnShowReport_Click(sender As Object, e As System.EventArgs) Handles btnApplyFilter.Click
		Guid.TryParse(ddlBulkProduct.SelectedValue, _bulkProductId)
		Guid.TryParse(ddlPanel.SelectedValue, _panelId)
		Guid.TryParse(ddlFacility.SelectedValue, _facilityId)
		PopulatePanelBulkProducts()
	End Sub

	Private Sub PopulateBulkProducts(ByVal connection As OleDbConnection)
		ddlBulkProduct.Items.Clear()
		ddlBulkProduct.Items.Add(New ListItem("All bulk products", Guid.Empty.ToString))
		For Each bulkProduct As KaBulkProduct In KaBulkProduct.GetAll(connection, "deleted=0", "name")
			ddlBulkProduct.Items.Add(New ListItem(bulkProduct.Name, bulkProduct.Id.ToString))
		Next
		Try
			ddlBulkProduct.SelectedValue = _bulkProductId.ToString()
		Catch ex As ArgumentOutOfRangeException
			ddlBulkProduct.SelectedIndex = 0
		End Try
	End Sub

	Private Sub PopulatePanels(ByVal connection As OleDbConnection)
		Dim currentPanelId As String = Guid.Empty.ToString()
		If ddlPanel.SelectedIndex >= 0 Then currentPanelId = ddlPanel.SelectedValue
		ddlPanel.Items.Clear()
		ddlPanel.Items.Add(New ListItem("All panels", Guid.Empty.ToString))
		Dim facilityId As Guid = Guid.Empty
		Guid.TryParse(ddlFacility.SelectedValue, facilityId)
		For Each r As KaPanel In KaPanel.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
			If facilityId.Equals(Guid.Empty) OrElse facilityId.Equals(r.LocationId) Then ddlPanel.Items.Add(New ListItem(r.Name, r.Id.ToString()))
		Next
		Try
			ddlPanel.SelectedValue = currentPanelId.ToString()
		Catch ex As ArgumentOutOfRangeException
			ddlPanel.SelectedIndex = 0
		End Try
	End Sub

	Private Sub PopulateLocations(ByVal connection As OleDbConnection)
		ddlFacility.Items.Clear()
		ddlFacility.Items.Add(New ListItem("All facilities", Guid.Empty.ToString))
		For Each facility As KaLocation In KaLocation.GetAll(connection, "deleted=0", "name")
			ddlFacility.Items.Add(New ListItem(facility.Name, facility.Id.ToString))
		Next
		Try
			ddlFacility.SelectedValue = _facilityId.ToString()
		Catch ex As ArgumentOutOfRangeException
			ddlFacility.SelectedIndex = 0
		End Try
	End Sub

	Protected Overrides Function SaveViewState() As Object
		Dim viewState(7) As Object
		'Saving the grid values to the View State
		viewState(0) = _bulkProductId
		viewState(1) = _facilityId
		viewState(2) = _panelId
		viewState(3) = _originalbulkProductPanelSettings
		viewState(4) = _panelReferencedSettings
		viewState(5) = _printerFriendlyViewSelected
		viewState(6) = _isReadOnly
		viewState(7) = MyBase.SaveViewState()
		Return viewState
	End Function

	Protected Overrides Sub LoadViewState(savedState As Object)
		'Getting the dropdown list value from view state.
		If savedState IsNot Nothing AndAlso CType(savedState, Object).Length > 0 Then
			_currentUser = Utilities.GetUser(Me)
			Dim viewState As Object() = savedState
			_bulkProductId = viewState(0)
			_facilityId = viewState(1)
			_panelId = viewState(2)
			_originalbulkProductPanelSettings = viewState(3)
			_panelReferencedSettings = viewState(4)
			_printerFriendlyViewSelected = viewState(5)
			_isReadOnly = viewState(6)
			ConvertSettingsToTable()
			MyBase.LoadViewState(viewState(7))
		Else
			MyBase.LoadViewState(savedState)
		End If
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
			Dim header As String = "Panel Bulk Products"
			' Force a refresh on the table to not show the checkboxes
			Dim currenPrinterFriendlyViewSelected As Boolean = _printerFriendlyViewSelected
			If Not currenPrinterFriendlyViewSelected Then
				_printerFriendlyViewSelected = True
				PopulatePanelBulkProducts()
			End If

			Dim body As String = Utilities.GenerateHTML(tblPanelBulkProducts)

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
					attachments.Add(New System.Net.Mail.Attachment(New IO.MemoryStream(Encoding.UTF8.GetBytes(newEmail.Body)), "PanelBulkProducts.html", System.Net.Mime.MediaTypeNames.Text.Html))
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
			If Not currenPrinterFriendlyViewSelected Then
				'Reset the table to have checkboxes
				_printerFriendlyViewSelected = False
				PopulatePanelBulkProducts()
			End If
		End If
	End Sub

	Protected Sub ddlFacility_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlFacility.SelectedIndexChanged
		PopulatePanels(GetUserConnection(_currentUser.Id))
		KaSetting.WriteSetting(GetUserConnection(_currentUser.Id), "PanelsPage:" & _currentUser.Id.ToString & "/LastFacilityUsed", ddlFacility.SelectedValue)
		ddlFilter_SelectedIndexChanged(ddlFacility, New EventArgs())
	End Sub

	Private Sub ddlFilter_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlBulkProduct.SelectedIndexChanged, ddlPanel.SelectedIndexChanged, ddlFacility.SelectedIndexChanged
		lblBulkProductStatus.Text = "Filter changed"
	End Sub
#End Region
End Class