Imports System.CodeDom
Imports System.CodeDom.Compiler
Imports System.Security.Permissions
Imports System.Web.Services.Description
Imports System.Reflection

Public Class DynamicWebService

	Public Function CallWebService(ByVal webServiceAsmxUrl As String, ByVal serviceName As String, ByVal methodName As String, ByVal args() As Object) As Object
		Return CallWebService(webServiceAsmxUrl, serviceName, methodName, args, DateTime.MinValue, DateTime.MinValue)
	End Function

	Public Function CallWebService(ByVal webServiceAsmxUrl As String, ByVal serviceName As String, ByVal methodName As String, ByVal args() As Object, ByVal fromDate As DateTime, ByVal toDate As DateTime) As Object
		Try
			Dim client As System.Net.WebClient = New System.Net.WebClient()
			If fromDate > New DateTime(1900, 1, 1) Then client.QueryString.Add("FromDate", fromDate)
			If toDate > New DateTime(1900, 1, 1) Then client.QueryString.Add("ToDate", toDate)

			'-Connect To the web service
			Dim stream As System.IO.Stream =
				client.OpenRead(webServiceAsmxUrl + "?wsdl")

			'Read the WSDL file describing a service.
			Dim description As ServiceDescription = ServiceDescription.Read(stream)

			'LOAD THE DOM'''''''''''''''''''''''''''

			'--Initialize a service description importer.
			Dim importer As ServiceDescriptionImporter = New ServiceDescriptionImporter()
			importer.ProtocolName = "Soap12" ' Use SOAP 1.2.
			importer.AddServiceDescription(description, Nothing, Nothing)

			'--Generate a proxy client. 

			importer.Style = ServiceDescriptionImportStyle.Client
			'--Generate properties to represent primitive values.
			importer.CodeGenerationOptions =
				 System.Xml.Serialization.CodeGenerationOptions.GenerateProperties

			'Initialize a Code-DOM tree into which we will import the service.
			Dim nmspace As CodeNamespace = New CodeNamespace()
			Dim unit1 As CodeCompileUnit = New CodeCompileUnit()
			unit1.Namespaces.Add(nmspace)

			'Import the service into the Code-DOM tree. 
			'This creates proxy code that uses the service.

			Dim warning As ServiceDescriptionImportWarnings = importer.Import(nmspace, unit1)

			If warning = 0 Then
				'--Generate the proxy code
				Dim provider1 As CodeDomProvider = CodeDomProvider.CreateProvider("VB")
				'--Compile the assembly proxy with the // appropriate references
				Dim assemblyReferences() As String
				assemblyReferences = New String() {"System.dll", "System.Web.Services.dll", "System.Web.dll", "System.Xml.dll", "System.Data.dll"}
				Dim parms As CompilerParameters = New CompilerParameters(assemblyReferences)
				parms.GenerateInMemory = True '(Thanks for this line nikolas)
				Dim results As CompilerResults = provider1.CompileAssemblyFromDom(parms, unit1)

				'-Check For Errors
				If results.Errors.Count > 0 Then
					Dim oops As CompilerError
					For Each oops In results.Errors
						System.Diagnostics.Debug.WriteLine("========Compiler error============")
						System.Diagnostics.Debug.WriteLine(oops.ErrorText)
					Next
					Throw New System.Exception("Compile Error Occurred calling webservice.")
				End If

				'--Finally, Invoke the web service method
				Dim wsvcClass As Object = results.CompiledAssembly.CreateInstance(serviceName)
				Dim mi As MethodInfo = wsvcClass.GetType().GetMethod(methodName)
				Return mi.Invoke(wsvcClass, args)
			Else
				Return Nothing
			End If
		Catch ex As Exception
			Throw ex
		End Try
	End Function
End Class
