namespace SwedbankPay.Episerver.Checkout.Common
{
    public static class Constants
    {
        public static readonly string SwedbankPaySystemKeyword = "SwedbankPay";

        public static readonly string SwedbankPayCheckoutSystemKeyword = SwedbankPaySystemKeyword + "Checkout";

        // Payment method property fields
        public static readonly string SwedbankPaySerializedMarketOptions = "SwedbankPaySerializedMarketOptions";
        
        // Purchase order meta fields
        public static readonly string SwedbankPayOrderIdField = "SwedbankPayOrderId";
        public static readonly string SwedbankPayPaymentIdField = "SwedbankPayPaymentId";
        public static readonly string SwedbankPayPayeeReference = "SwedbankPayPayeeReference";
        public static readonly string Culture = "Culture";
        public static readonly string ConsumerProfileRef = "ConsumerProfileRef";
        public static readonly string ConsumerUiScriptSource = "ConsumerUiScriptSource";
    }
}