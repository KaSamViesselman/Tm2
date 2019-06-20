Imports System.Reflection

Public Class Help
	Inherits System.Web.UI.Page

	Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
		Dim v As Version = GetApplicationAssembly.GetName.Version
		lblVersion.Text = KaTm2Database.Tm2Database.FormatVersion(v, "X")
	End Sub

	Private Function GetApplicationAssembly() As Assembly
		Dim a As Assembly
		' Look for web application assembly, otherwise try the EntryAssembly (doesn't work for ASP.NET classic pipeline
		If HttpContext.Current IsNot Nothing Then a = GetWebApplicationAssembly(HttpContext.Current) Else a = Assembly.GetEntryAssembly()
        ' Fall back to executing assembly
        If a IsNot Nothing Then Return a Else Return Assembly.GetExecutingAssembly()
	End Function

	Private Function GetWebApplicationAssembly(ByVal context As HttpContext) As Assembly
		Dim handler As IHttpHandler = context.CurrentHandler
		If handler Is Nothing Then Return Nothing
		Dim type As Type = handler.GetType()
		Do While type IsNot Nothing AndAlso TypeOf type Is Object AndAlso type.Namespace = "ASP"
			type = type.BaseType
		Loop
		Return type.Assembly
	End Function
End Class