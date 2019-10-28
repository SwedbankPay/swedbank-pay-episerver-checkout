using System.Threading.Tasks;
using SwedbankPay.Sdk.PaymentOrders;
using SwedbankPay.Sdk.Transactions;

namespace SwedbankPay.Checkout.Episerver.OrderManagement
{
    public interface ISwedbankPayOrderService
    {
	    Task<PaymentOrderResponseContainer> GetPaymentOrder(string id, PaymentOrderExpand paymentOrderExpand = PaymentOrderExpand.None);
        Task<TransactionResponse> Capture(TransactionRequestContainer request, string orderId);
        Task<TransactionResponse> Reversal(TransactionRequestContainer request, string orderId);
        Task<TransactionResponse> CancelOrder(string orderId);
    }
}

