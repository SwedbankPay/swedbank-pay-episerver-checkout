namespace PayEx.Net.Api.Models
{
	using System;

	public class AuthorizationListItem : ApiUri
    {
        public bool Direct { get; set; }
		public string CardBrand { get; set; }
		public string CardType { get; set; }
		public string PaymentToken { get; set; }
		public string ExpiryDate { get; set; }
		public string PanToken { get; set; }
		public string PanEnrolled { get; set; }
		public string AcquirerTransactionType { get; set; }
		public string AcquirerStan { get; set; }
		public string AcquirerTerminalId { get; set; }
		public DateTime AcquirerTransactionTime { get; set; }
		public TransactionListItem Transaction { get; set; }

	}
}
