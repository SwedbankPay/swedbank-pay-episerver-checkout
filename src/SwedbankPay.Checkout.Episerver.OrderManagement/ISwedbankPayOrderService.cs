using System.Threading.Tasks;
using SwedbankPay.Client.Models;
using SwedbankPay.Client.Models.Request.Transaction;
using SwedbankPay.Client.Models.Response;
using SwedbankPay.Client.Models.Vipps.TransactionAPI.Response;

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

