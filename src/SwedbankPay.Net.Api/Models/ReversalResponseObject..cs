namespace PayEx.Net.Api.Models
{
    public class ReversalResponseObject
    {
        public string Payment { get; set; }
        public PaymentOrderSubResponse Reversals { get; set; }
    }
}
