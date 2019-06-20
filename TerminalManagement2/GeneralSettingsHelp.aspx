<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="GeneralSettingsHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.GeneralSettingsHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Help : General Settings</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="GeneralSettingsHelp" runat="server">
		<div>
			<span style="font-size: large; font-weight: bold;"><a href="help.aspx#GeneralSettings">Help</a> : General Settings</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p><span class="helpItem">General settings</span> control the behavior of Terminal Management and the applications that work with Terminal Management.</p>
				<p><span class="helpItem">Default mass unit:</span> the default mass unit of measure to use where mass is specified.</p>
				<p><span class="helpItem">Default volume unit:</span> the default volume unit of measure to use where volume is specified.</p>
				<p><span class="helpItem">Web page title:</span> this text will display in the browser title.</p>
				<p><span class="helpItem">Database change e-mail address(es):</span> this setting will create an e-mail for any time a database schema was changed by a software application.</p>
				<p><span class="helpItem">Convert web page URL domain to requested page's domain for links on pages:</span> will try to convert any links that are on pages to use the same domain authority as the current page's domain authority for links that are pointed to the IIS server.</p>
				<p><span class="helpItem">Default new drivers to be valid for all accounts:</span> will preset the "Valid for all drivers" setting when a new driver is entered.</p>
				<p><span class="helpItem">Item traceability enabled:</span> will display the web pages and items associated with the Item Traceability module. This module allows the system to track the receiving and usage of bulk product shipments.</p>
				<hr />
				<p><span class="helpItem">Database backup settings</span> are default values that the database backup application uses.</p>
				<p><span class="helpItem">Backup folder:</span> the destination folder that the backup program will write the database backup file to if the Scheduled Task is called without a <i>-p</i> parameter.</p>
				<p><span class="helpItem">Number of backup sets:</span> the maximum number of database backup sets that the backup program will keep in the backup if the Scheduled Task is called without a <i>-c</i> parameter.</p>
				<p><span class="helpItem">Backup timeout:</span> the amount of time (in seconds) that the system will allow a backup to run prior to timing out if the Scheduled Task is called without a <i>-t</i> parameter.</p>
				<hr />
				<p><span class="helpItem">Event log settings</span> are default values for event log entries.</p>
				<p><span class="helpItem">Days to keep event log records:</span> the number of days to keep event log records in the database. To remove event log records, a Scheduled Task should be created that calls the TM2 Background Service with the appropriate parameters. This can be accomplished by running the TM2 Scheduled Task Creator.</p>
			</div>
			<p><span class="helpItem">Save general settings:</span> click to save the settings entered.</p>
		</div>
	</form>
</body>
</html>
