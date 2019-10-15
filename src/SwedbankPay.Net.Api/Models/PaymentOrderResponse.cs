namespace PayEx.Net.Api.Models
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class PaymentOrderResponse
    {
        /// <summary>
        /// The relative URI to the payment order.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// 	The ISO-8601 date of when the payment order was created.
        /// </summary>
        [JsonProperty("created")]
        public DateTime Created { get; set; }

        /// <summary>
        /// The ISO-8601 date of when the payment order was updated.	
        /// </summary>
        [JsonProperty("updated")]
        public DateTime Updated { get; set; }

        /// <summary>
        /// Ready, Pending, Failed or Aborted. Indicates the state of the payment order. This field is only for status display purposes.
        /// </summary>
        [JsonProperty("state")]
        public string State { get; set; }

        /// <summary>
        /// The currency of the payment order.	
        /// </summary>
        [JsonProperty("currency")]
        public string Currency { get; set; }

        /// <summary>
        /// The amount including VAT in the lowest monetary unit of the currency. E.g. 10000 equals 100.00 NOK and 5000 equals 50.00 NOK.	
        /// </summary>
        [JsonProperty("amount")]
        public int Amount { get; set; }

        /// <summary>
        /// The amount of VAT in the lowest monetary unit of the currency. E.g. 10000 equals 100.00 NOK and 5000 equals 50.00 NOK.	
        /// </summary>
        [JsonProperty("vatAmount")]
        public int VatAmount { get; set; }


        public int RemainingCaptureAmount { get; set; }
        public int RemainingCancellationAmount { get; set; }
        public int RemainingReversalAmount { get; set; }

        /// <summary>
        /// A textual description of maximum 40 characters of the purchase.	
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }
        public string InitiatingSystemUserAgent { get; set; }

        /// <summary>
        /// The user agent string of the consumer's browser.
        /// </summary>
        [JsonProperty("userAgent")]
        public string UserAgent { get; set; }

        /// <summary>
        /// nb-NO, sv-SE or en-US
        /// </summary>
        [JsonProperty("language")]
        public string Language { get; set; }

        /// <summary>
        /// The URI to the urls resource where all URIs related to the payment order can be retrieved.
        /// </summary>
        [JsonProperty("urls")]
        public Urls Urls { get; set; }

        /// <summary>
        /// The URI to the payeeinfo resource where the information about the payee of the payment order can be retrieved.
        /// </summary>
        [JsonProperty("payeeInfo")]
        public PayeeInfo PayeeInfo { get; set; }
        public ApiUri Settings { get; set; }

        /// <summary>
        /// The URI to the payers resource where information about the payee of the payment order can be retrieved.
        /// </summary>
        [JsonProperty("payers")]
        public Payer Payers { get; set; }
        public Dictionary<string, object> Metadata { get; set; }

        /// <summary>
        /// The URI to the payments resource where information about all underlying payments can be retrieved.
        /// </summary>
        [JsonProperty("payments")]
        public ApiUri Payments { get; set; }

        /// <summary>
        /// The URI to the currentPayment resource where information about the current - and sole active  - payment can be retrieved.
        /// </summary>
        [JsonProperty("currentPayment")]
        public CurrentPaymentResponseObject CurrentPayment { get; set; }
    }
}
