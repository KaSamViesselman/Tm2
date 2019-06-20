<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="ContainersHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.ContainersHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Help : Containers</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="form1" runat="server">
		<div>
			<span style="font-size: large; font-weight: bold;"><a href="help.aspx#Containers">Help</a>: Containers</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>Container records represent totes, mini-bulks, shuttles, etc. containing bulk product. EPA regulation 40 CFR 165.67 requires that the details for a container filled with chemical be tracked and a log of any activity on that container be kept. Whenever a container record is updated (whether in this page or through another application a log entry is created detailing any changes and when those changes were made).</p>
				<p><span class="helpItem">Container drop-down list:</span> located at the top of the page, this list contains all the container records. When creating a new container, select "Enter a new container" before entering the container details.</p>
				<table>
					<tr>
						<td style="width: 50%; border-right: 1px solid #404040;">
							<table>
								<tr>
									<td><span class="helpItem">Number:</span> a number that is used to identify the container. Typically this corresponds to a barcode that can be scanned by container filling/tracking software. The container number is required and may contain up to 50 characters (alpha-numeric)</td>
								</tr>
								<tr>
									<td><span class="helpItem">Container type:</span> a reference field used to identify what type of container this record represents. The container type is optional. Container types are user defined under the "Container Types" tab</td>
								</tr>
								<tr>
									<td><span class="helpItem">Volume:</span> the maximum volume that the container is able to hold. The volume is used by the container filling software to suggest to the operator how much should be filled into a container. Volume must be a numeric value greater than or equal to zero.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Empty weight:</span> the weight of the container without any bulk product. The empty weight is used by the container filling software to determine how much product is already in a container when filling or returning it. Empty weight must be a numeric value greater than or equal to zero.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Condition:</span> a reference field used to track the physical condition of a container. There are four choices: excellent, good, fair and poor.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Seal Number:</span> the number from the container's seal.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Notes:</span> a reference field for any notes on the container. This field is optional.</td>
								</tr>
								<tr>
									<td><span class="helpItem">In service:</span> the date that the container was put into service.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Last inspected:</span> the date that the container was last inspected. The last inspected date is used in the "All Containers Report" to highlight any containers that are approaching or have exceeded the number of days between inspections (configured in "General Settings") and also by the container filling software to prevent the operator from filling a container that hasn't been inspected.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Passed inspection:</span> a switch that indicates if the container has passed inspection. Used by the container filling software to prevent the operator from filling a container that has failed an inspection.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Manufactured:</span> the date that the container was manufactured.</td>
								</tr>
							</table>
						</td>
						<td style="width: 50%;">
							<table>
								<tr>
									<td><span class="helpItem">Owner:</span> the owner of the container. If an owner is selected, then only users setup for that owner (or for all owners) will be able to access the container record. The owner is optional.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Facility:</span> the facility that the container is currently at (when the status indicates "in facility") or was last at (when the status indicates "in transit" or "in customer custody").</td>
								</tr>
								<tr>
									<td><span class="helpItem">Status:</span> a reference field to indicate if the container is in facility, in transit or in customer custody.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Last filled:</span> the date and time that the container was last filled. The container filling software will automatically update this when the container is filled.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Last cleaned:</span> the date and time that the container was last cleaned.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Bulk product:</span> the product that is currently in or was last in the container.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Product weight:</span> the quantity of product currently in the container. The container filling software will automatically update this when the container is filled. The product weight must be a numeric value greater than or equal to zero.</td>
								</tr>
								<tr>
									<td><span class="helpItem">For order:</span> the order that this container was or will be filled for. The order selection is optional.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Last ticket:</span> the last ticket that the product in this container was sold under. The clear button (to the right) may be used to clear the last ticket reference.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Refillable:</span> indicates if the container is refillable.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Passed pressure test:</span> indicates if the container has passed its last pressure test.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Seal broken:</span> indicates if the seal on the container's filling port has been broken indicating possible contamination.</td>
								</tr>
								<tr>
									<td><span class="helpItem">One-way valve present:</span> indicates if the container is equipped with a one-way valve.</td>
								</tr>
								<tr>
									<td>
										<span class="helpItem">Lot number:</span> specifies the current lot number assigned to the container. The operator may select the option to "<span style="font-style: italic;">Create new lot</span>" where a new lot number can be created.
										<p class="indentedNote">This option will only be displayed if Item Traceability is enabled.</p>
									</td>
								</tr>
							</table>
						</td>
					</tr>
					<tr>
						<td>
							<table>
								<tr>
									<td>
										<h2>Product Change</h2>
										This section will display the settings for any inventory change records that may be created.
									</td>
								</tr>
								<tr>
									<td>
										<span class="helpItem">Default owner:</span> is the default owner that will be used if there is not an owner specified for the bulk product or the container. 
										<br />
										<span class="indentedNote">Note: If "Not specified" is selected, and there is not an owner specified for the bulk product or the container, then not inventory transactions will be created.</span>
									</td>
								</tr>
								<tr>
									<td>
										<span class="helpItem">Facility Inventory:</span> is the facility where the change in bulk product inventory will be assigned to.
									</td>
								</tr>
								<tr>
									<td>
										<span class="helpItem">Packaged Inventory:</span> is the virtual facility that the filled container inventory will be assigned to.
									</td>
								</tr>
							</table>
						</td>
						<td>&nbsp;</td>
					</tr>
				</table>
			</div>
			<p><span class="helpItem">Save button:</span> located at the top of the page, click this button to save any changes made to the container record or create a new container record when "Enter a new container" is selected in the containers drop-down list at the top of the page.</p>
			<p><span class="helpItem">Delete button:</span> located at the top of the page, click this button to delete the current container record.</p>
		</div>
	</form>
</body>
</html>
