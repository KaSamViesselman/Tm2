Imports KahlerAutomation.KaTm2Database
Imports System.Reflection
Imports System.Collections.Generic

Public Class About
	Inherits System.Web.UI.Page
	Private _currentUser As KaUser

	Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
		Response.Cache.SetCacheability(HttpCacheability.NoCache)
		_currentUser = Utilities.GetUser(Me)
		If _currentUser.Id.Equals(Guid.Empty) Then
			FormsAuthentication.SetAuthCookie(_currentUser.Id.ToString, False)
		End If

		SetApplicationVersions()

		Dim admin As Boolean = _currentUser.GetPermissionValueByName("AppConfig") = "M"

		Common.RegisterAuthView(authControl, admin)
        updateControl.Initialize(Common.UpdateService, admin)
    End Sub

#Region " Versions "
	Private Sub SetApplicationVersions()
		pnlVersionInformation.Controls.Clear()
		Dim attributes As Object()

        Dim tm2Assembly As Assembly = Common.ApplicationAssembly()
        Dim v As Version = tm2Assembly.GetName().Version

		Dim lfManufacturer As String = "Kahler Automation"
		Try
			attributes = tm2Assembly.GetCustomAttributes(GetType(AssemblyCompanyAttribute), False)
			If (attributes.Length > 0) Then lfManufacturer = CType(attributes(0), AssemblyCompanyAttribute).Company
		Catch ex As Exception
			' suppress
		End Try
		Dim tm2LoadFramworkManufacturer As String = "Manufacturer: {0}"
		AddInformationtoAboutPanel(String.Format(tm2LoadFramworkManufacturer, lfManufacturer), New List(Of KeyValuePair(Of String, String))())

		Dim productName As String = "Terminal Management 2"
		Try
            attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(GetType(AssemblyTitleAttribute), False)
            If attributes.Length > 0 Then productName = CType(attributes(0), AssemblyTitleAttribute).Title
		Catch ex As Exception
			' suppress
		End Try

		Dim copyright As String = ""
		Try
			attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(GetType(AssemblyCopyrightAttribute), False)
			If (attributes.Length <> 0) Then
				copyright = CType(attributes(0), AssemblyCopyrightAttribute).Copyright
			End If
		Catch ex As Exception ' Suppress
		End Try

		lblCopyright.Text = copyright
        AddInformationtoAboutPanel(productName, New List(Of KeyValuePair(Of String, String))({
                New KeyValuePair(Of String, String)("Version", Common.ApplicationVersion()),
                New KeyValuePair(Of String, String)("Copyright", copyright.Replace("Copyright ", ""))}))

        ' KaTm2LoadFramework
        Dim dllAssembly As Assembly = Assembly.GetAssembly(GetType(KahlerAutomation.KaTm2LoadFramework.LfLoad))
		v = dllAssembly.GetName().Version
		Dim lfVersion As String = Tm2Database.FormatVersion(v, "X")

		Dim lfModel As String = "Load Framework"
		Try
			attributes = dllAssembly.GetCustomAttributes(GetType(AssemblyDescriptionAttribute), False)
			If (attributes.Length > 0) Then lfModel = CType(attributes(0), AssemblyDescriptionAttribute).Description
		Catch ex As Exception
			' suppress
		End Try

		Dim lfNTEPMetrologicalVersion As String = "1"
		Try
			attributes = dllAssembly.GetCustomAttributes(GetType(KahlerAutomation.KaTm2LoadFramework.AssemblyAttributes.AssemblyNTEPMetrologicalVersion), False)
			If (attributes.Length > 0) Then lfNTEPMetrologicalVersion = CType(attributes(0), KahlerAutomation.KaTm2LoadFramework.AssemblyAttributes.AssemblyNTEPMetrologicalVersion).MetrologicalVersion
		Catch ex As Exception
			' suppress
		End Try

		Dim lfNTEPCertificateConformanceNumber As String = "14-007"
		Try
			attributes = dllAssembly.GetCustomAttributes(GetType(KahlerAutomation.KaTm2LoadFramework.AssemblyAttributes.AssemblyNTEPCertificateConformanceNumber), False)
			If (attributes.Length > 0) Then lfNTEPCertificateConformanceNumber = CType(attributes(0), KahlerAutomation.KaTm2LoadFramework.AssemblyAttributes.AssemblyNTEPCertificateConformanceNumber).CCNumber
		Catch ex As Exception
			' suppress
		End Try

		AddInformationtoAboutPanel($"{ dllAssembly.GetName().Name}.dll", New List(Of KeyValuePair(Of String, String))({
			New KeyValuePair(Of String, String)("Version", String.Format("{0} M{1:0}", KaCommonObjects.Assembly.GetAppVersion(dllAssembly), lfNTEPMetrologicalVersion)),
			New KeyValuePair(Of String, String)("CC", lfNTEPCertificateConformanceNumber)}))

		' KaTm2Datbase
		dllAssembly = Assembly.GetAssembly(GetType(KaOrder))
		v = dllAssembly.GetName().Version
		Dim tm2DbVersion As String = Tm2Database.FormatVersion(v, "X")

		Dim tm2DatabaseVersion As Integer = 0
		Try
			attributes = dllAssembly.GetCustomAttributes(GetType(KahlerAutomation.KaTm2Database.AssemblyAttributes.AssemblyDatabaseVersion), False)
			If (attributes.Length > 0) Then tm2DatabaseVersion = CType(attributes(0), KahlerAutomation.KaTm2Database.AssemblyAttributes.AssemblyDatabaseVersion).DatabaseVersion
		Catch ex As Exception
			' suppress
		End Try
		AddInformationtoAboutPanel($"{ dllAssembly.GetName().Name}.dll", New List(Of KeyValuePair(Of String, String))({
			New KeyValuePair(Of String, String)("Version", KaCommonObjects.Assembly.GetAppVersion(dllAssembly))
		}))

		' KaController
		AddInformationFromAssemblyType(GetType(KaController.Controller))

		' KaModBus
		AddInformationFromAssemblyType(GetType(KaModbus.ModbusNetwork))

		' KaCommonControls
		AddInformationFromAssemblyType(GetType(KahlerAutomation.EnterText))

		' KaCommonObjects
		AddInformationFromAssemblyType(GetType(KaCommonObjects.Logging))

		' KaLicenseActivation
		AddInformationFromAssemblyType(GetType(KaLicenseActivation.ActivatedClient))

		' KaTm2LicenseActivation
		AddInformationFromAssemblyType(GetType(KaTm2LicenseActivation.Authorization))

		' KaUpdate
		AddInformationFromAssemblyType(GetType(KaUpdate.Service))

		' Common Web Controls
		AddInformationFromAssemblyType(GetType(UpdateWebControl.UpdateControl))
	End Sub

	Private Sub AddInformationtoAboutPanel(groupingInfo As String, info As List(Of KeyValuePair(Of String, String)))
		If (info.Count > 0) Then
			pnlVersionInformation.Controls.Add(New HtmlGenericControl("li") With {.InnerHtml = $"<h2>{Server.HtmlEncode(groupingInfo)}</h2>"})
			For Each item As KeyValuePair(Of String, String) In info
				Dim row As HtmlGenericControl = New HtmlGenericControl("li")
				row.Controls.Add(New HtmlGenericControl("label") With {.InnerText = item.Key})
				Dim value As HtmlGenericControl = New HtmlGenericControl("label") With {.InnerText = item.Value}
				value.Attributes("Style") = "width:65%; text-align:left;"
				row.Controls.Add(value)
				pnlVersionInformation.Controls.Add(row)
			Next
		End If
	End Sub

	Private Sub AddInformationFromAssemblyType(type As Type)
		Dim assembly = Reflection.Assembly.GetAssembly(type)
		AddInformationtoAboutPanel($"{ assembly.GetName().Name}.dll", New List(Of KeyValuePair(Of String, String))({
			New KeyValuePair(Of String, String)("Version", KaCommonObjects.Assembly.GetAppVersion(assembly))
		}))
	End Sub
#End Region
End Class