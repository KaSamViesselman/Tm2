<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="Help.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.Help" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Help</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
	<script type="text/javascript" src="Scripts/jquery-1.11.1.min.js">
	</script>
	<script type="text/javascript">
		function loadHelp(divObjectName, pageLink) {
			var divObject = $('#' + divObjectName);
			divObject.load(pageLink + " #divHelp");
		}
	</script>
	<script type="text/javascript">
		$(document).ready(function () {
			loadHelp('divApplicators', 'ApplicatorsHelp.aspx');
			loadHelp('divBranches', 'BranchesHelp.aspx');
			loadHelp('divCarriers', 'CarriersHelp.aspx');
			loadHelp('divAllCarriersReport', 'AllCarriersHelp.aspx');
			loadHelp('divContainers', 'ContainersHelp.aspx');
			loadHelp('divContainerTypes', 'ContainerTypesHelp.aspx');
			loadHelp('divContainerEquipment', 'ContainerEquipmentHelp.aspx');
			loadHelp('divContainerEquipmentTypes', 'ContainerEquipmentTypesHelp.aspx');
			loadHelp('divAllContainersReport', 'AllContainersHelp.aspx');
			loadHelp('divAllContainerEquipmentReport', 'AllContainerEquipmentHelp.aspx');
			loadHelp('divContainerInventory', 'ContainerInventoryHelp.aspx');
			loadHelp('divCropTypes', 'CropTypesHelp.aspx');
			loadHelp('divCustomPages', 'CustomPagesHelp.aspx');
			loadHelp('divAccounts', 'AccountsHelp.aspx');
			loadHelp('divAccountDestinations', 'AccountDestinationsHelp.aspx');
			loadHelp('divAccountCoupling', 'AccountCouplingHelp.aspx');
			loadHelp('divAllCustomersReport', 'CustomerMasterFileHelp.aspx');
			loadHelp('divDrivers', 'DriversHelp.aspx');
			loadHelp('divDriversInFacility', 'DriverReportHelp.aspx');
			loadHelp('divDriversInFacilityHistory', 'DriverHistoryHelp.aspx');
			loadHelp('divAllDriversReport', 'AllDriversHelp.aspx');
			loadHelp('divFacilities', 'FacilitiesHelp.aspx');
			loadHelp('divBays', 'BaysHelp.aspx');
			loadHelp('divStorageLocations', 'StorageLocationsHelp.aspx')
			loadHelp('divCustomLoadQuestions', 'CustomLoadQuestionsHelp.aspx');
			loadHelp('divTracks', 'TracksHelp.aspx');
			loadHelp('divGeneralSettings', 'GeneralSettingsHelp.aspx');
			loadHelp('divOrderSettings', 'OrderSettingsHelp.aspx');
			loadHelp('divEmailSettings', 'EmailSettingsHelp.aspx');
			loadHelp('divAnalysisSettings', 'AnalysisSettingsHelp.aspx');
			loadHelp('divAccountCouplingSettings', 'AccountCouplingSettingsHelp.aspx');
			loadHelp('divReceivingPoSettings', 'ReceivingPoSettingsHelp.aspx');
			loadHelp('divContainerSettings', 'ContainerSettingsHelp.aspx');
			loadHelp('divTicketSettings', 'TicketSettingsHelp.aspx');
			loadHelp('divDefaultDeliveryWebTicketSettings', 'DefaultDeliveryWebTicketSettingsHelp.aspx');
			loadHelp('divDefaultOrderSummarySettings', 'DefaultOrderSummarySettingsHelp.aspx');
			loadHelp('divDefaultReceivingWebTicketSettings', 'DefaultReceivingWebTicketSettingsHelp.aspx');
			loadHelp('divDefaultReceivingWebPickTicketSettings', 'DefaultReceivingWebPickTicketSettingsHelp.aspx');
			loadhelp('divDefaultContainerLabelSettings', 'DefaultContainerLabelSettingsHelp.aspx')
			loadHelp('divInterfaces', 'InterfacesHelp.aspx');
			loadHelp('divInterfaceTypes', 'InterfaceTypesHelp.aspx');
			loadHelp('divInterfaceItems', 'InterfaceItemsHelp.aspx');
			loadHelp('divInterfaceUsageReport', 'InterfaceUsageReportHelp.aspx');
			loadHelp('divInterfaceAssignOrder', 'InterfaceAssignOrderHelp.aspx');
			loadHelp('divInterfaceTicketStatus', 'InterfaceTicketStatusHelp.aspx');
			loadHelp('divInterfaceReceivingTicketStatus', 'InterfaceReceivingTicketStatusHelp.aspx');
			loadHelp('divInventory', 'InventoryHelp.aspx');
			loadHelp('divInventoryChangeReport', 'InventoryChangeReportHelp.aspx');
			loadHelp('divInventoryGroups', 'InventoryGroupsHelp.aspx');
			loadHelp('divOrders', 'OrdersHelp.aspx');
			loadHelp('divPastOrders', 'PastOrdersHelp.aspx');
			loadHelp('divOrderList', 'OrderListHelp.aspx');
			loadHelp('divStagedOrders', 'StagedOrdersHelp.aspx');
			loadHelp('divInProgressRecordsHelp', 'InProgressRecordsHelp.aspx');
			loadHelp('divDeleteOrdersHelp', 'DeleteOrdersHelp.aspx');
			loadHelp('divArchiveOrdersHelp', 'ArchiveOrdersHelp.aspx');
			loadHelp('divArchiveTicketsHelp', 'ArchiveTicketsHelp.aspx');
			loadHelp('divPointOfSale', 'PointOfSaleHelp.aspx');
			loadHelp('divOwners', 'OwnersHelp.aspx');
			loadHelp('divPanels', 'PanelsHelp.aspx');
			loadHelp('divPanelGroupsHelp', 'PanelGroupsHelp.aspx');
			loadHelp('divDischargeLocations', 'DischargeLocationsHelp.aspx');
			loadHelp('divPanelBulkProductSettings', 'PanelBulkProductSettingsHelp.aspx');
			loadHelp('divPanelBulkProducts', 'PanelBulkProductsHelp.aspx');
			loadHelp('divPanelBulkProductFillLimits', 'PanelBulkProductFillLimitsHelp.aspx');
			loadHelp('divProducts', 'ProductsHelp.aspx');
			loadHelp('divBulkProducts', 'BulkProductsHelp.aspx');
			loadHelp('divBulkProductAnalysis', 'BulkProductAnalysisHelp.aspx');
			loadHelp('divProductAllocation', 'ProductAllocationHelp.aspx');
			loadHelp('divBulkProductAllocation', 'BulkProductAllocationHelp.aspx');
			loadHelp('divProductList', 'ProductListHelp.aspx');
			loadHelp('divProductGroups', 'ProductGroupsHelp.aspx');
			loadHelp('divLots', 'LotsHelp.aspx');
			loadHelp('divReceivingPurchaseOrders', 'ReceivingHelp.aspx');
			loadHelp('divPastReceivingPurchaseOrders', 'PastReceivingHelp.aspx');
			loadHelp('divReceivingPurchaseOrdersList', 'ReceivingPurchaseOrderListHelp.aspx');
			loadHelp('divSuppliers', 'SuppliersHelp.aspx');
			loadHelp('divDeleteReceivingPurchaseOrders', 'DeleteReceivingPurchaseOrdersHelp.aspx');
			loadHelp('divArchiveReceivingPurchaseOrders', 'ArchiveReceivingPurchaseOrdersHelp.aspx');
			loadHelp('divArchiveReceivingPurchaseOrderTickets', 'ArchiveReceivingPurchaseOrderTicketsHelp.aspx'); // 'ArchiveReceivingPurchaseOrderTicketsHelp.aspx');
			loadHelp('divCustomerActivityReport', 'CustomerActivityReportHelp.aspx');
			loadHelp('divReceivingActivityReport', 'ReceivingActivityReportHelp.aspx');
			loadHelp('divReceipts', 'ReceiptsHelp.aspx');
			loadHelp('divTrackReport', 'TrackReportHelp.aspx');
			loadHelp('divEmailReports', 'EmailReportsHelp.aspx');
			loadHelp('divEmails', 'EmailsHelp.aspx');
			loadHelp('divBulkProductUsageReport', 'BulkProductUsageReportHelp.aspx');
			loadHelp('divApplicationUsage', 'ApplicationUsageHelp.aspx');
			loadHelp('divEventLog', 'EventLogHelp.aspx');
			loadHelp('divTankLevels', 'TankLevelsHelp.aspx');
			loadHelp('divTanks', 'TanksHelp.aspx');
			loadHelp('divTankAnalysis', 'TankAnalysisHelp.aspx');
			loadHelp('divTankGroups', 'TankGroupsHelp.aspx');
			loadHelp('divTankLevelTrends', 'TankLevelTrendsHelp.aspx');
			loadHelp('divTankAlarmHistory', 'TankAlarmHistoryHelp.aspx');
			loadHelp('divTransports', 'TransportsHelp.aspx');
			loadHelp('divTransportTypes', 'TransportTypesHelp.aspx');
			loadHelp('divTransportsInFacility', 'TransportReportHelp.aspx');
			loadHelp('divTransportsInFacilityHistory', 'TransportHistoryHelp.aspx');
			loadHelp('divTransportsUsageReport', 'TransportUsageReportHelp.aspx');
			loadHelp('divAllTransportsReport', 'AllTransportsHelp.aspx');
			loadHelp('divTransportTrackingReport', 'TransportTrackingReportHelp.aspx');
			loadHelp('divTransportInspectionQuestions', 'TransportInspectionQuestionsHelp.aspx');
			loadHelp('divUnits', 'UnitsHelp.aspx');
			loadHelp('divUsers', 'UsersHelp.aspx');
			loadHelp('divUserProfiles', 'UserProfilesHelp.aspx');
			loadHelp('divAdvancedSearch', 'AdvancedSearchHelp.aspx');
			loadHelp('divAbout', 'AboutHelp.aspx');
		})
	</script>
	<style type="text/css">
		#tblIndex td {
			text-indent: 0em;
			padding-left: 0em;
		}
	</style>
</head>
<body>
	<form id="Help" runat="server">
		<div>
			<table>
				<tr>
					<td style="text-align: left; font-size: large; font-weight: bold;">&nbsp;
					</td>
					<td style="text-align: center; font-size: large; font-weight: bold;">
						<asp:Label ID="lblVersion" runat="server" Text="Terminal Management 2 Version:"></asp:Label>
					</td>
					<td style="text-align: right; vertical-align: bottom;">
						<a href="#Index" style="font-weight: normal; font-size: medium;">Index</a>
					</td>
				</tr>
			</table>
			<nav class="floating" bottom right>
				<a href="#Index" class="button">d</a><a href="#Top" class="button">u</a>
			</nav>
			<hr />
			<br />
			<%--Applicators--%>
			<a name="Applicators" class="HelpSubjects">Applicators</a>
			<div id="divApplicators" runat="server">
			</div>
			<hr />
			<%--Branches--%>
			<a name="Branches" class="HelpSubjects">Branches</a>
			<div id="divBranches" runat="server">
			</div>
			<hr />
			<%--Carriers--%>
			<a name="Carriers" class="HelpSubjects">Carriers</a>
			<div id="divCarriers" runat="server">
			</div>
			<hr />
			<a name="AllCarriersReport" class="HelpSubjects">All Carriers Report</a>
			<div id="divAllCarriersReport" runat="server">
			</div>
			<hr />
			<%--Containers--%>
			<a name="Containers" class="HelpSubjects">Containers</a>
			<div id="divContainers" runat="server">
			</div>
			<hr />
			<a name="ContainerTypes" class="HelpSubjects">Container Types</a>
			<div id="divContainerTypes" runat="server">
			</div>
			<hr />
			<a name="ContainerEquipment" class="HelpSubjects">Container Equipment</a>
			<div id="divContainerEquipment" runat="server">
			</div>
			<hr />
			<a name="ContainerEquipmentTypes" class="HelpSubjects">Container Equipment Types</a>
			<div id="divContainerEquipmentTypes" runat="server">
			</div>
			<hr />
			<a name="AllContainersReport" class="HelpSubjects">All Containers Report</a>
			<div id="divAllContainersReport" runat="server">
			</div>
			<hr />
			<a name="AllContainerEquipmentReport" class="HelpSubjects">All Container Equipment Report</a>
			<div id="divAllContainerEquipmentReport" runat="server">
			</div>
			<hr />
			<a name="ContainerInventory" class="HelpSubjects">Container Inventory</a>
			<div id="divContainerInventory" runat="server">
			</div>
			<hr />
			<%--Crops Types--%>
			<a name="CropTypes" class="HelpSubjects">Crop Types</a>
			<div id="divCropTypes" runat="server">
			</div>
			<hr />
			<%--Custom Pages--%>
			<a name="CustomPages" class="HelpSubjects">Custom Pages</a>
			<div id="divCustomPages" runat="server">
			</div>
			<hr />
			<%--Customer Accounts--%>
			<a name="Accounts" class="HelpSubjects">Accounts</a>
			<div id="divAccounts" runat="server">
			</div>
			<hr />
			<a name="AccountDestinations" class="HelpSubjects">Account Destinations</a>
			<div id="divAccountDestinations" runat="server">
			</div>
			<hr />
			<a name="AccountCoupling" class="HelpSubjects">Account Coupling</a>
			<div id="divAccountCoupling" runat="server">
			</div>
			<hr />
			<a name="AllCustomersReport" class="HelpSubjects">All Customers Report</a>
			<div id="divAllCustomersReport" runat="server">
			</div>
			<hr />
			<%--Drivers--%>
			<a name="Drivers" class="HelpSubjects">Drivers</a>
			<div id="divDrivers" runat="server">
			</div>
			<hr />
			<a name="DriversInFacility" class="HelpSubjects">Drivers In Facility</a>
			<div id="divDriversInFacility" runat="server">
			</div>
			<hr />
			<a name="DriversInFacilityHistory" class="HelpSubjects">Drivers In Facility History</a>
			<div id="divDriversInFacilityHistory" runat="server">
			</div>
			<hr />
			<a name="AllDriversReport" class="HelpSubjects">All Drivers Report</a>
			<div id="divAllDriversReport" runat="server">
			</div>
			<hr />
			<%--Facilities--%>
			<a name="Facilities" class="HelpSubjects">Facilities</a>
			<div id="divFacilities" runat="server">
			</div>
			<hr />
			<a name="Bays" class="HelpSubjects">Bays</a>
			<div id="divBays" runat="server">
			</div>
			<hr />
			<a name="StorageLocations" class="HelpSubjects">Storage Locations</a>
			<div id="divStorageLocations" runat="server">
			</div>
			<hr />
			<a name="CustomLoadQuestions" class="HelpSubjects">Custom Load Questions</a>
			<div id="divCustomLoadQuestions" runat="server">
			</div>
			<hr />
			<a name="Tracks" class="HelpSubjects">Tracks</a>
			<div id="divTracks" runat="server">
			</div>
			<hr />
			<%--General Settings--%>
			<a name="GeneralSettings" class="HelpSubjects">General Settings </a>
			<div id="divGeneralSettings" runat="server">
			</div>
			<hr />
			<a name="OrderSettings" class="HelpSubjects">Order Settings </a>
			<div id="divOrderSettings" runat="server">
			</div>
			<hr />
			<a name="EmailSettings" class="HelpSubjects">E-mail Settings </a>
			<div id="divEmailSettings" runat="server">
			</div>
			<hr />
			<a name="AnalysisSettings" class="HelpSubjects">Analysis Settings </a>
			<div id="divAnalysisSettings" runat="server">
			</div>
			<hr />
			<a name="AccountCouplingSettings" class="HelpSubjects">Account Coupling Settings </a>
			<div id="divAccountCouplingSettings" runat="server">
			</div>
			<hr />
			<a name="ReceivingPoSettings" class="HelpSubjects">Receiving PO Settings </a>
			<div id="divReceivingPoSettings" runat="server">
			</div>
			<hr />
			<a name="ContainerSettings" class="HelpSubjects">Container Settings </a>
			<div id="divContainerSettings" runat="server">
			</div>
			<hr />
			<a name="TicketSettings" class="HelpSubjects">Ticket Settings </a>
			<div id="divTicketSettings" runat="server">
			</div>
			<hr />
			<a name="DefaultDeliveryWebTicketSettings" class="HelpSubjects">Default Delivery Web Ticket Settings </a>
			<div id="divDefaultDeliveryWebTicketSettings" runat="server">
			</div>
			<hr />
			<a name="DefaultOrderSummarySettings" class="HelpSubjects">Default Order Summary Settings </a>
			<div id="divDefaultOrderSummarySettings" runat="server">
			</div>
			<hr />
			<a name="DefaultReceivingWebTicketSettings" class="HelpSubjects">Default Receiving Web Ticket Settings </a>
			<div id="divDefaultReceivingWebTicketSettings" runat="server">
			</div>
			<hr />
			<a name="DefaultReceivingWebPickTicketSettings" class="HelpSubjects">Default Receiving Web Pick Ticket Settings </a>
			<div id="divDefaultReceivingWebPickTicketSettings" runat="server">
			</div>
			<hr />
			<a name="DefaultContainerLabelSettings" class="HelpSubjects">Default Container Label Settings</a>
			<div id="divDefaultContainerLabelSettings" runat="server">
			</div>
			<hr />
			<%--Interfaces--%>
			<a name="Interfaces" class="HelpSubjects">Interfaces</a>
			<div id="divInterfaces" runat="server">
			</div>
			<hr />
			<a name="InterfaceTypes" class="HelpSubjects">Interface Types</a>
			<div id="divInterfaceTypes" runat="server">
			</div>
			<hr />
			<a name="InterfaceItems" class="HelpSubjects">Interface Items</a>
			<div id="divInterfaceItems" runat="server">
			</div>
			<hr />
			<a name="InterfaceUsageReport" class="HelpSubjects">Interface Usage Report</a>
			<div id="divInterfaceUsageReport" runat="server">
			</div>
			<hr />
			<a name="InterfaceAssignOrder" class="HelpSubjects">Assign Interface to Orders</a>
			<div id="divInterfaceAssignOrder" runat="server">
			</div>
			<hr />
			<a name="InterfaceTicketStatus" class="HelpSubjects">Interface Ticket Export Status</a>
			<div id="divInterfaceTicketStatus" runat="server">
			</div>
			<hr />
			<a name="InterfaceReceivingTicketStatus" class="HelpSubjects">Interface Receiving Ticket Export Status</a>
			<div id="divInterfaceReceivingTicketStatus" runat="server">
			</div>
			<hr />
			<%--Inventory--%>
			<a name="Inventory" class="HelpSubjects">Inventory</a>
			<div id="divInventory" runat="server">
			</div>
			<hr />
			<a name="InventoryChangeReport" class="HelpSubjects">Inventory Change Report</a>
			<div id="divInventoryChangeReport" runat="server">
			</div>
			<hr />
			<a name="InventoryGroups" class="HelpSubjects">Inventory Groups</a>
			<div id="divInventoryGroups" runat="server">
			</div>
			<hr />
			<%--Orders--%>
			<a name="Orders" class="HelpSubjects">Orders</a>
			<div id="divOrders" runat="server">
			</div>
			<hr />
			<a name="PastOrders" class="HelpSubjects">Past Orders</a>
			<div id="divPastOrders" runat="server">
			</div>
			<hr />
			<a name="OrderList" class="HelpSubjects">Order List</a>
			<div id="divOrderList" runat="server">
			</div>
			<hr />
			<a name="StagedOrders" class="HelpSubjects">Staged Orders</a>
			<div id="divStagedOrders" runat="server">
			</div>
			<hr />
			<a name="InProgressRecords" class="HelpSubjects">In Progress Records</a>
			<div id="divInProgressRecordsHelp" runat="server">
			</div>
			<hr />
			<a name="DeleteOrders" class="HelpSubjects">Delete Orders</a>
			<div id="divDeleteOrdersHelp" runat="server">
			</div>
			<hr />
			<a name="ArchiveOrders" class="HelpSubjects">Archive Orders</a>
			<div id="divArchiveOrdersHelp" runat="server">
			</div>
			<hr />
			<a name="ArchiveTickets" class="HelpSubjects">Archive Tickets</a>
			<div id="divArchiveTicketsHelp" runat="server">
			</div>
			<hr />
			<a name="PointOfSale" class="HelpSubjects">Point Of Sale</a>
			<div id="divPointOfSale" runat="server">
			</div>
			<hr />
			<%--Owners--%>
			<a name="Owners" class="HelpSubjects">Owners</a>
			<div id="divOwners" runat="server">
			</div>
			<hr />
			<%--Panels--%>
			<a name="Panels" class="HelpSubjects">Panels</a>
			<div id="divPanels" runat="server">
			</div>
			<hr />
			<a name="DischargeLocations" class="HelpSubjects">Discharge Locations</a>
			<div id="divDischargeLocations" runat="server">
			</div>
			<hr />
			<a name="PanelGroups" class="HelpSubjects">Panel Groups</a>
			<div id="divPanelGroupsHelp" runat="server">
			</div>
			<hr />
			<a name="PanelBulkProductSettings" class="HelpSubjects">Panel Bulk Product Settings</a>
			<div id="divPanelBulkProductSettings" runat="server">
			</div>
			<hr />
			<a name="PanelBulkProducts" class="HelpSubjects">Panel Bulk Products</a>
			<div id="divPanelBulkProducts" runat="server">
			</div>
			<hr />
			<a name="PanelBulkProductFillLimits" class="HelpSubjects">Panel Bulk Product Fill Limits</a>
			<div id="divPanelBulkProductFillLimits" runat="server">
			</div>
			<hr />
			<%--Products--%>
			<a name="Products" class="HelpSubjects">Products</a>
			<div id="divProducts" runat="server">
			</div>
			<hr />
			<a name="BulkProducts" class="HelpSubjects">Bulk Products</a>
			<div id="divBulkProducts" runat="server">
			</div>
			<hr />
			<a name="BulkProductAnalysis" class="HelpSubjects">Bulk Product Analysis</a>
			<div id="divBulkProductAnalysis" runat="server">
			</div>
			<hr />
			<a name="ProductAllocation" class="HelpSubjects">Product Allocation</a>
			<div id="divProductAllocation" runat="server">
			</div>
			<hr />
			<a name="BulkProductAllocation" class="HelpSubjects">Bulk Product Allocation</a>
			<div id="divBulkProductAllocation" runat="server">
			</div>
			<hr />
			<a name="ProductList" class="HelpSubjects">Product List</a>
			<div id="divProductList" runat="server">
			</div>
			<hr />
			<a name="ProductGroups" class="HelpSubjects">Product Groups</a>
			<div id="divProductGroups" runat="server">
			</div>
			<hr />
			<a name="Lots" class="HelpSubjects">Lots</a>
			<div id="divLots" runat="server">
			</div>
			<hr />
			<%--Receiving Purchase Orders--%>
			<a name="ReceivingPurchaseOrders" class="HelpSubjects">Receiving Purchase Orders</a>
			<div id="divReceivingPurchaseOrders" runat="server">
			</div>
			<hr />
			<a name="PastReceivingPurchaseOrders" class="HelpSubjects">Past Receiving Purchase Orders</a>
			<div id="divPastReceivingPurchaseOrders" runat="server">
			</div>
			<hr />
			<a name="ReceivingPurchaseOrdersList" class="HelpSubjects">Receiving Purchase Orders List</a>
			<div id="divReceivingPurchaseOrdersList" runat="server">
			</div>
			<hr />
			<a name="Suppliers" class="HelpSubjects">Suppliers</a>
			<div id="divSuppliers" runat="server">
			</div>
			<hr />
			<a name="DeleteReceivingPurchaseOrders" class="HelpSubjects">Delete Receiving Purchase Orders</a>
			<div id="divDeleteReceivingPurchaseOrders" runat="server">
			</div>
			<hr />
			<a name="ArchiveReceivingPurchaseOrders" class="HelpSubjects">Archive Receiving Purchase Orders</a>
			<div id="divArchiveReceivingPurchaseOrders" runat="server">
			</div>
			<hr />
			<a name="ArchiveReceivingPurchaseOrderTickets" class="HelpSubjects">Archive Receiving Purchase Order Tickets</a>
			<div id="divArchiveReceivingPurchaseOrderTickets" runat="server">
			</div>
			<hr />
			<%--Reports--%>
			<a name="CustomerActivityReport" class="HelpSubjects">Customer Activity Report</a>
			<div id="divCustomerActivityReport" runat="server">
			</div>
			<hr />
			<a name="ReceivingActivityReport" class="HelpSubjects">Receiving Activity Report</a>
			<div id="divReceivingActivityReport" runat="server">
			</div>
			<hr />
			<a name="Receipts" class="HelpSubjects">Receipts</a>
			<div id="divReceipts" runat="server">
			</div>
			<hr />
			<a name="TrackReport" class="HelpSubjects">Track Report</a>
			<div id="divTrackReport" runat="server">
			</div>
			<hr />
			<a name="EmailReports" class="HelpSubjects">E-mail Reports</a>
			<div id="divEmailReports" runat="server">
			</div>
			<hr />
			<a name="Emails" class="HelpSubjects">E-mails</a>
			<div id="divEmails" runat="server">
			</div>
			<hr />
			<a name="BulkProductUsageReport" class="HelpSubjects">Bulk Product Usage Report</a>
			<div id="divBulkProductUsageReport" runat="server">
			</div>
			<hr />
			<a name="ApplicationUsage" class="HelpSubjects">Application Usage</a>
			<div id="divApplicationUsage" runat="server">
			</div>
			<hr />
			<a name="EventLog" class="HelpSubjects">Event Log</a>
			<div id="divEventLog" runat="server">
			</div>
			<hr />
			<%--Tanks--%>
			<a name="TankLevels" class="HelpSubjects">Tank Levels</a>
			<div id="divTankLevels" runat="server">
			</div>
			<hr />
			<a name="Tanks" class="HelpSubjects">Tanks</a>
			<div id="divTanks" runat="server">
			</div>
			<hr />
			<a name="TankAnalysis" class="HelpSubjects">Tank Analysis</a>
			<div id="divTankAnalysis" runat="server">
			</div>
			<hr />
			<a name="TankGroups" class="HelpSubjects">Tank Groups</a>
			<div id="divTankGroups" runat="server">
			</div>
			<hr />
			<a name="TankLevelTrends" class="HelpSubjects">Tank Level Trends</a>
			<div id="divTankLevelTrends" runat="server">
			</div>
			<hr />
			<a name="TankAlarmHistory" class="HelpSubjects">Tank Alarm History</a>
			<div id="divTankAlarmHistory" runat="server">
			</div>
			<hr />
			<%--Transports--%>
			<a name="Transports" class="HelpSubjects">Transports</a>
			<div id="divTransports" runat="server">
			</div>
			<hr />
			<a name="TransportTypes" class="HelpSubjects">Transport Types</a>
			<div id="divTransportTypes" runat="server">
			</div>
			<hr />
			<a name="TransportsInFacility" class="HelpSubjects">Transports In Facility</a>
			<div id="divTransportsInFacility" runat="server">
			</div>
			<hr />
			<a name="TransportsInFacilityHistory" class="HelpSubjects">Transports In Facility History</a>
			<div id="divTransportsInFacilityHistory" runat="server">
			</div>
			<hr />
			<a name="TransportsUsageReport" class="HelpSubjects">Transports Usage Report</a>
			<div id="divTransportsUsageReport" runat="server">
			</div>
			<hr />
			<a name="AllTransportsReport" class="HelpSubjects">All Transports Report</a>
			<div id="divAllTransportsReport" runat="server">
			</div>
			<hr />
			<a name="TransportTrackingReport" class="HelpSubjects">Transport Tracking Report</a>
			<div id="divTransportTrackingReport" runat="server">
			</div>
			<hr />
			<a name="TransportInspectionQuestions" class="HelpSubjects">Transport Inspection Questions</a>
			<div id="divTransportInspectionQuestions" runat="server">
			</div>
			<hr />
			<%--Units--%>
			<a name="Units" class="HelpSubjects">Units</a>
			<div id="divUnits" runat="server">
			</div>
			<hr />
			<%--Users--%>
			<a name="Users" class="HelpSubjects">Users</a>
			<div id="divUsers" runat="server">
			</div>
			<hr />
			<a name="UserProfiles" class="HelpSubjects">User Profiles</a>
			<div id="divUserProfiles" runat="server">
			</div>
			<hr />
			<%--AdvancedSearch--%>
			<a name="AdvancedSearch" class="HelpSubjects">Advanced Search</a>
			<div id="divAdvancedSearch" runat="server">
			</div>
			<hr />
			<%--About--%>
			<a name="About" class="HelpSubjects">About</a>
			<div id="divAbout" runat="server">
			</div>
			<hr />
			<br />
			<br />
			<br />
			<a name="Index">Index</a>
			<table id="tblIndex" runat="server">
				<tr>
					<td class="IndexSection">Applicators
					</td>
					<td style="width: 30%;">
						<a href="#Applicators" class="IndexItem">Applicators</a>
					</td>
					<td class="IndexSection">Branches
					</td>
					<td style="width: 30%;">
						<a href="#Branches" class="IndexItem">Branches</a>
					</td>
				</tr>
				<tr>
					<td style="font-size: large">Carriers
					</td>
					<td>
						<a href="#Carriers" class="IndexItem">Carriers</a><br />
						<a href="#AllCarriersReport" class="IndexItem">All Carriers Report</a>
					</td>
					<td style="font-size: large">Containers
					</td>
					<td>
						<a href="#Containers" class="IndexItem">Containers</a><br />
						<a href="#ContainerTypes" class="IndexItem">Container Types</a><br />
						<a href="#ContainerEquipment" class="IndexItem">Container Equipment</a><br />
						<a href="#ContainerEquipmentTypes" class="IndexItem">Container Equipment Types</a><br />
						<a href="#AllContainersReport" class="IndexItem">All Containers Report</a><br />
						<a href="#AllContainerEquipmentReport" class="IndexItem">All Container Equipment Report</a><br />
						<a href="#ContainerInventory" class="IndexItem">Container Inventory</a>
					</td>
				</tr>
				<tr>
					<td style="font-size: large">Crops Types
					</td>
					<td>
						<a href="#CropTypes" class="IndexItem">Crop Types</a>
					</td>
					<td style="font-size: large">Custom Pages
					</td>
					<td>
						<a href="#CustomPages" class="IndexItem">Custom Pages</a>
					</td>
				</tr>
				<tr>
					<td style="font-size: large">Customer Accounts
					</td>
					<td>
						<a href="#Accounts" class="IndexItem">Accounts</a><br />
						<a href="#AccountDestinations" class="IndexItem">Account Destinations</a><br />
						<a href="#AccountCoupling" class="IndexItem">Account Coupling</a><br />
						<a href="#AllCustomersReport" class="IndexItem">All Customers Report</a>
					</td>
					<td style="font-size: large">Drivers
					</td>
					<td>
						<a href="#Drivers" class="IndexItem">Drivers</a><br />
						<a href="#DriversInFacility" class="IndexItem">Drivers In Facility</a><br />
						<a href="#DriversInFacilityHistory" class="IndexItem">Drivers In Facility History</a><br />
						<a href="#AllDriversReport" class="IndexItem">All Drivers Report</a>
					</td>
				</tr>
				<tr>
					<td style="font-size: large">Facilities
					</td>
					<td>
						<a href="#Facilities" class="IndexItem">Facilities</a><br />
						<a href="#Bays" class="IndexItem">Bays</a><br />
						<a href="#StorageLocations" class="IndexItem">Storage Locations</a><br />
						<a href="#CustomLoadQuestions" class="IndexItem">Custom Load Questions</a><br />
						<a href="#Tracks" class="IndexItem">Tracks</a>
					</td>
					<td style="font-size: large">General Settings
					</td>
					<td>
						<a href="#GeneralSettings" class="IndexItem">General Settings</a><br />
						<a href="#OrderSettings" class="IndexItem">Order Settings</a><br />
						<a href="#EmailSettings" class="IndexItem">E-mail Settings</a><br />
						<a href="#AnalysisSettings" class="IndexItem">Analysis Settings</a><br />
						<a href="#AccountCouplingSettings" class="IndexItem">Account Coupling Settings</a><br />
						<a href="#ReceivingPoSettings" class="IndexItem">Receiving PO Settings</a><br />
						<a href="#ContainerSettings" class="IndexItem">Container Settings</a><br />
						<a href="#TicketSettings" class="IndexItem">Ticket Settings</a><br />
						<a href="#DefaultDeliveryWebTicketSettings" class="IndexItem">Default Delivery Web Ticket Settings</a><br />
						<a href="#DefaultOrderSummarySettings" class="IndexItem">Default Order Summary Settings</a><br />
						<a href="#DefaultReceivingWebTicketSettings" class="IndexItem">Default Receiving Web Ticket Settings</a>
						<a href="#DefaultReceivingWebPickTicketSettings" class="IndexItem">Default Receiving Web Pick Ticket Settings</a>
						<a href="#DefaultContainerLabelSettings" class="IndexItem">Default Container Label Settings</a>
					</td>
				</tr>
				<tr>
					<td style="font-size: large">Interfaces
					</td>
					<td>
						<a href="#Interfaces" class="IndexItem">Interfaces</a><br />
						<a href="#InterfaceTypes" class="IndexItem">Interface Types</a><br />
						<a href="#InterfaceItems" class="IndexItem">Interface Items</a><br />
						<a href="#InterfaceUsageReport" class="IndexItem">Interface Usage Report</a><br />
						<a href="#InterfaceAssignOrder" class="IndexItem">Assign Interface to Orders</a><br />
						<a href="#InterfaceTicketStatus" class="IndexItem">Interface Ticket Export Status</a><br />
						<a href="#InterfaceReceivingTicketStatus" class="IndexItem">Interface Receiving Ticket Export Status</a>
					</td>
					<td style="font-size: large">Inventory
					</td>
					<td>
						<a href="#Inventory" class="IndexItem">Inventory</a><br />
						<a href="#InventoryChangeReport" class="IndexItem">Inventory Change Report</a><br />
						<a href="#InventoryGroups" class="IndexItem">Inventory Groups</a>
					</td>
				</tr>
				<tr>
					<td style="font-size: large">Orders
					</td>
					<td>
						<a href="#Orders" class="IndexItem">Orders</a><br />
						<a href="#PastOrders" class="IndexItem">Past Orders</a><br />
						<a href="#OrderList" class="IndexItem">Order List</a><br />
						<a href="#StagedOrders" class="IndexItem">Staged Orders</a><br />
						<a href="#InProgressRecords" class="IndexItem">In Progress Records</a><br />
						<a href="#DeleteOrders" class="IndexItem">Delete Orders</a><br />
						<a href="#ArchiveOrders" class="IndexItem">Archive Orders</a><br />
						<a href="#ArchiveTickets" class="IndexItem">Archive Tickets</a><br />
						<a href="#PointOfSale" class="IndexItem">Point Of Sale</a>
					</td>
					<td style="font-size: large">Owners
					</td>
					<td>
						<a href="#Owners" class="IndexItem">Owners</a>
					</td>
				</tr>
				<tr>
					<td style="font-size: large">Panels
					</td>
					<td>
						<a href="#Panels" class="IndexItem">Panels</a><br />
						<a href="#DischargeLocations" class="IndexItem">Discharge Locations</a><br />
						<a href="#PanelGroups" class="IndexItem">Panel Groups</a><br />
						<a href="#PanelBulkProductSettings" class="IndexItem">Panel Bulk Product Settings</a><br />
						<a href="#PanelBulkProducts" class="IndexItem">Panel Bulk Products</a><br />
						<a href="#PanelBulkProductFillLimits" class="IndexItem">Panel Bulk Product Fill Limits</a>
					</td>
					<td style="font-size: large">Products
					</td>
					<td>
						<a href="#Products" class="IndexItem">Products</a><br />
						<a href="#BulkProducts" class="IndexItem">Bulk Products</a><br />
						<a href="#BulkProductAnalysis" class="IndexItem">Bulk Product Analysis</a><br />
						<a href="#ProductAllocation" class="IndexItem">Product Allocation</a><br />
						<a href="#BulkProductAllocation" class="IndexItem">Bulk Product Allocation</a><br />
						<a href="#ProductList" class="IndexItem">Product List</a><br />
						<a href="#ProductGroups" class="IndexItem">Product Groups</a>
						<a href="#Lots" class="IndexItem">Lots</a>
					</td>
				</tr>
				<tr>
					<td style="font-size: large">Receiving Purchase Orders
					</td>
					<td>
						<a href="#ReceivingPurchaseOrders" class="IndexItem">Receiving Purchase Orders</a><br />
						<a href="#PastReceivingPurchaseOrders" class="IndexItem">Past Receiving Purchase Orders</a><br />
						<a href="#ReceivingPurchaseOrdersList" class="IndexItem">Receiving Purchase Orders List</a><br />
						<a href="#Suppliers" class="IndexItem">Suppliers</a><br />
						<a href="#DeleteReceivingPurchaseOrders" class="IndexItem">Delete Receiving Purchase Orders</a><br />
						<a href="#ArchiveReceivingPurchaseOrders" class="IndexItem">Archive Receiving Purchase Orders</a><br />
						<a href="#ArchiveReceivingPurchaseOrderTickets" class="IndexItem">Archive Receiving Purchase Order Tickets</a>
					</td>
					<td style="font-size: large">Reports
					</td>
					<td>
						<a href="#CustomerActivityReport" class="IndexItem">Customer Activity Report</a><br />
						<a href="#ReceivingActivityReport" class="IndexItem">Receiving Activity Report</a>
						<br />
						<a href="#Receipts" class="IndexItem">Receipts</a><br />
						<a href="#TrackReport" class="IndexItem">Track Report</a><br />
						<a href="#BulkProductUsageReport" class="IndexItem">Bulk Product Usage Report</a><br />
						<a href="#ApplicationUsage" class="IndexItem">Application Usage</a><br />
						<a href="#EventLog" class="IndexItem">Event Log</a><br />
						<a href="#EmailReports" class="IndexItem">E-mail Reports</a><br />
						<a href="#Emails" class="IndexItem">E-mails</a>
					</td>
				</tr>
				<tr>
					<td style="font-size: large">Tanks
					</td>
					<td>
						<a href="#TankLevels" class="IndexItem">Tank Levels</a><br />
						<a href="#Tanks" class="IndexItem">Tanks</a><br />
						<a href="#TankAnalysis" class="IndexItem">Tank Analysis</a><br />
						<a href="#TankGroups" class="IndexItem">Tank Groups</a><br />
						<a href="#TankLevelTrends" class="IndexItem">Tank Level Trends</a><br />
						<a href="#TankAlarmHistory" class="IndexItem">Tank Alarm History</a>
					</td>
					<td style="font-size: large">Transports
					</td>
					<td>
						<a href="#Transports" class="IndexItem">Transports</a><br />
						<a href="#TransportTypes" class="IndexItem">Transport Types</a><br />
						<a href="#TransportsInFacility" class="IndexItem">Transports In Facility</a><br />
						<a href="#TransportsInFacilityHistory" class="IndexItem">Transports In Facility History</a><br />
						<a href="#TransportsUsageReport" class="IndexItem">Transports Usage Report</a><br />
						<a href="#AllTransportsReport" class="IndexItem">All Transports Report</a><br />
						<a href="#TransportTrackingReport" class="IndexItem">Transport Tracking Report</a><br />
						<a href="#TransportInspectionQuestions" class="IndexItem">Transport Inspection Questions</a>
					</td>
				</tr>
				<tr>
					<td style="font-size: large">Units
					</td>
					<td>
						<a href="#Units" class="IndexItem">Units</a>
					</td>
					<td style="font-size: large">Users
					</td>
					<td>
						<a href="#Users" class="IndexItem">Users</a><br />
						<a href="#UserProfiles" class="IndexItem">User Profiles</a>
					</td>
				</tr>
				<tr>
					<td style="font-size: large"></td>
					<td>
						<a href="#AdvancedSearch" class="IndexItem">Advanced Search</a>
					</td>
					<td style="font-size: large"></td>
					<td></td>
				</tr>
				<tr>
					<td style="font-size: large"></td>
					<td>
						<a href="#About" class="IndexItem">About</a>
					</td>
					<td style="font-size: large"></td>
					<td></td>
				</tr>
			</table>
		</div>
	</form>
</body>
</html>
