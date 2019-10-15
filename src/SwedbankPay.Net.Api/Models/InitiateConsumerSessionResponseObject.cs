namespace PayEx.Net.Api.Models
{
    using System.Collections.Generic;

    public class InitiateConsumerSessionResponseObject
    {
        /// <summary>
        /// A session token used to initiate Checkout UI.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// The list of operation objects to choose from for.
        /// </summary>
        public List<Operation> Operations { get; set; }
    }
}
