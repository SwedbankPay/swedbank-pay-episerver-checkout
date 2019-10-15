using EPiServer.Commerce.Order;
using SwedbankPay.Checkout.Episerver.Common;

namespace SwedbankPay.Checkout.Episerver.OrderManagement.Extensions
{
    public static class PaymentExtensions
    {
        public static bool IsSwedbankPayPayment(this IPayment payment)
        {
            return payment?.PaymentMethodName?.StartsWith(Constants.SwedbankPaySystemKeyword) ?? false;
        }

    }
}
