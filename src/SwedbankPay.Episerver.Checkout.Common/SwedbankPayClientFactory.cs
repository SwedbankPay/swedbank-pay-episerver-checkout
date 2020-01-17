using System;
using EPiServer.ServiceLocation;

using Mediachase.Commerce;
using Mediachase.Commerce.Orders.Dto;

using Microsoft.Extensions.Logging;

using SwedbankPay.Episerver.Checkout.Common.Extensions;
using SwedbankPay.Sdk;

using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;

namespace SwedbankPay.Episerver.Checkout.Common
{
    /// <summary>
    /// Factory methods to create an instance of SwedbankPayClient
    /// Initializes it for a specific payment method and a specific market (since the API settings might vary)
    /// </summary>
    [ServiceConfiguration(typeof(ISwedbankPayClientFactory))]
    public class SwedbankPayClientFactory : ISwedbankPayClientFactory
    {
        protected static readonly ConcurrentDictionary<string, HttpClient> HttpClientCache = new ConcurrentDictionary<string, HttpClient>();

        
        private readonly ICheckoutConfigurationLoader _checkoutConfigurationLoader;
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;

        public SwedbankPayClientFactory(ICheckoutConfigurationLoader checkoutConfigurationLoader)
        {
            _checkoutConfigurationLoader = checkoutConfigurationLoader;
        }

        public virtual ISwedbankPayClient Create(IMarket market, ILogger logger = null)
        {
            CheckoutConfiguration checkoutConfiguration = _checkoutConfigurationLoader.GetConfiguration(market.MarketId, market.DefaultLanguage.Name);
            
            var key = $"{checkoutConfiguration.ApiUrl}:{checkoutConfiguration.MerchantId}:{checkoutConfiguration.Token}";

            HttpClientHandler handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            var httpClient = HttpClientCache.GetOrAdd(key, k =>
            {
                var client = new HttpClient(handler)
                {
                    BaseAddress = checkoutConfiguration.ApiUrl,
                    DefaultRequestHeaders = { { "Authorization", $"Bearer {checkoutConfiguration.Token}" } },
                    Timeout = TimeSpan.FromMinutes(10)
                    /* Other setup */
                };
                var sp = ServicePointManager.FindServicePoint(checkoutConfiguration.ApiUrl);
                sp.ConnectionLeaseTimeout = 60 * 1000; // 1 minute
                return client;
            });

            return new SwedbankPayClient(httpClient, null);
        }

        //public virtual ISwedbankPayClient Create(IPayment payment, IMarket market)
        //{
        //    return Create(PaymentManager.GetPaymentMethod(payment.PaymentMethodId), market.MarketId);
        //}

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