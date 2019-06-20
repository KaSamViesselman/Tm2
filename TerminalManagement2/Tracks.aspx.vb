Imports KahlerAutomation.KaTm2Database

Public Class Tracks : Inherits System.Web.UI.Page
    Private _currentUser As KaUser
    Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
    Private _currentTableName As String = KaLocation.TABLE_NAME

#Region "Page_Load"
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Response.Cache.SetCacheability(HttpCacheability.NoCache)
        _currentUser = Utilities.GetUser(Me)
        _currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Facilities")
        If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")

        If Not Page.IsPostBack Then
            SetTextboxMaxLengths()
            ClearFields()
            PopuluateTracksList()
            PopulateLocationList()
            PopulateLengthUnitsList()
            If Page.Request("TrackId") IsNot Nothing Then
                Try
                    ddlTracks.SelectedValue = Page.Request("TrackId")
                Catch ex As ArgumentOutOfRangeException
                    ddlTracks.SelectedIndex = 0
                End Try
            End If
            SetControlUsabilityFromPermissions()
            ddlTracks_SelectedIndexChanged(ddlTracks, New EventArgs())
            Utilities.ConfirmBox(Me.btnDelete, "Are you sure you want to delete this track?")
            Utilities.SetFocus(tbxName, Me)
        End If
    End Sub

    Private Sub PopuluateTracksList()
        ddlTracks.Items.Clear()
        Dim li As ListItem = New ListItem
        If _currentUserPermission(_currentTableName).Create Then li.Text = "Enter a new Track" Else li.Text = "Select a Track"
        li.Value = Guid.Empty.ToString
        ddlTracks.Items.Add(li)
        For Each track As KaTrack In KaTrack.GetAll(GetUserConnection(_currentUser.Id), "deleted = " & Q(False), "name asc")
            li = New ListItem
            li.Text = track.Name
            li.Value = track.Id.ToString
            ddlTracks.Items.Add(li)
        Next
    End Sub

    Private Sub PopulateLocationList()
        ddlFacility.Items.Clear()
        Dim li As ListItem = New ListItem
        li.Text = ""
        li.Value = Guid.Empty.ToString
        ddlFacility.Items.Add(li)
        For Each location As KaLocation In KaLocation.GetAll(GetUserConnection(_currentUser.Id), "deleted = " & Q(False), "name asc")
            li = New ListItem
            li.Text = location.Name
            li.Value = location.Id.ToString
            ddlFacility.Items.Add(li)
        Next
    End Sub

    Private Sub PopulateLengthUnitsList()
        ddlLengthUnit.Items.Clear()
        Dim li As New ListItem
        li.Text = "Feet"
        li.Value = "f"
        ddlLengthUnit.Items.Add(li)
        li = New ListItem
        li.Text = "Meters"
        li.Value = "m"
        ddlLengthUnit.Items.Add(li)
        ddlLengthUnit.SelectedIndex = 0
    End Sub
#End Region

    Private Sub ddlTracks_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlTracks.SelectedIndexChanged
        ClearFields()
        If Guid.Parse(ddlTracks.SelectedValue) <> Guid.Empty Then
            Dim track As KaTrack = New KaTrack(GetUserConnection(_currentUser.Id), Guid.Parse(ddlTracks.SelectedValue))
            With track
                tbxName.Text = .Name
                tbxLength.Text = .Length
                ddlLengthUnit.SelectedValue = IIf(.Metric, "m", "f")
                tbxNotes.Text = .Notes
                Try
                    ddlFacility.SelectedValue = .LocationId.ToString
                Catch ex As ArgumentOutOfRangeException
                    ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidFacility", Utilities.JsAlert("Facility not found where ID = " & .LocationId.ToString & ". Facility not set."))
                    ddlFacility.SelectedIndex = 0
                End Try
                btnDelete.Enabled = True
            End With
        End If
        SetControlUsabilityFromPermissions()
    End Sub

    Private Sub ClearFields()
        lblStatus.Text = ""
        tbxName.Text = ""
        ddlFacility.SelectedIndex = 0
        tbxLength.Text = "0"
        ddlLengthUnit.SelectedValue = "f"
        tbxNotes.Text = ""
        btnDelete.Enabled = False
    End Sub

    Private Sub btnSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSave.Click
        lblStatus.Text = ""
        If ValidateFields() Then
            With New KaTrack
                .Id = Guid.Parse(ddlTracks.SelectedValue)
                .Deleted = False
                .Name = tbxName.Text.Trim
                .LocationId = Guid.Parse(ddlFacility.SelectedValue)
                .Length = Convert.ToDouble(tbxLength.Text)
                .Metric = IIf(ddlLengthUnit.SelectedValue = "m", True, False)
                .Notes = tbxNotes.Text.Trim
                Dim status As String = ""
                If .Id = Guid.Empty Then
                    .SqlInsert(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                    status = "New track added successfully"
                Else
                    .SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                    status = "Selected track updated successfully"
                End If

                PopuluateTracksList()
                ddlTracks.SelectedValue = .Id.ToString
                ddlTracks_SelectedIndexChanged(ddlTracks, New EventArgs())
                lblStatus.Text = status
            End With
        End If
    End Sub

    Private Function ValidateFields() As Boolean
        If tbxName.Text.Trim = "" Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidName", Utilities.JsAlert("A name must be entered")) : Return False
        If Guid.Parse(ddlFacility.SelectedValue) = Guid.Empty Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidFacility", Utilities.JsAlert("A Facility must be selected.")) : Return False

        Dim allTracks As ArrayList = KaTrack.GetAll(GetUserConnection(_currentUser.Id), "deleted = " & Q(False) & " and name = " & Q(tbxName.Text.Trim), "")
        For Each track As KaTrack In allTracks
            If track.Id <> Guid.Parse(ddlTracks.SelectedValue) Then
                ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidNameDuplicated", Utilities.JsAlert("A track with name " & tbxName.Text.Trim & " already exists.  Track name must be unique.")) : Return False
            End If
        Next

        If Not IsNumeric(tbxLength.Text.Trim) Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidLength", Utilities.JsAlert("Length must be a number (greater than zero).")) : Return False
        Return True
    End Function

    Private Sub btnDelete_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnDelete.Click
        lblStatus.Text = ""
        If Guid.Parse(ddlTracks.SelectedValue) <> Guid.Empty Then
            With New KaTrack(GetUserConnection(_currentUser.Id), Guid.Parse(ddlTracks.SelectedValue))
                .Deleted = True
                .SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                ClearFields()
                PopuluateTracksList()
                ddlTracks.SelectedIndex = 0
                lblStatus.Text = "Selected Track deleted successfully"
                btnSave.Enabled = True
                btnDelete.Enabled = False
            End With
        End If
    End Sub

    Private Sub SetTextboxMaxLengths()
        tbxLength.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaTrack.TABLE_NAME, "length"))
        tbxName.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaTrack.TABLE_NAME, "name"))
        tbxNotes.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaTrack.TABLE_NAME, "notes"))
    End Sub

    Private Sub SetControlUsabilityFromPermissions()
        With _currentUserPermission(_currentTableName)
            Dim shouldEnable = (.Edit AndAlso ddlTracks.SelectedIndex > 0) OrElse (.Create AndAlso ddlTracks.SelectedIndex = 0)
            pnlEven.Enabled = shouldEnable
            btnSave.Enabled = shouldEnable
            btnDelete.Enabled = Not Guid.Parse(ddlTracks.SelectedValue).Equals(Guid.Empty) AndAlso .Edit AndAlso .Delete
        End With
    End Sub
End Class