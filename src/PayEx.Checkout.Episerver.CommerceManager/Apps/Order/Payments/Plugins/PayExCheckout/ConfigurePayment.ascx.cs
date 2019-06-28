namespace PayEx.Checkout.Episerver.CommerceManager.Apps.Order.Payments.Plugins.PayExCheckout
{
    using EPiServer.ServiceLocation;
    using Mediachase.Commerce;
    using Mediachase.Commerce.Markets;
    using Mediachase.Commerce.Orders.Dto;
    using Mediachase.Web.Console.Interfaces;
    using PayEx.Checkout.Episerver.Common;
    using System;
    using System.Linq;

    public partial class ConfigurePayment : System.Web.UI.UserControl, IGatewayControl
    {
        private IMarketService _marketService;
        private ICheckoutConfigurationLoader _checkoutConfigurationLoader;
        private PaymentMethodDto _paymentMethodDto;

        protected void Page_Load(object sender, EventArgs e)
        {
            _marketService = ServiceLocator.Current.GetInstance<IMarketService>();
            _checkoutConfigurationLoader = ServiceLocator.Current.GetInstance<ICheckoutConfigurationLoader>();

            if (IsPostBack || _paymentMethodDto?.PaymentMethodParameter == null) return;

            var markets = _paymentMethodDto.PaymentMethod.First().GetMarketPaymentMethodsRows();
            if (markets == null || markets.Length == 0)
            {
                pnl_marketselected.Visible = true;
                pnl_parameters.Visible = false;
                return;
            }

            

            var market = _marketService.GetMarket(markets.First().MarketId);
            var checkoutConfiguration = GetConfiguration(market.MarketId, market.DefaultLanguage.Name);
            BindConfigurationData(checkoutConfiguration);
            BindMarketData(markets);
        }


        protected void marketDropDownList_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            var market = _marketService.GetMarket(new MarketId(marketDropDownList.SelectedValue));
            var checkoutConfiguration = GetConfiguration(new MarketId(marketDropDownList.SelectedValue), market.DefaultLanguage.Name);
            BindConfigurationData(checkoutConfiguration);
            ConfigureUpdatePanelContentPanel.Update();
        }



        public void SaveChanges(object dto)
        {
            if (!Visible)
            {
                return;
            }

            var paymentMethod = dto as PaymentMethodDto;
            if (paymentMethod == null)
            {
                return;
            }
            var currentMarket = marketDropDownList.SelectedValue;

            var configuration = new CheckoutConfiguration
            {
                MarketId = currentMarket,

                Token = txtToken.Text,
                ApiUrl = txtApiUrl.Text,
                MerchantId = txtMerchantId.Text,
                HostUrls = txtHostUrls.Text?.Split(';').Select(c => c.Trim()).ToList(),
                CompleteUrl = txtCompletetUrl.Text,
                CancelUrl = txtCancelUrl.Text,
                CallbackUrl = txtCallbackUrl.Text,
                TermsOfServiceUrl = txtTermsOfServiceUrl.Text,
                UseAnonymousCheckout = chkAnonymous.Checked
            };

            _checkoutConfigurationLoader.SetConfiguration(configuration, paymentMethod, currentMarket);
        }

        public void LoadObject(object dto)
        {
            var paymentMethod = dto as PaymentMethodDto;
            if (paymentMethod == null)
            {
                return;
            }
            _paymentMethodDto = paymentMethod;
        }

        public string ValidationGroup { get; set; }

        private void BindMarketData(PaymentMethodDto.MarketPaymentMethodsRow[] markets)
        {
            marketDropDownList.DataSource = markets.Select(m => m.MarketId);
            marketDropDownList.DataBind();
        }

        public void BindConfigurationData(CheckoutConfiguration checkoutConfiguration)
        {

            txtToken.Text = checkoutConfiguration.Token;
            txtApiUrl.Text = checkoutConfiguration.ApiUrl;
            txtMerchantId.Text = checkoutConfiguration.MerchantId;

            txtHostUrls.Text = checkoutConfiguration.HostUrls != null && checkoutConfiguration.HostUrls.Any() ? string.Join("; ", checkoutConfiguration.HostUrls) : null;
            txtCompletetUrl.Text = checkoutConfiguration.CompleteUrl;
            txtCancelUrl.Text = checkoutConfiguration.CancelUrl;
            txtCallbackUrl.Text = checkoutConfiguration.CallbackUrl;
            txtTermsOfServiceUrl.Text = checkoutConfiguration.TermsOfServiceUrl;
            chkAnonymous.Checked = checkoutConfiguration.UseAnonymousCheckout;
        }

        private CheckoutConfiguration GetConfiguration(MarketId marketId, string languageId)
        {
            try
            {
                return _checkoutConfigurationLoader.GetConfiguration(marketId, languageId);
            }
            catch
            {
                return new CheckoutConfiguration();
            }
        }
    }
}