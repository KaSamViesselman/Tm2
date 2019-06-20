<%@ Page Language="vb" CodeBehind="Template.aspx.vb" Inherits="KahlerAutomation.TerminalManagement2.Template" %>
<!DOCTYPE html>
<html lang="en">
	<head>
		<title>Containers : Containers</title>
		<meta name="viewport" content="width=device-width" />
		<meta http-equiv="X-UA-Compatible" content="IE=edge" />
		<link rel="stylesheet" href="styles/site.css" />
		<link rel="stylesheet" href="styles/site-tablet.css" media="(max-width:768px)" />
		<link rel="stylesheet" href="styles/site-phone.css" media="(max-width:480px)" />
		<script type="text/javascript"  src="scripts/jquery-1.11.1.min.js"></script>
		<script type="text/javascript"  src="scripts/soap-1.0.1.js"></script>
		<script type="text/javascript"  src="scripts/page-controller.js"></script>
	</head>
	<body>
		<form id="main" runat="server">
			<div class="recordSelection">
				<label>Record</label>
				<select>
					<option>Record 1</option>
					<option>Record 2</option>
				</select>
			</div>
			<div class="sectionEven">
				<h1>Section 1</h1>
				<ul>
					<li>
						<label>Parameter 1</label>
						<input type="text" class="required" />
					</li>
					<li>
						<label>Parameter 2</label>
						<input type="text" />
					</li>
					<li>
						<label>Parameter 3</label>
						<select>
							<option>Option 1</option>
							<option>Option 2</option>
						</select>
					</li>
					<li>
						<label>Parameter 4</label>
						<input type="checkbox" />
					</li>
				</ul>
			</div>
            <div class="sectionOdd">
			    <div class="sectionEven">
				    <h1>Section 2</h1>
				    Once the driver is ready to load, the system interfaces to the flow meter or scale, and controls the valves and pumps to accurately and precisely deliver product to the truck/transport. As product is delivered to the truck/transport, the driver may watch the progress on touch screen. When the system is done delivering product to the truck/transport, the system will print a receipt. The system may be equipped with a digital signature pad to capture the driver’s signature on the receipt. The system may also be equipped with sensors on the fill arm and other equipment, to ensure that it is in the proper position before loading, and also ensuring that when loading is complete, the equipment is put back in the proper position so that it is not damaged.
			    </div>
			    <div class="sectionOdd">
				    <h1>Section 3</h1>
				    An optional gate and door access control system may be used to prevent unauthorized access to the site and load-out facility. The gate and door access control system may be configured to prompt the driver for, and verify driver number (or iButton key) and PIN (if used), order number, carrier number and truck/transport number. An optional truck scale pre-staging system may be used to measure truck tare weight and calculate the maximum amount of product that may be loaded into the truck, based on a maximum truck weight.
			    </div>
            </div>
			<div class="section">
				<h1>Section 4</h1>
				An optional gate and door access control system may be used to prevent unauthorized access to the site and load-out facility. The gate and door access control system may be configured to prompt the driver for, and verify driver number (or iButton key) and PIN (if used), order number, carrier number and truck/transport number. An optional truck scale pre-staging system may be used to measure truck tare weight and calculate the maximum amount of product that may be loaded into the truck, based on a maximum truck weight.
			</div>
            <div class="section">
                <h1>Section 5</h1>
                Here's a lovely table:
                <table>
                    <tr>
                        <th>Name</th>
                        <th>Street</th>
                        <th>City</th>
                        <th>State</th>
                        <th>Zip code</th>
                    </tr>
                    <tr>
                        <td>Jane Doe</td>
                        <td>809 Timberlake Rd</td>
                        <td>Fairmont</td>
                        <td>MN</td>
                        <td>56031</td>
                    </tr>
                    <tr>
                        <td>Al Doe</td>
                        <td>810 Timberlake Rd</td>
                        <td>Fairmont</td>
                        <td>MN</td>
                        <td>56031</td>
                    </tr>
                    <tr>
                        <td>Steve Doe</td>
                        <td>811 Timberlake Rd</td>
                        <td>Fairmont</td>
                        <td>MN</td>
                        <td>56031</td>
                    </tr>
                    <tr>
                        <td>Bruce Doe</td>
                        <td>812 Timberlake Rd</td>
                        <td>Fairmont</td>
                        <td>MN</td>
                        <td>56031</td>
                    </tr>
                    <tr>
                        <td>Chris Doe</td>
                        <td>813 Timberlake Rd</td>
                        <td>Fairmont</td>
                        <td>MN</td>
                        <td>56031</td>
                    </tr>
                </table>
            </div>
            <div class="section">
                <h1>Section 6</h1>
				<p>Here are some buttons:
				<a href="#" class="button">+</a>
				<a href="#" class="button">-</a>
				<a href="#" class="button">l</a>
				<a href="#" class="button">r</a>
				<a href="#" class="button">u</a>
				<a href="#" class="button">d</a>
				<a href="#" class="button">x</a></p>
                <p>Here is a collapsing section:</p>
                <h2>Collapsing section</h2>
				<div class="collapsingSection not">
					An optional gate and door access control system may be used to prevent unauthorized access to the site and load-out facility. The gate and door access control system may be configured to prompt the driver for, and verify driver number (or iButton key) and PIN (if used), order number, carrier number and truck/transport number. An optional truck scale pre-staging system may be used to measure truck tare weight and calculate the maximum amount of product that may be loaded into the truck, based on a maximum truck weight.
				</div>
            </div>
		</form>
	</body>
</html>
