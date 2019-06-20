Imports System.Data.OleDb
Imports KahlerAutomation.KaTm2Database

Module Settings

#Region "Constants"
    Public Const SN_CREATE_NEW_DESTINATION_FROM_ORDER_SHIP_TO_INFORMATION As String = "General/CreateNewDestinationFromOrderShipToInformation"
    Public Const SD_CREATE_NEW_DESTINATION_FROM_ORDER_SHIP_TO_INFORMATION As String = "False"
#End Region

#Region "Functions"

    Public Function GetMySetting(connection As OleDbConnection) As Boolean
       Return Boolean.Parse(KaSetting.GetSetting(connection, SN_CREATE_NEW_DESTINATION_FROM_ORDER_SHIP_TO_INFORMATION, "False"))
    End Function

    Public Sub SetMySetting(connection As OleDbConnection, value As Boolean)
        KaSetting.WriteSetting(connection, SN_CREATE_NEW_DESTINATION_FROM_ORDER_SHIP_TO_INFORMATION, value.ToString())
    End Sub

#End Region

End Module
