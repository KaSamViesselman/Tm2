Imports System.IO
'ToDo: Move to seperate dll for use by TM2 website and Email Server?

Namespace Tm2Reporting.DynamicRDLCGenerator
	Module ReportEngine
		'Private _logoPath As String = HttpContext.Current.Server.MapPath("Images") & "\logo.png"
		Private _logoPath As String = "C:\Projects\DefaultCollection\TerminalManagement2\TerminalManagement2\TerminalManagement2\images\Kahler-logo-standard.png"

		Function GenerateReport(ByVal ds As DataSet, reportTitle As String) As Stream
			Dim fieldHeaderCaptions As Dictionary(Of String, String) = New Dictionary(Of String, String)
			Dim fields As Dictionary(Of String, String) = New Dictionary(Of String, String)
			Return GenerateReport(ds, fieldHeaderCaptions, reportTitle)
		End Function

		''' <summary>
		''' 
		''' </summary>
		''' <param name="ds"></param>
		''' <param name="fieldAliasHeaderCaptions">headerText, alias_tableName.fieldname - value should be a key in the {fields} parameter </param>
		''' <param name="reportTitle"></param>
		''' <returns></returns>
		Function GenerateReport(ByVal ds As DataSet, fieldAliasHeaderCaptions As Dictionary(Of String, String), reportTitle As String) As Stream
			Dim reportBuilder As DynamicRDLCGenerator.ReportBuilderEntities.ReportBuilder = New DynamicRDLCGenerator.ReportBuilderEntities.ReportBuilder()
			reportBuilder.DataSource = ds

			reportBuilder.Page = New DynamicRDLCGenerator.ReportBuilderEntities.ReportPage()
			Dim reportFooter As DynamicRDLCGenerator.ReportBuilderEntities.ReportSections = New DynamicRDLCGenerator.ReportBuilderEntities.ReportSections()
			Dim reportFooterItems As DynamicRDLCGenerator.ReportBuilderEntities.ReportItems = New DynamicRDLCGenerator.ReportBuilderEntities.ReportItems()
			'Dim footerTxt As DynamicRDLCGenerator.ReportBuilderEntities.ReportTextBoxControl() = New DynamicRDLCGenerator.ReportBuilderEntities.ReportTextBoxControl() {
			'		New DynamicRDLCGenerator.ReportBuilderEntities.ReportTextBoxControl() With {.Name = "txtCopyright", .ValueOrExpression = New String() {String.Format("Copyright {0}", DateTime.Now.Year)}},
			'		New DynamicRDLCGenerator.ReportBuilderEntities.ReportTextBoxControl() With {.Name = "ExecutionTime", .ValueOrExpression = New String() {"Report Generated On " + DateTime.Now.ToString()}},
			'		New DynamicRDLCGenerator.ReportBuilderEntities.ReportTextBoxControl() With {.Name = "PageNumber", .ValueOrExpression = New String() {"Page ", DynamicRDLCGenerator.ReportBuilderEntities.ReportGlobalParameters.CurrentPageNumber, " of ", DynamicRDLCGenerator.ReportBuilderEntities.ReportGlobalParameters.TotalPages}}}
			Dim footerTxt As DynamicRDLCGenerator.ReportBuilderEntities.ReportTextBoxControl() = New DynamicRDLCGenerator.ReportBuilderEntities.ReportTextBoxControl() {
				 	New DynamicRDLCGenerator.ReportBuilderEntities.ReportTextBoxControl() With {.Name = "ExecutionTime", .ValueOrExpression = New String() {"Report Generated On " + DateTime.Now.ToString()}},
					New DynamicRDLCGenerator.ReportBuilderEntities.ReportTextBoxControl() With {.Name = "PageNumber", .ValueOrExpression = New String() {"Page ", DynamicRDLCGenerator.ReportBuilderEntities.ReportGlobalParameters.CurrentPageNumber, " of ", DynamicRDLCGenerator.ReportBuilderEntities.ReportGlobalParameters.TotalPages}}}

			reportFooterItems.TextBoxControls = footerTxt
			reportFooter.ReportControlItems = reportFooterItems
			reportBuilder.Page.ReportFooter = reportFooter

			Dim reportHeader As DynamicRDLCGenerator.ReportBuilderEntities.ReportSections = New DynamicRDLCGenerator.ReportBuilderEntities.ReportSections()
			reportHeader.Size = New DynamicRDLCGenerator.ReportBuilderEntities.ReportScale()
			reportHeader.Size.Height = 0.56849

			Dim reportHeaderItems As DynamicRDLCGenerator.ReportBuilderEntities.ReportItems = New DynamicRDLCGenerator.ReportBuilderEntities.ReportItems()
			Dim headerTxt As DynamicRDLCGenerator.ReportBuilderEntities.ReportTextBoxControl() = New DynamicRDLCGenerator.ReportBuilderEntities.ReportTextBoxControl() {
					New DynamicRDLCGenerator.ReportBuilderEntities.ReportTextBoxControl() With {.Name = "txtReportTitle", .ValueOrExpression = New String() {reportTitle}}}

			reportHeaderItems.TextBoxControls = headerTxt
			reportHeader.ReportControlItems = reportHeaderItems
			reportBuilder.Page.ReportHeader = reportHeader

			Return DynamicRDLCGenerator.ReportEngine.GenerateReport(reportBuilder, fieldAliasHeaderCaptions)
		End Function

		Function GenerateReport(ByVal reportBuilder As DynamicRDLCGenerator.ReportBuilderEntities.ReportBuilder, fieldHeaderCaptions As Dictionary(Of String, String)) As Stream
			Dim ret As Stream = New MemoryStream(Encoding.UTF8.GetBytes(GetReportData(reportBuilder, fieldHeaderCaptions)))
			Return ret
		End Function

		Private Function InitAutoGenerateReport(ByVal reportBuilder As DynamicRDLCGenerator.ReportBuilderEntities.ReportBuilder, fieldAliasHeaderCaptions As Dictionary(Of String, String)) As DynamicRDLCGenerator.ReportBuilderEntities.ReportBuilder

			If reportBuilder IsNot Nothing AndAlso reportBuilder.DataSource IsNot Nothing AndAlso reportBuilder.DataSource.Tables.Count > 0 Then
				Dim ds As DataSet = reportBuilder.DataSource
				Dim tablesCount As Integer = ds.Tables.Count
				Dim reportTables As DynamicRDLCGenerator.ReportBuilderEntities.ReportTable() = New DynamicRDLCGenerator.ReportBuilderEntities.ReportTable(tablesCount - 1) {}

				If reportBuilder.AutoGenerateReport Then
					For j As Integer = 0 To tablesCount - 1
						Dim dt As DataTable = ds.Tables(j)
						Dim columns As DynamicRDLCGenerator.ReportBuilderEntities.ReportColumns() = New DynamicRDLCGenerator.ReportBuilderEntities.ReportColumns(dt.Columns.Count - 1) {}
						Dim ColumnScale As DynamicRDLCGenerator.ReportBuilderEntities.ReportScale = New DynamicRDLCGenerator.ReportBuilderEntities.ReportScale()
						ColumnScale.Width = 4
						ColumnScale.Height = 1
						Dim ColumnPadding As DynamicRDLCGenerator.ReportBuilderEntities.ReportDimensions = New DynamicRDLCGenerator.ReportBuilderEntities.ReportDimensions()
						ColumnPadding.[Default] = 2

						For i As Integer = 0 To dt.Columns.Count - 1
							Dim headertext As String = dt.Columns(i).ColumnName
							If fieldAliasHeaderCaptions.ContainsKey(headertext) Then
								headertext = fieldAliasHeaderCaptions(dt.Columns(i).ColumnName)
							End If

							columns(i) = New DynamicRDLCGenerator.ReportBuilderEntities.ReportColumns() With {
								.ColumnCell = New DynamicRDLCGenerator.ReportBuilderEntities.ReportTextBoxControl() With {
									.Name = dt.Columns(i).ColumnName,
									.Size = ColumnScale,
									.Padding = ColumnPadding
								},
								.HeaderText = headertext,
								.HeaderColumnPadding = ColumnPadding
							}
						Next

						reportTables(j) = New DynamicRDLCGenerator.ReportBuilderEntities.ReportTable() With {
							.ReportName = dt.TableName,
							.ReportDataColumns = columns
						}
					Next
				End If

				reportBuilder.Body = New DynamicRDLCGenerator.ReportBuilderEntities.ReportBody()
				reportBuilder.Body.ReportControlItems = New DynamicRDLCGenerator.ReportBuilderEntities.ReportItems()
				reportBuilder.Body.ReportControlItems.ReportTable = reportTables
			End If

			Return reportBuilder
		End Function

		Private Function GetReportData(ByVal reportBuilder As DynamicRDLCGenerator.ReportBuilderEntities.ReportBuilder, fieldAliasHeaderCaptions As Dictionary(Of String, String)) As String
			reportBuilder = InitAutoGenerateReport(reportBuilder, fieldAliasHeaderCaptions)
			Dim rdlcXML As String = ""
			rdlcXML &= "<?xml version=""1.0"" encoding=""utf-8""?> 
	                       <Report xmlns=""http://schemas.microsoft.com/sqlserver/reporting/2008/01/reportdefinition""  
	                       xmlns:rd=""http://schemas.microsoft.com/SQLServer/reporting/reportdesigner""> 
	                     <Body>"
			Dim _tableData As String = GenerateTable(reportBuilder)

			If _tableData.Trim() <> "" Then
				rdlcXML &= "<ReportItems>" & _tableData & "</ReportItems>"
			End If

			rdlcXML &= "<Height>2.1162cm</Height> 
	                       <Style /> 
	                     </Body> 
	                     <Width>20.8cm</Width> 
	                     <Page> 
	                       " & GetPageHeader(reportBuilder) & GetFooter(reportBuilder) & GetReportPageSettings() & " 
	                       <Style /> 
	                     </Page> 
	                     <AutoRefresh>0</AutoRefresh> 
	                       " & GetDataSet(reportBuilder)
			If System.IO.File.Exists(_logoPath) Then
				Dim imgBinary As Byte() = File.ReadAllBytes(_logoPath)
				rdlcXML &= "<EmbeddedImages> 
	                       <EmbeddedImage Name=""Logo""> 
	                         <MIMEType>image/png</MIMEType> 
	                         <ImageData>" & System.Convert.ToBase64String(imgBinary) & "</ImageData> 
	                       </EmbeddedImage> 
	                     </EmbeddedImages> "
			End If
			rdlcXML &= "<Language>" & System.Globalization.CultureInfo.CurrentCulture.Name & "</Language> 
	                     <ConsumeContainerWhitespace>true</ConsumeContainerWhitespace> 
	                     <rd:ReportUnitType>Cm</rd:ReportUnitType> 
	                     <rd:ReportID>" & Guid.NewGuid().ToString() & "</rd:ReportID> 
	                   </Report>"
			Return rdlcXML
		End Function

		Private Function GetReportPageSettings() As String
			Return "<PageHeight>21cm</PageHeight> 
					   <PageWidth>29.5cm</PageWidth> 
					   <LeftMargin>0.1cm</LeftMargin> 
					   <RightMargin>0.1cm</RightMargin> 
					   <TopMargin>0.1cm</TopMargin> 
					   <BottomMargin>0.1cm</BottomMargin> 
					   <ColumnSpacing>1cm</ColumnSpacing>"
		End Function

		Private Function GetPageHeader(ByVal reportBuilder As DynamicRDLCGenerator.ReportBuilderEntities.ReportBuilder) As String
			Dim strHeader As String = ""
			If reportBuilder.Page Is Nothing OrElse reportBuilder.Page.ReportHeader Is Nothing Then Return ""
			Dim reportHeader As DynamicRDLCGenerator.ReportBuilderEntities.ReportSections = reportBuilder.Page.ReportHeader
			strHeader = "<PageHeader> 
	                         <Height>" & reportHeader.Size.Height.ToString() & "in</Height> 
	                         <PrintOnFirstPage>" & reportHeader.PrintOnFirstPage.ToString().ToLower() & "</PrintOnFirstPage> 
	                         <PrintOnLastPage>" & reportHeader.PrintOnLastPage.ToString().ToLower() & "</PrintOnLastPage> 
	                         <ReportItems>"
			Dim headerTxt As DynamicRDLCGenerator.ReportBuilderEntities.ReportTextBoxControl() = reportBuilder.Page.ReportHeader.ReportControlItems.TextBoxControls

			If headerTxt IsNot Nothing Then
				For i As Integer = 0 To headerTxt.Length - 1
					strHeader &= GetTextBox(headerTxt(i).Name, Nothing, headerTxt(i).ValueOrExpression)
				Next
			End If

			If System.IO.File.Exists(_logoPath) Then
				strHeader &= "	<Image Name=""Image1""> 
	                             <Source>Embedded</Source> 
	                             <Value>Logo</Value> 
	                             <Sizing>FitProportional</Sizing> 
	                             <Top>0.05807in</Top> 
	                             <Left>0.06529in</Left> 
	                             <Height>0.4375in</Height> 
	                             <Width>1.36459in</Width> 
	                             <ZIndex>1</ZIndex> 
	                             <Style /> 
	                           </Image> "
			End If
			strHeader &= "</ReportItems> 
	                         <Style /> 
	                       </PageHeader>"
			Return strHeader
		End Function

		Private Function GetFooter(ByVal reportBuilder As DynamicRDLCGenerator.ReportBuilderEntities.ReportBuilder) As String
			Dim strFooter As String = ""
			If reportBuilder.Page Is Nothing OrElse reportBuilder.Page.ReportFooter Is Nothing Then Return ""
			strFooter = "<PageFooter> 
	                         <Height>0.68425in</Height> 
	                         <PrintOnFirstPage>true</PrintOnFirstPage> 
	                         <PrintOnLastPage>true</PrintOnLastPage> 
	                         <ReportItems>"
			Dim footerTxt As DynamicRDLCGenerator.ReportBuilderEntities.ReportTextBoxControl() = reportBuilder.Page.ReportFooter.ReportControlItems.TextBoxControls

			If footerTxt IsNot Nothing Then

				For i As Integer = 0 To footerTxt.Length - 1
					strFooter &= GetTextBox(footerTxt(i).Name, Nothing, footerTxt(i).ValueOrExpression)
				Next
			End If

			strFooter &= "</ReportItems> 
	                         <Style /> 
	                       </PageFooter>"
			Return strFooter
		End Function

		Private Function GetDataSet(ByVal reportBuilder As DynamicRDLCGenerator.ReportBuilderEntities.ReportBuilder) As String
			Dim dataSetStr As String = ""

			If reportBuilder IsNot Nothing AndAlso reportBuilder.DataSource IsNot Nothing AndAlso reportBuilder.DataSource.Tables.Count > 0 Then
				Dim dsName As String = "rptCustomers"
				dataSetStr &= "<DataSources> 
								   <DataSource Name=""" & dsName & """> 
									 <ConnectionProperties> 
									   <DataProvider>System.Data.DataSet</DataProvider> 
									   <ConnectString>/* Local Connection */</ConnectString> 
									 </ConnectionProperties> 
									 <rd:DataSourceID>944b21fd-a128-4363-a5fc-312a032950a0</rd:DataSourceID> 
								   </DataSource> 
								 </DataSources> 
							<DataSets>" & GetDataSetTables(reportBuilder.Body.ReportControlItems.ReportTable, dsName) & "</DataSets>"
			End If

			Return dataSetStr
		End Function

		Private Function GetDataSetTables(ByVal tables As DynamicRDLCGenerator.ReportBuilderEntities.ReportTable(), ByVal DataSourceName As String) As String
			Dim strTables As String = ""

			For i As Integer = 0 To tables.Length - 1
				strTables &= "<DataSet Name=""" & tables(i).ReportName & """> 
								 <Query> 
								   <DataSourceName>" & DataSourceName & "</DataSourceName> 
								   <CommandText>/* Local Query */</CommandText> 
								 </Query> 
								" & GetDataSetFields(tables(i).ReportDataColumns) & " 
							   </DataSet>"
			Next

			Return strTables
		End Function

		Private Function GetDataSetFields(ByVal reportColumns As DynamicRDLCGenerator.ReportBuilderEntities.ReportColumns()) As String
			Dim strFields As String = ""
			strFields &= "<Fields>"

			For i As Integer = 0 To reportColumns.Length - 1
				strFields &= "<Field Name=""" & reportColumns(i).ColumnCell.Name & """> 
						 <DataField>" & reportColumns(i).ColumnCell.Name & "</DataField> 
						 <rd:TypeName>System.String</rd:TypeName> 
					   </Field>"
			Next

			strFields &= "</Fields>"
			Return strFields
		End Function

		Private Function GenerateTable(ByVal reportBuilder As DynamicRDLCGenerator.ReportBuilderEntities.ReportBuilder) As String
			Dim TableStr As String = ""

			If reportBuilder IsNot Nothing AndAlso reportBuilder.DataSource IsNot Nothing AndAlso reportBuilder.DataSource.Tables.Count > 0 Then
				Dim table As DynamicRDLCGenerator.ReportBuilderEntities.ReportTable = New DynamicRDLCGenerator.ReportBuilderEntities.ReportTable()

				For i As Integer = 0 To reportBuilder.Body.ReportControlItems.ReportTable.Length - 1
					table = reportBuilder.Body.ReportControlItems.ReportTable(i)
					TableStr &= "<Tablix Name=""table_" & table.ReportName & """> 
								   <TablixBody> 
									 " & GetTableColumns(reportBuilder, table) & " 
									 <TablixRows> 
									   " & GenerateTableHeaderRow(reportBuilder, table) & GenerateTableRow(reportBuilder, table) & " 
									 </TablixRows> 
								   </TablixBody>" & GetTableColumnHeirarchy(reportBuilder, table) & " 
								   <TablixRowHierarchy> 
									 <TablixMembers> 
									   <TablixMember> 
										 <KeepWithGroup>After</KeepWithGroup> 
									   </TablixMember> 
									   <TablixMember> 
										 <Group Name=""" & table.ReportName & "_Details" & """ /> 
									   </TablixMember> 
									 </TablixMembers> 
								   </TablixRowHierarchy> 
								   <RepeatColumnHeaders>true</RepeatColumnHeaders> 
								   <RepeatRowHeaders>true</RepeatRowHeaders> 
								   <DataSetName>" & table.ReportName & "</DataSetName>" & GetSortingDetails(reportBuilder) & " 
								   <Top>0.07056cm</Top> 
								   <Left>0cm</Left> 
								   <Height>1.2cm</Height> 
								   <Width>7.5cm</Width> 
								   <Style> 
									 <Border> 
									   <Style>None</Style> 
									 </Border> 
								   </Style> 
								 </Tablix>"
				Next
			End If

			Return TableStr
		End Function

		Private Function GetSortingDetails(ByVal reportBuilder As DynamicRDLCGenerator.ReportBuilderEntities.ReportBuilder) As String
			Return ""
			Dim tables As DynamicRDLCGenerator.ReportBuilderEntities.ReportTable() = reportBuilder.Body.ReportControlItems.ReportTable
			Dim columns As DynamicRDLCGenerator.ReportBuilderEntities.ReportColumns() = reportBuilder.Body.ReportControlItems.ReportTable(0).ReportDataColumns
			Dim sortColumn As DynamicRDLCGenerator.ReportBuilderEntities.ReportTextBoxControl = New DynamicRDLCGenerator.ReportBuilderEntities.ReportTextBoxControl()
			If columns Is Nothing Then Return ""
			Dim strSorting As String = ""
			strSorting = " <SortExpressions>"

			For i As Integer = 0 To columns.Length - 1
				sortColumn = columns(i).ColumnCell
				strSorting &= "<SortExpression><Value>=Fields!" & sortColumn.Name & ".Value</Value>"
				If columns(i).SortDirection = DynamicRDLCGenerator.ReportBuilderEntities.ReportSort.Descending Then strSorting &= "<Direction>Descending</Direction>"
				strSorting &= "</SortExpression>"
			Next

			strSorting &= "</SortExpressions>"
			Return strSorting
		End Function

		Private Function GenerateTableRow(ByVal reportBuilder As DynamicRDLCGenerator.ReportBuilderEntities.ReportBuilder, ByVal table As DynamicRDLCGenerator.ReportBuilderEntities.ReportTable) As String
			Dim columns As DynamicRDLCGenerator.ReportBuilderEntities.ReportColumns() = table.ReportDataColumns
			Dim ColumnCell As DynamicRDLCGenerator.ReportBuilderEntities.ReportTextBoxControl = New DynamicRDLCGenerator.ReportBuilderEntities.ReportTextBoxControl()
			Dim colHeight As DynamicRDLCGenerator.ReportBuilderEntities.ReportScale = ColumnCell.Size
			Dim padding As DynamicRDLCGenerator.ReportBuilderEntities.ReportDimensions = New DynamicRDLCGenerator.ReportBuilderEntities.ReportDimensions()
			If columns Is Nothing Then Return ""
			Dim strTableRow As String = ""
			strTableRow = "<TablixRow> 
							   <Height>0.6cm</Height> 
							   <TablixCells>"

			For i As Integer = 0 To columns.Length - 1
				ColumnCell = columns(i).ColumnCell
				padding = ColumnCell.Padding
				strTableRow &= "<TablixCell> 
									 <CellContents> 
									  " & GenerateTextBox("txtCell_" & table.ReportName & "_", ColumnCell.Name, "", True, padding) & " 
									 </CellContents> 
								   </TablixCell>"
			Next

			strTableRow &= "</TablixCells></TablixRow>"
			Return strTableRow
		End Function

		Private Function GenerateTableHeaderRow(ByVal reportBuilder As DynamicRDLCGenerator.ReportBuilderEntities.ReportBuilder, ByVal table As DynamicRDLCGenerator.ReportBuilderEntities.ReportTable) As String
			Dim columns As DynamicRDLCGenerator.ReportBuilderEntities.ReportColumns() = table.ReportDataColumns
			Dim ColumnCell As DynamicRDLCGenerator.ReportBuilderEntities.ReportTextBoxControl = New DynamicRDLCGenerator.ReportBuilderEntities.ReportTextBoxControl()
			Dim padding As DynamicRDLCGenerator.ReportBuilderEntities.ReportDimensions = New DynamicRDLCGenerator.ReportBuilderEntities.ReportDimensions()
			If columns Is Nothing Then Return ""
			Dim strTableRow As String = ""
			strTableRow = "<TablixRow> 
							   <Height>0.6cm</Height> 
							   <TablixCells>"

			For i As Integer = 0 To columns.Length - 1
				ColumnCell = columns(i).ColumnCell
				padding = columns(i).HeaderColumnPadding
				strTableRow &= "<TablixCell> 
								 <CellContents> 
								  " & GenerateTextBox("txtHeader_" & table.ReportName & "_", ColumnCell.Name, If(columns(i).HeaderText Is Nothing OrElse columns(i).HeaderText.Trim() = "", ColumnCell.Name, columns(i).HeaderText), False, padding) & " 
								 </CellContents> 
							   </TablixCell>"
			Next

			strTableRow &= "</TablixCells></TablixRow>"
			Return strTableRow
		End Function

		Private Function GetTableColumns(ByVal reportBuilder As DynamicRDLCGenerator.ReportBuilderEntities.ReportBuilder, ByVal table As DynamicRDLCGenerator.ReportBuilderEntities.ReportTable) As String
			Dim columns As DynamicRDLCGenerator.ReportBuilderEntities.ReportColumns() = table.ReportDataColumns
			Dim ColumnCell As DynamicRDLCGenerator.ReportBuilderEntities.ReportTextBoxControl = New DynamicRDLCGenerator.ReportBuilderEntities.ReportTextBoxControl()
			If columns Is Nothing Then Return ""
			Dim strColumnHeirarchy As String = ""
			strColumnHeirarchy = " 
	           <TablixColumns>"

			For i As Integer = 0 To columns.Length - 1
				ColumnCell = columns(i).ColumnCell
				strColumnHeirarchy &= " <TablixColumn> 
	                                         <Width>" & ColumnCell.Size.Width.ToString() & "cm</Width>  
	                                       </TablixColumn>"
			Next

			strColumnHeirarchy &= "</TablixColumns>"
			Return strColumnHeirarchy
		End Function

		Private Function GetTableColumnHeirarchy(ByVal reportBuilder As DynamicRDLCGenerator.ReportBuilderEntities.ReportBuilder, ByVal table As DynamicRDLCGenerator.ReportBuilderEntities.ReportTable) As String
			Dim columns As DynamicRDLCGenerator.ReportBuilderEntities.ReportColumns() = table.ReportDataColumns
			If columns Is Nothing Then Return ""
			Dim strColumnHeirarchy As String = ""
			strColumnHeirarchy = " 
	           <TablixColumnHierarchy> 
	         <TablixMembers>"

			For i As Integer = 0 To columns.Length - 1
				strColumnHeirarchy &= "<TablixMember />"
			Next

			strColumnHeirarchy &= "</TablixMembers> 
	       </TablixColumnHierarchy>"
			Return strColumnHeirarchy
		End Function

		Private Function GenerateTextBox(ByVal strControlIDPrefix As String, ByVal strName As String, ByVal Optional strValueOrExpression As String = "", ByVal Optional isFieldValue As Boolean = True, ByVal Optional padding As DynamicRDLCGenerator.ReportBuilderEntities.ReportDimensions = Nothing) As String
			Dim strTextBox As String = ""
			strTextBox = " <Textbox Name=""" & strControlIDPrefix & strName & """> 
							 <CanGrow>true</CanGrow> 
							 <KeepTogether>true</KeepTogether> 
							 <Paragraphs> 
							   <Paragraph> 
								 <TextRuns> 
								   <TextRun>"

			If isFieldValue Then
				strTextBox &= "<Value>=Fields!" & strName & ".Value</Value>"
			Else
				strTextBox &= "<Value>" & strValueOrExpression & "</Value>"
			End If

			strTextBox &= "<Style /> 
	                           </TextRun> 
	                         </TextRuns> 
	                         <Style /> 
	                       </Paragraph> 
	                     </Paragraphs> 
	                     <rd:DefaultName>" & strControlIDPrefix & strName & "</rd:DefaultName> 
	                     <Style> 
	                       <Border> 
	                         <Color>LightGrey</Color> 
	                         <Style>Solid</Style> 
	                       </Border>" & GetDimensions(padding) & "</Style> 
	                   </Textbox>"
			Return strTextBox
		End Function

		Private Function GetTextBox(ByVal textBoxName As String, ByVal padding As DynamicRDLCGenerator.ReportBuilderEntities.ReportDimensions, ParamArray strValues As String()) As String
			Dim strTextBox As String = ""
			strTextBox = " <Textbox Name=""" & textBoxName & """> 
							 <CanGrow>true</CanGrow> 
							 <KeepTogether>true</KeepTogether> 
							 <Paragraphs> 
							   <Paragraph> 
								 <TextRuns>"

			For i As Integer = 0 To strValues.Length - 1
				strTextBox &= GetTextRun(strValues(i).ToString())
			Next

			strTextBox &= "</TextRuns> 
							 <Style /> 
						   </Paragraph> 
						 </Paragraphs> 
						 <rd:DefaultName>" & textBoxName & "</rd:DefaultName> 
						 <Top>1.0884cm</Top> 
						 <Left>0cm</Left> 
						 <Height>0.6cm</Height> 
						 <Width>7.93812cm</Width> 
						 <ZIndex>2</ZIndex> 
						 <Style> 
						   <Border> 
							 <Style>None</Style> 
						   </Border>"
			strTextBox &= GetDimensions(padding) & "</Style> 
	       </Textbox>"
			Return strTextBox
		End Function

		Private Function GetTextRun(ByVal ValueOrExpression As String) As String
			Return "<TextRun> 
	                 <Value>" & ValueOrExpression & "</Value> 
	                 <Style> 
	                   <FontSize>8pt</FontSize> 
	                 </Style> 
	               </TextRun>"
		End Function

		Private Sub GenerateReportImage(ByVal reportBuilder As DynamicRDLCGenerator.ReportBuilderEntities.ReportBuilder)
		End Sub

		Private Function GetDimensions(ByVal Optional padding As DynamicRDLCGenerator.ReportBuilderEntities.ReportDimensions = Nothing) As String
			Dim strDimensions As String = ""

			If padding IsNot Nothing Then

				If padding.[Default] = 0 Then
					strDimensions &= String.Format("<PaddingLeft>{0}pt</PaddingLeft>", padding.Left)
					strDimensions &= String.Format("<PaddingRight>{0}pt</PaddingRight>", padding.Right)
					strDimensions &= String.Format("<PaddingTop>{0}pt</PaddingTop>", padding.Top)
					strDimensions &= String.Format("<PaddingBottom>{0}pt</PaddingBottom>", padding.Bottom)
				Else
					strDimensions &= String.Format("<PaddingLeft>{0}pt</PaddingLeft>", padding.[Default])
					strDimensions &= String.Format("<PaddingRight>{0}pt</PaddingRight>", padding.[Default])
					strDimensions &= String.Format("<PaddingTop>{0}pt</PaddingTop>", padding.[Default])
					strDimensions &= String.Format("<PaddingBottom>{0}pt</PaddingBottom>", padding.[Default])
				End If
			End If

			Return strDimensions
		End Function
	End Module
End Namespace
