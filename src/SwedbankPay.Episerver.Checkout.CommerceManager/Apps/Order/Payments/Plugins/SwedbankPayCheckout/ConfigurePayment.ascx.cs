using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.UI.WebControls;
using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using Mediachase.Commerce.Markets;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Web.Console.Interfaces;
using SwedbankPay.Episerver.Checkout.Common;

namespace SwedbankPay.Episerver.Checkout.CommerceManager.Apps.Order.Payments.Plugins.SwedbankPayCheckout
{
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

        protected void CountryList_CountryMoved(object sender, EventArgs e)
        {
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

            List<string> selectedRestrictedCountries = new List<string>();
            foreach (ListItem countryListRightItem in CountryList.RightItems)
            {
                selectedRestrictedCountries.Add(countryListRightItem.Value);
            }

            var configuration = new CheckoutConfiguration
            {
                MarketId = currentMarket,

                Token = txtToken.Text,
                ApiUrl = txtApiUrl.Text,
                MerchantId = txtMerchantId.Text,
                HostUrls = Enumerable.Select<string, string>(txtHostUrls.Text?.Split(';'), c => c.Trim()).ToList(),
                CompleteUrl = txtCompletetUrl.Text,
                CancelUrl = txtCancelUrl.Text,
                CallbackUrl = txtCallbackUrl.Text,
                TermsOfServiceUrl = txtTermsOfServiceUrl.Text,
                UseAnonymousCheckout = chkAnonymous.Checked,
                PaymentUrl = txtPaymentUrl.Text,
                ShippingAddressRestrictedToCountries = selectedRestrictedCountries
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
            txtPaymentUrl.Text = checkoutConfiguration.PaymentUrl;
            BindCountries(checkoutConfiguration.ShippingAddressRestrictedToCountries);
        }


        private void BindCountries(List<string> restrictedShippingCountries = null)
        {
            List<ListItem> listItemList1 = new List<ListItem>();
            List<ListItem> listItemList2 = new List<ListItem>();

            foreach (DataRow row in (InternalDataCollectionBase)CountryManager.GetCountries(true).Country.Rows)
            {
                bool flag = false;
                if (restrictedShippingCountries != null && restrictedShippingCountries.Count > 0)
                {
                    foreach (string country in restrictedShippingCountries)
                    {
                        if (country.Equals(row["Code"].ToString()))
                        {
                            flag = true;
                            break;
                        }
                    }
                }
                
                ListItem listItem = new ListItem(row["Name"].ToString(), row["Code"].ToString());

                if (flag)
                    listItemList2.Add(listItem);
                else
                    listItemList1.Add(listItem);
            }

            this.CountryList.LeftDataSource = (object)listItemList1;
            this.CountryList.RightDataSource = (object)listItemList2;
            this.CountryList.DataBind();
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