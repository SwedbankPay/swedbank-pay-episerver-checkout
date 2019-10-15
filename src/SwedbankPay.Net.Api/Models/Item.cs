namespace PayEx.Net.Api.Models
{
    public class Item
    {
        public CreditCard CreditCard { get; set; }
        public Invoice Invoice { get; set; }
        
        public CampaignInvoice CampaignInvoice { get; set; }
        public Swish Swish { get; set; }
    }
}
