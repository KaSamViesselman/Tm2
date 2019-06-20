Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class CropTypes
    Inherits System.Web.UI.Page

    Private _currentUser As KaUser
Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
    Private _currentTableName As String = KaCropType.TABLE_NAME
    Private _customFields As New List(Of KaCustomField)
    Private _customFieldData As New List(Of KaCustomFieldData)

#Region "Events"
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        Response.Cache.SetCacheability(HttpCacheability.NoCache)
        _currentUser = Utilities.GetUser(Me)
        _currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Crops")
        If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")

        If Not Page.IsPostBack Then
            SetTextboxMaxLengths()

            _customFields.Clear()
            For Each customField As KaCustomField In KaCustomField.GetAll(GetUserConnection(_currentUser.Id), String.Format("deleted=0 AND {0} = {1}", KaCustomField.FN_TABLE_NAME, Q(KaCropType.TABLE_NAME)), KaCustomField.FN_FIELD_NAME)
                _customFields.Add(customField)
            Next

            PopulateCropTypeList()
            If Page.Request("CropTypeId") IsNot Nothing Then
                Try
                    ddlCropTypes.SelectedValue = Page.Request("CropTypeId")
                Catch ex As ArgumentOutOfRangeException
                    ddlCropTypes.SelectedIndex = 0
                End Try
            End If
            ddlCropTypes_SelectedIndexChanged(ddlCropTypes, New EventArgs())
            Utilities.ConfirmBox(Me.btnDelete, "Are you sure you want to delete this crop type?") ' Delete confirmation box setup
            Utilities.SetFocus(tbxName, Me) ' set focus to the first textbox on the page
        End If
    End Sub

    Protected Sub ddlCropTypes_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles ddlCropTypes.SelectedIndexChanged
        PopulateCropTypeInformation(Guid.Parse(ddlCropTypes.SelectedValue))
        SetControlUsabilityFromPermissions()
    End Sub

    Protected Sub btnSave_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnSave.Click
        lblStatus.Text = ""
        If ValidateFields() Then
            With New KaCropType()
                .Id = Guid.Parse(ddlCropTypes.SelectedValue)
                .Name = tbxName.Text
                Dim status As String = ""
                If .Id <> Guid.Empty Then
                    .SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                    status = "Crop type updated successfully."
                Else
                    .SqlInsert(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                    status = "Crop type added successfully."
                End If

                Utilities.ConvertCustomFieldPanelToLists(_customFields, _customFieldData, lstCustomFields)
                For Each customData As KaCustomFieldData In _customFieldData
                    customData.RecordId = .Id
                    customData.SqlUpdateInsertIfNotFound(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                Next

                PopulateCropTypeList()
                ddlCropTypes.SelectedValue = .Id.ToString()
                ddlCropTypes_SelectedIndexChanged(ddlCropTypes, New EventArgs())
                lblStatus.Text = status
                btnDelete.Enabled = _currentUserPermission(_currentTableName).Delete
            End With
        End If
    End Sub

    Private Function ValidateFields() As Boolean
        If tbxName.Text.Trim().Length = 0 Then ' user didn't enter a name
            ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidName", Utilities.JsAlert("Please specify a name."))
            Return False
        End If
        Dim conditions As String = String.Format("deleted=0 AND name={0} AND id<>{1}", Q(tbxName.Text), Q(ddlCropTypes.SelectedValue))
        If KaCropType.GetAll(GetUserConnection(_currentUser.Id), conditions, "").Count > 0 Then ' a crop type with the specified name already exists
            ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidDuplicateName", Utilities.JsAlert("A crop type with the name """ & tbxName.Text & """ already exists. Please specify a unique name for this crop type."))
            Return False
        End If
        Return True
    End Function

    Protected Sub btnDelete_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnDelete.Click
        With New KaCropType(GetUserConnection(_currentUser.Id), Guid.Parse(ddlCropTypes.SelectedValue))
            .Deleted = True
            .SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
            lblStatus.Text = "Crop type deleted successfully."
        End With
        PopulateCropTypeList()
        PopulateCropTypeInformation(Guid.Empty)
        btnDelete.Enabled = False
    End Sub
#End Region

    Private Sub PopulateCropTypeList()
        ddlCropTypes.Items.Clear()
        If _currentUserPermission(_currentTableName).Create Then ddlCropTypes.Items.Add(New ListItem("Enter new crop type", Guid.Empty.ToString())) Else ddlCropTypes.Items.Add(New ListItem("Select a crop type", Guid.Empty.ToString()))
        For Each r As KaCropType In KaCropType.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
            ddlCropTypes.Items.Add(New ListItem(r.Name, r.Id.ToString()))
        Next
    End Sub

    Private Sub PopulateCropTypeInformation(ByVal id As Guid)
        _customFieldData.Clear()
        With New KaCropType()
            .Id = id
            Try
                .SqlSelect(GetUserConnection(_currentUser.Id))
                btnDelete.Enabled = _currentUserPermission(_currentTableName).Delete

                For Each customFieldValue As KaCustomFieldData In KaCustomFieldData.GetAll(GetUserConnection(_currentUser.Id), String.Format("deleted = 0 AND {0} = {1}", KaCustomFieldData.FN_RECORD_ID, Q(.Id)), KaCustomFieldData.FN_LAST_UPDATED)
                    _customFieldData.Add(customFieldValue)
                Next
            Catch ex As RecordNotFoundException
                btnDelete.Enabled = False
            End Try
            tbxName.Text = .Name
        End With
        Utilities.CreateDynamicCustomFieldPanelControls(_customFields, _customFieldData, lstCustomFields, Page)
    End Sub

    Private Sub SetTextboxMaxLengths()
        tbxName.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaCropType.TABLE_NAME, "name"))
    End Sub

    Protected Overrides Function SaveViewState() As Object
        Dim viewState(2) As Object

        Utilities.ConvertCustomFieldPanelToLists(_customFields, _customFieldData, lstCustomFields)
        viewState(0) = MyBase.SaveViewState()
        viewState(1) = _customFields
        viewState(2) = _customFieldData
        Return viewState
    End Function

    Protected Overrides Sub LoadViewState(savedState As Object)
        If savedState IsNot Nothing AndAlso CType(savedState, Object).Length = 3 Then
            Dim viewState As Object() = savedState
            MyBase.LoadViewState(viewState(0))
            _customFields = viewState(1)
            _customFieldData = viewState(2)
            Utilities.CreateDynamicCustomFieldPanelControls(_customFields, _customFieldData, lstCustomFields, Page)
        Else
            MyBase.LoadViewState(savedState)
        End If
    End Sub

    Private Sub SetControlUsabilityFromPermissions()
        With _currentUserPermission(_currentTableName)
            Dim shouldEnable = (.Edit AndAlso ddlCropTypes.SelectedIndex > 0) OrElse (.Create AndAlso ddlCropTypes.SelectedIndex = 0)
            tbxName.Enabled = shouldEnable
            btnSave.Enabled = shouldEnable
            btnDelete.Enabled = Not Guid.Parse(ddlCropTypes.SelectedValue).Equals(Guid.Empty) AndAlso .Edit AndAlso .Delete
        End With
    End Sub
End Class