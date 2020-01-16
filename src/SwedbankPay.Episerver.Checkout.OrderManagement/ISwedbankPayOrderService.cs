using System;
using System.Threading.Tasks;
using SwedbankPay.Sdk.PaymentOrders;

namespace SwedbankPay.Episerver.Checkout.OrderManagement
{
    public interface ISwedbankPayOrderService
    {
	    Task<PaymentOrder> GetPaymentOrder(Uri id, PaymentOrderExpand paymentOrderExpand = PaymentOrderExpand.None);
        Task<PaymentOrderResponse> Capture(CaptureRequest request, Uri orderId);
        Task<PaymentOrderResponse> Reversal(ReversalRequest request, Uri orderId);
        Task<PaymentOrderResponse> CancelOrder(Uri orderId);
    }
}

