namespace PayEx.Checkout.Episerver.OrderManagement
{
    using SwedbankPay.Client.Models;
    using SwedbankPay.Client.Models.Request.Transaction;
    using SwedbankPay.Client.Models.Response;
    using SwedbankPay.Client.Models.Vipps.TransactionAPI.Response;

    using System.Threading.Tasks;


    public interface ISwedbankPayOrderService
    {
	    Task<PaymentOrderResponseContainer> GetPaymentOrder(string id, PaymentOrderExpand paymentOrderExpand = PaymentOrderExpand.None);
        Task<TransactionResponse> Capture(TransactionRequestContainer request, string orderId);
        Task<TransactionResponse> Reversal(TransactionRequestContainer request, string orderId);
        Task<TransactionResponse> CancelOrder(string orderId);
    }
}

