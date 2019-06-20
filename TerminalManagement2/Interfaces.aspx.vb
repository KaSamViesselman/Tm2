Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb

Public Class Interfaces
	Inherits System.Web.UI.Page

	Private _currentUser As KaUser
	Private _currentUserPermission As Dictionary(Of String, KaTablePermission) = Nothing
	Private _currentTableName As String = KaInterface.TABLE_NAME
	Private _customFields As New List(Of KaCustomField)
	Private _customFieldData As New List(Of KaCustomFieldData)

	Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
		Response.Cache.SetCacheability(HttpCacheability.NoCache)
		_currentUser = Utilities.GetUser(Me)
		_currentUserPermission = Utilities.GetUserPagePermission(_currentUser, New List(Of String)({_currentTableName}), "Interfaces")
		If Not _currentUserPermission(_currentTableName).Read Then Response.Redirect("Welcome.aspx")

		If Not Page.IsPostBack Then
			SetTextboxMaxLengths()
			_customFields.Clear()
			For Each customField As KaCustomField In KaCustomField.GetAll(GetUserConnection(_currentUser.Id), String.Format("deleted=0 AND {0} = {1}", KaCustomField.FN_TABLE_NAME, Q(KaInterface.TABLE_NAME)), KaCustomField.FN_FIELD_NAME)
				_customFields.Add(customField)
			Next
			PopulateInterfaceTypeList()
			PopulateInterfacesList()
			SetControlUsabilityFromPermissions()
			If Page.Request("InterfaceId") IsNot Nothing Then
				Try
					ddlInterfaces.SelectedValue = Page.Request("InterfaceId")
				Catch ex As ArgumentOutOfRangeException
					ddlInterfaces.SelectedIndex = 0
				End Try
			End If
			ddlInterfaces_SelectedIndexChanged(ddlInterfaces, New EventArgs())
			Utilities.ConfirmBox(Me.btnDelete, "Are you sure you want to delete this interface?") ' Delete confirmation box setup
			Utilities.SetFocus(tbxName, Me) ' set focus to the first textbox on the page
		End If
	End Sub

	Protected Sub btnSave_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnSave.Click
		lblStatus.Text = ""
		If ValidateFields() Then
			With New KaInterface()
				.Id = Guid.Parse(ddlInterfaces.SelectedValue)
				.Name = tbxName.Text
				.InterfaceTypeId = Guid.Parse(ddlInterfaceTypes.SelectedValue)
				Dim status As String = ""
				If .Id <> Guid.Empty Then
					.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
					status = "Interface updated successfully."
				Else
					.SqlInsert(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
					status = "Interface added successfully."
				End If

				Utilities.ConvertCustomFieldPanelToLists(_customFields, _customFieldData, lstCustomFields)
				For Each customData As KaCustomFieldData In _customFieldData
					customData.RecordId = .Id
					customData.SqlUpdateInsertIfNotFound(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
				Next

				PopulateInterfacesList()
				ddlInterfaces.SelectedValue = .Id.ToString()
				ddlInterfaces_SelectedIndexChanged(Nothing, Nothing)
				lblStatus.Text = status
			End With
		End If
	End Sub

	Private Function ValidateFields() As Boolean
		If tbxName.Text.Trim = "" Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidName", Utilities.JsAlert("Name must be specified.")) : Return False
		If Guid.Parse(ddlInterfaceTypes.SelectedValue) = Guid.Empty Then ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidInterfaceType", Utilities.JsAlert("Interface Type must be specified")) : Return False

		Dim allInterface As ArrayList = KaInterface.GetAll(GetUserConnection(_currentUser.Id), "name = " & Q(tbxName.Text.Trim) & " and deleted = 0", "")
		If allInterface.Count > 0 Then
			Dim tempInterface As KaInterface = allInterface.Item(0)
			If tempInterface.Id <> Guid.Parse(ddlInterfaces.SelectedValue) Then
				ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidDuplicateName", Utilities.JsAlert("An Interface with name " & tbxName.Text.Trim & " already exists.  Name must be unique")) : Return False
			End If
		End If
		Return True
	End Function

	Protected Sub btnDelete_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnDelete.Click
		Dim id As Guid = Guid.Parse(ddlInterfaces.SelectedValue)
		Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)

		Dim sort As String = "last_updated DESC"
		Dim orders As ArrayList = KaOrder.GetAll(connection, $"{KaOrder.FN_DELETED} = 0 AND {KaOrder.FN_COMPLETED} = 0 AND {KaOrder.FN_INTERFACE_ID} = {Q(id)}", "")
		Dim unexportedTickets As ArrayList = Tm2Database.GetUnbilledTickets(connection, id, False, 0)
		Dim receivingPurchaseOrders As ArrayList = KaReceivingPurchaseOrder.GetAll(connection, $"deleted = 0 AND completed = 0 AND interface_id = {Q(id)}", "")
		Dim unexportedPoTickets As ArrayList = Tm2Database.GetUnbilledReceivingTickets(connection, id, False, Nothing)
		If orders.Count = 0 AndAlso unexportedTickets.Count = 0 AndAlso receivingPurchaseOrders.Count = 0 AndAlso unexportedPoTickets.Count = 0 Then
			With New KaInterface(connection, id)
				.Deleted = True
				.SqlUpdate(GetUserConnection(_currentUser.Id), Nothing, Database.ApplicationIdentifier, _currentUser.Name)
				lblStatus.Text = "Interface deleted successfully."
			End With
			PopulateInterfacesList()
			PopulateInterfaceInformation(Guid.Empty)
		Else
			Dim list As String = "" ' a list of all the records that reference this interface
			Dim invalidReferences As List(Of String)
			If orders.Count > 0 Then
				invalidReferences = New List(Of String)
				For Each o As KaOrder In orders
					If Not invalidReferences.Contains(o.Number) Then invalidReferences.Add(o.Number)
				Next
				list &= $"\r\nOrder{IIf(orders.Count > 1, "s", "")}: {String.Join(", ", invalidReferences)}"
			End If
			If unexportedTickets.Count > 0 Then
				invalidReferences = New List(Of String)
				For Each t As KaTicket In unexportedTickets
					If Not invalidReferences.Contains(t.Number) Then invalidReferences.Add(t.Number)
				Next
				list &= $"\r\nTicket{IIf(orders.Count > 1, "s", "")}: {String.Join(", ", invalidReferences)}"
			End If
			If receivingPurchaseOrders.Count > 0 Then
				invalidReferences = New List(Of String)
				For Each rpo As KaReceivingPurchaseOrder In receivingPurchaseOrders
					If Not invalidReferences.Contains(rpo.Number) Then invalidReferences.Add(rpo.Number)
				Next
				list &= $"\r\nReceiving purchase order{IIf(orders.Count > 1, "s", "")}: {String.Join(", ", invalidReferences)}"
			End If
			If unexportedPoTickets.Count > 0 Then
				invalidReferences = New List(Of String)
				For Each rt As KaReceivingTicket In unexportedPoTickets
					If Not invalidReferences.Contains(rt.Number) Then invalidReferences.Add(rt.Number)
				Next
				list &= $"\r\nReceiving purchase order ticket{IIf(orders.Count > 1, "s", "")}: {String.Join(", ", invalidReferences)}"
			End If
			ClientScript.RegisterClientScriptBlock(Me.GetType(), "InvalidInterfaceUsed", Utilities.JsAlert(String.Format("Cannot delete this interface because it is used by: {0}", list)))
		End If
	End Sub

	Private Sub PopulateInterfaceTypeList()
		ddlInterfaceTypes.Items.Clear()
		ddlInterfaceTypes.Items.Add(New ListItem("", Guid.Empty.ToString))
		For Each interfaceType As KaInterfaceTypes In KaInterfaceTypes.GetAll(GetUserConnection(_currentUser.Id), "deleted = " & Q(False), "name asc")
			ddlInterfaceTypes.Items.Add(New ListItem(interfaceType.Name, interfaceType.Id.ToString))
		Next
	End Sub

	Private Sub PopulateInterfacesList()
		ddlInterfaces.Items.Clear()
		If _currentUserPermission(_currentTableName).Create Then
			ddlInterfaces.Items.Add(New ListItem("Enter a new interface", Guid.Empty.ToString()))
		Else
			ddlInterfaces.Items.Add(New ListItem("Select a interface", Guid.Empty.ToString()))
		End If
		For Each r As KaInterface In KaInterface.GetAll(GetUserConnection(_currentUser.Id), "deleted = " & Q(False), "name ASC")
			ddlInterfaces.Items.Add(New ListItem(r.Name, r.Id.ToString()))
		Next
	End Sub

	Private Sub PopulateInterfaceInformation(ByVal interfaceId As Guid)
		_customFieldData.Clear()
		With New KaInterface()
			.Id = interfaceId
			If .Id <> Guid.Empty Then
				Dim connection As OleDbConnection = GetUserConnection(_currentUser.Id)

				.SqlSelect(connection)
				btnDelete.Enabled = _currentUserPermission(_currentTableName).Delete
				litInterfaceId.Text = .Id.ToString

				For Each customFieldValue As KaCustomFieldData In KaCustomFieldData.GetAll(GetUserConnection(_currentUser.Id), String.Format("deleted = 0 AND {0} = {1}", KaCustomFieldData.FN_RECORD_ID, Q(.Id)), KaCustomFieldData.FN_LAST_UPDATED)
					_customFieldData.Add(customFieldValue)
				Next
			Else
				btnDelete.Enabled = False
				litInterfaceId.Text = ""
			End If
			tbxName.Text = .Name
			ddlInterfaceTypes.SelectedValue = .InterfaceTypeId.ToString
		End With
		Utilities.CreateDynamicCustomFieldPanelControls(_customFields, _customFieldData, lstCustomFields, Page)
	End Sub

	Private Sub ddlInterfaces_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles ddlInterfaces.SelectedIndexChanged
		PopulateInterfaceInformation(Guid.Parse(ddlInterfaces.SelectedValue))
		SetControlUsabilityFromPermissions()
	End Sub

	Private Sub SetTextboxMaxLengths()
		tbxName.MaxLength = Math.Max(0, Tm2Database.GetMaxDatabaseFieldLength(KaInterface.TABLE_NAME, "name"))
	End Sub

	Protected Overrides Function SaveViewState() As Object
		Dim viewState(2) As Object
		'Saving the grid values to the View State
		Utilities.ConvertCustomFieldPanelToLists(_customFields, _customFieldData, lstCustomFields)
		viewState(0) = MyBase.SaveViewState()
		viewState(1) = _customFields
		viewState(2) = _customFieldData
		Return viewState
	End Function

	Protected Overrides Sub LoadViewState(savedState As Object)
		'Getting the dropdown list value from view state.
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
			Dim shouldEnable = (.Edit AndAlso ddlInterfaces.SelectedIndex > 0) OrElse (.Create AndAlso ddlInterfaces.SelectedIndex = 0)
			tbxName.Enabled = shouldEnable
			ddlInterfaceTypes.Enabled = shouldEnable
			btnSave.Enabled = shouldEnable
			Dim value = Guid.Parse(ddlInterfaces.SelectedValue)
			btnDelete.Enabled = .Edit AndAlso .Delete AndAlso value <> Guid.Empty
		End With
	End Sub
End Class