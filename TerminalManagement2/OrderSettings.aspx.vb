Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class OrderSettings : Inherits System.Web.UI.Page

    Private _currentUser As KaUser
    Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
    Private _currentTableName As String = KaSetting.TABLE_NAME

    Public Const SN_EMAIL_POINT_OF_SALE_TICKETS As String = "PointOfSale/EmailCreatedPointOfSaleTickets"
    Public Const SD_EMAIL_POINT_OF_SALE_TICKETS As String = "True"
    Public Const SN_POS_PRELOAD_QUESTIONS_AVAILABLE As String = "PointOfSale/PreLoadQuestionsAvailable"
    Public Const SN_POS_POSTLOAD_QUESTIONS_AVAILABLE As String = "PointOfSale/PostLoadQuestionsAvailable"

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Response.Cache.SetCacheability(HttpCacheability.NoCache)
        _currentUser = Utilities.GetUser(Me)
        _currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "GeneralSettings")
        If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")

        If Not Page.IsPostBack Then
            PopulateCustomersList()
            PopulateOwnersList()
            PopulateBranchesList()
            DisplaySettings()
        End If
        btnSave.Enabled = _currentUserPermission(_currentTableName).Edit
        btnSaveNextOrderNumber.Enabled = btnSave.Enabled
    End Sub

    Private Sub DisplaySettings()
        Dim c As OleDbConnection = GetUserConnection(_currentUser.Id)
        cbxAutoGenerate.Checked = Boolean.Parse(KaSetting.GetSetting(c, KaSetting.SN_AUTO_GENERATE_ORDER_NUMBER, KaSetting.SD_AUTO_GENERATE_ORDER_NUMBER))
        cbxAllowModification.Checked = Boolean.Parse(KaSetting.GetSetting(c, KaSetting.SN_USER_CAN_CHANGE_ORDER_NUMBER, KaSetting.SD_USER_CAN_CHANGE_ORDER_NUMBER))
        cbxSeparateOrderNumberPerOwner.Checked = Boolean.Parse(KaSetting.GetSetting(c, KaSetting.SN_SEPARATE_ORDER_NUMBER_PER_OWNER, KaSetting.SD_SEPARATE_ORDER_NUMBER_PER_OWNER))
        tbxStartingOrderNumber.Enabled = Not cbxSeparateOrderNumberPerOwner.Checked
        btnSaveNextOrderNumber.Enabled = Not cbxSeparateOrderNumberPerOwner.Checked
        tbxStartingOrderNumber.Text = KaSetting.GetSetting(c, KaSetting.SN_NEXT_ORDER_NUMBER, KaSetting.SD_NEXT_ORDER_NUMBER)
        cbxPartialDelete.Checked = KaSetting.GetSetting(c, "General/MarkOrdersAsIncomplete", "False")
        cbxLockOwner.Checked = KaSetting.GetSetting(c, "General/LockOwnerDDL", "False")
        cbxLockBranch.Checked = KaSetting.GetSetting(c, "General/LockBranchesDDL", "False")
        cbxLockRunOverPercent.Checked = KaSetting.GetSetting(c, "General/LockRunOverPercent", "False")
        tbxOrderComparisonPercentTolerance.Text = KaSetting.GetSetting(c, KaSetting.SN_ORDER_COMPARISON_PERCENT_TOLERANCE, KaSetting.SD_ORDER_COMPARISON_PERCENT_TOLERANCE)
        cbxShowReleaseNumberInOrderList.Checked = Boolean.Parse(KaSetting.GetSetting(c, "General/ShowReleaseNumberInOrderList", "False"))
        cbxSendOrderSummaryToApplicator.Checked = Boolean.Parse(KaSetting.GetSetting(c, KaSetting.SN_EMAIL_ORDER_SUMMARY_TO_APPLICATOR, KaSetting.SD_EMAIL_ORDER_SUMMARY_TO_APPLICATOR))
        cbxSendOrderSummaryToBranch.Checked = Boolean.Parse(KaSetting.GetSetting(c, KaSetting.SN_EMAIL_ORDER_SUMMARY_TO_BRANCH, KaSetting.SD_EMAIL_ORDER_SUMMARY_TO_BRANCH))
        cbxSendOrderSummaryToCustomerAccount.Checked = Boolean.Parse(KaSetting.GetSetting(c, KaSetting.SN_EMAIL_ORDER_SUMMARY_TO_CUSTOMER_ACCOUNT, KaSetting.SD_EMAIL_ORDER_SUMMARY_TO_CUSTOMER_ACCOUNT))
        cbxSendOrderSummaryToCustomerAccountLocation.Checked = Boolean.Parse(KaSetting.GetSetting(c, KaSetting.SN_EMAIL_ORDER_SUMMARY_TO_CUSTOMER_ACCOUNT_LOCATION, KaSetting.SD_EMAIL_ORDER_SUMMARY_TO_CUSTOMER_ACCOUNT_LOCATION))
        cbxSendOrderSummaryToOwner.Checked = Boolean.Parse(KaSetting.GetSetting(c, KaSetting.SN_EMAIL_ORDER_SUMMARY_TO_OWNER, KaSetting.SD_EMAIL_ORDER_SUMMARY_TO_OWNER))
        cbxCreateNewDestinationFromOrderShipToInformation.Checked = Boolean.Parse(Settings.GetMySetting(c))

        ddlOwners.SelectedValue = KaSetting.GetSetting(c, "General/Owner", Guid.Empty.ToString)
        ddlBranches.SelectedValue = KaSetting.GetSetting(c, "General/Branch", Guid.Empty.ToString)
        tbxRunOverPercent.Text = KaSetting.GetSetting(c, "General/RunOverPercent", "0")

        Boolean.TryParse(KaSetting.GetSetting(c, "StagedOrder/UseOrderPercentage", False), chkUseOrderPercentage.Checked)
        Boolean.TryParse(KaSetting.GetSetting(c, "StagedOrder/AllowOrdersToBeAssignedToMultipleStagedOrders", True), chkAllowOrdersToBeAssignedToMultipleStagedOrders.Checked)
        Boolean.TryParse(KaSetting.GetSetting(c, "PointOfSale/LimitDriversToDriversAssignedToAccount", False), chkLimitDriversToDriversAssignedToAccount.Checked)
        Boolean.TryParse(KaSetting.GetSetting(c, "PointOfSale/LimitTransportsToCarrierSelected", False), chkLimitTransportsToCarrierSelected.Checked)
        Boolean.TryParse(KaSetting.GetSetting(c, SN_EMAIL_POINT_OF_SALE_TICKETS, SD_EMAIL_POINT_OF_SALE_TICKETS), cbxEmailCreatedPointOfSaleTickets.Checked)
        For n = 0 To cblStagedOrderShortcuts.Items.Count - 1
            If Not Boolean.TryParse(KaSetting.GetSetting(c, "StagedOrder/Show" & cblStagedOrderShortcuts.Items(n).Value, True), cblStagedOrderShortcuts.Items(n).Selected) Then
                cblStagedOrderShortcuts.Items(n).Selected = True
            End If
        Next
        PopulateCustomShortcuts(c)
        PopulateCustomLoadQuestions(c)
    End Sub

    Protected Sub cbxSeparateOrderNumberPerOwner_CheckedChanged(sender As Object, e As EventArgs) Handles cbxSeparateOrderNumberPerOwner.CheckedChanged
        tbxStartingOrderNumber.Enabled = Not cbxSeparateOrderNumberPerOwner.Checked
        btnSaveNextOrderNumber.Enabled = Not cbxSeparateOrderNumberPerOwner.Checked
    End Sub

    Protected Sub btnSaveNextOrderNumber_Click(sender As Object, e As EventArgs) Handles btnSaveNextOrderNumber.Click
        KaSetting.WriteSetting(GetUserConnection(_currentUser.Id), KaSetting.SN_NEXT_ORDER_NUMBER, tbxStartingOrderNumber.Text)
        lblSaveNextOrderNumber.Visible = True
    End Sub

    Private Sub PopulateCustomersList()
        Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
        ddlInternalTransferCustomerAccount.Items.Clear()
        ddlInternalTransferCustomerAccount.Items.Add(New ListItem("", Guid.Empty.ToString()))
        For Each customerAccount As KaCustomerAccount In KaCustomerAccount.GetAll(connection, "deleted=0 AND is_supplier=0", "name ASC")
            ddlInternalTransferCustomerAccount.Items.Add(New ListItem(customerAccount.Name, customerAccount.Id.ToString()))
        Next
        Try ' to select the internal transfer customer account...
            ddlInternalTransferCustomerAccount.SelectedValue = KaSetting.GetSetting(connection, KaSetting.SN_INTERNAL_TRANSFER_CUSTOMER_ACCOUNT_ID, KaSetting.SD_INTERNAL_TRANSFER_CUSTOMER_ACCOUNT_ID)
        Catch ex As ArgumentException ' the previously selected customer account is no longer available...
            ddlInternalTransferCustomerAccount.SelectedValue = Guid.Empty.ToString()
        End Try
    End Sub

    Private Sub PopulateOwnersList()
        ddlOwners.Items.Clear()
        ddlOwners.Items.Add(New ListItem("", Guid.Empty.ToString))
        For Each o As KaOwner In KaOwner.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
            ddlOwners.Items.Add(New ListItem(o.Name, o.Id.ToString))
        Next
    End Sub

    Private Sub PopulateBranchesList()
        ddlBranches.Items.Clear()
        ddlBranches.Items.Add(New ListItem("", Guid.Empty.ToString))
        For Each u As KaBranch In KaBranch.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
            ddlBranches.Items.Add(New ListItem(u.Name, u.Id.ToString))
        Next
    End Sub

    Private Sub PopulateCustomShortcuts(c As OleDbConnection)
        Dim selectedShortcuts As List(Of Guid)
        Try
            selectedShortcuts = Tm2Database.FromXml(KaSetting.GetSetting(c, "StagedOrder/ShortcutsAvailable", ""), GetType(List(Of Guid)))
        Catch ex As Exception
            selectedShortcuts = New List(Of Guid)
        End Try

        cblStagedOrderCustomShortcuts.Items.Clear()
        For Each customShortcut As KaCustomPages In KaCustomPages.GetAll(c, "deleted=0 AND custom_shortcut=1", "page_label ASC")
            cblStagedOrderCustomShortcuts.Items.Add(New ListItem(customShortcut.PageLabel, customShortcut.Id.ToString()))
            cblStagedOrderCustomShortcuts.Items(cblStagedOrderCustomShortcuts.Items.Count - 1).Selected = (selectedShortcuts.Contains(customShortcut.Id))
        Next
        pnlStagedOrderCustomShortcuts.Visible = (cblStagedOrderCustomShortcuts.Items.Count > 0)
    End Sub

    Private Sub PopulateCustomLoadQuestions(c As OleDbConnection)
        Dim preLoadQuestionsAvailable As List(Of Guid)
        Try
            preLoadQuestionsAvailable = Tm2Database.FromXml(KaSetting.GetSetting(c, SN_POS_PRELOAD_QUESTIONS_AVAILABLE, ""), GetType(List(Of Guid)))
        Catch ex As Exception
            preLoadQuestionsAvailable = New List(Of Guid)
        End Try
        Dim postLoadQuestionsAvailable As List(Of Guid)
        Try
            postLoadQuestionsAvailable = Tm2Database.FromXml(KaSetting.GetSetting(c, SN_POS_POSTLOAD_QUESTIONS_AVAILABLE, ""), GetType(List(Of Guid)))
        Catch ex As Exception
            postLoadQuestionsAvailable = New List(Of Guid)
        End Try

        Dim loadQuestionRdr As OleDbDataReader = Tm2Database.ExecuteReader(c, String.Format("SELECT clqf.id, clqf.name, clqf.prompt_text, CASE WHEN b.name IS NULL THEN 'All bays' ELSE b.name END AS bay_name, clqf.post_load, clqf.input_type FROM custom_load_question_fields AS clqf LEFT OUTER JOIN bays AS b ON b.id = clqf.bay_id WHERE (clqf.deleted = 0) AND ((clqf.bay_id = {0}) OR (b.deleted = 0)) ORDER BY 2,3,4", Q(Guid.Empty)))
        cblPoSCustomPreLoadQuestion.Items.Clear()
        cblPoSCustomPreLoadQuestion.Items.Clear()
        Do While loadQuestionRdr.Read()
            If loadQuestionRdr.Item("input_type") <> KaCustomLoadQuestionFields.InputTypes.Url Then
                Dim customLoadItem As New ListItem(String.Format("{0} - {1}: for {2}", loadQuestionRdr.Item("name"), loadQuestionRdr.Item("prompt_text"), loadQuestionRdr.Item("bay_name")), loadQuestionRdr.Item("id").ToString())
                If loadQuestionRdr.Item("post_load") Then
                    cblPoSCustomPostLoadQuestion.Items.Add(customLoadItem)
                    cblPoSCustomPostLoadQuestion.Items(cblPoSCustomPostLoadQuestion.Items.Count - 1).Selected = (postLoadQuestionsAvailable.Contains(loadQuestionRdr.Item("id")))
                Else
                    cblPoSCustomPreLoadQuestion.Items.Add(customLoadItem)
                    cblPoSCustomPreLoadQuestion.Items(cblPoSCustomPreLoadQuestion.Items.Count - 1).Selected = (preLoadQuestionsAvailable.Contains(loadQuestionRdr.Item("id")))
                End If
            End If
        Loop

        pnlPoSCustomPreLoadQuestions.Visible = (cblPoSCustomPreLoadQuestion.Items.Count > 0)
        pnlPoSCustomPostLoadQuestion.Visible = (cblPoSCustomPostLoadQuestion.Items.Count > 0)
        pnlPoSCustomLoadQuestions.Visible = (cblPoSCustomPreLoadQuestion.Items.Count + cblPoSCustomPostLoadQuestion.Items.Count > 0)
    End Sub

    Private Sub btnSave_Click(sender As Object, e As System.EventArgs) Handles btnSave.Click
        ' validate settings
        If Not IsNumeric(tbxOrderComparisonPercentTolerance.Text) Then
            ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidOrderCompPercent", Utilities.JsAlert("Please enter a numeric value for the order comparison percent tolerance setting."))
            Exit Sub
        End If
        If Not IsNumeric(tbxRunOverPercent.Text) Then
            ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidRunOverPercent", Utilities.JsAlert("Please enter a numeric value for the order run over percent setting."))
            Exit Sub
        End If

        Dim c As OleDbConnection = GetUserConnection(_currentUser.Id)
        KaSetting.WriteSetting(c, KaSetting.SN_AUTO_GENERATE_ORDER_NUMBER, cbxAutoGenerate.Checked)
        KaSetting.WriteSetting(c, KaSetting.SN_USER_CAN_CHANGE_ORDER_NUMBER, cbxAllowModification.Checked)
        KaSetting.WriteSetting(c, KaSetting.SN_SEPARATE_ORDER_NUMBER_PER_OWNER, cbxSeparateOrderNumberPerOwner.Checked)
        KaSetting.WriteSetting(c, "General/MarkOrdersAsIncomplete", cbxPartialDelete.Checked)
        KaSetting.WriteSetting(c, "General/LockOwnerDDL", cbxLockOwner.Checked)
        KaSetting.WriteSetting(c, "General/LockBranchesDDL", cbxLockBranch.Checked)
        KaSetting.WriteSetting(c, "General/LockRunOverPercent", cbxLockRunOverPercent.Checked)
        KaSetting.WriteSetting(c, KaSetting.SN_ORDER_COMPARISON_PERCENT_TOLERANCE, tbxOrderComparisonPercentTolerance.Text)
        KaSetting.WriteSetting(c, "General/ShowReleaseNumberInOrderList", cbxShowReleaseNumberInOrderList.Checked)
        KaSetting.WriteSetting(c, KaSetting.SN_INTERNAL_TRANSFER_CUSTOMER_ACCOUNT_ID, ddlInternalTransferCustomerAccount.SelectedValue)
        KaSetting.WriteSetting(c, KaSetting.SN_EMAIL_ORDER_SUMMARY_TO_APPLICATOR, cbxSendOrderSummaryToApplicator.Checked)
        KaSetting.WriteSetting(c, KaSetting.SN_EMAIL_ORDER_SUMMARY_TO_BRANCH, cbxSendOrderSummaryToBranch.Checked)
        KaSetting.WriteSetting(c, KaSetting.SN_EMAIL_ORDER_SUMMARY_TO_CUSTOMER_ACCOUNT, cbxSendOrderSummaryToCustomerAccount.Checked)
        KaSetting.WriteSetting(c, KaSetting.SN_EMAIL_ORDER_SUMMARY_TO_CUSTOMER_ACCOUNT_LOCATION, cbxSendOrderSummaryToCustomerAccountLocation.Checked)
        KaSetting.WriteSetting(c, KaSetting.SN_EMAIL_ORDER_SUMMARY_TO_OWNER, cbxSendOrderSummaryToOwner.Checked)

        KaSetting.WriteSetting(c, "General/Owner", ddlOwners.SelectedValue.ToString)
        KaSetting.WriteSetting(c, "General/Branch", ddlBranches.SelectedValue.ToString)
        KaSetting.WriteSetting(c, "General/RunOverPercent", tbxRunOverPercent.Text)

        KaSetting.WriteSetting(c, "StagedOrder/UseOrderPercentage", chkUseOrderPercentage.Checked)
        KaSetting.WriteSetting(c, "StagedOrder/AllowOrdersToBeAssignedToMultipleStagedOrders", chkAllowOrdersToBeAssignedToMultipleStagedOrders.Checked)
        KaSetting.WriteSetting(c, "PointOfSale/LimitDriversToDriversAssignedToAccount", chkLimitDriversToDriversAssignedToAccount.Checked)
        KaSetting.WriteSetting(c, "PointOfSale/LimitTransportsToCarrierSelected", chkLimitTransportsToCarrierSelected.Checked)
        For n = 0 To cblStagedOrderShortcuts.Items.Count - 1
            KaSetting.WriteSetting(c, "StagedOrder/Show" & cblStagedOrderShortcuts.Items(n).Value, cblStagedOrderShortcuts.Items(n).Selected)
        Next

        'Staged order custom shortcuts
        Dim selectedShortcuts As New List(Of Guid)
        For Each customShortcut As ListItem In cblStagedOrderCustomShortcuts.Items
            If customShortcut.Selected Then selectedShortcuts.Add(Guid.Parse(customShortcut.Value))
        Next
        KaSetting.WriteSetting(c, "StagedOrder/ShortcutsAvailable", Tm2Database.ToXml(selectedShortcuts, GetType(List(Of Guid))))

        ' Pre Load Questions Available
        Dim preLoadQuestionsAvailable As New List(Of Guid)
        For Each preLoadQuestion As ListItem In cblPoSCustomPreLoadQuestion.Items
            If preLoadQuestion.Selected Then preLoadQuestionsAvailable.Add(Guid.Parse(preLoadQuestion.Value))
        Next
        KaSetting.WriteSetting(c, SN_POS_PRELOAD_QUESTIONS_AVAILABLE, Tm2Database.ToXml(preLoadQuestionsAvailable, GetType(List(Of Guid))))

        ' Post Load Questions Available
        Dim postLoadQuestionsAvailable As New List(Of Guid)
        For Each postLoadQuestion As ListItem In cblPoSCustomPostLoadQuestion.Items
            If postLoadQuestion.Selected Then postLoadQuestionsAvailable.Add(Guid.Parse(postLoadQuestion.Value))
        Next
        KaSetting.WriteSetting(c, SN_POS_POSTLOAD_QUESTIONS_AVAILABLE, Tm2Database.ToXml(postLoadQuestionsAvailable, GetType(List(Of Guid))))

        Settings.SetMySetting(c, cbxCreateNewDestinationFromOrderShipToInformation.Checked)
        KaSetting.WriteSetting(c, SN_EMAIL_POINT_OF_SALE_TICKETS, cbxEmailCreatedPointOfSaleTickets.Checked)

        lblSave.Visible = True
    End Sub
End Class