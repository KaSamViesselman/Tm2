<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="ProductsHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.ProductsHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <title>Help : Products</title>
    <link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
    <form id="ProductsHelp" runat="server">
        <div>
            <span style="font-size: large; font-weight: bold;"><a href="help.aspx#Products">Help</a>: Products</span>
            <hr style="width: 100%; color: #003399;" />
            <div id="divHelp">
                <p>Product records represent product that may be sold and dispensed as a part of an order.</p>
                <p><span class="helpItem">Product drop-down list:</span> contains a list of all the product records. To modify an existing product record, select it in the drop-down list. To enter a new product record select "Enter a new product" in the drop-down list.</p>
                <table>
                    <tr>
                        <td style="width: 50%; border-right: 1px solid #646464;"><span class="helpItem">General</span>
                            <table>
                                <tr>
                                    <td><span class="helpItem">Name:</span> the name of the product is required and may be up to 50 characters in length.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Owner:</span> assigning a specific owner to a product is optional, but it is not recommended except for certain situations.  
                                        <br />
                                        If an owner is selected, the product will only be available to users that are assigned to the same owner or have access to all owners.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Default unit:</span> unit that is first selected when this product is added to an order.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Notes:</span> reference notes for the product. The notes field is optional.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">EPA number:</span> a reference to the EPA information for the product. The EPA number field is optional.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">MSDS number:</span> a reference to the MSDS information for the product. The MSDS number field is optional.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Manufacturer:</span> a reference to the manufacturer of the product. The manufacturer field is optional.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Active ingredients:</span> a reference of the active ingredients used in the product. The active ingredients field is optional.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Restrictions:</span> a list of any restrictions for the product. The restrictions field is optional.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Max app. rate:</span> the maximum application rate suggested for the product. The maximum application rate field is optional.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Min app. rate:</span> the minimum application rate suggested for the product. The minimum application rate field is optional.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Do not stack (for weigh-tanks):</span> determines if the product may be blended with other products in a weigh-tank. If this option is checked, the system will discharge any product in the weigh-tank before measuring this product and discharge this product before measuring any other products.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Product Group:</span> used for filtering/display within Terminal Management 2 (tickets) and other supporting loadout software packages (Self Serve2, Plant Supervisor 4, etc.).</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Hazardous Material:</span> will flag this product as being a Hazardous Material. This can then be used on the ticket to display that it is a Hazardous Material.</td>
                                </tr>
                            </table>
                        </td>
                        <td style="width: 50%;"><span class="helpItem">Interfaces</span>
                            <p>Interfaces to 3rd party software packages require that interface settings for each product that may be specified by the 3rd party software package be setup.</p>
                            <table>
                                <tr>
                                    <td><span class="helpItem">Interface settings:</span> contains a list of all the available interface setting records. To modify an existing interface setting record, select it from the drop down list. To add a new interface setting record, select "Add an interface" from the drop-down list.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Interface:</span> selects the interface that this product interface settings correspond to.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Cross reference:</span> a cross reference used to identify the product when received in an order or sent as a ticket through an interface with a 3rd party software package. Cross reference may be up to 50 characters in length.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Interface exchange unit:</span> the unit of measure that the 3rd party software package will use when sending requests or reading delivered quantities. This field is only shown if the interface type has the setting <i>Show interface exchange unit of measure</i> is checked.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Order item unit:</span> the unit of measure that will be used when creating an order. This field is only shown if the interface type has the setting <i>Use the interface request unit of measure for the order item's requested unit of measure</i> unchecked.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Split product according to formulation at facility:</span> designates the facility to use to determine the formulation of the product when splitting the product into components. This will only be shown if the interface type ha the option selected to split the product into components.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Default setting:</span> used when there are multiple values set for an item for an interface, and the interface setting was not set for the item to be exported for this interface (exporting manually created orders, exporting to an interface from which it wasn&#39;t imported from, etc.).</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Export only:</span> used only for outbound tickets, to allow for multiple cross references available for different products. If checked, then this cross reference will not be used for inbound interface lookups.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Order item sort priority:</span> the number used for automatically sorting order items. the default value is 100. A number that is lower (closer to 1) will result in the order item being moved towards the top of the list. A number greater than 100 will move the order item towards the bottom of the list. If 2 items have the same priority, then their order relative to one another will remain the same.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Save interface:</span> click to save the product interface settings record.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Remove interface:</span> click to remove the selected product interface settings record.</td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
                <hr style="width: 100%; color: #646464; height: 1px" />
                <span class="helpItem">Bulk products</span>
                <p>A product is made up of one or more bulk products. Each facility may have its own "recipe" to create the same product. For example, one site may blend multiple bulk products to make the product while another facility may have that product ready made in storage as a bulk product. The bulk products and their ratio used to make the product are listed on the right side of the bulk product section.</p>
                <table>
                    <tr>
                        <td><span class="helpItem">Facility:</span> selects the facility to view the bulk products used to make the product at that facility.</td>
                    </tr>
                    <tr>
                        <td><span class="helpItem">Bulk product:</span> selects the bulk product to add, remove or modify.</td>
                    </tr>
                    <tr>
                        <td><span class="helpItem">Add product:</span> to add a new bulk product, select the bulk product in the bulk product drop-down list and click the "Add product" button.</td>
                    </tr>
                    <tr>
                        <td><span class="helpItem">Remove product:</span> to remove a product, select the bulk product in the bulk product drop-down list and click the "Remove product" button.</td>
                    </tr>
                    <tr>
                        <td><span class="helpItem">Percent of total:</span> to modify the percentage of a bulk product in the "recipe", select the bulk product in the bulk product drop-down list, enter the new percentage and click the "Update percentage" button.</td>
                    </tr>
                    <tr>
                        <td><span class="helpItem">Arrow up and arrow down:</span> will change the order that the bulk products will be dispensed on each panel.</td>
                    </tr>
                </table>
            </div>
            <p><span class="helpItem">Save button:</span> located at the top of the page, click this button to save any changes made to a product record or create a new product record.</p>
            <p><span class="helpItem">Delete button:</span> located at the top of the page, click this button to delete the current product record.</p>
        </div>
    </form>
</body>
</html>
