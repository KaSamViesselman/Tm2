Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports KahlerAutomation.KaTm2Database

Partial Public Class Recipes
    Inherits System.Web.UI.UserControl

    Private Property Facilities As List(Of Facility)
        Get
            Return CType(ViewState("Facilities"), List(Of Facility))
        End Get
        Set(value As List(Of Facility))
            ViewState("Facilities") = value
        End Set
    End Property

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs)
        If (Not Page.IsPostBack) Then
            SetTextboxMaxLengths()
            RegisterScripts()


        End If
    End Sub

    Public Sub Initialize(dbFacilities As IEnumerable(Of KaLocation), bulkProducts As IEnumerable(Of KaBulkProduct), pBulkProducts As IEnumerable(Of KaProductBulkProduct))

        Facilities = dbFacilities.Select(Function(l) New Facility() With {
                                             .Name = l.Name, .Id = l.Id,
                                             .BulkProducts = pBulkProducts.Where(Function(bp) bp.LocationId = l.Id) _
                                                                          .Select(Function(bp) New BulkProduct() With {
                                                                                    .AllowUp = True, .AllowDown = True,
                                                                                    .Name = bulkProducts.Where(Function(bProduct) bProduct.Id = bp.BulkProductId).First().Name,
                                                                                    .Portion = bp.Portion}).ToList(),
                                                                                    .Display = .BulkProducts.Count > 0}) _
                                                                          .ToList()

        Facilities.ForEach(Sub(f)
                               If (f.Display) Then
                                   f.BulkProducts.First().AllowUp = False
                                   f.BulkProducts.Last().AllowDown = False
                               End If
                           End Sub)

        ddlBulkProduct.Items.Clear()
        bulkProducts.ToList().ForEach(Sub(bp) ddlBulkProduct.Items.Add(New ListItem(bp.Name, bp.Id.ToString())))

        BindFacilitiesToPage()
    End Sub

    Private Sub FacilityDataBound(sender As Object, e As RepeaterItemEventArgs)

        If (e.Item.ItemType = ListItemType.Item Or e.Item.ItemType = ListItemType.AlternatingItem) Then

            Dim facility = CType(e.Item.DataItem, Facility)
            Dim repeater = CType(e.Item.FindControl("rpBulkProducts"), Repeater)

            repeater.DataSource = facility.BulkProducts
            AddHandler repeater.ItemDataBound, AddressOf ProductDataBound

            repeater.DataBind()
        End If
    End Sub

    Private Sub ProductDataBound(sender As Object, e As RepeaterItemEventArgs)
        If (e.Item.ItemType = ListItemType.Item Or e.Item.ItemType = ListItemType.AlternatingItem) Then
            Dim bProduct = CType(e.Item.DataItem, BulkProduct)

            If (Not bProduct.AllowDown) Then
                CType(e.Item.FindControl("btnDown"), LinkButton).CssClass += " disabled"
            End If

            If Not bProduct.AllowUp Then
                CType(e.Item.FindControl("btnUp"), LinkButton).CssClass += " disabled"
            End If
        End If
    End Sub

    Private Sub SetTextboxMaxLengths()

        '        tbxPercent.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaProductBulkProduct.TABLE_NAME, "portion"))

    End Sub

    <Serializable>
    Private Class Facility
        Public Property Name As String
        Public Property Id As Guid
        Public Property Display As Boolean
        Public Property BulkProducts As List(Of BulkProduct)
        Public ReadOnly Property Total As Double
            Get
                'ToDo do the math here to sum everything from the bulk products
                Return 60
            End Get
        End Property
    End Class

    <Serializable>
    Private Class BulkProduct
        Public Property Name As String
        Public Property Portion As Double
        Public Property AllowUp As Boolean
        Public Property AllowDown As Boolean
    End Class

    Protected Sub ddlBulkProduct_SelectedIndexChanged(sender As Object, e As EventArgs)
        Dim bulkProductId As Guid = Guid.Parse(ddlBulkProduct.SelectedValue)

        If Utilities.IsBulkProductParameterlessFunction(bulkProductId, Utilities.GetUser(Page).Id) Then
            lblPortionUnit.Text = ""
            tbxPercent.Visible = False
        ElseIf Utilities.IsBulkProductTimedFunction(bulkProductId, Utilities.GetUser(Page).Id) Then
            lblPortionUnit.Text = "Seconds"
            tbxPercent.Visible = True
        Else
            lblPortionUnit.Text = "Percent of total"
            tbxPercent.Visible = True
        End If
        Dim reopenModal = "$('#addEditRecipeModal').modal('show');"
        ScriptManager.RegisterStartupScript(Me, Me.GetType(), "ReopenModal", reopenModal, True)
    End Sub

    Private Sub RegisterScripts()

        Dim script As String = "function changeHideShow(button) {
                                    if( button.innerHTML == ""Show"") {
                                        button.innerHTML = ""Hide""
                                    } else { button.innerHTML = ""Show""}
                                 }"

        ScriptManager.RegisterClientScriptBlock(Me, Me.GetType(), "changeHideShow", script, True)
    End Sub

    Protected Sub btnAddRemove_Command(sender As Object, e As CommandEventArgs)

        Dim facilityId = Guid.Parse(CType(e.CommandArgument, String))
        Dim selectedFacility = Facilities.Where(Function(f) f.Id = facilityId).First()

        If (e.CommandName = "Add") Then
            selectedFacility.Display = True
        ElseIf (e.CommandName = "Remove") Then
            selectedFacility.Display = False
        Else
            Throw New InvalidOperationException("No CommandName " + e.CommandName + " exists")
        End If

        BindFacilitiesToPage()
    End Sub

    Private Sub BindFacilitiesToPage()
        AddHandler rpFacilities.ItemDataBound, AddressOf FacilityDataBound

        rpFacilities.DataSource = Facilities
        rpFacilities.DataBind()
    End Sub
End Class
