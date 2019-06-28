using System.Collections.Generic;

namespace PayEx.Net.Api.Models
{
	public class PaymentResponse
    {
        public Payment Payment { get; set; }
		public IList<Operation> Operations { get; set; }
    }
}
