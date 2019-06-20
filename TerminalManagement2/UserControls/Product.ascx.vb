Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports KahlerAutomation.KaTm2Database

Partial Public Class Product
    Inherits System.Web.UI.UserControl

    Public Event [Next] As Action
    Public Event Back As Action

    <Serializable>
    Enum States
        Required
        AddPrompt
        Additional
        AppRatesPrompt
        AppRates
        ProductGroupPrompt
        ProductGroup
        Custom
    End Enum

    Enum Triggers
        [Next]
        Back
        Yes
        No
    End Enum

    Public ReadOnly Property Product As KaProduct
        Get
            Return New KaProduct() With {
                .Id = Guid.Parse(ViewState("ProductId")),
                .Name = tbxName.Text,
                .OwnerId = ddlOwners.SelectedOwnerId,
                .DefaultUnitId = Guid.Parse(ddlUnit.SelectedValue),
                .Notes = tbxNotes.Text,
                .EpaNumber = tbxEpaNumber.Text,
                .MsdsNumber = tbxMsdsNumber.Text,
                .Manufacturer = tbxManufacturer.Text,
                .ActiveIngredients = tbxActiveIngredients.Text,
                .Restrictions = tbxRestrictions.Text,
                .MaximumApplicationRate = tbxMaxAppRate.Text,
                .MinimumApplicationRate = tbxMinAppRate.Text,
                .DoNotStack = chkDoNotStack.Checked,
                .ProductGroupId = Guid.Parse(ddlProductGroup.SelectedValue),
                .HazardousMaterial = chkHazardousMaterial.Checked}
        End Get
    End Property

    Private Property CurrentState As States
        Get
            Return CType(ViewState("State"), States)
        End Get
        Set(ByVal value As States)
            ViewState("State") = value
        End Set
    End Property

    Private _stateMachine As Stateless.StateMachine(Of States, Triggers)

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs)
        If Not Page.IsPostBack Then
            CurrentState = States.Required
            SetTextboxMaxLengths()
        End If

        _stateMachine = New Stateless.StateMachine(Of States, Triggers)(Function() CurrentState, Sub(s) CurrentState = s)

        _stateMachine.Configure(States.Required) _
                     .PermitDynamic(Triggers.Next, Function() ToAdditionalFields()) _
                     .InternalTransition(Triggers.Back, Sub() RaiseEvent Back()) _
                     .OnEntry(Sub() pnlRequiredFields.Visible = True) _
                     .OnExit(Sub() pnlRequiredFields.Visible = False)

        _stateMachine.Configure(States.AddPrompt) _
                     .Permit(Triggers.Back, States.Required) _
                     .Permit(Triggers.No, States.AppRatesPrompt) _
                     .Permit(Triggers.Yes, States.Additional) _
                     .OnEntry(Sub()
                                  pnlPromptAdditionalFields.Visible = True
                                  pnlBackNext.NextVisible = False
                              End Sub) _
                     .OnExit(Sub()
                                 pnlPromptAdditionalFields.Visible = False
                                 pnlBackNext.NextVisible = True
                             End Sub)

        _stateMachine.Configure(States.Additional) _
                     .Permit(Triggers.Back, States.Required) _
                     .Permit(Triggers.[Next], States.AppRatesPrompt) _
                     .OnEntry(Sub() pnlAdditionalFields.Visible = True) _
                     .OnExit(Sub() pnlAdditionalFields.Visible = False)

        _stateMachine.Configure(States.AppRatesPrompt) _
                     .PermitDynamic(Triggers.Back, Function() ToAdditionalFields()) _
                     .PermitDynamic(Triggers.No, Function() ToProductGroup()) _
                     .Permit(Triggers.Yes, States.AppRates) _
                     .OnEntry(Sub()
                                  pnlPromptAppRates.Visible = True
                                  pnlBackNext.NextVisible = False
                              End Sub) _
                     .OnExit(Sub()
                                 pnlPromptAppRates.Visible = False
                                 pnlBackNext.NextVisible = True
                             End Sub)

        _stateMachine.Configure(States.AppRates) _
                     .Permit(Triggers.Back, States.AppRatesPrompt) _
                     .Permit(Triggers.[Next], States.ProductGroupPrompt) _
                     .OnEntry(Sub() pnlApplicationRates.Visible = True) _
                     .OnExit(Sub() pnlApplicationRates.Visible = False)

        _stateMachine.Configure(States.ProductGroupPrompt) _
                     .PermitDynamic(Triggers.Back, Function() ToApplicationRates()) _
                     .PermitIf(Triggers.No, States.Custom, Function() CustomFieldsDefined()) _
                     .InternalTransitionIf(Triggers.No, Function(s) Not CustomFieldsDefined(), Sub() RaiseEvent [Next]()) _
                     .Permit(Triggers.Yes, States.ProductGroup) _
                     .OnEntry(Sub()
                                  pnlPromptProductGroup.Visible = True
                                  pnlBackNext.NextVisible = False
                              End Sub) _
                     .OnExit(Sub()
                                 pnlPromptProductGroup.Visible = False
                                 pnlBackNext.NextVisible = True
                             End Sub)

        _stateMachine.Configure(States.ProductGroup) _
                     .PermitDynamic(Triggers.Back, Function() ToApplicationRates()) _
                     .PermitIf(Triggers.Next, States.Custom, Function() CustomFieldsDefined()) _
                     .InternalTransitionIf(Triggers.Next, Function(s) Not CustomFieldsDefined(), Sub() RaiseEvent [Next]()) _
                     .OnEntry(Sub() pnlProductGroup.Visible = True) _
                     .OnExit(Sub() pnlProductGroup.Visible = False)

        _stateMachine.Configure(States.Custom) _
                     .PermitDynamic(Triggers.Back, Function() ToProductGroup()) _
                     .InternalTransition(Triggers.Next, Sub() RaiseEvent [Next]()) _
                     .OnEntry(Sub() pnlCustomFields.Visible = True) _
                     .OnExit(Sub() pnlCustomFields.Visible = False)

        AddHandler pnlBackNext.[Next], Sub() _stateMachine.Fire(Triggers.[Next])
        AddHandler pnlBackNext.Back, Sub() _stateMachine.Fire(Triggers.Back)
    End Sub

    Public Sub InitializeOptions(owners As ArrayList, units As ArrayList)
        ddlOwners.SetItems(owners)

        ddlUnit.Items.Add(New ListItem("", Guid.Empty.ToString()))
        For Each u In units
            'If showTime Xor Not KaUnit.IsTime(u.BaseUnit) Then
            ddlUnit.Items.Add(New ListItem(u.Name, u.Id.ToString()))
            'End If
        Next
    End Sub

    Public Sub SetProduct(product As KaProduct, showTime As Boolean)

        With product
            ViewState("ProductId") = .Id.ToString()
            tbxName.Text = .Name
            ddlOwners.SelectedValue = .OwnerId.ToString()

            Try
                ddlUnit.SelectedValue = .DefaultUnitId.ToString()
            Catch ex As ArgumentOutOfRangeException
                'Try
                '    'Dim u As New KaUnit(connection, Guid.Parse(ddlUnit.Items(1).Value))
                '    If showTime AndAlso KaUnit.IsTime(u.BaseUnit) Then
                '        ddlUnit.SelectedIndex = 1
                '    Else
                '        Throw
                '    End If
                'Catch ex2 As Exception
                '    ddlUnit.SelectedIndex = 0
                '    ScriptManager.RegisterClientScriptBlock(Me, Me.GetType(), "InvalidUnitId", Utilities.JsAlert("Record not found in units with ID = " & .DefaultUnitId.ToString() & ". Unit set to blank instead."), False)
                'End Try
            End Try
            tbxNotes.Text = .Notes
            tbxEpaNumber.Text = .EpaNumber
            tbxMsdsNumber.Text = .MsdsNumber
            tbxManufacturer.Text = .Manufacturer
            tbxActiveIngredients.Text = .ActiveIngredients
            tbxRestrictions.Text = .Restrictions
            tbxMaxAppRate.Text = .MaximumApplicationRate
            tbxMinAppRate.Text = .MinimumApplicationRate
            chkDoNotStack.Checked = .DoNotStack
            Try
                ddlProductGroup.SelectedValue = .ProductGroupId.ToString
            Catch ex As Exception
                'ddlOwner.SelectedIndex = 0
                ScriptManager.RegisterClientScriptBlock(Me, Me.GetType(), "InvalidGroupId", Utilities.JsAlert("Record not found in product groups with ID = " & .ProductGroupId.ToString() & ". Product Group set to ""none"" instead."), False)
            End Try
            chkHazardousMaterial.Checked = .HazardousMaterial
        End With
    End Sub

    Private Function ToAdditionalFields() As States
        If HasData(tbxEpaNumber) OrElse HasData(tbxMsdsNumber) OrElse HasData(tbxRestrictions) OrElse HasData(tbxActiveIngredients) OrElse HasData(tbxManufacturer) Then
            Return States.Additional
        End If

        Return States.AddPrompt
    End Function

    Private Function ToApplicationRates() As States
        If (HasDataAndNotZero(tbxMinAppRate) OrElse HasDataAndNotZero(tbxMaxAppRate)) Then
            Return States.AppRates
        End If

        Return States.AppRatesPrompt
    End Function

    Private Function ToProductGroup() As States
        Return If(ddlProductGroup.SelectedValue.Equals(Guid.Empty.ToString()), States.ProductGroupPrompt, States.ProductGroup)
    End Function

    Private Function HasData(textbox As TextBox) As Boolean
        Return textbox.Text.Trim().Length <> 0
    End Function

    Private Function HasDataAndNotZero(textbox As TextBox)
        Return HasData(textbox) And textbox.Text <> "0"
    End Function

    Private Function CustomFieldsDefined() As Boolean
        Return False
    End Function

    Private Sub SetTextboxMaxLengths()
        tbxActiveIngredients.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaProduct.TABLE_NAME, "active_ingredients"))
        tbxEpaNumber.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaProduct.TABLE_NAME, "epa_number"))
        tbxManufacturer.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaProduct.TABLE_NAME, "manufacturer"))
        tbxMaxAppRate.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaProduct.TABLE_NAME, "maximum_application_rate"))
        tbxMinAppRate.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaProduct.TABLE_NAME, "minimum_application_rate"))
        tbxMsdsNumber.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaProduct.TABLE_NAME, "msds_number"))
        tbxName.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaProduct.TABLE_NAME, "name"))
        tbxNotes.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaProduct.TABLE_NAME, "notes"))
        tbxRestrictions.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaProduct.TABLE_NAME, "restrictions"))
    End Sub

    Protected Sub Yes_Click(ByVal sender As Object, ByVal e As EventArgs)
        _stateMachine.Fire(Triggers.Yes)
    End Sub

    Protected Sub No_Click(ByVal sender As Object, ByVal e As EventArgs)
        _stateMachine.Fire(Triggers.No)
    End Sub
End Class
