<%@ Control Language="vb" AutoEventWireup="true" CodeBehind="SelectProduct.ascx.vb" Inherits="KahlerAutomation.TerminalManagement2.SelectProduct" %>

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
<div class="container">
    <div class="row justify-content-center">
        <h4 class="bs">Would you like to create a new or update an existing product?</h4>
    </div>
    <div class="row justify-content-center">
        <asp:LinkButton runat="server" Text="Create New Product" ID="btnCreateProduct" CssClass="btn btn-primary col-md-4 m-2" OnClick="CreateNewProduct_Click" />
        <button class="btn btn-primary col-md-4 m-2" type="button" data-toggle="collapse" data-target="#pnlExistingProducts">Update Existing Product</button>
    </div>
    <div class="collapse mb-3" id="pnlExistingProducts">
        <div class="row">
            <h4 class="col">Select the product you would like to update</h4>
        </div>
        <div class="form-group">
            <input class="form-control col" id="tbxSearch" type="text" placeholder="Enter product to search for...">
        </div>
        <div class="row">
            <asp:Repeater runat="server" ID="rpProductItems">
                <ItemTemplate>
                    <div class="col-md-3 col-sm-6 ">
                        <asp:LinkButton runat="server" class="list-group-item list-group-item-action text-truncate"
                        Text='<%# Eval("Name") %>' OnCommand="Product_Command" CommandName="Clicked" CommandArgument='<%# Eval("Id") %>' />
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </div>
    </div>
</div>
