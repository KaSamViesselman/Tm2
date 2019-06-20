Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class InventoryGroups
    Inherits System.Web.UI.Page

    Private _currentUser As KaUser
Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
    Private _currentTableName As String = KaBulkProductInventory.TABLE_NAME

#Region " Web Form Designer Generated Code "

    'This call is required by the Web Form Designer.
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()

    End Sub


    Private Sub Page_Init(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Init
        'CODEGEN: This method call is required by the Web Form Designer
        'Do not modify it using the code editor.
        InitializeComponent()
    End Sub

#End Region

#Region "Events"
    Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Response.Cache.SetCacheability(HttpCacheability.NoCache)
        _currentUser = Utilities.GetUser(Me)
        _currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Inventory")
        If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")

        If Not Page.IsPostBack Then
            SetTextboxMaxLengths()
            PopulateGroupsList()
            If Page.Request("InventoryGroupId") IsNot Nothing Then
                Try
                    ddlInventoryGroups.SelectedValue = Page.Request("InventoryGroupId")
                Catch ex As ArgumentOutOfRangeException
                    ddlInventoryGroups.SelectedIndex = 0
                End Try
            End If
            SetControlUsabilityFromPermissions()
            ddlInventoryGroups_SelectedIndexChanged(ddlInventoryGroups, New EventArgs())
            Utilities.ConfirmBox(Me.btnDelete, "Are you sure you want to delete this inventory group?")
            Utilities.SetFocus(tbxName, Me)
        End If
    End Sub

    Private Sub ddlInventoryGroups_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ddlInventoryGroups.SelectedIndexChanged
        lblStatus.Text = ""
        Dim groupId As Guid = Guid.Parse(ddlInventoryGroups.SelectedValue)
        PopulateGroupData(groupId)
        btnDelete.Enabled = Not groupId.Equals(Guid.Empty)
        SetControlUsabilityFromPermissions()
        Utilities.SetFocus(tbxName, Me)
    End Sub

    Private Sub btnSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSave.Click
        SaveGroup()
    End Sub

    Protected Sub btnDelete_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnDelete.Click
        lblStatus.Text = ""
        With New KaInventoryGroup(GetUserConnection(_currentUser.Id), Guid.Parse(ddlInventoryGroups.SelectedValue))
            .Deleted = True
            .SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
        End With
        PopulateGroupsList()
        ddlInventoryGroups.SelectedIndex = 0
        ddlInventoryGroups_SelectedIndexChanged(ddlInventoryGroups, New EventArgs())
        lblStatus.Text = "Selected inventory group deleted successfully"
        btnDelete.Enabled = False
    End Sub
#End Region

    Private Sub PopulateGroupsList()
        ddlInventoryGroups.Items.Clear() ' populate the inventory groups list
        If _currentUserPermission(_currentTableName).Create Then
            ddlInventoryGroups.Items.Add(New ListItem("Enter a new inventory group", Guid.Empty.ToString()))
        Else
			ddlInventoryGroups.Items.Add(New ListItem("Select an inventory group", Guid.Empty.ToString()))
		End If
        For Each r As KaInventoryGroup In KaInventoryGroup.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
            ddlInventoryGroups.Items.Add(New ListItem(r.Name, r.Id.ToString()))
        Next
        ddlInventoryGroups_SelectedIndexChanged(ddlInventoryGroups, New EventArgs())
    End Sub

    Private Sub PopulateGroupData(ByVal groupId As Guid)
        Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
        Dim invGroup As New KaInventoryGroup()
        invGroup.Id = groupId
        If invGroup.Id <> Guid.Empty Then
            invGroup.SqlSelect(connection)
        End If
        tbxName.Text = invGroup.Name
        cbxIsTotalizedGroup.Checked = invGroup.IsTotalizedGrouping
        Dim currentBulkProductList As New List(Of Guid)
        lstBulkProducts.Items.Clear()
        Dim bppsRdr As OleDbDataReader = Tm2Database.ExecuteReader(connection, String.Format("SELECT bulk_products.id, bulk_products.name " & _
                                            "FROM inventory_group_bulk_products " & _
                                            "INNER JOIN bulk_products ON inventory_group_bulk_products.bulk_product_id = bulk_products.id " & _
                                            "WHERE (inventory_group_bulk_products.deleted = 0) AND (bulk_products.deleted = 0) AND (inventory_group_bulk_products.inventory_group_id = {0}) " & _
                                            "ORDER BY name", Q(groupId)))
        Do While bppsRdr.Read()
            lstBulkProducts.Items.Add(New ListItem(bppsRdr.Item("name"), bppsRdr.Item("id").ToString()))
        Loop
        PopulateBulkProductList()
    End Sub

    Private Sub PopulateBulkProductList()
        Dim currentBulkProductList As New List(Of Guid)
        For Each currBulkProd As ListItem In lstBulkProducts.Items
            currentBulkProductList.Add(Guid.Parse(currBulkProd.Value))
        Next
        ddlBulkProduct.Items.Clear()
        ddlBulkProduct.Items.Add(New ListItem("", Guid.Empty.ToString()))
        For Each bulkProduct As KaBulkProduct In KaBulkProduct.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
            If Not currentBulkProductList.Contains(bulkProduct.Id) Then ddlBulkProduct.Items.Add(New ListItem(bulkProduct.Name, bulkProduct.Id.ToString()))
        Next
        If ddlBulkProduct.Items.Count > 0 Then ddlBulkProduct.SelectedIndex = 0
        ddlBulkProduct_SelectedIndexChanged(ddlBulkProduct, New EventArgs())
        Utilities.SortListControlList(lstBulkProducts, Utilities.ListControlSortBy.TextAsc)
    End Sub

    Private Sub SaveGroup()
        If ValidateFields() Then
            With New KaInventoryGroup()
                .Id = Guid.Parse(ddlInventoryGroups.SelectedValue)
                .Name = tbxName.Text.Trim
                .IsTotalizedGrouping = cbxIsTotalizedGroup.Checked
                Dim currentBulkProducts As New List(Of KaInventoryGroupBulkProduct)(.BulkProducts)
                .BulkProducts.Clear()
                For Each selectedBulkProduct As ListItem In lstBulkProducts.Items
                    Dim ingGrpId As Guid = Guid.Parse(selectedBulkProduct.Value)
                    Dim currBulkProd As KaInventoryGroupBulkProduct = Nothing
                    For Each bulkProd As KaInventoryGroupBulkProduct In currentBulkProducts
                        If bulkProd.BulkProductId.Equals(ingGrpId) Then
                            currBulkProd = bulkProd
                            Exit For
                        End If
                    Next
                    If currBulkProd Is Nothing Then
                        currBulkProd = New KaInventoryGroupBulkProduct
                        currBulkProd.BulkProductId = ingGrpId
                    End If
                    .BulkProducts.Add(currBulkProd)
                Next
                Dim statusText As String
                If .Id = Guid.Empty Then
                    statusText = "New inventory group added successfully"
                Else
                    statusText = "Selected inventory group updated successfully"
                End If
                .SqlUpdateInsertIfNotFound(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)

                PopulateGroupsList()
                ddlInventoryGroups.SelectedValue = .Id.ToString()
                ddlInventoryGroups_SelectedIndexChanged(ddlInventoryGroups, New EventArgs())
                lblStatus.Text = statusText
            End With
            btnDelete.Enabled = _currentUserPermission(_currentTableName).Delete
        End If
    End Sub

    Private Function ValidateFields() As Boolean
        Dim retval As Boolean = True
        If tbxName.Text.Trim().Length = 0 Then
            ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidName", Utilities.JsAlert("Please specify a name."))
            Return False
        End If
        If KaInventoryGroup.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND id<>" & Q(Guid.Parse(ddlInventoryGroups.SelectedValue)) & " AND name=" & Q(tbxName.Text), "").Count > 0 Then
            ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidNameExists", Utilities.JsAlert("An inventory group with the name """ & tbxName.Text & """ already exists. Please specify a unique name for this group."))
            Return False
        End If
        Return True
    End Function

    Private Sub SetTextboxMaxLengths()
        tbxName.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaInventoryGroup.TABLE_NAME, "name"))
    End Sub

    Protected Sub btnAddBulkProduct_Click(sender As Object, e As EventArgs) Handles btnAddBulkProduct.Click
        If ddlBulkProduct.SelectedIndex >= 0 Then lstBulkProducts.Items.Add(ddlBulkProduct.SelectedItem)
        PopulateBulkProductList()
    End Sub

    Protected Sub btnRemoveBulkProduct_Click(sender As Object, e As EventArgs) Handles btnRemoveBulkProduct.Click
        If lstBulkProducts.SelectedIndex >= 0 Then lstBulkProducts.Items.RemoveAt(lstBulkProducts.SelectedIndex)
        PopulateBulkProductList()
    End Sub

    Protected Sub ddlBulkProduct_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlBulkProduct.SelectedIndexChanged
        btnAddBulkProduct.Enabled = ddlBulkProduct.SelectedIndex >= 0
    End Sub

    Protected Sub lstBulkProducts_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lstBulkProducts.SelectedIndexChanged
        btnRemoveBulkProduct.Enabled = lstBulkProducts.SelectedIndex >= 0
    End Sub

    Private Sub SetControlUsabilityFromPermissions()
        With _currentUserPermission(_currentTableName)
            Dim shouldEnable = (.Edit AndAlso ddlInventoryGroups.SelectedIndex > 0) OrElse (.Create AndAlso ddlInventoryGroups.SelectedIndex = 0)
            btnSave.Enabled = shouldEnable
            btnAddBulkProduct.Enabled = shouldEnable
            btnRemoveBulkProduct.Enabled = shouldEnable
            cbxIsTotalizedGroup.Enabled = shouldEnable
            tbxName.Enabled = shouldEnable
            Dim value = Guid.Parse(ddlInventoryGroups.SelectedValue)
            btnDelete.Enabled = .Edit AndAlso .Delete AndAlso value <> Guid.Empty
        End With
    End Sub
End Class