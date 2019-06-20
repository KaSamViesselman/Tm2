Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb
Public Class DefaultContainerLabelSettings
	Inherits System.Web.UI.Page

	Private _currentUser As KaUser
	Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
	Private _currentTableName As String = KaSetting.TABLE_NAME


	Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
		Response.Cache.SetCacheability(HttpCacheability.NoCache)
		_currentUser = Utilities.GetUser(Me)
		_currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "GeneralSettings")
		If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")
		btnSaveBayWebTicketSettings.Enabled = _currentUserPermission(_currentTableName).Edit
		btnSaveProductGroupAdditionalUnits.Enabled = btnSaveBayWebTicketSettings.Enabled
		btnDeleteBayWebTicketSettings.Enabled = _currentUserPermission(_currentTableName).Delete
		If Not Page.IsPostBack Then
			PopulateBaysList()
			PopulateProductGroupsCombo()
			PopulateAdditionalUnitsList()
			PopulateWebTicketDensityUnits()
			PopulateWebTicketBaySettings(Guid.Empty)

			Utilities.ConfirmBox(Me.btnDeleteBayWebTicketSettings, "Are you sure you want to delete the web ticket settings for this bay?")
		End If
	End Sub

	Private Sub PopulateBaysList()
		ddlWebTicketBay.Items.Clear()
		ddlWebTicketBay.Items.Add(New ListItem("All bays", Guid.Empty.ToString))
		For Each o As KaBay In KaBay.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
			ddlWebTicketBay.Items.Add(New ListItem(o.Name, o.Id.ToString))
		Next
	End Sub

#Region " Default web ticket settings "
	Private Sub PopulateWebTicketBaySettings(ByVal bayId As Guid)
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

		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim allSettings As ArrayList = KaSetting.GetAll(connection, "name like 'ContainerLabelSetting:" & bayId.ToString & "%' and deleted=0", "")
		lblWebTicketSettingsExist.Visible = (allSettings.Count > 0)
		lblWebTicketSettingsExist.Text = "Settings exist"
		If bayId.Equals(Guid.Empty) Then
			Dim settingsValidForBays As String = ""
			For Each possibleBay As KaBay In KaBay.GetAll(connection, "deleted = 0", "name ASC")
				Dim bayTicketSettingsCountRdr As OleDbDataReader = Tm2Database.ExecuteReader(connection, "SELECT COUNT(*) FROM settings WHERE name LIKE 'ContainerLabelSetting:" & possibleBay.Id.ToString & "%' AND deleted = 0")
				If bayTicketSettingsCountRdr.Read() AndAlso bayTicketSettingsCountRdr.Item(0) = 0 Then
					If settingsValidForBays.Length > 0 Then settingsValidForBays &= ", "
					settingsValidForBays &= possibleBay.Name
				End If
				bayTicketSettingsCountRdr.Close()
			Next
			If settingsValidForBays.Length > 0 Then lblWebTicketSettingsExist.Text = "These settings are valid for " & settingsValidForBays
		End If

		For Each unitId As String In DefaultContainerLabel.GetSettingByBayId(connection, bayId, KaSetting.DefaultContainerLabelSettings.SN_ADDITIONAL_UNITS_FOR_TICKET, KaSetting.DefaultContainerLabelSettings.SD_ADDITIONAL_UNITS_FOR_TICKET).Trim().Split(",")
			For Each item As ListItem In cblAdditionalUnitsForTicket.Items
				If item.Value = unitId Then
					item.Selected = True
					Exit For
				End If
			Next
		Next

		tbxLabelHeight.Text = DefaultContainerLabel.GetSettingByBayId(connection, bayId, KaSetting.DefaultContainerLabelSettings.SN_LABEL_HEIGHT, KaSetting.DefaultContainerLabelSettings.SD_LABEL_HEIGHT)
		tbxLabelWidth.Text = DefaultContainerLabel.GetSettingByBayId(connection, bayId, KaSetting.DefaultContainerLabelSettings.SN_LABEL_WIDTH, KaSetting.DefaultContainerLabelSettings.SD_LABEL_WIDTH)
		Boolean.TryParse(DefaultContainerLabel.GetSettingByBayId(connection, bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_TICKET_NUMBER, KaSetting.DefaultContainerLabelSettings.SD_SHOW_TICKET_NUMBER), chkShowTicketNumber.Checked)
		Boolean.TryParse(DefaultContainerLabel.GetSettingByBayId(connection, bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_ORDER_NUMBER, KaSetting.DefaultContainerLabelSettings.SD_SHOW_ORDER_NUMBER), chkShowOrderNumber.Checked)
		Boolean.TryParse(DefaultContainerLabel.GetSettingByBayId(connection, bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_DATE, KaSetting.DefaultContainerLabelSettings.SD_SHOW_DATE), chkDate.Checked)
		Boolean.TryParse(DefaultContainerLabel.GetSettingByBayId(connection, bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_TIME, KaSetting.DefaultContainerLabelSettings.SD_SHOW_TIME), chkShowTime.Checked)
		chkDate_CheckedChanged(chkDate, New EventArgs())
		Boolean.TryParse(DefaultContainerLabel.GetSettingByBayId(connection, bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_OWNER, KaSetting.DefaultContainerLabelSettings.SD_SHOW_OWNER), chkOwner.Checked)
		Boolean.TryParse(DefaultContainerLabel.GetSettingByBayId(connection, bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_BRANCH_LOCATION, KaSetting.DefaultContainerLabelSettings.SD_SHOW_BRANCH_LOCATION), chkBranchLocation.Checked)
		Boolean.TryParse(DefaultContainerLabel.GetSettingByBayId(connection, bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_CUSTOMER, KaSetting.DefaultContainerLabelSettings.SD_SHOW_CUSTOMER), chkCustomer.Checked)
		Boolean.TryParse(DefaultContainerLabel.GetSettingByBayId(connection, bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_CUSTOMER_LOCATION, KaSetting.DefaultContainerLabelSettings.SD_SHOW_CUSTOMER_LOCATION), chkCustomerLocation.Checked)
		Boolean.TryParse(DefaultContainerLabel.GetSettingByBayId(connection, bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_BULK_PRODUCT_SUMMARY_TOTALS, KaSetting.DefaultContainerLabelSettings.SD_SHOW_BULK_PRODUCT_SUMMARY_TOTALS), chkShowBulkProductSummaryTotals.Checked)
		Boolean.TryParse(DefaultContainerLabel.GetSettingByBayId(connection, bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_BULK_PRODUCT_NOTES_ON_SUMMARY_ON_TICKET, KaSetting.DefaultContainerLabelSettings.SD_SHOW_BULK_PRODUCT_NOTES_ON_SUMMARY_ON_TICKET), chkShowBulkProductNotesSummaryTotals.Checked)
		Boolean.TryParse(DefaultContainerLabel.GetSettingByBayId(connection, bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_BULK_PRODUCT_EPA_NUMBER_ON_SUMMARY_ON_TICKET, KaSetting.DefaultContainerLabelSettings.SD_SHOW_BULK_PRODUCT_EPA_NUMBER_ON_SUMMARY_ON_TICKET), chkShowBulkProductEpaNumberSummaryTotals.Checked)
		Boolean.TryParse(DefaultContainerLabel.GetSettingByBayId(connection, bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_PRODUCT_SUMMARY_TOTALS, KaSetting.DefaultContainerLabelSettings.SD_SHOW_PRODUCT_SUMMARY_TOTALS), chkShowProductSummaryTotals.Checked)
		Boolean.TryParse(DefaultContainerLabel.GetSettingByBayId(connection, bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_PRODUCT_NOTES_ON_SUMMARY_ON_TICKET, KaSetting.DefaultContainerLabelSettings.SD_SHOW_PRODUCT_NOTES_ON_SUMMARY_ON_TICKET), chkShowProductNotesSummaryTotals.Checked)
		Boolean.TryParse(DefaultContainerLabel.GetSettingByBayId(connection, bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_REQUESTED_QUANTITIES, KaSetting.DefaultContainerLabelSettings.SD_SHOW_REQUESTED_QUANTITIES), chkShowRequestedQuantities.Checked)
		Boolean.TryParse(DefaultContainerLabel.GetSettingByBayId(connection, bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_TOTAL, KaSetting.DefaultContainerLabelSettings.SD_SHOW_TOTAL), chkTotal.Checked)
		Boolean.TryParse(DefaultContainerLabel.GetSettingByBayId(connection, bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_PRODUCT_HAZARDOUS_MATERIAL, KaSetting.DefaultContainerLabelSettings.SD_SHOW_PRODUCT_HAZARDOUS_MATERIAL), chkProductHazardousMaterial.Checked)
		Boolean.TryParse(DefaultContainerLabel.GetSettingByBayId(connection, bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_CARRIER, KaSetting.DefaultContainerLabelSettings.SD_SHOW_CARRIER), chkCarrierId.Checked)
		Boolean.TryParse(DefaultContainerLabel.GetSettingByBayId(connection, bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_TRANSPORT, KaSetting.DefaultContainerLabelSettings.SD_SHOW_TRANSPORT), chkTransport.Checked)
		Boolean.TryParse(DefaultContainerLabel.GetSettingByBayId(connection, bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_TRANSPORT_TARE_WEIGHTS, KaSetting.DefaultContainerLabelSettings.SD_SHOW_TRANSPORT_TARE_WEIGHTS), chkShowTicketTransportTareInfo.Checked)
		Boolean.TryParse(DefaultContainerLabel.GetSettingByBayId(connection, bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_DENSITY_ON_TICKET, KaSetting.DefaultContainerLabelSettings.SD_SHOW_DENSITY_ON_TICKET), chkShowDensityOnTicket.Checked)
		Boolean.TryParse(DefaultContainerLabel.GetSettingByBayId(connection, bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_DERIVED_FROM_ON_TICKET, KaSetting.DefaultContainerLabelSettings.SD_SHOW_DERIVED_FROM_ON_TICKET), chkShowDerivedFrom.Checked)
		Boolean.TryParse(DefaultContainerLabel.GetSettingByBayId(connection, bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_RINSE_ENTRIES, KaSetting.DefaultContainerLabelSettings.SD_SHOW_RINSE_ENTRIES), chkShowRinseEntries.Checked)
		Boolean.TryParse(DefaultContainerLabel.GetSettingByBayId(connection, bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_LABEL_COUNT, KaSetting.DefaultContainerLabelSettings.SD_SHOW_LABEL_COUNT), cbxShowLabelCount.Checked)

		Dim truckWeightOrder() As String = DefaultContainerLabel.GetSettingByBayId(connection, bayId, KaSetting.DefaultContainerLabelSettings.SN_TRUCK_TGN_ORDER, KaSetting.DefaultContainerLabelSettings.SD_TRUCK_TGN_ORDER).Split("-")
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
		Boolean.TryParse(DefaultContainerLabel.GetSettingByBayId(connection, bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_DRIVER, KaSetting.DefaultContainerLabelSettings.SD_SHOW_DRIVER), chkDriverName.Checked)
		Boolean.TryParse(DefaultContainerLabel.GetSettingByBayId(connection, bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_DRIVER_NUMBER, KaSetting.DefaultContainerLabelSettings.SD_SHOW_DRIVER_NUMBER), chkDriverNumber.Checked)
		Boolean.TryParse(DefaultContainerLabel.GetSettingByBayId(connection, bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_APPLICATOR, KaSetting.DefaultContainerLabelSettings.SD_SHOW_APPLICATOR), chkShowApplicatorOnTicket.Checked)
		Boolean.TryParse(DefaultContainerLabel.GetSettingByBayId(connection, bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_ACRES, KaSetting.DefaultContainerLabelSettings.SD_SHOW_ACRES), chkAcresOnTicket.Checked)
		Boolean.TryParse(DefaultContainerLabel.GetSettingByBayId(connection, bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_FERTILIZER_GRADE, KaSetting.DefaultContainerLabelSettings.SD_SHOW_FERTILIZER_GRADE), chkShowFertilizerGrade.Checked)
		Boolean.TryParse(DefaultContainerLabel.GetSettingByBayId(connection, bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_FERTILIZER_GUARANTEED_ANALYSIS, KaSetting.DefaultContainerLabelSettings.SD_SHOW_FERTILIZER_GUARANTEED_ANALYSIS), chkShowFertilizerGuaranteedAnalysis.Checked)
		chkDriverNumber.Enabled = chkDriverName.Checked
		Try
			Integer.TryParse(DefaultContainerLabel.GetSettingByBayId(connection, bayId, KaSetting.DefaultContainerLabelSettings.SN_GRADE_ANALYSIS_DECIMAL_COUNT_GREATER_THAN_ONE, KaSetting.DefaultContainerLabelSettings.SD_GRADE_ANALYSIS_DECIMAL_COUNT_GREATER_THAN_ONE), ddlGradeAnalysisDecimalCountGreaterThanOne.SelectedIndex)
		Catch ex As IndexOutOfRangeException
			ddlGradeAnalysisDecimalCountGreaterThanOne.SelectedIndex = 0
		End Try
		Try
			Integer.TryParse(DefaultContainerLabel.GetSettingByBayId(connection, bayId, KaSetting.DefaultContainerLabelSettings.SN_GRADE_ANALYSIS_DECIMAL_COUNT_LESS_THAN_ONE, KaSetting.DefaultContainerLabelSettings.SD_GRADE_ANALYSIS_DECIMAL_COUNT_LESS_THAN_ONE), ddlGradeAnalysisDecimalCountLessThanOne.SelectedIndex)
		Catch ex As IndexOutOfRangeException
			ddlGradeAnalysisDecimalCountLessThanOne.SelectedIndex = 2
		End Try
		Boolean.TryParse(DefaultContainerLabel.GetSettingByBayId(connection, bayId, KaSetting.DefaultContainerLabelSettings.SN_ANALYSIS_ENTRIES_ROUNDED_DOWN, KaSetting.DefaultContainerLabelSettings.SD_ANALYSIS_ENTRIES_ROUNDED_DOWN), cbxAnalysisEntriesRoundedDown.Checked)
		Boolean.TryParse(DefaultContainerLabel.GetSettingByBayId(connection, bayId, KaSetting.DefaultContainerLabelSettings.SN_ANALYSIS_SHOW_TRAILING_ZEROS, KaSetting.DefaultContainerLabelSettings.SD_ANALYSIS_SHOW_TRAILING_ZEROS), cbxAnalysisShowTrailingZeros.Checked)
		Boolean.TryParse(DefaultContainerLabel.GetSettingByBayId(connection, bayId, KaSetting.DefaultContainerLabelSettings.SN_HIDE_ZERO_PERCENT_ANALYSIS_NUTRIENTS, KaSetting.DefaultContainerLabelSettings.SD_HIDE_ZERO_PERCENT_ANALYSIS_NUTRIENTS), cbxHideZeroPercentAnalysisNutrients.Checked)
		Boolean.TryParse(DefaultContainerLabel.GetSettingByBayId(connection, bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_CUSTOMER_NOTES_ON_TICKET, KaSetting.DefaultContainerLabelSettings.SD_SHOW_CUSTOMER_NOTES_ON_TICKET), chkShowCustomerNotesOnTicket.Checked)
		Boolean.TryParse(DefaultContainerLabel.GetSettingByBayId(connection, bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_CUSTOMER_DESTINATION_NOTES_ON_TICKET, KaSetting.DefaultContainerLabelSettings.SD_SHOW_CUSTOMER_DESTINATION_NOTES_ON_TICKET), chkShowCustomerDestinationNotesOnTicket.Checked)
		Boolean.TryParse(DefaultContainerLabel.GetSettingByBayId(connection, bayId, KaSetting.DefaultContainerLabelSettings.SN_NTEP_COMPLIANT, KaSetting.DefaultContainerLabelSettings.SD_NTEP_COMPLIANT), chkNtepCompliant.Checked)
		Boolean.TryParse(DefaultContainerLabel.GetSettingByBayId(connection, bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_LOADED_BY_ON_TICKET, KaSetting.DefaultContainerLabelSettings.SD_SHOW_LOADED_BY_ON_TICKET), cbxShowLoadedBy.Checked)
		Boolean.TryParse(DefaultContainerLabel.GetSettingByBayId(connection, bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_APPLICATION_RATE_ON_TICKET, KaSetting.DefaultContainerLabelSettings.SD_SHOW_APPLICATION_RATE_ON_TICKET), chkShowApplicationRateOnTicket.Checked)
		Boolean.TryParse(DefaultContainerLabel.GetSettingByBayId(connection, bayId, KaSetting.DefaultContainerLabelSettings.SN_USE_ORIGINAL_ORDERS_APPLICATION_RATE_ON_TICKET, KaSetting.DefaultContainerLabelSettings.SD_USE_ORIGINAL_ORDERS_APPLICATION_RATE_ON_TICKET), chkUseOriginalOrdersApplicationRate.Checked)
		tbxDisclaimer.Text = DefaultContainerLabel.GetSettingByBayId(connection, bayId, KaSetting.DefaultContainerLabelSettings.SN_DISCLAIMER, KaSetting.DefaultContainerLabelSettings.SD_DISCLAIMER)
		tbxBlank1.Text = DefaultContainerLabel.GetSettingByBayId(connection, bayId, KaSetting.DefaultContainerLabelSettings.SN_BLANK1, KaSetting.DefaultContainerLabelSettings.SD_BLANK1)
		tbxBlank2.Text = DefaultContainerLabel.GetSettingByBayId(connection, bayId, KaSetting.DefaultContainerLabelSettings.SN_BLANK2, KaSetting.DefaultContainerLabelSettings.SD_BLANK2)
		tbxBlank3.Text = DefaultContainerLabel.GetSettingByBayId(connection, bayId, KaSetting.DefaultContainerLabelSettings.SN_BLANK3, KaSetting.DefaultContainerLabelSettings.SD_BLANK3)
		Boolean.TryParse(DefaultContainerLabel.GetSettingByBayId(connection, bayId, KaSetting.DefaultContainerLabelSettings.SN_DISPLAY_BLEND_GROUP_NAME_AS_PRODUCT_NAME, KaSetting.DefaultContainerLabelSettings.SD_DISPLAY_BLEND_GROUP_NAME_AS_PRODUCT_NAME), chkDisplayBlendGroupNameAsProductName.Checked)
		Boolean.TryParse(DefaultContainerLabel.GetSettingByBayId(connection, bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_COMMENTS, KaSetting.DefaultContainerLabelSettings.SN_SHOW_COMMENTS), chkShowComments.Checked)

		PopulateWebTicketDensitySettings(connection, bayId)
		Boolean.TryParse(DefaultContainerLabel.GetSettingByBayId(connection, bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_ALL_CUSTOM_FIELDS_ON_DELIVERY_TICKET, KaSetting.DefaultContainerLabelSettings.SD_SHOW_ALL_CUSTOM_FIELDS_ON_DELIVERY_TICKET), cbxShowAllCustomFieldsOnLabel.Checked)
		PopulateWebTicketCustomFieldsShownSettings(connection, bayId)
		Boolean.TryParse(DefaultContainerLabel.GetSettingByBayId(connection, bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_ALL_CUSTOM_PRE_LOAD_QUESTIONS, KaSetting.DefaultContainerLabelSettings.SD_SHOW_ALL_CUSTOM_PRE_LOAD_QUESTIONS), cbxShowAllCustomPreLoadQuestions.Checked)
		Boolean.TryParse(DefaultContainerLabel.GetSettingByBayId(connection, bayId, KaSetting.DefaultContainerLabelSettings.SN_SHOW_ALL_CUSTOM_POST_LOAD_QUESTIONS, KaSetting.DefaultContainerLabelSettings.SD_SHOW_ALL_CUSTOM_POST_LOAD_QUESTIONS), cbxShowAllCustomPostLoadQuestions.Checked)
		PopulateCustomPreAndPostLoadQuestions(connection, bayId)

		ShowSummaryCheckedChanged(Nothing, Nothing)
		cbxShowAllCustomFieldsOnLabel_CheckedChanged(cbxShowAllCustomFieldsOnLabel, New EventArgs())
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
			cblAdditionalUnitsForProductGroup.ClearSelection()
			Dim webTicketSettingFormat As String = "ContainerLabelSetting:{0}:{1}/" & KaSetting.DefaultContainerLabelSettings.SN_ADDITIONAL_UNITS_FOR_PRODUCT_GROUPS
			Dim webTicketSetting As String = String.Format(webTicketSettingFormat, Guid.Empty.ToString(), productGroupId.ToString())
			Dim defaultOwnerProductUnits As String = KaSetting.GetSetting(connection, webTicketSetting, "")
			webTicketSetting = String.Format(webTicketSettingFormat, ddlWebTicketBay.SelectedValue, productGroupId.ToString())
			For Each unitIdString As String In KaSetting.GetSetting(connection, webTicketSetting, defaultOwnerProductUnits, False, Nothing).Trim().Split(",")

				For Each item As ListItem In cblAdditionalUnitsForProductGroup.Items
					If item.Value = unitIdString Then
						item.Selected = True
						Exit For
					End If
				Next
			Next

			Dim productGroupDensityMassUnitIdSettingFormat As String = "ContainerLabelSetting:{0}:{1}/ProductGroupDensityMassUnitId"
			Dim productGroupDensityMassUnitIdSetting As String = String.Format(productGroupDensityMassUnitIdSettingFormat, ddlWebTicketBay.SelectedValue, productGroupId.ToString())
			Dim productGroupDensityMassUnitId As Guid = Guid.Parse(KaSetting.GetSetting(connection, productGroupDensityMassUnitIdSetting, KaUnit.GetSystemDefaultMassUnitOfMeasure(connection, Nothing).ToString(), False, Nothing))
			Try
				ddlProductGroupDensityMassUnit.SelectedValue = productGroupDensityMassUnitId.ToString
			Catch ex As Exception
				'Suppress
			End Try

			Dim productGroupDensityVolumeUnitIdSettingFormat As String = "ContainerLabelSetting:{0}:{1}/ProductGroupDensityVolumeUnitId"
			Dim productGroupDensityVolumeUnitIdSetting As String = String.Format(productGroupDensityVolumeUnitIdSettingFormat, ddlWebTicketBay.SelectedValue, productGroupId.ToString())
			Dim productGroupDensityVolumeUnitId As Guid = Guid.Parse(KaSetting.GetSetting(connection, productGroupDensityVolumeUnitIdSetting, KaUnit.GetSystemDefaultVolumeUnitOfMeasure(connection, Nothing).ToString(), False, Nothing))
			Try
				ddlProductGroupDensityVolumeUnit.SelectedValue = productGroupDensityVolumeUnitId.ToString
			Catch ex As Exception
				'Suppress
			End Try
		End If
	End Sub

	Private Sub SaveWebTicketBaySettings(ByVal bayId As Guid)
		If tbxLabelHeight.Text.Length = 0 Then tbxLabelHeight.Text = "0"
		If tbxLabelWidth.Text.Length = 0 Then tbxLabelWidth.Text = "0"
		If Not ValidateWebTicketBaySettings(bayId) Then Exit Sub

		Dim connection As OleDbConnection = New OleDbConnection(Tm2Database.GetDbConnection())
		Dim transaction As OleDbTransaction = Nothing
		Try
			connection.Open()
			transaction = connection.BeginTransaction

			Dim webTicketSetting As String = "ContainerLabelSetting:" & bayId.ToString() & "/"
			KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultContainerLabelSettings.SN_LABEL_HEIGHT, tbxLabelHeight.Text, transaction)
			KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultContainerLabelSettings.SN_LABEL_WIDTH, tbxLabelWidth.Text, transaction)
			KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultContainerLabelSettings.SN_SHOW_TICKET_NUMBER, chkShowTicketNumber.Checked, transaction)
			KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultContainerLabelSettings.SN_SHOW_ORDER_NUMBER, chkShowOrderNumber.Checked, transaction)
			Dim list As String = ""
			For Each item As ListItem In cblAdditionalUnitsForTicket.Items
				If item.Selected Then list &= IIf(list.Length > 0, ",", "") & item.Value
			Next
			KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultContainerLabelSettings.SN_ADDITIONAL_UNITS_FOR_TICKET, list, transaction)
			KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultContainerLabelSettings.SN_SHOW_DATE, chkDate.Checked, transaction)
			KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultContainerLabelSettings.SN_SHOW_TIME, chkShowTime.Checked, transaction)
			KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultContainerLabelSettings.SN_SHOW_OWNER, chkOwner.Checked, transaction)
			KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultContainerLabelSettings.SN_SHOW_BRANCH_LOCATION, chkBranchLocation.Checked, transaction)
			KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultContainerLabelSettings.SN_SHOW_CUSTOMER_LOCATION, chkCustomerLocation.Checked, transaction)
			KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultContainerLabelSettings.SN_SHOW_CUSTOMER, chkCustomer.Checked, transaction)
			KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultContainerLabelSettings.SN_SHOW_BULK_PRODUCT_SUMMARY_TOTALS, chkShowBulkProductSummaryTotals.Checked, transaction)
			KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultContainerLabelSettings.SN_SHOW_BULK_PRODUCT_NOTES_ON_SUMMARY_ON_TICKET, chkShowBulkProductNotesSummaryTotals.Checked, transaction)
			KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultContainerLabelSettings.SN_SHOW_BULK_PRODUCT_EPA_NUMBER_ON_SUMMARY_ON_TICKET, chkShowBulkProductEpaNumberSummaryTotals.Checked, transaction)
			KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultContainerLabelSettings.SN_SHOW_PRODUCT_SUMMARY_TOTALS, chkShowProductSummaryTotals.Checked, transaction)
			KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultContainerLabelSettings.SN_SHOW_PRODUCT_NOTES_ON_SUMMARY_ON_TICKET, chkShowProductNotesSummaryTotals.Checked, transaction)
			KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultContainerLabelSettings.SN_SHOW_REQUESTED_QUANTITIES, chkShowRequestedQuantities.Checked, transaction)
			KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultContainerLabelSettings.SN_SHOW_TOTAL, chkTotal.Checked, transaction)
			KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultContainerLabelSettings.SN_SHOW_PRODUCT_HAZARDOUS_MATERIAL, chkProductHazardousMaterial.Checked, transaction)
			KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultContainerLabelSettings.SN_SHOW_CARRIER, chkCarrierId.Checked, transaction)
			KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultContainerLabelSettings.SN_SHOW_TRANSPORT, chkTransport.Checked, transaction)
			KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultContainerLabelSettings.SN_SHOW_DENSITY_ON_TICKET, chkShowDensityOnTicket.Checked, transaction)
			KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultContainerLabelSettings.SN_SHOW_DERIVED_FROM_ON_TICKET, chkShowDerivedFrom.Checked, transaction)
			KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultContainerLabelSettings.SN_SHOW_LOADED_BY_ON_TICKET, cbxShowLoadedBy.Checked, transaction)
			If chkTransport.Checked Then
				KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultContainerLabelSettings.SN_SHOW_TRANSPORT_TARE_WEIGHTS, chkShowTicketTransportTareInfo.Checked, transaction)
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
				KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultContainerLabelSettings.SN_TRUCK_TGN_ORDER, truckTGNOrder, transaction)
			End If
			KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultContainerLabelSettings.SN_SHOW_DRIVER, chkDriverName.Checked, transaction)
			KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultContainerLabelSettings.SN_SHOW_DRIVER_NUMBER, chkDriverNumber.Checked, transaction)
			KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultContainerLabelSettings.SN_SHOW_APPLICATOR, chkShowApplicatorOnTicket.Checked, transaction)
			KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultContainerLabelSettings.SN_SHOW_ACRES, chkAcresOnTicket.Checked, transaction)
			KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultContainerLabelSettings.SN_SHOW_FERTILIZER_GRADE, chkShowFertilizerGrade.Checked, transaction)
			KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultContainerLabelSettings.SN_SHOW_FERTILIZER_GUARANTEED_ANALYSIS, chkShowFertilizerGuaranteedAnalysis.Checked, transaction)
			KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultContainerLabelSettings.SN_ANALYSIS_SHOW_TRAILING_ZEROS, cbxAnalysisShowTrailingZeros.Checked, transaction)
			KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultContainerLabelSettings.SN_HIDE_ZERO_PERCENT_ANALYSIS_NUTRIENTS, cbxHideZeroPercentAnalysisNutrients.Checked, transaction)
			KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultContainerLabelSettings.SN_GRADE_ANALYSIS_DECIMAL_COUNT_GREATER_THAN_ONE, ddlGradeAnalysisDecimalCountGreaterThanOne.SelectedIndex, transaction)
			KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultContainerLabelSettings.SN_GRADE_ANALYSIS_DECIMAL_COUNT_LESS_THAN_ONE, ddlGradeAnalysisDecimalCountLessThanOne.SelectedIndex, transaction)
			KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultContainerLabelSettings.SN_ANALYSIS_ENTRIES_ROUNDED_DOWN, cbxAnalysisEntriesRoundedDown.Checked, transaction)
			KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultContainerLabelSettings.SN_SHOW_CUSTOMER_NOTES_ON_TICKET, chkShowCustomerNotesOnTicket.Checked, transaction)
			KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultContainerLabelSettings.SN_SHOW_CUSTOMER_DESTINATION_NOTES_ON_TICKET, chkShowCustomerDestinationNotesOnTicket.Checked, transaction)
			KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultContainerLabelSettings.SN_NTEP_COMPLIANT, chkNtepCompliant.Checked, transaction)
			KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultContainerLabelSettings.SN_DISCLAIMER, tbxDisclaimer.Text, transaction)
			KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultContainerLabelSettings.SN_BLANK1, tbxBlank1.Text, transaction)
			KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultContainerLabelSettings.SN_BLANK2, tbxBlank2.Text, transaction)
			KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultContainerLabelSettings.SN_BLANK3, tbxBlank3.Text, transaction)
			KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultContainerLabelSettings.SN_DISPLAY_BLEND_GROUP_NAME_AS_PRODUCT_NAME, chkDisplayBlendGroupNameAsProductName.Checked, transaction)
			KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultContainerLabelSettings.SN_SHOW_ALL_CUSTOM_FIELDS_ON_DELIVERY_TICKET, cbxShowAllCustomFieldsOnLabel.Checked, transaction)
			KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultContainerLabelSettings.SN_SHOW_ALL_CUSTOM_PRE_LOAD_QUESTIONS, cbxShowAllCustomPreLoadQuestions.Checked, transaction)
			KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultContainerLabelSettings.SN_SHOW_ALL_CUSTOM_POST_LOAD_QUESTIONS, cbxShowAllCustomPostLoadQuestions.Checked, transaction)
			KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultContainerLabelSettings.SN_SHOW_APPLICATION_RATE_ON_TICKET, chkShowApplicationRateOnTicket.Checked, transaction)
			KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultContainerLabelSettings.SN_USE_ORIGINAL_ORDERS_APPLICATION_RATE_ON_TICKET, chkUseOriginalOrdersApplicationRate.Checked, transaction)
			KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultContainerLabelSettings.SN_SHOW_RINSE_ENTRIES, chkShowRinseEntries.Checked, transaction)
			KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultContainerLabelSettings.SN_SHOW_COMMENTS, chkShowComments.Checked, transaction)
			KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultContainerLabelSettings.SN_SHOW_LABEL_COUNT, cbxShowLabelCount.Checked, transaction)

			SaveWebTicketDensitySettings(connection, transaction, webTicketSetting)
			SaveWebTicketShowCustomFieldsSetting(connection, transaction, webTicketSetting)
			SaveCustomPreAndPostLoadQuestions(connection, transaction, webTicketSetting)
			transaction.Commit()
		Catch ex As Exception
			If transaction IsNot Nothing Then transaction.Rollback()
		Finally
			If transaction IsNot Nothing Then transaction.Dispose()
			connection.Close()
		End Try
		PopulateWebTicketBaySettings(bayId)
	End Sub

	Private Function ValidateWebTicketBaySettings(ByVal bayId As Guid) As Boolean
		Dim labelSize As Double
		If Not Double.TryParse(tbxLabelHeight.Text, labelSize) OrElse labelSize < 0 Then
			DisplayJavaScriptMessage("InvalidLabelHeight", Utilities.JsAlert("The label height must be greater than or equal to 0."))
			Return False
		ElseIf Not Double.TryParse(tbxLabelWidth.Text, labelSize) OrElse labelSize < 0 Then
			DisplayJavaScriptMessage("InvalidLabelWidth", Utilities.JsAlert("The label width must be greater than or equal to 0."))
			Return False
		End If
		Return True
	End Function

	Protected Sub btnSaveProductGroupAdditionalUnits_Click(sender As Object, e As EventArgs) Handles btnSaveProductGroupAdditionalUnits.Click
		If Guid.Parse(ddlProductGroup.SelectedValue) <> Guid.Empty Then
			Dim c As OleDbConnection = GetUserConnection(_currentUser.Id)
			Dim webTicketSetting As String = "ContainerLabelSetting:" & ddlWebTicketBay.SelectedValue & ":" & ddlProductGroup.SelectedValue

			Dim list As String = ""
			For Each item As ListItem In cblAdditionalUnitsForProductGroup.Items
				If item.Selected Then list &= IIf(list.Length > 0, ",", "") & item.Value
			Next
			KaSetting.WriteSetting(c, webTicketSetting & "/" & KaSetting.DefaultContainerLabelSettings.SN_ADDITIONAL_UNITS_FOR_PRODUCT_GROUPS, list)

			Dim productGroupDensityMassUnitIdSettingFormat As String = "ContainerLabelSetting:{0}:{1}/ProductGroupDensityMassUnitId"
			Dim productGroupDensityMassUnitIdSetting As String = String.Format(productGroupDensityMassUnitIdSettingFormat, ddlWebTicketBay.SelectedValue, ddlProductGroup.SelectedValue)
			Dim productGroupDensityMassUnitId As Guid = Guid.Parse(ddlProductGroupDensityMassUnit.SelectedValue)
			KaSetting.WriteSetting(c, productGroupDensityMassUnitIdSetting, productGroupDensityMassUnitId.ToString())

			Dim productGroupDensityVolumeUnitIdSettingFormat As String = "ContainerLabelSetting:{0}:{1}/ProductGroupDensityVolumeUnitId"
			Dim productGroupDensityVolumeUnitIdSetting As String = String.Format(productGroupDensityVolumeUnitIdSettingFormat, ddlWebTicketBay.SelectedValue, ddlProductGroup.SelectedValue)
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

	Private Sub DeleteWebTicketBaySettings(ByVal bayId As Guid)
		'Delete will set all the ticket settings to their 'default' values
		Dim c As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim allSettings As ArrayList = KaSetting.GetAll(c, "name like 'ContainerLabelSetting:" & bayId.ToString & "%' and deleted=0", "")
		For Each setting As KaSetting In allSettings
			Tm2Database.ExecuteNonQuery(c, "Delete from settings where id = " & Q(setting.Id))
		Next
		PopulateWebTicketBaySettings(bayId)
	End Sub

	Private Sub ddlWebTicketBay_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles ddlWebTicketBay.SelectedIndexChanged
		PopulateWebTicketBaySettings(Guid.Parse(ddlWebTicketBay.SelectedValue))
	End Sub

	Protected Sub btnSaveBayWebTicketSettings_Click(sender As Object, e As EventArgs) Handles btnSaveBayWebTicketSettings.Click
		SaveWebTicketBaySettings(Guid.Parse(ddlWebTicketBay.SelectedValue))
	End Sub

	Protected Sub btnDeleteBayWebTicketSettings_Click(sender As Object, e As EventArgs) Handles btnDeleteBayWebTicketSettings.Click
		DeleteWebTicketBaySettings(Guid.Parse(ddlWebTicketBay.SelectedValue))
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

	Protected Sub ShowSummaryCheckedChanged(sender As Object, e As EventArgs) Handles chkShowProductSummaryTotals.CheckedChanged, chkShowBulkProductSummaryTotals.CheckedChanged
		chkShowRequestedQuantities.Enabled = (chkShowProductSummaryTotals.Checked OrElse chkShowBulkProductSummaryTotals.Checked)
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
		Dim densityUnitSettings() As String = DefaultContainerLabel.GetSettingByBayId(connection, ownerId, KaSetting.DefaultContainerLabelSettings.SN_DENSITY_UNIT_PRECISION, KaSetting.DefaultContainerLabelSettings.SD_DENSITY_UNIT_PRECISION).Split(",")
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

	Private Sub SaveWebTicketDensitySettings(connection As OleDbConnection, transaction As OleDbTransaction, webTicketSetting As String)
		Dim value As String = ""
		For Each item As ListItem In lstWebTicketDensityList.Items
			value &= IIf(value.Length > 0, ",", "") & item.Value
		Next
		KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultContainerLabelSettings.SN_DENSITY_UNIT_PRECISION, value, transaction)
	End Sub

	Private Sub PopulateWebTicketCustomFieldsShownSettings(connection As OleDbConnection, ownerId As Guid)
		cblShowCustomFieldsOnLabel.Items.Clear()
		Dim customFieldsShownList As New List(Of String)
		customFieldsShownList.AddRange(DefaultContainerLabel.GetSettingByBayId(connection, ownerId, KaSetting.DefaultContainerLabelSettings.SN_CUSTOM_FIELDS_ON_DELIVERY_TICKET, KaSetting.DefaultContainerLabelSettings.SD_CUSTOM_FIELDS_ON_DELIVERY_TICKET).Split(","))
		Dim validTicketTables As String = Q(KaTicket.TABLE_NAME) & "," &
			Q(KaTicketCustomerAccount.TABLE_NAME) & "," &
			Q(KaTicketItem.TABLE_NAME) & "," &
			Q(KaTicketBulkItem.TABLE_NAME) & "," &
			Q(KaTicketTransport.TABLE_NAME) & "," &
			Q(KaTicketTransportCompartment.TABLE_NAME)
		For Each customTicketField As KaCustomField In KaCustomField.GetAll(connection, String.Format("{0} = {1} AND {2} IN ({3})", KaCustomField.FN_DELETED, Q(False), KaCustomField.FN_TABLE_NAME, validTicketTables), KaCustomField.FN_FIELD_NAME)
			cblShowCustomFieldsOnLabel.Items.Add(New ListItem(Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(customTicketField.TableName.Replace(KaLocation.TABLE_NAME, "Facilities").Replace("_", " ") & ": " & customTicketField.FieldName), customTicketField.Id.ToString()))
			cblShowCustomFieldsOnLabel.Items(cblShowCustomFieldsOnLabel.Items.Count - 1).Selected = (customFieldsShownList.Contains(customTicketField.Id.ToString()))
		Next
		pnlDeliveryTicketCustomFieldsAssigned.Visible = (cblShowCustomFieldsOnLabel.Items.Count > 0)
	End Sub

	Private Sub SaveWebTicketShowCustomFieldsSetting(connection As OleDbConnection, transaction As OleDbTransaction, ByVal webTicketSetting As String)
		Dim customFieldsShown As String = ""
		For Each customTicketField As ListItem In cblShowCustomFieldsOnLabel.Items
			If customTicketField.Selected Then
				If customFieldsShown.Length > 0 Then customFieldsShown &= ","
				customFieldsShown &= customTicketField.Value
			End If
		Next
		KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultContainerLabelSettings.SN_CUSTOM_FIELDS_ON_DELIVERY_TICKET, customFieldsShown, transaction)
	End Sub

	Private Sub PopulateCustomPreAndPostLoadQuestions(connection As OleDbConnection, ownerId As Guid)
		cblShowCustomPreLoadQuestions.Items.Clear()
		cblShowCustomPostLoadQuestions.Items.Clear()
		Dim customPreLoadQuestionsShowList As New List(Of String)
		customPreLoadQuestionsShowList.AddRange(DefaultContainerLabel.GetSettingByBayId(connection, ownerId, KaSetting.DefaultContainerLabelSettings.SN_CUSTOM_PRE_LOAD_QUESTIONS, KaSetting.DefaultContainerLabelSettings.SD_CUSTOM_PRE_LOAD_QUESTIONS).Split(","))
		Dim customPostLoadQuestionsShowList As New List(Of String)
		customPostLoadQuestionsShowList.AddRange(DefaultContainerLabel.GetSettingByBayId(connection, ownerId, KaSetting.DefaultContainerLabelSettings.SN_CUSTOM_POST_LOAD_QUESTIONS, KaSetting.DefaultContainerLabelSettings.SD_CUSTOM_POST_LOAD_QUESTIONS).Split(","))

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

	Private Sub SaveCustomPreAndPostLoadQuestions(connection As OleDbConnection, transaction As OleDbTransaction, ByVal webTicketSetting As String)
		Dim customPreLoadQuestionsShown As String = ""
		For Each customPreLoadQuestion As ListItem In cblShowCustomPreLoadQuestions.Items
			If customPreLoadQuestion.Selected Then
				If customPreLoadQuestionsShown.Length > 0 Then customPreLoadQuestionsShown &= ","
				customPreLoadQuestionsShown &= customPreLoadQuestion.Value
			End If
		Next
		KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultContainerLabelSettings.SN_CUSTOM_PRE_LOAD_QUESTIONS, customPreLoadQuestionsShown, transaction)

		Dim customPostLoadQuestionsShown As String = ""
		For Each customPostLoadQuestion As ListItem In cblShowCustomPostLoadQuestions.Items
			If customPostLoadQuestion.Selected Then
				If customPostLoadQuestionsShown.Length > 0 Then customPostLoadQuestionsShown &= ","
				customPostLoadQuestionsShown &= customPostLoadQuestion.Value
			End If
		Next
		KaSetting.WriteSetting(connection, webTicketSetting & KaSetting.DefaultContainerLabelSettings.SN_CUSTOM_POST_LOAD_QUESTIONS, customPostLoadQuestionsShown, transaction)
	End Sub

	Protected Sub ddlProductGroup_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles ddlProductGroup.SelectedIndexChanged
		PopulateAdditionalUnitsToDisplayForWebticketProductGroups(Guid.Parse(ddlProductGroup.SelectedValue))
	End Sub

	Private Sub cbxShowAllCustomFieldsOnLabel_CheckedChanged(sender As Object, e As System.EventArgs) Handles cbxShowAllCustomFieldsOnLabel.CheckedChanged
		cblShowCustomFieldsOnLabel.Enabled = Not cbxShowAllCustomFieldsOnLabel.Checked
	End Sub

	Private Sub FertilizerGradeCheckedChanged(sender As Object, e As System.EventArgs) Handles chkShowFertilizerGrade.CheckedChanged
		pnlAnalysisDecimalCount.Visible = chkShowFertilizerGrade.Checked Or chkShowFertilizerGuaranteedAnalysis.Checked
	End Sub

	Private Sub FertilizerGuaranteedAnalysisCheckedChanged(sender As Object, e As System.EventArgs) Handles chkShowFertilizerGuaranteedAnalysis.CheckedChanged
		pnlAnalysisDecimalCount.Visible = chkShowFertilizerGuaranteedAnalysis.Checked Or chkShowFertilizerGrade.Checked
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

	Private Sub chkCustomer_CheckedChanged(sender As Object, e As EventArgs) Handles chkCustomer.CheckedChanged
		chkShowCustomerNotesOnTicket.Enabled = chkCustomer.Checked
	End Sub

	Private Sub chkCustomerLocation_CheckedChanged(sender As Object, e As EventArgs) Handles chkCustomerLocation.CheckedChanged
		chkShowCustomerDestinationNotesOnTicket.Enabled = chkCustomerLocation.Checked
	End Sub
End Class