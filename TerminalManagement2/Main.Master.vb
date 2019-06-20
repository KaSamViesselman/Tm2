Imports KahlerAutomation.KaTm2Database

Public Class Main
    Inherits System.Web.UI.MasterPage

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        If (Not Page.IsPostBack) Then
            Dim customWebTitle As String = KaSetting.GetSetting(Tm2Database.GetDbConnection(), KaSetting.SN_WEB_PAGE_TITLE, "")

            If (customWebTitle.Length > 0) Then
                Page.Title = customWebTitle + " : " + Page.Title
            End If

            Dim pages = Utilities.GetListOfPagesForUser(Utilities.GetUser(Page.User))

            rpNavigationLinks.DataSource = pages
            rpNavigationLinks.DataBind()

            notification.Visible = Utilities.GetDisplayNotification()
        End If
    End Sub

    Protected Sub notification_Click(sender As Object, e As EventArgs)
        Response.Redirect("Notifications.aspx")
    End Sub
End Class