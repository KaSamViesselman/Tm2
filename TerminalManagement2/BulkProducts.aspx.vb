Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb
Imports System.Linq

Public Class BulkProducts
	Inherits System.Web.UI.Page

	Private _currentUser As KaUser
	Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
	Private _currentTableName As String = KaProduct.TABLE_NAME
	Private _customFields As New List(Of KaCustomField)
	Private _customFieldData As New List(Of KaCustomFieldData)
	Private _derivedFromEntries As List(Of String)

#Region "Events"
	Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
		Response.Cache.SetCacheability(HttpCacheability.NoCache)
		_currentUser = Utilities.GetUser(Me)
		_currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Products")
		If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")
		lblStatus.Text = ""
		If Not Page.IsPostBack Then
			Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
			SetTextboxMaxLengths()
			_customFields.Clear()
			For Each customField As KaCustomField In KaCustomField.GetAll(GetUserConnection(_currentUser.Id), String.Format("deleted=0 AND {0} = {1}", KaCustomField.FN_TABLE_NAME, Q(KaBulkProduct.TABLE_NAME)), KaCustomField.FN_FIELD_NAME)
				_customFields.Add(customField)
			Next
			PopulateCropTypes()
			PopulateBulkProductList()
			PopulateOwnerList()
			PopulateUnitLists()
			SetControlUsabilityFromPermissions()
			Utilities.ConfirmBox(Me.btnDelete, "Are you sure you want to delete this bulk product?") ' Delete confirmation box setup
			Utilities.SetFocus(tbxName, Me) ' set focus to the first textbox on the page
			btnDelete.Enabled = PopulateBulkProductInformation(Guid.Parse(ddlBulkProducts.SelectedValue)) AndAlso _currentUserPermission(_currentTableName).Delete
			pnlLotUsageTrackingType.Visible = Tm2Database.SystemItemTraceabilityEnabled
			If Page.Request("BulkProductId") IsNot Nothing Then
				Try
					ddlBulkProducts.SelectedValue = Page.Request("BulkProductId")
					ddlBulkProducts_SelectedIndexChanged(Nothing, Nothing)
				Catch ex As ArgumentOutOfRangeException
					ddlBulkProducts.SelectedIndex = 0
					ddlBulkProducts_SelectedIndexChanged(Nothing, Nothing)
				End Try
			Else
				ddlBulkProducts_SelectedIndexChanged(Nothing, Nothing)
				ddlOwner.SelectedValue = _currentUser.OwnerId.ToString()
			End If
		ElseIf Page.IsPostBack And Request("__EVENTARGUMENT") = "SaveNewProductForBulkProduct" Then
			SaveNewProductForBulkProduct()
		End If
	End Sub

	Private Sub ddlBulkProducts_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ddlBulkProducts.SelectedIndexChanged
		PopulateUnitLists()
		SetControlUsabilityFromPermissions()
		btnDelete.Enabled = PopulateBulkProductInformation(Guid.Parse(ddlBulkProducts.SelectedValue)) AndAlso _currentUserPermission(_currentTableName).Delete
		Utilities.SetFocus(tbxName, Me)
	End Sub

	Protected Sub btnRemoveInterface_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnRemoveInterface.Click
		RemoveInterface()
	End Sub

	Protected Sub btnSaveInterface_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnSaveInterface.Click
		SaveInterface()
	End Sub

	Protected Sub ddlBulkProductInterface_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles ddlBulkProductInterface.SelectedIndexChanged
		btnRemoveInterface.Enabled = PopulateInterfaceInformation(Guid.Parse(ddlBulkProductInterface.SelectedValue))
	End Sub

	Private Sub btnSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSave.Click
		Dim bulkProductId As Guid = Guid.Parse(ddlBulkProducts.SelectedValue)
		SaveBulkProduct(bulkProductId)
	End Sub

	Private Sub btnDelete_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDelete.Click
		If DeleteBulkProduct(Guid.Parse(ddlBulkProducts.SelectedValue)) Then
			PopulateBulkProductList()
			ddlBulkProducts.SelectedValue = Guid.Empty.ToString()
			PopulateBulkProductInformation(Guid.Empty)
			btnDelete.Enabled = False
		End If
	End Sub


#Region " Derived from "
	Protected Sub lstDerivedFrom_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lstDerivedFrom.SelectedIndexChanged
		If lstDerivedFrom.SelectedIndex >= 0 Then
			tbxDerivedFrom.Text = lstDerivedFrom.SelectedItem.Value

			tbxDerivedFrom.Enabled = True
			btnRemoveNewDerivedFrom.Enabled = True
			btnUpdateDerivedFrom.Enabled = True
		Else
			tbxDerivedFrom.Text = ""
			tbxDerivedFrom.Enabled = False
			btnRemoveNewDerivedFrom.Enabled = False
			btnUpdateDerivedFrom.Enabled = False
		End If
	End Sub

	Protected Sub btnAddNewDerivedFrom_Click(sender As Object, e As EventArgs) Handles btnAddNewDerivedFrom.Click
		If Not _derivedFromEntries.Contains("") Then
			_derivedFromEntries.Add("")
			lstDerivedFrom.Items.Add(New ListItem("(empty)", ""))
			lstDerivedFrom.SelectedIndex = lstDerivedFrom.Items.Count - 1
			PopulateDerivedEntries()
		End If
	End Sub

	Protected Sub btnRemoveNewDerivedFrom_Click(sender As Object, e As EventArgs) Handles btnRemoveNewDerivedFrom.Click
		Dim selectedIndex As Integer = lstDerivedFrom.SelectedIndex
		If selectedIndex >= 0 Then
			_derivedFromEntries.Remove(lstDerivedFrom.SelectedItem.Value)
			If _derivedFromEntries.Count > 0 Then
				lstDerivedFrom.SelectedIndex = Math.Max(0, Math.Min(selectedIndex - 1, _derivedFromEntries.Count - 1))
			End If
			PopulateDerivedEntries()
		End If
	End Sub

	Protected Sub btnUpdateDerivedFrom_Click(sender As Object, e As EventArgs) Handles btnUpdateDerivedFrom.Click
		Dim selectedIndex As Integer = lstDerivedFrom.SelectedIndex
		If selectedIndex >= 0 Then
			_derivedFromEntries.Remove(lstDerivedFrom.SelectedItem.Value)
			If Not _derivedFromEntries.Contains(tbxDerivedFrom.Text.Trim) Then _derivedFromEntries.Add(tbxDerivedFrom.Text.Trim)
			lstDerivedFrom.SelectedIndex = selectedIndex
			PopulateDerivedEntries()
		End If
	End Sub
#End Region
#End Region

#Region "Interfaces"
	Private Sub PopulateInterfaceList()
		ddlInterface.Items.Clear()
		ddlInterface.Items.Add(New ListItem("", Guid.Empty.ToString()))

		Dim getInterfaceRdr As OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), "SELECT interfaces.id, interfaces.name " &
				"FROM interfaces " &
				"INNER JOIN interface_types ON interfaces.interface_type_id = interface_types.id " &
				"WHERE (interfaces.deleted = 0) " &
					"AND (interface_types.deleted = 0) " &
					"AND ((" & KaInterfaceTypes.FN_SHOW_BULK_PRODUCT_INTERFACE & " = 1) " &
					"OR (interfaces.id IN (SELECT " & KaBulkProductInterfaceSettings.TABLE_NAME & ".interface_id " &
											"FROM " & KaBulkProductInterfaceSettings.TABLE_NAME & " " &
											"WHERE (deleted=0) " &
												"AND (" & KaBulkProductInterfaceSettings.TABLE_NAME & ".bulk_product_id = " & Q(ddlBulkProducts.SelectedValue) & ") " &
												"AND (" & KaBulkProductInterfaceSettings.TABLE_NAME & ".bulk_product_id <> " & Q(Guid.Empty) & ")))) " &
				"ORDER BY interfaces.name")
		Do While getInterfaceRdr.Read
			ddlInterface.Items.Add(New ListItem(getInterfaceRdr.Item("name"), getInterfaceRdr.Item("id").ToString()))
		Loop
		getInterfaceRdr.Close()
		pnlInterfaceSetup.Visible = (ddlInterface.Items.Count > 1) AndAlso ddlBulkProducts.SelectedValue <> Guid.Empty.ToString
	End Sub

	Private Sub SaveInterface()
		If Guid.Parse(ddlBulkProducts.SelectedValue) = Guid.Empty Then DisplayJavaScriptMessage("InvalidBulkProduct", Utilities.JsAlert("You must save the Bulk Product before you can set up interface cross references.")) : Exit Sub
		If Guid.Parse(ddlInterface.SelectedValue) = Guid.Empty Then DisplayJavaScriptMessage("InvalidInterface", Utilities.JsAlert("An interface must be selected. Interface settings not saved.")) : Exit Sub
		If tbxInterfaceCrossReference.Text.Length = 0 Then DisplayJavaScriptMessage("InvalidCrossReference", Utilities.JsAlert("A cross reference must be specified. Interface settings not saved.")) : Exit Sub
		If pnlInterfaceExchangeUnit.Visible AndAlso Guid.Parse(ddlInterfaceUnit.SelectedValue) = Guid.Empty Then DisplayJavaScriptMessage("InvalidExchangeUnit", Utilities.JsAlert("An interface exchange unit must be specified. Interface settings not saved.")) : Exit Sub

		' If this is not export only, check if there are any other settings with the same cross reference ID
		If Not chkExportOnly.Checked Then
			Dim allInterfaceSettings As ArrayList = KaBulkProductInterfaceSettings.GetAll(GetUserConnection(_currentUser.Id), KaBulkProductInterfaceSettings.FN_CROSS_REFERENCE & " = " & Q(tbxInterfaceCrossReference.Text.Trim) &
									" AND " & KaBulkProductInterfaceSettings.FN_INTERFACE_ID & " = " & Q(ddlInterface.SelectedValue) &
									" AND " & KaBulkProductInterfaceSettings.FN_DELETED & " = 0 " &
									" AND " & KaBulkProductInterfaceSettings.FN_EXPORT_ONLY & " = 0 " &
									" AND " & KaBulkProductInterfaceSettings.FN_ID & " <> " & Q(ddlBulkProductInterface.SelectedValue), "")
			If allInterfaceSettings.Count > 0 Then
				DisplayJavaScriptMessage("InvalidCrossReference", Utilities.JsAlert("A cross reference of " & tbxInterfaceCrossReference.Text.Trim & " already exists for this interface."))
				Exit Sub
			End If
		End If

		Dim bprodInterfaceId As Guid = Guid.Parse(ddlBulkProductInterface.SelectedValue)
		If bprodInterfaceId = Guid.Empty Then
			Dim bprodInterface As KaBulkProductInterfaceSettings = New KaBulkProductInterfaceSettings
			bprodInterface.BulkProductId = Guid.Parse(ddlBulkProducts.SelectedValue)
			bprodInterface.InterfaceId = Guid.Parse(ddlInterface.SelectedValue)
			bprodInterface.CrossReference = tbxInterfaceCrossReference.Text.Trim
			If pnlInterfaceExchangeUnit.Visible Then bprodInterface.UnitId = Guid.Parse(ddlInterfaceUnit.SelectedValue)
			bprodInterface.DefaultSetting = chkDefaultSetting.Checked
			bprodInterface.ExportOnly = chkExportOnly.Checked
			bprodInterface.SqlInsert(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
			bprodInterfaceId = bprodInterface.Id
		Else
			Dim bprodInterface As KaBulkProductInterfaceSettings = New KaBulkProductInterfaceSettings(GetUserConnection(_currentUser.Id), bprodInterfaceId)
			bprodInterface.BulkProductId = Guid.Parse(ddlBulkProducts.SelectedValue)
			bprodInterface.InterfaceId = Guid.Parse(ddlInterface.SelectedValue)
			bprodInterface.CrossReference = tbxInterfaceCrossReference.Text.Trim
			If pnlInterfaceExchangeUnit.Visible Then bprodInterface.UnitId = Guid.Parse(ddlInterfaceUnit.SelectedValue)
			bprodInterface.DefaultSetting = chkDefaultSetting.Checked
			bprodInterface.ExportOnly = chkExportOnly.Checked
			bprodInterface.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
		End If

		PopulateBulkProductInterfaceList(Guid.Parse(ddlBulkProducts.SelectedValue))
		ddlBulkProductInterface.SelectedValue = bprodInterfaceId.ToString
		ddlBulkProductInterface_SelectedIndexChanged(ddlBulkProductInterface, New EventArgs)
		btnRemoveInterface.Enabled = True
	End Sub

	Private Sub RemoveInterface()
		Dim selectedId As Guid = Guid.Parse(ddlBulkProductInterface.SelectedValue)
		If selectedId <> Guid.Empty Then
			Dim prodInterfaceSetting As KaBulkProductInterfaceSettings = New KaBulkProductInterfaceSettings(GetUserConnection(_currentUser.Id), selectedId)
			prodInterfaceSetting.Deleted = True
			prodInterfaceSetting.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
		End If
		PopulateBulkProductInterfaceList(Guid.Parse(ddlBulkProducts.SelectedValue))
		btnRemoveInterface.Enabled = PopulateInterfaceInformation(Guid.Empty)
	End Sub

	Private Sub DeleteInterfaceInformation(ByVal bulkProductId As Guid)
		For Each r As KaBulkProductInterfaceSettings In KaBulkProductInterfaceSettings.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND bulk_product_id=" & Q(bulkProductId), "")
			r.Deleted = True
			r.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
		Next
	End Sub

	Private Sub PopulateBulkProductInterfaceList(ByVal bulkProdId As Guid)
		PopulateInterfaceList()
		ddlBulkProductInterface.Items.Clear()
		ddlBulkProductInterface.Items.Add(New ListItem(IIf(ddlInterface.Items.Count > 1, "Add an interface", ""), Guid.Empty.ToString))
		Dim getInterfaceRdr As OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(_currentUser.Id), "SELECT " & KaBulkProductInterfaceSettings.TABLE_NAME & ".id, interfaces.name, " & KaBulkProductInterfaceSettings.TABLE_NAME & ".cross_reference " &
				"FROM " & KaBulkProductInterfaceSettings.TABLE_NAME & " " &
				"INNER JOIN interfaces ON " & KaBulkProductInterfaceSettings.TABLE_NAME & ".interface_id = interfaces.id " &
				"INNER JOIN interface_types ON interfaces.interface_type_id = interface_types.id " &
				"WHERE (" & KaBulkProductInterfaceSettings.TABLE_NAME & ".deleted = 0) " &
					"AND (interfaces.deleted = 0) " &
					"AND (interface_types.deleted = 0) " &
					"AND (" & KaBulkProductInterfaceSettings.TABLE_NAME & ".bulk_product_id=" & Q(bulkProdId) & ") " &
				"ORDER BY interfaces.name, " & KaBulkProductInterfaceSettings.TABLE_NAME & ".cross_reference")
		Do While getInterfaceRdr.Read
			ddlBulkProductInterface.Items.Add(New ListItem(getInterfaceRdr.Item("name") & " (" & getInterfaceRdr.Item("cross_reference") & ")", getInterfaceRdr.Item("id").ToString()))
		Loop
		getInterfaceRdr.Close()
	End Sub

	Private Function PopulateInterfaceInformation(ByVal bProdInterfaceId As Guid) As Boolean
		Dim retval As Boolean = False
		If bProdInterfaceId <> Guid.Empty Then
			Dim bProdInterfaceSetting As KaBulkProductInterfaceSettings = New KaBulkProductInterfaceSettings(GetUserConnection(_currentUser.Id), bProdInterfaceId)
			ddlInterface.SelectedValue = bProdInterfaceSetting.InterfaceId.ToString
			tbxInterfaceCrossReference.Text = bProdInterfaceSetting.CrossReference
			ddlInterfaceUnit.SelectedValue = bProdInterfaceSetting.UnitId.ToString
			chkDefaultSetting.Checked = bProdInterfaceSetting.DefaultSetting
			chkExportOnly.Checked = bProdInterfaceSetting.ExportOnly
			retval = True
		Else
			ddlInterface.SelectedIndex = 0
			tbxInterfaceCrossReference.Text = ""
			Try
				Dim defaultMassUnit As String = KaUnit.GetSystemDefaultMassUnitOfMeasure(GetUserConnection(_currentUser.Id), Nothing).ToString()
				ddlInterfaceUnit.SelectedValue = defaultMassUnit
			Catch ex As Exception
				ddlInterfaceUnit.SelectedIndex = 0
			End Try
			ddlInterface_SelectedIndexChanged(ddlInterface, New EventArgs)
			chkExportOnly.Checked = False
			retval = False
		End If

		Return retval
	End Function
#End Region

#Region "Crop Types"
	Private Sub PopulateCropTypes()
		cblCropTypes.Items.Clear()
		Dim allCropTypes As ArrayList = KaCropType.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
		If allCropTypes.Count = 0 Then
			lblCropTypes.Visible = False
		Else
			lblCropTypes.Visible = True
		End If

		For Each r As KaCropType In allCropTypes
			cblCropTypes.Items.Add(New ListItem(r.Name, r.Id.ToString()))
		Next
	End Sub

	Private Sub DeleteCropTypeInformation(ByVal bulkProductId As Guid)
		For Each r As KaBulkProductCropType In KaBulkProductCropType.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND bulk_product_id=" & Q(bulkProductId), "")
			r.Deleted = True
			r.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
		Next
	End Sub

	Private Sub SaveCropTypeInformation(ByVal bulkProductId As Guid)
		Dim l As ArrayList = KaBulkProductCropType.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND bulk_product_id=" & Q(bulkProductId), "")
		For Each i As ListItem In cblCropTypes.Items
			Dim found As Boolean = False
			Dim cropTypeId As Guid = Guid.Parse(i.Value)
			For Each r As KaBulkProductCropType In l
				If r.CropTypeId = cropTypeId Then
					If i.Selected Then
						found = True
					Else ' bulk product/crop type exists and should not, delete it
						r.Deleted = True
						r.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
					End If
				End If
			Next
			If Not found And i.Selected Then ' bulk product/crop type does not exist and should, create it
				With New KaBulkProductCropType()
					.BulkProductId = bulkProductId
					.CropTypeId = cropTypeId
					.SqlInsert(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
				End With
			End If
		Next
	End Sub

	Private Sub PopulateCropTypeInformation(ByVal bulkProductId As Guid)
		If bulkProductId = Guid.Empty Then
			For Each i As ListItem In cblCropTypes.Items
				i.Selected = False
			Next
		Else
			Dim l As ArrayList = KaBulkProductCropType.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND bulk_product_id=" & Q(bulkProductId), "")
			For Each i As ListItem In cblCropTypes.Items
				Dim j As Integer = 0
				Do While j < l.Count
					If CType(l(j), KaBulkProductCropType).CropTypeId = Guid.Parse(i.Value) Then
						i.Selected = True
						Exit Do
					End If
					j += 1
				Loop
				If j = l.Count Then i.Selected = False
			Next
		End If
	End Sub
#End Region

	Private Sub PopulateBulkProductList()
		ddlBulkProducts.Items.Clear() ' populate the bulk products list
		If _currentUserPermission(_currentTableName).Create Then
			ddlBulkProducts.Items.Add(New ListItem("Enter a new bulk product", Guid.Empty.ToString()))
		Else
			ddlBulkProducts.Items.Add(New ListItem("Select a bulk product", Guid.Empty.ToString()))
		End If
		Dim conditions As String = String.Format("deleted=0" & IIf(_currentUser.OwnerId <> Guid.Empty, " AND (owner_id={0} OR owner_id={1})", ""), Q(_currentUser.OwnerId), Q(Guid.Empty))
		For Each p As KaBulkProduct In KaBulkProduct.GetAll(GetUserConnection(_currentUser.Id), conditions, "name ASC")
			If Not p.IsFunction(GetUserConnection(_currentUser.Id)) OrElse p.IsRinseFunction(GetUserConnection(_currentUser.Id)) Then
				ddlBulkProducts.Items.Add(New ListItem(p.Name, p.Id.ToString()))
			End If
		Next
	End Sub

	Private Sub PopulateOwnerList()
		ddlOwner.Items.Clear() ' populate the owners list
		ddlOwner.Items.Add(New ListItem("All owners", Guid.Empty.ToString()))
		For Each o As KaOwner In KaOwner.GetAll(GetUserConnection(_currentUser.Id), "deleted=0" & IIf(_currentUser.OwnerId = Guid.Empty, "", " AND id=" & Q(_currentUser.OwnerId)), "name ASC")
			ddlOwner.Items.Add(New ListItem(o.Name, o.Id.ToString()))
		Next
	End Sub

	Private Sub PopulateUnitLists()
		Dim showTime As Boolean = False
		Dim bulkProductId As Guid = Guid.Parse(ddlBulkProducts.SelectedValue)
		If bulkProductId <> Guid.Empty Then
			Try
				Dim bulkProduct As New KaBulkProduct(GetUserConnection(_currentUser.Id), bulkProductId)
				showTime = bulkProduct.IsTimedFunction(GetUserConnection(_currentUser.Id))
			Catch ex As RecordNotFoundException ' suppress exception
			End Try
		End If
		ddlUnit.Items.Clear()
		ddlUnit.Items.Add(New ListItem("", Guid.Empty.ToString()))
		ddlUnitOfWeight.Items.Clear()
		ddlUnitOfVolume.Items.Clear()
		ddlInterfaceUnit.Items.Clear()
		ddlInterfaceUnit.Items.Add(New ListItem("", Guid.Empty.ToString()))
		For Each u As KaUnit In KaUnit.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
			If showTime Xor Not KaUnit.IsTime(u.BaseUnit) Then
				ddlUnit.Items.Add(New ListItem(u.Name, u.Id.ToString()))
				ddlInterfaceUnit.Items.Add(New ListItem(u.Name, u.Id.ToString()))
			End If
			If KaUnit.IsWeight(u.BaseUnit) Then
				ddlUnitOfWeight.Items.Add(New ListItem(u.Abbreviation, u.Id.ToString()))
			ElseIf Not KaUnit.IsTime(u.BaseUnit) Then
				ddlUnitOfVolume.Items.Add(New ListItem(u.Abbreviation, u.Id.ToString()))
			End If
		Next
	End Sub

	Private Function PopulateBulkProductInformation(ByVal bulkProductId As Guid) As Boolean
		_customFieldData.Clear()
		Dim bulkProductInfo As KaBulkProduct
		Try
			bulkProductInfo = New KaBulkProduct(GetUserConnection(_currentUser.Id), bulkProductId)
		Catch ex As RecordNotFoundException
			bulkProductInfo = New KaBulkProduct()
			With bulkProductInfo
				.Name = ""
				.Density = "0"
				ddlUnit.SelectedValue = Guid.Empty.ToString()
				.WeightUnitId = KaUnit.GetSystemDefaultMassUnitOfMeasure(GetUserConnection(_currentUser.Id), Nothing)
				.VolumeUnitId = KaUnit.GetSystemDefaultVolumeUnitOfMeasure(GetUserConnection(_currentUser.Id), Nothing)
				.EpaNumber = ""
				.Barcode = ""
				.OwnerId = Utilities.GetUser(Me).OwnerId
				.Notes = ""
			End With
		End Try

		With bulkProductInfo
			tbxName.Text = .Name
			Try
				ddlOwner.SelectedValue = .OwnerId.ToString()
			Catch ex As ArgumentOutOfRangeException
				ddlOwner.SelectedIndex = 0
				DisplayJavaScriptMessage("InvalidOwnerId", Utilities.JsAlert("Record not found in owners where ID = " & .OwnerId.ToString() & ". Owner set to ""all owners"" instead."))
			End Try
			tbxDensity.Text = .Density
			tbxEPA.Text = .EpaNumber
			tbxBarcode.Text = .Barcode
			Try
				ddlUnit.SelectedValue = .DefaultUnitId.ToString()
			Catch ex As ArgumentOutOfRangeException
				ddlUnit.SelectedIndex = 0
				DisplayJavaScriptMessage("InvalidUnitId", Utilities.JsAlert("Record not found in units where ID = " & .DefaultUnitId.ToString() & ". Default unit set to blank instead."))
			End Try
			Try
				ddlUnitOfWeight.SelectedValue = .WeightUnitId.ToString()
			Catch ex As ArgumentOutOfRangeException
				ddlUnitOfWeight.SelectedIndex = 0
				DisplayJavaScriptMessage("InvalidWeightUnitId", Utilities.JsAlert("Record not found in units where ID = " & .WeightUnitId.ToString() & "."))
			End Try
			Try
				ddlUnitOfVolume.SelectedValue = .VolumeUnitId.ToString()
			Catch ex As ArgumentOutOfRangeException
				ddlUnitOfVolume.SelectedIndex = 0
				DisplayJavaScriptMessage("InvalidVolumeUnitId", Utilities.JsAlert("Record not found in units where ID = " & .VolumeUnitId.ToString() & "."))
			End Try
			PopulateBulkProductInterfaceList(.Id)
			btnRemoveInterface.Enabled = PopulateInterfaceInformation(Guid.Parse(ddlBulkProductInterface.SelectedValue))
			PopulateCropTypeInformation(.Id)
			tbxNotes.Text = .Notes

			_derivedFromEntries = .DerivedFrom
			PopulateDerivedEntries()

			If Tm2Database.SystemItemTraceabilityEnabled Then
				Try
					rblLotUsageTrackingType.SelectedIndex = .LotUsageMethod
				Catch ex As ArgumentOutOfRangeException
					rblLotUsageTrackingType.SelectedIndex = 0
				End Try
			End If

			For Each customFieldValue As KaCustomFieldData In KaCustomFieldData.GetAll(GetUserConnection(_currentUser.Id), String.Format("deleted = 0 AND {0} = {1}", KaCustomFieldData.FN_RECORD_ID, Q(.Id)), KaCustomFieldData.FN_LAST_UPDATED)
				_customFieldData.Add(customFieldValue)
			Next
		End With
		Utilities.CreateDynamicCustomFieldPanelControls(_customFields, _customFieldData, lstCustomFields, Page)

		Return Not bulkProductInfo.Id.Equals(Guid.Empty)
	End Function

	Private Sub PopulateDerivedEntries()
		Dim currentDerivedEntry As String = ""
		If lstDerivedFrom.SelectedIndex >= 0 Then currentDerivedEntry = lstDerivedFrom.SelectedItem.Value
		lstDerivedFrom.Items.Clear()

		lstDerivedFrom.SelectedIndex = -1
		_derivedFromEntries.Sort(StringComparer.OrdinalIgnoreCase)
		For Each derivedFrom As String In _derivedFromEntries
			lstDerivedFrom.Items.Add(New ListItem(IIf(derivedFrom.Trim.Length = 0, "(empty)", derivedFrom.Trim), derivedFrom.Trim))
			If currentDerivedEntry = derivedFrom Then lstDerivedFrom.SelectedIndex = lstDerivedFrom.Items.Count - 1
		Next
		lstDerivedFrom_SelectedIndexChanged(lstDerivedFrom, Nothing)
	End Sub

	Private Function ConvertPageToDerivedEntries() As List(Of String)
		Dim retval As List(Of String) = New List(Of String)
		For Each entry As ListItem In lstDerivedFrom.Items
			If Not retval.Contains(entry.Value.Trim, StringComparer.OrdinalIgnoreCase) Then retval.Add(entry.Value.Trim)
		Next
		Return retval
	End Function

	Private Sub SaveBulkProduct(ByRef bulkProductId As Guid)
		If ValidateFields(bulkProductId) Then
			Dim newBulkProdAdded As Boolean = False
			With New KaBulkProduct()
				.Id = bulkProductId
				.EpaNumber = tbxEPA.Text
				.Barcode = tbxBarcode.Text
				.DefaultUnitId = Guid.Parse(ddlUnit.SelectedValue)
				.Density = tbxDensity.Text
				.Name = tbxName.Text
				.Notes = tbxNotes.Text.Trim
				.OwnerId = Guid.Parse(ddlOwner.SelectedValue)
				.VolumeUnitId = Guid.Parse(ddlUnitOfVolume.SelectedValue)
				.WeightUnitId = Guid.Parse(ddlUnitOfWeight.SelectedValue)
				.DerivedFrom = ConvertPageToDerivedEntries()
				If Tm2Database.SystemItemTraceabilityEnabled AndAlso rblLotUsageTrackingType.SelectedIndex >= 0 Then .LotUsageMethod = rblLotUsageTrackingType.SelectedIndex
				Dim status As String = ""
				If .Id = Guid.Empty Then
					.SqlInsert(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
					status = "Bulk product added successfully"
					newBulkProdAdded = True
				Else
					.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)

					'Update all containers that have this Bulk Product selected
					If .EpaNumber.Trim.Length > 0 Then
						Dim allContainers As ArrayList = KaContainer.GetAll(GetUserConnection(_currentUser.Id), "bulk_product_id = " & Q(.Id), "")
						For Each container As KaContainer In allContainers
							container.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
						Next
					End If

					status = "Bulk product updated successfully"
				End If
				bulkProductId = .Id
				SaveCropTypeInformation(bulkProductId)

				If newBulkProdAdded Then
					'Check if we want to add a Product for this Bulk Product
					Dim javaScript As String =
								  "<script language='JavaScript'>" &
								  "if (confirm('Do you want to create new Product(s) for this Bulk Product?') == true )" &
								  "{" &
								  ClientScript.GetPostBackEventReference(Me, "SaveNewProductForBulkProduct") &
								  "}" &
								  "</script>"
					DisplayJavaScriptMessage("ConfirmScript", javaScript)
				End If

				Utilities.ConvertCustomFieldPanelToLists(_customFields, _customFieldData, lstCustomFields)
				For Each customData As KaCustomFieldData In _customFieldData
					customData.RecordId = .Id
					customData.SqlUpdateInsertIfNotFound(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
				Next

				PopulateBulkProductList()
				ddlBulkProducts.SelectedValue = .Id.ToString
				ddlBulkProducts_SelectedIndexChanged(ddlBulkProducts, New EventArgs())
				lblStatus.Text = status
			End With
		End If
	End Sub

	Private Function ValidateFields(ByVal bulkProductId As Guid) As Boolean
		If tbxName.Text.Length = 0 Then DisplayJavaScriptMessage("InvalidName", Utilities.JsAlert("Name must be entered")) : Return False
		Dim density As Double = 0.0
		If Not Double.TryParse(tbxDensity.Text, density) Then DisplayJavaScriptMessage("InvalidDensity", Utilities.JsAlert("Density must be a numeric value")) : Return False
		If bulkProductId = Guid.Empty AndAlso KaBulkProduct.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND name=" & Q(tbxName.Text), "").Count > 0 Then _
				DisplayJavaScriptMessage("InvalidDuplicateName", Utilities.JsAlert("A bulk product already exists with the name specified.")) : Return False
		If ddlUnit.SelectedIndex = 0 Then DisplayJavaScriptMessage("InvalidUnit", Utilities.JsAlert("A default unit of measure must be selected.")) : Return False
		If density <= 0.0 Then
			Try
				Dim unitInfo As New KaUnit(GetUserConnection(_currentUser.Id), Guid.Parse(ddlUnit.SelectedValue))
				If Not KaUnit.IsWeight(unitInfo.BaseUnit) AndAlso Not KaUnit.IsTime(unitInfo.BaseUnit) Then DisplayJavaScriptMessage("InvalidDensityAmount", Utilities.JsAlert("The density must be set to a value greater than 0.")) : Return False
			Catch ex As RecordNotFoundException

			End Try
		End If
		Return True
	End Function

	Private Sub SaveNewProductForBulkProduct()
		Try
			Dim newBulkProd As KaBulkProduct = New KaBulkProduct(GetUserConnection(_currentUser.Id), Guid.Parse(ddlBulkProducts.SelectedValue))
			Dim newProd As KaProduct = New KaProduct
			With newProd
				.Name = newBulkProd.Name
				.DefaultUnitId = newBulkProd.DefaultUnitId
				.OwnerId = newBulkProd.OwnerId
				.EpaNumber = newBulkProd.EpaNumber
				.Deleted = False
				.SqlInsert(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
			End With
			For Each facility As KaLocation In KaLocation.GetAll(GetUserConnection(_currentUser.Id), "deleted = " & Q(False), "")
				Dim newProdBulkProd As KaProductBulkProduct = New KaProductBulkProduct()
				With newProdBulkProd
					.ProductId = newProd.Id
					.LocationId = facility.Id
					.BulkProductId = newBulkProd.Id
					.Portion = 100
					.Position = 0
					.Deleted = False
					.SqlInsert(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
				End With
			Next
			DisplayJavaScriptMessage("ProductAdded", Utilities.JsAlert("Product successfully added for this Bulk Product."))
		Catch ex As Exception
			DisplayJavaScriptMessage("ProductNotAdded", Utilities.JsAlert("Product failed to add.  You must manually create the Product for this Bulk Product."))
		End Try
	End Sub

	Private Function DeleteBulkProduct(ByVal bulkProductId As Guid) As Boolean
		' find all references to this product
		Dim containers As ArrayList = KaContainer.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND bulk_product_id=" & Q(bulkProductId), "")
		Dim productBulkProducts As ArrayList = KaProductBulkProduct.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND bulk_product_id=" & Q(bulkProductId), "")
		Dim receivingPurchaseOrders As ArrayList = KaReceivingPurchaseOrder.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND bulk_product_id=" & Q(bulkProductId), "")
		Dim tanks As ArrayList = KaTank.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND bulk_product_id=" & Q(bulkProductId), "")
		If containers.Count > 0 OrElse productBulkProducts.Count > 0 OrElse receivingPurchaseOrders.Count > 0 OrElse tanks.Count > 0 Then
			Dim warning As String = "Warning: this bulk product is associated with other records:\n"
			If containers.Count > 0 Then warning &= "\n" & "Containers:\n\n"
			For Each container As KaContainer In containers
				warning &= container.Number & "\n"
			Next
			If productBulkProducts.Count > 0 Then warning &= "\n" & "Products:\n"
			For Each productBulkProduct As KaProductBulkProduct In productBulkProducts
				Dim product As KaProduct = New KaProduct(GetUserConnection(_currentUser.Id), productBulkProduct.ProductId)
				Dim facility As KaLocation = New KaLocation(GetUserConnection(_currentUser.Id), productBulkProduct.LocationId)
				warning &= product.Name & "(" & facility.Name & ")" & "\n"
			Next
			If receivingPurchaseOrders.Count > 0 Then warning &= "\nReceiving purchase orders:\n"
			For Each receivingPurchaseOrder As KaReceivingPurchaseOrder In receivingPurchaseOrders
				warning &= receivingPurchaseOrder.Number
			Next
			If tanks.Count > 0 Then warning &= "\nTanks:\n"
			For Each tank As KaTank In tanks
				warning &= tank.Name & "\n"
			Next
			warning &= "\nRemove these associations before deleting this bulk product."
			DisplayJavaScriptMessage("InvalidBulkProductUsed", Utilities.JsAlert(warning))
			Return False
		Else
			With New KaBulkProduct(GetUserConnection(_currentUser.Id), bulkProductId)
				.Deleted = True
				.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
				DeleteCropTypeInformation(bulkProductId)
				DeleteInterfaceInformation(bulkProductId)
				For Each r As KaBulkProductInventory In KaBulkProductInventory.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND bulk_product_id=" & Q(bulkProductId), "")
					r.Deleted = True
					r.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
				Next
				For Each r As KaBulkProductPanelSettings In KaBulkProductPanelSettings.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND bulk_product_id=" & Q(bulkProductId), "")
					r.Deleted = True
					r.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
				Next
			End With
			lblStatus.Text = "Selected bulk product deleted successfully"
			Return True
		End If
	End Function

	Private Sub SetTextboxMaxLengths()
		tbxBarcode.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaBulkProduct.TABLE_NAME, "barcode"))
		tbxInterfaceCrossReference.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaBulkProductInterfaceSettings.TABLE_NAME, "cross_reference"))
		tbxDensity.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaBulkProduct.TABLE_NAME, "density"))
		tbxEPA.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaBulkProduct.TABLE_NAME, "epa_number"))
		tbxName.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaBulkProduct.TABLE_NAME, "name"))
	End Sub

	Private Sub ddlInterface_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles ddlInterface.SelectedIndexChanged
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		If Guid.Parse(ddlBulkProductInterface.SelectedValue) = Guid.Empty Then
			'Only do this check if we are a new interface setting
			Dim count As Integer = 0
			Try
				Dim rdr As OleDb.OleDbDataReader = Tm2Database.ExecuteReader(connection, "SELECT count(*) " &
																			 "FROM " & KaBulkProductInterfaceSettings.TABLE_NAME & " " &
																			 "WHERE " & KaBulkProductInterfaceSettings.FN_DELETED & " = 0 " &
																			 "AND " & KaBulkProductInterfaceSettings.FN_INTERFACE_ID & " = " & Q(Guid.Parse(ddlInterface.SelectedValue)) & " " &
																			 "AND " & KaBulkProductInterfaceSettings.FN_BULK_PRODUCT_ID & " = " & Q(Guid.Parse(ddlBulkProducts.SelectedValue)))
				If rdr.Read Then count = rdr.Item(0)
			Catch ex As Exception

			End Try
			chkDefaultSetting.Checked = (count = 0)
		End If

		Try
			Dim interfaceId As Guid = Guid.Empty
			Guid.TryParse(ddlInterface.SelectedValue, interfaceId)
			Dim interfaceTypeInfo As New KaInterfaceTypes(connection, New KaInterface(connection, interfaceId).InterfaceTypeId)
			pnlInterfaceExchangeUnit.Visible = interfaceTypeInfo.ShowInterfaceExchangeUnit
		Catch ex As RecordNotFoundException
			pnlInterfaceExchangeUnit.Visible = True
		End Try
	End Sub

	Protected Overrides Function SaveViewState() As Object
		Dim viewState(3) As Object

		Utilities.ConvertCustomFieldPanelToLists(_customFields, _customFieldData, lstCustomFields)
		viewState(0) = MyBase.SaveViewState()
		viewState(1) = _customFields
		viewState(2) = _customFieldData
		viewState(3) = ConvertPageToDerivedEntries()
		Return viewState
	End Function

	Protected Overrides Sub LoadViewState(savedState As Object)
		If savedState IsNot Nothing AndAlso CType(savedState, Object).Length > 1 Then
			Dim viewState As Object() = savedState
			MyBase.LoadViewState(viewState(0))
			_customFields = viewState(1)
			_customFieldData = viewState(2)
			Utilities.CreateDynamicCustomFieldPanelControls(_customFields, _customFieldData, lstCustomFields, Page)
			_derivedFromEntries = viewState(3)
			PopulateDerivedEntries()
		Else
			MyBase.LoadViewState(savedState)
		End If
	End Sub

	Private Sub SetControlUsabilityFromPermissions()
		With _currentUserPermission(_currentTableName)
			Dim shouldEnable = (.Edit AndAlso ddlBulkProducts.SelectedIndex > 0) OrElse (.Create AndAlso ddlBulkProducts.SelectedIndex = 0)
			pnlGeneralInformation.Enabled = shouldEnable
			pnlInterfaceSettings.Enabled = shouldEnable
			btnSave.Enabled = shouldEnable
			Dim value = Guid.Parse(ddlBulkProducts.SelectedValue)
			btnDelete.Enabled = .Edit AndAlso .Delete AndAlso value <> Guid.Empty
		End With
	End Sub

	Protected Sub ToolkitScriptManager1_AsyncPostBackError(ByVal sender As Object, ByVal e As System.Web.UI.AsyncPostBackErrorEventArgs)
		ToolkitScriptManager1.AsyncPostBackErrorMessage = e.Exception.Message
	End Sub

	Private Sub DisplayJavaScriptMessage(key As String, script As String)
		ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), key, script, False)
	End Sub
End Class
