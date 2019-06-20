Imports KahlerAutomation.KaTm2Database
Imports System.Net.Dns
Public Class Configuration
#Region "Events"
	Private Sub Configuration_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
		Dim db As New Tm2Database("")
		db.CheckDatabase(Tm2Database.Connection)
		Dim branch As String = ""
		Try
			Dim attributes As Object() = System.Reflection.Assembly.GetEntryAssembly().GetCustomAttributes(GetType(AssemblyAttributes.AssemblyDevelopmentBranch), False)
			If (attributes.Length > 0) Then branch = CType(attributes(0), AssemblyAttributes.AssemblyDevelopmentBranch).DevelopmentBranchVersion.Trim()
		Catch ex As Exception
			' suppress
		End Try

		lblVersion.Text = Tm2Database.FormatVersion(My.Application.Info.Version, "X") & IIf(branch.Length = 0, "", $" Branch: {branch}")
		lblError.MaximumSize = New Size(tpEmail.Width, 60)
		PopulateLocationList()
		PopulateEmailDetails()
		btnApply.Enabled = False
	End Sub

	Private Sub btnApply_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnApply.Click
		btnApply.Enabled = False
		Save()
	End Sub

	Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCancel.Click
		Close()
	End Sub

	Private Sub btnOk_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOk.Click
		Save()
		Close()
	End Sub

	Private Sub clbLocations_ItemCheck(ByVal sender As Object, ByVal e As System.Windows.Forms.ItemCheckEventArgs) Handles clbLocations.ItemCheck
		btnApply.Enabled = True
	End Sub
	Private Sub tbxEmail_TextChanged(sender As Object, e As EventArgs) Handles tbxEmail.TextChanged, tbxResendInterval.TextChanged
		btnApply.Enabled = True
	End Sub
#End Region

	Private Sub PopulateLocationList()
		For Each r As KaLocation In KaLocation.GetAll(Tm2Database.Connection, "deleted=0", "name ASC")
			clbLocations.Items.Add(r, IsLocationEnabled(r))
		Next
	End Sub

	Private Sub PopulateEmailDetails()
		tbxEmail.Text = KaSetting.GetSetting(Tm2Database.Connection, "@" & GetHostName() & "/Tm2TankStatusReader/EmailAddress", "")
		tbxResendInterval.Text = KaSetting.GetSetting(Tm2Database.Connection, "@" & GetHostName() & "/Tm2TankStatusReader/ResendInterval", "12")
	End Sub

	Private Function IsLocationEnabled(ByVal location As KaLocation) As Boolean
		Return Boolean.Parse(KaSetting.GetSetting(Tm2Database.Connection, "@" & GetHostName() & "/Tm2TankStatusReader/LocationEnabled/" & location.Id.ToString(), "False"))
	End Function

	Private Sub Save()
		If ValidateEntries() Then
			Dim i As Integer = 0
			Do While i < clbLocations.Items.Count
				KaSetting.WriteSetting(Tm2Database.Connection, "@" & GetHostName() & "/Tm2TankStatusReader/LocationEnabled/" & clbLocations.Items(i).Id.ToString(), clbLocations.GetItemChecked(i))
				i += 1
			Loop
			KaSetting.WriteSetting(Tm2Database.Connection, "@" & GetHostName() & "/Tm2TankStatusReader/EmailAddress", tbxEmail.Text)
			KaSetting.WriteSetting(Tm2Database.Connection, "@" & GetHostName() & "/Tm2TankStatusReader/ResendInterval", tbxResendInterval.Text)
		Else
			btnApply.Enabled = True
		End If
	End Sub

	Private Function ValidateEntries() As Boolean
		lblError.Text = ""
		If Not Double.TryParse(tbxResendInterval.Text, New Double) Then
			lblError.Text = tbxResendInterval.Text & " is not a valid interval. "
			Return False
		End If
		Dim badEmailMessage As String = ""
		If Not IsEmailFieldValid(tbxEmail.Text, badEmailMessage) Then
			lblError.Text += badEmailMessage
			Return False
		End If
		Return True
	End Function

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

	Private Sub lblError_TextChanged(sender As Object, e As EventArgs) Handles lblError.TextChanged
		lblError.MaximumSize = New Size(tpEmail.Width, 60)
	End Sub
End Class
