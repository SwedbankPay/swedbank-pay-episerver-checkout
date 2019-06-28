namespace PayEx.Net.Api.Models
{
    public class TransactionResponse : ApiUri
    {
        public string Type { get; set; }

        public string State { get; set; }
        public int Amount { get; set; }
        public int VatAmount  { get; set; }
        public string Description { get; set; }
        public string PayeeReference { get; set; }
    }
}
