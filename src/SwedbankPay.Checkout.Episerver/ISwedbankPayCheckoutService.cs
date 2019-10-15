using EPiServer.Commerce.Order;
using Mediachase.Commerce;
using SwedbankPay.Checkout.Episerver.Common;
using SwedbankPay.Client;
using SwedbankPay.Client.Models;
using SwedbankPay.Client.Models.Response;

namespace SwedbankPay.Checkout.Episerver
{
    public interface ISwedbankPayCheckoutService
    {
        ConsumerResourceResponse InitiateConsumerSession(string email = null, string mobilePhone = null, string ssn = null);
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
