namespace PayEx.Net.Api.Models
{
    public class CancelResponseObject
    {
        public string Payment { get; set; }
        public PaymentOrderSubResponse Cancellation { get; set; }
    }
}
