<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="TrackReportHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.TrackReportHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server"><title>Help : Track Report</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="TrackReportHelp" runat="server">
		<div><span style="font-size: large; font-weight: bold;"><a href="help.aspx#TrackReport">Help</a>: Track Report</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>The track report provides a list of rail cars that were recorded as being on a track at a facility.</p>
				<p><span class="helpItem">Start date:</span> the oldest rail car scan to include in the report.</p>
				<p><span class="helpItem">End date:</span> the most recent rail car scan to include in the report.</p>
				<p><span class="helpItem">Show operator name:</span> includes the name of the user that scanned the rail car when checked.</p>
				<p><span class="helpItem">Show RFID number:</span> includes the RFID number of the rail card when checked.</p>
				<p><span class="helpItem">Show rail car number:</span> includes the rail car number when checked.</p>
				<p><span class="helpItem">Show date/time:</span> includes the date and time that the rail car was scanned when checked.</p>
				<p><span class="helpItem">Show in reverse order:</span> show the report results in the reverse order from how they were scanned.</p>
				<p><span class="helpItem">Show report:</span> shows the report results for the selected track(s) and date range.</p>
				<p><span class="helpItem">Download:</span> downloads the report results for the selected track(s) and date range as a comma separated value (CSV) file that may be opened in a spreadsheet application.</p>
				<p><span class="helpItem">E-mail to:</span> the track report may be e-mailed directly from the page. Enter the e-mail addresses for the recipients (multiple addresses may be separated by commas) and click the "Send" button.</p>
				<p><span class="helpItem">Add address:</span> will look in the e-mail history, and items to find possible e-mail addresses that the report may be sent to.</p>
			</div>
		</div>
	</form>
</body>
</html>
