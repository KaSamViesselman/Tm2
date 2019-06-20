<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="ordersummary.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.ordersummary" %>

<!DOCTYPE html>
<html lang="en">
<head>
	<title>Order summary</title>
</head>
<body>
	<style type="text/css">
		html, body, div, span, applet, object, iframe, h1, h2, h3, h4, h5, h6, p, blockquote, pre, a, abbr, acronym, address, big, cite, code, del, dfn, em, img, ins, kbd, q, s, samp, small, strike, strong, sub, sup, tt, var, b, u, i, center, dl, dt, dd, ol, ul, li, fieldset, form, label, legend, table, caption, tbody, tfoot, thead, tr, th, td, article, aside, canvas, details, embed, figure, figcaption, footer, header, hgroup, menu, nav, output, ruby, section, summary, time, mark, audio, video {
			margin: 0;
			padding: 0;
			border: 0;
			font-size: 100%;
			font: inherit;
			vertical-align: baseline;
		}
		/* HTML5 display-role reset for older browsers */
		article, aside, details, figcaption, figure, footer, header, hgroup, menu, nav, section {
			display: block;
		}
		body {
			line-height: 1;
		}
		ol, ul, li {
			list-style: none;
		}
		blockquote, q {
			quotes: none;
		}
		blockquote:before, blockquote:after, q:before, q:after {
			content: '';
			content: none;
		}
		table {
			border-collapse: collapse;
			border-spacing: 0;
		}
		
		* {
			-moz-box-sizing: border-box;
			box-sizing: border-box;
		}
		
		body {
			font-family: Arial;
			font-size: 10pt;
		}
		
		.name {
			font-weight: bold;
		}
		
		h1 {
			font-size: 12pt;
			font-weight: bold;
			padding: 6px 0px 6px 0px;
		}
		
		table {
			width: 100%;
			position: relative;
			border-collapse: collapse;
			border-spacing: 0;
		}
		
		th {
			padding: 5px;
			background-color: White;
			text-align: left;
			font-weight: bold;
			border: 1px solid rgb(200, 200, 200);
			border-collapse: collapse;
		}
		
		tr:nth-child(odd) {
			background-color: rgb(240, 240, 240);
		}
		
		td {
			padding: 5px;
			border: 1px solid rgb(200, 200, 200);
			border-collapse: collapse;
		}
		
		.soldBySection, .soldToSection, .orderNumberSection, .branchSection, .orderHeaderSection, .shipToSection {
			width: 48%;
			padding: 6px;
		}
		
		.ticketSection, .orderDetailsSection {
			clear: both;
			width: 96%;
			padding: 6px;
		}
		
		.soldBySection, .branchSection {
			float: left;
		}
		
		.soldToSection, .shipToSection {
			float: right;
		}
		
		.orderNumberSection {
			margin-bottom: -10px;
		}
	</style>
	<form id="form1" runat="server">
	<div class="orderNumberSection">
		<h1>
			Order Number:
			<asp:Literal ID="litOrderNumber" runat="server" /></h1>
	</div>
	<div class="soldBySection" id="pnlSoldBySection" runat="server">
		<h1>
			Sold by</h1>
		<asp:Literal ID="litSoldBy" runat="server" />
	</div>
	<div class="soldToSection" id="pnlSoldToSection" runat="server">
		<h1>
			Sold to</h1>
		<asp:Literal ID="litSoldTo" runat="server" />
	</div>
	<div class="branchSection" id="pnlBranchSection" runat="server">
		<h1>
			Branch</h1>
		<asp:Literal ID="litBranch" runat="server" />
	</div>
	<div class="shipToSection" id="pnlShipToSection" runat="server">
		<h1>
			Ship to</h1>
		<asp:Literal ID="litShipToSection" runat="server" />
	</div>
	<div class="orderHeaderSection" id="pnlOrderHeaderSection" runat="server">
		<br />
		<asp:Literal ID="litOrderHeaderSection" runat="server" />
	</div>
	<div id="pnlticketSection" runat="server" class="ticketSection">
		<h1>
			Ticket history</h1>
		<table id="tblTickets" runat="server">
			<tr id="trTicketsHeader" runat="server">
				<th>
					Date
				</th>
				<th>
					Ticket
				</th>
				<th>
					Applicator
				</th>
				<th>
					Site
				</th>
				<th>
					Acres
				</th>
				<th>
					Transport(s)
				</th>
			</tr>
		</table>
	</div>
	<div class="orderDetailsSection">
		<h1>
			Order details</h1>
		<asp:Literal ID="litOrderDetails" runat="server" />
	</div>
	</form>
</body>
</html>
