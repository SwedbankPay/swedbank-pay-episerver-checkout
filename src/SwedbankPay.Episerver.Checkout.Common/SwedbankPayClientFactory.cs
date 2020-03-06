using EPiServer.ServiceLocation;

using Mediachase.Commerce;
using Mediachase.Commerce.Orders.Dto;

using SwedbankPay.Episerver.Checkout.Common.Extensions;
using SwedbankPay.Sdk;

using System;
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
        
        public SwedbankPayClientFactory(ICheckoutConfigurationLoader checkoutConfigurationLoader)
        {
            _checkoutConfigurationLoader = checkoutConfigurationLoader;
        }

        public virtual ISwedbankPayClient Create(IMarket market)
        {
            CheckoutConfiguration checkoutConfiguration = _checkoutConfigurationLoader.GetConfiguration(market.MarketId);
            return GetSwedbankPayClient(checkoutConfiguration);
        }

        public virtual ISwedbankPayClient Create(PaymentMethodDto paymentMethodDto, MarketId marketMarketId)
        {
            return Create(_checkoutConfigurationLoader.GetConfiguration(paymentMethodDto, marketMarketId));
        }

        public virtual ISwedbankPayClient Create(ConnectionConfiguration connectionConfiguration)
        {
            return GetSwedbankPayClient(connectionConfiguration);
        }

        private static ISwedbankPayClient GetSwedbankPayClient(ConnectionConfiguration connectionConfiguration)
        {
            var key = $"{connectionConfiguration.ApiUrl}:{connectionConfiguration.MerchantId}:{connectionConfiguration.Token}";

            HttpClientHandler handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            var httpClient = HttpClientCache.GetOrAdd(key, k =>
            {
                var client = new HttpClient(handler)
                {
                    BaseAddress = connectionConfiguration.ApiUrl,
                    DefaultRequestHeaders = { { "Authorization", $"Bearer {connectionConfiguration.Token}" } },
                    Timeout = TimeSpan.FromMinutes(10)
                };

                var sp = ServicePointManager.FindServicePoint(connectionConfiguration.ApiUrl);
                sp.ConnectionLeaseTimeout = 60 * 1000; // 1 minute
                return client;
            });

            return new SwedbankPayClient(httpClient);
        }
    }
}