Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports KahlerAutomation.KaTm2Database

Partial Public Class ProductInterfaces
    Inherits System.Web.UI.UserControl

    Public Event [Next] As Action
    Public Event Back As Action

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs)
        If (Not Page.IsPostBack) Then
            SetTextboxMaxLengths()
        End If

    End Sub

    Private Sub SetTextboxMaxLengths()
        tbxInterfaceCrossReference.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaProductInterfaceSettings.TABLE_NAME, "cross_reference"))
        tbxPriority.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaProductInterfaceSettings.TABLE_NAME, "product_priority"))
    End Sub
End Class
