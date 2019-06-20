<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="BulkProductsHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.BulkProductsHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Help : Bulk Products</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="BulkProductsHelp" runat="server">
		<div>
			<span style="font-size: large; font-weight: bold;"><a href="help.aspx#BulkProducts">Help</a> : Bulk Products</span>
			<hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>Bulk product records represent bulk products that may be dispensed or functions that panels may perform.</p>
				<p><span class="helpItem">Bulk product drop-down list:</span> contains a list of all the bulk product records. To modify an existing bulk product record select it in the drop-down list. To enter a new bulk product record select "Enter a new bulk product" from the drop-down list.</p>
				<table>
					<tr>
						<td style="width: 50%; border-right: 1px solid #646464;"><span class="helpItem">General</span>
							<table>
								<tr>
									<td><span class="helpItem">Name:</span> the name of the product is required and may be up to 50 characters in length.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Owner:</span> assigning a specific owner to a bulk product is optional, but it is not recommended except for certain situations.
                                         <br />
										Considerations when assigning a specific owner to a bulk product:
                                          <br />
										<span style="padding-left: 2.0em;">1. Any inventory transactions that occur from a ticket being created from the dispensing of the bulk product will be assigned to the owner, even if the owner assigned to the loadout order is different.</span>
										<br />
										<span style="padding-left: 2.0em;">2. Additionally, the bulk product may be filtered out from selection lists if the user, container, or other items, are not assigned to the same owner or have access to all owners.</span>
									</td>
								</tr>
								<tr>
									<td><span class="helpItem">Default unit:</span> the unit that inventory is kept in by default.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Density:</span> the density (mass to volume relationship) of the bulk product.</td>
								</tr>
								<tr>
									<td><span class="helpItem">EPA number:</span> a reference to the EPA information for the bulk product. The EPA number field is optional.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Barcode number:</span> barcode identifier associated with the product.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Crop types:</span> the crop types that this bulk product may be used on.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Notes:</span> reference notes for the product. The notes field is optional.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Derived from:</span> specifies the source(s) of the bulk product, that may be displayed on the delivery ticket.</td>
								</tr>
								<tr>
									<td>
										<span class="helpItem">Track specific lot usage during dispensing:</span> specifies that a specific lot must be assigned to a dispensing usage, if any lots are available.
										<p class="indentedNote">This option will only be displayed if Item Traceability is enabled.</p>
									</td>
								</tr>
							</table>
						</td>
						<td style="width: 50%;"><span class="helpItem">Interfaces</span>
							<p>Interfaces to 3rd party software packages may require that interface settings for each bulk product that may be specified by the 3rd party software package be setup.</p>
							<table>
								<tr>
									<td><span class="helpItem">Interface settings:</span> contains a list of all the available interface setting records. To modify an existing interface setting record, select it from the drop down list. To add a new interface setting record, select "Add an interface" from the drop-down list.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Interface:</span> selects the interface that this product interface settings correspond to.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Cross reference:</span> a cross reference used to identify the bulk product when received in an order or sent as a ticket through an interface with a 3rd party software package. Cross reference may be up to 50 characters in length.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Interface exchange unit:</span> the unit of measure that the 3rd party software package will use when sending requests or reading delivered quantities. This field is only shown if the interface type has the setting <i>Show interface exchange unit of measure</i> is checked.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Default setting:</span> used when there are multiple values set for an item for an interface, and the interface setting was not set for the item to be exported for this interface (exporting manually created orders, exporting to an interface from which it wasn&#39;t imported from, etc.).</td>
								</tr>
								<tr>
									<td><span class="helpItem">Export only:</span> used only for outbound tickets, to allow for multiple cross references available for different bulk products. If checked, then this cross reference will not be used for inbound interface lookups.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Save interface:</span> click to save the bulk product interface settings record.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Remove interface:</span> click to remove the selected bulk product interface settings record.</td>
								</tr>
							</table>
						</td>
					</tr>
				</table>
			</div>
			<p><span class="helpItem">Save button:</span> located at the top of the page, click this button to save any changes made to a bulk product record or create a new bulk product record.</p>
			<p><span class="helpItem">Delete button:</span> located at the top of the page, click this button to delete the current bulk product record.</p>
		</div>
	</form>
</body>
</html>
