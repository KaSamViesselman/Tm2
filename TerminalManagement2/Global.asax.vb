Imports System.Web.SessionState
Imports KahlerAutomation.KaTm2Database

Public Class Global_asax
    Inherits System.Web.HttpApplication

    Sub Application_Start(ByVal sender As Object, ByVal e As EventArgs)
        'InitializeDatabase()
        'KaSetting.WriteSetting(Tm2Database.GetDbConnection(), KaSetting.SN_USE_WINDOWS_AUTHENTICATION, Utilities.UseWindowsAuthentication())
        'Common.CheckAuthorized(True, True)
        'Tm2Database.SetApplicationVersion(Tm2Database.Connection, Nothing, System.Net.Dns.GetHostName(), "TM2", Common.ApplicationVersion(), Common.Tm2Authorization.ProductCode, Guid.Empty, "")
        'Tm2Database.RefreshUseWindowsAuthenticationSetting()
    End Sub

    Sub Session_Start(ByVal sender As Object, ByVal e As EventArgs)
        ' Fires when the session is started
        If Not Request.Url.AbsolutePath.ToLower().EndsWith("/about.aspx") AndAlso Not Common.CheckAuthorized(True, True) Then
            Response.Redirect("About.aspx")
        End If
    End Sub

    Sub Application_BeginRequest(ByVal sender As Object, ByVal e As EventArgs)
        ' Fires at the beginning of each request
    End Sub

    Sub Application_AuthenticateRequest(ByVal sender As Object, ByVal e As EventArgs)
        ' Fires upon attempting to authenticate the use
    End Sub

    Sub Application_Error(ByVal sender As Object, ByVal e As EventArgs)
        Dim exception = Server.GetLastError()

        If (exception.InnerException.GetType Is GetType(KaLicenseActivation.Exceptions.NotActivated)) Then
            FormsAuthentication.SignOut()
            Response.Redirect("login.aspx")
        End If
    End Sub

    Sub Session_End(ByVal sender As Object, ByVal e As EventArgs)
        ' Fires when the session ends
    End Sub

    Sub Application_End(ByVal sender As Object, ByVal e As EventArgs)
        ' Fires when the application ends
    End Sub
End Class