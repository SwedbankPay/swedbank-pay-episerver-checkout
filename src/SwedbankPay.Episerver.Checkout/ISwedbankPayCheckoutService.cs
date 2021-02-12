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
		IConsumersResponse InitiateConsumerSession(CultureInfo currentLanguage, string email = null, string mobilePhone = null, string ssn = null);
		IPaymentOrderResponse CreateOrUpdatePaymentOrder(IOrderGroup orderGroup, string description, string consumerProfileRef = null);
		CheckoutConfiguration LoadCheckoutConfiguration(IMarket market, string languageId);

		IPaymentOrderResponse GetPaymentOrder(IOrderGroup orderGroup, PaymentOrderExpand paymentOrderExpand = PaymentOrderExpand.None);
		IPaymentOrderResponse GetPaymentOrder(Uri id, IMarket market, string languageId, PaymentOrderExpand paymentOrderExpand = PaymentOrderExpand.None);
		void CancelOrder(IOrderGroup orderGroup);
		void Complete(IPurchaseOrder purchaseOrder);
	}
}
