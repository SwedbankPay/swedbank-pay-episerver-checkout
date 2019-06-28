namespace PayEx.Checkout.Episerver.OrderManagement
{
	using PayEx.Checkout.Episerver.Common;
	using PayEx.Net.Api;
	using PayEx.Net.Api.Models;
	using System;
	using System.Linq;

	public class PayExOrderService : IPayExOrderService
    {
        private readonly PayExApi _payExApi;

        public PayExOrderService(PayExApi payExApi, ConnectionConfiguration connectionConfiguration)
        {
            _payExApi = payExApi;
        }

        public PaymentOrderResponseObject GetOrder(string id, PaymentOrderExpand paymentOrderExpand = PaymentOrderExpand.None)
        {
            var paymentOrderResponseObject = _payExApi.PaymentOrders.GetPaymentOrder(id, paymentOrderExpand);
            return paymentOrderResponseObject;
        }

        public virtual CaptureResponseObject Capture(PaymentOrderTransactionObject captureRequestObject, string orderId)
        {
            var paymentOrderResponseObject = GetOrder(orderId);
            captureRequestObject.Transaction.PayeeReference = DateTime.Now.Ticks.ToString();

            var href = paymentOrderResponseObject.Operations.FirstOrDefault(x => x.Rel == OrderOperations.CapturePaymentOrder)?.Href;

            CaptureResponseObject captureResponseObject = _payExApi.PaymentOrders.Capture(href, captureRequestObject);
            return captureResponseObject;
        }

        public ReversalResponseObject Reversal(PaymentOrderTransactionObject captureRequestObject, string orderId)
        {
            var paymentOrderResponseObject = GetOrder(orderId);
            captureRequestObject.Transaction.PayeeReference = DateTime.Now.Ticks.ToString();

            var href = paymentOrderResponseObject.Operations.FirstOrDefault(x => x.Rel == OrderOperations.ReversePaymentOrder)?.Href;

            var reversalResponseObject = _payExApi.PaymentOrders.Reversal(href, captureRequestObject);
            return reversalResponseObject;
        }

        public CancelResponseObject CancelOrder(string orderId)
        {
            var paymentOrderResponseObject = GetOrder(orderId);

            if (paymentOrderResponseObject != null)
            {
                var cancelUri = paymentOrderResponseObject.Operations.FirstOrDefault(x => x.Rel == OrderOperations.CancelPaymentOrder)?.Href;
                var cancelResponseObject = _payExApi.PaymentOrders.Cancel(cancelUri, new PaymentOrderTransactionObject
				{
                    Transaction = new Transaction
                    {
                        PayeeReference = DateTime.Now.Ticks.ToString(),
                        Description = "Cancelling parts of the total amount"
                    }
                });
                return cancelResponseObject;
            }

            return null;
        }

        public PaymentResponse GetPayment(string orderId, PaymentExpand paymentExpand = PaymentExpand.None)
        {
	        var paymentResponse = _payExApi.PaymentOrders.GetPayment(orderId, paymentExpand);
	        return paymentResponse;
        }
    }
}
