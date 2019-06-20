Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class PanelBulkProductFillLimits
    Inherits System.Web.UI.Page
    Private _currentUser As KaUser
    Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
    Private _currentTableName As String = KaBulkProductPanelSettings.TABLE_NAME

    Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Response.Cache.SetCacheability(HttpCacheability.NoCache)
        _currentUser = Utilities.GetUser(Me)
        _currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "PanelBulkProductSettings")

        If Not _currentUserPermission(KaBulkProductPanelSettings.TABLE_NAME).Read Then Response.Redirect("Welcome.aspx")
        lblFillLimitStatus.Text = ""
        If Not Page.IsPostBack Then
            Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
            PopulateLocationList(connection)
            Try
                ddlFacilityFilter.SelectedValue = KaSetting.GetSetting(GetUserConnection(_currentUser.Id), "PanelsPage:" & _currentUser.Id.ToString & "/LastFacilityUsed", Guid.Empty.ToString())
            Catch ex As ArgumentOutOfRangeException
                ddlFacilityFilter.SelectedValue = Guid.Empty.ToString()
            End Try
            ddlFacilityFilter_SelectedIndexChanged(ddlFacilityFilter, New EventArgs())
            PopulatePanelFunctionList()
            PopulateBulkProductList(connection)
            PopulateUnitLists(connection)
            SetControlUsabilityFromPermissions()
            If Page.Request("PanelId") IsNot Nothing Then
                Try
                    ddlPanels.SelectedValue = Page.Request("PanelId")
                Catch ex As ArgumentOutOfRangeException
                    ddlPanels.SelectedIndex = 0
                End Try
            End If
            ddlPanels_SelectedIndexChanged(ddlPanels, New EventArgs())
            Utilities.ConfirmBox(Me.btnRemoveBulkProductFillLimit, "Are you sure you want to remove this bulk product fill limit for this panel?") ' confirmation box setup
        End If
    End Sub

    Private Sub PopulateLocationList(connection As OleDbConnection)
        ddlFacilityFilter.Items.Clear()
        ddlFacilityFilter.Items.Add(New ListItem("All facilities", Guid.Empty.ToString()))
        For Each locationInfo As KaLocation In KaLocation.GetAll(connection, "deleted=0", "name ASC")
            ddlFacilityFilter.Items.Add(New ListItem(locationInfo.Name, locationInfo.Id.ToString))
        Next
    End Sub

    Private Sub PopulatePanelList(connection As OleDbConnection)
        Dim currentPanelId As String = Guid.Empty.ToString()
        If ddlPanels.SelectedIndex >= 0 Then currentPanelId = ddlPanels.SelectedValue
        ddlPanels.Items.Clear()
        If _currentUserPermission(_currentTableName).Create Then
            ddlPanels.Items.Add(New ListItem("Enter a new panel", Guid.Empty.ToString()))
        Else
			ddlPanels.Items.Add(New ListItem("Select a panel", Guid.Empty.ToString()))
		End If
        ddlPanels.SelectedIndex = 0
        Dim facilityId As Guid = Guid.Empty
        Guid.TryParse(ddlFacilityFilter.SelectedValue, facilityId)
        For Each r As KaPanel In KaPanel.GetAll(connection, "deleted=0", "name ASC")
            If facilityId.Equals(Guid.Empty) OrElse facilityId.Equals(r.LocationId) Then ddlPanels.Items.Add(New ListItem(r.Name, r.Id.ToString()))
        Next
        Try
            ddlPanels.SelectedValue = currentPanelId
        Catch ex As ArgumentOutOfRangeException

        End Try
        ddlPanels_SelectedIndexChanged(ddlPanels, New EventArgs)
    End Sub

    Private Sub PopulatePanelFunctionList()
        ddlProductNumber.Items.Clear()
        ddlProductNumber.Items.Add(New ListItem("Specify from list", 0))
        Dim i As Integer = 1
        Do While i < 80
            ddlProductNumber.Items.Add(New ListItem("Product " & i, i))
            i += 1
        Loop
        ddlProductNumber.Items.Add(New ListItem("Hand-add", "99"))
    End Sub

    Private Sub PopulateUnitLists(connection As OleDbConnection)
        ddlFillLimitUnit.Items.Clear()

        For Each r As KaUnit In KaUnit.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
            If Not KaUnit.IsTime(r.BaseUnit) AndAlso Not r.BaseUnit = KaUnit.Unit.Pulses Then
                ddlFillLimitUnit.Items.Add(New ListItem(r.Name, r.Id.ToString()))
            End If
        Next
        Try
            ddlFillLimitUnit.SelectedValue = KaUnit.GetSystemDefaultMassUnitOfMeasure(connection, Nothing).ToString()
        Catch ex As ArgumentOutOfRangeException
            ddlFillLimitUnit.SelectedIndex = 0
        End Try
    End Sub

    Private Sub PopulateBulkProductList(connection As OleDbConnection)
        ddlBulkProducts.Items.Clear()
        ddlBulkProducts.Items.Add(New ListItem("", Guid.Empty.ToString()))
        Dim bulkProductIdsUsed As New List(Of Guid)
        Try
            For Each bpAssigned As KaBulkProductPanelFillLimitBulkProduct In GetCurrentFillLimit().BulkProducts
                If Not bulkProductIdsUsed.Contains(bpAssigned.BulkProductId) Then bulkProductIdsUsed.Add(bpAssigned.BulkProductId)
            Next
        Catch ex As Exception

        End Try
        For Each bulkProduct As KaBulkProduct In KaBulkProduct.GetAll(connection, "deleted=0", "name ASC")
            If Not bulkProductIdsUsed.Contains(bulkProduct.Id) AndAlso Not bulkProduct.IsFunction(connection, Nothing) Then ddlBulkProducts.Items.Add(New ListItem(bulkProduct.Name, bulkProduct.Id.ToString()))
        Next
    End Sub

    Private Sub PopulateFillLimits(ByVal connection As OleDbConnection, ByVal panelId As Guid)
        Dim currentSelectedId As String = Guid.Empty.ToString
        Dim fillLimit As KaBulkProductPanelFillLimit = GetCurrentFillLimit()
        If fillLimit IsNot Nothing Then currentSelectedId = fillLimit.Id.ToString()

        Dim fillLimits As List(Of KaBulkProductPanelFillLimit) = KaBulkProductPanelFillLimit.GetAllBulkProductPanelFillLimitsForPanel(connection, Nothing, panelId)
        PopulateFillLimits(connection, fillLimits)
        SetFillLimitSelection(currentSelectedId)
    End Sub

    Private Sub PopulateFillLimits(ByVal connection As OleDbConnection, ByVal fillLimits As List(Of KaBulkProductPanelFillLimit))
        lstBulkProductFillLimit.Items.Clear()
        Dim bulkProducts As New Dictionary(Of Guid, KaBulkProduct)
        Dim units As New Dictionary(Of Guid, KaUnit)
        For Each fillLimit As KaBulkProductPanelFillLimit In fillLimits
            InsertFillLimitToList(connection, fillLimit, lstBulkProductFillLimit.Items.Count, bulkProducts, units)
        Next
    End Sub

    Private Sub SetFillLimitSelection(currentSelectedId As String)
        Dim selectedIndex As Integer = -1
        For i = 0 To lstBulkProductFillLimit.Items.Count - 1
            If KaBulkProductPanelFillLimit.FromXml(lstBulkProductFillLimit.Items(i).Value).Id.ToString() = currentSelectedId Then
                selectedIndex = i
                Exit For
            End If
        Next
        lstBulkProductFillLimit.SelectedIndex = selectedIndex
        lstBulkProductFillLimit_SelectedIndexChanged(lstBulkProductFillLimit, Nothing)
    End Sub

    Private Function ConvertFillLimitListToObjects() As List(Of KaBulkProductPanelFillLimit)
        Dim fillLimits As New List(Of KaBulkProductPanelFillLimit)
        For Each fillLimit As ListItem In lstBulkProductFillLimit.Items
            Try
                fillLimits.Add(KaBulkProductPanelFillLimit.FromXml(fillLimit.Value))
            Catch ex As Exception
            End Try
        Next
        Return fillLimits
    End Function

    Protected Sub ddlPanels_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles ddlPanels.SelectedIndexChanged
        Dim panelId As Guid = Guid.Parse(ddlPanels.SelectedValue)
        PopulateFillLimits(GetUserConnection(_currentUser.Id), panelId)
        SetControlUsabilityFromPermissions()
    End Sub

    Protected Sub btnSaveFillLimit_Click(sender As Object, e As EventArgs) Handles btnSaveFillLimit.Click
        Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
        Dim panelId As Guid = Guid.Parse(ddlPanels.SelectedValue)
        Dim fillLimitList As List(Of KaBulkProductPanelFillLimit) = ConvertFillLimitListToObjects()
        Dim fillLimit As KaBulkProductPanelFillLimit = GetCurrentFillLimit()
        Dim i As Integer = 0
        If fillLimit IsNot Nothing Then
            If Not Double.TryParse(tbxFillLimit.Text, fillLimit.FillLimit) OrElse fillLimit.FillLimit <= 0 Then
                ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), "InvalidCharFillLimit", Utilities.JsAlert("Please enter a numeric value for the fill limit."), False) : Exit Sub
            ElseIf Not Guid.TryParse(ddlFillLimitUnit.SelectedValue, fillLimit.FillLimitUnitId) Then
                ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), "InvalidFillUnit", Utilities.JsAlert("Please enter a unit of measure for the fill limit."), False) : Exit Sub
            End If
            Integer.TryParse(ddlProductNumber.SelectedValue, fillLimit.ProductNumber)
            fillLimit.BulkProducts.Clear()
            For Each bp As ListItem In lstBulkProductsAssignedToFillLimit.Items
                Dim bpAssigned As KaBulkProductPanelFillLimitBulkProduct = KaBulkProductPanelFillLimitBulkProduct.FromXml(bp.Value)
                If Not fillLimit.BulkProductsContainBulkProductId(bpAssigned.BulkProductId) Then fillLimit.BulkProducts.Add(bpAssigned)
            Next
            fillLimit.PanelId = panelId
            i = 0
            Do While i < fillLimitList.Count
                If fillLimitList(i).Id.Equals(fillLimit.Id) Then
                    fillLimitList.RemoveAt(i)
                Else
                    i += 1
                End If
            Loop
            If fillLimit IsNot Nothing Then fillLimitList.Add(fillLimit)
        End If
        KaSetting.WriteSetting(connection, String.Format(KaBulkProductPanelFillLimit.SN_BULK_PRODUCT_PANEL_FILL_LIMITS, panelId.ToString()), Tm2Database.ToXml(fillLimitList, GetType(List(Of KaBulkProductPanelFillLimit))))
        lblFillLimitStatus.Text = "Fill limits saved"
        PopulateFillLimits(connection, fillLimitList)
    End Sub

    Protected Sub btnAddBulkProductFillLimit_Click(sender As Object, e As EventArgs) Handles btnAddBulkProductFillLimit.Click
        Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
        Dim panelId As Guid
        Guid.TryParse(ddlPanels.SelectedValue, panelId)
        Dim list As List(Of KaBulkProductPanelFillLimit) = ConvertFillLimitListToObjects()
        Dim newFillLimit As New KaBulkProductPanelFillLimit
        With newFillLimit
            .Id = Guid.NewGuid
            .PanelId = panelId
            .FillLimit = 0
            .FillLimitUnitId = KaUnit.GetSystemDefaultMassUnitOfMeasure(connection, Nothing)
        End With
        list.Add(newFillLimit)

        PopulateFillLimits(connection, list)
        SetFillLimitSelection(newFillLimit.Id.ToString())
    End Sub

    Protected Sub btnRemoveBulkProductFillLimit_Click(sender As Object, e As EventArgs) Handles btnRemoveBulkProductFillLimit.Click
        If lstBulkProductFillLimit.SelectedIndex >= 0 AndAlso lstBulkProductFillLimit.SelectedIndex < lstBulkProductFillLimit.Items.Count Then
            Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
            Dim list As List(Of KaBulkProductPanelFillLimit) = ConvertFillLimitListToObjects()
            Dim index As Integer = lstBulkProductFillLimit.SelectedIndex
            list.RemoveAt(index)
            PopulateFillLimits(connection, list)
            index -= 1
            index = Math.Max(0, Math.Min(list.Count - 1, index))
            If index >= 0 AndAlso index < list.Count Then SetFillLimitSelection(list(index).Id.ToString())
        End If
    End Sub

    Protected Sub lstBulkProductFillLimit_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lstBulkProductFillLimit.SelectedIndexChanged
        If lstBulkProductFillLimit.SelectedIndex >= 0 Then
            Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
            pnlFillLimitSettings.Visible = True
            PopulateFillLimitInformation()
            btnRemoveBulkProductFillLimit.Enabled = True
        Else
            pnlFillLimitSettings.Visible = False
            btnRemoveBulkProductFillLimit.Enabled = False
        End If
    End Sub

    Private Sub PopulateFillLimitInformation()
        Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
        Dim fillLimitInfo As KaBulkProductPanelFillLimit = GetCurrentFillLimit()
        If fillLimitInfo IsNot Nothing Then
            tbxFillLimit.Text = fillLimitInfo.FillLimit
            ddlFillLimitUnit.SelectedValue = fillLimitInfo.FillLimitUnitId.ToString()
            lstBulkProductsAssignedToFillLimit.Items.Clear()
            fillLimitInfo.BulkProducts.Sort(AddressOf KaBulkProductPanelFillLimitBulkProduct.CompareByString)

            For Each bpAssigned As KaBulkProductPanelFillLimitBulkProduct In fillLimitInfo.BulkProducts
                Try
                    lstBulkProductsAssignedToFillLimit.Items.Add(New ListItem(New KaBulkProduct(connection, bpAssigned.BulkProductId).Name, bpAssigned.ToXml()))
                Catch ex As RecordNotFoundException

                End Try
            Next
            ddlProductNumber.SelectedValue = fillLimitInfo.ProductNumber.ToString()
            ddlProductNumber_SelectedIndexChanged(ddlProductNumber, Nothing)
            PopulateBulkProductList(connection)
        End If
    End Sub

    Protected Sub btnAddBulkProductToFillLimit_Click(sender As Object, e As EventArgs) Handles btnAddBulkProductToFillLimit.Click
        Dim fillLimit As KaBulkProductPanelFillLimit = GetCurrentFillLimit()
        If fillLimit IsNot Nothing AndAlso ddlBulkProducts.SelectedIndex >= 0 Then
            Dim bulkProductId As Guid = Guid.Parse(ddlBulkProducts.SelectedValue)
            fillLimit.BulkProducts.Add(New KaBulkProductPanelFillLimitBulkProduct(bulkProductId))
            Dim index As Integer = lstBulkProductFillLimit.SelectedIndex
            lstBulkProductFillLimit.Items.RemoveAt(index)
            InsertFillLimitToList(GetUserConnection(_currentUser.Id), fillLimit, index, New Dictionary(Of Guid, KaBulkProduct), New Dictionary(Of Guid, KaUnit))
            SetFillLimitSelection(fillLimit.Id.ToString())
        End If
    End Sub

    Protected Sub btnRemoveBulkProductFromFillLimit_Click(sender As Object, e As EventArgs) Handles btnRemoveBulkProductFromFillLimit.Click
        Dim fillLimit As KaBulkProductPanelFillLimit = GetCurrentFillLimit()
        If fillLimit IsNot Nothing AndAlso lstBulkProductsAssignedToFillLimit.SelectedIndex >= 0 Then
            Dim bpToRemove As KaBulkProductPanelFillLimitBulkProduct = KaBulkProductPanelFillLimitBulkProduct.FromXml(lstBulkProductsAssignedToFillLimit.Items(lstBulkProductsAssignedToFillLimit.SelectedIndex).Value)
            Dim i As Integer = 0
            Do While i < fillLimit.BulkProducts.Count
                If fillLimit.BulkProducts(i).BulkProductId.Equals(bpToRemove.BulkProductId) Then
                    fillLimit.BulkProducts.RemoveAt(i)
                Else
                    i += 1
                End If
            Loop
            Dim index As Integer = lstBulkProductFillLimit.SelectedIndex
            lstBulkProductFillLimit.Items.RemoveAt(index)
            InsertFillLimitToList(GetUserConnection(_currentUser.Id), fillLimit, index, New Dictionary(Of Guid, KaBulkProduct), New Dictionary(Of Guid, KaUnit))
            SetFillLimitSelection(fillLimit.Id.ToString())
        End If
    End Sub

    Private Sub SetControlUsabilityFromPermissions()
        With _currentUserPermission(KaBulkProductPanelSettings.TABLE_NAME)
            Dim shouldEnable = (.Edit AndAlso ddlPanels.SelectedIndex > 0)
            pnlBulkProductFillLimits.Enabled = shouldEnable
            btnSaveFillLimit.Enabled = shouldEnable
        End With
    End Sub

    'Private Sub tbxFillLimit_TextChanged(sender As Object, e As EventArgs) Handles tbxFillLimit.TextChanged
    '	Dim fillLimit As KaBulkProductPanelFillLimit = GetCurrentFillLimit()
    '	If fillLimit IsNot Nothing AndAlso Double.TryParse(tbxFillLimit.Text, fillLimit.FillLimit) Then
    '		Dim index As Integer = lstBulkProductFillLimit.SelectedIndex
    '		lstBulkProductFillLimit.Items.RemoveAt(index)
    '		InsertFillLimitToList(GetUserConnection(_currentUser.Id), fillLimit, index, New Dictionary(Of Guid, KaBulkProduct), New Dictionary(Of Guid, KaUnit))
    '		SetFillLimitSelection(fillLimit.Id.ToString())
    '		tbxFillLimit.Focus()
    '	End If
    'End Sub

    Private Function GetCurrentFillLimit() As KaBulkProductPanelFillLimit
        Try
            Return KaBulkProductPanelFillLimit.FromXml(lstBulkProductFillLimit.SelectedItem.Value)
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    Private Sub InsertFillLimitToList(connection As OleDbConnection, fillLimit As KaBulkProductPanelFillLimit, index As Integer, bulkProducts As Dictionary(Of Guid, KaBulkProduct), units As Dictionary(Of Guid, KaUnit))
        Dim li As New ListItem(fillLimit.ToString(connection, Nothing, bulkProducts, units), fillLimit.ToXml())
        li.Attributes("title") = li.Text
        lstBulkProductFillLimit.Items.Insert(index, li)
    End Sub

    Private Sub lstbulkProductsAssignedToFillLimit_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lstBulkProductsAssignedToFillLimit.SelectedIndexChanged
        btnRemoveBulkProductFromFillLimit.Enabled = lstBulkProductsAssignedToFillLimit.SelectedIndex >= 0
    End Sub

    Protected Sub ddlBulkProducts_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlBulkProducts.SelectedIndexChanged
        btnAddBulkProductToFillLimit.Enabled = ddlBulkProducts.SelectedIndex >= 0 AndAlso lstBulkProductFillLimit.SelectedIndex >= 0
    End Sub

    Private Sub ddlProductNumber_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlProductNumber.SelectedIndexChanged
        pnlBulkProductAssigned.Visible = (ddlProductNumber.SelectedIndex = 0)
    End Sub

	Protected Sub ddlFacilityFilter_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlFacilityFilter.SelectedIndexChanged
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		PopulatePanelList(connection)
		KaSetting.WriteSetting(connection, "PanelsPage:" & _currentUser.Id.ToString & "/LastFacilityUsed", ddlFacilityFilter.SelectedValue)
	End Sub
	Protected Sub ScriptManager1_AsyncPostBackError(ByVal sender As Object, ByVal e As System.Web.UI.AsyncPostBackErrorEventArgs)
		ToolkitScriptManager1.AsyncPostBackErrorMessage = e.Exception.Message
	End Sub

	'Protected Sub ddlFillLimitUnit_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlFillLimitUnit.SelectedIndexChanged
	'	Dim fillLimit As KaBulkProductPanelFillLimit = GetCurrentFillLimit()
	'	If fillLimit IsNot Nothing AndAlso Guid.TryParse(ddlFillLimitUnit.SelectedValue, fillLimit.FillLimitUnitId) Then
	'		Dim index As Integer = lstBulkProductFillLimit.SelectedIndex
	'		lstBulkProductFillLimit.Items.RemoveAt(index)
	'		InsertFillLimitToList(GetUserConnection(_currentUser.Id), fillLimit, index, New Dictionary(Of Guid, KaBulkProduct), New Dictionary(Of Guid, KaUnit))
	'		SetFillLimitSelection(fillLimit.Id.ToString())
	'		ddlFillLimitUnit.Focus()
	'	End If
	'End Sub
End Class