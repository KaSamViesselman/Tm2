<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="TransportInspectionQuestionsHelp.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.TransportInspectionQuestionsHelp" EnableViewState="false" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
	<title>Help : Transport Inspection Questions</title>
	<link href="helpstyle.css" type="text/css" rel="stylesheet" />
</head>
<body>
	<form id="TransportInspectionQuestionsHelp" runat="server">
		<div>
			<span style="font-size: large; font-weight: bold;"><a href="help.aspx#TransportInspectionQuestions">Help</a> : Transport Inspection Questions</span><hr style="width: 100%; color: #003399;" />
			<div id="divHelp">
				<p>
					Inspection questions may be used on some load-out software to inspect a transport before and after it is loaded.
				</p>
				<p>
					<span class="helpItem">Bay:</span> the bay where the inspection question should be asked.
				</p>
				<p>
					<span class="helpItem">Pre/post inspection:</span> whether the question should be asked before or after loading the transport.
				</p>
				<p>
					<span class="helpItem">Transport type:</span> the type of transport that this question relates to.
				</p>
				<p>
					<span class="helpItem">Questions</span>
				</p>
				<table>
					<tr>
						<td>
							<span class="helpItem">Add question:</span> creates a new question for the selected bay and pre/post inspection selection.
						</td>
					</tr>
					<tr>
						<td>
							<span class="helpItem">Questions:</span> a list of all the questions for the selected bay, pre/post inspection and transport type.
						</td>
					</tr>
					<tr>
						<td>
							<span class="helpItem">Remove question:</span> removes the question currently selected in the questions list (to the left).
						</td>
					</tr>
					<tr>
						<td>
							<span class="helpItem">Move up:</span> moves the question up on the list (asked earlier).
						</td>
					</tr>
					<tr>
						<td>
							<span class="helpItem">Move down:</span> moves the question down on the list (asked later).
						</td>
					</tr>
				</table>
				<p>
					<span class="helpItem">Question details</span>
				</p>
				<table>
					<tr>
						<td>
							<span class="helpItem">Name:</span> the name of the question is required and may be up to 50 characters in length.
						</td>
					</tr>
					<tr>
						<td>
							<span class="helpItem">Prompt text:</span> the wording that the user will see when prompted with the question.
						</td>
					</tr>
					<tr>
						<td>
							<span class="helpItem">Post load:</span> determines if the question will be asked before loading (not checked) or after loading (checked).
						</td>
					</tr>
					<tr>
						<td>
							<span class="helpItem">Type:</span> the type of question. 
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
						<td>
							<span class="helpItem">Transport type:</span> the type of transport this question applies to.
						</td>
					</tr>
					<tr>
						<td>
							<span class="helpItem">Owner:</span> the owner that this question will be limited to.
						</td>
					</tr>
					<tr>
						<td>
							<span class="helpItem">URL:</span> used for specifying the web address used for a Custom Transport Inspection question. This web page can be used for more advanced processing of inspection questions, and validation of data. Each Custom Transport Inspection Question is developed specifically for each facility per the facilities specifications.
						</td>
					</tr>
					<tr>
						<td>
							<span class="helpItem">Parameters:</span> if the question type is list, parameters may be used to set the list items that the user may choose from. To add a new item, enter the text for the option and click "Add parameter". To remove an item, select it on the parameter list and click "Remove parameter". To move the parameter up on the list, click on the move up button. To move the parameter lower on the list, click on the move down button.
						</td>
					</tr>
					<tr>
						<td>
							<span class="helpItem">Disabled:</span> the question will not be prompted in the load out software. Can be re-enabled to start showing question again.
						</td>
					</tr>
				</table>
			</div>
			<p>
				<span class="helpItem">Save:</span> saves changes to the selected question.
			</p>
		</div>
	</form>
</body>
</html>
