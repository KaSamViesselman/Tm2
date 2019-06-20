<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="DefaultDeliveryWebTicketSettings.aspx.vb"
	MaintainScrollPositionOnPostback="true" Inherits="KahlerAutomation.TerminalManagement2.DefaultDeliveryWebTicketSettings" %>

<!DOCTYPE html>
<html lang="en">
<head id="Head1" runat="server">
	<title>General Settings : Default Delivery Web Ticket Settings</title>
	<link rel="shortcut icon" href="FavIcon.ico" />
	<meta name="viewport" content="width=device-width" />
	<meta http-equiv="X-UA-Compatible" content="IE=edge" />
	<link rel="stylesheet" href="styles/site.css" />
	<link rel="stylesheet" href="styles/site-tablet.css" media="(max-width:768px)" />
	<link rel="stylesheet" href="styles/site-phone.css" media="(max-width:480px)" />
	<script type="text/javascript" src="scripts/jquery-1.11.1.min.js"></script>
	<script type="text/javascript" src="scripts/soap-1.0.1.js"></script>
	<script type="text/javascript" src="scripts/page-controller.js"></script>
	<script type="text/javascript">
		function pageLoad(sender, args) {
			var sm = Sys.WebForms.PageRequestManager.getInstance();
			if (!sm.get_isInAsyncPostBack()) {
				sm.add_beginRequest(onBeginRequest);
				sm.add_endRequest(onRequestDone);
			}
		}

		function onBeginRequest(sender, args) {
			var send = args.get_postBackElement().value;
			$find('PleaseWaitPopup').show();
			document.getElementById('pnlRecordControl').style.visibility = "hidden";
			document.getElementById('pnlGeneralInformation').style.visibility = "hidden";
		}

		function onRequestDone(sender, args) {
			$find('PleaseWaitPopup').hide();
			document.getElementById('pnlRecordControl').style.visibility = "visible";
			document.getElementById('pnlGeneralInformation').style.visibility = "visible";

			if (args.get_error() != undefined) {
				var errorMessage = args.get_error().message
				args.set_errorHandled(true);
				alert(errorMessage);
			}
		}
	</script>
</head>
<body>
	<form id="main" method="post" runat="server" defaultfocus="cbxSeparateTicketNumberPerOwner">
		<asp:ScriptManager ID="ToolkitScriptManager1" runat="server" AsyncPostBackTimeout="3600" AsyncPostBackErrorMessage="Timeout while retrieving staged order data." EnablePartialRendering="true" OnAsyncPostBackError="ToolkitScriptManager1_AsyncPostBackError">
		</asp:ScriptManager>
		<asp:Panel ID="PleaseWaitMessagePanel" runat="server" CssClass="modalPopup" Style="height: 50px; width: 125px; text-align: center;">
			Please wait<br />
			<img src="images/ajax-loader.gif" alt="Please wait" />
		</asp:Panel>
		<asp:Button ID="HiddenButton" runat="server" Text="Hidden Button" Style="visibility: hidden;" ToolTip="Necessary for Modal Popup Extender" />
		<ajaxToolkit:ModalPopupExtender ID="PleaseWaitPopup" BehaviorID="PleaseWaitPopup" runat="server" TargetControlID="HiddenButton" PopupControlID="PleaseWaitMessagePanel" BackgroundCssClass="modalBackground">
		</ajaxToolkit:ModalPopupExtender>
		<asp:UpdatePanel ID="PleaseWaitPanel" runat="server" RenderMode="Inline">
			<ContentTemplate>
				<div class="recordSelection">
					<ul>
						<li>
							<label>
								Ticket owner
							</label>
							<asp:DropDownList ID="ddlWebTicketOwner" runat="server" AutoPostBack="true">
							</asp:DropDownList>
						</li>
						<li>
							<label>
							</label>
							<asp:Label ID="lblWebTicketSettingsExist" runat="server" Text="Settings exist" ForeColor="#FF3300"
								Visible="False"></asp:Label>
						</li>
					</ul>
				</div>
				<div id="pnlRecordControl" class="recordControl">
					<asp:Button ID="btnSaveOwnerWebTicketSettings" runat="server" Text="Save Owner Web Ticket Settings" />
					<asp:Button ID="btnDeleteOwnerWebTicketSettings" runat="server" Text="Delete Owner Web Ticket Settings" />
				</div>
				<div id="pnlGeneralInformation" class="section">
					<div class="sectionEven">
						<ul>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="chkNtepCompliant" runat="server" Text="NTEP compliant" />
							</li>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="chkAcresOnTicket" runat="server" Text="Show acres" />
							</li>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="chkShowApplicationRateOnTicket" runat="server" Text="Show application rate"
									AutoPostBack="true" />
							</li>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="chkUseOriginalOrdersApplicationRate" runat="server" Text="Use order's application rate"
									Style="margin-left: 2em;" />
							</li>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="chkShowApplicatorOnTicket" runat="server" Text="Show applicator" />
							</li>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="chkDisplayBlendGroupNameAsProductName" runat="server" Text="Display blend group name as product name" />
							</li>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="chkShowBulkProductSummaryTotals" runat="server" AutoPostBack="true"
									Text="Show bulk product summary totals" />
							</li>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="chkShowBulkProductNotesSummaryTotals" runat="server" Text="Show bulk product notes in summary totals"
									Style="margin-left: 2em;" />
							</li>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="chkShowBulkProductEpaNumberSummaryTotals" runat="server" Text="Show bulk product EPA number in summary totals"
									Style="margin-left: 2em;" />
							</li>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="chkBranchLocation" runat="server" Text="Show branch location" />
							</li>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="chkCarrierId" runat="server" Text="Show carrier" />
							</li>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="chkShowCompartmentsOnTicket" runat="server" AutoPostBack="true"
									Text="Show compartments" />
							</li>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="chkShowCompartmentProductNotes" runat="server" Text="Show product notes"
									Style="margin-left: 2em;" />
							</li>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="chkShowCompartmentBulkIngredients" runat="server" Text="Show bulk ingredients in compartment"
									AutoPostBack="true" Style="margin-left: 2em;" />
							</li>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="chkShowCompartmentBulkIngredientNotes" runat="server" Text="Show bulk ingredient notes"
									Style="margin-left: 4em;" />
							</li>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="chkShowCompartmentBulkIngredientEpaNumber" runat="server" Text="Show bulk ingredient EPA number"
									Style="margin-left: 4em;" />
							</li>
							<li id="pnlShowCompartmentBulkIngredientLotNumber" runat="server">
								<label>
								</label>
								<asp:CheckBox ID="chkShowCompartmentBulkIngredientLotNumber" runat="server" Text="Show bulk ingredient lot numbers assigned"
									Style="margin-left: 4em;" />
							</li>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="chkShowCompartmentTotals" runat="server" Text="Show compartment totals"
									Style="margin-left: 2em;" />
							</li>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="chkShowCompartmentLoadedIndex" runat="server" Text="Show compartment loaded index"
									Style="margin-left: 2em;" />
							</li>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="chkCustomer" runat="server" Text="Show customer" />
							</li>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="chkShowCustomerDestinationNotesOnTicket" runat="server" Text="Show customer destination notes" />
							</li>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="chkShowCustomerNotesOnTicket" runat="server" Text="Show customer notes" />
							</li>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="chkDate" runat="server" Text="Show date and " AutoPostBack="true" />
								<asp:CheckBox ID="chkShowTime" runat="server" Text="show time" />
							</li>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="chkShowDensityOnTicket" runat="server" Text="Show density on ticket" />
							</li>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="chkShowDerivedFrom" runat="server" Text="Show derived from on ticket" />
							</li>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="chkShowDischargeLocationOnTicket" runat="server" Text="Show discharge location(s)" />
							</li>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="chkDriverName" runat="server" AutoPostBack="true" Text="Show driver" />
							</li>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="chkDriverNumber" runat="server" Text="Show driver number" Style="margin-left: 2em;" />
							</li>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="chkEmailAddress" runat="server" Text="Show e-mail address" />
							</li>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="chkShowFacility" runat="server" Text="Show facility" />
							</li>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="chkShowFertilizerGuaranteedAnalysis" runat="server" Text="Show fertilizer guaranteed analysis"
									AutoPostBack="true" />
							</li>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="chkShowFertilizerGuaranteedAnalysisByCompartment" runat="server" Text="Show analysis by compartment for Do Not Blend orders" Style="margin-left: 2em;" />
							</li>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="chkShowFertilizerGrade" runat="server" Text="Show fertilizer grade"
									AutoPostBack="true" />
							</li>
							<li id="pnlAnalysisDecimalCount" runat="server">
								<label>
								</label>
								<ul class="input">
									<li>
										<asp:CheckBox ID="chkShowCompartmentFertilizerGrade" runat="server" Text="Show fertilizer grade by compartment for Do Not Blend orders"
											Style="margin-left: 2em;" />
									</li>
									<li style="margin-left: 2em; vertical-align: top; display: flex;">Number of decimals
							to display if analysis is greater than or equal to 1%
							<asp:DropDownList ID="ddlGradeAnalysisDecimalCountGreaterThanOne" runat="server"
								Style="width: auto; height: 1.5em; min-width: 3em;">
								<asp:ListItem Text="0" Value="0"></asp:ListItem>
								<asp:ListItem Text="1" Value="1"></asp:ListItem>
								<asp:ListItem Text="2" Value="2"></asp:ListItem>
								<asp:ListItem Text="3" Value="3"></asp:ListItem>
								<asp:ListItem Text="4" Value="4"></asp:ListItem>
							</asp:DropDownList>
									</li>
									<li style="margin-left: 2em; vertical-align: top; display: flex;">Number of decimals
							to display if analysis is less than 1%
							<asp:DropDownList ID="ddlGradeAnalysisDecimalCountLessThanOne" runat="server" Style="width: auto; height: 1.5em; min-width: 3em;">
								<asp:ListItem Text="0" Value="0"></asp:ListItem>
								<asp:ListItem Text="1" Value="1"></asp:ListItem>
								<asp:ListItem Text="2" Value="2"></asp:ListItem>
								<asp:ListItem Text="3" Value="3"></asp:ListItem>
								<asp:ListItem Text="4" Value="4"></asp:ListItem>
							</asp:DropDownList>
									</li>
									<li>
										<asp:CheckBox ID="cbxAnalysisEntriesRoundedDown" runat="server" Text="Analysis entries should round down"
											Style="margin-left: 2em; width: 80%;" />
									</li>
									<li>
										<asp:CheckBox ID="cbxAnalysisShowTrailingZeros" runat="server" Text="Show all decimal places on analysis"
											Style="margin-left: 2em;" />
									</li>
									<li>
										<asp:CheckBox ID="cbxHideZeroPercentAnalysisNutrients" runat="server" Text="Hide N-P-K nutrients with 0% inclusion in guaranteed analysis"
											Style="margin-left: 2em;" />
									</li>
								</ul>
							</li>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="cbxShowLoadedBy" runat="server" Text="Show loaded by (user)" />
							</li>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="chkShowOrderSummary" runat="server" AutoPostBack="true" Text="Show order summary" />
							</li>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="chkShowOrderSummaryHistorical" runat="server" Text="Show summary at time of ticket creation"
									Style="margin-left: 2em;" />
							</li>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="chkOwner" runat="server" Text="Show owner" />
							</li>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="chkProductHazardousMaterial" runat="server" Text="Show product hazardous material" />
							</li>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="chkShowProductSummaryTotals" runat="server" AutoPostBack="true"
									Text="Show product summary totals" />
							</li>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="chkShowProductNotesSummaryTotals" runat="server" Text="Show product notes"
									Style="margin-left: 2em;" />
							</li>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="chkPurchaseOrder" runat="server" Text="Show purchase order number" />
							</li>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="chkReleaseNumber" runat="server" Text="Show release number" />
							</li>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="chkShowRequestedQuantities" runat="server" Text="Show requested quantities" />
							</li>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="chkShowRinseEntries" runat="server" AutoPostBack="true" Text="Show rinse entries" />
							</li>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="chkCustomerLocation" runat="server" Text="Show ship to" />
							</li>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="chkTotal" runat="server" Text="Show total" />
							</li>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="chkTransport" runat="server" Text="Show transport" AutoPostBack="true"></asp:CheckBox>
							</li>
							<li>
								<label>
								</label>
								<asp:CheckBox ID="chkShowTicketTransportTareInfo" runat="server" Text="Show transport tare information"
									AutoPostBack="true" Style="margin-left: 2em;" />
							</li>
							<li id="tblTransportTareOptions" runat="server">
								<label>
								</label>
								<div class="input" style="margin-left: 2em;">
									<ul>
										<li id="rowTransportTareFirstOption" runat="server">
											<label>
												First position
											</label>
											<asp:DropDownList ID="ddlTransportTareFirstOption" runat="server" AutoPostBack="true">
											</asp:DropDownList>
										</li>
										<li id="rowTransportTareSecondOption" runat="server">
											<label>
												Second position
											</label>
											<asp:DropDownList ID="ddlTransportTareSecondOption" runat="server" AutoPostBack="true">
											</asp:DropDownList>
										</li>
										<li id="rowTransportTareThirdOption" runat="server">
											<label>
												Third position
											</label>
											<asp:DropDownList ID="ddlTransportTareThirdOption" runat="server">
											</asp:DropDownList>
										</li>
									</ul>
								</div>
							</li>
						</ul>
					</div>
					<div class="sectionOdd">
						<ul>
							<li>
								<label>
									Show additional units
								</label>
								<asp:CheckBoxList ID="cblAdditionalUnitsForTicket" runat="server" RepeatLayout="UnorderedList"
									CssClass="input">
								</asp:CheckBoxList>
							</li>
							<li>
								<label>
									Density unit precision
								</label>
								<ul class="input">
									<li class="addRemoveSection"><span class="addRemoveList">
										<asp:DropDownList ID="ddlWebTicketDensityMass" runat="server" AutoPostBack="True"
											Style="width: 40%;">
										</asp:DropDownList>
										&nbsp;/
							<asp:DropDownList ID="ddlWebTicketDensityVolume" runat="server" AutoPostBack="True"
								Style="width: 40%;">
							</asp:DropDownList>
									</span>
										<asp:Button ID="btnWebTicketDensityAdd" runat="server" Text="Add" CssClass="addRemoveButton" />
									</li>
									<li class="addRemoveSection">
										<asp:ListBox ID="lstWebTicketDensityList" runat="server" AutoPostBack="True" CssClass="addRemoveList"></asp:ListBox>
										<asp:Button ID="btnWebTicketDensityRemove" runat="server" Text="Remove" CssClass="addRemoveButton" />
										<ul class="addRemoveButton">
											<li id="trWebTicketDensityPrecisionControls" runat="server" visible="false">
												<ul>
													<li>
														<label>
															Whole
														</label>
														<span class="input">
															<asp:Button ID="btnWebTicketDensityAddWhole" runat="server" Text="+" CssClass="button" />
															<asp:Button ID="btnWebTicketDensityRemoveWhole" runat="server" Text="-" CssClass="button" /></span>
													</li>
													<li>
														<label>
															Fractional
														</label>
														<span class="input">
															<asp:Button ID="btnWebTicketDensityAddFractional" runat="server" Text="+" CssClass="button" />
															<asp:Button ID="btnWebTicketDensityRemoveFractional" runat="server" Text="-" CssClass="button" /></span>
													</li>
												</ul>
											</li>
										</ul>
									</li>
								</ul>
							</li>
							<li>
								<label>
									Show additional units for product groups</label>
								<span class="input"><span class="addRemoveSection">
									<asp:DropDownList ID="ddlProductGroup" runat="server" AutoPostBack="true" CssClass="addRemoveList">
									</asp:DropDownList>
									<asp:Button ID="btnSaveProductGroupAdditionalUnits" runat="server" Text="Save" AutoPostBack="true"
										CssClass="addRemoveButton" /></span></span> </li>
							<li>
								<label>
								</label>
								<asp:CheckBoxList ID="cblAdditionalUnitsForProductGroup" runat="server" RepeatLayout="UnorderedList"
									CssClass="input">
								</asp:CheckBoxList>
							</li>
							<li>
								<label>
								</label>
								<ul class="input">
									<li class="addRemoveSection"><span class="addRemoveList">Density display units (for selected product group)
                                <br />
										<asp:DropDownList ID="ddlProductGroupDensityMassUnit" runat="server" AutoPostBack="True"
											Style="width: 40%;">
										</asp:DropDownList>
										&nbsp;/
							    <asp:DropDownList ID="ddlProductGroupDensityVolumeUnit" runat="server" AutoPostBack="True"
									Style="width: 40%;">
								</asp:DropDownList>
									</span>
									</li>
								</ul>
							</li>
							<li>
								<label>
									Ticket logo
								</label>
								<asp:TextBox ID="tbxTicketLogo" runat="server"></asp:TextBox>
							</li>
							<li>
								<label>
									Owner message
								</label>
								<asp:TextBox ID="tbxOwnerMessage" runat="server" Text="" TextMode="MultiLine"></asp:TextBox>
							</li>
							<li>
								<label>
									Disclaimer
								</label>
								<asp:TextBox ID="tbxOwnerDisclaimer" runat="server" Text="" TextMode="MultiLine"></asp:TextBox>
							</li>
							<li>
								<label>
									Blank 1
								</label>
								<asp:TextBox ID="tbxBlank1" runat="server"></asp:TextBox>
							</li>
							<li>
								<label>
									Blank 2
								</label>
								<asp:TextBox ID="tbxBlank2" runat="server"></asp:TextBox>
							</li>
							<li>
								<label>
									Blank 3
								</label>
								<asp:TextBox ID="tbxBlank3" runat="server"></asp:TextBox>
							</li>
							<li>
								<label>
									Ticket add-on URL
								</label>
								<asp:TextBox ID="tbxTicketAddonURL" runat="server"></asp:TextBox>
							</li>
							<li id="pnlDeliveryTicketCustomFieldsAssigned" runat="server" visible="false">
								<label>
									Show custom fields on delivery ticket
								</label>
								<span class="input">
									<asp:CheckBox ID="cbxShowAllCustomFieldsOnDeliveryTicket" runat="server" Text="Show all custom fields on delivery ticket"
										AutoPostBack="true" />
									<asp:CheckBoxList ID="cblShowCustomFieldsOnDeliveryTicket" runat="server" Enabled="false"
										RepeatLayout="UnorderedList" Style="border: 1px solid black !important;">
									</asp:CheckBoxList>
								</span></li>
							<li id="pnlCustomPreLoadQuestions" runat="server" visible="false">
								<label>
									Show custom pre-load questions
								</label>
								<span class="input">
									<asp:CheckBox ID="cbxShowAllCustomPreLoadQuestions" runat="server" Text="Show all custom pre-load questions"
										AutoPostBack="true" />
									<asp:CheckBoxList ID="cblShowCustomPreLoadQuestions" runat="server" Enabled="false"
										RepeatLayout="UnorderedList" Style="border: 1px solid black !important;">
									</asp:CheckBoxList>
								</span></li>
							<li id="pnlCustomPostLoadQuestions" runat="server" visible="false">
								<label>
									Show custom post-load questions
								</label>
								<span class="input">
									<asp:CheckBox ID="cbxShowAllCustomPostLoadQuestions" runat="server" Text="Show all custom post-load questions"
										AutoPostBack="true" />
									<asp:CheckBoxList ID="cblShowCustomPostLoadQuestions" runat="server" Enabled="false"
										RepeatLayout="UnorderedList" Style="border: 1px solid black !important;">
									</asp:CheckBoxList>
								</span></li>
						</ul>
					</div>
				</div>
			</ContentTemplate>
		</asp:UpdatePanel>
	</form>
</body>
</html>
