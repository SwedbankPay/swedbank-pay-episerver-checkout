using EPiServer.Commerce.Order;
using Mediachase.Commerce;
using SwedbankPay.Episerver.Checkout.Common;
using SwedbankPay.Sdk;
using SwedbankPay.Sdk.Consumers;
using SwedbankPay.Sdk.PaymentOrders;

namespace SwedbankPay.Episerver.Checkout
{
    public interface ISwedbankPayCheckoutService
    {
        ConsumersResponse InitiateConsumerSession(string email = null, string mobilePhone = null, string ssn = null);
        ShippingDetails GetShippingDetails(string uri);

        PaymentOrderResponseContainer CreateOrUpdateOrder(IOrderGroup orderGroup, string userAgent, string consumerProfileRef = null);


        PaymentOrderResponseContainer CreateOrder(IOrderGroup orderGroup, string userAgent, string consumerProfileRef = null);
        PaymentOrderResponseContainer UpdateOrder(string orderId, IOrderGroup orderGroup, string userAgent);

        CheckoutConfiguration LoadCheckoutConfiguration(IMarket market);

        SwedbankPayOptions GetConfiguration(IMarket market);

        PaymentOrderResponseContainer GetOrder(string id, IMarket market, PaymentOrderExpand paymentOrderExpand = PaymentOrderExpand.None);

        //Task<PaymentResponse> GetPayment(string id, IOrderGroup orderGroup, PaymentExpand paymentExpand = PaymentExpand.None);


		void CancelOrder(IOrderGroup orderGroup);
        void Complete(IPurchaseOrder purchaseOrder);
    }
}
