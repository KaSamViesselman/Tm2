Imports KahlerAutomation.KaTm2Database
Imports System.Data.OleDb
Imports System.Reflection

Public Module Database

	Public Db As Tm2Database
	Public ReadOnly Property DownloadDirectory(referencePage As Page) As String
		Get
			Dim downloadDir As String = referencePage.Server.MapPath("") & "\download\"
			If Not IO.Directory.Exists(downloadDir) Then
				IO.Directory.CreateDirectory(downloadDir)
			End If
			Return downloadDir
		End Get
	End Property
	Public userConnection As New Hashtable()

	Public Const APPLICATION_ID As String = "TM2"
	Private _applicationIdentifier As String = ""
	Public Function ApplicationIdentifier() As String
		If _applicationIdentifier.Trim.Length = 0 Then
			_applicationIdentifier = System.Net.Dns.GetHostName & "/" & APPLICATION_ID
		End If
		Return _applicationIdentifier
	End Function

	Public Sub InitializeDatabase()
		Db = New Tm2Database(Tm2Database.GetDbConnection())
		Db.CheckDatabase()
		PopulateSectionTableNames()
		'Copy user's privileges to bulk product panel settings if they don't exist.
		CopyPanelBulkProductSettings()
	End Sub

	Private Sub PopulateSectionTableNames()
		With Utilities.SectionTableNames
			.Clear()
			.Add("Applicators", New List(Of String)({KaApplicator.TABLE_NAME}))
			.Add("Branches", New List(Of String)({KaBranch.TABLE_NAME}))
			.Add("Carriers", New List(Of String)({KaCarrier.TABLE_NAME}))
			.Add("Containers", New List(Of String)({KaContainer.TABLE_NAME}))
			.Add("Crops", New List(Of String)({KaCropType.TABLE_NAME}))
			.Add("CustomPages", New List(Of String)({KaCustomPages.TABLE_NAME}))
			.Add("Accounts", New List(Of String)({KaCustomerAccount.TABLE_NAME}))
			.Add("Drivers", New List(Of String)({KaDriver.TABLE_NAME}))
			.Add("Emails", New List(Of String)({KaEmail.TABLE_NAME}))
			.Add("Facilities", New List(Of String)({KaLocation.TABLE_NAME}))
			.Add("GeneralSettings", New List(Of String)({KaSetting.TABLE_NAME}))
			.Add("Interfaces", New List(Of String)({KaInterface.TABLE_NAME}))
			.Add("Inventory", New List(Of String)({KaBulkProductInventory.TABLE_NAME}))
			.Add("Orders", New List(Of String)({KaOrder.TABLE_NAME}))
			.Add("Owners", New List(Of String)({KaOwner.TABLE_NAME}))
			.Add("Panels", New List(Of String)({KaPanel.TABLE_NAME}))
			.Add("Products", New List(Of String)({KaProduct.TABLE_NAME}))
			.Add("PanelBulkProductSettings", New List(Of String)({KaBulkProductPanelSettings.TABLE_NAME}))
			.Add("PurchaseOrders", New List(Of String)({KaReceivingPurchaseOrder.TABLE_NAME}))
			'.Add("Reports", New List(Of String)({KaApplicator.TABLE_NAME}))
			.Add("StagedOrders", New List(Of String)({KaStagedOrder.TABLE_NAME}))
			.Add("Tanks", New List(Of String)({KaTank.TABLE_NAME}))
			.Add("Transports", New List(Of String)({KaTransport.TABLE_NAME}))
			.Add("Units", New List(Of String)({KaUnit.TABLE_NAME}))
			.Add("Users", New List(Of String)({KaUser.TABLE_NAME}))
		End With
	End Sub

	Private Sub CopyPanelBulkProductSettings()
		Dim hasDoneCopy As Boolean = Boolean.Parse(KaSetting.GetSetting(Tm2Database.Connection, "HasDoneCopyOfPanelBulkProductUserPrivelage", "False"))
		If Not hasDoneCopy Then
			Dim allUsers As ArrayList = KaUser.GetAll(Tm2Database.Connection, "deleted = 0 AND user_profile_id = " & Q(Guid.Empty), "")
			For Each user As KaUser In allUsers
				'Check to see if the permission exists for Panel Bulk Products
				If user.GetPermissionValueByName("PanelBulkProductSettings").Equals("") Then
					'Need to update the PanelBulkProductSettings permission, find the Panels permission and use it's value
					Dim newPermission As KaUserPermission = New KaUserPermission("PanelBulkProductSettings", user.GetPermissionValueByName("Panels"))
					user.Permissions.Add(newPermission)
					user.SqlUpdate(Tm2Database.Connection, Nothing, "TM2", "")
				End If
			Next

			KaSetting.WriteSetting(Tm2Database.Connection, "HasDoneCopyOfPanelBulkProductUserPrivelage", "True") 'Write this setting as we only ever need to do this once.
		End If
	End Sub

	Public Function Q(ByVal value As Object) As String
		If TypeOf value Is String Then
			Return "'" & CType(value, String).Replace("'", "''") & "'"
		ElseIf TypeOf value Is DateTime Or TypeOf value Is Date Then
			Return "'" & Format(value, "yyyyMMdd HH:mm:ss") & "'"
		ElseIf TypeOf value Is Boolean Then
			If value Then
				Return "1"
			Else
				Return "0"
			End If
		ElseIf TypeOf value Is Guid Then
			Return "'{" & value.ToString() & "}'"
		Else
			Return CType(value, String)
		End If
	End Function

	Public Sub AddUserConnection(ByVal userId As Guid, ByVal conn As OleDbConnection)
		If userConnection.ContainsKey(userId) Then
			userConnection.Remove(userId)
		End If
		userConnection.Add(userId, conn)
	End Sub

	Public Function GetUserConnection(ByVal userId As Guid) As OleDbConnection
		If userConnection.ContainsKey(userId) Then
			Dim conn As OleDbConnection = userConnection(userId)
			If conn.State = ConnectionState.Closed Or conn.State = ConnectionState.Broken Then
				conn.Open()
			End If
			Return conn
		Else
			Dim newConn As OleDbConnection = New OleDbConnection(Tm2Database.GetDbConnection())
			newConn.Open()
			userConnection.Add(userId, newConn)
			Return newConn
		End If
	End Function

    Public Function GetProductIdForBulkProductId(connection As OleDbConnection, bulkProductId As Guid) As Guid
        Dim r As OleDbDataReader = Tm2Database.ExecuteReader(connection, "SELECT product_id FROM product_bulk_products WHERE deleted = 0 AND portion = 100 AND bulk_product_id = " & Q(bulkProductId))
        Try
            If r.Read() Then Return r("product_id") Else Throw New RecordNotFoundException("product not found for bulk product")
        Finally
            r.Close()
        End Try
    End Function
End Module
