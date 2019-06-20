Imports System.Drawing

Namespace Tm2Reporting.DynamicRDLCGenerator
	Public Class ReportBuilderEntities
		Public Class ReportGlobalParameters
			Public Shared CurrentPageNumber As String = "=Globals!PageNumber"
			Public Shared TotalPages As String = "=Globals!OverallTotalPages"
		End Class

		Public Class ReportBuilder
			Public Property Page As ReportPage
			Public Property Body As ReportBody
			Public Property DataSource As DataSet
			Private _autoGenerateReport As Boolean = True

			Public Property AutoGenerateReport As Boolean
				Get
					Return _autoGenerateReport
				End Get
				Set(ByVal value As Boolean)
					_autoGenerateReport = value
				End Set
			End Property
		End Class

		Public Class ReportItems
			Public Property TextBoxControls As ReportTextBoxControl()
			Public Property ReportTable As ReportTable()
			Public Property ReportImages As ReportImage()
		End Class

		Public Class ReportTable
			Public Property ReportName As String
			Public Property ReportDataColumns As ReportColumns()
		End Class

		Public Class ReportColumns
			Public Property isGroupedColumn As Boolean
			Public Property HeaderText As String
			Public Property SortDirection As ReportSort
			Public Property Aggregate As ReportFunctions
			Public Property ColumnCell As ReportTextBoxControl
			Public Property HeaderColumnPadding As ReportDimensions
		End Class

		Public Class ReportTextBoxControl
			Public Property Name As String
			Public Property ValueOrExpression As String()
			Public Property Action As ReportActions
			Public Property Padding As ReportDimensions
			Public Property SpaceAfter As Integer
			Public Property SpaceBefore As Integer
			Private _textAlign As ReportHorizantalAlign = ReportHorizantalAlign.[Default]

			Public Property TextAlign As ReportHorizantalAlign
				Get
					Return _textAlign
				End Get
				Set(ByVal value As ReportHorizantalAlign)
					_textAlign = value
				End Set
			End Property

			Private _verticalAlign As ReportHorizantalAlign = ReportHorizantalAlign.[Default]

			Public Property VerticalAlign As ReportHorizantalAlign
				Get
					Return _verticalAlign
				End Get
				Set(ByVal value As ReportHorizantalAlign)
					_verticalAlign = value
				End Set
			End Property

			Public Property BorderStyle As ReportStyles
			Public Property BorderColor As ReportColor
			Public Property BorderWidth As ReportScale
			Public Property BackgroundColor As Color
			Public Property BackgroundImage As ReportImage
			Public Property TextFont As Font
			Public Property LineHeight As Double
			Public Property CanGrow As Boolean
			Public Property CanShrink As Boolean
			Public Property ToolTip As Boolean
			Public Property Position As ReportDimensions
			Public Property Size As ReportScale
			Public Property Visible As Boolean
		End Class

		Public Class ReportBody
			Public Property ReportBodySection As ReportSections
			Public Property ReportControlItems As ReportItems
		End Class

		Public Class ReportPage
			Public Property AutoRefresh As Boolean
			Public Property BackgroundColor As Color
			Public Property BackgroundImage As ReportImage
			Public Property BorderColor As ReportColor
			Public Property BorderWidth As ReportScale
			Public Property Columns As ReportColumnSettings
			Public Property InteractiveSize As ReportScale
			Public Property Margins As ReportDimensions
			Public Property PageSize As ReportScale
			Public Property ReportHeader As ReportSections
			Public Property ReportFooter As ReportSections
		End Class

		Public Class ReportSections
			Public Property BorderStyle As ReportStyles
			Public Property BorderColor As ReportColor
			Public Property BorderWidth As ReportScale
			Public Property BackgroundColor As Color
			Public Property BackgroundImage As ReportImage
			Public Property Size As ReportScale
			Private _printOnFirstPage As Boolean = True

			Public Property PrintOnFirstPage As Boolean
				Get
					Return _printOnFirstPage
				End Get
				Set(ByVal value As Boolean)
					_printOnFirstPage = value
				End Set
			End Property

			Private _printOnLastpage As Boolean = True

			Public Property PrintOnLastPage As Boolean
				Get
					Return _printOnLastpage
				End Get
				Set(ByVal value As Boolean)
					_printOnLastpage = value
				End Set
			End Property

			Public Property ReportControlItems As ReportItems
		End Class

		Public Class ReportColumnSettings
			Public Property Columns As Integer
			Public Property ColumnsSpacing As Integer
		End Class

		Public Class ReportActions
			Public Property ActionType As ReportActionType
			Public Property ValueOrExpression As String
		End Class

		Public Class ReportDimensions
			Public Property Left As Double
			Public Property Right As Double
			Public Property Top As Double
			Public Property Bottom As Double
			Private _default As Double = 2

			Public Property [Default] As Double
				Get
					Return _default
				End Get
				Set(ByVal value As Double)
					_default = value
				End Set
			End Property
		End Class

		Public Class ReportIndent
			Public Property HangingIndent As Double
			Public Property LeftIndent As Double
			Public Property RightIndent As Double
		End Class

		Public Class ReportScale
			Public Property Height As Double
			Public Property Width As Double
		End Class

		Public Class ReportImage
			Public Property ImagePath As ReportImageSource
			Public Property ValueOrExpression As String
			Public Property MIMEType As ReportImageMIMEType
			Public Property Border As ReportStyles
			Public Property Color As ReportColor
			Public Property Position As ReportDimensions
			Public Property Size As ReportScale
			Public Property Padding As ReportDimensions
			Private _reportImageScaling As ReportImageScaling = ReportImageScaling.AutoSize

			Public Property ReportImageScaling As ReportImageScaling
				Get
					Return _reportImageScaling
				End Get
				Set(ByVal value As ReportImageScaling)
					_reportImageScaling = value
				End Set
			End Property
		End Class

		Public Class ReportColor
			Public Property [Default] As Color
			Public Property Left As Color
			Public Property Right As Color
			Public Property Top As Color
			Public Property Bottom As Color
		End Class

		Public Class ReportStyles
			Public Property [Default] As ReportStyle
			Public Property Left As ReportStyle
			Public Property Right As ReportStyle
			Public Property Top As ReportStyle
			Public Property Bottom As ReportStyle
		End Class

		Public Enum ReportActionType
			None
			HyperLink
		End Enum

		Public Enum ReportHorizantalAlign
			Left
			Right
			Center
			General
			[Default]
		End Enum

		Public Enum ReportVerticalAlign
			Top
			Middle
			Bottom
			[Default]
		End Enum

		Public Enum ReportImageRepeat
			[Default]
			Repeat
			RepeatX
			RepeatY
			Clip
		End Enum

		Public Enum ReportImageScaling
			AutoSize
			Flip
			FlipProportional
			Clip
		End Enum

		Public Enum ReportImageSource
			External
			Embedded
			Database
		End Enum

		Public Enum ReportImageMIMEType
			Bitmap
			JPEG
			GIF
			PNG
			xPNG
		End Enum

		Public Enum ReportStyle
			[Default]
			Dashed
			Dotted
			[Double]
			Solid
			None
		End Enum

		Public Enum ReportSort
			Ascending
			Descending
		End Enum

		Public Enum ReportFunctions
			Avg
			Count
			Sum
			Min
			Max
			Aggregate
		End Enum
	End Class
End Namespace