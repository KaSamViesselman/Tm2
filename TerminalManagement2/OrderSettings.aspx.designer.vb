﻿'------------------------------------------------------------------------------
' <auto-generated>
'     This code was generated by a tool.
'
'     Changes to this file may cause incorrect behavior and will be lost if
'     the code is regenerated. 
' </auto-generated>
'------------------------------------------------------------------------------

Option Strict On
Option Explicit On


Partial Public Class OrderSettings
    
    '''<summary>
    '''main control.
    '''</summary>
    '''<remarks>
    '''Auto-generated field.
    '''To modify move field declaration from designer file to code-behind file.
    '''</remarks>
    Protected WithEvents main As Global.System.Web.UI.HtmlControls.HtmlForm
    
    '''<summary>
    '''btnSave control.
    '''</summary>
    '''<remarks>
    '''Auto-generated field.
    '''To modify move field declaration from designer file to code-behind file.
    '''</remarks>
    Protected WithEvents btnSave As Global.System.Web.UI.WebControls.Button
    
    '''<summary>
    '''lblSave control.
    '''</summary>
    '''<remarks>
    '''Auto-generated field.
    '''To modify move field declaration from designer file to code-behind file.
    '''</remarks>
    Protected WithEvents lblSave As Global.System.Web.UI.WebControls.Label
    
    '''<summary>
    '''pnlOrderSettings control.
    '''</summary>
    '''<remarks>
    '''Auto-generated field.
    '''To modify move field declaration from designer file to code-behind file.
    '''</remarks>
    Protected WithEvents pnlOrderSettings As Global.System.Web.UI.HtmlControls.HtmlGenericControl
    
    '''<summary>
    '''cbxAutoGenerate control.
    '''</summary>
    '''<remarks>
    '''Auto-generated field.
    '''To modify move field declaration from designer file to code-behind file.
    '''</remarks>
    Protected WithEvents cbxAutoGenerate As Global.System.Web.UI.WebControls.CheckBox
    
    '''<summary>
    '''cbxAllowModification control.
    '''</summary>
    '''<remarks>
    '''Auto-generated field.
    '''To modify move field declaration from designer file to code-behind file.
    '''</remarks>
    Protected WithEvents cbxAllowModification As Global.System.Web.UI.WebControls.CheckBox
    
    '''<summary>
    '''cbxSeparateOrderNumberPerOwner control.
    '''</summary>
    '''<remarks>
    '''Auto-generated field.
    '''To modify move field declaration from designer file to code-behind file.
    '''</remarks>
    Protected WithEvents cbxSeparateOrderNumberPerOwner As Global.System.Web.UI.WebControls.CheckBox
    
    '''<summary>
    '''cbxCreateNewDestinationFromOrderShipToInformation control.
    '''</summary>
    '''<remarks>
    '''Auto-generated field.
    '''To modify move field declaration from designer file to code-behind file.
    '''</remarks>
    Protected WithEvents cbxCreateNewDestinationFromOrderShipToInformation As Global.System.Web.UI.WebControls.CheckBox
    
    '''<summary>
    '''tbxStartingOrderNumber control.
    '''</summary>
    '''<remarks>
    '''Auto-generated field.
    '''To modify move field declaration from designer file to code-behind file.
    '''</remarks>
    Protected WithEvents tbxStartingOrderNumber As Global.System.Web.UI.WebControls.TextBox
    
    '''<summary>
    '''lblSaveNextOrderNumber control.
    '''</summary>
    '''<remarks>
    '''Auto-generated field.
    '''To modify move field declaration from designer file to code-behind file.
    '''</remarks>
    Protected WithEvents lblSaveNextOrderNumber As Global.System.Web.UI.WebControls.Label
    
    '''<summary>
    '''btnSaveNextOrderNumber control.
    '''</summary>
    '''<remarks>
    '''Auto-generated field.
    '''To modify move field declaration from designer file to code-behind file.
    '''</remarks>
    Protected WithEvents btnSaveNextOrderNumber As Global.System.Web.UI.WebControls.Button
    
    '''<summary>
    '''cbxPartialDelete control.
    '''</summary>
    '''<remarks>
    '''Auto-generated field.
    '''To modify move field declaration from designer file to code-behind file.
    '''</remarks>
    Protected WithEvents cbxPartialDelete As Global.System.Web.UI.WebControls.CheckBox
    
    '''<summary>
    '''cbxLockOwner control.
    '''</summary>
    '''<remarks>
    '''Auto-generated field.
    '''To modify move field declaration from designer file to code-behind file.
    '''</remarks>
    Protected WithEvents cbxLockOwner As Global.System.Web.UI.WebControls.CheckBox
    
    '''<summary>
    '''cbxLockBranch control.
    '''</summary>
    '''<remarks>
    '''Auto-generated field.
    '''To modify move field declaration from designer file to code-behind file.
    '''</remarks>
    Protected WithEvents cbxLockBranch As Global.System.Web.UI.WebControls.CheckBox
    
    '''<summary>
    '''cbxLockRunOverPercent control.
    '''</summary>
    '''<remarks>
    '''Auto-generated field.
    '''To modify move field declaration from designer file to code-behind file.
    '''</remarks>
    Protected WithEvents cbxLockRunOverPercent As Global.System.Web.UI.WebControls.CheckBox
    
    '''<summary>
    '''tbxOrderComparisonPercentTolerance control.
    '''</summary>
    '''<remarks>
    '''Auto-generated field.
    '''To modify move field declaration from designer file to code-behind file.
    '''</remarks>
    Protected WithEvents tbxOrderComparisonPercentTolerance As Global.System.Web.UI.WebControls.TextBox
    
    '''<summary>
    '''cbxShowReleaseNumberInOrderList control.
    '''</summary>
    '''<remarks>
    '''Auto-generated field.
    '''To modify move field declaration from designer file to code-behind file.
    '''</remarks>
    Protected WithEvents cbxShowReleaseNumberInOrderList As Global.System.Web.UI.WebControls.CheckBox
    
    '''<summary>
    '''ddlInternalTransferCustomerAccount control.
    '''</summary>
    '''<remarks>
    '''Auto-generated field.
    '''To modify move field declaration from designer file to code-behind file.
    '''</remarks>
    Protected WithEvents ddlInternalTransferCustomerAccount As Global.System.Web.UI.WebControls.DropDownList
    
    '''<summary>
    '''cbxSendOrderSummaryToApplicator control.
    '''</summary>
    '''<remarks>
    '''Auto-generated field.
    '''To modify move field declaration from designer file to code-behind file.
    '''</remarks>
    Protected WithEvents cbxSendOrderSummaryToApplicator As Global.System.Web.UI.WebControls.CheckBox
    
    '''<summary>
    '''cbxSendOrderSummaryToBranch control.
    '''</summary>
    '''<remarks>
    '''Auto-generated field.
    '''To modify move field declaration from designer file to code-behind file.
    '''</remarks>
    Protected WithEvents cbxSendOrderSummaryToBranch As Global.System.Web.UI.WebControls.CheckBox
    
    '''<summary>
    '''cbxSendOrderSummaryToCustomerAccount control.
    '''</summary>
    '''<remarks>
    '''Auto-generated field.
    '''To modify move field declaration from designer file to code-behind file.
    '''</remarks>
    Protected WithEvents cbxSendOrderSummaryToCustomerAccount As Global.System.Web.UI.WebControls.CheckBox
    
    '''<summary>
    '''cbxSendOrderSummaryToCustomerAccountLocation control.
    '''</summary>
    '''<remarks>
    '''Auto-generated field.
    '''To modify move field declaration from designer file to code-behind file.
    '''</remarks>
    Protected WithEvents cbxSendOrderSummaryToCustomerAccountLocation As Global.System.Web.UI.WebControls.CheckBox
    
    '''<summary>
    '''cbxSendOrderSummaryToOwner control.
    '''</summary>
    '''<remarks>
    '''Auto-generated field.
    '''To modify move field declaration from designer file to code-behind file.
    '''</remarks>
    Protected WithEvents cbxSendOrderSummaryToOwner As Global.System.Web.UI.WebControls.CheckBox
    
    '''<summary>
    '''pnlPresetOrderSettings control.
    '''</summary>
    '''<remarks>
    '''Auto-generated field.
    '''To modify move field declaration from designer file to code-behind file.
    '''</remarks>
    Protected WithEvents pnlPresetOrderSettings As Global.System.Web.UI.HtmlControls.HtmlGenericControl
    
    '''<summary>
    '''ddlOwners control.
    '''</summary>
    '''<remarks>
    '''Auto-generated field.
    '''To modify move field declaration from designer file to code-behind file.
    '''</remarks>
    Protected WithEvents ddlOwners As Global.System.Web.UI.WebControls.DropDownList
    
    '''<summary>
    '''ddlBranches control.
    '''</summary>
    '''<remarks>
    '''Auto-generated field.
    '''To modify move field declaration from designer file to code-behind file.
    '''</remarks>
    Protected WithEvents ddlBranches As Global.System.Web.UI.WebControls.DropDownList
    
    '''<summary>
    '''tbxRunOverPercent control.
    '''</summary>
    '''<remarks>
    '''Auto-generated field.
    '''To modify move field declaration from designer file to code-behind file.
    '''</remarks>
    Protected WithEvents tbxRunOverPercent As Global.System.Web.UI.WebControls.TextBox
    
    '''<summary>
    '''pnlPointOfSaleSettings control.
    '''</summary>
    '''<remarks>
    '''Auto-generated field.
    '''To modify move field declaration from designer file to code-behind file.
    '''</remarks>
    Protected WithEvents pnlPointOfSaleSettings As Global.System.Web.UI.HtmlControls.HtmlGenericControl
    
    '''<summary>
    '''chkUseOrderPercentage control.
    '''</summary>
    '''<remarks>
    '''Auto-generated field.
    '''To modify move field declaration from designer file to code-behind file.
    '''</remarks>
    Protected WithEvents chkUseOrderPercentage As Global.System.Web.UI.WebControls.CheckBox
    
    '''<summary>
    '''chkAllowOrdersToBeAssignedToMultipleStagedOrders control.
    '''</summary>
    '''<remarks>
    '''Auto-generated field.
    '''To modify move field declaration from designer file to code-behind file.
    '''</remarks>
    Protected WithEvents chkAllowOrdersToBeAssignedToMultipleStagedOrders As Global.System.Web.UI.WebControls.CheckBox
    
    '''<summary>
    '''chkLimitDriversToDriversAssignedToAccount control.
    '''</summary>
    '''<remarks>
    '''Auto-generated field.
    '''To modify move field declaration from designer file to code-behind file.
    '''</remarks>
    Protected WithEvents chkLimitDriversToDriversAssignedToAccount As Global.System.Web.UI.WebControls.CheckBox
    
    '''<summary>
    '''chkLimitTransportsToCarrierSelected control.
    '''</summary>
    '''<remarks>
    '''Auto-generated field.
    '''To modify move field declaration from designer file to code-behind file.
    '''</remarks>
    Protected WithEvents chkLimitTransportsToCarrierSelected As Global.System.Web.UI.WebControls.CheckBox
    
    '''<summary>
    '''cbxEmailCreatedPointOfSaleTickets control.
    '''</summary>
    '''<remarks>
    '''Auto-generated field.
    '''To modify move field declaration from designer file to code-behind file.
    '''</remarks>
    Protected WithEvents cbxEmailCreatedPointOfSaleTickets As Global.System.Web.UI.WebControls.CheckBox
    
    '''<summary>
    '''cblStagedOrderShortcuts control.
    '''</summary>
    '''<remarks>
    '''Auto-generated field.
    '''To modify move field declaration from designer file to code-behind file.
    '''</remarks>
    Protected WithEvents cblStagedOrderShortcuts As Global.System.Web.UI.WebControls.CheckBoxList
    
    '''<summary>
    '''pnlStagedOrderCustomShortcuts control.
    '''</summary>
    '''<remarks>
    '''Auto-generated field.
    '''To modify move field declaration from designer file to code-behind file.
    '''</remarks>
    Protected WithEvents pnlStagedOrderCustomShortcuts As Global.System.Web.UI.HtmlControls.HtmlGenericControl
    
    '''<summary>
    '''cblStagedOrderCustomShortcuts control.
    '''</summary>
    '''<remarks>
    '''Auto-generated field.
    '''To modify move field declaration from designer file to code-behind file.
    '''</remarks>
    Protected WithEvents cblStagedOrderCustomShortcuts As Global.System.Web.UI.WebControls.CheckBoxList
    
    '''<summary>
    '''pnlPoSCustomLoadQuestions control.
    '''</summary>
    '''<remarks>
    '''Auto-generated field.
    '''To modify move field declaration from designer file to code-behind file.
    '''</remarks>
    Protected WithEvents pnlPoSCustomLoadQuestions As Global.System.Web.UI.HtmlControls.HtmlGenericControl
    
    '''<summary>
    '''pnlPoSCustomPreLoadQuestions control.
    '''</summary>
    '''<remarks>
    '''Auto-generated field.
    '''To modify move field declaration from designer file to code-behind file.
    '''</remarks>
    Protected WithEvents pnlPoSCustomPreLoadQuestions As Global.System.Web.UI.HtmlControls.HtmlGenericControl
    
    '''<summary>
    '''cblPoSCustomPreLoadQuestion control.
    '''</summary>
    '''<remarks>
    '''Auto-generated field.
    '''To modify move field declaration from designer file to code-behind file.
    '''</remarks>
    Protected WithEvents cblPoSCustomPreLoadQuestion As Global.System.Web.UI.WebControls.CheckBoxList
    
    '''<summary>
    '''pnlPoSCustomPostLoadQuestion control.
    '''</summary>
    '''<remarks>
    '''Auto-generated field.
    '''To modify move field declaration from designer file to code-behind file.
    '''</remarks>
    Protected WithEvents pnlPoSCustomPostLoadQuestion As Global.System.Web.UI.HtmlControls.HtmlGenericControl
    
    '''<summary>
    '''cblPoSCustomPostLoadQuestion control.
    '''</summary>
    '''<remarks>
    '''Auto-generated field.
    '''To modify move field declaration from designer file to code-behind file.
    '''</remarks>
    Protected WithEvents cblPoSCustomPostLoadQuestion As Global.System.Web.UI.WebControls.CheckBoxList
End Class