<%@ Control Language="vb" AutoEventWireup="true" CodeBehind="ProductInterfaces.ascx.vb" Inherits="KahlerAutomation.TerminalManagement2.ProductInterfaces" %>

<div class="row justify-content-center">
    <h3>Will this product be imported or exported using an interface?</h3>
</div>
<div class="row justify-content-center">
    <button class="btn btn-primary col-3 mr-3" type="button" data-toggle="collapse" data-target="#pnlInterfaces">Yes</button>
    <button class="btn btn-primary col-3" type="button">No</button>
</div>
<div class="collapse mt-2 mb-2" id="pnlInterfaces">
    <div class="card">
        <div class="card-header">
            <h4 class="mb-0 d-inline">Tm2 AgVance Interface</h4>
            <button class="btn btn-link float-right" data-toggle="collapse" data-target="#collapseOne" type="button">
                Define Cross Reference
            </button>
        </div>
        <div class="card-body collapse" id="collapseOne">
            <div class="form-row">
                <div class="form-group col-md-4">
                    <label for="tbxInterfaceCrossReference">Cross reference</label>
                    <asp:TextBox ID="tbxInterfaceCrossReference" runat="server" CssClass="form-control" ClientIDMode="Static" MaxLength="50" placeholder="Enter Cross Reference"></asp:TextBox>
                </div>
                <div class="form-group col-md-4">
                    <label for="ddlInterfaceUnit">Interface exchange unit</label>
                    <asp:DropDownList ID="ddlInterfaceUnit" runat="server" CssClass="form-control" ClientIDMode="Static">
                        <asp:ListItem>Select Unit</asp:ListItem>
                    </asp:DropDownList>
                </div>
                <div class="form-group col-md-4">
                    <label for="ddlInterfaceOrderItemUnit">Order item unit</label>
                    <asp:DropDownList ID="ddlInterfaceOrderItemUnit" runat="server" CssClass="form-control" ClientIDMode="Static">
                        <asp:ListItem>Select Unit</asp:ListItem>
                    </asp:DropDownList>
                </div>
            </div>
            <div class="form-row">
                <div class="form-group col-md-4">
                    <label for="ddlSplitProductFormulationFacility">Split product formulation facility</label>
                    <asp:DropDownList ID="ddlSplitProductFormulationFacility" runat="server" CssClass="form-control" ClientIDMode="Static">
                        <asp:ListItem>Select Facility</asp:ListItem>
                    </asp:DropDownList>
                </div>
                <div class="form-group col-md-4">
                    <label for="tbxPriority">Order item sort priority</label>
                    <asp:TextBox ID="tbxPriority" runat="server" CssClass="form-control" ClientIDMode="Static" MaxLength="50" placeholder="Enter sort priority"></asp:TextBox>
                </div>
            </div>
            <div class="form-group">
                <div class="form-check">
                    <asp:CheckBox ID="chkDefaultSetting" runat="server" ClientIDMode="Static" class="form-check-input" />
                    <label class="mt-1" for="chkDefaultSetting">Default Setting</label>
                </div>
                <div class="form-check">
                    <asp:CheckBox ID="chkExportOnly" runat="server" ClientIDMode="Static" class="form-check-input" />
                    <label class="mt-1" for="chkExportOnly">Export only</label>
                </div>
            </div>
        </div>
    </div>
    <div class="card">
        <div class="card-header" id="headingTwo">
            <h4 class="mb-0 d-inline">Other Tm2 AgVance Interface</h4>
            <button class="btn btn-link collapsed" data-toggle="collapse" data-target="#collapseTwo" type="button">
            </button>
        </div>
        <div id="collapseTwo" class="collapse">
            <div class="card-body">
            </div>
        </div>
    </div>
    <div class="card">
        <div class="card-header" id="headingThree">
            <h4 class="mb-0 d-inline">Tm2 Agris Interface</h4>
            <button class="btn btn-link collapsed" data-toggle="collapse" data-target="#collapseThree" type="button">
            </button>
        </div>
        <div id="collapseThree" class="collapse">
            <div class="card-body">
                Anim pariatur cliche reprehenderit, enim eiusmod high life accusamus terry richardson ad squid. 3 wolf moon officia aute, non cupidatat skateboard dolor brunch. Food truck quinoa nesciunt laborum eiusmod. Brunch 3 wolf moon tempor, sunt aliqua put a bird on it squid single-origin coffee nulla assumenda shoreditch et. Nihil anim keffiyeh helvetica, craft beer labore wes anderson cred nesciunt sapiente ea proident. Ad vegan excepteur butcher vice lomo. Leggings occaecat craft beer farm-to-table, raw denim aesthetic synth nesciunt you probably haven't heard of them accusamus labore sustainable VHS.
            </div>
        </div>
    </div>
</div>

