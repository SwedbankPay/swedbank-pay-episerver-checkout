namespace PayEx.Net.Api.Models
{
    public class PayeeInfo : ApiUri
	{
		/// <summary>
        /// The ID of the payee, usually the merchant ID.
        /// </summary>
        public string PayeeId { get; set; }

        /// <summary>
        /// A unique reference from the merchant system. It is set per operation to ensure an exactly-once delivery of a transactional operation. See payeeReference for details.
        /// </summary>
        public string PayeeReference { get; set; }

        /// <summary>
        /// The name of the payee, usually the name of the merchant.
        /// </summary>
        public string PayeeName { get; set; }

        /// <summary>
        /// A product category or number sent in from the payee/merchant. This is not validated by PayEx, but will be passed through the payment process and may be used in the settlement process.
        /// </summary>
        public string ProductCategory { get; set; }

        /// <summary>
        /// The order reference should reflect the order reference found in the merchant's systems.
        /// </summary>
        public string OrderReference { get; set; }
    }
}
