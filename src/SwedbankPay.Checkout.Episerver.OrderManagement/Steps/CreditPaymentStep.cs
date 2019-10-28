using System;
using System.Linq;
using EPiServer.Commerce.Order;
using EPiServer.Logging;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders;
using Mediachase.MetaDataPlus;
using SwedbankPay.Checkout.Episerver.Common;
using SwedbankPay.Checkout.Episerver.Common.Helpers;
using SwedbankPay.Sdk.Transactions;

namespace SwedbankPay.Checkout.Episerver.OrderManagement.Steps
{
    public class CreditPaymentStep : PaymentStep
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(CreditPaymentStep));
        private readonly IRequestFactory _requestFactory;
        private readonly IMarket _market; 

        public CreditPaymentStep(IPayment payment, IMarket market, SwedbankPayOrderServiceFactory swedbankPayOrderServiceFactory, IRequestFactory requestFactory) 
            : base(payment, market, swedbankPayOrderServiceFactory)
        {
            _requestFactory = requestFactory;
            _market = market;
        }

        public override bool Process(IPayment payment, IOrderForm orderForm, IOrderGroup orderGroup, IShipment shipment, ref string message)
        {
            if (payment.TransactionType == TransactionType.Credit.ToString())
            {
                try
                {
                    var amount = AmountHelper.GetAmount((decimal) payment.Amount);
                    var orderId = orderGroup.Properties[Constants.SwedbankPayOrderIdField]?.ToString();
                    if (!string.IsNullOrEmpty(orderId))
                    {
                        var purchaseOrder = orderGroup as IPurchaseOrder;
                        if (purchaseOrder != null)
                        {
                            var returnForm = purchaseOrder.ReturnForms.FirstOrDefault(x => ((OrderForm)x).Status == ReturnFormStatus.Complete.ToString() && ((OrderForm)x).ObjectState == MetaObjectState.Modified);
                            
                            if (returnForm != null)
                            {
                                var transactionDescription = string.IsNullOrWhiteSpace(returnForm.ReturnComment)
                                        ? "credit"
                                        : returnForm.ReturnComment;
                                var captureTransactionRequest =
                                    _requestFactory.GetTransactionRequest(payment, _market, shipment, description: transactionDescription);
                                
                                var captureRequestObject = new TransactionRequestContainer(captureTransactionRequest);
                                
                                var reversalResponseObject = AsyncHelper.RunSync(() => SwedbankPayOrderService.Reversal(captureRequestObject, orderId));
                                if (reversalResponseObject == null)
                                {
                                    payment.Status = PaymentStatus.Failed.ToString();
                                    message = "Reversal is not a valid operation";
                                    AddNoteAndSaveChanges(orderGroup, payment.TransactionType, $"Error occurred {message}");
                                    Logger.Error($"Reversal is not a valid operation for {orderId}");
                                    return false;
                                }
                            }
                        }
                        payment.Status = PaymentStatus.Processed.ToString();

                        AddNoteAndSaveChanges(orderGroup, payment.TransactionType, $"Refunded {payment.Amount}");

                        return true;
                    }
                }
                catch (Exception ex)
                {
                    var exceptionMessage = GetExceptionMessage(ex);

                    payment.Status = PaymentStatus.Failed.ToString();
                    message = exceptionMessage;
                    AddNoteAndSaveChanges(orderGroup, payment.TransactionType, $"Error occurred {exceptionMessage}");
                    Logger.Error(exceptionMessage, ex);
                    return false;
                }
            }
            else if (Successor != null)
            {
                return Successor.Process(payment, orderForm, orderGroup, shipment, ref message);
            }
            return false;
        }
    }
}
