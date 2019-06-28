namespace PayEx.Checkout.Episerver
{
    using EPiServer.Commerce.Order;
    using Mediachase.Commerce;
    using PayEx.Checkout.Episerver.Common;
    using PayEx.Net.Api.Models;

    public interface IPayExCheckoutService
    {
        InitiateConsumerSessionResponseObject InitiateConsumerSession(string email = null, string mobilePhone = null, string ssn = null);
        ShippingDetails GetShippingDetails(string uri);

        PaymentOrderResponseObject CreateOrUpdateOrder(IOrderGroup orderGroup, string userAgent, string consumerProfileRef = null);


        PaymentOrderResponseObject CreateOrder(IOrderGroup orderGroup, string userAgent, string consumerProfileRef = null);
        PaymentOrderResponseObject UpdateOrder(string orderId, IOrderGroup orderGroup, string userAgent);


        CheckoutConfiguration GetConfiguration(IMarket market);

        PaymentOrderResponseObject GetOrder(string id, IMarket market, PaymentOrderExpand paymentOrderExpand = PaymentOrderExpand.None);

        PaymentResponse GetPayment(string id, IOrderGroup orderGroup, PaymentExpand paymentExpand = PaymentExpand.None);


		void CancelOrder(IOrderGroup orderGroup);
        void Complete(IPurchaseOrder purchaseOrder);
    }
}
