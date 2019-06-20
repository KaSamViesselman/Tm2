<%@ Control Language="vb" AutoEventWireup="true" CodeBehind="BulkProductSetup.ascx.vb" Inherits="KahlerAutomation.TerminalManagement2.BulkProductSetup" %>
<%@ Register Src="~/UserControls/BackNextButtons.ascx" TagPrefix="UserControls" TagName="BackNext" %>

<asp:Panel runat="server" ID="pnlSelectBulkProduct">
    <div class="row justify-content-center">
        <h4>Would you like to create a new or update an existing bulk product?</h4>
    </div>
    <div class="row justify-content-center">
        <asp:LinkButton runat="server" Text="Create New Product" CssClass="btn btn-primary col-md-4 m-2" ID="btnNewProduct" OnClick="btnNewProduct_Click" />
        <asp:LinkButton runat="server" Text="Update Existing Bulk Product" CssClass="btn btn-primary col-md-4 m-2" ID="btnExistingProduct" OnClick="btnExistingProduct_Click" />
    </div>
</asp:Panel>
<script type="text/javascript">
    $(document).ready(function () {
        $("#tbxSearch").on("keyup", function () {
            var value = $(this).val().toLowerCase();
            $(".list-group-item").filter(function () {
                $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
            });
        });
    });
</script>
<div class="row">
    <h4 class="col">Select the bulk product you would like to update</h4>
</div>
<div class="form-group">
    <label for="tbxSearch">Search</label>
    <input class="form-control col" id="tbxSearch" type="text" placeholder="Search...">
</div>
<div class="row mt-3">
    <div class="col-md-6 col-lg-3">
        <asp:LinkButton runat="server" class="list-group-item list-group-item-action text-truncate" Text="Urea" OnClick="Urea_Click" />
        <asp:LinkButton runat="server" class="list-group-item list-group-item-action" Text="Potash" />
        <asp:LinkButton runat="server" class="list-group-item list-group-item-action" Text="DAP" />
    </div>
    <div class="col-md-6 col-lg-3">
        <asp:LinkButton runat="server" class="list-group-item list-group-item-action" Text="Urea" />
        <asp:LinkButton runat="server" class="list-group-item list-group-item-action" Text="Potash" />
        <asp:LinkButton runat="server" class="list-group-item list-group-item-action" Text="DAP" />
    </div>
    <div class="col-md-6 col-lg-3">
        <asp:LinkButton runat="server" class="list-group-item list-group-item-action" Text="Urea" />
        <asp:LinkButton runat="server" class="list-group-item list-group-item-action" Text="Potash" />
    </div>
    <div class="col-md-6 col-lg-3">
        <asp:LinkButton runat="server" class="list-group-item list-group-item-action" Text="Urea" />
        <asp:LinkButton runat="server" class="list-group-item list-group-item-action" Text="Potash" />
    </div>
</div>
<div class="form-row">
    <div class="form-group col-lg-4">
        <label for="tbxName">Name</label>
        <asp:TextBox ID="tbxName" runat="server" CssClass="form-control" ClientIDMode="Static" MaxLength="50" placeholder="Enter product name"></asp:TextBox>
    </div>
    <div class="form-group col-lg-4">
        <label for="ddlOwner">Owner</label>
        <asp:DropDownList ID="ddlOwner" runat="server" CssClass="form-control" ClientIDMode="Static">
            <asp:ListItem>Select Owner</asp:ListItem>
        </asp:DropDownList>
    </div>
    <div class="form-group col-lg-4">
        <label>Default unit</label>
        <asp:DropDownList ID="DropDownList2" runat="server" CssClass="form-control" ClientIDMode="Static">
            <asp:ListItem>Select Default Unit</asp:ListItem>
        </asp:DropDownList>
    </div>
</div>
<div class="form-row">
    <label for="pnlDensity" class="col-sm-2 col-form-label">Density</label>
    <div class="col-sm-10" id="pnlDensity">
        <asp:TextBox ID="tbxDensity" runat="server"></asp:TextBox>
        <asp:DropDownList ID="ddlUnitOfWeight" runat="server">
            <asp:ListItem>Lbs</asp:ListItem>
            <asp:ListItem>Kgs</asp:ListItem>
        </asp:DropDownList>
        <asp:DropDownList ID="ddlUnitOfVolume" runat="server">
            <asp:ListItem>L</asp:ListItem>
            <asp:ListItem>Gal</asp:ListItem>
        </asp:DropDownList>
    </div>
</div>

<div class="form-row">
    <div class="form-group col-lg-4">
        <label for="tbxName">EPA Number</label>
        <asp:TextBox ID="tbxEPA" runat="server" CssClass="form-control" ClientIDMode="Static" MaxLength="50" placeholder="Enter EPA number"></asp:TextBox>
    </div>
    <div class="form-group col-lg-4">
        <label for="tbxBarcode">Barcode Number</label>
        <asp:TextBox ID="tbxBarcode" runat="server" CssClass="form-control" ClientIDMode="Static" MaxLength="50" placeholder="Enter barcode number"></asp:TextBox>
    </div>
    <div class="form-group col-lg-4">
        <label>Default unit</label>
        <asp:DropDownList ID="DropDownList1" runat="server" CssClass="form-control" ClientIDMode="Static">
            <asp:ListItem>Select Default Unit</asp:ListItem>
        </asp:DropDownList>
    </div>
</div>
<div class="form-row">
    <div class="form-group col-lg-4">
        <label for="tbxName">EPA Number</label>
        <asp:TextBox ID="TextBox1" runat="server" CssClass="form-control" ClientIDMode="Static" MaxLength="50" placeholder="Enter EPA number"></asp:TextBox>
    </div>
</div>
<div class="form-row">
    <div class="form-group col-12">
        <label for="tbxNotes">Notes</label>
        <asp:TextBox ID="tbxNotes" runat="server" CssClass="form-control" ClientIDMode="Static" TextMode="MultiLine"></asp:TextBox>
    </div>
</div>
<div class="row">
    <label>
        <asp:Label ID="lblCropTypes" runat="server" Text="Crop types" Visible="false"></asp:Label>
    </label>
    <asp:CheckBoxList ID="cblCropTypes" runat="server" CssClass="input" RepeatLayout="UnorderedList">
    </asp:CheckBoxList>
</div>
<asp:Panel runat="server" ID="pnlDerivedFrom" Visible="false">
    <div class="row">
        <label>Derived from</label>
        <div class="input">
            <asp:ListBox ID="lstDerivedFrom" runat="server" CssClass="addRemoveList" AutoPostBack="true"></asp:ListBox>
            <asp:Button ID="btnAddNewDerivedFrom" runat="server" CssClass="addRemoveButton" Text="Add" />
            <asp:Button ID="btnRemoveNewDerivedFrom" runat="server" CssClass="addRemoveButton" Text="Remove" />
        </div>
        <div class="row">
            <label>
                &nbsp;
            </label>
            <div class="input">
                <asp:TextBox ID="tbxDerivedFrom" runat="server" CssClass="addRemoveList"></asp:TextBox>
                <asp:Button ID="btnUpdateDerivedFrom" runat="server" CssClass="addRemoveButton" Text="Update" />
            </div>
        </div>
    </div>
</asp:Panel>
<asp:Panel runat="server" ID="pnlLotTracking" Visible="false">
    <div class="row">
        <label>
            Track specific lot usage during dispensing
        </label>
        <asp:RadioButtonList ID="rblLotUsageTrackingType" runat="server" RepeatLayout="UnorderedList" CssClass="input">
            <asp:ListItem Text="Do not track" Selected="True"></asp:ListItem>
            <asp:ListItem Text="FIFO" Selected="True"></asp:ListItem>
            <asp:ListItem Text="LIFO" Selected="True"></asp:ListItem>
        </asp:RadioButtonList>
    </div>
</asp:Panel>
<asp:Panel runat="server" ID="pnlCustomFields" Visible="false">
    <ul id="lstCustomFields" runat="server">
    </ul>
</asp:Panel>

<UserControls:BackNext runat="server" ID="pnlBackNext" />

