<Serializable()> _
Public Class Tm2Page
    Public Sub New()
        _name = ""
        _url = ""
        _tabs = New List(Of Tm2Page)
    End Sub

    Public Sub New(name As String, url As String)
        _name = name
        _url = url
        _tabs = New List(Of Tm2Page)
    End Sub

    Public Sub New(name As String, url As String, tabs As List(Of Tm2Page))
        _name = name
        _url = url
        _tabs = tabs
    End Sub

    Public Sub New(name As String, url As String, tabs As List(Of Tm2Page), openInNewWindow As Boolean)
        _name = name
        _url = url
        _tabs = tabs
        _openInNewWindow = openInNewWindow
    End Sub

    Private _name As String
    Public Property Name As String
        Get
            Return _name
        End Get
        Set(value As String)
            _name = value
        End Set
    End Property

    Private _url As String
    Public Property Url As String
        Get
            Return _url
        End Get
        Set(value As String)
            _url = value
        End Set
    End Property

    Private _tabs As List(Of Tm2Page)
    Public Property Tabs As List(Of Tm2Page)
        Get
            Return _tabs
        End Get
        Set(value As List(Of Tm2Page))
            _tabs = value
        End Set
    End Property

    Private _openInNewWindow As Boolean = False
    Public Property OpenInNewWindow() As Boolean
        Get
            Return _openInNewWindow
        End Get
        Set(ByVal value As Boolean)
            _openInNewWindow = value
        End Set
    End Property
End Class
