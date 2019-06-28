using System.Collections.Generic;

namespace PayEx.Net.Api.Models
{
	public class Transacations : ApiUri
    {
        public IEnumerable<TransactionListItem> TransactionList { get; set; }
    }
}
