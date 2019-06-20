<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="TransportInspectionQuestions.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.TransportInspectionQuestions" MaintainScrollPositionOnPostback="true" %>

<!DOCTYPE html>
<html lang="en">
<head>
	<title>Transports : Transport Inspection Questions</title>
	<link rel="shortcut icon" href="FavIcon.ico" />
	<meta name="viewport" content="width=device-width" />
	<meta http-equiv="X-UA-Compatible" content="IE=edge" />
	<link rel="stylesheet" href="styles/site.css" />
	<link rel="stylesheet" href="styles/site-tablet.css" media="(max-width:768px)" />
	<link rel="stylesheet" href="styles/site-phone.css" media="(max-width:480px)" />
	<script type="text/javascript" src="scripts/jquery-1.11.1.min.js"></script>
	<script type="text/javascript" src="scripts/soap-1.0.1.js"></script>
	<script type="text/javascript" src="scripts/page-controller.js"></script>
	<style type="text/css">
		.filter li {
			display: inline-block;
			width: 33%;
		}

		.filter label {
			display: inline-block;
			width: 40%;
			text-align: right;
			vertical-align: top;
			margin: 3px;
		}

		.filter select {
			width: 55%;
		}

		.filter input {
			width: 100%;
		}
	</style>
</head>
<body>
	<form id="main" method="post" runat="server" defaultfocus="tbxName">
		<asp:Panel ID="pnlTop" runat="server" CssClass="section">
			<h1>Questions</h1>
			<ul class="filter">
				<li>
					<label>
						Bay</label><asp:DropDownList ID="ddlBay" runat="server" AutoPostBack="True" />
				</li>
				<li>
					<label>
						Pre/post</label>
					<asp:DropDownList ID="ddlPrePost" runat="server" AutoPostBack="True">
						<asp:ListItem Value="Pre">Pre</asp:ListItem>
						<asp:ListItem Value="Post">Post</asp:ListItem>
					</asp:DropDownList>
				</li>
				<li>
					<label>
						Transport type</label><asp:DropDownList ID="ddlTransportType" runat="server" AutoPostBack="True" />
				</li>
			</ul>
			<ul class="addRemoveSection">
				<li>
					<asp:ListBox ID="lbxQuestions" runat="server" AutoPostBack="True" Style="height: 200px;" class="addRemoveList" />
					<asp:LinkButton ID="btnAddQuestion" runat="server" Text="+" class="button" Visible="false" ToolTip="Add" />
					<asp:LinkButton ID="btnRemoveQuestion" runat="server" Text="x" class="button" Visible="false" ToolTip="Remove" />
					<asp:LinkButton ID="btnMoveQuestionUp" runat="server" Text="u" class="button" Visible="false" ToolTip="Move up" />
					<asp:LinkButton ID="btnMoveQuestionDown" runat="server" Text="d" class="button" Visible="false" ToolTip="Move down" />
				</li>
			</ul>
		</asp:Panel>
		<div class="recordControl">
			<asp:Button ID="btnSave" runat="server" Text="Save" Enabled="False" />
			<asp:Label ID="lblStatus" runat="server" ForeColor="Red" />
		</div>
		<asp:Panel ID="pnlEven" runat="server" CssClass="sectionEven">
			<h1>Question details</h1>
			<ul>
				<li>
					<label>
						Name</label>
					<span class="required">
						<asp:TextBox ID="tbxName" runat="server" /></span> </li>
				<li>
					<label>
					</label>
					<asp:CheckBox ID="cbxPostLoad" runat="server" AutoPostBack="True" Text="Post load" />
				</li>
				<li>
					<label>
						Prompt text</label>
					<asp:TextBox ID="tbxPromptText" runat="server" />
				</li>
				<li>
					<label>
						Type</label>
					<asp:DropDownList ID="ddlQuestionType" runat="server" AutoPostBack="True">
						<asp:ListItem Value="0">Yes/No</asp:ListItem>
						<asp:ListItem Value="1">Text</asp:ListItem>
						<asp:ListItem Value="2">List</asp:ListItem>
						<asp:ListItem Value="3">Custom</asp:ListItem>
						<asp:ListItem Value="4">Table lookup</asp:ListItem>
						<asp:ListItem Value="5">Date and Time</asp:ListItem>
						<asp:ListItem Value="6">Date</asp:ListItem>
						<asp:ListItem Value="7">Time</asp:ListItem>
					</asp:DropDownList>
				</li>
				<li>
					<label>
						Transport type</label>
					<asp:DropDownList ID="ddlTransportTypeSel" runat="server" AutoPostBack="True" />
				</li>
				<li>
					<label>
						Owner</label>
					<asp:DropDownList ID="ddlOwner" runat="server" />
				</li>
				<li>
					<label>
						URL</label>
					<asp:TextBox ID="tbxUrl" runat="server" />
				</li>
				<li id="liTableLookupOptions" runat="server">
					<ul style="width: 100%;">
						<li>
							<label>
								Table name
							</label>
							<asp:DropDownList ID="ddlTableLookupTableName" runat="server" AutoPostBack="true"></asp:DropDownList>
						</li>
						<li>
							<label>
								Field name
							</label>
							<asp:DropDownList ID="ddlTableLookupFieldName" runat="server"></asp:DropDownList>
						</li>
					</ul>
				</li>
				<li>
					<label>
						Disabled</label>
					<asp:CheckBox ID="cbxDisabled" runat="server" AutoPostBack="True" Text="" />
				</li>
			</ul>
		</asp:Panel>
		<asp:Panel ID="pnlOdd" runat="server" CssClass="sectionOdd">
			<h1>Parameters</h1>
			<ul class="addRemoveSection">
				<li>
					<asp:ListBox ID="lbxParameters" runat="server" AutoPostBack="True" class="addRemoveList" />
					<asp:LinkButton ID="btnAddParameter" runat="server" Text="+" class="button" ToolTip="Add" />
					<asp:LinkButton ID="btnRemoveParameter" runat="server" Text="x" class="button" Visible="false" ToolTip="Remove" />
					<asp:LinkButton ID="btnMoveParameterUp" runat="server" Text="u" class="button" Visible="false" ToolTip="Move up" />
					<asp:LinkButton ID="btnMoveParameterDown" runat="server" Text="d" class="button" Visible="false" ToolTip="Move down" />
				</li>
				<li>
					<asp:TextBox ID="tbxParameter" runat="server" class="addRemoveList" />
					<asp:Button ID="btnUpdateParameter" runat="server" Text="Update" class="addRemoveButton" />
				</li>
			</ul>
		</asp:Panel>
	</form>
</body>
</html>
