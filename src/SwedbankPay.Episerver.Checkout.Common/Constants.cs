namespace SwedbankPay.Episerver.Checkout.Common
{
    public static class Constants
    {
        public const string SwedbankPaySystemKeyword = "SwedbankPay";

        public const string SwedbankPayCheckoutSystemKeyword = SwedbankPaySystemKeyword + "Checkout";

        // Payment method property fields
        public const string SwedbankPaySerializedMarketOptions = "SwedbankPaySerializedMarketOptions";

        // Purchase order meta fields
        public const string SwedbankPayOrderIdField = "SwedbankPayOrderId";
        public const string SwedbankPayPaymentIdField = "SwedbankPayPaymentId";
        public const string SwedbankPayPayeeReference = "SwedbankPayPayeeReference";
    }
}