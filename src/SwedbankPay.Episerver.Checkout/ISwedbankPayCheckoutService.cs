using System;
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
        Consumer InitiateConsumerSession(string email = null, string mobilePhone = null, string ssn = null);
        ShippingDetails GetShippingDetails(Uri uri);

        PaymentOrder CreateOrUpdateOrder(IOrderGroup orderGroup, string userAgent, string consumerProfileRef = null);


        PaymentOrder CreateOrder(IOrderGroup orderGroup, string userAgent, string consumerProfileRef = null);
        PaymentOrder UpdateOrder(Uri orderId, IOrderGroup orderGroup, string userAgent);

        CheckoutConfiguration LoadCheckoutConfiguration(IMarket market);
        
        PaymentOrder GetOrder(Uri id, IMarket market, PaymentOrderExpand paymentOrderExpand = PaymentOrderExpand.None);
        
		void CancelOrder(IOrderGroup orderGroup);
        void Complete(IPurchaseOrder purchaseOrder);
    }
}
