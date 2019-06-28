namespace PayEx.Net.Api.Models
{
	using System.Collections.Generic;

	public class CurrentPaymentResponseObject
    {
        public string Id { get; set; }
        public string MenuElementName { get; set; }
		public Payment Payment { get; set; }
		public IEnumerable<Operation> Operations { get; set; }
	}
}
