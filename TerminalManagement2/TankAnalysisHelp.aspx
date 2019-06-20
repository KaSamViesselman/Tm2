<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="TankAnalysisHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.TankAnalysisHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server"><title>Help : Tank Analysis</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="TankAnalysisHelp" runat="server">
		<div><span style="font-size: large; font-weight: bold;"><a href="help.aspx#TankAnalysis">Help</a> : Tank Analysis</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>Analysis information may be assigned to a tank so that analysis information will be recorded with the ticket for a load pulled from that tank.</p>
				<table>
					<tr>
						<td><span class="helpItem">Tank:</span> select the tank that the analysis will be assigned to, or that needs to be updated.</td>
					</tr>
					<tr>
						<td><span class="helpItem">Analysis type:</span> select the type of analysis that should be used for the bulk product in the tank.</td>
					</tr>
				</table>
			</div>
		</div>
	</form>
</body>
</html>
