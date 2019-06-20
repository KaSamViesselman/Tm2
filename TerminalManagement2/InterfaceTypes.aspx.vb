Imports KahlerAutomation.KaTm2Database

Public Class InterfaceTypes : Inherits System.Web.UI.Page

    Private _currentUser As KaUser
    Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
    Private _currentTableName As String = KaInterface.TABLE_NAME
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Response.Cache.SetCacheability(HttpCacheability.NoCache)
        _currentUser = Utilities.GetUser(Me)
        _currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Interfaces")
        If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")

        If Not Page.IsPostBack Then
            SetTextboxMaxLengths()
            PopulateInterfaceTypeList()
            SetControlUsabilityFromPermissions()
            If Page.Request("InterfaceTypeId") IsNot Nothing Then
                Try
                    ddlInterfaceTypes.SelectedValue = Page.Request("InterfaceTypeId")
                Catch ex As ArgumentOutOfRangeException
                    ddlInterfaceTypes.SelectedIndex = 0
                End Try
            End If
            ddlInterfaceTypes_SelectedIndexChanged(ddlInterfaceTypes, New EventArgs())
            Utilities.ConfirmBox(Me.btnDelete, "Are you sure you want to delete this interface type?") ' Delete confirmation box setup
            Utilities.SetFocus(tbxName, Me) ' set focus to the first textbox on the page
        End If
    End Sub

    Private Sub PopulateInterfaceTypeList()
        ddlInterfaceTypes.Items.Clear()
        If _currentUserPermission(_currentTableName).Create Then
            ddlInterfaceTypes.Items.Add(New ListItem("Enter a new interface type", Guid.Empty.ToString))
        Else
            ddlInterfaceTypes.Items.Add(New ListItem("Select an interface type", Guid.Empty.ToString))
        End If
        For Each type As KaInterfaceTypes In KaInterfaceTypes.GetAll(GetUserConnection(_currentUser.Id), "deleted = " & Q(False), "name asc")
            ddlInterfaceTypes.Items.Add(New ListItem(type.Name, type.Id.ToString))
        Next
    End Sub

    Private Sub PopulateInterfaceTypeInformation(ByVal typeId As Guid)
        With New KaInterfaceTypes()
            .Id = typeId
            If .Id <> Guid.Empty Then
                .SqlSelect(GetUserConnection(_currentUser.Id))
                btnDelete.Enabled = _currentUserPermission(_currentTableName).Delete
            Else
                btnDelete.Enabled = False
            End If
            tbxName.Text = .Name
            tbxConfigUrl.Text = .ConfigUrl
            chkShowApplicatorSetup.Checked = .ShowApplicatorInterface
            chkShowBranchSetup.Checked = .ShowBranchInterface
            chkShowBulkProductSetup.Checked = .ShowBulkProductInterface
            chkShowFacilitySetup.Checked = .ShowLocationInterface
            chkShowOwnerSetup.Checked = .ShowOwnerInterface
            chkShowTransportTypeSetup.Checked = .ShowTransportTypeInterface
            chkShowUnitSetup.Checked = .ShowUnitsInterface
            chkShowDriverSetup.Checked = .ShowDriversInterface
            chkShowCarrierSetup.Checked = .ShowCarrierInterface
            chkShowTransportSetup.Checked = .ShowTransportInterface
            chkShowTankSetup.Checked = .ShowTanksInterface
            chkShowInterfaceExchangeUnit.Checked = .ShowInterfaceExchangeUnit
            chkUseInterfaceUnitAsOrderItemUnit.Checked = .UseInterfaceUnitAsOrderItemUnit
            chkSplitImportedProductsIntoBulkProductOrderItems.Checked = .SplitProductIntoComponents
            chkAllowOrderProductChange.Checked = .AllowOrderProductChange
            chkAllowOrderProductGroupingChange.Checked = .AllowOrderItemGroupingChange
            chkAllowOrderCustomerChange.Checked = .AllowOrderCustomerChange
            chkAllowOrderStatusChangeTicketsExist.Checked = .AllowOrderStatusChangeTicketsExist
        End With
    End Sub

    Protected Sub btnSave_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnSave.Click
        lblStatus.Text = ""
        If ValidateFields() Then
            With New KaInterfaceTypes()
                .Id = Guid.Parse(ddlInterfaceTypes.SelectedValue)
                .Name = tbxName.Text.Trim
                .ConfigUrl = tbxConfigUrl.Text.Trim
                .ShowApplicatorInterface = chkShowApplicatorSetup.Checked
                .ShowBranchInterface = chkShowBranchSetup.Checked
                .ShowBulkProductInterface = chkShowBulkProductSetup.Checked
                .ShowLocationInterface = chkShowFacilitySetup.Checked
                .ShowOwnerInterface = chkShowOwnerSetup.Checked
                .ShowTransportTypeInterface = chkShowTransportTypeSetup.Checked
                .ShowUnitsInterface = chkShowUnitSetup.Checked
                .ShowDriversInterface = chkShowDriverSetup.Checked
                .ShowCarrierInterface = chkShowCarrierSetup.Checked
                .ShowTransportInterface = chkShowTransportSetup.Checked
                .ShowTanksInterface = chkShowTankSetup.Checked
                .ShowInterfaceExchangeUnit = chkShowInterfaceExchangeUnit.Checked
                .UseInterfaceUnitAsOrderItemUnit = chkUseInterfaceUnitAsOrderItemUnit.Checked
                .SplitProductIntoComponents = chkSplitImportedProductsIntoBulkProductOrderItems.Checked
                .AllowOrderProductChange = chkAllowOrderProductChange.Checked
                .AllowOrderItemGroupingChange = chkAllowOrderProductGroupingChange.Checked
                .AllowOrderCustomerChange = chkAllowOrderCustomerChange.Checked
                .AllowOrderStatusChangeTicketsExist = chkAllowOrderStatusChangeTicketsExist.Checked

                Dim status As String = ""
                If .Id <> Guid.Empty Then
                    .SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                    status = "Interface type updated successfully."
                Else
                    .SqlInsert(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                    status = "Interface type added successfully."
                End If
                PopulateInterfaceTypeList()
                ddlInterfaceTypes.SelectedValue = .Id.ToString()
                btnDelete.Enabled = _currentUserPermission(_currentTableName).Delete
                lblStatus.Text = status
            End With
        End If
    End Sub

    Private Function ValidateFields() As Boolean
        If tbxName.Text.Trim = "" Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidName", Utilities.JsAlert("Name must be specified.")) : Return False
        If tbxConfigUrl.Text.Trim = "" Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidUrl", Utilities.JsAlert("URL must be specified.")) : Return False

        Dim allInterfaceTypes As ArrayList = KaInterfaceTypes.GetAll(GetUserConnection(_currentUser.Id), "name = " & Q(tbxName.Text.Trim) & " and deleted = 0", "")
        If allInterfaceTypes.Count > 0 Then
            Dim tempInterfaceType As KaInterfaceTypes = allInterfaceTypes.Item(0)
            If tempInterfaceType.Id <> Guid.Parse(ddlInterfaceTypes.SelectedValue) Then
                ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidNameExists", Utilities.JsAlert("An Interface Type with name " & tbxName.Text.Trim & " already exists.  Name must be unique"))
                Return False
            End If
        End If
        Return True
    End Function

    Protected Sub btnDelete_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnDelete.Click
        Dim id As Guid = Guid.Parse(ddlInterfaceTypes.SelectedValue)
        Dim interfaces As ArrayList = KaInterface.GetAll(GetUserConnection(_currentUser.Id), String.Format("deleted=0 AND interface_type_id='{0}'", id.ToString()), "name ASC")
        If interfaces.Count = 0 Then
            With New KaInterfaceTypes(GetUserConnection(_currentUser.Id), id)
                .Deleted = True
                .SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
                lblStatus.Text = "Interface type deleted successfully."
            End With
            PopulateInterfaceTypeList()
            PopulateInterfaceTypeInformation(Guid.Empty)
        Else
            Dim list As String = ""
            For Each r As KaInterface In interfaces
                list &= IIf(list.Length > 0, ", ", "") & r.Name
            Next
            ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidTypeUsed", Utilities.JsAlert(String.Format("This interface type cannot be deleted since it is referenced by the following interfaces: {0}", list)))
        End If
    End Sub

    Private Sub ddlInterfaceTypes_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles ddlInterfaceTypes.SelectedIndexChanged
        PopulateInterfaceTypeInformation(Guid.Parse(ddlInterfaceTypes.SelectedValue))
        SetControlUsabilityFromPermissions()
    End Sub

    Private Sub SetTextboxMaxLengths()
        tbxConfigUrl.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaInterfaceTypes.TABLE_NAME, KaInterfaceTypes.FN_CONFIG_URL))
        tbxName.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaInterfaceTypes.TABLE_NAME, KaInterfaceTypes.FN_NAME))
    End Sub
    Private Sub SetControlUsabilityFromPermissions()
        With _currentUserPermission(_currentTableName)
            Dim shouldEnable = (.Edit AndAlso ddlInterfaceTypes.SelectedIndex > 0) OrElse (.Create AndAlso ddlInterfaceTypes.SelectedIndex = 0)
            pnlMain.Enabled = shouldEnable
            btnSave.Enabled = shouldEnable
            Dim value = Guid.Parse(ddlInterfaceTypes.SelectedValue)
            btnDelete.Enabled = .Edit AndAlso .Delete AndAlso value <> Guid.Empty
        End With
    End Sub
End Class