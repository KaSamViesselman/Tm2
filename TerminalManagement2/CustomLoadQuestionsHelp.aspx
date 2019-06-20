<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="CustomLoadQuestionsHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.CustomLoadQuestionsHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head id="Head1" runat="server">
	<title>Help : Custom Load Questions</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="InspectionQuestionsHelp" runat="server">
		<div>
			<span style="font-size: large; font-weight: bold;"><a href="help.aspx#CustomLoadQuestions">Help</a> : Custom Load Questions</span>
			<hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>Custom load questions may be used on some load-out software to gather information	before and after it is loaded.</p>
				<table>
					<tr>
						<td style="width: 50%; border-right: 1px solid #404040;">
							<p><span class="helpItem">Bay:</span> the bay where the custom load question	should be asked.</p>
							<p><span class="helpItem">Pre/post load:</span> whether the question should	be asked before or after loading the order.</p>
							<p><span class="helpItem">Questions</span> </p>
							<table>
								<tr>
									<td><span class="helpItem">Add question:</span> creates a new question for	the selected bay and pre/post load selection.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Questions:</span> a list of all the questions for	the selected bay and pre/post load.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Remove question:</span> removes the question currently	selected in the questions list (to the left).</td>
								</tr>
								<tr>
									<td><span class="helpItem">Move up:</span> moves the question up on the list	(asked earlier).</td>
								</tr>
								<tr>
									<td><span class="helpItem">Move down:</span> moves the question down on the	list (asked later).</td>
								</tr>
							</table>
						</td>
						<td style="width: 50%;">
							<p><span class="helpItem">Question details</span> </p>
							<table>
								<tr>
									<td><span class="helpItem">Name:</span> the name of the question is required	and may be up to 50 characters in length.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Prompt text:</span> the wording that the user will	see when prompted with the question.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Post load:</span> determines if the question will	be asked before loading (not checked) or after loading (checked).</td>
								</tr>
								<tr>
									<td><span class="helpItem">Type:</span> the type of question.
										<ul>
											<li><i>Yes/no</i> will prompt the user with a question that can be answered with either a yes or a no.</li>
											<li><i>Text</i> will prompt the user to enter a value that may be alpha-numeric.</li>
											<li><i>List</i> will present the user with a list of options. </li>
											<li><i>Custom</i> will allow for the configuration of a URL to prompt for a custom question. </li>
											<li><i>Table lookup</i> will look up the distinct values of a field from the table specified.  If there is a deleted flag in the table, those records will not be included.</li>
										</ul>
									</td>
								</tr>
								<tr>
									<td><span class="helpItem">Owner:</span> the owner that this question will	be limited to.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Parameters:</span> if the question type is list,	parameters may be used to set the list items that the user may choose from. To add	a new item, enter the text for the option and click "Add parameter". To remove an	item, select it on the parameter list and click "Remove parameter". To move the	parameter up on the list, click on the move up button. To move the parameter lower	on the list, click on the move down button.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Disabled:</span> the question will not be prompted 	in the load out software.  Can be re-enabled to start showing question again.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Minimum Characters Required:</span> is for Text input type.  This is the minimum  number of characters required to be a valid input.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Maximum Characters Allowed:</span> is for Text input type.  This is the maximum  number of characters allowed to be a valid input.</td>
								</tr>
								<tr>
									<td><span class="helpItem">Allow Only Numeric Values:</span> is for Text input type.  This option will hide  the alpha portion of the keyboard (they will only get to select numbers for input).</td>
								</tr>
							</table>
						</td>
					</tr>
				</table>
			</div>
			<p><span class="helpItem">Save:</span> saves changes to the selected question.</p>
		</div>
	</form>
</body>
</html>
