<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="PrintTickets.aspx.vb"
	Inherits="KahlerAutomation.TerminalManagement2.PrintTickets" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server" id="headMain">
	<title>Print Tickets</title>
	<link rel="shortcut icon" href="FavIcon.ico" />
	<script type="text/javascript" src="scripts/jquery-1.11.1.min.js"></script>
	<script language="javascript" type="text/javascript">
		$(document).ready(function () {
			var ticketSources = $('#lblTicketSources').text();
			$('#lblTicketSources').hide();
			var ticketArray = ticketSources.split(',');
			var firstTicket = true;
			$.each(ticketArray, function (index, value) {
				var container = $(document.createElement('form'));
				if (!firstTicket)
					container.css({
						pageBreakBefore: 'always'
					});
				container.load(value);
				$('#bdyMain').append(container);
				firstTicket = false;
			});
		});
	</script>
</head>
<body id="bdyMain">
	<asp:Label ID="lblTicketSources" runat="server"></asp:Label>
</body>
</html>
