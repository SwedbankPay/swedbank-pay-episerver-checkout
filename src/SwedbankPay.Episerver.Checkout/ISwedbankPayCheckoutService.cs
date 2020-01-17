using EPiServer.Commerce.Order;

using Mediachase.Commerce;

using SwedbankPay.Episerver.Checkout.Common;
using SwedbankPay.Sdk.Consumers;
using SwedbankPay.Sdk.PaymentOrders;

using System;

namespace SwedbankPay.Episerver.Checkout
{
    public interface ISwedbankPayCheckoutService
    {
        Consumer InitiateConsumerSession(string email = null, string mobilePhone = null, string ssn = null);
        PaymentOrder CreateOrUpdatePaymentOrder(IOrderGroup orderGroup, string description, string consumerProfileRef = null);
        CheckoutConfiguration LoadCheckoutConfiguration(IMarket market);
        PaymentOrder GetPaymentOrder(Uri id, IMarket market, PaymentOrderExpand paymentOrderExpand = PaymentOrderExpand.None);
        void CancelOrder(IOrderGroup orderGroup);
        void Complete(IPurchaseOrder purchaseOrder);
    }
}
