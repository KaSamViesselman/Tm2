<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="UnitsHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.UnitsHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Help : Units</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="UnitsHelp" runat="server">
		<div>
			<span style="font-size: large; font-weight: bold;"><a href="help.aspx#Units">Help</a>: Units</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>Unit of measures used in orders, reports, etc. may be configured. By default the following units are available: fluid ounces, gallons, kilograms, liters, ounces, pints, pounds, quarts, seconds, tons.</p>
				<p><span class="helpItem">Unit drop-down list:</span> located at the top of the page, this list contains all the unit of measure records.</p>
				<table>
					<tr>
						<td style="width: 50%; border-right: 1px solid #404040">
							<table>
								<tr>
									<td><span class="helpItem">Name:</span> the full name of the unit typically used when selecting the unit of measure. The name is required and may be up to 50 characters long.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Abbreviation:</span> the abbreviation used when displaying quantities with this unit of measure. The abbreviation is required and may be up to 10 characters long.<i>Example: "lb" is the abbreviation for the pounds unit of measure.</i></td>
								</tr>
								<tr>
									<td><span class="helpItem">Factor</span> how this unit of measure relates to a base unit of measure (selected in the drop-down list to the right). Factor is required and must be a numeric value greater than zero.<i>Example: the factor for a new unit of measure named "Cubic Feet" would be 7.48051948052 gal, because 1 cu.ft. = 7.48051948052 gal</i></td>
								</tr>
							</table>
						</td>
						<td style="width: 50%;"><span class="helpItem">Interfaces</span><p>Interfaces to 3rd party software packages may require that interface settings for each branch that may be specified by the 3rd party software package be setup.</p>
							<table>
								<tr>
									<td><span class="helpItem">Interface settings:</span> contains a list of all the available interface setting records. To modify an existing interface setting record, select it from the drop down list. To add a new interface setting record, select "Add an interface" from the drop-down list.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Interface:</span> selects the interface that this branch interface settings correspond to.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Cross reference:</span> a cross reference used to identify the branch when received in an order or sent as a ticket through an interface with a 3rd party software package. Cross reference may be up to 50 characters in length.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Default setting:</span> used when there are multiple values set for an item for an interface, and the interface setting was not set for the item to be exported for this interface (exporting manually created orders, exporting to an interface from which it wasn&#39;t imported from, etc.).</td>
								</tr>
								<tr>
									<td><span class="helpItem">Export only:</span> used only for outbound tickets, to allow for multiple cross references available for different branches. If checked, then this cross reference will not be used for inbound interface lookups.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Save interface:</span> click to save the branch interface settings record.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Remove interface:</span> click to remove the selected branch interface settings record.</td>
								</tr>
							</table>
						</td>
					</tr>
				</table>
				<p><span class="helpItem">Precision:</span> used to specify the number of digits to the left and to the right of the decimal place that should be displayed for quantities dispensed by this panel. Use the "+" and "-" buttons to add and remove decimal places to whole (left) and fractional (right) part of the number for each unit of measure.</p>
			</div>
			<p><span class="helpItem">Save button:</span> located at the top of the page, click this button to save any changes made to a unit record or create a new record.</p>
			<p><span class="helpItem">Delete button:</span> located at the top of the page, click this button to delete the current unit record.</p>
		</div>
	</form>
</body>
</html>
