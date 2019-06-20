<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="PanelsHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.PanelsHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <title>Help : Panels</title>
    <link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
    <form id="PanelsHelp" runat="server">
        <div>
            <span style="font-size: large; font-weight: bold;"><a href="help.aspx#Panels">Help</a>: Panels</span>
            <hr style="width: 100%; color: #003399;" />
            <div id="divHelp">
                <p>Panel records represent a panel with some type of controller. Examples include:</p>
                <ul>
                    <li>KA-2000 and KA-2000-5 controllers are used to control valves, pumps gates, etc., while reading some type of meter (mass-flow or volumetric flow) or scale (platform, truck, weigh-tank, weigh-hopper or weigh-mixer) to dispense product.</li>
                    <li>Tank level monitor (TLM) controllers used to read the physical level of product in a tank using some type of level sensor, such as pressure, ultrasonic, guided wave radar, etc.</li>
                    <li>KA-2000 controllers used to interface with another vendor's dispensing equipment such as line blenders and some loss in weight systems.</li>
                </ul>
                <p><span class="helpItem">Facility drop-down list:</span> contains all the facilities set up in the system. If a particular facility is selected, then only the panels that are assigned to that facility will be displayed.</p>
                <p><span class="helpItem">Panel drop-down list:</span> contains a list of all the panel records. To modify an existing panel record, select it in the drop-down list. To enter a new panel record, select "Enter a new panel" in the drop-down list.</p>
                <table>
                    <tr>
                        <td style="width: 50%; border-right: 1px solid #646464;">
                            <table>
                                <tr>
                                    <td><span class="helpItem">Name:</span> the name of the panel. The name is required and may be up to 50 characters in length.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Facility:</span> the facility where the panel is located. This is used by dispensing and tank level monitor software to determine which panels to access. Facility is optional, but if the facility is not selected it may not be available for use by dispensing or tank level monitor software.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Role:</span> the role of the panel is used to determine how the panel should be used:<ul>
                                        <li><i>Bulk-weigher</i>: a dry tower system (BW-200) used to run multiple batches of a single product using a Bulk weigher system.</li>
                                        <li><i>Declining weight hopper</i>: a component in a dry tower system where the amount of product dispensed is measured by the amount of weight loss from the scale platform.</li>
                                        <li><i>Dry tower mixer</i>: a component of a dry tower system (FDS-2850, 2856, 2866) used to blend dry/granular product and is typically paired with a dry tower weigh-hopper. Product is not measured directly into a dry tower mixer since it has no measurement system. The mixer typically has a gate used to dump product into a transport or vessel.</li>
                                        <li><i>Dry tower surge tank</i>: a component of a dry tower system (FDS-2856, 2866) used to hold dry/granular product until it is ready to be loaded into a transport or some type of a vessel and is typically paired with a dry tower weigh-hopper or dry tower weigh-mixer. Product is not measured directly into a dry tower surge hopper since it has no measurement system. The surge hopper typically has a gate used to dump product into a transport.</li>
                                        <li><i>Dry tower weigh-hopper</i>: a component of a dry tower system (FDS-2870, 2850, 2851, 2886, 2888) used to measure dry/granular product. The mass in the weigh-hopper is read by a scale and the weigh-hopper typically includes a gate used to dump product into a transport or a vessel.</li>
                                        <li><i>Dry tower weigh-hopper (pair)</i>: a component of a dry tower system (FDS-2856) used to measure dry/granular product. The mass in the weigh-hopper is read by a scale and the weigh-hopper typically includes a gate used to dump product into a transport or a vessel.</li>
                                        <li><i>Dry tower weigh-hoppers (combined)</i>: a component of a dry tower system (FDS-2886, 2888) used to measure dry/granular product. The mass in the weigh-hoppers are read by a scale and the weigh-hoppers typically include a gate used to dump product into a mixer. The combined weigh-hoppers used with this role will receive a list of products to dispense, and will assign them to each individual weigh-hopper at the time of dispensing.</li>
                                        <li><i>Dry tower weigh-mixer</i>: a component of a dry tower system (FDS-2852, 2853, 2858) used to measure and blend dry/granular product. The mass in the weigh-hopper is read by a scale and the weigh-mixer typically includes a gate used to dump product into a transport or a vessel.</li>
                                        <li><i>Line blender</i>: a component in a dry tower system where the amounts to be dispensed are sent to a 3rd party automation system, and the final dispensed results are returned to the Kahler system. These systems typically allow for dispensing of multiple products at a time.</li>
                                        <li><i>Line blender with weigh-hopper</i>: a component in a dry tower system where the amounts to be dispensed are sent to a 3rd party automation system, and the final dispensed results are returned to the Kahler system. These systems typically allow for dispensing of multiple products at a time. The amount dispensed into the weigh hopper will be prorated across each of the individual products.</li>
                                        <li><i>Liquid blender</i>: a weigh-tank system (FDS-2650) used for blending liquid products. The mass in the weigh-tank is read by a scale and weigh-tank typically includes an agitator used to mix the product together.</li>
                                        <li><i>Meter (mass-flow/volumetric)</i>: a liquid dispensing system (FDS-2550, 2750) used to measure liquid product. In the case of a mass-flow system (FDS-2750) product mass is measured using a mass-flow meter. In the case of a volumetric system (FDS-2550) product volume is measured using a volumetric flow meter.</li>
                                        <li><i>Multi-meter (mass-flow)</i>: a liquid dispensing system (FDS-2751) used to measure liquid product. Product mass is measured using one or more mass-flow meters where only one product may be measured at a time.</li>
                                        <li><i>Multi-meter simultaneous (mass-flow)</i>: a liquid dispensing system (FDS-2755) used to measure liquid product. Product mass is measured using one or more mass-flow meters where multiple products may be measured simultaneously.</li>
                                        <li><i>Multi-meter simultaneous (volumetric)</i>: a liquid dispensing system (FDS-2552, 2555) used to measure liquid product. Product volume is measured using one or more volumetric meters where multiple products may be measured simultaneously.</li>
                                        <li><i>Rotary mixer</i>: a component of a dry system (FDS-2870, 2850) used to blend dry/granular product and is typically paired with a dry tower weigh-hopper. Product is not measured directly in the rotary mixer since it has not measurement system.</li>
                                        <li><i>Platform scale</i>: a measuring system using a scale (FDS-2400). The system can measure an increase in weight (product being added to the scale) or a loss in weight (product being taken from the scale).</li>
                                        <li><i>Seed system hopper</i>: a component of a seed treater system (PSG) used to measure seed. The mass in the weigh-hopper is read by a scale and the weigh-hopper typically includes a gate used to dump product into a transport or a vessel.</li>
                                        <li><i>Seed system treater</i>: a component of a seed treater system (PSG) used to apply a chemical treatment to seeds. The treatment chemical is sent to the Seed Treater Controller, which applies the amount of product using preset rates.</li>
                                        <li><i>TLM-5</i>: a tank level monitor system. This panel uses level sensors (pressure, ultrasonic, guided wave radar, etc.) to measure the physical level of product in a tank.</li>
                                        <li><i>Truck scale</i>: a scale system used for loading trucks (FDS-2400). The system can measure an increase in weight (product being added to the truck) or a loss in weight (product being taken from the truck).</li>
                                        <li><i>Weigh-tank</i>: a scale system used to measure the mass of liquid product (FDS-2300, 3000).</li>
                                    </ul>
                                    </td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Rank:</span> determines the order that panels should be used when multiple panels are used in a load. Typically systems that should run first or feed into another should have a lower number rank.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Follow by percent:</span> the follow by percent is used to stage systems that should follow a system with a lower number rank. An example might be a chemical inject system on a dry fertilizer tower that should wait until 10% of the dry fertilizer is in the mixer before dispensing the chemical to the mixer.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Rinse threshold:</span> used by weigh-tanks (FDS-2300, 2650) to determine when to turn on the rinse output to rinse the tank as product is discharged from the tank. Quantity is specified in the mass unit of measure that the weigh-tank's scale operates in.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Hold threshold:</span> used to hold back the specified quantity of product when dispensing with more than one system so that the remainder of the product may be used to flush out the line. An example might be a liquid fertilizer system that has chemical injection. The liquid fertilizer may be configured to hold a quantity of product to ensure the last product through the line is the fertilizer/carrier.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Mass unit:</span> the unit of measure used by the panel to measure mass.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Volume unit:</span> the unit of measure used by the panel to measure volume.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Use reservations:</span> when enabled, loading/receiving software will use reservations to coordinate usage of an Ethernet connected panel between multiple applications and/or computers. If the panel is reserved the computer name and application name will be listed as well as a clear button that may be used to clear the reservation manually.</td>
                                </tr>
                            </table>
                        </td>
                        <td style="width: 50%;">
                            <table>
                                <tr>
                                    <td><span class="helpItem">Connection type:</span> the connection type used to communicate with the panel. Emulate may be used to simulate a panels behavior without actually running any equipment.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Controller number:</span> the ID assigned to a controller. Typically these are fixed for different panel types and should only be assigned by a Kahler Automation technician.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">System address:</span> the base address used by a controller. Typically these are fixed for different panel types and should only be assigned by a Kahler Automation technician.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Emulation rate:</span> available when the connection type is set to emulate. This controls the speed at which the system simulates dispensing product.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">IP address:</span> available when the connection type is set to Ethernet. This is the address on the network assigned to the panel. The configure link provides a shortcut to the panel's built in configuration pages.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">TCP port:</span> available when the connection type is set to Ethernet. This is the TCP/IP port used to communicate with the panel over Ethernet. The port number is typically either 2000 (encapsulated MODBUS/RTU over TCP, used by KA-2000-5s that are able to dispense product) or 502 (MODBUS/TCP, used by TLM and KA-2025).</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Serial port:</span> available when the connection type is set to serial. This is the RS-232 port on the computer that should be used to communicate with the panel.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Baud rate:</span> available when the connection type is set to serial. This is the data rate that the computer should use when communicating with the panel. Typically this is 19200 for KA-2000 and KA-2000-5.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Parity:</span> available when the connection type is set to serial. This is the parity to use when communicating with the panel. Typically this is set to none for KA-2000 and KA-2000-5.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Data bits:</span> available when the connection type is set to serial. This is the data bit count to use when communicating with the panel. Typically this is set to 8 for KA-2000 and KA-2000-5.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Stop bits:</span> available when the connection type is set to serial. This is the stop bit count to use when communicating with the panel. Typically this is set to 1 for KA-2000 and KA-2000-5.</td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
                <table>
                    <tr>
                        <td style="width: 50%;">
                            <table>
                                <tr>
                                    <td><span class="helpItem">Unit precision:</span> used to specify the number of digits to the left and to the right of the decimal place that should be displayed for quantities dispensed by this panel. Use the "+" and "-" buttons to add and remove decimal places to whole (left) and fractional (right) part of the number for each unit of measure.</td>
                                </tr>
                            </table>
                        </td>
                        <td style="width: 50%;">
                            <table>
                                <tr>
                                    <td><span class="helpItem">Unit precision calculations based on minimum scale/meter division:</span> is used to assist th end user in determining what the best unit precision values that should be used based upon the smallest division that the metering device is able to report.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Minimum scale/meter division:</span> is the smallest division that the metering device is able to report.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Typical product density:</span> is an amount used to help convert from mass to volume units for calculations.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Calculate recommendations:</span> will recalculate the unit precision values using the parameters defined.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Set panel precision:</span> will assign the calculated unit precision values to the panel for the units displayed.</td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </div>
            <p><span class="helpItem">Save button:</span> located at the top of the page, click this button to save any changes made to a panel record or create a new panel record.</p>
            <p><span class="helpItem">Delete button:</span> located at the top of the page, click this button to delete the current panel record.</p>
        </div>
    </form>
</body>
</html>
