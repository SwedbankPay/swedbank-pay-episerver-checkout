using System;

namespace PayEx.Net.Api.Models
{
	[Flags]
    public enum PaymentExpand
    {
		None = 0,
        Prices  = 1,
		Transactions = 2,
		Authorizations = 4,
		Urls = 8,
		PayeeInfo = 16,
		MetaData = 32,
		All = 63
    }
}
