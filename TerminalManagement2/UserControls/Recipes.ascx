<%@ Control Language="vb" AutoEventWireup="true" CodeBehind="Recipes.ascx.vb" Inherits="KahlerAutomation.TerminalManagement2.Recipes" %>
<%@ Register Src="~/UserControls/BulkProducts/BulkProductSetup.ascx" TagPrefix="friendlyUrls" TagName="BulkProductSetup" %>

<div class="row justify-content-between">
    <h2 class="col">Recipes</h2>
    <button type="button" class="btn btn-primary col-4 mt-2" data-toggle="modal" data-target="#exampleModal">
        Create Or Edit Bulk Products
    </button>
</div>
<div class="mb-2">
    <asp:Repeater runat="server" ID="rpFacilities">
        <ItemTemplate>
            <div class="card mt-3">
                <div class="card-header">
                    <h4 class="mb-0 d-inline"><%# Eval("Name") %></h4>
                    <asp:LinkButton runat="server" ID="btnAddRemove" CssClass="btn btn-link font-weight-bold pt-0 pl-0" OnCommand="btnAddRemove_Command" CommandName='<%# If(CType(Eval("Display"), Boolean), "Remove", "Add") %>' CommandArgument='<%# Eval("Id") %>'>
                        <%# If(CType(Eval("Display"), Boolean), "- Remove Recipe", "- Add Recipe") %>
                    </asp:LinkButton>
                    <div class="row justify-content-end m-0 d-inline float-right">
                        <button class="btn btn-link pb-0 collapse <%# If(CType(Eval("Display"), Boolean), "show", "") %>" id="btn<%# Eval("Id") %>" type="button" data-toggle="collapse" data-target="#pnl<%# Eval("Id") %>" onclick="changeHideShow(this);"><%# If(CType(Eval("Display"), Boolean), "Show", "Hide") %></button>
                    </div>
                </div>
                <div id="pnl<%# Eval("Id") %>" class="collapse card-body p-0 <%# If(CType(Eval("Display"), Boolean), "show", "") %>">
                    <asp:Repeater runat="server" ID="rpBulkProducts">
                        <HeaderTemplate>
                            <table border="1" id="tblBulkProducts" class="table table-sm w-100 m-0">
                                <tr>
                                    <th class="text-right">Bulk Product
                                        <asp:LinkButton runat="server" CssClass="btn btn-primary btn-sm fa fa-plus float-left" data-toggle="modal" data-target="#addEditRecipeModal" />
                                    </th>
                                    <th>Portion</th>
                                </tr>
                        </HeaderTemplate>
                        <ItemTemplate>
                            <tr>
                                <td class="text-right"><%# Eval("Name") %></td>
                                <td><%# Eval("Portion") %>%</td>
                                <td style="border: none; width:155px">
                                    <asp:LinkButton runat="server" ID="btnDown" CssClass="btn btn-primary btn-sm fa fa-arrow-down" />
                                    <asp:LinkButton runat="server" ID="btnUp" CssClass="btn btn-primary btn-sm fa fa-arrow-up" />
                                    <asp:LinkButton runat="server" CssClass="btn btn-primary btn-sm fa fa-edit" data-toggle="modal" data-target="#addEditRecipeModal" />
                                    <asp:LinkButton runat="server" CssClass="btn btn-primary btn-sm fa fa-trash" />
                                </td>
                            </tr>
                        </ItemTemplate>
                        <FooterTemplate>
                            <tr>
                                <td class="text-right">Total:</td>
                                <td><%# DataBinder.Eval(CType(Container.Parent.Parent, RepeaterItem).DataItem, "Total")%>%</td>
                            </tr>
                            </table>
                        </FooterTemplate>
                    </asp:Repeater>
                </div>
            </div>
        </ItemTemplate>
    </asp:Repeater>
</div>
<div class="modal fade" id="exampleModal" tabindex="-1" role="dialog">
    <div class="modal-dialog modal-xl" role="document">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title" id="exampleModalLabel">Bulk Product Setup</h5>
                <button type="button" class="close" data-dismiss="modal">
                    <span>&times;</span>
                </button>
            </div>
            <div class="modal-body">
                <asp:UpdatePanel runat="server">
                    <ContentTemplate>
                        <friendlyUrls:BulkProductSetup runat="server" />
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
        </div>
    </div>
</div>
<div class="modal fade" id="addEditRecipeModal" tabindex="-1" role="dialog">
    <div class="modal-dialog" role="document">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title" id="exampleModalLabel">Bulk Product Setup</h5>
                <button type="button" class="close" data-dismiss="modal">
                    <span>&times;</span>
                </button>
            </div>
            <div class="modal-body">
                <asp:UpdatePanel runat="server" ChildrenAsTriggers="true" UpdateMode="Conditional">
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="ddlBulkProduct" EventName="SelectedIndexChanged" />
                    </Triggers>
                    <ContentTemplate>
                        <div class="form-group">
                            <label for="ddlBulkProduct">Bulk product</label>
                            <asp:DropDownList ID="ddlBulkProduct" runat="server" ClientIDMode="AutoID" AutoPostBack="true" CssClass="form-control" OnSelectedIndexChanged="ddlBulkProduct_SelectedIndexChanged" />
                        </div>
                        <div class="form-group">
                            <label for="tbxPercent">Percent of total</label>
                            <asp:TextBox ID="tbxPercent" runat="server" CssClass="form-control" ClientIDMode="Static" placeholder="Enter product percent"></asp:TextBox>
                            <div class="input-group-append">
                                <asp:Label runat="server" ID="lblPortionUnit" CssClass="input-group-text" />
                            </div>
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-secondary" data-dismiss="modal">Cancel</button>
                <button type="button" class="btn btn-primary">Apply</button>
            </div>
        </div>
    </div>
</div>

