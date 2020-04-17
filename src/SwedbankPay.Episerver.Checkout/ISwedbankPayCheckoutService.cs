using EPiServer.Commerce.Order;

using Mediachase.Commerce;

using SwedbankPay.Episerver.Checkout.Common;
using SwedbankPay.Sdk.Consumers;
using SwedbankPay.Sdk.PaymentOrders;

using System;
using System.Globalization;

namespace SwedbankPay.Episerver.Checkout
{
    public interface ISwedbankPayCheckoutService : ISwedbankPayService
    {
        Consumer InitiateConsumerSession(CultureInfo currentLanguage, string email = null, string mobilePhone = null, string ssn = null);
        PaymentOrder CreateOrUpdatePaymentOrder(IOrderGroup orderGroup, string description, string consumerProfileRef = null);
        CheckoutConfiguration LoadCheckoutConfiguration(IMarket market, string languageId);

        PaymentOrder GetPaymentOrder(IOrderGroup orderGroup, PaymentOrderExpand paymentOrderExpand = PaymentOrderExpand.None);
        PaymentOrder GetPaymentOrder(Uri id, IMarket market, string languageId, PaymentOrderExpand paymentOrderExpand = PaymentOrderExpand.None);
        void CancelOrder(IOrderGroup orderGroup);
        void Complete(IPurchaseOrder purchaseOrder);
    }
}
