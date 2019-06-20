Dim installer
Dim database
Dim view
Dim msiPath
If WScript.Arguments.Count <> 1 Then
    WScript.Echo "Usage: cscript fixRemovePreviousVersions.vbs <path to MSI>"
    WScript.Quit -1
End If
msiPath = WScript.Arguments(0)
Set installer = CreateObject("WindowsInstaller.Installer")
Set database = installer.OpenDatabase(msiPath, 1)
Set view = database.OpenView("UPDATE InstallExecuteSequence SET Sequence=1450 WHERE Action='RemoveExistingProducts'")
view.Execute
database.Commit
WScript.Quit 0