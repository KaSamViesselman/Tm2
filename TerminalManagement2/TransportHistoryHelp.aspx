<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="TransportHistoryHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.TransportHistoryHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <title>Help : Transports In Facility History</title>
    <link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
    <form id="TransportHistoryReport" runat="server">
        <div>
            <span style="font-size: large; font-weight: bold;"><a href="help.aspx#TransportsInFacilityHistory">Help</a> : Transports In Facility History</span><hr style="width: 100%; color: #003399;" />
            <div id="divHelp">
                <p>The transports in facility history page shows which transports were in the facility over a selectable date range.</p>
                <p><span class="helpItem">Start date:</span> the oldest transport in facility record to show.</p>
                <p><span class="helpItem">End date:</span> the newest transport out of facility record to show.</p>
                <p><span class="helpItem">Show report:</span> click to create the report with the selected date range.</p>
                <p><span class="helpItem">Printer friendly:</span> opens the report in a new page with the selected date range.</p>
                <p><span class="helpItem">Download report:</span> downloads a CSV version of the report with the selected date range.</p>
                <p><span class="helpItem">E-mail to:</span> the transports in facility history report may be e-mailed directly from the page. Enter the e-mail addresses for the recipients (multiple addresses may be separated by commas) and click the "Send" button.</p>
                <p><span class="helpItem">Add address:</span> will look in the e-mail history, and items to find possible e-mail addresses that the report may be sent to.</p>
            </div>
        </div>
    </form>
</body>
</html>
