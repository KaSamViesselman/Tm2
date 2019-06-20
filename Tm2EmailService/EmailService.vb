Imports System.Data.OleDb
Imports System.IO
Imports System.Net
Imports System.Net.Mail
Imports System.Net.Mime
Imports System.Text
Imports System.Web.UI
Imports System.Web.UI.HtmlControls
Imports System.Xml
Imports KaCommonObjects
Imports System.Windows.Forms
Imports KahlerAutomation.KaTm2Database
Imports KahlerAutomation.TerminalManagement2

Public Class EmailService
	Private Alerts As Alerts = New Alerts
	Private Const APPLICATION_ID As String = "Tm2EmailService"
	Private _applicationIdentifier As String = ""
	Private Const SITE_CSS_FILENAME As String = "site.css"
	Private Const STYLE_CSS_FILENAME As String = "style.css"
	Private _thisAssembly = System.Reflection.Assembly.GetAssembly(GetType(Tm2EmailService.DynamicWebService))
	Private _logger As Common.Logger = New Common.Logger(KaSetting.SN_EMAIL_SERVICE_LOG_PATH, KaSetting.SD_EMAIL_SERVICE_LOG_PATH, APPLICATION_ID, _thisAssembly)

	Private Sub EmailService2_Load(sender As Object, e As System.EventArgs) Handles Me.Load
		Me.WindowState = FormWindowState.Minimized
		Me.ShowInTaskbar = False
		_applicationIdentifier = Dns.GetHostName & "/" & APPLICATION_ID

		Dim dbConnection As String = ""
		Dim reportListEndTimes As New Dictionary(Of KaEmailReport, DateTime)
		Dim excludeReportList As New List(Of Guid)
		Dim reportList As New List(Of Guid)
		Dim allReportsSelected As Boolean = False
		Dim emailTickets As Boolean = True
		Dim emailOrderSummaries As Boolean = True
		Dim emailReceivingTickets As Boolean = True

		Dim args As Collections.ObjectModel.ReadOnlyCollection(Of String) = My.Application.CommandLineArgs

		Dim i As Integer = 0
		Do While i < args.Count
			Select Case args(i).ToLower.Trim
				Case "-h"
					_logger.Write("Usage: Tm2EmailService [-a [-x ""report1_guid, report2_guid, ...""]] [-r ""report1_guid, report2_guid, ...""] [-d ""database connection string""] [-noticket] [-noordersummary] [-noreceivingticket]" & vbCrLf &
								   "     -a run all reports (overrides r switch)" & vbCrLf &
								   "     -x exclude listed reports (used with -a)" & vbCrLf &
								   "     -r run listed reports" & vbCrLf &
								   "     -d use specified database connection string" & vbCrLf &
								   "     -noticket do not create e-mails for tickets" & vbCrLf &
								   "     -noordersummary do not create e-mails for order summaries" & vbCrLf &
								   "     -noreceivingticket do not create e-mails for tickets", Common.Logger.LogFileType.Alarm)
					Environment.Exit(-1)
				Case "-d"
					Dim dbConnectionIndex As Integer = i + 1
					If dbConnectionIndex < args.Count Then
						dbConnection = args(dbConnectionIndex)
						i += 1 ' Move an extra index position to skip the dbconnection
					Else
						_logger.Write(String.Format("Error: Database connection not supplied"), Common.Logger.LogFileType.Alarm)

						Environment.Exit(-1)
					End If
				Case "-a"
					allReportsSelected = True
				Case "-r"
					Dim reportListIndex As Integer = i + 1
					If reportListIndex < args.Count Then
						For Each reportId As String In args(reportListIndex).Split(",")
							If reportId.Trim().Length > 0 Then
								Try ' to parse the report ID from the list
									reportList.Add(Guid.Parse(reportId))
									i += 1
								Catch ex2 As FormatException ' the ID was malformed
									_logger.Write(String.Format("Error: {0} is not a properly formatted report ID", reportId), Common.Logger.LogFileType.Alarm)
									CreateEventLogEntry(KaEventLog.Categories.Failure, String.Format("Error: {0} is not a properly formatted report ID", reportId), Tm2Database.Connection)
									Environment.Exit(-1)
								End Try
							End If
						Next
					End If
				Case "-x"
					Dim reportListIndex As Integer = i + 1
					If reportListIndex < args.Count Then
						For Each reportId As String In args(reportListIndex).Split(",")
							If reportId.Trim().Length > 0 Then
								Try ' to parse the report ID from the list
									excludeReportList.Add(Guid.Parse(reportId))
									i += 1
								Catch ex2 As FormatException ' the ID was malformed
									_logger.Write(String.Format("Error: {0} is not a properly formatted report ID", reportId), Common.Logger.LogFileType.Alarm)

									CreateEventLogEntry(KaEventLog.Categories.Failure, String.Format("Error: {0} is not a properly formatted report ID", reportId), Tm2Database.Connection)
									Environment.Exit(-1)
								End Try
							End If
						Next
					End If
				Case "-noticket"
					emailTickets = False
				Case "-noordersummary"
					emailOrderSummaries = False
				Case "-noreceivingticket"
					emailReceivingTickets = False
			End Select
			i += 1
		Loop

		LogInterfaceRun()

		If dbConnection.Length = 0 Then dbConnection = Tm2Database.GetDbConnection()
		_logger.Write("Database connection: " & dbConnection, Common.Logger.LogFileType.Processing)
		Dim connection As New OleDbConnection(dbConnection)
		Try
			Common.SetRunning()

			If (Common.CheckInstalling()) Then
				_logger.Write("Email Service cannot run because an update Is installing", Common.Logger.LogFileType.Alarm)
				Environment.Exit(-1)
			End If

			connection.Open()
			Console.Write("Checking database...")
			Dim db As New Tm2Database(Tm2Database.GetDbConnection())
			db.CheckDatabase(connection, True, False)
			_logger.LoadPath(KaSetting.SD_EMAIL_SERVICE_LOG_PATH)

			If Not CheckActivation() Then Environment.Exit(-1)

			Dim applicationVersion = KaCommonObjects.Assembly.GetAppVersion(_thisAssembly)
			Tm2Database.SetApplicationVersion(Tm2Database.Connection, Nothing, My.Computer.Name, My.Application.Info.Title, applicationVersion, Common.Tm2Authorization.ProductCode, KaCommonObjects.Assembly.GetGuid(_thisAssembly), "")

			' get configuration
			Dim smtpUrl As String = KaSetting.GetSetting(connection, KaSetting.SN_SMTP_URL, KaSetting.SD_SMTP_URL)
			_logger.Write("SMTP URL: " & smtpUrl, Common.Logger.LogFileType.Processing)
			Dim smtpPort As Integer = Integer.Parse(KaSetting.GetSetting(connection, KaSetting.SN_SMTP_PORT, KaSetting.SD_SMTP_PORT))
			_logger.Write(String.Format("SMTP TCP Port: {0:0}", smtpPort), Common.Logger.LogFileType.Processing)
			Dim senderAddress As String = KaSetting.GetSetting(connection, KaSetting.SN_EMAIL_ADDRESS, KaSetting.SD_EMAIL_ADDRESS)
			_logger.Write("E-mail address: " & senderAddress, Common.Logger.LogFileType.Processing)
			Dim client As New SmtpClient(smtpUrl, smtpPort)
			Dim useSSL As Boolean = Boolean.Parse(KaSetting.GetSetting(connection, KaSetting.SN_EMAIL_SERVER_USE_SSL, KaSetting.SD_EMAIL_SERVER_USE_SSL))
			If useSSL Then _logger.Write("Use SSL", Common.Logger.LogFileType.Processing)
			client.EnableSsl = useSSL
			Dim username As String = KaSetting.GetSetting(connection, KaSetting.SN_EMAIL_USERNAME, KaSetting.SD_EMAIL_USERNAME)
			If username.Length > 0 Then _logger.Write("SMTP Username: " & username, Common.Logger.LogFileType.Processing)
			Dim password As String = KaSetting.GetSetting(connection, KaSetting.SN_EMAIL_PASSWORD, KaSetting.SD_EMAIL_PASSWORD)
			If username.Length > 0 Then
				client.UseDefaultCredentials = False
				client.Credentials = New System.Net.NetworkCredential(username, password)
			End If
			Dim daysToKeepEmailRecords As Integer = Integer.Parse(KaSetting.GetSetting(connection, "Tm2EmailService/DaysToKeepEmailRecords", "30"))
			_logger.Write(String.Format("Keep e-mail records for {0:0} days", daysToKeepEmailRecords), Common.Logger.LogFileType.Processing)

			Dim timeCheckStarted As DateTime = Now
			For Each reportId As Guid In reportList
				Try
					reportListEndTimes.Add(New KaEmailReport(connection, reportId), timeCheckStarted)
				Catch ex As RecordNotFoundException ' report was not found in the database
					_logger.Write(String.Format("Error: report for ID={0} not found in database", reportId), Common.Logger.LogFileType.Alarm)
					CreateEventLogEntry(KaEventLog.Categories.Warning, String.Format("Error: report for ID={0} not found in database", reportId), connection)
				End Try
			Next

			' create e-mail records for reports
			If allReportsSelected Then
				reportListEndTimes.Clear()
				Dim rdr As OleDbDataReader = Tm2Database.ExecuteReader(connection, String.Format("SELECT {0} FROM {1} WHERE {2} = 0", KaEmailReport.FN_ID, KaEmailReport.TABLE_NAME, KaEmailReport.FN_DELETED))
				Do While rdr.Read()
					Try
						Dim report As New KaEmailReport(connection, rdr("id"))
						If Not excludeReportList.Contains(report.Id) AndAlso Not reportListEndTimes.ContainsKey(report) Then
							reportListEndTimes.Add(report, timeCheckStarted)
						End If
					Catch ex As RecordNotFoundException ' report was not found in the database
						_logger.Write(String.Format("Error: report for ID={0} not found in database", rdr("id")), Common.Logger.LogFileType.Alarm)
						CreateEventLogEntry(KaEventLog.Categories.Warning, String.Format("Error: report for ID={0} not found in database", rdr("id")), connection)
					End Try
				Loop
				rdr.Close()
			End If

			GetScheduledReports(connection, reportListEndTimes, excludeReportList, timeCheckStarted)

			For Each report As KaEmailReport In reportListEndTimes.Keys
				Try
					If report.Disabled Then
						_logger.Write(String.Format("Skipping report {0} because it is disabled", report.Name), Common.Logger.LogFileType.Processing)
						Continue For
					ElseIf reportListEndTimes(report) > timeCheckStarted Then
						_logger.Write(String.Format("Skipping report {0} because it has not reached it's next trigger time of {1}", report.Name, String.Format(reportListEndTimes(report), "MM/dd/yyyy HH:mm:ss")), Common.Logger.LogFileType.Processing)
						Continue For
					End If
					Dim email As New KaEmail()
					email.ApplicationId = APPLICATION_ID
					email.Recipients = report.Recipients
					email.ReportParameters = report.ReportParameters
					email.ReportType = report.ReportType
					If report.ReportRunType = KaEmailReport.ReportRunTypes.SaveAsFile Then
						_logger.Write(String.Format("Creating file for report: {0} at location {1}", report.Name, report.FileSaveLocation), Common.Logger.LogFileType.Processing)
					Else
						_logger.Write(String.Format("E-mailing: {0} to {1}", email.Subject, email.Recipients), Common.Logger.LogFileType.Processing)
					End If
					Dim attachments As New List(Of Attachment)
					Dim lastSent As DateTime = timeCheckStarted
					Dim endDate As DateTime = reportListEndTimes(report)
					Select Case report.ReportType ' get the report body and data
						Case KaEmailReport.ReportTypes.BulkProductUsageReport : GetBulkProductUsageReport(connection, report, email, attachments, endDate) : lastSent = endDate
						Case KaEmailReport.ReportTypes.CarrierList : GetCarrierListReport(connection, report, email, attachments)
						Case KaEmailReport.ReportTypes.ContainerHistory : GetContainerHistoryReport(connection, report, email, attachments, endDate) : lastSent = endDate
						Case KaEmailReport.ReportTypes.ContainerList : GetContainerListReport(connection, report, email, attachments)
						Case KaEmailReport.ReportTypes.CustomerActivityReport : GetCustomerActivityReport(connection, report, email, attachments, endDate) : lastSent = endDate
						Case KaEmailReport.ReportTypes.DriverList : GetDriverListReport(connection, report, email, attachments)
						Case KaEmailReport.ReportTypes.DriverInFacilityHistoryReport : GetDriverInFacilityHistoryReport(connection, report, email, attachments, endDate) : lastSent = endDate
						Case KaEmailReport.ReportTypes.Inventory : GetInventoryReport(connection, report, email, attachments)
						Case KaEmailReport.ReportTypes.InventoryChangeReport : GetInventoryChangeReport(connection, report, email, attachments, endDate) : lastSent = endDate
						Case KaEmailReport.ReportTypes.OrderList : GetOrderListReport(connection, report, email, attachments)
						Case KaEmailReport.ReportTypes.ProductAllocation : GetProductAllocationReport(connection, report, email, attachments)
						Case KaEmailReport.ReportTypes.ProductList : GetProductListReport(connection, report, email, attachments)
						Case KaEmailReport.ReportTypes.ReceivingActivityReport : GetReceivingActivityReport(connection, report, email, attachments, endDate) : lastSent = endDate
						Case KaEmailReport.ReportTypes.ReceivingPurchaseOrderList : GetReceivingPurchaseOrderListReport(connection, report, email, attachments)
						Case KaEmailReport.ReportTypes.TankAlarmHistory : GetTankAlarmReport(connection, report, email, attachments, endDate) : lastSent = endDate
						Case KaEmailReport.ReportTypes.TankLevels : GetTankLevelReport(connection, report, email, attachments)
						Case KaEmailReport.ReportTypes.TankLevelTrend : GetTankLevelTrendReport(connection, report, email, attachments, endDate) : lastSent = endDate
						Case KaEmailReport.ReportTypes.TrackReport : GetTrackReport(connection, report, email, attachments, endDate) : lastSent = endDate
						Case KaEmailReport.ReportTypes.TransportList : GetTransportListReport(connection, report, email, attachments)
						Case KaEmailReport.ReportTypes.TransportUsageReport : GetTransportUsageReport(connection, report, email, attachments, endDate) : lastSent = endDate
						Case KaEmailReport.ReportTypes.TransportTrackingReport : GetTransportTrackingReport(connection, report, email, attachments)
						Case KaEmailReport.ReportTypes.TransportInFacilityHistoryReport : GetTransportInFacilityHistoryReport(connection, report, email, attachments, endDate) : lastSent = endDate
						Case KaEmailReport.ReportTypes.CustomReport : GetCustomReport(connection, report, email, attachments, endDate) : lastSent = endDate
						Case KaEmailReport.ReportTypes.InterfaceTicketExportStatusReport : GetInterfaceTicketExportStatusReport(connection, report, email, attachments, endDate)
						Case KaEmailReport.ReportTypes.InterfaceTicketReceivingExportStatusReport : GetInterfaceTicketReceivingExportStatusReport(connection, report, email, attachments, endDate)
					End Select

					'If report.IsMonthToDate Then
					'    If endDate.Month = report.LastSent.Month Then
					'        lastSent = New Date(report.LastSent.Year, report.LastSent.Month, 1)
					'    Else
					'        endDate = New Date(report.LastSent.Year, report.LastSent.Month, endDate.Day) ' Day exceeded last day of month
					'    End If
					'End If

					If report.ReportRunType = KaEmailReport.ReportRunTypes.Email Then
						email.Subject = report.Subject.Replace("{last_sent}", String.Format("{0:M/d/yyyy h:mm:ss tt}", report.LastSent)).Replace("{now}", String.Format("{0:M/d/yyyy h:mm:ss tt}", endDate))
						report.LastSent = lastSent
						email.SerializeAttachments(attachments)
						If KaEmail.CreateEmail(connection, email, 0, Nothing, _applicationIdentifier, "") Then
							report.SqlUpdate(connection, Nothing, _applicationIdentifier, "")
						Else
							_logger.Write(String.Format("Could not create email for report: {0} to {1}", report.Name, email.Recipients), Common.Logger.LogFileType.Alarm)
							CreateEventLogEntry(KaEventLog.Categories.Warning, String.Format("Could not create email for report: {0} to {1}", report.Name, email.Recipients), connection)
						End If
					Else
						report.LastSent = lastSent

						Dim fileCreated As Boolean = False

						Dim filename As String = report.FileSaveLocation
						Dim directory As String = IO.Path.GetDirectoryName(filename)
						Dim extension As String = ""
						If IO.Path.HasExtension(filename) Then
							extension = IO.Path.GetExtension(filename)
						Else
							Dim mediaType As String = GetParameter("format", report.ReportParameters, KaReports.MEDIA_TYPE_HTML)
							Select Case mediaType
								Case KaReports.MEDIA_TYPE_COMMA
									extension = "csv"
								Case KaReports.MEDIA_TYPE_HTML
									extension = KaReports.MEDIA_TYPE_HTML
								Case KaReports.MEDIA_TYPE_PFV
									extension = KaReports.MEDIA_TYPE_HTML
								Case Else
									extension = "txt"
							End Select
						End If
						filename = IO.Path.GetFileNameWithoutExtension(filename).Replace("{last_sent}", String.Format("{0:yyyyMMddHHmmss}", report.LastSent)).Replace("{now}", String.Format("{0:yyyyMMddHHmmss}", endDate))
						Dim tempFilename As String = String.Format("{0}\{1}.{2}", directory, filename, extension)
						Try
							If attachments.Count > 0 Then
								If Not IO.Directory.Exists(directory) Then IO.Directory.CreateDirectory(directory)
								Using fileStream As FileStream = File.OpenWrite(tempFilename)
									attachments(0).ContentStream.CopyTo(fileStream)
								End Using
							End If
							fileCreated = True
						Catch ex As Exception

						End Try
						If fileCreated Then
							report.SqlUpdate(connection, Nothing, _applicationIdentifier, "")
						Else
							_logger.Write(String.Format("Could not create file for report: {0} at location {1}", report.Name, tempFilename), Common.Logger.LogFileType.Alarm)
							CreateEventLogEntry(KaEventLog.Categories.Warning, String.Format("Could not create file for report: {0} at location {1}", report.Name, tempFilename), connection)
						End If
					End If
				Catch ex As Exception
					_logger.Write(Alerts.FormatException(ex), Common.Logger.LogFileType.Alarm)
					If Not TypeOf ex Is SmtpException Then CreateEventLogEntry(KaEventLog.Categories.Failure, KaCommonObjects.Alerts.FormatException(ex), connection)
				End Try
			Next

			Dim owners As New Dictionary(Of Guid, KaOwner)
			Dim applicators As New Dictionary(Of Guid, KaApplicator)
			Dim branches As New Dictionary(Of Guid, KaBranch)
			Dim carriers As New Dictionary(Of Guid, KaCarrier)
			Dim drivers As New Dictionary(Of Guid, KaDriver)
			Dim locations As New Dictionary(Of Guid, KaLocation)
			Dim customerAccounts As New Dictionary(Of Guid, KaCustomerAccount)
			Dim customerAccountLocations As New Dictionary(Of Guid, KaCustomerAccountLocation)
			Dim suppliers As New Dictionary(Of Guid, KaSupplierAccount)

			' create e-mail records for tickets
			If emailTickets Then EmailTicketsToEntities(connection, smtpUrl, branches, customerAccounts, customerAccountLocations, owners, carriers, drivers, locations, applicators)

			' create e-mails for order summaries
			If emailOrderSummaries Then EmailOrderSummariesToEntities(connection, smtpUrl, branches, customerAccounts, customerAccountLocations, owners, applicators)

			' create e-mails for receiving tickets
			If emailReceivingTickets Then EmailReceivingTicketsToEntities(connection, smtpUrl, owners, carriers, drivers, locations, suppliers)

			' send all e-mails
			If smtpUrl.Trim().Length = 0 Then
				Throw New SmtpException("Invalid SMTP server address") ' if the system isn't configured to e-mail, exit now
			ElseIf senderAddress.Trim().Length = 0 Then
				Throw New SmtpException("Invalid SMTP email address") ' if the system isn't configured to e-mail, exit now
			End If
			Dim markEmailsWithNoRecipientAsSent As Boolean = True
			Boolean.TryParse(KaSetting.GetSetting(connection, "General/MarkEmailsWithNoRecipientAsSent", True), markEmailsWithNoRecipientAsSent)

			For Each email As KaEmail In KaEmail.GetAll(connection, KaEmail.FN_DELETED & "=0", "")
				Try
					If email.Recipients.Trim.Length = 0 AndAlso markEmailsWithNoRecipientAsSent Then
						Console.Write("Marking message {0} as sent since there are no recipients...", email.Subject, email.Recipients)
					Else
						Console.Write("Sending message {0} to {1}...", email.Subject, email.Recipients)

						client.Send(email.GetMessage(senderAddress))
					End If
					email.SentAt = Now
					email.Deleted = True
					email.SqlUpdate(connection, Nothing, _applicationIdentifier, "")
					Console.WriteLine("done")
					_logger.Write(String.Format("Sending message {0} to {1}...done", email.Subject, email.Recipients), Common.Logger.LogFileType.Processing)
				Catch ex As Exception
					Console.WriteLine("failed: " & ex.Message)
					_logger.Write(String.Format("Sending message {0} to {1}...failed: ", email.Subject, email.Recipients) & KaCommonObjects.Alerts.FormatException(ex), Common.Logger.LogFileType.Alarm)
					_logger.Write(String.Format("Sending message {0} to {1}...failed: ", email.Subject, email.Recipients) & KaCommonObjects.Alerts.FormatException(ex), Common.Logger.LogFileType.Processing)
					email.Deleted = False
					email.SendingErrorDetails = ex.Message
					email.DbConnection = dbConnection
					email.SqlUpdate(connection, Nothing, _applicationIdentifier, "")
					CreateEventLogEntry(KaEventLog.Categories.Failure, String.Format("Sending message {0} to {1}...failed: ", email.Subject, email.Recipients) & vbCrLf & KaCommonObjects.Alerts.FormatException(ex), connection)
				End Try
			Next

			' delete old e-mail records
			Tm2Database.ExecuteNonQuery(connection, String.Format("DELETE FROM emails WHERE deleted=1 AND last_updated < '{0:M/d/yyyy}'", Now.Subtract(New TimeSpan(daysToKeepEmailRecords, 0, 0, 0))))
		Catch ex As Exception
			_logger.Write(Alerts.FormatException(ex), Common.Logger.LogFileType.Alarm)
			If Not TypeOf ex Is SmtpException Then CreateEventLogEntry(KaEventLog.Categories.Failure, KaCommonObjects.Alerts.FormatException(ex), connection)
		Finally
			connection.Close()
			Common.SetNotRunning()
		End Try

		Environment.Exit(0)
	End Sub

	Private Sub EmailTicketsToEntities(ByVal connection As OleDbConnection, ByVal smtpUrl As String, ByRef branches As Dictionary(Of Guid, KaBranch), ByRef customerAccounts As Dictionary(Of Guid, KaCustomerAccount), ByRef customerAccountLocations As Dictionary(Of Guid, KaCustomerAccountLocation), ByRef owners As Dictionary(Of Guid, KaOwner), ByRef carriers As Dictionary(Of Guid, KaCarrier), ByRef drivers As Dictionary(Of Guid, KaDriver), ByRef locations As Dictionary(Of Guid, KaLocation), ByRef applicators As Dictionary(Of Guid, KaApplicator))
		Dim orders As New Dictionary(Of Guid, KaOrder)

		Dim emailOwner As Boolean = Boolean.Parse(KaSetting.GetSetting(connection, KaSetting.SN_EMAIL_TICKET_OWNER, KaSetting.SD_EMAIL_TICKET_OWNER))
		Dim emailApplicator As Boolean = Boolean.Parse(KaSetting.GetSetting(connection, KaSetting.SN_EMAIL_TICKET_APPLICATOR, KaSetting.SD_EMAIL_TICKET_APPLICATOR))
		Dim emailBranch As Boolean = Boolean.Parse(KaSetting.GetSetting(connection, KaSetting.SN_EMAIL_TICKET_BRANCH, KaSetting.SD_EMAIL_TICKET_BRANCH))
		Dim emailAccount As Boolean = Boolean.Parse(KaSetting.GetSetting(connection, KaSetting.SN_EMAIL_TICKET_ACCOUNT, KaSetting.SD_EMAIL_TICKET_ACCOUNT))
		Dim emailCarrier As Boolean = Boolean.Parse(KaSetting.GetSetting(connection, KaSetting.SN_EMAIL_TICKET_CARRIER, KaSetting.SD_EMAIL_TICKET_CARRIER))
		Dim emailDriver As Boolean = Boolean.Parse(KaSetting.GetSetting(connection, KaSetting.SN_EMAIL_TICKET_DRIVER, KaSetting.SD_EMAIL_TICKET_DRIVER))
		Dim emailLocation As Boolean = Boolean.Parse(KaSetting.GetSetting(connection, KaSetting.SN_EMAIL_TICKET_LOCATION, KaSetting.SD_EMAIL_TICKET_LOCATION))
		Dim emailAccountLocation As Boolean = Boolean.Parse(KaSetting.GetSetting(connection, KaSetting.SN_EMAIL_TICKET_ACCOUNT_LOCATION, KaSetting.SD_EMAIL_TICKET_ACCOUNT_LOCATION))

		For Each ticket As KaTicket In KaTicket.GetAll(connection, KaTicket.FN_EMAILED & " = 0 AND " & KaTicket.FN_DO_NOT_EMAIL & " = 0 AND " & KaTicket.FN_VOIDED & " = 0", "loaded_at ASC")
			If Not IsTicketEmailed(connection, ticket.Id) Then ' does the ticket still need to be e-mailed
				Dim order As KaOrder
				Try ' to get the order from the cache
					order = orders(ticket.OrderId)
				Catch ex As KeyNotFoundException ' the order isn't in the cache
					Try ' to get the order from the database
						order = New KaOrder(connection, ticket.OrderId)
						orders(order.Id) = order
					Catch ex2 As RecordNotFoundException ' the order isn't in the database
						order = Nothing
					End Try
				End Try
				If order IsNot Nothing Then
					Dim owner As KaOwner
					Try ' to get the owner from the cache
						If ticket.OwnerId = Guid.Empty Then
							owner = owners(order.OwnerId)
						Else
							owner = owners(ticket.OwnerId)
						End If
					Catch ex As KeyNotFoundException ' the owner isn't in the cache
						Try ' to get the owner from the database
							If ticket.OwnerId = Guid.Empty Then
								owner = New KaOwner(connection, order.OwnerId)
							Else
								owner = New KaOwner(connection, ticket.OwnerId)
							End If
							owners(owner.Id) = owner
						Catch ex2 As Exception ' the owner isn't in the database
							owner = Nothing
						End Try
					End Try
					If owner IsNot Nothing Then
						Dim url As String = KaSetting.GetSetting(connection, "TerminalManagement2/TicketAddress/OwnerId=" & owner.Id.ToString(), "http://localhost/TerminalManagement2/ticket.aspx")
						url &= IIf(url.Contains("ticket_id="), "", IIf(url.Contains("?"), "&", "?") & "ticket_id=") & ticket.Id.ToString()

						Dim recipients As String = ""
						If emailOwner Then
							recipients &= owner.Email
						End If
						If emailApplicator Then
							Dim applicator As KaApplicator = GetApplicator(connection, ticket.ApplicatorId, applicators)
							If applicator IsNot Nothing AndAlso applicator.Email.Trim().Length Then recipients &= IIf(recipients.Length > 0 > 0, ",", "") & applicator.Email
						End If
						If emailBranch Then
							Dim branch As KaBranch = GetBranch(connection, ticket.BranchId, branches)
							If branch IsNot Nothing AndAlso branch.Email.Trim().Length > 0 Then recipients &= IIf(recipients.Length > 0, ",", "") & branch.Email
						End If
						If emailAccount Then
							For Each tca As KaTicketCustomerAccount In ticket.TicketCustomerAccounts ' // e-mail this ticket to the customer accounts
								If tca.Email.Trim().Length > 0 Then recipients &= IIf(recipients.Length > 0, ",", "") & tca.Email
							Next
						End If
						If emailCarrier Then
							Dim carrier As KaCarrier = GetCarrier(connection, ticket.CarrierId, carriers)
							If carrier IsNot Nothing AndAlso carrier.Email.Trim().Length > 0 Then recipients &= IIf(recipients.Length > 0, ",", "") & carrier.Email
						End If
						If emailDriver Then
							Dim driver As KaDriver = GetDriver(connection, ticket.DriverId, drivers)
							If driver IsNot Nothing AndAlso driver.Email.Trim().Length > 0 Then recipients &= IIf(recipients.Length > 0, ",", "") & driver.Email
						End If
						Dim location As KaLocation = GetLocation(connection, ticket.LocationId, locations)
						If emailLocation Then
							If location IsNot Nothing AndAlso location.Email.Trim().Length > 0 Then recipients &= IIf(recipients.Length > 0, ",", "") & location.Email
						End If
						If emailAccountLocation Then
							Dim customerAccountLocation As KaCustomerAccountLocation = GetCustomerAccountLocation(connection, ticket.CustomerAccountLocationId, customerAccountLocations)
							If customerAccountLocation IsNot Nothing AndAlso customerAccountLocation.Email.Trim().Length > 0 Then recipients &= IIf(recipients.Length > 0, ",", "") & customerAccountLocation.Email
						End If

						If recipients.Trim().Length > 0 Then
							Dim email As New KaEmail()
							email.ApplicationId = APPLICATION_ID
							Try ' to get the ticket HTML from the ticket web application
								email.Body = GetTicketHtml(url)
								email.BodyIsHtml = True
								Dim htmlStream As New MemoryStream(Encoding.UTF8.GetBytes(email.Body))
								Dim attachments As New List(Of Attachment)
								attachments.Add(New Attachment(htmlStream, "ticket.html", MediaTypeNames.Text.Html))
								email.SerializeAttachments(attachments)
							Catch ex As Exception ' failed to get ticket HTML, send exception details instead
								email.Body = "Failed to get ticket, details below:" & Environment.NewLine & Environment.NewLine & ex.ToString()
								email.BodyIsHtml = False
							End Try
							email.Subject = String.Format("Ticket {0} for order {1}", ticket.Number, order.Number)
							If location IsNot Nothing Then email.Subject &= " at " & location.Name
							email.Recipients = recipients
							If KaEmail.CreateEmail(connection, email, 0, Nothing, System.Net.Dns.GetHostName() & "/" & My.Application.Info.ProductName, "") Then
								Tm2Database.ExecuteNonQuery(connection, String.Format("UPDATE tickets SET emailed=1, {0} = {1}, {2} = {3} WHERE id={4}", KaTicket.FN_LAST_UPDATED_APPLICATION, Q(_applicationIdentifier), KaTicket.FN_LAST_UPDATED_USER, Q(""), Q(ticket.Id)))
								_logger.Write(String.Format("E-mailing: {0} to {1}", email.Subject, email.Recipients), Common.Logger.LogFileType.Processing)
							ElseIf smtpUrl = "" Then
								Tm2Database.ExecuteNonQuery(connection, String.Format("UPDATE tickets SET emailed=1, {0} = {1}, {2} = {3} WHERE id={4}", KaTicket.FN_LAST_UPDATED_APPLICATION, Q(_applicationIdentifier), KaTicket.FN_LAST_UPDATED_USER, Q(""), Q(ticket.Id)))
								_logger.Write(String.Format("Marked ticket {0} for order {1} as emailed, since there were no email SMTP Servers assigned", ticket.Number, order.Number), Common.Logger.LogFileType.Processing)
							Else
								_logger.Write(String.Format("Could not create email: {0} to {1}", email.Subject, email.Recipients), Common.Logger.LogFileType.Alarm)
								CreateEventLogEntry(KaEventLog.Categories.Warning, String.Format("Could not create email for ticket: {0} to {1}", email.Subject, email.Recipients), connection)
							End If
						Else
							Tm2Database.ExecuteNonQuery(connection, String.Format("UPDATE tickets SET emailed=1, {0} = {1}, {2} = {3} WHERE id={4}", KaTicket.FN_LAST_UPDATED_APPLICATION, Q(_applicationIdentifier), KaTicket.FN_LAST_UPDATED_USER, Q(""), Q(ticket.Id)))
							_logger.Write(String.Format("Marked ticket {0} for order {1} as emailed, since there were no email recipients assigned", ticket.Number, order.Number), Common.Logger.LogFileType.Processing)
						End If
					End If
				End If
			End If
		Next
	End Sub

	Private Sub EmailOrderSummariesToEntities(ByVal connection As OleDbConnection, ByVal smtpUrl As String, ByRef branches As Dictionary(Of Guid, KaBranch), ByRef customerAccounts As Dictionary(Of Guid, KaCustomerAccount), ByRef customerAccountLocations As Dictionary(Of Guid, KaCustomerAccountLocation), ByRef owners As Dictionary(Of Guid, KaOwner), ByRef applicators As Dictionary(Of Guid, KaApplicator))
		Dim ownerOrderSummaryUrls As New Dictionary(Of Guid, String)

		Dim sendOrderSummaryToApplicator As Boolean = Boolean.Parse(KaSetting.GetSetting(connection, KaSetting.SN_EMAIL_ORDER_SUMMARY_TO_APPLICATOR, KaSetting.SD_EMAIL_ORDER_SUMMARY_TO_APPLICATOR))
		Dim sendOrderSummaryToBranch As Boolean = Boolean.Parse(KaSetting.GetSetting(connection, KaSetting.SN_EMAIL_ORDER_SUMMARY_TO_BRANCH, KaSetting.SD_EMAIL_ORDER_SUMMARY_TO_BRANCH))
		Dim sendOrderSummaryToCustomerAccount As Boolean = Boolean.Parse(KaSetting.GetSetting(connection, KaSetting.SN_EMAIL_ORDER_SUMMARY_TO_CUSTOMER_ACCOUNT, KaSetting.SD_EMAIL_ORDER_SUMMARY_TO_CUSTOMER_ACCOUNT))
		Dim sendOrderSummaryToCustomerAccountLocation As Boolean = Boolean.Parse(KaSetting.GetSetting(connection, KaSetting.SN_EMAIL_ORDER_SUMMARY_TO_CUSTOMER_ACCOUNT_LOCATION, KaSetting.SD_EMAIL_ORDER_SUMMARY_TO_CUSTOMER_ACCOUNT_LOCATION))
		Dim sendOrderSummaryToOwner As Boolean = Boolean.Parse(KaSetting.GetSetting(connection, KaSetting.SN_EMAIL_ORDER_SUMMARY_TO_OWNER, KaSetting.SD_EMAIL_ORDER_SUMMARY_TO_OWNER))
		For Each order As KaOrder In KaOrder.GetAll(connection, String.Format("{0}=1 AND {1}=0", KaOrder.FN_COMPLETED, KaOrder.FN_ORDER_SUMMARY_EMAILED), "last_updated ASC")
			Dim recipients As String = ""
			If sendOrderSummaryToApplicator Then
				Dim applicator As KaApplicator = GetApplicator(connection, order.ApplicatorId, applicators)
				If applicator IsNot Nothing AndAlso applicator.Email.Trim().Length > 0 Then recipients &= IIf(recipients.Length > 0, ",", "") & applicator.Email
			End If
			If sendOrderSummaryToBranch Then
				Dim branch As KaBranch = GetBranch(connection, order.BranchId, branches)
				If branch IsNot Nothing AndAlso branch.Email.Trim().Length > 0 Then recipients &= IIf(recipients.Length > 0, ",", "") & branch.Email
			End If
			If sendOrderSummaryToCustomerAccount Then
				For Each orderCustomerAccount As KaOrderCustomerAccount In order.OrderAccounts
					Dim customerAccount As KaCustomerAccount = GetCustomerAccount(connection, orderCustomerAccount.CustomerAccountId, customerAccounts)
					If customerAccount IsNot Nothing AndAlso customerAccount.Email.Trim().Length > 0 Then recipients &= IIf(recipients.Length > 0, ",", "") & customerAccount.Email
				Next
			End If
			If sendOrderSummaryToCustomerAccountLocation Then
				Dim customerAccountLocation As KaCustomerAccountLocation = GetCustomerAccountLocation(connection, order.CustomerAccountLocationId, customerAccountLocations)
				If customerAccountLocation IsNot Nothing AndAlso customerAccountLocation.Email.Trim().Length > 0 Then recipients &= IIf(recipients.Length > 0, ",", "") & customerAccountLocation.Email
			End If
			If sendOrderSummaryToOwner Then
				Dim owner As KaOwner = GetOwner(connection, order.OwnerId, owners)
				If owner IsNot Nothing AndAlso owner.Email.Trim().Length > 0 Then recipients &= IIf(recipients.Length > 0, ",", "") & owner.Email
			End If
			Dim url As String
			Try ' to look up the owner's order summary URL in the dictionary...
				url = ownerOrderSummaryUrls(order.OwnerId)
			Catch ex As KeyNotFoundException ' the owner's order summary URL isn't in the dictionary...
				url = KaSetting.GetSetting(connection, KaSetting.SN_ORDER_SUMMARY_URL_FOR_OWNER & order.OwnerId.ToString(), KaSetting.SD_ORDER_SUMMARY_URL_FOR_OWNER)
				url &= IIf(url.Contains("order_id="), "", IIf(url.Contains("?"), "&", "?") & "order_id=")
				ownerOrderSummaryUrls(order.OwnerId) = url
			End Try

			If recipients.Trim.Length > 0 Then
				Dim email As New KaEmail()
				email.ApplicationId = APPLICATION_ID
				Try ' to get the order HTML from the order summary web application
					email.Body = GetTicketHtml(url & order.Id.ToString())
					email.BodyIsHtml = True
					Dim htmlStream As New MemoryStream(Encoding.UTF8.GetBytes(email.Body))
					Dim attachments As New List(Of Attachment)
					attachments.Add(New Attachment(htmlStream, "ordersummary.html", MediaTypeNames.Text.Html))
					email.SerializeAttachments(attachments)
				Catch ex As Exception ' failed to get ticket HTML, send exception details instead
					email.Body = "Failed to get order summary, details below:" & Environment.NewLine & Environment.NewLine & ex.ToString()
					email.BodyIsHtml = False
				End Try
				email.Subject = String.Format("Order summary for order {0}", order.Number)
				email.Recipients = recipients
				If KaEmail.CreateEmail(connection, email, 0, Nothing, _applicationIdentifier, "") Then
					_logger.Write(String.Format("E-mailing: {0} to {1}", email.Subject, email.Recipients), Common.Logger.LogFileType.Processing)
					SetOrderSummaryEmailed(order.Id, connection)
				ElseIf smtpUrl = "" Then
					SetOrderSummaryEmailed(order.Id, connection)
					_logger.Write(String.Format("Marked order summary for order {0} as emailed, since there were no email SMTP Servers assigned", order.Number), Common.Logger.LogFileType.Processing)
				Else
					_logger.Write(String.Format("Could not create email for order summary: {0} to {1}", email.Subject, email.Recipients), Common.Logger.LogFileType.Alarm)
					CreateEventLogEntry(KaEventLog.Categories.Warning, String.Format("Could not create email for order summary: {0} to {1}", email.Subject, email.Recipients), connection)
				End If
			Else
				SetOrderSummaryEmailed(order.Id, connection)
				_logger.Write(String.Format("Marked order summary for order {0} as emailed, since there were no email recipients assigned", order.Number), Common.Logger.LogFileType.Processing)
			End If
		Next
	End Sub

	Private Sub EmailReceivingTicketsToEntities(ByVal connection As OleDbConnection, ByVal smtpUrl As String, ByRef owners As Dictionary(Of Guid, KaOwner), ByRef carriers As Dictionary(Of Guid, KaCarrier), ByRef drivers As Dictionary(Of Guid, KaDriver), ByRef locations As Dictionary(Of Guid, KaLocation), ByRef suppliers As Dictionary(Of Guid, KaSupplierAccount))
		' create e-mail records for tickets
		Dim receivingPurchaseOrders As New Dictionary(Of Guid, KaReceivingPurchaseOrder)

		Dim emailOwner As Boolean = Boolean.Parse(KaSetting.GetSetting(connection, KaSetting.SN_EMAIL_RECEIVING_TICKET_OWNER, KaSetting.SD_EMAIL_RECEIVING_TICKET_OWNER))
		Dim emailSupplier As Boolean = Boolean.Parse(KaSetting.GetSetting(connection, KaSetting.SN_EMAIL_RECEIVING_TICKET_SUPPLIER, KaSetting.SD_EMAIL_RECEIVING_TICKET_SUPPLIER))
		Dim emailCarrier As Boolean = Boolean.Parse(KaSetting.GetSetting(connection, KaSetting.SN_EMAIL_RECEIVING_TICKET_CARRIER, KaSetting.SD_EMAIL_RECEIVING_TICKET_CARRIER))
		Dim emailDriver As Boolean = Boolean.Parse(KaSetting.GetSetting(connection, KaSetting.SN_EMAIL_RECEIVING_TICKET_DRIVER, KaSetting.SD_EMAIL_RECEIVING_TICKET_DRIVER))
		Dim emailLocation As Boolean = Boolean.Parse(KaSetting.GetSetting(connection, KaSetting.SN_EMAIL_RECEIVING_TICKET_LOCATION, KaSetting.SD_EMAIL_RECEIVING_TICKET_LOCATION))

		For Each receivingTicket As KaReceivingTicket In KaReceivingTicket.GetAll(connection, KaReceivingTicket.FN_EMAILED & " = 0 AND " & KaReceivingTicket.FN_DO_NOT_EMAIL & " = 0 AND " & KaReceivingTicket.FN_VOIDED & " = 0", KaReceivingTicket.FN_CREATED & " ASC")
			If Not IsReceivingTicketEmailed(connection, receivingTicket.Id) Then ' does the ticket still need to be e-mailed
				Dim receivingPurchaseOrder As KaReceivingPurchaseOrder
				Try ' to get the order from the cache
					receivingPurchaseOrder = receivingPurchaseOrders(receivingTicket.ReceivingPurchaseOrderId)
				Catch ex As KeyNotFoundException ' the order isn't in the cache
					Try ' to get the order from the database
						receivingPurchaseOrder = New KaReceivingPurchaseOrder(connection, receivingTicket.ReceivingPurchaseOrderId)
						receivingPurchaseOrders(receivingPurchaseOrder.Id) = receivingPurchaseOrder
					Catch ex2 As RecordNotFoundException ' the order isn't in the database
						receivingPurchaseOrder = Nothing
					End Try
				End Try
				If receivingPurchaseOrder IsNot Nothing Then
					Dim owner As KaOwner
					Try ' to get the owner from the cache
						If receivingTicket.OwnerId = Guid.Empty Then
							owner = owners(receivingPurchaseOrder.OwnerId)
						Else
							owner = owners(receivingTicket.OwnerId)
						End If
					Catch ex As KeyNotFoundException ' the owner isn't in the cache
						Try ' to get the owner from the database
							If receivingTicket.OwnerId = Guid.Empty Then
								owner = New KaOwner(connection, receivingPurchaseOrder.OwnerId)
							Else
								owner = New KaOwner(connection, receivingTicket.OwnerId)
							End If
							owners(owner.Id) = owner
						Catch ex2 As Exception ' the owner isn't in the database
							owner = Nothing
						End Try
					End Try
					If owner IsNot Nothing Then
						Dim url As String = GetReceivingTicketHtmlAddress(connection, receivingTicket)
						Dim emailAddresses As New List(Of String)
						If emailOwner Then
							emailAddresses.Add(owner.Email)
						End If
						If emailSupplier Then
							Dim supplier As KaSupplierAccount = GetSupplierAccount(connection, receivingTicket.SupplierAccountId, suppliers)
							If supplier IsNot Nothing AndAlso supplier.Email.Length > 0 AndAlso Not emailAddresses.Contains(supplier.Email) Then emailAddresses.Add(supplier.Email)
						End If
						If emailCarrier Then
							Dim carrier As KaCarrier = GetCarrier(connection, receivingTicket.CarrierId, carriers)
							If carrier IsNot Nothing AndAlso carrier.Email.Length > 0 AndAlso Not emailAddresses.Contains(carrier.Email) Then emailAddresses.Add(carrier.Email)
						End If
						If emailDriver Then
							Dim driver As KaDriver = GetDriver(connection, receivingTicket.DriverId, drivers)
							If driver IsNot Nothing AndAlso driver.Email.Length > 0 AndAlso Not emailAddresses.Contains(driver.Email) Then emailAddresses.Add(driver.Email)
						End If
						Dim location As KaLocation = GetLocation(connection, receivingTicket.LocationId, locations)
						If emailLocation Then
							If location IsNot Nothing AndAlso location.Email.Length > 0 AndAlso Not emailAddresses.Contains(location.Email) Then emailAddresses.Add(location.Email)
						End If

						Dim recipients As String = ""
						For Each emailAddr As String In emailAddresses
							If recipients.Length > 0 Then recipients &= ","
							recipients &= emailAddr
						Next
						If recipients.Trim().Length > 0 Then
							Dim email As New KaEmail()
							email.ApplicationId = APPLICATION_ID
							Try ' to get the ticket HTML from the ticket web application
								email.Body = GetTicketHtml(url)
								email.BodyIsHtml = True
								Dim htmlStream As New MemoryStream(Encoding.UTF8.GetBytes(email.Body))
								Dim attachments As New List(Of Attachment)
								attachments.Add(New Attachment(htmlStream, "ReceivingTicket.html", MediaTypeNames.Text.Html))
								email.SerializeAttachments(attachments)
							Catch ex As Exception ' failed to get ticket HTML, send exception details instead
								email.Body = "Failed to get receiving ticket, details below:" & Environment.NewLine & Environment.NewLine & ex.ToString()
								email.BodyIsHtml = False
							End Try
							email.Subject = String.Format("Receiving ticket {0} for order {1}", receivingTicket.Number, receivingPurchaseOrder.Number)
							If location IsNot Nothing Then email.Subject &= " at " & location.Name
							email.Recipients = recipients
							If KaEmail.CreateEmail(connection, email, 0, Nothing, _applicationIdentifier, "") Then
								Tm2Database.ExecuteNonQuery(connection, String.Format("UPDATE " & KaReceivingTicket.TABLE_NAME & " SET " & KaReceivingTicket.FN_EMAILED & "=1 WHERE id={0}", Q(receivingTicket.Id)))
								_logger.Write(String.Format("E-mailing: {0} to {1}", email.Subject, email.Recipients), Common.Logger.LogFileType.Processing)
							ElseIf smtpUrl = "" Then
								Tm2Database.ExecuteNonQuery(connection, String.Format("UPDATE " & KaReceivingTicket.TABLE_NAME & " SET " & KaReceivingTicket.FN_EMAILED & "=1 WHERE id={0}", Q(receivingTicket.Id)))
								_logger.Write(String.Format("Marked receiving ticket {0} for purchase order {1} as emailed, since there were no email SMTP Servers assigned", receivingTicket.Number, receivingPurchaseOrder.Number), Common.Logger.LogFileType.Processing)
							Else
								_logger.Write(String.Format("Could not create email: {0} to {1}", email.Subject, email.Recipients), Common.Logger.LogFileType.Alarm)
								CreateEventLogEntry(KaEventLog.Categories.Warning, String.Format("Could not create email for receiving ticket: {0} to {1}", email.Subject, email.Recipients), connection)
							End If
						Else
							Tm2Database.ExecuteNonQuery(connection, String.Format("UPDATE " & KaReceivingTicket.TABLE_NAME & " SET " & KaReceivingTicket.FN_EMAILED & "=1 WHERE id={0}", Q(receivingTicket.Id)))
							_logger.Write(String.Format("Marked receiving ticket {0} for purchase order {1} as emailed, since there were no email recipients assigned", receivingTicket.Number, receivingPurchaseOrder.Number), Common.Logger.LogFileType.Processing)
						End If
					End If
				End If
			End If
		Next
	End Sub

	Public Shared Function GetReceivingTicketHtmlAddress(ByVal connection As OleDbConnection, ByVal receivingTicket As KaReceivingTicket) As String
		Dim bogusEntry As String = Guid.NewGuid.ToString
		Dim ownerTicketAddress As String = KaSetting.GetSetting(connection, "Receiving_PO_Web_Ticket_Address/OwnerId=" & receivingTicket.OwnerId.ToString(), bogusEntry, False, Nothing)
		If ownerTicketAddress = bogusEntry Then ownerTicketAddress = KaSetting.GetSetting(connection, "Receiving_PO_Web_Ticket_Address", "http://localhost/TerminalManagement2/ReceivingTicket.aspx", False, Nothing)
		Dim dbFound As Boolean = False
		Dim ticketIdFound As Boolean = False
		Dim truckIdFound As Boolean = False
		Dim parameters As String = ""
		If ownerTicketAddress.Trim().Length > 0 Then
			If ownerTicketAddress.IndexOf("?") > 0 Then
				Dim htmlParameters() As String = ownerTicketAddress.Substring(ownerTicketAddress.IndexOf("?") + 1).Split("&")
				For Each htmlParam As String In htmlParameters
					If htmlParam.Length > 0 Then
						If parameters.Length > 0 Then parameters &= "&"
						Dim param() As String = htmlParam.Split("=")
						Select Case param(0).Trim.ToLower
							Case "db"
								dbFound = True
								If param.Length > 1 Then
									If param(1).Trim.Length > 0 Then
										parameters &= param(0) & "=" & param(1)
									Else
										parameters &= param(0) & "=TM2"
									End If
								Else
									parameters &= param(0) & "=TM2"
								End If
							Case "ticket_id"
								ticketIdFound = True
								parameters &= param(0) & "=" + receivingTicket.Id.ToString
							Case "truck_id"
								truckIdFound = True
								parameters &= param(0) & "=" + receivingTicket.TransportId.ToString
							Case Else
								parameters &= htmlParam
						End Select
					End If
				Next
				ownerTicketAddress = ownerTicketAddress.Substring(0, ownerTicketAddress.IndexOf("?"))
			End If
			If (ownerTicketAddress.Trim().Length > 0) Then
				If Not dbFound Then
					If parameters.Length > 0 Then parameters &= "&"
					parameters &= "db=TM2"
				End If
				If Not ticketIdFound Then
					parameters &= "&ticket_id=" + receivingTicket.Id.ToString
				End If
				If Not truckIdFound Then
					parameters &= "&truck_id=" + receivingTicket.TransportId.ToString
				End If
				ownerTicketAddress &= "?" & parameters & "&instanceGuid=" & Guid.NewGuid.ToString
			End If
		End If
		'   End If
		Return ownerTicketAddress
	End Function


	Private Sub SetOrderSummaryEmailed(orderId As Guid, connection As OleDbConnection)
		Tm2Database.ExecuteNonQuery(connection, String.Format("UPDATE {0} SET {1}=1, {2}={3} WHERE {4}={5}", KaOrder.TABLE_NAME, KaOrder.FN_ORDER_SUMMARY_EMAILED, KaOrder.FN_LAST_UPDATED_APPLICATION, Q(_applicationIdentifier), KaOrder.FN_ID, Q(orderId)))
	End Sub

#Region " Get Reports "
	Private Sub GetCarrierListReport(connection As OleDbConnection, report As KaEmailReport, email As KaEmail, attachments As List(Of Attachment))
		Dim mediaType As String = GetParameter("format", report.ReportParameters, KaReports.MEDIA_TYPE_HTML)
		Dim table As ArrayList = KaReports.GetCarrierTable(connection)
		If mediaType = KaReports.MEDIA_TYPE_COMMA Then
			email.Body = "See attached report"
			email.BodyIsHtml = False
			attachments.Add(New Attachment(New MemoryStream(Encoding.UTF8.GetBytes(KaReports.GetTableCsv(email.Subject, "", table))), "report.csv", MediaTypeNames.Text.Plain))
		Else
			email.Body = CreateSiteCssStyle(SITE_CSS_FILENAME) & KaReports.GetTableHtml(email.Subject, table)
			email.BodyIsHtml = True
			attachments.Add(New Attachment(New MemoryStream(Encoding.UTF8.GetBytes(email.Body)), "report.html", MediaTypeNames.Text.Html))
		End If
	End Sub

	Private Sub GetContainerHistoryReport(connection As OleDbConnection, report As KaEmailReport, email As KaEmail, attachments As List(Of Attachment), ByVal endDate As DateTime)
		Dim containerId As Guid = Guid.Parse(GetParameter("container_id", report.ReportParameters, Guid.Empty.ToString()))
		Dim columnsDisplayed As ULong = 0
		For columnIndex = 0 To KaReports.ContainerReportColumns.RcLot
			columnsDisplayed += 2 ^ columnIndex
		Next
		columnsDisplayed = GetParameter("columns", report.ReportParameters, columnsDisplayed)
		Dim mediaType As String = GetParameter("format", report.ReportParameters, KaReports.MEDIA_TYPE_HTML)
		Dim cellAttributes As New List(Of String)
		Dim containerHistoryList As ArrayList = KaReports.GetContainerHistoryTable(connection, mediaType, containerId, columnsDisplayed, report.LastSent, endDate, cellAttributes)
		If mediaType = KaReports.MEDIA_TYPE_COMMA Then
			email.Body = "See attached report"
			email.BodyIsHtml = False
			attachments.Add(New Attachment(New MemoryStream(Encoding.UTF8.GetBytes(KaReports.GetTableCsv(email.Subject, "", containerHistoryList))), "report.csv", MediaTypeNames.Text.Plain))
		Else
			email.Body = CreateSiteCssStyle(SITE_CSS_FILENAME) & KaReports.GetTableHtml(email.Subject, "", containerHistoryList, False, "class=""display"" style=""width: 100%;""", "", New List(Of String), "", New List(Of String), cellAttributes, True)
			email.BodyIsHtml = True
			attachments.Add(New Attachment(New MemoryStream(Encoding.UTF8.GetBytes(email.Body)), "report.html", MediaTypeNames.Text.Html))
		End If
	End Sub

	Private Sub GetContainerListReport(connection As OleDbConnection, report As KaEmailReport, email As KaEmail, attachments As List(Of Attachment))
		Dim mediaType As String = GetParameter("format", report.ReportParameters, KaReports.MEDIA_TYPE_HTML)
		If mediaType = MEDIA_TYPE_HTML Then mediaType = MEDIA_TYPE_PFV
		Dim columnsDisplayed As ULong = 0
		For columnIndex = 0 To KaReports.ContainerReportColumns.RcLot
			columnsDisplayed += 2 ^ columnIndex
		Next
		columnsDisplayed = GetParameter("columns", report.ReportParameters, columnsDisplayed)
		Dim tableAttributes As String = ""
		Dim headerRowAttributes As String = ""
		Dim headerCellAttributes As New List(Of String)
		Dim rowAttributes As String = ""
		Dim columnAttributes As New List(Of String)
		Dim cellAttributes As New List(Of String)
		Dim containerList As ArrayList = KaReports.GetContainerTable(connection, mediaType, "deleted=0", "number ASC", -1, -1, report.ReportDomainURL, columnsDisplayed, tableAttributes, headerRowAttributes, headerCellAttributes, rowAttributes, columnAttributes, cellAttributes)

		If mediaType = KaReports.MEDIA_TYPE_COMMA Then
			email.Body = "See attached report"
			email.BodyIsHtml = False
			attachments.Add(New Attachment(New MemoryStream(Encoding.UTF8.GetBytes(KaReports.GetTableCsv("", "", containerList))), "report.csv", MediaTypeNames.Text.Plain))
		Else
			email.Body = CreateSiteCssStyle(SITE_CSS_FILENAME) & KaReports.GetTableHtml("", "", containerList, False, tableAttributes, headerRowAttributes, headerCellAttributes, rowAttributes, columnAttributes, cellAttributes, True)
			email.BodyIsHtml = True
			attachments.Add(New Attachment(New MemoryStream(Encoding.UTF8.GetBytes(email.Body)), "report.html", MediaTypeNames.Text.Html))
		End If
	End Sub

	Private Sub GetCustomerActivityReport(connection As OleDbConnection, report As KaEmailReport, email As KaEmail, attachments As List(Of Attachment), ByVal endDate As DateTime)
		Dim bogusId As Guid = Guid.NewGuid
		Dim ownerId As Guid = Guid.Parse(GetParameter("owner_id", report.ReportParameters, bogusId.ToString()))
		If ownerId = bogusId Then
#Disable Warning BC40000
			'For backwards compatibility
			ownerId = report.OwnerId
#Enable Warning BC40000 ' Type or member is obsolete
		End If
		Dim mediaType As String = GetParameter("format", report.ReportParameters, KaReports.MEDIA_TYPE_HTML)
		Dim table As HtmlTable
		If mediaType = KaReports.MEDIA_TYPE_COMMA Then
			table = Nothing
		Else
			table = New HtmlTable()
		End If
		Dim applicatorId As Guid = Guid.Parse(GetParameter("applicator_id", report.ReportParameters, Guid.Empty.ToString()))
		Dim branchId As Guid = Guid.Parse(GetParameter("branch_id", report.ReportParameters, Guid.Empty.ToString()))
		Dim customerAccountId As Guid = Guid.Parse(GetParameter("customer_account_id", report.ReportParameters, Guid.Empty.ToString()))
		Dim customerAccountLocationId As Guid = Guid.Parse(GetParameter("customer_account_location_id", report.ReportParameters, Guid.Empty.ToString()))
		Dim productId As Guid = Guid.Parse(GetParameter("product_id", report.ReportParameters, Guid.Empty.ToString()))
		Dim locationId As Guid = Guid.Parse(GetParameter("location_id", report.ReportParameters, Guid.Empty.ToString()))
		Dim bayId As Guid = Guid.Parse(GetParameter("bay_id", report.ReportParameters, Guid.Empty.ToString()))
		Dim driverId As Guid = Guid.Parse(GetParameter("driver_id", report.ReportParameters, Guid.Empty.ToString()))
		Dim transportId As Guid = Guid.Parse(GetParameter("transport_id", report.ReportParameters, Guid.Empty.ToString()))
		Dim carrierId As Guid = Guid.Parse(GetParameter("carrier_id", report.ReportParameters, Guid.Empty.ToString()))
		Dim unitId As Guid = Guid.Parse(GetParameter("unit_id", report.ReportParameters, Guid.Empty.ToString()))
		Dim userName As String = GetParameter("username", report.ReportParameters, "")
		Dim interfaceId As String = GetParameter("interface_id", report.ReportParameters, "-1")
		Dim sortBy As String = GetParameter("sort_by", report.ReportParameters, "tickets.loaded_at")
		'   Dim showBulkProducts As Boolean = Boolean.Parse(GetParameter("show_bulk_products", report.ReportParameters, "False"))
		Dim columns As ULong = ULong.Parse(GetParameter("columns", report.ReportParameters, Long.MaxValue.ToString()))
		Dim unitInfo As New KaUnit(connection, unitId)
		Dim precision As String = unitInfo.UnitPrecision
		Dim decimalCount As Integer = 0
		If precision.IndexOf(".") >= 0 Then decimalCount = Math.Max(0, precision.Length - precision.IndexOf(".") - 1)
		If Integer.TryParse(GetParameter("number_of_digits_after_decimal", report.ReportParameters, decimalCount.ToString()), decimalCount) Then
			' Set the unit's formatting
			unitInfo.UnitPrecision = "#,###,###0" & IIf(decimalCount > 0, ".".PadRight(decimalCount + 1, "0"), "")
		End If

		Dim tempguid As String = Guid.NewGuid.ToString()
		Dim totalUnitsAndDecimals As String = GetParameter("total_units_and_decimals", report.ReportParameters, tempguid)
		If totalUnitsAndDecimals = tempguid Then
			'Look up original values
			totalUnitsAndDecimals = ""
			If ((columns And 2 ^ KaReports.CustomerActivityReportColumns.RcTicketTotalQuantity) <> 0) Then
				Try
					Dim totalUnitInfo As New KaUnit(connection, Guid.Parse(GetParameter("total_unit_id", report.ReportParameters, Guid.Empty.ToString())))
					Dim totalDecimalsDisplayed As Integer = 0
					If Integer.TryParse(GetParameter("total_number_of_digits_after_decimal", report.ReportParameters, totalDecimalsDisplayed.ToString()), totalDecimalsDisplayed) Then
						' Set the unit's formatting
						totalUnitInfo.UnitPrecision = "#,###,###0" & IIf(totalDecimalsDisplayed > 0, ".".PadRight(totalDecimalsDisplayed + 1, "0"), "")
					End If
					totalUnitsAndDecimals = totalUnitInfo.Id.ToString() & ":" & totalDecimalsDisplayed.ToString() & ":true"
				Catch ex As RecordNotFoundException

				End Try
			End If
		End If

		Dim totalUnits As New List(Of KaUnit)
		For Each unitItem As String In totalUnitsAndDecimals.Split("|")
			Dim values() As String = unitItem.Split(":")
			Try
				Dim totalUnit As New KaUnit(connection, Guid.Parse(values(0)))
				Dim decimalsDisplayed As Integer = 0
				If Integer.TryParse(values(1), decimalsDisplayed) Then
					totalUnit.UnitPrecision = "#,###,###0" & IIf(decimalsDisplayed > 0, ".".PadRight(decimalsDisplayed + 1, "0"), "")
				End If
				Dim includeUnit As Boolean = values.Length < 3
				If values.Length > 2 Then Boolean.TryParse(values(2), includeUnit)
				If includeUnit AndAlso Not totalUnits.Contains(totalUnit) Then totalUnits.Add(totalUnit)
			Catch ex As Exception

			End Try
		Next

		Dim productDisplay As KaReports.CustomerActivityReportProductDisplayOptions = CustomerActivityReportProductDisplayOptions.ProductAsColumn

		Try
			Integer.TryParse(GetParameter("product_display_options", report.ReportParameters, IIf(Not Boolean.Parse(GetParameter("show_bulk_products", report.ReportParameters, "false")), "0", "1")), productDisplay)
		Catch ex As Exception
		End Try
		Dim includeVoidedTickets As Boolean = False
		Boolean.TryParse(GetParameter("include_voided_tickets", report.ReportParameters, False), includeVoidedTickets)
		Dim ticketNumberShown As Boolean = (columns And 2 ^ CustomerActivityReportColumns.RcTicketNumber)

		Dim query As String = "SELECT tickets.id, " & IIf(productDisplay = CustomerActivityReportProductDisplayOptions.BulkProductAsColumn Or productDisplay = CustomerActivityReportProductDisplayOptions.ProductAsRowBulkProductsAsColumn, "ticket_bulk_items.bulk_product_id", "ticket_items.product_id") & " " &
			"FROM tickets, ticket_items, ticket_bulk_items" &
				IIf(customerAccountId.Equals(Guid.Empty), "", ", ticket_customer_accounts") &
				IIf(transportId.Equals(Guid.Empty), "", ", ticket_transports") &
				IIf(ownerId.Equals(Guid.Empty), "", ", orders") & " " &
			"WHERE tickets.internal_transfer=0 AND ticket_bulk_items.ticket_item_id=ticket_items.id AND " &
				"ticket_items.ticket_id=tickets.id AND " &
				"tickets.loaded_at>=" & Q(report.LastSent) & " AND " &
				"tickets.loaded_at<=" & Q(endDate) &
				IIf(branchId.Equals(Guid.Empty), "", " AND tickets.branch_id=" & Q(branchId)) &
				IIf(customerAccountId.Equals(Guid.Empty), "", " AND tickets.id=ticket_customer_accounts.ticket_id AND ticket_customer_accounts.customer_account_id='" & customerAccountId.ToString() & "'") &
				IIf(customerAccountLocationId.Equals(Guid.Empty), "", " AND tickets.customer_account_location_id=" & Q(customerAccountLocationId)) &
				IIf(productId.Equals(Guid.Empty), "", " AND tickets.id=ticket_items.ticket_id AND ticket_items.product_id='" & productId.ToString() & "'") &
				IIf(ownerId.Equals(Guid.Empty), "", " AND tickets.order_id=orders.id AND orders.owner_id='" & ownerId.ToString() & "'") &
				IIf(locationId.Equals(Guid.Empty), "", " AND tickets.location_id='" & locationId.ToString() & "'") &
				IIf(bayId.Equals(Guid.Empty), "", " AND ticket_bulk_items.bay_id=" & Q(bayId)) &
				IIf(driverId.Equals(Guid.Empty), "", " AND tickets.driver_id='" & driverId.ToString() & "'") &
				IIf(transportId.Equals(Guid.Empty), "", " AND tickets.id=ticket_transports.ticket_id AND ticket_transports.transport_id='" & transportId.ToString() & "'") &
				IIf(carrierId.Equals(Guid.Empty), "", " AND tickets.carrier_id='" & carrierId.ToString() & "'") & " " &
				IIf(applicatorId.Equals(Guid.Empty), "", " AND tickets.applicator_id='" & applicatorId.ToString() & "'") & " " &
				IIf(userName.Trim.Length > 0, " AND tickets.username=" & Q(userName.Trim), "") &
				IIf(includeVoidedTickets AndAlso ticketNumberShown, "", " AND tickets.voided=0 ") &
				IIf(interfaceId.Equals("-1"), "", " AND tickets.interface_id=" & Q(interfaceId)) &
			"ORDER BY " & sortBy & " ASC"
		Dim tableDisplayAttributes As String = "border=""1"""
		Dim tableRowDisplayAttributes As String = ""
		Dim tableDetailDisplayAttributes As String = ""

		Dim reportDomainURL As String = ""
		If ConvertWebPageUrlDomainToRequestedPagesDomain(Tm2Database.Connection) Then reportDomainURL = report.ReportDomainURL

		Dim returnTable As String = GetCustomerActivityTable(connection, mediaType, query, productDisplay, email.Subject, columns, unitInfo, tableDisplayAttributes, tableRowDisplayAttributes, tableRowDisplayAttributes, totalUnits, reportDomainURL, True, True, False)

		If mediaType = KaReports.MEDIA_TYPE_COMMA Then
			email.Body = "See attached report"
			email.BodyIsHtml = False
			attachments.Add(New Attachment(New MemoryStream(Encoding.UTF8.GetBytes(returnTable)), "report.csv", MediaTypeNames.Text.Plain))
		Else
			email.Body = CreateSiteCssStyle(STYLE_CSS_FILENAME) & returnTable
			email.BodyIsHtml = True
			attachments.Add(New Attachment(New MemoryStream(Encoding.UTF8.GetBytes(email.Body)), "report.html", MediaTypeNames.Text.Html))
		End If
	End Sub

	Private Sub GetDriverListReport(connection As OleDbConnection, report As KaEmailReport, email As KaEmail, attachments As List(Of Attachment))
		Dim mediaType As String = GetParameter("format", report.ReportParameters, KaReports.MEDIA_TYPE_HTML)
		Dim table As ArrayList = KaReports.GetDriverTable(connection)
		If mediaType = KaReports.MEDIA_TYPE_COMMA Then
			email.Body = "See attached report"
			email.BodyIsHtml = False
			attachments.Add(New Attachment(New MemoryStream(Encoding.UTF8.GetBytes(KaReports.GetTableCsv(email.Subject, "", table))), "report.csv", MediaTypeNames.Text.Plain))
		Else
			email.Body = CreateSiteCssStyle(SITE_CSS_FILENAME) & KaReports.GetTableHtml(email.Subject, table)
			email.BodyIsHtml = True
			attachments.Add(New Attachment(New MemoryStream(Encoding.UTF8.GetBytes(email.Body)), "report.html", MediaTypeNames.Text.Html))
		End If
	End Sub

	Private Sub GetOrderListReport(connection As OleDbConnection, report As KaEmailReport, email As KaEmail, attachments As List(Of Attachment))
		Dim bogusId As Guid = Guid.NewGuid
		Dim ownerId As Guid = Guid.Parse(GetParameter("owner_id", report.ReportParameters, bogusId.ToString()))
		If ownerId = bogusId Then
#Disable Warning BC40000
			'For backwards compatibility
			ownerId = report.OwnerId
#Enable Warning BC40000 ' Type or member is obsolete
		End If
		Dim orderBy As String = GetParameter("order_list_order_by", report.ReportParameters, "orders.number")
		Dim ascDesc As String = GetParameter("order_list_asc_desc", report.ReportParameters, "asc")
		Dim customerId As Guid = Guid.Parse(GetParameter("customer_account_id", report.ReportParameters, Guid.Empty.ToString()))
		Dim locationId As Guid = Guid.Parse(GetParameter("location_id", report.ReportParameters, Guid.Empty.ToString()))
		Dim reportType As KaReports.OrderListReportType
		Select Case GetParameter("order_list_report_type", report.ReportParameters, KaReports.OrderListReportType.MultipleProductsOneColumn.ToString)
			Case KaReports.OrderListReportType.OneProductPerLine.ToString
				reportType = OrderListReportType.OneProductPerLine
			Case KaReports.OrderListReportType.MultipleProductsOneColumn.ToString
				reportType = OrderListReportType.MultipleProductsOneColumn
			Case Else
				reportType = OrderListReportType.OneProductPerColumn
		End Select

		Dim mediaType As String = GetParameter("format", report.ReportParameters, KaReports.MEDIA_TYPE_HTML)
		Dim table As HtmlTable = New HtmlTable
		Dim reportDataList As ArrayList = KaReports.GetOrdersTable(connection, ownerId, mediaType, orderBy & " " & ascDesc, customerId, locationId, reportType)
		Dim reportTitle As String = ""
		Try
			reportTitle &= IIf(reportTitle.Length > 0, ", ", "") & "Facility: " & New KaLocation(connection, locationId).Name
		Catch ex As Exception

		End Try
		Try
			reportTitle &= IIf(reportTitle.Length > 0, ", ", "") & "Owner: " & New KaOwner(connection, ownerId).Name
		Catch ex As Exception

		End Try
		Try
			reportTitle &= IIf(reportTitle.Length > 0, ", ", "") & "Account: " & New KaCustomerAccount(connection, customerId).Name
		Catch ex As Exception

		End Try
		If reportTitle.Length > 0 Then reportTitle = "For " & reportTitle
		If mediaType = KaReports.MEDIA_TYPE_COMMA Then
			Dim csv As String = KaReports.GetTableCsv("Order List", reportTitle, reportDataList)
			email.Body = "See attached report"
			email.BodyIsHtml = False
			attachments.Add(New Attachment(New MemoryStream(Encoding.UTF8.GetBytes(csv)), "report.csv", MediaTypeNames.Text.Plain))
		Else
			Dim writer As New StringWriter()
			Dim htmlWriter As New HtmlTextWriter(writer)
			table.RenderControl(htmlWriter)
			Dim headerRowList As ArrayList = reportDataList(0) 'This will be the header row
			Dim tableAttributes As String = "border=1; width=100%;"
			Dim headerRowAttributes As String = ""
			Dim rowAttributes As String = ""

			Dim headerCellAttributeList As New List(Of String)
			Dim detailCellAttributeList As New List(Of String)
			KaReports.GetOrderListHtmlTableFormatting(tableAttributes, headerRowAttributes, rowAttributes, headerCellAttributeList, detailCellAttributeList, headerRowList)

			email.Body = CreateSiteCssStyle(SITE_CSS_FILENAME) & KaReports.GetTableHtml("Order List", reportTitle, reportDataList, False, tableAttributes, headerRowAttributes, headerCellAttributeList, rowAttributes, detailCellAttributeList)
			email.BodyIsHtml = True
			attachments.Add(New Attachment(New MemoryStream(Encoding.UTF8.GetBytes(email.Body)), "report.html", MediaTypeNames.Text.Html))
		End If
	End Sub

	Private Sub GetReceivingPurchaseOrderListReport(connection As OleDbConnection, report As KaEmailReport, email As KaEmail, attachments As List(Of Attachment))
		Dim bogusId As Guid = Guid.NewGuid
		Dim ownerId As Guid = Guid.Parse(GetParameter("owner_id", report.ReportParameters, bogusId.ToString()))
		If ownerId = bogusId Then
#Disable Warning BC40000
			'For backwards compatibility
			ownerId = report.OwnerId
#Enable Warning BC40000 ' Type or member is obsolete
		End If
		Dim orderBy As String = GetParameter("receiving_purchase_order_list_order_by", report.ReportParameters, "purchase_order_number")
		Dim ascDesc As String = GetParameter("receiving_purchase_order_list_asc_desc", report.ReportParameters, "asc")
		Dim locationId As Guid = Guid.Parse(GetParameter("location_id", report.ReportParameters, Guid.Empty.ToString()))
		Dim supplierId As Guid = Guid.Parse(GetParameter("supplier_account_id", report.ReportParameters, Guid.Empty.ToString()))
		Dim bulkProductId As Guid = Guid.Parse(GetParameter("bulk_product_id", report.ReportParameters, Guid.Empty.ToString()))

		Dim reportDomainURL As String = ""
		If ConvertWebPageUrlDomainToRequestedPagesDomain(Tm2Database.Connection) Then reportDomainURL = report.ReportDomainURL

		Dim mediaType As String = GetParameter("format", report.ReportParameters, KaReports.MEDIA_TYPE_HTML)
		Dim table As HtmlTable = New HtmlTable
		Dim reportDataList As ArrayList = KaReports.GetReceivingPurchaseOrdersListTable(connection, ownerId, mediaType, orderBy & " " & ascDesc, supplierId, bulkProductId, reportDomainURL, True, locationId)
		Dim reportTitle As String = ""
		Try
			reportTitle &= IIf(reportTitle.Length > 0, ", ", "") & "Owner: " & New KaOwner(connection, ownerId).Name
		Catch ex As Exception

		End Try
		Try
			reportTitle &= IIf(reportTitle.Length > 0, ", ", "") & "Supplier: " & New KaCustomerAccount(connection, supplierId).Name
		Catch ex As Exception

		End Try
		If reportTitle.Length > 0 Then reportTitle = "For " & reportTitle
		If mediaType = KaReports.MEDIA_TYPE_COMMA Then
			Dim csv As String = KaReports.GetTableCsv("Receiving Purchase Order List", reportTitle, reportDataList)
			email.Body = "See attached report"
			email.BodyIsHtml = False
			attachments.Add(New Attachment(New MemoryStream(Encoding.UTF8.GetBytes(csv)), "report.csv", MediaTypeNames.Text.Plain))
		Else
			Dim writer As New StringWriter()
			Dim htmlWriter As New HtmlTextWriter(writer)
			table.RenderControl(htmlWriter)
			Dim headerRowList As ArrayList = reportDataList(0) 'This will be the header row
			Dim tableAttributes As String = "border=1; width=100%;"
			Dim headerRowAttributes As String = ""
			Dim rowAttributes As String = ""

			Dim headerCellAttributeList As New List(Of String)
			Dim detailCellAttributeList As New List(Of String)
			KaReports.GetReceivingPurchaseOrderListHtmlTableFormatting(tableAttributes, headerRowAttributes, rowAttributes, headerCellAttributeList, detailCellAttributeList, headerRowList)

			email.Body = CreateSiteCssStyle(SITE_CSS_FILENAME) & KaReports.GetTableHtml("Receiving Purhcase Order List", reportTitle, reportDataList, False, tableAttributes, headerRowAttributes, headerCellAttributeList, rowAttributes, detailCellAttributeList)
			email.BodyIsHtml = True
			attachments.Add(New Attachment(New MemoryStream(Encoding.UTF8.GetBytes(email.Body)), "report.html", MediaTypeNames.Text.Html))
		End If
	End Sub

	Private Sub GetProductAllocationReport(connection As OleDbConnection, report As KaEmailReport, email As KaEmail, attachments As List(Of Attachment))
		Dim mediaType As String = GetParameter("format", report.ReportParameters, KaReports.MEDIA_TYPE_HTML)
		Dim customerAccountId As Guid = Guid.Parse(GetParameter("customer_account_id", report.ReportParameters, Guid.Empty.ToString()))
		Dim productId As Guid = Guid.Parse(GetParameter("product_id", report.ReportParameters, Guid.Empty.ToString()))
		Dim unitId As Guid = Guid.Parse(GetParameter("unit_id", report.ReportParameters, Guid.Empty.ToString()))
		Dim locationId As Guid = Guid.Parse(GetParameter("location_id", report.ReportParameters, Guid.Empty.ToString()))
		Dim onlyShowProductsWithBulkProductsAtLocation As Boolean = False
		Boolean.TryParse(GetParameter("show_prods_with_formula", report.ReportParameters, False), onlyShowProductsWithBulkProductsAtLocation)

		Dim table As ArrayList = KaReports.GetProductAllocationTable(connection, customerAccountId, productId, unitId, locationId, onlyShowProductsWithBulkProductsAtLocation)
		If mediaType = KaReports.MEDIA_TYPE_COMMA Then
			email.Body = "See attached report"
			email.BodyIsHtml = False
			attachments.Add(New Attachment(New MemoryStream(Encoding.UTF8.GetBytes(KaReports.GetTableCsv(email.Subject, "", table))), "report.csv", MediaTypeNames.Text.Plain))
		Else
			email.Body = CreateSiteCssStyle(SITE_CSS_FILENAME) & KaReports.GetTableHtml(email.Subject, table)
			email.BodyIsHtml = True
			attachments.Add(New Attachment(New MemoryStream(Encoding.UTF8.GetBytes(email.Body)), "report.html", MediaTypeNames.Text.Html))
		End If
	End Sub

	Private Sub GetProductListReport(connection As OleDbConnection, report As KaEmailReport, email As KaEmail, attachments As List(Of Attachment))
		Dim bogusId As Guid = Guid.NewGuid
		Dim ownerId As Guid = Guid.Parse(GetParameter("owner_id", report.ReportParameters, bogusId.ToString()))
		If ownerId = bogusId Then
#Disable Warning BC40000
			'For backwards compatibility
			ownerId = report.OwnerId
#Enable Warning BC40000 ' Type or member is obsolete
		End If
		Dim locationId As Guid = Guid.Parse(GetParameter("location_id", report.ReportParameters, Guid.Empty.ToString()))
		email.Body = CreateSiteCssStyle(SITE_CSS_FILENAME) & KaReports.GetProductListReportHtml(connection, ownerId, locationId)
		email.BodyIsHtml = True
		attachments.Add(New Attachment(New MemoryStream(Encoding.UTF8.GetBytes(email.Body)), "report.html", MediaTypeNames.Text.Html))
	End Sub

	Private Sub GetReceivingActivityReport(connection As OleDbConnection, report As KaEmailReport, email As KaEmail, attachments As List(Of Attachment), ByVal endDate As DateTime)
		Dim bogusId As Guid = Guid.NewGuid
		Dim ownerId As Guid = Guid.Parse(GetParameter("owner_id", report.ReportParameters, bogusId.ToString()))
		If ownerId = bogusId Then
#Disable Warning BC40000
			'For backwards compatibility
			ownerId = report.OwnerId
#Enable Warning BC40000 ' Type or member is obsolete
		End If
		Dim mediaType As String = GetParameter("format", report.ReportParameters, KaReports.MEDIA_TYPE_HTML)
		Dim table As HtmlTable
		If mediaType = KaReports.MEDIA_TYPE_COMMA Then
			table = Nothing
		Else
			table = New HtmlTable()
		End If
		Dim supplierAccountId As Guid = Guid.Parse(GetParameter("supplier_account_id", report.ReportParameters, Guid.Empty.ToString()))
		Dim bulkProductId As Guid = Guid.Parse(GetParameter("bulk_product_id", report.ReportParameters, Guid.Empty.ToString()))
		Dim locationId As Guid = Guid.Parse(GetParameter("location_id", report.ReportParameters, Guid.Empty.ToString()))
		Dim driverId As Guid = Guid.Parse(GetParameter("driver_id", report.ReportParameters, Guid.Empty.ToString()))
		Dim transportId As Guid = Guid.Parse(GetParameter("transport_id", report.ReportParameters, Guid.Empty.ToString()))
		Dim carrierId As Guid = Guid.Parse(GetParameter("carrier_id", report.ReportParameters, Guid.Empty.ToString()))
		Dim unitId As Guid = Guid.Parse(GetParameter("unit_id", report.ReportParameters, Guid.Empty.ToString()))
		Dim sortBy As String = GetParameter("sort_by", report.ReportParameters, "tickets.loaded_at")
		Dim columns As ULong = ULong.Parse(GetParameter("columns", report.ReportParameters, Long.MaxValue.ToString()))
		Dim unitInfo As New KaUnit(connection, unitId)
		Dim precision As String = unitInfo.UnitPrecision
		Dim decimalCount As Integer = 0
		If precision.IndexOf(".") >= 0 Then decimalCount = Math.Max(0, precision.Length - precision.IndexOf(".") - 1)
		If Integer.TryParse(GetParameter("number_of_digits_after_decimal", report.ReportParameters, decimalCount.ToString()), decimalCount) Then
			' Set the unit's formatting
			unitInfo.UnitPrecision = "#,###,###0" & IIf(decimalCount > 0, ".".PadRight(decimalCount + 1, "0"), "")
		End If

		Dim tempguid As String = Guid.NewGuid.ToString()
		Dim totalUnitsAndDecimals As String = GetParameter("total_units_and_decimals", report.ReportParameters, tempguid)
		If totalUnitsAndDecimals = tempguid Then
			'Look up original values
			totalUnitsAndDecimals = ""
			If ((columns And 2 ^ KaReports.ReceivingActivityReportColumns.RcTicketTotalQuantity) <> 0) Then
				totalUnitsAndDecimals = unitInfo.Id.ToString() & ":" & decimalCount.ToString() & ":true"
			End If
		End If

		Dim totalUnits As New List(Of KaUnit)
		For Each unitItem As String In totalUnitsAndDecimals.Split("|")
			Dim values() As String = unitItem.Split(":")
			Try
				Dim totalUnit As New KaUnit(connection, Guid.Parse(values(0)))
				Dim decimalsDisplayed As Integer = 0
				If Integer.TryParse(values(1), decimalsDisplayed) Then
					totalUnit.UnitPrecision = "#,###,###0" & IIf(decimalsDisplayed > 0, ".".PadRight(decimalsDisplayed + 1, "0"), "")
				End If
				Dim includeUnit As Boolean = values.Length < 3
				If values.Length > 2 Then Boolean.TryParse(values(2), includeUnit)
				If includeUnit AndAlso Not totalUnits.Contains(totalUnit) Then totalUnits.Add(totalUnit)
			Catch ex As Exception

			End Try
		Next

		Dim includeVoidedTickets As Boolean = False
		Boolean.TryParse(GetParameter("include_voided_tickets", report.ReportParameters, False), includeVoidedTickets)
		Dim ticketNumberShown As Boolean = (columns And 2 ^ ReceivingActivityReportColumns.RcTicketNumber)

		Dim query As String = "SELECT receiving_tickets.id, receiving_tickets.bulk_product_id " &
				"FROM receiving_tickets " &
				"WHERE receiving_tickets.date_of_delivery>=" & Q(report.LastSent) &
					" AND receiving_tickets.date_of_delivery<=" & Q(endDate) &
					 IIf(supplierAccountId.Equals(Guid.Empty), "", " AND receiving_tickets.supplier_account_id=" & Q(supplierAccountId)) &
					 IIf(bulkProductId.Equals(Guid.Empty), "", " AND receiving_tickets.bulk_product_id=" & Q(bulkProductId)) &
					 IIf(ownerId.Equals(Guid.Empty), "", " AND receiving_tickets.owner_id=" & Q(ownerId)) &
					 IIf(locationId.Equals(Guid.Empty), "", " AND receiving_tickets.location_id=" & Q(locationId)) &
					 IIf(driverId.Equals(Guid.Empty), "", " AND receiving_tickets.driver_id=" & Q(driverId)) &
					 IIf(transportId.Equals(Guid.Empty), "", " AND receiving_tickets.transport_id=" & Q(transportId)) &
					 IIf(carrierId.Equals(Guid.Empty), "", " AND receiving_tickets.carrier_id=" & Q(carrierId)) &
					 IIf(includeVoidedTickets AndAlso ticketNumberShown, "", " AND receiving_tickets.voided=0 ") &
				" ORDER BY " & sortBy & " ASC"
		Dim tableDisplayAttributes As String = "border=""1"""
		Dim tableRowDisplayAttributes As String = ""
		Dim tableDetailDisplayAttributes As String = ""

		Dim reportDomainURL As String = ""
		If ConvertWebPageUrlDomainToRequestedPagesDomain(Tm2Database.Connection) Then reportDomainURL = report.ReportDomainURL

		Dim returnTable As String = KaReports.GetReceivingActivityTable(connection, mediaType, query, email.Subject, columns, unitInfo, tableDisplayAttributes, tableRowDisplayAttributes, tableRowDisplayAttributes, totalUnits, reportDomainURL, True, True, False)

		If mediaType = KaReports.MEDIA_TYPE_COMMA Then
			email.Body = "See attached report"
			email.BodyIsHtml = False
			attachments.Add(New Attachment(New MemoryStream(Encoding.UTF8.GetBytes(returnTable)), "report.csv", MediaTypeNames.Text.Plain))
		Else
			email.Body = CreateSiteCssStyle(STYLE_CSS_FILENAME) & returnTable
			email.BodyIsHtml = True
			attachments.Add(New Attachment(New MemoryStream(Encoding.UTF8.GetBytes(email.Body)), "report.html", MediaTypeNames.Text.Html))
		End If
	End Sub

	Private Sub GetTankLevelTrendReport(connection As OleDbConnection, report As KaEmailReport, email As KaEmail, attachments As List(Of Attachment), ByVal endDate As DateTime)
		Dim tankLevelTrendId As Guid = Guid.Parse(GetParameter("tank_level_trend_id", report.ReportParameters, Guid.Empty.ToString()))
		Dim displayUnitId As Guid = Guid.Parse(GetParameter("display_unit_id", report.ReportParameters, Guid.Empty.ToString()))
		Dim showTemperature As Boolean = False
		Boolean.TryParse(GetParameter("show_temperature", report.ReportParameters, Guid.Empty.ToString()), showTemperature)
		Dim mediaType As String = GetParameter("format", report.ReportParameters, KaReports.MEDIA_TYPE_HTML)
		If mediaType = KaReports.MEDIA_TYPE_COMMA Then
			email.Body = "See attached report"
			email.BodyIsHtml = False
			attachments.Add(New Attachment(New MemoryStream(Encoding.UTF8.GetBytes(KaReports.GetTableCsv(email.Subject, "", KaReports.GetTankLevelTrendTable(connection, tankLevelTrendId, report.LastSent, endDate, True, displayUnitId, showTemperature)))), "report.csv", MediaTypeNames.Text.Plain))
		Else
			Dim levelFillColor As System.Drawing.Color = System.Drawing.Color.FromArgb(&H99, &HCC, &HFF)
			Dim temperatureFillColor As System.Drawing.Color = System.Drawing.Color.FromArgb(0, 51, 151)
			email.Body = CreateSiteCssStyle(SITE_CSS_FILENAME) & $"<img alt=""Picture"" style=""width:100%; height:auto;"" src=""data:image/ png;base64, {ConvertImageToData(KaReports.GetTankLevelTrendGraph(connection, tankLevelTrendId, report.LastSent, endDate, 4.5, displayUnitId, showTemperature, levelFillColor, temperatureFillColor))}"" />" &
				"<br />" &
				KaReports.GetTableHtml(email.Subject, KaReports.GetTankLevelTrendTable(connection, tankLevelTrendId, report.LastSent, endDate, False, displayUnitId, showTemperature))
			email.BodyIsHtml = True
			attachments.Add(New Attachment(New MemoryStream(Encoding.UTF8.GetBytes(email.Body)), "report.html", MediaTypeNames.Text.Html))
		End If
	End Sub

	Private Sub GetTransportListReport(connection As OleDbConnection, report As KaEmailReport, email As KaEmail, attachments As List(Of Attachment))
		Dim mediaType As String = GetParameter("format", report.ReportParameters, KaReports.MEDIA_TYPE_HTML)
		Dim table As ArrayList = KaReports.GetTransportTable(connection)
		If mediaType = KaReports.MEDIA_TYPE_COMMA Then
			email.Body = "See attached report"
			email.BodyIsHtml = False
			attachments.Add(New Attachment(New MemoryStream(Encoding.UTF8.GetBytes(KaReports.GetTableCsv(email.Subject, "", table))), "report.csv", MediaTypeNames.Text.Plain))
		Else
			email.Body = CreateSiteCssStyle(SITE_CSS_FILENAME) & KaReports.GetTableHtml(email.Subject, table)
			email.BodyIsHtml = True
			attachments.Add(New Attachment(New MemoryStream(Encoding.UTF8.GetBytes(email.Body)), "report.html", MediaTypeNames.Text.Html))
		End If
	End Sub

	Private Sub GetTransportUsageReport(connection As OleDbConnection, report As KaEmailReport, email As KaEmail, attachments As List(Of Attachment), ByVal endDate As DateTime)
		Dim customerAccountId As Guid = Guid.Parse(GetParameter("customer_account_id", report.ReportParameters, Guid.Empty.ToString()))
		Dim transportId As Guid = Guid.Parse(GetParameter("transport_id", report.ReportParameters, Guid.Empty.ToString()))
		Dim table As ArrayList = KaReports.GetTransportUsageReportTable(connection, report.LastSent, endDate, customerAccountId, transportId)
		Dim mediaType As String = GetParameter("format", report.ReportParameters, KaReports.MEDIA_TYPE_HTML)
		If mediaType = KaReports.MEDIA_TYPE_COMMA Then
			email.Body = "See attached report"
			email.BodyIsHtml = False
			attachments.Add(New Attachment(New MemoryStream(Encoding.UTF8.GetBytes(KaReports.GetTableCsv(email.Subject, "", table))), "report.csv", MediaTypeNames.Text.Plain))
		Else
			email.Body = CreateSiteCssStyle(SITE_CSS_FILENAME) & KaReports.GetTableHtml(email.Subject, table)
			email.BodyIsHtml = True
			attachments.Add(New Attachment(New MemoryStream(Encoding.UTF8.GetBytes(email.Body)), "report.html", MediaTypeNames.Text.Html))
		End If
	End Sub

	Private Sub GetTransportTrackingReport(connection As OleDbConnection, report As KaEmailReport, email As KaEmail, attachments As List(Of Attachment))
		Dim orderBy As String = GetParameter("transport_tracking_order_by", report.ReportParameters, "name")
		Dim ascDesc As String = GetParameter("transport_tracking_asc_desc", report.ReportParameters, "asc")
		Dim displayUnitId As Guid = Guid.Parse(GetParameter("display_unit_id", report.ReportParameters, Guid.Empty.ToString))

		Dim mediaType As String = GetParameter("format", report.ReportParameters, KaReports.MEDIA_TYPE_HTML)
		Dim table As HtmlTable = New HtmlTable
		If mediaType = KaReports.MEDIA_TYPE_COMMA Then
			Dim csv As String = KaReports.GetTransportTrackingReport(connection, table, mediaType, orderBy & " " & ascDesc, displayUnitId)
			email.Body = "See attached report"
			email.BodyIsHtml = False
			attachments.Add(New Attachment(New MemoryStream(Encoding.UTF8.GetBytes(csv)), "report.csv", MediaTypeNames.Text.Plain))
		Else
			KaReports.GetTransportTrackingReport(connection, table, mediaType, orderBy & " " & ascDesc, displayUnitId)
			Dim writer As New StringWriter()
			Dim htmlWriter As New HtmlTextWriter(writer)
			table.RenderControl(htmlWriter)
			email.Body = CreateSiteCssStyle(SITE_CSS_FILENAME) & writer.ToString()
			email.BodyIsHtml = True
			attachments.Add(New Attachment(New MemoryStream(Encoding.UTF8.GetBytes(email.Body)), "report.html", MediaTypeNames.Text.Html))
		End If
	End Sub

	Private Sub GetInventoryReport(connection As OleDbConnection, report As KaEmailReport, email As KaEmail, attachments As List(Of Attachment))
		Dim bogusId As Guid = Guid.NewGuid
		Dim ownerId As Guid = Guid.Parse(GetParameter("owner_id", report.ReportParameters, bogusId.ToString()))
		If ownerId = bogusId Then
#Disable Warning BC40000
			'For backwards compatibility
			ownerId = report.OwnerId
#Enable Warning BC40000 ' Type or member is obsolete
		End If
		Dim locationId As Guid = Guid.Parse(GetParameter("location_id", report.ReportParameters, Guid.Empty.ToString()))
		Dim bulkProductId As Guid = Guid.Parse(GetParameter("bulk_product_id", report.ReportParameters, Guid.Empty.ToString()))
		Dim bulkProductIds As New List(Of Guid)
		If Not bulkProductId.Equals(Guid.Empty) Then bulkProductIds.Add(bulkProductId)
		Dim onlyShowNonZeroBulkProducts As Boolean = Boolean.Parse(GetParameter("only_show_bulk_products_with_non_zero_inventory", report.ReportParameters, False.ToString()))
		Dim assignPhysicalInventoryToOwner As Boolean = False
		Boolean.TryParse(GetParameter("assign_physical_inventory_to_owner", report.ReportParameters, False.ToString()), assignPhysicalInventoryToOwner)
		Dim additionalUnitsString As String = GetParameter("show_additional_units", report.ReportParameters, "")
		Dim additionalUnits As List(Of Guid) = New List(Of Guid)
		If additionalUnitsString.Trim.Length > 0 Then
			For Each unitId As String In additionalUnitsString.Split(",")
				additionalUnits.Add(Guid.Parse(unitId))
			Next
		End If
		Dim mediatype As String = GetParameter("format", report.ReportParameters, KaReports.MEDIA_TYPE_HTML)

		Dim url As String = "http://localhost/TerminalManagement2/InventoryReportView.aspx?OnlyNonzero=" & onlyShowNonZeroBulkProducts & "&AssignPhysicalByOwner=" & assignPhysicalInventoryToOwner
		If ConvertWebPageUrlDomainToRequestedPagesDomain(Tm2Database.Connection) Then url = Tm2Database.GetUrlInCurrentDomain(report.ReportDomainURL, url)

		Dim containerInventoryUrl As String = "http://localhost/TerminalManagement2/ContainerInventoryPFV.aspx"
		If ConvertWebPageUrlDomainToRequestedPagesDomain(Tm2Database.Connection) Then url = Tm2Database.GetUrlInCurrentDomain(report.ReportDomainURL, containerInventoryUrl)

		Dim reportDataList As ArrayList = KaReports.GetInventoryTable(connection, ownerId, locationId, bulkProductIds, onlyShowNonZeroBulkProducts, mediatype, assignPhysicalInventoryToOwner, url, additionalUnits, containerInventoryUrl)
		If mediatype = KaReports.MEDIA_TYPE_COMMA Then
			email.Body = "See attached report"
			email.BodyIsHtml = False
			attachments.Add(New Attachment(New MemoryStream(Encoding.UTF8.GetBytes(KaReports.GetTableCsv(email.Subject, "", reportDataList))), "report.csv", MediaTypeNames.Text.Plain))
		Else
			Dim tableAttributes As String = "border=1; width=100%;"
			Dim headerRowAttributes As String = ""
			Dim rowAttributes As String = ""
			Dim headerRowList As ArrayList = reportDataList(0) 'This will be the header row

			Dim headerCellAttributeList As New List(Of String)
			Dim detailCellAttributeList As New List(Of String)
			Dim columnCount As Integer = 0
			For i = 0 To 2
				headerCellAttributeList.Add("style=""font-weight: bold; text-align: left;""")
				detailCellAttributeList.Add("style=""text-align: left;""")
				columnCount += 1
			Next

			For i = 3 To headerRowList.Count - 1
				headerCellAttributeList.Add("style=""font-weight: bold; text-align: right;""")
				detailCellAttributeList.Add("style=""text-align: right;""")
				columnCount += 1
			Next

			email.Body = CreateSiteCssStyle(SITE_CSS_FILENAME) & KaReports.GetTableHtml("Inventory Report", DateTime.Now.ToString, reportDataList, False, tableAttributes, headerRowAttributes, headerCellAttributeList, rowAttributes, detailCellAttributeList)
			email.BodyIsHtml = True
			attachments.Add(New Attachment(New MemoryStream(Encoding.UTF8.GetBytes(email.Body)), "report.html", MediaTypeNames.Text.Html))
		End If
	End Sub

	Private Sub GetInventoryChangeReport(connection As OleDbConnection, report As KaEmailReport, email As KaEmail, attachments As List(Of Attachment), ByVal endDate As DateTime)
		Dim bogusId As Guid = Guid.NewGuid
		Dim ownerId As Guid = Guid.Parse(GetParameter("owner_id", report.ReportParameters, bogusId.ToString()))
		If ownerId = bogusId Then
#Disable Warning BC40000
			'For backwards compatibility
			ownerId = report.OwnerId
#Enable Warning BC40000 ' Type or member is obsolete
		End If
		Dim locationId As Guid = Guid.Parse(GetParameter("location_id", report.ReportParameters, Guid.Empty.ToString()))
		Dim bulkProductId As Guid = Guid.Parse(GetParameter("bulk_product_id", report.ReportParameters, Guid.Empty.ToString()))
		Dim additionalUnitsString As String = GetParameter("show_additional_units", report.ReportParameters, "")
		Dim additionalUnits As List(Of Guid) = New List(Of Guid)
		If additionalUnitsString.Trim.Length > 0 Then
			For Each unitId As String In additionalUnitsString.Split(",")
				additionalUnits.Add(Guid.Parse(unitId))
			Next
		End If
		Dim mediatype As String = GetParameter("format", report.ReportParameters, KaReports.MEDIA_TYPE_HTML)
		Dim reportDataList As ArrayList = KaReports.GetInventoryChangeTable(connection, ownerId, locationId, bulkProductId, report.LastSent, endDate, additionalUnits, mediatype)
		If mediatype = KaReports.MEDIA_TYPE_COMMA Then
			email.Body = "See attached report"
			email.BodyIsHtml = False
			attachments.Add(New Attachment(New MemoryStream(Encoding.UTF8.GetBytes(KaReports.GetTableCsv(email.Subject, "", reportDataList))), "report.csv", MediaTypeNames.Text.Plain))
		Else
			Dim tableAttributes As String = "border=1; width=100%;"
			Dim headerRowAttributes As String = ""
			Dim rowAttributes As String = ""
			Dim headerRowList As ArrayList = reportDataList(0) 'This will be the header row

			Dim headerCellAttributeList As New List(Of String)
			Dim detailCellAttributeList As New List(Of String)
			Dim columnCount As Integer = 0
			For i = 0 To 2
				headerCellAttributeList.Add("style=""font-weight: bold; text-align: left;""")
				detailCellAttributeList.Add("style=""text-align: left;""")
				columnCount += 1
			Next

			For i = 3 To headerRowList.Count - 1
				headerCellAttributeList.Add("style=""font-weight: bold; text-align: right;""")
				detailCellAttributeList.Add("style=""text-align: right;""")
				columnCount += 1
			Next

			email.Body = CreateSiteCssStyle(SITE_CSS_FILENAME) & KaReports.GetTableHtml("Inventory Change Report", DateTime.Now.ToString, reportDataList, False, tableAttributes, headerRowAttributes, headerCellAttributeList, rowAttributes, detailCellAttributeList)
			email.BodyIsHtml = True
			attachments.Add(New Attachment(New MemoryStream(Encoding.UTF8.GetBytes(email.Body)), "report.html", MediaTypeNames.Text.Html))
		End If
	End Sub

	Private Sub GetTankLevelReport(connection As OleDbConnection, report As KaEmailReport, email As KaEmail, attachments As List(Of Attachment))
		Dim bogusId As Guid = Guid.NewGuid
		Dim ownerId As Guid = Guid.Parse(GetParameter("owner_id", report.ReportParameters, bogusId.ToString()))
		If ownerId = bogusId Then
#Disable Warning BC40000
			'For backwards compatibility
			ownerId = report.OwnerId
#Enable Warning BC40000 ' Type or member is obsolete
		End If
		Dim locationId As Guid = Guid.Parse(GetParameter("location_id", report.ReportParameters, Guid.Empty.ToString()))
		Dim bulkProductId As Guid = Guid.Parse(GetParameter("bulk_product_id", report.ReportParameters, Guid.Empty.ToString()))
		Dim displayUnitId As Guid = Guid.Parse(GetParameter("display_unit_id", report.ReportParameters, Guid.Empty.ToString()))
		Dim mediaType As String = GetParameter("format", report.ReportParameters, KaReports.MEDIA_TYPE_HTML)
		If mediaType = KaReports.MEDIA_TYPE_COMMA Then
			Dim table As HtmlTable = KaReports.GetTankLevelReport(connection, ownerId, locationId, bulkProductId, False, displayUnitId)
			email.Body = "See attached report"
			email.BodyIsHtml = False
			attachments.Add(New Attachment(New MemoryStream(Encoding.UTF8.GetBytes(KaReports.GetTableCsv(email.Subject, "", table))), "tanks.csv", MediaTypeNames.Text.Plain))
			table = KaReports.GetTankGroupLevelReport(connection, ownerId, locationId, bulkProductId, False)
			attachments.Add(New Attachment(New MemoryStream(Encoding.UTF8.GetBytes(KaReports.GetTableCsv(email.Subject, "", table))), "tankGroups.csv", MediaTypeNames.Text.Plain))
		Else
			Dim table As HtmlTable = KaReports.GetTankLevelReport(connection, ownerId, locationId, bulkProductId, mediaType = MEDIA_TYPE_HTML, displayUnitId)
			Dim writer As New StringWriter()
			Dim htmlWriter As New HtmlTextWriter(writer)
			table.RenderControl(htmlWriter)
			Dim html As String = writer.ToString()
			email.Body = CreateSiteCssStyle(SITE_CSS_FILENAME) & html
			email.BodyIsHtml = True
			attachments.Add(New Attachment(New MemoryStream(Encoding.UTF8.GetBytes(html)), "tanks.html", MediaTypeNames.Text.Html))
			table = KaReports.GetTankGroupLevelReport(connection, ownerId, locationId, bulkProductId, True)
			writer = New StringWriter()
			htmlWriter = New HtmlTextWriter(writer)
			table.RenderControl(htmlWriter)
			html = writer.ToString()
			email.Body &= html
			attachments.Add(New Attachment(New MemoryStream(Encoding.UTF8.GetBytes(html)), "tankGroups.html", MediaTypeNames.Text.Html))
		End If
	End Sub

	Private Sub GetTankAlarmReport(connection As OleDbConnection, report As KaEmailReport, email As KaEmail, attachments As List(Of Attachment), ByVal endDate As DateTime)
		Dim bogusId As Guid = Guid.NewGuid
		Dim ownerId As Guid = Guid.Parse(GetParameter("owner_id", report.ReportParameters, bogusId.ToString()))
		If ownerId = bogusId Then
#Disable Warning BC40000
			'For backwards compatibility
			ownerId = report.OwnerId
#Enable Warning BC40000 ' Type or member is obsolete
		End If
		Dim tankId As Guid = Guid.Parse(GetParameter("tank_id", report.ReportParameters, Guid.Empty.ToString()))
		Dim table As HtmlTable = KaReports.GetTankAlarmHistoryReport(connection, ownerId, tankId, report.LastSent, endDate)
		Dim mediaType As String = GetParameter("format", report.ReportParameters, KaReports.MEDIA_TYPE_HTML)
		If mediaType = KaReports.MEDIA_TYPE_COMMA Then
			email.Body = "See attached report"
			email.BodyIsHtml = False
			attachments.Add(New Attachment(New MemoryStream(Encoding.UTF8.GetBytes(KaReports.GetTableCsv(email.Subject, "", table))), "report.csv", MediaTypeNames.Text.Plain))
		Else
			Dim writer As New StringWriter()
			Dim htmlWriter As New HtmlTextWriter(writer)
			table.RenderControl(htmlWriter)
			email.Body = CreateSiteCssStyle(SITE_CSS_FILENAME) & writer.ToString()
			email.BodyIsHtml = True
			attachments.Add(New Attachment(New MemoryStream(Encoding.UTF8.GetBytes(email.Body)), "report.html", MediaTypeNames.Text.Html))
		End If
	End Sub

	Private Sub GetBulkProductUsageReport(connection As OleDbConnection, report As KaEmailReport, email As KaEmail, attachments As List(Of Attachment), ByVal endDate As DateTime)
		Dim bogusId As Guid = Guid.NewGuid
		Dim ownerId As Guid = Guid.Parse(GetParameter("owner_id", report.ReportParameters, bogusId.ToString()))
		If ownerId = bogusId Then
#Disable Warning BC40000
			'For backwards compatibility
			ownerId = report.OwnerId
#Enable Warning BC40000 ' Type or member is obsolete
		End If
		Dim bayId As Guid = Guid.Parse(GetParameter("bay_id", report.ReportParameters, Guid.Empty.ToString()))
		Dim panelId As Guid = Guid.Parse(GetParameter("panel_id", report.ReportParameters, Guid.Empty.ToString()))
		Dim totalUnitsAndDecimals As String = GetParameter("total_units_and_decimals", report.ReportParameters, "")
		Dim totalUnits As New List(Of KaUnit)
		For Each unitItem As String In totalUnitsAndDecimals.Split("|")
			Dim values() As String = unitItem.Split(":")
			Try
				Dim totalUnit As New KaUnit(connection, Guid.Parse(values(0)))
				Dim decimalsDisplayed As Integer = 0
				If Integer.TryParse(values(1), decimalsDisplayed) Then
					totalUnit.UnitPrecision = "#,###,###0" & IIf(decimalsDisplayed > 0, ".".PadRight(decimalsDisplayed + 1, "0"), "")
				End If
				Dim includeUnit As Boolean = values.Length < 3
				If values.Length > 2 Then Boolean.TryParse(values(2), includeUnit)
				If includeUnit AndAlso Not totalUnits.Contains(totalUnit) Then totalUnits.Add(totalUnit)
			Catch ex As Exception

			End Try
		Next

		Dim mediatype As String = GetParameter("format", report.ReportParameters, KaReports.MEDIA_TYPE_HTML)
		Dim bulkProductList As New List(Of Guid) 'ToDo: define bulk products
		Dim allBulkProducts As Boolean = True
		Boolean.TryParse(GetParameter("all_bulk_products", report.ReportParameters, True), allBulkProducts)
		If Not allBulkProducts Then
			Try
				bulkProductList = Tm2Database.FromXml(GetParameter("bulk_product_ids", report.ReportParameters, ""), GetType(List(Of Guid)))
			Catch ex As Exception

			End Try
		End If
		Dim includeVoidedTickets As Boolean = False
		Boolean.TryParse(GetParameter("include_voided_tickets", report.ReportParameters, includeVoidedTickets.ToString()), includeVoidedTickets)
		Dim reportDataList As ArrayList = KaReports.GetBulkProductUsageReport(connection, report.LastSent, endDate, ownerId, panelId, bayId, totalUnits, bulkProductList, mediatype, includeVoidedTickets)
		If mediatype = KaReports.MEDIA_TYPE_COMMA Then
			email.Body = "See attached report"
			email.BodyIsHtml = False
			attachments.Add(New Attachment(New MemoryStream(Encoding.UTF8.GetBytes(KaReports.GetTableCsv(email.Subject, "", reportDataList))), "report.csv", MediaTypeNames.Text.Plain))
		Else
			Dim tableAttributes As String = "border=1; width=100%;"
			Dim headerRowAttributes As String = ""
			Dim rowAttributes As String = ""
			Dim headerRowList As ArrayList = reportDataList(0) 'This will be the header row

			Dim headerCellAttributeList As New List(Of String)
			Dim detailCellAttributeList As New List(Of String)
			Dim columnCount As Integer = 0
			For i = 0 To 2
				headerCellAttributeList.Add("style=""font-weight: bold; text-align: left;""")
				detailCellAttributeList.Add("style=""text-align: left;""")
				columnCount += 1
			Next

			For i = 3 To headerRowList.Count - 1
				headerCellAttributeList.Add("style=""font-weight: bold; text-align: right;""")
				detailCellAttributeList.Add("style=""text-align: right;""")
				columnCount += 1
			Next

			Dim header As String = "Bulk product usage report from " & report.LastSent & " to " & endDate
			If Not ownerId.Equals(Guid.Empty) Then
				header &= ", for owner '" & New KaOwner(connection, ownerId).Name & "'"
			End If
			If Not bayId.Equals(Guid.Empty) Then
				header &= ", in the '" & New KaBay(connection, bayId).Name & "' bay"
			End If
			If Not panelId.Equals(Guid.Empty) Then
				header &= ", using the '" & New KaPanel(connection, panelId).Name & "' panel"
			End If
			If bulkProductList.Count > 0 Then
				Dim bulkNameList As String = ""
				For Each bulkProdId As Guid In bulkProductList
					If bulkNameList.Length > 0 Then bulkNameList &= ", "
					Try
						bulkNameList &= Q(New KaBulkProduct(connection, bulkProdId).Name)
					Catch ex As RecordNotFoundException
					End Try
				Next
				header &= ", for the bulk product" & IIf(bulkProductList.Count > 1, "s", "") & ": " & bulkNameList
			End If
			email.Body = CreateSiteCssStyle(SITE_CSS_FILENAME) & KaReports.GetTableHtml(header, DateTime.Now.ToString, reportDataList, False, tableAttributes, headerRowAttributes, headerCellAttributeList, rowAttributes, detailCellAttributeList)
			email.BodyIsHtml = True
			attachments.Add(New Attachment(New MemoryStream(Encoding.UTF8.GetBytes(email.Body)), "report.html", MediaTypeNames.Text.Html))
		End If
	End Sub

	Private Sub GetDriverInFacilityHistoryReport(connection As OleDbConnection, report As KaEmailReport, email As KaEmail, attachments As List(Of Attachment), ByVal endDate As DateTime)
		Dim bogusId As Guid = Guid.NewGuid
		Dim ownerId As Guid = Guid.Parse(GetParameter("owner_id", report.ReportParameters, bogusId.ToString()))
		If ownerId = bogusId Then
#Disable Warning BC40000
			'For backwards compatibility
			ownerId = report.OwnerId
#Enable Warning BC40000 ' Type or member is obsolete
		End If
		Dim mediatype As String = GetParameter("format", report.ReportParameters, KaReports.MEDIA_TYPE_HTML)
		Dim reportDataList As ArrayList = KaReports.GetDriverHistoryTable(connection, report.LastSent, endDate)
		If mediatype = KaReports.MEDIA_TYPE_COMMA Then
			email.Body = "See attached report"
			email.BodyIsHtml = False
			attachments.Add(New Attachment(New MemoryStream(Encoding.UTF8.GetBytes(KaReports.GetTableCsv(email.Subject, "", reportDataList))), "report.csv", MediaTypeNames.Text.Plain))
		Else
			Dim tableAttributes As String = "border=1; width=100%;"
			Dim headerRowAttributes As String = ""
			Dim rowAttributes As String = ""
			Dim headerRowList As ArrayList = reportDataList(0) 'This will be the header row

			Dim headerCellAttributeList As New List(Of String)
			Dim detailCellAttributeList As New List(Of String)
			Dim columnCount As Integer = 0
			For i = 0 To 2
				headerCellAttributeList.Add("style=""font-weight: bold; text-align: left;""")
				detailCellAttributeList.Add("style=""text-align: left;""")
				columnCount += 1
			Next

			For i = 3 To headerRowList.Count - 1
				headerCellAttributeList.Add("style=""font-weight: bold; text-align: right;""")
				detailCellAttributeList.Add("style=""text-align: right;""")
				columnCount += 1
			Next

			email.Body = CreateSiteCssStyle(SITE_CSS_FILENAME) & KaReports.GetTableHtml("Driver in facility history for dates " & report.LastSent & " to " & endDate, DateTime.Now.ToString, reportDataList, False, tableAttributes, headerRowAttributes, headerCellAttributeList, rowAttributes, detailCellAttributeList)
			email.BodyIsHtml = True
			attachments.Add(New Attachment(New MemoryStream(Encoding.UTF8.GetBytes(email.Body)), "report.html", MediaTypeNames.Text.Html))
		End If
	End Sub

	Private Sub GetTrackReport(connection As OleDbConnection, report As KaEmailReport, email As KaEmail, attachments As List(Of Attachment), ByVal endDate As DateTime)
		Dim bogusId As Guid = Guid.NewGuid
		Dim ownerId As Guid = Guid.Parse(GetParameter("owner_id", report.ReportParameters, bogusId.ToString()))
		If ownerId = bogusId Then
#Disable Warning BC40000
			'For backwards compatibility
			ownerId = report.OwnerId
#Enable Warning BC40000 ' Type or member is obsolete
		End If

		Dim trackId As Guid = Guid.Parse(GetParameter("track_id", report.ReportParameters, Guid.Empty.ToString()))
		Dim showOperator As Boolean = Boolean.Parse(GetParameter("show_operator", report.ReportParameters, True))
		Dim showRfid As Boolean = Boolean.Parse(GetParameter("show_rfid", report.ReportParameters, True))
		Dim showCarNumber As Boolean = Boolean.Parse(GetParameter("show_car_number", report.ReportParameters, True))
		Dim showTrack As Boolean = Boolean.Parse(GetParameter("show_track", report.ReportParameters, True))
		Dim showScanTime As Boolean = Boolean.Parse(GetParameter("show_scan_time", report.ReportParameters, True))
		Dim showReverseOrder As Boolean = Boolean.Parse(GetParameter("show_reverse_order", report.ReportParameters, False))

		Dim mediatype As String = GetParameter("format", report.ReportParameters, KaReports.MEDIA_TYPE_HTML)
		Dim reportDataList As ArrayList = KaReports.GetTrackReportTable(connection, report.LastSent, endDate, trackId, showOperator, showRfid, showCarNumber, showTrack, showScanTime, showReverseOrder)
		If mediatype = KaReports.MEDIA_TYPE_COMMA Then
			email.Body = "See attached report"
			email.BodyIsHtml = False
			attachments.Add(New Attachment(New MemoryStream(Encoding.UTF8.GetBytes(KaReports.GetTableCsv(email.Subject, "", reportDataList))), "report.csv", MediaTypeNames.Text.Plain))
		Else
			Dim tableAttributes As String = "border=1; width=100%;"
			Dim headerRowAttributes As String = ""
			Dim rowAttributes As String = ""
			Dim headerRowList As ArrayList = reportDataList(0) 'This will be the header row

			Dim headerCellAttributeList As New List(Of String)
			Dim detailCellAttributeList As New List(Of String)
			Dim columnCount As Integer = 0
			For i = 0 To 2
				headerCellAttributeList.Add("style=""font-weight: bold; text-align: left;""")
				detailCellAttributeList.Add("style=""text-align: left;""")
				columnCount += 1
			Next

			For i = 3 To headerRowList.Count - 1
				headerCellAttributeList.Add("style=""font-weight: bold; text-align: right;""")
				detailCellAttributeList.Add("style=""text-align: right;""")
				columnCount += 1
			Next

			Dim header As String = "Track report for dates " & report.LastSent.ToString("d") & " to " & endDate.ToString("d")
			If Not trackId.Equals(Guid.Empty) Then header &= " for " & New KaTrack(connection, trackId).Name

			email.Body = CreateSiteCssStyle(SITE_CSS_FILENAME) & KaReports.GetTableHtml(header, DateTime.Now.ToString, reportDataList, False, tableAttributes, headerRowAttributes, headerCellAttributeList, rowAttributes, detailCellAttributeList)
			email.BodyIsHtml = True
			attachments.Add(New Attachment(New MemoryStream(Encoding.UTF8.GetBytes(email.Body)), "report.html", MediaTypeNames.Text.Html))
		End If
	End Sub

	Private Sub GetTransportInFacilityHistoryReport(connection As OleDbConnection, report As KaEmailReport, email As KaEmail, attachments As List(Of Attachment), ByVal endDate As DateTime)
		Dim bogusId As Guid = Guid.NewGuid
		Dim ownerId As Guid = Guid.Parse(GetParameter("owner_id", report.ReportParameters, bogusId.ToString()))
		If ownerId = bogusId Then
#Disable Warning BC40000
			'For backwards compatibility
			ownerId = report.OwnerId
#Enable Warning BC40000 ' Type or member is obsolete
		End If
		Dim mediatype As String = GetParameter("format", report.ReportParameters, KaReports.MEDIA_TYPE_HTML)
		Dim reportDataList As ArrayList = KaReports.GetTransportsInFacilityHistoryTable(connection, report.LastSent, endDate)
		If mediatype = KaReports.MEDIA_TYPE_COMMA Then
			email.Body = "See attached report"
			email.BodyIsHtml = False
			attachments.Add(New Attachment(New MemoryStream(Encoding.UTF8.GetBytes(KaReports.GetTableCsv(email.Subject, "", reportDataList))), "report.csv", MediaTypeNames.Text.Plain))
		Else
			Dim tableAttributes As String = "border=1; width=100%;"
			Dim headerRowAttributes As String = ""
			Dim rowAttributes As String = ""
			Dim headerRowList As ArrayList = reportDataList(0) 'This will be the header row

			Dim headerCellAttributeList As New List(Of String)
			Dim detailCellAttributeList As New List(Of String)
			Dim columnCount As Integer = 0
			For i = 0 To 2
				headerCellAttributeList.Add("style=""font-weight: bold; text-align: left;""")
				detailCellAttributeList.Add("style=""text-align: left;""")
				columnCount += 1
			Next

			For i = 3 To headerRowList.Count - 1
				headerCellAttributeList.Add("style=""font-weight: bold; text-align: right;""")
				detailCellAttributeList.Add("style=""text-align: right;""")
				columnCount += 1
			Next

			email.Body = CreateSiteCssStyle(SITE_CSS_FILENAME) & KaReports.GetTableHtml("Transport history for dates " & report.LastSent & " to " & endDate, DateTime.Now.ToString, reportDataList, False, tableAttributes, headerRowAttributes, headerCellAttributeList, rowAttributes, detailCellAttributeList)
			email.BodyIsHtml = True
			attachments.Add(New Attachment(New MemoryStream(Encoding.UTF8.GetBytes(email.Body)), "report.html", MediaTypeNames.Text.Html))
		End If
	End Sub

	Private Sub GetCustomReport(connection As OleDbConnection, report As KaEmailReport, email As KaEmail, attachments As List(Of Attachment), endDate As DateTime)
		Dim customPage As KaCustomPages = New KaCustomPages(connection, report.CustomPageId)
		Dim objCallWS As New DynamicWebService
		Dim arArguments(0) As Object
		arArguments(0) = report.Id
		Dim webServiceResult As Object = objCallWS.CallWebService(customPage.ReportWebServiceURL, customPage.ReportWebServiceServiceName, customPage.ReportWebServiceMethodName, arArguments, report.LastSent, endDate)

		Dim mediaType As String = GetParameter("format", report.ReportParameters, KaReports.MEDIA_TYPE_HTML)
		If mediaType = KaReports.MEDIA_TYPE_COMMA Then
			Dim reportCsvString As String = GetTableCsv(customPage.PageLabel, "", webServiceResult)
			email.Body = "See attached report"
			email.BodyIsHtml = False
			attachments.Add(New Attachment(New MemoryStream(Encoding.UTF8.GetBytes(reportCsvString)), "report.csv", MediaTypeNames.Text.Plain))
		Else
			email.Subject = report.Subject.Replace("{last_sent}", String.Format("{0:M/d/yyyy h:mm:ss tt}", report.LastSent)).Replace("{now}", String.Format("{0:M/d/yyyy h:mm:ss tt}", endDate))
			Dim reportHtmlString As String
			Try
				Dim returnObject As String = objCallWS.CallWebService(customPage.ReportWebServiceURL, customPage.ReportWebServiceServiceName, KaCustomPages.DEFAULT_WEBSERVICE_GET_ATTRIBUTE_METHOD_NAME, Nothing)
				Dim webServiceAttributes As KaReports.ReportTableAttributes = KaCommonObjects.XmlMethods.FromXml(returnObject, GetType(KaReports.ReportTableAttributes))
				webServiceAttributes.Title = customPage.PageLabel
				webServiceAttributes.SubTitle = email.Subject
				reportHtmlString = KaReports.GetTableHtml(webServiceResult, False, webServiceAttributes)
			Catch ex As Exception
				reportHtmlString = KaReports.GetTableHtml(customPage.PageLabel, email.Subject, webServiceResult)
			End Try
			Dim writer As New StringWriter()
			Dim htmlWriter As New HtmlTextWriter(writer)
			htmlWriter.Write(reportHtmlString)
			email.Body = writer.ToString()
			email.BodyIsHtml = True
			attachments.Add(New Attachment(New MemoryStream(Encoding.UTF8.GetBytes(email.Body)), "report.html", MediaTypeNames.Text.Html))
		End If
	End Sub


	Private Sub GetInterfaceTicketExportStatusReport(connection As OleDbConnection, report As KaEmailReport, email As KaEmail, attachments As List(Of Attachment), ByVal endDate As DateTime)
		Dim mediatype As String = GetParameter("format", report.ReportParameters, KaReports.MEDIA_TYPE_HTML)
		Dim interfaceId = New Guid(GetParameter("interface_id", report.ReportParameters, Guid.Empty.ToString))
		Dim sortCriteria = New String(GetParameter("ticket_sort", report.ReportParameters, "exported_at DESC"))
		Dim showTicketsExported = GetParameter("show_tickets_exported", report.ReportParameters, False)
		Dim includeTicketsMarkedManually = GetParameter("include_tickets_marked_manually", report.ReportParameters, False)
		Dim includeTicketsWithError = GetParameter("include_tickets_with_error", report.ReportParameters, False)
		Dim includeTicketsWithIgnoredError = GetParameter("include_tickets_with_ignored_error", report.ReportParameters, False)
		Dim onlyIncludeOrdersForThisInterface = GetParameter("only_include_orders_for_this_interface", report.ReportParameters, True)
		Dim useReceiving As Boolean
		Boolean.TryParse((GetParameter("is_receiving", report.ReportParameters, False)), useReceiving)

		Dim reportDataList As ArrayList
		If useReceiving Then
			reportDataList = KaReports.GetInterfaceReceivingTicketExportStatusReport(connection, interfaceId, sortCriteria, showTicketsExported, includeTicketsMarkedManually, includeTicketsWithError, includeTicketsWithIgnoredError, onlyIncludeOrdersForThisInterface)
		Else
			reportDataList = KaReports.GetInterfaceTicketExportStatusReport(connection, interfaceId, sortCriteria, showTicketsExported, includeTicketsMarkedManually, includeTicketsWithError, includeTicketsWithIgnoredError, onlyIncludeOrdersForThisInterface)
		End If

		If mediatype = KaReports.MEDIA_TYPE_COMMA Then
			email.Body = "See attached report"
			email.BodyIsHtml = False
			attachments.Add(New Attachment(New MemoryStream(Encoding.UTF8.GetBytes(KaReports.GetTableCsv(email.Subject, "", reportDataList))), "report.csv", MediaTypeNames.Text.Plain))
		Else
			Dim tableAttributes As String = "border='1px'; width='100%';"
			Dim headerRowAttributes As String = ""
			Dim rowAttributes As String = ""
			Dim headerRowList As ArrayList = reportDataList(0) 'This will be the header row
			Dim headerCellAttributeList As New List(Of String)
			Dim detailCellAttributeList As New List(Of String)
			For i = 0 To headerRowList.Count - 1
				headerCellAttributeList.Add("style=""font-weight: bold; """)
			Next

			email.Body = CreateSiteCssStyle("\" & SITE_CSS_FILENAME) & KaReports.GetTableHtml("", DateTime.Now.ToString, reportDataList, False, tableAttributes, headerRowAttributes, headerCellAttributeList, rowAttributes, detailCellAttributeList)
			email.BodyIsHtml = True
			attachments.Add(New Attachment(New MemoryStream(Encoding.UTF8.GetBytes(email.Body)), "report.html", MediaTypeNames.Text.Html))
		End If
	End Sub

	Private Sub GetInterfaceTicketReceivingExportStatusReport(connection As OleDbConnection, report As KaEmailReport, email As KaEmail, attachments As List(Of Attachment), ByVal endDate As DateTime)
		Dim mediatype As String = GetParameter("format", report.ReportParameters, KaReports.MEDIA_TYPE_HTML)
		Dim interfaceId = New Guid(GetParameter("interface_id", report.ReportParameters, Guid.Empty.ToString))
		Dim sortCriteria = New String(GetParameter("ticket_sort", report.ReportParameters, "exported_at DESC"))
		Dim showTicketsExported = GetParameter("show_tickets_exported", report.ReportParameters, False)
		Dim includeTicketsMarkedManually = GetParameter("include_tickets_marked_manually", report.ReportParameters, False)
		Dim includeTicketsWithError = GetParameter("include_tickets_with_error", report.ReportParameters, False)
		Dim includeTicketsWithIgnoredError = GetParameter("include_tickets_with_ignored_error", report.ReportParameters, False)
		Dim onlyIncludeOrdersForThisInterface = GetParameter("only_include_orders_for_this_interface", report.ReportParameters, True)

		Dim reportDataList As ArrayList = KaReports.GetInterfaceReceivingTicketExportStatusReport(connection, interfaceId, sortCriteria, showTicketsExported, includeTicketsMarkedManually, includeTicketsWithError, includeTicketsWithIgnoredError, onlyIncludeOrdersForThisInterface)


		If mediatype = KaReports.MEDIA_TYPE_COMMA Then
			email.Body = "See attached report"
			email.BodyIsHtml = False
			attachments.Add(New Attachment(New MemoryStream(Encoding.UTF8.GetBytes(KaReports.GetTableCsv(email.Subject, "", reportDataList))), "report.csv", MediaTypeNames.Text.Plain))
		Else
			Dim tableAttributes As String = "border='1px'; width='100%';"
			Dim headerRowAttributes As String = ""
			Dim rowAttributes As String = ""
			Dim headerRowList As ArrayList = reportDataList(0) 'This will be the header row
			Dim headerCellAttributeList As New List(Of String)
			Dim detailCellAttributeList As New List(Of String)
			For i = 0 To headerRowList.Count - 1
				headerCellAttributeList.Add("style=""font-weight: bold; """)
			Next

			email.Body = CreateSiteCssStyle("\" & SITE_CSS_FILENAME) & KaReports.GetTableHtml("", DateTime.Now.ToString, reportDataList, False, tableAttributes, headerRowAttributes, headerCellAttributeList, rowAttributes, detailCellAttributeList)
			email.BodyIsHtml = True
			attachments.Add(New Attachment(New MemoryStream(Encoding.UTF8.GetBytes(email.Body)), "report.html", MediaTypeNames.Text.Html))
		End If
	End Sub
#End Region

	Private Function GetApplicator(connection As OleDbConnection, id As Guid, cache As Dictionary(Of Guid, KaApplicator)) As KaApplicator
		Try ' to get the applicator from the cache
			Return cache(id)
		Catch ex As KeyNotFoundException ' the applicator is not in the cache
			Try ' to get the applicator from the database
				Dim applicator As New KaApplicator(connection, id)
				cache(id) = applicator
				Return applicator
			Catch ex2 As RecordNotFoundException ' the applicator is not in the database
				Return Nothing
			End Try
		End Try
	End Function

	Private Function GetBranch(connection As OleDbConnection, id As Guid, cache As Dictionary(Of Guid, KaBranch)) As KaBranch
		Try ' to get the branch from the cache
			Return cache(id)
		Catch ex As KeyNotFoundException ' the branch is not in the cache
			Try ' to get the branch from the database
				Dim branch As New KaBranch(connection, id)
				cache(id) = branch
				Return branch
			Catch ex2 As RecordNotFoundException ' the branch is not in the database
				Return Nothing
			End Try
		End Try
	End Function

	Private Function GetCarrier(connection As OleDbConnection, id As Guid, cache As Dictionary(Of Guid, KaCarrier)) As KaCarrier
		Try ' to get the carrier from the cache
			Return cache(id)
		Catch ex As KeyNotFoundException ' carrier isn't in the cache
			Try ' to get the carrier from the database
				Dim carrier As New KaCarrier(connection, id)
				cache(id) = carrier
				Return carrier
			Catch ex2 As RecordNotFoundException ' carrier isn't in the database
				Return Nothing
			End Try
		End Try
	End Function

	Private Function GetCustomerAccountLocation(connection As OleDbConnection, id As Guid, cache As Dictionary(Of Guid, KaCustomerAccountLocation)) As KaCustomerAccountLocation
		Try ' to get the customer account location from the cache
			Return cache(id)
		Catch ex As KeyNotFoundException ' the customer account location isn't in the cache
			Try ' to get the customer account location from the database
				Dim customerAccountLocation As New KaCustomerAccountLocation(connection, id)
				cache(customerAccountLocation.Id) = customerAccountLocation
				Return customerAccountLocation
			Catch ex2 As RecordNotFoundException ' the customer account location isn't in the database
				Return Nothing
			End Try
		End Try
	End Function

	Private Function GetDriver(connection As OleDbConnection, id As Guid, cache As Dictionary(Of Guid, KaDriver)) As KaDriver
		Try ' to get the driver from the cache
			Return cache(id)
		Catch ex As KeyNotFoundException ' the driver isn't in the cache
			Try ' to get the driver from the database
				Dim driver As New KaDriver(connection, id)
				cache(id) = driver
				Return driver
			Catch ex2 As RecordNotFoundException ' the driver isn't in the database
				Return Nothing
			End Try
		End Try
	End Function

	Private Function GetLocation(connection As OleDbConnection, id As Guid, cache As Dictionary(Of Guid, KaLocation)) As KaLocation
		Try ' to get the location from the cache
			Return cache(id)
		Catch ex As KeyNotFoundException ' the location isn't in the cache
			Try ' to get the location from the database
				Dim location As New KaLocation(connection, id)
				cache(id) = location
				Return location
			Catch ex2 As RecordNotFoundException ' the location isn't in the database
				Return Nothing
			End Try
		End Try
	End Function

	Private Function GetOwner(connection As OleDbConnection, id As Guid, cache As Dictionary(Of Guid, KaOwner)) As KaOwner
		Try ' to get the owner from the dictionary...
			Return cache(id)
		Catch ex As KeyNotFoundException ' the owner isn't in the dictionary...
			Dim owner As KaOwner
			Try ' to get the owner from the database...
				owner = New KaOwner(connection, id)
			Catch ex2 As RecordNotFoundException
				Return Nothing
			End Try
			cache(id) = owner
			Return owner
		End Try
	End Function

	Private Function GetCustomerAccount(connection As OleDbConnection, id As Guid, cache As Dictionary(Of Guid, KaCustomerAccount)) As KaCustomerAccount
		Try ' to get the customer account from the dictionary...
			Return cache(id)
		Catch ex As KeyNotFoundException ' the customer account isn't in the dictionary...
			Dim customerAccount As KaCustomerAccount
			Try ' to get the customer account from the database...
				customerAccount = New KaCustomerAccount(connection, id)
			Catch ex2 As RecordNotFoundException
				Return Nothing
			End Try
			cache(id) = customerAccount
			Return customerAccount
		End Try
	End Function

	Private Function GetSupplierAccount(connection As OleDbConnection, id As Guid, cache As Dictionary(Of Guid, KaSupplierAccount)) As KaSupplierAccount
		Try ' to get the customer account from the dictionary...
			Return cache(id)
		Catch ex As KeyNotFoundException ' the customer account isn't in the dictionary...
			Dim supplierAccount As KaSupplierAccount
			Try ' to get the customer account from the database...
				supplierAccount = New KaSupplierAccount(connection, id)
			Catch ex2 As RecordNotFoundException
				Return Nothing
			End Try
			cache(id) = supplierAccount
			Return supplierAccount
		End Try
	End Function

	Private Function GetTicketHtml(url As String) As String
		Dim html As String = New UTF8Encoding().GetString(New WebClient().DownloadData(url))
		Dim i As Integer = 0
		Do ' remove any ASP.NET state variables
			i = html.IndexOf("<input type=""hidden""")
			If i > -1 Then
				Dim j As Integer = html.IndexOf(">", i)
				html = html.Substring(0, i) & html.Substring(j + 1, html.Length - j - 1)
			End If
		Loop While i <> -1
		Return html
	End Function

	Private Function IsTicketEmailed(connection As OleDbConnection, id As Guid) As Boolean
		Dim reader As OleDbDataReader = Tm2Database.ExecuteReader(connection, String.Format("SELECT emailed, voided FROM tickets WHERE id={0}", Q(id)))
		If reader.Read() Then
			If reader.Item("voided") Then
				Return True ' Treat this as emailed
			ElseIf reader.Item("emailed") Then
				Return True
			End If
		Else
			Throw New RecordNotFoundException("Ticket not found with ID=" & id.ToString())
		End If
		reader.Close()

		Return False
	End Function

	Private Function IsReceivingTicketEmailed(connection As OleDbConnection, id As Guid) As Boolean
		Dim reader As OleDbDataReader = Tm2Database.ExecuteReader(connection, String.Format("SELECT " & KaReceivingTicket.FN_EMAILED & ", " & KaReceivingTicket.FN_VOIDED & " FROM " & KaReceivingTicket.TABLE_NAME & " WHERE id={0}", Q(id)))
		If reader.Read() Then
			If reader.Item(KaReceivingTicket.FN_VOIDED) Then
				Return True ' Treat this as emailed
			ElseIf reader.Item(KaReceivingTicket.FN_EMAILED) Then
				Return True
			End If
		Else
			Throw New RecordNotFoundException("Receiving ticket not found with ID=" & id.ToString())
		End If
		reader.Close()

		Return False
	End Function

	Private Function GetParameter(name As String, xml As String, defaultValue As String) As String
		Dim reader As XmlReader = XmlReader.Create(New MemoryStream(Encoding.UTF8.GetBytes(xml)))
		Dim elementName As String = ""
		Do While reader.Read()
			Select Case reader.NodeType
				Case XmlNodeType.Element
					elementName = reader.Name.ToLower()
				Case XmlNodeType.Text
					If elementName.ToLower() = name.ToLower() Then Return reader.Value
			End Select
		Loop
		Return defaultValue
	End Function

	Private Sub LogInterfaceRun()
		Dim alerts As New KaCommonObjects.Alerts
		If (Not String.IsNullOrEmpty(_logger.LogPath)) Then
			alerts.AlarmDocLocation = IO.Path.Combine(_logger.LogPath, APPLICATION_ID & "_AlarmHistory.txt")
		End If

		Dim argsSupplied As String = ""
		For Each arg As String In My.Application.CommandLineArgs
			If arg.Length > 0 Then
				If argsSupplied.Length > 0 Then argsSupplied &= " "
				If arg.Contains(" ") Then argsSupplied &= ControlChars.Quote
				argsSupplied &= arg
				If arg.Contains(" ") Then argsSupplied &= ControlChars.Quote
			End If
		Next
		_logger.Write("Arguments supplied: " & argsSupplied, Common.Logger.LogFileType.Processing)
	End Sub

	Private Sub GetScheduledReports(ByVal connection As OleDbConnection, ByRef reportListEndTimes As Dictionary(Of KaEmailReport, DateTime), ByVal excludeReportList As List(Of Guid), ByVal timeCheckStarted As DateTime)
		Dim rdr As OleDbDataReader = Tm2Database.ExecuteReader(connection, "SELECT DISTINCT email_reports.id " &
									"FROM email_reports " &
									"INNER JOIN email_report_triggers ON email_reports.id = email_report_triggers.email_report_id " &
									"WHERE (email_reports.deleted = 0) AND (email_report_triggers.deleted = 0) AND (email_report_triggers.disabled = 0)")
		Do While rdr.Read()
			Try
				Dim report As New KaEmailReport(connection, rdr("id"))
				If Not excludeReportList.Contains(report.Id) Then
					For Each reportRecord As KaEmailReport In reportListEndTimes.Keys
						If reportRecord.Equals(report) Then
							'This report is already in the list, do not re-add it, continue Do.
							Continue Do
						End If
					Next
					reportListEndTimes.Add(report, DateTime.MaxValue)
					reportListEndTimes(report) = report.GetEndDateFromReportTriggers(timeCheckStarted)

					If report.IsMonthToDate Then
						report.LastSent = New Date(report.LastSent.Year, report.LastSent.Month, 1, report.LastSent.Hour, report.LastSent.Minute, report.LastSent.Second)
					End If
				End If
			Catch ex As RecordNotFoundException ' report was not found in the database
				_logger.Write(String.Format("Error: report for ID={0} not found in database", rdr("id")), Common.Logger.LogFileType.Alarm)
				CreateEventLogEntry(KaEventLog.Categories.Warning, String.Format("Error: report for ID={0} not found in database", rdr("id")), connection)
			End Try
		Loop
		rdr.Close()
	End Sub

	Private Sub CreateEventLogEntry(ByVal category As KaEventLog.Categories, ByVal description As String, ByVal connection As OleDbConnection)
		CreateEventLogEntry(category, description, connection, 1)
	End Sub

	Private Sub CreateEventLogEntry(ByVal category As KaEventLog.Categories, ByVal description As String, ByVal connection As OleDbConnection, resendInterval As Integer)
		Try
			Dim eventLog As New KaEventLog
			With eventLog
				.ApplicationIdentifier = "TM2 Email Service"
				.ApplicationVersion = Tm2Database.FormatVersion(My.Application.Info.Version, "X")
				.Category = category
				.Computer = My.Computer.Name
				.Description = description

			End With
			KaEventLog.CreateEventLog(connection, eventLog, resendInterval, Nothing, eventLog.Computer & "/" & eventLog.ApplicationIdentifier, "")
		Catch ex As Exception

		End Try
	End Sub

	Private Function CreateSiteCssStyle(ByVal fileName As String) As String
		Dim cssString As String = ""
		Try
			Using sr As New StreamReader(fileName)
				cssString = "<style type=""text/css"">"
				Dim line As String
				' Read and display lines from the file until the end of
				' the file is reached.
				Do
					line = sr.ReadLine()
					If Not (line Is Nothing) Then
						cssString &= line
					End If
				Loop Until line Is Nothing
				cssString &= "</style>"
			End Using
		Catch ex As Exception

		End Try

		Return cssString
	End Function

	Public Shared Function ConvertWebPageUrlDomainToRequestedPagesDomain(ByVal connection As OleDbConnection) As Boolean
		Dim retVal As Boolean = False
		Boolean.TryParse(KaSetting.GetSetting(connection, "ConvertWebPageUrlDomainToRequestedPagesDomain", True), retVal)
		Return retVal
	End Function

	Private Function CheckActivation() As Boolean
		_logger.Write("Checking License...", Common.Logger.LogFileType.Processing)
		Return Common.CheckAuthorized(False, AddressOf LicenseAlmostExpired, AddressOf InTrialMode, AddressOf DeactivatedMessage, True)
	End Function

	Private Sub LicenseAlmostExpired(daysLeft As Integer)
		_logger.Write(daysLeft.ToString() & " day" + IIf(daysLeft = 1, " ", "s ") & "remaining before license expires", Common.Logger.LogFileType.Warnings)
	End Sub

	Private Sub InTrialMode(daysLeft As Integer)
		_logger.Write("Running in trial mode. " & daysLeft.ToString() & " day" + IIf(daysLeft = 1, " ", "s ") & "remaining in trial cycle.", Common.Logger.LogFileType.Warnings)
	End Sub

	Private Sub DeactivatedMessage(message As String)
		_logger.Write(message, Common.Logger.LogFileType.Alarm)
	End Sub

	Private Function ConvertImageToData(ByVal image As System.Drawing.Image) As String
		If image Is Nothing Then Return ""
		Dim s As New IO.MemoryStream()
		image.Save(s, System.Drawing.Imaging.ImageFormat.Png)
		s.Position = 0
		Dim data(s.Length - 1) As Byte
		s.Read(data, 0, s.Length)
		s.Close()
		Return Convert.ToBase64String(data)
	End Function
End Class
