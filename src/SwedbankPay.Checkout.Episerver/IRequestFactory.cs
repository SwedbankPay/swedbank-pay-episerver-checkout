using EPiServer.Commerce.Order;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders.Dto;
using SwedbankPay.Sdk.Models.Request;

namespace SwedbankPay.Checkout.Episerver
{
    public interface IRequestFactory
    {
        PaymentOrderRequestContainer CreatePaymentOrderRequestContainer(IOrderGroup orderGroup, IMarket market, PaymentMethodDto paymentMethodDto, string consumerProfileRef = null);
        ConsumersRequest CreateConsumerResourceRequest(IMarket market, string email, string mobilePhone, string ssn);
    }
}
