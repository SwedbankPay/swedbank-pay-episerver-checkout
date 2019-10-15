namespace PayEx.Net.Api.Models
{
    using System.Collections.Generic;

    public class PaymentOrderResponseObject
    {
        /// <summary>
        /// The payment order object.
        /// </summary>
        public PaymentOrderResponse PaymentOrder { get; set; }

        /// <summary>
        /// The list of operation objects to choose from for.
        /// </summary>
        public List<Operation> Operations { get; set; }
    }
}