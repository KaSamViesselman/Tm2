Imports System
Imports System.Web.UI

Partial Public Class BulkProductSetup
    Inherits System.Web.UI.UserControl

    <Serializable>
    Friend Enum States
        NewOrExisting
        Existing
        BulkProduct
        [Optional]
        Analysis
        Panel
        Summary
    End Enum

    Enum Triggers
        NewProduct
        ExistingBProduct
        BulkProductSelected
        [Next]
        Back
        Initialize
    End Enum

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
            CurrentState = States.NewOrExisting
        End If

        _stateMachine = SetupStateMachine()



        'AddHandler pnlSelectBulkProduct.NewProduct, Sub() _stateMachine.Fire(Triggers.NewProduct)
        'AddHandler pnlSelectBulkProduct.ExistingProduct, Sub() _stateMachine.Fire(Triggers.ExistingBProduct)
        'AddHandler pnlExistingBulkProduct.Back, Sub() _stateMachine.Fire(Triggers.Back)
        'AddHandler pnlExistingBulkProduct.BulkProductSelected, Sub(id) _stateMachine.Fire(Triggers.ExistingBProduct)
        'AddHandler pnlBulkProduct.Back, Sub() _stateMachine.Fire(Triggers.Back)
        'AddHandler pnlBulkProduct.[Next], Sub() _stateMachine.Fire(Triggers.[Next])
        'AddHandler pnlBulkProductOptional.Back, Sub() _stateMachine.Fire(Triggers.Back)
        'AddHandler pnlBulkProductOptional.[Next], Sub() _stateMachine.Fire(Triggers.[Next])
        'AddHandler pnlBulkProductAnalysis.Back, Sub() _stateMachine.Fire(Triggers.Back)
        'AddHandler pnlBulkProductAnalysis.[Next], Sub() _stateMachine.Fire(Triggers.[Next])
        'AddHandler pnlBulkPanel.Back, Sub() _stateMachine.Fire(Triggers.Back)
        'AddHandler pnlBulkPanel.[Next], Sub() _stateMachine.Fire(Triggers.[Next])
        'AddHandler pnlSummary.Back, Sub() _stateMachine.Fire(Triggers.Back)
        'AddHandler pnlSummary.[Next], Sub() _stateMachine.Fire(Triggers.[Next])
    End Sub

    Public Sub Initialize()
        _stateMachine.Fire(Triggers.Initialize)
    End Sub

    Private Function SetupStateMachine() As Stateless.StateMachine(Of States, Triggers)
        Dim stateMachine = New Stateless.StateMachine(Of States, Triggers)(Function() CurrentState, Function(s) CurrentState = s)

        stateMachine.Configure(States.NewOrExisting) _
                    .Permit(Triggers.NewProduct, States.BulkProduct) _
                    .Permit(Triggers.ExistingBProduct, States.Existing) _
                    .PermitReentry(Triggers.Initialize) _
                    .OnEntry(Sub() pnlSelectBulkProduct.Visible = True) _
                    .OnExit(Sub() pnlSelectBulkProduct.Visible = False)

        stateMachine.Configure(States.Existing) _
                    .Permit(Triggers.Back, States.NewOrExisting) _
                    .Permit(Triggers.BulkProductSelected, States.BulkProduct) _
                    .Permit(Triggers.Initialize, States.NewOrExisting) _
                    .OnEntry(Sub() pnlSelectBulkProduct.Visible = True) _
                    .OnExit(Sub() pnlSelectBulkProduct.Visible = False)

        stateMachine.Configure(States.BulkProduct) _
                    .Permit(Triggers.Back, States.NewOrExisting) _
                    .Permit(Triggers.[Next], States.[Optional]) _
                    .Permit(Triggers.Initialize, States.NewOrExisting) _
                    .OnEntry(Sub() pnlSelectBulkProduct.Visible = True) _
                    .OnExit(Sub() pnlSelectBulkProduct.Visible = False)

        stateMachine.Configure(States.[Optional]) _
                    .Permit(Triggers.Back, States.BulkProduct) _
                    .Permit(Triggers.[Next], States.Analysis) _
                    .Permit(Triggers.Initialize, States.NewOrExisting) _
                    .OnEntry(Sub() pnlSelectBulkProduct.Visible = True) _
                    .OnExit(Sub() pnlSelectBulkProduct.Visible = False)

        stateMachine.Configure(States.Analysis) _
                    .Permit(Triggers.Back, States.[Optional]) _
                    .Permit(Triggers.[Next], States.Panel) _
                    .Permit(Triggers.Initialize, States.NewOrExisting) _
                    .OnEntry(Sub() pnlSelectBulkProduct.Visible = True) _
                    .OnExit(Sub() pnlSelectBulkProduct.Visible = False)

        stateMachine.Configure(States.Panel) _
                    .Permit(Triggers.Back, States.Analysis) _
                    .Permit(Triggers.[Next], States.Summary) _
                    .Permit(Triggers.Initialize, States.NewOrExisting) _
                    .OnEntry(Sub() pnlSelectBulkProduct.Visible = True) _
                    .OnExit(Sub() pnlSelectBulkProduct.Visible = False)

        stateMachine.Configure(States.Summary) _
                    .Permit(Triggers.Back, States.Panel) _
                    .Permit(Triggers.Initialize, States.NewOrExisting) _
                    .OnEntry(Sub() pnlSelectBulkProduct.Visible = True) _
                    .OnExit(Sub() pnlSelectBulkProduct.Visible = False)

        Return stateMachine
    End Function

    Protected Sub btnNewProduct_Click(sender As Object, e As EventArgs)

    End Sub

    Protected Sub btnExistingProduct_Click(sender As Object, e As EventArgs)

    End Sub

    Protected Sub Urea_Click(sender As Object, e As EventArgs)

    End Sub
End Class

