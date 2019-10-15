namespace PayEx.Net.Api.Models
{
    public class CampaignInvoice
    {
        /// <summary>
        /// The name of the invoice campaign.
        /// </summary>
        public string CampaignCode { get; set; }

        /// <summary>
        /// The fee amount in the lowest monetary to apply if the consumer chooses to pay with campaign invoice.
        /// </summary>
        public int FeeAmount { get; set; }
    }
}
