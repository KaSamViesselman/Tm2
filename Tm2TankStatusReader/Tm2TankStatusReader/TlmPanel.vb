Imports KahlerAutomation.KaTm2Database
Imports KahlerAutomation.KaModbusTcp
Public Class TlmPanel
	Public Sub New(panel As KaPanel, notifyOn As ModbusTcpNetwork.DataReceived)
		_panel = panel
		_network = New ModbusTcpNetwork()
		_network.TcpAddress = panel.IpAddress
		_network.TcpPort = panel.TcpPort
		_network.NotifyOnDataReceived(notifyOn)
		If _network.TcpAddress.Trim().Length > 0 AndAlso _network.TcpPort > 0 Then _network.Open()
		For i As Integer = 0 To 31
			_tanksRead(i) = False
		Next
	End Sub

	Private _network As ModbusTcpNetwork
	Public Property Network As ModbusTcpNetwork
		Get
			Return _network
		End Get
		Set(ByVal value As ModbusTcpNetwork)
			_network = value
		End Set
	End Property

	Private _panel As KaPanel
	Public Property Panel As KaPanel
		Get
			Return _panel
		End Get
		Set(ByVal value As KaPanel)
			_panel = value
		End Set
	End Property

	Private _tanksRead(31) As Boolean
	Public Property TanksRead(index As Integer) As Boolean
		Get
			Return _tanksRead(index)
		End Get
		Set(value As Boolean)
			_tanksRead(index) = value
		End Set
	End Property

	Public Property MaximumNumberOfTanks As Integer
		Get
			Return _tanksRead.Length
		End Get
		Set(value As Integer)
			If value <> _tanksRead.Length Then ReDim _tanksRead(value - 1)
		End Set
	End Property

	Public Property TankDataRegisterLength As Integer = 24
End Class
