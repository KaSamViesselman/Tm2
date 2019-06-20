<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="ProductListView.aspx.vb"
    Inherits="KahlerAutomation.TerminalManagement2.ProductListView" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <title>Products : Product List</title>
    <link rel="shortcut icon" href="FavIcon.ico" />
    <meta name="viewport" content="width=device-width" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <link rel="stylesheet" href="styles/site.css" />
    <link rel="stylesheet" href="styles/site-tablet.css" media="(max-width:768px)" />
    <link rel="stylesheet" href="styles/site-phone.css" media="(max-width:480px)" />
</head>
<body>
    <form id="form1" runat="server">
    <div class="section">
        <asp:Literal ID="litReport" runat="server"></asp:Literal>
    </div>
    </form>
</body>
</html>
