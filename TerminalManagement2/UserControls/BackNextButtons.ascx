<%@ Control Language="vb" AutoEventWireup="true" CodeBehind="BackNextButtons.ascx.vb" Inherits="KahlerAutomation.TerminalManagement2.BackNextButtons" %>
<div class="row justify-content-between">
    <div class="col">
        <asp:Button runat="server" ID="btnBack" CssClass="btn btn-primary" Text="Back" OnClick="btnBack_Click" />
    </div>
    <div class="col">
        <asp:Button runat="server" ID="btnNext" CssClass="btn btn-primary float-right" Text="Next" OnClick="btnNext_Click" />
    </div>
</div>
