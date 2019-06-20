<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="GeneralSettings.aspx.vb"
	Inherits="KahlerAutomation.TerminalManagement2.GeneralSettings" MaintainScrollPositionOnPostback="true" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>General Settings : General Settings</title>
	<link rel="shortcut icon" href="FavIcon.ico" />
	<meta name="viewport" content="width=device-width" />
	<meta http-equiv="X-UA-Compatible" content="IE=edge" />
	<link rel="stylesheet" href="styles/site.css" />
	<link rel="stylesheet" href="styles/site-tablet.css" media="(max-width:768px)" />
	<link rel="stylesheet" href="styles/site-phone.css" media="(max-width:480px)" />
	<script type="text/javascript" src="scripts/jquery-1.11.1.min.js"></script>
	<script type="text/javascript" src="scripts/soap-1.0.1.js"></script>
	<script type="text/javascript" src="scripts/page-controller.js"></script>
</head>
<body>
	<form id="main" method="post" runat="server" defaultfocus="ddlDefaultMassUnit">
		<div class="recordControl">
			<asp:Button ID="btnSaveGeneralSettings" runat="server" Text="Save General Settings" />
			<asp:Label ID="lblGeneralSettingsSave" runat="server" ForeColor="Red" Text="General settings were saved successfully"
				Visible="False" />
		</div>
		<div class="sectionEven" id="pnlGeneralSettings" runat="server">
			<ul>
				<li>
					<label>
						Default mass unit
					</label>
					<asp:DropDownList ID="ddlDefaultMassUnit" runat="server">
					</asp:DropDownList>
				</li>
				<li>
					<label>
						Default volume unit
					</label>
					<asp:DropDownList ID="ddlDefaultVolumeUnit" runat="server">
					</asp:DropDownList>
				</li>
				<li>
					<label>
						Web page title
					</label>
					<asp:TextBox ID="tbxWebPageTitle" runat="server"></asp:TextBox>
				</li>
				<li>
					<label>
						Database change e-mail address(es)
					</label>
					<asp:TextBox ID="tbxDatabaseChangeEmailAddress" runat="server"></asp:TextBox>
				</li>
				<li>
					<label>
					</label>
					<asp:CheckBox ID="cbxConvertWebPageUrlDomainToRequestedPagesDomain" runat="server"
						Text="Convert web page URL domain to requested page's domain for links on pages"></asp:CheckBox>
				</li>
				<li>
					<label>
					</label>
					<asp:CheckBox ID="cbxDefaultNewDriversValidForAllAccounts" runat="server" Text="Default new drivers to be valid for all accounts" />
				</li>
				<li>
					<label></label>
					<asp:CheckBox ID="cbxItemTraceabilityEnabled" runat="server" Text="Item traceability enabled" />
				</li>
				<li>
					<hr />
				</li>
				<li>
					<h2>Event log settings</h2>
				</li>
				<li>
					<label>
						Days to keep event log records
					</label>
					<asp:TextBox ID="tbxDaysToKeepEventLogRecords" runat="server"></asp:TextBox>
				</li>
				<li>
					<hr />
				</li>
				<li>
					<h2>Activation settings</h2>
					<label>Proxy Address</label>
					<asp:TextBox ID="tbxProxyAddress" runat="server" />
					<label>Proxy Port</label>
					<asp:TextBox ID="tbxProxyPort" runat="server" />
				</li>
				<li>
					<label>
						Activation e-mail alerts (separated by commas)</label>
					<asp:TextBox ID="tbxActivationAlertEmailRecipients" runat="server" />
				</li>
			</ul>
		</div>
		<div class="sectionOdd" id="pnlApplicationSettings" runat="server">
			<ul>
				<li>
					<h2>Database backup settings</h2>
				</li>
				<li>
					<label>
						Backup folder</label><asp:TextBox ID="tbxDatabaseBackupFolder" runat="server"></asp:TextBox></li>
				<li>
					<label>
						Number of backup sets</label><asp:DropDownList ID="ddlDatabaseBackupNumberOfSets"
							runat="server">
						</asp:DropDownList>
				</li>
				<li>
					<label>
						Backup timeout</label>
					<asp:TextBox ID="tbxDatabaseBackupTimeout" runat="server"></asp:TextBox></li>
				<li>
				<li>
					<label>Log folder</label>
					<asp:TextBox ID="tbxBackupLog" runat="server"></asp:TextBox>
				</li>
				<li>
					<hr />
				</li>
				<li>
					<h2>Background Services</h2>
				</li>
				<li>
					<label>Log folder</label>
					<asp:TextBox ID="tbxBackgroundServicesLog" runat="server"></asp:TextBox>
				</li>
				<li>
					<hr />
				</li>
				<li>
					<h2>Email Service</h2>
				</li>
				<li>
					<label>Log folder</label>
					<asp:TextBox ID="tbxEmailServiceLog" runat="server"></asp:TextBox>
				</li>
				<li>
					<hr />
				</li>
				<li>
					<h2>Tank Status Reader</h2>
				</li>
				<li>
					<label>Log folder</label>
					<asp:TextBox ID="tbxTankStatusLog" runat="server"></asp:TextBox>
				</li>
			</ul>
		</div>
	</form>
</body>
</html>
