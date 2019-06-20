Imports KahlerAutomation.KaTm2Database
Imports System.Environment
Imports System.Data.OleDb

Public Class Containers : Inherits System.Web.UI.Page
	Private _currentUser As KaUser
	Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
	Private _currentTableName As String = KaContainer.TABLE_NAME

#Region "Events"
	Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
		Response.Cache.SetCacheability(HttpCacheability.NoCache)
		_currentUser = Utilities.GetUser(Me)
		_currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Containers")
		If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")
		lblStatus.Text = ""
		If Not Page.IsPostBack Then
			SetTextboxMaxLengths()
			PopulateFacilityList()
			PopulateContainersList(_currentUser)
			PopulateContainerTypeList()
			PopulateConditionList()
			PopulateOwnerList(_currentUser)
			PopulateLocationList()
			PopulateStatusList()
			PopulateUnitsList()
			PopulateBulkProductList(_currentUser, Guid.Parse(ddlOwner.SelectedValue))
			SetControlUsabilityFromPermissions()
			pnlLot.Visible = Tm2Database.SystemItemTraceabilityEnabled
			pnlNewLot.Visible = False
			PopulateContainerInformation(Guid.Empty)
			btnDelete.Enabled = False
			btnClearLastTicket.Enabled = False

			If Request.QueryString("id") IsNot Nothing Then
				ddlFacilityFilter.SelectedIndex = 0

				Dim containerId As Guid = Guid.Parse(Request.QueryString("id"))
				If containerId <> Guid.Empty Then
					SetSelectedContainer(containerId)
				End If
				Utilities.RemoveQueryString(Request.QueryString, "id")
			Else
				Try
					ddlFacilityFilter.SelectedValue = KaSetting.GetSetting(GetUserConnection(_currentUser.Id), "ContainersPage:" & _currentUser.Id.ToString & "/LastFacilityUsed", Guid.Empty.ToString())
					ddlFacilityFilter_SelectedIndexChanged(Nothing, Nothing)
				Catch ex As ArgumentOutOfRangeException
					'Suppress
				End Try
			End If

			Utilities.SetFocus(tbxNumber, Me) ' set focus to the first textbox on the page
		End If
	End Sub

	Private Sub SetSelectedContainer(ByVal containerId As Guid)
		ddlContainers.SelectedValue = containerId.ToString
		PopulateContainerInformation(containerId)
		btnDelete.Enabled = containerId <> Guid.Empty AndAlso _currentUserPermission(_currentTableName).Delete
		btnClearLastTicket.Enabled = containerId <> Guid.Empty
	End Sub

	Private Sub ddlContainers_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlContainers.SelectedIndexChanged
		Dim containerId As Guid = Guid.Parse(ddlContainers.SelectedValue)
		PopulateContainerInformation(containerId)
	End Sub

	Protected Sub ddlWeight_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles ddlWeight.SelectedIndexChanged
		Try
			Dim unitId As Guid = Guid.Parse(ddlWeight.SelectedValue)
			If unitId <> Guid.Empty Then lblProductWeight.Text = New KaUnit(GetUserConnection(_currentUser.Id), unitId).Abbreviation Else lblProductWeight.Text = ""
		Catch ex As Exception
			lblProductWeight.Text = ""
		End Try
	End Sub

	Protected Sub btnClearLastTicket_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnClearLastTicket.Click
		With New KaContainer(GetUserConnection(_currentUser.Id), Guid.Parse(ddlContainers.SelectedValue))
			.LastTicketId = Guid.Empty
			.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
			lblStatus.Text = "Last ticket successfully cleared."
		End With
	End Sub

	Protected Sub btnDelete_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnDelete.Click
		Dim containerId As Guid = Guid.Parse(ddlContainers.SelectedValue)
		If btnDelete.Text = "Undelete" Then
			PopulateContainerInformation(containerId)
			If KaContainer.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND id<>" & Q(Guid.Parse(ddlContainers.SelectedValue)) & " AND number=" & Q(tbxNumber.Text), "").Count > 0 Then
				DisplayJavaScriptMessage("InvalidNumberAlreadyUsed", Utilities.JsAlert("The number specified for this container has already been used for another container. Unable to undelete the container."))
				Exit Sub ' the user must correct this problem before we can save the container
			End If
			If Not FormDataValid() Then Exit Sub

			With New KaContainer(GetUserConnection(_currentUser.Id), containerId)
				.Deleted = False
				.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
				lblStatus.Text = "Container successfully undeleted."
			End With
		Else
			With New KaContainer(GetUserConnection(_currentUser.Id), containerId)
				.Deleted = True
				.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
				lblStatus.Text = "Container successfully deleted."
				containerId = Guid.Empty
			End With
		End If
		PopulateContainersList(Utilities.GetUser(Me))
		Try
			ddlContainers.SelectedValue = containerId.ToString()
		Catch ex As Exception
			Try
				ddlContainers.SelectedValue = Guid.Empty.ToString()
			Catch ex2 As Exception
				If ddlContainers.Items.Count > 0 Then ddlContainers.SelectedIndex = 0
			End Try
		End Try
		PopulateContainerInformation(containerId)
	End Sub

	Private Function FormDataValid() As Boolean
		If tbxNumber.Text.Trim().Length = 0 Then ' make sure the user has specified a number for this container
			DisplayJavaScriptMessage("InvalidNumber", Utilities.JsAlert("Please specify a number for this container."))
			Return False ' the user must correct this problem before we can save the container
		End If

		' ensure that the number specified is unique
		If KaContainer.GetAll(GetUserConnection(_currentUser.Id), "deleted=0 AND id<>" & Q(Guid.Parse(ddlContainers.SelectedValue)) & " AND number=" & Q(tbxNumber.Text), "").Count > 0 Then
			DisplayJavaScriptMessage("InvalidNumberAlreadyUsed", Utilities.JsAlert("The number specified for this container has already been used for another container. Please specify a unique number for this container."))
			Return False ' the user must correct this problem before we can save the container
		End If
		If Guid.Parse(ddlVolume.SelectedValue) = Guid.Empty Then ' make sure the user has selected a unit of measure for the container's volume
			DisplayJavaScriptMessage("InvalidVolumeUnit", Utilities.JsAlert("Please select a unit of measure for the container's volume."))
			Return False ' the user must correct this problem before we can save the container
		End If
		If Guid.Parse(ddlWeight.SelectedValue) = Guid.Empty Then ' make sure the user has selected a unit of measure for the container's empty weight
			DisplayJavaScriptMessage("InvalidWeightUnit", Utilities.JsAlert("Please select a unit of measure for the container's empty weight."))
			Return False ' the user must correct this problem before we can save the container
		End If
		If ddlLot.SelectedIndex = 1 AndAlso ddlLot.SelectedValue = "New" Then
			If tbxNewLotNumber.Text.Trim().Length = 0 Then
				DisplayJavaScriptMessage("InvalidLotNumber", Utilities.JsAlert("New lot number must be specified."))
				Return False
			End If
		End If
		Dim controls As New Dictionary(Of String, Control) ' check the validity of the numeric fields
		controls("volume") = tbxVolume
		controls("empty weight") = tbxEmptyWeight
		controls("product weight") = tbxProductWeight
		For Each key As String In controls.Keys
			Try ' to parse the control's value as a number...
				Dim value As Double = Double.Parse(CType(controls(key), TextBox).Text)
				If value < 0.0 Then ' the user has entered a negative value, which is not allowed
					DisplayJavaScriptMessage(key.Replace(" ", ""), Utilities.JsAlert(String.Format("Please enter a value greater than or equal to zero for {0}.", key)))
					Return False ' the user must correct this problem before we can save the container
				End If
			Catch ex As FormatException ' the user has entered an non-numeric value...
				DisplayJavaScriptMessage(key.Replace(" ", ""), Utilities.JsAlert(String.Format("Please enter a numeric value for {0}.", key)))
				Return False ' the user must correct this problem before we can save the container
			End Try
		Next
		controls.Clear() ' check the validity of the date fields
		controls("in service") = tbxInServiceDate
		controls("last inspected") = tbxLastInspectedDate
		controls("manufactured") = tbxManufacturedDate
		controls("last filled") = tbxLastFilledDate
		controls("last cleaned") = tbxLastCleanedDate
		For Each key As String In controls.Keys
			Try ' to parse the control's value as a date...
				Dim value As DateTime = DateTime.Parse(CType(controls(key), HtmlInputText).Value)
				If value < SQL_MINDATE Then ' SQL doesn't allow dates this old
					DisplayJavaScriptMessage(key.Replace(" ", ""), Utilities.JsAlert(String.Format("Please enter a date no earlier than {0:M/d/yyyy} for the container's {1} date.", SQL_MINDATE, key)))
					Return False ' the user must correct this problem before we can save the container
				End If
			Catch ex As FormatException ' the user has entered a value that isn't in a recognized date format...
				DisplayJavaScriptMessage(key.Replace(" ", ""), Utilities.JsAlert(String.Format("Please enter a date in the M/d/yyyy format for the container's {0} date.", key)))
				Return False ' the user must correct this problem before we can save the container
			End Try
		Next
		If Not VerifyContainerOwnerProductAvailability(Guid.Parse(ddlOwner.SelectedValue), Guid.Parse(ddlBulkProduct.SelectedValue)) Then
			DisplayJavaScriptMessage("InvalidBulkProductOwner", Utilities.JsAlert("The bulk product selected for this container is not available to the owner selected for this container (likely caused by the bulk product record having an owner selected)."))
			Return False ' the user must correct this problem before we can save the container
		End If

		Dim origContainer As KaContainer
		Try
			origContainer = New KaContainer(GetUserConnection(_currentUser.Id), Guid.Parse(ddlContainers.SelectedValue))
		Catch ex As Exception
			origContainer = New KaContainer()
		End Try
		'warn the user if the number has changed, and it is assigned to a deleted container
		If tbxNumber.Text.Trim.ToUpper <> origContainer.Number.Trim.ToUpper AndAlso KaContainer.GetAll(GetUserConnection(_currentUser.Id), "deleted=1 AND id<>" & Q(Guid.Parse(ddlContainers.SelectedValue)) & " AND number=" & Q(tbxNumber.Text), "").Count > 0 Then ' the user should be notified
			DisplayJavaScriptMessage("InvalidNumberAlreadyUsed", Utilities.JsAlert("The number specified for this container has already been used for another container, but that container has been deleted. Please verify that this container is assigned the correct number."))
		End If

		Return True ' everything looks OK
	End Function

	Private Class ProductChangedException : Inherits Exception
		Public Sub New(message As String)
			MyBase.New(message)
		End Sub
	End Class

	Private Class ProductWeightChangedException : Inherits Exception
		Public Sub New(message As String)
			MyBase.New(message)
		End Sub
	End Class

	Private Sub Save(ignoreProductChange As Boolean, ByRef message As String)
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		message = ""
		With ConvertPageToContainerObject(connection)
			Dim previousContainer As KaContainer
			Try
				previousContainer = New KaContainer(connection, .Id, Nothing)
			Catch ex As RecordNotFoundException
				previousContainer = New KaContainer()
				previousContainer.Id = .Id
				previousContainer.WeightUnitId = .WeightUnitId
			End Try
			Dim bulkProductId As Guid = .BulkProductId

			If Not ignoreProductChange Then ' determine if we need to shift inventory due to a product/product weight change
				If (Not previousContainer.BulkProductId.Equals(.BulkProductId) OrElse Not previousContainer.OwnerId.Equals(.OwnerId)) AndAlso (previousContainer.ProductWeight > 0.0 OrElse .ProductWeight > 0.0) Then
					Dim warnings As List(Of String) = New List(Of String)
					Dim details As String
					If (previousContainer.BulkProductId <> .BulkProductId) Then
						details = "The bulk product for the container has changed"
						Try ' to get the name of the bulk product that was selected...
							Dim bulkProduct As New KaBulkProduct(connection, previousContainer.BulkProductId)
							details &= " from " & bulkProduct.Name
						Catch ex As RecordNotFoundException ' the name isn't available...
						End Try
						Try ' to get the name of the bulk product that has been selected...
							Dim bulkProduct As New KaBulkProduct(connection, .BulkProductId)
							details &= " to " & bulkProduct.Name
						Catch ex As RecordNotFoundException ' the name isn't available...
						End Try
						warnings.Add(details & ".")
					End If
					If (previousContainer.OwnerId <> .OwnerId) Then
						details = "The owner for the container has changed"
						Try ' to get the name of the bulk product that was selected...
							Dim owner As New KaOwner(connection, previousContainer.OwnerId)
							details &= " from " & owner.Name
						Catch ex As RecordNotFoundException ' the name isn't available...
							details &= " from Unassigned"
						End Try
						Try ' to get the name of the bulk product that has been selected...
							Dim owner As New KaOwner(connection, .OwnerId)
							details &= " to " & owner.Name
						Catch ex As RecordNotFoundException ' the name isn't available...
							details &= " to Unassigned"
						End Try
						warnings.Add(details & ".")
					End If

					details = String.Join(" ", warnings) & " If the previous product has been returned to bulk storage and replaced with the new product inventory should be updated to reflect the movement of product. If this is the case, please select the default owner and inventory locations to use for the inventory adjustments. Otherwise, click ""Save Without Inventory Adjustment"" to save the changes to the container record without making the corresponding inventory adjustments."
					Throw New ProductChangedException(details)
				ElseIf previousContainer.ProductWeight <> .ProductWeight OrElse previousContainer.WeightUnitId <> .WeightUnitId Then
					Dim details As String = "The product weight specified for the container has changed"
					Try ' to get the unit of measure of the previous product weight...
						Dim unit As New KaUnit(connection, previousContainer.WeightUnitId)
						details &= String.Format(" from {0} {1}", previousContainer.ProductWeight.ToString(), unit.Abbreviation)
					Catch ex As RecordNotFoundException
					End Try
					Try ' to get the unit of measure of the new product weight...
						Dim unit As New KaUnit(connection, .WeightUnitId)
						details &= String.Format(" to {0} {1}", .ProductWeight.ToString(), unit.Abbreviation)
					Catch ex As RecordNotFoundException
					End Try
					details &= ". If the change in product weight was due to physically adding/removing product from the container then bulk product inventory should be updated to reflect the movement of product. If this is the case, please select the default owner and inventory locations to use for the inventory adjustments. Otherwise, click ""Save Without Inventory Adjustment"" to save the changes to the container record without making the corresponding inventory adjustments."
					Throw New ProductWeightChangedException(details)
				End If
			End If
			If Tm2Database.SystemItemTraceabilityEnabled Then
				If ddlLot.SelectedIndex = 1 AndAlso ddlLot.SelectedValue = "New" Then
					Dim newLot As KaLot = New KaLot() With {
							.Number = tbxNewLotNumber.Text.Trim(),
							.BulkProductId = bulkProductId
						}
					newLot.SqlInsert(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
					.LotId = newLot.Id
				Else
					Guid.TryParse(ddlLot.SelectedValue, .LotId)
				End If
			End If

			If .Id <> Guid.Empty Then
				message = "Container successfully updated."
			Else
				message = "Container successfully added."
			End If
			.SqlUpdateInsertIfNotFound(connection, Nothing, Database.ApplicationIdentifier, _currentUser.Name)
			If Tm2Database.SystemItemTraceabilityEnabled AndAlso Not (previousContainer.LotId.Equals(.LotId)) AndAlso .StorageLocation IsNot Nothing Then
				Dim changeTime As DateTime = DateTime.Now
				Dim slm As KaStorageLocationMovement = New KaStorageLocationMovement
				slm.ContainerId = .Id
				slm.LotAssignment = True
				slm.LotId = .LotId
				slm.StartDate = changeTime
				slm.StopDate = changeTime
				slm.StorageLocationId = .StorageLocation.Id
				slm.TransferFromStorageLocationId = Guid.Empty
				slm.SqlInsert(connection, Nothing, ApplicationIdentifier, UserName)
			End If
			PopulateContainersList(Utilities.GetUser(Me))
			Try
				ddlContainers.SelectedValue = .Id.ToString()
				PopulateContainerInformation(.Id)
			Catch ex As Exception
			End Try
		End With
	End Sub

	Private Function ConvertPageToContainerObject(connection As OleDbConnection) As KaContainer
		Dim container As KaContainer = New KaContainer()
		With container
			.Id = Guid.Parse(ddlContainers.SelectedValue)
			Try
				If .Id <> Guid.Empty Then .SqlSelect(connection)
			Catch ex As RecordNotFoundException

			End Try
			Dim bulkProductId As Guid = Guid.Parse(ddlBulkProduct.SelectedValue)
			.BulkProductId = bulkProductId
			.ProductWeight = Double.Parse(tbxProductWeight.Text)
			.Number = tbxNumber.Text
			.ContainerTypeId = Guid.Parse(ddlContainerType.SelectedValue)
			.Volume = Double.Parse(tbxVolume.Text)
			.VolumeUnitId = Guid.Parse(ddlVolume.SelectedValue)
			.EmptyWeight = Double.Parse(tbxEmptyWeight.Text)
			.WeightUnitId = Guid.Parse(ddlWeight.SelectedValue)
			.Condition = ddlCondition.SelectedValue
			.SealNumber = tbxSealNumber.Text
			.Notes = tbxNotes.Text
			.InService = DateTime.Parse(tbxInServiceDate.Value)
			.LastInspected = DateTime.Parse(tbxLastInspectedDate.Value)
			.PassedInspection = rblPassedInspection.SelectedValue <> "No"
			.Manufactured = DateTime.Parse(tbxManufacturedDate.Value)
			.OwnerId = Guid.Parse(ddlOwner.SelectedValue)
			.LocationId = Guid.Parse(ddlLocation.SelectedValue)
			.Status = ddlStatus.SelectedValue
			.LastFilled = DateTime.Parse(tbxLastFilledDate.Value)
			.LastCleaned = DateTime.Parse(tbxLastCleanedDate.Value)
			.Refillable = rblRefillable.SelectedValue <> "No"
			.PassedPressureTest = rblPassedPressureTest.SelectedValue <> "No"
			.SealBroken = rblSealBroken.SelectedValue <> "No"
			.OneWayValvePresent = rblOneWayValvePresent.SelectedValue <> "No"
			.LastUserId = Utilities.GetUser(Me).Id
			.ForOrderId = Guid.Parse(ddlOrders.SelectedValue)
		End With

		Return container
	End Function

	Private Sub ShowProductChangedForm(details As String, changeType As Type)
		tbxProductChangedType.Text = changeType.Name
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim container As KaContainer = ConvertPageToContainerObject(connection)

		Dim packagedInventoryLocationId As Guid = Utilities.GetContainerPackagedInventoryLocationId(GetUserConnection(_currentUser.Id))

		Dim i As Integer = 0 ' attempt to automatically select the facility location
		Do While i < ddlProductChangedFacility.Items.Count
			Dim id As Guid = Guid.Parse(ddlProductChangedFacility.Items(i).Value)
			If container.LocationId = Guid.Empty Then ' the container doesn't have a facility specified
				If Not id.Equals(packagedInventoryLocationId) Then
					ddlProductChangedFacility.SelectedIndex = i ' use the first non-packaged inventory location available
					Exit Do
				End If
			ElseIf container.LocationId = id Then ' the container has a facility specified and this is it
				ddlProductChangedFacility.SelectedIndex = i
				Exit Do
			End If
			i += 1
		Loop

		Try ' attempt to automatically select the packaged inventory location
			ddlProductChangedPackaged.SelectedValue = packagedInventoryLocationId.ToString()
		Catch ex As ArgumentOutOfRangeException
			ddlProductChangedDefaultOwner.SelectedIndex = 0
		End Try

		Dim possibleDefaultOwners As New List(Of Guid)
		For Each setting As KaSetting In KaSetting.GetAll(connection, "deleted=0 AND [name] LIKE '%/CF2/DefaultContainerOwnerId'", "")
			Dim possibleDefaultOwnerId As Guid ' parse the owner ID from the setting...
			If Guid.TryParse(setting.Value, possibleDefaultOwnerId) AndAlso Not possibleDefaultOwnerId.Equals(Guid.Empty) AndAlso Not possibleDefaultOwners.Contains(possibleDefaultOwnerId) Then possibleDefaultOwners.Add(possibleDefaultOwnerId)
		Next
		Dim defaultOwnerId As Guid = Guid.Empty
		For Each possibleDefaultOwnerId As Guid In possibleDefaultOwners
			Try
				defaultOwnerId = container.GetOwnerIdForInventory(connection, Nothing, possibleDefaultOwnerId, New Dictionary(Of String, Object))
			Catch ex As RecordNotFoundException ' This will throw an exception if a designated owner cannot be found
			End Try
			If Not defaultOwnerId.Equals(Guid.Empty) Then Exit For
		Next

		Try
			ddlProductChangedDefaultOwner.SelectedValue = defaultOwnerId.ToString()
		Catch ex As ArgumentOutOfRangeException
			ddlProductChangedDefaultOwner.SelectedIndex = 0
		End Try

		lblProductChangedDetails.Text = details
		productChangedRecordControl.Visible = True
		productChanged.Visible = True
		recordSelection.Visible = False
		recordControl.Visible = False
		general1.Visible = False
		general2.Visible = False
	End Sub

	Private Sub HideProductChangedForm()
		productChangedRecordControl.Visible = False
		productChanged.Visible = False
		recordSelection.Visible = True
		recordControl.Visible = True
		general1.Visible = True
		general2.Visible = True
	End Sub

	Protected Sub btnSave_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnSave.Click
		If FormDataValid() Then
			Try ' to save the container information...
				Save(False, lblStatus.Text)
			Catch ex As ProductChangedException ' the user has changed the product...
				ShowProductChangedForm(ex.Message, ex.GetType())
			Catch ex As ProductWeightChangedException ' the user has changed the product weight...
				ShowProductChangedForm(ex.Message, ex.GetType())
			End Try
		End If
	End Sub

	Private Sub ddlOwner_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlOwner.SelectedIndexChanged
		Dim currentSelectedValue As Guid = Guid.Parse(ddlBulkProduct.SelectedValue)
		PopulateBulkProductList(_currentUser, Guid.Parse(ddlOwner.SelectedValue))
		If currentSelectedValue <> Guid.Empty And Not DropDownListContains(ddlBulkProduct, currentSelectedValue) Then
			Dim li As ListItem = New ListItem
			Dim bulkProd As KaBulkProduct = New KaBulkProduct(GetUserConnection(_currentUser.Id), currentSelectedValue)
			li.Value = currentSelectedValue.ToString
			li.Text = bulkProd.Name
			ddlBulkProduct.Items.Add(li)
		End If
		ddlBulkProduct.SelectedValue = currentSelectedValue.ToString
	End Sub

	Protected Sub lbtHistory_Click(ByVal sender As Object, ByVal e As EventArgs) Handles lbtHistory.Click
		If ddlContainers.SelectedIndex > 0 Then
			Dim url As String = ""
			If Utilities.ConvertWebPageUrlDomainToRequestedPagesDomain(GetUserConnection(_currentUser.Id)) Then url = Tm2Database.GetCurrentPageUrlWithOriginalPort(Me.Request)

			Dim containerHistoryPageUrl As String = "ContainerHistory.aspx"
			If url.Trim.Length > 0 Then
				containerHistoryPageUrl = "http://localhost/TerminalManagement2/ContainerHistory.aspx"
				containerHistoryPageUrl = Tm2Database.GetUrlInCurrentDomain(url, containerHistoryPageUrl)
			End If

			DisplayJavaScriptMessage("Container History", Utilities.JsWindowOpen(containerHistoryPageUrl & "?container_id=" & ddlContainers.SelectedValue.ToString()))
		End If
	End Sub
#End Region

	Public Function VerifyContainerOwnerProductAvailability(ByVal ownerId As Guid, ByVal bulkProdId As Guid) As Boolean
		Dim bulkProd As KaBulkProduct = New KaBulkProduct(GetUserConnection(_currentUser.Id), bulkProdId)
		If bulkProd.OwnerId = Guid.Empty Or bulkProd.OwnerId = ownerId Or (bulkProd.OwnerId <> Guid.Empty And ownerId = Guid.Empty) Then
			Return True
		Else
			Return False
		End If
	End Function

	Private Sub PopulateFacilityList()
		ddlFacilityFilter.Items.Clear()
		ddlFacilityFilter.Items.Add(New ListItem("All facilities", Guid.Empty.ToString()))
		For Each l As KaLocation In KaLocation.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
			ddlFacilityFilter.Items.Add(New ListItem(l.Name, l.Id.ToString))
		Next
	End Sub

	Private Sub PopulateContainersList(ByVal currentUser As KaUser)
		ddlContainers.Items.Clear()
		If _currentUserPermission(_currentTableName).Create Then ddlContainers.Items.Add(New ListItem("Enter a new container", Guid.Empty.ToString())) Else ddlContainers.Items.Add(New ListItem("Select a container", Guid.Empty.ToString()))
		Dim containerId As Guid = Guid.Empty
		If Request.QueryString("id") IsNot Nothing Then Guid.TryParse(Request.QueryString("id"), containerId)
		For Each r As KaContainer In KaContainer.GetAll(GetUserConnection(currentUser.Id), IIf(containerId.Equals(Guid.Empty), "deleted=0", "(deleted=0 OR id = " & Q(containerId) & ")") & IIf(currentUser.OwnerId = Guid.Empty, "", " And owner_id=" & Q(currentUser.OwnerId)) & IIf(ddlFacilityFilter.SelectedIndex > 0, " AND (location_id = " & Q(ddlFacilityFilter.SelectedValue) & " OR location_id = " & Q(Guid.Empty) & ")", ""), "number ASC")
			ddlContainers.Items.Add(New ListItem(r.Number & IIf(r.Deleted, " (deleted)", ""), r.Id.ToString()))
		Next
	End Sub

	Private Sub PopulateContainerTypeList()
		ddlContainerType.Items.Clear()
		ddlContainerType.Items.Add(New ListItem("", Guid.Empty.ToString()))
		For Each r As KaContainerType In KaContainerType.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "name ASC")
			ddlContainerType.Items.Add(New ListItem(r.Name, r.Id.ToString()))
		Next
	End Sub

	Private Sub PopulateConditionList()
		ddlCondition.Items.Clear()
		ddlCondition.Items.Add(New ListItem("Excellent", KaContainer.ContainerCondition.Excellent))
		ddlCondition.Items.Add(New ListItem("Good", KaContainer.ContainerCondition.Good))
		ddlCondition.Items.Add(New ListItem("Fair", KaContainer.ContainerCondition.Fair))
		ddlCondition.Items.Add(New ListItem("Poor", KaContainer.ContainerCondition.Poor))
	End Sub

	Private Sub PopulateOwnerList(ByVal currentUser As KaUser)
		ddlOwner.Items.Clear()
		ddlProductChangedDefaultOwner.Items.Clear()
		If currentUser.OwnerId = Guid.Empty Then ddlOwner.Items.Add(New ListItem("All owners", Guid.Empty.ToString()))
		ddlProductChangedDefaultOwner.Items.Add(New ListItem("Not specified", Guid.Empty.ToString()))
		For Each r As KaOwner In KaOwner.GetAll(GetUserConnection(currentUser.Id), "deleted=0" & IIf(currentUser.OwnerId = Guid.Empty, "", " And id=" & Q(currentUser.OwnerId)), "name ASC")
			ddlOwner.Items.Add(New ListItem(r.Name, r.Id.ToString()))
			ddlProductChangedDefaultOwner.Items.Add(New ListItem(r.Name, r.Id.ToString()))
		Next
	End Sub

	Private Sub PopulateLocationList()
		ddlLocation.Items.Clear()
		ddlProductChangedFacility.Items.Clear()
		ddlProductChangedPackaged.Items.Clear()
		ddlLocation.Items.Add(New ListItem("", Guid.Empty.ToString()))
		Dim packagedInventoryLocationId As Guid = Utilities.GetContainerPackagedInventoryLocationId(GetUserConnection(_currentUser.Id))
		For Each r As KaLocation In KaLocation.GetAll(GetUserConnection(_currentUser.Id), $"deleted = 0 {IIf(ddlFacilityFilter.SelectedIndex > 0, $" AND (id = {Q(ddlFacilityFilter.SelectedValue)} OR id = {Q(packagedInventoryLocationId)})", "")}", "name ASC")
			If packagedInventoryLocationId.Equals(Guid.Empty) OrElse r.Id.Equals(packagedInventoryLocationId) Then
				ddlProductChangedPackaged.Items.Add(New ListItem(r.Name, r.Id.ToString()))
			End If
			If Not r.Id.Equals(packagedInventoryLocationId) Then
				ddlLocation.Items.Add(New ListItem(r.Name, r.Id.ToString()))
				ddlProductChangedFacility.Items.Add(New ListItem(r.Name, r.Id.ToString()))
			End If
		Next
	End Sub

	Private Sub PopulateStatusList()
		ddlStatus.Items.Clear()
		ddlStatus.Items.Add(New ListItem("In customer custody", KaContainer.ContainerStatus.InCustomerCustody))
		ddlStatus.Items.Add(New ListItem("In facility", KaContainer.ContainerStatus.InFacility))
		ddlStatus.Items.Add(New ListItem("In transit", KaContainer.ContainerStatus.InTransit))
	End Sub

	Private Sub PopulateUnitsList()
		ddlWeight.Items.Clear()
		ddlVolume.Items.Clear()
		For Each r As KaUnit In KaUnit.GetAll(GetUserConnection(_currentUser.Id), "deleted=0", "abbreviation ASC")
			If KaUnit.IsWeight(r.BaseUnit) Then
				ddlWeight.Items.Add(New ListItem(r.Abbreviation, r.Id.ToString()))
			ElseIf Not KaUnit.IsTime(r.BaseUnit) Then
				ddlVolume.Items.Add(New ListItem(r.Abbreviation, r.Id.ToString()))
			End If
		Next
	End Sub

	Private Sub PopulateOrdersCombo(ByVal productId As Guid, ByVal forOrderId As Guid)
		ddlOrders.Items().Clear()
		ddlOrders.Items.Add(New ListItem("", Guid.Empty.ToString))
		For Each order As KaOrder In GetOrdersWithProductId(productId)
			ddlOrders.Items.Add(New ListItem(order.Number, order.Id.ToString))
		Next

		Try
			ddlOrders.SelectedValue = forOrderId.ToString()
		Catch ex As ArgumentOutOfRangeException
			ddlOrders.SelectedIndex = 0
			Try
				Dim order As New KaOrder(GetUserConnection(_currentUser.Id), forOrderId)
				If order.Deleted Then
					DisplayJavaScriptMessage("InvalidOrderDeleted", Utilities.JsAlert("The order " & order.Number & " that was assigned To this container has been deleted. Order Not Set."))
				ElseIf order.Completed Then
					DisplayJavaScriptMessage("InvalidOrderCompleted", Utilities.JsAlert("The order " & order.Number & " that was assigned To this container has been completed. Order Not Set."))
				Else
					DisplayJavaScriptMessage("InvalidOrder", Utilities.JsAlert("The order " & order.Number & " that was assigned To this container Is no longer a valid order For this location Or bulk product. Order Not Set."))
				End If
			Catch notFoundEx As RecordNotFoundException
				DisplayJavaScriptMessage("InvalidOrderId", Utilities.JsAlert("Record not found in orders where ID = " & forOrderId.ToString() & ". Order Not Set."))
			End Try
		End Try
	End Sub

	Private Function GetOrdersWithProductId(ByVal productId As Guid) As ArrayList
		Dim allOrders As New ArrayList() ' a list of orders that reference this product
		If productId <> Guid.Empty Then
			Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
			Dim ordersRdr As OleDbDataReader = Tm2Database.ExecuteReader(connection, String.Format("Select DISTINCT orders.id, orders.number " &
											  "FROM orders " &
											  "INNER JOIN order_items On order_items.order_id = orders.id " &
											  "WHERE (orders.deleted=0) And (orders.completed=0) And (order_items.deleted=0) And (product_id={0}) " &
											  "ORDER by orders.number", Q(productId)))
			Do While ordersRdr.Read()
				If Not allOrders.Contains(ordersRdr.Item("id")) Then ' this will make sure we only get each order listed once
					Try
						Dim order As New KaOrder(connection, ordersRdr.Item("id"))
						If Not order.Deleted And Not order.Completed Then allOrders.Add(order)
					Catch ex As RecordNotFoundException ' suppress exception: it looks like the order item points to an order that no longer exists
					End Try
				End If
			Loop
			ordersRdr.Close()
		End If
		Return allOrders
	End Function

	Private Sub PopulateBulkProductList(ByVal currentUser As KaUser, ByVal ownerId As Guid)
		ddlBulkProduct.Items.Clear()
		Dim allBulkProds As ArrayList = New ArrayList
		If ownerId = Guid.Empty Then
			allBulkProds = KaBulkProduct.GetAll(GetUserConnection(currentUser.Id), "deleted=0", "name ASC")
		Else
			allBulkProds = KaBulkProduct.GetAll(GetUserConnection(currentUser.Id), "deleted=0 And (owner_id = " & Q(ownerId) & " Or owner_id = " & Q(Guid.Empty) & ")", "name ASC")
		End If
		For Each r As KaBulkProduct In allBulkProds
			If Not r.IsFunction(GetUserConnection(currentUser.Id)) Then
				ddlBulkProduct.Items.Add(New ListItem(r.Name, r.Id.ToString()))
			End If
		Next
	End Sub

	Private Sub PopulateContainerInformation(ByVal containerId As Guid)
		With New KaContainer()
			Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
			.Id = containerId
			Try
				.SqlSelect(connection)
			Catch ex As RecordNotFoundException
			End Try
			If .Id = Guid.Empty Then
				lbtHistory.Visible = False
			Else
				lbtHistory.Visible = True
			End If
			tbxNumber.Text = .Number
			Try
				ddlContainerType.SelectedValue = .ContainerTypeId.ToString()
			Catch ex As ArgumentOutOfRangeException
				ddlContainerType.SelectedIndex = 0
				DisplayJavaScriptMessage("InvalidContainerType", Utilities.JsAlert("Record not found in container types where ID = " & .ContainerTypeId.ToString() & ". Container type Not Set."))
			End Try
			tbxVolume.Text = .Volume
			If .VolumeUnitId = Guid.Empty Then
				.VolumeUnitId = KaUnit.GetSystemDefaultVolumeUnitOfMeasure(connection, Nothing)
			End If
			Try
				ddlVolume.SelectedValue = .VolumeUnitId.ToString()
			Catch ex As ArgumentOutOfRangeException
				ddlVolume.SelectedIndex = 0
				DisplayJavaScriptMessage("InvalidVolumeUnitId", Utilities.JsAlert("Record not found in units where ID = " & .VolumeUnitId.ToString() & ". Volume unit Not Set."))
			End Try
			tbxEmptyWeight.Text = .EmptyWeight
			If .WeightUnitId = Guid.Empty Then
				.WeightUnitId = KaUnit.GetSystemDefaultMassUnitOfMeasure(connection, Nothing)
			End If
			Try
				ddlWeight.SelectedValue = .WeightUnitId.ToString()
			Catch ex As ArgumentOutOfRangeException
				ddlWeight.SelectedIndex = 0
				DisplayJavaScriptMessage("InvalidWeightUnitId", Utilities.JsAlert("Record not found in units where ID = " & .WeightUnitId.ToString() & ". Weight unit Not Set."))
			End Try
			tbxSealNumber.Text = .SealNumber
			tbxNotes.Text = .Notes
			tbxInServiceDate.Value = .InService
			tbxLastInspectedDate.Value = .LastInspected
			rblPassedInspection.SelectedValue = IIf(.PassedInspection, "Yes", "No")
			tbxManufacturedDate.Value = .Manufactured
			Try
				ddlOwner.SelectedValue = .OwnerId.ToString()
			Catch ex As ArgumentOutOfRangeException
				ddlOwner.SelectedValue = Utilities.GetUser(Me).OwnerId.ToString()
				DisplayJavaScriptMessage("InvalidOwnerId", Utilities.JsAlert("Record not found in owners where ID = " & .OwnerId.ToString() & ". Owner Not Set."))
			End Try
			Try
				ddlLocation.SelectedValue = .LocationId.ToString()
			Catch ex As ArgumentOutOfRangeException
				ddlLocation.SelectedIndex = 0
				DisplayJavaScriptMessage("InvalidFacilityId", Utilities.JsAlert("Record not found in facilities where ID = " & .LocationId.ToString() & ". Facility Not Set."))
			End Try
			Try
				ddlStatus.SelectedValue = .Status
			Catch ex As ArgumentOutOfRangeException
				ddlStatus.SelectedValue = KaContainer.ContainerStatus.InFacility
				DisplayJavaScriptMessage("InvalidInfacility", Utilities.JsAlert("Invalid status value (" & .Status & "). Status Set To ""In facility""."))
			End Try
			tbxLastFilledDate.Value = .LastFilled
			tbxLastCleanedDate.Value = .LastCleaned
			Try
				If .BulkProductId <> Guid.Empty And Not DropDownListContains(ddlBulkProduct, .BulkProductId) Then
					Dim li As ListItem = New ListItem
					Dim bulkProd As KaBulkProduct = New KaBulkProduct(connection, .BulkProductId)
					li.Value = .BulkProductId.ToString
					li.Text = bulkProd.Name
					ddlBulkProduct.Items.Add(li)
				End If
				If .BulkProductId = Guid.Empty Then
					ddlBulkProduct.SelectedIndex = 0
				Else
					ddlBulkProduct.SelectedValue = .BulkProductId.ToString()
				End If
			Catch ex As ArgumentOutOfRangeException
				ddlBulkProduct.SelectedIndex = 0
				DisplayJavaScriptMessage("InvalidBulkProductId", Utilities.JsAlert("Record not found in bulk products where ID = " & .BulkProductId.ToString() & ". Bulk product not set."))
			End Try
			ddlBulkProduct_SelectedIndexChanged(ddlBulkProduct, New EventArgs())
			tbxProductWeight.Text = .ProductWeight
			Try
				If .WeightUnitId <> Guid.Empty Then lblProductWeight.Text = New KaUnit(connection, .WeightUnitId).Abbreviation Else lblProductWeight.Text = ""
			Catch ex As RecordNotFoundException
				lblProductWeight.Text = ""
			End Try
			Try
				If .LastTicketId <> Guid.Empty Then lblLastTicket.Text = New KaTicket(connection, .LastTicketId).Number Else lblLastTicket.Text = ""
			Catch ex As RecordNotFoundException
				lblLastTicket.Text = ""
				DisplayJavaScriptMessage("InvalidTicketId", Utilities.JsAlert("Record not found in tickets where ID = " & .BulkProductId.ToString() & ". Last ticket number not displayed."))
			End Try
			If Tm2Database.SystemItemTraceabilityEnabled Then
				Try
					ddlLot.SelectedValue = .LotId.ToString()
				Catch ex As RecordNotFoundException
					ddlLot.SelectedIndex = 0
					DisplayJavaScriptMessage("InvalidLotId", Utilities.JsAlert("Record not found in lots where ID = " & .LotId.ToString() & ". Lot not set."))
				End Try
			End If
			btnDelete.Attributes.Remove("onclick")
			If .Deleted Then
				btnDelete.Text = "Undelete"
				Utilities.ConfirmBox(Me.btnDelete, "Are you sure you want to undelete this container?") ' Delete confirmation box setup
				lblContainerDeletedStatus.Visible = True
			Else
				btnDelete.Text = "Delete"
				Utilities.ConfirmBox(Me.btnDelete, "Are you sure you want to delete this container?") ' Delete confirmation box setup
				lblContainerDeletedStatus.Visible = False
			End If
			rblRefillable.SelectedValue = IIf(.Refillable, "Yes", "No")
			rblPassedPressureTest.SelectedValue = IIf(.PassedPressureTest, "Yes", "No")
			rblSealBroken.SelectedValue = IIf(.SealBroken, "Yes", "No")
			rblOneWayValvePresent.SelectedValue = IIf(.OneWayValvePresent, "Yes", "No")
			PopulateOrdersCombo(GetProductIdForbulkProductId(.LocationId, .BulkProductId), .ForOrderId)
		End With
		SetControlUsabilityFromPermissions()
	End Sub

	Private Function DropDownListContains(ByVal ddl As DropDownList, ByVal value As Guid) As Boolean
		For Each item As ListItem In ddl.Items
			If Guid.Parse(item.Value) = value Then
				Return True
			End If
		Next
		Return False
	End Function

	Private Function GetProductIdForbulkProductId(ByVal locationId As Guid, ByVal bulkProductId As Guid) As Guid
		If locationId <> Guid.Empty And bulkProductId <> Guid.Empty Then
			Dim allProductbulkProducts As ArrayList = KaProductBulkProduct.GetAll(GetUserConnection(_currentUser.Id), "location_id = " & Q(locationId) & " And bulk_product_id = " & Q(bulkProductId) & " And portion = 100", "")
			If allProductbulkProducts.Count > 0 Then
				Dim productbulkProduct As KaProductBulkProduct = allProductbulkProducts.Item(0)
				Return productbulkProduct.ProductId
			End If
		End If
		Return Guid.Empty
	End Function

	Private Sub SetTextboxMaxLengths()
		tbxEmptyWeight.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaContainer.TABLE_NAME, "empty_weight"))
		tbxNumber.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaContainer.TABLE_NAME, "number"))
		tbxProductWeight.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaContainer.TABLE_NAME, "product_weight"))
		tbxVolume.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaContainer.TABLE_NAME, "volume"))
		tbxNewLotNumber.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaLot.TABLE_NAME, KaLot.FN_NUMBER))
	End Sub

	Private Sub ddlBulkProduct_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles ddlBulkProduct.SelectedIndexChanged
		Dim locationId As Guid = Guid.Empty
		If ddlLocation.SelectedIndex >= 0 Then Guid.TryParse(ddlLocation.SelectedValue, locationId)
		Dim bulkProductId As Guid = Guid.Empty
		If ddlBulkProduct.SelectedIndex >= 0 Then Guid.TryParse(ddlBulkProduct.SelectedValue, bulkProductId)
		Dim forOrderId As Guid = Guid.Empty
		If ddlOrders.SelectedIndex >= 0 Then Guid.TryParse(ddlOrders.SelectedValue, forOrderId)
		PopulateOrdersCombo(GetProductIdForbulkProductId(locationId, bulkProductId), forOrderId)
		PopulateReceivingLots(bulkProductId)
		ddlLot_SelectedIndexChanged(ddlLot, New EventArgs())
	End Sub

	Private Sub ddlLocation_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles ddlLocation.SelectedIndexChanged
		Dim locationId As Guid = Guid.Empty
		If ddlLocation.SelectedIndex >= 0 Then Guid.TryParse(ddlLocation.SelectedValue, locationId)
		Dim bulkProductId As Guid = Guid.Empty
		If ddlBulkProduct.SelectedIndex >= 0 Then Guid.TryParse(ddlBulkProduct.SelectedValue, bulkProductId)
		Dim forOrderId As Guid = Guid.Empty
		If ddlOrders.SelectedIndex >= 0 Then Guid.TryParse(ddlOrders.SelectedValue, forOrderId)
		PopulateOrdersCombo(GetProductIdForbulkProductId(locationId, bulkProductId), forOrderId)
	End Sub

	Protected Sub btnProductChangedSave_Click(sender As Object, e As EventArgs) Handles btnProductChangedSave.Click
		Dim facilityLocationId As Guid = Guid.Parse(ddlProductChangedFacility.SelectedValue)
		Dim packagedLocationId As Guid = Guid.Parse(ddlProductChangedPackaged.SelectedValue)
		Dim defaultOwnerId As Guid = Guid.Parse(ddlProductChangedDefaultOwner.SelectedValue)
		Dim user As KaUser = Utilities.GetUser(Page.User)
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)
		Dim containerId As Guid = Guid.Parse(ddlContainers.SelectedValue)
		Dim previousContainer As KaContainer
		Try
			previousContainer = New KaContainer(connection, containerId)
		Catch ex As RecordNotFoundException
			Dim currentContainer As KaContainer = ConvertPageToContainerObject(connection)
			previousContainer = New KaContainer With {.Id = currentContainer.Id, .WeightUnitId = currentContainer.WeightUnitId}
		End Try
		Save(True, lblStatus.Text)
		containerId = Guid.Parse(ddlContainers.SelectedValue)
		With New KaContainer(connection, containerId)
			If previousContainer.Id.Equals(Guid.Empty) Then previousContainer.Id = .Id
			If previousContainer.BulkProductId.Equals(Guid.Empty) Then previousContainer.BulkProductId = .BulkProductId
			.HandleBulkProductChange(connection, Nothing, previousContainer, facilityLocationId, packagedLocationId, defaultOwnerId, APPLICATION_ID, user.Username)
		End With
		HideProductChangedForm()
	End Sub

	Protected Sub btnProductChangedSaveWithoutInventoryAdjustment_Click(sender As Object, e As EventArgs) Handles btnProductChangedSaveWithoutInventoryAdjustment.Click
		Utilities.CreateEventLogEntry(KaEventLog.Categories.Warning, "User " & _currentUser.Name & " was prompted With the following message: '" & lblProductChangedDetails.Text & "' but chose to save changes to the container record without updating bulk product inventory.", GetUserConnection(_currentUser.Id))
		Save(True, lblStatus.Text)
		HideProductChangedForm()
	End Sub

	Protected Sub btnProductChangedCancel_Click(sender As Object, e As EventArgs) Handles btnProductChangedCancel.Click
		lblStatus.Text = "Save canceled"
		HideProductChangedForm()
	End Sub

	Private Sub SetControlUsabilityFromPermissions()
		With _currentUserPermission(_currentTableName)
			Dim shouldEnable = (.Edit AndAlso ddlContainers.SelectedIndex > 0) OrElse (.Create AndAlso ddlContainers.SelectedIndex = 0)
			pnlMain.Enabled = shouldEnable
			btnSave.Enabled = shouldEnable AndAlso btnDelete.Text <> "Undelete"

			Dim containerId As Guid = Guid.Parse(ddlContainers.SelectedValue)
			btnDelete.Enabled = Not containerId.Equals(Guid.Empty) AndAlso .Edit AndAlso (.Delete OrElse btnDelete.Text = "Undelete")
			btnClearLastTicket.Enabled = Not containerId.Equals(Guid.Empty) AndAlso .Edit
		End With
	End Sub

	Private Sub ddlFacilityFilter_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlFacilityFilter.SelectedIndexChanged
		PopulateLocationList()

		Dim currentContainerId As String = ddlContainers.SelectedValue
		PopulateContainersList(_currentUser)
		Try
			ddlContainers.SelectedValue = currentContainerId
		Catch ex As ArgumentOutOfRangeException

		End Try
		ddlContainers_SelectedIndexChanged(ddlContainers, New EventArgs)
		KaSetting.WriteSetting(GetUserConnection(_currentUser.Id), "ContainersPage:" & _currentUser.Id.ToString & "/LastFacilityUsed", ddlFacilityFilter.SelectedValue)
	End Sub

	Private Sub PopulateReceivingLots(bulkProductId As Guid)
		ddlLot.Items.Clear()
		ddlLot.Items.Add(New ListItem("Lot not assigned", Guid.Empty.ToString()))
		If Tm2Database.SystemItemTraceabilityEnabled Then
			Dim lotCreationUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({KaLot.TABLE_NAME}), "Products")

			With lotCreationUserPermission(KaLot.TABLE_NAME)
				If .Create Then
					ddlLot.Items.Add(New ListItem("Create new lot", "New"))
				End If
			End With

			For Each lot As KaLot In KaLot.GetAll(GetUserConnection(_currentUser.Id), $"{KaLot.FN_DELETED} = 0 AND {KaLot.FN_BULK_PRODUCT_ID} = {Q(bulkProductId)}", $"{KaLot.FN_NUMBER} ASC")
				ddlLot.Items.Add(New ListItem(lot.Number, lot.Id.ToString()))
			Next
		End If
	End Sub

	Protected Sub ddlLot_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlLot.SelectedIndexChanged
		If ddlLot.SelectedIndex = 1 AndAlso ddlLot.SelectedValue = "New" Then
			pnlNewLot.Visible = Tm2Database.SystemItemTraceabilityEnabled
			tbxNewLotNumber.Text = ""
		Else
			pnlNewLot.Visible = False
		End If
	End Sub

	Protected Sub ToolkitScriptManager1_AsyncPostBackError(ByVal sender As Object, ByVal e As System.Web.UI.AsyncPostBackErrorEventArgs)
		ToolkitScriptManager1.AsyncPostBackErrorMessage = e.Exception.Message
	End Sub

	Private Sub DisplayJavaScriptMessage(key As String, script As String)
		ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), key, script, False)
	End Sub
End Class
