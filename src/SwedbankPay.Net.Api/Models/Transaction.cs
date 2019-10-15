namespace PayEx.Net.Api.Models
{
    using System.Collections.Generic;

    public class Transaction
    {
        public string Description { get; set; }
        public int Amount { get; set; }
        public int VatAmount { get; set; }
        public string PayeeReference { get; set; }
        public List<ItemDescription> ItemDescriptions { get; set; }
        public List<VatSummary> VatSummary { get; set; }
    }
}


