namespace PayEx.Checkout.Episerver.OrderManagement
{
    using EPiServer.Commerce.Order;
    using Mediachase.Commerce;
    using Mediachase.Commerce.Orders.Dto;
    using Mediachase.Commerce.Orders.Managers;
    using PayEx.Checkout.Episerver.Common;
    using PayEx.Checkout.Episerver.Common.Extensions;
    using PayEx.Net.Api;

    /// <summary>
    /// Factory methods to create an instance of IPayExOrderService
    /// Initializes it for a specific payment method and a specific market (since the API settings might vary)
    /// </summary>
    public class PayExOrderServiceFactory
    {
        public virtual IPayExOrderService Create(IPayment payment, IMarket market)
        {
            return Create(PaymentManager.GetPaymentMethod(payment.PaymentMethodId), market.MarketId);
        }

        public virtual IPayExOrderService Create(PaymentMethodDto paymentMethodDto, MarketId marketMarketId)
        {
            return Create(paymentMethodDto.GetConnectionConfiguration(marketMarketId));
        }

        public virtual IPayExOrderService Create(ConnectionConfiguration connectionConfiguration)
        {
            var payExApi = new PayExApi(connectionConfiguration.ApiUrl, connectionConfiguration.Token); 
            return new PayExOrderService(payExApi, connectionConfiguration);
        }
    }
}