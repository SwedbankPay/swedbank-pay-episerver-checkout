namespace PayEx.Checkout.Episerver.OrderManagement.Extensions
{
    using EPiServer.Commerce.Order;
    using PayEx.Checkout.Episerver.Common;

    public static class PaymentExtensions
    {
        public static bool IsPayExPayment(this IPayment payment)
        {
            return payment?.PaymentMethodName?.StartsWith(Constants.PayExSystemKeyword) ?? false;
        }

    }
}
