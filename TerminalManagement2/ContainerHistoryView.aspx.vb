Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class ContainerHistoryView : Inherits System.Web.UI.Page
	Private _currentUser As KaUser

	Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
		Response.Cache.SetCacheability(HttpCacheability.NoCache)
		_currentUser = Utilities.GetUser(Me)
		If Not Utilities.GetUserPagePermission(_currentUser, New List(Of String)({"reports"}), "Reports")("reports").Read AndAlso Not Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaContainer.TABLE_NAME}), "Containers")(KaContainer.TABLE_NAME).Read Then Response.Redirect("Welcome.aspx")
		If Not Page.IsPostBack Then
			Dim containerId As Guid = Guid.Parse(Request.QueryString("containerId"))
			Dim columnsDisplayed As ULong = ContainerHistory.GetCurrentColumnSetting(_currentUser.Id)
			If Request.QueryString("columnsDisplayed") IsNot Nothing Then columnsDisplayed = Request.QueryString("columnsDisplayed")
			Dim cellAttributes As New List(Of String)
			litContainers.Text = KaReports.GetTableHtml("", "", KaReports.GetContainerHistoryTable(GetUserConnection(_currentUser.Id), KaReports.MEDIA_TYPE_PFV, containerId, columnsDisplayed, SQL_MINDATE, SQL_MAXDATE, cellAttributes), False, "class=""display"" width=""100%""", "", New List(Of String), "", New List(Of String), cellAttributes, True)
		End If
	End Sub

	Public Function GetControl(ByVal target As String) As System.Web.UI.Control
		Return FindControl(target)
	End Function
End Class