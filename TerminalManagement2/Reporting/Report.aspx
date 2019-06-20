<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="Report.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.Report" %>

<%@ Register Assembly="Microsoft.ReportViewer.WebForms, Version=15.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
	<style type="text/css">
		.auto-style1 {
			width: 100%;
			height: 100%;
		}
	</style>
</head>
<body>
	<form id="form1" runat="server" class="auto-style1">
		<asp:ScriptManager runat="server"></asp:ScriptManager>
		<rsweb:reportviewer id="ReportViewer1" runat="server" processingmode="Local" style="width: 100%; height: 100%;">
		</rsweb:reportviewer>
	</form>
</body>
</html>
