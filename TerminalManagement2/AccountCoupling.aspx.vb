Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class AccountCoupling
    Inherits System.Web.UI.Page


    ' declare control arrays
    Private _lblAccount() As Label
    Private _ddlAccts() As DropDownList
    Private _btnAdd() As Button
    Private _btnRemove() As Button
    Private _lblPercent() As Label
    Private _tbxPercent() As TextBox
    Private _btnPercent() As Button
    Private _currentUser As KaUser
    Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
    Private _currentTableName As String = KaCustomerAccount.TABLE_NAME

    Structure AccountCombination
        Public CustomerAccountId As Guid
        Public CombinationId As Guid
        Public Percentage As Double
        Public AccountName As String
        Public AccountNumber As String
        Public AssociationExists As Boolean
    End Structure

#Region "Events"
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        ' set up the controls into an array that can be looped through
        _ddlAccts = {ddlAccounts1, ddlAccounts2, ddlAccounts3, ddlAccounts4, ddlAccounts5, ddlAccounts6, ddlAccounts7, ddlAccounts8, ddlAccounts9, ddlAccounts10}
        _lblAccount = {lblAccount1, lblAccount2, lblAccount3, lblAccount4, lblAccount5, lblAccount6, lblAccount7, lblAccount8, lblAccount9, lblAccount10}
        _btnAdd = {btnAdd1, btnAdd2, btnAdd3, btnAdd4, btnAdd5, btnAdd6, btnAdd7, btnAdd8, btnAdd9, btnAdd10}
        _btnRemove = {btnRemove1, btnRemove2, btnRemove3, btnRemove4, btnRemove5, btnRemove6, btnRemove7, btnRemove8, btnRemove9, btnRemove10}
        _lblPercent = {lblPercent1, lblPercent2, lblPercent3, lblPercent4, lblPercent5, lblPercent6, lblPercent7, lblPercent8, lblPercent9, lblPercent10}
        _tbxPercent = {tbxPercent1, tbxPercent2, tbxPercent3, tbxPercent4, tbxPercent5, tbxPercent6, tbxPercent7, tbxPercent8, tbxPercent9, tbxPercent10}
        _btnPercent = {btnPercent1, btnPercent2, btnPercent3, btnPercent4, btnPercent5, btnPercent6, btnPercent7, btnPercent8, btnPercent9, btnPercent10}
        Response.Cache.SetCacheability(HttpCacheability.NoCache)
        _currentUser = Utilities.GetUser(Me)
        _currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Accounts")
        If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")

        If Not Page.IsPostBack Then
            For Each btnRemove As Button In _btnRemove ' set up delete confirmation boxes
                Utilities.ConfirmBox(btnRemove, "Are you sure you want to remove this account?")
            Next
            PopulateAccountList()
        End If
        pnlMain.Enabled = _currentUserPermission(_currentTableName).Edit
    End Sub

    Private Sub ddlCustomers_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ddlAccounts.SelectedIndexChanged
        RefreshDisplay()
    End Sub

    Private Sub btnCreateAssociation_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCreateAssociation.Click
        With New KaCustomerAccountCombination()
            .CustomerAccountId = Guid.Parse(ddlAccounts.SelectedValue)
            .CombinationId = Guid.NewGuid
            .Percentage = 100
            .SqlInsert(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
        End With
        RefreshDisplay()
    End Sub

    Private Sub btnRemove_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRemove1.Click, btnRemove2.Click, btnRemove3.Click, btnRemove4.Click, btnRemove5.Click, btnRemove6.Click, btnRemove7.Click, btnRemove8.Click, btnRemove9.Click, btnRemove10.Click
        Dim index As Integer = Integer.Parse(sender.ID.Substring(9, sender.ID.Length - 9)) - 1
        Dim accountId As Guid = Guid.Parse(_ddlAccts(index).SelectedValue)
        RemoveAssociatedAccount(accountId, index)
    End Sub

    Private Sub btnPercent_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnPercent1.Click, btnPercent2.Click, btnPercent3.Click, btnPercent4.Click, btnPercent5.Click, btnPercent6.Click, btnPercent7.Click, btnPercent8.Click, btnPercent9.Click, btnPercent10.Click
        Dim index As Integer = Integer.Parse(sender.ID.Substring(10, sender.ID.Length - 10)) - 1
        Try ' to update the percentage...
            UpdateSplitPercentage(Guid.Parse(_ddlAccts(index).SelectedValue), GetPercentage(index))
        Catch ex As Exception ' there was a problem updating the percentage...
            ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidPercent", Utilities.JsAlert(ex.Message))
        End Try
    End Sub

    Private Sub btnAdd_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAdd1.Click, btnAdd2.Click, btnAdd3.Click, btnAdd4.Click, btnAdd5.Click, btnAdd6.Click, btnAdd7.Click, btnAdd8.Click, btnAdd9.Click, btnAdd10.Click
        Dim index As Integer = Integer.Parse(sender.ID.Substring(6, sender.ID.Length - 6)) - 1
        Dim accountId As Guid = Guid.Parse(_ddlAccts(index).SelectedValue)
        AddAssociation(accountId, index)
    End Sub
#End Region

    Private Function GetPercentage(index As Integer) As Double
        If _tbxPercent(index).Text.Trim().Length > 0 Then
            Try ' to parse the value that the user entered for the percentage...
                Return Math.Max(Math.Min(Double.Parse(_tbxPercent(index).Text), 100), 0)
            Catch ex As FormatException ' unable to parse the percentage...
                Throw New Exception("Please enter a numeric value for the customer account percentage.")
            End Try
        Else ' a blank equals a zero
            Return 0
        End If
    End Function

    Private Sub ClearAllControls()
        btnCreateAssociation.Visible = False ' hide controls on new load
        For index As Integer = 0 To _ddlAccts.Length - 1 ' clear all controls that hold values and then hide them
            _tbxPercent(index).Text = "0"
            _lblAccount(index).Visible = False
            _ddlAccts(index).Visible = False
            _btnAdd(index).Visible = False
            _btnRemove(index).Visible = False
            _lblPercent(index).Visible = False
            _tbxPercent(index).Visible = False
            _btnPercent(index).Visible = False
        Next
    End Sub

    Private Function GetAssociatedAccounts(ByVal combinationId As Guid) As ArrayList
        Dim accountArray As New ArrayList
        Dim accountCombination As New AccountCombination
        accountCombination.AssociationExists = False
        Dim customer As KaCustomerAccount = New KaCustomerAccount()
        Try
            Dim list As ArrayList = KaCustomerAccountCombination.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 and combination_id=" & Q(combinationId), "percentage DESC")
            For Each combo As KaCustomerAccountCombination In list
                accountCombination.CombinationId = combinationId
                accountCombination.AssociationExists = True
                accountCombination.CustomerAccountId = combo.CustomerAccountId
                accountCombination.Percentage = combo.Percentage
                customer = New KaCustomerAccount(GetUserConnection(_currentUser.Id), combo.CustomerAccountId)
                accountCombination.AccountNumber = customer.AccountNumber
                accountCombination.AccountName = customer.Name
                accountArray.Add(accountCombination)
            Next
        Catch ex As Exception
            'No coupling found for this combinationId
        End Try
        Return accountArray
    End Function

    Private Function GetSelectedAccountInfo() As AccountCombination
        Dim c As OleDbConnection = GetUserConnection(_currentUser.Id)
        Dim accountCombination As New AccountCombination
        Dim customer As KaCustomerAccount = New KaCustomerAccount()
        accountCombination.AssociationExists = False
        accountCombination.CustomerAccountId = Guid.Parse(ddlAccounts.SelectedValue)
        If accountCombination.CustomerAccountId <> Guid.Empty Then
            Dim conditions As String = "deleted=0 AND customer_account_id=" & Q(Guid.Parse(ddlAccounts.SelectedValue))
            For Each combo As KaCustomerAccountCombination In KaCustomerAccountCombination.GetAll(c, conditions, "")
                accountCombination.CombinationId = combo.CombinationId
                accountCombination.AssociationExists = True
                accountCombination.Percentage = combo.Percentage
                customer = New KaCustomerAccount(c, accountCombination.CustomerAccountId)
                accountCombination.AccountNumber = customer.AccountNumber
                accountCombination.AccountName = customer.Name
            Next
        End If
        Return accountCombination
    End Function

    Private Sub PopulateAccountList()
        Dim c As OleDbConnection = GetUserConnection(_currentUser.Id)
        ddlAccounts.Items.Clear()
        ddlAccounts.Items.Add(New ListItem("Select an account", Guid.Empty.ToString()))
        For Each account As KaCustomerAccount In KaCustomerAccount.GetAll(c, "deleted=0 AND is_supplier=0" & IIf(_currentUser.OwnerId <> Guid.Empty, String.Format(" AND (owner_id={0} OR owner_id={1})", Q(Guid.Empty), Q(_currentUser.OwnerId)), ""), "name ASC")
            ddlAccounts.Items.Add(New ListItem(account.Name, account.Id.ToString()))
        Next
    End Sub

    Private Sub PopulateAccountList(index As Integer)
        Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
        _ddlAccts(index).Items.Clear()
        Dim conditions As String = "deleted=0 AND is_supplier=0" & IIf(_currentUser.OwnerId <> Guid.Empty, String.Format(" AND (owner_id={0} OR owner_id={1})", Q(Guid.Empty), Q(_currentUser.OwnerId)), "") ' put together a condition string to use when populating the customer account list
        For Each n As KaCustomerAccountCombination In KaCustomerAccountCombination.GetAll(connection, "deleted=0", "id ASC") ' exclude customer accounts already a part of a combination
            conditions &= String.Format(" AND id<>{0}", Q(n.CustomerAccountId))
        Next
        For Each n As KaCustomerAccount In KaCustomerAccount.GetAll(connection, conditions, "name ASC")
            _ddlAccts(index).Items.Add(New ListItem(n.Name, n.Id.ToString()))
        Next
    End Sub

    Private Sub RefreshDisplay()
        ClearAllControls()
        Dim html As String = "<table border=1 cellspacing=0 cellpadding=0 inset=2>" ' build the table for display
        Dim accountCombination As AccountCombination = GetSelectedAccountInfo() ' fill structure if there is an association
        If accountCombination.CustomerAccountId <> Guid.Empty Then
            If accountCombination.AssociationExists Then ' find all other associations and display
                html &= "<tr><td><strong>Account Number</strong></td><td><strong>Account Name</strong></td><td><strong>Percentage</strong></td></tr>"
                Dim index As Integer = 0
                Dim totalPercent As Double = 0
                For Each ac As AccountCombination In GetAssociatedAccounts(accountCombination.CombinationId)
                    html &= "<tr><td width=130px>" & ac.AccountNumber & "</td><td width=220px>" & ac.AccountName & "</td><td width=80px>" & String.Format("{0:0.0}%", ac.Percentage) & "</td></tr>"
                    ' display the controls for each entry found
                    _lblAccount(index).Visible = True
                    _lblAccount(index).Text = String.Format("{0} ({1})", ac.AccountName, ac.AccountNumber)
                    _ddlAccts(index).Items.Clear()
                    _ddlAccts(index).Items.Add(New ListItem("", ac.CustomerAccountId.ToString()))
                    _ddlAccts(index).SelectedValue = ac.CustomerAccountId.ToString()
                    _ddlAccts(index).Visible = False
                    _btnAdd(index).Visible = False
                    _tbxPercent(index).Text = ac.Percentage.ToString()
                    _btnRemove(index).Visible = True
                    _lblPercent(index).Visible = True
                    _tbxPercent(index).Visible = True
                    _btnPercent(index).Visible = True
                    totalPercent += ac.Percentage
                    index += 1
                Next
                ' display one additional line for next customer account to be selected by the user
                If index < _ddlAccts.Length Then
                    PopulateAccountList(index)
                    If _ddlAccts(index).Items.Count > 0 Then
                        _lblAccount(index).Visible = False
                        _ddlAccts(index).Visible = True
                        _btnAdd(index).Visible = True
                        _btnRemove(index).Visible = False
                        _lblPercent(index).Visible = True
                        _tbxPercent(index).Visible = True
                        _btnPercent(index).Visible = False
                    End If
                End If
                html &= String.Format("<tr><td><strong>Total</strong></td><td>&nbsp;</td><td><font color=""{0}"">{1:0.0}%</font></td></tr></table>", IIf(totalPercent <> 100, "#ff0000", "#000000"), totalPercent)
            Else ' no association for this account
                btnCreateAssociation.Visible = True
                html = "This account is not yet associated with any other accounts"
            End If
            litOutput.Text = html
        Else ' account not selected
            litOutput.Text = ""
        End If
    End Sub

    Private Sub AddAssociation(accountId As Guid, index As Integer)
        Dim combination As AccountCombination = GetSelectedAccountInfo()
        Try
            With New KaCustomerAccountCombination()
                .CustomerAccountId = Guid.Parse(_ddlAccts(index).SelectedValue)
                .CombinationId = combination.CombinationId
                .Percentage = GetPercentage(index)
                .SqlInsert(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
            End With
            RefreshDisplay()
        Catch ex As Exception
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidAssociation", Utilities.JsAlert(ex.Message))
		End Try
    End Sub

    Private Sub RemoveAssociatedAccount(accountId As Guid, index As Integer)
        If accountId <> Guid.Empty Then
            Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
            Dim combinationId As Guid = Guid.Empty
            For Each combo As KaCustomerAccountCombination In KaCustomerAccountCombination.GetAll(connection, "deleted=0 AND customer_account_id=" & Q(accountId), "")
                combinationId = combo.CombinationId
                combo.Deleted = True
                combo.SqlUpdate(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
            Next
            If combinationId <> Guid.Empty Then ' if after deleting there is only one account in this combination delete it also
                Dim allCombos As ArrayList = KaCustomerAccountCombination.GetAll(connection, "deleted=0 AND combination_id=" & Q(combinationId), "")
                If allCombos.Count = 1 Then
                    allCombos.Item(0).Deleted = True
                    allCombos.Item(0).SqlUpdate(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                End If
            End If
            _tbxPercent(index).Text = "0"
        End If
        RefreshDisplay()
    End Sub

    Private Sub UpdateSplitPercentage(accountId As Guid, percent As Double)
        Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
        For Each n As KaCustomerAccountCombination In KaCustomerAccountCombination.GetAll(connection, "deleted=0 AND customer_account_id=" & Q(accountId), "")
            n.Percentage = percent
            n.SqlUpdate(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
        Next
        RefreshDisplay()
    End Sub
End Class

