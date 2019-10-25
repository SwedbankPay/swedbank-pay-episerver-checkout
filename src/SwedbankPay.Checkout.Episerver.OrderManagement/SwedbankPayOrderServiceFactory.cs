using System;
using EPiServer.Commerce.Order;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Commerce.Orders.Managers;
using SwedbankPay.Checkout.Episerver.Common;
using SwedbankPay.Checkout.Episerver.Common.Extensions;
using SwedbankPay.Sdk;

namespace SwedbankPay.Checkout.Episerver.OrderManagement
{
    /// <summary>
    /// Factory methods to create an instance of SwedbankPayOrderService
    /// Initializes it for a specific payment method and a specific market (since the API settings might vary)
    /// </summary>
    public class SwedbankPayOrderServiceFactory
    {
        private readonly ICheckoutConfigurationLoader _checkoutConfigurationLoader;

        public SwedbankPayOrderServiceFactory(ICheckoutConfigurationLoader checkoutConfigurationLoader)
        {
            _checkoutConfigurationLoader = checkoutConfigurationLoader;
        }

        public virtual ISwedbankPayOrderService Create(IMarket market)
        {
            CheckoutConfiguration checkoutConfiguration = _checkoutConfigurationLoader.GetConfiguration(market.MarketId, market.DefaultLanguage.Name);
            var swedbankPayOptions = new SwedbankPayOptions
            {
                Token = checkoutConfiguration.Token,
                MerchantId = checkoutConfiguration.MerchantId,
                ApiBaseUrl = new Uri(checkoutConfiguration.ApiUrl)
            };
            var swedbankPayClient = new SwedbankPayClient(swedbankPayOptions);
            return new SwedbankPayOrderService(swedbankPayClient);
        }

        public virtual ISwedbankPayOrderService Create(IPayment payment, IMarket market)
        {
            return Create(PaymentManager.GetPaymentMethod(payment.PaymentMethodId), market.MarketId);
        }

        public virtual ISwedbankPayOrderService Create(PaymentMethodDto paymentMethodDto, MarketId marketMarketId)
        {
            return Create(paymentMethodDto.GetConnectionConfiguration(marketMarketId));
        }

        public virtual ISwedbankPayOrderService Create(ConnectionConfiguration connectionConfiguration)
        {
            var swedbankPayOptions = new SwedbankPayOptions();
            var swedbankPayClient = new SwedbankPayClient(swedbankPayOptions);  //TODO swed
            return new SwedbankPayOrderService(swedbankPayClient);
        }
    }
}