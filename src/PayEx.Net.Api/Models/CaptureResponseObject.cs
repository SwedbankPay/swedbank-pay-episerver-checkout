namespace PayEx.Net.Api.Models
{
    public class CaptureResponseObject
    {
        public string Payment { get; set; }
        public PaymentOrderSubResponse Capture {get; set; }
    }
}
