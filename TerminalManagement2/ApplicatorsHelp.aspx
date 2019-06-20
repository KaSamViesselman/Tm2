<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="ApplicatorsHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.ApplicatorsHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <title>Help : Applicators</title>
    <link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
    <form id="ApplicatorsHelp" runat="server">
        <div>
            <span style="font-size: large; font-weight: bold;"><a href="help.aspx#Applicators">Help</a>: Applicators</span><hr style="width: 100%; color: #003399;" />
            <div id="divHelp">
                <p>Applicators may be used to document who applied a load to a field. The applicator record also includes information to track applicators' performance.</p>
                <p><span class="helpItem">Applicator drop-down list:</span> located at the top of the page, this list contains all the applicator records. To edit an existing applicator, select their name in the drop-down list. To create a new applicator, select "Enter new applicator".</p>
                <table style="width: 100%; vertical-align: top;">
                    <tr>
                        <td style="width: 50%; vertical-align: top; border-right: 1px solid #404040;">
                            <table>
                                <tr>
                                    <td><span class="helpItem">Name:</span> the name of the applicator. The name is required and may include up to 50 characters.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">License:</span> the applicator's license number. The license is optional and may include up to 50 characters.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">EPA number:</span> the applicator's EPA number. The EPA number is optional and may include up to 50 characters.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Acres:</span> the total number of acres that this applicator has applied. This number accumulates any acres for loads that the applicator is assigned to.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">E-mail:</span> if Terminal Management 2 is configured to e-mail tickets a ticket will be sent to this (or these) e-mail address(es) when the ticket is created by a system for an order referencing this applicator. E-mail is optional and multiple e-mail addresses may be entered separated by a comma.</td>
                                </tr>
                            </table>
                        </td>
                        <td style="width: 50%; vertical-align: top;"><span class="helpItem">Interfaces</span><p>Interfaces to 3rd party software packages may require that interface settings for each applicator that may be specified by the 3rd party software package be setup.</p>
                            <table>
                                <tr>
                                    <td><span class="helpItem">Interface settings:</span> contains a list of all the available interface setting records. To modify an existing interface setting record, select it from the drop down list. To add a new interface setting record, select "Add an interface" from the drop-down list.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Interface:</span> selects the interface that this applicator interface settings correspond to.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Cross reference:</span> a cross reference used to identify the applicator when received in an order or sent as a ticket through an interface with a 3rd party software package. Cross reference may be up to 50 characters in length.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Default setting:</span> used when there are multiple values set for an item for an interface, and the interface setting was not set for the item to be exported for this interface (exporting manually created orders, exporting to an interface from which it wasn&#39;t imported from, etc.).</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Export only:</span> used only for outbound tickets, to allow for multiple cross references available for different applicators. If checked, then this cross reference will not be used for inbound interface lookups.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Save interface:</span> click to save the applicator interface settings record.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Remove interface:</span> click to remove the selected applicator interface settings record.</td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </div>
            <p><span class="helpItem">Save button:</span> located at the top of the page, click this button to save any changes made to an applicator record.</p>
            <p><span class="helpItem">Delete button:</span> located at the top of the page, click this button to delete the current applicator record.</p>
        </div>
    </form>
</body>
</html>
