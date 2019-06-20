Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb
Imports System.Drawing
Imports System.Drawing.Drawing2D

Public Class TankLevelTrendData : Inherits System.Web.UI.Page
	Private _currentUser As KaUser
	Public Shared LevelFillColor As Color = Color.FromArgb(&H99, &HCC, &HFF)
	Public Shared TemperatureFillColor As Color = Color.FromArgb(0, 51, 151)

#Region "Events"
	Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
		_currentUser = Utilities.GetUser(Me)
		If Not Page.IsPostBack Then
			Dim tankLevelTrendId As Guid = Guid.Parse(Request.QueryString("tank_level_trend_id"))
			Dim displayUnitId As Guid = Guid.Empty
			Dim startDate As DateTime = New DateTime(Now.Year, Now.Month, Now.Day, 0, 0, 0)
			Dim endDate As DateTime = New DateTime(Now.Year, Now.Month, Now.Day, 23, 59, 59)
			Dim includeTemp As Boolean = False
			Guid.TryParse(Request.QueryString("DisplayUnitId"), displayUnitId)
			DateTime.TryParse(Server.HtmlDecode(Request.QueryString("StartDate")), startDate)
			DateTime.TryParse(Server.HtmlDecode(Request.QueryString("EndDate")), endDate)
			Boolean.TryParse(Server.HtmlDecode(Request.QueryString("IncludeTemp")), includeTemp)

			PopulateReport(tankLevelTrendId, displayUnitId, startDate, endDate, includeTemp)
		End If
	End Sub
#End Region

	Private Function ConvertImageToData(ByVal image As Image) As String
		If image Is Nothing Then Return ""
		Dim s As New IO.MemoryStream()
		image.Save(s, Drawing.Imaging.ImageFormat.Png)
		s.Position = 0
		Dim data(s.Length - 1) As Byte
		s.Read(data, 0, s.Length)
		s.Close()
		Return Convert.ToBase64String(data)
	End Function

	Private Sub PopulateReport(ByVal tankLevelTrendId As Guid, displayUnitId As Guid, startDate As DateTime, endDate As DateTime, includeTemp As Boolean)
		litData.Text = KaReports.GetTableHtml(New KaTankLevelTrend(GetUserConnection(_currentUser.Id), tankLevelTrendId).Name, KaReports.GetTankLevelTrendTable(GetUserConnection(_currentUser.Id), tankLevelTrendId, startDate, endDate, False, displayUnitId, includeTemp))
		imgGraph.Src = $"data:image/png;base64,{ConvertImageToData(KaReports.GetTankLevelTrendGraph(GetUserConnection(_currentUser.Id), tankLevelTrendId, startDate, endDate, 4.5, displayUnitId, includeTemp, LevelFillColor, TemperatureFillColor))}"
		imgGraph.Attributes().Item("width") = "100%"
		imgGraph.Attributes().Item("height") = "auto"
	End Sub
End Class