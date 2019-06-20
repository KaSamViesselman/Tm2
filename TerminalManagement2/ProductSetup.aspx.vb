Imports System
Imports System.Collections.Generic
Imports System.Data.OleDb
Imports System.Linq
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports KahlerAutomation.KaTm2Database

Partial Public Class ProductSetup
    Inherits Page

    <Serializable>
    Enum States
        NewOrExisting
        Existing
        Product
        Recipes
        Panels
        Interfaces
    End Enum

    Enum Triggers
        NewProduct
        ExistingProduct
        ProductConfigured
        [Next]
        Back
    End Enum

    Private Property CurrentState As States
        Get
            Return CType(ViewState("ProductSetupState"), States)
        End Get
        Set(ByVal value As States)
            ViewState("ProductSetupState") = value
        End Set
    End Property

    Private _stateMachine As Stateless.StateMachine(Of States, Triggers)

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs)
        If Not Page.IsPostBack Then
            Page.Title = "Products : Product Setup"
            CurrentState = States.NewOrExisting
            pnlBackNext.Visible = False
            LoadFields()
        End If

        _stateMachine = New Stateless.StateMachine(Of States, Triggers)(Function() CurrentState, Sub(s) CurrentState = s)

        Dim existingPTrigger = _stateMachine.SetTriggerParameters(Of Guid)(Triggers.ExistingProduct)
        Dim productConfigTrigger = _stateMachine.SetTriggerParameters(Of Guid)(Triggers.ProductConfigured)

        _stateMachine.Configure(States.NewOrExisting) _
                     .Permit(Triggers.NewProduct, States.Product) _
                     .Permit(Triggers.ExistingProduct, States.Product) _
                     .OnEntry(Sub()
                                  pnlSelectProduct.Visible = True
                                  pnlBackNext.Visible = False
                              End Sub) _
                     .OnExit(Sub()
                                 pnlSelectProduct.Visible = False
                                 pnlBackNext.Visible = True
                             End Sub)

        _stateMachine.Configure(States.Product) _
                    .Permit(Triggers.Back, States.NewOrExisting) _
                    .Permit(Triggers.ProductConfigured, States.Recipes) _
                    .OnEntryFrom(Triggers.NewProduct, Sub() pnlProduct.SetProduct(New KaProduct(), False)) _
                    .OnEntryFrom(existingPTrigger, AddressOf LoadProduct) _
                    .OnEntry(Sub()
                                 pnlProduct.Visible = True
                                 pnlBackNext.Visible = False
                             End Sub) _
                    .OnExit(Sub()
                                pnlProduct.Visible = False
                                pnlBackNext.Visible = True
                            End Sub)

        _stateMachine.Configure(States.Recipes) _
                     .Permit(Triggers.[Next], States.Panels) _
                     .Permit(Triggers.Back, States.Product) _
                     .OnEntryFrom(productConfigTrigger, AddressOf LoadBulkProducts) _
                     .OnEntry(Sub() pnlRecipes.Visible = True) _
                     .OnExit(Sub() pnlRecipes.Visible = False)

        _stateMachine.Configure(States.Panels) _
                     .Permit(Triggers.[Next], States.Interfaces) _
                     .Permit(Triggers.Back, States.Recipes) _
                     .OnEntry(Sub() pnlBulkPanelSettings.Visible = True) _
                     .OnExit(Sub() pnlBulkPanelSettings.Visible = False)

        _stateMachine.Configure(States.Interfaces) _
                     .Permit(Triggers.Back, States.Panels) _
                     .OnEntry(Sub() pnlInterfaces.Visible = True) _
                     .OnExit(Sub() pnlInterfaces.Visible = False)

        AddHandler pnlSelectProduct.CreateProductClicked, Sub() _stateMachine.Fire(Triggers.NewProduct)
        AddHandler pnlSelectProduct.ExistingProductClicked, Sub(productId) _stateMachine.Fire(existingPTrigger, productId)
        AddHandler pnlProduct.Next, Sub() _stateMachine.Fire(productConfigTrigger, pnlProduct.Product.OwnerId)
        AddHandler pnlProduct.Back, AddressOf Back

        AddHandler pnlBackNext.Next, AddressOf [Next]
        AddHandler pnlBackNext.Back, AddressOf Back
    End Sub

    Private Sub LoadFields()

        Dim currentUser = Utilities.GetUser(Me)
        Dim currentUserPermission = Utilities.GetUserPagePermission(currentUser, New List(Of String)({KaProduct.TABLE_NAME}), "Products")
        Dim userConnection = GetUserConnection(currentUser.Id)
        If Not currentUserPermission(KaProduct.TABLE_NAME).Read Then Response.Redirect("Welcome.aspx")

        Dim owners = KaOwner.GetAll(userConnection, "deleted=0" & IIf(currentUser.OwnerId = Guid.Empty, "", " AND id=" & Q(currentUser.OwnerId)), "name ASC")
        Dim units = KaUnit.GetAll(userConnection, "deleted=0", "name ASC")

        pnlProduct.InitializeOptions(owners, units)

        Dim facilities = KaLocation.GetAll(userConnection, "deleted=0", "name ASC")

        Dim interfaces = KaInterface.GetAll(userConnection, "deleted=0", "name ASC")

        Dim conditions As String = String.Format("deleted=0" & IIf(currentUser.OwnerId <> Guid.Empty, " AND (owner_id={0} OR owner_id={1})", ""), Q(currentUser.OwnerId), Q(Guid.Empty))
        pnlSelectProduct.Initialize(KaProduct.GetAll(userConnection, conditions, "name ASC"), currentUserPermission(KaProduct.TABLE_NAME).Create)
    End Sub

    Private Sub LoadBulkProducts(ownerId As Guid)
        Dim bulkProdWhere As String = ""
        If ownerId <> Guid.Empty Then
            bulkProdWhere = " and (owner_id = " & Q(ownerId) & " or owner_id = " & Q(Guid.Empty) & ")"
        End If

        Dim userConnection = GetUserConnection(Utilities.GetUser(Me).Id)

        Dim productBulkProducts = KaProductBulkProduct.GetAll(userConnection, "deleted=0 AND product_id=" & Q(pnlProduct.Product.Id), "position ASC")
        Dim facilities = KaLocation.GetAll(userConnection, "deleted=0", "name ASC")

        Dim bulkProducts = KaBulkProduct.GetAll(GetUserConnection(Utilities.GetUser(Me).Id), "deleted = " & Q(False) & bulkProdWhere, "name ASC")

        pnlRecipes.Initialize(ToIEnumerable(Of KaLocation)(facilities), ToIEnumerable(Of KaBulkProduct)(bulkProducts), ToIEnumerable(Of KaProductBulkProduct)(productBulkProducts))
    End Sub

    Private Sub LoadProduct(id As Guid)
        Dim showTime As Boolean = False
        Dim connection = GetUserConnection(Utilities.GetUser(Me).Id)

        Try ' to determine if the product is a timed function
            showTime = New KaProduct(connection, id).IsTimedFunction(connection)
        Catch ex As RecordNotFoundException
        End Try

        pnlProduct.SetProduct(New KaProduct(connection, id), showTime)
    End Sub

    Private Sub [Next]()
        _stateMachine.Fire(Triggers.[Next])
    End Sub

    Private Sub Back()
        _stateMachine.Fire(Triggers.Back)
    End Sub

    Private Function ToIEnumerable(Of T)(arrayList As ArrayList)
        Return arrayList.Cast(Of T)().GetEnumerator()
    End Function
End Class
