using EPiServer.Commerce.Order;

using Mediachase.Commerce;
using Mediachase.Commerce.Orders.Dto;

using SwedbankPay.Sdk;
using SwedbankPay.Sdk.Consumers;
using SwedbankPay.Sdk.PaymentOrders;

using System.Collections.Generic;
using System.Globalization;

namespace SwedbankPay.Episerver.Checkout.Common
{
    public interface IRequestFactory
    {
        PaymentOrderRequest GetPaymentOrderRequest(IOrderGroup orderGroup, IMarket market, PaymentMethodDto paymentMethodDto, string description, string consumerProfileRef = null);
        ConsumersRequest GetConsumerResourceRequest(Language language, IEnumerable<RegionInfo> shippingAddressRestrictedToCountryCodes, EmailAddress email = null, Msisdn msisdn = null, NationalIdentifier nationalIdentifier = null);
        PaymentOrderAbortRequest GetAbortRequest();
        PaymentOrderCancelRequest GetCancelRequest(string description = "Cancelling purchase order.");
        PaymentOrderCaptureRequest GetCaptureRequest(IPayment payment, IMarket market, IShipment shipment, bool addShipmentInOrderItem = true, string description = "Capturing payment.");
        PaymentOrderReversalRequest GetReversalRequest(IEnumerable<ILineItem> lineItems, IMarket market, IShipment shipment, bool addShipmentInOrderItem = true, string description = "Reversing payment.");
        PaymentOrderUpdateRequest GetUpdateRequest(IOrderGroup orderGroup);
    }
}