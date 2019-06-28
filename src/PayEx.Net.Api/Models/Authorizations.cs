namespace PayEx.Net.Api.Models
{
	using System.Collections.Generic;

	public class Authorizations : ApiUri
    {
        public IEnumerable<AuthorizationListItem> AuthorizationList { get; set; }
    }
}
