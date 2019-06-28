namespace PayEx.Net.Api.Models
{
	using System;

	public class Payment : ApiUri
	{
		public long Number { get; set; }
		public DateTime Created { get; set; }
		public DateTime Updated { get; set; }
	    public string Instrument { get; set; }
		public string Operation { get; set; }
		public string Intent { get; set; }
		public string State { get; set; }
		public string Currency { get; set; }
		public ApiUri Prices { get; set; }
		public long Amount { get; set; }
		public long RemainingCaptureAmount { get; set; }
		public long RemainingCancellationAmount { get; set; }
		public string Description { get; set; }
		public string UserAgent { get; set; }
		public string Language { get; set; }
		public Transacations Transactions { get; set; }
		public Authorizations Authorizations { get; set; }
		public Urls Urls { get; set; }
		public PayeeInfo PayeeInfo { get; set; }
		public ApiUri MetaData { get; set; }
    }
}