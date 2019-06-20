Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class DefaultDeliveryWebTicketSettings : Inherits System.Web.UI.Page

	Private _currentUser As KaUser
	Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
	Private _currentTableName As String = KaSetting.TABLE_NAME

	Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
		Response.Cache.SetCacheability(HttpCacheability.NoCache)
		_currentUser = Utilities.GetUser(Me)
		_currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "GeneralSettings")
		If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")
		btnSaveOwnerWebTicketSettings.Enabled = _currentUserPermission(_currentTableName).Edit
		btnSaveProductGroupAdditionalUnits.Enabled = btnSaveOwnerWebTicketSettings.Enabled
		btnDeleteOwnerWebTicketSettings.Enabled = _currentUserPermission(_currentTableName).Delete
		If Not Page.IsPostBack Then
			PopulateOwnersList()
			PopulateProductGroupsCombo()
			PopulateAdditionalUnitsList()
			PopulateWebTicketDensityUnits()
			PopulateWebTicketOwnerSettings(Guid.Empty)
			pnlShowCompartmentBulkIngredientLotNumber.Visible = Tm2Database.SystemItemTraceabilityEnabled

			Utilities.ConfirmBox(Me.btnDeleteOwnerWebTicketSettings, "Are you sure you want to delete the web ticket settings for this owner?")
		End If
	End Sub

	Private Sub PopulateOwnersList()
		ddlWebTicketOwner.Items.Clear()
		ddlWebTicketOwner.Items.Add(New ListItem("All owners", Guid.Empty.ToString))
		For Each o As KaOwner In KaOwner.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
			ddlWebTicketOwner.Items.Add(New ListItem(o.Name, o.Id.ToString))
		Next
	End Sub

#Region " Default web ticket settings "
	Private Sub PopulateWebTicketOwnerSettings(ByVal ownerId As Guid)
		'Reset fields
		cblAdditionalUnitsForTicket.ClearSelection()
		lblWebTicketSettingsExist.Visible = False
		chkShowFertilizerGrade.Checked = False
		chkShowFertilizerGuaranteedAnalysis.Checked = False

		ddlProductGroup.SelectedIndex = 0
		ddlProductGroup_SelectedIndexChanged(Nothing, Nothing)
		cbxAnalysisShowTrailingZeros.Checked = True
		cbxHideZeroPercentAnalysisNutrients.Checked = True
		ddlGradeAnalysisDecimalCountGreaterThanOne.SelectedIndex = 0
		ddlGradeAnalysisDecimalCountLessThanOne.SelectedIndex = 2

		Dim c As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim allSettings As ArrayList = KaSetting.GetAll(c, "name like 'WebTicketSetting:" & ownerId.ToString & "%' and deleted=0", "")
		lblWebTicketSettingsExist.Visible = (allSettings.Count > 0)
		lblWebTicketSettingsExist.Text = "Settings exist"
		If ownerId.Equals(Guid.Empty) Then
			Dim settingsValidForOwners As String = ""
			For Each possibleOwner As KaOwner In KaOwner.GetAll(c, "deleted = 0", "name ASC")
				Dim ownerTicketSettingsCountRdr As OleDbDataReader = Tm2Database.ExecuteReader(c, "SELECT COUNT(*) FROM settings WHERE name LIKE 'WebTicketSetting:" & possibleOwner.Id.ToString & "%' AND deleted = 0")
				If ownerTicketSettingsCountRdr.Read() AndAlso ownerTicketSettingsCountRdr.Item(0) = 0 Then
					If settingsValidForOwners.Length > 0 Then settingsValidForOwners &= ", "
					settingsValidForOwners &= possibleOwner.Name
				End If
				ownerTicketSettingsCountRdr.Close()
			Next
			If settingsValidForOwners.Length > 0 Then lblWebTicketSettingsExist.Text = "These settings are valid for " & settingsValidForOwners
		End If

		For Each unitId As String In GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_ADDITIONAL_UNITS_FOR_TICKET, "").Trim().Split(",")
			For Each item As ListItem In cblAdditionalUnitsForTicket.Items
				If item.Value = unitId Then
					item.Selected = True
					Exit For
				End If
			Next
		Next

		Boolean.TryParse(GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_DISCHARGE_LOCATION, "False"), chkShowDischargeLocationOnTicket.Checked)
		Dim showDateAndTime As String = GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_DATE_AND_TIME, "True")
		Boolean.TryParse(GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_DATE, showDateAndTime), chkDate.Checked)
		Boolean.TryParse(GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_TIME, showDateAndTime), chkShowTime.Checked)
		chkDate_CheckedChanged(chkDate, New EventArgs())
		Boolean.TryParse(GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_OWNER, "True"), chkOwner.Checked)
		Boolean.TryParse(GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_BRANCH_LOCATION, "True"), chkBranchLocation.Checked)
		Boolean.TryParse(GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_CUSTOMER, "True"), chkCustomer.Checked)
		Boolean.TryParse(GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_CUSTOMER_LOCATION, "True"), chkCustomerLocation.Checked)
		Dim defaultShowBulkProducts As Boolean = True
		Boolean.TryParse(GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_BULK_INGREDIENTS, "True"), defaultShowBulkProducts)
		Boolean.TryParse(GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_BULK_PRODUCT_SUMMARY_TOTALS, "False"), chkShowBulkProductSummaryTotals.Checked)
		Boolean.TryParse(GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_BULK_PRODUCT_NOTES_ON_SUMMARY_ON_TICKET, False), chkShowBulkProductNotesSummaryTotals.Checked)
		Boolean.TryParse(GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_BULK_PRODUCT_EPA_NUMBER_ON_SUMMARY_ON_TICKET, False), chkShowBulkProductEpaNumberSummaryTotals.Checked)
		Boolean.TryParse(GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_PRODUCT_SUMMARY_TOTALS, "False"), chkShowProductSummaryTotals.Checked)
		Boolean.TryParse(GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_PRODUCT_NOTES_ON_SUMMARY_ON_TICKET, False), chkShowProductNotesSummaryTotals.Checked)
		Boolean.TryParse(GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_ORDER_SUMMARY, "False"), chkShowOrderSummary.Checked)
		Boolean.TryParse(GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_ORDER_SUMMARY_HISTORICAL, "False"), chkShowOrderSummaryHistorical.Checked)
		Boolean.TryParse(GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_COMPARTMENTS, "True"), chkShowCompartmentsOnTicket.Checked)
		Boolean.TryParse(GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_PRODUCT_NOTES_ON_COMPARTMENT_ON_TICKET, True), chkShowCompartmentProductNotes.Checked) ' Default this to true for backwards compatibility
		Boolean.TryParse(GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_COMPARTMENT_BULK_INGREDIENTS, defaultShowBulkProducts), chkShowCompartmentBulkIngredients.Checked)
		Boolean.TryParse(GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_BULK_PRODUCT_NOTES_ON_COMPARTMENT_ON_TICKET, False), chkShowCompartmentBulkIngredientNotes.Checked)
		Boolean.TryParse(GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_BULK_PRODUCT_EPA_NUMBER_ON_COMPARTMENT_ON_TICKET, False), chkShowCompartmentBulkIngredientEpaNumber.Checked)
		Boolean.TryParse(GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_COMPARTMENT_TOTALS, "False"), chkShowCompartmentTotals.Checked)
		Boolean.TryParse(GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_LOADED_INDEX_FOR_COMPARTMENT_ON_TICKET, True), chkShowCompartmentLoadedIndex.Checked)
		Boolean.TryParse(GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_COMPARTMENT_FERTILIZER_GRADE, False), chkShowCompartmentFertilizerGrade.Checked)
		chkShowCompartmentsOnTicket_CheckedChanged(chkShowCompartmentsOnTicket, New EventArgs())
		Boolean.TryParse(GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_REQUESTED_QUANTITIES, "True"), chkShowRequestedQuantities.Checked)
		Boolean.TryParse(GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_TOTAL, "True"), chkTotal.Checked)
		Boolean.TryParse(GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_RELEASE_NUMBER, "False"), chkReleaseNumber.Checked)
		Boolean.TryParse(GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_PURCHASE_ORDER_NUMBER, "True"), chkPurchaseOrder.Checked)
		Boolean.TryParse(GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_PRODUCT_HAZARDOUS_MATERIAL, "False"), chkProductHazardousMaterial.Checked)
		Boolean.TryParse(GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_CARRIER, "True"), chkCarrierId.Checked)
		Boolean.TryParse(GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_TRANSPORT, "True"), chkTransport.Checked)
		Boolean.TryParse(GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_TRANSPORT_TARE_WEIGHTS, "True"), chkShowTicketTransportTareInfo.Checked)
		Boolean.TryParse(GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_DENSITY_ON_TICKET, "False"), chkShowDensityOnTicket.Checked)
		Boolean.TryParse(GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_DERIVED_FROM_ON_TICKET, "False"), chkShowDerivedFrom.Checked)
		Boolean.TryParse(GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_RINSE_ENTRIES, "False"), chkShowRinseEntries.Checked)
		If Tm2Database.SystemItemTraceabilityEnabled Then
			Boolean.TryParse(GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_COMPARTMENT_BULK_INGREDIENT_LOT_NUMBER, "False"), chkShowCompartmentBulkIngredientLotNumber.Checked)
		End If


		Dim truckWeightOrder() As String = GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_TRUCK_TGN_ORDER, "T-G-N").Split("-")
		If truckWeightOrder.Length > 0 Then
			PopulateTicketTransportTareOptions(ddlTransportTareFirstOption, truckWeightOrder(0))
		Else
			PopulateTicketTransportTareOptions(ddlTransportTareFirstOption, "")
		End If
		If truckWeightOrder.Length > 1 Then
			PopulateTicketTransportTareOptions(ddlTransportTareSecondOption, truckWeightOrder(1))
		Else
			PopulateTicketTransportTareOptions(ddlTransportTareSecondOption, "")
		End If
		If truckWeightOrder.Length > 2 Then
			PopulateTicketTransportTareOptions(ddlTransportTareThirdOption, truckWeightOrder(2))
		Else
			PopulateTicketTransportTareOptions(ddlTransportTareThirdOption, "")
		End If
		chkTransport_CheckedChanged(chkShowTicketTransportTareInfo, New EventArgs)
		Boolean.TryParse(GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_DRIVER, "True"), chkDriverName.Checked)
		Boolean.TryParse(GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_DRIVER_NUMBER, "True"), chkDriverNumber.Checked)
		Boolean.TryParse(GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_APPLICATOR, "True"), chkShowApplicatorOnTicket.Checked)
		Boolean.TryParse(GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_ACRES, "False"), chkAcresOnTicket.Checked)
		Boolean.TryParse(GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_EMAIL_ADDRESS, "True"), chkEmailAddress.Checked)
		Boolean.TryParse(GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_FACILITY_ON_TICKET, "False"), chkShowFacility.Checked)
		Boolean.TryParse(GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_FERTILIZER_GRADE, "False"), chkShowFertilizerGrade.Checked)
		Boolean.TryParse(GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_FERTILIZER_GUARANTEED_ANALYSIS, "False"), chkShowFertilizerGuaranteedAnalysis.Checked)
		Boolean.TryParse(GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_COMPARTMENT_FERTILIZER_GUARANTEED_ANALYSIS, "False"), chkShowFertilizerGuaranteedAnalysisByCompartment.Checked)
		chkDriverNumber.Enabled = chkDriverName.Checked
		Try
			Integer.TryParse(GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_GRADE_ANALYSIS_DECIMAL_COUNT_GREATER_THAN_ONE, 0), ddlGradeAnalysisDecimalCountGreaterThanOne.SelectedIndex)
		Catch ex As IndexOutOfRangeException
			ddlGradeAnalysisDecimalCountGreaterThanOne.SelectedIndex = 0
		End Try
		Try
			Integer.TryParse(GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_GRADE_ANALYSIS_DECIMAL_COUNT_LESS_THAN_ONE, 2), ddlGradeAnalysisDecimalCountLessThanOne.SelectedIndex)
		Catch ex As IndexOutOfRangeException
			ddlGradeAnalysisDecimalCountLessThanOne.SelectedIndex = 2
		End Try
		Boolean.TryParse(GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_ANALYSIS_ENTRIES_ROUNDED_DOWN, "False"), cbxAnalysisEntriesRoundedDown.Checked)
		Boolean.TryParse(GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_ANALYSIS_SHOW_TRAILING_ZEROS, "True"), cbxAnalysisShowTrailingZeros.Checked)
		Boolean.TryParse(GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_HIDE_ZERO_PERCENT_ANALYSIS_NUTRIENTS, "True"), cbxHideZeroPercentAnalysisNutrients.Checked)
		Boolean.TryParse(GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_CUSTOMER_NOTES_ON_TICKET, "False"), chkShowCustomerNotesOnTicket.Checked)
		Boolean.TryParse(GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_CUSTOMER_DESTINATION_NOTES_ON_TICKET, "False"), chkShowCustomerDestinationNotesOnTicket.Checked)
		Boolean.TryParse(GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_NTEP_COMPLIANT, "True"), chkNtepCompliant.Checked)
		Boolean.TryParse(GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_LOADED_BY_ON_TICKET, False.ToString()), cbxShowLoadedBy.Checked)
		Boolean.TryParse(GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_APPLICATION_RATE_ON_TICKET, False.ToString()), chkShowApplicationRateOnTicket.Checked)
		Boolean.TryParse(GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_USE_ORIGINAL_ORDERS_APPLICATION_RATE_ON_TICKET, False.ToString()), chkUseOriginalOrdersApplicationRate.Checked)
		tbxTicketLogo.Text = GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_LOGO_PATH, "")
		tbxOwnerMessage.Text = GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_OWNER_MESSAGE, "")
		tbxOwnerDisclaimer.Text = GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_DISCLAIMER, "")
		tbxBlank1.Text = GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_BLANK1, "")
		tbxBlank2.Text = GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_BLANK2, "")
		tbxBlank3.Text = GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_BLANK3, "")
		tbxTicketAddonURL.Text = GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_TICKET_ADDON_URL, "")
		Boolean.TryParse(GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_DISPLAY_BLEND_GROUP_NAME_AS_PRODUCT_NAME, False.ToString()), chkDisplayBlendGroupNameAsProductName.Checked)

		PopulateWebTicketDensitySettings(c, ownerId)
		Boolean.TryParse(GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_ALL_CUSTOM_FIELDS_ON_DELIVERY_TICKET, True.ToString()), cbxShowAllCustomFieldsOnDeliveryTicket.Checked)
		PopulateWebTicketCustomFieldsShownSettings(c, ownerId)
		Boolean.TryParse(GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_ALL_CUSTOM_PRE_LOAD_QUESTIONS, True.ToString()), cbxShowAllCustomPreLoadQuestions.Checked)
		Boolean.TryParse(GetWebTicketSettingByOwnerId(c, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_ALL_CUSTOM_POST_LOAD_QUESTIONS, True.ToString()), cbxShowAllCustomPostLoadQuestions.Checked)
		PopulateCustomPreAndPostLoadQuestions(c, ownerId)

		ShowSummaryCheckedChanged(Nothing, Nothing)
		cbxShowAllCustomFieldsOnDeliveryTicket_CheckedChanged(cbxShowAllCustomFieldsOnDeliveryTicket, New EventArgs())
		cbxShowAllCustomPreLoadQuestions_CheckedChanged(cbxShowAllCustomPreLoadQuestions, New EventArgs())
		cbxShowAllCustomPostLoadQuestions_CheckedChanged(cbxShowAllCustomPostLoadQuestions, New EventArgs())
		FertilizerGradeCheckedChanged(Nothing, Nothing)
		FertilizerGuaranteedAnalysisCheckedChanged(Nothing, Nothing)
		DisplayJavaScriptMessage("ResetScrollPosition;", "resetDotNetScrollPosition();", True)
	End Sub

	Private Sub PopulateAdditionalUnitsToDisplayForWebticketProductGroups(ByVal productGroupId As Guid)
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		cblAdditionalUnitsForProductGroup.ClearSelection()
		ddlProductGroupDensityMassUnit.SelectedIndex = 0
		ddlProductGroupDensityVolumeUnit.SelectedIndex = 0

		If productGroupId <> Guid.Empty Then
			Dim c As OleDbConnection = GetUserConnection(_currentUser.Id)
			cblAdditionalUnitsForProductGroup.ClearSelection()
			Dim webTicketSettingFormat As String = "WebTicketSetting:{0}:{1}/AdditionalUnitsForProductGroup"
			Dim webTicketSetting As String = String.Format(webTicketSettingFormat, Guid.Empty.ToString(), productGroupId.ToString())
			Dim defaultOwnerProductUnits As String = KaSetting.GetSetting(connection, webTicketSetting, "")
			webTicketSetting = String.Format(webTicketSettingFormat, ddlWebTicketOwner.SelectedValue, productGroupId.ToString())
			For Each unitIdString As String In KaSetting.GetSetting(connection, webTicketSetting, defaultOwnerProductUnits, False, Nothing).Trim().Split(",")

				For Each item As ListItem In cblAdditionalUnitsForProductGroup.Items
					If item.Value = unitIdString Then
						item.Selected = True
						Exit For
					End If
				Next
			Next

			Dim productGroupDensityMassUnitIdSettingFormat As String = "WebTicketSetting:{0}:{1}/ProductGroupDensityMassUnitId"
			Dim productGroupDensityMassUnitIdSetting As String = String.Format(productGroupDensityMassUnitIdSettingFormat, ddlWebTicketOwner.SelectedValue, productGroupId.ToString())
			Dim productGroupDensityMassUnitId As Guid = Guid.Parse(KaSetting.GetSetting(connection, productGroupDensityMassUnitIdSetting, KaUnit.GetSystemDefaultMassUnitOfMeasure(connection, Nothing).ToString(), False, Nothing))
			Try
				ddlProductGroupDensityMassUnit.SelectedValue = productGroupDensityMassUnitId.ToString
			Catch ex As Exception
				'Suppress
			End Try

			Dim productGroupDensityVolumeUnitIdSettingFormat As String = "WebTicketSetting:{0}:{1}/ProductGroupDensityVolumeUnitId"
			Dim productGroupDensityVolumeUnitIdSetting As String = String.Format(productGroupDensityVolumeUnitIdSettingFormat, ddlWebTicketOwner.SelectedValue, productGroupId.ToString())
			Dim productGroupDensityVolumeUnitId As Guid = Guid.Parse(KaSetting.GetSetting(connection, productGroupDensityVolumeUnitIdSetting, KaUnit.GetSystemDefaultVolumeUnitOfMeasure(connection, Nothing).ToString(), False, Nothing))
			Try
				ddlProductGroupDensityVolumeUnit.SelectedValue = productGroupDensityVolumeUnitId.ToString
			Catch ex As Exception
				'Suppress
			End Try
		End If
	End Sub

	Private Function GetWebTicketSettingByOwnerId(ByVal connection As OleDbConnection, ByVal ownerId As Guid, ByVal settingName As String, ByVal defaultValue As Object) As Object
		'Find the owner specific setting.
		Dim allSettings As ArrayList = KaSetting.GetAll(connection, "name = " & Q("WebTicketSetting:" & ownerId.ToString & "/" & settingName) & " and deleted = 0", "")
		If allSettings.Count = 1 Then
			Return allSettings.Item(0).value
		End If

		'If there isn't an owner specific setting, get the All Owners setting, if that doesn't exist either, use the default value.
		Dim retval As String = KaSetting.GetSetting(connection, "WebTicketSetting:" & Guid.Empty.ToString & "/" & settingName, defaultValue.ToString, False, Nothing)
		Return retval
	End Function

	Private Sub SaveWebTicketOwnerSettings(ByVal ownerId As Guid)
		Dim c As OleDbConnection = GetUserConnection(_currentUser.Id)

		Dim webTicketSetting As String = "WebTicketSetting:" & ownerId.ToString() & "/"
		Dim list As String = ""
		For Each item As ListItem In cblAdditionalUnitsForTicket.Items
			If item.Selected Then list &= IIf(list.Length > 0, ",", "") & item.Value
		Next
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_ADDITIONAL_UNITS_FOR_TICKET, list)
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_DISCHARGE_LOCATION, chkShowDischargeLocationOnTicket.Checked.ToString())
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_DATE, chkDate.Checked)
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_TIME, chkShowTime.Checked)
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_OWNER, chkOwner.Checked)
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_BRANCH_LOCATION, chkBranchLocation.Checked)
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_CUSTOMER_LOCATION, chkCustomerLocation.Checked)
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_CUSTOMER, chkCustomer.Checked)
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_BULK_PRODUCT_SUMMARY_TOTALS, chkShowBulkProductSummaryTotals.Checked)
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_BULK_PRODUCT_NOTES_ON_SUMMARY_ON_TICKET, chkShowBulkProductNotesSummaryTotals.Checked)
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_BULK_PRODUCT_EPA_NUMBER_ON_SUMMARY_ON_TICKET, chkShowBulkProductEpaNumberSummaryTotals.Checked)
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_PRODUCT_SUMMARY_TOTALS, chkShowProductSummaryTotals.Checked)
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_PRODUCT_NOTES_ON_SUMMARY_ON_TICKET, chkShowProductNotesSummaryTotals.Checked)
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_ORDER_SUMMARY, chkShowOrderSummary.Checked)
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_ORDER_SUMMARY_HISTORICAL, chkShowOrderSummaryHistorical.Checked)
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_COMPARTMENTS, chkShowCompartmentsOnTicket.Checked)
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_PRODUCT_NOTES_ON_COMPARTMENT_ON_TICKET, chkShowCompartmentProductNotes.Checked)
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_COMPARTMENT_BULK_INGREDIENTS, chkShowCompartmentBulkIngredients.Checked)
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_BULK_PRODUCT_NOTES_ON_COMPARTMENT_ON_TICKET, chkShowCompartmentBulkIngredientNotes.Checked)
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_BULK_PRODUCT_EPA_NUMBER_ON_COMPARTMENT_ON_TICKET, chkShowCompartmentBulkIngredientEpaNumber.Checked)
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_COMPARTMENT_TOTALS, chkShowCompartmentTotals.Checked)
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_LOADED_INDEX_FOR_COMPARTMENT_ON_TICKET, chkShowCompartmentLoadedIndex.Checked)
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_COMPARTMENT_FERTILIZER_GRADE, chkShowCompartmentFertilizerGrade.Checked)
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_REQUESTED_QUANTITIES, chkShowRequestedQuantities.Checked)
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_TOTAL, chkTotal.Checked)
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_RELEASE_NUMBER, chkReleaseNumber.Checked)
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_PURCHASE_ORDER_NUMBER, chkPurchaseOrder.Checked)
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_PRODUCT_HAZARDOUS_MATERIAL, chkProductHazardousMaterial.Checked)
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_CARRIER, chkCarrierId.Checked)
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_TRANSPORT, chkTransport.Checked)
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_DENSITY_ON_TICKET, chkShowDensityOnTicket.Checked.ToString())
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_DERIVED_FROM_ON_TICKET, chkShowDerivedFrom.Checked.ToString())
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_LOADED_BY_ON_TICKET, cbxShowLoadedBy.Checked.ToString())
		If chkTransport.Checked Then
			KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_TRANSPORT_TARE_WEIGHTS, chkShowTicketTransportTareInfo.Checked)
			Dim truckTGNOrder As String = ""
			If ddlTransportTareFirstOption.SelectedIndex > 0 Then
				truckTGNOrder = ddlTransportTareFirstOption.SelectedValue
				If ddlTransportTareSecondOption.SelectedIndex > 0 Then
					truckTGNOrder &= "-" & ddlTransportTareSecondOption.SelectedValue
					If ddlTransportTareThirdOption.SelectedIndex > 0 Then
						truckTGNOrder &= "-" & ddlTransportTareThirdOption.SelectedValue
					End If
				End If
			End If
			KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_TRUCK_TGN_ORDER, truckTGNOrder)
		End If
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_DRIVER, chkDriverName.Checked)
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_DRIVER_NUMBER, chkDriverNumber.Checked)
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_APPLICATOR, chkShowApplicatorOnTicket.Checked)
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_ACRES, chkAcresOnTicket.Checked)
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_EMAIL_ADDRESS, chkEmailAddress.Checked)
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_FACILITY_ON_TICKET, chkShowFacility.Checked)
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_FERTILIZER_GRADE, chkShowFertilizerGrade.Checked)
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_FERTILIZER_GUARANTEED_ANALYSIS, chkShowFertilizerGuaranteedAnalysis.Checked)
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_COMPARTMENT_FERTILIZER_GUARANTEED_ANALYSIS, chkShowFertilizerGuaranteedAnalysisByCompartment.Checked)
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_ANALYSIS_SHOW_TRAILING_ZEROS, cbxAnalysisShowTrailingZeros.Checked)
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_HIDE_ZERO_PERCENT_ANALYSIS_NUTRIENTS, cbxHideZeroPercentAnalysisNutrients.Checked)
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_GRADE_ANALYSIS_DECIMAL_COUNT_GREATER_THAN_ONE, ddlGradeAnalysisDecimalCountGreaterThanOne.SelectedIndex)
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_GRADE_ANALYSIS_DECIMAL_COUNT_LESS_THAN_ONE, ddlGradeAnalysisDecimalCountLessThanOne.SelectedIndex)
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_ANALYSIS_ENTRIES_ROUNDED_DOWN, cbxAnalysisEntriesRoundedDown.Checked)
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_CUSTOMER_NOTES_ON_TICKET, chkShowCustomerNotesOnTicket.Checked)
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_CUSTOMER_DESTINATION_NOTES_ON_TICKET, chkShowCustomerDestinationNotesOnTicket.Checked)
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_NTEP_COMPLIANT, chkNtepCompliant.Checked)
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_LOGO_PATH, tbxTicketLogo.Text)
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_OWNER_MESSAGE, tbxOwnerMessage.Text)
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_DISCLAIMER, tbxOwnerDisclaimer.Text)
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_BLANK1, tbxBlank1.Text)
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_BLANK2, tbxBlank2.Text)
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_BLANK3, tbxBlank3.Text)
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_TICKET_ADDON_URL, tbxTicketAddonURL.Text)
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_DISPLAY_BLEND_GROUP_NAME_AS_PRODUCT_NAME, chkDisplayBlendGroupNameAsProductName.Checked)
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_ALL_CUSTOM_FIELDS_ON_DELIVERY_TICKET, cbxShowAllCustomFieldsOnDeliveryTicket.Checked)
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_ALL_CUSTOM_PRE_LOAD_QUESTIONS, cbxShowAllCustomPreLoadQuestions.Checked)
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_ALL_CUSTOM_POST_LOAD_QUESTIONS, cbxShowAllCustomPostLoadQuestions.Checked)
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_APPLICATION_RATE_ON_TICKET, chkShowApplicationRateOnTicket.Checked)
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_USE_ORIGINAL_ORDERS_APPLICATION_RATE_ON_TICKET, chkUseOriginalOrdersApplicationRate.Checked)
		KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_RINSE_ENTRIES, chkShowRinseEntries.Checked)
		If Tm2Database.SystemItemTraceabilityEnabled Then
			KaSetting.WriteSetting(c, webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_SHOW_COMPARTMENT_BULK_INGREDIENT_LOT_NUMBER, chkShowCompartmentBulkIngredientLotNumber.Checked)
		End If
		SaveWebTicketDensitySettings(webTicketSetting)
		SaveWebTicketShowCustomFieldsSetting(webTicketSetting)
		SaveCustomPreAndPostLoadQuestions(webTicketSetting)
		PopulateWebTicketOwnerSettings(ownerId)
	End Sub

	Protected Sub btnSaveProductGroupAdditionalUnits_Click(sender As Object, e As EventArgs) Handles btnSaveProductGroupAdditionalUnits.Click
		If Guid.Parse(ddlProductGroup.SelectedValue) <> Guid.Empty Then
			Dim c As OleDbConnection = GetUserConnection(_currentUser.Id)
			Dim webTicketSetting As String = "WebTicketSetting:" & ddlWebTicketOwner.SelectedValue & ":" & ddlProductGroup.SelectedValue

			Dim list As String = ""
			For Each item As ListItem In cblAdditionalUnitsForProductGroup.Items
				If item.Selected Then list &= IIf(list.Length > 0, ",", "") & item.Value
			Next
			KaSetting.WriteSetting(c, webTicketSetting & "/" & KaSetting.DefaultDeliveryWebTicketSettings.SN_ADDITIONAL_UNITS_FOR_PRODUCT_GROUPS, list)

			Dim productGroupDensityMassUnitIdSettingFormat As String = "WebTicketSetting:{0}:{1}/ProductGroupDensityMassUnitId"
			Dim productGroupDensityMassUnitIdSetting As String = String.Format(productGroupDensityMassUnitIdSettingFormat, ddlWebTicketOwner.SelectedValue, ddlProductGroup.SelectedValue)
			Dim productGroupDensityMassUnitId As Guid = Guid.Parse(ddlProductGroupDensityMassUnit.SelectedValue)
			KaSetting.WriteSetting(c, productGroupDensityMassUnitIdSetting, productGroupDensityMassUnitId.ToString())

			Dim productGroupDensityVolumeUnitIdSettingFormat As String = "WebTicketSetting:{0}:{1}/ProductGroupDensityVolumeUnitId"
			Dim productGroupDensityVolumeUnitIdSetting As String = String.Format(productGroupDensityVolumeUnitIdSettingFormat, ddlWebTicketOwner.SelectedValue, ddlProductGroup.SelectedValue)
			Dim productGroupDensityVolumeUnitId As Guid = Guid.Parse(ddlProductGroupDensityVolumeUnit.SelectedValue)
			KaSetting.WriteSetting(c, productGroupDensityVolumeUnitIdSetting, productGroupDensityVolumeUnitId.ToString())
		End If
	End Sub

	Private Sub PopulateTicketTransportTareOptions(ByRef ddlTransportTareOption As DropDownList, ByVal transportTareValue As String)
		ddlTransportTareOption.Items.Clear()
		Dim valueFound As Boolean = False
		ddlTransportTareOption.Items.Add(New ListItem("Not Shown", ""))
		ddlTransportTareOption.Items.Add(New ListItem("Tare", "T"))
		If transportTareValue = "T" Then ddlTransportTareOption.SelectedIndex = 1 : valueFound = True
		ddlTransportTareOption.Items.Add(New ListItem("Gross", "G"))
		If transportTareValue = "G" Then ddlTransportTareOption.SelectedIndex = 2 : valueFound = True
		ddlTransportTareOption.Items.Add(New ListItem("Net", "N"))
		If transportTareValue = "N" Then ddlTransportTareOption.SelectedIndex = 3 : valueFound = True

		If Not valueFound Then ddlTransportTareOption.SelectedIndex = 0
		transportTareOptionSelectionChanged(ddlTransportTareOption, New EventArgs)
	End Sub

	Private Sub DeleteWebTicketOwnerSettings(ByVal ownerId As Guid)
		'Delete will set all the ticket settings to their 'default' values
		Dim c As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim allSettings As ArrayList = KaSetting.GetAll(c, "name like 'WebTicketSetting:" & ownerId.ToString & "%' and deleted=0", "")
		For Each setting As KaSetting In allSettings
			Tm2Database.ExecuteNonQuery(c, "Delete from settings where id = " & Q(setting.Id))
		Next
		PopulateWebTicketOwnerSettings(ownerId)
	End Sub

	Private Sub ddlWebTicketOwner_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles ddlWebTicketOwner.SelectedIndexChanged
		PopulateWebTicketOwnerSettings(Guid.Parse(ddlWebTicketOwner.SelectedValue))
	End Sub

	Protected Sub btnSaveOwnerWebTicketSettings_Click(sender As Object, e As EventArgs) Handles btnSaveOwnerWebTicketSettings.Click
		SaveWebTicketOwnerSettings(Guid.Parse(ddlWebTicketOwner.SelectedValue))
	End Sub

	Protected Sub btbDeleteOwnerWebTicketSettings_Click(sender As Object, e As EventArgs) Handles btnDeleteOwnerWebTicketSettings.Click
		DeleteWebTicketOwnerSettings(Guid.Parse(ddlWebTicketOwner.SelectedValue))
	End Sub
#End Region

	Protected Sub chkTransport_CheckedChanged(sender As Object, e As EventArgs) Handles chkTransport.CheckedChanged
		chkShowTicketTransportTareInfo.Enabled = chkTransport.Checked
		tblTransportTareOptions.Visible = chkTransport.Checked
	End Sub

	Private Sub transportTareOptionSelectionChanged(sender As Object, e As System.EventArgs) Handles chkShowTicketTransportTareInfo.CheckedChanged, ddlTransportTareFirstOption.SelectedIndexChanged, ddlTransportTareSecondOption.SelectedIndexChanged
		rowTransportTareFirstOption.Visible = chkShowTicketTransportTareInfo.Checked
		rowTransportTareSecondOption.Visible = chkShowTicketTransportTareInfo.Checked AndAlso (ddlTransportTareFirstOption.SelectedIndex > 0)
		rowTransportTareThirdOption.Visible = chkShowTicketTransportTareInfo.Checked AndAlso (ddlTransportTareFirstOption.SelectedIndex > 0) AndAlso (ddlTransportTareSecondOption.SelectedIndex > 0)
	End Sub

	Protected Sub chkShowCompartmentsOnTicket_CheckedChanged(sender As Object, e As EventArgs) Handles chkShowCompartmentsOnTicket.CheckedChanged
		chkShowCompartmentBulkIngredients.Enabled = chkShowCompartmentsOnTicket.Checked
		chkShowCompartmentTotals.Enabled = chkShowCompartmentsOnTicket.Checked
		chkShowCompartmentProductNotes.Enabled = chkShowCompartmentsOnTicket.Checked
		chkShowCompartmentLoadedIndex.Enabled = chkShowCompartmentsOnTicket.Checked
		chkShowCompartmentFertilizerGrade.Enabled = chkShowCompartmentsOnTicket.Checked
		chkShowCompartmentBulkIngredients_CheckedChanged(chkShowCompartmentBulkIngredients, New EventArgs())
	End Sub

	Protected Sub ShowSummaryCheckedChanged(sender As Object, e As EventArgs) Handles chkShowProductSummaryTotals.CheckedChanged, chkShowCompartmentsOnTicket.CheckedChanged, chkShowBulkProductSummaryTotals.CheckedChanged, chkShowOrderSummary.CheckedChanged
		chkShowRequestedQuantities.Enabled = (chkShowProductSummaryTotals.Checked OrElse chkShowCompartmentsOnTicket.Checked OrElse chkShowBulkProductSummaryTotals.Checked OrElse chkShowOrderSummary.Checked)
		chkShowOrderSummaryHistorical.Enabled = chkShowOrderSummary.Checked
		chkShowProductNotesSummaryTotals.Enabled = chkShowProductSummaryTotals.Checked
		chkShowBulkProductNotesSummaryTotals.Enabled = chkShowBulkProductSummaryTotals.Checked
		chkShowBulkProductEpaNumberSummaryTotals.Enabled = chkShowBulkProductSummaryTotals.Checked
	End Sub

	Protected Sub chkDriverName_CheckedChanged(sender As Object, e As EventArgs) Handles chkDriverName.CheckedChanged
		chkDriverNumber.Enabled = chkDriverName.Checked
	End Sub

	Private Sub PopulateProductGroupsCombo()
		ddlProductGroup.Items.Clear()
		ddlProductGroup.Items.Add(New ListItem("", Guid.Empty.ToString()))
		Dim allProductGroups As ArrayList = KaProductGroup.GetAll(GetUserConnection(_currentUser.Id), "deleted = " & Q(False), "name asc")
		For Each productGroup As KaProductGroup In allProductGroups
			ddlProductGroup.Items.Add(New ListItem(productGroup.Name, productGroup.Id.ToString))
		Next
		ddlProductGroup.SelectedIndex = 0
	End Sub

	Private Sub PopulateAdditionalUnitsList()
		cblAdditionalUnitsForTicket.Items.Clear()
		cblAdditionalUnitsForProductGroup.Items.Clear()
		For Each n As KaUnit In KaUnit.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
			If n.BaseUnit <> KaUnit.Unit.Pulses AndAlso n.BaseUnit <> KaUnit.Unit.Seconds Then
				cblAdditionalUnitsForTicket.Items.Add(New ListItem(n.Name, n.Id.ToString()))
				cblAdditionalUnitsForProductGroup.Items.Add(New ListItem(n.Name, n.Id.ToString()))
			End If
		Next
	End Sub

	Private Sub PopulateWebTicketDensityUnits()
		ddlWebTicketDensityMass.Items.Clear()
		ddlProductGroupDensityMassUnit.Items.Clear()
		ddlWebTicketDensityVolume.Items.Clear()
		ddlProductGroupDensityVolumeUnit.Items.Clear()
		For Each n As KaUnit In KaUnit.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "abbreviation ASC")
			If Not KaUnit.IsTime(n.BaseUnit) Then ' ignore time unit of measures
				If KaUnit.IsWeight(n.BaseUnit) Then ' add to the mass unit list
					ddlWebTicketDensityMass.Items.Add(New ListItem(n.Abbreviation, n.Id.ToString()))
					ddlProductGroupDensityMassUnit.Items.Add(New ListItem(n.Abbreviation, n.Id.ToString()))
				Else ' add to the volume unit list
					ddlWebTicketDensityVolume.Items.Add(New ListItem(n.Abbreviation, n.Id.ToString()))
					ddlProductGroupDensityVolumeUnit.Items.Add(New ListItem(n.Abbreviation, n.Id.ToString()))
				End If
			End If
		Next
	End Sub

	Protected Sub lstWebTicketDensityList_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lstWebTicketDensityList.SelectedIndexChanged
		UpdateWebTicketDensityRemoveEnabled()
		UpdateWebTicketPrecisionVisible()
	End Sub

	Private Function IsWebTicketDensityInList(massUnitId As Guid, volumeUnitId As Guid) As Boolean
		For Each item As ListItem In lstWebTicketDensityList.Items
			Dim parts() As String = item.Value.Split("|") ' should produce 3 parts (<mass Guid>|<volume Guid>|precision), no need to check array dimensions as long as the routine that populates the list only adds items that conform the format
			If Guid.Parse(parts(0)) = massUnitId AndAlso Guid.Parse(parts(1)) = volumeUnitId Then Return True
		Next
		Return False
	End Function

	Private Sub UpdateWebTicketDensityAddEnabled()
		btnWebTicketDensityAdd.Enabled = Not IsWebTicketDensityInList(Guid.Parse(ddlWebTicketDensityMass.SelectedValue), Guid.Parse(ddlWebTicketDensityVolume.SelectedValue)) AndAlso _currentUserPermission(_currentTableName).Edit
	End Sub

	Private Sub UpdateWebTicketDensityRemoveEnabled()
		btnWebTicketDensityRemove.Enabled = lstWebTicketDensityList.SelectedIndex >= 0 AndAlso _currentUserPermission(_currentTableName).Edit
	End Sub

	Private Sub UpdateWebTicketPrecisionVisible()
		trWebTicketDensityPrecisionControls.Visible = lstWebTicketDensityList.SelectedIndex >= 0
	End Sub

	Protected Sub ddlWebTicketDensityUnit_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlWebTicketDensityMass.SelectedIndexChanged, ddlWebTicketDensityVolume.SelectedIndexChanged
		UpdateWebTicketDensityAddEnabled()
	End Sub

	Protected Sub btnWebTicketDensityAdd_Click(sender As Object, e As EventArgs) Handles btnWebTicketDensityAdd.Click
		Dim text As String = String.Format("{0}/{1} 0.00", ddlWebTicketDensityMass.SelectedItem.Text, ddlWebTicketDensityVolume.SelectedItem.Text)
		Dim value As String = String.Format("{0}|{1}|0.00", ddlWebTicketDensityMass.SelectedValue, ddlWebTicketDensityVolume.SelectedValue)
		Dim item As New ListItem(text, value)
		lstWebTicketDensityList.Items.Add(item)
		lstWebTicketDensityList.SelectedIndex = lstWebTicketDensityList.Items.Count - 1 ' select the item we just added
		UpdateWebTicketDensityAddEnabled()
		UpdateWebTicketDensityRemoveEnabled()
		UpdateWebTicketPrecisionVisible()
	End Sub

	Protected Sub btnWebTicketDensityRemove_Click(sender As Object, e As EventArgs) Handles btnWebTicketDensityRemove.Click
		lstWebTicketDensityList.Items.RemoveAt(lstWebTicketDensityList.SelectedIndex)
		UpdateWebTicketDensityAddEnabled()
		UpdateWebTicketDensityRemoveEnabled()
		UpdateWebTicketPrecisionVisible()
	End Sub

	Protected Sub btnWebTicketDensityAddWhole_Click(sender As Object, e As EventArgs) Handles btnWebTicketDensityAddWhole.Click
		Dim parts() As String = lstWebTicketDensityList.SelectedValue.Split("|") ' should produce 3 parts (<mass Guid>|<volume Guid>|precision), no need to check array dimensions as long as the routine that populates the list only adds items that conform the format
		Dim whole As UInteger = 0
		Dim fractional As UInteger = UInteger.MaxValue
		KaUnit.GetPrecisionDigits(parts(2), whole, fractional)
		Dim precision As String = KaUnit.GetPrecisionString(whole + 1, fractional, ",", 0)
		lstWebTicketDensityList.Items(lstWebTicketDensityList.SelectedIndex).Value = String.Format("{0}|{1}|{2}", parts(0), parts(1), precision)
		parts = lstWebTicketDensityList.Items(lstWebTicketDensityList.SelectedIndex).Text.Split(" ")
		lstWebTicketDensityList.Items(lstWebTicketDensityList.SelectedIndex).Text = String.Format("{0} {1}", parts(0), precision)
	End Sub

	Protected Sub btnWebTicketDensityRemoveWhole_Click(sender As Object, e As EventArgs) Handles btnWebTicketDensityRemoveWhole.Click
		Dim parts() As String = lstWebTicketDensityList.SelectedValue.Split("|") ' should produce 3 parts (<mass Guid>|<volume Guid>|precision), no need to check array dimensions as long as the routine that populates the list only adds items that conform the format
		Dim whole As UInteger = 0
		Dim fractional As UInteger = UInteger.MaxValue
		KaUnit.GetPrecisionDigits(parts(2), whole, fractional)
		Dim precision As String = KaUnit.GetPrecisionString(Math.Max(whole - 1, 0), fractional, ",", 0)
		lstWebTicketDensityList.Items(lstWebTicketDensityList.SelectedIndex).Value = String.Format("{0}|{1}|{2}", parts(0), parts(1), precision)
		parts = lstWebTicketDensityList.Items(lstWebTicketDensityList.SelectedIndex).Text.Split(" ")
		lstWebTicketDensityList.Items(lstWebTicketDensityList.SelectedIndex).Text = String.Format("{0} {1}", parts(0), precision)
	End Sub

	Protected Sub btnWebTicketDensityAddFractional_Click(sender As Object, e As EventArgs) Handles btnWebTicketDensityAddFractional.Click
		Dim parts() As String = lstWebTicketDensityList.SelectedValue.Split("|") ' should produce 3 parts (<mass Guid>|<volume Guid>|precision), no need to check array dimensions as long as the routine that populates the list only adds items that conform the format
		Dim whole As UInteger = 0
		Dim fractional As UInteger = UInteger.MaxValue
		KaUnit.GetPrecisionDigits(parts(2), whole, fractional)
		Dim precision As String = KaUnit.GetPrecisionString(whole, fractional + 1, ",", 0)
		lstWebTicketDensityList.Items(lstWebTicketDensityList.SelectedIndex).Value = String.Format("{0}|{1}|{2}", parts(0), parts(1), precision)
		parts = lstWebTicketDensityList.Items(lstWebTicketDensityList.SelectedIndex).Text.Split(" ")
		lstWebTicketDensityList.Items(lstWebTicketDensityList.SelectedIndex).Text = String.Format("{0} {1}", parts(0), precision)
	End Sub

	Protected Sub btnWebTicketDensityRemoveFractional_Click(sender As Object, e As EventArgs) Handles btnWebTicketDensityRemoveFractional.Click
		Dim parts() As String = lstWebTicketDensityList.SelectedValue.Split("|") ' should produce 3 parts (<mass Guid>|<volume Guid>|precision), no need to check array dimensions as long as the routine that populates the list only adds items that conform the format
		Dim whole As UInteger = 0
		Dim fractional As UInteger = UInteger.MaxValue
		KaUnit.GetPrecisionDigits(parts(2), whole, fractional)
		Dim precision As String = KaUnit.GetPrecisionString(whole, Math.Max(fractional - 1, 0), ",", 0)
		lstWebTicketDensityList.Items(lstWebTicketDensityList.SelectedIndex).Value = String.Format("{0}|{1}|{2}", parts(0), parts(1), precision)
		parts = lstWebTicketDensityList.Items(lstWebTicketDensityList.SelectedIndex).Text.Split(" ")
		lstWebTicketDensityList.Items(lstWebTicketDensityList.SelectedIndex).Text = String.Format("{0} {1}", parts(0), precision)
	End Sub

	Private Sub PopulateWebTicketDensitySettings(connection As OleDbConnection, ownerId As Guid)
		lstWebTicketDensityList.Items.Clear()
		Dim unitAbbreviations As New Dictionary(Of Guid, String)
		Dim densityUnitSettings() As String = GetWebTicketSettingByOwnerId(connection, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_DENSITY_UNIT_PRECISION, "").Split(",")
		For Each densityUnitPrecision As String In densityUnitSettings
			Dim parts() As String = densityUnitPrecision.Split("|")
			If parts.Length = 3 Then
				Try ' to parse the unit precision unit IDs
					Dim massUnitId As Guid = Guid.Parse(parts(0))
					Dim volumeUnitId As Guid = Guid.Parse(parts(1))
					Dim text As String
					Try ' to get the unit abbreviation for the mass unit from the dictionary...
						text = unitAbbreviations(massUnitId)
					Catch ex As KeyNotFoundException ' the abbreviation wasn't in the dictionary...
						Dim abbreviation As String = New KaUnit(connection, massUnitId).Abbreviation
						unitAbbreviations(massUnitId) = abbreviation
						text = abbreviation
					End Try
					Try ' to get the unit abbreviation for the volume unit from the dictionary...
						text &= "/" & unitAbbreviations(volumeUnitId)
					Catch ex As KeyNotFoundException ' the abbreviation wasn't in the dictionary...
						Dim abbreviation As String = New KaUnit(connection, volumeUnitId).Abbreviation
						unitAbbreviations(volumeUnitId) = abbreviation
						text &= "/" & abbreviation
					End Try
					text &= " " & parts(2)
					lstWebTicketDensityList.Items.Add(New ListItem(text, densityUnitPrecision))
				Catch ex As Exception ' the density unit precision setting wasn't formatted correctly...
				End Try
			End If
		Next
		UpdateWebTicketDensityAddEnabled()
		UpdateWebTicketDensityRemoveEnabled()
		UpdateWebTicketPrecisionVisible()
	End Sub

	Private Sub SaveWebTicketDensitySettings(webTicketSetting As String)
		Dim value As String = ""
		For Each item As ListItem In lstWebTicketDensityList.Items
			value &= IIf(value.Length > 0, ",", "") & item.Value
		Next
		KaSetting.WriteSetting(GetUserConnection(_currentUser.Id), webTicketSetting & "DensityUnitPrecision", value)
	End Sub

	Private Sub PopulateWebTicketCustomFieldsShownSettings(connection As OleDbConnection, ownerId As Guid)
		cblShowCustomFieldsOnDeliveryTicket.Items.Clear()
		Dim customFieldsShownList As New List(Of String)
		customFieldsShownList.AddRange(GetWebTicketSettingByOwnerId(connection, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_CUSTOM_FIELDS_ON_DELIVERY_TICKET, "").Split(","))
		Dim validTicketTables As String = Q(KaTicket.TABLE_NAME) & "," &
			Q(KaTicketCustomerAccount.TABLE_NAME) & "," &
			Q(KaTicketItem.TABLE_NAME) & "," &
			Q(KaTicketBulkItem.TABLE_NAME) & "," &
			Q(KaTicketTransport.TABLE_NAME) & "," &
			Q(KaTicketTransportCompartment.TABLE_NAME)
		For Each customTicketField As KaCustomField In KaCustomField.GetAll(connection, String.Format("{0} = {1} AND {2} IN ({3})", KaCustomField.FN_DELETED, Q(False), KaCustomField.FN_TABLE_NAME, validTicketTables), KaCustomField.FN_FIELD_NAME)
			cblShowCustomFieldsOnDeliveryTicket.Items.Add(New ListItem(Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(customTicketField.TableName.Replace(KaLocation.TABLE_NAME, "Facilities").Replace("_", " ") & ": " & customTicketField.FieldName), customTicketField.Id.ToString()))
			cblShowCustomFieldsOnDeliveryTicket.Items(cblShowCustomFieldsOnDeliveryTicket.Items.Count - 1).Selected = (customFieldsShownList.Contains(customTicketField.Id.ToString()))
		Next
		pnlDeliveryTicketCustomFieldsAssigned.Visible = (cblShowCustomFieldsOnDeliveryTicket.Items.Count > 0)
	End Sub

	Private Sub SaveWebTicketShowCustomFieldsSetting(ByVal webTicketSetting As String)
		Dim customFieldsShown As String = ""
		For Each customTicketField As ListItem In cblShowCustomFieldsOnDeliveryTicket.Items
			If customTicketField.Selected Then
				If customFieldsShown.Length > 0 Then customFieldsShown &= ","
				customFieldsShown &= customTicketField.Value
			End If
		Next
		KaSetting.WriteSetting(GetUserConnection(_currentUser.Id), webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_CUSTOM_FIELDS_ON_DELIVERY_TICKET, customFieldsShown)
	End Sub

	Private Sub PopulateCustomPreAndPostLoadQuestions(connection As OleDbConnection, ownerId As Guid)
		cblShowCustomPreLoadQuestions.Items.Clear()
		cblShowCustomPostLoadQuestions.Items.Clear()
		Dim customPreLoadQuestionsShowList As New List(Of String)
		customPreLoadQuestionsShowList.AddRange(GetWebTicketSettingByOwnerId(connection, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_CUSTOM_PRE_LOAD_QUESTIONS, "").Split(","))
		Dim customPostLoadQuestionsShowList As New List(Of String)
		customPostLoadQuestionsShowList.AddRange(GetWebTicketSettingByOwnerId(connection, ownerId, KaSetting.DefaultDeliveryWebTicketSettings.SN_CUSTOM_POST_LOAD_QUESTIONS, "").Split(","))

		Dim allCustomLoadQuestions As ArrayList = KaCustomLoadQuestionFields.GetAll(connection, "deleted = 0 AND (" & KaCustomLoadQuestionFields.FN_OWNER_ID & "=" & Q(ownerId) & " OR " & KaCustomLoadQuestionFields.FN_OWNER_ID & "=" & Q(Guid.Empty) & ")", "UPPER(name)")
		For Each question As KaCustomLoadQuestionFields In allCustomLoadQuestions
			If Not question.PostLoad Then
				'Pre
				cblShowCustomPreLoadQuestions.Items.Add(New ListItem(question.Name, question.Id.ToString))
				cblShowCustomPreLoadQuestions.Items(cblShowCustomPreLoadQuestions.Items.Count - 1).Selected = (customPreLoadQuestionsShowList.Contains(question.Id.ToString()))
			Else
				'Post
				cblShowCustomPostLoadQuestions.Items.Add(New ListItem(question.Name, question.Id.ToString))
				cblShowCustomPostLoadQuestions.Items(cblShowCustomPostLoadQuestions.Items.Count - 1).Selected = (customPostLoadQuestionsShowList.Contains(question.Id.ToString()))
			End If
		Next

		pnlCustomPreLoadQuestions.Visible = (cblShowCustomPreLoadQuestions.Items.Count > 0)
		pnlCustomPostLoadQuestions.Visible = (cblShowCustomPostLoadQuestions.Items.Count > 0)
	End Sub

	Private Sub SaveCustomPreAndPostLoadQuestions(ByVal webTicketSetting As String)
		Dim customPreLoadQuestionsShown As String = ""
		For Each customPreLoadQuestion As ListItem In cblShowCustomPreLoadQuestions.Items
			If customPreLoadQuestion.Selected Then
				If customPreLoadQuestionsShown.Length > 0 Then customPreLoadQuestionsShown &= ","
				customPreLoadQuestionsShown &= customPreLoadQuestion.Value
			End If
		Next
		KaSetting.WriteSetting(GetUserConnection(_currentUser.Id), webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_CUSTOM_PRE_LOAD_QUESTIONS, customPreLoadQuestionsShown)

		Dim customPostLoadQuestionsShown As String = ""
		For Each customPostLoadQuestion As ListItem In cblShowCustomPostLoadQuestions.Items
			If customPostLoadQuestion.Selected Then
				If customPostLoadQuestionsShown.Length > 0 Then customPostLoadQuestionsShown &= ","
				customPostLoadQuestionsShown &= customPostLoadQuestion.Value
			End If
		Next
		KaSetting.WriteSetting(GetUserConnection(_currentUser.Id), webTicketSetting & KaSetting.DefaultDeliveryWebTicketSettings.SN_CUSTOM_POST_LOAD_QUESTIONS, customPostLoadQuestionsShown)
	End Sub

	Protected Sub ddlProductGroup_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles ddlProductGroup.SelectedIndexChanged
		PopulateAdditionalUnitsToDisplayForWebticketProductGroups(Guid.Parse(ddlProductGroup.SelectedValue))
	End Sub

	Private Sub cbxShowAllCustomFieldsOnDeliveryTicket_CheckedChanged(sender As Object, e As System.EventArgs) Handles cbxShowAllCustomFieldsOnDeliveryTicket.CheckedChanged
		cblShowCustomFieldsOnDeliveryTicket.Enabled = Not cbxShowAllCustomFieldsOnDeliveryTicket.Checked
	End Sub

	Private Sub FertilizerGradeCheckedChanged(sender As Object, e As System.EventArgs) Handles chkShowFertilizerGrade.CheckedChanged
		pnlAnalysisDecimalCount.Visible = chkShowFertilizerGrade.Checked Or chkShowFertilizerGuaranteedAnalysis.Checked
		chkShowCompartmentFertilizerGrade.Enabled = chkShowFertilizerGrade.Checked
		If Not chkShowFertilizerGrade.Checked Then chkShowCompartmentFertilizerGrade.Checked = False
	End Sub

	Private Sub FertilizerGuaranteedAnalysisCheckedChanged(sender As Object, e As System.EventArgs) Handles chkShowFertilizerGuaranteedAnalysis.CheckedChanged
		pnlAnalysisDecimalCount.Visible = chkShowFertilizerGuaranteedAnalysis.Checked Or chkShowFertilizerGrade.Checked
		chkShowFertilizerGuaranteedAnalysisByCompartment.Enabled = chkShowFertilizerGuaranteedAnalysis.Checked
		If Not chkShowFertilizerGuaranteedAnalysis.Checked Then chkShowFertilizerGuaranteedAnalysisByCompartment.Checked = False
	End Sub

	Protected Sub chkDate_CheckedChanged(sender As Object, e As EventArgs) Handles chkDate.CheckedChanged
		chkShowTime.Enabled = chkDate.Checked
	End Sub

	Private Sub cbxShowAllCustomPreLoadQuestions_CheckedChanged(sender As Object, e As System.EventArgs) Handles cbxShowAllCustomPreLoadQuestions.CheckedChanged
		cblShowCustomPreLoadQuestions.Enabled = Not cbxShowAllCustomPreLoadQuestions.Checked
	End Sub

	Private Sub cbxShowAllCustomPostLoadQuestions_CheckedChanged(sender As Object, e As System.EventArgs) Handles cbxShowAllCustomPostLoadQuestions.CheckedChanged
		cblShowCustomPostLoadQuestions.Enabled = Not cbxShowAllCustomPostLoadQuestions.Checked
	End Sub

	Private Sub chkShowCompartmentBulkIngredients_CheckedChanged(sender As Object, e As System.EventArgs) Handles chkShowCompartmentBulkIngredients.CheckedChanged
		chkShowCompartmentBulkIngredientNotes.Enabled = chkShowCompartmentBulkIngredients.Checked And chkShowCompartmentBulkIngredients.Enabled
		chkShowCompartmentBulkIngredientEpaNumber.Enabled = chkShowCompartmentBulkIngredientNotes.Enabled
	End Sub

	Protected Sub chkShowApplicationRateOnTicket_CheckedChanged(sender As Object, e As EventArgs) Handles chkShowApplicationRateOnTicket.CheckedChanged
		chkUseOriginalOrdersApplicationRate.Enabled = chkShowApplicationRateOnTicket.Checked
	End Sub

	Protected Sub ToolkitScriptManager1_AsyncPostBackError(ByVal sender As Object, ByVal e As System.Web.UI.AsyncPostBackErrorEventArgs)
		ToolkitScriptManager1.AsyncPostBackErrorMessage = e.Exception.Message
	End Sub

	Private Sub DisplayJavaScriptMessage(key As String, script As String)
		DisplayJavaScriptMessage(key, script, False)
	End Sub

	Private Sub DisplayJavaScriptMessage(key As String, script As String, addScriptTags As Boolean)
		ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), key, script, addScriptTags)
	End Sub
End Class