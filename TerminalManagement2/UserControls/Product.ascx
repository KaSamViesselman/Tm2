<%@ Control Language="vb" AutoEventWireup="true" CodeBehind="Product.ascx.vb" Inherits="KahlerAutomation.TerminalManagement2.Product" %>
<%@ Register Src="~/UserControls/BackNextButtons.ascx" TagPrefix="UserControls" TagName="BackNext" %>
<%@ Register Src="~/UserControls/SelectOwners.ascx" TagPrefix="UserControls" TagName="SelectOwners" %>

<asp:Panel runat="server" ID="pnlRequiredFields">
    <div class="form-row">
        <div class="form-group col-md-4">
            <label for="tbxName">Name</label>
            <asp:TextBox ID="tbxName" runat="server" CssClass="form-control" ClientIDMode="Static" MaxLength="50" placeholder="Enter product name"></asp:TextBox>
        </div>
        <div class="form-group col-md-4">
            <UserControls:SelectOwners runat="server" ID="ddlOwners" />
        </div>
        <div class="form-group col-md-4">
            <label>Default unit</label>
            <asp:DropDownList ID="ddlUnit" runat="server" CssClass="form-control" ClientIDMode="Static">
                <asp:ListItem>Select Default Unit</asp:ListItem>
            </asp:DropDownList>
        </div>
    </div>
    <div class="form-row">
        <div class="form-group col-12">
            <label for="tbxNotes">Notes</label>
            <asp:TextBox ID="tbxNotes" runat="server" CssClass="form-control" ClientIDMode="Static" TextMode="MultiLine"></asp:TextBox>
        </div>
    </div>
    <div class="form-group">
        <div class="form-check">
            <asp:CheckBox ID="chkDoNotStack" runat="server" ClientIDMode="Static" class="form-check-input" />
            <label class="form-check-label" for="chkDoNotStack">Do not stack (for weigh-tanks)</label>
        </div>
    </div>
    <div class="form-group">
        <div class="form-check">
            <asp:CheckBox ID="chkHazardousMaterial" runat="server" ClientIDMode="Static" class="form-check-input" />
            <label class="form-check-label" for="chkHazardousMaterial">Hazardous material</label>
        </div>
    </div>
</asp:Panel>
<asp:Panel runat="server" ID="pnlPromptAdditionalFields" Visible="false">
    <div class="row justify-content-center">
        <h4>Would you like to set an EPA number, a MSDS number, a manufacturer, active ingredients, or restrictions for this product?</h4>
    </div>
    <div class="row justify-content-center mb-4">
        <asp:Button runat="server" Text="Yes" CssClass="btn btn-primary col-md-4 m-2" OnClick="Yes_Click" />
        <asp:Button runat="server" Text="No" CssClass="btn btn-primary col-md-4 m-2" OnClick="No_Click" />
    </div>
</asp:Panel>
<asp:Panel runat="server" ID="pnlAdditionalFields" Visible="false">
    <div class="form-row">
        <div class="form-group col-md-4">
            <label for="tbxEpaNumber">EPA number</label>
            <asp:TextBox ID="tbxEpaNumber" runat="server" CssClass="form-control" ClientIDMode="Static" placeholder="Enter EPA Number"></asp:TextBox>
        </div>
        <div class="form-group col-md-4">
            <label for="tbxMsdsNumber">MSDS Number</label>
            <asp:TextBox ID="tbxMsdsNumber" runat="server" CssClass="form-control" ClientIDMode="Static" placeholder="Enter MSDS Number"></asp:TextBox>
        </div>
        <div class="form-group col-md-4">
            <label for="tbxManufacturer">Default unit</label>
            <asp:TextBox ID="tbxManufacturer" runat="server" CssClass="form-control" ClientIDMode="Static" placeholder="Enter Manfacturer"></asp:TextBox>
        </div>
    </div>
    <div class="form-row">
        <div class="form-group col-md-4">
            <label for="tbxActiveIngredients">Active ingredients</label>
            <asp:TextBox ID="tbxActiveIngredients" runat="server" CssClass="form-control" ClientIDMode="Static" placeholder="Enter Active Ingredients" />
        </div>
        <div class="form-group col-md-4">
            <label for="tbxRestrictions">Restrictions</label>
            <asp:TextBox ID="tbxRestrictions" runat="server" CssClass="form-control" ClientIDMode="Static" placeholder="Enter Restrictions" />
        </div>
    </div>
</asp:Panel>
<asp:Panel runat="server" ID="pnlPromptAppRates" Visible="false">
    <div class="row justify-content-center">
        <h4>Would you like to set a recommended minimum or maximum application rate for this product?</h4>
    </div>
    <div class="row justify-content-center mb-4">
        <asp:Button runat="server" Text="Yes" CssClass="btn btn-primary col-md-4 m-2" OnClick="Yes_Click" />
        <asp:Button runat="server" Text="No" CssClass="btn btn-primary col-md-4 m-2" OnClick="No_Click" />
    </div>
</asp:Panel>
<asp:Panel runat="server" ID="pnlApplicationRates" CssClass="row" Visible="false">
    <div class="form-group col-md-6">
        <label for="tbxMaxAppRate">Maximum application rate</label>
        <asp:TextBox ID="tbxMaxAppRate" runat="server" CssClass="form-control" ClientIDMode="Static" placeholder="Enter Max Rate"></asp:TextBox>
    </div>
    <div class="form-group col-md-6">
        <label for="tbxMinAppRate">Minimum application rate</label>
        <asp:TextBox ID="tbxMinAppRate" runat="server" CssClass="form-control" ClientIDMode="Static" placeholder="Enter Min Rate"></asp:TextBox>
    </div>
</asp:Panel>
<asp:Panel runat="server" ID="pnlPromptProductGroup" Visible="false">
    <div class="row justify-content-center">
        <h4>Would you like to assign this product to a product group?</h4>
    </div>
    <div class="row justify-content-center mb-4">
        <asp:Button runat="server" Text="Yes" CssClass="btn btn-primary col-md-4 m-2" OnClick="Yes_Click" />
        <asp:Button runat="server" Text="No" CssClass="btn btn-primary col-md-4 m-2" OnClick="No_Click" />
    </div>
</asp:Panel>
<asp:Panel runat="server" ID="pnlProductGroup" CssClass="form-row" Visible="false">
    <div class="form-group col-md-6">
        <label for="ddlProductGroup">Product Group</label>
        <asp:DropDownList ID="ddlProductGroup" runat="server" CssClass="form-control" ClientIDMode="Static">
            <asp:ListItem Value="00000000-0000-0000-0000-000000000000">Select Product Group</asp:ListItem>
            <asp:ListItem Value="739B841B-5554-4DF1-86A9-9D12216440F9">Some group</asp:ListItem>
        </asp:DropDownList>
    </div>
</asp:Panel>
<asp:Panel runat="server" ID="pnlCustomFields" Visible="false">
    <h4>Custom Fields</h4>
    <asp:Repeater runat="server" ID="rpCustomFields">
        <ItemTemplate>

        </ItemTemplate>
    </asp:Repeater>    
</asp:Panel>
<UserControls:BackNext runat="server" ID="pnlBackNext" />

