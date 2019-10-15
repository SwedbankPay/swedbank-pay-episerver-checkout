namespace PayEx.Net.Api.Controllers
{
	using PayEx.Net.Api.Models;
	using RestSharp;
	using System.Net;

	public class PaymentOrdersController : ControllerAbstract
	{
		public PaymentOrdersController(IRestClient client) : base(client)
		{
		}

		public PaymentOrderResponseObject CreatePaymentOrder(PaymentOrderRequestObject paymentOrderRequest)
		{
			paymentOrderRequest.Paymentorder.Operation = "Purchase";
			var request = new RestRequest("/psp/paymentorders", Method.POST);
			request.AddQueryParameter("$expand", "urls,payeeInfo,metadata");

			return Execute<PaymentOrderResponseObject>(request, paymentOrderRequest)
				.Status(HttpStatusCode.Created)
				.Response.Data;
		}

		public PaymentOrderResponseObject GetPaymentOrder(string id, PaymentOrderExpand paymentOrderExpand = PaymentOrderExpand.None)
		{
			var request = new RestRequest(id, Method.GET);
			
			if (paymentOrderExpand != PaymentOrderExpand.None)
			{
				var expandQueryStringParameter = Utils.Utils.GetExpandQueryString(paymentOrderExpand);
				request.AddQueryParameter("$expand", expandQueryStringParameter);
			}
			return Execute<PaymentOrderResponseObject>(request)
				.Status(HttpStatusCode.OK)
				.Response.Data;
		}

		public PaymentOrderResponseObject UpdatePaymentOrder(PaymentOrderRequestObject paymentOrderRequest, string id)
		{
			paymentOrderRequest.Paymentorder.Operation = "UpdateOrder";
			var request = new RestRequest(id, Method.PATCH);
			return Execute<PaymentOrderResponseObject>(request, paymentOrderRequest)
				.Status(HttpStatusCode.OK)
				.Response.Data;
		}


		public CaptureResponseObject Capture(string uri, PaymentOrderTransactionObject requestObject)
		{
			uri = ConvertUriToRelative(uri);
			if (string.IsNullOrWhiteSpace(uri))
			{
				return null;
			}

			var request = new RestRequest(uri, Method.POST);
			return Execute<CaptureResponseObject>(request, requestObject)
				.Status(HttpStatusCode.OK)
				.Response.Data;
		}

		public CancelResponseObject Cancel(string uri, PaymentOrderTransactionObject requestObject)
		{
			uri = ConvertUriToRelative(uri);
			if (string.IsNullOrWhiteSpace(uri))
			{
				return null;
			}

			var request = new RestRequest(uri, Method.POST);
			return Execute<CancelResponseObject>(request, requestObject)
				.Status(HttpStatusCode.OK)
				.Response.Data;
		}

		public ReversalResponseObject Reversal(string uri, PaymentOrderTransactionObject requestObject)
		{
			uri = ConvertUriToRelative(uri);
			if (string.IsNullOrWhiteSpace(uri))
			{
				return null;
			}

			var request = new RestRequest(uri, Method.POST);
			return Execute<ReversalResponseObject>(request, requestObject)
				.Status(HttpStatusCode.OK)
				.Response.Data;
		}


		public PaymentResponse GetPayment(string uri, PaymentExpand paymentExpand = PaymentExpand.None)
		{
			var request = new RestRequest(uri, Method.GET);

			if (paymentExpand != PaymentExpand.None)
			{
				var expandQueryStringParameter = Utils.Utils.GetExpandQueryString(paymentExpand);
				request.AddQueryParameter("$expand", expandQueryStringParameter);
			}
			return Execute<PaymentResponse>(request)
				.Status(HttpStatusCode.OK)
				.Response.Data;
		}
	}
}
