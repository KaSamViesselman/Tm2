<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="TransportsHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.TransportsHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <title>Help : Transports</title>
    <link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
    <form id="TransportsHelp" runat="server">
        <div>
            <span style="font-size: large; font-weight: bold;"><a href="help.aspx#Transports">Help</a>: Transports</span>
            <hr style="width: 100%; color: #003399;" />
            <div id="divHelp">
                <p>The transports page may be used to setup transport records that represent trucks, trailers, rail cars, etc.</p>
                <p><span class="helpItem">Transport drop-down list:</span> to modify an existing transport, select the transport from the drop-down list. To create a new transport, select "Enter a new transport" from the drop-down list.</p>
                <table>
                    <tr>
                        <td style="width: 50%; border-right: 1px solid #404040">
                            <table>
                                <tr>
                                    <td><span class="helpItem">Name:</span> the transport name is required and may be up to 50 characters in length.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Number:</span> a number used to identify the transport, typically at a keypad or self-serve load-out system. The transport number is optional and may be up to 50 characters in length.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Type:</span> the transport type is optional and may be used to categorize transports. Transport types are user defined under the transport type tab.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">RFID number:</span> RFID tags may be used to identify the transport at a scale, gate or load-out point. A transport may have multiple RFID tags assigned to it. To add a new RFID tag, enter the tag ID in the blank to the right of the RFID tag list and click "Add New". To update a RFID tag, select it on the list, modify the tag ID in the blank to the right of the list, then click "Set". To remove a RFID tag, select the tag on the list and click the "Remove" button.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Unit:</span> the unit of measure to use for the transports empty and maximum weights.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Empty weight:</span> the empty or tare weight of the transport. The empty weight of the transport is optional. If specified the load-out software will use the empty weight in conjunction with the maximum weight to determine how much may be loaded into the transport.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Max weight:</span> the maximum gross weight that the transport may be loaded to. The maximum weight of the transport is optional. If specified the load-out software will use the maximum weight in conjunction with the empty weight to determine how much may be loaded into the transport.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Temporary maximum gross weight:</span> the temporary maximum gross weight that the transport may be loaded to until the <i>Temporary maximum gross weight expiration date</i>. The maximum weight of the transport is optional. If specified the load-out software will use the temporary maximum gross weight in conjunction with the empty weight to determine how much may be loaded into the transport.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Temporary maximum gross weight expiration date:</span> the date that the <i>Temporary maximum gross weight</i> is valid until. This value is optional. If not specified, then the load-out software will not use the temporary maximum gross weight in conjunction with the empty weight to determine how much may be loaded into the transport.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Length:</span> the length of the transport. The length is optional and may be specified in feet or meters.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Carrier:</span> the carrier that owns this transport. Carrier is optional.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Current order:</span> the order for the load that the transport is currently carrying. The current order is optional.</td>
                                </tr>
                            </table>
                            <p><span class="helpItem">Compartments</span> </p>
                            <table>
                                <tr>
                                    <td><span class="helpItem">Compartment:</span> to modify an existing compartment, select it in the drop-down list.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Add compartment:</span> click to add a new compartment to the selected transport.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Save compartment:</span> save the changes made to the selected compartment.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Delete compartment:</span> remove the selected compartment from the transport.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Capacity:</span> the capacity of the compartment.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Unit:</span> the unit of measure to use for the capacity of the compartment.</td>
                                </tr>
                            </table>
                            <p><span class="helpItem">In facility</span> </p>
                            <table>
                                <tr>
                                    <td><span class="helpItem">Facility:</span> is the facility that the in facility record was recorded against.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Last entered:</span> is the time that was recorded as entering a facility for the most recent in facility record. If the date was not recorded, then this section will not be displayed.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Last exited:</span> is the time that was recorded as exiting a facility for the most recent in facility record. If the date was not recorded, then this section will not be displayed.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Current status:</span> will display the current status of the most recent in facility record.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Set as exited facility:</span> will mark the current in facility entry for the transport as being out of the facility.</td>
                                </tr>
                            </table>
                        </td>
                        <td style="width: 50%;"><span class="helpItem">Interfaces</span>
                            <p>Interfaces to 3rd party software packages may require that interface settings for each transport that may be specified by the 3rd party software package be setup.</p>
                            <table>
                                <tr>
                                    <td><span class="helpItem">Interface settings:</span> contains a list of all the available interface setting records. To modify an existing interface setting record, select it from the drop down list. To add a new interface setting record, select "Add an interface" from the drop-down list.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Interface:</span> selects the interface that this transport interface settings correspond to.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Cross reference:</span> a cross reference used to identify the transport when received in an order or sent as a ticket through an interface with a 3rd party software package. Cross reference may be up to 50 characters in length.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Default setting:</span> used when there are multiple values set for an item for an interface, and the interface setting was not set for the item to be exported for this interface (exporting manually created orders, exporting to an interface from which it wasn&#39;t imported from, etc.).</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Export only:</span> used only for outbound tickets, to allow for multiple cross references available for different transports. If checked, then this cross reference will not be used for inbound interface lookups.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Save interface:</span> click to save the transport interface settings record.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Remove interface:</span> click to remove the selected transport interface settings record.</td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>

            </div>
            <p><span class="helpItem">Save:</span> saves the changes made to an existing transport is selected. Creates a new transport if "Enter a new transport" is selected in the transport drop-down list.</p>
            <p><span class="helpItem">Delete:</span> deletes the selected transport.</p>
        </div>
    </form>
</body>
</html>
