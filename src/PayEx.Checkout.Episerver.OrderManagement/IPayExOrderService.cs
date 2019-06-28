namespace PayEx.Checkout.Episerver.OrderManagement
{
    using PayEx.Net.Api.Models;

    public interface IPayExOrderService
    {
	    PaymentOrderResponseObject GetOrder(string id, PaymentOrderExpand paymentOrderExpand = PaymentOrderExpand.None);
		CaptureResponseObject Capture(PaymentOrderTransactionObject captureRequestObject, string orderId);
        ReversalResponseObject Reversal(PaymentOrderTransactionObject captureRequestObject, string orderId);
        CancelResponseObject CancelOrder(string orderId);
        PaymentResponse GetPayment(string orderId, PaymentExpand paymentExpand = PaymentExpand.None);
    }
}

