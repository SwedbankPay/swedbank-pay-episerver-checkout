using System;
using System.Collections.Generic;

namespace PayEx.Net.Api.Models
{
	public class TransactionListItem : ApiUri
    {
        public DateTime Created { get; set; }
		public DateTime Updated { get; set; }
		public string Type { get; set; }
		public string State { get; set; }

		public long Number { get; set; }
		public int Amount { get; set; }
		public int VatAmount { get; set; }
		public string Description { get; set; }
		public string PayeeReference { get; set; }
        public string FailedReason { get; set; }
        public string FailedActivityName { get; set; }
        public string FailedErrorCode { get; set; }
        public string FailedErrorDescription { get; set; }
        public bool IsOperational { get; set; }

        public IEnumerable<Error> Problem { get; set; }
		public IEnumerable<Operation> Operations { get; set; }
	}
}
