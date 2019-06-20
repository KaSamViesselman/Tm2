<%@ Page Language="vb" AutoEventWireup="True" CodeBehind="ProductSetup.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.ProductSetup" MasterPageFile="Main.Master" %>
<%@ Register Src="~/UserControls/SelectProduct.ascx" TagPrefix="ProductSetup" TagName="SelectProduct" %>
<%@ Register Src="~/UserControls/Product.ascx" TagPrefix="ProductSetup" TagName="Product" %>
<%@ Register Src="~/UserControls/Recipes.ascx" TagPrefix="ProductSetup" TagName="Recipes" %>
<%@ Register Src="~/UserControls/ProductInterfaces.ascx" TagPrefix="ProductSetup" TagName="ProductInterfaces" %>
<%@ Register Src="~/UserControls/BulkPanelSettings.ascx" TagPrefix="ProductSetup" TagName="BulkPanelSettings" %>
<%@ Register Src="~/UserControls/BackNextButtons.ascx" TagPrefix="Common" TagName="BackNext" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <asp:UpdatePanel runat="server">
        <ContentTemplate>
            <ProductSetup:SelectProduct runat="server" ID="pnlSelectProduct" />
            <ProductSetup:Product runat="server" ID="pnlProduct" Visible="false" />
            <ProductSetup:Recipes runat="server" ID="pnlRecipes" Visible="false" />
            <ProductSetup:BulkPanelSettings runat="server" ID="pnlBulkPanelSettings" Visible="false" />
            <ProductSetup:ProductInterfaces runat="server" ID="pnlInterfaces" Visible="false" />
            <Common:BackNext runat="server" ID="pnlBackNext" />
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
