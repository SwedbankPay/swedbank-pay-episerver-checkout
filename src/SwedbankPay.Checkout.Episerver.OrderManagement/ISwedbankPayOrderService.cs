using System.Threading.Tasks;
using SwedbankPay.Sdk.Models;
using SwedbankPay.Sdk.Models.Request.Transaction;
using SwedbankPay.Sdk.Models.Response.PaymentOrder;
using SwedbankPay.Sdk.Models.Vipps.TransactionAPI.Response;

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

