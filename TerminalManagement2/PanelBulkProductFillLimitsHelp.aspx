<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="PanelBulkProductFillLimitsHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.PanelBulkProductFillLimitsHelp" %>

<!DOCTYPE html>
<html lang="en">
<head id="Head1" runat="server">
    <title>Help : Panel Bulk Product Fill Limits</title>
    <link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
    <form id="PanelBulkProductFillLimitsHelp" runat="server">
        <div>
            <span style="font-size: large; font-weight: bold;"><a href="help.aspx#PanelBulkProductFillLimits">Help</a> : Panel Bulk Product Fill Limits</span>
            <hr style="width: 100%; color: #003399;" />
            <div id="divHelp">
                <p>
                    Panel bulk product fill limits are used to limit the amount of a combination of bulk products that can be dispensed in a batch.
					<br />
                    An example of when this would be used is when there is a line blender system that is used to handle hand-adds which can only dispense so much product (limited by the capacity of the hopper that contains the hand add). A fill limit would be created that would contain all of the bulk products that are designated as hand adds for this panel, with the limit of the hand add hopper.
                </p>
                <p><span class="helpItem">Facility drop-down list:</span> contains all the facilities set up in the system. If a particular facility is selected, then only the panels that are assigned to that facility will be displayed.</p>
                <p><span class="helpItem">Panel drop-down list:</span> contains a list of all the panel records. To modify the fill limits assigned to an existing panel record, select it in the drop-down list.</p>
                <table>
                    <tr>
                        <td style="width: 50%; border-right: 1px solid #404040;">
                            <table>
                                <tr>
                                    <td><span class="helpItem">Bulk product fill limits:</span> the fill limits for bulk products that this panel is limited to dispensing per fill/discharge cycle. To add a new fill limit, click the "<em>Add fill limit</em>" button. Multiple of the same bulk product can be added to a panel, but only one similarly named bulk product can be added to each fill limit at a time. To remove a fill limit select the fill limit from the list (below the drop-down list) and click the "<em>Remove fill limit</em>" button.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Fill limit:</span> is the maximum quantity of  the bulk product(s) that may be delivered or routed through panel. If the quantity to be dispensed to or through this panel is larger than the fill limit, the quantity is split into batches. If the fill limit is 0 or there are no bulk products assigned, it is ignored.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Bulk product source from:</span> is where the panel should specify which bulk products to use for the fill limit. If the source is set to <i>Specify from list</i> then the user will specify which bulk products should be included in the fill limit. If it is set to a specific product number, or hand add, then the bulk products included in the fill limit will be determined based upon which bulk products have been assigned to the selected panel using the selected product number that are not disabled.</td>
                                </tr>
                                <tr>
                                    <td><span class="helpItem">Bulk products:</span> are the bulk products that this fill limit applies to. To add a bulk product select it from the drop-down list and click the "<em>Add bulk product</em>" button. Multiple of the same bulk product can be added to a panel, but only one similarly named bulk product can be enabled at a time. To remove a bulk product select the product from the list (below the drop-down list) and click the "<em>Remove bulk product</em>" button.</td>
                                </tr>
                            </table>
                        </td>
                        <td style="width: 50%;">&nbsp; 
                        </td>
                    </tr>
                </table>
            </div>
            <p><span class="helpItem">Save settings button:</span> located at the top of the page, click this to save bulk product/panel fill limit settings.</p>
        </div>
    </form>
</body>
</html>
