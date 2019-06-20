<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="CustomFields.aspx.vb"
	Inherits="KahlerAutomation.TerminalManagement2.CustomFields" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
	<title>Custom Fields : Custom Fields</title>
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
	<form id="main" method="post" runat="server" defaultfocus="tbxName">
		<div class="recordSelection">
			<label>
				Custom Field</label>
			<asp:DropDownList ID="ddlCustomFields" runat="server" AutoPostBack="True" />
		</div>
		<div class="recordControl">
			<asp:Button ID="btnSave" runat="server" Text="Save" />
			<asp:Button ID="btnDelete" runat="server" Text="Delete" Enabled="false" />
			<asp:Label ID="lblStatus" runat="server" ForeColor="Red" />
			<span class="sectionRequiredField"><span class="required"></span>&nbsp;indicates required
            field </span>
		</div>
		<div class="sectionEven">
			<h1>General</h1>
			<ul>
				<li>
					<label>
						Name
					</label>
					<span class="required">
						<asp:TextBox ID="tbxName" runat="server" MaxLength="50"></asp:TextBox>
					</span></li>
				<li>
					<label>
						Table
					</label>
					<span class="required">
						<asp:DropDownList ID="ddlTableSource" runat="server" AutoPostBack="True">
						</asp:DropDownList>
					</span></li>
				<li id="pnlInputType" runat="server">
					<label>
						Type
					</label>
					<span class="required">
						<asp:DropDownList ID="ddlInputType" runat="server" AutoPostBack="True">
						</asp:DropDownList>
					</span></li>
				<li id="pnlTicketSource" runat="server" visible="false">
					<label>
						Ticket source</label>
					<asp:DropDownList ID="ddlTicketSource" runat="server">
					</asp:DropDownList>
				</li>
			</ul>
		</div>
		<div class="sectionOdd" id="pnlOptions" runat="server" visible="false">
			<h1>Options
			</h1>
			<ul class="addRemoveSection">
				<li class="addRemoveSection">
					<asp:TextBox ID="tbxOption" runat="server" CssClass="addRemoveList"></asp:TextBox>
					<asp:Button ID="btnAddOption" runat="server" CssClass="addRemoveButton" Text="Add"></asp:Button>
				</li>
				<li class="addRemoveSection">
					<asp:ListBox ID="lstOptions" runat="server" CssClass="addRemoveList"></asp:ListBox>
					<asp:Button ID="btnRemoveOption" runat="server" CssClass="addRemoveButton" Text="Remove"></asp:Button>
					<asp:Button ID="btnSortOptions" runat="server" CssClass="addRemoveButton" Text="Sort Asc"></asp:Button>
				</li>
			</ul>
		</div>
		<div class="sectionOdd" id="pnlTableLookupOptions" runat="server">
			<h1>Options
			</h1>
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
		</div>
	</form>
</body>
</html>
