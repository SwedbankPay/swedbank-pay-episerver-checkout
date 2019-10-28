using System;
using System.Threading.Tasks;
using SwedbankPay.Sdk;
using SwedbankPay.Sdk.PaymentOrders;
using SwedbankPay.Sdk.Transactions;

namespace SwedbankPay.Checkout.Episerver.OrderManagement
{
    public class SwedbankPayOrderService : ISwedbankPayOrderService
    {
        private readonly SwedbankPayClient _swedbankPayClient;

        public SwedbankPayOrderService(SwedbankPayClient swedbankPayClient)
        {
            _swedbankPayClient = swedbankPayClient;
        }

        public async Task<PaymentOrderResponseContainer> GetPaymentOrder(string id, PaymentOrderExpand paymentOrderExpand = PaymentOrderExpand.None)
        {
            var paymentOrderResponseObject = await _swedbankPayClient.PaymentOrders.GetPaymentOrder(id, paymentOrderExpand);
            return paymentOrderResponseObject;
        }

        public async Task<TransactionResponse> Capture(TransactionRequestContainer request, string orderId)
        {
            request.Transaction.PayeeReference = DateTime.Now.Ticks.ToString();
            var transactionResponse = await _swedbankPayClient.PaymentOrders.Capture(orderId, request);
            return transactionResponse;
        }

        public async Task<TransactionResponse> Reversal(TransactionRequestContainer request, string orderId)
        {
            request.Transaction.PayeeReference = DateTime.Now.Ticks.ToString();
            var reversalResponseObject = await _swedbankPayClient.PaymentOrders.Reversal(orderId, request);
            return reversalResponseObject;
        }

        public async Task<TransactionResponse> CancelOrder(string orderId)
        {
            var transactionRequestContainer = new TransactionRequestContainer(new TransactionRequest
            {
                PayeeReference = DateTime.Now.Ticks.ToString(),
                Description = "Cancelling parts of the total amount"
            });

            var transactionResponse = await _swedbankPayClient.PaymentOrders.CancelPaymentOrder(orderId, transactionRequestContainer);

            return transactionResponse;
        }
    }
}
