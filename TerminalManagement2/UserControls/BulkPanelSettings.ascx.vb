Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports KahlerAutomation.KaTm2Database

Partial Public Class BulkPanelSettings
    Inherits System.Web.UI.UserControl

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs)
        If (Not Page.IsPostBack) Then
            rpFacility.DataSource = KaLocation.GetAll(GetUserConnection(Utilities.GetUser(Page).Id), "deleted=0", "name ASC")

            AddHandler rpFacility.ItemDataBound, AddressOf FacilityDataBound

            rpFacility.DataBind()

        End If
    End Sub

    Private Sub FacilityDataBound(sender As Object, e As RepeaterItemEventArgs)

        If (e.Item.ItemType = ListItemType.Item Or e.Item.ItemType = ListItemType.AlternatingItem) Then
            Dim value = e.Item.DataItem

            Dim panelsRepeater = CType(e.Item.FindControl("rpPanels"), Repeater)

            Dim testPanel1 = New Panel
            Dim testPanel2 = New Panel

            testPanel1.Name = "0-60-0"
            testPanel2.Name = "DAP"

            panelsRepeater.DataSource = New List(Of Panel) From {testPanel1, testPanel2}
            panelsRepeater.DataBind()
        End If
    End Sub

    Private Class Panel
        Public Property Name As String
    End Class
End Class
