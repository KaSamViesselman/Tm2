Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports KahlerAutomation.KaTm2Database

Partial Public Class SelectProduct
    Inherits System.Web.UI.UserControl

    Public Event CreateProductClicked As Action
    Public Event ExistingProductClicked As Action(Of Guid)

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs)

    End Sub

    Public Sub Initialize(products As ArrayList, createProducts As Boolean)
        btnCreateProduct.Visible = createProducts

        rpProductItems.DataSource = products
        rpProductItems.DataBind()
    End Sub

    Protected Sub CreateNewProduct_Click(ByVal sender As Object, ByVal e As EventArgs)
        RaiseEvent CreateProductClicked()
    End Sub

    Protected Sub Product_Command(sender As Object, e As CommandEventArgs)
        RaiseEvent ExistingProductClicked(Guid.Parse(e.CommandArgument))
    End Sub
End Class
