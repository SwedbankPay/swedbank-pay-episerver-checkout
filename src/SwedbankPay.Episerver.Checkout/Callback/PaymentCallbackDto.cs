namespace SwedbankPay.Episerver.Checkout.Callback
{
    public class PaymentCallbackDto
    {
        public CallbackString PaymentOrder { get; set; }
        public CallbackNumber Payment { get; set; }
        public CallbackNumber Transaction { get; set; }
    }
}
