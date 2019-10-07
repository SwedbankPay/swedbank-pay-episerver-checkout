namespace PayEx.Checkout.Episerver.OrderManagement.Steps
{
    using EPiServer.Commerce.Order;
    using EPiServer.Logging;

    using Mediachase.Commerce;
    using Mediachase.Commerce.Orders;
    using Mediachase.MetaDataPlus;

    using PayEx.Checkout.Episerver.Common;
    using PayEx.Checkout.Episerver.Common.Helpers;

    using SwedbankPay.Client.Models.Request.Transaction;

    using System;
    using System.Linq;
    using System.Net;

    public class CreditPaymentStep : PaymentStep
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(CreditPaymentStep));

        public CreditPaymentStep(IPayment payment, MarketId marketId, SwedbankPayOrderServiceFactory swedbankPayOrderServiceFactory) 
            : base(payment, marketId, swedbankPayOrderServiceFactory)
        {
        }

        public override bool Process(IPayment payment, IOrderForm orderForm, IOrderGroup orderGroup, IShipment shipment, ref string message)
        {
            if (payment.TransactionType == TransactionType.Credit.ToString())
            {
                try
                {
                    var amount = AmountHelper.GetAmount((decimal) payment.Amount);
                    var orderId = orderGroup.Properties[Constants.PayExOrderIdField]?.ToString();
                    if (!string.IsNullOrEmpty(orderId))
                    {
                        var purchaseOrder = orderGroup as IPurchaseOrder;
                        if (purchaseOrder != null)
                        {
                            var returnForm = purchaseOrder.ReturnForms.FirstOrDefault(x => ((OrderForm)x).Status == ReturnFormStatus.Complete.ToString() && ((OrderForm)x).ObjectState == MetaObjectState.Modified);
                            
                            if (returnForm != null)
                            {
                                var transactionRequest = new TransactionRequest
                                {
                                    Amount = amount,
                                    Description = string.IsNullOrWhiteSpace(returnForm.ReturnComment) ? "credit" : returnForm.ReturnComment
                                };
                                var captureRequestObject = new TransactionRequestContainer(transactionRequest);
                                
                                var reversalResponseObject = SwedbankPayOrderService.Reversal(captureRequestObject, orderId);
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
