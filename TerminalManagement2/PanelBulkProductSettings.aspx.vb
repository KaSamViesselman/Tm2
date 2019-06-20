Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class PanelBulkProductSettings
	Inherits System.Web.UI.Page
	Private _currentUser As KaUser
	Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
	Private _currentTableName As String = KaBulkProductPanelSettings.TABLE_NAME

	Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
		Response.Cache.SetCacheability(HttpCacheability.NoCache)
		_currentUser = Utilities.GetUser(Me)
		_currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "PanelBulkProductSettings")

		If Not _currentUserPermission(KaBulkProductPanelSettings.TABLE_NAME).Read Then Response.Redirect("Welcome.aspx")
		lblBulkProductStatus.Text = ""
		If Not Page.IsPostBack Then
			SetTextboxMaxLengths()
			PopulateLocationList()
			Try
				ddlFacilityFilter.SelectedValue = KaSetting.GetSetting(GetUserConnection(_currentUser.Id), "PanelsPage:" & _currentUser.Id.ToString & "/LastFacilityUsed", Guid.Empty.ToString())
			Catch ex As ArgumentOutOfRangeException
				ddlFacilityFilter.SelectedValue = Guid.Empty.ToString()
			End Try
			ddlFacilityFilter_SelectedIndexChanged(ddlFacilityFilter, New EventArgs())
			Try
				chkHideDisabledBulkProducts.Checked = Boolean.Parse(KaSetting.GetSetting(GetUserConnection(_currentUser.Id), "PanelBulkProductSettings:" & _currentUser.Id.ToString & "/HideDisabled", False.ToString()))
			Catch ex As Exception
				chkHideDisabledBulkProducts.Checked = False
			End Try
			PopulateBulkProductList()
			PopulateUnitLists()
			If Page.Request("PanelId") IsNot Nothing Then
				Try
					ddlPanels.SelectedValue = Page.Request("PanelId")
				Catch ex As ArgumentOutOfRangeException
					ddlPanels.SelectedIndex = 0
				End Try
			End If
			ddlPanels_SelectedIndexChanged(ddlPanels, New EventArgs())
			Utilities.ConfirmBox(Me.btnRemoveBulkProduct, "Are you sure you want to remove this bulk product setting for this panel?") ' confirmation box setup
		ElseIf Page.IsPostBack And Request("__EVENTARGUMENT").StartsWith("DisableBulkProductPanelSettings") Then
			Dim bulkProductId As String = Request("__EVENTARGUMENT").Split(",")(0).Replace("DisableBulkProductPanelSettings('", "").Replace("'", "")
			Dim panelId As String = Request("__EVENTARGUMENT").Split(",")(1).Replace("'", "").Replace(")", "")
			DisableBulkProductPanelSettings(bulkProductId, panelId)
		End If
	End Sub

	Private Sub PopulateLocationList()
		ddlFacilityFilter.Items.Clear()
		ddlFacilityFilter.Items.Add(New ListItem("All facilities", Guid.Empty.ToString()))
		For Each locationInfo As KaLocation In KaLocation.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
			ddlFacilityFilter.Items.Add(New ListItem(locationInfo.Name, locationInfo.Id.ToString))
		Next
	End Sub

	Private Sub PopulatePanelList()
		Dim currentPanelId As String = Guid.Empty.ToString()
		If ddlPanels.SelectedIndex >= 0 Then currentPanelId = ddlPanels.SelectedValue
		ddlPanels.Items.Clear()
		If _currentUserPermission(_currentTableName).Create Then
			ddlPanels.Items.Add(New ListItem("Enter a new panel", Guid.Empty.ToString()))
		Else
			ddlPanels.Items.Add(New ListItem("Select a panel", Guid.Empty.ToString()))
		End If
		ddlPanels.SelectedIndex = 0
		Dim facilityId As Guid = Guid.Empty
		Guid.TryParse(ddlFacilityFilter.SelectedValue, facilityId)
		For Each r As KaPanel In KaPanel.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
			If facilityId.Equals(Guid.Empty) OrElse facilityId.Equals(r.LocationId) Then ddlPanels.Items.Add(New ListItem(r.Name, r.Id.ToString()))
		Next
		Try
			ddlPanels.SelectedValue = currentPanelId
		Catch ex As ArgumentOutOfRangeException

		End Try
		ddlPanels_SelectedIndexChanged(ddlPanels, New EventArgs)
	End Sub

	Private Sub PopulateBulkProductPanelSettingsList(ByVal panelId As Guid)
		Dim bppsId As Guid
		Try
			bppsId = Tm2Database.FromXml(lstBulkProducts.SelectedValue, GetType(KaBulkProductPanelSettings)).Id
		Catch ex As Exception
			bppsId = Guid.Empty
		End Try
		PopulateBulkProductPanelSettingsListOnly(panelId, bppsId)
		PopulateBulkProductInformation()
	End Sub

	Protected Sub ddlPanels_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles ddlPanels.SelectedIndexChanged
		Dim panelId As Guid = Guid.Empty
		If ddlPanels.SelectedIndex >= 0 Then Guid.TryParse(ddlPanels.SelectedValue, panelId)
		PopulateBulkProductPanelSettingsList(panelId)
		SetControlUsabilityFromPermissions()
	End Sub

	Protected Sub lstBulkProducts_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles lstBulkProducts.SelectedIndexChanged
		PopulatePanelFunctionList()
		PopulateBulkProductInformation()
		ddlBulkProduct.SelectedIndex = 0
		SetControlUsabilityFromPermissions()
	End Sub

	Protected Sub btnAddBulkProduct_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnAddBulkProduct.Click
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim getSimilarBulkProductPanelSettingsRdr As OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), "SELECT count(*) as settings_count " &
					"FROM bulk_product_panel_settings " &
					"WHERE deleted = 0 AND disabled = 0 AND bulk_product_id = " & Q(Guid.Parse(ddlBulkProduct.SelectedValue)) & " AND panel_id = " & Q(Guid.Parse(ddlPanels.SelectedValue)))
		Dim hasEnabledSettings As Boolean = False
		Do While getSimilarBulkProductPanelSettingsRdr.Read()
			If (Integer.Parse(getSimilarBulkProductPanelSettingsRdr.Item("settings_count")) > 0) Then
				hasEnabledSettings = True
			End If
		Loop
		getSimilarBulkProductPanelSettingsRdr.Close()
		If (hasEnabledSettings) Then
			DisplayJavaScriptMessage("InvalidDisableOtherBulkProducts", Utilities.JsAlert("Please disable other bulk products with same name on this panel before adding a new bulk product."))
			Exit Sub
		End If
		Dim bpps As New KaBulkProductPanelSettings()
		bpps.BulkProductId = Guid.Parse(ddlBulkProduct.SelectedValue)
		bpps.PanelId = Guid.Parse(ddlPanels.SelectedValue)
		Try ' to set the conversion factor unit of measure to the default volume unit of measure...
			bpps.ConversionFactorUnitId = KaUnit.GetSystemDefaultVolumeUnitOfMeasure(connection, Nothing)
		Catch ex As ArgumentOutOfRangeException ' the default volume unit of measure isn't available...
		End Try ' suppress the exception, the user will be warned that they need to select a unit of measure
		Try ' to set the anticipation unit of measure to the default unit of measure for the bulk product...
			bpps.AnticipationUnitId = New KaBulkProduct(connection, bpps.BulkProductId).DefaultUnitId
		Catch ex As ArgumentOutOfRangeException ' the bulk product default unit of measure isn't available...
			Try ' to set the anticipation unit of measure to the default weight unit of measure...
				bpps.AnticipationUnitId = KaUnit.GetSystemDefaultMassUnitOfMeasure(connection, Nothing)
			Catch ex2 As ArgumentOutOfRangeException ' the default weight unit of measure isn't available...

			End Try
		End Try
		bpps.SqlInsert(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
		PopulateBulkProductPanelSettingsListOnly(Guid.Parse(ddlPanels.SelectedValue), bpps.Id)
		lstBulkProducts_SelectedIndexChanged(Nothing, Nothing)
		ddlBulkProduct.SelectedIndex = 0
		SetControlUsabilityFromPermissions()
	End Sub

	Protected Sub ddlBulkProduct_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles ddlBulkProduct.SelectedIndexChanged
		lstBulkProducts.SelectedIndex = -1
		PopulateBulkProductInformation()
		SetControlUsabilityFromPermissions()
	End Sub

	Protected Sub btnSave_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnSave.Click
		Dim testInteger As Int64
		Dim testDouble As Double

		' check the values that the user has entered
		If Not Integer.TryParse(tbxStartParameter.Text, testInteger) Then ' the start parameter isn't numeric
			DisplayJavaScriptMessage("InvalidCharStartParameter", Utilities.JsAlert("Please enter a numeric value for the start parameter."))
			Exit Sub
		ElseIf testInteger < UShort.MinValue OrElse testInteger > UShort.MaxValue Then ' the start parameter is out of range
			DisplayJavaScriptMessage("InvalidStartParameter", Utilities.JsAlert("Please enter a value between 0 and " & UShort.MaxValue & " for the start parameter."))
			Exit Sub
		ElseIf Not Integer.TryParse(tbxFinishingParameter.Text, testInteger) Then ' the finishing parameter isn't numeric
			DisplayJavaScriptMessage("InvalidCharFinishingParameter", Utilities.JsAlert("Please enter a numeric value for the Finishing parameter."))
			Exit Sub
		ElseIf testInteger < UShort.MinValue OrElse testInteger > UShort.MaxValue Then ' the finishing parameter is out of range
			DisplayJavaScriptMessage("InvalidFinishingParameter", Utilities.JsAlert("Please enter a value between 0 and " & UShort.MaxValue & " for the finishing parameter."))
			Exit Sub
		ElseIf Not Double.TryParse(tbxAnticipation.Text, testDouble) Then ' anticipation isn't numeric
			DisplayJavaScriptMessage("InvalidCharAnticipation", Utilities.JsAlert("Please enter a numeric value for the anticipation."))
			Exit Sub
		ElseIf testDouble < 0 OrElse testDouble > 4095.9375 Then ' anticipation is out of range
			DisplayJavaScriptMessage("InvalidAnticipation", Utilities.JsAlert("Please enter a value between 0 and 4095.9375 for the anticipation."))
			Exit Sub
		ElseIf Not Double.TryParse(tbxAnticipationUpdateFactor.Text, testDouble) Then ' anticipation update factor isn't numeric
			DisplayJavaScriptMessage("InvalidCharAnticipationFactor", Utilities.JsAlert("Please enter a numeric value for the anticipation update factor."))
			Exit Sub
		ElseIf testDouble < 0 OrElse testDouble > 1 Then ' anticipation update factor is out of range
			DisplayJavaScriptMessage("InvalidAnticipationFactor", Utilities.JsAlert("Please enter a value between 0 and 1 for the anticipation update factor."))
			Exit Sub
		ElseIf Not Double.TryParse(tbxConversionFactor.Text, testDouble) Then ' the conversion factor isn't numeric
			DisplayJavaScriptMessage("InvalidCharConversionFactor", Utilities.JsAlert("Please enter a numeric value for the conversion factor."))
			Exit Sub
		ElseIf testDouble < 0 OrElse testDouble > 65535.9999847 Then ' the conversion factor is out of range
			DisplayJavaScriptMessage("Invalid ConversionFactor", Utilities.JsAlert("Please enter a value between 0 and 65535.9999847 for the conversion factor."))
			Exit Sub
		ElseIf testDouble > 0 AndAlso Guid.Parse(ddlConversionFactorUnit.SelectedValue) = Guid.Empty Then ' conversion factor unit hasn't been selected
			DisplayJavaScriptMessage("InvalidConversionUnit", Utilities.JsAlert("Please select a unit of measure for the conversion factor."))
			Exit Sub
		ElseIf Not Integer.TryParse(tbxDumpTime.Text, testInteger) Then ' dump time isn't numeric
			DisplayJavaScriptMessage("InvalidCharDumpTime", Utilities.JsAlert("Please enter a numeric value for the dump time."))
			Exit Sub
		ElseIf testInteger < 0 Then ' dump time is out of range
			DisplayJavaScriptMessage("InvalidDumpTime", Utilities.JsAlert("Please enter a value greater than or equal to zero for the dump time."))
			Exit Sub
		End If
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id) ' update the bulk product panel settings record
		Dim bpps As New KaBulkProductPanelSettings
		Try
			If lstBulkProducts.SelectedIndex >= 0 AndAlso lstBulkProducts.SelectedValue.Length > 0 Then
				bpps = Tm2Database.FromXml(lstBulkProducts.SelectedValue, GetType(KaBulkProductPanelSettings))
			End If
		Catch ex As Exception
			bpps = New KaBulkProductPanelSettings
		End Try
		Dim showEnableBulkProductWarning As Boolean = False
		If (bpps.Disabled) Then
			showEnableBulkProductWarning = True
		End If
		If (Not chkDisabled.Checked AndAlso showEnableBulkProductWarning) Then
			Dim enabledSettings As ArrayList = KaBulkProductPanelSettings.GetAll(connection, String.Format("deleted = 0 AND disabled = 0 AND bulk_product_id = {0} AND panel_id = {1} ", Q(bpps.BulkProductId), Q(Guid.Parse(ddlPanels.SelectedValue))), "")
			Dim similarSettings As ArrayList = KaBulkProductPanelSettings.GetAll(connection, String.Format("deleted = 0AND bulk_product_id = {0} AND panel_id = {1} ", Q(bpps.BulkProductId), Q(Guid.Parse(ddlPanels.SelectedValue))), "")
			If (enabledSettings.Count > 0 AndAlso similarSettings.Count > 1) Then
				Dim javaScript As String =
							  "<script language='JavaScript'>" &
							  "if ( confirm('Enabling this bulk product will disable all similarly named bulk products on this panel.') == true )" &
							  "{" &
							   ClientScript.GetPostBackEventReference(Me, "DisableBulkProductPanelSettings('" & bpps.BulkProductId.ToString() & "','" & ddlPanels.SelectedValue & "')") &
							  "}" &
							  "</script>"
				ClientScript.RegisterStartupScript(Me.GetType(), "DisableBulkProductPanelSettingsScript", javaScript)
				'DisableBulkProductPanelSettings(bulkProductPanelSettings.BulkProductId, Guid.Parse(ddlPanels.SelectedValue))
			Else
				SaveBulkProduct()
			End If
		Else
			SaveBulkProduct()
		End If
	End Sub

	Protected Sub btnRemoveBulkProduct_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnRemoveBulkProduct.Click
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id) ' delete the bulk product panel settings from the database
		Dim bpps As KaBulkProductPanelSettings
		Try
			bpps = Tm2Database.FromXml(lstBulkProducts.SelectedValue, GetType(KaBulkProductPanelSettings))
			bpps.Deleted = True
			bpps.SqlUpdate(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
		Catch ex As Exception
			bpps = New KaBulkProductPanelSettings
		End Try
		PopulateBulkProductPanelSettingsList(Guid.Parse(ddlPanels.SelectedValue))
		lstBulkProducts_SelectedIndexChanged(lstBulkProducts, New EventArgs())
	End Sub

	Private Function PopulateBulkProductInformation() As Boolean
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim validBpps As Boolean = False
		Dim bpps As KaBulkProductPanelSettings
		If lstBulkProducts.SelectedItem IsNot Nothing Then
			ddlNumber.Enabled = True
			tbxStartParameter.Enabled = True
			tbxFinishingParameter.Enabled = True
			chkAlwaysUseFinishParameter.Enabled = True
			tbxAnticipation.Enabled = True
			ddlAnticipationUnit.Enabled = True
			tbxAnticipationUpdateFactor.Enabled = True
			tbxConversionFactor.Enabled = True
			ddlConversionFactorUnit.Enabled = True
			chkUpdateDensityUsingMeter.Enabled = True
			tbxDumpTime.Enabled = True
			chkUseAverageDensityForTicket.Enabled = True
			chkDisabled.Enabled = True

			bpps = Tm2Database.FromXml(lstBulkProducts.SelectedValue, GetType(KaBulkProductPanelSettings))
			Try
				lblBulkProductName.Text = New KaBulkProduct(connection, bpps.BulkProductId).Name
			Catch ex As RecordNotFoundException
				lblBulkProductName.Text = ""
			End Try

			Try ' to set the product number...
				ddlNumber.SelectedValue = bpps.ProductNumber
			Catch ex As ArgumentOutOfRangeException ' the specified product number isn't available...
				Try ' to set the product number to the default value: hand-add...
					ddlNumber.SelectedValue = "99"
					DisplayJavaScriptMessage("InvalidPanelFunction", Utilities.JsAlert("Panel function " & bpps.ProductNumber & " not found. Panel function set to ""Generic hand-add"""))
				Catch ex2 As ArgumentOutOfRangeException ' hand-add isn't in the list either...
					ddlNumber.SelectedIndex = 0 ' use the first available product number instead
				End Try
			End Try
			tbxStartParameter.Text = bpps.StartParameter
			tbxFinishingParameter.Text = bpps.FinishParameter
			chkAlwaysUseFinishParameter.Checked = bpps.AlwaysUseFinishParameter
			tbxAnticipation.Text = bpps.Anticipation
			Try ' to set the conversion factor unit of measure...
				ddlAnticipationUnit.SelectedValue = bpps.AnticipationUnitId.ToString()
			Catch ex As ArgumentOutOfRangeException ' the selected unit of measure isn't available...
				If bpps.ProductNumber < 80 OrElse bpps.ProductNumber = 99 Then ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), "InvalidAnticipationUnit", Utilities.JsAlert("Record not found in units where ID = " & bpps.AnticipationUnitId.ToString() & ". Please select a unit of measure for the anticipation factor."), False)
			End Try
			tbxAnticipationUpdateFactor.Text = bpps.AnticipationUpdateFactor
			tbxConversionFactor.Text = bpps.ConversionFactor
			Try ' to set the conversion factor unit of measure...
				ddlConversionFactorUnit.SelectedValue = bpps.ConversionFactorUnitId.ToString()
			Catch ex As ArgumentOutOfRangeException ' the selected unit of measure isn't available...
				If bpps.ProductNumber < 80 OrElse bpps.ProductNumber = 99 Then ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), "InvalidConversionUnit", Utilities.JsAlert("Record not found in units where ID = " & bpps.ConversionFactorUnitId.ToString() & ". Please select a unit of measure for the conversion factor."), False)
			End Try
			chkUpdateDensityUsingMeter.Checked = bpps.UpdateDensityUsingMeter
			tbxDumpTime.Text = bpps.DumpTime.ToString()
			chkUseAverageDensityForTicket.Checked = bpps.UseAverageDensityForTicket
			chkDisabled.Checked = bpps.Disabled
			validBpps = bpps.BulkProductId <> Guid.Empty
			PopulateStorageLocationsAssigned(bpps.Id)
			PopulateStorageLocationList(bpps.Id)
			pnlSettings.Visible = True
			pnlStorageLocations.Visible = ddlStorageLocations.Items.Count > 0
		Else
			lblBulkProductName.Text = ""
			ddlNumber.Enabled = False
			tbxStartParameter.Enabled = False
			tbxFinishingParameter.Enabled = False
			bpps = New KaBulkProductPanelSettings()
			chkAlwaysUseFinishParameter.Enabled = False
			tbxAnticipation.Enabled = False
			ddlAnticipationUnit.Enabled = False
			tbxAnticipationUpdateFactor.Enabled = False
			tbxConversionFactor.Enabled = False
			ddlConversionFactorUnit.Enabled = False
			chkUpdateDensityUsingMeter.Enabled = False
			tbxDumpTime.Enabled = False
			chkUseAverageDensityForTicket.Enabled = False
			chkDisabled.Enabled = False

			tbxStartParameter.Text = "0"
			tbxFinishingParameter.Text = "0"
			tbxAnticipation.Text = "0"
			ddlAnticipationUnit.SelectedIndex = 0
			tbxAnticipationUpdateFactor.Text = "0"
			tbxConversionFactor.Text = "0"
			Try
				ddlConversionFactorUnit.SelectedValue = KaUnit.GetSystemDefaultVolumeUnitOfMeasure(connection, Nothing).ToString()
			Catch ex As ArgumentOutOfRangeException
				ddlConversionFactorUnit.SelectedIndex = 0
			End Try
			chkUpdateDensityUsingMeter.Checked = False
			tbxDumpTime.Text = "0"
			chkUseAverageDensityForTicket.Checked = False
			validBpps = False
			pnlSettings.Visible = False
			pnlStorageLocations.Visible = False
		End If

		SetControlUsabilityFromPermissions()
		Return validBpps
	End Function

	Private Sub PopulateUnitLists()
		ddlAnticipationUnit.Items.Clear()
		ddlConversionFactorUnit.Items.Clear()

		ddlAnticipationUnit.Items.Add(New ListItem("Not set", Guid.Empty.ToString()))

		For Each r As KaUnit In KaUnit.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
			If Not KaUnit.IsTime(r.BaseUnit) Then
				ddlAnticipationUnit.Items.Add(New ListItem(r.Abbreviation, r.Id.ToString()))
				ddlConversionFactorUnit.Items.Add(New ListItem(r.Abbreviation, r.Id.ToString()))
			End If
		Next
	End Sub

	Private Sub PopulateBulkProductList()
		ddlBulkProduct.Items.Clear()
		ddlBulkProduct.Items.Add(New ListItem("", Guid.Empty.ToString()))
		For Each bulkProduct As KaBulkProduct In KaBulkProduct.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
			ddlBulkProduct.Items.Add(New ListItem(bulkProduct.Name, bulkProduct.Id.ToString()))
		Next
	End Sub

	Private Sub PopulatePanelFunctionList()
		Dim hasFunction As Boolean = False
		Dim hasProduct As Boolean = False
		Dim panelId As Guid = Guid.Parse(ddlPanels.SelectedValue)
		Dim bpps As KaBulkProductPanelSettings
		Try
			bpps = Tm2Database.FromXml(lstBulkProducts.SelectedValue, GetType(KaBulkProductPanelSettings))
		Catch ex As Exception
			bpps = New KaBulkProductPanelSettings
		End Try

		Dim conditions As String = "deleted = 0 AND bulk_product_id = " & Q(bpps.BulkProductId) & " AND panel_id <> " & Q(panelId)
		Dim list As ArrayList = KaBulkProductPanelSettings.GetAll(GetUserConnection(_currentUser.Id), conditions, "")
		For Each otherBpps As KaBulkProductPanelSettings In list
			If otherBpps.ProductNumber >= 80 AndAlso otherBpps.ProductNumber <> 99 Then hasFunction = True
			If otherBpps.ProductNumber < 80 OrElse otherBpps.ProductNumber = 99 Then hasProduct = True
		Next
		ddlNumber.Items.Clear()
		If Not hasFunction OrElse IsFunctionInList(list, 99) Then ddlNumber.Items.Add(New ListItem("Hand-add", "99"))
		Dim i As Integer = 1
		Do While i < 80
			If Not hasFunction OrElse IsFunctionInList(list, i) Then ddlNumber.Items.Add(New ListItem("Product " & i, i))
			i += 1
		Loop
		Do While i < 99
			If Not (hasFunction OrElse hasProduct) OrElse IsFunctionInList(list, 80) Then
				Dim separator1 As ListItem = New ListItem("", "")
				separator1.Attributes.Add("disabled", "true")
				ddlNumber.Items.Add(separator1)
				Dim separator2 As ListItem = New ListItem("---------Functions---------", "")
				separator2.Attributes.Add("disabled", "true")
				ddlNumber.Items.Add(separator2)
				Dim separator3 As ListItem = New ListItem("", "")
				separator3.Attributes.Add("disabled", "true")
				ddlNumber.Items.Add(separator3)
				Exit Do
			End If
			i += 1
		Loop

		If Not (hasFunction OrElse hasProduct) OrElse IsFunctionInList(list, 80) Then ddlNumber.Items.Add(New ListItem("Measured discharge", "80"))
		If Not (hasFunction OrElse hasProduct) OrElse IsFunctionInList(list, 81) Then ddlNumber.Items.Add(New ListItem("Timed mix", "81"))
		If Not (hasFunction OrElse hasProduct) OrElse IsFunctionInList(list, 82) Then ddlNumber.Items.Add(New ListItem("Discharge", "82"))
		If Not (hasFunction OrElse hasProduct) OrElse IsFunctionInList(list, 83) Then ddlNumber.Items.Add(New ListItem("Timed purge", "83"))
		If Not (hasFunction OrElse hasProduct) OrElse IsFunctionInList(list, 84) Then ddlNumber.Items.Add(New ListItem("Automatic discharge", "84"))
		If Not (hasFunction OrElse hasProduct) OrElse IsFunctionInList(list, 85) Then ddlNumber.Items.Add(New ListItem("Pause", "85"))
		If Not (hasFunction OrElse hasProduct) OrElse IsFunctionInList(list, 86) Then ddlNumber.Items.Add(New ListItem("Start recirculation", "86"))
		If Not (hasFunction OrElse hasProduct) OrElse IsFunctionInList(list, 87) Then ddlNumber.Items.Add(New ListItem("Stop recirculation", "87"))
		If Not (hasFunction OrElse hasProduct) OrElse IsFunctionInList(list, 88) Then ddlNumber.Items.Add(New ListItem("Start agitator", "88"))
		If Not (hasFunction OrElse hasProduct) OrElse IsFunctionInList(list, 89) Then ddlNumber.Items.Add(New ListItem("Stop agitator", "89"))
		If Not (hasFunction OrElse hasProduct) OrElse IsFunctionInList(list, 90) Then ddlNumber.Items.Add(New ListItem("Timed agitate", "90"))
		If Not (hasFunction OrElse hasProduct) OrElse IsFunctionInList(list, 91) Then ddlNumber.Items.Add(New ListItem("Rinse", "91"))
	End Sub

	Private Function IsFunctionInList(bulkProductPanelSettings As ArrayList, productNumber As Integer) As Boolean
		For Each bpps As KaBulkProductPanelSettings In bulkProductPanelSettings
			If bpps.ProductNumber = productNumber Then Return True
		Next
		Return False
	End Function

	Private Sub PopulateBulkProductPanelSettingsListOnly(ByVal panelId As Guid, ByVal bulkProductPanelSettingId As Guid)
		lstBulkProducts.Items.Clear()
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim panels As New Dictionary(Of Guid, KaPanel)
		If KaPanel.GetPanel(connection, Nothing, panels, panelId) IsNot Nothing Then
			For Each panel As KaPanel In KaPanel.GetPanelsShareCommunicationConnection(connection, Nothing, panelId)
				If Not panels.ContainsKey(panel.Id) Then panels.Add(panel.Id, panel)
			Next
			Dim settings As Dictionary(Of Guid, List(Of KaBulkProductPanelSettings)) = New Dictionary(Of Guid, List(Of KaBulkProductPanelSettings))
			KaBulkProductPanelSettings.GetBulkProductAndSettingsForPanel(connection, Nothing, panelId, panels, New List(Of Guid), Not chkHideDisabledBulkProducts.Checked, True, settings)
			For Each bulkProductId As Guid In settings.Keys
				Dim bulkProduct As New KaBulkProduct(connection, bulkProductId)
				For Each bpps As KaBulkProductPanelSettings In settings(bulkProductId)
					Dim panel As KaPanel
					Try
						panel = KaPanel.GetPanel(connection, Nothing, panels, bpps.PanelId)
						lstBulkProducts.Items.Add(New ListItem(bulkProduct.Name & IIf(bpps.Disabled, " (disabled)", "") & IIf(bpps.PanelId.Equals(panelId), "", " (Panel source: " & panel.Name & ")"), Tm2Database.ToXml(bpps, GetType(KaBulkProductPanelSettings))))
						If bpps.Id.Equals(bulkProductPanelSettingId) Then lstBulkProducts.SelectedIndex = lstBulkProducts.Items.Count - 1
					Catch ex As Exception

					End Try
				Next
			Next
		End If
		lstBulkProducts.Rows = Math.Max(4, Math.Min(20, lstBulkProducts.Items.Count))
		lstBulkProducts_SelectedIndexChanged(lstBulkProducts, Nothing)
	End Sub

	Private Sub DisableBulkProductPanelSettings(ByVal bulkProductId As String, ByVal panelId As String)
		Tm2Database.ExecuteNonQuery(GetUserConnection(_currentUser.Id), "UPDATE bulk_product_panel_settings SET disabled = 1 WHERE disabled = 0 AND bulk_product_id = " & Q(Guid.Parse(bulkProductId)) & " AND panel_id = " & Q(Guid.Parse(panelId)) & "")
		SaveBulkProduct()
	End Sub

	Private Sub SaveBulkProduct()
		Dim panelId As Guid = Guid.Parse(ddlPanels.SelectedValue)
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id) ' update the bulk product panel settings record
		If lstBulkProducts.SelectedIndex >= 0 AndAlso lstBulkProducts.SelectedValue.Length > 0 Then
			Dim bpps As KaBulkProductPanelSettings = Tm2Database.FromXml(lstBulkProducts.SelectedValue, GetType(KaBulkProductPanelSettings))
			If Not bpps.PanelId.Equals(panelId) Then ' this was for a different panel, so it should be inserted as a new setting for this panel
				bpps.Id = Guid.NewGuid
				bpps.PanelId = panelId
			End If
			With bpps
				If Tm2Database.SystemItemTraceabilityEnabled Then
					Dim currentStorageLocations As Dictionary(Of Guid, KaBulkProductPanelStorageLocation) = New Dictionary(Of Guid, KaBulkProductPanelStorageLocation)
					For Each sl As KaBulkProductPanelStorageLocation In .StorageLocations
						currentStorageLocations.Add(sl.StorageLocationId, sl)
					Next

					Dim storageLocationIds As List(Of Guid) = AssignedStorageLocationIds()
					For Each slId As Guid In storageLocationIds
						If Not currentStorageLocations.ContainsKey(slId) Then currentStorageLocations.Add(slId, New KaBulkProductPanelStorageLocation() With {.StorageLocationId = slId})
					Next
					.StorageLocations.Clear()
					For Each slId As Guid In currentStorageLocations.Keys
						.StorageLocations.Add(currentStorageLocations(slId))
					Next
				Else
					' Leave the list as-is
				End If
				.ProductNumber = ddlNumber.SelectedValue
				.StartParameter = tbxStartParameter.Text
				.FinishParameter = tbxFinishingParameter.Text
				.AlwaysUseFinishParameter = chkAlwaysUseFinishParameter.Checked
				.Anticipation = tbxAnticipation.Text
				.AnticipationUnitId = Guid.Parse(ddlAnticipationUnit.SelectedValue)
				.AnticipationUpdateFactor = tbxAnticipationUpdateFactor.Text
				.ConversionFactor = tbxConversionFactor.Text
				.ConversionFactorUnitId = Guid.Parse(ddlConversionFactorUnit.SelectedValue)
				.UpdateDensityUsingMeter = chkUpdateDensityUsingMeter.Checked
				.DumpTime = Integer.Parse(tbxDumpTime.Text)
				.UseAverageDensityForTicket = chkUseAverageDensityForTicket.Checked
				.Disabled = chkDisabled.Checked
				.SqlUpdateInsertIfNotFound(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
			End With
			PopulateBulkProductPanelSettingsListOnly(panelId, bpps.Id)
			lblBulkProductStatus.Text = "Bulk product panel settings saved"
		End If
	End Sub

	Private Sub SetTextboxMaxLengths()
		tbxAnticipation.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaBulkProductPanelSettings.TABLE_NAME, "anticipation"))
		tbxAnticipationUpdateFactor.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaBulkProductPanelSettings.TABLE_NAME, "anticipation_update_factor"))
		tbxConversionFactor.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaBulkProductPanelSettings.TABLE_NAME, "conversion_factor"))
		tbxDumpTime.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaBulkProductPanelSettings.TABLE_NAME, KaBulkProductPanelSettings.FN_DUMP_TIME))
		tbxFinishingParameter.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaBulkProductPanelSettings.TABLE_NAME, "finish_parameter"))
		tbxStartParameter.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaBulkProductPanelSettings.TABLE_NAME, "start_parameter"))
	End Sub

	Private Sub SetControlUsabilityFromPermissions()
		With _currentUserPermission(KaBulkProductPanelSettings.TABLE_NAME)
			Dim shouldEnable = (.Edit AndAlso ddlPanels.SelectedIndex > 0)
			pnlBulkProductSettings.Enabled = ddlPanels.SelectedIndex > 0
			pnlSettings.Enabled = shouldEnable

			btnAddBulkProduct.Enabled = shouldEnable AndAlso ddlBulkProduct.SelectedIndex > 0
			btnRemoveBulkProduct.Enabled = shouldEnable AndAlso lstBulkProducts.SelectedIndex >= 0
			btnSave.Enabled = shouldEnable AndAlso lstBulkProducts.SelectedIndex >= 0
			btnAddStorageLocations.Enabled = shouldEnable AndAlso ddlStorageLocations.SelectedIndex > 0
			btnRemoveStorageLocations.Enabled = shouldEnable AndAlso lstStorageLocations.SelectedIndex >= 0
		End With
	End Sub

	Protected Sub ddlFacilityFilter_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlFacilityFilter.SelectedIndexChanged
		PopulatePanelList()
		KaSetting.WriteSetting(GetUserConnection(_currentUser.Id), "PanelsPage:" & _currentUser.Id.ToString & "/LastFacilityUsed", ddlFacilityFilter.SelectedValue)
	End Sub

	Protected Sub chkHideDisabledBulkProducts_CheckedChanged(sender As Object, e As EventArgs) Handles chkHideDisabledBulkProducts.CheckedChanged
		PopulateBulkProductPanelSettingsList(Guid.Parse(ddlPanels.SelectedValue))
		KaSetting.WriteSetting(GetUserConnection(_currentUser.Id), "PanelBulkProductSettings:" & _currentUser.Id.ToString & "/HideDisabled", chkHideDisabledBulkProducts.Checked.ToString)
	End Sub

	Protected Sub ScriptManager1_AsyncPostBackError(ByVal sender As Object, ByVal e As System.Web.UI.AsyncPostBackErrorEventArgs)
		ToolkitScriptManager1.AsyncPostBackErrorMessage = e.Exception.Message
	End Sub
	Private Sub DisplayJavaScriptMessage(key As String, script As String)
		ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), key, script, False)
	End Sub

#Region " Storage locations "
	Private Sub PopulateStorageLocationList()
		Dim bulkProducSettingId As Guid = Guid.Empty
		Try
			If lstBulkProducts.SelectedIndex >= 0 Then Guid.TryParse(lstBulkProducts.SelectedValue, bulkProducSettingId)
		Catch ex As RecordNotFoundException

		End Try
		PopulateStorageLocationList(bulkProducSettingId)
	End Sub

	Private Sub PopulateStorageLocationList(dischargeLocationId As Guid)
		ddlStorageLocations.Items.Clear()
		If Tm2Database.SystemItemTraceabilityEnabled Then
			Dim locationId As Guid = Guid.Empty
			Guid.TryParse(ddlFacilityFilter.SelectedValue, locationId)
			ddlStorageLocations.Items.Add(New ListItem("", Guid.Empty.ToString()))
			Dim alreadyAssignedIds As List(Of Guid) = AssignedStorageLocationIds()

			Dim sql As String
			sql = $"SELECT 1 AS tableIndex, {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_ID}, {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_NAME} " &
							$"FROM {KaStorageLocation.TABLE_NAME} " &
							$"WHERE ({KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_DELETED} = 0) " &
								$"AND ({KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_TANK_ID} = {Q(Guid.Empty)}) " &
								$"AND ({KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_CONTAINER_ID} = {Q(Guid.Empty)}) " &
								IIf(locationId.Equals(Guid.Empty), "", $"AND ({KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_LOCATION_ID} = {Q(locationId)}) ") & ' if Facility filter is not set, display all possible storage locations
					"UNION " &
					$"SELECT 1 AS tableIndex, {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_ID}, {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_NAME} " &
							$"FROM {KaStorageLocation.TABLE_NAME} " &
							$"WHERE ({KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_DELETED} = 0) " &
								$"AND ({KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_TANK_ID} = {Q(Guid.Empty)}) " &
								$"AND ({KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_CONTAINER_ID} = {Q(Guid.Empty)}) " &
								$"AND ({KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_LOCATION_ID} = {Q(Guid.Empty)}) " &
					"UNION " &
					$"SELECT 2 AS tableIndex, {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_ID}, " &
								$"CASE WHEN {KaTank.TABLE_NAME}.name = {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_NAME} THEN 'Tank: ' + {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_NAME} ELSE {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_NAME} + ' (Tank: ' + {KaTank.TABLE_NAME}.name + ')' END AS {KaStorageLocation.FN_NAME} " &
							$"FROM {KaStorageLocation.TABLE_NAME} INNER JOIN {KaTank.TABLE_NAME} ON {KaTank.TABLE_NAME}.{KaTank.FN_ID} = {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_TANK_ID} " &
							$"WHERE ({KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_DELETED} = 0) " &
								$"AND ({KaTank.TABLE_NAME}.deleted = {Q(False)}) " &
								$"AND ({KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_TANK_ID} <> {Q(Guid.Empty)}) " &
								IIf(locationId.Equals(Guid.Empty), "", $"AND ({KaTank.TABLE_NAME}.location_id = {Q(locationId)}) ") & ' if Facility filter is not set, display all possible storage locations
					"UNION " &
					$"SELECT 2 AS tableIndex, {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_ID}, " &
								$"CASE WHEN {KaTank.TABLE_NAME}.name = {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_NAME} THEN 'Tank: ' + {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_NAME} ELSE {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_NAME} + ' (Tank: ' + {KaTank.TABLE_NAME}.name + ')' END AS {KaStorageLocation.FN_NAME} " &
							$"FROM {KaStorageLocation.TABLE_NAME} INNER JOIN {KaTank.TABLE_NAME} ON {KaTank.TABLE_NAME}.{KaTank.FN_ID} = {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_TANK_ID} " &
							$"WHERE ({KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_DELETED} = 0) " &
								$"AND ({KaTank.TABLE_NAME}.deleted = {Q(False)}) " &
								$"AND ({KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_TANK_ID} <> {Q(Guid.Empty)}) " &
								$"AND ({KaTank.TABLE_NAME}.location_id = {Q(Guid.Empty)}) " &
					"UNION " &
					$"SELECT 2 AS tableIndex, {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_ID}, " &
								$"CASE WHEN {KaContainer.TABLE_NAME}.number = {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_NAME} THEN 'Container: ' + {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_NAME} ELSE {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_NAME} + ' (Container: ' + {KaContainer.TABLE_NAME}.number + ')' END AS {KaStorageLocation.FN_NAME} " &
							$"FROM {KaStorageLocation.TABLE_NAME} " &
							$"INNER JOIN {KaContainer.TABLE_NAME} ON {KaContainer.TABLE_NAME}.{KaContainer.FN_ID} = {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_CONTAINER_ID} " &
							$"WHERE ({KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_DELETED} = 0) " &
								$"AND ({KaContainer.TABLE_NAME}.deleted = {Q(False)}) " &
								$"AND ({KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_CONTAINER_ID} <> {Q(Guid.Empty)}) " &
							$"ORDER BY tableIndex, {KaStorageLocation.FN_NAME}"
			Dim storageLocationsRdr As OleDbDataReader = Nothing
			Try
				storageLocationsRdr = Tm2Database.ExecuteReader(Tm2Database.Connection, sql)
				While storageLocationsRdr.Read()
					Dim slId As Guid = storageLocationsRdr.Item(KaStorageLocation.FN_ID)
					If Not alreadyAssignedIds.Contains(slId) Then ddlStorageLocations.Items.Add(New ListItem(storageLocationsRdr.Item(KaStorageLocation.FN_NAME), slId.ToString()))
				End While
			Finally
				If storageLocationsRdr IsNot Nothing Then storageLocationsRdr.Close()
			End Try
		End If
		ddlStorageLocations_SelectedIndexChanged(ddlStorageLocations, New EventArgs())
	End Sub

	Private Sub PopulateStorageLocationsAssigned(bulkProductSettingId As Guid)
		lstStorageLocations.Items.Clear()
		If Tm2Database.SystemItemTraceabilityEnabled Then
			Dim sql As String
			sql = $"SELECT 1 AS tableIndex, {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_ID}, {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_NAME} " &
						$"FROM {KaStorageLocation.TABLE_NAME} " &
						$"INNER JOIN {KaBulkProductPanelStorageLocation.TABLE_NAME} ON {KaBulkProductPanelStorageLocation.TABLE_NAME}.{KaBulkProductPanelStorageLocation.FN_STORAGE_LOCATION_ID} = {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_ID} " &
						$"WHERE ({KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_DELETED} = 0) " &
							$"AND ({KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_TANK_ID} = {Q(Guid.Empty)}) " &
							$"AND ({KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_CONTAINER_ID} = {Q(Guid.Empty)}) " &
							$"AND ({KaBulkProductPanelStorageLocation.TABLE_NAME}.{KaBulkProductPanelStorageLocation.FN_DELETED} = {Q(False)}) " &
							$"AND ({KaBulkProductPanelStorageLocation.TABLE_NAME}.{KaBulkProductPanelStorageLocation.FN_BULK_PRODUCT_PANEL_SETTING_ID} = {Q(bulkProductSettingId)}) " &
							$"AND ({KaBulkProductPanelStorageLocation.TABLE_NAME}.{KaBulkProductPanelStorageLocation.FN_BULK_PRODUCT_PANEL_SETTING_ID} <> {Q(Guid.Empty)}) " &
				"UNION " &
				$"SELECT 2 AS tableIndex, {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_ID}, " &
							$"CASE WHEN {KaTank.TABLE_NAME}.name = {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_NAME} THEN 'Tank: ' + {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_NAME} ELSE {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_NAME} + ' (Tank: ' + {KaTank.TABLE_NAME}.name + ')' END AS {KaStorageLocation.FN_NAME} " &
						$"FROM {KaStorageLocation.TABLE_NAME} " &
						$"INNER JOIN {KaTank.TABLE_NAME} ON {KaTank.TABLE_NAME}.{KaTank.FN_ID} = {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_TANK_ID} " &
						$"INNER JOIN {KaBulkProductPanelStorageLocation.TABLE_NAME} ON {KaBulkProductPanelStorageLocation.TABLE_NAME}.{KaBulkProductPanelStorageLocation.FN_STORAGE_LOCATION_ID} = {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_ID} " &
						$"WHERE ({KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_DELETED} = 0) " &
							$"AND ({KaTank.TABLE_NAME}.deleted = {Q(False)}) " &
							$"AND ({KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_TANK_ID} <> {Q(Guid.Empty)}) " &
							$"AND ({KaBulkProductPanelStorageLocation.TABLE_NAME}.{KaBulkProductPanelStorageLocation.FN_DELETED} = {Q(False)}) " &
							$"AND ({KaBulkProductPanelStorageLocation.TABLE_NAME}.{KaBulkProductPanelStorageLocation.FN_BULK_PRODUCT_PANEL_SETTING_ID} = {Q(bulkProductSettingId)}) " &
							$"AND ({KaBulkProductPanelStorageLocation.TABLE_NAME}.{KaBulkProductPanelStorageLocation.FN_BULK_PRODUCT_PANEL_SETTING_ID} <> {Q(Guid.Empty)}) " &
				"UNION " &
				$"SELECT 2 AS tableIndex, {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_ID}, " &
							$"CASE WHEN {KaContainer.TABLE_NAME}.number = {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_NAME} THEN 'Container: ' + {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_NAME} ELSE {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_NAME} + ' (Container: ' + {KaContainer.TABLE_NAME}.number + ')' END AS {KaStorageLocation.FN_NAME} " &
						$"FROM {KaStorageLocation.TABLE_NAME} " &
						$"INNER JOIN {KaContainer.TABLE_NAME} ON {KaContainer.TABLE_NAME}.{KaContainer.FN_ID} = {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_CONTAINER_ID} " &
						$"INNER JOIN {KaBulkProductPanelStorageLocation.TABLE_NAME} ON {KaBulkProductPanelStorageLocation.TABLE_NAME}.{KaBulkProductPanelStorageLocation.FN_STORAGE_LOCATION_ID} = {KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_ID} " &
						$"WHERE ({KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_DELETED} = 0) " &
							$"AND ({KaContainer.TABLE_NAME}.deleted = {Q(False)}) " &
							$"AND ({KaStorageLocation.TABLE_NAME}.{KaStorageLocation.FN_CONTAINER_ID} <> {Q(Guid.Empty)}) " &
							$"AND ({KaBulkProductPanelStorageLocation.TABLE_NAME}.{KaBulkProductPanelStorageLocation.FN_DELETED} = {Q(False)}) " &
							$"AND ({KaBulkProductPanelStorageLocation.TABLE_NAME}.{KaBulkProductPanelStorageLocation.FN_BULK_PRODUCT_PANEL_SETTING_ID} = {Q(bulkProductSettingId)}) " &
							$"AND ({KaBulkProductPanelStorageLocation.TABLE_NAME}.{KaBulkProductPanelStorageLocation.FN_BULK_PRODUCT_PANEL_SETTING_ID} <> {Q(Guid.Empty)}) " &
				$"ORDER BY tableIndex, {KaStorageLocation.FN_NAME}"
			Dim storageLocationsRdr As OleDbDataReader = Nothing
			Try
				storageLocationsRdr = Tm2Database.ExecuteReader(Tm2Database.Connection, sql)
				While storageLocationsRdr.Read()
					lstStorageLocations.Items.Add(New ListItem(storageLocationsRdr.Item(KaStorageLocation.FN_NAME), storageLocationsRdr.Item(KaStorageLocation.FN_ID).ToString()))
				End While
			Finally
				If storageLocationsRdr IsNot Nothing Then storageLocationsRdr.Close()
			End Try
		End If
		lstStorageLocations_SelectedIndexChanged(lstStorageLocations, New EventArgs())
	End Sub

	Private Function AssignedStorageLocationIds() As List(Of Guid)
		Dim alreadyAssignedIds As List(Of Guid) = New List(Of Guid)
		For Each li As ListItem In lstStorageLocations.Items
			Dim slId As Guid = Guid.Empty
			If Guid.TryParse(li.Value, slId) AndAlso Not slId.Equals(Guid.Empty) AndAlso Not alreadyAssignedIds.Contains(slId) Then alreadyAssignedIds.Add(slId)
		Next
		Return alreadyAssignedIds
	End Function

	Protected Sub ddlStorageLocations_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlStorageLocations.SelectedIndexChanged
		SetControlUsabilityFromPermissions()
	End Sub

	Protected Sub lstStorageLocations_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lstStorageLocations.SelectedIndexChanged
		SetControlUsabilityFromPermissions()
	End Sub

	Protected Sub btnAddStorageLocations_Click(sender As Object, e As EventArgs) Handles btnAddStorageLocations.Click
		If ddlStorageLocations.SelectedIndex > 0 Then
			lstStorageLocations.Items.Add(New ListItem(ddlStorageLocations.SelectedItem.Text, ddlStorageLocations.SelectedItem.Value))
		End If
		PopulateStorageLocationList()
	End Sub

	Protected Sub btnRemoveStorageLocations_Click(sender As Object, e As EventArgs) Handles btnRemoveStorageLocations.Click
		If lstStorageLocations.SelectedIndex >= 0 Then
			lstStorageLocations.Items.RemoveAt(lstStorageLocations.SelectedIndex)
		End If
		PopulateStorageLocationList()
	End Sub
#End Region
End Class