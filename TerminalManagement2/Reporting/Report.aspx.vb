Imports KahlerAutomation.KaTm2Database

Public Class Report
	Inherits System.Web.UI.Page

	Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
		If Not Page.IsPostBack Then
			If Page.Request("InstanceId") IsNot Nothing AndAlso Session.Item(Page.Request("InstanceId")) IsNot Nothing Then
				Dim ds As DataSet = New DataSet
				Dim fieldAliasHeaderCaptions As Dictionary(Of String, String) = New Dictionary(Of String, String)
				Dim fieldAliasHeaderList As List(Of KeyValuePair(Of String, String)) = Session.Item(Page.Request("FieldAliasHeaderList"))
				For Each pair As KeyValuePair(Of String, String) In fieldAliasHeaderList
					fieldAliasHeaderCaptions.Add(pair.Key, pair.Value)
				Next

				Dim sql As String = Session.Item(Page.Request("InstanceId"))
				If sql.Length > 0 Then
					Dim da As System.Data.OleDb.OleDbDataAdapter = New OleDb.OleDbDataAdapter(sql, Tm2Database.Connection)
					da.Fill(ds)
				End If

				'Report Viewer, Builder And Engine 
				ReportViewer1.Reset()
				For i As Integer = 0 To ds.Tables.Count - 1
					ReportViewer1.LocalReport.DataSources.Add(New Microsoft.Reporting.WebForms.ReportDataSource(ds.Tables(i).TableName, ds.Tables(i)))
				Next

				Dim reportTitle As String = Session.Item("ReportTitle")
				ReportViewer1.LocalReport.LoadReportDefinition(Tm2Reporting.DynamicRDLCGenerator.ReportEngine.GenerateReport(ds, fieldAliasHeaderCaptions, reportTitle))
			End If
		End If
	End Sub
End Class