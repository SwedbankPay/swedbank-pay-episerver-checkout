using EPiServer.Commerce.Order;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders.Dto;
using SwedbankPay.Client.Models.Request;

namespace SwedbankPay.Checkout.Episerver
{
    public interface IRequestFactory
    {
        PaymentOrderRequestContainer Create(IOrderGroup orderGroup, IMarket market, PaymentMethodDto paymentMethodDto, string consumerProfileRef = null);
        ConsumerResourceRequest Create(IMarket market, string email, string mobilePhone, string ssn);
    }
}
