Imports KahlerAutomation.KaTm2Database
Imports System.IO
Imports System.Data.OleDb

Public Class Utilities
	Public Const SQL_MINYEAR As Integer = 1753
	Public Shared SectionTableNames As New Dictionary(Of String, List(Of String))

	''' <summary>
	''' Adds the attribute for a popup delete confirmation
	''' </summary>
	''' <param name="btn"></param>
	''' <param name="message"></param>
	''' <remarks></remarks>
	Public Shared Sub ConfirmBox(ByRef btn As WebControls.Button, ByVal message As String)
		btn.Attributes.Add("onclick", "return confirm('" & message & "');")
	End Sub

	''' <summary>
	''' Adds the attribute for a popup delete confirmation
	''' </summary>
	''' <param name="btn"></param>
	''' <param name="message"></param>
	''' <remarks></remarks>
	Public Shared Sub ConfirmBox(ByRef btn As WebControls.LinkButton, ByVal message As String)
		btn.Attributes.Add("onclick", "return confirm('" & message & "');")
	End Sub

	''' <summary>
	''' Strips all carriage returns and line feeds off the supplied string
	''' </summary>
	''' <param name="text"></param>
	''' <returns></returns>
	''' <remarks></remarks>
	Public Shared Function StripCrLf(ByVal text As String) As String
		Dim result As String = ""
		Do While text.Length > 0
			If Asc(text.Substring(0, 1)) <> 10 And Asc(text.Substring(0, 1)) <> 13 Then
				result &= text.Substring(0, 1)
			ElseIf Asc(text.Substring(0, 1)) = 10 Then
				result &= "; "
			End If
			text = text.Substring(1, text.Length - 1)
		Loop
		Return result
	End Function

	''' <summary>
	''' Strip out single quotes to avoid SQL injection
	''' </summary>
	''' <param name="text"></param>
	''' <returns></returns>
	''' <remarks></remarks>
	Public Shared Function StripSqlInjection(ByVal text As String)
		Return text.Replace("'", "")
	End Function

	Public Shared Function GetListOfPagesForUser(user As KaUser) As List(Of Tm2Page)
		Return GetListOfPagesForUser(user, "", True)
	End Function

	Public Shared Function GetListOfPagesForUser(user As KaUser, authCheck As Boolean) As List(Of Tm2Page)
		Return GetListOfPagesForUser(user, "", authCheck)
	End Function

	Public Shared Function GetListOfPagesForUser(user As KaUser, ByVal currentUrlAddress As String) As List(Of Tm2Page)
		Return GetListOfPagesForUser(user, currentUrlAddress, True)
	End Function

	Public Shared Function GetListOfPagesForUser(user As KaUser, ByVal currentUrlAddress As String, authCheck As Boolean) As List(Of Tm2Page)
		Dim connection As OleDbConnection = GetUserConnection(user.Id)
		Dim list As New List(Of Tm2Page)

		Dim permissionName As String
		Dim tablePermissions As Dictionary(Of String, KaTablePermission) = New Dictionary(Of String, KaTablePermission)
		For Each section As String In {"Applicators", "Branches", "Carriers", "Containers", "Crops", "Custom Pages|CustomPages", "Customer Accounts|Accounts", "Drivers", "Facilities", "General Settings|GeneralSettings", "Interfaces", "Inventory", "Orders", "Owners", "Panels", "Products", "Purchase Orders|PurchaseOrders", "Reports", "Tanks", "Transports", "Units", "Users"}
			Dim sectionTitle As String
			Dim parts() As String = section.Split("|")
			If parts.Length = 2 Then
				sectionTitle = parts(0)
				permissionName = parts(1)
			Else
				sectionTitle = section
				permissionName = section
			End If
			If Not tablePermissions.ContainsKey(permissionName) Then
				Try
					If SectionTableNames.ContainsKey(permissionName) Then
						Dim tableNames As List(Of String) = SectionTableNames(permissionName)
						tablePermissions.Add(permissionName, GetUserPagePermission(user, tableNames, permissionName, authCheck)(tableNames(0)))
					Else
						tablePermissions.Add(permissionName, GetUserPagePermission(user, New List(Of String)({permissionName}), permissionName, authCheck)(permissionName))
					End If
				Catch ex As Exception
				End Try
			End If
			'tablePermissions = GetUserPagePermission(user, New List(Of String)({permissionName}), permissionName)

			If tablePermissions.ContainsKey(permissionName) AndAlso tablePermissions(permissionName).Read Then
				Select Case sectionTitle
					Case "Applicators"
						list.Add(New Tm2Page(sectionTitle, "applicators.aspx"))
					Case "Branches"
						list.Add(New Tm2Page(sectionTitle, "branches.aspx"))
					Case "Carriers"
						Dim tabs As New List(Of Tm2Page)
						tabs.Add(New Tm2Page("Carriers", "carriers.aspx"))
						tabs.Add(New Tm2Page("All Carriers Report", "allcarriers.aspx"))
						list.Add(New Tm2Page(sectionTitle, "carriers.aspx", tabs))
					Case "Containers"
						Dim tabs As New List(Of Tm2Page)
						tabs.Add(New Tm2Page("Containers", "containers.aspx"))
						tabs.Add(New Tm2Page("Container Types", "containertypes.aspx"))
						tabs.Add(New Tm2Page("Container Equipment", "containerequipment.aspx"))
						tabs.Add(New Tm2Page("Container Equipment Types", "containerequipmenttypes.aspx"))
						tabs.Add(New Tm2Page("All Containers Report", "allcontainers.aspx"))
						tabs.Add(New Tm2Page("All Container Equipment Report", "allcontainerequipment.aspx"))
						tabs.Add(New Tm2Page("Container Inventory", "ContainerInventory.aspx"))
						list.Add(New Tm2Page(sectionTitle, "containers.aspx", tabs))
					Case "Crops"
						list.Add(New Tm2Page(sectionTitle, "croptypes.aspx"))
					Case "Custom Pages"
						list.Add(New Tm2Page(sectionTitle, "custompages.aspx"))
					Case "Customer Accounts"
						Dim tabs As New List(Of Tm2Page)
						tabs.Add(New Tm2Page("Accounts", "accounts.aspx"))
						tabs.Add(New Tm2Page("Destinations", "accountdestinations.aspx"))
						tabs.Add(New Tm2Page("Account Coupling", "accountcoupling.aspx"))
						tabs.Add(New Tm2Page("All Customers Report", "customermasterfile.aspx"))
						list.Add(New Tm2Page(sectionTitle, "accounts.aspx", tabs))
					Case "Drivers"
						Dim tabs As New List(Of Tm2Page)
						tabs.Add(New Tm2Page("Drivers", "drivers.aspx"))
						tabs.Add(New Tm2Page("Drivers In Facility", "driverreport.aspx"))
						tabs.Add(New Tm2Page("Drivers In Facility History", "driverhistory.aspx"))
						tabs.Add(New Tm2Page("All Drivers Report", "alldrivers.aspx"))
						list.Add(New Tm2Page(sectionTitle, "drivers.aspx", tabs))
					Case "Facilities"
						Dim tabs As New List(Of Tm2Page)
						tabs.Add(New Tm2Page("Facilities", "facilities.aspx"))
						tabs.Add(New Tm2Page("Bays", "bays.aspx"))
						If Tm2Database.SystemItemTraceabilityEnabled Then tabs.Add(New Tm2Page("Storage Locations", "StorageLocations.aspx"))
						tabs.Add(New Tm2Page("Custom Load Questions", "CustomLoadQuestions.aspx"))
						tabs.Add(New Tm2Page("Tracks", "tracks.aspx"))
						tabs.Add(New Tm2Page("Assignments", "assignments.aspx"))
						list.Add(New Tm2Page(sectionTitle, "facilities.aspx", tabs))
					Case "General Settings"
						Dim tabs As New List(Of Tm2Page)
						tabs.Add(New Tm2Page("General Settings", "GeneralSettings.aspx"))
						tabs.Add(New Tm2Page("Order Settings", "OrderSettings.aspx"))
						tabs.Add(New Tm2Page("E-mail Settings", "EmailSettings.aspx"))
						tabs.Add(New Tm2Page("Analysis Settings", "AnalysisSettings.aspx"))
						tabs.Add(New Tm2Page("Account Coupling<br />Settings", "AccountCouplingSettings.aspx"))
						tabs.Add(New Tm2Page("Receiving PO<br />Settings", "ReceivingPoSettings.aspx"))
						tabs.Add(New Tm2Page("Container Settings", "ContainerSettings.aspx"))
						tabs.Add(New Tm2Page("Ticket Settings", "TicketSettings.aspx"))
						tabs.Add(New Tm2Page("Default Delivery<br />Web Ticket Settings", "DefaultDeliveryWebTicketSettings.aspx"))
						tabs.Add(New Tm2Page("Default Order<br />Summary Settings", "DefaultOrderSummarySettings.aspx"))
						tabs.Add(New Tm2Page("Default Receiving<br />Web Ticket Settings", "DefaultReceivingWebTicketSettings.aspx"))
						tabs.Add(New Tm2Page("Default Receiving<br />Web Pick Ticket Settings", "DefaultReceivingWebPickTicketSettings.aspx"))
						tabs.Add(New Tm2Page("Default Container<br />Label Settings", "DefaultContainerLabelSettings.aspx"))
						list.Add(New Tm2Page(sectionTitle, "generalsettings.aspx", tabs))
					Case "Interfaces"
						Dim tabs As New List(Of Tm2Page)
						tabs.Add(New Tm2Page("Interfaces", "interfaces.aspx"))
						tabs.Add(New Tm2Page("Interface Types", "interfacetypes.aspx"))
						tabs.Add(New Tm2Page("Interface Items", "interfaceitems.aspx"))
						tabs.Add(New Tm2Page("Interface Usage Report", "interfaceusagereport.aspx"))
						tabs.Add(New Tm2Page("Assign Interface to Orders", "InterfaceAssignOrder.aspx"))
						tabs.Add(New Tm2Page("Ticket Export Status", "interfaceticketstatus.aspx"))
						tabs.Add(New Tm2Page("Ticket Receiving Export Status", "InterfaceReceivingTicketStatus.aspx"))

						For Each interfaceInfo As KaInterface In KaInterface.GetAll(connection, "deleted = 0", "name ASC")
							Try
								Dim interfaceType As New KaInterfaceTypes(connection, interfaceInfo.InterfaceTypeId)
								If interfaceType.ConfigUrl.Trim().Length > 0 Then ' show the interface type's configuration page
									Dim url As String = interfaceType.ConfigUrl & "?interface_id=" & interfaceInfo.Id.ToString
									If currentUrlAddress IsNot Nothing AndAlso currentUrlAddress.Trim.Length > 0 AndAlso Utilities.ConvertWebPageUrlDomainToRequestedPagesDomain(connection) Then url = Tm2Database.GetUrlInCurrentDomain(currentUrlAddress, url)

									tabs.Add(New Tm2Page(interfaceInfo.Name, "custom.aspx?page_title=Interfaces:" & System.Web.HttpUtility.HtmlEncode(interfaceInfo.Name) & "&url=" & url))
								End If
							Catch ex As RecordNotFoundException

							End Try
						Next
						list.Add(New Tm2Page(sectionTitle, "interfaces.aspx", tabs))
					Case "Inventory"
						Dim tabs As New List(Of Tm2Page)
						tabs.Add(New Tm2Page("Inventory", "inventory.aspx"))
						tabs.Add(New Tm2Page("Inventory Change Report", "inventorychangereport.aspx"))
						tabs.Add(New Tm2Page("Inventory Groups", "InventoryGroups.aspx"))
						list.Add(New Tm2Page(sectionTitle, "inventory.aspx", tabs))
					Case "Orders"
						Dim tabs As New List(Of Tm2Page)
						tabs.Add(New Tm2Page("Orders", "orders.aspx"))
						tabs.Add(New Tm2Page("Past Orders", "pastorders.aspx"))
						tabs.Add(New Tm2Page("Order List", "orderlist.aspx"))
						If Not tablePermissions.ContainsKey("StagedOrders") Then
							Try
								If SectionTableNames.ContainsKey("StagedOrders") Then
									Dim tableNames As List(Of String) = SectionTableNames("StagedOrders")
									tablePermissions.Add("StagedOrders", GetUserPagePermission(user, tableNames, "StagedOrders")(tableNames(0)))
								Else
									tablePermissions.Add("StagedOrders", GetUserPagePermission(user, New List(Of String)({"StagedOrders"}), "StagedOrders")(permissionName))
								End If
							Catch ex As Exception
							End Try
						End If
						If tablePermissions.ContainsKey("StagedOrders") AndAlso tablePermissions("StagedOrders").Read Then tabs.Add(New Tm2Page("Staged Orders", "stagedorders.aspx"))
						tabs.Add(New Tm2Page("In Progress", "inprogressrecords.aspx"))
						tabs.Add(New Tm2Page("Delete Orders", "deleteorders.aspx"))
						tabs.Add(New Tm2Page("Archive Orders", "archiveorders.aspx"))
						tabs.Add(New Tm2Page("Archive Tickets", "archivetickets.aspx"))
						list.Add(New Tm2Page(sectionTitle, "orders.aspx", tabs))
					Case "Owners"
						list.Add(New Tm2Page(sectionTitle, "owners.aspx"))
					Case "Panels"
						Dim tabs As New List(Of Tm2Page)
						tabs.Add(New Tm2Page("Panels", "panels.aspx"))
						tabs.Add(New Tm2Page("Discharge Locations", "dischargelocations.aspx"))
						tabs.Add(New Tm2Page("Panel Groups", "panelgroups.aspx"))
						list.Add(New Tm2Page(sectionTitle, "panels.aspx", tabs))
					Case "Products"
						Dim tabs As New List(Of Tm2Page)
						tabs.Add(New Tm2Page("Products", "products.aspx"))
						tabs.Add(New Tm2Page("Bulk Products", "bulkproducts.aspx"))
						tabs.Add(New Tm2Page("Bulk Product Analysis", "bulkproductanalysis.aspx"))
						tabs.Add(New Tm2Page("Product Allocation", "productallocation.aspx"))
						tabs.Add(New Tm2Page("Bulk Product Allocation", "bulkproductallocation.aspx"))
						tabs.Add(New Tm2Page("Product List", "productlist.aspx"))
                        tabs.Add(New Tm2Page("Product Groups", "productgroups.aspx"))
                        tabs.Add(New Tm2Page("Product Setup", "productsetup.aspx"))
                        If Tm2Database.SystemItemTraceabilityEnabled Then tabs.Add(New Tm2Page("Bulk Product Lots", "lots.aspx"))
						list.Add(New Tm2Page(sectionTitle, "bulkproducts.aspx", tabs))
					Case "Purchase Orders"
						Dim tabs As New List(Of Tm2Page)
						tabs.Add(New Tm2Page("Receiving Purchase Orders", "receiving.aspx"))
						tabs.Add(New Tm2Page("Past Receiving Purchase Orders", "pastreceiving.aspx"))
						tabs.Add(New Tm2Page("Receiving Purchase Orders List", "receivingpurchaseorderlist.aspx"))
						tabs.Add(New Tm2Page("In Progress", "inprogressrecords.aspx?menu_tab=receiving"))
						tabs.Add(New Tm2Page("Suppliers", "suppliers.aspx"))
						tabs.Add(New Tm2Page("Delete Orders", "deletereceivingpurchaseorders.aspx"))
						tabs.Add(New Tm2Page("Archive Orders", "archivereceivingpurchaseorders.aspx"))
						tabs.Add(New Tm2Page("Archive Tickets", "archivereceivingpurchaseordertickets.aspx"))
						list.Add(New Tm2Page(sectionTitle, "receiving.aspx", tabs))
					Case "Reports"
						Dim tabs As New List(Of Tm2Page)
						tabs.Add(New Tm2Page("Customer Activity Report", "customeractivityreport.aspx"))
						tabs.Add(New Tm2Page("Receiving Activity Report", "receivingactivityreport.aspx"))
						tabs.Add(New Tm2Page("Receipts", "receipts.aspx"))
						tabs.Add(New Tm2Page("Track Report", "trackreport.aspx"))
						tabs.Add(New Tm2Page("Bulk Product Usage Report", "BulkProductUsageReport.aspx"))
						tabs.Add(New Tm2Page("Applications", "applicationusage.aspx"))
						tabs.Add(New Tm2Page("Event Log", "eventlog.aspx"))

						For Each customPage As KaCustomPages In KaCustomPages.GetAll(connection, "report=1 AND view_report=1 AND deleted<>1", "page_label ASC")
							Dim permissionName2 = customPage.Id.ToString()
							Dim tablePermission As KaTablePermission = GetUserPagePermission(user, New List(Of String)({permissionName2}), permissionName2, authCheck)(permissionName2)
							If tablePermission.Read Then
								tabs.Add(New Tm2Page(customPage.PageLabel, String.Format("custom.aspx?pageId={0}", permissionName2)))
							End If
						Next

						list.Add(New Tm2Page(sectionTitle, "receipts.aspx", tabs))
					Case "Tanks"
						Dim tabs As New List(Of Tm2Page)
						tabs.Add(New Tm2Page("Tank Levels", "tanklevels.aspx"))
						tabs.Add(New Tm2Page("Tanks", "tanks.aspx"))
						tabs.Add(New Tm2Page("Tank Analysis", "tankanalysis.aspx"))
						tabs.Add(New Tm2Page("Tank Groups", "tankgroups.aspx"))
						tabs.Add(New Tm2Page("Tank Level Trends", "tankleveltrends.aspx"))
						tabs.Add(New Tm2Page("Tank Alarm History", "tankalarmhistory.aspx"))
						list.Add(New Tm2Page(sectionTitle, "tanklevels.aspx", tabs))
					Case "Transports"
						Dim tabs As New List(Of Tm2Page)
						tabs.Add(New Tm2Page("Transports", "transports.aspx"))
						tabs.Add(New Tm2Page("Transport Types", "transporttypes.aspx"))
						tabs.Add(New Tm2Page("Transports In Facility", "transportreport.aspx"))
						tabs.Add(New Tm2Page("Transports In Facility History", "transporthistory.aspx"))
						tabs.Add(New Tm2Page("Usage Report", "transportusagereport.aspx"))
						tabs.Add(New Tm2Page("All Transports Report", "alltransports.aspx"))
						tabs.Add(New Tm2Page("Transport Tracking Report", "transporttrackingreport.aspx"))
						'tabs.Add(New Tm2Page("Transport Inspection Questions", "transportinspectionquestions.aspx")) 'Turn off per feature: 8461
						list.Add(New Tm2Page(sectionTitle, "transports.aspx", tabs))
					Case "Units"
						list.Add(New Tm2Page(sectionTitle, "units.aspx"))
					Case "Users"
						Dim tabs As New List(Of Tm2Page)
						tabs.Add(New Tm2Page("Users", "users.aspx"))
						tabs.Add(New Tm2Page("User Profiles", "userprofiles.aspx"))
						list.Add(New Tm2Page(sectionTitle, "users.aspx", tabs))
				End Select
			End If
		Next

		'Insert any custom pages
		For Each customPage As KaCustomPages In KaCustomPages.GetAll(connection, "main_menu_link=1 AND deleted<>1", "page_label ASC")
			permissionName = customPage.Id.ToString()
			Dim tablePermission As KaTablePermission = GetUserPagePermission(user, New List(Of String)({permissionName}), permissionName, authCheck)(permissionName)
			If tablePermission.Read Then
				list.Add(New Tm2Page(customPage.PageLabel, String.Format("custom.aspx?pageId={0}", permissionName)))
			End If
		Next

		'Insert the Panel Bulk Product settings
		permissionName = "PanelBulkProductSettings"
		Dim permissions As String = user.GetPermissionValueByName("PanelBulkProductSettings")
		If Not tablePermissions.ContainsKey("PanelBulkProductSettings") Then
			Try
				Dim tableNames As List(Of String) = SectionTableNames("PanelBulkProductSettings")
				tablePermissions.Add("PanelBulkProductSettings", GetUserPagePermission(user, tableNames, permissionName, authCheck)(tableNames(0)))
			Catch ex As Exception
			End Try
		End If

		If tablePermissions.ContainsKey("PanelBulkProductSettings") AndAlso tablePermissions("PanelBulkProductSettings").Read Then
			Dim panelsMenu As Tm2Page = Nothing
			For Each menuPage As Tm2Page In list
				If menuPage.Name.ToLower = "panels" Then
					panelsMenu = menuPage
					Exit For
				End If
			Next
			If panelsMenu Is Nothing Then
				panelsMenu = New Tm2Page("Panels", "PanelBulkProductSettings.aspx", New List(Of Tm2Page))
				Dim insertPoint As Integer = 0
				For i = 0 To list.Count - 1
					If list(i).Name > "Panels" Then
						insertPoint = i
						Exit For
					End If
				Next
				list.Insert(insertPoint, panelsMenu)
			End If
			panelsMenu.Tabs.Add(New Tm2Page("Panel Bulk Product Settings", "PanelBulkProductSettings.aspx"))
			panelsMenu.Tabs.Add(New Tm2Page("Panel Bulk Products Report", "custom.aspx?page_title=Panels:Panel Bulk Products Report&url=panelbulkproducts.aspx"))
			panelsMenu.Tabs.Add(New Tm2Page("Panel Bulk Product Fill Limits", "PanelBulkProductFillLimits.aspx"))
		End If
		'Insert the email menu
		permissionName = "Emails"
		Dim permissions2 As String = user.GetPermissionValueByName("Emails")
		If permissions2 = "" Then permissionName = "Reports"
		If Not tablePermissions.ContainsKey("Emails") Then
			Try
				Dim tableNames As List(Of String) = SectionTableNames("Emails")
				tablePermissions.Add("Emails", GetUserPagePermission(user, tableNames, permissionName, authCheck)(tableNames(0)))
			Catch ex As Exception
			End Try
		End If

		If tablePermissions.ContainsKey("Emails") AndAlso tablePermissions("Emails").Read Then
			Dim reportsMenu As Tm2Page = Nothing
			For Each menuPage As Tm2Page In list
				If menuPage.Name.ToLower = "reports" Then
					reportsMenu = menuPage
					Exit For
				End If
			Next
			If reportsMenu Is Nothing Then
				reportsMenu = New Tm2Page("Reports", "emails.aspx", New List(Of Tm2Page))
				Dim insertPoint As Integer = 0
				For i = 0 To list.Count - 1
					If list(i).Name > "Reports" Then
						insertPoint = i
						Exit For
					End If
				Next
				list.Insert(insertPoint, reportsMenu)
			End If
			reportsMenu.Tabs.Add(New Tm2Page("E-mail Reports", "emailreports.aspx"))
			reportsMenu.Tabs.Add(New Tm2Page("E-mails", "emails.aspx"))
		End If

		list.Add(New Tm2Page("Manual", "help.aspx", New List(Of Tm2Page), True))
		If Not UseWindowsAuthentication() Then
			If user Is Nothing OrElse user.Id.Equals(Guid.Empty) Then
				list.Add(New Tm2Page("Login", "login.aspx?new_session=true"))
			Else
				list.Add(New Tm2Page("Logout", "login.aspx?new_session=true"))
			End If
		End If
		list.Add(New Tm2Page("About", "about.aspx", New List(Of Tm2Page), False))
		Return list
	End Function

    Public Shared Function GetDisplayNotification() As Boolean
        Dim updatesRdr As OleDbDataReader = Nothing
        Try
            updatesRdr = Tm2Database.ExecuteReader(Tm2Database.Connection, $"SELECT COUNT(*) FROM {KaApplicationInformation.TABLE_NAME} WHERE {KaApplicationInformation.FN_DELETED} = 0 AND {KaApplicationInformation.FN_UPDATE_AVAILABLE} = 1")
            If updatesRdr.Read() AndAlso updatesRdr.Item(0) > 0 Then Return True
        Catch ex As Exception
        Finally
            If updatesRdr IsNot Nothing Then updatesRdr.Close()
        End Try
        Return False
    End Function

    Public Shared Function BuildAuthorizedMenuAndSetPageTitle(ByRef page As Web.UI.Page) As String
		Dim title As String = KaSetting.GetSetting(GetUserConnection(GetUser(page).Id), KaSetting.SN_WEB_PAGE_TITLE, KaSetting.SD_WEB_PAGE_TITLE).Trim()
		If title.Length > 0 Then page.Title = title & " : " & page.Title
		Const deselectedBorderStyle As String = "" '"style=""BORDER-RIGHT: 2px outset; BORDER-TOP: 2px outset; BORDER-LEFT: 2px outset; BORDER-BOTTOM: 2px outset"""
		Const selectedBorderStyle As String = "" '"style=""BORDER-RIGHT: 2px inset; BORDER-TOP: 2px inset; BORDER-LEFT: 2px inset; BORDER-BOTTOM: 2px inset"""
		Const alignment As String = "align=""center"""
		Const deselectedColor As String = "bgcolor=""#99ccff"""
		Const selectedColor As String = "bgcolor=""#ffffff"""
		Dim html As New StringBuilder("")
		For Each p As Tm2Page In GetListOfPagesForUser(GetUser(page), page.Request.Url.ToString())
			Dim selected As Boolean = Path.GetFileName(page.Request.PhysicalPath).ToLower() = p.Url.ToLower() OrElse page.Form.Name = p.Name
			If Not selected Then
				For Each p2 As Tm2Page In p.Tabs
					If Path.GetFileName(page.Request.PhysicalPath).ToLower() = p2.Url.ToLower() OrElse page.Form.Name = p2.Name Then
						selected = True
						Exit For
					End If
				Next
			End If
			html.Append("<tr><td " & IIf(selected, selectedBorderStyle, deselectedBorderStyle) & alignment & IIf(selected, selectedColor, deselectedColor) & "><a href=""" & p.Url & """>" & p.Name & "</a></td></tr>")
		Next
		Return html.ToString()
	End Function

	Public Shared Sub SetFocus(ByVal FocusControl As Control, ByRef Page As Web.UI.Page)
		Dim script As New System.Text.StringBuilder
		Dim clientID As String = FocusControl.ClientID
		With script
			.Append("<script language='javascript'>")
			.Append("document.getElementById('")
			.Append(clientID)
			.Append("').focus();")
			.Append("</script>")
		End With
		Page.ClientScript.RegisterStartupScript(Page.GetType(), "setFocus", script.ToString())
		Page.SetFocus(clientID)
	End Sub

	Public Shared Function GetUser(page As Page) As KaUser
		Return GetUser(page.User)
	End Function

	Public Shared Function GetUser(user As System.Security.Principal.IPrincipal) As KaUser
		Try
			Select Case CType(System.Web.Configuration.WebConfigurationManager.GetSection("system.web/authentication"), System.Web.Configuration.AuthenticationSection).Mode
				Case Web.Configuration.AuthenticationMode.Forms
					Dim userId As Guid = Guid.Empty
					Guid.TryParse(user.Identity.Name, userId)
					Return New KaUser(Tm2Database.Connection, userId)
				Case Web.Configuration.AuthenticationMode.Windows
					Dim list As ArrayList = KaUser.GetAll(Tm2Database.Connection, String.Format("deleted=0 AND username={0}", Q(user.Identity.Name)), "name ASC")
					If list.Count = 1 Then
						Return list(0)
					ElseIf list.Count = 0 Then
						Throw New RecordNotFoundException(String.Format("User with username ""{0}"" does not exist", user.Identity.Name))
					ElseIf list.Count > 1 Then
						Throw New TooManyRecordsException(String.Format("More than one user with username ""{0}""", user.Identity.Name))
					End If
				Case Else
					Throw New Exception("Unsupported authentication mode. Check web.config file.")
			End Select
		Catch ex As RecordNotFoundException
		End Try
		Return New KaUser() ' return a user with no rights
	End Function

	Public Shared Function UseWindowsAuthentication() As Boolean
		Try
			Return CType(System.Web.Configuration.WebConfigurationManager.GetSection("system.web/authentication"), System.Web.Configuration.AuthenticationSection).Mode = Web.Configuration.AuthenticationMode.Windows
		Catch ex As FileNotFoundException
			Return False
		End Try
	End Function

	Public Shared Function AutoCompleteYear(ByVal year As String) As String
		If year.Length < 4 Then
			year = "2" & StrDup(3 - year.Length, "0") & year
		End If
		Return year
	End Function

	Public Shared Function JsAlert(ByVal text As String) As String
		Return "<script language=""javascript"" type=""text/javascript"">alert(""" & text.Replace(ControlChars.Quote, "\x22").Replace(vbCrLf, "\n").Replace(vbCr, "\n").Replace(vbLf, "\n") & """)</script>"
	End Function

	Public Shared Function JsWindowOpen(url As String) As String
		Return "<script language=""javascript"" type=""text/javascript"">window.open(""" & url & """)</script>"
	End Function

	' ''' <summary>
	' ''' 
	' ''' </summary>
	' ''' <param name="url"></param>
	' ''' <param name="specs">A comma-separated list of items. The following values are supported:
	' '''     channelmode=yes|no|1|0 	Whether or not to display the window in theater mode. Default is no. IE only
	' '''     directories=yes|no|1|0 	Obsolete. Whether or not to add directory buttons. Default is yes. IE only
	' '''     fullscreen=yes|no|1|0 	Whether or not to display the browser in full-screen mode. Default is no. A window in full-screen mode must also be in theater mode. IE only
	' '''     height=pixels 	The height of the window. Min. value is 100
	' '''     left=pixels 	The left position of the window. Negative values not allowed
	' '''     location=yes|no|1|0 	Whether or not to display the address field. Opera only
	' '''     menubar=yes|no|1|0 	Whether or not to display the menu bar
	' '''     resizable=yes|no|1|0 	Whether or not the window is resizable. IE only
	' '''     scrollbars=yes|no|1|0 	Whether or not to display scroll bars. IE, Firefox and Opera only
	' '''     status=yes|no|1|0 	Whether or not to add a status bar
	' '''     titlebar=yes|no|1|0 	Whether or not to display the title bar. Ignored unless the calling application is an HTML Application or a trusted dialog box
	' '''     toolbar=yes|no|1|0 	Whether or not to display the browser toolbar. IE and Firefox only
	' '''     top=pixels 	The top position of the window. Negative values not allowed
	' '''     width=pixels 	The width of the window. Min. value is 100</param>
	' ''' <param name="replaceBrowserHistory">Specifies whether the URL creates a new entry or replaces the current entry in the history list. The following values are supported:
	' '''     true - URL replaces the current document in the history list
	' '''     false - URL creates a new entry in the history list</param>
	' ''' <returns></returns>
	' ''' <remarks></remarks>
	'Public Shared Function JsWindowOpen(url As String, specs As String, replaceBrowserHistory As Boolean) As String
	'    Return "<script language=""javascript"" type=""text/javascript"">window.open(""" & url & """,null,'" & specs & "','" & replaceBrowserHistory.ToString().ToLower & "');</script>"
	'End Function

	Public Shared Function FindId(ByVal id As Guid, ByRef list As ArrayList) As Integer
		For i As Integer = 0 To list.Count - 1
			If list(i).Id = id Then Return i
		Next
		Return -1
	End Function

	Public Shared Function IsBulkProductTimedFunction(bulkProductId As Guid, userId As Guid) As Boolean
		Dim hasTimedFunction As Boolean = False
		For Each bpps As KaBulkProductPanelSettings In KaBulkProductPanelSettings.GetAll(GetUserConnection(userId), "deleted=0 AND bulk_product_id=" & Q(bulkProductId), "")
			' if product number is for timed mix, timed purge or timed agitate
			If bpps.ProductNumber = KaController.Controller.FunctionNumber.TimedMix OrElse bpps.ProductNumber = KaController.Controller.FunctionNumber.TimedPurge OrElse bpps.ProductNumber = KaController.Controller.FunctionNumber.TimedAgitate Then
				hasTimedFunction = True
			Else
				Return False
			End If
		Next
		Return hasTimedFunction
	End Function

	Public Shared Function IsBulkProductRinseFunction(bulkProductId As Guid, userId As Guid) As Boolean
		Dim hasRinseFunction As Boolean = False
		For Each bpps As KaBulkProductPanelSettings In KaBulkProductPanelSettings.GetAll(GetUserConnection(userId), "deleted=0 AND bulk_product_id=" & Q(bulkProductId), "")
			' if product number is for timed mix, timed purge or timed agitate
			If bpps.ProductNumber = KaController.Controller.FunctionNumber.Rinse Then
				hasRinseFunction = True
			Else
				Return False
			End If
		Next
		Return hasRinseFunction
	End Function

	Public Shared Function IsBulkProductParameterlessFunction(bulkProductId As Guid, userId As Guid) As Boolean
		Dim hasParameterlessFunction As Boolean = False
		For Each bpps As KaBulkProductPanelSettings In KaBulkProductPanelSettings.GetAll(GetUserConnection(userId), "deleted=0 AND bulk_product_id=" & Q(bulkProductId), "")
			' if product number is for discharge, automatic discharge, wait, start recirc, end recirc, start agitate, end agitate
			If bpps.ProductNumber = KaController.Controller.FunctionNumber.Discharge OrElse bpps.ProductNumber = KaController.Controller.FunctionNumber.AutomaticDischarge OrElse bpps.ProductNumber = KaController.Controller.FunctionNumber.WaitNext OrElse bpps.ProductNumber = KaController.Controller.FunctionNumber.StartRecirculation OrElse bpps.ProductNumber = KaController.Controller.FunctionNumber.StopRecirculation OrElse bpps.ProductNumber = KaController.Controller.FunctionNumber.StartAgitator OrElse bpps.ProductNumber = KaController.Controller.FunctionNumber.StopAgitator Then
				hasParameterlessFunction = True
			Else
				Return False
			End If
		Next
		Return hasParameterlessFunction
	End Function

	Public Shared Function IsProductTimedFunction(productId As Guid, userId As Guid) As Boolean
		Dim hasTimedFunction As Boolean = False
		For Each pbp As KaProductBulkProduct In KaProductBulkProduct.GetAll(GetUserConnection(userId), "deleted=0 AND product_id=" & Q(productId), "")
			If IsBulkProductTimedFunction(pbp.BulkProductId, userId) Then
				hasTimedFunction = True
			Else
				Return False
			End If
		Next
		Return hasTimedFunction
	End Function

	Public Shared Function IsProductParameterlessFunction(productId As Guid, userId As Guid) As Boolean
		Dim hasParameterlessFunction As Boolean = False
		For Each pbp As KaProductBulkProduct In KaProductBulkProduct.GetAll(GetUserConnection(userId), "deleted=0 AND product_id=" & Q(productId), "")
			If IsBulkProductParameterlessFunction(pbp.BulkProductId, userId) Then
				hasParameterlessFunction = True
			Else
				Return False
			End If
		Next
		Return hasParameterlessFunction
	End Function

	'Public Shared Function GetKeywordSearchConditionsForTable(userId As Guid, tableName As String, keyword As String) As String
	'    ' build a conditions string that will search all the "LIKEable" text fields in the specified table
	'    Dim reader As OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(userId), "SELECT column_name,data_type,character_maximum_length FROM information_schema.columns WHERE table_name=" & Q(tableName))
	'    Dim conditions As String = ""
	'    Do While reader.Read()
	'        If reader(1).ToLower() = "nvarchar" AndAlso reader(2) > 0 Then
	'            conditions &= IIf(conditions.Length > 0, " OR ", "") & "[" & reader(0) & "]" & " LIKE " & Q("%" & keyword & "%")
	'        End If
	'    Loop
	'    reader.Close()
	'    Return conditions
	'End Function

	'Public Shared Function GetOrdersWithKeyword(userId As Guid, ByVal keyword As String, ByVal completedOrders As Boolean) As List(Of Guid)
	'    Dim orders As New List(Of Guid) ' get a list of the order IDs
	'    Dim orderConditions As String = Utilities.GetKeywordSearchConditionsForTable(userId, KaOrder.TABLE_NAME, keyword)
	'    Dim productConditions As String = Utilities.GetKeywordSearchConditionsForTable(userId, KaProduct.TABLE_NAME, keyword)
	'    Dim customerAccountConditions As String = Utilities.GetKeywordSearchConditionsForTable(userId, KaCustomerAccount.TABLE_NAME, keyword)

	'    Dim orderIdRdr As OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(userId), "SELECT orders.id " & _
	'                                        "FROM orders " & _
	'                                        "WHERE " & orderConditions & " OR " & _
	'                                            "(orders.id in " & _
	'                                                "(select order_id " & _
	'                                                "from order_items " & _
	'                                                "where product_id in " & _
	'                                                    "(select id from products WHERE " & productConditions & " AND deleted = 0))) OR " & _
	'                                            "(orders.id in " & _
	'                                                "(select order_id " & _
	'                                                "from order_customer_accounts " & _
	'                                                "where customer_account_id in " & _
	'                                                    "(select id from customer_accounts WHERE " & customerAccountConditions & " AND deleted=0 AND is_supplier = 0))) " & _
	'                                            "AND deleted = 0 and completed = " & Q(completedOrders) & " " & _
	'                                            "ORDER BY orders.number ASC")
	'    Do While orderIdRdr.Read()
	'        If Not orders.Contains(orderIdRdr.Item("id")) Then orders.Add(orderIdRdr.Item("id"))
	'    Loop
	'    orderIdRdr.Close()
	'    Return orders
	'End Function

	'Public Shared Function GetTicketsWithKeyword(userId As Guid, ByVal keyword As String) As List(Of Guid)
	'    Dim tickets As New List(Of Guid) ' get a list of the ticket IDs
	'    Dim ticketConditions As String = Utilities.GetKeywordSearchConditionsForTable(userId, KaTicket.TABLE_NAME, keyword)
	'    Dim ticketBulkItemConditions As String = Utilities.GetKeywordSearchConditionsForTable(userId, KaTicketBulkItem.TABLE_NAME, keyword)
	'    Dim ticketCustomerAccountConditions As String = Utilities.GetKeywordSearchConditionsForTable(userId, KaTicketCustomerAccount.TABLE_NAME, keyword)
	'    Dim ticketItemConditions As String = Utilities.GetKeywordSearchConditionsForTable(userId, KaTicketItem.TABLE_NAME, keyword)
	'    Dim ticketTransportConditions As String = Utilities.GetKeywordSearchConditionsForTable(userId, KaTicketTransport.TABLE_NAME, keyword)

	'    Dim ticketIdRdr As OleDbDataReader = Tm2Database.ExecuteReader(GetUserConnection(userId), "SELECT tickets.id " & _
	'                                        "FROM tickets " & _
	'                                        "WHERE " & ticketConditions & " OR " & _
	'                                            "(tickets.id in " & _
	'                                                "(select ticket_id " & _
	'                                                "from ticket_items " & _
	'                                                "where " & ticketItemConditions & " OR " & _
	'                                                    "(ticket_items.id IN " & _
	'                                                        "(select ticket_item_id " & _
	'                                                        "from ticket_bulk_items " & _
	'                                                        "where " & ticketBulkItemConditions & ")))) OR " & _
	'                                            "(tickets.id in " & _
	'                                                "(select ticket_id " & _
	'                                                "from ticket_customer_accounts " & _
	'                                                "where " & ticketCustomerAccountConditions & ")) OR " & _
	'                                            "(tickets.id in " & _
	'                                                "(select ticket_id " & _
	'                                                "from ticket_transports " & _
	'                                                "where " & ticketTransportConditions & "))")
	'    Do While ticketIdRdr.Read()
	'        If Not tickets.Contains(ticketIdRdr.Item("id")) Then tickets.Add(ticketIdRdr.Item("id"))
	'    Loop
	'    ticketIdRdr.Close()
	'    Return tickets
	'End Function

	Public Shared Function IsEmailFieldValid(field As String, ByRef message As String) As Boolean
		message = "" ' clear out the error messages
		Dim valid As Boolean
		If field.Trim().Length > 0 Then
			valid = True ' valid until we find a mistake
			Dim addresses() As String = field.Split(New Char() {",", ";"}) ' multiple e-mail addresses must be separated by commas
			For i As Integer = 0 To addresses.Length - 1
				If addresses(i).Trim().Length > 0 Then
					Dim parts() As String = addresses(i).Split("@")
					If parts.Length <> 2 OrElse parts(0).Trim().Length = 0 OrElse parts(1).Trim().Length = 0 Then
						message = String.Format("The {0} e-mail address is not formatted correctly. Please enter an e-mail address in the following format: user@domain.com.", GetOrdinalNumber(i + 1))
						valid = False
						Exit For ' no need to proceed any further
					End If
				Else
					message = String.Format("The {0} e-mail address is blank. Please either specify an e-mail address or remove the extra comma.", GetOrdinalNumber(i + 1))
					valid = False
					Exit For ' no need to proceed any further
				End If
				Try
					Dim address As New System.Net.Mail.MailAddress(addresses(i), addresses(i))
				Catch ex As FormatException
					message = String.Format("The {0} e-mail address is not formatted correctly. Please enter an e-mail address in the following format: user@domain.com.", GetOrdinalNumber(i + 1))
					valid = False
					Exit For ' no need to proceed any further
				End Try
			Next
		Else ' blank (no e-mail addresses) is valid
			valid = True
		End If
		Return valid
	End Function

	Public Shared Function GetAllEmailAddresses() As List(Of System.Net.Mail.MailAddress)
		Dim addresses As New List(Of System.Net.Mail.MailAddress)
		Dim sql As String = "SELECT DISTINCT '' as name, RTRIM(LTRIM(recipients)) as email, 'emails' AS table_name " &
		   "FROM emails " &
		   "WHERE (recipients > '') AND (NOT (recipients LIKE '%;%')) AND (NOT (recipients LIKE '%,%'))" ' Only accept single addresses
		For Each tableName As String In GetListOfTablesWithEmailAddresses()
			sql &= " UNION SELECT RTRIM(LTRIM(name)), RTRIM(LTRIM(email)), " & Q(tableName) & " AS table_name FROM " & tableName & " WHERE deleted=0 AND email>''"
		Next

		If sql.Length > 0 Then
			Dim getEmailRdr As OleDbDataReader = Tm2Database.ExecuteReader(Tm2Database.Connection, sql & " ORDER BY 1, 2")
			Do While getEmailRdr.Read
				Try
					Dim displayName As String = getEmailRdr.Item("name").trim
					Dim emailAddress As String = getEmailRdr.Item("email").trim
					If displayName.Length = 0 Then displayName = emailAddress
					addresses.Add(New System.Net.Mail.MailAddress(emailAddress, displayName))
				Catch ex As FormatException
					' This is sometimes thrown because of invalid email addresses.  Don't include those.
				End Try
			Loop
			getEmailRdr.Close()
		End If
		Return addresses
	End Function

	''' <summary>
	''' This will return a list of table names that have both the email and deleted fields 
	''' </summary>
	''' <returns></returns>
	''' <remarks></remarks>
	Public Shared Function GetListOfTablesWithEmailAddresses() As List(Of String)
		Dim tableList As New List(Of String)
		Dim tablesRdr As OleDbDataReader = Tm2Database.ExecuteReader(Tm2Database.Connection, "SELECT t.name " &
											 "FROM sys.columns c " &
											 "INNER JOIN sys.tables t ON c.object_id = t.object_id " &
											 "WHERE c.name = 'name' AND t.name IN (SELECT t.name AS TableName FROM sys.columns c INNER JOIN sys.tables t ON c.object_id = t.object_id WHERE c.name = 'email') AND t.name IN (SELECT t.name AS TableName FROM sys.columns c INNER JOIN sys.tables t ON c.object_id = t.object_id WHERE c.name = 'deleted')")
		Do While tablesRdr.Read
			tableList.Add(tablesRdr.Item("name"))
		Loop
		tablesRdr.Close()

		Return tableList
	End Function

	Public Shared Sub PopulateEmailAddressList(ByRef tbxRecipients As System.Web.UI.WebControls.TextBox, ByRef ddlAddEmailAddress As System.Web.UI.WebControls.DropDownList, ByRef btnAddEmailAddress As System.Web.UI.WebControls.Button)
		Dim currentAddresses() As String = tbxRecipients.Text.Split(New Char() {",", ";"})
		ddlAddEmailAddress.Items.Clear()
		ddlAddEmailAddress.Items.Add(New ListItem("", ""))
		For Each email As System.Net.Mail.MailAddress In Utilities.GetAllEmailAddresses()
			Dim address As String = email.Address.Trim
			If Array.IndexOf(currentAddresses, address) < 0 Then
				Dim displayName As String = email.DisplayName.Trim
				If address.ToUpper <> displayName.ToUpper Then
					If displayName.Length = 0 Then
						displayName = address
					Else
						displayName &= " {" & address & "}"
					End If
				End If
				ddlAddEmailAddress.Items.Add(New ListItem(displayName, address))
			End If
		Next
		ddlAddEmailAddress.SelectedIndex = 0
		btnAddEmailAddress.Style("visibility") = "hidden"
	End Sub

	Public Shared Function GetOrdinalNumber(number As Integer) As String
		Dim suffix As String
		Dim offset As Integer = number Mod 10
		If offset = 1 AndAlso number <> 11 Then
			suffix = "st"
		ElseIf offset = 2 AndAlso number <> 12 Then
			suffix = "nd"
		ElseIf offset = 3 AndAlso number <> 13 Then
			suffix = "rd"
		Else
			suffix = "th"
		End If
		Return String.Format("{0:0}{1}", number, suffix)
	End Function

#Region " Custom Fields "
	Public Shared Sub ConvertCustomFieldPanelToLists(ByRef customFields As List(Of KaCustomField), ByRef customFieldData As List(Of KaCustomFieldData), ByVal lstCustomFields As HtmlGenericControl)
		For Each customFieldItem As Object In lstCustomFields.Controls
			If TypeOf customFieldItem Is HtmlGenericControl Then
				For Each childObject As Object In customFieldItem.Controls
					If TypeOf childObject Is HtmlInputHidden Then
						Dim tempData As KaCustomFieldData = Tm2Database.FromXml(CType(childObject, HtmlInputHidden).Value, GetType(KaCustomFieldData))
						Dim data As KaCustomFieldData = Nothing
						For Each possibleData As KaCustomFieldData In customFieldData
							If possibleData.CustomFieldId.Equals(tempData.CustomFieldId) Then
								data = possibleData
							End If
						Next
						Dim customField As KaCustomField = Nothing
						If data IsNot Nothing Then
							For Each possibleCustomField As KaCustomField In customFields
								If possibleCustomField.Id.Equals(data.CustomFieldId) Then
									customField = possibleCustomField
									Exit For
								End If
							Next
						End If
						If customField Is Nothing Then Continue For
						Select Case customField.InputType
							Case KaCustomField.UserInputType.TextBox
								Dim tbx As TextBox = customFieldItem.FindControl("tbxCustomField" & customField.Id.ToString)
								If tbx IsNot Nothing Then data.Value = tbx.Text
							Case KaCustomField.UserInputType.ListSingleSelect
								Dim selectedValues As New List(Of String)
								Dim lst As ListBox = customFieldItem.FindControl("lstCustomField" & customField.Id.ToString)
								If lst IsNot Nothing Then
									For Each item As ListItem In lst.Items
										If item.Selected AndAlso data.Value.Length > 0 Then
											selectedValues.Add(item.Value)
										End If
									Next
								End If
								data.Value = SerializeListOfString(selectedValues)
							Case KaCustomField.UserInputType.ListMultipleSelect, KaCustomField.UserInputType.TableLookupMultipleSelect
								Dim selectedValues As New List(Of String)
								Dim lst As ListBox = customFieldItem.FindControl("lstCustomField" & customField.Id.ToString)
								If lst IsNot Nothing Then
									For Each item As ListItem In lst.Items
										If item.Selected AndAlso data.Value.Length > 0 Then
											selectedValues.Add(item.Value)
										End If
									Next
								End If
								data.Value = SerializeListOfString(selectedValues)
							Case KaCustomField.UserInputType.Combo, KaCustomField.UserInputType.TableLookupSingleSelect
								Dim ddl As DropDownList = customFieldItem.FindControl("ddlCustomField" & customField.Id.ToString)
								If ddl IsNot Nothing Then data.Value = ddl.SelectedValue
							Case KaCustomField.UserInputType.DateTime
								Dim tbx As TextBox = customFieldItem.FindControl("tbxCustomField" & customField.Id.ToString)
								If tbx IsNot Nothing Then data.Value = tbx.Text
							Case KaCustomField.UserInputType.CheckBox
								Dim cbx As CheckBox = customFieldItem.FindControl("cbxCustomField" & customField.Id.ToString)
								If cbx IsNot Nothing Then data.Value = cbx.Checked
						End Select
						Exit For ' Continue to the next list item
					End If
				Next
			End If
		Next
	End Sub

    Public Shared Sub CreateDynamicCustomFieldPanelControls(ByVal customFields As List(Of KaCustomField), ByVal customFieldData As List(Of KaCustomFieldData), ByRef lstCustomFields As HtmlGenericControl, page As System.Web.UI.Page)
        Dim sm As ScriptManager = System.Web.UI.ScriptManager.GetCurrent(page)

        lstCustomFields.Controls.Clear()
        For Each customField As KaCustomField In customFields
            Dim customListItem As New HtmlGenericControl("li")
            Dim customFieldPrompt As New HtmlGenericControl("label")
            customFieldPrompt.InnerText = customField.FieldName
            ' customFieldPrompt.CssClass = "label"
            customListItem.Controls.Add(customFieldPrompt)
            Dim data As KaCustomFieldData = Nothing
            For Each possibleData As KaCustomFieldData In customFieldData
                If possibleData.CustomFieldId.Equals(customField.Id) Then
                    data = possibleData
                    Exit For
                End If
            Next
            If data Is Nothing Then
                data = New KaCustomFieldData
                data.CustomFieldId = customField.Id
                data.Value = ""
                customFieldData.Add(data)
            End If

            Select Case customField.InputType
                Case KaCustomField.UserInputType.TextBox
                    Dim tbx As New TextBox
                    With tbx
                        .ID = "tbxCustomField" & customField.Id.ToString
                        .Text = data.Value
                        .CssClass = "input"
                    End With
                    customListItem.Controls.Add(tbx)
                Case KaCustomField.UserInputType.ListSingleSelect
                    Dim selectedValues As List(Of String) = DeserializeListOfString(data.Value)
                    Dim lst As New ListBox
                    With lst
                        .ID = "lstCustomField" & customField.Id.ToString
                        .SelectionMode = ListSelectionMode.Single
                        For Each listOption As ListItem In DeserializeCustomFieldOptions(customField.Options)
                            .Items.Add(listOption)
                            listOption.Selected = selectedValues.Contains(listOption.Value)
                        Next
                        .CssClass = "input"
                    End With
                    customListItem.Controls.Add(lst)
                Case KaCustomField.UserInputType.ListMultipleSelect
                    Dim selectedValues As List(Of String) = DeserializeListOfString(data.Value)
                    Dim lst As New ListBox
                    With lst
                        .ID = "lstCustomField" & customField.Id.ToString
                        .SelectionMode = ListSelectionMode.Multiple
                        For Each listOption As ListItem In DeserializeCustomFieldOptions(customField.Options)
                            .Items.Add(listOption)
                            listOption.Selected = selectedValues.Contains(listOption.Value)
                        Next
                        .CssClass = "input"
                    End With
                    customListItem.Controls.Add(lst)
                Case KaCustomField.UserInputType.Combo
                    Dim ddl As New DropDownList
                    With ddl
                        .ID = "ddlCustomField" & customField.Id.ToString
                        For Each listOption As ListItem In DeserializeCustomFieldOptions(customField.Options)
                            .Items.Add(listOption)
                        Next
                        Try
                            .SelectedValue = data.Value
                        Catch ex As ArgumentOutOfRangeException

                        End Try
                        .CssClass = "input"
                    End With
                    customListItem.Controls.Add(ddl)
                Case KaCustomField.UserInputType.DateTime
                    Dim span As New HtmlGenericControl("span")
                    span.Attributes("class") = "input"
                    customListItem.Controls.Add(span)

                    Dim tbx As New TextBox
                    With tbx
                        .ID = "tbxCustomField" & customField.Id.ToString
                        .Text = data.Value
                        .CssClass = "input"
                    End With
                    span.Controls.Add(tbx)
                    Dim datePicker As New HtmlGenericControl("script")
                    datePicker.Attributes("type") = "text/javascript"
                    Dim script As String = "$('#" & tbx.ID & "').datetimepicker({" & vbCrLf &
                        "timeFormat: 'h:mm:ss TT'," & vbCrLf &
                        "showSecond: true," & vbCrLf &
                        "showOn: ""both""," & vbCrLf &
                        "buttonImage: 'Images/Calendar_scheduleHS.png'," & vbCrLf &
                        "buttonImageOnly: true," & vbCrLf &
                        "buttonText: ""Show calendar""" & vbCrLf &
                        "});"
                    datePicker.InnerHtml = script
                    span.Controls.Add(datePicker)
                    If sm IsNot Nothing Then
                        ScriptManager.RegisterClientScriptBlock(page, page.GetType(), tbx.ID.Replace("-", "") & "Script", script, True)
                    End If
                Case KaCustomField.UserInputType.CheckBox
                    Dim cbx As New CheckBox
                    With cbx
                        .ID = "cbxCustomField" & customField.Id.ToString
                        Boolean.TryParse(data.Value, .Checked)
                        .Text = customFieldPrompt.InnerText
                        customFieldPrompt.InnerText = ""
                        .CssClass = "input"
                    End With
                    customListItem.Controls.Add(cbx)
                Case KaCustomField.UserInputType.TableLookupSingleSelect
                    Dim ddl As New DropDownList
                    With ddl
                        .ID = "ddlCustomField" & customField.Id.ToString
                        .Items.Clear()

                        Try
                            Dim tableLookup As KaCustomQuestionTableLookup = Tm2Database.FromXml(customField.Options, GetType(KaCustomQuestionTableLookup))
                            Dim r As OleDbDataReader = Tm2Database.ExecuteReader(Tm2Database.Connection, String.Format("SELECT DISTINCT {0} FROM {1} WHERE deleted = 0 ORDER BY {0}", tableLookup.FieldName, tableLookup.TableName))
                            Do While (r.Read())
                                Dim parameter As String = IsNull(r.Item(0).ToString().Trim(), "")
                                Dim listOption As New ListItem(IIf(parameter.Length > 0, parameter, "(blank)"), parameter)
                                .Items.Add(listOption)
                            Loop
                        Catch ex As Exception
                            .Items.Clear()

                            Try
                                Dim tableLookup As KaCustomQuestionTableLookup = Tm2Database.FromXml(customField.Options, GetType(KaCustomQuestionTableLookup))
                                Dim r As OleDbDataReader = Tm2Database.ExecuteReader(Tm2Database.Connection, String.Format("SELECT DISTINCT {0} FROM {1} ORDER BY {0}", tableLookup.FieldName, tableLookup.TableName))
                                Do While (r.Read())
                                    Dim parameter As String = IsNull(r.Item(0).ToString().Trim(), "")
                                    Dim listOption As New ListItem(IIf(parameter.Length > 0, parameter, "(blank)"), parameter)
                                    .Items.Add(listOption)
                                Loop
                            Catch ex2 As Exception
                            End Try
                        End Try
                        Try
                            .SelectedValue = data.Value
                        Catch ex As ArgumentOutOfRangeException

                        End Try
                        .CssClass = "input"
                    End With
                    customListItem.Controls.Add(ddl)
                Case KaCustomField.UserInputType.TableLookupMultipleSelect
                    Dim selectedValues As List(Of String) = DeserializeListOfString(data.Value)
                    Dim lst As New ListBox
                    With lst
                        .ID = "lstCustomField" & customField.Id.ToString
                        .SelectionMode = ListSelectionMode.Multiple
                        Try
                            Dim tableLookup As KaCustomQuestionTableLookup = Tm2Database.FromXml(customField.Options, GetType(KaCustomQuestionTableLookup))
                            Dim r As OleDbDataReader = Tm2Database.ExecuteReader(Tm2Database.Connection, String.Format("SELECT DISTINCT {0} FROM {1} WHERE deleted = 0 ORDER BY {0}", tableLookup.FieldName, tableLookup.TableName))
                            Do While (r.Read())
                                Dim parameter As String = IsNull(r.Item(0).ToString().Trim(), "")
                                Dim listOption As New ListItem(IIf(parameter.Length > 0, parameter, "(blank)"), parameter)
                                .Items.Add(listOption)
                                listOption.Selected = selectedValues.Contains(listOption.Value)
                            Loop
                        Catch ex As Exception
                            Try
                                Dim tableLookup As KaCustomQuestionTableLookup = Tm2Database.FromXml(customField.Options, GetType(KaCustomQuestionTableLookup))
                                Dim r As OleDbDataReader = Tm2Database.ExecuteReader(Tm2Database.Connection, String.Format("SELECT DISTINCT {0} FROM {1} ORDER BY {0}", tableLookup.FieldName, tableLookup.TableName))
                                Do While (r.Read())
                                    Dim parameter As String = IsNull(r.Item(0).ToString().Trim(), "")
                                    Dim listOption As New ListItem(IIf(parameter.Length > 0, parameter, "(blank)"), parameter)
                                    .Items.Add(listOption)
                                    listOption.Selected = selectedValues.Contains(listOption.Value)
                                Loop
                            Catch ex2 As Exception
                            End Try
                        End Try
                        .CssClass = "input"
                    End With
                    customListItem.Controls.Add(lst)
                Case Else
                    Continue For
            End Select
            Dim value As New HtmlInputHidden()
            value.ID = customField.Id.ToString & ""
            value.Value = Tm2Database.ToXml(data, GetType(KaCustomFieldData))
            customListItem.Controls.Add(value)

            lstCustomFields.Controls.Add(customListItem)
        Next
        lstCustomFields.Visible = (lstCustomFields.Controls.Count > 0)
    End Sub

    Public Shared Sub GetCustomField(ByRef displayedText As String, customDataTable As DataTable, ByVal tableName As String, ByVal recordId As Guid)
        Dim customFields As New List(Of String)

        For Each customFieldRow As DataRow In customDataTable.Select("source_table=" & Q(tableName))
            If recordId.Equals(recordId) Then
                Dim value As String = customFieldRow.Item("value").ToString.Trim
                Dim fieldName As String = customFieldRow.Item("field_name")
                Dim displayedValue As String = ""
                If (value.ToLower.StartsWith("<ArrayOfString>".ToLower) AndAlso value.ToLower.EndsWith("</ArrayOfString>".ToLower)) Or value.ToLower.Equals("<ArrayOfString />".ToLower) Then
                    For Each custVal As String In Utilities.DeserializeListOfString(value)
                        If custVal.Length > 0 Then
                            If displayedValue.Length > 0 Then displayedValue &= ", "
                            displayedValue &= custVal
                        End If
                    Next
                ElseIf value.ToLower.Equals("true") OrElse value.ToLower.Equals("false") Then
                    If Boolean.Parse(value) Then
                        displayedValue = "Yes"
                    Else
                        displayedValue = "No"
                    End If
                Else
                    displayedValue = customFieldRow.Item("value")
                End If
                Dim newCustomField As String = fieldName & ": " & displayedValue
                If Not customFields.Contains(newCustomField) Then customFields.Add(newCustomField)
            End If
        Next
        For Each newField As String In customFields
            If displayedText.Length > 0 Then displayedText &= vbCrLf
            displayedText &= newField
        Next
    End Sub

    Public Shared Function SerializeCustomFieldOptions(optionList As List(Of ListItem)) As String
		Dim nsSerializer As New System.Xml.Serialization.XmlSerializerNamespaces
		nsSerializer.Add("", "")
		Dim writerSettings As New System.Xml.XmlWriterSettings
		writerSettings.Indent = False
		writerSettings.OmitXmlDeclaration = True
		Dim xmlHandler As New System.Xml.Serialization.XmlSerializer(GetType(List(Of ListItem)))
		Dim xmlStringWriter As New IO.StringWriter
		Dim xmlWriterObject As System.Xml.XmlWriter = System.Xml.XmlWriter.Create(xmlStringWriter, writerSettings)

		xmlHandler.Serialize(xmlWriterObject, optionList, nsSerializer)

		Return xmlStringWriter.ToString
	End Function

	Public Shared Function DeserializeCustomFieldOptions(ByVal optionList As String) As List(Of ListItem)
		Try
			Dim xmlHandler As New System.Xml.Serialization.XmlSerializer(GetType(List(Of ListItem)))
			Dim xmlStringReader As New IO.StringReader(optionList)
			Return xmlHandler.Deserialize(xmlStringReader)
		Catch ex As InvalidOperationException
			Dim newList As New List(Of ListItem)
			If optionList.Length > 0 Then
				For Each item As String In optionList.Split("|")
					If item.Length > 0 Then newList.Add(New ListItem(item))
				Next
			End If
			Return newList
		End Try
	End Function

	Public Shared Function SerializeListOfString(optionList As List(Of String)) As String
		Dim nsSerializer As New System.Xml.Serialization.XmlSerializerNamespaces
		nsSerializer.Add("", "")
		Dim writerSettings As New System.Xml.XmlWriterSettings
		writerSettings.Indent = False
		writerSettings.OmitXmlDeclaration = True
		Dim xmlHandler As New System.Xml.Serialization.XmlSerializer(GetType(List(Of String)))
		Dim xmlStringWriter As New IO.StringWriter
		Dim xmlWriterObject As System.Xml.XmlWriter = System.Xml.XmlWriter.Create(xmlStringWriter, writerSettings)

		xmlHandler.Serialize(xmlWriterObject, optionList, nsSerializer)

		Return xmlStringWriter.ToString
	End Function

	Public Shared Function DeserializeListOfString(ByVal stringList As String) As List(Of String)
		Try
			Dim xmlHandler As New System.Xml.Serialization.XmlSerializer(GetType(List(Of String)))
			Dim xmlStringReader As New IO.StringReader(stringList)
			Return xmlHandler.Deserialize(xmlStringReader)
		Catch ex As InvalidOperationException
			Dim newList As New List(Of String)
			If stringList.Length > 0 Then newList.Add(stringList)
			Return newList
		End Try
	End Function
#End Region

	Public Shared Function ConvertWebPageUrlDomainToRequestedPagesDomain(ByVal connection As OleDbConnection) As Boolean
		Dim retVal As Boolean = False
		Boolean.TryParse(KaSetting.GetSetting(connection, GeneralSettings.SN_CONVERT_WEB_PAGE_URL_DOMAIN_TO_REQUESTED_DOMAIN, True), retVal)
		Return retVal
	End Function

	Public Shared Sub CreateEventLogEntry(ByVal category As KaEventLog.Categories, ByVal description As String, ByVal connection As OleDbConnection)
		CreateEventLogEntry(category, description, connection, 0)
	End Sub

	Public Shared Sub CreateEventLogEntry(ByVal category As KaEventLog.Categories, ByVal description As String, ByVal connection As OleDbConnection, ByVal resendIntervalHours As Integer)
		Try
			Dim eventLog As New KaEventLog
			With eventLog
				.ApplicationIdentifier = "TM2"
				.ApplicationVersion = Tm2Database.FormatVersion(System.Reflection.Assembly.GetExecutingAssembly().GetName.Version, "X")
				.Category = category
				.Computer = My.Computer.Name
				.Description = description
			End With
			KaEventLog.CreateEventLog(connection, eventLog, resendIntervalHours, Nothing, eventLog.Computer & "/" & eventLog.ApplicationIdentifier, "")
		Catch ex As Exception

		End Try
	End Sub

#Region " Sort List Control Items "
	Public Enum ListControlSortBy
		TextAsc
		ValueAsc
		TextDesc
		ValueDesc
	End Enum

	Public Shared Sub SortListControlList(ByRef lc As ListControl, ByVal sortType As ListControlSortBy)
		Dim currentSelectedId As String = ""
		If lc.SelectedIndex >= 0 Then currentSelectedId = lc.SelectedValue

		Dim t As New List(Of ListItem)()
		Dim compare As Object = Nothing
		Select Case sortType
			Case ListControlSortBy.TextAsc
				compare = New Comparison(Of ListItem)(AddressOf CompareListItemsByTextAsc)
			Case ListControlSortBy.TextDesc
				compare = New Comparison(Of ListItem)(AddressOf CompareListItemsByTextDesc)
			Case ListControlSortBy.ValueAsc
				compare = New Comparison(Of ListItem)(AddressOf CompareListItemsByValueAsc)
			Case ListControlSortBy.ValueDesc
				compare = New Comparison(Of ListItem)(AddressOf CompareListItemsByValueDesc)
			Case Else
				compare = New Comparison(Of ListItem)(AddressOf CompareListItemsByTextAsc)
		End Select
		For Each lbItem As ListItem In lc.Items
			t.Add(lbItem)
		Next
		t.Sort(compare)
		lc.Items.Clear()
		lc.Items.AddRange(t.ToArray())

		Try
			lc.SelectedValue = currentSelectedId
		Catch ex As ArgumentOutOfRangeException

		End Try
	End Sub

	Private Shared Function CompareListItemsByTextAsc(ByVal li1 As ListItem, ByVal li2 As ListItem) As Integer
		Return String.Compare(li1.Text, li2.Text)
	End Function

	Private Shared Function CompareListItemsByTextDesc(ByVal li1 As ListItem, ByVal li2 As ListItem) As Integer
		Return String.Compare(li2.Text, li1.Text)
	End Function

	Private Shared Function CompareListItemsByValueAsc(ByVal li1 As ListItem, ByVal li2 As ListItem) As Integer
		Return String.Compare(li1.Value, li2.Value)
	End Function

	Private Shared Function CompareListItemsByValueDesc(ByVal li1 As ListItem, ByVal li2 As ListItem) As Integer
		Return String.Compare(li2.Value, li1.Value)
	End Function
#End Region

#Region " Generate HTML for object "
	Public Shared Function GenerateHTML(ByVal objControl As Control) As String
		Dim sb As New System.Text.StringBuilder
		GenerateHTML(objControl, sb)
		Return sb.ToString()
	End Function

	Public Shared Sub GenerateHTML(ByVal objControl As Control, ByVal sb As System.Text.StringBuilder)
		Dim str As New System.Text.StringBuilder
		Dim sw As New System.IO.StringWriter(str)
		Dim hw As New System.Web.UI.HtmlTextWriter(sw)
		If TypeOf (objControl) Is HtmlTable Then
			sb.Append("<table ")
			CType(objControl, HtmlTable).Attributes.Render(hw)
			sb.Append(str.ToString & " >")
			Dim objControl1 As Control
			For Each objControl1 In objControl.Controls
				GenerateHTML(objControl1, sb)
			Next
			sb.Append("</table>")
		ElseIf TypeOf (objControl) Is Table Then
			sb.Append("<table ")
			CType(objControl, Table).Attributes.Render(hw)
			sb.Append(str.ToString & " >")
			Dim objControl1 As Control
			For Each objControl1 In objControl.Controls
				GenerateHTML(objControl1, sb)
			Next
			sb.Append("</table>")
		ElseIf TypeOf (objControl) Is HtmlTableRow Then
			sb.Append("<tr ")
			CType(objControl, HtmlTableRow).Attributes.Render(hw)
			sb.Append(str.ToString & " >")
			Dim objControl1 As Control
			For Each objControl1 In objControl.Controls
				GenerateHTML(objControl1, sb)
			Next
			sb.Append("</tr>")
		ElseIf TypeOf (objControl) Is TableRow Then
			sb.Append("<tr ")
			CType(objControl, TableRow).Attributes.Render(hw)
			sb.Append(str.ToString & " >")
			Dim objControl1 As Control
			For Each objControl1 In objControl.Controls
				GenerateHTML(objControl1, sb)
			Next
			sb.Append("</tr>")
		ElseIf TypeOf (objControl) Is HtmlTableCell Then
			sb.Append("<td ")
			CType(objControl, HtmlTableCell).Attributes.Render(hw)
			sb.Append(str.ToString & " >")
			If CType(objControl, HtmlTableCell).Controls.Count > 0 Then
				Dim objControl1 As Control
				For Each objControl1 In objControl.Controls
					GenerateHTML(objControl1, sb)
				Next
			Else
				sb.Append(CType(objControl, TableCell).Text)
			End If
			sb.Append("</td>")
		ElseIf TypeOf (objControl) Is TableCell Then
			sb.Append("<td ")
			CType(objControl, TableCell).Attributes.Render(hw)
			sb.Append(str.ToString & " >")
			If CType(objControl, TableCell).Controls.Count > 0 Then
				Dim objControl1 As Control
				For Each objControl1 In objControl.Controls
					GenerateHTML(objControl1, sb)
				Next
			Else
				sb.Append(CType(objControl, TableCell).Text)
			End If
			sb.Append("</td>")
		ElseIf TypeOf (objControl) Is LiteralControl Then
			sb.Append(CType(objControl, LiteralControl).Text)
		ElseIf TypeOf (objControl) Is TextBox Then
			sb.Append(CType(objControl, TextBox).Text)
		ElseIf TypeOf (objControl) Is Label Then
			sb.Append(CType(objControl, Label).Text)
		ElseIf TypeOf (objControl) Is CheckBox Then
			Dim chk As CheckBox = CType(objControl, CheckBox)
			sb.Append("<input type='checkbox'" & IIf(chk.Checked, "Checked ", " >") & chk.Text)
		ElseIf TypeOf (objControl) Is RadioButton Then
			Dim rad As RadioButton = CType(objControl, RadioButton)
			sb.Append("<input type='radio'" & IIf(rad.Checked, "Checked >", " >") & rad.Text)
		End If
	End Sub
#End Region

	Public Shared Function CreateSiteCssStyle() As String
		Dim cssString As String = "<style type=""text/css"">"
		Try
			Using sr As New StreamReader(My.Request.PhysicalApplicationPath & "styles\site.css")
				Dim line As String
				' Read and display lines from the file until the end of
				' the file is reached.
				Do
					line = sr.ReadLine()
					If Not (line Is Nothing) Then
						cssString &= line
					End If
				Loop Until line Is Nothing
			End Using

		Catch ex As Exception

		End Try
		cssString &= "</style>"

		Return cssString
	End Function

	Public Shared Function GetUserTm2Permission(user As KaUser) As KaUserProfile
		Return New KaUserProfile(GetUserConnection(user.Id), user.UserProfileId)
	End Function

	Public Shared Function GetUserPagePermission(user As KaUser, tableNames As List(Of String), originalPermissionName As String, Optional authCheck As Boolean = True) As Dictionary(Of String, KaTablePermission)
		If (authCheck) Then
			If Not Common.CheckAuthorized(True, False) Then Throw New KaLicenseActivation.Exceptions.NotActivated()
		End If

		Dim permissions As New Dictionary(Of String, KaTablePermission)
		Dim up As KaUserProfile = Nothing
		Try
			up = GetUserTm2Permission(user)
		Catch ex As RecordNotFoundException

		End Try

		For Each tableName As String In tableNames
			If Not permissions.ContainsKey(tableName) Then
				Try
					If up Is Nothing Then Throw New RecordNotFoundException("User profile not defined for user.")
					permissions.Add(tableName, up.GetPermissionsForTable(tableName))
				Catch ex As RecordNotFoundException When originalPermissionName.Length > 0
					permissions.Add(tableName, GetUserTablePermissionFromPermissions(user, originalPermissionName))
				End Try
			End If
		Next
		Return permissions
	End Function

	Public Shared Function GetUserTablePermissionFromPermissions(user As KaUser, originalPermissionName As String) As KaTablePermission
		Dim tp As KaTablePermission = New KaTablePermission()
		With tp
			Select Case user.GetPermissionValueByName(originalPermissionName)
				Case "M"
					.Edit = True
					.Create = True
					.Delete = True
					.Read = True
				Case "V"
					.Read = True
			End Select
			.TableName = KaUser.TABLE_NAME
		End With
		Return tp
	End Function

	Public Shared Function GetContainerPackagedInventoryLocationId(connection As OleDbConnection) As Guid
		Dim currentDefaultPackagedInvLocId As Guid = Guid.Empty
		Dim locationList As ArrayList = KaLocation.GetAll(connection, "deleted = 0 AND " & KaLocation.FN_IS_CONTAINER_PACKAGED_INVENTORY & " = 1", "Name")
		If locationList.Count > 0 Then
			currentDefaultPackagedInvLocId = locationList(0).Id
		End If
		If currentDefaultPackagedInvLocId.Equals(Guid.Empty) Then
			Guid.TryParse(KaSetting.GetSetting(connection, "PackagedInventoryLocationId", "", False, Nothing), currentDefaultPackagedInvLocId)

			If currentDefaultPackagedInvLocId.Equals(Guid.Empty) Then
				For Each setting As KaSetting In KaSetting.GetAll(connection, "deleted=0 AND [name] LIKE '%/CF2/PackagedInventoryLocation'", "")
					Try ' to parse the packaged inventory location ID from the setting...
						currentDefaultPackagedInvLocId = Guid.Parse(setting.Value)
						If Not currentDefaultPackagedInvLocId.Equals(Guid.Empty) Then Exit For
					Catch ex As FormatException ' the ID wasn't in a usable format...
						' ignore this setting
					End Try
				Next
			End If
		End If
		Return currentDefaultPackagedInvLocId
	End Function

	Public Shared Sub RemoveQueryString(queryString As NameValueCollection, parameterName As String)
		If queryString(parameterName) IsNot Nothing Then
			Dim isreadonly As System.Reflection.PropertyInfo = GetType(System.Collections.Specialized.NameValueCollection).GetProperty("IsReadOnly", System.Reflection.BindingFlags.Instance Or System.Reflection.BindingFlags.NonPublic)

			' make collection editable
			isreadonly.SetValue(queryString, False, Nothing)

			' remove
			queryString.Remove(parameterName)
		End If
	End Sub

	Public Shared Function GetMimeType(imageFormat As System.Drawing.Imaging.ImageFormat) As String
		If imageFormat.Guid.Equals(System.Drawing.Imaging.ImageFormat.Bmp.Guid) Then
			Return "bmp"
		ElseIf imageFormat.Guid.Equals(System.Drawing.Imaging.ImageFormat.Emf.Guid) Then
			Return "emf"
		ElseIf imageFormat.Guid.Equals(System.Drawing.Imaging.ImageFormat.Exif.Guid) Then
			Return "exif"
		ElseIf imageFormat.Guid.Equals(System.Drawing.Imaging.ImageFormat.Gif.Guid) Then
			Return "gif"
		ElseIf imageFormat.Guid.Equals(System.Drawing.Imaging.ImageFormat.Icon.Guid) Then
			Return "icon"
		ElseIf imageFormat.Guid.Equals(System.Drawing.Imaging.ImageFormat.Jpeg.Guid) Then
			Return "jpeg"
		ElseIf imageFormat.Guid.Equals(System.Drawing.Imaging.ImageFormat.MemoryBmp.Guid) Then
			Return "membmp"
		ElseIf imageFormat.Guid.Equals(System.Drawing.Imaging.ImageFormat.Png.Guid) Then
			Return "png"
		ElseIf imageFormat.Guid.Equals(System.Drawing.Imaging.ImageFormat.Tiff.Guid) Then
			Return "tiff"
		ElseIf imageFormat.Guid.Equals(System.Drawing.Imaging.ImageFormat.Wmf.Guid) Then
			Return "wmf"
		Else
			Return "unknown"
		End If
	End Function

	Public Shared Function NearlyEqual(ByVal a As Double, ByVal b As Double, ByVal epsilon As Double) As Boolean
		Dim absA As Double = Math.Abs(a)
		Dim absB As Double = Math.Abs(b)
		Dim diff As Double = Math.Abs(a - b)

		If a = b Then ' shortcut, Handles infinities
			Return True
		ElseIf (a = 0 OrElse b = 0 OrElse diff < Double.Epsilon) Then
			' a Or b Is zero Or both are extremely close to it
			' relative error Is less meaningful here
			Return diff < (epsilon * Double.Epsilon)
		Else ' use relative error
			Return diff / Math.Min((absA + absB), Double.MaxValue) < epsilon
		End If
	End Function
End Class
