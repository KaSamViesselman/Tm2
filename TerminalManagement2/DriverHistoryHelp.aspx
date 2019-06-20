<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="DriverHistoryHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.DriverHistoryHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <title>Help : Drivers In Facility History</title>
    <link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
    <form id="DriverHistoryHelp" runat="server">
        <div>
            <span style="font-size: large; font-weight: bold;"><a href="help.aspx#DriversInFacilityHistory">Help</a> : Drivers In Facility History</span><hr style="width: 100%; color: #003399;" />
            <div id="divHelp">
                <p>The drivers in facility history page shows which drivers were in the facility over a selectable date range.</p>
                <p><span class="helpItem">From:</span> the oldest driver in facility record to show.</p>
                <p><span class="helpItem">To:</span> the newest driver out of facility record to show.</p>
                <p><span class="helpItem">Show report:</span> click to create the report with the selected date range.</p>
                <p><span class="helpItem">Printer friendly:</span> opens the report in a new page with the selected date range.</p>
                <p><span class="helpItem">Download report:</span> downloads a CSV version of the report with the selected date range.</p>
                <p><span class="helpItem">E-mail to:</span> the driver in facility history report may be e-mailed directly from the page. Enter the e-mail addresses for the recipients (multiple addresses may be separated by commas) and click the "Send" button.</p>
                <p><span class="helpItem">Add address:</span> will look in the e-mail history, and items to find possible e-mail addresses that the report may be sent to.</p>
            </div>
        </div>
    </form>
</body>
</html>
