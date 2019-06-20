<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="DefaultEthanolAnalysis.aspx.vb"
    Inherits="KahlerAutomation.TerminalManagement2.DefaultEthanolAnalysis" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <title>Ethanol Analysis</title>
    <link rel="shortcut icon" href="FavIcon.ico" />
    <meta name="viewport" content="width=device-width" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <link rel="stylesheet" href="styles/site.css" />
    <link rel="stylesheet" href="styles/site-tablet.css" media="(max-width:768px)" />
    <link rel="stylesheet" href="styles/site-phone.css" media="(max-width:480px)" />  
</head>
<body>
    <form id="form1" runat="server" height="100%">
    <div class="section">
        <h1>
            <asp:Label ID="lblLastAnalysisAt" runat="server" Text="Last Analysis At:"></asp:Label>
        </h1>
    </div>
    <div class="recordControl">
        <asp:Button ID="btnSave" runat="server" Text="Save" />
        <asp:Label ID="lblStatus" runat="server" ForeColor="Red"></asp:Label>
        <span class="sectionRequiredField"><span class="required"></span>&nbsp;indicates required
            field </span>
    </div>
    <div class="sectionEven">
        <ul>
            <li>
                <label>
                    <asp:Label ID="lblAcidity" runat="server" Text="Acidity"></asp:Label>
                </label>
                <span class="input"><span class="required">
                    <asp:TextBox ID="tbxAcidity" runat="server" CssClass="inputNumeric">0</asp:TextBox>%
                </span></span></li>
            <li>
                <label>
                    <asp:Label ID="lblApparentProof" runat="server" Text="Apparent Proof"></asp:Label>
                </label>
                <span class="input"><span class="required">
                    <asp:TextBox ID="tbxApparentProof" runat="server" CssClass="inputNumeric">0</asp:TextBox>
                    % </span></span></li>
            <li>
                <label>
                    <asp:Label ID="lblAromatic" runat="server" Text="Aromatic"></asp:Label>
                </label>
                <span class="input"><span class="required">
                    <asp:TextBox ID="tbxAromatic" runat="server" CssClass="inputNumeric">0</asp:TextBox>
                    % </span></span></li>
            <li>
                <label>
                    <asp:Label ID="lblBenzene" runat="server" Text="Benzene"></asp:Label>
                </label>
                <span class="input"><span class="required">
                    <asp:TextBox ID="tbxBenzene" runat="server" CssClass="inputNumeric">0</asp:TextBox>
                    % </span></span></li>
            <li>
                <label>
                    <asp:Label ID="lblCopperContent" runat="server" Text="Copper Content"></asp:Label>
                </label>
                <span class="input"><span class="required">
                    <asp:TextBox ID="tbxCopperContent" runat="server" CssClass="inputNumeric">0</asp:TextBox>
                    PPM </span></span></li>
            <li>
                <label>
                    <asp:Label ID="lblDenaturantVolume" runat="server" Text="Denaturant Volume"></asp:Label>
                </label>
                <span class="input"><span class="required">
                    <asp:TextBox ID="tbxDenaturantVolume" runat="server" CssClass="inputNumeric">0</asp:TextBox>
                    % </span></span></li>
            <li>
                <label>
                    <asp:Label ID="lblEthanolVolumePercent" runat="server" Text="Ethanol Volume %"></asp:Label>
                </label>
                <span class="input"><span class="required">
                    <asp:TextBox ID="tbxEthanolVolume" runat="server" CssClass="inputNumeric">0</asp:TextBox>
                    % </span></span></li>
            <li>
                <label>
                    <asp:Label ID="lblInorganicChlorideContent" runat="server" Text="Inorganic Chloride Content"></asp:Label>
                </label>
                <span class="input"><span class="required">
                    <asp:TextBox ID="tbxInorganicChlorideContent" runat="server" CssClass="inputNumeric">0</asp:TextBox>
                    PPM </span></span></li>
            <li>
                <label>
                    <asp:Label ID="lblMethanol" runat="server" Text="Methanol"></asp:Label>
                </label>
                <span class="input"><span class="required">
                    <asp:TextBox ID="tbxMethanol" runat="server" CssClass="inputNumeric">0</asp:TextBox>
                    % </span></span></li>
            <li>
                <label>
                    <asp:Label ID="lblOlefins" runat="server" Text="Olefins"></asp:Label>
                </label>
                <span class="input"><span class="required">
                    <asp:TextBox ID="tbxOlefins" runat="server" CssClass="inputNumeric">0</asp:TextBox>
                    % </span></span></li>
        </ul>
    </div>
    <div class="sectionOdd">
        <ul>
            <li>
                <label>
                    <asp:Label ID="lblPHE" runat="server" Text="PHE"></asp:Label>
                </label>
                <span class="input"><span class="required">
                    <asp:TextBox ID="tbxPHE" runat="server" CssClass="inputNumeric">0</asp:TextBox>
                    % </span></span></li>
            <li>
                <label>
                    <asp:Label ID="lblPercentWater" runat="server" Text="Percent Water"></asp:Label>
                </label>
                <span class="input"><span class="required">
                    <asp:TextBox ID="tbxPercentWater" runat="server" CssClass="inputNumeric">0</asp:TextBox>
                    % </span></span></li>
            <li>
                <label>
                    <asp:Label ID="lblSolventWashedGum" runat="server" Text="Solvent Washed Gum"></asp:Label>
                </label>
                <span class="input"><span class="required">
                    <asp:TextBox ID="tbxSolventWashedGum" runat="server" CssClass="inputNumeric">0</asp:TextBox>
                    PPM </span></span></li>
            <li>
                <label>
                    <asp:Label ID="lblSulfates" runat="server" Text="Sulfates"></asp:Label>
                </label>
                <span class="input"><span class="required">
                    <asp:TextBox ID="tbxSulfates" runat="server" CssClass="inputNumeric">0</asp:TextBox>
                    PPM </span></span></li>
            <li>
                <label>
                    <asp:Label ID="lblSulfur" runat="server" Text="Sulfur"></asp:Label>
                </label>
                <span class="input"><span class="required">
                    <asp:TextBox ID="tbxSulfur" runat="server" CssClass="inputNumeric">0</asp:TextBox>
                    PPM </span></span></li>
            <li>
                <label>
                    <asp:Label ID="lblVisualAppearance" runat="server" Text="Visual Appearance"></asp:Label>
                </label>
                <asp:TextBox ID="tbxVisualAppearance" runat="server"></asp:TextBox>
            </li>
            <li>
                <label>
                    <asp:Label ID="lblSource" runat="server" Text="Source"></asp:Label>
                </label>
                <asp:TextBox ID="tbxSource" runat="server"></asp:TextBox>
            </li>
            <li>
                <label>
                    <asp:Label ID="lblScaleNumber" runat="server" Text="Scale #"></asp:Label>
                </label>
                <asp:TextBox ID="tbxScaleNumber" runat="server"></asp:TextBox>
            </li>
            <li>
                <label>
                    <asp:Label ID="lblAnalysisDate" runat="server" Text="Analysis Date"></asp:Label>
                </label>
                <span class="required">
                    <asp:TextBox ID="tbxAnalysisDate" runat="server"></asp:TextBox>
                </span></li>
            <li>
                <label>
                    <asp:Label ID="lblAnalysisBy" runat="server" Text="Analysis By"></asp:Label>
                </label>
                <span class="required">
                    <asp:TextBox ID="tbxAnalysisBy" runat="server"></asp:TextBox>
                </span></li>
        </ul>
    </div>
    </form>
</body>
</html>
