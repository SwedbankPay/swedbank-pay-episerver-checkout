namespace PayEx.Net.Api.Controllers
{
    using PayEx.Net.Api.Models;
    using RestSharp;
    using System.Net;

    public class ConsumersController : ControllerAbstract
    {
        public ConsumersController(IRestClient client) : base(client)
        {
        }

        public InitiateConsumerSessionResponseObject InitiateConsumerSession(InitiateConsumerSessionRequestObject initiateConsumerSessionRequest)
        {
            var request = new RestRequest("/psp/consumers", Method.POST);
            return Execute<InitiateConsumerSessionResponseObject>(request, initiateConsumerSessionRequest)
                .Status(HttpStatusCode.OK)
                .Response.Data;
        }


        public ShippingDetails GetShippingDetails(string uri)
        {
            uri = ConvertUriToRelative(uri);
            if (string.IsNullOrWhiteSpace(uri))
            {
                return null;
            }

            var request = new RestRequest(uri, Method.GET);
            var shippingDetails = Execute<ShippingDetails>(request)
                .Status(HttpStatusCode.OK)
                .Response.Data;

            return shippingDetails;
        }
    }
}
