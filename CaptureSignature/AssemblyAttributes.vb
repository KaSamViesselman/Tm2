Imports System
Namespace AssemblyAttributes
    <Serializable()>
    Public Class AssemblyDevelopmentBranch
        Inherits Attribute
        Private _version As String = ""

        Public Sub New()

        End Sub

        Public Sub New(version As String)
            _version = version
        End Sub

        Public ReadOnly Property DevelopmentBranchVersion As String
            Get
                Return _version.Trim()
            End Get
        End Property
    End Class
End Namespace