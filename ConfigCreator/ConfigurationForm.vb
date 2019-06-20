Imports KahlerAutomation.KaTm2Database

Public Class ConfigurationForm
	Private _configFileName As String = "C:\Kaco\TerminalManagement2\Config.xml"
	Private _currentConnectionSettings As New ConnectionSettings()

	Private Sub ConfigurationForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
		Dim testInteger As Integer

		With _currentConnectionSettings
			.UseDefaultConnectionRetries = Not Integer.TryParse(Tm2Database.GetXmlSetting(Tm2Database.SN_DB_CONNECTION_RETRIES_SETTING, ""), testInteger)
			.UseDefaultQueryRetries = Not Integer.TryParse(Tm2Database.GetXmlSetting(Tm2Database.SN_DB_QUERY_RETRIES_SETTING, ""), testInteger)
			.UseDefaultRetryWait = Not Integer.TryParse(Tm2Database.GetXmlSetting(Tm2Database.SN_DB_RETRY_WAIT_SETTING, ""), testInteger)
			.UseDefaultCommandTimeout = Not Integer.TryParse(Tm2Database.GetXmlSetting(Tm2Database.SN_DB_COMMAND_TIMEOUT_SETTING, ""), testInteger)
			.DataConnection = Tm2Database.GetDbConnection()
			.CommandTimeout = Tm2Database.CommandTimeout
			.ConnectionRetries = Tm2Database.ConnectionRetries
			.QueryRetries = Tm2Database.QueryRetries
			.RetryWait = Tm2Database.RetryWait
		End With
		SetObjectsFromCommunicationSettings()
		btnCreateConfigXml.Visible = IO.Directory.Exists(IO.Path.GetDirectoryName(_configFileName)) AndAlso Not IO.File.Exists(_configFileName)
	End Sub

	Private Sub SetObjectsFromCommunicationSettings()
		With _currentConnectionSettings
			tbxConnectionString.Text = .DataConnection
			nudCommandTimeout.Value = .CommandTimeout
			cbxUseDefaultCommandTimeout.Checked = .UseDefaultCommandTimeout
			cbxUseDefaultCommandTimeout_CheckedChanged(cbxUseDefaultCommandTimeout, Nothing)
			nudConnectionRetries.Value = .ConnectionRetries
			cbxUseDefaultConnectionRetries.Checked = .UseDefaultConnectionRetries
			cbxUseDefaultConnectionRetries_CheckedChanged(cbxUseDefaultConnectionRetries, Nothing)
			nudQueryRetries.Value = .QueryRetries
			cbxUseDefaultQueryRetries.Checked = .UseDefaultQueryRetries
			cbxUseDefaultQueryRetries_CheckedChanged(cbxUseDefaultQueryRetries, Nothing)
			nudRetryWait.Value = .RetryWait
			cbxUseDefaultRetryWait.Checked = .UseDefaultRetryWait
			cbxUseDefaultRetryWait_CheckedChanged(cbxUseDefaultRetryWait, Nothing)
		End With
	End Sub

	Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
		Dim d As System.Xml.XmlDocument = Nothing
		Try
			If IO.File.Exists(_configFileName) Then
				d = New System.Xml.XmlDocument()
				d.Load(_configFileName)
			End If
            Dim result As Boolean = SaveSettings(d)
            If result Then
                If d IsNot Nothing Then d.Save(_configFileName)
                Windows.Forms.MessageBox.Show("Settings saved successfully.")
                'System.Threading.Thread.Sleep(50)
                Me.Close()
            End If
        Catch ex As Exception
			Windows.Forms.MessageBox.Show(ex.ToString())
		End Try
	End Sub

	Private Function SaveSettings(d As System.Xml.XmlDocument)
        If tbxConnectionString.Text.Trim.Length = 0 Then
            Windows.Forms.MessageBox.Show("A database connection string must be specified to save the settings.", "Invalid connection string", Windows.Forms.MessageBoxButtons.OK, Windows.Forms.MessageBoxIcon.Exclamation)
            Return False
        End If
        Try
                If d IsNot Nothing Then
                    SetXmlSetting(d, Tm2Database.SN_DB_CONNECTION_STRING_SETTING, tbxConnectionString.Text)
                    If cbxUseDefaultCommandTimeout.Checked Then
                        ClearXmlSetting(d, Tm2Database.SN_DB_COMMAND_TIMEOUT_SETTING)
                        PopulateXmlSettings(d, Tm2Database.SN_DB_COMMAND_TIMEOUT_SETTING & "_RemoveThisToUse", "30")
                    Else
                        SetXmlSetting(d, Tm2Database.SN_DB_COMMAND_TIMEOUT_SETTING, nudCommandTimeout.Value)
                        ClearXmlSetting(d, Tm2Database.SN_DB_COMMAND_TIMEOUT_SETTING & "_RemoveThisToUse")
                    End If
                    If cbxUseDefaultConnectionRetries.Checked Then
                        ClearXmlSetting(d, Tm2Database.SN_DB_CONNECTION_RETRIES_SETTING)
                        PopulateXmlSettings(d, Tm2Database.SN_DB_CONNECTION_RETRIES_SETTING & "_RemoveThisToUse", "3")
                    Else
                        SetXmlSetting(d, Tm2Database.SN_DB_CONNECTION_RETRIES_SETTING, nudConnectionRetries.Value)
                        ClearXmlSetting(d, Tm2Database.SN_DB_CONNECTION_RETRIES_SETTING & "_RemoveThisToUse")
                    End If
                    If cbxUseDefaultQueryRetries.Checked Then
                        ClearXmlSetting(d, Tm2Database.SN_DB_QUERY_RETRIES_SETTING)
                        PopulateXmlSettings(d, Tm2Database.SN_DB_QUERY_RETRIES_SETTING & "_RemoveThisToUse", "3")
                    Else
                        SetXmlSetting(d, Tm2Database.SN_DB_QUERY_RETRIES_SETTING, nudQueryRetries.Value)
                        ClearXmlSetting(d, Tm2Database.SN_DB_QUERY_RETRIES_SETTING & "_RemoveThisToUse")
                    End If
                    If cbxUseDefaultRetryWait.Checked Then
                        ClearXmlSetting(d, Tm2Database.SN_DB_RETRY_WAIT_SETTING)
                        PopulateXmlSettings(d, Tm2Database.SN_DB_RETRY_WAIT_SETTING & "_RemoveThisToUse", "100")
                    Else
                        SetXmlSetting(d, Tm2Database.SN_DB_RETRY_WAIT_SETTING, nudRetryWait.Value)
                        ClearXmlSetting(d, Tm2Database.SN_DB_RETRY_WAIT_SETTING & "_RemoveThisToUse")
                    End If
                Else
                    SetRegistrySetting(Tm2Database.SN_DB_CONNECTION_STRING_SETTING, tbxConnectionString.Text)
                    If cbxUseDefaultCommandTimeout.Checked Then
                        ClearRegistrySetting(Tm2Database.SN_DB_COMMAND_TIMEOUT_SETTING)
                    Else
                        SetRegistrySetting(Tm2Database.SN_DB_COMMAND_TIMEOUT_SETTING, nudCommandTimeout.Value)
                    End If
                    If cbxUseDefaultConnectionRetries.Checked Then
                        ClearRegistrySetting(Tm2Database.SN_DB_CONNECTION_RETRIES_SETTING)
                    Else
                        SetRegistrySetting(Tm2Database.SN_DB_CONNECTION_RETRIES_SETTING, nudConnectionRetries.Value)
                    End If
                    If cbxUseDefaultQueryRetries.Checked Then
                        ClearRegistrySetting(Tm2Database.SN_DB_QUERY_RETRIES_SETTING)
                    Else
                        SetRegistrySetting(Tm2Database.SN_DB_QUERY_RETRIES_SETTING, nudQueryRetries.Value)
                    End If
                    If cbxUseDefaultRetryWait.Checked Then
                        ClearRegistrySetting(Tm2Database.SN_DB_RETRY_WAIT_SETTING)
                    Else
                        SetRegistrySetting(Tm2Database.SN_DB_RETRY_WAIT_SETTING, nudRetryWait.Value)
                    End If
                End If
                Return True
            Catch ex As Exception
                Windows.Forms.MessageBox.Show(ex.ToString())
			Return False
		End Try
	End Function

	Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
		Me.Close()
	End Sub

	Private Sub btnImportSettings_Click(sender As Object, e As EventArgs) Handles btnImportSettings.Click
		If openFileDialog1.ShowDialog(Me) = Windows.Forms.DialogResult.OK Then
			Try
				_currentConnectionSettings = Tm2Database.FromXml(IO.File.ReadAllText(openFileDialog1.FileName), GetType(ConnectionSettings))
				SetObjectsFromCommunicationSettings()
				Windows.Forms.MessageBox.Show("Settings imported successfully.")
			Catch ex As Exception
				Windows.Forms.MessageBox.Show(ex.ToString())
			End Try
		End If
	End Sub

	Private Sub btnExportSettings_Click(sender As Object, e As EventArgs) Handles btnExportSettings.Click
		If saveFileDialog1.ShowDialog(Me) = Windows.Forms.DialogResult.OK Then
			Try
				IO.File.WriteAllText(saveFileDialog1.FileName, Tm2Database.ToXml(_currentConnectionSettings, GetType(ConnectionSettings)))
				Windows.Forms.MessageBox.Show("Settings exported successfully.")
			Catch ex As Exception
				Windows.Forms.MessageBox.Show(ex.ToString())
			End Try
		End If
	End Sub

	Private Sub cbxUseDefaultConnectionRetries_CheckedChanged(sender As Object, e As EventArgs) Handles cbxUseDefaultConnectionRetries.CheckedChanged
		nudConnectionRetries.Enabled = Not cbxUseDefaultConnectionRetries.Checked
	End Sub

	Private Sub cbxUseDefaultQueryRetries_CheckedChanged(sender As Object, e As EventArgs) Handles cbxUseDefaultQueryRetries.CheckedChanged
		nudQueryRetries.Enabled = Not cbxUseDefaultQueryRetries.Checked
	End Sub

	Private Sub cbxUseDefaultRetryWait_CheckedChanged(sender As Object, e As EventArgs) Handles cbxUseDefaultRetryWait.CheckedChanged
		nudRetryWait.Enabled = Not cbxUseDefaultRetryWait.Checked
	End Sub

	Private Sub cbxUseDefaultCommandTimeout_CheckedChanged(sender As Object, e As EventArgs) Handles cbxUseDefaultCommandTimeout.CheckedChanged
		nudCommandTimeout.Enabled = Not cbxUseDefaultCommandTimeout.Checked
	End Sub

	Private Sub SetXmlSetting(d As System.Xml.XmlDocument, settingName As String, settingValue As String)
		Dim valueFound As Boolean = False
		Dim i As Integer = 0
		Do While (i < d.DocumentElement.ChildNodes.Count)
			If String.Equals(d.DocumentElement.ChildNodes(i).Attributes(0).InnerText, settingName, StringComparison.OrdinalIgnoreCase) Then
				d.DocumentElement.ChildNodes(i).InnerText = settingValue
				valueFound = True
				Exit Do
			Else
				i += 1
			End If
		Loop
		If Not valueFound Then
			Dim newSettingNode As Xml.XmlElement = d.CreateElement("setting")
			Dim attribute As Xml.XmlAttribute = d.CreateAttribute("name")
			attribute.Value = settingName
			newSettingNode.Attributes.Append(attribute)
			newSettingNode.InnerText = settingValue
			d.DocumentElement.AppendChild(newSettingNode)
		End If
	End Sub

	Private Sub ClearXmlSetting(d As System.Xml.XmlDocument, settingName As String)
		Dim valueFound As Boolean = False
		Dim i As Integer = 0
		Do While (i < d.DocumentElement.ChildNodes.Count)
			If String.Equals(d.DocumentElement.ChildNodes(i).Attributes(0).InnerText, settingName, StringComparison.OrdinalIgnoreCase) Then
				d.DocumentElement.RemoveChild(d.DocumentElement.ChildNodes(i))
				valueFound = True
				Exit Do
			Else
				i += 1
			End If
		Loop
	End Sub

	Private Sub PopulateXmlSettings(d As Xml.XmlDocument, ByVal settingName As String, ByVal settingValue As String)
		Dim i As Integer = 0
		Do While (i < d.DocumentElement.ChildNodes.Count)
			If String.Equals(d.DocumentElement.ChildNodes(i).Attributes(0).InnerText, settingName, StringComparison.OrdinalIgnoreCase) Then Exit Sub
			i += 1
		Loop
		Dim newSettingNode As Xml.XmlElement = d.CreateElement("setting")
		Dim attribute As Xml.XmlAttribute = d.CreateAttribute("name")
		attribute.Value = settingName
		newSettingNode.Attributes.Append(attribute)
		newSettingNode.InnerText = settingValue
		d.DocumentElement.AppendChild(newSettingNode)
	End Sub

	Private Sub SetRegistrySetting(settingName As String, settingValue As String)
		My.Computer.Registry.SetValue("HKEY_LOCAL_MACHINE\" & Tm2Database.REGISTRY_CONFIG_KEY_LOCATION, settingName, settingValue, Microsoft.Win32.RegistryValueKind.String)
		If Environment.Is64BitProcess AndAlso Environment.Is64BitOperatingSystem Then
			My.Computer.Registry.SetValue("HKEY_LOCAL_MACHINE\" & Tm2Database.REGISTRY_CONFIG_KEY_32BIT_LOCATION, settingName, settingValue, Microsoft.Win32.RegistryValueKind.String)
		End If
	End Sub

	Private Sub ClearRegistrySetting(settingName As String)
		Dim key As Microsoft.Win32.RegistryKey = My.Computer.Registry.LocalMachine.OpenSubKey(Tm2Database.REGISTRY_CONFIG_KEY_LOCATION, Microsoft.Win32.RegistryKeyPermissionCheck.ReadWriteSubTree, Security.AccessControl.RegistryRights.FullControl)
		If key IsNot Nothing Then key.DeleteValue(settingName, False)
		key = My.Computer.Registry.LocalMachine.OpenSubKey(Tm2Database.REGISTRY_CONFIG_KEY_32BIT_LOCATION, Microsoft.Win32.RegistryKeyPermissionCheck.ReadWriteSubTree, Security.AccessControl.RegistryRights.FullControl)
		If key IsNot Nothing Then key.DeleteValue(settingName, False)
	End Sub

	Private Sub btnCreateConfigXml_Click(sender As Object, e As EventArgs) Handles btnCreateConfigXml.Click
		Dim d As New System.Xml.XmlDocument()
		Try
			Dim newSettingNode As Xml.XmlElement = d.CreateElement("XML")
			Dim attribute As Xml.XmlAttribute = d.CreateAttribute("id")
			attribute.Value = "config"
			newSettingNode.Attributes.Append(attribute)
			d.AppendChild(newSettingNode)
            Dim result As Boolean = SaveSettings(d)
            If result Then
                If d IsNot Nothing Then d.Save(_configFileName)
                Windows.Forms.MessageBox.Show("File created successfully at " & _configFileName & ".")
            End If
        Catch ex As Exception
			Windows.Forms.MessageBox.Show(ex.ToString())
		End Try
	End Sub

	Private Sub btnSetDefaultConnectionString_Click(sender As Object, e As EventArgs) Handles btnSetDefaultConnectionString.Click
		tbxConnectionString.Text = "Provider=SQLOLEDB;Data Source=localhost;Initial Catalog=Tm2;User Id=sa;Password=Kahler6648;Persist Security Info=True;"
	End Sub

	<Serializable>
	Public Class ConnectionSettings
		Public Sub New()

		End Sub
		Private _dataConnection As String = ""
		Public Property DataConnection() As String
			Get
				Return _dataConnection
			End Get
			Set(ByVal value As String)
				_dataConnection = value
			End Set
		End Property

		Private _UseDefaultConnectionRetries As Boolean = True
		Public Property UseDefaultConnectionRetries() As Boolean
			Get
				Return _UseDefaultConnectionRetries
			End Get
			Set(ByVal value As Boolean)
				_UseDefaultConnectionRetries = value
			End Set
		End Property

		Private _connectionRetries As Integer = 3
		Public Property ConnectionRetries() As String
			Get
				Return _connectionRetries
			End Get
			Set(ByVal value As String)
				_connectionRetries = value
			End Set
		End Property

		Private _useDefaultQueryRetries As Boolean = True
		Public Property UseDefaultQueryRetries() As Boolean
			Get
				Return _useDefaultQueryRetries
			End Get
			Set(ByVal value As Boolean)
				_useDefaultQueryRetries = value
			End Set
		End Property

		Private _queryRetries As Integer = 3
		Public Property QueryRetries() As Integer
			Get
				Return _queryRetries
			End Get
			Set(ByVal value As Integer)
				_queryRetries = value
			End Set
		End Property

		Private _useDefaultRetryWait As Boolean = True
		Public Property UseDefaultRetryWait() As Boolean
			Get
				Return _useDefaultRetryWait
			End Get
			Set(ByVal value As Boolean)
				_useDefaultRetryWait = value
			End Set
		End Property

		Private _retryWait As Integer = 100
		Public Property RetryWait() As Integer
			Get
				Return _retryWait
			End Get
			Set(ByVal value As Integer)
				_retryWait = Math.Max(1, value)
			End Set
		End Property

		Private _useDefaultCommandTimeout As Boolean = True
		Public Property UseDefaultCommandTimeout() As Boolean
			Get
				Return _useDefaultCommandTimeout
			End Get
			Set(ByVal value As Boolean)
				_useDefaultCommandTimeout = value
			End Set
		End Property

		Private _commandTimeout As Integer = 30
		Public Property CommandTimeout() As Integer
			Get
				Return _commandTimeout
			End Get
			Set(ByVal value As Integer)
				_commandTimeout = value
			End Set
		End Property
	End Class
End Class