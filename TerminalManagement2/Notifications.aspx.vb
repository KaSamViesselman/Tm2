Imports KahlerAutomation.KaTm2Database
Imports System.Environment
Imports System.Data.OleDb

Public Class Notifications : Inherits System.Web.UI.Page
	Private _currentUser As KaUser
	Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
	Private _currentTableName As String = KaSetting.TABLE_NAME

	Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
		Response.Cache.SetCacheability(HttpCacheability.NoCache)
		_currentUser = Utilities.GetUser(Me)
		_currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "GeneralSettings")
		If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")

		If Not Page.IsPostBack Then
			CheckForUpdateVersions()
		End If
	End Sub

#Region " Updates "
	Private Sub CheckForUpdateVersions()
		Do While tblUpdatesAvailable.Rows.Count > 1
			tblUpdatesAvailable.Rows.RemoveAt(1)
		Loop
		Dim updatesTable As DataTable = New DataTable()
		Dim updatesRdr As OleDbDataReader = Nothing
		Try
			updatesRdr = Tm2Database.ExecuteReader(Tm2Database.Connection, $"SELECT {KaApplicationInformation.FN_PC_NAME}, {KaApplicationInformation.FN_APPLICATION_NAME}, {KaApplicationInformation.FN_APPLICATION_VERSION}, {KaApplicationInformation.FN_UPDATE_VERSION}, {KaApplicationInformation.FN_UPDATE_NOTES} FROM {KaApplicationInformation.TABLE_NAME} WHERE {KaApplicationInformation.FN_DELETED} = 0 AND {KaApplicationInformation.FN_UPDATE_AVAILABLE} = 1 ORDER BY {KaApplicationInformation.FN_APPLICATION_NAME}, {KaApplicationInformation.FN_PC_NAME}")
			While updatesRdr.Read()
				Dim updateRow As TableRow = New TableRow
				updateRow.Cells.Add(New TableCell() With {.Text = updatesRdr.Item(KaApplicationInformation.FN_APPLICATION_NAME)})
				updateRow.Cells.Add(New TableCell() With {.Text = updatesRdr.Item(KaApplicationInformation.FN_PC_NAME)})
				updateRow.Cells.Add(New TableCell() With {.Text = updatesRdr.Item(KaApplicationInformation.FN_APPLICATION_VERSION)})
				updateRow.Cells.Add(New TableCell() With {.Text = updatesRdr.Item(KaApplicationInformation.FN_UPDATE_VERSION)})
				updateRow.Cells.Add(New TableCell() With {.Text = updatesRdr.Item(KaApplicationInformation.FN_UPDATE_NOTES)})
				tblUpdatesAvailable.Rows.Add(updateRow)
			End While
		Catch ex As Exception
		Finally
			If updatesRdr IsNot Nothing Then updatesRdr.Close()
		End Try
		pnlUpdatesAvailable.Visible = tblUpdatesAvailable.Rows.Count > 1
	End Sub
#End Region

	Protected Sub ToolkitScriptManager1_AsyncPostBackError(ByVal sender As Object, ByVal e As System.Web.UI.AsyncPostBackErrorEventArgs)
		ToolkitScriptManager1.AsyncPostBackErrorMessage = e.Exception.Message
	End Sub

	Private Sub DisplayJavaScriptMessage(key As String, script As String)
		ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), key, script, False)
	End Sub
End Class