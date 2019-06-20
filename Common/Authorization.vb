Imports System.Web
Imports KahlerAutomation.KaTm2Database
Imports KahlerAutomation.KaTm2LicenseActivation

Public Module Authorization
	Private WithEvents _tm2Authorization As KaTm2LicenseActivation.Authorization = Nothing
	Public ReadOnly Property Tm2Authorization As KaTm2LicenseActivation.Authorization
		Get
			Return _tm2Authorization
		End Get
	End Property

    Private _webApplication As Boolean = False

    Public Const APPLICATION_ID As String = "TM2"
	Private _applicationIdentifier = System.Net.Dns.GetHostName & "/" & APPLICATION_ID

    Public ReadOnly Property ApplicationIdentifier As String
        Get
            Return _applicationIdentifier
        End Get
    End Property

    Public ReadOnly Property UpdateService As KaUpdate.Service
        Get
            Return Authorization.UpdateService
        End Get
    End Property

    Public Sub SetRunning()
        Authorization.UpdateService.SetApplicationRunning()
    End Sub

    Public Sub SetNotRunning()
        Authorization.UpdateService.SetNotApplicationRunning()
    End Sub

    Public Function CheckInstalling()
        Return Authorization.UpdateService.UpdateInstalling
    End Function

    Private ReadOnly Property Authorization As KaTm2LicenseActivation.Authorization
		Get
			Dim licenseType As KaLicenseActivation.LicenseType = KaLicenseActivation.LicenseType.Timed
#If PERMANENT_LICENSE Then
                licenseType = KaLicenseActivation.LicenseType.Permanent
#End If
			If (_tm2Authorization Is Nothing) Then
                _tm2Authorization = New KaTm2LicenseActivation.Authorization(Reflection.Assembly.GetExecutingAssembly(), AppDomain.CurrentDomain.BaseDirectory, "Terminal Management 2", Tm2Database.Connection, licenseType)

                AddHandler _tm2Authorization.StartUpdateInstall, Function()
                                                                     ' Only allow the web site to update, not the applications
                                                                     Return _webApplication
                                                                 End Function
                AddHandler _tm2Authorization.Shutdown, Sub()
														   HttpRuntime.UnloadAppDomain()
													   End Sub
			End If
			Return _tm2Authorization
		End Get
	End Property

    Public Sub RegisterAuthView(authView As KaLicenseActivation.ILicenseView, admin As Boolean)
        Authorization.RegisterView(authView, admin)
    End Sub

    Public Function CheckAuthorized(webApplication As Boolean, contactServer As Boolean)
		_webApplication = webApplication
		Return Authorization.Authorized(If(contactServer, AuthCheckType.Server, AuthCheckType.Cached))
	End Function

	Public Function CheckAuthorized(webApplication As Boolean, almostExpired As Action(Of Integer), trialMode As Action(Of Integer), notActivatedNotification As Action(Of String), contactServer As Boolean)
		_webApplication = webApplication
		Return Authorization.Authorized(almostExpired, trialMode, notActivatedNotification, If(contactServer, AuthCheckType.Server, AuthCheckType.Cached))
	End Function

    Public Function ApplicationAssembly() As Reflection.Assembly
        Return Reflection.Assembly.GetExecutingAssembly()
    End Function

    Public Function ApplicationVersion() As String
        Return KaCommonObjects.Assembly.GetAppVersion(ApplicationAssembly())
    End Function

    Sub WriteToEventLog(message As String, category As KaEventLog.Categories)
        Dim newEvent As New KaEventLog
        With newEvent
            .ApplicationIdentifier = _applicationIdentifier
            .ApplicationVersion = KaCommonObjects.Assembly.GetAppVersion(Reflection.Assembly.GetExecutingAssembly())
            .Category = category
            .Computer = My.Computer.Name
            .Description = message
        End With
        KaEventLog.CreateEventLog(Tm2Database.Connection, newEvent, 12, Nothing, _applicationIdentifier, "")
    End Sub
End Module
