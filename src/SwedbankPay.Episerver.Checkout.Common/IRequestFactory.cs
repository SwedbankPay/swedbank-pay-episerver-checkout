using EPiServer.Commerce.Order;

using Mediachase.Commerce;
using Mediachase.Commerce.Orders.Dto;

using SwedbankPay.Sdk;
using SwedbankPay.Sdk.Consumers;
using SwedbankPay.Sdk.PaymentOrders;

using System.Collections.Generic;

namespace SwedbankPay.Episerver.Checkout.Common
{
	public interface IRequestFactory
    {
	    PaymentOrderRequest GetPaymentOrderRequest(IOrderGroup orderGroup, IMarket market, PaymentMethodDto paymentMethodDto, string description, string consumerProfileRef = null);
        ConsumerRequest GetConsumerResourceRequest(Language language, IList<CountryCode> shippingAddressRestrictedToCountryCodes, EmailAddress email = null, Msisdn msisdn = null, NationalIdentifier nationalIdentifier = null);
        PaymentOrderAbortRequest GetAbortRequest(string abortReason);
        PaymentOrderCancelRequest GetCancelRequest(string description = "Cancelling purchase order.");
        PaymentOrderCaptureRequest GetCaptureRequest(IPayment payment, IMarket market, IShipment shipment, bool addShipmentInOrderItem = true, string description = "Capturing payment.");
        PaymentOrderReversalRequest GetReversalRequest(IPayment payment, IEnumerable<ILineItem> lineItems, IMarket market, IShipment shipment, string description = "Reversing payment.");
        PaymentOrderUpdateRequest GetUpdateRequest(IOrderGroup orderGroup, IMarket market);
    }
}