Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class Products
	Inherits System.Web.UI.Page

	Private _currentUser As KaUser
	Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
	Private _currentTableName As String = KaProduct.TABLE_NAME
	Private _customFields As New List(Of KaCustomField)
	Private _customFieldData As New List(Of KaCustomFieldData)

#Region "Events"
	Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load, Me.Load
		Response.Cache.SetCacheability(HttpCacheability.NoCache)
		_currentUser = Utilities.GetUser(Me)
		_currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Products")
		If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")

		If Not Page.IsPostBack Then
			SetTextboxMaxLengths()

			_customFields.Clear()
			For Each customField As KaCustomField In KaCustomField.GetAll(GetUserConnection(_currentUser.Id), String.Format("deleted=0 AND {0} = {1}", KaCustomField.FN_TABLE_NAME, Q(KaProduct.TABLE_NAME)), KaCustomField.FN_FIELD_NAME)
				_customFields.Add(customField)
			Next

			PopulateProductList()
			PopulateOwnerList()
			PopulateUnitList()
			PopulateInterfaceList()
			PopulateBulkProductList()
			PopulateLocationList(Guid.Empty)
			PopulateFacilityForSplitProductDropdownLists()
			PopulateProductGroupList()
			SetControlUsabilityFromPermissions()
			If Page.Request("ProductId") IsNot Nothing Then
				Try
					ddlProducts.SelectedValue = Page.Request("ProductId")
					ddlProducts_SelectedIndexChanged(ddlProducts, New EventArgs())
				Catch ex As ArgumentOutOfRangeException
					ddlProducts.SelectedIndex = 0
					ddlProducts_SelectedIndexChanged(ddlProducts, New EventArgs())
					ddlOwner.SelectedValue = _currentUser.OwnerId.ToString()
				End Try
			Else
				ddlProducts.SelectedIndex = 0
				ddlProducts_SelectedIndexChanged(ddlProducts, New EventArgs())
				ddlOwner.SelectedValue = _currentUser.OwnerId.ToString()
			End If

			Utilities.ConfirmBox(Me.btnDelete, "Are you sure you want to delete this product?") ' Delete confirmation box setup
			' Utilities.ConfirmBox(Me.btnBulkRemove, "Are you sure you want to remove this product?")
			Utilities.SetFocus(tbxName, Me) ' set focus to the first textbox on the page
		End If
		PopulateBulkProductTable()
	End Sub

	Private Sub ddlProducts_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ddlProducts.SelectedIndexChanged
		PopulateUnitList()
		SetControlUsabilityFromPermissions()
		lblStatus.Text = ""
		btnDelete.Enabled = PopulateProductInformation(Guid.Parse(ddlProducts.SelectedValue)) AndAlso _currentUserPermission(_currentTableName).Delete
	End Sub

	Protected Sub ddlProductInterface_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles ddlProductInterface.SelectedIndexChanged
		lblStatus.Text = ""
		btnRemoveInterface.Enabled = PopulateProductInterfaceInformation(Guid.Parse(ddlProductInterface.SelectedValue))
	End Sub

	Private Sub btnSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSave.Click
		Dim productId As Guid = Guid.Parse(ddlProducts.SelectedValue)
		If SaveProductInformation(productId) Then
			PopulateProductList()
			ddlProducts.SelectedValue = productId.ToString()
			btnDelete.Enabled = PopulateProductInformation(productId) AndAlso _currentUserPermission(_currentTableName).Delete
		End If
	End Sub

	Private Sub btnDelete_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDelete.Click
		If DeleteProductInformation(Guid.Parse(ddlProducts.SelectedValue)) Then
			PopulateProductList()
			ddlProducts.SelectedValue = Guid.Empty.ToString()
			ddlLocation.SelectedIndex = 0
			btnDelete.Enabled = PopulateProductInformation(Guid.Empty) AndAlso _currentUserPermission(_currentTableName).Delete
		End If
	End Sub

	Protected Sub btnSaveInterface_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnSaveInterface.Click
		SaveProductInterface()
	End Sub

	Protected Sub btnRemoveInterface_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnRemoveInterface.Click
		RemoveProductInterface()
	End Sub

	Protected Sub ddlLocation_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles ddlLocation.SelectedIndexChanged
		UpdateBulkProductControls()
	End Sub

	Protected Sub btnAddBulkProduct_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnAddBulkProduct.Click
		Dim bulkProductId As Guid = Guid.Parse(ddlBulkProduct.SelectedValue)
		If Not Utilities.IsBulkProductParameterlessFunction(bulkProductId, _currentUser.Id) Then
			Dim bulkProductIsTimedFunction As Boolean = Utilities.IsBulkProductTimedFunction(bulkProductId, _currentUser.Id)
			If Not IsNumeric(tbxPercent.Text) Then
				ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidCharPercent", Utilities.JsAlert("Please enter a numeric value for the " & IIf(bulkProductIsTimedFunction, "function time", "percent of total")))
				Exit Sub
			ElseIf Double.Parse(tbxPercent.Text) <= 0 AndAlso (bulkProductIsTimedFunction OrElse Double.Parse(tbxPercent.Text) <= 100) Then
				ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidPercentTotal", Utilities.JsAlert("Please enter a " & IIf(bulkProductIsTimedFunction, "time", "percent") & " value greater than zero" & IIf(bulkProductIsTimedFunction, "", " and less than or equal to 100")))
				Exit Sub
			End If
		End If
		Dim l As New BulkProductList(ddlLocation.SelectedValue)
		l.BulkProducts.Add(New BulkProduct(bulkProductId, tbxPercent.Text))
		ddlLocation.SelectedItem.Value = l.ToString()
		btnAddBulkProduct.Enabled = False
		btnRemoveBulkProduct.Enabled = True
		btnUpdateBulkProductPercent.Enabled = True
		PopulateBulkProductTable()
	End Sub

	Protected Sub btnRemoveBulkProduct_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnRemoveBulkProduct.Click
		Dim l As New BulkProductList(ddlLocation.SelectedValue)
		l.RemoveBulkProduct(Guid.Parse(ddlBulkProduct.SelectedValue))
		ddlLocation.SelectedItem.Value = l.ToString()
		UpdateBulkProductControls()
	End Sub

	Protected Sub btnUpdateBulkProductPercent_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnUpdateBulkProductPercent.Click
		Dim bulkProductId As Guid = Guid.Parse(ddlBulkProduct.SelectedValue)
		If Not Utilities.IsBulkProductParameterlessFunction(bulkProductId, _currentUser.Id) Then
			Dim bulkProductIsTimedFunction As Boolean = Utilities.IsBulkProductTimedFunction(bulkProductId, _currentUser.Id)
			If Not IsNumeric(tbxPercent.Text) Then
				ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidCharPercent", Utilities.JsAlert("Please enter a numeric value for the " & IIf(bulkProductIsTimedFunction, "function time", "percent of total")))
				Exit Sub
			ElseIf Double.Parse(tbxPercent.Text) <= 0 AndAlso (bulkProductIsTimedFunction OrElse Double.Parse(tbxPercent.Text) <= 100) Then
				ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidPercentTotal", Utilities.JsAlert("Please enter a " & IIf(bulkProductIsTimedFunction, "time", "percent") & " value greater than zero" & IIf(bulkProductIsTimedFunction, "", " and less than or equal to 100")))
				Exit Sub
			End If
		End If
		Dim l As New BulkProductList(ddlLocation.SelectedValue)
		l.UpdateBulkProductPercentage(Guid.Parse(ddlBulkProduct.SelectedValue), tbxPercent.Text)
		ddlLocation.SelectedItem.Value = l.ToString()
		PopulateBulkProductTable()
	End Sub

	Protected Sub ddlBulkProduct_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles ddlBulkProduct.SelectedIndexChanged
		Dim l As New BulkProductList(ddlLocation.SelectedValue)
		Dim bulkProductId As Guid = Guid.Parse(ddlBulkProduct.SelectedValue)
		Dim inList As Boolean = l.IsBulkProductInList(bulkProductId)
		Dim percent As Double = l.GetBulkProductPercentage(bulkProductId)
		btnAddBulkProduct.Enabled = l.LocationId <> Guid.Empty AndAlso bulkProductId <> Guid.Empty AndAlso Not inList
		btnRemoveBulkProduct.Enabled = l.LocationId <> Guid.Empty AndAlso bulkProductId <> Guid.Empty AndAlso inList
		btnUpdateBulkProductPercent.Enabled = btnRemoveBulkProduct.Enabled
		tbxPercent.Text = percent
		PopulateBulkProductTable()
		If Utilities.IsBulkProductParameterlessFunction(bulkProductId, _currentUser.Id) Then
			lblPortionUnit.Text = ""
			tbxPercent.Visible = False
			btnUpdateBulkProductPercent.Visible = False
		ElseIf Utilities.IsBulkProductTimedFunction(bulkProductId, _currentUser.Id) Then
			lblPortionUnit.Text = "Seconds"
			tbxPercent.Visible = True
			btnUpdateBulkProductPercent.Visible = True
			btnUpdateBulkProductPercent.Text = "Update Time"
		Else
			lblPortionUnit.Text = "Percent of total"
			tbxPercent.Visible = True
			btnUpdateBulkProductPercent.Visible = True
			btnUpdateBulkProductPercent.Text = "Update %"
		End If
	End Sub
#End Region

#Region "Interface"
	Private Sub PopulateInterfaceList()
		ddlInterface.Items.Clear()
		ddlInterface.Items.Add(New ListItem("", Guid.Empty.ToString()))
		For Each i As KaInterface In KaInterface.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
			ddlInterface.Items.Add(New ListItem(i.Name, i.Id.ToString()))
		Next
	End Sub

	Private Sub PopulateProductInterfaceList(ByVal productId As Guid)
		ddlProductInterface.Items.Clear()
		ddlProductInterface.Items.Add(New ListItem("Add an interface", Guid.Empty.ToString()))
		For Each i As KaProductInterfaceSettings In KaProductInterfaceSettings.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND product_id=" & Q(productId), "")
			Dim inter As KaInterface = New KaInterface(GetUserConnection(_currentUser.Id), i.InterfaceId)
			Dim li As ListItem = New ListItem
			li.Text = inter.Name & " (" & i.CrossReference & ")"
			li.Value = i.Id.ToString
			ddlProductInterface.Items.Add(li)
		Next
	End Sub

	Private Function PopulateProductInterfaceInformation(ByVal prodInterfaceId As Guid) As Boolean
		Dim retval As Boolean = False
		If prodInterfaceId <> Guid.Empty Then
			Dim prodInterfaceSet As KaProductInterfaceSettings = New KaProductInterfaceSettings(GetUserConnection(_currentUser.Id), prodInterfaceId)
			ddlInterface.SelectedValue = prodInterfaceSet.InterfaceId.ToString
			tbxInterfaceCrossReference.Text = prodInterfaceSet.CrossReference
			ddlInterfaceUnit.SelectedValue = prodInterfaceSet.UnitId.ToString
			If prodInterfaceSet.OrderItemUnitId.Equals(Guid.Empty) Then
				ddlInterfaceOrderItemUnit.SelectedValue = prodInterfaceSet.UnitId.ToString
			Else
				ddlInterfaceOrderItemUnit.SelectedValue = prodInterfaceSet.OrderItemUnitId.ToString
			End If
			Try
				ddlSplitProductFormulationFacility.SelectedValue = prodInterfaceSet.ProductSplitFormulationFacilityId.ToString()
			Catch ex As Exception
				ddlSplitProductFormulationFacility.SelectedIndex = 0
			End Try
			tbxPriority.Text = prodInterfaceSet.ProductPriority
			chkDefaultSetting.Checked = prodInterfaceSet.DefaultSetting
			chkExportOnly.Checked = prodInterfaceSet.ExportOnly
			retval = True
		Else
			ddlInterface.SelectedIndex = 0
			tbxInterfaceCrossReference.Text = ""
			Try
				Dim defaultMassUnit As String = KaUnit.GetSystemDefaultMassUnitOfMeasure(GetUserConnection(_currentUser.Id), Nothing).ToString()
				ddlInterfaceUnit.SelectedValue = defaultMassUnit
				ddlInterfaceOrderItemUnit.SelectedIndex = defaultMassUnit
			Catch ex As Exception
				ddlInterfaceUnit.SelectedIndex = 0
				ddlInterfaceOrderItemUnit.SelectedIndex = 0
			End Try
			ddlSplitProductFormulationFacility.SelectedIndex = 0
			tbxPriority.Text = 100
			chkExportOnly.Checked = False
			retval = False
		End If
		ddlInterface_SelectedIndexChanged(ddlInterface, New EventArgs)

		Return retval
	End Function

	Private Sub SaveProductInterface()
		Dim id As Guid = Guid.Empty
		If Not Guid.TryParse(ddlProducts.SelectedValue, id) OrElse id.Equals(Guid.Empty) Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidNotSaved", Utilities.JsAlert("You must save the product before you can set up interface cross references.")) : Exit Sub
		If Not Guid.TryParse(ddlInterface.SelectedValue, id) OrElse id.Equals(Guid.Empty) Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidInterface", Utilities.JsAlert("An interface must be selected. Interface settings not saved.")) : Exit Sub
		If tbxInterfaceCrossReference.Text.Trim.Length = 0 Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidCrossReference", Utilities.JsAlert("A cross reference must be specified. Interface settings not saved.")) : Exit Sub
		If pnlInterfaceExchangeUnit.Visible AndAlso (Not Guid.TryParse(ddlInterfaceUnit.SelectedValue, id) OrElse id.Equals(Guid.Empty)) Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidExchangeUnit", Utilities.JsAlert("An interface exchange unit must be specified. Interface settings not saved.")) : Exit Sub
		If pnlInterfaceOrderItemUnit.Visible AndAlso (Not Guid.TryParse(ddlInterfaceOrderItemUnit.SelectedValue, id) OrElse id.Equals(Guid.Empty)) Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidOrderItem", Utilities.JsAlert("An interface order item unit must be specified.  Interface Setting not save")) : Exit Sub
		If Not IsNumeric(tbxPriority.Text.Trim) Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidPriority", Utilities.JsAlert("An order item sort priority must be a numeric value (default is 100)")) : Exit Sub

		' If this is not export only, check if there are any other settings with the same cross reference ID
		If Not chkExportOnly.Checked Then
			Dim allInterfaceSettings As ArrayList = KaProductInterfaceSettings.GetAll(GetUserConnection(_currentUser.Id), KaProductInterfaceSettings.FN_CROSS_REFERENCE & " = " & Q(tbxInterfaceCrossReference.Text.Trim) &
																							" AND " & KaProductInterfaceSettings.FN_INTERFACE_ID & " = " & Q(ddlInterface.SelectedValue) &
																							" AND " & KaProductInterfaceSettings.FN_DELETED & " = 0 " &
																							" AND " & KaProductInterfaceSettings.FN_EXPORT_ONLY & " = 0 " &
																							" AND " & KaProductInterfaceSettings.FN_ID & " <> " & Q(ddlProductInterface.SelectedValue), "")
			If allInterfaceSettings.Count > 0 Then
				ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidCrossReferenceExists", Utilities.JsAlert("A cross reference of " & tbxInterfaceCrossReference.Text.Trim & " already exists for this interface."))
				Exit Sub
			End If
		End If

		Dim prod As KaProduct = New KaProduct(GetUserConnection(_currentUser.Id), Guid.Parse(ddlProducts.SelectedValue))
		Dim prodInterfaceId As Guid = Guid.Parse(ddlProductInterface.SelectedValue)
		Dim prodInterface As KaProductInterfaceSettings
		Try
			prodInterface = New KaProductInterfaceSettings(GetUserConnection(_currentUser.Id), prodInterfaceId)
		Catch ex As RecordNotFoundException
			prodInterface = New KaProductInterfaceSettings
		End Try
		prodInterface.InterfaceId = Guid.Parse(ddlInterface.SelectedValue)
		prodInterface.CrossReference = tbxInterfaceCrossReference.Text.Trim
		If pnlInterfaceExchangeUnit.Visible Then prodInterface.UnitId = Guid.Parse(ddlInterfaceUnit.SelectedValue)
		If pnlInterfaceOrderItemUnit.Visible Then prodInterface.OrderItemUnitId = Guid.Parse(ddlInterfaceOrderItemUnit.SelectedValue)
		If pnlSplitProductFormulationFacility.Visible Then prodInterface.ProductSplitFormulationFacilityId = Guid.Parse(ddlSplitProductFormulationFacility.SelectedValue)
		prodInterface.ProductId = Guid.Parse(ddlProducts.SelectedValue)
		prodInterface.ProductPriority = tbxPriority.Text.Trim
		prodInterface.DefaultSetting = chkDefaultSetting.Checked
		prodInterface.ExportOnly = chkExportOnly.Checked
		prodInterface.SqlUpdateInsertIfNotFound(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
		prodInterfaceId = prodInterface.Id

		prod.ProductInterfaces.Add(prodInterface)

		PopulateProductInterfaceList(Guid.Parse(ddlProducts.SelectedValue))
		ddlProductInterface.SelectedValue = prodInterfaceId.ToString
		btnRemoveInterface.Enabled = True
	End Sub

	Private Sub RemoveProductInterface()
		Dim selectedId As Guid = Guid.Parse(ddlProductInterface.SelectedValue)
		If selectedId <> Guid.Empty Then
			Dim prodInterfaceSetting As KaProductInterfaceSettings = New KaProductInterfaceSettings(GetUserConnection(_currentUser.Id), selectedId)
			prodInterfaceSetting.Deleted = True
			prodInterfaceSetting.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
		End If
		PopulateProductInterfaceList(Guid.Parse(ddlProducts.SelectedValue))
		btnRemoveInterface.Enabled = PopulateProductInterfaceInformation(Guid.Empty)
	End Sub

	Private Sub DeleteProductInterfaceInformation(ByVal productId As Guid)
		For Each i As KaProductInterfaceSettings In KaProductInterfaceSettings.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND product_id=" & Q(productId), "")
			i.Deleted = True
			i.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
		Next
	End Sub
#End Region

#Region "Bulk Products"
	' values are stored as location_id|product_bulk_product_id~bulk_product_id~portion:product_bulk_product_id~bulk_product_id~portion
	Private Class BulkProductList
		Public Sub New()
		End Sub

		Public Sub New(ByVal value As String)
			Dim parts() As String = value.Split("|")
			_locationId = Guid.Parse(parts(0))
			If parts.Length > 1 Then
				For Each p As String In parts(1).Split(":")
					_bulkProducts.Add(New BulkProduct(p))
				Next
			End If
		End Sub

		Public Sub New(ByVal locationId As Guid)
			_locationId = locationId
		End Sub

		Private _locationId As Guid = Guid.Empty
		Public Property LocationId As Guid
			Get
				Return _locationId
			End Get
			Set(ByVal value As Guid)
				_locationId = value
			End Set
		End Property

		Private _bulkProducts As New List(Of BulkProduct)
		Public ReadOnly Property BulkProducts As List(Of BulkProduct)
			Get
				Return _bulkProducts
			End Get
		End Property

		Public Overrides Function ToString() As String
			Dim l As String = ""
			For Each p As BulkProduct In _bulkProducts
				If l.Length > 0 Then l &= ":"
				l &= p.ToString()
			Next
			Return _locationId.ToString() & IIf(l.Length > 0, "|", "") & l
		End Function

		Private Function GetBulkProductIndex(ByVal bulkProductId As Guid) As Integer
			Dim i As Integer = 0
			Do While i < _bulkProducts.Count
				If CType(_bulkProducts(i), BulkProduct).BulkProductId = bulkProductId Then
					Return i
				Else
					i += 1
				End If
			Loop
			Throw New RecordNotFoundException("Bulk product not found")
		End Function

		Public Sub RemoveBulkProduct(ByVal bulkProductId As Guid)
			Try
				_bulkProducts.RemoveAt(GetBulkProductIndex(bulkProductId))
			Catch ex As RecordNotFoundException
			End Try
		End Sub

		Public Sub UpdateBulkProductPercentage(ByVal bulkProductId As Guid, ByVal percent As Double)
			Try
				CType(_bulkProducts(GetBulkProductIndex(bulkProductId)), BulkProduct).Percent = percent
			Catch ex As RecordNotFoundException
			End Try
		End Sub

		Public Function GetBulkProductPercentage(ByVal bulkProductId As Guid) As Double
			Try
				Return CType(_bulkProducts(GetBulkProductIndex(bulkProductId)), BulkProduct).Percent
			Catch ex As RecordNotFoundException
                Return 0
            End Try
		End Function

		Public Function IsBulkProductInList(ByVal bulkProductId As Guid) As Boolean
			Try
				GetBulkProductIndex(bulkProductId)
				Return True
			Catch ex As RecordNotFoundException
				Return False
			End Try
		End Function

		Public Sub ClearBulkItems()
			_bulkProducts = New List(Of BulkProduct)
		End Sub
	End Class

	Private Class BulkProduct
		Public Sub New()
		End Sub

		Public Sub New(ByVal value As String)
			Dim parts() As String = value.Split("~")
			_bulkProductId = Guid.Parse(parts(0))
			_percent = Double.Parse(parts(1))
		End Sub

		Public Sub New(ByVal bulkProductId As Guid, ByVal percent As Double)
			_bulkProductId = bulkProductId
			_percent = percent
		End Sub

		Private _bulkProductId As Guid = Guid.Empty
		Public Property BulkProductId As Guid
			Get
				Return _bulkProductId
			End Get
			Set(ByVal value As Guid)
				_bulkProductId = value
			End Set
		End Property

		Private _percent As Double = 0
		Public Property Percent As Double
			Get
				Return _percent
			End Get
			Set(ByVal value As Double)
				_percent = value
			End Set
		End Property

        Public Overrides Function ToString() As String
            Return _bulkProductId.ToString() & "~" & _percent
        End Function
	End Class

	Private Sub UpdateBulkProductControls()
		Dim l As New BulkProductList(ddlLocation.SelectedValue)
		Dim bulkProductId As Guid = Guid.Parse(ddlBulkProduct.SelectedValue)
		Dim inList As Boolean = l.IsBulkProductInList(bulkProductId)
		Dim percent As Double = l.GetBulkProductPercentage(bulkProductId)
		btnAddBulkProduct.Enabled = l.LocationId <> Guid.Empty AndAlso bulkProductId <> Guid.Empty AndAlso Not inList
		btnRemoveBulkProduct.Enabled = l.LocationId <> Guid.Empty AndAlso bulkProductId <> Guid.Empty AndAlso inList
		btnUpdateBulkProductPercent.Enabled = btnRemoveBulkProduct.Enabled
		tbxPercent.Text = percent
		PopulateBulkProductTable()
	End Sub

	Private Sub PopulateLocationList(ByVal productId As Guid)
		Dim locationId As Guid
		Try
			locationId = New BulkProductList(ddlLocation.SelectedValue).LocationId
		Catch ex As FormatException
			locationId = Guid.Empty
		End Try
		Dim lastValue As String = ddlLocation.SelectedValue
		ddlLocation.Items.Clear()
		ddlLocation.Items.Add(New ListItem("", Guid.Empty.ToString()))
		For Each location As KaLocation In KaLocation.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
			Dim l As New BulkProductList(location.Id)
			For Each r As KaProductBulkProduct In KaProductBulkProduct.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND product_id=" & Q(productId) & " AND location_id=" & Q(location.Id), "position ASC")
				l.BulkProducts.Add(New BulkProduct(r.BulkProductId, r.Portion))
			Next
			ddlLocation.Items.Add(New ListItem(location.Name, l.ToString()))
		Next
		Dim i As Integer = 0
		Do While i < ddlLocation.Items.Count
			Dim p As New BulkProductList(ddlLocation.Items(i).Value)
			If p.LocationId = locationId Then
				ddlLocation.SelectedIndex = i
				UpdateBulkProductControls()
				Exit Do
			End If
			i += 1
		Loop
	End Sub

	Private Sub PopulateProductGroupList()
		ddlProductGroup.Items.Clear()
		Dim li As ListItem = New ListItem
		li.Text = ""
		li.Value = Guid.Empty.ToString
		ddlProductGroup.Items.Add(li)
		Dim allProductGroups As ArrayList = KaProductGroup.GetAll(GetUserConnection(_currentUser.Id), "deleted = " & Q(False), "name asc")
		For Each productGroup As KaProductGroup In allProductGroups
			li = New ListItem
			li.Text = productGroup.Name
			li.Value = productGroup.Id.ToString
			ddlProductGroup.Items.Add(li)
		Next
	End Sub

	Private Sub PopulateBulkProductList()
		ddlBulkProduct.Items.Clear()
		ddlBulkProduct.Items.Add(New ListItem("", Guid.Empty.ToString()))
		Dim selectedOwnerId As Guid = Guid.Parse(ddlOwner.SelectedValue)
		Dim bulkProdWhere As String = ""
		If selectedOwnerId <> Guid.Empty Then
			bulkProdWhere = " and (owner_id = " & Q(selectedOwnerId) & " or owner_id = " & Q(Guid.Empty) & ")"
		End If
		Dim allBulkProds As ArrayList = KaBulkProduct.GetAll(GetUserConnection(_currentUser.Id), "deleted = " & Q(False) & bulkProdWhere, "name ASC")
        For Each bulkProduct As KaBulkProduct In allBulkProds
            ddlBulkProduct.Items.Add(New ListItem(bulkProduct.Name, bulkProduct.Id.ToString()))
        Next
    End Sub

	Private Sub PopulateBulkProductTable()
		Dim total As Double = 0
		Dim l As New BulkProductList(ddlLocation.SelectedValue)
		tblBulkProducts.Rows.Clear()
		Dim headerRow As New HtmlTableRow
		Dim headerCell1 As New HtmlTableCell("th")
		headerCell1.InnerHtml = "Bulk product"
		'headerCell1.Width = "120px"
		headerRow.Controls.Add(headerCell1)
		Dim headerCell2 As New HtmlTableCell("th")
		headerCell2.InnerHtml = "Portion"
		'  headerCell2.Width = "60px"
		headerRow.Controls.Add(headerCell2)
		If l.BulkProducts.Count > 1 Then
			Dim headerCell3 As New HtmlTableCell("th")
			headerCell3.InnerHtml = "Order"
			'  headerCell3.Width = "60px"
			headerCell3.ColSpan = 2
			headerRow.Controls.Add(headerCell3)
		End If
		tblBulkProducts.Rows.Add(headerRow)

		Dim bpIndex As Integer = 1
		For Each p As BulkProduct In l.BulkProducts
			Dim bpRow As New HtmlTableRow
			Dim bpCell1 As New HtmlTableCell
			bpCell1.InnerHtml = New KaBulkProduct(GetUserConnection(_currentUser.Id), p.BulkProductId).Name
			bpRow.Controls.Add(bpCell1)
			Dim bpCell2 As New HtmlTableCell
			If Utilities.IsBulkProductTimedFunction(p.BulkProductId, _currentUser.Id) Then
                bpCell2.InnerHtml &= String.Format("{0} sec", p.Percent)
            ElseIf Utilities.IsBulkProductParameterlessFunction(p.BulkProductId, _currentUser.Id) Then
				bpCell2.InnerHtml &= "&nbsp;"
			Else
                bpCell2.InnerHtml &= String.Format("{0}%", p.Percent)
                total += p.Percent
			End If
			bpRow.Controls.Add(bpCell2)
			If l.BulkProducts.Count > 1 Then
				Dim bpCell3 As New HtmlTableCell
				bpCell3.Align = "center"
				If bpIndex > 1 Then
					Dim moveUp As New LinkButton
					With moveUp
						.ID = "btnMoveProductUp" & bpIndex
						'  .AlternateText = "Move Up"
						.CssClass = "button"
						.Text = "u"
						.EnableViewState = True
						AddHandler .Click, AddressOf btnMoveProductUp1_Click
					End With
					bpCell3.Controls.Add(moveUp)
				Else
					bpCell3.InnerHtml = ""
				End If
				'    bpCell3.Width = "30px"
				bpRow.Controls.Add(bpCell3)
				Dim bpCell4 As New HtmlTableCell
				bpCell4.Align = "center"
				If bpIndex < l.BulkProducts.Count Then
					Dim moveDown As New LinkButton
					With moveDown
						.ID = "btnMoveProductDown" & bpIndex
						'   .AlternateText = "Move Down"
						.CssClass = "button"
						.Text = "d"
						.EnableViewState = True
						AddHandler .Click, AddressOf btnMoveProductDown1_Click
					End With
					bpCell4.Controls.Add(moveDown)
				Else
					bpCell4.InnerHtml = ""
				End If
				'  bpCell4.Width = "30px"
				bpRow.Controls.Add(bpCell4)
			End If
			tblBulkProducts.Rows.Add(bpRow)
			bpIndex += 1
		Next

		Dim footerRow As New HtmlTableRow()
		Dim footerCell1 As New HtmlTableCell
		footerCell1.InnerHtml = "Total"
		footerRow.Controls.Add(footerCell1)
		Dim footerCell2 As New HtmlTableCell
        footerCell2.InnerHtml = IIf(total = 100, "", "<font color=""FF0000"">") & String.Format("{0}%", total) & IIf(total = 100, "", "</font>")
        footerRow.Controls.Add(footerCell2)
		Dim footerCell3 As New HtmlTableCell
		footerCell3.InnerHtml = ""
		footerRow.Controls.Add(footerCell3)
		Dim footerCell4 As New HtmlTableCell
		footerCell4.InnerHtml = ""
		footerRow.Controls.Add(footerCell4)
		tblBulkProducts.Rows.Add(footerRow)

		pnlBulkProducts.Visible = ddlProducts.SelectedIndex > 0 AndAlso ddlLocation.SelectedIndex > 0
	End Sub

	Private Sub SaveBulkProductInformation(ByVal productId As Guid)
		Dim i As Integer = 1 ' skip the first (blank) entry
		Do While i < ddlLocation.Items.Count ' for each location
			Dim l As New BulkProductList(ddlLocation.Items(i).Value) ' parse the value into a structure
			' get the current records from the database
			Dim m As ArrayList = KaProductBulkProduct.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND product_id=" & Q(productId) & " AND location_id=" & Q(l.LocationId), "")
			Dim position As Integer = 0
			For Each p As BulkProduct In l.BulkProducts
				Dim found As Boolean = False
				Dim j As Integer = 0
				Do While j < m.Count
					If CType(m(j), KaProductBulkProduct).BulkProductId = p.BulkProductId Then ' if the record already exists, update it and remove it from this list
						CType(m(j), KaProductBulkProduct).Position = position
						CType(m(j), KaProductBulkProduct).Portion = p.Percent
						CType(m(j), KaProductBulkProduct).SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
						found = True
						m.RemoveAt(j)
					Else
						j += 1
					End If
				Loop
				If Not found Then ' if the record does not already exist, create it
					With New KaProductBulkProduct()
						.BulkProductId = p.BulkProductId
						.LocationId = l.LocationId
						.Portion = p.Percent
						.ProductId = productId
						.Position = position
						.SqlInsert(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
					End With
				End If
				position += 1
			Next
			For Each r As KaProductBulkProduct In m ' delete the remaining records (which must have been removed)
				r.Deleted = True
				r.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
			Next
			i += 1
		Loop
	End Sub

	Private Sub DeleteBulkProductInformation(ByVal productId As Guid)
		For Each r As KaProductBulkProduct In KaProductBulkProduct.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND product_id=" & Q(productId), "")
			r.Deleted = True
			r.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
		Next
	End Sub

	Protected Sub btnMoveProductUp1_Click(sender As Object, e As EventArgs)
		Dim index As Integer = Integer.Parse(CType(sender, LinkButton).ID.Replace("btnMoveProductUp", ""))
		Dim l As New BulkProductList(ddlLocation.SelectedValue)
		Dim itemList As New SortedList(Of Integer, BulkProduct)
		For n As Integer = 1 To l.BulkProducts.Count
			If n = index - 1 Then
				itemList.Add(n + 1, l.BulkProducts(n - 1))
			ElseIf n = index Then
				itemList.Add(n - 1, l.BulkProducts(n - 1))
			Else
				itemList.Add(n, l.BulkProducts(n - 1))
			End If
		Next
		l.ClearBulkItems()
		For n As Integer = 1 To itemList.Count
			l.BulkProducts.Add(itemList(n))
		Next

		ddlLocation.SelectedItem.Value = l.ToString()
		PopulateBulkProductTable()
	End Sub

	Protected Sub btnMoveProductDown1_Click(sender As Object, e As EventArgs)
		Dim index As Integer = Integer.Parse(CType(sender, LinkButton).ID.Replace("btnMoveProductDown", ""))
		Dim l As New BulkProductList(ddlLocation.SelectedValue)
		Dim itemList As New SortedList(Of Integer, BulkProduct)
		For n As Integer = 1 To l.BulkProducts.Count
			If n = index Then
				itemList.Add(n + 1, l.BulkProducts(n - 1))
			ElseIf n = index + 1 Then
				itemList.Add(n - 1, l.BulkProducts(n - 1))
			Else
				itemList.Add(n, l.BulkProducts(n - 1))
			End If
		Next
		l.ClearBulkItems()
		For n As Integer = 1 To itemList.Count
			l.BulkProducts.Add(itemList(n))
		Next

		ddlLocation.SelectedItem.Value = l.ToString()
		PopulateBulkProductTable()
	End Sub
#End Region

	Private Sub PopulateProductList()
		ddlProducts.Items.Clear()
		If _currentUserPermission(_currentTableName).Create Then
			ddlProducts.Items.Add(New ListItem("Enter a new product", Guid.Empty.ToString()))
		Else
			ddlProducts.Items.Add(New ListItem("Select a product", Guid.Empty.ToString()))
		End If
		Dim conditions As String = String.Format("deleted=0" & IIf(_currentUser.OwnerId <> Guid.Empty, " AND (owner_id={0} OR owner_id={1})", ""), Q(_currentUser.OwnerId), Q(Guid.Empty))
		For Each product As KaProduct In KaProduct.GetAll(GetUserConnection(_currentUser.Id), conditions, "name ASC")
			ddlProducts.Items.Add(New ListItem(product.Name, product.Id.ToString()))
		Next
	End Sub

	Private Sub PopulateOwnerList()
		ddlOwner.Items.Clear()
		ddlOwner.Items.Add(New ListItem("All owners", Guid.Empty.ToString()))
		For Each owner In KaOwner.GetAll(GetUserConnection(_currentUser.Id), "deleted=0" & IIf(_currentUser.OwnerId = Guid.Empty, "", " AND id=" & Q(_currentUser.OwnerId)), "name ASC")
			ddlOwner.Items.Add(New ListItem(owner.Name, owner.Id.ToString()))
		Next
	End Sub

	Private Sub PopulateUnitList()
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim showTime As Boolean = False
		Dim productId As Guid = Guid.Parse(ddlProducts.SelectedValue)
		If productId <> Guid.Empty Then
			Try ' to determine if the product is a timed function
				showTime = New KaProduct(GetUserConnection(_currentUser.Id), productId).IsTimedFunction(connection)
			Catch ex As RecordNotFoundException ' suppress exception
			End Try
		End If
		ddlUnit.Items.Clear()
		ddlUnit.Items.Add(New ListItem("", Guid.Empty.ToString()))
		ddlInterfaceUnit.Items.Clear()
		ddlInterfaceUnit.Items.Add(New ListItem("", Guid.Empty.ToString()))
		ddlInterfaceOrderItemUnit.Items.Clear()
		ddlInterfaceOrderItemUnit.Items.Add(New ListItem("", Guid.Empty.ToString()))
		For Each u As KaUnit In KaUnit.GetAll(connection, "deleted=0", "name ASC")
			If showTime Xor Not KaUnit.IsTime(u.BaseUnit) Then
				ddlUnit.Items.Add(New ListItem(u.Name, u.Id.ToString()))
				ddlInterfaceUnit.Items.Add(New ListItem(u.Name, u.Id.ToString()))
				ddlInterfaceOrderItemUnit.Items.Add(New ListItem(u.Name, u.Id.ToString()))
			End If
		Next
	End Sub

	Private Sub PopulateFacilityForSplitProductDropdownLists()
		ddlSplitProductFormulationFacility.Items.Clear()
		ddlSplitProductFormulationFacility.Items.Add(New ListItem("Do not split product", Guid.Empty.ToString()))
		For Each u As KaLocation In KaLocation.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
			ddlSplitProductFormulationFacility.Items.Add(New ListItem(u.Name, u.Id.ToString()))
		Next
	End Sub

	Private Function PopulateProductInformation(ByVal productId As Guid) As Boolean
		_customFieldData.Clear()
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim product As KaProduct
		Try
			product = New KaProduct(connection, productId)
			pnlProductInterfaceSetup.Visible = KaInterface.GetAll(connection, "deleted=0", "name ASC").Count > 0

			For Each customFieldValue As KaCustomFieldData In KaCustomFieldData.GetAll(GetUserConnection(_currentUser.Id), String.Format("deleted = 0 AND {0} = {1}", KaCustomFieldData.FN_RECORD_ID, Q(product.Id)), KaCustomFieldData.FN_LAST_UPDATED)
				_customFieldData.Add(customFieldValue)
			Next
		Catch ex As RecordNotFoundException
			product = New KaProduct()
			pnlProductInterfaceSetup.Visible = False
		End Try

		With product
			tbxName.Text = .Name
			Try
				ddlOwner.SelectedValue = .OwnerId.ToString()
			Catch ex As ArgumentOutOfRangeException
				ddlOwner.SelectedIndex = 0
				ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidOwnerId", Utilities.JsAlert("Record not found in owners with ID = " & .OwnerId.ToString() & ". Owner set to ""all owners."" instead."))
			End Try
			Try
				ddlUnit.SelectedValue = .DefaultUnitId.ToString()
			Catch ex As ArgumentOutOfRangeException
				Dim showTime As Boolean = False
				If productId <> Guid.Empty Then
					Try ' to determine if the product is a timed function
						showTime = New KaProduct(GetUserConnection(_currentUser.Id), productId).IsTimedFunction(connection)
					Catch exRecNotFound As RecordNotFoundException ' suppress exception
					End Try
				End If
				Try
					Dim u As New KaUnit(connection, Guid.Parse(ddlUnit.Items(1).Value))
					If showTime AndAlso KaUnit.IsTime(u.BaseUnit) Then
						ddlUnit.SelectedIndex = 1
					Else
						Throw
					End If
				Catch ex2 As Exception
					ddlUnit.SelectedIndex = 0
					ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidUnitId", Utilities.JsAlert("Record not found in units with ID = " & .DefaultUnitId.ToString() & ". Unit set to blank instead."))
				End Try
			End Try
			tbxNotes.Text = .Notes
			tbxEpaNumber.Text = .EpaNumber
			tbxMsdsNumber.Text = .MsdsNumber
			tbxManufacturer.Text = .Manufacturer
			tbxActiveIngredients.Text = .ActiveIngredients
			tbxRestrictions.Text = .Restrictions
			tbxMaxAppRate.Text = .MaximumApplicationRate
			tbxMinAppRate.Text = .MinimumApplicationRate
			chkDoNotStack.Checked = .DoNotStack
			Try
				ddlProductGroup.SelectedValue = .ProductGroupId.ToString
			Catch ex As Exception
				ddlOwner.SelectedIndex = 0
				ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidGroupId", Utilities.JsAlert("Record not found in product groups with ID = " & .ProductGroupId.ToString() & ". Product Group set to ""none"" instead."))
			End Try
			chkHazardousMaterial.Checked = .HazardousMaterial
		End With
		PopulateProductInterfaceList(productId)
		PopulateLocationList(productId)
		PopulateBulkProductTable()
		btnRemoveInterface.Enabled = PopulateProductInterfaceInformation(Guid.Parse(ddlProductInterface.SelectedValue))
		Utilities.CreateDynamicCustomFieldPanelControls(_customFields, _customFieldData, lstCustomFields, Page)
		Return productId <> Guid.Empty
	End Function

	Private Function SaveProductInformation(ByRef productId As Guid) As Boolean
		lblStatus.Text = ""
		If tbxName.Text.Length = 0 Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidName", Utilities.JsAlert("Name must be entered")) : Return False
		If Guid.Parse(ddlUnit.SelectedValue) = Guid.Empty Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidUnit", Utilities.JsAlert("A default unit must be selected")) : Return False
		If Not IsNumeric(tbxMaxAppRate.Text) Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidCharMaxAppRate", Utilities.JsAlert("The max application rate must be a numeric value")) : Return False
		If Not IsNumeric(tbxMinAppRate.Text) Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidMaxAppRate", Utilities.JsAlert("The min application rate must be a numeric value")) : Return False
		Dim product As New KaProduct()
		With product
			.Id = productId
			.Name = tbxName.Text
			.OwnerId = Guid.Parse(ddlOwner.SelectedValue)
			.DefaultUnitId = Guid.Parse(ddlUnit.SelectedValue)
			.Notes = tbxNotes.Text
			.EpaNumber = tbxEpaNumber.Text
			.MsdsNumber = tbxMsdsNumber.Text
			.Manufacturer = tbxManufacturer.Text
			.ActiveIngredients = tbxActiveIngredients.Text
			.Restrictions = tbxRestrictions.Text
			.MaximumApplicationRate = tbxMaxAppRate.Text
			.MinimumApplicationRate = tbxMinAppRate.Text
			.DoNotStack = chkDoNotStack.Checked
			.ProductGroupId = Guid.Parse(ddlProductGroup.SelectedValue)
			.HazardousMaterial = chkHazardousMaterial.Checked
			.ProductInterfaces = GetAllProductInterfaceSettings()
			Dim status As String = ""
			If .Id = Guid.Empty Then
				.SqlInsert(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
				status = "Product added successfully."
			Else
				.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
				status = "Product updated successfully."
			End If
			productId = .Id
			SaveBulkProductInformation(productId)

			Utilities.ConvertCustomFieldPanelToLists(_customFields, _customFieldData, lstCustomFields)
			For Each customData As KaCustomFieldData In _customFieldData
				customData.RecordId = .Id
				customData.SqlUpdateInsertIfNotFound(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
			Next

			PopulateProductList()
			ddlProducts.SelectedValue = .Id.ToString
			ddlProducts_SelectedIndexChanged(ddlProducts, New EventArgs())
			lblStatus.Text = status
		End With
		Return True
	End Function

	Private Function GetAllProductInterfaceSettings() As List(Of KaProductInterfaceSettings)
		Dim retval As List(Of KaProductInterfaceSettings) = New List(Of KaProductInterfaceSettings)
		For Each li As ListItem In ddlProductInterface.Items
			Dim tempId As Guid = Guid.Parse(li.Value)
			If tempId <> Guid.Empty Then
				Dim prodInterfaceSetting As KaProductInterfaceSettings = New KaProductInterfaceSettings(GetUserConnection(_currentUser.Id), tempId)
				retval.Add(prodInterfaceSetting)
			End If
		Next
		Return retval
	End Function

	Private Function DeleteProductInformation(ByVal productId As Guid) As Boolean
		lblStatus.Text = ""
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim productIdVal As String = productId.ToString()
		Dim reader As OleDbDataReader = Tm2Database.ExecuteReader(connection, String.Format("SELECT DISTINCT orders.id AS order_id FROM orders, order_items WHERE order_items.product_id={0} AND order_items.deleted=0 AND order_items.order_id=orders.id AND orders.deleted=0 AND orders.completed=0", Q(productId)))
		Dim conditions As String = "id=" & Q(Guid.Empty)
		Do While reader.Read()
			conditions &= " OR id=" & Q(reader("order_id"))
		Loop
		reader.Close()
		Dim orders As ArrayList = KaOrder.GetAll(connection, conditions, "number ASC")

		If orders.Count > 0 Then
			Dim warning As String = "Product is associated with other records (see below for details). Product information not deleted.\n\nDetails:\n\nOrders:\n"
			Dim lastOrderId As Guid = Guid.Empty
			For Each order As KaOrder In orders
				warning &= New KaOrder(GetUserConnection(_currentUser.Id), order.Id).Number & " "
				lastOrderId = order.Id
			Next
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidProductUsed", Utilities.JsAlert(warning))
			Return False
		Else
			With New KaProduct(GetUserConnection(_currentUser.Id), productId)
				.Deleted = True
				.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
				lblStatus.Text = "Product deleted successfully."
			End With
			Return True
		End If
	End Function

	Private Sub ddlOwner_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlOwner.SelectedIndexChanged
		PopulateBulkProductList()
	End Sub

	Private Sub SetTextboxMaxLengths()
		tbxActiveIngredients.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaProduct.TABLE_NAME, "active_ingredients"))
		tbxInterfaceCrossReference.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaProductInterfaceSettings.TABLE_NAME, "cross_reference"))
		tbxEpaNumber.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaProduct.TABLE_NAME, "epa_number"))
		tbxManufacturer.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaProduct.TABLE_NAME, "manufacturer"))
		tbxMaxAppRate.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaProduct.TABLE_NAME, "maximum_application_rate"))
		tbxMinAppRate.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaProduct.TABLE_NAME, "minimum_application_rate"))
		tbxMsdsNumber.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaProduct.TABLE_NAME, "msds_number"))
		tbxName.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaProduct.TABLE_NAME, "name"))
		tbxNotes.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaProduct.TABLE_NAME, "notes"))
		tbxPercent.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaProductBulkProduct.TABLE_NAME, "portion"))
		tbxPriority.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaProductInterfaceSettings.TABLE_NAME, "product_priority"))
		tbxRestrictions.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaProduct.TABLE_NAME, "restrictions"))
	End Sub

	Private Sub ddlInterface_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles ddlInterface.SelectedIndexChanged
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		If Guid.Parse(ddlProductInterface.SelectedValue) = Guid.Empty Then
			'Only do this check if we are a new interface setting
			Dim count As Integer = 0
			Try
				Dim rdr As OleDb.OleDbDataReader = Tm2Database.ExecuteReader(connection, "SELECT count(*) " &
						"FROM " & KaProductInterfaceSettings.TABLE_NAME & " " &
						"WHERE " & KaProductInterfaceSettings.FN_DELETED & " = 0 " &
						"AND " & KaProductInterfaceSettings.FN_INTERFACE_ID & " = " & Q(Guid.Parse(ddlInterface.SelectedValue)) & " " &
						"AND " & KaProductInterfaceSettings.FN_PRODUCT_ID & " = " & Q(Guid.Parse(ddlProducts.SelectedValue)))
				If rdr.Read Then count = rdr.Item(0)
				rdr.Close()
			Catch ex As Exception

			End Try
			chkDefaultSetting.Checked = (count = 0)
		End If

		Try
			Dim interfaceId As Guid = Guid.Empty
			Guid.TryParse(ddlInterface.SelectedValue, interfaceId)
			Dim interfaceTypeInfo As New KaInterfaceTypes(connection, New KaInterface(connection, interfaceId).InterfaceTypeId)
			pnlInterfaceExchangeUnit.Visible = interfaceTypeInfo.ShowInterfaceExchangeUnit
			pnlInterfaceOrderItemUnit.Visible = Not interfaceTypeInfo.UseInterfaceUnitAsOrderItemUnit
			pnlSplitProductFormulationFacility.Visible = interfaceTypeInfo.SplitProductIntoComponents
		Catch ex As RecordNotFoundException
			pnlInterfaceExchangeUnit.Visible = True
			pnlInterfaceOrderItemUnit.Visible = False
			pnlSplitProductFormulationFacility.Visible = False
		End Try
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
			Dim shouldEnable = (.Edit AndAlso ddlProducts.SelectedIndex > 0) OrElse (.Create AndAlso ddlProducts.SelectedIndex = 0)
			pnlEven.Enabled = shouldEnable
			pnlInterfaceSettings.Enabled = shouldEnable
			pnlBulkProducts.Enabled = shouldEnable
			btnSave.Enabled = shouldEnable
			Dim value = Guid.Parse(ddlProducts.SelectedValue)
			btnDelete.Enabled = .Edit AndAlso .Delete AndAlso value <> Guid.Empty
		End With
	End Sub
End Class