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
        ConsumersRequest GetConsumerResourceRequest(CultureInfo language, IEnumerable<RegionInfo> shippingAddressRestrictedToCountryCodes, EmailAddress email = null, Msisdn msisdn = null, NationalIdentifier nationalIdentifier = null);
        AbortRequest GetAbortRequest();
        CancelRequest GetCancelRequest();
        CaptureRequest GetCaptureRequest(IPayment payment, IMarket market, IShipment shipment, bool addShipmentInOrderItem = true);
        ReversalRequest GetReversalRequest(IPayment payment, IMarket market, IShipment shipment, bool addShipmentInOrderItem = true);
        UpdateRequest GetUpdateRequest(IOrderGroup orderGroup);
    }
}