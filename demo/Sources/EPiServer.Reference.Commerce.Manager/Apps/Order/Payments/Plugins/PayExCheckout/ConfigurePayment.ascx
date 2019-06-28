<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ConfigurePayment.ascx.cs" Inherits="PayEx.Checkout.Episerver.CommerceManager.Apps.Order.Payments.Plugins.PayExCheckout.ConfigurePayment" %>

<%@ Register TagPrefix="mc" Namespace="Mediachase.BusinessFoundation" Assembly="Mediachase.BusinessFoundation, Version=10.4.3.0, Culture=neutral, PublicKeyToken=41d2e7a615ba286c" %>

<asp:UpdatePanel UpdateMode="Conditional" ID="ConfigureUpdatePanelContentPanel" runat="server" RenderMode="Inline" ChildrenAsTriggers="true">
	<ContentTemplate>
		
		<style>
			.payexpayment-parameters table.DataForm tbody tr td.FormLabelCell {
				width: 200px;
			}

			.payexpayment-parameters h2 {
				margin-top: 20px
			}

			.payexpayment-parameters-url {
				width: 500px;
			}

			.payexpayment-list {
				list-style: disc;
				padding: 10px;
			}

			.payexpayment-table tr {
				vertical-align: top;
			}

			.pnl_warning {
				margin-top: 20px;
			}

			.payexpayment-warning {
				color: Red;
				background-color: #DFDFDF;
				font-weight: bold;
				padding: 6px;
				text-align: left;
			}
		</style>

		<div class="payexpayment-parameters">

			<div>
				<h2>Prerequisites</h2>
				<p>Before you can start integrating PayEx Checkout you need to have the following in place:</p>

				<ul class="payexpayment-list">
					<li>HTTPS enabled web server</li>
					<li>Agreement that includes PayEx Checkout</li>
					<li>Obtained credentials (merchant access token) from PayEx through PayEx Admin.</li>
				</ul>

				<p>If you're missing either of these, please contact <a href="mailto:support.ecom@payex.com">support.ecom@payex.com</a> for assistance.</p>
				<asp:Panel runat="server" ID="pnl_marketselected" Visible="False" CssClass="pnl_warning">
					<p class="payexpayment-warning">Before you can set parameters you have to set a market under the tab Markets and click OK </p>
				</asp:Panel>
			</div>

			<asp:Panel runat="server" ID="pnl_parameters">

				<h2>Market</h2>
				<table class="DataForm">
					<tbody>
						<tr>
							<td class="FormLabelCell">Select a market:</td>
							<td class="FormFieldCell">
								<asp:DropDownList runat="server" ID="marketDropDownList" OnSelectedIndexChanged="marketDropDownList_OnSelectedIndexChanged" AutoPostBack="True" />
							</td>
						</tr>
					</tbody>
				</table>

				<h2>Checkout/Checkin</h2>
				<table class="DataForm">
					<tbody>
						<tr>
							<td class="FormLabelCell">Use anonymous checkout:</td>
							<td class="FormFieldCell">
								<asp:CheckBox runat="server" ID="chkAnonymous" CssClass="payexpayment-parameters-url" />
							</td>
						</tr>
					</tbody>
				</table>

				<h2>Payex connection settings</h2>
				<table class="DataForm">
					<tbody>
						<tr>
							<td class="FormLabelCell">Token:</td>
							<td class="FormFieldCell">
								<asp:TextBox runat="server" ID="txtToken" CssClass="payexpayment-parameters-url" />
								<asp:RequiredFieldValidator ID="requiredUsername" runat="server" ControlToValidate="txtToken" ErrorMessage="Token is required." />
							</td>
						</tr>
						<tr>
							<td class="FormLabelCell">ApiUrl:</td>
							<td class="FormFieldCell">
								<asp:TextBox runat="server" ID="txtApiUrl" CssClass="payexpayment-parameters-url" />
								<asp:RequiredFieldValidator ID="requiredApiUrl" runat="server" ControlToValidate="txtApiUrl" ErrorMessage="Api URL is required." />
							</td>
						</tr>
						<tr>
							<td class="FormLabelCell">Merchant Id:</td>
							<td class="FormFieldCell">
								<asp:TextBox runat="server" ID="txtMerchantId" CssClass="payexpayment-parameters-url" />
								<asp:RequiredFieldValidator ID="requiredMerchantId" runat="server" ControlToValidate="txtMerchantId" ErrorMessage="Merchant Id is required." />
							</td>
						</tr>
					</tbody>
				</table>


				<h2>Merchant/callback URLs</h2>
				<table class="DataForm payexpayment-table">
					<tbody>
						<tr>
							<td class="FormLabelCell">
								<p>Host URLs: </p>
								<i>The list of URIs valid for embedding of PayEx Hosted Views.</i>
							</td>
							<td class="FormFieldCell">
								<asp:TextBox runat="server" ID="txtHostUrls" CssClass="payexpayment-parameters-url" />
								<asp:RequiredFieldValidator ID="requiredHostUrl" runat="server" ControlToValidate="txtHostUrls" ErrorMessage="Host URL is required." />
							</td>
						</tr>
						<tr>
							<td class="FormLabelCell">
								<p>CompleteUrl URL: </p>
								<i>The URI to redirect the payer to once the payment is completed. {orderGroupId} in the url will be replaced with Carts OrderGroupId</i>
							</td>
							<td class="FormFieldCell">
								<asp:TextBox runat="server" ID="txtCompletetUrl" CssClass="payexpayment-parameters-url" />
								<asp:RequiredFieldValidator ID="requiredCheckoutUrl" runat="server" ControlToValidate="txtCompletetUrl" ErrorMessage="CompleteUrl URL is required." />
							</td>
						</tr>
						<tr>
							<td class="FormLabelCell">
								<p>Cancel URL: </p>
								<i>The URI that PayEx will redirect back to when the user presses the cancel button in the payment page. {orderGroupId} in the url will be replaced with Carts OrderGroupId</i>
							</td>
							<td class="FormFieldCell">
								<asp:TextBox runat="server" ID="txtCancelUrl" CssClass="payexpayment-parameters-url" />
								<asp:RequiredFieldValidator ID="requiredCancelUrl" runat="server" ControlToValidate="txtCancelUrl" ErrorMessage="Cancel URL is required." />
							</td>
						</tr>
						<tr>
							<td class="FormLabelCell">
								<p>Callback URL: </p>
								<i>The URI to the API endpoint receiving POST requests on transaction activity related to the payment order. {orderGroupId} in the url will be replaced with Carts OrderGroupId</i>
							</td>
							<td class="FormFieldCell">
								<asp:TextBox runat="server" ID="txtCallbackUrl" CssClass="payexpayment-parameters-url" />
								<asp:RequiredFieldValidator ID="requiredCallbackUrl" runat="server" ControlToValidate="txtCallbackUrl" ErrorMessage="Callback URL is required." />
							</td>
						</tr>
						<tr>
							<td class="FormLabelCell">
								<p>Terms of Service URL: </p>
								<i>A URI that contains your terms and conditions for the payment, to be linked on the payment page. Require https.</i>
							</td>
							<td class="FormFieldCell">
								<asp:TextBox runat="server" ID="txtTermsOfServiceUrl" CssClass="payexpayment-parameters-url" />
								<asp:RequiredFieldValidator ID="requiredTermsOfServiceUrl" runat="server" ControlToValidate="txtTermsOfServiceUrl" ErrorMessage="Terms of Service URL is required." />
							</td>
						</tr>
					</tbody>
				</table>
			</asp:Panel>
		</div>
	</ContentTemplate>
</asp:UpdatePanel>
