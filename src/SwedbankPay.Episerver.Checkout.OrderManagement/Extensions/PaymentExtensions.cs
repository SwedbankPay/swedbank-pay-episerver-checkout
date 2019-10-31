using EPiServer.Commerce.Order;
using SwedbankPay.Episerver.Checkout.Common;

namespace SwedbankPay.Episerver.Checkout.OrderManagement.Extensions
{
    public static class PaymentExtensions
    {
        public static bool IsSwedbankPayPayment(this IPayment payment)
        {
            return payment?.PaymentMethodName?.StartsWith(Constants.SwedbankPaySystemKeyword) ?? false;
        }

    }
}
