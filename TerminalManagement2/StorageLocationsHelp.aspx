<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="StorageLocationsHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.StorageLocationsHelp" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>Help : Storage Locations</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="StorageLocationsHelp" runat="server">
		<div>
			<span style="font-size: large; font-weight: bold;"><a href="help.aspx#StorageLocations">Help</a>: Storage Locations</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>Storage locations are used for determining item traceability. When a bulk product is received, or manufactured, it is placed into a storage location. Then, when a bulk product is dispensed, it will generate a record to assist in determining the source(s) that may be included in the load.</p>
				<p><span class="helpItem">Show storage locations of type:</span> located at the top of the page, this list allows the user to specify what type of storage locations to display. By default, storage locations are created for tanks and containers</p>
				<p><span class="helpItem">Storage Location:</span> located at the top of the page, this list contains all the storage locations that are of the specified storage location type.</p>
				<table>
					<tr>
						<td style="width: 50%; border-right: 1px solid #404040">
							<h2>General</h2>
							<table>
								<tr>
									<td><span class="helpItem">Name:</span> the full name of the storage location. The name is required and may be up to 50 characters long.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Facility:</span> is the facility associated with the storage location. This field is only displayed for <span style="font-style: italic;">Bulk product storage</span> types.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Tank:</span> is the tank associated with the storage location. This field is only displayed for <span style="font-style: italic;">Tank storage</span> types.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Container:</span> is the container associated with the storage location. This field is only displayed for <span style="font-style: italic;">Container storage</span> types.</td>
								</tr>
							</table>
						</td>
						<td style="width: 50%; border-right: 1px solid #404040">
							<h2>Clean-out/empty</h2>
							<table>
								<tr>
									<td><span class="helpItem">Date of last clean-out entry:</span> is the last stored clean-out date for the storage location. If ther has not been a clean-out date specified, it will be displayed as <span style="font-style: italic;">N/A</span></td>
								</tr>
								<tr>
									<td><span class="helpItem">New clean-out date:</span> is the date to specify that a storage location was observed to be cleaned out.</td>
								</tr>
								<tr>
									<td>
										<span class="helpItem">Mark as cleaned out button:</span> is located after the clean out date. Click this button to set the clean-out date for the specified storage location. 
									</td>
								</tr>
							</table>
							<h2>Storage location transfer</h2>
							<table>
								<tr>
									<td><span class="helpItem">Transfer to:</span> is the storage location that storage location contents were moved to.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Transfer start:</span> is the date and time that them movement started.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Transfer end:</span> is the date and time that them movement completed.</td>
								</tr>
							</table>
						</td>
					</tr>
				</table>
			</div>
			<p><span class="helpItem">Save button:</span> located at the top of the page, click this button to save any changes made to a storage location record or create a new record.</p>
			<p><span class="helpItem">Delete button:</span> located at the top of the page, click this button to delete the current storage location record.</p>
		</div>
	</form>
</body>
</html>
