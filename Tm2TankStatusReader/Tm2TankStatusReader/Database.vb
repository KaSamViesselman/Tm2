Imports System.Xml
Imports System.Data.OleDb
Imports KahlerAutomation.KaTm2Database
Imports System.IO
Imports System.Net.Dns
Imports KahlerAutomation.KaModbusTcp

Public Module Database
	Public ApplicationIdentifier As String = String.Format("{0}/Tm2TankStatusReader", GetHostName())

	Private Function GetLocationList() As ArrayList
		Dim list As New ArrayList()
		For Each r As KaLocation In KaLocation.GetAll(Tm2Database.Connection, "deleted=0", "")
			If Boolean.Parse(KaSetting.GetSetting(Tm2Database.Connection, String.Format("@{0}/Tm2TankStatusReader/LocationEnabled/{1}", GetHostName(), r.Id.ToString()), "False")) Then list.Add(r.Id)
		Next
		Return list
	End Function

	Public Function GetPanelList() As List(Of KaPanel)
		Dim list As New List(Of KaPanel)
		Dim conditions As String = ""
		For Each locationId As Guid In GetLocationList()
			conditions &= IIf(conditions.Length > 0, " OR", "") & " location_id=" & Q(locationId)
		Next
		conditions = String.Format("deleted=0 AND role={0:0}{1}", CType(KaPanel.PanelRole.TLM5, Integer), IIf(conditions.Length > 0, " AND (" & conditions & ")", ""))
		For Each panel In KaPanel.GetAll(Tm2Database.Connection, conditions, "name ASC")
			list.Add(panel)
		Next
		Return list
	End Function

	Public Sub UpdateTank(panel As KaPanel, tank As KaTank, c As ModbusTcpCommand, offset As Integer)
		If tank.Name = "" Then ' if the name is blank, use the name read from the KA-2000
			Dim name As String = ""
			For j As Integer = 8 To 17 : name &= WordToString(c.Data(offset + j)) : Next

			If name.Trim.Length = 0 Then
				name = $"{panel.Name} - Tank {tank.Sensor + 1}"
			End If

			tank.Name = name.Trim()
		End If
		Dim status As UShort = c.Data(offset)
		Dim level As Double = c.Data(offset + 1) / 10
		Dim quantity As Double = CDbl(c.Data(offset + 2)) + CDbl(c.Data(offset + 3)) * (CDbl(UShort.MaxValue) + 1)
		Dim si As UInt16 = 0
		Dim flowRate As Double = c.Data(offset + 4)
		Dim height As Double = c.Data(offset + 5) / 10
		Dim capacity As Double = CDbl(c.Data(offset + 6)) + CDbl(c.Data(offset + 7)) * (CDbl(UShort.MaxValue) + 1)

		tank.UpdateTank(Tm2Database.Connection, Nothing, status, level, quantity, si, flowRate, height, capacity, ApplicationIdentifier, "")
	End Sub

	Private Function WordToString(word As UShort) As String
		Dim firstLetter As Char = ""
		Try
			If (word And &HFF&) > 0 Then firstLetter = Chr(word And &HFF&)
		Catch ex As Exception
		End Try
		Dim secondLetter As Char = ""
		Try
			If ((word And &HFF00&) >> 8) > 0 Then secondLetter = Chr((word And &HFF00&) >> 8)
		Catch ex As Exception
		End Try
		Dim retval As String = ""
		If firstLetter <> vbNullChar AndAlso firstLetter <> vbNullString Then retval &= firstLetter
		If secondLetter <> vbNullChar AndAlso secondLetter <> vbNullString Then retval &= secondLetter
		Return retval
	End Function
End Module
