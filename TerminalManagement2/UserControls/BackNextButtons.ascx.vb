Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls

Partial Public Class BackNextButtons
    Inherits System.Web.UI.UserControl

    Public Event [Next] As Action
    Public Event Back As Action

    Public Property BackVisible As Boolean
        Get
            Return btnBack.Visible
        End Get
        Set(ByVal value As Boolean)
            btnBack.Visible = value
        End Set
    End Property

    Public Property NextVisible As Boolean
        Get
            Return btnNext.Visible
        End Get
        Set(ByVal value As Boolean)
            btnNext.Visible = value
        End Set
    End Property

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs)
    End Sub

    Protected Sub btnNext_Click(ByVal sender As Object, ByVal e As EventArgs)
        RaiseEvent [Next]()
    End Sub

    Protected Sub btnBack_Click(ByVal sender As Object, ByVal e As EventArgs)
        RaiseEvent Back()
    End Sub
End Class
