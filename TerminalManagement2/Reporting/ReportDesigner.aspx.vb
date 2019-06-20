Imports KahlerAutomation.KaTm2Database

Public Class ReportDesigner
	Inherits System.Web.UI.Page
	Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
		If Not Page.IsPostBack Then
			ddlReportBasis.Items.Add("Select report type")
			ddlReportBasis.Items.Add("Applicators")
			ddlReportBasis.Items.Add("Bays")
			ddlReportBasis.Items.Add("Branches")
			ddlReportBasis.Items.Add("Bulk products")
			ddlReportBasis.Items.Add("Bulk product inventory")
			ddlReportBasis.Items.Add("Carriers")
			ddlReportBasis.Items.Add("Customers")
			ddlReportBasis.Items.Add("Coupled customers")
			ddlReportBasis.Items.Add("Customer account driver")
			ddlReportBasis.Items.Add("Containers")
			ddlReportBasis.Items.Add("Crop types")
			ddlReportBasis.Items.Add("Discharge locations")
			ddlReportBasis.Items.Add("Drivers")
			ddlReportBasis.Items.Add("Interfaces")
			ddlReportBasis.Items.Add("Interface types")
			ddlReportBasis.Items.Add("Inventory changes")
			ddlReportBasis.Items.Add("Facilities")
			ddlReportBasis.Items.Add("Facility driver access")
			ddlReportBasis.Items.Add("Orders")
			ddlReportBasis.Items.Add("Owners")
			ddlReportBasis.Items.Add("Tickets")
			ddlReportBasis.Items.Add("Panels")
			ddlReportBasis.Items.Add("Products")
			ddlReportBasis.Items.Add("Receiving purchase orders")
			ddlReportBasis.Items.Add("Suppliers")
			ddlReportBasis.Items.Add("Tanks")
			ddlReportBasis_SelectedIndexChanged(ddlReportBasis, New EventArgs())
			pnlShowReport.Visible = lstSelectedDetails.Items.Count > 0
		End If
	End Sub

	Private Sub btnShowReport_Click(sender As Object, e As EventArgs) Handles btnShowReport.Click
		Dim ds As DataSet = New DataSet 'GetDataSet("select top 10 * from Orders;select top 4 * from customers"); 
		Dim fieldAliasHeaderCaptions As Dictionary(Of String, String) = New Dictionary(Of String, String)
		Dim sql As String = GenerateSqlScriptForKaRecordEntity(fieldAliasHeaderCaptions)
		If sql.Length > 0 Then
			Dim instanceId As String = Guid.NewGuid().ToString()
			Session.Item(instanceId) = sql
			Session.Item("ReportTitle") = ddlReportBasis.SelectedValue
			Dim fieldAliasHeaderList As List(Of KeyValuePair(Of String, String)) = New List(Of KeyValuePair(Of String, String))
			For Each key As String In fieldAliasHeaderCaptions.Keys
				fieldAliasHeaderList.Add(New KeyValuePair(Of String, String)(key, fieldAliasHeaderCaptions(key)))
			Next
			Session.Item(Page.Request("FieldAliasHeaderList")) = fieldAliasHeaderList

			ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), "ShowReport", Utilities.JsWindowOpen($"Report.aspx?ReportBasis={ddlReportBasis.SelectedValue}&InstanceId={instanceId}"), False)
		End If
	End Sub

	Public Function GenerateSqlScriptForKaRecordEntity(ByRef fieldAliasHeaderCaptions As Dictionary(Of String, String)) As String
		Dim baseTableName As String = ""
		Dim objectType As Type = Nothing
		GetSelectedType(objectType, baseTableName)
		Dim tableJoins As List(Of String) = New List(Of String)

		Dim reportObjects As List(Of Reporting.ReportingObject) = New List(Of Reporting.ReportingObject)
		Dim reportingDictionary As Dictionary(Of Guid, Reporting.ReportingObject) = New Dictionary(Of Guid, Reporting.ReportingObject)

		GetReportingObjectsFromTreeview(reportObjects, reportingDictionary)
		Dim fieldsToDisplay As List(Of String) = New List(Of String)

		For Each li As ListItem In lstSelectedDetails.Items
			Dim ro As Reporting.ReportingObject = Tm2Database.FromXml(Server.HtmlDecode(li.Value), GetType(Reporting.ReportingObject))
			If ro.Children Is Nothing OrElse ro.Children.Count = 0 Then
				fieldsToDisplay.Add(ro.SqlFieldName & IIf(ro.AliasedSqlFieldName.Length > 0, $" AS {ro.AliasedSqlFieldName}", ""))
			End If
			AddReportingObjectToSqlList(ro, reportingDictionary, tableJoins)
			fieldAliasHeaderCaptions(ro.AliasedSqlFieldName) = ro.TableHeaderText
		Next

		Dim sql As String = ""
		If fieldsToDisplay.Count > 0 Then sql = $"SELECT DISTINCT {String.Join(", ", fieldsToDisplay)} FROM {baseTableName} {String.Join(" ", tableJoins)}"
		If KaRecord.TableContainsDeletedField(objectType) Then sql &= $" WHERE {baseTableName}.deleted = 0"
		Return sql
	End Function

	Private Sub AddReportingObjectToSqlList(ByVal ro As Reporting.ReportingObject, reportingDictionary As Dictionary(Of Guid, Reporting.ReportingObject), ByRef tableJoins As List(Of String))
		If Not ro.ParentId.Equals(Guid.Empty) AndAlso reportingDictionary.ContainsKey(ro.ParentId) Then
			AddReportingObjectToSqlList(reportingDictionary(ro.ParentId), reportingDictionary, tableJoins)
		End If
		If ro.SqlTableJoin.Length > 0 AndAlso Not tableJoins.Contains(ro.SqlTableJoin) Then tableJoins.Add(ro.SqlTableJoin)
	End Sub

	Private Function GetReportingObjects() As List(Of KahlerAutomation.KaTm2Database.Reporting.ReportingObject)
		Dim type As Type = Nothing
		Dim baseTableName As String = ""
		GetSelectedType(type, baseTableName)
		If type IsNot Nothing Then
			Return GetReportingObjects(type, baseTableName)
		End If
		Return New List(Of Reporting.ReportingObject)
	End Function

	Private Function GetReportingObjects(type As Type, baseTableName As String) As List(Of KahlerAutomation.KaTm2Database.Reporting.ReportingObject)
		Dim reportingObjects As List(Of KahlerAutomation.KaTm2Database.Reporting.ReportingObject) = KaRecord.GenerateReportingObjectsForKaRecordEntity(type, baseTableName, "")
		Return reportingObjects
	End Function

	Private Sub ddlReportBasis_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlReportBasis.SelectedIndexChanged
		Dim baseTableName As String = ""
		Dim reportingObjects As List(Of KahlerAutomation.KaTm2Database.Reporting.ReportingObject) = GetReportingObjects()

		tvDisplayedFields.Nodes.Clear()
		lstSelectedDetails.Items.Clear()
		For Each ro As Reporting.ReportingObject In reportingObjects
			AddTreeViewNode(ro, Nothing)
		Next

		pnlDisplayedFields.Visible = ddlReportBasis.SelectedIndex > 0
		pnlSelectedDetails.Visible = ddlReportBasis.SelectedIndex > 0
		pnlShowReport.Visible = lstSelectedDetails.Items.Count > 0
	End Sub

	Private Sub AddTreeViewNode(ro As Reporting.ReportingObject, parentNode As TreeNode)
		'ToDo: Create custom object to define unit of measure to display in, if field can be totalized, or if the field should be displayed as a column (customer activity report, for example)
		Dim newTreeNode As TreeNode = New TreeNode() With {
				.Checked = False,
				.Selected = False,
				.Text = ro.HeaderText,
				.Value = Server.HtmlEncode(Tm2Database.ToXml(ro, GetType(Reporting.ReportingObject)))
			}
		If ro.Children IsNot Nothing AndAlso ro.Children.Count > 0 Then
			For Each roChild As Reporting.ReportingObject In ro.Children
				AddTreeViewNode(roChild, newTreeNode)
			Next
		Else
			newTreeNode.ShowCheckBox = True
		End If

		If parentNode Is Nothing Then
			tvDisplayedFields.Nodes.Add(newTreeNode)
		Else
			parentNode.ChildNodes.Add(newTreeNode)
		End If
	End Sub

	Friend Sub GetSelectedType(ByRef type As Type, ByRef baseTableName As String)
		baseTableName = ""
		type = Nothing

		Select Case ddlReportBasis.SelectedValue
			Case "Applicators"
				baseTableName = KaApplicator.TABLE_NAME
				type = GetType(KaApplicator)
			Case "Bays"
				baseTableName = KaBay.TABLE_NAME
				type = GetType(KaBay)
			Case "Branches"
				baseTableName = KaBranch.TABLE_NAME
				type = GetType(KaBranch)
			Case "Bulk products"
				baseTableName = KaBulkProduct.TABLE_NAME
				type = GetType(KaBulkProduct)
			Case "Bulk product inventory"
				baseTableName = KaBulkProductInventory.TABLE_NAME
				type = GetType(KaBulkProductInventory)
			Case "Carriers"
				baseTableName = KaCarrier.TABLE_NAME
				type = GetType(KaCarrier)
			Case "Customers"
				baseTableName = KaCustomerAccount.TABLE_NAME
				type = GetType(KaCustomerAccount)
			Case "Coupled customers"
				baseTableName = KaCustomerAccountCombination.TABLE_NAME
				type = GetType(KaCustomerAccountCombination)
			Case "Customer account driver"
				baseTableName = KaCustomerAccountDriver.TABLE_NAME
				type = GetType(KaCustomerAccountDriver)
			Case "Containers"
				baseTableName = KaContainer.TABLE_NAME
				type = GetType(KaContainer)
			Case "Crop types"
				baseTableName = KaCropType.TABLE_NAME
				type = GetType(KaCropType)
			Case "Discharge locations"
				baseTableName = KaDischargeLocation.TABLE_NAME
				type = GetType(KaDischargeLocation)
			Case "Drivers"
				baseTableName = KaDriver.TABLE_NAME
				type = GetType(KaDriver)
			Case "Facilities"
				baseTableName = KaLocation.TABLE_NAME
				type = GetType(KaLocation)
			Case "Facility driver access"
				baseTableName = KaLocationDriverAccess.TABLE_NAME
				type = GetType(KaLocationDriverAccess)
			Case "Interfaces"
				baseTableName = KaInterface.TABLE_NAME
				type = GetType(KaInterface)
			Case "Interface types"
				baseTableName = KaInterfaceTypes.TABLE_NAME
				type = GetType(KaInterfaceTypes)
			Case "Inventory changes"
				baseTableName = KaInventoryChange.TABLE_NAME
				type = GetType(KaInventoryChange)
			Case "Orders"
				baseTableName = KaOrder.TABLE_NAME
				type = GetType(KaOrder)
			Case "Owners"
				baseTableName = KaOwner.TABLE_NAME
				type = GetType(KaOwner)
			Case "Panels"
				baseTableName = KaPanel.TABLE_NAME
				type = GetType(KaPanel)
			Case "Products"
				baseTableName = KaProduct.TABLE_NAME
				type = GetType(KaProduct)
			Case "Receiving purchase orders"
				baseTableName = KaReceivingPurchaseOrder.TABLE_NAME
				type = GetType(KaReceivingPurchaseOrder)
			Case "Suppliers"
				baseTableName = KaSupplierAccount.TABLE_NAME
				type = GetType(KaSupplierAccount)
			Case "Tanks"
				baseTableName = KaTank.TABLE_NAME
				type = GetType(KaTank)
			Case "Tickets"
				baseTableName = KaTicket.TABLE_NAME
				type = GetType(KaTicket)
		End Select
	End Sub

	Private Function GetReportingObjectsFromTreeview(ByRef reportObjects As List(Of Reporting.ReportingObject), ByRef reportingDictionary As Dictionary(Of Guid, Reporting.ReportingObject)) As List(Of Reporting.ReportingObject)
		For Each childNode As TreeNode In tvDisplayedFields.Nodes
			GetReportingObjectsFromTreeviewNode(childNode, reportingDictionary)
		Next

		For Each roId As Guid In reportingDictionary.Keys
			Dim ro As Reporting.ReportingObject = reportingDictionary(roId)
			If Not ro.ParentId.Equals(Guid.Empty) AndAlso reportingDictionary.ContainsKey(ro.ParentId) Then
				'If reportingDictionary(ro.ParentId).Children Is Nothing OrElse ro.Children.Count = 0 Then reportingDictionary(ro.ParentId).Children = New List(Of Reporting.ReportingObject)
				If reportingDictionary(ro.ParentId).Children Is Nothing Then reportingDictionary(ro.ParentId).Children = New List(Of Reporting.ReportingObject)
				reportingDictionary(ro.ParentId).Children.Add(ro)
			End If
		Next
		For Each roId As Guid In reportingDictionary.Keys
			Dim ro As Reporting.ReportingObject = reportingDictionary(roId)
			If ro.ParentId.Equals(Guid.Empty) Then
				reportObjects.Add(ro)
			End If
		Next
		Return reportObjects
	End Function

	Private Sub GetReportingObjectsFromTreeviewNode(node As TreeNode, ByRef reportingDictionary As Dictionary(Of Guid, Reporting.ReportingObject))
		Dim ro As Reporting.ReportingObject = Tm2Database.FromXml(Server.HtmlDecode(node.Value), GetType(Reporting.ReportingObject))
		If Not reportingDictionary.ContainsKey(ro.Id) Then reportingDictionary.Add(ro.Id, ro)
		For Each childNode As TreeNode In node.ChildNodes
			GetReportingObjectsFromTreeviewNode(childNode, reportingDictionary)
		Next
	End Sub

	Private Sub tvDisplayedFields_TreeNodeCheckChanged(sender As Object, e As TreeNodeEventArgs) Handles tvDisplayedFields.TreeNodeCheckChanged
		Dim node As TreeNode = e.Node
		Dim ro As Reporting.ReportingObject = Tm2Database.FromXml(Server.HtmlDecode(node.Value), GetType(Reporting.ReportingObject))
		If e.Node.Checked Then
			AddReportingObjectToSelectedList(ro)
		Else
			Dim fieldCounter As Integer = 0
			While fieldCounter < lstSelectedDetails.Items.Count
				Dim currentRo As Reporting.ReportingObject = Tm2Database.FromXml(Server.HtmlDecode(lstSelectedDetails.Items(fieldCounter).Value), GetType(Reporting.ReportingObject))

				If currentRo.Id.Equals(ro.Id) Then
					lstSelectedDetails.Items.RemoveAt(fieldCounter)
				Else
					fieldCounter += 1
				End If
			End While
		End If
		pnlShowReport.Visible = lstSelectedDetails.Items.Count > 0
		lstSelectedDetails.Rows = Math.Max(4, lstSelectedDetails.Items.Count)
	End Sub

	Protected Sub AddReportingObjectToSelectedList(ro As Reporting.ReportingObject)
		lstSelectedDetails.Items.Add(New ListItem(ro.TableHeaderText, Server.HtmlEncode(Tm2Database.ToXml(ro, GetType(Reporting.ReportingObject)))))
	End Sub

	Protected Sub btnMoveFieldUp_Click(sender As Object, e As EventArgs) Handles btnMoveFieldUp.Click
		Dim index As Integer = lstSelectedDetails.SelectedIndex
		If index > 0 Then
			Dim currentRo As Reporting.ReportingObject = Tm2Database.FromXml(Server.HtmlDecode(lstSelectedDetails.Items(index).Value), GetType(Reporting.ReportingObject))
			lstSelectedDetails.Items.RemoveAt(index)
			lstSelectedDetails.Items.Insert(index - 1, New ListItem(currentRo.TableHeaderText, Server.HtmlEncode(Tm2Database.ToXml(currentRo, GetType(Reporting.ReportingObject)))))
			Try
				lstSelectedDetails.SelectedIndex = index - 1
			Catch ex As ArgumentOutOfRangeException
			End Try
		End If
	End Sub

	Protected Sub btnMoveFieldDown_Click(sender As Object, e As EventArgs) Handles btnMoveFieldDown.Click
		Dim index As Integer = lstSelectedDetails.SelectedIndex
		If index >= 0 Then
			Dim currentRo As Reporting.ReportingObject = Tm2Database.FromXml(Server.HtmlDecode(lstSelectedDetails.Items(index).Value), GetType(Reporting.ReportingObject))
			lstSelectedDetails.Items.RemoveAt(index)
			lstSelectedDetails.Items.Insert(index, New ListItem(currentRo.TableHeaderText, Server.HtmlEncode(Tm2Database.ToXml(currentRo, GetType(Reporting.ReportingObject)))))
			Try
				lstSelectedDetails.SelectedIndex = index + 1
			Catch ex As ArgumentOutOfRangeException
			End Try
		End If
	End Sub
End Class