Imports KahlerAutomation.KaTm2Database
Imports System.Net
Imports System.Data.OleDb
Imports System.IO
Imports System.Environment

Module Reports
	Public Function GetUnitById(ByVal id As Guid, ByRef list As ArrayList, ByVal currentUser As KaUser) As KaUnit
		Dim i As Integer = Utilities.FindId(id, list)
		If i = -1 Then
			list.Add(New KaUnit(GetUserConnection(currentUser.Id), id))
			i = list.Count - 1
		End If
		Return list(i)
	End Function

	Public Function GetCustomerAccountById(ByVal id As Guid, ByRef list As ArrayList, ByVal currentUser As KaUser) As KaCustomerAccount
		Dim i As Integer = Utilities.FindId(id, list)
		If i = -1 Then
			list.Add(New KaCustomerAccount(GetUserConnection(currentUser.Id), id))
			i = list.Count - 1
		End If
		Return list(i)
	End Function

	Public Function GetProductById(ByVal id As Guid, ByRef list As ArrayList, ByVal currentUser As KaUser) As KaProduct
		Dim i As Integer = Utilities.FindId(id, list)
		If i = -1 Then
			list.Add(New KaProduct(GetUserConnection(currentUser.Id), id))
			i = list.Count - 1
		End If
		Return list(i)
	End Function

	Public Property OrderSummaryUrlForOwner(connection As OleDbConnection, ownerId As Guid) As String
		Get
			Return KaSetting.GetSetting(connection, KaSetting.SN_ORDER_SUMMARY_URL_FOR_OWNER & ownerId.ToString(), KaSetting.SD_ORDER_SUMMARY_URL_FOR_OWNER)
		End Get
		Set(value As String)
			KaSetting.WriteSetting(connection, KaSetting.SN_ORDER_SUMMARY_URL_FOR_OWNER & ownerId.ToString(), value)
		End Set
	End Property

	Public Property WebTicketUrlForOwner(connection As OleDbConnection, ownerId As Guid) As String
		Get
			Return KaSetting.GetSetting(connection, "TerminalManagement2/TicketAddress/OwnerId=" & ownerId.ToString(), "http://localhost/TerminalManagement2/ticket.aspx")
		End Get
		Set(value As String)
			KaSetting.WriteSetting(connection, "TerminalManagement2/TicketAddress/OwnerId=" & ownerId.ToString(), value)
		End Set
	End Property

	Public Property ReceivingPoWebTicketUrlForOwner(connection As OleDbConnection, ownerId As Guid) As String
		Get
			Dim bogusEntry As String = Guid.NewGuid.ToString
			Dim ownerTicketAddress As String = KaSetting.GetSetting(connection, "Receiving_PO_Web_Ticket_Address/OwnerId=" & ownerId.ToString(), bogusEntry, False, Nothing)
			If ownerTicketAddress = bogusEntry Then ownerTicketAddress = KaSetting.GetSetting(connection, "Receiving_PO_Web_Ticket_Address", "http://localhost/TerminalManagement2/ReceivingTicket.aspx", False, Nothing)
			Return ownerTicketAddress
		End Get
		Set(value As String)
			KaSetting.WriteSetting(connection, "Receiving_PO_Web_Ticket_Address/OwnerId=" & ownerId.ToString(), value)
		End Set
	End Property

	Public Property NextOrderNumberForOwner(ownerId As Guid, user As KaUser)
		Get
			Return KaSetting.GetSetting(GetUserConnection(user.Id), KaSetting.SN_NEXT_ORDER_NUMBER_FOR_OWNER & ownerId.ToString(), KaSetting.SD_NEXT_ORDER_NUMBER_FOR_OWNER)
		End Get
		Set(value)
			KaSetting.WriteSetting(GetUserConnection(user.Id), KaSetting.SN_NEXT_ORDER_NUMBER_FOR_OWNER & ownerId.ToString(), value)
		End Set
	End Property

	Public Property NextTicketNumberForOwner(ownerId As Guid, user As KaUser)
		Get
			Return KaSetting.GetSetting(GetUserConnection(user.Id), KaSetting.SN_NEXT_TICKET_NUMBER_FOR_OWNER & ownerId.ToString(), KaSetting.SD_NEXT_TICKET_NUMBER_FOR_OWNER)
		End Get
		Set(value)
			KaSetting.WriteSetting(GetUserConnection(user.Id), KaSetting.SN_NEXT_TICKET_NUMBER_FOR_OWNER & ownerId.ToString(), value)
		End Set
	End Property

	Public Function GetOwnerIdForTicket(ByVal connection As OleDbConnection, ByVal ticketId As Guid) As Guid
		Dim ticket As KaTicket = New KaTicket(connection, ticketId)
		If ticket.OwnerId = Guid.Empty Then
			'This will only get hit with Legacy code that does not stamp owner id to ticket
			Dim r As OleDbDataReader = Tm2Database.ExecuteReader(connection, "SELECT orders.owner_id FROM orders, tickets WHERE tickets.id=" & Q(ticketId) & " AND orders.id = tickets.order_id")
			If r.Read() Then Return r(0)
			r.Close()
		Else
			Return ticket.OwnerId
		End If
	End Function
End Module
