using EPiServer.Commerce.Order;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders.Dto;
using SwedbankPay.Client;
using SwedbankPay.Client.Models.Request;

namespace PayEx.Checkout.Episerver
{
    public interface IRequestFactory
    {
        PaymentOrderRequestContainer Create(IOrderGroup orderGroup, IMarket market, PaymentMethodDto paymentMethodDto, string consumerProfileRef = null);
    }
}
