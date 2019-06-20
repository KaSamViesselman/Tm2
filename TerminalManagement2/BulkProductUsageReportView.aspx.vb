Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb
Public Class BulkProductUsageReportView
	Inherits System.Web.UI.Page

	Private _currentUser As KaUser
	Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
		Response.Cache.SetCacheability(HttpCacheability.NoCache)
		_currentUser = Utilities.GetUser(Me)
		If Not Utilities.GetUserPagePermission(_currentUser, New List(Of String)({"reports"}), "Reports")("reports").Read Then Response.Redirect("Welcome.aspx")

		If Not Page.IsPostBack Then
			PopulateReport()
		End If
	End Sub

	Private Sub PopulateReport()
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)

		Dim fromDate As DateTime = DateTime.Parse(Request.QueryString("fromDate"))
		Dim toDate As DateTime = DateTime.Parse(Request.QueryString("toDate"))
		Dim ownerId As Guid = Guid.Parse(Request.QueryString("ownerId"))
		Dim panelId As Guid = Guid.Parse(Request.QueryString("panelId"))
		Dim bayId As Guid = Guid.Parse(Request.QueryString("bayId"))
		Dim includeVoidedTickets As Boolean = False
		Boolean.TryParse(Request.QueryString("include_voided"), includeVoidedTickets)
		Dim decimalsDisplayed As Integer
		Dim units As New List(Of KaUnit)
		Dim unitsAndDecimals As String = Request.QueryString("units_displayed")
		If unitsAndDecimals IsNot Nothing Then
			For Each unitItem As String In unitsAndDecimals.Split("|")
				Dim values() As String = unitItem.Split(":")
				Try
					Dim unit As New KaUnit(connection, Guid.Parse(values(0)))
					decimalsDisplayed = 0
					If Integer.TryParse(values(1), decimalsDisplayed) Then
						unit.UnitPrecision = Tm2Database.GeneratePrecisionFormat(1, decimalsDisplayed)
					End If
					If Not units.Contains(unit) Then units.Add(unit)
				Catch ex As Exception

				End Try
			Next
		End If

		Dim mediaType As String = KaReports.MEDIA_TYPE_HTML
		If Request.QueryString("media_type") IsNot Nothing Then mediaType = Request.QueryString("media_type")
		Dim bulkProductList As New List(Of Guid)
		If Request.QueryString("bulkProdList") IsNot Nothing Then
			Try
				bulkProductList = Tm2Database.FromXml(Request.QueryString("bulkProdList"), GetType(List(Of Guid)))
			Catch ex As Exception

			End Try
		End If
		Dim reportTitle As String = GenerateHeader(connection, bulkProductList, fromDate, toDate, ownerId, bayId, panelId)
		litReport.Text = KaReports.GetTableHtml(reportTitle, "", KaReports.GetBulkProductUsageReport(connection, fromDate, toDate, ownerId, panelId, bayId, units, bulkProductList, KaReports.MEDIA_TYPE_HTML, includeVoidedTickets))
	End Sub

	Public Shared Function GenerateHeader(connection As OleDbConnection, bulkProductList As List(Of Guid), fromdate As DateTime, todate As DateTime, ownerId As Guid, bayId As Guid, panelId As Guid) As String
		Dim header As String = "Bulk product usage report from " & fromdate & " to " & todate
		If Not ownerId.Equals(Guid.Empty) Then
			header &= ", for owner '" & (New KaOwner(connection, ownerId)).Name & "'"
		End If
		If Not bayId.Equals(Guid.Empty) Then
			header &= ", in the '" & (New KaBay(connection, bayId)).Name & "' bay"
		End If
		If Not panelId.Equals(Guid.Empty) Then
			header &= ", using the '" & (New KaPanel(connection, panelId)).Name & "' panel"
		End If
		If bulkProductList.Count > 0 Then
			Dim bulkNameList As String = ""
			For Each bulkProdId As Guid In bulkProductList
				If bulkNameList.Length > 0 Then bulkNameList &= ", "
				Try
					bulkNameList &= Q(New KaBulkProduct(connection, bulkProdId).Name)
				Catch ex As RecordNotFoundException
				End Try
			Next
			header &= ", for the bulk product" & IIf(bulkProductList.Count > 1, "s", "") & ": " & bulkNameList
		End If
		Return header
	End Function


End Class