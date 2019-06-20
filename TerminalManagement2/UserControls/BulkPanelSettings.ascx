<%@ Control Language="vb" AutoEventWireup="true" CodeBehind="BulkPanelSettings.ascx.vb" Inherits="KahlerAutomation.TerminalManagement2.BulkPanelSettings" %>

<div class="section">
    <h3>Bulk Product Panel Settings</h3>
</div>

<asp:Repeater runat="server" ID="rpFacility">
    <ItemTemplate>
        <div class="card">
            <div class="card-header">
                <h5 class="mb-0"><%# DataBinder.Eval(Container.DataItem, "Name") %></h5>
            </div>
            <div class="card-body p-1">

                <asp:Repeater runat="server" ID="rpPanels">
                    <HeaderTemplate>
                        <table id="tblPanelBulkProducts" class="table">
                            <tr>
                                <th class="border-top-0">Bulk Product</th>
                                <th class="border-top-0">Panel</th>
                                <th class="border-top-0">Panel function</th>
                            </tr>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <tr>
                            <td rowspan="2">
                                <div class="d-inline-block">
                                    <button type="button" class="btn btn-primary btn-sm fa fa-plus" data-toggle="modal" data-target="#exampleModal" />
                                </div>
                                <h5 class="d-inline-block"><%# DataBinder.Eval(Container.DataItem, "Name") %></h5>
                            </td>
                            <td>Weigh Hopper</td>
                            <td>Product 1
                                <button type="button" class="btn btn-primary btn-sm fa fa-trash float-right" data-toggle="modal" data-target="#deleteModal" />
                                <button type="button" class="btn btn-primary btn-sm fa fa-edit float-right" data-toggle="modal" data-target="#exampleModal" />
                            </td>
                        </tr>
                        <tr>
                            <td>Mixer</td>
                            <td>Hand-add
                            <button type="button" class="btn btn-primary btn-sm fa fa-trash float-right" data-toggle="modal" data-target="#deleteModal" />
                                <button type="button" class="btn btn-primary btn-sm fa fa-edit float-right" data-toggle="modal" data-target="#exampleModal" />
                            </td>
                        </tr>
                    </ItemTemplate>
                    <FooterTemplate>
                        </table>
                    </FooterTemplate>
                </asp:Repeater>
            </div>
        </div>
    </ItemTemplate>
</asp:Repeater>
<div class="modal fade" id="exampleModal" tabindex="-1" role="dialog" aria-labelledby="exampleModalLabel" aria-hidden="true">
    <div class="modal-dialog" role="document">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title" id="exampleModalLabel">Panel settings for bulk product</h5>
                <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                    <span aria-hidden="true">&times;</span>
                </button>
            </div>
            <div class="modal-body">
                <asp:Panel ID="pnlSettings" runat="server">

                    <div class="form-group">
                        <label for="tbxBulkProduct">Bulk Product</label>
                        <asp:TextBox ID="tbxBulkProduct" runat="server" ReadOnly="true" CssClass="form-control" ClientIDMode="Static" MaxLength="50" placeholder="Enter product name"></asp:TextBox>
                    </div>
                    <div class="form-group">
                        <label for="ddlFunction">Panel function</label>
                        <asp:DropDownList ID="ddlFunction" runat="server" />
                    </div>
                    <div class="form-group">
                        <label for="tbxStartParameter">Start parameter (flood time, start bumps)</label>
                        <asp:TextBox ID="tbxStartParameter" runat="server" CssClass="form-control" ClientIDMode="Static" />
                    </div>
                    <li>
                        <label>
                            Finishing parameter (purge time)</label>
                        <asp:TextBox ID="tbxFinishingParameter" runat="server" />
                    </li>
                    <li>
                        <label>
                        </label>
                        <asp:CheckBox ID="chkAlwaysUseFinishParameter" runat="server" Text="Always use purge time"
                            Enabled="False" />
                    </li>
                    <li>
                        <label>
                            Anticipation</label>
                        <span class="input">
                            <asp:TextBox ID="tbxAnticipation" runat="server" Width="60%" />&nbsp;
					<asp:DropDownList ID="ddlAnticipationUnit" runat="server" Width="30%" />
                        </span></li>
                    <li>
                        <label>
                            Anticipation update factor (0 to 1)</label>
                        <asp:TextBox ID="tbxAnticipationUpdateFactor" runat="server" />
                    </li>
                    <li>
                        <label>
                            Conversion factor</label>
                        <span class="input">
                            <asp:TextBox ID="tbxConversionFactor" runat="server" Width="30%" />
                            pulses per
					<asp:DropDownList ID="ddlConversionFactorUnit" runat="server" Width="30%" />
                        </span></li>
                    <li>
                        <label>
                        </label>
                        <asp:CheckBox ID="chkUpdateDensityUsingMeter" runat="server" Text="Update density using meter" />
                    </li>
                    <li>
                        <label>
                        </label>
                        <asp:CheckBox ID="chkUseAverageDensityForTicket" runat="server" Text="Use average density for ticket" />
                    </li>
                    <li>
                        <label>
                            Dump time (sec)</label>
                        <asp:TextBox ID="tbxDumpTime" runat="server" />
                    </li>
                    <li>
                        <label>
                        </label>
                        <asp:CheckBox ID="chkDisabled" runat="server" Text="Disabled" />
                    </li>
                </asp:Panel>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-secondary" data-dismiss="modal">Close</button>
                <button type="button" class="btn btn-primary">Save changes</button>
            </div>
        </div>
    </div>
</div>
<div class="modal fade" id="deleteModal" tabindex="-1" role="dialog">
    <div class="modal-dialog" role="document">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title" id="exampleModalLabel">Panel settings for bulk product</h5>
                <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                    <span aria-hidden="true">&times;</span>
                </button>
            </div>
            <div class="modal-body">
                <h2>Are you sure you want to delete this panel?</h2>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-secondary" data-dismiss="modal">Cancel</button>
                <button type="button" class="btn btn-primary">Delete</button>
            </div>
        </div>
    </div>
</div>
