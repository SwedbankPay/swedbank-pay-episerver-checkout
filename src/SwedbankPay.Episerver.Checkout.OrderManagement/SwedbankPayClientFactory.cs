using System;
using System.Net.Http;
using EPiServer.Commerce.Order;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Commerce.Orders.Managers;
using Microsoft.Extensions.Http;
using SwedbankPay.Episerver.Checkout.Common;
using SwedbankPay.Episerver.Checkout.Common.Extensions;
using SwedbankPay.Sdk;

namespace SwedbankPay.Episerver.Checkout.OrderManagement
{
    /// <summary>
    /// Factory methods to create an instance of SwedbankPayClient
    /// Initializes it for a specific payment method and a specific market (since the API settings might vary)
    /// </summary>
    public class SwedbankPayClientFactory
    {
        private readonly ICheckoutConfigurationLoader _checkoutConfigurationLoader;
        private readonly HttpClient _httpClient;
        
        public SwedbankPayClientFactory(ICheckoutConfigurationLoader checkoutConfigurationLoader, IHttpClientFactory httpClientFactory)
        {
            _checkoutConfigurationLoader = checkoutConfigurationLoader;
            _httpClient = httpClientFactory.CreateClient();
        }

        public virtual ISwedbankPayClient Create(IMarket market)
        {
            CheckoutConfiguration checkoutConfiguration = _checkoutConfigurationLoader.GetConfiguration(market.MarketId, market.DefaultLanguage.Name);
            _httpClient.BaseAddress = checkoutConfiguration.ApiUrl;
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {checkoutConfiguration.Token}");

            return new SwedbankPayClient(_httpClient);
        }

        public virtual ISwedbankPayClient Create(IPayment payment, IMarket market)
        {
            return Create(PaymentManager.GetPaymentMethod(payment.PaymentMethodId), market.MarketId);
        }

        public virtual ISwedbankPayClient Create(PaymentMethodDto paymentMethodDto, MarketId marketMarketId)
        {
            return Create(paymentMethodDto.GetConnectionConfiguration(marketMarketId));
        }

        public virtual ISwedbankPayClient Create(ConnectionConfiguration connectionConfiguration)
        {
            _httpClient.BaseAddress = connectionConfiguration.ApiUrl;
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {connectionConfiguration.Token}");

            return new SwedbankPayClient(_httpClient);
        }
    }
}