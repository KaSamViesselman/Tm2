<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="AssignmentsHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.AssignmentsHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <title>Help : Assignments</title>
    <link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
    <form id="AccountsHelp" runat="server">
        <div>
            <span style="font-size: large; font-weight: bold;"><a href="help.aspx#Accounts">Help</a>: Assignments</span><hr style="width: 100%; color: #003399;" />
            <div id="divHelp">
                <p>Assignments are used to assign a driver or customer account to a specific facility.</p>
                <p><span class="helpItem">Facility drop-down list:</span> the facility drop-down list contains all the facilities. To modify a facilities assignments, select the facility from the drop-down list.</p>
                <table>
                    <tr>
                        <td style="width: 50%;">
                            <span class="helpItem">Drivers</span><p>Shows which drivers are assigned to the selected facility. Drivers may be assigned to multiple facilities.</p>
                            <table>
                                <tr>
                                    <td><span class="helpItem">Driver drop-down list:</span> the list shows which drivers are currently available for the selected facility.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Driver assignment list:</span> the list shows which drivers are currently assigned to the selected facility.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Driver drop-down list:</span> select the driver to be assigned to the facility.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Add driver:</span> add the driver currently selected in the driver drop-down list to the facility.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Remove driver:</span> remove the driver currently selected in the driver assignment list from the facility.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Add all drivers:</span> add all the available drivers to the facility.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Remove all drivers:</span> remove all drivers listed in the driver assignment list from the facility.</td>
                                </tr>
                            </table>
                            <p><span class="helpItem">Facilities</span></p>
                            <table>
                                <tr>
                                    <td><span class="helpItem">Valid for all facilities:</span> allows the customer to pick from all facilities, including new facilities added at a later time.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Customer account drop-down list:</span> the list shows which accounts are currently available for the selected facility.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Customer account assignment list:</span> the list shows which accounts are currently assigned to the selected facility.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Customer account drop-down list:</span> select the account to be assigned to the facility.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Add account:</span> add the account currently selected in the account drop-down list to the facility.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Remove account:</span> remove the account currently selected in the account assignment list from the facility.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Add all accounts:</span> add all the available accounts to the facility.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Remove all accounts:</span> remove all accounts listed in the account assignment list from the facility.</td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </div>
        </div>
    </form>
</body>
</html>
