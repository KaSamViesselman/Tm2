Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class TicketSettings : Inherits System.Web.UI.Page

    Private _currentUser As KaUser
    Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
    Private _currentTableName As String = KaSetting.TABLE_NAME

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Response.Cache.SetCacheability(HttpCacheability.NoCache)
        _currentUser = Utilities.GetUser(Me)
        _currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "GeneralSettings")
        If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")

        If Not Page.IsPostBack Then
            DisplaySettings()
        End If
        btnSave.Enabled = _currentUserPermission(_currentTableName).Edit
        btnSaveNextTicketNumber.Enabled = btnSave.Enabled
    End Sub

    Private Sub DisplaySettings()
        Dim c As OleDbConnection = GetUserConnection(_currentUser.Id)
        cbxSeparateTicketNumberPerOwner.Checked = Boolean.Parse(KaSetting.GetSetting(c, KaSetting.SN_SEPARATE_TICKET_NUMBER_PER_OWNER, KaSetting.SD_SEPARATE_TICKET_NUMBER_PER_OWNER))
        tbxNextTicketNumber.Enabled = Not cbxSeparateTicketNumberPerOwner.Checked
        btnSaveNextTicketNumber.Enabled = Not cbxSeparateTicketNumberPerOwner.Checked
        tbxNextTicketNumber.Text = KaSetting.GetSetting(c, KaSetting.SN_NEXT_TICKET_NUMBER, KaSetting.SD_NEXT_TICKET_NUMBER)
        cbxSeparateTicketPrefixPerOwner.Checked = Boolean.Parse(KaSetting.GetSetting(c, KaSetting.SN_SEPARATE_TICKET_PREFIX_PER_OWNER, KaSetting.SD_SEPARATE_TICKET_PREFIX_PER_OWNER))
        tbxTicketPrefix.Enabled = Not cbxSeparateTicketPrefixPerOwner.Checked
        tbxTicketPrefix.Text = KaSetting.GetSetting(c, KaSetting.SN_TICKET_PREFIX, KaSetting.SD_TICKET_PREFIX)
        cbxSeparateTicketSuffixPerOwner.Checked = Boolean.Parse(KaSetting.GetSetting(c, KaSetting.SN_SEPARATE_TICKET_SUFFIX_PER_OWNER, KaSetting.SD_SEPARATE_TICKET_SUFFIX_PER_OWNER))
        tbxTicketSuffix.Enabled = Not cbxSeparateTicketSuffixPerOwner.Checked
        tbxTicketSuffix.Text = KaSetting.GetSetting(c, KaSetting.SN_TICKET_SUFFIX, KaSetting.SD_TICKET_SUFFIX)
        chkEmailTicketAccount.Checked = Boolean.Parse(KaSetting.GetSetting(c, KaSetting.SN_EMAIL_TICKET_ACCOUNT, KaSetting.SD_EMAIL_TICKET_ACCOUNT))
        chkEmailTicketAccountLocation.Checked = Boolean.Parse(KaSetting.GetSetting(c, KaSetting.SN_EMAIL_TICKET_ACCOUNT_LOCATION, KaSetting.SD_EMAIL_TICKET_ACCOUNT_LOCATION))
        chkEmailTicketApplicator.Checked = Boolean.Parse(KaSetting.GetSetting(c, KaSetting.SN_EMAIL_TICKET_APPLICATOR, KaSetting.SD_EMAIL_TICKET_APPLICATOR))
        chkEmailTicketBranch.Checked = Boolean.Parse(KaSetting.GetSetting(c, KaSetting.SN_EMAIL_TICKET_BRANCH, KaSetting.SD_EMAIL_TICKET_BRANCH))
        chkEmailTicketCarrier.Checked = Boolean.Parse(KaSetting.GetSetting(c, KaSetting.SN_EMAIL_TICKET_CARRIER, KaSetting.SD_EMAIL_TICKET_CARRIER))
        chkEmailTicketDriver.Checked = Boolean.Parse(KaSetting.GetSetting(c, KaSetting.SN_EMAIL_TICKET_DRIVER, KaSetting.SD_EMAIL_TICKET_DRIVER))
        chkEmailTicketLocation.Checked = Boolean.Parse(KaSetting.GetSetting(c, KaSetting.SN_EMAIL_TICKET_LOCATION, KaSetting.SD_EMAIL_TICKET_LOCATION))
        chkEmailTicketOwner.Checked = Boolean.Parse(KaSetting.GetSetting(c, KaSetting.SN_EMAIL_TICKET_OWNER, KaSetting.SD_EMAIL_TICKET_OWNER))
    End Sub

    Private Sub btnSave_Click(sender As Object, e As System.EventArgs) Handles btnSave.Click
        ' validate settings
        If tbxNextTicketNumber.Text.Trim().Length = 0 AndAlso Not cbxSeparateTicketNumberPerOwner.Checked Then
            ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidNextticketNumber", Utilities.JsAlert("Please specify the next ticket number."))
            Exit Sub
        End If
        Dim c As OleDbConnection = GetUserConnection(_currentUser.Id)
        KaSetting.WriteSetting(c, KaSetting.SN_SEPARATE_TICKET_NUMBER_PER_OWNER, cbxSeparateTicketNumberPerOwner.Checked)
        KaSetting.WriteSetting(c, KaSetting.SN_SEPARATE_TICKET_PREFIX_PER_OWNER, cbxSeparateTicketPrefixPerOwner.Checked)
        KaSetting.WriteSetting(c, KaSetting.SN_TICKET_PREFIX, tbxTicketPrefix.Text)
        KaSetting.WriteSetting(c, KaSetting.SN_SEPARATE_TICKET_SUFFIX_PER_OWNER, cbxSeparateTicketSuffixPerOwner.Checked)
        KaSetting.WriteSetting(c, KaSetting.SN_TICKET_SUFFIX, tbxTicketSuffix.Text)
        KaSetting.WriteSetting(c, KaSetting.SN_EMAIL_TICKET_OWNER, chkEmailTicketOwner.Checked)
        KaSetting.WriteSetting(c, KaSetting.SN_EMAIL_TICKET_APPLICATOR, chkEmailTicketApplicator.Checked)
        KaSetting.WriteSetting(c, KaSetting.SN_EMAIL_TICKET_BRANCH, chkEmailTicketBranch.Checked)
        KaSetting.WriteSetting(c, KaSetting.SN_EMAIL_TICKET_ACCOUNT, chkEmailTicketAccount.Checked)
        KaSetting.WriteSetting(c, KaSetting.SN_EMAIL_TICKET_CARRIER, chkEmailTicketCarrier.Checked)
        KaSetting.WriteSetting(c, KaSetting.SN_EMAIL_TICKET_DRIVER, chkEmailTicketDriver.Checked)
        KaSetting.WriteSetting(c, KaSetting.SN_EMAIL_TICKET_LOCATION, chkEmailTicketLocation.Checked)
        KaSetting.WriteSetting(c, KaSetting.SN_EMAIL_TICKET_ACCOUNT_LOCATION, chkEmailTicketAccountLocation.Checked)

        lblSave.Visible = True
    End Sub

    Protected Sub cbxSeparateTicketNumberPerOwner_CheckedChanged(sender As Object, e As EventArgs) Handles cbxSeparateTicketNumberPerOwner.CheckedChanged
        tbxNextTicketNumber.Enabled = Not cbxSeparateTicketNumberPerOwner.Checked
        btnSaveNextTicketNumber.Enabled = Not cbxSeparateTicketNumberPerOwner.Checked
    End Sub

    Private Sub cbxSeparateTicketPrefixPerOwner_CheckedChanged(sender As Object, e As System.EventArgs) Handles cbxSeparateTicketPrefixPerOwner.CheckedChanged
        tbxTicketPrefix.Enabled = Not cbxSeparateTicketPrefixPerOwner.Checked
    End Sub

    Private Sub cbxSeparateTicketSuffixPerOwner_CheckedChanged(sender As Object, e As System.EventArgs) Handles cbxSeparateTicketSuffixPerOwner.CheckedChanged
        tbxTicketSuffix.Enabled = Not cbxSeparateTicketSuffixPerOwner.Checked
    End Sub

    Protected Sub btnSaveNextTicketNumber_Click(sender As Object, e As EventArgs) Handles btnSaveNextTicketNumber.Click
        KaSetting.WriteSetting(GetUserConnection(_currentUser.Id), KaSetting.SN_NEXT_TICKET_NUMBER, tbxNextTicketNumber.Text)
        lblSaveNextTicketNumber.Visible = True
    End Sub
End Class