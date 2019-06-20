Imports System.Linq
Imports KahlerAutomation.KaTm2Database

Public Class SelectOwners
    Inherits System.Web.UI.UserControl

    Public Enum SelectBehavior
        One
        All
    End Enum

    Public Property Behavior As SelectBehavior = SelectBehavior.All

    Public ReadOnly Property SelectedOwnerId As Guid
        Get
            Return Guid.Parse(ddlOwner.SelectedValue)
        End Get
    End Property

    Public ReadOnly Property SelectedOwnerName As String
        Get
            Return ddlOwner.SelectedItem.Text
        End Get
    End Property

    Public Property SelectedValue As String
        Get
            Return ddlOwner.SelectedValue
        End Get
        Set(value As String)
            Try
                ddlOwner.SelectedValue = value
            Catch ex As ArgumentOutOfRangeException
                ddlOwner.SelectedIndex = 0
                ScriptManager.RegisterClientScriptBlock(Me, Me.GetType(), "InvalidOwnerId", Utilities.JsAlert("Record not found in owners with ID = " & value & ". Owner set to ""all owners."" instead."), False)
            End Try
        End Set
    End Property

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    End Sub

    Public Sub SetItems(owners As ArrayList)

        ddlOwner.Items.Clear()

        If SelectBehavior.All = Behavior Then
            ddlOwner.Items.Add(New ListItem("All owners", Guid.Empty.ToString()))
        End If

        For Each owner In owners
            ddlOwner.Items.Add(New ListItem(owner.Name, owner.Id.ToString()))
        Next
    End Sub
End Class